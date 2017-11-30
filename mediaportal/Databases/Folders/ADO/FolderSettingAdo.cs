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
using System.Data.SqlClient;
using System.IO;
using System.Collections;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace Databases.Folders.SqlServer
{
  public class FolderSettingAdo : IFolderSettings, IDisposable
  {
    private SqlConnection _connection;

    public FolderSettingAdo()
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

    public void Dispose()
    {
      if (_connection != null)
      {
        _connection.Close();
        _connection.Dispose();
        _connection = null;
      }
    }

    private void CreateTables()
    {
      SqlServerUtility.AddTable(_connection, "tblFolderPath",
                                "CREATE TABLE tblFolderPath ( idPath int IDENTITY(1,1) NOT NULL, strPath varchar(2048))");
      SqlServerUtility.AddPrimaryKey(_connection, "tblFolderPath", "idPath");

      SqlServerUtility.AddTable(_connection, "tblFolderSetting",
                                "CREATE TABLE tblFolderSetting ( idSetting int IDENTITY(1,1) NOT NULL, idPath int NOT NULL, tagName varchar(2048), tagValue varchar(2048))");
      SqlServerUtility.AddPrimaryKey(_connection, "tblFolderSetting", "idSetting");
    }

    private int AddPath(string filteredPath)
    {
      if (filteredPath == null)
      {
        return -1;
      }
      if (filteredPath == string.Empty)
      {
        return -1;
      }
      try
      {
        string sql = String.Format("select * from tblFolderPath where strPath like '{0}'", filteredPath);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = sql;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              int id = (int)reader["idPath"];
              reader.Close();
              return id;
            }
            else
            {
              reader.Close();
              sql = String.Format("insert into tblFolderPath ( strPath) values (  '{0}' )", filteredPath);
              return SqlServerUtility.InsertRecord(_connection, sql);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return -1;
    }

    public void DeleteFolderSetting(string strPath, string Key)
    {
      DeleteFolderSetting(strPath, Key, false);
    }

    public void DeleteFolderSetting(string path, string Key, bool withPath)
    {
      if (path == null)
      {
        return;
      }
      if (path == string.Empty)
      {
        return;
      }
      if (Key == null)
      {
        return;
      }
      if (Key == string.Empty)
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
        string strSQL = String.Format("delete from tblFolderSetting where idPath={0} and tagName ='{1}'", PathId,
                                      keyFiltered);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        if (withPath)
        {
          strSQL = String.Format("delete from tblPath where idPath={0}", PathId);
          SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void AddFolderSetting(string path, string key, Type type, object Value)
    {
      if (path == null)
      {
        return;
      }
      if (path == string.Empty)
      {
        return;
      }
      if (key == null)
      {
        return;
      }
      if (key == string.Empty)
      {
        return;
      }

      try
      {
        string pathFiltered = Utils.RemoveTrailingSlash(path);
        string keyFiltered = key;
        DatabaseUtility.RemoveInvalidChars(ref pathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref keyFiltered);

        int idPath = AddPath(pathFiltered);
        if (idPath < 0)
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
              string valueText = reader.ReadToEnd();
              string valueFiltered = valueText;
              DatabaseUtility.RemoveInvalidChars(ref valueFiltered);

              string sql =
                String.Format("insert into tblFolderSetting (idPath, tagName,tagValue) values( {0}, '{1}', '{2}') ",
                              idPath, keyFiltered, valueFiltered);
              SqlServerUtility.InsertRecord(_connection, sql);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void GetPath(string strPath, ref ArrayList strPathList, string strKey)
    {
      /*try
      {
        if (strKey == string.Empty)
        {
          return;
        }
        if (strPath == string.Empty)
        {
          return;
        }

        string sql = string.Format(
          "SELECT strPath from tblPath where strPath like '{0}%' and idPath in (SELECT idPath from tblSetting where tblSetting.idPath = tblPath.idPath and tblSetting.tagName = '{1}')"
          , strPath, strKey);

        SQLiteResultSet results = m_db.Execute(sql);

        if (results.Rows.Count == 0)
        {
          return;
        }
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          strPathList.Add(DatabaseUtility.Get(results, iRow, "strPath"));
        }
      }
      catch (Exception ex)
      {
        Log.Error("Lolderdatabase.GetPath() exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }*/
    }

    public void GetFolderSetting(string path, string key, Type type, out object valueObject)
    {
      valueObject = null;
      if (path == null)
      {
        return;
      }
      if (path == string.Empty)
      {
        return;
      }
      if (key == null)
      {
        return;
      }
      if (key == string.Empty)
      {
        return;
      }

      try
      {
        string pathFiltered = Utils.RemoveTrailingSlash(path);
        string keyFiltered = key;
        DatabaseUtility.RemoveInvalidChars(ref pathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref keyFiltered);

        int idPath = AddPath(pathFiltered);
        if (idPath < 0)
        {
          return;
        }

        string strValue = string.Empty;
        string sql = String.Format("select * from tblFolderSetting where idPath={0} and tagName like '{1}'", idPath,
                                   keyFiltered);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = sql;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (!reader.Read())
            {
              reader.Close();
              return;
            }
            strValue = reader["tagValue"].ToString();
            reader.Close();
          }
        }
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
              catch (Exception) {}
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void GetViewSetting(string strPath, string Key, Type type, out object Value)
    {
      Value = null;
      if (strPath == null)
      {
        return;
      }
      if (strPath == string.Empty)
      {
        return;
      }
      if (Key == null)
      {
        return;
      }
      if (Key == string.Empty)
      {
        return;
      }

      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(strPath);
        string KeyFiltered = Key;
        DatabaseUtility.RemoveInvalidChars(ref strPathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref KeyFiltered);

        int PathId = AddPath(strPathFiltered);
        if (PathId < 0)
        {
          return;
        }

        string strValue = string.Empty;
        string sql = String.Format("select * from tblFolderSetting where idPath={0} and tagName like '{1}'", PathId,
                                   KeyFiltered);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = sql;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (!reader.Read())
            {
              reader.Close();
              return;
            }
            strValue = reader["tagValue"].ToString();
            reader.Close();
          }
        }
        Log.Debug("GetViewSetting: {0} found.", strPathFiltered);
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
                Value = serializer.Deserialize(r);
              }
              catch (Exception) { }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public string DatabaseName
    {
      get { return _connection.ConnectionString; }
    }

    public bool DbHealth
    {
      get
      {
        return true;
      }
    }
  }
}