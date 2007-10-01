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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using System.Collections.Generic;
using SQLite.NET;
using MediaPortal.GUI.View;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using MediaPortal.Music.Database;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for MusicViewHandler.
  /// </summary>
  public class MusicViewHandler
  {
    string defaultMusicViews = Config.GetFile(Config.Dir.Base, "defaultMusicViews.xml");
    string customMusicViews = Config.GetFile(Config.Dir.Config, "MusicViews.xml");

    ViewDefinition currentView;
    int currentLevel = 0;
    List<ViewDefinition> views = new List<ViewDefinition>();

    MusicDatabase database;
    int restrictionLength = 0;   // used to sum up the length of all restrictions

    public MusicViewHandler()
    {
      if (!File.Exists(customMusicViews))
      {
        File.Copy(defaultMusicViews, customMusicViews);
      }

      try
      {
        using (FileStream fileStream = new FileInfo(customMusicViews).OpenRead())
        {
          SoapFormatter formatter = new SoapFormatter();
          ArrayList viewlist = (ArrayList)formatter.Deserialize(fileStream);
          foreach (ViewDefinition view in viewlist)
          {
            views.Add(view);
          }
          fileStream.Close();
        }
      }
      catch (Exception)
      {
      }

      database = MusicDatabase.Instance;
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
    public string LocalizedCurrentView
    {
      get
      {
        if (currentView == null)
          return String.Empty;
        return currentView.LocalizedName;
      }
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
        bool done = false;
        foreach (ViewDefinition definition in views)
        {
          if (definition.Name == value)
          {
            currentView = definition;
            CurrentLevel = 0;
            done = true;
            break;
          }
        }
        if (!done)
        {
          if (views.Count > 0)
          {
            currentView = (ViewDefinition)views[0];
          }
        }
      }
    }
    public int CurrentViewIndex
    {
      get { return views.IndexOf(currentView); }
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
      definition.SelectedValue = GetFieldValue(song, definition.Where).ToString();
      if (currentLevel + 1 < currentView.Filters.Count) currentLevel++;

    }
    
    public List<Song> Execute()
    {
      //build the query
      List<Song> songs = new List<Song>();
      string whereClause = String.Empty;
      string orderClause = String.Empty;
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];

      restrictionLength = 0;
      for (int i = 0; i < CurrentLevel; ++i)
      {
        BuildSelect((FilterDefinition)currentView.Filters[i], ref whereClause);
      }
      BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel], ref orderClause);

      if (CurrentLevel > 0)
      {
        whereClause = "where " + whereClause;
      }

      //execute the query
      string sql;
      if (CurrentLevel == 0)
      {
        bool useSongTable = false;
        bool useAlbumTable = false;
        bool useArtistTable = false;
        bool useAlbumArtistTable = false;
        bool useGenreTable = false;
        FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];
        string table = GetTable(defRoot.Where, ref useSongTable, ref useAlbumTable, ref useArtistTable, ref useAlbumArtistTable, ref useGenreTable);
        string searchField = "";

        // Handle the grouping of songs
        if (definition.SqlOperator == "group")
        {
          if (table == "artist")
          {
            searchField = "strArtist";
            useAlbumTable = false;
            useArtistTable = true;
            useAlbumArtistTable = false;
            useGenreTable = false;
            useSongTable = false;
          }
          else if (table == "albumartist")
          {
            searchField = "strAlbumArtist";
            useAlbumTable = false;
            useArtistTable = false;
            useAlbumArtistTable = true;
            useGenreTable = false;
            useSongTable = false;
          }
          else if (table == "genre")
          {
            searchField = "strGenre";
            useAlbumTable = false;
            useArtistTable = false;
            useAlbumArtistTable = false;
            useGenreTable = true;
            useSongTable = false;
          }
          else if (table == "tracks")
          {
            if (useAlbumTable)
            {
              searchField = "strAlbum";
              useAlbumTable = true;
              useSongTable = false;
            }
            else
            {
              searchField = "strTitle";
              useAlbumTable = false;
              useSongTable = true;
            }

            useArtistTable = false;
            useAlbumArtistTable = false;
            useGenreTable = false;
          }

          sql = String.Format("Select UPPER(SUBSTR({0},1,{1})) IX, Count(distinct {0}) from {2} GROUP BY IX", searchField, definition.Restriction, table);
          database.GetSongsByIndex(sql, out songs, CurrentLevel, useArtistTable, useAlbumTable, useAlbumArtistTable, useSongTable, useGenreTable);
          return songs;
        }

        if (table == "artist")
        {
          sql = String.Format("select * from artist ");
          if (whereClause != String.Empty) sql += "where " + whereClause;
          if (orderClause != String.Empty) sql += orderClause;
          database.GetSongsByFilter(sql, out songs, true, false, false, false, false);
        }
        else if (table == "albumartist")
        {
          sql = String.Format("select * from albumartist ");
          if (whereClause != String.Empty) sql += "where " + whereClause;
          if (orderClause != String.Empty) sql += orderClause;
          database.GetSongsByFilter(sql, out songs, false, true, false, false, false);
        }
        else if (table == "tracks" && useAlbumTable)  // Album was selected
        {
          sql = String.Format("select distinct strAlbum from tracks ");
          if (whereClause != String.Empty) sql += "where " + whereClause;
          if (orderClause != String.Empty) sql += orderClause;
          database.GetSongsByFilter(sql, out songs, false, false, false, false, true);
        }
        else if (table == "genre")
        {
          sql = String.Format("select * from genre ");
          if (whereClause != String.Empty) sql += "where " + whereClause;
          if (orderClause != String.Empty) sql += orderClause;
          database.GetSongsByFilter(sql, out songs, false, false, false, true, false);
        }
        else if (defRoot.Where == "year")
        {
          songs = new List<Song>();
          sql = String.Format("select distinct iYear from tracks ");
          SQLiteResultSet results = MusicDatabase.DirectExecute(sql);
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
          whereClause = "";
          BuildRestriction(defRoot, ref whereClause);
          sql = String.Format("select * from tracks {0} {1}",
            whereClause, orderClause);
          database.GetSongsByFilter(sql, out songs, false, false, true, false, false);
        }
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        bool isVariousArtistsAlbum = false;

        bool useSongTable = false;
        bool useAlbumTable = false;
        bool useArtistTable = false;
        bool useAlbumArtistTable = false;
        bool useGenreTable = false;
        FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
        string table = GetTable(defCurrent.Where, ref useSongTable, ref useAlbumTable, ref useArtistTable, ref useAlbumArtistTable, ref useGenreTable);

        if (defCurrent.SqlOperator == "group")
        {
          // get previous filter to find out the length of the substr search
          FilterDefinition defPrevious = (FilterDefinition)currentView.Filters[CurrentLevel - 1];
          int previousRestriction = 0;
          if (defPrevious.SqlOperator == "group")
            previousRestriction = Convert.ToInt16(defPrevious.Restriction);
          string field = GetField(defCurrent.Where);
          sql = String.Format("select UPPER(SUBSTR({0},1,{3})) IX, Count(distinct {0}), * from tracks {1} {2}",
                                            field, whereClause, orderClause, previousRestriction + Convert.ToInt16(defCurrent.Restriction));

          database.GetSongsByIndex(sql, out songs, CurrentLevel, useArtistTable, useAlbumTable, useAlbumArtistTable, useSongTable, useGenreTable);
        }
        else
        {
          if (useAlbumTable)
            whereClause += " group by strAlbum ";

          // Now we need to check the previous filters, if we were already on the tracks table previously
          // In this case the from clause must contain the tracks table only
          string from = String.Format("{1} from {0}", table, GetField(defCurrent.Where));
          for (int i = CurrentLevel - 1; i == 0; i--)
          {
            FilterDefinition filter = (FilterDefinition)currentView.Filters[i];
            if (filter.Where != table)
            {
              from = String.Format("{0} from tracks", GetField(defCurrent.Where));
              break;
            }
          }

          sql = String.Format("select distinct {0} {1} {2}",
                          from, whereClause, orderClause);

          database.GetSongsByFilter(sql, out songs, useArtistTable, useAlbumArtistTable, useSongTable, useGenreTable, useAlbumTable);
        }
      }
      else
      {
        sql = String.Format("select * from tracks {0} {1}",
          whereClause, orderClause);

        database.GetSongsByFilter(sql, out songs, false, false, true, false, false);
      }
      return songs;
    }

    void BuildSelect(FilterDefinition filter, ref string whereClause)
    {
      if (filter.SqlOperator == "group")
      {
        if (whereClause != "") whereClause += " and ";
        // Was the value selected a "#"? Then we have the group of special chars and need to search for values < A
        if (filter.SelectedValue == "#")
          whereClause += String.Format(" {0} < 'A'", GetField(filter.Where));
        else
        {
          restrictionLength += Convert.ToInt16(filter.Restriction);
          whereClause += String.Format(" ({0} like '{1}%' or {0} like '{2}%')", GetField(filter.Where), filter.SelectedValue.PadRight(restrictionLength), filter.SelectedValue);
        }
      }
      else
      {
        if (whereClause != "") whereClause += " and ";
        string selectedValue = filter.SelectedValue;
        Database.DatabaseUtility.RemoveInvalidChars(ref selectedValue);
        whereClause += String.Format(" {0} like '%{1}%'", GetField(filter.Where), selectedValue);
      }
    }

    void BuildRestriction(FilterDefinition filter, ref string whereClause)
    {
      if (filter.SqlOperator != String.Empty && filter.Restriction != String.Empty)
      {
        if (filter.SqlOperator == "group")
        {
          whereClause += " group by ix";
          return;
        }
        if (whereClause != "") 
          whereClause += " and ";
        else 
          whereClause = "where ";

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
        whereClause += String.Format(" {0} {1} '{2}'", GetField(filter.Where), filter.SqlOperator, restriction);
      }
    }

    void BuildWhere(FilterDefinition filter, ref string whereClause)
    {
      if (filter.WhereValue != "*")
      {
        if (whereClause != "") whereClause += " and ";
        string selectedValue = filter.WhereValue;
        Database.DatabaseUtility.RemoveInvalidChars(ref selectedValue);
        whereClause += String.Format(" {0} like '%{1}%'", GetField(filter.Where), selectedValue);
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

    string GetTable(string where, ref bool useSongTable, ref bool useAlbumTable, ref bool useArtistTable, ref bool useAlbumArtistTable, ref bool useGenreTable)
    {
      if (where == "album") { useSongTable = true; useAlbumTable = true; return "tracks"; }
      if (where == "artist") { useArtistTable = true; return "artist"; }
      if (where == "albumartist") { useAlbumArtistTable = true; return "albumartist"; }
      if (where == "title") { useSongTable = true; return "tracks"; }
      if (where == "genre") { useGenreTable = true; return "genre"; }
      if (where == "year") { useSongTable = true; return "tracks"; }
      if (where == "track") { useSongTable = true; return "tracks"; }
      if (where == "timesplayed") { useSongTable = true; return "tracks"; }
      if (where == "rating") { useSongTable = true; return "tracks"; }
      if (where == "favorites") { useSongTable = true; return "tracks"; }
      return null;
    }

    string GetField(string where)
    {
      if (where == "album") return "strAlbum";
      if (where == "artist") return "strArtist";
      if (where == "albumartist") return "strAlbumArtist";
      if (where == "title") return "strTitle";
      if (where == "genre") return "strGenre";
      if (where == "year") return "iYear";
      if (where == "track") return "iTrack";
      if (where == "timesplayed") return "iTimesPlayed";
      if (where == "rating") return "iRating";
      if (where == "favorites") return "iFavorite";
      return null;
    }

    string GetFieldValue(Song song, string where)
    {
      if (where == "album") return song.Album;
      if (where == "artist") return song.Artist;
      if (where == "albumartist") return song.AlbumArtist;
      if (where == "title") return song.Title;
      if (where == "genre") return song.Genre;
      if (where == "year") return song.Year.ToString();
      if (where == "track") return song.Track.ToString();
      if (where == "timesplayed") return song.TimesPlayed.ToString();
      if (where == "rating") return song.Rating.ToString();
      if (where == "favorites")
      {
        if (song.Favorite) return "1";
        return "0";
      }
      return "";
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
        // Don't clear the label in case of a Group/Index view, to show the counter
        if (definition.SqlOperator != "group")
        {
          item.Label2 = song.Artist;
        }
        item.Label3 = String.Empty;
      }
      if (definition.Where == "artist")
      {
        item.Label = song.Artist;
        // Don't clear the label in case of a Group/Index view, to show the counter
        if (definition.SqlOperator != "group")
        {
          item.Label2 = String.Empty;
        }
        item.Label3 = String.Empty;
      }
      if (definition.Where == "albumartist")
      {
        item.Label = song.AlbumArtist;
        // Don't clear the label in case of a Group/Index view, to show the counter
        if (definition.SqlOperator != "group")
        {
          item.Label2 = String.Empty;
        }
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