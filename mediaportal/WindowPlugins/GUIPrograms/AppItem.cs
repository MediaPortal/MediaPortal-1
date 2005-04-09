using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using SQLite.NET;

using MediaPortal.Ripper;
using MediaPortal.Player;
using MediaPortal.GUI.Library;		
using MediaPortal.Util;
using WindowPlugins.GUIPrograms;
using Programs.Utils;


namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for AppItem.
	/// </summary>
	public class AppItem
	{
		protected static SQLiteClient m_db=null;
		private ProgramDBComparer dbPc = new ProgramDBComparer(); 

		public delegate void FilelinkLaunchEventHandler (FilelinkItem curLink, bool MPGUIMode);
		public event FilelinkLaunchEventHandler OnLaunchFilelink = null;

		int mAppID;
		int mFatherID;
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
		bool mEnableGUIRefresh;
		int mPincode;
		int mContentID; 
	  string mSystemDefault;
    bool mWaitForExit;

		string mLaunchErrorMsg;

		// two magic image-slideshow counters
		int mThumbIndex = 0;
		int mThumbFolderIndex = -1;

		string mLastFilepath = ""; // cached path
		
		protected bool bFilesLoaded = false; // load on demand....
		protected Filelist mFiles = null;

		protected bool bLinksLoaded = false;
		protected FilelinkList mFileLinks = null;

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
			mFatherID = -1;
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
			mEnableGUIRefresh = false;
			mPincode = -1;
			mContentID = -1;
			mSystemDefault = "";
			mWaitForExit = true;
			bFilesLoaded = false;
		}

		public SQLiteClient db
		{
			get{ return m_db; }
		}

		public int CurrentSortIndex
		{
			get{ return GetCurrentSortIndex(); }
			set{ SetCurrentSortIndex(value);}
		}

		public bool CurrentSortIsAscending
		{
			get { return GetCurrentSortIsAscending(); }
			set { SetCurrentSortIsAscending(value); }
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

		protected void LaunchGenericPlayer(string strCommand, string strFilename)
		{
			// don't use quotes!
			strFilename = strFilename.Trim();
			strFilename = strFilename.TrimStart('\"');
			strFilename = strFilename.TrimEnd('\"');
			if (strCommand == "%PLAY%")
			{
				g_Player.Play(strFilename);
			}
			else if (strCommand == "%PLAYAUDIOSTREAM%")
			{
				g_Player.PlayAudioStream(strFilename);
			}
			else if (strCommand == "%PLAYVIDEOSTREAM%")
			{
				g_Player.PlayVideoStream(strFilename);
			}
			else 
			{
				Log.Write("error in myPrograms: AppItem.LaunchGenericPlayer: unknown command: {0}", strCommand);
				return;
			}
		}
		
		public virtual void LaunchFile(FileItem curFile, bool MPGUIMode)
		{
			string curFilename = curFile.Filename;
			if (curFilename == "") { return; }

			// Launch File by item
			if (MPGUIMode)
			{
				curFile.UpdateLaunchInfo();
			}
			ProcessStartInfo procStart = new ProcessStartInfo();
			if (Filename != "")
			{
				// use the APPLICATION launcher and add current file information
				procStart.FileName = Filename; // filename of the application
				// set the arguments: one of the arguments is the fileitem-filename
				procStart.Arguments = " " + this.Arguments + " ";
				if (UseQuotes) 
				{
					// avoid double quotes around the filename-argument.....
					curFilename = "\"" + (curFile.Filename.TrimStart('\"')).TrimEnd('\"') + "\"";
				}

				if (procStart.Arguments.IndexOf("%FILEnoPATHnoEXT%") >= 0)
				{
					// ex. kawaks:
					// winkawaks.exe alpham2
					// => filename without path and extension is necessary!
					string strFileNoPathNoExt = curFile.ExtractFileName();
					strFileNoPathNoExt = (strFileNoPathNoExt.TrimStart('\"')).TrimEnd('\"');
					strFileNoPathNoExt = Path.GetFileNameWithoutExtension(strFileNoPathNoExt);
					procStart.Arguments = procStart.Arguments.Replace("%FILEnoPATHnoEXT%", strFileNoPathNoExt);
				}
				else
				{
					// the fileitem-argument can be positioned anywhere in the argument string...
					if (procStart.Arguments.IndexOf("%FILE%") == -1)
					{
						// no placeholder found => default handling: add the fileitem as the last argument
						procStart.Arguments = procStart.Arguments + curFilename;
					}
					else
					{
						// placeholder found => replace the placeholder by the correct filename
						procStart.Arguments = procStart.Arguments.Replace("%FILE%", curFilename);
					}
				}
				procStart.WorkingDirectory  = Startupdir;
				if (procStart.WorkingDirectory.IndexOf("%FILEDIR%") != -1)
				{
					procStart.WorkingDirectory = procStart.WorkingDirectory.Replace("%FILEDIR%", Path.GetDirectoryName(curFile.Filename));
				}
				procStart.UseShellExecute = UseShellExecute;
			}
			else 
			{
				// application has no launch-file 
				// => try to make a correct launch using the current FILE object
				string strCurFilename = curFile.ExtractFileName();
				procStart.FileName = strCurFilename;
				procStart.Arguments = curFile.ExtractArguments();
				curFilename = procStart.Arguments;
				procStart.WorkingDirectory  = curFile.ExtractDirectory(strCurFilename);
				procStart.UseShellExecute = UseShellExecute; 
			}
			procStart.WindowStyle = this.WindowStyle;


			//			bool bUseGenericPlayer = (Filename.ToUpper() == "%PLAY%") || 
			//				(Filename.ToUpper() == "%PLAYAUDIOSTREAM%") || 
			//				(Filename.ToUpper() == "%PLAYVIDEOSTREAM%");
			bool bUseGenericPlayer = (procStart.FileName.ToUpper() == "%PLAY%") || 
													  		(procStart.FileName.ToUpper() == "%PLAYAUDIOSTREAM%") || 
																(procStart.FileName.ToUpper() == "%PLAYVIDEOSTREAM%");
				
			this.LaunchErrorMsg = "";
			try
			{
				if (MPGUIMode)
				{
					AutoPlay.StopListening();
				}
				if (!bUseGenericPlayer)
				{
					Utils.StartProcess(procStart, WaitForExit);
					if (MPGUIMode) 
					{
						GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);
						AutoPlay.StartListening();
					}
				}
				else
				{
					// use generic player
					if (MPGUIMode)
					{
						LaunchGenericPlayer(procStart.FileName, curFilename);
						GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);
						AutoPlay.StartListening();
					}
					else
					{
						// generic player can only be used in MPGUI mode! 
						// => Apologize to the user :-)
						string ProblemString = "Sorry! The internal generic players cannot be used in Configuration. \nTry it in the MediaPortal application!";
						this.LaunchErrorMsg = ProblemString;
					}
				}

			}
			catch (Exception ex)
			{
				string ErrorString = String.Format("myPrograms: error launching program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
					procStart.FileName, 
					procStart.Arguments, 
					procStart.WorkingDirectory, 
					ex.Message, 
					ex.Source, 
					ex.StackTrace);
				Log.Write(ErrorString);
				this.LaunchErrorMsg = ErrorString;
			}
		}

		public virtual void LaunchFile(GUIListItem item)
		{
			// Launch File by GUILISTITEM
			// => look for FileItem and launch it using the found object
			if (item.MusicTag == null) {return;}
			FileItem curFile = (FileItem)item.MusicTag;
			if (curFile == null) { return;}
			this.LaunchFile(curFile, true);
		}

		protected virtual void LaunchFilelink(FilelinkItem curLink, bool MPGUIMode)
		{
			this.OnLaunchFilelink(curLink, MPGUIMode);
		}

		public virtual string DefaultFilepath()
		{
			return ""; // override this if the appitem can have subfolders
		}

		public virtual int DisplayFiles(string Filepath)
		{
			int Total = 0;
			if (Filepath != mLastFilepath)
			{
				Files.Load(this.AppID, Filepath);
				Filelinks.Load(this.AppID, Filepath);
			}
			Total = Total + DisplayArrayList(Filepath, this.Files);
			Total = Total + DisplayArrayList(Filepath, this.Filelinks);
			mLastFilepath = Filepath;
			return Total;
		}

		protected int DisplayArrayList(string Filepath, ArrayList dbItems)
		{
			int Total = 0;
			foreach(FileItem curFile in dbItems)
			{
				Total = Total + 1;
				GUIListItem gli = new GUIListItem( curFile.Title );
				gli.MusicTag = curFile;
				gli.IsFolder = curFile.IsFolder; 
				gli.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
				gli.OnItemSelected +=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnItemSelected);
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_VIEW,gli);
			}
			return Total;
		}


		void OnRetrieveCoverArt(GUIListItem gli)
		{
			if ((gli.MusicTag != null) && (gli.MusicTag is FileItem))
			{
				FileItem curFile = (FileItem)gli.MusicTag;
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
			}
		}

		private void OnItemSelected(GUIListItem item, GUIControl parent)
		{
			GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
			if (filmstrip == null) return;
			if (item == null) return;
			if ((item.MusicTag != null) && (item.MusicTag is FileItem) && (!item.IsFolder))
			{
				filmstrip.InfoImageFileName=item.ThumbnailImage;
			}
			else
			{
				filmstrip.InfoImageFileName="";
			}
		}

		
		public virtual void OnSort(GUIFacadeControl view, bool bDoSwitch)
		{
			if (!bFilesLoaded)
			{
				LoadFiles();
			} 

			if (bDoSwitch)
			{
				dbPc.updateState();
			}
			view.Sort(dbPc);
		}

		public virtual void OnSortToggle(GUIFacadeControl view)
		{
			dbPc.bAsc = (!dbPc.bAsc);
			view.Sort(dbPc);
		}

		public virtual int GetCurrentSortIndex()
		{
			return dbPc.currentSortMethodIndex;
		}

		public virtual void SetCurrentSortIndex(int newValue)
		{
			dbPc.currentSortMethodIndex = newValue;
		}

		public virtual string CurrentSortTitle()
		{
			return dbPc.currentSortMethodAsText;
		}

		public virtual bool GetCurrentSortIsAscending()
		{
			return dbPc.bAsc;
		}

		public virtual void SetCurrentSortIsAscending(bool newValue)
		{
			dbPc.bAsc = newValue;
		}

		public virtual bool RefreshButtonVisible()
		{
			return false; // otherwise, override this in child class
		}

		public virtual bool FileEditorAllowed()
		{
			return true;  // otherwise, override this in child class
		}

		public virtual bool FileAddAllowed()
		{
			return true;  // otherwise, override this in child class
		}

		public virtual bool FilesCanBeFavourites()
		{
			return true;  // otherwise, override this in child class
		}

		public virtual bool FileBrowseAllowed()
		{
			// set this to true, if SUBDIRECTORIES are allowed
			// (example: possible for DIRECTORY-CACHE)
			return false;  // otherwise, override this in child class
		}

		public virtual bool SubItemsAllowed()
		{
			return false;
		}

		public virtual bool ProfileLoadingAllowed()
		{
			return false;
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

		public int FatherID
		{
			get{ return mFatherID; }
			set{ mFatherID = value; }
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

		public int ContentID
		{
			get{ return mContentID; }
			set{ mContentID = value; }
		}

		public string SystemDefault
		{
			get{ return mSystemDefault; }
			set{ mSystemDefault = value; }
		}

		public bool WaitForExit
		{
			get{ return mWaitForExit; }
			set{ mWaitForExit = value; }
		}



		public bool GUIRefreshPossible
		{
			get{ return RefreshButtonVisible();}
		}

		public bool EnableGUIRefresh
		{
			get{ return mEnableGUIRefresh; }
			set{ mEnableGUIRefresh = value; }
		}

		public int Pincode
		{
			get{ return mPincode; }
			set{ mPincode = value; }
		}

		public string LaunchErrorMsg
		{
			get{ return mLaunchErrorMsg; }
			set{ mLaunchErrorMsg = value; }
		}


		public Filelist Files
		{
			// load on demand....
			get
			{
				if (!bFilesLoaded)
				{LoadFiles();} 
				return mFiles; 
			}
		}


		public FilelinkList Filelinks
		{
			// load on demand....
			get
			{
				if (!bLinksLoaded)
				{LoadFileLinks();} 
				return mFileLinks; 
			}
		}

	
		private int GetNewAppID()
		{
			if (m_db != null)
			{

				// won't work in multiuser environment :)
				SQLiteResultSet results;
				int res = 0;
				results = m_db.Execute("SELECT MAX(APPID) FROM application");
				ArrayList arr = (ArrayList)results.Rows[0];
				if (arr[0] != null)
				{
					res = Int32.Parse((string)arr[0]);
				}
				else
				{
					res = 0;
				}
				return res + 1;
			}
			else return -1;
		}

		private void Insert()
		{
			if (m_db != null)
			{
				try
				{
					this.AppID = GetNewAppID(); // important to avoid subsequent inserts!
					string strSQL = String.Format("insert into application (appid, fatherID, title, shorttitle, filename, arguments, windowstyle, startupdir, useshellexecute, usequotes, source_type, source, imagefile, filedirectory, imagedirectory, validextensions, importvalidimagesonly, position, enabled, enableGUIRefresh, GUIRefreshPossible, pincode, contentID, systemDefault, WaitForExit) values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{22}', '{23}', '{24}')", 
						AppID, FatherID, ProgramUtils.Encode(Title), ProgramUtils.Encode(ShortTitle), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Arguments), 
						ProgramUtils.WindowStyleToStr(WindowStyle), ProgramUtils.Encode(Startupdir), ProgramUtils.BooleanToStr(UseShellExecute), 
						ProgramUtils.BooleanToStr(UseQuotes), ProgramUtils.SourceTypeToStr(SourceType), ProgramUtils.Encode(Source), ProgramUtils.Encode(Imagefile), 
						ProgramUtils.Encode(FileDirectory), ProgramUtils.Encode(ImageDirectory), ProgramUtils.Encode(ValidExtensions), ProgramUtils.BooleanToStr(mImportValidImagesOnly), Position, 
						ProgramUtils.BooleanToStr(Enabled), ProgramUtils.BooleanToStr(EnableGUIRefresh), ProgramUtils.BooleanToStr(GUIRefreshPossible), Pincode,
						ContentID, ProgramUtils.Encode(SystemDefault), ProgramUtils.BooleanToStr(WaitForExit)
						);
					m_db.Execute(strSQL);
				}
				catch (SQLiteException ex) 
				{
					Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		private void Update()
		{
			string strSQL = "";
			if ((this.AppID >= 0) && (m_db != null))
			{
				try
				{
					strSQL = String.Format("update application set title = '{0}', shorttitle = '{1}', filename = '{2}', arguments = '{3}', windowstyle = '{4}', startupdir = '{5}', useshellexecute = '{6}', usequotes = '{7}', source_type = '{8}', source = '{9}', imagefile = '{10}',filedirectory = '{11}',imagedirectory = '{12}',validextensions = '{13}',importvalidimagesonly = '{14}',position = {15}, enabled = '{16}', fatherID = '{17}', enableGUIRefresh = '{18}', GUIRefreshPossible = '{19}', pincode = '{20}', contentID = '{21}', systemDefault = '{22}', WaitForExit = '{23}' where appID = {24}", 
						ProgramUtils.Encode(Title), ProgramUtils.Encode(ShortTitle), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Arguments), 
						ProgramUtils.WindowStyleToStr(WindowStyle), ProgramUtils.Encode(Startupdir), ProgramUtils.BooleanToStr(UseShellExecute), 
						ProgramUtils.BooleanToStr(UseQuotes), ProgramUtils.SourceTypeToStr(SourceType), ProgramUtils.Encode(Source), ProgramUtils.Encode(Imagefile), 
						ProgramUtils.Encode(FileDirectory), ProgramUtils.Encode(ImageDirectory), ProgramUtils.Encode(ValidExtensions), ProgramUtils.BooleanToStr(mImportValidImagesOnly), Position, 
						ProgramUtils.BooleanToStr(Enabled), FatherID, ProgramUtils.BooleanToStr(EnableGUIRefresh), ProgramUtils.BooleanToStr(GUIRefreshPossible),
						Pincode, ContentID, ProgramUtils.Encode(SystemDefault), ProgramUtils.BooleanToStr(WaitForExit),
						AppID);
					m_db.Execute(strSQL);
				}
				catch (SQLiteException ex) 
				{	
					Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Log.Write("sql \n{0}", strSQL);
				}
			}	
		}

		public void Delete()
		{	
			if ((this.AppID >= 0) && (m_db != null))
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
			if ((this.AppID >= 0) && (m_db != null))
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
		}


		public virtual void LoadFiles()
		{
			if (m_db != null)
			{

				// load Files and fill Files-arraylist here!
				if (mFiles == null) 
				{
					mFiles = new Filelist(m_db);}
				else 
				{ 
					mFiles.Clear();
				}
				mLastFilepath = "";
				mFiles.Load(AppID, "");
				bFilesLoaded = true;
			}
		}

		protected virtual void LoadFileLinks()
		{
			if (m_db != null)
			{
				if (mFileLinks == null) 
				{
					mFileLinks = new FilelinkList(m_db);}
				else 
				{ 
					mFileLinks.Clear();
				}
				mLastFilepath = "";
				mFileLinks.Load(AppID, "");
				bLinksLoaded = true;
			}
		}

		protected virtual void FixFileLinks()
		{
			// after a import the appitem has completely new
			// fileitems (new ids) and LINKS stored in filteritems
			// are out of sync... fix this here!

			// query with data to fix
			string SQLSelect = String.Format("select fi.appid, fi.fileid as oldfileid, f.fileid as newfileid, fi.filename as filename from filteritem fi, file f where fi.appID = f.appid and fi.filename = f.filename and fi.appID = {0}", this.AppID);
				
			// update command to fix one single link
			string SQLUpdate = "update filteritem set fileID = {0}, tag = 0 where appID = {1} and filename = '{2}'";
	
			SQLiteResultSet rows2fix;


			try
			{
				// 1) initialize TAG
				m_db.Execute(String.Format("update filteritem set tag = 1234 where appid = {0}", this.AppID));

				// 2) fix all fileids of the newly imported files
				rows2fix = m_db.Execute(SQLSelect);
				int nOldFileID;
				int nNewFileID;
				string strFilename;
				if (rows2fix.Rows.Count == 0)  return;
				for (int iRow=0; iRow < rows2fix.Rows.Count;iRow++)
				{
					nOldFileID = ProgramUtils.GetIntDef(rows2fix, iRow, "oldfileid", -1);
					nNewFileID = ProgramUtils.GetIntDef(rows2fix, iRow, "newfileid", -1);
					strFilename = ProgramUtils.Get(rows2fix, iRow, "filename");
					m_db.Execute(String.Format(SQLUpdate, nNewFileID, this.AppID, ProgramUtils.Encode(strFilename)));
				}

				// 3) delete untouched links ( they were not imported anymore )
				m_db.Execute(String.Format("delete from filteritem where appid = {0} and tag = 1234", this.AppID));

			}
			catch (SQLiteException ex) 
			{
				Log.Write("programdatabase exception (AppItem.FixFileLinks) err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
	
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

		public virtual string CurrentFilePath()
		{
			return this.FileDirectory;
		}



		public void Assign(AppItem sourceApp)
		{
			this.Enabled = sourceApp.Enabled;
			this.AppID = sourceApp.AppID;
			this.FatherID = sourceApp.FatherID;
			this.Title = sourceApp.Title;
			this.ShortTitle = sourceApp.ShortTitle;
			this.Filename = sourceApp.Filename;
			this.Arguments = sourceApp.Arguments;
			this.WindowStyle = sourceApp.WindowStyle;
			this.Startupdir = sourceApp.Startupdir;
			this.UseShellExecute = sourceApp.UseShellExecute;
			this.UseQuotes = sourceApp.UseQuotes;
			this.SourceType = sourceApp.SourceType;
			this.Source = sourceApp.Source;
			this.Imagefile = sourceApp.Imagefile;
			this.FileDirectory = sourceApp.FileDirectory;
			this.ImageDirectory = sourceApp.ImageDirectory;
			this.ValidExtensions = sourceApp.ValidExtensions;
			this.ImportValidImagesOnly = sourceApp.ImportValidImagesOnly;
			this.Position = sourceApp.Position;
			this.EnableGUIRefresh = sourceApp.EnableGUIRefresh;
			this.Pincode = sourceApp.Pincode;
			this.WaitForExit = sourceApp.WaitForExit;
			this.SystemDefault = sourceApp.SystemDefault;
			this.ContentID = sourceApp.ContentID;
		}

		public bool CheckPincode()
		{
			bool res = true;
			if (this.Pincode > 0)
			{
				res = false;
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
				GUIWindowManager.SendMessage(msg);
				int iPincode = -1;
				try
				{
					iPincode = Int32.Parse(msg.Label);
				}
				catch (Exception)
				{
					res = false;
				}
				res = (iPincode == this.Pincode);
			}
			return res;
		}

		// imagedirectory stuff
		// get next imagedirectory that holds at least one image for a fileitem
		// * m_pFile:       the file we're looking images for
		private void GetNextThumbFolderIndex(FileItem m_pFile)
		{
			if (m_pFile == null) return;
			bool bFound = false;
			while (!bFound)
			{
				mThumbFolderIndex++;
				if (mThumbFolderIndex >= ImageDirs.Length)
				{
					mThumbFolderIndex = -1;
					bFound = true;
				}
				else
				{
					string strCandFolder = ImageDirs[mThumbFolderIndex];
					string strCand = strCandFolder + "\\" + m_pFile.ExtractImageFileNoPath();
					if (strCand.ToLower() != m_pFile.Imagefile.ToLower())
					{
						bFound = (System.IO.File.Exists(strCand));
					}
					else
					{
						// skip the initial directory, in case it's reentered as a search directory!
						bFound = false;
					}
				}
			}
		}

		public string GetCurThumb(FileItem m_pFile)
		{
			string strThumb = "";
			if (mThumbFolderIndex == -1)
			{
				strThumb = m_pFile.Imagefile; 
			}
			else
			{
				string strFolder = ImageDirs[mThumbFolderIndex];
				strThumb = strFolder + "\\" + m_pFile.ExtractImageFileNoPath();
			}
			if (mThumbIndex > 0)
			{
				// try to find another thumb....
				// use the myGames convention:
				// every thumb has the postfix "_1", "_2", etc with the same file extension
				string strExtension = m_pFile.ExtractImageExtension();
				if (strThumb != "")
				{
					string strCand = strThumb.Replace(strExtension, "_" + mThumbIndex.ToString() + strExtension);
					if (System.IO.File.Exists(strCand))
					{
						// found another thumb => override the filename!
						strThumb = strCand;
					}
					else 
					{
						mThumbIndex = 0; // restart at the first thumb!
						GetNextThumbFolderIndex(m_pFile); 
					}
				}
			}
			return strThumb;
		}

		public void ResetThumbs()
		{
			mThumbIndex = 0;
			mThumbFolderIndex = -1;
		}

		public void NextThumb()
		{
			mThumbIndex++; 
		}


		public void LoadFromXmlProfile(XmlNode node)
		{
			XmlNode titleNode = node.SelectSingleNode("title");
			if (titleNode != null)
			{
				this.Title = titleNode.InnerText;
			}

			XmlNode launchingAppNode = node.SelectSingleNode("launchingApplication");
			if (launchingAppNode != null)
			{
				this.Filename = launchingAppNode.InnerText;
			}

			XmlNode useShellExecuteNode = node.SelectSingleNode("useShellExecute");
			if (useShellExecuteNode != null)
			{
				this.UseShellExecute = ProgramUtils.StrToBoolean(useShellExecuteNode.InnerText);
			}

			XmlNode argumentsNode = node.SelectSingleNode("arguments");
			if (argumentsNode != null)
			{
				this.Arguments = argumentsNode.InnerText;
			}

			XmlNode windowStyleNode = node.SelectSingleNode("windowStyle");
			if (windowStyleNode != null)
			{
				this.WindowStyle = ProgramUtils.StringToWindowStyle(windowStyleNode.InnerText);
			}

			XmlNode startupDirNode = node.SelectSingleNode("startupDir");
			if (startupDirNode != null)
			{
				this.Startupdir = startupDirNode.InnerText;
			}

			XmlNode useQuotesNode = node.SelectSingleNode("useQuotes");
			if (useQuotesNode != null)
			{
				this.UseQuotes = ProgramUtils.StrToBoolean(useQuotesNode.InnerText);
			}

			XmlNode fileExtensioneNode = node.SelectSingleNode("fileextensions");
			if (fileExtensioneNode != null)
			{
				this.ValidExtensions = fileExtensioneNode.InnerText;
			}
		}

	}

}
