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
using System.Threading;
using MediaPortal.Database;
using MediaPortal.Profile;
using MediaPortal.ServiceImplementations;
using MediaPortal.TagReader;
using MediaPortal.Util;
using SQLite.NET;

namespace MediaPortal.Music.Database
{

  #region Usings

  #endregion

  #region Reorg class

  public delegate void MusicDBReorgEventHandler(object sender, DatabaseReorgEventArgs e);

  public class DatabaseReorgEventArgs : EventArgs
  {
    public int progress;
    // Provide one or more constructors, as well as fields and
    // accessors for the arguments.
    public string phase;
  }

  #endregion

  public partial class MusicDatabase
  {
    #region Delegates

    public delegate void MusicRatingChangedHandler(object sender, string filePath, int rating);

    public static event MusicRatingChangedHandler MusicRatingChanged;

    #endregion

    #region Reorg events

    // An event that clients can use to be notified whenever the
    // elements of the list change.
    public static event MusicDBReorgEventHandler DatabaseReorgChanged;

    // Invoke the Changed event; called whenever list changes
    protected virtual void OnDatabaseReorgChanged(DatabaseReorgEventArgs e)
    {
      if (DatabaseReorgChanged != null)
      {
        DatabaseReorgChanged(this, e);
      }
    }

    #endregion

    #region Enums

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

    #endregion

    #region Variables

    private ArrayList _shares = new ArrayList();
    private string strSQL;
    private List<string> musicFolders;
    private List<string> cueFiles;
    private Hashtable allFiles;
    private int _processCount = 0;
    private int _songsSkipped = 0;
    private int _songsProcessed = 0;
    private int _songsAdded = 0;
    private int _songsUpdated = 0;
    private bool _updateSinceLastImport = false;
    private bool _excludeHiddenFiles = false;
    private bool _singleFolderScan = false;

    private Thread _scanThread = null;
    private AutoResetEvent _resetEvent = null;
    //private bool _abortScan = false;

    private string _previousDirectory = null;
    private string _previousNegHitDir = null;
    private string _previousPosHitDir = null;
    private MusicTag _previousMusicTag = null;
    private bool _foundVariousArtist = false;

    private readonly char[] trimChars = {' ', '\x00', '|'};

    private readonly string[] _multipleValueFields = new string[] {"artist", "albumartist", "genre", "composer"};

    #endregion

    #region Favorite / Ratings

    public void SetFavorite(Song aSong)
    {
      try
      {
        if (aSong.Id == -1)
        {
          return;
        }

        int iFavorite = 0;
        if (aSong.Favorite)
        {
          iFavorite = 1;
        }
        string strSQL = String.Format("UPDATE tracks SET iFavorite={0} WHERE idTrack={1}", iFavorite, aSong.Id);
        DirectExecute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetRating(string aFilename, int aRating)
    {
      if (string.IsNullOrEmpty(aFilename))
      {
        return;
      }

      try
      {
        Song song = new Song();
        string strFileName = aFilename;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        strSQL = String.Format("UPDATE tracks SET iRating={0} WHERE strPath='{1}'", aRating, strFileName);

        DirectExecute(strSQL);

        // Let's fire the Ratings change event
        if (MusicRatingChanged != null)
        {
          MusicRatingChanged(this, strFileName, aRating);
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    #endregion

    #region Top100

    public void ResetTop100()
    {
      try
      {
        string strSQL = String.Format("UPDATE tracks SET iTimesPlayed=0");
        DirectExecute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool IncrTop100CounterByFileName(string aFileName)
    {
      try
      {
        Song song = new Song();
        string strFileName = aFileName;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        strSQL = String.Format("SELECT * from tracks WHERE strPath = '{0}'", strFileName);

        SQLiteResultSet results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        int idSong = DatabaseUtility.GetAsInt(results, 0, "idTrack");
        int iTimesPlayed = DatabaseUtility.GetAsInt(results, 0, "iTimesPlayed");

        strSQL = String.Format("UPDATE tracks SET iTimesPlayed={0}, dateLastPlayed='{1}' where idTrack='{2}'", 
          ++iTimesPlayed, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), idSong);
        if (DirectExecute(strSQL).Rows.Count > 0)
        {
          Log.Debug("MusicDatabase: increased playcount for song {1} to {0}", Convert.ToString(iTimesPlayed), aFileName);
          return true;
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

    #region Album / Artist Info

    public void AddAlbumInfo(AlbumInfo aAlbumInfo)
    {
      string strSQL;
      try
      {
        AlbumInfo album = aAlbumInfo.Clone();
        string strTmp;

        strTmp = album.Album;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Album = strTmp;

        strTmp = album.Artist;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Artist = strTmp;

        strTmp = album.Tones;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Tones = strTmp == "unknown" ? "" : strTmp;

        strTmp = album.Styles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Styles = strTmp == "unknown" ? "" : strTmp;

        strTmp = album.Review;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Review = strTmp == "unknown" ? "" : strTmp;

        strTmp = album.Image;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Image = strTmp == "unknown" ? "" : strTmp;

        strTmp = album.Tracks;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Tracks = strTmp == "unknown" ? "" : strTmp;

        if (null == MusicDbClient)
        {
          return;
        }

        strSQL = String.Format("delete from albuminfo where strAlbum like '{0}' and strArtist like '{1}%'", album.Album,
                               album.Artist);
        MusicDbClient.Execute(strSQL);

        strSQL =
          String.Format(
            "insert into albuminfo (strAlbum,strArtist, strTones,strStyles,strReview,strImage,iRating,iYear,strTracks) values('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}')",
            album.Album,
            album.Artist,
            album.Tones,
            album.Styles,
            album.Review,
            album.Image,
            album.Rating,
            album.Year,
            album.Tracks);

        MusicDbClient.Execute(strSQL);

        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public void AddArtistInfo(ArtistInfo aArtistInfo)
    {
      string strSQL;
      try
      {
        ArtistInfo artist = aArtistInfo.Clone();
        string strTmp;

        strTmp = artist.Artist;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Artist = strTmp;

        strTmp = artist.Born;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Born = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.YearsActive;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.YearsActive = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Genres;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Genres = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Instruments;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Instruments = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Tones;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Tones = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Styles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Styles = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.AMGBio;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.AMGBio = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Image;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Image = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Albums;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Albums = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Compilations;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Compilations = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Singles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Singles = strTmp == "unknown" ? "" : strTmp;

        strTmp = artist.Misc;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Misc = strTmp == "unknown" ? "" : strTmp;

        if (null == MusicDbClient)
        {
          return;
        }

        strSQL = String.Format("delete from artistinfo where strArtist like '{0}%'", artist.Artist);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);

        strSQL =
          String.Format(
            "insert into artistinfo (strArtist,strBorn,strYearsActive,strGenres,strTones,strStyles,strInstruments,strImage,strAMGBio, strAlbums,strCompilations,strSingles,strMisc) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}' )",
            artist.Artist,
            artist.Born,
            artist.YearsActive,
            artist.Genres,
            artist.Tones,
            artist.Styles,
            artist.Instruments,
            artist.Image,
            artist.AMGBio,
            artist.Albums,
            artist.Compilations,
            artist.Singles,
            artist.Misc);

        MusicDbClient.Execute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public void DeleteAlbumInfo(string aAlbumName, string aArtistName)
    {
      string strAlbum = aAlbumName;
      DatabaseUtility.RemoveInvalidChars(ref strAlbum);
      string strArtist = aArtistName;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      strSQL = String.Format("delete from albuminfo where strAlbum like '{0}' and strArtist like '{1}'", strAlbum, strArtist);
      DirectExecute(strSQL);
    }

    public void DeleteArtistInfo(string aArtistName)
    {
      string strArtist = aArtistName;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      strSQL = String.Format("delete from artistinfo where strArtist like '{0}'", strArtist);
      DirectExecute(strSQL);
    }

    #endregion

    #region Lyrics

    public bool UpdateLyrics(string aFileName, string aLyrics)
    {
      try
      {
        string strFileName = aFileName;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        string strLyrics = aLyrics;
        DatabaseUtility.RemoveInvalidChars(ref strLyrics);

        strSQL = String.Format("SELECT * from tracks WHERE strPath = '{0}'", strFileName);

        SQLiteResultSet results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        int idSong = DatabaseUtility.GetAsInt(results, 0, "idTrack");

        strSQL = String.Format("UPDATE tracks SET strLyrics='{0}' where idTrack='{1}'", strLyrics, idSong);
        if (DirectExecute(strSQL).Rows.Count == 0)
        {
          Log.Debug("MusicDatabase: Updated Lyrics for song {0}", aFileName);
          return true;
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

    #region		Database rebuild

    /// <summary>
    /// Called from with GUIMusicFiles for a single folder update
    /// </summary>
    /// <param name="strFolder"></param>
    public void ImportFolder(object strFolder)
    {
      ArrayList shares = new ArrayList();
      shares.Add((string)strFolder);
      _singleFolderScan = true;

      MusicDatabaseReorg(shares, null);
    }

    /// <summary>
    /// This method is called out of plugins or the GUI
    /// </summary>
    /// <param name="shares"></param>
    /// <returns></returns>
    public int MusicDatabaseReorg(ArrayList shares)
    {
      LoadDBSettings();
      return MusicDatabaseReorg(shares, null);
    }

    /// <summary>
    /// This method is called directly from the Config dialog and should use all settings,
    /// which may have changed in the Config GUI
    /// </summary>
    /// <param name="shares"></param>
    /// <param name="setting"></param>
    /// <returns></returns>
    public int MusicDatabaseReorg(ArrayList shares, MusicDatabaseSettings setting)
    {
      // Get the values from the Setting Object, which we received from the Config
      using (Settings xmlreader = new MPSettings())
      {
        _updateSinceLastImport = xmlreader.GetValueAsBool("musicfiles", "updateSinceLastImport", false);
        _excludeHiddenFiles = xmlreader.GetValueAsBool("musicfiles", "excludeHiddenFiles", false);
      }
      if (setting != null)
      {
        _createMissingFolderThumbs = setting.CreateMissingFolderThumb;
        _extractEmbededCoverArt = setting.ExtractEmbeddedCoverArt;
        _stripArtistPrefixes = setting.StripArtistPrefixes;
        _treatFolderAsAlbum = setting.TreatFolderAsAlbum;
        _useFolderThumbs = setting.UseFolderThumbs;
        _useAllImages = setting.UseAllImages;
        _createArtistPreviews = setting.CreateArtistPreviews;
        _createGenrePreviews = setting.CreateGenrePreviews;
        _updateSinceLastImport = setting.UseLastImportDate;
        _dateAddedValue = setting.DateAddedValue;
        //_excludeHiddenFiles = setting.ExcludeHiddenFiles; <-- no GUI setting yet; use xml file to specify this
      }

      if (!_updateSinceLastImport && !_singleFolderScan)
      {
        _lastImport = DateTime.MinValue;
      }

      if (shares == null)
      {
        LoadShares();
      }
      else
      {
        _shares = (ArrayList)shares.Clone();
      }
      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();

      DateTime startTime = DateTime.UtcNow;

      try
      {
        if (_singleFolderScan)
        {
          Log.Info("Musicdatabasereorg: Importing Music for folder: {0}", _shares[0].ToString());
        }
        else
        {
          Log.Info("Musicdatabasereorg: Beginning music database reorganization...");
          Log.Info("Musicdatabasereorg: Last import done at {0}", _lastImport.ToString());
        }

        BeginTransaction();

        // When starting a complete rescan, we cleanup the foreign keys
        if (_lastImport == DateTime.MinValue && !_singleFolderScan)
        {
          MyArgs.progress = 2;
          MyArgs.phase = "Cleaning up Artists, AlbumArtists and Genres";
          OnDatabaseReorgChanged(MyArgs);
          Log.Info("Musicdatabasereorg: Cleaning up Artists, AlbumArtists and Genres");
          CleanupForeignKeys();
        }

        // Add missing files (example: You downloaded some new files)
        MyArgs.progress = 3;
        MyArgs.phase = "Scanning new files";
        OnDatabaseReorgChanged(MyArgs);
        Log.Info("Musicdatabasereorg: Scanning for music files");

        // Start the Scanning and Update Thread
        musicFolders = new List<string>(4000);
        cueFiles = new List<string>(500);
        allFiles = new Hashtable(100000);

        _resetEvent = new AutoResetEvent(false);
        _scanThread = new Thread(AddUpdateFiles);
        _scanThread.Start();

        // Wait for the Scan and Update Thread to finish, before continuing
        _resetEvent.WaitOne();

        Log.Info("Musicdatabasereorg: Total Songs: {0}. {1} added / {2} updated / {3} skipped", _processCount,
                 _songsAdded, _songsUpdated, _songsSkipped);

        DateTime stopTime = DateTime.UtcNow;
        TimeSpan ts = stopTime - startTime;
        float fSecsPerTrack = ((float)ts.TotalSeconds / (float)_processCount);
        string trackPerSecSummary = "";

        if (_processCount > 0)
        {
          trackPerSecSummary = string.Format(" ({0} seconds per track)", fSecsPerTrack);
        }

        Log.Info(
          "Musicdatabasereorg: Processed {0} tracks in: {1:d2}:{2:d2}:{3:d2}{4}", _processCount, ts.Hours, ts.Minutes,
          ts.Seconds, trackPerSecSummary);


        if (!_singleFolderScan)
        {
          // Delete files that don't exist anymore (example: you deleted files from the Windows Explorer)
          Log.Info("Musicdatabasereorg: Removing non existing songs from the database");
          MyArgs.progress = 80;
          MyArgs.phase = "Removing non existing songs";
          OnDatabaseReorgChanged(MyArgs);
          DeleteNonExistingSongs();
        }

        if (_useFolderThumbs)
        {
          Log.Info("Musicdatabasereorg: Create Folder Thumbs");
          CreateFolderThumbs(85, 90, _shares);
        }

        if (_createArtistPreviews)
        {
          Log.Info("Musicdatabasereorg: Create Artist Thumbs");
          CreateArtistThumbs(90, 92);
        }

        if (_createGenrePreviews)
        {
          Log.Info("Musicdatabasereorg: Create Genre Thumbs");
          CreateGenreThumbs(93, 94);
        }

        if (_createMissingFolderThumbs)
        {
          // implement sth like that:
          // Util.Picture.CreateThumbnail(aThumbLocation, folderThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
        }

        if (!_singleFolderScan)
        {
          MyArgs.progress = 95;
          MyArgs.phase = "Cleanup non-existing Artists, AlbumArtists and Genres";
          CleanupMultipleEntryTables();
        }
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabasereorg: Unhandled error {0} - scan aborted!\n{1}\n{2}", ex.Message, ex.Source,
                  ex.StackTrace);

        RollbackTransaction();
        _singleFolderScan = false;
        return (int)Errors.ERROR_CANCEL;
      }
      finally
      {
        MyArgs.progress = 96;
        MyArgs.phase = "Finishing";
        OnDatabaseReorgChanged(MyArgs);
        CommitTransaction();

        MyArgs.progress = 98;
        MyArgs.phase = "Compressing the database";
        OnDatabaseReorgChanged(MyArgs);
        Compress();

        MyArgs.progress = 100;
        MyArgs.phase = string.Format("Rescan completed. Total {0} Added {1} / Updated {2} / Skipped {3}", _processCount,
                                     _songsAdded, _songsUpdated, _songsSkipped);
        OnDatabaseReorgChanged(MyArgs);

        Log.Info("Musicdatabasereorg: Finished Reorganisation of the Database");

        // Save the time of the reorg, to be able to skip the files not updated / added the next time
        if (!_singleFolderScan)
        {
          using (Settings xmlreader = new MPSettings())
          {
            xmlreader.SetValue("musicfiles", "lastImport",
                               startTime.ToString("yyyy-M-d H:m:s", CultureInfo.InvariantCulture));
          }
        }

        GC.Collect();
      }
      _singleFolderScan = false;
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
        DatabaseUtility.CompactDatabase(Instance.DbConnection);
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
      _shares.Clear(); // Clear the list of Shares to avoid duplicates on a rerun

      using (Settings xmlreader = new MPSettings())
      {
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
        {
          string strSharePath = String.Format("sharepath{0}", i);
          string strShareType = String.Format("sharetype{0}", i);
          string strShareScan = String.Format("sharescan{0}", i);

          string SharePath = xmlreader.GetValueAsString("music", strSharePath, string.Empty);
          string ShareType = xmlreader.GetValueAsString("music", strShareType, "no");
          string ShareScan = xmlreader.GetValueAsString("music", strShareScan, "no");

          if (SharePath.Length > 0)
          {
            if (ShareType.ToLower() == "yes")
            {
              Log.Info("Musicdatabasereorg: Skipping scan of Remote Share: {0}", SharePath);
            }
            else if (ShareScan.ToLower() == "no")
            {
              Log.Info("Musicdatabasereorg: Skipping scan of non-selected Share: {0}", SharePath);
            }
            else
            {
              _shares.Add(SharePath);
            }
          }
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
        results = DirectExecute(strSQL);
        if (results == null)
        {
          return (int)Errors.ERROR_REORG_SONGS;
        }
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabasereorg: Unable to retrieve songs from database in DeleteNonExistingSongs() {0}",
                  ex.Message);
        return (int)Errors.ERROR_REORG_SONGS;
      }
      int removed = 0;
      Log.Info("Musicdatabasereorg: starting song cleanup for {0} songs", (int)results.Rows.Count);
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        string strFileName = DatabaseUtility.Get(results, i, "tracks.strPath");

        if (!allFiles.Contains(strFileName))
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
          MyArgs.phase = String.Format("Removing non existing songs:{0}/{1} checked, {2} removed", i, results.Rows.Count,
                                       removed);
          OnDatabaseReorgChanged(MyArgs);
        }
      } //for (int i=0; i < results.Rows.Count;++i)
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
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string strSQL =
          String.Format("select idTrack, strArtist, strAlbumArtist, strGenre from tracks where strPath = '{0}'",
                        strFileName);

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count > 0)
        {
          int idTrack = DatabaseUtility.GetAsInt(results, 0, "tracks.idTrack");
          string strArtist = DatabaseUtility.Get(results, 0, "tracks.strArtist");
          string strAlbumArtist = DatabaseUtility.Get(results, 0, "tracks.strAlbumArtist");
          string strGenre = DatabaseUtility.Get(results, 0, "tracks.strGenre");

          // Delete
          strSQL = String.Format("delete from tracks where idTrack={0}", idTrack);
          if (DirectExecute(strSQL).Rows.Count > 0)
          {
            Log.Info("Musicdatabase: Deleted no longer existing or moved song {0}", strFileName);
          }

          // Check if we have now Artists and Genres for which no song exists
          if (bCheck)
          {
            // split up the artist, in case we've got multiple artists
            string[] artists = strArtist.Split('|');
            foreach (string artist in artists)
            {
              strSQL = String.Format("select idTrack from tracks where strArtist like '%{0}%'", artist.Trim());
              if (DirectExecute(strSQL).Rows.Count == 0)
              {
                // Delete artist with no songs
                strSQL = String.Format("delete from artist where strArtist = '{0}'", artist.Trim());
                DirectExecute(strSQL);

                // Delete artist info
                strSQL = String.Format("delete from artistinfo where strArtist = '{0}'", artist.Trim());
                DirectExecute(strSQL);
              }
            }

            // split up the artist, in case we've got multiple artists
            string[] albumartists = strAlbumArtist.Split('|');
            foreach (string artist in albumartists)
            {
              strSQL = String.Format("select idTrack from tracks where strArtist like '%{0}%'", artist.Trim());
              if (DirectExecute(strSQL).Rows.Count == 0)
              {
                // Delete artist with no songs
                strSQL = String.Format("delete from albumartist where strAlbumArtist = '{0}'", artist.Trim());
                DirectExecute(strSQL);

                // Delete artist info
                strSQL = String.Format("delete from artistinfo where strArtist = '{0}'", artist.Trim());
                DirectExecute(strSQL);
              }
            }

            // split up the genre, in case we've got multiple genres
            string[] genres = strGenre.Split('|');
            foreach (string genre in genres)
            {
              strSQL = String.Format("select idTrack from tracks where strGenre like '%{0}%'", genre.Trim());
              if (DirectExecute(strSQL).Rows.Count == 0)
              {
                // Delete genres with no songs
                strSQL = String.Format("delete from genre where strGenre = '{0}'", genre.Trim());
                DirectExecute(strSQL);
              }
            }
          }
        }
        return;
      }
      catch (Exception ex1)
      {
        Log.Error("Musicdatabase: Exception err:{0} stack:{1}", ex1.Message, ex1.StackTrace);
        Open();
      }
      return;
    }

    #endregion

    #region Scan Folders

    /// <summary>
    /// Scan the folders in the selected shares for music files to be added to the database
    /// </summary>
    private void AddUpdateFiles()
    {
      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      string strSQL;

      _processCount = 0;
      _songsAdded = 0;
      _songsUpdated = 0;
      _songsSkipped = 0;

      int allFilesCount = 0;

      foreach (string Share in _shares)
      {
        //dummy call to stop lots of watchers being created unnecessarily
        Util.Utils.FileExistsInCache(Path.Combine(Share, "folder.jpg"));
        // Get all the files for the given Share / Path
        try
        {
          foreach (FileInformation file in GetFilesRecursive(new DirectoryInfo(Share)))
          {
            allFilesCount++;

            if (allFilesCount % 1000 == 0)
            {
              Log.Info("MusicDBReorg: Procesing file {0}", allFilesCount);
            }

            MyArgs.progress = 4;
            MyArgs.phase = String.Format("Processing file {0}", allFilesCount);
            OnDatabaseReorgChanged(MyArgs);

            if (!CheckFileForInclusion(file))
            {
              continue;
            }

            _processCount++;

            AddUpdateSong(file.Name);
          }
        }
        catch (Exception ex)
        {
          Log.Error("MusicDBReorg: Exception accessing file or folder: {0}", ex.Message);
        }
      }

      // Now we will remove the CUE data file from the database, since we will add Fake Tracks in the next step
      foreach (string cueFile in cueFiles)
      {
        try
        {
          CueSheet cueSheet = new CueSheet(cueFile);
          string cuePath = Path.GetDirectoryName(cueFile);
          string cueDataFile = cuePath + "\\" + cueSheet.Tracks[0].DataFile.Filename;
          DatabaseUtility.RemoveInvalidChars(ref cueDataFile);
          strSQL = String.Format("delete from tracks where strPath='{0}'", cueDataFile);
          try
          {
            DirectExecute(strSQL);
          }
          catch (Exception ex)
          {
            Log.Error("Error deleting song from Database: {0}", ex.Message);
          }
        }
        catch (Exception ex)
        {
          Log.Error("Exception Processing CUE File: {0}: {1}", cueFile, ex.Message);
        }
      }

      try
      {
        // Apply CUE Filter
        List<string> cueFileFakeTracks = new List<string>();
        cueFileFakeTracks =
          (List<string>)CueUtil.CUEFileListFilterList<string>(cueFiles, CueUtil.CUE_TRACK_FILE_STRING_BUILDER);

        // and add them also to the Hashtable, so that they don't get deleted in the next step
        foreach (string song in cueFileFakeTracks)
        {
          _processCount++;
          AddUpdateSong(song);
          allFiles.Add(song, false);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Error processing CUE files: {0}", ex.Message);
      }

      _resetEvent.Set();
    }

    /// <summary>
    /// Check, if a file should be Added or Updated to the DB
    /// </summary>
    /// <param name="file"></param>
    private void AddUpdateSong(string file)
    {
      SQLiteResultSet results;
      string strSQL;

      string song = file;
      string strFileName = song;
      DatabaseUtility.RemoveInvalidChars(ref strFileName);
      strSQL = String.Format("select idTrack from tracks where strPath='{0}'", strFileName);

      try
      {
        results = MusicDbClient.Execute(strSQL);
        if (results == null)
        {
          Log.Info("Musicdatabasereorg: AddMissingFiles finished with error (results == null)");
          return;
        }
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: AddMissingFiles finished with error (exception for select)");
        return;
      }

      if (results.Rows.Count == 0)
      {
        //The song does not exist, we will add it.
        AddSong(song);
        _songsAdded++;
      }
      else
      {
        UpdateSong(song);
        _songsUpdated++;
      }
    }


    /// <summary>
    /// Build an Iterator over the given Directory, returning all files recursively
    /// </summary>
    /// <param name="dirInfo"></param>
    /// <returns></returns>
    private IEnumerable<FileInformation> GetFilesRecursive(DirectoryInfo dirInfo)
    {
      return GetFilesRecursive(dirInfo, "*.*");
    }

    /// <summary>
    /// Build an Iterator over the given Directory, returning all files recursively
    /// </summary>
    /// <param name="dirInfo">DirectoryInfo for the directory we want files returned for</param>
    /// <param name="searchPattern">This parameter is ignored in the current implementation</param>
    /// <returns></returns>
    /// 
    /// A much more elegant way would be using the method below, but it is not allowed to have a yield inside a try / catch,
    /// and we need to capture Access Exceptions to Directories and Files.
    /// The method used now has the same speed.
    /// 
    /// private IEnumerable<FileInfo> GetFilesRecursive(DirectoryInfo dirInfo, string searchPattern)
    /// {
    ///  foreach (DirectoryInfo di in dirInfo.GetDirectories())
    ///  {
    ///    musicFolders.Add(di.FullName);
    ///    foreach (FileInfo fi in GetFilesRecursive(di, searchPattern))
    ///    {
    ///      yield return fi;
    ///    }
    ///  }
    ///
    ///  foreach (FileInfo fi in dirInfo.GetFiles(searchPattern))
    ///  {
    ///    yield return fi;
    ///  }
    /// }
    private IEnumerable<FileInformation> GetFilesRecursive(DirectoryInfo dirInfo, string searchPattern)
    {
      Queue<DirectoryInfo> directories = new Queue<DirectoryInfo>();
      directories.Enqueue(dirInfo);
      Queue<FileInformation> files = new Queue<FileInformation>();
      while (files.Count > 0 || directories.Count > 0)
      {
        if (files.Count > 0)
        {
          yield return files.Dequeue();
        }
        try
        {
          if (directories.Count > 0)
          {
            DirectoryInfo dir = directories.Dequeue();
            musicFolders.Add(dir.FullName);

            DirectoryInfo[] newDirectories = dir.GetDirectories();
            FileInformation[] newFiles = MediaPortal.Util.NativeFileSystemOperations.GetFileInformation(dir.FullName,
                                                                                                        !_excludeHiddenFiles);
            // dir.GetFiles(searchPattern);
            string[] newFiles2 = MediaPortal.Util.NativeFileSystemOperations.GetFiles(dir.FullName);
            foreach (DirectoryInfo di in newDirectories)
            {
              directories.Enqueue(di);
            }
            foreach (FileInformation file in newFiles)
            {
              files.Enqueue(file);
            }
          }
        }
        catch (UnauthorizedAccessException ex)
        {
          Log.Error("MusicDBReorg: File / Directory Access error: {0}", ex.Message);
        }
      }
    }


    /// <summary>
    /// Should the file be included in the list to be added
    /// </summary>
    /// <param name="fileInfo"></param>
    private bool CheckFileForInclusion(FileInformation fileInfo)
    {
      string file = fileInfo.Name;
      bool fileinCluded = false;
      try
      {
        string ext = Path.GetExtension(file).ToLower();
        if (ext == ".m3u")
        {
          return false;
        }
        if (ext == ".pls")
        {
          return false;
        }
        if (ext == ".wpl")
        {
          return false;
        }
        if (ext == ".b4s")
        {
          return false;
        }


        // Provide an easy way to exclude problematic or unwanted files from the scan
        //if (_excludeHiddenFiles)
        //{
        //  if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
        //  {
        //    return false;
        //  }
        //}

        // Only get files with the required extension
        if (_supportedExtensions.IndexOf(ext) == -1)
        {
          return false;
        }

        // Cue Files are being processed separately
        if (CueUtil.isCueFile(file))
        {
          cueFiles.Add(file);
          return false;
        }

        // Add the files to the Hastable, which is used in the Delete Non-existing songs, to prevent file system access
        allFiles.Add(file, false);

        // Only Add files to the list, if they have been Created / Updated after the Last Import date
        if (_updateSinceLastImport && !_singleFolderScan)
        {
          if (fileInfo.CreationTime > _lastImport || fileInfo.ModificationTime > _lastImport)
          {
            _songsProcessed++;
            fileinCluded = true;
          }
          else
          {
            _songsSkipped++;
            fileinCluded = false;
          }
        }
        else
        {
          _songsProcessed++;
          fileinCluded = true;
        }
      }
      catch (UnauthorizedAccessException)
      {
        Log.Warn("Musicdatabasereorg: Not enough permissions to include file {0}", file);
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabasereorg: Cannot include file {0} because of {1}", file, ex.Message);
      }
      return fileinCluded;
    }

    #endregion

    #region Add / Update Song

    /// <summary>
    /// Retrieves the Tags from a file and tries to extract coverart
    /// </summary>
    /// <param name="strFileName">The full path for a music file to process</param>
    /// <returns>A MusicTag with escaped chars formatted suiteable for insertion into the database</returns>
    public MusicTag GetTag(string strFileName)
    {
      MusicTag tag = TagReader.TagReader.ReadTag(strFileName);
      if (tag != null)
      {
        // Extract the Coverart first because else the quote string escape will result in double "'" for the coverart filenames
        ExtractCoverArt(tag);

        string strTmp;
        strTmp = tag.Album;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Album = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Genre;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Genre = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Artist;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Artist = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Title;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Title = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.AlbumArtist;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.AlbumArtist = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Lyrics;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Lyrics = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Composer;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Composer = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Conductor;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Conductor = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Comment;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Comment = strTmp == "unknown" ? "" : strTmp;
        strTmp = tag.Codec;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        tag.Codec = strTmp == "unknown" ? "" : strTmp;

        if (!tag.HasAlbumArtist)
        {
          tag.AlbumArtist = tag.Artist;
        }

        // When we got Multiple Entries of either Artist, Genre, Albumartist in WMP notation, separated by ";",
        // we will store them separeted by "|"
        tag.Artist = Util.Utils.FormatMultiItemMusicString(tag.Artist, _stripArtistPrefixes);
        tag.AlbumArtist = Util.Utils.FormatMultiItemMusicString(tag.AlbumArtist, _stripArtistPrefixes);
        tag.Genre = Util.Utils.FormatMultiItemMusicString(tag.Genre, false);
        tag.Composer = Util.Utils.FormatMultiItemMusicString(tag.Composer, _stripArtistPrefixes);

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
      // Get the Tags from the file
      MusicTag tag = GetTag(strFileName);
      if (tag != null)
      {
        DateTime dateadded = DateTime.Now;
        switch (_dateAddedValue)
        {
          case 0:
            // Nothing to do here. we have already set the curremt date before the switch. statement left here for completness
            break;

          case 1:
            dateadded = File.GetCreationTime(strFileName);
            break;

          case 2:
            dateadded = File.GetLastWriteTime(strFileName);
            break;
        }

        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        strSQL =
          String.Format(
            @"insert into tracks (
                               strPath, strArtist, strAlbumArtist, strAlbum, strGenre, strComposer, strConductor, strTitle, 
                               iTrack, iNumTracks, iDuration, iYear, iTimesPlayed, iRating, iFavorite, 
                               iResumeAt, iDisc, iNumDisc, strLyrics, strComment, strFileType, strFullCodec, strBitRateMode, 
                               iBPM, iBitRate, iChannels, iSampleRate, dateLastPlayed, dateAdded) 
                               values ( 
                               '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}',
                               {8}, {9}, {10}, {11}, {12}, {13}, {14}, 
                               {15}, {16}, {17}, '{18}', '{19}',  '{20}', '{21}',  '{22}',  {23}, {24}, {25}, {26}, '{27}', '{28}' )",
            strFileName, tag.Artist, tag.AlbumArtist, tag.Album, tag.Genre, tag.Composer, tag.Conductor, tag.Title,
            tag.Track, tag.TrackTotal, tag.Duration, tag.Year, 0, tag.Rating, 0,
            0, tag.DiscID, tag.DiscTotal, tag.Lyrics, tag.Comment, tag.FileType, tag.Codec, tag.BitRateMode,
            tag.BPM, tag.BitRate, tag.Channels, tag.SampleRate, DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"), dateadded.ToString("yyyy-MM-dd HH:mm:ss")
            );
        try
        {
          SQLiteResultSet results = DirectExecute(strSQL);
          if (results == null)
          {
            Log.Info("Insert of song {0} failed", strFileName);
            return (int)Errors.ERROR_REORG_SONGS;
          }

          // Now add the Multiple Value Fields to their tables
          AddMultipleValueFields(tag);

          if (_treatFolderAsAlbum)
          {
            UpdateVariousArtist(tag);
          }
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


    private void AddMultipleValueFields(MusicTag tag)
    {
      try
      {
        string strSQL;
        string strMultiValueFieldValue = "";

        foreach (string field in _multipleValueFields)
        {
          // split up the multiple value field
          strMultiValueFieldValue = GetMultipleValueFieldValue(tag, field).Trim(new char[] {'|', ' '});
          string[] splittedFields = strMultiValueFieldValue.Split(new char[] {';', '|'});
          foreach (string s in splittedFields)
          {
            // ATTENTION: We need to use the 'like' operator instead of '=' to have case insensitive searching
            strSQL = String.Format("select {0} from {1} where {0} like '{2}'", GetMultipleValueField(field),
                                   GetMultipleValueTable(field), s == "" ? " " : s.Trim());
            if (DirectExecute(strSQL).Rows.Count < 1)
            {
              // Insert the Artist
              strSQL = String.Format("insert into {1} ({0}) values ('{2}')", GetMultipleValueField(field),
                                     GetMultipleValueTable(field), s == "" ? " " : s.Trim());
              DirectExecute(strSQL);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabase: Exception adding multiple field value: {0} stack: {1}", ex.Message, ex.StackTrace);
        Open();
      }
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
          DateTime dateadded = DateTime.Now;
          switch (_dateAddedValue)
          {
            case 0:
              // Nothing to do here. we have already set the curremt date before the switch. statement left here for completness
              break;

            case 1:
              dateadded = File.GetCreationTime(strFileName);
              break;

            case 2:
              dateadded = File.GetLastWriteTime(strFileName);
              break;
          }

          DatabaseUtility.RemoveInvalidChars(ref strFileName);

          strSQL =
            String.Format(
              @"update tracks 
                                 set strArtist = '{0}', strAlbumArtist = '{1}', strAlbum = '{2}', 
                                 strGenre = '{3}', strTitle = '{4}', iTrack = {5}, iNumTracks = {6}, 
                                 iDuration = {7}, iYear = {8}, iRating = {9}, iDisc = {10}, iNumDisc = {11}, 
                                 strLyrics = '{12}', strComposer = '{13}', strConductor = '{14}',
                                 strComment = '{15}', strFileType = '{16}', strFullCodec = '{17}',
                                 strBitRateMode = '{18}', iBPM = {19}, iBitRate = {20}, iChannels = {21},
                                 iSampleRate = {22}, dateAdded = '{23}' 
                                 where strPath = '{24}'",
              tag.Artist, tag.AlbumArtist, tag.Album,
              tag.Genre, tag.Title, tag.Track, tag.TrackTotal,
              tag.Duration, tag.Year, tag.Rating, tag.DiscID, tag.DiscTotal,
              tag.Lyrics, tag.Composer, tag.Conductor, tag.Comment, tag.FileType, tag.Codec,
              tag.BitRateMode, tag.BPM, tag.BitRate, tag.Channels, tag.SampleRate, dateadded.ToString("yyyy-MM-dd HH:mm:ss"),
              strFileName
              );
          try
          {
            DirectExecute(strSQL);

            // Now add the Artist, AlbumArtist and Genre to the Artist / Genre Tables
            AddMultipleValueFields(tag);

            if (_treatFolderAsAlbum)
            {
              UpdateVariousArtist(tag);
            }
          }
          catch (Exception)
          {
            Log.Error("MusicDatabase: Update tags for {0} failed because of DB exception", strFileName);
            return false;
          }
        }
        else
        {
          Log.Info("MusicDatabase: cannot get tag for {0}", strFileName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      return true;
    }

    private void ExtractCoverArt(MusicTag tag)
    {
      string formattedAlbum = tag.Album.Trim(trimChars);

      // Mantis 3078: Filename for Multiple Artists should not contain a semicolon, cause it wouldn't be found later on
      int i = 0;
      string formattedArtist = "";
      string[] strArtistSplit = tag.Artist.Split(new char[] {';', '|'});
      foreach (string strArtist in strArtistSplit)
      {
        string s = strArtist.Trim();
        if (_stripArtistPrefixes)
        {
          Util.Utils.StripArtistNamePrefix(ref s, true);
        }

        // Concatenate multiple Artists with " _ "
        // When we search for multiple artists Covers: "artist a | artist b", the string " | " gets replaced by " _ ",
        // so we need to build the file accordingly
        if (i > 0)
        {
          formattedArtist += " _ ";
        }
        formattedArtist += s.Trim();
        i++;
      }

      string tagAlbumName = string.Format("{0}-{1}", formattedArtist, formattedAlbum);
      string smallThumbPath = Util.Utils.GetCoverArtName(Thumbs.MusicAlbum, Util.Utils.MakeFileName(tagAlbumName));
      string largeThumbPath = Util.Utils.GetLargeCoverArtName(Thumbs.MusicAlbum, Util.Utils.MakeFileName(tagAlbumName));

      // Get the cover image directly out of the mp3 file's ID3 tag
      if (_extractEmbededCoverArt)
      {
        try
        {
          if (tag.CoverArtImageBytes != null)
          {
            bool extractFile = false;
            if (!Util.Utils.FileExistsInCache(smallThumbPath))
            {
              extractFile = true;
            }
            else
            {
              // Prevent creation of the thumbnail multiple times, when all songs of an album contain coverart <-- that's ugly (rtv)
              try
              {
                DateTime fileDate = File.GetLastWriteTime(smallThumbPath);
                TimeSpan span = _currentDate - fileDate;
                if (span.Days > 0)
                {
                  extractFile = true;
                }
              }
              catch (Exception)
              {
                extractFile = true;
              }
            }

            if (extractFile)
            {
              try
              {
                string mp3TagImage = tag.CoverArtFile;

                if (!String.IsNullOrEmpty(mp3TagImage))
                {
                  if (
                    !Util.Picture.CreateThumbnail(mp3TagImage, smallThumbPath, (int)Thumbs.ThumbResolution,
                                                  (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
                  {
                    Log.Info("MusicDatabase: Could not extract thumbnail from {0}", tag.FileName);
                  }
                  if (
                    !Util.Picture.CreateThumbnail(mp3TagImage, largeThumbPath, (int)Thumbs.ThumbLargeResolution,
                                                  (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge))
                  {
                    Log.Info("MusicDatabase: Could not extract thumbnail from {0}", tag.FileName);
                  }

                  Util.Utils.FileDelete(mp3TagImage); // clean up the temp file directly
                }
              }
              catch (Exception)
              {
                Log.Warn("MusicDatabase: Invalid cover art image found in {0}-{1}! {2}", tag.Artist, tag.Title,
                         tag.FileName);
              }
            }
          }
        }
        catch (Exception) {}
      }

      // Scan folders only one time per song
      if (string.IsNullOrEmpty(_previousNegHitDir) || (Path.GetDirectoryName(tag.FileName) != _previousNegHitDir))
      {
        string sharefolderThumb;
        // no mp3 coverart - use folder art if present to get an album thumb
        if (_useFolderThumbs)
        {
          // Do not overwrite covers extracted from mp3 files.
          if (!Util.Utils.FileExistsInCache(smallThumbPath))
          {
            // No Album thumb found - create one.            
            bool foundThumb = false;

            if (_useAllImages)
            {
              sharefolderThumb = Util.Utils.TryEverythingToGetFolderThumbByFilename(tag.FileName, true);
              if (string.IsNullOrEmpty(sharefolderThumb))
              {
                _previousNegHitDir = Path.GetDirectoryName(tag.FileName);
                //Log.Debug("MusicDatabase: No useable album art images found in {0}", _previousNegHitDir);
              }
              else
              {
                _previousNegHitDir = string.Empty;
                foundThumb = true;
              }
            }
            else
            {
              sharefolderThumb = Util.Utils.GetFolderThumb(tag.FileName);
              if (Util.Utils.FileExistsInCache(sharefolderThumb))
              {
                foundThumb = true;
              }
            }

            if (foundThumb)
            {
              if (
                !Util.Picture.CreateThumbnail(sharefolderThumb, smallThumbPath, (int)Thumbs.ThumbResolution,
                                              (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
              {
                Log.Info("MusicDatabase: Could not create album thumb from folder {0}", tag.FileName);
              }
              if (
                !Util.Picture.CreateThumbnail(sharefolderThumb, largeThumbPath, (int)Thumbs.ThumbLargeResolution,
                                              (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge))
              {
                Log.Info("MusicDatabase: Could not create large album thumb from folder {0}", tag.FileName);
              }
            }
          }
        }

        // MP has an album cover in the thumb cache (maybe downloaded from last.fm) but no folder.jpg in the shares - create it
        if (_createMissingFolderThumbs)
        {
          if (string.IsNullOrEmpty(_previousPosHitDir) || (Path.GetDirectoryName(tag.FileName) != _previousPosHitDir))
          {
            sharefolderThumb = Util.Utils.GetFolderThumb(tag.FileName);
            if (!Util.Utils.FileExistsInCache(sharefolderThumb))
            {
              string sourceCover = Util.Utils.TryEverythingToGetFolderThumbByFilename(tag.FileName, true);
              if (string.IsNullOrEmpty(sourceCover))
              {
                sourceCover = smallThumbPath;
              }
              if (Util.Utils.FileExistsInCache(Util.Utils.ConvertToLargeCoverArt(sourceCover)))
              {
                sourceCover = Util.Utils.ConvertToLargeCoverArt(sourceCover);
              }
              if (Util.Utils.FileExistsInCache(sourceCover))
              {
                _previousPosHitDir = Path.GetDirectoryName(tag.FileName);
                //FolderThumbCreator newThumb = new FolderThumbCreator(tag.FileName, tag);
                if (
                  !Util.Picture.CreateThumbnail(sourceCover, sharefolderThumb, (int)Thumbs.ThumbLargeResolution,
                                                (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge))
                {
                  Log.Info("MusicDatabase: Could not create missing folder thumb in share path {0}", sharefolderThumb);
                }
                Thread.Sleep(1);
              }
            }
          }
        }
      }
    }

    private void CreateFolderThumbs(int aProgressStart, int aProgressEnd, ArrayList aShareList)
    {
      DatabaseReorgEventArgs MyFolderArgs = new DatabaseReorgEventArgs();
      foreach (string sharePath in aShareList)
      {
        try
        {
          int i = 0;
          foreach (string coverPath in musicFolders)
          {
            try
            {
              //string displayPath = Path.GetDirectoryName(coverPath);
              //displayPath = displayPath.Remove(0, displayPath.LastIndexOf('\\'));
              MyFolderArgs.phase = string.Format("Caching folder thumbs: {0}/{1}", Convert.ToString(i + 1),
                                                 Convert.ToString(musicFolders.Count));
              // range = 80-89
              int folderProgress = aProgressStart + (((i + 1) / musicFolders.Count) * (aProgressEnd - aProgressStart));
              MyFolderArgs.progress = folderProgress;
              OnDatabaseReorgChanged(MyFolderArgs);

              bool foundCover = false;
              string sharefolderThumb;
              // We add the slash to be able to use TryEverythingToGetFolderThumbByFilename and GetLocalFolderThumb
              string currentPath = string.Format("{0}\\", coverPath);
              string localFolderThumb = Util.Utils.GetLocalFolderThumb(currentPath);
              string localFolderLThumb = Util.Utils.ConvertToLargeCoverArt(localFolderThumb);

              if (_useAllImages)
              {
                sharefolderThumb = Util.Utils.TryEverythingToGetFolderThumbByFilename(currentPath, true);
                if (!string.IsNullOrEmpty(sharefolderThumb))
                {
                  foundCover = true;
                }
              }
              else
              {
                sharefolderThumb = Util.Utils.GetFolderThumb(currentPath);
                if (Util.Utils.FileExistsInCache(sharefolderThumb))
                {
                  foundCover = true;
                }
              }

              if (foundCover)
              {
                if (
                  !Util.Picture.CreateThumbnail(sharefolderThumb, localFolderThumb, (int)Thumbs.ThumbResolution,
                                                (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
                {
                  Log.Info("MusicDatabase: Could not cache folder thumb from folder {0}", sharePath);
                }
                if (
                  !Util.Picture.CreateThumbnail(sharefolderThumb, localFolderLThumb, (int)Thumbs.ThumbLargeResolution,
                                                (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge))
                {
                  Log.Info("MusicDatabase: Could not cache large folder thumb from folder {0}", sharePath);
                }
              }
            }
            catch (Exception ex2)
            {
              Log.Error("MusicDatabase: Error caching folder thumb of {0} - {1}", coverPath, ex2.Message);
            }
            i++;
          }
        }
        catch (Exception ex)
        {
          Log.Error("MusicDatabase: Error caching folder thumbs - {0}", ex.Message);
        }
      }
    }

    private void CreateArtistThumbs(int aProgressStart, int aProgressEnd)
    {
      DatabaseReorgEventArgs MyArtistArgs = new DatabaseReorgEventArgs();
      ArrayList allArtists = new ArrayList();
      List<Song> groupedArtistSongs = new List<Song>();
      List<String> imageTracks = new List<string>();

      if (GetAllArtists(ref allArtists))
      {
        for (int i = 0; i < allArtists.Count; i++)
        {
          string curArtist = allArtists[i].ToString();
          if (!string.IsNullOrEmpty(curArtist) && curArtist != "unknown")
          {
            MyArtistArgs.phase = string.Format("Creating artist preview thumbs: {1}/{2} - {0}", curArtist,
                                               Convert.ToString(i + 1), Convert.ToString(allArtists.Count));
            // range = 80-89
            int artistProgress = aProgressStart + (((i + 1) / allArtists.Count) * (aProgressEnd - aProgressStart));
            MyArtistArgs.progress = artistProgress;
            OnDatabaseReorgChanged(MyArtistArgs);

            groupedArtistSongs.Clear();
            imageTracks.Clear();
            string artistThumbPath = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, curArtist);
            if (!Util.Utils.FileExistsInCache(artistThumbPath))
            {
              if (GetSongsByArtist(curArtist, ref groupedArtistSongs, true))
              {
                for (int j = 0; j < groupedArtistSongs.Count; j++)
                {
                  bool foundDup = false;
                  string coverArt = Util.Utils.TryEverythingToGetFolderThumbByFilename(groupedArtistSongs[j].FileName,
                                                                                       true);
                  if (!string.IsNullOrEmpty(coverArt))
                  {
                    foreach (string dupCheck in imageTracks)
                    {
                      if (dupCheck == coverArt)
                      {
                        foundDup = true;
                      }
                    }
                    if (!foundDup)
                    {
                      imageTracks.Add(coverArt);
                    }
                  }

                  // we need a maximum of 4 covers for the preview
                  if (imageTracks.Count >= 4)
                  {
                    break;
                  }
                }

                if (Util.Utils.CreateFolderPreviewThumb(imageTracks, artistThumbPath))
                {
                  Log.Info("MusicDatabase: Added artist thumb for {0}", curArtist);
                }
              }
            }
          }
        }
      }
    }


    private void CreateGenreThumbs(int aProgressStart, int aProgressEnd)
    {
      DatabaseReorgEventArgs MyGenreArgs = new DatabaseReorgEventArgs();
      ArrayList allGenres = new ArrayList();
      List<Song> groupedGenreSongs = new List<Song>();
      List<String> imageTracks = new List<string>();

      if (GetGenres(ref allGenres))
      {
        for (int i = 0; i < allGenres.Count; i++)
        {
          string curGenre = allGenres[i].ToString();
          if (!string.IsNullOrEmpty(curGenre) && curGenre != "unknown")
          {
            MyGenreArgs.phase = string.Format("Creating genre preview thumbs: {1}/{2} - {0}", curGenre,
                                              Convert.ToString(i + 1), Convert.ToString(allGenres.Count));
            int genreProgress = aProgressStart + (((i + 1) / allGenres.Count) * (aProgressEnd - aProgressStart));
            MyGenreArgs.progress = genreProgress;
            OnDatabaseReorgChanged(MyGenreArgs);

            groupedGenreSongs.Clear();
            imageTracks.Clear();
            string genreThumbPath = Util.Utils.GetCoverArtName(Thumbs.MusicGenre, curGenre);
            if (!Util.Utils.FileExistsInCache(genreThumbPath))
            {
              if (GetSongsByGenre(curGenre, ref groupedGenreSongs, true))
              {
                for (int j = 0; j < groupedGenreSongs.Count; j++)
                {
                  bool foundDup = false;
                  string coverArt = Util.Utils.TryEverythingToGetFolderThumbByFilename(groupedGenreSongs[j].FileName,
                                                                                       true);
                  if (!string.IsNullOrEmpty(coverArt))
                  {
                    foreach (string dupCheck in imageTracks)
                    {
                      if (dupCheck == coverArt)
                      {
                        foundDup = true;
                      }
                    }
                    if (!foundDup)
                    {
                      imageTracks.Add(coverArt);
                    }
                  }
                  // we need a maximum of 4 covers for the preview
                  if (imageTracks.Count >= 4)
                  {
                    break;
                  }
                }

                if (Util.Utils.CreateFolderPreviewThumb(imageTracks, genreThumbPath))
                {
                  Log.Info("MusicDatabase: Added genre thumb for {0}", curGenre);
                }
              }
            }
          }
        } // for all genres
      }
    }

    private void UpdateVariousArtist(MusicTag tag)
    {
      string currentDir = Path.GetDirectoryName(tag.FileName);
      // on first cal, set the directory name
      if (_previousDirectory == null)
      {
        _previousDirectory = currentDir;
      }

      if (_previousMusicTag == null)
      {
        _previousMusicTag = tag;
      }

      if (_previousDirectory.ToLowerInvariant() == currentDir.ToLowerInvariant())
      {
        // already have detected Various artists in this folder. no further checking needed
        if (_foundVariousArtist)
        {
          return;
        }

        // Is the Artist different and also no different AlbumArtist set?
        if (_previousMusicTag.Artist != tag.Artist && !tag.HasAlbumArtist)
        {
          _foundVariousArtist = true;
          return;
        }
      }
      else
      {
        if (_foundVariousArtist)
        {
          // Let's add the "Various Artist" to the albumArtist table
          string varArtist = "| Various Artists | ";
          strSQL = string.Format("insert into albumartist (strAlbumArtist) values('{0}')", varArtist);
          DirectExecute(strSQL);

          List<SongMap> songs = new List<SongMap>();
          GetSongsByPath(_previousDirectory, ref songs);

          foreach (SongMap map in songs)
          {
            int id = map.m_song.Id;
            strSQL = string.Format("update tracks set strAlbumArtist = '| Various Artists | ' where idTrack={0}", id);
            DirectExecute(strSQL);

            // Now we need to remove the Artist of the song from the AlbumArtist table,
            // if he's not an AlbumArtist in a different album
            Song song = map.m_song as Song;
            string[] strAlbumArtistSplit = song.AlbumArtist.Split(new char[] {'|'});
            foreach (string strTmp in strAlbumArtistSplit)
            {
              string strAlbumArtist = strTmp.Trim(trimChars);
              DatabaseUtility.RemoveInvalidChars(ref strAlbumArtist);

              strSQL = string.Format("select strAlbumArtist from tracks where strAlbumArtist like '%| {0} |%'",
                                     strAlbumArtist);
              if (DirectExecute(strSQL).Rows.Count < 1)
              {
                // No AlbumArtist entry found, so let's remove this artist from albumartist
                strSQL = String.Format("delete from albumartist where strAlbumArtist like '%{0}%'", strAlbumArtist);
                DirectExecute(strSQL);
              }
            }
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
    /// When tags of a song have been updated, it might happen that we have entries in on of the Multiple Value Fields Table,
    /// for which no longer a song exists.
    /// Do a cleanup to get rid of them.
    /// </summary>
    private void CleanupMultipleEntryTables()
    {
      // Working with a Temporary table is much faster, than reading single rows
      if (DatabaseUtility.TableExists(MusicDbClient, "tbltmp"))
      {
        MusicDbClient.Execute("drop table tbltmp");
      }

      try
      {
        string strSQL;

        foreach (string field in _multipleValueFields)
        {
          Log.Info("Musicdatabasereorg: Cleaning up {0} with no songs.", field);
          strSQL = "create table tbltmp (strMultiField text)";
          MusicDbClient.Execute(strSQL);

          strSQL = String.Format("select distinct rtrim(ltrim({0}, '| '), ' |') from tracks",
                                 GetMultipleValueField(field));
          SQLiteResultSet results = DirectExecute(strSQL);
          for (int i = 0; i < results.Rows.Count; i++)
          {
            string[] splittedFields = DatabaseUtility.Get(results, i, 0).Split('|');
            foreach (string s in splittedFields)
            {
              string strTmp = s;
              DatabaseUtility.RemoveInvalidChars(ref strTmp);
              if (strTmp == "unknown")
              {
                strTmp = " ";
              }

              strSQL = String.Format("insert into tbltmp values('{0}')", strTmp.Trim() == "" ? " " : strTmp.Trim());
              MusicDbClient.Execute(strSQL);
            }
          }

          strSQL = String.Format("delete from {0} where {1} not in (select distinct strMultiField from tbltmp)",
                                 GetMultipleValueTable(field), GetMultipleValueField(field));
          MusicDbClient.Execute(strSQL);

          MusicDbClient.Execute("drop table tbltmp");
        }
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabase: Exception adding multiple field value: {0} stack: {1}", ex.Message, ex.StackTrace);
        Open();
      }
      Log.Info("Musicdatabasereorg: Finished with cleaning up Mutiple Value Fields Tables.");
    }

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
        strSql = "delete from composer";
        MusicDbClient.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: CleanupForeignKeys failed");
        return (int)Errors.ERROR_REORG_ARTIST;
      }

      Log.Info("Musicdatabasereorg: CleanupForeignKeys completed");
      return (int)Errors.ERROR_OK;
    }

    /// <summary>
    /// Returns the SQLITE Table name for the requested field
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private string GetMultipleValueTable(string field)
    {
      if (field == "artist")
      {
        return "artist";
      }
      else if (field == "albumartist")
      {
        return "albumartist";
      }
      else if (field == "genre")
      {
        return "genre";
      }
      else if (field == "composer")
      {
        return "composer";
      }
      return "";
    }

    /// <summary>
    /// Returns the Sqlite field name for the requested field
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private string GetMultipleValueField(string field)
    {
      if (field == "artist")
      {
        return "strArtist";
      }
      else if (field == "albumartist")
      {
        return "strAlbumartist";
      }
      else if (field == "genre")
      {
        return "strGenre";
      }
      else if (field == "composer")
      {
        return "strComposer";
      }
      return "";
    }

    /// <summary>
    /// Returns the field value out of the MusicTag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    private string GetMultipleValueFieldValue(MusicTag tag, string field)
    {
      if (field == "artist")
      {
        return tag.Artist;
      }
      else if (field == "albumartist")
      {
        return tag.AlbumArtist;
      }
      else if (field == "genre")
      {
        return tag.Genre;
      }
      else if (field == "composer")
      {
        return tag.Composer;
      }
      return "";
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
      strSQL = String.Format("select idTrack from tracks where strPath like '{0}'",
                             strFileName);

      SQLiteResultSet results;
      results = MusicDbClient.Execute(strSQL);
      if (results.Rows.Count > 0)
      {
        // Found
        return true;
      }
      else
      {
        // Not Found
        return false;
      }
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
        {
          return -1;
        }
        if (userName_.Length == 0)
        {
          return -1;
        }
        string strUserName = userName_;

        DatabaseUtility.RemoveInvalidChars(ref strUserName);


        strSQL = String.Format("select * from scrobbleusers where strUsername like '{0}'", strUserName);
        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into scrobbleusers (idScrobbleUser , strUsername) values ( NULL, '{0}' )",
                                 strUserName);
          DirectExecute(strSQL);
          Log.Info("MusicDatabase: added scrobbleuser {0} with ID {1}", strUserName,
                   Convert.ToString(MusicDbClient.LastInsertID()));
          return Instance.DbConnection.LastInsertID();
        }
        else
        {
          return DatabaseUtility.GetAsInt(results, 0, "idScrobbleUser");
        }
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
        {
          return string.Empty;
        }
        if (userID_.Length == 0)
        {
          return string.Empty;
        }
        string strUserPassword = userPassword_;

        DatabaseUtility.RemoveInvalidChars(ref strUserPassword);

        SQLiteResultSet results;
        strSQL = String.Format("select * from scrobbleusers where idScrobbleUser = '{0}'", userID_);
        results = DirectExecute(strSQL);
        // user doesn't exist therefore no password to change
        if (results.Rows.Count == 0)
        {
          return string.Empty;
        }

        if (DatabaseUtility.Get(results, 0, "strPassword") == strUserPassword)
        {
          // password didn't change
          return userPassword_;
        }
          // set new password
        else
        {
          // if no password was given = fetch it
          if (userPassword_ == "")
          {
            return DatabaseUtility.Get(results, 0, "strPassword");
          }
          else
          {
            strSQL = String.Format("update scrobbleusers set strPassword='{0}' where idScrobbleUser like '{1}'",
                                   strUserPassword, userID_);
            DirectExecute(strSQL);
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
        {
          return -1;
        }
        if (userID_.Length == 0 || fieldName_.Length == 0)
        {
          return -1;
        }

        SQLiteResultSet results;

        strSQL =
          String.Format("select idScrobbleSettings, idScrobbleUser from scrobblesettings where idScrobbleUser = '{0}'",
                        userID_);
        results = DirectExecute(strSQL);
        currentSettingID = DatabaseUtility.Get(results, 0, "idScrobbleSettings");
        //Log.Info("MusicDatabase: updating settings with ID {0}", currentSettingID);

        // setting doesn't exist - add it
        if (results.Rows.Count == 0)
        {
          strSQL =
            String.Format(
              "insert into scrobblesettings (idScrobbleSettings, idScrobbleUser, " + fieldName_ +
              ") values ( NULL, '{0}', '{1}')", userID_, fieldValue_);
          DirectExecute(strSQL);
          Log.Info("MusicDatabase: added scrobblesetting {0} for userid {1}",
                   Convert.ToString(MusicDbClient.LastInsertID()), userID_);
          if (fieldValue_ > -1)
          {
            return Instance.DbConnection.LastInsertID();
          }
          else
          {
            return fieldValue_;
          }
        }
        else
        {
          strSQL = String.Format("select " + fieldName_ + " from scrobblesettings where idScrobbleSettings = '{0}'",
                                 currentSettingID);
          results = DirectExecute(strSQL);

          if (DatabaseUtility.GetAsInt(results, 0, fieldName_) == fieldValue_)
          {
            // setting didn't change
            return fieldValue_;
          }
            // set new value
          else
          {
            // if no value was given = fetch it
            if (fieldValue_ == -1)
            {
              return DatabaseUtility.GetAsInt(results, 0, fieldName_);
            }
            else
            {
              strSQL =
                String.Format(
                  "update scrobblesettings set " + fieldName_ + "='{0}' where idScrobbleSettings like '{1}'",
                  fieldValue_, currentSettingID);
              DirectExecute(strSQL);
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
      results = DirectExecute(strSQL);

      if (results.Rows.Count != 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
        {
          scrobbleUsers.Add(DatabaseUtility.Get(results, i, "strUsername"));
        }
      }
      // what else?

      return scrobbleUsers;
    }

    public bool DeleteScrobbleUser(string userName_)
    {
      if (string.IsNullOrEmpty(userName_))
      {
        return false;
      }

      string strSQL;
      int strUserID;
      try
      {
        string strUserName = userName_;
        DatabaseUtility.RemoveInvalidChars(ref strUserName);

        // get the UserID
        strUserID = AddScrobbleUser(strUserName);

        strSQL = String.Format("delete from scrobblesettings where idScrobbleUser = '{0}'", strUserID);
        SQLiteResultSet results = DirectExecute(strSQL);

        // setting removed now remove user
        strSQL = String.Format("delete from scrobbleusers where idScrobbleUser = '{0}'", strUserID);
        DirectExecute(strSQL);
        return true;
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