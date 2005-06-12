using System.Collections;
using System.Diagnostics;
using System.IO;
using MediaPortal.Util;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for FileInfo.
  /// </summary>
  public class FileInfo
  {

    string mRelevance;
    int mRelevanceNorm;
    string mYear;
    string mGameURL;
    string mTitle;
    string mGenre;
    string mGenre2;
    string mGenre3;
    string mGenre4;
    string mGenre5;
    string mStyle;
    string mPlatform;
    string mRating;
    int mRatingNorm;
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
      mRelevanceNorm = 0;
      mYear = "";
      mGameURL = "";
      mTitle = "";
      mGenre = "";
      mGenre2 = "";
      mGenre3 = "";
      mGenre4 = "";
      mGenre5 = "";
      mStyle = "";
      mPlatform = "";
      mRating = "";
      mRatingNorm = 0;
      mImageURLs = "";
      mManufacturer = "";
      mOverview = "";
      bLoaded = false;

    }
    public string RelevanceOrig
    {
      get
      {
        return mRelevance;
      }
      set
      {
        mRelevance = value;
      }
    }

    public int RelevanceNorm
    {
      get
      {
        return mRelevanceNorm;
      }
      set
      {
        mRelevanceNorm = value;
      }
    }

    public string Year
    {
      get
      {
        return mYear;
      }
      set
      {
        mYear = value;
      }
    }

    public string GameURL
    {
      get
      {
        return mGameURL;
      }
      set
      {
        mGameURL = value;
      }
    }

    public string GameURLPostParams
    {
      get
      {
        return GetGameURLPostParams();
      }
    }

    public string Title
    {
      get
      {
        return mTitle;
      }
      set
      {
        mTitle = value;
      }
    }

    public string Genre
    {
      get
      {
        return mGenre;
      }
      set
      {
        mGenre = value;
      }
    }

    public string Genre2
    {
      get
      {
        return mGenre2;
      }
      set
      {
        mGenre2 = value;
      }
    }

    public string Genre3
    {
      get
      {
        return mGenre3;
      }
      set
      {
        mGenre3 = value;
      }
    }

    public string Genre4
    {
      get
      {
        return mGenre4;
      }
      set
      {
        mGenre4 = value;
      }
    }

    public string Genre5
    {
      get
      {
        return mGenre5;
      }
      set
      {
        mGenre5 = value;
      }
    }

    public string Style
    {
      get
      {
        return mStyle;
      }
      set
      {
        mStyle = value;
      }
    }

    public string Platform
    {
      get
      {
        return mPlatform;
      }
      set
      {
        mPlatform = value;
      }
    }

    public string RatingOrig
    {
      get
      {
        return mRating;
      }
      set
      {
        mRating = value;
      }
    }

    public int RatingNorm
    {
      get
      {
        return mRatingNorm;
      }
      set
      {
        mRatingNorm = value;
      }
    }

    public string Manufacturer
    {
      get
      {
        return mManufacturer;
      }
      set
      {
        mManufacturer = value;
      }
    }

    public string Overview
    {
      get
      {
        return mOverview;
      }
      set
      {
        mOverview = value;
      }
    }

    public string ImageURLs
    {
      get
      {
        return mImageURLs;
      }
      set
      {
        mImageURLs = value;
      }
    }

    public bool Loaded
    {
      get
      {
        return bLoaded;
      }
      set
      {
        bLoaded = value;
      }
    }

    public void LaunchURL()
    {
      if (GameURL == null)
        return ;
      if (GameURL.Length > 0)
      {
        string strGameURL = GameURL.Replace("|", "");
        ProcessStartInfo sInfo = new ProcessStartInfo(strGameURL);
        Process.Start(sInfo);
        //				System.Diagnostics.Process.Start("iexplore.exe", GameURL);
      }
    }

    public string GetGameURLPostParams()
    {
      string result = "";
      int iPos = mGameURL.IndexOf('?');
      if (iPos < 0)
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
      if (curFile == null)
        return ;
      ArrayList mImgUrls = new ArrayList(this.ImageURLs.Split('\n'));
      int i = 0;
      string strFile = "";

      // delete images from this fileitem
      curFile.DeleteImages(curApp);
      curFile.Imagefile = "";

      // download all images
      foreach (string strImgUrl in mImgUrls)
      {
        // strImgUrl contains a full URL with one picture to download

        i++;
        strFile = curFile.GetNewImageFile(curApp, Path.GetExtension(strImgUrl));
        Utils.DownLoadImage(strImgUrl, strFile);
        if ((File.Exists(strFile)) && (curFile.Imagefile == ""))
        {
          // download successful
          // make sure the first found pic is the imagefile...
          curFile.Imagefile = strFile;
        }
      }
    }
  }
}
