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
using MediaPortal.Utils.Services;

namespace MediaPortal.Util
{
	/// <summary>
	/// Summary description for Thumbs.
	/// </summary>
	public class Thumbs
	{
    static ServiceProvider services = GlobalServiceProvider.Instance;
    static IConfig _config = services.Get<IConfig>();

		static public readonly string TvNotifyIcon="tvguide_notify_button.png";
		static public readonly string TvRecordingIcon="tvguide_record_button.png";
		static public readonly string TvRecordingSeriesIcon="tvguide_recordserie_button.png";
		static public readonly string TvConflictRecordingIcon="tvguide_recordconflict_button.png";

    static public readonly string MusicAlbum = _config.Get(Config.Options.ThumbsPath) + @"music\albums";
    static public readonly string MusicArtists = _config.Get(Config.Options.ThumbsPath) + @"music\artists";
    static public readonly string MusicGenre = _config.Get(Config.Options.ThumbsPath) + @"music\genre";

    static public readonly string MovieTitle = _config.Get(Config.Options.ThumbsPath) + @"Videos\Title";
    static public readonly string MovieActors = _config.Get(Config.Options.ThumbsPath) + @"Videos\Actors";
    static public readonly string MovieGenre = _config.Get(Config.Options.ThumbsPath) + @"Videos\genre";

    static public readonly string TVChannel = _config.Get(Config.Options.ThumbsPath) + @"tv\logos";
    static public readonly string TVShows = _config.Get(Config.Options.ThumbsPath) + @"tv\shows";

    static public readonly string Radio = _config.Get(Config.Options.ThumbsPath) + @"Radio";
    static public readonly string Pictures = _config.Get(Config.Options.ThumbsPath) + @"Pictures";
    static public readonly string Yac = _config.Get(Config.Options.ThumbsPath) + @"yac";

		static public void CreateFolders()
		{
				try
				{
					System.IO.Directory.CreateDirectory(_config.Get(Config.Options.ThumbsPath));
          System.IO.Directory.CreateDirectory(_config.Get(Config.Options.ThumbsPath) + "music");
          System.IO.Directory.CreateDirectory(_config.Get(Config.Options.ThumbsPath) + "videos");
          System.IO.Directory.CreateDirectory(_config.Get(Config.Options.ThumbsPath) + "tv");
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
