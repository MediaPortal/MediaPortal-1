using System;

namespace MediaPortal.Music.Database
{
	/// <summary>
	/// 
	/// </summary>
	public class Song
	{


    string m_strFileName="";
    string m_strTitle="";
    string m_strArtist="";
    string m_strAlbum="";
    string m_strGenre="";
    int m_iTrack=0;
    int m_iDuration=0;
    int m_iYear=0;
    int m_iTimedPlayed=0;

    public Song()
		{
		}
    public Song Clone()
    {
      Song newsong = new Song();
      newsong.Album = Album;
      newsong.Artist = Artist;
      newsong.Duration = Duration;
      newsong.FileName = FileName;
      newsong.Genre = Genre;
      newsong.TimesPlayed = TimesPlayed;
      newsong.Title = Title;
      newsong.Track = Track;
      newsong.Year = Year;
      return newsong;
    }

    public void Clear() 
    {
      m_strFileName="";
      m_strTitle="";
      m_strArtist="";
      m_strAlbum="";
      m_strGenre="";
      m_iTrack=0;
      m_iDuration=0;
      m_iYear=0;
      m_iTimedPlayed=0;
    }

    public string FileName
    {
      get { return m_strFileName;}
      set {m_strFileName=value;}
    }
    public string Title
    {
      get { return m_strTitle;}
      set {m_strTitle=value;}
    }
    public string Artist
    {
      get { return m_strArtist;}
      set {m_strArtist=value;}
    }
    public string Album
    {
      get { return m_strAlbum;}
      set {m_strAlbum=value;}
    }
    public string Genre
    {
      get { return m_strGenre;}
      set {m_strGenre=value;}
    }
    public int Track
    {
      get { return m_iTrack;}
      set {m_iTrack=value;}
    }
    public int Duration
    {
      get { return m_iDuration;}
      set {m_iDuration=value;}
    }
    public int Year
    {
      get { return m_iYear;}
      set {m_iYear=value;}
    }
    public int TimesPlayed
    {
      get { return m_iTimedPlayed;}
      set {m_iTimedPlayed=value;}
    }

  }
	public class SongMap
	{
		public string m_strPath;
		public Song   m_song;
	}
}
