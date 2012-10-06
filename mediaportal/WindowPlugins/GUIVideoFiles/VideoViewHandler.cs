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
      
      if (CurrentLevel == MaxLevels - 1)
      {
        whereClause = "WHERE movieinfo.idmovie=movie.idmovie AND movie.idpath=path.idpath";
        fromClause = "movie,movieinfo,path";
      }

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
        FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];
        string table = GetTable(defRoot.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                ref useGenreTable, ref useUserGroupsTable);

        if (string.IsNullOrEmpty(table) && defRoot.Where == "actorindex")
        {
          sql = String.Format("SELECT UPPER(SUBSTR(strActor,1,1)) AS IX, COUNT (strActor) FROM actors WHERE idActor NOT IN (SELECT DISTINCT idDirector FROM movieinfo WHERE strDirector <> 'unknown') AND strActor <> 'unknown' GROUP BY IX ");
          VideoDatabase.GetIndexByFilter(sql, true, out movies);
        }
        else if (string.IsNullOrEmpty(table) && defRoot.Where == "directorindex")
        {
          sql = String.Format("SELECT UPPER(SUBSTR(strActor,1,1)) AS IX, COUNT (strActor) FROM actors WHERE idActor IN (SELECT DISTINCT idDirector FROM movieinfo WHERE strDirector <> 'unknown') GROUP BY IX ");
          VideoDatabase.GetIndexByFilter(sql, true, out movies);
        }
        else if (string.IsNullOrEmpty(table) && defRoot.Where == "titleindex")
        {
          sql = String.Format("SELECT UPPER(SUBSTR(strTitle,1,1)) AS IX, COUNT (strTitle) FROM movieinfo GROUP BY IX ");
          VideoDatabase.GetIndexByFilter(sql, true, out movies);
        }
        else if (table == "actors")
        {
          if (defRoot.Where == "director")
          {
            sql = String.Format("SELECT idActor, strActor, imdbActorId FROM actors WHERE idActor IN (SELECT DISTINCT idDirector FROM movieinfo WHERE strDirector <> 'unknown') AND strActor <> 'unknown' ");
          }
          else
          {
            sql = String.Format("SELECT * FROM actors WHERE strActor <> 'unknown' ");
          }

          if (whereClause != string.Empty && defRoot.Where == "director")
          {
            sql += "WHERE " + whereClause;
          }
          
          if(whereClause != string.Empty && defRoot.Where == "actor")
          {
            sql += "AND " + whereClause;
          }
          
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, true, false, false, false);
        }
        else if (table == "genre")
        {
          sql = String.Format("SELECT * FROM genre ");
          if (whereClause != string.Empty)
          {
            sql += "WHERE " + whereClause;
          }
          
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, false, true, false);
        }
        else if (table == "usergroup")
        {
          sql = String.Format("SELECT * FROM usergroup ");
          
          if (whereClause != string.Empty)
          {
            sql += "WHERE " + whereClause;
          }
          
          if (orderClause != string.Empty)
          {
            sql += orderClause;
          }
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, false, false, true);

          ArrayList moviesExt = new ArrayList();
          sql = String.Format("SELECT * FROM movieinfo WHERE idMovie NOT IN (SELECT DISTINCT idMovie FROM usergrouplinkmovie) ORDER BY strTitle");
          VideoDatabase.GetMoviesByFilter(sql, out moviesExt, false, true, false, false);
          movies.AddRange(moviesExt);
        }
        else if (defRoot.Where == "year")
        {
          movies = new ArrayList();
          sql = String.Format("SELECT DISTINCT iYear FROM movieinfo ");

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

            whereClause = String.Format("WHERE movieinfo.dateAdded >= '{0}'",
                                        searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));
            sql = String.Format("SELECT * FROM movieinfo {0} {1}", whereClause, orderClause);

            VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false, false);
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

            whereClause = String.Format("WHERE movieinfo.dateWatched >= '{0}'",
                                        searchDate.ToString("yyyy-MM-dd" + " 00:00:00"));

            sql = String.Format("SELECT * FROM movieinfo {0} {1}", whereClause, orderClause);

            VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false, false);
          }
          catch (Exception) { }
        }
        else
        {
          whereClause =
            "WHERE movieinfo.idmovie=movie.idmovie AND movie.idpath=path.idpath";

          BuildRestriction(defRoot, ref whereClause);

          sql = String.Format("SELECT * FROM {0} {1} {2}",
                              fromClause, whereClause, orderClause);

          VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, true, true);
        }
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        bool useMovieInfoTable = false;
        bool useAlbumTable = false;
        bool useActorsTable = false;
        bool useGenreTable = false;
        bool useUserGroupsTable = false;
        string join = string.Empty;
        string fields = "*";
        
        FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
        
        string table = GetTable(defCurrent.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                                ref useGenreTable, ref useUserGroupsTable);

        if (defCurrent.Where == "director")
        {
          fields = "idActor, strActor, imdbActorId";
          join = "INNER JOIN movieinfo ON movieinfo.idDirector = actors.idActor";
        }
        
        if (whereClause != string.Empty)
        {
          if (!whereClause.ToUpperInvariant().Trim().StartsWith("WHERE"))
          {
            whereClause = "WHERE" + whereClause;
          }
        }

        if (defCurrent.Where == "actor")
        {
          if (whereClause != string.Empty)
          {
            whereClause = whereClause + " AND idActor NOT IN (SELECT idDirector FROM movieinfo)";
          }
          else
          {
            whereClause = " WHERE idActor NOT IN (SELECT idDirector FROM movieinfo)";
          }
        }
       
        sql = String.Format("SELECT DISTINCT {0} FROM {1} {2} {3} {4}",
                            fields, table, join, whereClause, orderClause);
        VideoDatabase.GetMoviesByFilter(sql, out movies, useActorsTable, useMovieInfoTable, useGenreTable, useUserGroupsTable);
      }
      else
      {
        sql =
          String.Format(
            "SELECT DISTINCT movieinfo.idMovie, " + 
                   "movieinfo.idDirector, " +
                   "movieinfo.strDirector, " + 
                   "movieinfo.strPlotOutline, " +
                   "movieinfo.strPlot, " +
                   "movieinfo.strTagLine, " +
                   "movieinfo.strVotes, " +
                   "movieinfo.fRating, " +
                   "movieinfo.strCast, " +
                   "movieinfo.strCredits, " +
                   "movieinfo.iYear, " +
                   "movieinfo.strGenre, " +
                   "movieinfo.strPictureURL, " +
                   "movieinfo.strTitle, " +
                   "movieinfo.IMDBID, " +
                   "movieinfo.mpaa, " +
                   "movieinfo.runtime, " +
                   "movieinfo.iswatched, " +
                   "movieinfo.strUserReview, " +
                   "movieinfo.strFanartURL, " +
                   "movieinfo.dateAdded, " +
                   "movieinfo.dateWatched, " +
                   "movieinfo.studios, " +
                   "movieinfo.country, " +
                   "movieinfo.language, " +
                   "movieinfo.lastupdate, " +
                   "movieinfo.strSortTitle, " +
                   "path.strPath, " +
                   "movie.discid, " +
                   "path.cdlabel " +
                   "FROM {0} {1} {2}",
            fromClause, whereClause, orderClause);
        
        VideoDatabase.GetMoviesByFilter(sql, out movies, true, true, true, true);
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
          
          if (filter.Where == "actorindex" || filter.Where == "directorindex")
          {
            whereClause += @" SUBSTR(strActor,1,1) IN (" + nWordChar +")";
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
      string table = GetTable(filter.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable,
                              ref useGenreTable, ref useUserGroupsTable);
      if (useGenreTable)
      {
        fromClause += String.Format(",genre,genrelinkmovie");
        whereClause += " AND genre.idGenre=genrelinkMovie.idGenre AND genrelinkMovie.idMovie=movieinfo.idMovie";
        return;
      }

      if (useUserGroupsTable)
      {
        fromClause += String.Format(",usergroup,usergrouplinkmovie");
        whereClause += " AND usergroup.idGroup=usergrouplinkmovie.idGroup AND usergrouplinkMovie.idMovie=movieinfo.idMovie";
        return;
      }

      if (useActorsTable)
      {
        if (CurrentLevel == MaxLevels - 1 && filter.Where == "actor")
        {
          fromClause += String.Format(",actors ,actorlinkmovie");
          whereClause += " AND actors.idActor=actorlinkmovie.idActor AND actorlinkmovie.idMovie=movieinfo.idMovie";
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
                            ref bool useGenreTable, ref bool useUserGroupsTable)
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
        return "strActor";
      }
      if (where == "title")
      {
        return "strTitle";
      }
      if (where == "genre")
      {
        return "strGenre";
      }
      if (where == "user groups")
      {
        return "strGroup";
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
        return "movieinfo.idMovie";
      }
      if (where == "actor")
      {
        return "actors.idActor";
      }
      if (where == "director")
      {
        return "movieinfo.idDirector";
      }
      if (where == "title")
      {
        return "movieinfo.idMovie";
      }
      if (where == "genre")
      {
        return "genre.idGenre";
      }
      if (where == "user groups")
      {
        return "userGroup.idGroup";
      }
      if (where == "year")
      {
        return "movieinfo.iYear";
      }
      if (where == "rating")
      {
        return "movieinfo.fRating";
      }
      if (where == "recently added")
      {
        return "movieinfo.idMovie";
      }
      if (where == "recently watched")
      {
        return "movieinfo.idMovie";
      }
      if (where == "actorindex")
      {
        return "SUBSTR(actors.strActor,1,1)";
      }
      if (where == "directorindex")
      {
        return "SUBSTR(movieinfo.strDirector,1,1)";
      }
      if (where == "titleindex")
      {
        return "SUBSTR(movieinfo.strTitle,1,1)";
      }
      return null;
    }

    private string GetFieldName(string where)
    {
      if (where == "watched")
      {
        return "movieinfo.iswatched";
      }
      if (where == "actor")
      {
        return "actor.strActor";
      }
      if (where == "director")
      {
        return "actor.strActor";
      }
      if (where == "title")
      {
        return "movieinfo.strTitle";
      }
      if (where == "genre")
      {
        return "genre.strGenre";
      }
      if (where == "user groups")
      {
        return "usergroup.strGroup";
      }
      if (where == "year")
      {
        return "movieinfo.iYear";
      }
      if (where == "rating")
      {
        return "movieinfo.fRating";
      }
      if (where == "recently added")
      {
        return "movieinfo.dateAdded";
      }
      if (where == "recently watched")
      {
        return "movieinfo.dateWatched";
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
      if (where == "user groups")
      {
        return movie.UserGroupID.ToString();
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
      if (definition.Where == "user groups")
      {
        if (movie.Title == string.Empty)
        {
          item.Label = movie.SingleUserGroup;
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
        default:
          localizedLevelName = lvlName;
          break;
      }
      
      return localizedLevelName;
    } 

  }
}