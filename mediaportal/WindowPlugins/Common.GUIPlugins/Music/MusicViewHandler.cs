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
  public class MusicViewHandler : ViewHandler
  {
    private readonly string defaultMusicViews = Path.Combine(DefaultsDirectory, "MusicViews.xml");
    private readonly string customMusicViews = Config.GetFile(Config.Dir.Config, "MusicViews.xml");

    private int previousLevel = 0;

    private MusicDatabase database;
    private int restrictionLength = 0; // used to sum up the length of all restrictions

    private Song currentSong = null; // holds the current Song selected in the list

    public int PreviousLevel
    {
      get { return previousLevel; }
    }

    public override string CurrentView
    {
      get
      {
        return base.CurrentView;
      }
      set
      {
        previousLevel = -1;
        base.CurrentView = value;
      }
    }

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

    public void Restore(ViewDefinition view, int level)
    {
      currentView = view;
      currentLevel = level;
    }

    public ViewDefinition GetView()
    {
      return currentView;
    }


    public void Select(Song song)
    {
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      definition.SelectedValue = GetFieldValue(song, definition.Where).ToString();
      if (currentLevel < currentView.Filters.Count)
      {
        currentLevel++;
      }
      currentSong = song;
    }

    public bool Execute(out List<Song> songs)
    {
      if (currentLevel < 0)
      {
        previousLevel = -1;
        songs = new List<Song>();
        return false;
      }

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
          // only group special characters into a "#" entry is field is text based
          if (defRoot.Where == "rating" || defRoot.Where == "year" || defRoot.Where == "track" || defRoot.Where == "disc#" ||
              defRoot.Where == "timesplayed" || defRoot.Where == "favourites" || defRoot.Where == "date")
          {
            database.GetSongsByFilter(sql, out songs, table);
          }
          else
          {
            database.GetSongsByIndex(sql, out songs, CurrentLevel, table);
          }

          previousLevel = currentLevel;
          
          return true;  

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

              previousLevel = currentLevel;

              return true;
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

        // Now we need to check the previous filters, if we were already on the tracks table previously
        // In this case the from clause must contain the tracks table only
        bool isUsingTrackTable = false;
        string allPrevColumns = string.Empty;
        for (int i = CurrentLevel; i > -1; i--)
        {
          FilterDefinition filter = (FilterDefinition)currentView.Filters[i];

          allPrevColumns += " " + GetField(filter.Where) + " ,";
          if (GetTable(filter.Where) != table)
          {
            isUsingTrackTable = true;
          }
        }
        allPrevColumns = allPrevColumns.Remove(allPrevColumns.Length - 1, 1); // remove extra trailing comma

        if (defCurrent.SqlOperator == "group")
        {
          // in an odd scenario here as we have a group operator
          // but not at the first level of view

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

          if (isUsingTrackTable && searchTable != "tracks")
          {
            // have the messy case where previous filters in view
            // do not use the same table as the current level
            // which means we can not just lookup values in search table

            string joinSQL;
            if (IsMultipleValueField(searchField))
            {
              joinSQL = string.Format("and tracks.{1} like '%| '||{0}.{1}||' |%' ",
                                      searchTable, searchField);
            }
            else
            {
              joinSQL = string.Format("and tracks.{1} = {0}.{1} ",
                                      searchTable, searchField);
            }

            whereClause = whereClause.Replace("group by ix", "");
            whereClause = string.Format(" where exists ( " +
                                        "    select 0 " +
                                        "    from tracks " +
                                        "    {0} " +
                                        "    {1} " +
                                        ") " +
                                        "group by ix "
                                        , whereClause, joinSQL);
          }

          sql = String.Format("select UPPER(SUBSTR({0},1,{1})) IX, Count(distinct {2}) from {3} {4} {5}",
                              searchField, Convert.ToInt16(defCurrent.Restriction), countField,
                              searchTable, whereClause, orderClause);
          database.GetSongsByIndex(sql, out songs, CurrentLevel, table);
        }
        else
        {
          string from = String.Format("{1} from {0}", table, GetField(defCurrent.Where));

          if (isUsingTrackTable && table != "album" && defCurrent.Where != "Disc#")
          {
            from = String.Format("{0} from tracks", allPrevColumns);
            table = "tracks";
          }

          // When searching for an album, we need to retrieve the AlbumArtist as well, because we could have same album names for different artists
          // We need also the Path to retrieve the coverart
          // We don't have an album table anymore, so change the table to search for to tracks here.

          for (int i = 0; i < currentLevel; i++)
          {
            // get previous filter to see, if we had an album that was not a group level
            FilterDefinition defPrevious = (FilterDefinition)currentView.Filters[i];
            if (defPrevious.Where == "album" && defPrevious.SqlOperator != "group")
            {
              if (whereClause != "")
              {
                whereClause += " and ";
              }

              string selectedArtist = currentSong.AlbumArtist;
              DatabaseUtility.RemoveInvalidChars(ref selectedArtist);

              // we don't store "unknown" into the datbase, so let's correct empty values
              if (selectedArtist == "unknown")
              {
                selectedArtist = string.Empty;
              }

              whereClause += String.Format("strAlbumArtist like '%| {0} |%'", selectedArtist);
              break;
            }
          }

          if (table == "album")
          {
            from = String.Format("* from tracks", GetField(defCurrent.Where));
            whereClause += " group by strAlbum, strAlbumArtist ";
          }
          if (defCurrent.Where == "disc#")
          {
            from = String.Format("* from tracks", GetField(defCurrent.Where));
            whereClause += " group by strAlbum, strAlbumArtist, iDisc ";
          }

          sql = String.Format("select distinct {0} {1} {2}", from, whereClause, orderClause);

          database.GetSongsByFilter(sql, out songs, table);
        }
      }
      else
      {
        for (int i = 0; i < currentLevel; i++)
        {
          // get previous filter to see, if we had an album that was not a group level
          FilterDefinition defPrevious = (FilterDefinition)currentView.Filters[i];
          if (defPrevious.Where == "album" && defPrevious.SqlOperator != "group")
          {
            if (whereClause != "")
            {
              whereClause += " and ";
            }

            string selectedArtist = currentSong.AlbumArtist;
            DatabaseUtility.RemoveInvalidChars(ref selectedArtist);

            // we don't store "unknown" into the datbase, so let's correct empty values
            if (selectedArtist == "unknown")
            {
              selectedArtist = string.Empty;
            }

            whereClause += String.Format("strAlbumArtist like '%| {0} |%'", selectedArtist);
            break;
          }
        }

        sql = String.Format("select * from tracks {0} {1}", whereClause, orderClause);

        database.GetSongsByFilter(sql, out songs, "tracks");
      }

      if (songs.Count == 1 && definition.SkipLevel)
      {
        if (currentLevel < MaxLevels - 1)
        {
          if (previousLevel < currentLevel)
          {
            FilterDefinition fd = (FilterDefinition)currentView.Filters[currentLevel];
            fd.SelectedValue = GetFieldValue(songs[0], fd.Where);
            currentLevel = currentLevel + 1;
          }
          else
          {
            currentLevel = currentLevel - 1;
          }
          if (!Execute(out songs))
          {
            return false;
          }
        }
      }

      previousLevel = currentLevel;

      return true;
    }

    private void BuildSelect(FilterDefinition filter, ref string whereClause, int filterLevel)
    {
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

        restrictionLength += Convert.ToInt16(filter.Restriction);

        // muliple value fields are stored in one database field in tracks
        // table but on different rows in other tables
        if (IsMultipleValueField(GetField(filter.Where)))
        {
          bool usingTracksTable = true;
          if (GetTable(CurrentLevelWhere) != "tracks")
          {
            usingTracksTable = false;
          }
          if (!usingTracksTable)
          {
            // current level is not using tracks table so check whether
            // any filters above this one are using a different table
            // and if so data will be taken from tracks table
            FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];
            string table = GetTable(defRoot.Where);
            for (int i = CurrentLevel; i > -1; i--)
            {
              FilterDefinition prevFilter = (FilterDefinition)currentView.Filters[i];
              if (GetTable(prevFilter.Where) != table)
              {
                usingTracksTable = true;
                break;
              }
            }
          }

          // now we know if we are using the tracks table or not we can
          // figure out how to format multiple value fields
          if (usingTracksTable)
          {
            if (filter.SelectedValue == "#")
            {
              // need a special case here were user selects # from grouped menu
              // as multiple values can be stored in the same field
              whereClause += String.Format(" exists ( " +
                                           "   select 0 from {0} " +
                                           "   where  {1} < 'A' " +
                                           "   and    tracks.{1} like '%| '||{0}.{1}||' |%' " +
                                           " ) ", GetTable(filter.Where), GetField(filter.Where));
            }
            else
            {
              whereClause += String.Format(" ({0} like '| {1}%' or '| {2}%')", GetField(filter.Where),
                                           filter.SelectedValue.PadRight(restrictionLength), filter.SelectedValue);
            }
          }
          else
          {
            if (filter.SelectedValue == "#")
            {
              whereClause += String.Format(" {0} < 'A'", GetField(filter.Where));
            }
            else
            {
              whereClause += String.Format(" ({0} like '{1}%' or '{2}%')", GetField(filter.Where),
                                           filter.SelectedValue.PadRight(restrictionLength), filter.SelectedValue);
            }
          }
        }
        else
        {
          // we are looking for fields which do not contain multiple values
          if (filter.SelectedValue == "#")
          {
            // deal with non standard characters
            whereClause += String.Format(" {0} < 'A'", GetField(filter.Where));
          }
          else
          {
            whereClause += String.Format(" ({0} like '{1}%' or '{2}%')", GetField(filter.Where),
                                         filter.SelectedValue.PadRight(restrictionLength), filter.SelectedValue);
          }
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
      string[] sortFields = GetSortField(filter).Split('|');
      orderClause = " order by ";
      for (int i = 0; i < sortFields.Length; i++)
      {
        if (i > 0)
        {
          orderClause += ", ";
        }
        orderClause += sortFields[i];
        if (!filter.SortAscending)
        {
          orderClause += " desc";
        }
        else
        {
          orderClause += " asc";
        }
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
    private static bool IsMultipleValueField(string field)
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

    private static string GetTable(string where)
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
      if (where == "disc#")
      {
        return "tracks";
      }
      return null;
    }

    private static string GetField(string where)
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
      if (where == "disc#")
      {
        return "iDisc";
      }
      return null;
    }

    public static string GetFieldValue(Song song, string where)
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
      if (where == "disc#")
      {
        return song.DiscId.ToString();
      }
      return "";
    }

    private static string GetSortField(FilterDefinition filter)
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
      if (filter.DefaultSort == "disc#")
      {
        return "iDisc";
      }
      if (filter.DefaultSort == "Track")
      {
        return "iDisc|iTrack"; // We need to sort on Discid + Track
      }


      return GetField(filter.Where);
    }

    protected override string GetLocalizedViewLevel(string lvlName)
    {
      string localizedLevelName = string.Empty;
      
      switch(lvlName)
      {          
        case "artist":
          localizedLevelName = GUILocalizeStrings.Get(133);
          break;
        case "albumartist":
          localizedLevelName = GUILocalizeStrings.Get(528);
          break;
        case "album":
          localizedLevelName = GUILocalizeStrings.Get(132);
          break;
        case "genre":
          localizedLevelName = GUILocalizeStrings.Get(135);
          break;
        case "year":
          localizedLevelName = GUILocalizeStrings.Get(987);
          break;
        case "composer":
          localizedLevelName = GUILocalizeStrings.Get(1214);
          break;          
        case "conductor":
          localizedLevelName = GUILocalizeStrings.Get(1215);
          break;          
        case"disc#":
          localizedLevelName = GUILocalizeStrings.Get(1216);
          break;          
        case "title":
        case "timesplayed":
        case "rating":
        case "favorites":
        case "recently added":
        case "track":
          localizedLevelName = GUILocalizeStrings.Get(1052);
          break;
        default:
          localizedLevelName = lvlName;
          break;
      }
      
      return localizedLevelName;
    }    
    
  }
}