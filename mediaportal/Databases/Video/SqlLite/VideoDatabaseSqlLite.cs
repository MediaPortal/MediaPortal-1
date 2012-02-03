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
          }
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
        // Studios
        if (DatabaseUtility.TableColumnExists(m_db, "movieinfo", "studios") == false)
        {
          string strSQL = "ALTER TABLE \"main\".\"movieinfo\" ADD COLUMN \"studios\" TEXT DEFAULT ''";
          m_db.Execute(strSQL);
        }
        // Actor death infos
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
      DatabaseUtility.AddTable(m_db, "movie",
                               "CREATE TABLE movie ( idMovie integer primary key, idPath integer, hasSubtitles integer, discid text, watched bool)");
      DatabaseUtility.AddTable(m_db, "movieinfo",
                               "CREATE TABLE movieinfo ( idMovie integer, idDirector integer, strDirector text, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text, mpaa text,runtime integer, iswatched integer, strUserReview text, strFanartURL text, dateAdded timestamp, dateWatched timestamp, studios text)");
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
                               "CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text, thumbURL text, IMDBActorID text, dateofdeath text, placeofdeath text,)");
      DatabaseUtility.AddTable(m_db, "actorinfomovies",
                               "CREATE TABLE actorinfomovies ( idActor integer, idDirector integer, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text, mpaa text, runtime integer, iswatched integer, role text)");
      DatabaseUtility.AddTable(m_db, "IMDBmovies",
                               "CREATE TABLE IMDBmovies ( idIMDB text, idTmdb text, strPlot text, strCast text, strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, mpaa text)");
      DatabaseUtility.AddTable(m_db, "VideoThumbBList",
                               "CREATE TABLE VideoThumbBList ( idVideoThumbBList integer primary key, strPath text, strExpires text, strFileDate text, strFileSize text)");
      DatabaseUtility.AddTable(m_db, "filesmediainfo",
                               "CREATE TABLE filesmediainfo ( idFile integer primary key, videoCodec text, videoResolution text, aspectRatio text, hasSubtitles bool, audioCodec text, audioChannels text)");
      DatabaseUtility.AddIndex(m_db, "idxactorinfo_idActor",
                               "CREATE INDEX idxactorinfo_idActor ON actorinfo(idActor ASC)");
      DatabaseUtility.AddIndex(m_db, "idxactorinfomovies_idActor",
                               "CREATE INDEX idxactorinfomovies_idActor ON actorinfomovies(idActor ASC)");
      DatabaseUtility.AddIndex(m_db, "idxactorlinkmovie_idActor",
                               "CREATE INDEX idxactorlinkmovie_idActor ON actorlinkmovie(idActor ASC)");
      DatabaseUtility.AddIndex(m_db, "idxactorlinkmovie_idMovie",
                               "CREATE INDEX idxactorlinkmovie_idMovie ON actorlinkmovie(idMovie ASC)");
      DatabaseUtility.AddIndex(m_db, "idxactors_strActor", "CREATE INDEX idxactors_strActor ON actors(strActor ASC)");
      DatabaseUtility.AddIndex(m_db, "idxfiles_idMovie", "CREATE INDEX idxfiles_idMovie ON files(idMovie ASC)");
      DatabaseUtility.AddIndex(m_db, "idxfiles_idPath", "CREATE INDEX idxfiles_idPath ON files(idPath ASC)");
      DatabaseUtility.AddIndex(m_db, "idxgenrelinkmovie_idGenre",
                               "CREATE INDEX idxgenrelinkmovie_idGenre ON genrelinkmovie(idGenre ASC)");
      DatabaseUtility.AddIndex(m_db, "idxgenrelinkmovie_idMovie",
                               "CREATE INDEX idxgenrelinkmovie_idMovie ON genrelinkmovie(idMovie ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovie_idPath", "CREATE INDEX idxmovie_idPath ON movie(idPath ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_iYear", "CREATE INDEX idxmovieinfo_iYear ON movieinfo(iYear ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_idDirector",
                               "CREATE INDEX idxmovieinfo_idDirector ON movieinfo(idDirector ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_idMovie",
                               "CREATE UNIQUE INDEX idxmovieinfo_idMovie ON movieinfo(idMovie ASC)");
      DatabaseUtility.AddIndex(m_db, "idxmovieinfo_strTitle",
                               "CREATE INDEX idxmovieinfo_strTitle ON movieinfo(strTitle ASC)");
      DatabaseUtility.AddIndex(m_db, "idxpath_strPath", "CREATE INDEX idxpath_strPath ON path(strPath ASC)");
      DatabaseUtility.AddIndex(m_db, "idxVideoThumbBList_strPath",
                               "CREATE INDEX idxVideoThumbBList_strPath ON VideoThumbBList(strPath ASC, strExpires ASC)");
      DatabaseUtility.AddIndex(m_db, "idxVideoThumbBList_strExpires",
                               "CREATE INDEX idxVideoThumbBList_strExpires ON VideoThumbBList(strExpires ASC)");

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
          CheckMediaInfo(strFileName, string.Empty, lPathId, lFileId);
          return lFileId;
        }
        strSQL = String.Format("insert into files (idFile, idMovie,idPath, strFileName) values(null, {0},{1},'{2}')",
                               lMovieId, lPathId, strFileName);
        results = m_db.Execute(strSQL);
        lFileId = m_db.LastInsertID();
        CheckMediaInfo(strFileName, string.Empty, lPathId, lFileId);
        return lFileId;
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    // Check and add, if necessary, media info for video files
    // Use (file, pathID and fileID) or (full filename with path and fileID)
    private void CheckMediaInfo(string file, string fullPathFilename, int pathID, int fileID)
    {
      string strSQL = string.Empty;
      string strFilenameAndPath = string.Empty;
      
      // Get path name from pathID
      strSQL = String.Format("select * from path where idPath={0}", pathID);
      SQLiteResultSet results = m_db.Execute(strSQL);
      
      // No ftp or http videos
      string path = DatabaseUtility.Get(results, 0, "strPath");
      if (path.IndexOf("remote:") >= 0 || path.IndexOf("http:") >= 0)
        return;
      
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
        return;

      // Check if we processed file allready
      strSQL = String.Format("select * from filesmediainfo where idFile={0}", fileID);

      results = m_db.Execute(strSQL);
      if (results.Rows.Count == 0)
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
            if(!DaemonTools.Mount(strFilenameAndPath, out drive))
              return;
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
          strSQL = String.Format(
              "insert into filesmediainfo (idFile, videoCodec, videoResolution, aspectRatio, hasSubtitles, audioCodec, audioChannels) values({0},'{1}','{2}','{3}',{4},'{5}','{6}')",
              fileID,
              Util.Utils.MakeFileName(mInfo.VideoCodec),
              mInfo.VideoResolution,
              mInfo.AspectRatio,
              subtitles,
              Util.Utils.MakeFileName(mInfo.AudioCodec),
              mInfo.AudioChannelsFriendly);
          // Prevent empty record for future or unknown codecs
          if (mInfo.VideoCodec == string.Empty)
            return;
          m_db.Execute(strSQL);
        }
        catch (Exception) {}
      }
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
        GetFiles(lMovieId, ref files);

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

    public void GetFiles(int lMovieId, ref ArrayList movies)
    {
      try
      {
        movies.Clear();
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
          movies.Add(strFile);
        }
      }
      catch (Exception ex)
      {
        Log.Error("videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetVideoFilesMediaInfo(string strFilenameAndPath, ref VideoFilesMediaInfo mediaInfo)
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
        if (results.Rows.Count == 0)
        {
          try
          {
            CheckMediaInfo(string.Empty, strFilenameAndPath, -1, fileID);
            results = m_db.Execute(strSQL);
          }
          catch (Exception) {}
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

        string strSQL = "select * from Actors where IMDBActorId = '";
        strSQL += strActorImdbId;
        strSQL += "'";
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

        // add director
        int lDirector = details1.DirectorID;
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
              "insert into movieinfo ( idMovie, idDirector, strPlotOutline, strPlot, strTagLine, strVotes, fRating, strCast, strCredits, iYear, strGenre, strPictureURL, strTitle, IMDBID, mpaa, runtime, iswatched, strUserReview, strFanartURL, strDirector, dateAdded, studios) values({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9},'{10}','{11}','{12}','{13}','{14}',{15},{16},'{17}','{18}','{19}','{20}','{21}')",
              lMovieId, lDirector, details1.PlotOutline,
              details1.Plot, details1.TagLine,
              details1.Votes, strRating,
              details1.Cast, details1.WritingCredits,
              details1.Year, details1.Genre,
              details1.ThumbURL, details1.Title,
              details1.IMDBNumber, details1.MPARating, details1.RunTime, details1.Watched, details1.UserReview,
              details1.FanartURL, details1.Director, details1.DateAdded, details1.Studios);

          //			Log.Error("dbs:{0}", strSQL);
          SQLiteResultSet result = m_db.Execute(strSQL);
        }
        else
        {
          // Update movie info (no dateAdded update)
          strSQL =
            String.Format(
              "update movieinfo set idDirector={0}, strPlotOutline='{1}', strPlot='{2}', strTagLine='{3}', strVotes='{4}', fRating='{5}', strCast='{6}',strCredits='{7}', iYear={8}, strGenre='{9}', strPictureURL='{10}', strTitle='{11}', IMDBID='{12}', mpaa='{13}', runtime={14}, iswatched={15} , strUserReview='{16}', strFanartURL='{17}' , strDirector ='{18}', dateWatched='{19}', studios = '{20}' where idMovie={21}",
              lDirector, details1.PlotOutline,
              details1.Plot, details1.TagLine,
              details1.Votes, strRating,
              details1.Cast, details1.WritingCredits,
              details1.Year, details1.Genre,
              details1.ThumbURL, details1.Title,
              details1.IMDBNumber,
              details1.MPARating, details1.RunTime,
              details1.Watched, details1.UserReview, details1.FanartURL, details1.Director, details1.DateWatched ,details1.Studios, lMovieId);

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
          details1.Director = details1.Director.Replace("''", "'");
          details1.Studios = details1.Studios.Replace("''", "'");
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
        VideoDatabase.GetFiles((int)lMovieId, ref files);

        foreach (string file in files)
        {
          int fileId = VideoDatabase.GetFileId(file);
          VideoDatabase.DeleteMovieStopTime(fileId);
        }

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
          "select * from movieinfo,movie,path where path.idpath=movie.idpath and movie.idMovie=movieinfo.idMovie and movieinfo.idmovie={0}",//" and idDirector=idActor",
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
        // Add directorID
        try 
        {
          details.DirectorID = Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.idDirector"));
        }
        catch (Exception)
        {

          details.DirectorID = -1;
        }
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

    public int GetMovieDuration(int iFileId)
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

    public void SetMovieDuration(int iFileId, int duration)
    {
      try
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
            iWatched = 1;
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

    public bool GetMovieWatchedStatus(int idMovie, ref int percent)
    {
      try
      {
        percent = 0;
        string sql = String.Format("select * from movie where idMovie={0}", idMovie);
        SQLiteResultSet results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          return false;
        }
        int watched;
        int.TryParse(DatabaseUtility.Get(results, 0, "watched"), out watched);
        int.TryParse(DatabaseUtility.Get(results, 0, "iwatchedPercent"), out percent);
        
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
          // Add directorID
          try 
          {
            details.DirectorID = Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.idDirector"));
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
          "select * from genrelinkmovie,genre,movie,movieinfo,actors,path where path.idpath=movie.idpath and genrelinkmovie.idGenre=genre.idGenre and genrelinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and genre.strGenre='{0}' and movieinfo.iddirector=actors.idActor",
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
          details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
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
          details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
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
          "select * from movie,movieinfo,actors,path where path.idpath=movie.idpath and movieinfo.idmovie=movie.idmovie and movieinfo.iddirector=actors.idActor and movieinfo.iYear={0}",
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
          details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
          details.RunTime = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          details.Studios = DatabaseUtility.Get(results, iRow, "movieinfo.studios");
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
                                  bool genreTable)
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
          if (movieinfoTable)
          {
            movie.Rating = (float)Double.Parse(DatabaseUtility.Get(results, i, "movieinfo.fRating"));
            if (movie.Rating > 10.0f)
            {
              movie.Rating /= 10.0f;
            }
            movie.Director = DatabaseUtility.Get(results, i, "movieinfo.strDirector");
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
            // FanArt search need this (for database GUI view)
            // Share view is handled in GUIVideoFIles class)
            ArrayList files = new ArrayList();
            GetFiles(movie.ID, ref files);
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
        if (movie.MovieImdbID.ToLower().StartsWith("tt"))
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
          actor.id = Convert.ToInt32(DatabaseUtility.Get(results, 0, "actorinfo.idActor"));

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

    public void ExecuteSQL (string strSql)
    {
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
        Open();
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