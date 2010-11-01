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
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.Database;
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

    [DllImport("sqlite.dll")]
    internal static extern int sqlite3_open16([MarshalAs(UnmanagedType.LPWStr)] string dbname, out IntPtr handle);

    [DllImport("sqlite.dll")]
    internal static extern void sqlite3_close(IntPtr sqlite_handle);

    [DllImport("sqlite.dll")]
    internal static extern IntPtr sqlite3_errmsg16(IntPtr sqlite_handle);

    [DllImport("sqlite.dll")]
    internal static extern int sqlite3_changes(IntPtr handle);

    [DllImport("sqlite.dll")]
    internal static extern int sqlite3_last_insert_rowid(IntPtr sqlite_handle);

    [DllImport("sqlite.dll")]
    internal static extern SqliteError sqlite3_prepare16(IntPtr sqlite_handle,
                                                         [MarshalAs(UnmanagedType.LPWStr)] string zSql, int zSqllen,
                                                         out IntPtr pVm, out IntPtr pzTail);

    [DllImport("sqlite.dll")]
    internal static extern SqliteError sqlite3_step(IntPtr pVm);

    [DllImport("sqlite.dll")]
    internal static extern SqliteError sqlite3_finalize(IntPtr pVm, out IntPtr pzErrMsg);

    [DllImport("sqlite.dll")]
    internal static extern SqliteError sqlite3_exec16(IntPtr handle, string sql, IntPtr callback, IntPtr user_data,
                                                      out IntPtr errstr_ptr);

    [DllImport("sqlite.dll")]
    internal static extern IntPtr sqlite3_column_name16(IntPtr pVm, int col);

    [DllImport("sqlite.dll")]
    internal static extern IntPtr sqlite3_column_text16(IntPtr pVm, int col);

    [DllImport("sqlite.dll")]
    internal static extern IntPtr sqlite3_column_blob(IntPtr pVm, int col);

    [DllImport("sqlite.dll")]
    internal static extern int sqlite3_column_bytes(IntPtr pVm, int col);

    [DllImport("sqlite.dll")]
    internal static extern int sqlite3_column_count(IntPtr pVm);

    [DllImport("sqlite.dll")]
    internal static extern int sqlite3_column_type(IntPtr pVm, int col);

    [DllImport("sqlite.dll")]
    internal static extern Int64 sqlite3_column_int64(IntPtr pVm, int col);

    [DllImport("sqlite.dll")]
    internal static extern double sqlite3_column_double(IntPtr pVm, int col);

    [DllImport("sqlite.dll")]
    internal static extern IntPtr sqlite3_libversion();

    #endregion

    #region variables

    // Fields
    private int busyRetries = 5;
    private int busyRetryDelay = 25;
    private static int currentWaitCount = 0;
    private IntPtr dbHandle = IntPtr.Zero;
    private string databaseName = string.Empty;
    private string DBName = string.Empty;
    //private long dbHandleAdres=0;

    #endregion

    #region constants

    private const int MAX_WAIT_REMOTE_DB = 60; //secs.

    #endregion

    #region enums

    // Nested Types
    public enum SqliteError : int
    {
      /// <value>Successful result</value>
      OK = 0,
      /// <value>SQL error or missing database</value>
      ERROR = 1,
      /// <value>An internal logic error in SQLite</value>
      INTERNAL = 2,
      /// <value>Access permission denied</value>
      PERM = 3,
      /// <value>Callback routine requested an abort</value>
      ABORT = 4,
      /// <value>The database file is locked</value>
      BUSY = 5,
      /// <value>A table in the database is locked</value>
      LOCKED = 6,
      /// <value>A malloc() failed</value>
      NOMEM = 7,
      /// <value>Attempt to write a readonly database</value>
      READONLY = 8,
      /// <value>Operation terminated by public const int interrupt()</value>
      INTERRUPT = 9,
      /// <value>Some kind of disk I/O error occurred</value>
      IOERR = 10,
      /// <value>The database disk image is malformed</value>
      CORRUPT = 11,
      /// <value>(Internal Only) Table or record not found</value>
      NOTFOUND = 12,
      /// <value>Insertion failed because database is full</value>
      FULL = 13,
      /// <value>Unable to open the database file</value>
      CANTOPEN = 14,
      /// <value>Database lock protocol error</value>
      PROTOCOL = 15,
      /// <value>(Internal Only) Database table is empty</value>
      EMPTY = 16,
      /// <value>The database schema changed</value>
      SCHEMA = 17,
      /// <value>Too much data for one row of a table</value>
      TOOBIG = 18,
      /// <value>Abort due to contraint violation</value>
      CONSTRAINT = 19,
      /// <value>Data type mismatch</value>
      MISMATCH = 20,
      /// <value>Library used incorrectly</value>
      MISUSE = 21,
      /// <value>Uses OS features not supported on host</value>
      NOLFS = 22,
      /// <value>Authorization denied</value>
      AUTH = 23,
      /// <value>Auxiliary database format error</value>
      FORMAT = 24,
      /// <value>2nd parameter to sqlite_bind out of range</value>
      RANGE = 25,
      /// <value>File opened that is not a database file</value>
      NOTADB = 26,
      /// <value>sqlite_step() has another row ready</value>
      ROW = 100,
      /// <value>sqlite_step() has finished executing</value>
      DONE = 101
    }

    #endregion

    static SQLiteClient()
    {
      string libVersion;
      currentWaitCount = 0;
      IntPtr pName = sqlite3_libversion();
      if (pName != IntPtr.Zero)
      {
        libVersion = Marshal.PtrToStringAnsi(pName);
        Log.Info("using sqlite {0}", libVersion);
      }
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

    // Methods

    private void init(string dbName)
    {
      bool isRemotePath = PathIsNetworkPath(dbName);
      if (isRemotePath)
      {
        Log.Info("SQLLiteClient: database is remote {0}", this.DBName);
        WaitForFile(dbName);
      }

      this.DBName = dbName;
      databaseName = Path.GetFileName(dbName);
      //Log.Info("dbs:open:{0}",databaseName);
      dbHandle = IntPtr.Zero;

      SqliteError err = (SqliteError)sqlite3_open16(dbName, out dbHandle);
      //Log.Info("dbs:opened:{0} {1} {2:X}",databaseName, err.ToString(),dbHandle.ToInt32());
      if (err != SqliteError.OK)
      {
        throw new SQLiteException(string.Format("Failed to open database, SQLite said: {0} {1}", dbName, err.ToString()));
      }
      //Log.Info("dbs:opened:{0} {1:X}",databaseName, dbHandle.ToInt32());
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
      if (dbHandle == IntPtr.Zero)
      {
        return 0;
      }
      return sqlite3_changes(dbHandle);
    }

    public void Close()
    {
      if (dbHandle != IntPtr.Zero)
      {
        //System.Diagnostics.Debugger.Launch();
        //Log.Info("SQLiteClient: Closing database: {0} st {1}", databaseName, Environment.StackTrace);
        Log.Info("SQLiteClient: Closing database: {0}", databaseName);
        try
        {
          sqlite3_close(dbHandle);
        }
        catch (Exception e)
        {
          Log.Error("SQLiteClient: Trouble closing database: {0} ({1})", databaseName, e.Message);
        }
        finally
        {
          dbHandle = IntPtr.Zero;
          databaseName = string.Empty;
        }
      }
    }

    private void ThrowError(string statement, string sqlQuery, SqliteError err)
    {
      string errorMsg = Marshal.PtrToStringUni(sqlite3_errmsg16(dbHandle));
      Log.Error("SQLiteClient: {0} cmd:{1} err:{2} detailed:{3} query:{4}",
                databaseName, statement, err.ToString(), errorMsg, sqlQuery);

      throw new SQLiteException(
        String.Format("SQLiteClient: {0} cmd:{1} err:{2} detailed:{3} query:{4}", databaseName, statement,
                      err.ToString(),
                      errorMsg, sqlQuery), err);
    }

    public SQLiteResultSet Execute(string query)
    {
      SQLiteResultSet set1 = new SQLiteResultSet();
      lock (typeof (SQLiteClient))
      {
        //Log.Info("dbs:{0} sql:{1}", databaseName,query);
        if (query == null)
        {
          Log.Error("SQLiteClient: query==null");
          return set1;
        }
        if (query.Length == 0)
        {
          Log.Error("SQLiteClient: query==''");
          return set1;
        }
        IntPtr errMsg;
        //string msg = "";

        SqliteError err;
        set1.LastCommand = query;

        try
        {
          IntPtr pVm;
          IntPtr pzTail;
          err = sqlite3_prepare16(dbHandle, query, query.Length * 2, out pVm, out pzTail);
          if (err == SqliteError.OK)
          {
            ReadpVm(query, set1, ref pVm);
          }

          if (pVm == IntPtr.Zero)
          {
            ThrowError("sqlite3_prepare16:pvm=null", query, err);
          }
          err = sqlite3_finalize(pVm, out errMsg);
        }
        finally {}
        if (err != SqliteError.OK)
        {
          Log.Error("SQLiteClient: query returned {0} {1}", err.ToString(), query);
          ThrowError("sqlite3_finalize", query, err);
        }
      }
      return set1;
    }

    internal void ReadpVm(string query, SQLiteResultSet set1, ref IntPtr pVm)
    {
      int pN;
      SqliteError res = SqliteError.ERROR;

      if (pVm == IntPtr.Zero)
      {
        ThrowError("SQLiteClient: pvm=null", query, res);
      }
      DateTime now = DateTime.Now;
      TimeSpan ts = now - DateTime.Now;
      while (true && ts.TotalSeconds > -15)
      {
        res = sqlite3_step(pVm);
        pN = sqlite3_column_count(pVm);
        /*
        if (res == SqliteError.ERROR)
        {
          ThrowError("sqlite3_step", query, res);
        }
        */
        if (res == SqliteError.DONE)
        {
          break;
        }


        // when resuming from hibernation or standby and where the db3 files are located on a network drive, we often end up in a neverending loop
        // while (true)...it never exits. and the app is hanging.
        // Lets handle it by disconnecting the DB, and then reconnect.
        if (res == SqliteError.BUSY || res == SqliteError.ERROR)
        {
          this.Close();

          dbHandle = IntPtr.Zero;

          // bool res2 = WaitForFile(this.DBName);

          SqliteError err = (SqliteError)sqlite3_open16(this.DBName, out dbHandle);

          if (err != SqliteError.OK)
          {
            throw new SQLiteException(string.Format("Failed to re-open database, SQLite said: {0} {1}", DBName,
                                                    err.ToString()));
          }
          else
          {
            IntPtr pzTail;
            err = sqlite3_prepare16(dbHandle, query, query.Length * 2, out pVm, out pzTail);

            res = sqlite3_step(pVm);
            pN = sqlite3_column_count(pVm);

            if (pVm == IntPtr.Zero)
            {
              ThrowError("sqlite3_prepare16:pvm=null", query, err);
            }
          }
        }

        // We have some data; lets read it
        if (set1.ColumnNames.Count == 0)
        {
          for (int i = 0; i < pN; i++)
          {
            string colName;
            IntPtr pName = sqlite3_column_name16(pVm, i);
            if (pName == IntPtr.Zero)
            {
              ThrowError(String.Format("SqlClient:sqlite3_column_name16() returned null {0}/{1}", i, pN), query, res);
            }
            colName = Marshal.PtrToStringUni(pName);
            set1.columnNames.Add(colName);
            set1.ColumnIndices[colName] = i;
          }
        }

        SQLiteResultSet.Row row = new SQLiteResultSet.Row();
        for (int i = 0; i < pN; i++)
        {
          string colData = "";
          IntPtr pName = sqlite3_column_text16(pVm, i);
          if (pName != IntPtr.Zero)
          {
            colData = Marshal.PtrToStringUni(pName);
          }
          row.fields.Add(colData);
        }
        set1.Rows.Add(row);

        ts = now - DateTime.Now;
      }

      if (res == SqliteError.BUSY || res == SqliteError.ERROR)
      {
        ThrowError("sqlite3_step", query, res);
      }
    }

    ~SQLiteClient()
    {
      //Log.Info("dbs:{0} ~ctor()", databaseName);
      Close();
    }

    /*

		public ArrayList GetAll(string query)
		{
			SQLiteResultSet set1 = this.Execute(query);
			return set1.Rows;
		}
 

		public ArrayList GetAllHash(string query)
		{
			SQLiteResultSet set1 = this.Execute(query);
			ArrayList list1 = new ArrayList();
			while (set1.IsMoreData)
			{
				list1.Add(set1.GetRowHash());
			}
			return list1;
		}
 

		public ArrayList GetColumn(string query)
		{
			return this.GetColumn(query, 0);
		}
 

		public ArrayList GetColumn(string query, int column)
		{
			SQLiteResultSet set1 = this.Execute(query);
			return set1.GetColumn(column);
		}
 



		public string GetOne(string query)
		{
			SQLiteResultSet set1 = this.Execute(query);
			return set1.GetField(0, 0);
		}
 

		public ArrayList GetRow(string query)
		{
			return this.GetRow(query, 0);
		}
 

		public ArrayList GetRow(string query, int row)
		{
			SQLiteResultSet set1 = this.Execute(query);
			return set1.GetRow(row);
		}
 

		public Hashtable GetRowHash(string query)
		{
			return this.GetRowHash(query, 0);
		}
 

		public Hashtable GetRowHash(string query, int row)
		{
			SQLiteResultSet set1 = this.Execute(query);
			return set1.GetRowHash(row);
		}
 
    */

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
      return sqlite3_last_insert_rowid(dbHandle);
    }


    public static string Quote(string input)
    {
      return string.Format("'{0}'", input.Replace("'", "''"));
    }


    // Properties
    public int BusyRetries
    {
      get { return busyRetries; }
      set { busyRetries = value; }
    }


    public int BusyRetryDelay
    {
      get { return busyRetryDelay; }
      set { busyRetryDelay = value; }
    }

    #region IDisposable Members

    public void Dispose()
    {
      //Log.Info("dbs:{0} Dispose()", databaseName);
      Close();
    }

    #endregion
  }
}