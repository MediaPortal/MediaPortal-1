using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIDialogSetRating: GUIWindow
	{
		public enum ResultCode
		{
			Close,
			Next,
			Previous
		};
		enum Controls
		{
			ID_LABEL_NAME  = 4
			,	ID_BUTTON_MIN   =11
			, ID_BUTTON_PLUS  =10
			, ID_BUTTON_OK = 12
			, ID_NEXTITEM=13
			, ID_BUTTON_PLAY=14
			, ID_PREVITEM=15
			,	CONTROL_STARS=100
		};

		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion
    
		bool m_bPrevOverlay=true;
		int  m_iRating=1;
		string fileName;
		ResultCode resultCode;

		public GUIDialogSetRating()
		{
			GetID=(int)GUIWindow.Window.WINDOW_DIALOG_RATING;
		}

		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\dialogRating.xml");
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
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				Close();
				return;
			}
			base.OnAction(action);
		}

		#region Base Dialog Members
		public void RenderDlg(float timePassed)
		{
			
			lock (this)
			{
				// render the parent window
				if (null!=m_pParentWindow) 
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
				GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
				OnMessage(msg);

				GUIWindowManager.UnRoute();
				m_pParentWindow=null;
				m_bRunning=false;
			}
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
					resultCode=ResultCode.Close;
					m_bPrevOverlay=GUIGraphicsContext.Overlay;
					base.OnMessage(message);
					GUIGraphicsContext.Overlay=false;
					Update();
				}
					return true;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					int iControl=message.SenderControlId;
        
					if (iControl==(int)Controls.ID_BUTTON_OK)
					{
						Close();
						resultCode=ResultCode.Close;
						return true;
					}
					if (iControl==(int)Controls.ID_NEXTITEM)
					{
						Close();
						resultCode=ResultCode.Next;
						return true;
					}
					if (iControl==(int)Controls.ID_PREVITEM)
					{
						Close();
						resultCode=ResultCode.Previous;
						return true;
					}
					if (iControl==(int)Controls.ID_BUTTON_PLAY)
					{
						Log.Write("DialogSetRating:Play:{0}",FileName);
						g_Player.Play(FileName);
					}

					if (iControl==(int)Controls.ID_BUTTON_MIN)
					{
						if (m_iRating >=1) m_iRating--;
						Update();
						return true;
					}
					if (iControl==(int)Controls.ID_BUTTON_PLUS)
					{
						if (m_iRating<5) m_iRating++;
						Update();
						return true;
					}
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

		public void SetTitle(string title)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Controls.ID_LABEL_NAME,0,0,null);
			msg.Label=title; 
			OnMessage(msg);
		}

		void Update()
		{
			for (int i=0; i < 5; ++i)
			{
				if ( (i+1) > (int)(Rating) )
					GUIControl.HideControl(GetID, (int)Controls.CONTROL_STARS+i);
				else
					GUIControl.ShowControl(GetID, (int)Controls.CONTROL_STARS+i);
			}
		}

		public override void Render(float timePassed)
		{
			RenderDlg(timePassed);
		}
		public int Rating
		{
			get { return m_iRating;}
			set {m_iRating=value;}
		}
		public string FileName
		{
			get { return fileName;}
			set {fileName=value;}
		}

		public ResultCode Result
		{
			get { return resultCode;}
		}
	}
}
