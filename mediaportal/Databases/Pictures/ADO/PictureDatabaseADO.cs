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
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Profile;

namespace MediaPortal.Picture.Database
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class PictureDatabaseADO : IPictureDatabase, IDisposable
  {
    private SqlConnection _connection;

    public PictureDatabaseADO()
    {
      string connectionString;
      using (Settings reader = new MPSettings())
      {
        connectionString = reader.GetValueAsString("database", "connectionstring",
                                                   SqlServerUtility.DefaultConnectionString);
      }
      _connection = new SqlConnection(connectionString);
      _connection.Open();
      CreateTables();
    }

    private void CreateTables()
    {
      SqlServerUtility.AddTable(_connection, "tblPicture",
                                "CREATE TABLE tblPicture ( idPicture int IDENTITY(1,1) NOT NULL, strFile varchar(2048), iRotation int, strDateTaken datetime)");
      SqlServerUtility.AddPrimaryKey(_connection, "tblPicture", "idPicture");
    }

    public void Dispose()
    {
      if (_connection != null)
      {
        _connection.Close();
        _connection.Dispose();
        _connection = null;
      }
    }

    public int AddPicture(string strPicture, int iRotation)
    {
      if (strPicture == null)
      {
        return -1;
      }
      if (strPicture.Length == 0)
      {
        return -1;
      }
      lock (typeof (PictureDatabase))
      {
        string strSQL = "";
        try
        {
          string strPic = strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          strSQL = String.Format("select * from tblPicture where strFile like '{0}'", strPic);
          using (SqlCommand cmd = _connection.CreateCommand())
          {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSQL;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
              if (reader.Read())
              {
                int id = (int)reader["idPicture"];
                reader.Close();
                return id;
              }
              reader.Close();
            }
          }

          //Changed mbuzina
          DateTime dateTaken = DateTime.Now;
          using (ExifMetadata extractor = new ExifMetadata())
          {
            ExifMetadata.Metadata metaData = extractor.GetExifMetadata(strPic);
            try
            {
              DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
              dateTimeFormat.ShortDatePattern = "yyyy:MM:dd HH:mm:ss";

              dateTaken = DateTime.ParseExact(metaData.DatePictureTaken.DisplayValue, "d", dateTimeFormat);
            }
            catch (Exception) {}
            // Smirnoff: Query the orientation information
            //						if(iRotation == -1)
            iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));
          }
          strSQL = String.Format("insert into tblPicture (strFile, iRotation, strDateTaken) values('{0}',{1},'{2}')",
                                 strPic, iRotation, dateTaken);
          return SqlServerUtility.InsertRecord(_connection, strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
        return -1;
      }
    }

    public void DeletePicture(string strPicture)
    {
      string strSQL = "";
      try
      {
        string strPic = strPicture;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        strSQL = String.Format("delete from tblPicture where strFile like '{0}'", strPic);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("MediaPortal.Picture.Database exception deleting picture err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public int GetRotation(string strPicture)
    {
      string strSQL = "";
      try
      {
        string strPic = strPicture;
        int iRotation;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        strSQL = String.Format("select * from tblPicture where strFile like '{0}'", strPic);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              iRotation = (int)reader["iRotation"];
              reader.Close();
              return iRotation;
            }
            reader.Close();
          }
        }

        ExifMetadata extractor = new ExifMetadata();
        ExifMetadata.Metadata metaData = extractor.GetExifMetadata(strPicture);
        iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));

        AddPicture(strPicture, iRotation);
        return 0;
      }
      catch (Exception ex)
      {
        Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return 0;
    }

    public void SetRotation(string strPicture, int iRotation)
    {
      string strSQL = "";
      try
      {
        string strPic = strPicture;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        long lPicId = AddPicture(strPicture, iRotation);
        if (lPicId >= 0)
        {
          strSQL = String.Format("update tblPicture set iRotation={0} where strFile like '{1}'", iRotation, strPic);
          SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public DateTime GetDateTaken(string strPicture)
    {
      string strSQL = "";
      try
      {
        string strPic = strPicture;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        strSQL = String.Format("select * from tblPicture where strFile like '{0}'", strPic);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              DateTime dtDateTime = (DateTime)reader["strDateTaken"];
              reader.Close();
              return dtDateTime;
            }
            reader.Close();
          }
        }
        AddPicture(strPicture, -1);
        string strDateTime;
        using (ExifMetadata extractor = new ExifMetadata())
        {
          ExifMetadata.Metadata metaData = extractor.GetExifMetadata(strPic);
          strDateTime = DateTime.Parse(metaData.DatePictureTaken.DisplayValue).ToString("yyyy-MM-dd HH:mm:ss");
        }
        if (strDateTime != string.Empty && strDateTime != "")
        {
          DateTime dtDateTime = DateTime.ParseExact(strDateTime, "yyyy-MM-dd HH:mm:ss", new CultureInfo(""));
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
      }
      return DateTime.MinValue;
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
      throw (new NotImplementedException("List Years not yet implemented for ADO Database"));
    }

    public int ListMonths(string Year, ref List<string> Months)
    {
      throw (new NotImplementedException("List Months not yet implemented for ADO Database"));
    }

    public int ListDays(string Month, string Year, ref List<string> Days)
    {
      throw (new NotImplementedException("List Days not yet implemented for ADO Database"));
    }

    public int ListPicsByDate(string Date, ref List<string> Pics)
    {
      throw (new NotImplementedException("List Pics by Date not yet implemented for ADO Database"));
    }

    public int CountPicsByDate(string Date)
    {
      throw (new NotImplementedException("Count Pics by Date not yet implemented for ADO Database"));
    }

    public string DatabaseName
    {
      get { return _connection.ConnectionString; }
    }
  }
}