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
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
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

    public static void SetGentleConfigFile()
    {
      try
      {
        NameValueCollection appSettings = ConfigurationManager.AppSettings;
        appSettings.Set("GentleConfigFile", Config.GetFile(Config.Dir.Config, "gentle.config"));
      }
      catch (Exception ex)
      {
        Log.Error("TVUtil.SetGentleConfigFile: Error occured while setting the gentle configuration store: {0}", ex);
        throw;
      }
    }

    public IList<Schedule> GetRecordingTimes(Schedule rec)
    {
      DateTime startTime = DateTime.Now;
      DateTime endTime = startTime.AddDays(_days);

      IList<Program> programs = Schedule.GetProgramsForSchedule(rec);
      IList<Schedule> recordings = recordings = AddProgramsToSchedulesList(rec, programs);
      return recordings;
    }

    private IList<Schedule> AddProgramsToSchedulesList(Schedule rec, IList<Program> programs)
    {
      IList<Schedule> recordings = new List<Schedule>();
      if (programs != null && programs.Count > 0)
      {
        foreach (Program prg in programs)
        {
          Schedule recNew = rec.Clone();          
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.StartTime = prg.StartTime;
          recNew.EndTime = prg.EndTime;
          recNew.IdChannel = prg.IdChannel;
          recNew.Series = true;
          recNew.IdParentSchedule = rec.IdSchedule;
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

    private static void UpdateCurrentProgramTitle(ref Schedule recNew)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      IList<Program> programs = layer.GetPrograms(recNew.ReferencedChannel(), recNew.StartTime, recNew.EndTime);
      if (programs != null && programs.Count > 0)
      {
        recNew.ProgramName = programs[0].Title;
      }
    }

    public static string GetDisplayTitle(Schedule schedule)
    {
      string displayname = "";
      Program program = Program.RetrieveByTitleTimesAndChannel(schedule.ProgramName, schedule.StartTime,
                                                       schedule.EndTime, schedule.IdChannel);

      //if we have found program details then use nicely formatted name
      if (program != null)
      {
        displayname = TVUtil.GetDisplayTitle(program);
      }
      else
      {
        displayname = schedule.ProgramName;
      }
      return displayname;
    }

    public static string GetDisplayTitle(Recording rec)
    {
      StringBuilder strBuilder = new StringBuilder();
      TitleDisplay(strBuilder, rec.Title, rec.EpisodeName, rec.SeriesNum, rec.EpisodeNum, rec.EpisodePart);

      return strBuilder.ToString();
    }

    public static string GetDisplayTitle(Program prog)
    {
      StringBuilder strBuilder = new StringBuilder();
      TitleDisplay(strBuilder, prog.Title, prog.EpisodeName, prog.SeriesNum, prog.EpisodeNum, prog.EpisodePart);
      return strBuilder.ToString();
    }

    ///
    /// Get Episode info
    /// Builds string by the following rules set by ShowEpisodeInfo
    /// 0: [None] (empty string)
    /// 1: seriesNum.episodeNum.episodePart
    /// 2: episodeName
    /// 3: seriesNum.episodeNum.episodePart episodeName
    public static void GetEpisodeInfo(StringBuilder strBuilder, string episodeName, string seriesNum, string episodeNum, string episodePart)
    {
      bool episodeInfoWritten = false;

      bool hasSeriesNum = !(String.IsNullOrEmpty(seriesNum)) && (ShowEpisodeInfo == 1 || ShowEpisodeInfo == 3);      
      bool hasEpisodeNum = !(String.IsNullOrEmpty(episodeNum)) && (ShowEpisodeInfo == 1 || ShowEpisodeInfo == 3);
      bool hasEpisodePart = !(String.IsNullOrEmpty(episodePart)) && (ShowEpisodeInfo == 1 || ShowEpisodeInfo == 3);
      bool hasEpisodeName = !(String.IsNullOrEmpty(episodeName)) && (ShowEpisodeInfo == 2 || ShowEpisodeInfo == 3);

      bool hasEpisodeInfo = (hasSeriesNum || hasEpisodeNum || hasEpisodePart || hasEpisodeName);

      if (hasEpisodeInfo && ShowEpisodeInfo != 0)
      {
        strBuilder.Append(" (");                
      }         

      if (ShowEpisodeInfo == 1 || ShowEpisodeInfo == 3)
      {
        if (hasSeriesNum)
        {
          strBuilder.Append(seriesNum.Trim());
          episodeInfoWritten = true;
        }
        if (hasEpisodeNum)
        {
          if (episodeInfoWritten)
          {
            strBuilder.Append(".");                        
          }
          strBuilder.Append(episodeNum.Trim());
          episodeInfoWritten = true;
        }
        if (hasEpisodePart)
        {
          if (episodeInfoWritten)
          {
            strBuilder.Append(".");            
          }
          strBuilder.Append(episodePart.Trim());
          episodeInfoWritten = true;
        }
      }
      
      if (hasEpisodeName)
      {
        if (episodeInfoWritten)
        {
          strBuilder.Append(" ");
        }
        strBuilder.Append(episodeName);        
      }

      if (hasEpisodeInfo && ShowEpisodeInfo !=0)
      {
        strBuilder.Append(")"); 
      }      
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
    public static void TitleDisplay(StringBuilder strBuilder, string title, string episodeName, string seriesNum, string episodeNum,
                                      string episodePart)
    {
      strBuilder.Append(title);
      GetEpisodeInfo(strBuilder, episodeName, seriesNum, episodeNum, episodePart);      
    }


    /// <summary>
    /// Create Timeshifting filename based on RTSP url, UNC or real path.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetFileNameForTimeshifting()
    {
      bool useRTSP = TVHome.UseRTSP();
      string fileName = "";

      if (!useRTSP) //SMB mode
      {
        bool fileExists = File.Exists(TVHome.Card.TimeShiftFileName);
        if (!fileExists)
          // fileName does not exist b/c it points to the local folder on the tvserver, which is ofcourse invalid on the tv client.
        {
          if (TVHome.TimeshiftingPath().Length > 0)
          {
            //use user defined timeshifting folder as either UNC or network drive
            fileName = Path.GetFileName(TVHome.Card.TimeShiftFileName);
            fileName = TVHome.TimeshiftingPath() + "\\" + fileName;
          }
          else // use automatic UNC path
          {
            fileName = TVHome.Card.TimeShiftFileName.Replace(":", "");
            fileName = "\\\\" + RemoteControl.HostName + "\\" + fileName;
          }
        }
        else
        {
          fileName = TVHome.Card.TimeShiftFileName;
        }
      }
      else //RTSP mode, get RTSP url
      {
        fileName = TVHome.Card.RTSPUrl;
      }

      return fileName;
    }

    /// <summary>
    /// Create Recording filename based on RTSP url, UNC or real path.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetFileNameForRecording(Recording rec)
    {
      bool useRTSP = TVHome.UseRTSP();
      string fileName = "";

      if (!useRTSP) //SMB mode
      {
        bool fileExists = File.Exists(rec.FileName);
        if (!fileExists)
          // fileName does not exist b/c it points to the local folder on the tvserver, which is ofcourse invalid on the tv client.
        {
          if (TVHome.RecordingPath().Length > 0)
          {
            //use user defined recording folder as either UNC or network drive
            fileName = Path.GetFileName(rec.FileName);
            string fileNameSimple = TVHome.RecordingPath() + "\\" + fileName;

            fileExists = File.Exists(fileNameSimple);

            if (!fileExists)
              //maybe file exist in folder, schedules recs often appear in folders, no way to intelligently determine this.
            {
              DirectoryInfo dirInfo = Directory.GetParent(rec.FileName);

              if (dirInfo != null)
              {
                string parentFolderName = dirInfo.Name;
                fileName = TVHome.RecordingPath() + "\\" + parentFolderName + "\\" + fileName;
              }
            }
            else
            {
              fileName = fileNameSimple;
            }
          }
          else // use automatic UNC path
          {
            fileName = rec.FileName.Replace(":", "");
            fileName = "\\\\" + RemoteControl.HostName + "\\" + fileName;
          }
        }
        else //file exists, return it.
        {
          fileName = rec.FileName;
        }
      }
      else //RTSP mode, get RTSP url for file.
      {
        fileName = TVHome.TvServer.GetStreamUrlForFileName(rec.IdRecording);
      }

      return fileName;
    }

    
    public static bool PlayRecording(Recording rec)
    {
      return PlayRecording(rec, 0);
    }

    public static bool PlayRecording(Recording rec, double startOffset)
    {
      return PlayRecording(rec, startOffset, g_Player.MediaType.Recording);
    }
    
    public static bool PlayRecording(Recording rec, double startOffset, g_Player.MediaType mediaType)
    {
      string fileName = GetFileNameForRecording(rec);

      bool useRTSP = TVHome.UseRTSP();
      string chapters = useRTSP ? TVHome.TvServer.GetChaptersForFileName(rec.IdRecording) : null;

      Log.Info("PlayRecording:{0} - using rtsp mode:{1}", fileName, useRTSP);
      if (g_Player.Play(fileName, mediaType, chapters))
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
          // 5 second margin is used that the TsReader wont stop playback right after it has been started
          double dTime = g_Player.Duration - 5;
          g_Player.SeekAbsolute(dTime);
        }
        
        TvRecorded.SetActiveRecording(rec);

        //populates recording metadata to g_player;
        g_Player.currentFileName = rec.FileName;
        g_Player.currentTitle = GetDisplayTitle(rec);
        g_Player.currentDescription = rec.Description;

        rec.TimesWatched++;
        rec.Persist();

        return true;
      }
      return false;
    }


    #region scheduler helper methods


    /// <summary>
    /// Deletes a single or a complete schedule.    
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndSchedQuietly(Schedule schedule, int idChannel)
    {
      DateTime canceledStartTime = schedule.StartTime;
      return (DeleteRecAndSchedQuietly(schedule, canceledStartTime, idChannel));
    }
    /// <summary>
    /// Deletes a single or a complete schedule.    
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <param name="canceledStartTime">start time of the schedule to cancel</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndSchedQuietly(Schedule schedule, DateTime canceledStartTime, int idChannel)
    {
      bool wasDeleted = false;
      if (IsValidSchedule(schedule))
      {   
        Schedule parentSchedule = null;
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);
        wasDeleted = StopRecAndDeleteSchedule(schedule, parentSchedule, idChannel, canceledStartTime);        
      }
      return wasDeleted;
    }

    
    /// <summary>
    /// Deletes a single or a complete schedule.    
    /// If the schedule is currently recording, then this is stopped also.
    /// The entire schedule will be deleted
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>        
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndEntireSchedQuietly(Schedule schedule)
    {
      DateTime canceledStartTime = schedule.StartTime;
      return (DeleteRecAndEntireSchedQuietly(schedule, canceledStartTime));      
    }

    /// <summary>
    /// Deletes a single or a complete schedule.    
    /// If the schedule is currently recording, then this is stopped also.
    /// The entire schedule will be deleted
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <param name="canceledStartTime">start time of the schedule to cancel</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndEntireSchedQuietly(Schedule schedule, DateTime canceledStartTime)
    {
      bool wasDeleted = false;
      if (IsValidSchedule(schedule))
      {
        Schedule parentSchedule = null;
        wasDeleted = StopRecAndDeleteEntireSchedule(schedule, parentSchedule, canceledStartTime);        
      }
      return wasDeleted;
    }



    /// <summary>
    /// Stops a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>        
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool StopRecAndSchedWithPrompt(Schedule schedule, int idChannel)
    {
      DateTime canceledStartTime = schedule.StartTime;
      return (StopRecAndSchedWithPrompt(schedule, canceledStartTime, idChannel));
    }

        /// <summary>
    /// Deletes a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>        
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndSchedWithPrompt(Schedule schedule, int idChannel)
    {
      DateTime canceledStartTime = schedule.StartTime;
      return (DeleteRecAndSchedWithPrompt(schedule, canceledStartTime, idChannel));
    }

    /// <summary>
    /// Deletes a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <param name="canceledStartTime">start time of the schedule to cancel</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool StopRecAndSchedWithPrompt(Schedule schedule, DateTime canceledStartTime, int idChannel)
    {
      bool wasDeleted = false;
      if (IsValidSchedule(schedule))
      {
        Schedule parentSchedule = null;
        bool confirmed = true;
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);
        confirmed = PromptStopRecording(schedule.IdSchedule);

        if (confirmed)
        {
          wasDeleted = StopRecAndDeleteSchedule(schedule, parentSchedule, idChannel, canceledStartTime);
        }
      }
      return wasDeleted;
    }

    /// <summary>
    /// Deletes a single or a complete schedule.
    /// The user is being prompted if the schedule is currently recording.
    /// If the schedule is currently recording, then this is stopped also.    
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <param name="canceledStartTime">start time of the schedule to cancel</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndSchedWithPrompt(Schedule schedule, Program prg)
    {      
      bool wasDeleted = false;
      if (IsValidSchedule(schedule))
      {                
        Schedule parentSchedule = null;
        bool confirmed = true;
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);
        confirmed = PromptDeleteRecording(schedule.IdSchedule, prg);

        if (confirmed)
        {
          if (prg != null)
          {
            DateTime canceledStartTime = schedule.GetSchedStartTimeForProg(prg);
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
    /// </summary>
    /// <param name="Schedule">schedule id to be deleted</param>    
    /// <param name="canceledStartTime">start time of the schedule to cancel</param>    
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndSchedWithPrompt(Schedule schedule, DateTime canceledStartTime, int idChannel)
    {
      bool wasDeleted = false;
      if (IsValidSchedule(schedule))
      {
        Program prg = Program.RetrieveByTitleAndTimes(schedule.ProgramName, schedule.StartTime, schedule.EndTime);
        wasDeleted = DeleteRecAndSchedWithPrompt(schedule, prg);
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
    /// <returns>true if the schedule was deleted, otherwise false</returns>
    public static bool DeleteRecAndEntireSchedWithPrompt(Schedule schedule)
    {
      DateTime canceledStartTime = schedule.StartTime;
      return (DeleteRecAndEntireSchedWithPrompt(schedule, canceledStartTime));          
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
        bool confirmed = true;
        GetParentAndSpawnSchedule(ref schedule, out parentSchedule);
        
        confirmed = PromptDeleteRecording(schedule.IdSchedule);        
        if (confirmed)
        {
          wasDeleted = StopRecAndDeleteEntireSchedule(schedule, parentSchedule, canceledStartTime);
        }        
      }
      return wasDeleted;
    }

    private static bool StopRecAndDeleteSchedule(Schedule schedule, Schedule parentSchedule, int idChannel, DateTime canceledStartTime)
    {
      bool wasCanceled = CancelEpisode(canceledStartTime, parentSchedule, idChannel);
      bool wasDeleted = false;
      if (canceledStartTime == schedule.StartTime)
      {
        bool isScheduleTypeOnce = IsScheduleTypeOnce(schedule.IdSchedule);

        wasDeleted = StopRecording(schedule);
        if (isScheduleTypeOnce && !wasDeleted)
        {
          wasDeleted = DeleteSchedule(schedule.IdSchedule);
        }        
      }
            
      TvServer server = new TvServer();
      server.OnNewSchedule();
      return wasDeleted || wasCanceled;
    }

    private static bool StopRecAndDeleteEntireSchedule(Schedule schedule, Schedule parentSchedule, DateTime canceledStartTime)
    {
      int idChannel = schedule.IdChannel;      
      CancelEpisode(canceledStartTime, parentSchedule, idChannel);
      TvServer server = new TvServer();
      bool wasRecStopped = StopRecording(schedule);            
      bool wasDeleted = DeleteEntireOrOnceSchedule(schedule, parentSchedule);              
                        
      server.OnNewSchedule();
      return wasRecStopped || wasDeleted;
    }

    private static bool IsScheduleTypeOnce (int IdSchedule)
    {
      Schedule schedule = Schedule.Retrieve(IdSchedule);
      bool isOnce = (schedule.ScheduleType == (int)ScheduleRecordingType.Once);
      return isOnce;
    }

    private static bool DeleteEntireOrOnceSchedule(Schedule schedule, Schedule parentSchedule)
    {
      //is the schedule recording, then stop it now.
      bool wasDeleted = false;
      try
      {
        bool isRec = TvDatabase.Schedule.IsScheduleRecording(schedule.IdSchedule);
        bool isOnce = IsScheduleTypeOnce(parentSchedule.IdSchedule);
                

        if (parentSchedule != null)
        {
          wasDeleted = DeleteSchedule(parentSchedule.IdSchedule);          
        }

        if (schedule != null)
        {
          wasDeleted = DeleteSchedule(schedule.IdSchedule);          
        } 
      }
      catch (Exception)
      {
        //consume ex
      }            
      return wasDeleted;
    }

    private static bool DeleteSchedule (int IdSchedule)
    {
      bool scheduleDeleted = false;
      if (IdSchedule > 0)
      {
        Schedule sched2del = Schedule.Retrieve(IdSchedule);
        if (sched2del != null)
        {
          sched2del.Delete();          
        }
        scheduleDeleted = true;
      }
      else
      {
        scheduleDeleted = false;
        throw new Exception("schedule id is invalid");
      }
      return scheduleDeleted;
    }

    private static bool CancelEpisode(DateTime cancelStartTime, Schedule scheduleToCancel, int idChannel)
    {
      bool episodeCanceled = false;

      if (scheduleToCancel != null)
      {
        //create the canceled schedule to prevent the schedule from restarting again.
        // we only create cancelled recordings on series type schedules      
        Schedule sched2cancel = Schedule.Retrieve(scheduleToCancel.IdSchedule);
        bool isOnce = IsScheduleTypeOnce(scheduleToCancel.IdSchedule);
        if (!isOnce)
        {
          DateTime cancel = cancelStartTime;
          int IdForScheduleToCancel = scheduleToCancel.IdSchedule;
          CanceledSchedule canceledSchedule = new CanceledSchedule(IdForScheduleToCancel, idChannel, cancel);
          canceledSchedule.Persist();
          episodeCanceled = true;
        }
      }
      return episodeCanceled;
    }          

    private static bool PromptStopRecording(int IdSchedule)
    {
      bool confirmed = false;
      bool isRec = TvDatabase.Schedule.IsScheduleRecording(IdSchedule);

      if (isRec)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo)
        {
          Log.Error("TVProgramInfo.DeleteRecordingPrompt: ERROR no GUIDialogYesNo found !!!!!!!!!!");
        }
        else
        {
          dlgYesNo.SetHeading(1449); // stop recording
          dlgYesNo.SetLine(1, 1450); // are you sure to stop recording?          

          string recordingTitle = GetRecordingTitle(IdSchedule);
          dlgYesNo.SetLine(2, recordingTitle); 

          dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
          confirmed = dlgYesNo.IsConfirmed;
        }
      }
      else
      {
        confirmed = true;
      }
      return confirmed;
    }

    private static string GetRecordingTitle(int IdSchedule) {
      string recordingTitle = "";          

      Schedule schedule = Schedule.Retrieve(IdSchedule);
      if (schedule != null)
      {
        Schedule spawnedSchedule = Schedule.RetrieveSpawnedSchedule(IdSchedule, schedule.StartTime);
        if (spawnedSchedule != null)
        {
          recordingTitle = TVUtil.GetDisplayTitle(Recording.ActiveRecording(spawnedSchedule.IdSchedule));
        }
        else
        {
          recordingTitle = TVUtil.GetDisplayTitle(Recording.ActiveRecording(IdSchedule));
        }
      }
      return recordingTitle;
    }

    private static bool PromptDeleteRecording(int IdSchedule)
    {
      bool confirmed = false;

      bool isRec = TvDatabase.Schedule.IsScheduleRecording(IdSchedule);

      if (isRec)
      {
        confirmed = SetupConfirmDelRecDialogue();
      }
      else
      {
        confirmed = true;
      }
      return confirmed;
    }

    private static bool SetupConfirmDelRecDialogue()
    {
      bool confirmed = false;
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        Log.Error("TVProgramInfo.DeleteRecordingPrompt: ERROR no GUIDialogYesNo found !!!!!!!!!!");
      }
      else
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
        confirmed = dlgYesNo.IsConfirmed;
      }
      return confirmed;
    }

    private static bool PromptDeleteRecording(int idSchedule, Program prg)
    {
      bool confirmed = false;
      bool isRec = false;
      if (prg != null)
      {
        isRec = TvDatabase.Schedule.IsScheduleRecording(idSchedule, prg);  
      }
      else
      {
        var tvServer = new TvServer();
        VirtualCard vCard;
        isRec = tvServer.IsRecordingSchedule(idSchedule, out vCard);
      }      

      if (isRec)
      {
        confirmed = SetupConfirmDelRecDialogue();        
      }
      else
      {
        confirmed = true;
      }
      return confirmed;
    }

    private static void GetParentAndSpawnSchedule(ref Schedule schedule, out Schedule parentSchedule)
    {
      parentSchedule = schedule.ReferencedSchedule();
      if (parentSchedule == null)
      {
        parentSchedule = schedule;
        Schedule spawn = Schedule.RetrieveSpawnedSchedule(parentSchedule.IdSchedule, parentSchedule.StartTime);
        if (spawn != null)
        {
          schedule = spawn;
        }
      }
    }

    private static bool IsValidSchedule(Schedule schedule)
    {
      if (schedule == null)
      {
        return false;
      }
      int scheduleId = schedule.IdSchedule;

      if (scheduleId < 1)
      {
        return false;
      }
      return true;
    }

    private static bool StopRecording(Schedule schedule)
    {
      bool stoppedRec = false;
      bool isRec = TvDatabase.Schedule.IsScheduleRecording(schedule.IdSchedule);
      if (isRec)
      {
        TvServer server = new TvServer();
        server.StopRecordingSchedule(schedule.IdSchedule);
        stoppedRec = true;
      }
      return stoppedRec;
    }

    #endregion
  }
}