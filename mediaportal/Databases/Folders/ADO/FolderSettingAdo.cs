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
using System.IO;
using System.Linq;
using System.Collections;
using System.Threading;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace Databases.Folders.SqlServer
{
  public class FolderSettingAdo : IFolderSettings, IDisposable
  {
    private foldersettingEntities _connection;
    private bool _dbHealth = false;
    private string _connectionString;

    public FolderSettingAdo()
    {
      ConnectDb();

      Thread threadCreateDb = new Thread(CreateDb);
      threadCreateDb.Priority = ThreadPriority.Lowest;
      threadCreateDb.IsBackground = true;
      threadCreateDb.Name = "CreateFolderDBThread";
      threadCreateDb.Start();
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
            Log.Debug("FolderdatabaseADO:WaitForConnected trying to connect to the database. {0}", i);
          }
          else
          {
            Log.Error("FolderdatabaseADO:WaitForConnected exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, ex.InnerException);
          }
        }
        Thread.Sleep(500);
      }
      return false;
    }

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
          "metadata=res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;provider=MySql.Data.MySqlClient;provider connection string=\"server={0};user id={1};password={2};persistsecurityinfo=True;database=foldersetting;charset=utf8\"",
          host, userName, password);

        _connection = new foldersettingEntities(ConnectionString);
        _connectionString = string.Format("server={0};user id={1};password={2}", host, userName, password);
      }
      catch (Exception ex)
      {
        Log.Error("FolderdatabaseADO:ConnectDb exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, ex.InnerException);
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

    public void Dispose()
    {
      if (_connection != null)
      {
        _connection.Dispose();
        _connection = null;
      }
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
        Log.Error("FolderSettingADO: database host is not connected.");
      }

      WaitForConnected();

      try
      {
        if (!_connection.DatabaseExists())
        {
          Log.Debug("FolderSettingADO: database is not exist, createing...");
          System.Reflection.Assembly assembly = this.GetType().Assembly;

          string DatabaseName = assembly.GetName().Name + ".Folders.ADO.create_folderdatabase.sql";
          _dbHealth = DatabaseUtility.CreateDb(_connectionString, DatabaseName);
        }
        else
        {
          _dbHealth = true;
          Log.Debug("FolderSettingADO: database is connected.");
        }
      }
      catch (Exception ex)
      {
        Log.Error("FolderSettingADO:CreateDb exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, ex.InnerException);
      }
    }

    private int AddPath(string filteredPath)
    {
      if (!IsConnected())
      {
        return -1;
      }
      
      if (string.IsNullOrEmpty(filteredPath))
      {
        return -1;
      }

      try
      {
        var query = (from sql in _connection.tblpaths 
                     where sql.strPath == filteredPath 
                     select sql).FirstOrDefault();
        if (query == null)
        {
          // doesnt exists, add it
          Log.Debug("FolderSettingADO: AddPath doesnt exists, add it  {0}", filteredPath);
          tblpath path = new tblpath()
          {
            strPath = filteredPath
          };

          _connection.tblpaths.AddObject(path);
          _connection.SaveChanges();

          var query2 = (from sql in _connection.tblpaths
                       where sql.strPath == filteredPath
                       select sql).FirstOrDefault();

          return query2.idPath;
        }
        else
        {
          return query.idPath;
        }
      }
      catch (Exception ex)
      {
        Log.Error("FolderSettingADO:AddPath exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return -1;
    }

    public void DeleteFolderSetting(string path, string Key)
    {
      if (!IsConnected())
      {
        return;
      }

      if (string.IsNullOrEmpty(path))
      {
        return;
      }

      if (string.IsNullOrEmpty(Key))
      {
        return;
      }

      try
      {
        string pathFiltered = Utils.RemoveTrailingSlash(path);
        string keyFiltered = Key;
        DatabaseUtility.RemoveInvalidChars(ref pathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref keyFiltered);

        int PathId = AddPath(pathFiltered);
        if (PathId < 0)
        {
          return;
        }

        var delObj = (from u in _connection.tblsettings
                     where u.idPath == PathId && u.tagName == keyFiltered
                      select u).FirstOrDefault();

        if (delObj != null)
        {
          _connection.DeleteObject(delObj);
          _connection.SaveChanges();
        }

      }
      catch (Exception ex)
      {
        Log.Error("FolderSettingADO:DeleteFolderSetting exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void AddFolderSetting(string path, string key, Type type, object Value)
    {
      if (string.IsNullOrEmpty(path))
      {
        return;
      }
      if (string.IsNullOrEmpty(key))
      {
        return;
      }
      if (!IsConnected())
      {
        return;
      }

      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(path);
        string KeyFiltered = key;
        DatabaseUtility.RemoveInvalidChars(ref strPathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref KeyFiltered);

        int PathId = AddPath(strPathFiltered);
        if (PathId < 0)
        {
          return;
        }

        DeleteFolderSetting(path, key);

        XmlSerializer serializer = new XmlSerializer(type);
        //serialize...
        using (MemoryStream strm = new MemoryStream())
        {
          using (TextWriter w = new StreamWriter(strm))
          {
            serializer.Serialize(w, Value);
            w.Flush();
            strm.Seek(0, SeekOrigin.Begin);

            using (TextReader reader = new StreamReader(strm))
            {
              string ValueText = reader.ReadToEnd();
              string ValueTextFiltered = ValueText;
              DatabaseUtility.RemoveInvalidChars(ref ValueTextFiltered);

              tblsetting obj = new tblsetting()
              {
                idPath = PathId,
                tagName = KeyFiltered,
                tagValue = ValueTextFiltered
              };

              _connection.tblsettings.AddObject(obj);
              _connection.SaveChanges();
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("FolderSettingADO:AddFolderSetting exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void GetPath(string strPath, ref ArrayList strPathList, string strKey)
    {
      try
      {
        if (string.IsNullOrEmpty(strKey))
        {
          return;
        }
        if (string.IsNullOrEmpty(strPath))
        {
          return;
        }
        if (!IsConnected())
        {
          return;
        }

        strPath = strPath.Replace("\\", "\\\\").Trim();
        strPath = strPath.Replace("\\", "\\\\").Trim();
        strPath = strPath.Replace("'", "\\'").Trim();

        string strSQL = string.Format(
          "SELECT * from tblPath where strPath like '{0}%' and idPath in (SELECT idPath from tblSetting where tblSetting.idPath = tblPath.idPath and tblSetting.tagName = '{1}')"
          , strPath, strKey);

        var query = _connection.ExecuteStoreQuery<Databases.tblpath>(strSQL).ToList();

        if (query.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < query.Count; iRow++)
        {
          strPathList.Add(query[iRow].strPath);
        }
      }
      catch (Exception ex)
      {
        Log.Error("FolderSettingADO.GetPath() exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void GetFolderSetting(string path, string key, Type type, out object valueObject)
    {
      valueObject = null;

      if (string.IsNullOrEmpty(path))
      {
        return;
      }
      if (string.IsNullOrEmpty(key))
      {
        return;
      }
      if (!IsConnected())
      {
        return;
      }

      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(path);
        string KeyFiltered = key;
        DatabaseUtility.RemoveInvalidChars(ref strPathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref KeyFiltered);

        int PathId = AddPath(strPathFiltered);
        if (PathId < 0)
        {
          return;
        }

        var query = (from sql in _connection.tblsettings
                     where sql.idPath == PathId && sql.tagName == KeyFiltered
                     select sql).FirstOrDefault();

        if (query == null)
        {
          int pos = strPathFiltered.LastIndexOf(@"\");
          if ((strPathFiltered.Substring(1, 1) == ":" && pos > 1) || (strPathFiltered.Substring(0, 1) == "\\" && pos > 5))
          {
            string folderName;
            folderName = strPathFiltered.Substring(0, pos);

            Log.Debug("FolderSettingADO:GetFolderSetting: {1} not found, trying the parent {0}", folderName, strPathFiltered);
            GetFolderSetting(folderName, key, type, out valueObject);
            return;
          }
          if (strPathFiltered != "root")
          {
            Log.Debug("FolderSettingADO:GetFolderSetting: {0} parent not found. Trying the root.", strPathFiltered);
            GetFolderSetting("root", key, type, out valueObject);
            return;
          }
          Log.Debug("FolderSettingADO:GetFolderSetting: {0} parent not found. Will use the default share settings.", strPathFiltered);
          return;
        }
        string strValue = query.tagValue;

        Log.Debug("FolderSettingADO:GetFolderSetting: {0} found.", strPathFiltered);
        //deserialize...

        using (MemoryStream strm = new MemoryStream())
        {
          using (StreamWriter writer = new StreamWriter(strm))
          {
            writer.Write(strValue);
            writer.Flush();
            strm.Seek(0, SeekOrigin.Begin);
            using (TextReader r = new StreamReader(strm))
            {
              try
              {
                XmlSerializer serializer = new XmlSerializer(type);
                valueObject = serializer.Deserialize(r);
              }
              catch (Exception ex) 
              {
                Log.Error("FolderSettingADO:GetFolderSetting exception err:{0} stack:{1}", ex.Message, ex.StackTrace); 
              }
            }
          }
        }

      }
      catch (Exception ex)
      {
        Log.Error("FolderSetting:GetFolderSetting exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public string DatabaseName
    {
      get 
      { 
        if (_connection != null)
        {
          return "FolderSettings";
        }
        return "";
      }
    }

    public bool DbHealth
    {
      get
      {
        return IsConnected();
      }
    }
  }
}