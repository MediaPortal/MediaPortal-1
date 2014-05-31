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

#region usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardAllocation;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardReservation;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardReservation.Implementations;
using Mediaportal.TV.Server.TVLibrary.DiskManagement;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.CardReservation;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using RecordingManagement = Mediaportal.TV.Server.TVLibrary.DiskManagement.RecordingManagement;

#endregion

namespace Mediaportal.TV.Server.TVLibrary.Scheduler
{
  /// <summary>
  /// Scheduler class.
  /// This class will take care of recording all schedules in the database
  /// </summary>
  public class Scheduler
  {
    #region const

    private const int SCHEDULE_THREADING_TIMER_INTERVAL = 15000;

    #endregion

    #region imports

    [Flags]
    public enum EXECUTION_STATE : uint
    {
      ES_SYSTEM_REQUIRED = 0x00000001,
      ES_DISPLAY_REQUIRED = 0x00000002,
      // legacy flag should not be used
      // ES_USER_PRESENT   = 0x00000004,
      ES_CONTINUOUS = 0x80000000,
    }

    [DllImport("Kernel32.DLL", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE state);

    #endregion

    #region variables



    private EpisodeManagement _episodeManagement;
    private List<RecordingDetail> _recordingsInProgressList;
    private bool _createTagInfoXML;
    private bool _preventDuplicateEpisodes;
    private static int _preventDuplicateEpisodesKey;
    private Thread _schedulerThread = null;

    private static ManualResetEvent _evtSchedulerCtrl;
    private static ManualResetEvent _evtSchedulerWaitCtrl;

    /// <summary>
    /// Indicates how many free cards to try for recording
    /// </summary>
    private int _maxRecordFreeCardsToTry;

    #endregion

    #region ctor

    /// <summary>
    /// Constructor
    /// </summary>
    public Scheduler()
    {
      LoadSettings();
    }

    #endregion

    #region public members

    /// <summary>
    /// Resets the scheduler timer. This causes the scheduler to immediatly check
    /// if any schedule should be recorded
    /// </summary>
    public void ResetTimer()
    {
      _evtSchedulerWaitCtrl.Set();
    }

    /// <summary>
    /// Starts the scheduler
    /// </summary>
    public void Start()
    {
      this.LogDebug("Scheduler: started");

      ResetRecordingStates();

      _recordingsInProgressList = new List<RecordingDetail>();
      IList<Schedule> schedules = ScheduleManagement.ListAllSchedules(ScheduleIncludeRelationEnum.None);
      this.LogDebug("Scheduler: loaded {0} schedules", schedules.Count);
      StartSchedulerThread();
      new DiskManagement.DiskManagement();
      new RecordingManagement();
      _episodeManagement = new EpisodeManagement();
      HandleSleepMode();
    }

    /// <summary>
    /// Stops the scheduler
    /// </summary>
    public void Stop()
    {
      this.LogDebug("Scheduler: stopped");
      StopSchedulerThread();

      ResetRecordingStates();

      _episodeManagement = null;
      _recordingsInProgressList = new List<RecordingDetail>();
      HandleSleepMode();
    }

    /// <summary>
    /// This function checks whether something should be recorded at the given time.
    /// </summary>
    public bool IsTimeToRecord(DateTime currentTime)
    {
      IList<Schedule> schedules = ScheduleManagement.ListAllSchedules();
      foreach (Schedule schedule in schedules)
      {
        //if schedule has been canceled then do nothing

        if (schedule.Canceled != Schedule.MinSchedule)
          continue;

        //check if its time to record this schedule.
        RecordingDetail newRecording;
        if (IsTimeToRecord(schedule, currentTime, out newRecording))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// This function checks whether a specific schedule should be recorded at the given time.
    /// </summary>
    public bool IsTimeToRecord(Schedule schedule, DateTime currentTime)
    {
      //if schedule has been canceled then do nothing
      if (schedule.Canceled != Schedule.MinSchedule)
        return false;

      //check if its time to record this schedule.
      RecordingDetail newRecording;
      return IsTimeToRecord(schedule, currentTime, out newRecording);
    }

    /// <summary>
    /// Method which returns which card is currently recording the Schedule with the specified scheduleid
    /// </summary>
    /// <param name="idSchedule">database id of the schedule</param>
    /// <param name="card">virtual card</param>
    /// <returns>true if a card is recording the schedule, else false</returns>
    public bool IsRecordingSchedule(int idSchedule, out IVirtualCard card)
    {
      card = null;
      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
        if (rec.Schedule.Entity.IdSchedule == idSchedule)
        {
          IUser user = UserFactory.CreateSchedulerUser(rec.Schedule.Entity.IdSchedule, rec.CardInfo.Id);
          card = new VirtualCard(user);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Stop recording the Schedule with the specified schedule id
    /// </summary>
    /// <param name="idSchedule">database schedule id</param>
    public void StopRecordingSchedule(int idSchedule)
    {
      this.LogDebug("recList:StopRecordingSchedule {0}", idSchedule);
      RecordingDetail foundRec = _recordingsInProgressList.FirstOrDefault(rec => rec.Schedule.Entity.IdSchedule == idSchedule);

      if (foundRec != null)
      {
        StopRecord(foundRec);
      }
    }

    /// <summary>
    /// Method which returns the database schedule id for the card
    /// </summary>
    /// <param name="cardId">id of the card</param>
    /// <param name="channelId">Channel id</param>
    /// <returns>id of schedule the card is recording or -1 if its not recording</returns>
    public int GetRecordingScheduleForCard(int cardId, int channelId)
    {
      //reverse loop, since items can be removed during iteration
      for (int i = _recordingsInProgressList.Count - 1; i >= 0; i--)
      {
        RecordingDetail rec = _recordingsInProgressList[i];
        if (rec.CardInfo.Id == cardId && rec.Channel.IdChannel == channelId)
        {
          return rec.Schedule.Entity.IdSchedule;
        }
      }
      return -1;
    }

    /// <summary>
    /// Stops recording on the card specified
    /// </summary>
    /// <param name="cardId">id of the card</param>
    public void StopRecordingOnCard(int cardId)
    {
      this.LogDebug("recList:StopRecordingOnCard {0}", cardId);
      RecordingDetail foundRec = null;
      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
        if (rec.CardInfo.Id == cardId)
        {
          foundRec = rec;
          break;
        }
      }

      if (foundRec != null)
      {
        StopRecord(foundRec);
      }
    }

    /// <summary>
    /// Returns the number of active recordings
    /// </summary>
    public int ActiveRecordingsCount
    {
      get { return _recordingsInProgressList.Count; }
    }

    #endregion

    #region private members

    private void StartSchedulerThread()
    {
      _evtSchedulerCtrl = new ManualResetEvent(false);
      _evtSchedulerWaitCtrl = new ManualResetEvent(true);

      // setup scheduler thread.
      // thread already running, then leave it.
      if (_schedulerThread != null)
      {
        if (_schedulerThread.IsAlive)
        {
          return;
        }
      }
      this.LogDebug("Scheduler: thread started.");
      _schedulerThread = new Thread(SchedulerWorker);
      _schedulerThread.IsBackground = true;
      _schedulerThread.Name = "scheduler thread";
      _schedulerThread.Priority = ThreadPriority.Lowest;
      _schedulerThread.Start();
    }

    private void StopSchedulerThread()
    {
      if (_schedulerThread != null && _schedulerThread.IsAlive)
      {
        try
        {
          _evtSchedulerWaitCtrl.Set();
          _evtSchedulerCtrl.Set();
          _schedulerThread.Join();
          this.LogDebug("Scheduler: thread stopped.");
        }
        catch (Exception) { }
        finally
        {
          _evtSchedulerWaitCtrl.Close();
          _evtSchedulerCtrl.Close();
        }
      }
    }

    private void LoadSettings()
    {
      _createTagInfoXML = SettingsManagement.GetValue("createtaginfoxml", true);
      _preventDuplicateEpisodes = SettingsManagement.GetValue("PreventDuplicates", false);
      _preventDuplicateEpisodesKey = SettingsManagement.GetValue("EpisodeKey", 0);
      _maxRecordFreeCardsToTry = SettingsManagement.GetValue("recordMaxFreeCardsToTry", 0);
    }

    private static void ResetRecordingStates()
    {
      TVDatabase.TVBusinessLayer.RecordingManagement.ResetActiveRecordings();
    }

    private void SchedulerWorker()
    {
      try
      {
        bool firstRun = true;
        while (!_evtSchedulerCtrl.WaitOne(1))
        {
          bool resetTimer = _evtSchedulerWaitCtrl.WaitOne(SCHEDULE_THREADING_TIMER_INTERVAL);

          try
          {
            DoScheduleWork();
          }
          catch (Exception ex)
          {
            this.LogDebug("scheduler: SchedulerWorker inner exception {0}", ex);
          }
          finally
          {
            if (resetTimer || firstRun)
            {
              _evtSchedulerWaitCtrl.Reset();
            }
            firstRun = false;
          }
        }
        _evtSchedulerWaitCtrl.Set();
      }
      catch (Exception ex2)
      {
        this.LogDebug("scheduler: SchedulerWorker outer exception {0}", ex2);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void DoScheduleWork()
    {
      StopAnyDueRecordings();
      StartAnyDueRecordings();
      CheckAndDeleteOrphanedRecordings();
      CheckAndDeleteOrphanedOnceSchedules();
      HandleSleepMode();
    }

    private void CheckAndDeleteOrphanedOnceSchedules()
    {
      //only delete orphaned schedules when not recording.
      if (!IsRecordingsInProgress())
      {
        ScheduleManagement.DeleteOrphanedOnceSchedules();
      }
    }

    private static string CleanEpisodeTitle(string aEpisodeTitle)
    {
      try
      {
        string cleanedEpisode = aEpisodeTitle.Replace(" (LIVE)", String.Empty).Replace(" (Wdh.)", String.Empty);
        return cleanedEpisode.Trim();
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Scheduler: Could not cleanup episode title {0}", aEpisodeTitle);
        return aEpisodeTitle;
      }
    }

    private void CheckAndDeleteOrphanedRecordings()
    {
      List<IVirtualCard> vCards = ServiceManager.Instance.InternalControllerService.GetAllRecordingCards();

      foreach (VirtualCard vCard in vCards)
      {
        int schedId = vCard.RecordingScheduleId;
        if (schedId > 0)
        {
          Schedule sc = ScheduleManagement.GetSchedule(schedId);
          if (sc == null)
          {
            //seems like the schedule has disappeared  stop the recording also.
            this.LogDebug("Scheduler: Orphaned Recording found {0} - removing", schedId);
            StopRecordingSchedule(schedId);
          }
        }
      }
    }

    /// <summary>
    /// StartAnyDueRecordings() will start recording any schedule if its time todo so
    /// </summary>
    private void StartAnyDueRecordings()
    {
      IList<Schedule> schedules = ScheduleManagement.ListAllSchedules();
      foreach (Schedule schedule in schedules)
      {
        bool isScheduleReadyForRecording = IsScheduleReadyForRecording(schedule);

        if (isScheduleReadyForRecording)
        {
          //check if its time to record this schedule.
          RecordingDetail newRecording;
          DateTime now = DateTime.Now;
          if (IsTimeToRecord(schedule, now, out newRecording))
          {
            if (newRecording != null)
            {
              //yes - let's check whether this file is already present and therefore doesn't need to be recorded another time
              if (ShouldRecordEpisode(schedule.ScheduleType, newRecording))
              {
                StartRecord(newRecording);
              }
            }
            else
            {
              this.LogInfo("StartAnyDueRecordings: RecordingDetail was null");
            }
          }
        }
      }
    }

    private bool IsScheduleReadyForRecording(Schedule schedule)
    {
      bool isScheduleReadyForRecording = true;
      DateTime now = DateTime.Now;
      IVirtualCard card;
      ScheduleBLL scheduleBll = new ScheduleBLL(schedule);
      if (schedule.Canceled != Schedule.MinSchedule ||
          IsRecordingSchedule(schedule.IdSchedule, out card) ||
          scheduleBll.IsSerieIsCanceled(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)))
      {
        isScheduleReadyForRecording = false;
      }

      return isScheduleReadyForRecording;
    }

    private bool ShouldRecordEpisode(int scheduleType, RecordingDetail newRecording)
    {
      string currentEpisodeTitle = "";
      bool shouldRecordEpisode = true;

      //cleanup: remove EPG additions of Clickfinder plugin      
      try
      {
        // Allow user to turn this on or off in case of unreliable EPG
        if (_preventDuplicateEpisodes)
        {
          string currentEpisodeName = GetEpisodeName(newRecording);
          Program currentProgram = newRecording.Program.Entity;
          currentEpisodeTitle = CleanEpisodeTitle(currentProgram.Title);
          this.LogDebug("Scheduler: Check recordings for schedule {0}...", currentEpisodeTitle);
          // EPG needs to have episode information to distinguish between repeatings and new broadcasts
          if (HasEpisodeName(currentEpisodeName))
          {
            shouldRecordEpisode = ShouldRecordEpisodeBasedOnPastRecordings(newRecording, currentEpisodeName, currentProgram, currentEpisodeTitle);
          }
          else
          {
            // Check the type so we aren't logging too verbose on single runs
            if (scheduleType != (int)ScheduleRecordingType.Once)
            {
              this.LogInfo("Scheduler: No epsisode title found for schedule {0} - omitting repeating check.",
                       currentProgram.Title);
            }
          }
        }
      }
      catch (Exception ex1)
      {
        this.LogError("Scheduler: Error checking schedule {0} for repeatings {1}", currentEpisodeTitle, ex1.ToString());
      }
      return shouldRecordEpisode;
    }

    private bool ShouldRecordEpisodeBasedOnPastRecordings(RecordingDetail newRecording, string currentEpisodeName, Program currentProgram, string currentEpisodeTitle)
    {
      bool shouldRecordEpisode = true;
      IList<Recording> pastRecordings = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);
      foreach (Recording pastRecording in pastRecordings)
      {
        if (IsCurrentEpisodeTitleInPastEpisode(currentEpisodeTitle, pastRecording) && IsCurrentEpisodeNameInPastEpisode(currentEpisodeName, pastRecording))
        {
          Schedule schedule = newRecording.Schedule.Entity;
          if (IsIncompleteRecording(pastRecording, newRecording))
          {
            if (AlreadyHasValidRecordingFile(pastRecording.FileName))
            {
              shouldRecordEpisode = false;
              HandleOnceScheduleTypes(newRecording, currentEpisodeTitle, pastRecording, currentEpisodeName, schedule, currentProgram);
            }
            else
            {
              this.LogError(
                "Scheduler: Schedule {0} ({1}) has already been recorded but the file is invalid! Going to record again...",
                currentEpisodeTitle, currentEpisodeName);
            }
          }
          else
          {
            this.LogInfo(
              "Scheduler: Schedule {0} ({1}) had already been started - expect previous failure and try to resume...",
              currentEpisodeTitle, currentEpisodeName);
          }
        }
      }
      return shouldRecordEpisode;
    }

    private static bool IsIncompleteRecording(Recording rec, RecordingDetail newRec)
    {
      // How to handle "interrupted" recordings?
      // E.g. Windows reboot because of update installation: Previously the tvservice restarted to record the episode 
      // and simply took care of creating a unique filename.
      // Check if new program overlaps with existing recording
      // if so assume previous failure and recording needs to be resumed
      var startExisting = rec.StartTime;
      var endExisting = rec.EndTime;
      var startNew = newRec.Program.Entity.StartTime.AddMinutes(-newRec.Schedule.Entity.PreRecordInterval);
      var endNew = newRec.Program.Entity.EndTime.AddMinutes(newRec.Schedule.Entity.PostRecordInterval);

      TimeSpan tsNewRec = endNew - startNew;
      TimeSpan tsExistingRec = endExisting - startExisting;

      bool isOverlap = startNew < endExisting && endNew > startExisting;
      bool hasIncompleteDuration = (tsNewRec.Minutes != tsExistingRec.Minutes);
      bool isIncompleteRecording = hasIncompleteDuration && isOverlap;

      return isIncompleteRecording;
    }


    private static bool IsCurrentEpisodeTitleInPastEpisode(string currentEpisodeTitle, Recording pastRecording)
    {
      // Checking the record "title" itself to avoid unnecessary checks.
      // Furthermore some EPG sources could misuse the episode field for other, non-unique information
      string pastEpisodeTitle = CleanEpisodeTitle(pastRecording.Title);
      return pastEpisodeTitle.Equals(currentEpisodeTitle, StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool IsCurrentEpisodeNameInPastEpisode(string currentEpisodeName, Recording pastRecording)
    {
      //this.LogDebug("Scheduler: Found recordings of schedule {0} - checking episodes...", ToRecordTitle);
      // The schedule which is about to be recorded is already found on our disk
      string pastEpisodeName = GetPastEpisodeName(pastRecording);
      return pastEpisodeName.Equals(currentEpisodeName, StringComparison.CurrentCultureIgnoreCase);
    }

    private void HandleOnceScheduleTypes(RecordingDetail newRecording, string episodeTitle, Recording pastRecording,
                                         string episodeName, Schedule schedule, Program program)
    {
      // Handle schedules so TV Service won't try to re-schedule them every 15 seconds
      if ((ScheduleRecordingType)schedule.ScheduleType == ScheduleRecordingType.Once)
      {
        // One-off schedules can be spawned for some schedule types to record the actual episode
        // if this is the case then add a cancelled schedule for this episode against the parent
        int? parentScheduleId = schedule.IdParentSchedule;
        if (parentScheduleId > 0)
        {
          CancelSchedule(newRecording, parentScheduleId.GetValueOrDefault());
        }
        FireScheduleDeletedEvent(newRecording);
        ScheduleManagement.DeleteSchedule(newRecording.Schedule.Entity.IdSchedule);
      }
      else
      {
        CancelSchedule(newRecording, schedule.IdSchedule);
      }

      this.LogInfo("Scheduler: Schedule {0}-{1} ({2}) has already been recorded ({3}) - aborting...",
               program.StartTime.ToString(CultureInfo.InvariantCulture), episodeTitle, episodeName,
               pastRecording.StartTime.ToString(CultureInfo.InvariantCulture));
    }

    private void FireScheduleDeletedEvent(RecordingDetail newRecording)
    {
      IUser user = newRecording.User;
      ServiceManager.Instance.InternalControllerService.Fire(this,
        new TvServerEventArgs(TvServerEventType.ScheduleDeleted, new VirtualCard(user), (User)user, newRecording.Schedule.Entity.IdSchedule, -1));
    }

    private static bool AlreadyHasValidRecordingFile(string filename)
    {
      bool alreadyHasValidRecordingFile = false;
      // Make sure there's no 1KB file left over (e.g when card fails to tune to channel)
      try
      {
        var fi = new FileInfo(filename);
        alreadyHasValidRecordingFile = (fi.Length > 4096);
      }
      catch
      {
        //ignore
      }
      return alreadyHasValidRecordingFile;
    }

    private static string GetPastEpisodeName(Recording pastRecording)
    {
      string pastRecordEpisode;
      switch (_preventDuplicateEpisodesKey)
      {
        case 1: // Episode Number
          pastRecordEpisode = pastRecording.SeriesNum + "." + pastRecording.EpisodeNum + "." + pastRecording.EpisodePart;
          break;
        default: // 0 EpisodeName
          pastRecordEpisode = CleanEpisodeTitle(pastRecording.EpisodeName);
          break;
      }
      return pastRecordEpisode;
    }

    private static bool HasEpisodeName(string episodeName)
    {
      return !episodeName.Equals(String.Empty) && !episodeName.Equals("..");
    }

    private string GetEpisodeName(RecordingDetail newRecording)
    {
      string episodeName;
      switch (_preventDuplicateEpisodesKey)
      {
        case 1: // Episode Number
          episodeName = newRecording.Program.Entity.SeriesNum + "." + newRecording.Program.Entity.EpisodeNum + "." + newRecording.Program.Entity.EpisodePart;
          break;
        default: // Episode Name
          episodeName = CleanEpisodeTitle(newRecording.Program.Entity.EpisodeName);
          break;
      }
      return episodeName;
    }

    private void CancelSchedule(RecordingDetail newRecording, int scheduleId)
    {
      CanceledSchedule canceled = CanceledScheduleFactory.CreateCanceledSchedule(scheduleId, newRecording.Program.Entity.IdChannel, newRecording.Program.Entity.StartTime);
      CanceledScheduleManagement.SaveCanceledSchedule(canceled);
      _episodeManagement.OnScheduleEnded(newRecording.FileName, newRecording.Schedule.Entity, newRecording.Program.Entity);
    }

    /// <summary>
    /// StopAnyDueRecordings() will stop any recording which should be stopped
    /// </summary>
    private void StopAnyDueRecordings()
    {
      //reverse loop, since items can be removed during iteration
      for (int i = _recordingsInProgressList.Count - 1; i >= 0; i--)
      {
        RecordingDetail rec = _recordingsInProgressList[i];
        if (!rec.IsRecording)
        {
          StopRecord(rec);
        }
      }
    }


    /// <summary>
    /// Under vista we must disable the sleep timer when we're recording
    /// Otherwise vista may simple shutdown or suspend
    /// </summary>
    private void HandleSleepMode()
    {
      if (_recordingsInProgressList == null)
      {
        return;
      }

      if (IsRecordingsInProgress())
      {
        //reset the sleep timer
        SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
      }
    }

    private bool IsRecordingsInProgress()
    {
      return _recordingsInProgressList.Count > 0;
    }

    /// <summary>
    /// Method which checks if its time to record the schedule specified
    /// </summary>
    /// <param name="schedule">Schedule</param>
    /// <param name="currentTime">current Date/Time</param>
    /// <param name="newRecording">Recording detail which is used to further process the recording</param>
    /// <returns>true if schedule should be recorded now, else false</returns>
    private bool IsTimeToRecord(Schedule schedule, DateTime currentTime, out RecordingDetail newRecording)
    {
      bool isTimeToRecord = false;
      newRecording = null;
      ScheduleRecordingType type = (ScheduleRecordingType)schedule.ScheduleType;

      switch (type)
      {
        case ScheduleRecordingType.Once:
          newRecording = IsTimeToRecordOnce(schedule, currentTime, out isTimeToRecord);
          break;

        case ScheduleRecordingType.Daily:
          newRecording = IsTimeToRecordDaily(schedule, currentTime, out isTimeToRecord);
          break;

        case ScheduleRecordingType.Weekends:
          newRecording = IsTimeToRecordWeekends(schedule, currentTime, out isTimeToRecord);
          break;

        case ScheduleRecordingType.WorkingDays:
          newRecording = IsTimeToRecordWorkingDays(schedule, currentTime, out isTimeToRecord);
          break;

        case ScheduleRecordingType.Weekly:
          newRecording = IsTimeToRecordWeekly(schedule, currentTime, out isTimeToRecord);
          break;

        case ScheduleRecordingType.EveryTimeOnThisChannel:
          isTimeToRecord = IsTimeToRecordEveryTimeOnThisChannel(schedule, currentTime);
          break;

        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          isTimeToRecord = IsTimeToRecordEveryTimeOnEveryChannel(schedule);
          break;
        case ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
          isTimeToRecord = IsTimeToRecordWeeklyEveryTimeOnThisChannel(schedule, currentTime);
          break;
      }
      return isTimeToRecord;
    }

    private bool IsTimeToRecordWeeklyEveryTimeOnThisChannel(Schedule schedule, DateTime currentTime)
    {
      bool isTimeToRecord = false;
      Program current = ProgramManagement.GetProgramAt(currentTime.AddMinutes(schedule.PreRecordInterval), schedule.ProgramName);

      if (current != null)
      {
        // (currentTime.DayOfWeek == schedule.startTime.DayOfWeek)
        // this.LogDebug("Scheduler.cs WeeklyEveryTimeOnThisChannel: {0} {1} current.startTime.DayOfWeek == schedule.startTime.DayOfWeek {2} == {3}", schedule.programName, schedule.Channel.Name, current.startTime.DayOfWeek, schedule.startTime.DayOfWeek);
        if (current.StartTime.DayOfWeek == schedule.StartTime.DayOfWeek)
        {
          if (currentTime >= current.StartTime.AddMinutes(-schedule.PreRecordInterval) &&
              currentTime <= current.EndTime.AddMinutes(schedule.PostRecordInterval))
          {
            var scheduleBLL = new ScheduleBLL(schedule);
            if (!scheduleBLL.IsSerieIsCanceled(current.StartTime))
            {
              bool createSpawnedOnceSchedule = CreateSpawnedOnceSchedule(scheduleBLL.Entity, current);
              if (createSpawnedOnceSchedule)
              {
                ResetTimer(); //lets process the spawned once schedule at once.
              }
            }
          }
        }
      }
      return isTimeToRecord;
    }

    private bool IsTimeToRecordEveryTimeOnEveryChannel(Schedule schedule)
    {
      bool isTimeToRecord = false;
      bool createSpawnedOnceSchedule = false;

      IList<Program> programs = ProgramManagement.RetrieveCurrentRunningByTitle(schedule.ProgramName, schedule.PreRecordInterval, schedule.PostRecordInterval);
      var scheduleBLL = new ScheduleBLL(schedule);
      foreach (Program program in programs)
      {
        if (!scheduleBLL.IsSerieIsCanceled(program.StartTime))
        {
          if (CreateSpawnedOnceSchedule(scheduleBLL.Entity, program))
          {
            createSpawnedOnceSchedule = true;
          }
        }
      }
      if (createSpawnedOnceSchedule)
      {
        ResetTimer(); //lets process the spawned once schedule at once.
      }
      return isTimeToRecord;
    }

    private bool IsTimeToRecordEveryTimeOnThisChannel(Schedule schedule, DateTime currentTime)
    {
      bool isTimeToRecord = false;
      Program current = ProgramManagement.GetProgramAt(currentTime.AddMinutes(schedule.PreRecordInterval), schedule.ProgramName);

      if (current != null)
      {
        if (currentTime >= current.StartTime.AddMinutes(-schedule.PreRecordInterval) && currentTime <= current.EndTime.AddMinutes(schedule.PostRecordInterval))
        {
          ScheduleBLL scheduleBll = new ScheduleBLL(schedule);
          if (!scheduleBll.IsSerieIsCanceled(current.StartTime))
          {
            bool createSpawnedOnceSchedule = CreateSpawnedOnceSchedule(schedule, current);
            if (createSpawnedOnceSchedule)
            {
              ResetTimer(); //lets process the spawned once schedule at once.
            }
          }
        }
      }
      return isTimeToRecord;
    }

    private RecordingDetail IsTimeToRecordWeekly(Schedule schedule, DateTime currentTime, out bool isTimeToRecord)
    {
      isTimeToRecord = false;
      RecordingDetail newRecording = null;
      if ((currentTime.DayOfWeek == schedule.StartTime.DayOfWeek) && (currentTime.Date >= schedule.StartTime.Date))
      {
        newRecording = CreateNewRecordingDetail(schedule, currentTime);
        isTimeToRecord = (newRecording != null);
      }
      return newRecording;
    }

    private RecordingDetail IsTimeToRecordWorkingDays(Schedule schedule, DateTime currentTime, out bool isTimeToRecord)
    {
      isTimeToRecord = false;
      RecordingDetail newRecording = null;
      if (WeekEndTool.IsWorkingDay(currentTime.DayOfWeek))
      {
        newRecording = CreateNewRecordingDetail(schedule, currentTime);
        isTimeToRecord = (newRecording != null);
      }
      return newRecording;
    }

    private RecordingDetail IsTimeToRecordWeekends(Schedule schedule, DateTime currentTime, out bool isTimeToRecord)
    {
      isTimeToRecord = false;
      RecordingDetail newRecording = null;
      if (WeekEndTool.IsWeekend(currentTime.DayOfWeek))
      {
        newRecording = CreateNewRecordingDetail(schedule, currentTime);
        isTimeToRecord = (newRecording != null);
      }
      return newRecording;
    }

    private RecordingDetail IsTimeToRecordDaily(Schedule schedule, DateTime currentTime, out bool isTimeToRecord)
    {
      RecordingDetail newRecording = CreateNewRecordingDetail(schedule, currentTime);
      isTimeToRecord = (newRecording != null);
      return newRecording;
    }

    private RecordingDetail IsTimeToRecordOnce(Schedule schedule, DateTime currentTime, out bool isTimeToRecord)
    {
      isTimeToRecord = false;
      RecordingDetail newRecording = null;
      if (currentTime >= schedule.StartTime.AddMinutes(-schedule.PreRecordInterval) && currentTime <= schedule.EndTime.AddMinutes(schedule.PostRecordInterval))
      {
        IVirtualCard vCard;
        bool isRecordingSchedule = IsRecordingSchedule(schedule.IdSchedule, out vCard);
        if (!isRecordingSchedule)
        {
          newRecording = new RecordingDetail(schedule, schedule.Channel, schedule.EndTime, schedule.Series);
          isTimeToRecord = true;
        }
      }
      return newRecording;
    }

    private bool CreateSpawnedOnceSchedule(Schedule schedule, Program current)
    {
      bool isSpawnedOnceScheduleCreated = false;

      Schedule dbSchedule = ScheduleManagement.RetrieveOnce(current.IdChannel, current.Title, current.StartTime, current.EndTime);
      if (dbSchedule == null) // not created yet
      {
        Schedule once = ScheduleManagement.RetrieveOnce(current.IdChannel, current.Title, current.StartTime, current.EndTime);

        if (once == null) // make sure that we DO NOT create multiple once recordings.
        {
          Schedule newSchedule = ScheduleFactory.Clone(schedule);
          newSchedule.IdChannel = current.IdChannel;
          newSchedule.StartTime = current.StartTime;
          newSchedule.EndTime = current.EndTime;
          newSchedule.ScheduleType = (int)ScheduleRecordingType.Once;
          newSchedule.Series = true;
          newSchedule.IdParentSchedule = schedule.IdSchedule;
          ScheduleManagement.SaveSchedule(newSchedule);
          isSpawnedOnceScheduleCreated = true;
          // 'once typed' created schedule will be used instead at next call of IsTimeToRecord()
        }
      }
      return isSpawnedOnceScheduleCreated;
    }

    private RecordingDetail CreateNewRecordingDetail(Schedule schedule, DateTime currentTime)
    {
      RecordingDetail newRecording = null;
      DateTime start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, schedule.StartTime.Second);
      DateTime end = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.EndTime.Hour, schedule.EndTime.Minute, schedule.EndTime.Second);
      if (start > end)
        end = end.AddDays(1);
      if (currentTime >= start.AddMinutes(-schedule.PreRecordInterval) && currentTime <= end.AddMinutes(schedule.PostRecordInterval))
      {
        ScheduleBLL scheduleBll = new ScheduleBLL(schedule);
        if (!scheduleBll.IsSerieIsCanceled(start))
        {
          IVirtualCard vCard;
          bool isRecordingSchedule = IsRecordingSchedule(schedule.IdSchedule, out vCard);
          if (!isRecordingSchedule)
          {
            newRecording = new RecordingDetail(schedule, schedule.Channel, end, true);
          }
        }
      }
      return newRecording;
    }

    /// <summary>
    /// Starts recording the recording specified
    /// </summary>
    /// <param name="recDetail"></param>
    /// <returns>true if recording is started, otherwise false</returns>
    private void StartRecord(RecordingDetail recDetail)
    {
      IUser user = recDetail.User;

      this.LogDebug("Scheduler: Time to record {0} {1}-{2} {3}", recDetail.Channel.DisplayName,
                DateTime.Now.ToShortTimeString(), recDetail.EndTime.ToShortTimeString(),
                recDetail.Schedule.Entity.ProgramName);
      //get list of all cards we can use todo the recording
      StartRecordOnFreeCard(recDetail, ref user);
    }

    private void StartRecordOnCard(RecordingDetail recDetail,ref IUser user,  ICollection<CardDetail> cardsForReservation)
    {
      var cardRes = new CardReservationRec();

      if (cardsForReservation.Count == 0)
      {
        //no free cards available
        this.LogDebug("scheduler: no free cards found for recording during initial card allocation.");
      }
      else
      {
        IterateCardsUntilRecording(recDetail, user, cardsForReservation, cardRes);
      }
    }

    private void IterateCardsUntilRecording(RecordingDetail recDetail, IUser user, ICollection<CardDetail> cardsForReservation, CardReservationRec cardRes)
    {
      IDictionary<CardDetail, ICardTuneReservationTicket> tickets = null;
      try
      {
        ICollection<CardDetail> cardsIterated = new HashSet<CardDetail>();

        int cardIterations = 0;
        bool moreCardsAvailable = true;
        bool recSucceded = false;
        while (moreCardsAvailable && !recSucceded)
        {
          tickets = CardReservationHelper.RequestCardReservations(user, cardsForReservation, cardRes, cardsIterated, recDetail.Channel.IdChannel);
          if (tickets.Count == 0)
          {
            //no free cards available
            this.LogDebug("scheduler: no free card reservation(s) could be made.");
            break;
          }
          TvResult tvResult;
          ICollection<ICardTuneReservationTicket> ticketsList = tickets.Values;
          var cardAllocationTicket = new AdvancedCardAllocationTicket(ticketsList);
          ICollection<CardDetail> cards = cardAllocationTicket.UpdateFreeCardsForChannelBasedOnTicket(cardsForReservation, user, out tvResult);
          CardReservationHelper.CancelCardReservationsExceedingMaxConcurrentTickets(tickets, cards);
          CardReservationHelper.CancelCardReservationsNotFoundInFreeCards(cardsForReservation, tickets, cards);
          CardReservationHelper.CancelCardReservationsExceedingMaxConcurrentTickets(tickets, cards);
          CardReservationHelper.CancelCardReservationsNotFoundInFreeCards(cardsForReservation, tickets, cards);
          int maxCards = GetMaxCards(cards);
          CardReservationHelper.CancelCardReservationsBasedOnMaxCardsLimit(tickets, cards, maxCards);
          UpdateCardsIterated(cardsIterated, cards); //keep track of what cards have been iterated here.

          if (cards != null && cards.Count > 0)
          {
            cardIterations += cards.Count;
            recSucceded = IterateTicketsUntilRecording(recDetail, user, cards, cardRes, maxCards, tickets, cardsIterated);
            moreCardsAvailable = _maxRecordFreeCardsToTry == 0 || _maxRecordFreeCardsToTry > cardIterations;
          }
          else
          {
            this.LogDebug("scheduler: no free cards found for recording.");
            break;
          }
        } // end of while
      }
      finally
      {
        CardReservationHelper.CancelAllCardReservations(tickets);
      }
    }

    private bool IterateTicketsUntilRecording(RecordingDetail recDetail, IUser user, ICollection<CardDetail> cards, CardReservationRec cardRes, int maxCards, IDictionary<CardDetail, ICardTuneReservationTicket> tickets, ICollection<CardDetail> cardsIterated)
    {
      bool recSucceded = false;
      while (!recSucceded && tickets.Count > 0)
      {
        List<CardDetail> freeCards = cards.Where(t => t.NumberOfOtherUsers == 0 || (t.NumberOfOtherUsers > 0 && t.SameTransponder)).ToList();
        List<CardDetail> availCards = cards.Where(t => t.NumberOfOtherUsers > 0 && !t.SameTransponder).ToList();

        this.LogDebug("scheduler: try max {0} of {1} free cards for recording", maxCards, cards.Count);
        if (freeCards.Count > 0)
        {
          recSucceded = FindFreeCardAndStartRecord(recDetail, user, freeCards, maxCards, tickets, cardRes);
        }
        else if (availCards.Count > 0)
        {
          recSucceded = FindAvailCardAndStartRecord(recDetail, user, availCards, maxCards, tickets, cardRes);
        }

        if (!recSucceded)
        {
          CardDetail cardInfo = GetCardDetailForRecording(cards);
          cards.Remove(cardInfo);
          RecordingFailedNotification(recDetail);
        }
      }
      return recSucceded;
    }

    private void StartRecordOnFreeCard(RecordingDetail recDetail, ref IUser user)
    {
      var cardAllocationStatic = new AdvancedCardAllocationStatic();
      List<CardDetail> freeCardsForReservation = cardAllocationStatic.GetFreeCardsForChannel(ServiceManager.Instance.InternalControllerService.CardCollection, recDetail.Channel, user);
      StartRecordOnCard(recDetail, ref user, freeCardsForReservation);
    }

    private static void UpdateCardsIterated(ICollection<CardDetail> freeCardsIterated, IEnumerable<CardDetail> freeCards)
    {
      foreach (CardDetail card in freeCards)
      {
        UpdateCardsIterated(freeCardsIterated, card);
      }
    }

    private static void UpdateCardsIterated(ICollection<CardDetail> freeCardsIterated, CardDetail card)
    {
      if (!freeCardsIterated.Contains(card))
      {
        freeCardsIterated.Add(card);
      }
    }

    private bool FindAvailCardAndStartRecord(RecordingDetail recDetail, IUser user, ICollection<CardDetail> cards, int maxCards, IDictionary<CardDetail, ICardTuneReservationTicket> tickets, CardReservationRec cardResImpl)
    {
      bool result = false;
      //keep tuning each card until we are succesful
      for (int k = 0; k < maxCards; k++)
      {
        ITvCardHandler tvCardHandler;
        CardDetail cardInfo = GetCardDetailForRecording(cards);
        if (ServiceManager.Instance.InternalControllerService.CardCollection.TryGetValue(cardInfo.Id, out tvCardHandler))
        {
          ICardTuneReservationTicket ticket = GetTicketByCardDetail(cardInfo, tickets);

          if (ticket == null)
          {
            ticket = CardReservationHelper.RequestCardReservation(user, cardInfo, cardResImpl, recDetail.Channel.IdChannel);
            if (ticket != null)
            {
              tickets[cardInfo] = ticket;
            }
          }

          if (ticket != null)
          {
            try
            {
              cardInfo = HijackCardForRecording(cards, ticket);
              result = SetupAndStartRecord(recDetail, ref user, cardInfo, ticket, cardResImpl);
              if (result)
              {
                break;
              }

            }
            catch (Exception ex)
            {
              CardReservationHelper.CancelCardReservationAndRemoveTicket(cardInfo, tickets);
              this.LogError(ex);
              StopFailedRecord(recDetail);
            }
          }
          else
          {
            this.LogDebug("scheduler: could not find available cardreservation on card:{0}", cardInfo.Id);
          }
        }
        this.LogDebug("scheduler: recording failed, lets try next available card.");
        CardReservationHelper.CancelCardReservationAndRemoveTicket(cardInfo, tickets);
        if (cardInfo != null && cards.Contains(cardInfo))
        {
          cards.Remove(cardInfo);
        }
      }
      return result;
    }

    private static ICardTuneReservationTicket GetTicketByCardDetail(CardDetail cardInfo, IDictionary<CardDetail, ICardTuneReservationTicket> tickets)
    {
      ICardTuneReservationTicket ticket;
      tickets.TryGetValue(cardInfo, out ticket);
      return ticket;
    }

    private bool FindFreeCardAndStartRecord(RecordingDetail recDetail, IUser user, ICollection<CardDetail> cards, int maxCards, IDictionary<CardDetail, ICardTuneReservationTicket> tickets, CardReservationRec cardResImpl)
    {
      bool result = false;
      //keep tuning each card until we are succesful
      for (int i = 0; i < maxCards; i++)
      {
        CardDetail cardInfo = null;
        try
        {
          cardInfo = GetCardDetailForRecording(cards);
          ITvCardHandler tvCardHandler;
          if (ServiceManager.Instance.InternalControllerService.CardCollection.TryGetValue(cardInfo.Id, out tvCardHandler))
          {
            ICardTuneReservationTicket ticket = GetTicketByCardDetail(cardInfo, tickets);
            if (ticket == null)
            {
              ticket = CardReservationHelper.RequestCardReservation(user, cardInfo, cardResImpl, recDetail.Channel.IdChannel);
              if (ticket != null)
              {
                tickets[cardInfo] = ticket;
              }
            }

            if (ticket != null)
            {
              result = SetupAndStartRecord(recDetail, ref user, cardInfo, ticket, cardResImpl);
              if (result)
              {
                break;
              }
            }
            else
            {
              this.LogDebug("scheduler: could not find free cardreservation on card:{0}", cardInfo.Id);
            }
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "");
        }
        this.LogDebug("scheduler: recording failed, lets try next available card.");
        CardReservationHelper.CancelCardReservationAndRemoveTicket(cardInfo, tickets);
        StopFailedRecord(recDetail);
        if (cardInfo != null && cards.Contains(cardInfo))
        {
          cards.Remove(cardInfo);
        }
      }
      return result;
    }

    private bool SetupAndStartRecord(RecordingDetail recDetail, ref IUser user, CardDetail cardInfo, ICardTuneReservationTicket ticket, CardReservationRec cardResImpl)
    {
      bool result = false;
      if (cardInfo != null)
      {
        user.CardId = cardInfo.Id;
        StartRecordingNotification(recDetail);
        SetupRecordingFolder(cardInfo);
        if (StartRecordingOnDisc(recDetail, ref user, cardInfo, ticket, cardResImpl))
        {
          CreateRecording(recDetail);
          try
          {
            recDetail.User.CardId = user.CardId;
            SetRecordingProgramState(recDetail);
            _recordingsInProgressList.Add(recDetail);
            RecordingStartedNotification(recDetail);
            SetupQualityControl(recDetail);
            WriteMatroskaFile(recDetail);
          }
          catch (Exception ex)
          {
            //consume exception, since it isn't catastrophic
            this.LogError(ex);
          }

          this.LogDebug("Scheduler: recList: count: {0} add scheduleid: {1} card: {2}",
                    _recordingsInProgressList.Count,
                    recDetail.Schedule.Entity.IdSchedule, recDetail.CardInfo.Card.Name);
          result = true;
        }
      }
      else
      {
        this.LogDebug("scheduler: no card found to record on.");
      }
      return result;
    }

    /// <summary>
    /// stops failed recording
    /// </summary>
    /// <param name="recording">Recording</param>    
    private void StopFailedRecord(RecordingDetail recording)
    {
      try
      {
        IUser user = recording.User;

        if (recording.CardInfo != null && ServiceManager.Instance.InternalControllerService.SupportsSubChannels(recording.CardInfo.Id) == false)
        {
          ServiceManager.Instance.InternalControllerService.StopTimeShifting(user.Name, out user);
        }

        this.LogDebug("Scheduler: stop failed record {0} {1}-{2} {3}", recording.Channel.DisplayName,
                  recording.RecordingStartDateTime,
                  recording.EndTime, recording.Schedule.Entity.ProgramName);

        if (ServiceManager.Instance.InternalControllerService.IsRecording(ref user))
        {
          if (ServiceManager.Instance.InternalControllerService.StopRecording(user.Name, user.CardId, out user))
          {
            ResetRecordingStateOnProgram(recording);
            if (recording.Recording != null)
            {

              TVDatabase.TVBusinessLayer.RecordingManagement.DeleteRecording(recording.Recording.IdRecording);
              recording.Recording = null;
            }

            if (_recordingsInProgressList.Contains(recording))
            {
              _recordingsInProgressList.Remove(recording);
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    private static CardDetail GetCardDetailForRecording(IEnumerable<CardDetail> freeCards)
    {
      //first try to start recording using the recommended card
      CardDetail cardInfo = freeCards.FirstOrDefault();
      return cardInfo;
    }

    private int GetMaxCards(ICollection<CardDetail> freeCards)
    {
      int maxCards;
      if (_maxRecordFreeCardsToTry == 0)
      {
        maxCards = freeCards.Count;
      }
      else
      {
        maxCards = Math.Min(_maxRecordFreeCardsToTry, freeCards.Count);

        if (maxCards > freeCards.Count)
        {
          maxCards = freeCards.Count;
        }
      }
      return maxCards;
    }

    private CardDetail HijackCardForRecording(ICollection<CardDetail> availableCards, ICardTuneReservationTicket ticket)
    {
      CardDetail cardInfo = HijackCardTimeshiftingOnSameTransponder(availableCards, ticket);

      if (cardInfo == null)
      {
        cardInfo = HijackCardTimeshiftingOnDifferentTransponder(availableCards, ticket);
      }
      if (cardInfo == null)
      {
        this.LogDebug("Scheduler : no free card was found and no card was found where user can be kicked.");
      }
      return cardInfo;
    }

    private CardDetail HijackCardTimeshiftingOnDifferentTransponder(IEnumerable<CardDetail> availableCards, ICardTuneReservationTicket ticket)
    {
      CardDetail cardInfo = null;
      foreach (CardDetail cardDetail in availableCards)
      {
        if (!cardDetail.SameTransponder)
        {
          bool canKickAll = CanKickAllUsersOnTransponder(ticket);
          if (canKickAll)
          {
            cardInfo = cardDetail;
            KickAllUsersOnTransponder(cardDetail, ticket);
            break;
          }
        }
      }
      return cardInfo;
    }

    private void KickAllUsersOnTransponder(CardDetail cardDetail, ICardTuneReservationTicket ticket)
    {
      this.LogDebug(
        "Scheduler : card is not tuned to the same transponder and not recording, kicking all users. record on card:{0} priority:{1}",
        cardDetail.Id, cardDetail.Card.Priority);
      for (int i = 0; i < ticket.TimeshiftingUsers.Count; i++)
      {
        IUser timeshiftingUser = ticket.TimeshiftingUsers[i];
        this.LogDebug("Scheduler : kicking user:{0}", timeshiftingUser.Name);

        foreach (ISubChannel subchannel in timeshiftingUser.SubChannels.Values)
        {
          int idChannel = subchannel.IdChannel;

          ServiceManager.Instance.InternalControllerService.StopTimeShifting(ref timeshiftingUser, TvStoppedReason.RecordingStarted, idChannel);

          this.LogDebug(
            "Scheduler : card is tuned to the same transponder but not free. record on card:{0} priority:{1}, kicking user:{2}",
            cardDetail.Id, cardDetail.Card.Priority, timeshiftingUser.Name);
        }
      }
    }

    private static bool CanKickAllUsersOnTransponder(ICardTuneReservationTicket ticket)
    {
      IList<IUser> recUsers = ticket.RecordingUsers;
      bool canKickAll = (recUsers.Count == 0);
      return canKickAll;
    }

    private CardDetail HijackCardTimeshiftingOnSameTransponder(IEnumerable<CardDetail> availableCards, ICardTuneReservationTicket ticket)
    {
      CardDetail cardInfo = null;
      foreach (CardDetail cardDetail in availableCards.Where(cardDetail => cardDetail.SameTransponder))
      {
        KickUserOnSameTransponder(cardDetail, ticket, ref cardInfo);
        if (cardInfo != null)
        {
          break;
        }
      }
      return cardInfo;
    }

    private void KickUserOnSameTransponder(CardDetail cardDetail, ICardTuneReservationTicket ticket, ref CardDetail cardInfo)
    {
      bool canKickAllUsersOnTransponder = CanKickAllUsersOnTransponder(ticket);

      if (canKickAllUsersOnTransponder)
      {
        for (int i = 0; i < ticket.TimeshiftingUsers.Count; i++)
        {
          IUser timeshiftingUser = ticket.TimeshiftingUsers[i];
          foreach (var subchannel in timeshiftingUser.SubChannels.Values)
          {
            this.LogDebug(
               "Scheduler : card is tuned to the same transponder but not free. record on card:{0} priority:{1}, kicking user:{2}",
             cardDetail.Id, cardDetail.Card.Priority, timeshiftingUser.Name);
            ServiceManager.Instance.InternalControllerService.StopTimeShifting(ref timeshiftingUser, TvStoppedReason.RecordingStarted, subchannel.IdChannel);
            cardInfo = cardDetail;
          }
        }
      }
    }

    private void RecordingFailedNotification(RecordingDetail recDetail)
    {
      IUser user = recDetail.User;
      ServiceManager.Instance.InternalControllerService.Fire(this, new TvServerEventArgs(TvServerEventType.RecordingFailed, new VirtualCard(user), (User)user));
    }


    private void RecordingStartedNotification(RecordingDetail recDetail)
    {
      IUser user = recDetail.User;
      ServiceManager.Instance.InternalControllerService.Fire(this,
        new TvServerEventArgs(TvServerEventType.RecordingStarted, new VirtualCard(user), (User)user, recDetail.Schedule.Entity.IdSchedule, recDetail.Recording.IdRecording));
    }

    private void StartRecordingNotification(RecordingDetail recDetail)
    {
      IUser user = recDetail.User;
      ServiceManager.Instance.InternalControllerService.Fire(this,
        new TvServerEventArgs(TvServerEventType.StartRecording, new VirtualCard(user), (User)user, recDetail.Schedule.Entity.IdSchedule, -1));
    }

    private void SetupRecordingFolder(CardDetail cardInfo)
    {
      if (cardInfo.Card.RecordingFolder == String.Empty)
        cardInfo.Card.RecordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
      if (cardInfo.Card.TimeshiftingFolder == String.Empty)
        cardInfo.Card.TimeshiftingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }

    private bool StartRecordingOnDisc(RecordingDetail recDetail, ref IUser user, CardDetail cardInfo, ICardTuneReservationTicket ticket, CardReservationRec cardResImpl)
    {
      bool startRecordingOnDisc = false;
      ServiceManager.Instance.InternalControllerService.EpgGrabberEnabled = false;
      this.LogDebug("Scheduler : record, first tune to channel");

      cardResImpl.CardInfo = cardInfo;
      cardResImpl.RecDetail = recDetail;

      TvResult tuneResult = ServiceManager.Instance.InternalControllerService.Tune(ref user, cardInfo.TuningDetail, recDetail.Channel.IdChannel, ticket, cardResImpl);
      startRecordingOnDisc = (tuneResult == TvResult.Succeeded);

      return startRecordingOnDisc;
    }

    private static void CreateRecording(RecordingDetail recDetail)
    {
      Log.Debug(String.Format("Scheduler: adding new row in db for title=\"{0}\" of type=\"{1}\"",
                              recDetail.Program.Entity.Title, recDetail.Schedule.Entity.ScheduleType));

      recDetail.Recording = RecordingFactory.CreateRecording(recDetail.Schedule.Entity.IdChannel, recDetail.Schedule.Entity.IdSchedule, true,
                                          recDetail.RecordingStartDateTime, DateTime.Now, recDetail.Program.Entity.Title,
                                          recDetail.Program.Entity.Description, recDetail.Program.Entity.ProgramCategory, recDetail.FileName,
                                          recDetail.Schedule.Entity.KeepMethod,
                                          recDetail.Schedule.Entity.KeepDate.GetValueOrDefault(DateTime.MinValue), 0, recDetail.Program.Entity.EpisodeName,
                                          recDetail.Program.Entity.SeriesNum, recDetail.Program.Entity.EpisodeNum,
                                          recDetail.Program.Entity.EpisodePart);

      TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecording(recDetail.Recording);
    }

    private static void SetRecordingProgramState(RecordingDetail recDetail)
    {
      if (recDetail.Program.Entity.IdProgram > 0)
      {
        recDetail.Program.IsRecordingOnce = true;
        recDetail.Program.IsRecordingSeries = recDetail.Schedule.Entity.Series;
        recDetail.Program.IsRecordingManual = recDetail.Schedule.IsManual;
        recDetail.Program.IsRecordingOncePending = false;
        recDetail.Program.IsRecordingSeriesPending = false;
        ProgramManagement.SaveProgram(recDetail.Program.Entity);
      }
    }

    private void SetupQualityControl(RecordingDetail recDetail)
    {
      IUser user = recDetail.User;
      int cardId = user.CardId;
      if (ServiceManager.Instance.InternalControllerService.SupportsQualityControl(cardId))
      {
        if (recDetail.Schedule.BitRateMode != VIDEOENCODER_BITRATE_MODE.NotSet && ServiceManager.Instance.InternalControllerService.SupportsBitRate(cardId))
        {
          ServiceManager.Instance.InternalControllerService.SetQualityType(cardId, recDetail.Schedule.QualityType);
        }
        if (recDetail.Schedule.QualityType != QualityType.NotSet && ServiceManager.Instance.InternalControllerService.SupportsBitRateModes(cardId) &&
            ServiceManager.Instance.InternalControllerService.SupportsPeakBitRateMode(cardId))
        {
          ServiceManager.Instance.InternalControllerService.SetBitRateMode(cardId, recDetail.Schedule.BitRateMode);
        }
      }
    }

    private void WriteMatroskaFile(RecordingDetail recDetail)
    {
      if (_createTagInfoXML)
      {
        string fileName = recDetail.FileName;
        var category = "";
        if (recDetail.Program.Entity.ProgramCategory != null)
        {
          category = recDetail.Program.Entity.ProgramCategory.Category;
        }
        var info = new MatroskaTagInfo
                                 {
                                   title = recDetail.Program.Entity.Title,
                                   description = recDetail.Program.Entity.Description,
                                   genre = category,
                                   channelName = recDetail.Schedule.Entity.Channel.DisplayName,
                                   episodeName = recDetail.Program.Entity.EpisodeName,
                                   seriesNum = recDetail.Program.Entity.SeriesNum,
                                   episodeNum = recDetail.Program.Entity.EpisodeNum,
                                   episodePart = recDetail.Program.Entity.EpisodePart,
                                   startTime = recDetail.RecordingStartDateTime,
                                   endTime = recDetail.EndTime,
                                   mediaType = Convert.ToString(recDetail.Recording.MediaType)
                                 };

        MatroskaTagHandler.WriteTag(Path.ChangeExtension(fileName, ".xml"), info);
      }
    }


    /// <summary>
    /// stops recording the specified recording 
    /// </summary>
    /// <param name="recording">Recording</param>    
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void StopRecord(RecordingDetail recording)
    {
      try
      {
        IUser user = recording.User;

        if (ServiceManager.Instance.InternalControllerService.SupportsSubChannels(recording.CardInfo.Id) == false)
        {
          ServiceManager.Instance.InternalControllerService.StopTimeShifting(user.Name, out user);
        }

        this.LogDebug("Scheduler: stop record {0} {1}-{2} {3}", recording.Channel.DisplayName,
                  recording.RecordingStartDateTime,
                  recording.EndTime, recording.Schedule.Entity.ProgramName);

        if (ServiceManager.Instance.InternalControllerService.StopRecording(user.Name, user.CardId, out user))
        {
          ResetRecordingState(recording);
          ResetRecordingStateOnProgram(recording);
          _recordingsInProgressList.Remove(recording); //only remove recording from the list, if we are succesfull

          if ((ScheduleRecordingType)recording.Schedule.Entity.ScheduleType == ScheduleRecordingType.Once)
          {
            StopRecordOnOnceSchedule(recording);
          }
          else
          {
            StopRecordOnSeriesSchedule(recording);
          }

          RecordingEndedNotification(recording);
        }
        else
        {
          RetryStopRecord(recording);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    private void RetryStopRecord(RecordingDetail recording)
    {
      this.LogDebug("Scheduler: stop record did not succeed (trying again in 1 min.) {0} {1}-{2} {3}",
                recording.Channel.DisplayName, recording.RecordingStartDateTime, recording.EndTime,
                recording.Schedule.Entity.ProgramName);
      recording.Recording.EndTime = recording.Recording.EndTime.AddMinutes(1);
      //lets try and stop the recording in 1 min. again.      
      TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecording(recording.Recording);
    }

    private void RecordingEndedNotification(RecordingDetail recording)
    {
      IUser user = recording.User;
      ServiceManager.Instance.InternalControllerService.Fire(this,
        new TvServerEventArgs(TvServerEventType.RecordingEnded, new VirtualCard(user), (User)user, recording.Schedule.Entity.IdSchedule, recording.Recording.IdRecording));
    }

    private void StopRecordOnSeriesSchedule(RecordingDetail recording)
    {
      this.LogDebug("Scheduler: endtime={0}, Program.endTime={1}, postRecTime={2}", recording.EndTime,
                recording.Program.Entity.EndTime, recording.Schedule.Entity.PostRecordInterval);
      if (DateTime.Now <= recording.Program.Entity.EndTime.AddMinutes(recording.Schedule.Entity.PostRecordInterval))
      {
        CancelSchedule(recording, recording.Schedule.Entity.IdSchedule);
      }
      else
      {
        _episodeManagement.OnScheduleEnded(recording.FileName, recording.Schedule.Entity, recording.Program.Entity);
      }
    }

    private void StopRecordOnOnceSchedule(RecordingDetail recording)
    {
      IUser user = recording.User;
      if (recording.IsSerie)
      {
        _episodeManagement.OnScheduleEnded(recording.FileName, recording.Schedule.Entity, recording.Program.Entity);
      }
      ServiceManager.Instance.InternalControllerService.Fire(this,
        new TvServerEventArgs(TvServerEventType.ScheduleDeleted, new VirtualCard(user), (User)user, recording.Schedule.Entity.IdSchedule, -1));
      // now we can safely delete it
      ScheduleManagement.DeleteSchedule(recording.Schedule.Entity.IdSchedule);
    }

    private void ResetRecordingState(RecordingDetail recording)
    {
      try
      {
        Recording rec = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(recording.Recording.IdRecording);
        rec.EndTime = DateTime.Now;
        rec.IsRecording = false;
        TVDatabase.TVBusinessLayer.RecordingManagement.SaveRecording(rec);
      }
      catch (Exception ex)
      {
        this.LogError("StopRecord - updating record id={0} failed {1}", recording.Recording.IdRecording, ex.StackTrace);
      }
    }

    private static void ResetRecordingStateOnProgram(RecordingDetail recording)
    {
      if (recording.Program.Entity.IdProgram > 0)
      {
        recording.Program.IsRecordingManual = false;
        recording.Program.IsRecordingSeries = false;
        recording.Program.IsRecordingOnce = false;
        recording.Program.IsRecordingOncePending = false;
        recording.Program.IsRecordingSeriesPending = false;
        ProgramManagement.SaveProgram(recording.Program.Entity);
      }
    }

    #endregion
  }
}