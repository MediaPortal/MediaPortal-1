using System;

namespace MediaPortal.TV.Database
{
  /// <summary>
  /// Class which holds all details about a TV program
  /// </summary>
  public class TVProgram
	{
		string m_strChannel="";
    string m_strGenre="";
    string m_strTitle="";
    string m_strEpisode="";
    string m_strDescription="";
    string m_strRepeat="";
    long   m_iStartTime=0;
    long   m_iEndTime=0;
    int    m_iID=0;
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
      prog.m_strChannel=m_strChannel;
      prog.m_strGenre=m_strGenre;
      prog.m_strTitle=m_strTitle;
	  prog.m_strEpisode=m_strEpisode;
      prog.m_strDescription=m_strDescription;
	  prog.m_strRepeat=m_strRepeat;
      prog.m_iStartTime=m_iStartTime;
      prog.m_iEndTime=m_iEndTime;
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
	public string Repeat
	{
	  get { return m_strRepeat;}
	  set { m_strRepeat=value;}
	}
    /// <summary>
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
	}
}
