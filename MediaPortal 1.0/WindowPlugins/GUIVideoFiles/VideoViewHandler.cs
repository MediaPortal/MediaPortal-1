#region Copyright (C) 2005-2008 Team MediaPortal
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using System.Collections.Generic;

using SQLite.NET;

using MediaPortal.GUI.View;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for VideoViewHandler.
  /// </summary>
  public class VideoViewHandler
  {
    string defaultVideoViews = Config.GetFile(Config.Dir.Base, "defaultVideoViews.xml");
    string customVideoViews = Config.GetFile(Config.Dir.Config, "VideoViews.xml");

    ViewDefinition currentView;
    int currentLevel = 0;
    List<ViewDefinition> views = new List<ViewDefinition>();

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
      catch (Exception)
      {
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

    public string LocalizedCurrentView
    {
      get
      {
        if (currentView == null)
          return string.Empty;
        return currentView.LocalizedName;
      }
    }

    public string CurrentView
    {
      get
      {
        if (currentView == null)
          return string.Empty;
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

    public string CurrentLevelWhere
    {
      get
      {
        FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
        if (definition == null) return string.Empty;
        return definition.Where;
      }
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

    public void Select(IMDBMovie movie)
    {
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      definition.SelectedValue = GetFieldIdValue(movie, definition.Where).ToString();
      if (currentLevel + 1 < currentView.Filters.Count) currentLevel++;

    }

    public ArrayList Execute()
    {
      //build the query
      ArrayList movies = new ArrayList();
      string whereClause = string.Empty;
      string orderClause = string.Empty;
      string fromClause = "actors,movie,movieinfo,path";
      if (CurrentLevel > 0)
      {
        whereClause = "where actors.idactor=movieinfo.idDirector and movieinfo.idmovie=movie.idmovie and movie.idpath=path.idpath";
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
        FilterDefinition defRoot = (FilterDefinition)currentView.Filters[0];
        string table = GetTable(defRoot.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable, ref useGenreTable);

        if (table == "actors")
        {
          sql = String.Format("select * from actors ");
          if (whereClause != string.Empty) sql += "where " + whereClause;
          if (orderClause != string.Empty) sql += orderClause;
          VideoDatabase.GetMoviesByFilter(sql, out movies, true, false, false);
        }
        else if (table == "genre")
        {
          sql = String.Format("select * from genre ");
          if (whereClause != string.Empty) sql += "where " + whereClause;
          if (orderClause != string.Empty) sql += orderClause;
          VideoDatabase.GetMoviesByFilter(sql, out movies, false, false, true);
        }
        else if (defRoot.Where == "year")
        {
          movies = new ArrayList();
          sql = String.Format("select distinct iYear from movieinfo ");
          SQLiteResultSet results = VideoDatabase.GetResults(sql);
          for (int i = 0; i < results.Rows.Count; i++)
          {
            IMDBMovie movie = new IMDBMovie();
            movie.Year = (int)Math.Floor(0.5d + Double.Parse(Database.DatabaseUtility.Get(results, i, "iYear")));
            movies.Add(movie);
          }
        }
        else
        {
          whereClause = "where actors.idActor=movieinfo.idDirector and movieinfo.idmovie=movie.idmovie and movie.idpath=path.idpath";
          BuildRestriction(defRoot, ref whereClause);
          sql = String.Format("select * from {0} {1} {2}",
              fromClause, whereClause, orderClause);
          VideoDatabase.GetMoviesByFilter(sql, out movies, true, true, true);
        }
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        bool useMovieInfoTable = false;
        bool useAlbumTable = false;
        bool useActorsTable = false;
        bool useGenreTable = false;
        FilterDefinition defCurrent = (FilterDefinition)currentView.Filters[CurrentLevel];
        string table = GetTable(defCurrent.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable, ref useGenreTable);
        sql = String.Format("select distinct {0}.* {1} {2} {3}",
            table, fromClause, whereClause, orderClause);
        VideoDatabase.GetMoviesByFilter(sql, out movies, useActorsTable, useMovieInfoTable, useGenreTable);

      }
      else
      {
        sql = String.Format("select movieinfo.fRating,actors.strActor,movieinfo.strCredits,movieinfo.strTagLine,movieinfo.strPlotOutline,movieinfo.strPlot,movieinfo.strVotes,movieinfo.strCast,movieinfo.iYear,movieinfo.strGenre,movieinfo.strPictureURL,movieinfo.strTitle,path.strPath,movie.discid,movieinfo.IMDBID,movieinfo.idMovie,path.cdlabel,movieinfo.mpaa,movieinfo.runtime,movieinfo.iswatched from {0} {1} {2}",
            fromClause, whereClause, orderClause);
        VideoDatabase.GetMoviesByFilter(sql, out movies, true, true, true);
      }
      return movies;
    }

    void BuildSelect(FilterDefinition filter, ref string whereClause, ref string fromClause)
    {
      if (whereClause != "") whereClause += " and ";
      whereClause += String.Format(" {0}='{1}'", GetFieldId(filter.Where), filter.SelectedValue);

      bool useMovieInfoTable = false;
      bool useAlbumTable = false;
      bool useActorsTable = false;
      bool useGenreTable = false;
      string table = GetTable(filter.Where, ref useMovieInfoTable, ref useAlbumTable, ref useActorsTable, ref useGenreTable);
      if (useGenreTable)
      {
        fromClause += String.Format(",genre,genrelinkmovie");
        whereClause += " and genre.idGenre=genrelinkMovie.idGenre and genrelinkMovie.idMovie=movieinfo.idMovie";
      }
      if (useActorsTable)
      {
        fromClause += String.Format(",actors castactors,actorlinkmovie");
        whereClause += " and castactors.idActor=actorlinkmovie.idActor and actorlinkmovie.idMovie=movieinfo.idMovie";
      }
    }

    void BuildRestriction(FilterDefinition filter, ref string whereClause)
    {
      if (filter.SqlOperator != string.Empty && filter.Restriction != string.Empty)
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

    string GetTable(string where, ref bool useMovieInfoTable, ref bool useAlbumTable, ref bool useActorsTable, ref bool useGenreTable)
    {
      if (where == "actor") { useActorsTable = true; return "actors"; }
      if (where == "title") { useMovieInfoTable = true; return "movieinfo"; }
      if (where == "genre") { useGenreTable = true; return "genre"; }
      if (where == "year") { useMovieInfoTable = true; return "movieinfo"; }
      if (where == "rating") { useMovieInfoTable = true; return "movieinfo"; }
      return null;
    }

    string GetField(string where)
    {
      if (where == "watched") return "iswatched";
      if (where == "actor") return "strActor";
      if (where == "title") return "strTitle";
      if (where == "genre") return "strGenre";
      if (where == "year") return "iYear";
      if (where == "rating") return "fRating";
      return null;
    }

    string GetFieldId(string where)
    {
      if (where == "watched") return "movieinfo.idMovie";
      if (where == "actor") return "castactors.idActor";
      if (where == "title") return "movieinfo.idMovie";
      if (where == "genre") return "genre.idGenre";
      if (where == "year") return "movieinfo.iYear";
      if (where == "rating") return "movieinfo.fRating";
      return null;
    }

    string GetFieldName(string where)
    {
      if (where == "watched") return "movieinfo.iswatched";
      if (where == "actor") return "actor.strActor";
      if (where == "title") return "movieinfo.strTitle";
      if (where == "genre") return "genre.strGenre";
      if (where == "year") return "movieinfo.iYear";
      if (where == "rating") return "movieinfo.fRating";
      return null;
    }

    int GetFieldIdValue(IMDBMovie movie, string where)
    {
      if (where == "watched") return (int)movie.Watched;
      if (where == "actor") return movie.actorId;
      if (where == "title") return movie.ID;
      if (where == "genre") return movie.genreId;
      if (where == "year") return movie.Year;
      if (where == "rating") return (int)movie.Rating;
      return -1;
    }

    public void SetLabel(IMDBMovie movie, ref GUIListItem item)
    {
      if (movie == null) return;
      FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
      if (definition.Where == "genre")
      {
        item.Label = movie.SingleGenre;
        item.Label2 = string.Empty;
        item.Label3 = string.Empty;
      }
      if (definition.Where == "actor")
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

    }
  }
}