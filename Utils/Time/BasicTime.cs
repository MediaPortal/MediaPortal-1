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

namespace MediaPortal.Utils.Time
{
  public class BasicTime
  {
    private int _hour = 0;
    private int _minute = 0;

    public BasicTime(int hour, int minute)
    {
      _hour = hour;
      _minute = minute;
    }

    public BasicTime(DateTime time)
    {
      _hour = time.Hour;
      _minute = time.Minute;
    }

    public BasicTime(string time)
    {
      ParseTimeString(time);
    }

    public BasicTime(long time)
    {
      time /= 100L;
      _minute = (int)(time % 100L);
      time /= 100L;
      _hour = (int)(time % 100L); 
    }

    public int Minute
    {
      get { return _minute; }
    }

    public int Hour
    {
      get { return _hour; }
    }

    #region operators
    public static bool operator >(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour > time2.Hour)
        return true;

      if (time1.Hour == time2.Hour && time1.Minute > time2.Minute)
        return true;

      return false;
    }

    public static bool operator <(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour < time2.Hour)
        return true;

      if (time1.Hour == time2.Hour && time1.Minute < time2.Minute)
        return true;

      return false;
    }

    public static bool operator >=(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
          return true;

        if (time1.Minute > time2.Minute)
          return true;
      }

      if (time1.Hour > time2.Hour)
        return true;

      return false;
    }

    public static bool operator <=(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
          return true;

        if (time1.Minute < time2.Minute)
          return true;
      }

      if (time1.Hour < time2.Hour)
        return true;

      return false;
    }

    public static bool operator >(BasicTime time1, DateTime time2)
    {
      if (time1.Hour > time2.Hour)
        return true;

      if (time1.Hour == time2.Hour && time1.Minute > time2.Minute)
        return true;

      return false;
    }

    public static bool operator <(BasicTime time1, DateTime time2)
    {
      if (time1.Hour < time2.Hour)
        return true;

      if (time1.Hour == time2.Hour && time1.Minute < time2.Minute)
        return true;

      return false;
    }

    public static bool operator >=(BasicTime time1, DateTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
          return true;

        if (time1.Minute > time2.Minute)
          return true;
      }

      if (time1.Hour > time2.Hour)
        return true;

      return false;
    }

    public static bool operator <=(BasicTime time1, DateTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
          return true;

        if (time1.Minute < time2.Minute)
          return true;
      }

      if (time1.Hour < time2.Hour)
        return true;

      return false;
    }
    #endregion

    private void ParseTimeString(string strTime)
    {
      strTime = strTime.Replace(" ", "");
      if (strTime == "")
        throw (new ArgumentException("Time String Empty"));

      int sepPos;

      char[] timeSeperators = { ':', '.', 'h' };

      if ((sepPos = strTime.IndexOfAny(timeSeperators)) != -1)
      {
        try
        {
          _hour = int.Parse(strTime.Substring(0, sepPos));
          _minute = int.Parse(strTime.Substring(sepPos + 1, 2));
        }
        catch (Exception)
        {
          throw (new ArgumentException("Invalid Time Argument"));
        }
      }
      else
      {
        throw (new ArgumentException("Invalid Time Argument"));
      }

      if (strTime.ToLower().IndexOf("pm") != -1 && _hour != 0)
      {
        if (_hour != 12)
          _hour += 12;
      }

      if (strTime.ToLower().IndexOf("am") != -1 && _hour == 12)
        _hour = 0;

      if (_hour == 24)
        _hour = 0;
    }
  }
}
