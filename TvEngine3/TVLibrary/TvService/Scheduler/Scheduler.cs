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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using TvControl;
using TvDatabase;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvService
{
  /// <summary>
  /// Scheduler class.
  /// This class will take care of recording all schedules in the database
  /// </summary>
  public class Scheduler
  {
    #region const

    private const int ScheduleInterval = 15;

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
    private IController _controller;
    private readonly TVController _tvController;
    private bool _reEntrant;
    private readonly System.Timers.Timer _timer = new System.Timers.Timer();
    private DateTime _scheduleCheckTimer;
    private List<RecordingDetail> _recordingsInProgressList;
    private User _user;
    private bool _createTagInfoXML;
    private bool _preventDuplicateEpisodes;
    private int _preventDuplicateEpisodesKey;

    #endregion

    #region ctor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="controller">instance of the TVController</param>
    public Scheduler(TVController controller)
    {
      _user = new User("Scheduler", true);
      _tvController = controller;
      TvBusinessLayer layer = new TvBusinessLayer();
      _createTagInfoXML = (layer.GetSetting("createtaginfoxml", "yes").Value == "yes");
      _preventDuplicateEpisodes = (layer.GetSetting("PreventDuplicates", "no").Value == "yes");
      _preventDuplicateEpisodesKey = Convert.ToInt32(layer.GetSetting("EpisodeKey", "0").Value);
    }

    #endregion

    #region public members

    /// <summary>
    /// Resets the scheduler timer. This causes the scheduler to immediatly check
    /// if any schedule should be recorded
    /// </summary>
    public void ResetTimer()
    {
      _scheduleCheckTimer = DateTime.MinValue;
    }

    /// <summary>
    /// Starts the scheduler
    /// </summary>
    public void Start()
    {
      Log.Write("Scheduler: started");
      _controller = RemoteControl.Instance;

      //ResetEPGprogramStateData();
      ResetRecordingStates();

      _recordingsInProgressList = new List<RecordingDetail>();
      IList<Schedule> schedules = Schedule.ListAll();
      Log.Write("Scheduler: loaded {0} schedules", schedules.Count);
      _scheduleCheckTimer = DateTime.MinValue;
      _timer.Interval = 1000;
      _timer.Enabled = true;
      _timer.Elapsed += timer_Elapsed;

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
      _timer.Enabled = false;

      //ResetEPGprogramStateData();
      ResetRecordingStates();

      _episodeManagement = null;
      _recordingsInProgressList = new List<RecordingDetail>();
      HandleSleepMode();
    }

    #endregion

    #region private members

    /*private static void ResetEPGprogramStateData()
    {
      TvDatabase.Program.ResetStates();
    }*/

    private static void ResetRecordingStates()
    {
      Recording.ResetActiveRecordings();
    }

    /// <summary>
    /// Timer callback which gets fired every 30 seconds
    /// The method will check if a schedule should be started to record
    /// and/or if a recording which is currently being recorded should be stopped
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      //security check, dont allow re-entrancy here
      if (_reEntrant)
        return;

      try
      {
        string threadname = Thread.CurrentThread.Name;
        if (string.IsNullOrEmpty(threadname))
          Thread.CurrentThread.Name = "Scheduler timer";
      }
      catch (InvalidOperationException) {}

      try
      {
        _reEntrant = true;
        TimeSpan ts = DateTime.Now - _scheduleCheckTimer;
        if (ts.TotalSeconds < ScheduleInterval)
          return;

        HandleRecordingList();
        DoSchedule();
        HandleSleepMode();
        _scheduleCheckTimer = DateTime.Now;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        _reEntrant = false;
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
    /// DoSchedule() will start recording any schedule if its time todo so
    /// </summary>
    private void DoSchedule()
    {
      IList<Schedule> schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        //Log.Debug("Checking: schedule {0}, type {1}", schedule.ProgramName, schedule.ScheduleType);
        //if schedule has been canceled then do nothing
        if (schedule.Canceled != Schedule.MinSchedule)
          continue;

        //if we are already recording this schedule then do nothing
        VirtualCard card;
        if (IsRecordingSchedule(schedule.IdSchedule, out card))
          continue;

        //check if this series is canceled
        DateTime now = DateTime.Now;
        if (schedule.IsSerieIsCanceled(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)))
          continue;

        //check if its time to record this schedule.
        RecordingDetail newRecording;
        if (IsTimeToRecord(schedule, now, out newRecording))
        {
          //yes - let's check whether this file is already present and therefore doesn't need to be recorded another time
          if (IsEpisodeUnrecorded(schedule, newRecording))
          {
            StartRecord(newRecording);
          }
        }

        CheckAndDeleteOrphanedRecordings();
      }
    }

    private bool IsEpisodeUnrecorded(Schedule schedule, RecordingDetail newRecording)
    {
      bool NewRecordingNeeded = true;
      //cleanup: remove EPG additions of Clickfinder plugin
      string ToRecordTitle = CleanEpisodeTitle(newRecording.Program.Title);
      string ToRecordEpisode = "";
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
      try
      {
        // Allow user to turn this on or off in case of unreliable EPG
        if (_preventDuplicateEpisodes)
        {
          IList<Recording> pastRecordings = Recording.ListAll();
          Log.Debug("Scheduler: Check recordings for schedule {0}...", ToRecordTitle);
          // EPG needs to have episode information to distinguish between repeatings and new broadcasts
          if (ToRecordEpisode.Equals(String.Empty))
          {
            // Check the type so we aren't logging too verbose on single runs
            if (schedule.ScheduleType != (int)ScheduleRecordingType.Once)
            {
              Log.Info("Scheduler: No epsisode title found for schedule {0} - omitting repeating check.",
                       newRecording.Program.Title);
            }
          }
          else
          {
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
                  if (pastRecordings[i].StartTime.Date != newRecording.Program.StartTime.Date)
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
                          // even for Once type , the schedule can initially be a serie
                          if (newRecording.IsSerie)
                          {
                            _episodeManagement.OnScheduleEnded(newRecording.FileName, newRecording.Schedule,
                                                               newRecording.Program);
                          }
                          _tvController.Fire(this,
                                             new TvServerEventArgs(TvServerEventType.ScheduleDeleted,
                                                                   new VirtualCard(_user), _user, newRecording.Schedule,
                                                                   null));
                          // now we can safely delete it
                          newRecording.Schedule.Delete();
                        }
                        else
                        {
                          CanceledSchedule canceled = new CanceledSchedule(newRecording.Schedule.IdSchedule,
                                                                           newRecording.Program.StartTime);
                          canceled.Persist();
                          _episodeManagement.OnScheduleEnded(newRecording.FileName, newRecording.Schedule,
                                                             newRecording.Program);
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

    /// <summary>
    /// HandleRecordingList() will stop any recording which should be stopped
    /// </summary>
    private void HandleRecordingList()
    {
      while (true)
      {
        bool removed = false;
        foreach (RecordingDetail recording in _recordingsInProgressList)
        {
          if (!recording.IsRecording)
          {
            StopRecord(recording);
            removed = true;
            break;
          }
        }

        if (removed == false)
        {
          return;
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
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        return;
      }

      if (_recordingsInProgressList.Count > 0)
      {
        //disable the sleep timer
        SetThreadExecutionState(
          (EXECUTION_STATE)((uint)EXECUTION_STATE.ES_CONTINUOUS + (uint)EXECUTION_STATE.ES_SYSTEM_REQUIRED));
      }
      else
      {
        //enable the sleep timer
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
      }
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
    /// Method which checks if its time to record the schedule specified
    /// </summary>
    /// <param name="schedule">Schedule</param>
    /// <param name="currentTime">current Date/Time</param>
    /// <param name="newRecording">Recording detail which is used to further process the recording</param>
    /// <returns>true if schedule should be recorded now, else false</returns>
    private bool IsTimeToRecord(Schedule schedule, DateTime currentTime, out RecordingDetail newRecording)
    {
      newRecording = null;
      ScheduleRecordingType type = (ScheduleRecordingType)schedule.ScheduleType;
      if (type == ScheduleRecordingType.Once)
      {
        if (currentTime >= schedule.StartTime.AddMinutes(-schedule.PreRecordInterval) &&
            currentTime <= schedule.EndTime.AddMinutes(schedule.PostRecordInterval))
        {
          newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), schedule.EndTime, schedule.Series);
          //Log.Debug("Scheduler: Recording {0} added to _recordingsInProgressList, isSerie={1}", schedule.ProgramName, schedule.Series);
          return true;
        }
        return false;
      }

      if (type == ScheduleRecordingType.Daily)
      {
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
            newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), end, true);
            return true;
          }
        }
        return false;
      }

      if (type == ScheduleRecordingType.Weekends)
      {
        WeekEndTool weekEndTool = Setting.GetWeekEndTool();
        if (weekEndTool.IsWeekend(currentTime.DayOfWeek))
        {
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
              newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), end, true);
              return true;
            }
          }
        }
        return false;
      }
      if (type == ScheduleRecordingType.WorkingDays)
      {
        WeekEndTool weekEndTool = Setting.GetWeekEndTool();
        if (weekEndTool.IsWorkingDay(currentTime.DayOfWeek))
        {
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
              newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), end, true);
              return true;
            }
          }
        }
        return false;
      }

      if (type == ScheduleRecordingType.Weekly)
      {
        if ((currentTime.DayOfWeek == schedule.StartTime.DayOfWeek) && (currentTime.Date >= schedule.StartTime.Date))
        {
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
              newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), end, true);
              return true;
            }
          }
        }
        return false;
      }

      if (type == ScheduleRecordingType.EveryTimeOnThisChannel)
      {
        TvDatabase.Program current =
          schedule.ReferencedChannel().GetProgramAt(currentTime.AddMinutes(schedule.PreRecordInterval));
        if (current != null)
        {
          if (currentTime >= current.StartTime.AddMinutes(-schedule.PreRecordInterval) &&
              currentTime <= current.EndTime.AddMinutes(schedule.PostRecordInterval))
          {
            // same title 
            if (String.Compare(current.Title, schedule.ProgramName, true) == 0)
            {
              if (!schedule.IsSerieIsCanceled(current.StartTime))
              {
                Schedule dbSchedule = Schedule.RetrieveOnce(current.IdChannel, current.Title, current.StartTime,
                                                            current.EndTime);
                if (dbSchedule == null) // not created yet
                {
                  Schedule newSchedule = new Schedule(schedule);
                  newSchedule.StartTime = current.StartTime;
                  newSchedule.EndTime = current.EndTime;
                  newSchedule.ScheduleType = 0; // type Once
                  newSchedule.Series = true;
                  newSchedule.IdParentSchedule = schedule.IdSchedule;
                  newSchedule.Persist();
                  return false; // 'once typed' created schedule will be used instead at next call of IsTimeToRecord()
                }
              }
            }
          }
        }
        return false;
      }

      if (type == ScheduleRecordingType.EveryTimeOnEveryChannel)
      {
        IList<TvDatabase.Program> programs = TvDatabase.Program.RetrieveCurrentRunningByTitle(schedule.ProgramName,
                                                                                              schedule.PreRecordInterval,
                                                                                              schedule.
                                                                                                PostRecordInterval);
        foreach (TvDatabase.Program program in programs)
        {
          if (!schedule.IsSerieIsCanceled(program.StartTime))
          {
            Schedule dbSchedule = Schedule.RetrieveOnce(program.IdChannel, program.Title, program.StartTime,
                                                        program.EndTime);
            if (dbSchedule == null) // not created yet
            {
              Schedule newSchedule = new Schedule(schedule);
              newSchedule.IdChannel = program.IdChannel;
              newSchedule.StartTime = program.StartTime;
              newSchedule.EndTime = program.EndTime;
              newSchedule.ScheduleType = 0; // type Once
              newSchedule.IdParentSchedule = schedule.IdSchedule;
              newSchedule.Series = true;
              newSchedule.Persist();
              return false; // 'once typed' created schedule will be used instead at next call of IsTimeToRecord()
            }
          }
        }
        return false;
      }
      return false;
    }

    /// <summary>
    /// Starts recording the recording specified
    /// </summary>
    /// <param name="recording">Recording instance</param>
    /// <returns>true if recording is started, otherwise false</returns>
    private void StartRecord(RecordingDetail RecDetail)
    {
      _user.Name = string.Format("scheduler{0}", RecDetail.Schedule.IdSchedule);
      _user.CardId = -1;
      _user.SubChannel = -1;
      _user.IsAdmin = true;
      Log.Write("Scheduler: Time to record {0} {1}-{2} {3}", RecDetail.Channel.DisplayName,
                DateTime.Now.ToShortTimeString(), RecDetail.EndTime.ToShortTimeString(), RecDetail.Schedule.ProgramName);
      TvResult result;
      //get list of all cards we can use todo the recording
      ICardAllocation allocation = new AdvancedCardAllocation(); //CardAllocationFactory.Create(false);
      List<CardDetail> freeCards = allocation.GetAvailableCardsForChannel(_tvController.CardCollection,
                                                                          RecDetail.Channel, ref _user, false,
                                                                          out result, RecDetail.Schedule.RecommendedCard);
      if (freeCards.Count == 0)
        return;

      CardDetail cardInfo = null;

      //first try to start recording using the recommended card
      if (RecDetail.Schedule.RecommendedCard > 0)
      {
        foreach (CardDetail card in freeCards)
        {
          User byUser;
          bool isCardInUse = _tvController.IsCardInUse(card.Id, out byUser);
          //if (card.Id != recording.Schedule.RecommendedCard) continue;
          // now the card allocator handles the recommenedcard.

          //when card is idle we can use it
          //when card is busy, but already tuned to the correct channel then we can use it also
          //when card is busy, but already tuned to the correct transponder then we can use it also          
          User tmpUser = _tvController.GetUserForCard(card.Id);
            //added by joboehl - Allows the CurrentDbChannel bellow to work when TVServer and client are on different machines
          if ((isCardInUse == false) ||
              _tvController.CurrentDbChannel(ref tmpUser) == RecDetail.Channel.IdChannel ||
              (_tvController.IsTunedToTransponder(card.Id, card.TuningDetail)))
          {
            // use the recommended card.
            cardInfo = card;
            Log.Write("Scheduler : record on recommended card:{0} priority:{1}", cardInfo.Id, cardInfo.Card.Priority);
            break;
          }
        }
        if (cardInfo == null)
        {
          Log.Write("Scheduler : recommended card:{0} is not available", RecDetail.Schedule.RecommendedCard);
        }
      }
      if (cardInfo == null)
      {
        //first try, find a card which is already tuned to the correct transponder
        foreach (CardDetail card in freeCards)
        {
          if (_tvController.IsTunedToTransponder(card.Id, card.TuningDetail))
          {
            cardInfo = card;
            Log.Write("Scheduler : record on free card:{0} priority:{1}", cardInfo.Id, cardInfo.Card.Priority);
            break;
          }
        }
      }

      if (cardInfo == null)
      {
        //first try, find a free card
        foreach (CardDetail card in freeCards)
        {
          User byUser;
          if ((_tvController.IsCardInUse(card.Id, out byUser) == false))
          {
            cardInfo = card;
            Log.Write("Scheduler : record on free card:{0} priority:{1}", cardInfo.Id, cardInfo.Card.Priority);
            break;
          }
        }
      }

      if (cardInfo == null)
      {
        Log.Write("Scheduler : all cards busy, check if any card is already tuned to channel:{0}",
                  RecDetail.Channel.Name);
        //all cards in use, check if a card is already tuned to the channel we want to record
        foreach (CardDetail card in freeCards)
        {
          User tmpUser = _tvController.GetUserForCard(card.Id);
            //added by joboehl - Allows the CurrentDbChannel bellow to work when TVServer and client are on different machines
          if (_tvController.CurrentDbChannel(ref tmpUser) == RecDetail.Channel.IdChannel)
          {
            if (_tvController.IsRecording(ref tmpUser) == false)
            {
              cardInfo = card;
              Log.Write("Scheduler : record on card:{0} priority:{1} which is tuned to {2}", cardInfo.Id,
                        cardInfo.Card.Priority, RecDetail.Channel.DisplayName);
              break;
            }
          }
        }
      }

      if (cardInfo == null)
      {
        //all cards in use, no card tuned to the channel, use the first one.
        TvBusinessLayer layer = new TvBusinessLayer();
        User tmpUser = _tvController.GetUserForCard(freeCards[0].Id);
        if ((_tvController.IsRecording(ref tmpUser) == false) &&
            (layer.GetSetting("scheduleroverlivetv", "yes").Value == "yes"))
        {
          if (_tvController.IsTimeShifting(ref tmpUser))
          {
            _tvController.StopTimeShifting(ref tmpUser, TvStoppedReason.RecordingStarted);
          }
          cardInfo = freeCards[0];
          Log.Write(
            "Scheduler : no card is tuned to the correct channel. record on card:{0} priority:{1}, kicking user:{2}",
            cardInfo.Id, cardInfo.Card.Priority, tmpUser.Name);
        }
        else
        {
          Log.Write("Scheduler : no card was found and scheduler not allowed to stop other users LiveTV or recordings. ");
          return;
        }
      }

      try
      {
        _user.CardId = cardInfo.Id;

        _tvController.Fire(this,
                           new TvServerEventArgs(TvServerEventType.StartRecording, new VirtualCard(_user), _user,
                                                 RecDetail.Schedule, null));

        if (cardInfo.Card.RecordingFolder == String.Empty)
          cardInfo.Card.RecordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                                        Environment.GetFolderPath(
                                                          Environment.SpecialFolder.CommonApplicationData));
        if (cardInfo.Card.TimeShiftFolder == String.Empty)
          cardInfo.Card.TimeShiftFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer",
                                                        Environment.GetFolderPath(
                                                          Environment.SpecialFolder.CommonApplicationData));

        Log.Write("Scheduler : record, first tune to channel");
        TvResult tuneResult = _controller.Tune(ref _user, cardInfo.TuningDetail, RecDetail.Channel.IdChannel);
        if (tuneResult != TvResult.Succeeded)
        {
          return;
        }

        if (_controller.SupportsSubChannels(cardInfo.Card.IdCard) == false)
        {
          Log.Write("Scheduler : record, now start timeshift");
          string timeshiftFileName = String.Format(@"{0}\live{1}-{2}.ts", cardInfo.Card.TimeShiftFolder, cardInfo.Id,
                                                   _user.SubChannel);
          if (TvResult.Succeeded != _controller.StartTimeShifting(ref _user, ref timeshiftFileName))
          {
            return;
          }
        }

        RecDetail.MakeFileName(cardInfo.Card.RecordingFolder);
        RecDetail.CardInfo = cardInfo;
        Log.Write("Scheduler : record to {0}", RecDetail.FileName);
        string fileName = RecDetail.FileName;
        if (TvResult.Succeeded != _controller.StartRecording(ref _user, ref fileName, false, 0))
        {
          return;
        }
        RecDetail.FileName = fileName;
        RecDetail.RecordingStartDateTime = DateTime.Now;
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


        if (RecDetail.Program.IdProgram > 0)
        {
          RecDetail.Program.IsRecordingOnce = true;
          RecDetail.Program.IsRecordingSeries = RecDetail.Schedule.Series;
          RecDetail.Program.IsRecordingManual = RecDetail.Schedule.IsManual;
          RecDetail.Program.IsRecordingOncePending = false;
          RecDetail.Program.IsRecordingSeriesPending = false;
          RecDetail.Program.Persist();
        }

        _recordingsInProgressList.Add(RecDetail);

        _tvController.Fire(this,
                           new TvServerEventArgs(TvServerEventType.RecordingStarted, new VirtualCard(_user), _user,
                                                 RecDetail.Schedule, RecDetail.Recording));
        int cardId = _user.CardId;
        if (_controller.SupportsQualityControl(cardId))
        {
          if (RecDetail.Schedule.BitRateMode != VIDEOENCODER_BITRATE_MODE.NotSet && _controller.SupportsBitRate(cardId))
          {
            _controller.SetQualityType(cardId, RecDetail.Schedule.QualityType);
          }
          if (RecDetail.Schedule.QualityType != QualityType.NotSet && _controller.SupportsBitRateModes(cardId) &&
              _controller.SupportsPeakBitRateMode(cardId))
          {
            _controller.SetBitRateMode(cardId, RecDetail.Schedule.BitRateMode);
          }
        }
        if (_createTagInfoXML)
        {
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
        Log.Write("Scheduler: recList: count: {0} add scheduleid: {1} card: {2}", _recordingsInProgressList.Count,
                  RecDetail.Schedule.IdSchedule, RecDetail.CardInfo.Card.Name);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return;
    }

    /// <summary>
    /// stops recording the specified recording 
    /// </summary>
    /// <param name="recording">Recording</param>
    private void StopRecord(RecordingDetail recording)
    {
      try
      {
        _user.CardId = recording.CardInfo.Id;
        _user.Name = string.Format("scheduler{0}", recording.Schedule.IdSchedule);
        _user.IsAdmin = true;

        if (_controller.SupportsSubChannels(recording.CardInfo.Id) == false)
        {
          _controller.StopTimeShifting(ref _user);
        }

        Log.Write("Scheduler: stop record {0} {1}-{2} {3}", recording.Channel.Name, recording.RecordingStartDateTime,
                  recording.EndTime, recording.Schedule.ProgramName);
        if (_controller.StopRecording(ref _user))
        {
          recording.Recording.Refresh();
          recording.Recording.EndTime = DateTime.Now;
          recording.Recording.IsRecording = false;
          recording.Recording.Persist();

          if (recording.Program.IdProgram > 0)
          {
            recording.Program.IsRecordingManual = false;
            recording.Program.IsRecordingSeries = false;
            recording.Program.IsRecordingOnce = false;
            recording.Program.IsRecordingOncePending = false;
            recording.Program.IsRecordingSeriesPending = false;
            recording.Program.Persist();
          }

          if ((ScheduleRecordingType)recording.Schedule.ScheduleType == ScheduleRecordingType.Once)
          {
            // even for Once type , the schedule can initially be a serie
            if (recording.IsSerie)
            {
              _episodeManagement.OnScheduleEnded(recording.FileName, recording.Schedule, recording.Program);
            }
            _tvController.Fire(this,
                               new TvServerEventArgs(TvServerEventType.ScheduleDeleted, new VirtualCard(_user), _user,
                                                     recording.Schedule, null));
            // now we can safely delete it
            recording.Schedule.Delete();
          }
          else
          {
            Log.Debug("Scheduler: endtime={0}, Program.EndTime={1}, postRecTime={2}", recording.EndTime,
                      recording.Program.EndTime, recording.Schedule.PostRecordInterval);
            if (DateTime.Now <= recording.Program.EndTime.AddMinutes(recording.Schedule.PostRecordInterval))
            {
              CanceledSchedule canceled = new CanceledSchedule(recording.Schedule.IdSchedule,
                                                               recording.Program.StartTime);
              canceled.Persist();
            }
            _episodeManagement.OnScheduleEnded(recording.FileName, recording.Schedule, recording.Program);
          }

          _recordingsInProgressList.Remove(recording); //only remove recording from the list, if we are succesfull

          _tvController.Fire(this,
                             new TvServerEventArgs(TvServerEventType.RecordingEnded, new VirtualCard(_user), _user,
                                                   recording.Schedule, recording.Recording));
        }
        else
        {
          Log.Write("Scheduler: stop record did not succeed (trying again in 1 min.) {0} {1}-{2} {3}",
                    recording.Channel.Name, recording.RecordingStartDateTime, recording.EndTime,
                    recording.Schedule.ProgramName);
          recording.Recording.EndTime = recording.Recording.EndTime.AddMinutes(1);
            //lets try and stop the recording in 1 min. again.
          recording.Recording.Persist();
        }
        //DatabaseManager.Instance.SaveChanges();        		        
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
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
          User user = new User();
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

      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
        if (rec.Schedule.IdSchedule == idSchedule)
        {
          StopRecord(rec);
          return;
        }
      }
      return;
    }

    /// <summary>
    /// Method which returns the database schedule id for the card
    /// </summary>
    /// <param name="cardId">id of the card</param>
    /// <param name="ChannelId">Channel id</param>
    /// <returns>id of schedule the card is recording or -1 if its not recording</returns>
    public int GetRecordingScheduleForCard(int cardId, int ChannelId)
    {
      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
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
      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
        if (rec.CardInfo.Id == cardId)
        {
          StopRecord(rec);
          return;
        }
      }
    }

    #endregion
  }
}