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
using System.IO;
using System.Text.RegularExpressions;
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
    /// <param name="channel">Channel on which the recording is done</param>
    /// <param name="endTime">Date/Time the recording should start without pre-record interval</param>
    /// <param name="isSerie">Is serie recording</param>
    public RecordingDetail(Schedule schedule, Channel channel, DateTime endTime, bool isSerie)
    {
      _user = UserFactory.CreateSchedulerUser(schedule.IdSchedule);
      _schedule = new ScheduleBLL(schedule);
      _channel = channel;
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

      Program program = ProgramManagement.GetProgramAt(startTime, schedule.Channel.IdChannel);
      
      //no program? then treat this as a manual recording
      if (program == null)
      {
        program = ProgramFactory.CreateEmptyProgram();
        program.IdChannel = 0;
        program.StartTime = DateTime.Now;
        program.EndTime = endTime;
        program.Title = "manual";
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
      string format;
      if (!IsSerie)
      {
        this.LogDebug("RecordingDetail: using movie naming format");
        format = SettingsManagement.GetValue("moviesformat", "%title%");
      }
      else
      {
        this.LogDebug("RecordingDetail: using series naming format");
        format = SettingsManagement.GetValue("seriesformat", "%title%");
      }
      if (string.IsNullOrEmpty(format))
      {
        format = "%title%";
      }

      string[] tagNames = {
                            "%program_title%",
                            "%episode_name%",
                            "%series_number%",
                            "%episode_number%",
                            "%episode_part%",
                            "%channel_name%",
                            "%genre%",
                            "%date%",
                            "%start%",
                            "%end%",
                            "%start_year%",
                            "%start_month%",
                            "%start_day%",
                            "%start_hour%",
                            "%start_minute%",
                            "%end_year%",
                            "%end_month%",
                            "%end_day%",
                            "%end_hour%",
                            "%end_minute%"
                          };

      // Limit length of title and episode name to try to ensure the file name
      // length is kept within limits (eg. MAX_PATH).
      string programTitle = Program.Entity.Title;
      if (programTitle.Length > 80)
      {
        programTitle = programTitle.Substring(0, 77) + "...";
      }
      string programEpisodeName = Program.Entity.EpisodeName ?? string.Empty;
      if (programEpisodeName.Length > 80)
      {
        programEpisodeName = programEpisodeName.Substring(0, 77) + "...";
      }

      string channelName = _schedule.Entity.Channel.Name.Trim();
      string programCategory = string.Empty;
      if (Program.Entity.ProgramCategory != null)
      {
        programCategory = Program.Entity.ProgramCategory.Category.Trim();
      }
      string[] tagValues = {
                             programTitle,
                             programEpisodeName,
                             Program.Entity.SeasonNumber.ToString(),
                             Program.Entity.EpisodeNumber.ToString(),
                             Program.Entity.EpisodePartNumber.ToString(),
                             channelName,
                             programCategory,
                             Program.Entity.StartTime.ToString("yyyy-MM-dd"),
                             Program.Entity.StartTime.ToShortTimeString(),
                             Program.Entity.EndTime.ToShortTimeString(),
                             Program.Entity.StartTime.ToString("yyyy"),
                             Program.Entity.StartTime.ToString("MM"),
                             Program.Entity.StartTime.ToString("dd"),
                             Program.Entity.StartTime.ToString("HH"),
                             Program.Entity.StartTime.ToString("mm"),
                             Program.Entity.EndTime.ToString("yyyy"),
                             Program.Entity.EndTime.ToString("MM"),
                             Program.Entity.EndTime.ToString("dd"),
                             Program.Entity.EndTime.ToString("HH"),
                             Program.Entity.EndTime.ToString("mm")
                           };

      for (int i = 0; i < tagNames.Length; i++)
      {
        format = ReplaceTag(format, tagNames[i], tagValues[i]);
        if (!format.Contains("%"))
        {
          break;
        }
      }

      format = format.Trim();
      if (string.IsNullOrEmpty(format))
      {
        format = string.Format("{0}_{1}_{2}", channelName, programTitle, Program.Entity.StartTime.ToString("yyyy-MM-dd_HHmm"));
      }

      _fileName = format;
    }

    private static string ReplaceTag(string line, string tag, string value)
    {
      value = value.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');

      // This regex checks for optional sections of the form [*%tag%*].
      // Nesting (eg. "Hello [%tag1% [%tag2% ]]world!") is not supported.
      string regexPattern = string.Format(@"\[[^%]*{0}[^\]]*[\]]", tag.Replace("%", "\\%"));
      Regex r;
      try
      {
        r = new Regex(regexPattern);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "RecordingDetail: recording file name tag replacement regex failed, regex = {0}", regexPattern);
        return line;
      }

      Match match = r.Match(line);
      if (match.Success)  // means there are one or more optional sections
      {
        // Remove the entire optional section. If the tag has a value, reinsert
        // the section without the square braces.
        line = line.Remove(match.Index, match.Length);
        if (!string.IsNullOrEmpty(value))
        {
          string m = match.Value.Substring(1, match.Value.Length - 2);
          line = line.Insert(match.Index, m);
        }
      }

      // Replace tags.
      return line.Replace(tag, value);
    }
  }
}