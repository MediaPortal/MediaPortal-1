using System;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
namespace SQLite.NET
{
	/// <summary>
	/// 
	/// </summary>
	///


	public unsafe class SQLiteClient
	{
		public unsafe delegate int SQLiteCallback(void* pArg, int argc, char** argv, char** columnNames);


		[DllImport("sqlite.dll")]
		private static extern int sqlite3_changes(void* handle);
		[DllImport("sqlite.dll")]
		private static extern void sqlite3_close(void* handle);
		[DllImport("sqlite.dll")]
		private static extern ResultCode sqlite3_exec(void* handle, string sql, SQLiteCallback callBack, IntPtr pArg, out string errMsg);
		[DllImport("sqlite.dll")]
		private static extern void sqlite3_interrupt(void* handle);
		[DllImport("sqlite.dll")]
		private static extern int sqlite3_last_insert_rowid(void* handle);
		[DllImport("sqlite.dll")]
		private static extern string sqlite3_libencoding();
		[DllImport("sqlite.dll")]
		private static extern string sqlite3_libversion();
		[DllImport("sqlite.dll")]
		private static extern int sqlite3_open(string filename, void** handle);

		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_prepare(void* handle, string sql, int nbytes, void** stmt, void** tail);
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_prepare16(void* handle,  [MarshalAs(UnmanagedType.LPWStr)]string sql, int nbytes, void** stmt, void** tail);
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_finalize(void* stmt);
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_reset(void* stmt);		
		[DllImport("sqlite.dll")]
		private static extern SQLiteClient.ResultCode  sqlite3_step(void* stmt);		
		[DllImport("sqlite.dll")]
		private static extern char* sqlite3_column_name16(void* stmt, int nCol);		
		[DllImport("sqlite.dll")]
		private static extern sbyte* sqlite3_column_name(void* stmt, int nCol);		
		[DllImport("sqlite.dll")]
		private static extern int sqlite3_column_count(void* stmt);
		[DllImport("sqlite.dll")]
		private static extern char* sqlite3_column_text16(void* stmt, int nCol);		

		// Fields
		private int busyRetries;
		private int busyRetryDelay;
		private unsafe void* dbHandle;

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
			string text1;
			this.busyRetries = 5;
			this.busyRetryDelay = 5;
			
			dbHandle=null;
			
			fixed( void** ptr=&dbHandle)
			{
				int err=SQLiteClient.sqlite3_open(dbName, ptr);
				text1="";
				if (err!=0)
				{
					throw new SQLiteException(string.Format("Failed to open database, SQLite said: {0}", text1));
				}
			}
		}
 

		public int ChangedRows()
		{
			return SQLiteClient.sqlite3_changes(this.dbHandle);
		}
 

		public void Close()
		{
			if (this.dbHandle!=null)
				SQLiteClient.sqlite3_close(this.dbHandle);
			this.dbHandle=null;
		}
 

		public SQLiteResultSet Execute(string query)
		{
			SQLiteClient.ResultCode err=ResultCode.EMPTY;
			SQLiteResultSet set1 = new SQLiteResultSet();
			set1.LastCommand=query;	
			void *stmt=null;
			void** pStmnt=&stmt;
			{
				for (int x=0; x < 5;++x)
				{
					err= sqlite3_prepare16(this.dbHandle, query, query.Length, pStmnt, null);
					if (err!=ResultCode.OK)
					{
						if (pStmnt!=null) 
						{
							sqlite3_finalize(stmt);
						}
						if (err==ResultCode.ERROR)
							throw new SQLiteException(String.Format("SQL1:{0} failed err:{1}", query,err.ToString()), err);

						System.Threading.Thread.Sleep(5);
						continue;
					}
					break;
				}
				if (err!=ResultCode.OK && err!=ResultCode.Done)
					throw new SQLiteException(String.Format("SQL2:{0} failed err:{1}", query,err.ToString()), err);

				
				int nCol = sqlite3_column_count(stmt);
				int row=0;
				while(true)
				{
					err= sqlite3_step(stmt);
					if (err!=ResultCode.Row)
					{
						sqlite3_finalize(stmt);
						pStmnt=null;
						stmt=null;
						if (err!=ResultCode.OK && err!=ResultCode.Done)
							throw new SQLiteException(String.Format("SQL3:{0} failed err:{1}", query,err.ToString()), err);
						break;
					}
					else
					{
						if (row==0)
						{
							for (int col=0; col < nCol;col++)
							{
								char* pColumnName= sqlite3_column_name16(stmt,col);
								string columName = new string(pColumnName);
								set1.ColumnNames.Add(columName);
								set1.ColumnIndices[columName]=col;
							}
						}
						ArrayList rowData = new ArrayList();
						for (int col=0; col < nCol;col++)
						{
							char* pColumnValue= sqlite3_column_text16(stmt,col);
							string columValue = new string(pColumnValue);
							rowData.Add(columValue);
						}
						set1.Rows.Add(rowData);
						row++;
					}
				}
			}
			if (stmt!=null)
			{
				sqlite3_finalize(stmt);
				pStmnt=null;
			}
			if (err!=ResultCode.OK && err!=ResultCode.Done)
				throw new SQLiteException(String.Format("SQL4:{0} failed err:{1}", query,err.ToString()), err);
			return set1;
		}
 

		~SQLiteClient()
		{
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
 

	}
 

}
