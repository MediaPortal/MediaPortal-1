#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace MediaPortal.TV.Database
{
  public class PrePostRecord
  {
    //specifies the number of minutes the notify should be send before a program starts
    private int _preRecordingWarningTime = 2;
    // number of minutes we should start recording before the program starts
    private int _preRecordInterval = 0;
    // number of minutes we keeprecording after the program starts
    private int _postRecordInterval = 0;

    private static PrePostRecord _instance;

    private PrePostRecord()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _preRecordInterval = xmlreader.GetValueAsInt("capture", "prerecord", 5);
        _postRecordInterval = xmlreader.GetValueAsInt("capture", "postrecord", 5);
        _preRecordingWarningTime = xmlreader.GetValueAsInt("mytv", "recordwarningtime", 2);
      }
    }

    public static PrePostRecord Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new PrePostRecord();
        }
        return _instance;
      }
    }

    public int DefaultPreRecord
    {
      get { return _preRecordInterval; }
      set { _preRecordInterval = value; }
    }

    public int DefaultPostRecord
    {
      get { return _postRecordInterval; }
      set { _postRecordInterval = value; }
    }

    public int PreRecordingWarningTime
    {
      get { return _preRecordingWarningTime; }
      set { _preRecordingWarningTime = value; }
    }
  }

  /// <summary>
  /// Class which contains all information about a scheduled recording
  /// </summary>
  [Serializable()]
  public class TVRecording
  {
    public class PriorityComparer : IComparer<TVRecording>
    {
      private bool _sortAscending = true;

      public PriorityComparer(bool sortAscending)
      {
        _sortAscending = sortAscending;
      }

      #region IComparer Members

      public int Compare(TVRecording rec1, TVRecording rec2)
      {
        if (_sortAscending)
        {
          if (rec1.Priority > rec2.Priority)
          {
            return -1;
          }
          if (rec1.Priority < rec2.Priority)
          {
            return 1;
          }
        }
        else
        {
          if (rec1.Priority > rec2.Priority)
          {
            return 1;
          }
          if (rec1.Priority < rec2.Priority)
          {
            return -1;
          }
        }
        return 0;
      }

      #endregion
    }


    public static readonly int HighestPriority = Int32.MaxValue;
    public static readonly int LowestPriority = 0;

    #region enums

    /// <summary>
    /// Type of recording
    /// </summary>
    public enum RecordingType
    {
      Once,
      EveryTimeOnThisChannel,
      EveryTimeOnEveryChannel,
      Daily,
      Weekly,
      WeekDays,
      WeekEnds
    } ;

    //quality of recording
    public enum QualityType
    {
      NotSet,
      Portable,
      Low,
      Medium,
      High
    }

    /// <summary>
    /// Current recording status
    /// </summary>
    public enum RecordingStatus
    {
      Waiting,
      Finished,
      Canceled
    } ;

    #endregion

    #region variables

    private long _startTime;
    private long _endTime;
    private long _timeCanceled = 0;
    private string _title;
    private string _channelName;
    private RecordingType _recordingType;
    private int _recordingId;
    private bool _isContentRecording = true;
    private bool _isSeries = false;
    private int _recordingPriority = 0;
    private int _paddingFrontInterval = -1, _paddingEndInterval = -1;
    private int _numberOfEpisodesToKeep = Int32.MaxValue; //all
    private QualityType _recordingQuality = QualityType.NotSet;
    private List<long> _listCanceledEpisodes = new List<long>();
    private bool _isAnnouncementSend = false;
    private DateTime _keepUntilDate = DateTime.MaxValue;
    private TVRecorded.KeepMethod _keepMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;

    #endregion

    #region properties

    public DateTime KeepRecordingTill
    {
      get { return _keepUntilDate; }
      set { _keepUntilDate = value; }
    }

    public TVRecorded.KeepMethod KeepRecordingMethod
    {
      get { return _keepMethod; }
      set { _keepMethod = value; }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public TVRecording()
    {
      _isContentRecording = true;
    }

    public TVRecording(TVRecording rec)
    {
      _startTime = rec._startTime;
      _endTime = rec._endTime;
      _timeCanceled = rec._timeCanceled;
      _title = rec._title;
      _channelName = rec._channelName;
      _recordingType = rec._recordingType;
      _recordingId = rec._recordingId;
      _isContentRecording = rec._isContentRecording;
      _isSeries = rec._isSeries;
      _recordingPriority = rec._recordingPriority;
      _recordingQuality = rec._recordingQuality;
      _listCanceledEpisodes.Clear();
      _listCanceledEpisodes.AddRange(rec._listCanceledEpisodes);
      EpisodesToKeep = rec.EpisodesToKeep;
      PaddingFront = rec.PaddingFront;
      PaddingEnd = rec.PaddingEnd;
      _keepMethod = rec._keepMethod;
      _keepUntilDate = rec._keepUntilDate;
      _isAnnouncementSend = rec._isAnnouncementSend;
    }

    public int PaddingFront
    {
      get { return _paddingFrontInterval; }
      set { _paddingFrontInterval = value; }
    }

    public int PaddingEnd
    {
      get { return _paddingEndInterval; }
      set { _paddingEndInterval = value; }
    }

    public int PreRecord
    {
      get
      {
        if (_paddingFrontInterval == -1)
        {
          return PrePostRecord.Instance.DefaultPreRecord;
        }
        else if (_paddingFrontInterval == -2)
        {
          return 0;
        }
        else
        {
          return _paddingFrontInterval;
        }
      }
    }

    public int PostRecord
    {
      get
      {
        if (_paddingEndInterval == -1)
        {
          return PrePostRecord.Instance.DefaultPostRecord;
        }
        else if (_paddingEndInterval == -2)
        {
          return 0;
        }
        else
        {
          return _paddingEndInterval;
        }
      }
    }

    public bool IsAnnouncementSend
    {
      get { return _isAnnouncementSend; }
      set { _isAnnouncementSend = value; }
    }

    /// <summary>
    /// Property to get/set the recording type
    /// </summary>
    public RecordingType RecType
    {
      get { return _recordingType; }
      set { _recordingType = value; }
    }

    /// <summary>
    /// Property to get/set whether this recording is a content or reference recording
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Recording.IGraph"/>
    public bool IsContentRecording
    {
      get { return _isContentRecording; }
      set { _isContentRecording = value; }
    }

    public int EpisodesToKeep
    {
      get { return _numberOfEpisodesToKeep; }
      set { _numberOfEpisodesToKeep = value; }
    }

    /// <summary>
    /// Property to get/set the TV channel name on which the recording should be done
    /// </summary>
    public string Channel
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// Property to get/set the title of the program to record
    /// </summary>
    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }

    /// <summary>
    /// Property to get/set the starttime of the program to record in xmltv format: yyyymmddhhmmss
    /// </summary>
    public long Start
    {
      get { return _startTime; }
      set { _startTime = value; }
    }

    /// <summary>
    /// Property to get/set the endtime of the program to record in xmltv format: yyyymmddhhmmss
    /// </summary>
    public long End
    {
      get { return _endTime; }
      set { _endTime = value; }
    }

    /// <summary>
    /// Property to get/set the database id of this recording 
    /// </summary>
    public int ID
    {
      get { return _recordingId; }
      set { _recordingId = value; }
    }

    /// <summary>
    /// Property to get the  starttime of the program to record
    /// </summary>
    public DateTime StartTime
    {
      get { return Util.Utils.longtodate(_startTime); }
      set { _startTime = Util.Utils.datetolong(value); }
    }

    /// <summary>
    /// Property to get the endtime of the program to record
    /// </summary>
    public DateTime EndTime
    {
      get { return Util.Utils.longtodate(_endTime); }
      set { _endTime = Util.Utils.datetolong(value); }
    }

    #endregion

    #region methods

    /// <summary>
    /// Checks whether this recording is finished and can be deleted
    /// 
    /// </summary>
    /// <returns>true:Recording is finished can be deleted
    ///          false:Recording is not done yet, or needs to be done multiple times
    /// </returns>
    public bool IsDone()
    {
      if (_recordingType != RecordingType.Once)
      {
        return false;
      }
      if (DateTime.Now > EndTime)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Check is the recording should be started/running between the specified start/end time
    /// </summary>
    /// <param name="tStartTime">starttime</param>
    /// <param name="tEndTime">endtime</param>
    /// <returns>true if the recording should be running between starttime-endtime</returns>
    public bool RunningAt(DateTime tStartTime, DateTime tEndTime)
    {
      DateTime dtStart = StartTime;
      DateTime dtEnd = EndTime;

      bool bRunningAt = false;
      if (dtEnd >= tStartTime && dtEnd <= tEndTime)
      {
        bRunningAt = true;
      }
      if (dtStart >= tStartTime && dtStart <= tEndTime)
      {
        bRunningAt = true;
      }
      if (dtStart <= tStartTime && dtEnd >= tEndTime)
      {
        bRunningAt = true;
      }
      return bRunningAt;
    }

    /// <summary>
    /// Checks if the recording should record the specified tvprogram
    /// </summary>
    /// <param name="program">TVProgram to check</param>
    /// <returns>true if the specified tvprogram should be recorded</returns>
    /// <returns>filterCanceledRecordings (true/false)
    /// if true then  we'll return false if recording has been canceled for this program</returns>
    /// if false then we'll return true if recording has been not for this program</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public bool IsRecordingProgram(TVProgram program, bool filterCanceledRecordings)
    {
      switch (_recordingType)
      {
        case RecordingType.Once:
          {
            if (program.Start == Start && program.End == End && program.Channel == Channel)
            {
              if (filterCanceledRecordings && Canceled > 0)
              {
                return false;
              }
              return true;
            }
          }
          break;
        case RecordingType.EveryTimeOnEveryChannel:
          if (program.Title == Title)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime))
            {
              return false;
            }
            return true;
          }
          break;
        case RecordingType.EveryTimeOnThisChannel:
          if (program.Title == Title && program.Channel == Channel)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime))
            {
              return false;
            }
            return true;
          }
          break;
        case RecordingType.Daily:
          if (program.Channel == Channel)
          {
            int iHourProg = program.StartTime.Hour;
            int iMinProg = program.StartTime.Minute;
            if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
            {
              iHourProg = program.EndTime.Hour;
              iMinProg = program.EndTime.Minute;
              if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
              {
                if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime))
                {
                  return false;
                }
                return true;
              }
            }
          }
          break;
        case RecordingType.WeekDays:
          if (program.StartTime.DayOfWeek >= DayOfWeek.Monday && program.StartTime.DayOfWeek <= DayOfWeek.Friday)
          {
            if (program.Channel == Channel)
            {
              int iHourProg = program.StartTime.Hour;
              int iMinProg = program.StartTime.Minute;
              if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
              {
                iHourProg = program.EndTime.Hour;
                iMinProg = program.EndTime.Minute;
                if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime))
                  {
                    return false;
                  }
                  return true;
                }
              }
            }
          }
          break;
        case RecordingType.WeekEnds:
          if (program.StartTime.DayOfWeek == DayOfWeek.Saturday || program.StartTime.DayOfWeek == DayOfWeek.Sunday)
          {
            if (program.Channel == Channel)
            {
              int iHourProg = program.StartTime.Hour;
              int iMinProg = program.StartTime.Minute;
              if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
              {
                iHourProg = program.EndTime.Hour;
                iMinProg = program.EndTime.Minute;
                if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime))
                  {
                    return false;
                  }
                  return true;
                }
              }
            }
          }
          break;
        case RecordingType.Weekly:
          if (program.Channel == Channel)
          {
            int iHourProg = program.StartTime.Hour;
            int iMinProg = program.StartTime.Minute;
            if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
            {
              iHourProg = program.EndTime.Hour;
              iMinProg = program.EndTime.Minute;
              if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
              {
                if (StartTime.DayOfWeek == program.StartTime.DayOfWeek)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime))
                  {
                    return false;
                  }
                  return true;
                }
              }
            }
          }
          break;
      }
      return false;
    } //IsRecordingProgram(TVProgram program, bool filterCanceledRecordings)

    /// <summary>
    /// Given a periodic recording and a date, returns the next start and end
    /// dates starting from dtTime.
    /// </summary>
    /// <param name="dtTime">time</param>
    /// <param name="recording">TVRecording</param>
    /// <returns>dtStart and dtEnd</returns>
    private void GetNewRecordingDates(DateTime dtTime, TVRecording recording, ref DateTime dtStart, ref DateTime dtEnd)
    {
      TimeSpan tDiff;
      DateTime wEnd, actualdaytwelve;

      dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, recording.StartTime.Hour, recording.StartTime.Minute,
                             0);
      actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
      tDiff = recording.EndTime - recording.StartTime;
      // On recordings that span across midnight, correct the starting date so
      // after 12:00 we consider that the recording will start later that day,
      // before 12:00 we consider that the recording started the day before so a correction is applied
      wEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, recording.EndTime.Hour, recording.EndTime.Minute, 0);
      if ((wEnd < dtStart) && (dtTime < actualdaytwelve))
      {
        dtStart = dtStart.AddDays(-1);
        Log.Debug("TVRecording: Recording a program that started the day before, correction applied");
      }
      dtEnd = dtStart + tDiff;
      Log.Debug("TVRecording: GetNewRecordingDates (dtStart, dtEnd): {0}, {1}", dtStart.ToString(), dtEnd.ToString());
    }

    /// <summary>
    /// Overloaded procedure.
    /// </summary>
    private void GetNewRecordingDates(DateTime dtTime, TVProgram program, ref DateTime dtStart, ref DateTime dtEnd)
    {
      TimeSpan tDiff;
      DateTime wEnd, actualdaytwelve;

      dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, program.StartTime.Hour, program.StartTime.Minute, 0);
      actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
      tDiff = program.EndTime - program.StartTime;
      // On recordings that span across midnight, correct the starting date so
      // after 12:00 we consider that the recording will start later that day,
      // before 12:00 we consider that the recording started the day before so a correction is applied
      wEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, program.EndTime.Hour, program.EndTime.Minute, 0);
      if ((wEnd < dtStart) && (dtTime < actualdaytwelve))
      {
        dtStart = dtStart.AddDays(-1);
        Log.Debug("TVRecording: Recording a program that started the day before, correction applied");
      }
      dtEnd = dtStart + tDiff;
      Log.Debug("TVRecording: GetNewRecordingDates (dtStart, dtEnd):  {0}, {1}", dtStart.ToString(), dtEnd.ToString());
    }

    /// <summary>
    /// Checks whether the recording should be recording at the specified time including the pre/post intervals
    /// </summary>
    /// <param name="dtTime">time</param>
    /// <param name="TVProgram">TVProgram</param>
    /// <param name="iPreInterval">pre record interval</param>
    /// <param name="iPostInterval">post record interval</param>
    /// <returns>true if the recording should record</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public bool IsRecordingAtTime(DateTime dtTime, TVProgram currentProgram, int iPreInterval, int iPostInterval)
    {
      DateTime dtStart;
      DateTime dtEnd;
      DateTime actualdaytwelve;

      switch (RecType)
      {
          // record program just once
        case RecordingType.Once:
          if (dtTime >= this.StartTime.AddMinutes(-iPreInterval) && dtTime <= this.EndTime.AddMinutes(iPostInterval))
          {
            if (Canceled > 0)
            {
              return false;
            }
            return true;
          }
          break;

          // record program daily at same time & channel
        case RecordingType.Daily:
          //<--------- begin old code --------->
          // check if recording start/date time is correct
          //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
          //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
          // dtend < dtstart means that the program starts before 00:00 and ends after
          // We have to care that we will change of dayweek while recording
          // As the year/month/day values are used to adjust a periodic recording
          // we need to adjust the start/end values before testing the interval
          // this depends if "Now" is before of after midnight
          // so if now < 12:00 or now >12:00
          //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
          //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
          //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
          //<--------- end old code --------->

          //IG 2007-9-13: Use new function to get the new start/end dates
          dtStart = new DateTime();
          dtEnd = new DateTime();
          GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

          if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
          {
            // not canceled?
            if (IsSerieIsCanceled(currentProgram.StartTime))
            {
              return false;
            }
            return true;
          }
          break;

          // record program daily at same time & channel
        case RecordingType.WeekDays:
          //<--------- begin old code --------->
          // check if recording start/date time is correct
          //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
          //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
          //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
          //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
          //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
          //<--------- end old code --------->

          //IG 2007-9-13: Use new function to get the new start/end dates
          dtStart = new DateTime();
          dtEnd = new DateTime();
          GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

          if (dtStart.DayOfWeek >= DayOfWeek.Monday && dtStart.DayOfWeek <= DayOfWeek.Friday)
          {
            if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
            {
              // not canceled?
              if (IsSerieIsCanceled(currentProgram.StartTime))
              {
                return false;
              }
              return true;
            }
          }
          break;

        case RecordingType.WeekEnds:
          //<--------- begin old code --------->
          // check if recording start/date time is correct
          //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
          //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
          //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
          //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
          //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
          //<--------- end old code --------->

          //IG 2007-9-13: Use new function to get the new start/end dates
          dtStart = new DateTime();
          dtEnd = new DateTime();
          GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

          if (dtStart.DayOfWeek == DayOfWeek.Saturday || dtStart.DayOfWeek == DayOfWeek.Sunday)
          {
            if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
            {
              // not canceled?
              if (IsSerieIsCanceled(currentProgram.StartTime))
              {
                return false;
              }
              return true;
            }
          }
          break;
          // record program weekly at same time & channel
        case RecordingType.Weekly:
          // check if day of week of recording matches 
          // if the recording overlaps midnight and it's past midnight,
          // we gotta use the previous DayOfWeek
          DayOfWeek _dayOfWeek;
          actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
          if ((this.EndTime.Hour < this.StartTime.Hour) && (dtTime < actualdaytwelve))
          {
            DateTime _tempDate = dtTime;
            _tempDate = _tempDate.AddDays(-1);
            _dayOfWeek = _tempDate.DayOfWeek;
          }
          else
          {
            _dayOfWeek = dtTime.DayOfWeek;
          }

          if (this.StartTime.DayOfWeek == _dayOfWeek)
          {
            //<--------- begin old code --------->
            // check if start/end time of recording is correct
            //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
            //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
            //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
            //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
            //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
            //<--------- end old code --------->

            //IG 2007-8-9: get next date for this weekly recording
            dtStart = new DateTime();
            dtEnd = new DateTime();
            GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

            if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
            {
              // not canceled?
              if (IsSerieIsCanceled(currentProgram.StartTime))
              {
                return false;
              }
              return true;
            }
          }
          break;

          //record program everywhere
        case RecordingType.EveryTimeOnEveryChannel:
          if (currentProgram == null)
          {
            return false; // we need a program
          }
          if (currentProgram.Title == this.Title) // check title
          {
            // check program time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) &&
                dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval))
            {
              // not canceled?
              if (IsSerieIsCanceled(currentProgram.StartTime))
              {
                return false;
              }
              return true;
            }
          }
          break;

          //record program on this channel
        case RecordingType.EveryTimeOnThisChannel:
          if (currentProgram == null)
          {
            return false; // we need a channel
          }

          // check channel & title
          if (currentProgram.Title == this.Title && currentProgram.Channel == this.Channel)
          {
            // check time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) &&
                dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval))
            {
              // not canceled?
              if (IsSerieIsCanceled(currentProgram.StartTime))
              {
                return false;
              }
              return true;
            }
          }
          break;
      }
      return false;
    }

    /// <summary>
    /// Checks whether the recording should record the specified TVProgram
    /// at the specified time including the pre/post intervals
    /// </summary>
    /// <param name="dtTime">time</param>
    /// <param name="TVProgram">TVProgram</param>
    /// <param name="iPreInterval">pre record interval</param>
    /// <param name="iPostInterval">post record interval</param>
    /// <returns>true if the recording should record</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public bool IsRecordingProgramAtTime(DateTime dtTime, TVProgram currentProgram, int iPreInterval, int iPostInterval)
    {
      DateTime dtStart;
      DateTime dtEnd;
      DateTime actualdaytwelve;

      switch (RecType)
      {
          // record program just once
        case RecordingType.Once:
          if (dtTime >= this.StartTime.AddMinutes(-iPreInterval) && dtTime <= this.EndTime.AddMinutes(iPostInterval))
          {
            if (Canceled > 0)
            {
              return false;
            }

            if (currentProgram != null)
            {
              if (currentProgram.Channel != this.Channel)
              {
                return false;
              }
              if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) &&
                  dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval))
              {
                return true;
              }
              return false;
            }
            string strManual = GUILocalizeStrings.Get(413);
            if (this.Title.Length == 0 || String.Compare(this.Title, strManual, true) == 0)
            {
              return true;
            }

            strManual = GUILocalizeStrings.Get(736);
            if (this.Title.Length == 0 || String.Compare(this.Title, strManual, true) == 0)
            {
              return true;
            }
            if (this.Title == "unknown")
            {
              return true;
            }
            return false;
          }
          break;

          // record program daily at same time & channel
        case RecordingType.WeekDays:
          if (currentProgram == null)
          {
            return false; //we need a program
          }
          if (currentProgram.Channel == this.Channel) //check channel is correct
          {
            //<--------- begin old code --------->
            // check if program start/date time is correct
            //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.StartTime.Hour, currentProgram.StartTime.Minute, 0);
            //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.EndTime.Hour, currentProgram.EndTime.Minute, 0);
            //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
            //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
            //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
            //<--------- end old code --------->

            //IG 2007-9-13: Use new function to get the new start/end dates
            dtStart = new DateTime();
            dtEnd = new DateTime();
            GetNewRecordingDates(dtTime, currentProgram, ref dtStart, ref dtEnd);

            if (dtStart.DayOfWeek >= DayOfWeek.Monday && dtStart.DayOfWeek <= DayOfWeek.Friday)
            {
              if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
              {
                //<--------- begin old code --------->
                // check if recording start/date time is correct
                //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
                //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
                //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
                //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
                //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
                //<--------- end old code --------->

                //IG 2007-9-13: Use new function to get the new start/end dates
                GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

                if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
                {
                  // not canceled?
                  if (IsSerieIsCanceled(currentProgram.StartTime))
                  {
                    return false;
                  }
                  return true;
                }
              }
            }
          }
          break;
          // record program on the weekend at same time & channel
        case RecordingType.WeekEnds:
          if (currentProgram == null)
          {
            return false; //we need a program
          }
          if (currentProgram.Channel == this.Channel) //check channel is correct
          {
            //<--------- begin old code --------->
            // check if program start/date time is correct
            //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.StartTime.Hour, currentProgram.StartTime.Minute, 0);
            //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.EndTime.Hour, currentProgram.EndTime.Minute, 0);
            //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
            //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
            //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
            //<--------- end old code --------->

            //IG 2007-9-13: Use new function to get the new start/end dates
            dtStart = new DateTime();
            dtEnd = new DateTime();
            GetNewRecordingDates(dtTime, currentProgram, ref dtStart, ref dtEnd);

            if (dtStart.DayOfWeek == DayOfWeek.Saturday || dtStart.DayOfWeek == DayOfWeek.Sunday)
            {
              if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
              {
                //<--------- begin old code --------->
                // check if recording start/date time is correct
                //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
                //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
                //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
                //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
                //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
                //<--------- end old code --------->

                //IG 2007-9-13: Use new function to get the new start/end dates
                GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

                if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
                {
                  // not canceled?
                  if (IsSerieIsCanceled(currentProgram.StartTime))
                  {
                    return false;
                  }
                  return true;
                }
              }
            }
          }
          break;
          // record program daily at same time & channel
        case RecordingType.Daily:
          if (currentProgram == null)
          {
            return false; //we need a program
          }
          if (currentProgram.Channel == this.Channel) //check channel is correct
          {
            //<--------- begin old code --------->
            // check if program start/date time is correct
            //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.StartTime.Hour, currentProgram.StartTime.Minute, 0);
            //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.EndTime.Hour, currentProgram.EndTime.Minute, 0);
            //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
            //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
            //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
            //<--------- end old code --------->

            //IG 2007-9-13: Use new function to get the new start/end dates
            dtStart = new DateTime();
            dtEnd = new DateTime();
            GetNewRecordingDates(dtTime, currentProgram, ref dtStart, ref dtEnd);

            if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
            {
              //<--------- begin old code --------->
              // check if recording start/date time is correct
              //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
              //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
              //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
              //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
              //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
              //<--------- end old code --------->

              //IG 2007-9-13: Use new function to get the new start/end dates
              GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

              if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
              {
                // not canceled?
                if (IsSerieIsCanceled(currentProgram.StartTime))
                {
                  return false;
                }
                return true;
              }
            }
          }
          break;

          // record program weekly at same time & channel
        case RecordingType.Weekly:
          if (currentProgram == null)
          {
            return false; // we need a program
          }
          if (currentProgram.Channel == this.Channel) // check if channel is correct
          {
            // check if day of week of recording matches 
            // if the recording overlaps midnight and it's past midnight,
            // we gotta use the previous DayOfWeek
            DayOfWeek _dayOfWeek;
            actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
            if ((this.EndTime.Hour < this.StartTime.Hour) && (dtTime < actualdaytwelve))
            {
              DateTime _tempDate = dtTime;
              _tempDate = _tempDate.AddDays(-1);
              _dayOfWeek = _tempDate.DayOfWeek;
            }
            else
            {
              _dayOfWeek = dtTime.DayOfWeek;
            }

            if (currentProgram.StartTime.DayOfWeek == _dayOfWeek)
            {
              //<--------- begin old code --------->
              // check if start/end time of program is correct
              //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.StartTime.Hour, currentProgram.StartTime.Minute, 0);
              //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, currentProgram.EndTime.Hour, currentProgram.EndTime.Minute, 0);
              //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
              //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
              //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
              //<--------- end old code --------->

              //IG 2007-8-9: get next date for this weekly recording
              dtStart = new DateTime();
              dtEnd = new DateTime();
              GetNewRecordingDates(dtTime, currentProgram, ref dtStart, ref dtEnd);

              if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
              {
                // check if day of week of recording matches 
                if (this.StartTime.DayOfWeek == dtTime.DayOfWeek)
                {
                  //<--------- begin old code --------->
                  // check if start/end time of recording is correct
                  //dtStart = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.StartTime.Hour, this.StartTime.Minute, 0);
                  //dtEnd = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, this.EndTime.Hour, this.EndTime.Minute, 0);
                  //actualdaytwelve = new DateTime(dtTime.Year, dtTime.Month, dtTime.Day, 12, 0, 0);
                  //if (dtEnd < dtStart && dtTime > actualdaytwelve) { dtEnd = dtEnd.AddDays(1); }
                  //if (dtEnd < dtStart && dtTime < actualdaytwelve) { dtStart = dtStart.AddDays(-1); }
                  //<--------- end old code --------->
                  //IG 2007-8-9: get next date for this weekly recording
                  GetNewRecordingDates(dtTime, this, ref dtStart, ref dtEnd);

                  if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval))
                  {
                    // not canceled?
                    if (IsSerieIsCanceled(currentProgram.StartTime))
                    {
                      return false;
                    }
                    return true;
                  }
                }
              }
            }
          }
          break;

          //record program everywhere
        case RecordingType.EveryTimeOnEveryChannel:
          if (currentProgram == null)
          {
            return false; // we need a program
          }
          if (currentProgram.Title == this.Title) // check title
          {
            // check program time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) &&
                dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval))
            {
              // not canceled?
              if (IsSerieIsCanceled(currentProgram.StartTime))
              {
                return false;
              }
              return true;
            }
          }
          break;

          //record program on this channel
        case RecordingType.EveryTimeOnThisChannel:
          if (currentProgram == null)
          {
            return false; // we need a channel
          }

          // check channel & title
          if (currentProgram.Title == this.Title && currentProgram.Channel == this.Channel)
          {
            // check time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) &&
                dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval))
            {
              // not canceled?
              if (IsSerieIsCanceled(currentProgram.StartTime))
              {
                return false;
              }
              return true;
            }
          }
          break;
      }
      return false;
    }

    /// <summary>
    /// Returns a string describing the recording
    /// </summary>
    /// <returns>Returns a string describing the recording</returns>
    public override string ToString()
    {
      string strLine = String.Empty;
      switch (RecType)
      {
        case RecordingType.Once:
          strLine = String.Format("Record once {0} on {1} from {2} {3} - {4} {5}",
                                  this.Title, this.Channel,
                                  StartTime.ToShortDateString(),
                                  StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  EndTime.ToShortDateString(),
                                  EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          break;

        case RecordingType.WeekDays:
          strLine = String.Format("{0} on {1} {2}-{3} from {4} - {5}",
                                  this.Title, this.Channel,
                                  GUILocalizeStrings.Get(657), //657=Mon
                                  GUILocalizeStrings.Get(661), //661=Fri
                                  StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          break;

        case RecordingType.WeekEnds:
          strLine = String.Format("{0} on {1} {2}-{3} from {4} - {5}",
                                  this.Title, this.Channel,
                                  GUILocalizeStrings.Get(662), //662=Sat
                                  GUILocalizeStrings.Get(663), //663=Sun
                                  StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          break;

        case RecordingType.Daily:
          strLine = String.Format("Record daily {0} on {1} from {2} - {3}",
                                  this.Title, this.Channel,
                                  StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          break;
        case RecordingType.Weekly:
          string day;
          switch (StartTime.DayOfWeek)
          {
            case DayOfWeek.Monday:
              day = GUILocalizeStrings.Get(11);
              break;
            case DayOfWeek.Tuesday:
              day = GUILocalizeStrings.Get(12);
              break;
            case DayOfWeek.Wednesday:
              day = GUILocalizeStrings.Get(13);
              break;
            case DayOfWeek.Thursday:
              day = GUILocalizeStrings.Get(14);
              break;
            case DayOfWeek.Friday:
              day = GUILocalizeStrings.Get(15);
              break;
            case DayOfWeek.Saturday:
              day = GUILocalizeStrings.Get(16);
              break;
            default:
              day = GUILocalizeStrings.Get(17);
              break;
          }
          strLine = String.Format("Record {0} on {1} every {2} from {3} - {4}",
                                  this.Title, this.Channel,
                                  day,
                                  StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          break;

        case RecordingType.EveryTimeOnEveryChannel:
          strLine = String.Format("Record {0} every time on any channel", Title);
          break;

        case RecordingType.EveryTimeOnThisChannel:
          strLine = String.Format("Record {0} every time on {1}", Title, Channel);
          break;
      }
      return strLine;
    }

    /// <summary>
    /// Property to get/set the time the recording was canceled in xmltv format:yyyymmddhhmmss
    /// </summary>
    public long Canceled
    {
      get { return _timeCanceled; }
      set { _timeCanceled = value; }
    }

    /// <summary>
    /// Property to get the time the recording was canceled 
    /// </summary>
    public DateTime CanceledTime
    {
      get { return Util.Utils.longtodate(_timeCanceled); }
    }

    /// <summary>
    /// Property to get current status about the recording
    /// </summary>
    public RecordingStatus Status
    {
      get
      {
        if (IsDone())
        {
          return RecordingStatus.Finished;
        }
        if (Canceled > 0)
        {
          return RecordingStatus.Canceled;
        }
        return RecordingStatus.Waiting;
      }
    }

    /// <summary>
    /// Property indicating if this recording belongs to a tv series or not
    /// </summary>
    public bool Series
    {
      get { return (this.RecType != RecordingType.Once); }
      set { _isSeries = value; }
    }


    /// <summary>
    /// Priority of this recording (1-10) where 1=lowest, 10=highest
    /// </summary>
    public int Priority
    {
      get { return _recordingPriority; }
      set { _recordingPriority = value; }
    }

    /// <summary>
    /// quality of this recording
    /// </summary>
    public QualityType Quality
    {
      get { return _recordingQuality; }
      set { _recordingQuality = value; }
    }

    public void CancelSerie(long datetime)
    {
      _listCanceledEpisodes.Add(datetime);
    }

    public List<long> CanceledSeries
    {
      get { return _listCanceledEpisodes; }
    }

    public void UnCancelSerie(DateTime datetime)
    {
      long dtProgram = Util.Utils.datetolong(datetime);
      foreach (long dtCanceled in _listCanceledEpisodes)
      {
        if (dtCanceled == dtProgram)
        {
          _listCanceledEpisodes.Remove(dtCanceled);
          return;
        }
      }
      return;
    }

    public bool IsSerieIsCanceled(DateTime datetime)
    {
      long dtProgram = Util.Utils.datetolong(datetime);
      foreach (long dtCanceled in _listCanceledEpisodes)
      {
        if (dtCanceled == dtProgram)
        {
          return true;
        }
      }
      return false;
    }

    public void SetProperties(TVProgram prog)
    {
      GUIPropertyManager.SetProperty("#TV.Scheduled.Title", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
      GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", String.Empty);

      string strTime = String.Format("{0} {1} - {2}",
                                     Util.Utils.GetShortDayString(StartTime),
                                     StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                     EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.Scheduled.Title", Title);
      GUIPropertyManager.SetProperty("#TV.Scheduled.Time", strTime);
      if (prog != null)
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Description", prog.Description);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", prog.Genre);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", prog.Channel);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.Description", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Genre", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Scheduled.Channel", String.Empty);
      }


      string logo = Util.Utils.GetCoverArt(Thumbs.TVChannel, Channel);
      if (File.Exists(logo))
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", logo);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Scheduled.thumb", "defaultVideoBig.png");
      }
    }


    public bool DoesUseEpisodeManagement
    {
      get
      {
        if (RecType == RecordingType.Once)
        {
          return false;
        }
        if (EpisodesToKeep == Int32.MaxValue)
        {
          return false;
        }
        if (EpisodesToKeep < 1)
        {
          return false;
        }
        return true;
      }
    }

    #endregion
  }
}