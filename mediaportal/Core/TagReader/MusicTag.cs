using System;

namespace MediaPortal.TagReader
{
	/// <summary>
	/// Class holding all information about a music file (like an .mp3)
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
		
		/// <summary>
		/// empty constructor
		/// </summary>
    public MusicTag()
		{
    }

		/// <summary>
		/// copy constructor
		/// </summary>
		/// <param name="tag"></param>
    public MusicTag(MusicTag tag)
    {
			if (tag==null) return;
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
		/// <summary>
		/// Property to get/set the comment field of the music file
		/// </summary>
    public string Comment
    {
      get { return m_strComment;}
      set {
				if (value==null) return;
				m_strComment=value.Trim();
			}
		}
		/// <summary>
		/// Property to get/set the Title field of the music file
		/// </summary>
    public string Title
    {
      get { return m_strTitle;}
			set 
			{
				if (value==null) return;
				m_strTitle=value.Trim();
			}
		}
		/// <summary>
		/// Property to get/set the Artist field of the music file
		/// </summary>
    public string Artist
    {
      get { return m_strArtist;}
			set 
			{
				if (value==null) return;
				m_strArtist=value.Trim();
			}
		}
		/// <summary>
		/// Property to get/set the comment Album name of the music file
		/// </summary>
    public string Album
    {
      get { return m_strAlbum;}
			set 
			{
				if (value==null) return;
				m_strAlbum=value.Trim();
			}
		}
		/// <summary>
		/// Property to get/set the Genre field of the music file
		/// </summary>
    public string Genre
    {
      get { return m_strGenre;}
			set 
			{
				if (value==null) return;
				m_strGenre=value.Trim();
			}
		}
		/// <summary>
		/// Property to get/set the Year field of the music file
		/// </summary>
    public int Year
    {
      get { return m_iYear;}
      set {m_iYear=value;}
		}
		/// <summary>
		/// Property to get/set the duration in seconds of the music file
		/// </summary>
    public int Duration
    {
      get { return m_iDuration;}
      set {m_iDuration=value;}
		}
		/// <summary>
		/// Property to get/set the Track number field of the music file
		/// </summary>
    public int Track
    {
      get { return m_iTrack;}
      set {m_iTrack=value;}
		}
		/// <summary>
		/// Property to get/set the number of times this file has been played
		/// </summary>
    public int TimesPlayed
    {
      get { return _TimesPlayed;}
      set {_TimesPlayed=value;}
		}
		/// <summary>
		/// Method to clear the current item
		/// </summary>
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
