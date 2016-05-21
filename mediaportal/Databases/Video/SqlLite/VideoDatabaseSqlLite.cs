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
using System.Threading;
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

    #region Database init/deinit

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
        //CleanUpDatabase();
        // Update latest movies
        SetLatestMovieProperties();
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

        #region actorinfo table

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
        if (DatabaseUtility.TableColumnExists(m_db, "actorinfo", "lastupdate") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actorinfo\" ADD COLUMN \"lastupdate\" timestamp DEFAULT '0001-01-01 00:00:00'";
          m_db.Execute(strSQL);
        }

        #endregion

        #region Actor table

        if (DatabaseUtility.TableColumnExists(m_db, "actors", "IMDBActorID") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actors\" ADD COLUMN \"IMDBActorID\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }

        #endregion

        #region movieinfo table

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
          strSQL = String.Format("SELECT idMovie, idDirector, actors.strActor FROM movieinfo, actors WHERE idDirector = idActor");
          SQLiteResultSet results = m_db.Execute(strSQL);
          
          // Upgrade director name in movieinfo
          for (int i = 0; i < results.Rows.Count; i++)
          {
            string directorName = DatabaseUtility.Get(results, i, "actors.strActor");
            int movieId = Convert.ToInt32(DatabaseUtility.Get(results, i, "idMovie"));
            strSQL = String.Format("UPDATE movieinfo SET strDirector='{0}' WHERE idMovie = {1}", directorName, movieId);
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
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "lastupdate") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"lastupdate\" timestamp DEFAULT '0001-01-01 00:00:00'";
          m_db.Execute(strSQL);
        }
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "strSortTitle") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"strSortTitle\" TEXT DEFAULT ''";
          m_db.Execute(strSQL);

          // Fill sort title with title
          strSQL = "UPDATE movieinfo SET strSortTitle = strTitle";
          m_db.Execute(strSQL);
        }

        #endregion

        #region Movie table

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
          string strSQL = String.Format("SELECT idMovie, iswatched FROM movieinfo");
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

          strSQL = String.Format("SELECT idMovie FROM movie WHERE watched = 1");
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

        // bdtitle int
        if (DatabaseUtility.TableColumnExists(m_db, "resume", "bdtitle") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"resume\" ADD COLUMN \"bdtitle\" integer DEFAULT 1000";
          m_db.Execute(strSQL);
          watchedUpg = true;
        }

        #endregion

        #region MediaInfo table

        if (DatabaseUtility.TableExists(m_db, "filesmediainfo") == false)
        {
          DatabaseUtility.AddTable(m_db, "filesmediainfo",
                                   "CREATE TABLE filesmediainfo ( idFile integer primary key, videoCodec text, videoResolution text, aspectRatio text, hasSubtitles bool, audioCodec text, audioChannels text)");
        }

        #endregion

        #region Actorlinkmovie table

        if (DatabaseUtility.TableColumnExists(m_db, "actorlinkmovie", "strRole") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"actorlinkmovie\" ADD COLUMN \"strRole\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }

        #endregion

        #region IMDB Movies table

        if (DatabaseUtility.TableExists(m_db, "IMDBmovies") == false)
        {
          DatabaseUtility.AddTable(m_db, "IMDBmovies",
                                   "CREATE TABLE IMDBmovies ( idIMDB text, idTmdb text, strPlot text, strCast text, strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, mpaa text)");
        }

        #endregion

        #region UserGroups table
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
        // User groups description
        if (DatabaseUtility.TableColumnExists(m_db, "usergroup", "strGroupDescription") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"usergroup\" ADD COLUMN \"strGroupDescription\" text DEFAULT ''";
          m_db.Execute(strSQL);
        }
        
        #endregion

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

        string strSql = String.Format("SELECT * FROM path");
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
            string strSqlfile = String.Format("SELECT * FROM files WHERE idPath={0}", idPath);
            SQLiteResultSet resultsFile = m_db.Execute(strSqlfile);
            // Delete data in file related tables
            for (int iRowFiles = 0; iRowFiles < resultsFile.Rows.Count; iRowFiles++)
            {
              int idFile = Int32.Parse(DatabaseUtility.Get(resultsFile, iRowFiles, "idFile"));
              // Bookmark
              strSql = String.Format("DELETE FROM bookmark WHERE idFile={0}", idFile);
              m_db.Execute(strSql);
              cleanedRows += m_db.ChangedRows();
              // Duration
              strSql = String.Format("DELETE FROM duration WHERE idFile={0}", idFile);
              m_db.Execute(strSql);
              cleanedRows += m_db.ChangedRows();
              // Resume
              strSql = String.Format("DELETE FROM resume WHERE idFile={0}", idFile);
              m_db.Execute(strSql);
              cleanedRows += m_db.ChangedRows();
            }
            // Delete files
            strSql = String.Format("DELETE FROM files WHERE idPath={0}", idPath);
            m_db.Execute(strSql);
            cleanedRows += m_db.ChangedRows();
            // Delete movies
            strSql = String.Format("DELETE FROM movie WHERE idPath={0}", idPath);
            m_db.Execute(strSql);
            cleanedRows += m_db.ChangedRows();
            // Delete path
            strSql = String.Format("DELETE FROM path WHERE idPath={0}", idPath);
            m_db.Execute(strSql);
            cleanedRows += m_db.ChangedRows();
          }
        }
        Log.Info("Cleaned up " + cleanedRows + " rows for unwanted paths.");

        // Find all files without not existing path link
        strSql = String.Format("SELECT * FROM files WHERE files.idPath NOT IN (SELECT idPath FROM path)");
        SQLiteResultSet results = m_db.Execute(strSql);
        Log.Info("Found " + results.Rows.Count + " files without path link. Cleaning files related tables.");

        cleanedRows = 0;

        // Delete data in file related tables
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          int idFile = Int32.Parse(DatabaseUtility.Get(results, iRow, "files.idFile"));
          // Bookmark
          strSql = String.Format("DELETE FROM bookmark WHERE idFile={0}", idFile);
          m_db.Execute(strSql);
          cleanedRows += m_db.ChangedRows();
          // Duration
          strSql = String.Format("DELETE FROM duration WHERE idFile={0}", idFile);
          m_db.Execute(strSql);
          cleanedRows += m_db.ChangedRows();
          // Resume
          strSql = String.Format("DELETE FROM resume WHERE idFile={0}", idFile);
          m_db.Execute(strSql);
          cleanedRows += m_db.ChangedRows();
        }
        Log.Info("Cleaned up " + cleanedRows + " rows for tables without file link.");

        // Delete files without path link
        strSql = String.Format("DELETE FROM files WHERE files.idPath NOT IN (SELECT idPath FROM path)");
        m_db.Execute(strSql);
        Log.Info("Clean up files (no path link): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete path without any file link
        strSql = String.Format("DELETE FROM path WHERE path.idPath NOT IN (SELECT idPath FROM files)");
        m_db.Execute(strSql);
        Log.Info("Clean up path (no file link): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete movies without path link
        strSql = String.Format("DELETE FROM movie WHERE movie.idPath NOT IN (SELECT idPath FROM path)");
        m_db.Execute(strSql);
        Log.Info("Clean up movie (no path link): " + m_db.ChangedRows() + " rows affected.");

        // Delete actorinfo without link to actorId
        strSql = String.Format("DELETE FROM actorinfo WHERE actorinfo.idActor NOT IN (SELECT idActor FROM actors)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorinfo (no actorId link to actors): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete actorlinkmovie without link to actorId 
        strSql = String.Format("DELETE FROM actorlinkmovie where actorlinkmovie.idActor NOT IN (SELECT idActor FROM actors)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorlinkmovie (no actorId link to actors): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete actorlinkmovie without link to the movie
        strSql = String.Format("DELETE FROM actorlinkmovie WHERE actorlinkmovie.idMovie NOT IN (SELECT idMovie FROM movie)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorlinkmovie (no movie link): " + m_db.ChangedRows() + " rows affected.");

        // Delete actorinfomovies without link to the actorId in actors
        strSql = String.Format("DELETE FROM actorinfomovies WHERE actorinfomovies.idActor NOT IN (SELECT idActor FROM actors)");
        m_db.Execute(strSql);
        Log.Info("Clean up actorinfomovies (no actorId link to actors): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete movieinfo without link to the movie
        strSql = String.Format("DELETE FROM movieinfo WHERE movieinfo.idMovie NOT IN (SELECT idMovie FROM movie)");
        m_db.Execute(strSql);
        Log.Info("Clean up movieinfo (no movie link): " + m_db.ChangedRows() + " rows affected.");
        
        // Delete genrelinkmovie without link to the movie
        strSql = String.Format("DELETE FROM genrelinkmovie WHERE genrelinkmovie.idMovie NOT IN (SELECT idMovie FROM movie)");
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
                               "CREATE TABLE usergroup ( idGroup integer primary key, strGroup text, strRule text, strGroupDescription)");
      DatabaseUtility.AddTable(m_db, "usergrouplinkmovie",
                               "CREATE TABLE usergrouplinkmovie ( idGroup integer, idMovie integer)");
      DatabaseUtility.AddTable(m_db, "movie",
                               "CREATE TABLE movie ( idMovie integer primary key, idPath integer, hasSubtitles integer, discid text, watched bool, timeswatched integer, iduration integer)");
      DatabaseUtility.AddTable(m_db, "movieinfo",
                               "CREATE TABLE movieinfo ( idMovie integer, idDirector integer, strDirector text, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text, mpaa text,runtime integer, iswatched integer, strUserReview text, strFanartURL text, dateAdded timestamp, dateWatched timestamp, studios text, country text, language text, lastupdate timestamp, strSortTitle text)");
      DatabaseUtility.AddTable(m_db, "actorlinkmovie",
                               "CREATE TABLE actorlinkmovie ( idActor integer, idMovie integer, strRole text)");
      DatabaseUtility.AddTable(m_db, "actors",
                               "CREATE TABLE actors ( idActor integer primary key, strActor text, IMDBActorID text)");
      DatabaseUtility.AddTable(m_db, "path",
                               "CREATE TABLE path ( idPath integer primary key, strPath text, cdlabel text)");
      DatabaseUtility.AddTable(m_db, "files",
                               "CREATE TABLE files ( idFile integer primary key, idPath integer, idMovie integer,strFilename text)");
      DatabaseUtility.AddTable(m_db, "resume",
                               "CREATE TABLE resume ( idResume integer primary key, idFile integer, stoptime integer, resumeData blob, bdtitle integer)");
      DatabaseUtility.AddTable(m_db, "duration",
                               "CREATE TABLE duration ( idDuration integer primary key, idFile integer, duration integer)");
      DatabaseUtility.AddTable(m_db, "actorinfo",
                               "CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text, thumbURL text, IMDBActorID text, dateofdeath text, placeofdeath text, lastupdate timestamp)");
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
      Log.Debug("VideodatabaseSqllite AddFile:{0}", strFileName);

      // GetMediaInfoThread and starting to play a file can run simultaneously and both of them use addfile method
      lock (typeof(VideoDatabaseSqlLite))
      {
        if (m_db == null)
        {
          return -1;
        }
        try
        {
          int lFileId = -1;
          strFileName = strFileName.Trim();

        string strSQL = String.Format("SELECT * FROM files WHERE idmovie={0} AND idpath={1} AND strFileName = '{2}'",
                                        lMovieId, lPathId, strFileName);
          SQLiteResultSet results = m_db.Execute(strSQL);

          if (results != null && results.Rows.Count > 0)
          {
            Int32.TryParse(DatabaseUtility.Get(results, 0, "idFile"), out lFileId);
            CheckMediaInfo(strFileName, string.Empty, lPathId, lFileId, false);
            return lFileId;
          }

          strSQL = String.Format("INSERT INTO files (idFile, idMovie,idPath, strFileName) VALUES(null, {0},{1},'{2}')",
                                 lMovieId, lPathId, strFileName);
          results = m_db.Execute(strSQL);
          lFileId = m_db.LastInsertID();
          CheckMediaInfo(strFileName, string.Empty, lPathId, lFileId, false);
          Log.Debug("VideodatabaseSqllite Finished AddFile:{0}", strFileName);
          return lFileId;
        }
        catch (Exception ex)
        {
          Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return -1;
      }
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
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
        strFilenameAndPath = strFilenameAndPath.Trim();
        DatabaseUtility.Split(strFilenameAndPath, out strPath, out strFileName);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        lPathId = GetPath(strPath);
        
        if (lPathId < 0)
        {
          return -1;
        }

        string strSQL = String.Format("SELECT * FROM files WHERE idpath={0} AND strFilename = '{1}'", lPathId, strFileName);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count > 0)
        {
                int lFileId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idFile"), out lFileId);
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idMovie"), out lMovieId);
                return lFileId;
              }
            }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int GetTitleBDId(int iFileId, out byte[] resumeData)//, int bdtitle)
    {
      resumeData = null;

      try
      {
        string sql = String.Format("SELECT * FROM resume WHERE idFile={0}", iFileId);// bdtitle);
        SQLiteResultSet results = m_db.Execute(sql);
        int BDTileID;

        if (results.Rows.Count != 0)
        {
          Int32.TryParse(DatabaseUtility.Get(results, 0, "bdtitle"), out BDTileID);
          return BDTileID;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public int AddMovieFile(string strFile)
    {
      bool bHassubtitles = false;
      
      if (strFile.ToLowerInvariant().IndexOf(".ifo") >= 0)
      {
        bHassubtitles = true;
      }
      
      if (strFile.ToLowerInvariant().IndexOf(".vob") >= 0)
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
        string strSQL = String.Format("SELECT * FROM path WHERE strPath like '{0}' AND cdlabel like '{1}'", strPath,
                                      cdlabel);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("INSERT INTO Path (idPath, strPath, cdlabel) VALUES( NULL, '{0}', '{1}')", strPath,
                                 cdlabel);
          m_db.Execute(strSQL);
          int lPathId = m_db.LastInsertID();
          return lPathId;
        }
        else
        {
          int lPathId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idPath"), out lPathId);
          strSQL = String.Format("UPDATE path SET strPath='{0}' WHERE idPath = {1}", strPath, lPathId);
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
        
        string cdlabel = string.Empty;
        string strSQL = string.Empty;
        strPath = strPath.Trim();
        
        if (Util.Utils.IsDVD(strPath))
        {
          // It's a DVD! Any drive letter should be OK as long as the label and rest of the path matches
          strPath = strPath.Replace(strPath.Substring(0, 1), "_");
          cdlabel = GetDVDLabel(strPath);
          DatabaseUtility.RemoveInvalidChars(ref cdlabel);
          strSQL = String.Format("SELECT * FROM path WHERE strPath = '{0}' AND cdlabel = '{1}'", strPath,
                                      cdlabel);
        }
        else
        {
          strSQL = String.Format("SELECT * FROM path WHERE strPath = '{0}'", strPath);
        }
        
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count > 0)
        {
          int lPathId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idPath"), out lPathId);
          return lPathId;
        }
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
        string strSQL = String.Format("DELETE FROM files WHERE idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM resume WHERE idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM duration WHERE idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM filesmediainfo WHERE idfile={0}", iFileId);
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
          strSQL = String.Format("DELETE FROM resume WHERE idfile={0}", idFile);
          m_db.Execute(strSQL);
          strSQL = String.Format("DELETE FROM duration WHERE idfile={0}", idFile);
          m_db.Execute(strSQL);
          strSQL = String.Format("DELETE FROM filesmediainfo WHERE idfile={0}", idFile);
          m_db.Execute(strSQL);
        }

        strSQL = String.Format("DELETE FROM files WHERE idMovie={0}", lMovieId);
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

    private int GetFileId(int movieId)
    {
      int fileId = -1;

      try
      {
        if (null == m_db)
        {
          return -1;
        }

        string strSQL = String.Format("SELECT * FROM files WHERE idMovie = {0} ORDER BY strFilename", movieId);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count > 0)
        {
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idFile"), out fileId);
        }
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return fileId;
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
          "SELECT * FROM path,files WHERE path.idPath=files.idPath AND files.idmovie={0} ORDER BY strFilename ASC",
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
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
      strSQL = String.Format("SELECT * FROM path WHERE idPath={0}", pathID);
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
      strSQL = String.Format("SELECT * FROM filesmediainfo WHERE idFile={0}", fileID);
      results = m_db.Execute(strSQL);

      if (results.Rows.Count == 0 || refresh)
      {
        Log.Info("VideoDatabase media info scanning file: {0}", strFilenameAndPath);
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

        // Set currentMediaInfoFilePlaying for later use if it's the same media to play (it will cache mediainfo data)
        MediaInfoWrapper mInfo = null;
        if (!string.IsNullOrEmpty(g_Player.currentMediaInfoFilePlaying) && (g_Player.currentMediaInfoFilePlaying == strFilenameAndPath))
        {
          mInfo = g_Player._mediaInfo;
        }
        else
        {
          g_Player.currentMediaInfoFilePlaying = strFilenameAndPath;
          mInfo = g_Player._mediaInfo = new MediaInfoWrapper(strFilenameAndPath);
        }

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
              "INSERT INTO filesmediainfo (idFile, videoCodec, videoResolution, aspectRatio, hasSubtitles, audioCodec, audioChannels) VALUES({0},'{1}','{2}','{3}',{4},'{5}','{6}')",
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
              "UPDATE filesmediainfo SET videoCodec='{1}', videoResolution='{2}', aspectRatio='{3}', hasSubtitles='{4}', audioCodec='{5}', audioChannels='{6}' WHERE idFile={0}",
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

          //Update movie subtitle field
          strSQL = String.Format("UPDATE movie SET hasSubtitles={0} WHERE idMovie={1} ", subtitles, movieId);
          m_db.Execute(strSQL);
        }
        catch (ThreadAbortException)
        {
          // Will be logged in thread main code
        }
        catch (Exception) { }
      }
    }

    public void GetVideoFilesMediaInfo(string strFilenameAndPath, ref VideoFilesMediaInfo mediaInfo, bool refresh)
    {
      try
      {
        if (strFilenameAndPath == String.Empty || !Util.Utils.IsVideo(strFilenameAndPath))
        {
          return;
        }

        if (strFilenameAndPath.IndexOf("remote:") >= 0 || strFilenameAndPath.IndexOf("http:") >= 0)
        {
          return;
        }

        int fileID = GetFileId(strFilenameAndPath);

        if (fileID < 1)
        {
          return;
        }

        // Get media info from database
        string strSQL = String.Format("SELECT * FROM filesmediainfo WHERE idFile={0}", fileID);
        SQLiteResultSet results = m_db.Execute(strSQL);

        // Set mInfo for files already in db but not scanned before
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
        mediaInfo.Duration = GetVideoDuration(fileID);
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase mediainfo exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private void GetVideoFilesMediaInfo(int movieId, ref VideoFilesMediaInfo mediaInfo)
    {
      try
      {
        if (movieId < 1)
        {
          return;
        }

        int fileID = GetFileId(movieId);

        if (fileID < 1 )
        {
          return;
        }

        // Get media info from database
        string strSQL = String.Format("SELECT * FROM filesmediainfo WHERE idFile={0}", fileID);
        SQLiteResultSet results = m_db.Execute(strSQL);

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
        mediaInfo.Duration = GetVideoDuration(fileID);
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
        string strSQL = String.Format("SELECT * FROM filesmediainfo WHERE idFile={0}", fileID);
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
        string strSQL = "SELECT * FROM genre WHERE strGenre LIKE '";
        strSQL += strGenre;
        strSQL += "'";
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = "INSERT INTO genre (idGenre, strGenre) VALUES( NULL, '";
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
        SQLiteResultSet results = m_db.Execute("SELECT * FROM genre ORDER BY strGenre");
        
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

    public string GetGenreById(int genreId)
    {
      if (m_db == null)
      {
        return string.Empty;
      }

      string strGenre = string.Empty;

      try
      {
        string sql = string.Format("SELECT * FROM genre WHERE idGenre = {0}", genreId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return string.Empty;
        }
        strGenre = DatabaseUtility.Get(results, 0, "strGenre");
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return strGenre;
    }

    public void AddGenreToMovie(int lMovieId, int lGenreId)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        
        string strSQL = String.Format("SELECT * FROM genrelinkmovie WHERE idGenre={0} AND idMovie={1}", lGenreId,
                                      lMovieId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("INSERT INTO genrelinkmovie (idGenre, idMovie) VALUES( {0},{1})", lGenreId, lMovieId);
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
        string sql = String.Format("SELECT * FROM genre WHERE strGenre LIKE '{0}'", genreFiltered);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return;
        }

        int idGenre;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idGenre"), out idGenre);
        m_db.Execute(sql);
        m_db.Execute(String.Format("DELETE FROM genrelinkmovie WHERE idGenre={0}", idGenre));
        m_db.Execute(String.Format("DELETE FROM genre WHERE idGenre={0}", idGenre));
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

        string strSQL = String.Format("DELETE FROM genrelinkmovie WHERE idMovie={0}", lMovieId);
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

    public int AddUserGroup(string userGroup)
    {
      try
      {
        string strUserGroup = userGroup.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strUserGroup);
        
        if (null == m_db)
        {
          return -1;
        }

        string strSQL = string.Format("SELECT * FROM usergroup WHERE strGroup like '{0}'", strUserGroup);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = string.Format("INSERT INTO usergroup (idGroup, strGroup, strRule, strGroupDescription) VALUES( NULL, '{0}', '', '')", strUserGroup); 
          m_db.Execute(strSQL);
          int groupId = m_db.LastInsertID();
          return groupId;
        }
        else
        {
          int groupId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idGroup"), out groupId);
          return groupId;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int GetUserGroupId (string userGroup)
    {
      try
      {
        string strUserGroup = userGroup.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strUserGroup);

        if (null == m_db)
        {
          return -1;
        }

        string strSQL = string.Format("SELECT idGroup FROM usergroup WHERE strGroup like '{0}'", strUserGroup);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count > 0)
        {
          int groupId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idGroup"), out groupId);
          return groupId;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void AddUserGroupDescription(string userGroup, string description)
    {
      try
      {
        string strUserGroup = userGroup.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strUserGroup);

        string strGroupDescription = description.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strGroupDescription);

        if (null == m_db)
        {
          return;
        }

        string strSQL = string.Format("SELECT * FROM usergroup WHERE strGroup like '{0}'", strUserGroup);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count > 0)
        {
          int groupId;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idGroup"), out groupId);

          if (!string.IsNullOrEmpty(strGroupDescription) && strGroupDescription != Strings.Unknown)
          {
            strSQL = String.Format("UPDATE usergroup SET strGroupDescription='{0}' WHERE idGroup={1}",
                                   strGroupDescription, groupId);
            m_db.Execute(strSQL);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
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

        string strSQL = String.Format("UPDATE usergroup SET strRule='{0}' WHERE idGroup={1}", rule, groupId);
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

        string strSQL = String.Format("UPDATE usergroup SET strRule='{0}' WHERE strGroup LIKE '{1}'", rule, groupName);
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
        SQLiteResultSet results = m_db.Execute("SELECT * FROM usergroup ORDER BY strGroup");
        
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

    public string GetUserGroupById(int groupId)
    {
      if (m_db == null)
      {
        return string.Empty;
      }

      string strGroup = string.Empty;

      try
      {
        string sql = string.Format("SELECT strGroup FROM usergroup WHERE idGroup = {0}", groupId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        strGroup =  DatabaseUtility.Get(results, 0, "strGroup");
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return strGroup;
    }

    public string GetUserGroupDescriptionById(int groupId)
    {
      if (m_db == null)
      {
        return string.Empty;
      }

      string strGroup = string.Empty;

      try
      {
        string sql = string.Format("SELECT strGroupDescription FROM usergroup WHERE idGroup = {0}", groupId);
        SQLiteResultSet results = m_db.Execute(sql);

        strGroup = DatabaseUtility.Get(results, 0, "strGroupDescription");

        if (strGroup == Strings.Unknown)
        {
          strGroup = string.Empty;
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return strGroup;
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
        string strSQL = String.Format("SELECT idGroup FROM usergrouplinkmovie WHERE idMovie={0}", movieId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }
        
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          int groupId = Convert.ToInt32(DatabaseUtility.Get(results, iRow, "idGroup"));
          strSQL = String.Format("SELECT strGroup FROM usergroup WHERE idGroup = {0}", groupId);
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

        string strSQL = String.Format("SELECT strRule FROM usergroup WHERE strGroup like '{0}'", group);
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
        string strSQL = String.Format("SELECT * FROM usergrouplinkmovie WHERE idGroup={0} and idMovie={1}", lUserGroupId,
                                      lMovieId);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("INSERT INTO usergrouplinkmovie (idGroup, idMovie) VALUES( {0},{1})", lUserGroupId, lMovieId);
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

        string strSQL = String.Format("DELETE FROM usergrouplinkmovie WHERE idGroup={0} AND idMovie={1}", lUserGroupId,
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
        string sql = String.Format("SELECT * FROM usergroup WHERE strGroup like '{0}'", userGroupFiltered);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return;
        }

        int idUserGroup;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idGroup"), out idUserGroup);
        m_db.Execute(sql);
        m_db.Execute(String.Format("DELETE FROM usergrouplinkmovie WHERE idGroup={0}", idUserGroup));
        m_db.Execute(String.Format("DELETE FROM usergroup WHERE idGroup={0}", idUserGroup));
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

        string strSQL = String.Format("DELETE FROM usergrouplinkmovie WHERE idMovie={0}", lMovieId);
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

        string strSQL = String.Format("UPDATE usergroup SET strRule='' WHERE strGroup LIKE '{0}'", groupName);
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
        DatabaseUtility.RemoveInvalidChars(ref strActorImdbId);
        DatabaseUtility.RemoveInvalidChars(ref strActorName);
        
        if (null == m_db)
        {
          return -1;
        }

        string strSQL = string.Empty;

        if (string.IsNullOrEmpty(strActorImdbId) || strActorImdbId == Strings.Unknown)
        {
          strSQL = "SELECT * FROM actors WHERE strActor LIKE '";
          strSQL += strActorName;
          strSQL += "'";
        }
        else
        {
          strSQL = "SELECT * FROM actors WHERE IMDBActorId = '";
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
            strSQL = string.Format("UPDATE actors SET IMDBActorId='{0}', strActor = '{1}' WHERE idActor ={2}",
                                  strActorImdbId,
                                  strActorName,
                                  idActor);
            m_db.Execute(strSQL);
            return idActor;
          }
          else
          {
            strSQL = "INSERT INTO actors (idActor, strActor, IMDBActorId) VALUES( NULL, '";
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
        SQLiteResultSet results = m_db.Execute("SELECT * FROM actors");
        
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

    public string GetActorNameById(int actorId)
    {
      if (m_db == null)
      {
        return string.Empty;
      }

      string strActor = string.Empty;

      try
      {
        string sql = string.Format("SELECT strActor FROM actors WHERE idActor = {0}", actorId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return string.Empty;
        }
        
        strActor =  DatabaseUtility.Get(results, 0, "strActor");
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return strActor;
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
        SQLiteResultSet results = m_db.Execute("SELECT * FROM actors WHERE strActor LIKE '%" + strActorName + "%'");
        
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
      if (m_db == null)
      {
        return -1;
      }

      try
      {
        SQLiteResultSet results = m_db.Execute("SELECT * FROM actors WHERE strActor LIKE '%" + strActorName + "%'");
        
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
            "SELECT actors.idActor, actors.strActor, actors.IMDBActorId, actorlinkmovie.strRole FROM actors INNER JOIN actorlinkmovie ON actors.idActor = actorlinkmovie.idActor WHERE actorlinkmovie.idMovie={0}",
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
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
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

        string strSQL = String.Format("SELECT * FROM actorlinkmovie WHERE idActor={0} AND idMovie={1}", lActorId,
                                      lMovieId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("INSERT INTO actorlinkmovie (idActor, idMovie, strRole) VALUES( {0},{1},'{2}')", lActorId, lMovieId, role);
          m_db.Execute(strSQL);
        }
        else
        {
          // exists, update it (role only)
          strSQL = String.Format("UPDATE actorlinkmovie SET strRole = '{0}' WHERE idActor={1} AND idMovie={2}", role,lActorId, lMovieId);
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
        m_db.Execute(String.Format("DELETE FROM actorlinkmovie WHERE idMovie={0} AND idActor={1}", movieId, actorId));
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
        string sql = String.Format("SELECT * FROM actors WHERE IMDBActorId='{0}'", actorFiltered);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return;
        }

        int idactor;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idActor"), out idactor);
        m_db.Execute(sql);
        m_db.Execute(String.Format("DELETE FROM actorlinkmovie WHERE idActor={0}", idactor));
        m_db.Execute(String.Format("DELETE FROM actors WHERE idActor={0}", idactor));
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

        string strSQL = String.Format("DELETE FROM actorlinkmovie WHERE idMovie={0}", lMovieId);
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
            "SELECT strRole from actorlinkmovie WHERE idMovie={0} AND idActor={1}", lMovieId, lActorId);
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
        
        string strSQL = String.Format("DELETE FROM bookmark WHERE idFile={0}", lFileId);
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
       
        string strSQL = String.Format("SELECT * FROM bookmark WHERE idFile={0} AND fPercentage='{1}'", lFileId, fTime);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count != 0)
        {
          return;
        }

        strSQL = String.Format("INSERT INTO bookmark (idBookmark, idFile, fPercentage) VALUES(NULL,{0},'{1}')", lFileId,
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

        string strSQL = String.Format("SELECT * FROM bookmark WHERE idFile={0} ORDER BY fPercentage", lFileId);
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

    public void SetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      SetMovieInfoById(lMovieId, ref details, false);
    }
    
    public void SetMovieInfoById(int lMovieId, ref IMDBMovie details, bool updateTimeStamp)
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
        strLine = details1.Title.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strLine);
        details1.Title = strLine;
        // SortTtitle
        strLine = details1.SortTitle.Trim();

        if (!string.IsNullOrEmpty(strLine))
        {
          DatabaseUtility.RemoveInvalidChars(ref strLine);
          details1.SortTitle = strLine;
        }
        else
        {
          details1.SortTitle = details1.Title;
        }
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
        // Last update
        if (updateTimeStamp)
        {
          details1.LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
          details1.LastUpdate = existingDetails.LastUpdate;
        }
        // add all genres
        string szGenres = details.Genre;
        ArrayList vecGenres = new ArrayList();
        
        if (szGenres != Strings.Unknown)
        {
          if (szGenres.IndexOf("/") >= 0 || szGenres.IndexOf("|") >= 0)
          {
            Tokens f = new Tokens(szGenres, new[] {'/', '|'});
            foreach (string strGenre in f)
            {
              if (!string.IsNullOrEmpty(strGenre.Trim()))
              {
                int lGenreId = AddGenre(strGenre.Trim());
                vecGenres.Add(lGenreId);
              }
            }
          }
          else
          {
            string strGenre = details.Genre;
            strGenre = strGenre.Trim();
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

        string strSQL = String.Format("SELECT * FROM movieinfo WHERE idmovie={0}", lMovieId);
        //	Log.Error("dbs:{0}", strSQL);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // Insert new movie info - no date watched update
          strSQL =
            String.Format(
              "INSERT INTO movieinfo ( idMovie, idDirector, strPlotOutline, strPlot, strTagLine, strVotes, fRating, strCast, strCredits, iYear, strGenre, strPictureURL, strTitle, IMDBID, mpaa, runtime, iswatched, strUserReview, strFanartURL, strDirector, dateAdded, studios, country, language, lastupdate, strSortTitle) VALUES({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9},'{10}','{11}','{12}','{13}','{14}',{15},{16},'{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}')",
              lMovieId, lDirector, details1.PlotOutline,
              details1.Plot, details1.TagLine,
              details1.Votes, strRating,
              details1.Cast, details1.WritingCredits,
              details1.Year, details1.Genre,
              details1.ThumbURL, details1.Title,
              details1.IMDBNumber, details1.MPARating, 
              details1.RunTime, details1.Watched, 
              details1.UserReview, details1.FanartURL, 
              details1.Director, details1.DateAdded, 
              details1.Studios, details1.Country, 
              details1.Language, details1.LastUpdate, details1.SortTitle);

          //			Log.Error("dbs:{0}", strSQL);
          m_db.Execute(strSQL);
          // Update latest movies
          SetLatestMovieProperties();
        }
        else
        {
          // Update movie info (no dateAdded update)
          strSQL =
            String.Format(
              "UPDATE movieinfo SET idDirector={0}, strPlotOutline='{1}', strPlot='{2}', strTagLine='{3}', strVotes='{4}', fRating='{5}', strCast='{6}',strCredits='{7}', iYear={8}, strGenre='{9}', strPictureURL='{10}', strTitle='{11}', IMDBID='{12}', mpaa='{13}', runtime={14}, iswatched={15} , strUserReview='{16}', strFanartURL='{17}' , strDirector ='{18}', dateWatched='{19}', studios = '{20}', country = '{21}', language = '{22}' , lastupdate = '{23}', strSortTitle = '{24}' WHERE idMovie={25}",
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
              details1.LastUpdate, details1.SortTitle,
              lMovieId);

          //		Log.Error("dbs:{0}", strSQL);
          m_db.Execute(strSQL);
        }
        
        VideoDatabase.GetMovieInfoById(details1.ID, ref details1);
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

        IMDBMovie movie = new IMDBMovie();
        GetMovieInfoById((int) lMovieId, ref movie);

        // Delete movie cover
        FanArt.DeleteCovers(movie.Title, (int) lMovieId);
        // Delete movie fanart
        FanArt.DeleteFanarts((int) lMovieId);
        // Delete user groups for movie
        RemoveUserGroupsForMovie((int) lMovieId);

        string strSQL = String.Format("DELETE FROM genrelinkmovie WHERE idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM actorlinkmovie WHERE idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM movieinfo WHERE idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM files WHERE idMovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM movie WHERE idMovie={0}", lMovieId);
        m_db.Execute(strSQL);

        // Update latest movies
        SetLatestMovieProperties();
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
        
        string strSQL = String.Format("SELECT * FROM movieinfo WHERE movieinfo.idmovie={0}", lMovieId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return false;
        }
        
        return true;
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
          "SELECT * FROM movieinfo,movie,path WHERE path.idpath=movie.idpath AND movie.idMovie=movieinfo.idMovie AND movieinfo.idmovie={0}",
          lMovieId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }
        
        SetMovieDetails(ref details, 0, results);
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
        
        string strSQL = String.Format("UPDATE movieinfo SET iswatched={0}, datewatched = '{1}' WHERE idMovie={2}", 
                                      details.Watched,
                                      DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
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
        {
          details.DateWatched = "0001-01-01 00:00:00";
        }

        string strSQL = String.Format("UPDATE movieinfo SET dateWatched='{0}' WHERE idMovie={1}", 
                                      details.DateWatched,
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
        string sql = String.Format("DELETE FROM resume WHERE idFile={0}", iFileId);
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
        string sql = string.Format("SELECT * FROM resume WHERE idFile={0}", iFileId);
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
        string sql = String.Format("SELECT * FROM resume WHERE idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          sql = String.Format("INSERT INTO resume ( idResume,idFile,stoptime) VALUES(NULL,{0},{1})",
                              iFileId, stoptime);
        }
        else
        {
          sql = String.Format("UPDATE resume SET stoptime={0} WHERE idFile={1}",
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

    /// <summary>
    /// Deprecated Method (this one will not use the new Blu-ray Title mode resume)
    /// </summary>
    public int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData)
    {
      return GetMovieStopTimeAndResumeData(iFileId, out resumeData, g_Player.BdDefaultTitle);
    }

    public int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData, int bdtitle)
    {
      resumeData = null;

      try
      {
        string sql = String.Format("SELECT * FROM resume WHERE idFile={0} AND bdtitle={1}", iFileId, bdtitle);
        SQLiteResultSet results = m_db.Execute(sql);
        int stoptime;
        int BDTileID;

        if (results.Rows.Count != 0)
        {
          Int32.TryParse(DatabaseUtility.Get(results, 0, "stoptime"), out stoptime);
          string resumeString = DatabaseUtility.Get(results, 0, "resumeData");
          resumeData = new byte[resumeString.Length/2];
          FromHexString(resumeString).CopyTo(resumeData, 0);
          return stoptime;
        }
        else
        {
          Int32.TryParse(DatabaseUtility.Get(results, 0, "bdtitle"), out BDTileID);
          if (bdtitle != BDTileID)
          {
            return 0;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    /// <summary>
    /// Deprecated Method (this one will not use the new Blu-ray Title mode resume)
    /// </summary>
    public void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData)
    {
      SetMovieStopTimeAndResumeData(iFileId, stoptime, resumeData, g_Player.BdDefaultTitle);
    }

    public void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData, int bdtitle)
    {
      try
      {
        // The next line is too enable record of stoptime for each Title BD
        //string sql = String.Format("SELECT * FROM resume WHERE idFile={0} AND bdtitle={1}", iFileId, bdtitle);

        // Only store stoptime with one current Title BD
        string sql = String.Format("SELECT * FROM resume WHERE idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);
        string resumeString = "-";

        if (resumeData != null)
        {
          resumeString = ToHexString(resumeData);
        }


        // Only store stoptime with one current Title BD
        if (results.Rows.Count != 0)
        {
          sql = String.Format("UPDATE resume SET stoptime={0},resumeData='{1}',bdtitle='{2}' WHERE idFile={3}",
                              stoptime, resumeString, bdtitle, iFileId);
        }
        else if (bdtitle >= 0)
        {
          sql =
            String.Format(
              "INSERT INTO resume ( idResume,idFile,stoptime,resumeData,bdtitle) VALUES(NULL,{0},{1},'{2}',{3})",
              iFileId, stoptime, resumeString, bdtitle);
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
        string sql = string.Format("SELECT * FROM movie WHERE idMovie={0}", iMovieId);
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
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
        string sql = string.Format("SELECT * FROM duration WHERE idFile={0}", iFileId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count == 0)
        {
          return 0;
        }
        
        int duration;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "duration"), out duration);
        return duration;
      }
      catch (ThreadAbortException)
      {
        // Will be logged in main thread code  
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
          string sql = String.Format("SELECT * FROM duration WHERE idFile={0}", iFileId);
          SQLiteResultSet results = m_db.Execute(sql);

          if (results.Rows.Count == 0)
          {
            sql = String.Format("INSERT INTO duration ( idDuration,idFile,duration) VALUES(NULL,{0},{1})",
                                iFileId, duration);
          }
          else
          {
            sql = String.Format("UPDATE duration SET duration={0} WHERE idFile={1}",
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
          string sql = sql = String.Format("UPDATE movie SET iduration={0} WHERE idMovie={1}",
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
        string sql = String.Format("SELECT * FROM movie WHERE idMovie={0}", idMovie);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count != 0)
        {
          int iWatched = 0;
          
          if (watched)
          {
            iWatched = 1;
          }
          
          sql = String.Format("UPDATE movie SET watched={0}, iwatchedPercent = {1} WHERE idMovie={2}",
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
        string sql = String.Format("SELECT * FROM movie WHERE idMovie={0}", idMovie);
        SQLiteResultSet results = m_db.Execute(sql);
        int watchedCount = 0;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "movie.timeswatched"), out watchedCount);

        if (results.Rows.Count != 0)
        {
          watchedCount++;
          sql = String.Format("UPDATE movie SET timeswatched = {0} WHERE idMovie={1}",
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
        string sql = String.Format("SELECT * FROM movie WHERE idMovie={0}", movieId);
        SQLiteResultSet results = m_db.Execute(sql);
        
        if (results.Rows.Count != 0)
        {
          sql = String.Format("UPDATE movie SET timeswatched = {0} WHERE idMovie={1}",
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
        string sql = String.Format("SELECT * FROM movie WHERE idMovie={0}", idMovie);
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
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
      {
        pathList.Add(DatabaseUtility.Get(results, i, 0), DatabaseUtility.Get(results, i, 1));
      }

      foreach (KeyValuePair<string, string> kvp in pathList)
      {
        results = m_db.Execute("SELECT strFilename FROM files WHERE idPath=" + kvp.Key);
        
        for (int j = 0; j < results.Rows.Count; ++j)
        {
          DeleteSingleMovie(kvp.Value + DatabaseUtility.Get(results, j, 0));
        }
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
          "SELECT * FROM path,files WHERE path.idPath=files.idPath AND files.idmovie={0} ORDER BY strFilename",
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
        strSQL = String.Format("DELETE FROM genrelinkmovie WHERE idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM actorlinkmovie WHERE idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM movieinfo WHERE idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("DELETE FROM movie WHERE idmovie={0}", lMovieId);
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
            "INSERT INTO movie (idMovie, idPath, hasSubtitles, discid) VALUES( NULL, {0}, {1},'')", lPathId, iHasSubs);

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
          "SELECT * FROM movie,movieinfo,path WHERE movieinfo.idmovie=movie.idmovie AND movie.idpath=path.idpath");
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
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
        string strSQL = String.Format("SELECT * FROM movie WHERE movie.idMovie={0}", lMovieId);

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

        string strSQL = String.Format("UPDATE movieinfo SET strPictureURL='{0}' WHERE idMovie={1}", thumbURL, lMovieId);
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

        string strSQL = String.Format("UPDATE movieinfo SET strFanartURL='{0}' WHERE idMovie={1}", fanartURL, lMovieId);
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

        string strSQL = String.Format("UPDATE movie SET discid='{0}' WHERE idMovie={1}", strDVDLabel1, lMovieId);
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
        SQLiteResultSet results = m_db.Execute("SELECT * FROM movieinfo");
        
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
          "SELECT * FROM genrelinkmovie,genre,movie,movieinfo,path WHERE path.idpath=movie.idpath AND genrelinkmovie.idGenre=genre.idGenre AND genrelinkmovie.idmovie=movie.idmovie AND movieinfo.idmovie=movie.idmovie AND genre.strGenre='{0}' ORDER BY movieinfo.strTitle",
          strGenre);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }
        
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetRandomMoviesByGenre(string strGenre1, ref ArrayList movies, int limit)
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
          "SELECT * FROM genrelinkmovie,genre,movie,movieinfo,path WHERE path.idpath=movie.idpath AND genrelinkmovie.idGenre=genre.idGenre AND genrelinkmovie.idmovie=movie.idmovie AND movieinfo.idmovie=movie.idmovie AND genre.strGenre='{0}' ORDER BY RANDOM() LIMIT {1};", 
          strGenre, limit);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetMovieTitlesByGenre(string strGenre)
    {
      string titles = string.Empty;

      try
      {
        if (null == m_db)
        {
          return titles;
        }


        string strSQL = String.Format(
          "SELECT DISTINCT movieinfo.strTitle FROM movieinfo WHERE movieinfo.strGenre LIKE '%{0}%' ORDER BY movieinfo.strTitle ASC",
          strGenre);

        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return titles;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          titles += DatabaseUtility.Get(results, iRow, "movieinfo.strTitle") + "\n";
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return titles;
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
          "SELECT * FROM usergrouplinkmovie,usergroup,movie,movieinfo,path WHERE path.idpath=movie.idpath AND usergrouplinkmovie.idGroup=usergroup.idGroup AND usergrouplinkmovie.idmovie=movie.idmovie AND movieinfo.idmovie=movie.idmovie AND usergroup.strGroup='{0}' ORDER BY movieinfo.strTitle ASC",
          strUserGroup);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }
        
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetMovieTitlesByUserGroup(int idGroup)
    {
      string titles = string.Empty;

      try
      {
        if (null == m_db)
        {
          return titles;
        }

        
        string strSQL = String.Format(
          "SELECT DISTINCT movieinfo.strTitle FROM movieinfo INNER JOIN usergrouplinkmovie ON movieinfo.idMovie = usergrouplinkmovie.idMovie WHERE usergrouplinkmovie.idgroup = {0} ORDER BY movieinfo.strTitle ASC",
          idGroup);

        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return titles;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          titles += DatabaseUtility.Get(results, iRow, "movieinfo.strTitle") + "\n";
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return titles;
    }

    public void GetRandomMoviesByUserGroup(string strUserGroup, ref ArrayList movies, int limit)
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
          "SELECT * FROM usergrouplinkmovie,usergroup,movie,movieinfo,path WHERE path.idpath=movie.idpath AND usergrouplinkmovie.idGroup=usergroup.idGroup AND usergrouplinkmovie.idmovie=movie.idmovie AND movieinfo.idmovie=movie.idmovie AND usergroup.strGroup='{0}' ORDER BY RANDOM() LIMIT {1}",
          strUserGroup, limit);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
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
          "SELECT * FROM actorlinkmovie,actors,movie,movieinfo,path WHERE path.idpath=movie.idpath AND actors.idActor=actorlinkmovie.idActor AND actorlinkmovie.idmovie=movie.idmovie AND movieinfo.idmovie=movie.idmovie AND actors.stractor='{0}' ORDER BY movieinfo.strTitle ASC",
          strActor);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }
        
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetRandomMoviesByActor(string strActor1, ref ArrayList movies, int limit)
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
          "SELECT * FROM actorlinkmovie,actors,movie,movieinfo,path WHERE path.idpath=movie.idpath AND actors.idActor=actorlinkmovie.idActor AND actorlinkmovie.idmovie=movie.idmovie AND movieinfo.idmovie=movie.idmovie AND actors.stractor='{0}' ORDER BY RANDOM() LIMIT {1}",
          strActor, limit);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetMovieTitlesByActor(int actorId)
    {
      string titles = string.Empty;

      try
      {
        if (null == m_db)
        {
          return titles;
        }


        string strSQL = String.Format(
          "SELECT DISTINCT movieinfo.strTitle FROM movieinfo INNER JOIN actorlinkmovie ON movieinfo.idMovie = actorlinkmovie.idMovie WHERE actorlinkmovie.idActor = {0} ORDER BY movieinfo.strTitle ASC",
          actorId);

        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return titles;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          titles += DatabaseUtility.Get(results, iRow, "movieinfo.strTitle") + "\n";
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return titles;
    }

    public string GetMovieTitlesByDirector(int directorId)
    {
      string titles = string.Empty;

      try
      {
        if (null == m_db)
        {
          return titles;
        }


        string strSQL = String.Format(
          "SELECT DISTINCT movieinfo.strTitle FROM movieinfo WHERE movieinfo.idDirector = {0} ORDER BY movieinfo.strTitle ASC",
          directorId);

        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return titles;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          titles += DatabaseUtility.Get(results, iRow, "movieinfo.strTitle") + "\n";
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return titles;
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
          "SELECT * FROM movie,movieinfo,path WHERE path.idpath=movie.idpath AND movieinfo.idmovie=movie.idmovie AND movieinfo.iYear={0} ORDER BY movieinfo.strTitle ASC",
          iYear);

        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          return;
        }
        
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetRandomMoviesByYear(string strYear, ref ArrayList movies, int limit)
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
          "SELECT * FROM movie,movieinfo,path WHERE path.idpath=movie.idpath AND movieinfo.idmovie=movie.idmovie AND movieinfo.iYear={0} ORDER BY RANDOM() LIMIT {1}",
          iYear, limit);

        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetMovieTitlesByYear(string strYear)
    {
      string titles = string.Empty;

      try
      {
        int iYear;
        Int32.TryParse(strYear, out iYear);

        if (null == m_db)
        {
          return titles;
        }

        string strSQL = String.Format(
          "SELECT DISTINCT movieinfo.strTitle FROM movieinfo WHERE movieinfo.iYear = {0} ORDER BY movieinfo.strTitle ASC",
          iYear);

        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return titles;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          titles += DatabaseUtility.Get(results, iRow, "movieinfo.strTitle") + "\n";
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return titles;
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
          String.Format("SELECT * FROM files,movieinfo WHERE files.idpath={0} AND files.idMovie=movieinfo.idMovie",
                        lPathId);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetRandomMoviesByPath(string strPath1, ref ArrayList movies, int limit)
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
          String.Format("SELECT * FROM files,movieinfo WHERE files.idpath={0} AND files.idMovie=movieinfo.idMovie ORDER BY RANDOM() LIMIT {1}",
                        lPathId, limit);
        SQLiteResultSet results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0)
        {
          return;
        }

        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          SetMovieDetails(ref details, iRow, results);
          movies.Add(details);
        }
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
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
            SetMovieDetails(ref movie, i, results);
            movie.File = movie.VideoFileName;
          }
          movies.Add(movie);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetIndexByFilter(string sql, bool filterNonWordChar, out ArrayList movieList)
    {
      movieList = new ArrayList();
      try
      {
        if (null == m_db)
        {
          return;
        }

        SQLiteResultSet results = GetResults(sql);
        bool nonWordCharFound = false;
        int nwCount = 0;

        // Count nowWord items
        if (filterNonWordChar)
        {
          for (int i = 0; i < results.Rows.Count; i++)
          {
            SQLiteResultSet.Row fields = results.Rows[i];

            if (Regex.Match(fields.fields[0], @"\W|\d").Success)
            {
              int iCount = Convert.ToInt32(fields.fields[1]);
              nwCount = nwCount + iCount;
            }
          }
        }

        for (int i = 0; i < results.Rows.Count; i++)
        {
          IMDBMovie movie = new IMDBMovie();
          SQLiteResultSet.Row fields = results.Rows[i];
          string value = fields.fields[0];
          int countN = Convert.ToInt32(fields.fields[1]);

          if (filterNonWordChar && Regex.Match(fields.fields[0], @"\W|\d").Success)
          {
            if (!nonWordCharFound)
            {
              value = "#";
              nonWordCharFound = true;
              countN = nwCount;
            }
            else
            {
              continue;
            }
          }

          movie.Title = value;
          movie.RunTime = countN;
          movieList.Add(movie);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetMovieTitlesByIndex(string sql)
    {
      string titles = string.Empty;

      try
      {
        if (null == m_db)
        {
          return titles;
        }

        SQLiteResultSet results = GetResults(sql);
        
        for (int i = 0; i < results.Rows.Count; i++)
        {
          SQLiteResultSet.Row fields = results.Rows[i];
          string value = fields.fields[0];
          titles += value + "\n";
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return titles;
    }

    public void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel)
    {
      if (movieDetails == null)
      {
        return;
      }
      
      try
      {
        string sql = String.Format("SELECT idPath FROM path WHERE cdlabel = '{0}'", movieDetails.CDLabel);
        SQLiteResultSet results = m_db.Execute(sql);
        int idPath;
        Int32.TryParse(DatabaseUtility.Get(results, 0, "idPath"), out idPath);
        sql = String.Format("UPDATE path SET cdlabel = '{0}' WHERE idPath = '{1}'", CDlabel, idPath);
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

    public void SetActorInfo(int idActor, IMDBActor actor)
    {
      try
      {
        if (null == m_db)
        {
          return;
        }
        
        string strSQL = String.Format("SELECT * FROM actorinfo WHERE idActor ={0}", idActor);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL =
            String.Format(
              "INSERT INTO actorinfo (idActor , dateofbirth , placeofbirth , minibio , biography, thumbURL, IMDBActorID, dateofdeath , placeofdeath, lastupdate ) VALUES( {0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
              idActor, 
              DatabaseUtility.RemoveInvalidChars(actor.DateOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.MiniBiography),
              DatabaseUtility.RemoveInvalidChars(actor.Biography),
              DatabaseUtility.RemoveInvalidChars(actor.ThumbnailUrl),
              DatabaseUtility.RemoveInvalidChars(actor.IMDBActorID),
              DatabaseUtility.RemoveInvalidChars(actor.DateOfDeath),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfDeath),
              DatabaseUtility.RemoveInvalidChars(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
          m_db.Execute(strSQL);
        }
        else
        {
          // exists, modify it
          strSQL =
            String.Format(
              "UPDATE actorinfo SET dateofbirth='{1}', placeofbirth='{2}', minibio='{3}', biography='{4}', thumbURL='{5}', IMDBActorID='{6}', dateofdeath='{7}', placeofdeath='{8}', lastupdate ='{9}' WHERE idActor={0}",
              idActor, 
              DatabaseUtility.RemoveInvalidChars(actor.DateOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfBirth),
              DatabaseUtility.RemoveInvalidChars(actor.MiniBiography),
              DatabaseUtility.RemoveInvalidChars(actor.Biography),
              DatabaseUtility.RemoveInvalidChars(actor.ThumbnailUrl),
              DatabaseUtility.RemoveInvalidChars(actor.IMDBActorID),
              DatabaseUtility.RemoveInvalidChars(actor.DateOfDeath),
              DatabaseUtility.RemoveInvalidChars(actor.PlaceOfDeath),
              DatabaseUtility.RemoveInvalidChars(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
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
      string movieTitle = DatabaseUtility.RemoveInvalidChars(movie.MovieTitle);
      string movieRole = DatabaseUtility.RemoveInvalidChars(movie.Role);
      
      try
      {
        if (null == m_db)
        {
          return;
        }
        
        string strSQL =
          String.Format(
            "INSERT INTO actorinfomovies (idActor, idDirector , strPlotOutline , strPlot , strTagLine , strVotes , fRating ,strCast ,strCredits , iYear , strGenre , strPictureURL , strTitle , IMDBID , mpaa ,runtime , iswatched , role  ) VALUES( {0} ,{1} ,'{2}' , '{3}' , '{4}' , '{5}' , '{6}' ,'{7}' ,'{8}' , {9} , '{10}' , '{11}' , '{12}' , '{13}' ,'{14}',{15} , {16} , '{17}' )",
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
          strSQL = String.Format("SELECT * FROM IMDBMovies WHERE idIMDB='{0}'", movie.MovieImdbID);
          SQLiteResultSet results = m_db.Execute(strSQL);
          
          if (results.Rows.Count == 0)
          {
            strSQL = String.Format("INSERT INTO IMDBMovies (  idIMDB, idTmdb, strPlot, strCast, strCredits, iYear, strGenre, strPictureURL, strTitle, mpaa) VALUES( '{0}' ,'{1}' ,'{2}' , '{3}' , '{4}' , {5} , '{6}' ,'{7}' ,'{8}' , '{9}')",
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
            strSQL = String.Format("UPDATE IMDBMovies SET iYear={0}, strTitle='{1}' WHERE idIMDB='{2}'",
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
        
        string strSQL = String.Format("DELETE FROM actorinfomovies WHERE idActor={0}", actorId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public IMDBActor GetActorInfo(int idActor)
    {
      try
      {
        if (null == m_db)
        {
          return null;
        }
        string strSql = String.Format(
            "SELECT actorinfo.biography, actorinfo.dateofbirth, actorinfo.dateofdeath, actorinfo.minibio, actors.strActor, actorinfo.placeofbirth, actorinfo.placeofdeath, actorinfo.thumbURL, actorinfo.lastupdate, actors.IMDBActorID, actorinfo.idActor FROM actors,actorinfo WHERE actors.idActor=actorinfo.idActor AND actors.idActor ={0}", idActor);
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
          actor.LastUpdate = DatabaseUtility.Get(results, 0, "actorinfo.lastupdate");
          actor.IMDBActorID = DatabaseUtility.Get(results, 0, "actors.IMDBActorID");
          actor.ID = Convert.ToInt32(DatabaseUtility.Get(results, 0, "actorinfo.idActor"));

          strSql = String.Format("SELECT * FROM actorinfomovies WHERE idActor ={0}", idActor);
          results = m_db.Execute(strSql);
          
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            string imdbId = DatabaseUtility.Get(results, i, "IMDBID");
            strSql = String.Format("SELECT * FROM IMDBMovies WHERE idIMDB='{0}'", imdbId);
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
        string strSql = String.Format("SELECT actors.IMDBActorID FROM actors WHERE actors.idActor ={0}", idActor);
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
          "SELECT idVideoThumbBList FROM VideoThumbBList WHERE strPath = '{0}' AND strExpires > '{1:yyyyMMdd}' AND strFileDate = '{2:s}' AND strFileSize = '{3}'",
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
        string strSQL = String.Format("SELECT idVideoThumbBList FROM VideoThumbBList WHERE strPath = '{0}'", path);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL =
            String.Format(
              "INSERT INTO VideoThumbBList (idVideoThumbBList, strPath, strExpires, strFileDate, strFileSize) VALUES( NULL, '{0}', '{1:yyyyMMdd}', '{2:s}', '{3}')",
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
                "UPDATE VideoThumbBList SET strExpires='{1:yyyyMMdd}', strFileDate='{2:s}', strFileSize='{3}' WHERE idVideoThumbBList={0}",
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
        string strSQL = String.Format("SELECT idVideoThumbBList FROM VideoThumbBList WHERE strPath = '{0}'", path);
        SQLiteResultSet results = m_db.Execute(strSQL);
        
        if (results.Rows.Count != 0)
        {
          int id = -1;
          Int32.TryParse(DatabaseUtility.Get(results, 0, "idVideoThumbBList"), out id);
          
          if (id != -1)
          {
            strSQL = String.Format("DELETE FROM VideoThumbBList WHERE idVideoThumbBList={0}", id);
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

        string strSQL = String.Format("DELETE FROM VideoThumbBList WHERE strExpires <= '{0:yyyyMMdd}'", DateTime.Now);
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

        m_db.Execute("DELETE FROM VideoThumbBList");
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    public void ExecuteSQL (string strSql, out bool error, out string errorMessage)
    {
      error = false;
      errorMessage = string.Empty;

      try
      {
        if (m_db == null)
        {
          return;
        }
        
        m_db.Execute(strSql);
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        error = true;
        errorMessage = ex.Message;
        Open();
      }
    }

    public ArrayList ExecuteRuleSQL(string strSql, string fieldName, out bool error, out string errorMessage)
    {
      ArrayList values = new ArrayList();
      error = false;
      errorMessage = string.Empty;

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
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        error = true;
        errorMessage = ex.Message;
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
      Match ttNo = Regex.Match(id, @"tt[\d]{7}?", RegexOptions.IgnoreCase);
      
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
      Match ttNo = Regex.Match(id, @"nm[\d]{7}?", RegexOptions.IgnoreCase);
      
      if (!ttNo.Success)
      {
        return false;
      }
      
      return true;
    }

    public void ImportNfoUsingVideoFile(string videoFile, bool skipExisting, bool refreshdbOnly)
    {
      try
      {
        string nfoFile = string.Empty;
        string path = string.Empty;
        bool isbdDvd = false;
        string nfoExt = ".nfo";

        if (videoFile.ToUpperInvariant().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          //DVD folder
          path = videoFile.Substring(0, videoFile.ToUpperInvariant().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase));
          isbdDvd = true;
        }
        else if (videoFile.ToUpperInvariant().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          //BD folder
          path = videoFile.Substring(0, videoFile.ToUpperInvariant().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase));
          isbdDvd = true;
        }

        if (isbdDvd)
        {
          string cleanFile = string.Empty;
          cleanFile = Path.GetFileNameWithoutExtension(videoFile);
          Util.Utils.RemoveStackEndings(ref cleanFile);
          nfoFile = path + @"\" + cleanFile + nfoExt;

          if (!File.Exists(nfoFile))
          {
            cleanFile = Path.GetFileNameWithoutExtension(path);
            Util.Utils.RemoveStackEndings(ref cleanFile);
            nfoFile = path + @"\" + cleanFile + nfoExt;
          }
        }
        else
        {
          string cleanFile = string.Empty;
          string strPath, strFilename;
          DatabaseUtility.Split(videoFile, out strPath, out strFilename);
          cleanFile = strFilename;
          Util.Utils.RemoveStackEndings(ref cleanFile);
          cleanFile = strPath + cleanFile;
          nfoFile = Path.ChangeExtension(cleanFile, nfoExt);
        }

        Log.Debug("Importing nfo:{0} using video file:{1}", nfoFile, videoFile);

        if (!File.Exists(nfoFile))
        {
          return;
        }

        IMDBMovie movie = new IMDBMovie();
        int id = GetMovieInfo(videoFile, ref movie);
        
        if (skipExisting && id > 0)
        {
          movie = null;
          return;
        }

        ImportNfo(nfoFile, skipExisting, refreshdbOnly);
        movie = null;
      }
      catch (Exception ex)
      {
        Log.Error("Error importing nfo for file {0} Error:{1} ", videoFile, ex);
      }
    }

    public void ImportNfo(string nfoFile, bool skipExisting, bool refreshdbOnly)
    {
      IMDBMovie movie = new IMDBMovie();
      bool isMovieFolder = Util.Utils.IsFolderDedicatedMovieFolder(Path.GetFullPath(nfoFile));
      bool useInternalNfoScraper = false;
      
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // Use only nfo scrapper
        useInternalNfoScraper = xmlreader.GetValueAsBool("moviedatabase", "useonlynfoscraper", false);
      }

      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(nfoFile);
        Log.Debug("Importing nfo file:{0}", nfoFile);

        if (doc.DocumentElement != null)
        {
          int id = -1;

          XmlNodeList movieList = doc.DocumentElement.SelectNodes("/movie");
          
          if (movieList == null)
          {
            Log.Debug("Movie tag for nfo file:{0} not exist. Nfo skipped.", nfoFile);
            return;
          }

          foreach (XmlNode nodeMovie in movieList)
          {
            string genre = string.Empty;
            string cast = string.Empty;
            string path = string.Empty;
            string nfofileName = string.Empty;
            
            #region nodes

            XmlNode nodeTitle = nodeMovie.SelectSingleNode("title");
            XmlNode nodeSortTitle = nodeMovie.SelectSingleNode("sorttitle");
            XmlNode nodeRating = nodeMovie.SelectSingleNode("rating");
            XmlNode nodeYear = nodeMovie.SelectSingleNode("year");
            XmlNode nodeDuration = nodeMovie.SelectSingleNode("runtime");
            XmlNode nodePlotShort = nodeMovie.SelectSingleNode("outline");
            XmlNode nodePlot = nodeMovie.SelectSingleNode("plot");
            XmlNode nodeTagline = nodeMovie.SelectSingleNode("tagline");
            XmlNode nodeDirector = nodeMovie.SelectSingleNode("director");
            XmlNode nodeDirectorImdb = nodeMovie.SelectSingleNode("directorimdb");
            XmlNode nodeImdbNumber = nodeMovie.SelectSingleNode("imdb");
            XmlNode nodeIdImdbNumber = nodeMovie.SelectSingleNode("id");
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

            #region Moviefiles

            // Get path from *.nfo file)
            Util.Utils.Split(nfoFile, out path, out nfofileName);
            // Movie filename to search from gathered files from nfo path
            nfofileName = Util.Utils.GetFilename(nfofileName, true);
            // Get all video files from nfo path
            ArrayList files = new ArrayList();
            GetVideoFiles(path, ref files);
            bool isDvdBdFolder = false;

            foreach (String file in files)
            {
              //Log.Debug("Import nfo-processing video file:{0} (Total files: {1})", file, files.Count);
              string logFilename = Path.GetFileName(file);

              if ((file.ToUpperInvariant().Contains("VIDEO_TS.IFO") ||
                  file.ToUpperInvariant().Contains("INDEX.BDMV")) && files.Count == 1)
              {
                var pattern = Util.Utils.StackExpression();
                int stackSequence = -1; // seq 0 = [x-y], seq 1 = CD1, Part1....
                int digit = 0;

                for (int i = 0; i < pattern.Length; i++)
                {
                  if (pattern[i].IsMatch(file))
                  {
                    digit = Convert.ToInt16(pattern[i].Match(file).Groups["digit"].Value);
                    stackSequence = i;
                    break;
                  }
                }
                if (digit > 1)
                {
                  Log.Debug("Import nfo-file: {0} is stack part.", file);
                  string filename;
                  string tmpPath = string.Empty;
                  DatabaseUtility.Split(file, out path, out filename);

                  try
                  {
                    if (stackSequence == 0)
                    {
                      string strReplace = "[" + digit;
                      int stackIndex = path.LastIndexOf(strReplace);
                      tmpPath = path.Remove(stackIndex, 2);
                      tmpPath = tmpPath.Insert(stackIndex, "[1");
                    }
                    else
                    {
                      int stackIndex = path.LastIndexOf(digit.ToString());
                      tmpPath = path.Remove(stackIndex, 1);
                      tmpPath = tmpPath.Insert(stackIndex, "1");
                    }

                    int movieId = VideoDatabase.GetMovieId(tmpPath + filename);
                    int pathId = VideoDatabase.AddPath(path);
                    Log.Debug("Import nfo-Adding file: {0}", logFilename);
                    VideoDatabase.AddFile(movieId, pathId, filename);
                    return;
                  }
                  catch(Exception ex)
                  {
                    Log.Error("Import nfo error-stack check for path {0} Error: {1}", path, ex.Message);
                    return;
                  }
                }

                id = VideoDatabase.AddMovie(file, true);
                movie.ID = id;
                isDvdBdFolder = true;
              }
              else
              {
                string tmpFile = string.Empty;
                string tmpPath = string.Empty;
                
                // Read filename
                Util.Utils.Split(file, out tmpPath, out tmpFile);
                // Remove extension
                tmpFile = Util.Utils.GetFilename(tmpFile, true);
                // Remove stack endings (CD1...)
                Util.Utils.RemoveStackEndings(ref tmpFile);
                Util.Utils.RemoveStackEndings(ref nfofileName);
                
                // Check and add to vdb and get movieId
                if (tmpFile.Equals(nfofileName, StringComparison.InvariantCultureIgnoreCase))
                {
                  Log.Debug("Import nfo-Adding file: {0}", logFilename);
                  id = VideoDatabase.AddMovie(file, true);
                  movie.ID = id;
                }
                else if (isMovieFolder && tmpPath.Length > 0) // Every movie in it's own folder, compare by folder name
                {
                  try
                  {
                    tmpPath = tmpPath.Substring(tmpPath.LastIndexOf(@"\") + 1).Trim();

                    if (tmpPath.Equals(nfofileName, StringComparison.InvariantCultureIgnoreCase) || nfofileName.ToLowerInvariant() == "movie")
                    {
                      Log.Debug("Import nfo-Adding file: {0}", logFilename);
                      id = VideoDatabase.AddMovie(file, true);
                      movie.ID = id;
                    }
                    else
                    {
                      Log.Debug("Import nfo-Skipping file:{0}", logFilename);
                    }
                  }
                  catch (Exception ex)
                  {
                    Log.Error("Import nfo-Error comparing path name. File:{0} Err.:{1}", file, ex.Message);
                  }
                }
                else
                {
                  Log.Debug("Import nfo-Skipping file: {0}", logFilename);
                }
              }
            }

            #endregion

            #region Check for existing movie or refresh database only
            
            GetMovieInfoById(id, ref movie);

            if (skipExisting && !movie.IsEmpty || refreshdbOnly && movie.IsEmpty || id < 1)
            {
              Log.Debug("Import nfo-Skipping import for movieId = {0}).", id);
              return;
            }

            movie = new IMDBMovie();
            movie.ID = id;

            #endregion

            #region Genre

            XmlNodeList genres = nodeMovie.SelectNodes("genre");
            
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
              genres = nodeMovie.SelectNodes("genres/genre");

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
            }

            movie.Genre = genre;
            
            #endregion

            #region Credits (Writers)

            // Writers
            if (nodeCredits != null)
            {
              movie.WritingCredits = nodeCredits.InnerText;
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

            #region Sort Title

            // SortTitle
            if (nodeSortTitle != null)
            {
              if (!string.IsNullOrEmpty(nodeTitle.InnerText))
              {
                movie.SortTitle = nodeSortTitle.InnerText;
              }
              else
              {
                movie.SortTitle = movie.Title;
              }
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

            if (string.IsNullOrEmpty(movie.IMDBNumber) && nodeIdImdbNumber != null)
            {
              if (CheckMovieImdbId(nodeIdImdbNumber.InnerText))
              {
                movie.IMDBNumber = nodeIdImdbNumber.InnerText;
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
              dirImdb = nodeDirectorImdb.InnerText;
              
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
              if (Double.TryParse(nodeRating.InnerText.Replace(".", ","), out rating))
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
                MatchCollection mc = Regex.Matches(nodeDuration.InnerText, regex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
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
                else
                {
                  regex = @"\d*\s*min.";
                  if (Regex.Match(nodeDuration.InnerText, regex, RegexOptions.IgnoreCase).Success)
                  {
                    regex = @"\d*";
                    int minutes = 0;
                    Int32.TryParse(Regex.Match(nodeDuration.InnerText, regex).Value, out minutes);
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
            int percent = 0;
            int watchedCount = 0;
            GetMovieWatchedStatus(movie.ID, out percent, out watchedCount);

            if (watchedCount < 1)
            {
              if (nodeWatched != null)
              {
                if (nodeWatched.InnerText.ToLowerInvariant() == "true" || nodeWatched.InnerText == "1")
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
                watchedCount = 0;
                Int32.TryParse(nodePlayCount.InnerText, out watchedCount);
                SetMovieWatchedCount(movie.ID, watchedCount);
                
                if (watchedCount > 0 && movie.Watched == 0)
                {
                  movie.Watched = 1;
                  VideoDatabase.SetMovieWatchedStatus(movie.ID, true, 100);
                }
                else if (watchedCount == 0 && movie.Watched > 0)
                {
                  SetMovieWatchedCount(movie.ID, 1);
                }
              }
            }
            else
            {
              movie.Watched = 1;
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
            string thumbJpgFile = string.Empty;
            string thumbTbnFile = string.Empty;
            string thumbFolderJpgFile = string.Empty;
            string thumbFolderTbnFile = string.Empty;
            string titleExt = movie.Title + "{" + id + "}";
            string jpgExt = @".jpg";
            string tbnExt = @".tbn";
            string folderJpg = @"\folder.jpg";
            string folderTbn = @"\folder.tbn";

            if (isDvdBdFolder)
            {
              thumbJpgFile = path + @"\" + Path.GetFileNameWithoutExtension(path) + jpgExt;
              thumbTbnFile = path + @"\" + Path.GetFileNameWithoutExtension(path) + tbnExt;
              thumbFolderJpgFile = path + @"\" + folderJpg;
              thumbFolderTbnFile = path + @"\" + folderTbn;
            }
            else
            {
              thumbJpgFile = path + @"\" + nfofileName + jpgExt;
              thumbTbnFile = path + @"\" + nfofileName + tbnExt;

              if (isMovieFolder)
              {
                thumbFolderJpgFile = path + @"\" + folderJpg;
                thumbFolderTbnFile = path + @"\" + folderTbn;
              }
            }

            if (nodePoster != null)
            {
              // Local source cover
              if (File.Exists(thumbJpgFile))
              {
                CreateCovers(titleExt, thumbJpgFile, movie);
              }
              else if (File.Exists(thumbTbnFile))
              {
                CreateCovers(titleExt, thumbTbnFile, movie);
              }
              else if (!string.IsNullOrEmpty(thumbFolderJpgFile) && File.Exists(thumbFolderJpgFile))
              {
                CreateCovers(titleExt, thumbFolderJpgFile, movie);
              }
              else if (!string.IsNullOrEmpty(thumbFolderTbnFile) && File.Exists(thumbFolderTbnFile))
              {
                CreateCovers(titleExt, thumbFolderTbnFile, movie);
              }
              else if (!nodePoster.InnerText.StartsWith("http:") && File.Exists(nodePoster.InnerText))
              {
                CreateCovers(titleExt, nodePoster.InnerText, movie);
              }
              else if (!nodePoster.InnerText.StartsWith("http:") && File.Exists(path + @"\" + nodePoster.InnerText))
              {
                CreateCovers(titleExt, path + @"\" + nodePoster.InnerText, movie);
              }
              // web source cover
              else if (nodePoster.InnerText.StartsWith("http:"))
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
                        imageExtension = jpgExt;
                      }
                      string temporaryFilename = "MPTempImage";
                      temporaryFilename += imageExtension;
                      temporaryFilename = Path.Combine(Path.GetTempPath(), temporaryFilename);
                      Util.Utils.FileDelete(temporaryFilename);
                      Util.Utils.DownLoadAndOverwriteCachedImage(imageUrl, temporaryFilename);
                        
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
                catch (Exception ex)
                {
                  Log.Error("Import nfo - Poster node: {0}", ex.Message);
                }
                movie.ThumbURL = nodePoster.InnerText;
              }
              // MP scrapers cover
              else
              {
                if (movie.ThumbURL == string.Empty && !useInternalNfoScraper)
                {
                  // IMPAwards
                  IMPAwardsSearch impSearch = new IMPAwardsSearch();

                  if (movie.Year > 1900)
                  {
                    impSearch.SearchCovers(movie.Title + " " + movie.Year, movie.IMDBNumber);
                  }
                  else
                  {
                    impSearch.SearchCovers(movie.Title, movie.IMDBNumber);
                  }

                  if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
                  {
                    movie.ThumbURL = impSearch[0];
                  }

                  // If no IMPAwards lets try TMDB 
                  TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();

                  if (impSearch.Count == 0)
                  {
                    tmdbSearch.SearchCovers(movie.Title, movie.IMDBNumber);

                    if ((tmdbSearch.Count > 0) && (tmdbSearch[0] != string.Empty))
                    {
                      movie.ThumbURL = tmdbSearch[0];
                    }
                  }
                  // All fail, last try IMDB
                  if (impSearch.Count == 0 && tmdbSearch.Count == 0)
                  {
                    IMDBSearch imdbSearch = new IMDBSearch();
                    imdbSearch.SearchCovers(movie.IMDBNumber, true);

                    if ((imdbSearch.Count > 0) && (imdbSearch[0] != string.Empty))
                    {
                      movie.ThumbURL = imdbSearch[0];
                    }
                  }
                }

                string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
                string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);

                if (movie.ID >= 0)
                {
                  if (!string.IsNullOrEmpty(movie.ThumbURL))
                  {
                    Util.Utils.FileDelete(largeCoverArt);
                    Util.Utils.FileDelete(coverArt);
                  }
                  
                  // Save cover thumbs
                  if (!string.IsNullOrEmpty(movie.ThumbURL))
                  {
                    IMDBFetcher.DownloadCoverArt(Thumbs.MovieTitle, movie.ThumbURL, titleExt);
                  }
                }
              }
            }
            else // Node thumb not exist
            {
              if (File.Exists(thumbJpgFile))
              {
                CreateCovers(titleExt, thumbJpgFile, movie);
              }
              else if (File.Exists(thumbTbnFile))
              {
                CreateCovers(titleExt, thumbTbnFile, movie);
              }
              else if (!string.IsNullOrEmpty(thumbFolderJpgFile) && File.Exists(thumbFolderJpgFile))
              {
                CreateCovers(titleExt, thumbFolderJpgFile, movie);
              }
              else if (!string.IsNullOrEmpty(thumbFolderTbnFile) && File.Exists(thumbFolderTbnFile))
              {
                CreateCovers(titleExt, thumbFolderTbnFile, movie);
              }
              else if (movie.ThumbURL == string.Empty && !useInternalNfoScraper)
              {
                // IMPAwards
                IMPAwardsSearch impSearch = new IMPAwardsSearch();

                if (movie.Year > 1900)
                {
                  impSearch.SearchCovers(movie.Title + " " + movie.Year, movie.IMDBNumber);
                }
                else
                {
                  impSearch.SearchCovers(movie.Title, movie.IMDBNumber);
                }

                if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
                {
                  movie.ThumbURL = impSearch[0];
                }

                // If no IMPAwards lets try TMDB 
                TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();

                if (impSearch.Count == 0)
                {
                  tmdbSearch.SearchCovers(movie.Title, movie.IMDBNumber);

                  if ((tmdbSearch.Count > 0) && (tmdbSearch[0] != string.Empty))
                  {
                    movie.ThumbURL = tmdbSearch[0];
                  }
                }
                // All fail, last try IMDB
                if (impSearch.Count == 0 && tmdbSearch.Count == 0)
                {
                  IMDBSearch imdbSearch = new IMDBSearch();
                  imdbSearch.SearchCovers(movie.IMDBNumber, true);

                  if ((imdbSearch.Count > 0) && (imdbSearch[0] != string.Empty))
                  {
                    movie.ThumbURL = imdbSearch[0];
                  }
                }
                
                string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
                string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);

                if (movie.ID >= 0)
                {
                  if (!string.IsNullOrEmpty(movie.ThumbURL))
                  {
                    Util.Utils.FileDelete(largeCoverArt);
                    Util.Utils.FileDelete(coverArt);
                  }

                  // Save cover thumbs
                  if (!string.IsNullOrEmpty(movie.ThumbURL))
                  {
                    IMDBFetcher.DownloadCoverArt(Thumbs.MovieTitle, movie.ThumbURL, titleExt);
                  }
                }
              }
            }

            #endregion

            #region Fanart

            // Fanart
            XmlNodeList fanartNodeList = nodeMovie.SelectNodes("fanart/thumb");

            int faIndex = 0;
            bool faFound = false;
            string faFile = string.Empty;
            FanArt fa = new FanArt();

            foreach (XmlNode fanartNode in fanartNodeList)
            {
              if (fanartNode != null)
              {
                faFile = path + @"\" + fanartNode.InnerText;
                
                if (File.Exists(faFile))
                {
                  fa.GetLocalFanart(id, "file://" + faFile, faIndex);
                  movie.FanartURL = faFile;
                  faFound = true;
                }
              }
              faIndex ++;

              if (faIndex == 5)
              {
                break;
              }
            }

            if (!faFound)
            {
              List<string> localFanart = new List<string>(); 
              faIndex = 0;
              faFile = path + @"\" + nfofileName + "-fanart.jpg";
              localFanart.Add(faFile);
              faFile = path + @"\" + nfofileName + "-backdrop.jpg";
              localFanart.Add(faFile);
              faFile= path + @"\" + "backdrop.jpg";
              localFanart.Add(faFile);
              faFile = path + @"\" + "fanart.jpg";
              localFanart.Add(faFile);

              foreach (string fanart in localFanart)
              {
                if (File.Exists(fanart))
                {
                  fa.GetLocalFanart(id, "file://" + fanart, faIndex);
                  movie.FanartURL = fanart;
                  faFound = true;
                  break;
                }
              }
              
              if (!faFound && !useInternalNfoScraper)
              {
                fa.GetTmdbFanartByApi(movie.ID, movie.IMDBNumber, string.Empty, false, 1, string.Empty);
              }
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

            #region UserGroups

            XmlNodeList userGroups = nodeMovie.SelectNodes("set");
            
            // Main node as <set> ---- </set> with subnodes name, rule, image
            foreach (XmlNode nodeUserGroup in userGroups)
            {
              if (nodeUserGroup != null)
              {
                string name = string.Empty;
                string description = string.Empty;
                string rule = string.Empty;
                string image = string.Empty;
                XmlNode nodeSetName = nodeUserGroup.SelectSingleNode("setname");
                XmlNode nodeSetDescription = nodeUserGroup.SelectSingleNode("setdescription");
                XmlNode nodeSetRule = nodeUserGroup.SelectSingleNode("setrule");
                XmlNode nodeSetImage = nodeUserGroup.SelectSingleNode("setimage");

                if (nodeSetName != null && nodeSetName.InnerText != null)
                {
                  name = nodeSetName.InnerText;
                }

                if (nodeSetDescription != null && nodeSetDescription.InnerText != null)
                {
                  description = nodeSetDescription.InnerText;
                }

                if (nodeSetRule != null && nodeSetRule.InnerText != null)
                {
                  rule = nodeSetRule.InnerText;
                }

                if (nodeSetImage != null && nodeSetImage.InnerText != null)
                {
                  image = nodeSetImage.InnerText;
                  image = string.Format("{0}/{1}", path, image);
                }
                
                if (!string.IsNullOrEmpty(name))
                {
                  int iUserGroup = AddUserGroup(name);
                  AddUserGroupToMovie(movie.ID, iUserGroup);

                  if (!string.IsNullOrEmpty(description))
                  {
                    AddUserGroupDescription(name, description);
                  }

                  if (!string.IsNullOrEmpty(rule))
                  {
                    bool error = false;
                    string errorMessage = string.Empty;

                    VideoDatabase.ExecuteSql(rule, out error, out errorMessage);

                    if (!error)
                    {
                      AddUserGroupRuleByGroupId(iUserGroup, rule);
                    }
                    else
                    {
                      Log.Error("VideoDatabase nfo import: error adding user group {0} rule: {1}", name, rule);
                    }
                  }

                  // Only local image
                  if (!string.IsNullOrEmpty(image) && File.Exists(image))
                  {
                    string smallThumb = Util.Utils.GetCoverArtName(Thumbs.MovieUserGroups, Path.GetFileNameWithoutExtension(image));
                    string largeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieUserGroups, Path.GetFileNameWithoutExtension(image));
                    Util.Utils.FileDelete(smallThumb);
                    Util.Utils.FileDelete(largeThumb);

                    if (Util.Picture.CreateThumbnail(image, smallThumb, (int) Thumbs.ThumbResolution,
                      (int) Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
                    {
                      Util.Picture.CreateThumbnail(image, largeThumb, (int) Thumbs.ThumbLargeResolution,
                        (int) Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
                    }
                  }
                }
                else // Single node as <set>setname</set>
                {
                  name = nodeUserGroup.InnerText;

                  if (!string.IsNullOrEmpty(name))
                  {
                    int iUserGroup = AddUserGroup(name);
                    AddUserGroupToMovie(movie.ID, iUserGroup);
                  }
                }
              }
            }

            VideoDatabase.SetMovieInfoById(id, ref movie, true);

            // Add groups with rules
            ArrayList groups = new ArrayList();
            VideoDatabase.GetUserGroups(groups);

            foreach (string group in groups)
            {
              string rule = VideoDatabase.GetUserGroupRule(group);

              if (!string.IsNullOrEmpty(rule))
              {
                try
                {
                  ArrayList values = new ArrayList();
                  bool error = false;
                  string errorMessage = string.Empty;
                  values = VideoDatabase.ExecuteRuleSql(rule, "movieinfo.idMovie", out error, out errorMessage);

                  if (error)
                  {
                    Log.Error("VideoDatabase nfo import: error executing rule {0} syntax, {1}", rule, errorMessage);
                    continue;
                  }

                  if (values.Count > 0 && values.Contains(movie.ID.ToString()))
                  {
                    VideoDatabase.AddUserGroupToMovie(movie.ID, VideoDatabase.AddUserGroup(group));
                  }
                }
                catch (Exception ex)
                {
                  Log.Error("VideoDatabase nfo import: error importing usergroup rule {0} - {1}", rule, ex.Message);
                }
              }
            }

            #endregion

            
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception error importing nfo file {0}:{1} ", nfoFile, ex.Message);
      }
    }

    public bool MakeNfo (int movieId)
    {
      string moviePath = string.Empty;
      string movieFile = string.Empty;
      ArrayList movieFiles = new ArrayList();
      ArrayList nfoFiles = new ArrayList();
      string nfoFile = string.Empty;
      int fileCounter = 0;

      try
      {
        // Get files
        GetFilesForMovie(movieId, ref movieFiles);

        foreach (string file in movieFiles)
        {
          if (!File.Exists(file))
          {
            return false;
          }

          movieFile = file;
          Util.Utils.Split(movieFile, out moviePath, out movieFile);

          // Check for DVD folder
          if (movieFile.ToUpperInvariant() == "VIDEO_TS.IFO" || movieFile.ToUpperInvariant() == "INDEX.BDMV")
          {
            // Remove \VIDEO_TS from directory structure
            string directoryDVD = moviePath.Substring(0, moviePath.LastIndexOf(@"\"));

            if (Directory.Exists(directoryDVD))
            {
              moviePath = directoryDVD;
              movieFile = directoryDVD;
            }
          }
          else
          {
            if (fileCounter > 0)
            {
              return true;
            }
          }
          // remove stack endings (CDx..) form filename
          Util.Utils.RemoveStackEndings(ref movieFile);
          // Remove file extension
          movieFile = Util.Utils.GetFilename(movieFile, true).Trim();
          // Add nfo extension
          nfoFile = moviePath + @"\" + movieFile + ".nfo";
          Util.Utils.FileDelete(nfoFile);
          nfoFiles.Add(nfoFile);
          //}

          IMDBMovie movieDetails = new IMDBMovie();
          GetMovieInfoById(movieId, ref movieDetails);
          // Prepare XML
          XmlDocument doc = new XmlDocument();
          XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

          // Main tag
          XmlNode mainNode = doc.CreateElement("movie");
          XmlNode subNode;

          #region Movie fields

          // Filenames
          foreach (string strMovieFile in movieFiles)
          {
            CreateXmlNode(mainNode, doc, "filenameandpath", strMovieFile);
          }

          // Title
          CreateXmlNode(mainNode, doc, "title", movieDetails.Title);
          // Sort Title
          if (!string.IsNullOrEmpty(movieDetails.SortTitle))
          {
            CreateXmlNode(mainNode, doc, "sorttitle", movieDetails.SortTitle);
          }
          else
          {
            CreateXmlNode(mainNode, doc, "sorttitle", movieDetails.Title);
          }

          //  movie IMDB number
          CreateXmlNode(mainNode, doc, "imdb", movieDetails.IMDBNumber);
          CreateXmlNode(mainNode, doc, "id", movieDetails.IMDBNumber);
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
            try
            {
              File.Copy(largeCoverArtImage, coverFilename, true);
              File.SetAttributes(coverFilename, FileAttributes.Normal);
              CreateXmlNode(mainNode, doc, "thumb", movieFile + ".jpg");
            }
            catch (Exception ex)
            {
              Log.Info("VideoDatabase: Error in creating nfo - poster node:{0}", ex.Message);
            }
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

              try
              {
                string faFilename = moviePath + @"\" + movieFile + "-fanart" + index + ".jpg";
                File.Copy(faFile, faFilename, true);
                File.SetAttributes(faFilename, FileAttributes.Normal);
                CreateXmlNode(subNode, doc, "thumb", movieFile + "-fanart" + index + ".jpg");
              }
              catch (Exception ex)
              {
                Log.Info("VideoDatabas: Error in creating nfo - fanart section:{0}", ex.Message);
              }

            }
          }
          mainNode.AppendChild(subNode);

          // Genre
          string szGenres = movieDetails.Genre;

          if (szGenres.IndexOf("/") >= 0 || szGenres.IndexOf("|") >= 0)
          {
            Tokens f = new Tokens(szGenres, new[] {'/', '|'});

            foreach (string strGenre in f)
            {
              if (!string.IsNullOrEmpty(strGenre))
              {
                CreateXmlNode(mainNode, doc, "genre", strGenre.Trim());
              }
            }
          }
          else
          {
            CreateXmlNode(mainNode, doc, "genre", movieDetails.Genre);
          }

          // Cast
          ArrayList castList = new ArrayList();
          GetActorsByMovieID(movieId, ref castList);

          foreach (string actor in castList)
          {
            IMDBActor actorInfo = new IMDBActor();
            subNode = doc.CreateElement("actor");

            char[] splitter = {'|'};
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

          // User groups
          ArrayList userGroups = new ArrayList();
          GetMovieUserGroups(movieId, userGroups);
          
          if (userGroups.Count > 0)
          {
            foreach (string userGroup in userGroups)
            {
              subNode = doc.CreateElement("set");
              CreateXmlNode(subNode, doc, "setname", userGroup);

              string rule = GetUserGroupRule(userGroup);
              string description = GetUserGroupDescriptionById(GetUserGroupId(userGroup));

              if (!string.IsNullOrEmpty(rule))
              {
                CreateXmlNode(subNode, doc, "setrule", rule);
              }

              if (!string.IsNullOrEmpty(description))
              {
                CreateXmlNode(subNode, doc, "setdescription", description);
              }

              // Image is not exportable beacuse it is already resized and not in original quality
              //CreateXmlNode(subNode, doc, "setimage", string.Empty);

              mainNode.AppendChild(subNode);
            }
          }

          // Trailer
          CreateXmlNode(mainNode, doc, "trailer", string.Empty);

          #endregion

          // End and save
          doc.AppendChild(mainNode);
          doc.InsertBefore(xmldecl, mainNode);
          doc.Save(nfoFile);
          fileCounter++;
        }
      }
      catch(Exception ex)
      {
        Log.Info("VideoDatabase: Error in creating nfo file:{0} Error:{1}", nfoFile ,ex.Message);
        return false;
      }

      return true;
    }

    private void CreateXmlNode(XmlNode mainNode, XmlDocument doc, string element, string innerTxt)
    {
      XmlNode subNode = doc.CreateElement(element);
      subNode.InnerText = innerTxt;
      mainNode.AppendChild(subNode);
    }

    private void CreateCovers(string titleExt, string coverImage, IMDBMovie movie)
    {
      movie.ThumbURL = coverImage;
      string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
      string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
      Util.Utils.FileDelete(largeCoverArt);
      Util.Utils.FileDelete(coverArt);

      if (Util.Picture.CreateThumbnail(coverImage, largeCoverArt, (int)Thumbs.ThumbLargeResolution,
                                       (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsSmall))
      {
        Util.Picture.CreateThumbnail(coverImage, coverArt, (int)Thumbs.ThumbResolution,
                                     (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsLarge);
      }
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
        using (Settings xmlReaderWriter = new MPSettings())
        {
          _currentCreateVideoThumbs = xmlReaderWriter.GetValueAsBool("thumbnails", "videoondemand", true);
          xmlReaderWriter.SetValueAsBool("thumbnails", "videoondemand", false);
        }

        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true, false);
        
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            if (item.Label != "..")
            {
              if (item.Path.ToUpperInvariant().IndexOf(@"\VIDEO_TS") >= 0)
              {
                string strFile = String.Format(@"{0}\VIDEO_TS.IFO", item.Path);
                availableFiles.Add(strFile);
              }
              else if (item.Path.ToUpperInvariant().IndexOf(@"\BDMV") >= 0)
              {
                string strFile = String.Format(@"{0}\index.bdmv", item.Path);
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
              string extension = Path.GetExtension(item.Path);

              if (extension != null && extension.ToUpperInvariant() != @".IFO" && extension.ToUpperInvariant() != ".BDMV")
              {
                availableFiles.Add(item.Path);
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("VideoDatabase: Exception counting video files:{0}", e);
      }
      finally
      {
        // Restore thumbcreation setting
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("thumbnails", "videoondemand", _currentCreateVideoThumbs);
        }
      }
    }

    private void SetLatestMovieProperties()
    {
      string strSQL = "SELECT * FROM movieinfo ORDER BY dateAdded DESC LIMIT 3";
      ArrayList movies = new ArrayList();
      GetMoviesByFilter(strSQL, out movies, false, true, false, false);
      
      IMDBMovie movie1 = new IMDBMovie();
      IMDBMovie movie2 = new IMDBMovie();
      IMDBMovie movie3 = new IMDBMovie();

      GUIPropertyManager.SetProperty("#myvideos.latest1.enabled", "false");
      GUIPropertyManager.SetProperty("#myvideos.latest2.enabled", "false");
      GUIPropertyManager.SetProperty("#myvideos.latest3.enabled", "false");

      if (movies.Count > 0)
      {
        movie1 = (IMDBMovie) movies[0];
        // Movie 1
        GUIPropertyManager.SetProperty("#myvideos.latest1.genre", movie1.Genre.Replace(" /", ","));
        //
        string poster = string.Empty;
        string titleExt = movie1.Title + "{" + movie1.ID + "}";
        poster = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
        GUIPropertyManager.SetProperty("#myvideos.latest1.thumb", poster);
        //
        GUIPropertyManager.SetProperty("#myvideos.latest1.title", movie1.Title);
        GUIPropertyManager.SetProperty("#myvideos.latest1.year", movie1.Year.ToString());
        //
        DateTime dateAdded;
        DateTime.TryParseExact(movie1.DateAdded, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateAdded);
        GUIPropertyManager.SetProperty("#myvideos.latest1.dateAdded", dateAdded.ToShortDateString());
        //
        GUIPropertyManager.SetProperty("#myvideos.latest1.runtime", movie1.RunTime +
                                " " +
                                GUILocalizeStrings.Get(2998) +
                                " (" + Util.Utils.SecondsToHMString(movie1.RunTime * 60) + ")");
        GUIPropertyManager.SetProperty("#myvideos.latest1.enabled", "true");

        if (movies.Count > 1)
        {
          movie2 = (IMDBMovie) movies[1];
          // Movie 2
          GUIPropertyManager.SetProperty("#myvideos.latest2.genre", movie2.Genre.Replace(" /", ","));
          //
          poster = string.Empty;
          titleExt = movie2.Title + "{" + movie2.ID + "}";
          poster = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
          GUIPropertyManager.SetProperty("#myvideos.latest2.thumb", poster);
          //
          GUIPropertyManager.SetProperty("#myvideos.latest2.title", movie2.Title);
          GUIPropertyManager.SetProperty("#myvideos.latest2.year", movie2.Year.ToString());
          //
          DateTime.TryParseExact(movie2.DateAdded, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateAdded);
          GUIPropertyManager.SetProperty("#myvideos.latest2.dateAdded", dateAdded.ToShortDateString());
          //
          GUIPropertyManager.SetProperty("#myvideos.latest2.runtime", movie2.RunTime +
                                  " " +
                                  GUILocalizeStrings.Get(2998) +
                                  " (" + Util.Utils.SecondsToHMString(movie2.RunTime * 60) + ")");
          GUIPropertyManager.SetProperty("#myvideos.latest2.enabled", "true");
        }

        if (movies.Count > 2)
        {
          movie3 = (IMDBMovie) movies[2];
          // Movie 3
          GUIPropertyManager.SetProperty("#myvideos.latest3.genre", movie3.Genre.Replace(" /", ","));
          //
          poster = string.Empty;
          titleExt = movie3.Title + "{" + movie3.ID + "}";
          poster = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
          GUIPropertyManager.SetProperty("#myvideos.latest3.thumb", poster);
          //
          GUIPropertyManager.SetProperty("#myvideos.latest3.title", movie3.Title);
          GUIPropertyManager.SetProperty("#myvideos.latest3.year", movie3.Year.ToString());
          //
          DateTime.TryParseExact(movie3.DateAdded, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateAdded);
          GUIPropertyManager.SetProperty("#myvideos.latest3.dateAdded", dateAdded.ToShortDateString());
          //
          GUIPropertyManager.SetProperty("#myvideos.latest3.runtime", movie3.RunTime +
                                  " " +
                                  GUILocalizeStrings.Get(2998) +
                                  " (" + Util.Utils.SecondsToHMString(movie3.RunTime * 60) + ")");
          GUIPropertyManager.SetProperty("#myvideos.latest3.enabled", "true");
        }
      }
    }

    private void SetMovieDetails(ref IMDBMovie details, int iRow, SQLiteResultSet results)
    {
      if (details == null || iRow < 0 || results == null)
      {
        return;
      }

      double rating = 0;
      Double.TryParse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"), out rating);
      details.Rating = (float)rating;

      if (details.Rating > 10.0f)
      {
        details.Rating /= 10.0f;
      }

      details.Director = DatabaseUtility.Get(results, iRow, "movieinfo.strDirector").Replace("''", "'");

      // Add directorID
      try
      {
        details.DirectorID = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idDirector"));
      }
      catch (Exception)
      {
        details.DirectorID = -1;
      }
      
      details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits").Replace("''", "'");
      details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine").Replace("''", "'");
      details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline").Replace("''", "'");
      details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot").Replace("''", "'");
      // Added user review
      details.UserReview = DatabaseUtility.Get(results, iRow, "movieinfo.strUserReview").Replace("''", "'");
      details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
      details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast").Replace("''", "'");
      details.Year = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
      details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
      details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
      // Fanart
      details.FanartURL = DatabaseUtility.Get(results, iRow, "movieinfo.strFanartURL");
      // Date Added
      details.DateAdded = DatabaseUtility.Get(results, iRow, "movieinfo.dateAdded");
      // Date Watched
      details.DateWatched = DatabaseUtility.Get(results, iRow, "movieinfo.dateWatched");
      details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle").Replace("''", "'");
      details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
      details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
      details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
      Int32 lMovieId = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
      details.SearchString = String.Format("{0}", details.Title);
      details.CDLabel = DatabaseUtility.Get(results, iRow, "path.cdlabel");
      details.MPARating = DatabaseUtility.Get(results, iRow, "movieinfo.mpaa");
      int runtime = 0;
      Int32.TryParse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"), out runtime);
      details.RunTime = runtime;
      int watched = 0;
      Int32.TryParse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"), out watched);
      details.Watched = watched;
      details.ID = lMovieId;
      details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
      details.Country = DatabaseUtility.Get(results, iRow, "movieinfo.country");
      details.Language = DatabaseUtility.Get(results, iRow, "movieinfo.language");
      details.LastUpdate = DatabaseUtility.Get(results, iRow, "movieinfo.lastupdate");
      details.SortTitle = DatabaseUtility.Get(results, iRow, "movieinfo.strSortTitle").Replace("''", "'");

      if (string.IsNullOrEmpty(details.Path) && details.ID > 0)
      {
        string strSQL = String.Format(
          "SELECT path.strPath FROM movie,path WHERE path.idpath=movie.idpath AND movie.idMovie = {0}", details.ID);
        results = m_db.Execute(strSQL);
        details.Path = DatabaseUtility.Get(results, 0, "path.strPath");
      }

      if (details.ID > 0)
      {
        int percent = 0;
        int watchedCount = 0;
        GetMovieWatchedStatus(details.ID, out percent, out watchedCount);
        details.WatchedPercent = percent;
        details.WatchedCount = watchedCount;

        string movieFilename = string.Empty;
        ArrayList files = new ArrayList();
        GetFilesForMovie(details.ID, ref files);

        int duration = GetMovieDuration(details.ID);
        details.Duration = duration;

        if (files.Count > 0)
        {
          movieFilename = (string) files[0];
        }

        details.VideoFileName = movieFilename;
        details.VideoFilePath = details.Path;

        VideoFilesMediaInfo mInfo = new VideoFilesMediaInfo();
        GetVideoFilesMediaInfo(details.ID, ref mInfo);
        details.MediaInfo = mInfo;
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

    public void FlushTransactionsToDisk()
    {
      try
      {
        m_db.Execute("PRAGMA synchronous='FULL'");
      }
      catch (Exception ex)
      {
        Log.Error("VideoDatabase FlushTransactionsToDisk() exception: {0}", ex.Message);
      }
    }

    public void RevertFlushTransactionsToDisk()
    {
      try
      {
        m_db.Execute("PRAGMA synchronous='OFF'");
      }
      catch (Exception ex)
      {
        Log.Error("VideoDatabase RevertFlushTransactionsToDisk() exception: {0}", ex.Message);
      }
    }
  }
}