#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Video.Database;
using SQLite.NET;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for VideoViewHandler.
  /// </summary>
  public class VideoViewHandler : ViewHandler
  {
    private readonly string defaultVideoViews = Path.Combine(DefaultsDirectory, "VideoViews.xml");
    private readonly string customVideoViews = Config.GetFile(Config.Dir.Config, "VideoViews.xml");

    private string _parentWhere = string.Empty;

    public string ParentWhere 
    {
      get { return _parentWhere; }
    }

    public VideoViewHandler()
    {
      if (!File.Exists(customVideoViews))
      {
        File.Copy(defaultVideoViews, customVideoViews);
      }

      try
      {
        using (FileStream fileStream = new FileInfo(customVideoViews).OpenRead())
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
    }

    public void Select(IMDBMovie movie)
    {
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      definition.SelectedValue = GetFieldIdValue(movie, definition.Where);
      if (currentLevel + 1 < currentView.Filters.Count)
      {
        currentLevel++;
      }
    }

    public ArrayList Execute()
    {
      //build the query
      ArrayList movies = new ArrayList();
      string whereClause = string.Empty;
      string orderClause = string.Empty;
      string fromClause = string.Empty;

      string defViewFields = VideoDatabase.DefaultVideoViewFields;

      for (int i = 0; i < CurrentLevel; ++i)
      {
        BuildSelect((FilterDefinition)currentView.Filters[i], ref whereClause, ref fromClause);
      }

      BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel], ref orderClause);

      _parentWhere = whereClause;

      //execute the query
      string sql;

      if ((CurrentLevel >= 0) && (CurrentLevel < MaxLevels))
      {
        bool useMovieInfoTable = false;
        bool useAlbumTable = false;
        bool useActorsTable = false;
        bool useGenreTable = false;
        bool useUserGroupsTable = false;
        bool useMovieCollectionTable = false;
        string join = string.Empty;
        string fields = defViewFields;

        FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
        string view = defCurrent.Where;

        // Actor, Director, Title Index
        if ((view == "actorindex") || (view == "directorindex") || (view == "titleindex"))
        {
          sql = String.Format("SELECT {0} AS IX, COUNT ({1}) " +
                              "FROM movieView " +
                              "WHERE {1} <> 'unknown' AND {1} IS NOT NULL {2} GROUP BY IX ",
                              GetFieldId(view), GetFieldName(view), (!string.IsNullOrEmpty(whereClause) ? "AND " + whereClause : ""));
          VideoDatabase.GetIndexByFilter(sql, true, out movies);
          return movies;
        }

        // Year
        if (view == "year")
        {
          sql = String.Format("SELECT DISTINCT {0} FROM movieView {1}",
                              GetFieldId(view), (!string.IsNullOrEmpty(whereClause) ? "WHERE " + whereClause : ""));
          SQLiteResultSet results = VideoDatabase.GetResults(sql);

          for (int i = 0; i < results.Rows.Count; i++)
          {
            IMDBMovie movie = new IMDBMovie();
            movie.Year = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "iYear")));
            movies.Add(movie);
          }
          return movies;
        }

        string table = GetTable(view, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                      ref useGenreTable, ref useUserGroupsTable, ref useMovieCollectionTable);

        // Recently added, Recently watched
        if ((view == "recently added") || (view == "recently watched"))
        {
          try
          {
            if (string.IsNullOrEmpty(defCurrent.Restriction))
            {
              defCurrent.Restriction = "7";
            }

            TimeSpan ts = new TimeSpan(Convert.ToInt32(defCurrent.Restriction), 0, 0, 0);
            DateTime searchDate = DateTime.Today - ts;

            whereClause = String.Format("WHERE {0} >= '{1}'",
                                        GetFieldName(view),
                                        searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));
            useMovieInfoTable = true;
          }
          catch (Exception) { }
        }
        // Director
        else if (view == "director")
        {
          fields = "idActorDirector, strActorDirector, strIMDBActorDirectorID";
          whereClause = "WHERE strActorDirector <> 'unknown' AND strActorDirector IS NOT NULL";
        }
        // Actor
        else if (view == "actor")
        {
          fields = "idActor, strActor, imdbActorId";
          whereClause = "WHERE strActor <> 'unknown' AND strActor IS NOT NULL";
        }
        // Genre
        else if (view == "genre")
        {
          fields = "idSingleGenre, strSingleGenre";
          whereClause = "WHERE strSingleGenre IS NOT NULL";
        }
        // User groups
        else if (view == "user groups" || view == "user groups only")
        {
          fields = "idGroup, strGroup";
          whereClause = "WHERE strGroup IS NOT NULL";
        }
        // Collections
        else if (view == "movie collections" || view == "movie collections only")
        {
          fields = "idCollection, strCollection, strCollectionDescription";
          whereClause = "WHERE strCollection IS NOT NULL";
        }
        // Title
        else
        {
          fields = defViewFields;
          whereClause = string.Empty; // Already storred in ParentWhere
          useMovieInfoTable = true;
        }

        table = "movieView"; // MP1-4775
        if (!string.IsNullOrEmpty(ParentWhere))
        {
          if (!string.IsNullOrEmpty(whereClause))
          {
            whereClause = whereClause + " AND " + ParentWhere;
          }
          else
          {
            whereClause = "WHERE " + ParentWhere;
          }
        }

        sql = String.Format("SELECT DISTINCT {0} FROM {1} {2} {3} {4}",
                            fields, table, join, whereClause, orderClause);
        VideoDatabase.GetMoviesByFilter(sql, out movies, useActorsTable, useMovieInfoTable, useGenreTable, useUserGroupsTable, useMovieCollectionTable);

        if ((view == "user groups") || (view == "movie collections"))
        {
          ArrayList moviesExt = new ArrayList();
          sql = String.Format("SELECT DISTINCT {0} FROM {1} WHERE {2} IS NULL {3} ORDER BY strTitle",
                              defViewFields, table, GetFieldId(view), (!string.IsNullOrEmpty(ParentWhere) ? "AND " + ParentWhere : ""));
          VideoDatabase.GetMoviesByFilter(sql, out moviesExt, false, true, false, false, false);
          movies.AddRange(moviesExt);
        }
      }
      return movies;
    }

    private void BuildSelect(FilterDefinition filter, ref string whereClause, ref string fromClause)
    {
      if (whereClause != "")
      {
        whereClause += " AND ";
      }
      
      string cleanValue = DatabaseUtility.RemoveInvalidChars(filter.SelectedValue);
      
      if (cleanValue == "#" && (filter.Where == "actorindex" || filter.Where == "directorindex" || filter.Where == "titleindex"))
      {
        string nWordChar = VideoDatabase.NonwordCharacters();
        whereClause += String.Format(" {0} IN ({1})", GetFieldId(filter.Where), nWordChar);
      }
      else
      {
        whereClause += String.Format(" {0}='{1}'", GetFieldId(filter.Where), cleanValue);
      }
      
      bool useMovieInfoTable = false;
      bool useAlbumTable = false;
      bool useActorsTable = false;
      bool useGenreTable = false;
      bool useUserGroupsTable = false;
      bool useMovieCollectionTable = false;
      string table = GetTable(filter.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                            ref useGenreTable, ref useUserGroupsTable, ref useMovieCollectionTable);
      if (useGenreTable)
      {
        whereClause += " AND idSingleGenre IS NOT NULL";
        return;
      }

      if (useUserGroupsTable)
      {
        whereClause += " AND idGroup IS NOT NULL";
        return;
      }

      if (useMovieCollectionTable)
      {
        whereClause += " AND idCollection IS NOT NULL";
        return;
      }

      if (useActorsTable)
      {
        // if ((CurrentLevel > 0) && (CurrentLevel == MaxLevels - 1))
        {
          whereClause += string.Format(" AND {0} <> 'unknown' AND {0} IS NOT NULL", GetFieldName(filter.Where));
        }
        return;
      }
    }

    private void BuildRestriction(FilterDefinition filter, ref string whereClause)
    {
      if (filter.SqlOperator != string.Empty && filter.Restriction != string.Empty)
      {
        if (whereClause != "")
        {
          whereClause += " AND ";
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
            filter.SqlOperator = "LIKE";
          }
        }
        
        whereClause += String.Format(" {0} {1} '{2}'", GetFieldName(filter.Where), filter.SqlOperator, restriction);
      }
    }

    private void BuildWhere(FilterDefinition filter, ref string whereClause)
    {
      if (filter.WhereValue != "*")
      {
        if (whereClause != "")
        {
          whereClause += " AND ";
        }
        whereClause += String.Format(" {0}='{1}'", GetFieldName(filter.Where), filter.WhereValue);
      }
    }

    private void BuildOrder(FilterDefinition filter, ref string orderClause)
    {
      orderClause = " ORDER BY " + GetFieldName(filter.Where) + " ";
      
      if (!filter.SortAscending)
      {
        orderClause += "DESC";
      }
      else
      {
        orderClause += "ASC";
      }
      
      if (filter.Limit > 0)
      {
        orderClause += String.Format(" LIMIT {0}", filter.Limit);
      }
    }

    private string GetTable(string where, ref bool useMovieInfoTable, ref bool useAlbumTable, ref bool useActorsTable,
                            ref bool useGenreTable, ref bool useUserGroupsTable, ref bool useMovieCollectionTable)
    {
      if (where == "actor")
      {
        useActorsTable = true;
        return "actors";
      }
      if (where == "director")
      {
        useActorsTable = true;
        return "actors";
      }
      if (where == "title")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "genre")
      {
        useGenreTable = true;
        return "genre";
      }
      if (where == "user groups")
      {
        useUserGroupsTable = true;
        return "usergroup";
      }
      if (where == "user groups only")
      {
        useUserGroupsTable = true;
        return "usergrouponly";
      }
      if (where == "movie collections")
      {
        useMovieCollectionTable = true;
        return "moviecollection";
      }
      if (where == "movie collections only")
      {
        useMovieCollectionTable = true;
        return "moviecollectiononly";
      }
      if (where == "year")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "rating")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "recently added")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      if (where == "recently watched")
      {
        useMovieInfoTable = true;
        return "movieinfo";
      }
      return null;
    }

    private string GetFieldId(string where)
    {
      if (where == "watched")
      {
        return "idMovie";
      }
      if (where == "actor")
      {
        return "idActor";
      }
      if (where == "director")
      {
        return "idActorDirector";
      }
      if (where == "title")
      {
        return "idMovie";
      }
      if (where == "genre")
      {
        return "idSingleGenre";
      }
      if (where == "user groups" || where == "user groups only")
      {
        return "idGroup";
      }
      if (where == "movie collections" || where == "movie collections only")
      {
        return "idCollection";
      }
      if (where == "year")
      {
        return "iYear";
      }
      if (where == "rating")
      {
        return "fRating";
      }
      if (where == "recently added")
      {
        return "idMovie";
      }
      if (where == "recently watched")
      {
        return "idMovie";
      }
      if (where == "actorindex" || where == "directorindex" || where == "titleindex") 
      {
        return string.Format("UPPER(SUBSTR({0},1,1))", GetFieldName(where));
      }
      return null;
    }

    private string GetFieldName(string where)
    {
      if (where == "watched")
      {
        return "iswatched";
      }
      if (where == "actor" || where == "actorindex")
      {
        return "strActor";
      }
      if (where == "director" || where == "directorindex")
      {
        return "strActorDirector";
      }
      if (where == "title" || where == "titleindex")
      {
        return "strTitle";
      }
      if (where == "genre")
      {
        return "strSingleGenre";
      }
      if (where == "user groups" || where == "user groups only")
      {
        return "strGroup";
      }
      if (where == "movie collections" || where == "movie collections only")
      {
        return "strCollection";
      }
      if (where == "year")
      {
        return "iYear";
      }
      if (where == "rating")
      {
        return "fRating";
      }
      if (where == "recently added")
      {
        return "dateAdded";
      }
      if (where == "recently watched")
      {
        return "dateWatched";
      }
      return null;
    }

    private string GetFieldIdValue(IMDBMovie movie, string where)
    {
      if (where == "watched")
      {
        return movie.Watched.ToString();
      }
      if (where == "actor")
      {
        return movie.ActorID.ToString();
      }
      if (where == "director")
      {
        return movie.ActorID.ToString();
      }
      if (where == "title")
      {
        return movie.ID.ToString();
      }
      if (where == "genre")
      {
        return movie.GenreID.ToString();
      }
      if (where == "user groups" || where == "user groups only")
      {
        return movie.UserGroupID.ToString();
      }
      if (where == "movie collections" || where == "movie collections only")
      {
        return movie.MovieCollectionID.ToString();
      }
      if (where == "year")
      {
        return movie.Year.ToString();
      }
      if (where == "rating")
      {
        return movie.Rating.ToString();
      }
      if (where == "recently added")
      {
        return movie.ID.ToString();
      }
      if (where == "recently watched")
      {
        return movie.ID.ToString();
      }
      if (where == "actorindex" || where == "directorindex" || where == "titleindex")
      {
        return movie.Title;
      }
      return "";
    }

    public void SetLabel(IMDBMovie movie, ref GUIListItem item)
    {
      if (movie == null)
      {
        return;
      }
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      if (definition.Where == "genre")
      {
        item.Label = movie.SingleGenre;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "user groups" || definition.Where == "user groups only")
      {
        if (movie.Title == string.Empty)
        {
          item.Label = movie.SingleUserGroup;
          item.Label2 = string.Empty;
          item.Label3 = string.Empty;
        }
      }
      if (definition.Where == "movie collections" || definition.Where == "movie collections only")
      {
        if (movie.Title == string.Empty)
        {
          item.Label = movie.SingleMovieCollection;
          item.Label2 = string.Empty;
          item.Label3 = string.Empty;
        }
      }
      if (definition.Where == "actor")
      {
        item.Label = movie.Actor;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "director")
      {
        item.Label = movie.Actor;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "year")
      {
        item.Label = movie.Year.ToString();
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "actorindex" || definition.Where == "directorindex" || definition.Where == "titleindex" )
      {
        item.Label = movie.Title;
        item.Label2 = string.Empty;
        item.Label3 = "#" + movie.RunTime.ToString();
      }
      if (definition.Where == "recently added")
      {
        item.Label = movie.Title;
        item.Label2 = movie.DateAdded;
        //Convert.ToDateTime(movie.DateAdded).ToShortDateString() + " " +
        //Convert.ToDateTime(movie.DateAdded).ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        //item.Label3 = string.Empty;          // Watched percentage is here
      }
      if (definition.Where == "recently watched")
      {
        item.Label = movie.Title;
        item.Label2 = movie.DateWatched;
        //Convert.ToDateTime(movie.DateWatched).ToShortDateString() + " " +
        //Convert.ToDateTime(movie.DateWatched).ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        //item.Label3 = string.Empty;          // Watched percentage is here
      }
    }

    protected override string GetLocalizedViewLevel(string lvlName)
    {
      string localizedLevelName = string.Empty;
      
      switch(lvlName)
      {          
        case "actor":
          localizedLevelName = GUILocalizeStrings.Get(344);
          break;
        case "genre":
          localizedLevelName = GUILocalizeStrings.Get(135);
          break;
        case "year":
          localizedLevelName = GUILocalizeStrings.Get(987);
          break;      
        case "watched":
        case "unwatched":
        case "title":
        case "rating":
        case "recently added":
        case "recently watched":
          localizedLevelName = GUILocalizeStrings.Get(342);
          break;
        case "actorindex":
          localizedLevelName = GUILocalizeStrings.Get(1288);
          break;
        case "directorindex":
          localizedLevelName = GUILocalizeStrings.Get(1289);
          break;
        case "titleindex":
          localizedLevelName = GUILocalizeStrings.Get(1287);
          break;
        case "user groups":
          localizedLevelName = GUILocalizeStrings.Get(1265);
          break;
        case "user groups only":
          localizedLevelName = GUILocalizeStrings.Get(1330);
          break;
        case "movie collections":
          localizedLevelName = GUILocalizeStrings.Get(1331);
          break;
        case "movie collections only":
          localizedLevelName = GUILocalizeStrings.Get(1332);
          break;
        default:
          localizedLevelName = lvlName;
          break;
      }
      
      return localizedLevelName;
    } 

  }
}