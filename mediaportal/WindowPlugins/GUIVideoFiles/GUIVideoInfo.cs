/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
	public class GUIVideoInfo : GUIWindow
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
		enum ViewMode
    {
      Image,
      Cast,
    }

    #region Base Dialog Variables
    bool m_bRunning = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;

    #endregion

    
    ViewMode viewmode= ViewMode.Image;
    bool m_bRefresh = false;
    IMDBMovie currentMovie = null;
    bool m_bPrevOverlay = false;
    AmazonImageSearch amazonSearch = new AmazonImageSearch();
    string imdbCoverArtUrl = String.Empty;

    public GUIVideoInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_VIDEO_INFO;
    }
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogVideoInfo.xml");
    }
    public override void PreInit()
    {
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    #region Base Dialog Members
    public void RenderDlg(float timePassed)
    {
      // render the parent window
			lock (this)
			{
				if (null != m_pParentWindow) 
					m_pParentWindow.Render(timePassed);

				GUIFontManager.Present();
				// render this dialog box
				base.Render(timePassed);
			}
    }

    void Close()
		{
			GUIWindowManager.IsSwitchingToNewWindow=true;
			lock (this)
			{
				m_bRunning = false;
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
				OnMessage(msg);

				GUIWindowManager.UnRoute();
				m_pParentWindow = null;
			}
			GUIWindowManager.IsSwitchingToNewWindow=false;
    }

    public void DoModal(int dwParentId)
    {	
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }

			GUIWindowManager.IsSwitchingToNewWindow=true;
      GUIWindowManager.RouteToWindow(GetID);

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);


			GUIWindowManager.IsSwitchingToNewWindow=false;
      m_bRunning = true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }
    #endregion

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();

			// Default picture					
			imdbCoverArtUrl = currentMovie.ThumbURL;
			spinImages.Reset();
			spinImages.SetRange(0,0);
      
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
			Refresh();Update();
			Thread workerThread = new Thread(new ThreadStart(AmazonLookupThread));
			workerThread.Start();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			currentMovie = null;
			GUIGraphicsContext.Overlay = m_bPrevOverlay;
		}


		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnRefresh)
			{
				if (currentMovie.ThumbURL.Length > 0)
				{
					string thumbnailImage = Utils.GetCoverArt(Thumbs.MovieTitle,currentMovie.Title);
					Utils.FileDelete(thumbnailImage);
				}
				m_bRefresh = true;
				Close();
				return ;
			}

			if (control==spinImages)
			{
				int item=spinImages.Value-1;

				if (imdbCoverArtUrl == String.Empty)
				{
					if (item < 0 || item >= amazonSearch.Count) item=0;
					currentMovie.ThumbURL = amazonSearch[item];
				}
				else
				{
					if (item == 0)
					{
						currentMovie.ThumbURL = imdbCoverArtUrl;
					}
					else
					{
						if (item-1 < 0 || item-1 >= amazonSearch.Count) item=1;
						currentMovie.ThumbURL = amazonSearch[item-1];
					}
				}
						
				string coverArtImage = Utils.GetCoverArtName(Thumbs.MovieTitle,currentMovie.Title);
				string largeCoverArtImage = Utils.GetLargeCoverArtName(Thumbs.MovieTitle,currentMovie.Title);
				Utils.FileDelete(coverArtImage);
				Utils.FileDelete(largeCoverArtImage);
				Refresh();            
				Update();
				int idMovie = -1;
				if (currentMovie.SearchString != String.Empty)
				{
					try
					{
						idMovie = System.Int32.Parse(currentMovie.SearchString);
					}
					catch(Exception)
					{
					}
				}

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
				int idMovie = System.Int32.Parse(currentMovie.SearchString);
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
				Close();
				GUIVideoFiles.PlayMovie(id);
				return ;
			}
		}

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
        {
          m_bPrevOverlay = GUIGraphicsContext.Overlay;
          m_bRefresh = false;
          base.OnMessage(message);
          return true;
        }
      }
      return base.OnMessage(message);
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
				lblImage.IsVisible=false;
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
				lblImage.IsVisible=true;
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

    
    public override void Render(float timePassed)
    {
			if (!m_bRunning) return;
      RenderDlg(timePassed);
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
    
    public bool NeedsRefresh
    {
      get { return m_bRefresh; }
    }
		void AmazonLookupThread()
		{
			// Search for more pictures
			IMDBMovie movie=currentMovie;
			amazonSearch.Search(movie.Title);

			// Set number of picture URL's (x from Search + 1 from movie database)					
			int pictureCount = amazonSearch.Count+1;
			int pictureIndex = 1;

			// Search selected picture in amazonSearch list					
			int counter=0;
			while (counter < amazonSearch.Count)
			{
				string url=amazonSearch[counter].ToLower();
				if (url.Equals(movie.ThumbURL.ToLower() ) )
				{
					// Duplicate URL found in search list
					imdbCoverArtUrl = String.Empty;
					pictureCount--;
					pictureIndex--;
					break;
				}
				counter++;
			}

			if (currentMovie==null) return;
			spinImages.Reset();
			spinImages.SetReverse(true);
			spinImages.SetRange(1,pictureCount);
			spinImages.Value = 1;

			spinImages.ShowRange=true;
			spinImages.UpDownType =GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
			for (int i=0; i < amazonSearch.Count;++i)
			{
				string url=amazonSearch[i].ToLower();
				if (url.Equals(movie.ThumbURL.ToLower() ) )
				{
					spinImages.Value=pictureIndex+1;								
					break;
				}
				pictureIndex++;
			}
		}
  }
}
