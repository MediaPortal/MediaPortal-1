using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using SQLite.NET;
using MediaPortal.GUI.View;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;

namespace MediaPortal.GUI.Music
{
	/// <summary>
	/// Summary description for MusicViewHandler.
	/// </summary>
	public class MusicViewHandler
	{


		ViewDefinition currentView;
		int						 currentLevel=0;
		MusicDatabase	 database;
		ArrayList      views=new ArrayList();					
		public MusicViewHandler()
		{
			database = new MusicDatabase();
			using(FileStream fileStream = new FileStream("musicViews.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				try
				{
					SoapFormatter formatter = new SoapFormatter();
					views = (ArrayList)formatter.Deserialize(fileStream);
					fileStream.Close();
				}
				catch
				{
				}
			}
		}

		public ViewDefinition View
		{
			get { return currentView; }
			set { currentView=value;}
		}


		public ArrayList Views
		{
			get { return views; }
			set { views=value;}
		}

		public string CurrentView 
		{
			get { 
				if (currentView == null) 
					return String.Empty;
				return currentView.Name; 
			}
			set { 
				foreach (ViewDefinition definition in views)
				{
					if (definition.Name == value) 
					{
						View=definition;
						CurrentLevel=0;
					}
				}
			}
		}

		public int CurrentLevel
		{
			get { return currentLevel;}
			set { 
				if (value < 0 || value >= currentView.Filters.Count) return;
				currentLevel=value;
			}
		}
		public int MaxLevels
		{
			get { return currentView.Filters.Count;}
		}
		
		public void Select(Song song)
		{
			FilterDefinition definition=(FilterDefinition)currentView.Filters[CurrentLevel];
			definition.SelectedValue=GetFieldIdValue(song,definition.Where).ToString();
			if (currentLevel+1 < currentView.Filters.Count) currentLevel++;

		}
		public ArrayList Execute()
		{
			//build the query
			ArrayList songs=new ArrayList();
			string whereClause=String.Empty;
			string orderClause=String.Empty;
			if (CurrentLevel >0)
			{
				whereClause="where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist ";
			}

			for (int i=0; i < CurrentLevel;++i)
			{
				BuildSelect((FilterDefinition)currentView.Filters[i],ref whereClause );
			}
			BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel],ref whereClause );
			BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel],ref whereClause );
			BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel],ref orderClause );

			//execute the query
			string sql;
			if (CurrentLevel==0)
			{
				bool useSongTable=false;
				bool useAlbumTable=false;
				bool useArtistTable=false;
				bool useGenreTable=false;
				FilterDefinition defRoot=(FilterDefinition)currentView.Filters[0];
				string table=GetTable(defRoot.Where,ref useSongTable, ref useAlbumTable, ref useArtistTable,ref useGenreTable);

				if (table=="album")
				{
					sql=String.Format("select * from album ");
					if (whereClause!=String.Empty) sql+= "where "+whereClause;
					if (orderClause!=String.Empty) sql+= orderClause;
					database.GetSongsByFilter(sql, out songs,false, true, false, false);
				}
				else if (table=="artist")
				{
					sql=String.Format("select * from artist ");
					if (whereClause!=String.Empty) sql+= "where "+whereClause;
					if (orderClause!=String.Empty) sql+= orderClause;
					database.GetSongsByFilter(sql, out songs,true, false, false, false);
				}
				else if (table=="genre")
				{
					sql=String.Format("select * from genre ");
					if (whereClause!=String.Empty) sql+= "where "+whereClause;
					if (orderClause!=String.Empty) sql+= orderClause;
					database.GetSongsByFilter(sql, out songs,false, false, false, true);
				}
				else if (defRoot.Where=="year")
				{
					songs = new ArrayList();
					sql=String.Format("select distinct iYear from song ");
					SQLiteResultSet results=database.GetResults(sql);
					for (int i=0; i<results.Rows.Count; i++)
					{
						Song song = new Song();
						song.Year =  (int)Math.Floor(0.5d+Double.Parse(Database.DatabaseUtility.Get(results,i,"iYear")));
						songs.Add(song);
					}
				}
				else
				{
					whereClause="where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist ";
					BuildRestriction(defRoot,ref whereClause );
					sql=String.Format("select * from song,album,genre,artist,path {0} {1}",
						whereClause,orderClause);
					database.GetSongsByFilter(sql, out songs,true, true, true, true);
				}
			}
			else if (CurrentLevel < MaxLevels-1)
			{
				bool useSongTable=false;
				bool useAlbumTable=false;
				bool useArtistTable=false;
				bool useGenreTable=false;
				FilterDefinition defCurrent=(FilterDefinition)currentView.Filters[CurrentLevel];
				string table=GetTable(defCurrent.Where,ref useSongTable, ref useAlbumTable, ref useArtistTable,ref useGenreTable);
				sql=String.Format("select distinct {0}.* from song,album,genre,artist,path {1} {2}",
													table,whereClause,orderClause);
				database.GetSongsByFilter(sql, out songs,useArtistTable, useAlbumTable, useSongTable, useGenreTable);
				
			}
			else
			{
				sql=String.Format("select * from song,album,genre,artist,path {0} {1}",
					whereClause,orderClause);
				database.GetSongsByFilter(sql, out songs,true, true, true, true);
			}
			return songs;
		}

		void BuildSelect(FilterDefinition filter,ref string whereClause )
		{
			if (whereClause!="") whereClause+=" and ";
			whereClause+=String.Format(" {0}='{1}'",GetFieldId(filter.Where),filter.SelectedValue);
		}
		void BuildRestriction(FilterDefinition filter,ref string whereClause )
		{
			if (filter.SqlOperator != String.Empty && filter.Restriction != String.Empty)
			{
				if (whereClause!="") whereClause+=" and ";
				string restriction=filter.Restriction;
				restriction=restriction.Replace("*","%");
				whereClause+=String.Format(" {0} {1} '{2}'",GetFieldName(filter.Where),filter.SqlOperator,restriction);
			}
		}
		void BuildWhere(FilterDefinition filter,ref string whereClause )
		{
			if (filter.WhereValue!="*") 
			{
				if (whereClause!="") whereClause+=" and ";
				whereClause+=String.Format(" {0}='{1}'",GetField(filter.Where),filter.WhereValue);
			}
		}

		void BuildOrder(FilterDefinition filter,ref string orderClause )
		{
			orderClause=" order by "+GetField(filter.Where) + " ";
			if (!filter.SortAscending) orderClause+="desc";
			else orderClause+="asc";
			if (filter.Limit>0)
			{
				orderClause += String.Format(" Limit {0}", filter.Limit);
			}
		}
		string GetTable(string where,ref bool useSongTable, ref bool useAlbumTable, ref bool useArtistTable,ref bool useGenreTable)
		{
			if (where=="album") { useAlbumTable=true;return "album";}
			if (where=="artist") { useArtistTable=true;return "artist";}
			if (where=="title") { useSongTable=true;return "song";}
			if (where=="genre") { useGenreTable=true;return "genre";}
			if (where=="year") { useSongTable=true;return "song";}
			if (where=="track") { useSongTable=true;return "song";}
			if (where=="timesplayed") { useSongTable=true;return "song";}
			if (where=="rating") { useSongTable=true;return "song";}
			if (where=="favorites") { useSongTable=true;return "song";}
			return null;
		}
		string GetField(string where)
		{
			if (where=="album") return "strAlbum";
			if (where=="artist") return "strArtist";
			if (where=="title") return "strTitle";
			if (where=="genre") return "strGenre";
			if (where=="year") return "iYear";
			if (where=="track") return "iTrack";
			if (where=="timesplayed") return "iTimesPlayed";
			if (where=="rating") return "iRating";
			if (where=="favorites") return "favorite";
			return null;
		}
		string GetFieldId(string where)
		{
			if (where=="album") return "album.idAlbum";
			if (where=="artist") return "song.idArtist";
			if (where=="title") return "song.idSong";
			if (where=="genre") return "genre.idGenre";
			if (where=="year") return "song.iYear";
			if (where=="track") return "song.iTrack";
			if (where=="timesplayed") return "song.iTimesPlayed";
			if (where=="rating") return "song.iRating";
			if (where=="favorites") return "song.favorite";
			return null;
		}
		string GetFieldName(string where)
		{
			if (where=="album") return "album.strAlbum";
			if (where=="artist") return "artist.strArtist";
			if (where=="title") return "song.strTitle";
			if (where=="genre") return "genre.strGenre";
			if (where=="year") return "song.iYear";
			if (where=="track") return "song.iTrack";
			if (where=="timesplayed") return "song.iTimesPlayed";
			if (where=="rating") return "song.iRating";
			if (where=="favorites") return "song.favorite";
			return null;
		}
		int GetFieldIdValue(Song song,string where)
		{
			if (where=="album") return song.albumId;
			if (where=="artist") return song.artistId;
			if (where=="title") return song.songId;
			if (where=="genre") return song.genreId;
			if (where=="year") return song.Year;
			if (where=="track") return song.Track;
			if (where=="timesplayed") return song.TimesPlayed;
			if (where=="rating") return song.Rating;
			if (where=="favorites") 
			{
				if (song.Favorite) return 1;
				return 0;
			}
			return -1;
		}

		public void SetLabel(Song song,ref GUIListItem item)
		{
			if (song==null) return;
			FilterDefinition definition=(FilterDefinition)currentView.Filters[CurrentLevel];
			if (definition.Where=="genre")
			{
				item.Label=song.Genre;
				item.Label2=String.Empty;
				item.Label3=String.Empty;
			}
			if (definition.Where=="album")
			{
				item.Label=song.Album;
				item.Label2=song.Artist;
				item.Label3=String.Empty;
			}
			if (definition.Where=="artist")
			{
				item.Label=song.Artist;
				item.Label2=String.Empty;
				item.Label3=String.Empty;
			}
			if (definition.Where=="year")
			{
				item.Label=song.Year.ToString();
				item.Label2=String.Empty;
				item.Label3=String.Empty;
			}

		}
	}
}
