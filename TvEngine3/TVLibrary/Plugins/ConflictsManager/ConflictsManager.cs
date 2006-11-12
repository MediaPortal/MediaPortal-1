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
        return "1.0.0.0";
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
      Log.WriteFile("plugin: ConflictsManager stopped");
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
        UpdateConflicts();
    }

    /// <summary>
    /// Plugin setup form
    /// </summary>
    public SetupTv.SectionSettings Setup
    {
      get { return null; }
    }

    /// <summary>
    /// Parses the sheduled recordings
    /// and updates the conflicting recordings table
    /// </summary>
    public void UpdateConflicts()
    {
      Log.Info("ConflictManager: Updating conflicts list");
      ClearConflictTable();
      //
      IList _schedules = Schedule.ListAll();
      List<Schedule>[] sortedList = AssignSchedulesToCards(_schedules);
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
      IList _cList = Conflict.ListAll();
      foreach (Conflict aconflict in _cList) aconflict.Remove();
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
    static private bool IsOverlap(Schedule sched_1, Schedule sched_2)
    {
      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2    s--------------------------------e
      // sch_2  ------------------e
      if (
        ((sched_2.StartTime.AddMinutes(-sched_2.PreRecordInterval) >= sched_1.StartTime.AddMinutes(-sched_1.PreRecordInterval)) && (sched_2.StartTime.AddMinutes(-sched_2.PreRecordInterval) < sched_1.EndTime.AddMinutes(sched_1.PostRecordInterval))) ||
          ((sched_2.StartTime.AddMinutes(-sched_2.PreRecordInterval) <= sched_1.StartTime.AddMinutes(-sched_1.PreRecordInterval)) && (sched_2.EndTime.AddMinutes(sched_2.PostRecordInterval) >= sched_1.EndTime.AddMinutes(sched_1.PostRecordInterval))) ||
          ((sched_2.EndTime.AddMinutes(sched_2.PostRecordInterval) > sched_1.StartTime.AddMinutes(-sched_1.PreRecordInterval)) && (sched_2.EndTime.AddMinutes(sched_2.PostRecordInterval) <= sched_1.EndTime.AddMinutes(sched_1.PostRecordInterval)))
        ) return true;
      return false;
    }

    /// <summary>Assign all shedules to cards</summary>
    /// <param name="Schedules">An IList containing all scheduled recordings</param>
    /// <returns>Array of List<Schedule> : one per card, index [0] contains unassigned schedules</returns>
    private List<Schedule>[] AssignSchedulesToCards(IList Schedules)
    {
      IList _cards = cmLayer.Cards;
      // creates an array of Schedule ILists
      // element [0] will be filled with conflicting schedules
      // element [x] will be filled with the schedules assigned to card with idcard=x
      List<Schedule>[] _cardSchedules = new List<Schedule>[_cards.Count + 1];
      for (int i = 0; i < _cards.Count + 1; i++) _cardSchedules[i] = new List<Schedule>();

      #region assigns schedules from table
      foreach (Schedule _Schedule in Schedules)
      {
        bool _assigned = false;
        Schedule _lastOverlappingSchedule = null;
        int _lastBusyCard = 0;
        foreach (Card _card in _cards)
        {
          if (_card.canViewTvChannel(_Schedule.IdChannel))
          {
            // checks if any schedule assigned to this cards overlaps current parsed schedule
            bool free = true;
            foreach (Schedule _assignedShedule in _cardSchedules[_card.IdCard])
            {
              if (IsOverlap(_Schedule, _assignedShedule))
              {
                free = false;
                _lastOverlappingSchedule = _assignedShedule;
                _lastBusyCard = _card.IdCard;
                break;
              }
            }
            if (free)
            {
              _cardSchedules[_card.IdCard].Add(_Schedule);
              _assigned = true;
              _Schedule.RecommendedCard = _card.IdCard;
              _Schedule.Persist();
              break;
            }
          }
        }
        if (!_assigned)
        {
          _cardSchedules[0].Add(_Schedule);
          Conflict _newConflict = new Conflict(_Schedule.IdSchedule, _lastOverlappingSchedule.IdSchedule, _Schedule.IdChannel, _Schedule.StartTime);
          _newConflict.IdCard = _lastBusyCard;
          _newConflict.Persist();

        }
      }
      #endregion

      #region gets incoming periodic schedules for the next 30 days
      List<Schedule> _incomingSchedules = new List<Schedule>();
      foreach (Schedule _Schedule in _schedules)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          switch (scheduleType)
          {
            // daily
            case ScheduleRecordingType.Daily:
              {
                // first set the Original schedule's date to today's date
                if (_Schedule.StartTime.Day != _Schedule.EndTime.Day)
                {
                  _Schedule.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _Schedule.StartTime.Hour, _Schedule.StartTime.Minute, _Schedule.StartTime.Second);
                  _Schedule.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _Schedule.EndTime.Hour, _Schedule.EndTime.Minute, _Schedule.EndTime.Second);
                  _Schedule.EndTime = _Schedule.EndTime.AddDays(1);
                  _Schedule.Persist();
                }
                else
                {
                  _Schedule.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _Schedule.StartTime.Hour, _Schedule.StartTime.Minute, _Schedule.StartTime.Second);
                  _Schedule.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _Schedule.EndTime.Hour, _Schedule.EndTime.Minute, _Schedule.EndTime.Second);
                  _Schedule.Persist();
                }
                // then generate the daily schedules for the next 30 days
                for (int i = 1; i <= 30; i++)
                {
                  Schedule _incomingSchedule = _Schedule.Clone();
                  _incomingSchedule.StartTime = _incomingSchedule.StartTime.AddDays(i);
                  _incomingSchedule.EndTime = _incomingSchedule.EndTime.AddDays(i);
                  _incomingSchedules.Add(_incomingSchedule);
                }
              }
              break;
            // weekly 
            case ScheduleRecordingType.Weekly:
              {
              }
              break;
            // EveryTimeOnThisChannel
            case ScheduleRecordingType.EveryTimeOnThisChannel:
              {
              }
              break;
            // EveryTimeOnEveryChannel
            case ScheduleRecordingType.EveryTimeOnEveryChannel:
              {
              }
              break;
            // Weekends
            case ScheduleRecordingType.Weekends:
              {
              }
              break;
            // WorkingDays
            case ScheduleRecordingType.WorkingDays:
              {
              }
              break;
          }//switch (scheduleType)
        }//if (_Schedule.Canceled != null)
      }//foreach (Schedule _Schedule in _schedules)
      #endregion

      #region assigns incoming periodic schedules
      foreach (Schedule _Schedule in _incomingSchedules)
      {
        bool _assigned = false;
        Schedule _lastOverlappingSchedule = null;
        int _lastBusyCard = 0;
        foreach (Card _card in _cards)
        {
          if (_card.canViewTvChannel(_Schedule.IdChannel))
          {
            // checks if any schedule assigned to this cards overlaps current parsed schedule
            bool free = true;
            foreach (Schedule _assignedShedule in _cardSchedules[_card.IdCard])
            {
              if (IsOverlap(_Schedule, _assignedShedule))
              {
                free = false;
                _lastOverlappingSchedule = _assignedShedule;
                _lastBusyCard = _card.IdCard;
                break;
              }
            }
            if (free)
            {
              _cardSchedules[_card.IdCard].Add(_Schedule);
              _assigned = true;
              _Schedule.RecommendedCard = _card.IdCard;
              break;
            }
          }
        }
        if (!_assigned)
        {
          _cardSchedules[0].Add(_Schedule);
          Conflict _newConflict = new Conflict(_Schedule.IdSchedule, _lastOverlappingSchedule.IdSchedule, _Schedule.IdChannel, _Schedule.StartTime);
          _newConflict.IdCard = _lastBusyCard;
          _newConflict.Persist();

        }
      }
      #endregion

      return _cardSchedules;
    }

    #endregion
  }
}
