using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using MediaPortal.Util;
using MediaPortal.GUI.Library;		

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for FileInfo.
	/// </summary>
	public class FileInfo
	{

		string mRelevance;
		string mYear;
		string mGameURL;
		string mTitle;
		string mGenre;
		string mStyle;
		string mPlatform;
		string mRating;
		string mImageURLs;
		string mManufacturer;
		string mOverview;
		bool bLoaded;

		public FileInfo()
		{
			//
			// TODO: Add constructor logic here
			//
			mRelevance = "";
			mYear = "";
			mGameURL = "";
			mTitle = "";
			mGenre = "";
			mStyle = "";
			mPlatform = "";
			mRating = "";
			mImageURLs = "";
			mManufacturer = "";
			mOverview = "";
			bLoaded = false;

		}
		public string Relevance
		{
			get{ return mRelevance; }
			set{ mRelevance = value; }
		}

		public string Year
		{
			get{ return mYear; }
			set{ mYear = value; }
		}

		public string GameURL
		{
			get{ return mGameURL; }
			set{ mGameURL = value; }
		}

		public string GameURLPostParams
		{
			get { return GetGameURLPostParams();}
		}

		public string Title
		{
			get{ return mTitle; }
			set{ mTitle = value; }
		}
		
		public string Genre
		{
			get{ return mGenre; }
			set{ mGenre = value; }
		}
		
		public string Style
		{
			get{ return mStyle; }
			set{ mStyle = value; }
		}
		
		public string Platform
		{
			get{ return mPlatform; }
			set{ mPlatform = value; }
		}
		
		public string Rating
		{
			get{ return mRating; }
			set{ mRating = value; }
		}

		public string Manufacturer
		{
			get { return mManufacturer; }
			set { mManufacturer = value; }
		}

		public string Overview
		{
			get { return mOverview; }
			set { mOverview = value; }
		}

		public string ImageURLs
		{
			get { return mImageURLs; }
			set { mImageURLs = value; }
		}
		
		public bool Loaded
		{
			get{ return bLoaded; }
			set{ bLoaded = value; }
		}

		public void LaunchURL()
		{
			if(GameURL==null)
				return;
			if(GameURL.Length>0)
			{
// this code doesn't work with firefox !?! (launches a 6-tab browser window)
//				System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(GameURL);
//				System.Diagnostics.Process.Start(sInfo);
				System.Diagnostics.Process.Start("iexplore.exe", GameURL);
			}
		}

		public string GetGameURLPostParams()
		{
			string result = "";
			int iPos = mGameURL.IndexOf('?');
			if (iPos< 0) 
			{
				result = "";
			}
			else
			{
				result = mGameURL.Substring(iPos + 1);
			}
			return result;
		}

		public void AddImageURL(string strURL)
		{
			if (mImageURLs != "") 
			{
				mImageURLs = mImageURLs + "\n" + strURL;
			}
			else
			{
				mImageURLs = strURL;
			}
		}

		public void DownloadImages(AppItem curApp, FileItem curFile)
		{
			if (curFile == null) return;
			ArrayList mImgUrls = new ArrayList( this.ImageURLs.Split('\n'));
			int i = 0;
			string strFile = "";

			// delete images from this fileitem
			curFile.DeleteImages(curApp);
			curFile.Imagefile = "";

			// download all images
			foreach(string strImgUrl in mImgUrls)
			{
				// strImgUrl contains a full URL with one picture to download

				i++;
				strFile = curFile.GetNewImageFile(curApp, Path.GetExtension(strImgUrl));
				Utils.DownLoadImage(strImgUrl, strFile);
				if ((System.IO.File.Exists(strFile)) && (curFile.Imagefile == ""))
				{
					// download successful
					// make sure the first found pic is the imagefile...
					curFile.Imagefile = strFile;
				}
			}
		}
	}
}
