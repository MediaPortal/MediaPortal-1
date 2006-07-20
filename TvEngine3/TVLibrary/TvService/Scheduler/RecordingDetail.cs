using System;
using System.Collections.Generic;
using System.Text;

using TvLibrary.Log;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
using TvDatabase;
using TvControl;

namespace TvService
{
  /// <summary>
  /// class which holds all details about a schedule which is current being recorded
  /// </summary>
  public class RecordingDetail
  {
    #region variables
    Schedule _schedule;
    string _channel;
    string _fileName;
    DateTime _endTime;
    TvDatabase.Program _program;
    CardDetail _cardInfo;
    #endregion

    #region ctor
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="schedule">Schedule of this recording</param>
    /// <param name="channel">Channel on which the recording is done</param>
    /// <param name="endTime">Date/Time the recording should stop</param>
    public RecordingDetail(Schedule schedule, string channel, DateTime endTime)
    {
      _schedule = schedule;
      _channel = channel;
      _endTime = endTime;

      //find which program we are recording
      _program = schedule.Channel.CurrentProgram;
      if (_program != null)
      {
        //if we started recording before the schedule start time
        if (DateTime.Now < _schedule.StartTime)
        {
          // and the schedule endtime is past the current program endtime
          if (schedule.Channel.NextProgram != null)
          {
            if (_schedule.EndTime >= _program.EndTime)
            {
              //then we are not recording the current program, but the next one
              _program = schedule.Channel.NextProgram;
            }
          }
        }
      }

      //no program? then treat this as a manual recording
      if (_program == null)
      {
        _program = TvDatabase.Program.New();
        _program.Title = "manual";
        _program.StartTime = DateTime.Now;
        _program.EndTime = endTime;
        _program.Description = "";
        _program.Genre = "";
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// get/sets the CardInfo for this recording
    /// </summary>
    public CardDetail CardInfo
    {
      get
      {
        return _cardInfo;
      }
      set
      {
        _cardInfo = value;
      }
    }

    /// <summary>
    /// gets the Schedule belonging to this recording
    /// </summary>
    public Schedule Schedule
    {
      get
      {
        return _schedule;
      }
    }
    /// <summary>
    /// gets the Channel which is being recorded
    /// </summary>
    public string Channel
    {
      get
      {
        return _channel;
      }
    }
    /// <summary>
    /// Gets the filename of the recording
    /// </summary>
    public string FileName
    {
      get
      {
        return _fileName;
      }
    }
    /// <summary>
    /// Gets the date/time on which the recording should stop
    /// </summary>
    public DateTime EndTime
    {
      get
      {
        return _endTime;
      }
    }
    /// <summary>
    /// Gets the Program which is being recorded
    /// </summary>
    public TvDatabase.Program Program
    {
      get
      {
        return _program;
      }
    }

    /// <summary>
    /// Property which returns true when recording is busy
    /// and false when recording should be stopped
    /// </summary>
    public bool IsRecording
    {
      get
      {
        if (DateTime.Now >= EndTime.AddMinutes(_schedule.PostRecordInterval)) return false;
        return true;
      }
    }
    #endregion

    #region private members
    /// <summary>
    /// Create the filename for the recording 
    /// </summary>
    /// <param name="recordingPath"></param>
    public void MakeFileName(string recordingPath)
    {
      Setting setting;
      TvBusinessLayer layer = new TvBusinessLayer();
      if ((ScheduleRecordingType)_schedule.ScheduleType == ScheduleRecordingType.Once)
      {
        setting = layer.GetSetting("moviesformat", "%title%");
      }
      else
      {
        setting = layer.GetSetting("seriesformat", "%title%");
      }
      string strInput = "title%";
      if (setting != null)
      {
        if (setting.Value != null)
        {
          strInput = setting.Value;
        }
      }
      string recFileFormat = string.Empty;
      string recDirFormat = string.Empty;
      string subDirectory = string.Empty;
      string fullPath = recordingPath;
      string fileName = string.Empty;
      string recEngineExt = ".mpg";

      strInput = Utils.ReplaceTag(strInput, "%channel%", Utils.MakeFileName(_schedule.Channel.Name), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%title%", Utils.MakeFileName(Program.Title), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%name%", Utils.MakeFileName(Program.Episode), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%series%", Utils.MakeFileName(Program.SeriesNum), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%episode%", Utils.MakeFileName(Program.EpisodeNum), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%part%", Utils.MakeFileName(Program.EpisodePart), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%date%", Utils.MakeFileName(Program.StartTime.Date.ToShortDateString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%start%", Utils.MakeFileName(Program.StartTime.ToShortTimeString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%end%", Utils.MakeFileName(Program.EndTime.ToShortTimeString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%genre%", Utils.MakeFileName(Program.Genre), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startday%", Utils.MakeFileName(Program.StartTime.Day.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startmonth%", Utils.MakeFileName(Program.StartTime.Month.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startyear%", Utils.MakeFileName(Program.StartTime.Year.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%starthh%", Utils.MakeFileName(Program.StartTime.Hour.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startmm%", Utils.MakeFileName(Program.StartTime.Minute.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endday%", Utils.MakeFileName(Program.EndTime.Day.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endmonth%", Utils.MakeFileName(Program.EndTime.Month.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startyear%", Utils.MakeFileName(Program.EndTime.Year.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endhh%", Utils.MakeFileName(Program.EndTime.Hour.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endmm%", Utils.MakeFileName(Program.EndTime.Minute.ToString()), "unknown");

      int index = strInput.LastIndexOf('\\');
      if (index != -1)
      {
        subDirectory = strInput.Substring(0, index).Trim();
        fileName = strInput.Substring(index + 1).Trim();
      }
      else
        fileName = strInput.Trim();


      if (subDirectory != string.Empty)
      {
        subDirectory = Utils.RemoveTrailingSlash(subDirectory);
        subDirectory = Utils.MakeDirectoryPath(subDirectory);
        fullPath = recordingPath + "\\" + subDirectory;
        if (!System.IO.Directory.Exists(fullPath))
          System.IO.Directory.CreateDirectory(fullPath);
      }
      if (fileName == string.Empty)
      {
        DateTime dt = Program.StartTime;
        fileName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}",
                                  _schedule.Channel.Name, Program.Title,
                                  dt.Year, dt.Month, dt.Day,
                                  dt.Hour,
                                  dt.Minute,
                                  DateTime.Now.Minute, DateTime.Now.Second);
      }
      fileName = Utils.MakeFileName(fileName);
      if (System.IO.File.Exists(fullPath + "\\" + fileName + recEngineExt))
      {
        int i = 1;
        while (System.IO.File.Exists(fullPath + "\\" + fileName + "_" + i.ToString() + recEngineExt))
          ++i;
        fileName += "_" + i.ToString();
      }
      _fileName = fullPath + "\\" + fileName + recEngineExt;
    }
    #endregion
  }

}
