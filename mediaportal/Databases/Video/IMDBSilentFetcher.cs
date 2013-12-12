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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Profile;

namespace MediaPortal.Video.Database
{
  public class IMDBSilentFetcher
  {
    private IMDB _imdb = new IMDB();
    private bool _currentCreateVideoThumbs; // Original setting for thumbnail creation
    private bool _foldercheck = true;
    
    /// <summary>
    /// Find movie for videofile (file must be with full path)
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public bool FindMovie(string file)
    {
      if (string.IsNullOrEmpty(file) || !File.Exists(file) || !Win32API.IsConnectedToInternet())
      {
        return false;
      }

      // No scan for already existing
      if (VideoDatabase.HasMovieInfo(file))
      {
        return false;
      }

      IMDBMovie movieDetails = new IMDBMovie();

      // Try nfo file
      if (VideoDatabase.ImportNfoUsingVideoFile(file, true, false))
      {
        VideoDatabase.GetMovieInfo(file, ref movieDetails);
        FetchActorsInMovie(ref movieDetails);
        return true;
      }

      int digit = 0; // Only first file in set will proceed (cd1, part1, dvd1...)
      var pattern = Util.Utils.StackExpression();
      
      for (int i = 0; i < pattern.Length; i++)
      {
        if (pattern[i].IsMatch(file))
        {
          digit = Convert.ToInt16(pattern[i].Match(file).Groups["digit"].Value);
        }
      }

      try
      {
        // Check existing movie
        int id;

        // Single file movie or 1st file only
        if (digit < 2)
        {
          string path, filename;
          Util.Utils.Split(file, out path, out filename);
          movieDetails.Path = path;
          movieDetails.File = filename;

          if (path.EndsWith(@"\"))
          {
            path = path.Substring(0, path.Length - 1);
            movieDetails.Path = path;
          }

          if (filename.StartsWith(@"\"))
          {
            filename = filename.Substring(1);
            movieDetails.File = filename;
          }

          id = VideoDatabase.AddMovieFile(file);
          movieDetails.ID = id;
          VirtualDirectory dir = new VirtualDirectory();
          dir.SetExtensions(Util.Utils.VideoExtensions);
          
          // Thumb creation spam
          //
          // Temporary disable thumbcreation
          //
          using (Settings xmlreader = new MPSettings())
          {
            _currentCreateVideoThumbs = xmlreader.GetValueAsBool("thumbnails", "videoondemand", true);
          }
          using (Settings xmlwriter = new MPSettings())
          {
            xmlwriter.SetValueAsBool("thumbnails", "videoondemand", false);
          }
          List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true, false);
          //

          foreach (GUIListItem item in items)
          {
            if (item.IsFolder)
            {
              continue;
            }

            // Add stack parts related to original file to the same movie, only part 1 goes under scan (see digit variable usage above)
            if (Util.Utils.ShouldStack(item.Path, file) && item.Path != file)
            {
              string strPath, strFileName;
              DatabaseUtility.Split(item.Path, out strPath, out strFileName);
              DatabaseUtility.RemoveInvalidChars(ref strPath);
              DatabaseUtility.RemoveInvalidChars(ref strFileName);
              int pathId = VideoDatabase.AddPath(strPath);
              VideoDatabase.AddFile(id, pathId, strFileName);
            }
          }

          // SCAN (Find movie and movie details)!!!
          if (!GetMovieDetails(ref movieDetails, file))
          {
            using (Settings xmlwriter = new MPSettings())
            {
              xmlwriter.SetValueAsBool("thumbnails", "videoondemand", _currentCreateVideoThumbs);
            }

            return false;
          }

          // Restore thumbcreation setting
          using (Settings xmlwriter = new MPSettings())
          {
            xmlwriter.SetValueAsBool("thumbnails", "videoondemand", _currentCreateVideoThumbs);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Scan IMDBSilent err:{0} src:{1}, stack:{2}, ", ex.Message, ex.Source, ex.StackTrace);
      }
      
      return true;
    }
    
    /// <summary>
    /// Download movie details.
    /// </summary>
    private bool GetMovieDetails(ref IMDBMovie currentMovie, string fullPathFilename)
    {
      try
      {
        string strMovieName = string.Empty;
        string strFileName = fullPathFilename;
        string path = currentMovie.Path;
        string filename = currentMovie.File;
        _foldercheck = Util.Utils.IsFolderDedicatedMovieFolder(path);

        // Search string
        if (Util.Utils.IsDVD(strFileName))
        {
          // DVD
          string strDrive = strFileName.Substring(0, 2);
          currentMovie.DVDLabel = Util.Utils.GetDriveName(strDrive);
          strMovieName = currentMovie.DVDLabel;
        }
        else if (strFileName.ToUpperInvariant().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
        {
          // DVD folder
          string dvdFolder = strFileName.Substring(0, strFileName.ToUpperInvariant().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
          currentMovie.DVDLabel = Path.GetFileName(dvdFolder);
          strMovieName = currentMovie.DVDLabel;
        }
        else if (strFileName.ToUpperInvariant().IndexOf(@"\BDMV\INDEX.BDMV") >= 0)
        {
          // BD folder
          string bdFolder = strFileName.Substring(0, strFileName.ToUpperInvariant().IndexOf(@"\BDMV\INDEX.BDMV"));
          currentMovie.DVDLabel = Path.GetFileName(bdFolder);
          strMovieName = currentMovie.DVDLabel;
        }
        else
        {
          // Movie - Folder title and new ->remove CDx from name
          using (Settings xmlreader = new MPSettings())
          {
            bool preferFileName = xmlreader.GetValueAsBool("moviedatabase", "preferfilenameforsearch", false);
            
            if (_foldercheck && !preferFileName)
            {
              strMovieName = Path.GetFileName(Path.GetDirectoryName(strFileName));
            }
            else
            {
              strMovieName = Path.GetFileNameWithoutExtension(strFileName);
            }
            
            // Test pattern (CD, DISC(K), Part, X-Y...) and remove it from filename
            var pattern = Util.Utils.StackExpression();

            for (int i = 0; i < pattern.Length; i++)
            {
              if (strMovieName != null && (_foldercheck == false && pattern[i].IsMatch(strMovieName)))
              {
                strMovieName = pattern[i].Replace(strMovieName, "");
              }
            }
          }

          if ((strMovieName == string.Empty) || (strMovieName == Strings.Unknown))
          {
            return false;
          }
        }

        // Try to get IMDB tt number from filename or video txt file
        SearchForImdbId(path, filename, _foldercheck, ref strMovieName);
        Util.Utils.RemoveStackEndings(ref strMovieName);
        currentMovie.SearchString = strMovieName;

        // Search movie (CSSCRIPT)
        _imdb.FindSilent(strMovieName);

        if (_imdb.Count == 0)
        {
          Log.Debug("IMDBSilentFetcher: No movie found for {0}", strMovieName);
          return false;
        }

        // Find nearest match (no conflict list, maybe to do this later)
        int matchingDistance = int.MaxValue;
        string name = _imdb.GetSearchString(filename);
        int movieIndex = 0;

        for (int index = 0; index < _imdb.Count; ++index)
        {
          int distance = Levenshtein.Match(name, _imdb[index].Title.ToLowerInvariant());

          if (distance < matchingDistance)
          {
            movieIndex = index;
            matchingDistance = distance;
          }
        }

        // Get movie details (CSSCRIPT)
        if (!_imdb.GetDetails(_imdb[movieIndex], ref currentMovie))
        {
          return false;
        }

        // Set extra details (covers, actors, fanart....)
        SetExtraMovieDetails(ref currentMovie);
        // Send message that we have new movie (fanart like this to refresh new downloaded fanart)
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
        Log.Info("IMDBSilent RefreshIMDB() - Found movie and added info for: {0} (Year: {1})",
          currentMovie.Title, currentMovie.Year);
      }
      catch (ThreadAbortException)
      {
        // N/A
      }
      catch (Exception ex)
      {
        Log.Error("IMDBSIlentFetcher error: {0}", ex.Message);
        return false;
      }
      return true;
    }

    #region Helpers

    #region Set Extra Movie details (cover, fanart, actors)

    private void SetExtraMovieDetails(ref IMDBMovie movieDetails)
    {
      try
      {
        if (movieDetails == null)
        {
          movieDetails = new IMDBMovie();
          return;
        }

        // Get special settings for grabbing
        bool folderTitle = _foldercheck;
        int faCount;
        bool stripPrefix;
        bool useFanArt;

        using (Settings xmlreader = new MPSettings())
        {
          // Number of downloaded fanart per movie
          faCount = xmlreader.GetValueAsInt("moviedatabase", "fanartnumber", 1);
          stripPrefix = xmlreader.GetValueAsBool("moviedatabase", "striptitleprefixes", false);
          useFanArt = xmlreader.GetValueAsBool("moviedatabase", "usefanart", false);
        }

        if (stripPrefix)
        {
          string tmpTitle = movieDetails.Title;
          Util.Utils.StripMovieNamePrefix(ref tmpTitle, true);
          movieDetails.Title = tmpTitle;
        }

        #region Covers

        //
        // Covers - If cover is not empty don't change it, else download new
        //
        // Local cover check (every movie is in it's own folder), lookin' for folder.jpg
        // or look for local cover named as movie file
        string localCover = string.Empty; // local cover named as movie filename
        string movieFile = string.Empty;
        string moviePath = movieDetails.Path;
        string titleExt = string.Empty;
        string largeCoverArt = string.Empty;
        string coverArt = string.Empty;

        // Find movie file(s)
        ArrayList files = new ArrayList();
        VideoDatabase.GetFilesForMovie(movieDetails.ID, ref files);

        // Remove stack endings for video file(CD, Part...)
        if (files.Count > 0)
        {
          movieFile = (string) files[0];
          Util.Utils.RemoveStackEndings(ref movieFile);
        }

        localCover = moviePath + @"\" + Util.Utils.GetFilename(movieFile, true) + ".jpg";

        // Every movie in it's own folder?
        if (folderTitle)
        {
          string folderCover = moviePath + @"\folder.jpg";

          if (File.Exists(folderCover))
          {
            movieDetails.ThumbURL = "file://" + folderCover;
          }
          else if (File.Exists(localCover))
          {
            movieDetails.ThumbURL = "file://" + localCover;
          }
        }
          // Try local movefilename.jpg
        else if (File.Exists(localCover))
        {
          movieDetails.ThumbURL = "file://" + localCover;
        }

        // No local or scraper thumb
        if (movieDetails.ThumbURL == string.Empty)
        {
          TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();
          tmdbSearch.SearchCovers(movieDetails.Title, movieDetails.IMDBNumber);

          if ((tmdbSearch.Count > 0) && (tmdbSearch[0] != string.Empty))
          {
            movieDetails.ThumbURL = tmdbSearch[0];
          }

          if (tmdbSearch.Count == 0)
          {
            // IMPAwards
            IMPAwardsSearch impSearch = new IMPAwardsSearch();

            if (movieDetails.Year > 1900)
            {
              impSearch.SearchCovers(movieDetails.Title + " " + movieDetails.Year, movieDetails.IMDBNumber);
            }
            else
            {
              impSearch.SearchCovers(movieDetails.Title, movieDetails.IMDBNumber);
            }

            if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
            {
              movieDetails.ThumbURL = impSearch[0];
            }

            // All fail, last try IMDB
            if (impSearch.Count == 0 && tmdbSearch.Count == 0)
            {
              IMDBSearch imdbSearch = new IMDBSearch();
              imdbSearch.SearchCovers(movieDetails.IMDBNumber, true);

              if ((imdbSearch.Count > 0) && (imdbSearch[0] != string.Empty))
              {
                movieDetails.ThumbURL = imdbSearch[0];
              }
            }
          }
        }

        titleExt = movieDetails.Title + "{" + movieDetails.ID + "}";
        largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
        coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);

        if (movieDetails.ID >= 0)
        {
          if (!string.IsNullOrEmpty(movieDetails.ThumbURL))
          {
            Util.Utils.FileDelete(largeCoverArt);
            Util.Utils.FileDelete(coverArt);
          }
        }

        #endregion

        #region Fanart

        // FanArt grab
        if (useFanArt)
        {
          string localFanart = string.Empty;
          FanArt fanartSearch = new FanArt();

          // Check local fanart (only if every movie is in it's own folder), lookin for fanart.jpg
          // Looking for fanart.jpg or videofilename-fanart.jpg
          if (folderTitle)
          {
            localFanart = moviePath + @"\fanart.jpg";

            if (File.Exists(localFanart))
            {
              movieDetails.FanartURL = "file://" + localFanart;
            }
            else
            {
              localFanart = moviePath + @"\" + Util.Utils.GetFilename(movieFile, true) + @"-fanart.jpg";

              if (File.Exists(localFanart))
              {
                movieDetails.FanartURL = "file://" + localFanart;
              }
            }
          }
          else
          {
            localFanart = moviePath + @"\" + Util.Utils.GetFilename(movieFile, true) + @"-fanart.jpg";

            if (File.Exists(localFanart))
            {
              movieDetails.FanartURL = "file://" + localFanart;
            }
          }

          if (movieDetails.FanartURL == string.Empty || movieDetails.FanartURL == Strings.Unknown)
          {
            fanartSearch.GetTmdbFanartByApi
              (movieDetails.ID, movieDetails.IMDBNumber, movieDetails.Title, true, faCount, string.Empty);
            // Set fanart url to db
            movieDetails.FanartURL = fanartSearch.DefaultFanartUrl;
          }
          else // Local file or user url
          {
            fanartSearch.GetLocalFanart(movieDetails.ID, movieDetails.FanartURL, 0);
          }
        }

        #endregion

        #region Actors

        if (VideoDatabase.CheckMovieImdbId(movieDetails.IMDBNumber))
        {
          // Do not save movieinfo to database when fetching actors (false paramater) beacuse we don't have all
          // movie metadata yet
          FetchActorsInMovie(ref movieDetails);
        }

        #endregion


        #region Download & save covers, save movie info

        if (movieDetails.ID >= 0)
        {
          // Save cover thumbs
          if (!string.IsNullOrEmpty(movieDetails.ThumbURL))
          {
            DownloadCoverArt(Thumbs.MovieTitle, movieDetails.ThumbURL, titleExt);
          }

          // Set folder.jpg for ripped DVDs
          try
          {
            string path = movieDetails.Path;
            string filename = movieDetails.File;

            if (filename.ToUpperInvariant() == "VIDEO_TS.IFO" || filename.ToUpperInvariant() == "INDEX.BDMV")
            {
              // Remove \VIDEO_TS from directory structure
              string directoryDVD = path.Substring(0, path.LastIndexOf(@"\"));

              if (Directory.Exists(directoryDVD))
              {
                // Copy large cover file as folder.jpg
                File.Copy(largeCoverArt, directoryDVD + @"\folder.jpg", true);
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("IMDBFetcher.FetchDetailsThread() folder.jpg copy error: {0}", ex.Message);
          }

          // Check movie table if there is an entry that new movie is already played as share
          int percentage = 0;
          int timesWatched = 0;

          if (VideoDatabase.GetmovieWatchedStatus(movieDetails.ID, out percentage, out timesWatched))
          {
            movieDetails.Watched = 1;
          }

          // Save movie info
          VideoDatabase.SetMovieInfoById(movieDetails.ID, ref movieDetails, true);

          // Add groups with rules
          ArrayList groups = new ArrayList();
          VideoDatabase.GetUserGroups(groups);

          foreach (string group in groups)
          {
            string rule = VideoDatabase.GetUserGroupRule(group);

            if (!string.IsNullOrEmpty(rule))
            {
              try
              {
                ArrayList values = new ArrayList();
                bool error = false;
                string errorMessage = string.Empty;
                values = VideoDatabase.ExecuteRuleSql(rule, "movieinfo.idMovie", out error, out errorMessage);

                if (error)
                {
                  continue;
                }

                if (values.Count > 0 && values.Contains(movieDetails.ID.ToString()))
                {
                  VideoDatabase.AddUserGroupToMovie(movieDetails.ID, VideoDatabase.AddUserGroup(group));
                }
              }
              catch (Exception)
              {
              }
            }
          }
        }

          #endregion

        else
        {
          movieDetails = null;
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Debug("IMDBSilentFetcher SetMovieDetails Error: {0}", ex.Message);
      }
    }

    #endregion

    #region All movie Actors fetch/refresh

    private void FetchActorsInMovie(ref IMDBMovie movieDetails)
    {
      ArrayList actors = new ArrayList();

      if (movieDetails == null)
      {
        return;
      }

      // Check for IMDBid 
      if (VideoDatabase.CheckMovieImdbId(movieDetails.IMDBNumber))
      {
        // Returns nm1234567 as actor name (IMDB actorID)
        IMDB imdbActors = new IMDB();
        imdbActors.GetIMDBMovieActorsList(movieDetails.IMDBNumber, ref actors);
      }
      else
      {
        return;
      }

      if (actors.Count > 0)
      {
        // Clean old actors for movie
        VideoDatabase.RemoveActorsForMovie(movieDetails.ID);

        for (int i = 0; i < actors.Count; ++i)
        {
          string actor = (string)actors[i];
          string actorImdbId = string.Empty;
          string actorName = string.Empty;
          string role = string.Empty;
          bool director = false;

          char[] splitter = { '|' };
          string[] temp = actor.Split(splitter);
          actorName = temp[0];

          // Check if actor is movie director
          if (actorName.StartsWith("*d"))
          {
            actorName = actorName.Replace("*d", string.Empty);
            director = true;
          }

          actorImdbId = temp[1];
          role = temp[2];
          // Add actor and link actor to movie
          int actorId = VideoDatabase.AddActor(actorImdbId, actorName);

          if (actorId == -1)
          {
            continue;
          }

          VideoDatabase.AddActorToMovie(movieDetails.ID, actorId, role);

          // Update director in movieinfo
          if (director)
          {
            movieDetails.DirectorID = actorId;
            movieDetails.Director = actorName;
            VideoDatabase.SetMovieInfoById(movieDetails.ID, ref movieDetails);
          }
        }
      }
    }

    #endregion

    private void DownloadCoverArt(string type, string imageUrl, string title)
    {
      try
      {
        if (imageUrl.Length > 0)
        {
          string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(type, title);
          string coverArtImage = Util.Utils.GetCoverArtName(type, title);

          if (!File.Exists(coverArtImage))
          {
            string imageExtension = Path.GetExtension(imageUrl);

            if (imageExtension == string.Empty)
            {
              imageExtension = ".jpg";
            }

            string temporaryFilename = "MPTempImage";
            temporaryFilename += imageExtension;
            temporaryFilename = Path.Combine(Path.GetTempPath(), temporaryFilename);
            Util.Utils.FileDelete(temporaryFilename);

            // Check if image is file
            if (imageUrl.Length > 7 && imageUrl.Substring(0, 7).Equals("file://"))
            {
              // Local image, don't download, just copy
              File.Copy(imageUrl.Substring(7), temporaryFilename);
              File.SetAttributes(temporaryFilename, FileAttributes.Normal);
            }
            else
            {
              Util.Utils.DownLoadAndOverwriteCachedImage(imageUrl, temporaryFilename);
            }

            if (File.Exists(temporaryFilename))
            {
              if (Util.Picture.CreateThumbnail(temporaryFilename, largeCoverArtImage, (int)Thumbs.ThumbLargeResolution,
                                               (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsSmall))
              {
                Util.Picture.CreateThumbnail(temporaryFilename, coverArtImage, (int)Thumbs.ThumbResolution,
                                             (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsLarge);
              }
            }

            Util.Utils.FileDelete(temporaryFilename);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("IMDBFetcher: DownloadCoverArt({0}, {1}, {2}) error: {3}", type, imageUrl, title, ex.Message);
      }
    }

    /// <summary>
    /// Try to find IMDB id number in filename or in txt-nfo file
    /// IMDB script and other scripts which use that number can benefit from this
    /// This will change search string to IMDB id number so maybe this helper should
    /// be activated in setting???
    /// </summary>
    /// <param name="path">path without filename</param>
    /// <param name="filename">filename without path</param>
    /// <param name="dedicatedMovieFolders">is every movie in its own folder?</param>
    /// <param name="searchString">returns tt1234567 if success</param>
    private void SearchForImdbId(string path, string filename, bool dedicatedMovieFolders, ref string searchString)
    {
      try
      {
        // First check for DVD/BD folders and take main path
        bool isDvdBd = false;
        if (path.ToUpperInvariant().Contains(@"\VIDEO_TS"))
        {
          path = path.Replace(@"\VIDEO_TS", string.Empty);
          isDvdBd = true;
        }
        else if (path.ToUpperInvariant().Contains(@"\BDMV"))
        {
          path = path.Replace(@"\BDMV", string.Empty);
          isDvdBd = true;
        }

        // DVD or BD
        if (isDvdBd || dedicatedMovieFolders)
        {
          // Check for tt in path name
          if (MatchImdb(ref path))
          {
            searchString = path;
          }
          else
          {
            // Get all txt and nfo files in main path directory
            ArrayList files = new ArrayList();
            string[] txtFiles = Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly);
            files.AddRange(txtFiles);
            txtFiles = Directory.GetFiles(path, "*.nfo", SearchOption.TopDirectoryOnly);
            files.AddRange(txtFiles);

            foreach (string file in files)
            {
              if (File.Exists(file))
              {
                // Check for tt in txt or nfo filename
                string strFile = file;

                if (MatchImdb(ref strFile))
                {
                  searchString = strFile;
                  return;
                }

                // check for tt in files content (firts match success will break loop))
                string txt = string.Empty;

                using (StreamReader reader = new StreamReader(file))
                {
                  txt = reader.ReadToEnd();

                  if (MatchImdb(ref txt))
                  {
                    searchString = txt;
                    break;
                  }
                }
              }
            }
          }
        }
        else
        {
          // normal videos (nfo or txt files must contain videofilename part)
          string[] txtFile = {
                               path + @"\" + Util.Utils.GetFilename(filename,true) + @"-IMDB.txt",
                               path + @"\" + Util.Utils.GetFilename(filename,true) + @"-IMDB.nfo",
                               path + @"\" + Util.Utils.GetFilename(filename,true) + @".nfo",
                               path + @"\" + Util.Utils.GetFilename(filename,true) + @".txt",
                             };

          foreach (string file in txtFile)
          {
            if (File.Exists(file))
            {
              // check first tt in filename
              string strFile = file;

              if (MatchImdb(ref strFile))
              {
                searchString = strFile;
                return;
              }

              // check filename content for tt
              string txt = string.Empty;

              using (StreamReader reader = new StreamReader(file))
              {
                txt = reader.ReadToEnd();

                if (MatchImdb(ref txt))
                {
                  searchString = txt;
                  break;
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("IMDBFetcher SearchForImdbId exception {0}", ex.Message);
      }
    }

    /// <summary>
    /// Check is IMDB id in valid format
    /// </summary>
    /// <param name="strToMatch"></param>
    /// <returns></returns>
    private bool MatchImdb(ref string strToMatch)
    {
      Match match = Regex.Match(strToMatch, @"tt[\d]{7}?", RegexOptions.IgnoreCase);

      if (match.Success)
      {
        strToMatch = match.Value;
        return true;
      }

      return false;
    }

    #endregion
  }
}