using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MediaPortal.Utils.Services;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Database;

namespace MediaPortal.Picture.Database
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class PictureDatabaseADO : IPictureDatabase, IDisposable
  {
    SqlConnection _connection;
    protected ILog _log;
    protected IConfig _config;

    public PictureDatabaseADO()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      _config = services.Get<IConfig>();

      string connectionString;
      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(_config.Get(Config.Options.ConfigPath) + "mediaportal.xml"))
      {
        connectionString = reader.GetValueAsString("database", "connectionstring", SqlServerUtility.DefaultConnectionString);
      }
      _connection = new SqlConnection(connectionString);
      _connection.Open();
      CreateTables();
    }
    void CreateTables()
    {
      SqlServerUtility.AddTable(_connection, "tblPicture", "CREATE TABLE tblPicture ( idPicture int IDENTITY(1,1) NOT NULL, strFile varchar(2048), iRotation int, strDateTaken datetime)");
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
      if (strPicture == null) return -1;
      if (strPicture.Length == 0) return -1;
      lock (typeof(PictureDatabase))
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
            catch (Exception)
            {
            }
            // Smirnoff: Query the orientation information
            //						if(iRotation == -1)
            iRotation = EXIFOrientationToRotation(Convert.ToInt32(metaData.Orientation.Hex));
          }
          strSQL = String.Format("insert into tblPicture (strFile, iRotation, strDateTaken) values('{0}',{1},'{2}')", strPic, iRotation, dateTaken);
          return SqlServerUtility.InsertRecord(_connection, strSQL);
        }
        catch (Exception ex)
        {
          _log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
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
        SqlServerUtility.ExecuteNonQuery(_connection,strSQL);
      }
      catch (Exception ex)
      {
        _log.Error("MediaPortal.Picture.Database exception deleting picture err:{0} stack:{1}", ex.Message, ex.StackTrace);

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
        _log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);

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
          SqlServerUtility.ExecuteNonQuery(_connection,strSQL);
        }
      }
      catch (Exception ex)
      {
        _log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);

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
        _log.Error("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return DateTime.MinValue;
    }

    public int EXIFOrientationToRotation(int orientation)
    {

      if (orientation == 6)
        return 1;

      if (orientation == 3)
        return 2;

      if (orientation == 8)
        return 3;

      return 0;
    }
  }
}