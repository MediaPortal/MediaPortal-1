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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Music.Database;
using SQLite.NET;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for MusicViewHandler.
  /// </summary>
  public class MusicViewHandler
  {
    private string defaultMusicViews = Config.GetFile(Config.Dir.Base, "defaultMusicViews.xml");
    private string customMusicViews = Config.GetFile(Config.Dir.Config, "MusicViews.xml");

    private ViewDefinition currentView;
    private int currentLevel = 0;
    private List<ViewDefinition> views = new List<ViewDefinition>();

    private MusicDatabase database;
    private int restrictionLength = 0; // used to sum up the length of all restrictions

    private Song currentSong = null; // holds the current Song selected in the list

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
      catch (Exception) {}

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
        {
          return string.Empty;
        }
        return currentView.LocalizedName;
      }
    }

    public string CurrentView
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
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
        if (value < 0 || value >= currentView.Filters.Count)
        {
          return;
        }
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
      if (currentLevel + 1 < currentView.Filters.Count)
      {
        currentLevel++;
      }
      currentSong = song;
    }

    public List<Song> Execute()
    {
      //build the query
      List<Song> songs = new List<Song>();
      string whereClause = string.Empty;
      string orderClause = string.Empty;
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];

      restrictionLength = 0;
      for (int i = 0; i < CurrentLevel; ++i)
      {
        BuildSelect((FilterDefinition)currentView.Filters[i], ref whereClause, i);
      }
      BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel], ref orderClause);

      if (CurrentLevel > 0)
      {
        // When grouping is active, we need to omit the "where ";
        if (!whereClause.Trim().StartsWith("group ") && whereClause.Trim() != "")
        {
          whereClause = "where " + whereClause;
        }
      }

      //execute the query
      string sql = "";
      if (CurrentLevel == 0)
      {
        FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];
        string table = GetTable(defRoot.Where);
        string searchField = GetField(defRoot.Where);

        // Handle the grouping of songs
        if (definition.SqlOperator == "group")
        {
          string searchTable = table;
          string countField = searchField; // when grouping on Albums, we need to count the artists
          // We don't have an album table anymore, so change the table to search for to tracks here.
          if (table == "album")
          {
            searchTable = "tracks";
            countField = "strAlbumArtist";
          }

          sql = String.Format("Select UPPER(SUBSTR({0},1,{1})) as IX, Count(distinct {2}) from {3} GROUP BY IX",
                              searchField, definition.Restriction, countField, searchTable);
          database.GetSongsByIndex(sql, out songs, CurrentLevel, table);
          return songs;
        }

        switch (table)
        {
          case "artist":
          case "albumartist":
          case "genre":
          case "composer":
            sql = String.Format("select * from {0} ", table);
            if (whereClause != string.Empty)
            {
              sql += "where " + whereClause;
            }
            if (orderClause != string.Empty)
            {
              sql += orderClause;
            }
            break;

          case "album":
            sql = String.Format("select * from tracks ");
            if (whereClause != string.Empty)
            {
              sql += "where " + whereClause;
            }
            sql += " group by strAlbum, strAlbumArtist ";
              // We need to group on AlbumArtist, to show Albums with same name for different artists
            if (orderClause != string.Empty)
            {
              sql += orderClause;
            }
            break;

          case "tracks":
            if (defRoot.Where == "year")
            {
              songs = new List<Song>();
              sql = String.Format("select distinct iYear from tracks ");
              SQLiteResultSet results = MusicDatabase.DirectExecute(sql);
              for (int i = 0; i < results.Rows.Count; i++)
              {
                Song song = new Song();
                try
                {
                  song.Year = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "iYear")));
                }
                catch (Exception)
                {
                  song.Year = 0;
                }
                if (song.Year > 1000)
                {
                  songs.Add(song);
                }
              }
              return songs;
            }
            else if (defRoot.Where == "recently added")
            {
              try
              {
                whereClause = "";
                TimeSpan ts = new TimeSpan(Convert.ToInt32(defRoot.Restriction), 0, 0, 0);
                DateTime searchDate = DateTime.Today - ts;

                whereClause = String.Format("where {0} > '{1}'", searchField, searchDate.ToString("yyyy-MM-dd hh:mm:ss"));
                sql = String.Format("select * from tracks {0} {1}", whereClause, orderClause);
              }
              catch (Exception) {}
            }
            else if (defRoot.Where == "conductor")
            {
              whereClause = "";
              BuildRestriction(defRoot, ref whereClause);
              if (whereClause != string.Empty)
              {
                whereClause = String.Format("where {0}", whereClause);
              }
              sql = String.Format("select distinct strConductor from tracks {0} {1}", whereClause, orderClause);
            }
            else
            {
              whereClause = "";
              BuildRestriction(defRoot, ref whereClause);
              if (whereClause != string.Empty)
              {
                whereClause = String.Format("where {0}", whereClause);
              }
              sql = String.Format("select * from tracks {0} {1}", whereClause, orderClause);
            }
            break;
        }
        database.GetSongsByFilter(sql, out songs, table);
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
        string table = GetTable(defCurrent.Where);

        if (defCurrent.SqlOperator == "group")
        {
          // get previous filter to find out the length of the substr search
          FilterDefinition defPrevious = (FilterDefinition)currentView.Filters[CurrentLevel - 1];
          int previousRestriction = 0;
          if (defPrevious.SqlOperator == "group")
          {
            previousRestriction = Convert.ToInt16(defPrevious.Restriction);
          }

          // Build correct table for search
          string searchTable = GetTable(defCurrent.Where);
          string searchField = GetField(defCurrent.Where);
          string countField = searchField;
          // We don't have an album table anymore, so change the table to search for to tracks here.
          if (table == "album")
          {
            searchTable = "tracks";
            countField = "strAlbumArtist";
          }
          sql = String.Format("select UPPER(SUBSTR({0},1,{1})) IX, Count(distinct {2}), * from {3} {4} {5}",
                              searchField, previousRestriction + Convert.ToInt16(defCurrent.Restriction), countField,
                              searchTable, whereClause, orderClause);

          database.GetSongsByIndex(sql, out songs, CurrentLevel, table);
        }
        else
        {
          // Now we need to check the previous filters, if we were already on the tracks table previously
          // In this case the from clause must contain the tracks table only
          string from = String.Format("{1} from {0}", table, GetField(defCurrent.Where));
          for (int i = CurrentLevel; i > -1; i--)
          {
            FilterDefinition filter = (FilterDefinition)currentView.Filters[i];
            if (filter.Where != table)
            {
              from = String.Format("{0} from tracks", GetField(defCurrent.Where));
              break;
            }
          }

          // When searching for an album, we need to retrieve the AlbumArtist as well, because we could have same album names for different artists
          // We need also the Path to retrieve the coverart
          // We don't have an album table anymore, so change the table to search for to tracks here.
          if (table == "album")
          {
            from = String.Format("* from tracks", GetField(defCurrent.Where));
            whereClause += " group by strAlbum, strAlbumArtist ";
          }

          sql = String.Format("select distinct {0} {1} {2}", from, whereClause, orderClause);

          database.GetSongsByFilter(sql, out songs, table);
        }
      }
      else
      {
        // get previous filter to see, if we had an album 
        FilterDefinition defPrevious = (FilterDefinition)currentView.Filters[CurrentLevel - 1];
        if (defPrevious.Where == "album")
        {
          if (whereClause != "")
          {
            whereClause += " and ";
          }

          string selectedArtist = currentSong.AlbumArtist;
          DatabaseUtility.RemoveInvalidChars(ref selectedArtist);

          whereClause += String.Format("strAlbumArtist like '%| {0} |%'", selectedArtist);
        }

        sql = String.Format("select * from tracks {0} {1}", whereClause, orderClause);

        database.GetSongsByFilter(sql, out songs, "tracks");
      }
      return songs;
    }

    private void BuildSelect(FilterDefinition filter, ref string whereClause, int filterLevel)
    {
      // Clear the WhereClause, if the previous levels contained grouping. otherwise we get wrong results
      if (CurrentLevel != filterLevel)
      {
        if (filterLevel > 0)
        {
          FilterDefinition filterPrevious = (FilterDefinition)currentView.Filters[filterLevel - 1];
          if (filterPrevious.SqlOperator == "group")
          {
            whereClause = "";
          }
        }
      }

      if (filter.SqlOperator == "group")
      {
        // Don't need to include the grouping value, when it was on the first level
        if (CurrentLevel > 1 && filterLevel == 0)
        {
          return;
        }

        if (whereClause != "")
        {
          whereClause += " and ";
        }
        // Was the value selected a "#"? Then we have the group of special chars and need to search for values < A
        if (filter.SelectedValue == "#")
        {
          whereClause += String.Format(" {0} < 'A'", GetField(filter.Where));
        }
        else
        {
          restrictionLength += Convert.ToInt16(filter.Restriction);
          whereClause += String.Format(" ({0} like '{1}%' or '{2}%')", GetField(filter.Where),
                                       filter.SelectedValue.PadRight(restrictionLength), filter.SelectedValue);
        }
      }
      else
      {
        if (whereClause != "")
        {
          whereClause += " and ";
        }
        string selectedValue = filter.SelectedValue;
        DatabaseUtility.RemoveInvalidChars(ref selectedValue);

        // we don't store "unknown" into the datbase, so let's correct empty values
        if (selectedValue == "unknown")
        {
          selectedValue = "";
        }

        // If we have a multiple values field then we need to compare with like
        if (IsMultipleValueField(GetField(filter.Where)))
        {
          whereClause += String.Format("{0} like '%| {1} |%'", GetField(filter.Where), selectedValue);
        }
        else
        {
          // use like for case insensitivity
          whereClause += String.Format("{0} like '{1}'", GetField(filter.Where), selectedValue);
        }
      }
    }

    private void BuildRestriction(FilterDefinition filter, ref string whereClause)
    {
      if (filter.SqlOperator != string.Empty && filter.Restriction != string.Empty)
      {
        if (filter.SqlOperator == "group")
        {
          whereClause += " group by ix";
          return;
        }
        if (whereClause != "")
        {
          whereClause += " and ";
        }

        string restriction = filter.Restriction;
        restriction = restriction.Replace("*", "%");
        DatabaseUtility.RemoveInvalidChars(ref restriction);
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

    private void BuildWhere(FilterDefinition filter, ref string whereClause)
    {
      if (filter.WhereValue != "*")
      {
        if (whereClause != "")
        {
          whereClause += " and ";
        }
        string selectedValue = filter.WhereValue;
        DatabaseUtility.RemoveInvalidChars(ref selectedValue);

        // Do we have a Multiplevalues field, then we need compare with like
        if (IsMultipleValueField(GetField(filter.Where)))
        {
          whereClause += String.Format(" {0} like '%| {1} |%'", GetField(filter.Where), selectedValue);
        }
        else
        {
          // use like for case insensitivity
          whereClause += String.Format(" {0} like '{1}'", GetField(filter.Where), selectedValue);
        }
      }
    }

    private void BuildOrder(FilterDefinition filter, ref string orderClause)
    {
      orderClause = " order by " + GetSortField(filter) + " ";
      if (!filter.SortAscending)
      {
        orderClause += "desc";
      }
      else
      {
        orderClause += "asc";
      }
      if (filter.Limit > 0)
      {
        orderClause += String.Format(" Limit {0}", filter.Limit);
      }
    }

    /// <summary>
    /// Check, if this is a field with multiple values, for which we need to compare with Like %value% instead of equals
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private bool IsMultipleValueField(string field)
    {
      switch (field)
      {
        case "strArtist":
        case "strAlbumArtist":
        case "strGenre":
        case "strComposer":
          return true;

        default:
          return false;
      }
    }

    private string GetTable(string where)
    {
      if (where == "album")
      {
        return "album";
      }
      if (where == "artist")
      {
        return "artist";
      }
      if (where == "albumartist")
      {
        return "albumartist";
      }
      if (where == "title")
      {
        return "tracks";
      }
      if (where == "genre")
      {
        return "genre";
      }
      if (where == "composer")
      {
        return "composer";
      }
      if (where == "conductor")
      {
        return "tracks";
      }
      if (where == "year")
      {
        return "tracks";
      }
      if (where == "track")
      {
        return "tracks";
      }
      if (where == "timesplayed")
      {
        return "tracks";
      }
      if (where == "rating")
      {
        return "tracks";
      }
      if (where == "favorites")
      {
        return "tracks";
      }
      if (where == "recently added")
      {
        return "tracks";
      }
      return null;
    }

    private string GetField(string where)
    {
      if (where == "album")
      {
        return "strAlbum";
      }
      if (where == "artist")
      {
        return "strArtist";
      }
      if (where == "albumartist")
      {
        return "strAlbumArtist";
      }
      if (where == "title")
      {
        return "strTitle";
      }
      if (where == "genre")
      {
        return "strGenre";
      }
      if (where == "composer")
      {
        return "strComposer";
      }
      if (where == "conductor")
      {
        return "strConductor";
      }
      if (where == "year")
      {
        return "iYear";
      }
      if (where == "track")
      {
        return "iTrack";
      }
      if (where == "timesplayed")
      {
        return "iTimesPlayed";
      }
      if (where == "rating")
      {
        return "iRating";
      }
      if (where == "favorites")
      {
        return "iFavorite";
      }
      if (where == "recently added")
      {
        return "dateAdded";
      }
      if (where == "date")
      {
        return "dateAdded";
      }
      return null;
    }

    private string GetFieldValue(Song song, string where)
    {
      if (where == "album")
      {
        return song.Album;
      }
      if (where == "artist")
      {
        return song.Artist;
      }
      if (where == "albumartist")
      {
        return song.AlbumArtist;
      }
      if (where == "title")
      {
        return song.Title;
      }
      if (where == "genre")
      {
        return song.Genre;
      }
      if (where == "composer")
      {
        return song.Composer;
      }
      if (where == "conductor")
      {
        return song.Conductor;
      }
      if (where == "year")
      {
        return song.Year.ToString();
      }
      if (where == "track")
      {
        return song.Track.ToString();
      }
      if (where == "timesplayed")
      {
        return song.TimesPlayed.ToString();
      }
      if (where == "rating")
      {
        return song.Rating.ToString();
      }
      if (where == "favorites")
      {
        if (song.Favorite)
        {
          return "1";
        }
        return "0";
      }
      if (where == "recently added")
      {
        return song.DateTimeModified.ToShortDateString();
      }
      return "";
    }

    private string GetSortField(FilterDefinition filter)
    {
      // Don't allow anything else but the fieldnames itself on Multiple Fields
      if (filter.Where == "artist" || filter.Where == "albumartist" || filter.Where == "genre" ||
          filter.Where == "composer")
      {
        return GetField(filter.Where);
      }

      if (filter.DefaultSort == "Date")
      {
        return GetField("date");
      }
      if (filter.DefaultSort == "Year")
      {
        return GetField("year");
      }
      if (filter.DefaultSort == "Name")
      {
        return GetField("title");
      }
      if (filter.DefaultSort == "Duration")
      {
        return "iDuration";
      }

      return GetField(filter.Where);
    }

    public void SetLabel(Song song, ref GUIListItem item)
    {
      if (song == null)
      {
        return;
      }
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      if (definition.Where == "genre")
      {
        item.Label = song.Genre;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "album")
      {
        item.Label = song.Album;
        // Don't clear the label in case of a Group/Index view, to show the counter
        if (definition.SqlOperator != "group")
        {
          if (song.AlbumArtist != "")
          {
            item.Label2 = song.AlbumArtist; // Use the AlbumArtist if it's available
          }
          else
          {
            item.Label2 = song.Artist;
          }
        }
        item.Label3 = string.Empty;
      }
      if (definition.Where == "artist")
      {
        item.Label = song.Artist;
        // Don't clear the label in case of a Group/Index view, to show the counter
        if (definition.SqlOperator != "group")
        {
          item.Label2 = string.Empty;
        }
        item.Label3 = string.Empty;
      }
      if (definition.Where == "albumartist")
      {
        item.Label = song.AlbumArtist;
        // Don't clear the label in case of a Group/Index view, to show the counter
        if (definition.SqlOperator != "group")
        {
          item.Label2 = string.Empty;
        }
        item.Label3 = string.Empty;
      }
      if (definition.Where == "composer")
      {
        item.Label = song.Composer;
        // Don't clear the label in case of a Group/Index view, to show the counter
        if (definition.SqlOperator != "group")
        {
          item.Label2 = string.Empty;
        }
        item.Label3 = string.Empty;
      }
      if (definition.Where == "conductor")
      {
        item.Label = song.Conductor;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "year")
      {
        item.Label = song.Year.ToString();
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
    }
  }
}