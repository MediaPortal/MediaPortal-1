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

		#region SkinControls
		// Labels
		[SkinControlAttribute(20)]   protected GUILabelControl lblTitle=null;
		[SkinControlAttribute(31)]   protected GUILabelControl lblSystemCaption=null;
		[SkinControlAttribute(32)]   protected GUILabelControl lblYearManuCaption=null;
		[SkinControlAttribute(33)]   protected GUILabelControl lblRatingCaption=null;
		[SkinControlAttribute(34)]   protected GUILabelControl lblGenreCaption=null;
		[SkinControlAttribute(6)]    protected GUILabelControl lblLaunchStat=null;

		// Fadelabels
		[SkinControlAttribute(21)]   protected GUIFadeLabel lblSystemData=null;
		[SkinControlAttribute(22)]   protected GUIFadeLabel lblYearManuData=null;
		[SkinControlAttribute(23)]   protected GUIFadeLabel lblRatingData=null;
		[SkinControlAttribute(24)]   protected GUIFadeLabel lblGenreData=null;

                                             
		// Textbox                   
		[SkinControlAttribute(4)]    protected GUITextScrollUpControl tbOverviewData=null;
                                             
		//Images                     
		[SkinControlAttribute(3)]    protected GUIImage imgSmall=null;
		[SkinControlAttribute(10)]   protected GUIImage imgBig=null;
                                             
		// Buttons                   
		[SkinControlAttribute(5)]    protected GUIButtonControl btnBack=null;
		[SkinControlAttribute(7)]    protected GUIButtonControl btnPrev=null;
		[SkinControlAttribute(8)]    protected GUIButtonControl btnLaunch=null;
		[SkinControlAttribute(9)]    protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(11)]   protected GUIButtonControl btnToggleOverview=null;
		#endregion

		#region Base & Content Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		FileItem m_pFile=null;
		AppItem m_pApp=null;
		Texture m_pTexture=null;
		int m_iTextureWidth=0;
		int m_iTextureHeight=0;
		bool m_bOverlay=false;
		bool m_bOverviewVisible=true;
		int m_iSpeed=3;
		int m_lSlideTime=0;


		string strSystemLabel = "";
		string strManufacturerLabel = "";
		string strRatingLabel = "";
		string strGenreLabel = "";

		string strSystemText = "";
		string strManufacturerText = "";
		string strRatingText = "";
		string strGenreText = "";

		string strOverviewText = "";
		#endregion

		#region Constructor / Destructor
		public GUIFileInfo()
		{
			GetID=(int)ProgramUtils.ProgramInfoID;
		}
		#endregion

		#region Properties

		public FileItem File
		{
			set {m_pFile=value; }
		}

		public AppItem App
		{
			set 
			{
				m_pApp=value; 
				if (m_pApp != null)
					m_pApp.ResetThumbs();
			}
		}
		#endregion

		#region Overrides

		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\DialogFileInfo.xml");
		}


		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			m_bOverlay=GUIGraphicsContext.Overlay;
			m_pTexture=null;

			if (m_pApp != null)
			{
				m_pApp.ResetThumbs();
			}
			// if there is no overview text, default to bigger pictures
			if (m_pFile != null)
			{
				if (m_pFile.Overview == "")
				{
					this.m_bOverviewVisible = false;
				}
			}
			Refresh();
		}


		protected override void OnPageDestroy(int newWindowId)
		{
			m_pFile=null;
			if (m_pTexture!=null)
			{
				m_pTexture.Dispose();
				m_pTexture=null;
			}
			GUIGraphicsContext.Overlay=m_bOverlay;
			base.OnPageDestroy (newWindowId);
		}


		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control == btnBack)
			{
				Close();
			}
			else if (control == btnPrev)
			{
				m_pFile = m_pApp.PrevFile(m_pFile);
				m_pApp.ResetThumbs();
				Refresh();
			}
			else if (control == btnNext)
			{
				m_pFile = m_pApp.NextFile(m_pFile);
				m_pApp.ResetThumbs();
				Refresh();
			}
			else if (control == btnLaunch)
			{
				if (m_pApp != null)
				{
					m_pApp.LaunchFile(m_pFile, true);
					Refresh();
				}

			}
			else if (control == btnToggleOverview)
			{
				m_bOverviewVisible = !m_bOverviewVisible;
				Refresh();
			}
		}


		public override void OnAction(Action action)
		{
			if ((action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) || (action.wID == Action.ActionType.ACTION_PARENT_DIR))
			{
				Close();
				return;
			}

			base.OnAction(action);
		}

		public override void Render(float timePassed)
		{
			RenderDlg(timePassed);

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
				pControl = imgSmall;
			}
			else
			{
				pControl = imgBig;
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

		#endregion

		#region Display

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

		void RefreshPicture()
		{
			if (m_pTexture!=null)
			{
				m_pTexture.Dispose();
				m_pTexture=null;
			}

			if (m_pFile != null)
			{
				string strThumb = m_pApp.GetCurThumb(m_pFile); 
				// load the found thumbnail picture
				if (System.IO.File.Exists(strThumb) )
				{
					m_pTexture=Picture.Load(strThumb,0,512,512,true,false,out m_iTextureWidth,out m_iTextureHeight);
				}
				m_pApp.NextThumb(); // try to find a next thumbnail
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

			ReadContent();

			if (m_bOverviewVisible)
			{
				imgBig.IsVisible = false;
				tbOverviewData.IsVisible = true;
				tbOverviewData.Label = ""; // force clear first....
				tbOverviewData.Label = strOverviewText; // ... and set text next!
				btnToggleOverview.Label = GUILocalizeStrings.Get(13006);
			}
			else 
			{
				imgBig.IsVisible = true;
				tbOverviewData.IsVisible = false;
				tbOverviewData.Label = "";
				btnToggleOverview.Label = GUILocalizeStrings.Get(13007);
			}

			lblTitle.Label = m_pFile.Title;

			// if any title is overwritten, re-set the fresh text
			if (strSystemLabel != "")
			{
				lblSystemCaption.Label = strSystemLabel;
			}
			else
			{
				lblSystemCaption.Label = GUILocalizeStrings.Get(13000);
			}
			if (strManufacturerLabel != "")
			{
				lblYearManuCaption.Label = strManufacturerLabel;
			}
			else
			{
				lblYearManuCaption.Label = GUILocalizeStrings.Get(13001);
			}
		
			if (strRatingLabel != "")
			{
				lblRatingCaption.Label = strRatingLabel;
			}
			else
			{
				lblRatingCaption.Label = GUILocalizeStrings.Get(173);
			}
			if (strGenreLabel != "")
			{
				lblGenreCaption.Label = strGenreLabel;
			}
			else
			{
				lblGenreCaption.Label = GUILocalizeStrings.Get(174);
			}

			lblSystemData.Label = strSystemText;
			lblYearManuData.Label = strManufacturerText;
			lblRatingData.Label = strRatingText;
			lblGenreData.Label = strGenreText;

			btnBack.Label = GUILocalizeStrings.Get(8008);

			if (m_pFile.Filename != "")
			{
				btnLaunch.Disabled = false;
			}
			else
			{
				btnLaunch.Disabled = true;
			}

			lblLaunchStat.Label = ""; // always disable..... doesn't look nice!!
		}


		void ReadContent()
		{
			// read fields out of the content profile
			// fields can contain texts and / or references to fields
			strSystemLabel = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line1Label");
			strManufacturerLabel = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line2Label");
			strRatingLabel = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line3Label");
			strGenreLabel = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line4Label");

			strSystemText = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line1Data");
			strManufacturerText = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line2Data");
			strRatingText = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line3Data");
			strGenreText = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "Line4Data");
			strOverviewText = ProgramContentManager.GetFieldValue(m_pApp, m_pFile, "OverviewData");
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


		public void RenderDlg(float timePassed)
		{
			// render the parent window
			if (null!=m_pParentWindow) 
				m_pParentWindow.Render(timePassed);
			GUIFontManager.Present();
			// render this dialog box
			base.Render(timePassed);
		}


		#endregion

	}
}
