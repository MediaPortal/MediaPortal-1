#region Copyright (C) 2007 Team MediaPortal

/* 
 *	Copyright (C) 2007 Team MediaPortal
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

namespace MediaPortal.Music.Database
{
  #region Usings
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;

  using SQLite.NET;

  using MediaPortal.Configuration;
  using MediaPortal.Database;
  using MediaPortal.ServiceImplementations;
  using MediaPortal.TagReader;
  using MediaPortal.Util;
  #endregion

  #region Reorg class
  public delegate void MusicDBReorgEventHandler(object sender, DatabaseReorgEventArgs e);

  public class DatabaseReorgEventArgs : System.EventArgs
  {
    public int progress;
    // Provide one or more constructors, as well as fields and
    // accessors for the arguments.
    public string phase;
  }
  #endregion

  public partial class MusicDatabase
  {
    #region Cache classes
    public class AlbumInfoCache : AlbumInfo
    {
      public int idAlbum = 0;
      public int idArtist = 0;
      public int idAlbumArtist = 0;
      public int idPath = -1;
    };

    private ArrayList _albumCache = new ArrayList();
    private ArrayList _shares = new ArrayList();
    #endregion

    #region Reorg events
    // An event that clients can use to be notified whenever the
    // elements of the list change.
    public static event MusicDBReorgEventHandler DatabaseReorgChanged;

    // Invoke the Changed event; called whenever list changes
    protected virtual void OnDatabaseReorgChanged(DatabaseReorgEventArgs e)
    {
      if (DatabaseReorgChanged != null)
        DatabaseReorgChanged(this, e);
    }
    #endregion

    private enum Errors
    {
      ERROR_OK = 317,
      ERROR_CANCEL = 0,
      ERROR_DATABASE = 315,
      ERROR_REORG_SONGS = 319,
      ERROR_REORG_ARTIST = 321,
      ERROR_REORG_ALBUMARTIST = 322,
      ERROR_REORG_GENRE = 323,
      ERROR_REORG_PATH = 325,
      ERROR_REORG_ALBUM = 327,
      ERROR_WRITING_CHANGES = 329,
      ERROR_COMPRESSING = 332
    }

    private string strSQL;
    private ArrayList availableFiles;

    private string _previousDirectory = null;
    private MusicTag _previousMusicTag = null;
    private bool _foundVariousArtist = false;


    public void EmptyCache()
    {
      _albumCache.Clear();
    }

    #region		Database rebuild
    public int MusicDatabaseReorg(ArrayList shares)
    {
      bool updateSinceLastImport = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        updateSinceLastImport = xmlreader.GetValueAsBool("musicfiles", "updateSinceLastImport", false);
      }
      return MusicDatabaseReorg(shares, _treatFolderAsAlbum, updateSinceLastImport);
    }

    public int MusicDatabaseReorg(ArrayList shares, bool treatFolderAsAlbum, bool updateSinceLastImport)
    {
      // Make sure we use the selected settings if the user hasn't saved the configuration
      _treatFolderAsAlbum = treatFolderAsAlbum;

      if (!updateSinceLastImport)
        _lastImport = DateTime.MinValue;

      if (shares == null)
      {
        LoadShares();
      }
      else
      {
        _shares = (ArrayList)shares.Clone();
      }
      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();

      DateTime startTime = DateTime.Now;
      int fileCount = 0;

      try
      {
        Log.Info("Musicdatabasereorg: Beginning music database reorganization...");
        Log.Info("Musicdatabasereorg: Last import done at {0}", _lastImport.ToString());

        BeginTransaction();


        // When starting a complete rescan, we cleanup the foreign keys
        if (_lastImport == DateTime.MinValue)
        {
          MyArgs.progress = 2;
          MyArgs.phase = "Cleaning up Artists, AlbumArtists and Genres";
          OnDatabaseReorgChanged(MyArgs);
          CleanupForeignKeys();
        }

        /// Delete files that don't exist anymore (example: you deleted files from the Windows Explorer)
        MyArgs.progress = 4;
        MyArgs.phase = "Removing non existing songs";
        OnDatabaseReorgChanged(MyArgs);
        DeleteNonExistingSongs();

        /// Add missing files (example: You downloaded some new files)
        MyArgs.progress = 6;
        MyArgs.phase = "Scanning new files";
        OnDatabaseReorgChanged(MyArgs);

        int GetFilesResult = GetFiles(8, 85, ref fileCount);
        Log.Info("Musicdatabasereorg: Add / Update files: {0} files added / updated", GetFilesResult);
      }

      catch (Exception ex)
      {
        Log.Error("music-scan{0} {1} {2}",
                            ex.Message, ex.Source, ex.StackTrace);
      }

      finally
      {
        CommitTransaction();

        MyArgs.progress = 96;
        MyArgs.phase = "Finishing";
        OnDatabaseReorgChanged(MyArgs);


        MyArgs.progress = 98;
        MyArgs.phase = "Compressing the database";
        OnDatabaseReorgChanged(MyArgs);
        Compress();

        MyArgs.progress = 100;
        MyArgs.phase = "Rescan completed";
        OnDatabaseReorgChanged(MyArgs);
        EmptyCache();


        DateTime stopTime = DateTime.Now;
        TimeSpan ts = stopTime - startTime;
        float fSecsPerTrack = ((float)ts.TotalSeconds / (float)fileCount);
        string trackPerSecSummary = "";

        if (fileCount > 0)
          trackPerSecSummary = string.Format(" ({0} seconds per track)", fSecsPerTrack);

        Log.Info("Musicdatabasereorg: Music database reorganization done.  Processed {0} tracks in: {1:d2}:{2:d2}:{3:d2}{4}",
            fileCount, ts.Hours, ts.Minutes, ts.Seconds, trackPerSecSummary);

        // Save the time of the reorg, to be able to skip the files not updated / added the next time
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlreader.SetValue("musicfiles", "lastImport", startTime.ToString("yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture));
        }
      }
      return (int)Errors.ERROR_OK;
    }

    /// <summary>
    /// Compress the database to save space
    /// </summary>
    /// <returns></returns>
    private int Compress()
    {
      try
      {
        DatabaseUtility.CompactDatabase(MusicDatabase.Instance.DbConnection);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: vacuum failed");
        return (int)Errors.ERROR_COMPRESSING;
      }
      Log.Info("Musicdatabasereorg: Compress completed");
      return (int)Errors.ERROR_OK;
    }

    /// <summary>
    /// Load the shares as set in the Configuration
    /// </summary>
    /// <returns></returns>
    private int LoadShares()
    {
      string currentFolder = String.Empty;
      bool fileMenuEnabled = false;
      string fileMenuPinCode = String.Empty;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);

        string strDefault = xmlreader.GetValueAsString("music", "default", String.Empty);
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}", i);
          string strSharePath = String.Format("sharepath{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);

          string SharePath = xmlreader.GetValueAsString("music", strSharePath, String.Empty);

          if (SharePath.Length > 0)
            _shares.Add(SharePath);
        }
      }
      return 0;
    }

    #region Delete Song
    /// <summary>
    /// Remove songs, which are not existing anymore, because they have been moved, deleted.
    /// </summary>
    /// <returns></returns>
    private int DeleteNonExistingSongs()
    {
      SQLiteResultSet results;
      strSQL = String.Format("select idTrack, strPath from tracks");
      try
      {
        results = MusicDatabase.DirectExecute(strSQL);
        if (results == null)
          return (int)Errors.ERROR_REORG_SONGS;
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: Unable to retrieve songs from database in DeleteNonExistingSongs()");
        return (int)Errors.ERROR_REORG_SONGS;
      }
      int removed = 0;
      Log.Info("Musicdatabasereorg: starting song cleanup for {0} songs", (int)results.Rows.Count);
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        string strFileName = DatabaseUtility.Get(results, i, "tracks.strPath");

        if (!System.IO.File.Exists(strFileName))
        {
          /// song doesn't exist anymore, delete it
          /// We don't care about foreign keys at this moment. We'll just change this later.
          removed++;
          DeleteSong(strFileName, false);

        }
        if ((i % 10) == 0)
        {
          DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
          MyArgs.progress = 4;
          MyArgs.phase = String.Format("Removing non existing songs:{0}/{1} checked, {2} removed", i, results.Rows.Count, removed);
          OnDatabaseReorgChanged(MyArgs);
        }
      }//for (int i=0; i < results.Rows.Count;++i)
      Log.Info("Musicdatabasereorg: DeleteNonExistingSongs completed. Removed {0} non-existing songs", removed);
      return (int)Errors.ERROR_OK;
    }

    /// <summary>
    /// Delete a song from the database
    /// </summary>
    /// <param name="strFileName"></param>
    /// <param name="bCheck"></param>
    public void DeleteSong(string strFileName, bool bCheck)
    {
      try
      {
        if (null == MusicDbClient)
          return;

        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string strSQL;

        strSQL = String.Format("select idTrack, strArtist, strGenre from tracks where strPath = '{0}'", strFileName);

        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          int idTrack = DatabaseUtility.GetAsInt(results, 0, "tracks.idTrack");
          string strArtist = DatabaseUtility.Get(results, 0, "tracks.strArtist");
          string strGenre = DatabaseUtility.Get(results, 0, "tracks.strGenre");

          // Delete
          strSQL = String.Format("delete from tracks where idTrack={0}", idTrack);
          MusicDbClient.Execute(strSQL);

          // Check if we have now Artists and Genres for which no song exists
          if (bCheck)
          {
            // split up the artist, in case we've got multiple artists
            string[] artists = strArtist.Split('|');
            foreach (string artist in artists)
            {
              strSQL = String.Format("select idTrack from tracks where strArtist like '%{0}%'", artist.Trim());
              results = MusicDbClient.Execute(strSQL);
              if (results.Rows.Count == 0)
              {
                // Delete artist with no songs
                strSQL = String.Format("delete from artist where strArtist = '{0}'", artist.Trim());
                MusicDbClient.Execute(strSQL);

                // Delete artist info
                strSQL = String.Format("delete from artistinfo where strArtist = '{0}'", artist.Trim());
                MusicDbClient.Execute(strSQL);
              }
            }

            // split up the genre, in case we've got multiple genres
            string[] genres = strGenre.Split('|');
            foreach (string genre in genres)
            {
              strSQL = String.Format("select idTrack from tracks where strGenre like '%{0}%'", genre.Trim());
              results = MusicDbClient.Execute(strSQL);
              if (results.Rows.Count == 0)
              {
                // Delete genres with no songs
                strSQL = String.Format("delete from genre where strGenre = '{0}'", genre.Trim());
                MusicDbClient.Execute(strSQL);
              }
            }
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabase Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }
    #endregion

    #region Add / Update Song
    /// <summary>
    /// Scan the Music Shares and add all new songs found to the database.
    /// Update tags for Songs, which have been updated
    /// </summary>
    /// <param name="StartProgress"></param>
    /// <param name="EndProgress"></param>
    /// <param name="fileCount"></param>
    /// <returns></returns>
    private int GetFiles(int StartProgress, int EndProgress, ref int fileCount)
    {
      availableFiles = new ArrayList();

      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      string strSQL;

      int totalFiles = 0;

      int ProgressRange = EndProgress - StartProgress;
      int SongCounter = 0;
      int AddedCounter = 0;
      int TotalSongs = 0;
      double NewProgress;

      foreach (string Share in _shares)
      {
        // Get all the files for the given Share / Path
        CountFilesInPath(Share, ref totalFiles);

        // Now get the files from the root directory, which we missed in the above search
        try
        {
          foreach (string file in Directory.GetFiles(Share, "*.*"))
          {
            CheckFileForInclusion(file, ref totalFiles);
          }
        }
        catch (Exception)
        {
          // ignore exception that we get on CD / DVD shares
        }
      }
      TotalSongs = totalFiles;
      Log.Info("Musicdatabasereorg: Found {0} files.", (int)totalFiles);
      Log.Info("Musicdatabasereorg: Now check for new / updated files.");

      SQLiteResultSet results;
      foreach (string MusicFile in availableFiles)
      {
        SongCounter++;

        string strFileName = MusicFile;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        strSQL = String.Format("select idTrack from tracks where strPath='{0}'", strFileName);

        try
        {
          results = MusicDbClient.Execute(strSQL);
          if (results == null)
          {
            Log.Info("Musicdatabasereorg: AddMissingFiles finished with error (results == null)");
            return (int)Errors.ERROR_REORG_SONGS;
          }
        }

        catch (Exception)
        {
          Log.Error("Musicdatabasereorg: AddMissingFiles finished with error (exception for select)");
          return (int)Errors.ERROR_REORG_SONGS;
        }

        if (results.Rows.Count == 0)
        {
          //The song does not exist, we will add it.
          AddSong(MusicFile);
        }
        else
        {
          UpdateSong(MusicFile);
        }
        AddedCounter++;


        if ((SongCounter % 10) == 0)
        {
          NewProgress = StartProgress + ((ProgressRange * SongCounter) / TotalSongs);
          MyArgs.progress = Convert.ToInt32(NewProgress);
          MyArgs.phase = String.Format("Adding new files {0}/{1}", SongCounter, availableFiles.Count);
          OnDatabaseReorgChanged(MyArgs);
        }
      } //end for-each
      Log.Info("Musicdatabasereorg: Checked {0} files.", totalFiles);
      Log.Info("Musicdatabasereorg: {0} skipped because of creation before the last import", totalFiles - AddedCounter);

      fileCount = TotalSongs;
      return SongCounter;
    }

    /// <summary>
    /// Retrieve all the in a given path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="totalFiles"></param>
    private void CountFilesInPath(string path, ref int totalFiles)
    {
      try
      {
        foreach (string dir in Directory.GetDirectories(path))
        {
          foreach (string file in Directory.GetFiles(dir, "*.*"))
          {
            CheckFileForInclusion(file, ref totalFiles);
            if ((totalFiles % 10) == 0)
            {
              DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
              MyArgs.progress = 4;
              MyArgs.phase = String.Format("Counting files in Shares: {0} files found", totalFiles);
              OnDatabaseReorgChanged(MyArgs);
            }
          }
          CountFilesInPath(dir, ref totalFiles);
        }
      }
      catch
      {
        // Ignore
      }
    }

    /// <summary>
    /// Should the file be included in the list to be added
    /// </summary>
    /// <param name="file"></param>
    /// <param name="totalFiles"></param>
    private void CheckFileForInclusion(string file, ref int totalFiles)
    {
      try
      {

        string ext = System.IO.Path.GetExtension(file).ToLower();
        if (ext == ".m3u")
          return;
        if (ext == ".pls")
          return;
        if (ext == ".wpl")
          return;
        if (ext == ".b4s")
          return;
        if ((File.GetAttributes(file) & FileAttributes.Hidden) == FileAttributes.Hidden)
          return;

        // Only get files with the required extension
        if (_supportedExtensions.IndexOf(ext) == -1)
          return;

        // Only Add files to the list, if they have been Created / Updated after the Last Import date
        if (System.IO.File.GetCreationTime(file) > _lastImport || System.IO.File.GetLastWriteTime(file) > _lastImport)
          availableFiles.Add(file);
      }
      catch (Exception)
      {
        // File.GetAttributes may fail if (file) is 0 bytes long
      }
      totalFiles++;
    }

    /// <summary>
    /// Retrieves the Tags from a file and formats them suiteable for insertion into the databse
    /// </summary>
    /// <param name="strFileName"></param>
    /// <returns></returns>
    public MusicTag GetTag(string strFileName)
    {
      MusicTag tag = TagReader.ReadTag(strFileName);
      if (tag != null)
      {
        string strTmp;
        strTmp = tag.Album;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Album = strTmp;
        strTmp = tag.Genre;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Genre = strTmp;
        strTmp = tag.Artist;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Artist = strTmp;
        strTmp = tag.Title;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Title = strTmp;
        strTmp = tag.AlbumArtist;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.AlbumArtist = strTmp;
        strTmp = tag.Lyrics;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Lyrics = strTmp;

        // When we got Multiple Entries of either Artist, Genre, Albumartist in WMP notation, separated by ";",
        // we will store them separeted by "|"
        tag.Artist = tag.Artist.Replace(';', '|');
        tag.AlbumArtist = tag.AlbumArtist.Replace(';', '|');
        tag.Genre = tag.Genre.Replace(';', '|');

        if (tag.AlbumArtist == "unknown" || tag.AlbumArtist == String.Empty)
          tag.AlbumArtist = tag.Artist;

        // Extract the Coverart
        ExtractCoverArt(tag);

        return tag;
      }
      return null;
    }

    /// <summary>
    /// Adds a Song to the Database
    /// </summary>
    /// <param name="strFileName"></param>
    /// <returns></returns>
    public int AddSong(string strFileName)
    {
      SQLiteResultSet results;

      // Get the Tags from the file
      MusicTag tag = GetTag(strFileName);
      if (tag != null)
      {
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string sortableArtist = tag.Artist;
        StripArtistNamePrefix(ref sortableArtist, true);

        strSQL = String.Format("insert into tracks ( " +
                               "strPath, strArtist, strArtistSortName, strAlbumArtist, strAlbum, strGenre, " +
                               "strTitle, iTrack, iNumTracks, iDuration, iYear, iTimesPlayed, iRating, iFavorite, " +
                               "iResumeAt, iDisc, iNumDisc, iGainTrack, iPeakTrack, strLyrics, musicBrainzID, dateLastPlayed) " +
                               "values ( " +
                               "'{0}', '{1}', '{2}', '{3}', '{4}', '{5}', " +
                               "'{6}', {7}, {8}, {9}, {10}, {11}, {12}, {13}, " +
                               "{14}, {15}, {16}, {17}, {18}, '{19}', '{20}', '{21}' )",
                               strFileName, tag.Artist, sortableArtist, tag.AlbumArtist, tag.Album, tag.Genre,
                               tag.Title, tag.Track, tag.TrackTotal, tag.Duration, tag.Year, 0, 0, 0,
                               0, tag.DiscID, tag.DiscTotal, 0, 0, tag.Lyrics, "", DateTime.MinValue
        );
        try
        {
          results = MusicDbClient.Execute(strSQL);
          if (results == null)
          {
            Log.Info("Insert of song {0} failed", strFileName);
            return (int)Errors.ERROR_REORG_SONGS;
          }

          // Now add the Artist, AlbumArtist and Genre to the Artist / Genre Tables
          AddArtist(tag.Artist);
          AddAlbumArtist(tag.AlbumArtist);
          AddGenre(tag.Genre);

          if (_treatFolderAsAlbum)
            UpdateVariousArtist(tag);
        }
        catch (Exception)
        {
          Log.Error("Insert of song {0} failed", strFileName);
          return (int)Errors.ERROR_REORG_SONGS;
        }
        return (int)Errors.ERROR_OK;
      }
      return (int)Errors.ERROR_OK;
    }

    /// <summary>
    /// Add the artist to the Artist table, to allow us having mutiple artists per song
    /// </summary>
    /// <param name="strArtist"></param>
    /// <returns></returns>
    private void AddArtist(string strArtist)
    {
      try
      {
        if (null == MusicDbClient)
          return;

        string strSQL;

        // split up the artist, in case we've got multiple artists
        string[] artists = strArtist.Split(new char[] { ';', '|' });
        foreach (string artist in artists)
        {
          strSQL = String.Format("select idArtist from artist where strArtist = '{0}'", strArtist.Trim());
          if (MusicDatabase.DirectExecute(strSQL).Rows.Count < 1)
          {
            // Insert the Artist
            strSQL = String.Format("insert into artist (strArtist) values ('{0}')", artist.Trim());
            MusicDbClient.Execute(strSQL);
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabase Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    /// <summary>
    /// Add the albumartist to the AlbumArtist table, to allow us having mutiple artists per song
    /// </summary>
    /// <param name="strArtist"></param>
    /// <returns></returns>
    private void AddAlbumArtist(string strAlbumArtist)
    {
      try
      {
        if (null == MusicDbClient)
          return;

        string strSQL;

        // split up the albumartist, in case we've got multiple albumartists
        string[] artists = strAlbumArtist.Split(new char[] { ';', '|' });
        foreach (string artist in artists)
        {
          strSQL = String.Format("select idAlbumArtist from albumartist where strAlbumArtist = '{0}'", artist.Trim());
          if (MusicDatabase.DirectExecute(strSQL).Rows.Count < 1)
          {
            // Insert the AlbumArtist
            strSQL = String.Format("insert into albumartist (strAlbumArtist) values ('{0}')", artist.Trim());
            MusicDbClient.Execute(strSQL);
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabase Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    /// <summary>
    /// Add the genre to the Genre Table, to allow maultiple Genres per song
    /// </summary>
    /// <param name="strGenre"></param>
    private void AddGenre(string strGenre)
    {
      try
      {
        string strSQL;

        // split up the artist, in case we've got multiple artists
        string[] genres = strGenre.Split(new char[] { ';', '|' });
        foreach (string genre in genres)
        {
          strSQL = String.Format("select idGenre from genre where upper(strGenre) = '{0}'", genre.Trim().ToUpperInvariant());
          if (MusicDatabase.DirectExecute(strSQL).Rows.Count < 1)
          {
            // Insert the Genre
            strSQL = String.Format("insert into genre (strGenre) values ('{0}')", genre.Trim());
            MusicDatabase.DirectExecute(strSQL);
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabase Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    /// <summary>
    /// Update an existing song with the Tags from the file
    /// </summary>
    /// <param name="strFileName"></param>
    /// <returns></returns>
    public bool UpdateSong(string strFileName)
    {
      try
      {
        MusicTag tag = GetTag(strFileName);
        if (tag != null)
        {
          DatabaseUtility.RemoveInvalidChars(ref strFileName);

          strSQL = String.Format("update tracks " +
                                 "set strArtist = '{0}', strAlbumArtist = '{1}', strAlbum = '{2}', " +
                                 "strGenre = '{3}', strTitle = '{4}', iTrack = {5}, iNumTracks = {6}, " +
                                 "iDuration = {7}, iYear = {8}, iRating = {9}, iDisc = {10}, iNumDisc = {11}, " +
                                 "strLyrics = '{12}' " +
                                 "where strPath = '{13}'",
                                 tag.Artist, tag.AlbumArtist, tag.Album,
                                 tag.Genre, tag.Title, tag.Track, tag.TrackTotal,
                                 tag.Duration, tag.Year, tag.Rating, tag.DiscID, tag.DiscTotal,
                                 tag.Lyrics,
                                 strFileName
                                 );
          try
          {
            MusicDbClient.Execute(strSQL);

            // Now add the Artist, AlbumArtist and Genre to the Artist / Genre Tables
            AddArtist(tag.Artist);
            AddAlbumArtist(tag.AlbumArtist);
            AddGenre(tag.Genre);

            if (_treatFolderAsAlbum)
              UpdateVariousArtist(tag);
          }
          catch (Exception)
          {
            Log.Error("Musicdatabasereorg: Update tags for {0} failed because of DB exception", strFileName);
            return false;
          }
        }
        else
        {
          Log.Info("Musicdatabasereorg: cannot get tag for {0}", strFileName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabasereorg: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      return true;
    }

    private void ExtractCoverArt(MusicTag tag)
    {
      char[] trimChars = { ' ', '\x00' };
      String tagAlbumName = String.Format("{0}-{1}", tag.Artist.Trim(trimChars), tag.Album.Trim(trimChars));
      string strSmallThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicAlbum, Util.Utils.MakeFileName(tagAlbumName));
      string strLargeThumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MusicAlbum, Util.Utils.MakeFileName(tagAlbumName));

      if (tag.CoverArtImageBytes != null)
      {
        if (_extractEmbededCoverArt)
        {
          try
          {
            bool extractFile = false;
            if (!System.IO.File.Exists(strSmallThumb))
              extractFile = true;
            else
            {
              // Prevent creation of the thumbnail multiple times, when all songs of an album contain coverart
              DateTime fileDate = System.IO.File.GetLastWriteTime(strSmallThumb);
              TimeSpan span = _currentDate - fileDate;
              if (span.Hours > 0)
                extractFile = true;
            }

            if (extractFile)
            {
              if (!Util.Picture.CreateThumbnail(tag.CoverArtImage, strSmallThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0))
                Log.Debug("Could not extract thumbnail from {0}", tag.FileName);
              if (!Util.Picture.CreateThumbnail(tag.CoverArtImage, strLargeThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0))
                Log.Debug("Could not extract thumbnail from {0}", tag.FileName);
            }
          }
          catch (Exception) { }
        }
      }
      // no mp3 coverart - use folder art if present to get an album thumb
      if (_useFolderThumbs)
      {
        // only create for the first file
        if (!System.IO.File.Exists(strSmallThumb))
        {
          string sharefolderThumb = MediaPortal.Util.Utils.GetFolderThumb(tag.FileName);
          if (System.IO.File.Exists(sharefolderThumb))
          {
            if (!MediaPortal.Util.Picture.CreateThumbnail(sharefolderThumb, strSmallThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0))
              Log.Debug("Could not create album thumb from folder {0}", tag.FileName);
            if (!MediaPortal.Util.Picture.CreateThumbnail(sharefolderThumb, strLargeThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0))
              Log.Debug("Could not create large album thumb from folder {0}", tag.FileName);
          }
        }
      }

      // create the local folder thumb cache / and folder.jpg itself if not present
      if (_useFolderThumbs || _createMissingFolderThumbs)
        CreateFolderThumbs(tag.FileName, strSmallThumb);

      if (_useFolderArtForArtistGenre)
      {
        CreateArtistThumbs(strSmallThumb, tag.Artist);
        CreateGenreThumbs(tag.FileName, tag.Genre);
      }
    }

    private void CreateFolderThumbs(string strSongPath, string strSmallThumb)
    {
      if (System.IO.File.Exists(strSmallThumb) && strSongPath != String.Empty)
      {
        string folderThumb = MediaPortal.Util.Utils.GetFolderThumb(strSongPath);
        string localFolderThumb = MediaPortal.Util.Utils.GetLocalFolderThumb(strSongPath);
        string localFolderLThumb = localFolderThumb;
        localFolderLThumb = Util.Utils.ConvertToLargeCoverArt(localFolderLThumb);

        try
        {
          // we've embedded art but no folder.jpg --> copy the large one for cache and create a small cache thumb
          if (!System.IO.File.Exists(folderThumb))
          {
            if (_createMissingFolderThumbs)
            {
              MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, folderThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);

              if (_useFolderThumbs)
              {
                if (!System.IO.File.Exists(localFolderThumb))
                  MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, localFolderThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
                if (!System.IO.File.Exists(localFolderLThumb))
                  System.IO.File.Copy(folderThumb, localFolderLThumb, true);
              }
            }
          }
          else
          {
            if (_useFolderThumbs)
            {
              if (!System.IO.File.Exists(localFolderThumb))
                MediaPortal.Util.Picture.CreateThumbnail(folderThumb, localFolderThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
              if (!System.IO.File.Exists(localFolderLThumb))
              {
                // just copy the folder.jpg if it is reasonable in size - otherwise re-create it
                System.IO.FileInfo fiRemoteFolderArt = new System.IO.FileInfo(folderThumb);
                if (fiRemoteFolderArt.Length < 32000)
                  System.IO.File.Copy(folderThumb, localFolderLThumb, true);
                else
                  MediaPortal.Util.Picture.CreateThumbnail(folderThumb, localFolderLThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
              }
            }
          }
        }
        catch (Exception ex1)
        {
          Log.Warn("Database: could not create folder thumb for {0} - {1}", strSongPath, ex1.Message);
        }
      }
    }

    private void CreateArtistThumbs(string strSmallThumb, string songArtist)
    {
      if (System.IO.File.Exists(strSmallThumb) && songArtist != String.Empty)
      {
        string artistThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicArtists, Util.Utils.MakeFileName(songArtist));
        if (!System.IO.File.Exists(artistThumb))
        {
          try
          {
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, artistThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, Util.Utils.ConvertToLargeCoverArt(artistThumb), (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
          }
          catch (Exception) { }
        }
      }
    }

    private void CreateGenreThumbs(string strSmallThumb, string songGenre)
    {
      if (System.IO.File.Exists(strSmallThumb) && songGenre != String.Empty)
      {
        // using the thumb of the first item of a gerne / artist having a thumb

        // The genre may contains unallowed chars
        string strGenre = MediaPortal.Util.Utils.MakeFileName(songGenre);

        // Sometimes the genre contains a number code in brackets -> remove that
        // (code borrowed from addGenre() method)
        if (String.Compare(strGenre.Substring(0, 1), "(") == 0)
        {
          bool FixedTheCode = false;
          for (int i = 1; (i < 10 && i < strGenre.Length & !FixedTheCode); ++i)
          {
            if (String.Compare(strGenre.Substring(i, 1), ")") == 0)
            {
              strGenre = strGenre.Substring(i + 1, (strGenre.Length - i - 1));
              FixedTheCode = true;
            }
          }
        }
        // Now the genre is clean and sober -> build a filename out of it
        string genreThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicGenre, Util.Utils.MakeFileName(strGenre));

        if (!System.IO.File.Exists(genreThumb))
        {
          // thumb for this genre does not exist yet -> simply use the folderThumb from above
          // and copy it to thumbs\music\gerne\<genre>.jpg 
          try
          {
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, genreThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, Util.Utils.ConvertToLargeCoverArt(genreThumb), (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
          }
          catch (Exception) { }
        }
      }
    }

    /// <summary>
    /// Move the Prefix of an artist to the end of the string for better sorting
    /// i.e. "The Rolling Stones" -> "Rolling Stones, The" 
    /// </summary>
    /// <param name="artistName"></param>
    /// <param name="appendPrefix"></param>
    /// <returns></returns>
    private bool StripArtistNamePrefix(ref string artistName, bool appendPrefix)
    {
      string temp = artistName.ToLower();

      foreach (string s in ArtistNamePrefixes)
      {
        if (s.Length == 0)
          continue;

        string prefix = s;
        prefix = prefix.Trim().ToLower();
        int pos = temp.IndexOf(prefix + " ");
        if (pos == 0)
        {
          string tempName = artistName.Substring(prefix.Length).Trim();

          if (appendPrefix)
            artistName = string.Format("{0}, {1}", tempName, artistName.Substring(0, prefix.Length));

          else
            artistName = temp;

          return true;
        }
      }

      return false;
    }

    private void UpdateVariousArtist(MusicTag tag)
    {
      string currentDir = Path.GetDirectoryName(tag.FileName);
      // on first cal, set the directory name
      if (_previousDirectory == null)
        _previousDirectory = currentDir;

      if (_previousMusicTag == null)
        _previousMusicTag = tag;

      if (_previousDirectory.ToLowerInvariant() == currentDir.ToLowerInvariant())
      {
        // already have detected Various artists in this folder. no further checking needed
        if (_foundVariousArtist)
          return;

        // Is the Artist different and also no different AlbumArtist set?
        if (_previousMusicTag.Artist != tag.Artist && tag.AlbumArtist == tag.Artist)
        {
          _foundVariousArtist = true;
          return;
        }
      }
      else
      {
        if (_foundVariousArtist)
        {
          List<SongMap> songs = new List<SongMap>();
          GetSongsByPath(_previousDirectory, ref songs);

          foreach (SongMap map in songs)
          {
            int id = map.m_song.Id;
            strSQL = string.Format("update tracks set strAlbumArtist = 'Various Artists' where idTrack={0}", id);
            MusicDbClient.Execute(strSQL);
          }
        }

        _previousMusicTag = tag;
        _previousDirectory = currentDir;
        _foundVariousArtist = false;

      }
    }
    #endregion

    #region Clean Up Foreign Keys

    /// <summary>
    /// On a complete rescan of the shares, we will delete all the entries in the Artist, AlbumArtist and Genre table,
    /// to get rid of "dead" entries for which no song exists
    /// </summary>
    /// <returns></returns>
    private int CleanupForeignKeys()
    {
      try
      {
        string strSql = "delete from artist";
        MusicDbClient.Execute(strSql);
        strSql = "delete from albumartist";
        MusicDbClient.Execute(strSql);
        strSql = "delete from genre";
        MusicDbClient.Execute(strSql);
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabasereorg: CleanupForeignKeys failed");
        return (int)Errors.ERROR_REORG_ARTIST;
      }

      Log.Info("Musicdatabasereorg: CleanupForeignKeys completed");
      return (int)Errors.ERROR_OK;
    }
    #endregion

    #region MusicShareWatcher
    /// <summary>
    /// Does a song exist in the database
    /// </summary>
    /// <param name="strFileName"></param>
    /// <returns></returns>
    public bool SongExists(string strFileName)
    {
      DatabaseUtility.RemoveInvalidChars(ref strFileName);

      string strSQL;
      strSQL = String.Format("select idTrack from tracks where strFileName like '{0}'",
                     strFileName);

      SQLiteResultSet results;
      results = MusicDbClient.Execute(strSQL);
      if (results.Rows.Count > 0)
        // Found
        return true;
      else
        // Not Found
        return false;
    }

    /// <summary>
    /// Musicsharewatcher detected that a File or Directory has been renamed
    /// </summary>
    /// <param name="strOldFileName"></param>
    /// <param name="strNewFileName"></param>
    /// <returns></returns>
    public bool RenameSong(string strOldFileName, string strNewFileName)
    {
      try
      {
        // The rename may have been on a directory or a file
        // In case of a directory rename, the Path needs to be corrected
        FileInfo fi = new FileInfo(strNewFileName);
        if (fi.Exists)
        {
          DatabaseUtility.RemoveInvalidChars(ref strOldFileName);
          DatabaseUtility.RemoveInvalidChars(ref strNewFileName);

          string strSQL;
          strSQL = String.Format("update tracks set strPath = '{0}' where strPath like '{1}'",
                       strNewFileName,
                       strOldFileName);

          SQLiteResultSet results;
          results = MusicDbClient.Execute(strSQL);
          return true;
        }
        else
        {
          // See if it is a directory
          DirectoryInfo di = new DirectoryInfo(strNewFileName);
          if (di.Exists)
          {
            // Must be a directory, so let's change the path entries, containing the old
            // name with the new name
            DatabaseUtility.RemoveInvalidChars(ref strOldFileName);
            DatabaseUtility.RemoveInvalidChars(ref strNewFileName);

            SQLiteResultSet results;
            string strPath = "";
            string strSQL;

            strSQL = String.Format("select idTrack, strPath from tracks where strPath like '{0}%'",
                    strOldFileName);

            results = MusicDbClient.Execute(strSQL);
            if (results.Rows.Count > 0)
            {
              try
              {
                BeginTransaction();
                // We might have changed a Top directory, so we get a lot of path entries returned
                for (int rownum = 0; rownum < results.Rows.Count; rownum++)
                {
                  int idTrack = DatabaseUtility.GetAsInt(results, rownum, "tracks.idTrack");
                  string strTmpPath = DatabaseUtility.Get(results, rownum, "tracks.strPath");
                  strPath = strTmpPath.Replace(strOldFileName, strNewFileName);
                  // Need to keep an unmodified path for the later CRC calculation
                  strTmpPath = strPath;
                  DatabaseUtility.RemoveInvalidChars(ref strTmpPath);
                  strSQL = String.Format("update tracks set strPath='{0}' where idTrack={1}",
                          strTmpPath,
                          idTrack);

                  MusicDbClient.Execute(strSQL);
                  EmptyCache();
                }
                CommitTransaction();
                return true;
              }
              catch (Exception)
              {
                RollbackTransaction();
                Log.Warn("RenameSong: Rename for {0} failed because of DB exception", strPath);
                return false;
              }
            }
            return true;
          }
          else
          {
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    /// <summary>
    /// A complete Path has been deleted. Now we need to remove all Songs for that path from the dB.
    /// </summary>
    /// <param name="strPath"></param>
    /// <returns></returns>
    public bool DeleteSongDirectory(string strPath)
    {
      DatabaseUtility.RemoveInvalidChars(ref strPath);

      string strSQL;
      strSQL = String.Format("delete from tracks where strPath like '{0}%'",
              strPath);

      try
      {
        MusicDbClient.Execute(strSQL);
      }
      catch (Exception)
      {
        Log.Error("Delete Directory for {0} failed because of DB exception", strPath);
        return false;
      }
      return true;
    }
    #endregion

    #region Scrobble
    public int AddScrobbleUser(string userName_)
    {
      string strSQL;
      try
      {
        if (userName_ == null)
          return -1;
        if (userName_.Length == 0)
          return -1;
        string strUserName = userName_;

        DatabaseUtility.RemoveInvalidChars(ref strUserName);
        if (null == MusicDbClient)
          return -1;

        SQLiteResultSet results;
        strSQL = String.Format("select * from scrobbleusers where strUsername like '{0}'", strUserName);
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into scrobbleusers (idScrobbleUser , strUsername) values ( NULL, '{0}' )", strUserName);
          MusicDbClient.Execute(strSQL);
          Log.Info("MusicDatabase: added scrobbleuser {0} with ID {1}", strUserName, Convert.ToString(MusicDbClient.LastInsertID()));
          return MusicDbClient.LastInsertID();
        }
        else
          return DatabaseUtility.GetAsInt(results, 0, "idScrobbleUser");
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public string AddScrobbleUserPassword(string userID_, string userPassword_)
    {
      string strSQL;
      try
      {
        if (userPassword_ == null || userID_ == null)
          return string.Empty;
        if (userID_.Length == 0)
          return string.Empty;
        string strUserPassword = userPassword_;

        DatabaseUtility.RemoveInvalidChars(ref strUserPassword);
        if (null == MusicDbClient)
          return string.Empty;

        SQLiteResultSet results;
        strSQL = String.Format("select * from scrobbleusers where idScrobbleUser = '{0}'", userID_);
        results = MusicDbClient.Execute(strSQL);
        // user doesn't exist therefore no password to change
        if (results.Rows.Count == 0)
          return string.Empty;

        if (DatabaseUtility.Get(results, 0, "strPassword") == strUserPassword)
          // password didn't change
          return userPassword_;
        // set new password
        else
        {
          // if no password was given = fetch it
          if (userPassword_ == "")
            return DatabaseUtility.Get(results, 0, "strPassword");
          else
          {
            strSQL = String.Format("update scrobbleusers set strPassword='{0}' where idScrobbleUser like '{1}'", strUserPassword, userID_);
            MusicDbClient.Execute(strSQL);
            return userPassword_;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return string.Empty;
    }

    public int AddScrobbleUserSettings(string userID_, string fieldName_, int fieldValue_)
    {
      string strSQL;
      string currentSettingID;
      try
      {
        if (fieldName_ == null || userID_ == null || userID_ == "-1")
          return -1;
        if (userID_.Length == 0 || fieldName_.Length == 0)
          return -1;

        SQLiteResultSet results;

        strSQL = String.Format("select idScrobbleSettings, idScrobbleUser from scrobblesettings where idScrobbleUser = '{0}'", userID_);
        results = MusicDbClient.Execute(strSQL);
        currentSettingID = DatabaseUtility.Get(results, 0, "idScrobbleSettings");
        //Log.Info("MusicDatabase: updating settings with ID {0}", currentSettingID);

        // setting doesn't exist - add it
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("insert into scrobblesettings (idScrobbleSettings, idScrobbleUser, " + fieldName_ + ") values ( NULL, '{0}', '{1}')", userID_, fieldValue_);
          MusicDbClient.Execute(strSQL);
          Log.Info("MusicDatabase: added scrobblesetting {0} for userid {1}", Convert.ToString(MusicDbClient.LastInsertID()), userID_);
          if (fieldValue_ > -1)
            return MusicDbClient.LastInsertID();
          else
            return fieldValue_;
        }
        else
        {
          strSQL = String.Format("select " + fieldName_ + " from scrobblesettings where idScrobbleSettings = '{0}'", currentSettingID);
          results = MusicDbClient.Execute(strSQL);

          if (DatabaseUtility.GetAsInt(results, 0, fieldName_) == fieldValue_)
            // setting didn't change
            return fieldValue_;
          // set new value
          else
          {
            // if no value was given = fetch it
            if (fieldValue_ == -1)
              return DatabaseUtility.GetAsInt(results, 0, fieldName_);
            else
            {
              strSQL = String.Format("update scrobblesettings set " + fieldName_ + "='{0}' where idScrobbleSettings like '{1}'", fieldValue_, currentSettingID);
              MusicDbClient.Execute(strSQL);
              return fieldValue_;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public List<string> GetAllScrobbleUsers()
    {
      SQLiteResultSet results;
      List<string> scrobbleUsers = new List<string>();

      strSQL = "select * from scrobbleusers";
      results = MusicDbClient.Execute(strSQL);

      if (results.Rows.Count != 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
          scrobbleUsers.Add(DatabaseUtility.Get(results, i, "strUsername"));
      }
      // what else?

      return scrobbleUsers;
    }

    public bool DeleteScrobbleUser(string userName_)
    {
      string strSQL;
      int strUserID;
      try
      {
        if (userName_ == null)
          return false;
        if (userName_.Length == 0)
          return false;
        string strUserName = userName_;

        DatabaseUtility.RemoveInvalidChars(ref strUserName);
        if (null == MusicDbClient)
          return false;

        strUserID = AddScrobbleUser(strUserName);

        SQLiteResultSet results;
        strSQL = String.Format("delete from scrobblesettings where idScrobbleUser = '{0}'", strUserID);
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 1)
        {
          // setting removed now remove user
          strSQL = String.Format("delete from scrobbleusers where idScrobbleUser = '{0}'", strUserID);
          MusicDbClient.Execute(strSQL);
          return true;
        }
        else
        {
          Log.Error("MusicDatabase: could not delete settings for scrobbleuser {0} with ID {1}", strUserName, strUserID);
          return false;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }
    #endregion
    #endregion
  }
}
