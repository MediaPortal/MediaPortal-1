﻿#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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
using System.Collections;
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
    private bool _dbHealth = false;

    public FolderSettingsSqlLite()
    {
      try
      {
        // Open database
        Log.Info("Open FolderDatabase");
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "FolderDatabase3.db3"));

        _dbHealth = DatabaseUtility.IntegrityCheck(m_db);

        DatabaseUtility.SetPragmas(m_db);
        DatabaseUtility.AddTable(m_db, "tblPath", "CREATE TABLE tblPath ( idPath integer primary key, strPath text)");
        DatabaseUtility.AddTable(m_db, "tblSetting",
                                 "CREATE TABLE tblSetting ( idSetting integer primary key, idPath integer , tagName text, tagValue text)");
        // Indexes for tblPath
        DatabaseUtility.AddIndex(m_db, "idx_tblPath_strPath", "CREATE INDEX idx_tblPath_strPath ON tblPath (strPath ASC)");
        DatabaseUtility.AddIndex(m_db, "idx_tblPath_idPath_strPath", "CREATE INDEX idx_tblPath_idPath_strPath ON tblPath (idPath ASC, strPath ASC)");

        // Indexes for tblSetting
        DatabaseUtility.AddIndex(m_db, "idx_tblSetting_idPath", "CREATE INDEX idx_tblSetting_idPath ON tblSetting (idPath ASC)");
        DatabaseUtility.AddIndex(m_db, "idx_tblSetting_tagName", "CREATE INDEX idx_tblSetting_tagName ON tblSetting (tagName ASC)");
        DatabaseUtility.AddIndex(m_db, "idx_tblSetting_idPath_tagName", "CREATE INDEX idx_tblSetting_idPath_tagName ON tblSetting (idPath ASC, tagName ASC)");

        // Cleanup DB
        Log.Debug("Cleanup FolderDatabase");
        string strSQL = String.Format("delete from tblPath where idPath not in (select idPath from tblSetting)");
        m_db.Execute(strSQL);
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
        return string.Empty;
      }
      if (results.Rows.Count < iRecord)
      {
        return string.Empty;
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
      return string.Empty;
    }

    private void RemoveInvalidChars(ref string strTxt)
    {
      if (strTxt == null)
      {
        return;
      }

      string strReturn = string.Empty;
      for (int i = 0; i < (int)strTxt.Length; ++i)
      {
        char k = strTxt[i];
        if (k == '\'')
        {
          strReturn += "'";
        }
        strReturn += k;
      }
      if (strReturn == string.Empty)
      {
        strReturn = Strings.Unknown;
      }
      strTxt = strReturn.Trim();
    }

    private int AddPath(string FilteredPath)
    {
      if (string.IsNullOrEmpty(FilteredPath))
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

    public void GetPath(string strPath, ref ArrayList strPathList, string strKey)
    {
      if (string.IsNullOrEmpty(strKey))
      {
        return;
      }

      if (m_db == null)
      {
        return;
      }
      try
      {
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
        Log.Error("FolderDatabase.GetPath() exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void DeleteFolderSetting(string strPath, string Key)
    {
      DeleteFolderSetting(strPath, Key, false);
    }

    public void DeleteFolderSetting(string strPath, string Key, bool withPath)
    {
      if (string.IsNullOrEmpty(strPath))
      {
        return;
      }
      if (string.IsNullOrEmpty(Key))
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
        if (withPath)
        {
          strSQL = String.Format("delete from tblPath where idPath={0}", PathId);
          m_db.Execute(strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void AddFolderSetting(string strPath, string Key, Type type, object Value)
    {
      if (string.IsNullOrEmpty(strPath))
      {
        return;
      }
      if (string.IsNullOrEmpty(Key))
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
      if (string.IsNullOrEmpty(strPath))
      {
        return;
      }
      if (string.IsNullOrEmpty(Key))
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
          int pos = strPathFiltered.LastIndexOf(@"\");
          if ((strPathFiltered.Substring(1, 1) == ":" && pos > 1) || (strPathFiltered.Substring(0, 1) == "\\" && pos > 5))
          {
            string folderName;
            folderName = strPathFiltered.Substring(0, pos);

            Log.Debug("GetFolderSetting: {1} not found, trying the parent {0}", folderName, strPathFiltered);
            GetFolderSetting(folderName, Key, type, out Value);
            return;
          }
          if (strPathFiltered != "root")
          {
            Log.Debug("GetFolderSetting: {0} parent not found. Trying the root.", strPathFiltered);
            GetFolderSetting("root", Key, type, out Value);
            return;
          }
          Log.Debug("GetFolderSetting: {0} parent not found. Will use the default share settings.", strPathFiltered);
          return;
        }
        string strValue = Get(results, 0, "tagValue");
        Log.Debug("GetFolderSetting: {0} found.", strPathFiltered);
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
      if (string.IsNullOrEmpty(strPath))
      {
        return;
      }
      if (string.IsNullOrEmpty(Key))
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
          strSQL = String.Format("delete from tblPath where idPath={0}", PathId);
          m_db.Execute(strSQL);
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
            using (TextReader r = new StreamReader(strm))
            {
              try
              {
                XmlSerializer serializer = new XmlSerializer(type);
                Value = serializer.Deserialize(r);
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

    public bool DbHealth
    {
      get
      {
        return _dbHealth;
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