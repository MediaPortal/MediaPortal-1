using System;

namespace MediaPortal.Music.Database
{
	/// <summary>
	/// 
	/// </summary>
	public class AlbumInfo 
	{
    string m_strAlbum="";
    string m_strArtist="";
    string m_strGenre="";
    string m_strTones="" ;
    string m_strStyles="" ;
    string m_strReview="" ;
    string m_strImage ="";
    string m_strTracks ="";
    int    m_iRating=0 ;
    int		 m_iYear=0;
    
		
    public AlbumInfo()
		{
		}
    
    public AlbumInfo Clone()
    {
      AlbumInfo newalbum = new AlbumInfo();
      newalbum.Album=Album;
      newalbum.Artist=Artist;
      newalbum.Genre=Genre;
      newalbum.Image=Image;
      newalbum.Rating=Rating;
      newalbum.Review=Review;
      newalbum.Styles=Styles;
      newalbum.Tones=Tones;
      newalbum.Tracks=Tracks;
      newalbum.Year=Year;
      return newalbum;
    }

    public int Year
    {
      get { return m_iYear;}
      set { m_iYear=value;}
    }
    public int Rating
    {
      get { return m_iRating;}
      set { m_iRating=value;}
    }
    public string Image
    {
      get { return m_strImage;}
      set { m_strImage=value;}
    }
    public string Review
    {
      get { return m_strReview;}
      set { m_strReview=value;}
    }
    public string Styles
    {
      get { return m_strStyles;}
      set { m_strStyles=value;}
    }
    public string Tones
    {
      get { return m_strTones;}
      set { m_strTones=value;}
    }
    public string Genre
    {
      get { return m_strGenre;}
      set { m_strGenre=value;}
    }
    public string Artist
    {
      get { return m_strArtist;}
      set { m_strArtist=value;}
    }

    public string Album
    {
      get { return m_strAlbum;}
      set { m_strAlbum=value;}
    }
    public string Tracks
    {
      get
      {
        return m_strTracks;
      }
      set 
      {
        m_strTracks=value;
      }
    }
	}
}
