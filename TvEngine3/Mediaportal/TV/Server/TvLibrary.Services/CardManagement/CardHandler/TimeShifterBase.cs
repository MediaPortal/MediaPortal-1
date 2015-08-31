using System;
using System.IO;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using ISubChannel = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ISubChannel;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public abstract class TimeShifterBase
  {
    protected ITvCardHandler _cardHandler;
    private TimeShiftingEpgGrabber _timeShiftingEpgGrabber = null;
    private int _waitForVideoOrAudio = 15000;   // unit = ms
    protected readonly ManualResetEvent _eventAudio = new ManualResetEvent(false); // gets signaled when audio PID is seen
    protected readonly ManualResetEvent _eventVideo = new ManualResetEvent(false); // gets signaled when video PID is seen
    protected bool _cancelled;
    protected readonly ManualResetEvent _eventTimeshift = new ManualResetEvent(true);
    protected ISubChannel _subchannel; // the active sub channel to record

    private readonly string _folderSettingName;
    protected string _folder = string.Empty;

    protected TimeShifterBase(ITvCardHandler cardHandler, string folderSettingName)
    {
      _cardHandler = cardHandler;
      _eventAudio.Reset();
      _eventVideo.Reset();

      _folderSettingName = folderSettingName;
      ReloadConfiguration();

      if (_cardHandler != null)
      {
        if (_cardHandler.Tuner != null)
        {
          _cardHandler.Tuner.OnAfterCancelTuneEvent += new OnAfterCancelTuneDelegate(Tuner_OnAfterCancelTuneEvent);
        }

        _timeShiftingEpgGrabber = new TimeShiftingEpgGrabber(_cardHandler.Card);
      }
    }

    public virtual void ReloadConfiguration()
    {
      _waitForVideoOrAudio = SettingsManagement.GetValue("timeLimitReceiveStream", 7500);
      this.LogDebug("  receive stream time limit = {0} ms", _waitForVideoOrAudio);

      _folder = SettingsManagement.GetValue(_folderSettingName, string.Empty);
      string originalFolder = _folder;
      bool useDefault = true;
      if (!string.IsNullOrEmpty(_folder))
      {
        try
        {
          _folder = Path.GetFullPath(_folder);
          useDefault = false;
        }
        catch
        {
        }
      }
      if (useDefault)
      {
        _folder = GetDefaultFolder();
      }
      if (!Directory.Exists(_folder))
      {
        try
        {
          Directory.CreateDirectory(_folder);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "card: failed to create {0} time-shifting or recording folder, folder = {1}", useDefault ? "default" : "configured", _folder);
          if (!useDefault)
          {
            _folder = GetDefaultFolder();
            try
            {
              Directory.CreateDirectory(_folder);
            }
            catch (Exception ex2)
            {
              this.LogError(ex2, "card: failed to create default time-shifting or recording folder, folder = {0}", _folder);
            }
          }
        }
      }
      if (!string.Equals(originalFolder, _folder))
      {
        SettingsManagement.SaveValue(_folderSettingName, _folder);
      }
      this.LogDebug("  folder                    = {0}", _folder);
    }

    protected abstract string GetDefaultFolder();

    protected abstract void AudioVideoEventHandler(PidType pidType);

    private void Tuner_OnAfterCancelTuneEvent(int subchannelId)
    {
      try
      {
        if (_cardHandler.Card.IsEnabled == false)
        {
          return;
        }

        this.LogDebug("TimeShifterBase: tuning interrupted.");
        _cancelled = true;

        ISubChannel subchannel = GetSubChannel(subchannelId);
        this.LogDebug("card {2}: Cancel Timeshifting sub:{1}", subchannel, _cardHandler.Card.Name);
        subchannel.AudioVideoEvent -= AudioVideoEventHandler;
        _eventAudio.Set();
        _eventVideo.Set();
        _eventTimeshift.WaitOne();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        _cancelled = false;
      }
    }

    protected bool IsTuneCancelled()
    {
      return _cancelled;
    }

    protected ISubChannel GetSubChannel(string userName, int idChannel)
    {
      ISubChannel subchannel = null;
      if (_cardHandler.Card.IsEnabled)
      {
           
        //_cardHandler.UserManagement.RefreshUser(ref user, out userExists);
        bool userExists = _cardHandler.UserManagement.DoesUserExist(userName);
        if (userExists)
        {
          subchannel = GetSubChannel(_cardHandler.UserManagement.GetSubChannelIdByChannelId(userName, idChannel));
        }          
      }
      
      return subchannel;
    }

    private bool IsScrambled(ref IUser user)
    {
      bool isScrambled = false;
      //lets check if stream is initially scrambled, if it is and the card has no CA, then we are unable to decrypt stream.
      if (_cardHandler.IsScrambled(user.Name))
      {
        if (!_cardHandler.Card.IsConditionalAccessSupported)
        {
          this.LogDebug("card: WaitForTimeShiftFile - return scrambled, since the device does not support conditional access");
          isScrambled = true;
        }
      }
      return isScrambled;
    }

    /// <summary>
    /// Waits for timeshifting / recording file to be at leat 300kb. 
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="scrambled">Indicates if the channel is scambled</param>
    /// <returns>true when timeshift files is at least of 300kb, else timeshift file is less then 300kb</returns>
    protected bool WaitForFile(ref IUser user, out bool scrambled)
    {
      scrambled = false;

      if (!_cardHandler.Card.IsEnabled)
      {
        return false;
      }
      
      scrambled = IsScrambled(ref user);
      if (scrambled)
      {
        return false;
      }

      DateTime timeStart = DateTime.Now;

      if (_cardHandler.Card.CurrentTuningDetail == null)
        return false;

      IChannel channel = _subchannel.CurrentChannel;
      bool isRadio = (channel.MediaType == MediaType.Radio);

      if (isRadio)
      {
        this.LogDebug("card: WaitForFile - waiting _eventAudio");
        // wait for audio PID to be seen
        if (_eventAudio.WaitOne(_waitForVideoOrAudio, true))
        {
          if (IsTuneCancelled())
          {
            this.LogDebug("card: WaitForFile - Tune Cancelled");
            return false;
          }
          // start of the video & audio is seen
          TimeSpan ts = DateTime.Now - timeStart;
          this.LogDebug("card: WaitForFile - audio is seen after {0} seconds", ts.TotalSeconds);
          return true;
        }
        else
        {
          TimeSpan ts = DateTime.Now - timeStart;
          this.LogDebug("card: WaitForRecordingFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(user.Name))
          {
            this.LogDebug("card: WaitForFile - audio stream is scrambled");
            scrambled = true;
          }
        }
      }
      else
      {
        this.LogDebug("card: WaitForFile - waiting _eventAudio & _eventVideo");
        // block until video & audio PIDs are seen or the timeout is reached
        if (_eventAudio.WaitOne(_waitForVideoOrAudio, true))
        {
          if (IsTuneCancelled())
          {
            return false;
          }
          if (_eventVideo.WaitOne(_waitForVideoOrAudio, true))
          {
            if (IsTuneCancelled())
            {
              this.LogDebug("card: WaitForFile - Tune Cancelled");
              return false;
            }
            // start of the video & audio is seen
            TimeSpan ts = DateTime.Now - timeStart;
            this.LogDebug("card: WaitForFile - video and audio are seen after {0} seconds", ts.TotalSeconds);
            return true;
          }
          else
          {
            TimeSpan ts = DateTime.Now - timeStart;
            this.LogDebug("card: WaitForFile - video was found, but audio was not found after {0} seconds",
                      ts.TotalSeconds);
            if (_cardHandler.IsScrambled(user.Name))
            {
              this.LogDebug("card: WaitForFile - audio stream is scrambled");
              scrambled = true;
            }
          }
        }
        else
        {
          TimeSpan ts = DateTime.Now - timeStart;
          this.LogDebug("card: WaitForFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(user.Name))
          {
            this.LogDebug("card: WaitForFile - audio and video stream is scrambled");
            scrambled = true;
          }
        }
      }
      return false;
    }

    protected ISubChannel GetSubChannel(int subchannel)
    {
      return _cardHandler.Card.GetSubChannel(subchannel);
    }

    protected void StartTimeShiftingEpgGrabber(IUser user)
    {
      _timeShiftingEpgGrabber.StartGrab(_cardHandler.Card.CurrentTuningDetail);
    }

    protected void StopTimeShiftingEpgGrabber()
    {
      _timeShiftingEpgGrabber.StopGrab();
    }

    protected TvResult GetFailedTvResult(bool isScrambled)
    {
      TvResult result;    
      if (IsTuneCancelled())
      {
        result = TvResult.TuneCancelled;
      }
      else if (isScrambled)
      {
        result = TvResult.ChannelIsScrambled;
      }
      else
      {
        result = TvResult.NoVideoAudioDetected;
      }
      return result;
    }

    protected void AttachAudioVideoEventHandler(ISubChannel subchannel)
    {
      subchannel.AudioVideoEvent += AudioVideoEventHandler;
    }

    protected void DetachAudioVideoEventHandler(ISubChannel subchannel)
    {
      subchannel.AudioVideoEvent -= AudioVideoEventHandler;
    }
  }
}
