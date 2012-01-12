#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvDatabase;
using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Player;

namespace TvPlugin
{
  internal class TvTimeShiftPositionWatcher
  {
    #region Variables
    private static int idChannelToWatch = -1;
    private static Int64 snapshotBuferPosition = -1;
    private static string snapshotBufferFile = "";
    private static decimal preRecordInterval = -1;
    private static Timer _timer=null;
    private static int isEnabled = 0;
    private static int secondsElapsed = 0;
    #endregion

    #region Event handlers
    static void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type == g_Player.MediaType.TV)
      {
        SetNewChannel(-1);
      }
    }
    static void _timer_Tick(object sender, EventArgs e)
    {
      CheckRecordingStatus();
      secondsElapsed++;
      if (secondsElapsed == 60)
      {
        CheckOrUpdateTimeShiftPosition();
        secondsElapsed = 0;
      }
    }
    #endregion

    #region Private members
    private static bool IsEnabled()
    {
      if (isEnabled == 0)
      {
        if (DebugSettings.EnableRecordingFromTimeshift)
          isEnabled = 1;
        else
          isEnabled = -1;
      }
      return (isEnabled == 1);
    }
    private static void StartTimer()
    {
      if (_timer == null)
      {
        _timer = new Timer();
        _timer.Interval = 1000;
        _timer.Tick += new EventHandler(_timer_Tick);
        g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
      }
      Log.Debug("TvTimeShiftPositionWatcher: Channel changed.");
      SnapshotTimeShiftBuffer();
      secondsElapsed = 0;
      _timer.Enabled = true;
    }
    private static void SnapshotTimeShiftBuffer()
    {
      Log.Debug("TvTimeShiftPositionWatcher: Snapshotting timeshift buffer");
      IUser u = TVHome.Card.User;
      if (u == null)
      {
        Log.Error("TvTimeShiftPositionWatcher: Snapshot buffer failed. TvHome.Card.User==null");
        return;
      }
      long bufferId = 0;
      if (!RemoteControl.Instance.TimeShiftGetCurrentFilePosition(ref u, ref snapshotBuferPosition, ref bufferId))
      {
        Log.Error("TvTimeShiftPositionWatcher: TimeShiftGetCurrentFilePosition failed.");
        return;
      }
      snapshotBufferFile = RemoteControl.Instance.TimeShiftFileName(ref u) + bufferId.ToString() + ".ts";
      Log.Debug("TvTimeShiftPositionWatcher: Snapshot done - position: {0}, filename: {1}", snapshotBuferPosition, snapshotBufferFile);
    }
    private static void CheckRecordingStatus()
    {
      try
      {
        if (TVHome.Card.IsRecording)
        {
          int scheduleId = TVHome.Card.RecordingScheduleId;
          if (scheduleId > 0)
          {
            Recording rec = Recording.ActiveRecording(scheduleId);
            Log.Debug("TvTimeShiftPositionWatcher: Detected a started recording. ProgramName: {0}", rec.Title);
            InitiateBufferFilesCopyProcess(rec.FileName);
            SetNewChannel(-1);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvTimeshiftPositionWatcher.CheckRecordingStatus exception : {0}", ex);
      }
    }
    private static void CheckOrUpdateTimeShiftPosition()
    {
      if (idChannelToWatch == -1)
        return;
      if (!TVHome.Connected)
        return;
      Channel chan = Channel.Retrieve(idChannelToWatch);
      if (chan == null)
        return;
      try
      {
        DateTime current = DateTime.Now;
        current = current.AddMinutes((double)preRecordInterval);
        current = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);
        DateTime dtProgEnd = chan.CurrentProgram.EndTime;
        dtProgEnd = new DateTime(dtProgEnd.Year, dtProgEnd.Month, dtProgEnd.Day, dtProgEnd.Hour, dtProgEnd.Minute, 0);
        Log.Debug("TvTimeShiftPositionWatcher: Checking {0} == {1}", current.ToString("dd.MM.yy HH:mm"), dtProgEnd.ToString("dd.MM.yy HH:mm"));
        if (current == dtProgEnd)
        {
          Log.Debug("TvTimeShiftPositionWatcher: Next program starts within the configured Pre-Rec interval. Current program: [{0}] ending: {1}", chan.CurrentProgram.Title, chan.CurrentProgram.EndTime.ToString());
          SnapshotTimeShiftBuffer();
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvTimeshiftPositionWatcher.CheckOrUpdateTimeShiftPosition exception : {0}", ex);
      }
    }
    private static void InitiateBufferFilesCopyProcess(string recordingFilename)
    {
      if (!IsEnabled())
        return;
      if (snapshotBuferPosition != -1)
      {
        IUser u = TVHome.Card.User;
        long bufferId = 0;
        Int64 currentPosition = -1;
        if (RemoteControl.Instance.TimeShiftGetCurrentFilePosition(ref u, ref currentPosition, ref bufferId))
        {
          string currentFile = RemoteControl.Instance.TimeShiftFileName(ref u) + bufferId.ToString() + ".ts";
          Log.Info("**");
          Log.Info("**");
          Log.Info("**");
          Log.Info("TvTimeshiftPositionWatcher: Starting to copy buffer files for recording {0}", recordingFilename);
          Log.Info("**");
          Log.Info("**");
          Log.Info("**");
          RemoteControl.Instance.CopyTimeShiftFile(snapshotBuferPosition, snapshotBufferFile, currentPosition,
                                                   currentFile, recordingFilename);
        }
      }
    }
    #endregion

    public static void SetNewChannel(int idChannel)
    {
      if (!IsEnabled())
        return;
      if (preRecordInterval == -1)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        preRecordInterval = Decimal.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      }
      Log.Debug("TvTimeShiftPositionWatcher: SetNewChannel(" + idChannel.ToString() + ")");
      idChannelToWatch = idChannel;
      if (idChannel == -1)
      {
        snapshotBuferPosition = -1;
        snapshotBufferFile = "";
        _timer.Enabled = false;
        Log.Debug("TvTimeShiftPositionBuffer: Timer stopped because recording on this channel started or tv stopped.");
      }
      else
        StartTimer();
    }
  }
}