using System;

namespace MediaPortal.Music.Database
{
	/// <summary>
	/// 
	/// </summary>
	public class MusicSong
	{
    int					m_iTrack;
    string    	m_strSongName;
    int					m_iDuration;

    public MusicSong()
		{
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
    public string SongName
    {
      get { return m_strSongName;}
      set {m_strSongName=value.Trim();}
    }
	}
}
