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

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIVideoInfo : GUIWindow
	{
    enum Controls
    {
      CONTROL_TITLE = 20
      ,	CONTROL_DIRECTOR = 21
      ,	CONTROL_CREDITS = 22
      ,	CONTROL_GENRE = 23
      ,	CONTROL_YEAR = 24
      ,	CONTROL_TAGLINE = 25
      ,	CONTROL_PLOTOUTLINE = 26
      ,	CONTROL_RATING = 27
      ,	CONTROL_VOTES = 28
      ,	CONTROL_CAST = 29
      ,	CONTROL_IMAGE_LABEL = 30

      , CONTROL_IMAGE = 3
      , CONTROL_SPIN = 4
			, CONTROL_TEXTAREA = 5
			, CONTROL_PLOTAREA = 10

      , CONTROL_BTN_TRACKS = 6
      , CONTROL_BTN_REFRESH = 7
      , CONTROL_DISC = 8
			, CONTROL_PLAY=9
			, CONTROL_STARS=100
    };
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

    const string ThumbsFolder=@"thumbs\Videos\Title";
    ViewMode viewmode= ViewMode.Image;
    bool m_bRefresh = false;
    IMDBMovie m_movie = null;
    bool m_bPrevOverlay = false;
    AmazonImageSearch m_search = new AmazonImageSearch();
    string m_sIMDBThumbURL = "";

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
			lock (this)
			{
				m_bRunning = false;
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
				OnMessage(msg);

				GUIWindowManager.UnRoute();
				m_pParentWindow = null;
			}
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

      GUIWindowManager.RouteToWindow(GetID);

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      m_bRunning = true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }
    #endregion
	
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
        {
          m_movie = null;
          GUIGraphicsContext.Overlay = m_bPrevOverlay;
        }
        break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
        {
          m_bPrevOverlay = GUIGraphicsContext.Overlay;
          m_bRefresh = false;
          base.OnMessage(message);

          // Default picture					
          m_sIMDBThumbURL = m_movie.ThumbURL;

          // Search for more pictures
          m_search.Search(m_movie.Title);

          // Set number of picture URL's (x from Search + 1 from movie database)					
          int m_iPictureCount = m_search.Count+1;
          int m_iPictureIndex = 1;

          // Search selected picture in m_search list					
          int iLoop=0;
          while (iLoop < m_search.Count)
          {
            string url=m_search[iLoop].ToLower();
            if (url.Equals(m_movie.ThumbURL.ToLower() ) )
            {
              // Duplicate URL found in search list
              m_sIMDBThumbURL = "";
              m_iPictureCount--;
              m_iPictureIndex--;
              break;
            }
            iLoop++;
          }

          GUIControl.ClearControl(GetID, (int)Controls.CONTROL_SPIN);
          GUISpinControl spin = GetControl((int)Controls.CONTROL_SPIN) as GUISpinControl;
          if (spin!=null)
          {
            spin.SetReverse(true);
            spin.SetRange(1,m_iPictureCount);
            spin.Value = 1;

            spin.ShowRange=true;
            spin.UpDownType =GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
            for (int i=0; i < m_search.Count;++i)
            {
              string url=m_search[i].ToLower();
              if (url.Equals(m_movie.ThumbURL.ToLower() ) )
              {
                spin.Value=m_iPictureIndex+1;								
                break;
              }
              m_iPictureIndex++;
            }
          }
        
          viewmode=ViewMode.Image;			    
          GUIControl.ClearControl(GetID, (int)Controls.CONTROL_DISC);
          GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_DISC, "HD");
          for (int i = 0; i < 1000; ++i)
          {
            string strItem = String.Format("DVD#{0:000}", i);
            GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_DISC, strItem);
          }
          
          GUIControl.HideControl(GetID, (int)Controls.CONTROL_DISC);
          GUIControl.DisableControl(GetID, (int)Controls.CONTROL_DISC);
          int iItem = 0;
          if (Utils.IsDVD(m_movie.Path))
          {
            GUIControl.ShowControl(GetID, (int)Controls.CONTROL_DISC);
            GUIControl.EnableControl(GetID, (int)Controls.CONTROL_DISC);
            string szNumber = "";
            int iPos = 0;
            bool bNumber = false;
            for (int i = 0; i < m_movie.DVDLabel.Length; ++i)
            {
              char kar = m_movie.DVDLabel[i];
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
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_DISC, iItem);
          }
          Refresh();Update();
          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_CLICKED : 
        {
          int iControl = message.SenderControlId;
          if (iControl == (int)Controls.CONTROL_BTN_REFRESH)
          {
            if (m_movie.ThumbURL.Length > 0)
            {
              string strThumb = "";
              string strImage = m_movie.Title;
              strThumb = Utils.GetCoverArt(ThumbsFolder,strImage);
              Utils.FileDelete(strThumb);
            }
            m_bRefresh = true;
            Close();
            return true;
          }

          if (iControl == (int)Controls.CONTROL_SPIN)
          {
            GUISpinControl spin = (GUISpinControl)GetControl(iControl);
            int item=spin.Value-1;

            if (m_sIMDBThumbURL == "")
            {
              m_movie.ThumbURL = m_search[item];
            }
            else
            {
              if (item == 0)
              {
                m_movie.ThumbURL = m_sIMDBThumbURL;
              }
              else
              {
                m_movie.ThumbURL = m_search[item-1];
              }
            }
						
            string strThumb = Utils.GetCoverArtName(ThumbsFolder,m_movie.Title);
            string LargeThumb = Utils.GetLargeCoverArtName(ThumbsFolder,m_movie.Title);
            Utils.FileDelete(strThumb);
            Utils.FileDelete(LargeThumb);
            Refresh();            
            int lMovieId = -1;
            if (m_movie.SearchString != "")
            {
              try
              {
                lMovieId = System.Int32.Parse(m_movie.SearchString);
              }
              catch(Exception)
              {
              }
            }

            if (lMovieId>=0)
              VideoDatabase.SetThumbURL(lMovieId,m_movie.ThumbURL);
            return true;
          }

          if (iControl == (int)Controls.CONTROL_BTN_TRACKS)
          {
            switch (viewmode)
            {
              case ViewMode.Image: 
                viewmode=ViewMode.Cast;
                break;
                
              case ViewMode.Cast: 
                viewmode=ViewMode.Image;
                break;
            }
            Update();
          }

          if (iControl == (int)Controls.CONTROL_DISC)
          {
            GUISpinControl cntl = (GUISpinControl)GetControl(iControl);
            string strItem = cntl.GetLabel();
            int lMovieId = System.Int32.Parse(m_movie.SearchString);
            if (lMovieId > 0)
            {
              if (strItem != "HD" && strItem != "share") 
              {
                VideoDatabase.SetDVDLabel(lMovieId, strItem);
              }
              else
              {
                VideoDatabase.SetDVDLabel(lMovieId, "HD");
              }
            }
          }

					if (iControl==(int)Controls.CONTROL_PLAY)
					{
						Close();
						GUIVideoTitle.PlayMovie(m_movie.ID);
						return;
					}
        }
        break;
      }

      return base.OnMessage(message);
    }

    public IMDBMovie Movie
    {
      get { return m_movie; }
      set { m_movie = value; }
    }

    void Update()
    {
      if (m_movie == null) return;

      //cast->image
      if (viewmode==ViewMode.Cast)
      {
				GUIControl.HideControl(GetID,(int)Controls.CONTROL_PLOTAREA);
        GUIControl.ShowControl(GetID,(int)Controls.CONTROL_TEXTAREA);
				GUIControl.ShowControl(GetID,(int)Controls.CONTROL_IMAGE);
				GUIControl.HideControl(GetID,(int)Controls.CONTROL_IMAGE_LABEL);
        GUIControl.HideControl(GetID,(int)Controls.CONTROL_SPIN);
      }
      //cast->plot
      if (viewmode==ViewMode.Image)
			{
				GUIControl.ShowControl(GetID,(int)Controls.CONTROL_PLOTAREA);
				GUIControl.HideControl(GetID,(int)Controls.CONTROL_TEXTAREA);
				GUIControl.ShowControl(GetID,(int)Controls.CONTROL_IMAGE);
        GUIControl.ShowControl(GetID,(int)Controls.CONTROL_IMAGE_LABEL);
        GUIControl.ShowControl(GetID,(int)Controls.CONTROL_SPIN);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_TRACKS, GUILocalizeStrings.Get(207));
      }
    }

    
    public override void Render(float timePassed)
    {
			if (!m_bRunning) return;
      RenderDlg(timePassed);
    }

    
    void Refresh()
    {
			string strThumb = "";
			try
      {

        string strImage = m_movie.ThumbURL;
        if (strImage.Length > 0)
        {
          string LargeThumb = Utils.GetLargeCoverArtName(ThumbsFolder,m_movie.Title);
          strThumb = Utils.GetCoverArtName(ThumbsFolder,m_movie.Title);
          if (!System.IO.File.Exists(strThumb))
          {
            string strExtension;
            strExtension = System.IO.Path.GetExtension(strImage);
            if (strExtension.Length > 0)
            {
              string strTemp = "temp";
              strTemp += strExtension;
              Utils.FileDelete(strTemp);
             
              Utils.DownLoadAndCacheImage(strImage, strTemp);
              if (System.IO.File.Exists(strTemp))
              {
                MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
                MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);
              }

              Utils.FileDelete(strTemp);
            }//if ( strExtension.Length>0)
            else
            {
              Log.Write("image has no extension:{0}", strImage);
            }
          }
        }
        
       
        Update();
      }
      catch (Exception)
      {
			}
			m_movie.SetProperties();
		}
    
    public bool NeedsRefresh
    {
      get { return m_bRefresh; }
    }
  }
}
