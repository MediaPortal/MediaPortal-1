/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;
using MediaPortal.Utils.Services;

namespace MediaPortal.Database
{
	/// <summary>
	/// Summary description for DatabaseUtility.
	/// </summary>
	public class DatabaseUtility
	{
    static ILog _log;

    private DatabaseUtility()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    static public void CompactDatabase(SQLiteClient m_db)
    {
      //m_db.Execute("vacuum");
    }
    static public void SetPragmas(SQLiteClient m_db)
    {
      m_db.Execute("PRAGMA cache_size=2000");
      m_db.Execute("PRAGMA synchronous='OFF'");
      m_db.Execute("PRAGMA count_changes=1");
      m_db.Execute("PRAGMA full_column_names=0");
      m_db.Execute("PRAGMA short_column_names=0");
      m_db.Execute("PRAGMA auto_vacuum=1");
      DatabaseUtility.CompactDatabase(m_db);
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
      results = m_db.Execute("SELECT name FROM sqlite_master WHERE name like '" + table + "' and type like 'table'");// UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
			if (results!=null)
			{
				if (results.Rows.Count==1) 
				{
					SQLiteResultSet.Row arr = results.Rows[0];
					if (arr.fields.Count==1)
					{
						if ( arr.fields[0] == table) 
						{
							return true;
						}
					}
				}
			}
			return false;
		}
    
    static public void AddIndex(SQLiteClient dbHandle, string indexName, string strSQL)
    {
      SQLiteResultSet results;
      bool res = false;
      results = dbHandle.Execute("SELECT name FROM sqlite_master WHERE name='" + indexName + "' and type='index' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='index' ORDER BY name");
      if (results != null && results.Rows.Count > 0)
      {
        if (results.Rows.Count == 1)
        {
          SQLiteResultSet.Row arr = results.Rows[0];
          if (arr.fields.Count == 1)
          {
            if (arr.fields[0] == indexName)
            {
              res = true;
            }
          }
        }
      }
      if (res == true) return;
      try
      {
        dbHandle.Execute(strSQL);
      }
      catch (SQLiteException ex)
      {
        _log.Error("DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message, ex.StackTrace, strSQL);
      }
      return ;
    }
		/// <summary>
		/// Helper function to create a new table in the database
		/// </summary>
		/// <param name="strTable">name of table</param>
		/// <param name="strSQL">SQL command to create the new table</param>
		/// <returns>true if table is created</returns>
		static public bool AddTable( SQLiteClient dbHandle,  string strTable, string strSQL)
		{
      if (TableExists(dbHandle, strTable)) return false;
			try 
			{
				//_log.Info("create table:{0} {1}", strSQL,dbHandle);
				dbHandle.Execute(strSQL);
				//_log.Info("table created");
			}
			catch (SQLiteException ex) 
			{
				_log.Error("DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message,ex.StackTrace,strSQL);
			}
			return true;
		}

		static public int GetAsInt(SQLiteResultSet results, int iRecord, string strColum)
		{
			string result=Get(results, iRecord, strColum);
      if (result == null) return 0;
      if (result.Length==0) return 0;
			int returnValue=-1;
			try
			{
				returnValue=Int32.Parse(result);
			}
			catch(Exception)
			{
				_log.Info("DatabaseUtility:GetAsInt() column:{0} record:{1} value:{2} is not an int",
													strColum,iRecord,result);
			}
			return returnValue;
		}
		static public long GetAsInt64(SQLiteResultSet results, int iRecord, string strColum)
		{
      string result = Get(results, iRecord, strColum);
      if (result == null) return 0;
      if (result.Length == 0) return 0;
			long returnValue=-1;
			try
			{
				returnValue=Int64.Parse(result);
			}
			catch(Exception)
			{
				_log.Info("DatabaseUtility:GetAsInt64() column:{0} record:{1} value:{2} is not an Int64",
					strColum,iRecord,result);
			}
			return returnValue;
		}
		static public string Get(SQLiteResultSet results, int iRecord, string strColum)
		{
			if (null == results) return String.Empty;
      if (results.Rows.Count == 0) return String.Empty;
			if (results.Rows.Count < iRecord) return String.Empty;
      SQLiteResultSet.Row arr = results.Rows[iRecord];
			int iCol = 0;
			if (results.ColumnIndices.ContainsKey(strColum))
			{
				iCol=(int)results.ColumnIndices[strColum];
				if (arr.fields[iCol]==null) return String.Empty;
				string strLine = (arr.fields[iCol]).Trim();
				//strLine = strLine.Replace("''","'");
				return strLine;
			}
			int pos=strColum.IndexOf(".");
			if (pos < 0) return String.Empty;
			strColum=strColum.Substring(pos+1);
			if (results.ColumnIndices.ContainsKey(strColum))
			{
				iCol=(int)results.ColumnIndices[strColum];
				if (arr.fields[iCol]==null) return String.Empty;
				string strLine = (arr.fields[iCol]).Trim();
				//strLine = strLine.Replace("''","'");
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
    
    static public long GetAsInt64(SQLiteResultSet results, int iRecord, int column)
    {
      string result = Get(results, iRecord, column);
      try
      {
        long longValue = Int64.Parse(result);
        return longValue;
      }
      catch (Exception)
      { }
      return 0;
    }

		static public string Get(SQLiteResultSet results, int iRecord, int column)
		{
			if (null == results) return String.Empty;
			if (results.Rows.Count < iRecord) return String.Empty;
			if (column<0 || column>=results.ColumnNames.Count ) return String.Empty;
			SQLiteResultSet.Row arr = results.Rows[iRecord];
			if (arr.fields[column]==null) return String.Empty;
			string strLine = (arr.fields[column]).Trim();
			//strLine = strLine.Replace("''","'");
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
