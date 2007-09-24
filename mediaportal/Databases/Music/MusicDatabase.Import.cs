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
    public class CArtistCache
    {
      public int idArtist = 0;
      public string strArtist = String.Empty;
    };

    public class CPathCache
    {
      public int idPath = 0;
      public string strPath = String.Empty;
    };

    public class CGenreCache
    {
      public int idGenre = 0;
      public string strGenre = String.Empty;
    };

    public class AlbumInfoCache : AlbumInfo
    {
      public int idAlbum = 0;
      public int idArtist = 0;
      public int idAlbumArtist = 0;
      public int idPath = -1;
    };

    public class ArtistInfoCache : ArtistInfo
    {
      public int idArtist = 0;
    }

    public class CAlbumArtistCache
    {
      public int idAlbumArtist = 0;
      public string strAlbumArtist = String.Empty;
    };

    private ArrayList _artistCache = new ArrayList();
    private ArrayList _genreCache = new ArrayList();
    private ArrayList _pathCache = new ArrayList();
    private ArrayList _albumCache = new ArrayList();
    private ArrayList _albumartistCache = new ArrayList();
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

    private SQLiteResultSet PathResults;
    private SQLiteResultSet PathDeleteResults;
    private int NumPaths;
    private int PathNum;
    private string strSQL;

    private ArrayList availableFiles;

    public void EmptyCache()
    {
      _artistCache.Clear();
      _genreCache.Clear();
      _pathCache.Clear();
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
      return MusicDatabaseReorg(shares, _treatFolderAsAlbum, _scanForVariousArtists, updateSinceLastImport);
    }

    public int MusicDatabaseReorg(ArrayList shares, bool treatFolderAsAlbum, bool scanForVariousArtists, bool updateSinceLastImport)
    {
      // Make sure we use the selected settings if the user hasn't saved the
      // configuration...
      _treatFolderAsAlbum = treatFolderAsAlbum;
      _scanForVariousArtists = scanForVariousArtists;

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
        /// Delete song that are in non-existing MusicFolders (for example: you moved everything to another disk)
        MyArgs.progress = 2;
        MyArgs.phase = "Removing songs in old folders";
        OnDatabaseReorgChanged(MyArgs);
        DeleteSongsOldMusicFolders();


        /// Delete files that don't exist anymore (example: you deleted files from the Windows Explorer)
        MyArgs.progress = 4;
        MyArgs.phase = "Removing non existing songs";
        OnDatabaseReorgChanged(MyArgs);
        DeleteNonExistingSongs();

        /// Add missing files (example: You downloaded some new files)
        MyArgs.progress = 6;
        MyArgs.phase = "Scanning new files";
        OnDatabaseReorgChanged(MyArgs);

        int AddMissingFilesResult = AddMissingFiles(8, 36, ref fileCount);
        Log.Info("Musicdatabasereorg: Addmissingfiles: {0} files added", AddMissingFilesResult);

        /// Update the tags
        MyArgs.progress = 38;
        MyArgs.phase = "Adding info to DB";
        OnDatabaseReorgChanged(MyArgs);
        UpdateTags(40, 82);	//This one works for all the files in the MusicDatabase

        /// Cleanup foreign keys tables.
        /// We added, deleted new files
        /// We update all the tags
        /// Now lets clean up all the foreign keys
        MyArgs.progress = 84;
        MyArgs.phase = "Checking Artists";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeleteArtistids();

        MyArgs.progress = 85;
        MyArgs.phase = "Checking AlbumArtists";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeleteAlbumArtistids();

        MyArgs.progress = 86;
        MyArgs.phase = "Checking Genres";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeleteGenreids();

        MyArgs.progress = 88;
        MyArgs.phase = "Checking Paths";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeletePathids();

        MyArgs.progress = 90;
        MyArgs.phase = "Checking Albums";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeleteAlbumids();

        MyArgs.progress = 92;
        MyArgs.phase = "Updating album artists counts";
        OnDatabaseReorgChanged(MyArgs);
        UpdateAlbumArtistsCounts(92, 94);

        MyArgs.progress = 94;
        MyArgs.phase = "Updating sortable artist names";
        OnDatabaseReorgChanged(MyArgs);
        UpdateSortableArtistNames();

        // Check for a database backup and delete it if it exists       
        string backupDbPath = Config.GetFile(Config.Dir.Database, "musicdatabase4.db3.bak");

        if (File.Exists(backupDbPath))
        {
          MyArgs.progress = 95;
          MyArgs.phase = "Deleting backup database";
          OnDatabaseReorgChanged(MyArgs);

          File.Delete(backupDbPath);
        }
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

    private int UpdateTags(int StartProgress, int EndProgress)
    {
      SQLiteResultSet results;
      string strSQL;
      int NumRecordsUpdated = 0;
      string MusicFileName, MusicFilePath;

      Log.Info("Musicdatabasereorg: starting Tag update");
      Log.Info("Musicdatabasereorg: Going to check tags of {0} files", availableFiles.Count);

      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      int ProgressRange = EndProgress - StartProgress;
      int TotalSongs = availableFiles.Count;
      int SongCounter = 0;

      double NewProgress;
      _currentDate = System.DateTime.Now;

      foreach (string MusicFile in availableFiles)
      {
        DatabaseUtility.Split(MusicFile, out MusicFilePath, out MusicFileName);

        /// Convert.ToChar(34) wil give you a "
        /// This is handy in building strings for SQL
        strSQL = String.Format("select * from song,path where song.idPath=path.idPath and strFileName={1}{0}{1} and strPath like {1}{2}{1}", MusicFileName, Convert.ToChar(34), MusicFilePath);

        try
        {
          results = MusicDbClient.Execute(strSQL);
          if (results == null)
          {
            Log.Info("Musicdatabasereorg: UpdateTags finished with error (results == null)");
            return (int)Errors.ERROR_REORG_SONGS;
          }
        }

        catch (Exception)
        {
          Log.Error("Musicdatabasereorg: UpdateTags finished with error (exception for select)");
          return (int)Errors.ERROR_REORG_SONGS;
        }

        if (results.Rows.Count >= 1)
        {
          // The song will be updated, tags from the file will be checked against the tags in the database
          int idSong = Int32.Parse(DatabaseUtility.Get(results, 0, "song.idSong"));
          if (!UpdateSong(MusicFile, idSong))
          {
            Log.Info("Musicdatabasereorg: Song update after tag update failed for: {0}", MusicFile);
            //m_db.Execute("rollback"); 
            return (int)Errors.ERROR_REORG_SONGS;
          }
          else
          {
            NumRecordsUpdated++;
          }
        }
        if ((SongCounter % 10) == 0)
        {
          NewProgress = StartProgress + ((ProgressRange * SongCounter) / TotalSongs);
          MyArgs.progress = Convert.ToInt32(NewProgress);
          MyArgs.phase = String.Format("Updating tags {0}/{1}", SongCounter, availableFiles.Count);
          OnDatabaseReorgChanged(MyArgs);
        }
        SongCounter++;
      }

      Log.Info("Musicdatabasereorg: UpdateTags completed for {0} songs", (int)NumRecordsUpdated);
      return (int)Errors.ERROR_OK;
    }

    public bool UpdateSong(string strPathSong, int idSong)
    {
      try
      {
        int idAlbum = 0;
        int idArtist = 0;
        int idPath = 0;
        int idGenre = 0;
        int idAlbumArtist = 0;

        MusicTag tag;
        tag = TagReader.ReadTag(strPathSong);
        if (tag != null)
        {
          //Log.Write ("Musicdatabasereorg: We are gonna update the tags for {0}", strPathSong);
          Song song = new Song();
          song.Title = tag.Title;
          song.Genre = tag.Genre;
          song.FileName = strPathSong;
          song.Artist = tag.Artist;
          song.Album = tag.Album;
          song.AlbumArtist = tag.AlbumArtist;
          song.Year = tag.Year;
          song.Track = tag.Track;
          song.Duration = tag.Duration;
          song.Rating = tag.Rating;

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
                    Log.Debug("Could not extract thumbnail from {0}", strPathSong);
                  if (!Util.Picture.CreateThumbnail(tag.CoverArtImage, strLargeThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0))
                    Log.Debug("Could not extract thumbnail from {0}", strPathSong);
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
              string sharefolderThumb = MediaPortal.Util.Utils.GetFolderThumb(strPathSong);
              if (System.IO.File.Exists(sharefolderThumb))
              {
                if (!MediaPortal.Util.Picture.CreateThumbnail(sharefolderThumb, strSmallThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0))
                  Log.Debug("Could not create album thumb from folder {0}", strPathSong);
                if (!MediaPortal.Util.Picture.CreateThumbnail(sharefolderThumb, strLargeThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0))
                  Log.Debug("Could not create large album thumb from folder {0}", strPathSong);
              }
            }
          }


          // create the local folder thumb cache / and folder.jpg itself if not present
          if (_useFolderThumbs || _createMissingFolderThumbs)
            CreateFolderThumbs(strPathSong, strSmallThumb);

          if (_useFolderArtForArtistGenre)
          {
            CreateArtistThumbs(strSmallThumb, song.Artist);
            CreateGenreThumbs(strSmallThumb, song.Genre);
          }

          string strPath, strFileName;
          DatabaseUtility.Split(song.FileName, out strPath, out strFileName);

          string strTmp;
          strTmp = song.Album;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Album = strTmp;
          strTmp = song.Genre;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Genre = strTmp;
          strTmp = song.Artist;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Artist = strTmp;
          strTmp = song.Title;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Title = strTmp;
          strTmp = song.AlbumArtist;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.AlbumArtist = strTmp;

          DatabaseUtility.RemoveInvalidChars(ref strFileName);

          /// PDW 25 may 2005
          /// Adding these items starts a select and insert query for each. 
          /// Maybe we should check if anything has changed in the tags
          /// if not, no need to add and invoke query's.
          /// here we are gonna (try to) add the tags

          idGenre = AddGenre(tag.Genre);
          //Log.Write ("Tag.genre = {0}",tag.Genre);
          idArtist = AddArtist(tag.Artist);
          //Log.Write ("Tag.Artist = {0}",tag.Artist);
          idPath = AddPath(strPath);
          //Log.Write ("strPath= {0}",strPath);
          if (tag.AlbumArtist == "")
            tag.AlbumArtist = tag.Artist;
          idAlbumArtist = AddAlbumArtist(tag.AlbumArtist);

          if (_treatFolderAsAlbum)
            idAlbum = AddAlbum(tag.Album, /*idArtist,*/ idAlbumArtist, idPath);
          else
            idAlbum = AddAlbum(tag.Album, /*idArtist,*/ idAlbumArtist);

          ulong dwCRC = 0;
          CRCTool crc = new CRCTool();
          crc.Init(CRCTool.CRCCode.CRC32);
          dwCRC = crc.calc(strPathSong);

          //SQLiteResultSet results;

          //Log.Write ("Song {0} will be updated with CRC={1}",song.FileName,dwCRC);

          string strSQL;
          strSQL = String.Format("update song set idArtist={0},idAlbum={1},idAlbumArtist={2},idGenre={3},idPath={4},strTitle='{5}',iTrack={6},iDuration={7},iYear={8},dwFileNameCRC='{9}',strFileName='{10}',iRating={12} where idSong={11}",
            idArtist, idAlbum, idAlbumArtist, idGenre, idPath,
            song.Title,
            song.Track, song.Duration, song.Year,
            dwCRC,
            strFileName, idSong, song.Rating);
          //Log.Write (strSQL);
          try
          {
            MusicDbClient.Execute(strSQL);
          }
          catch (Exception)
          {
            Log.Error("Musicdatabasereorg: Update tags for {0} failed because of DB exception", strPathSong);
            return false;
          }
        }
        else
        {
          Log.Info("Musicdatabasereorg: cannot get tag for {0}", strPathSong);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabasereorg: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      //Log.Write ("Musicdatabasereorg: Update for {0} success", strPathSong);
      return true;
    }

    private void CreateFolderThumbs(string strSongPath, string strSmallThumb)
    {
      if (System.IO.File.Exists(strSmallThumb) && strSongPath != String.Empty)
      {
        string folderThumb = MediaPortal.Util.Utils.GetFolderThumb(strSongPath);
        //string folderLThumb = folderThumb;
        //folderLThumb = Util.Utils.ConvertToLargeCoverArt(folderLThumb);
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
            //System.IO.File.Copy(strSmallThumb, artistThumb, true);
            //System.IO.File.SetAttributes(artistThumb, System.IO.File.GetAttributes(artistThumb) | System.IO.FileAttributes.Hidden);
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
          for (int i = 1 ; (i < 10 && i < strGenre.Length & !FixedTheCode) ; ++i)
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
            //System.IO.File.Copy(strSmallThumb, genreThumb, true);
            //System.IO.File.SetAttributes(genreThumb, System.IO.File.GetAttributes(genreThumb) | System.IO.FileAttributes.Hidden);
          }
          catch (Exception) { }
        }
      }
    }

    private int DeleteNonExistingSongs()
    {
      string strSQL;
      /// Opening the MusicDatabase

      SQLiteResultSet results;
      strSQL = String.Format("select * from song, path where song.idPath=path.idPath");
      try
      {
        results = MusicDatabase.DirectExecute(strSQL);
        if (results == null)
          return (int)Errors.ERROR_REORG_SONGS;
      }
      catch (Exception)
      {
        Log.Error("DeleteNonExistingSongs() to get songs from database");
        return (int)Errors.ERROR_REORG_SONGS;
      }
      int removed = 0;
      Log.Info("Musicdatabasereorg: starting song cleanup for {0} songs", (int)results.Rows.Count);
      for (int i = 0 ; i < results.Rows.Count ; ++i)
      {
        string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
        strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
        ///pDlgProgress.SetLine(2, System.IO.Path.GetFileName(strFileName) );

        if (!System.IO.File.Exists(strFileName))
        {
          /// song doesn't exist anymore, delete it
          /// We don't care about foreign keys at this moment. We'll just change this later.

          removed++;
          //Log.Info("Musicdatabasereorg:Song {0} will to be deleted from MusicDatabase", strFileName);
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
      Log.Info("Musicdatabasereorg: DeleteNonExistingSongs completed");
      return (int)Errors.ERROR_OK;
    }

    private int ExamineAndDeleteArtistids()
    {
      /// This will delete all artists and artistinfo from the database that don't have a corresponding song anymore
      /// First delete all the albuminfo before we delete albums (foreign keys)

      /// TODO: delete artistinfo first
      string strSql = "delete from artist where artist.idArtist not in (select idArtist from song)";
      try
      {
        MusicDbClient.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteArtistids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_ARTIST;
      }

      Log.Info("Musicdatabasereorg: ExamineAndDeleteArtistids completed");
      return (int)Errors.ERROR_OK;
    }

    private int ExamineAndDeleteAlbumArtistids()
    {
      /// This will delete all albumartists from the database that don't have a corresponding song anymore
      string strSql = "delete from albumartist where albumartist.idAlbumArtist not in (select idAlbumArtist from song)";
      try
      {
        MusicDbClient.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteAlbumArtistids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_ALBUMARTIST;
      }

      Log.Info("Musicdatabasereorg: ExamineAndDeleteAlbumArtistids completed");
      return (int)Errors.ERROR_OK;
    }

    private int ExamineAndDeleteGenreids()
    {
      /// This will delete all genres from the database that don't have a corresponding song anymore
      SQLiteResultSet result;
      string strSql = "delete from genre where idGenre not in (select idGenre from song)";
      try
      {
        MusicDbClient.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteGenreids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_GENRE;
      }

      strSql = "select count (*) aantal from genre where idGenre not in (select idGenre from song)";
      try
      {
        result = MusicDatabase.DirectExecute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteGenreids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_GENRE;

      }
      string Aantal = DatabaseUtility.Get(result, 0, "aantal");
      if (Aantal != "0")
        return (int)Errors.ERROR_REORG_GENRE;
      Log.Info("Musicdatabasereorg: ExamineAndDeleteGenreids completed");

      return (int)Errors.ERROR_OK;
    }

    private int ExamineAndDeletePathids()
    {
      /// This will delete all paths from the database that don't have a corresponding song anymore
      string strSql = String.Format("delete from path where idPath not in (select idPath from song)");
      try
      {
        MusicDbClient.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeletePathids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_PATH;
      }
      Log.Info("Musicdatabasereorg: ExamineAndDeletePathids completed");
      return (int)Errors.ERROR_OK;
    }

    private int ExamineAndDeleteAlbumids()
    {
      /// This will delete all albums from the database that don't have a corresponding song anymore
      /// First delete all the albuminfo before we delete albums (foreign keys)
      string strSql = String.Format("delete from albuminfo where idAlbum not in (select idAlbum from song)");
      try
      {
        MusicDbClient.Execute(strSql);
      }
      catch (Exception)
      {
        //m_db.Execute("rollback");
        Log.Error("MusicDatabasereorg: ExamineAndDeleteAlbumids() unable to delete old albums");
        return (int)Errors.ERROR_REORG_ALBUM;
      }
      /// Now all the albums without songs will be deleted.
      ///SQLiteResultSet results;
      strSql = String.Format("delete from album where idAlbum not in (select idAlbum from song)");
      try
      {
        MusicDbClient.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteAlbumids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_ALBUM;
      }
      Log.Info("Musicdatabasereorg: ExamineAndDeleteAlbumids completed");
      return (int)Errors.ERROR_OK;
    }

    private int Compress()
    {
      //	compress database
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

    private int LoadShares()
    {
      /// 25-may-2005 TFRO71
      /// Added this function to make scan the Music Shares that are in the configuration file.
      /// Songs that are not in these Shares will be removed from the MusicDatabase
      /// The files will offcourse not be touched
      string currentFolder = String.Empty;
      bool fileMenuEnabled = false;
      string fileMenuPinCode = String.Empty;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);

        string strDefault = xmlreader.GetValueAsString("music", "default", String.Empty);
        for (int i = 0 ; i < 20 ; i++)
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

    private int DeleteSongsOldMusicFolders()
    {

      /// PDW 24-05-2005
      /// Here we handle the songs in non-existing MusicFolders (shares).
      /// So we have to check Mediaportal.XML
      /// Loading the current MusicFolders
      Log.Info("Musicdatabasereorg: deleting songs in non-existing shares");

      /// For each path in the MusicDatabase we will check if it's in a share
      /// If not, we will delete all the songs in this path.
      strSQL = String.Format("select * from path");

      try
      {
        PathResults = MusicDatabase.DirectExecute(strSQL);
        if (PathResults == null)
          return (int)Errors.ERROR_REORG_SONGS;
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg:DeleteSongsOldMusicFolders() failed");
        //MusicDatabase.DBHandle.Execute("rollback");
        return (int)Errors.ERROR_REORG_SONGS;
      }
      NumPaths = PathResults.Rows.Count;

      /// We will walk through all the paths (from the songs) and see if they match with a share/MusicFolder (from the config)
      for (PathNum = 0 ; PathNum < PathResults.Rows.Count ; ++PathNum)
      {
        string Path = DatabaseUtility.Get(PathResults, PathNum, "strPath");
        string PathId = DatabaseUtility.Get(PathResults, PathNum, "idPath");
        /// We now have a path, we will check it along all the shares
        bool Path_has_Share = false;
        foreach (string Share in _shares)
        {
          ///Here we can check if the Path has an existing share
          if (Share.Length <= Path.Length)
          {
            string Path_part = Path.Substring(0, Share.Length);
            if (Share.ToUpper() == Path_part.ToUpper())
              Path_has_Share = true;
          }
        }
        if (!Path_has_Share)
        {
          Log.Info("Musicdatabasereorg: Path {0} with id {1} has no corresponding share, songs will be deleted ", Path, PathId);
          strSQL = String.Format("delete from song where idPath = {0}", PathId);
          try
          {
            PathDeleteResults = MusicDatabase.DirectExecute(strSQL);
            if (PathDeleteResults == null)
              return (int)Errors.ERROR_REORG_SONGS;
          }
          catch (Exception)
          {
            //MusicDatabase.DBHandle.Execute("rollback");
            Log.Error("Musicdatabasereorg: DeleteSongsOldMusicFolders failed");
            return (int)Errors.ERROR_REORG_SONGS;
          }

          Log.Info("Trying to commit the deletes from the DB");

        } /// If path has no share
      } /// For each path
      Log.Info("Musicdatabasereorg: DeleteSongsOldMusicFolders completed");
      return (int)Errors.ERROR_OK;
    }

    private int AddMissingFiles(int StartProgress, int EndProgress, ref int fileCount)
    {
      /// This seems to clear the arraylist and make it valid
      availableFiles = new ArrayList();

      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      string strSQL;
      ulong dwCRC;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);

      int totalFiles = 0;

      int ProgressRange = EndProgress - StartProgress;
      int SongCounter = 0;
      int AddedCounter = 0;
      int TotalSongs = 0;
      string MusicFilePath, MusicFileName;
      double NewProgress;

      foreach (string Share in _shares)
      {
        ///Here we can check if the Path has an existing share
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
      Log.Info("Musicdatabasereorg: Found {0} files to check if they are new", (int)totalFiles);
      SQLiteResultSet results;

      foreach (string MusicFile in availableFiles)
      {
        ///Here we can check if the Path has an existing share
        ///
        SongCounter++;

        DatabaseUtility.Split(MusicFile, out MusicFilePath, out MusicFileName);

        dwCRC = crc.calc(MusicFile);

        /// Convert.ToChar(34) wil give you a "
        /// This is handy in building strings for SQL
        strSQL = String.Format("select * from song,path where song.idPath=path.idPath and strFileName={1}{0}{1} and strPath like {1}{2}{1}", MusicFileName, Convert.ToChar(34), MusicFilePath);
        //Log.Write (strSQL);
        //Log.Write (MusicFilePath);
        //Log.Write (MusicFile);
        //Log.Write (MusicFileName);

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
          //m_db.Execute("rollback");
          return (int)Errors.ERROR_REORG_SONGS;
        }

        if (results.Rows.Count >= 1)
        {
          /// The song exists
          /// Log.Write ("Song {0} exists, dont do a thing",MusicFileName);
          /// string strFileName = DatabaseUtility.Get(results,0,"path.strPath") ;
          /// strFileName += DatabaseUtility.Get(results,0,"song.strFileName") ;
        }
        else
        {
          //The song does not exist, we will add it.
          AddSong(MusicFileName, MusicFilePath);
          AddedCounter++;
        }

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
              MyArgs.phase = String.Format("Adding new files: {0} files found", totalFiles);
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

    private void CheckFileForInclusion(string file, ref int totalFiles)
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

      totalFiles++;
    }

    public void UpdateAlbumArtistsCounts(int startProgress, int endProgress)
    {
      if (_albumCache.Count == 0)
        return;

      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      int progressRange = endProgress - startProgress;
      int totalAlbums = _albumCache.Count;
      int albumCounter = 0;
      double newProgress = 0;

      Hashtable artistCountTable = new Hashtable();
      bool variousArtistsFound = false;

      string strSQL;

      try
      {
        // Process the array from the end, to have no troubles when removing processed items
        for (int j = _albumCache.Count - 1 ; j > -1 ; j--)
        {
          AlbumInfoCache album = (AlbumInfoCache)_albumCache[j];
          artistCountTable.Clear();

          if (album.Album == MediaPortal.GUI.Library.Strings.Unknown)
            continue;

          int lAlbumId = album.idAlbum;
          int lAlbumArtistId = album.idAlbumArtist;

          List<Song> songs = new List<Song>();

          if (_treatFolderAsAlbum)
            GetSongsByPathId(album.idPath, ref songs);

          else
            this.GetSongsByAlbumID(album.idAlbum, ref songs);

          ++albumCounter;

          if (songs.Count > 1)
          {
            //	Are the artists of this album all the same
            for (int i = 0 ; i < songs.Count ; i++)
            {
              Song song = (Song)songs[i];
              artistCountTable[song.artistId] = song;
            }
          }

          int artistCount = Math.Max(1, artistCountTable.Count);

          if (artistCount > 1)
          {
            variousArtistsFound = true;
            Log.Info("Musicdatabasereorg: multiple album artists album found: {0}.  Updating album artist count ({1}).", album.Album, artistCount);

            foreach (DictionaryEntry entry in artistCountTable)
            {
              Song s = (Song)entry.Value;
              Log.Info("   ArtistID:{0}  Artist Name:{1}  Track Title:{2}", s.artistId, s.Artist, s.Title);
            }
          }

          strSQL = string.Format("update album set iNumArtists={0} where idAlbum={1}", artistCount, album.idAlbum);
          MusicDbClient.Execute(strSQL);

          // Remove the processed Album from the cache
          lock (_albumCache)
          {
            _albumCache.RemoveAt(j);
          }

          if ((albumCounter % 10) == 0)
          {
            newProgress = startProgress + ((progressRange * albumCounter) / totalAlbums);
            MyArgs.progress = Convert.ToInt32(newProgress);
            MyArgs.phase = String.Format("Updating album {0}/{1} artist counts", albumCounter, totalAlbums);
            OnDatabaseReorgChanged(MyArgs);
          }
        }

        // Finally, set the artist id to the "Various Artists" id on all albums with more than one artist
        if (_scanForVariousArtists && variousArtistsFound)
        {
          long idVariousArtists = GetVariousArtistsId();

          if (idVariousArtists != -1)
          {
            Log.Info("Musicdatabasereorg: updating artist id's for 'Various Artists' albums");

            strSQL = string.Format("update album set idAlbumArtist={0} where iNumArtists>1", idVariousArtists);
            MusicDbClient.Execute(strSQL);
          }
        }
      }

      catch (Exception ex)
      {
        Log.Info("Musicdatabasereorg: {0}", ex);
      }
    }

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

    private void UpdateSortableArtistNames()
    {
      ArrayList artists = new ArrayList();
      this.GetAllArtists(ref artists);

      for (int i = 0 ; i < artists.Count ; i++)
      {
        string origArtistName = (string)artists[i];
        string sortableArtistName = origArtistName;

        StripArtistNamePrefix(ref sortableArtistName, true);

        try
        {
          DatabaseUtility.RemoveInvalidChars(ref sortableArtistName);
          DatabaseUtility.RemoveInvalidChars(ref origArtistName);
          string strSQL = String.Format("update artist set strSortName='{0}' where strArtist like '{1}'", sortableArtistName, origArtistName);
          MusicDbClient.Execute(strSQL);
        }

        catch (Exception ex)
        {
          Log.Info("UpdateSortableArtistNames: {0}", ex);
        }
      }
    }

    public bool SongExists(string strFileName)
    {
      ulong dwCRC = 0;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);
      dwCRC = crc.calc(strFileName);

      string strSQL;
      strSQL = String.Format("select idSong from song where dwFileNameCRC like '{0}'",
                     dwCRC);

      SQLiteResultSet results;
      results = MusicDbClient.Execute(strSQL);
      if (results.Rows.Count > 0)
        // Found
        return true;
      else
        // Not Found
        return false;
    }

    public bool RenameSong(string strOldFileName, string strNewFileName)
    {
      try
      {
        string strPath, strFName;
        DatabaseUtility.Split(strNewFileName, out strPath, out strFName);

        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);

        // The rename may have been on a directory or a file
        // In case of a directory rename, the Path needs to be corrected
        FileInfo fi = new FileInfo(strNewFileName);
        if (fi.Exists)
        {
          // Must be a file that has been changed
          // Now get the CRC of the original file name and the new file name
          ulong dwOldCRC = crc.calc(strOldFileName);
          ulong dwNewCRC = crc.calc(strNewFileName);

          DatabaseUtility.RemoveInvalidChars(ref strFName);

          string strSQL;
          strSQL = String.Format("update song set dwFileNameCRC = '{0}', strFileName = '{1}' where dwFileNameCRC like '{2}'",
                       dwNewCRC,
                       strFName,
                       dwOldCRC);
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

            string strSQL;
            strSQL = String.Format("select * from path where strPath like '{0}%'",
                    strOldFileName);

            SQLiteResultSet results;
            SQLiteResultSet resultSongs;
            ulong dwCRC = 0;
            results = MusicDbClient.Execute(strSQL);
            if (results.Rows.Count > 0)
            {
              try
              {
                BeginTransaction();
                // We might have changed a Top directory, so we get a lot of path entries returned
                for (int rownum = 0 ; rownum < results.Rows.Count ; rownum++)
                {
                  int lPathId = DatabaseUtility.GetAsInt(results, rownum, "path.idPath");
                  string strTmpPath = DatabaseUtility.Get(results, rownum, "path.strPath");
                  strPath = strTmpPath.Replace(strOldFileName, strNewFileName);
                  // Need to keep an unmodified path for the later CRC calculation
                  strTmpPath = strPath;
                  DatabaseUtility.RemoveInvalidChars(ref strTmpPath);
                  strSQL = String.Format("update path set strPath='{0}' where idPath={1}",
                          strTmpPath,
                          lPathId);

                  MusicDbClient.Execute(strSQL);
                  // And now we need to update the songs with the new CRC
                  strSQL = String.Format("select * from song where idPath = {0}",
                               lPathId);
                  resultSongs = MusicDbClient.Execute(strSQL);
                  if (resultSongs.Rows.Count > 0)
                  {
                    for (int i = 0 ; i < resultSongs.Rows.Count ; i++)
                    {
                      strFName = DatabaseUtility.Get(resultSongs, i, "song.strFileName");
                      int lSongId = DatabaseUtility.GetAsInt(resultSongs, i, "song.idSong");
                      dwCRC = crc.calc(strPath + strFName);
                      strSQL = String.Format("update song set dwFileNameCRC='{0}' where idSong={1}",
                                dwCRC,
                                lSongId);
                      MusicDbClient.Execute(strSQL);
                    }
                  }
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
      try
      {
        DatabaseUtility.RemoveInvalidChars(ref strPath);

        string strSQL;
        strSQL = String.Format("select * from path where strPath like '{0}%'",
                strPath);

        // Get all songs and Path matching the deleted directory and remove them.
        SQLiteResultSet results;
        SQLiteResultSet resultSongs;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          try
          {
            BeginTransaction();
            // We might have deleted a Top directory, so we get a lot of path entries returned
            for (int rownum = 0 ; rownum < results.Rows.Count ; rownum++)
            {
              int lPathId = DatabaseUtility.GetAsInt(results, rownum, "path.idPath");
              string strSongPath = DatabaseUtility.Get(results, rownum, "path.strPath");
              // And now we need to remove the songs
              strSQL = String.Format("select * from song where idPath = {0}",
                           lPathId);
              resultSongs = MusicDbClient.Execute(strSQL);
              if (resultSongs.Rows.Count > 0)
              {
                for (int i = 0 ; i < resultSongs.Rows.Count ; i++)
                {
                  string strFName = DatabaseUtility.Get(resultSongs, i, "song.strFileName");
                  DeleteSong(strSongPath + strFName, true);
                }
              }
              EmptyCache();
            }
            // And finally let's remove all the path information
            strSQL = String.Format("delete from path where strPath like '{0}%'",
                                strPath);
            results = MusicDbClient.Execute(strSQL);
            CommitTransaction();
            return true;
          }
          catch (Exception)
          {
            RollbackTransaction();
            Log.Error("Delete Directory for {0} failed because of DB exception", strPath);
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        return false;
      }
      return true;
    }

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
        for (int i = 0 ; i < results.Rows.Count ; i++)
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

  }
}
