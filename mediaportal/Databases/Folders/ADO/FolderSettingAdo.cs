using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Serialization;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Database;

namespace Databases.Folders.SqlServer
{
  public class FolderSettingAdo: IFolderSettings
  {
    SqlConnection _connection;

    public FolderSettingAdo()
    {
      string connectionString;
      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings("mediaportal.xml"))
      {
        connectionString=reader.GetValueAsString("database","connectionstring",SqlServerUtility.DefaultConnectionString);
      }
      _connection = new SqlConnection(connectionString);
      _connection.Open();
      CreateTables();
    }

    void CreateTables()
    {
      SqlServerUtility.AddTable(_connection, "tblPath", "CREATE TABLE tblPath ( idPath int IDENTITY(1,1) NOT NULL, strPath varchar(2048))");
      SqlServerUtility.AddPrimaryKey(_connection, "tblPath", "idPath");

      SqlServerUtility.AddTable(_connection, "tblSetting", "CREATE TABLE tblSetting ( idSetting int IDENTITY(1,1) NOT NULL, idPath int NOT NULL, tagName varchar(2048), tagValue varchar(2048))");
      SqlServerUtility.AddPrimaryKey(_connection, "tblSetting", "idSetting");
    }

    int AddPath(string filteredPath)
    {
      if (filteredPath == null) return -1;
      if (filteredPath == String.Empty) return -1;
      try
      {
        string sql = String.Format("select * from tblPath where strPath like '{0}'", filteredPath);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = sql;
          SqlDataReader reader=cmd.ExecuteReader();
          if (reader.Read())
          {
            int id = (int)reader["idPath"];
            reader.Close();
            return id;
          }
          else
          {
            reader.Close();
            sql = String.Format("insert into tblPath ( strPath) values (  '{0}' )", filteredPath);
            return SqlServerUtility.InsertRecord(_connection, sql);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return -1;
    }

    public void DeleteFolderSetting(string path, string Key)
    {
      if (path == null) return;
      if (path == String.Empty) return;
      if (Key == null) return;
      if (Key == String.Empty) return;
      try
      {
        string pathFiltered = Utils.RemoveTrailingSlash(path);
        string keyFiltered = Key;
        DatabaseUtility.RemoveInvalidChars(ref pathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref keyFiltered);

        int PathId = AddPath(pathFiltered);
        if (PathId < 0) return;
        string strSQL = String.Format("delete from tblSetting where idPath={0} and tagName ='{1}'", PathId, keyFiltered);
        SqlServerUtility.ExecuteNonQuery(_connection,strSQL);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public void AddFolderSetting(string path, string key, Type type, object Value)
    {
      if (path == null) return;
      if (path == String.Empty) return;
      if (key == null) return;
      if (key == String.Empty) return;

      try
      {
        string pathFiltered = Utils.RemoveTrailingSlash(path);
        string keyFiltered = key;
        DatabaseUtility.RemoveInvalidChars(ref pathFiltered);
        DatabaseUtility.RemoveInvalidChars(ref keyFiltered);

        int idPath = AddPath(pathFiltered);
        if (idPath < 0) return;

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

              string sql = String.Format("insert into tblSetting (idPath, tagName,tagValue) values( {0}, '{1}', '{2}') ", idPath, keyFiltered, valueFiltered);
              SqlServerUtility.InsertRecord(_connection,sql);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);

      }
    }

    public void GetFolderSetting(string path, string key, Type type, out object valueObject)
    {
      valueObject=null;
			if (path==null) return;
			if (path==String.Empty) return;
			if (key==null) return;
			if (key==String.Empty) return;

      try
      {
        string pathFiltered = Utils.RemoveTrailingSlash(path);
        string keyFiltered=key;
        DatabaseUtility.RemoveInvalidChars(ref pathFiltered );
        DatabaseUtility.RemoveInvalidChars(ref keyFiltered);

        int idPath=AddPath(pathFiltered);
        if (idPath<0) return ;
        
        string strValue=String.Empty;
        string sql = String.Format("select * from tblSetting where idPath={0} and tagName like '{1}'", idPath, keyFiltered);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType=CommandType.Text;
          cmd.CommandText=sql;
          SqlDataReader reader = cmd.ExecuteReader();
          if (!reader.Read())
          {
            reader.Close();
            return;
          }
          strValue = reader["tagValue"].ToString();
          reader.Close();
        }
        //deserialize...

        using (MemoryStream strm = new MemoryStream())
        {
          using (StreamWriter writer = new StreamWriter(strm))
          {
            writer.Write(strValue);
            writer.Flush();
            strm.Seek(0,SeekOrigin.Begin);
            TextReader r = new StreamReader( strm );
            try
            {
              XmlSerializer serializer= new XmlSerializer( type );
              valueObject=serializer.Deserialize(r);
            }
            catch(Exception )
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
  }
}
