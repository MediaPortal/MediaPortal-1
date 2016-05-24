#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.Scheduler
{
  /// <summary>
  /// class which holds all details about a schedule which is current being recorded
  /// </summary>
  public class RecordingDetail
  {
    #region variables
    
    private readonly ScheduleBLL _schedule;
    private readonly Channel _channel;
    private string _fileName;
    private readonly DateTime _endTime;
    private readonly ProgramBLL _program;
    private int _cardId;
    private DateTime _dateTimeRecordingStarted;
    private Recording _recording;
    private readonly bool _isSerie;
    private readonly IUser _user;

    #endregion

    #region ctor

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="schedule">Schedule of this recording</param>
    /// <param name="endTime">Date/Time the recording should start without pre-record interval</param>
    /// <param name="isSerie">Is serie recording</param>
    public RecordingDetail(Schedule schedule, DateTime endTime, bool isSerie)
    {
      _user = UserFactory.CreateSchedulerUser(schedule.IdSchedule);
      _schedule = new ScheduleBLL(schedule);
      _channel = schedule.Channel;
      _endTime = endTime;
      _program = null;
      _isSerie = isSerie;

      DateTime startTime = DateTime.MinValue;
      if (isSerie)
      {
        DateTime now = DateTime.Now.AddMinutes(schedule.PreRecordInterval ?? SettingsManagement.GetValue("preRecordInterval", 7));
        startTime = new DateTime(now.Year, now.Month, now.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, 0);
      }
      else
      {
        startTime = schedule.StartTime;
      }

      Program program = ProgramManagement.GetProgramAt(startTime, _channel.IdChannel);
      
      //no program? then treat this as a manual recording
      if (program == null)
      {
        program = ProgramFactory.CreateProgram(_channel.IdChannel, DateTime.Now, endTime, "manual");
      }
      _program = new ProgramBLL(program);
    }

    #endregion

    #region properties

    public Recording Recording
    {
      get { return _recording; }
      set { _recording = value; }
    }

    /// <summary>
    /// get the ID of the tuner that is performing the recording
    /// </summary>
    public int CardId
    {
      get { return _cardId; }
      set { _cardId = value; }
    }

    /// <summary>
    /// Gets or sets the recording start date time.
    /// </summary>
    /// <value>The recording start date time.</value>
    public DateTime RecordingStartDateTime
    {
      get { return _dateTimeRecordingStarted; }
      set { _dateTimeRecordingStarted = value; }
    }

    /// <summary>
    /// gets the Schedule belonging to this recording
    /// </summary>
    public ScheduleBLL Schedule
    {
      get { return _schedule; }
    }

    /// <summary>
    /// gets the Channel which is being recorded
    /// </summary>
    public Channel Channel
    {
      get { return _channel; }
    }

    /// <summary>
    /// Gets or sets the filename of the recording
    /// </summary>
    public string FileName
    {
      get { return _fileName; }
      set { _fileName = value; }
    }

    /// <summary>
    /// Gets the date/time on which the recording should stop
    /// </summary>
    public DateTime EndTime
    {
      get { return _endTime; }
    }

    /// <summary>
    /// Gets the Program which is being recorded
    /// </summary>
    public ProgramBLL Program
    {
      get { return _program; }
    }

    /// <summary>
    /// Property which returns true when recording is busy
    /// and false when recording should be stopped
    /// </summary>
    public bool IsRecording(int defaultPostRecordInterval)
    {
      bool isRecording = false;
      try
      {
        Schedule _sched = ScheduleManagement.GetSchedule(_schedule.Entity.IdSchedule); // Refresh();
        if (_sched != null)
        {
          isRecording = (DateTime.Now < EndTime.AddMinutes(_sched.PostRecordInterval ?? defaultPostRecordInterval));
        }
      }
      catch (Exception e)
      {
        this.LogError("RecordingDetail: exception occured {0}", e);
      }
      return isRecording;
    }

    /// <summary>
    /// Property wich returns true if the recording detail is a serie
    /// </summary>
    public bool IsSerie
    {
      get { return _isSerie; }
    }

    public IUser User
    {
      get { return _user; }
    }

    #endregion

    /// <summary>
    /// Create the filename for the recording 
    /// </summary>
    public void MakeFileName()
    {
      string template;
      if (!IsSerie)
      {
        this.LogDebug("RecordingDetail: using movie naming format");
        template = SettingsManagement.GetValue("recordingNamingTemplateNonSeries", "%program_title% - %channel_name% - %date%");
      }
      else
      {
        this.LogDebug("RecordingDetail: using series naming format");
        template = SettingsManagement.GetValue("recordingNamingTemplateSeries", "%program_title%\\%program_title% - %date%[ - S%series_number%E%episode_number%][ - %episode_name%]");
      }
      _fileName = Program.Entity.GetRecordingFileName(template, _schedule.Entity.Channel.Name);
    }
  }
}