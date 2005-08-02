using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;


namespace MediaPortal.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIDialogText: GUIWindow
	{

		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion
		[SkinControlAttribute(2)]			protected GUIButtonControl btnClose=null;
		[SkinControlAttribute(3)]			protected GUITextControl txtArea=null;
		[SkinControlAttribute(4)]			protected GUILabelControl lblHeading=null;
		[SkinControlAttribute(5)]			protected GUIImage imgLogo=null;

		bool    m_bPrevOverlay=false;

		public GUIDialogText()
		{
			GetID=(int)GUIWindow.Window.WINDOW_DIALOG_TEXT;
		}

		public override bool Init()
		{ 
			return Load (GUIGraphicsContext.Skin+@"\DialogText.xml");
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
			else if ( action.wID == Action.ActionType.ACTION_MOVE_DOWN )
			{
				// Frig the action to pretend its a page down
				action.wID = Action.ActionType.ACTION_PAGE_DOWN;
			}
			else if ( action.wID == Action.ActionType.ACTION_MOVE_UP )
			{
				// Frig the action to pretend its a page up
				action.wID = Action.ActionType.ACTION_PAGE_UP;
			}
			base.OnAction(action);
		}

		#region Base Dialog Members
		public void RenderDlg(float timePassed)
		{
			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				if (VMR7Util.g_vmr7!=null)
				{
					using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width,GUIGraphicsContext.Height))
					{
						using (Graphics g = Graphics.FromImage(bmp))
						{
							GUIGraphicsContext.graphics=g;

							// render the parent window
							if (null!=m_pParentWindow) 
								m_pParentWindow.Render(timePassed);

							GUIFontManager.Present();
							// render this dialog box
							base.Render(timePassed);

							GUIGraphicsContext.graphics=null;
							VMR7Util.g_vmr7.SaveBitmap(bmp,true,true,1.0f);
							g.Dispose();
							bmp.Dispose();
						}
					}
					return;
				}
			}
			// render the parent window
			if (null!=m_pParentWindow) 
				m_pParentWindow.Render(timePassed);

			GUIFontManager.Present();
			// render this dialog box
			base.Render(timePassed);
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
			txtArea.Label=text;
		}
		public void SetImage(string filename)
		{
      
			imgLogo.FreeResources();
			imgLogo.SetFileName(filename);
			imgLogo.AllocResources();
		}
    public void SetImageDimensions(Size size, bool keepAspectRatio,bool centered)
    {
      imgLogo.Width = size.Width;
      imgLogo.Height = size.Height;
      imgLogo.KeepAspectRatio = keepAspectRatio;
      imgLogo.Centered = centered;
    }
		public override void Render(float timePassed)
		{
			RenderDlg(timePassed);
		}
	}
}
