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
using TvControl;
using System.Threading;
using TvLibrary.Log;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvEngine.Interfaces;

namespace TvEngine
{
  public class ConflictsManager : ITvServerPlugin
  {
    #region variables
    TvBusinessLayer cmLayer = new TvBusinessLayer();
    IList _schedules = Schedule.ListAll();
    IList _cards = Card.ListAll();
    #endregion

    #region properties
    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get
      {
        return "ConflictsManager";
      }
    }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get
      {
        return "1.0.0.1";
      }
    }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get
      {
        return "Broceliande";
      }
    }

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get
      {
        return true;
      }
    }
    #endregion

    #region public members

    /// <summary>
    /// Starts the plugin
    /// </summary>
    public void Start(IController controller)
    {
      Log.WriteFile("plugin: ConflictsManager started");
      ITvServerEvent events = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
      events.OnTvServerEvent += new TvServerEventHandler(events_OnTvServerEvent);
    }

    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
      Log.WriteFile("plugin: ConflictsManager stopped");
      ITvServerEvent events = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
      events.OnTvServerEvent -= new TvServerEventHandler(events_OnTvServerEvent);
    }

    /// <summary>
    /// Handles the OnTvServerEvent event fired by the server.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The <see cref="System.EventArgs"/> the event data.</param>
    void events_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;
      if (tvEvent.EventType == TvServerEventType.ScheduledAdded || tvEvent.EventType == TvServerEventType.ScheduleDeleted)
      {
        UpdateConflicts();
        Setting setting = cmLayer.GetSetting("CMLastUpdateTime", DateTime.Now.ToString());
        setting.Value = DateTime.Now.ToString();
        setting.Persist();
        //TvController mycontrol;
      }
    }

    /// <summary>
    /// Plugin setup form
    /// </summary>
    public SetupTv.SectionSettings Setup
    {
      get
      {
        return new SetupTv.Sections.CMSetup();
      }
    }

    /// <summary>
    /// Parses the sheduled recordings
    /// and updates the conflicting recordings table
    /// </summary>
    public void UpdateConflicts()
    {
      Log.Info("ConflictManager: Updating conflicts list");
      // hmm... 
      ClearConflictTable();
      // Gets schedules from db
      IList scheduleList = Schedule.ListAll();
      // parses all schedules and add the calculated incoming schedules 
      IList scheduleOnceList = getRecordOnceSchedules(scheduleList);
      IList scheduleRepeatingList = getRepeatingSchedules(scheduleList);
      IList scheduleEveryTimeEveryChannelList = getEveryTimeOnEveryChannelSchedules(scheduleList);
      IList scheduleEveryTimeThisChannelList = getEveryTimeOnThisChannelSchedules(scheduleList);
      // test section
      bool cmDebug = cmLayer.GetSetting("CMDebugMode", "false").Value == "true";// activate debug mode
      #region debug informations
      if (cmDebug)
      {
        foreach (Schedule schedule in scheduleOnceList) Log.Debug("Record Once schedule: {0} {1} - {2}", schedule.ProgramName, schedule.StartTime, schedule.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule schedule in scheduleRepeatingList) Log.Debug("Repeating schedule: {0} {1} - {2}", schedule.ProgramName, schedule.StartTime, schedule.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule schedule in scheduleEveryTimeEveryChannelList) Log.Debug("Every time on every chan. schedule: {0} {1} - {2}", schedule.ProgramName, schedule.StartTime, schedule.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule schedule in scheduleEveryTimeThisChannelList) Log.Debug("Every time on this chan. schedule: {0} {1} - {2}", schedule.ProgramName, schedule.StartTime, schedule.EndTime);
        Log.Debug("------------------------------------------------");
      }
      #endregion
      // Rebuilds a list with all schedules to parse
      scheduleList.Clear();
      foreach (Schedule schedule in scheduleOnceList) scheduleList.Add(schedule);
      foreach (Schedule schedule in scheduleRepeatingList) scheduleList.Add(schedule);
      foreach (Schedule schedule in scheduleEveryTimeEveryChannelList) scheduleList.Add(schedule);
      foreach (Schedule schedule in scheduleEveryTimeThisChannelList) scheduleList.Add(schedule);
      // try to assign all schedules to existing cards

      if (cmDebug) Log.Debug("Calling assignSchedulestoCards with {0} schedules", scheduleList.Count);
      List<Schedule>[] assignedList = AssignSchedulesToCards(scheduleList);

      //List<Conflict> _conflicts = new List<Conflict>();
    }

    #endregion

    #region private members

    /// <summary>
    /// Removes all records in Table : Conflict
    /// </summary>
    private static void ClearConflictTable()
    {
      // clears all conflicts in db
      IList conflictList = Conflict.ListAll();
      foreach (Conflict aconflict in conflictList) aconflict.Remove();
    }

    private void Init()
    {
    }

    /// <summary>
    /// Checks if 2 scheduled recordings are overlapping
    /// </summary>
    /// <param name="Schedule 1"></param>
    /// <param name="Schedule 2"></param>
    /// <returns>true if sheduled recordings are overlapping, false either</returns>
    static private bool IsOverlap(Schedule schedule1, Schedule schedule2)
    {
      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2    s--------------------------------e
      // sch_2  ------------------e
      if (
        ((schedule2.StartTime.AddMinutes(-schedule2.PreRecordInterval) >= schedule1.StartTime.AddMinutes(-schedule1.PreRecordInterval)) && (schedule2.StartTime.AddMinutes(-schedule2.PreRecordInterval) < schedule1.EndTime.AddMinutes(schedule1.PostRecordInterval))) ||
          ((schedule2.StartTime.AddMinutes(-schedule2.PreRecordInterval) <= schedule1.StartTime.AddMinutes(-schedule1.PreRecordInterval)) && (schedule2.EndTime.AddMinutes(schedule2.PostRecordInterval) >= schedule1.EndTime.AddMinutes(schedule1.PostRecordInterval))) ||
          ((schedule2.EndTime.AddMinutes(schedule2.PostRecordInterval) > schedule1.StartTime.AddMinutes(-schedule1.PreRecordInterval)) && (schedule2.EndTime.AddMinutes(schedule2.PostRecordInterval) <= schedule1.EndTime.AddMinutes(schedule1.PostRecordInterval)))
        ) return true;
      return false;
    }

    /// <summary>Assign all shedules to cards</summary>
    /// <param name="Schedules">An IList containing all scheduled recordings</param>
    /// <returns>Array of List<Schedule> : one per card, index [0] contains unassigned schedules</returns>
    private List<Schedule>[] AssignSchedulesToCards(IList Schedules)
    {
      IList cardsList = cmLayer.Cards;
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
          if (card.canViewTvChannel(schedule.IdChannel))
          {
            // checks if any schedule assigned to this cards overlaps current parsed schedule
            bool free = true;
            foreach (Schedule assignedShedule in cardSchedules[cardno[card.IdCard]])
            {
              if (schedule.IsOverlapping(assignedShedule))
              {
                if(!(schedule.isSameTransponder(assignedShedule) && card.supportSubChannels) )
                free = false;
                //_overlap = true;
                lastOverlappingSchedule = assignedShedule;
                lastBusyCard = card.IdCard;
                break;
              }
            }
            if (free)
            {
              cardSchedules[cardno[card.IdCard]].Add(schedule);
              assigned = true;
              if (overlap)
              {
                schedule.RecommendedCard = card.IdCard;
                schedule.Persist();
              }
              break;
            }
          }
        }
        if (!assigned)
        {
          cardSchedules[0].Add(schedule);
          Conflict newConflict = new Conflict(schedule.IdSchedule, lastOverlappingSchedule.IdSchedule, schedule.IdChannel, schedule.StartTime);
          newConflict.IdCard = lastBusyCard;
          newConflict.Persist();

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
      int _counter = 0;
      foreach (Card _card in _cards)
      {
        if (_card.canViewTvChannel(_shedule.IdChannel)) _counter++;
      }
      return _counter;
    }

    /// <summary>
    /// Splits a schedules list 
    /// Sorts each element in a List array
    /// where list[x] contains schedules that can be viewed by x cards
    /// </summary>
    /// <param name="Schedules">a shedules IList</param>
    /// <returns>a list array of schedules. Element [0] contains schedules that cannot be viewed by any card</returns>
    private List<Schedule>[] sortSchedules(IList Schedules)
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
            if (IsOverlap(_unassignedSchedule, _assignedSchedule))
            {
              
            }//if (IsOverlap(_unassignedSchedule, _assignedSchedule))
          }//foreach (Schedule _assignedSchedule in _listToSolve[i])
        }//for (int i = 1; i <= _cardsCount; i++)
      }//foreach (Schedule _unassignedSchedule in _listToSolve[0])
      return _sortedList;
    }

    /// <summary>
    /// gets "Record Once" Schedules in a list of schedules
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the "record once" schedules</returns>
    private IList getRecordOnceSchedules(IList schedulesList)
    {
      IList recordOnceSchedules = new List<Schedule>();
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.Once) continue;
        recordOnceSchedules.Add(schedule);
      }
      return recordOnceSchedules;
    }

    /// <summary>
    /// gets Repeating Schedules (Daily, Weekly,..) in a given list of schedules
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the Daily schedules</returns>
    private IList getRepeatingSchedules(IList schedulesList)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      IList incomingSchedules = new List<Schedule>();
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.Daily
            && scheduleType != ScheduleRecordingType.WorkingDays
            && scheduleType != ScheduleRecordingType.Weekends
            && scheduleType != ScheduleRecordingType.Weekly)
        {
          continue;
        }

        Channel channel = schedule.ReferencedChannel();
        if (layer.GetPrograms(channel, DateTime.Now, DateTime.Now.AddDays(1)).Count > 0)
        {
          // This channel has a guide, so let's find the scheduled programs there.
          IList programsList = layer.SearchMinimalPrograms(DateTime.Now, DateTime.Now.AddYears(1), schedule.ProgramName, channel);
          if (programsList != null)
          {
            foreach (Program program in programsList)
            {
              if (schedule.IsRecordingProgram(program, true) && program.EndTime >= DateTime.Now)
              {
                Schedule incomingSchedule = schedule.Clone();
                incomingSchedule.IdChannel = program.IdChannel;
                incomingSchedule.ProgramName = program.Title;
                incomingSchedule.StartTime = program.StartTime;
                incomingSchedule.EndTime = program.EndTime;

                incomingSchedule.PreRecordInterval = schedule.PreRecordInterval;
                incomingSchedule.PostRecordInterval = schedule.PostRecordInterval;
                incomingSchedules.Add(incomingSchedule);
              }
            }
          }//foreach (Program _program in _programsList)
        }
        else
        {
          // This channel does not seem to have a guide, so use a simple calculation.
          DateTime todayStartTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, 0);

          // generate the daily schedules for the next 30 days
          for (int i = 0; i <= 30; i++)
          {
            DateTime tempTime = todayStartTime.AddDays(i);
            if (tempTime.Date >= schedule.StartTime.Date)
            {
              DateTime recStartTime;
              DateTime recEndTime;
              if (schedule.GetTimesNearestTo(tempTime, out recStartTime, out recEndTime))
              {
                Schedule incomingSchedule = schedule.Clone();
                incomingSchedule.StartTime = recStartTime;
                incomingSchedule.EndTime = recEndTime;
                incomingSchedules.Add(incomingSchedule);
              }
            }//if (_tempDate>=_Schedule.StartTime)
          }//for (int i = 0; i <= 30; i++)
        }
      }
      return incomingSchedules;
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

    /// <summary>
    /// get incoming "EveryTimeOnThisChannel" type schedules in Program Table
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the schedules</returns>
    private IList getEveryTimeOnEveryChannelSchedules(IList schedulesList)
    {
      IList incomingSchedules = new List<Schedule>();
      IList programsList = Program.ListAll();
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.EveryTimeOnEveryChannel) continue;
        foreach (Program program in programsList)
        {
          if ((program.Title == schedule.ProgramName) && (program.IdChannel == schedule.IdChannel) && (program.EndTime >= DateTime.Now))
          {
            Schedule incomingSchedule = schedule.Clone();
            incomingSchedule.IdChannel = program.IdChannel;
            incomingSchedule.ProgramName = program.Title;
            incomingSchedule.StartTime = program.StartTime;
            incomingSchedule.EndTime = program.EndTime;
            incomingSchedule.PreRecordInterval = schedule.PreRecordInterval;
            incomingSchedule.PostRecordInterval = schedule.PostRecordInterval;
            incomingSchedules.Add(incomingSchedule);
          }
        }//foreach (Program _program in _programsList)
      }//foreach (Schedule _Schedule in schedulesList)
      return incomingSchedules;
    }

    /// <summary>
    /// Gets the every time on this channel schedules.
    /// </summary>
    /// <param name="schedulesList">The schedules list.</param>
    /// <returns></returns>
    private IList getEveryTimeOnThisChannelSchedules(IList schedulesList)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      IList incomingSchedules = new List<Schedule>();
      foreach (Schedule schedule in schedulesList)
      {
        ScheduleRecordingType scheduleType = (ScheduleRecordingType)schedule.ScheduleType;
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (scheduleType != ScheduleRecordingType.EveryTimeOnThisChannel) continue;
        Channel channel = Channel.Retrieve(schedule.IdChannel);
        IList programsList = layer.SearchMinimalPrograms(DateTime.Now, DateTime.Now.AddYears(1), schedule.ProgramName, channel);
        if (programsList != null)
        {
          foreach (Program program in programsList)
          {
            Schedule incomingSchedule = schedule.Clone();
            incomingSchedule.IdChannel = program.IdChannel;
            incomingSchedule.ProgramName = program.Title;
            incomingSchedule.StartTime = program.StartTime;
            incomingSchedule.EndTime = program.EndTime;

            incomingSchedule.PreRecordInterval = schedule.PreRecordInterval;
            incomingSchedule.PostRecordInterval = schedule.PostRecordInterval;
            incomingSchedules.Add(incomingSchedule);
          }
        }//foreach (Program _program in _programsList)
      }//foreach (Schedule _Schedule in schedulesList)
      return incomingSchedules;
    }

    #endregion
  }
}
