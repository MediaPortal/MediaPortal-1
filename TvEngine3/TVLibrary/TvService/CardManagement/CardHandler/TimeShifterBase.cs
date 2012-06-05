using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TvControl;
using TvDatabase;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Log;

namespace TvService
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

    protected TimeShifterBase(ITvCardHandler cardHandler)
    {
      _eventAudio.Reset();
      _eventVideo.Reset();

      var layer = new TvBusinessLayer();
      _waitForTimeshifting = Int32.Parse(layer.GetSetting("timeshiftWaitForTimeshifting", "15").Value);

      if (_cardHandler != null && _cardHandler.Tuner != null)
      {
        _cardHandler.Tuner.OnAfterCancelTuneEvent += Tuner_OnAfterCancelTuneEvent;
      }

      if (_cardHandler != null && _cardHandler.Tuner != null)
      {
        _cardHandler.Tuner.OnAfterCancelTuneEvent += new CardTuner.OnAfterCancelTuneDelegate(Tuner_OnAfterCancelTuneEvent);
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

    protected ITvSubChannel GetSubChannel(ref IUser user)
    {
      ITvSubChannel subchannel = null;
      if (_cardHandler.DataBaseCard.Enabled)
      {
        var context = _cardHandler.Card.Context as TvCardContext;
        if (context != null)
        {
          bool userExists;
          context.GetUser(ref user, out userExists);
          if (userExists)
          {
            subchannel = GetSubChannel(user.SubChannel);
          }          
        }        
      }
      
      return subchannel;
    }

    private bool IsScrambled(ref IUser user)
    {
      bool isScrambled = false;
      //lets check if stream is initially scrambled, if it is and the card has no CA, then we are unable to decrypt stream.
      if (_cardHandler.IsScrambled(ref user))
      {
        if (!_cardHandler.HasCA)
        {
          Log.Write("card: IsScrambled - return scrambled, since card has no CAM.");
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

      if (_cardHandler.DataBaseCard.Enabled == false)
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
      bool isRadio = channel.IsRadio;

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
          if (_cardHandler.IsScrambled(ref user))
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
            if (_cardHandler.IsScrambled(ref user))
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
          if (_cardHandler.IsScrambled(ref user))
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
        Channel channel = Channel.Retrieve(user.IdChannel);
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
