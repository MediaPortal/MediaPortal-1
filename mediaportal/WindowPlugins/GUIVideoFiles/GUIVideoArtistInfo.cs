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
using System.Threading;
using System.Web;
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

    [SkinControl(3)] protected GUIToggleButtonControl btnBiography;
    [SkinControl(4)] protected GUIToggleButtonControl btnMovies;
    [SkinControl(20)] protected GUITextScrollUpControl tbBiographyArea;
    [SkinControl(21)] protected GUIImage imgCoverArt;
    [SkinControl(22)] protected GUITextControl tbMovieArea;
    [SkinControl(24)] protected GUIListControl listActorMovies;
    [SkinControl(25)] protected GUIImage imgMovieCover;
    [SkinControl(26)] protected GUITextScrollUpControl tbMoviePlot;

    #region Base Variables

    private int _actorIdState = -1; // Current session setting
    private int _selectedItemState = -1; //last selected item index
    private string _viewModeState = string.Empty;
    
    private bool _forceRefreshAll; // Refresh all movies (context menu)
    private string _strBody = string.Empty; // Fetched html body for IMDB
    private string[] _vdbParserStr;
    
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
          OnMovieInfo(item);
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      _forceRefreshAll = false;

      _vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("GUIVideoArtistInfo");

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
      _currentActor = VideoDatabase.GetActorInfo(_currentActor.ID);
      
      SaveState();

      //Clean properties
      IMDBActor actor = new IMDBActor();
      actor.SetProperties();
      
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
            GUIVideoFiles.PlayMovie(Convert.ToInt32(listActorMovies.SelectedListItem.DVDLabel), true);
          }
          catch { }
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
      for (int i = 0; i < _currentActor.Count; ++i)
      {
        string line = String.Format("{0}. {1} ({2})",
                                    _currentActor[i].Year,
                                    _currentActor[i].MovieTitle,
                                    _currentActor[i].Role);
        line.Replace("()", string.Empty).Trim();

        //List view
        var item = new GUIListItem();
        item.ItemId = i;
        item.Label = line; // Year+Title+Role (visible on screen item)
        
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
          // New colors suggested by Dadeo
          //if (movie.Watched > 0)
          //  item.IsPlayed = true;
        }
        //else // We don't have a movie
        //{
        //  item.IsRemote = true;
        //}
        
        item.ThumbnailImage = filenameL;
        item.OnItemSelected += OnItemSelected;
        listActorMovies.Add(item);
      }

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
        }
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {
        GUIPropertyManager.SetProperty("#Actor.MoviePlot", ListItemMovieInfo(item).MoviePlot);
        GUIPropertyManager.SetProperty("#Actor.MovieImage", item.ThumbnailImage);
        GUIPropertyManager.SetProperty("#imdbnumber", ListItemMovieInfo(item).MovieImdbID);
        GUIPropertyManager.SetProperty("#Actor.MovieExtraDetails", GUILocalizeStrings.Get(199) + " " +
                                                                   ListItemMovieInfo(item).MovieCredits.Replace(" /", ",") + " : : : " +
                                                                   GUILocalizeStrings.Get(174) + " " +
                                                                   ListItemMovieInfo(item).MovieGenre.Replace(" /", ",") + " : : : " +
                                                                   GUILocalizeStrings.Get(204) + " " +
                                                                   ListItemMovieInfo(item).MovieMpaaRating + " : : : " +
                                                                   GUILocalizeStrings.Get(344) + ": " +
                                                                   ListItemMovieInfo(item).MovieCast.Replace(" /", ","));
        GUIPropertyManager.SetProperty("#Actor.MovieTitle", ListItemMovieInfo(item).MovieTitle);
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
      VideoDatabase.GetMovieInfoById(Convert.ToInt32(item.DVDLabel), ref movie);
      
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
        var dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK); ;
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
        var dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);;
        if (dlg == null)
        {
          return;
        }
        dlg.SetHeading(GUILocalizeStrings.Get(1020));
        dlg.SetLine(1, "Refreshing is already active.");
        dlg.SetLine(2, "Please wait to finish it first.");
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
        _selectedItemState = xmlreader.GetValueAsInt("VideoArtistInfo", "itemid", -1);
        
        if (_currentActor.ID == _actorIdState)
        {
          if (_viewModeState == "Movies" &&
              GUIWindowManager.GetPreviousActiveWindow() != (int)Window.WINDOW_VIDEO_TITLE &&
              GUIWindowManager.GetPreviousActiveWindow() != (int)Window.WINDOW_VIDEO_INFO)
          {
            _viewmode = ViewMode.Movies;
            if (_selectedItemState >= 0 && listActorMovies != null && listActorMovies.Count >= _selectedItemState)
            {
              Update();
              listActorMovies.SelectedListItemIndex = _selectedItemState;
              OnItemSelected(listActorMovies[_selectedItemState], listActorMovies);
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
            OnItemSelected(listActorMovies[0], listActorMovies);
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
            _strBody = string.Empty;
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
              countCurrent += 1;
              GUIPropertyManager.SetProperty("#Actor.Name", _currentActor.Name + "   (" + percCount + @"%)");
            }
            //
            _strBody = string.Empty;
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
      string plot = ListItemMovieInfo(item).MoviePlot;
      string cover = item.ThumbnailImage;
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
        if (plot == string.Empty)
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
      if (plot != string.Empty)
        SetPlot(item, plot);
      
      // Save cover url into db
      if (cover.StartsWith("http://"))
        SetThumb(ref item, cover);

      // Update/Set back original item label (Downloading... -> Movie title)
      string line = String.Format("{0}. {1} ({2})",
                                    ListItemMovieInfo(item).Year,
                                    ListItemMovieInfo(item).MovieTitle,
                                    ListItemMovieInfo(item).Role);
      line.Replace("()", string.Empty).Trim();

      tempItemLabel = line; // Year+Title+Role (visible on screen item)
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
          Util.Utils.DownLoadAndCacheImage(thumb, temporaryFilename);
          // Convert downloaded image to large and small file and save on disk
          SaveCover(temporaryFilename, filenameL); // Temp file is deleted in SetCover method

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
        VideoDatabase.ExecuteSql(sql, out error);
      }
      catch (Exception) {}
    }

    private void SaveCover(string fileImgSource, string fileImgTargetL)
    {
      // Large cover
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
          if (m.Groups["cover"].Value.ToLower().Contains("mid.jpg"))
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
      if (_vdbParserStr == null || _vdbParserStr.Length != 10)
      {
        return string.Empty;
      }

      //string strUrl = String.Format("http://m.imdb.com/title/{0}", ListItemMovieInfo(item).MovieImdbID);
      string strUrl = String.Format(_vdbParserStr[0], ListItemMovieInfo(item).MovieImdbID);
      //string regex = @"<h1>Plot\sSummary</h1>\s+<p>(?<moviePlot>.+?)</p>";
      string regex = _vdbParserStr[1];

      _strBody = string.Empty;
      string shortPlot = GetPlot(strUrl, regex, ref _strBody);

      // Full plot
      //strUrl = String.Format("http://m.imdb.com/title/{0}/plotsummary", ListItemMovieInfo(item).MovieImdbID);
      strUrl = String.Format(_vdbParserStr[2], ListItemMovieInfo(item).MovieImdbID);
      //regex = @"<section\sclass=""plot"".*?<p>(?<moviePlot>.*?)</p>";
      regex = _vdbParserStr[3];
      
      string plotBody = string.Empty;
      string fullPlot = GetPlot(strUrl, regex, ref plotBody);
      
      if (fullPlot != string.Empty)
        shortPlot = fullPlot;
      
      // Director, actors, rating....
      GetExtraDataImdb(item);
      
      return shortPlot;
    }

    private void GetExtraDataImdb(GUIListItem item)
    {
      //Update title/Year
      //regex = title"\scontent="(?<movieTitle>.*?)[(](?<movieYear>.{4})[)]
      string regex = _vdbParserStr[4];
      string title = Regex.Match(_strBody, regex, RegexOptions.Singleline).Groups["movieTitle"].Value.Trim();
      int year = 0;
      int.TryParse(Regex.Match(_strBody, regex, RegexOptions.Singleline).Groups["movieYear"].Value.Trim(), out year);
      
      if (title != string.Empty)
      {
        ExecuteSql("strTitle", title, item);
        ListItemMovieInfo(item).MovieTitle = title;
      }

      if (year == 0)
      {
        year = DateTime.Today.Year + 3;
      }
      
      ExecuteSql("iYear", year.ToString(), item);
      ListItemMovieInfo(item).Year = year;
      
      // Director
      //regex = @"(<h1>Director</h1>|<h1>Directors</h1>)\s+<p>\s+<a\shref="".*?>(?<director>.*?)</a>";
      regex = _vdbParserStr[5];
      string director = Regex.Match(_strBody, regex, RegexOptions.Singleline).Groups["director"].Value.Trim();
      
      if (director == string.Empty)
        director = Strings.Unknown;
      
      ExecuteSql("strCredits", director, item);
      ListItemMovieInfo(item).MovieCredits = director;

      // Genre
      //regex = @"<h1>Genre</h1>\s+<p>(?<genre>.+?)</p>";
      regex = _vdbParserStr[6];
      string genre = Regex.Match(_strBody, regex, RegexOptions.Singleline).Groups["genre"].Value.Trim();
      genre = genre.Replace(", ", " / ");
      
      if (genre == string.Empty)
        genre = Strings.Unknown;
      
      ExecuteSql("strGenre", genre, item);
      ListItemMovieInfo(item).MovieGenre = genre;

      // Rating
      //regex = @"<h1>Rated</h1>\s+<p>(?<rating>.+?)</p>";
      regex = _vdbParserStr[7];
      string mpaaRating = Regex.Match(_strBody, regex, RegexOptions.Singleline).Groups["rating"].Value.Trim();
      
      if (mpaaRating == string.Empty)
        mpaaRating = Strings.Unknown;
      
      ExecuteSql("mpaa", mpaaRating, item);
      ListItemMovieInfo(item).MovieMpaaRating = mpaaRating;

      // Actors
      //regex = @"<div\sclass=""label"">\s+<div\sclass=""title"">\s+<a\shref="".*?>(?<actor>.*?)<";
      regex = _vdbParserStr[8];
      MatchCollection actors = Regex.Matches(_strBody, regex, RegexOptions.Singleline);
      
      string strActor = string.Empty;
      foreach (Match actor in actors)
      {
        string tmpActor = actor.Groups["actor"].Value;
        tmpActor = HttpUtility.HtmlDecode(tmpActor);

        if (tmpActor != string.Empty)
        {
          strActor += tmpActor + " / ";
        }
      }
      int index = strActor.LastIndexOf(" /");
      
      if (index > 0)
        strActor = strActor.Remove(index);
      ExecuteSql("strCast", strActor, item);
      ListItemMovieInfo(item).MovieCast = strActor;
    }

    private string GetThumbImdb(GUIListItem item)
    {
      if (_vdbParserStr == null || _vdbParserStr.Length != 10)
      {
        return string.Empty;
      }

      string strBody = string.Empty;
      string thumb = string.Empty;

      if (_strBody == string.Empty)
      {
        string uri;
        string strUrl = String.Format("http://m.imdb.com/title/" + ListItemMovieInfo(item).MovieImdbID);
        strBody = GetPage(strUrl, "utf-8", out uri);
      }
      else
      {
        strBody = _strBody;
      }
      //thumb = Regex.Match(strBody, @"<div\sclass=""poster"">\s+<a\shref=""(?<poster>.*?)""",
      //                            RegexOptions.Singleline).Groups["poster"].Value;
      thumb = Regex.Match(strBody, _vdbParserStr[9],
                                  RegexOptions.Singleline).Groups["poster"].Value;
      _strBody = string.Empty;
      return thumb;
    }

    #endregion
    
    private string GetPlot(string strUrl, string regex, ref string strBody)
    {
      string absoluteUri;
      strBody = HttpUtility.HtmlDecode(GetPage(strUrl, "utf-8", out absoluteUri));
      
      if (strBody != null)
      {
        string shortPlot = Regex.Match(strBody, regex, RegexOptions.Singleline).Groups["moviePlot"].Value.
          Replace("&amp;", "&").
          Replace("&lt;", "<").
          Replace("&gt;", ">").
          Replace("&quot;", "\"").
          Replace("&apos;", "'").
          Replace("No overview found.", string.Empty).Trim();


        shortPlot = Util.Utils.stripHTMLtags(shortPlot);
        
        // extra cleanup
        if (!string.IsNullOrEmpty(shortPlot))
        {
          int index = shortPlot.LastIndexOf(@"See full summary");
          
          if (index > 0)
          {
            shortPlot = shortPlot.Remove(index);
          }
          
          index = shortPlot.LastIndexOf(@"See full synopsis");
          
          if (index > 0)
          {
            shortPlot = shortPlot.Remove(index);
          }
          
          index = shortPlot.LastIndexOf("\n");

          if (index > 0)
          {
            shortPlot = shortPlot.Remove(index);
          }
        }
        return shortPlot;
      }
      return string.Empty;
    }

    // Download helper
    private string GetPage(string strUrl, string strEncode, out string absoluteUri)
    {
      string strBody = "";
      absoluteUri = string.Empty;
      Stream receiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strUrl);
        
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          req.Headers.Add("Accept-Language", "en-US");
          req.UserAgent = "Mozilla/8.0 (compatible; MSIE 9.0; Windows NT 6.1; .NET CLR 1.0.3705;)";
          req.Proxy.Credentials = CredentialCache.DefaultCredentials;
          req.Timeout = 10000;
        }
        catch (Exception) { }
        result = req.GetResponse();
        receiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = Encoding.GetEncoding(strEncode);
        using (sr = new StreamReader(receiveStream, encode))
        {
          strBody = sr.ReadToEnd();
        }


        absoluteUri = result.ResponseUri.AbsoluteUri;
      }
      catch (Exception)
      {
        //Log.Error("Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strURL, strEncode, ex.Message, ex.StackTrace);
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
      return strBody;
    }

    #endregion

    #endregion
  }
}