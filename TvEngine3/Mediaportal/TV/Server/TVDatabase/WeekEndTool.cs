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
using System.Threading;

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

    private static bool _startDayLoaded = false;
    private static DayOfWeek _firstWorkingDay = DayOfWeek.Monday;

    #region public method

    private static int GetDayIndex(DayOfWeek day)
    {
      return ((int)day + 7 - (int)GetFirstWorkingDay()) % 7;
    }

    private static DayOfWeek GetDayByIndex(int dayIndex)
    {
      return (DayOfWeek)((dayIndex + (int)GetFirstWorkingDay()) % 7);
    }

    /// <summary>
    /// Returns true if the day is a weekend day
    /// </summary>
    /// <value>true for weekend day, false for working day.</value>
    public static bool IsWeekend(DayOfWeek today)
    {
      int index = GetDayIndex(today);
      return index == 5 || index == 6;
    }

    private static DayOfWeek GetFirstWorkingDay()
    {
      if (!_startDayLoaded)
      {
        // load first working day from database
        var setting = Setting.RetrieveByTag("FirstDayOfWeekend");
        if (setting == null)
        {
          _firstWorkingDay = DayOfWeek.Monday;
        }
        else
        {
          int dayOfWeek = Convert.ToInt16(setting.Value);
          // in DayOfWeek enum Sunday = 0 so need to convert from value stored in database
          // which is Saturday = 0, Sunday=1 etc
          _firstWorkingDay = (DayOfWeek)((dayOfWeek + 1) % 7); 
        }
        _startDayLoaded = true;
      }

      return _firstWorkingDay;
    }

    /// <summary>
    /// Returns true if the day is a working day
    /// </summary>
    /// <value>true for working day, false for weekend day.</value>
    public static bool IsWorkingDay(DayOfWeek today)
    {
      return !IsWeekend(today);
    }

    private static int GetText(DayOfWeek dayOfWeek)
    {
      switch (dayOfWeek)
      {
        case DayOfWeek.Monday:
          return 657;
        case DayOfWeek.Tuesday:
          return 658;
        case DayOfWeek.Wednesday:
          return 659;
        case DayOfWeek.Thursday:
          return 660;
        case DayOfWeek.Friday:
          return 661;
        case DayOfWeek.Saturday:
          return 662;
        case DayOfWeek.Sunday:
          return 663;
        default:
          return -1;
      }
    }

    /// <summary>
    /// Returns the localiztion text of a specified week day type
    /// </summary>
    public static int GetText(DayType dayType)
    {
      switch (dayType)
      {
        case DayType.FirstWorkingDay:
          return GetText(GetDayByIndex(0));
        case DayType.LastWorkingDay:
          return GetText(GetDayByIndex(4));
        case DayType.FirstWeekendDay:
          return GetText(FirstWeekendDay);
        case DayType.LastWeekendDay:
          return GetText(SecondWeekendDay);
        case DayType.WorkingDays:
          return 680;
        case DayType.Record_WorkingDays:
          return 672;
        case DayType.WeekendDays:
          return 1050;
        case DayType.Record_WeekendDays:
          return 1051;
        default:
          return -1;
      }
    }

    /// <summary>
    /// Is this the first day of the weekend?
    /// </summary>
    /// <value>true for the first weekend day.</value>
    public static bool IsFirstWeekendDay(DayOfWeek today)
    {
      return today == FirstWeekendDay;
    }

    /// <summary>
    /// Returns the value of the first weekend day.
    /// </summary>
    /// <value>the first weekend day</value>
    public static DayOfWeek FirstWeekendDay
    {
      get { return GetDayByIndex(5); }
    }

    /// <summary>
    /// Returns the value of the second weekend day.
    /// </summary>
    /// <value>the second weekend day</value>
    public static DayOfWeek SecondWeekendDay
    {
      get { return GetDayByIndex(6); }
    }

    /// <summary>
    /// Returns a comma separated list of the weekend days as 
    /// numbers suitable for use in SQL queries
    /// </summary>
    /// <value>the weekend days list</value>
    public static string SqlWeekendDays
    {
      get
      {
        int firstWeekend = (int)FirstWeekendDay + 1;
        int secondWeekend = (int)SecondWeekendDay + 1;
        return "" + firstWeekend + "," + secondWeekend;
      }
    }

    /// <summary>
    /// Returns a comma separated list of the working days as 
    /// numbers suitable for use in SQL queries
    /// </summary>
    /// <value>the working days list</value>
    public static string SqlWorkingDays
    {
      get
      {
        string result = "";
        for (int i = 0; i < 5; i++)
        {
          int sqlDayNumber = (int)GetDayByIndex(i) + 1;
          if (i != 0)
          {
            result += ",";
          }
          result += sqlDayNumber;
        }
        return result;
      }
    }

    #endregion
  }
}