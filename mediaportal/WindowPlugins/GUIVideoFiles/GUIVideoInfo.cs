/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Drawing;
using MediaPortal.Video.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using System.Web;
using System.Net;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using System.Threading;
namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// 
	/// </summary>
    public class GUIVideoInfo : GUIWindow, IRenderLayer, IMDB.IProgress
	{
		[SkinControlAttribute(2)]		protected GUIButtonControl btnPlay=null;
		[SkinControlAttribute(3)]		protected GUIToggleButtonControl btnPlot=null;
		[SkinControlAttribute(4)]		protected GUIToggleButtonControl btnCast=null;
		[SkinControlAttribute(5)]		protected GUIButtonControl btnRefresh=null;
		[SkinControlAttribute(6)]		protected GUIToggleButtonControl btnWatched=null;
		[SkinControlAttribute(10)]		protected GUISpinControl spinImages=null;
		[SkinControlAttribute(11)]		protected GUISpinControl spinDisc=null;
		[SkinControlAttribute(20)]		protected GUITextScrollUpControl tbPlotArea=null;
		[SkinControlAttribute(21)]		protected GUIImage imgCoverArt=null;
		[SkinControlAttribute(22)]		protected GUITextControl tbTextArea=null;
		[SkinControlAttribute(30)]		protected GUILabelControl lblImage=null;
        [SkinControlAttribute(100)]     protected GUILabelControl lblDisc = null;
        enum ViewMode
    {
      Image,
      Cast,
    }



    ViewMode viewmode= ViewMode.Image;
    IMDBMovie currentMovie = null;
    string[] coverArtUrls = new string[1];
    string imdbCoverArtUrl = String.Empty;
    Thread imageSearchThread = null;
    bool _isFuzzyMatching = false;

    public GUIVideoInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_VIDEO_INFO;
    }
    public override bool Init()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            _isFuzzyMatching = xmlreader.GetValueAsBool("movies", "fuzzyMatching", false);
      return Load(GUIGraphicsContext.Skin + @"\DialogVideoInfo.xml");

    }
    public override void PreInit()
    {
    }



		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
            this._isOverlayAllowed = true;
            GUIVideoOverlay videoOverlay = (GUIVideoOverlay)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_OVERLAY);
            if ((videoOverlay != null) && (videoOverlay.Focused)) videoOverlay.Focused = false;
            if (currentMovie == null)
            {
                return;
            }
			// Default picture					
			imdbCoverArtUrl = currentMovie.ThumbURL;
            coverArtUrls = new string[1];
            coverArtUrls[0] = imdbCoverArtUrl;
            spinImages.Reset();
            spinImages.SetReverse(true);
            spinImages.SetRange(1, 1);
            spinImages.Value = 1;

            spinImages.ShowRange = true;
            spinImages.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
      
			spinDisc.Reset();
			viewmode=ViewMode.Image;			    
			spinDisc.AddLabel("HD",0);
			for (int i = 0; i < 1000; ++i)
			{
				string description = String.Format("DVD#{0:000}", i);
				spinDisc.AddLabel( description,0);
			}
          
			spinDisc.IsVisible=false;
			spinDisc.Disabled=true;
			int iItem = 0;
			if (Utils.IsDVD(currentMovie.Path))
			{
				spinDisc.IsVisible=true;
				spinDisc.Disabled=false;
				string szNumber = String.Empty;
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
						if (bNumber) break;
					}
				}
				int iDVD = 0;
				if (szNumber.Length > 0)
				{
					int x = 0;
					while (szNumber[x] == '0' && x + 1 < szNumber.Length) x++;
					if (x < szNumber.Length)
					{
						szNumber = szNumber.Substring(x);
						iDVD = System.Int32.Parse(szNumber);
						if (iDVD < 0 && iDVD >= 1000)
							iDVD = -1;
						else iDVD++;
					}
				}
				if (iDVD <= 0) iDVD = 0;
				iItem = iDVD;
				//0=HD
				//1=DVD#000
				//2=DVD#001
				GUIControl.SelectItemControl(GetID, spinDisc.GetID, iItem);
			}
            Refresh();
            Update();
			imageSearchThread = new Thread(new ThreadStart(AmazonLookupThread));
            imageSearchThread.Start();
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


		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnRefresh)
			{
                if (IMDBFetcher.RefreshIMDB(this, ref currentMovie, _isFuzzyMatching)){
                    if ((imageSearchThread != null) && (imageSearchThread.IsAlive))
                    {
                        imageSearchThread.Abort();
                        imageSearchThread = null;
                    }
                    imdbCoverArtUrl = currentMovie.ThumbURL;
                    coverArtUrls = new string[1];
                    coverArtUrls[0] = imdbCoverArtUrl;
                    spinImages.Reset();
                    spinImages.SetReverse(true);
                    spinImages.SetRange(1, 1);
                    spinImages.Value = 1;
                    spinImages.ShowRange = true;
                    spinImages.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
                    Refresh();
                    Update();
                    imageSearchThread = new Thread(new ThreadStart(AmazonLookupThread));
                    imageSearchThread.Start();
                }
				return ;
			}

			if (control==spinImages)
			{
                int currentValue = spinImages.Value;
				int item=spinImages.Value-1;

    			if (item < 0 || item >= coverArtUrls.Length) item=0;
                if (currentValue == item)
                {
                    return;
                }

                currentMovie.ThumbURL = coverArtUrls[item];		
				string coverArtImage = Utils.GetCoverArtName(Thumbs.MovieTitle,currentMovie.Title);
				string largeCoverArtImage = Utils.GetLargeCoverArtName(Thumbs.MovieTitle,currentMovie.Title);
				Utils.FileDelete(coverArtImage);
				Utils.FileDelete(largeCoverArtImage);
				Refresh();            
				Update();
				int idMovie = currentMovie.ID;
				if (idMovie>=0)
					VideoDatabase.SetThumbURL(idMovie,currentMovie.ThumbURL);
				return ;
			}

			if (control==btnCast)
			{
				viewmode=ViewMode.Cast;
				Update();
			}
			if (control==btnPlot)
			{

				viewmode=ViewMode.Image;
				Update();
			}
			if (control==btnWatched)
			{
				if (currentMovie.Watched>0) 
					currentMovie.Watched=0;
				else
					currentMovie.Watched=1;
				VideoDatabase.SetMovieInfoById(currentMovie.ID,ref currentMovie);
			}

			if (control==spinDisc)
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

			if (control==btnPlay)
			{
				int id=currentMovie.ID;
				GUIVideoFiles.PlayMovie(id);
				return ;
			}
		}


    public IMDBMovie Movie
    {
      get { return currentMovie; }
      set { currentMovie = value; }
    }

    void Update()
    {
      if (currentMovie == null) return;

      //cast->image
      if (viewmode==ViewMode.Cast)
      {
				tbPlotArea.IsVisible=false;
				tbTextArea.IsVisible=true;
				imgCoverArt.IsVisible=true;
				lblDisc.IsVisible=false;
				spinDisc.IsVisible=false;
				btnPlot.Selected=false;
				btnCast.Selected=true;
      }
      //cast->plot
      if (viewmode==ViewMode.Image)
			{
				tbPlotArea.IsVisible=true;
				tbTextArea.IsVisible=false;
				imgCoverArt.IsVisible=true;
				lblDisc.IsVisible=true;
				spinDisc.IsVisible=true;
				btnPlot.Selected=true;
				btnCast.Selected=false;

      }
			btnWatched.Selected = (currentMovie.Watched!=0);
			currentMovie.SetProperties();
			if (imgCoverArt!=null)
			{
				imgCoverArt.FreeResources();
				imgCoverArt.AllocResources();
			}
			
    }
 
    
    void Refresh()
    {
			  string coverArtImage = String.Empty;
			  try
              {

                string imageUrl = currentMovie.ThumbURL;
                if (imageUrl.Length > 0)
                {
                  string largeCoverArtImage = Utils.GetLargeCoverArtName(Thumbs.MovieTitle,currentMovie.Title);
                  coverArtImage = Utils.GetCoverArtName(Thumbs.MovieTitle,currentMovie.Title);
                  if (!System.IO.File.Exists(coverArtImage))
                  {
                    string imageExtension;
                    imageExtension = System.IO.Path.GetExtension(imageUrl);
                    if (imageExtension.Length > 0)
                    {
                      string temporaryFilename = "temp";
                      temporaryFilename += imageExtension;
                      Utils.FileDelete(temporaryFilename);
                     
                      Utils.DownLoadAndCacheImage(imageUrl, temporaryFilename);
                      if (System.IO.File.Exists(temporaryFilename))
                      {
                        MediaPortal.Util.Picture.CreateThumbnail(temporaryFilename, coverArtImage, 128, 128, 0);
                        MediaPortal.Util.Picture.CreateThumbnail(temporaryFilename, largeCoverArtImage, 512, 512, 0);
                      }

                      Utils.FileDelete(temporaryFilename);
                    }//if ( strExtension.Length>0)
                    else
                    {
                      Log.Write("image has no extension:{0}", imageUrl);
                    }
                  }
                }
                
               
              }
              catch (Exception)
              {
  			  }
  			  currentMovie.SetProperties();
		}
		void AmazonLookupThread()
		{
            try
            {
                if (currentMovie == null) return;
                // Search for more pictures
                IMDBMovie movie = currentMovie;
                IMPawardsSearch impSearch = new IMPawardsSearch();
                impSearch.Search(movie.Title);
                AmazonImageSearch amazonSearch = new AmazonImageSearch();
                amazonSearch.Search(movie.Title);
                int thumb = 0;
                if (movie.ThumbURL != string.Empty)
                {
                    thumb = 1;
                }
                int pictureCount = amazonSearch.Count + impSearch.Count + thumb;
                if (pictureCount == 0)
                {
                    return;
                }
                int pictureIndex = 0;
                coverArtUrls = new string[pictureCount];
                if (movie.ThumbURL != string.Empty)
                {
                    coverArtUrls[pictureIndex++] = movie.ThumbURL;
                }
                if ((impSearch.Count > 0) && (impSearch[0] != string.Empty))
                {
                    for (int i = 0; i < impSearch.Count; ++i)
                    {
                        coverArtUrls[pictureIndex++] = impSearch[i];
                    }
                }
                if (amazonSearch.Count > 0)
                {
                    for (int i = 0; i < amazonSearch.Count; ++i)
                    {
                        coverArtUrls[pictureIndex++] = amazonSearch[i];
                    }
                }


                spinImages.Reset();
                spinImages.SetReverse(true);
                spinImages.SetRange(1, pictureCount);
                spinImages.Value = 1;

                spinImages.ShowRange = true;
                spinImages.UpDownType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
            }
            catch (ThreadAbortException)
            {
            }
            finally
            {
                imageSearchThread = null;
            }
        }

        #region IMDB.IProgress
        public bool OnDisableCancel(IMDBFetcher fetcher)
        {
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            if (pDlgProgress.IsInstance(fetcher))
            {
                pDlgProgress.DisableCancel(true);
            }
            return true;
        }
        public void OnProgress(string line1, string line2, string line3, int percent)
        {
            if (!GUIWindowManager.IsRouted) return;
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            pDlgProgress.ShowProgressBar(true);
            pDlgProgress.SetLine(1, line1);
            pDlgProgress.SetLine(2, line2);
            if (percent > 0)
                pDlgProgress.SetPercentage(percent);
            pDlgProgress.Progress();
        }
        public bool OnSearchStarting(IMDBFetcher fetcher)
        {
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            // show dialog that we're busy querying www.imdb.com
            pDlgProgress.SetHeading(197);
            pDlgProgress.SetLine(1, fetcher.MovieName);
            pDlgProgress.SetLine(2, String.Empty);
            pDlgProgress.SetObject(fetcher);
            pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
            return true;
        }
        public bool OnSearchStarted(IMDBFetcher fetcher)
        {
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
            {
                pDlgProgress.Close();
            }
            return true;
        }
        public bool OnMovieNotFound(IMDBFetcher fetcher)
        {
            // show dialog...
            GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            pDlgOK.SetHeading(195);
            pDlgOK.SetLine(1, fetcher.MovieName);
            pDlgOK.SetLine(2, String.Empty);
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            return true;
        }
        public bool OnDetailsStarted(IMDBFetcher fetcher)
        {
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            // show dialog that we're downloading the movie info
            pDlgProgress.SetHeading(198);
            pDlgProgress.SetLine(1, fetcher.MovieName);
            pDlgProgress.SetLine(2, String.Empty);
            pDlgProgress.SetObject(fetcher);
            pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
            return true;
        }
        public bool OnDetailsEnd(IMDBFetcher fetcher)
        {
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
            {
                pDlgProgress.Close();
            }
            return true;
        }
        public bool OnActorsStarted(IMDBFetcher fetcher)
        {
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            // show dialog that we're downloading the actor info
            pDlgProgress.SetHeading(986);
            pDlgProgress.SetLine(1, fetcher.MovieName);
            pDlgProgress.SetLine(2, String.Empty);
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
            GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            // show dialog...
            pDlgOK.SetHeading(195);
            pDlgOK.SetLine(1, fetcher.MovieName);
            pDlgOK.SetLine(2, String.Empty);
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            return false;
        }

        public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
        {
            string strMovieName = "";
            GetStringFromKeyboard(ref strMovieName);
            movieName = strMovieName;
            if (movieName == string.Empty) {
                return false;
            }
            return true;
        }
        public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
        {
            GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            // more then 1 movie found
            // ask user to select 1
            pDlgSelect.SetHeading(196);//select movie
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

        static public void GetStringFromKeyboard(ref string strLine)
        {
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            if (null == keyboard) return;
            keyboard.Reset();
            keyboard.Text = strLine;
            keyboard.DoModal(GUIWindowManager.ActiveWindow);
            strLine = String.Empty;
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
