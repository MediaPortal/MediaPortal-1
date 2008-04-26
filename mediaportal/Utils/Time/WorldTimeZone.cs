#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Win32;

namespace MediaPortal.Utils.Time
{
  /// <summary>
  /// A World Time Zone
  /// </summary>
  public class WorldTimeZone : TimeZone
  {
    #region Variables
    //private const string VALUE_INDEX = "Index";
    private const string VALUE_DISPLAY_NAME = "Display";
    private const string VALUE_STANDARD_NAME = "Std";
    private const string VALUE_DAYLIGHT_NAME = "Dlt";
    private const string VALUE_ZONE_INFO = "TZI";
    private const int LENGTH_ZONE_INFO = 44;
    private const int LENGTH_DWORD = 4;
    private const int LENGTH_WORD = 2;
    private const int LENGTH_SYSTEMTIME = 16;
    private static string[] REG_KEYS_TIME_ZONES = { 
          "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones",
          "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Time Zones" };

    private static Dictionary<string, TimeZoneInfo> _TimeZoneList = null;
    private static Dictionary<string, string> _TimeZoneNames = null;

    private TimeZoneInfo _TimeZone;

    private bool _bHasDlt;
    #endregion

    //#region Structs
    //private struct TimeZoneDate
    //{
    //  public int Month;
    //  public DayOfWeek DayOfWeek;
    //  public int WeekOfMonth;
    //  public TimeSpan TimeOfDay;
    //}

    //private struct TimeZoneInfo
    //{
    //  //public int Index;
    //  public string Display;
    //  public string StdName;
    //  public string DltName;

    //  public Int32 Offset;
    //  public Int32 StdOffset;
    //  public Int32 DltOffset;

    //  public TimeZoneDate StdDate;
    //  public TimeZoneDate DltDate;
    //}
    //#endregion

    #region Constructors/Destructors
    /// <summary>
    /// Initializes a new instance of the <see cref="WorldTimeZone"/> class.
    /// </summary>
    /// <param name="TimeZone">The time zone.</param>
    public WorldTimeZone(string TimeZone)
    {
      if (TimeZone == string.Empty)
        throw new ArgumentException("TimeZone Not valid");

      string TimeZoneName = string.Empty;

      if (_TimeZoneList == null)
        LoadRegistryTimeZones();

      if (_TimeZoneNames.ContainsKey(TimeZone))
        TimeZoneName = _TimeZoneNames[TimeZone];

      if (_TimeZoneList.ContainsKey(TimeZone))
        TimeZoneName = TimeZone;

      if (TimeZoneName == string.Empty)
        throw new ArgumentException("TimeZone Not valid");

      _TimeZone = (TimeZoneInfo)_TimeZoneList[TimeZoneName];

      _bHasDlt = true;
      if (_TimeZone.DltDate.Month == 0)
      {
        _bHasDlt = false;
      }
      else
      {
        if (_TimeZone.DltDate.WeekOfMonth < 0
            || _TimeZone.DltDate.WeekOfMonth > 4)
          _bHasDlt = false;
      }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Determines whether [is local time zone].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is local time zone]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocalTimeZone()
    {
      if (TimeZone.CurrentTimeZone.StandardName == this.StandardName)
        return true;

      return false;
    }

    /// <summary>
    /// Converts DateTime to the local time.
    /// </summary>
    /// <param name="time">The time.</param>
    /// <returns>Local DateTime</returns>
    public DateTime FromLocalTime(DateTime time)
    {
      if (time.Kind != DateTimeKind.Unspecified)
        time = new DateTime(time.Ticks, DateTimeKind.Unspecified);

      return time.Add(GetUtcOffset(time) - System.TimeZone.CurrentTimeZone.GetUtcOffset(time));
    }

    public static List<TimeZoneInfo> GetTimeZones()
    {
      if (_TimeZoneList == null)
        LoadRegistryTimeZones();

      List<TimeZoneInfo> timezonelist = new List<TimeZoneInfo>();

      foreach (TimeZoneInfo timezone in _TimeZoneList.Values)
        timezonelist.Add(timezone);

      return timezonelist;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Gets the date time.
    /// </summary>
    /// <param name="TimeChange">The time change.</param>
    /// <param name="year">The year.</param>
    /// <returns></returns>
    private DateTime GetDateTime(TimeZoneDate TimeChange, int year)
    {
      DateTime ChangeDay;
      int MonthOffset = 0;

      if (TimeChange.WeekOfMonth == 4)
      {
        int LastDay = DateTime.DaysInMonth(year, TimeChange.Month);
        ChangeDay = new DateTime(year, TimeChange.Month, LastDay);

        int MonthEnd = (int)ChangeDay.DayOfWeek;
        int WeekDay = (int)TimeChange.DayOfWeek;

        if (MonthEnd != WeekDay)
        {
          if (MonthEnd > WeekDay)
            MonthOffset = 0 - (MonthEnd - WeekDay);
          else
            MonthOffset = 0 - (MonthEnd + 7 - WeekDay);
        }
      }
      else
      {
        ChangeDay = new DateTime(year, TimeChange.Month, 1);

        int MonthStart = (int)ChangeDay.DayOfWeek;
        int WeekDay = (int)TimeChange.DayOfWeek;

        if (MonthStart != WeekDay)
        {
          if (WeekDay > MonthStart)
            MonthOffset = WeekDay - MonthStart;
          else
            MonthOffset = WeekDay + 7 - MonthStart;
        }
        MonthOffset += 7 * TimeChange.WeekOfMonth;
      }
      ChangeDay = ChangeDay.AddDays(MonthOffset);
      ChangeDay = ChangeDay.Add(TimeChange.TimeOfDay);

      return ChangeDay;
    }

    /// <summary>
    /// Loads the registry time zones.
    /// </summary>
    private static void LoadRegistryTimeZones()
    {
      RegistryKey RegKeyRoot = null;

      foreach (string currentRegKey in REG_KEYS_TIME_ZONES)
      {
        RegKeyRoot = Registry.LocalMachine.OpenSubKey(currentRegKey);
        if (RegKeyRoot != null)
          break;
        else
          RegKeyRoot.Close();
      }

      if (RegKeyRoot != null)
      {
        _TimeZoneList = new Dictionary<string, TimeZoneInfo>();
        _TimeZoneNames = new Dictionary<string, string>();
        string[] timeZoneKeys = RegKeyRoot.GetSubKeyNames();

        for (int i = 0; i < timeZoneKeys.Length; i++)
        {
          try
          {
            using (RegistryKey TZKey = RegKeyRoot.OpenSubKey(timeZoneKeys[i]))
              if (TZKey != null && TZKey.ValueCount > 0)
              {
                TimeZoneInfo TZInfo = new TimeZoneInfo();

                //TZInfo.Index = (int)TZKey.GetValue(VALUE_INDEX);
                TZInfo.Display = (string)TZKey.GetValue(VALUE_DISPLAY_NAME);
                TZInfo.StdName = (string)TZKey.GetValue(VALUE_STANDARD_NAME);
                TZInfo.DltName = (string)TZKey.GetValue(VALUE_DAYLIGHT_NAME);
                byte[] timeZoneData = (byte[])TZKey.GetValue(VALUE_ZONE_INFO);

                if (timeZoneData != null)
                {
                  int index = 0;
                  TZInfo.Offset = BitConverter.ToInt32(timeZoneData, index);
                  index += LENGTH_DWORD;
                  TZInfo.StdOffset = BitConverter.ToInt32(timeZoneData, index);
                  index += LENGTH_DWORD;
                  TZInfo.DltOffset = BitConverter.ToInt32(timeZoneData, index);
                  index += LENGTH_DWORD;
                  TZInfo.StdDate = GetDate(timeZoneData, index);
                  index += LENGTH_SYSTEMTIME;
                  TZInfo.DltDate = GetDate(timeZoneData, index);

                  _TimeZoneList.Add(timeZoneKeys[i], TZInfo);

                  if (!_TimeZoneNames.ContainsKey(TZInfo.StdName))
                    _TimeZoneNames.Add(TZInfo.StdName, timeZoneKeys[i]);
                }

              }
          }
          catch (Exception)
          {
          }
        }
        RegKeyRoot.Close();
      }
    }

    /// <summary>
    /// Gets the date.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="index">The index.</param>
    /// <returns>TimeZoneDate</returns>
    private static TimeZoneDate GetDate(byte[] bytes, Int32 index)
    {
      TimeZoneDate TimeChange = new TimeZoneDate();

      //int Year = BitConverter.ToInt16(bytes, index);
      index += LENGTH_WORD;
      TimeChange.Month = BitConverter.ToInt16(bytes, index);

      index += LENGTH_WORD;
      TimeChange.DayOfWeek = (DayOfWeek)BitConverter.ToInt16(bytes, index);

      index += LENGTH_WORD;
      TimeChange.WeekOfMonth = BitConverter.ToInt16(bytes, index) - 1;

      index += LENGTH_WORD;
      int Hours = BitConverter.ToInt16(bytes, index);

      index += LENGTH_WORD;
      int Minutes = BitConverter.ToInt16(bytes, index);

      index += LENGTH_WORD;
      int Seconds = BitConverter.ToInt16(bytes, index);

      //index += LENGTH_WORD;
      //int Milliseconds = BitConverter.ToInt16(bytes, index);

      TimeChange.TimeOfDay = new TimeSpan(Hours, Minutes, Seconds);

      return TimeChange;
    }
    #endregion

    #region TimeZone Overloads
    /// <summary>
    /// Gets the daylight saving time zone name.
    /// </summary>
    /// <value></value>
    /// <returns>The daylight saving time zone name.</returns>
    public override string DaylightName
    {
      get { return _TimeZone.DltName; }
    }

    /// <summary>
    /// Gets the standard time zone name.
    /// </summary>
    /// <value></value>
    /// <returns>The standard time zone name.</returns>
    /// <exception cref="T:System.ArgumentNullException">Attempted to set this property to null. </exception>
    public override string StandardName
    {
      get { return _TimeZone.StdName; }
    }

    /// <summary>
    /// Returns a value indicating whether the specified date and time is within a daylight saving time period.
    /// </summary>
    /// <param name="time">A date and time.</param>
    /// <returns>
    /// true if time is in a daylight saving time period; false otherwise, or if time is null.
    /// </returns>
    public override bool IsDaylightSavingTime(DateTime time)
    {
      if (_TimeZone.DltDate.Month == 0)   // Never Dlt time;
        return false;

      DaylightTime DLTime = GetDaylightChanges(time.Year);

      if (DLTime.Start > DLTime.End)
      {
        if (time >= DLTime.Start || time < DLTime.End)
          return true;
      }
      else
      {
        if (time >= DLTime.Start && time < DLTime.End)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Returns the daylight saving time period for a particular year.
    /// </summary>
    /// <param name="year">The year to which the daylight saving time period applies.</param>
    /// <returns>
    /// A <see cref="T:System.Globalization.DaylightTime"></see> instance containing the start and end date for daylight saving time in year.
    /// </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">year is less than 1 or greater than 9999. </exception>
    public override DaylightTime GetDaylightChanges(int year)
    {
      DaylightTime DLTime = null;

      if (_bHasDlt)
      {
        DateTime StdDay = GetDateTime(_TimeZone.StdDate, year);
        DateTime DltDay = GetDateTime(_TimeZone.DltDate, year);

        DLTime = new DaylightTime(DltDay, StdDay, new TimeSpan(0, _TimeZone.DltOffset, 0));
      }

      return DLTime;
    }

    /// <summary>
    /// Returns the coordinated universal time (UTC) that corresponds to a specified local time.
    /// </summary>
    /// <param name="time">The local date and time.</param>
    /// <returns>
    /// A <see cref="T:System.DateTime"></see> instance whose value is the UTC time that corresponds to time.
    /// </returns>
    public override DateTime ToUniversalTime(DateTime time)
    {
      return time.Add(-GetUtcOffset(time));
    }

    /// <summary>
    /// Returns the local time that corresponds to a specified coordinated universal time (UTC).
    /// </summary>
    /// <param name="time">A UTC time.</param>
    /// <returns>
    /// A <see cref="T:System.DateTime"></see> instance whose value is the local time that corresponds to time.
    /// </returns>
    public override DateTime ToLocalTime(DateTime time)
    {
      if (time.Kind != DateTimeKind.Unspecified)
        time = new DateTime(time.Ticks, DateTimeKind.Unspecified);

      return time.Add(System.TimeZone.CurrentTimeZone.GetUtcOffset(time) - GetUtcOffset(time));
    }

    /// <summary>
    /// Returns the coordinated universal time (UTC) offset for the specified local time.
    /// </summary>
    /// <param name="time">The local date and time.</param>
    /// <returns>
    /// The UTC offset from time, measured in ticks.
    /// </returns>
    public override TimeSpan GetUtcOffset(DateTime time)
    {
      int UtcOffset = _TimeZone.Offset;

      if (IsDaylightSavingTime(time))
        UtcOffset += _TimeZone.DltOffset;

      return new TimeSpan(0, -UtcOffset, 0);
    }
    #endregion
  }
}
