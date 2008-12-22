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

using System;

namespace MediaPortal.Music.Database
{
	/// <summary>
	/// 
	/// </summary>
  [Serializable()]
	public class AlbumInfo 
	{
    string m_strAlbum="";
    string m_strArtist="";
    string m_strAlbumArtist="";
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

    public string AlbumArtist
    {
      get { return m_strAlbumArtist; }
      set { m_strAlbumArtist = value; }
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
