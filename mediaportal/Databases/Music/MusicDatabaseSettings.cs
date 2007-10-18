#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
	public class MusicDatabaseSettings
	{
    bool _treatFolderAsAlbum;
    bool _extractEmbededCoverArt;
    bool _useFolderThumbs;
    bool _useFolderArtForArtistGenre;
    bool _createMissingFolderThumbs;  
    bool _stripArtistPrefixes;
    bool _useLastImportDate;


    public MusicDatabaseSettings()
		{
		}

    public bool TreatFolderAsAlbum
    {
      get { return _treatFolderAsAlbum; }
      set { _treatFolderAsAlbum = value; }
    }

    public bool ExtractEmbeddedCoverArt
    {
      get { return _extractEmbededCoverArt; }
      set { _extractEmbededCoverArt = value; }
    }

    public bool UseFolderThumbs
    {
      get { return _useFolderThumbs; }
      set { _useFolderThumbs = value; }
    }

    public bool UseFolderThumbsForArtistGenre
    {
      get { return _useFolderArtForArtistGenre; }
      set { _useFolderArtForArtistGenre = value; }
    }

    public bool CreateMissingFolderThumb
    {
      get { return _createMissingFolderThumbs; }
      set { _createMissingFolderThumbs = value; }
    }

    public bool StripArtistPrefixes
    {
      get { return _stripArtistPrefixes; }
      set { _stripArtistPrefixes = value; }
    }

    public bool UseLastImportDate
    {
      get { return _useLastImportDate; }
      set { _useLastImportDate = value; }
    }
	}
}
