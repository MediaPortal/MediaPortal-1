using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.Topbar
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class GUITopbarHome : GUIWindow
  {
    bool m_bFocused=false;
    bool m_bEnabled=false;
		public GUITopbarHome()
		{
		}
    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\topbarhome.xml");
      GetID=(int)GUIWindow.Window.WINDOW_TOPBARHOME;
      m_bEnabled=PluginManager.IsPluginNameEnabled("Topbar");
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
        Focused=false;
      }
    }

	}
}
