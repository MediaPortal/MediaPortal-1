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
		/// Delete a column from a table
		/// </summary>
		/// <param name="table">table name</param>
		/// <param name="column">column name</param>
		static public void DeleteColumnFromTable(SQLiteClient m_db, string table, string column)
		{
			if (m_db==null) return ;
			if (table==null) return ;
			if (table.Length==0) return ;
			if (column==null) return ;
			if (column.Length==0) return ;
			if (!TableExists(m_db,table)) return;
			if (!TableColumnExists(m_db,table, column)) return;
			try
			{
				string sql=String.Format("ALTER TABLE {0} DROP COLUMN {1} ",table,column);
				m_db.Execute(sql);
			} 
			catch (SQLiteException ex) 
			{
				Log.Write("DatabaseUtility exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		/// <summary>
		/// Add a column to a table
		/// </summary>
		/// <param name="table">table name</param>
		/// <param name="column">name of new new column </param>
		/// <param name="columnType">type of the new column</param>
		/// <param name="defaultValue">default value for this column</param>
		static public void AddColumnToTable(SQLiteClient m_db, string table, string column, string columnType, string defaultValue)
		{
			if (m_db==null) return ;
			if (table==null) return ;
			if (table.Length==0) return ;
			if (column==null) return ;
			if (column.Length==0) return ;
			if (columnType==null) return ;
			if (columnType.Length==0) return ;
			if (!TableExists(m_db,table)) return;
			if (TableColumnExists(m_db,table, column)) return;

			try
			{
				string sql=String.Format("ALTER TABLE {0} ADD {1} {2}",table,column,columnType);
				m_db.Execute(sql);
				if (defaultValue!=null && defaultValue.Length>0)
				{
					sql=String.Format("update {0} set {1}='{2}'",table,column,defaultValue);
					m_db.Execute(sql);
				}
			} 
			catch (SQLiteException ex) 
			{
				Log.Write("DatabaseUtility exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

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
		static public bool AddTable(SQLiteClient m_db,  string strTable, string strSQL)
		{
			lock (typeof(DatabaseUtility))
			{
				if (m_db==null) 
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
				results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+strTable+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
				if (results!=null)
				{
					//Log.Write("  results:{0}", results.Rows.Count);
					if (results.Rows.Count==1) 
					{
						//Log.Write(" check result:0");
						ArrayList arr = (ArrayList)results.Rows[0];
						if (arr.Count==1)
						{

							if ( (string)arr[0] == strTable) 
							{
								//Log.Write(" table exists");
								return false;
							}
							//Log.Write(" table has different name:{0}", (string)arr[0]);
						}
						//else Log.Write(" array contains:{0} items?", arr.Count);
					}
				}

				try 
				{
					//Log.Write("create table:{0}", strSQL);
					m_db.Execute(strSQL);
					//Log.Write("table created");
				}
				catch (SQLiteException ex) 
				{
					Log.Write("DatabaseUtility exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return true;
			}
		}

		static public string Get(SQLiteResultSet results, int iRecord, string strColum)
		{
			if (null == results) return "";
			if (results.Rows.Count < iRecord) return "";
			ArrayList arr = (ArrayList)results.Rows[iRecord];
			int iCol = 0;
			foreach (string columnName in results.ColumnNames)
			{
				if (strColum == columnName)
				{
					string strLine = ((string)arr[iCol]).Trim();
					strLine = strLine.Replace("''","'");
					return strLine;
				}
				iCol++;
			}
			return "";
		}

		static public void RemoveInvalidChars(ref string strTxt)
		{
			if (strTxt==null) 
			{
				strTxt="unknown";
				return;
			}
			if (strTxt.Length==0) 
			{
				strTxt="unknown";
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
				strReturn += k;
			}
			strReturn=strReturn.Trim();
			if (strReturn == String.Empty) 
				strReturn = "unknown";
			strTxt = strReturn;
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
