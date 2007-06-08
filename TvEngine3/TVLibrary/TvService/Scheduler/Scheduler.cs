/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using TvLibrary.Log;
using TvDatabase;
using TvControl;
using TvEngine.Events;
using System.Threading;

namespace TvService
{
  /// <summary>
  /// Scheduler class.
  /// This class will take care of recording all schedules in the database
  /// </summary>
  public class Scheduler
  {

    #region const
    const int ScheduleInterval = 15;
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
    private extern static EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE state);
    #endregion

    #region variables
    DiskManagement _diskManagement;
    EpisodeManagement _episodeManagement;
    RecordingManagement _recordingManagement;
    IController _controller;
    TVController _tvController;
    bool _reEntrant = false;
    System.Timers.Timer _timer = new System.Timers.Timer();
    DateTime _scheduleCheckTimer;
    List<RecordingDetail> _recordingsInProgressList;
    IList _channels;
    User _user;
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
      _recordingsInProgressList = new List<RecordingDetail>();
      _channels = Channel.ListAll();
      IList schedules = Schedule.ListAll();
      Log.Write("Scheduler: loaded {0} schedules", schedules.Count);
      _scheduleCheckTimer = DateTime.MinValue;
      _timer.Interval = 1000;
      _timer.Enabled = true;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

      _diskManagement = new DiskManagement();
      _recordingManagement = new RecordingManagement();
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

      _diskManagement = null;
      _recordingManagement = null;
      _episodeManagement = null;
      _recordingsInProgressList = new List<RecordingDetail>();
      HandleSleepMode();

    }
    #endregion

    #region private members
    /// <summary>
    /// Timer callback which gets fired every 30 seconds
    /// The method will check if a schedule should be started to record
    /// and/or if a recording which is currently being recorded should be stopped
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      //security check, dont allow re-entrancy here
      if (_reEntrant) return;

      try
      {
        _reEntrant = true;
        TimeSpan ts = DateTime.Now - _scheduleCheckTimer;
        if (ts.TotalSeconds < ScheduleInterval) return;

        DoSchedule();
        HandleRecordingList();
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

    /// <summary>
    /// DoSchedule() will start recording any schedule if its time todo so
    /// </summary>
    void DoSchedule()
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      DateTime now = DateTime.Now;
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        //if schedule has been canceled then do nothing
        if (schedule.Canceled != Schedule.MinSchedule) continue;

        //if we are already recording this schedule then do nothing
        VirtualCard card;
        if (IsRecordingSchedule(schedule.IdSchedule, out card)) continue;

        //check if this series is canceled
        if (schedule.IsSerieIsCanceled(now)) continue;

        //check if its time to record this schedule.
        RecordingDetail newRecording;
        if (IsTimeToRecord(schedule, now, out newRecording))
        {
          //yes, then lets recording it
          StartRecord(newRecording);
        }
      }
    }

    /// <summary>
    /// HandleRecordingList() will stop any recording which should be stopped
    /// </summary>
    void HandleRecordingList()
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
    void HandleSleepMode()
    {
      if (_recordingsInProgressList == null)
      {
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        return;
      }

      if (_recordingsInProgressList.Count > 0)
      {
        //disable the sleep timer
        SetThreadExecutionState((EXECUTION_STATE)((uint)EXECUTION_STATE.ES_CONTINUOUS + (uint)EXECUTION_STATE.ES_SYSTEM_REQUIRED));
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
      TvBusinessLayer layer = new TvBusinessLayer();

      DateTime now = DateTime.Now;
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        //if schedule has been canceled then do nothing
        if (schedule.Canceled != Schedule.MinSchedule) continue;

        //check if its time to record this schedule.
        RecordingDetail newRecording;
        if (IsTimeToRecord(schedule, now, out newRecording))
        {
          return true;
        }
      }
      return false;
    }


    /// <summary>
    /// Method which checks if its time to record the schedule specified
    /// </summary>
    /// <param name="schedule">Schedule</param>
    /// <param name="currentTime">current Date/Time</param>
    /// <param name="newRecording">Recording detail which is used to further process the recording</param>
    /// <returns>true if schedule should be recorded now, else false</returns>
    bool IsTimeToRecord(Schedule schedule, DateTime currentTime, out RecordingDetail newRecording)
    {
      newRecording = null;
      ScheduleRecordingType type = (ScheduleRecordingType)schedule.ScheduleType;
      if (type == ScheduleRecordingType.Once)
      {
        if (currentTime >= schedule.StartTime.AddMinutes(-schedule.PreRecordInterval) &&
            currentTime <= schedule.EndTime.AddMinutes(schedule.PostRecordInterval))
        {

          newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), schedule.StartTime, schedule.EndTime);
          return true;
        }
        return false;
      }

      if (type == ScheduleRecordingType.Daily)
      {
        DateTime start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, schedule.StartTime.Second);
        DateTime end = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.EndTime.Hour, schedule.EndTime.Minute, schedule.EndTime.Second);
        if (currentTime >= start.AddMinutes(-schedule.PreRecordInterval) &&
            currentTime <= end.AddMinutes(schedule.PostRecordInterval))
        {
          if (!schedule.IsSerieIsCanceled(start))
          {
            newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), start, end);
            return true;
          }
        }
        return false;
      }

      if (type == ScheduleRecordingType.Weekends)
      {
        if (currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday)
        {
          DateTime start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, schedule.StartTime.Second);
          DateTime end = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.EndTime.Hour, schedule.EndTime.Minute, schedule.EndTime.Second);
          if (currentTime >= start.AddMinutes(-schedule.PreRecordInterval) &&
              currentTime <= end.AddMinutes(schedule.PostRecordInterval))
          {

            if (!schedule.IsSerieIsCanceled(start))
            {
              newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), start, end);
              return true;
            }
          }
        }
        return false;
      }
      if (type == ScheduleRecordingType.WorkingDays)
      {
        if (currentTime.DayOfWeek != DayOfWeek.Saturday && currentTime.DayOfWeek != DayOfWeek.Sunday)
        {
          DateTime start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, schedule.StartTime.Second);
          DateTime end = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.EndTime.Hour, schedule.EndTime.Minute, schedule.EndTime.Second);
          if (currentTime >= start.AddMinutes(-schedule.PreRecordInterval) &&
              currentTime <= end.AddMinutes(schedule.PostRecordInterval))
          {
            if (!schedule.IsSerieIsCanceled(start))
            {
              newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), start, end);
              return true;
            }
          }
        }
        return false;
      }

      if (type == ScheduleRecordingType.Weekly)
      {
        if (currentTime.DayOfWeek == schedule.StartTime.DayOfWeek)
        {
          DateTime start = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, schedule.StartTime.Second);
          DateTime end = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, schedule.EndTime.Hour, schedule.EndTime.Minute, schedule.EndTime.Second);
          if (currentTime >= start.AddMinutes(-schedule.PreRecordInterval) &&
              currentTime <= end.AddMinutes(schedule.PostRecordInterval))
          {
            if (!schedule.IsSerieIsCanceled(start))
            {
              newRecording = new RecordingDetail(schedule, schedule.ReferencedChannel(), start, end);
              return true;
            }
          }
        }
        return false;
      }

      if (type == ScheduleRecordingType.EveryTimeOnThisChannel)
      {
        TvDatabase.Program current = schedule.ReferencedChannel().GetProgramAt(currentTime.AddMinutes(schedule.PreRecordInterval));
        if (current != null)
        {
          if (currentTime >= current.StartTime.AddMinutes(-schedule.PreRecordInterval) && currentTime <= current.EndTime.AddMinutes(schedule.PostRecordInterval))
          {
            if (String.Compare(current.Title, schedule.ProgramName, true) == 0)
            {
              if (!schedule.IsSerieIsCanceled(current.StartTime))
              {
                if (!isInRecordingsList(current))
                {
                  Schedule newSchedule = new Schedule(schedule);
                  newSchedule.StartTime = current.StartTime;
                  newSchedule.EndTime = current.EndTime;
                  newSchedule.ScheduleType = 0; // type Once
                  newSchedule.Persist();
                  newRecording = new RecordingDetail(newSchedule, current.ReferencedChannel(), current.StartTime, current.EndTime);
                  return true;
                }
              }
            }
          }
        }
      }

      if (type == ScheduleRecordingType.EveryTimeOnEveryChannel)
      {
        foreach (Channel channel in _channels)
        {
          TvDatabase.Program current = schedule.ReferencedChannel().GetProgramAt(DateTime.Now.AddMinutes(schedule.PreRecordInterval));
          //TvDatabase.Program next = channel.GetProgramAt(current.EndTime.AddMinutes(1));
          if (current != null)
          {
            if (currentTime >= current.StartTime.AddMinutes(-schedule.PreRecordInterval) && currentTime <= current.EndTime.AddMinutes(schedule.PostRecordInterval))
            {
              if (String.Compare(current.Title, schedule.ProgramName, true) == 0)
              {
                if (!schedule.IsSerieIsCanceled(current.StartTime))
                {
                  if (!isInRecordingsList(current))
                  {
                    Schedule newSchedule = new Schedule(schedule);
                    newSchedule.IdChannel = channel.IdChannel;
                    newSchedule.StartTime = current.StartTime;
                    newSchedule.EndTime = current.EndTime;
                    newSchedule.ScheduleType = 0; // type Once
                    newSchedule.Persist();
                    newRecording = new RecordingDetail(newSchedule, current.ReferencedChannel(), current.StartTime, current.EndTime);
                    return true;
                  }

                }
              }
            }
          }
        }
      }
      return false;      
    }

    /// <summary>
    /// Check if a program is in the recordings in progress ' s list
    /// </summary>
    /// <param name="prog">program we wanna look for</param>
    /// <returns>false/true</returns>
    private bool isInRecordingsList(TvDatabase.Program prog)
    {
      foreach (RecordingDetail rec in _recordingsInProgressList)
      {
        if (rec.Program.IdProgram == prog.IdProgram)
        {
          return true;
          break;
        }
      }
      return false;
    }

    /// <summary>
    /// Starts recording the recording specified
    /// </summary>
    /// <param name="recording">Recording instance</param>
    /// <returns>true if recording is started, otherwise false</returns>
    bool StartRecord(RecordingDetail recording)
    {
      _user.Name = string.Format("scheduler{0}", recording.Schedule.IdSchedule);
      _user.CardId = -1;
      _user.SubChannel = -1;
      _user.IsAdmin = true;
      Log.Write("Scheduler : time to record {0} {1}-{2} {3}", recording.Channel, DateTime.Now, recording.EndTime, recording.Schedule.ProgramName);
      TvResult result;
      //get list of all cards we can use todo the recording
      List<CardDetail> freeCards = _tvController.GetFreeCardsForChannel(recording.Channel, ref _user, false, false, out result);
      if (freeCards.Count == 0) return false;//none available..

      CardDetail cardInfo = null;

      //first try to start recording using the recommended card
      if (recording.Schedule.RecommendedCard > 0)
      {
        foreach (CardDetail card in freeCards)
        {
          if (card.Id != recording.Schedule.RecommendedCard) continue;

          //when card is idle we can use it
          //when card is busy, but already tuned to the correct channel then we can use it also
          //when card is busy, but already tuned to the correct transponder then we can use it also
          User byUser;
          User tmpUser = new User();
          tmpUser = _tvController.GetUserForCard(card.Id);//added by joboehl - Allows the CurrentDbChannel bellow to work when TVServer and client are on different machines
          if ((_tvController.IsCardInUse(card.Id, out byUser) == false) ||
               _tvController.CurrentDbChannel(ref tmpUser) == recording.Channel.IdChannel ||
               (_tvController.IsTunedToTransponder(card.Id, card.TuningDetail) && _tvController.Type(card.Id) != CardType.Analog))
          {
            // use the recommended card.
            cardInfo = card;
            Log.Write("Scheduler : record on recommended card:{0} priority:{1}", cardInfo.Id, cardInfo.Card.Priority);
            break;
          }
        }
        if (cardInfo == null)
        {
          Log.Write("Scheduler : recommended card:{0} is not available", recording.Schedule.RecommendedCard);
        }
      }
      if (cardInfo == null)
      {
        //first try, find a card which is already tuned to the correct transponder
        foreach (CardDetail card in freeCards)
        {
          if (_tvController.IsTunedToTransponder(card.Id, card.TuningDetail) && _tvController.Type(card.Id) != CardType.Analog)
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
        Log.Write("Scheduler : all cards busy, check if any card is already tuned to channel:{0}", recording.Channel);
        //all cards in use, check if a card is already tuned to the channel we want to record
        foreach (CardDetail card in freeCards)
        {
          User tmpUser = new User();
          tmpUser = _tvController.GetUserForCard(card.Id);//added by joboehl - Allows the CurrentDbChannel bellow to work when TVServer and client are on different machines
          if (_tvController.CurrentDbChannel(ref tmpUser) == recording.Channel.IdChannel)
          {
            if (_tvController.IsRecording(ref tmpUser) == false)
            {
              cardInfo = card;
              Log.Write("Scheduler : record on card:{0} priority:{1} which is tuned to {2}", cardInfo.Id, cardInfo.Card.Priority, recording.Channel.Name);
              break;
            }
          }
        }
      }

      if (cardInfo == null)
      {
        //all cards in use, no card tuned to the channel, use the first one.
        TvBusinessLayer layer = new TvBusinessLayer();
        User tmpUser = new User();
        tmpUser.CardId = freeCards[0].Id;
        if ((_tvController.IsRecording(ref tmpUser) == false) && (layer.GetSetting("scheduleroverlivetv", "yes").Value == "yes"))
        {
          if (_tvController.IsTimeShifting(ref tmpUser)) { _tvController.StopTimeShifting(ref tmpUser); }
          cardInfo = freeCards[0];
          Log.Write("Scheduler : no card is tuned to the correct channel. record on card:{0} priority:{1}", cardInfo.Id, cardInfo.Card.Priority);
        }
        else
        {
          Log.Write("Scheduler : no card was found and scheduler not allowed to stop other users LiveTV or recordings. ");
          return false;
        }
      }

      try
      {
        _user.CardId = cardInfo.Id;

        _tvController.Fire(this, new TvServerEventArgs(TvServerEventType.StartRecording, new VirtualCard(_user), _user, recording.Schedule, null));

        if (cardInfo.Card.RecordingFolder == String.Empty)
          cardInfo.Card.RecordingFolder = System.IO.Directory.GetCurrentDirectory();
        if (cardInfo.Card.TimeShiftFolder == String.Empty)
          cardInfo.Card.TimeShiftFolder = System.IO.Directory.GetCurrentDirectory();

        Log.Write("Scheduler : record, first tune to channel");
        TvResult tuneResult = _controller.Tune(ref _user, cardInfo.TuningDetail, recording.Channel.IdChannel);
        if (tuneResult != TvResult.Succeeded)
        {
          return false;
        }


        if (_controller.SupportsSubChannels(cardInfo.Card.IdCard) == false)
        {
          Log.Write("Scheduler : record, now start timeshift");
          string timeshiftFileName = String.Format(@"{0}\live{1}-{2}.ts", cardInfo.Card.TimeShiftFolder, cardInfo.Id, _user.SubChannel);
          if (TvResult.Succeeded != _controller.StartTimeShifting(ref _user, ref timeshiftFileName))
          {
            return false;
          }
        }

        recording.MakeFileName(cardInfo.Card.RecordingFolder);
        recording.CardInfo = cardInfo;
        Log.Write("Scheduler : record to {0}", recording.FileName);
        string fileName = recording.FileName;
        if (false == _controller.StartRecording(ref _user, ref fileName, false, 0))
        {
          return false;
        }
        recording.FileName = fileName;
        recording.RecordingStartDateTime = DateTime.Now;
        int idServer = recording.CardInfo.Card.IdServer;
        recording.Recording = new Recording(recording.Schedule.IdChannel, recording.RecordingStartDateTime, DateTime.Now, recording.Program.Title,
                            recording.Program.Description, recording.Program.Genre, recording.FileName, (int)recording.Schedule.KeepMethod,
                            recording.Schedule.KeepDate, 0, idServer);
        recording.Recording.Persist();
        _recordingsInProgressList.Add(recording);
        _tvController.Fire(this, new TvServerEventArgs(TvServerEventType.RecordingStarted, new VirtualCard(_user), _user, recording.Schedule, recording.Recording));
        Log.Write("recList:count:{0} add scheduleid:{1} card:{2}", _recordingsInProgressList.Count, recording.Schedule.IdSchedule, recording.CardInfo.Card.Name);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return true;
    }

    /// <summary>
    /// stops recording the specified recording 
    /// </summary>
    /// <param name="recording">Recording</param>
    void StopRecord(RecordingDetail recording)
    {
      try
      {
        _user.CardId = recording.CardInfo.Id;
        _user.Name = string.Format("scheduler{0}", recording.Schedule.IdSchedule);
        _user.IsAdmin = true;
        Log.Write("Scheduler : stop record {0} {1}-{2} {3}", recording.Channel, recording.RecordingStartDateTime, recording.EndTime, recording.Schedule.ProgramName);
        _controller.StopRecording(ref _user);

        if (_controller.SupportsSubChannels(recording.CardInfo.Id) == false)
        {
          _controller.StopTimeShifting(ref _user);
        }


        recording.Recording.EndTime = DateTime.Now;
        recording.Recording.Persist();
        _tvController.Fire(this, new TvServerEventArgs(TvServerEventType.RecordingEnded, new VirtualCard(_user), _user, recording.Schedule, recording.Recording));


        //DatabaseManager.Instance.SaveChanges();

        PostProcessing processor = new PostProcessing();
        processor.Process(recording);
        if ((ScheduleRecordingType)recording.Schedule.ScheduleType == ScheduleRecordingType.Once)
        {
          recording.Schedule.Delete();
          _tvController.Fire(this, new TvServerEventArgs(TvServerEventType.ScheduleDeleted, new VirtualCard(_user), _user, recording.Schedule, null));
        }
        else
        {
          if (DateTime.Now < recording.Program.EndTime)
          {
            CanceledSchedule canceled = new CanceledSchedule(recording.Schedule.IdSchedule, recording.Program.StartTime);
            canceled.Persist();
          }
          _episodeManagement.OnScheduleEnded(recording.FileName, recording.Schedule, recording.Program);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        _recordingsInProgressList.Remove(recording);
      }
    }

    /// <summary>
    /// Method which returns which card is currently recording the Schedule with the specified scheduleid
    /// </summary>
    /// <param name="idSchedule">database id of the schedule</param>
    /// <param name="cardId">id of card currently recording the schedule or -1 if none is recording the schedule</param>
    /// <returns>if a card is recording the schedule, else false</returns>
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
      Log.Write("recList:StopRecordingSchedule{0}", idSchedule);

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
      Log.Write("recList:StopRecordingOnCard{0}", cardId);
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
