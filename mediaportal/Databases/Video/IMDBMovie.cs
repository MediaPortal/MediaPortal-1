using System;

namespace MediaPortal.Video.Database
{
	/// <summary>
	/// @ 23.09.2004 by FlipGer
	/// new attribute string m_strDatabase, holds for example IMDB or OFDB
	/// </summary>
	public class IMDBMovie
	{

    string 			m_strDirector="";
    string 			m_strWritingCredits="";
    string 			m_strGenre="";
    string 			m_strTagLine="";
    string 			m_strPlotOutline="";
    string 			m_strPlot="";
    string 			m_strPictureURL="";
    string 			m_strTitle="";
    string 			m_strVotes="";
    string 			m_strCast="";
    string 			m_strSearchString="";
    string 			m_strFile="";
    string 			m_strPath="";
    string 			m_strDVDLabel="";
    string 			m_strIMDBNumber="";
	string 			m_strDatabase="";
    string      m_strCDLabel="";
    int				 m_iTop250=0;
    int    		 m_iYear=1900;
    float  		 m_fRating=0.0f;
    
    public IMDBMovie()
		{
		}
    public string Director
    {
      get { return m_strDirector;}
      set { m_strDirector=value;}
    }
    public string WritingCredits
    {
      get { return m_strWritingCredits;}
      set { m_strWritingCredits=value;}
    }
    public string Genre
    {
      get { return m_strGenre;}
      set { m_strGenre=value;}
    }
    public string TagLine
    {
      get { return m_strTagLine;}
      set { m_strTagLine=value;}
    }
    public string PlotOutline
    {
      get { return m_strPlotOutline;}
      set { m_strPlotOutline=value;}
    }
    public string Plot
    {
      get { return m_strPlot;}
      set { m_strPlot=value;}
    }
    public string ThumbURL
    {
      get { return m_strPictureURL;}
      set { m_strPictureURL=value;}
    }
    public string Title
    {
      get { return m_strTitle;}
      set { m_strTitle=value;}
    }
    public string Votes
    {
      get { return m_strVotes;}
      set { m_strVotes=value;}
    }
    public string Cast
    {
      get { return m_strCast;}
      set { m_strCast=value;}
    }
    public string SearchString
    {
      get { return m_strSearchString;}
      set { m_strSearchString=value;}
    }
    public string File
    {
      get { return m_strFile;}
      set { m_strFile=value;}
    }
    public string Path
    {
      get { return m_strPath;}
      set { m_strPath=value;}
    }
    public string DVDLabel
    {
      get { return m_strDVDLabel;}
      set { m_strDVDLabel=value;}
    }
    
      
    public string CDLabel
    {
      get { return m_strCDLabel;}
      set { m_strCDLabel=value;}
    }
    public string IMDBNumber
    {
      get { return m_strIMDBNumber;}
      set { m_strIMDBNumber=value;}
    }
    public int Top250
    {
      get { return m_iTop250;}
      set { m_iTop250=value;}
    }
    public int Year
    {
      get { return m_iYear;}
      set { m_iYear=value;}
    }
    public float Rating
    {
      get { return m_fRating;}
      set { m_fRating=value;}
    }
	public string Database
	{
		get { return m_strDatabase;}
		set { m_strDatabase = value;}
	}
    public void Reset()
    {
      m_strDirector="";
      m_strWritingCredits="";
      m_strGenre="";
      m_strTagLine="";
      m_strPlotOutline="";
      m_strPlot="";
      m_strPictureURL="";
      m_strTitle="";
      m_strVotes="";
      m_strCast="";
      m_strSearchString="";
      m_strFile="";
      m_strPath="";
      m_strDVDLabel="";
      m_strIMDBNumber="";
      m_iTop250=0;
      m_iYear=1900;
      m_fRating=0.0f;
		m_strDatabase = "";
    }
	}
}
