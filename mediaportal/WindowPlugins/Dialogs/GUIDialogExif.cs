using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Picture.Database;

namespace MediaPortal.Dialogs
{
	/// <summary>
	/// Shows a dialog box with an OK button  
	/// </summary>
	public class GUIDialogExif: GUIWindow
	{
		enum Controls
		{
			Picture=3,
			ImageTitleLabel=20,
			ImageDimensions=21,
			Resolutions=22,
			Flash=23,
			MeteringMode=24,
			ExposureCompensation=25,
			ShutterSpeed=26,
			DateTakenLabel=27,
			Fstop=28,
			ExposureTime=29,
			CameraModel=30,
			EquipmentMake=31,
			ViewComments=32
		};

		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion

    int m_iTextureWidth, m_iTextureHeight;
		bool m_bPrevOverlay=true;
		string fileName;
		Texture m_pTexture = null;

		public GUIDialogExif()
		{
			GetID=(int)GUIWindow.Window.WINDOW_DIALOG_EXIF;
		}

		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\DialogPictureInfo.xml");
		}

		public override bool SupportsDelayedLoad
		{
			get { return true;}
		}    
		public override void PreInit()
		{
		}


		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
			{
				Close();
				return;
			}
			base.OnAction(action);
		}

		#region Base Dialog Members
		public void RenderDlg(long timePassed)
		{
			// render the parent window
			if (null!=m_pParentWindow) 
				m_pParentWindow.Render(timePassed);

			GUIFontManager.Present();
			// render this dialog box
			base.Render(timePassed);
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
				System.Threading.Thread.Sleep(100);
			}
		}
		#endregion
	
		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					GUIGraphicsContext.Overlay=m_bPrevOverlay;		
					FreeResources();
					DeInitControls();
					if (m_pTexture !=null)
						m_pTexture.Dispose();
					m_pTexture =null;

					return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					m_bPrevOverlay=GUIGraphicsContext.Overlay;
					base.OnMessage(message);
					GUIGraphicsContext.Overlay=false;
					Update();
				}
					return true;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					int iControl=message.SenderControlId;
        /*
					if ( GetControl((int)Controls.ID_BUTTON_YES) == null)
					{
						Close();
						return true;
					}*/
				}
					break;
			}

			return base.OnMessage(message);
		}



		public void  SetHeading( string strLine)
		{
			LoadSkin();
			AllocResources();
			InitControls();

			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,2,0,0,null);
			msg.Label=strLine; 
			OnMessage(msg);
		}

		public void SetHeading(int iString)
		{
			if (iString==0) SetHeading ("");
			else SetHeading (GUILocalizeStrings.Get(iString) );
		}


		public string FileName
		{
			get { return fileName;}
			set { fileName=value;}
		}


		void Update()
		{
			if (m_pTexture !=null) 
				m_pTexture.Dispose();


			PictureDatabase dbs = new PictureDatabase();
			int iRotate=dbs.GetRotation(FileName);

			m_pTexture = MediaPortal.Util.Picture.Load(FileName, iRotate, 512, 512, true, false, out m_iTextureWidth, out m_iTextureHeight);

			SetLabel((int)Controls.CameraModel         , String.Empty);
			SetLabel((int)Controls.DateTakenLabel      , String.Empty);
			SetLabel((int)Controls.EquipmentMake       , String.Empty);
			SetLabel((int)Controls.ExposureCompensation, String.Empty);
			SetLabel((int)Controls.ExposureTime        , String.Empty);
			SetLabel((int)Controls.Flash               , String.Empty);
			SetLabel((int)Controls.Fstop               , String.Empty);
			SetLabel((int)Controls.ImageDimensions     , String.Empty);
			SetLabel((int)Controls.ImageTitleLabel     , String.Empty);
			SetLabel((int)Controls.MeteringMode        , String.Empty);
			SetLabel((int)Controls.Resolutions			   , String.Empty);
			SetLabel((int)Controls.ShutterSpeed				 , String.Empty);
			SetLabel((int)Controls.ViewComments				 , String.Empty);

			using (ExifMetadata extractor = new ExifMetadata())
			{
				ExifMetadata.Metadata metaData=extractor.GetExifMetadata(FileName);

				SetLabel((int)Controls.CameraModel         , metaData.CameraModel.DisplayValue);
				SetLabel((int)Controls.DateTakenLabel      , metaData.DatePictureTaken.DisplayValue);
				SetLabel((int)Controls.EquipmentMake       , metaData.EquipmentMake.DisplayValue);
				SetLabel((int)Controls.ExposureCompensation, metaData.ExposureCompensation.DisplayValue);
				SetLabel((int)Controls.ExposureTime        , metaData.ExposureTime.DisplayValue);
				SetLabel((int)Controls.Flash               , metaData.Flash.DisplayValue);
				SetLabel((int)Controls.Fstop               , metaData.Fstop.DisplayValue);
				SetLabel((int)Controls.ImageDimensions     , metaData.ImageDimensions.DisplayValue);
				SetLabel((int)Controls.ImageTitleLabel     , System.IO.Path.GetFileNameWithoutExtension(FileName));
				SetLabel((int)Controls.MeteringMode        , metaData.MeteringMode.DisplayValue);
				SetLabel((int)Controls.Resolutions			   , metaData.Resolution.DisplayValue);
				SetLabel((int)Controls.ShutterSpeed				 , metaData.ShutterSpeed.DisplayValue);
				SetLabel((int)Controls.ViewComments				 , metaData.ViewerComments.DisplayValue);

				GUIImage image = GetControl((int)Controls.Picture) as GUIImage;
				if (image!=null)
					image.IsVisible=false;
			}
		}

		void SetLabel(int label, string strLine)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,label,0,0,null);
			msg.Label=strLine; 
			OnMessage(msg);
		}

		public override void Render(long timePassed)
		{
			RenderDlg(timePassed);
			if (null == m_pTexture) return;
			GUIControl pControl = (GUIControl)GetControl((int)Controls.Picture);
			if (null != pControl)
			{
				float x = (float)pControl.XPosition;
				float y = (float)pControl.YPosition;
				int width;
				int height;
				GUIGraphicsContext.Correct(ref x, ref y);

				GUIFontManager.Present();
				GUIGraphicsContext.GetOutputRect(m_iTextureWidth, m_iTextureHeight, pControl.Width, pControl.Height, out width, out height);
				MediaPortal.Util.Picture.RenderImage(ref m_pTexture, (int)x, (int)y, width, height, m_iTextureWidth, m_iTextureHeight, 0, 0, true);
			}
		}
	}
}
