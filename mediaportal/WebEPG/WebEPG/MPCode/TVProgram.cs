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
using System.Diagnostics;
using System.Text;
using System.Data.SqlTypes;

namespace MediaPortal.Webepg.TV.Database
{
  /// <summary>
  /// Class which holds all details about a TV program
  /// </summary>
  public class TVProgram //: IComparable
	{
		string _strChannel=string.Empty;
    string _strGenre=string.Empty;
    string _strTitle=string.Empty;
    string _strEpisode=string.Empty;
    string _strDescription=string.Empty;
    string _strRepeat=string.Empty;
    string _strDate=string.Empty;
    string _strSeriesNum=string.Empty;
    string _strEpisodeNum=string.Empty;
    string _strEpisodePart=string.Empty;
    string _strEpisodeFullDetails=string.Empty;
    string _strStarRating=string.Empty;
    string _strClassification=string.Empty;
    long   _iStartTime=0;
    long   _iEndTime=0;
    int    _iID=0;
    string _strDuration=string.Empty;
    string _strTimeFromNow=string.Empty;
    /// <summary>
    /// Constructor
    /// </summary>
    public TVProgram()
		{
		}

    /// <summary>
    /// Returns a new TVProgram instance which contains the same values
    /// </summary>
    /// <returns>new TVProgram</returns>
    public TVProgram Clone()
    {
      TVProgram prog = new TVProgram();
      prog.ID=_iID;
			prog._strChannel=_strChannel;
      prog._strGenre=_strGenre;
      prog._strTitle=_strTitle;
	  prog._strEpisode=_strEpisode;
      prog._strDescription=_strDescription;
	  prog._strRepeat=_strRepeat;
      prog._iStartTime=_iStartTime;
      prog._iEndTime=_iEndTime;
      prog._strDate=_strDate;
      prog._strSeriesNum=_strSeriesNum;
      prog._strEpisodeNum=_strEpisodeNum;
      prog._strEpisodePart=_strEpisodePart;
      prog._strStarRating=_strStarRating;
      prog._strClassification=_strClassification;
      prog._strDuration=_strDuration;
      prog._strTimeFromNow=_strTimeFromNow;
      prog._strEpisodeFullDetails=_strEpisodeFullDetails;
      return prog;
    }

    /// <summary>
    /// Converts a date/time in xmltv format (yyyymmddhhmmss) to a DateTime object
    /// </summary>
    /// <param name="ldate">date/time</param>
    /// <returns>DateTime object containing the date/time</returns>
    DateTime longtodate(long ldate)
    {
      if (ldate <= 0)
        return SqlDateTime.MinValue.Value;
      int year,month,day,hour,minute,sec;
      sec=(int)(ldate%100L); ldate /=100L;
      minute=(int)(ldate%100L); ldate /=100L;
      hour=(int)(ldate%100L); ldate /=100L;
      day=(int)(ldate%100L); ldate /=100L;
      month=(int)(ldate%100L); ldate /=100L;
      year=(int)ldate;
      if (day < 0 || day > 31)
        return SqlDateTime.MinValue.Value;
      if (month < 0 || month > 12)
        return SqlDateTime.MinValue.Value;
      if (year < 1900 || year > 2100)
        return SqlDateTime.MinValue.Value;
      if (sec<0 || sec>59)
        return SqlDateTime.MinValue.Value;
      if (minute<0 || minute>59)
        return SqlDateTime.MinValue.Value;
      if (hour<0 || hour>23)
        return SqlDateTime.MinValue.Value;
      try
      {
        DateTime dt=new DateTime(year,month,day,hour,minute,0,0);
        return dt;
      }
      catch(Exception)
      {
      }
      return SqlDateTime.MinValue.Value;
    }

    /// <summary>
    /// Property to get/set the name of this tv program
    /// </summary>
    public string Channel
    {
      get { return _strChannel;}
      set { _strChannel=value;}
    }

    /// <summary>
    /// Property to get/set the genre of this tv program
    /// </summary>
    public string Genre
    {
      get { return _strGenre;}
      set { _strGenre=value;}
    }


    /// <summary>
    /// Property to get/set the databse ID of this tv program
    /// </summary>
    public int ID
    {
      get { return _iID;}
      set { _iID=value;}
    }

    /// <summary>
    /// Property to get/set the title of this tv program
    /// </summary>
    public string Title
    {
      get { return _strTitle;}
      set { _strTitle=value;}
    }

    /// <summary>
    /// Property to get/set the description of this tv program
    /// </summary>
    public string Description
    {
      get { return _strDescription;}
      set { _strDescription=value;}
    }
    /// <summary>
    /// Property to get/set the episode name of this tv program
    /// </summary>
	public string Episode
	{
	  get { return _strEpisode;}
	  set { _strEpisode=value;}
    }
    /// <summary>
    /// Property to get/set whether this tv program is a repeat
    /// </summary>	
	public string Repeat
	{
	  get { return _strRepeat;}
	  set { _strRepeat=value;}
	}
    /// <summary>
    /// Property to get/set the series number of this tv program
    /// </summary>
  public string SeriesNum
  {
    get { return _strSeriesNum;}
    set { _strSeriesNum=value;}
  }
    /// <summary>
    /// Property to get/set the episode number of this tv program
    /// </summary>
  public string EpisodeNum
  {
    get { return _strEpisodeNum;}
    set { _strEpisodeNum=value;}
  }
    /// <summary>
    /// Property to get/set the episode part of this tv program eg: part 1 of 2
    /// </summary>
  public string EpisodePart
  {
    get { return _strEpisodePart;}
    set { _strEpisodePart=value;}
  }  
    /// <summary>
    /// Property to get/set the original date of this tv program
    /// </summary>
  public string Date
  {
    get { return _strDate;}
    set { _strDate=value;}
  }
  /// <summary>
  /// Property to get/set the star rating of this tv program(film)
  /// </summary>
  public string StarRating
  {
    get { return _strStarRating;}
    set { _strStarRating=value;}
  }
  /// <summary>
  /// Property to get/set the classification of this tv program(film eg: PG,18 etc)
  /// </summary>
  public string Classification
  {
    get { return _strClassification;}
    set { _strClassification=value;}
  }
  /// <summary>
  /// Property to get the duration of this tv program
  /// </summary>
//  public string Duration
//  {
//    get
//    {
//      GetDuration();
//      return _strDuration;
//    }
//  }
  /// <summary>
  /// <summary>
  /// Property to get the start time relative to current time of this tv program
  /// eg. Starts in 2 Hours 25 Minutes, Started 35 Minutes ago - 25 Minutes remaining
  /// </summary>
//  public string TimeFromNow
//  {
//    get
//    {
//      GetStartTimeFromNow();
//      return _strTimeFromNow;
//    }
//  }
    /// <summary>
    /// <summary>
    /// Property to get the full episode details of a tv program
    /// eg. The One with the Fake Party (Series 4 Episode 16 Part 1 of 2)
    /// </summary>
//    public string EpisodeDetails
//    {
//      get
//      {
//        GetEpisodeDetail();
//        return _strEpisodeFullDetails;
//      }
//    }
    /// Property to get/set the starttime in xmltv format (yyyymmddhhmmss) of this tv program
    /// </summary>
    public long Start
    {
      get { return _iStartTime;}
      set { _iStartTime=value;}
    }

    /// <summary>
    /// Property to get/set the endtime in xmltv format (yyyymmddhhmmss) of this tv program
    /// </summary>
    public long End
    {
      get { return _iEndTime;}
      set { _iEndTime=value;}
    }

    /// <summary>
    /// Property to get the starttime of this tv program
    /// </summary>
    public DateTime StartTime
    {
      get { return longtodate(_iStartTime);}
    }

    /// <summary>
    /// Property to get the endtime of this tv program
    /// </summary>
    public DateTime EndTime
    {
      get { return longtodate(_iEndTime);}
    }
    /// <summary>
    /// Checks if the program is running between the specified start and end time/dates
    /// </summary>
    /// <param name="tStartTime">Start date and time</param>
    /// <param name="tEndTime">End date and time</param>
    /// <returns>true if program is running between tStartTime-tEndTime</returns>
    public bool RunningAt(DateTime tStartTime, DateTime tEndTime)
    {
      DateTime dtStart=StartTime;
      DateTime dtEnd=EndTime;

      bool bRunningAt=false;
      if (dtEnd>=tStartTime && dtEnd <= tEndTime) bRunningAt=true;
      if (dtStart >=tStartTime && dtStart <= tEndTime) bRunningAt=true;
      if (dtStart <=tStartTime && dtEnd>=tEndTime) bRunningAt=true;
      return bRunningAt;
    }

    /// <summary>
    /// Checks if the program is running at the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program is running at tCurTime</returns>
    public bool IsRunningAt(DateTime tCurTime)
    {
      bool bRunningAt=false;
      if (tCurTime >=StartTime && tCurTime <= EndTime) bRunningAt=true;
      return bRunningAt;
    }
    /// <summary>
    /// Calculates the duration of a program and sets the Duration property
    /// </summary>
//    private void GetDuration()
//    {
//      if (_strTitle == "No TVGuide data available") return;
//      string space = " ";
//      DateTime progStart = longtodate(_iStartTime);
//      DateTime progEnd = longtodate(_iEndTime);
//      TimeSpan progDuration = progEnd.Subtract(progStart);
//      switch (progDuration.Hours)
//      {
//        case 0:
//          _strDuration = progDuration.Minutes+space+GUILocalizeStrings.Get(3004);
//          break;
//        case 1:
//          if (progDuration.Minutes==1) _strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3001)+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3003);
//          else if (progDuration.Minutes>1) _strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3001)+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3004);
//          else _strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3001);
//          break;
//         default:
//           if (progDuration.Minutes==1) _strDuration = progDuration.Hours+" Hours"+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3003);
//           else if (progDuration.Minutes>0) _strDuration = progDuration.Hours+" Hours"+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3004);
//           else _strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3002);
//          break;
//      }
//    }
    /// <summary>
    /// Calculates how long from current time a program starts or started, set the TimeFromNow property
    /// </summary>
//    private void GetStartTimeFromNow() 
//    {
//      if (_strTitle == "No TVGuide data available") return;
//      string space = " ";
//      string strRemaining=string.Empty;
//      DateTime progStart = longtodate(_iStartTime);
//      TimeSpan timeRelative = progStart.Subtract(DateTime.Now);
//      if (timeRelative.Days==0)
//      {
//        if (timeRelative.Hours>=0 && timeRelative.Minutes>=0)
//        {
//          switch (timeRelative.Hours)
//          {
//            case 0:
//              if (timeRelative.Minutes==1) _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3003);// starts in 1 minute
//              else if (timeRelative.Minutes>1) _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3004);//starts in x minutes
//              else _strTimeFromNow=GUILocalizeStrings.Get(3013);
//              break;
//            case 1:
//              if (timeRelative.Minutes==1) _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3003);//starts in 1 hour, 1 minute
//              else if (timeRelative.Minutes>1) _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3004);//starts in 1 hour, x minutes
//              else _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+GUILocalizeStrings.Get(3001);//starts in 1 hour
//              break;
//            default:
//              if (timeRelative.Minutes==1) _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3002)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3003);//starts in x hours, 1 minute
//              else if (timeRelative.Minutes>1) _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3002)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3004);//starts in x hours, x minutes
//              else _strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3002);//starts in x hours
//              break;
//          }
//        }
//        else //already started
//        {
//          DateTime progEnd = longtodate(_iEndTime);
//          TimeSpan tsRemaining = DateTime.Now.Subtract(progEnd);
//          if (tsRemaining.Minutes>0)
//          {
//            _strTimeFromNow=GUILocalizeStrings.Get(3016);
//            return;
//          }
//          switch (tsRemaining.Hours)
//          {
//            case 0:
//              if (timeRelative.Minutes==1) strRemaining="("+ -tsRemaining.Minutes+space+GUILocalizeStrings.Get(3018)+")";//(1 Minute Remaining)
//              else strRemaining="("+ -tsRemaining.Minutes+space+GUILocalizeStrings.Get(3010)+")";//(x Minutes Remaining)
//              break;
//            case -1:
//              if (timeRelative.Minutes==1) strRemaining="("+ -tsRemaining.Hours+space+GUILocalizeStrings.Get(3001)+", "+ -tsRemaining.Minutes+space+GUILocalizeStrings.Get(3018)+")";//(1 Hour,1 Minute Remaining)
//              else if (timeRelative.Minutes>1) strRemaining="("+ -tsRemaining.Hours+space+GUILocalizeStrings.Get(3001)+", "+ -tsRemaining.Minutes+space+GUILocalizeStrings.Get(3010)+")";//(1 Hour,x Minutes Remaining)
//              else strRemaining="("+ -tsRemaining.Hours+space+GUILocalizeStrings.Get(3012)+")";//(1 Hour Remaining)
//              break;
//            default:
//              if (timeRelative.Minutes==1) strRemaining="("+ -tsRemaining.Hours+space+GUILocalizeStrings.Get(3002)+", "+ -tsRemaining.Minutes+space+GUILocalizeStrings.Get(3018)+")";//(x Hours,1 Minute Remaining)
//              else if (timeRelative.Minutes>1) strRemaining="("+ -tsRemaining.Hours+space+GUILocalizeStrings.Get(3002)+", "+ -tsRemaining.Minutes+space+GUILocalizeStrings.Get(3010)+")";//(x Hours,x Minutes Remaining)
//              else strRemaining="("+ -tsRemaining.Hours+space+GUILocalizeStrings.Get(3012)+")";//(x Hours Remaining)
//              break;
//          }
//          switch (timeRelative.Hours)
//          {
//            case 0:
//              if (timeRelative.Minutes==-1) _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3007)+space+strRemaining;//Started 1 Minute ago
//              else if (timeRelative.Minutes<-1) _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+space+strRemaining;//Started x Minutes ago
//              else _strTimeFromNow=GUILocalizeStrings.Get(3013);//Starting Now
//              break;
//            case -1:
//              if (timeRelative.Minutes==-1) _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3007)+" "+strRemaining;//Started 1 Hour,1 Minute ago
//              else if (timeRelative.Minutes<-1) _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+" "+strRemaining;//Started 1 Hour,x Minutes ago
//              else _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3005)+space+strRemaining;//Started 1 Hour ago
//              break;
//            default:
//              if (timeRelative.Minutes==-1) _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3006)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+" "+strRemaining;//Started x Hours,1 Minute ago
//              else if (timeRelative.Minutes<-1) _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3006)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+" "+strRemaining;//Started x Hours,x Minutes ago
//              else _strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3006)+space+strRemaining;//Started x Hours ago
//              break;
//          }      
//        }
//      }
//      else
//      {
//        if (timeRelative.Days==1) _strTimeFromNow=GUILocalizeStrings.Get(3009)+space+timeRelative.Days+space+GUILocalizeStrings.Get(3014);//Starts in 1 Day
//        else _strTimeFromNow=GUILocalizeStrings.Get(3009)+space+timeRelative.Days+space+GUILocalizeStrings.Get(3015);//Starts in x Days
//      }
//
//    }

//    private void GetEpisodeDetail()
//    {
//      string space = " ";
//      StringBuilder epDetail = new StringBuilder();
//      if ((_strEpisode != "-")&(_strEpisode !=string.Empty)) epDetail.Append(_strEpisode);
//      if ((_strSeriesNum !="-")&(_strSeriesNum !=string.Empty))
//      {
//        epDetail.Append(space+"()");
//        epDetail.Insert(epDetail.Length-1,GUILocalizeStrings.Get(3019)+space+_strSeriesNum);
//      }
//      if ((_strEpisodeNum !="-")&(_strEpisodeNum !=string.Empty)) epDetail.Insert(epDetail.Length-1,space+GUILocalizeStrings.Get(3020)+space+_strEpisodeNum);
//      if ((_strEpisodePart !="-")&(_strEpisodePart !=string.Empty)) epDetail.Insert(epDetail.Length-1,space+GUILocalizeStrings.Get(3021)+space+_strEpisodePart.Substring(0,1)+space+GUILocalizeStrings.Get(3022)+space+_strEpisodePart.Substring(2,1));
//      _strEpisodeFullDetails = epDetail.ToString();
//		}
//		#region IComparable Members
//
//		public int CompareTo(object obj)
//		{
//			TVProgram prog = (TVProgram)obj;
//			return (Title.CompareTo(prog.Title) );
//		}
//
//		#endregion
	}
}
