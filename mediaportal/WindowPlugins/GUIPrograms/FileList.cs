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

	public class Filelist: ArrayList
	{

		string mFilepath = "";

		static SQLiteClient m_db=null;

		public Filelist(SQLiteClient paramDB)
		{
			// constructor: save SQLiteDB object 
			m_db = paramDB;
		}

		public string Filepath
		{
			get{ return mFilepath; }
		}


		public FileItem GetFileByID(int nFileID)
		{
			foreach(FileItem curFile in this)
			{
				if (curFile.FileID == nFileID) 
				{
					return curFile;
				}
			}
			return null;
		}


		static private FileItem DBGetFileItem(SQLiteResultSet results,int iRecord)
		{
			FileItem newFile = new FileItem(m_db);
			newFile.FileID = ProgramUtils.GetIntDef(results, iRecord, "fileid", -1);
			newFile.AppID = ProgramUtils.GetIntDef(results, iRecord, "appid", -1);
			newFile.Title = ProgramUtils.Get(results,iRecord,"title");
			newFile.Filename = ProgramUtils.Get(results,iRecord,"filename");
			newFile.Filepath = ProgramUtils.Get(results,iRecord,"filepath");
			newFile.Imagefile = ProgramUtils.Get(results,iRecord,"imagefile");
			newFile.Genre = ProgramUtils.Get(results,iRecord,"genre");
			newFile.Country = ProgramUtils.Get(results,iRecord,"country");
			newFile.Manufacturer = ProgramUtils.Get(results,iRecord,"manufacturer");
			newFile.Year = ProgramUtils.GetIntDef(results, iRecord, "year", -1);
			newFile.Rating = ProgramUtils.GetIntDef(results, iRecord, "rating", 5);
			newFile.Overview = ProgramUtils.Get(results,iRecord,"overview");
			newFile.System_ = ProgramUtils.Get(results,iRecord,"system");
			newFile.ExtFileID = ProgramUtils.GetIntDef(results, iRecord, "external_id", -1);
			newFile.LastTimeLaunched = ProgramUtils.GetDateDef(results,iRecord, "lastTimeLaunched", DateTime.MinValue);
			newFile.LaunchCount = ProgramUtils.GetIntDef(results, iRecord, "launchcount", 0);
			newFile.IsFolder = ProgramUtils.GetBool(results, iRecord, "isfolder");
			newFile.TagData =  ProgramUtils.Get(results,iRecord,"tagdata");
			newFile.CategoryData =  ProgramUtils.Get(results,iRecord,"categorydata");
			return newFile;
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
				mFilepath = strPath;
				if (strPath == "") 
				{
					strSQL = String.Format("select * from file where appid = {0} order by isfolder desc, uppertitle", nAppID);
				}
				else 
				{
					strSQL = String.Format("select * from file where appid = {0} and filepath = '{1}' order by isfolder desc, uppertitle", nAppID, strPath);
				}
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow < results.Rows.Count;iRow++)
				{
					FileItem curFile = DBGetFileItem(results,iRow);
					Add(curFile);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("Filedatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}


	}

}
