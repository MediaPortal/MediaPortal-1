using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using SQLite.NET;

using MediaPortal.GUI.Library;		
using WindowPlugins.GUIPrograms;
using Programs.Utils;


namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for FilelinkItem.
	/// </summary>
	public class FilelinkItem: ProgramsDatabase.FileItem
	{
		int mTargetAppID;

		public FilelinkItem(SQLiteClient paramDB): base(paramDB)
		{
		}

		public int TargetAppID
		{
			get{ return mTargetAppID; }
			set{ mTargetAppID = value; }
		}

		public override void Clear()
		{
			base.Clear();
			mTargetAppID = -1;
		}

		public override void Write()
		{
			if (Exists())
			{
				Update();
			}
			else
			{
				Insert();
			}
		}

		public override void Delete()
		{	
			try
			{
				//m_db.Execute("begin");
				string strSQL2 = String.Format(String.Format("DELETE FROM filterItem WHERE appid = {0} AND grouperAppID = {1} AND fileID = {2}", this.TargetAppID, this.AppID, this.FileID));
				m_db.Execute(strSQL2);
				//m_db.Execute("commit");
			}
			catch (SQLiteException ex) 
			{	
				m_db.Execute("rollback");
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}


		private bool Exists()
		{
			SQLiteResultSet results;
			int res = 0;
			results = m_db.Execute(String.Format("SELECT COUNT(*) FROM filterItem WHERE appid = {0} AND grouperAppID = {1} AND fileID = {2};", this.TargetAppID, this.AppID, this.FileID));
			if (results!=null&& results.Rows.Count>0) 
			{
				ArrayList arr = (ArrayList)results.Rows[0];
				res = Int32.Parse((string)arr[0]);
			}
			return (res > 0);
		}

		private void Insert()
		{
			try
			{
				//m_db.Execute("begin");
				string strSQL2 = String.Format(String.Format("INSERT INTO filterItem (appid, grouperAppID, fileID, filename) VALUES ({0}, {1}, {2}, '{3}');", this.TargetAppID, this.AppID, this.FileID, ProgramUtils.Encode(Filename)));
				Log.Write("hi from filelinkiteminsert: {0}", strSQL2);
				m_db.Execute(strSQL2);
				//m_db.Execute("commit");
			}
			catch (SQLiteException ex) 
			{	
				m_db.Execute("rollback");
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		private void Update()
		{
			// nothing to update (yet)
			//...... as all FILTERITEM fields are primary key fields...
		}

	}
}
