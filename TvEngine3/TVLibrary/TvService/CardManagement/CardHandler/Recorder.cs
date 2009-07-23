/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Implementations.DVB;
using TvLibrary.Log;
using TvControl;
using TvDatabase;
using System.Threading;


namespace TvService
{
  public class Recorder
  {
    readonly ITvCardHandler _cardHandler;
    readonly bool _timeshiftingEpgGrabberEnabled;
    readonly int _waitForTimeshifting = 15; // seconds
    ManualResetEvent _eventAudio; // gets signaled when audio PID is seen
    ManualResetEvent _eventVideo; // gets signaled when video PID is seen
    bool _eventsReady;

    DateTime _timeAudioEvent;
    DateTime _timeVideoEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="Recording"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public Recorder(ITvCardHandler cardHandler)
    {
      _eventAudio = new ManualResetEvent(false);
      _eventVideo = new ManualResetEvent(false);
      _eventsReady = true;

      TvBusinessLayer layer = new TvBusinessLayer();
      _cardHandler = cardHandler;
      _timeshiftingEpgGrabberEnabled = (layer.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value == "yes");
      _waitForTimeshifting = Int32.Parse(layer.GetSetting("timeshiftWaitForTimeshifting", "15").Value);

      _timeAudioEvent = DateTime.Now;
      _timeVideoEvent = DateTime.Now;
    }

    private void AudioVideoEventHandler(PidType pidType)
    {
      // we are only interested in video and audio PIDs
      if (pidType == PidType.Audio)
      {
        TimeSpan ts = DateTime.Now - _timeAudioEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          Log.Info("audioVideoEventHandler {0}", pidType);
          _eventAudio.Set();
        }
        _timeAudioEvent = DateTime.Now;
      }

      if (pidType == PidType.Video)
      {
        TimeSpan ts = DateTime.Now - _timeVideoEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          Log.Info("audioVideoEventHandler {0}", pidType);
          _eventVideo.Set();
        }
        _timeVideoEvent = DateTime.Now;
      }
    }
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">if true then create a content recording else a reference recording</param>
    /// <param name="startTime">not used</param>
    /// <returns></returns>
    public TvResult Start(ref User user, ref string fileName, bool contentRecording, long startTime)
    {
      bool useErrorDetection = false;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return TvResult.CardIsDisabled;
        }

        lock (this)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
              return TvResult.CardIsDisabled;

            if (_cardHandler.IsLocal == false)
            {
              return RemoteControl.Instance.StartRecording(ref user, ref fileName, contentRecording, startTime);
            }
          } catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return TvResult.UnknownError;
          }

          TvCardContext context = _cardHandler.Card.Context as TvCardContext;
          if (context == null)
            return TvResult.UnknownChannel;

          context.GetUser(ref user);
          ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
          if (subchannel == null)
            return TvResult.UnknownChannel;
          //gibman 
          // RecordingFormat 0 = ts
          // RecordingFormat 1 = mpeg
          if (subchannel.IsRecordingTransportStream || (_cardHandler.DataBaseCard.RecordingFormat == 0))
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".ts");
          }
          else
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".mpg");
          }

          if (_cardHandler.Card.CardType == CardType.DvbC ||
              _cardHandler.Card.CardType == CardType.DvbS ||
              _cardHandler.Card.CardType == CardType.DvbT
          )
          {
            useErrorDetection = true;
          }

          if (useErrorDetection)
          {
            if (subchannel is BaseSubChannel)
            {
              ((BaseSubChannel)subchannel).AudioVideoEvent += AudioVideoEventHandler;
            }

            if (!_eventsReady)
            {
              _eventAudio = new ManualResetEvent(false);
              _eventVideo = new ManualResetEvent(false);
              _eventsReady = true;
            }
          }

          Log.Write("card: StartRecording {0} {1}", _cardHandler.DataBaseCard.IdCard, fileName);
          bool result = subchannel.StartRecording((_cardHandler.DataBaseCard.RecordingFormat == 0), fileName);
          bool isScrambled;
          if (result)
          {
            fileName = subchannel.RecordingFileName;
            context.Owner = user;
            if (useErrorDetection)
            {
              if (!WaitForRecordingFile(ref user, out isScrambled))
              {
                Log.Write("card: Recording failed! {0} {1}", _cardHandler.DataBaseCard.IdCard, fileName);

                Stop(ref user);
                _cardHandler.Users.RemoveUser(user);
                if (isScrambled)
                {
                  return TvResult.ChannelIsScrambled;
                }
                return TvResult.NoVideoAudioDetected;
              }
            }
          }
          if (_timeshiftingEpgGrabberEnabled)
          {
            Channel channel = Channel.Retrieve(user.IdChannel);
            if (channel.GrabEpg)
              _cardHandler.Card.GrabEpg();
            else
              Log.Info("TimeshiftingEPG: channel {0} is not configured for grabbing epg", channel.DisplayName);
          }

          return TvResult.Succeeded;

        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool Stop(ref User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;
        Log.Write("card: StopRecording card={0}, user={1}", _cardHandler.DataBaseCard.IdCard, user.Name);
        lock (this)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
              return false;

            if (_cardHandler.IsLocal == false)
            {
              return RemoteControl.Instance.StopRecording(ref user);
            }
          } catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return false;
          }
          Log.Write("card: StopRecording for card:{0}", _cardHandler.DataBaseCard.IdCard);
          TvCardContext context = _cardHandler.Card.Context as TvCardContext;
          if (context == null)
          {
            Log.Write("card: StopRecording context null");
            return false;
          }
          if (IsRecording(ref user))
          {
            context.GetUser(ref user);
            ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
            if (subchannel == null)
            {
              Log.Write("card: StopRecording subchannel null, skipping");
              return false;
            }
            subchannel.StopRecording();
            _cardHandler.Card.FreeSubChannel(user.SubChannel);
            if (subchannel.IsTimeShifting == false || context.Users.Length <= 1)
            {
              _cardHandler.Users.RemoveUser(user);
            }
          }
          else
          {
            Log.Write("card: StopRecording user '{0}' not recording, skipping", user.Name);
          }

          User[] users = context.Users;
          for (int i = 0; i < users.Length; ++i)
          {
            ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(users[i].SubChannel);
            if (subchannel != null)
            {
              if (subchannel.IsRecording)
              {
                Log.Write("card: StopRecording setting new context owner on user '{0}'", users[i].Name);
                context.Owner = users[i];
                break;
              }
            }
          }
          return true;
        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }



    public bool IsRecordingChannel(string channelName)
    {
      User[] users = _cardHandler.Users.GetUsers();
      if (users == null)
        return false;
      if (users.Length == 0)
        return false;

      for (int i = 0; i < users.Length; ++i)
      {
        User user = users[i];
        if (!user.IsAdmin)
          continue;
        if (_cardHandler.CurrentChannelName(ref user) == null)
          continue;
        if (_cardHandler.CurrentChannelName(ref user) == channelName)
        {
          if (_cardHandler.Recorder.IsRecording(ref user))
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Gets a value indicating whether this card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this card is recording; otherwise, <c>false</c>.
    /// </value>
    public bool IsAnySubChannelRecording
    {
      get
      {
        User[] users = _cardHandler.Users.GetUsers();
        if (users == null)
          return false;
        if (users.Length == 0)
          return false;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (IsRecording(ref user))
            return true;
        }
        return false;
      }
    }

    public bool IsRecordingAnyUser()
    {
      User[] users = _cardHandler.Users.GetUsers();
      if (users == null)
        return false;
      if (users.Length == 0)
        return false;

      for (int i = 0; i < users.Length; ++i)
      {
        User user = users[i];
        if (!user.IsAdmin)
          continue;
        if (_cardHandler.CurrentChannelName(ref user) == null)
          continue;

        if (_cardHandler.Recorder.IsRecording(ref user))
        {
          return true;
        }

      }
      return false;
    }

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(ref User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return false;

          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.IsRecording(ref user);
          }
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return false;
        return subchannel.IsRecording;
      } catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>filename or null when not recording</returns>
    public string FileName(ref User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return "";

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return "";

          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.RecordingFileName(ref user);
          }
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return "";
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return null;
        return subchannel.RecordingFileName;
      } catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return DateTime.MinValue;
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return DateTime.MinValue;
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.RecordingStarted(user);
          }
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return DateTime.MinValue;
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return DateTime.MinValue;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return DateTime.MinValue;
        return subchannel.RecordingStarted;
      } catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }
    /// <summary>
    /// Waits for recording file to be at leat 300kb. 
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="scrambled">Indicates if the channel is scambled</param>
    /// <returns>true when timeshift files is at least of 300kb, else timeshift file is less then 300kb</returns>
    public bool WaitForRecordingFile(ref User user, out bool scrambled)
    {
      ///(taken from timeshifter)
      scrambled = false;
  
      //lets check if stream is initially scrambled, if it is and the card has no CA, then we are unable to decrypt stream.
      if (_cardHandler.IsScrambled(ref user))
      {
        if (!_cardHandler.HasCA)
        {
          Log.Write("card: WaitForRecordingFile - return scrambled, since card has no CAM.");
          scrambled = true;
          return false;
        }
      }

      int waitForEvent = _waitForTimeshifting * 1000; // in ms           

      DateTime timeStart = DateTime.Now;

      if (_cardHandler.Card.SubChannels.Length <= 0)
        return false;
      IChannel channel = _cardHandler.Card.SubChannels[0].CurrentChannel;
      bool isRadio = channel.IsRadio;

      if (isRadio)
      {
        Log.Write("card: WaitForRecordingFile - waiting _eventAudio");
        // wait for audio PID to be seen
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          // start of the video & audio is seen
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForRecordingFile - audio is seen after {0} seconds", ts.TotalSeconds);
          _eventVideo.Reset();
          _eventAudio.Reset();
          return true;
        }
        else
        {
          _eventVideo.Reset();
          _eventAudio.Reset();
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForRecordingFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(ref user))
          {
            Log.Write("card: WaitForRecordingFile - audio stream is scrambled");
            scrambled = true;
          }
        }
      }
      else
      {
        Log.Write("card: WaitForRecordingFile - waiting _eventAudio & _eventVideo");
        // block until video & audio PIDs are seen or the timeout is reached
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          _eventAudio.Reset();
          if (_eventVideo.WaitOne(waitForEvent, true))
          {
            // start of the video & audio is seen
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write("card: WaitForRecordingFile - video and audio are seen after {0} seconds", ts.TotalSeconds);
            _eventVideo.Reset();
            return true;
          }
          else
          {
            _eventVideo.Reset();
            _eventAudio.Reset();
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write("card: WaitForRecordingFile - video was found, but audio was not found after {0} seconds", ts.TotalSeconds);
            if (_cardHandler.IsScrambled(ref user))
            {
              Log.Write("card: WaitForRecordingFile - audio stream is scrambled");
              scrambled = true;
            }
          }
        }
        else
        {
          _eventVideo.Reset();
          _eventAudio.Reset();
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForRecordingFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(ref user))
          {
            Log.Write("card: WaitForRecordingFile - audio and video stream is scrambled");
            scrambled = true;
          }
        }
      }
      return false;
    }
  }
}
