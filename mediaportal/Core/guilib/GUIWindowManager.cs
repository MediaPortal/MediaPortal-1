/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Threading;
using System.Collections;
using System.Reflection;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// static class which takes care of window management
	/// Things done are:
	///   - loading & initizling all windows
	///   - routing messages, keypresses, mouse clicks etc to the currently active window
	///   - rendering the currently active window
	///   - methods for switching to the previous window
	///   - methods to switch to another window
	///   
	/// </summary>
  public class GUIWindowManager
  {
		#region delegates and events
		public delegate void OnCallBackHandler();
		public delegate void PostRendererHandler(int level, float timePassed);
		public delegate bool PostRenderActionHandler(Action action, GUIMessage msg, bool focus);
		static public event  SendMessageHandler Receivers;
		static public event  OnActionHandler	  OnNewAction;
		static public event  OnCallBackHandler  Callbacks;
		static public event  PostRenderActionHandler  OnPostRenderAction;
		static public event  PostRendererHandler  OnPostRender;

		#endregion
		#region variables
		static int					 windowCount=0;
    static GUIWindow[]	 m_vecWindows				= new GUIWindow[200];
		static ArrayList		 m_vecThreadMessages	= new ArrayList();
		static ArrayList		 m_vecThreadActions	= new ArrayList();
    static ArrayList		 m_vecWindowList = new ArrayList();
    static int					 m_iActiveWindow=-1;
    static int					 m_iActiveWindowID=-1;
		static int           m_iPrevActiveWindow=-1;
    static int					 m_iPrevActiveWindowID=-1;
    static GUIWindow     m_pRouteWindow=null;
    static bool          m_bRefresh=false;
    static bool          m_bSwitching=false;
		static string        m_strCurrentWindowName = String.Empty;
		#endregion

		#region ctor
    // singleton. Dont allow any instance of this class
    private GUIWindowManager()
    {
    }
		#endregion

		#region messaging
    /// <summary>
    /// Send message to a window/control
    /// </summary>
    /// <param name="message">message to send</param>
    static public void SendMessage(GUIMessage message)
    {
			if (message==null) return;
      if (message.Message==GUIMessage.MessageType.GUI_MSG_LOSTFOCUS||
          message.Message==GUIMessage.MessageType.GUI_MSG_SETFOCUS)
      {
				if (OnPostRenderAction!=null)
				{
					System.Delegate[] delegates=OnPostRenderAction.GetInvocationList();
					for (int i=0; i < delegates.Length;++i)
					{
						if ((bool)delegates[i].DynamicInvoke(new object[] {null,message,false} )) 
							return;
					}
					delegates=null;
				}
      }

			try
			{
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
				if (m_iActiveWindow >= 0 && m_iActiveWindow < windowCount) 
				{
					activewindow=m_vecWindows[m_iActiveWindow];
					if (message.SendToTargetWindow==true)
					{
						for (int i=0; i < windowCount;++i)
						{
							pWindow=m_vecWindows[i];
							if (pWindow.GetID==message.TargetWindowId)
							{
								pWindow.OnMessage(message);
								pWindow=null;
								activewindow=null;
								return;
							}
						}
						activewindow=null;
						return;
					}
				}

				// else send message to the current active window
				if (activewindow!=null)
					activewindow.OnMessage(message);
				activewindow=null;
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"Exception: {0}",ex.ToString());
			}
    }
		/// <summary>
		/// send thread message. Same as sendmessage() however message is placed on a queue
		/// which is processed later.
		/// </summary>
		/// <param name="message">new message to send</param>
		static public void SendThreadMessage(GUIMessage message)
		{
			if (message!=null)
				m_vecThreadMessages.Add(message);
		}

		/// <summary>
		/// process the thread messages and actions
		/// This method gets called by the main thread only and ensures that
		/// all messages & actions are handled by 1 thread only
		/// </summary>
		static public void DispatchThreadMessages()
		{
			if (m_vecThreadMessages.Count>0)
			{
				ArrayList list=m_vecThreadMessages;
				m_vecThreadMessages=new ArrayList();
				for (int i=0; i < list.Count;++i)
				{
					SendMessage(list[i] as GUIMessage);
				}
				list=null;
			}
			if (m_vecThreadActions.Count>0)
			{
				ArrayList list=m_vecThreadActions;
				m_vecThreadActions=new ArrayList();
				for (int i=0; i < list.Count;++i)
				{
					if (OnNewAction!=null) 
					{
						OnNewAction(list[i] as Action);
					}
				}
				list=null;
			}
		}

		/// <summary>
		/// event handler which is called by GUIGraphicsContext when a new action has occured
		/// The method will add the action to a list which is processed later on in the process () function
		/// The reason for this is that multiple threads can add new action and they should only be
		/// processed by the main thread
		/// </summary>
		/// <param name="action">new action</param>
		static void OnActionReceived(Action action)
		{
			if (action!=null)
				m_vecThreadActions.Add(action);
		}

		/// <summary>
		/// This method will handle a given action. Its called by the process() function
		/// The window manager will give the action to the current active window 2 handle
		/// </summary>
		/// <param name="action">new action for current active window</param>
		static public void OnAction(Action action)
		{
			if (action==null) return;
			if (action.wID==Action.ActionType.ACTION_INVALID) return;
			if( action.wID == Action.ActionType.ACTION_MOVE_LEFT ||
				action.wID == Action.ActionType.ACTION_MOVE_RIGHT ||
				action.wID == Action.ActionType.ACTION_MOVE_UP ||
				action.wID == Action.ActionType.ACTION_MOVE_DOWN ||
				action.wID == Action.ActionType.ACTION_SELECT_ITEM)
			{
				if (OnPostRenderAction!=null)
				{
					System.Delegate[] delegates=OnPostRenderAction.GetInvocationList();
					for (int i=0; i < delegates.Length;++i)
					{
						int iActiveWindow=ActiveWindow;
						bool focused=(bool)delegates[i].DynamicInvoke(new object[] {action,null,false} );
						if (focused || iActiveWindow!=ActiveWindow)
							return;
					}
					delegates=null;
					if (m_iActiveWindow >= 0 && m_iActiveWindow < windowCount) 
					{
						GUIWindow pCurrentWindow=m_vecWindows[m_iActiveWindow];
						
						if (pCurrentWindow.GetFocusControlId()<0)
						{	
							GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, pCurrentWindow.GetID, 0, pCurrentWindow.PreviousFocusedId, 0, 0, null);
							pCurrentWindow.OnMessage(msg);
							return;
						}
					}
				}
			}

			if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
			{
				if (OnPostRenderAction!=null)
				{
					OnPostRenderAction(action,null,false);
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
						newaction=null;
						return;
					}
					newaction=null;
				}
				m_pRouteWindow.OnAction(action);
				return;
			}

			// else send it to the current active window
			if (m_iActiveWindow < 0 || m_iActiveWindow >= windowCount) return;
			GUIWindow pWindow=m_vecWindows[m_iActiveWindow];
			if (null!=pWindow) 
			{
				pWindow.OnAction(action);

				if (action.wID == Action.ActionType.ACTION_MOVE_UP)
				{
					if (pWindow.GetFocusControlId()<0)
					{
						bool focused=false;
						System.Delegate[] delegates=OnPostRenderAction.GetInvocationList();
						for (int i=0; i < delegates.Length;++i)
						{
							int iActiveWindow=ActiveWindow;
							focused=(bool)delegates[i].DynamicInvoke(new object[] {action,null,true} );
							if (focused || iActiveWindow!=ActiveWindow)
								break;
						}
						delegates=null;
						if (!focused)
						{	
							GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, pWindow.GetID, 0, pWindow.PreviousFocusedId, 0, 0, null);
							pWindow.OnMessage(msg);
						}
					}
				}
				if (action.wID == Action.ActionType.ACTION_MOVE_DOWN)
				{
					if (pWindow.GetFocusControlId()<0)
					{
						if (OnPostRenderAction!=null)
						{
							bool focused=false;
							System.Delegate[] delegates=OnPostRenderAction.GetInvocationList();
							for (int i=0; i < delegates.Length;++i)
							{
								int iActiveWindow=ActiveWindow;
								focused=(bool)delegates[i].DynamicInvoke(new object[] {action,null,true} );
								if (focused || iActiveWindow!=ActiveWindow)
									break;
							}
							delegates=null;
							if (!focused)
							{	
								GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, pWindow.GetID, 0, pWindow.PreviousFocusedId, 0, 0, null);
								pWindow.OnMessage(msg);
							}
						}
					}
				}
			}
		}
		
		#endregion

		#region window initialisation / deinitialisation
    /// <summary>
    /// Initialize the window manager
    /// </summary>
    static public void Initialize()
    {
      //no active window yet
			m_iActiveWindow=-1;
      m_iActiveWindowID=-1;
      m_bSwitching=false;
      m_vecWindowList.Clear();
      
      //register ourselves for the messages from the GUIGraphicsContext 
			GUIGraphicsContext.Receivers += new SendMessageHandler(SendThreadMessage);
			GUIGraphicsContext.OnNewAction  += new OnActionHandler(OnActionReceived);
    }

    /// <summary>
    /// Add new window to the window manager
    /// </summary>
    /// <param name="Window">new window to add</param>
    static public void Add(ref GUIWindow Window)
    {
			if (Window==null) return;
      for (int i=0; i < windowCount; ++i)
      {
        if (m_vecWindows[i].GetID==Window.GetID)
        {
          Log.WriteFile(Log.LogType.Log,true,"Window:{0} and window {1} have the same id's!!!", Window,m_vecWindows[i]);
          return;
        }
      }
			//Log.Write("Add window :{0} id:{1}", Window.ToString(), Window.GetID);
			m_vecWindows[windowCount]=Window;
			windowCount++;
    }

		/// <summary>
		/// call ResetallControls() for every window
		/// This will cause each control to use the default
		/// position, width & size as mentioned in the skin files
		/// </summary>
		static public void ResetAllControls()
		{
			for (int x=0; x < windowCount;++x)
			{
				m_vecWindows[x].ResetAllControls();
			}
		}
		/// <summary>
		/// Restore() will restore all the positions of all controls of all windows
		/// to their original values as specified in the skin files
		/// </summary>
		static public void Restore()
		{
			// reload all controls from the xml file
			for (int x=0; x < windowCount;++x)
			{
				m_vecWindows[x].Restore();
			}
		}

		/// <summary>
		/// Removes all windows 
		/// </summary>
		static public void Clear()
		{
			GUIGraphicsContext.Receivers -= new SendMessageHandler(SendThreadMessage);
			GUIGraphicsContext.OnNewAction  -= new OnActionHandler(OnActionReceived);
			for (int x=0; x < windowCount;++x)
			{
				m_vecWindows[x].DeInit();
				m_vecWindows[x].FreeResources();
			}
			m_pRouteWindow=null;
			m_vecThreadMessages.Clear();
			m_vecThreadActions.Clear();
			GUIWindow.Clear();
		}

		/// <summary>
		/// Asks all windows to cleanup their resources
		/// </summary>
		static public void Dispose()
		{
			for (int x=0; x < windowCount;++x)
			{
				m_vecWindows[x].FreeResources();
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
			for (int x=0; x < windowCount;++x)
			{
				GUIWindow window=m_vecWindows[x];
				try
				{
					window.PreInit();
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"Exception in {0}.Preinit() {1}",
						window.GetType().ToString(), ex.ToString());
				}
			}
		}

		/// <summary>
		/// Tell window manager it should route all actions/messages to 
		/// another window. This is used for dialogs
		/// </summary>
		/// <param name="dwID">id of window which should receive the actions/messages</param>

		#endregion

		#region DirectX lost/restore device handling
    
		/// <summary>
    /// called by the runtime when DirectX device has been restored
    /// Just let current active window know about this so they can re-allocate their directx resources
    /// </summary>
    static public void OnDeviceRestored()
    {
      for (int x=0; x < windowCount;++x)
      {
        m_vecWindows[x].OnDeviceRestored();
      }
    }

    /// <summary>
    /// Called by the runtime when the DirectX device has been lost
    /// just let all windoww know about this so they can free their directx resources
    /// </summary>
    static public void OnDeviceLost()
    {
      for (int x=0; x < windowCount;++x)
      {
        m_vecWindows[x].OnDeviceLost();
      }
    }

		#endregion

		#region window switching
		/// <summary>
    /// ActivateWindow() 
    /// This function will show/present/activate the window specified
    /// </summary>
    /// <param name="iWindowID">window id of the window to activate</param>
    static public void ReplaceWindow(int iWindowID)
    {
      ActivateWindow(iWindowID, true);
    }
    static public void ActivateWindow(int iWindowID)
    {
      ActivateWindow(iWindowID, false);
    }
    static public void ActivateWindow(int iWindowID, bool bReplaceWindow)
    {
      m_bSwitching=true;
      try
      {
				if (OnPostRenderAction!=null)
				{
					OnPostRenderAction(null,null,false);
				}
				if (m_pRouteWindow!=null)
				{
					GUIMessage msgDlg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,m_pRouteWindow.GetID,0,0,iWindowID,0,null);
					m_pRouteWindow.OnMessage(msgDlg);
					m_pRouteWindow=null;
				}

        GUIMessage msg;
        GUIWindow pWindow;  
				int iActiveWindow=m_iActiveWindow;

        // deactivate current window
        if (m_iActiveWindow >=0 && m_iActiveWindow < windowCount)
        {
          // store current window settings
          // get active window
          pWindow=m_vecWindows[m_iActiveWindow];

          if (!bReplaceWindow)
          {                
            // push active window id to window stack
            m_iActiveWindowID = pWindow.GetID;
            if (iWindowID != m_iActiveWindowID)
            {
              if (m_vecWindowList.Count > 15) m_vecWindowList.RemoveAt(0);
              m_vecWindowList.Add(m_iActiveWindowID);         
              //Log.Write("Window list add Id:{0} new count: {1}", m_iActiveWindowID, m_vecWindowList.Count);
            }
          }

          // deactivate active window
          msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,pWindow.GetID,0,0,iWindowID,0,null);
          pWindow.OnMessage(msg);

					if (!bReplaceWindow)
					{
						m_iPrevActiveWindowID = m_iActiveWindowID;
						m_iPrevActiveWindow = m_iActiveWindow;
					}
          m_iActiveWindow=-1;
          m_iActiveWindowID=-1;
        }        
				UnRoute();

        // activate the new window
        for (int i=0; i < windowCount; i++)
        {
          pWindow=m_vecWindows[i];
          if (pWindow.GetID == iWindowID) 
          {
						try
						{
							m_iActiveWindow=i;
							m_iActiveWindowID=iWindowID;
							msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,m_iPrevActiveWindowID,0,null);
							pWindow.OnMessage(msg);		
							return;
						}
						catch(Exception ex) 
						{
							Log.Write("WindowManager:Unable to initialize window:{0} {1} {2} {3}",
											iWindowID,ex.Message,ex.Source,ex.StackTrace);
							break;
						}
          }
        }

        // new window doesnt exists. (maybe .xml file is invalid or doesnt exists)
        // so we go back to the previous (last active) window

				// Remove the stored (last active) window from the list cause we are going back to that window
				if ( (!bReplaceWindow) && (m_vecWindowList.Count > 0) )
				{
					m_vecWindowList.RemoveAt(m_vecWindowList.Count-1);
				}
				// Get previous window id (previous to the last active window) id
				m_iPrevActiveWindowID=(int)GUIWindow.Window.WINDOW_HOME;
				if (m_vecWindowList.Count > 0)
				{
					m_iPrevActiveWindowID = (int)m_vecWindowList[m_vecWindowList.Count-1];
				}

				m_iActiveWindow=iActiveWindow;
				// Check if replacement window was fault, ifso return to home
				if (bReplaceWindow) 
				{
					// activate HOME window
					m_iActiveWindowID=(int)GUIWindow.Window.WINDOW_HOME;
					for (int i=0; i < windowCount; i++)
					{
						pWindow=m_vecWindows[i];
						if (pWindow.GetID == m_iActiveWindowID) 
						{
							m_iActiveWindow=i;
							break;
						}
					}
				}
				// (re)load
        if (m_iActiveWindow<0 || m_iActiveWindow>=windowCount) m_iActiveWindow=0;
        pWindow=m_vecWindows[m_iActiveWindow];
        m_iActiveWindowID=pWindow.GetID;
        msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,m_iPrevActiveWindowID,0,null);
        pWindow.OnMessage(msg);		
      }
      catch(Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"Exception: {0}",ex.ToString());
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
		static public void ShowPreviousWindow()
		{
			m_bSwitching=true;
			try
			{
				// Exit if there is no previous window
				if (m_vecWindowList.Count == 0) return;

				if (OnPostRenderAction!=null)
				{
					OnPostRenderAction(null,null,false);
				}
				/*
				for (int x=0; x < windowCount;++x)
				{
					if (m_vecWindows[x].DoesPostRender() )
					{
						m_vecWindows[x].Focused=false;
					}
				}*/

				// deactivate any window
				GUIMessage msg;
				GUIWindow pWindow;        				
				int m_iPrevActiveWindowID=(int)GUIWindow.Window.WINDOW_HOME;
				if (m_vecWindowList.Count > 0)
				{
					m_iPrevActiveWindowID = (int)m_vecWindowList[m_vecWindowList.Count-1];
					m_vecWindowList.RemoveAt(m_vecWindowList.Count-1);
					//Log.Write("Window list remove Id:{0} new count: {1}", m_iPrevActiveWindowID, m_vecWindowList.Count);
				}

				if ((m_iActiveWindow >=0 && m_iActiveWindow < windowCount))
				{
					// deactivate current window
					pWindow=m_vecWindows[m_iActiveWindow];

					msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,pWindow.GetID,0,0,m_iPrevActiveWindowID,0,null);
					pWindow.OnMessage(msg);
					m_iActiveWindow=-1;
					m_iActiveWindowID=-1;
				}

				UnRoute();

				// activate the new window
				for (int i=0; i < windowCount; i++)
				{
					pWindow=m_vecWindows[i];
					if (pWindow.GetID == m_iPrevActiveWindowID) 
					{
						try
						{
							m_iPrevActiveWindowID = (int)GUIWindow.Window.WINDOW_INVALID;
							if (m_vecWindowList.Count > 0) m_iPrevActiveWindowID = (int)m_vecWindowList[m_vecWindowList.Count-1];
							m_iActiveWindow=i;
							m_iActiveWindowID=pWindow.GetID;
							msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,m_iPrevActiveWindowID,0,null);
							pWindow.OnMessage(msg);
							return;
						}
						catch(Exception)
						{
							break;
						}
					}
				}

				// previous window doesnt exists. (maybe .xml file is invalid or doesnt exists)
				// so we go back to the first (home) window cause its the only way to get back 
				// to a working window.
				m_iActiveWindow=0;
				m_iActiveWindowID=(int)GUIWindow.Window.WINDOW_HOME;
				for (int i=0; i < windowCount; i++)
				{
					pWindow=m_vecWindows[i];
					if (pWindow.GetID == m_iActiveWindowID) 
					{
						m_iActiveWindow=i;
						break;
					}
				}

				pWindow=m_vecWindows[m_iActiveWindow];
				m_iActiveWindowID=pWindow.GetID;
				msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,pWindow.GetID,0,0,(int)GUIWindow.Window.WINDOW_INVALID,0,null);
				pWindow.OnMessage(msg);
			}
			finally
			{
				m_bSwitching=false;
			}
		}


		#endregion

		#region properties
		/// <summary>
		/// return true if window manager has been initialized 
		/// else false
		/// </summary>
		static public bool Initalized
		{
			get { return (windowCount>0);}
		}
    

		/// <summary>
		/// returns true if we're busy switching from window A->window B
		/// used because we want to prevent rendering during this time
		/// </summary>
		static public bool IsSwitchingToNewWindow
		{
			get { return m_bSwitching;}
			set { m_bSwitching=value;}
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
		/// return the ID of the current active window or dialog
		/// </summary>
		static public int	ActiveWindowEx
		{
			get
			{
				if (IsRouted) return m_pRouteWindow.GetID;
				if (m_iActiveWindowID < 0) return 0;
				else return m_iActiveWindowID;
			}
		}

		/// <summary>
		/// returns true if current window wants to refresh/redraw itself
		/// other wise false
		/// </summary>
		/// <returns>true,false</returns>
		static public bool NeedRefresh()
		{
			if (m_iActiveWindow < 0 || m_iActiveWindow >=windowCount) return false;
			GUIWindow pWindow=m_vecWindows[m_iActiveWindow];
			bool bRefresh=m_bRefresh;
			m_bRefresh=false;
			return (bRefresh|pWindow.NeedRefresh());
		}
		/// <summary>
		/// GetWindow() returns the window with the specified ID
		/// </summary>
		/// <param name="dwID">id of window</param>
		/// <returns>window found or null if not found</returns>
		static public GUIWindow GetWindow(int dwID)
		{
			for (int x=0; x < windowCount;++x)
			{
				if ( m_vecWindows[x].GetID==dwID) return m_vecWindows[x];
			}
			return null;
		}


		#endregion

		#region rendering
		/// <summary>
    /// PostRender() gives the windows the oppertunity to overlay itself ontop of
    /// the other window(s)
    /// It gets called at the end of every rendering cycle 
    /// 
    /// this function will call the PostRender() of every window
    /// Example of windows using it:
    /// - music overlay
    /// - video overlay
    /// - topbar
    /// 
    /// </summary>
    static void PostRender(float timePassed)
    {
      if (GUIGraphicsContext.IsFullScreenVideo && GUIGraphicsContext.ShowBackground) return;
			if (OnPostRender!=null)
			{
				//render overlay layer 1-10
				for (int iLayer=1; iLayer <= 2; iLayer++)
				{
					OnPostRender(iLayer,timePassed);
				}
			}
      GUIPropertyManager.Changed=false;
    }

		/// <summary>
		/// This method will call the process() method on the currently active window
		/// This method gets calle on a regular basis and allows the window todo some stuff
		/// without any user action necessary
		/// </summary>
    static public void ProcessWindows()
    {
			try
			{
				if (m_iActiveWindow >=0) 
				{
					GUIWindow pWindow=m_vecWindows[m_iActiveWindow];
					if (null!=pWindow) 
					{
						pWindow.Process();
						pWindow=null;
					}
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"ProcessWindows exception:{0}", ex.ToString());
			}
    }


    /// <summary>
    /// Render()
    /// ask the current active window to render itself
    /// </summary>
    static public void Render(float timePassed)
    {
      // if there's a dialog, then render that
			if (null!=m_pRouteWindow)
			{
        m_pRouteWindow.Render(timePassed);
        // and call postrender
        PostRender(timePassed);
				return;
			}

      // else render the current active window
      if (m_iActiveWindow >=0 && m_iActiveWindow < windowCount) 
      {
        GUIWindow pWindow=m_vecWindows[m_iActiveWindow];
				if (null!=pWindow) 
				{
					pWindow.Render(timePassed);
					pWindow=null;
				}
      }

      // and call postrender
      PostRender(timePassed);
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

		#endregion

		#region dialog routing
    /// <summary>
    /// Property which returns true when there is a dialog on screen
    /// else false
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
    /// return the ID of the window which is routed to
		/// <returns>-1 when there is no dialog on screen</returns>
		/// <returns>ID of dialog when there is a dialog on screen</returns>
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
		/// tell the window manager to unroute the current routing
		/// </summary>
		static public void UnRoute()
		{			
			if (m_strCurrentWindowName != String.Empty && m_pRouteWindow != null)
				GUIPropertyManager.SetProperty("#currentmodule", m_strCurrentWindowName);
			else
				GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000+ActiveWindow));
			m_pRouteWindow=null;
			m_bRefresh=true;
		}

		static public void RouteToWindow(int dialogId)
		{
			m_bRefresh=true;
			m_pRouteWindow=GetWindow(dialogId);
			m_strCurrentWindowName=GUIPropertyManager.GetProperty("#currentmodule");
		}

    
		#endregion

		#region various
    public static bool MyInterfaceFilter(Type typeObj,Object criteriaObj)
    {
      if( typeObj.ToString() .Equals( criteriaObj.ToString()))
        return true;
      else
        return false;
    }


		/// <summary>
		/// This method will show a warning dialog onscreen
		/// and returns when the user has clicked the dialog away
		/// </summary>
		/// <param name="iHeading">label id for the dialog header</param>
		/// <param name="iLine1">label id for the 1st line in the dialog</param>
		/// <param name="iLine2">label id for the 2nd line in the dialog</param>
    static public void ShowWarning(int iHeading, int iLine1, int iLine2)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,GUIWindowManager.ActiveWindow,0,0,iHeading,iLine1,null);
      msg.Param3=iLine2;
      GUIWindowManager.SendThreadMessage(msg);
    }

		#endregion
  }
}
