#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Database object for artist info details
  /// </summary>
  [Serializable()]
  public class ArtistInfo
  {
    private string m_strArtist = string.Empty;
    private string m_strBorn = string.Empty;
    private string m_strYearsActive = string.Empty;
    private string m_strGenres = string.Empty;
    private string m_strTones = string.Empty;
    private string m_strStyles = string.Empty;
    private string m_strInstruments = string.Empty;
    private string m_strAMGBio = string.Empty;
    private string m_strImage = string.Empty;
    private string m_strAlbums = string.Empty;
    private string m_strCompilations = string.Empty;
    private string m_strSingles = string.Empty;
    private string m_strMisc = string.Empty;

    public ArtistInfo Clone()
    {
      var newartist = new ArtistInfo
        {
          Artist = Artist,
          Born = Born,
          YearsActive = YearsActive,
          Genres = Genres,
          Tones = Tones,
          Styles = Styles,
          Image = Image,
          Instruments = Instruments,
          AMGBio = AMGBio,
          Albums = Albums,
          Compilations = Compilations,
          Singles = Singles,
          Misc = Misc
        };
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