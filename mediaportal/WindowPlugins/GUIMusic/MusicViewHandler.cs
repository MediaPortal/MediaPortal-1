#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using System.Collections.Generic;
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
        int currentLevel = 0;
        MusicDatabase database;
        List<ViewDefinition> views = new List<ViewDefinition>();
        public MusicViewHandler()
        {
            if (!System.IO.File.Exists("musicviews.xml"))
            {
                //genres
                FilterDefinition filter1, filter2, filter3;
                ViewDefinition viewGenre = new ViewDefinition();
                viewGenre.Name = "Genres";
                filter1 = new FilterDefinition(); filter1.Where = "genre"; filter1.SortAscending = true;
                filter2 = new FilterDefinition(); filter2.Where = "title"; filter2.SortAscending = true;
                viewGenre.Filters.Add(filter1);
                viewGenre.Filters.Add(filter2);

                //top100
                ViewDefinition viewTop100 = new ViewDefinition();
                viewTop100.Name = "Top100";
                filter1 = new FilterDefinition(); filter1.Where = "timesplayed"; filter1.SortAscending = false; filter1.Limit = 100;
                filter1.SqlOperator = ">";
                filter1.Restriction = "0";
                viewTop100.Filters.Add(filter1);

                //artists
                ViewDefinition viewArtists = new ViewDefinition();
                viewArtists.Name = "Artists";
                filter1 = new FilterDefinition(); filter1.Where = "artist"; ; filter1.SortAscending = true;
                filter2 = new FilterDefinition(); filter2.Where = "album"; ; filter2.SortAscending = true;
                filter3 = new FilterDefinition(); filter3.Where = "title"; ; filter3.SortAscending = true;
                viewArtists.Filters.Add(filter1);
                viewArtists.Filters.Add(filter2);
                viewArtists.Filters.Add(filter3);

                //albums
                ViewDefinition viewAlbums = new ViewDefinition();
                viewAlbums.Name = "Albums";
                filter1 = new FilterDefinition(); filter1.Where = "album"; ; filter1.SortAscending = true;
                filter2 = new FilterDefinition(); filter2.Where = "title"; ; filter2.SortAscending = true;
                viewAlbums.Filters.Add(filter1);
                viewAlbums.Filters.Add(filter2);

                //years
                ViewDefinition viewYears = new ViewDefinition();
                viewYears.Name = "Years";
                filter1 = new FilterDefinition(); filter1.Where = "year"; ; filter1.SortAscending = true;
                filter2 = new FilterDefinition(); filter2.Where = "title"; ; filter2.SortAscending = true;
                viewYears.Filters.Add(filter1);
                viewYears.Filters.Add(filter2);

                //favorites
                ViewDefinition viewFavorites = new ViewDefinition();
                viewFavorites.Name = "Favorites";
                filter1 = new FilterDefinition(); filter1.Where = "favorites"; filter1.SqlOperator = "="; filter1.Restriction = "1"; filter1.SortAscending = true;
                viewFavorites.Filters.Add(filter1);

                //all songs
                ViewDefinition viewAllSongs = new ViewDefinition();
                viewAllSongs.Name = "All songs";
                filter1 = new FilterDefinition(); filter1.Where = "title"; filter1.SqlOperator = ""; filter1.Restriction = ""; filter1.SortAscending = true;
                viewAllSongs.Filters.Add(filter1);

                List<ViewDefinition> listViews = new List<ViewDefinition>();
                listViews.Add(viewGenre);
                listViews.Add(viewTop100);
                listViews.Add(viewArtists);
                listViews.Add(viewAlbums);
                listViews.Add(viewYears);
                listViews.Add(viewFavorites);
                listViews.Add(viewAllSongs);

                using (FileStream fileStream = new FileStream("musicViews.xml", FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    ArrayList list = new ArrayList();
                    foreach (ViewDefinition view in listViews)
                        list.Add(view);
                    SoapFormatter formatter = new SoapFormatter();
                    formatter.Serialize(fileStream, list);
                    fileStream.Close();
                }
            }

            database = new MusicDatabase();
            using (FileStream fileStream = new FileStream("musicViews.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                try
                {
                    SoapFormatter formatter = new SoapFormatter();
                    ArrayList viewlist = (ArrayList)formatter.Deserialize(fileStream);
                    foreach (ViewDefinition view in viewlist)
                    {
                        views.Add(view);
                    }
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
            set { currentView = value; }
        }


        public List<ViewDefinition> Views
        {
            get { return views; }
            set { views = value; }
        }

        public string CurrentView
        {
            get
            {
                if (currentView == null)
                    return String.Empty;
                return currentView.Name;
            }
            set
            {
                foreach (ViewDefinition definition in views)
                {
                    if (definition.Name == value)
                    {
                        View = definition;
                        CurrentLevel = 0;
                    }
                }
            }
        }

        public void Restore(ViewDefinition view, int level)
        {
            currentView = view;
            currentLevel = level;
        }
        public ViewDefinition GetView()
        {
            return currentView;
        }

        public int CurrentLevel
        {
            get { return currentLevel; }
            set
            {
                if (value < 0 || value >= currentView.Filters.Count) return;
                currentLevel = value;
            }
        }
        public int MaxLevels
        {
            get { return currentView.Filters.Count; }
        }

        public void Select(Song song)
        {
            FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
            definition.SelectedValue = GetFieldIdValue(song, definition.Where).ToString();
            if (currentLevel + 1 < currentView.Filters.Count) currentLevel++;

        }
        public List<Song> Execute()
        {
            //build the query
            List<Song> songs = new List<Song>();
            string whereClause = String.Empty;
            string orderClause = String.Empty;
            if (CurrentLevel > 0)
            {
                whereClause = "where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist ";
            }

            for (int i = 0; i < CurrentLevel; ++i)
            {
                BuildSelect((FilterDefinition)currentView.Filters[i], ref whereClause);
            }
            BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
            BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
            BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel], ref orderClause);

            //execute the query
            string sql;
            if (CurrentLevel == 0)
            {
                bool useSongTable = false;
                bool useAlbumTable = false;
                bool useArtistTable = false;
                bool useGenreTable = false;
                FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];
                string table = GetTable(defRoot.Where, ref useSongTable, ref useAlbumTable, ref useArtistTable, ref useGenreTable);

                if (table == "album")
                {
                    sql = String.Format("select * from album,artist where album.idArtist=artist.idArtist");
                    if (whereClause != String.Empty) sql += "and " + whereClause;
                    if (orderClause != String.Empty) sql += orderClause;
                    database.GetSongsByFilter(sql, out songs, false, true, false, false);
                }
                else if (table == "artist")
                {
                    sql = String.Format("select * from artist ");
                    if (whereClause != String.Empty) sql += "where " + whereClause;
                    if (orderClause != String.Empty) sql += orderClause;
                    database.GetSongsByFilter(sql, out songs, true, false, false, false);
                }
                else if (table == "genre")
                {
                    sql = String.Format("select * from genre ");
                    if (whereClause != String.Empty) sql += "where " + whereClause;
                    if (orderClause != String.Empty) sql += orderClause;
                    database.GetSongsByFilter(sql, out songs, false, false, false, true);
                }
                else if (defRoot.Where == "year")
                {
                    songs = new List<Song>();
                    sql = String.Format("select distinct iYear from song ");
                    SQLiteResultSet results = database.GetResults(sql);
                    for (int i = 0; i < results.Rows.Count; i++)
                    {
                        Song song = new Song();
                        try
                        {
                            song.Year = (int)Math.Floor(0.5d + Double.Parse(Database.DatabaseUtility.Get(results, i, "iYear")));
                        }
                        catch (Exception)
                        {
                            song.Year = 0;
                        }
                        if (song.Year > 1000)
                            songs.Add(song);
                    }
                }
                else
                {
                    whereClause = "where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist ";
                    BuildRestriction(defRoot, ref whereClause);
                    sql = String.Format("select * from song,album,genre,artist,path {0} {1}",
                      whereClause, orderClause);
                    database.GetSongsByFilter(sql, out songs, true, true, true, true);
                }
            }
            else if (CurrentLevel < MaxLevels - 1)
            {
                bool isVariousArtistsAlbum = false;

                bool useSongTable = false;
                bool useAlbumTable = false;
                bool useArtistTable = false;
                bool useGenreTable = false;
                FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
                string table = GetTable(defCurrent.Where, ref useSongTable, ref useAlbumTable, ref useArtistTable, ref useGenreTable);

                sql = String.Format("select distinct {0}.* from song,album,genre,artist,path {1} {2}",
                                table, whereClause, orderClause);

                if (useAlbumTable && CurrentLevel > 0)
                    isVariousArtistsAlbum = ModifyAlbumQueryForVariousArtists(ref sql);

                // Handle "Various Artists" cases where the artist id is different for 
                // many/most/all of the album tracks
                ////if (useAlbumTable && CurrentLevel > 0)
                ////{
                ////    try
                ////    {
                ////        string selectedVal = ((FilterDefinition)currentView.Filters[CurrentLevel - 1]).SelectedValue;

                ////        if (selectedVal.Length > 0)
                ////        {
                ////            int artistId = int.Parse(selectedVal);
                ////            string variousArtists = GUILocalizeStrings.Get(340);

                ////            if (variousArtists.Length == 0)
                ////                variousArtists = "various artists";

                ////            long idVariousArtists = database.AddArtist(variousArtists);

                ////            if (artistId == idVariousArtists)
                ////                sql = sql.Replace("and  song.idArtist=", "and  album.idArtist=");
                ////        }
                ////    }

                ////    catch { }
                ////}

                database.GetSongsByFilter(sql, out songs, useArtistTable, useAlbumTable, useSongTable, useGenreTable);

                if (table == "album")
                {
                    List<AlbumInfo> albums = new List<AlbumInfo>();

                    if (!isVariousArtistsAlbum)
                        database.GetAlbums(ref albums);

                    else
                        database.GetVariousArtistsAlbums(ref albums);

                    foreach (Song song in songs)
                    {
                        foreach (AlbumInfo album in albums)
                        {
                            if (song.Album.Equals(album.Album))
                            {
                                song.Artist = album.Artist;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                sql = String.Format("select * from song,album,genre,artist,path {0} {1}",
                  whereClause, orderClause);

                bool modified = ModifySongsQueryForVariousArtists(ref sql);
                database.GetSongsByFilter(sql, out songs, true, true, true, true);
            }
            return songs;
        }

        // Handle "Various Artists" cases where the artist id is different for 
        // many/most/all of the album tracks
        bool ModifyAlbumQueryForVariousArtists(ref string sOrigSql)
        {
            if (CurrentLevel < 1)
                return false;

            bool modified = false;

            try
            {
                // Replace occurances of multiple space chars with single space
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\s+");
                string temp = r.Replace(sOrigSql, " ");
                sOrigSql = temp;

                string selectedVal = ((FilterDefinition)currentView.Filters[CurrentLevel - 1]).SelectedValue;

                if (selectedVal.Length > 0)
                {
                    int artistId = int.Parse(selectedVal);
                    string variousArtists = GUILocalizeStrings.Get(340);

                    if (variousArtists.Length == 0)
                        variousArtists = "Various Artists";

                    long idVariousArtists = database.AddArtist(variousArtists);

                    if (artistId == idVariousArtists)
                    {
                        modified = true;
                        sOrigSql = sOrigSql.Replace("and song.idArtist=", "and album.idArtist=");
                    }
                }
            }

            catch { }

            return modified;
        }

        bool ModifySongsQueryForVariousArtists(ref string sOrigSql)
        {
             if (CurrentLevel < 1)
                return false;

            bool modified = false;

            try
            {
                // Replace occurances of multiple space chars with single space
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\s+");
                string temp = r.Replace(sOrigSql, " ");
                sOrigSql = temp;

                string variousArtists = GUILocalizeStrings.Get(340);

                if (variousArtists.Length == 0)
                    variousArtists = "Various Artists";

                long idVariousArtists = database.AddArtist(variousArtists);
                string sSearch = string.Format("song.idArtist='{0}'", idVariousArtists);
                string sReplace = string.Format("album.idArtist='{0}'", idVariousArtists);

                string newSqlString = sOrigSql.Replace(sSearch, sReplace);
                modified = newSqlString != sOrigSql;

                sOrigSql = newSqlString;
            }

            catch { }

            return modified;
       }

        void BuildSelect(FilterDefinition filter, ref string whereClause)
        {
            if (whereClause != "") whereClause += " and ";
            whereClause += String.Format(" {0}='{1}'", GetFieldId(filter.Where), filter.SelectedValue);
        }
        void BuildRestriction(FilterDefinition filter, ref string whereClause)
        {
            if (filter.SqlOperator != String.Empty && filter.Restriction != String.Empty)
            {
                if (whereClause != "") whereClause += " and ";
                string restriction = filter.Restriction;
                restriction = restriction.Replace("*", "%");
                Database.DatabaseUtility.RemoveInvalidChars(ref restriction);
                if (filter.SqlOperator == "=")
                {
                    bool isascii = false;
                    for (int x = 0; x < restriction.Length; ++x)
                    {
                        if (!Char.IsDigit(restriction[x]))
                        {
                            isascii = true;
                            break;
                        }
                    }
                    if (isascii)
                    {
                        filter.SqlOperator = "like";
                    }
                }
                whereClause += String.Format(" {0} {1} '{2}'", GetFieldName(filter.Where), filter.SqlOperator, restriction);
            }
        }
        void BuildWhere(FilterDefinition filter, ref string whereClause)
        {
            if (filter.WhereValue != "*")
            {
                if (whereClause != "") whereClause += " and ";
                whereClause += String.Format(" {0}='{1}'", GetField(filter.Where), filter.WhereValue);
            }
        }

        void BuildOrder(FilterDefinition filter, ref string orderClause)
        {
            orderClause = " order by " + GetField(filter.Where) + " ";
            if (!filter.SortAscending) orderClause += "desc";
            else orderClause += "asc";
            if (filter.Limit > 0)
            {
                orderClause += String.Format(" Limit {0}", filter.Limit);
            }
        }
        string GetTable(string where, ref bool useSongTable, ref bool useAlbumTable, ref bool useArtistTable, ref bool useGenreTable)
        {
            if (where == "album") { useAlbumTable = true; return "album"; }
            if (where == "artist") { useArtistTable = true; return "artist"; }
            if (where == "title") { useSongTable = true; return "song"; }
            if (where == "genre") { useGenreTable = true; return "genre"; }
            if (where == "year") { useSongTable = true; return "song"; }
            if (where == "track") { useSongTable = true; return "song"; }
            if (where == "timesplayed") { useSongTable = true; return "song"; }
            if (where == "rating") { useSongTable = true; return "song"; }
            if (where == "favorites") { useSongTable = true; return "song"; }
            return null;
        }
        string GetField(string where)
        {
            if (where == "album") return "strAlbum";
            if (where == "artist") return "strArtist";
            if (where == "title") return "strTitle";
            if (where == "genre") return "strGenre";
            if (where == "year") return "iYear";
            if (where == "track") return "iTrack";
            if (where == "timesplayed") return "iTimesPlayed";
            if (where == "rating") return "iRating";
            if (where == "favorites") return "favorite";
            return null;
        }

        string SetField(Song song, string where, string newValue)
        {
            if (where == "album") song.Album = newValue;
            if (where == "artist") song.Artist = newValue;
            if (where == "title") song.Title = newValue;
            if (where == "genre") song.Genre = newValue;
            if (where == "year") song.Year = Int32.Parse(newValue);
            if (where == "track") song.Track = Int32.Parse(newValue);
            if (where == "timesplayed") song.TimesPlayed = Int32.Parse(newValue);
            if (where == "rating") song.Rating = Int32.Parse(newValue);
            if (where == "favorites") song.Favorite = (Int32.Parse(newValue) != 0);
            return null;
        }
        string GetFieldId(string where)
        {
            if (where == "album") return "album.idAlbum";
            if (where == "artist") return "song.idArtist";
            if (where == "title") return "song.idSong";
            if (where == "genre") return "genre.idGenre";
            if (where == "year") return "song.iYear";
            if (where == "track") return "song.iTrack";
            if (where == "timesplayed") return "song.iTimesPlayed";
            if (where == "rating") return "song.iRating";
            if (where == "favorites") return "song.favorite";
            return null;
        }
        string GetFieldName(string where)
        {
            if (where == "album") return "album.strAlbum";
            if (where == "artist") return "artist.strArtist";
            if (where == "title") return "song.strTitle";
            if (where == "genre") return "genre.strGenre";
            if (where == "year") return "song.iYear";
            if (where == "track") return "song.iTrack";
            if (where == "timesplayed") return "song.iTimesPlayed";
            if (where == "rating") return "song.iRating";
            if (where == "favorites") return "song.favorite";
            return null;
        }
        int GetFieldIdValue(Song song, string where)
        {
            if (where == "album") return song.albumId;
            if (where == "artist") return song.artistId;
            if (where == "title") return song.songId;
            if (where == "genre") return song.genreId;
            if (where == "year") return song.Year;
            if (where == "track") return song.Track;
            if (where == "timesplayed") return song.TimesPlayed;
            if (where == "rating") return song.Rating;
            if (where == "favorites")
            {
                if (song.Favorite) return 1;
                return 0;
            }
            return -1;
        }


        public void SetLabel(Song song, ref GUIListItem item)
        {
            if (song == null) return;
            FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
            if (definition.Where == "genre")
            {
                item.Label = song.Genre;
                item.Label2 = String.Empty;
                item.Label3 = String.Empty;
            }
            if (definition.Where == "album")
            {
                item.Label = song.Album;
                item.Label2 = song.Artist;
                item.Label3 = String.Empty;
            }
            if (definition.Where == "artist")
            {
                item.Label = song.Artist;
                item.Label2 = String.Empty;
                item.Label3 = String.Empty;
            }
            if (definition.Where == "year")
            {
                item.Label = song.Year.ToString();
                item.Label2 = String.Empty;
                item.Label3 = String.Empty;
            }

        }
    }
}