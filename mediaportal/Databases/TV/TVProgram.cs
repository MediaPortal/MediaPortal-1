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
using System.Text;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Database
{
  /// <summary>
  /// Class which holds all details about a TV program
  /// </summary>
  [Serializable()]
  public class TVProgram : IComparable, IComparable<TVProgram>
  {
    #region Variables

    private string _channelName = string.Empty;
    private string _genre = string.Empty;
    private string _title = string.Empty;
    private string _epsiode = string.Empty;
    private string _description = string.Empty;
    private string _repeat = string.Empty;
    private string _date = string.Empty;
    private string _serieNumber = string.Empty;
    private string _epsiodeNum = string.Empty;
    private string _epsiodePart = string.Empty;
    private string _epsiodeFullDetails = string.Empty;
    private string _starRating = string.Empty;
    private string _classification = string.Empty;
    private long _startTime = 0;
    private long _endTime = 0;
    private int _programId = 0;
    private string _duration = string.Empty;
    private string _timeFromNow = string.Empty;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Constructor
    /// </summary>
    public TVProgram()
    {
    }

    public TVProgram(string channelName, DateTime start, DateTime end, string title)
    {
      _channelName = channelName;
      _startTime = Util.Utils.datetolong(start);
      _endTime = Util.Utils.datetolong(end);
      _title = title;
    }

    public TVProgram(string channelName, DateTime start, DateTime end, string title, string description)
    {
      _channelName = channelName;
      _startTime = Util.Utils.datetolong(start);
      _endTime = Util.Utils.datetolong(end);
      _description = description;
      _title = title;
    }

    public TVProgram(string channelName, DateTime start, DateTime end, string title, string description, string genre)
    {
      _channelName = channelName;
      _startTime = Util.Utils.datetolong(start);
      _endTime = Util.Utils.datetolong(end);
      _description = description;
      _genre = genre;
      _title = title;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Property to get/set the name of this tv program
    /// </summary>
    public string Channel
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// Property to get/set the genre of this tv program
    /// </summary>
    public string Genre
    {
      get { return _genre; }
      set { _genre = value; }
    }


    /// <summary>
    /// Property to get/set the databse ID of this tv program
    /// </summary>
    public int ID
    {
      get { return _programId; }
      set { _programId = value; }
    }

    /// <summary>
    /// Property to get/set the title of this tv program
    /// </summary>
    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }

    /// <summary>
    /// Property to get/set the description of this tv program
    /// </summary>
    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }

    /// <summary>
    /// Property to get/set the episode name of this tv program
    /// </summary>
    public string Episode
    {
      get { return _epsiode; }
      set { _epsiode = value; }
    }

    /// <summary>
    /// Property to get/set whether this tv program is a repeat
    /// </summary>	
    public string Repeat
    {
      get { return _repeat; }
      set { _repeat = value; }
    }

    /// <summary>
    /// Property to get/set the series number of this tv program
    /// </summary>
    public string SeriesNum
    {
      get { return _serieNumber; }
      set { _serieNumber = value; }
    }

    /// <summary>
    /// Property to get/set the episode number of this tv program
    /// </summary>
    public string EpisodeNum
    {
      get { return _epsiodeNum; }
      set { _epsiodeNum = value; }
    }

    /// <summary>
    /// Property to get/set the episode part of this tv program eg: part 1 of 2
    /// </summary>
    public string EpisodePart
    {
      get { return _epsiodePart; }
      set { _epsiodePart = value; }
    }

    /// <summary>
    /// Property to get/set the original date of this tv program
    /// </summary>
    public string Date
    {
      get { return _date; }
      set { _date = value; }
    }

    /// <summary>
    /// Property to get/set the star rating of this tv program(film)
    /// </summary>
    public string StarRating
    {
      get { return _starRating; }
      set { _starRating = value; }
    }

    /// <summary>
    /// Property to get/set the classification of this tv program(film eg: PG,18 etc)
    /// </summary>
    public string Classification
    {
      get { return _classification; }
      set { _classification = value; }
    }

    /// <summary>
    /// Property to get the duration of this tv program
    /// </summary>
    public string Duration
    {
      get
      {
        GetDuration();
        return _duration;
      }
    }

    /// <summary>
    /// <summary>
    /// Property to get the start time relative to current time of this tv program
    /// eg. Starts in 2 Hours 25 Minutes, Started 35 Minutes ago - 25 Minutes remaining
    /// </summary>
    public string TimeFromNow
    {
      get
      {
        GetStartTimeFromNow();
        return _timeFromNow;
      }
    }

    /// <summary>
    /// <summary>
    /// Property to get the full episode details of a tv program
    /// eg. The One with the Fake Party (Series 4 Episode 16 Part 1 of 2)
    /// </summary>
    public string EpisodeDetails
    {
      get
      {
        GetEpisodeDetail();
        return _epsiodeFullDetails;
      }
    }

    /// <summary>
    /// Property to get/set the starttime in xmltv format (yyyymmddhhmmss) of this tv program
    /// </summary>
    public long Start
    {
      get { return _startTime; }
      set { _startTime = value; }
    }

    /// <summary>
    /// Property to get/set the endtime in xmltv format (yyyymmddhhmmss) of this tv program
    /// </summary>
    public long End
    {
      get { return _endTime; }
      set { _endTime = value; }
    }

    /// <summary>
    /// Property to get the starttime of this tv program
    /// </summary>
    public DateTime StartTime
    {
      get { return longtodate(_startTime); }
    }

    /// <summary>
    /// Property to get the endtime of this tv program
    /// </summary>
    public DateTime EndTime
    {
      get { return longtodate(_endTime); }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns a new TVProgram instance which contains the same values
    /// </summary>
    /// <returns>new TVProgram</returns>
    public TVProgram Clone()
    {
      TVProgram prog = new TVProgram();
      prog.ID = _programId;
      prog._channelName = _channelName;
      prog._genre = _genre;
      prog._title = _title;
      prog._epsiode = _epsiode;
      prog._description = _description;
      prog._repeat = _repeat;
      prog._startTime = _startTime;
      prog._endTime = _endTime;
      prog._date = _date;
      prog._serieNumber = _serieNumber;
      prog._epsiodeNum = _epsiodeNum;
      prog._epsiodePart = _epsiodePart;
      prog._starRating = _starRating;
      prog._classification = _classification;
      prog._duration = _duration;
      prog._timeFromNow = _timeFromNow;
      prog._epsiodeFullDetails = _epsiodeFullDetails;
      return prog;
    }

    /// <summary>
    /// Checks if the program is running between the specified start and end time/dates
    /// </summary>
    /// <param name="tStartTime">Start date and time</param>
    /// <param name="tEndTime">End date and time</param>
    /// <returns>true if program is running between tStartTime-tEndTime</returns>
    public bool RunningAt(DateTime tStartTime, DateTime tEndTime)
    {
      DateTime dtStart = StartTime;
      DateTime dtEnd = EndTime;

      bool bRunningAt = false;
      if (dtEnd >= tStartTime && dtEnd <= tEndTime)
      {
        bRunningAt = true;
      }
      if (dtStart >= tStartTime && dtStart <= tEndTime)
      {
        bRunningAt = true;
      }
      if (dtStart <= tStartTime && dtEnd >= tEndTime)
      {
        bRunningAt = true;
      }
      return bRunningAt;
    }

    /// <summary>
    /// Checks if the program is running at the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program is running at tCurTime</returns>
    public bool IsRunningAt(DateTime tCurTime)
    {
      bool bRunningAt = false;
      if (tCurTime >= StartTime && tCurTime <= EndTime)
      {
        bRunningAt = true;
      }
      return bRunningAt;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Converts a date/time in xmltv format (yyyymmddhhmmss) to a DateTime object
    /// </summary>
    /// <param name="ldate">date/time</param>
    /// <returns>DateTime object containing the date/time</returns>
    private DateTime longtodate(long ldate)
    {
      if (ldate <= 0)
      {
        return DateTime.MinValue;
      }
      int year, month, day, hour, minute, sec;
      sec = (int) (ldate%100L);
      ldate /= 100L;
      minute = (int) (ldate%100L);
      ldate /= 100L;
      hour = (int) (ldate%100L);
      ldate /= 100L;
      day = (int) (ldate%100L);
      ldate /= 100L;
      month = (int) (ldate%100L);
      ldate /= 100L;
      year = (int) ldate;
      if (day < 0 || day > 31)
      {
        return DateTime.MinValue;
      }
      if (month < 0 || month > 12)
      {
        return DateTime.MinValue;
      }
      if (year < 1900 || year > 2100)
      {
        return DateTime.MinValue;
      }
      if (sec < 0 || sec > 59)
      {
        return DateTime.MinValue;
      }
      if (minute < 0 || minute > 59)
      {
        return DateTime.MinValue;
      }
      if (hour < 0 || hour > 23)
      {
        return DateTime.MinValue;
      }
      try
      {
        DateTime dt = new DateTime(year, month, day, hour, minute, 0, 0);
        return dt;
      }
      catch (Exception)
      {
      }
      return DateTime.MinValue;
    }

    /// <summary>
    /// Calculates the duration of a program and sets the Duration property
    /// </summary>
    private void GetDuration()
    {
      if (_title == "No TVGuide data available")
      {
        return;
      }
      string space = " ";
      DateTime progStart = longtodate(_startTime);
      DateTime progEnd = longtodate(_endTime);
      TimeSpan progDuration = progEnd.Subtract(progStart);
      switch (progDuration.Hours)
      {
        case 0:
          _duration = progDuration.Minutes + space + GUILocalizeStrings.Get(3004);
          break;
        case 1:
          if (progDuration.Minutes == 1)
          {
            _duration = progDuration.Hours + space + GUILocalizeStrings.Get(3001) + ", " + progDuration.Minutes + space +
                        GUILocalizeStrings.Get(3003);
          }
          else if (progDuration.Minutes > 1)
          {
            _duration = progDuration.Hours + space + GUILocalizeStrings.Get(3001) + ", " + progDuration.Minutes + space +
                        GUILocalizeStrings.Get(3004);
          }
          else
          {
            _duration = progDuration.Hours + space + GUILocalizeStrings.Get(3001);
          }
          break;
        default:
          if (progDuration.Minutes == 1)
          {
            _duration = progDuration.Hours + " Hours" + ", " + progDuration.Minutes + space +
                        GUILocalizeStrings.Get(3003);
          }
          else if (progDuration.Minutes > 0)
          {
            _duration = progDuration.Hours + " Hours" + ", " + progDuration.Minutes + space +
                        GUILocalizeStrings.Get(3004);
          }
          else
          {
            _duration = progDuration.Hours + space + GUILocalizeStrings.Get(3002);
          }
          break;
      }
    }

    /// <summary>
    /// Calculates how long from current time a program starts or started, set the TimeFromNow property
    /// </summary>
    private void GetStartTimeFromNow()
    {
      if (_title == "No TVGuide data available")
      {
        return;
      }
      string space = " ";
      string strRemaining = string.Empty;
      DateTime progStart = longtodate(_startTime);
      TimeSpan timeRelative = progStart.Subtract(DateTime.Now);
      if (timeRelative.Days == 0)
      {
        if (timeRelative.Hours >= 0 && timeRelative.Minutes >= 0)
        {
          switch (timeRelative.Hours)
          {
            case 0:
              if (timeRelative.Minutes == 1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3003); // starts in 1 minute
              }
              else if (timeRelative.Minutes > 1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3004); //starts in x minutes
              }
              else
              {
                _timeFromNow = GUILocalizeStrings.Get(3013);
              }
              break;
            case 1:
              if (timeRelative.Minutes == 1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                               GUILocalizeStrings.Get(3001) + ", " + timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3003); //starts in 1 hour, 1 minute
              }
              else if (timeRelative.Minutes > 1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                               GUILocalizeStrings.Get(3001) + ", " + timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3004); //starts in 1 hour, x minutes
              }
              else
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + GUILocalizeStrings.Get(3001);
                  //starts in 1 hour
              }
              break;
            default:
              if (timeRelative.Minutes == 1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                               GUILocalizeStrings.Get(3002) + ", " + timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3003); //starts in x hours, 1 minute
              }
              else if (timeRelative.Minutes > 1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                               GUILocalizeStrings.Get(3002) + ", " + timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3004); //starts in x hours, x minutes
              }
              else
              {
                _timeFromNow = GUILocalizeStrings.Get(3009) + " " + timeRelative.Hours + space +
                               GUILocalizeStrings.Get(3002); //starts in x hours
              }
              break;
          }
        }
        else //already started
        {
          DateTime progEnd = longtodate(_endTime);
          TimeSpan tsRemaining = DateTime.Now.Subtract(progEnd);
          if (tsRemaining.Minutes > 0)
          {
            _timeFromNow = GUILocalizeStrings.Get(3016);
            return;
          }
          switch (tsRemaining.Hours)
          {
            case 0:
              if (timeRelative.Minutes == 1)
              {
                strRemaining = "(" + -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3018) + ")";
                  //(1 Minute Remaining)
              }
              else
              {
                strRemaining = "(" + -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3010) + ")";
                  //(x Minutes Remaining)
              }
              break;
            case -1:
              if (timeRelative.Minutes == 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3001) + ", " +
                               -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3018) + ")";
                  //(1 Hour,1 Minute Remaining)
              }
              else if (timeRelative.Minutes > 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3001) + ", " +
                               -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3010) + ")";
                  //(1 Hour,x Minutes Remaining)
              }
              else
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3012) + ")";
                  //(1 Hour Remaining)
              }
              break;
            default:
              if (timeRelative.Minutes == 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3002) + ", " +
                               -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3018) + ")";
                  //(x Hours,1 Minute Remaining)
              }
              else if (timeRelative.Minutes > 1)
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3002) + ", " +
                               -tsRemaining.Minutes + space + GUILocalizeStrings.Get(3010) + ")";
                  //(x Hours,x Minutes Remaining)
              }
              else
              {
                strRemaining = "(" + -tsRemaining.Hours + space + GUILocalizeStrings.Get(3012) + ")";
                  //(x Hours Remaining)
              }
              break;
          }
          switch (timeRelative.Hours)
          {
            case 0:
              if (timeRelative.Minutes == -1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3007) + space + strRemaining; //Started 1 Minute ago
              }
              else if (timeRelative.Minutes < -1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Minutes + space +
                               GUILocalizeStrings.Get(3008) + space + strRemaining; //Started x Minutes ago
              }
              else
              {
                _timeFromNow = GUILocalizeStrings.Get(3013); //Starting Now
              }
              break;
            case -1:
              if (timeRelative.Minutes == -1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3001) +
                               ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3007) + " " + strRemaining;
                  //Started 1 Hour,1 Minute ago
              }
              else if (timeRelative.Minutes < -1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3001) +
                               ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3008) + " " + strRemaining;
                  //Started 1 Hour,x Minutes ago
              }
              else
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3005) +
                               space + strRemaining; //Started 1 Hour ago
              }
              break;
            default:
              if (timeRelative.Minutes == -1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3006) +
                               ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3008) + " " + strRemaining;
                  //Started x Hours,1 Minute ago
              }
              else if (timeRelative.Minutes < -1)
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3006) +
                               ", " + -timeRelative.Minutes + space + GUILocalizeStrings.Get(3008) + " " + strRemaining;
                  //Started x Hours,x Minutes ago
              }
              else
              {
                _timeFromNow = GUILocalizeStrings.Get(3017) + -timeRelative.Hours + space + GUILocalizeStrings.Get(3006) +
                               space + strRemaining; //Started x Hours ago
              }
              break;
          }
        }
      }
      else
      {
        if (timeRelative.Days == 1)
        {
          _timeFromNow = GUILocalizeStrings.Get(3009) + space + timeRelative.Days + space + GUILocalizeStrings.Get(3014);
            //Starts in 1 Day
        }
        else
        {
          _timeFromNow = GUILocalizeStrings.Get(3009) + space + timeRelative.Days + space + GUILocalizeStrings.Get(3015);
            //Starts in x Days
        }
      }
    }

    private void GetEpisodeDetail()
    {
      string space = " ";
      StringBuilder epDetail = new StringBuilder();
      if ((_epsiode != "-") & (_epsiode != string.Empty))
      {
        epDetail.Append(_epsiode);
      }
      if ((_serieNumber != "-") & (_serieNumber != string.Empty))
      {
        epDetail.Append(space + "()");
        epDetail.Insert(epDetail.Length - 1, GUILocalizeStrings.Get(3019) + space + _serieNumber);
      }
      if ((_epsiodeNum != "-") & (_epsiodeNum != string.Empty))
      {
        epDetail.Insert(epDetail.Length - 1, space + GUILocalizeStrings.Get(3020) + space + _epsiodeNum);
      }
      if ((_epsiodePart != "-") & (_epsiodePart != string.Empty))
      {
        epDetail.Insert(epDetail.Length - 1,
                        space + GUILocalizeStrings.Get(3021) + space + _epsiodePart.Substring(0, 1) + space +
                        GUILocalizeStrings.Get(3022) + space + _epsiodePart.Substring(2, 1));
      }
      _epsiodeFullDetails = epDetail.ToString();
    }

    #endregion

    #region IComparable Members

    public int CompareTo(object obj)
    {
      TVProgram prog = (TVProgram) obj;
      return (Title.CompareTo(prog.Title));
    }

    public int CompareTo(TVProgram prog)
    {
      return (Title.CompareTo(prog.Title));
    }

    #endregion
  }
}