/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Globalization;
using System.Text;
using Microsoft.Win32;

namespace MediaPortal.Utils
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
        
        private static Hashtable m_TimeZoneList = null;
        private static Hashtable m_TimeZoneNames = null;

        private TimeZoneInfo m_TimeZone;

        private bool m_bHasDlt;

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

            if( m_TimeZoneList == null )
               LoadRegistryTimeZones();

           if (m_TimeZoneNames.Contains(TimeZone))
               TimeZoneName = (String) m_TimeZoneNames[TimeZone];

           if (m_TimeZoneList.Contains(TimeZone))
               TimeZoneName = TimeZone;

            if ( TimeZoneName == string.Empty )
               throw new ArgumentException("TimeZone Not valid"); 

            m_TimeZone = (TimeZoneInfo) m_TimeZoneList[ TimeZoneName ];        

            m_bHasDlt = true;
            if ( m_TimeZone.DltDate.Month == 0)
            {
                m_bHasDlt = false;
            }
            else
            {
                if (m_TimeZone.DltDate.WeekOfMonth < 0 
                    || m_TimeZone.DltDate.WeekOfMonth > 4)
                m_bHasDlt = false;
            }
        }

        public override string DaylightName
        {
            get
            {
               return m_TimeZone.DltName;
            }
        }

        public override string StandardName
        {
            get
            {
               return m_TimeZone.StdName;
            }
        }

        public override DaylightTime GetDaylightChanges(int year)
        {
            DaylightTime DLTime = null;

            if ( m_bHasDlt )
            {
                DateTime StdDay = GetDateTime(m_TimeZone.StdDate, year);
                DateTime DltDay = GetDateTime(m_TimeZone.DltDate, year);

                DLTime = new DaylightTime(DltDay, StdDay, new TimeSpan(0, m_TimeZone.DltOffset, 0));
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

                int MonthEnd = (int) ChangeDay.DayOfWeek;
                int WeekDay = (int) TimeChange.DayOfWeek;

                if (MonthEnd != WeekDay)
                {
                    if ( MonthEnd > WeekDay )
                        MonthOffset = 0 - (MonthEnd - WeekDay);
                    else
                        MonthOffset = 0 - (MonthEnd + 7 - WeekDay);
                }
            }
            else
            {
                ChangeDay = new DateTime(year, TimeChange.Month, 1);
            
                int MonthStart = (int) ChangeDay.DayOfWeek;
                int WeekDay = (int) TimeChange.DayOfWeek;

                if (MonthStart != WeekDay)
                {
                    if ( WeekDay > MonthStart)
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
            int UtcOffset = m_TimeZone.Offset;

            if (IsDaylightSavingTime(time))
                UtcOffset += m_TimeZone.DltOffset;

            return new TimeSpan(0, -UtcOffset, 0);
        }

        public bool IsLocalTimeZone()
        {
            if (TimeZone.CurrentTimeZone.StandardName == this.StandardName)
                return true;

            return false;
        }

        public override bool IsDaylightSavingTime( DateTime time )
        {
            if ( m_TimeZone.DltDate.Month == 0)   // Never Dlt time;
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
                if ((RegKeyRoot = Registry.LocalMachine.OpenSubKey( currentRegKey )) != null)
                    break;
            }

            if(RegKeyRoot != null)
            {
                m_TimeZoneList = new Hashtable();
                m_TimeZoneNames = new Hashtable();
                string[] timeZoneKeys = RegKeyRoot.GetSubKeyNames();

                for(int i=0; i < timeZoneKeys.Length; i++)
                {
                    RegistryKey TZKey = RegKeyRoot.OpenSubKey( timeZoneKeys[i] );
                    if( TZKey != null )
                    {
                        TimeZoneInfo TZInfo = new TimeZoneInfo();

                        TZInfo.Index = (int) TZKey.GetValue(VALUE_INDEX);
                        TZInfo.Display = (string) TZKey.GetValue(VALUE_DISPLAY_NAME);
                        TZInfo.StdName = (string) TZKey.GetValue(VALUE_STANDARD_NAME);
                        TZInfo.DltName = (string) TZKey.GetValue(VALUE_DAYLIGHT_NAME);
                        byte[] timeZoneData = (byte[]) TZKey.GetValue(VALUE_ZONE_INFO);

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

                        m_TimeZoneList.Add(timeZoneKeys[i], TZInfo);

                        if(!m_TimeZoneNames.ContainsKey(TZInfo.StdName))
                            m_TimeZoneNames.Add(TZInfo.StdName, timeZoneKeys[i]);

                    }
                }
            }
        }

        private TimeZoneDate GetDate(byte[] bytes, Int32 index) 
        {
            TimeZoneDate TimeChange = new TimeZoneDate();

            //int Year = BitConverter.ToInt16(bytes, index);
            index += LENGTH_WORD;
            TimeChange.Month = BitConverter.ToInt16(bytes, index);

            index += LENGTH_WORD;
            TimeChange.DayOfWeek = (DayOfWeek) BitConverter.ToInt16(bytes, index);

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

            TimeChange.TimeOfDay = new TimeSpan( Hours, Minutes, Seconds );

            return TimeChange;
        }
    }
}
