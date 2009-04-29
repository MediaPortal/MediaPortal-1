#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

//#define LOGPROPERTIES  

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Summary description for RecorderProperties.
  /// </summary>
  public class RecorderProperties
  {
    private static TVChannel _currentTvChannel = null;
    private static TVRecording _lastTvRecording = null;
    private static TVProgram _lastProgramRecording = null;
    private static List<TVChannel> _tvChannelList = new List<TVChannel>();
    private static long _programStart = -1;
    private static bool _subscribeRecorderHandler = false;

    static RecorderProperties()
    {
    }

    public static void Init()
    {
      _tvChannelList.Clear();
      TVDatabase.GetChannels(ref _tvChannelList);

      if (!_subscribeRecorderHandler)
      {
        _subscribeRecorderHandler = true;
        Recorder.OnTvChannelChanged += new Recorder.OnTvChannelChangeHandler(Recorder_OnTvChannelChanged);
        Recorder.OnTvRecordingChanged += new Recorder.OnTvRecordingChangedHandler(Recorder_OnTvRecordingChanged);
      }
      Clean();
    }

    /// <summary>
    /// Updates the TV tags for the skin bases on the current tv recording...
    /// </summary>
    /// <remarks>
    /// Tags updated are:
    /// #TV.Record.channel, #TV.Record.start,#TV.Record.stop, #TV.Record.genre, #TV.Record.title, #TV.Record.description, #TV.Record.thumb
    /// </remarks>
    public static void UpdateRecordingProperties()
    {
      // handle properties...
      if (Recorder.IsRecording())
      {
        DateTime dtStart, dtEnd, dtStarted;
        if (_lastProgramRecording != null)
        {
          dtStart = _lastProgramRecording.StartTime;
          dtEnd = _lastProgramRecording.EndTime;
          dtStarted = Recorder.TimeRecordingStarted;
          if (dtStarted < dtStart)
          {
            dtStarted = dtStart;
          }
          SetProgressBarProperties(dtStart, dtStarted, dtEnd);
        }
        else
        {
          if (_lastTvRecording != null)
          {
            dtStart = _lastTvRecording.StartTime;
            dtEnd = _lastTvRecording.EndTime;
            dtStarted = Recorder.TimeRecordingStarted;
            if (dtStarted < dtStart)
            {
              dtStarted = dtStart;
            }
            SetProgressBarProperties(dtStart, dtStarted, dtEnd);
          }
        }
      }
      else if (Recorder.View && _currentTvChannel != null)
      {
        if (_currentTvChannel.CurrentProgram != null)
        {
          DateTime showStart, showEnd, timeshiftStart;
          showStart = _currentTvChannel.CurrentProgram.StartTime;
          showEnd = _currentTvChannel.CurrentProgram.EndTime;
          timeshiftStart = Recorder.TimeTimeshiftingStarted;
          if (g_Player.Playing && g_Player.IsTV)
          {
            //we're playing the timeshift file of tv
            //check which program we are currently watching in the timeshift file
            //and use that to update the properties
            DateTime livePoint = timeshiftStart.AddSeconds(g_Player.CurrentPosition);
            livePoint = livePoint.AddSeconds(g_Player.ContentStart);
            if (livePoint < showStart || livePoint > showEnd)
            {
              //Log.Info("get program at livepoint:{0} timeshift start:{1}", livePoint.ToString(),timeshiftStart.ToString());
              TVProgram program = _currentTvChannel.GetProgramAt(livePoint);
              if (program != null)
              {
                showStart = program.StartTime;
                showEnd = program.EndTime;
                //Log.Info("  program at livepoint:{0} {1}-{2} start:{3}", livePoint.ToString(), showStart.ToString(), showEnd.ToString(), timeshiftStart.ToString());
                SetProgressBarProperties(showStart, timeshiftStart, showEnd);
                UpdateTvProgramProperties(_currentTvChannel.Name, program);
                return;
              }
              UpdateTvProgramProperties(_currentTvChannel.Name, null);
              return;
            }
          }
          SetProgressBarProperties(showStart, timeshiftStart, showEnd);
        }
        else
        {
          // we dont have any tvguide data. 
          // so just suppose program started when timeshifting started and ends 2 hours after that
          DateTime dtStart, dtEnd, dtStarted;
          dtStart = Recorder.TimeTimeshiftingStarted;

          dtEnd = dtStart;
          dtEnd = dtEnd.AddHours(2);

          dtStarted = Recorder.TimeTimeshiftingStarted;
          if (dtStarted < dtStart)
          {
            dtStarted = dtStart;
          }
          SetProgressBarProperties(dtStart, dtStarted, dtEnd);
        }
      }
      if (_currentTvChannel != null)
      {
        TVProgram currentTvProgram = _currentTvChannel.CurrentProgram;
        if (currentTvProgram != null && currentTvProgram.Start != _programStart)
        {
          UpdateTvProgramProperties(_currentTvChannel.Name);
        }
      }
    }

    /// <summary>
    /// Empties/clears all tv related skin tags. Gets called during startup en shutdown of
    /// the scheduler
    /// </summary>
    public static void Clean()
    {
      _currentTvChannel = null;
      _lastTvRecording = null;
      _lastProgramRecording = null;

      GUIPropertyManager.SetProperty("#TV.View.channel", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.thumb", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.start", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.stop", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.remaining", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.genre", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.title", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.description", string.Empty);

      GUIPropertyManager.SetProperty("#TV.Next.start", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.stop", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.genre", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.title", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.description", string.Empty);

      GUIPropertyManager.SetProperty("#TV.Record.channel", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.start", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.stop", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.genre", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.title", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.description", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.thumb", string.Empty);

      GUIPropertyManager.SetProperty("#TV.Record.percent1", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.percent2", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.percent3", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.duration", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Record.current", string.Empty);
    } //static void CleanProperties()

#if LOGPROPERTIES
    static DateTime updatetimer=DateTime.Now;
#endif

    /// <summary>
    /// this method will update all tags for the tv progress bar
    /// </summary>
    private static void SetProgressBarProperties(DateTime MovieStartTime, DateTime RecordingStarted,
                                                 DateTime MovieEndTime)
    {
#if LOGPROPERTIES
      TimeSpan ts1 = DateTime.Now - updatetimer;
      if (ts1.TotalSeconds < 30) return;
      updatetimer = DateTime.Now;
#endif
      //                                1s       20s        200s
      //  0sec       100s         200s  201s    220s        400s
      // [MS------------------------ME][MS--------------------ME]
      //             [RS]-------------->RS
      //             ---------------------------[LP]    (current position=220 - (200-100) = 20s)
      //             
      //
      // 0     10            20                    50                      100
      //[MS----------------------------------------------------------------[ME]
      //      [RS]XXXXXXXXXX-----------------------[LP]
      //                    [RS]-------------------[LP]
      //                      
      TimeSpan tsMovieDuration = (MovieEndTime - MovieStartTime);
      float fMovieDurationInSecs = (float) tsMovieDuration.TotalSeconds;
      GUIPropertyManager.SetProperty("#TV.Record.duration",
                                     Util.Utils.SecondsToShortHMSString((int) fMovieDurationInSecs));

      float notInBufferAnymore = 0;
      float currentPlayingPosition = 0;
      notInBufferAnymore = (float) g_Player.ContentStart;
      currentPlayingPosition = (float) g_Player.CurrentPosition;
#if LOGPROPERTIES
      TimeSpan tsCP = new TimeSpan(0, 0, 0, (int)currentPlayingPosition);
      TimeSpan tsNB = new TimeSpan(0, 0, 0, (int)notInBufferAnymore);
      Log.Info("movie :{0} {1}-{2}", GUIPropertyManager.GetProperty("#TV.View.title"), GUIPropertyManager.GetProperty("#TV.View.start"), GUIPropertyManager.GetProperty("#TV.View.stop"));
      Log.Info("movie     start   :{0}", MovieStartTime.ToString());
      Log.Info("movie     end     :{0}", MovieEndTime.ToString());
      Log.Info("movie     duration:{0}", tsMovieDuration.ToString());
      Log.Info("timeshift started :{0}", RecordingStarted.ToString());
      Log.Info("current position  :{0} {1}", currentPlayingPosition.ToString(), tsCP.ToString());
      Log.Info("notInBufferAnymore:{0} {1}", notInBufferAnymore.ToString(), tsNB.ToString());
#endif
      if (g_Player.Playing && g_Player.IsTV)
      {
        notInBufferAnymore = (float) g_Player.ContentStart;
        currentPlayingPosition = (float) g_Player.CurrentPosition;
        RecordingStarted = RecordingStarted.AddSeconds(notInBufferAnymore);
        //currentPlayingPosition -= notInBufferAnymore;

        if (RecordingStarted < MovieStartTime)
        {
          TimeSpan ts = MovieStartTime - RecordingStarted;
          currentPlayingPosition -= (float) ts.TotalSeconds;
          RecordingStarted = MovieStartTime;
        }
      }
#if LOGPROPERTIES
      tsCP = new TimeSpan(0, 0, 0, (int)currentPlayingPosition);
      Log.Info("  timeshift started :{0}", RecordingStarted.ToString());
      Log.Info("  current position  :{0} {1}", currentPlayingPosition.ToString(), tsCP.ToString());
#endif
      // get point where we started timeshifting/recording relative to the start of movie
      TimeSpan tsRecordingStart = (RecordingStarted - MovieStartTime);
      float fRelativeRecordingStart = (float) tsRecordingStart.TotalSeconds;
      float percentRecStart = (fRelativeRecordingStart/fMovieDurationInSecs)*100.00f;
      int iPercentRecStart = (int) Math.Floor(percentRecStart);
      GUIPropertyManager.SetProperty("#TV.Record.percent1", iPercentRecStart.ToString());

      // get the point we're currently watching relative to the start of movie
      if (g_Player.Playing && g_Player.IsTV)
      {
        float fRelativeViewPoint = (float) currentPlayingPosition + fRelativeRecordingStart;
        float fPercentViewPoint = (fRelativeViewPoint/fMovieDurationInSecs)*100.00f;
        int iPercentViewPoint = (int) Math.Floor(fPercentViewPoint);
        GUIPropertyManager.SetProperty("#TV.Record.percent2", iPercentViewPoint.ToString());
        GUIPropertyManager.SetProperty("#TV.Record.current",
                                       Util.Utils.SecondsToShortHMSString((int) fRelativeViewPoint));
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Record.percent2", iPercentRecStart.ToString());
        GUIPropertyManager.SetProperty("#TV.Record.current",
                                       Util.Utils.SecondsToShortHMSString((int) fRelativeRecordingStart));
      }

      // get point the live program is now
      TimeSpan tsRelativeLivePoint = (DateTime.Now - MovieStartTime);
      float fRelativeLiveSec = (float) tsRelativeLivePoint.TotalSeconds;
      float percentLive = (fRelativeLiveSec/fMovieDurationInSecs)*100.00f;
      int iPercentLive = (int) Math.Floor(percentLive);
      if (iPercentLive > 100)
      {
        iPercentLive = 100;
      }
      GUIPropertyManager.SetProperty("#TV.Record.percent3", iPercentLive.ToString());

#if LOGPROPERTIES
      Log.Info("  percent1 :{0} percent2:{1} percent3:{2}",
            GUIPropertyManager.GetProperty("#TV.Record.percent1"),
            GUIPropertyManager.GetProperty("#TV.Record.percent2"),
            GUIPropertyManager.GetProperty("#TV.Record.percent3"));
#endif
    } //static void SetProgressBarProperties(DateTime MovieStartTime,DateTime RecordingStarted, DateTime MovieEndTime)


    private static void Recorder_OnTvChannelChanged(string tvChannelName)
    {
      Log.WriteFile(LogType.Recorder, "Recorder: tv channel changed:{0}", tvChannelName);
      UpdateTvProgramProperties(tvChannelName);
    }

    private static void UpdateTvProgramProperties(string tvChannelName, TVProgram prog)
    {
      _programStart = -1;
      // for each tv-channel
      for (int i = 0; i < _tvChannelList.Count; ++i)
      {
        TVChannel chan = _tvChannelList[i];
        if (chan.Name.Equals(tvChannelName))
        {
          _currentTvChannel = chan;
          break;
        }
      }


      if (prog != null)
      {
        _programStart = prog.Start;
        GUIPropertyManager.SetProperty("#TV.View.start",
                                       prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.View.stop",
                                       prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        GUIPropertyManager.SetProperty("#TV.View.remaining",
                                       Util.Utils.SecondsToHMSString(prog.EndTime - prog.StartTime));
        GUIPropertyManager.SetProperty("#TV.View.genre", prog.Genre);
        GUIPropertyManager.SetProperty("#TV.View.title", prog.Title);
        GUIPropertyManager.SetProperty("#TV.View.description", prog.Description);
        //Log.Info("current program(live) :{0} {1}-{2}", prog.Title, prog.StartTime.ToString(), prog.EndTime.ToString());
        // up next in tv
        TVProgram prognext = _currentTvChannel.GetProgramAt(prog.EndTime.AddMinutes(1));
        if (prognext != null)
        {
          GUIPropertyManager.SetProperty("#TV.Next.start",
                                         prognext.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.Next.stop",
                                         prognext.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.Next.genre", prognext.Genre);
          GUIPropertyManager.SetProperty("#TV.Next.title", prognext.Title);
          GUIPropertyManager.SetProperty("#TV.Next.description", prognext.Description);
        }
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.View.start", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.stop", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.remaining", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.genre", string.Empty);
        GUIPropertyManager.SetProperty("#TV.View.title", tvChannelName);
        GUIPropertyManager.SetProperty("#TV.View.description", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.start", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.stop", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.genre", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Next.title", tvChannelName);
        GUIPropertyManager.SetProperty("#TV.Next.description", string.Empty);
      }

      string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, tvChannelName);
      if (!File.Exists(strLogo))
      {
        strLogo = "defaultVideoBig.png";
      }
      GUIPropertyManager.SetProperty("#TV.View.channel", tvChannelName);
      GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);
    }

    private static void UpdateTvProgramProperties(string tvChannelName)
    {
      _programStart = -1;
      // for each tv-channel
      for (int i = 0; i < _tvChannelList.Count; ++i)
      {
        TVChannel chan = _tvChannelList[i];
        if (chan.Name.Equals(tvChannelName))
        {
          _currentTvChannel = chan;
          break;
        }
      }

      GUIPropertyManager.SetProperty("#TV.View.start", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.stop", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.remaining", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.genre", string.Empty);
      GUIPropertyManager.SetProperty("#TV.View.title", tvChannelName);
      GUIPropertyManager.SetProperty("#TV.View.description", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.start", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.stop", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.genre", string.Empty);
      GUIPropertyManager.SetProperty("#TV.Next.title", tvChannelName);
      GUIPropertyManager.SetProperty("#TV.Next.description", string.Empty);


      if (_currentTvChannel != null)
      {
        TVProgram prog = _currentTvChannel.CurrentProgram;
        if (prog != null)
        {
          _programStart = prog.Start;
          GUIPropertyManager.SetProperty("#TV.View.start",
                                         prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.View.stop",
                                         prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          GUIPropertyManager.SetProperty("#TV.View.remaining",
                                         Util.Utils.SecondsToHMSString(prog.EndTime - prog.StartTime));
          GUIPropertyManager.SetProperty("#TV.View.genre", prog.Genre);
          GUIPropertyManager.SetProperty("#TV.View.title", prog.Title);
          GUIPropertyManager.SetProperty("#TV.View.description", prog.Description);
          //Log.Info("current program       :{0} {1}-{2}", prog.Title, prog.StartTime.ToString(), prog.EndTime.ToString());
          // up next in tv
          TVProgram prognext = _currentTvChannel.GetProgramAt(prog.EndTime.AddMinutes(1));
          if (prognext != null)
          {
            GUIPropertyManager.SetProperty("#TV.Next.start",
                                           prognext.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Next.stop",
                                           prognext.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Next.genre", prognext.Genre);
            GUIPropertyManager.SetProperty("#TV.Next.title", prognext.Title);
            GUIPropertyManager.SetProperty("#TV.Next.description", prognext.Description);
          }
        }
      } //if (_currentTvChannel!=null)

      string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, tvChannelName);
      if (!File.Exists(strLogo))
      {
        strLogo = "defaultVideoBig.png";
      }
      GUIPropertyManager.SetProperty("#TV.View.channel", tvChannelName);
      GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);
    }

    private static void Recorder_OnTvRecordingChanged()
    {
      Log.WriteFile(LogType.Recorder, "Recorder: recording state changed");
      if (Recorder.IsRecording())
      {
        if (_lastTvRecording != Recorder.CurrentTVRecording || _lastProgramRecording != Recorder.ProgramRecording)
        {
          _lastTvRecording = Recorder.CurrentTVRecording;
          _lastProgramRecording = Recorder.ProgramRecording;
          if (_lastProgramRecording == null)
          {
            string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, _lastTvRecording.Channel);
            if (!File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            GUIPropertyManager.SetProperty("#TV.Record.thumb", strLogo);
            GUIPropertyManager.SetProperty("#TV.Record.start",
                                           _lastTvRecording.StartTime.ToString("t",
                                                                               CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Record.stop",
                                           _lastTvRecording.EndTime.ToString("t",
                                                                             CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Record.genre", string.Empty);
            GUIPropertyManager.SetProperty("#TV.Record.title", _lastTvRecording.Title);
            GUIPropertyManager.SetProperty("#TV.Record.description", string.Empty);
          }
          else
          {
            string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, _lastProgramRecording.Channel);
            if (!File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            GUIPropertyManager.SetProperty("#TV.Record.thumb", strLogo);
            GUIPropertyManager.SetProperty("#TV.Record.channel", _lastProgramRecording.Channel);
            GUIPropertyManager.SetProperty("#TV.Record.start",
                                           _lastProgramRecording.StartTime.ToString("t",
                                                                                    CultureInfo.CurrentCulture.
                                                                                      DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Record.stop",
                                           _lastProgramRecording.EndTime.ToString("t",
                                                                                  CultureInfo.CurrentCulture.
                                                                                    DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Record.genre", _lastProgramRecording.Genre);
            GUIPropertyManager.SetProperty("#TV.Record.title", _lastProgramRecording.Title);
            GUIPropertyManager.SetProperty("#TV.Record.description", _lastProgramRecording.Description);
          }
        }
      }
      else // not recording.
      {
        if (_lastTvRecording != null)
        {
          _lastTvRecording = null;
          _lastProgramRecording = null;
          GUIPropertyManager.SetProperty("#TV.Record.channel", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Record.start", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Record.stop", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Record.genre", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Record.title", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Record.description", string.Empty);
          GUIPropertyManager.SetProperty("#TV.Record.thumb", string.Empty);
        }
      }
    }
  }
}