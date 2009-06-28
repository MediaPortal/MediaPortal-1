#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Radio.Database;

namespace MediaPortal.TV.Recording
{
  [PluginIcons("ProcessPlugins.PowerScheduler.PowerScheduler.gif",
    "ProcessPlugins.PowerScheduler.PowerScheduler_disabled.gif")]
  public class TVE2Recorder : IPluginReceiver, ISetupForm
  {
    private const int PBT_APMQUERYSUSPEND = 0x0000;
    private const int PBT_APMQUERYSTANDBY = 0x0001;
    private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
    private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
    private const int PBT_APMSUSPEND = 0x0004;
    private const int PBT_APMSTANDBY = 0x0005;
    private const int PBT_APMRESUMECRITICAL = 0x0006;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    private const int PBT_APMBATTERYLOW = 0x0009;
    private const int PBT_APMPOWERSTATUSCHANGE = 0x000A;
    private const int PBT_APMOEMEVENT = 0x000B;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    private const int WM_POWERBROADCAST = 0x0218;
    private const int BROADCAST_QUERY_DENY = 0x424D5144;
    private const int WM_QUERYENDSESSION = 0x0011;

    private static object syncObj = new object();
    private bool _suspended;
    private bool _onResumeAutomaticRunning;
    private static object syncResumeAutomatic = new object();
    private bool _runAutomaticResume;
    private OnActionHandler _actionHandler;
    private Thread t;

    public TVE2Recorder()
    {
      _actionHandler = OnNewAction;
    }

    private void OnNewAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_EXIT:
          if (Recorder.IsAnyCardRecording())
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
            msg.Param1 = 1033;
            msg.Param2 = 506;
            msg.Param3 = 0;
            GUIWindowManager.SendMessage(msg);
            if (msg.Param1 != 1)
            {
              return;
            }
          }
          break;
        case Action.ActionType.ACTION_REBOOT:
          if (Recorder.IsAnyCardRecording())
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
            msg.Param1 = 1033;
            msg.Param2 = 506;
            msg.Param3 = 0;
            GUIWindowManager.SendMessage(msg);
            if (msg.Param1 != 1)
            {
              return;
            }
          }
          break;
        case Action.ActionType.ACTION_SHUTDOWN:
          if (Recorder.IsAnyCardRecording())
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
            msg.Param1 = 1033;
            msg.Param2 = 506;
            msg.Param3 = 0;
            GUIWindowManager.SendMessage(msg);
            if (msg.Param1 != 1)
            {
              GUIWindow win = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_HOME);
              if (win != null)
              {
                win.OnAction(new Action(Action.ActionType.ACTION_MOVE_LEFT, 0, 0));
              }
              /*GUIOverlayWindow topBar =
                  (GUIOverlayWindow)
                  GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TOPBARHOME);
              if (topBar != null)
              {
                topBar.Focused = true;
              }*/
              return;
            }
          }
          break;
        case Action.ActionType.ACTION_STOP:
          if (Recorder.IsRadio())
          {
            Recorder.StopRadio();
          }
          break;
      }
    }
    #region <Interface> Implementations


    #region IPluginReceiver Interface

    /// <summary>
    /// This method will be called by mediaportal to start your process plugin
    /// </summary>
    public void Start()
    {
      t = new Thread(Process);
      t.Priority = ThreadPriority.BelowNormal;
      t.IsBackground = true;
      t.Start();

      GUIWindowManager.OnNewAction += _actionHandler;
      Recorder.Start();
      using (Settings xmlreader = new MPSettings())
      {
        string strDefault = xmlreader.GetValueAsString("myradio", "default", "");
        if (strDefault != "")
        {
          RadioStation station;
          RadioDatabase.GetStation(strDefault, out station);
          GUIMessage msg;
          if (station.URL != null && !station.URL.Equals(string.Empty) && station.Frequency == 0)
          {
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
            msg.Label = station.URL;
          } else
          {
            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO,
                                 (int)GUIWindow.Window.WINDOW_RADIO, 0, 0, 0, 0, null);
            msg.Label = strDefault;
          }
          GUIGraphicsContext.SendMessage(msg);
        }
      }
    }

    /// <summary>
    /// This method will be called by mediaportal to stop your process plugin
    /// </summary>
    public void Stop()
    {
      if (Recorder.IsAnyCardRecording())
      {
        Recorder.StopRecording();
      }
      Recorder.Stop();
      GUIWindowManager.OnNewAction -= _actionHandler;
      t.Abort();
    }

    public void Process()
    {
      try
      {
        while (true)
        {
          Recorder.Process();
          if (!Recorder.View)
          {
            GUIGraphicsContext.IsFullScreenVideo = false;
          }
          Thread.Sleep(250);
        }
      }catch
      {
        Log.Debug("Process Thread of TVE2-Recorder");
      }
    }

    /// <summary>
    /// This method will be called by mediaportal to send system messages to your process plugin,
    /// if the plugin implements WndProc (optional) / added by mPod
    /// </summary>
    public bool WndProc(ref Message msg)
    {
      if (msg.Msg == WM_POWERBROADCAST)
      {
        Log.Info("Main: WM_POWERBROADCAST: {0}", msg.WParam.ToInt32());
        switch (msg.WParam.ToInt32())
        {
          //The PBT_APMQUERYSUSPEND message is sent to request permission to suspend the computer.
          //An application that grants permission should carry out preparations for the suspension before returning.
          //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSUSPEND:
            if (!OnQuerySuspend(ref msg))
            {
              return false;
            }
            break;

          //The PBT_APMQUERYSTANDBY message is sent to request permission to suspend the computer.
          //An application that grants permission should carry out preparations for the suspension before returning.
          //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSTANDBY:
            // Stop all media before suspending or hibernating
            if (!OnQuerySuspend(ref msg))
            {
              return false;
            }
            break;
          case PBT_APMSTANDBY:
            if (!OnQuerySuspend(ref msg))
            {
              return false;
            }
            _runAutomaticResume = true;
            _suspended = true;
            Recorder.Stop();
            break;

          case PBT_APMSUSPEND:
            if (!OnQuerySuspend(ref msg))
            {
              return false;
            }
            _runAutomaticResume = true;
            _suspended = true;
            Recorder.Stop();
            break;
          //The PBT_APMRESUMESUSPEND event is broadcast as a notification that the system has resumed operation after being suspended.
          case PBT_APMRESUMESUSPEND:
            _suspended = false;
            break;

          //The PBT_APMRESUMESTANDBY event is broadcast as a notification that the system has resumed operation after being standby.
          case PBT_APMRESUMESTANDBY:
            _suspended = false;
            break;
          //The PBT_APMRESUMEAUTOMATIC event is broadcast when the computer wakes up automatically to
          //handle an event. An application will not generally respond unless it is handling the event, because the user is not present.
          case PBT_APMRESUMEAUTOMATIC:
            Log.Info("Main: Windows has resumed from standby or hibernate mode to handle a requested event");
            OnResumeAutomatic();
            break;
        }
      } else if (msg.Msg == WM_QUERYENDSESSION)
      {
        Log.Info("Main: Windows is requesting shutdown mode");
        if (!OnQueryShutDown(ref msg))
        {
          Log.Info("Main: shutdown mode denied");
          return false;
        }
        Log.Info("Main: shutdown mode granted");
        msg.Result = (IntPtr)1; //tell windows we are ready to shutdown     
        return true;
      }

      return false; // false = all other processes will handle the msg
    }

    //called when windows asks permission to restart, shutdown or logout
    private static bool OnQueryShutDown(ref Message msg)
    {
      lock (syncObj)
      {
        if (Recorder.IsAnyCardRecording()) // if we are recording then deny request
        {
          msg.Result = new IntPtr(BROADCAST_QUERY_DENY);
          Log.Info("Main: TVRecording running -> Shutdown stopped");
          return false;
        }
        return true;
      }
    }

    //called when windows asks permission to hibernate or standby
    private bool OnQuerySuspend(ref Message msg)
    {
      lock (syncObj)
      {
        if (_suspended)
        {
          return true;
        }
        if (Recorder.IsAnyCardRecording()) // if we are recording then deny request
        {
          msg.Result = new IntPtr(BROADCAST_QUERY_DENY);
          Log.Info("Main: TVRecording running -> Suspend stopped");
          return false;
        }
        return true;
      }
    }

    private void OnResumeAutomatic()
    {
      if (_onResumeAutomaticRunning)
      {
        Log.Info("Main: OnResumeAutomatic - already running -> return without further action");
        return;
      }
      Log.Debug("Main: OnResumeAutomatic - set lock for syncronous inits");
      lock (syncResumeAutomatic)
      {
        if (!_runAutomaticResume)
        {
          Log.Info("Main: OnResumeAutomatic - OnResume called but !_suspended");
          return;
        }

        _onResumeAutomaticRunning = true;
        Recorder.Stop();
        if (!Recorder.Running)
        {
          Log.Info("Main: OnResumeAutomatic - Starting recorder");
          Recorder.Start();
        }

        _onResumeAutomaticRunning = false;
        _runAutomaticResume = false;
        Log.Info("Main: OnResumeAutomatic - Done");
      }
    }

    #endregion

    #region ISetupForm Interface

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "TVE2-Recorder";
    }

    public bool HasSetup()
    {
      return false;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return 6040;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = "TVE2-Recorder";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public string Author()
    {
      return "Team Mediaportal";
    }

    public string Description()
    {
      return "Core plugin for TVE2 recorder";
    }

    public void ShowPlugin() // show the setup dialog
    {
    }

    #endregion

    #endregion
  }
}