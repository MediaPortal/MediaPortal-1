using System;

namespace MediaPortal.TagReader
{
	/// <summary>
	/// 
	/// </summary>
	public class MusicTag
	{
    string m_strArtist="";
    string m_strAlbum="";
    string m_strGenre="";
    string m_strTitle="";
    string m_strComment="";
    int    m_iYear=0;
    int    m_iDuration=0;
    int    m_iTrack=0;
    int    _TimesPlayed=0;
		
    public MusicTag()
		{
    }
    public MusicTag(MusicTag tag)
    {
      _TimesPlayed=tag._TimesPlayed;
      Artist=tag.Artist;
      Album=tag.Album;
      Genre=tag.Genre;
      Title=tag.Title;
      Comment=tag.Comment;
      Year=tag.Year;
      Duration=tag.Duration;
      Track=tag.Track;
    }
    public string Comment
    {
      get { return m_strComment;}
      set {m_strComment=value.Trim();}
    }
    public string Title
    {
      get { return m_strTitle;}
      set {m_strTitle=value.Trim();}
    }
    public string Artist
    {
      get { return m_strArtist;}
      set {m_strArtist=value.Trim();}
    }
    public string Album
    {
      get { return m_strAlbum;}
      set {m_strAlbum=value.Trim();}
    }
    public string Genre
    {
      get { return m_strGenre;}
      set {m_strGenre=value.Trim();}
    }
    public int Year
    {
      get { return m_iYear;}
      set {m_iYear=value;}
    }
    public int Duration
    {
      get { return m_iDuration;}
      set {m_iDuration=value;}
    }
    public int Track
    {
      get { return m_iTrack;}
      set {m_iTrack=value;}
    }
    public int TimesPlayed
    {
      get { return _TimesPlayed;}
      set {_TimesPlayed=value;}
    }
    public void Clear()
    {
      _TimesPlayed=0;
      m_strArtist="";
      m_strAlbum="";
      m_strGenre="";
      m_strTitle="";
      m_strComment="";
      m_iYear=0;
      m_iDuration=0;
      m_iTrack=0;
    }
	}
}
