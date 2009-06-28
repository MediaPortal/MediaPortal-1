#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
              int id = (int) reader["idPath"];
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

    public void DeleteFolderSetting(string path, string Key)
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
            TextReader r = new StreamReader(strm);
            try
            {
              XmlSerializer serializer = new XmlSerializer(type);
              valueObject = serializer.Deserialize(r);
            }
            catch (Exception)
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }
  }
}