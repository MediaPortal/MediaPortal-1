using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;

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
    
		bool m_bPrevOverlay=true;
		string fileName;

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
			get { return false;}
		}    
		public override void PreInit()
		{
			AllocResources();
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

		public override void Render()
		{
			RenderDlg();
		}

		void Update()
		{
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
				{
					image.SetFileName(FileName);
				}
			}
		}

		void SetLabel(int label, string strLine)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,label,0,0,null);
			msg.Label=strLine; 
			OnMessage(msg);
		}

	}
}
