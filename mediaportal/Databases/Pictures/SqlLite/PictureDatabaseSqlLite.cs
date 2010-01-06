#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Runtime.CompilerServices;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Player;
using SQLite.NET;

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
          catch (Exception) {}
        }

        // Open database
        try
        {
          Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception) {}
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "PictureDatabase.db3"));
        // Retry 10 times on busy (DB in use or system resources exhausted)
        m_db.BusyRetries = 10;
        // Wait 100 ms between each try (default 10)
        m_db.BusyRetryDelay = 100;

        DatabaseUtility.SetPragmas(m_db);
        CreateTables();
        InitSettings();
      }
      catch (Exception ex)
      {
        Log.Error("picture database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      Log.Info("picture database opened");
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
      DatabaseUtility.AddTable(m_db, "picture",
                               "CREATE TABLE picture ( idPicture integer primary key, strFile text, iRotation integer, strDateTaken text)");
      return true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int AddPicture(string strPicture, int iRotation)
    {
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

        // we need the date nevertheless for database view / sorting
        if (!GetExifDetails(strPicture, ref iRotation, ref strDateTaken))
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
        strSQL =
          String.Format(
            "insert into picture (idPicture, strFile, iRotation, strDateTaken) values(null, '{0}',{1},'{2}')", strPic,
            iRotation, strDateTaken);

        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          Log.Debug("PictureDatabaseSqlLite: Added to database - {0}", strPic);
        }
        strSQL = "commit";
        results = m_db.Execute(strSQL);

        lPicId = m_db.LastInsertID();

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
        Log.Error("PictureDatabaseSqlLite: exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    private bool GetExifDetails(string strPicture, ref int iRotation, ref string strDateTaken)
    {
      using (ExifMetadata extractor = new ExifMetadata())
      {
        ExifMetadata.Metadata metaData = extractor.GetExifMetadata(strPicture);
        try
        {
          string picExifDate = metaData.DatePictureTaken.DisplayValue;
          if (!String.IsNullOrEmpty(picExifDate))
            // If the image contains a valid exif date store it in the database, otherwise use the file date
          {
            DateTime dat;
            DateTime.TryParseExact(picExifDate, "G", Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out dat);
            strDateTaken = dat.ToString("yyyy-MM-dd HH:mm:ss");
            if (_useExif)
            {
              iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));
            }
            return true;
          }
        }
        catch (FormatException ex)
        {
          Log.Error("PictureDatabaseSqlLite: Exif date conversion exception err:{0} stack:{1}", ex.Message,
                    ex.StackTrace);
        }
      }
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
              if (s.ToLower() == "[" + Path.GetFileName(strPic).ToLower() + "]")
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
      if (m_db == null)
      {
        return -1;
      }
      try
      {
        string strPic = strPicture;
        int iRotation = 0;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        SQLiteResultSet results =
          m_db.Execute(String.Format("select strFile, iRotation from picture where strFile like '{0}'", strPic));
        if (results != null && results.Rows.Count > 0)
        {
          iRotation = Int32.Parse(DatabaseUtility.Get(results, 0, 1));
          return iRotation;
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
      if (orientation == 6)
      {
        return 1;
      }
      if (orientation == 3)
      {
        return 2;
      }
      if (orientation == 8)
      {
        return 3;
      }
      return 0;
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
          catch (Exception) {}
          m_db = null;
        }
      }
    }

    #endregion
  }
}