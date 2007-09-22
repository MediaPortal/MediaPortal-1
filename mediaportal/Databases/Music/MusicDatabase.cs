#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

using SQLite.NET;

using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.ServiceImplementations;
using MediaPortal.Util;


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

    private static bool _treatFolderAsAlbum = false;
    private static bool _scanForVariousArtists = true;
    private static bool _extractEmbededCoverArt = true;
    private static bool _useFolderThumbs = true;
    private static bool _useFolderArtForArtistGenre = false;
    private static bool _createMissingFolderThumbs = false;
    private static string _supportedExtensions = ".mp3,.wma,.ogg,.flac,.wav,.cda,.m3u,.pls,.b4s,.m4a,.m4p,.mp4,.wpl,.wv,.ape,.mpc";

    private static DateTime _lastImport = DateTime.ParseExact("1900-01-01 00:00:00", "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture);
    private static System.DateTime _currentDate = DateTime.Now;

    string[] ArtistNamePrefixes = new string[]
            {
              "the",
              "les"
            };

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
      // Log.Debug("MusicDatabase: private database constructor");
      LoadDBSettings();
      Open();
    }

    ~MusicDatabase()
    {
      Log.Debug("MusicDatabase: Disposing database");
      //if (MusicDB != null)
      //  MusicDB.Close();
    }
    #endregion

    #region Getters & Setters
    private SQLiteClient DbConnection
    {
      get 
      {
        if (MusicDbClient == null)
          MusicDbClient = new SQLiteClient(Config.GetFile(Config.Dir.Database, "MusicDatabaseV9.db3"));
        
        return MusicDbClient; 
      }
    }
    #endregion

    #region Functions
    public static SQLiteResultSet DirectExecute(string aSQL)
    {
      return Instance.DbConnection.Execute(aSQL);
    }

    private void LoadDBSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _treatFolderAsAlbum = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);
        _scanForVariousArtists = xmlreader.GetValueAsBool("musicfiles", "scanForVariousArtists", true);
        _extractEmbededCoverArt = xmlreader.GetValueAsBool("musicfiles", "extractthumbs", true);
        _useFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        _createMissingFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs", false);
        _useFolderArtForArtistGenre = xmlreader.GetValueAsBool("musicfiles", "createartistgenrethumbs", false);
        _supportedExtensions = xmlreader.GetValueAsString("music", "extensions", ".mp3,.wma,.ogg,.flac,.wav,.cda,.m3u,.pls,.b4s,.m4a,.m4p,.mp4,.wpl,.wv,.ape,.mpc");
        
        try
        {
          string lastImport = xmlreader.GetValueAsString("musicfiles", "lastImport", "1900-01-01 00:00:00");
          _lastImport = DateTime.ParseExact(lastImport, "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
          _lastImport = DateTime.ParseExact("1900-01-01 00:00:00", "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture); ;
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
          System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception) { }

        // no database V9 - copy and update V8
        if (!File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV9.db3")))
        {
          if (File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV8.db3")))
          {
            File.Copy((Config.GetFile(Config.Dir.Database, "MusicDatabaseV8.db3")), (Config.GetFile(Config.Dir.Database, "MusicDatabaseV9.db3")), false);
            if (UpdateDB_V8_to_V9())
            {
              Log.Info("MusicDatabaseV9: old V8 database successfully updated");
            }
            else
            {
              Log.Error("MusicDatabaseV8: error while trying to update your database to V9");
              // Remove the invalid V9 database
              File.Delete(Config.GetFile(Config.Dir.Database, "MusicDatabaseV9.db3"));
            }
          }
        }


        // Get the DB handle or create it if necessary
        MusicDbClient = DbConnection;


        // set connection params
        DatabaseUtility.SetPragmas(MusicDbClient);

        DatabaseUtility.AddTable(MusicDbClient, "artist", "CREATE TABLE artist ( idArtist integer primary key, strArtist text, strSortName text)");
        DatabaseUtility.AddTable(MusicDbClient, "album", "CREATE TABLE album ( idAlbum integer primary key, idAlbumArtist integer, strAlbum text, iNumArtists integer)");
        DatabaseUtility.AddTable(MusicDbClient, "albumartist", "CREATE TABLE albumartist ( idAlbumArtist integer primary key, strAlbumArtist text)");
        DatabaseUtility.AddTable(MusicDbClient, "genre", "CREATE TABLE genre ( idGenre integer primary key, strGenre text)");
        DatabaseUtility.AddTable(MusicDbClient, "path", "CREATE TABLE path ( idPath integer primary key,  strPath text)");
        DatabaseUtility.AddTable(MusicDbClient, "albuminfo", "CREATE TABLE albuminfo ( idAlbumInfo integer primary key, idAlbum integer, idArtist integer, idAlbumArtist integer,iYear integer, idGenre integer, strTones text, strStyles text, strReview text, strImage text, strTracks text, iRating integer)");
        DatabaseUtility.AddTable(MusicDbClient, "artistinfo", "CREATE TABLE artistinfo ( idArtistInfo integer primary key, idArtist integer, strBorn text, strYearsActive text, strGenres text, strTones text, strStyles text, strInstruments text, strImage text, strAMGBio text, strAlbums text, strCompilations text, strSingles text, strMisc text)");
        DatabaseUtility.AddTable(MusicDbClient, "song", "CREATE TABLE song ( idSong integer primary key, idArtist integer, idAlbum integer, idAlbumArtist integer, idGenre integer, idPath integer, strTitle text, iTrack integer, iDuration integer, iYear integer, dwFileNameCRC text, strFileName text, iTimesPlayed integer, iRating integer, favorite integer, dateadded timestamp)");
        // Add a Trigger for inserting the Date into the song table, whenever we do an update
        string strSQL = "CREATE TRIGGER IF NOT EXISTS insert_song_timeStamp AFTER INSERT ON song " +
                        "BEGIN " +
                        " UPDATE song SET dateadded = DATETIME('NOW') " +
                        " WHERE rowid = new.rowid; " +
                        "END;";
        MusicDbClient.Execute(strSQL);

        DatabaseUtility.AddTable(MusicDbClient, "scrobbleusers", "CREATE TABLE scrobbleusers ( idScrobbleUser integer primary key, strUsername text, strPassword text)");
        DatabaseUtility.AddTable(MusicDbClient, "scrobblesettings", "CREATE TABLE scrobblesettings ( idScrobbleSettings integer primary key, idScrobbleUser integer, iAddArtists integer, iAddTracks integer, iNeighbourMode integer, iRandomness integer, iScrobbleDefault integer, iSubmitOn integer, iDebugLog integer, iOfflineMode integer, iPlaylistLimit integer, iPreferCount integer, iRememberStartArtist integer)");
        DatabaseUtility.AddTable(MusicDbClient, "scrobblemode", "CREATE TABLE scrobblemode ( idScrobbleMode integer primary key, idScrobbleUser integer, iSortID integer, strModeName text)");
        DatabaseUtility.AddTable(MusicDbClient, "scrobbletags", "CREATE TABLE scrobbletags ( idScrobbleTag integer primary key, idScrobbleMode integer, iSortID integer, strTagName text)");

        DatabaseUtility.AddIndex(MusicDbClient, "idxartist_strArtist", "CREATE UNIQUE INDEX idxartist_strArtist ON artist(strArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartist_strSortName", "CREATE INDEX idxartist_strSortName ON artist(strSortName ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbum_idAlbumArtist", "CREATE INDEX idxalbum_idAlbumArtist ON album(idAlbumArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbum_strAlbum", "CREATE INDEX idxalbum_strAlbum ON album(strAlbum ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxgenre_strGenre", "CREATE UNIQUE INDEX idxgenre_strGenre ON genre(strGenre ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxpath_strPath", "CREATE UNIQUE INDEX idxpath_strPath ON path(strPath ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_idAlbum", "CREATE INDEX idxalbuminfo_idAlbum ON albuminfo(idAlbum ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_idArtist", "CREATE INDEX idxalbuminfo_idArtist ON albuminfo(idArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_idGenre", "CREATE INDEX idxalbuminfo_idGenre ON albuminfo(idGenre ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartistinfo_idArtist", "CREATE INDEX idxartistinfo_idArtist ON artistinfo(idArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxsong_idArtist", "CREATE INDEX idxsong_idArtist ON song(idArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxsong_idAlbum", "CREATE INDEX idxsong_idAlbum ON song(idAlbum ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxsong_idGenre", "CREATE INDEX idxsong_idGenre ON song(idGenre ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxsong_idPath", "CREATE INDEX idxsong_idPath ON song(idPath ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxsong_strTitle", "CREATE INDEX idxsong_strTitle ON song(strTitle ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxsong_strFileName", "CREATE INDEX idxsong_strFileName ON song(strFileName ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxsong_dwFileNameCRC", "CREATE INDEX idxsong_dwFileNameCRC ON song(dwFileNameCRC ASC)");

      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      Log.Info("music database opened");
    }

    private bool UpdateDB_V8_to_V9()
    {
      try
      {
        bool success = true;
        // We're working on a copy of the V8 database        
        using (SQLiteClient update_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "MusicDatabaseV9.db3")))
        {
          DatabaseUtility.SetPragmas(update_db);
          SQLiteResultSet results;

          // workaround for missing ALTER TABLE commands
          string strSQL = @"CREATE TEMPORARY TABLE album_backup(idAlbum integer primary key, idAlbumArtist integer, strAlbum text, iNumArtists integer)";
          results = update_db.Execute(strSQL);
          strSQL = "INSERT INTO album_backup SELECT idAlbum, idArtist, strAlbum, iNumArtists FROM album";
          results = update_db.Execute(strSQL);
          strSQL = "DROP TABLE album";
          results = update_db.Execute(strSQL);
          strSQL = "CREATE TABLE album(idAlbum integer primary key, idAlbumArtist integer, strAlbum text, iNumArtists integer)";
          results = update_db.Execute(strSQL);
          strSQL = "INSERT INTO album SELECT idAlbum, idAlbumArtist, strAlbum, iNumArtists FROM album_backup";
          results = update_db.Execute(strSQL);
          strSQL = "DROP TABLE album_backup";
          results = update_db.Execute(strSQL);

          strSQL = "CREATE TABLE albumartist ( idAlbumArtist integer primary key, strAlbumArtist text)";
          results = update_db.Execute(strSQL);
          strSQL = "INSERT INTO albumartist SELECT idArtist, strArtist FROM artist";
          results = update_db.Execute(strSQL);

          strSQL = "ALTER TABLE albuminfo ADD COLUMN idAlbumArtist integer";
          results = update_db.Execute(strSQL);
          strSQL = "UPDATE albuminfo SET idAlbumArtist=(SELECT idAlbumArtist FROM album where album.idAlbum=albuminfo.idAlbum)";
          results = update_db.Execute(strSQL);

          strSQL = "ALTER TABLE song ADD COLUMN idAlbumArtist integer";
          results = update_db.Execute(strSQL);
          strSQL = "UPDATE song SET idAlbumArtist=(SELECT idAlbumArtist FROM album where album.idAlbum=song.idAlbum)";
          results = update_db.Execute(strSQL);

          strSQL = "ALTER TABLE scrobblesettings ADD COLUMN iAnnounce integer";
          results = update_db.Execute(strSQL);

          //strSQL = "DROP INDEX idxalbum_idArtist";
          //results = update_db.Execute(strSQL);

          // do a quick test whether the update was successful
          if (!DatabaseUtility.TableColumnExists(update_db, "album", "idAlbumArtist"))
            success = false;

          // write the journal before using closes the db handle
          update_db.Close();
        }
        return success;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase: Error updating V8 to V9: {0} stack: {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }
    #endregion

    #region Transactions
    public void BeginTransaction()
    {
      try
      {
        MusicDbClient.Execute("begin");
      }
      catch (Exception ex)
      {
        Log.Error("BeginTransaction: musicdatabase begin transaction failed exception err:{0} ", ex.Message);
        //Open();
      }
    }

    public void CommitTransaction()
    {
      Log.Info("Commit will effect {0} rows", MusicDbClient.ChangedRows());
      SQLiteResultSet CommitResults;
      try
      {
        CommitResults = MusicDbClient.Execute("commit");
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase commit failed exception err:{0} ", ex.Message);
        Open();
      }
    }

    public void RollbackTransaction()
    {
      try
      {
        MusicDbClient.Execute("rollback");
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase rollback failed exception err:{0} ", ex.Message);
        Open();
      }
    }
    #endregion
  }
}
