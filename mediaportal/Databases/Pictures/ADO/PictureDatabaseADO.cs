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
using System.Data;
using System.Linq;
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
using MediaPortal.Profile;

namespace MediaPortal.Picture.Database
{
  /// <summary>
  /// Summary description for PictureDatabaseSqlLite.
  /// </summary>
  public class PictureDatabaseADO : IPictureDatabase, IDisposable
  {
    private bool _useExif = true;
    private bool _usePicasa = false;
    private Databases.picturedatabaseEntities _connection;
    private bool _dbHealth = false;
    private string _connectionString;

    public PictureDatabaseADO()
    {
      ConnectDb();

      Thread threadCreateDb = new Thread(CreateDb);
      threadCreateDb.Priority = ThreadPriority.Lowest;
      threadCreateDb.IsBackground = true;
      threadCreateDb.Name = "CreatePictureDBThread";
      threadCreateDb.Start();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void ConnectDb()
    {
      try
      {
        string host;
        string userName;
        string password;
        using (Settings reader = new MPSettings())
        {
          host = reader.GetValueAsString("mpdatabase", "hostname", string.Empty);
          userName = reader.GetValueAsString("mpdatabase", "username", "root");
          password = reader.GetValueAsString("mpdatabase", "password", "MediaPortal");

          if (host == string.Empty)
          {
            host = reader.GetValueAsString("tvservice", "hostname", "localhost");
          }
        }

        string ConnectionString = string.Format(
          "metadata=res://*/Model3.csdl|res://*/Model3.ssdl|res://*/Model3.msl;provider=MySql.Data.MySqlClient;provider connection string=\"server={0};user id={1};password={2};persistsecurityinfo=True;database=picturedatabase;Convert Zero Datetime=True;charset=utf8\"",
          host, userName, password);

         _connection = new Databases.picturedatabaseEntities(ConnectionString);
         _connectionString = string.Format("server={0};user id={1};password={2}", host, userName, password);
      }
      catch (Exception ex)
      {
        Log.Error("PicturedatabaseADO:ConnectDb exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, ex.InnerException);
      }
    }

    private bool IsConnected()
    {
      if (_connection == null)
      {
        return false;
      }
      else
        if (_dbHealth)
        {
          return true;
        }
        else
        {
          CreateDb();
          return _dbHealth;
        }
    }

    private bool WaitForConnected()
    {
      for (int i = 1; i < 30; i++)
      {
        try
        {
          if (_connection == null)
            return false;
          
          _connection.DatabaseExists();
          return true;
        }
        catch (Exception ex)
        {
          if (i < 29)
          {
            Log.Debug("PicturedatabaseADO:WaitForConnected trying to connect to the database. {0}", i);
          }
          else
          {
            Log.Error("PicturedatabaseADO:WaitForConnected exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, ex.InnerException);
          }
        }
        Thread.Sleep(500);
      }
      return false;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void CreateDb()
    {
      if (_connection == null)
        return;
      
      string host;
      using (Settings reader = new MPSettings())
      {
        host = reader.GetValueAsString("mpdatabase", "hostname", string.Empty);

        if (host == string.Empty)
        {
          host = reader.GetValueAsString("tvservice", "hostname", "localhost");
        }
      }

      if (!WakeupUtil.HandleWakeUpServer(host))
      {
        Log.Error("PicturedatabaseADO: database host is not connected.");
      }

      WaitForConnected();

      try
      {
        if (_connection == null)
          return;

        if (!_connection.DatabaseExists())
        {
          Log.Debug("PicturedatabaseADO: database is not exist, createing...");
          System.Reflection.Assembly assembly = this.GetType().Assembly;

          string DatabaseName = assembly.GetName().Name + ".Video.Ado.create_videodatabase.sql";
          _dbHealth = DatabaseUtility.CreateDb(_connectionString, DatabaseName);
        }
        else
        {
          _dbHealth = true;
          Log.Debug("PicturedatabaseADO: database is connected.");
        }
      }
      catch (Exception ex)
      {
        Log.Error("PicturedatabaseADO:CreateDb exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, ex.InnerException);
      }
    }

    public bool ClearDB()
    {
      if (!IsConnected())
      {
        return false;
      }

      try
      {
        _connection.ExecuteStoreCommand("drop database picturedatabase");
        Log.Debug("PicturedatabaseADO:ClearDB picturedatabase is dropped.");
        CreateDb();
      }
      catch (Exception ex)
      {
        Log.Error("PicturedatabaseADO:ClearDB exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, ex.InnerException);
        return false;
      }
      return true;
    }

    public void Dispose()
    {
      if (_connection != null)
      {
        _connection.Dispose();
        _connection = null;
      }
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
    public int AddPicture(string strPicture, int iRotation)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
        return -1;

      if (String.IsNullOrEmpty(strPicture))
      {
        return -1;
      }

      if (!IsConnected())
      {
        return -1;
      }
      try
      {
        int lPicId = -1;
        string strPic = strPicture;
        string strDateTaken = String.Empty;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        var query = (from sql in _connection.pictures
                     where sql.strFile == strPic
                     select sql).FirstOrDefault();

        if (query != null)
        {
          return query.idPicture;
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
            Log.Error("PictureDatabaseADO:AddPicture Conversion exception getting file date - err:{0} stack:{1}", ex.Message,
                      ex.StackTrace);
          }
        }

        // Save potential performance penalty
        if (_usePicasa)
        {
          if (GetPicasaRotation(strPic, ref iRotation))
          {
            Log.Debug("PictureDatabaseADO:AddPicture Changed rotation of image {0} based on picasa file to {1}", strPic,
                      iRotation);
          }
        }

        Databases.picture obj = new Databases.picture()
        {
          strFile = strPic,
          iRotation = iRotation,
          strDateTaken = strDateTaken
        };

        _connection.pictures.AddObject(obj);
        _connection.SaveChanges();

        Log.Debug("PictureDatabaseSqlLite:AddPicture Added to database - {0}", strPic);

        var query2 = (from sql in _connection.pictures
                     where sql.strFile == strPic
                     select sql).FirstOrDefault();

        lPicId = query2.idPicture;

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
        Log.Error("PictureDatabaseADO:AddPicture exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        ConnectDb();
      }
      return -1;
    }

    private bool GetExifDetails(string strPicture, ref int iRotation, ref string strDateTaken)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
        return false;

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
          Log.Error("PictureDatabaseADO:GetExifDetails Exif date conversion exception err:{0} stack:{1}", ex.Message,
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
      lock (typeof(PictureDatabase))
      {
        // Continue only if it's a picture files
        if (!Util.Utils.IsPicture(strPicture))
          return;

        if (!IsConnected())
        {
          return;
        }

        try
        {
          string strPic = strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          var query = (from sql in _connection.pictures
                       where sql.strFile == strPic
                       select sql).FirstOrDefault();

          if (query != null)
          {
            _connection.DeleteObject(query);
            _connection.SaveChanges();
          }
        }
        catch (Exception ex)
        {
          Log.Error("PictureDatabaseADO:DeletePicture exception deleting picture err:{0} stack:{1}", ex.Message,
                    ex.StackTrace);
          ConnectDb();
        }
        return;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int GetRotation(string strPicture)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
        return -1;

      if (!IsConnected())
      {
        return -1;
      }
      try
      {
        string strPic = strPicture;
        int iRotation = 0;
        DatabaseUtility.RemoveInvalidChars(ref strPic);

        var query = (from sql in _connection.pictures
                     where sql.strFile == strPic
                     select sql).FirstOrDefault();

        if (query != null)
        {
          return (int)query.iRotation;
        }

        if (_useExif)
        {
          iRotation = Util.Picture.GetRotateByExif(strPicture);
          Log.Debug("PictureDatabaseADO:GetRotation GetRotateByExif = {0} for {1}", iRotation, strPicture);
        }

        AddPicture(strPicture, iRotation);

        return iRotation;
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseADO:GetRotation exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        ConnectDb();
      }
      return 0;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetRotation(string strPicture, int iRotation)
    {
      // Continue only if it's a picture files
      if (!Util.Utils.IsPicture(strPicture))
        return;

      if (!IsConnected())
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
          var query = (from sql in _connection.pictures
                       where sql.strFile == strPic
                       select sql).FirstOrDefault();

          query.iRotation = iRotation;
          _connection.SaveChanges();
        }
      }
      catch (Exception ex)
      {
        Log.Error("PictureDatabaseADO:SetRotation exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        ConnectDb();
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
      lock (typeof(PictureDatabase))
      {
        if (!IsConnected())
        {
          return 0;
        }

        try
        {
          var query = (from sql in _connection.pictures
                       select new { result = sql.strDateTaken.Substring(0, 4) }).Distinct().OrderBy(x => x.result).ToList();
        
          if (query.Count != 0)
          {
            for (Count = 0; Count < query.Count; Count++)
            {
              Years.Add(query[Count].result);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("PictureDatabaseADO:ListYears exception getting Years err:{0} stack:{1}", ex.Message, ex.StackTrace);
          ConnectDb();
        }
        return Count;
      }
    }

    public int ListMonths(string Year, ref List<string> Months)
    {
      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        if (!IsConnected())
        {
          return 0;
        }
        try
        {
          var query = (from sql in _connection.pictures
                       where sql.strDateTaken.Substring(0, 4) == Year
                       select new { result = sql.strDateTaken.Substring(5, 2) }).Distinct().OrderBy(x => x.result).ToList();

          if (query.Count != 0)
          {
            for (Count = 0; Count < query.Count; Count++)
            {
              Months.Add(query[Count].result);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("PictureDatabaseADO:ListMonths exception getting Months err:{0} stack:{1}", ex.Message, ex.StackTrace);
          ConnectDb();
        }
        return Count;
      }
    }

    public int ListDays(string Month, string Year, ref List<string> Days)
    {
      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        if (!IsConnected())
        {
          return 0;
        }
        try
        {
          var query = (from sql in _connection.pictures
                       where sql.strDateTaken.Substring(0, 7) == Year + "-" + Month
                       select new { result = sql.strDateTaken.Substring(8, 2) }).Distinct().OrderBy(x => x.result).ToList();

          if (query.Count != 0)
          {
            for (Count = 0; Count < query.Count; Count++)
            {
              Days.Add(query[Count].result);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("PictureDatabaseADO:ListDays exception getting Days err:{0} stack:{1}", ex.Message, ex.StackTrace);
          ConnectDb();
        }
        return Count;
      }
    }

    public int ListPicsByDate(string Date, ref List<string> Pics)
    {
      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        if (!IsConnected())
        {
          return 0;
        }
        try
        {
          var query = (from sql in _connection.pictures
                       where sql.strDateTaken.StartsWith(Date)
                       orderby sql.strDateTaken
                       select sql).ToList();

          if (query.Count != 0)
          {
            for (Count = 0; Count < query.Count; Count++)
            {
              Pics.Add(query[Count].strFile);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("PictureDatabaseADO:ListPicsByDate exception getting Picture by Date err:{0} stack:{1}", ex.Message,
                    ex.StackTrace);
          ConnectDb();
        }
        return Count;
      }
    }

    public int CountPicsByDate(string Date)
    {
      int Count = 0;
      lock (typeof(PictureDatabase))
      {
        if (!IsConnected())
        {
          return 0;
        }
        try
        {
          var query = (from sql in _connection.pictures
                       where sql.strDateTaken.StartsWith(Date)
                       select sql).ToList();

          if (query != null)
          {
            Count = query.Count;
          }
        }
        catch (Exception ex)
        {
          Log.Error("PictureDatabaseADO:CountPicsByDate exception getting Picture by Date err:{0} stack:{1}", ex.Message,
                    ex.StackTrace);
          ConnectDb();
        }
        return Count;
      }
    }

    public bool DbHealth
    {
      get
      {
        return IsConnected();
      }
    }

    public string DatabaseName
    {
      get
      {
        if (_connection != null)
        {
          return "Picturedatabase";
        }
        return "";
      }
    }

  }
}