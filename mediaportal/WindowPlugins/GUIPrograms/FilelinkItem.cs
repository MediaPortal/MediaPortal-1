using System;
using System.Collections;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for FilelinkItem.
	/// </summary>
	public class FilelinkItem: FileItem
	{
		int mTargetAppID;

		public FilelinkItem(SQLiteClient initSqlDB): base(initSqlDB)
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
				//sqlDB.Execute("begin");
				string strSQL2 = String.Format(String.Format("DELETE FROM filterItem WHERE appid = {0} AND grouperAppID = {1} AND fileID = {2}", this.TargetAppID, this.AppID, this.FileID));
				sqlDB.Execute(strSQL2);
				//sqlDB.Execute("commit");
			}
			catch (SQLiteException ex) 
			{	
				sqlDB.Execute("rollback");
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}


		private bool Exists()
		{
			SQLiteResultSet results;
			int res = 0;
			results = sqlDB.Execute(String.Format("SELECT COUNT(*) FROM filterItem WHERE appid = {0} AND grouperAppID = {1} AND fileID = {2};", this.TargetAppID, this.AppID, this.FileID));
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
				//sqlDB.Execute("begin");
				string strSQL2 = String.Format(String.Format("INSERT INTO filterItem (appid, grouperAppID, fileID, filename) VALUES ({0}, {1}, {2}, '{3}');", this.TargetAppID, this.AppID, this.FileID, ProgramUtils.Encode(Filename)));
				Log.Write("hi from filelinkiteminsert: {0}", strSQL2);
				sqlDB.Execute(strSQL2);
				//sqlDB.Execute("commit");
			}
			catch (SQLiteException ex) 
			{	
				sqlDB.Execute("rollback");
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
