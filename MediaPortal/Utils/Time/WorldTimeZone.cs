#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Microsoft.Win32;

namespace MediaPortal.Utils.Time
{
  public class WorldTimeZone : TimeZone
  {
    private const string VALUE_INDEX = "Index";
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

    private static Hashtable _TimeZoneList = null;
    private static Hashtable _TimeZoneNames = null;

    private TimeZoneInfo _TimeZone;

    private bool _bHasDlt;

    private struct TimeZoneDate
    {
      public int Month;
      public DayOfWeek DayOfWeek;
      public int WeekOfMonth;
      public TimeSpan TimeOfDay;
    }

    private struct TimeZoneInfo
    {
      public int Index;
      public string Display;
      public string StdName;
      public string DltName;

      public Int32 Offset;
      public Int32 StdOffset;
      public Int32 DltOffset;

      public TimeZoneDate StdDate;
      public TimeZoneDate DltDate;
    }

    private WorldTimeZone()
    {
    }

    public WorldTimeZone(string TimeZone)
    {
      string TimeZoneName = string.Empty;

      if (_TimeZoneList == null)
        LoadRegistryTimeZones();

      if (_TimeZoneNames.Contains(TimeZone))
        TimeZoneName = (String)_TimeZoneNames[TimeZone];

      if (_TimeZoneList.Contains(TimeZone))
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

    public override string DaylightName
    {
      get
      {
        return _TimeZone.DltName;
      }
    }

    public override string StandardName
    {
      get
      {
        return _TimeZone.StdName;
      }
    }

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

    public override DateTime ToUniversalTime(DateTime time)
    {
      return time.Add(-GetUtcOffset(time));
    }

    public override DateTime ToLocalTime(DateTime time)
    {
      if (time.Kind != DateTimeKind.Unspecified)
        time = new DateTime(time.Ticks, DateTimeKind.Unspecified);

      return time.Add(System.TimeZone.CurrentTimeZone.GetUtcOffset(time) - GetUtcOffset(time));
    }

    public override TimeSpan GetUtcOffset(DateTime time)
    {
      int UtcOffset = _TimeZone.Offset;

      if (IsDaylightSavingTime(time))
        UtcOffset += _TimeZone.DltOffset;

      return new TimeSpan(0, -UtcOffset, 0);
    }

    public bool IsLocalTimeZone()
    {
      if (TimeZone.CurrentTimeZone.StandardName == this.StandardName)
        return true;

      return false;
    }

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

    private void LoadRegistryTimeZones()
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
        _TimeZoneList = new Hashtable();
        _TimeZoneNames = new Hashtable();
        string[] timeZoneKeys = RegKeyRoot.GetSubKeyNames();

        for (int i = 0; i < timeZoneKeys.Length; i++)
        {
          using (RegistryKey TZKey = RegKeyRoot.OpenSubKey(timeZoneKeys[i]))
            if (TZKey != null)
            {
              TimeZoneInfo TZInfo = new TimeZoneInfo();

              TZInfo.Index = (int)TZKey.GetValue(VALUE_INDEX);
              TZInfo.Display = (string)TZKey.GetValue(VALUE_DISPLAY_NAME);
              TZInfo.StdName = (string)TZKey.GetValue(VALUE_STANDARD_NAME);
              TZInfo.DltName = (string)TZKey.GetValue(VALUE_DAYLIGHT_NAME);
              byte[] timeZoneData = (byte[])TZKey.GetValue(VALUE_ZONE_INFO);

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
        RegKeyRoot.Close();
      }
    }

    private TimeZoneDate GetDate(byte[] bytes, Int32 index)
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
  }
}
