#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;

namespace TvDatabase
{
  /// <summary>
  /// Specifies types of days in the week
  /// </summary>
  public enum DayType
  {
    /// <summary>
    /// The first working day
    /// </summary>
    FirstWorkingDay,
    /// <summary>
    /// The last working day
    /// </summary>
    LastWorkingDay,
    /// <summary>
    /// The first weekend day
    /// </summary>
    FirstWeekendDay,
    /// <summary>
    /// The last weekend day
    /// </summary>
    LastWeekendDay,
    /// <summary>
    /// The working day
    /// </summary>
    WorkingDays,
    /// <summary>
    /// The record working days
    /// </summary>
    Record_WorkingDays,
    /// <summary>
    /// The weekend day
    /// </summary>
    WeekendDays,
    /// <summary>
    /// The record weekend day
    /// </summary>
    Record_WeekendDays
  }

  /// <summary>
  /// Class for support weekend starting on Saturday or Friday
  /// </summary>
  public class WeekEndTool
  {
    private readonly bool _deltaInWorkingDay; // Is there a delta in the first working days?
    //  False= week starts on Monday.
    //  True=  week starts on Sunday.

    /// <summary>
    /// Interal constructor
    /// </summary>
    /// <param name="deltaInWorkingDay">Delta for working day</param>
    internal WeekEndTool(bool deltaInWorkingDay)
    {
      _deltaInWorkingDay = deltaInWorkingDay;
    }

    #region public method

    /// <summary>
    /// Returns true if the day is a weekend day
    /// </summary>
    /// <value>true for weekend day, false for working day.</value>
    public bool IsWeekend(DayOfWeek Today)
    {
      return ((!_deltaInWorkingDay && (Today == DayOfWeek.Saturday || Today == DayOfWeek.Sunday))
              || (_deltaInWorkingDay && (Today == DayOfWeek.Friday || Today == DayOfWeek.Saturday)));
    }

    /// <summary>
    /// Returns true if the day is a working day
    /// </summary>
    /// <value>true for working day, false for weekend day.</value>
    public bool IsWorkingDay(DayOfWeek Today)
    {
      return ((!_deltaInWorkingDay && (Today != DayOfWeek.Saturday && Today != DayOfWeek.Sunday))
              || (_deltaInWorkingDay && (Today != DayOfWeek.Friday && Today != DayOfWeek.Saturday)));
    }

    /// <summary>
    /// Returns the localiztion text of a specified week day type
    /// </summary>
    public int GetText(DayType Day)
    {
      if (!_deltaInWorkingDay) // Working days starting on Monday
      {
        if (Day == DayType.FirstWorkingDay)
          return 657; //657=Mon
        if (Day == DayType.LastWorkingDay)
          return 661; //661=Fri
        if (Day == DayType.FirstWeekendDay)
          return 662; //662=Sat
        if (Day == DayType.LastWeekendDay)
          return 663; //663=Sun
        if (Day == DayType.WorkingDays)
          return 680; //680=Mon-Fri
        if (Day == DayType.Record_WorkingDays)
          return 672; //672=Record Mon-Fri
        if (Day == DayType.WeekendDays)
          return 1050; //1050=Sat-sun
        if (Day == DayType.Record_WeekendDays)
          return 1051; //1051=Record Sat-Sun
      }
      else
      {
        // Working days starting on Sunday
        if (Day == DayType.FirstWorkingDay)
          return 663; //663=Sun
        if (Day == DayType.LastWorkingDay)
          return 660; //660=Thu
        if (Day == DayType.FirstWeekendDay)
          return 661; //661=Fri
        if (Day == DayType.LastWeekendDay)
          return 662; //662=Sat
        if (Day == DayType.WorkingDays)
          return 1059; //1059=Sun-Thu
        if (Day == DayType.Record_WorkingDays)
          return 1057; //1057=Record Sun-Thu
        if (Day == DayType.WeekendDays)
          return 1060; //1060=Fri-Sat
        if (Day == DayType.Record_WeekendDays)
          return 1058; //1058=Record Fri-Sat
      }
      return -1;
    }

    /// <summary>
    /// Is this the first day of the weekend?
    /// </summary>
    /// <value>true for the first weekend day.</value>
    public bool IsFirstWeekendDay(DayOfWeek Today)
    {
      return (!_deltaInWorkingDay && (Today == DayOfWeek.Saturday))
             || (_deltaInWorkingDay && (Today == DayOfWeek.Friday));
    }

    /// <summary>
    /// Returns the value of the first weekend day.
    /// </summary>
    /// <value>the first weekend day</value>
    public DayOfWeek FirstWeekendDay
    {
      get { return !_deltaInWorkingDay ? (DayOfWeek.Saturday) : (DayOfWeek.Friday); }
    }

    /// <summary>
    /// Returns the value of the second weekend day.
    /// </summary>
    /// <value>the second weekend day</value>
    public DayOfWeek SecondWeekendDay
    {
      get { return !_deltaInWorkingDay ? (DayOfWeek.Sunday) : (DayOfWeek.Saturday); }
    }

    #endregion
  }
}