#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Utils.Web;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for FileInfo.
  /// </summary>
  public class FileInfo : IParserData
  {
    #region Variables
    private string mRelevance = string.Empty;
    private int mRelevanceNorm = 0;
    private string mTitle = string.Empty;
    private string mYear = string.Empty;
    private string mGameURL = string.Empty;
    private string mGenre = string.Empty;
    private string mGenre2 = string.Empty;
    private string mGenre3 = string.Empty;
    private string mGenre4 = string.Empty;
    private string mGenre5 = string.Empty;
    private string mStyle = string.Empty;
    private string mPlatform = string.Empty;
    private string mRating = string.Empty;
    private int mRatingNorm = 0;
    private string mImageURLs = string.Empty;
    private string mManufacturer = string.Empty;
    private string mOverview = string.Empty;
    private bool bLoaded = false;
    #endregion

    #region Constructors/Destructors
    public FileInfo()
    {
    }
    #endregion

    #region Properties
    // Public Properties
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
    #endregion

    #region Public Methods
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
        MediaPortal.Util.Utils.DownLoadImage(strImgUrl, strFile);
        if ((File.Exists(strFile)) && (curFile.Imagefile == ""))
        {
          // download successful
          // make sure the first found pic is the imagefile...
          curFile.Imagefile = strFile;
        }
      }
    }
    #endregion

    #region Private Methods
    #endregion

    #region IParserData Member

    public void SetElement(string tag, string value)
    {
      try
      {
        switch (tag)
        {
          case "#RELEVANCE":
            RelevanceOrig = value;
            RelevanceNorm = GetNumber(value);
            break;
          case "#TITLE":
            Title = value.Trim(' ', '\n', '\t');
            break;
          case "#URL":
            GameURL = value.Trim(' ', '\n', '\t');
            break;
          case "#YEAR":
            Year = value.Trim(' ', '\n', '\t');
            break;
          case "#GENRE":
            Genre = value.Trim(' ', '\n', '\t');
            break;
          case "#STYLE":
            Style = value.Trim(' ', '\n', '\t');
            break;
          case "#PLATFORM":
            Platform = value.Trim(' ', '\n', '\t');
            break;
          case "#OVERVIEW":
            Overview = value.Trim(' ', '\n', '\t');
            break;
          case "#DEVELOPER":
            Manufacturer = value.Trim(' ', '\n', '\t');
            break;
          case "#PUBLISHER":
            //Manufacturer = value.Trim(' ', '\n', '\t');
            break;
          default:
            break;
        }
      }
      catch (Exception)
      {
        Log.Error("MyPrograms: Parsing error {0} : {1}", tag, value);
      }
    }

    public int GetNumber(string value)
    {
      string number = string.Empty;
      int numberValue;
      bool found = false;

      for (int i = 0; i < value.Length; i++)
      {
        if (!found)
        {
          if (Char.IsDigit(value[i]))
          {
            number += value[i];
            found = true;
          }
        }
        else
        {
          if (Char.IsDigit(value[i]))
          {
            number += value[i];
          }
          else
          {
            break;
          }
        }
      }

      try
      {
        numberValue = Int32.Parse(number);
      }
      catch (Exception)
      {
        numberValue = 0;
      }

      return numberValue;
    }

    #endregion
  }
}
