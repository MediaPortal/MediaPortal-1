using System;
using System.Threading;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public abstract class TimeShifterBase
  {    
    protected ITvCardHandler _cardHandler;
    protected bool _timeshiftingEpgGrabberEnabled;
    private readonly int _waitForTimeshifting = 15;
    protected readonly ManualResetEvent _eventAudio = new ManualResetEvent(false); // gets signaled when audio PID is seen
    protected readonly ManualResetEvent _eventVideo = new ManualResetEvent(false); // gets signaled when video PID is seen
    protected bool _cancelled;
    protected readonly ManualResetEvent _eventTimeshift = new ManualResetEvent(true);
    protected ITvSubChannel _subchannel; // the active sub channel to record        

    protected TimeShifterBase()
    {
      _eventAudio.Reset();
      _eventVideo.Reset();

      
      _waitForTimeshifting = Int32.Parse(SettingsManagement.GetSetting("timeshiftWaitForTimeshifting", "15").Value);

      if (_cardHandler != null && _cardHandler.Tuner != null)
      {
        _cardHandler.Tuner.OnAfterCancelTuneEvent += Tuner_OnAfterCancelTuneEvent;
      }

      if (_cardHandler != null && _cardHandler.Tuner != null)
      {
        _cardHandler.Tuner.OnAfterCancelTuneEvent += new OnAfterCancelTuneDelegate(Tuner_OnAfterCancelTuneEvent);
      }
    }

    protected abstract void AudioVideoEventHandler(PidType pidType);

    private void Tuner_OnAfterCancelTuneEvent(int subchannelId)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return;
        }

        Log.Debug("TimeShifterBase: tuning interrupted.");
        _cancelled = true;

        ITvSubChannel subchannel = GetSubChannel(subchannelId);
        if (subchannel is BaseSubChannel)
        {
          Log.Write("card {2}: Cancel Timeshifting sub:{1}", subchannel, _cardHandler.Card.Name);
          ((BaseSubChannel)subchannel).AudioVideoEvent -= AudioVideoEventHandler;
          _eventAudio.Set();
          _eventVideo.Set();
          _eventTimeshift.WaitOne();
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
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

    protected ITvSubChannel GetSubChannel(string userName, int idChannel)
    {
      ITvSubChannel subchannel = null;
      if (_cardHandler.DataBaseCard.Enabled)
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
        if (!_cardHandler.IsConditionalAccessSupported)
        {
          Log.Write("card: WaitForTimeShiftFile - return scrambled, since the device does not support conditional access");
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

      if (!_cardHandler.DataBaseCard.Enabled)
      {
        return false;
      }
      
      scrambled = IsScrambled(ref user);
      if (scrambled)
      {
        return false;
      }

      int waitForEvent = _waitForTimeshifting * 1000; // in ms           

      DateTime timeStart = DateTime.Now;

      if (_cardHandler.Card.SubChannels.Length <= 0)
        return false;

      IChannel channel = _subchannel.CurrentChannel;
      bool isRadio = (channel.MediaType == MediaTypeEnum.Radio);

      if (isRadio)
      {
        Log.Write("card: WaitForFile - waiting _eventAudio");
        // wait for audio PID to be seen
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          if (IsTuneCancelled())
          {
            Log.Write("card: WaitForFile - Tune Cancelled");
            return false;
          }
          // start of the video & audio is seen
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForFile - audio is seen after {0} seconds", ts.TotalSeconds);
          return true;
        }
        else
        {
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForRecordingFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(user.Name))
          {
            Log.Write("card: WaitForFile - audio stream is scrambled");
            scrambled = true;
          }
        }
      }
      else
      {
        Log.Write("card: WaitForFile - waiting _eventAudio & _eventVideo");
        // block until video & audio PIDs are seen or the timeout is reached
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          if (IsTuneCancelled())
          {
            return false;
          }
          if (_eventVideo.WaitOne(waitForEvent, true))
          {
            if (IsTuneCancelled())
            {
              Log.Write("card: WaitForFile - Tune Cancelled");
              return false;
            }
            // start of the video & audio is seen
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write("card: WaitForFile - video and audio are seen after {0} seconds", ts.TotalSeconds);
            return true;
          }
          else
          {
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write("card: WaitForFile - video was found, but audio was not found after {0} seconds",
                      ts.TotalSeconds);
            if (_cardHandler.IsScrambled(user.Name))
            {
              Log.Write("card: WaitForFile - audio stream is scrambled");
              scrambled = true;
            }
          }
        }
        else
        {
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(user.Name))
          {
            Log.Write("card: WaitForFile - audio and video stream is scrambled");
            scrambled = true;
          }
        }
      }
      return false;
    }

    protected ITvSubChannel GetSubChannel(int subchannel)
    {
      return _cardHandler.Card.GetSubChannel(subchannel);
    }

    protected void StartTimeShiftingEPGgrabber(IUser user)
    {
      if (_timeshiftingEpgGrabberEnabled)
      {
        Channel channel = ChannelManagement.GetChannel(_cardHandler.UserManagement.GetRecentChannelId(user.Name));
        if (channel.GrabEpg)
        {
          _cardHandler.Card.GrabEpg();
        }
        else
        {
          Log.Info("TimeshiftingEPG: channel {0} is not configured for grabbing epg",
                   channel.DisplayName);
        }
      }
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

    protected void AttachAudioVideoEventHandler(ITvSubChannel subchannel)
    {
      if (subchannel is BaseSubChannel)
      {
        ((BaseSubChannel)subchannel).AudioVideoEvent += AudioVideoEventHandler;
      }
    }

    protected void DetachAudioVideoEventHandler(ITvSubChannel subchannel)
    {
      if (subchannel is BaseSubChannel)
      {
        ((BaseSubChannel)subchannel).AudioVideoEvent -= AudioVideoEventHandler;
      }      
    }
  }
}
