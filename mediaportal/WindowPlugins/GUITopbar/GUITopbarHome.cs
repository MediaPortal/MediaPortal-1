using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.Topbar
{
  /// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class GUITopbarHome : GUIWindow
  {
    const int HIDE_SPEED = 8;

    enum Controls 
    {
      TOPBAR_BACKGROUND=1
    };

    bool m_bFocused=false;
    bool m_bEnabled=false;
    bool m_bAutoHide=false;
    bool m_bTopBarEffect=false;
    bool m_bTopBarHidden=false;
    int m_iMoveUp=0;
    int m_iMoveUpCount=0;
    int m_iAutoHideTimeOut=25;
        
    public GUITopbarHome()
		{
		}
    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\topbarhome.xml");
      GetID=(int)GUIWindow.Window.WINDOW_TOPBARHOME;
      m_bEnabled=PluginManager.IsPluginNameEnabled("Topbar");

      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_iAutoHideTimeOut = xmlreader.GetValueAsInt("TopBar", "autohidetimeout", 15);

        m_bAutoHide = false;
        if (xmlreader.GetValueAsInt("TopBar", "autohide", 0) == 1) m_bAutoHide = true;
      }

      return bResult;
    }
    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    
    public override void PreInit()
    {
      AllocResources();
    }
    public override void Render()
    {
    }
    public override bool DoesPostRender()
    {
      if (!m_bEnabled) return false;
      if (GUIWindowManager.ActiveWindow!=(int)GUIWindow.Window.WINDOW_HOME)  return false;
      if (GUIGraphicsContext.IsFullScreenVideo) return false;
      return true;
    }
    public override void PostRender(int iLayer)
    {
      if (!m_bEnabled) return;
      if (iLayer !=1) return;
      CheckFocus();

      // Check autohide timeout
      if (m_bFocused)
      {
        m_bTopBarHidden = false;
        GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
      }

      TimeSpan ts=DateTime.Now-GUIGraphicsContext.TopBarTimeOut;
      if (m_bAutoHide && (ts.TotalSeconds > m_iAutoHideTimeOut) && !m_bTopBarHidden)
      {
        // Hide topbar with effect
        m_bTopBarHidden = true;

        GUIControl cntl=GetControl((int)Controls.TOPBAR_BACKGROUND);
        if (cntl!=null)
        {
          m_iMoveUpCount = cntl.Height;
          m_iMoveUp=0;
        }							
      }

      if (GUIGraphicsContext.TopBarHidden != m_bTopBarHidden)
      {
        m_bTopBarEffect = true;
      }

      if (m_bTopBarEffect)
      {
        if (m_bTopBarHidden)
        {
          m_iMoveUp+=HIDE_SPEED;
          if (m_iMoveUp >= m_iMoveUpCount) 
          {
            GUIGraphicsContext.TopBarHidden = true;
            m_bTopBarEffect = false;
          }
        }
        else
        {
          GUIGraphicsContext.TopBarHidden = false;
          m_iMoveUp = 0;            
          /*m_iMoveUp-=HIDE_SPEED;
          if (m_iMoveUp < 0) 
          {
            m_iMoveUp = 0;            
            m_bTopBarEffect = false;
          }*/
        }

        foreach (CPosition pos in m_vecPositions)
        {
          pos.control.SetPosition((int)pos.XPos,(int)pos.YPos - m_iMoveUp);         
        }
      }

      if (GUIGraphicsContext.TopBarHidden) return;     
     
      base.Render();
    }

    public void CheckFocus()
    {
      if (GUIWindowManager.IsRouted)
      {
        m_bFocused=false;
      }
      if (!m_bFocused)
      {
        foreach (GUIControl control in m_vecControls)
        {
          control.Focus=false;
        }
      }
    }

    public override bool Focused
    {
      get { 
        return m_bFocused;
      }
      set {
        m_bFocused=value;
        if (m_bFocused==true)
        {
          // reset autohide timer
          GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
          m_bTopBarHidden = false;

          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID, 0,m_dwDefaultFocusControlID,0,0,null);
          OnMessage(msg);
        }
        else
        {
          foreach (GUIControl control in m_vecControls)
          {
            control.Focus=false;
          }
        }
      }
    }

    public override void OnAction(Action action)
    {
      CheckFocus();
      if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
      {
        // reset autohide timer       
        if (m_bTopBarHidden)
        {
          if (action.fAmount2 < m_iMoveUpCount)
          {
            GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
            m_bTopBarHidden = false;
          }
        }

        foreach (GUIControl control in m_vecControls)
        {
          bool bFocus;
          int id;
          if (control.HitTest((int)action.fAmount1,(int)action.fAmount2,out id, out bFocus))
          {	
            if (!bFocus)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID,0,id,0,0,null);
              OnMessage(msg);
              control.HitTest((int)action.fAmount1,(int)action.fAmount2,out id, out bFocus);
            }
            control.OnAction(action);
            m_bFocused=true;
            return ;
          }
        }
        
        Focused=false;
        return ;
      }
      base.OnAction (action);
      if (action.wID==Action.ActionType.ACTION_MOVE_DOWN)
      {
        // reset autohide timer
        GUIGraphicsContext.TopBarTimeOut = DateTime.Now;
        Focused=false;
      }
    }

	}
}
