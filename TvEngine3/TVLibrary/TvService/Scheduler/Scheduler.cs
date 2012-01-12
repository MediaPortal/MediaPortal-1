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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TvControl;
using TvDatabase;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvService
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

    [FlagsAttribute]
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
    private readonly TVController _tvController;
    private List<RecordingDetail> _recordingsInProgressList;
    private bool _createTagInfoXML;
    private bool _preventDuplicateEpisodes;
    private int _preventDuplicateEpisodesKey;
    private Thread _schedulerThread = null;
    private TvBusinessLayer _layer = new TvBusinessLayer();

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
    /// <param name="controller">instance of the TVController</param>
    public Scheduler(TVController controller)
    {
      _tvController = controller;
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
      Log.Write("Scheduler: started");

      ResetRecordingStates();

      _recordingsInProgressList = new List<RecordingDetail>();
      IList<Schedule> schedules = Schedule.ListAll();
      Log.Write("Scheduler: loaded {0} schedules", schedules.Count);
      StartSchedulerThread();
      new DiskManagement();
      new RecordingManagement();
      _episodeManagement = new EpisodeManagement();
      HandleSleepMode();
    }

    /// <summary>
    /// Stops the scheduler
    /// </summary>
    public void Stop()
    {
      Log.Write("Scheduler: stopped");
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
      IList<Schedule> schedules = Schedule.ListAll();
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
    public bool IsRecordingSchedule(int idSchedule, out VirtualCard card)
    {
      card = null;
      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
        if (rec.Schedule.IdSchedule == idSchedule)
        {
          IUser user = new User();
          user.Name = string.Format("scheduler{0}", rec.Schedule.IdSchedule);
          user.CardId = rec.CardInfo.Id;
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
      Log.Write("recList:StopRecordingSchedule {0}", idSchedule);
      RecordingDetail foundRec = null;

      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
        if (rec.Schedule.IdSchedule == idSchedule)
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
    /// Method which returns the database schedule id for the card
    /// </summary>
    /// <param name="cardId">id of the card</param>
    /// <param name="ChannelId">Channel id</param>
    /// <returns>id of schedule the card is recording or -1 if its not recording</returns>
    public int GetRecordingScheduleForCard(int cardId, int ChannelId)
    {
      //reverse loop, since items can be removed during iteration
      for (int i = _recordingsInProgressList.Count - 1; i >= 0; i--)
      {
        RecordingDetail rec = _recordingsInProgressList[i];
        if (rec.CardInfo.Id == cardId && rec.Channel.IdChannel == ChannelId)
        {
          return rec.Schedule.IdSchedule;
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
      Log.Write("recList:StopRecordingOnCard {0}", cardId);
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
      Log.Debug("Scheduler: thread started.");
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
          Log.Debug("Scheduler: thread stopped.");
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
      _createTagInfoXML = (_layer.GetSetting("createtaginfoxml", "yes").Value == "yes");
      _preventDuplicateEpisodes = (_layer.GetSetting("PreventDuplicates", "no").Value == "yes");
      _preventDuplicateEpisodesKey = Convert.ToInt32(_layer.GetSetting("EpisodeKey", "0").Value);
      _maxRecordFreeCardsToTry = Int32.Parse(_layer.GetSetting("recordMaxFreeCardsToTry", "0").Value);
    }

    private static void ResetRecordingStates()
    {
      Recording.ResetActiveRecordings();
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
            Log.Write("scheduler: SchedulerWorker inner exception {0}", ex);
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
        Log.Write("scheduler: SchedulerWorker outer exception {0}", ex2);
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
        IList<Schedule> schedules = Schedule.FindOrphanedOnceSchedules();
        foreach (Schedule orphan in schedules)
        {
          Log.Debug("Scheduler: Orphaned once schedule found {0} - removing", orphan.IdSchedule);
          orphan.Delete();
        }
      }
    }

    private string CleanEpisodeTitle(string aEpisodeTitle)
    {
      try
      {
        string CleanedEpisode = aEpisodeTitle.Replace(" (LIVE)", String.Empty);
        CleanedEpisode = aEpisodeTitle.Replace(" (Wdh.)", String.Empty);
        return CleanedEpisode.Trim();
      }
      catch (Exception ex)
      {
        Log.Error("Scheduler: Could not cleanup episode title {0} - {1}", aEpisodeTitle, ex.ToString());
        return aEpisodeTitle;
      }
    }

    private void CheckAndDeleteOrphanedRecordings()
    {
      List<VirtualCard> vCards = _tvController.GetAllRecordingCards();

      foreach (VirtualCard vCard in vCards)
      {
        int schedId = vCard.RecordingScheduleId;
        if (schedId > 0)
        {
          Schedule sc = Schedule.Retrieve(schedId);
          if (sc == null)
          {
            //seems like the schedule has disappeared  stop the recording also.
            Log.Debug("Scheduler: Orphaned Recording found {0} - removing", schedId);
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
      IList<Schedule> schedules = Schedule.ListAll();
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
            //yes - let's check whether this file is already present and therefore doesn't need to be recorded another time
            if (IsEpisodeUnrecorded(schedule.ScheduleType, newRecording))
            {
              if (newRecording != null)
              {
                StartRecord(newRecording);
              }
              else
              {
                Log.Info("StartAnyDueRecordings: RecordingDetail was null");
              }
            }
          }
        }
      }
    }

    private bool IsScheduleReadyForRecording(Schedule schedule)
    {
      bool isScheduleReadyForRecording = true;
      DateTime now = DateTime.Now;
      VirtualCard card;

      if (schedule.Canceled != Schedule.MinSchedule ||
          IsRecordingSchedule(schedule.IdSchedule, out card) ||
          schedule.IsSerieIsCanceled(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)))
      {
        isScheduleReadyForRecording = false;
      }

      return isScheduleReadyForRecording;
    }

    private bool IsEpisodeUnrecorded(int scheduleType, RecordingDetail newRecording)
    {
      string ToRecordTitle = "";
      string ToRecordEpisode = "";
      bool NewRecordingNeeded = true;

      //cleanup: remove EPG additions of Clickfinder plugin      
      try
      {
        // Allow user to turn this on or off in case of unreliable EPG
        if (_preventDuplicateEpisodes && newRecording != null)
        {
          switch (_preventDuplicateEpisodesKey)
          {
            case 1: // Episode Number
              ToRecordEpisode = newRecording.Program.SeriesNum + "." + newRecording.Program.EpisodeNum + "." +
                                newRecording.Program.EpisodePart;
              break;
            default: // Episode Name
              ToRecordEpisode = CleanEpisodeTitle(newRecording.Program.EpisodeName);
              break;
          }

          ToRecordTitle = CleanEpisodeTitle(newRecording.Program.Title);

          Log.Debug("Scheduler: Check recordings for schedule {0}...", ToRecordTitle);
          // EPG needs to have episode information to distinguish between repeatings and new broadcasts
          if (ToRecordEpisode.Equals(String.Empty) || ToRecordEpisode.Equals(".."))
          {
            // Check the type so we aren't logging too verbose on single runs
            if (scheduleType != (int)ScheduleRecordingType.Once)
            {
              Log.Info("Scheduler: No epsisode title found for schedule {0} - omitting repeating check.",
                       newRecording.Program.Title);
            }
          }
          else
          {
            IList<Recording> pastRecordings = Recording.ListAll();
            string pastRecordEpisode = "";
            for (int i = 0; i < pastRecordings.Count; i++)
            {
              // Checking the record "title" itself to avoid unnecessary checks.
              // Furthermore some EPG sources could misuse the episode field for other, non-unique information
              if (CleanEpisodeTitle(pastRecordings[i].Title).Equals(ToRecordTitle,
                                                                    StringComparison.CurrentCultureIgnoreCase))
              {
                //Log.Debug("Scheduler: Found recordings of schedule {0} - checking episodes...", ToRecordTitle);
                // The schedule which is about to be recorded is already found on our disk
                switch (_preventDuplicateEpisodesKey)
                {
                  case 1: // Episode Number
                    pastRecordEpisode = pastRecordings[i].SeriesNum + "." + pastRecordings[i].EpisodeNum + "." +
                                        pastRecordings[i].EpisodePart;
                    break;
                  default: // 0 EpisodeName
                    pastRecordEpisode = CleanEpisodeTitle(pastRecordings[i].EpisodeName);
                    break;
                }
                if (pastRecordEpisode.Equals(ToRecordEpisode, StringComparison.CurrentCultureIgnoreCase))
                {
                  // How to handle "interrupted" recordings?
                  // E.g. Windows reboot because of update installation: Previously the tvservice restarted to record the episode 
                  // and simply took care of creating a unique filename.
                  // Now we need to check whether Recording's and Scheduling's Starttime are identical. If they are we expect that
                  // the recording process should be resume because of previous failures.
                  if (pastRecordings[i].StartTime <= newRecording.Program.EndTime.AddMinutes(newRecording.Schedule.PostRecordInterval) &&
                      pastRecordings[i].EndTime >= newRecording.Program.StartTime.AddMinutes(-newRecording.Schedule.PreRecordInterval))
                  {
                    // Check whether the file itself does really exist
                    // There could be faulty drivers 
                    try
                    {
                      // Make sure there's no 1KB file left over (e.g when card fails to tune to channel)
                      FileInfo fi = new FileInfo(pastRecordings[i].FileName);
                      // This will throw an exception if the file is not present
                      if (fi.Length > 4096)
                      {
                        NewRecordingNeeded = false;

                        // Handle schedules so TV Service won't try to re-schedule them every 15 seconds
                        if ((ScheduleRecordingType)newRecording.Schedule.ScheduleType == ScheduleRecordingType.Once)
                        {
                          // One-off schedules can be spawned for some schedule types to record the actual episode
                          // if this is the case then add a cancelled schedule for this episode against the parent
                          int parentScheduleID = newRecording.Schedule.IdParentSchedule;
                          if (parentScheduleID > 0)
                          {                            
                            CancelSchedule(newRecording, parentScheduleID);
                          }

                          
                          IUser user = newRecording.User;
                          _tvController.Fire(this,
                                             new TvServerEventArgs(TvServerEventType.ScheduleDeleted,
                                                                   new VirtualCard(user), (User)user,
                                                                   newRecording.Schedule,
                                                                   null));
                          // now we can safely delete it
                          newRecording.Schedule.Delete();
                        }
                        else
                        {
                          CancelSchedule(newRecording, newRecording.Schedule.IdSchedule);
                        }

                        Log.Info("Scheduler: Schedule {0}-{1} ({2}) has already been recorded ({3}) - aborting...",
                                 newRecording.Program.StartTime.ToString(), ToRecordTitle, ToRecordEpisode,
                                 pastRecordings[i].StartTime.ToString());
                      }
                    }
                    catch (Exception ex)
                    {
                      Log.Error(
                        "Scheduler: Schedule {0} ({1}) has already been recorded but the file is invalid ({2})! Going to record again...",
                        ToRecordTitle, ToRecordEpisode, ex.Message);
                    }
                  }
                  else
                  {
                    Log.Info(
                      "Scheduler: Schedule {0} ({1}) had already been started - expect previous failure and try to resume...",
                      ToRecordTitle, ToRecordEpisode);
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex1)
      {
        Log.Error("Scheduler: Error checking schedule {0} for repeatings {1}", ToRecordTitle, ex1.ToString());
      }
      return NewRecordingNeeded;
    }

    private void CancelSchedule(RecordingDetail newRecording, int scheduleId)
    {
      CanceledSchedule canceled = new CanceledSchedule(scheduleId,
                                                       newRecording.Program.IdChannel,
                                                       newRecording.Program.StartTime);
      canceled.Persist();
      _episodeManagement.OnScheduleEnded(newRecording.FileName, newRecording.Schedule,
                                         newRecording.Program);
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
      TvDatabase.Program current =
        schedule.ReferencedChannel().GetProgramAt(currentTime.AddMinutes(schedule.PreRecordInterval),
                                                  schedule.ProgramName);

      if (current != null)
      {
        // (currentTime.DayOfWeek == schedule.StartTime.DayOfWeek)
        // Log.Debug("Scheduler.cs WeeklyEveryTimeOnThisChannel: {0} {1} current.StartTime.DayOfWeek == schedule.StartTime.DayOfWeek {2} == {3}", schedule.ProgramName, schedule.ReferencedChannel().Name, current.StartTime.DayOfWeek, schedule.StartTime.DayOfWeek);
        if (current.StartTime.DayOfWeek == schedule.StartTime.DayOfWeek)
        {
          if (currentTime >= current.StartTime.AddMinutes(-schedule.PreRecordInterval) &&
              currentTime <= current.EndTime.AddMinutes(schedule.PostRecordInterval))
          {
            if (!schedule.IsSerieIsCanceled(current.StartTime))
            {
              bool createSpawnedOnceSchedule = CreateSpawnedOnceSchedule(schedule, current);
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

      IList<TvDatabase.Program> programs = TvDatabase.Program.RetrieveCurrentRunningByTitle(schedule.ProgramName,
                                                                                            schedule.PreRecordInterval,
                                                                                            schedule.PostRecordInterval);
      foreach (TvDatabase.Program program in programs)
      {
        if (!schedule.IsSerieIsCanceled(program.StartTime))
        {
          if (CreateSpawnedOnceSchedule(schedule, program))
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
      TvDatabase.Program current =
        schedule.ReferencedChannel().GetProgramAt(currentTime.AddMinutes(schedule.PreRecordInterval),
                                                  schedule.ProgramName);

      if (current != null)
      {
        if (currentTime >= current.StartTime.AddMinutes(-schedule.PreRecordInterval) &&
            currentTime <= current.EndTime.AddMinutes(schedule.PostRecordInterval))
        {
          if (!schedule.IsSerieIsCanceled(current.StartTime))
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
      isTimeToRecord = false;
      RecordingDetail newRecording = null;
      newRecording = CreateNewRecordingDetail(schedule, currentTime);
      isTimeToRecord = (newRecording != null);
      return newRecording;
    }

    private RecordingDetail IsTimeToRecordOnce(Schedule schedule, DateTime currentTime, out bool isTimeToRecord)
    {
      isTimeToRecord = false;
      RecordingDetail newRecording = null;
      if (currentTime >= schedule.StartTime.AddMinutes(-schedule.PreRecordInterval) &&
          currentTime <= schedule.EndTime.AddMinutes(schedule.PostRecordInterval))
      {
        VirtualCard vCard = null;
        bool isRecordingSchedule = IsRecordingSchedule(schedule.IdSchedule, out vCard);
        if (!isRecordingSchedule)
        {
          newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), schedule.EndTime, schedule.Series);
          isTimeToRecord = true;
        }
      }
      return newRecording;
    }

    private bool CreateSpawnedOnceSchedule(Schedule schedule, TvDatabase.Program current)
    {
      bool isSpawnedOnceScheduleCreated = false;
      Schedule dbSchedule = Schedule.RetrieveOnce(current.IdChannel, current.Title, current.StartTime,
                                                  current.EndTime);
      if (dbSchedule == null) // not created yet
      {
        Schedule once = Schedule.RetrieveOnce(current.IdChannel, current.Title, current.StartTime, current.EndTime);

        if (once == null) // make sure that we DO NOT create multiple once recordings.
        {
          Schedule newSchedule = new Schedule(schedule);
          newSchedule.IdChannel = current.IdChannel;
          newSchedule.StartTime = current.StartTime;
          newSchedule.EndTime = current.EndTime;
          newSchedule.ScheduleType = 0; // type Once
          newSchedule.Series = true;
          newSchedule.IdParentSchedule = schedule.IdSchedule;
          newSchedule.Persist();
          isSpawnedOnceScheduleCreated = true;
          // 'once typed' created schedule will be used instead at next call of IsTimeToRecord()
        }
      }

      return isSpawnedOnceScheduleCreated;
    }

    private RecordingDetail CreateNewRecordingDetail(Schedule schedule, DateTime currentTime)
    {
      RecordingDetail newRecording = null;
      DateTime start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.StartTime.Hour,
                                    schedule.StartTime.Minute, schedule.StartTime.Second);
      DateTime end = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.EndTime.Hour,
                                  schedule.EndTime.Minute, schedule.EndTime.Second);
      if (start > end)
        end = end.AddDays(1);
      if (currentTime >= start.AddMinutes(-schedule.PreRecordInterval) &&
          currentTime <= end.AddMinutes(schedule.PostRecordInterval))
      {
        if (!schedule.IsSerieIsCanceled(start))
        {
          VirtualCard vCard = null;
          bool isRecordingSchedule = IsRecordingSchedule(schedule.IdSchedule, out vCard);
          if (!isRecordingSchedule)
          {
            newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), end, true);
          }
        }
      }
      return newRecording;
    }

    /// <summary>
    /// Starts recording the recording specified
    /// </summary>
    /// <param name="recording">Recording instance</param>
    /// <returns>true if recording is started, otherwise false</returns>
    private void StartRecord(RecordingDetail RecDetail)
    {
      IUser user = RecDetail.User;

      Log.Write("Scheduler: Time to record {0} {1}-{2} {3}", RecDetail.Channel.DisplayName,
                DateTime.Now.ToShortTimeString(), RecDetail.EndTime.ToShortTimeString(),
                RecDetail.Schedule.ProgramName);
      //get list of all cards we can use todo the recording      
      ICardAllocation allocation = new AdvancedCardAllocation(_layer, _tvController);
      TvResult tvResult;
      List<CardDetail> freeCards = allocation.GetFreeCardsForChannel(_tvController.CardCollection,
                                                                     RecDetail.Channel, ref user, out tvResult);

      int maxCards = 0;

      bool recSucceded = false;
      if (freeCards.Count > 0)
      {
        //Free cards are those cards not being used by anyone
        maxCards = GetMaxCards(freeCards);
        Log.Write("scheduler: try max {0} of {1} FREE cards for recording", maxCards, freeCards.Count);
        recSucceded = FindFreeCardAndStartRecord(RecDetail, user, freeCards, maxCards);
      }
      else
      {
        Log.Write("scheduler: no free cards found for recording.");
      }
      if (!recSucceded)
      {
        List<CardDetail> availCards = allocation.GetAvailableCardsForChannel(_tvController.CardCollection,
                                                                             RecDetail.Channel, ref user);

        if (availCards.Count > 0)
        {
          //Avail cards are ALL cards, even if used by another timeshifting user (not scheduler users).
          //These cards are used as a last option.
          maxCards = GetMaxCards(availCards);
          Log.Write("scheduler: try max {0} of {1} AVAILABLE cards for recording", maxCards, availCards.Count);

          //remove all cards previously cards tried for recording during the "free cards" iteration.
          foreach (var cardDetail in freeCards)
          {
            availCards.RemoveAll(t => t.Id == cardDetail.Id);
          }

          recSucceded = FindAvailCardAndStartRecord(RecDetail, user, availCards, maxCards);
        }
        else
        {
          Log.Write("scheduler: no available cards found for recording.");
        }
      }
    }

    private bool FindAvailCardAndStartRecord(RecordingDetail RecDetail, IUser user, List<CardDetail> cards, int maxCards)
    {
      bool result = false;
      //keep tuning each card until we are succesful                
      for (int i = 0; i < maxCards; i++)
      {
        CardDetail cardInfo = null;
        try
        {
          cardInfo = HijackCardForRecording(RecDetail.Schedule.RecommendedCard, cards);
          result = SetupAndStartRecord(RecDetail, ref user, cardInfo);
          if (result)
          {
            break;
          }
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          StopFailedRecord(RecDetail);
        }
        Log.Write("scheduler: recording failed, lets try next available card.");
        if (cardInfo != null && cards.Contains(cardInfo))
        {
          cards.Remove(cardInfo);
        }
      }
      return result;
    }

    private bool FindFreeCardAndStartRecord(RecordingDetail RecDetail, IUser user, List<CardDetail> cards, int maxCards)
    {
      bool result = false;
      //keep tuning each card until we are succesful                
      for (int i = 0; i < maxCards; i++)
      {
        CardDetail cardInfo = null;
        try
        {
          cardInfo = GetCardInfoForRecording(RecDetail, ref user, cards);
          result = SetupAndStartRecord(RecDetail, ref user, cardInfo);
          if (result)
          {
            break;
          }
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          StopFailedRecord(RecDetail);
        }
        Log.Write("scheduler: recording failed, lets try next available card.");
        if (cardInfo != null && cards.Contains(cardInfo))
        {
          cards.Remove(cardInfo);
        }
      }
      return result;
    }

    private bool SetupAndStartRecord(RecordingDetail RecDetail, ref IUser user, CardDetail cardInfo)
    {
      bool result = false;
      if (cardInfo != null)
      {
        user.CardId = cardInfo.Id;
        StartRecordingNotification(RecDetail);
        SetupRecordingFolder(cardInfo);
        if (StartRecordingOnDisc(RecDetail, ref user, cardInfo))
        {
          CreateRecording(RecDetail);
          try
          {
            RecDetail.User.CardId = user.CardId;
            SetRecordingProgramState(RecDetail);
            _recordingsInProgressList.Add(RecDetail);
            RecordingStartedNotification(RecDetail);
            SetupQualityControl(RecDetail);
            WriteMatroskaFile(RecDetail);
          }
          catch (Exception ex)
          {
            //consume exception, since it isn't catastrophic
            Log.Write(ex);
          }

          Log.Write("Scheduler: recList: count: {0} add scheduleid: {1} card: {2}",
                    _recordingsInProgressList.Count,
                    RecDetail.Schedule.IdSchedule, RecDetail.CardInfo.Card.Name);
          result = true;
        }
      }
      else
      {
        Log.Write("scheduler: no card found to record on.");
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

        if (recording.CardInfo != null && _tvController.SupportsSubChannels(recording.CardInfo.Id) == false)
        {
          _tvController.StopTimeShifting(ref user);
        }

        Log.Write("Scheduler: stop failed record {0} {1}-{2} {3}", recording.Channel.DisplayName,
                  recording.RecordingStartDateTime,
                  recording.EndTime, recording.Schedule.ProgramName);

        if (_tvController.IsRecording(ref user))
        {
          if (_tvController.StopRecording(ref user))
          {
            ResetRecordingStateOnProgram(recording);
            if (recording.Recording != null)
            {
              recording.Recording.Delete();
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
        Log.Write(ex);
      }
    }

    private CardDetail GetCardInfoForRecording(RecordingDetail RecDetail, ref IUser user, List<CardDetail> freeCards)
    {
      CardDetail cardInfo;

      //first try to start recording using the recommended card
      cardInfo = FindRecommendedCard(RecDetail.Schedule.RecommendedCard, freeCards);

      if (cardInfo == null)
      {
        cardInfo = freeCards[0];
      }
      return cardInfo;
    }

    private int GetMaxCards(List<CardDetail> freeCards)
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


    private static CardDetail FindRecommendedCard(int recommendedCard, List<CardDetail> freeCards)
    {
      CardDetail cardInfo = null;
      if (recommendedCard > 0)
      {
        foreach (CardDetail card in freeCards)
        {
          if (card.Id == recommendedCard)
          {
            cardInfo = card;
            Log.Write("Scheduler : record on recommended card:{0} priority:{1}", cardInfo.Id, cardInfo.Card.Priority);
            break;
          }
        }
        if (cardInfo == null)
        {
          Log.Write("Scheduler : recommended card:{0} is not available", recommendedCard);
        }
      }
      return cardInfo;
    }

    private CardDetail HijackCardForRecording(int recommendedCard, List<CardDetail> availableCards)
    {
      CardDetail cardInfo = null;
      if ((_layer.GetSetting("scheduleroverlivetv", "yes").Value == "yes"))
      {
        cardInfo = HijackCardTimeshiftingOnSameTransponder(availableCards, recommendedCard, cardInfo);

        if (cardInfo == null)
        {
          cardInfo = HijackCardTimeshiftingOnDifferentTransponder(availableCards, cardInfo);
        }
        if (cardInfo == null)
        {
          Log.Write("Scheduler : no free card was found and no card was found where user can be kicked.");
        }
      }
      else
      {
        Log.Write("Scheduler : no free card was found and scheduler is not allowed to stop other users LiveTV. ");
      }

      return cardInfo;
    }

    private CardDetail HijackCardTimeshiftingOnDifferentTransponder(List<CardDetail> availableCards, CardDetail cardInfo)
    {
      foreach (CardDetail cardDetail in availableCards)
      {
        if (!cardDetail.SameTransponder)
        {
          IUser[] tmpUsers = _tvController.GetUsersForCard(cardDetail.Id);
          Boolean canKickAll = true;
          for (int i = 0; i < tmpUsers.Length; i++)
          {
            IUser tmpUser = tmpUsers[i];
            if (_tvController.IsRecording(ref tmpUser))
            {
              canKickAll = false;
            }
          }
          if (canKickAll)
          {
            cardInfo = cardDetail;
            Log.Write(
              "Scheduler : card is not tuned to the same transponder and not recording, kicking all users. record on card:{0} priority:{1}",
              cardInfo.Id, cardInfo.Card.Priority);
            for (int i = 0; i < tmpUsers.Length; i++)
            {
              IUser tmpUser = tmpUsers[i];
              if (_tvController.IsTimeShifting(ref tmpUser))
              {
                Log.Write(
                  "Scheduler : kicking user:{0}",
                  tmpUser.Name);
                _tvController.StopTimeShifting(ref tmpUser, TvStoppedReason.RecordingStarted);
              }
              Log.Write(
                "Scheduler : card is tuned to the same transponder but not free. record on card:{0} priority:{1}, kicking user:{2}",
                cardInfo.Id, cardInfo.Card.Priority, tmpUser.Name);
            }
            break;
          }
        }
      }
      return cardInfo;
    }

    private CardDetail HijackCardTimeshiftingOnSameTransponder(List<CardDetail> availableCards, int recommendedCard,
                                                               CardDetail cardInfo)
    {
      availableCards.Sort(new RecordingCardDetailComparer(recommendedCard));
      foreach (CardDetail cardDetail in availableCards)
      {
        if (cardDetail.SameTransponder)
        {
          IUser[] tmpUsers = _tvController.GetUsersForCard(cardDetail.Id);

          for (int i = 0; i < tmpUsers.Length; i++)
          {
            IUser tmpUser = tmpUsers[i];
            if (!_tvController.IsRecording(ref tmpUser))
            {
              if (_tvController.IsTimeShifting(ref tmpUser))
              {
                Log.Write(
                  "Scheduler : card is tuned to the same transponder but not free. record on card:{0} priority:{1}, kicking user:{2}",
                  cardDetail.Id, cardDetail.Card.Priority, tmpUser.Name);
                _tvController.StopTimeShifting(ref tmpUser, TvStoppedReason.RecordingStarted);
              }
              cardInfo = cardDetail;
              break;
            }
          }
          if (cardInfo != null)
          {
            break;
          }
        }
      }
      return cardInfo;
    }


    private void RecordingStartedNotification(RecordingDetail RecDetail)
    {
      IUser user = RecDetail.User;
      _tvController.Fire(this,
                         new TvServerEventArgs(TvServerEventType.RecordingStarted, new VirtualCard(user), (User)user,
                                               RecDetail.Schedule, RecDetail.Recording));
    }

    private void StartRecordingNotification(RecordingDetail RecDetail)
    {
      IUser user = RecDetail.User;
      _tvController.Fire(this,
                         new TvServerEventArgs(TvServerEventType.StartRecording, new VirtualCard(user), (User)user,
                                               RecDetail.Schedule, null));
    }

    private void SetupRecordingFolder(CardDetail cardInfo)
    {
      if (cardInfo.Card.RecordingFolder == String.Empty)
        cardInfo.Card.RecordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                                      Environment.GetFolderPath(
                                                        Environment.SpecialFolder.CommonApplicationData));
      if (cardInfo.Card.TimeShiftFolder == String.Empty)
        cardInfo.Card.TimeShiftFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer",
                                                      Environment.GetFolderPath(
                                                        Environment.SpecialFolder.CommonApplicationData));
    }

    private bool StartRecordingOnDisc(RecordingDetail RecDetail, ref IUser user, CardDetail cardInfo)
    {
      bool startRecordingOnDisc = false;
      _tvController.EpgGrabberEnabled = false;
      Log.Write("Scheduler : record, first tune to channel");
      TvResult tuneResult = _tvController.Tune(ref user, cardInfo.TuningDetail, RecDetail.Channel.IdChannel);

      startRecordingOnDisc = (tuneResult == TvResult.Succeeded);

      if (startRecordingOnDisc)
      {
        if (_tvController.SupportsSubChannels(cardInfo.Card.IdCard) == false)
        {
          Log.Write("Scheduler : record, now start timeshift");
          string timeshiftFileName = String.Format(@"{0}\live{1}-{2}.ts", cardInfo.Card.TimeShiftFolder, cardInfo.Id,
                                                   user.SubChannel);
          startRecordingOnDisc = (TvResult.Succeeded == _tvController.StartTimeShifting(ref user, ref timeshiftFileName));
        }

        if (startRecordingOnDisc)
        {
          RecDetail.MakeFileName(cardInfo.Card.RecordingFolder);
          RecDetail.CardInfo = cardInfo;
          Log.Write("Scheduler : record to {0}", RecDetail.FileName);
          string fileName = RecDetail.FileName;
          startRecordingOnDisc = (TvResult.Succeeded == _tvController.StartRecording(ref user, ref fileName, false, 0));

          if (startRecordingOnDisc)
          {
            RecDetail.FileName = fileName;
            RecDetail.RecordingStartDateTime = DateTime.Now;
          }
        }
      }
      if (!startRecordingOnDisc && _tvController.AllCardsIdle)
      {
        _tvController.EpgGrabberEnabled = true;
      }
      return startRecordingOnDisc;
    }

    private void CreateRecording(RecordingDetail RecDetail)
    {
      int idServer = RecDetail.CardInfo.Card.IdServer;
      Log.Debug(String.Format("Scheduler: adding new row in db for title=\"{0}\" of type=\"{1}\"",
                              RecDetail.Program.Title, RecDetail.Schedule.ScheduleType));
      RecDetail.Recording = new Recording(RecDetail.Schedule.IdChannel, RecDetail.Schedule.IdSchedule, true,
                                          RecDetail.RecordingStartDateTime, DateTime.Now, RecDetail.Program.Title,
                                          RecDetail.Program.Description, RecDetail.Program.Genre, RecDetail.FileName,
                                          RecDetail.Schedule.KeepMethod,
                                          RecDetail.Schedule.KeepDate, 0, idServer, RecDetail.Program.EpisodeName,
                                          RecDetail.Program.SeriesNum, RecDetail.Program.EpisodeNum,
                                          RecDetail.Program.EpisodePart);
      RecDetail.Recording.Persist();
    }

    private void SetRecordingProgramState(RecordingDetail RecDetail)
    {
      if (RecDetail.Program.IdProgram > 0)
      {
        RecDetail.Program.IsRecordingOnce = true;
        RecDetail.Program.IsRecordingSeries = RecDetail.Schedule.Series;
        RecDetail.Program.IsRecordingManual = RecDetail.Schedule.IsManual;
        RecDetail.Program.IsRecordingOncePending = false;
        RecDetail.Program.IsRecordingSeriesPending = false;
        RecDetail.Program.Persist();
      }
    }

    private void SetupQualityControl(RecordingDetail RecDetail)
    {
      IUser user = RecDetail.User;
      int cardId = user.CardId;
      if (_tvController.SupportsQualityControl(cardId))
      {
        if (RecDetail.Schedule.BitRateMode != VIDEOENCODER_BITRATE_MODE.NotSet && _tvController.SupportsBitRate(cardId))
        {
          _tvController.SetQualityType(cardId, RecDetail.Schedule.QualityType);
        }
        if (RecDetail.Schedule.QualityType != QualityType.NotSet && _tvController.SupportsBitRateModes(cardId) &&
            _tvController.SupportsPeakBitRateMode(cardId))
        {
          _tvController.SetBitRateMode(cardId, RecDetail.Schedule.BitRateMode);
        }
      }
    }

    private void WriteMatroskaFile(RecordingDetail RecDetail)
    {
      if (_createTagInfoXML)
      {
        string fileName = RecDetail.FileName;
        MatroskaTagInfo info = new MatroskaTagInfo();
        info.title = RecDetail.Program.Title;
        info.description = RecDetail.Program.Description;
        info.genre = RecDetail.Program.Genre;

        info.channelName = RecDetail.Schedule.ReferencedChannel().DisplayName;
        info.episodeName = RecDetail.Program.EpisodeName;
        info.seriesNum = RecDetail.Program.SeriesNum;
        info.episodeNum = RecDetail.Program.EpisodeNum;
        info.episodePart = RecDetail.Program.EpisodePart;
        info.startTime = RecDetail.RecordingStartDateTime;
        info.endTime = RecDetail.EndTime;

        MatroskaTagHandler.WriteTag(System.IO.Path.ChangeExtension(fileName, ".xml"), info);
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

        if (_tvController.SupportsSubChannels(recording.CardInfo.Id) == false)
        {
          _tvController.StopTimeShifting(ref user);
        }

        Log.Write("Scheduler: stop record {0} {1}-{2} {3}", recording.Channel.DisplayName,
                  recording.RecordingStartDateTime,
                  recording.EndTime, recording.Schedule.ProgramName);

        if (_tvController.StopRecording(ref user))
        {
          ResetRecordingState(recording);
          ResetRecordingStateOnProgram(recording);
          _recordingsInProgressList.Remove(recording); //only remove recording from the list, if we are succesfull

          if ((ScheduleRecordingType)recording.Schedule.ScheduleType == ScheduleRecordingType.Once)
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
        Log.Write(ex);
      }
    }

    private void RetryStopRecord(RecordingDetail recording)
    {
      Log.Write("Scheduler: stop record did not succeed (trying again in 1 min.) {0} {1}-{2} {3}",
                recording.Channel.DisplayName, recording.RecordingStartDateTime, recording.EndTime,
                recording.Schedule.ProgramName);
      recording.Recording.EndTime = recording.Recording.EndTime.AddMinutes(1);
      //lets try and stop the recording in 1 min. again.
      recording.Recording.Persist();
    }

    private void RecordingEndedNotification(RecordingDetail recording)
    {
      IUser user = recording.User;
      _tvController.Fire(this,
                         new TvServerEventArgs(TvServerEventType.RecordingEnded, new VirtualCard(user), (User)user,
                                               recording.Schedule, recording.Recording));
    }

    private void StopRecordOnSeriesSchedule(RecordingDetail recording)
    {
      Log.Debug("Scheduler: endtime={0}, Program.EndTime={1}, postRecTime={2}", recording.EndTime,
                recording.Program.EndTime, recording.Schedule.PostRecordInterval);
      if (DateTime.Now <= recording.Program.EndTime.AddMinutes(recording.Schedule.PostRecordInterval))
      {
        CancelSchedule(recording, recording.Schedule.IdSchedule);
      }
      else
      {
        _episodeManagement.OnScheduleEnded(recording.FileName, recording.Schedule, recording.Program);
      }
    }

    private void StopRecordOnOnceSchedule(RecordingDetail recording)
    {
      IUser user = recording.User;
      if (recording.IsSerie)
      {
        _episodeManagement.OnScheduleEnded(recording.FileName, recording.Schedule, recording.Program);
      }
      _tvController.Fire(this,
                         new TvServerEventArgs(TvServerEventType.ScheduleDeleted, new VirtualCard(user), (User)user,
                                               recording.Schedule, null));
      // now we can safely delete it
      recording.Schedule.Delete();
    }

    private void ResetRecordingState(RecordingDetail recording)
    {
      try
      {
        recording.Recording.Refresh();
        recording.Recording.EndTime = DateTime.Now;
        recording.Recording.IsRecording = false;
        recording.Recording.Persist();
      }
      catch (Exception ex)
      {
        Log.Error("StopRecord - updating record id={0} failed {1}", recording.Recording.IdRecording, ex.StackTrace);
      }
    }

    private void ResetRecordingStateOnProgram(RecordingDetail recording)
    {
      if (recording.Program.IdProgram > 0)
      {
        recording.Program.IsRecordingManual = false;
        recording.Program.IsRecordingSeries = false;
        recording.Program.IsRecordingOnce = false;
        recording.Program.IsRecordingOncePending = false;
        recording.Program.IsRecordingSeriesPending = false;
        recording.Program.Persist();
      }
    }

    private class RecordingCardDetailComparer : IComparer<CardDetail>
    {
      private readonly int _recommendedCard;

      public RecordingCardDetailComparer(int recommendedCard)
      {
        _recommendedCard = recommendedCard;
      }

      public int Compare(CardDetail first, CardDetail second)
      {
        if (first == second)
          return 0;

        if (first.Id == _recommendedCard)
        {
          return -1;
        }

        if (second.Id == _recommendedCard)
        {
          return 1;
        }
        return first.CompareTo(second);
      }
    }

    #endregion
  }
}