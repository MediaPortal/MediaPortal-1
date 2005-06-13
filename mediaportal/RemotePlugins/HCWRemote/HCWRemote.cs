using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.TV.Recording;

namespace MediaPortal
{
  /// <summary>
  /// Hauppauge HCW remote control class / by mPod
  /// 34 and 45 buttons are supported
  /// </summary>
  public class HCWRemote
  {
    /// <summary>
    /// 34-button remote codes
    /// </summary>
    enum PVR_1
    {
      BTN_0      = 1000,
      BTN_1      = 1001,
      BTN_2      = 1002,
      BTN_3      = 1003,
      BTN_4      = 1004,
      BTN_5      = 1005,
      BTN_6      = 1006,
      BTN_7      = 1007,
      BTN_8      = 1008,
      BTN_9      = 1009,
      GREEN      = 1046,
      YELLOW     = 1056,
      RED        = 1011,
      BLUE       = 1041,
      FUNC       = 1012,
      MENU       = 1013,
      MUTE       = 1015,
      VOLUP      = 1016,
      VOLDOWN    = 1017,
      CHNLUP     = 1032,
      CHNLDOWN   = 1033,
      GRNPOWER   = 1061,
      BACK       = 1031,
      OK         = 1037,
      GO         = 1059,
      FULLSCREEN = 1060,
      REC        = 1055,
      STOP       = 1054,
      PAUSE      = 1048,
      PLAY       = 1053,
      REWIND     = 1050,
      FASTFWD    = 1052,
      SKIPFWD    = 1030,
      SKIPREV    = 1036
    }

    /// <summary>
    /// 45-button remote codes
    /// </summary>
    enum PVR_2
    {
      BTN_0      = 2000,
      BTN_1      = 2001,
      BTN_2      = 2002,
      BTN_3      = 2003,
      BTN_4      = 2004,
      BTN_5      = 2005,
      BTN_6      = 2006,
      BTN_7      = 2007,
      BTN_8      = 2008,
      BTN_9      = 2009,
      GREEN      = 2046,
      YELLOW     = 2056,
      RED        = 2011,
      BLUE       = 2041,
      MENU       = 2013,
      MUTE       = 2015,
      VOLUP      = 2016,
      VOLDOWN    = 2017,
      CHNLUP     = 2032,
      CHNLDOWN   = 2033,
      GRNPOWER   = 2061,
      BACK       = 2031,
      OK         = 2037,
      GO         = 2059,
      REC        = 2055,
      STOP       = 2054,
      PAUSE      = 2048,
      PLAY       = 2053,
      REWIND     = 2050,
      FASTFWD    = 2052,
      SKIPFWD    = 2030,
      SKIPREV    = 2036,
      RADIO      = 2012,
      TVNEW      = 2028,
      VIDEOS     = 2024,
      MUSIC      = 2025,
      PICTURES   = 2026,
      GUIDE      = 2027,
      NAVLEFT    = 2022,
      NAVRIGHT   = 2023,
      NAVUP      = 2020,
      NAVDOWN    = 2021,
      TEXT       = 2010,
      SUBCC      = 2014,
      CHNLPREV   = 2018
    }

    /// <summary>
    /// Power-down actions
    /// </summary>
    enum PowerButtonAction
    {
      DoNothing,
      ShutdownMP,
      ShutdownWin,
      Standby,
      Hibernate
    }

    bool controlEnabled;            // HCW remote enabled
    bool allowExternal;             // External processes are controlled by the Hauppauge app
    bool keepControl;               // Keep control, if MP loses focus
    bool logVerbose;                // Verbose logging
    int repeatDelay;                // Repeat delay
    int powerButton;                // Shutdown action
    bool restartIRApp     = false;  // Restart Haupp. IR-app. after MP quit
    IntPtr handlerIR;               // Window handler
    bool appActive        = true;   // Focus
    DateTime lastTime;              // Timestamp of last execution
    int lastCommand;                // Last executed command
    bool enableCursorMode = true;   // Cursor Mode for 34 Button Remotes

    const int IR_NOKEY               = 0x1FFF;  // No key received
    const int HCWPVR2                = 0x001E;  // 43-Button Remote
    const int HCWPVR                 = 0x001F;  // 34-Button Remote

    const int WM_TIMER               = 0x0113;
    const int WM_ACTIVATEAPP         = 0x001C;
    const int WM_POWERBROADCAST      = 0x0218;
    const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    const int PBT_APMRESUMECRITICAL  = 0x0006;

    /// <summary>
    /// The SetDllDirectory function adds a directory to the search path used to locate DLLs for the application.
    /// http://msdn.microsoft.com/library/en-us/dllproc/base/setdlldirectory.asp
    /// </summary>
    /// <param name="PathName">Pointer to a null-terminated string that specifies the directory to be added to the search path.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    static extern bool SetDllDirectory(
      string PathName);

    /// <summary>

    /// The GetLongPathName function converts the specified path to its long form.
    /// If no long path is found, this function simply returns the specified name.
    /// http://msdn.microsoft.com/library/en-us/fileio/fs/getlongpathname.asp
    /// </summary>
    /// <param name="ShortPath">Pointer to a null-terminated path to be converted.</param>
    /// <param name="LongPath">Pointer to the buffer to receive the long path.</param>
    /// <param name="Buffer">Size of the buffer.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
    static extern uint GetLongPathName(
      string ShortPath,
      [Out] StringBuilder LongPath,
      uint Buffer);

    /// <summary>
    /// Registers window handle with Hauppauge IR driver
    /// </summary>
    /// <param name="WindowHandle"></param>
    /// <param name="Msg"></param>
    /// <param name="Verbose"></param>
    /// <param name="IRPort"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    static extern bool IR_Open(
      IntPtr WindowHandle,
      uint Msg,
      bool Verbose,
      uint IRPort);

    /// <summary>
    /// Gets the received key code (new version, works for PVR-150 as well)
    /// </summary>
    /// <param name="RepeatCount"></param>
    /// <param name="RemoteCode"></param>
    /// <param name="KeyCode"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    static extern bool IR_GetSystemKeyCode(
      ref IntPtr RepeatCount,
      ref IntPtr RemoteCode,
      ref IntPtr KeyCode);

    /// <summary>
    /// Unregisters window handle from Hauppauge IR driver
    /// </summary>
    /// <param name="WindowHandle"></param>
    /// <param name="Msg"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    static extern bool IR_Close(
      IntPtr WindowHandle,
      uint Msg);

    /// <summary>
    /// HCW control enabled
    /// </summary>
    /// <returns>Returns true/false.</returns>
    public bool Enabled()
    {
      return controlEnabled;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public HCWRemote()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
        allowExternal  = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        keepControl    = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        logVerbose     = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        repeatDelay    = xmlreader.GetValueAsInt ("remote", "HCWDelay", 0);
        powerButton    = xmlreader.GetValueAsInt ("remote", "HCWPower", (int)PowerButtonAction.ShutdownMP);
      }
      if (controlEnabled)
      {
        if (allowExternal)
        {
          Utils.OnStartExternal  += new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal  += new Utils.UtilEventHandler(OnStopExternal);
        }
        if (logVerbose) Log.Write("HCW: Repeat-delay: {0}", repeatDelay);

        try
        {
          if (!SetDllDirectory(GetDllPath()))
          {
            Log.Write("HCW: Set DLL path failed!");
          }
        }
        catch (Exception e)
        {
          if (logVerbose) Log.Write("HCW Exception: SetDllDirectory: " + e.Message);
        }
      }
    }

    /// <summary>
    /// Remove all events
    /// </summary>
    public void DeInit()
    {
      if (allowExternal)
      {
        Utils.OnStartExternal -= new Utils.UtilEventHandler(OnStartExternal);
        Utils.OnStopExternal -= new Utils.UtilEventHandler(OnStopExternal);
      }
      StopHCW();
    }

    /// <summary>
    /// External process (e.g. myPrograms) started
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="waitForExit"></param>
    public void OnStartExternal(Process proc, bool waitForExit)
    {
      StopHCW();
    }

    /// <summary>
    /// External process (e.g. myPrograms) stopped
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="waitForExit"></param>
    public void OnStopExternal(Process proc, bool waitForExit)
    {
      Init();
    }

    /// <summary>
    /// Stop IR.exe and initiate HCW start
    /// </summary>
    public void Init()
    {
      try
      {
        if (Process.GetProcessesByName("Ir").Length != 0)
        {
          restartIRApp = true;
          int i = 0;
          while ((Process.GetProcessesByName("Ir").Length != 0) && (i < 15))
          {
            i++;
            if (logVerbose) Log.Write("HCW: Terminating external control: attempt #{0}", i);
            if (Process.GetProcessesByName("Ir").Length != 0)
            {
              Process.Start(GetHCWPath() + "Ir.exe", "/QUIT");
              Thread.Sleep(200);
            }
          }
          if (Process.GetProcessesByName("Ir").Length != 0)
          {
            Log.Write("HCW: External control could not be terminated!");
          }
        }
        StartHCW();
      }
      catch (Exception e)
      {
        Log.Write("HCW: Failed to start driver components! (Not installed?)");
        if (logVerbose) Log.Write("HCW Exception: StartHCW: " + e.Message);
      }
    }

    /// <summary>
    /// Start HCW control
    /// </summary>
    public void StartHCW()
    {
      if (!IR_Close(handlerIR, 0))
      {
        Log.Write("HCW: Internal control could not be terminated!");
      }
      handlerIR = GUIGraphicsContext.ActiveForm;
      if (!IR_Open(handlerIR, 0, false, 0))
      {
        Log.Write("HCW: Enabling internal control failed!");
      }
      else
      {
        if (logVerbose) Log.Write("HCW: Internal control enabled");
      }
    }

    /// <summary>
    /// Stop HCW control
    /// </summary>
    public void StopHCW()
    {
      try
      {
        if (!IR_Close(handlerIR, 0))
        {
          Log.Write("HCW: Internal control could not be terminated!");
        }
        else if ((Process.GetProcessesByName("Ir").Length == 0) && (restartIRApp))
        {
          Thread.Sleep(500);
          if (logVerbose) Log.Write("HCW: Enabling external control");
          Process.Start(GetHCWPath() + "Ir.exe", "/QUIET");
        }
      }
      catch (Exception e)
      {
        if (logVerbose) Log.Write("HCW Exception: StopHCW: " + e.Message);
      }
    }

    #region Helper procedures

    /// <summary>
    /// Converts a short to a long path.
    /// </summary>
    /// <param name="shortName">Short path</param>
    /// <returns>Long path</returns>
    static string LongPathName(string shortName)
    {
      StringBuilder longNameBuffer = new StringBuilder(256);
      uint bufferSize = (uint)longNameBuffer.Capacity;
      GetLongPathName(shortName, longNameBuffer, bufferSize);
      return longNameBuffer.ToString();
    }

    /// <summary>
    /// Get the Hauppauge IR components installation path from the windows registry.
    /// </summary>
    /// <returns>Installation path of the Hauppauge IR components</returns>
    public static string GetHCWPath()
    {
      string dllPath = null;
      try
      {
        RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Hauppauge WinTV Infrared Remote");
        dllPath = rkey.GetValue("UninstallString").ToString();
        if (dllPath.IndexOf("UNir32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNir32"));
        else if (dllPath.IndexOf("UNIR32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNIR32"));
      }
      catch (System.NullReferenceException)
      {
        Log.Write("HCW: Could not find registry entries for driver components! (Not installed?)");
      }
      return dllPath;
    }

    /// <summary>
    /// Returns the path of the DLL component
    /// </summary>
    /// <returns>DLL path</returns>
    public static string GetDllPath()
    {
      string dllPath = GetHCWPath();
      if (!File.Exists(dllPath + "irremote.DLL"))
      {
        dllPath = null;
      }
      return dllPath;
    }

    #endregion

    /// <summary>
    /// Evaluate received messages.
    /// </summary>
    /// <param name="msg">Message</param>
    public void WndProc(Message msg)
    {
      switch (msg.Msg)
      {
        case WM_ACTIVATEAPP:
        {
          if ((allowExternal) && (!keepControl))
          {
            if (((int)msg.WParam != 0))
            {
              if (!appActive)
              {
                appActive = true;
                if (logVerbose) Log.Write("HCW: Got focus - internal control enabled");
                Init();
              }
            }
            else if (appActive)
            {
              appActive = false;
              if (logVerbose) Log.Write("HCW: Lost focus - releasing internal control");
              DeInit();
            }
          }
        }
          break;

        case WM_TIMER:
        {
          IntPtr repeatCount = new IntPtr();
          IntPtr remoteCode = new IntPtr();
          IntPtr keyCode = new IntPtr();
          try
          {
            if (IR_GetSystemKeyCode(ref repeatCount, ref remoteCode, ref keyCode))
            {
              if (logVerbose) Log.Write("HCW: Repeat Count: {0}", repeatCount.ToString());
              if (logVerbose) Log.Write("HCW: Remote Code : {0}", remoteCode.ToString());
              if (logVerbose) Log.Write("HCW: Key Code    : {0}", keyCode.ToString());
              int remoteCommand = 0;
              switch ((int) remoteCode)
              {
                case HCWPVR:
                  remoteCommand = ((int)keyCode) + 1000;
                  break;
                case HCWPVR2:
                  remoteCommand = ((int)keyCode) + 2000;
                  break;
              }
              if (repeatDelay != 0)
              {
                if (((lastTime.AddMilliseconds(repeatDelay)) <= DateTime.Now) || (lastCommand != remoteCommand))
                {
                  lastTime = DateTime.Now;
                  lastCommand = remoteCommand;
                  ExecuteCommand(remoteCommand);
                }
              }
              else
                ExecuteCommand(remoteCommand);
            }
          }
          catch (Exception ex)
          {
            if (logVerbose) Log.Write("HCW: Driver exception: {0}", ex.Message);
          }
        }
          break;

        case WM_POWERBROADCAST:
        {
          if (msg.WParam.ToInt32() == PBT_APMRESUMEAUTOMATIC)
            StartHCW();
        }
          break;
      }
    }

    /// <summary>
    /// Execute the command corresponding to the pressed button
    /// </summary>
    /// <param name="remoteCommand">Internal button number (PVR_1/PVR_2)</param>
    void ExecuteCommand(int remoteCommand)
    {
      switch (remoteCommand)
      {
        case (int)PVR_1.RED:  // HCWPVR Button RED
        case (int)PVR_2.RED:  // HCWPVR2 Button RED  
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Teletext Command RED
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_REMOTE_RED_BUTTON,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command TTX RED");
          }
          else
          {
            // Toggle GUI/FS
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command RED: Toggle GUI/FS");
          }
        }
          break;


        case (int)PVR_1.GREEN:  // HCWPVR Button GREEN
        case (int)PVR_2.GREEN:  // HCWPVR2 Button GREEN
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Teletext Command GREEN
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_REMOTE_GREEN_BUTTON,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command TTX GREEN");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            if (GUIGraphicsContext.IsFullScreenVideo)
            {
              // Activate Fullscreen Teletext
              Utils.PlaySound("click.wav", false, true);
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
              if (logVerbose) Log.Write("HCW: [PVR2] Command GREEN: TV -> Activate Fullscreen Teletext");
            }
            else
            {
              // Activate Teletext in Window
              Utils.PlaySound("click.wav", false, true);
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
              if (logVerbose) Log.Write("HCW: [PVR2] Command GREEN: TV -> Activate Teletext in Window");
            }
          }
        }
          break;


        case (int)PVR_1.YELLOW:  // HCWPVR2 Button YELLOW
        case (int)PVR_2.YELLOW:  // HCWPVR2 Button YELLOW
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Teletext Command YELLOW
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_REMOTE_YELLOW_BUTTON,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command TTX YELLOW");
          }
          else if (GUIGraphicsContext.IsFullScreenVideo)
          {
            // FS Video/TV: Toggle OSD
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_SHOW_OSD,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command YELLOW: Toggle OSD");
          }
          else
          {
            // Show Info on selected Item
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_SHOW_INFO,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command YELLOW: Show Info on selected Item");
          }
        }
          break;

        
        case (int)PVR_1.BLUE:  // HCWPVR Button BLUE
        case (int)PVR_2.BLUE:  // HCWPVR2 Button BLUE
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Teletext Command BLUE
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_REMOTE_BLUE_BUTTON,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command TTX BLUE");
          }
          else
          {
            // Toggle Aspect Ratio
            Action action = new Action(Action.ActionType.ACTION_ASPECT_RATIO,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command BLUE: Toggle Aspect Ratio");
          }
        }
          break;


        case (int)PVR_2.TEXT:  // HCWPVR2 Button TEXT
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            if (GUIGraphicsContext.IsFullScreenVideo)
            {
              // Activate Fullscreen Teletext
              Utils.PlaySound("click.wav", false, true);
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
              if (logVerbose) Log.Write("HCW: [PVR2] Command TEXT: TV -> Activate Fullscreen Teletext");
            }
            else
            {
              // Activate Teletext in Window
              Utils.PlaySound("click.wav", false, true);
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
              if (logVerbose) Log.Write("HCW: [PVR2] Command TEXT: TV -> Activate Teletext in Window");
            }
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Activate Fullscreen Teletext
            Utils.PlaySound("click.wav", false, true);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
            if (logVerbose) Log.Write("HCW: [PVR2] Command TEXT: TTX -> Activate Fullscreen Teletext");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT)
          {
            // Activate Teletext in Window
            Utils.PlaySound("click.wav", false, true);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
            if (logVerbose) Log.Write("HCW: [PVR2] Command TEXT: TTX -> Activate Teletext in Window");
          }
        }
          break;


        case (int)PVR_1.FULLSCREEN:  // HCWPVR Button FULLSCREEN
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT)
          {
            // Activate Teletext in Window
            Utils.PlaySound("click.wav", false, true);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command FULLSCREEN: Activate Teletext in Window");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Activate Fullscreen Teletext
            Utils.PlaySound("click.wav", false, true);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command FULLSCREEN: Activate Fullscreen Teletext");
          }
          else
          {
            // Toggle GUI/FS
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command FULLSCREEN: Toggle GUI/FS");
          }
        }
          break;


        case (int)PVR_1.MENU:  // HCWPVR Button MENU
        case (int)PVR_2.MENU:  // HCWPVR2 Button MENU
        {
          // Show Context Menu
          Utils.PlaySound("click.wav", false, true);
          Action action = new Action(Action.ActionType.ACTION_CONTEXT_MENU,0,0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command MENU: Show Context Menu");
        }
          break;


        case (int)PVR_1.MUTE:  // HCWPVR Button MUTE
        case (int)PVR_2.MUTE:  // HCWPVR2 Button MUTE
        {
          Action action = new Action(Action.ActionType.ACTION_VOLUME_MUTE, 0, 0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command MUTE");
        }
          break;


        case (int)PVR_2.VOLUP:  // HCWPVR2 Button VOLUP
        {
          // Increase Audio Volume
          if (!g_Player.Playing)
            Utils.PlaySound("cursor.wav", false, true);
          Action action = new Action(Action.ActionType.ACTION_VOLUME_UP,0,0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR2] Command VOLUP: Increase Audio Volume");
        }
          break;


        case (int)PVR_2.VOLDOWN:  // HCWPVR2 Button VOLDOWN
        {
          // Decrease Audio Volume
          if (!g_Player.Playing)
            Utils.PlaySound("cursor.wav", false, true);
          Action action = new Action(Action.ActionType.ACTION_VOLUME_DOWN,0,0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR2] Command VOLUP: Decrease Audio Volume");
        }
          break;

        
        case (int)PVR_2.CHNLUP:  // HCWPVR2 Button CHNLUP
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Teletext: Previous Subpage
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_REMOTE_SUBPAGE_UP,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command TTX Next Subpage");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom In
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_IN,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLUP: Zoom In");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            // Next Channel
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_NEXT_CHANNEL,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLUP");
          }
          else
          {
            // Page up
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PAGE_UP,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLUP: Page Up");
          }
        }
          break;


        case (int)PVR_2.CHNLDOWN:  // HCWPVR2 Button CHNDOWN
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Teletext: Previous Subpage
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_REMOTE_SUBPAGE_DOWN,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command TTX Previous Subpage");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Out
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_OUT,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLDOWN: Zoom Out");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            // Previous Channel
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PREV_CHANNEL,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLDOWN");
          }
          else
          {
            // Page down
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PAGE_DOWN,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLDOWN: Page Down");
          }
        }
          break;


        case (int)PVR_1.FUNC:  // HCWPVR Button FUNC
        {
          // Toggle Cursor Keys Mode
          enableCursorMode = !enableCursorMode;
          if (enableCursorMode)
          {
            Utils.PlaySound("back.wav", false, true);
            if (logVerbose) Log.Write("HCW: [PVR] Command FUNC: Entering Cursor Keys Mode");
          }
          else if (!enableCursorMode)
          {
            Utils.PlaySound("click.wav", false, true);
            if (logVerbose) Log.Write("HCW: [PVR] Command FUNC: Leaving Cursor Keys Mode");
          }
        }
          break;


        case (int)PVR_1.VOLUP:  // HCWPVR Button VOLUP / NAVRIGHT
        {
          if (enableCursorMode)
          {
            // Right
            SendKeys.SendWait("{RIGHT}");
            if (logVerbose) Log.Write("HCW: [PVR] Command RIGHT");
          }
          else
          {
            // Increase Audio Volume
            if (!g_Player.Playing)
              Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_VOLUME_UP,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR] Command VOLUP: Increase Audio Volume");
          }
        }
          break;


        case (int)PVR_1.VOLDOWN:  // HCWPVR Button VOLDOWN / NAVLEFT
        {
          if (enableCursorMode)
          {
            // Left
            SendKeys.SendWait("{LEFT}");
            if (logVerbose) Log.Write("HCW: [PVR] Command LEFT");
          }
          else
          {
            // Decrease Audio Volume
            if (!g_Player.Playing)
              Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_VOLUME_DOWN,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR] Command VOLUP: Decrease Audio Volume");
          }
        }
          break;


        case (int)PVR_1.CHNLUP:  // HCWPVR Button VOLUP / NAVUP
        {
          if (enableCursorMode)
          {
            // Up
            SendKeys.SendWait("{UP}");
            if (logVerbose) Log.Write("HCW: [PVR] Command UP");
          }
          else
          {
            if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
              GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
            {
              // Teletext: Previous Subpage
              Utils.PlaySound("click.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_REMOTE_SUBPAGE_UP,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command TTX Next Subpage");
            }
            else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
            {
              // Slideshow: Zoom In
              Utils.PlaySound("click.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_ZOOM_IN,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command CHNLUP: Zoom In");
            }
            else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
              GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
            {
              // Next Channel
              Utils.PlaySound("cursor.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_NEXT_CHANNEL,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command CHNLUP");
            }
            else
            {
              // Page up
              Utils.PlaySound("cursor.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_PAGE_UP,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command CHNLUP: Page Up");
            }
          }
        }
          break;


        case (int)PVR_1.CHNLDOWN:  // HCWPVR Button VOLDOWN / NAVDOWN
        {
          if (enableCursorMode)
          {
            // Down
            SendKeys.SendWait("{DOWN}");
            if (logVerbose) Log.Write("HCW: [PVR] Command DOWN");
          }
          else
          {
            if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
              GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
            {
              // Teletext: Previous Subpage
              Utils.PlaySound("click.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_REMOTE_SUBPAGE_DOWN,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command TTX Previous Subpage");
            }
            else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
            {
              // Slideshow: Zoom Out
              Utils.PlaySound("click.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_ZOOM_OUT,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command CHNLDOWN: Zoom Out");
            }
            else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
              GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
            {
              // Previous Channel
              Utils.PlaySound("cursor.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_PREV_CHANNEL,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command CHNLDOWN");
            }
            else
            {
              // Page down
              Utils.PlaySound("cursor.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_PAGE_DOWN,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR] Command CHNLDOWN: Page Down");
            }
          }
        }
          break;


        case (int)PVR_1.GRNPOWER:  // HCWPVR Button GRNPOWER
        case (int)PVR_2.GRNPOWER:  // HCWPVR2 Button GRNPOWER
        {
          if ((powerButton == (int)PowerButtonAction.ShutdownWin) || (powerButton == (int)PowerButtonAction.Standby) || (powerButton == (int)PowerButtonAction.Hibernate))
          {
            Recorder.StopViewing();
            Recorder.Stop();
            g_Player.Stop();
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
            GUIWindowManager.Dispose();
            GUITextureManager.Dispose();
            GUIFontManager.Dispose();
          }

          switch (powerButton)
          {
            case (int)PowerButtonAction.DoNothing:
              if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command GRNPOWER: Do Nothing");
              break;

            case (int)PowerButtonAction.ShutdownWin:
            {
              // Shutdown Windows
              if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command GRNPOWER: Shutdown Windows");
              WindowsController.ExitWindows(RestartOptions.ShutDown, true);
            }
              break;

            case (int)PowerButtonAction.Standby:
            {
              // Standby
              if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command GRNPOWER: Standby");
              WindowsController.ExitWindows(RestartOptions.Suspend, true);
            }
              break;

            case (int)PowerButtonAction.Hibernate:
            {
              // Hibernate
              if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command GRNPOWER: Hibernate");
              WindowsController.ExitWindows(RestartOptions.Hibernate, true);
            }
              break;

            default:
            {
              // Quit Media Portal
              Utils.PlaySound("back.wav", false, true);
              Action action = new Action(Action.ActionType.ACTION_EXIT,0,0);
              GUIGraphicsContext.OnAction(action);
              if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command GRNPOWER: Quit Media Portal");
            }
              break;
          }
        }
          break;


        case (int)PVR_1.BACK:  // HCWPVR Button BACK
        case (int)PVR_2.BACK:  // HCWPVR2 Button BACK
        {
          // Previous Menu/Screen
          SendKeys.SendWait("{ESC}");
          if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command BACK: Previous Menu/Screen");
        }
          break;


        case (int)PVR_1.OK:  // HCWPVR Button OK
        case (int)PVR_2.OK:  // HCWPVR2 Button OK
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_MOVIE_CALIBRATION)
          {
            // Movie-Calibration: Swap Arrows
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_CALIBRATE_SWAP_ARROWS,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command OK: Calibration Swap Arrows");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_UI_CALIBRATION)
          {
            // UI-Calibration: Calibration Finished
            Utils.PlaySound("back.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PREVIOUS_MENU,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command OK: Calibration Finished");
          }
          else
          {
            // Select Item
            SendKeys.SendWait("~");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command OK");
          }
        }
          break;


        case (int)PVR_1.GO:  // HCWPVR Button GO
        case (int)PVR_2.GO:  // HCWPVR2 Button GO
        {
          // Jump to Home Screen
          Utils.PlaySound("back.wav", false, true);
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
          if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command GO/HOME");
        }
          break;


        case (int)PVR_1.REC:  // HCWPVR Button REC
        case (int)PVR_2.REC:  // HCWPVR2 Button REC
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Guide: Select Item / Record
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_SELECT_ITEM,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command REC: Select Item / Record");
          }
          else
          {
            // Record
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_RECORD,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command REC");
          }
        }
          break;


        case (int)PVR_1.STOP:  // HCWPVR Button STOP
        case (int)PVR_2.STOP:  // HCWPVR2 Button STOP
        {
          // Stop
          Utils.PlaySound("click.wav", false, true);
          Action action = new Action(Action.ActionType.ACTION_STOP,0,0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command STOP");
        }
          break;


        case (int)PVR_1.PAUSE:  // HCWPVR Button PAUSE
        case (int)PVR_2.PAUSE:  // HCWPVR2 Button PAUSE
        {
          // Pause
          Utils.PlaySound("click.wav", false, true);
          Action action = new Action(Action.ActionType.ACTION_PAUSE,0,0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command PAUSE");
        }
          break;


        case (int)PVR_1.PLAY:  // HCWPVR Button PLAY
        case (int)PVR_2.PLAY:  // HCWPVR2 Button PLAY
        {
          // Play
          Utils.PlaySound("click.wav", false, true);
          Action action = new Action(Action.ActionType.ACTION_PLAY,0,0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command PLAY");
        }
          break;


        case (int)PVR_1.REWIND:  // HCWPVR Button REWIND
        case (int)PVR_2.REWIND:  // HCWPVR2 Button REWIND
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Guide: Time -
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_DECREASE_TIMEBLOCK,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command REWIND: Time -");
          }
          else
          {
            // Rewind
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_REWIND,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command REWIND");
          }
        }
          break;


        case (int)PVR_1.FASTFWD:  // HCWPVR Button FASTFWD
        case (int)PVR_2.FASTFWD:  // HCWPVR2 Button FASTFWD
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Guide: Time +
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_INCREASE_TIMEBLOCK,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command FASTFWD: Time +");
          }
          else
          {
            // Fast Forward
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_FORWARD,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command FASTFWD");
          }
        }
          break;


        case (int)PVR_1.SKIPFWD:  // HCWPVR Button SKIPFWD
        case (int)PVR_2.SKIPFWD:  // HCWPVR2 Button SKIPFWD
        {
          if ((g_Player.Playing) && (g_Player.IsDVD))
          {
            // DVD: Next Chapter
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_NEXT_CHAPTER,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command SKIPFWD: Next Chapter");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Guide: Day +
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PAGE_UP,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command SKIPFWD: Day +");
          }
          else
          {
            // Next Item
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_NEXT_ITEM,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command SKIPFWD: Next Item");
          }
        }
          break;


        case (int)PVR_1.SKIPREV:  // HCWPVR Button SKIPREV
        case (int)PVR_2.SKIPREV:  // HCWPVR2 Button SKIPREV
        {
          if ((g_Player.Playing) && (g_Player.IsDVD))
          {
            // DVD: Previous Chapter
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PREV_CHAPTER,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command SKIPREV: Previous Chapter");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Guide: Day -
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PAGE_DOWN,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command SKIPREV: Day -");
          }
          else
          {
            // Previous Item
            Utils.PlaySound("cursor.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PREV_ITEM,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command SKIPREV: Previous Item");
          }
        }
          break;


        case (int)PVR_2.RADIO:  // HCWPVR2 Button RADIO
        {
          // Jump to Radio
          Utils.PlaySound("click.wav", false, true);
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_RADIO);
          if (logVerbose) Log.Write("HCW: [PVR2] Command RADIO");
        }
          break;


        case (int)PVR_2.TVNEW:  // HCWPVR2 Button TVNEW
        {
          Utils.PlaySound("click.wav", false, true);
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
            if (logVerbose) Log.Write("HCW: [PVR2] Command TV: TTX Fullscreen -> TV Fullscreen");
          }
            // TV already running?
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TELETEXT)
          {
            // Toggle Fullscreen / GUI
            Action action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command TV: Toggle Fullscreen");
          }
          else
          {
            // Jump to TV
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV);
            if (logVerbose) Log.Write("HCW: [PVR2] Command TV");
          }
        }
          break;


        case (int)PVR_2.VIDEOS:  // HCWPVR2 Button VIDEOS
        {
          Utils.PlaySound("click.wav", false, true);
          // Video already running?
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_VIDEOS ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
          {
            // Toggle Fullscreen / GUI
            Action action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command VIDEOS: Toggle Fullscreen");
          }
          else
          {
            // Jump to Videos
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEOS);
            if (logVerbose) Log.Write("HCW: [PVR2] Command VIDEOS");
          }
        }
          break;


        case (int)PVR_2.MUSIC:  // HCWPVR2 Button MUSIC
        {
          // Jump to Music
          Utils.PlaySound("click.wav", false, true);
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_FILES);
          if (logVerbose) Log.Write("HCW: [PVR2] Command MUSIC");
        }
          break;


        case (int)PVR_2.PICTURES:  // HCWPVR2 Button PICTURES
        {
          // Jump to Pictures
          Utils.PlaySound("click.wav", false, true);
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_PICTURES);
          if (logVerbose) Log.Write("HCW: [PVR2] Command PICTURES");
        }
          break;


        case (int)PVR_2.GUIDE:  // HCWPVR2 Button GUIDE
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Already in Guide?
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PREVIOUS_MENU,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command GUIDE: Back");
          }
          else
          {
            // Jump to Guide
            Utils.PlaySound("click.wav", false, true);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVGUIDE);
            if (logVerbose) Log.Write("HCW: [PVR2] Command GUIDE");
          }
        }
          break;


        case (int)PVR_2.NAVLEFT:  // HCWPVR2 Button NAVLEFT
        {
          // Left
          SendKeys.SendWait("{LEFT}");
          if (logVerbose) Log.Write("HCW: [PVR2] Command LEFT");
        }
          break;


        case (int)PVR_2.NAVRIGHT:  // HCWPVR2 Button NAVRIGHT
        {
          // Right
          SendKeys.SendWait("{RIGHT}");
          if (logVerbose) Log.Write("HCW: [PVR2] Command RIGHT");
        }
          break;


        case (int)PVR_2.NAVUP:  // HCWPVR2 Button NAVUP
        {
          // Up
          SendKeys.SendWait("{UP}");
          if (logVerbose) Log.Write("HCW: [PVR2] Command UP");
        }
          break;


        case (int)PVR_2.NAVDOWN:  // HCWPVR2 Button NAVDOWN
        {
          // Down
          SendKeys.SendWait("{DOWN}");
          if (logVerbose) Log.Write("HCW: [PVR2] Command DOWN");
        }
          break;


        case (int)PVR_2.SUBCC:  // HCWPVR2 Button SUBCC
        {
          // Toggle Aspect Ratio
          Utils.PlaySound("click.wav", false, true);
          Action action = new Action(Action.ActionType.ACTION_ASPECT_RATIO,0,0);
          GUIGraphicsContext.OnAction(action);
          if (logVerbose) Log.Write("HCW: [PVR2] Command SUBCC: Toggle Aspect Ratio");
        }
          break;


        case (int)PVR_2.CHNLPREV:  // HCWPVR2 Button CHNLPREV
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TV ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            Utils.PlaySound("back.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_LAST_VIEWED_CHANNEL,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLPREV: Zap to Last Viewed Channel");
          }
          else if (GUIGraphicsContext.IsFullScreenVideo && g_Player.IsDVD)
          {
            // DVD: Show DVD-Menu
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_DVD_MENU,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLPREV: DVD-Menu");
          }
          else if (GUIGraphicsContext.IsFullScreenVideo && g_Player.Playing)
          {
            // FS: Small Step Back
            Action action = new Action(Action.ActionType.ACTION_SMALL_STEP_BACK,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLPREV: Small Step Back");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Guide: TV Guide Reset
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_TVGUIDE_RESET,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command CHNLPREV: TV Guide Reset");
          }
          else
          {
            // Go to Parent Directory
            Utils.PlaySound("back.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_PARENT_DIR,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR2] Command CHNLPREV: Parent Directory");
          }
        }
          break;


        case (int)PVR_1.BTN_1:  // HCWPVR Button 1
        case (int)PVR_2.BTN_1:  // HCWPVR2 Button 1
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 1 (Normal)
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 1: Zoom Level Normal");
          }
          else
          {
            // GUI: Key 1
            SendKeys.SendWait("1");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 1");
          }
        }
          break;


        case (int)PVR_1.BTN_2:  // HCWPVR Button 2
        case (int)PVR_2.BTN_2:  // HCWPVR2 Button 2
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 2
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_1,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 2: Zoom Level 1");
          }
          else
          {
            // GUI: Key 2
            SendKeys.SendWait("2");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 2");
          }
        }
          break;


        case (int)PVR_1.BTN_3:  // HCWPVR Button 3
        case (int)PVR_2.BTN_3:  // HCWPVR2 Button 3
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 3
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_2,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 3: Zoom Level 2");
          }
          else
          {
            // GUI: Key 3
            SendKeys.SendWait("3");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 3");
          }
        }
          break;


        case (int)PVR_1.BTN_4:  // HCWPVR Button 4
        case (int)PVR_2.BTN_4:  // HCWPVR2 Button 4
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 4
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_3,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 4: Zoom Level 3");
          }
          else
          {
            // GUI: Key 4
            SendKeys.SendWait("4");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 4");
          }
        }
          break;


        case (int)PVR_1.BTN_5:  // HCWPVR Button 5
        case (int)PVR_2.BTN_5:  // HCWPVR2 Button 5
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 5
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_4,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 5: Zoom Level 4");
          }
          else
          {
            // GUI: Key 5
            SendKeys.SendWait("5");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 5");
          }
        }
          break;


        case (int)PVR_1.BTN_6:  // HCWPVR Button 6
        case (int)PVR_2.BTN_6:  // HCWPVR2 Button 6
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 6
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_5,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 6: Zoom Level 5");
          }
          else
          {
            // GUI: Key 6
            SendKeys.SendWait("6");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 6");
          }
        }
          break;


        case (int)PVR_1.BTN_7:  // HCWPVR Button 7
        case (int)PVR_2.BTN_7:  // HCWPVR2 Button 7
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 7
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_6,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 7: Zoom Level 6");
          }
          else
          {
            // GUI: Key 7
            SendKeys.SendWait("7");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 7");
          }
        }
          break;


        case (int)PVR_1.BTN_8:  // HCWPVR Button 8
        case (int)PVR_2.BTN_8:  // HCWPVR2 Button 8
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 8
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_7,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 8: Zoom Level 7");
          }
          else
          {
            // GUI: Key 8
            SendKeys.SendWait("8");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 8");
          }
        }
          break;


        case (int)PVR_1.BTN_9:  // HCWPVR Button 9
        case (int)PVR_2.BTN_9:  // HCWPVR2 Button 9
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
          {
            // Slideshow: Zoom Level 9
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_ZOOM_LEVEL_8,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 9: Zoom Level 8");
          }
          else
          {
            // GUI: Key 9
            SendKeys.SendWait("9");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 9");
          }
        }
          break;


        case (int)PVR_1.BTN_0:  // HCWPVR Button 0
        case (int)PVR_2.BTN_0:  // HCWPVR2 Button 0
        {
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_PICTURES ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_VIDEO_TITLE ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_RECORDEDTV ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL)
          {
            // Slideshow/Pictures/Video Title/Recorded TV/Recorded TV Genre/Recorded TV Channel: Delete Item
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_DELETE_ITEM,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 0: Delete Item");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_UI_CALIBRATION ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_MOVIE_CALIBRATION)
          {
            // Reset UI-/Movie-Calibration
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_CALIBRATE_RESET,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 0: Reset UI-/Movie-Calibration");
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
          {
            // Guide: Default time interval
            Utils.PlaySound("click.wav", false, true);
            Action action = new Action(Action.ActionType.ACTION_DEFAULT_TIMEBLOCK,0,0);
            GUIGraphicsContext.OnAction(action);
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 0: Default time interval");
          }
          else
          {
            // GUI: Key 0
            SendKeys.SendWait("0");
            if (logVerbose) Log.Write("HCW: [PVR/PVR2] Command 0");
          }
        }
          break;
      }
    }
  }
}