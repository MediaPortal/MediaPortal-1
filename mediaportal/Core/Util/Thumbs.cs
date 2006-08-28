/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;

namespace MediaPortal.Util
{
	/// <summary>
	/// Summary description for Thumbs.
	/// </summary>
	public class Thumbs
	{

		static public readonly string TvNotifyIcon="tvguide_notify_button.png";
		static public readonly string TvRecordingIcon="tvguide_record_button.png";
		static public readonly string TvRecordingSeriesIcon="tvguide_recordserie_button.png";
		static public readonly string TvConflictRecordingIcon="tvguide_recordconflict_button.png";

    static public readonly string MusicAlbum = Config.Get(Config.Dir.Thumbs) + @"music\albums";
    static public readonly string MusicArtists = Config.Get(Config.Dir.Thumbs) + @"music\artists";
    static public readonly string MusicGenre = Config.Get(Config.Dir.Thumbs) + @"music\genre";

    static public readonly string MovieTitle = Config.Get(Config.Dir.Thumbs) + @"Videos\Title";
    static public readonly string MovieActors = Config.Get(Config.Dir.Thumbs) + @"Videos\Actors";
    static public readonly string MovieGenre = Config.Get(Config.Dir.Thumbs) + @"Videos\genre";

    static public readonly string TVChannel = Config.Get(Config.Dir.Thumbs) + @"tv\logos";
    static public readonly string TVShows = Config.Get(Config.Dir.Thumbs) + @"tv\shows";

    static public readonly string Radio = Config.Get(Config.Dir.Thumbs) + @"Radio";
    static public readonly string Pictures = Config.Get(Config.Dir.Thumbs) + @"Pictures";
    static public readonly string Yac = Config.Get(Config.Dir.Thumbs) + @"yac";

		static public void CreateFolders()
		{
				try
				{
					System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Thumbs));
          System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Thumbs) + "music");
          System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Thumbs) + "videos");
          System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Thumbs) + "tv");
					System.IO.Directory.CreateDirectory(MusicAlbum);
					System.IO.Directory.CreateDirectory(MusicArtists);
					System.IO.Directory.CreateDirectory(MusicGenre);
					System.IO.Directory.CreateDirectory(Pictures);
					System.IO.Directory.CreateDirectory(Radio);
					System.IO.Directory.CreateDirectory(TVChannel);
					System.IO.Directory.CreateDirectory(TVShows);
					System.IO.Directory.CreateDirectory(MovieGenre);
					System.IO.Directory.CreateDirectory(MovieTitle);
					System.IO.Directory.CreateDirectory(MovieActors);
					System.IO.Directory.CreateDirectory(Yac);
				}
				catch(Exception){}
		}
	}
}
