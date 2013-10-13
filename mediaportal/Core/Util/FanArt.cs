#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
  /// <summary>
  /// FanArt grabber from TMDB (Online Movie Database).
  /// </summary>
  public class FanArt
  {
    private ArrayList _fanartList = new ArrayList();
    private string _fileFanArtDefault = string.Empty;
    private string _fileMovie = string.Empty;
    private string _fanartUrl = string.Empty;
    
    public int Count
    {
      get { return _fanartList.Count; }
    }

    public string DefaultFanartUrl
    {
      get { return _fanartUrl; }
    }

    public string this[int index]
    {
      get
      {
        if (index < 0 || index >= _fanartList.Count) return string.Empty;
        return (string)_fanartList[index];
      }
    }

    public string FanartTitleFile
    {
      get { return _fileFanArtDefault; }
    }

    public string FanartMovieFile
    {
      get { return _fileMovie; }
    }

    # region Public methods

    /// <summary>
    /// Use this to set local user fanart. Parameter title should begin with file:// or http://
    /// Parameter index is the fanart file index (from 0 to 4) ie. Fanart{0}.jpg -> index = 0
    /// </summary>
    /// <param name="movieId"></param>
    /// <param name="localFile"></param>
    /// <param name="index"></param>
    public void GetLocalFanart(int movieId, string localFile, int index)
    {
      if (localFile == string.Empty)
      {
        return;
      }
      bool isUrl = true;

      if (localFile.Length > 7 && localFile.Substring(0, 7).Equals("file://"))
      {
        localFile = localFile.Replace("file://", "");
        isUrl = false;
      }
      else if (localFile.Length > 7 && !localFile.Substring(0, 7).Equals("http://"))
      {
        return;
      }

      // Check FanArt Plugin directory in MP configuration folder (it will exists if FA plugin is installed)
      string configDir;
      GetFanArtFolder(out configDir);

      if (Directory.Exists(configDir))
      {
        string dbFile = SetFanArtFileName(movieId, index);

        if (!isUrl)
        {
          try
          {
            if (!localFile.Equals(dbFile, StringComparison.OrdinalIgnoreCase) && File.Exists(localFile))
            {
              DeleteFanart(movieId, index);
              File.Copy(localFile, dbFile, true);
              File.SetAttributes(dbFile, FileAttributes.Normal);
            }
          }
          catch (Exception ex)
          {
            Log.Error("GetlocalFanart (local file) error: {0}", ex.Message);
          }

        }
        else
        {
          try
          {
            DeleteFanart(movieId, index);
            var webClient = new WebClient();
            webClient.DownloadFile(localFile, dbFile);
            webClient.Dispose();
          }
          catch (Exception ex)
          {
            Log.Error("GetlocalFanart (url) error: {0}", ex.Message);
          }
        }

      }
    }

    /// <summary>
    /// Use this to find and set fanart from TMDB website using it's API. Parameter countFA is the wanted
    /// number of downloaded and saved fanart from founded movie fanart collection (max 5).
    /// If search is based on ImdbId tt number, title and search string can be string.Empty 
    /// </summary>
    /// <param name="movieId"></param>
    /// <param name="imdbTT"></param>
    /// <param name="title"></param>
    /// <param name="random"></param>
    /// <param name="countFA"></param>
    /// <param name="strSearch"></param>
    public void GetTmdbFanartByApi(int movieId, string imdbTT, string title, bool random, int countFA, string strSearch)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }
      
      _fanartList.Clear();
      
      try
      {
        _fanartList = InternalCSScriptGrabbersLoader.Movies.ImagesGrabber.MovieImagesGrabber.GetTmdbFanartByApi(
          movieId, imdbTT, title, random, countFA, strSearch, out _fileFanArtDefault, out _fanartUrl);
      }
      catch (Exception ex)
      {
        Log.Error("Util Fanart GetTmdbFanartByApi error: {0}", ex.Message);
      }
    }

    /// <summary>
    /// Use this to download fanart directly from TMDB with url parameter as TMDB fanart image location.
    /// Parameter index is the fanart file index (from 0 to 4) ie. Fanart{0}.jpg -> index = 0
    /// </summary>
    /// <param name="movieId"></param>
    /// <param name="url"></param>
    /// <param name="index"></param>
    public void GetTmdbFanartByUrl(int movieId, string url, int index)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      try
      {
        string configDir;
        GetFanArtFolder(out configDir);

        if (Directory.Exists(configDir))
        {
          string fileFanArt = SetFanArtFileName(movieId, index);

          DeleteFanart(movieId, index);

          WebClient webClient = new WebClient();
          webClient.DownloadFile(url, fileFanArt);
          webClient.Dispose();
        }
      }
      catch (Exception ex)
      {
        Log.Error("GetFanartByUrl error: {0}", ex.Message);
      }
      finally { }
    }

    // Helper funct to get saved fanart filename by movie title
    /// <summary>
    /// Checks and returns existing fullpath fanart filename. If fanart file not exists retruns "Unknown" string
    /// Searched fanart file should have filename by movie title.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="fileIndex"></param>
    /// <param name="fileart"></param>
    public static void GetFanArtfilename(string title, int fileIndex, out string fileart)
    {
      fileart = string.Empty;
      // FanArt directory
      string configDir;
      GetFanArtFolder(out configDir);

      title = MakeFanartFileName(title);

      if (Directory.Exists(configDir))
      {
        fileart = SetFanArtFileName(title, fileIndex);
        if (!File.Exists(fileart))
          fileart = "Unknown";
      }
    }
    
    // Helper funct to get saved fanart filename by movieId
    /// <summary>
    /// Checks and returns existing fullpath fanart filename. If fanart file not exists retruns "Unknown" string
    /// Searched fanart file should have filename by movieId.
    /// </summary>
    /// <param name="movieId"></param>
    /// <param name="fileIndex"></param>
    /// <param name="fileart"></param>
    public static void GetFanArtfilename(int movieId, int fileIndex, out string fileart)
    {
      fileart = string.Empty;
      // FanArt directory
      string configDir;
      GetFanArtFolder(out configDir);

      if (Directory.Exists(configDir))
      {
        fileart = SetFanArtFileName(movieId, fileIndex);
        if (!File.Exists(fileart))
          fileart = "Unknown";
      }
    }

    // Helper funct to delete covers
    public static void DeleteCovers(string title, int id)
    {
      if (title == string.Empty || id < 0)
      {
        return;
      }
      string titleExt = title + "{" + id + "}";
      string file = Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
      Util.Utils.FileDelete(file);

      file = Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
      Util.Utils.FileDelete(file);
    }

    // Helper funct to delete fanarts
    public static void DeleteFanarts(int movieId)
    {
      if (movieId == -1)
        return;

      int fanartQ = 5;
      string configDir;

      GetFanArtFolder(out configDir);

      for (int i = 0; i < fanartQ; i++)
      {
        string file = SetFanArtFileName(movieId, i);
        Util.Utils.FileDelete(file);
      }
    }

    public static void DeleteFanart(int movieId, int index)
    {
      if (movieId == -1)
        return;

      string configDir;
      GetFanArtFolder(out configDir);

      string file = SetFanArtFileName(movieId, index);
      Util.Utils.FileDelete(file);
    }

    // Returns default MP fanart folder
    public static void GetFanArtFolder(out string fanartFolder)
    {
      // FanArt directory
      fanartFolder = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\Movies\";
    }

    // Set fanart filename - by title
    /// <summary>
    /// Returns full path fanart filename by movie title.
    /// Use for set fanart file name.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string SetFanArtFileName(string title, int index)
    {
      title = MakeFanartFileName(title);

      string configDir = string.Empty;
      GetFanArtFolder(out configDir);
      string ext = ".jpg";
      //
      return configDir + title + "{" + index + "}" + ext;
    }

    // Set fanart filename - by movieId
    /// <summary>
    /// Returns full path fanart filename by MP movieId
    /// </summary>
    /// <param name="movieId"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string SetFanArtFileName(int movieId, int index)
    {
      string configDir = string.Empty;
      GetFanArtFolder(out configDir);
      string ext = ".jpg";
      //
      return configDir + movieId + "{" + index + "}" + ext;
    }

    /// <summary>
    /// Cleans string from forbidden filename characters and replaces them with space tring
    /// </summary>
    /// <param name="strText"></param>
    /// <returns></returns>
    public static string MakeFanartFileName(string strText)
    {
      if (strText == null) return string.Empty;
      if (strText.Length == 0) return string.Empty;

      string strFName = strText.Replace(':', ' ');
      strFName = strFName.Replace('/', ' ');
      strFName = strFName.Replace('\\', ' ');
      strFName = strFName.Replace('*', ' ');
      strFName = strFName.Replace('?', ' ');
      strFName = strFName.Replace('\"', ' ');
      strFName = strFName.Replace('<', ' ');
      strFName = strFName.Replace('>', ' ');
      strFName = strFName.Replace('|', ' ');

      bool unclean = true;
      char[] invalids = Path.GetInvalidFileNameChars();
      while (unclean)
      {
        unclean = false;

        char[] filechars = strFName.ToCharArray();

        foreach (char c in filechars)
        {
          if (!unclean)
            foreach (char i in invalids)
            {
              if (c == i)
              {
                unclean = true;
                strFName = strFName.Replace(c, ' ');
                break;
              }
            }
        }
      }
      return strFName;
    }

    public static string GetRandomFanartFileName(int movieId)
    {
      string fanart = string.Empty;

      if (movieId > 0)
      {
        ArrayList fanarts = new ArrayList();

        for (int i = 0; i < 5; i++)
        {
          GetFanArtfilename(movieId, i, out fanart);

          if (File.Exists(fanart))
          {
            fanarts.Add(fanart);
          }
        }

        if (fanarts.Count > 0)
        {
          Random rnd = new Random();
          int r = rnd.Next(fanarts.Count);
          fanart = fanarts[r].ToString();
        }
      }

      return fanart;
    }

    #endregion

  }
}