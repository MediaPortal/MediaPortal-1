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
using System.IO;
using System.Text;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.TvPlugin.Recorded;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Log = Mediaportal.TV.Server.TVLibrary.Interfaces.Logging.Log;

namespace Mediaportal.TV.TvPlugin.Helper
{
  /// <summary>
  /// Helper class which can be used to determine which tv program is
  /// running at a specific time and date
  /// </summary>
  public class TVUtil
  {
    #region vars

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

    public static IList<Schedule> GetRecordingTimes(ScheduleBLL rec)
    {
      IList<Program> programs = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsForSchedule(rec.Entity);
      IList<Schedule> recordings = new List<Schedule>();
      if (programs != null && programs.Count > 0)
      {
        foreach (Program prg in programs)
        {
          Schedule recNew = ScheduleFactory.Clone(rec.Entity);
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.StartTime = prg.StartTime;
          recNew.EndTime = prg.EndTime;
          recNew.IdChannel = prg.IdChannel;
          recNew.Series = true;
          recNew.IdParentSchedule = rec.Entity.IdSchedule;
          recNew.ProgramName = prg.Title;

          if (rec.IsSerieIsCanceled(rec.GetSchedStartTimeForProg(prg)))
          {
            recNew.Canceled = recNew.StartTime;
          }
          
          recordings.Add(recNew);
        }
      }
      return recordings;
    }

    public static string GetDisplayTitle(Schedule schedule)
    {
      Program program = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByTitleTimesAndChannel(schedule.ProgramName, schedule.StartTime,
                                                                          schedule.EndTime, schedule.IdChannel);      
      if (program != null)
      {
        return GetDisplayTitle(program);
      }
      return schedule.ProgramName;
    }

    public static string GetDisplayTitle(Recording rec)
    {
      return TitleDisplay(rec.Title, rec.EpisodeName, rec.SeasonNumber, rec.EpisodeNumber, rec.EpisodePartNumber);
    }

    public static string GetDisplayTitle(Program prog)
    {
      return TitleDisplay(prog.Title, prog.EpisodeName, prog.SeasonNumber, prog.EpisodeNumber, prog.EpisodePartNumber);
    }

    /// Episode info depends on ShowEpisodeInfo configuration.
    /// 0: None (empty string).
    /// 1: [season number].[episode number].[episode part number]
    /// 2: [episode name]
    /// 3: [season number].[episode number].[episode part number] [episode name]
    public static string TitleDisplay(string title, string episodeName, int? seasonNumber, int? episodeNumber, int? episodePartNumber)
    {
      if (ShowEpisodeInfo == 0)
      {
        return title;
      }

      StringBuilder s = new StringBuilder(100);
      bool episodeInfoWritten = false;
      if (ShowEpisodeInfo == 1 || ShowEpisodeInfo == 3)
      {
        if (seasonNumber.HasValue)
        {
          s.Append(seasonNumber.ToString());
          episodeInfoWritten = true;
        }
        if (episodeNumber.HasValue)
        {
          if (episodeInfoWritten)
          {
            s.Append(".");
          }
          s.Append(episodeNumber.ToString());
          episodeInfoWritten = true;
        }
        if (episodePartNumber.HasValue)
        {
          if (episodeInfoWritten)
          {
            s.Append(".");
          }
          s.Append(episodePartNumber.ToString());
          episodeInfoWritten = true;
        }
      }
      
      if (!string.IsNullOrEmpty(episodeName) && (ShowEpisodeInfo == 2 || ShowEpisodeInfo == 3))
      {
        if (episodeInfoWritten)
        {
          s.Append(" ");
        }
        s.Append(episodeName);
        episodeInfoWritten = true;
      }

      if (episodeInfoWritten)
      {
        s.Insert(0, " (");
        s.Append(")"); 
      }
      return s.ToString();
    }

    public static bool PlayRecording(Recording rec, double startOffset = 0, g_Player.MediaType mediaType = g_Player.MediaType.Recording)
    {
      string fileName = "";
      string chapters = null;
      if (TVHome.UseRTSP()) //SMB mode
      {
        fileName = ServiceAgents.Instance.ControllerServiceAgent.GetRecordingUrl(rec.IdRecording);
        chapters = ServiceAgents.Instance.ControllerServiceAgent.GetRecordingChapters(rec.IdRecording);
      }
      else
      {
        fileName = GetRecordingFileName(rec);
      }

      Log.Info("PlayRecording:{0}", fileName);
      if (g_Player.Play(fileName, mediaType, chapters, false)) // Force to use TsReader if true it will use Movie Codec and Splitter
      {
        if (Utils.IsVideo(fileName) && !g_Player.IsRadio)
        {
          g_Player.ShowFullScreenWindow();
        }
        if (startOffset > 0)
        {
          g_Player.SeekAbsolute(startOffset);
        }
        else if (startOffset == -1)
        {
          // 5 second margin is used so that the TsReader wont stop playback right after it has been started
          double dTime = g_Player.Duration - 5;
          g_Player.SeekAbsolute(dTime);
        }
        
        TvRecorded.SetActiveRecording(rec);

        //populates recording metadata to g_player;
        g_Player.currentFileName = rec.FileName;
        g_Player.currentTitle = GetDisplayTitle(rec);
        g_Player.currentDescription = rec.Description;

        rec.WatchedCount++;
        ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(rec);
        return true;
      }
      return false;
    }

    private static string GetRecordingFileName(Recording rec)
    {
      if (File.Exists(rec.FileName))
      {
        return rec.FileName;
      }

      // fileName does not exist b/c it points to the local folder on the tvserver, which is ofcourse invalid on the tv client.
      string uncRecordingPath = TVHome.RecordingPath();
      if (uncRecordingPath.Length == 0)
      {
        return @"\\" + Path.Combine(ServiceAgents.Instance.Hostname, rec.FileName.Replace(":", ""));
      }

      // use user defined recording folder as either UNC or network drive
      string fileName = Path.GetFileName(rec.FileName);
      string fileNameSimple = Path.Combine(uncRecordingPath, fileName);
      if (File.Exists(fileNameSimple))
      {
        return fileNameSimple;
      }

      //maybe file exist in folder, schedules recs often appear in folders, no way to intelligently determine this.
      DirectoryInfo dirInfo = Directory.GetParent(rec.FileName);
      if (dirInfo == null)
      {
        Log.Warn("failed to confirm recording file existence");
        return rec.FileName;
      }

      fileName = Path.Combine(uncRecordingPath, dirInfo.Name, fileName);
      if (File.Exists(fileName))
      {
        return fileName;
      }

      //Get last foldername of RecordingPath
      string parentFolderNameRecording = string.Format("{0}{1}{0}", Path.DirectorySeparatorChar, Path.GetFileName(dirInfo.Name));

      //Search the last folder of the set recording path in var rec.FileName 
      int iPos = rec.FileName.IndexOf(parentFolderNameRecording);
      if (iPos != -1)
      {
        //We have found the last Folder of the set recording path in var rec.FileName 
        //Cut the first string (ussaly the TV Server Local Path) and remove the last Recording Folder from string
        fileName = rec.FileName.Substring(iPos).Replace(parentFolderNameRecording, "");
        fileName = Path.Combine(uncRecordingPath, fileName);
      }
      return fileName;
    }

    public static string GetChannelLogo(Channel channel)
    {
      if (channel != null)
      {
        if (channel.MediaType == (int)MediaType.Television)
        {
          return Utils.GetCoverArt(Thumbs.TVChannel, channel.Name);
        }
        else if (channel.MediaType == (int)MediaType.Radio)
        {
          return Utils.GetCoverArt(Thumbs.Radio, channel.Name);
        }
      }
      return string.Empty;
    }

    #region scheduler helper methods

    /// <summary>
    /// Deletes a single or a complete schedule.
    /// If the schedule is currently recording, then this is stopped also.
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>
    public static void DeleteRecAndSchedQuietly(Schedule schedule, int idChannel)
    {
      if (IsValidSchedule(schedule))
      {   
        Schedule parentSchedule = null;
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);
        StopRecAndDeleteSchedule(schedule, parentSchedule, idChannel, schedule.StartTime);        
      }
    }

    /// <summary>
    /// Stops a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule to be deleted</param>        
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool StopRecAndSchedWithPrompt(Schedule schedule, int idChannel)
    {
      if (IsValidSchedule(schedule))
      {
        Schedule parentSchedule = null;
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);

        if (ServiceAgents.Instance.ScheduleServiceAgent.IsScheduleRecording(schedule.IdSchedule))
        {
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null == dlgYesNo)
          {
            Log.Error("TVProgramInfo.DeleteRecordingPrompt: ERROR no GUIDialogYesNo found !!!!!!!!!!");
            return false;
          }

          dlgYesNo.SetHeading(1449); // stop recording
          dlgYesNo.SetLine(1, 1450); // are you sure to stop recording?
          dlgYesNo.SetLine(2, GetRecordingTitle(schedule));
          dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
          if (dlgYesNo.IsConfirmed)
          {
            return StopRecAndDeleteSchedule(schedule, parentSchedule, idChannel, schedule.StartTime);
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Deletes a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <param name="canceledStartTime">start time of the schedule to cancel</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndSchedWithPrompt(Schedule schedule, Program prg = null)
    {      
      bool wasDeleted = false;
      if (IsValidSchedule(schedule))
      {
        if (prg == null)
        {
          prg = ServiceAgents.Instance.ProgramServiceAgent.GetProgramByTitleAndTimes(schedule.ProgramName, schedule.StartTime, schedule.EndTime);
        }
        Schedule parentSchedule = null;
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);

        bool isRec = false;
        if (prg != null)
        {
          isRec = ServiceAgents.Instance.ScheduleServiceAgent.IsScheduleRecordingProgram(schedule.IdSchedule, prg.IdProgram);
        }
        else
        {
          isRec = ServiceAgents.Instance.ControllerServiceAgent.IsRecordingSchedule(schedule.IdSchedule);
        }

        if (isRec && SetupConfirmDelRecDialogue())
        {
          if (prg != null)
          {
            DateTime canceledStartTime = new ScheduleBLL(schedule).GetSchedStartTimeForProg(prg);
            int idChannel = prg.IdChannel;
            wasDeleted = StopRecAndDeleteSchedule(schedule, parentSchedule, idChannel, canceledStartTime);             
          }
          else
          {
            wasDeleted = StopRecording(schedule);
          }
        }
      }
      return wasDeleted;
    }  

    /// <summary>
    /// Deletes a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.
    /// The entire schedule will be deleted
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <param name="canceledStartTime">start time of the schedule to cancel</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndEntireSchedWithPrompt(Schedule schedule, DateTime canceledStartTime)
    {
      bool wasDeleted = false;
      if (IsValidSchedule(schedule))
      {        
        Schedule parentSchedule = null;        
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);

        if (ServiceAgents.Instance.ScheduleServiceAgent.IsScheduleRecording(schedule.IdSchedule) && SetupConfirmDelRecDialogue())
        {
          wasDeleted |= StopRecording(schedule);
          if (parentSchedule != null)
          {
            ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(parentSchedule.IdSchedule);
            wasDeleted = true;
          }
          if (schedule != null)
          {
            ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(schedule.IdSchedule);
            wasDeleted = true;
          }
          ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
        }
      }
      return wasDeleted;
    }

    private static bool StopRecAndDeleteSchedule(Schedule schedule, Schedule parentSchedule, int idChannel, DateTime canceledStartTime)
    {
      bool wasCanceled = false;
      if (parentSchedule != null && parentSchedule.ScheduleType != (int)ScheduleRecordingType.Once)
      {
        //create the canceled schedule to prevent the schedule from restarting again.
        // we only create cancelled recordings on series type schedules      
        CanceledSchedule canceledSchedule = CanceledScheduleFactory.CreateCanceledSchedule(parentSchedule.IdSchedule, idChannel, canceledStartTime);
        ServiceAgents.Instance.CanceledScheduleServiceAgent.SaveCanceledSchedule(canceledSchedule);
        wasCanceled = true;
      }

      bool wasDeleted = false;
      if (canceledStartTime == schedule.StartTime && schedule.ScheduleType == (int)ScheduleRecordingType.Once && !StopRecording(schedule))
      {
        ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(schedule.IdSchedule);
        wasDeleted = true;
      }

      ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
      return wasDeleted || wasCanceled;
    }

    private static string GetRecordingTitle(Schedule schedule)
    {
      if (schedule != null)
      {
        Schedule spawnedSchedule = ServiceAgents.Instance.ScheduleServiceAgent.RetrieveSpawnedSchedule(schedule.IdSchedule, schedule.StartTime);        
        if (spawnedSchedule != null)
        {
          return GetDisplayTitle(ServiceAgents.Instance.RecordingServiceAgent.GetActiveRecording(spawnedSchedule.IdSchedule));
        }
        return GetDisplayTitle(ServiceAgents.Instance.RecordingServiceAgent.GetActiveRecording(schedule.IdSchedule));
      }
      return string.Empty;
    }

    private static bool SetupConfirmDelRecDialogue()
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        Log.Error("TVProgramInfo.DeleteRecordingPrompt: ERROR no GUIDialogYesNo found !!!!!!!!!!");
        return false;
      }

      dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
      dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
      dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
      return dlgYesNo.IsConfirmed;
    }

    private static void GetParentAndSpawnSchedule(ref Schedule schedule, out Schedule parentSchedule)
    {
      parentSchedule = schedule.ParentSchedule;
      if (parentSchedule == null)
      {
        parentSchedule = schedule;
        Schedule spawn = ServiceAgents.Instance.ScheduleServiceAgent.RetrieveSpawnedSchedule(schedule.IdSchedule, schedule.StartTime);
        if (spawn != null)
        {
          schedule = spawn;
        }
      }
    }

    private static bool IsValidSchedule(Schedule schedule)
    {
      return schedule != null && schedule.IdSchedule >= 1;
    }

    private static bool StopRecording(Schedule schedule)
    {
      if (ServiceAgents.Instance.ScheduleServiceAgent.IsScheduleRecording(schedule.IdSchedule))
      {
        ServiceAgents.Instance.ControllerServiceAgent.StopRecordingSchedule(schedule.IdSchedule);
        return true;
      }
      return false;
    }

    #endregion

    public static string GetCategory(ProgramCategory programCategory)
    {
      string category = GUILocalizeStrings.Get(2014); // unknown;
      if (programCategory != null && !string.IsNullOrEmpty(programCategory.Category))
      {
        category = programCategory.Category;
      }
      return category;
    }
  }
}