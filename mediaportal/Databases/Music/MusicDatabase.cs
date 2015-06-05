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
using System.Globalization;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Profile;
using MediaPortal.ServiceImplementations;
using SQLite.NET;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Singleton class which establishes a database connection and provides lookup methods
  /// </summary>
  public partial class MusicDatabase
  {
    #region Variables

    public static readonly MusicDatabase Instance = new MusicDatabase();

    private SQLiteClient MusicDbClient = null;

    private static bool _treatFolderAsAlbum;
    private static bool _extractEmbededCoverArt;
    private static bool _useFolderThumbs;
    private static bool _useAllImages;
    private static bool _createArtistPreviews;
    private static bool _createGenrePreviews;
    private static bool _createMissingFolderThumbs;
    private static string _supportedExtensions;
    private static bool _stripArtistPrefixes;
    private static DateTime _lastImport;
    private static DateTime _currentDate = DateTime.Now;
    private static int _dateAddedValue;
    private bool _dbHealth = false;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// static constructor. Opens or creates the music database
    /// </summary>
    static MusicDatabase()
    {
      // Log.Debug("MusicDatabase: static database constructor");
    }

    /// <summary>
    /// private constructor to prevent any instance of this class
    /// </summary>
    private MusicDatabase()
    {
      // Set default values 
      _treatFolderAsAlbum = false;
      _extractEmbededCoverArt = true;
      _useFolderThumbs = true;
      _useAllImages = true;
      _createArtistPreviews = false;
      _createGenrePreviews = true;
      _createMissingFolderThumbs = false;
      _supportedExtensions = Util.Utils.AudioExtensionsDefault;
      _stripArtistPrefixes = false;
      _currentDate = DateTime.Now;
      _dateAddedValue = 0;

      LoadDBSettings();
      Open();

      // Create Temp Folder, which we can use for all purposes. e.g. Storing temporary folder thumbs
      var tmpFolder = Path.Combine(Path.GetTempPath(), "TeamMediaPortal");
      if (!Directory.Exists(tmpFolder))
      {
        Directory.CreateDirectory(tmpFolder);
      }
    }

    ~MusicDatabase()
    {
      // Cleanup Temp folder
      var tmpFolder = Path.Combine(Path.GetTempPath(), "TeamMediaPortal");
      if (Directory.Exists(tmpFolder))
      {
        foreach (var file in Directory.GetFiles(tmpFolder))
        {
          try
          {
            File.Delete(file);
          }
          catch (IOException)
          {
            // Don't need to report anything, if we couldn't delete a temp file
          }
        }
      }
    }

    public static void ReOpen()
    {
      Dispose();
      Instance.Open();
    }

    public static void Dispose()
    {
      if (Instance.MusicDbClient != null)
      {
        Instance.MusicDbClient.Dispose();
      }
      Instance.MusicDbClient = null;
    }

    #endregion

    #region Getters & Setters

    private SQLiteClient DbConnection
    {
      get
      {
        if (MusicDbClient == null)
        {
          MusicDbClient = new SQLiteClient(Config.GetFile(Config.Dir.Database, "MusicDatabaseV13.db3"));
        }

        return MusicDbClient;
      }
    }

    public string DatabaseName
    {
      get { return DbConnection.DatabaseName; }
    }

    #endregion

    #region Functions

    public static SQLiteResultSet DirectExecute(string aSQL)
    {
      return Instance.DbConnection.Execute(aSQL);
    }

    private void LoadDBSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _treatFolderAsAlbum = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);
        _extractEmbededCoverArt = xmlreader.GetValueAsBool("musicfiles", "extractthumbs", true);
        _useFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        _useAllImages = xmlreader.GetValueAsBool("musicfiles", "useAllImages", _useFolderThumbs);
        _createMissingFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs",
                                                              _treatFolderAsAlbum);
        _createArtistPreviews = xmlreader.GetValueAsBool("musicfiles", "createartistthumbs", false);
        _createGenrePreviews = xmlreader.GetValueAsBool("musicfiles", "creategenrethumbs", true);
        _supportedExtensions = xmlreader.GetValueAsString("music", "extensions", Util.Utils.AudioExtensionsDefault);
        _stripArtistPrefixes = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
        _dateAddedValue = xmlreader.GetValueAsInt("musicfiles", "dateadded", 0);
        _updateSinceLastImport = xmlreader.GetValueAsBool("musicfiles", "updateSinceLastImport", false);

        try
        {
          string lastImport = xmlreader.GetValueAsString("musicfiles", "lastImport", "1900-01-01 00:00:00");
          _lastImport = DateTime.ParseExact(lastImport, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
          _lastImport = DateTime.ParseExact("1900-01-01 00:00:00", "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
          ;
        }
      }
    }

    private void Open()
    {
      Log.Info("MusicDatabase: Opening database");

      try
      {
        // Open database
        try
        {
          Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception) {}

        if (!File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV13.db3")))
        {
          if (File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV11.db3")))
          {
            Log.Info("MusicDatabase: Found older version of database. Upgrade to new layout.");
            File.Copy(Config.GetFile(Config.Dir.Database, "MusicDatabaseV11.db3"),
                      Config.GetFile(Config.Dir.Database, "MusicDatabaseV13.db3"));

            // Get the DB handle or create it if necessary
            MusicDbClient = DbConnection;

            UpgradeDBV11_V13();

            return;
          }
          if (File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV12.db3")))
          {
            // upgrade DB (add last fm user table)
            File.Copy(Config.GetFile(Config.Dir.Database, "MusicDatabaseV12.db3"),
                      Config.GetFile(Config.Dir.Database, "MusicDatabaseV13.db3"));

            // Get the DB handle or create it if necessary
            MusicDbClient = DbConnection;

            if (!CreateDatabase())
            {
              Log.Error("MusicDatabase: Error creating new database. aborting upgrade}");
            }

            return;
          }

          // Get the DB handle or create it if necessary
          MusicDbClient = DbConnection;

          // When we have deleted the database, we need to scan from the beginning, regardsless of the last import setting
          _lastImport = DateTime.ParseExact("1900-01-01 00:00:00", "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);

          Log.Info("MusicDatabase: Database does not exist. Create it.");
          if (!CreateDatabase())
          {
            return;
          }
        }

        // Get the DB handle or create it if necessary
        MusicDbClient = DbConnection;

        _dbHealth = DatabaseUtility.IntegrityCheck(MusicDbClient);
      }

      catch (Exception ex)
      {
        Log.Error("MusicDatabase: exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      Log.Info("MusicDatabase: Database opened");
    }

    private void UpgradeDBV11_V13()
    {
      try
      {
        // First rename the tracks table
        string strSQL = "alter table tracks rename to tracksV11";
        MusicDbClient.Execute(strSQL);

        // Now call the Create Datbase function to create the new table
        if (!CreateDatabase())
        {
          Log.Error("MusicDatabase: Error creating new database. aborting upgrade}");
          return;
        }

        // Now copy the content of the old V11 tracks table to the new V12 tracks table
        strSQL =
          "insert into tracks select idTrack, strPath, strArtist, strAlbumArtist, strAlbum, strGenre, strComposer, strConductor, " +
          "strTitle, iTRack, iNumTracks, iDuration, iYear, iTimesPlayed, iRating, iFavorite, iResumeAt, iDisc, iNumDisc, " +
          "strLyrics, '', '', '', '', 0, 0, 0, 0, dateLastPlayed, dateAdded from tracksV11";

        MusicDbClient.Execute(strSQL);

        strSQL = "drop table tracksV11";
        MusicDbClient.Execute(strSQL);

        Log.Info("MusicDatabase: Finished upgrading database.");
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: exception while renaming table:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private bool CreateDatabase()
    {
      try
      {
        DatabaseUtility.SetPragmas(MusicDbClient);

        // Tracks table containing information for songs
        DatabaseUtility.AddTable(
          MusicDbClient, "tracks",
          @"CREATE TABLE tracks ( " +
          "idTrack integer primary key autoincrement, " + // Unique id Autoincremented
          "strPath text, " + // Full  path of the file.
          "strArtist text, " + // Artist
          "strAlbumArtist text, " + // Album Artist
          "strAlbum text, " + // Album
          "strGenre text, " + // Genre  (multiple genres)
          "strComposer text, " + // Composer (multiple composers)
          "strConductor text, " + // Conductor
          "strTitle text, " + // Song Title
          "iTrack integer, " + // Track Number
          "iNumTracks integer, " + // Total  Number of Tracks on Album
          "iDuration integer, " + // Duration in seconds
          "iYear integer, " + // Year
          "iTimesPlayed integer, " + // # Times Played
          "iRating integer, " + // Rating
          "iFavorite integer, " + // Favorite Indicator
          "iResumeAt integer, " + // Resume  song from position
          "iDisc integer, " + // Disc Number
          "iNumDisc integer, " + // Total  Number of Discs
          "strLyrics text, " + // Lyric Text
          "strComment text, " + // Comment
          "strFileType text, " + // File Format (mp3, flac, etc.)           
          "strFullCodec text, " + // Full Codec Description      
          "strBitRateMode text, " + // Bitrate mode (CBR / VBR)           
          "iBPM integer, " + // Beats per Minute
          "iBitRate integer, " + // Bitrate
          "iChannels integer, " + // Channels
          "iSampleRate integer, " + // Sample Rate    
          "dateLastPlayed timestamp, " + // Date, Last Time Played
          "dateAdded timestamp" + // Date added. Either Insertion date, Creation date, LastWrite
          ")"
          );

        MusicDbClient.Execute(strSQL);

        // Indices for Tracks table
        DatabaseUtility.AddIndex(MusicDbClient, "idxpath_strPath", "CREATE INDEX idxpath_strPath ON tracks(strPath ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartist_strArtist",
                                 "CREATE INDEX idxartist_strArtist ON tracks(strArtist ASC)");
        //DatabaseUtility.AddIndex(MusicDbClient, "idxartist_strArtistSortName", "CREATE INDEX idxartist_strArtistSortName ON tracks(strArtistSortName ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbum_strAlbumArtist",
                                 "CREATE INDEX idxalbum_strAlbumArtist ON tracks(strAlbumArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbum_strAlbum",
                                 "CREATE INDEX idxalbum_strAlbum ON tracks(strAlbum ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxgenre_strGenre",
                                 "CREATE INDEX idxgenre_strGenre ON tracks(strGenre ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxcomposer_strComposer",
                                 "CREATE INDEX idxcomposer_strComposer ON tracks(strComposer ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxconductor_strConductor",
                                 "CREATE INDEX idxconductor_strConductor ON tracks(strConductor ASC)");

        // Artist 
        DatabaseUtility.AddTable(MusicDbClient, "artist",
                                 "CREATE TABLE artist ( idArtist integer primary key autoincrement, strArtist text)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartisttable_strArtist",
                                 "CREATE INDEX idxartisttable_strArtist ON artist(strArtist ASC)");

        // AlbumArtist 
        DatabaseUtility.AddTable(MusicDbClient, "albumartist",
                                 "CREATE TABLE albumartist ( idAlbumArtist integer primary key autoincrement, strAlbumArtist text)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbumartisttable_strAlbumArtist",
                                 "CREATE INDEX idxalbumartisttable_strAlbumArtist ON albumartist(strAlbumArtist ASC)");

        // Genre
        DatabaseUtility.AddTable(MusicDbClient, "genre",
                                 "CREATE TABLE genre ( idGenre integer primary key autoincrement, strGenre text)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxgenretable_strGenre",
                                 "CREATE INDEX idxgenretable_strGenre ON genre(strGenre ASC)");

        // Composer
        DatabaseUtility.AddTable(MusicDbClient, "composer",
                                 "CREATE TABLE composer ( idComposer integer primary key autoincrement, strComposer text)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxcomposertable_strComposer",
                                 "CREATE INDEX idxcomposertable_strComposer ON composer(strComposer ASC)");

        // Artist Info and Album Info
        DatabaseUtility.AddTable(MusicDbClient, "albuminfo",
                                 "CREATE TABLE albuminfo ( idAlbumInfo integer primary key autoincrement, strAlbum text, strArtist text, strAlbumArtist text,iYear integer, idGenre integer, strTones text, strStyles text, strReview text, strImage text, strTracks text, iRating integer)");
        DatabaseUtility.AddTable(MusicDbClient, "artistinfo",
                                 "CREATE TABLE artistinfo ( idArtistInfo integer primary key autoincrement, strArtist text, strBorn text, strYearsActive text, strGenres text, strTones text, strStyles text, strInstruments text, strImage text, strAMGBio text, strAlbums text, strCompilations text, strSingles text, strMisc text)");

        // Indices for Album and Artist Info
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_strAlbum",
                                 "CREATE INDEX idxalbuminfo_strAlbum ON albuminfo(strAlbum ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_strArtist",
                                 "CREATE INDEX idxalbuminfo_strArtist ON albuminfo(strArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_idGenre",
                                 "CREATE INDEX idxalbuminfo_idGenre ON albuminfo(idGenre ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartistinfo_strArtist",
                                 "CREATE INDEX idxartistinfo_strArtist ON artistinfo(strArtist ASC)");
        
        // last.fm users
        DatabaseUtility.AddTable(MusicDbClient, "lastfmusers",
                         "CREATE TABLE lastfmusers ( idLastFMUser integer primary key, strUsername text, strSK text)");

        Log.Info("MusicDatabase: New Database created successfully");
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Create of database failed. Err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return false;
    }

    #endregion

    public bool DbHealth
    {
      get
      {
        return _dbHealth;
      }
    }

    #region Transactions

    public void BeginTransaction()
    {
      try
      {
        DirectExecute("begin");
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: BeginTransaction: musicdatabase begin transaction failed exception err:{0} ",
                  ex.Message);
        //Open();
      }
    }

    public void CommitTransaction()
    {
      Log.Debug("MusicDatabase: Commit will effect {0} rows", Instance.DbConnection.ChangedRows());
      SQLiteResultSet CommitResults;
      try
      {
        CommitResults = DirectExecute("commit");
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Commit failed exception err:{0} ", ex.Message);
        Open();
      }
    }

    public void RollbackTransaction()
    {
      Log.Debug("MusicDatabase: Rolling back transactions due to unrecoverable error. Effecting {0} rows",
                Instance.DbConnection.ChangedRows());
      try
      {
        DirectExecute("rollback");
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Rollback failed exception err:{0} ", ex.Message);
        Open();
      }
    }

    #endregion
  }
}