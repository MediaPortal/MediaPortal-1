/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections;
using System.Globalization;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Database;

namespace MediaPortal.Picture.Database
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class PictureDatabaseSqlLite : IPictureDatabase, IDisposable
  {
    bool disposed = false;
    SQLiteClient m_db = null;

    public PictureDatabaseSqlLite()
    {

      Open();
    }

    void Open()
    {
      lock (typeof(PictureDatabase))
      {
        Log.Info("opening picture database");
        try
        {
          // Open database
          try
          {
            System.IO.Directory.CreateDirectory(Config.Get(Config.Dir.Database));
          }
          catch (Exception) { }
          m_db = new SQLiteClient(Config.Get(Config.Dir.Database) + "PictureDatabase.db3");

          DatabaseUtility.SetPragmas(m_db);
          CreateTables();

        }
        catch (Exception ex)
        {
          Log.Error("picture database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        Log.Info("picture database opened");
      }
    }
    bool CreateTables()
    {
      lock (typeof(PictureDatabase))
      {
        if (m_db == null) return false;
        //Changed mbuzina
        DatabaseUtility.AddTable(m_db, "picture", "CREATE TABLE picture ( idPicture integer primary key, strFile text, iRotation integer, strDateTaken text)");
        //End Changed
        return true;
      }
    }
    public int AddPicture(string strPicture, int iRotation)
    {
      if (strPicture == null) return -1;
      if (strPicture.Length == 0) return -1;
      lock (typeof(PictureDatabase))
      {
        if (m_db == null) return -1;
        string strSQL = "";
        try
        {
          int lPicId = -1;
          SQLiteResultSet results;
          string strPic = strPicture;
          string strDateTaken = "";
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          strSQL = String.Format("select * from picture where strFile like '{0}'", strPic);
          results = m_db.Execute(strSQL);
          if (results != null && results.Rows.Count > 0)
          {
            lPicId = System.Int32.Parse(DatabaseUtility.Get(results, 0, "idPicture"));
            return lPicId;
          }
          //Changed mbuzina
          using (ExifMetadata extractor = new ExifMetadata())
          {
            ExifMetadata.Metadata metaData = extractor.GetExifMetadata(strPic);
            try
            {
              //Exception here!!! (very bad since the insert doesn't happen then..
              //  DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
              //  dateTimeFormat.ShortDatePattern = "yyyy:MM:dd HH:mm:ss";
              string picExifDate = metaData.DatePictureTaken.DisplayValue;
              //DateTime dat = DateTime.ParseExact(picExifDate, "d", dateTimeFormat);

              DateTime dat;
              DateTimeStyles mpPicStyle;
              mpPicStyle = DateTimeStyles.None;
              DateTime.TryParseExact(picExifDate, "G", System.Threading.Thread.CurrentThread.CurrentCulture, mpPicStyle, out dat);
              strDateTaken = dat.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (System.FormatException ex)
            {
              Log.Error("date conversion exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
            }
            iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));
          }

          strSQL = String.Format("insert into picture (idPicture, strFile, iRotation, strDateTaken) values(null, '{0}',{1},'{2}')", strPic, iRotation, strDateTaken);
          //End Changed

          results = m_db.Execute(strSQL);
          lPicId = m_db.LastInsertID();
          return lPicId;
        }
        catch (Exception ex)
        {
          Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return -1;
      }
    }

    public void DeletePicture(string strPicture)
    {
      lock (typeof(PictureDatabase))
      {
        if (m_db == null) return;
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
          Log.Error("MediaPortal.Picture.Database exception deleting picture err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return;
      }
    }

    public int GetRotation(string strPicture)
    {
      lock (typeof(PictureDatabase))
      {
        if (m_db == null) return -1;
        string strSQL = "";
        try
        {
          SQLiteResultSet results;
          string strPic = strPicture;
          int iRotation;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          strSQL = String.Format("select * from picture where strFile like '{0}'", strPic);
          results = m_db.Execute(strSQL);
          if (results != null && results.Rows.Count > 0)
          {
            iRotation = System.Int32.Parse(DatabaseUtility.Get(results, 0, "iRotation"));
            return iRotation;
          }

          ExifMetadata extractor = new ExifMetadata();
          ExifMetadata.Metadata metaData = extractor.GetExifMetadata(strPicture);
          iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));

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
    }

    public void SetRotation(string strPicture, int iRotation)
    {
      lock (typeof(PictureDatabase))
      {
        if (m_db == null) return;
        string strSQL = "";
        try
        {
          SQLiteResultSet results;
          string strPic = strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          long lPicId = AddPicture(strPicture, iRotation);
          if (lPicId >= 0)
          {
            strSQL = String.Format("update picture set iRotation={0} where strFile like '{1}'", iRotation, strPic);
            results = m_db.Execute(strSQL);
          }
        }
        catch (Exception ex)
        {
          Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    //Changed mbuzina
    public DateTime GetDateTaken(string strPicture)
    {
      lock (typeof(PictureDatabase))
      {
        if (m_db == null) return DateTime.MinValue;
        string strSQL = "";
        try
        {
          SQLiteResultSet results;
          string strPic = strPicture;
          string strDateTime;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          strSQL = String.Format("select * from picture where strFile like '{0}'", strPic);
          results = m_db.Execute(strSQL);
          if (results != null && results.Rows.Count > 0)
          {
            strDateTime = DatabaseUtility.Get(results, 0, "strDateTaken");
            if (strDateTime != String.Empty && strDateTime != "")
            {
              DateTime dtDateTime = DateTime.ParseExact(strDateTime, "yyyy-MM-dd HH:mm:ss", new System.Globalization.CultureInfo(""));
              return dtDateTime;
            }
          }
          AddPicture(strPicture, -1);
          using (ExifMetadata extractor = new ExifMetadata())
          {
            ExifMetadata.Metadata metaData = extractor.GetExifMetadata(strPic);
            strDateTime = System.DateTime.Parse(metaData.DatePictureTaken.DisplayValue).ToString("yyyy-MM-dd HH:mm:ss");
          }
          if (strDateTime != String.Empty && strDateTime != "")
          {
            DateTime dtDateTime = DateTime.ParseExact(strDateTime, "yyyy-MM-dd HH:mm:ss", new System.Globalization.CultureInfo(""));
            return dtDateTime;
          }
          else
          {
            return DateTime.MinValue;
          }
        }
        catch (Exception ex)
        {
          Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return DateTime.MinValue;
      }
    }
    //End Changed

    public int EXIFOrientationToRotation(int orientation)
    {
      Log.Info("Orientation: {0}", orientation);

      if (orientation == 6)
        return 1;

      if (orientation == 3)
        return 2;

      if (orientation == 8)
        return 3;

      return 0;
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
          catch (Exception) { }
          m_db = null;
        }
      }
    }

    #endregion
  }
}
