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
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using SQLite.NET;
using MediaPortal.Profile;

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class VideoDatabaseSqlLite : IVideoDatabase, IDisposable
  {
    public SQLiteClient m_db;

    #region ctor

    public VideoDatabaseSqlLite()
    {
      Open();
    }

    #endregion

    private bool _currentCreateVideoThumbs;

    #region helper funcs

    // Changed - added column check for actorinfo thumbURL column
    private void Open()
    {
      Log.Info("opening video database");
      try
      {
        if (m_db != null)
        {
          Log.Info("Opening video database: VideoDatabase already opened.");
          return;
        }

        // Open database
        String strPath = Config.GetFolder(Config.Dir.Database);
        try
        {
          Directory.CreateDirectory(strPath);
        }
        catch (Exception) {}
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, @"VideoDatabaseV5.db3"));
        DatabaseUtility.SetPragmas(m_db);
        
        CreateTables();
        //
        // Check and upgrade database with new columns if necessary
        //
        UpgradeDatabase();
        // Clean trash from tables
        CleanUpDatabase();
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      Log.Info("video database opened");
    }

    // Added - Check if new columns exists in tables and add new columns if missing (old db upgrade)
    private void UpgradeDatabase()
    {
      try
      {
        if (m_db == null)
        {
          return;
        }

        // actorinfo table
        if (DatabaseUtility.TableColumnExists(m_db, "actorinfo", "thumbURL") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actorinfo\" ADD COLUMN \"thumbURL\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "actorinfo", "IMDBActorID") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actorinfo\" ADD COLUMN \"IMDBActorID\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "actorinfo", "dateofdeath") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actorinfo\" ADD COLUMN \"dateofdeath\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "actorinfo", "placeofdeath") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actorinfo\" ADD COLUMN \"placeofdeath\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        // Actor table
        if (DatabaseUtility.TableColumnExists(m_db, "actors", "IMDBActorID") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actors\" ADD COLUMN \"IMDBActorID\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        // movieinfo table
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "strUserReview") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"strUserReview\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "strFanartURL") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"strFanartURL\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "strDirector") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"strDirector\" text DEFAULT ''";
          m_db.Execute(strSQL);
          
          // Add director name from actors
          strSQL = String.Format("select idMovie, idDirector, actors.strActor from movieinfo, actors where idDirector = idActor");
          SQLiteResultSet results = m_db.Execute(strSQL);
          // Upgrade director name in movieinfo
          for (int i = 0; i < results.Rows.Count; i++)
          {
            string directorName = DatabaseUtility.Get(results, i, "actors.strActor");
            int movieId = Convert.ToInt32(DatabaseUtility.Get(results, i, "idMovie"));
            strSQL = String.Format("update movieinfo set strDirector='{0}' where idMovie = {1}", directorName, movieId);
            m_db.Execute(strSQL);
          }
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "dateAdded") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"dateAdded\" timestamp DEFAULT '0001-01-01 00:00:00'";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "dateWatched") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"dateWatched\" timestamp DEFAULT '0001-01-01 00:00:00'";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "studios") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"studios\" TEXT DEFAULT ''";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "country") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"country\" TEXT DEFAULT ''";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "language") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"language\" TEXT DEFAULT ''";
          m_db.Execute(strSQL);
        }
        // Movie table
        bool watchedUpg = false;
        
        if (DatabaseUtility.TableColumnExists(m_db, "movie", "watched") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movie\" ADD COLUMN \"watched\" bool DEFAULT 0";
          m_db.Execute(strSQL);
          watchedUpg = true;
        }

        if (DatabaseUtility.TableColumnExists(m_db, "movie", "iwatchedPercent") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movie\" ADD COLUMN \"iwatchedPercent\" integer DEFAULT 0";
          m_db.Execute(strSQL);
          watchedUpg = true;
        }

        // Video file Times watched
        if (DatabaseUtility.TableColumnExists(m_db, "movie", "timeswatched") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movie\" ADD COLUMN \"timeswatched\" integer DEFAULT 0";
          m_db.Execute(strSQL);
          watchedUpg = true;
        }

        if (watchedUpg)
        {
          // Set status for movies after upgrade
          string strSQL = String.Format("select idMovie, iswatched from movieinfo");
          SQLiteResultSet results = m_db.Execute(strSQL);

          for (int i = 0; i < results.Rows.Count; i++)
          {
            int movieId = Int32.Parse(DatabaseUtility.Get(results, i, "idMovie"));
            int watched = Int32.Parse(DatabaseUtility.Get(results, i, "iswatched"));
            
            if (watched > 0)
            {
              SetMovieWatchedStatus(movieId, true, 100);
            }
          }

          strSQL = String.Format("select idMovie from movie where watched = 1");
          results = m_db.Execute(strSQL);

          for (int i = 0; i < results.Rows.Count; i++)
          {
            int movieId = Int32.Parse(DatabaseUtility.Get(results, i, "idMovie"));
            SetMovieWatchedStatus(movieId, true, 100);
            MovieWatchedCountIncrease(movieId);
          }
        }
        // Video file duration (stacked)
        if (DatabaseUtility.TableColumnExists(m_db, "movie", "iduration") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movie\" ADD COLUMN \"iduration\" integer DEFAULT 0";
          m_db.Execute(strSQL);
          watchedUpg = true;
        }
        // MediaInfo table
        if (DatabaseUtility.TableExists(m_db, "filesmediainfo") == false)
        {
          DatabaseUtility.AddTable(m_db, "filesmediainfo",
                                   "CREATE TABLE filesmediainfo ( idFile integer primary key, videoCodec text, videoResolution text, aspectRatio text, hasSubtitles bool, audioCodec text, audioChannels text)");
        }
        // Actorlinkmovie
        if (DatabaseUtility.TableColumnExists(m_db, "actorlinkmovie", "strRole") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actorlinkmovie\" ADD COLUMN \"strRole\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        // IMDB Movies
        if (DatabaseUtility.TableExists(m_db, "IMDBmovies") == false)
        {
          DatabaseUtility.AddTable(m_db, "IMDBmovies",
                                   "CREATE TABLE IMDBmovies ( idIMDB text, idTmdb text, strPlot text, strCast text, strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, mpaa text)");
        }
        // UserGroups table
        if (DatabaseUtility.TableExists(m_db, "usergroup") == false)
        {
          DatabaseUtility.AddTable(m_db, "usergroup",
                               "CREATE TABLE usergroup ( idGroup integer primary key, strGroup text, strRule text)");
        }
        // UserGroups movie links table
        if (DatabaseUtility.TableExists(m_db, "usergrouplinkmovie") == false)
        {
          DatabaseUtility.AddTable(m_db, "usergrouplinkmovie",
                               "CREATE TABLE usergrouplinkmovie ( idGroup integer, idMovie integer)");
        }
      }

      catch (Exception ex)
      {
        Log.Error("videodatabase upgrade exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    // Deletes unwanted inserted data from the videodatabase tables which polutes user data, ie. when OnlineVideos play
    // some online video, MP automatically insert it's data in tables and we really don't need'em here
    // Also cleans all broken linked data from other tables
    private void CleanUpDatabase()
    {
      try
      {
        if (m_db == null)
        {
          return;
        }

        string strSql = String.Format("select * from path");
        SQLiteResultSet resultsPath = m_db.Execute(strSql);

        Int32 cleanedRows = 0;

        // Get paths
        for (int iRowPath = 0; iRowPath < resultsPath.Rows.Count; iRowPath++)
        {
          int idPath = Int32.Parse(DatabaseUtility.Get(resultsPath, iRowPath, "idPath"));
          string strPath = DatabaseUtility.Get(resultsPath, iRowPath, "strPath");
          
          // Check for trash paths
          if (strPath.StartsWith("http:"))
          {
            // Find all files related to current trash path
            string strSqlfile = String.Format("select * from files where idPath={0}", idPath);
            SQLiteResultSet resultsFile = m_db.Execute(strSqlfile);
            // Delete data in file related tables
            for (int iRowFiles = 0; iRowFiles < resultsFile.Rows.Count; iRowFiles++)
            {
              int idFile = Int32.Parse(DatabaseUtility.Get(resultsFile, iRowFiles, "idFile"));
              // Bookmark
              strSql = String.Format("delete from bookmark where idFile={0}", idFile);
              m_db.Execute(strSql);
              cleanedRows += m_db.ChangedRows();
              // Duration
              strSql = String.Format("delete from duration where idFile={0}", idFile);
              m_db.Execute(strSql);
              cleanedRows += m_db.ChangedRows();
              // Resume
              strSql = String.Format("delete from resume where idFile={0}", idFile);
              m_db.Execute(strSql);
              cleanedRows += m_db.ChangedRows();
            }
            // Delete files
            strSql = String.Format("delete from files where idPath={0}", idPath);
            m_db.Execute(strSql);
            cleanedRows += m_db.ChangedRows();
            // Delete movies
            strSql = String.Format("delete from movie where idPath={0}", idPath);
            m_db.Execute(strSql);
            cleanedRows += m_db.ChangedRows();
            // Delete path
            strSql = String.Format("delete from path where idPath={0}", idPath);
            m_db.Execute(strSql);
            cleanedRows += m_db.ChangedRows();
          }
        }
        Log.Info("Cleaned up " + cleanedRows + " rows for unwanted paths.");

        // Find all files without not existing path link
        strSql = String.Format("select * from files where files.idPath not in (select idPath from path)");
        SQLiteResultSet results = m_db.Execute(strSql);
        Log.Info("Found " + results.Rows.Count + " files without path link. Cleaning files related tables.");

        cleanedRows = 0;

        // Delete data in file related tables
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          int idFile = Int32.Parse(DatabaseUtility.Get(results, iRow, "files.idFile"));
          // Bookmark
          strSql = String.Format("delete from bookmark where idFile={0}", idFile);
          m_db.Execute(strSql);
          cleanedRows += m_db.ChangedRows();
          // Duration
          strSql = String.Format("delete from duration where idFile={0}", idFile);
          m_db.Execute(strSql);
          cleanedRows += m_db.ChangedRows();
          // Resume
          strSql = String.Format("delete from resume where idFile={0}", idFile);
          m_db.Execute(strSql);
          cleanedRows += m_db.ChangedRows();
        }
        Log.Info("Cleaned up " + cleanedRows + " rows for tables without file link.");

        // Delete files without path link
        strSql = String.Format("delete from files where files.idPath not in (select idPath from path)");
        m_db.Execute(strSql);
        Log.Info("Clean up files (no path link): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete path without any file link
        strSql = String.Format("delete from path where path.idPath not in (select idPath from files)");
        m_db.Execute(strSql);
        Log.Info("Clean up path (no file link): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete movies without path link
        strSql = String.Format("delete from movie where movie.idPath not in (select idPath from path)");
        m_db.Execute(strSql);
        Log.Info("Clean up movie (no path link): " + m_db.ChangedRows() + " rows affected.");

        // Delete actorinfo without link to actorId
        strSql = String.Format("delete from actorinfo where actorinfo.idActor not in (select idActor from actors)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorinfo (no actorId link to actors): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete actorlinkmovie without link to actorId 
        strSql = String.Format("delete from actorlinkmovie where actorlinkmovie.idActor not in (select idActor from actors)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorlinkmovie (no actorId link to actors): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete actorlinkmovie without link to the movie
        strSql = String.Format("delete from actorlinkmovie where actorlinkmovie.idMovie not in (select idMovie from movie)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorlinkmovie (no movie link): " + m_db.ChangedRows() + " rows affected.");

        // Delete actorinfomovies without link to the actorId in actors
        strSql = String.Format("delete from actorinfomovies where actorinfomovies.idActor not in (select idActor from actors)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorinfomovies (no actorId link to actors): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete movieinfo without link to the movie
        strSql = String.Format("delete from movieinfo where movieinfo.idMovie not in (select idMovie from movie)");
        m_db.Execute(strSql);
        Log.Info("Clean up movieinfo (no movie link): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete genrelinkmovie without link to the movie
        strSql = String.Format("delete from genrelinkmovie where genrelinkmovie.idMovie not in (select idMovie from movie)");
        m_db.Execute(strSql);
        Log.Info("Clean up genrelinkmovie (no movie link): " + m_db.ChangedRows() + " rows affected.");
        
        // Compact db after cleanup
        long vdbFile = new FileInfo(Config.GetFile(Config.Dir.Database, @"VideoDatabaseV5.db3")).Length;
        Log.Info("Compacting videodatabase: " + vdbFile + " bytes");
        //DatabaseUtility.CompactDatabase(m_db);
        try
        {
          DatabaseUtility.CompactDatabase(m_db);
          vdbFile = new FileInfo(Config.GetFile(Config.Dir.Database, @"VideoDatabaseV5.db3")).Length;
          Log.Info("Compacting finished successfully. New file lenght: " + vdbFile + " bytes");
        }
        catch (Exception)
        {
          Log.Error("Compact videodatabase: vacuum failed");
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase cleanup exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void Dispose()
    {
      if (m_db != null)
      {
        m_db.Close();
        m_db.Dispose();
        m_db = null;
      }
    }

    private void CreateTables()
    {
      if (m_db == null)
      {
        return;
      }
      DatabaseUtility.AddTable(m_db, "bookmark",
                               "CREATE TABLE bookmark ( idBookmark integer primary key, idFile integer, fPercentage text)");
      DatabaseUtility.AddTable(m_db, "genre",
                               "CREATE TABLE genre ( idGenre integer primary key, strGenre text)");
      DatabaseUtility.AddTable(m_db, "genrelinkmovie",
                               "CREATE TABLE genrelinkmovie ( idGenre integer, idMovie integer)");
      DatabaseUtility.AddTable(m_db, "usergroup",
                               "CREATE TABLE usergroup ( idGroup integer primary key, strGroup text, strRule text)");
      DatabaseUtility.AddTable(m_db, "usergrouplinkmovie",
                               "CREATE TABLE usergrouplinkmovie ( idGroup integer, idMovie integer)");
      DatabaseUtility.AddTable(m_db, "movie",
                               "CREATE TABLE movie ( idMovie integer primary key, idPath integer, hasSubtitles integer, discid text, watched bool, timeswatched integer, iduration integer)");
      DatabaseUtility.AddTable(m_db, "movieinfo",
                               "CREATE TABLE movieinfo ( idMovie integer, idDirector integer, strDirector text, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text, mpaa text,runtime integer, iswatched integer, strUserReview text, strFanartURL text, dateAdded timestamp, dateWatched timestamp, studios text, country text, language text)");
      DatabaseUtility.AddTable(m_db, "actorlinkmovie",
                               "CREATE TABLE actorlinkmovie ( idActor integer, idMovie integer, strRole text)");
      DatabaseUtility.AddTable(m_db, "actors",
                               "CREATE TABLE actors ( idActor integer primary key, strActor text, IMDBActorID text)");
      DatabaseUtility.AddTable(m_db, "path",
                               "CREATE TABLE path ( idPath integer primary key, strPath text, cdlabel text)");
      DatabaseUtility.AddTable(m_db, "files",
                               "CREATE TABLE files ( idFile integer primary key, idPath integer, idMovie integer,strFilename text)");
      DatabaseUtility.AddTable(m_db, "resume",
                               "CREATE TABLE resume ( idResume integer primary key, idFile integer, stoptime integer, resumeData blob)");
      DatabaseUtility.AddTable(m_db, "duration",
                               "CREATE TABLE duration ( idDuration integer primary key, idFile integer, duration integer)");
      DatabaseUtility.AddTable(m_db, "actorinfo",
                               "CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text, thumbURL text, IMDBActorID text, dateofdeath text, placeofdeath text)");
      DatabaseUtility.AddTable(m_db, "actorinfomovies",
                               "CREATE TABLE actorinfomovies ( idActor integer, idDirector integer, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text, mpaa text, runtime integer, iswatched integer, role text)");
      DatabaseUtility.AddTable(m_db, "IMDBmovies",
                               "CREATE TABLE IMDBmovies ( idIMDB text, idTmdb text, strPlot text, strCast text, strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, mpaa text)");
      DatabaseUtility.AddTable(m_db, "VideoThumbBList",
                               "CREATE TABLE VideoThumbBList ( idVideoThumbBList integer primary key, strPath text, strExpires text, strFileDate text, strFileSize text)");
      DatabaseUtility.AddTable(m_db, "filesmediainfo",
                               "CREATE TABLE filesmediainfo ( idFile integer primary key, videoCodec text, videoResolution text, aspectRatio text, hasSubtitles bool, audioCodec text, audioChannels text)");
      // Indexes
      // ActorInfo
      DatabaseUtility.AddIndex(m_db, "idxactorinfo_idActor",
                               "CREATE INDEX idxactorinfo_idActor ON actorinfo(idActor ASC)");
      // ActorInfoMovies
      DatabaseUtility.AddIndex(m_db, "idxactorinfomovies_idActor",
                               "CREATE INDEX idxactorinfomovies_idActor ON actorinfomovies(idActor ASC)");
      // ActorLinkMovie
      DatabaseUtility.AddIndex(m_db, "idxactorlinkmovie_idActor",
                               "CREATE INDEX idxactorlinkmovie_idActor ON actorlinkmovie(idActor ASC)");
      DatabaseUtility.AddIndex(m_db, "idxactorlinkmovie_idMovie",
                               "CREATE INDEX idxactorlinkmovie_idMovie ON actorlinkmovie(idMovie ASC)");
      // Actors
      DatabaseUtility.AddIndex(m_db, "idxactors_strActor", "CREATE INDEX idxactors_strActor ON actors(strActor ASC)");
      DatabaseUtility.AddIndex(m_db, "idxactors_idActor", 
                              "CREATE UNIQUE INDEX idxactors_idActor ON actors(idActor ASC)");
      DatabaseUtility.AddIndex(m_db, "idxactors_idIMDB",
                              "CREATE INDEX idxactors_idIMDB ON actors(IMDBActorID ASC)");
      // Files
      DatabaseUtility.AddIndex(m_db, "idxfiles_idFile", "CREATE UNIQUE INDEX idxfiles_idFile ON files(idFile ASC)");
      DatabaseUtility.AddIndex(m_db, "idxfiles_idMovie", "CREATE INDEX idxfiles_idMovie ON files(idMovie ASC)");
      DatabaseUtility.AddIndex(m_db, "idxfiles_idPath", "CREATE INDEX idxfiles_idPath ON files(idPath ASC)");
      // GenreLinkMovie
      DatabaseUtility.AddIndex(m_db, "idxgenrelinkmovie_idGenre",
                               "CREATE INDEX idxgenrelinkmovie_idGenre ON genrelinkmovie(idGenre ASC)");
      DatabaseUtility.AddIndex(m_db, "idxgenrelinkmovie_idMovie",
                               "CREATE INDEX idxgenrelinkmovie_idMovie ON genrelinkmovie(idMovie ASC)");
      // Movie
      DatabaseUtility.AddIndex(m_db, "idxmovie_idMovie", "CREATE UNIQUE INDEX idxmovie_idMovie ON movie(idMovie ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovie_idPath", "CREATE INDEX idxmovie_idPath ON movie(idPath ASC)");
      // MovieInfo
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_iYear", "CREATE INDEX idxmovieinfo_iYear ON movieinfo(iYear ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_idDirector",
                               "CREATE INDEX idxmovieinfo_idDirector ON movieinfo(idDirector ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_idMovie",
                               "CREATE UNIQUE INDEX idxmovieinfo_idMovie ON movieinfo(idMovie ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_strTitle",
                               "CREATE INDEX idxmovieinfo_strTitle ON movieinfo(strTitle ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_idIMDB",
                               "CREATE INDEX idxmovieinfo_idIMDB ON movieinfo(IMDBID ASC)");
      // Path
      DatabaseUtility.AddIndex(m_db, "idxpath_idPath", "CREATE INDEX idxpath_idPath ON path(idPath ASC)");
      DatabaseUtility.AddIndex(m_db, "idxpath_strPath", "CREATE INDEX idxpath_strPath ON path(strPath ASC)");
      // VideThumbList
      DatabaseUtility.AddIndex(m_db, "idxVideoThumbBList_strPath",
                               "CREATE INDEX idxVideoThumbBList_strPath ON VideoThumbBList(strPath ASC, strExpires ASC)");
      DatabaseUtility.AddIndex(m_db, "idxVideoThumbBList_strExpires",
                               "CREATE INDEX idxVideoThumbBList_strExpires ON VideoThumbBList(strExpires ASC)");
      // FilesMediaInfo
      DatabaseUtility.AddIndex(m_db, "idxfilesmediainfo_idFile",
                               "CREATE UNIQUE INDEX idxfilesmediainfo_idFile ON filesmediainfo (idFile ASC)");
      // UserGroup
      DatabaseUtility.AddIndex(m_db, "idxuserGroup_idGroup",
                               "CREATE UNIQUE INDEX idxuserGroup_idGroup ON usergroup (idGroup ASC)");
      DatabaseUtility.AddIndex(m_db, "idxuserGroup_strGroup",
                               "CREATE INDEX idxuserGroup_strGroup ON usergroup (strGroup ASC)");
      // userGroupLinkMovie
      DatabaseUtility.AddIndex(m_db, "idxusergrouplinkmovie_idGroup",
                               "CREATE INDEX idxusergrouplinkmovie_idGroup ON usergrouplinkmovie (idGroup ASC)");
      DatabaseUtility.AddIndex(m_db, "idxusergrouplinkmovie_idMovie",
                               "CREATE INDEX idxusergrouplinkmovie_idMovie ON usergrouplinkmovie (idMovie ASC)");
      // Duration
      DatabaseUtility.AddIndex(m_db, "idxduration_idFile",
                               "CREATE UNIQUE INDEX idxduration_idFile ON duration (idFile ASC)");
      // IMDBMovies
      DatabaseUtility.AddIndex(m_db, "idximdbmovies_idIMDB",
                               "CREATE UNIQUE INDEX idximdbmovies_idIMDB ON IMDBmovies (idIMDB ASC)");

      return;
    }

    #endregion

    #region Movie files and paths

    public int AddFile(int lMovieId, int lPathId, string strFileName)
    {
      if (m_db == null)
      {
        return -1;
      }
      try
      {
        int lFileId = -1;
        strFileName = strFileName.Trim();

        string strSQL = String.Format("select * from files where idmovie={0} and idpath={1} and strFileName like '{2}'",
                                      lMovieId, lPathId, strFileName);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results != null && results.Rows.Count > 0)
        {
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idFile"), out lFileId);
          CheckMediaInfo(strFileName, string.Empty, lPathId, lFileId, false);
          return lFileId;
        }

        strSQL = String.Format("insert into files (idFile, idMovie,idPath, strFileName) values(null, {0},{1},'{2}')",
                               lMovieId, lPathId, strFileName);
        results = m_db.Execute(strSQL);
        lFileId = m_db.LastInsertID();
        CheckMediaInfo(strFileName, string.Empty, lPathId, lFileId, false);
        return lFileId;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }
    
    private int MovieDuration(ArrayList files)
    {
      int totalMovieDuration = 0;

      if (files == null || files.Count == 0)
      {
        return totalMovieDuration;
      }

      try
      {
        foreach (string file in files)
        {
          int fileID = VideoDatabase.GetFileId(file);
          int tempDuration = VideoDatabase.GetVideoDuration(fileID);

          totalMovieDuration += tempDuration;
        }
      }
      catch (Exception) { }

      return totalMovieDuration;
    }

    public int GetFile(string strFilenameAndPath, out int lPathId, out int lMovieId, bool bExact)
    {
      lPathId = -1;
      lMovieId = -1;
      try
      {
        if (null == m_db)
        {
          return -1;
        }
        string strPath, strFileName;
        string cdlabel = GetDVDLabel(strFilenameAndPath);
        DatabaseUtility.RemoveInvalidChars(ref cdlabel);
        strFilenameAndPath = strFilenameAndPath.Trim();
        DatabaseUtility.Split(strFilenameAndPath, out strPath, out strFileName);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        lPathId = GetPath(strPath);
        if (lPathId < 0)
        {
          return -1;
        }

        string strSQL = String.Format("select * from files where idpath={0}", lPathId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          for (int iRow = 0; iRow < results.Rows.Count; ++iRow)
          {
            string strFname = DatabaseUtility.Get(results, iRow, "strFilename");
            if (bExact)
            {
              if (strFname.ToUpperInvariant() == strFileName.ToUpperInvariant())
              {
                // was just returning 'true' here, but this caused problems with
                // the bookmarks as these are stored by fileid. forza.
                int lFileId;
                Int32.TryParse(DatabaseUtility.Get(results, iRow, "idFile"), out lFileId);
                Int32.TryParse(DatabaseUtility.Get(results, iRow, "idMovie"), out lMovieId);
                return lFileId;
              }
            }
            else
            {
              if (Util.Utils.ShouldStack(strFname, strFileName))
              {
                int lFileId;
                Int32.TryParse(DatabaseUtility.Get(results, iRow, "idFile"), out lFileId);
                Int32.TryParse(DatabaseUtility.Get(results, iRow, "idMovie"), out lMovieId);
                return lFileId;
              }
              if (strFname.ToUpperInvariant() == strFileName.ToUpperInvariant())
              {
                // was just returning 'true' here, but this caused problems with
                // the bookmarks as these are stored by fileid. forza.
                int lFileId;
                Int32.TryParse(DatabaseUtility.Get(results, iRow, "idFile"), out lFileId);
                Int32.TryParse(DatabaseUtility.Get(results, iRow, "idMovie"), out lMovieId);
                return lFileId;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int AddMovieFile(string strFile)
    {
      bool bHassubtitles = false;
      if (strFile.ToLower().IndexOf(".ifo") >= 0)
      {
        bHassubtitles = true;
      }
      if (strFile.ToLower().IndexOf(".vob") >= 0)
      {
        bHassubtitles = true;
      }
      string strCDLabel = "";
      if (Util.Utils.IsDVD(strFile))
      {
        strCDLabel = Util.Utils.GetDriveSerial(strFile);
      }
      string[] sub_exts = {
                            ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss",
                            ".ass", ".idx", ".ifo"
                          };
      // check if movie has subtitles
      for (int i = 0; i < sub_exts.Length; i++)
      {
        string strSubTitleFile = strFile;
        strSubTitleFile = Path.ChangeExtension(strFile, sub_exts[i]);
        if (File.Exists(strSubTitleFile))
        {
          bHassubtitles = true;
          break;
        }
      }
      return VideoDatabase.AddMovie(strFile, bHassubtitles);
    }

    public int AddPath(string strPath)
    {
      try
      {
        if (null == m_db)
        {
          return -1;
        }

        string cdlabel = GetDVDLabel(strPath);
        DatabaseUtility.RemoveInvalidChars(ref cdlabel);

        strPath = strPath.Trim();
        string strSQL = String.Format("select * from path where strPath like '{0}' and cdlabel like '{1}'", strPath,
                                      cdlabel);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into Path (idPath, strPath, cdlabel) values( NULL, '{0}', '{1}')", strPath,
                                 cdlabel);
          m_db.Execute(strSQL);
          int lPathId = m_db.LastInsertID();
          return lPathId;
        }
        else
        {
          int lPathId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idPath"), out lPathId);
          strSQL = String.Format("update path set strPath='{0}' where idPath = {1}", strPath, lPathId);
          m_db.Execute(strSQL);
          return lPathId;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int GetPath(string strPath)
    {
      try
      {
        if (null == m_db)
        {
          return -1;
        }
        string cdlabel = GetDVDLabel(strPath);
        DatabaseUtility.RemoveInvalidChars(ref cdlabel);

        strPath = strPath.Trim();
        if (Util.Utils.IsDVD(strPath))
        {
          // It's a DVD! Any drive letter should be OK as long as the label and rest of the path matches
          strPath = strPath.Replace(strPath.Substring(0, 1), "_");
        }
        string strSQL = String.Format("select * from path where strPath like '{0}' and cdlabel like '{1}'", strPath,
                                      cdlabel);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          int lPathId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idPath"), out lPathId);
          return lPathId;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void DeleteFile(int iFileId)
    {
      try
      {
        string strSQL = String.Format("delete from files where idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from resume where idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from duration where idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from filesmediainfo where idfile={0}", iFileId);
        m_db.Execute(strSQL);
      }
      catch (SQLiteException ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void RemoveFilesForMovie(int lMovieId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        // Delete files data from other tables
        string strSQL = string.Empty;
        ArrayList files = new ArrayList();
        GetFilesForMovie(lMovieId, ref files);

        foreach (string file in files)
        {
          int idFile = GetFileId(file);
          strSQL = String.Format("delete from resume where idfile={0}", idFile);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from duration where idfile={0}", idFile);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from filesmediainfo where idfile={0}", idFile);
          m_db.Execute(strSQL);
        }

        strSQL = String.Format("delete from files where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetFileId(string strFilenameAndPath)
    {
      int lPathId;
      int lMovieId;
      return GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
    }

    public void GetFilesForMovie(int lMovieId, ref ArrayList files)
    {
      try
      {
        files.Clear();
        if (null == m_db)
        {
          return;
        }
        if (lMovieId < 0)
        {
          return;
        }

        string strSQL = String.Format(
          "select * from path,files where path.idPath=files.idPath and files.idmovie={0} order by strFilename",
          lMovieId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strFile = DatabaseUtility.Get(results, i, "files.strFilename");
          string strPath = DatabaseUtility.Get(results, i, "path.strPath");
          if (strPath != Strings.Unknown)
          {
            strFile = strPath + strFile;
          }
          files.Add(strFile);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    # region MediaInfo

    // Check and add, if necessary, media info for video files
    // Use (file, pathID and fileID) or (full filename with path and fileID)
    private void CheckMediaInfo(string file, string fullPathFilename, int pathID, int fileID, bool refresh)
    {
      string strSQL = string.Empty;
      string strFilenameAndPath = string.Empty;

      // Get path name from pathID
      strSQL = String.Format("select * from path where idPath={0}", pathID);
      SQLiteResultSet results = m_db.Execute(strSQL);

      // No ftp or http videos
      string path = DatabaseUtility.Get(results, 0, "strPath");

      if (path.IndexOf("remote:") >= 0 || path.IndexOf("http:") >= 0)
      {
        return;
      }

      // We can use (path+file) or full path filename
      if (fullPathFilename == string.Empty)
      {
        strFilenameAndPath = path + file.Replace("''", "'");
      }
      else
      {
        strFilenameAndPath = fullPathFilename;
      }

      // Prevent empty database record for empty media scan
      if (!File.Exists(strFilenameAndPath))
      {
        return;
      }

      // Check if we processed file allready
      strSQL = String.Format("select * from filesmediainfo where idFile={0}", fileID);
      results = m_db.Execute(strSQL);

      if (results.Rows.Count == 0 || refresh)
      {
        Log.Info("VideoDatabase media info file: {0}", strFilenameAndPath);
        bool isImage = false;
        string drive = string.Empty;
        bool daemonAutoPlay = false;
        string autoplayVideo = string.Empty;

        if (VirtualDirectory.IsImageFile(Path.GetExtension(strFilenameAndPath)))
        {
          if (!DaemonTools.IsMounted(strFilenameAndPath))
          {
            using (Settings xmlreader = new MPSettings())
            {
              daemonAutoPlay = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
              autoplayVideo = xmlreader.GetValueAsString("general", "autoplay_video", "Ask");
              xmlreader.SetValueAsBool("daemon", "askbeforeplaying", false);
              xmlreader.SetValue("general", "autoplay_video", "No");
            }

            if (!DaemonTools.Mount(strFilenameAndPath, out drive))
            {
              return;
            }
          }
          isImage = true;
        }

        MediaInfoWrapper mInfo = new MediaInfoWrapper(strFilenameAndPath);

        if (isImage && DaemonTools.IsMounted(strFilenameAndPath))
        {
          DaemonTools.UnMount();
          using (Settings xmlwriter = new MPSettings())
          {
            xmlwriter.SetValueAsBool("daemon", "askbeforeplaying", daemonAutoPlay);
            xmlwriter.SetValue("general", "autoplay_video", autoplayVideo);
          }
        }

        int subtitles = 0;

        if (mInfo.HasSubtitles)
        {
          subtitles = 1;
        }

        try
        {
          if (results.Rows.Count == 0)
          {
            strSQL = String.Format(
              "insert into filesmediainfo (idFile, videoCodec, videoResolution, aspectRatio, hasSubtitles, audioCodec, audioChannels) values({0},'{1}','{2}','{3}',{4},'{5}','{6}')",
              fileID,
              Util.Utils.MakeFileName(mInfo.VideoCodec),
              mInfo.VideoResolution,
              mInfo.AspectRatio,
              subtitles,
              Util.Utils.MakeFileName(mInfo.AudioCodec),
              mInfo.AudioChannelsFriendly);
          }
          else
          {
            strSQL = String.Format(
              "update filesmediainfo set videoCodec='{1}', videoResolution='{2}', aspectRatio='{3}', hasSubtitles='{4}', audioCodec='{5}', audioChannels='{6}' where idFile={0}",
              fileID,
              Util.Utils.MakeFileName(mInfo.VideoCodec),
              mInfo.VideoResolution,
              mInfo.AspectRatio,
              subtitles,
              Util.Utils.MakeFileName(mInfo.AudioCodec),
              mInfo.AudioChannelsFriendly);
          }

          // Prevent empty record for future or unknown codecs
          if (mInfo.VideoCodec == string.Empty)
          {
            return;
          }

          m_db.Execute(strSQL);
          SetVideoDuration(fileID, mInfo.VideoDuration / 1000);
          ArrayList movieFiles = new ArrayList();
          int movieId = VideoDatabase.GetMovieId(strFilenameAndPath);
          VideoDatabase.GetFilesForMovie(movieId, ref movieFiles);
          SetMovieDuration(movieId, MovieDuration(movieFiles));
        }
        catch (Exception) { }
      }
    }

    public void GetVideoFilesMediaInfo(string strFilenameAndPath, ref VideoFilesMediaInfo mediaInfo, bool refresh)
    {
      try
      {
        if (strFilenameAndPath == String.Empty || !Util.Utils.IsVideo(strFilenameAndPath))
          return;

        if (strFilenameAndPath.IndexOf("remote:") >= 0 || strFilenameAndPath.IndexOf("http:") >= 0)
          return;

        int fileID = GetFileId(strFilenameAndPath);

        if (fileID < 1)
        {
          return;
        }

        // Get media info from database
        string strSQL = String.Format("select * from filesmediainfo where idFile={0}", fileID);
        SQLiteResultSet results = m_db.Execute(strSQL);

        // Set mInfo for files already in db but not scanned before
        //if (results.Rows.Count == 0 || refresh)  // Do not scan 0 result beacuse it stall MP (multi scan of the same file produce error)
        if (results.Rows.Count > 0 && refresh) // Only refresh from context menu "Refresh media info" from shares view
        {
          try
          {
            CheckMediaInfo(string.Empty, strFilenameAndPath, -1, fileID, refresh);
            results = m_db.Execute(strSQL);
          }
          catch (Exception) { }
        }

        mediaInfo.VideoCodec = DatabaseUtility.Get(results, 0, "videoCodec");
        mediaInfo.VideoResolution = DatabaseUtility.Get(results, 0, "videoResolution");
        mediaInfo.AspectRatio = DatabaseUtility.Get(results, 0, "aspectRatio");

        int hasSubtitles;
        int.TryParse(DatabaseUtility.Get(results, 0, "hasSubtitles"), out hasSubtitles);

        if (hasSubtitles != 0)
        {
          mediaInfo.HasSubtitles = true;
        }
        else
        {
          mediaInfo.HasSubtitles = false;
        }

        mediaInfo.AudioCodec = DatabaseUtility.Get(results, 0, "audioCodec");
        mediaInfo.AudioChannels = DatabaseUtility.Get(results, 0, "audioChannels");
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase mediainfo exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool HasMediaInfo(string fileName)
    {
      try
      {
        if (fileName == String.Empty || !Util.Utils.IsVideo(fileName))
          return false;

        if (fileName.IndexOf("remote:") >= 0 || fileName.IndexOf("http:") >= 0)
          return false;

        int fileID = GetFileId(fileName);

        if (fileID < 1)
        {
          return false;
        }

        // Get media info from database
        string strSQL = String.Format("select * from filesmediainfo where idFile={0}", fileID);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase mediainfo exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    #endregion

    #region Genres

    public int AddGenre(string strGenre1)
    {
      try
      {
        string strGenre = strGenre1.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strGenre);

        if (null == m_db)
        {
          return -1;
        }
        string strSQL = "select * from genre where strGenre like '";
        strSQL += strGenre;
        strSQL += "'";
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = "insert into genre (idGenre, strGenre) values( NULL, '";
          strSQL += strGenre;
          strSQL += "')";
          m_db.Execute(strSQL);
          int lGenreId = m_db.LastInsertID();
          return lGenreId;
        }
        else
        {
          int lGenreId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idGenre"), out lGenreId);
          return lGenreId;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void GetGenres(ArrayList genres)
    {
      if (m_db == null)
      {
        return;
      }
      try
      {
        genres.Clear();
        SQLiteResultSet results = m_db.Execute("select * from genre order by strGenre");
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          genres.Add(DatabaseUtility.Get(results, iRow, "strGenre"));
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void AddGenreToMovie(int lMovieId, int lGenreId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("select * from genrelinkmovie where idGenre={0} and idMovie={1}", lGenreId,
                                      lMovieId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into genrelinkmovie (idGenre, idMovie) values( {0},{1})", lGenreId, lMovieId);
          m_db.Execute(strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void DeleteGenre(string genre)
    {
      try
      {
        string genreFiltered = genre;
        DatabaseUtility.RemoveInvalidChars(ref genreFiltered);
        string sql = String.Format("select * from genre where strGenre like '{0}'", genreFiltered);
        SQLiteResultSet results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          return;
        }

        int idGenre;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idGenre"), out idGenre);
        m_db.Execute(sql);

        m_db.Execute(String.Format("delete from genrelinkmovie where idGenre={0}", idGenre));
        m_db.Execute(String.Format("delete from genre where idGenre={0}", idGenre));
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void RemoveGenresForMovie(int lMovieId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("delete from genrelinkmovie where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    #region UserGroups

    public int AddUserGroup(string strUserGroup1)
    {
      try
      {
        string strUserGroup = strUserGroup1.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strUserGroup);

        if (null == m_db)
        {
          return -1;
        }
        string strSQL = "select * from usergroup where strGroup like '";
        strSQL += strUserGroup;
        strSQL += "'";
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = "insert into usergroup (idGroup, strGroup) values( NULL, '";
          strSQL += strUserGroup;
          strSQL += "')";
          m_db.Execute(strSQL);
          int lUserGroupId = m_db.LastInsertID();
          return lUserGroupId;
        }
        else
        {
          int lUserGroupId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idGroup"), out lUserGroupId);
          return lUserGroupId;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void AddUserGroupRuleByGroupId(int groupId, string rule)
    {
      try
      {
        DatabaseUtility.RemoveInvalidChars(ref rule);

        if (null == m_db)
        {
          return;
        }

        string strSQL = String.Format("update usergroup set strRule='{0}' where idGroup={1}", rule, groupId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void AddUserGroupRuleByGroupName(string groupName, string rule)
    {
      try
      {
        DatabaseUtility.RemoveInvalidChars(ref rule);

        if (null == m_db)
        {
          return;
        }

        string strSQL = String.Format("update usergroup set strRule='{0}' where strGroup like '{1}'", rule, groupName);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetUserGroups(ArrayList userGroups)
    {
      if (m_db == null)
      {
        return;
      }
      try
      {
        userGroups.Clear();
        SQLiteResultSet results = m_db.Execute("select * from usergroup order by strGroup");
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          userGroups.Add(DatabaseUtility.Get(results, iRow, "strGroup"));
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }
    
    public void GetMovieUserGroups(int movieId, ArrayList userGroups)
    {
      if (m_db == null)
      {
        return;
      }
      try
      {
        userGroups.Clear();
        string strSQL = String.Format("select idGroup from usergrouplinkmovie where idMovie={0}", movieId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }
        
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          int groupId = Convert.ToInt32(DatabaseUtility.Get(results, iRow, "idGroup"));
          strSQL = String.Format("select strGroup from usergroup where idGroup = {0}", groupId);
          SQLiteResultSet resultsGroup = m_db.Execute(strSQL);
          
          if (resultsGroup.Rows.Count > 0)
          {
            userGroups.Add(DatabaseUtility.Get(resultsGroup, 0, "strGroup"));
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetUserGroupRule(string group)
    {
      try
      {
        if (null == m_db)
        {
          return string.Empty;
        }
        string strSQL = String.Format("SELECT strRule from usergroup WHERE strGroup like '{0}'", group);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count != 0)
        {
          return DatabaseUtility.Get(results, 0, "strRule");
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return string.Empty;
    }

    public void AddUserGroupToMovie(int lMovieId, int lUserGroupId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("select * from usergrouplinkmovie where idGroup={0} and idMovie={1}", lUserGroupId,
                                      lMovieId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into usergrouplinkmovie (idGroup, idMovie) values( {0},{1})", lUserGroupId, lMovieId);
          m_db.Execute(strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void RemoveUserGroupFromMovie(int lMovieId, int lUserGroupId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("delete from usergrouplinkmovie where idGroup={0} and idMovie={1}", lUserGroupId,
                                      lMovieId);

        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void DeleteUserGroup(string userGroup)
    {
      try
      {
        string userGroupFiltered = userGroup;
        DatabaseUtility.RemoveInvalidChars(ref userGroupFiltered);
        string sql = String.Format("select * from usergroup where strGroup like '{0}'", userGroupFiltered);
        SQLiteResultSet results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          return;
        }

        int idUserGroup;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idGroup"), out idUserGroup);
        m_db.Execute(sql);

        m_db.Execute(String.Format("delete from usergrouplinkmovie where idGroup={0}", idUserGroup));
        m_db.Execute(String.Format("delete from usergroup where idGroup={0}", idUserGroup));
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      string thumb = Util.Utils.GetCoverArtName(Thumbs.MovieUserGroups, userGroup);
      string largeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieUserGroups, userGroup);
      Util.Utils.FileDelete(thumb);
      Util.Utils.FileDelete(largeThumb);
    }

    public void RemoveUserGroupsForMovie(int lMovieId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("delete from usergrouplinkmovie where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void RemoveUserGroupRule(string groupName)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("update usergroup set strRule='' where strGroup like '{0}'", groupName);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    #region Actors

    public int AddActor(string strActorImdbId, string strActorName)
    {
      try
      {
        //string strActor = strActorImdbId;
        DatabaseUtility.RemoveInvalidChars(ref strActorImdbId);
        DatabaseUtility.RemoveInvalidChars(ref strActorName);
        
        if (null == m_db)
        {
          return -1;
        }
        string strSQL = string.Empty;

        if (strActorImdbId == Strings.Unknown)
        {
          strSQL = "select * from Actors where strActor like '";
          strSQL += strActorName;
          strSQL += "'";
        }
        else
        {
          strSQL = "select * from Actors where IMDBActorId = '";
          strSQL += strActorImdbId;
          strSQL += "'";
        }
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it but check first if it exists without ImdbId
          int idActor = CheckActorByName(strActorName);
          
          if (idActor != -1)
          {
            strSQL = string.Format("update Actors set IMDBActorId='{0}', strActor = '{1}' where idActor ={2}",
                                  strActorImdbId,
                                  strActorName,
                                  idActor);
            m_db.Execute(strSQL);
            return idActor;
          }
          else
          {
            strSQL = "insert into Actors (idActor, strActor, IMDBActorId) values( NULL, '";
            strSQL += strActorName;
            strSQL += "','";
            strSQL += strActorImdbId;
            strSQL += "')";
            m_db.Execute(strSQL);
            int lActorId = m_db.LastInsertID();
            return lActorId;
          }
        }
        else
        {
          int lActorId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idActor"), out lActorId);
          return lActorId;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void GetActors(ArrayList actors)
    {
      if (m_db == null)
      {
        return;
      }
      try
      {
        actors.Clear();
        SQLiteResultSet results = m_db.Execute("select * from actors");
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          actors.Add(DatabaseUtility.Get(results, iRow, "idActor") + "|" +
                     DatabaseUtility.Get(results, iRow, "strActor") + "|" +
                     DatabaseUtility.Get(results, iRow, "IMDBActorId"));
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetActorByName(string strActorName, ArrayList actors)
    {
      strActorName = DatabaseUtility.RemoveInvalidChars(strActorName);
      if (m_db == null)
      {
        return;
      }
      try
      {
        actors.Clear();
        SQLiteResultSet results = m_db.Execute("select * from Actors where strActor like '%" + strActorName + "%'");
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          actors.Add(DatabaseUtility.Get(results, iRow, "idActor") + "|" +
                     DatabaseUtility.Get(results, iRow, "strActor") + "|" + 
                     DatabaseUtility.Get(results, iRow, "IMDBActorId"));
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private int CheckActorByName(string strActorName)
    {
      //strActorName = DatabaseUtility.RemoveInvalidChars(strActorName);
      if (m_db == null)
      {
        return -1;
      }
      try
      {
        SQLiteResultSet results = m_db.Execute("select * from Actors where strActor like '%" + strActorName + "%'");
        if (results.Rows.Count == 0)
        {
          return -1;
        }
        return Convert.ToInt32(DatabaseUtility.Get(results, 0, "idActor"));
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    // Changed-New added actors by movie ID
    public void GetActorsByMovieID(int idMovie, ref ArrayList actorsByMovieID)
    {
      try
      {
        actorsByMovieID.Clear();
        if (null == m_db)
        {
          return;
        }
        string strSQL =
          String.Format(
            "SELECT actors.idActor, actors.strActor, actors.IMDBActorId, actorlinkmovie.strRole from actors INNER JOIN actorlinkmovie ON actors.idActor = actorlinkmovie.idActor WHERE actorlinkmovie.idMovie={0}",
            idMovie);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            actorsByMovieID.Add(DatabaseUtility.Get(results, i, "actors.idActor") + "|" +
                                DatabaseUtility.Get(results, i, "actors.strActor")  + "|" +
                                DatabaseUtility.Get(results, i, "actors.IMDBActorId") + "|" +
                                DatabaseUtility.Get(results, i, "actorlinkmovie.strRole"));
          }
          return;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    public void AddActorToMovie(int lMovieId, int lActorId, string role)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }

        DatabaseUtility.RemoveInvalidChars(ref role);

        string strSQL = String.Format("select * from actorlinkmovie where idActor={0} and idMovie={1}", lActorId,
                                      lMovieId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into actorlinkmovie (idActor, idMovie, strRole) values( {0},{1},'{2}')", lActorId, lMovieId, role);
          m_db.Execute(strSQL);
        }
        else
        {
          // exists, update it (role only)
          strSQL = String.Format("update actorlinkmovie set strRole = '{0}' where idActor={1} and idMovie={2}", role,lActorId, lMovieId);
          m_db.Execute(strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void DeleteActorFromMovie(int movieId, int actorId)
    {
      try
      {
        m_db.Execute(String.Format("delete from actorlinkmovie where idMovie={0} and idActor={1}", movieId, actorId));
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void DeleteActor(string actorImdbId)
    {
      try
      {
        string actorFiltered = actorImdbId;
        DatabaseUtility.RemoveInvalidChars(ref actorFiltered);
        string sql = String.Format("select * from actors where IMDBActorId='{0}'", actorFiltered);
        SQLiteResultSet results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          return;
        }

        int idactor;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idActor"), out idactor);
        m_db.Execute(sql);

        m_db.Execute(String.Format("delete from actorlinkmovie where idActor={0}", idactor));
        m_db.Execute(String.Format("delete from actors where idActor={0}", idactor));
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void RemoveActorsForMovie(int lMovieId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("delete from actorlinkmovie where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetRoleByMovieAndActorId (int lMovieId, int lActorId)
    {
      try
      {
        if (null == m_db)
        {
          return string.Empty;
        }
        string strSQL =
          String.Format(
            "SELECT strRole from actorlinkmovie WHERE idMovie={0} and idActor={1}", lMovieId, lActorId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          return DatabaseUtility.Get(results, 0, "actorlinkmovie.strRole");
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return string.Empty;
    }
	#endregion

    #region bookmarks

    public void ClearBookMarksOfMovie(string strFilenameAndPath)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        int lPathId, lMovieId;
        int lFileId = GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
        if (lFileId < 0)
        {
          return;
        }
        string strSQL = String.Format("delete from bookmark where idFile={0}", lFileId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void AddBookMarkToMovie(string strFilenameAndPath, float fTime)
    {
      try
      {
        int lPathId, lMovieId;
        int lFileId = GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
        if (lFileId < 0)
        {
          return;
        }
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("select * from bookmark where idFile={0} and fPercentage='{1}'", lFileId, fTime);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          return;
        }

        strSQL = String.Format("insert into bookmark (idBookmark, idFile, fPercentage) values(NULL,{0},'{1}')", lFileId,
                               fTime);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetBookMarksForMovie(string strFilenameAndPath, ref ArrayList bookmarks)
    {
      bookmarks.Clear();
      try
      {
        int lPathId, lMovieId;
        int lFileId = GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
        if (lFileId < 0)
        {
          return;
        }
        bookmarks.Clear();
        if (null == m_db)
        {
          return;
        }

        string strSQL = String.Format("select * from bookmark where idFile={0} order by fPercentage", lFileId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          double fTime = Convert.ToDouble(DatabaseUtility.Get(results, iRow, "fPercentage"));
          bookmarks.Add(fTime);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    #region MovieInfo

    public void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      if (strFilenameAndPath.Length == 0)
      {
        return;
      }
      int lMovieId = GetMovie(strFilenameAndPath, true);
      if (lMovieId < 0)
      {
        return;
      }
      details.ID = lMovieId;
      SetMovieInfoById(lMovieId, ref details);

      string strPath, strFileName;
      DatabaseUtility.Split(strFilenameAndPath, out strPath, out strFileName);
      details.Path = strPath;
      details.File = strFileName;
    }

    // Changed cast fix
    public void SetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      try
      {
        details.ID = lMovieId;

        IMDBMovie details1 = details;
        IMDBMovie existingDetails = new IMDBMovie();
        VideoDatabase.GetMovieInfoById(details1.ID, ref existingDetails);
        
        // Cast
        string strLine = details1.Cast;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Cast = strLine;
        // Director
        strLine = details1.Director;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Director = strLine;
        // add director Id
        int lDirector = - 1;
        if (details1.DirectorID < 1 && !string.IsNullOrEmpty(details1.Director))
        {
          lDirector = AddActor("", details1.Director);
          AddActorToMovie(details1.ID, lDirector, GUILocalizeStrings.Get(199).Replace(":", string.Empty));
          
          if (!CheckMovieImdbId(details1.IMDBNumber))
          {
            // Add actors from cast
            ArrayList vecActors = new ArrayList();
            ArrayList vecRoles = new ArrayList();
            if (details1.Cast != Strings.Unknown)
            {
              string castFix = details1.Cast.Replace("''", "'");
              char[] splitter = { '\n', ',' };
              string[] actors = castFix.Split(splitter);

              for (int i = 0; i < actors.Length; ++i)
              {
                int pos = actors[i].IndexOf(" as ");
                string actor = actors[i];
                string role = string.Empty;
                if (pos >= 0)
                {
                  if (actor.Length >= pos + 4)
                  {
                    role = actor.Substring(pos + 4);
                  }
                  actor = actors[i].Substring(0, pos);
                }
                actor = actor.Trim();
                role = role.Trim();
                int lActorId = AddActor(string.Empty, actor);
                vecActors.Add(lActorId);
                vecRoles.Add(role);
              }
            }
            for (int i = 0; i < vecActors.Count; i++)
            {
              AddActorToMovie(lMovieId, (int)vecActors[i], (string)vecRoles[i]);
            }
          }
        }
        else
        {
          lDirector = details1.DirectorID;
        }
        // Plot
        strLine = details1.Plot;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Plot = strLine;
        // User Review
        strLine = details1.UserReview;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.UserReview = strLine;
        // Plot outline
        strLine = details1.PlotOutline;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.PlotOutline = strLine;
        // Tagline
        strLine = details1.TagLine;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.TagLine = strLine;
        // Cover
        strLine = details1.ThumbURL;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.ThumbURL = strLine;
        // Fanart
        strLine = details1.FanartURL;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.FanartURL = strLine;
        // Date Added
        details1.DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        // Date Watched
        if (details1.DateWatched == string.Empty && existingDetails.ID >= 0)
        {
          details1.DateWatched = existingDetails.DateWatched;
        }
        if (string.IsNullOrEmpty(details1.DateWatched))
        {
          details1.DateWatched = "0001-01-01 00:00:00";
        }
        // Watched status
        if (details1.Watched < 1 && existingDetails.ID >= 0)
        {
          details1.Watched = existingDetails.Watched;
        }
        // Search string
        strLine = details1.SearchString;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.SearchString = strLine;
        // Title
        strLine = details1.Title;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Title = strLine;
        // Votes
        strLine = details1.Votes;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Votes = strLine;
        // Writers
        strLine = details1.WritingCredits;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.WritingCredits = strLine;
        // Genres
        //Clear old genres link for movie
        RemoveGenresForMovie(lMovieId);
        strLine = details1.Genre;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Genre = strLine;
        // IMDB Movie ID
        strLine = details1.IMDBNumber;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.IMDBNumber = strLine;
        // MPAA Rating
        strLine = details1.MPARating;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.MPARating = strLine;
        // Studios
        strLine = details1.Studios;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Studios = strLine;
        // Country
        strLine = details1.Country;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Country = strLine;
        // Language
        strLine = details1.Language;
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Language = strLine;
        // add all genres
        string szGenres = details.Genre;
        ArrayList vecGenres = new ArrayList();
        if (szGenres != Strings.Unknown)
        {
          if (szGenres.IndexOf("/") >= 0)
          {
            Tokens f = new Tokens(szGenres, new[] {'/'});
            foreach (string strGenre in f)
            {
              strGenre.Trim();
              int lGenreId = AddGenre(strGenre);
              vecGenres.Add(lGenreId);
            }
          }
          else
          {
            string strGenre = details.Genre;
            strGenre.Trim();
            int lGenreId = AddGenre(strGenre);
            vecGenres.Add(lGenreId);
          }
        }
        
        for (int i = 0; i < vecGenres.Count; ++i)
        {
          AddGenreToMovie(lMovieId, (int)vecGenres[i]);
        }

        string strRating = String.Format("{0}", details1.Rating);
        if (strRating == "")
        {
          strRating = "0.0";
        }
        string strSQL = String.Format("select * from movieinfo where idmovie={0}", lMovieId);
        //	Log.Error("dbs:{0}", strSQL);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // Insert new movie info - no date watched update
          strSQL =
            String.Format(
              "insert into movieinfo ( idMovie, idDirector, strPlotOutline, strPlot, strTagLine, strVotes, fRating, strCast, strCredits, iYear, strGenre, strPictureURL, strTitle, IMDBID, mpaa, runtime, iswatched, strUserReview, strFanartURL, strDirector, dateAdded, studios, country, language) values({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9},'{10}','{11}','{12}','{13}','{14}',{15},{16},'{17}','{18}','{19}','{20}','{21}','{22}','{23}')",
              lMovieId, lDirector, details1.PlotOutline,
              details1.Plot, details1.TagLine,
              details1.Votes, strRating,
              details1.Cast, details1.WritingCredits,
              details1.Year, details1.Genre,
              details1.ThumbURL, details1.Title,
              details1.IMDBNumber, details1.MPARating, details1.RunTime, details1.Watched, details1.UserReview,
              details1.FanartURL, details1.Director, details1.DateAdded, details1.Studios, details1.Country, details1.Language);

          //			Log.Error("dbs:{0}", strSQL);
          SQLiteResultSet result = m_db.Execute(strSQL);
        }
        else
        {
          // Update movie info (no dateAdded update)
          strSQL =
            String.Format(
              "update movieinfo set idDirector={0}, strPlotOutline='{1}', strPlot='{2}', strTagLine='{3}', strVotes='{4}', fRating='{5}', strCast='{6}',strCredits='{7}', iYear={8}, strGenre='{9}', strPictureURL='{10}', strTitle='{11}', IMDBID='{12}', mpaa='{13}', runtime={14}, iswatched={15} , strUserReview='{16}', strFanartURL='{17}' , strDirector ='{18}', dateWatched='{19}', studios = '{20}', country = '{21}', language = '{22}' where idMovie={23}",
              lDirector, details1.PlotOutline,
              details1.Plot, details1.TagLine,
              details1.Votes, strRating,
              details1.Cast, details1.WritingCredits,
              details1.Year, details1.Genre,
              details1.ThumbURL, details1.Title,
              details1.IMDBNumber,
              details1.MPARating, details1.RunTime,
              details1.Watched, details1.UserReview, 
              details1.FanartURL, details1.Director, 
              details1.DateWatched ,details1.Studios,
              details1.Country, details1.Language,
              lMovieId);

          //		Log.Error("dbs:{0}", strSQL);
          SQLiteResultSet result = m_db.Execute(strSQL);
        }
        // Double single quota fix (after scan and executing DatabaseUtility.RemoveInvalidChars method, movie info can contain double single quota
        // which looks ugly on screen and also produce wrong cover thumb filename which leads to duplication of cover
        // for the same movie ie. That's Life{x}L.jpg  and That''s Life{x}L.jpg, this is only visible and reproducable after scan,
        // after is OK)
        {
          details1.PlotOutline = details1.PlotOutline.Replace("''", "'");
          details1.Plot = details1.Plot.Replace("''", "'");
          details1.TagLine = details1.TagLine.Replace("''", "'");
          details1.WritingCredits = details1.WritingCredits.Replace("''", "'");
          details1.Genre = details1.Genre.Replace("''", "'");
          details1.Title = details1.Title.Replace("''", "'");
          details1.UserReview = details1.UserReview.Replace("''", "'");
          if (details1.Director != null) details1.Director = details1.Director.Replace("''", "'");
          details1.Studios = details1.Studios.Replace("''", "'");
          details1.Country = details1.Country.Replace("''", "'");
          details1.Language = details1.Language.Replace("''", "'");

        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} src:{2}, stack:{1}", ex.Message, ex.Source, ex.StackTrace);
        Open();
      }
    }

    public void DeleteMovieInfo(string strFileNameAndPath)
    {
      int lMovieId = GetMovie(strFileNameAndPath, false);
      if (lMovieId < 0)
      {
        return;
      }
      DeleteMovieInfoById(lMovieId);
    }

    public void DeleteMovieInfoById(long lMovieId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        if (lMovieId == -1)
        {
          return;
        }
        Log.Info("Removing movie:{0}", lMovieId);

        // Delete movie file stop time data
        ArrayList files = new ArrayList();
        VideoDatabase.GetFilesForMovie((int)lMovieId, ref files);

        foreach (string file in files)
        {
          int fileId = VideoDatabase.GetFileId(file);
          VideoDatabase.DeleteMovieStopTime(fileId);
        }

        RemoveUserGroupsForMovie((int) lMovieId);

        string strSQL = String.Format("delete from genrelinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from actorlinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movieinfo where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from files where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movie where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool HasMovieInfo(string strFilenameAndPath)
    {
      if (string.IsNullOrEmpty(strFilenameAndPath))
      {
        return false;
      }
      try
      {
        if (null == m_db)
        {
          return false;
        }
        int lMovieId = GetMovie(strFilenameAndPath, false);
        if (lMovieId < 0)
        {
          return false;
        }
        string strSQL = String.Format("select * from movieinfo where movieinfo.idmovie={0}", lMovieId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public int GetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      int lMovieId = GetMovie(strFilenameAndPath, false);
      if (lMovieId < 0)
      {
        return -1;
      }

      if (!HasMovieInfo(strFilenameAndPath))
      {
        return -1;
      }
      GetMovieInfoById(lMovieId, ref details);
      return lMovieId;
    }

    // Changed Added DirectorID, userrev, fanart
    public void GetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      try
      {
        string strSQL = String.Format(
          "select * from movieinfo,movie,path where path.idpath=movie.idpath and movie.idMovie=movieinfo.idMovie and movieinfo.idmovie={0}",
          lMovieId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        details.Rating = (float)Double.Parse(DatabaseUtility.Get(results, 0, "movieinfo.fRating"));
        if (details.Rating > 10.0f)
        {
          details.Rating /= 10.0f;
        }
        details.Director = DatabaseUtility.Get(results, 0, "movieinfo.strDirector").Replace("''", "'");
        // Add directorID
        try
        {
          details.DirectorID = Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.idDirector"));
        }
        catch (Exception)
        {
          details.DirectorID = -1;
        }
        details.WritingCredits = DatabaseUtility.Get(results, 0, "movieinfo.strCredits").Replace("''", "'");
        details.TagLine = DatabaseUtility.Get(results, 0, "movieinfo.strTagLine").Replace("''", "'");
        details.PlotOutline = DatabaseUtility.Get(results, 0, "movieinfo.strPlotOutline").Replace("''", "'");
        details.Plot = DatabaseUtility.Get(results, 0, "movieinfo.strPlot").Replace("''", "'");
        // Added user review
        details.UserReview = DatabaseUtility.Get(results, 0, "movieinfo.strUserReview").Replace("''", "'");
        details.Votes = DatabaseUtility.Get(results, 0, "movieinfo.strVotes");
        details.Cast = DatabaseUtility.Get(results, 0, "movieinfo.strCast").Replace("''", "'");
        details.Year = Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.iYear"));
        details.Genre = DatabaseUtility.Get(results, 0, "movieinfo.strGenre").Trim();
        details.ThumbURL = DatabaseUtility.Get(results, 0, "movieinfo.strPictureURL");
        // Fanart
        details.FanartURL = DatabaseUtility.Get(results, 0, "movieinfo.strFanartURL");
        // Date Added
        details.DateAdded = DatabaseUtility.Get(results, 0, "movieinfo.dateAdded");
        // Date Watched
        details.DateWatched = DatabaseUtility.Get(results, 0, "movieinfo.dateWatched");
        details.Title = DatabaseUtility.Get(results, 0, "movieinfo.strTitle").Replace("''", "'");
        details.Path = DatabaseUtility.Get(results, 0, "path.strPath");
        details.DVDLabel = DatabaseUtility.Get(results, 0, "movie.discid");
        details.IMDBNumber = DatabaseUtility.Get(results, 0, "movieinfo.IMDBID");
        lMovieId = Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.idMovie"));
        details.SearchString = String.Format("{0}", details.Title);
        details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
        details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
        details.RunTime = Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.runtime"));
        details.Watched = Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.iswatched"));
        details.ID = lMovieId;
        details.Studios = DatabaseUtility.Get(results, 0, "movieinfo.studios");
        details.Country = DatabaseUtility.Get(results, 0, "movieinfo.country");
        details.Language = DatabaseUtility.Get(results, 0, "movieinfo.language");
        
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetWatched(IMDBMovie details)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        if (details.ID < 0)
        {
          return;
        }
        string strSQL = String.Format("update movieinfo set iswatched={0} where idMovie={1}", details.Watched,
                                      details.ID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetDateWatched(IMDBMovie details)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        if (details.ID < 0)
        {
          return;
        }

        if (string.IsNullOrEmpty(details.DateWatched))
          details.DateWatched = "0001-01-01 00:00:00";

        string strSQL = String.Format("update movieinfo set dateWatched='{0}' where idMovie={1}", details.DateWatched,
                                      details.ID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }
    #endregion

    #region Movie Resume

    public void DeleteMovieStopTime(int iFileId)
    {
      try
      {
        string sql = String.Format("delete from resume where idFile={0}", iFileId);
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetMovieStopTime(int iFileId)
    {
      try
      {
        string sql = string.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          return 0;
        }
        int stoptime;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "stoptime"), out stoptime);
        return stoptime;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public void SetMovieStopTime(int iFileId, int stoptime)
    {
      try
      {
        string sql = String.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          sql = String.Format("insert into resume ( idResume,idFile,stoptime) values(NULL,{0},{1})",
                              iFileId, stoptime);
        }
        else
        {
          sql = String.Format("update resume set stoptime={0} where idFile={1}",
                              stoptime, iFileId);
        }
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private byte[] FromHexString(string s)
    {
      byte[] bytes = new byte[s.Length / 2];
      for (int i = 0; i < bytes.Length; i++)
      {
        bytes[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);
      }
      return bytes;
    }

    private string ToHexString(byte[] bytes)
    {
      char[] hexDigits = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
      char[] chars = new char[bytes.Length * 2];
      for (int i = 0; i < bytes.Length; i++)
      {
        int b = bytes[i];
        chars[i * 2] = hexDigits[b >> 4];
        chars[i * 2 + 1] = hexDigits[b & 0xF];
      }
      return new string(chars);
    }

    public int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData)
    {
      resumeData = null;

      try
      {
        string sql = string.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          return 0;
        }
        int stoptime;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "stoptime"), out stoptime);
        string resumeString = DatabaseUtility.Get(results, 0, "resumeData");
        resumeData = new byte[resumeString.Length / 2];
        FromHexString(resumeString).CopyTo(resumeData, 0);
        return stoptime;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData)
    {
      try
      {
        string sql = String.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);

        string resumeString = "-";
        if (resumeData != null)
        {
          resumeString = ToHexString(resumeData);
        }
        if (results.Rows.Count == 0)
        {
          sql = String.Format("insert into resume ( idResume,idFile,stoptime,resumeData) values(NULL,{0},{1},'{2}')",
                              iFileId, stoptime, resumeString);
        }
        else
        {
          sql = String.Format("update resume set stoptime={0},resumeData='{1}' where idFile={2}",
                              stoptime, resumeString, iFileId);
        }
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetMovieDuration(int iMovieId)
    {
      try
      {
        int duration = 0;
        string sql = string.Format("select * from movie where idMovie={0}", iMovieId);
        SQLiteResultSet results = m_db.Execute(sql);

        if (results.Rows.Count == 0)
        {
          return duration;
        }

        if (Int32.TryParse(DatabaseUtility.Get(results, 0, "iduration"), out duration))
        {
          return duration;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public int GetVideoDuration(int iFileId)
    {
      try
      {
        string sql = string.Format("select * from duration where idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return 0;
        }
        
        int duration;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "duration"), out duration);
        return duration;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public void SetVideoDuration(int iFileId, int duration)
    {
      try
      {
        if (duration > 0)
        {
          string sql = String.Format("select * from duration where idFile={0}", iFileId);
          SQLiteResultSet results = m_db.Execute(sql);

          if (results.Rows.Count == 0)
          {
            sql = String.Format("insert into duration ( idDuration,idFile,duration) values(NULL,{0},{1})",
                                iFileId, duration);
          }
          else
          {
            sql = String.Format("update duration set duration={0} where idFile={1}",
                                duration, iFileId);
          }
          m_db.Execute(sql);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetMovieDuration(int iMovieId, int duration)
    {
      try
      {
        if (duration > 0)
        {
          string sql = sql = String.Format("update movie set iduration={0} where idMovie={1}",
                                           duration, iMovieId);
          m_db.Execute(sql);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetMovieWatchedStatus(int idMovie, bool watched, int percent)
    {
      try
      {
        string sql = String.Format("select * from movie where idMovie={0}", idMovie);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count != 0)
        {
          int iWatched = 0;
          
          if (watched)
          {
            iWatched = 1;
          }
          
          sql = String.Format("update movie set watched={0}, iwatchedPercent = {1} where idMovie={2}",
                              iWatched, percent, idMovie);
        }
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    /// <summary>
    /// Increase times watched by 1
    /// </summary>
    /// <param name="idMovie"></param>
    public void MovieWatchedCountIncrease(int idMovie)
    {
      try
      {
        string sql = String.Format("select * from movie where idMovie={0}", idMovie);
        SQLiteResultSet results = m_db.Execute(sql);
        int watchedCount = 0;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "movie.timeswatched"), out watchedCount);

        if (results.Rows.Count != 0)
        {
          watchedCount++;
          sql = String.Format("update movie set timeswatched = {0} where idMovie={1}",
                              watchedCount, idMovie);
          m_db.Execute(sql);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetMovieWatchedCount(int movieId , int watchedCount)
    {
      try
      {
        string sql = String.Format("select * from movie where idMovie={0}", movieId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count != 0)
        {
          sql = String.Format("update movie set timeswatched = {0} where idMovie={1}",
                              watchedCount, movieId);
          m_db.Execute(sql);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool GetMovieWatchedStatus(int idMovie, out int percent, out int timesWatched)
    {
      percent = 0;
      timesWatched = 0;
      
      try
      {
        string sql = String.Format("select * from movie where idMovie={0}", idMovie);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return false;
        }
        
        int watched;
        int.TryParse(DatabaseUtility.Get(results, 0, "watched"), out watched);
        int.TryParse(DatabaseUtility.Get(results, 0, "iwatchedPercent"), out percent);
        int.TryParse(DatabaseUtility.Get(results, 0, "timeswatched"), out timesWatched);
        
        if (watched != 0)
        {
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    #endregion

    #region Movie

    public void DeleteMovie(string strFilenameAndPath)
    {
      if (Directory.Exists(strFilenameAndPath))
        DeleteMoviesInFolder(strFilenameAndPath);
      else
        DeleteSingleMovie(strFilenameAndPath);
    }

    private void DeleteMoviesInFolder(string strPath)
    {
      SQLiteResultSet results = m_db.Execute("SELECT idPath,strPath FROM path WHERE strPath LIKE '" + strPath + "%'");

      SortedDictionary<string, string> pathList = new SortedDictionary<string, string>();
      for (int i = 0; i < results.Rows.Count; ++i)
        pathList.Add(DatabaseUtility.Get(results, i, 0), DatabaseUtility.Get(results, i, 1));

      foreach (KeyValuePair<string, string> kvp in pathList)
      {
        results = m_db.Execute("SELECT strFilename FROM files WHERE idPath=" + kvp.Key);
        for (int j = 0; j < results.Rows.Count; ++j)
          DeleteSingleMovie(kvp.Value + DatabaseUtility.Get(results, j, 0));
      }
    }

    private void DeleteSingleMovie(string strFilenameAndPath)
    {
      try
      {
        int lPathId;
        int lMovieId;
        if (null == m_db)
        {
          return;
        }
        if (GetFile(strFilenameAndPath, out lPathId, out lMovieId, false) < 0)
        {
          return;
        }

        ClearBookMarksOfMovie(strFilenameAndPath);

        // Delete files attached to the movie
        string strSQL = String.Format(
          "select * from path,files where path.idPath=files.idPath and files.idmovie={0} order by strFilename",
          lMovieId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          int iFileId;
          Int32.TryParse(DatabaseUtility.Get(results, i, "files.idFile"), out iFileId);
          DeleteFile(iFileId);
        }
        // Delete covers
        IMDBMovie movieDetails = new IMDBMovie();
        GetMovieInfoById(lMovieId, ref movieDetails);
        FanArt.DeleteCovers(movieDetails.Title, movieDetails.ID);
        // Delete fanarts
        FanArt.DeleteFanarts(movieDetails.ID);
        // Delete user groups
        RemoveUserGroupsForMovie(movieDetails.ID);
        //
        strSQL = String.Format("delete from genrelinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from actorlinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movieinfo where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int AddMovie(string strFilenameAndPath, bool bHassubtitles)
    {
      if (m_db == null)
      {
        return -1;
      }
      try
      {
        string strPath, strFileName;

        DatabaseUtility.Split(strFilenameAndPath, out strPath, out strFileName);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        int lMovieId = GetMovie(strFilenameAndPath, false);
        if (lMovieId < 0)
        {
          int lPathId = AddPath(strPath);

          if (lPathId < 0)
          {
            return -1;
          }
          int iHasSubs = 0;
          if (bHassubtitles)
          {
            iHasSubs = 1;
          }
          string strSQL = String.Format(
            "insert into movie (idMovie, idPath, hasSubtitles, discid) values( NULL, {0}, {1},'')", lPathId, iHasSubs);

          m_db.Execute(strSQL);
          lMovieId = m_db.LastInsertID();
          AddFile(lMovieId, lPathId, strFileName);
        }
        else
        {
          int lPathId = GetPath(strPath);
          if (lPathId < 0)
          {
            return -1;
          }
          AddFile(lMovieId, lPathId, strFileName);
        }
        return lMovieId;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int GetMovie(string strFilenameAndPath, bool bExact)
    {
      int lPathId;
      int lMovieId;
      if (GetFile(strFilenameAndPath, out lPathId, out lMovieId, bExact) < 0)
      {
        return -1;
      }
      return lMovieId;
    }

    // Changed - user review added
    public void GetMovies(ref ArrayList movies)
    {
      try
      {
        movies.Clear();
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format(
          "select * from movie,movieinfo,path where movieinfo.idmovie=movie.idmovie and movie.idpath=path.idpath");

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f)
          {
            details.Rating /= 10.0f;
          }
          details.Director = DatabaseUtility.Get(results, iRow, "movieinfo.strDirector");
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          // Added user review
          details.UserReview = DatabaseUtility.Get(results, iRow, "movieinfo.strUserReview");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          // Fanart
          details.FanartURL = DatabaseUtility.Get(results, iRow, "movieinfo.strFanartURL");
          // Date added
          details.DateAdded = DatabaseUtility.Get(results, iRow, "movieinfo.dateAdded");
          // Date Watched
          details.DateWatched = DatabaseUtility.Get(results, iRow, "movieinfo.dateWatched");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, iRow, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, iRow, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
          details.Country = DatabaseUtility.Get(results, iRow, "movieinfo.country");
          details.Language = DatabaseUtility.Get(results, iRow, "movieinfo.language");
          // Add directorID
          try 
          {
            details.DirectorID = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idDirector"));
          }
          catch (Exception)
          {
            details.DirectorID = -1;
          }
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetMovieId(string strFilenameAndPath)
    {
      int lMovieId = GetMovie(strFilenameAndPath, true);
      return lMovieId;
    }

    public bool HasSubtitle(string strFilenameAndPath)
    {
      try
      {
        if (null == m_db)
        {
          return false;
        }
        int lMovieId = GetMovie(strFilenameAndPath, false);
        if (lMovieId < 0)
        {
          return false;
        }
        string strSQL = String.Format("select * from movie where movie.idMovie={0}", lMovieId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }
        int lHasSubs;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "hasSubtitles"), out lHasSubs);
        if (lHasSubs != 0)
        {
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public void SetThumbURL(int lMovieId, string thumbURL)
    {
      DatabaseUtility.RemoveInvalidChars(ref thumbURL);
      try
      {
        if (null == m_db)
        {
          return;
        }

        string strSQL = String.Format("update movieinfo set strPictureURL='{0}' where idMovie={1}", thumbURL, lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    // Fanart add
    public void SetFanartURL(int lMovieId, string fanartURL)
    {
      DatabaseUtility.RemoveInvalidChars(ref fanartURL);
      try
      {
        if (null == m_db)
        {
          return;
        }

        string strSQL = String.Format("update movieinfo set strFanartURL='{0}' where idMovie={1}", fanartURL, lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetDVDLabel(int lMovieId, string strDVDLabel1)
    {
      string strDVDLabel = strDVDLabel1;
      DatabaseUtility.RemoveInvalidChars(ref strDVDLabel);
      try
      {
        if (null == m_db)
        {
          return;
        }

        string strSQL = String.Format("update movie set discid='{0}' where idMovie={1}", strDVDLabel1, lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    #region Movie Queries

    public void GetYears(ArrayList years)
    {
      if (m_db == null)
      {
        return;
      }
      try
      {
        years.Clear();
        SQLiteResultSet results = m_db.Execute("select * from movieinfo");
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          string strYear = DatabaseUtility.Get(results, iRow, "iYear");
          bool bAdd = true;
          for (int i = 0; i < years.Count; ++i)
          {
            if (strYear == (string)years[i])
            {
              bAdd = false;
            }
          }
          if (bAdd)
          {
            years.Add(strYear);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    // Changed - added user review
    public void GetMoviesByGenre(string strGenre1, ref ArrayList movies)
    {
      try
      {
        string strGenre = strGenre1;
        DatabaseUtility.RemoveInvalidChars(ref strGenre);

        movies.Clear();
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format(
          "select * from genrelinkmovie,genre,movie,movieinfo,path where path.idpath=movie.idpath and genrelinkmovie.idGenre=genre.idGenre and genrelinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and genre.strGenre='{0}'",
          strGenre);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f)
          {
            details.Rating /= 10.0f;
          }
          details.Director = DatabaseUtility.Get(results, iRow, "movieinfo.strDirector");
          int directorId = -1;
          Int32.TryParse(DatabaseUtility.Get(results, iRow, "movieinfo.idDirector"), out directorId);
          details.DirectorID = directorId;
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          // Added user review
          details.UserReview = DatabaseUtility.Get(results, iRow, "movieinfo.strUserReview");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          // Fanart
          details.FanartURL = DatabaseUtility.Get(results, iRow, "movieinfo.strFanartURL");
          // Date added
          details.DateAdded = DatabaseUtility.Get(results, iRow, "movieinfo.dateAdded");
          // Date watched
          details.DateWatched = DatabaseUtility.Get(results, iRow, "movieinfo.dateWatched");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, iRow, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, iRow, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
          details.Country = DatabaseUtility.Get(results, iRow, "movieinfo.country");
          details.Language = DatabaseUtility.Get(results, iRow, "movieinfo.language");
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetMoviesByUserGroup(string strUserGroup, ref ArrayList movies)
    {
      try
      {
        DatabaseUtility.RemoveInvalidChars(ref strUserGroup);

        movies.Clear();
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format(
          "select * from usergrouplinkmovie,usergroup,movie,movieinfo,path where path.idpath=movie.idpath and usergrouplinkmovie.idGroup=usergroup.idGroup and usergrouplinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and usergroup.strGroup='{0}'",
          strUserGroup);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f)
          {
            details.Rating /= 10.0f;
          }
          details.Director = DatabaseUtility.Get(results, iRow, "movieinfo.strDirector");
          int directorId = -1;
          Int32.TryParse(DatabaseUtility.Get(results, iRow, "movieinfo.idDirector"), out directorId);
          details.DirectorID = directorId;
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          // Added user review
          details.UserReview = DatabaseUtility.Get(results, iRow, "movieinfo.strUserReview");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          // Fanart
          details.FanartURL = DatabaseUtility.Get(results, iRow, "movieinfo.strFanartURL");
          // Date added
          details.DateAdded = DatabaseUtility.Get(results, iRow, "movieinfo.dateAdded");
          // Date watched
          details.DateWatched = DatabaseUtility.Get(results, iRow, "movieinfo.dateWatched");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, iRow, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, iRow, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
          details.Country = DatabaseUtility.Get(results, iRow, "movieinfo.country");
          details.Language = DatabaseUtility.Get(results, iRow, "movieinfo.language");
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    // Changed - added user review
    public void GetMoviesByActor(string strActor1, ref ArrayList movies)
    {
      try
      {
        string strActor = strActor1;
        DatabaseUtility.RemoveInvalidChars(ref strActor);

        movies.Clear();
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format(
          "select * from actorlinkmovie,actors,movie,movieinfo,path where path.idpath=movie.idpath and actors.idActor=actorlinkmovie.idActor and actorlinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and actors.stractor='{0}'",
          strActor);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f)
          {
            details.Rating /= 10.0f;
          }
          details.Director = DatabaseUtility.Get(results, iRow, "movieinfo.strDirector");
          int directorId = -1;
          Int32.TryParse(DatabaseUtility.Get(results, iRow, "movieinfo.idDirector"), out directorId);
          details.DirectorID = directorId;
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          // Added user review
          details.UserReview = DatabaseUtility.Get(results, iRow, "movieinfo.strUserReview");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          // Fanart
          details.FanartURL = DatabaseUtility.Get(results, iRow, "movieinfo.strFanartURL");
          // Date Added
          details.DateAdded = DatabaseUtility.Get(results, iRow, "movieinfo.dateAdded");
          // Date Watched
          details.DateWatched = DatabaseUtility.Get(results, iRow, "movieinfo.dateWatched");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, iRow, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, iRow, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
          details.Country = DatabaseUtility.Get(results, iRow, "movieinfo.country");
          details.Language = DatabaseUtility.Get(results, iRow, "movieinfo.language");
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    // Changed - added user review
    public void GetMoviesByYear(string strYear, ref ArrayList movies)
    {
      try
      {
        int iYear;
        Int32.TryParse(strYear, out iYear);

        movies.Clear();
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format(
          "select * from movie,movieinfo,path where path.idpath=movie.idpath and movieinfo.idmovie=movie.idmovie and movieinfo.iYear={0}",
          iYear);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f)
          {
            details.Rating /= 10.0f;
          }
          details.Director = DatabaseUtility.Get(results, iRow, "movieinfo.strDirector");
          int directorId = -1;
          Int32.TryParse(DatabaseUtility.Get(results, iRow, "movieinfo.idDirector"), out directorId);
          details.DirectorID = directorId;
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          // Added user review
          details.UserReview = DatabaseUtility.Get(results, iRow, "movieinfo.strUserReview");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          // Fanart
          details.FanartURL = DatabaseUtility.Get(results, iRow, "movieinfo.strFanartURL");
          // Date Added
          details.DateAdded = DatabaseUtility.Get(results, iRow, "movieinfo.dateAdded");
          // Date Watched
          details.DateWatched = DatabaseUtility.Get(results, iRow, "movieinfo.dateWatched");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, iRow, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, iRow, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
          details.Country = DatabaseUtility.Get(results, iRow, "movieinfo.country");
          details.Language = DatabaseUtility.Get(results, iRow, "movieinfo.language");
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetMoviesByPath(string strPath1, ref ArrayList movies)
    {
      try
      {
        string strPath = strPath1;
        if (strPath.Length > 0)
        {
          if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          {
            strPath = strPath.Substring(0, strPath.Length - 1);
          }
        }

        DatabaseUtility.RemoveInvalidChars(ref strPath);


        movies.Clear();
        if (null == m_db)
        {
          return;
        }
        int lPathId = GetPath(strPath);
        if (lPathId < 0)
        {
          return;
        }
        string strSQL =
          String.Format("select * from files,movieinfo where files.idpath={0} and files.idMovie=movieinfo.idMovie",
                        lPathId);

        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          int lMovieId;
          Int32.TryParse(DatabaseUtility.Get(results, iRow, "files.idMovie"), out lMovieId);
          details.SearchString = String.Format("{0}", details.Title);
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.File = DatabaseUtility.Get(results, iRow, "files.strFilename");
          details.ID = lMovieId;
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    // Changed - added user review
    public void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable,
                                  bool genreTable, bool usergroupTable)
    {
      movies = new ArrayList();
      try
      {
        if (null == m_db)
        {
          return;
        }
        SQLiteResultSet results = GetResults(sql);
        IMDBMovie movie;

        for (int i = 0; i < results.Rows.Count; i++)
        {
          movie = new IMDBMovie();
          SQLiteResultSet.Row fields = results.Rows[i];
          if (actorTable && !movieinfoTable)
          {
            movie.Actor = fields.fields[1];
            movie.ActorID = (int)Math.Floor(0.5d + Double.Parse(fields.fields[0]));
          }
          if (genreTable && !movieinfoTable)
          {
            movie.SingleGenre = fields.fields[1];
            movie.GenreID = (int)Math.Floor(0.5d + Double.Parse(fields.fields[0]));
          }
          if (usergroupTable && !movieinfoTable)
          {
            movie.SingleUserGroup = fields.fields[1];
            movie.UserGroupID = (int)Math.Floor(0.5d + Double.Parse(fields.fields[0]));
          }
          if (movieinfoTable)
          {
            movie.Rating = (float)Double.Parse(DatabaseUtility.Get(results, i, "movieinfo.fRating"));
            if (movie.Rating > 10.0f)
            {
              movie.Rating /= 10.0f;
            }
            movie.Director = DatabaseUtility.Get(results, i, "movieinfo.strDirector");
            int directorId = -1;
            Int32.TryParse(DatabaseUtility.Get(results, i, "movieinfo.idDirector"), out directorId);
            movie.DirectorID = directorId;
            movie.WritingCredits = DatabaseUtility.Get(results, i, "movieinfo.strCredits");
            movie.TagLine = DatabaseUtility.Get(results, i, "movieinfo.strTagLine");
            movie.PlotOutline = DatabaseUtility.Get(results, i, "movieinfo.strPlotOutline");
            movie.Plot = DatabaseUtility.Get(results, i, "movieinfo.strPlot");
            // Added user review
            movie.UserReview = DatabaseUtility.Get(results, i, "movieinfo.strUserReview");
            movie.Votes = DatabaseUtility.Get(results, i, "movieinfo.strVotes");
            movie.Cast = DatabaseUtility.Get(results, i, "movieinfo.strCast");
            movie.Year = Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.iYear"));
            movie.Genre = DatabaseUtility.Get(results, i, "movieinfo.strGenre").Trim();
            movie.ThumbURL = DatabaseUtility.Get(results, i, "movieinfo.strPictureURL");
            // Fanart
            movie.FanartURL = DatabaseUtility.Get(results, i, "movieinfo.strFanartURL");
            // Date added
            movie.DateAdded = DatabaseUtility.Get(results, i, "movieinfo.dateAdded");
            // Date watched
            movie.DateWatched = DatabaseUtility.Get(results, i, "movieinfo.dateWatched");
            movie.Title = DatabaseUtility.Get(results, i, "movieinfo.strTitle");
            movie.Path = DatabaseUtility.Get(results, i, "path.strPath");
            movie.DVDLabel = DatabaseUtility.Get(results, i, "movie.discid");
            movie.IMDBNumber = DatabaseUtility.Get(results, i, "movieinfo.IMDBID");
            long lMovieId = Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.idMovie"));
            movie.SearchString = String.Format("{0}", movie.Title);
            movie.CDLabel = DatabaseUtility.Get(results, i, "path.cdlabel");
            movie.MPARating = DatabaseUtility.Get(results, i, "movieinfo.mpaa");
            movie.RunTime = Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.runtime"));
            movie.Watched = Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.iswatched"));
            movie.ID = (int)lMovieId;
            movie.Studios= DatabaseUtility.Get(results, i, "movieinfo.studios");
            movie.Country = DatabaseUtility.Get(results, i, "movieinfo.country");
            movie.Language = DatabaseUtility.Get(results, i, "movieinfo.language");
            // FanArt search need this (for database GUI view)
            // Share view is handled in GUIVideoFIles class)
            ArrayList files = new ArrayList();
            GetFilesForMovie(movie.ID, ref files);
            if (files.Count > 0)
            {
              // We need only first file if there is multiple files for one movie, fanart class will handle filename
              movie.File = files[0].ToString();
            }
          }
          movies.Add(movie);
        }

        return;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel)
    {
      if (movieDetails == null)
      {
        return;
      }
      try
      {
        string sql = String.Format("select idPath from path where cdlabel = '{0}'", movieDetails.CDLabel);
        SQLiteResultSet results = m_db.Execute(sql);
        int idPath;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idPath"), out idPath);
        sql = String.Format("update path set cdlabel = '{0}' where idPath = '{1}'", CDlabel, idPath);
        results = m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetDVDLabel(string strFile)
    {
      string cdlabel = string.Empty;
      if (Util.Utils.IsDVD(strFile))
      {
        cdlabel = Util.Utils.GetDriveSerial(strFile);
      }
      return cdlabel;
    }

    public SQLiteResultSet GetResults(string sql)
    {
      try
      {
        if (null == m_db)
        {
          return null;
        }
        SQLiteResultSet results = m_db.Execute(sql);
        return results;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return null;
    }

    #endregion

    #region ActorInfo

    // Changed thumbURl added - IMDBActorID added
    public void SetActorInfo(int idActor, IMDBActor actor)
    {
      //"CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("select * from actorinfo where idActor ={0}", idActor);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL =
            String.Format(
              "insert into actorinfo (idActor , dateofbirth , placeofbirth , minibio , biography, thumbURL, IMDBActorID, dateofdeath , placeofdeath ) values( {0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
              idActor, 
              DatabaseUtility.RemoveInvalidChars(actor.DateOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.MiniBiography),
              DatabaseUtility.RemoveInvalidChars(actor.Biography),
              DatabaseUtility.RemoveInvalidChars(actor.ThumbnailUrl),
              DatabaseUtility.RemoveInvalidChars(actor.IMDBActorID),
              DatabaseUtility.RemoveInvalidChars(actor.DateOfDeath),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfDeath));
          m_db.Execute(strSQL);
        }
        else
        {
          // exists, modify it
          strSQL =
            String.Format(
              "update actorinfo set dateofbirth='{1}', placeofbirth='{2}' , minibio='{3}' , biography='{4}' , thumbURL='{5}' , IMDBActorID='{6}', dateofdeath='{7}', placeofdeath='{8}' where idActor={0}",
              idActor, 
              DatabaseUtility.RemoveInvalidChars(actor.DateOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.MiniBiography),
              DatabaseUtility.RemoveInvalidChars(actor.Biography),
              DatabaseUtility.RemoveInvalidChars(actor.ThumbnailUrl),
              DatabaseUtility.RemoveInvalidChars(actor.IMDBActorID),
              DatabaseUtility.RemoveInvalidChars(actor.DateOfDeath),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfDeath));
          m_db.Execute(strSQL);
          RemoveActorInfoMovie(idActor);
        }
        for (int i = 0; i < actor.Count; ++i)
        {
          AddActorInfoMovie(idActor, actor[i]);
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    public void AddActorInfoMovie(int idActor, IMDBActor.IMDBActorMovie movie)
    {
      //idActor, idDirector , strPlotOutline , strPlot , strTagLine , strVotes , fRating ,strCast ,strCredits , iYear , strGenre , strPictureURL , strTitle , IMDBID , mpaa ,runtime , iswatched , role 
      string movieTitle = DatabaseUtility.RemoveInvalidChars(movie.MovieTitle);
      //string moviePlot = DatabaseUtility.RemoveInvalidChars(movie.MoviePlot);
      //string movieCast = DatabaseUtility.RemoveInvalidChars(movie.MovieCast);
      //string movieGenre = DatabaseUtility.RemoveInvalidChars(movie.MovieGenre);
      string movieRole = DatabaseUtility.RemoveInvalidChars(movie.Role);
      //string movieCredits = DatabaseUtility.RemoveInvalidChars(movie.MovieCredits);
      try
      {
        if (null == m_db)
        {
          return;
        }
        // Changed-added IMDBid value
        string strSQL =
          String.Format(
            "insert into actorinfomovies (idActor, idDirector , strPlotOutline , strPlot , strTagLine , strVotes , fRating ,strCast ,strCredits , iYear , strGenre , strPictureURL , strTitle , IMDBID , mpaa ,runtime , iswatched , role  ) values( {0} ,{1} ,'{2}' , '{3}' , '{4}' , '{5}' , '{6}' ,'{7}' ,'{8}' , {9} , '{10}' , '{11}' , '{12}' , '{13}' ,'{14}',{15} , {16} , '{17}' )",
                                          idActor, 
                                          -1, 
                                          "",
                                          "", 
                                          "", 
                                          "", 
                                          "",
                                          "",
                                          "", 
                                          1900,
                                          "",
                                          "",
                                          "", 
                                          movie.MovieImdbID, 
                                          "", 
                                          -1, 
                                          0,
                                          movieRole);
        m_db.Execute(strSQL);
        // populate IMDB Movies
        if (CheckMovieImdbId(movie.MovieImdbID))
        {
          strSQL = String.Format("select * from IMDBMovies where idIMDB='{0}'", movie.MovieImdbID);
          SQLiteResultSet results = m_db.Execute(strSQL);
          
          if (results.Rows.Count == 0)
          {
            strSQL = String.Format("insert into IMDBMovies (  idIMDB, idTmdb, strPlot, strCast, strCredits, iYear, strGenre, strPictureURL, strTitle, mpaa) values( '{0}' ,'{1}' ,'{2}' , '{3}' , '{4}' , {5} , '{6}' ,'{7}' ,'{8}' , '{9}')",
                                      movie.MovieImdbID,
                                      "", // Not used (TMDBid)
                                      "",
                                      "",
                                      "",
                                      movie.Year,
                                      "",
                                      "",
                                      movieTitle,
                                      "");
            m_db.Execute(strSQL);
          }
          else
          {
            strSQL = String.Format("update IMDBMovies set iYear={0}, strTitle='{1}' where idIMDB='{2}'",
                                      movie.Year,
                                      movieTitle,
                                      movie.MovieImdbID);
            m_db.Execute(strSQL);
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    public void RemoveActorInfoMovie(int actorId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL = String.Format("delete from actorinfomovies where idActor={0}", actorId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    // Changed get thumbnailURL - IMDBActorID - IMDBID for movies
    public IMDBActor GetActorInfo(int idActor)
    {
      //"CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text
      try
      {
        if (null == m_db)
        {
          return null;
        }
        string strSql = String.Format(
            "select actorinfo.biography, actorinfo.dateofbirth, actorinfo.dateofdeath, actorinfo.minibio, actors.strActor, actorinfo.placeofbirth, actorinfo.placeofdeath, actorinfo.thumbURL, actors.IMDBActorID, actorinfo.idActor from actors,actorinfo where actors.idActor=actorinfo.idActor and actors.idActor ={0}", idActor);
        SQLiteResultSet results = m_db.Execute(strSql);
        if (results.Rows.Count != 0)
        {
          IMDBActor actor = new IMDBActor();
          actor.Biography = DatabaseUtility.Get(results, 0, "actorinfo.biography".Replace("''", "'"));
          actor.DateOfBirth = DatabaseUtility.Get(results, 0, "actorinfo.dateofbirth".Replace("''", "'"));
          actor.DateOfDeath = DatabaseUtility.Get(results, 0, "actorinfo.dateofdeath".Replace("''", "'"));
          actor.MiniBiography = DatabaseUtility.Get(results, 0, "actorinfo.minibio".Replace("''", "'"));
          actor.Name = DatabaseUtility.Get(results, 0, "actors.strActor".Replace("''", "'"));
          actor.PlaceOfBirth = DatabaseUtility.Get(results, 0, "actorinfo.placeofbirth".Replace("''", "'"));
          actor.PlaceOfDeath = DatabaseUtility.Get(results, 0, "actorinfo.placeofdeath".Replace("''", "'"));
          actor.ThumbnailUrl = DatabaseUtility.Get(results, 0, "actorinfo.thumbURL");
          actor.IMDBActorID = DatabaseUtility.Get(results, 0, "actors.IMDBActorID");
          actor.ID = Convert.ToInt32(DatabaseUtility.Get(results, 0, "actorinfo.idActor"));

          strSql = String.Format("select * from actorinfomovies where idActor ={0}", idActor);
          results = m_db.Execute(strSql);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            string imdbId = DatabaseUtility.Get(results, i, "IMDBID");
            strSql = String.Format("select * from IMDBMovies where idIMDB='{0}'", imdbId);
            SQLiteResultSet resultsImdb = m_db.Execute(strSql);

            IMDBActor.IMDBActorMovie movie = new IMDBActor.IMDBActorMovie();
            movie.ActorID = Convert.ToInt32(DatabaseUtility.Get(results, i, "idActor"));
            movie.Role = DatabaseUtility.Get(results, i, "role");
            if (resultsImdb.Rows.Count != 0)
            {
              // Added IMDBid
              movie.MovieTitle = DatabaseUtility.Get(resultsImdb, 0, "strTitle");
              movie.Year = Int32.Parse(DatabaseUtility.Get(resultsImdb, 0, "iYear"));
              movie.MovieImdbID = DatabaseUtility.Get(resultsImdb, 0, "idIMDB");
              movie.MoviePlot = DatabaseUtility.Get(resultsImdb, 0, "strPlot");
              movie.MovieCover = DatabaseUtility.Get(resultsImdb, 0, "strPictureURL");
              movie.MovieGenre = DatabaseUtility.Get(resultsImdb, 0, "strGenre");
              movie.MovieCast = DatabaseUtility.Get(resultsImdb, 0, "strCast");
              movie.MovieCredits = DatabaseUtility.Get(resultsImdb, 0, "strCredits");
              movie.MovieRuntime = Int32.Parse(DatabaseUtility.Get(results, i, "runtime")); // Not used
              movie.MovieMpaaRating = DatabaseUtility.Get(resultsImdb, 0, "mpaa");
            }
            actor.Add(movie);
          }
          return actor;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return null;
    }

    public string GetActorImdbId(int idActor)
    {
      try
      {
        if (null == m_db)
        {
          return null;
        }
        string strSql = String.Format("select actors.IMDBActorID from actors where actors.idActor ={0}", idActor);
        SQLiteResultSet results = m_db.Execute(strSql);
        if (results.Rows.Count != 0)
        {
          return DatabaseUtility.Get(results, 0, "actors.IMDBActorID");
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return string.Empty;
    }

    #endregion

    #region Video thumbnail blacklisting

    public bool IsVideoThumbBlacklisted(string path)
    {
      try
      {
        if (null == m_db)
        {
          return false;
        }
        FileInfo fileInfo = null;
        try
        {
          fileInfo = new FileInfo(path);
        }
        catch
        {
          //ignore
        }

        if (fileInfo == null || !fileInfo.Exists)
        {
          return false;
        }

        path = path.Trim();
        DatabaseUtility.RemoveInvalidChars(ref path);

        string strSQL = String.Format(
          "select idVideoThumbBList from VideoThumbBList where strPath = '{0}' and strExpires > '{1:yyyyMMdd}' and strFileDate = '{2:s}' and strFileSize = '{3}'",
          path, DateTime.Now, fileInfo.LastWriteTimeUtc, fileInfo.Length);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public int VideoThumbBlacklist(string path, DateTime expiresOn)
    {
      try
      {
        if (null == m_db)
        {
          return -1;
        }
        FileInfo fileInfo = null;

        try
        {
          fileInfo = new FileInfo(path);
        }
        catch
        {
          //ignore
        }

        if (fileInfo == null || !fileInfo.Exists)
        {
          return -1;
        }

        path = path.Trim();
        DatabaseUtility.RemoveInvalidChars(ref path);

        string strSQL = String.Format("select idVideoThumbBList from VideoThumbBList where strPath = '{0}'", path);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL =
            String.Format(
              "insert into VideoThumbBList (idVideoThumbBList, strPath, strExpires, strFileDate, strFileSize) values( NULL, '{0}', '{1:yyyyMMdd}', '{2:s}', '{3}')",
              path, expiresOn, fileInfo.LastWriteTimeUtc, fileInfo.Length);
          m_db.Execute(strSQL);
          int id = m_db.LastInsertID();
          RemoveExpiredVideoThumbBlacklistEntries();
          return id;
        }
        else
        {
          int id = -1;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idVideoThumbBList"), out id);
          if (id != -1)
          {
            strSQL =
              String.Format(
                "update VideoThumbBList set strExpires='{1:yyyyMMdd}', strFileDate='{2:s}', strFileSize='{3}' where idVideoThumbBList={0}",
                id, expiresOn, fileInfo.LastWriteTimeUtc, fileInfo.Length);
            m_db.Execute(strSQL);
          }
          return id;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public bool VideoThumbRemoveFromBlacklist(string path)
    {
      try
      {
        if (null == m_db)
        {
          return false;
        }

        path = path.Trim();
        DatabaseUtility.RemoveInvalidChars(ref path);

        string strSQL = String.Format("select idVideoThumbBList from VideoThumbBList where strPath = '{0}'", path);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          int id = -1;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idVideoThumbBList"), out id);
          if (id != -1)
          {
            strSQL = String.Format("delete from VideoThumbBList where idVideoThumbBList={0}", id);
            m_db.Execute(strSQL);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public void RemoveExpiredVideoThumbBlacklistEntries()
    {
      try
      {
        if (null == m_db)
        {
          return;
        }

        string strSQL = String.Format("delete from VideoThumbBList where strExpires <= '{0:yyyyMMdd}'", DateTime.Now);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void RemoveAllVideoThumbBlacklistEntries()
    {
      try
      {
        if (null == m_db)
        {
          return;
        }

        m_db.Execute("delete from VideoThumbBList");
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    public void ExecuteSQL (string strSql, out bool error)
    {
      error = false;
      
      try
      {
        if (m_db == null)
        {
          return;
        }
        
        m_db.Execute(strSql);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        error = true;
        Open();
      }
      return;
    }

    public ArrayList ExecuteRuleSQL(string strSql, string fieldName, out bool error)
    {
      ArrayList values = new ArrayList();
      error = false;

      try
      {
        if (m_db == null)
        {
          return values;
        }

        SQLiteResultSet results = m_db.Execute(strSql);

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          values.Add(DatabaseUtility.Get(results, iRow, fieldName));
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        error = true;
        Open();
      }
      values.Sort();
      return values;
    }

    public bool CheckMovieImdbId(string id)
    {
      // IMDBtt search check tt number, must be exactly 9 chars with leading zeros if needed
      if (id == null || id.Length != 9)
      {
        return false;
      }
      // Final IMDBtt check
      Match ttNo = Regex.Match(id, @"tt[\d]{7}?");
      if (!ttNo.Success)
      {
        return false;
      }
      return true;
    }

    public bool CheckActorImdbId(string id)
    {
      // IMDBnm search check nm number, must be exactly 9 chars with leading zeros if needed
      if (id == null || id.Length != 9)
      {
        return false;
      }
      // Final IMDBtt check
      Match ttNo = Regex.Match(id, @"nm[\d]{7}?");
      if (!ttNo.Success)
      {
        return false;
      }
      return true;
    }

    public void ImportNfo(string nfoFile)
    {
      IMDBMovie movie = new IMDBMovie();
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(nfoFile);
        
        if (doc.DocumentElement != null)
        {
          int id = 0;

          XmlNodeList movieList = doc.DocumentElement.SelectNodes("/movie");
          
          if (movieList == null)
          {
            return;
          }

          foreach (XmlNode nodeMovie in movieList)
          {
            string genre = string.Empty;
            string cast = string.Empty;
            string path = string.Empty;
            string fileName = string.Empty;
            
            #region nodes

            XmlNode nodeTitle = nodeMovie.SelectSingleNode("title");
            XmlNode nodeRating = nodeMovie.SelectSingleNode("rating");
            XmlNode nodeYear = nodeMovie.SelectSingleNode("year");
            XmlNode nodeDuration = nodeMovie.SelectSingleNode("runtime");
            XmlNode nodePlotShort = nodeMovie.SelectSingleNode("outline");
            XmlNode nodePlot = nodeMovie.SelectSingleNode("plot");
            XmlNode nodeTagline = nodeMovie.SelectSingleNode("tagline");
            XmlNode nodeDirector = nodeMovie.SelectSingleNode("director");
            XmlNode nodeDirectorImdb = nodeMovie.SelectSingleNode("directorimdb");
            XmlNode nodeImdbNumber = nodeMovie.SelectSingleNode("imdb");
            XmlNode nodeMpaa = nodeMovie.SelectSingleNode("mpaa");
            XmlNode nodeTop250 = nodeMovie.SelectSingleNode("top250");
            XmlNode nodeVotes = nodeMovie.SelectSingleNode("votes");
            XmlNode nodeStudio = nodeMovie.SelectSingleNode("studio");
            XmlNode nodePlayCount = nodeMovie.SelectSingleNode("playcount");
            XmlNode nodeWatched = nodeMovie.SelectSingleNode("watched");
            XmlNode nodeFanart = nodeMovie.SelectSingleNode("fanart");
            XmlNode nodePoster = nodeMovie.SelectSingleNode("thumb");
            XmlNode nodeLanguage = nodeMovie.SelectSingleNode("language");
            XmlNode nodeCountry = nodeMovie.SelectSingleNode("country");
            XmlNode nodeReview = nodeMovie.SelectSingleNode("review");
            XmlNode nodeCredits = nodeMovie.SelectSingleNode("credits");
            
            
            #endregion

            #region Genre

            XmlNodeList genres = nodeMovie.SelectNodes("genres/genre");
            
            foreach (XmlNode nodeGenre in genres)
            {
              if (nodeGenre.InnerText != null)
              {
                if (genre.Length > 0)
                {
                  genre += " / ";
                }
                genre += nodeGenre.InnerText;
              }
            }

            if (string.IsNullOrEmpty(genre))
            {
              XmlNode nodeGenre = nodeMovie.SelectSingleNode("genre");
              genre = nodeGenre.InnerText;
            }

            // Genre
            movie.Genre = genre;
            
            #endregion

            #region Credits (Writers)

            // Writers
            if (nodeCredits != null)
            {
              movie.WritingCredits = nodeCredits.InnerText;
            }
            #endregion

            #region Moviefiles
            
            // Get path from *.nfo file)
            Util.Utils.Split(nfoFile, out path, out fileName);
            // Movie filename to search from gathered files from nfo path
            fileName = Util.Utils.GetFilename(fileName, true);
            // Get all video files from nfo path
            ArrayList files = new ArrayList();
            GetVideoFiles(path, ref files);

            foreach (String file in files)
            {
              string tmpFile = string.Empty;
              string tmpPath = string.Empty;
              // Read filename
              Util.Utils.Split(file, out tmpPath, out tmpFile);
              // Remove extension
              tmpFile = Util.Utils.GetFilename(tmpFile, true);
              // Remove stack endings (CD1...)
              Util.Utils.RemoveStackEndings(ref tmpFile);
              // Check and add to vdb and get movieId
              if (tmpFile.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
              {
                id = VideoDatabase.AddMovie(file, true);
                movie.ID = id;
              }
            }

            #endregion

            #region DateAdded

            movie.DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            #endregion

            #region Title

            // Title
            if (nodeTitle != null)
            {
              movie.Title = nodeTitle.InnerText;
            }

            #endregion

            #region Language

            // Title
            if (nodeLanguage != null)
            {
              movie.Language = nodeLanguage.InnerText;
            }

            #endregion

            #region Country

            // Title
            if (nodeCountry != null)
            {
              movie.Country = nodeCountry.InnerText;
            }

            #endregion

            #region IMDB number

            // IMDB number
            if (nodeImdbNumber != null)
            {
              if (CheckMovieImdbId(nodeImdbNumber.InnerText))
              {
                movie.IMDBNumber = nodeImdbNumber.InnerText;
              }
            }

            #endregion

            #region CD/DVD labels

            // CD label
            movie.CDLabel = string.Empty;

            // DVD label
            movie.DVDLabel = string.Empty;

            #endregion

            #region Director

            // Director
            string dirImdb = string.Empty;
            if (nodeDirectorImdb != null)
            {
              dirImdb = nodeDirector.InnerText;
              
              if (!CheckActorImdbId(dirImdb))
              {
                dirImdb = string.Empty;
              }
            }
            if (nodeDirector != null)
            {
              movie.Director = nodeDirector.InnerText;
              movie.DirectorID = VideoDatabase.AddActor(dirImdb, movie.Director);
            }
            #endregion

            #region Studio

            // Studio
            if (nodeStudio != null)
            {
              movie.Studios = nodeStudio.InnerText;
            }

            #endregion

            #region MPAA

            // MPAA
            if (nodeMpaa != null)
            {
              movie.MPARating = nodeMpaa.InnerText;
            }
            else
            {
              movie.MPARating = "NR";
            }
            
            #endregion
            
            #region Plot/Short plot

            // Plot
            if (nodePlot != null)
            {
              movie.Plot = nodePlot.InnerText;
            }
            else
            {
              movie.Plot = string.Empty;
            }
            // Short plot
            if (nodePlotShort != null)
            {
              movie.PlotOutline = nodePlotShort.InnerText;
            }
            else
            {
              movie.PlotOutline = string.Empty;
            }

            #endregion

            #region Review

            // Title
            if (nodeReview != null)
            {
              movie.UserReview = nodeReview.InnerText;
            }

            #endregion

            #region Rating (n.n/10)

            // Rating
            if (nodeRating != null)
            {
              double rating = 0;
              if (Double.TryParse(nodeRating.InnerText, out rating))
              {
                movie.Rating = (float) rating;
                
                if (movie.Rating > 10.0f)
                {
                  movie.Rating /= 10.0f;
                }
              }
            }

            #endregion

            #region Duration

            // Duration
            if (nodeDuration != null)
            {
              int runtime = 0;
              if (Int32.TryParse(nodeDuration.InnerText, out runtime))
              {
                movie.RunTime = runtime;
              }
              else
              {
                string regex = "(?<h>[0-9]*)h.(?<m>[0-9]*)";
                MatchCollection mc = Regex.Matches(nodeDuration.InnerText, regex, RegexOptions.Singleline);
                if (mc.Count > 0)
                {
                  foreach (Match m in mc)
                  {
                    int hours = 0;
                    Int32.TryParse(m.Groups["h"].Value, out hours);
                    int minutes = 0;
                    Int32.TryParse(m.Groups["m"].Value, out minutes);
                    hours = hours*60;
                    minutes = hours + minutes;
                    movie.RunTime = minutes;
                  }
                }
              }
            }
            else
            {
              movie.RunTime = 0;
            }

            #endregion
            
            #region Tagline

            // Tagline
            if (nodeTagline != null)
            {
              movie.TagLine = nodeTagline.InnerText;
            }

            #endregion

            #region TOP250

            // Top250
            if (nodeTop250 != null)
            {
              int top250 = 0;
              Int32.TryParse(nodeTop250.InnerText, out top250);
              movie.Top250 = top250;
            }
            else
            {
              movie.Top250 = 0;
            }


            #endregion

            #region votes

            // Votes
            if (nodeVotes != null)
            {
              movie.Votes = nodeVotes.InnerText;
            }

            #endregion

            #region Watched/watched count

            // Watched
            if (nodeWatched != null)
            {
              if (nodeWatched.InnerText == "true" || nodeWatched.InnerText == "1")
              {
                movie.Watched = 1;
                VideoDatabase.SetMovieWatchedStatus(movie.ID, true, 100);
              }
              else
              {
                movie.Watched = 0;
                VideoDatabase.SetMovieWatchedStatus(movie.ID, false, 0);
              }
            }
            // Watched count
            if (nodePlayCount != null)
            {
              int watchedCount = 0;
              Int32.TryParse(nodePlayCount.InnerText, out watchedCount);
              SetMovieWatchedCount(movie.ID, watchedCount);
            }

            #endregion

            #region Year

            // Year
            if (nodeYear != null)
            {
              int year = 0;
              Int32.TryParse(nodeYear.InnerText, out year);
              movie.Year = year;
            }

            #endregion

            #region poster

            // Poster
            if (nodePoster != null)
            {
              string thumbJpgFile = path + @"\" + nodePoster.InnerText;
              string thumbTbnFile = path + @"\" + nodePoster.InnerText;
              string titleExt = movie.Title + "{" + id + "}";
              
              if (File.Exists(thumbJpgFile))
              {
                movie.ThumbURL = thumbJpgFile;
                string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
                string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);

                if (Util.Picture.CreateThumbnail(thumbJpgFile, largeCoverArt, (int)Thumbs.ThumbLargeResolution,
                                                 (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsSmall))
                {
                  Util.Picture.CreateThumbnail(thumbJpgFile, coverArt, (int)Thumbs.ThumbResolution,
                                                (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsLarge);
                }
              }
              else if (File.Exists(thumbTbnFile))
              {
                movie.ThumbURL = thumbTbnFile;
                string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
                string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
                
                if (Util.Picture.CreateThumbnail(thumbTbnFile, largeCoverArt, (int)Thumbs.ThumbLargeResolution,
                                                 (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsSmall))
                {
                  Util.Picture.CreateThumbnail(thumbTbnFile, coverArt, (int)Thumbs.ThumbResolution,
                                                (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsLarge);
                }
              }
              else
              {
                if (nodePoster.InnerText.StartsWith("http:"))
                {
                  try
                  {
                    string imageUrl = nodePoster.InnerText;
                    if (imageUrl.Length > 0)
                    {
                      string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
                      string coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
                      if (!File.Exists(coverArtImage))
                      {
                        string imageExtension = Path.GetExtension(imageUrl);
                        if (imageExtension == string.Empty)
                        {
                          imageExtension = ".jpg";
                        }
                        string temporaryFilename = "temp";
                        temporaryFilename += imageExtension;
                        Util.Utils.FileDelete(temporaryFilename);

                        Util.Utils.DownLoadAndCacheImage(imageUrl, temporaryFilename);
                        
                        if (File.Exists(temporaryFilename))
                        {
                          if (Util.Picture.CreateThumbnail(temporaryFilename, largeCoverArtImage, (int)Thumbs.ThumbLargeResolution,
                                                           (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsSmall))
                          {
                            Util.Picture.CreateThumbnail(temporaryFilename, coverArtImage, (int)Thumbs.ThumbResolution,
                                                         (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsLarge);
                          }
                        }

                        Util.Utils.FileDelete(temporaryFilename);
                      }
                    }
                  }
                  catch (Exception) { }
                  movie.ThumbURL = nodePoster.InnerText;
                }
              }
            }

            #endregion

            #region Fanart

            // Fanart
            XmlNodeList fanartNodeList = nodeMovie.SelectNodes("fanart/thumb");

            int faIndex = 0;

            foreach (XmlNode fanartNode in fanartNodeList)
            {
              if (fanartNode != null)
              {
                string faFile = path + @"\" + fanartNode.InnerText;
                FanArt fa = new FanArt();
                
                if (File.Exists(faFile))
                {
                  fa.GetLocalFanart(id, "file://" + faFile, faIndex);
                  movie.FanartURL = faFile;
                }
              }
              faIndex ++;
            }

            #endregion

            #region Cast

            // Cast parse
            XmlNodeList actorsList = nodeMovie.SelectNodes("actor");
            foreach (XmlNode nodeActor in actorsList)
            {
              string name = string.Empty;
              string role = string.Empty;
              string actorImdbId = string.Empty;
              string line = string.Empty;
              XmlNode nodeActorName = nodeActor.SelectSingleNode("name");
              XmlNode nodeActorRole = nodeActor.SelectSingleNode("role");
              XmlNode nodeActorImdbId = nodeActor.SelectSingleNode("imdb");

              XmlNode nodeActorBirthDate = nodeActor.SelectSingleNode("birthdate");
              XmlNode nodeActorBirthPlace = nodeActor.SelectSingleNode("birthplace");
              XmlNode nodeActorDeathDate = nodeActor.SelectSingleNode("deathdate");
              XmlNode nodeActorDeathPlace = nodeActor.SelectSingleNode("deathplace");
              XmlNode nodeActorMiniBio = nodeActor.SelectSingleNode("minibiography");
              XmlNode nodeActorBiography= nodeActor.SelectSingleNode("biography");
              XmlNode nodeActorThumbnail = nodeActor.SelectSingleNode("thumb");

              if (nodeActorName != null && nodeActorName.InnerText != null)
              {
                name = nodeActorName.InnerText;
              }
              if (nodeActorRole != null && nodeActorRole.InnerText != null)
              {
                role = nodeActorRole.InnerText;
              }
              if (nodeActorImdbId != null)
              {
                if (CheckActorImdbId(nodeActorImdbId.InnerText))
                {
                  actorImdbId = nodeActorImdbId.InnerText;
                }
              }
              if (!string.IsNullOrEmpty(name))
              {
                if (!string.IsNullOrEmpty(role))
                {
                  line = String.Format("{0} as {1}\n", name, role);
                }
                else
                {
                  line = String.Format("{0}\n", name);
                }
                cast += line;

                int actId = VideoDatabase.AddActor(actorImdbId, name);
                
                VideoDatabase.AddActorToMovie(id, actId, role);

                if (CheckActorImdbId(actorImdbId))
                {
                  IMDBActor info = new IMDBActor();
                  info.IMDBActorID = actorImdbId;
                  
                  if (nodeActorBirthDate != null)
                  {
                    info.DateOfBirth = nodeActorBirthDate.InnerText;
                  }
                  if (nodeActorBirthPlace != null)
                  {
                    info.PlaceOfBirth = nodeActorBirthPlace.InnerText;
                  }
                  if (nodeActorDeathDate != null)
                  {
                    info.DateOfDeath = nodeActorDeathDate.InnerText;
                  }
                  if (nodeActorDeathPlace != null)
                  {
                    info.PlaceOfDeath = nodeActorDeathPlace.InnerText;
                  }
                  if (nodeActorMiniBio != null)
                  {
                    info.MiniBiography = nodeActorMiniBio.InnerText;
                  }
                  if (nodeActorBiography != null)
                  {
                    info.Biography = nodeActorBiography.InnerText;
                  }
                  
                  if (info.DateOfBirth != string.Empty || 
                      info.PlaceOfBirth != string.Empty||
                      info.DateOfDeath != string.Empty ||
                      info.PlaceOfBirth != string.Empty ||
                      info.MiniBiography != string.Empty ||
                      info.Biography != string.Empty)
                  {
                    SetActorInfo(actId, info);
                  }
                }
              }
            }
            // Cast
            movie.Cast = cast;

            #endregion

            VideoDatabase.SetMovieInfoById(id, ref movie);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Error importing nfo file {0}:{1} ", nfoFile, ex);
      }
    }

    public bool MakeNfo (int movieId)
    {
      string moviePath = string.Empty;
      string movieFile = string.Empty;
      ArrayList movieFiles = new ArrayList();
      string nfoFile = string.Empty;
      
      // Get files
      GetFilesForMovie(movieId, ref movieFiles);
      // Wee need only 1 if they are stacked
      movieFile = movieFiles[0].ToString();

      if (!File.Exists(movieFile))
      {
        return false;
      }

      // Split to filename and path
      Util.Utils.Split(movieFile, out moviePath, out movieFile);
      // Check for DVD folder
      if (movieFile.ToUpperInvariant() == "VIDEO_TS.IFO")
      {
        // Remove \VIDEO_TS from directory structure
        string directoryDVD = moviePath.Substring(0, moviePath.LastIndexOf("\\"));
        
        if (Directory.Exists(directoryDVD))
        {
          moviePath = directoryDVD;
        }
      }
      // remove stack endings (CDx..) form filename
      Util.Utils.RemoveStackEndings(ref movieFile);
      // Remove file extension
      movieFile = Util.Utils.GetFilename(movieFile, true);
      // Add nfo extension
      nfoFile = moviePath + @"\" + movieFile + ".nfo";
      Util.Utils.FileDelete(nfoFile);
      
      IMDBMovie movieDetails = new IMDBMovie();
      GetMovieInfoById(movieId, ref movieDetails);
      // Prepare XML
      XmlDocument doc = new XmlDocument();
      XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
      
      // Main tag
      XmlNode mainNode = doc.CreateElement("movie");
      XmlNode subNode;

      #region Movie fields

      // Title
      CreateXmlNode(mainNode, doc, "title", movieDetails.Title);
      //  movie IMDB number
      CreateXmlNode(mainNode, doc, "imdb", movieDetails.IMDBNumber);
      //  Language
      CreateXmlNode(mainNode, doc, "language", movieDetails.Language);
      //  Country
      CreateXmlNode(mainNode, doc, "country", movieDetails.Country);
      //  Year
      CreateXmlNode(mainNode, doc, "year", movieDetails.Year.ToString());
      //  Rating
      CreateXmlNode(mainNode, doc, "rating", movieDetails.Rating.ToString().Replace(",", "."));
      //  Runtime
      CreateXmlNode(mainNode, doc, "runtime", movieDetails.RunTime.ToString());
      // MPAA
      CreateXmlNode(mainNode, doc, "mpaa", movieDetails.MPARating);
      // Votes
      CreateXmlNode(mainNode, doc, "votes", movieDetails.Votes);
      // TOp 250
      CreateXmlNode(mainNode, doc, "top250", movieDetails.Top250.ToString());
      // Studio
      CreateXmlNode(mainNode, doc, "studio", movieDetails.Studios);
      //  Director
      CreateXmlNode(mainNode, doc, "director", movieDetails.Director);
      //  Director imdbId
      CreateXmlNode(mainNode, doc, "directorimdb", GetActorImdbId(movieDetails.ID));
      // Credits
      CreateXmlNode(mainNode, doc, "credits", movieDetails.WritingCredits);
      // Tagline
      CreateXmlNode(mainNode, doc, "tagline", movieDetails.TagLine);
      // Plot outline (short one)
      CreateXmlNode(mainNode, doc, "outline", movieDetails.PlotOutline);
      // Plot - long
      CreateXmlNode(mainNode, doc, "plot", movieDetails.Plot);
      // Review
      CreateXmlNode(mainNode, doc, "review", movieDetails.UserReview);
      // Watched
      string watched = "false";
      
      if (movieDetails.Watched > 0)
      {
        watched = "true";
      }

      CreateXmlNode(mainNode, doc, "watched", watched);
      
      // Watched count
      int percent = 0;
      int watchedCount = 0;
      GetMovieWatchedStatus(movieId, out percent, out watchedCount);
      CreateXmlNode(mainNode, doc, "playcount", watchedCount.ToString());
      
      // Poster
      string titleExt = movieDetails.Title + "{" + movieId + "}";
      string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
      string coverFilename = moviePath + @"\" + movieFile + ".jpg";
      
      if (File.Exists(largeCoverArtImage))
      {
        File.Copy(largeCoverArtImage, coverFilename, true);
        CreateXmlNode(mainNode, doc, "thumb", movieFile + ".jpg");
      }

      // Fanart
      string faFile = string.Empty;
      subNode = doc.CreateElement("fanart");
      
      for (int i = 0; i < 5; i++)
      {
        FanArt.GetFanArtfilename(movieId, i, out faFile);
        string index = string.Empty;

        if (File.Exists(faFile))
        {
          if (i > 0)
          {
            index = i.ToString();
          }

          string faFilename = moviePath + @"\" + movieFile + "-fanart" + index + ".jpg";
          File.Copy(faFile, faFilename, true);
          CreateXmlNode(subNode, doc, "thumb", movieFile + "-fanart" + index + ".jpg");
          
        }
      }
      mainNode.AppendChild(subNode);

      // Genre
      string szGenres = movieDetails.Genre;
      
      if (szGenres.IndexOf("/") >= 0)
      {
        subNode = doc.CreateElement("genres");
        Tokens f = new Tokens(szGenres, new[] { '/' });
        
        foreach (string strGenre in f)
        {
          CreateXmlNode(subNode, doc, "genre", strGenre.Trim());
        }
        
        mainNode.AppendChild(subNode);
      }
      else
      {
        CreateXmlNode(mainNode, doc, "genre", szGenres.Trim());
      }
      
      // Cast
      ArrayList castList = new ArrayList();
      GetActorsByMovieID(movieId, ref castList);

      foreach (string actor in castList)
      {
        IMDBActor actorInfo = new IMDBActor();
        subNode = doc.CreateElement("actor");

        char[] splitter = { '|' };
        string[] temp = actor.Split(splitter);
        actorInfo = GetActorInfo(Convert.ToInt32(temp[0]));

        CreateXmlNode(subNode, doc, "name", temp[1]);
        CreateXmlNode(subNode, doc, "role", temp[3]);
        CreateXmlNode(subNode, doc, "imdb", temp[2]);

        if (actorInfo != null)
        {
          CreateXmlNode(subNode, doc, "thumb", actorInfo.ThumbnailUrl);
          CreateXmlNode(subNode, doc, "birthdate", actorInfo.DateOfBirth);
          CreateXmlNode(subNode, doc, "birthplace", actorInfo.PlaceOfBirth);
          CreateXmlNode(subNode, doc, "deathdate", actorInfo.DateOfDeath);
          CreateXmlNode(subNode, doc, "deathplace", actorInfo.PlaceOfDeath);
          CreateXmlNode(subNode, doc, "minibiography", actorInfo.MiniBiography);
          CreateXmlNode(subNode, doc, "biography", actorInfo.Biography);
        }

        mainNode.AppendChild(subNode);
      }
      
      #endregion

      // End and save
      doc.AppendChild(mainNode);
      doc.InsertBefore(xmldecl, mainNode);
      doc.Save(nfoFile);
      return true;
    }

    private void CreateXmlNode(XmlNode mainNode, XmlDocument doc, string element, string innerTxt)
    {
      XmlNode subNode = doc.CreateElement(element);
      subNode.InnerText = innerTxt;
      mainNode.AppendChild(subNode);
    }

    public void GetVideoFiles(string path, ref ArrayList availableFiles)
    {
      //
      // Count the files in the current directory
      //
      try
      {
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.VideoExtensions);
        ArrayList imagePath = new ArrayList();
        // Thumbs creation spam no1 causing this call
        //
        // Temporary disable thumbcreation
        //
        using (Settings xmlreader = new MPSettings())
        {
          _currentCreateVideoThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
        }
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", false);
        }

        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true);
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            if (item.Label != "..")
            {
              if (item.Path.IndexOf("VIDEO_TS", StringComparison.InvariantCultureIgnoreCase) >= 0)
              {
                string strFile = String.Format(@"{0}\VIDEO_TS.IFO", item.Path);
                availableFiles.Add(strFile);
              }
              else
              {
                GetVideoFiles(item.Path, ref availableFiles);
              }
            }
          }
          else
          {
            bool skipDuplicate = false;

            if (VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
            {
              string filePath = Path.GetDirectoryName(item.Path) + @"\" + Path.GetFileNameWithoutExtension(item.Path);

              if (!imagePath.Contains(filePath))
              {
                imagePath.Add(filePath);
              }
              else
              {
                skipDuplicate = true;
              }
            }
            if (!skipDuplicate)
            {
              availableFiles.Add(item.Path);
            }
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("VideoDatabas: Exception counting video files:{0}", e);
      }
      finally
      {
        // Restore thumbcreation setting
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", _currentCreateVideoThumbs);
        }
      }
    }
    
    public string DatabaseName
    {
      get
      {
        if (m_db != null)
        {
          return m_db.DatabaseName;
        }
        return "";
      }
    }
  }
}