using System;
using System.Threading;
using System.Collections;
using System.Reflection;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// static class which takes care of window management
	/// </summary>
  public class GUIWindowManager
  {

		public delegate void OnCallBackHandler();
		static public event  SendMessageHandler Receivers;
		static public event  OnCallBackHandler  Callbacks;
    static ArrayList		 m_vecWindows				= new ArrayList();
    static ArrayList		 m_vecThreadMessages	= new ArrayList();
    static int					 m_iActiveWindow=-1;
    static int					 m_iActiveWindowID=-1;
    static GUIWindow     m_pRouteWindow=null;
    static bool          m_bRefresh=false;
    static bool          m_bSwitching=false;

    // singleton. Dont allow any instance of this class
    private GUIWindowManager()
    {
    }
    /// <summary>
    /// Send message to window/control
    /// </summary>
    /// <param name="message">message to send</param>
    static public void SendMessage(GUIMessage message)
    {

      if (message.Message==GUIMessage.MessageType.GUI_MSG_LOSTFOCUS||
        message.Message==GUIMessage.MessageType.GUI_MSG_SETFOCUS)
      {
        for (int x=0; x < m_vecWindows.Count;++x)
        {
          if ( ((GUIWindow)m_vecWindows[x]).DoesPostRender() && ((GUIWindow)m_vecWindows[x]).Focused)
          {
            ((GUIWindow)m_vecWindows[x]).OnMessage(message);
            if (((GUIWindow)m_vecWindows[x]).Focused) return;
          }
        }
      }

      // send message to other objects interested
			if (Receivers!=null)
			{
				Receivers(message);
			}

      // if dialog is onscreen, then send message to that window
			if (null!=m_pRouteWindow)
			{
        if (message.TargetWindowId==m_pRouteWindow.GetID)
        {
          m_pRouteWindow.OnMessage(message);
          return;
        }
			}

      GUIWindow pWindow=null;
      GUIWindow activewindow=null;
      if (m_iActiveWindow >= 0) 
      {
        activewindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
        if (message.SendToTargetWindow==true)
        {
          for (int i=0; i < m_vecWindows.Count;++i)
          {
            pWindow=(GUIWindow)m_vecWindows[i];
            if (pWindow.GetID==message.TargetWindowId)
            {
              pWindow.OnMessage(message);
              return;
            }
          }
          return;
        }
      }

      // else send message to the current active window
			if (activewindow!=null)
			  activewindow.OnMessage(message);
    }

    /// <summary>
    /// Initialize the window manager
    /// </summary>
    static public void Initialize()
    {
      //no active window yet
			m_iActiveWindow=-1;
      m_iActiveWindowID=-1;
      m_bSwitching=false;
      
      //register ourselves for the messages from the GUIGraphicsContext 
			GUIGraphicsContext.Receivers += new SendMessageHandler(SendMessage);
    }

    /// <summary>
    /// Add new window to the window manager
    /// </summary>
    /// <param name="Window">new window to add</param>
    static public void Add(ref GUIWindow Window)
    {
      foreach (GUIWindow win in m_vecWindows)
      {
        if (win.GetID==Window.GetID)
        {
          Log.Write("Window:{0} and window {1} have the same id's!!!", Window,win);
          return;
        }
      }
			m_vecWindows.Add(Window);
    }
    static public bool IsSwitchingToNewWindow
    {
      get { return m_bSwitching;}
    }

    /// <summary>
    /// call ResetallControls() for every window
    /// </summary>
    static public void ResetAllControls()
    {
      for (int x=0; x < m_vecWindows.Count;++x)
      {
        ((GUIWindow)m_vecWindows[x]).ResetAllControls();
      }
    }

    /// <summary>
    /// called by the runtime when DirectX device has been restored
    /// Just let current active window know about this
    /// </summary>
    static public void OnDeviceRestored()
    {
      for (int x=0; x < m_vecWindows.Count;++x)
      {
        ((GUIWindow)m_vecWindows[x]).OnDeviceRestored();
      }
    }

    /// <summary>
    /// Called by the runtime when the DirectX device has been lost
    /// just let the current active window know about this
    /// </summary>
    static public void OnDeviceLost()
    {
      for (int x=0; x < m_vecWindows.Count;++x)
      {
        ((GUIWindow)m_vecWindows[x]).OnDeviceLost();
      }
    }

    /// <summary>
    /// ActivateWindow() 
    /// This function will show/present/activate the window specified
    /// </summary>
    /// <param name="iWindowID">window id of the window to activate</param>
    static public void ActivateWindow(int iWindowID)
    {
      m_bSwitching=true;
      try
      {
        for (int x=0; x < m_vecWindows.Count;++x)
        {
          if (((GUIWindow)m_vecWindows[x]).DoesPostRender() )
          {
            ((GUIWindow)m_vecWindows[x]).Focused=false;
          }
        }      
        // deactivate any window
        GUIMessage msg;
        GUIWindow pWindow;
        int iPrevActiveWindow=m_iActiveWindow;
        if (m_iActiveWindow >=0)
        {
          pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
          msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,pWindow.GetID,0,0,iWindowID,0,null);
          pWindow.OnMessage(msg);
          m_iActiveWindow=-1;
          m_iActiveWindowID=-1;
        }

        // activate the new window
        for (int i=0; i < m_vecWindows.Count; i++)
        {
          pWindow=(GUIWindow )m_vecWindows[i];
          if (pWindow.GetID == iWindowID) 
          {
            m_iActiveWindow=i;
            m_iActiveWindowID=iWindowID;

            // Check to see that this window is not our previous window
            if (iPrevActiveWindow>=0)
            {
              GUIWindow prevWindow=(GUIWindow)m_vecWindows[iPrevActiveWindow];
              if (prevWindow.PreviousWindowID == iWindowID)
              {	// we are going to the lsat window - don't update it's previous window id
                msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,(int)GUIWindow.Window.WINDOW_INVALID,0,null);
                pWindow.OnMessage(msg);
              }
              else
              {	
                // we are going to a new window - put our current window into it's previous window ID
                if (null!=prevWindow)
                {
                  msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,prevWindow.GetID,0,null);
                  pWindow.OnMessage(msg);
                }
                else
                {
                  msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,(int)GUIWindow.Window.WINDOW_INVALID,0,null);
                  pWindow.OnMessage(msg);
                }
              }
            }
            else
            {
              msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,(int)GUIWindow.Window.WINDOW_INVALID,0,null);
              pWindow.OnMessage(msg);
            }
            return;
          }
        }

        // new window doesnt exists. (maybe .xml file is invalid or doesnt exists)
        // so we go back to the previous window
        m_iActiveWindow=iPrevActiveWindow;
        if (m_iActiveWindow<0 || m_iActiveWindow>m_vecWindows.Count)
          m_iActiveWindow=0;
        pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
        m_iActiveWindowID=pWindow.GetID;
        msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,(int)GUIWindow.Window.WINDOW_INVALID,0,null);
        pWindow.OnMessage(msg);		
      }
      finally
      {
        m_bSwitching=false;
      }
    }

    /// <summary>
    /// Show previous window. When user goes back (ESC)
    /// this function will show the previous active window
    /// </summary>
    static public void PreviousWindow()
    {
      m_bSwitching=true;
      try
      {
        for (int x=0; x < m_vecWindows.Count;++x)
        {
          if (((GUIWindow)m_vecWindows[x]).DoesPostRender() )
          {
            ((GUIWindow)m_vecWindows[x]).Focused=false;
          }
        }

        // deactivate any window
        GUIMessage msg;
        GUIWindow pWindow;
        int iPrevActiveWindow=m_iActiveWindow;
        int iPrevActiveWindowID=0;
        if (m_iActiveWindow >=0)
        {
      
          if (m_iActiveWindow >=0 && m_iActiveWindow < m_vecWindows.Count)
          {
            pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
            iPrevActiveWindowID = pWindow.PreviousWindowID;
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,pWindow.GetID,0,0,iPrevActiveWindowID,0,null);
            pWindow.OnMessage(msg);
            m_iActiveWindow=-1;
            m_iActiveWindowID=-1;
          }
        }

        // activate the new window
        for (int i=0; i < m_vecWindows.Count; i++)
        {
          pWindow=(GUIWindow)m_vecWindows[i];
          if (pWindow.GetID == iPrevActiveWindowID) 
          {
            m_iActiveWindow=i;
            m_iActiveWindowID=pWindow.GetID;
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,(int)GUIWindow.Window.WINDOW_INVALID,0,null);
            pWindow.OnMessage(msg);
            return;
          }
        }

        // previous window doesnt exists. (maybe .xml file is invalid or doesnt exists)
        // so we go back to the previous window
        m_iActiveWindow=0;
        pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
        m_iActiveWindowID=pWindow.GetID;
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,(int)GUIWindow.Window.WINDOW_INVALID,0,null);
        pWindow.OnMessage(msg);
      }
      finally
      {
        m_bSwitching=false;
      }
    }


    /// <summary>
    /// Called when a new action has been arrived for the window
    /// The window manager will give the action to the current active window 2 handle
    /// </summary>
    /// <param name="action">new action for current active window</param>
    static public void OnAction(Action action)
    {
      if( action.wID == Action.ActionType.ACTION_MOVE_LEFT ||
          action.wID == Action.ActionType.ACTION_MOVE_RIGHT ||
          action.wID == Action.ActionType.ACTION_MOVE_UP ||
          action.wID == Action.ActionType.ACTION_MOVE_DOWN ||
          action.wID == Action.ActionType.ACTION_SELECT_ITEM)
      {
        for (int x=0; x < m_vecWindows.Count;++x)
        {
          if (((GUIWindow)m_vecWindows[x]).DoesPostRender() && ((GUIWindow)m_vecWindows[x]).Focused)
          {
            int iActiveWindow=ActiveWindow;
            ((GUIWindow)m_vecWindows[x]).OnAction(action);
            if (((GUIWindow)m_vecWindows[x]).Focused || iActiveWindow!=ActiveWindow) return;
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
      {
        for (int x=0; x < m_vecWindows.Count;++x)
        {
          if (((GUIWindow)m_vecWindows[x]).DoesPostRender() )
          {
            ((GUIWindow)m_vecWindows[x]).OnAction(action);
          }
        }
      }

      // if a dialog is onscreen then route the action to the dialog
			if (null!=m_pRouteWindow)
			{
        if (action.wID != Action.ActionType.ACTION_KEY_PRESSED &&
            action.wID != Action.ActionType.ACTION_MOUSE_CLICK)
        {
          Action newaction=new Action();
          if (ActionTranslator.GetAction(m_pRouteWindow.GetID,action.m_key,ref newaction))
          {
            m_pRouteWindow.OnAction(newaction);
            return;
          }
        }
        m_pRouteWindow.OnAction(action);
				return;
			}

      // else send it to the current active window
			if (m_iActiveWindow < 0 || m_iActiveWindow >= m_vecWindows.Count) return;
			GUIWindow pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
      if (null!=pWindow) 
      {
        pWindow.OnAction(action);

        if (action.wID == Action.ActionType.ACTION_MOVE_UP)
        {
          if (pWindow.GetFocusControlId()<0)
          {
            for (int x=0; x < m_vecWindows.Count;++x)
            {
              if (((GUIWindow)m_vecWindows[x]).DoesPostRender() )
              {
                ((GUIWindow)m_vecWindows[x]).Focused=true;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// PostRender() gives the windows the oppertunity to overlay itself ontop of
    /// the other window(s)
    /// It gets called at the end of every rendering cycle 
    /// 
    /// this function will call the PostRender() of every window
    /// </summary>
    static void PostRender()
    {
      //render overlay layer 1-10
      for (int iLayer=1; iLayer <= 2; iLayer++)
      {
        for (int x=0; x < m_vecWindows.Count;++x)
        {
          if (((GUIWindow)m_vecWindows[x]).DoesPostRender() )
            ((GUIWindow)m_vecWindows[x]).PostRender(iLayer);
        }
      }
      GUIPropertyManager.Changed=false;
    }

    static public void ProcessWindows()
    {
      if (m_iActiveWindow >=0) 
      {
        GUIWindow pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
        if (null!=pWindow) pWindow.Process();
      }
    }


    /// <summary>
    /// Render()
    /// ask the current active window to render itself
    /// </summary>
    static public void Render()
    {
      // if there's a dialog, then render that
			if (null!=m_pRouteWindow)
			{
        m_pRouteWindow.Render();
        // and call postrender
        PostRender();
				return;
			}

      // else render the current active window
      if (m_iActiveWindow >=0) 
      {
        GUIWindow pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
        if (null!=pWindow) pWindow.Render();
      }

      // and call postrender
      PostRender();
    }

    /// <summary>
    /// GetWindow() returns the window with the specified ID
    /// </summary>
    /// <param name="dwID">id of window</param>
    /// <returns>window found or null if not found</returns>
    static public GUIWindow GetWindow(int dwID)
    {
			for (int x=0; x < m_vecWindows.Count;++x)
			{
				if (((GUIWindow)m_vecWindows[x]).GetID==dwID) return (GUIWindow)m_vecWindows[x];
			}
			return null;
    }

    /// <summary>
    /// 
    /// </summary>
    static public void Process()
    {
			if (null!=Callbacks)
			{
				Callbacks();
			}
    }
    /// <summary>
    /// returns true if current window wants to refresh/redraw itself
    /// other wise false
    /// </summary>
    /// <returns>true,false</returns>
    static public bool NeedRefresh()
    {
      if (m_iActiveWindow < 0) return false;
      GUIWindow pWindow=(GUIWindow)m_vecWindows[m_iActiveWindow];
      bool bRefresh=m_bRefresh;
      m_bRefresh=false;
      return (bRefresh|pWindow.NeedRefresh());
    }

    /// <summary>
    /// Restore() will restore all the positions of all controls of all windows
    /// </summary>
    static public void Restore()
    {
      // reload all controls from the xml file
      for (int x=0; x < m_vecWindows.Count;++x)
      {
        ((GUIWindow)m_vecWindows[x]).Restore();
      }
    }

    /// <summary>
    /// Removes all windows 
    /// </summary>
    static public void Clear()
    {
			GUIGraphicsContext.Receivers -= new SendMessageHandler(SendMessage);
      for (int x=0; x < m_vecWindows.Count;++x)
      {
        ((GUIWindow)m_vecWindows[x]).DeInit();
        ((GUIWindow)m_vecWindows[x]).FreeResources();
      }
      m_pRouteWindow=null;
      m_vecWindows.Clear();
      m_vecThreadMessages.Clear();
      GUIWindow.Clear();
    }

    /// <summary>
    /// Asks all windows to cleanup their resources
    /// </summary>
    static public void Dispose()
    {
      for (int x=0; x < m_vecWindows.Count;++x)
      {
        ((GUIWindow)m_vecWindows[x]).FreeResources();
      }
    }

    /// <summary>
    /// Call preinit for every window
    /// This function gets called once by the runtime when everything is up & running
    /// directX is now initialized, but before the first window is activated. 
    /// It gives the window the oppertunity to allocate any (directx) resources
    /// it may need
    /// </summary>
    static public void PreInit()
    {
      for (int x=0; x < m_vecWindows.Count;++x)
      {
        ((GUIWindow)m_vecWindows[x]).PreInit();
      }
    }

    /// <summary>
    /// Tell window manager it should route all actions/messages to 
    /// another window. This is used for dialogs
    /// </summary>
    /// <param name="dwID">id of window which should receive the actions/messages</param>
    static public void RouteToWindow(int dwID)
		{
      m_bRefresh=true;
			m_pRouteWindow=GetWindow(dwID);
    }

    /// <summary>
    /// tell the window manager to unroute the current routing
    /// </summary>
    static public void UnRoute()
		{
			m_pRouteWindow=null;
      m_bRefresh=true;
      GUIPropertyManager.SetProperty("#currentmodule",GUILocalizeStrings.Get(10000+ActiveWindow));
    }

    /// <summary>
    /// send thread message. Same as sendmessage() however message is placed on a queue
    /// which is processed later.
    /// </summary>
    /// <param name="message">new message to send</param>
    static public void SendThreadMessage(GUIMessage message)
    {
        m_vecThreadMessages.Add(message);
    }

    /// <summary>
    /// process the thread messages
    /// </summary>
    static public void DispatchThreadMessages()
    {
      if (m_vecThreadMessages.Count>0)
      {
        ArrayList list=m_vecThreadMessages;
        m_vecThreadMessages=new ArrayList();
        foreach(GUIMessage message in list)
        {
          SendMessage(message);
        }
      }
    }

    /// <summary>
    /// return the ID of the current active window
    /// </summary>
    static public int	ActiveWindow
    {
			get
			{
				if (m_iActiveWindowID < 0) return 0;
				else return m_iActiveWindowID;
			}
    }

    /// <summary>
    /// return whether GUIWindowManager is routing all messages/actions to a dialog or not
    /// </summary>
    static public bool IsRouted
    {
			get
			{
				if (null!=m_pRouteWindow) return true;
				return false;
			}
    }
    /// <summary>
    /// return whether ID of the window which is routed to
    /// </summary>
    static public int RoutedWindow
    {
      get
      {
        if (null!=m_pRouteWindow) return m_pRouteWindow.GetID;
        return -1;
      }
    }

    /// <summary>
    /// return true if initialized else false
    /// </summary>
    static public bool Initalized
    {
      get { return (m_vecWindows.Count>0);}
    }
    
    

    public static bool MyInterfaceFilter(Type typeObj,Object criteriaObj)
    {
      if( typeObj.ToString() .Equals( criteriaObj.ToString()))
        return true;
      else
        return false;
    }


    static public void ShowWarning(int iHeading, int iLine1, int iLine2)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,GUIWindowManager.ActiveWindow,0,0,iHeading,iLine1,null);
      msg.Param3=iLine2;
      GUIWindowManager.SendThreadMessage(msg);
    }
  }
}
