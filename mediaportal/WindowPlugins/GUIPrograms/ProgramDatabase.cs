using System;
using System.Collections;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using Programs.Utils;


namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for DBPrograms.
	/// </summary>
	public class ProgramDatabase
	{
		public static SQLiteClient m_db = null;
		static Applist mAppList = null;

		// singleton. Dont allow any instance of this class
		private ProgramDatabase()
		{
		}

		static ProgramDatabase()
		{
			try 
			{
				// Open database
				System.IO.Directory.CreateDirectory("database");
				m_db = new SQLiteClient(@"database\ProgramDatabase.db");
				// make sure the DB-structure is complete
				CreateObjects();
			} 
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			mAppList = new Applist(m_db);
		}

		static void MigrateXML2App()
		{
			using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				for (int i=0; i < 20; i++)
				{
					AppItem app = new AppItem(m_db);
					app.Title = xmlreader.GetValueAsString("myprograms", String.Format("sharename{0}",i),"");
					app.Filename = xmlreader.GetValueAsString("myprograms", String.Format("sharelaun{0}",i),"");
					app.FileDirectory = xmlreader.GetValueAsString("myprograms", String.Format("sharepath{0}",i),"");
					app.ImageDirectory = xmlreader.GetValueAsString("myprograms", String.Format("shareimgd{0}",i),"");
					app.ValidExtensions = xmlreader.GetValueAsString("myprograms", String.Format("shareexts{0}",i),"");
					app.SourceType = myProgSourceType.DIRBROWSE;
					app.Enabled = true;
					app.Position = (i+1) * 10;
					if (app.Title.Length > 0)
					{
						app.Write();
					}
				}
			}

		}


		static bool AddObject( string strName, string strType, string strSQL)
		// checks if object exists and returns true if it newly added the object
		{
			if (m_db==null) return false;
			SQLiteResultSet results;
			results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+strName+"' and type='"+strType+"' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='"+strType+"' ORDER BY name");
			if (results!=null&& results.Rows.Count>0) 
			{
				if (results.Rows.Count==1) 
				{
					ArrayList arr = (ArrayList)results.Rows[0];
					if (arr.Count==1)
					{
						if ( (string)arr[0] == strName) 
						{
							return false;
						}
					}
				}
			}

			try 
			{
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return true;
		}

		static bool CreateObjects()
		{
			bool bDoMigration = false;
			if (m_db==null) return false;
			bDoMigration = AddObject("application", "table", "CREATE TABLE application (appid integer primary key, title text, shorttitle text, filename text, arguments text, windowstyle text, startupdir text, useshellexecute text, usequotes text, source_type text, source text, imagefile text, filedirectory text, imagedirectory text, validextensions text, enabled text, importvalidimagesonly text, position integer);\n");
			AddObject("file", "table", "CREATE TABLE file (fileid integer primary key, appid integer, title text, filename text, imagefile text, genre text, genre2 text, genre3 text, genre4 text, genre5 text, country text, manufacturer text, year integer, rating integer, overview text, system text, import_flag integer, manualfilename text, lastTimeLaunched text, launchcount integer, isfolder text, external_id integer)\n");
			AddObject("idxFile1", "index", "CREATE INDEX idxFile1 ON file(appid)\n");
			if (bDoMigration) // if application table had to be created, do a migration of the current XML settings!
				MigrateXML2App();
			return true;
		}


		static public Applist AppList
		{
			get{ return mAppList; }
		}

	}
}
