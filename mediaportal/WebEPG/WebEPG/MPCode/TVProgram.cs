using System;
using MediaPortal.GUI.Library;
using System.Diagnostics;
using System.Text;

namespace MediaPortal.TV.Database
{
  /// <summary>
  /// Class which holds all details about a TV program
  /// </summary>
  public class TVProgram //: IComparable
	{
		string m_strChannel=String.Empty;
    string m_strGenre=String.Empty;
    string m_strTitle=String.Empty;
    string m_strEpisode=String.Empty;
    string m_strDescription=String.Empty;
    string m_strRepeat=String.Empty;
    string m_strDate=String.Empty;
    string m_strSeriesNum=String.Empty;
    string m_strEpisodeNum=String.Empty;
    string m_strEpisodePart=String.Empty;
    string m_strEpisodeFullDetails=String.Empty;
    string m_strStarRating=String.Empty;
    string m_strClassification=String.Empty;
    long   m_iStartTime=0;
    long   m_iEndTime=0;
    int    m_iID=0;
    string m_strDuration=String.Empty;
    string m_strTimeFromNow=String.Empty;
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
      prog.ID=m_iID;
			prog.m_strChannel=m_strChannel;
      prog.m_strGenre=m_strGenre;
      prog.m_strTitle=m_strTitle;
	  prog.m_strEpisode=m_strEpisode;
      prog.m_strDescription=m_strDescription;
	  prog.m_strRepeat=m_strRepeat;
      prog.m_iStartTime=m_iStartTime;
      prog.m_iEndTime=m_iEndTime;
      prog.m_strDate=m_strDate;
      prog.m_strSeriesNum=m_strSeriesNum;
      prog.m_strEpisodeNum=m_strEpisodeNum;
      prog.m_strEpisodePart=m_strEpisodePart;
      prog.m_strStarRating=m_strStarRating;
      prog.m_strClassification=m_strClassification;
      prog.m_strDuration=m_strDuration;
      prog.m_strTimeFromNow=m_strTimeFromNow;
      prog.m_strEpisodeFullDetails=m_strEpisodeFullDetails;
      return prog;
    }

    /// <summary>
    /// Converts a date/time in xmltv format (yyyymmddhhmmss) to a DateTime object
    /// </summary>
    /// <param name="ldate">date/time</param>
    /// <returns>DateTime object containing the date/time</returns>
    DateTime longtodate(long ldate)
    {
      if (ldate<=0) return DateTime.MinValue;
      int year,month,day,hour,minute,sec;
      sec=(int)(ldate%100L); ldate /=100L;
      minute=(int)(ldate%100L); ldate /=100L;
      hour=(int)(ldate%100L); ldate /=100L;
      day=(int)(ldate%100L); ldate /=100L;
      month=(int)(ldate%100L); ldate /=100L;
      year=(int)ldate;
      if (day < 0 || day > 31) return DateTime.MinValue;
      if (month < 0 || month > 12) return DateTime.MinValue;
      if (year < 1900 || year > 2100) return DateTime.MinValue;
      if (sec<0 || sec>59) return DateTime.MinValue;
      if (minute<0 || minute>59) return DateTime.MinValue;
      if (hour<0 || hour>23) return DateTime.MinValue;
      try
      {
        DateTime dt=new DateTime(year,month,day,hour,minute,0,0);
        return dt;
      }
      catch(Exception)
      {
      }
      return DateTime.MinValue;
    }

    /// <summary>
    /// Property to get/set the name of this tv program
    /// </summary>
    public string Channel
    {
      get { return m_strChannel;}
      set { m_strChannel=value;}
    }

    /// <summary>
    /// Property to get/set the genre of this tv program
    /// </summary>
    public string Genre
    {
      get { return m_strGenre;}
      set { m_strGenre=value;}
    }


    /// <summary>
    /// Property to get/set the databse ID of this tv program
    /// </summary>
    public int ID
    {
      get { return m_iID;}
      set { m_iID=value;}
    }

    /// <summary>
    /// Property to get/set the title of this tv program
    /// </summary>
    public string Title
    {
      get { return m_strTitle;}
      set { m_strTitle=value;}
    }

    /// <summary>
    /// Property to get/set the description of this tv program
    /// </summary>
    public string Description
    {
      get { return m_strDescription;}
      set { m_strDescription=value;}
    }
    /// <summary>
    /// Property to get/set the episode name of this tv program
    /// </summary>
	public string Episode
	{
	  get { return m_strEpisode;}
	  set { m_strEpisode=value;}
    }
    /// <summary>
    /// Property to get/set whether this tv program is a repeat
    /// </summary>	
	public string Repeat
	{
	  get { return m_strRepeat;}
	  set { m_strRepeat=value;}
	}
    /// <summary>
    /// Property to get/set the series number of this tv program
    /// </summary>
  public string SeriesNum
  {
    get { return m_strSeriesNum;}
    set { m_strSeriesNum=value;}
  }
    /// <summary>
    /// Property to get/set the episode number of this tv program
    /// </summary>
  public string EpisodeNum
  {
    get { return m_strEpisodeNum;}
    set { m_strEpisodeNum=value;}
  }
    /// <summary>
    /// Property to get/set the episode part of this tv program eg: part 1 of 2
    /// </summary>
  public string EpisodePart
  {
    get { return m_strEpisodePart;}
    set { m_strEpisodePart=value;}
  }  
    /// <summary>
    /// Property to get/set the original date of this tv program
    /// </summary>
  public string Date
  {
    get { return m_strDate;}
    set { m_strDate=value;}
  }
  /// <summary>
  /// Property to get/set the star rating of this tv program(film)
  /// </summary>
  public string StarRating
  {
    get { return m_strStarRating;}
    set { m_strStarRating=value;}
  }
  /// <summary>
  /// Property to get/set the classification of this tv program(film eg: PG,18 etc)
  /// </summary>
  public string Classification
  {
    get { return m_strClassification;}
    set { m_strClassification=value;}
  }
  /// <summary>
  /// Property to get the duration of this tv program
  /// </summary>
//  public string Duration
//  {
//    get
//    {
//      GetDuration();
//      return m_strDuration;
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
//      return m_strTimeFromNow;
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
//        return m_strEpisodeFullDetails;
//      }
//    }
    /// Property to get/set the starttime in xmltv format (yyyymmddhhmmss) of this tv program
    /// </summary>
    public long Start
    {
      get { return m_iStartTime;}
      set { m_iStartTime=value;}
    }

    /// <summary>
    /// Property to get/set the endtime in xmltv format (yyyymmddhhmmss) of this tv program
    /// </summary>
    public long End
    {
      get { return m_iEndTime;}
      set { m_iEndTime=value;}
    }

    /// <summary>
    /// Property to get the starttime of this tv program
    /// </summary>
    public DateTime StartTime
    {
      get { return longtodate(m_iStartTime);}
    }

    /// <summary>
    /// Property to get the endtime of this tv program
    /// </summary>
    public DateTime EndTime
    {
      get { return longtodate(m_iEndTime);}
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
//      if (m_strTitle == "No TVGuide data available") return;
//      string space = " ";
//      DateTime progStart = longtodate(m_iStartTime);
//      DateTime progEnd = longtodate(m_iEndTime);
//      TimeSpan progDuration = progEnd.Subtract(progStart);
//      switch (progDuration.Hours)
//      {
//        case 0:
//          m_strDuration = progDuration.Minutes+space+GUILocalizeStrings.Get(3004);
//          break;
//        case 1:
//          if (progDuration.Minutes==1) m_strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3001)+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3003);
//          else if (progDuration.Minutes>1) m_strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3001)+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3004);
//          else m_strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3001);
//          break;
//         default:
//           if (progDuration.Minutes==1) m_strDuration = progDuration.Hours+" Hours"+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3003);
//           else if (progDuration.Minutes>0) m_strDuration = progDuration.Hours+" Hours"+", "+progDuration.Minutes+space+GUILocalizeStrings.Get(3004);
//           else m_strDuration = progDuration.Hours+space+GUILocalizeStrings.Get(3002);
//          break;
//      }
//    }
    /// <summary>
    /// Calculates how long from current time a program starts or started, set the TimeFromNow property
    /// </summary>
//    private void GetStartTimeFromNow() 
//    {
//      if (m_strTitle == "No TVGuide data available") return;
//      string space = " ";
//      string strRemaining=String.Empty;
//      DateTime progStart = longtodate(m_iStartTime);
//      TimeSpan timeRelative = progStart.Subtract(DateTime.Now);
//      if (timeRelative.Days==0)
//      {
//        if (timeRelative.Hours>=0 && timeRelative.Minutes>=0)
//        {
//          switch (timeRelative.Hours)
//          {
//            case 0:
//              if (timeRelative.Minutes==1) m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3003);// starts in 1 minute
//              else if (timeRelative.Minutes>1) m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3004);//starts in x minutes
//              else m_strTimeFromNow=GUILocalizeStrings.Get(3013);
//              break;
//            case 1:
//              if (timeRelative.Minutes==1) m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3003);//starts in 1 hour, 1 minute
//              else if (timeRelative.Minutes>1) m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3004);//starts in 1 hour, x minutes
//              else m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+GUILocalizeStrings.Get(3001);//starts in 1 hour
//              break;
//            default:
//              if (timeRelative.Minutes==1) m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3002)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3003);//starts in x hours, 1 minute
//              else if (timeRelative.Minutes>1) m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3002)+", "+timeRelative.Minutes+space+GUILocalizeStrings.Get(3004);//starts in x hours, x minutes
//              else m_strTimeFromNow=GUILocalizeStrings.Get(3009)+" "+timeRelative.Hours+space+GUILocalizeStrings.Get(3002);//starts in x hours
//              break;
//          }
//        }
//        else //already started
//        {
//          DateTime progEnd = longtodate(m_iEndTime);
//          TimeSpan tsRemaining = DateTime.Now.Subtract(progEnd);
//          if (tsRemaining.Minutes>0)
//          {
//            m_strTimeFromNow=GUILocalizeStrings.Get(3016);
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
//              if (timeRelative.Minutes==-1) m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3007)+space+strRemaining;//Started 1 Minute ago
//              else if (timeRelative.Minutes<-1) m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+space+strRemaining;//Started x Minutes ago
//              else m_strTimeFromNow=GUILocalizeStrings.Get(3013);//Starting Now
//              break;
//            case -1:
//              if (timeRelative.Minutes==-1) m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3007)+" "+strRemaining;//Started 1 Hour,1 Minute ago
//              else if (timeRelative.Minutes<-1) m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3001)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+" "+strRemaining;//Started 1 Hour,x Minutes ago
//              else m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3005)+space+strRemaining;//Started 1 Hour ago
//              break;
//            default:
//              if (timeRelative.Minutes==-1) m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3006)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+" "+strRemaining;//Started x Hours,1 Minute ago
//              else if (timeRelative.Minutes<-1) m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3006)+", "+ -timeRelative.Minutes+space+GUILocalizeStrings.Get(3008)+" "+strRemaining;//Started x Hours,x Minutes ago
//              else m_strTimeFromNow=GUILocalizeStrings.Get(3017)+ -timeRelative.Hours+space+GUILocalizeStrings.Get(3006)+space+strRemaining;//Started x Hours ago
//              break;
//          }      
//        }
//      }
//      else
//      {
//        if (timeRelative.Days==1) m_strTimeFromNow=GUILocalizeStrings.Get(3009)+space+timeRelative.Days+space+GUILocalizeStrings.Get(3014);//Starts in 1 Day
//        else m_strTimeFromNow=GUILocalizeStrings.Get(3009)+space+timeRelative.Days+space+GUILocalizeStrings.Get(3015);//Starts in x Days
//      }
//
//    }

//    private void GetEpisodeDetail()
//    {
//      string space = " ";
//      StringBuilder epDetail = new StringBuilder();
//      if ((m_strEpisode != "-")&(m_strEpisode !=String.Empty)) epDetail.Append(m_strEpisode);
//      if ((m_strSeriesNum !="-")&(m_strSeriesNum !=String.Empty))
//      {
//        epDetail.Append(space+"()");
//        epDetail.Insert(epDetail.Length-1,GUILocalizeStrings.Get(3019)+space+m_strSeriesNum);
//      }
//      if ((m_strEpisodeNum !="-")&(m_strEpisodeNum !=String.Empty)) epDetail.Insert(epDetail.Length-1,space+GUILocalizeStrings.Get(3020)+space+m_strEpisodeNum);
//      if ((m_strEpisodePart !="-")&(m_strEpisodePart !=String.Empty)) epDetail.Insert(epDetail.Length-1,space+GUILocalizeStrings.Get(3021)+space+m_strEpisodePart.Substring(0,1)+space+GUILocalizeStrings.Get(3022)+space+m_strEpisodePart.Substring(2,1));
//      m_strEpisodeFullDetails = epDetail.ToString();
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
