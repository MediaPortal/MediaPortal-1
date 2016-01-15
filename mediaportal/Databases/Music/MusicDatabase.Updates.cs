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
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TagReader;
using MediaPortal.Util;
using SQLite.NET;
using Log = MediaPortal.ServiceImplementations.Log;

namespace MediaPortal.Music.Database
{
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
    private List<string> _cueFiles;
    private Hashtable _allFiles;
    private int _processCount = 0;
    private int _songsSkipped = 0;
    private int _allFilesCount = 0;
    private int _songsProcessed = 0;
    private int _songsAdded = 0;
    private int _songsUpdated = 0;
    private bool _updateSinceLastImport = false;
    private bool _excludeHiddenFiles = false;
    private bool _singleFolderScan = false;
    private int _folderQueueLength = 0;

    private Thread _scanThread = null;
    private ManualResetEvent _scanSharesFinishedEvent = null;
    private ManualResetEvent _scanFoldersFinishedEvent = null;
    private DatabaseReorgEventArgs _myArgs = null;

    private readonly char[] trimChars = { ' ', '\x00', '|' };

    private readonly string[] _multipleValueFields = new string[] { "artist", "albumartist", "genre", "composer" };

    private int _songCount, _artistCount, _albumCount, _genreCount, _folderCount, _shareCount = 0; // Count for the various IDs

    private string _currentShare = null;

    // Objects to put a lock on the code during thread execution
    private readonly object _artistLock = new object();
    private readonly object _albumLock = new object();
    private readonly object _genreLock = new object();
    private readonly object _folderLock = new object();
    private readonly object _shareLock = new object();


    #endregion

    #region Resume

    /// <summary>
    /// Sets the position of a Song to resume on next Playback
    /// </summary>
    /// <param name="aSong"></param>
    public void SetResume(Song aSong)
    {
      if (aSong.Id == -1)
      {
        return;
      }

      string strSQL = String.Format("UPDATE Song SET ResumeAt={0} WHERE IdSong={1}", aSong.ResumeAt, aSong.Id);
      ExecuteNonQuery(strSQL);
    }

    #endregion

    #region Favorite / Ratings

    /// <summary>
    /// Mark Song as Favorite
    /// </summary>
    /// <param name="aSong"></param>
    public void SetFavorite(Song aSong)
    {
      if (aSong.Id == -1)
      {
        return;
      }

      var iFavorite = 0;
      if (aSong.Favorite)
      {
        iFavorite = 1;
      }
      string strSQL = String.Format("UPDATE Song SET Favorite={0} WHERE IdSong={1}", iFavorite, aSong.Id);
      ExecuteNonQuery(strSQL);
    }

    /// <summary>
    /// Set the Rating of a Song
    /// </summary>
    /// <param name="aFilename"></param>
    /// <param name="aRating"></param>
    public void SetRating(string aFilename, int aRating)
    {
      if (string.IsNullOrEmpty(aFilename))
      {
        return;
      }

      var strFileName = DatabaseUtility.RemoveInvalidChars(aFilename);

      // The underscore is treated as special symbol in a like clause, which produces wrong results
      // we need to escape it and use the sql escape clause  escape '\x0001'
      strFileName = strFileName.Replace("_", "\x0001_");
      strSQL = string.Format(@"Update Song Set Rating={1} " +
       "where IdSong = " +
       "(select s.idsong from Song s " +
       "join folder fo on fo.IdFolder = s.IdFolder " +
       "join share sh on sh.IdShare = fo.IDShare " +
       "where (sh.ShareName || fo.FolderName || s.FileName) like '{0}' escape '\x0001')", strFileName, aRating);

      ExecuteNonQuery(strSQL);

      // Let's fire the Ratings change event
      if (MusicRatingChanged != null)
      {
        MusicRatingChanged(this, strFileName, aRating);
      }
    }

    #endregion

    #region Top100

    /// <summary>
    /// Resets the Top 100 Counter
    /// </summary>
    public void ResetTop100()
    {
      strSQL = String.Format("UPDATE Song SET TimesPlayed=0");
      ExecuteNonQuery(strSQL);
    }

    public bool IncrTop100CounterByFileName(string aFileName)
    {
      var strFileName = DatabaseUtility.RemoveInvalidChars(aFileName);

      strSQL = string.Format("select s.idsong from Song s " +
       "join folder fo on fo.IdFolder = s.IdFolder " +
       "join share sh on sh.IdShare = fo.IDShare " +
       "where (sh.ShareName || fo.FolderName || s.FileName) like '{0}' escape '\x0001'", strFileName);

      var results = ExecuteQuery(strSQL);
      if (results.Rows.Count == 0)
      {
        return false;
      }

      var idSong = DatabaseUtility.GetAsInt(results, 0, "IdSong");
      var iTimesPlayed = DatabaseUtility.GetAsInt(results, 0, "TimesPlayed");

      // The underscore is treated as special symbol in a like clause, which produces wrong results
      // we need to escape it and use the sql escape clause  escape '\x0001'
      strFileName = strFileName.Replace("_", "\x0001_");
      strSQL = string.Format(@"Update Song Set TimesPlayed={1}, DateLastPlayed='{2}' " +
       "where IdSong = {0}", idSong, ++iTimesPlayed, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

      ExecuteNonQuery(strSQL);

      Log.Debug("MusicDatabase: increased playcount for song {1} to {0}", Convert.ToString(iTimesPlayed), aFileName);
      return true;
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
        ExecuteNonQuery(strSQL);

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

        ExecuteNonQuery(strSQL);

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
        ExecuteNonQuery(strSQL);

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

        ExecuteNonQuery(strSQL);
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
      ExecuteNonQuery(strSQL);
    }

    public void DeleteArtistInfo(string aArtistName)
    {
      string strArtist = aArtistName;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      strSQL = String.Format("delete from artistinfo where strArtist like '{0}'", strArtist);
      ExecuteNonQuery(strSQL);
    }

    #endregion

    #region Lyrics

    public bool UpdateLyrics(string aFileName, string aLyrics)
    {
      var strFileName = DatabaseUtility.RemoveInvalidChars(aFileName);
      var strLyrics = DatabaseUtility.RemoveInvalidChars(aLyrics);

      // The underscore is treated as special symbol in a like clause, which produces wrong results
      // we need to escape it and use the sql escape clause  escape '\x0001'
      strFileName = strFileName.Replace("_", "\x0001_");
      strSQL = string.Format(@"Update Song Set Lyrics='{1}' " +
       "where IdSong = " +
       "(select s.idsong from Song s " +
       "join folder fo on fo.IdFolder = s.IdFolder " +
       "join share sh on sh.IdShare = fo.IDShare " +
       "where (sh.ShareName || fo.FolderName || s.FileName) like '{0}' escape '\x0001')", strFileName, strLyrics);

      if (ExecuteNonQuery(strSQL))
      {
        Log.Debug("MusicDatabase: Updated Lyrics for song {0}", aFileName);
        return true;
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
      _myArgs = new DatabaseReorgEventArgs();

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
          Log.Info("Musicdatabasereorg: Last import at {0}", _lastImport.ToString());
        }

        BeginTransaction();

        // Before we begin with the scan we need to setup the Cache for Artist, Album, Genre, Shares and Folders
        // It is used to speed up the proces, so that we don't need to issue a query for every single file, if we already
        // have inserted the Artist, Folder, etc.
        _myArgs.progress = 2;
        _myArgs.phase = "Getting Max Values for Folders, Artists, Albums and Genres";
        OnDatabaseReorgChanged(_myArgs);
        Log.Info("Musicdatabasereorg: Getting Max Values for Folders, Artists, Albums and Genres");
        GetMaxValues();

        // Start Shares Scan in a separate Thread
        _myArgs.progress = 3;
        _myArgs.phase = "Scanning for music files";
        OnDatabaseReorgChanged(_myArgs);
        Log.Info("Musicdatabasereorg: Scanning for music files");

        // Start the Scanning and Update Thread
        _cueFiles = new List<string>(500);
        _allFiles = new Hashtable(100000);

        _scanSharesFinishedEvent = new ManualResetEvent(false);
        _scanThread = new Thread(ScanShares);
        _scanThread.Start();

        // Wait for the Scan Thread to finish, before continuing
        _scanSharesFinishedEvent.WaitOne();

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
          _myArgs.progress = 80;
          _myArgs.phase = "Removing non existing songs";
          OnDatabaseReorgChanged(_myArgs);
          DeleteNonExistingSongs();
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

        if (!_singleFolderScan)
        {
          _myArgs.progress = 95;
          _myArgs.phase = "Cleanup non-existing Artists, AlbumArtists and Genres";
          CleanupMultipleEntryTables();
          Log.Info("Musicdatabasereorg: Finished with cleaning up Mutiple Value Fields Tables.");
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
        _myArgs.progress = 96;
        _myArgs.phase = "Finishing";
        OnDatabaseReorgChanged(_myArgs);
        CommitTransaction();

        _myArgs.progress = 98;
        _myArgs.phase = "Compressing the database";
        OnDatabaseReorgChanged(_myArgs);
        Compress();

        _myArgs.progress = 100;
        _myArgs.phase = string.Format("Rescan completed. Total {0} Added {1} / Updated {2} / Skipped {3}", _processCount,
                                     _songsAdded, _songsUpdated, _songsSkipped);
        OnDatabaseReorgChanged(_myArgs);

        Log.Info("Musicdatabasereorg: Finished Reorganisation of the Database");

        // Save the time of the reorg, to be able to skip the files not updated / added the next time
        if (!_singleFolderScan)
        {
          ExecuteNonQuery(string.Format("update Configuration set Value='{0}' where Parameter = 'LastImport'", startTime.ToString("yyyy-M-d H:m:s", CultureInfo.InvariantCulture)));
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
        ExecuteNonQuery("vacuum");
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
            if (ShareType.ToLowerInvariant() == "yes")
            {
              Log.Info("Musicdatabasereorg: Skipping scan of Remote Share: {0}", SharePath);
            }
            else if (ShareScan.ToLowerInvariant() == "no")
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

    #endregion

    #region Delete Song

    /// <summary>
    /// Remove songs, which are not existing anymore, because they have been moved, deleted.
    /// </summary>
    /// <returns></returns>
    private int DeleteNonExistingSongs()
    {
      strSQL = @"select s.idsong, (sh.ShareName || fo.FolderName || s.FileName) as Path from Song s " +
               "join folder fo on fo.IdFolder = s.IdFolder " +
               "join share sh on sh.IdShare = fo.IDShare ";

      var results = ExecuteQuery(strSQL);
      if (results.Rows.Count == 0)
      {
        return (int)Errors.ERROR_REORG_SONGS;
      }

      var removed = 0;
      Log.Info("Musicdatabasereorg: starting song cleanup for {0} songs", results.Rows.Count);
      for (var i = 0; i < results.Rows.Count; ++i)
      {
        var strFileName = DatabaseUtility.Get(results, i, "Path");
        var idSong = DatabaseUtility.GetAsInt(results, i, "IdSong");

        if (!_allFiles.Contains(strFileName))
        {
          // song doesn't exist anymore, delete it
          // We don't care about foreign keys at this moment. We'll just change this later.
          removed++;
          DeleteSong(idSong, false);
        }
        if ((i % 10) == 0)
        {
          var MyArgs = new DatabaseReorgEventArgs();
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
    /// Delete a song from the database based on the Song ID
    /// </summary>
    /// <param name="idSong">The Id of the Song</param>
    /// <param name="bCheck">Check if we would have Artist, Genre, etc. without a Song</param>
    public void DeleteSong(int idSong, bool bCheck)
    {
      strSQL = string.Format("delete from Song where IdSong = {0}", idSong);
      ExecuteNonQuery(strSQL);

      DeleteSongRelations(idSong, bCheck);
    }

    /// <summary>
    /// Delete a song from the database using a file name
    /// </summary>
    /// <param name="strFileName">The Path of a Song</param>
    /// <param name="bCheck">Check if we would have Artist, Genre, etc. without a Song</param>
    public void DeleteSong(string strFileName, bool bCheck)
    {
      DatabaseUtility.RemoveInvalidChars(ref strFileName);

      // The underscore is treated as special symbol in a like clause, which produces wrong results
      // we need to escape it and use the sql escape clause  escape '\x0001'
      strFileName = strFileName.Replace("_", "\x0001_");
      strSQL = string.Format(@"select s.idsong, (sh.ShareName || fo.FolderName || s.FileName) as Path from Song s " +
       "join folder fo on fo.IdFolder = s.IdFolder " +
       "join share sh on sh.IdShare = fo.IDShare " +
       "where Path like '{0}' escape '\x0001'", strFileName);

      var results = ExecuteQuery(strSQL);
      if (results.Rows.Count > 0)
      {
        var idSong = DatabaseUtility.GetAsInt(results, 0, "IdSong");

        strSQL = string.Format("delete from Song where IdSong = {0}", idSong);
        ExecuteNonQuery(strSQL);

        DeleteSongRelations(idSong, bCheck);
      }
    }

    /// <summary>
    /// Deletes the Relations that a Song has to Artist, Genre, Composer, Conductor
    /// </summary>
    /// <param name="idSong"></param>
    /// <param name="bCheck"></param>
    private void DeleteSongRelations(int idSong, bool bCheck)
    {
      string[] tblPrefix = { "Artist", "Composer", "Conductor", "Genre" };

      foreach (var prefix in tblPrefix)
      {
        // Delete the Song from the Relations table
        strSQL = string.Format("delete from {0}Song where IdSong = {1}", prefix, idSong);
        ExecuteNonQuery(strSQL);

        if (bCheck)
        {
          CleanupMultipleEntryTables();
        }
      }
    }

    #endregion

    #region Scan Shares

    /// <summary>
    /// Scan the folders in the selected shares for music files to be added to the database
    /// </summary>
    private void ScanShares()
    {
      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      string strSQL;

      _processCount = 0;
      _songsAdded = 0;
      _songsUpdated = 0;
      _songsSkipped = 0;

      foreach (string share in _shares)
      {
        //dummy call to stop lots of watchers being created unnecessarily
        Util.Utils.FileExistsInCache(Path.Combine(share, "folder.jpg"));
        // Get all the files for the given Share / Path
        _currentShare = share;
        try
        {
          var maxThreads = 0;
          var maxComplThreads = 0;
          ThreadPool.GetAvailableThreads(out maxThreads, out maxComplThreads);

          ThreadPool.SetMaxThreads(Environment.ProcessorCount, maxComplThreads);
          _scanFoldersFinishedEvent = new ManualResetEvent(false);

          var di = new DirectoryInfo(share);
          foreach (var folder in GetFolders(di))
          {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessFolder), folder);
            _folderQueueLength++;
          }

          _scanFoldersFinishedEvent.WaitOne();
        }
        catch (Exception ex)
        {
          Log.Error("MusicDBReorg: Exception accessing file or folder: {0}", ex.Message);
        }
      }

      // Now we will remove the CUE data file from the database, since we will add Fake Tracks in the next step
      foreach (var cueFile in _cueFiles)
      {
        try
        {
          var cueSheet = new CueSheet(cueFile);
          string cuePath = Path.GetDirectoryName(cueFile);
          string cueDataFile = cuePath + "\\" + cueSheet.Tracks[0].DataFile.Filename;
          DatabaseUtility.RemoveInvalidChars(ref cueDataFile);
          strSQL = string.Format("delete from Song " +
                                 "where IdSong =  " +
                                    "(select s.idsong from Song s " +
                                    "join folder fo on fo.IdFolder = s.IdFolder " +
                                    "join share sh on sh.IdShare = fo.IDShare " +
                                    "where (sh.ShareName || fo.FolderName || s.FileName) like '{0}')"
                                    , cueDataFile);
          ExecuteNonQuery(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("Exception Processing CUE File: {0}: {1}", cueFile, ex.Message);
        }
      }

      try
      {
        // Apply CUE Filter
        var cueFileFakeTracks =
          (List<string>)CueUtil.CUEFileListFilterList<string>(_cueFiles, CueUtil.CUE_TRACK_FILE_STRING_BUILDER);

        // and add them also to the Hashtable, so that they don't get deleted in the next step
        foreach (string song in cueFileFakeTracks)
        {
          _processCount++;
          AddUpdateSong(song);
          _allFiles.Add(song, false);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Error processing CUE files: {0}", ex.Message);
      }

      _scanSharesFinishedEvent.Set();
    }

    /// <summary>
    /// Processes all Songs of a folder
    /// </summary>
    /// <param name="dirInfo"></param>
    private void ProcessFolder(object dirInfo)
    {
      var di = (DirectoryInfo)dirInfo;
      try
      {
        foreach (FileInformation file in GetFiles(di))
        {
          Interlocked.Increment(ref _allFilesCount);

          if (_allFilesCount % 1000 == 0)
          {
            Log.Info("MusicDBReorg: Procesing file {0}", _allFilesCount);
          }

          _myArgs.progress = 4;
          _myArgs.phase = String.Format("Processing file {0}", _allFilesCount);
          OnDatabaseReorgChanged(_myArgs);

          if (!CheckFileForInclusion(file))
          {
            continue;
          }

          Interlocked.Increment(ref _processCount);

          AddUpdateSong(file.Name);
        }
      }
      catch (Exception ex)
      {
        Log.Error("MusicDBReorg: Exception accessing file or folder: {0}", ex.Message);
      }

      // Check if we have different Artists and 
      if (_treatFolderAsAlbum)
      {
        UpdateVariousArtist(di);
      }

      // Create Folder Thumb if necessary
      if (_useFolderThumbs)
      {
        CreateFolderThumbs(di);
      }

      Interlocked.Decrement(ref _folderQueueLength);
      if (_folderQueueLength == 0)
      {
        _scanFoldersFinishedEvent.Set();
      }
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

      // Let's check, if we've got already that file in the database
      strSQL = string.Format(@"select s.idsong, (sh.ShareName || fo.FolderName || s.FileName) as Path from Song s " +
        "join folder fo on fo.IdFolder = s.IdFolder " +
        "join share sh on sh.IdShare = fo.IDShare " +
        "where Path  like '{0}'", strFileName);

      results = ExecuteQuery(strSQL);
      if (results.Rows.Count == 0)
      {
        //The song does not exist, we will add it.
        AddSong(song, 0);
        Interlocked.Increment(ref _songsAdded);
      }
      else
      {
        AddSong(song, Convert.ToInt32(results.Rows[0].fields[0]));
        Interlocked.Increment(ref _songsUpdated);
      }
    }

    /// <summary>
    /// Build an Iterator over the given Folder, returning all Foldres recursively
    /// </summary>
    /// <param name="dirInfo">DirectoryInfo for the directory we want files returned for</param>
    /// <returns>IENumerable</returns>
    /// 
    /// A much more elegant way would be using the method below, but it is not allowed to have a yield inside a try / catch,
    /// and we need to capture Access Exceptions to Directories and Files.
    /// The method used now has the same speed.
    /// 
    /// private IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo dirInfo)
    /// {
    ///  foreach (DirectoryInfo di in dirInfo.GetDirectories())
    ///  {
    ///    yield return di;
    ///  }
    /// }
    private IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo dirInfo)
    {
      Queue<DirectoryInfo> directories = new Queue<DirectoryInfo>();
      directories.Enqueue(dirInfo);
      while (directories.Count > 0)
      {
        DirectoryInfo dir = directories.Dequeue();
        yield return dir;

        try
        {
          foreach (DirectoryInfo di in dir.GetDirectories())
          {
            directories.Enqueue(di);
          }
        }
        catch (UnauthorizedAccessException ex)
        {
          Log.Error("Musicdatabasereorg: File / Directory Access error: {0}", ex.Message);
        }
      }
    }

    /// <summary>
    ///   Read a Folder and return the files
    /// </summary>
    /// <param name = "dirInfo">DirectoryInfo of the folder to process</param>
    private IEnumerable<FileInformation> GetFiles(DirectoryInfo dirInfo)
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
            FileInformation[] newFiles = MediaPortal.Util.NativeFileSystemOperations.GetFileInformation(dir.FullName,
                                                                                                        !_excludeHiddenFiles);

            foreach (FileInformation fi in newFiles)
            {
              files.Enqueue(fi);
            }
          }
        }
        catch (UnauthorizedAccessException ex)
        {
          Log.Error("MusicdatabaseReorg: File / Directory Access error: {0}", ex.Message);
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
        string ext = Path.GetExtension(file).ToLowerInvariant();
        if (ext == ".m3u" || ext == ".m3u8")
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

        // Only get files with the required extension
        if (_supportedExtensions.IndexOf(ext) == -1)
        {
          return false;
        }

        // Cue Files are being processed separately
        if (CueUtil.isCueFile(file))
        {
          _cueFiles.Add(file);
          return false;
        }

        // Add the files to the Hastable, which is used in the Delete Non-existing songs, to prevent file system access
        _allFiles.Add(file, false);

        // Only Add files to the list, if they have been Created / Updated after the Last Import date
        if (_updateSinceLastImport && !_singleFolderScan)
        {
          if (fileInfo.CreationTime > _lastImport || fileInfo.ModificationTime > _lastImport)
          {
            Interlocked.Increment(ref _songsProcessed);
            fileinCluded = true;
          }
          else
          {
            Interlocked.Increment(ref _songsSkipped);
          }
        }
        else
        {
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

        tag.Album = string.IsNullOrEmpty(tag.Album) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Album);
        tag.AlbumSort = string.IsNullOrEmpty(tag.AlbumSort) ? "" : DatabaseUtility.RemoveInvalidChars(tag.AlbumSort);
        tag.Genre = string.IsNullOrEmpty(tag.Genre) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Genre);
        tag.Artist = string.IsNullOrEmpty(tag.Artist) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Artist);
        tag.ArtistSort = string.IsNullOrEmpty(tag.ArtistSort) ? "" : DatabaseUtility.RemoveInvalidChars(tag.ArtistSort);
        tag.Title = string.IsNullOrEmpty(tag.Title) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Title);
        tag.TitleSort = string.IsNullOrEmpty(tag.TitleSort) ? "" : DatabaseUtility.RemoveInvalidChars(tag.TitleSort);
        tag.AlbumArtist = string.IsNullOrEmpty(tag.AlbumArtist) ? "" : DatabaseUtility.RemoveInvalidChars(tag.AlbumArtist);
        tag.AlbumArtistSort = string.IsNullOrEmpty(tag.AlbumArtistSort) ? "" : DatabaseUtility.RemoveInvalidChars(tag.AlbumArtistSort);
        tag.Lyrics = string.IsNullOrEmpty(tag.Lyrics) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Lyrics);
        tag.Composer = string.IsNullOrEmpty(tag.Composer) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Composer);
        tag.ComposerSort = string.IsNullOrEmpty(tag.ComposerSort) ? "" : DatabaseUtility.RemoveInvalidChars(tag.ComposerSort);
        tag.Conductor = string.IsNullOrEmpty(tag.Conductor) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Conductor);
        tag.Comment = string.IsNullOrEmpty(tag.Comment) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Comment);
        tag.Codec = string.IsNullOrEmpty(tag.Codec) ? "" : DatabaseUtility.RemoveInvalidChars(tag.Codec);

        if (!tag.HasAlbumArtist)
        {
          tag.AlbumArtist = tag.Artist;
        }

        if (_stripArtistPrefixes)
        {
          var strTmp = tag.Artist.Trim();
          Util.Utils.StripArtistNamePrefix(ref strTmp, true);
          tag.Artist = strTmp;

          strTmp = tag.AlbumArtist.Trim();
          Util.Utils.StripArtistNamePrefix(ref strTmp, true);
          tag.AlbumArtist = strTmp;

          strTmp = tag.Composer.Trim();
          Util.Utils.StripArtistNamePrefix(ref strTmp, true);
          tag.Composer = strTmp;

          strTmp = tag.Conductor.Trim();
          Util.Utils.StripArtistNamePrefix(ref strTmp, true);
          tag.Conductor = strTmp;
        }

        return tag;
      }
      return null;
    }

    /// <summary>
    /// Gets the Tag Value from the Tag, formatting it correctly for SQL Statements
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    private string GetTagValue(object tag)
    {
      if (tag is string)
      {
        string s = (string)tag;
        DatabaseUtility.RemoveInvalidChars(ref s);
        if (s == string.Empty || s == Strings.Unknown)
        {
          return "null";
        }
        return string.Format("'{0}'", s);
      }

      int i = (int)tag;
      if (i == 0)
      {
        return "null";
      }
      return string.Format("{0}", i);
    }


    /// <summary>
    /// Adds a Song to the Database
    /// </summary>
    /// <param name="strFileName">The filename to insert</param>
    /// <param name="aSongId">The song id, if song already exists in DB</param>
    /// <returns></returns>
    public int AddSong(string strFileName, int aSongId)
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

        // Add the Folder and Album to get the IDs for the Song insert 
        var folderId = AddFolder(Path.GetDirectoryName(strFileName));
        var albumId = AddAlbum(tag);

        // Song Id
        var songId = aSongId;
        if (songId == 0) // The song is not in the database
        {
          songId = Interlocked.Increment(ref _songCount);
        }
        var fileName = Path.GetFileName(strFileName);

        strSQL =
          string.Format(
            @"insert or replace into Song ( " +
                       "IdSong, IdFolder, IdAlbum, FileName, Title, TitleSort, Track, TrackCount, Disc, DiscCount," +
                       "Duration, Year, Timesplayed, Rating, Lyrics, Comment, Copyright," +
                       "AmazonId, Grouping, MusicBrainzArtistId, MusicBrainzDiscId, MusicBrainzReleaseArtistId," +
                       "MusicBrainzReleaseCountry, MusicBrainzReleaseId, MusicBrainzReleaseStatus, MusicBrainzReleaseTrackid," +
                       "MusicBrainzReleaseType, MusicIpId, ReplayGainTrack, ReplayGainTrackPeak, ReplayGainAlbum, ReplayGainAlbumPeak," +
                       "FileType, Codec, BitRateMode, BPM, Bitrate, Channels, Samplerate, DateLastPlayed, DateAdded, Favorite, ResumeAt) " +
                  "values ( " +
                       "{0}, {1}, {2}, '{3}', {4}, {5}, {6}, {7}, {8}, {9}," +
                       "{10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}," +
                       "{19}, {20}, {21}, {22}, {23}," +
                       "{24}, {25}, {26}, {27}," +
                       "{28}, {29}, {30}, {31}, {32}, {33}," +
                       "{34}, {35}, {36}, {37}, {38}, '{39}', '{40}', null, null)",
                  songId, folderId, albumId, fileName, GetTagValue(tag.Title), GetTagValue(tag.TitleSort), GetTagValue(tag.Track),
                  GetTagValue(tag.TrackTotal), GetTagValue(tag.DiscID), GetTagValue(tag.DiscTotal), GetTagValue(tag.Duration),
                  GetTagValue(tag.Year), GetTagValue(tag.TimesPlayed), GetTagValue(tag.Rating), GetTagValue(tag.Lyrics),
                  GetTagValue(tag.Comment), GetTagValue(tag.Copyright), GetTagValue(tag.AmazonId), GetTagValue(tag.Grouping),
                  GetTagValue(tag.MusicBrainzArtistId), GetTagValue(tag.MusicBrainzDiscId), GetTagValue(tag.MusicBrainzReleaseArtistId),
                  GetTagValue(tag.MusicBrainzReleaseCountry), GetTagValue(tag.MusicBrainzReleaseId), GetTagValue(tag.MusicBrainzReleaseStatus),
                  GetTagValue(tag.MusicBrainzReleaseTrackId), GetTagValue(tag.MusicBrainzReleaseType), GetTagValue(tag.MusicIpid),
                  GetTagValue(tag.ReplayGainTrack), GetTagValue(tag.ReplayGainTrackPeak), GetTagValue(tag.ReplayGainAlbum),
                  GetTagValue(tag.ReplayGainAlbumPeak), GetTagValue(tag.FileType), GetTagValue(tag.Codec), GetTagValue(tag.BitRateMode),
                  GetTagValue(tag.BPM), GetTagValue(tag.BitRate), GetTagValue(tag.Channels), GetTagValue(tag.SampleRate),
                  DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"), dateadded.ToString("yyyy-MM-dd HH:mm:ss")
                  );

        try
        {
          if (!ExecuteNonQuery(strSQL))
          {
            Log.Info("Insert of song {0} failed", strFileName);
            return (int)Errors.ERROR_REORG_SONGS;
          }

          // Now add the Multiple Value Fields to their tables
          string[] splittedValues = tag.Artist.Split(';');
          foreach (var artistName in splittedValues)
          {
            int artistId = AddArtist(artistName);
            strSQL = string.Format("insert or ignore into ArtistSong values ({0}, {1})", artistId, songId);
            ExecuteNonQuery(strSQL);
          }

          splittedValues = tag.AlbumArtist.Split(';');
          foreach (var albumArtistName in splittedValues)
          {
            int artistId = AddArtist(albumArtistName);
            strSQL = string.Format("insert or ignore into AlbumArtist values ({0}, {1})", artistId, albumId);
            ExecuteNonQuery(strSQL);
          }

          splittedValues = tag.Genre.Split(';');
          foreach (var genreName in splittedValues)
          {
            int genreId = AddGenre(genreName);
            strSQL = string.Format("insert or ignore into genresong values ({0}, {1})", genreId, songId);
            ExecuteNonQuery(strSQL);
          }

          splittedValues = tag.Composer.Split(';');
          foreach (var composerName in splittedValues)
          {
            int artistId = AddArtist(composerName);
            strSQL = string.Format("insert or ignore into composersong values ({0}, {1})", artistId, songId);
            ExecuteNonQuery(strSQL);
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

    /// <summary>
    /// Adds a folder to the Cache and to the Databasem, if not found in Cache
    /// The Share is stripped from the folder and added to the Share Cache / Database
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    private int AddFolder(string fullPath)
    {
      lock (_folderLock)
      {
        var folder = fullPath.Replace(_currentShare, "").Trim('\\');
        if (folder.Length > 0)
        {
          folder = folder + "\\"; // Append backslash at the end
        }
        var shareId = AddShare(_currentShare);
        var sql = string.Format("select IdFolder from folder where FolderName = '{0}' and IdShare = {1}", folder, shareId);
        var result = ExecuteQuery(sql);
        if (result.Rows.Count == 0)
        {
          var folderId = ++_folderCount;
          sql = string.Format("insert into folder values ({0}, '{1}', '{2}')", folderId, shareId, folder);
          if (ExecuteNonQuery(sql))
          {
            return folderId;
          }
        }
        else
        {
          return Convert.ToInt32(result.Rows[0].fields[0]);
        }
        return 0;
      }
    }

    /// <summary>
    /// Adds a Share to the Cache and to the database, if it doesn't exist in Cache
    /// </summary>
    /// <param name="shareName"></param>
    /// <returns></returns>
    private int AddShare(string shareName)
    {
      lock (_shareLock)
      {
        var sql = string.Format(@"select IdShare from Share where ShareName = '{0}\'", shareName);
        var result = ExecuteQuery(sql);
        if (result.Rows.Count == 0)
        {
          var shareId = ++_shareCount;
          sql = string.Format(@"insert into Share values ({0}, '{1}\')", shareId, shareName);
          if (ExecuteNonQuery(sql))
          {
            return shareId;
          }
        }
        else
        {
          return Convert.ToInt32(result.Rows[0].fields[0]);
        }
        return 0;
      }
    }

    /// <summary>
    /// Adds an Album to the Cache and to the Database
    /// </summary>
    /// <param name="albumName"></param>
    /// <returns></returns>
    private int AddAlbum(MusicTag tag)
    {
      lock (_albumLock)
      {
        var sql = string.Format("select IdAlbum from Album where AlbumName = '{0}'", tag.Album);
        var result = ExecuteQuery(sql);
        if (result.Rows.Count == 0)
        {
          var albumId = ++_albumCount;
          sql = string.Format("insert into album values ({0}, '{1}', '{2}', {3})", albumId, tag.Album,
                                     tag.AlbumSort, GetTagValue(tag.Year));
          if (ExecuteNonQuery(sql))
          {
            return albumId;
          }
        }
        else
        {
          return Convert.ToInt32(result.Rows[0].fields[0]);
        }
        return 0;
      }
    }

    /// <summary>
    /// Adds an Artist, AlbumArtist, Compoer, Conductor to the Artist Cache and to the Database
    /// if it doesn't exist in Cache
    /// </summary>
    /// <param name="artistName"></param>
    /// <returns></returns>
    private int AddArtist(string artistName)
    {
      lock (_artistLock)
      {
        var sql = string.Format("select IdArtist from Artist where ArtistName = '{0}'", artistName);
        var result = ExecuteQuery(sql);
        if (result.Rows.Count == 0)
        {
          var artistId = ++_artistCount;
          sql = string.Format("insert into artist values ({0}, '{1}', '{2}')", artistId, artistName, "");
          if (ExecuteNonQuery(sql))
          {
            return artistId;
          }
        }
        else
        {
          return Convert.ToInt32(result.Rows[0].fields[0]);
        }
        return 0;
      }
    }

    /// <summary>
    /// Adds a Genre to the Cache and if it doesn't exist to the Database
    /// </summary>
    /// <param name="genreName"></param>
    /// <returns></returns>
    private int AddGenre(string genreName)
    {
      lock (_genreLock)
      {
        var sql = string.Format("select IdGenre from Genre where GenreName = '{0}'", genreName);
        var result = ExecuteQuery(sql);
        if (result.Rows.Count == 0)
        {
          var genreId = ++_genreCount;
          sql = string.Format("insert into genre values ({0}, '{1}')", genreId, genreName);
          if (ExecuteNonQuery(sql))
          {
            return genreId;
          }
        }
        else
        {
          return Convert.ToInt32(result.Rows[0].fields[0]);
        }
        return 0;
      }
    }


    /// <summary>
    /// If the "Treat all songs in a folder as album" has been set, we check all the Songs in the folder
    /// if they have different artists AND NO Album artist set.
    /// In this case we will assign them the localised version of "Various Artist" as Album Artist.
    /// </summary>
    /// <param name="di"></param>
    private void UpdateVariousArtist(DirectoryInfo di)
    {
      var songs = new List<SongMap>();
      GetSongsByPath(di.FullName, ref songs);

      var needVariousArtist = false;
      var previousArtist = "";
      if (songs.Count > 0)
      {
        previousArtist = songs[0].m_song.AlbumArtist;
      }

      foreach (var map in songs)
      {
        if (previousArtist != map.m_song.AlbumArtist)
        {
          needVariousArtist = true;
          break;
        }
        previousArtist = map.m_song.Artist;
      }

      if (needVariousArtist)
      {
        var variousArtistId = AddArtist(GUILocalizeStrings.Get(340));
        foreach (var map in songs)
        {
          strSQL = string.Format("Delete from AlbumArtist where IdAlbum = {0}", map.m_song.AlbumId);
          ExecuteNonQuery(strSQL);
          strSQL = string.Format("insert or ignore into AlbumArtist values ({0}, {1})", variousArtistId, map.m_song.AlbumId);
          ExecuteNonQuery(strSQL);
        }
      }
    }
    #endregion

    #region Thumbs Handling

    private void ExtractCoverArt(MusicTag tag)
    {
      string formattedAlbum = tag.Album.Trim(trimChars);

      // Mantis 3078: Filename for Multiple Artists should not contain a semicolon, cause it wouldn't be found later on
      int i = 0;
      string formattedArtist = "";
      string[] strArtistSplit = tag.Artist.Split(new char[] { ';', '|' });
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
      if (_extractEmbededCoverArt && tag.CoverArtImageBytes != null)
      {
        try
        {
          bool extractFile = !Util.Utils.FileExistsInCache(smallThumbPath);

          if (extractFile)
          {
            try
            {
              string mp3TagImage = tag.CoverArtFile;

              if (!String.IsNullOrEmpty(mp3TagImage))
              {
                Util.Picture.CreateThumbnail(mp3TagImage, smallThumbPath, (int)Thumbs.ThumbResolution,
                                             (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall, false, true);

                Util.Picture.CreateThumbnail(mp3TagImage, largeThumbPath, (int)Thumbs.ThumbLargeResolution,
                                             (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge, false, true);

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
        catch (Exception) { }
      }

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
            if (!string.IsNullOrEmpty(sharefolderThumb))
            {
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
            Util.Picture.CreateThumbnail(sharefolderThumb, smallThumbPath, (int)Thumbs.ThumbResolution,
                                         (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall, false, false);

            Util.Picture.CreateThumbnail(sharefolderThumb, largeThumbPath, (int)Thumbs.ThumbLargeResolution,
                                         (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge, false, false);
          }
        }
      }

      // MP has an album cover in the thumb cache (maybe downloaded from last.fm) but no folder.jpg in the shares - create it
      if (_createMissingFolderThumbs)
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
            Util.Picture.CreateThumbnail(sourceCover, sharefolderThumb, (int)Thumbs.ThumbLargeResolution,
                                         (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge, false, false);
          }
        }
      }
    }

    private void CreateFolderThumbs(DirectoryInfo di)
    {
      try
      {
        bool foundCover = false;
        string sharefolderThumb;
        // We add the slash to be able to use TryEverythingToGetFolderThumbByFilename and GetLocalFolderThumb
        string currentPath = string.Format("{0}\\", di.FullName);
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
          Util.Picture.CreateThumbnail(sharefolderThumb, localFolderThumb, (int)Thumbs.ThumbResolution,
                                       (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall);
          
          Util.Picture.CreateThumbnail(sharefolderThumb, localFolderLThumb, (int)Thumbs.ThumbLargeResolution,
                                       (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
        }
      }
      catch (Exception ex2)
      {
        Log.Error("MusicDatabase: Error caching folder thumb of {0} - {1}", di.FullName, ex2.Message);
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

    #endregion

    #region Max Values

    /// <summary>
    /// Set the starting values for the various Id
    /// </summary>
    private void GetMaxValues()
    {
      _shareCount = GetMaxValue("Share");
      _folderCount = GetMaxValue("Folder");
      _artistCount = GetMaxValue("Artist");
      _albumCount = GetMaxValue("Album");
      _genreCount = GetMaxValue("Genre");
    }

    /// <summary>
    /// Fills the Cache from the various SQL tables
    /// </summary>
    /// <param name="aTable">Table Name</param>
    /// <returns>Max Value found for ID</returns>
    private int GetMaxValue(string aTable)
    {
      var max = 0;
      strSQL = string.Format("select max(Id{0}) from {0}", aTable);
      var maxValue = ExecuteQuery(strSQL);
      if (maxValue.Rows.Count > 0)
      {
        max = Int32.Parse(maxValue.Rows[0].fields[0]);
      }
      return max;
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
      // Cleanup Artist Table, which contains Artists, Composers and Conductors
      strSQL = "delete from Artist where IdArtist not in " + 
               "(select distinct IdArtist from ArtistSong " + 
               " union " + 
               "select distinct IdComposer from ComposerSong " +
               " union " +
               "select distinct IdConductor from ConductorSong " +
               ")";
      ExecuteNonQuery(strSQL);


      // Cleanup Genre Table
      strSQL = "delete from Genre where IdGenre not in (select distinct IdGenre from GenreSong)";
      ExecuteNonQuery(strSQL);

      // Cleanup Album Table
      strSQL = "delete from Album where IdAlbum not in (select distinct IdAlbum from Song)";
      ExecuteNonQuery(strSQL);

      // Cleanup AlbumArtist Table
      strSQL = "delete from AlbumArtist where IdAlbum not in (select distinct IdAlbum from Album)";
      ExecuteNonQuery(strSQL);
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

    #region last.fm

    public bool AddLastFMUser(string userName, string lastFmKey)
    {
      if (string.IsNullOrEmpty(lastFmKey)) return false;

      try
      {
        strSQL = @"select * from lastfmusers";
        var results = DirectExecute(strSQL);
        if(results.Rows.Count == 0)
        {
          strSQL = String.Format("insert into lastfmusers (idLastFMUser , strUsername, strSK) values ( NULL, '{0}', '{1}' )",
                                 userName, lastFmKey);
          DirectExecute(strSQL);
          Log.Info("LastFM Key added to database");
        }
        else
        {
          strSQL = String.Format("update lastfmusers set strUsername = '{0}', strSK = '{1}'", userName, lastFmKey);
          DirectExecute(strSQL);
          Log.Info("LastFM Key updated in database");
        }

      }
      catch (Exception e)
      {
        Log.Error("Unable to add last.fm key to database");
        Log.Error(e);
        return false;
      }

      return true;
    }

    #endregion
  }
}