#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using MediaPortal.ExtensionMethods;
//using System.Reflection;
//using System.Security;
//using System.Runtime.InteropServices;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// static class which takes care of window management
  /// Things done are:
  ///   - loading and initializing all windows
  ///   - routing messages, key presses, mouse clicks etc to the currently active window
  ///   - rendering the currently active window
  ///   - methods for switching to the previous window
  ///   - methods to switch to another window
  ///   
  /// </summary>
  public class GUIWindowManager
  {
    private static Stopwatch clockWatch = new Stopwatch();

    private static Stopwatch clockWatchMadVr = new Stopwatch();

    #region Frame limiting code

    private static void WaitForFrameClock()
    {
      long milliSecondsLeft;
      long timeElapsed = 0;

      // frame limiting code.
      // sleep as long as there are ticks left for this frame
      clockWatch.Stop();
      timeElapsed = clockWatch.ElapsedTicks;
      if (timeElapsed < GUIGraphicsContext.DesiredFrameTime)
      {
        milliSecondsLeft = (((GUIGraphicsContext.DesiredFrameTime - timeElapsed) * 1000) / Stopwatch.Frequency);
        if (milliSecondsLeft > 0)
        {
          Thread.Sleep((int)milliSecondsLeft);
          //Log.Debug("GUIWindowManager: Wait for desired framerate - sleeping {0} ms.", milliSecondsLeft);
        }
        else
        {
          // Allow to finish other thread context
          Thread.Sleep(1);
          //Log.Debug("GUIWindowManager: Cannot reach desired framerate - please check your system config!");
        }
      }
    }

    private static void StartFrameClock()
    {
      clockWatch.Reset();
      clockWatch.Start();
    }

    internal static void WaitForMadVrFrameClock()
    {
      long milliSecondsLeft;
      long timeElapsed = 0;

      // frame limiting code.
      // sleep as long as there are ticks left for this frame
      clockWatchMadVr.Stop();
      timeElapsed = clockWatchMadVr.ElapsedTicks;
      if (timeElapsed < GUIGraphicsContext.DesiredFrameTime)
      {
        milliSecondsLeft = (((GUIGraphicsContext.DesiredFrameTime - timeElapsed) * 1000) / Stopwatch.Frequency);
        if (milliSecondsLeft > 0)
        {
          Thread.Sleep((int)milliSecondsLeft);
          //Log.Debug("GUIWindowManager: Wait for desired framerate - sleeping {0} ms.", milliSecondsLeft);
        }
        else
        {
          // Allow to finish other thread context
          Thread.Sleep(1);
          //Log.Debug("GUIWindowManager: Cannot reach desired framerate - please check your system config!");
        }
      }
    }

    internal static void StartMadVrFrameClock()
    {
      clockWatchMadVr.Reset();
      clockWatchMadVr.Start();
    }

    #endregion

    public enum FocusState
    {
      NOT_FOCUSED = 0,
      FOCUSED = 1,
      JUST_LOST_FOCUS = 2
    } ;

    #region delegates and events

    public delegate void ThreadMessageHandler(object sender, GUIMessage message);

    public delegate void OnCallBackHandler();

    public delegate void PostRendererHandler(int level, float timePassed);

    public delegate int PostRenderActionHandler(Action action, GUIMessage msg, bool focus);

    public delegate void WindowActivationHandler(int windowId);

    public static event SendMessageHandler Receivers;
    public static event OnActionHandler OnNewAction;
    public static event OnCallBackHandler Callbacks;
    public static event OnCallBackHandler MadVrCallbacks;
    public static event PostRenderActionHandler OnPostRenderAction;
    //public static event  PostRendererHandler  OnPostRender;
    public static event WindowActivationHandler OnActivateWindow;
    public static event WindowActivationHandler OnDeActivateWindow;
    public static event ThreadMessageHandler OnThreadMessageHandler;

    #endregion

    #region variables

    private static int _windowCount = 0;
    //private static GUIWindow[] _listWindows = new GUIWindow[200];
    private static readonly Dictionary<int, GUIWindow> _listWindows = new Dictionary<int, GUIWindow>(200);
    private static List<GUIMessage> _listThreadMessages = new List<GUIMessage>();
    private static readonly object _listThreadMessagesLock = new object();
    private static List<Action> _listThreadActions = new List<Action>();
    private static readonly List<int> _listHistory = new List<int>();
    private static int _activeWindowId = -1;
    private static int _previousActiveWindowId = -1;
    private static GUIWindow _routedWindow = null;
    private static GUIWindow _displayedOsd = null;
    private static bool _shouldRefresh = false;
    private static bool _isSwitchingToNewWindow = false;
    private static string _currentWindowName = string.Empty;
    private static int _nextWindowID = -1;
    private static bool _startWithBasicHome = false;
    private static readonly Object thisLock = new Object(); // used in Route functions
    private static readonly Object thisLockProcess = new Object(); // used to avoid duplicate process

    #endregion

    #region ctor

    // singleton. Dont allow any instance of this class
    private GUIWindowManager() {}

    static GUIWindowManager() {}

    #endregion

    #region messaging

    /// <summary>
    /// Send message to a window/control
    /// </summary>
    /// <param name="message">message to send</param>
    public static void SendMessage(GUIMessage message)
    {
      if (message == null)
      {
        return;
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_CALLBACK)
      {
        CallbackMsg(message);
        return;
      }


      if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
          message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
      {
        if (OnPostRenderAction != null)
        {
          Delegate[] delegates = OnPostRenderAction.GetInvocationList();
          for (int i = 0; i < delegates.Length; ++i)
          {
            if ((FocusState)delegates[i].DynamicInvoke(new object[] {null, message, false}) == FocusState.FOCUSED)
            {
              return;
            }
          }
        }
      }

      try
      {
        // send message to other objects interested
        if (Receivers != null)
        {
          Receivers(message);
        }

        // if dialog is onscreen, then send message to that window
        if (null != _routedWindow)
        {
          if (message.TargetWindowId == _routedWindow.GetID)
          {
            _routedWindow.OnMessage(message);
            return;
          }
        }

        GUIWindow pWindow = null;
        GUIWindow activewindow = null;
        activewindow = GetWindow(ActiveWindow);
        if (message.SendToTargetWindow)
        {
          pWindow = GetWindow(message.TargetWindowId);
          if (pWindow != null && activewindow != null)
          {
            pWindow.OnMessage(message);
            return;
          }
          return;
        }

        // else send message to the current active window
        if (activewindow != null)
        {
          activewindow.OnMessage(message);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Exception: {0}", ex.ToString());
      }
    }

    /// <summary>
    /// send thread message. Same as sendmessage() however message is placed on a queue
    /// which is processed later.
    /// </summary>
    /// <param name="message">new message to send</param>
    public static void SendThreadMessage(GUIMessage message)
    {
      if (OnThreadMessageHandler != null)
      {
        OnThreadMessageHandler(null, message);
      }
      if (message != null)
      {
        lock (_listThreadMessagesLock)
        {
          _listThreadMessages.Add(message);
        }
      }
    }

    public delegate int Callback(int param1, int param2, object data);

    private class CallbackEnv
    {
      public int param1, param2;
      public object data;
      public int result;
      public Callback callback;
      public readonly AutoResetEvent finished = new AutoResetEvent(false);
    }

    private static void CallbackMsg(GUIMessage msg)
    {
      CallbackEnv env = (CallbackEnv)msg.Object;
      env.result = env.callback(env.param1, env.param2, env.data);
      env.finished.Set();
    }

    /// <summary>
    /// This function can be used to call a callback within the context of the message processing thread.
    /// The function waits until the message thread has picked up and executed the callback.
    /// This function is also safe if the current thread is the message processing thread.
    /// </summary>
    /// <param name="callback">Callback to be executed</param>
    /// <param name="param1">Param to callback</param>
    /// <param name="param2">Param to callback</param>
    /// <param name="data">Param to callback</param>
    /// <returns>Return value of callback( param1, param2, data )</returns>
    public static int SendThreadCallbackAndWait(Callback callback, int param1, int param2, object data)
    {
      CallbackEnv env = new CallbackEnv();
      env.callback = callback;
      env.param1 = param1;
      env.param2 = param2;
      env.data = data;

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CALLBACK, 0, 0, 0, 0, 0, env);
      SendThreadMessage(msg);

      // if this is the main thread, then dispatch the messages
      if (Thread.CurrentThread.Name == "MPMain" || Thread.CurrentThread.Name == "Config Main")
      {
        DispatchThreadMessages();
      }

      Log.Debug("SendThreadCallbackAndWait - Waitone");
      env.finished.WaitOne();

      return env.result;
    }

    public static void SendThreadCallback(Callback callback, int param1, int param2, object data)
    {
      CallbackEnv env = new CallbackEnv();
      env.callback = callback;
      env.param1 = param1;
      env.param2 = param2;
      env.data = data;

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CALLBACK, 0, 0, 0, 0, 0, env);
      SendThreadMessage(msg);

      // if this is the main thread, then dispatch the messages
      if (Thread.CurrentThread.Name == "MPMain" || Thread.CurrentThread.Name == "Config Main")
      {
        DispatchThreadMessages();
      }
    }

    public static int SendThreadCallbackSkin(Callback callback, int param1, int param2, object data)
    {
      CallbackEnv env = new CallbackEnv();
      env.callback = callback;
      env.param1 = param1;
      env.param2 = param2;
      env.data = data;

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CALLBACK, 0, 0, 0, 0, 0, env);
      SendThreadMessage(msg);

      // if this is the main thread, then dispatch the messages
      if (Thread.CurrentThread.Name == "MPMain" || Thread.CurrentThread.Name == "Config Main")
      {
        DispatchThreadMessages();
      }

      Log.Debug("SendThreadCallbackAndWait - Waitone");
      env.finished.WaitOne(200);

      return env.result;
    }


    /// <summary>
    /// process the thread messages and actions
    /// This method gets called by the main thread only and ensures that
    /// all messages & actions are handled by 1 thread only
    /// </summary>
    public static void DispatchThreadMessages()
    {
      if (_listThreadMessages.Count > 0)
      {
        List<GUIMessage> list;
        //				System.Diagnostics.Debug.WriteLine("process messages");
        lock (_listThreadMessagesLock) // need lock when switching queues
        {
          list = _listThreadMessages;
          _listThreadMessages = new List<GUIMessage>();
        }
        for (int i = 0; i < list.Count; ++i)
        {
          SendMessage(list[i]);
        }
      }
      if (_listThreadActions.Count > 0)
      {
        //				System.Diagnostics.Debug.WriteLine("process actions");
        List<Action> list;
        lock (_listThreadMessagesLock) // need lock when switching queues
        {
          list = _listThreadActions;
          _listThreadActions = new List<Action>();
        }
        for (int i = 0; i < list.Count; ++i)
        {
          if (OnNewAction != null)
          {
            OnNewAction(list[i]);
          }
        }
      }
    }

    /// <summary>
    /// event handler which is called by GUIGraphicsContext when a new action has occurred
    /// The method will add the action to a list which is processed later on in the process () function
    /// The reason for this is that multiple threads can add new action and they should only be
    /// processed by the main thread
    /// </summary>
    /// <param name="action">new action</param>
    private static void OnActionReceived(Action action)
    {
      if (action != null)
      {
        lock (_listThreadMessagesLock)
        {
          _listThreadActions.Add(action);
        }
      }
    }

    /// <summary>
    /// This method will handle a given action. Its called by the process() function
    /// The window manager will give the action to the current active window 2 handle
    /// </summary>
    /// <param name="action">new action for current active window</param>
    public static void OnAction(Action action)
    {
      bool foundOverlayRecentlyLostFocus = false;
      if (action == null)
      {
        return;
      }
      if (action.wID == Action.ActionType.ACTION_INVALID)
      {
        return;
      }
      if (action.wID == Action.ActionType.ACTION_MOVE_LEFT ||
          action.wID == Action.ActionType.ACTION_MOVE_RIGHT ||
          action.wID == Action.ActionType.ACTION_MOVE_UP ||
          action.wID == Action.ActionType.ACTION_MOVE_DOWN ||
          action.wID == Action.ActionType.ACTION_SELECT_ITEM)
      {
        if (OnPostRenderAction != null)
        {
          Delegate[] delegates = OnPostRenderAction.GetInvocationList();
          for (int i = 0; i < delegates.Length; ++i)
          {
            int iActiveWindow = ActiveWindow;
            FocusState focusState = (FocusState)delegates[i].DynamicInvoke(new object[] {action, null, false});
            if (focusState == FocusState.FOCUSED || iActiveWindow != ActiveWindow)
            {
              return;
            }
            else if (focusState == FocusState.JUST_LOST_FOCUS)
            {
              foundOverlayRecentlyLostFocus = true;
            }
          }
          if (!GUIGraphicsContext.IsFullScreenVideo)
          {
            GUIWindow pCurrentWindow = GetWindow(ActiveWindow);
            if (pCurrentWindow.GetFocusControlId() < 0)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, pCurrentWindow.GetID, 0,
                                              pCurrentWindow.PreviousFocusedId, 0, 0, null);
              pCurrentWindow.OnMessage(msg);
              //return;
            }
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_MOUSE_DOUBLECLICK || action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
      {
        if (OnPostRenderAction != null)
        {
          //OnPostRenderAction(action, null, false);
          Delegate[] delegates = OnPostRenderAction.GetInvocationList();
          for (int i = 0; i < delegates.Length; ++i)
          {
            int iActiveWindow = ActiveWindow;
            FocusState focusState = (FocusState)delegates[i].DynamicInvoke(new object[] { action, null, false });
            if (focusState == FocusState.FOCUSED || iActiveWindow != ActiveWindow)
            {
              return;
            }
          }
        }
      }

      // if a dialog is onscreen then route the action to the dialog
      if (null != _routedWindow)
      {
        if (action.wID != Action.ActionType.ACTION_KEY_PRESSED &&
            action.wID != Action.ActionType.ACTION_MOUSE_CLICK)
        {
          Action newaction = new Action();
          if (ActionTranslator.GetAction(_routedWindow.GetID, action.m_key, ref newaction))
          {
            _routedWindow.OnAction(newaction);
            return;
          }
        }
        _routedWindow.OnAction(action);
        return;
      }

      // else send it to the current active window
      GUIWindow pWindow = GetWindow(ActiveWindow);
      if (null != pWindow)
      {
        if (!foundOverlayRecentlyLostFocus)
          // Don't send it to window if overlay has just lost focus. Correct control already focused!
        {
          pWindow.OnAction(action);
        }

        if (action.wID == Action.ActionType.ACTION_MOVE_UP)
        {
          if (pWindow.GetFocusControlId() < 0)
          {
            FocusState focusState = FocusState.NOT_FOCUSED;
            Delegate[] delegates = OnPostRenderAction.GetInvocationList();
            for (int i = 0; i < delegates.Length; ++i)
            {
              int iActiveWindow = ActiveWindow;
              focusState = (FocusState)delegates[i].DynamicInvoke(new object[] {action, null, true});
              if (focusState == FocusState.FOCUSED || iActiveWindow != ActiveWindow)
              {
                break;
              }
            }
            if (focusState != FocusState.FOCUSED)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, pWindow.GetID, 0,
                                              pWindow.PreviousFocusedId, 0, 0, null);
              pWindow.OnMessage(msg);
            }
          }
        }
        if (action.wID == Action.ActionType.ACTION_MOVE_DOWN)
        {
          if (pWindow.GetFocusControlId() < 0)
          {
            if (OnPostRenderAction != null)
            {
              FocusState focusState = FocusState.NOT_FOCUSED;
              Delegate[] delegates = OnPostRenderAction.GetInvocationList();
              for (int i = 0; i < delegates.Length; ++i)
              {
                int iActiveWindow = ActiveWindow;
                focusState = (FocusState)delegates[i].DynamicInvoke(new object[] {action, null, true});
                if (focusState == FocusState.FOCUSED || iActiveWindow != ActiveWindow)
                {
                  break;
                }
              }
              delegates = null;
              if (focusState != FocusState.FOCUSED)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, pWindow.GetID, 0,
                                                pWindow.PreviousFocusedId, 0, 0, null);
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
    public static void Initialize()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _startWithBasicHome = xmlreader.GetValueAsBool("gui", "startbasichome", true);
      }
      //no active window yet
      _activeWindowId = -1;
      _isSwitchingToNewWindow = false;
      _listHistory.Clear();

      //register ourselves for the messages from the GUIGraphicsContext 
      GUIGraphicsContext.Receivers += new SendMessageHandler(SendThreadMessage);
      GUIGraphicsContext.OnNewAction += new OnActionHandler(OnActionReceived);
    }

    /// <summary>
    /// Add new window to the window manager
    /// </summary>
    /// <param name="Window">new window to add</param>
    public static void Add(ref GUIWindow Window)
    {
      if (Window == null)
      {
        return;
      }
      lock (thisLock)
      {
        if (!_listWindows.ContainsKey(Window.GetID))
        {
          //Log.Info("Add window :{0} id:{1}", Window.ToString(), Window.GetID);
          _listWindows.Add(Window.GetID, Window);
        }
        //else Log.Error("Window:{0} and window {1} have the same id's!!!", Window, _listWindows[i]);
      }
      _windowCount++;
      Window.OnAdded();
    }

    /// <summary>
    /// call ResetallControls() for every window
    /// This will cause each control to use the default
    /// position, width and size as mentioned in the skin files
    /// </summary>
    public static void ResetAllControls()
    {
      LockAndDoOnAllRegisteredWindows(window => window.ResetAllControls());
    }

    /// <summary>
    /// OnResize() will restore all the positions of all controls of all windows
    /// to their original values as specified in the skin files
    /// </summary>
    public static void OnResize()
    {
      GUIWaitCursor.Dispose();
      GUIWaitCursor.Init();

      LockAndDoOnAllRegisteredWindows(window => window.Restore());
    }

    /// <summary>
    /// Removes all windows 
    /// </summary>
    public static void Clear()
    {
      CloseCurrentWindow();
      GUIGraphicsContext.Receivers -= new SendMessageHandler(SendThreadMessage);
      GUIGraphicsContext.OnNewAction -= new OnActionHandler(OnActionReceived);

      LockAndDoOnAllRegisteredWindows(window =>
                                        {
                                          window.DeInit();
                                          window.SafeDispose();
                                        });
      _routedWindow = null;
      _listThreadMessages.Clear();
      _listThreadActions.Clear();
      GUIWindow.Clear();
    }

    /// <summary>
    /// Asks all windows to cleanup their resources
    /// </summary>
    public static void Dispose()
    {
      LockAndDoOnAllRegisteredWindows(window => window.SafeDispose());

      GUIWaitCursor.Dispose();
    }

    /// <summary>
    /// Call preinit for every window
    /// This function gets called once by the runtime when everything is up & running
    /// directX is now initialized, but before the first window is activated. 
    /// It gives the window the oppertunity to allocate any (directx) resources
    /// it may need
    /// </summary>
    public static void PreInit()
    {
      LockAndDoOnAllRegisteredWindows(window =>
                                        {
                                          try
                                          {
                                            window.PreInit();
                                          }
                                          catch (Exception ex)
                                          {
                                            Log.Error("Exception in {0}.Preinit() {1}",
                                                      window.GetType().ToString(), ex.ToString());
                                          }
                                        });

      GUIWaitCursor.Init();
    }

    #endregion

    #region DirectX lost/restore device handling

    /// <summary>
    /// called by the runtime when DirectX device has been restored
    /// Just let current active window know about this so they can re-allocate their directx resources
    /// </summary>
    public static void OnDeviceRestored()
    {
      LockAndDoOnAllRegisteredWindows(window => window.OnDeviceRestored());
    }

    #endregion

    #region window switching

    /// <summary>
    /// ReplaceWindow() 
    /// This function will replace current window with specified window
    /// </summary>
    /// <param name="iWindowID">window id of the window to activate</param>
    public static void ReplaceWindow(int windowId)
    {
      ActivateWindow(windowId, true);
    }

    public static int GetPreviousActiveWindow()
    {
      return _previousActiveWindowId;
    }

    /// <summary>
    /// ActivateWindow() 
    /// This function will show/present/activate the window specified
    /// </summary>
    /// <param name="iWindowID">window id of the window to activate</param>    
    public static void ActivateWindow(int windowId)
    {
      ActivateWindow(windowId, false, false, null, false, -1);
    }

    /// <summary>
    /// ActivateWindow() 
    /// This function will show/present/activate and replace current window
    /// with the window specified
    /// </summary>
    /// <param name="iWindowID">window id of the window to activate</param>   
    /// <param name="bReplaceWindow">replace current window</param>    
    public static void ActivateWindow(int windowId, bool replaceWindow)
    {
      ActivateWindow(windowId, replaceWindow, false, null, false, -1);
    }

    /// <summary>
    /// ActivateWindow() 
    /// This function will show/present/activate and replace current window
    /// with the window specified.
    /// </summary>
    /// <param name="windowId">window id of the window to activate</param>
    /// <param name="replaceWindow">replace current window</param>
    /// <param name="skipAnimation">do not perform open and close animation during this activation</param>
    /// <param name="focusControlId">focus on this control rather than the window default</param>
    public static void ActivateWindow(int windowId, bool replaceWindow, bool skipAnimation, int focusControlId)
    {
      ActivateWindow(windowId, replaceWindow, false, null, skipAnimation, focusControlId);
    }

    /// <summary>
    /// This function will show/present/activate the window specified
    /// and pass a parameter to the window.
    /// </summary>
    /// <param name="windowId">window id of the window to activate</param>
    /// <param name="loadParameter">a parameter string to pass to the new window</param>
    public static void ActivateWindow(int windowId, String loadParameter)
    {
      ActivateWindow(windowId, false, false, loadParameter, false, -1);
    }

    /// <summary>
    /// This function will show/present/activate the window specified
    /// and pass a parameter to the window.
    /// </summary>
    /// <param name="windowId">window id of the window to activate</param>
    /// <param name="loadParameter">a parameter string to pass to the new window</param>
    /// <param name="bReplaceWindow">replace current window</param>  
    public static void ActivateWindow(int windowId, String loadParameter, bool replaceWindow)
    {
      ActivateWindow(windowId, replaceWindow, false, loadParameter, false, -1);
    }

    private static void RemoveDoubleHistory(int newWindow)
    {
      List<int> search = new List<int>();
      List<int> data = new List<int>();

      // use temporary list and DO NOT add also current active window for
      // pattern search data, beacause it is already in _listHistory.
      // reverse list for search
      data.AddRange(_listHistory);
      data.Reverse();
      if (data.Count > 3)
      {
        if (data[0] == data[2] && data[1] == data[3])
        {
          _listHistory[_listHistory.Count - 4] = _listHistory[_listHistory.Count - 2];
          _listHistory[_listHistory.Count - 3] = _listHistory[_listHistory.Count - 1];
          _listHistory.RemoveRange(_listHistory.Count - 2, 2);
        }
      }
    }

    private static void AddNewWindowToHistory(int WindowId)
    {
      if (WindowId > -1)
      {
        if (_listHistory.Count > 15)
        {
          _listHistory.RemoveAt(0);
        }
        _listHistory.Add(WindowId);
        RemoveDoubleHistory(WindowId);
      }
    }

    public static void ResetWindowsHistory()
    {
      _listHistory.Clear();
    }

    private static void ActivateWindow(int newWindowId, bool replaceWindow, bool skipHistory, String loadParameter)
    {
      ActivateWindow(newWindowId, replaceWindow, skipHistory, loadParameter, false, -1);
    }

    private static void ActivateWindow(int newWindowId, bool replaceWindow, bool skipHistory, String loadParameter, bool skipAnimation, int focusControlId)
    {
      _isSwitchingToNewWindow = true;
      try
      {
        if (OnPostRenderAction != null)
        {
          OnPostRenderAction(null, null, false);
        }
        if (_routedWindow != null)
        {
          GUIMessage msgDlg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _routedWindow.GetID, 0, 0,
                                             newWindowId, 0, null);
          _routedWindow.OnMessage(msgDlg);
          _routedWindow = null;
        }

        GUIMessage msg;
        GUIWindow previousWindow = null;
        GUIWindow newWindow = null;
        int previousWindowID = _activeWindowId;


        // set nextwindow id
        _nextWindowID = newWindowId;
        // get active window
        previousWindow = GetWindow(ActiveWindow, false);
        // store current window settings
        if (!replaceWindow)
        {
          // push active window id to window stack
          if (newWindowId != _activeWindowId && !skipHistory)
          {
            AddNewWindowToHistory(_activeWindowId);
          }
        }
        //deactivate previous window
        if (previousWindow != null)
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, previousWindow.GetID, 0, 0, newWindowId, (skipAnimation ? 1 : 0),
                               null);
          previousWindow.OnMessage(msg);
          if (OnDeActivateWindow != null)
          {
            OnDeActivateWindow(previousWindow.GetID);
          }
          UnRoute();
          if (!replaceWindow)
          {
            _previousActiveWindowId = _activeWindowId;
          }
          _activeWindowId = -1;
        }

        #region find new window

        // find the new window
        newWindow = GetWindow(newWindowId, false);

        if (newWindow == null)
        {
          // new window doesnt exists. (maybe .xml file is invalid or doesnt exists)
          // so we go back to the previous (last active) window
          // Remove the stored (last active) window from the list cause we are going back to that window
          if ((!replaceWindow) && (_listHistory.Count > 0))
          {
            _listHistory.RemoveAt(_listHistory.Count - 1);
          }
          // Get previous window id (previous to the last active window) id
          if (_listHistory.Count <= 0)
          {
            if (_startWithBasicHome && File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\basichome.xml")))
            {
              _previousActiveWindowId = (int)GUIWindow.Window.WINDOW_SECOND_HOME;
            }
            else
            {
              _previousActiveWindowId = (int)GUIWindow.Window.WINDOW_HOME;
            }
          }
          newWindowId = _previousActiveWindowId;
          // Check if replacement window was fault, ifso return to home          
          if (replaceWindow)
          {
            // activate HOME window
            if (_startWithBasicHome && File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\basichome.xml")))
            {
              newWindowId = (int)GUIWindow.Window.WINDOW_SECOND_HOME;
            }
            else
            {
              newWindowId = (int)GUIWindow.Window.WINDOW_HOME;
            }
          }
          // (re)load            
          newWindow = GetWindow(newWindowId, false);
        }

        #endregion

        //activate the new window
        if (newWindow != null)
        {
          _activeWindowId = newWindow.GetID;
          if (OnActivateWindow != null)
          {
            OnActivateWindow(_activeWindowId);
          }

            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _activeWindowId, 0, 0, _previousActiveWindowId,
                                 (skipAnimation ? 1 : 0), loadParameter);
            msg.Param3 = focusControlId;
            newWindow.OnMessage(msg);
        }
        else
        {
          Log.Error("Windowmanager: can't load newWindow");
        }
      }
      catch (Exception ex)
      {
        Log.Error("Exception: {0}", ex.ToString());
      }
      finally
      {
        _isSwitchingToNewWindow = false;
        _nextWindowID = -1;
      }
    }

    /// <summary>
    /// Checks if ShowPreviousWindow could activate a previous window. If no, then there is no previous window.
    /// </summary>
    /// <returns></returns>
    public static bool HasPreviousWindow()
    {
      return _listHistory.Count > 0;
    }

    /// <summary>
    /// Show previous window. When user goes back (ESC)
    /// this function will show the previous active window
    /// </summary>
    public static void ShowPreviousWindow()
    {
      Log.Debug("Windowmanager: Goto previous window");
      if (!HasPreviousWindow())
      {
        // if _listhistory count gets corrupted, go home          
        if (_startWithBasicHome && File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\basichome.xml")))
          _listHistory.Add((int)GUIWindow.Window.WINDOW_SECOND_HOME);
        else
          _listHistory.Add((int)GUIWindow.Window.WINDOW_HOME);
      }

      _previousActiveWindowId = (int)_listHistory[_listHistory.Count - 1];
      _listHistory.RemoveAt(_listHistory.Count - 1);

      bool isFullscreen = (
                            _previousActiveWindowId == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                            _previousActiveWindowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC ||
                            _previousActiveWindowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
                            _previousActiveWindowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);

      // do not go back to fullscreen if not playing or nothing to show
      if (isFullscreen && (!Player.g_Player.Playing || (!Player.g_Player.HasVideo && !Player.g_Player.HasViz)))
        ShowPreviousWindow();

      // do not go back to music now playing screen if music is not playing
      if (_previousActiveWindowId == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW && ! (g_Player.Playing && g_Player.IsMusic))
      {
        ShowPreviousWindow();
      }

      if (_previousActiveWindowId != ActiveWindow)
        ActivateWindow(_previousActiveWindowId, false, true, null);
    }

    /// <summary>
    /// Close current window. When MediaPortal closes
    /// we need to close current window
    /// </summary>
    public static void CloseCurrentWindow()
    {
      Log.Debug("Windowmanager: closing current window");
      _isSwitchingToNewWindow = true;
      try
      {
        int fromWindowId = ActiveWindow;

        if (OnPostRenderAction != null)
        {
          OnPostRenderAction(null, null, false);
        }

        // deactivate current window
        GUIWindow pWindow = GetWindow(ActiveWindow, false);

        // deactivate any window
        if (_routedWindow != null)
        {
          GUIMessage msgDlg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _routedWindow.GetID, 0, 0,
                                             ActiveWindow, 0, null);
          _routedWindow.OnMessage(msgDlg);
          _routedWindow = null;
        }

        if (pWindow != null)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, ActiveWindow, 0, 0,
                                          _previousActiveWindowId, 0, null);
          pWindow.OnMessage(msg);
          if (OnDeActivateWindow != null)
          {
            OnDeActivateWindow(pWindow.GetID);
          }
        }
        _activeWindowId = -1;
      }
      finally
      {
        _isSwitchingToNewWindow = false;
      }
    }

    /* TODO: candidate for removal; this method has no callers.
    public static void ActivateSkin(string skinName, int newWindowId)
    {
      _isSwitchingToNewWindow = true;

      try
      {
        GUIMessage msg;
        GUIWindow previousWindow = null;
        GUIWindow newWindow = null;

        // Find the previous (still current) window.
        previousWindow = GetWindow(ActiveWindow);

        // Find the new window.
        newWindow = GetWindow(newWindowId);

        // If the new window was not found then we do not change the skin or the window.
        if (newWindow != null)
        {
          // Deactivate the previous window.
          if (previousWindow != null)
          {
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, previousWindow.GetID, 0, 0, newWindowId,
                                 0, null);
            previousWindow.OnMessage(msg);

            if (OnDeActivateWindow != null)
            {
              OnDeActivateWindow(previousWindow.GetID);
            }

            _activeWindowId = -1;
          }

          // Change the skin and reload the GUI.
          GUIGraphicsContext.Skin = skinName;
          using (Settings xmlwriter = new MPSettings())
          {
            xmlwriter.SetValue("skin", "name", skinName);
            Configuration.Config.SkinName = skinName;
          }

          GUITextureManager.Clear();
          GUITextureManager.Init();
          GUIFontManager.LoadFonts(GUIGraphicsContext.Skin + @"\fonts.xml");
          GUIFontManager.InitializeDeviceObjects();
          GUIExpressionManager.ClearExpressionCache();
          GUIControlFactory.ClearReferences();
          GUIControlFactory.LoadReferences(GUIGraphicsContext.Skin + @"\references.xml");
          GUIWindowManager.OnResize();

          // Activate the new window.
          _activeWindowId = newWindow.GetID;

          if (OnActivateWindow != null)
          {
            OnActivateWindow(newWindow.GetID);
          }

          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, newWindow.GetID, 0, 0,
                               _previousActiveWindowId, 0, null);
          newWindow.OnMessage(msg);
        }

        // Autosize the window if specified.
        using (Settings xmlreader = new MPSettings())
        {
          xmlreader.SetValue("general", "skinobsoletecount", 0);
          bool autosize = xmlreader.GetValueAsBool("gui", "autosize", true);
          if (autosize && !GUIGraphicsContext.Fullscreen)
          {
            try
            {
              System.Windows.Forms.Form.ActiveForm.ClientSize =
                new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
            }
            catch (System.Exception ex)
            {
              Log.Error("Exception: {0}", ex.ToString());
            }
          }
        }

        // Reinitialize the music player visualization window.
        if (BassMusicPlayer.Player != null && BassMusicPlayer.Player.VisualizationWindow != null)
        {
          BassMusicPlayer.Player.VisualizationWindow.Reinit();
        }
      }
      catch (Exception ex)
      {
        Log.Error("Exception: {0}", ex.ToString());
      }
      finally
      {
        _isSwitchingToNewWindow = false;
      }
    }
    */

    #endregion

    #region properties

    /// <summary>
    /// return true if window manager has been initialized 
    /// else false
    /// </summary>
    public static bool Initalized
    {
      get { return (_windowCount > 0); }
    }

    /// <summary>
    /// returns true if we're busy switching from window A->window B
    /// used because we want to prevent rendering during this time
    /// </summary>
    public static bool IsSwitchingToNewWindow
    {
      get { return _isSwitchingToNewWindow; }
      set { _isSwitchingToNewWindow = value; }
    }

    /// <summary>
    /// return the ID of the current active window
    /// </summary>
    public static int ActiveWindow
    {
      get
      {
        if (_activeWindowId < 0)
        {
          return 0;
        }
        return _activeWindowId;
      }
      set { _activeWindowId = value; }
    }

    /// <summary>
    /// return the ID of the current active window or dialog
    /// </summary>
    public static int ActiveWindowEx
    {
      get
      {
        if (IsRouted)
        {
          return _routedWindow.GetID;
        }
        return ActiveWindow;
      }
    }

    /// <summary>
    /// returns true if current window wants to refresh/redraw itself
    /// other wise false
    /// </summary>
    /// <returns>true,false</returns>
    public static bool NeedRefresh()
    {
      GUIWindow pWindow = GetWindow(ActiveWindow);
      bool bRefresh = _shouldRefresh;
      _shouldRefresh = false;
      return (bRefresh | pWindow.NeedRefresh());
    }

    /// <summary>
    /// GetWindow() returns the window with the specified ID
    /// </summary>
    /// <param name="dwID">id of window</param>
    /// <returns>window found or null if not found</returns>
    public static GUIWindow GetWindow(int dwID)
    {
      return GetWindow(dwID, true);
    }

    private static GUIWindow GetWindow(int dwID, bool tryRestoreSkin)
    {
      GUIWindow win;
      if (!_listWindows.TryGetValue(dwID, out win))
      {
        if (dwID != 0)
          Log.Info("GUIWindowManager: Could not find window {0}", dwID);
        // surprese warning if looking for home, the assumption is that we are not fully loaded yet, however some plugins can already load some skinstate which might implicitally call GetWindow for some visibility stuff or somesuch
      }
      else if (tryRestoreSkin && win != null) win.DoRestoreSkin();
      return win;
    }

    #endregion

    #region rendering

    /*
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
      //if (GUIGraphicsContext.IsFullScreenVideo && GUIGraphicsContext.ShowBackground) return;
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
    */

    /// <summary>
    /// This method will call the process() method on the currently active window
    /// This method gets calle on a regular basis and allows the window todo some stuff
    /// without any user action necessary
    /// </summary>
    public static void ProcessWindows()
    {
      try
      {
        if (ActiveWindowEx >= 0)
        {
          GUIWindow pWindow = GetWindow(ActiveWindowEx);
          if (null != pWindow)
          {
            pWindow.Process();
          }
        }
      }
      catch (ThreadStateException ex)
      {
        Log.Error("ProcessWindows thread exception:{0}", ex.ToString());
      }
      catch (Exception ex)
      {
        Log.Error("ProcessWindows exception:{0}", ex.ToString());
      }
    }

    /// <summary>
    /// Render()
    /// ask the current active window to render itself
    /// </summary>
    public static void Render(float timePassed)
    {
      /*
      // if there's a dialog, then render that
			if (null!=_routedWindow)
			{
        _routedWindow.Render(timePassed);
        // and call postrender
        PostRender(timePassed);
				return;
			}*/

      // else render the current active window

      GUIWindow pWindow = GetWindow(ActiveWindow);
      if (null != pWindow)
      {
        pWindow.Render(timePassed);
      }

      GUIGraphicsContext.SetScalingResolution(0, 0, false);
      //pWindow = GetWindow(_nextWindowID);
      //if (null != pWindow)
      //{
      //    pWindow.Render(timePassed);
      //    pWindow = null;
      //}

      // and call postrender
      // PostRender(timePassed);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Process()
    {
      lock (thisLockProcess)
      {
        StartFrameClock();
        if (null != Callbacks)
        {
          Callbacks();
        }
        WaitForFrameClock();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void MadVrProcess()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        StartFrameClock();
        if (null != MadVrCallbacks)
        {
          MadVrCallbacks();
        }
        WaitForFrameClock();
      }
    }

    #endregion

    #region dialog routing

    /// <summary>
    /// Property which returns true when there is a dialog on screen
    /// else false
    /// </summary>
    public static bool IsRouted
    {
      get
      {
        lock (thisLock)
        {
          if (null != _routedWindow)
          {
            return true;
          }
          return false;
        }
      }
    }

    /// <summary>
    /// Tells whether we need Text Input rather than raw keys.
    /// </summary>
    public static bool NeedsTextInput
    {
      get
      {
      // Do we need IsRouted here?
      return GUIWindowManager.IsRouted || 
              GUIWindowManager.ActiveWindowEx == (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD || 
              GUIWindowManager.ActiveWindowEx == (int)GUIWindow.Window.WINDOW_TV_SEARCH;
      }
    }



    /// <summary>
    /// return the ID of the window which is routed to
    /// <returns>-1 when there is no dialog on screen</returns>
    /// <returns>ID of dialog when there is a dialog on screen</returns>
    /// </summary>
    public static int RoutedWindow
    {
      get
      {
        lock (thisLock)
        {
          if (_routedWindow != null)
          {
            return _routedWindow.GetID;
          }
          return -1;
        }
      }
    }

    /// <summary>
    /// Are we displaying an OSD?
    /// </summary>
    public static bool IsOsdVisible
    {
      get { return (_displayedOsd != null); }
      set
      {
        if (!value)
        {
          _displayedOsd = null;
        }
      }
    }

    /// <summary>
    /// Are we displaying pause OSD?
    /// </summary>
    public static bool IsPauseOsdVisible { get; set; }

    /// <summary>
    /// Returns the ID of the current visible OSD
    /// <returns>GUIWindow.Window.WINDOW_INVALID if no OSD is visible</returns>
    /// <returns>GUIWindow.Window when OSD is visible</returns>
    /// </summary>
    public static GUIWindow.Window VisibleOsd
    {
      get
      {
        if (_displayedOsd != null)
        {
          return (GUIWindow.Window)_displayedOsd.GetID;
        }
        else
        {
          return GUIWindow.Window.WINDOW_INVALID;
        }
      }
      set { _displayedOsd = GetWindow((int)value); }
    }

    /// <summary>
    /// tell the window manager to unroute the current routing
    /// </summary>
    public static void UnRoute()
    {
      lock (thisLock)
      {
        GUIWindow currentRoutedWindow = _routedWindow;
        
        if (currentRoutedWindow != null)
        {
          Log.Debug("WindowManager: unroute to {0}:{1}->{2}:{3}",
                    currentRoutedWindow, currentRoutedWindow.GetID, GetWindow(ActiveWindow), ActiveWindow);

          GUIMessage msgDlg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, currentRoutedWindow.GetID, 0, 0,
                                 currentRoutedWindow.PreviousWindowId, 0, null);
          currentRoutedWindow.OnMessage(msgDlg);
        }
        //if (_currentWindowName != string.Empty && _routedWindow != null)
        {
          GUIPropertyManager.SetProperty("#currentmodule", _currentWindowName);
        }

        _routedWindow = null;
        _shouldRefresh = true;
      }
    }

    public static void RouteToWindow(int dialogId)
    {
      lock (thisLock)
      {
        _shouldRefresh = true;
        _routedWindow = GetWindow(dialogId);
        Log.Debug("WindowManager: route {0}:{1}->{2}:{3}",
                  GetWindow(ActiveWindow), ActiveWindow, _routedWindow, dialogId);
        _currentWindowName = GUIPropertyManager.GetProperty("#currentmodule");
      }
    }

    #endregion

    #region various

    public static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return typeObj.ToString().Equals(criteriaObj.ToString());
    }

    /// <summary>
    /// This method will show a warning dialog onscreen
    /// and returns when the user has clicked the dialog away
    /// </summary>
    /// <param name="iHeading">label id for the dialog header</param>
    /// <param name="iLine1">label id for the 1st line in the dialog</param>
    /// <param name="iLine2">label id for the 2nd line in the dialog</param>
    public static void ShowWarning(int iHeading, int iLine1, int iLine2)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, ActiveWindow, 0, 0, iHeading, iLine1,
                                      null);
      msg.Param3 = iLine2;
      SendThreadMessage(msg);
    }

    public static void Replace(int windowId, GUIWindow window)
    {
      lock (thisLock)
      {
        GUIWindow existingWindow = GetWindow(windowId);

        if (existingWindow != null)
        {
          Log.Debug("WindowManager: Replaced {0} with {1}", existingWindow, window);
          ISetupForm frm = window as ISetupForm;
          if (frm != null)
          {
            for (int x = 0; x < PluginManager.SetupForms.Count; ++x)
            {
              if (((ISetupForm)PluginManager.SetupForms[x]).GetWindowId() == windowId)
              {
                Log.Debug("WindowManager: Setup...");
                PluginManager.SetupForms.RemoveAt(x);
                break;
                //PluginManager.SetupForms[x] = frm;
              }
            }
          }
          _listWindows[windowId] = window;
        }
      }
    }

    #endregion

    private static void LockAndDoOnAllRegisteredWindows(Action<GUIWindow> toDo)
    {
      lock (thisLock)
      {
        foreach (var window in _listWindows)
        {
          GUIWindow cw = window.Value;
          toDo(cw);
        }
      }
    }
  }
}