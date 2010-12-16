#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.Profile;

namespace MediaPortal.Util
{
  /// <summary>
  /// FanArt grabber from TMDB (Online Movie Database).
  /// </summary>
  public class FanArt
  {
    private ArrayList _fanartList = new ArrayList();
    private string _fileArt = "";
    private string _fileArtDefault = "";
    private string _fileMovie = "";
    private string _fanartURL = "";
       
    public int Count
    {
      get { return _fanartList.Count; }
    }

    public string DefaultFanartURL
    {
      get { return _fanartURL; }
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
      get { return _fileArtDefault; }
    }

    public string FanartMovieFile
    {
      get { return _fileMovie; }
    }

    /// <summary>
    /// Use this to set local fanart. Parameter title should begin with file:// or http://
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="title"></param>
    /// <param name="localFile"></param>
    /// <param name="index"></param>
    /// <param name="share"></param>
    public void GetLocalFanart(string path, string filename, string title, string localFile, int index, bool share) 
    {
      if (localFile == string.Empty)
      {
        return;
      }
      bool isUrl = true;
      string ext = localFile.Substring(localFile.LastIndexOf(".")); // url or local file extension

      filename = Path.GetFileNameWithoutExtension(filename);
      filename = Utils.RemoveTrailingSlash(filename);
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
      // Check if FanArt directory Exists
      if (Directory.Exists(configDir))
      {
        string dbFile = configDir + title + " " + index + ext;
        //
        // DB view file
        //
        if (isUrl == false)
        {
          File.Copy(localFile, dbFile, true);
        }
        else
        {
          try
          {
            var webClient = new WebClient();
            webClient.DownloadFile(localFile, dbFile);
            webClient.Dispose();
          }
          catch (Exception){}
        }
        //
        // Share view file
        //
         if (share) // If not DVD video
        {
          if (!filename.ToUpper().Contains("VIDEO_TS"))
          {
            string fileclean = GetFileclean(filename);
            _fileMovie = configDir + fileclean + " " + index + ext;
            if (dbFile.ToLower() != _fileMovie.ToLower())
            {
              File.Copy(dbFile, _fileMovie, true);
            }
          }
          else // DVD Video
          {
            SetDVD_fileMovie(configDir, path, index, ext);
            if (dbFile.ToLower() != _fileMovie.ToLower())
            {
              File.Copy(dbFile, _fileMovie, true);
            }
          }
        }
      }
    }

    /// <summary>
    /// Use this to find and set fanart from TMDB website. Parameter countFA is the number of downloaded
    /// and saved fanart from founded collection of movie fanart.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="imdbTT"></param>
    /// <param name="title"></param>
    /// <param name="random"></param>
    /// <param name="countFA"></param>
    /// <param name="share"></param>
    public void GetWebFanart(string path, string filename, string imdbTT, string title, bool random, int countFA, bool share) 
    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      filename = Path.GetFileNameWithoutExtension(filename);
      filename = Utils.RemoveTrailingSlash(filename);

      _fanartList.Clear();

      try
      {
        string strAbsUrl = "";
        string tmdbUrl = ""; // TMDB Fanart api URL
        // Firts try by IMDB id (100% accurate) then, if fail, by movie name (first result will be taken as defult fanart, no random)
        if (imdbTT != string.Empty && imdbTT.StartsWith("tt"))
        {
          tmdbUrl = "http://api.themoviedb.org/2.1/Movie.imdbLookup/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" +
                    imdbTT;
        }
        else
        {
          tmdbUrl = "http://api.themoviedb.org/2.1/Movie.search/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" +
                    title;
          random = false;
        }
        // Download fanart xml 
        string tmdbXml = GetPage(tmdbUrl, "utf-8", out strAbsUrl);

        string matchBackdrop = "<image\\stype=\"backdrop\"\\surl=\"(?<BackDrop>.*?)\"";

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
              if (strBd.Contains("original"))
              {
                // Hack the extension cause many files shows as png but in reality on TMDB they are jpg (bug in API)
                int extfile = 0;
                extfile = strBd.LastIndexOf(".");
                strBd = strBd.Remove(extfile) + ".jpg";
                _fanartList.Add(strBd);
              }
            }
          }
          // Exception prevention if fanart collection is lower than wanted fanarts q per movie
          if (_fanartList.Count < countFA)
            countFA = _fanartList.Count;
            
          if (_fanartList.Count > 0)
          {
            if (countFA == 1)
            {
              string tempFile = (string)_fanartList[0];

              if (random)
              {
                var r = new Random();
                int rnd = r.Next(_fanartList.Count);
                tempFile = (string)_fanartList[rnd];
              }
                
              // Temporary only jpg but maybe later more formats so lets prepare it
              string ext = tempFile.Substring(tempFile.LastIndexOf("."));
              // We need two backdrop files for different views in GUI
              // 1st is named as movie title for database view (ie. year, actor, title..)
              // 2nd is named as movie filename without disk number for share view

              // Title named fanart for database View in GUI
              DownloadFA(title, configDir, 0, ext);

              // File name Backdrop for SHARES view

              // If not DVD video
              if (share)
              {
                if (!filename.ToUpper().Contains("VIDEO_TS"))
                {
                  string fileclean = GetFileclean(filename);
                  _fileMovie = configDir + fileclean + " 0" + ext;
                  if (!_fileMovie.Equals(_fileArt, StringComparison.OrdinalIgnoreCase))
                    File.Copy(_fileArt, _fileMovie, true);
                }
                  // DVD Video
                else
                {
                  SetDVD_fileMovie(configDir, path, 0, ext);
                  if (!_fileMovie.Equals(_fileArt, StringComparison.OrdinalIgnoreCase))
                    File.Copy(_fileArt, _fileMovie, true);
                }
              }
            }
            else
            {
              //Randomize order of fanarts in array
              if (_fanartList.Count > countFA && random)
                ShuffleFanart(ref _fanartList);

              _fileArtDefault = configDir + title + " 0" + ".jpg";

              for (int i = 0; i < countFA; i++)
              {
                var tempFile = (string)_fanartList[i];
                string ext = tempFile.Substring(tempFile.LastIndexOf("."));
                  
                // Database view
                DownloadFA(title, configDir, i, ext);

                // Share view
                if (share)
                {
                  // Regular video
                  if (!filename.ToUpper().Contains("VIDEO_TS"))
                  {
                    string fileclean = filename;
                    var pattern = Utils.StackExpression();
                    foreach (var t in pattern)
                    {
                      if (t.IsMatch(filename))
                      {
                        fileclean = t.Replace(filename, "");
                      }
                    }
                    _fileMovie = configDir + fileclean + " " + i + ext;
                    if (!_fileMovie.Equals(_fileArt, StringComparison.OrdinalIgnoreCase))
                      File.Copy(_fileArt, _fileMovie, true);
                  }
                    // DVD video
                  else
                  {
                    SetDVD_fileMovie(configDir, path, i, ext);
                    if (!_fileMovie.Equals(_fileArt, StringComparison.OrdinalIgnoreCase))
                      File.Copy(_fileArt, _fileMovie, true);
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception) {}
      finally {}
    }

    /// <summary>
    /// Use this to download fanart directly from TMDB with url parameter as TMDB fanart image location.
    /// Parameter faPosition is the fanart file index ie. Fanart0.jpg -> faPosition = 0
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="title"></param>
    /// <param name="url"></param>
    /// <param name="faPosition"></param>
    /// <param name="share"></param>
    public void GetTMDBFanart (string path, string filename, string title, string url, int index, bool share)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      filename = Path.GetFileNameWithoutExtension(filename);
      filename = Utils.RemoveTrailingSlash(filename);

      try
      {
        string configDir;
        GetFanArtFolder(out configDir);
        string ext = ".jpg";
        
        if (Directory.Exists(configDir))
        {
          _fileArt = configDir + title + " " + index + ".jpg";

          WebClient webClient = new WebClient();
          // Database view
          webClient.DownloadFile(url, _fileArt);
          webClient.Dispose();

          // Share view
          if (share)
          {
            // If not DVD video
            if (!filename.ToUpper().Contains("VIDEO_TS"))
            {
              string fileclean = GetFileclean(filename);
              _fileMovie = configDir + fileclean + " " + index + ext;
              if (!_fileMovie.Equals(_fileArt, StringComparison.OrdinalIgnoreCase))
                File.Copy(_fileArt, _fileMovie, true);
            }
              // DVD Video
            else
            {
              SetDVD_fileMovie(configDir, path, index, ext);
              if (!_fileMovie.Equals(_fileArt, StringComparison.OrdinalIgnoreCase))
                File.Copy(_fileArt, _fileMovie, true);
            }
          }
        }
      }
      catch (Exception) { }
      finally { }
    }

    private void DownloadFA(string title, string configDir, int index, string ext)
    {
      try
      {
        _fileArt = configDir + title + " " + index + ext;
        var webClient = new WebClient();
        webClient.DownloadFile((string)_fanartList[index], _fileArt);
        _fanartURL = _fanartList[0].ToString();
        webClient.Dispose();
      }
      catch (Exception) { }
    }

    // Remove trash text from file name
    private string GetFileclean(string filename) 
    {
      string fileclean = filename;
      // Test pattern (CD, DISC(K), Part, X-Y...) and remove it from filename
      var pattern = Utils.StackExpression();
      foreach (var t in pattern)
      {
        if (t.IsMatch(filename))
        {
          fileclean = t.Replace(filename, "");
        }
      }
      return fileclean;
    }

    private void SetDVD_fileMovie(string configDir, string path, int fileIndex, string ext)
    {
      int end = path.ToUpper().IndexOf(@"\VIDEO_TS");
      string strTrimed = path.Substring(0, end);
      int start = strTrimed.LastIndexOf(@"\") + 1;
      string titleFolder = strTrimed.Substring(start);
      //Folder name DVD backdrop for SHARES View
      _fileMovie = configDir + titleFolder + " " + fileIndex + ext;
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

    // Helper funct to retreive saved fanart filename
    public static void GetFanArtfilename(string title, int fileIndex, out string fileart)
    {
      fileart = "";
      // FanArt directory
      string configDir;
      GetFanArtFolder(out configDir);
      if (Directory.Exists(configDir))
      {
        fileart = configDir + title + " " + fileIndex + ".jpg";
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
      if (File.Exists(file))
      {
        File.Delete(file);
      }
      file = Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
      if (File.Exists(file))
      {
        File.Delete(file);
      }
    }

    // Helper funct to delete fanarts
    public static void DeleteFanarts(string pathAndFilename, string title)
    {
      if (pathAndFilename == string.Empty || title == string.Empty)
      {
        return;
      }

      int fanartQ = 5;
      string configDir;

      GetFanArtFolder(out configDir);

      for (int i = 0; i < fanartQ; i++)
      {
        if (File.Exists(configDir + title + " " + i + ".jpg"))
        {
          File.Delete(configDir + title + " " + i + ".jpg");
        }
      }
      
      string fileNameArt;
      string strPath = string.Empty;
      Split(pathAndFilename, out strPath, out fileNameArt);

      if (!fileNameArt.ToUpper().Contains("VIDEO_TS"))
      {
        string fileclean = Path.GetFileNameWithoutExtension(fileNameArt);
        // Test pattern (CD, DISC(K), Part, X-Y...) and remove it from filename
        var pattern = Utils.StackExpression();

        foreach (var t in pattern)
        {
          if (t.IsMatch(fileNameArt))
          {
              fileclean = t.Replace(fileNameArt, "");
          }
        }
        
        for (int i = 0; i < fanartQ; i++)
        {
          string fileMovie = configDir + fileclean + " " + i + ".jpg";
          if (File.Exists(fileMovie))
          {
            File.Delete(fileMovie);
          }
        }
      }
        // DVD Video
      else
      {
        int end = strPath.ToUpper().IndexOf(@"\VIDEO_TS");
        string strTrimed = strPath.Substring(0, end);
        int start = strTrimed.LastIndexOf(@"\") + 1;
        string titleFolder = strTrimed.Substring(start);
        //Folder name DVD backdrop for SHARES View

        for (int i = 0; i < fanartQ; i++)
        {
          string fileMovie = configDir + titleFolder + " " + i + ".jpg";
          if (File.Exists(fileMovie))
          {
            File.Delete(fileMovie);
          }
        }
      }
    }

    // Returns default MP fanart folder
    public static void GetFanArtFolder(out string fanartFolder)
    {
      // FanArt directory
      fanartFolder = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\Movies\";
    }

    // Split path and filename
    private static void Split(string strFileNameAndPath, out string strPath, out string strFileName)
    {
      strFileNameAndPath = strFileNameAndPath.Trim();
      strFileName = "";
      strPath = "";
      if (strFileNameAndPath.Length == 0)
      {
        return;
      }
      int i = strFileNameAndPath.Length - 1;
      while (i > 0)
      {
        char ch = strFileNameAndPath[i];
        if (ch == ':' || ch == '/' || ch == '\\')
        {
          break;
        }
        i--;
      }
      strPath = strFileNameAndPath.Substring(0, i).Trim();
      strFileName = strFileNameAndPath.Substring(i, strFileNameAndPath.Length - i).Trim();
    }

    // Get URL HTML body
    private string GetPage(string strUrl, string strEncode, out string absoluteUri)
    {
      string strBody = "";
      absoluteUri = String.Empty;
      Stream receiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        //Log.Info("IMDB: get page:{0}", strURL);
        WebRequest req = WebRequest.Create(strUrl);
        req.Timeout = 10000;
        result = req.GetResponse();
        receiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = Encoding.GetEncoding(strEncode);
        sr = new StreamReader(receiveStream, encode);
        strBody = sr.ReadToEnd();

        absoluteUri = result.ResponseUri.AbsoluteUri;
      }
      catch (Exception ex)
      {
        Log.Error("Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strUrl, strEncode, ex.Message, ex.StackTrace);
      }
      finally
      {
        if (sr != null)
        {
          try
          {
            sr.Close();
          }
          catch (Exception) {}
        }
        if (receiveStream != null)
        {
          try
          {
            receiveStream.Close();
          }
          catch (Exception) {}
        }
        if (result != null)
        {
          try
          {
            result.Close();
          }
          catch (Exception) {}
        }
      }
      return strBody;
    }
  }
}