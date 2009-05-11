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
using System.IO;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIVideoInfo : GUIWindow, IRenderLayer, IMDB.IProgress
  {
    #region ThumbDownloader

    public class ThumbDownloader
    {
      private IMDBMovie _aMovie = null;
      private Work work;

      // Filename must only be the path of the directory
      public ThumbDownloader(IMDBMovie LookupMovie)
      {
        _aMovie = LookupMovie;
        work = new Work(new DoWorkHandler(this.PerformRequest));
        work.ThreadPriority = ThreadPriority.Normal;
        GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Normal);
      }

      private void PerformRequest()
      {
        try
        {
          if (_aMovie == null)
          {
            return;
          }
          // Search for more pictures
          string[] thumbUrls = new string[1];
          IMDBMovie movie = _aMovie;
          IMPawardsSearch impSearch = new IMPawardsSearch();
          impSearch.Search(movie.Title);
          AmazonImageSearch amazonSearch = new AmazonImageSearch();
          amazonSearch.Search(movie.Title);
          int thumb = 0;

          if (movie.ThumbURL != string.Empty)
          {
            thumbUrls[0] = movie.ThumbURL;
            thumb = 1;
          }

          int pictureCount = amazonSearch.Count + impSearch.Count + thumb;
          if (pictureCount == 0)
          {
            return;
          }

          int pictureIndex = 0;
          thumbUrls = new string[pictureCount];

          if (movie.ThumbURL != string.Empty)
          {
            thumbUrls[pictureIndex++] = movie.ThumbURL;
          }

          if (amazonSearch.Count > 0)
          {
            for (int i = 0; i < amazonSearch.Count; ++i)
            {
              thumbUrls[pictureIndex++] = amazonSearch[i];
            }
          }

          if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
          {
            for (int i = 0; i < impSearch.Count; ++i)
            {
              thumbUrls[pictureIndex++] = impSearch[i];
            }
          }
          if (AmazonImagesDownloaded != null)
          {
            AmazonImagesDownloaded(thumbUrls);
          }
        }
        catch (ThreadAbortException)
        {
        }
      }
    }

    #endregion

    [SkinControl(2)] protected GUIButtonControl btnPlay = null;
    [SkinControl(3)] protected GUIToggleButtonControl btnPlot = null;
    [SkinControl(4)] protected GUIToggleButtonControl btnCast = null;
    [SkinControl(5)] protected GUIButtonControl btnRefresh = null;
    [SkinControl(6)] protected GUIToggleButtonControl btnWatched = null;
    [SkinControl(10)] protected GUISpinControl spinImages = null;
    [SkinControl(11)] protected GUISpinControl spinDisc = null;
    [SkinControl(20)] protected GUITextScrollUpControl tbPlotArea = null;
    [SkinControl(21)] protected GUIImage imgCoverArt = null;
    [SkinControl(22)] protected GUITextControl tbTextArea = null;
    [SkinControl(30)] protected GUILabelControl lblImage = null;
    [SkinControl(100)] protected GUILabelControl lblDisc = null;

    public delegate void AmazonLookupCompleted(string[] coverThumbURLs);

    public static event AmazonLookupCompleted AmazonImagesDownloaded;

    private enum ViewMode
    {
      Image,
      Cast,
    }

    private ViewMode viewmode = ViewMode.Image;
    private IMDBMovie currentMovie = null;
    private string folderForThumbs = string.Empty;
    private string[] coverArtUrls = new string[1];
    private string imdbCoverArtUrl = string.Empty;

    private Thread imageSearchThread = null;

    public GUIVideoInfo()
    {
      GetID = (int) Window.WINDOW_VIDEO_INFO;
    }

    public override bool Init()
    {
      AmazonImagesDownloaded += new AmazonLookupCompleted(OnAmazonImagesDownloaded);
      return Load(GUIGraphicsContext.Skin + @"\DialogVideoInfo.xml");
    }

    public override void PreInit()
    {
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      this._isOverlayAllowed = true;
      GUIVideoOverlay videoOverlay = (GUIVideoOverlay) GUIWindowManager.GetWindow((int) Window.WINDOW_VIDEO_OVERLAY);
      if ((videoOverlay != null) && (videoOverlay.Focused))
      {
        videoOverlay.Focused = false;
      }
      if (currentMovie == null)
      {
        return;
      }
      // Default picture					
      imdbCoverArtUrl = currentMovie.ThumbURL;
      coverArtUrls = new string[1];
      coverArtUrls[0] = imdbCoverArtUrl;
      //spinImages.Reset();
      //spinImages.SetReverse(true);
      //spinImages.SetRange(1, 1);
      //spinImages.Value = 1;

      //spinImages.ShowRange = true;
      //spinImages.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;

      ResetSpinControl();
      spinDisc.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER;
      spinDisc.Reset();
      viewmode = ViewMode.Image;
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

      ThumbDownloader thumbWorker = new ThumbDownloader(currentMovie);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      if ((imageSearchThread != null) && (imageSearchThread.IsAlive))
      {
        imageSearchThread.Abort();
        imageSearchThread = null;
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnRefresh)
      {
        if (IMDBFetcher.RefreshIMDB(this, ref currentMovie, false, false))
        {
          if ((imageSearchThread != null) && (imageSearchThread.IsAlive))
          {
            imageSearchThread.Abort();
            imageSearchThread = null;
          }
          imdbCoverArtUrl = currentMovie.ThumbURL;
          coverArtUrls = new string[1];
          coverArtUrls[0] = imdbCoverArtUrl;
          //spinImages.Reset();
          //spinImages.SetReverse(true);
          //spinImages.SetRange(1, 1);
          //spinImages.Value = 1;
          //spinImages.ShowRange = true;
          //spinImages.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;

          ResetSpinControl();

          Refresh(false);
          Update();

          ThumbDownloader thumbWorker = new ThumbDownloader(currentMovie);
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
        string coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, currentMovie.Title);
        string largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, currentMovie.Title);
        Util.Utils.FileDelete(coverArtImage);
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
        viewmode = ViewMode.Image;
        Update();
      }

      if (control == btnWatched)
      {
        if (currentMovie.Watched > 0)
        {
          currentMovie.Watched = 0;
        }
        else
        {
          currentMovie.Watched = 1;
        }
        VideoDatabase.SetMovieInfoById(currentMovie.ID, ref currentMovie);
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
        GUIVideoFiles.PlayMovie(id);
        return;
      }
    }


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

      //cast->image
      if (viewmode == ViewMode.Cast)
      {
        tbPlotArea.IsVisible = false;
        tbTextArea.IsVisible = true;
        imgCoverArt.IsVisible = true;
        lblDisc.IsVisible = false;
        spinDisc.IsVisible = false;
        btnPlot.Selected = false;
        btnCast.Selected = true;
      }
      //cast->plot
      if (viewmode == ViewMode.Image)
      {
        tbPlotArea.IsVisible = true;
        tbTextArea.IsVisible = false;
        imgCoverArt.IsVisible = true;
        lblDisc.IsVisible = true;
        spinDisc.IsVisible = true;
        btnPlot.Selected = true;
        btnCast.Selected = false;
      }

      btnWatched.Selected = (currentMovie.Watched != 0);
      currentMovie.SetProperties();

      if (imgCoverArt != null)
      {
        imgCoverArt.FreeResources();
        imgCoverArt.AllocResources();
      }
    }


    private void Refresh(bool forceFolderThumb)
    {
      string coverArtImage = string.Empty;
      string largeCoverArtImage = string.Empty; //added by BoelShit
      try
      {
        string imageUrl = currentMovie.ThumbURL;
        if (imageUrl.Length > 0)
        {
          coverArtImage = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, currentMovie.Title);
          largeCoverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, currentMovie.Title);
            //added by BoelShit
          string largeCoverArtImageConvert = Util.Utils.ConvertToLargeCoverArt(coverArtImage); //edited by Boelshit

          if (!File.Exists(coverArtImage))
          {
            string imageExtension;
            imageExtension = Path.GetExtension(imageUrl);
            if (imageExtension.Length > 0)
            {
              string temporaryFilename = Path.GetTempFileName();
              temporaryFilename += imageExtension;
              string temporaryFilenameLarge = Util.Utils.ConvertToLargeCoverArt(temporaryFilename);
              temporaryFilenameLarge += imageExtension;
              Util.Utils.FileDelete(temporaryFilename);
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
              {
                Util.Picture.CreateThumbnail(temporaryFilename, coverArtImage, (int) Thumbs.ThumbResolution,
                                             (int) Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall);

                if (File.Exists(temporaryFilenameLarge))
                {
                  Util.Picture.CreateThumbnail(temporaryFilenameLarge, largeCoverArtImageConvert,
                                               (int) Thumbs.ThumbLargeResolution, (int) Thumbs.ThumbLargeResolution, 0,
                                               Thumbs.SpeedThumbsLarge); //edited by Boelshit              
                }
                else
                {
                  Util.Picture.CreateThumbnail(temporaryFilename, largeCoverArtImageConvert,
                                               (int) Thumbs.ThumbLargeResolution, (int) Thumbs.ThumbLargeResolution, 0,
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

          if (((File.Exists(largeCoverArtImage)) && (FolderForThumbs != string.Empty)) || forceFolderThumb)
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
                if (File.Exists(largeCoverArtImage))
                {
                  File.Copy(largeCoverArtImage, strFolderImage, true);
                }

                else if (File.Exists(largeCoverArtImageConvert)) //edited by BoelShit
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
      currentMovie.SetProperties();
    }

    private void AmazonLookupThread()
    {
//
    }

    private void ResetSpinControl()
    {
      spinImages.Reset();
      //spinImages.SetReverse(true);
      //spinImages.SetRange(1, pictureCount);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      // show dialog...
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      pDlgOK.SetHeading(195);
      pDlgOK.SetLine(1, fetcher.MovieName);
      pDlgOK.SetLine(2, string.Empty);
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
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
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
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
      GUIDialogSelect pDlgSelect = (GUIDialogSelect) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT);
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
      VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
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
  }
}