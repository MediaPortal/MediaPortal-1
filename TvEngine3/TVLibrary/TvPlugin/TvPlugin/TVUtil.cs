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
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using TvDatabase;
using MediaPortal.Profile;
using System.IO;
using TvControl;

namespace TvPlugin
{
  /// <summary>
  /// Helper class which can be used to determine which tv program is
  /// running at a specific time and date
  /// </summary>
  public class TVUtil
  {
    #region vars
    private int _days;
    private static int _showEpisodeInfo = -1;
    private static int ShowEpisodeInfo
    {
      get
      {
        if (_showEpisodeInfo == -1)
        {
          using (Settings xmlreader = new MPSettings())
          {
            _showEpisodeInfo = xmlreader.GetValueAsInt("mytv", "showEpisodeInfo", 0);
          }
        }
        return _showEpisodeInfo;
      }
    }
    #endregion

    public TVUtil()
    {
      _days = 15;
    }

    public TVUtil(int days)
    {
      _days = days;
    }

    #region IDisposable Members

    #endregion

    public List<Schedule> GetRecordingTimes(Schedule rec)
    {
      WeekEndTool weekEndTool = Setting.GetWeekEndTool();
      TvBusinessLayer layer = new TvBusinessLayer();
      List<Schedule> recordings = new List<Schedule>();

      DateTime dtDay = DateTime.Now;
      if (rec.ScheduleType == (int) ScheduleRecordingType.Once)
      {
        recordings.Add(rec);
        return recordings;
      }

      if (rec.ScheduleType == (int) ScheduleRecordingType.Daily)
      {
        for (int i = 0; i < _days; ++i)
        {
          Schedule recNew = rec.Clone();
          recNew.ScheduleType = (int) ScheduleRecordingType.Once;
          recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                          0);
          if (rec.EndTime.Day > rec.StartTime.Day)
          {
            dtDay = dtDay.AddDays(1);
          }
          recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
          if (rec.EndTime.Day > rec.StartTime.Day)
          {
            dtDay = dtDay.AddDays(-1);
          }
          recNew.Series = true;
          if (recNew.StartTime >= DateTime.Now)
          {
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.ScheduleType == (int) ScheduleRecordingType.WorkingDays)
      {
        for (int i = 0; i < _days; ++i)
        {
          if (weekEndTool.IsWorkingDay(dtDay.DayOfWeek))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int) ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                            0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.ScheduleType == (int) ScheduleRecordingType.Weekends)
      {
        IList<Program> progList;
        progList = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(_days), rec.ProgramName, rec.ReferencedChannel());

        foreach (Program prog in progList)
        {
          if ((rec.IsRecordingProgram(prog, false)) && (weekEndTool.IsWeekend(prog.StartTime.DayOfWeek)))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int) ScheduleRecordingType.Once;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;

            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
        }
        return recordings;
      }
      if (rec.ScheduleType == (int) ScheduleRecordingType.Weekly)
      {
        for (int i = 0; i < _days; ++i)
        {
          if (dtDay.DayOfWeek == rec.StartTime.DayOfWeek)
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int) ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                            0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }


      IList<Program> programs;
      if (rec.ScheduleType == (int) ScheduleRecordingType.EveryTimeOnThisChannel)
      {
        Log.Debug("get {0} {1} EveryTimeOnThisChannel", rec.ProgramName, rec.ReferencedChannel().DisplayName);
        programs = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(_days), rec.ProgramName, rec.ReferencedChannel());
      }
      else
      {
        Log.Debug("get {0} EveryTimeOnAllChannels", rec.ProgramName);

        programs = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(_days), rec.ProgramName, null);
      }
      foreach (Program prog in programs)
      {
        if (rec.IsRecordingProgram(prog, false))
        {
          Schedule recNew = rec.Clone();
          recNew.ScheduleType = (int) ScheduleRecordingType.Once;
          recNew.IdChannel = prog.IdChannel;
          recNew.StartTime = prog.StartTime;
          recNew.EndTime = prog.EndTime;
          recNew.Series = true;
          if (rec.IsSerieIsCanceled(recNew.StartTime))
          {
            recNew.Canceled = recNew.StartTime;
          }
          recordings.Add(recNew);
        }
      }
      return recordings;
    }
    public static string GetDisplayTitle(Recording rec)
    {
      return TitleDisplay(rec.Title,rec.EpisodeName,rec.SeriesNum,rec.EpisodeNum,rec.EpisodePart);
    }
    public static string GetDisplayTitle(Program prog)
    {
      return TitleDisplay(prog.Title, prog.EpisodeName, prog.SeriesNum, prog.EpisodeNum, prog.EpisodePart);
    }
    ///
    /// Get Episode info
    /// Builds string by the following rules set by ShowEpisodeInfo
    /// 0: [None] (empty string)
    /// 1: seriesNum.episodeNum.episodePart
    /// 2: episodeName
    /// 3: seriesNum.episodeNum.episodePart episodeName
    public static string GetEpisodeInfo(string episodeName, string seriesNum, string episodeNum, string episodePart)
    {
      string episodeInfo = "";
      if (ShowEpisodeInfo == 1 || ShowEpisodeInfo == 3)
      {
        if (!String.IsNullOrEmpty(seriesNum))
        {
          episodeInfo = seriesNum.Trim();
        }
        if (!String.IsNullOrEmpty(episodeNum))
        {
          if (episodeInfo.Length != 0)
          {
            episodeInfo = episodeInfo + "." + episodeNum.Trim();
          }
          else
          {
            episodeInfo = episodeNum.Trim();
          }
        }
        if (!String.IsNullOrEmpty(episodePart))
        {
          if (!String.IsNullOrEmpty(episodeInfo))
          {
            episodeInfo = episodeInfo + "." + episodePart.Trim();
          }
          else
          {
            episodeInfo = episodePart.Trim();
          }
        }
        if (ShowEpisodeInfo == 2 || ShowEpisodeInfo == 3)
        {
          if (!String.IsNullOrEmpty(episodeName))
          {
            if (!String.IsNullOrEmpty(episodeInfo))
            {
              episodeInfo = episodeInfo + " " + episodeName;
            }
            else
            {
              episodeInfo = episodeName;
            }
          }
        }
      }
      return episodeInfo;
    }
    /// <summary>
    ///  Create Display Title
    /// </summary>
    /// <param name="title"></param>
    /// <param name="episodeName"></param>
    /// <param name="seriesNum"></param>
    /// <param name="episodeNum"></param>
    /// <param name="episodePart"></param>
    /// <returns></returns>
    public static string TitleDisplay(string title, string episodeName, string seriesNum, string episodeNum, string episodePart)
    {
      string titleDisplay = GetEpisodeInfo(episodeName, seriesNum, episodeNum, episodePart);
      if (!String.IsNullOrEmpty(titleDisplay))
      {
        titleDisplay = title + " (" + titleDisplay + ")";
      }
      else
      {
        titleDisplay = title;
      }
      return titleDisplay;
    }

    //bool _isSeries = false;

    /// <summary>
    /// Create File Name
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetFileName(string fileName)
    {
      bool useRTSP = TVHome.UseRTSP();
      bool fileExists = File.Exists(fileName);

      if (!fileExists && !useRTSP) //singleseat
      {
        if (TVHome.RecordingPath().Length > 0)
        {
          string path = Path.GetDirectoryName(fileName);
          int index = path.IndexOf("\\");

          if (index == -1)
          {
            fileName = TVHome.RecordingPath() + "\\" + Path.GetFileName(fileName);
          }
          else
          {
            fileName = TVHome.RecordingPath() + path.Substring(index) + "\\" + Path.GetFileName(fileName);
          }
        }
        else
        {
          fileName = fileName.Replace(":", "");
          fileName = "\\\\" + RemoteControl.HostName + "\\" + fileName;
        }
      }
      return fileName;
    }
  }
}