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

namespace MediaPortal.Time
{
  public class BasicDate
  {
    #region Variables

    private int _day;
    private int _month;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicDate"/> class.
    /// </summary>
    /// <param name="day">The day.</param>
    /// <param name="month">The month.</param>
    public BasicDate(int day, int month)
    {
      _day = day;
      _month = month;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicDate"/> class.
    /// </summary>
    /// <param name="time">The time as a DateTime</param>
    public BasicDate(DateTime time)
    {
      _day = time.Day;
      _month = time.Month;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicDate"/> class.
    /// </summary>
    /// <param name="time">The time as a long</param>
    public BasicDate(long time)
    {
      time /= 100L;
      _month = (int)(time % 100L);
      time /= 100L;
      _day = (int)(time % 100L);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicDate"/> class.
    /// </summary>
    /// <param name="time">The time as a string</param>
    public BasicDate(string time)
    {
      ParseDateString(time);
    }

    #endregion Constructors/Destructors

    #region Properties

    /// <summary>
    /// Gets the day.
    /// </summary>
    /// <value>The day.</value>
    public int Day
    {
      get { return _day; }
    }

    /// <summary>
    /// Gets the month.
    /// </summary>
    /// <value>The month.</value>
    public int Month
    {
      get { return _month; }
    }

    #endregion Properties

    #region operators

    /// <summary>
    /// Operator &gt;s the specified time1.
    /// </summary>
    /// <param name="time1">The time1.</param>
    /// <param name="time2">The time2.</param>
    /// <returns></returns>
    public static bool operator >(BasicDate time1, BasicDate time2)
    {
      if (time1.Month > time2.Month)
      {
        return true;
      }

      if (time1.Month == time2.Month && time1.Day > time2.Day)
      {
        return true;
      }

      return false;
    }

    public static bool operator <(BasicDate time1, BasicDate time2)
    {
      if (time1.Month < time2.Month)
      {
        return true;
      }

      if (time1.Month == time2.Month && time1.Day < time2.Day)
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
    public static bool operator >=(BasicDate time1, BasicDate time2)
    {
      if (time1.Month == time2.Month)
      {
        if (time1.Day == time2.Day)
        {
          return true;
        }

        if (time1.Day > time2.Day)
        {
          return true;
        }
      }

      if (time1.Month > time2.Month)
      {
        return true;
      }

      return false;
    }

    public static bool operator <=(BasicDate time1, BasicDate time2)
    {
      if (time1.Month == time2.Month)
      {
        if (time1.Day == time2.Day)
        {
          return true;
        }

        if (time1.Day < time2.Day)
        {
          return true;
        }
      }

      if (time1.Month < time2.Month)
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
    public static bool operator >(BasicDate time1, DateTime time2)
    {
      if (time1.Month > time2.Month)
      {
        return true;
      }

      if (time1.Month == time2.Month && time1.Day > time2.Day)
      {
        return true;
      }

      return false;
    }

    public static bool operator <(BasicDate time1, DateTime time2)
    {
      if (time1.Month < time2.Month)
      {
        return true;
      }

      if (time1.Month == time2.Month && time1.Day < time2.Day)
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
    public static bool operator >=(BasicDate time1, DateTime time2)
    {
      if (time1.Month == time2.Month)
      {
        if (time1.Day == time2.Day)
        {
          return true;
        }

        if (time1.Day > time2.Day)
        {
          return true;
        }
      }

      if (time1.Month > time2.Month)
      {
        return true;
      }

      return false;
    }

    public static bool operator <=(BasicDate time1, DateTime time2)
    {
      if (time1.Month == time2.Month)
      {
        if (time1.Day == time2.Day)
        {
          return true;
        }

        if (time1.Day < time2.Day)
        {
          return true;
        }
      }

      if (time1.Month < time2.Month)
      {
        return true;
      }

      return false;
    }

    #endregion operators

    #region private methods

    /// <summary>
    /// Parses a date string.
    /// </summary>
    /// <param name="strDate">The date as a string</param>
    private void ParseDateString(string strDate)
    {
      strDate = strDate.Replace(" ", "");
      if (strDate == "")
      {
        throw (new ArgumentException("Date String Empty"));
      }

      int sepPos;

      char[] dateSeperators = {':', '.', 'm', 'M'};

      if ((sepPos = strDate.IndexOfAny(dateSeperators)) != -1)
      {
        try
        {
          int start = sepPos - 2;
          if (start < 0)
          {
            start = 0;
          }
          _month = int.Parse(strDate.Substring(start, sepPos - start));
          _day = int.Parse(strDate.Substring(sepPos + 1, 2));
        }
        catch (Exception ex)
        {
          throw (new ArgumentException("Invalid Time Argument " + ex.Message));
        }
      }
      else
      {
        // no seperator. Only numbers 0124 ?
        int date;
        try
        {
          date = int.Parse(strDate);
        }
        catch (Exception ex)
        {
          throw (new ArgumentException("Invalid Time Argument " + ex.Message));
        }

        if (date > 0100 && date < 1231)
        {
          _month = date % 100;
          _day = date / 100;
        }
      }
    }

    #endregion private methods
  }
}