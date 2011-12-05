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
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace SQLite.NET
{
  /// <summary>
  /// 
  /// </summary>
  ///
  public class SQLiteClient : IDisposable
  {
    #region imports

    [DllImport("shlwapi.dll")]
    private static extern bool PathIsNetworkPath(string Path);

    #endregion

    #region variables

    private static int currentWaitCount = 0;
    private string databaseName = string.Empty;
    private string DBName = string.Empty;
    private System.Data.SQLite.SQLiteConnection _connection;

    #endregion

    #region constants

    private const int MAX_WAIT_REMOTE_DB = 60; //secs.

    #endregion

    static SQLiteClient()
    {
      currentWaitCount = 0;

      Log.Info("Using System.Data.SQLite v{0} with SQLite v{1}", SystemDataSQLiteVersion(), System.Data.SQLite.SQLiteConnection.SQLiteVersion);
    }

    private static string SystemDataSQLiteVersion()
    {
      Assembly SDS = Assembly.GetAssembly(typeof(System.Data.SQLite.SQLiteConnection));
      if (null != SDS)
      {
        AssemblyName SDSName = SDS.GetName();
        if (null != SDSName)
        {
          return SDSName.Version.ToString();
        }
      }
      return "UNKNOWN";
    }

    private static bool WaitForFile(string fileName)
    {
      // while waking up from hibernation it can take a while before a network drive is accessible.
      //int count = 0;
      bool validFile = false;
      try
      {
        string file = System.IO.Path.GetFileName(fileName);

        validFile = file.Length > 0;
      }
      catch (Exception)
      {
        validFile = false;
      }

      int maxWaitCount = ((MAX_WAIT_REMOTE_DB * 1000) / 250);

      if (validFile)
      {
        while (!File.Exists(fileName) && currentWaitCount < maxWaitCount)
        {
          System.Threading.Thread.Sleep(250);
          currentWaitCount++;
          Log.Info("SQLLiteClient: waiting for remote database file {0} for {1} msec", fileName, currentWaitCount * 240);
        }
      }
      else
      {
        return true;
      }

      if (validFile && currentWaitCount < maxWaitCount)
      {
        currentWaitCount = 0;
        return true;
      }
      else
      {
        return false;
      }
    }

    private void init(string dbName)
    {
      bool isRemotePath = PathIsNetworkPath(dbName);
      if (isRemotePath)
      {
        Log.Info("SQLiteClient: Database is remote {0}", dbName);
        WaitForFile(dbName);
      }

      this.DBName = dbName;
      databaseName = Path.GetFileName(dbName);

      if (CheckConnection())
      {
        Close();
      }
      _connection = new SQLiteConnection(string.Format("Data Source={0};Pooling=true;FailIfMissing=false", dbName));
      try
      {
        _connection.Open();
      }
      catch (Exception ex)
      {
        try
        {
          _connection.Close();
        }
        catch { }
        if (_connection != null)
          _connection.Dispose();
        _connection = null;
        throw new SQLiteException(string.Format("SQLiteClient: Failed to open database {0}: {1}", dbName, ex.ToString()));
      }
    }

    public string DatabaseName
    {
      get { return this.DBName; }
    }

    public SQLiteClient(string dbName)
    {
      init(dbName);
    }

    public int ChangedRows()
    {
      if (!CheckConnection())
      {
        return 0;
      }

      return _connection.Changes;
    }

    private bool CheckConnection()
    {
      return _connection != null && _connection.State != ConnectionState.Broken && _connection.State != ConnectionState.Closed && _connection.State != ConnectionState.Connecting;
    }

    public void Close()
    {
      try
      {
        if (CheckConnection())
          _connection.Close();
      }
      catch (Exception ex)
      {
        throw new SQLiteException(string.Format("Failed to open database {0}: {1}", databaseName, ex.ToString()));
      }
      finally
      {
        if (_connection != null)
          _connection.Dispose();
        _connection = null;
        databaseName = string.Empty;
        DBName = string.Empty;
      }
    }
    
    public SQLiteResultSet Execute(string query)
    {
      SQLiteResultSet settemp = new SQLiteResultSet();

      if (!CheckConnection())
      {
        Log.Error("SQLiteClient: _connection==null");
        return settemp;
      }
      if (query == null)
      {
        Log.Error("SQLiteClient: query==null");
        return settemp;
      }
      if (query.Length == 0)
      {
        Log.Error("SQLiteClient: query==''");
        return settemp;
      }

      DataTable table = new DataTable();
      using (SQLiteCommand cmd = _connection.CreateCommand())
      {
        cmd.CommandText = query;
        using (SQLiteDataReader reader = cmd.ExecuteReader())
        {
          table.Load(reader);
          reader.Close();
        }
      }

      settemp.LastCommand = query;
      if (settemp.ColumnNames.Count == 0)
      {
        for (int j = 0; j < table.Columns.Count; j++)
        {
          string colName = table.Columns[j].ColumnName;
          settemp.columnNames.Add(colName);
          settemp.ColumnIndices[colName] = j;
        }
      }

      for (int i = 0; i < table.Rows.Count; i++)
      {
        SQLiteResultSet.Row row = new SQLiteResultSet.Row();
        for (int j = 0; j < table.Columns.Count; j++)
        {
          row.fields.Add(PrepareColumnData(table.Rows[i][j]));
        }
        settemp.Rows.Add(row);
      }

      return settemp;
    }

    private string PrepareColumnData(object data)
    {
      if (data is System.Byte[])
      {
        return BitConverter.ToString(data as System.Byte[]).Replace("-", string.Empty);
      }
      return data.ToString();
    }

    public ArrayList GetColumn(string query)
    {
      return GetColumn(query, 0);
    }

    public ArrayList GetColumn(string query, int column)
    {
      SQLiteResultSet set1 = Execute(query);
      return set1.GetColumn(column);
    }

    public int LastInsertID()
    {
      if (!CheckConnection())
      {
        return 0;
      }

      return (int)_connection.LastInsertRowId;
    }

    public static string Quote(string input)
    {
      return string.Format("'{0}'", input.Replace("'", "''"));
    }

    #region IDisposable Members

    ~SQLiteClient()
    {
      Close();
    }

    public void Dispose()
    {
      Close();
    }

    #endregion
  }
}