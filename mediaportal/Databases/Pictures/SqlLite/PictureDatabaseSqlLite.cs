#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using SQLite.NET;

using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Player;
using MediaPortal.Util;

namespace MediaPortal.Picture.Database
{
  /// <summary>
  /// Summary description for PictureDatabaseSqlLite.
  /// </summary>
  public class PictureDatabaseSqlLite : IPictureDatabase, IDisposable
  {
    private bool disposed = false;
    private SQLiteClient m_db = null;
    private bool _useExif = true;
    private bool _usePicasa = false;
    private bool _dbHealth = false;

    public PictureDatabaseSqlLite()
    {
      Open();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void Open()
    {
      try
      {
        // Maybe called by an exception
        if (m_db != null)
        {
          try
          {
            m_db.Close();
            m_db.Dispose();
            m_db = null;
            Log.Warn("Picture.DB.SQLite: Disposing current DB instance..");
          }
          catch (Exception ex)
          {
            Log.Error("Picture.DB.SQLite: Open: {0}", ex.Message);
          }
        }

        // Open database
        try
        {
          Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Create DB directory: {0}", ex.Message);
        }

        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "PictureDatabaseV3.db3"));
        // Retry 10 times on busy (DB in use or system resources exhausted)
        m_db.BusyRetries = 10;
        // Wait 100 ms between each try (default 10)
        m_db.BusyRetryDelay = 100;

        _dbHealth = DatabaseUtility.IntegrityCheck(m_db);

        DatabaseUtility.SetPragmas(m_db);
        m_db.Execute("PRAGMA foreign_keys=ON");
        m_db.Execute("PRAGMA optimize;");

        CreateTables();
        InitSettings();
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: Open DB: {0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      Log.Info("Picture database opened...");
    }

    private void InitSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _useExif = xmlreader.GetValueAsBool("pictures", "useExif", true);
        _usePicasa = xmlreader.GetValueAsBool("pictures", "usePicasa", false);
      }
    }

    private bool CreateTables()
    {
      if (m_db == null)
      {
        return false;
      }

      #region Tables
      DatabaseUtility.AddTable(m_db, "picture",
                               "CREATE TABLE picture (idPicture INTEGER PRIMARY KEY, strFile TEXT, iRotation INTEGER, strDateTaken TEXT, " +
                                                      "iImageWidth INTEGER, iImageHeight INTEGER, " +
                                                      "iImageXReso INTEGER, iImageYReso INTEGER);");
      #endregion

      #region Indexes
      DatabaseUtility.AddIndex(m_db, "idxpicture_strFile", "CREATE INDEX idxpicture_strFile ON picture (strFile ASC)");
      DatabaseUtility.AddIndex(m_db, "idxpicture_strDateTaken", "CREATE INDEX idxpicture_strDateTaken ON picture (strDateTaken ASC)");
      DatabaseUtility.AddIndex(m_db, "idxpicture_strDateTaken_Year", "CREATE INDEX idxpicture_strDateTaken_Year ON picture (SUBSTR(strDateTaken,1,4))");
      DatabaseUtility.AddIndex(m_db, "idxpicture_strDateTaken_Month", "CREATE INDEX idxpicture_strDateTaken_Month ON picture (SUBSTR(strDateTaken,6,2))");
      DatabaseUtility.AddIndex(m_db, "idxpicture_strDateTaken_Day", "CREATE INDEX idxpicture_strDateTaken_Day ON picture (SUBSTR(strDateTaken,9,2))");
      #endregion

      #region Exif Tables
      DatabaseUtility.AddTable(m_db, "camera",
                               "CREATE TABLE camera (idCamera INTEGER PRIMARY KEY, strCamera TEXT, strCameraMake TEXT);");
      DatabaseUtility.AddTable(m_db, "lens",
                               "CREATE TABLE lens (idLens INTEGER PRIMARY KEY, strLens TEXT, strLensMake TEXT);");
      DatabaseUtility.AddTable(m_db, "orientation",
                               "CREATE TABLE orientation (idOrientation INTEGER PRIMARY KEY, strOrientation TEXT);");
      DatabaseUtility.AddTable(m_db, "flash",
                               "CREATE TABLE flash (idFlash INTEGER PRIMARY KEY, strFlash TEXT);");
      DatabaseUtility.AddTable(m_db, "meteringmode",
                               "CREATE TABLE meteringmode (idMeteringMode INTEGER PRIMARY KEY, strMeteringMode TEXT);");
      DatabaseUtility.AddTable(m_db, "country",
                               "CREATE TABLE country (idCountry INTEGER PRIMARY KEY, strCountryCode TEXT, strCountry TEXT);");
      DatabaseUtility.AddTable(m_db, "state",
                               "CREATE TABLE state (idState INTEGER PRIMARY KEY, strState TEXT);");
      DatabaseUtility.AddTable(m_db, "city",
                               "CREATE TABLE city (idCity INTEGER PRIMARY KEY, strCity TEXT);");
      DatabaseUtility.AddTable(m_db, "sublocation",
                               "CREATE TABLE sublocation (idSublocation INTEGER PRIMARY KEY, strSublocation TEXT);");
      DatabaseUtility.AddTable(m_db, "exposureprogram",
                               "CREATE TABLE exposureprogram (idExposureProgram INTEGER PRIMARY KEY, strExposureProgram TEXT);");
      DatabaseUtility.AddTable(m_db, "exposuremode",
                               "CREATE TABLE exposuremode (idExposureMode INTEGER PRIMARY KEY, strExposureMode TEXT);");
      DatabaseUtility.AddTable(m_db, "sensingmethod",
                               "CREATE TABLE sensingmethod (idSensingMethod INTEGER PRIMARY KEY, strSensingMethod TEXT);");
      DatabaseUtility.AddTable(m_db, "scenetype",
                               "CREATE TABLE scenetype (idSceneType INTEGER PRIMARY KEY, strSceneType TEXT);");
      DatabaseUtility.AddTable(m_db, "scenecapturetype",
                               "CREATE TABLE scenecapturetype (idSceneCaptureType INTEGER PRIMARY KEY, strSceneCaptureType TEXT);");
      DatabaseUtility.AddTable(m_db, "whitebalance",
                               "CREATE TABLE whitebalance (idWhiteBalance INTEGER PRIMARY KEY, strWhiteBalance TEXT);");
      DatabaseUtility.AddTable(m_db, "author",
                               "CREATE TABLE author (idAuthor INTEGER PRIMARY KEY, strAuthor TEXT);");
      DatabaseUtility.AddTable(m_db, "byline",
                               "CREATE TABLE byline (idByline INTEGER PRIMARY KEY, strByline TEXT);");
      DatabaseUtility.AddTable(m_db, "software",
                               "CREATE TABLE software (idSoftware INTEGER PRIMARY KEY, strSoftware TEXT);");
      DatabaseUtility.AddTable(m_db, "usercomment",
                               "CREATE TABLE usercomment (idUserComment INTEGER PRIMARY KEY, strUserComment TEXT);");
      DatabaseUtility.AddTable(m_db, "copyright",
                               "CREATE TABLE copyright (idCopyright INTEGER PRIMARY KEY, strCopyright TEXT);");
      DatabaseUtility.AddTable(m_db, "copyrightnotice",
                               "CREATE TABLE copyrightnotice (idCopyrightNotice INTEGER PRIMARY KEY, strCopyrightNotice TEXT);");
      DatabaseUtility.AddTable(m_db, "iso",
                               "CREATE TABLE iso (idISO INTEGER PRIMARY KEY, strISO TEXT);");
      DatabaseUtility.AddTable(m_db, "exposuretime",
                               "CREATE TABLE exposuretime (idExposureTime INTEGER PRIMARY KEY, strExposureTime TEXT);");
      DatabaseUtility.AddTable(m_db, "exposurecompensation",
                               "CREATE TABLE exposurecompensation (idExposureCompensation INTEGER PRIMARY KEY, strExposureCompensation TEXT);");
      DatabaseUtility.AddTable(m_db, "fstop",
                               "CREATE TABLE fstop (idFStop INTEGER PRIMARY KEY, strFStop TEXT);");
      DatabaseUtility.AddTable(m_db, "shutterspeed",
                               "CREATE TABLE shutterspeed (idShutterSpeed INTEGER PRIMARY KEY, strShutterSpeed TEXT);");
      DatabaseUtility.AddTable(m_db, "focallength",
                               "CREATE TABLE focallength (idFocalLength INTEGER PRIMARY KEY, strFocalLength TEXT);");
      DatabaseUtility.AddTable(m_db, "focallength35mm",
                               "CREATE TABLE focallength35mm (idFocalLength35mm INTEGER PRIMARY KEY, strFocalLength35mm TEXT);");

      DatabaseUtility.AddTable(m_db, "gpslocation",
                                     "CREATE TABLE gpslocation (idGPSLocation INTEGER PRIMARY KEY, latitude REAL NOT NULL, longitude REAL NOT NULL, altitude REAL);");

      DatabaseUtility.AddTable(m_db, "keyword",
                               "CREATE TABLE keyword (idKeyword INTEGER PRIMARY KEY, strKeyword TEXT);");
      DatabaseUtility.AddTable(m_db, "keywordslinkpicture",
                               "CREATE TABLE keywordslinkpicture (idKeyword INTEGER REFERENCES keyword(idKeyword) ON DELETE CASCADE, idPicture INTEGER REFERENCES picture(idPicture) ON DELETE CASCADE," +
                                 "PRIMARY KEY (idKeyword, idPicture));");

      DatabaseUtility.AddTable(m_db, "exifdata",
                               "CREATE TABLE exifdata (idPicture INTEGER PRIMARY KEY REFERENCES picture(idPicture) ON DELETE CASCADE, " +
                                                       "idCamera INTEGER REFERENCES camera(idCamera) ON DELETE SET NULL, " +
                                                       "idLens INTEGER REFERENCES lens(idLens) ON DELETE SET NULL, " +
                                                       "idISO INTEGER REFERENCES iso(idIso) ON DELETE SET NULL, " +
                                                       "idExposureTime INTEGER REFERENCES exposuretime(idExposureTime) ON DELETE SET NULL, " +
                                                       "idExposureCompensation INTEGER REFERENCES exposurecompensation(idExposureCompensation) ON DELETE SET NULL, " +
                                                       "idFStop INTEGER REFERENCES fstop(idFStop) ON DELETE SET NULL, " +
                                                       "idShutterSpeed INTEGER REFERENCES shutterspeed(idShutterSpeed) ON DELETE SET NULL, " +
                                                       "idFocalLength INTEGER REFERENCES focallength(idFocalLength) ON DELETE SET NULL, " +
                                                       "idFocalLength35mm INTEGER REFERENCES focallength35mm(idFocalLength35mm) ON DELETE SET NULL, " +
                                                       "idGPSLocation INTEGER REFERENCES gpslocation(idGPSLocation) ON DELETE SET NULL, " +
                                                       "idOrientation INTEGER REFERENCES orientation(idOrientation) ON DELETE SET NULL, " +
                                                       "idFlash INTEGER REFERENCES flash(idFlash) ON DELETE SET NULL, " +
                                                       "idMeteringMode INTEGER REFERENCES meteringmode(idMeteringMode) ON DELETE SET NULL, " +
                                                       "idExposureProgram INTEGER REFERENCES exposureprogram(idExposureProgram) ON DELETE SET NULL, " +
                                                       "idExposureMode INTEGER REFERENCES exposuremode(idExposureMode) ON DELETE SET NULL, " +
                                                       "idSensingMethod INTEGER REFERENCES sensingmethod(idSensingMethod) ON DELETE SET NULL, " +
                                                       "idSceneType INTEGER REFERENCES scenetype(idSceneType) ON DELETE SET NULL, " +
                                                       "idSceneCaptureType INTEGER REFERENCES scenecapturetype(idSceneCaptureType) ON DELETE SET NULL, " +
                                                       "idWhiteBalance INTEGER REFERENCES whitebalance(idWhiteBalance) ON DELETE SET NULL," +
                                                       "idAuthor INTEGER REFERENCES author(idAuthor) ON DELETE SET NULL, " +
                                                       "idByline INTEGER REFERENCES byline(idByline) ON DELETE SET NULL, " +
                                                       "idSoftware INTEGER REFERENCES software(idSoftware) ON DELETE SET NULL, " +
                                                       "idUserComment INTEGER REFERENCES usercomment(idUserComment) ON DELETE SET NULL, " +
                                                       "idCopyright INTEGER REFERENCES copyright(idCopyright) ON DELETE SET NULL, " +
                                                       "idCopyrightNotice INTEGER REFERENCES copyrightnotice(idCopyrightNotice) ON DELETE SET NULL, " +
                                                       "idCountry INTEGER REFERENCES country(idCountry) ON DELETE SET NULL, " +
                                                       "idState INTEGER REFERENCES state(idState) ON DELETE SET NULL, " +
                                                       "idCity INTEGER REFERENCES city(idCity) ON DELETE SET NULL, " +
                                                       "idSublocation INTEGER REFERENCES sublocation(idSublocation) ON DELETE SET NULL);");
      #endregion

      #region Exif Indexes

      DatabaseUtility.AddIndex(m_db, "idxkeyword_strKeyword", "CREATE INDEX idxkeyword_strKeyword ON keyword(strKeyword);");

      DatabaseUtility.AddIndex(m_db, "idxkeywordslinkpicture_idPicture", "CREATE INDEX idxkeywordslinkpicture_idPicture ON keywordslinkpicture(idPicture);");

      DatabaseUtility.AddIndex(m_db, "idxexifdata_idCamera", "CREATE INDEX idxexifdata_idCamera ON exifdata(idCamera);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idLens", "CREATE INDEX idxexifdata_idLens ON exifdata(idLens);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idGPSLocation", "CREATE INDEX idxexifdata_idGPSLocation ON gpslocation(idGPSLocation);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idOrientation", "CREATE INDEX idxexifdata_idOrientation ON exifdata(idOrientation);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idFlash", "CREATE INDEX idxexifdata_idFlash ON exifdata(idFlash);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idMeteringMode", "CREATE INDEX idxexifdata_idMeteringMode ON exifdata(idMeteringMode);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idExposureProgram", "CREATE INDEX idxexifdata_idExposureProgram ON exifdata(idExposureProgram);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idExposureMode", "CREATE INDEX idxexifdata_idExposureMode ON exifdata(idExposureMode);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idSensingMethod", "CREATE INDEX idxexifdata_idSensingMethod ON exifdata(idSensingMethod);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idSceneType", "CREATE INDEX idxexifdata_idSceneType ON exifdata(idSceneType);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idSceneCaptureType", "CREATE INDEX idxexifdata_idSceneCaptureType ON exifdata(idSceneCaptureType);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idWhiteBalance", "CREATE INDEX idxexifdata_idWhiteBalance ON exifdata(idWhiteBalance);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idAuthor", "CREATE INDEX idxexifdata_idAuthor ON exifdata(idAuthor);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idByline", "CREATE INDEX idxexifdata_idByline ON exifdata(idByline);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idSoftware", "CREATE INDEX idxexifdata_idSoftware ON exifdata(idSoftware);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idUserComment", "CREATE INDEX idxexifdata_idUserComment ON exifdata(idUserComment);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idCopyright", "CREATE INDEX idxexifdata_idCopyright ON exifdata(idCopyright);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idCopyrightNotice", "CREATE INDEX idxexifdata_idCopyrightNotice ON exifdata(idCopyrightNotice);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idCountry", "CREATE INDEX idxexifdata_idCountry ON exifdata(idCountry);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idState", "CREATE INDEX idxexifdata_idState ON exifdata(idState);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idCity", "CREATE INDEX idxexifdata_idCity ON exifdata(idCity);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idSublocation", "CREATE INDEX idxexifdata_idSublocation ON exifdata(idSublocation);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idIso", "CREATE INDEX idxexifdata_idIso ON exifdata(idIso);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idExposureTime", "CREATE INDEX idxexifdata_idExposureTime ON exifdata(idExposureTime);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idExposureCompensation", "CREATE INDEX idxexifdata_idExposureCompensation ON exifdata(idExposureCompensation);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idFStop", "CREATE INDEX idxexifdata_idFStop ON exifdata(idFStop);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idShutterSpeed", "CREATE INDEX idxexifdata_idShutterSpeed ON exifdata(idShutterSpeed);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idFocalLength", "CREATE INDEX idxexifdata_idFocalLength ON exifdata(idFocalLength);");
      DatabaseUtility.AddIndex(m_db, "idxexifdata_idFocalLength35mm", "CREATE INDEX idxexifdata_idFocalLength35mm ON exifdata(idFocalLength35mm);");

      #endregion

      #region Exif Triggers
      DatabaseUtility.AddTrigger(m_db, "Delete_ExtraData",
            "CREATE TRIGGER Delete_ExtraData AFTER DELETE ON exifdata " +
            "BEGIN " +
            "  DELETE FROM camera WHERE idCamera NOT IN (SELECT DISTINCT idCamera FROM exifdata); " +
            "  DELETE FROM lens WHERE idLens NOT IN (SELECT DISTINCT idLens FROM exifdata); " +
            "  DELETE FROM gpslocation WHERE idGPSLocation NOT IN (SELECT DISTINCT idGPSLocation FROM exifdata); " +
            "  DELETE FROM orientation WHERE idOrientation NOT IN (SELECT DISTINCT idOrientation FROM exifdata); " +
            "  DELETE FROM flash WHERE idFlash NOT IN (SELECT DISTINCT idFlash FROM exifdata); " +
            "  DELETE FROM meteringmode WHERE idMeteringMode NOT IN (SELECT DISTINCT idMeteringMode FROM exifdata); " +
            "  DELETE FROM exposureprogram WHERE idExposureProgram NOT IN (SELECT DISTINCT idExposureProgram FROM exifdata); " +
            "  DELETE FROM exposuremode WHERE idExposureMode NOT IN (SELECT DISTINCT idExposureMode FROM exifdata); " +
            "  DELETE FROM sensingmethod WHERE idSensingMethod NOT IN (SELECT DISTINCT idSensingMethod FROM exifdata); " +
            "  DELETE FROM scenetype WHERE idSceneType NOT IN (SELECT DISTINCT idSceneType FROM exifdata); " +
            "  DELETE FROM scenecapturetype WHERE idSceneCaptureType NOT IN (SELECT DISTINCT idSceneCaptureType FROM exifdata); " +
            "  DELETE FROM whitebalance WHERE idWhiteBalance NOT IN (SELECT DISTINCT idWhiteBalance FROM exifdata); " +
            "  DELETE FROM author WHERE idAuthor NOT IN (SELECT DISTINCT idAuthor FROM exifdata); " +
            "  DELETE FROM byline WHERE idByline NOT IN (SELECT DISTINCT idByline FROM exifdata); " +
            "  DELETE FROM software WHERE idSoftware NOT IN (SELECT DISTINCT idSoftware FROM exifdata); " +
            "  DELETE FROM usercomment WHERE idUserComment NOT IN (SELECT DISTINCT idUserComment FROM exifdata); " +
            "  DELETE FROM copyright WHERE idCopyright NOT IN (SELECT DISTINCT idCopyright FROM exifdata); " +
            "  DELETE FROM copyrightnotice WHERE idCopyrightNotice NOT IN (SELECT DISTINCT idCopyrightNotice FROM exifdata); " +
            "  DELETE FROM country WHERE idCountry NOT IN (SELECT DISTINCT idCountry FROM exifdata); " +
            "  DELETE FROM state WHERE idState NOT IN (SELECT DISTINCT idState FROM exifdata); " +
            "  DELETE FROM city WHERE idCity NOT IN (SELECT DISTINCT idCity FROM exifdata); " +
            "  DELETE FROM sublocation WHERE idSublocation NOT IN (SELECT DISTINCT idSublocation FROM exifdata); " +
            "  DELETE FROM iso WHERE idIso NOT IN (SELECT DISTINCT idIso FROM exifdata); " +
            "  DELETE FROM exposuretime WHERE idExposureTime NOT IN (SELECT DISTINCT idExposureTime FROM exifdata); " +
            "  DELETE FROM exposurecompensation WHERE idExposureCompensation NOT IN (SELECT DISTINCT idExposureCompensation FROM exifdata); " +
            "  DELETE FROM fstop WHERE idFStop NOT IN (SELECT DISTINCT idFStop FROM exifdata); " +
            "  DELETE FROM shutterspeed WHERE idShutterSpeed NOT IN (SELECT DISTINCT idShutterSpeed FROM exifdata); " +
            "  DELETE FROM focallength WHERE idFocalLength NOT IN (SELECT DISTINCT idFocalLength FROM exifdata); " +
            "  DELETE FROM focallength35mm WHERE idFocalLength35mm NOT IN (SELECT DISTINCT idFocalLength35mm FROM exifdata); " +
            "END;");
      DatabaseUtility.AddTrigger(m_db, "Delete_ExtraKeywords",
            "CREATE TRIGGER Delete_ExtraKeywords AFTER DELETE ON keywordslinkpicture " +
            "BEGIN " +
            "  DELETE FROM keyword WHERE idKeyword NOT IN (SELECT DISTINCT idKeyword FROM keywordslinkpicture); " +
            "END;");
      #endregion

      #region Exif Views
      DatabaseUtility.AddView(m_db, "picturedata", "CREATE VIEW picturedata AS " +
                                                          "SELECT picture.idPicture, strFile, strDateTaken, iImageWidth, iImageHeight, iImageXReso, iImageYReso, " +
                                                          "strCamera, strCameraMake, strLens, strISO, strExposureTime, strExposureCompensation, strFStop, strShutterSpeed, " +
                                                          "strFocalLength, strFocalLength35mm, " +
                                                          "strOrientation, strFlash, strMeteringMode, " +
                                                          "strCountryCode, strCountry, strState, strCity, strSubLocation, strExposureProgram, strExposureMode, strSensingMethod, strSceneType, " +
                                                          "strSceneCaptureType, strWhiteBalance, strAuthor, strByLine, strSoftware, strUserComment, strCopyright, strCopyrightNotice, " +
                                                          "iImageWidth||'x'||iImageHeight as strImageDimension, iImageXReso||'x'||iImageYReso as strImageResolution, " +
                                                          "gpslocation.latitude, gpslocation.longitude, gpslocation.altitude, exifdata.* " +
                                                          "FROM picture " +
                                                          "LEFT JOIN exifdata USING (idPicture) " +
                                                          "LEFT JOIN camera USING (idCamera) " +
                                                          "LEFT JOIN lens USING (idLens) " +
                                                          "LEFT JOIN orientation USING (idOrientation) " +
                                                          "LEFT JOIN flash USING (idFlash) " +
                                                          "LEFT JOIN meteringmode USING (idMeteringMode) " +
                                                          "LEFT JOIN country USING (idCountry) " +
                                                          "LEFT JOIN state USING (idState) " +
                                                          "LEFT JOIN city USING (idCity) " +
                                                          "LEFT JOIN sublocation USING (idSublocation) " +
                                                          "LEFT JOIN exposureprogram USING (idExposureProgram) " +
                                                          "LEFT JOIN exposuremode USING (idExposureMode) " +
                                                          "LEFT JOIN sensingmethod USING (idSensingMethod) " +
                                                          "LEFT JOIN scenetype USING (idSceneType) " +
                                                          "LEFT JOIN scenecapturetype USING (idSceneCaptureType) " +
                                                          "LEFT JOIN whitebalance USING (idWhiteBalance) " +
                                                          "LEFT JOIN author USING (idAuthor) " +
                                                          "LEFT JOIN byline USING (idByline) " +
                                                          "LEFT JOIN software USING (idSoftware) " +
                                                          "LEFT JOIN usercomment USING (idUserComment) " +
                                                          "LEFT JOIN copyright USING (idCopyright) " +
                                                          "LEFT JOIN copyrightnotice USING (idCopyrightNotice) " +
                                                          "LEFT JOIN iso USING (idISO) " +
                                                          "LEFT JOIN exposuretime USING (idExposureTime) " +
                                                          "LEFT JOIN exposurecompensation USING (idExposureCompensation) " +
                                                          "LEFT JOIN fstop USING (idFStop) " +
                                                          "LEFT JOIN shutterspeed USING (idShutterSpeed) " +
                                                          "LEFT JOIN focallength USING (idFocalLength) " +
                                                          "LEFT JOIN focallength35mm USING (idFocalLength35mm) " +
                                                          "LEFT JOIN gpslocation USING (idGPSLocation);");

      DatabaseUtility.AddView(m_db, "picturekeywords", "CREATE VIEW picturekeywords AS " +
                                                       "SELECT picture.*, keyword.strKeyword FROM picture " +
                                                       "JOIN keywordslinkpicture USING (idPicture) " +
                                                       "JOIN keyword USING (idKeyword);");
      #endregion

      return true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Optimize()
    {
      if (m_db == null)
      {
        Log.Error("Database not initialized");
        return;
      }
      try
      {
        m_db.Execute("analyze;");
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: Analyze {0}", ex.Message);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int AddPicture(string strPicture, int iRotation)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture) || m_db == null)
      {
        return -1;
      }

      try
      {
        string strPic = strPicture;
        string strDateTaken = string.Empty;

        DatabaseUtility.RemoveInvalidChars(ref strPic);
        string strSQL = String.Format("SELECT idPicture FROM picture WHERE strFile = '{0}'", strPic);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results != null && results.Rows.Count > 0)
        {
          return DatabaseUtility.GetAsInt(results, 0, "idPicture");
        }

        ExifMetadata.Metadata exifData;

        // We need the date nevertheless for database view / sorting
        if (!GetExifDetails(strPicture, ref iRotation, ref strDateTaken, out exifData))
        {
          try
          {
            DateTime dat = File.GetLastWriteTime(strPicture);
            if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(dat))
            {
              dat = dat.AddHours(1); // Try to respect the timezone of the file date
            }
            strDateTaken = dat.ToString("yyyy-MM-dd HH:mm:ss");
          }
          catch (Exception ex)
          {
            Log.Error("Picture.DB.SQLite: Conversion exception getting file date: {0} stack:{1}", ex.Message, ex.StackTrace);
          }
        }

        // Save potential performance penalty
        if (_usePicasa)
        {
          if (GetPicasaRotation(strPic, ref iRotation))
          {
            Log.Debug("Picture.DB.SQLite: Changed rotation of image {0} based on picasa file to {1}", strPic, iRotation);
          }
        }

        // Transactions are a special case for SQLite - they speed things up quite a bit
        BeginTransaction();

        strSQL = String.Format("INSERT INTO picture (idPicture, strFile, iRotation, strDateTaken, iImageWidth, iImageHeight, iImageXReso, iImageYReso) VALUES " +
                                                   "(NULL, '{0}', {1}, '{2}', {3}, {4}, {5}, {6})",
                                                            strPic, iRotation, strDateTaken,
                                                            exifData.ImageDimensions.Width, exifData.ImageDimensions.Height,
                                                            exifData.Resolution.Width, exifData.Resolution.Height);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          Log.Debug("Picture.DB.SQLite: Added Picture to database - {0}", strPic);
        }

        CommitTransaction();

        int lPicId = m_db.LastInsertID();
        AddPictureExifData(lPicId, exifData);

        if (g_Player.Playing)
        {
          Thread.Sleep(50);
        }
        else
        {
          Thread.Sleep(1);
        }

        return lPicId;
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: AddPicture: {0} stack:{1}", ex.Message, ex.StackTrace);
        RollbackTransaction();
      }
      return -1;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int UpdatePicture(string strPicture, int iRotation)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        return -1;
      }
      if (m_db == null)
      {
        return -1;
      }

      DeletePicture(strPicture);
      return AddPicture(strPicture, iRotation);
    }

    #region EXIF

    private string GetValueForQuery(int value)
    {
      return value < 0 ? "NULL" : value.ToString();
    }

    private string GetGPSValueForQuery(string value)
    {
       return String.IsNullOrEmpty(value) ? "NULL" : value;
    }

    private void AddPictureExifData(int iDbID, ExifMetadata.Metadata exifData)
    {
      if (exifData.IsEmpty() || iDbID <= 0)
      {
        return;
      }

      try
      {
        BeginTransaction();

        AddKeywords(iDbID, exifData.Keywords.DisplayValue);

        try
        {
          string strSQL = String.Format("INSERT OR REPLACE INTO exifdata (idPicture, " +
                                                                         "idCamera, " +
                                                                         "idLens, " +
                                                                         "idOrientation, " +
                                                                         "idFlash, " +
                                                                         "idMeteringMode, " +
                                                                         "idExposureProgram, idExposureMode, " +
                                                                         "idSensingMethod, " +
                                                                         "idSceneType, idSceneCaptureType, " +
                                                                         "idWhiteBalance, " +
                                                                         "idAuthor, idByline, " +
                                                                         "idSoftware, idUserComment, " +
                                                                         "idCopyright, idCopyrightNotice, " +
                                                                         "idCountry, idState, idCity, idSublocation, " +
                                                                         "idIso, idExposureTime, idExposureCompensation, idFstop, " +
                                                                         "idShutterSpeed, idFocalLength, idFocalLength35mm, " +
                                                                         "idGPSLocation) " +
                                   "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, " +
                                           "{16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29});",
                                            iDbID,
                                            GetValueForQuery(AddItem("Camera", exifData.CameraModel.DisplayValue, "CameraMake", exifData.EquipmentMake.DisplayValue)),
                                            GetValueForQuery(AddItem("Lens", exifData.Lens.DisplayValue, "LensMake", exifData.Lens.Value)),
                                            GetValueForQuery(AddOrienatation(exifData.Orientation.Value, exifData.Orientation.DisplayValue)),
                                            GetValueForQuery(AddItem("Flash", exifData.Flash.DisplayValue)),
                                            GetValueForQuery(AddItem("MeteringMode", exifData.MeteringMode.DisplayValue)),
                                            GetValueForQuery(AddItem("ExposureProgram", exifData.ExposureProgram.DisplayValue)),
                                            GetValueForQuery(AddItem("ExposureMode", exifData.ExposureMode.DisplayValue)),
                                            GetValueForQuery(AddItem("SensingMethod", exifData.SensingMethod.DisplayValue)),
                                            GetValueForQuery(AddItem("SceneType", exifData.SceneType.DisplayValue)),
                                            GetValueForQuery(AddItem("SceneCaptureType", exifData.SceneCaptureType.DisplayValue)),
                                            GetValueForQuery(AddItem("WhiteBalance", exifData.WhiteBalance.DisplayValue)),
                                            GetValueForQuery(AddItem("Author", exifData.Author.DisplayValue)),
                                            GetValueForQuery(AddItem("Byline", exifData.ByLine.DisplayValue)),
                                            GetValueForQuery(AddItem("Software", exifData.ViewerComments.DisplayValue)),
                                            GetValueForQuery(AddItem("UserComment", exifData.Comment.DisplayValue)),
                                            GetValueForQuery(AddItem("Copyright", exifData.Copyright.DisplayValue)),
                                            GetValueForQuery(AddItem("CopyrightNotice", exifData.CopyrightNotice.DisplayValue)),
                                            GetValueForQuery(AddItem("Country", exifData.CountryName.DisplayValue, "CountryCode", exifData.CountryCode.DisplayValue)),
                                            GetValueForQuery(AddItem("State", exifData.ProvinceOrState.DisplayValue)),
                                            GetValueForQuery(AddItem("City", exifData.City.DisplayValue)),
                                            GetValueForQuery(AddItem("Sublocation", exifData.SubLocation.DisplayValue)),
                                            GetValueForQuery(AddItem("ISO", exifData.ISO.DisplayValue)),
                                            GetValueForQuery(AddItem("ExposureTime", exifData.ExposureTime.DisplayValue)),
                                            GetValueForQuery(AddItem("ExposureCompensation", exifData.ExposureCompensation.DisplayValue)),
                                            GetValueForQuery(AddItem("FStop", exifData.Fstop.DisplayValue)),
                                            GetValueForQuery(AddItem("ShutterSpeed", exifData.ShutterSpeed.DisplayValue)),
                                            GetValueForQuery(AddItem("FocalLength", exifData.FocalLength.DisplayValue)),
                                            GetValueForQuery(AddItem("FocalLength35mm", exifData.FocalLength35MM.DisplayValue)),
                                            GetValueForQuery(AddLocation(exifData.Latitude.DisplayValue, exifData.Longitude.DisplayValue, exifData.Altitude.DisplayValue))
                                            );

          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: AddExifLinks: {0} stack:{1}", ex.Message, ex.StackTrace);
        }

        CommitTransaction();

      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: AddPictureExifData: {0} stack:{1}", ex.Message, ex.StackTrace);
        RollbackTransaction();
      }
    }

    private string CleanupString(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return String.Empty;
      }
      value = Regex.Replace(value, @"[\u0000-\u001F]+", string.Empty);
      value = DatabaseUtility.RemoveInvalidChars(value);
      return Regex.Replace(value, @"\s*unknown\s*(?:\(\d*\))?\s*", string.Empty,RegexOptions.IgnoreCase).Trim();
    }

    private int AddItem(string tableName, string value)
    {
      value = CleanupString(value);
      if (value.Length == 0)
      {
        return -1;
      }

      try
      {
        string strSQL = String.Format("SELECT id{0} FROM {0} WHERE str{0} = '{1}'", tableName, value);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO {0} (id{0}, str{0}) VALUES (NULL, '{1}')", tableName, value);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          return DatabaseUtility.GetAsInt(results, 0, 0);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: Add{0}: {1} stack:{2}", tableName, ex.Message, ex.StackTrace);
      }
      return -1;
    }

    private int AddItem(string tableName, string value, string additionalName, string additionalValue)
    {
      value = CleanupString(value);
      if (value.Length == 0)
      {
        return -1;
      }

      additionalValue = CleanupString(additionalValue);
      try
      {
        string strSQL = String.Format("SELECT id{0} FROM {0} WHERE str{0} = '{1}'", tableName, value);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO {0} (id{0}, str{0}, str{1}) VALUES (NULL, '{2}', '{3}')", tableName, additionalName, value, additionalValue);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          return DatabaseUtility.GetAsInt(results, 0, 0);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: Add{0}: {1} stack:{2}", tableName, ex.Message, ex.StackTrace);
      }
      return -1;
    }

    private int AddOrienatation(string id, string name)
    {
      name = CleanupString(name);
      if (string.IsNullOrWhiteSpace(id) || name == String.Empty)
      {
        return -1;
      }

      try
      {
        string strId = DatabaseUtility.RemoveInvalidChars(id.Trim());

        string strSQL = String.Format("SELECT idOrientation FROM orientation WHERE idOrientation = {0}", strId);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO orientation (idOrientation, strOrientation) VALUES ({0}, '{1}')", strId, DatabaseUtility.RemoveInvalidChars(name.Trim()));
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          return DatabaseUtility.GetAsInt(results, 0, "idOrientation");
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: AddOrienatation: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return -1;
    }

    private int AddLocation(string latitude, string longitude, string altitude)
    {
      if (String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longitude))
        return -1;

      try
      {
        string strSQL = String.Format("SELECT idGPSLocation FROM gpslocation WHERE latitude = {0} AND longitude = {1} and altitude = {2}", latitude, longitude, GetGPSValueForQuery(altitude));
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO gpslocation (idGPSLocation, latitude, longitude, altitude) VALUES (NULL, {0}, {1}, {2})", latitude, longitude, GetGPSValueForQuery(altitude));
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          return DatabaseUtility.GetAsInt(results, 0, 0);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: AddGPSLocation: {1} stack:{2}", ex.Message, ex.StackTrace);
      }
      return -1;
    }

    private void AddKeywords(int picID, string keywords)
    {
      if (string.IsNullOrWhiteSpace(keywords) || m_db == null)
      {
        return;
      }

      try
      {
        foreach (string part in keywords.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
        {
          AddKeywordToPicture(AddItem("Keyword", part), picID);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: AddKeywords: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private void AddKeywordToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT OR IGNORE INTO keywordslinkpicture (idKeyword, idPicture) VALUES ({0}, {1})", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: AddKeywordToPicture: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public ExifMetadata.Metadata GetExifFromFile(string strPicture)
    {

      if (!Util.Utils.IsPicture(strPicture))
      {
        return new ExifMetadata.Metadata();
      }

      using (ExifMetadata extractor = new ExifMetadata())
      {
        return extractor.GetExifMetadata(strPicture);
      }
    }

    private string GetExifDBKeywords(int idPicture)
    {
      if (idPicture < 1)
      {
        return string.Empty;
      }

      try
      {
        string SQL = String.Format("SELECT strKeyword FROM picturekeywords WHERE idPicture = {0} ORDER BY 1", idPicture);
        SQLiteResultSet results = m_db.Execute(SQL);
        if (results != null && results.Rows.Count > 0)
        {
          StringBuilder result = new StringBuilder();
          for (int i = 0; i < results.Rows.Count; i++)
          {
            string keyw = results.Rows[i].fields[0].Trim();
            if (!String.IsNullOrEmpty(keyw))
            {
              if (result.Length > 0)
                result.Append("; ");
              result.Append(keyw);
            }
          }
          return result.ToString();
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: GetExifDBKeywords: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return string.Empty;
    }

    private bool AssignAllExifFieldsFromResultSet(ref ExifMetadata.Metadata aExif, SQLiteResultSet aResult, int aRow)
    {
      if (aResult == null || aResult.Rows.Count < 1)
      {
        return false;
      }

      aExif.DatePictureTaken.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strDateTaken");
      aExif.Orientation.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strOrientation");
      aExif.EquipmentMake.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strCameraMake");
      aExif.CameraModel.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strCamera");
      aExif.Lens.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strLens");
      aExif.Fstop.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strFStop");
      aExif.ShutterSpeed.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strShutterSpeed");
      aExif.ExposureTime.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strExposureTime");
      aExif.ExposureCompensation.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strExposureCompensation");
      aExif.ExposureProgram.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strExposureProgram");
      aExif.ExposureMode.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strExposureMode");
      aExif.MeteringMode.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strMeteringMode");
      aExif.Flash.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strFlash");
      aExif.ISO.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strISO");
      aExif.WhiteBalance.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strWhiteBalance");
      aExif.SensingMethod.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strSensingMethod");
      aExif.SceneType.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strSceneType");
      aExif.SceneCaptureType.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strSceneCaptureType");
      aExif.FocalLength.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strFocalLength");
      aExif.FocalLength35MM.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strFocalLength35mm");
      aExif.CountryCode.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strCountryCode");
      aExif.CountryName.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strCountry");
      aExif.ProvinceOrState.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strState");
      aExif.City.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strCity");
      aExif.SubLocation.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strSublocation");
      aExif.Author.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strAuthor");
      aExif.Copyright.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strCopyright");
      aExif.CopyrightNotice.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strCopyrightNotice");
      aExif.Comment.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strUserComment");
      aExif.ViewerComments.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strSoftware");
      aExif.ByLine.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strByline");
      aExif.Latitude.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strGPSLatitude");
      aExif.Longitude.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strGPSLongitude");
      aExif.Altitude.DisplayValue = DatabaseUtility.Get(aResult, aRow, "strGPSAltitude");
      aExif.ImageDimensions.Width = DatabaseUtility.GetAsInt(aResult, aRow, "iImageWidth");
      aExif.ImageDimensions.Height = DatabaseUtility.GetAsInt(aResult, aRow, "iImageHeight");
      aExif.Resolution.Width = DatabaseUtility.GetAsInt(aResult, aRow, "iImageXReso");
      aExif.Resolution.Height = DatabaseUtility.GetAsInt(aResult, aRow, "iImageYReso");

      aExif.Orientation.Value = DatabaseUtility.GetAsInt(aResult, aRow, "idOrientation").ToString();
      aExif.DatePictureTaken.Value = DatabaseUtility.GetAsDateTime(aResult, aRow, "strDateTaken").ToString();
      return true;
    }

    public ExifMetadata.Metadata GetExifFromDB(string strPicture)
    {
      if (m_db == null || !Util.Utils.IsPicture(strPicture))
      {
        return new ExifMetadata.Metadata();
      }

      ExifMetadata.Metadata metaData = new ExifMetadata.Metadata();
      try
      {
        string strPic = strPicture;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        string SQL = String.Format("SELECT idPicture FROM picture WHERE strFile = '{0}'", strPic);
        SQLiteResultSet results = m_db.Execute(SQL);
        if (results != null && results.Rows.Count > 0)
        {
          int idPicture = DatabaseUtility.GetAsInt(results, 0, "idPicture");
          if (idPicture > 0)
          {
            SQL = String.Format("SELECT * FROM picturedata WHERE idPicture = {0}", idPicture);
            results = m_db.Execute(SQL);
            if (results != null && results.Rows.Count > 0)
            {
              AssignAllExifFieldsFromResultSet(ref metaData, results, 0);
              metaData.Keywords.DisplayValue = GetExifDBKeywords(idPicture);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: GetExifDBData: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return metaData;
    }

    private bool GetExifDetails(string strPicture, ref int iRotation, ref string strDateTaken, out ExifMetadata.Metadata metaData)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        metaData = new ExifMetadata.Metadata();
        return false;
      }

      metaData = GetExifFromFile(strPicture);
      if (metaData.IsEmpty())
      {
        return false;
      }

      try
      {
        strDateTaken = metaData.DatePictureTaken.Value;
        if (!string.IsNullOrWhiteSpace(strDateTaken))
        // If the image contains a valid exif date store it in the database, otherwise use the file date
        {
          if (_useExif)
          {
            iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Value));
          }
          return true;
        }
      }
      catch (FormatException ex)
      {
        Log.Error("Picture.DB.SQLite: Exif details: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
      strDateTaken = string.Empty;
      return false;
    }

    public int EXIFOrientationToRotation(int orientation)
    {
      return orientation.ToRotation();
    }

    #endregion

    private bool GetPicasaRotation(string strPic, ref int iRotation)
    {
      bool foundValue = false;
      if (File.Exists(Path.GetDirectoryName(strPic) + "\\Picasa.ini"))
      {
        using (StreamReader sr = File.OpenText(Path.GetDirectoryName(strPic) + "\\Picasa.ini"))
        {
          try
          {
            string s = string.Empty;
            bool searching = true;
            while ((s = sr.ReadLine()) != null && searching)
            {
              if (s.ToLowerInvariant() == "[" + Path.GetFileName(strPic).ToLowerInvariant() + "]")
              {
                do
                {
                  s = sr.ReadLine();
                  if (s.StartsWith("rotate=rotate("))
                  {
                    // Find out Rotate Setting
                    try
                    {
                      iRotation = int.Parse(s.Substring(14, 1));
                      foundValue = true;
                    }
                    catch (Exception ex)
                    {
                      Log.Error("Picture.DB.SQLite: Error converting number picasa.ini", ex.Message, ex.StackTrace);
                    }
                    searching = false;
                  }
                } while (s != null && !s.StartsWith("[") && searching);
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("Picture.DB.SQLite: File read problem picasa.ini", ex.Message, ex.StackTrace);
          }
        }
      }
      return foundValue;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public string GetDateTaken(string strPicture)
    {
      if (m_db == null)
      {
        return string.Empty;
      }
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        return string.Empty;
      }

      string result = string.Empty;

      try
      {
        string strPic = strPicture;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        string SQL = String.Format("SELECT strDateTaken FROM picture WHERE strFile = '{0}'", strPic);
        SQLiteResultSet results = m_db.Execute(SQL);
        if (results != null && results.Rows.Count > 0)
        {
          result = DatabaseUtility.Get(results, 0, "strDateTaken");
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: GetDateTaken: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return result;
    }

    public DateTime GetDateTimeTaken(string strPicture)
    {
      string dbDateTime = GetDateTaken(strPicture);
      if (string.IsNullOrEmpty(dbDateTime))
      {
        return DateTime.MinValue;
      }

      try
      {
        DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
        dateTimeFormat.ShortDatePattern = "yyyy-MM-dd HH:mm:ss";
        return DateTime.ParseExact(dbDateTime, "d", dateTimeFormat);
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: GetDateTaken Date parse Error: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return DateTime.MinValue;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int GetRotation(string strPicture)
    {
      if (m_db == null)
      {
        return -1;
      }
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        return -1;
      }

      try
      {
        string strPic = strPicture;
        int iRotation = 0;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        SQLiteResultSet results = m_db.Execute(String.Format("SELECT iRotation FROM picture WHERE strFile = '{0}'", strPic));
        if (results != null && results.Rows.Count > 0)
        {
          iRotation = DatabaseUtility.GetAsInt(results, 0, "iRotation");
          return iRotation;
        }

        if (_useExif)
        {
          iRotation = Util.Picture.GetRotateByExif(strPicture);
          Log.Debug("Picture.DB.SQLite: GetRotateByExif = {0} for {1}", iRotation, strPicture);
        }

        AddPicture(strPicture, iRotation);

        return iRotation;
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: GetRotation: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return 0;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetRotation(string strPicture, int iRotation)
    {
      if (m_db == null)
      {
        return;
      }
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        return;
      }

      try
      {
        string strPic = strPicture;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        long lPicId = AddPicture(strPicture, iRotation);
        if (lPicId >= 0)
        {
          m_db.Execute(String.Format("UPDATE picture SET iRotation={0} WHERE strFile = '{1}'", iRotation, strPic));
        }
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: {0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void DeletePicture(string strPicture)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        return;
      }
      if (m_db == null)
      {
        return;
      }

      lock (typeof(PictureDatabase))
      {
        try
        {
          string strPic = strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          string strSQL = String.Format("DELETE FROM picture WHERE strFile = '{0}'", strPic);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Deleting picture err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return;
      }
    }

    private string GetSearchQuery(string find)
    {
      string result = string.Empty;
      MatchCollection matches = Regex.Matches(find, @"([+|]?[^+|;]+;?)");
      foreach (Match match in matches)
      {
        foreach (Capture capture in match.Captures)
        {
          if (!string.IsNullOrEmpty(capture.Value))
          {
            string part = capture.Value;
            if (part.Contains("+"))
            {
              part = part.Replace("+", " AND {0} = '") + "'";
            }
            if (part.Contains("|"))
            {
              part = part.Replace("|", " OR {0} = '") + "'";
            }
            if (part.Contains(";"))
            {
              part = "{0} = '" + part.Replace(";", "' AND ");
            }
            if (!part.Contains("{0}"))
            {
              part = "{0} = '" + part + "'";
            }
            if (part.Contains("%"))
            {
              part = part.Replace("{0} = '", "{0} LIKE '");
            }
            result = result + part;
          }
        }
      }
      Log.Debug("Picture.DB.SQLite: Search -> Where: {0} -> {1}", find, result);
      // Picture.DB.SQLite: Search -> Where: word;word1+word2|word3|%like% -> {0} = 'word' AND {0} = 'word1' AND {0} = 'word2' OR {0} = 'word3' OR {0} LIKE '%like%'
      // Picture.DB.SQLite: Search -> Where: word -> {0} = 'word'
      // Picture.DB.SQLite: Search -> Where: word%|word2 -> {0} LIKE 'word%' OR {0} = 'word2'
      return result;
    }
    /*
        private string GetSearchQuery (string find)
        {
          string result = string.Empty;
          MatchCollection matches = Regex.Matches(find, @"([|]?[^|]+)");
          foreach (Match match in matches)
          {
            foreach (Capture capture in match.Captures)
            {
              if (!string.IsNullOrEmpty(capture.Value))
              {
                string part = capture.Value;
                if (part.Contains("+") || part.Contains(";"))
                {
                  part = part.Replace("+", ",");
                  part = part.Replace(";", ",");
                  part = part.Replace(",", "','");
                }
                if (part.Contains("|"))
                {
                  part = part.Replace("|", " OR {0} = '") + "'";
                }
                if (!part.Contains("{0}"))
                {
                  part = "{0} = '" + part + "'";
                }
                if (part.Contains(","))
                {
                  part = part.Replace("{0} = ", " {0} IN (") + ")";
                }
                if (part.Contains("%"))
                {
                  part = part.Replace("{0} = '", "{0} LIKE '");
                }
                result = result + part;
              }
            }
          }
          Log.Debug ("Picture.DB.SQLite: Search -> Where: {0} -> {1}", find, result);
          // Picture.DB.SQLite: Search -> Where: word;word1+word2|word3|%like% ->  {0} IN ('word','word1','word2') OR {0} = 'word3' OR {0} LIKE '%like%'
          // Picture.DB.SQLite: Search -> Where: word -> {0} = 'word'
          // Picture.DB.SQLite: Search -> Where: word%|word2 -> {0} LIKE 'word%' OR {0} = 'word2'
          return result;
        }
    */
    private string GetSearchWhere(string keyword, string where)
    {
      if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(where))
      {
        return string.Empty;
      }
      return "WHERE " + string.Format(GetSearchQuery(where), keyword);
    }

    private string GetSelect(string field, string search)
    {
      if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(field))
      {
        return string.Empty;
      }

      if (!search.Contains("Private"))
      {
        search += "#!Private";
      }

      string result = string.Empty;
      string[] lines = search.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
      if (lines.Length == 0)
      {
        return string.Empty;
      }

      string firstPart = string.Format("SELECT DISTINCT {0} FROM picturekeywords {1}", field, GetSearchWhere("strKeyword", lines[0]));
      string debug = string.Empty;
      for (int i = 1; i < lines.Length; i++)
      {
        debug = debug + (string.IsNullOrEmpty(debug) ? string.Empty : " <- ") + lines[i];
        string sql = string.Format("SELECT DISTINCT idPicture FROM picturekeywords {0}", GetSearchWhere("strKeyword", lines[i].Replace("!", "")));
        result += string.Format(string.IsNullOrEmpty(result) ? "{0}" : " AND idPicture {1}IN ({0}", sql, lines[i].Contains("!") ? "NOT " : string.Empty);
      }
      if (!string.IsNullOrEmpty(result))
      {
        debug = lines[0] + (string.IsNullOrEmpty(debug) ? string.Empty : " <- ") + debug;
        result = result + new String(')', lines.Length - 2);
        result = string.Format("{0} AND idPicture {2}IN ({1})", firstPart, result, lines.Length >= 2 && lines[1].Contains("!") ? "NOT " : string.Empty);
      }
      else
      {
        result = firstPart;
      }

      if (lines.Length > 1)
      {
        Log.Debug("Multi search: " + debug);
      }
      return result + " ORDER BY strDateTaken";
      // GetSelect("qwe") -> SELECT DISTINCT strFile FROM picturekeywords WHERE strKeyword = 'qwe' ORDER BY strDateTaken
      // GetSelect("qwe#aaa") -> SELECT DISTINCT strFile FROM picturekeywords WHERE strKeyword = 'qwe' AND idPicture IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'aaa') ORDER BY strDateTaken
      // GetSelect("q1#q2#q3.1|q3.2#q4%#q5") -> SELECT DISTINCT strFile FROM picturekeywords WHERE strKeyword = 'q1' AND idPicture IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'q2' AND idPicture IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'q3.1' OR strKeyword = 'q3.2' AND idPicture IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword LIKE 'q4%' AND idPicture IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'q5')))) ORDER BY strDateTaken
    }

    public int ListKeywords(ref List<string> Keywords)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT DISTINCT strKeyword FROM keyword WHERE strKeyword <> 'Private' ORDER BY 1";
        try
        {
          SQLiteResultSet result = m_db.Execute(strSQL);
          if (result != null)
          {
            for (Count = 0; Count < result.Rows.Count; Count++)
            {
              Keywords.Add(DatabaseUtility.Get(result, Count, 0));
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Keywords err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int ListPicsByKeyword(string Keyword, ref List<string> Pics)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT strFile FROM picturekeywords WHERE strKeyword = '" + Keyword + "' " +
                               "AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private') " +
                               "ORDER BY strDateTaken";
        try
        {
          SQLiteResultSet result = m_db.Execute(strSQL);
          if (result != null)
          {
            for (Count = 0; Count < result.Rows.Count; Count++)
            {
              Pics.Add(DatabaseUtility.Get(result, Count, 0));
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Picture by Keyword err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int CountPicsByKeyword(string Keyword)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT COUNT(strFile) FROM picturekeywords WHERE strKeyword = '" + Keyword + "' " +
                               "AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private') " +
                               "ORDER BY strDateTaken";
        try
        {
          SQLiteResultSet result = m_db.Execute(strSQL);
          if (result != null)
          {
            Count = DatabaseUtility.GetAsInt(result, 0, 0);
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Count of Picture by Keyword err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int ListPicsByKeywordSearch(string Keyword, ref List<string> Pics)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = GetSelect("strFile", Keyword);
        try
        {
          SQLiteResultSet result = m_db.Execute(strSQL);
          if (result != null)
          {
            for (Count = 0; Count < result.Rows.Count; Count++)
            {
              Pics.Add(DatabaseUtility.Get(result, Count, 0));
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Picture by Keyword Search err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int CountPicsByKeywordSearch(string Keyword)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = GetSelect("COUNT(strFile)", Keyword);
        try
        {
          SQLiteResultSet result = m_db.Execute(strSQL);
          if (result != null)
          {
            Count = DatabaseUtility.GetAsInt(result, 0, 0);
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Count of Picture by Keyword Search err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int ListYears(ref List<string> Years)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT DISTINCT SUBSTR(strDateTaken,1,4) FROM picture ORDER BY 1";
        SQLiteResultSet result;
        try
        {
          result = m_db.Execute(strSQL);
          if (result != null)
          {
            for (Count = 0; Count < result.Rows.Count; Count++)
            {
              Years.Add(DatabaseUtility.Get(result, Count, 0));
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Years err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int ListMonths(string Year, ref List<string> Months)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT DISTINCT SUBSTR(strDateTaken,6,2) FROM picture WHERE SUBSTR(strDateTaken,1,4) = '" + Year + "' ORDER BY strDateTaken";
        SQLiteResultSet result;
        try
        {
          result = m_db.Execute(strSQL);
          if (result != null)
          {
            for (Count = 0; Count < result.Rows.Count; Count++)
            {
              Months.Add(DatabaseUtility.Get(result, Count, 0));
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Months err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int ListDays(string Month, string Year, ref List<string> Days)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT DISTINCT SUBSTR(strDateTaken,9,2) FROM picture WHERE SUBSTR(strDateTaken,1,7) = '" + Year + "-" + Month + "' ORDER BY strDateTaken";
        SQLiteResultSet result;
        try
        {
          result = m_db.Execute(strSQL);
          if (result != null)
          {
            for (Count = 0; Count < result.Rows.Count; Count++)
            {
              Days.Add(DatabaseUtility.Get(result, Count, 0));
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Days err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int ListPicsByDate(string Date, ref List<string> Pics)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT strFile FROM picture WHERE strDateTaken LIKE '" + Date + "%' " +
                        "AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')" +
                        "ORDER BY strDateTaken";
        SQLiteResultSet result;
        try
        {
          result = m_db.Execute(strSQL);
          if (result != null)
          {
            for (Count = 0; Count < result.Rows.Count; Count++)
            {
              Pics.Add(DatabaseUtility.Get(result, Count, 0));
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Picture by Date err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    public int CountPicsByDate(string Date)
    {
      if (m_db == null)
      {
        return 0;
      }

      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        string strSQL = "SELECT COUNT(strFile) FROM picture WHERE strDateTaken LIKE '" + Date + "%' " +
                        "AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')" +
                        "ORDER BY strDateTaken";
        SQLiteResultSet result;
        try
        {
          result = m_db.Execute(strSQL);
          if (result != null)
          {
            Count = DatabaseUtility.GetAsInt(result, 0, 0);
          }
        }
        catch (Exception ex)
        {
          Log.Error("Picture.DB.SQLite: Getting Count Picture by Date err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return Count;
      }
    }

    #region Transactions

    private void BeginTransaction()
    {
      try
      {
        m_db.Execute("BEGIN");
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: Begin transaction failed exception err: {0} ", ex.Message);
        // Open();
      }
    }

    private void CommitTransaction()
    {
      try
      {
        m_db.Execute("COMMIT");
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: Commit failed exception err: {0} ", ex.Message);
        RollbackTransaction();
      }
    }

    private void RollbackTransaction()
    {
      try
      {
        m_db.Execute("ROLLBACK");
      }
      catch (Exception ex)
      {
        Log.Error("Picture.DB.SQLite: Rollback failed exception err: {0} ", ex.Message);
        // Open();
      }
    }

    #endregion

    public bool DbHealth
    {
      get
      {
        return _dbHealth;
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
        return string.Empty;
      }
    }

    #region IDisposable Members

    public void Dispose()
    {
      if (!disposed)
      {
        disposed = true;
        if (m_db != null)
        {
          try
          {
            m_db.Close();
            m_db.Dispose();
          }
          catch (Exception ex)
          {
            Log.Error("Picture.DB.SQLite: Dispose: {0}", ex.Message);
          }
          m_db = null;
        }
      }
    }

    #endregion
  }
}