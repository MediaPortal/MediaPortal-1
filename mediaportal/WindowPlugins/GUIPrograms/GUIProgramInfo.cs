using System;
using Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using ProgramsDatabase;
using Programs.Utils;


	
namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIFileInfo : GUIWindow
	{
    enum Controls
    {
	  CONTROL_TITLE		=20
      ,	CONTROL_SYSTEM	=21
      ,	CONTROL_YEARMANU 		=22
      ,	CONTROL_RATING	=23
      ,	CONTROL_GENRE		=24

      , CONTROL_IMAGE		 =3
      , CONTROL_TEXTAREA =4

      , CONTROL_BTN_BACK	=5
	  , CONTROL_LAUNCHSTAT = 6
	  , CONTROL_BTN_PREV = 7
	  , CONTROL_BTN_NEXT = 9
	  , CONTROL_BTN_LAUNCH = 8
	  , CONTROL_IMAGE_BIG = 10
	  , CONTROL_OVERVIEW_TOGGLE = 11
    }

    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;

    #endregion

    FileItem m_pFile=null;
	AppItem m_pApp=null;
	Texture m_pTexture=null;
	int m_iTextureWidth=0;
	int m_iTextureHeight=0;
	bool m_bOverlay=false;
	bool m_bOverviewVisible=true;
	int m_iThumbIndex = 0;
	int m_iThumbFolderIndex = -1;
	int m_iSpeed=3;
    int m_lSlideTime=0;


    public GUIFileInfo()
    {
      GetID=(int)ProgramUtils.ProgramInfoID;
    }
    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\DialogFileInfo.xml");
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

	public void RenderDlg()
	{
		// render the parent window
		if (null!=m_pParentWindow) 
			m_pParentWindow.Render();
		GUIFontManager.Present();
 		// render this dialog box
		base.Render();
	}


    void Close()
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      m_pParentWindow=null;
      m_bRunning=false;
    }

    public void DoModal(int dwParentId)
    {
      m_dwParentWindowID=dwParentId;
      m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
      if (null==m_pParentWindow)
      {
        m_dwParentWindowID=0;
        return;
      }

      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      m_bRunning=true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }
    #endregion
	

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					m_pFile=null;
					if (m_pTexture!=null)
					{
						m_pTexture.Dispose();
						m_pTexture=null;
					}
					GUIGraphicsContext.Overlay=m_bOverlay;
				}
					break;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					m_bOverlay=GUIGraphicsContext.Overlay;
					base.OnMessage(message);
					m_pTexture=null;
					m_iThumbIndex = 0;
					m_iThumbFolderIndex = -1;
					Refresh();
					return true;
				}
	
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_BTN_BACK)
					{
						Close();
						return true;
					}
					else if (iControl==(int)Controls.CONTROL_BTN_PREV)
					{
						m_pFile = m_pApp.PrevFile(m_pFile);
						m_iThumbIndex = 0;
						m_iThumbFolderIndex = -1;
						Refresh();
						return true;
					}
					else if (iControl==(int)Controls.CONTROL_BTN_NEXT)
					{
						m_pFile = m_pApp.NextFile(m_pFile);
						m_iThumbIndex = 0;
						m_iThumbFolderIndex = -1;
						Refresh();
						return true;
					}
					else if (iControl==(int)Controls.CONTROL_BTN_LAUNCH)
					{
						if (m_pApp != null)
						{
							m_pApp.LaunchFile(m_pFile, true);
							Refresh();
						}
						return true;
					}
					else if (iControl==(int)Controls.CONTROL_OVERVIEW_TOGGLE)
					{
						this.m_bOverviewVisible = !m_bOverviewVisible;
						Refresh();
						return true;
					}

					

				}
					break;
			
			}

			return base.OnMessage(message);
		}


		public FileItem File
		{
			set {m_pFile=value; }
		}

		public AppItem App
		{
			set {m_pApp=value; }
		}

		private void GetNextThumbFolder()
		{
			bool bFound = false;
			while (!bFound)
			{
				m_iThumbFolderIndex++;
				if (m_iThumbFolderIndex >= m_pApp.ImageDirs.Length)
				{
					m_iThumbFolderIndex = -1;
					bFound = true;
				}
				else
				{
					string strCandFolder = m_pApp.ImageDirs[m_iThumbFolderIndex];
					string strCand = strCandFolder + "\\" + m_pFile.ExtractImageFileNoPath();
					if (strCand.ToLower() != m_pFile.Imagefile.ToLower())
					{
						bFound = (System.IO.File.Exists(strCand));
					}
					else
					{
						// skip the initial directory, in case it's reentered as a search directory!
						bFound = false;
					}
				}
			}

		}

		private string GetNextPicture()
		{
			string strThumb = "";
			if (m_iThumbFolderIndex == -1)
			{
				strThumb = m_pFile.Imagefile; 
			}
			else
			{
				string strFolder = m_pApp.ImageDirs[m_iThumbFolderIndex];
				strThumb = strFolder + "\\" + m_pFile.ExtractImageFileNoPath();
			}
			if (m_iThumbIndex > 0)
			{
				// try to find another thumb....
				// use the myGames convention:
				// every thumb has the postfix "_1", "_2", etc with the same file extension
				string strExtension = m_pFile.ExtractImageExtension();
				string strCand = strThumb.Replace(strExtension, "_" + m_iThumbIndex.ToString() + strExtension);
				if (System.IO.File.Exists(strCand))
				{
					// found another thumb => override the filename!
					strThumb = strCand;
				}
				else 
				{
					m_iThumbIndex = 0; // restart at the first thumb!
					GetNextThumbFolder();
				}
			}
			return strThumb;
		}

		void RefreshPicture()
		{
			if (m_pTexture!=null)
			{
				m_pTexture.Dispose();
				m_pTexture=null;
			}

			if (m_pFile != null)
			{
				string strThumb = GetNextPicture();
				// load the found thumbnail picture
				if (System.IO.File.Exists(strThumb) )
				{
					m_pTexture=Picture.Load(strThumb,0,512,512,true,false,out m_iTextureWidth,out m_iTextureHeight);
				}
				m_iThumbIndex++; // next refresh call will try to get the next thumb
			}
			m_lSlideTime=(int)(DateTime.Now.Ticks/10000); // reset timer!
		}
		
		void Refresh()
		{
			RefreshPicture();
			Update();
		}

		
		void Update()
	    {
			if (null==m_pFile) return;

			if (m_bOverviewVisible)
			{
				GUIControl.HideControl(GetID,(int)Controls.CONTROL_IMAGE_BIG);
				GUIControl.ShowControl(GetID,(int)Controls.CONTROL_TEXTAREA);
				// do not remove the ""-setting line!
				//force a change otherwise subsequent calls of the same fileitem won't display the overview text!
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTAREA,""); 
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTAREA,m_pFile.Overview);
				SetLabel((int)Controls.CONTROL_OVERVIEW_TOGGLE, GUILocalizeStrings.Get(13006));
			}
			else 
			{
				GUIControl.ShowControl(GetID,(int)Controls.CONTROL_IMAGE_BIG);
				GUIControl.HideControl(GetID,(int)Controls.CONTROL_TEXTAREA);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTAREA,""); 
				SetLabel((int)Controls.CONTROL_OVERVIEW_TOGGLE, GUILocalizeStrings.Get(13007));
			}


			SetLabel((int)Controls.CONTROL_TITLE, m_pFile.Title);
			SetLabel((int)Controls.CONTROL_SYSTEM, m_pFile.System_);
			SetLabel((int)Controls.CONTROL_YEARMANU, m_pFile.YearManu);
		    string strRating="";
			if (m_pFile.Rating > 0)
			    strRating=String.Format("{0}/10", m_pFile.Rating);
			SetLabel((int)Controls.CONTROL_RATING, strRating );
			SetLabel((int)Controls.CONTROL_GENRE, m_pFile.Genre);

			

			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_BACK,GUILocalizeStrings.Get(8008));

			if (m_pFile.LaunchCount > 0)
			{
				string strLaunchStat=String.Format("Number of launches: {0} / Last launch: {1}", m_pFile.LaunchCount, m_pFile.LastTimeLaunched);
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LAUNCHSTAT, strLaunchStat);
			}
			else
			{
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LAUNCHSTAT, "");
			}

		}

		public override void Render()
		{
			RenderDlg();

			if (null==m_pTexture) return;

			// does the thumb needs replacing??
			int dwTimeElapsed = ((int)(DateTime.Now.Ticks/10000)) - m_lSlideTime;
			if (dwTimeElapsed >= (m_iSpeed*1000))
			{
				RefreshPicture(); // only refresh the picture, don't refresh the other data otherwise scrolling of labels is interrupted!
			}


			GUIControl pControl = null;
			if (this.m_bOverviewVisible)
			{
				pControl=(GUIControl)GetControl((int)Controls.CONTROL_IMAGE);
			}
			else
			{
				pControl=(GUIControl)GetControl((int)Controls.CONTROL_IMAGE_BIG);
			}
			if (null!=pControl)
			{
				float x=(float)pControl.XPosition;
				float y=(float)pControl.YPosition;
				int iwidth;
				int iheight;
				GUIGraphicsContext.Correct(ref x,ref y);

				int iMaxWidth=pControl.Width;
				int iMaxHeight=pControl.Height;
				GUIGraphicsContext.GetOutputRect(m_iTextureWidth, m_iTextureHeight,iMaxWidth,iMaxHeight, out iwidth,out iheight);
				GUIFontManager.Present();
				Picture.RenderImage(ref m_pTexture,(int)x,(int)y,iwidth,iheight,m_iTextureWidth,m_iTextureHeight,0,0,true);
			}
		}


		void SetLabel(int iControl,  string strLabel)
		{
			string strLabel1=strLabel;
			if (strLabel1.Length==0)
				strLabel1=GUILocalizeStrings.Get(416);
    	
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET,GetID,0,iControl,0,0,null);
			msg.Label=(strLabel1);
			OnMessage(msg);

		}

	}
}
