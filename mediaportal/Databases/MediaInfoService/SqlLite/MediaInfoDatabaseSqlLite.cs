using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Reflection;
using System.Threading;
using SQLite.NET;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Database;
using MediaPortal.Services;
using MediaInfo;
using MediaInfo.Model;
using DirectShowLib;
using DirectShowLib.Dvd;
using Newtonsoft.Json;

namespace MediaPortal.MediaInfoService.Database
{
  public class MediaInfoDatabaseSqlLite
  {
    private const int _MAINTENANCE_PERIOD = 24; //in hours

    private enum VideoFormatEnum
    {
      Unknown,
      Unsupported,
      DVD,
      Bluray,
      Video,
      Audio,
      Picture,
      Image,
      AudioCD,
    }

    private static readonly Dictionary<string, string[]> _TagMappings = new Dictionary<string, string[]>()
        {
            { "Title", new string[] {"General_Title" } },
            { "Description", new string[] {"General_Description" } },
            { "Keywords", new string[] {"General_Keywords" } },
            { "Country", new string[] {"General_Country" } },
            { "ReleasedDate", new string[] {"General_Released_Date" } },
            { "EncodedDate", new string[] {"General_Encoded_Date" } },
            { "TaggedDate", new string[] {"General_Tagged_Date" } },
            { "Comment", new string[] {"General_Comment" } },
            { "Rating", new string[] {"General_Rating" } },
            { "Copyright", new string[] {"General_Copyright" } },
            { "Publisher", new string[] {"General_Publisher" } },
            { "PublisherUrl", new string[] {"General_Publisher_URL" } },
            { "DistributedBy", new string[] {"General_DistributedBy" } },
            { "Bpm", new string[] {"General_BPM" } },

            { "Collection", new string[]{"General_Collection" } },
            { "Season", new string[]{"General_Season" } },
            { "Part", new string[]{"General_Part" }},
            { "Movie", new string[] {"General_Movie", "General_Title"} },
            { "Chapter", new string[] { "General_Chapter", "General_Title" } },
            { "OriginalMovie", new string[] { "General_Original_Movie" } },
            { "TrackPosition", new string[] { "General_Track_Position" } },
            { "Composer", new string[] { "General_Composer" } },
            { "ComposerNationality", new string[] { "General_Composer_Nationality" } },
            { "Arranger", new string[] { "General_Arranger" } },
            { "Lyricist", new string[] { "General_Lyricist" } },
            { "Conductor", new string[] { "General_Conductor" } },
            { "SoundEngineer", new string[] { "General_SoundEngineer" } },
            { "Actor", new string[] { "General_Actor" } },
            { "ActorCharacter", new string[] { "General_Actor_Character" } },
            { "WrittenBy", new string[] { "General_WrittenBy" } },
            { "ScreenplayBy", new string[] { "General_ScreenplayBy" } },
            { "Director", new string[] { "General_Director" } },
            { "AssistantDirector", new string[] { "General_AssistantDirector" } },
            { "DirectorOfPhotography", new string[] { "General_DirectorOfPhotography" } },
            { "ArtDirector", new string[] { "General_ArtDirector" } },
            { "EditedBy", new string[] { "General_EditedBy" } },
            { "Producer", new string[] { "General_Producer" } },
            { "CoProducer", new string[] { "General_CoProducer" } },
            { "ExecutiveProducer", new string[] { "General_ExecutiveProducer" } },
            { "ProductionDesigner", new string[] { "General_ProductionDesigner" } },
            { "CostumeDesigner", new string[] { "General_CostumeDesigner" } },
            { "Choreographer", new string[] { "General_Choreographer" } },
            { "ProductionStudio", new string[] { "General_ProductionStudio" } },
            { "WrittenDate", new string[] { "General_Written_Date" } },
            { "Genre", new string[] { "General_Genre" } },
            { "Mood", new string[] { "General_Mood" } },
            { "EncodedApplication", new string[] { "General_Encoded_Application" } },
            { "EncodedLibrary", new string[] { "Video_Encoded_Library", "General_Encoded_Library" } },
            { "EncodedLibrarySettings", new string[] { "Video_Encoded_Library_Settings", "General_Encoded_Library_Settings" } },
            { "Summary", new string[] { "General_Summary" } },

            { "Album", new string[] { "General_Album", "General_Title", "Audio_Title" } },
            { "Track", new string[] {"General_Track", "General_Label", "General_Title", "Audio_Title" } },
            { "SubTrack", new string[] { "General_SubTrack" } },
            { "OriginalAlbum", new string[] { "General_Original_Album" } },
            { "OriginalTrack", new string[] { "General_Original_Track" } },
            { "TotalTracks", new string[] { "General_Track_Position_Total" } },
            { "DiscNumber", new string[] { "General_Part_Position" } },
            { "TotalDiscs", new string[] { "General_Part_Position_Total" } },
            { "Artist", new string[] { "General_Performer", "General_Album_Performer" } },
            { "AlbumArtist", new string[] { "General_Album_Performer" } },
            { "ArtistUrl", new string[] { "General_Performer_Url" } },
            { "Accompaniment", new string[] { "General_Accompaniment" } },
            { "MasteredBy", new string[] { "General_MasteredBy" } },
            { "RemixedBy", new string[] { "General_RemixedBy" } },
            { "Label", new string[] { "General_Label" } },
            { "RecordedDate", new string[] { "General_Recorded_Date" } },
            { "Isrc", new string[] { "General_ISRC" } },
            { "BarCode", new string[] { "General_BarCode" } },
            { "Lccn", new string[] { "General_LCCN" } },
            { "CatalogNumber", new string[] { "General_CatalogNumber" } },
            { "LabelCode", new string[] { "General_LabelCode" } },
            { "EncodedBy", new string[] { "General_EncodedBy" } },
    };

    private SQLiteClient _db;
    private bool _dbHealth = false;
    private DateTime _MaintenanceLast = DateTime.MinValue;


    public bool EnabledCachingForBluray { get; set; }
    public bool EnabledCachingForDVD { get; set; }
    public bool EnabledCachingForVideo { get; set; }
    public bool EnabledCachingForAudio { get; set; }
    public bool EnabledCachingForPicture { get; set; }
    public bool EnabledCachingForImage { get; set; }
    public bool EnabledCachingForAudioCD { get; set; }

    public int RecordLifeTime { get; set; }

    public MediaInfoDatabaseSqlLite()
    {
      this.EnabledCachingForBluray = true;
      this.EnabledCachingForDVD = true;
      this.EnabledCachingForVideo = true;
      this.EnabledCachingForAudio = true;
      this.EnabledCachingForPicture = true;
      this.EnabledCachingForImage = true;
      this.EnabledCachingForAudioCD = true;

      this.RecordLifeTime = 0;

      this.open();
    }

    public MediaInfoWrapper Get(string strMediaFullPath)
    {
      Log.Debug("[MediaInfoDatabaseSqlLite][Get] File fullpath: '{0}'", strMediaFullPath);

      if (string.IsNullOrWhiteSpace(strMediaFullPath) || !File.Exists(strMediaFullPath))
        return null;

      ILogger logger = GlobalServiceProvider.Get<ILogger>();

      bool bUseDb;
      VideoFormatEnum videoType = getVideoType(strMediaFullPath);
      switch (videoType)
      {
        case VideoFormatEnum.Bluray:
          bUseDb = this.EnabledCachingForBluray;
          break;

        case VideoFormatEnum.DVD:
          bUseDb = this.EnabledCachingForDVD;
          break;

        case VideoFormatEnum.Video:
          bUseDb = this.EnabledCachingForVideo;
          break;

        case VideoFormatEnum.Audio:
          bUseDb = this.EnabledCachingForAudio;
          break;

        case VideoFormatEnum.Picture:
          bUseDb = this.EnabledCachingForPicture;
          break;

        case VideoFormatEnum.Image:
          bUseDb = this.EnabledCachingForImage;
          break;

        case VideoFormatEnum.AudioCD:
          bUseDb = this.EnabledCachingForAudioCD;
          break;

        default:
          bUseDb = false;
          break;
      }

      if (!bUseDb)
        return new MediaInfoWrapper(strMediaFullPath, logger);

      bool bLocked = false;
      Monitor.Enter(typeof(MediaInfoDatabaseSqlLite), ref bLocked);
      try
      {
        if (this._db == null)
          return null;

        Monitor.Exit(typeof(MediaInfoDatabaseSqlLite));
        bLocked = false;

        this.doMaintenance();

        //Try get serial number
        string strSerial = getVideoSerialNumber(strMediaFullPath, ref videoType);
        if (!string.IsNullOrWhiteSpace(strSerial))
        {
          string strSQL;
          SQLiteResultSet result;

          string strKeyFile;
          if (videoType == VideoFormatEnum.Bluray || videoType == VideoFormatEnum.DVD)
            strKeyFile = Path.GetFileName(strMediaFullPath).ToLower();
          else
            strKeyFile = strMediaFullPath;

          //Try get existing MediaInfo from the DB
          strSQL = string.Format("SELECT * FROM files WHERE fullPath='{0}' AND serial='{1}' AND type='{2}'",
                sanitySqlValue(strKeyFile), strSerial, videoType);

          Monitor.Enter(typeof(MediaInfoDatabaseSqlLite), ref bLocked);

          MediaInfoWrapper mi;
          result = this._db.Execute(strSQL);
          if (result != null && result.Rows.Count > 0)
          {
            try
            {
              //Found
              mi = new MediaInfoWrapperEx(
                  this._db,
                  result,
                  strMediaFullPath,
                  DatabaseUtility.GetAsInt64(result, 0, "size"),
                  logger);

              //Update last acces ts
              strSQL = string.Format("UPDATE files SET accessLast={0} WHERE id={1}",
                DateTime.Now.ToUniversalTime().Ticks, DatabaseUtility.GetAsInt(result, 0, "id"));
              this._db.Execute(strSQL);

              return mi;
            }
            catch (Exception ex)
            {
              Log.Error("[MediaInfoDatabaseSqlLite][Get] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);

              return new MediaInfoWrapper(strMediaFullPath, logger);
            }
          }

          Monitor.Exit(typeof(MediaInfoDatabaseSqlLite));
          bLocked = false;

          //Create mediainfo
          mi = new MediaInfoWrapper(strMediaFullPath, logger);
          if (mi.Success)
          {
            int iIdFile = -1;
            int iStreamPack;
            List<MediaStream> list;
            CultureInfo ciEn = CultureInfo.GetCultureInfo("en-US");
            Monitor.Enter(typeof(MediaInfoDatabaseSqlLite), ref bLocked);
            try
            {
              #region Store MediaInfo into the database
              long lDate = DateTime.Now.ToUniversalTime().Ticks;
              strSQL = String.Format("INSERT INTO files (id, created, accessLast, fullPath, type, serial, size, format, formatVersion, isStreamable, writingApplication, writingLibrary, attachments, profile, codec, scanType, aspectRatio, duration)" +
                  " VALUES(null, {0},{1},'{2}','{3}','{4}',{5},'{6}','{7}',{8},'{9}','{10}','{11}','{12}','{13}','{14}','{15}',{16})",
                 lDate,
                 lDate,
                 sanitySqlValue(strKeyFile),
                 videoType,
                 strSerial,
                 mi.Size,
                 mi.Format,
                 mi.FormatVersion,
                 mi.IsStreamable ? 1 : 0,
                 sanitySqlValue(mi.WritingApplication),
                 sanitySqlValue(mi.WritingLibrary),
                 sanitySqlValue(mi.Attachments),
                 mi.Profile,
                 mi.Codec,
                 mi.ScanType,
                 mi.AspectRatio,
                 mi.Duration
                 );
              result = this._db.Execute(strSQL);
              iIdFile = this._db.LastInsertID();

              #region VideoStreams
              list = mi.VideoStreams.Cast<MediaStream>().ToList();
              for (int i = 0; i < mi.VideoStreams.Count; i++)
              {
                VideoStream s = mi.VideoStreams[i];
                iStreamPack = getEqualStreamCount(list, i);
                strSQL = String.Format("INSERT INTO videoStreams (id, idFile, streamId, streamName, streamPosition, streamNumber, streamPack, language, lcid, flagDefault, flagForced, streamSize, frameRate, frameRateMode, width, height, bitRate, aspectRatio, interlaced, stereoMode, format, codec, codecName, codecProfile, videoStandard, colorSpace, transferCharacteristics, chromaSubSampling, bitDepth, duration, hdr)" +
                    " VALUES(null, {0},{1},'{2}',{3},{4},{5},'{6}',{7},{8},{9},{10},'{11}','{12}',{13},{14},'{15}','{16}',{17},'{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}',{27},{28},'{29}')",
                    iIdFile,
                    s.Id,
                    sanitySqlValue(s.Name),
                    s.StreamPosition,
                    s.StreamNumber,
                    iStreamPack,
                    s.Language,
                    s.Lcid,
                    s.Default ? 1 : 0,
                    s.Forced ? 1 : 0,
                    s.StreamSize,
                    s.FrameRate.ToString(ciEn),
                    s.FrameRateMode,
                    s.Width,
                    s.Height,
                    s.Bitrate,
                    s.AspectRatio,
                    s.Interlaced ? 1 : 0,
                    s.Stereoscopic,
                    s.Format,
                    s.Codec,
                    s.CodecName,
                    s.CodecProfile,
                    s.Standard,
                    s.ColorSpace,
                    s.TransferCharacteristics,
                    s.SubSampling,
                    s.BitDepth,
                    s.Duration.Ticks,
                    s.Hdr
                    );
                this._db.Execute(strSQL);
                int iIdStream = this._db.LastInsertID();
                i += iStreamPack;
                this.insertTags(s.Tags, iIdFile, "videoStreams", iIdStream);
              }
              #endregion

              #region AudioStreams
              list = mi.AudioStreams.Cast<MediaStream>().ToList();
              for (int i = 0; i < mi.AudioStreams.Count; i++)
              {
                AudioStream s = mi.AudioStreams[i];
                iStreamPack = getEqualStreamCount(list, i);
                strSQL = String.Format("INSERT INTO audioStreams (id, idFile, streamId, streamName, streamPosition, streamNumber, streamPack, language, lcid, flagDefault, flagForced, streamSize, format, bitRate, bitDepth, bitRateMode, codec, codecName, codecDescription, duration, channel, samplingRate)" +
                    " VALUES(null, {0},{1},'{2}',{3},{4},{5},'{6}',{7},{8},{9},{10},'{11}','{12}',{13},'{14}','{15}','{16}','{17}',{18},{19},'{20}')",
                    iIdFile,
                    s.Id,
                    sanitySqlValue(s.Name),
                    s.StreamPosition,
                    s.StreamNumber,
                    iStreamPack,
                    s.Language,
                    s.Lcid,
                    s.Default ? 1 : 0,
                    s.Forced ? 1 : 0,
                    s.StreamSize,
                    s.Format,
                    s.Bitrate.ToString(ciEn),
                    s.BitDepth,
                    s.BitrateMode,
                    s.Codec,
                    s.CodecName,
                    sanitySqlValue(s.CodecDescription),
                    s.Duration.Ticks,
                    s.Channel,
                    s.SamplingRate.ToString(ciEn)
                    );
                this._db.Execute(strSQL);
                int iIdStream = this._db.LastInsertID();
                i += iStreamPack;
                this.insertTags(s.Tags, iIdFile, "audioStreams", iIdStream);
              }
              #endregion

              #region Subtitles
              list = mi.Subtitles.Cast<MediaStream>().ToList();
              for (int i = 0; i < mi.Subtitles.Count; i++)
              {
                SubtitleStream s = mi.Subtitles[i];
                iStreamPack = getEqualStreamCount(list, i);
                strSQL = String.Format("INSERT INTO subtitleStreams (id, idFile, streamId, streamName, streamPosition, streamNumber, streamPack, language, lcid, flagDefault, flagForced, streamSize, format, codec)" +
                    " VALUES(null, {0},{1},'{2}',{3},{4},{5},'{6}',{7},{8},{9},{10},'{11}','{12}')",
                    iIdFile,
                    s.Id,
                    sanitySqlValue(s.Name),
                    s.StreamPosition,
                    s.StreamNumber,
                    iStreamPack,
                    s.Language,
                    s.Lcid,
                    s.Default ? 1 : 0,
                    s.Forced ? 1 : 0,
                    s.StreamSize,
                    s.Format,
                    s.Codec
                    );
                this._db.Execute(strSQL);
                i += iStreamPack;
              }
              #endregion

              #region Chapters
              list = mi.Chapters.Cast<MediaStream>().ToList();
              for (int i = 0; i < mi.Chapters.Count; i++)
              {
                ChapterStream s = mi.Chapters[i];
                iStreamPack = getEqualStreamCount(list, i);
                strSQL = String.Format("INSERT INTO chapterStreams (id, idFile, streamId, streamName, streamPosition, streamNumber, streamPack, offset, description)" +
                    " VALUES(null, {0},{1},'{2}',{3},{4},{5},'{6}','{7}')",
                    iIdFile,
                    s.Id,
                    sanitySqlValue(s.Name),
                    s.StreamPosition,
                    s.StreamNumber,
                    iStreamPack,
                    s.Offset.ToString(ciEn),
                    sanitySqlValue(s.Description)
                    );
                this._db.Execute(strSQL);
                i += iStreamPack;
              }
              #endregion

              #region MenuStreams
              for (int i = 0; i < mi.MenuStreams.Count; i++)
              {
                MenuStream s = mi.MenuStreams[i];
                strSQL = String.Format("INSERT INTO menuStreams (id, idFile, streamId, streamName, streamPosition, streamNumber, streamPack, duration)" +
                    " VALUES(null, {0},{1},'{2}',{3},{4},{5},{6})",
                    iIdFile,
                    s.Id,
                    sanitySqlValue(s.Name),
                    s.StreamPosition,
                    s.StreamNumber,
                    0,
                    s.Duration.Ticks
                    );
                this._db.Execute(strSQL);
                int iIdMenu = this._db.LastInsertID();

                foreach (Chapter ch in s.Chapters)
                {
                  strSQL = String.Format("INSERT INTO chapters (id, idFile, idStream, position, name) VALUES(null, {0},{1},{2},'{3}')",
                      iIdFile,
                      iIdMenu,
                      ch.Position.Ticks,
                      sanitySqlValue(ch.Name)
                      );
                  this._db.Execute(strSQL);
                }
              }
              #endregion

              #endregion

              Log.Debug("[MediaInfoDatabaseSqlLite][Get] Database record created for the file: '{0}'", strMediaFullPath);
              return mi;
            }
            catch (Exception ex)
            {
              Log.Error("[MediaInfoDatabaseSqlLite][Get] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
            }

            //Something went wrong. Delete the record from the DB
            if (iIdFile >= 0)
              this.deleteRecord(iIdFile);
          }
          else
            Log.Error("[MediaInfoDatabaseSqlLite][Get] Failed to retrieve MediaInfo for the file: '{0}'", strMediaFullPath);

          return mi;
        }
        else
          Log.Error("[MediaInfoDatabaseSqlLite][Get] Failed to retrieve serial number for the file: '{0}'", strMediaFullPath);
      }
      catch (Exception ex)
      {
        Log.Error("[MediaInfoDatabaseSqlLite][Get] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      finally
      {
        if (bLocked)
          Monitor.Exit(typeof(MediaInfoDatabaseSqlLite));
      }

      return null;
    }

    public void Clear()
    {
      lock (typeof(MediaInfoDatabaseSqlLite))
      {
        if (this._db == null)
          return;

        try
        {
          this._db.Execute("DELETE FROM files");
          this._db.Execute("DELETE FROM videoStreams");
          this._db.Execute("DELETE FROM audioStreams");
          this._db.Execute("DELETE FROM subtitleStreams");
          this._db.Execute("DELETE FROM chapterStreams");
          this._db.Execute("DELETE FROM menuStreams");
          this._db.Execute("DELETE FROM chapters");
          this._db.Execute("DELETE FROM tags");
        }
        catch (Exception ex)
        {
          Log.Error("[MediaInfoDatabaseSqlLite][Clear] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }

    private void open()
    {
      Log.Info("[MediaInfoDatabaseSqlLite][open] Opening the database...");
      lock (typeof(MediaInfoDatabaseSqlLite))
      {
        try
        {
          if (this._db != null)
          {
            Log.Info("[MediaInfoDatabaseSqlLite][open] Already opened.");
            return;
          }

          // Open database
          string strPath = Config.GetFolder(Config.Dir.Database);
          try
          {
            Directory.CreateDirectory(strPath);
          }
          catch (Exception ex)
          {
            Log.Error("[MediaInfoDatabaseSqlLite][open] Excetion: {0}", ex.Message);
          }

          this._db = new SQLiteClient(Config.GetFile(Config.Dir.Database, @"MediaInfoDatabaseV1.db3"));

          this._dbHealth = DatabaseUtility.IntegrityCheck(this._db);

          DatabaseUtility.SetPragmas(this._db);

          this.createTables();
          this.upgradeDatabase();

          Log.Info("[MediaInfoDatabaseSqlLite][open] Database opened.");
        }
        catch (Exception ex)
        {
          Log.Error("[MediaInfoDatabaseSqlLite][open] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
          this._db = null;
        }
      }
    }

    private void createTables()
    {
      if (this._db == null)
        return;

      #region Tables
      DatabaseUtility.AddTable(this._db, "files",
        "CREATE TABLE files (id INTEGER primary key, created DATE_TIME, accessLast DATE_TIME, fullPath TEXT, type ENUM, serial TEXT, size LONG, format TEXT, formatVersion TEXT, isStreamable BOOL, writingApplication TEXT, writingLibrary TEXT, attachments TEXT, profile TEXT, codec TEXT, scanType TEXT, aspectRatio TEXT, duration INTEGER)");

      DatabaseUtility.AddTable(this._db, "videoStreams",
        "CREATE TABLE videoStreams (id INTEGER primary key, idFile INTEGER, streamId INTEGER, streamName TEXT, streamPosition INTEGER, streamNumber INTEGER, streamPack INTEGER, language TEXT, lcid INTEGER, flagDefault BOOL, flagForced BOOL, streamSize LONG, frameRate REAL, frameRateMode ENUM, width INTEGER, height INTEGER, bitRate REAL, aspectRatio ENUM, interlaced BOOL, stereoMode ENUM, format TEXT, codec ENUM, codecName TEXT, codecProfile TEXT, videoStandard ENUM, colorSpace ENUM, transferCharacteristics ENUM, chromaSubSampling ENUM, bitDepth INTEGER, duration LONG, hdr ENUM)");

      DatabaseUtility.AddTable(this._db, "audioStreams",
        "CREATE TABLE audioStreams (id INTEGER primary key, idFile INTEGER, streamId INTEGER, streamName TEXT, streamPosition INTEGER, streamNumber INTEGER, streamPack INTEGER, language TEXT, lcid INTEGER, flagDefault BOOL, flagForced BOOL, streamSize LONG, format TEXT, bitRate REAL, bitDepth INTEGER, bitRateMode ENUM, codec ENUM, codecName TEXT, codecDescription TEXT, duration LONG, channel INTEGER, samplingRate REAL)");

      DatabaseUtility.AddTable(this._db, "subtitleStreams",
        "CREATE TABLE subtitleStreams (id INTEGER primary key, idFile INTEGER, streamId INTEGER, streamName TEXT, streamPosition INTEGER, streamNumber INTEGER, streamPack INTEGER, language TEXT, lcid INTEGER, flagDefault BOOL, flagForced BOOL, streamSize LONG, format TEXT, codec ENUM)");

      DatabaseUtility.AddTable(this._db, "chapterStreams",
        "CREATE TABLE chapterStreams (id INTEGER primary key, idFile INTEGER, streamId INTEGER, streamName TEXT, streamPosition INTEGER, streamNumber INTEGER, streamPack INTEGER, offset REAL, description TEXT)");

      DatabaseUtility.AddTable(this._db, "menuStreams",
        "CREATE TABLE menuStreams (id INTEGER primary key, idFile INTEGER, streamId INTEGER, streamName TEXT, streamPosition INTEGER, streamNumber INTEGER, streamPack INTEGER, duration LONG)");

      DatabaseUtility.AddTable(this._db, "chapters",
        "CREATE TABLE chapters (id INTEGER primary key, idFile INTEGER, idStream INTEGER, position LONG, name TEXT)");

      DatabaseUtility.AddTable(this._db, "tags",
        "CREATE TABLE tags (id INTEGER primary key, idFile INTEGER, streamTable TEXT, streamId INTEGER, tagId TEXT, tagValue TEXT)");
      #endregion

      #region Indexes
      DatabaseUtility.AddIndex(this._db, "idxFiles_search", "CREATE INDEX idxFiles_search ON files (fullPath ASC, serial ASC, type ASC)");
      DatabaseUtility.AddIndex(this._db, "idxFiles_created", "CREATE INDEX idxFiles_created ON files (created ASC)");
      DatabaseUtility.AddIndex(this._db, "idxFiles_accessLast", "CREATE INDEX idxFiles_accessLast ON files (accessLast ASC)");

      DatabaseUtility.AddIndex(this._db, "idxVideoStreams_idFile", "CREATE INDEX idxVideoStreams_idFile ON videoStreams (idFile ASC)");

      DatabaseUtility.AddIndex(this._db, "idxAudioStreams_idFile", "CREATE INDEX idxAudioStreams_idFile ON audioStreams (idFile ASC)");

      DatabaseUtility.AddIndex(this._db, "idxSubtitleStreams_idFile", "CREATE INDEX idxSubtitleStreams_idFile ON subtitleStreams (idFile ASC)");

      DatabaseUtility.AddIndex(this._db, "idxChapterStreams_idFile", "CREATE INDEX idxChapterStreams_idFile ON chapterStreams (idFile ASC)");

      DatabaseUtility.AddIndex(this._db, "idxMenuStreams_idFile", "CREATE INDEX idxMenuStreams_idFile ON menuStreams (idFile ASC)");

      DatabaseUtility.AddIndex(this._db, "idxChapters_idFile", "CREATE INDEX idxChapters_idFile ON chapters (idFile ASC)");
      DatabaseUtility.AddIndex(this._db, "idxChapters_idStream", "CREATE INDEX idxChapters_idStream ON chapters (idStream ASC)");

      DatabaseUtility.AddIndex(this._db, "idxTags_idFile", "CREATE INDEX idxTags_idFile ON tags (idFile ASC)");
      DatabaseUtility.AddIndex(this._db, "idxTags_search", "CREATE INDEX idxTags_search ON tags (streamTable ASC, streamId ASC)");
      #endregion
    }

    private void upgradeDatabase()
    {
      try
      {
        if (this._db == null)
          return;
      }
      catch (Exception ex)
      {
        Log.Error("[MediaInfoDatabaseSqlLite][upgradeDatabase] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private void deleteRecord(int iId)
    {
      if (iId < 1)
        return;

      Log.Debug("[MediaInfoDatabaseSqlLite][deleteRecord] ID:" + iId);

      try
      {
        this._db.Execute("DELETE FROM videoStreams WHERE idFile=" + iId);
        this._db.Execute("DELETE FROM audioStreams WHERE idFile=" + iId);
        this._db.Execute("DELETE FROM subtitleStreams WHERE idFile=" + iId);
        this._db.Execute("DELETE FROM chapterStreams WHERE idFile=" + iId);
        this._db.Execute("DELETE FROM menuStreams WHERE idFile=" + iId);
        this._db.Execute("DELETE FROM chapters WHERE idFile=" + iId);
        this._db.Execute("DELETE FROM tags WHERE idFile=" + iId);
        this._db.Execute("DELETE FROM files WHERE id=" + iId);
      }
      catch (Exception ex)
      {
        Log.Error("[MediaInfoDatabaseSqlLite][deleteRecord] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private void doMaintenance()
    {
      try
      {
        int iDays = this.RecordLifeTime;
        if (iDays > 0 && (DateTime.Now - this._MaintenanceLast).TotalHours >= _MAINTENANCE_PERIOD)
        {
          Log.Debug("[MediaInfoDatabaseSqlLite][doMaintenance] Run ...");
          SQLiteResultSet result = this._db.Execute("SELECT id FROM files WHERE accessLast <= " + DateTime.Now.ToUniversalTime().AddDays(iDays * -1).Ticks);
          if (result != null && result.Rows.Count > 0)
            result.Rows.ForEach(row => this.deleteRecord(int.Parse(row.fields[0])));

          this._MaintenanceLast = DateTime.Now;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MediaInfoDatabaseSqlLite][doMaintenance] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private static string getVideoSerialNumber(string strMediaFullPath, ref VideoFormatEnum videoType)
    {
      switch (videoType)
      {
        case VideoFormatEnum.DVD:
          string strVtsPath = strMediaFullPath.ToLower().Replace(@"\video_ts.ifo", @"\");
          try
          {
            IDvdInfo2 dvdInfo = (IDvdInfo2)new DVDNavigator();
            long lDiscID = 0;
            dvdInfo.GetDiscID(strVtsPath, out lDiscID);
            if (lDiscID != 0)
              return Convert.ToString(lDiscID, 16);
          }
          catch (Exception e)
          {
          }
          return null;

        case VideoFormatEnum.Bluray:
          int iIdx = filePathEndsWith(strMediaFullPath, @"\bdmv\index.bdmv");
          if (iIdx < 0)
            iIdx = filePathEndsWith(strMediaFullPath, @"\bdmv\stream\*.m2ts");
          if (iIdx < 0)
            iIdx = filePathEndsWith(strMediaFullPath, @"\bdmv\playlist\*.mpls");

          if (iIdx > 0)
          {
            //Retail disc only
            string strKeyFilePath = strMediaFullPath.Substring(0, iIdx) + @"\AACS\Unit_Key_RO.inf";
            if (File.Exists(strKeyFilePath))
              return getSHA1Hash(new FileInfo(strKeyFilePath));
            else
            {
              //Force to File type
              videoType = VideoFormatEnum.Video;
              return getPartialHash(new FileInfo(strMediaFullPath));
            }
          }
          return null;

        case VideoFormatEnum.Unknown:
        case VideoFormatEnum.Unsupported:
          return null;

        default:
          return getPartialHash(new FileInfo(strMediaFullPath));
      }
    }

    private static VideoFormatEnum getVideoType(string strMediaFullPath)
    {
      if (string.IsNullOrWhiteSpace(strMediaFullPath))
        return VideoFormatEnum.Unknown;

      if (strMediaFullPath.StartsWith(("http://"), StringComparison.OrdinalIgnoreCase)
          || strMediaFullPath.StartsWith(("https://"), StringComparison.OrdinalIgnoreCase)
          || strMediaFullPath.StartsWith(("rtsp://"), StringComparison.OrdinalIgnoreCase)
          || strMediaFullPath.StartsWith(("mms://"), StringComparison.OrdinalIgnoreCase))
        return VideoFormatEnum.Unsupported;

      //AudioCD
      if (filePathEndsWith(strMediaFullPath, @":\track*.cda") >= 0)
        return VideoFormatEnum.AudioCD;

      //DVD
      if (strMediaFullPath.EndsWith((@"\video_ts.ifo"), StringComparison.OrdinalIgnoreCase))
        return VideoFormatEnum.DVD;

      //Bluray
      if (strMediaFullPath.EndsWith((@"\bdmv\index.bdmv"), StringComparison.OrdinalIgnoreCase))
        return VideoFormatEnum.Bluray;

      if (filePathEndsWith(strMediaFullPath, @"\bdmv\stream\*.m2ts") >= 0)
        return VideoFormatEnum.Bluray;

      if (filePathEndsWith(strMediaFullPath, @"\bdmv\playlist\*.mpls") >= 0)
        return VideoFormatEnum.Bluray;

      string strExt = Path.GetExtension(strMediaFullPath).ToLowerInvariant();

      //All videos
      if (Util.Utils.VideoExtensions.Contains(strExt))
        return VideoFormatEnum.Video;

      //All audios
      if (Util.Utils.AudioExtensions.Contains(strExt))
        return VideoFormatEnum.Audio;

      //All pictures
      if (Util.Utils.PictureExtensions.Contains(strExt))
        return VideoFormatEnum.Picture;

      //All images
      if (Util.Utils.ImageExtensions.Contains(strExt))
        return VideoFormatEnum.Image;

      return VideoFormatEnum.Unsupported;
    }

    private static int filePathEndsWith(string strFilePath, string strPattern)
    {
      int iIdxFile = strFilePath.Length - 1;
      int iIdxPattern = strPattern.Length - 1;
      while (iIdxPattern >= 0 && iIdxFile >= 0)
      {
        if (strPattern[iIdxPattern] == '*')
        {
          if (iIdxPattern > 0 && char.ToLowerInvariant(strPattern[iIdxPattern - 1]) == char.ToLowerInvariant(strFilePath[iIdxFile]))
            iIdxPattern -= 2;
        }
        else if (char.ToLowerInvariant(strPattern[iIdxPattern]) == char.ToLowerInvariant(strFilePath[iIdxFile]))
          iIdxPattern--;
        else
          return -1; //no match

        iIdxFile--;
      }

      return iIdxFile + 1;
    }

    private static string getPartialHash(FileInfo fi)
    {
      try
      {
        using (Stream stream = fi.OpenRead())
        {
          const int BLOCK_SIZE = 65536;
          int iOffset, iRd;
          ulong dwHash = (ulong)stream.Length;
          byte[] buffer = new byte[BLOCK_SIZE];
          while (true)
          {
            iOffset = 0;
            iRd = stream.Read(buffer, 0, BLOCK_SIZE);

            while (iRd >= sizeof(long))
            {
              unchecked { dwHash += BitConverter.ToUInt64(buffer, iOffset); }
              iRd -= sizeof(long);
              iOffset += sizeof(long);
            }

            if (stream.Position >= stream.Length)
              break;

            //Last 64kb of the file
            stream.Position = Math.Max(0, stream.Length - BLOCK_SIZE);
          }

          return dwHash.ToString("x16");
        }
      }
      catch (Exception ex)
      {
        Log.Error("[MediaInfoDatabaseSqlLite][getPartialHash] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return null;
    }

    private static string getSHA1Hash(FileInfo fi)
    {
      string strHashHex = null;
      if (fi.Exists)
      {
        Stream stream = null;
        try
        {
          stream = fi.OpenRead();
          HashAlgorithm hashObj = new SHA1Managed();
          byte[] hash = hashObj.ComputeHash(stream);
          strHashHex = printHex(hash);
        }
        catch
        {
        }
        finally
        {
          if (stream != null)
            stream.Close();
        }
      }

      return strHashHex;
    }

    private static string printHex(byte[] data)
    {
      if (data == null || data.Length == 0)
        return string.Empty;

      int i;
      StringBuilder sb = new StringBuilder(data.Length * 2);
      for (int iIdx = 0; iIdx < data.Length; iIdx++)
      {
        i = data[iIdx] >> 4;
        sb.Append((char)((i > 9 ? 'W' : '0') + i));
        i = data[iIdx] & 15;
        sb.Append((char)((i > 9 ? 'W' : '0') + i));
      }
      return sb.ToString();
    }

    private static string sanitySqlValue(string strValue)
    {
      if (string.IsNullOrWhiteSpace(strValue))
        return string.Empty;

      if (strValue.Contains('\''))
        return strValue.Replace("'", "''");
      else
        return strValue;
    }

    private static int getEqualStreamCount(IList<MediaStream> list, int iIdxCurrent)
    {
      //Get number of consecutive equal streams from the given position

      int iResult = 0;
      MediaStream msCurrent = list[iIdxCurrent++];
      PropertyInfo[] props = msCurrent.GetType().GetProperties().Where(p =>
        p.CanWrite && p.Name != "StreamNumber" && p.Name != "StreamPosition" && p.Name != "Tags").ToArray();

      while (iIdxCurrent < list.Count)
      {
        MediaStream msNext = list[iIdxCurrent++];
        if (msCurrent.StreamNumber + 1 != msNext.StreamNumber
            || msCurrent.StreamPosition + 1 != msNext.StreamPosition)
          return iResult;

        for (int i = 0; i < props.Length; i++)
        {
          PropertyInfo pi = props[i];

          if (!pi.GetValue(msCurrent, null).Equals(pi.GetValue(msNext, null)))
            return iResult;
        }

        msCurrent = msNext;
        iResult++;
      }

      return iResult;
    }

    private void insertTags(object o, int iIdFile, string strStreamTable, int iIdStream)
    {
      //Get all needed tags and put them into the database

      List<string> tagNames = new List<string>();
      PropertyInfo[] props = o.GetType().GetProperties();

      //Proceed with each public property
      for (int i = 0; i < props.Length; i++)
      {
        PropertyInfo p = props[i];
        object oValue = p.GetValue(o, null);
        if (oValue != null)
        {
          string strName = p.Name;

          if (p.PropertyType.Name == "IEnumerable`1")
          {
            Type tKey = p.PropertyType.GetGenericArguments()[0];
            if (tKey != typeof(CoverInfo))
              continue;

            //Serialize CoverInfo
            string strValue = JsonConvert.SerializeObject(oValue);
            if (!string.IsNullOrWhiteSpace(strValue) && strValue.Length > 2)
            {
              string strSQL = String.Format("INSERT INTO tags (id, idFile, streamTable, streamId, tagId, tagValue) VALUES(null, {0},'{1}','{2}','{3}','{4}:{5}')",
                  iIdFile,
                  strStreamTable,
                  iIdStream,
                  "CoverInfo",
                  tKey.FullName, strValue
              );
              this._db.Execute(strSQL);
            }
          }
          else
          {
            if (_TagMappings.TryGetValue(strName, out string[] tags))
            {
              for (int iT = 0; iT < tags.Length; iT++)
              {
                string strTag = tags[iT];
                if (!tagNames.Exists(t => t.Equals(strTag)))
                  tagNames.Add(strTag);
              }
            }
          }
        }
      }

      //Prepare dictionaries
      IDictionary dictGeneral = (IDictionary)typeof(BaseTags).GetProperty("GeneralTags", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o, null);
      Type tKeyGeneral = dictGeneral.GetType().GetInterface("IDictionary`2").GetGenericArguments()[0];
      IDictionary dictVideo = null;
      Type tKeyVideo = null;
      IDictionary dictAudio = null;
      Type tKeyAudio = null;
      if (o is VideoTags)
      {
        dictVideo = (IDictionary)typeof(VideoTags).GetProperty("VideoDataTags", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o, null);
        tKeyVideo = dictVideo.GetType().GetInterface("IDictionary`2").GetGenericArguments()[0];
      }
      else if (o is AudioTags)
      {
        dictAudio = (IDictionary)typeof(AudioTags).GetProperty("AudioDataTags", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o, null);
        tKeyAudio = dictAudio.GetType().GetInterface("IDictionary`2").GetGenericArguments()[0];
      }

      //Place each tag into the database
      tagNames.ForEach(strTagName =>
      {
        object oValue = null;

        if (strTagName.StartsWith("General_"))
        {
          object oKey = Enum.Parse(tKeyGeneral, strTagName);
          if (dictGeneral.Contains(oKey))
            oValue = dictGeneral[oKey];
        }
        else if (strTagName.StartsWith("Video_"))
        {
          object oKey = Enum.Parse(tKeyVideo, strTagName);
          if (dictVideo.Contains(oKey))
            oValue = dictVideo[oKey];
        }
        else if (strTagName.StartsWith("Audio_"))
        {
          object oKey = Enum.Parse(tKeyAudio, strTagName);
          if (dictAudio.Contains(oKey))
            oValue = dictAudio[oKey];
        }

        if (oValue != null)
        {
          string strSQL = String.Format("INSERT INTO tags (id, idFile, streamTable, streamId, tagId, tagValue) VALUES(null, {0},'{1}','{2}','{3}','{4}:{5}')",
            iIdFile,
            strStreamTable,
            iIdStream,
            strTagName,
            oValue.GetType().FullName, sanitySqlValue(oValue.ToString())
            );
          this._db.Execute(strSQL);
        }
      });
    }
  }
}
