using System;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using MediaPortal.GUI.Library;
namespace SQLite.NET
{
	/// <summary>
	/// 
	/// </summary>
	///


	public class SQLiteClient : IDisposable
	{
		[DllImport("sqlite.dll")]
		private static extern int sqlite3_changes(IntPtr handle);
		[DllImport("sqlite.dll")]
		private static extern void sqlite3_close(IntPtr handle);
		[DllImport("sqlite.dll")]
		private static extern void sqlite3_interrupt(IntPtr handle);
		[DllImport("sqlite.dll")]
		private static extern int sqlite3_last_insert_rowid(IntPtr handle);
		[DllImport("sqlite.dll")]
		private static extern string sqlite3_libencoding();
		[DllImport("sqlite.dll")]
		private static extern string sqlite3_libversion();
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode sqlite3_open(string filename, ref IntPtr handle);

		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_prepare(IntPtr handle, string sql, int nbytes, ref IntPtr stmt, ref IntPtr tail);
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_prepare16(IntPtr handle,  [MarshalAs(UnmanagedType.LPWStr)]string sql, int nbytes, ref IntPtr stmt, ref IntPtr tail);
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_finalize(IntPtr stmt);
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_reset(IntPtr stmt);		
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_step(IntPtr stmt);		
		[DllImport("sqlite.dll")]
		private static extern char* sqlite3_column_name16(IntPtr stmt, int nCol);		
		[DllImport("sqlite.dll")]
		private static extern sbyte* sqlite3_column_name(IntPtr stmt, int nCol);		
		[DllImport("sqlite.dll")]
		private static extern int sqlite3_column_count(IntPtr stmt);
		[DllImport("sqlite.dll")]
		private static extern char* sqlite3_column_text16(IntPtr stmt, int nCol);		
		[DllImport("sqlite.dll")]
		private static extern char* sqlite3_errmsg16(IntPtr handle);

		// Fields
		private int busyRetries=5;
		private int busyRetryDelay=25;
		IntPtr dbHandle=IntPtr.Zero;
		string databaseName=String.Empty;
		//private long dbHandleAdres=0;
		// Nested Types
		public enum ResultCode
		{
			// Fields
			ABORT = 4,
			AUTH = 0x17,
			BUSY = 5,
			CANTOPEN = 14,
			CONSTRAINT = 0x13,
			CORRUPT = 11,
			EMPTY = 0x10,
			ERROR = 1,
			FULL = 13,
			INTERNAL = 2,
			INTERRUPT = 9,
			IOERR = 10,
			LOCKED = 6,
			MISMATCH = 20,
			MISUSE = 0x15,
			NOLFS = 0x16,
			NOMEM = 7,
			NOTFOUND = 12,
			OK = 0,
			PERM = 3,
			PROTOCOL = 15,
			READONLY = 8,
			SCHEMA = 0x11,
			TOOBIG = 0x12,
			Row=100,
			Done=101
		}

		// Methods
		public SQLiteClient(string dbName)
		{
			databaseName=System.IO.Path.GetFileName(dbName);
			//Log.Write("dbs:open:{0}",databaseName);
			dbHandle=IntPtr.Zero;
			
			SQLiteClient.ResultCode err=SQLiteClient.sqlite3_open(dbName, ref dbHandle);
			//Log.Write("dbs:opened:{0} {1} {2:X}",databaseName, err.ToString(),dbHandle.ToInt32());
			if (err!=ResultCode.OK)
			{
				throw new SQLiteException(string.Format("Failed to open database, SQLite said: {0} {1}", dbName,err.ToString() ));
			}
			//Log.Write("dbs:opened:{0} {1:X}",databaseName, dbHandle.ToInt32());
		}
 

		public int ChangedRows()
		{
			if (this.dbHandle==IntPtr.Zero) return 0;
			return SQLiteClient.sqlite3_changes(this.dbHandle);
		}
 

		public void Close()
		{
			if (this.dbHandle!=IntPtr.Zero)
			{	
			//Log.Write("dbs:close:{0}",databaseName);
				SQLiteClient.sqlite3_close(this.dbHandle);
				this.dbHandle=IntPtr.Zero;
				databaseName=String.Empty;
			}
		}
 
		void ThrowError(string statement, string sqlQuery,ResultCode err)
		{
			string errorMsg =String.Empty;
			unsafe
			{
				char* pErr= sqlite3_errmsg16(this.dbHandle);
				errorMsg = new string(pErr);
			}
			Log.WriteFile(Log.LogType.Log,true,"SQL:{0} cmd:{1} err:{2} detailed:{3} query:{4}",
											databaseName,statement,err.ToString(),errorMsg,sqlQuery);
					
			throw new SQLiteException( String.Format("SQL:{0} cmd:{1} err:{2} detailed:{3} query:{4}",databaseName,statement,err.ToString(),errorMsg,sqlQuery),err);
		}

		public SQLiteResultSet Execute(string query)
		{
				//Log.Write("dbs:{0} sql:{1}", databaseName,query);
				if (query==null) return null;
				int len=query.Length;
				if (len==0) return null;
				if (query[len-1] != '\n' && query[len-2] != ';' )
				{
					query+=";";
				}
				//if ( (long)dbHandle != dbHandleAdres)
				//	throw new SQLiteException(String.Format("SQL0: ptr changed:{0:X} {1:X}", dbHandleAdres,(long)dbHandle), ResultCode.INTERNAL);
				SQLiteClient.ResultCode err=ResultCode.EMPTY;
				SQLiteResultSet set1 = new SQLiteResultSet();
				set1.LastCommand=query;	
				IntPtr stmt=IntPtr.Zero;
				IntPtr ptrTail=IntPtr.Zero;

				for (int x=0; x < busyRetries;++x)
				{
					
					//Log.Write("dbs:{0} prepare16 :{1:X} {2}",databaseName,dbHandle.ToInt32(),query);
					err= sqlite3_prepare16(this.dbHandle, query, query.Length, ref stmt, ref ptrTail);
					if (err!=ResultCode.OK)
					{
						//Log.Write("dbs:{0} prepare16 returns:{1}",databaseName, err.ToString());
						if (stmt!=IntPtr.Zero) 
						{
							sqlite3_finalize(stmt);
							stmt=IntPtr.Zero;
						}
							
						if (err==ResultCode.EMPTY||err==ResultCode.Done)
						{
							//table is empty
							return set1;
						}

						if (err==ResultCode.BUSY)
						{
							System.Threading.Thread.Sleep(busyRetryDelay);
						}
						if (err==ResultCode.ERROR)
						{
							ThrowError("sqlite3_prepare16",query,err);
						}
						continue;
					}
					else
					{
						//Log.Write("dbs:{0} prepare16 returns:{1}",databaseName, err.ToString());
						break;
					}
				}
				if (err==ResultCode.EMPTY||err==ResultCode.Done)
				{
					//table is empty
					return set1;
				}
				if (err!=ResultCode.OK && err!=ResultCode.Done)
				{
					ThrowError("sqlite3_prepare16(2)",query,err);
				}
					
				//Log.Write("dbs:{0} sqlite3_column_count:{1:X}",databaseName, stmt.ToInt32());
				int nCol = sqlite3_column_count(stmt);
				//Log.Write("dbs:{0} sqlite3_column_count returns:{1:X} {2}",databaseName, stmt.ToInt32(), nCol);
				int row=0;
				while(true)
				{
					for (int x=0; x < busyRetries;++x)
					{
						//Log.Write("dbs:{0} sqlite3_step:{1:X}",databaseName, stmt.ToInt32());
						err= sqlite3_step(stmt);
						//Log.Write("dbs:{0} sqlite3_step returns:{1:X} {2}",databaseName, stmt.ToInt32(), err.ToString());
						if (err!=ResultCode.BUSY) break;
						System.Threading.Thread.Sleep(busyRetryDelay);
					}
					if (err==ResultCode.EMPTY)
					{
						//table is empty
						sqlite3_finalize(stmt);
						stmt=IntPtr.Zero;
						return set1;
					}
					else if (err!=ResultCode.Row)
					{
						sqlite3_finalize(stmt);
						stmt=IntPtr.Zero;
						if (err!=ResultCode.OK && err!=ResultCode.Done)
						{
							ThrowError("sqlite3_step(2)",query,err);
						}
						break;
					}
					else
					{
						if (row==0)
						{
							//Log.Write("dbs:{0} Get columnnames:{1:X}",databaseName,stmt.ToInt32());
							for (int col=0; col < nCol;col++)
							{
								string columName =String.Empty;
								unsafe
								{
									char* pColumnName= sqlite3_column_name16(stmt,col);
									columName = new string(pColumnName);
								}
								set1.ColumnNames.Add(columName);
								set1.ColumnIndices[columName]=col;
							}
						}
						//Log.Write("dbs:{0} Get row:{1:X} {2}",databaseName,stmt.ToInt32(),row);
						ArrayList rowData = new ArrayList();
						for (int col=0; col < nCol;col++)
						{
							string columValue =String.Empty;
							unsafe
							{
								char* pColumnValue= sqlite3_column_text16(stmt,col);
								columValue = new string(pColumnValue);
							}
							rowData.Add(columValue);
						}
						set1.Rows.Add(rowData);
						//Log.Write("dbs:{0} Get row:{1} done",databaseName,row);
						row++;
					}
				}

				if (stmt!=IntPtr.Zero)
				{
					//Log.Write("dbs:{0} finalize :{1:X}",databaseName,stmt);
					sqlite3_finalize(stmt);
					stmt=IntPtr.Zero;
				}
			
			  //Log.Write("dbs:{0} done:{1}",databaseName,err.ToString());
				if (err!=ResultCode.OK && err!=ResultCode.Done)
				{
					ThrowError("sqlite3_finalize(2)",query,err);
				}
				return set1;
		}
 

		~SQLiteClient()
		{
			//Log.Write("dbs:{0} ~ctor()", databaseName);
			this.Close();
		}
 

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
 

		public static string GetLibEncoding()
		{
			return SQLiteClient.sqlite3_libencoding();
		}
 

		public static string GetLibVersion()
		{
			return SQLiteClient.sqlite3_libversion();
		}
 

		public static string GetMessageForError(SQLiteClient.ResultCode errorCode)
		{
			switch (errorCode)
			{
				case SQLiteClient.ResultCode.OK:
				{
					return "Successful result";
				}
				case SQLiteClient.ResultCode.ERROR:
				{
					return "SQL error or missing database";
				}
				case SQLiteClient.ResultCode.INTERNAL:
				{
					return "An internal logic error in SQLite";
				}
				case SQLiteClient.ResultCode.PERM:
				{
					return "Access permission denied";
				}
				case SQLiteClient.ResultCode.ABORT:
				{
					return "Callback routine requested an abort";
				}
				case SQLiteClient.ResultCode.BUSY:
				{
					return "The database file is locked";
				}
				case SQLiteClient.ResultCode.LOCKED:
				{
					return "A table in the database is locked";
				}
				case SQLiteClient.ResultCode.NOMEM:
				{
					return "A malloc() failed";
				}
				case SQLiteClient.ResultCode.READONLY:
				{
					return "Attempt to write a readonly database";
				}
				case SQLiteClient.ResultCode.INTERRUPT:
				{
					return "Operation terminated by sqlite_interrupt()";
				}
				case SQLiteClient.ResultCode.IOERR:
				{
					return "Some kind of disk I/O error occurred";
				}
				case SQLiteClient.ResultCode.CORRUPT:
				{
					return "The database disk image is malformed";
				}
				case SQLiteClient.ResultCode.NOTFOUND:
				{
					return "(Internal Only) Table or record not found";
				}
				case SQLiteClient.ResultCode.FULL:
				{
					return "Insertion failed because database is full";
				}
				case SQLiteClient.ResultCode.CANTOPEN:
				{
					return "Unable to open the database file";
				}
				case SQLiteClient.ResultCode.PROTOCOL:
				{
					return "Database lock protocol error";
				}
				case SQLiteClient.ResultCode.EMPTY:
				{
					return "(Internal Only) Database table is empty";
				}
				case SQLiteClient.ResultCode.SCHEMA:
				{
					return "The database schema changed";
				}
				case SQLiteClient.ResultCode.TOOBIG:
				{
					return "Too much data for one row of a table";
				}
				case SQLiteClient.ResultCode.CONSTRAINT:
				{
					return "Abort due to contraint violation";
				}
				case SQLiteClient.ResultCode.MISMATCH:
				{
					return "Data type mismatch";
				}
				case SQLiteClient.ResultCode.MISUSE:
				{
					return "Library used incorrectly";
				}
				case SQLiteClient.ResultCode.NOLFS:
				{
					return "Uses OS features not supported on host";
				}
				case SQLiteClient.ResultCode.AUTH:
				{
					return "Authorization denied";
				}
			}
			return "";
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
 

		public void Interrupt()
		{
			SQLiteClient.sqlite3_interrupt(this.dbHandle);
		}
 

		public int LastInsertID()
		{
			return SQLiteClient.sqlite3_last_insert_rowid(this.dbHandle);
		}
 

		public static string Quote(string input)
		{
			return string.Format("'{0}'", input.Replace("'", "''"));
		}
 


		// Properties
		public int BusyRetries
		{
			get
			{
				return this.busyRetries;
			}
			set
			{
				this.busyRetries = value;
			}
		}
 

		public int BusyRetryDelay
		{
			get
			{
				return this.busyRetryDelay;
			}
			set
			{
				this.busyRetryDelay = value;
			}
		}
		#region IDisposable Members

		public void Dispose()
		{
			//Log.Write("dbs:{0} Dispose()", databaseName);
		}

		#endregion
	}
 

}
