using System;

namespace MediaPortal.Util
{
	/// <summary>
	/// Summary description for Thumbs.
	/// </summary>
	public class Thumbs
	{

		static public readonly string TvRecordingIcon="tvguide_record_button.png";
		static public readonly string TvRecordingSeriesIcon="tvguide_recordserie_button.png";
		static public readonly string TvConflictRecordingIcon="tvguide_recordconflict_button.png";

		static public readonly string MusicAlbum=@"thumbs\music\albums";
		static public readonly string MusicArtists=@"thumbs\music\artists";
		static public readonly string MusicGenre=@"thumbs\music\genre";

		static public readonly string MovieTitle=@"thumbs\Videos\Title";
		static public readonly string MovieActors=@"thumbs\Videos\Actors";
		static public readonly string MovieGenre=@"thumbs\Videos\genre";

		static public readonly string TVChannel=@"thumbs\tv\logos";
		static public readonly string TVShows=@"thumbs\tv\shows";

		static public readonly string Radio=@"Thumbs\Radio";
		static public readonly string Pictures=@"Thumbs\Pictures";
		static public readonly string Yac=@"Thumbs\yac";
		
		static public void CreateFolders()
		{
				try
				{
					System.IO.Directory.CreateDirectory(@"thumbs");
					System.IO.Directory.CreateDirectory(@"thumbs\music");
					System.IO.Directory.CreateDirectory(@"thumbs\videos");
					System.IO.Directory.CreateDirectory(@"thumbs\tv");
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
