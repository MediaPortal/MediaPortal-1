using System;
using System.Collections;
using SQLite.NET;
using MediaPortal.GUI.Library;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for ProgramSettings.
	/// </summary>
	public class ProgramSettings
	{
		static public SQLiteClient m_db=null;

		// singleton. Dont allow any instance of this class
		private ProgramSettings()
		{
		}

		static ProgramSettings()
		{
		}


		static public string ReadSetting(string Key)
		{
			SQLiteResultSet results;
			string res = null;
			string SQL = "SELECT value FROM setting WHERE key ='"+Key+"'";
			results = m_db.Execute(SQL);
			if (results!=null&& results.Rows.Count>0) 
			{
				ArrayList arr = (ArrayList)results.Rows[0];
				res = (string)arr[0];
			}
			return res;
		}

		static int CountKey(string Key)
		{
			SQLiteResultSet results;
			int res = 0;
			results = m_db.Execute("SELECT COUNT(*) FROM setting WHERE key ='"+Key+"'");
			if (results!=null&& results.Rows.Count>0) 
			{
				ArrayList arr = (ArrayList)results.Rows[0];
				res = Int32.Parse((string)arr[0]);
			}
			return res;
		}

		static public bool KeyExists(string Key)
		{
			return (CountKey(Key) > 0);
		}

		static public void WriteSetting(string Key, string Value)
		{
			if (KeyExists(Key))
			{
				m_db.Execute("update setting set value = '" + Value + "' where key = '" + Key + "'");
			}
			else
			{
				m_db.Execute("insert into setting (key, value) values ('" + Key + "', '" + Value + "');");
			}
		}

		static public void DeleteSetting(string Key)
		{
			m_db.Execute("delete from setting where key = '" + Key + "'");
		}



	}
}
