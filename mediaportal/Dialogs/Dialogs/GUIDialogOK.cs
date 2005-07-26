using System;
using System.Collections;
using MediaPortal.GUI.Library;


namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Shows a dialog box with an OK button  
  /// </summary>
  public class GUIDialogOK: GUIWindow
  {

    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
    
		[SkinControlAttribute(10)]			protected GUIButtonControl btnNo=null;
		[SkinControlAttribute(11)]			protected GUIButtonControl btnYes=null;
    bool m_bConfirmed = false;
    bool m_bPrevOverlay=true;

    public GUIDialogOK()
    {
      GetID=(int)GUIWindow.Window.WINDOW_DIALOG_OK;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\dialogOK.xml");
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
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
			OnMessage(msg);
			GUIWindowManager.IsSwitchingToNewWindow=false;

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
					m_pParentWindow=null;
					m_bRunning=false;
          GUIGraphicsContext.Overlay=m_bPrevOverlay;		
          FreeResources();
          DeInitControls();

          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          m_bPrevOverlay=GUIGraphicsContext.Overlay;
          m_bConfirmed = false;
          base.OnMessage(message);
          GUIGraphicsContext.Overlay=false;
        }
        return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iControl=message.SenderControlId;
        
          if ( btnYes == null)
          {
            m_bConfirmed=true;
            Close();
            return true;
          }
          if (iControl==btnNo.GetID)
          {
            m_bConfirmed=false;
            Close();
            return true;
          }
          if (iControl==btnYes.GetID)
          {
            m_bConfirmed=true;
            Close();
            return true;
          }
        }
        break;
      }

      return base.OnMessage(message);
    }


    public bool IsConfirmed
    {
      get { return m_bConfirmed;}
    }

    public void  SetHeading( string strLine)
    {
			LoadSkin();
			AllocResources();
			InitControls();


      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,2,0,0,null);
      msg.Label=strLine; 
      OnMessage(msg);
      SetLine(1,String.Empty);
      SetLine(2,String.Empty);
      SetLine(3,String.Empty);
    }

    public void SetHeading(int iString)
    {
      if (iString==0) SetHeading (String.Empty);
      else SetHeading (GUILocalizeStrings.Get(iString) );
    }

    public void SetLine(int iLine, string strLine)
    {
      if (iLine<=0) return;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,2+iLine,0,0,null);
      msg.Label=strLine; 
      OnMessage(msg);
    }

    public void SetLine(int iLine,int iString)
    {
      if (iLine<=0) return;
      if (iString==0) SetLine (iLine, String.Empty);
      SetLine (iLine, GUILocalizeStrings.Get(iString) );
    }

    public override void Render(float timePassed)
    {
      RenderDlg(timePassed);
    }
  }
}
