using System;
using System.Collections;
using MediaPortal.GUI.Library;


namespace MediaPortal.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIDialogNotify: GUIWindow
	{

		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion
		[SkinControlAttribute(4)]			protected GUIButtonControl btnClose=null;
		[SkinControlAttribute(3)]			protected GUILabelControl lblHeading=null;
		[SkinControlAttribute(5)]			protected GUIImage imgLogo=null;
		[SkinControlAttribute(6)]			protected GUITextControl txtArea=null;

		bool    m_bPrevOverlay=false;
		int     timeOutInSeconds=5;

		public GUIDialogNotify()
		{
			GetID=(int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY;
		}

		public override bool Init()
		{ 
			return Load (GUIGraphicsContext.Skin+@"\DialogNotify.xml");
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
			GUIWindowManager.IsSwitchingToNewWindow=true;
			lock (this)
			{
				GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
				OnMessage(msg);

				GUIWindowManager.UnRoute();
				m_pParentWindow=null;
				m_bRunning=false;
			}
			GUIWindowManager.IsSwitchingToNewWindow=false;
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

			GUIWindowManager.IsSwitchingToNewWindow=true;
			GUIWindowManager.RouteToWindow( GetID );

			// active this window...
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,-1,0,null);
			OnMessage(msg);

			GUIWindowManager.IsSwitchingToNewWindow=false;
			m_bRunning=true;
			DateTime timeStart=DateTime.Now;
			while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
			{
				GUIWindowManager.Process();
				if (!GUIGraphicsContext.Vmr9Active)
					System.Threading.Thread.Sleep(100);

				TimeSpan timeElapsed=DateTime.Now-timeStart;
				if (TimeOut>0)
				{
					if (timeElapsed.TotalSeconds>=TimeOut)
					{
						Close();
						return;
					}
				}
			}
		}
		#endregion
	
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnClose)
			{
				Close();
			}
		}

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
				}
					return true;

			}

			return base.OnMessage(message);
		}

		public void Reset()
		{
			LoadSkin();
			AllocResources();
			InitControls();
			timeOutInSeconds=5;
		}

		public void  SetHeading( string strLine)
		{
			LoadSkin();
			AllocResources();
			InitControls();

			lblHeading.Label=strLine;
		}


		public void SetHeading(int iString)
		{

			SetHeading (GUILocalizeStrings.Get(iString) );
		}

		public void SetText(string text)
		{
			LoadSkin();
			AllocResources();
			InitControls();
			txtArea.Label=text;
		}
		public void SetImage(string filename)
		{
			LoadSkin();
			AllocResources();
			InitControls();
			imgLogo.FreeResources();
			imgLogo.SetFileName(filename);
			imgLogo.AllocResources();
		}
		public override void Render(float timePassed)
		{
			RenderDlg(timePassed);
		}

		public int TimeOut
		{
			get
			{
				return timeOutInSeconds;
			}
			set
			{
				timeOutInSeconds=value;
			}

		}

	}
}
