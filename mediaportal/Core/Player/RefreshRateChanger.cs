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

#region using

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using SharpDX.Direct3D9;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion

namespace MediaPortal.Player
{
  // http://forum.team-mediaportal.com/plugins-47/very-simple-refreshrate-changer-39790/
  public sealed class Win32
  {
    public const int CCHDEVICENAME = 32;
    public const int CCHFORMNAME = 32;

    [Flags]
    public enum DISPLAY_DEVICE_StateFlags : uint
    {
      None = 0x00000000,
      DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001,
      DISPLAY_DEVICE_MULTI_DRIVER = 0x00000002,
      DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004,
      DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008,
      DISPLAY_DEVICE_VGA_COMPATIBLE = 0x00000010,
      DISPLAY_DEVICE_REMOVABLE = 0x00000020,
      DISPLAY_DEVICE_MODESPRUNED = 0x08000000,
      DISPLAY_DEVICE_REMOTE = 0x04000000,
      DISPLAY_DEVICE_DISCONNECT = 0x02000000
    }

    [Flags]
    public enum DEVMODE_Fields : uint
    {
      None = 0x00000000,
      DM_POSITION = 0x00000020,
      DM_BITSPERPEL = 0x00040000,
      DM_PELSWIDTH = 0x00080000,
      DM_PELSHEIGHT = 0x00100000,
      DM_DISPLAYFLAGS = 0x00200000,
      DM_DISPLAYFREQUENCY = 0x00400000
    }


    [Flags]
    public enum ChangeDisplaySettings_Flags : uint
    {
      None = 0x00000000,
      CDS_UPDATEREGISTRY = 0x00000001,
      CDS_TEST = 0x00000002,
      CDS_FULLSCREEN = 0x00000004,
      CDS_GLOBAL = 0x00000008,
      CDS_SET_PRIMARY = 0x00000010,
      CDS_VIDEOPARAMETERS = 0x00000020,
      CDS_RESET = 0x40000000,
      CDS_NORESET = 0x10000000
    }


    public enum ChangeDisplaySettings_Result : int
    {
      DISP_CHANGE_SUCCESSFUL = 0,
      DISP_CHANGE_RESTART = 1,
      DISP_CHANGE_FAILED = -1,
      DISP_CHANGE_BADMODE = -2,
      DISP_CHANGE_NOTUPDATED = -3,
      DISP_CHANGE_BADFLAGS = -4,
      DISP_CHANGE_BADPARAM = -5,
      DISP_CHANGE_BADDUALVIEW = -6
    }


    public enum EnumDisplaySettings_EnumMode : uint
    {
      ENUM_CURRENT_SETTINGS = uint.MaxValue,
      ENUM_REGISTRY_SETTINGS = uint.MaxValue - 1
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class DISPLAY_DEVICE
    {
      public uint cb = (uint)Marshal.SizeOf(typeof (DISPLAY_DEVICE));
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName = "";
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString = "";
      public DISPLAY_DEVICE_StateFlags StateFlags = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID = "";
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey = "";
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
      public POINTL(int x, int y)
      {
        this.x = x;
        this.y = y;
      }

      public int x;
      public int y;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class DEVMODE_Display
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)] public string dmDeviceName = null;
      public ushort dmSpecVersion = 0;
      public ushort dmDriverVersion = 0;
      public ushort dmSize = (ushort)Marshal.SizeOf(typeof (DEVMODE_Display));
      public ushort dmDriverExtra = 0;
      public DEVMODE_Fields dmFields = DEVMODE_Fields.None;
      public POINTL dmPosition = new POINTL();
      public uint dmDisplayOrientation = 0;
      public uint dmDisplayFixedOutput = 0;
      public short dmColor = 0;
      public short dmDuplex = 0;
      public short dmYResolution = 0;
      public short dmTTOption = 0;
      public short dmCollate = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)] public string dmFormName = null;
      public ushort dmLogPixels = 0;
      public uint dmBitsPerPel = 0;
      public uint dmPelsWidth = 0;
      public uint dmPelsHeight = 0;
      public uint dmDisplayFlags = 0;
      public uint dmDisplayFrequency = 0;
      public uint dmICMMethod = 0;
      public uint dmICMIntent = 0;
      public uint dmMediaType = 0;
      public uint dmDitherType = 0;
      public uint dmReserved1 = 0;
      public uint dmReserved2 = 0;
      public uint dmPanningWidth = 0;
      public uint dmPanningHeight = 0;
    }


    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int EnumDisplayDevices([In] string lpDevice, [In] uint iDevNum,
                                                [In] [Out] DISPLAY_DEVICE lpDisplayDevice, [In] uint dwFlags);


    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int EnumDisplaySettingsEx([In] string lpszDeviceName,
                                                   [In] EnumDisplaySettings_EnumMode iModeNum,
                                                   [In] [Out] DEVMODE_Display
                                                     lpDevMode, [In] uint dwFlags);


    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern ChangeDisplaySettings_Result
      ChangeDisplaySettingsEx([In] string lpszDeviceName, [In] DEVMODE_Display
                                                            lpDevMode, [In] IntPtr hwnd,
                              [In] ChangeDisplaySettings_Flags dwFlags, [In] IntPtr lParam);


    [DllImport("dwmapi.dll")]
    public static extern int DwmIsCompositionEnabled(ref int pfEnabled);


    /// <summary>
    /// Finds the monitorIndex based on current specified screen on its primary monitor
    /// </summary>
    /// <returns>The monitorIndex that has the specified screen on its primary monitor</returns>
    internal static int FindMonitorIndexForScreen()
    {
      if (GUIGraphicsContext.DX9Device != null && !GUIGraphicsContext.DX9Device.IsDisposed)
      {
        uint deviceNum = 0;
        DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
        displayDevice.cb = (ushort) Marshal.SizeOf(displayDevice);
        while (EnumDisplayDevices(null, deviceNum, displayDevice, 0) != 0)
        {
          if (displayDevice.DeviceName ==
              GUIGraphicsContext.Direct3D.Adapters[GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal].Details.DeviceName)
          {
            // Set new monitorIndex
            GUIGraphicsContext.currentMonitorIdx = (int) deviceNum;
            Log.Debug("CycleRefreshRate: return new detected MonitorIndex : {0}", (int) deviceNum);
            return (int) deviceNum;
          }
          ++deviceNum;
        }
        Log.Debug("CycleRefreshRate: return current default MonitorIndex, detection failed to find the new one : {0}",
          GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal);
        return GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal;
      }
      return -1;
    }

    public static void Win32_SetRefreshRate(uint monitorIndex, uint refreshRate)
    {
      RefreshRateChanger.RefreshRateChangePending = true;
      DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
      displayDevice.cb = (ushort)Marshal.SizeOf(displayDevice);
      DEVMODE_Display devMode = new DEVMODE_Display();
      devMode.dmSize = (ushort)Marshal.SizeOf(devMode);
      ChangeDisplaySettings_Result displayResult = ChangeDisplaySettings_Result.DISP_CHANGE_SUCCESSFUL;

      int result = EnumDisplayDevices(null, monitorIndex, displayDevice, 0);
      if (result != 0)
      {
        if (GUIGraphicsContext.DX9Device != null && !GUIGraphicsContext.DX9Device.IsDisposed)
        {
          Log.Debug("CycleRefreshRate: Current MonitorIndex : {0} and current deviceName : {1}", (int) monitorIndex,
            GUIGraphicsContext.Direct3D.Adapters[GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal].Details.DeviceName);
          if (displayDevice.DeviceName !=
              GUIGraphicsContext.Direct3D.Adapters[GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal].Details.DeviceName)
          {
            // Analyse monitorIndex to be sure to get on the good one (some multiscreen setup can failed otherwise)
            monitorIndex = (uint) FindMonitorIndexForScreen();

            // Try to get new displayDevice based on newest detected monitorIndex
            result = EnumDisplayDevices(null, monitorIndex, displayDevice, 0);
            Log.Debug("CycleRefreshRate: New MonitorIndex : {0} based on current deviceName : {1}", (int) monitorIndex,
              GUIGraphicsContext.Direct3D.Adapters[GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal].Details.DeviceName);
          }
        }

        if (result != 0)
        {
          result = EnumDisplaySettingsEx(displayDevice.DeviceName, 0, devMode, 2);
          if (result != 0)
          {
            result = EnumDisplaySettingsEx(displayDevice.DeviceName, EnumDisplaySettings_EnumMode.ENUM_CURRENT_SETTINGS, devMode, 2); // EDS_RAWMODE = 2

            if (result != 0)
            {
              // Get current Value
              uint Width = devMode.dmPelsWidth;
              uint Height = devMode.dmPelsHeight;

              if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
                  GUIGraphicsContext.ForcedRefreshRate3D)
              {
                // Add a delay for 3D
                if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
                {
                  Log.Debug("RefreshRateChanger.SetRefreshRateBasedOnFPS delayed start when using madVR for 3D");
                  Thread.Sleep(10000);
                }

                // we are here because it's a 3D movie in SBS or TAB and needed to force a 1080p resolution when we are in 4K config
                // Get current Value
                if (GUIGraphicsContext.ForcedRR3DBackDefault)
                {
                  Width = (uint)GUIGraphicsContext.ForcedRR3DWitdhBackup;
                  Height = (uint)GUIGraphicsContext.ForcedRR3DHeightBackup;
                  refreshRate = (uint)GUIGraphicsContext.ForcedRR3DRate;
                  Log.Debug("CycleRefreshRate: restore backup value {0} x {1}", GUIGraphicsContext.ForcedRR3DWitdhBackup, GUIGraphicsContext.ForcedRR3DHeightBackup);
                }
                else
                {
                  GUIGraphicsContext.ForcedRR3DWitdhBackup = GUIGraphicsContext.form.Width;
                  GUIGraphicsContext.ForcedRR3DHeightBackup = GUIGraphicsContext.form.Height;
                  Log.Debug("CycleRefreshRate: backup current value {0} x {1}", GUIGraphicsContext.ForcedRR3DWitdhBackup, GUIGraphicsContext.ForcedRR3DHeightBackup);
                  Width = 1920;
                  Height = 1080;
                  Log.Debug("CycleRefreshRate: 3D used forced value {0} x {1}", Width, Height);
                }
                // needed to resize screen and GUI after resolution change
                GUIGraphicsContext.ForceMadVRRefresh3D = true;
                // needed to avoid multiple refresh rate
                GUIGraphicsContext.ForcedRefreshRate3DDone = true;
              }

              //Log.Info("CycleRefreshRate: code result {0} enum devMode", result);
              devMode.dmFields = (DEVMODE_Fields.DM_BITSPERPEL | DEVMODE_Fields.DM_PELSWIDTH |
                                  DEVMODE_Fields.DM_PELSHEIGHT | DEVMODE_Fields.DM_DISPLAYFREQUENCY);
              devMode.dmBitsPerPel = 32;
              devMode.dmPelsWidth = Width;
              devMode.dmPelsHeight = Height;
              devMode.dmDisplayFrequency = refreshRate;

              // First set settings
              ChangeDisplaySettings_Result r = ChangeDisplaySettingsEx(displayDevice.DeviceName, devMode,
                                                                                   IntPtr.Zero,
                                                                                   (ChangeDisplaySettings_Flags
                                                                                         .CDS_NORESET |
                                                                                    ChangeDisplaySettings_Flags
                                                                                         .CDS_UPDATEREGISTRY),
                                                                                   IntPtr.Zero);              
              if (r != displayResult)
              {
                Log.Info("CycleRefreshRate: unable to change refresh rate {0}Hz for monitor {1} for resolution {2} x {3}", refreshRate, monitorIndex, devMode.dmPelsWidth, devMode.dmPelsHeight);
              }
              else
              {
                // Apply settings
                r = ChangeDisplaySettingsEx(null, null, IntPtr.Zero, 0, IntPtr.Zero);
                Log.Info("CycleRefreshRate: result {0} for refresh rate change {1}Hz for resolution {2} x {3}", r, refreshRate, devMode.dmPelsWidth, devMode.dmPelsHeight);
                FixDwm();
              }
            }
          }
        }
      }
      else
      {
        Log.Info("CycleRefreshRate: unable to change refresh rate {0}Hz for monitor {1}", refreshRate, monitorIndex);
      }
      // Refresh rate done
      RefreshRateChanger.RefreshRateChangePending = false;
    }

    public static void CycleRefreshRate(uint monitorIndex, double refreshRate)
    {
      Win32_SetRefreshRate(monitorIndex, (uint)refreshRate);
    }

    // Fix for Mantis 0002608
    public class SuicideForm : Form
    {
      public SuicideForm()
      {
        RefreshRateChanger.RefreshRateChangeRunning = true;
        Thread.Sleep(500);
        Activated += SuicideFormActivated;
        Opacity = 0;
      }

      protected override void Dispose(bool disposing)
      {
        Activated -= SuicideFormActivated;
        base.Dispose(disposing);
      }

      private void SuicideFormActivated(Object sender, EventArgs e)
      {
        Thread.Sleep(1000);
        Close();
      }
    }


    public static void KillFormThread()
    {
      try
      {
        var suicideForm = new SuicideForm();
        suicideForm.Show();
        suicideForm.Focus();
      }
      catch (Exception ex)
      {
        Log.Error("CycleRefresh: KillFormThread exception {0}", ex);
      }
    }


    public static void FixDwm()
    {
      if (!OSInfo.OSInfo.Win8OrLater())
      {
        try
        {
          int dwmEnabled = 0;
          DwmIsCompositionEnabled(ref dwmEnabled);

          if (dwmEnabled > 0)
          {
            Log.Debug("CycleRefresh: DWM Detected, performing shenanigans");
            ThreadStart starter = KillFormThread;
            var killFormThread = new Thread(starter) {IsBackground = true};
            killFormThread.Start();
          }
        }
        catch (Exception ex)
        {
          Log.Error("CycleRefresh: FixDwm exception {0}", ex);
        }
      }
    }

    public static void FixFCU()
    {
      try
      {
        Log.Debug("CycleRefresh: FixFCU");
        ThreadStart starter = KillFormThread;
        var killFormThread = new Thread(starter) {IsBackground = true};
        killFormThread.Start();
      }
      catch (Exception ex)
      {
        Log.Error("CycleRefresh: FixFCU exception {0}", ex);
      }
    }
  }

  internal class RefreshRateSetting
  {
    #region private vars

    private string _name = null;
    private List<double> _fps = null;
    private double _hz = -1;
    private string _extCmd = null;

    #endregion

    #region contructors

    internal RefreshRateSetting() {}

    #endregion

    #region public properties

    internal string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    internal List<double> Fps
    {
      get { return _fps; }
      set { _fps = value; }
    }

    internal double Hz
    {
      get { return _hz; }
      set { _hz = value; }
    }

    internal string ExtCmd
    {
      get { return _extCmd; }
      set { _extCmd = value; }
    }

    #endregion

    #region public methods

    #endregion
  }

  public static class RefreshRateChanger
  {
    #region public constants

    public const int WAIT_FOR_REFRESHRATE_RESET_MAX = 10; //10 secs

    #endregion

    #region private static vars

    private static double _refreshrateChangeCurrentRR = 0;
    private static string _refreshrateChangeStrFile = "";
    private static MediaType _refreshrateChangeMediaType;
    private static bool _refreshrateChangePending = false;
    private static bool _refreshrateChangeFullscreenVideo = false;
    private static DateTime _refreshrateChangeExecutionTime = DateTime.MinValue;

    private static List<RefreshRateSetting> _refreshRateSettings = null;
    private static double _workerFps;
    private static string _workerStrFile;
    private static MediaType _workerType;
    
    #endregion

    #region public enums

    public enum MediaType // copy present in g_player
    {
      Video,
      TV,
      Radio,
      Music,
      Recording,
      Unknown
    };

    #endregion

    #region private static methods

    private static void RefreshRateShowNotification(string msg, bool waitForFullscreen)
    {
      if (GUIGraphicsContext.IsFullScreenVideo == waitForFullscreen)
      {
        GUIMessage guiMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REFRESHRATE_CHANGED, 0, 0, 0, 0, 0, null);
        guiMsg.Label = "Refreshrate";
        guiMsg.Label2 = msg;
        guiMsg.Param1 = 5;

        GUIGraphicsContext.SendMessage(guiMsg);
      }
    }

    private static void NotifyRefreshRateChangedThread(object oMsg, object oWaitForFullScreen)
    {
      if (!(oMsg is string))
      {
        return;
      }
      if (!(oWaitForFullScreen is bool))
      {
        return;
      }

      string msg = (string)oMsg;
      bool waitForFullScreen = (bool)oWaitForFullScreen;

      DateTime now = DateTime.Now;
      TimeSpan ts = DateTime.Now - now;


      while (GUIGraphicsContext.IsFullScreenVideo != waitForFullScreen && ts.TotalSeconds < 10)
        //lets wait 5sec for fullscreen video to occur.
      {
        Thread.Sleep(50);
        ts = DateTime.Now - now;
      }

      if (GUIGraphicsContext.IsFullScreenVideo == waitForFullScreen)
      {
        RefreshRateShowNotification(msg, waitForFullScreen);
      }
      Thread.CurrentThread.Abort();
    }

    private static void NotifyRefreshRateChanged(string msg, bool waitForFullScreen)
    {
      using (Settings xmlreader = new MPSettings())
      {
        bool notify = xmlreader.GetValueAsBool("general", "notify_on_refreshrate", false);

        if (notify)
        {
          ThreadStart starter = delegate { NotifyRefreshRateChangedThread((object)msg, (object)waitForFullScreen); };
          Thread notifyRefreshRateChangedThread = new Thread(starter);
          notifyRefreshRateChangedThread.IsBackground = true;
          notifyRefreshRateChangedThread.Start();

          //Log.Info("RefreshRateChanger.NotifyRefreshRateChanged");          
        }
      }
    }

    private static RefreshRateSetting RetrieveRefreshRateChangerSetting(string name)
    {
      GetRefreshRateConfiguration();

      name = name.ToLowerInvariant();

      foreach (RefreshRateSetting setting in _refreshRateSettings)
      {
        if (setting.Name.ToLowerInvariant().Equals(name))
        {
          return setting;
        }
      }
      return null;
    }


    private static void GetRefreshRateConfiguration()
    {
      if (_refreshRateSettings == null)
      {
        _refreshRateSettings = new List<RefreshRateSetting>();

        NumberFormatInfo provider = new NumberFormatInfo();
        provider.NumberDecimalSeparator = ".";

        Settings xmlreader = new MPSettings();

        for (int i = 1; i < 100; i++)
        {
          string extCmd = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_ext", "");
          string name = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_name", "");

          if (string.IsNullOrEmpty(name))
          {
            continue;
          }

          string fps = xmlreader.GetValueAsString("general", name + "_fps", "");
          string hz = xmlreader.GetValueAsString("general", name + "_hz", "");

          RefreshRateSetting setting = new RefreshRateSetting();

          setting.Name = name;

          char[] splitter = {';'};
          string[] fpsArray = fps.Split(splitter);

          List<double> fpsList = new List<double>();
          foreach (string fpsItem in fpsArray)
          {
            double fpsAsDouble;
            if (double.TryParse(fpsItem, NumberStyles.AllowDecimalPoint, provider, out fpsAsDouble))
            {
              fpsList.Add(fpsAsDouble);
            }
          }

          setting.Fps = fpsList;

          double hzAsDouble = -1;
          double.TryParse(hz, NumberStyles.AllowDecimalPoint, provider, out hzAsDouble);

          setting.Hz = hzAsDouble;
          setting.ExtCmd = extCmd;

          _refreshRateSettings.Add(setting);
        }
      }
    }

    private static void FindExtCmdfromSettings(double fps, out double newRR,
                                               out string newExtCmd, out string newRRDescription)
    {
      GetRefreshRateConfiguration();

      newRR = 0;
      newExtCmd = "";
      newRRDescription = "";

      //Round fps down to 3 decimal places
      fps = Math.Round(fps, 3);

      foreach (RefreshRateSetting setting in _refreshRateSettings)
      {
        foreach (double fpsSetting in setting.Fps)
        {
          if (fps == fpsSetting)
          {
            newRR = setting.Hz;
            newExtCmd = setting.ExtCmd;
            //newRRDescription = setting.Name;
            newRRDescription = setting.Name + "@" + setting.Hz + "hz";
            return;
          }
        }
      }
    }

    private static bool RunExternalJob(string newExtCmd, string strFile, MediaType type, bool deviceReset)
    {
      Log.Info("RefreshRateChanger.RunExternalJob: running external job in order to change refreshrate {0}", newExtCmd);


      // extract the command from the parameters.
      string args = "";
      string cmd = "";

      newExtCmd = newExtCmd.Trim().ToLowerInvariant();

      int lenCmd = newExtCmd.Length;
      int idxCmd = newExtCmd.IndexOf(".cmd");

      if (idxCmd == -1)
      {
        idxCmd = newExtCmd.IndexOf(".exe");
      }

      if (idxCmd == -1)
      {
        idxCmd = newExtCmd.IndexOf(".bat");
      }

      if (idxCmd == -1) //we didnt find anything usable, lets carry on anyways.
      {
        cmd = newExtCmd;
      }
      else
      {
        cmd = newExtCmd.Substring(0, idxCmd + 4);
        if (idxCmd < lenCmd)
        {
          int idxArgs = newExtCmd.IndexOf(" ", idxCmd);
          if (idxArgs > idxCmd)
          {
            args = newExtCmd.Substring(idxArgs + 1);
          }
        }
      }

      ProcessStartInfo psi = new ProcessStartInfo(cmd);
      psi.RedirectStandardOutput = true;
      psi.WindowStyle = ProcessWindowStyle.Hidden;
      psi.UseShellExecute = false;
      psi.Arguments = args;
      psi.CreateNoWindow = true;

      Process changeRR = null;

      try
      {
        if (deviceReset)
        {
          _refreshrateChangeStrFile = strFile;
          _refreshrateChangeMediaType = type;
          _refreshrateChangePending = true;
          _refreshrateChangeExecutionTime = DateTime.Now;
        }
        changeRR = Process.Start(psi);
      }
      catch (Exception e)
      {
        _refreshrateChangePending = false;
        Log.Info("RefreshRateChanger.RunExternalJob: running external job failed {0}", e.Message);
        return false;
      }

      if (changeRR != null)
      {
        changeRR.WaitForExit(10000); //lets wait max 10secs on the external job to finish.

        if (changeRR.HasExited)
        {
          Log.Info("RefreshRateChanger.RunExternalJob: running external job completed");
          return true;
        }
        else
        {
          Log.Info("RefreshRateChanger.RunExternalJob: running external job DID not complete within the allowed 10 secs");
          return false;
        }
      }
      return false;
    }

    #endregion

    #region public static methods

    public static void ResetRefreshRateState()
    {
      _refreshrateChangeStrFile = "";
      _refreshrateChangeMediaType = MediaType.Unknown;
      _refreshrateChangePending = false;
      _refreshrateChangeFullscreenVideo = false;
      _refreshrateChangeExecutionTime = DateTime.MinValue;
      _refreshrateChangeCurrentRR = 0;
    }

    public static void SetRefreshRateBasedOnFPS(double fps, string strFile, MediaType type)
    {
      _workerFps = fps;
      _workerStrFile = strFile;
      _workerType = type;
      Thread workerThread = new Thread(new ThreadStart(SetRefreshRateBasedOnFpsThread))
      {
        IsBackground = true,
        Name = "SetRefreshRateBasedOnFPS thread",
        Priority = ThreadPriority.AboveNormal
      };
      workerThread.Start();
    }

    public static void SetRefreshRateBasedOnFpsThread()
    {
      try
      {
        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
            !GUIGraphicsContext.ForcedRefreshRate3D)
        {
          using (Settings xmlreader = new MPSettings())
          {
            if (!xmlreader.GetValueAsBool("general", "useInternalDRC", false))
            {
              return;
            }
          }
        }

        double currentRR = 0;
        if (GUIGraphicsContext.DX9Device != null && !GUIGraphicsContext.DX9Device.IsDisposed)
        {
          if ((GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal == -1) ||
              (GUIGraphicsContext.Direct3D.Adapters.Count <= GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal) ||
              (GUIGraphicsContext.Direct3D.Adapters.Count > Screen.AllScreens.Length))
          {
            Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: adapter number out of bounds");
          }
          else
          {
            currentRR =
              GUIGraphicsContext.Direct3D.Adapters[GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal].CurrentDisplayMode.RefreshRate;
          }
          _refreshrateChangeCurrentRR = currentRR;

          bool deviceReset;
          bool forceRefreshRate;
          using (Settings xmlreader = new MPSettings())
          {
            if (!xmlreader.GetValueAsBool("general", "autochangerefreshrate", false))
            {
              if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR ||
                  !GUIGraphicsContext.ForcedRefreshRate3D)
              {
                Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: 'auto refreshrate changer' disabled");
                return;
              }
            }
            forceRefreshRate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false);
            deviceReset = xmlreader.GetValueAsBool("general", "devicereset", false);
          }

          double newRR;
          string newExtCmd;
          string newRRDescription;
          FindExtCmdfromSettings(_workerFps, out newRR, out newExtCmd, out newRRDescription);

          if (newRR > 0 && (currentRR != newRR || forceRefreshRate) ||
              (GUIGraphicsContext.ForcedRefreshRate3D && !GUIGraphicsContext.ForcedRefreshRate3DDone))
          {
            Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: current refreshrate is {0}hz - changing it to {1}hz",
              currentRR, newRR);

            // Add a delay for HDR
            if (!g_Player.Playing && GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
            {
              Log.Debug("RefreshRateChanger.SetRefreshRateBasedOnFPS delayed start when using madVR");
              Thread.Sleep(10000);
            }

            if (String.IsNullOrEmpty(newExtCmd))
            {
              Log.Info(
                "RefreshRateChanger.SetRefreshRateBasedOnFPS: using internal win32 method for changing refreshrate. current is {0}hz, desired is {1}",
                currentRR, newRR);
              Log.Info("RefreshRateChanger AdapterOrdinal value is {0}",
                (uint) GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal);
              Win32.CycleRefreshRate((uint) GUIGraphicsContext.DX9Device.Capabilities.AdapterOrdinal, newRR);
              NotifyRefreshRateChanged(newRRDescription, false);
            }
            else if (RunExternalJob(newExtCmd, _workerStrFile, _workerType, deviceReset) && newRR != currentRR)
            {
              Win32.FixDwm();
              NotifyRefreshRateChanged(newRRDescription, false);
            }

            if (GUIGraphicsContext.Vmr9Active &&
                GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
            {
              Log.Info(
                "RefreshRateChanger.SetRefreshRateBasedOnFPS: dynamic refresh rate change - notify video renderer");
              VMR9Util.g_vmr9.UpdateEVRDisplayFPS();
            }
          }
          else
          {
            if (newRR == 0)
            {
              Log.Info(
                "RefreshRateChanger.SetRefreshRateBasedOnFPS: could not find a matching refreshrate based on {0} fps (check config)",
                _workerFps);
            }
            else
            {
              Log.Info(
                "RefreshRateChanger.SetRefreshRateBasedOnFPS: no refreshrate change required. current is {0}hz, desired is {1}",
                currentRR, newRR);
            }
          }
          Log.Info("RefreshRateChanger.SwitchFocus");
          Util.Utils.SwitchFocus();
        }
      }
      catch (Exception ex)
      {
        // RefreshRate failed
        Log.Error("RefreshRate failed: {0}", ex.Message);
      }
    }

    // defaults the refreshrate
    public static void AdaptRefreshRate()
    {
      if (_refreshrateChangePending || _refreshrateChangeCurrentRR == 0)
      {
        return;
      }

      string defaultKeyHZ = "";
      double defaultHZ = 0;
      bool enabled = false;
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";
      double defaultFPS = 0;
      using (Settings xmlreader = new MPSettings())
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);
        if (!enabled)
        {
          if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR ||
              !GUIGraphicsContext.ForcedRefreshRate3D)
          {
            Log.Info("RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' disabled");
            return;
          }
        }

        bool useDefaultHz = xmlreader.GetValueAsBool("general", "use_default_hz", false);

        if (!useDefaultHz)
        {
          if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR ||
              !GUIGraphicsContext.ForcedRefreshRate3D)
          {
            Log.Info(
              "RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' not going back to default refreshrate");
            return;
          }
        }

        defaultKeyHZ = xmlreader.GetValueAsString("general", "default_hz", "");

        if (defaultKeyHZ.Length > 0)
        {
          double.TryParse(defaultKeyHZ, NumberStyles.AllowDecimalPoint,
                          provider, out defaultHZ);
        }


        foreach (RefreshRateSetting setting in _refreshRateSettings)
        {
          if (setting.Hz == defaultHZ)
          {
            if (setting.Fps.Count > 0)
            {
              defaultFPS = setting.Fps[0];
            }
          }
        }
      }

      SetRefreshRateBasedOnFPS(defaultFPS, "", MediaType.Unknown);
      ResetRefreshRateState();
    }

    // change screen refresh rate based on media framerate
    public static void AdaptRefreshRate(string strFile, MediaType type)
    {
      if (_refreshrateChangePending)
      {
        return;
      }

      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isDVD = Util.Utils.IsDVD(strFile);
      bool isVideo = Util.Utils.IsVideo(strFile);
      bool isRTSP = Util.Utils.IsRTSP(strFile); //rtsp users for live TV and recordings.

      if (!isTV && !isDVD && !isVideo && !isRTSP)
      {
        return;
      }

      bool enabled = false;
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";
      bool deviceReset = false;
      bool force_refresh_rate = false;
      using (Settings xmlreader = new MPSettings())
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false) || GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
                  GUIGraphicsContext.ForcedRefreshRate3D;

        if (!enabled)
        {
          Log.Info("RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' disabled");
          return;
        }

        deviceReset = xmlreader.GetValueAsBool("general", "devicereset", false);
        force_refresh_rate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false);
      }

      RefreshRateSetting setting = RetrieveRefreshRateChangerSetting("TV");

      if (setting == null)
      {
        Log.Error(
          "RefreshRateChanger.AdaptRefreshRate: TV section not found in mediaportal.xml, please delete file and reconfigure.");
        return;
      }

      List<double> tvFPS = setting.Fps;
      double fps = -1;

      if ((isVideo || isDVD || isTV) && !isRTSP)
      {
        if (g_Player.MediaInfo != null)
        {
          fps = g_Player.MediaInfo.Framerate;
          if (fps == 0.0 && isTV && tvFPS.Count > 0) //fallback for mediainfo.wrapper<=20.09
          {
            fps = tvFPS[0];
          }

          Log.Info("RefreshRateChanger.AdaptRefreshRate: Scantype on file {0} is {1}", strFile, g_Player.MediaInfo.ScanType);

          if (g_Player.MediaInfo.IsInterlaced ||
            (g_Player.MediaInfo.ScanType != null && g_Player.MediaInfo.ScanType.Equals("mbaff", StringComparison.OrdinalIgnoreCase)))
          {
            Log.Info("RefreshRateChanger.AdaptRefreshRate: Interlacing detected on file {0}. Fps is {1}", strFile, fps);

            if (fps == 25.00)
              fps = 50.00;
            else if (fps == 29.97)
              fps = 59.97;
            else if (fps == 30.00)
              fps = 60.00;
          }
        }
        else
        {
          StackTrace st = new StackTrace(true);
          StackFrame sf = st.GetFrame(0);

          Log.Error("RefreshRateChanger.AdaptRefreshRate: g_Player.MediaInfo was null. file: {0} st: {1}", strFile,
                    sf.GetMethod().Name);

          _workerFps = 0;

          return;
        }
      }
      else if (isRTSP)
      {
        if (tvFPS.Count > 0)
        {
          fps = tvFPS[0];
        }
      }

      if (fps < 1)
      {
        Log.Info("RefreshRateChanger.AdaptRefreshRate: unable to guess framerate on file {0}", strFile);
      }
      else
      {
        Log.Info("RefreshRateChanger.AdaptRefreshRate: framerate on file {0} is {1}", strFile, fps);
      }

      SetRefreshRateBasedOnFPS(fps, strFile, type);
    }

    public static void AdaptRefreshRateFromVideoDecoder(string strFile)
    {
      if (_workerFps == 0 && g_Player.Player is VideoPlayerVMR9)
      {
        Log.Info("[AdaptRefreshRateFromVideoDecoder] Try detect fps from Video Decoder Pin");

        try
        {
          double dFps = Math.Round(VMR9Util.g_vmr9.GetEVRVideoFPS(3), 2);

          if (dFps <= 0)
          {
            DirectShowLib.IBaseFilter filter = ((VideoPlayerVMR9)g_Player.Player).filterCodec.VideoCodec;
            if (filter != null)
            {
              Log.Debug("[AdaptRefreshRateFromVideoDecoder] Video Decoder found.");

              DirectShowLib.IPin pin;
              int iResult = filter.FindPin("Out", out pin);
              if (iResult == 0)
              {
                Log.Debug("[AdaptRefreshRateFromVideoDecoder] Output Pin of Video Decoder retrieved.");
                DirectShowLib.AMMediaType mediatype = new DirectShowLib.AMMediaType();
                iResult = pin.ConnectionMediaType(mediatype);

                if (iResult == 0)
                {
                  bool bInterlaced = false;
                  string strGUID = mediatype.formatType.ToString();
                  Log.Debug("[AdaptRefreshRateFromVideoDecoder] AMMediaType: " + strGUID);

                  if (strGUID.Equals("F72A76A0-EB0A-11D0-ACE4-0000C0CC16BA", StringComparison.OrdinalIgnoreCase) //VIDEOINFOHEADER2
                      || strGUID.Equals("E06D80E3-DB46-11CF-B4D1-00805F6CBBEA", StringComparison.OrdinalIgnoreCase)) //WMFORMAT_MPEG2Video
                  {
                    DirectShowLib.VideoInfoHeader2 videoHeader = new DirectShowLib.VideoInfoHeader2();
                    Marshal.PtrToStructure(mediatype.formatPtr, videoHeader);
                    if (videoHeader != null)
                    {
                      bInterlaced = (videoHeader.InterlaceFlags & DirectShowLib.AMInterlace.IsInterlaced) == DirectShowLib.AMInterlace.IsInterlaced;
                      dFps = Math.Round(10000000F / videoHeader.AvgTimePerFrame, 2);
                      Log.Debug("[AdaptRefreshRateFromVideoDecoder] AvgTimePerFrame from VideoInfoHeader2: {0}, Interlaced: {1}",
                          videoHeader.AvgTimePerFrame, bInterlaced);
                    }
                  }
                  else
                  {
                    DirectShowLib.VideoInfoHeader videoHeader = new DirectShowLib.VideoInfoHeader();
                    Marshal.PtrToStructure(mediatype.formatPtr, videoHeader);

                    if (videoHeader != null)
                    {
                      Log.Debug("[AdaptRefreshRateFromVideoDecoder] AvgTimePerFrame from VideoInfoHeader: " + videoHeader.AvgTimePerFrame);
                      dFps = Math.Round(10000000F / videoHeader.AvgTimePerFrame, 2);
                    }
                  }

                  if (dFps == 23.98)
                    dFps = 23.976;

                  Log.Info("[AdaptRefreshRateFromVideoDecoder] Detected FPS from Video Decoder: " + dFps);

                  if (bInterlaced)
                  {
                    switch (dFps)
                    {
                      case 25:
                        dFps = 50.0;
                        break;

                      case 29.97:
                        dFps = 59.97;
                        break;

                      case 30:
                        dFps = 60.0;
                        break;
                    }
                  }
                }
                else
                  Log.Debug("[AdaptRefreshRateFromVideoDecoder] Unable to get AMMediaType.");

                DShowNET.Helper.DirectShowUtil.ReleaseComObject(pin);
              }

              //DShowNET.Helper.DirectShowUtil.ReleaseComObject(filter);
            }
          }
          else
          {
            if (dFps == 23.98)
              dFps = 23.976;

            Log.Info("[AdaptRefreshRateFromVideoDecoder] Detected FPS from EVR: " + dFps);
          }

          if (dFps > 0)
            SetRefreshRateBasedOnFPS(dFps, strFile, MediaType.Video);
        }
        catch (Exception ex)
        {
          Log.Warn("[AdaptRefreshRateFromVideoDecoder] Exception when getting refresh rate from Video Decoder: " + ex.ToString());
        }
      }

    }

    #endregion

    #region public properties

    public static bool RefreshRateChangeRunning { get; set; }

    public static bool RefreshRateChangePending
    {
      get { return _refreshrateChangePending; }
      set { _refreshrateChangePending = value; }
    }

    public static string RefreshRateChangeStrFile
    {
      get { return _refreshrateChangeStrFile; }
    }


    public static MediaType RefreshRateChangeMediaType
    {
      get { return _refreshrateChangeMediaType; }
    }

    public static DateTime RefreshRateChangeExecutionTime
    {
      get { return _refreshrateChangeExecutionTime; }
    }

    public static bool RefreshRateChangeFullscreenVideo
    {
      get { return _refreshrateChangeFullscreenVideo; }
      set { _refreshrateChangeFullscreenVideo = value; }
    }

    public static double RefreshRateChangeCurrentRR
    {
      get { return _refreshrateChangeCurrentRR; }
    }

    public static double RefreshRateChangeWorkerFps
    {
      get { return _workerFps; }
    }

    #endregion
  }
}
