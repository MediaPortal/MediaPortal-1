using System;

namespace MediaPortal.Freedb
{
	/// <summary>
	/// Summary description for CDTrackDetails.
	/// </summary>
	public class CDTrackDetail
	{
    private int m_trackNumber;
    private string m_artist;
    private string m_songTitle;
    private int m_duration;
    private int m_offset;
    private string m_extt;

		public CDTrackDetail()
		{
			//
			// TODO: Add constructor logic here
			//
		}

    public CDTrackDetail(string songTitle, string artist, string extt, 
                         int trackNumber, int offset, int duration)
    {
      m_songTitle = songTitle;
      m_artist = artist;
      m_extt = extt;
      m_trackNumber = trackNumber;
      m_offset = offset;
      m_duration = duration;
    }

    public string Title
    {
      get
      {
        return m_songTitle;
      }
      set
      {
        m_songTitle = value;
      }
    }

    // can be null if the artist is the same as the main
    // album
    public string Artist
    {
      get
      {
        return m_artist;
      }
      set
      {
        m_artist = value;
      }
    }


    public int TrackNumber
    {
      get
      {
        return m_trackNumber;
      }
      set
      {
        m_trackNumber = value;
      }
    }

    public int Duration
    {
      get
      {
        return m_duration;
      }
      set
      {
        m_duration = value;
      }
    }

    public int Offset
    {
      get
      {
        return m_offset;
      }
      set
      {
        m_offset = value;
      }
    }

    public string EXTT
    {
      get
      {
        return m_extt;
      }
      set
      {
        m_extt = value;
      }
    }
	}
}
