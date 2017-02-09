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

      string defViewFields = "idMovie, idDirector, strPlotOutline, strPlot, strTagLine, strVotes, fRating, strCast, " +
                             "strCredits, iYear, strGenre, strPictureURL, strTitle, IMDBID, mpaa, runtime, iswatched, " + 
                             "strUserReview, strFanartURL, strDirector, dateAdded, dateWatched, studios, country, " + 
                             "language, lastupdate, strSortTitle, TMDBNumber, LocalDBNumber, iUserRating, " +
                             "discid, strPath, cdlabel";
      
      for (int i = 0; i < CurrentLevel; ++i)
      {
        BuildSelect((FilterDefinition)currentView.Filters[i], ref whereClause, ref fromClause);
      }
      
      BuildWhere((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildRestriction((FilterDefinition)currentView.Filters[CurrentLevel], ref whereClause);
      BuildOrder((FilterDefinition)currentView.Filters[CurrentLevel], ref orderClause);

      //execute the query
      string sql;
      
      if (CurrentLevel == 0)
      {
        bool useMovieInfoTable = false;
        bool useAlbumTable = false;
        bool useActorsTable = false;
        bool useGenreTable = false;
        bool useUserGroupsTable = false;
        bool useMovieCollectionTable = false;

        FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];

        string table = GetTable(defRoot.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                ref useGenreTable, ref useUserGroupsTable, ref useMovieCollectionTable);

        if (string.IsNullOrEmpty(table) && defRoot.Where == "actorindex")
        {
          sql = String.Format("SELECT UPPER(SUBSTR(strActor,1,1)) AS IX, COUNT (strActor) " + 
                              "FROM movieView " + 
                              "WHERE strActor <> 'unknown' AND strActor IS NOT NULL GROUP BY IX ");
          VideoDatabase.GetIndexByFilter(sql, true, out movies);
        }
        else if (string.IsNullOrEmpty(table) && defRoot.Where == "directorindex")
        {
          sql = String.Format("SELECT UPPER(SUBSTR(strActorDirector,1,1)) AS IX, COUNT (strActorDirector) " + 
                              "FROM movieView " + 
                              "WHERE strActorDirector <> 'unknown' AND strActorDirector IS NOT NULL GROUP BY IX ");
          VideoDatabase.GetIndexByFilter(sql, true, out movies);
        }
        else if (string.IsNullOrEmpty(table) && defRoot.Where == "titleindex")
        {
          sql = String.Format("SELECT UPPER(SUBSTR(strTitle,1,1)) AS IX, COUNT (strTitle) FROM movieView GROUP BY IX ");
          VideoDatabase.GetIndexByFilter(sql, true, out movies);
        }
        else if (table == "actors")
        {
          if (defRoot.Where == "director")
          {
            sql = String.Format("SELECT DISTINCT idActorDirector, strActorDirector, strIMDBActorDirectorID FROM movieView WHERE strActorDirector <> 'unknown' AND strActorDirector IS NOT NULL ");
          }
          else
          {
            sql = String.Format("SELECT DISTINCT idActor, strActor, IMDBActorID FROM movieView WHERE strActor <> 'unknown' AND strActor IS NOT NULL ");
          }

          if(whereClause != string.Empty)
          {
            sql += "AND " + whereClause;
          }

          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, true, false, false, false, false);
        }
        else if (table == "genre")
        {
          sql = String.Format("SELECT DISTINCT idSingleGenre, strSingleGenre FROM movieView WHERE strSingleGenre IS NOT NULL ");
          if (whereClause != string.Empty)
          {
            sql += "AND " + whereClause;
          }
          
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, false, true, false, false);
        }
        else if (table == "usergroup" || table == "usergrouponly")
        {
          sql = String.Format("SELECT DISTINCT idGroup, strGroup FROM movieView WHERE strGroup IS NOT NULL ");
          
          if (whereClause != string.Empty)
          {
            sql += "AND " + whereClause;
          }
          
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, false, false, true, false);

          if (table == "usergroup")
          {
            ArrayList moviesExt = new ArrayList();
            sql = String.Format("SELECT DISTINCT {0} " + 
                                "FROM movieView " +
                                "WHERE idGroup IS NULL ORDER BY strTitle", defViewFields);
            VideoDatabase.GetMoviesByFilter(sql, out moviesExt, false, true, false, false, false);
            movies.AddRange(moviesExt);
          }
        }
        else if (table == "moviecollection" || table == "moviecollectiononly")
        {
          sql = String.Format("SELECT DISTINCT idCollection, strCollection FROM movieView WHERE strCollection IS NOT NULL ");
          
          if (whereClause != string.Empty)
          {
            sql += "AND " + whereClause;
          }
          
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, false, false, false, true);

          if (table == "moviecollection")
          {
            ArrayList moviesExt = new ArrayList();
            sql = String.Format("SELECT DISTINCT {0} " + 
                                "FROM movieView " +
                                "WHERE idCollection IS NULL ORDER BY strTitle", defViewFields);
            VideoDatabase.GetMoviesByFilter(sql, out moviesExt, false, true, false, false, false);
            movies.AddRange(moviesExt);
          }
        }
        else if (defRoot.Where == "year")
        {
          movies = new ArrayList();
          sql = String.Format("SELECT DISTINCT iYear FROM movieView");

          SQLiteResultSet results = VideoDatabase.GetResults(sql);

          for (int i = 0; i < results.Rows.Count; i++)
          {
            IMDBMovie movie = new IMDBMovie();
            movie.Year = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "iYear")));
            movies.Add(movie);
          }
        }
        // Recently added
        else if (defRoot.Where == "recently added")
        {
          try
          {
            if (string.IsNullOrEmpty(defRoot.Restriction))
              defRoot.Restriction = "7";

            TimeSpan ts = new TimeSpan(Convert.ToInt32(defRoot.Restriction), 0, 0, 0);
            DateTime searchDate = DateTime.Today - ts;

            whereClause = String.Format("WHERE dateAdded >= '{0}'",
                                        searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));
            sql = String.Format("SELECT DISTINCT {0} " + 
                                "FROM movieView {1} {2}", defViewFields, whereClause, orderClause);

            VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false, false, false);
          }
          catch (Exception) { }
        }
        // Recently watched
        else if (defRoot.Where == "recently watched")
        {
          try
          {
            if (string.IsNullOrEmpty(defRoot.Restriction))
              defRoot.Restriction = "7";

            TimeSpan ts = new TimeSpan(Convert.ToInt32(defRoot.Restriction), 0, 0, 0);
            DateTime searchDate = DateTime.Today - ts;

            whereClause = String.Format("WHERE dateWatched >= '{0}'",
                                        searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));

            sql = String.Format("SELECT DISTINCT {0} " + 
                                "FROM movieView {1} {2}", defViewFields, whereClause, orderClause);

            VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false, false, false);
          }
          catch (Exception) { }
        }
        else
        {
          whereClause = string.Empty;

          BuildRestriction(defRoot, ref whereClause);

          if (whereClause != string.Empty) // MP1-4775
          {
            whereClause = "WHERE " + whereClause;
          }

          sql = String.Format("SELECT DISTINCT {0} " + 
                              "FROM movieView {1} {2}", defViewFields, whereClause, orderClause);

          VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, true, true, true);
        }
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        bool useMovieInfoTable = false;
        bool useAlbumTable = false;
        bool useActorsTable = false;
        bool useGenreTable = false;
        bool useUserGroupsTable = false;
        bool useMovieCollectionTable = false;
        string join = string.Empty;
        string fields = "*";
        string _whereClause = whereClause;
        
        FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
        
        string table = GetTable(defCurrent.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                ref useGenreTable, ref useUserGroupsTable, ref useMovieCollectionTable);
        
        if (table == "usergrouponly")
        {
          table = "usergroup";
        }
        if (table == "moviecollectiononly")
        {
          table = "moviecollection";
        }

        if (defCurrent.Where == "director")
        {
          fields = "idActorDirector, strActorDirector, strIMDBActorDirectorID";
          whereClause = "WHERE strActorDirector <> 'unknown' AND strActorDirector IS NOT NULL " + (!string.IsNullOrEmpty(whereClause) ? "AND " + whereClause : "");
        }
        
        if (defCurrent.Where == "actor")
        {
          fields = "idActor, strActor, imdbActorId" ;
          whereClause = "WHERE strActor <> 'unknown' AND strActor IS NOT NULL " + (!string.IsNullOrEmpty(whereClause) ? "AND " + whereClause : "");
        }
       
        if (defCurrent.Where == "genre")
        {
          fields = "idSingleGenre, strSingleGenre";
          whereClause = "WHERE strSingleGenre IS NOT NULL " + (!string.IsNullOrEmpty(whereClause) ? "AND " + whereClause : "");
        }

        if (defCurrent.Where == "user groups" || defCurrent.Where == "user groups only")
        {
          fields = "idGroup, strGroup";
          whereClause = "WHERE strGroup IS NOT NULL " + (!string.IsNullOrEmpty(whereClause) ? "AND " + whereClause : "");
        }

        if (defCurrent.Where == "movie collections" || defCurrent.Where == "movie collections only")
        {
          fields = "idCollection, strCollection, strCollectionDescription";
          whereClause = "WHERE strCollection IS NOT NULL " + (!string.IsNullOrEmpty(whereClause) ? "AND " + whereClause : "");
        }

        table = "movieView"; // MP1-4775

        sql = String.Format("SELECT DISTINCT {0} FROM {1} {2} {3} {4}",
                            fields, table, join, whereClause, orderClause);
        VideoDatabase.GetMoviesByFilter(sql, out movies, useActorsTable, useMovieInfoTable, useGenreTable, useUserGroupsTable, useMovieCollectionTable);

        if (defCurrent.Where == "user groups")
        {
          ArrayList moviesExt = new ArrayList();
          sql = String.Format("SELECT DISTINCT {0} " + 
                              "FROM movieView " +
                              "WHERE idGroup IS NULL {1} ORDER BY strTitle", defViewFields, (!string.IsNullOrEmpty(_whereClause) ? "AND " + _whereClause : ""));
          VideoDatabase.GetMoviesByFilter(sql, out moviesExt, false, true, false, false, false);
          movies.AddRange(moviesExt);
        }
        if (defCurrent.Where == "movie collections")
        {
          ArrayList moviesExt = new ArrayList();
          sql = String.Format("SELECT DISTINCT {0} " + 
                              "FROM movieView " +
                              "WHERE idCollection IS NULL {1} ORDER BY strTitle", defViewFields, (!string.IsNullOrEmpty(_whereClause) ? "AND " + _whereClause : ""));
          VideoDatabase.GetMoviesByFilter(sql, out moviesExt, false, true, false, false, false);
          movies.AddRange(moviesExt);
        }
      }
      else
      {
        if (whereClause != string.Empty)
        {
          whereClause = "WHERE " + whereClause;
        }
        sql = String.Format("SELECT DISTINCT {0} " + 
                            "FROM movieView {1} {2}", defViewFields, whereClause, orderClause);
        VideoDatabase.GetMoviesByFilter(sql, out movies, true, true, true, true, true);
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
      
      if (filter.Where == "actorindex" || filter.Where == "directorindex" || filter.Where == "titleindex")
      {
        if (cleanValue == "#")
        {
          string nWordChar = VideoDatabase.NonwordCharacters();
          
          if (filter.Where == "actorindex")
          {
            whereClause += @" SUBSTR(strActor,1,1) IN (" + nWordChar +")";
          }
          else if (filter.Where == "directorindex")
          {
            whereClause += @" SUBSTR(strActorDirector,1,1) IN (" + nWordChar +")";
          }
          else
          {
            whereClause += @" SUBSTR(strTitle,1,1) IN (" + nWordChar +")";
          }
        }
        else
        {
          whereClause += String.Format(" {0}='{1}'", GetFieldId(filter.Where), cleanValue);
        }
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
        // fromClause += String.Format(",genre,genrelinkmovie");
        whereClause += " AND idSingleGenre IS NOT NULL";
        return;
      }

      if (useUserGroupsTable)
      {
        // fromClause += String.Format(",usergroup,usergrouplinkmovie");
        whereClause += " AND idGroup IS NOT NULL";
        return;
      }

      if (useMovieCollectionTable)
      {
        // fromClause += String.Format(",moviecollection,moviecollectionlinkmovie");
        whereClause += " AND idCollection IS NOT NULL";
        return;
      }

      if (useActorsTable)
      {
        if (CurrentLevel == MaxLevels - 1)
        {
          if (filter.Where == "director")
          {
            whereClause += " AND strActorDirector <> 'unknown' AND strActorDirector IS NOT NULL";
          }
          else if (filter.Where == "actor")
          {
            whereClause += " AND strActor <> 'unknown' AND strActor IS NOT NULL";
          }
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
        whereClause += String.Format(" {0}='{1}'", GetField(filter.Where), filter.WhereValue);
      }
    }

    private void BuildOrder(FilterDefinition filter, ref string orderClause)
    {
      orderClause = " ORDER BY " + GetField(filter.Where) + " ";
      
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

    private string GetField(string where)
    {
      if (where == "watched")
      {
        return "iswatched";
      }
      if (where == "actor")
      {
        return "strActor";
      }
      if (where == "director")
      {
        return "strActorDirector";
      }
      if (where == "title")
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
      if (where == "actorindex")
      {
        return "SUBSTR(strActor,1,1)";
      }
      if (where == "directorindex")
      {
        return "SUBSTR(strActorDirector,1,1)";
      }
      if (where == "titleindex")
      {
        return "SUBSTR(strTitle,1,1)";
      }
      return null;
    }

    private string GetFieldName(string where)
    {
      if (where == "watched")
      {
        return "iswatched";
      }
      if (where == "actor")
      {
        return "strActor";
      }
      if (where == "director")
      {
        return "strActorDirector";
      }
      if (where == "title")
      {
        return "strTitle";
      }
      if (where == "genre")
      {
        return "strGenre";
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