#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
  public class ArtistInfo
  {
    private string m_strArtist = "";
    private string m_strBorn = "";
    private string m_strYearsActive = "";
    private string m_strGenres = "";
    private string m_strTones = "";
    private string m_strStyles = "";
    private string m_strInstruments = "";
    private string m_strAMGBio = "";
    private string m_strImage = "";
    private string m_strAlbums = "";
    private string m_strCompilations = "";
    private string m_strSingles = "";
    private string m_strMisc = "";

    public ArtistInfo()
    {
    }

    public ArtistInfo Clone()
    {
      ArtistInfo newartist = new ArtistInfo();
      newartist.Artist = Artist;
      newartist.Born = Born;
      newartist.YearsActive = YearsActive;
      newartist.Genres = Genres;
      newartist.Tones = Tones;
      newartist.Styles = Styles;
      newartist.Image = Image;
      newartist.Instruments = Instruments;
      newartist.AMGBio = AMGBio;
      newartist.Albums = Albums;
      newartist.Compilations = Compilations;
      newartist.Singles = Singles;
      newartist.Misc = Misc;
      return newartist;
    }

    public string Born
    {
      get { return m_strBorn; }
      set { m_strBorn = value; }
    }

    public string YearsActive
    {
      get { return m_strYearsActive; }
      set { m_strYearsActive = value; }
    }

    public string Image
    {
      get { return m_strImage; }
      set { m_strImage = value; }
    }

    public string AMGBio
    {
      get { return m_strAMGBio; }
      set { m_strAMGBio = value; }
    }

    public string Styles
    {
      get { return m_strStyles; }
      set { m_strStyles = value; }
    }

    public string Tones
    {
      get { return m_strTones; }
      set { m_strTones = value; }
    }

    public string Genres
    {
      get { return m_strGenres; }
      set { m_strGenres = value; }
    }

    public string Artist
    {
      get { return m_strArtist; }
      set { m_strArtist = value; }
    }

    public string Albums
    {
      get { return m_strAlbums; }
      set { m_strAlbums = value; }
    }

    public string Compilations
    {
      get { return m_strCompilations; }
      set { m_strCompilations = value; }
    }

    public string Singles
    {
      get { return m_strSingles; }
      set { m_strSingles = value; }
    }

    public string Misc
    {
      get { return m_strMisc; }
      set { m_strMisc = value; }
    }

    public string Instruments
    {
      get { return m_strInstruments; }
      set { m_strInstruments = value; }
    }
  }
}