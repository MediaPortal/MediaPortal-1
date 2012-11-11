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
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Mediaportal.TV.Server.Plugins.Base;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using MediaPortal.Common.Utils;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.ConflictsManager
{
  [Interceptor("PluginExceptionInterceptor")]
  public class ConflictsManager : ITvServerPlugin
  {


    #region variables

    private IList<Schedule> _schedules = null;
    private IList<Card> _cards = null;

    private IList<Program> _conflictingPrograms;

    #endregion

    #region properties

    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get { return "ConflictsManager"; }
    }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get { return "1.0.0.1"; }
    }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get { return "Broceliande"; }
    }

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get { return true; }
    }

    #endregion

    #region public members

    /// <summary>
    /// Starts the plugin
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("plugin: ConflictsManager started");

      _schedules = ScheduleManagement.ListAllSchedules();
      _cards = CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB

      _conflictingPrograms = new List<Program>();
      ITvServerEvent events = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
      events.OnTvServerEvent += new TvServerEventHandler(events_OnTvServerEvent);
    }

    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
      this.LogDebug("plugin: ConflictsManager stopped");
      ClearConflictTable();
      ClearConflictPrograms();


      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= events_OnTvServerEvent;
      }
    }

    /// <summary>
    /// Handles the OnTvServerEvent event fired by the server.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The <see cref="System.EventArgs"/> the event data.</param>
    private void events_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;
      if (tvEvent.EventType == TvServerEventType.ScheduledAdded ||
          tvEvent.EventType == TvServerEventType.ScheduleDeleted)
      {
        UpdateConflicts();
        SettingsManagement.SaveSetting("CMLastUpdateTime", DateTime.Now.ToString());        

      }
    }

    /// <summary>
    /// Plugin setup form
    /// </summary>
    public SectionSettings Setup
    {
      get { return new CMSetup(); }
    }

    /// <summary>
    /// Parses the sheduled recordings
    /// and updates the conflicting recordings table
    /// </summary>
    public void UpdateConflicts()
    {
      this.LogInfo("ConflictManager: Updating conflicts list");
      DateTime startUpdate = DateTime.Now;
      // hmm... 
      ClearConflictTable();
      ClearConflictPrograms();
      // Gets schedules from db
      IList<Schedule> scheduleList = ScheduleManagement.ListAllSchedules();
      IList<Schedule> scheduleListToParse = new List<Schedule>();
      // parses all schedules and add the calculated incoming schedules 
      getRecordOnceSchedules(scheduleList, scheduleListToParse);
      getDailySchedules(scheduleList, scheduleListToParse);
      getWeeklySchedules(scheduleList, scheduleListToParse);
      getWeekendsSchedules(scheduleList, scheduleListToParse);
      getWorkingDaysSchedules(scheduleList, scheduleListToParse);
      getWeeklyEveryTimeOnThisChannelSchedules(scheduleList, scheduleListToParse);
      getEveryTimeOnEveryChannelSchedules(scheduleList, scheduleListToParse);
      getEveryTimeOnThisChannelSchedules(scheduleList, scheduleListToParse);
      removeCanceledSchedules(scheduleListToParse);

      // test section
      bool cmDebug = SettingsManagement.GetSetting("CMDebugMode", "false").Value == "true"; // activate debug mode

      #region debug informations

      /*
      if (cmDebug)
      {
        foreach (Schedule schedule in scheduleOnceList) this.LogDebug("Record Once schedule: {0} {1} - {2}", schedule.programName, schedule.startTime, schedule.endTime);
        this.LogDebug("------------------------------------------------");
        foreach (Schedule schedule in scheduleDailyList) this.LogDebug("Daily schedule: {0} {1} - {2}", schedule.programName, schedule.startTime, schedule.endTime);
        this.LogDebug("------------------------------------------------");
        foreach (Schedule schedule in scheduleWeeklyList) this.LogDebug("Weekly schedule: {0} {1} - {2}", schedule.programName, schedule.startTime, schedule.endTime);
        this.LogDebug("------------------------------------------------");
        foreach (Schedule schedule in scheduleWeekendsList) this.LogDebug("Weekend schedule: {0} {1} - {2}", schedule.programName, schedule.startTime, schedule.endTime);
        this.LogDebug("------------------------------------------------");
        foreach (Schedule schedule in scheduleWorkingDaysList) this.LogDebug("Working days schedule: {0} {1} - {2}", schedule.programName, schedule.startTime, schedule.endTime);
        this.LogDebug("------------------------------------------------");
        foreach (Schedule schedule in scheduleEveryTimeEveryChannelList) this.LogDebug("Evry time on evry chan. schedule: {0} {1} - {2}", schedule.programName, schedule.startTime, schedule.endTime);
        this.LogDebug("------------------------------------------------");
        foreach (Schedule schedule in scheduleEveryTimeThisChannelList) this.LogDebug("Evry time on this chan. schedule: {0} {1} - {2}", schedule.programName, schedule.startTime, schedule.endTime);
        this.LogDebug("------------------------------------------------");
       * 
      }*/

      #endregion

      scheduleList.Clear();
      TimeSpan ts = DateTime.Now - startUpdate;
      this.LogInfo("Schedules List built {0} ms", ts.TotalMilliseconds);
      // try to assign all schedules to existing cards
      if (cmDebug) this.LogDebug("Calling assignSchedulestoCards with {0} schedules", scheduleListToParse.Count);
      List<Schedule>[] assignedList = AssignSchedulesToCards(scheduleListToParse);
      ts = DateTime.Now - startUpdate;
      this.LogInfo("ConflictManager: Update done within {0} ms", ts.TotalMilliseconds);
      //List<Conflict> _conflicts = new List<Conflict>();
    }

    #endregion

    #region private members

    private void ClearConflictPrograms()
    {
      foreach (Program prg in _conflictingPrograms)
      {
        var bll = new ProgramBLL(prg) {HasConflict = false};
        ProgramManagement.SaveProgram(bll.Entity);        
      }
      _conflictingPrograms.Clear();
    }

    /// <summary>
    /// Removes all records in Table : Conflict
    /// </summary>
    private static void ClearConflictTable()
    {
      // clears all conflicts in db
      IList<Conflict> conflictList = ConflictManagement.ListAllConflicts();
      foreach (Conflict aconflict in conflictList)
      {
        ConflictManagement.DeleteConflict(aconflict.IdConflict);        
      }
    }

    private void Init() {}

    /// <summary>
    /// Checks if 2 scheduled recordings are overlapping
    /// </summary>
    /// <param name="Schedule 1"></param>
    /// <param name="Schedule 2"></param>
    /// <returns>true if sheduled recordings are overlapping, false either</returns>
    private static bool IsOverlap(Schedule schedule1, Schedule schedule2)
    {
      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2    s--------------------------------e
      // sch_2  ------------------e
      if (
        ((schedule2.StartTime.AddMinutes(-schedule2.PreRecordInterval) >=
          schedule1.StartTime.AddMinutes(-schedule1.PreRecordInterval)) &&
         (schedule2.StartTime.AddMinutes(-schedule2.PreRecordInterval) <
          schedule1.EndTime.AddMinutes(schedule1.PostRecordInterval))) ||
        ((schedule2.StartTime.AddMinutes(-schedule2.PreRecordInterval) <=
          schedule1.StartTime.AddMinutes(-schedule1.PreRecordInterval)) &&
         (schedule2.EndTime.AddMinutes(schedule2.PostRecordInterval) >=
          schedule1.EndTime.AddMinutes(schedule1.PostRecordInterval))) ||
        ((schedule2.EndTime.AddMinutes(schedule2.PostRecordInterval) >
          schedule1.StartTime.AddMinutes(-schedule1.PreRecordInterval)) &&
         (schedule2.EndTime.AddMinutes(schedule2.PostRecordInterval) <=
          schedule1.EndTime.AddMinutes(schedule1.PostRecordInterval)))
        ) return true;
      return false;
    }

    /// <summary>Assign all shedules to cards</summary>
    /// <param name="Schedules">An IList containing all scheduled recordings</param>
    /// <returns>Array of List<Schedule> : one per card, index [0] contains unassigned schedules</returns>
    private List<Schedule>[] AssignSchedulesToCards(IList<Schedule> Schedules)
    {
      IList<Card> cardsList = CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
      // creates an array of Schedule Lists
      // element [0] will be filled with conflicting schedules
      // element [x] will be filled with the schedules assigned to card with idcard=x
      List<Schedule>[] cardSchedules = new List<Schedule>[cardsList.Count + 1];
      for (int i = 0; i < cardsList.Count + 1; i++) cardSchedules[i] = new List<Schedule>();

      #region assigns schedules from table

      //
      Dictionary<int, int> cardno = new Dictionary<int, int>();
      int n = 1;
      foreach (Card _card in _cards)
      {
        cardno.Add(_card.IdCard, n);
        n++;
      }
      //
      foreach (Schedule schedule in Schedules)
      {
        bool assigned = false;
        Schedule lastOverlappingSchedule = null;
        int lastBusyCard = 0;
        bool overlap = false;

        foreach (Card card in cardsList)
        {
          if (CardManagement.CanViewTvChannel(card, schedule.IdSchedule))
          {
            // checks if any schedule assigned to this cards overlaps current parsed schedule
            bool free = true;
            foreach (Schedule assignedShedule in cardSchedules[cardno[card.IdCard]])
            {
              ScheduleBLL bll = new ScheduleBLL(schedule);
              if (bll.IsOverlapping(assignedShedule))
              {                                                
                if (!(bll.IsSameTransponder(assignedShedule)))
                {
                  free = false;
                  //_overlap = true;
                  lastOverlappingSchedule = assignedShedule;
                  lastBusyCard = card.IdCard;
                  break;
                }
              }
            }
            if (free)
            {
              cardSchedules[cardno[card.IdCard]].Add(schedule);
              assigned = true;
              break;
            }
          }
        }
        if (!assigned)
        {
          cardSchedules[0].Add(schedule);
          var newConflict = new Conflict
                              {
                                IdSchedule = schedule.IdSchedule,
                                IdConflictingSchedule = lastOverlappingSchedule.IdSchedule,
                                IdChannel = schedule.IdChannel,
                                ConflictDate = schedule.StartTime,
                                IdCard = lastBusyCard
                              };

          ConflictManagement.SaveConflict(newConflict);          
          Program prg = ProgramManagement.RetrieveByTitleTimesAndChannel(schedule.ProgramName, schedule.StartTime,
                                                               schedule.EndTime, schedule.IdChannel);

          if (prg != null)
          {
            var bll = new ProgramBLL(prg) {HasConflict = true};
            ProgramManagement.SaveProgram(bll.Entity);
            _conflictingPrograms.Add(prg);
          }
        }
      }

      #endregion

      return cardSchedules;
    }

    /// <summary>
    /// Counts how many cards can be used to view a shedule
    /// </summary>
    /// <param name="_shedule">Schedule</param>
    /// <returns>int: number of cards</returns>
    private int howManyCardsCanView(Schedule _shedule)
    {
      return _cards.Count(_card => CardManagement.CanViewTvChannel(_card, _shedule.IdChannel));
    }

    /// <summary>
    /// Splits a schedules list 
    /// Sorts each element in a List array
    /// where list[x] contains schedules that can be viewed by x cards
    /// </summary>
    /// <param name="Schedules">a shedules IList</param>
    /// <returns>a list array of schedules. Element [0] contains schedules that cannot be viewed by any card</returns>
    private List<Schedule>[] sortSchedules(IList<Schedule> Schedules)
    {
      List<Schedule>[] _sortedSchedules = new List<Schedule>[_cards.Count];
      for (int i = 0; i < _cards.Count + 1; i++) _sortedSchedules[i] = new List<Schedule>();

      foreach (Schedule _Schedule in _schedules)
      {
        _sortedSchedules[howManyCardsCanView(_Schedule)].Add(_Schedule);
      }
      return _sortedSchedules;
    }

    private List<Schedule>[] tryToResolve(List<Schedule>[] _sortedList)
    {
      int _cardsCount = _cards.Count;
      foreach (Schedule _unassignedSchedule in _sortedList[0])
      {
        for (int i = 1; i <= _cardsCount; i++)
        {
          foreach (Schedule _assignedSchedule in _sortedList[i])
          {
            if (IsOverlap(_unassignedSchedule, _assignedSchedule)) {} //if (IsOverlap(_unassignedSchedule, _assignedSchedule))
          } //foreach (Schedule _assignedSchedule in _listToSolve[i])
        } //for (int i = 1; i <= _cardsCount; i++)
      } //foreach (Schedule _unassignedSchedule in _listToSolve[0])
      return _sortedList;
    }

    /// <summary>
    /// gets "Record Once" Schedules in a list of schedules
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the "record once" schedules</returns>
    private void getRecordOnceSchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      foreach (Schedule schedule in schedulesList)
      {                
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.Once) continue;
        refFillList.Add(schedule);
      }
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// gets Daily Schedules in a given list of schedules
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the Daily schedules</returns>
    private void getDailySchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.Daily) continue;
        // create a temporay base schedule with today's date
        // (will be used to calculate incoming schedules)
        // and adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
        Schedule baseSchedule = ScheduleFactory.Clone(schedule);
        if (baseSchedule.StartTime.Day != baseSchedule.EndTime.Day)
        {
          baseSchedule.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                                baseSchedule.StartTime.Hour, baseSchedule.StartTime.Minute,
                                                baseSchedule.StartTime.Second);
          baseSchedule.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                              baseSchedule.EndTime.Hour, baseSchedule.EndTime.Minute,
                                              baseSchedule.EndTime.Second);
          baseSchedule.EndTime = baseSchedule.EndTime.AddDays(1);
        }
        else
        {
          baseSchedule.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                                baseSchedule.StartTime.Hour, baseSchedule.StartTime.Minute,
                                                baseSchedule.StartTime.Second);
          baseSchedule.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                              baseSchedule.EndTime.Hour, baseSchedule.EndTime.Minute,
                                              baseSchedule.EndTime.Second);
        }

        // generate the daily schedules for the next 30 days
        DateTime tempDate;
        for (int i = 0; i <= 30; i++)
        {
          tempDate = DateTime.Now.AddDays(i);
          if (tempDate.Date >= schedule.StartTime.Date)
          {
            Schedule incomingSchedule = ScheduleFactory.Clone(baseSchedule);
            incomingSchedule.StartTime = incomingSchedule.StartTime.AddDays(i);
            incomingSchedule.EndTime = incomingSchedule.EndTime.AddDays(i);
            refFillList.Add(incomingSchedule);
          } //if (_tempDate>=_Schedule.startTime)
        } //for (int i = 0; i <= 30; i++)
      }
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// gets Weekly Schedules in a given list of schedules for the next 30 days 
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the Weekly schedules</returns>
    private void getWeeklySchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.Weekly) continue;
        DateTime tempDate;
        //  generate the weekly schedules for the next 30 days
        for (int i = 0; i <= 30; i++)
        {
          tempDate = DateTime.Now.AddDays(i);
          if ((tempDate.DayOfWeek == schedule.StartTime.DayOfWeek) && (tempDate.Date >= schedule.StartTime.Date))
          {
            Schedule tempSchedule = ScheduleFactory.Clone(schedule);

            #region Set Schedule Time & Date

            // adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
            if (tempSchedule.StartTime.Day != tempSchedule.EndTime.Day)
            {
              tempSchedule.StartTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day,
                                                    tempSchedule.StartTime.Hour, tempSchedule.StartTime.Minute,
                                                    tempSchedule.StartTime.Second);
              tempSchedule.EndTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, tempSchedule.EndTime.Hour,
                                                  tempSchedule.EndTime.Minute, tempSchedule.EndTime.Second);
              tempSchedule.EndTime = tempSchedule.EndTime.AddDays(1);
            }
            else
            {
              tempSchedule.StartTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day,
                                                    tempSchedule.StartTime.Hour, tempSchedule.StartTime.Minute,
                                                    tempSchedule.StartTime.Second);
              tempSchedule.EndTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, tempSchedule.EndTime.Hour,
                                                  tempSchedule.EndTime.Minute, tempSchedule.EndTime.Second);
            }

            #endregion

            refFillList.Add(tempSchedule);
          } //if (_tempDate.DayOfWeek == _Schedule.startTime.DayOfWeek && _tempDate >= _Schedule.startTime)
        } //for (int i = 0; i < 30; i++)
      } //foreach (Schedule _Schedule in schedulesList)
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// gets Weekends Schedules in a given list of schedules for the next 30 days 
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the Weekends schedules</returns>
    private void getWeekendsSchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.Weekends) continue;
        DateTime tempDate;
        //  generate the weekly schedules for the next 30 days
        for (int i = 0; i <= 30; i++)
        {
          tempDate = DateTime.Now.AddDays(i);
          if (WeekEndTool.IsWeekend(tempDate.DayOfWeek) && (tempDate.Date >= schedule.StartTime.Date))
          {
            Schedule tempSchedule = ScheduleFactory.Clone(schedule);

            #region Set Schedule Time & Date

            // adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
            if (tempSchedule.StartTime.Day != tempSchedule.EndTime.Day)
            {
              tempSchedule.StartTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day,
                                                    tempSchedule.StartTime.Hour, tempSchedule.StartTime.Minute,
                                                    tempSchedule.StartTime.Second);
              tempSchedule.EndTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, tempSchedule.EndTime.Hour,
                                                  tempSchedule.EndTime.Minute, tempSchedule.EndTime.Second);
              tempSchedule.EndTime = tempSchedule.EndTime.AddDays(1);
            }
            else
            {
              tempSchedule.StartTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day,
                                                    tempSchedule.StartTime.Hour, tempSchedule.StartTime.Minute,
                                                    tempSchedule.StartTime.Second);
              tempSchedule.EndTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, tempSchedule.EndTime.Hour,
                                                  tempSchedule.EndTime.Minute, tempSchedule.EndTime.Second);
            }

            #endregion

            refFillList.Add(tempSchedule);
          } //if (_tempDate.DayOfWeek == _Schedule.startTime.DayOfWeek && _tempDate >= _Schedule.startTime)
        } //for (int i = 0; i < 30; i++)
      } //foreach (Schedule _Schedule in schedulesList)
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// gets WorkingDays Schedules in a given list of schedules for the next 30 days 
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the WorkingDays schedules</returns>
    private void getWorkingDaysSchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.WorkingDays) continue;
        DateTime tempDate;
        //  generate the weekly schedules for the next 30 days
        for (int i = 0; i <= 30; i++)
        {
          tempDate = DateTime.Now.AddDays(i);
          if ((WeekEndTool.IsWorkingDay(tempDate.DayOfWeek)) && (tempDate.Date >= schedule.StartTime.Date))
          {
            Schedule tempSchedule = ScheduleFactory.Clone(schedule);

            #region Set Schedule Time & Date

            // adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
            if (tempSchedule.StartTime.Day != tempSchedule.EndTime.Day)
            {
              tempSchedule.StartTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day,
                                                    tempSchedule.StartTime.Hour, tempSchedule.StartTime.Minute,
                                                    tempSchedule.StartTime.Second);
              tempSchedule.EndTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, tempSchedule.EndTime.Hour,
                                                  tempSchedule.EndTime.Minute, tempSchedule.EndTime.Second);
              tempSchedule.EndTime = tempSchedule.EndTime.AddDays(1);
            }
            else
            {
              tempSchedule.StartTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day,
                                                    tempSchedule.StartTime.Hour, tempSchedule.StartTime.Minute,
                                                    tempSchedule.StartTime.Second);
              tempSchedule.EndTime = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, tempSchedule.EndTime.Hour,
                                                  tempSchedule.EndTime.Minute, tempSchedule.EndTime.Second);
            }

            #endregion

            refFillList.Add(tempSchedule);
          } //if (_tempDate.DayOfWeek == _Schedule.startTime.DayOfWeek && _tempDate >= _Schedule.startTime)
        } //for (int i = 0; i < 30; i++)
      } //foreach (Schedule _Schedule in schedulesList)
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// get incoming "EveryTimeOnThisChannel" type schedules in Program Table
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the schedules</returns>
    private void getEveryTimeOnEveryChannelSchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      //IList programsList = Program.ListAll();
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.EveryTimeOnEveryChannel) continue;
        IList<Program> programsList = ProgramManagement.RetrieveByTitleAndTimesInterval(schedule.ProgramName, schedule.StartTime,
                                                                              schedule.StartTime.AddMonths(1));
        foreach (Program program in programsList)
        {
          if ((program.Title == schedule.ProgramName) && (program.IdChannel == schedule.IdChannel) &&
              (program.EndTime >= DateTime.Now))
          {
            Schedule incomingSchedule = ScheduleFactory.Clone(schedule);
            incomingSchedule.IdChannel = program.IdChannel;
            incomingSchedule.ProgramName = program.Title;
            incomingSchedule.StartTime = program.StartTime;
            incomingSchedule.EndTime = program.EndTime;
            incomingSchedule.PreRecordInterval = schedule.PreRecordInterval;
            incomingSchedule.PostRecordInterval = schedule.PostRecordInterval;
            refFillList.Add(incomingSchedule);
          }
        } //foreach (Program _program in _programsList)
      } //foreach (Schedule _Schedule in schedulesList)
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// Gets the every time on this channel schedules.
    /// </summary>
    /// <param name="schedulesList">The schedules list.</param>
    /// <returns></returns>
    private void getEveryTimeOnThisChannelSchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.EveryTimeOnThisChannel) continue;
        Channel channel = ChannelManagement.GetChannel(schedule.IdChannel);

        IList<Program> programsList = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(channel.IdChannel,
                                                                        schedule.ProgramName, DateTime.Now,
                                                                        DateTime.Now.AddMonths(1));          
        if (programsList != null)
        {
          foreach (Program program in programsList)
          {
            Schedule incomingSchedule = ScheduleFactory.Clone(schedule);
            incomingSchedule.IdChannel = program.IdChannel;
            incomingSchedule.ProgramName = program.Title;
            incomingSchedule.StartTime = program.StartTime;
            incomingSchedule.EndTime = program.EndTime;

            incomingSchedule.PreRecordInterval = schedule.PreRecordInterval;
            incomingSchedule.PostRecordInterval = schedule.PostRecordInterval;
            refFillList.Add(incomingSchedule);
          }
        } //foreach (Program _program in _programsList)
      } //foreach (Schedule _Schedule in schedulesList)
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// Gets the Weekly every time on this channel schedules.
    /// </summary>
    /// <param name="schedulesList">The schedules list.</param>
    /// <returns></returns>
    private void getWeeklyEveryTimeOnThisChannelSchedules(IList<Schedule> schedulesList, IList<Schedule> refFillList)
    {
      
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.WeeklyEveryTimeOnThisChannel) continue;
        Channel channel = ChannelManagement.GetChannel(schedule.IdChannel);

        IList<Program> programsList = ProgramManagement.GetProgramsByChannelAndTitleAndStartEndTimes(channel.IdChannel,
                                                                        schedule.ProgramName, DateTime.Now,
                                                                        DateTime.Now.AddMonths(1));          
       
        if (programsList != null)
        {
          foreach (Program program in programsList)
          {
            if (program.StartTime.DayOfWeek == schedule.StartTime.DayOfWeek)
            {
              Schedule incomingSchedule = ScheduleFactory.Clone(schedule);
              incomingSchedule.IdChannel = program.IdChannel;
              incomingSchedule.ProgramName = program.Title;
              incomingSchedule.StartTime = program.StartTime;
              incomingSchedule.EndTime = program.EndTime;

              incomingSchedule.PreRecordInterval = schedule.PreRecordInterval;
              incomingSchedule.PostRecordInterval = schedule.PostRecordInterval;
              refFillList.Add(incomingSchedule);
            }
          }
        } //foreach (Program _program in _programsList)
      } //foreach (Schedule _Schedule in schedulesList)
      foreach (Schedule sched in refFillList) schedulesList.Remove(sched);
    }

    /// <summary>
    /// Removes every cancled schedule.
    /// </summary>
    /// <param name="refFillList">The schedules list.</param>
    /// <returns></returns>
    private void removeCanceledSchedules(IList<Schedule> refFillList)
    {
      IList<CanceledSchedule> canceledList = CanceledScheduleManagement.ListAllCanceledSchedules();
      foreach (CanceledSchedule canceled in canceledList)
      {
        foreach (Schedule sched in refFillList)
        {
          if ((canceled.IdSchedule == sched.IdSchedule) && (canceled.CancelDateTime == sched.StartTime))
          {
            refFillList.Remove(sched);
            break;
          }
        }
      }
    }


    /// <summary>
    /// checks if the decryptLimit for a card, regarding to a list of assigned shedules
    /// has been reached or not
    /// </summary>
    /// <param name="card">card we wanna use</param>
    /// <param name="assignedSchedules">List of schedules assigned to this card</param>
    /// <returns></returns>
    private bool cardLimitReached(Card card, IList<Schedule> assignedSchedules)
    {
      return false;
    }

    #endregion
  }
}