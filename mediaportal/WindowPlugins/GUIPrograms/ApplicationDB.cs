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

	public class Applist: ArrayList
	{
		static SQLiteClient m_db=null;
		static ApplicationFactory appFactory = ApplicationFactory.AppFactory;

		public Applist(SQLiteClient paramDB)
		{
			// constructor: save SQLiteDB object and load list from DB
			m_db = paramDB;
			LoadEnabled();
		}

		static private AppItem DBGetApp(SQLiteResultSet results,int iRecord)
		{
			// AppItem newApp = new AppItem(m_db);
			AppItem newApp = appFactory.GetAppItem(m_db, ProgramUtils.GetSourceType(results, iRecord, "source_type"));
			newApp.Enabled = ProgramUtils.GetBool(results, iRecord, "enabled");
			newApp.AppID = ProgramUtils.GetIntDef(results, iRecord, "appid", -1);
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
			return newApp;
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


		public int GetMaxPosition()
		{
			int nRes = 0;
			foreach(AppItem curApp in this)
			{
				if (curApp.Position > nRes)
				{nRes = curApp.Position;}
			}
			return nRes;

		}

		public void LoadEnabled()
		{
			if (m_db==null) return;
			try
			{
				Clear();
				if (null==m_db) return ;
				SQLiteResultSet results;
				results=m_db.Execute("select * from application where enabled = 'T' order by position");
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


	public class Filelist: ArrayList
	{

		static SQLiteClient m_db=null;

		public Filelist(SQLiteClient paramDB)
		{
			// constructor: save SQLiteDB object 
			m_db = paramDB;
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
			newFile.Imagefile = ProgramUtils.Get(results,iRecord,"imagefile");
			newFile.Genre = ProgramUtils.Get(results,iRecord,"genre");
			newFile.Country = ProgramUtils.Get(results,iRecord,"country");
			newFile.Manufacturer = ProgramUtils.Get(results,iRecord,"manufacturer");
			newFile.Year = ProgramUtils.GetIntDef(results, iRecord, "year", -1);
			newFile.Rating = ProgramUtils.GetIntDef(results, iRecord, "rating", 5);
			newFile.Overview = ProgramUtils.Get(results,iRecord,"overview");
			newFile.System = ProgramUtils.Get(results,iRecord,"system");
			newFile.ExtFileID = ProgramUtils.GetIntDef(results, iRecord, "external_id", -1);
			newFile.LastTimeLaunched = ProgramUtils.GetDateDef(results,iRecord, "lastTimeLaunched", DateTime.MinValue);
			newFile.LaunchCount = ProgramUtils.GetIntDef(results, iRecord, "launchcount", 0);
			return newFile;
		}

		public void Load(int nAppID)
		{
			if (m_db==null) return;
			try
			{
				Clear();
				if (null==m_db) return ;
				SQLiteResultSet results;
				results=m_db.Execute(String.Format("select * from file where appid = {0} order by title", nAppID));
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

	public class AppItem
	{
		protected static SQLiteClient m_db=null;
		private ProgramDBComparer dbPc = new ProgramDBComparer(); 

		int mAppID;
		string mTitle;
		string mShortTitle; 
		string mFilename; 
		string mArguments; 
		ProcessWindowStyle mWindowStyle; 
		string mStartupdir; 
		bool mUseShellExecute;
		bool mUseQuotes;
		myProgSourceType mSourceType;
		string mSource;
		string mImagefile; 
		public string[] ImageDirs;
		string mFileDirectory; 
		string mImageDirectory; 
		string mValidExtensions;
		bool mImportValidImagesOnly;
		int mPosition; 
		bool mEnabled;
		
		protected bool bChildrenLoaded = false; // load on demand....
		protected Filelist mFiles = null;

		// event: read new file
		public delegate void RefreshInfoEventHandler (string strLine);
		public event RefreshInfoEventHandler OnRefreshInfo = null;

		protected void SendRefreshInfo(string Message)
		{
			if (OnRefreshInfo != null)
			{
				OnRefreshInfo(Message);
			}
		}

		protected int GetID = ProgramUtils.GetID;

		public AppItem(SQLiteClient paramDB)
		{
			// constructor: save SQLiteDB object 
			m_db = paramDB;
			// .. init member variables ...
			mAppID = -1;
			mTitle = "";
			mShortTitle = ""; 
			mFilename = ""; 
			mArguments = ""; 
			mWindowStyle = ProcessWindowStyle.Normal;
			mStartupdir = ""; 
			mUseShellExecute = false;
			mUseQuotes = true;
			mEnabled = true;
			mSourceType = myProgSourceType.UNKNOWN; 
			mSource = ""; 
			mImagefile = ""; 
			mFileDirectory = ""; 
			mImageDirectory = ""; 
			mValidExtensions = ""; 
			mPosition = 0;
			mImportValidImagesOnly = false;
			
			bChildrenLoaded = false;
		}

		public SQLiteClient db
		{
			get{ return m_db; }
		}


		public FileItem PrevFile(FileItem curFile)
		{
			if (Files == null) {return null;}
			if (Files.Count == 0) {return null;}
			int nIndex = this.Files.IndexOf(curFile);
			nIndex = nIndex - 1;
			if (nIndex < 0)
				nIndex = Files.Count - 1;
			return (FileItem)Files[nIndex];
		}

		public FileItem NextFile(FileItem curFile)
		{
			if (Files == null) {return null;}
			if (Files.Count == 0) {return null;}
			int nIndex = this.Files.IndexOf(curFile);
			nIndex = nIndex + 1;
			if (nIndex > Files.Count - 1)
				nIndex = 0;
			return (FileItem)Files[nIndex];
		}


		public virtual void LaunchFile(int FileID)
		{
			// todo: launch File by ID
		}

		public virtual void LaunchFile(FileItem curFile)
		{
			string curFilename = curFile.Filename;
			// Launch File by item
			curFile.UpdateLaunchInfo();
			Process proc = new Process();
			if (Filename != "")
			{
				// use the APPLICATION launcher and add current file information
				proc.StartInfo.FileName = Filename; // filename of the application
				// set the arguments: one of the arguments is the fileitem-filename
				proc.StartInfo.Arguments = " " + this.Arguments + " ";
				if (UseQuotes) 
				{
					// avoid double quotes around the filename-argument.....
					curFilename = " \"" + (curFile.Filename.TrimStart('\"')).TrimEnd('\"') + "\"";
				}
				// the fileitem-argument can be positioned anywhere in the argument string...
				if (proc.StartInfo.Arguments.IndexOf("%FILE%") == -1)
				{
					// no placeholder found => default handling: add the fileitem as the last argument
					proc.StartInfo.Arguments = proc.StartInfo.Arguments + curFilename;
				}
				else
				{
					// placeholder found => replace the placeholder by the correct filename
					proc.StartInfo.Arguments = proc.StartInfo.Arguments.Replace("%FILE%", curFile.Filename);
				}
				proc.StartInfo.WorkingDirectory  = Startupdir;
				if (proc.StartInfo.WorkingDirectory.IndexOf("%FILEDIR%") != -1)
				{
					//Log.Write("curFile.Filename {0}", curFile.Filename);
					proc.StartInfo.WorkingDirectory = proc.StartInfo.WorkingDirectory.Replace("%FILEDIR%", Path.GetDirectoryName(curFile.Filename));
				}
				proc.StartInfo.UseShellExecute = UseShellExecute;
			}
			else 
			{
				// application has no launch-file 
				// => try to make a correct launch using the current FILE object
				string strCurFilename = curFile.ExtractFileName();
				proc.StartInfo.FileName = strCurFilename;
				proc.StartInfo.Arguments = curFile.ExtractArguments();
				proc.StartInfo.WorkingDirectory  = curFile.ExtractDirectory(strCurFilename);
				proc.StartInfo.UseShellExecute = UseShellExecute; // todo: check if a file is executable
			}
			proc.StartInfo.WindowStyle = this.WindowStyle;
			try
			{

				proc.Start();
				proc.WaitForExit();
				GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);


				//				Log.Write("myPrograms: DEBUG LOG program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n",
				//					proc.StartInfo.FileName, 
				//					proc.StartInfo.Arguments, 
				//					proc.StartInfo.WorkingDirectory);
			}
			catch (Exception ex)
			{
				Log.Write("myPrograms: error launching program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
					proc.StartInfo.FileName, 
					proc.StartInfo.Arguments, 
					proc.StartInfo.WorkingDirectory, 
					ex.Message, 
					ex.Source, 
					ex.StackTrace);
			}   
		}

		public virtual void LaunchFile(GUIListItem item)
		{
			// Launch File by GUILISTITEM
			// => look for FileItem and launch it using the found object
			if (item.MusicTag == null) {return;}
			FileItem curFile = (FileItem)item.MusicTag;
			if (curFile == null) { return;}
			this.LaunchFile(curFile);
		}

		public virtual void DisplayFiles(GUIListItem itemParent)
		{
			GUIControl.ClearControl(ProgramUtils.GetID, (int)Controls.CONTROL_LIST ); 
			GUIControl.ClearControl(ProgramUtils.GetID, (int)Controls.CONTROL_THUMBS );
			ProgramUtils.AddBackButton();
			foreach(FileItem curFile in this.Files)
			{
				GUIListItem gli = new GUIListItem( curFile.Title );
				//				gli.Label2 = curFile.LaunchCount.ToString(); // debug
				//              gli.Label2 = String.Format("{0}", curFile.LastTimeLaunched); 
				if (curFile.Imagefile != "")
				{
					gli.ThumbnailImage = curFile.Imagefile;
					gli.IconImageBig = curFile.Imagefile;
					gli.IconImage = curFile.Imagefile;
				}
				else 
				{
					gli.ThumbnailImage = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
					gli.IconImageBig = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
					gli.IconImage = GUIGraphicsContext.Skin+@"\media\DefaultFolderNF.png";
				}
				gli.MusicTag = curFile;
				gli.IsFolder = false; 
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST, gli );
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,gli);
			}
			string strObjects=String.Format("{0} {1}", Files.Count, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}

		public virtual void OnSort(GUIListControl list, GUIThumbnailPanel panel)
		{
			dbPc.updateState();
			list.Sort(dbPc);
			panel.Sort(dbPc);
		}

		public virtual void OnSortToggle(GUIListControl list, GUIThumbnailPanel panel)
		{
			dbPc.bAsc = (!dbPc.bAsc);
			list.Sort(dbPc);
			panel.Sort(dbPc);
		}

		public virtual string CurrentSortTitle()
		{
			return dbPc.currentSortMethodAsText;
		}

		public virtual bool CurrentSortIsAscending()
		{
			return dbPc.bAsc;
		}


		public virtual bool BackItemClick(GUIListItem itemBack)
		{
			return true; // if subfolders are allowed, override this class, do stuff and return true only if mp needs to go back to application-list
		}

		public virtual bool RefreshButtonVisible()
		{
			return false; // otherwise, override this in child class
		}

		public virtual bool FileEditorAllowed()
		{
			return true;  // otherwise, override this in child class
		}

		public virtual void Refresh(bool bGUIMode)
		{
			// descendant classes do that!
		}


		public virtual void OnInfo(GUIListItem item)
		{
			GUIFileInfo pDlgFileInfo = (GUIFileInfo)GUIWindowManager.GetWindow((int)ProgramUtils.ProgramInfoID);
			if (null != pDlgFileInfo)
			{
				if (item.MusicTag == null) { return; }
				FileItem curFile = (FileItem)item.MusicTag;
				pDlgFileInfo.App = this;
				pDlgFileInfo.File = curFile;
				pDlgFileInfo.DoModal(GetID);
				return;
			}
		}

		public int AppID
		{
			get{ return mAppID; }
			set{ mAppID = value; }
		}

		public string Title
		{
			get{ return mTitle; }
			set{ mTitle = value; }
		}

		public string ShortTitle 
		{
			get{ return mShortTitle; }
			set{ mShortTitle = value; }
		}

		public string Filename 
		{
			get{ return mFilename; }
			set{ mFilename = value; }
		}
		public string Arguments 
		{
			get{ return mArguments; }
			set{ mArguments = value; }
		}
		public bool UseQuotes 
		{
			get{ return mUseQuotes; }
			set{ mUseQuotes = value; }
		}
		public bool UseShellExecute
		{
			get{ return mUseShellExecute; }
			set{ mUseShellExecute = value; }
		}

		public bool Enabled
		{
			get{ return mEnabled; }
			set{ mEnabled = value; }
		}

		//		public bool ShortFilenames
		//		{
		//			get{ return mShortFilenames; }
		//			set{ mShortFilenames = value; }
		//		}
		public ProcessWindowStyle WindowStyle 
		{
			get{ return mWindowStyle; }
			set{ mWindowStyle = value; }
		}
		public string Startupdir 
		{
			get{ return mStartupdir; }
			set{ mStartupdir = value; }
		}
		public string FileDirectory 
		{
			get{ return mFileDirectory; }
			set{ mFileDirectory = value; }
		}
		public string ImageDirectory 
		{
			get{ return mImageDirectory; }
			set{ SetImageDirectory(value); }
		}
		private void SetImageDirectory(string value)
		{
			mImageDirectory = value;
			ImageDirs = ((string)mImageDirectory).Split('\r');
			for (int i=0; i < ImageDirs.Length;i++)
			{
				ImageDirs[i] = ImageDirs[i].Trim();
				// hack the \n away.... 
				ImageDirs[i] = ImageDirs[i].TrimStart('\n');
				// hack trailing backslashes away
				ImageDirs[i] = ImageDirs[i].TrimEnd('\\');
			}
		}
		public string Imagefile 
		{
			get{ return mImagefile; }
			set{ mImagefile = value; }
		}
		public string Source 
		{
			get{ return mSource; }
			set{ mSource = value; }
		}
		public myProgSourceType SourceType
		{
			get{ return mSourceType; }
			set{ mSourceType = value; }
		}
		public string ValidExtensions
		{
			get{ return mValidExtensions; }
			set{ mValidExtensions = value; }
		}
		public bool ImportValidImagesOnly
		{
			get{ return mImportValidImagesOnly; }
			set{ mImportValidImagesOnly = value; }
		}
		public int Position 
		{
			get{ return mPosition; }
			set{ mPosition = value; }
		}

		public Filelist Files
		{
			// load on demand....
			get
			{
				if (!bChildrenLoaded)
				{LoadFiles();} 
				return mFiles; 
			}
		}



		private void Insert()
		{
			try
			{
				string strSQL = String.Format("insert into application (appid, title, shorttitle, filename, arguments, windowstyle, startupdir, useshellexecute, usequotes, source_type, source, imagefile, filedirectory, imagedirectory, validextensions, importvalidimagesonly, position, enabled) values(null, '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', {15}, '{16}')", 
					ProgramUtils.Encode(Title), ProgramUtils.Encode(ShortTitle), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Arguments), 
					ProgramUtils.WindowStyleToStr(WindowStyle), ProgramUtils.Encode(Startupdir), ProgramUtils.BooleanToStr(UseShellExecute), 
					ProgramUtils.BooleanToStr(UseQuotes), ProgramUtils.SourceTypeToStr(SourceType), ProgramUtils.Encode(Source), ProgramUtils.Encode(Imagefile), 
					ProgramUtils.Encode(FileDirectory), ProgramUtils.Encode(ImageDirectory), ProgramUtils.Encode(ValidExtensions), ProgramUtils.BooleanToStr(mImportValidImagesOnly), Position, ProgramUtils.BooleanToStr(Enabled));
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		private void Update()
		{
			if (this.AppID >= 0)
			{
				try
				{
					string strSQL = String.Format("update application set title = '{0}', shorttitle = '{1}', filename = '{2}', arguments = '{3}', windowstyle = '{4}', startupdir = '{5}', useshellexecute = '{6}', usequotes = '{7}', source_type = '{8}', source = '{9}', imagefile = '{10}',filedirectory = '{11}',imagedirectory = '{12}',validextensions = '{13}',importvalidimagesonly = '{14}',position = {15}, enabled = '{16}' where appID = {17}", 
						ProgramUtils.Encode(Title), ProgramUtils.Encode(ShortTitle), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Arguments), 
						ProgramUtils.WindowStyleToStr(WindowStyle), ProgramUtils.Encode(Startupdir), ProgramUtils.BooleanToStr(UseShellExecute), 
						ProgramUtils.BooleanToStr(UseQuotes), ProgramUtils.SourceTypeToStr(SourceType), ProgramUtils.Encode(Source), ProgramUtils.Encode(Imagefile), 
						ProgramUtils.Encode(FileDirectory), ProgramUtils.Encode(ImageDirectory), ProgramUtils.Encode(ValidExtensions), ProgramUtils.BooleanToStr(mImportValidImagesOnly), Position, ProgramUtils.BooleanToStr(Enabled), AppID);
					m_db.Execute(strSQL);
				}
				catch (SQLiteException ex) 
				{	
					Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}	
		}

		public void Delete()
		{	
			if (this.AppID >= 0)
			{
				try
				{
					DeleteFiles();
					string strSQL2 = String.Format("delete from application where appid = {0}", this.AppID);
					m_db.Execute(strSQL2);
				}
				catch (SQLiteException ex) 
				{	
					Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}	
		}



		protected void DeleteFiles()
		{
			try
			{
				m_db.Execute(String.Format("delete from file where appid = {0}", AppID));
			}
			catch (SQLiteException ex) 
			{	
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}


		protected virtual void LoadFiles()
		{
			// load Files and fill Files-arraylist here!
			if (mFiles == null) 
			{
				mFiles = new Filelist(m_db);}
			else { 
				mFiles.Clear();
			}
			mFiles.Load(AppID);
			bChildrenLoaded = true;
		}


		public void Write()
		{
			if (mAppID == -1)
			{
				Insert();
			}
			else 
			{
				Update();
			}
		}
	}

	public class FileItem
	{
		static SQLiteClient m_db=null;

		int mFileID;
		int mAppID;
		string mTitle;
		string mFilename;
		string mImagefile;
		string mGenre;
		string mCountry;
		string mManufacturer;
		int mYear;
		int mRating;
		string mOverview;
		string mSystem;
		string mManualFilename;
		DateTime mLastTimeLaunched;
		int mLaunchCount;
		int mExtFileID;
		bool mIsFolder;

		public FileItem(SQLiteClient paramDB)
		{
			// constructor: save SQLiteDB object 
			m_db = paramDB;
			Clear();
		}


		public void Clear()
		{
			mFileID = -1;
			mAppID = -1;
			mTitle = "";
			mFilename = "";
			mImagefile = "";
			mGenre = "";
			mCountry = "";
			mManufacturer = "";
			mYear = -1;
			mRating = -1;
			mOverview = "";
			mSystem = "";
			mManualFilename = "";
			mIsFolder = false;
			mLastTimeLaunched = DateTime.MinValue;
			mLaunchCount = 0;
			mExtFileID = -1;
		}

		private string GetYearManu()
		{
			string result = "";
			if (mYear <= 0) 
			{
				result = mManufacturer;
			}
			else 
			{
				result = mManufacturer + " [" + mYear + "]";
			}
			return (result.Trim());
		}


		public int FileID
		{
			get{ return mFileID; }
			set{ mFileID = value; }
		}
		public int AppID
		{
			get{ return mAppID; }
			set{ mAppID = value; }
		}
		public string Title
		{
			get{ return mTitle; }
			set{ mTitle = value; }
		}
		public string Filename
		{
			get{ return mFilename; }
			set{ mFilename = value; }
		}
		public string Imagefile
		{
			get{ return mImagefile; }
			set{ mImagefile = value; }
		}
		public string Genre
		{
			get{ return mGenre; }
			set{ mGenre = value; }
		}
		public string Country
		{
			get{ return mCountry; }
			set{ mCountry = value; }
		}
		public string Manufacturer
		{
			get{ return mManufacturer; }
			set{ mManufacturer = value; }
		}
		public int Year
		{
			get{ return mYear; }
			set{ mYear = value; }
		}
		public int Rating
		{
			get{ return mRating; }
			set{ mRating = value; }
		}
		public string Overview
		{
			get{ return mOverview; }
			set{ mOverview = value; }
		}
		public string System
		{
			get{ return mSystem; }
			set{ mSystem = value; }
		}

		public string ManualFilename
		{
			get{ return mManualFilename; }
			set{ mManualFilename = value; }
		}

		public DateTime LastTimeLaunched
		{
			get{ return mLastTimeLaunched; }
			set{ mLastTimeLaunched = value; }
		}
		
		public int LaunchCount
		{
			get{ return mLaunchCount; }
			set{ mLaunchCount = value; }
		}

		public bool IsFolder
		{
			get{ return mIsFolder; }
			set{ mIsFolder = value; }
		}

		public int ExtFileID
		{
			get{ return mExtFileID; }
			set{ mExtFileID = value; }
		}

		public string YearManu
		{
			get {return GetYearManu();}
		}

		private int CountQuotes(string strVal)
		{
			int at = 0;
			int start = 0; 
			int nRes = 0;
			while((start < strVal.Length) && (at > -1))
			{
				at = strVal.IndexOf("\"", start);
				if (at == -1) break;
				nRes = nRes + 1;
				start = at+1;
			}
			return nRes;
		}
		
		public string ExtractFileName()
		{
			if (Filename == "") {return "";}
			string strRes = "";
			string strSep = "";
			string[] parts = ((string)Filename).Split(' ');
			if (Filename.StartsWith("\""))
			{
				// filename is quoted => traverse array and concetenate strings until two quotes are found
				int nNbOfQuotes = 0;
				for (int i = 0; i < parts.Length; i++)
				{
					if (nNbOfQuotes <= 2)
					{
						strRes = strRes + strSep + parts[i];
						strSep = " ";
					}
					if (parts[i].IndexOf("\"") >= 0)
					{
						nNbOfQuotes = nNbOfQuotes + CountQuotes(parts[i]);
						if (nNbOfQuotes == 2)
						{break;}
					}
				}
			}
			else
			{
				strRes = parts[0];
			}
			return strRes;

		}


		public string ExtractDirectory(string curFilename)
		{
			string strRes = "";
			string strSep = "";
			string[] parts = ((string)curFilename).Split('\\');
			for (int i = 0; i < parts.Length - 1; i++)
			{
				strRes = strRes + strSep + parts[i];
				strSep = "\\";
			}
			strRes = strRes.TrimStart('\"');
			return strRes;
		}

		public string ExtractImageExtension()
		{
			string strRes = "";
			string[] parts = ((string)this.Imagefile).Split('.');
			if (parts.Length >= 2) 
			{
				// there was an extension
				strRes = '.' + parts[parts.Length-1];
			}
			return strRes;
		}

		public string ExtractImageFileNoPath()
		{
			string strRes = "";
			string[] parts = ((string)this.Imagefile).Split('\\');
			if (parts.Length >= 1) 
			{
				strRes = parts[parts.Length-1];
			}
			return strRes;
		}



		public string ExtractArguments()
		{
			string strRes = "";
			string strSep = "";
			string[] parts = ((string)Filename).Split(' ');
			if (Filename.StartsWith("\""))
			{
				// filename is quoted => traverse array and concetenate strings after two quotes have been found
				int nNbOfQuotes = 0;
				for (int i = 0; i < parts.Length; i++)
				{
					if (nNbOfQuotes >= 2)
					{
						strRes = strRes + strSep + parts[i];
						strSep = " ";
					}
					if (parts[i].IndexOf("\"") >= 0)
					{
						nNbOfQuotes = nNbOfQuotes + CountQuotes(parts[i]);
					}
				}
			}
			else
			{
				for (int i = 1; i < parts.Length; i++)
				{
					strRes = strRes + strSep + parts[i];
					strSep = " ";
				}
			}
			return strRes;
		}

		private void Insert()
		{
			string strLastLaunch = "";
			string strLaunchCount = "";
			string strYear = "";
			if (LastTimeLaunched != DateTime.MinValue)
			{
				strLastLaunch = String.Format("{0}", LastTimeLaunched);
			}
			if (LaunchCount > 0)
			{
				strLaunchCount = String.Format("{0}", LaunchCount); // poor man's inttostr :-)
			}
			if (Year > 0)
			{
				strYear = String.Format("{0}", Year);
			}

			try
			{
				string strSQL = String.Format("insert into file (fileid, appid, title, filename, imagefile, genre, country, manufacturer, year, rating, overview, system, manualfilename, lastTimeLaunched, launchcount, isfolder, external_id) values (null, {0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', {15})", 
					AppID, ProgramUtils.Encode(Title), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Imagefile), ProgramUtils.Encode(Genre), Country, ProgramUtils.Encode(Manufacturer), strYear, Rating, ProgramUtils.Encode(Overview), ProgramUtils.Encode(System), 
					ProgramUtils.Encode(ManualFilename), strLastLaunch, strLaunchCount, ProgramUtils.BooleanToStr(IsFolder), ExtFileID);
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}
			

		private void Update()
		{
			string strLastLaunch = "";
			string strLaunchCount = "";
			string strYear = "";
			if (LastTimeLaunched != DateTime.MinValue)
			{
				strLastLaunch = String.Format("{0}", LastTimeLaunched);
			}
			if (LaunchCount > 0)
			{
				strLaunchCount = String.Format("{0}", LaunchCount); // poor man's inttostr :-)
			}
			if (Year > 0)
			{
				strYear = String.Format("{0}", Year);
			}

			try
			{
				string strSQL = String.Format(
					"update file set title = '{1}', filename = '{2}', imagefile = '{3}', genre = '{4}', country = '{5}', manufacturer = '{6}', year = '{7}', rating = {8}, overview = '{9}', system = '{10}' where  fileid = {0}",
					FileID, 
					ProgramUtils.Encode(Title),
					ProgramUtils.Encode(Filename),
					ProgramUtils.Encode(Imagefile),
					ProgramUtils.Encode(Genre),
					ProgramUtils.Encode(Country),
					ProgramUtils.Encode(Manufacturer),
					strYear,
					Rating,
					ProgramUtils.Encode(Overview),
					ProgramUtils.Encode(System)
				);
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		public void UpdateLaunchInfo()
		{
			try
			{
				LastTimeLaunched = DateTime.Now;
				LaunchCount = LaunchCount + 1;
				string strSQL = String.Format("update file set lastTimeLaunched = '{0}', launchcount = {1} where fileid = {2}", LastTimeLaunched, LaunchCount, FileID);
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		public void Write()
		{
			if (mFileID == -1)	
				{Insert();}
			else 
				{Update();}
		}

		public void Delete()
		{	
			if (this.FileID >= 0)
			{
				try
				{
					string strSQL2 = String.Format("delete from file where fileid = {0}", this.FileID);
					m_db.Execute(strSQL2);
				}
				catch (SQLiteException ex) 
				{	
					Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}	
		}

	}
}
