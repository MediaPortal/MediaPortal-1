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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
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
  public class GUIVideoInfo : GUIInternalWindow, IRenderLayer, IMDB.IProgress
  {
    #region Skin controls
    [SkinControl(2)] protected GUIButtonControl btnPlay = null;
    [SkinControl(3)] protected GUICheckButton btnPlot = null;
    [SkinControl(4)] protected GUICheckButton btnCast = null;
    [SkinControl(5)] protected GUIButtonControl btnRefresh = null;
    [SkinControl(6)] protected GUICheckButton btnWatched = null;
    [SkinControl(7)] protected GUICheckButton btnReview = null;
    [SkinControl(10)] protected GUISpinControl spinImages = null;
    [SkinControl(11)] protected GUISpinControl spinDisc = null;
    [SkinControl(20)] protected GUITextScrollUpControl tbPlotArea = null;
    [SkinControl(21)] protected GUIImage imgCoverArt = null;
    [SkinControl(22)] protected GUITextControl tbCastTextArea = null;
    [SkinControl(23)] protected GUITextScrollUpControl tbReviwArea = null;
    [SkinControl(30)] protected GUILabelControl lblImage = null;
    [SkinControl(100)] protected GUILabelControl lblDisc = null;

    // Test actorlist
    [SkinControl(24)] protected GUIListControl listActors = null;
    [SkinControl(25)] protected GUIImage imgActorArt = null;
    // Rename movie title
    [SkinControl(26)] protected GUIButtonControl btnRename = null;
    
    #endregion

    public delegate void CoversLookupCompleted(string[] coverThumbURLs);

    public static event CoversLookupCompleted CoverImagesDownloaded;

    private enum ViewMode
    {
      Plot,
      Cast,
      Review,
    }
    
    #region Variables

    private ViewMode _viewmode = ViewMode.Plot;
    private IMDBMovie _currentMovie;
    private string _folderForThumbs = string.Empty;
    private string[] _coverArtUrls = new string[1];
    private string _imdbCoverArtUrl = string.Empty;
    private ArrayList _actors = new ArrayList();
    // Saved state settings
    private string _viewModeState = string.Empty;
    private int _currentSelectedItem = -1;
    private int _movieIdState = -1;

    private Thread _imageSearchThread;
    private Thread _fanartRefreshThread;
	
	  private bool _addToDatabase = true; // Used for fake movies, skipping any interaction with videodatabase
    private bool _useOnlyNfoScraper = false;

    #endregion

    public GUIVideoInfo()
    {
      GetID = (int)Window.WINDOW_VIDEO_INFO;
    }

    #region Overrides

    public override bool Init()
    {
      CoverImagesDownloaded += OnCoverImagesDownloaded;
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\DialogVideoInfo.xml"));
    }

    public override void PreInit() { }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      using (Profile.Settings xmlreader = new MPSettings())
      {
        _useOnlyNfoScraper = xmlreader.GetValueAsBool("moviedatabase", "useonlynfoscraper", false);
      }

      this._isOverlayAllowed = true;
      GUIVideoOverlay videoOverlay = (GUIVideoOverlay)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_OVERLAY);
      
      if ((videoOverlay != null) && (videoOverlay.Focused))
      {
        videoOverlay.Focused = false;
      }
      
      // GoBack bug fix (corrupted video info)
      if (_currentMovie == null)
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

      // Check for a fake movie (comes from EPG or only nfo scraper)
      if (_currentMovie.ID < 1)
      {
        _addToDatabase = false;
        _currentMovie.LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      }
      
      // Refresh data in case that we open movie info after scan (some details missing)
      if (_addToDatabase)
      {
        VideoDatabase.GetMovieInfoById(_currentMovie.ID, ref _currentMovie);
      }

      // Default picture					
      _imdbCoverArtUrl = _currentMovie.ThumbURL;
      _coverArtUrls = new string[1];
      _coverArtUrls[0] = _imdbCoverArtUrl;
      
      ResetSpinControl();
      spinDisc.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER;
      spinDisc.Reset();
      _viewmode = ViewMode.Plot;
      spinDisc.AddLabel("HD", 0);
      
      for (int i = 0; i < 1000; ++i)
      {
        string description = String.Format("DVD#{0:000}", i);
        spinDisc.AddLabel(description, 0);
      }

      spinDisc.IsVisible = false;
      spinDisc.Disabled = true;
      int iItem = 0;
      
      if (Util.Utils.IsDVD(_currentMovie.Path))
      {
        spinDisc.IsVisible = true;
        spinDisc.Disabled = false;
        string szNumber = string.Empty;
        int iPos = 0;
        bool bNumber = false;
        
        for (int i = 0; i < _currentMovie.DVDLabel.Length; ++i)
        {
          char kar = _currentMovie.DVDLabel[i];
          if (Char.IsDigit(kar))
          {
            szNumber += kar;
            iPos++;
            bNumber = true;
          }
          else
          {
            if (bNumber)
            {
              break;
            }
          }
        }
        int iDVD = 0;
        
        if (szNumber.Length > 0)
        {
          int x = 0;
          while (szNumber[x] == '0' && x + 1 < szNumber.Length)
          {
            x++;
          }
          if (x < szNumber.Length)
          {
            szNumber = szNumber.Substring(x);
            iDVD = Int32.Parse(szNumber);
            if (iDVD < 0 && iDVD >= 1000)
            {
              iDVD = -1;
            }
            else
            {
              iDVD++;
            }
          }
        }
        
        if (iDVD <= 0)
        {
          iDVD = 0;
        }
        iItem = iDVD;
        //0=HD
        //1=DVD#000
        //2=DVD#001
        GUIControl.SelectItemControl(GetID, spinDisc.GetID, iItem);
      }

      Refresh();
      SetActorGUIListItems();
      Update();
      LoadState();
      SearchImages();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if ((_imageSearchThread != null) && (_imageSearchThread.IsAlive))
      {
        _imageSearchThread.Abort();
        _imageSearchThread = null;
      }

      if ((_fanartRefreshThread != null) && (_fanartRefreshThread.IsAlive))
      {
        _fanartRefreshThread.Abort();
        _fanartRefreshThread = null;
      }

      SaveState();
      
      // Delete cover for fake movie
      if (!_addToDatabase)
      {
        string titleExt = _currentMovie.Title + "{" + _currentMovie.ID + "}";
        string coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
        string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
        Util.Utils.FileDelete(coverArtImage);
        Util.Utils.FileDelete(largeCoverArtImage);
      }
      _addToDatabase = true;

      // Reset currentMovie variable if we go to windows which initialize that variable
      // Database and share views windows are only screens which do that
      if (newWindowId == (int)Window.WINDOW_VIDEOS || newWindowId == (int)Window.WINDOW_VIDEO_TITLE)
        _currentMovie = null;
      
      GUIPropertyManager.SetProperty("#actorThumb", string.Empty);
      ReleaseResources();
      base.OnPageDestroy(newWindowId);
    }
    
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          {
            PlayMovie();
            return;
          }
      }

      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      //
      // Refresh button
      //
      if (control == btnRefresh && _addToDatabase)
      {
        // Check Internet connection
        if (!Win32API.IsConnectedToInternet())
        {
          GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(257);
          dlgOk.SetLine(1, GUILocalizeStrings.Get(703));
          dlgOk.DoModal(GUIWindowManager.ActiveWindow);
          return;
        }
        // Actors list active? Refresh them
        if ((listActors != null && listActors.IsVisible) ||
            (listActors != null && listActors.Count == 0 && tbCastTextArea.IsVisible))
        {
          if (btnRefresh != null)
          {
            btnRefresh.Selected = false;
            btnRefresh.Focus = false;
          }
          
          if (!IMDBFetcher.FetchMovieActors(this, Movie))
          {
            // Canceled fetch (maybe will be needed in the future
            SetActorGUIListItems();
            
            if (tbCastTextArea.IsVisible)
            {
              ShowActors(true);
            }
            else
            {
              ShowActors(false);
            }
            return;
          }

          SetActorGUIListItems();
          
          if (tbCastTextArea.IsVisible)
          {
            ShowActors(true);
          }
          else
          {
            ShowActors(false);
          }
          return;
        }
        
        // Movie info active, refresh movie
        if (_useOnlyNfoScraper && CheckForNfoFile(_currentMovie.VideoFileName)) 
        {
          VideoDatabase.ImportNfoUsingVideoFile(_currentMovie.VideoFileName, false, false);
          VideoDatabase.GetMovieInfo(_currentMovie.VideoFileName, ref _currentMovie);
          UpdateMovieAfterRefresh();
        }
        else if (IMDBFetcher.RefreshIMDB(this, ref _currentMovie, false, false, _addToDatabase))
        {
          UpdateMovieAfterRefresh();
        }
        return;
      }
      //
      // Cover change spin button
      //
      if (control == spinImages)
      {
        int item = spinImages.Value - 1;
        
        if (item < 0 || item >= _coverArtUrls.Length)
        {
          item = 0;
        }
        
        if (_currentMovie.ThumbURL == _coverArtUrls[item])
        {
          return;
        }

        _currentMovie.ThumbURL = _coverArtUrls[item];
        string titleExt = string.Empty;
        
        // Title suffix for problem with covers and movie with the same name
        titleExt = _currentMovie.Title + "{" + _currentMovie.ID + "}";
        string coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
        string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
        
        Util.Utils.FileDelete(coverArtImage);
        //
        // 07.11.2010 Deda: Cache entry Flag change for cover thumb file
        //
        Util.Utils.DoInsertNonExistingFileIntoCache(coverArtImage);
        //
        Util.Utils.FileDelete(largeCoverArtImage);
        Refresh();
        Update();
        int idMovie = _currentMovie.ID;
        
        if (idMovie >= 0)
        {
          VideoDatabase.SetThumbURL(idMovie, _currentMovie.ThumbURL);
        }
        return;
      }
      //
      // Cast button
      //
      if (control == btnCast)
      {
        ShowActors(true);
      }
      //
      // Plot button
      //
      if (control == btnPlot)
      {
        _viewmode = ViewMode.Plot;
        Update();
      }
      //
      // Review button
      //
      if (control == btnReview)
      {
        _viewmode = ViewMode.Review;
        Update();
      }
      //
      // Watched button
      //
      if (control == btnWatched)
      {
        if (!_addToDatabase)
        {
          btnWatched.Selected = false;
          return;
        }

        int iPercent = 0;
        int iTimesWatched = 0;
        VideoDatabase.GetmovieWatchedStatus(_currentMovie.ID, out iPercent, out iTimesWatched);

        if (_currentMovie.Watched > 0)
        {
          GUIPropertyManager.SetProperty("#iswatched", "no");
          _currentMovie.Watched = 0;
          VideoDatabase.SetMovieWatchedStatus(_currentMovie.ID, false, iPercent);
        }
        else
        {
          GUIPropertyManager.SetProperty("#iswatched", "yes");
          _currentMovie.Watched = 1;
          VideoDatabase.SetMovieWatchedStatus(_currentMovie.ID, true, iPercent);
        }
        VideoDatabase.SetWatched(_currentMovie);
      }
      //
      // ---
      //
      if (control == spinDisc)
      {
        if (!_addToDatabase)
        {
          return;
        }

        string selectedItem = spinDisc.GetLabel();
        int idMovie = _currentMovie.ID;
        
        if (idMovie > 0)
        {
          if (selectedItem != "HD" && selectedItem != "share")
          {
            VideoDatabase.SetDVDLabel(idMovie, selectedItem);
          }
          else
          {
            VideoDatabase.SetDVDLabel(idMovie, "HD");
          }
        }
      }
      //
      // Play button
      //
      if (control == btnPlay)
      {
        PlayMovie();
        return;
      }
      //
      // Actor listview
      //
      if (listActors != null && _addToDatabase)
      {
        if (control == listActors)
        {
          IMDBActor actor = VideoDatabase.GetActorInfo(listActors.SelectedListItem.ItemId);

          if (actor == null ||
              (actor.Count == 0 || actor.IMDBActorID == string.Empty || actor.IMDBActorID == Strings.Unknown))
          {
            OnVideoArtistInfo(actor, true);
          }
          else
          {
            OnVideoArtistInfo(actor, false);
          }
        }
      }
      //
      // Rename movie title
      //
      if (control == btnRename && _addToDatabase)
      {
        RenameTitle();
      }
    }

    protected override void OnShowContextMenu()
    {
      if (!_addToDatabase)
      {
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }
      
      dlg.Reset();
      dlg.SetHeading(498); // menu
      
      // Dialog items
      if (listActors != null && _addToDatabase)
      {
        GUIListItem item = listActors.SelectedListItem;
        if (item != null && listActors.IsVisible)
        {
          dlg.AddLocalizedString(1297); //Refresh actor info
        }
      }

      dlg.AddLocalizedString(1262); // Update grabber scripts
      dlg.AddLocalizedString(1307); // Update internal grabber scripts
      dlg.AddLocalizedString(1263); // Set default grabber
      // Fanart refresh
      Profile.Settings xmlreader = new MPSettings();
      
      if (xmlreader.GetValueAsBool("moviedatabase", "usefanart", false) && _addToDatabase)
      {
        dlg.AddLocalizedString(1298); //Refresh fanart
      }

      dlg.AddLocalizedString(1304); //Export to nfo file
      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedId== -1)
      {
        return;
      }
      
      switch (dlg.SelectedId)
      {
        case 1297: // Refresh actor info
          IMDBActor actor = VideoDatabase.GetActorInfo(listActors.SelectedListItem.ItemId);
          OnVideoArtistInfo(actor, true);
          break;
        case 1298: // Refresh fanart
          OnFanartRefresh();
          break;
        case 1263: // Set deault grabber script
          GUIVideoFiles.SetDefaultGrabber();
          break;
        case 1262: // Update grabber scripts
          GUIVideoFiles.UpdateGrabberScripts(false);
          break;
        case 1307: // Update internal grabber scripts
          GUIVideoFiles.UpdateGrabberScripts(true);
          break;
        case 1304: // Create nfo file
          OnCreateNfoFile();
          break;

      }
    }

    #endregion

    public IMDBMovie Movie
    {
      get { return _currentMovie; }
      set { _currentMovie = value; }
    }
    
    public string FolderForThumbs
    {
      get { return _folderForThumbs; }
      set { _folderForThumbs = value; }
    }

    private void ShowActors(bool update)
    {
      _viewmode = ViewMode.Cast;
      
      if (update)
      {
        Update();
      }
      else
      {
        if (listActors != null)
        {
          if (tbCastTextArea != null) tbCastTextArea.IsVisible = false;
          listActors.Visible = true;

          if (!listActors.IsEnabled)
          {
            GUIControl.EnableControl(GetID, listActors.GetID);
          }

          GUIControl.SelectControl(GetID, listActors.GetID);
          GUIControl.FocusControl(GetID, listActors.GetID);
          
          if (listActors.Count > 0)
          {
            _currentSelectedItem = 0;
            SelectItem();
          }
        }
      }
    }

    private void PlayMovie()
    {
      if (!_addToDatabase)
      {
        return;
      }

      int id = _currentMovie.ID;
      ArrayList files = new ArrayList();
      VideoDatabase.GetFilesForMovie(id, ref files);

      if (files.Count > 1)
      {
        GUIVideoFiles.StackedMovieFiles = files;
        GUIVideoFiles.IsStacked = true;
      }
      
      GUIVideoFiles.MovieDuration(files, false);
      GUIVideoFiles.PlayMovie(id, false);
    }

    private void Update()
    {
      if (_currentMovie == null)
      {
        return;
      }

      try 
      {
        // Cast
        if (_viewmode == ViewMode.Cast)
        {
          if (tbPlotArea != null) tbPlotArea.IsVisible = false;

          if (tbReviwArea != null) tbReviwArea.IsVisible = false;

          if (tbCastTextArea != null) tbCastTextArea.IsVisible = false;

          if (imgCoverArt != null) imgCoverArt.IsVisible = true;

          if (lblDisc != null) lblDisc.IsVisible = false;

          if (spinDisc != null) spinDisc.IsVisible = false;

          if (btnPlot != null) btnPlot.Selected = false;

          if (btnReview != null) btnReview.Selected = false;

          if (btnCast != null)
          {
            btnCast.Selected = true;
            btnCast.Focus = false;
          }

          if (listActors != null && !listActors.IsVisible)
          {
            listActors.IsVisible = true;

            if (!listActors.IsEnabled)
            {
              GUIControl.EnableControl(GetID, listActors.GetID);
            }

            GUIControl.SelectControl(GetID, listActors.GetID);
            GUIControl.FocusControl(GetID, listActors.GetID);
            GUIPropertyManager.SetProperty("#itemcount", listActors.Count.ToString());
            listActors.SelectedListItemIndex = _currentSelectedItem;
            SelectItem();
          }

          if (imgActorArt != null) imgActorArt.IsVisible = true;

          if ((listActors == null && tbCastTextArea != null) || (listActors != null && listActors.Count == 0))
          {
            tbCastTextArea.IsVisible = true;

            if (listActors != null) listActors.IsVisible = false;
          }
        }
        // Plot
        if (_viewmode == ViewMode.Plot)
        {
          if (tbPlotArea != null) tbPlotArea.IsVisible = true;

          if (tbReviwArea != null) tbReviwArea.IsVisible = false;

          if (tbCastTextArea != null) tbCastTextArea.IsVisible = false;

          if (imgCoverArt != null) imgCoverArt.IsVisible = true;

          if (lblDisc != null) lblDisc.IsVisible = true;

          if (spinDisc != null) spinDisc.IsVisible = true;

          if (btnPlot != null) btnPlot.Selected = true;

          if (btnReview != null) btnReview.Selected = false;

          if (btnCast != null) btnCast.Selected = false;

          if (listActors != null)
          {
            listActors.IsVisible = false;
            _currentSelectedItem = listActors.SelectedListItemIndex;
            GUIPropertyManager.SetProperty("#itemcount", string.Empty);
          }

          if (imgActorArt != null) imgActorArt.IsVisible = false;
        }
        // Review
        if (_viewmode == ViewMode.Review)
        {
          if (tbPlotArea != null) tbPlotArea.IsVisible = false;

          if (tbReviwArea != null) tbReviwArea.IsVisible = true;

          if (tbCastTextArea != null) tbCastTextArea.IsVisible = false;

          if (imgCoverArt != null) imgCoverArt.IsVisible = true;

          if (lblDisc != null) lblDisc.IsVisible = true;

          if (spinDisc != null) spinDisc.IsVisible = true;

          if (btnPlot != null) btnPlot.Selected = false;

          if (btnReview != null) btnReview.Selected = true;

          if (btnCast != null) btnCast.Selected = false;

          if (listActors != null)
          {
            listActors.IsVisible = false;
            _currentSelectedItem = listActors.SelectedListItemIndex;
            GUIPropertyManager.SetProperty("#itemcount", string.Empty);
          }

          if (imgActorArt != null) imgActorArt.IsVisible = false;

        }
        
        btnWatched.Selected = (_currentMovie.Watched != 0);
        
        if (imgCoverArt != null)
        {
          imgCoverArt.Dispose();
          imgCoverArt.AllocResources();
        }

        if (imgActorArt != null)
        {
          imgCoverArt.Dispose();
          imgCoverArt.AllocResources();
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIVideoInfo Update controls Error: {1}", ex.Message);
      }
    }

    private void SelectItem()
    {
      if (_currentSelectedItem >= 0 && listActors != null)
      {
        GUIControl.SelectItemControl(GetID, listActors.GetID, _currentSelectedItem);
      }
    }

    private void Refresh()
    {
      // If spin button is pressed, small and large thumb are deleted before it calls Refresh()
      string coverArtImage = string.Empty;
      string largeCoverArtImage = string.Empty;

      try
      {
        string imageUrl = _currentMovie.ThumbURL;
        string titleCoverFilename = string.Empty;
        string imageExt = string.Empty;

        if (imageUrl.Length > 7 && 
          !imageUrl.Substring(0, 7).Equals("file://") &&
          !imageUrl.Substring(0, 7).Equals("http://"))
        {
          imageExt = Util.Utils.GetFileExtension(imageUrl);
          if ((Util.Utils.IsPicture(imageUrl) || imageExt.ToLowerInvariant() == ".tbn") && File.Exists(imageUrl))
          {
            imageUrl = "file://" + imageUrl;
          }
        }

        if (imageUrl.Length > 7 && (imageUrl.Substring(0, 7).Equals("file://") ||
                                    imageUrl.Substring(0, 7).Equals("http://")))
        {
          // Set cover thumb filename (movieTitle{movieId})
          titleCoverFilename = _currentMovie.Title + "{" + _currentMovie.ID + "}";
          // Set small thumb filename (C:\ProgramData\Team MediaPortal\MediaPortal\Thumbs\Videos\Title\movieTitle{movieId}.jpg)
          coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleCoverFilename);
          // Set large thumb filename (C:\ProgramData\Team MediaPortal\MediaPortal\Thumbs\Videos\Title\movieTitle{movieId}L.jpg)
          largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleCoverFilename);

          if (!Util.Utils.FileExistsInCache(coverArtImage))
          {
            Log.Debug("GUIVideoInfo: Refresh image: New Image url -> {0}", imageUrl);
            // Get image file extension from origin
            string imageExtension = Path.GetExtension(imageUrl);
            
            if (!string.IsNullOrEmpty(imageExtension))
            {
              // Creates a temporary file with a .TMP extension (this will be downloaded image path+filename 
              // before conversion to small and large thumb)
              string temporaryFilename = Path.GetTempFileName();
              string tmpFile = temporaryFilename;
              // Add image extension to temporary filename (this is small temp thumb)
              temporaryFilename += imageExtension;
              // Delete tmp file (we just needed rnd tmp filename)
              Util.Utils.FileDelete(tmpFile);
              
              //If image is local image (file) copy it as temporary file (one we get above)
              if (imageUrl.Length > 7 && imageUrl.Substring(0, 7).Equals("file://"))
              {
                File.Copy(imageUrl.Substring(7), temporaryFilename);
                File.SetAttributes(temporaryFilename, FileAttributes.Normal);
              }
              else // Dowload image if it is url link to a temporary file
              {
                Log.Debug("GUIVideoInfo Refresh image: Downloading image, new Image url -> {0}", imageUrl);
                Util.Utils.DownLoadAndCacheImage(imageUrl, temporaryFilename);
              }

              if (File.Exists(temporaryFilename))
              {
                // Create small thumb
                Log.Debug("GUIVideoInfo Refresh image: Creating new image -> {0}", coverArtImage);
                Util.Picture.CreateThumbnail(temporaryFilename, coverArtImage, (int) Thumbs.ThumbResolution,
                  (int) Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall);
                // Create large thumb
                Log.Debug("GUIVideoInfo Refresh image: Creating new image -> {0}", largeCoverArtImage);
                Util.Picture.CreateThumbnail(temporaryFilename, largeCoverArtImage,
                  (int) Thumbs.ThumbLargeResolution, (int) Thumbs.ThumbLargeResolution, 0,Thumbs.SpeedThumbsLarge);

                // Create folder thumb (movie is DVD or BD)
                if (FolderForThumbs != string.Empty)
                //edited by BoelShit
                {
                  // copy icon to folder also;
                  string strFolderImage = string.Empty;

                  strFolderImage = Path.GetFullPath(FolderForThumbs);
                  strFolderImage += "\\folder.jpg";
                  
                  try
                  {
                    Util.Utils.FileDelete(strFolderImage);

                    if (!File.Exists(strFolderImage))
                    {
                      File.Copy(largeCoverArtImage, strFolderImage, true); //edited by BoelShit
                    }
                  }
                  catch (Exception ex)
                  {
                    Log.Info("GUIVideoInfo: Error creating folder thumb {0}", ex.Message);
                  }
                }
              }
              
              // Delete original image temp file from a temp folder
              Util.Utils.FileDelete(temporaryFilename);
            }
            else
            {
              Log.Info("image has no extension:{0}", imageUrl);
            }
          }
        }
        else
        {
          Log.Error("GUIVideoInfo Refresh image: New cover image link is not valid -> {0}", imageUrl);
          return;
        }
      }
      catch (Exception ex2)
      {
        Log.Error("GUIVideoInfo Refresh image: Error creating new thumbs for {0} - {1}", _currentMovie.ThumbURL, ex2.Message);
      }

      SetProperties();
    }

    private void SetProperties()
    {
      ArrayList files = new ArrayList();
      VideoDatabase.GetFilesForMovie(_currentMovie.ID, ref files);

      if (files.Count > 0)
      {
        _currentMovie.SetProperties(false, (string)files[0]);
      }
      else
      {
        _currentMovie.SetProperties(false, string.Empty);
      }
    }
    
    private void  RenameTitle ()
    {
      if (!_addToDatabase)
      {
        return;
      }

      string movieTitle = _currentMovie.Title;
      ArrayList files = new ArrayList();
      VideoDatabase.GetFilesForMovie(_currentMovie.ID, ref files);
      string movieFileName = string.Empty;

      if (files.Count > 0)
      {
        movieFileName = (string)files[0];
        movieFileName = Util.Utils.GetFilename(movieFileName, true);
      }

      GetStringFromKeyboard(ref movieTitle);

      if (string.IsNullOrEmpty(movieTitle) || movieTitle == _currentMovie.Title)
      {
        return;
      }

      movieTitle = movieTitle.Trim();
      // Rename cover thumbs
      string oldTitleExt = _currentMovie.Title + "{" + _currentMovie.ID + "}";
      string newTitleExt = movieTitle + "{" + _currentMovie.ID + "}"; 
      string oldSmallThumb= Util.Utils.GetCoverArtName(Thumbs.MovieTitle, oldTitleExt);
      string oldLargeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, oldTitleExt);
      string newSmallThumb = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, newTitleExt);
      string newLargeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, newTitleExt);
      
      if (File.Exists(oldSmallThumb))
      {
        try
        {
          File.Copy(oldSmallThumb, newSmallThumb);
          File.Delete(oldSmallThumb);
        }
        catch (Exception) {}
      }
      if (File.Exists(oldLargeThumb))
      {
        try
        {
          File.Copy(oldLargeThumb, newLargeThumb);
          File.Delete(oldLargeThumb);
        }
        catch (Exception) { }
      }
      
      _currentMovie.Title = movieTitle;
      VideoDatabase.SetMovieInfoById(_currentMovie.ID, ref _currentMovie);
      
      Update();
      // Update thumbs for share selected item (if we activate videoinfo from share view))
      if (GUIWindowManager.GetPreviousActiveWindow() == (int)Window.WINDOW_VIDEOS)
      {
        if (GUIVideoFiles.CurrentSelectedGUIItem != null)
        {
          GUIVideoFiles.CurrentSelectedGUIItem.IconImage = newSmallThumb;
          GUIVideoFiles.CurrentSelectedGUIItem.IconImageBig = newSmallThumb;
          GUIVideoFiles.CurrentSelectedGUIItem.ThumbnailImage = newLargeThumb;
        }
      }

      Refresh();
    }

    private void ResetSpinControl()
    {
      spinImages.Reset();
      spinImages.SetRange(1, _coverArtUrls.Length);
      spinImages.Value = 1;
      
      spinImages.ShowRange = true;
      spinImages.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
    }

    private void OnCoverImagesDownloaded(string[] aThumbArray)
    {
      lock (this)
      {
        if (aThumbArray.Length > 0)
        {
          List<string> coverlist = new List<string>();

          for (int i = 0; i < aThumbArray.Count(); i++)
          {
            if (!coverlist.Contains(aThumbArray[i]))
            {
              coverlist.Add(aThumbArray[i]);
            }
          }

          aThumbArray = coverlist.ToArray();
          _coverArtUrls = null;
          _coverArtUrls = new string[aThumbArray.Length];
          aThumbArray.CopyTo(_coverArtUrls, 0);
          ResetSpinControl();
        }
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      try 
      {
        if (item != null)
        {
          GUIPropertyManager.SetProperty("#actorThumb", item.ThumbnailImage);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIVideoInfo OnItemSelected exception: {0}", ex.Message);
      }
    }

    private void SetActorGUIListItems()
    {
      if (!_addToDatabase)
      {
        return;
      }

      try 
      {
        _actors.Clear();
      
        if (listActors != null)
        {
          listActors.Clear();
        }
        else
        {
          return;
        }
      
        VideoDatabase.GetActorsByMovieID(Movie.ID, ref _actors);
      
        if (_actors.Count == 0)
        {
          return;
        }
      
        char[] splitter = { '|' };
        string[] temp;
      
        foreach (string actor in _actors)
        {
          temp = actor.Split(splitter);
          GUIListItem item = new GUIListItem();
          item.ItemId = Convert.ToInt32(temp[0]); // idActor from videodatabase Actors table
          item.Label = temp[1] + " - " + temp[3]; // Actor name + role
          item.Label2 = temp[1]; // Actor name
          item.Label3 = temp[2]; // Actor IMDB Id
          
          string largeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, item.ItemId.ToString()); // Actor thumb filename
          
          if (!File.Exists(largeThumb))
          {
            largeThumb = "defaultActor.png";
          }
          
          item.IconImage = largeThumb;
          item.ThumbnailImage = largeThumb;
          item.OnItemSelected += OnItemSelected;
          listActors.Add(item);
        }

        if (listActors.Count > 0)
        {
          listActors.SelectedListItemIndex = 0;
          _currentSelectedItem = 0;
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIVideoInfo exception SetActorGUIListItems: {0}", ex.Message);
      }
    }

    private void OnVideoArtistInfo(IMDBActor actor, bool refresh)
    {
      GUIVideoArtistInfo infoDlg =
        (GUIVideoArtistInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_ARTIST_INFO);
      
      if (infoDlg == null)
      {
        return;
      }

      if (actor != null)
      {
        string restriction = "30"; // Refresh every week actor info and movies

        TimeSpan ts = new TimeSpan(Convert.ToInt32(restriction), 0, 0, 0);
        DateTime searchDate = DateTime.Today - ts;
        DateTime lastUpdate;

        if (DateTime.TryParse(actor.LastUpdate, out lastUpdate))
        {
          if (searchDate > lastUpdate)
          {
            refresh = true;
          }
        }
      }


      // Scan if no actor record or actor movies are unknown or refresh
      if (actor == null || actor.Count == 0 || refresh)
      {
        string selectedActor;

        if (VideoDatabase.CheckActorImdbId(listActors.SelectedListItem.Label3))
        {
           selectedActor = listActors.SelectedListItem.Label3; // ActorImdbId
        }
        else
        {
          selectedActor = listActors.SelectedListItem.Label2; // Actor Name
        }
        
        GUIListItem item = listActors.SelectedListItem;
        item.AlbumInfoTag = _currentMovie;
        
        if (Win32API.IsConnectedToInternet())
        {
          IMDBFetcher.FetchMovieActor(this, _currentMovie, selectedActor, item.ItemId);
        }

        actor = VideoDatabase.GetActorInfo(item.ItemId);

        if (actor == null)
          return;

        // Refresh selected item
        item.Label = actor.Name + " - " + VideoDatabase.GetRoleByMovieAndActorId(Movie.ID, item.ItemId);
      }
      
      infoDlg.Actor = actor;
      infoDlg.Movie = _currentMovie;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_VIDEO_ARTIST_INFO);
    }
    
    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsInstance(fetcher))
      {
        pDlgProgress.DisableCancel(true);
      }
      return true;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (!GUIWindowManager.IsRouted)
      {
        return;
      }
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.SetLine(1, line1);
      pDlgProgress.SetLine(2, line2);
      if (percent > 0)
      {
        pDlgProgress.SetPercentage(percent);
      }
      pDlgProgress.Progress();
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're busy querying www.imdb.com
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(197);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      // show dialog...
      GUIDialogOK pDlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      pDlgOk.SetHeading(195);
      pDlgOk.SetLine(1, fetcher.MovieName);
      pDlgOk.SetLine(2, string.Empty);
      pDlgOk.DoModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the movie info
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(198);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading movie actors and roles
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(1301);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorInfoStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(1302);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      return true;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      // show dialog...
      GUIDialogOK pDlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      // show dialog...
      pDlgOk.SetHeading(195);
      pDlgOk.SetLine(1, fetcher.MovieName);
      pDlgOk.SetLine(2, string.Empty);
      pDlgOk.DoModal(GUIWindowManager.ActiveWindow);
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      string strMovieName = string.Empty  ;

      if (_addToDatabase)
      {
        ArrayList files = new ArrayList();
        VideoDatabase.GetFilesForMovie(_currentMovie.ID, ref files);
        string filename = string.Empty;
        
        try
        {
          if (files[0].ToString().ToUpperInvariant().Contains(@"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            filename = files[0].ToString().ToUpperInvariant().Replace(@"\VIDEO_TS\VIDEO_TS.IFO", string.Empty);
            int iIndex = 0;
            iIndex = filename.LastIndexOf(@"\");
            filename = filename.Substring(iIndex + 1);
          }
          else if (files[0].ToString().ToUpperInvariant().Contains(@"\BDMV\INDEX.BDMV"))
          {
            filename = files[0].ToString().ToUpperInvariant().Replace(@"\BDMV\INDEX.BDMV", string.Empty);
            int iIndex = 0;
            iIndex = filename.LastIndexOf(@"\");
            filename = filename.Substring(iIndex + 1);
          }
          else
          {
            filename = Path.GetFileNameWithoutExtension(files[0].ToString());
            Util.Utils.RemoveStackEndings(ref filename);
          }
        }
        catch (Exception)
        {
        }

        strMovieName = filename;
      }
      else
      {
        strMovieName = fetcher.Movie.Title;
      }

      GetStringFromKeyboard(ref strMovieName);
      movieName = strMovieName;

      if (movieName == string.Empty)
      {
        return false;
      }
      return true;
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      string filename = string.Empty;
      
      // more then 1 movie found
      // ask user to select 1
      if (_addToDatabase)
      {
        ArrayList files = new ArrayList();
        VideoDatabase.GetFilesForMovie(_currentMovie.ID, ref files);
        
        try
        {
          if (files[0].ToString().ToUpperInvariant().Contains(@"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            filename = files[0].ToString().ToUpperInvariant().Replace(@"\VIDEO_TS\VIDEO_TS.IFO", string.Empty);
            int iIndex = 0;
            iIndex = filename.LastIndexOf(@"\");
            filename = filename.Substring(iIndex + 1);
          }
          else if (files[0].ToString().ToUpperInvariant().Contains(@"\BDMV\INDEX.BDMV"))
          {
            filename = files[0].ToString().ToUpperInvariant().Replace(@"\BDMV\INDEX.BDMV", string.Empty);
            int iIndex = 0;
            iIndex = filename.LastIndexOf(@"\");
            filename = filename.Substring(iIndex + 1);
          }
          else
          {
            filename = Path.GetFileNameWithoutExtension(files[0].ToString());
            Util.Utils.RemoveStackEndings(ref filename);
          }
        }
        catch (Exception)
        {
        }
      }
      else
      {
        filename = fetcher.Movie.Title;
      }
      string strHeading = GUILocalizeStrings.Get(196);// + " " + filename;

      if (!string.IsNullOrEmpty(filename))
      {
        GUIPropertyManager.SetProperty("#selecteditem", filename);
      }

      pDlgSelect.SetHeading(strHeading);
      pDlgSelect.Reset();
      
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }

      // Clean selected item title in DialogWindow
      pDlgSelect.EnableButton(true);
      pDlgSelect.SetButtonLabel(413); // manual
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

      // and wait till user selects one
      selectedMovie = pDlgSelect.SelectedLabel;
      
      if (selectedMovie != -1)
      {
        return true;
      }

      if (!pDlgSelect.IsButtonPressed)
      {
        return false;
      }
      return true;
    }

    public bool OnSelectActor(IMDBFetcher fetcher, out int selectedActor)
    {
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      // more then 1 actor found
      // ask user to select 1
      pDlgSelect.SetHeading(GUILocalizeStrings.Get(1310)); //select actor
      pDlgSelect.Reset();
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
      pDlgSelect.EnableButton(false);
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

      // and wait till user selects one
      selectedActor = pDlgSelect.SelectedLabel;
      if (selectedActor != -1)
      {
        return true;
      }
      return false;
    }

    public bool OnScanStart(int total)
    {
      return true;
    }

    public bool OnScanEnd()
    {
      return true;
    }

    public bool OnScanIterating(int count)
    {
      return true;
    }

    public bool OnScanIterated(int count)
    {
      return true;
    }

    #endregion

    public static void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      strLine = string.Empty;
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
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

    // Fanart refresh thread
    private void OnFanartRefresh()
    {
      if (_currentMovie != null && Win32API.IsConnectedToInternet())
      {
        if ((_fanartRefreshThread != null) && (_fanartRefreshThread.IsAlive))
        {
          _fanartRefreshThread.Abort();
          _fanartRefreshThread = null;
        }

        _fanartRefreshThread = new Thread(ThreadFanartRefresh);
        _fanartRefreshThread.IsBackground = true;
        _fanartRefreshThread.Start();
      }
      else
      {
        // Notify user that new fanart download failed
        GUIDialogNotify dlgNotify =
          (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
        
        if (null != dlgNotify)
        {
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1298));
          dlgNotify.SetText(GUILocalizeStrings.Get(517));
          dlgNotify.DoModal(GetID);
        }
      }
    }

    private void OnCreateNfoFile()
    {
      if (_currentMovie != null)
      {
        VideoDatabase.MakeNfo(_currentMovie.ID);

        // Notify user that nfo is created
        GUIDialogNotify dlgNotify =
          (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
        
        if (null != dlgNotify)
        {
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1304));
          dlgNotify.SetText(GUILocalizeStrings.Get(1305));
          dlgNotify.DoModal(GetID);
        }
      }
    }

    private void ThreadFanartRefresh()
    {
      try
      {
        if (_currentMovie.ID > 0)
        {
          _currentMovie.UserFanart = string.Empty;
        }

        Profile.Settings xmlreader = new MPSettings();
        int faCount = xmlreader.GetValueAsInt("moviedatabase", "fanartnumber", 1);
        FanArt fa = new FanArt();
        fa.GetTmdbFanartByApi(_currentMovie.ID, _currentMovie.IMDBNumber, "", true, faCount, "");
        // Send global message that movie is refreshed/scanned
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
        // Notify user that new fanart are downloaded
        GUIDialogNotify dlgNotify =
          (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
        
        if (null != dlgNotify)
        {
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1298));
          dlgNotify.SetText(GUILocalizeStrings.Get(997));
          dlgNotify.DoModal(GetID);
        }
      }
      catch (Exception) { }
    }

    // Images fetch thread
    private void SearchImages()
    {
      _imageSearchThread = new Thread(ThreadSearchImages);
      _imageSearchThread.IsBackground = true;
      _imageSearchThread.Start();
    }

    private void ThreadSearchImages()
    {
      try
      {
        if (_currentMovie == null)
        {
          return;
        }

        // Search for more covers
        string[] thumbUrls = new string[1];
        IMDBMovie movie = _currentMovie;
        // TMDB Search  Deda 30.4.2010
        TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();
        tmdbSearch.SearchCovers(movie.Title, movie.IMDBNumber);
        // IMPAward search
        IMPAwardsSearch impSearch = new IMPAwardsSearch();
        
        if (movie.Year > 1900)
        {
          impSearch.SearchCovers(movie.Title + " " + movie.Year, movie.IMDBNumber);
        }
        else
        {
          impSearch.SearchCovers(movie.Title, movie.IMDBNumber);
        }

        int thumb = 0;

        if (movie.ThumbURL != string.Empty)
        {
          thumbUrls[0] = movie.ThumbURL;
          thumb = 1;
        }
        int pictureCount = impSearch.Count + tmdbSearch.Count + thumb;
        //Hugh, no covers, lets pull our last card
        if ((tmdbSearch.Count == 0) & (impSearch.Count == 0))
        {
          // IMDB
          // Last defence in search for covers - will be counted if previous methods fails
          IMDBSearch imdbSearch = new IMDBSearch();
          imdbSearch.SearchCovers(movie.IMDBNumber, false);
          // Nothing found, we loose -> exit
          if (imdbSearch.Count == 0)
          {
            return;
          }
          // Last defence survived, so lets grab what we can from IMDB and get out of here
          pictureCount = imdbSearch.Count + thumb;

          int pictureIndeximdb = 0;
          thumbUrls = new string[pictureCount];

          if ((imdbSearch.Count > 0) && (imdbSearch[0] != string.Empty))
          {
            for (int i = 0; i < imdbSearch.Count; ++i)
            {
              if (thumbUrls[0] != imdbSearch[i])
              {
                thumbUrls[pictureIndeximdb++] = imdbSearch[i];
              }
            }
          }
          if (CoverImagesDownloaded != null)
          {
            CoverImagesDownloaded(thumbUrls);
          }
          return;
        }

        int pictureIndex = 0;
        thumbUrls = new string[pictureCount];

        if (movie.ThumbURL != string.Empty)
        {
          thumbUrls[pictureIndex++] = movie.ThumbURL;
        }
        
        // IMP Award check and add
        if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
        {
          for (int i = 0; i < impSearch.Count; ++i)
          {
            thumbUrls[pictureIndex++] = impSearch[i];
          }
        }

        // TMDB Count check and add into thumbs Deda 30.4.2010
        if ((tmdbSearch.Count > 0) && (tmdbSearch[0] != string.Empty))
        {
          for (int i = 0; i < tmdbSearch.Count; ++i)
          {
            thumbUrls[pictureIndex++] = tmdbSearch[i];
          }
        }

        if (CoverImagesDownloaded != null)
        {
          CoverImagesDownloaded(thumbUrls);
        }

        if (VideoDatabase.CheckMovieImdbId(movie.IMDBNumber))
        {
          string restriction = "7"; // Refresh every week votes, rating

          TimeSpan ts = new TimeSpan(Convert.ToInt32(restriction), 0, 0, 0);
          DateTime searchDate = DateTime.Today - ts;
          DateTime lastUpdate;
          
          if (DateTime.TryParse(movie.LastUpdate, out lastUpdate))
          {
            if (searchDate > lastUpdate)
            {
              if (Win32API.IsConnectedToInternet() && VideoDatabase.CheckMovieImdbId(_currentMovie.IMDBNumber))
              {
                RefreshImdbData();
              }
            }
          }
        }
      }
      catch (ThreadAbortException) { }
    }

    private void RefreshImdbData()
    {
      if (!_addToDatabase)
      {
        return;
      }

      try
      {
        string uri;
        string movieUrl = "http://www.imdb.com/title/" + _currentMovie.IMDBNumber;
        string strBody = GetPage(movieUrl, "utf-8", out uri);
        string regexPattern = string.Empty;
        string[] vdbParserStr = VdbParserStringVideoInfo();

        if (vdbParserStr == null || vdbParserStr.Length != 3)
        {
          return;
        }

        // Runtime
        regexPattern = vdbParserStr[0];
        int runtime;
          
        if (int.TryParse(Regex.Match(strBody, regexPattern).Groups["movieRuntime"].Value, out runtime))
        {
          if (_currentMovie.RunTime <= 0 && runtime > 0)
          {
            _currentMovie.RunTime = runtime;
            GUIPropertyManager.SetProperty("#runtime", _currentMovie.RunTime +
                                                       GUILocalizeStrings.Get(2998) +
                                                       " (" + Util.Utils.SecondsToHMString(_currentMovie.RunTime*60) +
                                                       ")");
          }
        }

        // Rating
        regexPattern = vdbParserStr[1];
        string rating = Regex.Match(strBody, regexPattern).Groups["movieScore"].Value.Replace('.', ',');
          
        if (!string.IsNullOrEmpty(rating))
        {
          double dRating = 0;
          Double.TryParse(rating, out dRating);

          if (dRating > 0)
          {
            _currentMovie.Rating = (float) dRating;

            if (_currentMovie.Rating > 10.0f)
            {
              _currentMovie.Rating /= 10.0f;
            }

            GUIPropertyManager.SetProperty("#rating", _currentMovie.Rating.ToString());
            GUIPropertyManager.SetProperty("#strrating",
                                           "(" + _currentMovie.Rating.ToString(CultureInfo.CurrentCulture) + "/10)");
          }
        }
        
        // Votes
        regexPattern = vdbParserStr[2];
        string strVotes = Regex.Match(strBody, regexPattern).Groups["moviePopularity"].Value;
        
        strVotes = strVotes.Replace(",", "");
          
        Int32 i_votes = 0;
        string votes = string.Empty;
        Int32.TryParse(strVotes, out i_votes);

        if (i_votes > 0)
        {
          votes = String.Format("{0:N0}", i_votes);
          GUIPropertyManager.SetProperty("#votes", votes);
          _currentMovie.Votes = strVotes;
        }
          
        VideoDatabase.SetMovieInfoById(_currentMovie.ID, ref _currentMovie, true);

        DateTime lastUpdate;
        DateTime.TryParseExact(_currentMovie.LastUpdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out lastUpdate);
        GUIPropertyManager.SetProperty("#lastupdate", lastUpdate.ToShortDateString());
      }
      catch (Exception){}
    }

    // Get videoinfo parser strings
    private string[] VdbParserStringVideoInfo()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("GUIVideoInfo");
      return vdbParserStr;
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

    private bool CheckForNfoFile (string videoFile)
    {
      try
      {
        string nfoFile = string.Empty;
        string path = string.Empty;
        bool isbdDvd = false;

        if (videoFile.ToUpperInvariant().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          //DVD folder
          path = videoFile.Substring(0, videoFile.ToUpperInvariant().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase));
          isbdDvd = true;
        }
        else if (videoFile.ToUpperInvariant().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          //BD folder
          path = videoFile.Substring(0, videoFile.ToUpperInvariant().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase));
          isbdDvd = true;
        }

        if (isbdDvd)
        {
          string cleanFile = string.Empty;
          cleanFile = Path.GetFileNameWithoutExtension(videoFile);
          Util.Utils.RemoveStackEndings(ref cleanFile);
          nfoFile = path + @"\" + cleanFile + ".nfo";

          if (!File.Exists(nfoFile))
          {
            cleanFile = Path.GetFileNameWithoutExtension(path);
            Util.Utils.RemoveStackEndings(ref cleanFile);
            nfoFile = path + @"\" + cleanFile + ".nfo";
          }
        }
        else
        {
          string cleanFile = string.Empty;
          string strPath, strFilename;
          DatabaseUtility.Split(videoFile, out strPath, out strFilename);
          cleanFile = strFilename;
          Util.Utils.RemoveStackEndings(ref cleanFile);
          cleanFile = strPath + cleanFile;
          nfoFile = Path.ChangeExtension(cleanFile, ".nfo");
        }

        if (!File.Exists(nfoFile))
        {
          return false;
        }

        // Validate nfo xml
        XmlDocument doc = new XmlDocument();
        doc.Load(nfoFile);
        doc = null;
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }

    private void LoadState()
    {
      if (!_addToDatabase)
      {
        return;
      }

      using (Profile.Settings xmlreader = new MPSettings())
      {
        _viewModeState = xmlreader.GetValueAsString("VideoInfo", "lastview", string.Empty);
        _movieIdState = xmlreader.GetValueAsInt("VideoInfo", "movieid", -1);
        
        if (_viewModeState == "Cast" &&
              GUIWindowManager.GetPreviousActiveWindow() != (int)Window.WINDOW_VIDEO_TITLE &&
              GUIWindowManager.GetPreviousActiveWindow() != (int)Window.WINDOW_VIDEOS &&
              _movieIdState == _currentMovie.ID)
        {
          if (_currentSelectedItem >= 0 && listActors != null && listActors.Count >= _currentSelectedItem)
          {
            _currentSelectedItem = xmlreader.GetValueAsInt("VideoInfo", "itemid", -1);
            _viewmode = ViewMode.Cast;
            Update();

            if (!listActors.IsEnabled)
            {
              GUIControl.EnableControl(GetID, listActors.GetID);
            }

            GUIControl.FocusControl(GetID, listActors.GetID);
            SelectItem();
          }
        }
      }
    }

    private void SaveState()
    {
      if (!_addToDatabase)
      {
        return;
      }

      using (Profile.Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("VideoInfo", "lastview", _viewmode);
        if (_currentMovie != null)
          xmlwriter.SetValue("VideoInfo", "movieid", _currentMovie.ID);
        if (listActors != null)
          xmlwriter.SetValue("VideoInfo", "itemid", listActors.SelectedListItemIndex);
      }
    }

    private void UpdateMovieAfterRefresh()
    {
      if ((_imageSearchThread != null) && (_imageSearchThread.IsAlive))
      {
        _imageSearchThread.Abort();
        _imageSearchThread = null;
      }

      _imdbCoverArtUrl = _currentMovie.ThumbURL;
      _coverArtUrls = new string[1];
      _coverArtUrls[0] = _imdbCoverArtUrl;

      ResetSpinControl();

      Refresh();
      SetActorGUIListItems();
      Update();
      // Start images search thread
      SearchImages();
      // Send global message that movie is refreshed/scanned
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);
    }

    private void ReleaseResources()
    {
      if (listActors != null)
      {
        listActors.Clear();
      }

      if (imgCoverArt != null)
      {
        imgCoverArt.Dispose();
      }

      if (imgActorArt != null)
      {
        imgActorArt.Dispose();
      }
    }

  }
}