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
    private string _fileFanArt = string.Empty;
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
            }
          }
          catch (Exception ex)
          {
            Log.Error(ex);
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
          catch (Exception) { }
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

      string[] vdbParserStr = VdbParserString();

      if (vdbParserStr == null || vdbParserStr.Length != 6)
      {
        return;
      }

      try
      {
        string strAbsUrl = string.Empty;
        string tmdbUrl = string.Empty; // TMDB Fanart api URL
        string tmdbUrlWorkaround = string.Empty; // Sometimes link conatains double "/" before ttnumber
        // Firts try by IMDB id (100% accurate) then, if fail, by movie name (first result will be taken as defult fanart, no random)
        if ( imdbTT != string.Empty && imdbTT.StartsWith("tt"))
        {
          //tmdbUrl = "http://api.themoviedb.org/2.1/Movie.imdbLookup/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" +
          //          imdbTT;
          tmdbUrl = vdbParserStr[0] + imdbTT;
          //tmdbUrlWorkaround =
          //  "http://api.themoviedb.org/2.1/Movie.imdbLookup/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a//" +
          //  imdbTT;
          tmdbUrlWorkaround = vdbParserStr[1] + imdbTT;
        }
        else
        {
          if (strSearch == string.Empty)
          {
            //tmdbUrl = "http://api.themoviedb.org/2.1/Movie.search/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" +
            //          title;
            tmdbUrl = vdbParserStr[2] + title;
            //tmdbUrlWorkaround = "http://api.themoviedb.org/2.1/Movie.search/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a//" +
            //                    title;
            tmdbUrlWorkaround = vdbParserStr[3] + title;
          }
          else
          {
            //tmdbUrl = "http://api.themoviedb.org/2.1/Movie.search/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" +
            //          strSearch;
            tmdbUrl = vdbParserStr[2] +strSearch;
            //tmdbUrlWorkaround = "http://api.themoviedb.org/2.1/Movie.search/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a//" +
            //                    strSearch;
            tmdbUrlWorkaround = vdbParserStr[3] + strSearch;
          }
          random = false;
        }
        // Download fanart xml 
        string tmdbXml = string.Empty;
        if (!GetPage(tmdbUrl, "utf-8", out strAbsUrl, ref tmdbXml))
        {
          if (!GetPage(tmdbUrlWorkaround, "utf-8", out strAbsUrl, ref tmdbXml))
          {
            Log.Info("Fanart Serach: TMDB returns no API result for - {0} ({1})", title, tmdbUrl);
            return;
          }
        }

        //string matchBackdrop = "<image\\stype=\"backdrop\"\\surl=\"(?<BackDrop>.*?)\"";
        string matchBackdrop = vdbParserStr[4];

        // Check FanArt Plugin directory in MP configuration folder (it will exists if FA plugin is installed)
        string configDir;
        GetFanArtFolder(out configDir);
        // Check if FanArt directory Exists
        if (Directory.Exists(configDir))
        {
          MatchCollection mcBd = Regex.Matches(tmdbXml, matchBackdrop);
          // Set fanart collection
          if (mcBd.Count != 0)
          {
            foreach (Match mBd in mcBd)
            {
              string strBd = string.Empty;
              strBd = mBd.Groups["BackDrop"].Value;
              
              if (strBd.Contains(vdbParserStr[5]))
              {
                // Hack the extension cause many files shows as png but in reality on TMDB they are jpg (bug in API)
                int extfile = 0;
                extfile = strBd.LastIndexOf(".");
                strBd = strBd.Remove(extfile) + ".jpg";
                _fanartList.Add(strBd);
              }
            }
          }
          else
          {
            Log.Info("Fanart Serach: No fanart found for - {0} ({1})", title, tmdbUrl);
            return;
          }
          // Check if fanart collection is lower than wanted fanarts quantity per movie
          if (_fanartList.Count < countFA)
            countFA = _fanartList.Count;

          if (_fanartList.Count > 0)
          {
            // Delete old FA
            DeleteFanarts(movieId);

            if (countFA == 1) //Only one fanart found
            {
              DownloadFanart(movieId, 0);
            }
            else //Get max 5 fanart per movie
            {
              //Randomize order of fanarts in array
              if (_fanartList.Count > countFA && random)
                ShuffleFanart(ref _fanartList);

              _fileFanArtDefault = SetFanArtFileName(movieId, 0);

              for (int i = 0; i < countFA; i++)
              {
                DownloadFanart(movieId, i);
              }
            }
          }
        }
      }
      catch (Exception) { }
      finally { }
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
          _fileFanArt = SetFanArtFileName(movieId, index);

          DeleteFanart(movieId, index);

          WebClient webClient = new WebClient();
          webClient.DownloadFile(url, _fileFanArt);
          webClient.Dispose();
        }
      }
      catch (Exception) { }
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
      DeleteFile(file);

      file = Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
      DeleteFile(file);
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
        DeleteFile(file);
      }
    }

    public static void DeleteFanart(int movieId, int index)
    {
      if (movieId == -1)
        return;

      string configDir;
      GetFanArtFolder(out configDir);

      string file = SetFanArtFileName(movieId, index);
      DeleteFile(file);
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
                //Log.Warn("Utils: *** File name {1} still contains invalid chars - {0}", Convert.ToString(c), strFName);
                strFName = strFName.Replace(c, ' ');
                break;
              }
            }
        }
      }
      return strFName;
    }

    private string[] VdbParserString()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("FanArt");
      return vdbParserStr;
    }

    #endregion

    #region Private Methods

    // Download and save fanart
    private void DownloadFanart(int movieId, int index)
    {
      try
      {
        _fileFanArt = SetFanArtFileName(movieId, index);
        var webClient = new WebClient();
        webClient.DownloadFile((string)_fanartList[index], _fileFanArt);
        _fanartUrl = _fanartList[0].ToString();
        webClient.Dispose();
      }
      catch (Exception) { }
    }

    // Randomize fanart array list
    private void ShuffleFanart(ref ArrayList faArray)
    {
      Random rnd = new Random();
      for (int i = faArray.Count - 1; i > 0; i--)
      {
        int position = rnd.Next(i + 1);
        object temp = faArray[i];
        faArray[i] = faArray[position];
        faArray[position] = temp;
      }
    }

    private static void DeleteFile(string file)
    {
      if (File.Exists(file))
      {
        File.Delete(file);
      }
    }

    // Get URL HTML body
    private bool GetPage(string strUrl, string strEncode, out string absoluteUri, ref string strBody)
    {
      bool sucess = true;
      absoluteUri = String.Empty;
      Stream receiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        //Log.Info("IMDB: get page:{0}", strURL);
        WebRequest req = WebRequest.Create(strUrl);
        req.Timeout = 20000;
        result = req.GetResponse();
        receiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = Encoding.GetEncoding(strEncode);
        sr = new StreamReader(receiveStream, encode);
        strBody = sr.ReadToEnd();

        absoluteUri = result.ResponseUri.AbsoluteUri;
      }
      catch (Exception)
      {
        sucess = false;
      }
      finally
      {
        if (sr != null)
        {
          try
          {
            sr.Close();
          }
          catch (Exception) { }
        }
        if (receiveStream != null)
        {
          try
          {
            receiveStream.Close();
          }
          catch (Exception) { }
        }
        if (result != null)
        {
          try
          {
            result.Close();
          }
          catch (Exception) { }
        }
      }
      return sucess;
    }

    #endregion
  }
}