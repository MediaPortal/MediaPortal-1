using System.Collections;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for Applist.
	/// </summary>
	public class Applist: ArrayList
	{
		public static SQLiteClient m_db=null;
		static ApplicationFactory appFactory = ApplicationFactory.AppFactory;

		static public event AppItem.FilelinkLaunchEventHandler OnLaunchFilelink = null;

		public Applist(SQLiteClient paramDB, AppItem.FilelinkLaunchEventHandler curHandler)
		{
			// constructor: save SQLiteDB object and load list from DB
			m_db = paramDB;
			OnLaunchFilelink += curHandler;
			LoadAll();
		}

		static private AppItem DBGetApp(SQLiteResultSet results,int iRecord)
		{
			// AppItem newApp = new AppItem(m_db);
			AppItem newApp = appFactory.GetAppItem(m_db, ProgramUtils.GetSourceType(results, iRecord, "source_type"));
			newApp.OnLaunchFilelink += new AppItem.FilelinkLaunchEventHandler(LaunchFilelink);
			newApp.Enabled = ProgramUtils.GetBool(results, iRecord, "enabled");
			newApp.AppID = ProgramUtils.GetIntDef(results, iRecord, "appid", -1);
			newApp.FatherID = ProgramUtils.GetIntDef(results, iRecord, "fatherID", -1);
			newApp.Title = ProgramUtils.Get(results, iRecord, "title");
			newApp.ShortTitle = ProgramUtils.Get(results, iRecord, "shorttitle");
			newApp.Filename = ProgramUtils.Get(results, iRecord, "filename");
			newApp.Arguments = ProgramUtils.Get(results, iRecord, "arguments");
			newApp.WindowStyle = ProgramUtils.GetProcessWindowStyle(results, iRecord, "windowstyle");
			newApp.Startupdir = ProgramUtils.Get(results, iRecord, "startupdir");
			newApp.UseShellExecute = ProgramUtils.GetBool(results, iRecord, "useshellexecute");
			newApp.UseQuotes = ProgramUtils.GetBool(results, iRecord, "usequotes");
			newApp.SourceType = ProgramUtils.GetSourceType(results, iRecord, "source_type");  
			newApp.Source = ProgramUtils.Get(results, iRecord, "source");
			newApp.Imagefile = ProgramUtils.Get(results, iRecord, "imagefile");
			newApp.FileDirectory = ProgramUtils.Get(results, iRecord, "filedirectory");
			newApp.ImageDirectory = ProgramUtils.Get(results, iRecord, "imagedirectory");
			newApp.ValidExtensions = ProgramUtils.Get(results, iRecord, "validextensions");
			newApp.ImportValidImagesOnly = ProgramUtils.GetBool(results, iRecord, "importvalidimagesonly");
			newApp.Position = ProgramUtils.GetIntDef(results, iRecord, "position", 0);
			newApp.EnableGUIRefresh = ProgramUtils.GetBool(results, iRecord, "enableGUIRefresh");
			newApp.ContentID = ProgramUtils.GetIntDef(results, iRecord, "contentID", 100);
			newApp.SystemDefault = ProgramUtils.Get(results, iRecord, "systemdefault");
			newApp.WaitForExit = ProgramUtils.GetBool(results, iRecord, "waitforexit");
			newApp.Pincode = ProgramUtils.GetIntDef(results, iRecord, "pincode", -1);
			return newApp;
		}

		public ArrayList appsOfFatherID(int FatherID)
		{
			ArrayList res = new ArrayList();
			foreach(AppItem curApp in this)
			{
				if (curApp.FatherID == FatherID)
				{
					res.Add(curApp);
				}
			}
			return res;
		}

		public ArrayList appsOfFather(AppItem Father)
		{
			if (Father == null)
			{
				return appsOfFatherID(-1); // return children of root node!
			}
			else
			{
				return appsOfFatherID(Father.AppID);
			}
		}



		public AppItem GetAppByID(int nAppID)
		{
			foreach(AppItem curApp in this)
			{
				if (curApp.AppID == nAppID) 
				{
					return curApp;
				}
			}
			return null;
		}

		public AppItem CloneAppItem(AppItem sourceApp)
		{
			AppItem newApp = appFactory.GetAppItem(m_db, sourceApp.SourceType);
			newApp.Assign(sourceApp);
			newApp.AppID = -1; // to force a sql INSERT when written
			Add(newApp);
			return newApp;
		}


		
		static void LaunchFilelink(FilelinkItem curLink, bool MPGUIMode)
		{
			OnLaunchFilelink(curLink, MPGUIMode);
		}


		public int GetMaxPosition(int FatherID)
		{
			int nRes = 0;
			foreach(AppItem curApp in this)
			{
				if ((curApp.FatherID == FatherID) && (curApp.Position > nRes))
				{
					nRes = curApp.Position;
				}
			}
			return nRes;

		}

//		public void LoadEnabled()
//		{
//			if (m_db==null) return;
//			try
//			{
//				Clear();
//				if (null==m_db) return ;
//				SQLiteResultSet results;
//				results=m_db.Execute("select * from application where enabled = 'T' order by position");
//				if (results.Rows.Count == 0)  return;
//				for (int iRow=0; iRow < results.Rows.Count;iRow++)
//				{
//					AppItem curApp = DBGetApp(results,iRow);
//					Add(curApp);
//				}
//			}
//			catch (SQLiteException ex) 
//			{
//				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
//			}
//		}
//

		public void LoadAll()
		{
			if (m_db==null) return;
			try
			{
				Clear();
				if (null==m_db) return ;
				SQLiteResultSet results;
				results=m_db.Execute("select * from application order by position");
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow < results.Rows.Count;iRow++)
				{
					AppItem curApp = DBGetApp(results,iRow);
					Add(curApp);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

	}

}
