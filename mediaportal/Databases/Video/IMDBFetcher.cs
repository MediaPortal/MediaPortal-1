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
  public class IMDBFetcher : IMDB.IProgress
  {
    private string _movieName;
    private IMDB.IMDBUrl _url;
    private IMDB _imdb;
    private IMDBMovie _movieDetails;
    private Thread _movieThread;
    private Thread _detailsThread;
    private Thread _actorsThread;
    private Thread _actorThread;
    private IMDB.IProgress _progress;
    private bool _disableCancel;
    private bool _getActors;
    private static bool _currentCreateVideoThumbs; // Original setting for thumbnail creation
    private String _actor;
    private IMDBActor _imdbActor;
    private int _actorId;
    private int _actorIndex;
    private static bool _foldercheck = true;
    private ArrayList _nfoFiles;
    private bool _addToDatabase = true;
    private bool _skipExisting;
    private bool _refreshdbOnly;
    
    public IMDBFetcher(IMDB.IProgress progress)
    {
      _imdb = new IMDB(this);
      _progress = progress;
    }

    #region Movie Fetch

    public bool Fetch(string movieName)
    {
      _movieName = movieName;
      
      if (movieName == string.Empty)
      {
        return true;
      }
      
      if (!OnSearchStarting(this))
      {
        return false;
      }
      
      _movieThread = new Thread(FetchThread);
      _movieThread.Name = "IMDBFetcher";
      _movieThread.IsBackground = true;
      _movieThread.Start();

      if (!OnSearchStarted(this))
      {
        CancelFetch();
        return false;
      }
      return true;
    }

    private void FetchThread()
    {
      try
      {
        _disableCancel = false;
        
        if (_movieName == null)
        {
          return;
        }

        _imdb.Find(_movieName);
      }
      catch (ThreadAbortException) {}
      finally
      {
        OnSearchEnd(this);
        _disableCancel = false;
        _movieThread = null;
      }
    }

    #endregion

    #region NfoFetch

    public void FetchNfo(ArrayList nfoFiles, bool skipExisting, bool refreshdbOnly)
    {
      if (nfoFiles == null || nfoFiles.Count == 0)
      {
        return;
      }

      if (!OnSearchStarting(this))
      {
        return;
      }

      _skipExisting = skipExisting;
      _refreshdbOnly = refreshdbOnly;
      _nfoFiles = nfoFiles;
      _movieThread = new Thread(FetchNfoThread);
      _movieThread.Name = "NfoFetcher";
      _movieThread.IsBackground = true;
      _movieThread.Start();

      if (!OnSearchStarted(this))
      {
        CancelFetch();
      }
    }

    private void FetchNfoThread()
    {
      try
      {
        _disableCancel = false;

        foreach (string nfoFile in _nfoFiles)
        {
          OnProgress(nfoFile, string.Empty, string.Empty, 0);
          VideoDatabase.ImportNfo(nfoFile, _skipExisting, _refreshdbOnly);
        }
      }
      catch (ThreadAbortException) { }
      finally
      {
        OnSearchEnd(this);
        _disableCancel = false;
        _movieThread = null;
      }
    }

    #endregion

    #region Fetch Movie details

     public bool FetchDetails(int selectedMovie, ref IMDBMovie currentMovie)
     {
       return FetchDetails(selectedMovie, ref currentMovie, true);
     }

    // Movie details fetch/refresh
    public bool FetchDetails(int selectedMovie, ref IMDBMovie currentMovie, bool addToDatabase)
    {
      try
      {
        _movieDetails = currentMovie;
        _addToDatabase = addToDatabase;
        
        if ((selectedMovie < 0) || (selectedMovie >= Count))
        {
          return true;
        }

        _url = this[selectedMovie];
        _movieName = _url.Title;
        
        if (!OnDetailsStarting(this))
        {
          return false;
        }
        
        _detailsThread = new Thread(FetchDetailsThread);
        _detailsThread.IsBackground = true;
        _detailsThread.Name = "IMDBDetails";
        _detailsThread.Start();
      }
      catch (Exception ex)
      {
        Log.Error("Fetch movie details err:{0} src:{2}, stack:{1}", ex.Message, ex.Source, ex.StackTrace);
        CancelFetchDetails();
        return false;
      }
      if (!OnDetailsStarted(this))
      {
        CancelFetchDetails();
        return false;
      }
      return true;
    }
    
    private void FetchDetailsThread()
    {
      try
      {
        _disableCancel = false;
        
        if (_movieDetails == null)
        {
          _movieDetails = new IMDBMovie();
        }
        
        if (_url == null)
        {
          return;
        }
        
        // Progress bar visualization (GUI and Config), not neccessary as we see text but it looks better then text only
        // Action steps in code for which we want to see progress increase
        // Action 0-1 - Movie details fetch (20%)
        // Action 1-2 - IMPAw or TMDB search (40%)
        // Action 2-3 - FanArt download & actors fetch (60%)
        // Action 3-4 - Actor fetch (80%)
        // Action 3-4 - End (100%)
        
        string line1 = GUILocalizeStrings.Get(198);
        OnProgress(line1, _url.Title, string.Empty, 0);
        
        if (_imdb.GetDetails(_url, ref _movieDetails))
        {
          //percent = percent + step; // **Progress bar downloading details end
          OnProgress(line1, _url.Title, string.Empty, 20);
          
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
            string tmpTitle = _movieDetails.Title;
            Util.Utils.StripMovieNamePrefix(ref tmpTitle, true);
            _movieDetails.Title = tmpTitle;
          }

          #region Covers

          //
          // Covers - If cover is not empty don't change it, else download new
          //
          // Local cover check (every movie is in it's own folder), lookin' for folder.jpg
          // or look for local cover named as movie file
          string localCover = string.Empty; // local cover named as movie filename
          string movieFile = string.Empty;
          string moviePath = _movieDetails.Path;
          string titleExt = string.Empty;
          string largeCoverArt = string.Empty;
          string coverArt = string.Empty;

          if (_addToDatabase)
          {
            // Find movie file(s)
            ArrayList files = new ArrayList();
            VideoDatabase.GetFilesForMovie(_movieDetails.ID, ref files);

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
                _movieDetails.ThumbURL = "file://" + folderCover;
              }
              else if (File.Exists(localCover))
              {
                _movieDetails.ThumbURL = "file://" + localCover;
              }
            }
              // Try local movefilename.jpg
            else if (File.Exists(localCover))
            {
              _movieDetails.ThumbURL = "file://" + localCover;
            }

            // No local or scraper thumb
            if (_movieDetails.ThumbURL == string.Empty)
            {
              // **Progress bar message cover search IMPAw start
              line1 = GUILocalizeStrings.Get(928) + ": IMP Awards";
              OnProgress(line1, _url.Title, string.Empty, 20);

              // Added IMDBNumber parameter for movie cover check
              // This number is checked on HTML cover source page, if it's equal then this is the cover for our movie

              // IMPAwards
              IMPAwardsSearch impSearch = new IMPAwardsSearch();
              impSearch.SearchCovers(_movieDetails.Title, _movieDetails.IMDBNumber);

              if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
              {
                _movieDetails.ThumbURL = impSearch[0];

                // **Progress bar message for IMPAw end
                OnProgress(line1, _url.Title, string.Empty, 40);
              }

              // If no IMPAwards lets try TMDB 
              TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();

              if (impSearch.Count == 0)
              {
                line1 = GUILocalizeStrings.Get(928) + ": TMDB"; // **Progress bar message for TMDB start
                OnProgress(line1, _url.Title, string.Empty, 20);

                tmdbSearch.SearchCovers(_movieDetails.Title, _movieDetails.IMDBNumber);

                if ((tmdbSearch.Count > 0) && (tmdbSearch[0] != string.Empty))
                {
                  _movieDetails.ThumbURL = tmdbSearch[0];
                  // **Progress bar message for TMDB end
                  OnProgress(line1, _url.Title, string.Empty, 40);
                }
              }
              // All fail, last try IMDB
              if (impSearch.Count == 0 && tmdbSearch.Count == 0)
              {
                // **Progress bar message for IMDB start
                line1 = GUILocalizeStrings.Get(928) + ": IMDB";
                OnProgress(line1, _url.Title, string.Empty, 20);

                IMDBSearch imdbSearch = new IMDBSearch();
                imdbSearch.SearchCovers(_movieDetails.IMDBNumber, true);

                if ((imdbSearch.Count > 0) && (imdbSearch[0] != string.Empty))
                {
                  _movieDetails.ThumbURL = imdbSearch[0];
                  // **Progress bar message for IMDB end
                  OnProgress(line1, _url.Title, string.Empty, 40);
                }
              }
            }

            titleExt = _movieDetails.Title + "{" + _movieDetails.ID + "}";
            largeCoverArt = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
            coverArt = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);

            if (_movieDetails.ID >= 0)
            {
              if (!string.IsNullOrEmpty(_movieDetails.ThumbURL))
              {
                Util.Utils.FileDelete(largeCoverArt);
                Util.Utils.FileDelete(coverArt);
                line1 = GUILocalizeStrings.Get(1009);
              }
              OnProgress(line1, _url.Title, string.Empty, 40); // **Too fast so leave percent
            }
          }

          #endregion

          #region Fanart

          // FanArt grab
          if (useFanArt && _addToDatabase)
          {
            // **Progress bar message fanart start
            line1 = GUILocalizeStrings.Get(921) + ": Fanart";
            OnProgress(line1, _url.Title, string.Empty, 40);
            
            string localFanart = string.Empty;
            FanArt fanartSearch = new FanArt();
            
            // Check local fanart (only if every movie is in it's own folder), lookin for fanart.jpg
            // Looking for fanart.jpg or videofilename-fanart.jpg
            if (folderTitle)
            {
              localFanart = moviePath + @"\fanart.jpg";
              
              if (File.Exists(localFanart))
              {
                _movieDetails.FanartURL = "file://" + localFanart;
              }
              else
              {
                localFanart = moviePath + @"\" + Util.Utils.GetFilename(movieFile, true) + @"-fanart.jpg";
                
                if (File.Exists(localFanart))
                {
                  _movieDetails.FanartURL = "file://" + localFanart;
                }
              }
            }
            else
            {
              localFanart = moviePath + @"\" + Util.Utils.GetFilename(movieFile, true) + @"-fanart.jpg";
              
              if (File.Exists(localFanart))
              {
                _movieDetails.FanartURL = "file://" + localFanart;
              }
            }
            
            if (_movieDetails.FanartURL == string.Empty || _movieDetails.FanartURL == Strings.Unknown)
            {
              fanartSearch.GetTmdbFanartByApi
                (_movieDetails.ID, _movieDetails.IMDBNumber, _movieDetails.Title, true, faCount, string.Empty);
              // Set fanart url to db
              _movieDetails.FanartURL = fanartSearch.DefaultFanartUrl;
            }
            else // Local file or user url
            {
              fanartSearch.GetLocalFanart(_movieDetails.ID, _movieDetails.FanartURL, 0);
            }
            
            // **Progress bar message fanart End
            OnProgress(line1, _url.Title, string.Empty, 60);
          }

          #endregion

          #region Actors

          if (VideoDatabase.CheckMovieImdbId(_movieDetails.IMDBNumber) && _addToDatabase)
          {
            line1 = GUILocalizeStrings.Get(344); // **Progress bar actors start sets actual value to 0
            OnProgress(line1, _url.Title, string.Empty, 60);
            FetchActorsInMovie();
          }

          #endregion

          OnDisableCancel(this);

          #region Download & save covers, save movie info

          if (_movieDetails.ID >= 0 && _addToDatabase)
          {
            // **Progress bar downloading cover art, final step
            line1 = GUILocalizeStrings.Get(1009); 
            OnProgress(line1, _url.Title, string.Empty, 80);
            
            // Save cover thumbs
            if (!string.IsNullOrEmpty(_movieDetails.ThumbURL))
            {
              DownloadCoverArt(Thumbs.MovieTitle, _movieDetails.ThumbURL, titleExt);
            }
            
            // Set folder.jpg for ripped DVDs
            try
            {
              string path = _movieDetails.Path;
              string filename = _movieDetails.File;

              if (filename.ToUpper() == "VIDEO_TS.IFO" || filename.ToUpper() == "INDEX.BDMV")
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

            if (VideoDatabase.GetmovieWatchedStatus(_movieDetails.ID, out percentage, out timesWatched))
            {
              _movieDetails.Watched = 1;
            }

            // Save movie info
            VideoDatabase.SetMovieInfoById(_movieDetails.ID, ref _movieDetails, true);

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

                  if (values.Count > 0 && values.Contains(_movieDetails.ID.ToString()))
                  {
                    VideoDatabase.AddUserGroupToMovie(_movieDetails.ID, VideoDatabase.AddUserGroup(group));
                  }
                }
                catch (Exception)
                {
                }
              }
            }
            OnProgress(line1, _url.Title, string.Empty, 100); // **Progress bar end details
          }

          #endregion

        }
        else
        {
          line1 = GUILocalizeStrings.Get(416);
          OnProgress(line1, _url.Title, string.Empty, 100); // **Progress bar end (no details)
          _movieDetails = null;
        }
      }
      catch (ThreadAbortException) {}
      finally
      {
        OnDetailsEnd(this);
        _disableCancel = false;
         _detailsThread = null;
      }
    }

    #endregion

    #region All movie Actors fetch/refresh

    public static bool FetchMovieActors(IMDB.IProgress progress, IMDBMovie details)
    {
      IMDBFetcher fetcher = new IMDBFetcher(progress);
      fetcher._movieDetails = details;
      return fetcher.FetchActors();
    }

    public bool FetchActors()
    {
      if (!OnActorsStarting(this))
      {
        return false;
      }
      
      _actorsThread = new Thread(FetchActorsThread);
      _actorsThread.IsBackground = true;
      _actorsThread.Name = "IMDBActors";
      _actorsThread.Start();
      
      if (!OnActorsStarted(this))
      {
        CancelFetchActors();
        return false;
      }
      
      return true;
    }

    private void FetchActorsThread()
    {
      try
      {
        FetchActorsInMovie();
      }
      catch (ThreadAbortException) { }
      finally
      {
        OnDetailsEnd(this);
        
        if (_actorsThread.IsAlive)
        {
          _actorsThread.Abort();
        }

        _actorsThread = null;
        _disableCancel = false;
      }
    }

    private void FetchActorsInMovie()
    {
      ArrayList actors = new ArrayList();
      
      if (_movieDetails == null)
      {
        return;
      }

      // Check for IMDBid 
      if (VideoDatabase.CheckMovieImdbId(_movieDetails.IMDBNumber))
      {
        // Returns nm1234567 as actor name (IMDB actorID)
        IMDBSearch imdbActors = new IMDBSearch();
        imdbActors.SearchActors(_movieDetails.IMDBNumber, ref actors);
      }
      else
      {
        return;
      }
      
      if (actors.Count > 0)
      {
        // Clean old actors for movie
        VideoDatabase.RemoveActorsForMovie(_movieDetails.ID);

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

          VideoDatabase.AddActorToMovie(_movieDetails.ID, actorId, role);
          
          // Update director in movieinfo
          if (director)
          {
            _movieDetails.DirectorID = actorId;
            _movieDetails.Director = actorName;
            VideoDatabase.SetMovieInfoById(_movieDetails.ID, ref _movieDetails);
          }
        }
      }
    }
    
    #endregion

    #region Single Actor info fetch/refresh

    /// <summary>
    /// Downloads actor info.
    /// Movie details can be empty (it is used to help update role for movie if role is empty)
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="details"></param>
    /// <param name="actor"></param>
    /// <param name="actorId"></param>
    public static IMDBActor FetchMovieActor(IMDB.IProgress progress, IMDBMovie details, string actor, int actorId)
    {
      if (actor == string.Empty)
        return null;
      
      IMDBFetcher fetcher = new IMDBFetcher(progress);
      fetcher._movieDetails = details;
      // Find actor
      IMDB imdb = new IMDB();

      // Don't search for actor if name is IMDBactorId (little speed up)
      if (!VideoDatabase.CheckActorImdbId(actor))
      {
        imdb = fetcher.FindActor(actor);

        // Check for results
        if (imdb.Count > 0)
        {
          int i = 0;

          // If more than 1, invoke selection
          if (imdb.Count > 1)
          {
            if (!fetcher.OnSelectActor(fetcher, out i))
            {
              return null;
            }
          }
          // Fetch actor details
          return fetcher.FetchActorDetails(actor, actorId, i);
        }
      }
      else // Direct get actor details (by actorImdbId) as name
      {
        fetcher._imdb.SetIMDBActor("http://www.imdb.com/name/" + actor, actor);
        return fetcher.FetchActorDetails(actor, actorId, 0);
      }
      return null;
    }
    
    public IMDB FindActor(string actor)
    {
      if (!OnActorInfoStarting(this))
      {
        return _imdb;
      }

      _actor = actor;
      _actorThread = new Thread(FindActorThread);
      _actorThread.IsBackground = true;
      _actorThread.Name = "IMDBSingleActor";
      _actorThread.Start();
      
      //if (!OnActorInfoStarting(this))
      if (OnActorsStarted(this))
      {
        CancelFetchActor();
        return _imdb;
      }
      
      return _imdb;
    }

    private void FindActorThread()
    {
      try
      {
        FindActor();
      }
      catch (ThreadAbortException) { }
      finally
      {
        OnDetailsEnd(this);
        
        if (_actorThread.IsAlive)
        {
          _actorThread.Abort();
        }

        _actorThread = null;
        _disableCancel = false;
      }
    }

    private void FindActor()
    {
      _imdb = new IMDB();
      string line1 = GUILocalizeStrings.Get(197); ; //Querying IMDB info.
      string line2 = string.Empty;
      string line3 = string.Empty;
      OnProgress(line1, line2, line3, -1);
      _imdb.FindActor(_actor);
    }

    #endregion

    #region Single Actor Details

    public IMDBActor FetchActorDetails(string actor, int actorId, int actorIndex)
    {
      if (!OnActorInfoStarting(this))
      {
        return _imdbActor;
      }
      _actor = actor;
      _actorId = actorId;
      _actorIndex = actorIndex;

      if(_actorThread != null)
      {
        _actorThread.Abort();
        _actorThread = null;
      }

      _actorThread = new Thread(FetchActorDetailsThread);
      _actorThread.IsBackground = true;
      _actorThread.Name = "IMDBSingleActorDetails";
      _actorThread.Start();

      if (!OnActorsStarted(this))
      {
        CancelFetchActor();
        return _imdbActor;
      }
      
      return _imdbActor;
    }

    private void FetchActorDetailsThread()
    {
      try
      {
        FetchActorDetails();
      }
      catch (ThreadAbortException) { }
      finally
      {
        OnDetailsEnd(this);
        
        if (_actorThread.IsAlive)
        {
          _actorThread.Abort();
        }

        _actorThread = null;
        _disableCancel = false;
      }
    }

    private void FetchActorDetails()
    {
      string line1 = _actor;
      string line2 = string.Empty;
      string line3 = string.Empty;
      OnProgress(line1, line2, line3, -1);
      _imdb.GetActorDetails(_imdb[_actorIndex], out _imdbActor);

      // Try to update role
      for (int j = 0; j < _imdbActor.Count; j++)
      {
        string actorRole = _imdbActor[j].Role;
        string actorMovieId = _imdbActor[j].MovieImdbID;
        
        if (actorMovieId == _movieDetails.IMDBNumber)
        {
          if (!string.IsNullOrEmpty(actorRole))
          {
            VideoDatabase.AddActorToMovie(_movieDetails.ID, _actorId, actorRole);
          }

          break;
        }
      }

      // Update ActorImdbId
      string sql = string.Format("update Actors set IMDBActorId='{0}' where idActor ={1}",
                                  _imdbActor.IMDBActorID,
                                  _actorId);
      bool error = false;
      string errorMessage = string.Empty;
      VideoDatabase.ExecuteSql(sql, out error, out errorMessage);

      // Keep user actor image
      bool userActorImage = false;
      IMDBActor tmpActor = new IMDBActor();
      tmpActor = VideoDatabase.GetActorInfo(_actorId);

      if (tmpActor != null && tmpActor.ThumbnailUrl.StartsWith("file://"))
      {
        _imdbActor.ThumbnailUrl = tmpActor.ThumbnailUrl;
        userActorImage = true;
      }

      VideoDatabase.SetActorInfo(_actorId, _imdbActor);
      
      // Actor thumbs
      if (!string.IsNullOrEmpty(_imdbActor.ThumbnailUrl) && !userActorImage)
      {
        string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, _actorId.ToString());
        string coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieActors, _actorId.ToString());
        Util.Utils.FileDelete(largeCoverArtImage);
        Util.Utils.FileDelete(coverArtImage);
        line1 = _actor;
        line2 = GUILocalizeStrings.Get(1009); //Downloading cover art
        line3 = string.Empty;
        OnProgress(line1, line2, line3, -1);
        DownloadCoverArt(Thumbs.MovieActors, _imdbActor.ThumbnailUrl, _actorId.ToString());
      }
      else
      {
        if (!userActorImage)
        {
          // Sometimes we can have wrong actor pic (wrong actor selected in list before) so delete it
          string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, _actorId.ToString());
          string coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieActors, _actorId.ToString());
          Util.Utils.FileDelete(largeCoverArtImage);
          Util.Utils.FileDelete(coverArtImage);
        }
      }
      _imdbActor = VideoDatabase.GetActorInfo(_actorId);
    }

    #endregion
    
    public void CancelFetchActors()
    {
      if (_actorsThread == null)
      {
        return;
      }
      if ((_actorsThread.IsAlive) && (!_disableCancel))
      {
        //Log.Info("Aborting Thread for Fetching:{0}", actorsThread.ManagedThreadId);
        _actorsThread.Abort();
      }
    }

    public void CancelFetchActor()
    {
      if (_actorThread == null)
      {
        return;
      }
      if ((_actorThread.IsAlive) && (!_disableCancel))
      {
        //Log.Info("Aborting Thread for Fetching:{0}", actorsThread.ManagedThreadId);
        _actorThread.Abort();
      }
    }

    public void CancelFetch()
    {
      if (_movieThread == null)
      {
        return;
      }
      if ((_movieThread.IsAlive) && (!_disableCancel))
      {
        //Log.Info("Aborting Thread for Fetching:{0}", movieThread.ManagedThreadId);
        _movieThread.Abort();
      }
    }

    public void CancelFetchDetails()
    {
      if (_detailsThread == null)
      {
        return;
      }
      if ((_detailsThread.IsAlive) && (!_disableCancel))
      {
        //Log.Info("Aborting Thread for Fetching:{0}", detailsThread.ManagedThreadId);
        _detailsThread.Abort();
      }
    }

    // MovieName for the search
    public string MovieName
    {
      get { return _movieName; }
      set { _movieName = value; }
    }

    public string ActorName
    {
      get { return _actor; }
    }

    // count the elements
    public int Count
    {
      get { return _imdb.Count; }
    }

    // URL for the details
    public IMDB.IMDBUrl URL
    {
      get { return _url; }
      set { _url = value; }
    }

    // Movie the elements
    public IMDBMovie Movie
    {
      get { return _movieDetails; }
      set { _movieDetails = value; }
    }

    public IMDB.IMDBUrl this[int index]
    {
      get { return _imdb[index]; }
    }

    public int FuzzyMatch(string name)
    {
      int matchingIndex = -1;
      int matchingDistance = int.MaxValue;
      bool isAmbiguous = false;
      name = _imdb.GetSearchString(name);

      for (int index = 0; index < _imdb.Count; ++index)
      {
        int distance = Levenshtein.Match(name, _imdb[index].Title.ToLower());

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
      _disableCancel = true;
      if (_progress != null)
      {
        return _progress.OnDisableCancel(fetcher);
      }
      return false;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (_progress != null)
      {
        _progress.OnProgress(line1, line2, line3, percent);
      }
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnSearchStarted(fetcher);
      }
      return false;
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnSearchStarting(fetcher);
      }
      return false;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnSearchEnd(fetcher);
      }
      return false;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnMovieNotFound(fetcher);
      }
      return false;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnDetailsStarted(fetcher);
      }
      return false;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnDetailsStarting(fetcher);
      }
      return false;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnDetailsEnd(fetcher);
      }
      return false;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnActorsStarted(fetcher);
      }
      return false;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnActorsStarting(fetcher);
      }
      return false;
    }

    public bool OnActorInfoStarting(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnActorInfoStarting(fetcher);
      }
      return false;
    }

    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnActorsEnd(fetcher);
      }
      return false;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      if (_progress != null)
      {
        return _progress.OnDetailsNotFound(fetcher);
      }
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      if (_progress != null)
      {
        return _progress.OnRequestMovieTitle(fetcher, out movieName);
      }
      movieName = string.Empty;
      return false;
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selected)
    {
      if (_progress != null)
      {
        return _progress.OnSelectMovie(fetcher, out selected);
      }
      selected = -1;
      return false;
    }

    public bool OnSelectActor(IMDBFetcher fetcher, out int selected)
    {
      if (_progress != null)
      {
        return _progress.OnSelectActor(fetcher, out selected);
      }
      selected = -1;
      return false;
    }

    public bool OnScanStart(int total)
    {
      if (_progress != null)
      {
        return _progress.OnScanStart(total);
      }
      return true;
    }

    public bool OnScanEnd()
    {
      if (_progress != null)
      {
        return _progress.OnScanEnd();
      }
      return true;
    }

    public bool OnScanIterating(int count)
    {
      if (_progress != null)
      {
        return _progress.OnScanIterating(count);
      }
      return true;
    }

    public bool OnScanIterated(int count)
    {
      if (_progress != null)
      {
        return _progress.OnScanIterated(count);
      }
      return true;
    }

    #endregion

    /// <summary>
    /// Download IMDB info for a movie. For existing movie using IMDBid from database.
    /// </summary>
    public static bool RefreshIMDB(IMDB.IProgress progress, ref IMDBMovie currentMovie, bool fuzzyMatching,
                                   bool getActors, bool addToDatabase)
    {
      if (currentMovie.Title != string.Empty || currentMovie.SearchString != string.Empty)
      {
        Log.Info("RefreshIMDB() - Refreshing MovieInfo for {0}-{1}", currentMovie.Title, currentMovie.SearchString);
      }

      string strMovieName = currentMovie.SearchString;
      string strFileName = string.Empty;
      string path = currentMovie.Path;
      string filename = currentMovie.File;
      _foldercheck = Util.Utils.IsFolderDedicatedMovieFolder(path);

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
            // DVD
            string strDrive = strFileName.Substring(0, 2);
            currentMovie.DVDLabel = Util.Utils.GetDriveName(strDrive);
            strMovieName = currentMovie.DVDLabel;
          }
          else if (strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
          {
            // DVD folder
            string dvdFolder = strFileName.Substring(0, strFileName.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
            currentMovie.DVDLabel = Path.GetFileName(dvdFolder);
            strMovieName = currentMovie.DVDLabel;
          }
          else if (strFileName.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV") >= 0)
          {
            // BD folder
            string bdFolder = strFileName.Substring(0, strFileName.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV"));
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
                if (_foldercheck == false && pattern[i].IsMatch(strMovieName))
                {
                  strMovieName = pattern[i].Replace(strMovieName, "");
                }
              }
            }
          }
        }
        if ((strMovieName == string.Empty) || (strMovieName == Strings.Unknown))
        {
          return true;
        }
      }
      if (currentMovie.ID == -1 && addToDatabase)
      {
        currentMovie.ID = VideoDatabase.AddMovieFile(strFileName);
      }

      if (addToDatabase)
      {
        SearchForImdbId(path, filename, _foldercheck, ref strMovieName);
      }

      Util.Utils.RemoveStackEndings(ref strMovieName);
      currentMovie.SearchString = strMovieName;
      
      if (currentMovie.ID >= 0 || !addToDatabase)
      {
        if (!Win32API.IsConnectedToInternet())
        {
          return false;
        }
        IMDBFetcher fetcher = new IMDBFetcher(progress);
        fetcher.Movie = currentMovie;
        fetcher._getActors = getActors;
        int selectedMovie = -1;
        do
        {
          if (!fetcher.Fetch(strMovieName))
          {
            return false;
          }
          if (fuzzyMatching)
          {
            IMDB tmpImdb = new IMDB();
            selectedMovie = fetcher.FuzzyMatch(tmpImdb.GetSearchString(fetcher.MovieName));
            if (selectedMovie == -1 && fetcher.Count > 0)
            {
              if (!fetcher.OnSelectMovie(fetcher, out selectedMovie))
              {
                return false;
              }
              if (selectedMovie == -1)
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
            else if (selectedMovie == -1)
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
              
              // EPG - one result, get movie without ask
              if (iMoviesFound == 1 && !addToDatabase)
              {
                selectedMovie = 0;
                break;
              }
              
              //GEMX 28.03.08: There should always be a choice to enter the movie manually 
              //               in case the 1 and only found name is wrong
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
        
        if (!fetcher.FetchDetails(selectedMovie, ref currentMovie, addToDatabase))
        {
          return false;
        }
        
        IMDBMovie movieDetails = fetcher.Movie;

        if (movieDetails != null)
        {
          Log.Info("RefreshIMDB() - Found movie and added info for: {0} (Year: {1})",
                   movieDetails.Title, movieDetails.Year);

          movieDetails.SearchString = strMovieName;
          currentMovie = movieDetails;
          return true;
        }
        
        if (fetcher.Movie == null)
        {
          fetcher.Movie = currentMovie;
        }
        
        return fetcher.OnDetailsNotFound(fetcher);
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
            //Util.Utils.DownLoadAndCacheImage(imageUrl, temporaryFilename);
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
    /// Download IMDB info for all movies in a collection of paths
    /// </summary>
    public static bool ScanIMDB(IMDB.IProgress progress, ArrayList paths, bool fuzzyMatching, bool skipExisting,
                                bool getActors, bool refreshDBonly)
    {
      bool success = true;
      ArrayList availableFiles = new ArrayList();
      
      foreach (string path in paths)
      // Caution - Thumb creation spam no1 starts in CountFiles
      {
        VideoDatabase.GetVideoFiles(path, ref availableFiles);
      }
      
      if (progress != null)
      {
        progress.OnScanStart(availableFiles.Count);
      }

      int count = 1;
      
      foreach (string file in availableFiles)
      {
        if ((progress != null) && (!progress.OnScanIterating(count)))
        {
          success = false;
          break;
        }
        // Test pattern (CD, DISK, Part, X-Y...) and extrude last digit as set number check
        // Lets kill double check for the same movie if it consists from more than one file
        int digit = 0; // Only first file in set will proceed (cd1, part1, dvd1...)

        var pattern = Util.Utils.StackExpression();
        int stackSequence = 0; // seq 0 = [x-y], seq 1 = CD1, Part1....
        
        for (int i = 0; i < pattern.Length; i++)
        {
          if (pattern[i].IsMatch(file))
          {
            digit = Convert.ToInt16(pattern[i].Match(file).Groups["digit"].Value);
            stackSequence = i;
            break;
          }
        }
        try
        {
          IMDBMovie movieDetails = new IMDBMovie();
          int id = VideoDatabase.GetMovieInfo(file, ref movieDetails);

          if (refreshDBonly && id != -1 && digit < 2)
          {
            string path, filename;
            Util.Utils.Split(file, out path, out filename);
            movieDetails.Path = path;
            movieDetails.File = filename;
            // Caution - Thumb creation spam no2 starts in GetInfoFromIMDB
            GetInfoFromIMDB(progress, ref movieDetails, fuzzyMatching, getActors);
          }
          else
          {
            if ((!skipExisting || id == -1) && refreshDBonly == false && digit < 2)
            {
              string path, filename;
              Util.Utils.Split(file, out path, out filename);
              movieDetails.Path = path;
              movieDetails.File = filename;
              // Caution - Thumb creation spam no2 starts in GetInfoFromIMDB
              GetInfoFromIMDB(progress, ref movieDetails, fuzzyMatching, getActors);
            }
            else if (digit > 1) // Add DVD or BD stack folders to existing movie
            {
              string path, filename;
              string tmpPath = string.Empty;
              DatabaseUtility.Split(file, out path, out filename);

              if (path.ToUpperInvariant().Contains(@"\VIDEO_TS") || path.ToUpperInvariant().Contains(@"\BDMV"))
              {
                try
                {
                  if (stackSequence == 0)
                  {
                    string strReplace = "[" + digit;
                    int stackIndex = path.LastIndexOf(strReplace);
                    tmpPath = path.Remove(stackIndex, 2);
                    tmpPath = tmpPath.Insert(stackIndex, "[1");
                  }
                  else
                  {
                    int stackIndex = path.LastIndexOf(digit.ToString());
                    tmpPath = path.Remove(stackIndex, 1);
                    tmpPath = tmpPath.Insert(stackIndex, "1");
                  }

                  int movieId = VideoDatabase.GetMovieId(tmpPath + filename);
                  int pathId = VideoDatabase.AddPath(path);
                  VideoDatabase.AddFile(movieId, pathId, filename);
                }
                catch (Exception){}
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Scan IMDB err:{0} src:{1}, stack:{2}, ", ex.Message, ex.Source, ex.StackTrace);
          success = false;
          break;
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
    /// Download IMDB info for videofile (file must be with full path)
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="file"></param>
    /// <param name="fuzzyMatching"></param>
    /// <param name="skipExisting"></param>
    /// <param name="getActors"></param>
    /// <param name="refreshDBonly"></param>
    /// <returns></returns>
    public static bool ScanIMDB(IMDB.IProgress progress, string file, bool fuzzyMatching, bool skipExisting,
                                bool getActors, bool refreshDBonly)
    {
      bool success = true;
      
      if (progress != null)
      {
        progress.OnScanStart(1);
      }

      int digit = 0; // Only first file in set will proceed (cd1, part1, dvd1...)

      var pattern = Util.Utils.StackExpression();
      int stackSequence = 0; // seq 0 = [x-y], seq 1 = CD1, Part1....

      for (int i = 0; i < pattern.Length; i++)
      {
        if (pattern[i].IsMatch(file))
        {
          digit = Convert.ToInt16(pattern[i].Match(file).Groups["digit"].Value);
        }
      }
      try
      {
        IMDBMovie movieDetails = new IMDBMovie();
        int id = VideoDatabase.GetMovieInfo(file, ref movieDetails);

        if (refreshDBonly && id != -1 && digit < 2)
        {
          string path, filename;
          Util.Utils.Split(file, out path, out filename);
          movieDetails.Path = path;
          movieDetails.File = filename;
          // Caution - Thumb creation spam no2 starts in GetInfoFromIMDB
          GetInfoFromIMDB(progress, ref movieDetails, fuzzyMatching, getActors);
        }
        else
        {
          if ((!skipExisting || id == -1) && refreshDBonly == false && digit < 2)
          {
            string path, filename;
            Util.Utils.Split(file, out path, out filename);
            movieDetails.Path = path;
            movieDetails.File = filename;
            // Caution - Thumb creation spam no2 starts in GetInfoFromIMDB
            GetInfoFromIMDB(progress, ref movieDetails, fuzzyMatching, getActors);
          }
          else if (digit > 1) // Add DVD or BD stack folders to existing movie
          {
            string path, filename;
            string tmpPath = string.Empty;
            DatabaseUtility.Split(file, out path, out filename);

            if (path.ToUpperInvariant().Contains(@"\VIDEO_TS") || path.ToUpperInvariant().Contains(@"\BDMV"))
            {
              try
              {
                if (stackSequence == 0)
                {
                  string strReplace = "[" + digit;
                  int stackIndex = path.LastIndexOf(strReplace);
                  tmpPath = path.Remove(stackIndex, 2);
                  tmpPath = tmpPath.Insert(stackIndex, "[1");
                }
                else
                {
                  int stackIndex = path.LastIndexOf(digit.ToString());
                  tmpPath = path.Remove(stackIndex, 1);
                  tmpPath = tmpPath.Insert(stackIndex, "1");
                }

                int movieId = VideoDatabase.GetMovieId(tmpPath + filename);
                int pathId = VideoDatabase.AddPath(path);
                VideoDatabase.AddFile(movieId, pathId, filename);
              }
              catch (Exception) { }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Scan IMDB err:{0} src:{1}, stack:{2}, ", ex.Message, ex.Source, ex.StackTrace);
        success = false;
      }

      if (progress != null)
      {
        progress.OnScanEnd();
      }
      return success;
    }
    
    public static bool GetInfoFromIMDB(IMDB.IProgress progress, ref IMDBMovie movieDetails, bool isFuzzyMatching,
                                       bool getActors)
    {
      string file;
      string path = movieDetails.Path;
      string filename = movieDetails.File;
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

      bool addToDB = true;
      int id = movieDetails.ID;
      if (id < 0)
      {
        if (File.Exists(file))
        {
          id = VideoDatabase.AddMovieFile(file);

          VirtualDirectory dir = new VirtualDirectory();
          dir.SetExtensions(Util.Utils.VideoExtensions);
          // Thumb creation spam no2 causing this call
          //
          // Temporary disable thumbcreation
          //
          using (Settings xmlreader = new MPSettings())
          {
            _currentCreateVideoThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
          }
          using (Settings xmlwriter = new MPSettings())
          {
            xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", false);
          }
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
          // Restore thumbcreation setting
          using (Settings xmlwriter = new MPSettings())
          {
            xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", _currentCreateVideoThumbs);
          }

          movieDetails.ID = id;
        }
        else
        {
          Log.Info("File doesn't exists. So no info is stored in db.");
          getActors = false;
          addToDB = false;
        }
      }
      if (RefreshIMDB(progress, ref movieDetails, isFuzzyMatching, getActors, addToDB))
      {
        if (movieDetails != null)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">path without filename</param>
    /// <param name="filename">filename without path</param>
    /// <param name="dedicatedMovieFolders">is every movie in its own folder?</param>
    /// <param name="searchString">returns tt1234567 if success</param>
    private static void SearchForImdbId(string path, string filename, bool dedicatedMovieFolders, ref string searchString)
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

    private static bool MatchImdb(ref string strToMatch)
    {
      Match match = Regex.Match(strToMatch, @"tt[\d]{7}?", RegexOptions.IgnoreCase);
      
      if (match.Success)
      {
        strToMatch = match.Value;
        return true;
      }

      return false;
    }
  }
}