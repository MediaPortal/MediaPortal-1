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
  public class WorldDateTime : IComparable
  {
    /// <summary>
    /// Date Time with WorldTimeZone
    /// </summary>

    #region Variables

    //private DateTime _dt;
    private int _year;
    private int _month;
    private int _day;
    private int _hour;
    private int _minute;
    private int _second;
    private WorldTimeZone _timeZone;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldDateTime"/> class.
    /// </summary>
    public WorldDateTime()
    {
      _year = 0;
      _month = 0;
      _day = 0;
      _hour = 0;
      _minute = 0;
      _second = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldDateTime"/> class.
    /// </summary>
    /// <param name="datetime">The datetime.</param>
    public WorldDateTime(DateTime datetime)
    {
      SetFromDateTime(datetime);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldDateTime"/> class.
    /// </summary>
    /// <param name="datetime">The datetime.</param>
    public WorldDateTime(DateTime datetime, WorldTimeZone timeZone)
    {
      SetFromDateTime(datetime);
      _timeZone = timeZone;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldDateTime"/> class.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>
    /// <param name="hour">The hour.</param>
    /// <param name="minute">The minute.</param>
    public WorldDateTime(int year, int month, int day, int hour, int minute)
    {
      _year = year;
      _month = month;
      _day = day;
      _hour = hour;
      _minute = minute;
    }

    /// <summary>
    /// Creates a WorldDateTime object base on date/time in xmltv format (yyyymmddhhmmss)
    /// </summary>
    /// <param name="ldatetime">date/time in xmltv format (yyyymmddhhmmss) or (yyyymmddhhmm)</param>
    public WorldDateTime(long ldatetime)
    {
      SetFromLong(ldatetime);

      // Test that DateTime is vaild
      DateTime dt = new DateTime(_year, _month, _day, _hour, _minute, _second, 0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldDateTime"/> class.
    /// </summary>
    /// <param name="dateTimeGmt">The date time GMT.</param>
    /// <param name="useOffset">if set to <c>true</c> [use offset].</param>
    public WorldDateTime(string dateTimeGmt, bool useOffset)
    {
      dateTimeGmt = dateTimeGmt.Trim();
      string longTime;


      int timeEndPos = dateTimeGmt.IndexOf(' ');
      if (timeEndPos == -1)
      {
        longTime = dateTimeGmt;
        useOffset = false;
      }
      else
      {
        if (timeEndPos != 12 && timeEndPos != 14)
        {
          throw new ArgumentOutOfRangeException();
        }
        longTime = dateTimeGmt.Substring(0, timeEndPos);
      }

      try
      {
        long ldatetime = Int64.Parse(longTime);
        SetFromLong(ldatetime);
      }
      catch (Exception)
      {
        throw new ArgumentOutOfRangeException();
      }

      if (useOffset)
      {
        DateTime dt = this.DateTime;
        TimeSpan ts = GetTimeOffset(dateTimeGmt.Substring(timeEndPos, dateTimeGmt.Length - timeEndPos));
        SetFromDateTime(dt.Add(ts));
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldDateTime"/> class.
    /// </summary>
    /// <param name="dateTimeGmt">The date time GMT.</param>
    public WorldDateTime(string dateTimeGmt)
      : this(dateTimeGmt, true) {}

    #endregion

    #region Properties

    /// <summary>
    /// Gets the date time.
    /// </summary>
    /// <value>The date time.</value>
    public DateTime DateTime
    {
      get { return new DateTime(_year, _month, _day, _hour, _minute, _second); }
    }

    /// <summary>
    /// Gets or sets the year.
    /// </summary>
    /// <value>The year.</value>
    public int Year
    {
      set { _year = value; }
      get { return _year; }
    }

    /// <summary>
    /// Gets or sets the month.
    /// </summary>
    /// <value>The month.</value>
    public int Month
    {
      set { _month = value; }
      get { return _month; }
    }

    /// <summary>
    /// Gets or sets the day.
    /// </summary>
    /// <value>The day.</value>
    public int Day
    {
      set { _day = value; }
      get { return _day; }
    }

    /// <summary>
    /// Gets or sets the hour.
    /// </summary>
    /// <value>The hour.</value>
    public int Hour
    {
      set { _hour = value; }
      get { return _hour; }
    }

    /// <summary>
    /// Gets or sets the minute.
    /// </summary>
    /// <value>The minute.</value>
    public int Minute
    {
      set { _minute = value; }
      get { return _minute; }
    }

    /// <summary>
    /// Gets or sets the second.
    /// </summary>
    /// <value>The second.</value>
    public int Second
    {
      set { _second = value; }
      get { return _second; }
    }

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    /// <value>The time zone.</value>
    public WorldTimeZone TimeZone
    {
      set { _timeZone = value; }
      get { return _timeZone; }
    }

    //public DayOfWeek DayOfWeek
    //{
    //  get { return _dt.DayOfWeek; }
    //}

    #endregion

    #region System.DateTime Methods

    //public TimeSpan Subtract(DateTime value)
    //{
    //  return _dt.Subtract(value);
    //}

    //public WorldDateTime Subtract(TimeSpan value)
    //{
    //  return new WorldDateTime(_dt.Subtract(value));
    //}

    //public WorldDateTime Add(TimeSpan value)
    //{
    //  return new WorldDateTime(_dt.Add(value));
    //}

    /// <summary>
    /// Adds the days.
    /// </summary>
    /// <param name="days">The days.</param>
    /// <returns></returns>
    public WorldDateTime AddDays(double days)
    {
      DateTime dt = this.DateTime;
      return new WorldDateTime(dt.AddDays(days), _timeZone);
    }

    //public WorldDateTime AddHours(double hours)
    //{
    //  return new WorldDateTime(_dt.AddHours(hours));
    //}

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns a date/time in xmltv format (yyyymmddhhmmss)
    /// </summary>
    /// <returns>long in xmltv format (yyyymmddhhmmss)</returns>
    public long ToLongDateTime()
    {
      long lDatetime;

      lDatetime = _year;
      lDatetime *= 100;
      lDatetime += _month;
      lDatetime *= 100;
      lDatetime += _day;
      lDatetime *= 100;
      lDatetime += _hour;
      lDatetime *= 100;
      lDatetime += _minute;
      lDatetime *= 100;
      lDatetime += _second;

      return lDatetime;
    }

    /// <summary>
    /// Returns a date/time in xmltv format (yyyymmddhhmmss)
    /// </summary>
    /// <returns>long in xmltv format (yyyymmddhhmmss)</returns>
    public long ToLocalLongDateTime()
    {
      DateTime dt = ToLocalTime();

      long lDatetime;

      lDatetime = dt.Year;
      lDatetime *= 100;
      lDatetime += dt.Month;
      lDatetime *= 100;
      lDatetime += dt.Day;
      lDatetime *= 100;
      lDatetime += dt.Hour;
      lDatetime *= 100;
      lDatetime += dt.Minute;
      lDatetime *= 100;
      lDatetime += dt.Second;

      return lDatetime;
    }

    public long DaysSince(DateTime value)
    {
      TimeSpan ts = this.DateTime.Subtract(value);

      return (long)ts.TotalDays;
    }

    public long SecondsSince(DateTime value)
    {
      TimeSpan ts = this.DateTime.Subtract(value);

      return (long)ts.TotalSeconds;
    }

    public long ToEpochTime()
    {
      DateTime dt = this.DateTime;
      if (_timeZone != null)
        dt = _timeZone.ToUniversalTime(dt);
      DateTime dtEpochStartTime = Convert.ToDateTime("1/1/1970 12:00:00 AM");
      TimeSpan ts = dt.Subtract(dtEpochStartTime);

      //long epochtime = ((((((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes) * 60) + ts.Seconds);

      return (long)ts.TotalSeconds;
    }

    public long ToEpochDate()
    {
      DateTime dt = this.DateTime;
      if (_timeZone != null)
        dt = _timeZone.ToUniversalTime(dt);
      DateTime dtEpochStartTime = Convert.ToDateTime("1/1/1970 12:00:00 AM");
      TimeSpan ts = dt.Subtract(dtEpochStartTime);

      return ts.Days;
    }

    public DateTime ToLocalTime()
    {
      DateTime dt = this.DateTime;
      if (_timeZone != null)
      {
        return _timeZone.ToLocalTime(dt);
      }

      return dt;
    }

    public DateTime ToUniveralTime()
    {
      DateTime dt = this.DateTime;
      if (_timeZone != null)
      {
        return _timeZone.ToUniversalTime(dt);
      }

      return dt;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Sets from date time.
    /// </summary>
    /// <param name="datetime">The datetime.</param>
    private void SetFromDateTime(DateTime datetime)
    {
      _year = datetime.Year;
      _month = datetime.Month;
      _day = datetime.Day;
      _hour = datetime.Hour;
      _minute = datetime.Minute;
      _second = datetime.Second;
    }

    private void SetFromLong(long ldatetime)
    {
      if (ldatetime <= 0)
      {
        SetFromDateTime(DateTime.MinValue);
        return;
      }

      _second = 0;
      if (ldatetime > 10000000000000)
      {
        _second = (int)(ldatetime % 100L);
        ldatetime /= 100L;
        if (_second < 0 || _second > 59)
        {
          throw new ArgumentOutOfRangeException();
        }
      }

      _minute = (int)(ldatetime % 100L);
      ldatetime /= 100L;
      _hour = (int)(ldatetime % 100L);
      ldatetime /= 100L;
      _day = (int)(ldatetime % 100L);
      ldatetime /= 100L;
      _month = (int)(ldatetime % 100L);
      ldatetime /= 100L;
      _year = (int)ldatetime;

      if (_day < 0 || _day > 31)
      {
        throw new ArgumentOutOfRangeException();
      }
      if (_month < 0 || _month > 12)
      {
        throw new ArgumentOutOfRangeException();
      }
      if (_year < 1900 || _year > 2100)
      {
        throw new ArgumentOutOfRangeException();
      }
      if (_minute < 0 || _minute > 59)
      {
        throw new ArgumentOutOfRangeException();
      }
      if (_hour < 0 || _hour > 23)
      {
        if (_hour == 24)
        {
          _hour = 0;
        }
        else
        {
          throw new ArgumentOutOfRangeException();
        }
      }
    }

    private TimeSpan GetTimeOffset(string strTimeZone)
    {
      TimeSpan ts = new TimeSpan(0, 0, 0);

      // timezone can be in format:
      // GMT +0100 or GMT -0500
      // or just +0300
      if (strTimeZone.Length == 0)
      {
        return ts;
      }
      strTimeZone = strTimeZone.ToLower();

      // just ignore GMT offsets, since we're calculating everything from GMT anyway
      if (strTimeZone.IndexOf("gmt") >= 0)
      {
        int ipos = strTimeZone.IndexOf("gmt");
        strTimeZone = strTimeZone.Substring(ipos + "GMT".Length);
      }

      strTimeZone = strTimeZone.Trim();
      if (strTimeZone[0] == '+' || strTimeZone[0] == '-')
      {
        string strOff = strTimeZone.Substring(1);
        try
        {
          int iOff = Int32.Parse(strOff);
          int mintue = (iOff % 100);
          iOff /= 100;
          int hour = (iOff % 100);
          TimeSpan tsOff = new TimeSpan(hour, mintue, 0);
          if (strTimeZone[0] == '-')
          {
            ts = ts.Subtract(tsOff);
          }
          else
          {
            ts = tsOff;
          }
        }
        catch (Exception) {}
      }
      return ts;
    }

    #endregion

    #region Operators

    /// <summary>
    /// Operator &gt;=s the specified LHS.
    /// </summary>
    /// <param name="lhs">The LHS.</param>
    /// <param name="rhs">The RHS.</param>
    /// <returns></returns>
    public static bool operator >=(WorldDateTime lhs, WorldDateTime rhs)
    {
      return lhs.DateTime >= rhs.DateTime;
    }

    public static bool operator <=(WorldDateTime lhs, WorldDateTime rhs)
    {
      return lhs.DateTime <= rhs.DateTime;
    }

    /// <summary>
    /// Operator &gt;s the specified LHS.
    /// </summary>
    /// <param name="lhs">The LHS.</param>
    /// <param name="rhs">The RHS.</param>
    /// <returns></returns>
    public static bool operator >(WorldDateTime lhs, WorldDateTime rhs)
    {
      return lhs.DateTime > rhs.DateTime;
    }

    public static bool operator <(WorldDateTime lhs, WorldDateTime rhs)
    {
      return lhs.DateTime < rhs.DateTime;
    }

    #endregion

    #region IComparable Members

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than obj. Zero This instance is equal to obj. Greater than zero This instance is greater than obj.
    /// </returns>
    /// <exception cref="T:System.ArgumentException">obj is not the same type as this instance. </exception>
    public int CompareTo(object obj)
    {
      WorldDateTime compareObj = (WorldDateTime)obj;
      return (DateTime.CompareTo(compareObj.DateTime));
    }

    #endregion
  }
}