using System;

using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
namespace MediaPortal.TV.Database
{
	/// <summary>
	/// Class which holds all information about a recorded TV program
	/// </summary>
	public class TVRecorded
	{
    long				m_iStartTime;
    long				m_iEndTime;
    string			m_strTitle;
    string      m_strChannel;
    string      m_strGenre;
    string      m_strDescription;
    string      m_strFilename;
    int         m_iID=-1;
    int         m_iPlayed=0;

    /// <summary>
    /// Property to get/set the filename of this recorded tv program
    /// </summary>
    public string FileName
    {
      get { return m_strFilename;}
      set { m_strFilename=value;}
    }

    /// <summary>
    /// Property to get/set the description of the recorded tv program 
    /// </summary>
    public string Description
    {
      get { return m_strDescription;}
      set { m_strDescription=value;}
    }

    /// <summary>
    /// Property to get/set the genre of the recorded tv program 
    /// </summary>
    public string Genre
    {
      get { return m_strGenre;}
      set { m_strGenre=value;}
    }

    /// <summary>
    /// Property to get/set the tv channel name of the recorded tv program 
    /// </summary>
    public string Channel
    {
      get { return m_strChannel;}
      set { m_strChannel=value;}
    }

    /// <summary>
    /// Property to get/set the title of the recorded tv program 
    /// </summary>
    public string Title
    {
      get { return m_strTitle;}
      set { m_strTitle=value;}
    }

    /// <summary>
    /// Property to get/set the start time of the recorded tv program in xmltv format :yyyymmddhhmmss
    /// </summary>
    public long Start
    {
      get { return m_iStartTime;}
      set { m_iStartTime=value;}
    }

    /// <summary>
    /// Property to get/set the end time of the recorded tv program in xmltv format :yyyymmddhhmmss
    /// </summary>
    public long End
    {
      get { return m_iEndTime;}
      set { m_iEndTime=value;}
    }

    /// <summary>
    /// Property to get the start time of the recorded tv program  
    /// </summary>
    public DateTime StartTime
    {
      get { return Utils.longtodate(m_iStartTime);}
    }

    /// <summary>
    /// Property to get the end time of the recorded tv program  
    /// </summary>
    public DateTime EndTime
    {
      get { return Utils.longtodate(m_iEndTime);}
    }

    /// <summary>
    /// Property to get/set the database ID of the recorded tv program  
    /// </summary>
    public int ID
    {
      get { return m_iID;}
      set { m_iID=value;}
    }

    /// <summary>
    /// Property to get/set how many times the record tv program has been watched
    /// </summary>
    public int Played
    {
      get { return m_iPlayed;}
      set { m_iPlayed=value;}
    }

	}
}
