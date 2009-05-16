#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.Video.Database
{
  public class IMDBFetcher : IMDB.IProgress
  {
    private string movieName;
    private IMDB.IMDBUrl url;
    private IMDB _imdb;
    private IMDBMovie movieDetails;
    private Thread movieThread;
    private Thread detailsThread;
    private Thread actorsThread;
    private IMDB.IProgress progress;
    private bool disableCancel = false;
    private bool getActors;

    static IMDBFetcher()
    {
    }

    public IMDBFetcher(IMDB.IProgress progress)
    {
      _imdb = new IMDB(this);
      this.progress = progress;
    }

    public bool Fetch(string movieName)
    {
      this.movieName = movieName;
      if (movieName == string.Empty)
      {
        return true;
      }
      if (!OnSearchStarting(this))
      {
        return false;
      }
      movieThread = new Thread(new ThreadStart(this._fetch));
      movieThread.Name = "IMDBFetcher";
      movieThread.IsBackground = true;
      movieThread.Start();
      if (!OnSearchStarted(this))
      {
        this.CancelFetch();
        return false;
      }
      return true;
    }

    private void _fetch()
    {
      try
      {
        disableCancel = false;
        if (movieName == null)
        {
          return;
        }
        _imdb.Find(this.movieName);
      }
      catch (ThreadAbortException)
      {
      }
      finally
      {
        OnSearchEnd(this);
        disableCancel = false;
        //Log.Info("Ending Thread for Fetching movie list:{0}", movieThread.ManagedThreadId);
        movieThread = null;
      }
    }

    public bool FetchDetails(int selectedMovie, ref IMDBMovie currentMovie)
    {
      movieDetails = currentMovie;
      if ((selectedMovie < 0) || (selectedMovie >= this.Count))
      {
        return true;
      }
      this.url = this[selectedMovie];
      this.movieName = this.url.Title;
      if (!OnDetailsStarting(this))
      {
        return false;
      }
      detailsThread = new Thread(new ThreadStart(this._fetchDetails));
      detailsThread.IsBackground = true;
      detailsThread.Name = "IMDBDetails";
      detailsThread.Start();
      if (!OnDetailsStarted(this))
      {
        this.CancelFetchDetails();
        return false;
      }
      return true;
    }

    private void _fetchDetails()
    {
      try
      {
        disableCancel = false;
        if (movieDetails == null)
        {
          movieDetails = new IMDBMovie();
        }
        if (url == null)
        {
          return;
        }
        if (_imdb.GetDetails(this.url, ref movieDetails))
        {
          string line1;
          if (movieDetails.ThumbURL == string.Empty)
          {
            line1 = GUILocalizeStrings.Get(928) + ":IMP Awards";
            OnProgress(line1, movieDetails.Title, string.Empty, -1);
            IMPawardsSearch impSearch = new IMPawardsSearch();
            impSearch.Search(movieDetails.Title);
            if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
            {
              movieDetails.ThumbURL = impSearch[0];
            }
            else
            {
              line1 = GUILocalizeStrings.Get(928) + ":Amazon";
              OnProgress(line1, movieDetails.Title, string.Empty, -1);
              AmazonImageSearch search = new AmazonImageSearch();
              search.Search(movieDetails.Title);
              if (search.Count > 0)
              {
                movieDetails.ThumbURL = search[0];
              }
            }
          }
          string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, movieDetails.Title);
          string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, movieDetails.Title);
          Util.Utils.FileDelete(largeCoverArt);
          Util.Utils.FileDelete(coverArt);
          line1 = GUILocalizeStrings.Get(1009);
          OnProgress(line1, movieDetails.Title, string.Empty, -1);
          //Only get actors if we really want to.
          if (getActors)
          {
            _fetchActorsInMovie();
          }
          OnDisableCancel(this);
          DownloadCoverArt(Thumbs.MovieTitle, movieDetails.ThumbURL, movieDetails.Title);
          VideoDatabase.SetMovieInfoById(movieDetails.ID, ref movieDetails);
        }
        else
        {
          movieDetails = null;
        }
      }
      catch (ThreadAbortException)
      {
      }
      finally
      {
        OnDetailsEnd(this);
        disableCancel = false;
        //Log.Info("Ending Thread for Fetching movie details:{0}", detailsThread.ManagedThreadId);
        detailsThread = null;
      }
    }

    public bool FetchActors()
    {
      if (!OnActorsStarting(this))
      {
        return false;
      }
      actorsThread = new Thread(new ThreadStart(this._fetchActors));
      actorsThread.IsBackground = true;
      actorsThread.Name = "IMDBActors";
      actorsThread.Start();
      if (!OnActorsStarted(this))
      {
        this.CancelFetchActors();
        return false;
      }
      return true;
    }

    private void _fetchActors()
    {
      try
      {
        _fetchActorsInMovie();
      }
      catch (ThreadAbortException)
      {
      }
      finally
      {
        OnActorsEnd(this);
        //Log.Info("Ending Thread for Fetching actors:{0}", actorsThread.ManagedThreadId);
        actorsThread = null;
        disableCancel = false;
      }
    }

    private void _fetchActorsInMovie()
    {
      if (movieDetails == null)
      {
        return;
      }
      string cast = movieDetails.Cast + "," + movieDetails.Director;
      char[] splitter = {'\n', ','};
      string[] temp = cast.Split(splitter);
      ArrayList actors = new ArrayList();
      foreach (string element in temp)
      {
        string el = element.Trim();
        if (el != string.Empty)
        {
          actors.Add(el);
        }
      }
      if (actors.Count > 0)
      {
        int percent = 0;
        for (int i = 0; i < actors.Count; ++i)
        {
          string actor = (string) actors[i];
          string role = string.Empty;
          int pos = actor.IndexOf(" as ");
          if (pos >= 0)
          {
            role = actor.Substring(pos + 4);
            actor = actor.Substring(0, pos);
          }
          actor = actor.Trim();
          string line1, line2, line3;
          line1 = GUILocalizeStrings.Get(986);
          line2 = actor;
          line3 = "";
          OnProgress(line1, line2, line3, percent);
          _imdb.FindActor(actor);
          IMDBActor imdbActor = new IMDBActor();
          if (_imdb.Count > 0)
          {
            int index = FuzzyMatch(actor);
            if (index == -1)
            {
              index = 0;
            }
            ////Log.Info("Getting actor:{0}", _imdb[index].Title);
            _imdb.GetActorDetails(_imdb[index], out imdbActor);
            ////Log.Info("Adding actor:{0}({1}),{2}", imdbActor.Name, actor, percent);
            int actorId = VideoDatabase.AddActor(imdbActor.Name);
            if (actorId > 0)
            {
              line1 = GUILocalizeStrings.Get(986);
              line2 = imdbActor.Name;
              line3 = "";
              OnProgress(line1, line2, line3, -1);
              VideoDatabase.SetActorInfo(actorId, imdbActor);
              VideoDatabase.AddActorToMovie(movieDetails.ID, actorId);
              if (imdbActor.ThumbnailUrl != string.Empty)
              {
                string largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, imdbActor.Name);
                string coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieActors, imdbActor.Name);
                Util.Utils.FileDelete(largeCoverArt);
                Util.Utils.FileDelete(coverArt);
                line1 = GUILocalizeStrings.Get(1009);
                OnProgress(line1, line2, line3, percent);
                DownloadCoverArt(Thumbs.MovieActors, imdbActor.ThumbnailUrl, imdbActor.Name);
              }
            }
          }
          else
          {
            line1 = GUILocalizeStrings.Get(986);
            line2 = actor;
            line3 = "";
            OnProgress(line1, line2, line3, -1);
            int actorId = VideoDatabase.AddActor(actor);
            imdbActor.Name = actor;
            IMDBActor.IMDBActorMovie imdbActorMovie = new IMDBActor.IMDBActorMovie();
            imdbActorMovie.MovieTitle = movieDetails.Title;
            imdbActorMovie.Year = movieDetails.Year;
            imdbActorMovie.Role = role;
            imdbActor.Add(imdbActorMovie);
            VideoDatabase.SetActorInfo(actorId, imdbActor);
            VideoDatabase.AddActorToMovie(movieDetails.ID, actorId);
          }
          percent += 100/actors.Count;
        }
      }
    }

    public void CancelFetchActors()
    {
      if (actorsThread == null)
      {
        return;
      }
      if ((actorsThread.IsAlive) && (!disableCancel))
      {
        //Log.Info("Aborting Thread for Fetching:{0}", actorsThread.ManagedThreadId);
        actorsThread.Abort();
      }
    }

    public void CancelFetch()
    {
      if (movieThread == null)
      {
        return;
      }
      if ((movieThread.IsAlive) && (!disableCancel))
      {
        //Log.Info("Aborting Thread for Fetching:{0}", movieThread.ManagedThreadId);
        movieThread.Abort();
      }
    }

    public void CancelFetchDetails()
    {
      if (detailsThread == null)
      {
        return;
      }
      if ((detailsThread.IsAlive) && (!disableCancel))
      {
        //Log.Info("Aborting Thread for Fetching:{0}", detailsThread.ManagedThreadId);
        detailsThread.Abort();
      }
    }

    // MovieName for the search
    public string MovieName
    {
      get { return this.movieName; }
      set { this.movieName = value; }
    } // END MovieName
    // count the elements
    public int Count
    {
      get { return _imdb.Count; }
    } // END Count
    // URL for the details
    public IMDB.IMDBUrl URL
    {
      get { return url; }
      set { this.url = value; }
    } // END URL
    // Movie the elements
    public IMDBMovie Movie
    {
      get { return movieDetails; }
      set { this.movieDetails = value; }
    } // END Count

    public IMDB.IMDBUrl this[int index]
    {
      get { return _imdb[index]; }
    } // END IMDB.IMDBUrl this[int index]

    public int FuzzyMatch(string name)
    {
      int matchingIndex = -1;
      int matchingDistance = int.MaxValue;
      bool isAmbiguous = false;

      for (int index = 0; index < _imdb.Count; ++index)
      {
        int distance = Levenshtein.Match(name, _imdb[index].Title);

        if (distance == matchingDistance && matchingDistance != int.MaxValue)
        {
          isAmbiguous = true;
        }

        if (distance < matchingDistance)
        {
          isAmbiguous = false;
          matchingDistance = distance;
          matchingIndex = index;
        }
      }

      if (isAmbiguous)
      {
        return -1;
      }

      return matchingIndex;
    }

    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      disableCancel = true;
      if (progress != null)
      {
        return progress.OnDisableCancel(fetcher);
      }
      return false;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (progress != null)
      {
        progress.OnProgress(line1, line2, line3, percent);
      }
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnSearchStarted(fetcher);
      }
      return false;
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnSearchStarting(fetcher);
      }
      return false;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnSearchEnd(fetcher);
      }
      return false;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnMovieNotFound(fetcher);
      }
      return false;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnDetailsStarted(fetcher);
      }
      return false;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnDetailsStarting(fetcher);
      }
      return false;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnDetailsEnd(fetcher);
      }
      return false;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnActorsStarted(fetcher);
      }
      return false;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnActorsStarting(fetcher);
      }
      return false;
    }

    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnActorsEnd(fetcher);
      }
      return false;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      if (progress != null)
      {
        return progress.OnDetailsNotFound(fetcher);
      }
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      if (progress != null)
      {
        return progress.OnRequestMovieTitle(fetcher, out movieName);
      }
      else
      {
        movieName = string.Empty;
        return false;
      }
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selected)
    {
      if (progress != null)
      {
        return progress.OnSelectMovie(fetcher, out selected);
      }
      else
      {
        selected = -1;
        return false;
      }
    }

    public bool OnScanStart(int total)
    {
      if (progress != null)
      {
        return progress.OnScanStart(total);
      }
      return true;
    }

    public bool OnScanEnd()
    {
      if (progress != null)
      {
        return progress.OnScanEnd();
      }
      return true;
    }

    public bool OnScanIterating(int count)
    {
      if (progress != null)
      {
        return progress.OnScanIterating(count);
      }
      return true;
    }

    public bool OnScanIterated(int count)
    {
      if (progress != null)
      {
        return progress.OnScanIterated(count);
      }
      return true;
    }

    #endregion

    /// <summary>
    /// Download IMDB info for a movie
    /// </summary>
    public static bool RefreshIMDB(IMDB.IProgress progress, ref IMDBMovie currentMovie, bool fuzzyMatching,
                                   bool getActors)
    {
      Log.Info("RefreshIMDB() - Refreshing MovieInfo for {0}-{1}", currentMovie.Title, currentMovie.SearchString);
      string strMovieName = currentMovie.SearchString;
      string strFileName = string.Empty;
      string path = currentMovie.Path;
      string filename = currentMovie.File;
      if (path != string.Empty)
      {
        if (path.EndsWith(@"\"))
        {
          path = path.Substring(0, path.Length - 1);
          currentMovie.Path = path;
        }
        if (filename.StartsWith(@"\"))
        {
          filename = filename.Substring(1);
          currentMovie.File = filename;
        }
        strFileName = path + @"\" + filename;
      }
      else
      {
        strFileName = filename;
      }
      if ((strMovieName == string.Empty) || (strMovieName == Strings.Unknown))
      {
        strMovieName = currentMovie.Title;
        if ((strMovieName == string.Empty) || (strMovieName == Strings.Unknown))
        {
          if (strFileName == string.Empty)
          {
            return true;
          }
          if (Util.Utils.IsDVD(strFileName))
          {
            //DVD
            string strDrive = strFileName.Substring(0, 2);
            currentMovie.DVDLabel = Util.Utils.GetDriveName(strDrive);
            strMovieName = currentMovie.DVDLabel;
          }
          else if (strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
          {
            //DVD folder
            string dvdFolder = strFileName.Substring(0, strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
            currentMovie.DVDLabel = Path.GetFileName(dvdFolder);
            strMovieName = currentMovie.DVDLabel;
          }
          else
          {
            //Movie 
            strMovieName = Path.GetFileNameWithoutExtension(strFileName);
          }
        }
        if ((strMovieName == string.Empty) || (strMovieName == Strings.Unknown))
        {
          return true;
        }
      }
      if (currentMovie.ID == -1)
      {
        currentMovie.ID = VideoDatabase.AddMovieFile(strFileName);
      }
      currentMovie.SearchString = strMovieName;
      if (currentMovie.ID >= 0)
      {
        if (!Win32API.IsConnectedToInternet())
        {
          return false;
        }
        IMDBFetcher fetcher = new IMDBFetcher(progress);
        fetcher.Movie = currentMovie;
        fetcher.getActors = getActors;
        int selectedMovie = -1;
        do
        {
          if (!fetcher.Fetch(strMovieName))
          {
            return false;
          }
          if (fuzzyMatching)
          {
            selectedMovie = fetcher.FuzzyMatch(fetcher.MovieName);
            if (selectedMovie == -1)
            {
              if (!fetcher.OnMovieNotFound(fetcher))
              {
                return false;
              }
              if (!fetcher.OnRequestMovieTitle(fetcher, out strMovieName))
              {
                return false;
              }
              if (strMovieName == string.Empty)
              {
                return false;
              }
            }
          }
          else
          {
            if (fetcher.Count > 0)
            {
              int iMoviesFound = fetcher.Count;
              //GEMX 28.03.08: There should always be a choice to enter the movie manually 
              //               in case the 1 and only found name is wrong
              /*if (iMoviesFound == 1)
              {
                selectedMovie = 0;
              } else */
              if (iMoviesFound > 0)
              {
                if (!fetcher.OnSelectMovie(fetcher, out selectedMovie))
                {
                  return false;
                }
                if (selectedMovie < 0)
                {
                  if (!fetcher.OnRequestMovieTitle(fetcher, out strMovieName))
                  {
                    return false;
                  }
                  if (strMovieName == string.Empty)
                  {
                    return false;
                  }
                }
              }
            }
            else
            {
              if (!fetcher.OnMovieNotFound(fetcher))
              {
                return false;
              }
              if (!fetcher.OnRequestMovieTitle(fetcher, out strMovieName))
              {
                return false;
              }
              if (strMovieName == string.Empty)
              {
                return false;
              }
            }
          }
        } while (selectedMovie < 0);
        if (!fetcher.FetchDetails(selectedMovie, ref currentMovie))
        {
          return false;
        }
        IMDBMovie movieDetails = fetcher.Movie;

        if (movieDetails != null)
        {
          movieDetails.SearchString = strMovieName;
          currentMovie = movieDetails;
          return true;
        }
        else
        {
          return fetcher.OnDetailsNotFound(fetcher);
        }
      }
      return false;
    }

    public static void DownloadCoverArt(string type, string imageUrl, string title)
    {
      try
      {
        if (imageUrl.Length > 0)
        {
          string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(type, title);
          string coverArtImage = Util.Utils.GetCoverArtName(type, title);
          if (!File.Exists(coverArtImage))
          {
            string imageExtension;
            imageExtension = Path.GetExtension(imageUrl);
            if (imageExtension == string.Empty)
            {
              imageExtension = ".jpg";
            }
            string temporaryFilename = "temp";
            temporaryFilename += imageExtension;
            Util.Utils.FileDelete(temporaryFilename);

            Util.Utils.DownLoadAndCacheImage(imageUrl, temporaryFilename);
            if (File.Exists(temporaryFilename))
            {
              if (Util.Picture.CreateThumbnail(temporaryFilename, largeCoverArtImage, (int) Thumbs.ThumbLargeResolution,
                                               (int) Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsSmall))
              {
                Util.Picture.CreateThumbnail(temporaryFilename, coverArtImage, (int) Thumbs.ThumbResolution,
                                             (int) Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsLarge);
              }
            }

            Util.Utils.FileDelete(temporaryFilename);
          }
        }
      }
      catch (Exception)
      {
      }
    }

    /// <summary>
    /// Download IMDB info for all movies in a collection of paths
    /// </summary>
    public static bool ScanIMDB(IMDB.IProgress progress, ArrayList paths, bool fuzzyMatching, bool skipExisting,
                                bool getActors)
    {
      bool success = true;
      ArrayList availableFiles = new ArrayList();
      foreach (string path in paths)
      {
        CountFiles(path, ref availableFiles);
      }
      if (progress != null)
      {
        progress.OnScanStart(availableFiles.Count);
      }

      int count = 1;
      foreach (string file in availableFiles)
      {
        Log.Info("Scanning file: {0}", file);
        if ((progress != null) && (!progress.OnScanIterating(count)))
        {
          success = false;
          break;
        }

        IMDBMovie movieDetails = new IMDBMovie();
        int id = VideoDatabase.GetMovieInfo(file, ref movieDetails);
        if (!skipExisting || id == -1)
        {
          string path, filename;
          Util.Utils.Split(file, out path, out filename);
          movieDetails.Path = path;
          movieDetails.File = filename;
          GetInfoFromIMDB(progress, ref movieDetails, fuzzyMatching, getActors);
        }
        if ((progress != null) && (!progress.OnScanIterated(count++)))
        {
          success = false;
          break;
        }
      }

      if (progress != null)
      {
        progress.OnScanEnd();
      }
      return success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="totalFiles"></param>
    private static void CountFiles(string path, ref ArrayList availableFiles)
    {
      //
      // Count the files in the current directory
      //
      try
      {
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.VideoExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true);
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            if (item.Label != "..")
            {
              if (item.Path.ToLower().IndexOf("video_ts") >= 0)
              {
                string strFile = String.Format(@"{0}\VIDEO_TS.IFO", item.Path);
                availableFiles.Add(strFile);
              }
              else
              {
                CountFiles(item.Path, ref availableFiles);
              }
            }
          }
          else
          {
            availableFiles.Add(item.Path);
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("Exception counting files:{0}", e);
        // Ignore
      }
    }

    public static bool GetInfoFromIMDB(IMDB.IProgress progress, ref IMDBMovie movieDetails, bool isFuzzyMatching,
                                       bool getActors)
    {
      string file, path, filename;
      path = movieDetails.Path;
      filename = movieDetails.File;
      if (path != string.Empty)
      {
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
        file = path + Path.DirectorySeparatorChar + filename;
      }
      else
      {
        file = filename;
      }

      int id = movieDetails.ID;
      if (id < 0)
      {
        if (File.Exists(file))
        {
          Log.Info("Adding file:{0}", file);
          id = VideoDatabase.AddMovieFile(file);
        }
        else
          Log.Info("File doesn't exists. So no info is stored in db.");
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.VideoExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true);
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            continue;
          }
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
        movieDetails.ID = id;
      }
      if (RefreshIMDB(progress, ref movieDetails, isFuzzyMatching, getActors))
      {
        if (movieDetails != null)
        {
          return true;
        }
      }
      return false;
    }
  }
}