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
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
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
    [SkinControl(3)] protected GUIToggleButtonControl btnPlot = null;
    [SkinControl(4)] protected GUIToggleButtonControl btnCast = null;
    [SkinControl(5)] protected GUIButtonControl btnRefresh = null;
    [SkinControl(6)] protected GUIToggleButtonControl btnWatched = null;
    [SkinControl(7)] protected GUIToggleButtonControl btnReview = null;
    [SkinControl(10)] protected GUISpinControl spinImages = null;
    [SkinControl(11)] protected GUISpinControl spinDisc = null;
    [SkinControl(20)] protected GUITextScrollUpControl tbPlotArea = null;
    [SkinControl(21)] protected GUIImage imgCoverArt = null;
    [SkinControl(22)] protected GUITextControl tbTextArea = null;
    [SkinControl(23)] protected GUITextScrollUpControl tbReviwArea = null;
    [SkinControl(30)] protected GUILabelControl lblImage = null;
    [SkinControl(100)] protected GUILabelControl lblDisc = null;
    #endregion

    public delegate void AmazonLookupCompleted(string[] coverThumbURLs);

    public static event AmazonLookupCompleted AmazonImagesDownloaded;

    private enum ViewMode
    {
      Plot,
      Cast,
      Review,
    }
    
    #region Variables

    private ViewMode viewmode = ViewMode.Plot;
    private IMDBMovie currentMovie = null;
    private string folderForThumbs = string.Empty;
    private string[] coverArtUrls = new string[1];
    private string imdbCoverArtUrl = string.Empty;

    private Thread imageSearchThread = null;

    #endregion

    public GUIVideoInfo()
    {
      GetID = (int)Window.WINDOW_VIDEO_INFO;
    }

    #region Overrides

    public override bool Init()
    {
      AmazonImagesDownloaded += new AmazonLookupCompleted(OnAmazonImagesDownloaded);
      return Load(GUIGraphicsContext.Skin + @"\DialogVideoInfo.xml");
    }

    public override void PreInit() { }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      this._isOverlayAllowed = true;
      GUIVideoOverlay videoOverlay = (GUIVideoOverlay)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_OVERLAY);
      if ((videoOverlay != null) && (videoOverlay.Focused))
      {
        videoOverlay.Focused = false;
      }
      // GoBack bug fix (corrupted video info)
      if (currentMovie == null)
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
      // Default picture					
      imdbCoverArtUrl = currentMovie.ThumbURL;
      coverArtUrls = new string[1];
      coverArtUrls[0] = imdbCoverArtUrl;
      
      ResetSpinControl();
      spinDisc.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER;
      spinDisc.Reset();
      viewmode = ViewMode.Plot;
      spinDisc.AddLabel("HD", 0);
      for (int i = 0; i < 1000; ++i)
      {
        string description = String.Format("DVD#{0:000}", i);
        spinDisc.AddLabel(description, 0);
      }

      spinDisc.IsVisible = false;
      spinDisc.Disabled = true;
      int iItem = 0;
      if (Util.Utils.IsDVD(currentMovie.Path))
      {
        spinDisc.IsVisible = true;
        spinDisc.Disabled = false;
        string szNumber = string.Empty;
        int iPos = 0;
        bool bNumber = false;
        for (int i = 0; i < currentMovie.DVDLabel.Length; ++i)
        {
          char kar = currentMovie.DVDLabel[i];
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
      Refresh(false);
      Update();

      SearchImages();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if ((imageSearchThread != null) && (imageSearchThread.IsAlive))
      {
        imageSearchThread.Abort();
        imageSearchThread = null;
      }

      // Reset currentMovie variable if we go to windows which initialize that variable
      // Database and share views windows are only screens which do that
      if (newWindowId == (int)Window.WINDOW_VIDEOS || newWindowId == (int)Window.WINDOW_VIDEO_TITLE)
        currentMovie = null;

      base.OnPageDestroy(newWindowId);
    }

    // Changed - covers and the same movie name
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnRefresh)
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
        string title = currentMovie.Title;
        int id = currentMovie.ID;
        string file = currentMovie.Path + "\\" + currentMovie.File;
        // Delete covers
        FanArt.DeleteCovers(title, id);
        //Delete fanarts
        FanArt.DeleteFanarts(file, title);

        if (IMDBFetcher.RefreshIMDB(this, ref currentMovie, false, false, true))
        {
          if ((imageSearchThread != null) && (imageSearchThread.IsAlive))
          {
            imageSearchThread.Abort();
            imageSearchThread = null;
          }

          imdbCoverArtUrl = currentMovie.ThumbURL;
          coverArtUrls = new string[1];
          coverArtUrls[0] = imdbCoverArtUrl;
          
          ResetSpinControl();

          Refresh(false);
          Update();
          // Start images search thread
          SearchImages();
        }
        return;
      }

      if (control == spinImages)
      {
        int item = spinImages.Value - 1;
        if (item < 0 || item >= coverArtUrls.Length)
        {
          item = 0;
        }
        if (currentMovie.ThumbURL == coverArtUrls[item])
        {
          return;
        }

        currentMovie.ThumbURL = coverArtUrls[item];
        // Title suffix for problem with covers and movie with the same name
        string titleExt = currentMovie.Title + "{" + currentMovie.ID + "}";
        string coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
        string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
        Util.Utils.FileDelete(coverArtImage);
        //
        // 07.11.2010 Deda: Cache entry Flag change for cover thumb file
        //
        Util.Utils.DoInsertNonExistingFileIntoCache(coverArtImage);
        //
        Util.Utils.FileDelete(largeCoverArtImage);
        Refresh(false);
        Update();
        int idMovie = currentMovie.ID;
        if (idMovie >= 0)
        {
          VideoDatabase.SetThumbURL(idMovie, currentMovie.ThumbURL);
        }
        return;
      }

      if (control == btnCast)
      {
        viewmode = ViewMode.Cast;
        Update();
      }

      if (control == btnPlot)
      {
        viewmode = ViewMode.Plot;
        Update();
      }

      if (control == btnReview)
      {
        viewmode = ViewMode.Review;
        Update();
      }

      if (control == btnWatched)
      {
        if (currentMovie.Watched > 0)
        {
          GUIPropertyManager.SetProperty("#iswatched", "no");
          currentMovie.Watched = 0;
          VideoDatabase.SetMovieWatchedStatus(currentMovie.ID, false);
          ArrayList files = new ArrayList();
          VideoDatabase.GetFiles(currentMovie.ID, ref files);

          foreach (string file in files)
          {
            int fileId = VideoDatabase.GetFileId(file);
            VideoDatabase.DeleteMovieStopTime(fileId);
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#iswatched", "yes");
          currentMovie.Watched = 1;
          VideoDatabase.SetMovieWatchedStatus(currentMovie.ID, true);
        }
        VideoDatabase.SetWatched(currentMovie);
      }

      if (control == spinDisc)
      {
        string selectedItem = spinDisc.GetLabel();
        int idMovie = currentMovie.ID;
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

      if (control == btnPlay)
      {
        int id = currentMovie.ID;

        ArrayList files = new ArrayList();
        VideoDatabase.GetFiles(id, ref files);

        if (files.Count > 1)
        {
          GUIVideoFiles._stackedMovieFiles = files;
          GUIVideoFiles._isStacked = true;
          GUIVideoFiles.MovieDuration(files);
        }

        GUIVideoFiles.PlayMovie(id, false);
        return;
      }
    }

    #endregion

    public IMDBMovie Movie
    {
      get { return currentMovie; }
      set { currentMovie = value; }
    }

    public string FolderForThumbs
    {
      get { return folderForThumbs; }
      set { folderForThumbs = value; }
    }

    private void Update()
    {
      if (currentMovie == null)
      {
        return;
      }

      // Cast
      if (viewmode == ViewMode.Cast)
      {
        if (tbPlotArea != null) tbPlotArea.IsVisible = false;
        if (tbReviwArea != null) tbReviwArea.IsVisible = false;
        if (tbTextArea != null) tbTextArea.IsVisible = true;
        if (imgCoverArt != null) imgCoverArt.IsVisible = true;
        if (lblDisc != null) lblDisc.IsVisible = false;
        if (spinDisc != null) spinDisc.IsVisible = false;
        if (btnPlot != null) btnPlot.Selected = false;
        if (btnReview != null) btnReview.Selected = false;
        if (btnCast != null) btnCast.Selected = true;
      }
      // Plot
      if (viewmode == ViewMode.Plot)
      {
        if (tbPlotArea != null) tbPlotArea.IsVisible = true;
        if (tbReviwArea != null) tbReviwArea.IsVisible = false;
        if (tbTextArea != null) tbTextArea.IsVisible = false;
        if (imgCoverArt != null) imgCoverArt.IsVisible = true;
        if (lblDisc != null) lblDisc.IsVisible = true;
        if (spinDisc != null) spinDisc.IsVisible = true;
        if (btnPlot != null) btnPlot.Selected = true;
        if (btnReview != null) btnReview.Selected = false;
        if (btnCast != null) btnCast.Selected = false;
      }
      // Review
      if (viewmode == ViewMode.Review)
      {
        if (tbPlotArea != null) tbPlotArea.IsVisible = false;
        if (tbReviwArea != null) tbReviwArea.IsVisible = true;
        if (tbTextArea != null) tbTextArea.IsVisible = false;
        if (imgCoverArt != null) imgCoverArt.IsVisible = true;
        if (lblDisc != null) lblDisc.IsVisible = true;
        if (spinDisc != null) spinDisc.IsVisible = true;
        if (btnPlot != null) btnPlot.Selected = false;
        if (btnReview != null) btnReview.Selected = true;
        if (btnCast != null) btnCast.Selected = false;
      }

      btnWatched.Selected = (currentMovie.Watched != 0);
      currentMovie.SetProperties(false);

      if (imgCoverArt != null)
      {
        imgCoverArt.Dispose();
        imgCoverArt.AllocResources();
      }
    }

    //Changed - covers and same movie names
    private void Refresh(bool forceFolderThumb)
    {
      string coverArtImage = string.Empty;
      string largeCoverArtImage = string.Empty; //added by BoelShit
      try
      {
        string imageUrl = currentMovie.ThumbURL;
        if (imageUrl.Length > 0)
        {
          string titleExt = currentMovie.Title + "{" + currentMovie.ID + "}";
          coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, titleExt);
          largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
          //added by BoelShit
          string largeCoverArtImageConvert = Util.Utils.ConvertToLargeCoverArt(coverArtImage); //edited by Boelshit

          if (!Util.Utils.FileExistsInCache(coverArtImage))
          {
            string imageExtension;
            imageExtension = Path.GetExtension(imageUrl);
            if (imageExtension.Length > 0)
            {
              string temporaryFilename = Path.GetTempFileName();
              string tmpFile = temporaryFilename;
              temporaryFilename += imageExtension;
              string temporaryFilenameLarge = Util.Utils.ConvertToLargeCoverArt(temporaryFilename);
              temporaryFilenameLarge += imageExtension;
              Util.Utils.FileDelete(tmpFile);
              Util.Utils.FileDelete(temporaryFilenameLarge);
              if (imageUrl.Length > 7 && imageUrl.Substring(0, 7).Equals("file://"))
              {
                // Local image, don't download, just copy
                File.Copy(imageUrl.Substring(7), temporaryFilename);
              }
              else
              {
                Util.Utils.DownLoadAndCacheImage(imageUrl, temporaryFilename);
              }
              if (File.Exists(temporaryFilename))
              // Reverted from mantis : 3126 (unwanted TMP folder scan and cache entry)
              {
                Util.Picture.CreateThumbnail(temporaryFilename, coverArtImage, (int)Thumbs.ThumbResolution,
                                             (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall);

                if (File.Exists(temporaryFilenameLarge))
                // Reverted from mantis : 3126 (unwanted TMP folder scan and cache entry)
                {
                  Util.Picture.CreateThumbnail(temporaryFilenameLarge, largeCoverArtImageConvert,
                                               (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0,
                                               Thumbs.SpeedThumbsLarge); //edited by Boelshit              
                }
                else
                {
                  Util.Picture.CreateThumbnail(temporaryFilename, largeCoverArtImageConvert,
                                               (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0,
                                               Thumbs.SpeedThumbsLarge); //edited by Boelshit              
                }
              }
              Util.Utils.FileDelete(temporaryFilename);
            } //if ( strExtension.Length>0)
            else
            {
              Log.Info("image has no extension:{0}", imageUrl);
            }
          }
          if (!Util.Utils.FileExistsInCache(coverArtImage))
          {
            int idMovie = currentMovie.ID;
            System.Collections.ArrayList movies = new System.Collections.ArrayList();
            VideoDatabase.GetFiles(idMovie, ref movies);
            if (movies.Count > 0)
            {
              for (int i = 0; i < movies.Count; i++)
              {
                string thumbFile = Util.Utils.EncryptLine((string)movies[i]);
                coverArtImage = Util.Utils.GetCoverArtName(Thumbs.Videos, thumbFile);
                largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.Videos, thumbFile);
                if (Util.Utils.FileExistsInCache(largeCoverArtImage))
                {
                  currentMovie.ThumbURL = "file://" + largeCoverArtImage;
                  Refresh(forceFolderThumb);
                  break;
                }
              }
            }
          }

          if (((Util.Utils.FileExistsInCache(largeCoverArtImage)) && (FolderForThumbs != string.Empty)) ||
              forceFolderThumb)
          //edited by BoelShit
          {
            // copy icon to folder also;
            string strFolderImage = string.Empty;
            if (forceFolderThumb)
            {
              strFolderImage = Path.GetFullPath(currentMovie.Path);
            }
            else
            {
              strFolderImage = Path.GetFullPath(FolderForThumbs);
            }

            strFolderImage += "\\folder.jpg"; //TODO                  
            try
            {
              Util.Utils.FileDelete(strFolderImage);
              if (forceFolderThumb)
              {
                if (Util.Utils.FileExistsInCache(largeCoverArtImage))
                {
                  File.Copy(largeCoverArtImage, strFolderImage, true);
                }

                else if (Util.Utils.FileExistsInCache(largeCoverArtImageConvert)) //edited by BoelShit
                {
                  File.Copy(largeCoverArtImageConvert, strFolderImage, true); //edited by BoelShit
                }
                else //edited by BoelShit
                {
                  File.Copy(coverArtImage, strFolderImage, true); //edited by BoelShit
                }
              }
              else
              {
                File.Copy(largeCoverArtImage, strFolderImage, false); //edited by BoelShit
              }
              File.Copy(coverArtImage, strFolderImage, false); //edited by BoelShit
            }
            catch (Exception ex1)
            {
              Log.Error("GUIVideoInfo: Error creating folder thumb {0}", ex1.Message);
            }
          }
        }
      }
      catch (Exception ex2)
      {
        Log.Error("GUIVideoInfo: Error creating new thumbs for {0} - {1}", currentMovie.ThumbURL, ex2.Message);
      }
      currentMovie.SetProperties(false);
    }

    private void AmazonLookupThread()
    {
      //
    }

    private void ResetSpinControl()
    {
      spinImages.Reset();
      spinImages.SetRange(1, coverArtUrls.Length);
      spinImages.Value = 1;

      spinImages.ShowRange = true;
      spinImages.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
    }

    private void OnAmazonImagesDownloaded(string[] aThumbArray)
    {
      lock (this)
      {
        if (aThumbArray.Length > 0)
        {
          coverArtUrls = null;
          coverArtUrls = new string[aThumbArray.Length];
          aThumbArray.CopyTo(coverArtUrls, 0);
          ResetSpinControl();
        }
      }
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
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      pDlgOK.SetHeading(195);
      pDlgOK.SetLine(1, fetcher.MovieName);
      pDlgOK.SetLine(2, string.Empty);
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
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
      // show dialog that we're downloading the actor info
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(986);
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
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      // show dialog...
      pDlgOK.SetHeading(195);
      pDlgOK.SetLine(1, fetcher.MovieName);
      pDlgOK.SetLine(2, string.Empty);
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      string strMovieName = "";
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
      // more then 1 movie found
      // ask user to select 1
      pDlgSelect.SetHeading(196); //select movie
      pDlgSelect.Reset();
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
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
      else
      {
        return true;
      }
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
    
    // Images fetch thread
    private void SearchImages()
    {
      imageSearchThread = new Thread(ThreadSearchImages);
      imageSearchThread.IsBackground = true;
      imageSearchThread.Start();
    }

    private void ThreadSearchImages()
    {
      try
      {
        if (currentMovie == null)
        {
          return;
        }
        // Search for more covers
        string[] thumbUrls = new string[1];
        IMDBMovie movie = currentMovie;
        // TMDB Search  Deda 30.4.2010
        TMDBCoverSearch tmdbSearch = new TMDBCoverSearch();
        tmdbSearch.SearchCovers(movie.Title, movie.IMDBNumber);
        // IMPAward search
        IMPAwardsSearch impSearch = new IMPAwardsSearch();
        impSearch.SearchCovers(movie.Title, movie.IMDBNumber);

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
          if (AmazonImagesDownloaded != null)
          {
            AmazonImagesDownloaded(thumbUrls);
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

        if (AmazonImagesDownloaded != null)
        {
          AmazonImagesDownloaded(thumbUrls);
        }
      }
      catch (ThreadAbortException) { }
    }
  }
}