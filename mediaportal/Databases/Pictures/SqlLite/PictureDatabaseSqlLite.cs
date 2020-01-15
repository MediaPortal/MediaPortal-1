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

using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Player;
using MediaPortal.Util;

using SQLite.NET;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

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
            Log.Warn("PictureDatabaseSqlLite: Disposing current instance..");
          }
          catch (Exception ex)
          {
            Log.Error("PictureDatabaseSqlLite: Open: {0}", ex.Message);
          }
        }

        // Open database
        try
        {
          Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception ex)
        {
          Log.Error("PictureDatabaseSqlLite: Create directory: {0}", ex.Message);
        }

        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "PictureDatabaseV3.db3"));
        // Retry 10 times on busy (DB in use or system resources exhausted)
        m_db.BusyRetries = 10;
        // Wait 100 ms between each try (default 10)
        m_db.BusyRetryDelay = 100;

        _dbHealth = DatabaseUtility.IntegrityCheck(m_db);

        DatabaseUtility.SetPragmas(m_db);

        CreateTables();
        InitSettings();
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      Log.Info("Picture database opened");
    }

    private void InitSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _useExif = xmlreader.GetValueAsBool("pictures", "useExif", true);
        _usePicasa = xmlreader.GetValueAsBool("pictures", "usePicasa", false);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private bool CreateTables()
    {
      if (m_db == null)
      {
        return false;
      }

      #region Tables
      DatabaseUtility.AddTable(m_db, "picture",
                               "CREATE TABLE picture ( idPicture integer primary key, strFile text, iRotation integer, strDateTaken text)");
      #endregion
      
      #region Indexes
      DatabaseUtility.AddIndex(m_db, "idxpicture_strFile", "CREATE INDEX idxpicture_strFile ON picture (strFile ASC)");
      DatabaseUtility.AddIndex(m_db, "idxpicture_strDateTaken", "CREATE INDEX idxpicture_strDateTaken ON picture (strDateTaken ASC)");
      #endregion

      #region Exif Tables
      DatabaseUtility.AddTable(m_db, "camera",
                               "CREATE TABLE camera (idCamera INTEGER PRIMARY KEY, strCamera TEXT, strCameraMake TEXT);");
      DatabaseUtility.AddTable(m_db, "cameralinkpicture",
                               "CREATE TABLE cameralinkpicture (idCamera INTEGER, idPicture INTEGER);");

      DatabaseUtility.AddTable(m_db, "lens",
                               "CREATE TABLE lens (idLens INTEGER PRIMARY KEY, strLens TEXT, strLensMake TEXT);");
      DatabaseUtility.AddTable(m_db, "lenslinkpicture",
                               "CREATE TABLE lenslinkpicture (idLens INTEGER, idPicture INTEGER);");

      DatabaseUtility.AddTable(m_db, "country",
                               "CREATE TABLE country (idCountry INTEGER PRIMARY KEY, strCountryCode TEXT, strCountry TEXT);");
      DatabaseUtility.AddTable(m_db, "countrylinkpicture",
                               "CREATE TABLE countrylinkpicture (idCountry INTEGER, idPicture INTEGER);");

      DatabaseUtility.AddTable(m_db, "state",
                               "CREATE TABLE state (idState INTEGER PRIMARY KEY, strState TEXT);");
      DatabaseUtility.AddTable(m_db, "statelinkpicture",
                               "CREATE TABLE statelinkpicture (idState INTEGER, idPicture INTEGER);");

      DatabaseUtility.AddTable(m_db, "city",
                               "CREATE TABLE city (idCity INTEGER PRIMARY KEY, strCity TEXT);");
      DatabaseUtility.AddTable(m_db, "citylinkpicture",
                               "CREATE TABLE citylinkpicture (idCity INTEGER, idPicture INTEGER);");

      DatabaseUtility.AddTable(m_db, "sublocation",
                               "CREATE TABLE sublocation (idSublocation INTEGER PRIMARY KEY, strSublocation TEXT);");
      DatabaseUtility.AddTable(m_db, "sublocationlinkpicture",
                               "CREATE TABLE sublocationlinkpicture (idSublocation INTEGER, idPicture INTEGER);");

      DatabaseUtility.AddTable(m_db, "keywords",
                               "CREATE TABLE keywords (idKeyword INTEGER PRIMARY KEY, strKeyword TEXT);");
      DatabaseUtility.AddTable(m_db, "keywordslinkpicture",
                               "CREATE TABLE keywordslinkpicture (idKeyword INTEGER, idPicture INTEGER);");

      DatabaseUtility.AddTable(m_db, "exifdata",
                               "CREATE TABLE exifdata (idExif INTEGER PRIMARY KEY, idPicture INTEGER, " +
                                                      "strISO TEXT, " +
                                                      "iMeteringMode INTEGER, " +
                                                      "strMeteringMode TEXT, " +
                                                      "iFlash INTEGER, " +
                                                      "strFlash TEXT, " +
                                                      "strExposureTime TEXT, " +
                                                      "strExposureProgram TEXT, " +
                                                      "strExposureMode TEXT, " +
                                                      "strExposureCompensation TEXT, " +
                                                      "strFStop TEXT, " +
                                                      "strShutterSpeed TEXT, " +
                                                      "strSensingMethod TEXT, " +
                                                      "strSceneType TEXT, " +
                                                      "strSceneCaptureType TEXT, " +
                                                      "strWhiteBalance TEXT, " +
                                                      "strFocalLength TEXT, " +
                                                      "strFocalLength35 TEXT, " +
                                                      "strAuthor TEXT, " + 
                                                      "strUserComment TEXT, " +
                                                      "strByline TEXT, " +
                                                      "strSoftware TEXT, " + 
                                                      "strCopyright TEXT, " +
                                                      "strCopyrightNotice TEXT, " +
                                                      "strGPSLatitude TEXT, " +
                                                      "strGPSLongitude TEXT, " +
                                                      "strGPSAltitude TEXT);");
      #endregion

      #region Exif Indexes
      DatabaseUtility.AddIndex(m_db, "idxcameralinkpicture_idCamera", "CREATE INDEX idxcameralinkpicture_idCamera ON cameralinkpicture(idCamera ASC);");
      DatabaseUtility.AddIndex(m_db, "idxcameralinkpicture_idPicture", "CREATE INDEX idxcameralinkpicture_idPicture ON cameralinkpicture(idPicture ASC);");

      DatabaseUtility.AddIndex(m_db, "idxlenslinkpicture_idLens", "CREATE INDEX idxlenslinkpicture_idLens ON lenslinkpicture(idLens ASC);");
      DatabaseUtility.AddIndex(m_db, "idxlenslinkpicture_idPicture", "CREATE INDEX idxlenslinkpicture_idPicture ON lenslinkpicture(idPicture ASC);");

      DatabaseUtility.AddIndex(m_db, "idxcountrylinkpicture_idCountry", "CREATE INDEX idxcountrylinkpicture_idCountry ON countrylinkpicture(idCountry ASC);");
      DatabaseUtility.AddIndex(m_db, "idxcountrylinkpicture_idPicture", "CREATE INDEX idxcountrylinkpicture_idPicture ON countrylinkpicture(idPicture ASC);");

      DatabaseUtility.AddIndex(m_db, "idxstatelinkpicture_idState", "CREATE INDEX idxstatelinkpicture_idState ON statelinkpicture(idState ASC);");
      DatabaseUtility.AddIndex(m_db, "idxstatelinkpicture_idPicture", "CREATE INDEX idxstatelinkpicture_idPicture ON statelinkpicture(idPicture ASC);");

      DatabaseUtility.AddIndex(m_db, "idxcitylinkpicture_idCity", "CREATE INDEX idxcitylinkpicture_idCity ON citylinkpicture(idCity ASC);");
      DatabaseUtility.AddIndex(m_db, "idxcitylinkpicture_idPicture", "CREATE INDEX idxcitylinkpicture_idPicture ON citylinkpicture(idPicture ASC);");

      DatabaseUtility.AddIndex(m_db, "idxsublocationlinkpicture_idSublocation", "CREATE INDEX idxsublocationlinkpicture_idSublocation ON sublocationlinkpicture(idSublocation ASC);");
      DatabaseUtility.AddIndex(m_db, "idxsublocationlinkpicture_idPicture", "CREATE INDEX idxsublocationlinkpicture_idPicture ON sublocationlinkpicture(idPicture ASC);");

      DatabaseUtility.AddIndex(m_db, "idxkeywordslinkpicture_idKeyword", "CREATE INDEX idxkeywordslinkpicture_idKeyword ON keywordslinkpicture(idKeyword ASC);");
      DatabaseUtility.AddIndex(m_db, "idxkeywordslinkpicture_idPicture", "CREATE INDEX idxkeywordslinkpicture_idPicture ON keywordslinkpicture(idPicture ASC);");

      DatabaseUtility.AddIndex(m_db, "idxexifdata_idPicture", "CREATE INDEX idxexifdata_idPicture ON exifdata(idPicture ASC);");
      #endregion

      #region Exif Views
      DatabaseUtility.AddView(m_db, "picturedata", "CREATE VIEW picturedata AS " +
                                                   "SELECT picture.*, camera.*, lens.*, country.*, state.*, city.*, sublocation.*  FROM picture " +
                                                   "LEFT JOIN cameralinkpicture ON picture.idPicture = cameralinkpicture.idPicture " +
                                                   "LEFT JOIN camera ON camera.idCamera = cameralinkpicture.idCamera " +
                                                   "LEFT JOIN lenslinkpicture ON picture.idPicture = lenslinkpicture.idPicture " +
                                                   "LEFT JOIN lens ON lens.idLens = lenslinkpicture.idLens " +
                                                   "LEFT JOIN countrylinkpicture ON picture.idPicture = countrylinkpicture.idPicture " +
                                                   "LEFT JOIN country ON country.idCountry = countrylinkpicture.idCountry " +
                                                   "LEFT JOIN statelinkpicture ON picture.idPicture = statelinkpicture.idPicture " +
                                                   "LEFT JOIN state ON state.idState = statelinkpicture.idState " +
                                                   "LEFT JOIN citylinkpicture ON picture.idPicture = citylinkpicture.idPicture " +
                                                   "LEFT JOIN city ON city.idCity = citylinkpicture.idCity " +
                                                   "LEFT JOIN sublocationlinkpicture ON picture.idPicture = sublocationlinkpicture.idPicture " +
                                                   "LEFT JOIN sublocation ON sublocation.idSublocation = sublocationlinkpicture.idSublocation;");

      DatabaseUtility.AddView(m_db, "picturekeywords", "CREATE VIEW picturekeywords AS " +
                                                       "SELECT picture.idPicture, keywords.idKeyword FROM picture " +
                                                       "JOIN keywordslinkpicture ON picture.idPicture = keywordslinkpicture.idPicture " +
                                                       "JOIN keywords ON keywordslinkpicture.idKeyword = keywords.idKeyword;");
      #endregion

      return true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int AddPicture(string strPicture, int iRotation)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        return -1;
      }
      if (String.IsNullOrEmpty(strPicture))
      {
        return -1;
      }
      if (m_db == null)
      {
        return -1;
      }

      try
      {
        int lPicId = -1;
        string strPic = strPicture;
        string strDateTaken = String.Empty;

        DatabaseUtility.RemoveInvalidChars(ref strPic);
        string strSQL = String.Format("select * from picture where strFile like '{0}'", strPic);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results != null && results.Rows.Count > 0)
        {
          lPicId = Int32.Parse(DatabaseUtility.Get(results, 0, "idPicture"));
          return lPicId;
        }

        ExifMetadata.Metadata exifData = new ExifMetadata.Metadata();

        // We need the date nevertheless for database view / sorting
        if (!GetExifDetails(strPicture, ref iRotation, ref strDateTaken, ref exifData))
        {
          exifData.DatePictureTaken.DisplayValue = string.Empty;

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
            Log.Error("PictureDatabaseSqlLite: Conversion exception getting file date - err:{0} stack:{1}", ex.Message,
                      ex.StackTrace);
          }
        }

        // Save potential performance penalty
        if (_usePicasa)
        {
          if (GetPicasaRotation(strPic, ref iRotation))
          {
            Log.Debug("PictureDatabaseSqlLite: Changed rotation of image {0} based on picasa file to {1}", strPic,
                      iRotation);
          }
        }

        // Transactions are a special case for SQLite - they speed things up quite a bit
        strSQL = "begin";
        results = m_db.Execute(strSQL);
        strSQL = String.Format("insert into picture (idPicture, strFile, iRotation, strDateTaken) values(null, '{0}',{1},'{2}')",
                                strPic, iRotation, strDateTaken);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          Log.Debug("PictureDatabaseSqlLite: Added to database - {0}", strPic);
        }
        strSQL = "commit";
        results = m_db.Execute(strSQL);
        lPicId = m_db.LastInsertID();

        AddPictureExifData(lPicId, exifData) ; 

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
        Log.Error("PictureDatabaseSqlLite: AddPicture: exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private int AddPictureExifData(int iDbID, ExifMetadata.Metadata exifData)
    {
      if (String.IsNullOrEmpty(exifData.DatePictureTaken.Value))
      {
        return -1;
      }
      if (iDbID <= 0)
      {
        return -1;
      }
      if (m_db == null)
      {
        return -1;
      }

      try
      {
        int idCamera = AddCamera(exifData.CameraModel.DisplayValue, exifData.EquipmentMake.DisplayValue);
        int idLens = AddLens(exifData.Lens.DisplayValue, exifData.Lens.Value);
        int idCountry = AddCountry(exifData.CountryCode.DisplayValue, exifData.CountryName.DisplayValue);
        int idState = AddState(exifData.ProvinceOrState.DisplayValue);
        int idCity = AddCity(exifData.City.DisplayValue);
        int idSubLocation = AddSubLocation(exifData.SubLocation.DisplayValue);

        // Transactions are a special case for SQLite - they speed things up quite a bit
        string strSQL = "begin";
        SQLiteResultSet results = m_db.Execute(strSQL);

        AddCameraToPicture(idCamera, iDbID);
        AddLensToPicture(idLens, iDbID);
        AddCountryToPicture(idCountry, iDbID);
        AddStateToPicture(idState, iDbID);
        AddCityToPicture(idCity, iDbID);
        AddSubLocationToPicture(idSubLocation, iDbID);

        AddKeywords(iDbID, exifData.Keywords.DisplayValue);

        int idExif = AddExif(iDbID, exifData);

        strSQL = "commit";
        results = m_db.Execute(strSQL);

        return idExif;
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddPictureExifData: exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int AddCamera(string camera, string make)
    {
      if (string.IsNullOrEmpty(camera))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }
      if (string.IsNullOrEmpty(make))
      {
        make = string.Empty;
      }

      try
      {
        string strCamera = camera.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strCamera);
        string strMake = make.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strMake);

        string strSQL = String.Format("SELECT * FROM camera WHERE strCamera = '{0}'", strCamera);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO camera (idCamera, strCamera, strCameraMake) VALUES (NULL, '{0}', '{1}')", strCamera, strMake);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idCamera"), out iID))
          {
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddCamera exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int AddLens(string lens, string make)
    {
      if (string.IsNullOrEmpty(lens))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }
      if (string.IsNullOrEmpty(make))
      {
        make = string.Empty;
      }

      try
      {
        string strLens = lens.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strLens);
        string strMake = make.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strMake);

        string strSQL = String.Format("SELECT * FROM lens WHERE strLens = '{0}'", strLens);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO lens (idLens, strLens, strLensMake) VALUES (NULL, '{0}', '{1}')", strLens, strMake);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idLens"), out iID))
          {
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddLens exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int AddCountry(string code, string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }
      if (string.IsNullOrEmpty(code))
      {
        code = string.Empty;
      }

      try
        {
        string strCode = code.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strCode);
        string strName = name.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strName);

        string strSQL = String.Format("SELECT * FROM country WHERE strCountry = '{0}'", strName);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO country (idCountry, strCountryCode, strCountry) VALUES (NULL, '{0}', '{1}')", strCode, strName);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idCountry"), out iID))
          {
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddCountry exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int AddState(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }

      try
      {
        string strName = name.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strName);

        string strSQL = String.Format("SELECT * FROM state WHERE strState = '{0}'", strName);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO state (idState, strState) VALUES (NULL, '{0}')", strName);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idState"), out iID))
          {
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddState exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int AddCity(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }

      try
      {
        string strName = name.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strName);

        string strSQL = String.Format("SELECT * FROM city WHERE strCity = '{0}'", strName);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO city (idCity, strCity) VALUES (NULL, '{0}')", strName);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idCity"), out iID))
          {
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddCity exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int AddSubLocation(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }

      try
      {
        string strName = name.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strName);

        string strSQL = String.Format("SELECT * FROM sublocation WHERE strSublocation = '{0}'", strName);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO sublocation (idSublocation, strSublocation) VALUES (NULL, '{0}')", strName);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idSublocation"), out iID))
          {
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddSubLocation exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private int AddKeyword(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }

      try
      {
        string strName = name.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strName);

        string strSQL = String.Format("SELECT * FROM keywords WHERE strKeyword = '{0}'", strName);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO keywords (idKeyword, strKeyword) VALUES (NULL, '{0}')", strName);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idKeyword"), out iID))
          {
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddKeyword exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private void AddKeywords(string keywords)
    {
      if (string.IsNullOrEmpty(keywords))
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string[] parts = keywords.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
          AddKeyword(part);
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddKeywords exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private void AddKeywords(int picID, string keywords)
    {
      if (string.IsNullOrEmpty(keywords))
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string[] parts = keywords.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
          int id = AddKeyword(part);
          if (id > 0)
          {
            AddKeywordToPicture(id, picID);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddKeywords exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private void AddCameraToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT INTO cameralinkpicture (idCamera, idPicture) VALUES ('{0}', '{1}')", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddCameraToPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    private void AddLensToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT INTO lenslinkpicture (idLens, idPicture) VALUES ('{0}', '{1}')", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddLensToPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private void AddCountryToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT INTO countrylinkpicture (idCountry, idPicture) VALUES ('{0}', '{1}')", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddCountryToPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private void AddStateToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT INTO statelinkpicture (idState, idPicture) VALUES ('{0}', '{1}')", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddStateToPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private void AddCityToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT INTO citylinkpicture (idCity, idPicture) VALUES ('{0}', '{1}')", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddCityToPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private void AddSubLocationToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT INTO sublocationlinkpicture (idSublocation, idPicture) VALUES ('{0}', '{1}')", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddSubLocationToPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }

    private void AddKeywordToPicture(int keyID, int picID)
    {
      if (keyID <= 0 || picID <= 0)
      {
        return;
      }
      if (null == m_db)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("INSERT INTO keywordslinkpicture (idKeyword, idPicture) VALUES ('{0}', '{1}')", keyID, picID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddKeywordToPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private int AddExif(int iDbID, ExifMetadata.Metadata exifData)
    {
      if (iDbID <= 0 || string.IsNullOrEmpty(exifData.DatePictureTaken.Value))
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }

      try
      {
        string strISO = exifData.ISO.DisplayValue;
        string strMeteringMode = exifData.MeteringMode.DisplayValue;
        string strFlash = exifData.Flash.DisplayValue;
        string strExposureTime = exifData.ExposureTime.DisplayValue;
        string strExposureProgram = exifData.ExposureProgram.DisplayValue;
        string strExposureMode = exifData.ExposureMode.DisplayValue;
        string strExposureCompensation = exifData.ExposureCompensation.DisplayValue;
        string strFStop = exifData.Fstop.DisplayValue;
        string strShutterSpeed = exifData.ShutterSpeed.DisplayValue;
        string strSensingMethod = exifData.SensingMethod.DisplayValue;
        string strSceneType = exifData.SceneType.DisplayValue;
        string strSceneCaptureType = exifData.SceneCaptureType.DisplayValue;
        string strWhiteBalance = exifData.WhiteBalance.DisplayValue;
        string strFocalLength = exifData.FocalLength.DisplayValue;
        string strFocalLength35 = exifData.FocalLength35MM.DisplayValue;
        string strAuthor = exifData.Author.DisplayValue;
        string strUserComment = exifData.Comment.DisplayValue;
        string strByline = exifData.ByLine.DisplayValue;
        string strSoftware = exifData.ViewerComments.DisplayValue;
        string strCopyright = exifData.Copyright.DisplayValue;
        string strCopyrightNotice = exifData.CopyrightNotice.DisplayValue;
        string strGPSLatitude = exifData.Latitude.DisplayValue;
        string strGPSLongitude = exifData.Longitude.DisplayValue;
        string strGPSAltitude = exifData.Altitude.DisplayValue;

        DatabaseUtility.RemoveInvalidChars(ref strISO);
        DatabaseUtility.RemoveInvalidChars(ref strMeteringMode);
        DatabaseUtility.RemoveInvalidChars(ref strFlash);
        DatabaseUtility.RemoveInvalidChars(ref strExposureTime);
        DatabaseUtility.RemoveInvalidChars(ref strExposureProgram);
        DatabaseUtility.RemoveInvalidChars(ref strExposureMode);
        DatabaseUtility.RemoveInvalidChars(ref strExposureCompensation);
        DatabaseUtility.RemoveInvalidChars(ref strFStop);
        DatabaseUtility.RemoveInvalidChars(ref strShutterSpeed);
        DatabaseUtility.RemoveInvalidChars(ref strSensingMethod);
        DatabaseUtility.RemoveInvalidChars(ref strSceneType);
        DatabaseUtility.RemoveInvalidChars(ref strSceneCaptureType);
        DatabaseUtility.RemoveInvalidChars(ref strWhiteBalance);
        DatabaseUtility.RemoveInvalidChars(ref strFocalLength);
        DatabaseUtility.RemoveInvalidChars(ref strFocalLength35);
        DatabaseUtility.RemoveInvalidChars(ref strAuthor);
        DatabaseUtility.RemoveInvalidChars(ref strUserComment);
        DatabaseUtility.RemoveInvalidChars(ref strByline);
        DatabaseUtility.RemoveInvalidChars(ref strSoftware);
        DatabaseUtility.RemoveInvalidChars(ref strCopyright);
        DatabaseUtility.RemoveInvalidChars(ref strCopyrightNotice);
        DatabaseUtility.RemoveInvalidChars(ref strGPSLatitude);
        DatabaseUtility.RemoveInvalidChars(ref strGPSLongitude);
        DatabaseUtility.RemoveInvalidChars(ref strGPSAltitude);

        string strSQL = String.Format("SELECT * FROM exifdata WHERE idPicture = '{0}'", iDbID);
        SQLiteResultSet results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("INSERT INTO exifdata (idExif, idPicture, " +
                                                       "strISO, " +
                                                       "iMeteringMode, strMeteringMode, " +
                                                       "iFlash, strFlash, " +
                                                       "strExposureTime, strExposureProgram, strExposureMode, strExposureCompensation, " + 
                                                       "strFStop, strShutterSpeed, " +
                                                       "strSensingMethod, strSceneType, strSceneCaptureType, " +
                                                       "strWhiteBalance, " +
                                                       "strFocalLength, strFocalLength35, " +
                                                       "strAuthor, strUserComment, strByline, " +
                                                       "strSoftware, " +
                                                       "strCopyright, strCopyrightNotice, " +
                                                       "strGPSLatitude, strGPSLongitude, strGPSAltitude) " +
                                 "VALUES (NULL, '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}'," + 
                                               "'{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{22}', '{23}', '{24}', '{25}', '{26}');",
                                  iDbID, strISO, 
                                  exifData.MeteringMode.Value, strMeteringMode,
                                  exifData.Flash.Value, strFlash,
                                  strExposureTime, strExposureProgram, strExposureMode, strExposureCompensation,
                                  strFStop, strShutterSpeed,
                                  strSensingMethod, strSceneType, strSceneCaptureType,
                                  strWhiteBalance, 
                                  strFocalLength, strFocalLength35,
                                  strAuthor, strUserComment, strByline,
                                  strSoftware, 
                                  strCopyright, strCopyrightNotice, 
                                  strGPSLatitude, strGPSLongitude, strGPSAltitude);
          m_db.Execute(strSQL);
          int iID = m_db.LastInsertID();
          return iID;
        }
        else
        {
          int iID;
          if (Int32.TryParse(DatabaseUtility.Get(results, 0, "idExif"), out iID))
          {
            strSQL = String.Format("UPDATE exifdata SET strISO = '{0}', " +
                                                       "iMeteringMode = '{1}', strMeteringMode = '{2}', " +
                                                       "iFlash = '{3}', strFlash = '{4}', " +
                                                       "strExposureTime = '{5}', strExposureProgram = '{6}', strExposureMode = '{7}', strExposureCompensation = '{8}', " + 
                                                       "strFStop = '{9}', strShutterSpeed = '{10}', " +
                                                       "strSensingMethod = '{11}', strSceneType = '{12}', strSceneCaptureType = '{13}', " +
                                                       "strWhiteBalance = '{14}', " +
                                                       "strFocalLength = '{15}', strFocalLength35 = '{16}', " +
                                                       "strAuthor = '{17}', strUserComment = '{18}', strByline = '{19}', " +
                                                       "strSoftware = '{20}', " +
                                                       "strCopyright = '{21}', strCopyrightNotice = '{22}', " +
                                                       "strGPSLatitude = '{23}', strGPSLongitude = '{24}', strGPSAltitude = '{25}' " +
                                   "WHERE idExif = '{26}'",
                                    strISO, 
                                    exifData.MeteringMode.Value, strMeteringMode,
                                    exifData.Flash.Value, strFlash,
                                    strExposureTime, strExposureProgram, strExposureMode, strExposureCompensation,
                                    strFStop, strShutterSpeed,
                                    strSensingMethod, strSceneType, strSceneCaptureType,
                                    strWhiteBalance, 
                                    strFocalLength, strFocalLength35,
                                    strAuthor, strUserComment, strByline,
                                    strSoftware, 
                                    strCopyright, strCopyrightNotice, 
                                    strGPSLatitude, strGPSLongitude, strGPSAltitude,
                                    iDbID);
            return iID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseSqlLite: AddExif exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private bool GetExifDetails(string strPicture, ref int iRotation, ref string strDateTaken, ref ExifMetadata.Metadata metaData)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
      {
        return false;
      }

      using (ExifMetadata extractor = new ExifMetadata())
      {
        metaData = extractor.GetExifMetadata(strPicture);
        try
        {
          strDateTaken = metaData.DatePictureTaken.Value;
          if (!String.IsNullOrEmpty(strDateTaken))
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
          Log.Error("PictureDatabaseSqlLite: Exif conversion exception err: {0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
      strDateTaken = string.Empty;
      return false;
    }

    private bool GetPicasaRotation(string strPic, ref int iRotation)
    {
      bool foundValue = false;
      if (File.Exists(Path.GetDirectoryName(strPic) + "\\Picasa.ini"))
      {
        using (StreamReader sr = File.OpenText(Path.GetDirectoryName(strPic) + "\\Picasa.ini"))
        {
          try
          {
            string s = "";
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
                      Log.Error("MyPictures: error converting number picasa.ini", ex.Message, ex.StackTrace);
                    }
                    searching = false;
                  }
                } while (s != null && !s.StartsWith("[") && searching);
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("MyPictures: file read problem picasa.ini", ex.Message, ex.StackTrace);
          }
        }
      }
      return foundValue;
    }

    public void DeletePicture(string strPicture)
    {
      lock (typeof (PictureDatabase))
      {
        // Continue only if it's a picture files
        if (!Util.Utils.IsPicture(strPicture))
          return;

        if (m_db == null)
        {
          return;
        }
        string strSQL = "";
        try
        {
          string strPic = strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          strSQL = String.Format("delete from picture where strFile like '{0}'", strPic);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("MediaPortal.Picture.Database exception deleting picture err:{0} stack:{1}", ex.Message,
                    ex.StackTrace);
          Open();
        }
        return;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int GetRotation(string strPicture)
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

      try
      {
        string strPic = strPicture;
        int iRotation = 0;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        SQLiteResultSet results = m_db.Execute(String.Format("select strFile, iRotation from picture where strFile like '{0}'", strPic));
        if (results != null && results.Rows.Count > 0)
        {
          iRotation = Int32.Parse(DatabaseUtility.Get(results, 0, 1));
          return iRotation;
        }

        if (_useExif)
        {
          iRotation = Util.Picture.GetRotateByExif(strPicture);
          Log.Debug("PictureDatabaseSqlLite: GetRotateByExif = {0} for {1}", iRotation, strPicture);
        }

        AddPicture(strPicture, iRotation);

        return iRotation;
      }
      catch (Exception ex)
      {
        Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetRotation(string strPicture, int iRotation)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
        return;

      if (m_db == null)
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
          /*SQLiteResultSet results = */
          m_db.Execute(String.Format("update picture set iRotation={0} where strFile like '{1}'", iRotation, strPic));
        }
      }
      catch (Exception ex)
      {
        Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int EXIFOrientationToRotation(int orientation)
    {
      return orientation.ToRotation();
    }

    public int ListYears(ref List<string> Years)
    {
      int Count = 0;
      lock (typeof (PictureDatabase))
      {
        if (m_db == null)
        {
          return 0;
        }
        string strSQL = "select distinct substr(strDateTaken,1,4) from picture order by 1";
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
          Log.Error("MediaPortal.Picture.Database exception getting Years err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return Count;
      }
    }

    public int ListMonths(string Year, ref List<string> Months)
    {
      int Count = 0;
      lock (typeof (PictureDatabase))
      {
        if (m_db == null)
        {
          return 0;
        }
        string strSQL = "select distinct substr(strDateTaken,6,2) from picture where strDateTaken like '" + Year +
                        "%' order by 1";
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
          Log.Error("MediaPortal.Picture.Database exception getting Months err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return Count;
      }
    }

    public int ListDays(string Month, string Year, ref List<string> Days)
    {
      int Count = 0;
      lock (typeof (PictureDatabase))
      {
        if (m_db == null)
        {
          return 0;
        }
        string strSQL = "select distinct substr(strDateTaken,9,2) from picture where strDateTaken like '" + Year + "-" +
                        Month + "%' order by 1";
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
          Log.Error("MediaPortal.Picture.Database exception getting Days err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return Count;
      }
    }

    public int ListPicsByDate(string Date, ref List<string> Pics)
    {
      int Count = 0;
      lock (typeof (PictureDatabase))
      {
        if (m_db == null)
        {
          return 0;
        }
        string strSQL = "select strFile from picture where strDateTaken like '" + Date + "%' order by 1";
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
          Log.Error("MediaPortal.Picture.Database exception getting Picture by Date err:{0} stack:{1}", ex.Message,
                    ex.StackTrace);
          Open();
        }
        return Count;
      }
    }

    public int CountPicsByDate(string Date)
    {
      int Count = 0;
      lock (typeof (PictureDatabase))
      {
        if (m_db == null)
        {
          return 0;
        }
        string strSQL = "select count(strFile) from picture where strDateTaken like '" + Date + "%' order by 1";
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
          Log.Error("MediaPortal.Picture.Database exception getting Picture by Date err:{0} stack:{1}", ex.Message,
                    ex.StackTrace);
          Open();
        }
        return Count;
      }
    }

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
        return "";
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
            Log.Error("PictureDatabaseSqlLite:Dispose: {0}", ex.Message);
          }
          m_db = null;
        }
      }
    }

    #endregion
  }
}