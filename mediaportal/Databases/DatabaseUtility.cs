using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;


namespace MediaPortal.Database
{
	/// <summary>
	/// Summary description for DatabaseUtility.
	/// </summary>
	public class DatabaseUtility
	{
    

		/// <summary>
		/// Check if a table column exists
		/// </summary>
		/// <param name="table">table name</param>
		/// <param name="column">column name</param>
		/// <returns>true if table + column exists
		/// false if table does not exists or if table doesnt contain the specified column</returns>
		static public bool TableColumnExists(SQLiteClient m_db, string table, string column)
		{
			SQLiteResultSet results;
			if (m_db==null) return false;
			if (table==null) return false;
			if (table.Length==0) return false;
			results = m_db.Execute("SELECT * FROM '"+table+"'");
			if (results!=null)
			{
				for (int i=0; i < results.ColumnNames.Count;++i)
				{
					if ( (string)results.ColumnNames[i] == column) 
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
		static public bool TableExists(SQLiteClient m_db, string table)
		{
			SQLiteResultSet results;
			if (m_db==null) return false;
			if (table==null) return false;
			if (table.Length==0) return false;
			results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+table+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
			if (results!=null)
			{
				if (results.Rows.Count==1) 
				{
					ArrayList arr = (ArrayList)results.Rows[0];
					if (arr.Count==1)
					{
						if ( (string)arr[0] == table) 
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Helper function to create a new table in the database
		/// </summary>
		/// <param name="strTable">name of table</param>
		/// <param name="strSQL">SQL command to create the new table</param>
		/// <returns>true if table is created</returns>
		static public bool AddTable( SQLiteClient dbHandle,  string strTable, string strSQL)
		{
		//	lock (typeof(DatabaseUtility))
			{
				Log.Write("AddTable: {0}",strTable);
				if (dbHandle==null) 
				{
					Log.Write("AddTable: database not opened");
					return false;
				}
				if (strSQL==null) 
				{
					Log.Write("AddTable: no sql?");
					return false;
				}
				if (strTable==null) 
				{
					Log.Write("AddTable: No table?");
					return false;
				}
				if (strTable.Length==0) 
				{
					Log.Write("AddTable: empty table?");
					return false;
				}
				if (strSQL.Length==0) 
				{
					Log.Write("AddTable: empty sql?");
					return false;
				}

				//Log.Write("check for  table:{0}", strTable);
				SQLiteResultSet results;
				results = dbHandle.Execute("SELECT name FROM sqlite_master WHERE name='"+strTable+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
				if (results!=null)
				{
					Log.Write("  results:{0}", results.Rows.Count);
					if (results.Rows.Count==1) 
					{
						Log.Write(" check result:0");
						ArrayList arr = (ArrayList)results.Rows[0];
						if (arr.Count==1)
						{
							string tableName=((string)arr[0]).Trim();
							if ( String.Compare(tableName,strTable,true)==0) 
							{
								//Log.Write(" table exists");
								return false;
							}
							Log.Write(" table has different name:[{0}[ [{1}]", tableName,strTable);
						}
						else Log.Write(" array contains:{0} items?", arr.Count);
					}
				}

				try 
				{
					//Log.Write("create table:{0} {1}", strSQL,dbHandle);
					dbHandle.Execute(strSQL);
					//Log.Write("table created");
				}
				catch (SQLiteException ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message,ex.StackTrace,strSQL);
				}
				return true;
			}
		}

		static public int GetAsInt(SQLiteResultSet results, int iRecord, string strColum)
		{
			string result=Get(results, iRecord, strColum);
			int returnValue=-1;
			try
			{
				returnValue=Int32.Parse(result);
			}
			catch(Exception)
			{
				Log.Write("DatabaseUtility:GetAsInt() column:{0} record:{1} value:{2} is not an int",
													strColum,iRecord,result);
			}
			return returnValue;
		}
		static public long GetAsInt64(SQLiteResultSet results, int iRecord, string strColum)
		{
			string result=Get(results, iRecord, strColum);
			long returnValue=-1;
			try
			{
				returnValue=Int64.Parse(result);
			}
			catch(Exception)
			{
				Log.Write("DatabaseUtility:GetAsInt64() column:{0} record:{1} value:{2} is not an Int64",
					strColum,iRecord,result);
			}
			return returnValue;
		}
		static public long GetAsInt64(SQLiteResultSet results, int iRecord, int iColumn)
		{
			string result=Get(results, iRecord, iColumn);
			long returnValue=-1;
			try
			{
				returnValue=Int64.Parse(result);
			}
			catch(Exception)
			{
				Log.Write("DatabaseUtility:GetAsInt64() column:{0} record:{1} value:{2} is not an Int64",
					iColumn,iRecord,result);
			}
			return returnValue;
		}
		static public string Get(SQLiteResultSet results, int iRecord, string strColum)
		{
			if (null == results) return String.Empty;
			if (results.Rows.Count < iRecord) return String.Empty;
			ArrayList arr = (ArrayList)results.Rows[iRecord];
			int iCol = 0;
			if (results.ColumnIndices.ContainsKey(strColum))
			{
				iCol=(int)results.ColumnIndices[strColum];
				string strLine = ((string)arr[iCol]).Trim();
				strLine = strLine.Replace("''","'");
				return strLine;
			}
			int pos=strColum.IndexOf(".");
			if (pos < 0) return String.Empty;
			strColum=strColum.Substring(pos+1);
			if (results.ColumnIndices.ContainsKey(strColum))
			{
				iCol=(int)results.ColumnIndices[strColum];
				string strLine = ((string)arr[iCol]).Trim();
				strLine = strLine.Replace("''","'");
				return strLine;
			}

			return String.Empty;
		}

		static public int GetAsInt(SQLiteResultSet results, int iRecord, int column)
		{
			string result=Get(results, iRecord, column);
			try
			{
				int intValue=Int32.Parse(result);
				return intValue;
			}
			catch(Exception)
			{}
			return 0;
		}
		static public string Get(SQLiteResultSet results, int iRecord, int column)
		{
			if (null == results) return String.Empty;
			if (results.Rows.Count < iRecord) return String.Empty;
			if (column<0 || column>=results.ColumnNames.Count ) return String.Empty;
			ArrayList arr = (ArrayList)results.Rows[iRecord];
			if (arr[column]==null) return String.Empty;
			string strLine = ((string)arr[column]).Trim();
			strLine = strLine.Replace("''","'");
			return strLine;;
		}

		static public void RemoveInvalidChars(ref string strTxt)
		{
			if (strTxt==null) 
			{
				strTxt=Strings.Unknown;
				return;
			}
			if (strTxt.Length==0) 
			{
				strTxt=Strings.Unknown;
				return;
			}
			string strReturn = String.Empty;
			for (int i = 0; i < (int)strTxt.Length; ++i)
			{
				char k = strTxt[i];
				if (k == '\'') 
				{
					strReturn += "'";
				}
				if((byte)k==0)// remove 0-bytes from the string
					k=(char)32;

				strReturn += k;
			}
			strReturn=strReturn.Trim();
			if (strReturn == String.Empty) 
				strReturn = Strings.Unknown;
			strTxt = strReturn;
		}
		static public string FilterText(string strTxt)
		{
			if (strTxt==null) 
			{
				return Strings.Unknown;
			}
			if (strTxt.Length==0) 
			{
				return Strings.Unknown;
			}
			string strReturn = String.Empty;
			for (int i = 0; i < (int)strTxt.Length; ++i)
			{
				char k = strTxt[i];
				if (k == '\'') 
				{
					strReturn += "'";
				}
				if((byte)k==0)// remove 0-bytes from the string
					k=(char)32;

				strReturn += k;
			}
			strReturn=strReturn.Trim();
			if (strReturn == String.Empty) 
				strReturn = Strings.Unknown;
			return strReturn;
		}

		static public void Split(string strFileNameAndPath, out string strPath, out string strFileName)
		{
			strFileNameAndPath = strFileNameAndPath.Trim();
			strFileName = "";
			strPath = "";
			if (strFileNameAndPath.Length == 0) return;
			int i = strFileNameAndPath.Length - 1;
			while (i > 0)
			{
				char ch = strFileNameAndPath[i];
				if (ch == ':' || ch == '/' || ch == '\\') break;
				else i--;
			}
			strPath = strFileNameAndPath.Substring(0, i).Trim();
			strFileName = strFileNameAndPath.Substring(i, strFileNameAndPath.Length - i).Trim();
		}

	}
}
