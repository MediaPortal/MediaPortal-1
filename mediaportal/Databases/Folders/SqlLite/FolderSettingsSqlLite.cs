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
using System.IO;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using SQLite.NET;

namespace Databases.Folders
{
  /// <summary>
  /// 
  /// </summary>
  public class FolderSettingsSqlLite : IFolderSettings, IDisposable
  {
    public SQLiteClient m_db = null;

    public FolderSettingsSqlLite()
    {
      try
      {
        // Open database
        Log.Info("open folderdatabase");
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "FolderDatabase3.db3"));

        DatabaseUtility.SetPragmas(m_db);
        DatabaseUtility.AddTable(m_db, "tblPath", "CREATE TABLE tblPath ( idPath integer primary key, strPath text)");
        DatabaseUtility.AddTable(m_db, "tblSetting",
                                 "CREATE TABLE tblSetting ( idSetting integer primary key, idPath integer , tagName text, tagValue text)");
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void Dispose()
    {
      if (m_db != null)
      {
        m_db.Close();
        m_db.Dispose();
        m_db = null;
      }
    }

    private string Get(SQLiteResultSet results, int iRecord, string strColum)
    {
      if (null == results)
      {
        return "";
      }
      if (results.Rows.Count < iRecord)
      {
        return "";
      }
      SQLiteResultSet.Row arr = results.Rows[iRecord];
      int iCol = 0;
      foreach (string columnName in results.ColumnNames)
      {
        if (strColum == columnName)
        {
          string strLine = arr.fields[iCol].Trim();
          strLine = strLine.Replace("''", "'");
          return strLine;
        }
        iCol++;
      }
      return "";
    }

    private void RemoveInvalidChars(ref string strTxt)
    {
      if (strTxt == null)
      {
        return;
      }
      string strReturn = "";
      for (int i = 0; i < (int)strTxt.Length; ++i)
      {
        char k = strTxt[i];
        if (k == '\'')
        {
          strReturn += "'";
        }
        strReturn += k;
      }
      if (strReturn == "")
      {
        strReturn = Strings.Unknown;
      }
      strTxt = strReturn.Trim();
    }

    private int AddPath(string FilteredPath)
    {
      if (FilteredPath == null)
      {
        return -1;
      }
      if (FilteredPath == string.Empty)
      {
        return -1;
      }
      if (null == m_db)
      {
        return -1;
      }
      try
      {
        SQLiteResultSet results;
        string strSQL = String.Format("select * from tblPath where strPath like '{0}'", FilteredPath);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into tblPath (idPath, strPath) values ( NULL, '{0}' )", FilteredPath);
          m_db.Execute(strSQL);
          return m_db.LastInsertID();
        }
        else
        {
          return Int32.Parse(Get(results, 0, "idPath"));
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
      if (null == m_db)
      {
        return;
      }
      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(strPath);
        string KeyFiltered = Key;
        RemoveInvalidChars(ref strPathFiltered);
        RemoveInvalidChars(ref KeyFiltered);

        int PathId = AddPath(strPathFiltered);
        if (PathId < 0)
        {
          return;
        }
        string strSQL = String.Format("delete from tblSetting where idPath={0} and tagName ='{1}'", PathId, KeyFiltered);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void AddFolderSetting(string strPath, string Key, Type type, object Value)
    {
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

      if (null == m_db)
      {
        return;
      }
      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(strPath);
        string KeyFiltered = Key;
        RemoveInvalidChars(ref strPathFiltered);
        RemoveInvalidChars(ref KeyFiltered);

        int PathId = AddPath(strPathFiltered);
        if (PathId < 0)
        {
          return;
        }

        DeleteFolderSetting(strPath, Key);


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
              RemoveInvalidChars(ref ValueTextFiltered);

              string strSQL =
                String.Format(
                  "insert into tblSetting (idSetting,idPath, tagName,tagValue) values(NULL, {0}, '{1}', '{2}') ", PathId,
                  KeyFiltered, ValueTextFiltered);
              m_db.Execute(strSQL);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void GetFolderSetting(string strPath, string Key, Type type, out object Value)
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

      if (null == m_db)
      {
        return;
      }
      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(strPath);
        string KeyFiltered = Key;
        RemoveInvalidChars(ref strPathFiltered);
        RemoveInvalidChars(ref KeyFiltered);

        int PathId = AddPath(strPathFiltered);
        if (PathId < 0)
        {
          return;
        }

        SQLiteResultSet results;
        string strSQL = String.Format("select * from tblSetting where idPath={0} and tagName like '{1}'", PathId,
                                      KeyFiltered);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return;
        }
        string strValue = Get(results, 0, "tagValue");

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
              Value = serializer.Deserialize(r);
            }
            catch (Exception) {}
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
      get
      {
        if (m_db != null)
        {
          return m_db.DatabaseName;
        }
        return "";
      }
    }
  }
}