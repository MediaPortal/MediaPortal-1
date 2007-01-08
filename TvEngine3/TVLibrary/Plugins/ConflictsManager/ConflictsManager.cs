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
      // hmm... 
      ClearConflictTable();
      // Gets schedules from db
      IList _schedules = Schedule.ListAll();
      // parses all schedules and add the calculated incoming schedules 
      IList _once = getRecordOnceSchedules(_schedules);
      IList _daily = getDailySchedules(_schedules);
      IList _weekly = getWeeklySchedules(_schedules);
      IList _weekends = getWeekendsSchedules(_schedules);
      IList _workindays = getWorkingDaysSchedules(_schedules);
      IList _everytimeeverychannel = getEveryTimeOnEveryChannelSchedules(_schedules);
      IList _everytimethischannel = getEveryTimeOnThisChannelSchedules(_schedules);
      // test section
      bool cmDebug = true;// activate debug mode , todo : set it as a setup value
      #region debug informations
      if (cmDebug)
      {
        foreach (Schedule _asched in _once) Log.Debug("Record Once schedule: {0} {1} - {2}", _asched.ProgramName, _asched.StartTime, _asched.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule _asched in _daily) Log.Debug("Daily schedule: {0} {1} - {2}", _asched.ProgramName, _asched.StartTime, _asched.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule _asched in _weekly) Log.Debug("Weekly schedule: {0} {1} - {2}", _asched.ProgramName, _asched.StartTime, _asched.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule _asched in _weekends) Log.Debug("Weekend schedule: {0} {1} - {2}", _asched.ProgramName, _asched.StartTime, _asched.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule _asched in _workindays) Log.Debug("Working days schedule: {0} {1} - {2}", _asched.ProgramName, _asched.StartTime, _asched.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule _asched in _everytimeeverychannel) Log.Debug("Evry time on evry chan. schedule: {0} {1} - {2}", _asched.ProgramName, _asched.StartTime, _asched.EndTime);
        Log.Debug("------------------------------------------------");
        foreach (Schedule _asched in _everytimethischannel) Log.Debug("Evry time on this chan. schedule: {0} {1} - {2}", _asched.ProgramName, _asched.StartTime, _asched.EndTime);
        Log.Debug("------------------------------------------------");
      }
      #endregion
      // Rebuilds a list with all schedules to parse
      _schedules.Clear();
      foreach (Schedule _asched in _once) _schedules.Add(_asched);
      foreach (Schedule _asched in _daily) _schedules.Add(_asched);
      foreach (Schedule _asched in _weekly) _schedules.Add(_asched);
      foreach (Schedule _asched in _weekends) _schedules.Add(_asched);
      foreach (Schedule _asched in _workindays) _schedules.Add(_asched);
      foreach (Schedule _asched in _everytimeeverychannel) _schedules.Add(_asched);
      foreach (Schedule _asched in _everytimethischannel) _schedules.Add(_asched);
      // try to assign all schedules to existing cards
      Log.Debug("Calling assignSchedulestoCards with {0} schedules",_schedules.Count);
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
      // creates an array of Schedule Lists
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
        bool _overlap = false;
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
                //_overlap = true;
                _lastOverlappingSchedule = _assignedShedule;
                _lastBusyCard = _card.IdCard;
                break;
              }
            }
            if (free)
            {
              _cardSchedules[_card.IdCard].Add(_Schedule);
              _assigned = true;
              if (_overlap)
              {
                _Schedule.RecommendedCard = _card.IdCard;
                _Schedule.Persist();
              }
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

    /// <summary>
    /// gets "Record Once" Schedules in a list of schedules
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the "record once" schedules</returns>
    private IList getRecordOnceSchedules(IList schedulesList)
    {
      IList _recordOnceSchedules = new List<Schedule>();
      foreach (Schedule _Schedule in schedulesList)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          if (scheduleType == ScheduleRecordingType.Once)
          {
            _recordOnceSchedules.Add(_Schedule);
          }
        }
      }
      return _recordOnceSchedules;
    }

    /// <summary>
    /// gets Daily Schedules in a given list of schedules
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the Daily schedules</returns>
    private IList getDailySchedules(IList schedulesList)
    {
      IList _incomingSchedules = new List<Schedule>();
      foreach (Schedule _Schedule in schedulesList)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          if (scheduleType == ScheduleRecordingType.Daily)
          {
            // create a temporay base schedule with today's date
            // (will be used to calculate incoming schedules)
            // and adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
            Schedule _baseSchedule = _Schedule.Clone();
            if (_baseSchedule.StartTime.Day != _baseSchedule.EndTime.Day)
            {
              _baseSchedule.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _baseSchedule.StartTime.Hour, _baseSchedule.StartTime.Minute, _baseSchedule.StartTime.Second);
              _baseSchedule.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _baseSchedule.EndTime.Hour, _baseSchedule.EndTime.Minute, _baseSchedule.EndTime.Second);
              _baseSchedule.EndTime = _baseSchedule.EndTime.AddDays(1);
            }
            else
            {
              _baseSchedule.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _baseSchedule.StartTime.Hour, _baseSchedule.StartTime.Minute, _baseSchedule.StartTime.Second);
              _baseSchedule.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _baseSchedule.EndTime.Hour, _baseSchedule.EndTime.Minute, _baseSchedule.EndTime.Second);
            }

            // generate the daily schedules for the next 30 days
            DateTime _tempDate;
            for (int i = 0; i <= 30; i++)
            {
              _tempDate = DateTime.Now.AddDays(i);
              if (_tempDate.Date >= _Schedule.StartTime.Date)
              {
                Schedule _incomingSchedule = _baseSchedule.Clone();
                _incomingSchedule.StartTime = _incomingSchedule.StartTime.AddDays(i);
                _incomingSchedule.EndTime = _incomingSchedule.EndTime.AddDays(i);
                _incomingSchedules.Add(_incomingSchedule);
              }//if (_tempDate>=_Schedule.StartTime)
            }//for (int i = 0; i <= 30; i++)
          }//if (scheduleType == ScheduleRecordingType.Daily)
        }//if (_Schedule.Canceled != null)
      }
      return _incomingSchedules;
    }

    /// <summary>
    /// gets Weekly Schedules in a given list of schedules for the next 30 days 
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the Weekly schedules</returns>
    private IList getWeeklySchedules(IList schedulesList)
    {
      IList _incomingSchedules = new List<Schedule>();
      foreach (Schedule _Schedule in schedulesList)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          if (scheduleType == ScheduleRecordingType.Weekly)
          {
            DateTime _tempDate;
            //  generate the weekly schedules for the next 30 days
            for (int i = 0; i <= 30; i++)
            {
              _tempDate = DateTime.Now.AddDays(i);
              if ((_tempDate.DayOfWeek == _Schedule.StartTime.DayOfWeek) && (_tempDate.Date >= _Schedule.StartTime.Date))
              {
                Schedule _tempSchedule = _Schedule.Clone();
                #region Set Schedule Time & Date
                // adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
                if (_tempSchedule.StartTime.Day != _tempSchedule.EndTime.Day)
                {
                  _tempSchedule.StartTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.StartTime.Hour, _tempSchedule.StartTime.Minute, _tempSchedule.StartTime.Second);
                  _tempSchedule.EndTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.EndTime.Hour, _tempSchedule.EndTime.Minute, _tempSchedule.EndTime.Second);
                  _tempSchedule.EndTime = _tempSchedule.EndTime.AddDays(1);
                }
                else
                {
                  _tempSchedule.StartTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.StartTime.Hour, _tempSchedule.StartTime.Minute, _tempSchedule.StartTime.Second);
                  _tempSchedule.EndTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.EndTime.Hour, _tempSchedule.EndTime.Minute, _tempSchedule.EndTime.Second);
                }
                #endregion
                _incomingSchedules.Add(_tempSchedule);
              }//if (_tempDate.DayOfWeek == _Schedule.StartTime.DayOfWeek && _tempDate >= _Schedule.StartTime)
            }//for (int i = 0; i < 30; i++)
          }//if (scheduleType == ScheduleRecordingType.Weekly)
        }//if (_Schedule.Canceled != null)
      }//foreach (Schedule _Schedule in schedulesList)
      return _incomingSchedules;
    }

    /// <summary>
    /// gets Weekends Schedules in a given list of schedules for the next 30 days 
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the Weekends schedules</returns>
    private IList getWeekendsSchedules(IList schedulesList)
    {
      IList _incomingSchedules = new List<Schedule>();
      foreach (Schedule _Schedule in schedulesList)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          if (scheduleType == ScheduleRecordingType.Weekends)
          {
            DateTime _tempDate;
            //  generate the weekly schedules for the next 30 days
            for (int i = 0; i <= 30; i++)
            {
              _tempDate = DateTime.Now.AddDays(i);
              if ((_tempDate.DayOfWeek == DayOfWeek.Saturday) || (_tempDate.DayOfWeek == DayOfWeek.Sunday) && (_tempDate.Date >= _Schedule.StartTime.Date))
              {
                Schedule _tempSchedule = _Schedule.Clone();
                #region Set Schedule Time & Date
                // adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
                if (_tempSchedule.StartTime.Day != _tempSchedule.EndTime.Day)
                {
                  _tempSchedule.StartTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.StartTime.Hour, _tempSchedule.StartTime.Minute, _tempSchedule.StartTime.Second);
                  _tempSchedule.EndTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.EndTime.Hour, _tempSchedule.EndTime.Minute, _tempSchedule.EndTime.Second);
                  _tempSchedule.EndTime = _tempSchedule.EndTime.AddDays(1);
                }
                else
                {
                  _tempSchedule.StartTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.StartTime.Hour, _tempSchedule.StartTime.Minute, _tempSchedule.StartTime.Second);
                  _tempSchedule.EndTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.EndTime.Hour, _tempSchedule.EndTime.Minute, _tempSchedule.EndTime.Second);
                }
                #endregion
                _incomingSchedules.Add(_tempSchedule);
              }//if (_tempDate.DayOfWeek == _Schedule.StartTime.DayOfWeek && _tempDate >= _Schedule.StartTime)
            }//for (int i = 0; i < 30; i++)
          }//if (scheduleType == ScheduleRecordingType.Weekly)
        }//if (_Schedule.Canceled != null)
      }//foreach (Schedule _Schedule in schedulesList)
      return _incomingSchedules;
    }

    /// <summary>
    /// gets WorkingDays Schedules in a given list of schedules for the next 30 days 
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the WorkingDays schedules</returns>
    private IList getWorkingDaysSchedules(IList schedulesList)
    {
      IList _incomingSchedules = new List<Schedule>();
      foreach (Schedule _Schedule in schedulesList)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          if (scheduleType == ScheduleRecordingType.WorkingDays)
          {
            DateTime _tempDate;
            //  generate the weekly schedules for the next 30 days
            for (int i = 0; i <= 30; i++)
            {
              _tempDate = DateTime.Now.AddDays(i);
              if ((_tempDate.DayOfWeek != DayOfWeek.Saturday) && (_tempDate.DayOfWeek != DayOfWeek.Sunday) && (_tempDate.Date >= _Schedule.StartTime.Date))
              {
                Schedule _tempSchedule = _Schedule.Clone();
                #region Set Schedule Time & Date
                // adjusts Endtime for schedules that overlap 2 days (eg : 23:00 - 00:30)
                if (_tempSchedule.StartTime.Day != _tempSchedule.EndTime.Day)
                {
                  _tempSchedule.StartTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.StartTime.Hour, _tempSchedule.StartTime.Minute, _tempSchedule.StartTime.Second);
                  _tempSchedule.EndTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.EndTime.Hour, _tempSchedule.EndTime.Minute, _tempSchedule.EndTime.Second);
                  _tempSchedule.EndTime = _tempSchedule.EndTime.AddDays(1);
                }
                else
                {
                  _tempSchedule.StartTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.StartTime.Hour, _tempSchedule.StartTime.Minute, _tempSchedule.StartTime.Second);
                  _tempSchedule.EndTime = new DateTime(_tempDate.Year, _tempDate.Month, _tempDate.Day, _tempSchedule.EndTime.Hour, _tempSchedule.EndTime.Minute, _tempSchedule.EndTime.Second);
                }
                #endregion
                _incomingSchedules.Add(_tempSchedule);
              }//if (_tempDate.DayOfWeek == _Schedule.StartTime.DayOfWeek && _tempDate >= _Schedule.StartTime)
            }//for (int i = 0; i < 30; i++)
          }//if (scheduleType == ScheduleRecordingType.Weekly)
        }//if (_Schedule.Canceled != null)
      }//foreach (Schedule _Schedule in schedulesList)
      return _incomingSchedules;
    }

    /// <summary>
    /// get incoming "EveryTimeOnThisChannel" type schedules in Program Table
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the schedules</returns>
    private IList getEveryTimeOnEveryChannelSchedules(IList schedulesList)
    {
      IList _incomingSchedules = new List<Schedule>();
      IList _programsList = Program.ListAll();
      foreach (Schedule _Schedule in schedulesList)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          if (scheduleType == ScheduleRecordingType.EveryTimeOnEveryChannel)
          {
            foreach (Program _program in _programsList)
            {
              if ((_program.Title == _Schedule.ProgramName) && (_program.IdChannel == _Schedule.IdChannel) && (_program.EndTime >= DateTime.Now))
              {
                Schedule _incomingSchedule = new Schedule(_program.IdChannel, _program.Title, _program.StartTime, _program.EndTime);
                _incomingSchedule.PreRecordInterval = _Schedule.PreRecordInterval;
                _incomingSchedule.PostRecordInterval = _Schedule.PostRecordInterval;
                _incomingSchedules.Add(_incomingSchedule);
              }
            }//foreach (Program _program in _programsList)
          }//if (scheduleType == ScheduleRecordingType.Weekly)
        }//if (_Schedule.Canceled != null)
      }//foreach (Schedule _Schedule in schedulesList)
      return _incomingSchedules;
    }

    /// <summary>
    /// get incoming "EveryTimeOnEveryChannel" type schedules in Program Table
    /// canceled Schedules are ignored
    /// </summary>
    /// <param name="schedulesList">a IList contaning the schedules to parse</param>
    /// <returns>a collection containing the schedules</returns>
    private IList getEveryTimeOnThisChannelSchedules(IList schedulesList)
    {
      IList _incomingSchedules = new List<Schedule>();
      IList _programsList = Program.ListAll();
      foreach (Schedule _Schedule in schedulesList)
      {
        if (_Schedule.Canceled != null)
        {
          ScheduleRecordingType scheduleType = (ScheduleRecordingType)_Schedule.ScheduleType;
          if (scheduleType == ScheduleRecordingType.EveryTimeOnThisChannel)
          {
            foreach (Program _program in _programsList)
            {
              if ((_program.Title == _Schedule.ProgramName) && (_program.EndTime >= DateTime.Now))
              {
                Schedule _incomingSchedule = new Schedule(_program.IdChannel, _program.Title, _program.StartTime, _program.EndTime);
                _incomingSchedule.PreRecordInterval = _Schedule.PreRecordInterval;
                _incomingSchedule.PostRecordInterval = _Schedule.PostRecordInterval;
                _incomingSchedules.Add(_incomingSchedule);
              }
            }//foreach (Program _program in _programsList)
          }//if (scheduleType == ScheduleRecordingType.Weekly)
        }//if (_Schedule.Canceled != null)
      }//foreach (Schedule _Schedule in schedulesList)
      return _incomingSchedules;
    }

    #endregion
  }
}
