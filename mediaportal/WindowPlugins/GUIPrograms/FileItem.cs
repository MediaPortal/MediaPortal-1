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
	public class FileItem
	{
		protected static SQLiteClient m_db=null;

		int mFileID;
		int mAppID;
		string mTitle;
		string mFilename;
		string mFilepath;
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


		public virtual void Clear()
		{
			mFileID = -1;
			mAppID = -1;
			mTitle = "";
			mFilename = "";
			mFilepath = "";
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
		public string Filepath
		{
			get{ return mFilepath; }
			set{ mFilepath = value; }
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
		public string System_
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
				string strSQL = String.Format("insert into file (fileid, appid, title, filename, filepath, imagefile, genre, country, manufacturer, year, rating, overview, system, manualfilename, lastTimeLaunched, launchcount, isfolder, external_id, uppertitle) values (null, '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}')", 
					AppID, ProgramUtils.Encode(Title), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Filepath), ProgramUtils.Encode(Imagefile), ProgramUtils.Encode(Genre), Country, ProgramUtils.Encode(Manufacturer), strYear, Rating, ProgramUtils.Encode(Overview), ProgramUtils.Encode(System_), 
					ProgramUtils.Encode(ManualFilename), strLastLaunch, strLaunchCount, ProgramUtils.BooleanToStr(IsFolder), ExtFileID, ProgramUtils.Encode(Title.ToUpper()));
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
					"update file set title = '{1}', filename = '{2}', filepath = '{3}', imagefile = '{4}', genre = '{5}', country = '{6}', manufacturer = '{7}', year = '{8}', rating = '{9}', overview = '{10}', system = '{11}', uppertitle = '{12}' where  fileid = {0}",					
					FileID, 
					ProgramUtils.Encode(Title),
					ProgramUtils.Encode(Filename),
					ProgramUtils.Encode(Filepath),
					ProgramUtils.Encode(Imagefile),
					ProgramUtils.Encode(Genre),
					ProgramUtils.Encode(Country),
					ProgramUtils.Encode(Manufacturer),
					strYear,
					Rating,
					ProgramUtils.Encode(Overview),
					ProgramUtils.Encode(System_),
					ProgramUtils.Encode(Title.ToUpper())
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

		public virtual void Write()
		{
			//no! quotes  or composite filenames will fuck up in getdirectoryname.......
			//			if (mFilepath == "")
			//			{
			//				mFilepath = System.IO.Path.GetDirectoryName(mFilename);
			//			}
			if (mFileID == -1)	
			{Insert();}
			else 
			{Update();}
		}

		public virtual void Delete()
		{	
			if (this.FileID >= 0)
			{
				try
				{
					string strSQL1 = String.Format("delete from fileitem where fileid = {0}", this.FileID);
					string strSQL2 = String.Format("delete from file where fileid = {0}", this.FileID);
					m_db.Execute(strSQL1);
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
