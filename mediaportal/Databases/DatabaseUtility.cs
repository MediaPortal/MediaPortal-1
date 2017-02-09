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
using MediaPortal.GUI.Library;
using SQLite.NET;

namespace MediaPortal.Database
{
  /// <summary>
  /// Summary description for DatabaseUtility.
  /// </summary>
  public class DatabaseUtility
  {
    private DatabaseUtility() {}

    public static void CompactDatabase(SQLiteClient m_db)
    {
      m_db.Execute("PRAGMA count_changes=0");
      m_db.Execute("vacuum");
      m_db.Execute("PRAGMA count_changes=1");
    }

    public static void SetPragmas(SQLiteClient m_db)
    {
      m_db.Execute("PRAGMA encoding = \"UTF-8\"");
      m_db.Execute("PRAGMA cache_size=4096");
      m_db.Execute("PRAGMA page_size=8192");
      m_db.Execute("PRAGMA synchronous='OFF'");
      m_db.Execute("PRAGMA count_changes=1");
      m_db.Execute("PRAGMA full_column_names=0");
      m_db.Execute("PRAGMA short_column_names=0");
      m_db.Execute("PRAGMA auto_vacuum=0");
    }

    /// <summary>
    /// Check if a table column exists
    /// </summary>
    /// <param name="table">table name</param>
    /// <param name="column">column name</param>
    /// <returns>true if table + column exists
    /// false if table does not exists or if table doesnt contain the specified column</returns>
    public static bool TableColumnExists(SQLiteClient m_db, string table, string column)
    {
      SQLiteResultSet results;
      if (m_db == null)
      {
        return false;
      }
      if (table == null)
      {
        return false;
      }
      if (table.Length == 0)
      {
        return false;
      }
      // This only works for tables that are not empty
      //results = m_db.Execute("SELECT * FROM '" + table + "'");
      //if (results != null)
      //{
      //  for (int i = 0; i < results.ColumnNames.Count; ++i)
      //  {
      //    if ((string)results.ColumnNames[i] == column)
      //    {
      //      return true;
      //    }
      //  }
      //}
      //return false;

      // We will use --> PRAGMA table_info( your_table_name )
      // PRAGMA returns one row for each column in the named table. 
      // Columns in the result set include the columnID, column name, data type, 
      // whether or not the column can be NULL, and the default value for the column.
      // More info: http://www.sqlite.org/pragma.html
      results = m_db.Execute("PRAGMA table_info('" + table + "')");
      if (results != null)
      {
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          if ((string)results.Rows[i].fields[1] == column) // fields[1] is column name
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Check if a table exists
    /// </summary>
    /// <param name="table">name of table</param>
    /// <returns>true: table exists
    /// false: table does not exist</returns>
    public static bool TableExists(SQLiteClient m_db, string table)
    {
      SQLiteResultSet results;
      if (m_db == null)
      {
        return false;
      }
      if (table == null)
      {
        return false;
      }
      if (table.Length == 0)
      {
        return false;
      }
      results = m_db.Execute("SELECT name FROM sqlite_master WHERE name like '" + table + "' and type like 'table'");
      // UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
      if (results != null)
      {
        if (results.Rows.Count == 1)
        {
          SQLiteResultSet.Row arr = results.Rows[0];
          if (arr.fields.Count == 1)
          {
            if (arr.fields[0] == table)
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Check if a view exists
    /// </summary>
    /// <param name="table">name of view</param>
    /// <returns>true: view exists
    /// false: view does not exist</returns>
    public static bool ViewExists(SQLiteClient m_db, string view)
    {
      SQLiteResultSet results;
      if (m_db == null)
      {
        return false;
      }
      if (view == null)
      {
        return false;
      }
      if (view.Length == 0)
      {
        return false;
      }
      results = m_db.Execute("SELECT name FROM sqlite_master WHERE name like '" + view + "' and type like 'view'");
      // UNION ALL SELECT name FROM sqlite_temp_master WHERE type='view' ORDER BY name");
      if (results != null)
      {
        if (results.Rows.Count == 1)
        {
          SQLiteResultSet.Row arr = results.Rows[0];
          if (arr.fields.Count == 1)
          {
            if (arr.fields[0] == view)
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    public static void AddIndex(SQLiteClient dbHandle, string indexName, string strSQL)
    {
      SQLiteResultSet results;
      results =
        dbHandle.Execute("SELECT name FROM sqlite_master WHERE name='" + indexName + "' and type='index' " +
                         "UNION " +
                         "SELECT name FROM sqlite_temp_master WHERE name ='" + indexName + "' and type='index'");
      if (results != null && results.Rows.Count == 1)
      {
        return;
      }
      try
      {
        dbHandle.Execute(strSQL);
      }
      catch (SQLiteException ex)
      {
        Log.Error("DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message, ex.StackTrace, strSQL);
      }
      return;
    }

    /// <summary>
    /// Helper function to create a new table in the database
    /// </summary>
    /// <param name="strTable">name of table</param>
    /// <param name="strSQL">SQL command to create the new table</param>
    /// <returns>true if table is created</returns>
    public static bool AddTable(SQLiteClient dbHandle, string strTable, string strSQL)
    {
      if (TableExists(dbHandle, strTable))
      {
        return false;
      }
      try
      {
        //Log.Info("create table:{0} {1}", strSQL,dbHandle);
        dbHandle.Execute(strSQL);
        //Log.Info("table created");
      }
      catch (SQLiteException ex)
      {
        Log.Error("DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message, ex.StackTrace, strSQL);
      }
      return true;
    }

    /// <summary>
    /// Helper function to create a new view in the database
    /// </summary>
    /// <param name="strView">name of view</param>
    /// <param name="strSQL">SQL command to create the new view</param>
    /// <returns>true if view is created</returns>
    public static bool AddView(SQLiteClient dbHandle, string strView, string strSQL)
    {
      if (ViewExists(dbHandle, strView))
      {
        return false;
      }
      try
      {
        //Log.Info("create view:{0} {1}", strSQL,dbHandle);
        dbHandle.Execute(strSQL);
        //Log.Info("view created");
      }
      catch (SQLiteException ex)
      {
        Log.Error("DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message, ex.StackTrace, strSQL);
      }
      return true;
    }

    public static int GetAsInt(SQLiteResultSet results, int iRecord, string strColum)
    {
      string result = Get(results, iRecord, strColum);
      if (result == null)
      {
        return 0;
      }
      if (result.Length == 0)
      {
        return 0;
      }
      int returnValue = -1;
      try
      {
        //Remove decimal from string
        try
        {
          if (result.Length > 1)
          {
            int slashPos = result.IndexOf(".", StringComparison.Ordinal);
            if (slashPos > 0)
            {
              result = result.Substring(0, result.IndexOf('.', 0));
            }
          }
        }
        catch (Exception)
        {
          // Can't convert or remove decimal from the string
        }

        int numValue;
        bool parsed = Int32.TryParse(result, out numValue);
        if (parsed)
        {
          returnValue = Int32.Parse(result);
        }
      }
      catch (Exception)
      {
        Log.Info("DatabaseUtility:GetAsInt() column:{0} record:{1} value:{2} is not an int",
                 strColum, iRecord, result);
      }
      return returnValue;
    }


    public static int GetAsInt(SQLiteResultSet results, int iRecord, int column)
    {
      string result = Get(results, iRecord, column);
      try
      {
        int intValue = Int32.Parse(result);
        return intValue;
      }
      catch (Exception) {}
      return 0;
    }

    public static long GetAsInt64(SQLiteResultSet results, int iRecord, int column)
    {
      string result = Get(results, iRecord, column);
      try
      {
        long longValue = Int64.Parse(result);
        return longValue;
      }
      catch (Exception) {}
      return 0;
    }

    public static long GetAsInt64(SQLiteResultSet results, int iRecord, string strColum)
    {
      string result = Get(results, iRecord, strColum);
      if (result == null)
      {
        return 0;
      }
      if (result.Length == 0)
      {
        return 0;
      }
      long returnValue = -1;
      try
      {
        returnValue = Int64.Parse(result);
      }
      catch (Exception)
      {
        Log.Info("DatabaseUtility:GetAsInt64() column:{0} record:{1} value:{2} is not an Int64",
                 strColum, iRecord, result);
      }
      return returnValue;
    }

    public static DateTime GetAsDateTime(SQLiteResultSet results, int iRecord, string aTimestampColum)
    {
      DateTime finalResult = DateTime.MinValue;
      if (results == null || string.IsNullOrEmpty(aTimestampColum) || results.Rows.Count < 1 ||
          results.Rows.Count < iRecord)
      {
        return finalResult;
      }

      try
      {
        SQLiteResultSet.Row arr = results.Rows[iRecord];
        int iCol = 0;
        if (results.ColumnIndices.ContainsKey(aTimestampColum))
        {
          iCol = (int)results.ColumnIndices[aTimestampColum];
          if (arr.fields[iCol] != null)
          {
            finalResult = Convert.ToDateTime((arr.fields[iCol]));
          }
        }
      }
      catch (Exception) {}

      return finalResult;
    }


    public static string Get(SQLiteResultSet results, int iRecord, int column)
    {
      if (null == results)
      {
        return string.Empty;
      }
      if (results.Rows.Count < iRecord)
      {
        return string.Empty;
      }
      if (column < 0 || column >= results.ColumnNames.Count)
      {
        return string.Empty;
      }
      SQLiteResultSet.Row arr = results.Rows[iRecord];
      if (arr.fields[column] == null)
      {
        return string.Empty;
      }
      string strLine = (arr.fields[column]).Trim();
      //strLine = strLine.Replace("''","'");
      return strLine;
      ;
    }

    public static string Get(SQLiteResultSet results, int iRecord, string strColum)
    {
      if (null == results)
      {
        return string.Empty;
      }
      if (results.Rows.Count == 0)
      {
        return string.Empty;
      }
      if (results.Rows.Count < iRecord)
      {
        return string.Empty;
      }
      SQLiteResultSet.Row arr = results.Rows[iRecord];
      int iCol = 0;
      if (results.ColumnIndices.ContainsKey(strColum))
      {
        iCol = (int)results.ColumnIndices[strColum];
        if (arr.fields[iCol] == null)
        {
          return string.Empty;
        }
        string strLine = (arr.fields[iCol]).Trim();
        //strLine = strLine.Replace("''","'");
        return strLine;
      }
      int pos = strColum.IndexOf(".", StringComparison.Ordinal);
      if (pos < 0)
      {
        return string.Empty;
      }
      strColum = strColum.Substring(pos + 1);
      if (results.ColumnIndices.ContainsKey(strColum))
      {
        iCol = (int)results.ColumnIndices[strColum];
        if (arr.fields[iCol] == null)
        {
          return string.Empty;
        }
        string strLine = (arr.fields[iCol]).Trim();
        //strLine = strLine.Replace("''","'");
        return strLine;
      }

      return string.Empty;
    }

    /// <summary>
    /// This will remove all chars which are not allowed for the DB and quote / escape when needed
    /// </summary>
    /// <param name="aStringToClean">The value to be stored</param>
    /// <returns>The string to put into an SQL statement</returns>
    public static string RemoveInvalidChars(string aStringToClean)
    {
      string result = aStringToClean;
      RemoveInvalidChars(ref result);
      return result;
    }

    /// <summary>
    /// This will remove all chars which are not allowed for the DB and quote / escape when needed
    /// </summary>
    /// <param name="strTxt">The value to be stored</param>
    public static void RemoveInvalidChars(ref string strTxt)
    {
      strTxt = FilterText(strTxt);
    }

    private static string FilterText(string strTxt)
    {
      if (string.IsNullOrEmpty(strTxt))
      {
        return Strings.Unknown;
      }
      strTxt = strTxt.Replace("'", "''").Trim();

      return strTxt;
    }

    public static bool IntegrityCheck(SQLiteClient m_db)
    {
      SQLiteResultSet results;
      if (m_db == null)
      {
        return false;
      }

      results = m_db.Execute("PRAGMA integrity_check;");
      if (results != null)
      {
        if (results.Rows.Count == 1)
        {
          SQLiteResultSet.Row arr = results.Rows[0];
          if (arr.fields.Count == 1)
          {
            if (arr.fields[0] == "ok")
            {
              Log.Debug("IntegrityCheck: the {0} is OK", m_db.DatabaseName);
              return true;
            }
          }
        }
      }
      Log.Error("IntegrityCheck: the {0} is corrupt.", m_db.DatabaseName);
      return false;
    }

    public static void Split(string strFileNameAndPath, out string strPath, out string strFileName)
    {
      strFileNameAndPath = strFileNameAndPath.Trim();
      strFileName = "";
      strPath = "";
      if (strFileNameAndPath.Length == 0)
      {
        return;
      }
      int i = strFileNameAndPath.Length - 1;
      while (i > 0)
      {
        char ch = strFileNameAndPath[i];
        if (ch == ':' || ch == '/' || ch == '\\')
        {
          break;
        }
        else
        {
          i--;
        }
      }
      strPath = strFileNameAndPath.Substring(0, i).Trim();
      strFileName = strFileNameAndPath.Substring(i, strFileNameAndPath.Length - i).Trim();
    }
  }
}