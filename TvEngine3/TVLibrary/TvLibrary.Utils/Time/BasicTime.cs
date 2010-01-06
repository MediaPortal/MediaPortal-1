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

namespace MediaPortal.Utils.Time
{
  /// <summary>
  /// Basic time class has only hours and mintues.
  /// </summary>
  public class BasicTime
  {
    #region Variables

    private int _hour = 0;
    private int _minute = 0;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicTime"/> class.
    /// </summary>
    /// <param name="hour">The hour.</param>
    /// <param name="minute">The minute.</param>
    public BasicTime(int hour, int minute)
    {
      _hour = hour;
      _minute = minute;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicTime"/> class.
    /// </summary>
    /// <param name="time">The time as a DateTime</param>
    public BasicTime(DateTime time)
    {
      _hour = time.Hour;
      _minute = time.Minute;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicTime"/> class.
    /// </summary>
    /// <param name="time">The time as a string</param>
    public BasicTime(string time)
    {
      ParseTimeString(time);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicTime"/> class.
    /// </summary>
    /// <param name="time">The time as a long</param>
    public BasicTime(long time)
    {
      time /= 100L;
      _minute = (int)(time % 100L);
      time /= 100L;
      _hour = (int)(time % 100L);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the minute.
    /// </summary>
    /// <value>The minute.</value>
    public int Minute
    {
      get { return _minute; }
    }

    /// <summary>
    /// Gets the hour.
    /// </summary>
    /// <value>The hour.</value>
    public int Hour
    {
      get { return _hour; }
    }

    #endregion

    #region operators

    /// <summary>
    /// Operator &gt;s the specified time1.
    /// </summary>
    /// <param name="time1">The time1.</param>
    /// <param name="time2">The time2.</param>
    /// <returns></returns>
    public static bool operator >(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour > time2.Hour)
      {
        return true;
      }

      if (time1.Hour == time2.Hour && time1.Minute > time2.Minute)
      {
        return true;
      }

      return false;
    }

    public static bool operator <(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour < time2.Hour)
      {
        return true;
      }

      if (time1.Hour == time2.Hour && time1.Minute < time2.Minute)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Operator &gt;=s the specified time1.
    /// </summary>
    /// <param name="time1">The time1.</param>
    /// <param name="time2">The time2.</param>
    /// <returns></returns>
    public static bool operator >=(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
        {
          return true;
        }

        if (time1.Minute > time2.Minute)
        {
          return true;
        }
      }

      if (time1.Hour > time2.Hour)
      {
        return true;
      }

      return false;
    }

    public static bool operator <=(BasicTime time1, BasicTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
        {
          return true;
        }

        if (time1.Minute < time2.Minute)
        {
          return true;
        }
      }

      if (time1.Hour < time2.Hour)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Operator &gt;s the specified time1.
    /// </summary>
    /// <param name="time1">The time1.</param>
    /// <param name="time2">The time2.</param>
    /// <returns></returns>
    public static bool operator >(BasicTime time1, DateTime time2)
    {
      if (time1.Hour > time2.Hour)
      {
        return true;
      }

      if (time1.Hour == time2.Hour && time1.Minute > time2.Minute)
      {
        return true;
      }

      return false;
    }

    public static bool operator <(BasicTime time1, DateTime time2)
    {
      if (time1.Hour < time2.Hour)
      {
        return true;
      }

      if (time1.Hour == time2.Hour && time1.Minute < time2.Minute)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Operator &gt;=s the specified time1.
    /// </summary>
    /// <param name="time1">The time1.</param>
    /// <param name="time2">The time2.</param>
    /// <returns></returns>
    public static bool operator >=(BasicTime time1, DateTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
        {
          return true;
        }

        if (time1.Minute > time2.Minute)
        {
          return true;
        }
      }

      if (time1.Hour > time2.Hour)
      {
        return true;
      }

      return false;
    }

    public static bool operator <=(BasicTime time1, DateTime time2)
    {
      if (time1.Hour == time2.Hour)
      {
        if (time1.Minute == time2.Minute)
        {
          return true;
        }

        if (time1.Minute < time2.Minute)
        {
          return true;
        }
      }

      if (time1.Hour < time2.Hour)
      {
        return true;
      }

      return false;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Parses a time string.
    /// </summary>
    /// <param name="strTime">The time as a string</param>
    private void ParseTimeString(string strTime)
    {
      strTime = strTime.Replace(" ", "");
      if (strTime == "")
      {
        throw (new ArgumentException("Time String Empty"));
      }

      int sepPos;

      char[] timeSeperators = {':', '.', 'h', 'H'};

      if ((sepPos = strTime.IndexOfAny(timeSeperators)) != -1)
      {
        try
        {
          int start = sepPos - 2;
          if (start < 0)
          {
            start = 0;
          }
          _hour = int.Parse(strTime.Substring(start, sepPos - start));
          _minute = int.Parse(strTime.Substring(sepPos + 1, 2));
        }
        catch (Exception)
        {
          throw (new ArgumentException("Invalid Time Argument"));
        }
      }
      else
      {
        // no seperator. Only numbers 0630 ?
        int time;
        try
        {
          time = int.Parse(strTime);
        }
        catch (Exception)
        {
          throw (new ArgumentException("Invalid Time Argument"));
        }

        if (time > 0 && time < 2400)
        {
          _minute = time % 100;
          _hour = time / 100;
        }
      }

      if (strTime.ToLower().IndexOf("pm") != -1 && _hour != 0)
      {
        if (_hour != 12)
        {
          _hour += 12;
        }
      }

      if (strTime.ToLower().IndexOf("am") != -1 && _hour == 12)
      {
        _hour = 0;
      }

      if (_hour == 24)
      {
        _hour = 0;
      }
    }

    #endregion

    public override string ToString()
    {
      return _hour + ":" + _minute;
    }
  }
}