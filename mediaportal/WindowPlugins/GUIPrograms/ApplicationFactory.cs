using System;
using System.Collections;
using System.Diagnostics;
using SQLite.NET;

using Programs.Utils;


namespace ProgramsDatabase
{
	/// <summary>
	/// Factory object that creates the matchin AppItem descendant class
	/// depending on the sourceType parameter
	/// Descendant classes differ in LOADING and REFRESHING filelists
	/// </summary>
	public class ApplicationFactory
	{
		static public ApplicationFactory AppFactory = new ApplicationFactory();

		// singleton. Dont allow any instance of this class
		private ApplicationFactory()
		{
		}

		static ApplicationFactory()
		{
			// nothing to create......
		}

		public AppItem GetAppItem(SQLiteClient m_db, myProgSourceType sourceType)
		{
			AppItem res = null;
			switch (sourceType)
			{
				case myProgSourceType.DIRBROWSE:
					res = new appItemDirBrowse(m_db);
					break;
				case myProgSourceType.DIRCACHE:
					res = new appItemDirCache(m_db);
					break;
				case myProgSourceType.MYFILEINI:
					res = new appItemMyFileINI(m_db);
					break;
				case myProgSourceType.MYFILEMEEDIO:
					res = new appItemMyFileMLF(m_db);
					break;
				case myProgSourceType.MYGAMESDIRECT:
					res = new appItemMyGamesDirect(m_db);
					break;
				case myProgSourceType.FILELAUNCHER:
					res = new appFilesEdit(m_db);
					break;
				case myProgSourceType.GROUPER:
					res = new appGrouper(m_db);
					break;
			}
			return res;
		}



	}
}
