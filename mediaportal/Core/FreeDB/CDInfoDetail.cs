#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

namespace MediaPortal.Freedb
{
  /// <summary>
  /// Summary description for CDInfoDetail.
  /// </summary>
  public class CDInfoDetail
  {
    private string m_discId;
    private string m_artist;
    private string m_title;
    private string m_genre;
    private int m_year;
    private int m_duration;
    private CDTrackDetail[] m_tracks;
    private string m_extd;
    private int[] m_playorder;

    public CDInfoDetail()
    {
      //
      // TODO: Add constructor logic here
      //
    }

    public CDInfoDetail(string discID, string artist, string title,
                        string genre, int year, int duration, CDTrackDetail[] tracks,
                        string extd, int[] playorder)
    {
      m_discId = discID;
      m_artist = artist;
      m_title = title;
      m_genre = genre;
      m_year = year;
      m_duration = duration;
      m_tracks = tracks;
      m_extd = extd;
      m_playorder = playorder;
    }

    public string DiscID
    {
      get { return m_discId; }
      set { m_discId = value; }
    }

    public string Artist
    {
      get { return m_artist; }
      set { m_artist = value; }
    }

    public string Title
    {
      get { return m_title; }
      set { m_title = value; }
    }

    public string Genre
    {
      get { return m_genre; }
      set { m_genre = value; }
    }

    public int Year
    {
      get { return m_year; }
      set { m_year = value; }
    }

    public int Duration
    {
      get { return m_duration; }
      set { m_duration = value; }
    }

    public CDTrackDetail getTrack(int index)
    {
      return m_tracks[index - 1];
    }

    public CDTrackDetail[] Tracks
    {
      get { return m_tracks; }
      set { m_tracks = value; }
    }

    public string EXTD
    {
      get { return m_extd; }
      set { m_extd = value; }
    }

    public int[] PlayOrder
    {
      get { return m_playorder; }
      set { m_playorder = value; }
    }
  }
}