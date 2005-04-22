using System;
using System.Collections;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for FilelinkList.
	/// </summary>
	public class FilelinkList: ArrayList
	{
		public FilelinkList(SQLiteClient paramDB)
		{
			// constructor: save SQLiteDB object 
			m_db = paramDB;
		}

		static SQLiteClient m_db=null;


		static private FilelinkItem DBGetFilelinkItem(SQLiteResultSet results,int iRecord)
		{
			FilelinkItem newLink = new FilelinkItem(m_db);
			newLink.FileID = ProgramUtils.GetIntDef(results, iRecord, "fileid", -1);
			newLink.AppID = ProgramUtils.GetIntDef(results, iRecord, "grouperappid", -1);
			newLink.TargetAppID = ProgramUtils.GetIntDef(results, iRecord, "targetappid", -1);
			newLink.Title = ProgramUtils.Get(results,iRecord,"title");
			newLink.Filename = ProgramUtils.Get(results,iRecord,"filename");
			newLink.Filepath = ProgramUtils.Get(results,iRecord,"filepath");
			newLink.Imagefile = ProgramUtils.Get(results,iRecord,"imagefile");
			newLink.Genre = ProgramUtils.Get(results,iRecord,"genre");
			newLink.Country = ProgramUtils.Get(results,iRecord,"country");
			newLink.Manufacturer = ProgramUtils.Get(results,iRecord,"manufacturer");
			newLink.Year = ProgramUtils.GetIntDef(results, iRecord, "year", -1);
			newLink.Rating = ProgramUtils.GetIntDef(results, iRecord, "rating", 5);
			newLink.Overview = ProgramUtils.Get(results,iRecord,"overview");
			newLink.System_ = ProgramUtils.Get(results,iRecord,"system");
			newLink.ExtFileID = ProgramUtils.GetIntDef(results, iRecord, "external_id", -1);
			newLink.LastTimeLaunched = ProgramUtils.GetDateDef(results,iRecord, "lastTimeLaunched", DateTime.MinValue);
			newLink.LaunchCount = ProgramUtils.GetIntDef(results, iRecord, "launchcount", 0);
			newLink.IsFolder = ProgramUtils.GetBool(results, iRecord, "isfolder");
			return newLink;
		}


		public void Load(int nAppID, string strPath)
		{
			if (m_db==null) return;
			try
			{
				Clear();
				if (null==m_db) return ;
				SQLiteResultSet results;
				string strSQL = "";
				// mFilepath = strPath;
				// app.
				// SPECIAL: the current application IS NOT the application with the launchinfo!
				strSQL = String.Format("SELECT fi.appid AS targetappid, fi.grouperappid AS grouperappid, f.fileid AS fileid, title, uppertitle, f.filename as filename, filepath, imagefile, genre, genre2, genre3, genre4, genre5, country, manufacturer, YEAR, rating, overview, SYSTEM, import_flag, manualfilename, lasttimelaunched, launchcount, isfolder, external_id FROM FILE f, filteritem fi WHERE f.fileid = fi.fileid AND grouperappid = {0} ORDER BY filepath, uppertitle", nAppID);
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow < results.Rows.Count;iRow++)
				{
					FilelinkItem curLink = DBGetFilelinkItem(results,iRow);
					Add(curLink);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("Filedatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

	}
}
