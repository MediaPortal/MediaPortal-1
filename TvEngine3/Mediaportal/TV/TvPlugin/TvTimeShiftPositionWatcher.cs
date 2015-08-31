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
using System.IO;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Configuration;
using MediaPortal.Player;

namespace Mediaportal.TV.TvPlugin
{
  internal class TvTimeShiftPositionWatcher
  {
    #region variables

    private static TvTimeShiftPositionWatcher _instance = null;

    private bool _isEnabled = false;
    private ChannelBLL _channel = null;
    private long _snapshotBufferPosition = -1;
    private long _snapshotBufferId = -1;
    private Timer _timer = null;
    private int _secondsElapsed = 0;

    #endregion

    private TvTimeShiftPositionWatcher()
    {
      _isEnabled = DebugSettings.EnableRecordingFromTimeshift;
      if (_isEnabled)
      {
        _timer = new Timer();
        _timer.Interval = 1000;
        _timer.Tick += new EventHandler(_timer_Tick);
        g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
      }
    }

    ~TvTimeShiftPositionWatcher()
    {
      if (_timer != null)
      {
        _timer.Dispose();
      }
    }

    public static TvTimeShiftPositionWatcher Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new TvTimeShiftPositionWatcher();
        }
        return _instance;
      }
    }

    public void SetNewChannel(Channel channel)
    {
      if (!_isEnabled)
      {
        return;
      }

      this.LogDebug("time-shift position watcher: set channel, ID = {0}", channel == null ? -1 : channel.IdChannel);
      _snapshotBufferPosition = -1;
      _snapshotBufferId = -1;
      if (channel == null)
      {
        _channel = null;
        _timer.Enabled = false;
        return;
      }

      _channel = new ChannelBLL(channel);
      if (!TVHome.Card.IsRecording)
      {
        SnapshotTimeShiftBuffer();
      }
      _secondsElapsed = 0;
      _timer.Enabled = true;
    }

    private void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type == g_Player.MediaType.TV)
      {
        SetNewChannel(null);
      }
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
      try
      {
        if (_channel == null || _channel.Entity == null || !TVHome.Connected)
        {
          return;
        }

        // Copy the time-shift buffer files if a recording has started mid-program.
        // TODO the logic for this section seems to be seriously flawed
        if (_snapshotBufferPosition != -1 && TVHome.Card.IsRecording)
        {
          int scheduleId = TVHome.Card.RecordingScheduleId;
          if (scheduleId > 0)
          {
            Recording rec = ServiceAgents.Instance.RecordingServiceAgent.GetActiveRecording(scheduleId);
            if (rec.IdChannel == _channel.Entity.IdChannel)
            {
              this.LogDebug("time-shift position watcher: recording started, title = {0}, file name = {1}", rec.Title, rec.FileName);
              IUser u = TVHome.Card.User;
              long currentBufferPosition = 0;
              long currentBufferId = 0;
              if (ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, out currentBufferPosition, out currentBufferId))
              {
                string destination = Path.Combine(Path.GetDirectoryName(rec.FileName), string.Format("{0}_buffer{1}", Path.GetFileNameWithoutExtension(rec.FileName), Path.GetExtension(rec.FileName)));
                this.LogInfo("time-shift position watcher: copy buffer file(s), destination = {0}", destination);
                ServiceAgents.Instance.ControllerServiceAgent.CopyTimeShiftBuffer(u.Name, _snapshotBufferPosition, _snapshotBufferId,
                                                                                  currentBufferPosition, currentBufferId, destination);
              }
            }
            _snapshotBufferPosition = -1;
            _snapshotBufferId = -1;
          }
        }

        // Take a new snapshot if a new program is about to start.
        _secondsElapsed++;
        if (_secondsElapsed >= 60 && _snapshotBufferPosition == -1)
        {
          Program program = _channel.CurrentProgram;
          if (program != null)
          {
            DateTime now = DateTime.Now.AddMinutes(ServiceAgents.Instance.SettingServiceAgent.GetValue("preRecordInterval", 7));
            if (now >= program.EndTime)
            {
              this.LogDebug("time-shift position watcher: next program starts within the configured pre-recording interval, current program = {0}, end time = {1}", program.Title, program.EndTime);
              SnapshotTimeShiftBuffer();
            }
          }
          _secondsElapsed = 0;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "time-shift position watcher: unexpected exception");
      }
    }

    private void SnapshotTimeShiftBuffer()
    {
      this.LogInfo("time-shift position watcher: snapshotting time-shift buffer");

      IUser u = TVHome.Card.User;
      if (u == null)
      {
        this.LogError("time-shift position watcher: snapshot failed, user not set");
        return;
      }
      if (!ServiceAgents.Instance.ControllerServiceAgent.TimeShiftGetCurrentFilePosition(u.Name, out _snapshotBufferPosition, out _snapshotBufferId))
      {
        this.LogError("time-shift position watcher: snapshot failed");
        _snapshotBufferPosition = -1;
        _snapshotBufferId = -1;
        return;
      }
      this.LogDebug("time-shift position watcher: snapshot complete, position = {0}, buffer ID = {1}", _snapshotBufferPosition, _snapshotBufferId);
    }
  }
}