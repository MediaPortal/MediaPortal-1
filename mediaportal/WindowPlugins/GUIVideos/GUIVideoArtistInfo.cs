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
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIVideoArtistInfo : GUIInternalWindow, IRenderLayer
  {
    private IMDBActor _currentActor;
    private IMDBMovie _currentMovie;
    private ViewMode _viewmode;
    private Thread _scanThread;

    [SkinControl(3)] protected GUICheckButton btnBiography;
    [SkinControl(4)] protected GUICheckButton btnMovies;
    [SkinControl(20)] protected GUITextScrollUpControl tbBiographyArea;
    [SkinControl(21)] protected GUIImage imgCoverArt;
    [SkinControl(22)] protected GUITextControl tbMovieArea;
    [SkinControl(24)] protected GUIListControl listActorMovies;
    [SkinControl(25)] protected GUIImage imgMovieCover;
    [SkinControl(26)] protected GUITextScrollUpControl tbMoviePlot;

    #region Base Variables

    private int _actorIdState = -1; // Current session setting
    private string _viewModeState = string.Empty;
    private bool _movieInfoBeforePlay;
    private bool _playClicked;
    private int _currentSelectedItem = -1;
    
    private bool _forceRefreshAll; // Refresh all movies (context menu)
    
    #endregion

    #region Nested type: ViewMode

    private enum ViewMode
    {
      Biography,
      Movies,
    }

    #endregion

    public GUIVideoArtistInfo()
    {
      GetID = (int)Window.WINDOW_VIDEO_ARTIST_INFO;
    }

    public IMDBActor Actor
    {
      get { return _currentActor; }
      set { _currentActor = value; }
    }

    public IMDBMovie Movie
    {
      get { return _currentMovie; }
      set { _currentMovie = value; }
    }

    #region Overrides

    public override bool Init()
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        _movieInfoBeforePlay = xmlreader.GetValueAsBool("moviedatabase", "movieinfobeforeplay", false);
      }

      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\DialogVideoArtistInfo.xml"));
    }

    public override void PreInit() {}

    public override void OnAction(Action action)
    {
      // F3 key - IMDB info
      if (action.wID == Action.ActionType.ACTION_SHOW_INFO)
      {
        GUIListItem item = listActorMovies.SelectedListItem;

        if (!item.IsRemote)
        {
          OnMovieInfo(item);
        }
      }

      if (action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
      {
        _playClicked = true;
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      _forceRefreshAll = false;

      if (_currentActor == null)
      {
        if (GUIWindowManager.HasPreviousWindow())
        {
          GUIWindowManager.ShowPreviousWindow();
        }
        else
        {
          GUIWindowManager.CloseCurrentWindow();
        }
        return;
      }
      
      //_internalGrabber.LoadScript();
      _currentActor.SetProperties();
      string biography = _currentActor.Biography;

      if (biography == string.Empty || biography == Strings.Unknown)
      {
        biography = "";
        _viewmode = ViewMode.Movies;
      }
      else
      {
        _viewmode = ViewMode.Biography;
      }
      
      if (listActorMovies != null && listActorMovies.Count == 0)
      {
        SetNewproperties();
      }
      else
      {
        SetOldProperties();
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if ((_scanThread != null) && (_scanThread.IsAlive))
      {
        _scanThread.Abort();
        _scanThread = null;
      }
      
      // Refresh actor info
      if (_currentActor != null)
      {
        _currentActor = VideoDatabase.GetActorInfo(_currentActor.ID);
        SaveState();
        //Clean properties
        _currentActor.ResetProperties();
      }

      ReleaseResources();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      //
      // Movies
      //
      if (control == btnMovies)
      {
        _viewmode = ViewMode.Movies;
        Update();
      }
      //
      // Biography
      //
      if (control == btnBiography)
      {
        _viewmode = ViewMode.Biography;
        Update();
      }
      //
      // List control item
      //
      if (control == listActorMovies)
      {
        if (listActorMovies.SelectedListItem.IsPlayed)
        {
          try
          {
            if (_movieInfoBeforePlay && !_playClicked)
            {
              OnMovieInfo(listActorMovies.SelectedListItem);
              return;
            }

            _playClicked = false;
            GUIVideoFiles.PlayMovie(Convert.ToInt32(listActorMovies.SelectedListItem.DVDLabel), true);
          }
          catch { }
        }
        else
        {
          _playClicked = false;
          _forceRefreshAll = true;
          OnRefreshSingleMovie();
        }
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = listActorMovies.SelectedListItem;

      if (item == null)
      {
        return;
      }
      var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

      dlg.AddLocalizedString(1296); // Refresh all actor movies
      dlg.AddLocalizedString(1290); // Refresh selected movie

      if (item.IsPlayed) // Movie in MP database
      {
        dlg.AddLocalizedString(208); // Play
        dlg.AddLocalizedString(368); // IMDB
      }

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 1296: // Refresh All actor movies
          _forceRefreshAll = true;
          OnRefreshMovie();
          break;
        case 1290: // Refresh selected movie
          _forceRefreshAll = true;
          OnRefreshSingleMovie();
          break;
        case 208: // Play
          try 
          {
            GUIVideoFiles.PlayMovie(Convert.ToInt32(item.DVDLabel), true);
          }
          catch {}
          break;
        case 368: // IMDB
          OnMovieInfo(item);
          break;
      }
    }

    #endregion

    private IMDBActor.IMDBActorMovie ListItemMovieInfo(GUIListItem item)
    {
      IMDBActor.IMDBActorMovie movie = item.AlbumInfoTag as IMDBActor.IMDBActorMovie;
      return movie;
    }

    // Just because of compatibility with old skins (not needed in the future)
    private void SetOldProperties()
    {
      string movies = string.Empty;
      for (int i = 0; i < _currentActor.Count; ++i)
      {
        string line = String.Format("{0}. {1} ({2})\n            {3}\n", i + 1, _currentActor[i].MovieTitle,
                                    _currentActor[i].Year, _currentActor[i].Role);
        movies += line;
      }
      GUIPropertyManager.SetProperty("#Actor.Movies", movies);

      string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, _currentActor.ID.ToString());
      if (imgCoverArt != null)
      {
        imgCoverArt.Dispose();
        imgCoverArt.SetFileName(largeCoverArtImage);
        imgCoverArt.AllocResources();
      }
      Update();
    }

    private void SetNewproperties()
    {
      _currentActor.SortActorMoviesByYear();

      for (int i = 0; i < _currentActor.Count; ++i)
      {
        string line = String.Format("{0}. {1} ({2})",
                                    _currentActor[i].Year,
                                    _currentActor[i].MovieTitle,
                                    _currentActor[i].Role);
        //List view
        var item = new GUIListItem();
        item.ItemId = i;
        item.Label = line.Replace("()", string.Empty).Trim(); // Year+Title+Role (visible on screen item)
        
        if (_currentActor[i].MoviePlot == "-" || _currentActor[i].MoviePlot == Strings.Unknown)
        {
          _currentActor[i].MoviePlot = string.Empty; // Plot
        }
        item.AlbumInfoTag = Actor[i];

        string filenameL = string.Empty;
        string path = string.Empty;
        // Find image
        if (VideoDatabase.CheckMovieImdbId(_currentActor[i].MovieImdbID))
        {
          string ttFolder = Regex.Replace(_currentActor[i].MovieImdbID, "(tt(0*))", string.Empty);
          int i_ttFolder = 0;
          int.TryParse(ttFolder, out i_ttFolder);
          i_ttFolder = i_ttFolder / 25000; // 25000 thumbs in one folder
          ttFolder = i_ttFolder.ToString();
          path = string.Format(@"{0}\Videos\Actors\ActorsMovies\{1}\", Config.GetFolder(Config.Dir.Thumbs), ttFolder);
          filenameL = _currentActor[i].MovieImdbID + ".jpg";

          if (File.Exists(path + filenameL))
          {
            filenameL = path + filenameL; // Movie cover file
            item.IconImage = filenameL;
          }
          else
          {
            filenameL = string.Empty; // Movie cover file
            item.IconImage = string.Empty;
          }
        }
        
        // Show in list if user have that movie in collection (played property = true)
        ArrayList movies = new ArrayList();
        string sql = string.Format("SELECT * FROM movieinfo WHERE IMDBID = '{0}'", _currentActor[i].MovieImdbID);
        VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false, false);

        if (movies.Count > 0) // We have a movie, color normal or color played for watched
        {
          IMDBMovie movie = new IMDBMovie();
          movie = (IMDBMovie)movies[0];
          item.DVDLabel = movie.ID.ToString(); // DVD label holds videodatabase movieID
          item.IsPlayed = true;
        }

        item.ThumbnailImage = filenameL;
        item.OnItemSelected += OnItemSelected;
        listActorMovies.Add(item);
      }

      
      if (listActorMovies.ListItems.Count == 0)
      {
        GUIListItem item = new GUIListItem();
        item.Label = GUILocalizeStrings.Get(284);
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
        movie = new IMDBMovie();
        item.AlbumInfoTag = movie;
        listActorMovies.Add(item);
      }
      
      _currentSelectedItem = 0;
      string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, _currentActor.ID.ToString());

      if (imgCoverArt != null)
      {
        imgCoverArt.Dispose();
        imgCoverArt.SetFileName(largeCoverArtImage);
        imgCoverArt.AllocResources();
      }
      
      // Update skin controls visibility
      Update();
      
      // Restore screen from last session if needed
      LoadState();
    }

    private void Update()
    {
      if (_currentActor == null)
      {
        return;
      }

      // Movie mode
      if (_viewmode == ViewMode.Movies)
      {
        if (tbBiographyArea != null) tbBiographyArea.IsVisible = false;
        if (imgCoverArt != null) imgCoverArt.IsVisible = true;
        if (btnBiography != null) btnBiography.Selected = false;
        if (btnMovies != null)
        {
          btnMovies.Selected = true;
          btnMovies.Focus = false;
        }
        // ListActors
        if (listActorMovies != null)
        {
          listActorMovies.IsVisible = true;
          listActorMovies.Selected = true;
          GUIControl.FocusControl(GetID, listActorMovies.GetID);
          if (tbMovieArea != null) tbMovieArea.IsVisible = false;
          if (tbMoviePlot != null) tbMoviePlot.IsVisible = true;
          if (imgMovieCover != null) imgMovieCover.IsVisible = true;
          GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(_currentActor.Count));
          listActorMovies.SelectedListItemIndex = _currentSelectedItem;
          SelectItem();
        }
        else
        {
          if (tbMovieArea != null) tbMovieArea.IsVisible = true;
        }
      }

      // Biography mode
      if (_viewmode == ViewMode.Biography)
      {
        if (tbMovieArea != null) tbMovieArea.IsVisible = false;
        if (tbBiographyArea != null)
        {
          tbBiographyArea.IsVisible = true;
          tbBiographyArea.Focus = true;
        }
        if (imgCoverArt != null) imgCoverArt.IsVisible = true;
        if (btnBiography != null) btnBiography.Selected = true;
        if (btnMovies != null) btnMovies.Selected = false;
        // ListActors
        if (listActorMovies != null)
        {
          listActorMovies.IsVisible = false;
          listActorMovies.Selected = false;
          listActorMovies.Focus = false;
          if (tbMoviePlot != null) tbMoviePlot.IsVisible = false;
          if (imgMovieCover != null) imgMovieCover.IsVisible = false;
          _currentSelectedItem = listActorMovies.SelectedListItemIndex;
          GUIPropertyManager.SetProperty("#itemcount", string.Empty);
        }
      }
    }

    private void SelectItem()
    {
      if (_currentSelectedItem >= 0 && listActorMovies != null)
      {
        GUIControl.SelectItemControl(GetID, listActorMovies.GetID, _currentSelectedItem);
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {
        if (ListItemMovieInfo(item) != null)
        {
          GUIPropertyManager.SetProperty("#Actor.MoviePlot", ListItemMovieInfo(item).MoviePlot);
          GUIPropertyManager.SetProperty("#Actor.MovieImage", item.ThumbnailImage);
          GUIPropertyManager.SetProperty("#imdbnumber", ListItemMovieInfo(item).MovieImdbID);
          GUIPropertyManager.SetProperty("#Actor.MovieExtraDetails", GUILocalizeStrings.Get(199) + " " +
                                                                     ListItemMovieInfo(item).MovieCredits.Replace(" /",
                                                                                                                  ",") +
                                                                     "  |  " +
                                                                     GUILocalizeStrings.Get(174) + " " +
                                                                     ListItemMovieInfo(item).MovieGenre.Replace(" /",
                                                                                                                ",") +
                                                                     "  |  " +
                                                                     GUILocalizeStrings.Get(204) + " " +
                                                                     ListItemMovieInfo(item).MovieMpaaRating + "  |  " +
                                                                     GUILocalizeStrings.Get(344) + ": " +
                                                                     ListItemMovieInfo(item).MovieCast.Replace(" /", ","));
          GUIPropertyManager.SetProperty("#Actor.MovieTitle", ListItemMovieInfo(item).MovieTitle);
        }
        else
        {
          GUIPropertyManager.SetProperty("#Actor.MoviePlot", string.Empty);
          GUIPropertyManager.SetProperty("#Actor.MovieImage", string.Empty);
          GUIPropertyManager.SetProperty("#imdbnumber", string.Empty);
          GUIPropertyManager.SetProperty("#Actor.MovieExtraDetails", string.Empty);
          GUIPropertyManager.SetProperty("#Actor.MovieTitle", string.Empty);
        }
        // For fanart handler
        if (item.IsPlayed)
        {
          GUIPropertyManager.SetProperty("#movieid", item.DVDLabel);
        }
        else if (_currentMovie != null)
        {
          //GUIPropertyManager.SetProperty("#selecteditem", _currentMovie.Title);
          GUIPropertyManager.SetProperty("#movieid", _currentMovie.ID.ToString());
        }

      }
    }

    private void OnMovieInfo(GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      // Get movie info (item.DVDLabel holds movie id from videodatabase)
      IMDBMovie movie = new IMDBMovie();
      int movieId = -1;
      int.TryParse(item.DVDLabel, out movieId);
      VideoDatabase.GetMovieInfoById(movieId, ref movie);
      
      if (movie == null)
      {
        return;
      }
      
      if (movie.ID >= 0)
      {
        GUIVideoInfo videoInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_INFO);
        videoInfo.Movie = movie;
        videoInfo.FolderForThumbs = string.Empty;
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_VIDEO_INFO);
      }
    }

    // Manual refresh
    private void OnRefreshSingleMovie()
    {
      if ((_scanThread != null) && (_scanThread.IsAlive))
      {
        var dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        if (dlg == null)
        {
          return;
        }
        dlg.SetHeading(GUILocalizeStrings.Get(1020));
        dlg.SetLine(1, GUILocalizeStrings.Get(1291));
        dlg.SetLine(2, GUILocalizeStrings.Get(1292));
        dlg.DoModal(GetID);
        return;
      }

      GetSingleMovieDetails();
    }

    private void OnRefreshMovie()
    {
      if ((_scanThread != null) && (_scanThread.IsAlive))
      {
        var dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        if (dlg == null)
        {
          return;
        }
        dlg.SetHeading(GUILocalizeStrings.Get(1020));
        dlg.SetLine(1, GUILocalizeStrings.Get(1311)); // Refresh is already active.
        dlg.DoModal(GetID);
        return;
      }

      GetMovieDetails();
    }

    private void LoadState()
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        _viewModeState = xmlreader.GetValueAsString("VideoArtistInfo", "lastview", string.Empty);
        _actorIdState = xmlreader.GetValueAsInt("VideoArtistInfo", "actorid", -1);
        
        if (_currentActor.ID == _actorIdState)
        {
          if (_viewModeState == "Movies" &&
              GUIWindowManager.GetPreviousActiveWindow() != (int)Window.WINDOW_VIDEO_TITLE &&
              GUIWindowManager.GetPreviousActiveWindow() != (int)Window.WINDOW_VIDEO_INFO)
          {
            _viewmode = ViewMode.Movies;

            if (_currentSelectedItem >= 0 && listActorMovies != null && listActorMovies.Count >= _currentSelectedItem)
            {
              _currentSelectedItem = xmlreader.GetValueAsInt("VideoArtistInfo", "itemid", -1);
              Update();
              SelectItem();
            }
          }
          else
          {
            GetMovieDetails();
          }
        }
        else
        {
          if (_viewmode == ViewMode.Movies && listActorMovies != null && listActorMovies.Count >= 0)
          {
            _currentSelectedItem = 0;
            SelectItem();
          }
          
          GetMovieDetails();
        }
      }
    }

    private void SaveState()
    {
      using (Profile.Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("VideoArtistInfo", "lastview", _viewmode);
        xmlwriter.SetValue("VideoArtistInfo", "actorid", _currentActor.ID);

        if (listActorMovies != null)
        {
          xmlwriter.SetValue("VideoArtistInfo", "itemid", listActorMovies.SelectedListItemIndex);
        }
      }
    }

    private void ReleaseResources()
    {
      if (listActorMovies != null)
      {
        listActorMovies.Clear();
      }

      if (imgCoverArt != null)
      {
        imgCoverArt.Dispose();
      }

      if (imgMovieCover != null)
      {
        imgMovieCover.Dispose();
      }
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion

    #region Thread MovieDetails

    private void GetSingleMovieDetails()
    {
      _scanThread = new Thread(ThreadMainGetSingleMovieDetails);
      _scanThread.IsBackground = true;
      _scanThread.Start();
    }

    private void GetMovieDetails()
    {
      _scanThread = new Thread(ThreadMainGetDetails);
      _scanThread.IsBackground = true;
      _scanThread.Start();
    }

    #region Thread get movie info details

    private void ThreadMainGetSingleMovieDetails()
    {
      try
      {
        if (Win32API.IsConnectedToInternet())
        {
          int count = 1;
          int countCurrent = 1;
          GUIListItem item = listActorMovies.SelectedListItem;

          if (item != null)
          {
            // Show visible progress
            if (count > 0)
            {
              int percCount = (100 * countCurrent) / count;
              countCurrent += 1;
              GUIPropertyManager.SetProperty("#Actor.Name", _currentActor.Name + "   (" + percCount + @"%)");
            }
            //
            GetDetails(item);

            // Refresh item data if it's selected
            if (listActorMovies.SelectedListItem == item)
            {
              OnItemSelected(item, listActorMovies);
            }
          }

          // Reset variables and save state
          GUIPropertyManager.SetProperty("#Actor.Name", _currentActor.Name);
          _forceRefreshAll = false;
          SaveState();
        }
        else
        {
          Log.Info("VideoArtistInfo: Refreshing not possible. No Internet connection.");
          _forceRefreshAll = false;
        }
      }
      catch (ThreadAbortException)
      {

      }
    }

    private void ThreadMainGetDetails()
    {
      try
      {
        if (Win32API.IsConnectedToInternet())
        {
          int count = listActorMovies.ListItems.Count;
          int countCurrent = 1;
          
          foreach (GUIListItem item in listActorMovies.ListItems)
          {
            // Show visible progress
            if (count > 0)
            {
              int percCount = (100 * countCurrent) / count;
              GUIPropertyManager.SetProperty("#Actor.Name", _currentActor.Name + "   (" + countCurrent + "/" + count + " - " + +percCount + "%" + ")");
              countCurrent += 1;
            }
            //
            GetDetails(item);
            
            // Refresh item data if it's selected
            if (listActorMovies.SelectedListItem == item)
            {
              OnItemSelected(item, listActorMovies);
            }
          }
          
          // Reset variables and save state
          GUIPropertyManager.SetProperty("#Actor.Name", _currentActor.Name);
          _forceRefreshAll = false;
          SaveState();
        }
        else
        {
          Log.Info("VideoArtistInfo: Refreshing not possible. No Internet connection.");
        }
      }
      catch (ThreadAbortException)
      {
        
      }
    }

    private void GetDetails(GUIListItem item)
    {
      if (ListItemMovieInfo(item) == null)
      {
        return;
      }

      string plot = ListItemMovieInfo(item).MoviePlot;
      string cover = item.ThumbnailImage;
      string mpaa = ListItemMovieInfo(item).MovieMpaaRating;
      string tempItemLabel = item.Label; // To show Downloading on list item then switch to original label

      if (_forceRefreshAll)
      {
        item.Label = GUILocalizeStrings.Get(198) + "..."; // Downloading movie details
        // IMDB
        plot = GetPlotImdb(item);
        cover = GetThumbImdb(item);
      }
      else
      {
        if (plot == string.Empty || mpaa == string.Empty || mpaa == Strings.Unknown)
        {
          item.Label = GUILocalizeStrings.Get(198) + "...";
          plot = GetPlotImdb(item);
        }
        
        if (cover == string.Empty)
        {
          item.Label = GUILocalizeStrings.Get(198) + "...";
          cover = GetThumbImdb(item);
        }
      }
      
      // Save plot into DB
      if (plot != string.Empty || _forceRefreshAll)
      {
        SetPlot(item, plot);
      }

      // Save cover url into db
      if (cover.StartsWith("http://") || _forceRefreshAll)
      {
        SetThumb(ref item, cover);
      }

      // Update/Set back original item label (Downloading... -> Movie title)
      string line = String.Format("{0}. {1} ({2})",
                                    ListItemMovieInfo(item).Year,
                                    ListItemMovieInfo(item).MovieTitle,
                                    ListItemMovieInfo(item).Role);
      tempItemLabel = line.Replace("()", string.Empty).Trim(); // Year+Title+Role (visible on screen item)
      item.Label = tempItemLabel;
    }

    private void SetPlot(GUIListItem item, string shortPlot)
    {
      // Update database
      string plotSql = shortPlot;
      ExecuteSql("strPlot", plotSql, item);
      ListItemMovieInfo(item).MoviePlot = shortPlot;
      
    }

    private void SetThumb(ref GUIListItem item, string thumb)
    {
      // Save image (large and small)
      string filenameL = string.Empty;

      if (VideoDatabase.CheckMovieImdbId(ListItemMovieInfo(item).MovieImdbID))
      {
        string ttFolder = Regex.Replace(ListItemMovieInfo(item).MovieImdbID, "(tt(0*))", string.Empty);
        int ittFolder = 0;
        int.TryParse(ttFolder, out ittFolder);
        ittFolder = ittFolder / 25000;
        ttFolder = ittFolder.ToString();

        string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Videos\Actors\ActorsMovies\" + ittFolder + @"\";

        if (ListItemMovieInfo(item).MovieImdbID != string.Empty)
        {
          filenameL = ListItemMovieInfo(item).MovieImdbID;
        }

        if (filenameL != string.Empty)
        {
          if (!Directory.Exists(path))
          {
            Directory.CreateDirectory(path);
          }

          filenameL = path + filenameL + @".jpg";

          string temporaryFilename = Path.GetTempFileName();
          string tmpFile = temporaryFilename;
          temporaryFilename += ".jpg";
          string temporaryFilenameLarge = Util.Utils.ConvertToLargeCoverArt(temporaryFilename);
          temporaryFilenameLarge += ".jpg";
          Util.Utils.FileDelete(tmpFile);
          Util.Utils.FileDelete(temporaryFilenameLarge);
          
          if (!string.IsNullOrEmpty(thumb))
          {
            Util.Utils.DownLoadAndOverwriteCachedImage(thumb, temporaryFilename);
            // Convert downloaded image to large and small file and save on disk
            SaveCover(temporaryFilename, filenameL); // Temp file is deleted in SetCover method
          }
          else
          {
            Util.Utils.FileDelete(filenameL);
            filenameL = string.Empty;
          }

          // Update database
          ExecuteSql("strPictureURL", thumb, item);
          item.RefreshCoverArt();
          item.IconImage = filenameL;
          item.ThumbnailImage = filenameL;
        }
      }
    }
    
    private void ExecuteSql(string columnName, string columnData, GUIListItem item)
    {
      try
      {
        DatabaseUtility.RemoveInvalidChars(ref columnData);
        string sql = string.Format("update IMDBMovies set {0}='{1}' where idIMDB ='{2}'",
                                    columnName,
                                    columnData,
                                    ListItemMovieInfo(item).MovieImdbID);
        bool error = false;
        string errorMessage = string.Empty;
        VideoDatabase.ExecuteSql(sql, out error, out errorMessage);
      }
      catch (Exception) {}
    }

    private void SaveCover(string fileImgSource, string fileImgTargetL)
    {
      // Large cover
      Util.Utils.FileDelete(fileImgTargetL);
      Util.Picture.CreateThumbnail(fileImgSource, fileImgTargetL, (int)Thumbs.ThumbLargeResolution,
                                   (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsSmall);
      
      // Delete temp cover img file
      Util.Utils.FileDelete(fileImgSource);
    }

    #region Tmdb
    /*
    private string GetPlotTmdb(GUIListItem item, ref string thumbOut)
    {
      // Get TMDBId to retreive full details from TMDB
      string shortPlot = string.Empty;
      string strUrl =
        String.Format("http://api.themoviedb.org/2.1/Movie.imdbLookup/en/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" +
                      ListItemMovieInfo(item).MovieImdbID);
      string absUri;
      string regex = @"<id>(?<tmdbId>.*?)</id>";
      string body = GetPage(strUrl, "utf-8", out absUri);
      string tmdbId = Regex.Match(body, regex, RegexOptions.Singleline).Groups["tmdbId"].Value;

      if (tmdbId == string.Empty)
      {
        shortPlot = string.Empty;
        thumbOut = string.Empty;
        return shortPlot;
      }

      // Main - details by TMDBid
      strUrl =
        String.Format("http://api.themoviedb.org/2.1/Movie.getInfo/-/xml/2ed40b5d82aa804a2b1fcedb5ca8d97a/" + tmdbId);

      regex = @"<overview>(?<moviePlot>.*)</overview>";
      body = string.Empty;

      // Search Tmdb by API, body is XML result
      shortPlot = GetPlot(strUrl, regex, ref body);
      // Check if XML is valid (on English)
      regex = @"<translated>false</translated>";
      if (Regex.Match(body, regex).Success || shortPlot == string.Empty)
      {
        shortPlot = string.Empty;
        thumbOut = string.Empty;
        return shortPlot;
      }

      thumbOut = GetThumbTmdb(body);

      // MPARating
      regex = @"<certification>(?<certification>.*)</certification>";
      string mpaaRating = Regex.Match(body, regex, RegexOptions.Singleline).Groups["certification"].Value;

      if (mpaaRating == string.Empty)
        mpaaRating = Strings.Unknown;

      // Genres
      string regexBlockPattern = @"<categories>(?<cast>.*?)</categories>";
      regex = @"<category\stype=""genre""\sname=""(?<genre>.*?)""";
      string block = Regex.Match(body, regexBlockPattern, RegexOptions.Singleline).Value;
      MatchCollection mc = Regex.Matches(block, regex, RegexOptions.Singleline);

      string genre = string.Empty;

      foreach (Match m in mc)
      {
        genre += m.Groups["genre"].Value;
        genre += " / ";
      }
      if (genre != string.Empty)
        genre = genre.Remove(genre.LastIndexOf(" / "));

      // Actors
      regexBlockPattern = @"<cast>(?<cast>.*?)</cast>";
      regex = @"<person\sname=""(?<name>.*?)"".?character=""(?<role>.*?)"".?job=""(?<job>.*?)""";
      block = Regex.Match(body, regexBlockPattern, RegexOptions.Singleline).Value;
      mc = Regex.Matches(block, regex, RegexOptions.Singleline);

      string name = string.Empty;
      string job = string.Empty;
      string strCast = string.Empty;

      if (mc.Count != 0)
      {
        foreach (Match m in mc)
        {
          name = m.Groups["name"].Value;
          job = m.Groups["job"].Value;

          switch (job)
          {
            case "Director":
              break;

            case "Screenplay":
              break;

            case "Writer":
              break;

            case "Actor":
              strCast += name;
              strCast += " / ";
              break;
          }
        }
      }
      int index = strCast.LastIndexOf(" /");
      if (index > 0)
        strCast = strCast.Remove(index);

      // Execute sql to db for genres, rating and cast list
      ExecuteSql("strGenre", genre, item);
      ExecuteSql("mpaa", mpaaRating, item);
      ExecuteSql("strCast", strCast, item);
      ListItemMovieInfo(item).MovieGenre = genre;
      ListItemMovieInfo(item).MovieMpaaRating = mpaaRating;
      ListItemMovieInfo(item).MovieCast = strCast;

      return shortPlot;
    }

    private string GetThumbTmdb(string body)
    {
      string thumb = string.Empty;
      string regexBlockPattern = @"<images>.*?</images>";
      string regex = @"<image\stype=""poster""\surl=""(?<cover>http://cf1.imgobject.com/posters/.*?jpg)""";
      string block = Regex.Match(body, regexBlockPattern, RegexOptions.Singleline).Value;

      MatchCollection mc = Regex.Matches(block, regex, RegexOptions.Singleline);
      if (mc.Count > 0)
      {
        foreach (Match m in mc)
        {
          // Get cover - using mid quality cover
          if (m.Groups["cover"].Value.ToLowerInvariant().Contains("mid.jpg"))
          {
            thumb = m.Groups["cover"].Value;
            break;
          }
        }
      }
      return thumb;
    }
    */
    #endregion

    #region IMDB

    private string GetPlotImdb(GUIListItem item)
    {
      try
      {
        string plot = string.Empty;
        IMDBMovie movie = new IMDBMovie();
        
        movie.IMDBNumber = ListItemMovieInfo(item).MovieImdbID;

        if (!IMDB.InternalActorsScriptGrabber.InternalActorsGrabber.GetPlotImdb(ref movie))
        {
          return string.Empty;
        }
        
        plot = movie.PlotOutline;

        #region Extra data

        // Title
        if (movie.Title != string.Empty)
        {
          ExecuteSql("strTitle", movie.Title, item);
          ListItemMovieInfo(item).MovieTitle = movie.Title;
        }

        // Year
        if (movie.Year == 0)
        {
          movie.Year = DateTime.Today.Year + 3;
        }
        ExecuteSql("iYear", movie.Year.ToString(), item);
        ListItemMovieInfo(item).Year = movie.Year;

        // Director
        if (movie.WritingCredits == string.Empty)
        {
          movie.WritingCredits = Strings.Unknown;
        }
        ExecuteSql("strCredits", movie.WritingCredits, item);
        ListItemMovieInfo(item).MovieCredits = movie.WritingCredits;

        // Genres
        if (movie.SingleGenre == string.Empty)
        {
          movie.SingleGenre = Strings.Unknown;
        }
        ExecuteSql("strGenre", movie.SingleGenre, item);
        ListItemMovieInfo(item).MovieGenre = movie.SingleGenre;

        // MPAA rating
        if (movie.MPARating == string.Empty)
        {
          movie.MPARating = Strings.Unknown;
        }
        ExecuteSql("mpaa", movie.MPARating, item);
        ListItemMovieInfo(item).MovieMpaaRating = movie.MPARating;

        // Cast list
        ExecuteSql("strCast", movie.Cast, item);
        ListItemMovieInfo(item).MovieCast = movie.Cast;

        #endregion

        movie = null;
        return plot;
      }
      catch(ThreadAbortException)
      {
        Log.Info("GUIVideoArtistInfo: Movie database lookup GetDetails(): Thread aborted");
      }
      catch (Exception ex)
      {
        Log.Error("GUIVideoArtistInfo: Movie database lookup GetDetails() error: {0}", ex.Message);
      }

      return string.Empty;
    }

    private string GetThumbImdb(GUIListItem item)
    {
      try
      {
        string thumb =  IMDB.InternalActorsScriptGrabber.InternalActorsGrabber.GetThumbImdb(ListItemMovieInfo(item).MovieImdbID);
        return thumb;
      }
      catch (ThreadAbortException)
      {
        Log.Info("Movie database lookup GetThumbImdb() Thread aborted");
      }
      catch (Exception ex)
      {
        Log.Error("Movie database lookup GetThumbImdb() {0}", ex.Message);
      }

      return string.Empty;
    }

    #endregion
    
    #endregion

    #endregion
  }
}