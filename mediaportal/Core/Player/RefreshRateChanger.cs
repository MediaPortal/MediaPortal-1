#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#region using

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;

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
      public uint cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICE));
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string DeviceName = "";
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceString = "";
      public DISPLAY_DEVICE_StateFlags StateFlags = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceID = "";
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceKey = "";
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
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
      public string dmDeviceName = null;
      public ushort dmSpecVersion = 0;
      public ushort dmDriverVersion = 0;
      public ushort dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE_Display));
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
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
      public string dmFormName = null;
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
    public extern static int EnumDisplayDevices([In] string lpDevice, [In]uint iDevNum, [In][Out] DISPLAY_DEVICE lpDisplayDevice, [In] uint dwFlags);


    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public extern static int EnumDisplaySettingsEx([In] string lpszDeviceName,
    [In] EnumDisplaySettings_EnumMode iModeNum, [In][Out] DEVMODE_Display
    lpDevMode, [In] uint dwFlags);


    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public extern static ChangeDisplaySettings_Result
    ChangeDisplaySettingsEx([In] string lpszDeviceName, [In] DEVMODE_Display
    lpDevMode, [In] IntPtr hwnd, [In] ChangeDisplaySettings_Flags dwFlags, [In]IntPtr lParam);


    public static void CycleRefreshRate(uint monitorIndex, uint refreshRate)
    {
      Win32.DISPLAY_DEVICE displayDevice = new Win32.DISPLAY_DEVICE();
      int result = Win32.EnumDisplayDevices(null, monitorIndex, displayDevice, 0);

      if (result != 0)
      {
        Win32.DEVMODE_Display devMode = new Win32.DEVMODE_Display();
        devMode.dmFields = Win32.DEVMODE_Fields.DM_DISPLAYFREQUENCY;
        devMode.dmDisplayFrequency = refreshRate;
        Win32.ChangeDisplaySettings_Result r = Win32.ChangeDisplaySettingsEx(displayDevice.DeviceName, devMode, IntPtr.Zero, Win32.ChangeDisplaySettings_Flags.None, IntPtr.Zero);
      }
    }
  }

  public static class RefreshRateChanger
  {
    #region public constants

    public const int WAIT_FOR_REFRESHRATE_RESET_MAX = 10; //10 secs

    #endregion

    #region private delegates

    private delegate void NotifyRefreshRateChangedSuccessful(string msg, bool waitForFullScreen);

    private delegate void NotifyRefreshRateInteractGUI(string msg, bool waitForFullScreen);

    #endregion

    #region private events

    private static event NotifyRefreshRateChangedSuccessful OnNotifyRefreshRateChangedCompleted;

    #endregion

    #region private static vars

    private static double _refreshrateChangeCurrentRR = 0;
    private static string _refreshrateChangeStrFile = "";
    private static MediaType _refreshrateChangeMediaType;
    private static bool _refreshrateChangePending = false;
    private static bool _refreshrateChangeFullscreenVideo = false;
    private static DateTime _refreshrateChangeExecutionTime = DateTime.MinValue;

    #endregion

    #region public enums

    public enum MediaType
    {
      Video,
      TV,
      Radio,
      Music,
      Recording,
      Unknown
    } ;

    #endregion

    #region private static methods

    private static void NotifyRefreshRateChangedCompleted(string msg, bool waitForFullScreen)
    {
      try
      {
        GUIGraphicsContext.form.Invoke(new NotifyRefreshRateInteractGUI(RefreshRateShowNotification),
                                       new object[] {msg, waitForFullScreen});
      }
      catch (Exception)
      {
      }
    }


    private static void RefreshRateShowNotification(string msg, bool waitForFullscreen)
    {
      if (GUIGraphicsContext.IsFullScreenVideo == waitForFullscreen)
      {
        /*
        Dialogs.GUIDialogNotify pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (pDlgNotify != null)
        {
          pDlgNotify.Reset();
          pDlgNotify.ClearAll();
          pDlgNotify.SetHeading("Refreshrate");
          pDlgNotify.SetText(msg);
          pDlgNotify.TimeOut = 5;
          pDlgNotify.DoModal(GUIWindowManager.ActiveWindow);
        }

        pDlgNotify = null;*/
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

      string msg = (string) oMsg;
      bool waitForFullScreen = (bool) oWaitForFullScreen;

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
        if (OnNotifyRefreshRateChangedCompleted != null)
        {
          OnNotifyRefreshRateChangedCompleted(msg, waitForFullScreen);
        }
      }
      Thread.CurrentThread.Abort();
    }

    private static void NotifyRefreshRateChanged(string msg, bool waitForFullScreen)
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bool notify = xmlreader.GetValueAsBool("general", "notify_on_refreshrate", false);

        if (notify)
        {
          if (OnNotifyRefreshRateChangedCompleted == null)
          {
            OnNotifyRefreshRateChangedCompleted +=
              new NotifyRefreshRateChangedSuccessful(NotifyRefreshRateChangedCompleted);
          }

          ThreadStart starter = delegate { NotifyRefreshRateChangedThread((object) msg, (object) waitForFullScreen); };
          Thread notifyRefreshRateChangedThread = new Thread(starter);
          notifyRefreshRateChangedThread.IsBackground = true;
          notifyRefreshRateChangedThread.Start();
        }
      }
    }

    private static double[] RetriveRefreshRateChangerSettings(string key)
    {
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";

      string[] arrStr = new string[0];
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        arrStr = xmlreader.GetValueAsString("general", key, "").Split(';');
      }

      double[] settingsHZ = new double[arrStr.Length];

      for (int i = 0; i < arrStr.Length; i++)
      {
        double hzItem = 0;
        double.TryParse(arrStr[i], NumberStyles.AllowDecimalPoint, provider, out hzItem);

        if (hzItem > 0)
        {
          settingsHZ[i] = hzItem;
        }
      }
      return settingsHZ;
    }

    private static void FindExtCmdfromSettings(double fps, double currentRR, bool deviceReset, out double newRR,
                                               out string newExtCmd, out string newRRDescription)
    {
      double cinemaHZ = 0;
      double palHZ = 0;
      double ntscHZ = 0;
      double tvHZ = 0;

      double[] cinemaFPS = RetriveRefreshRateChangerSettings("cinema_fps");
      double[] palFPS = RetriveRefreshRateChangerSettings("pal_fps");
      double[] ntscFPS = RetriveRefreshRateChangerSettings("ntsc_fps");
      double[] tvFPS = RetriveRefreshRateChangerSettings("tv_fps");

      string cinemaEXT = "";
      string palEXT = "";
      string ntscEXT = "";
      string tvEXT = "";

      newRR = 0;
      newExtCmd = "";
      newRRDescription = "";

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        NumberFormatInfo provider = new NumberFormatInfo();
        provider.NumberDecimalSeparator = ".";

        double.TryParse(xmlreader.GetValueAsString("general", "cinema_hz", ""), NumberStyles.AllowDecimalPoint, provider,
                        out cinemaHZ);
        double.TryParse(xmlreader.GetValueAsString("general", "pal_hz", ""), NumberStyles.AllowDecimalPoint, provider,
                        out palHZ);
        double.TryParse(xmlreader.GetValueAsString("general", "ntsc_hz", ""), NumberStyles.AllowDecimalPoint, provider,
                        out ntscHZ);
        double.TryParse(xmlreader.GetValueAsString("general", "tv_hz", ""), NumberStyles.AllowDecimalPoint, provider,
                        out tvHZ);

        cinemaEXT = xmlreader.GetValueAsString("general", "cinema_ext", "");
        palEXT = xmlreader.GetValueAsString("general", "pal_ext", "");
        ntscEXT = xmlreader.GetValueAsString("general", "ntsc_ext", "");
        tvEXT = xmlreader.GetValueAsString("general", "tv_ext", "");
      }
      bool newRRRequired = false;
      foreach (double fpsItem in cinemaFPS)
      {
        if (fpsItem == fps)
        {
          newRRRequired = (currentRR != cinemaHZ || !deviceReset);
          if (newRRRequired)
          {
            newRRDescription = "CINEMA@" + cinemaHZ + "hz";
            newRR = cinemaHZ;
            newExtCmd = cinemaEXT;
            return;
          }
          break;
        }
      }

      foreach (double fpsItem in palFPS)
      {
        if (fpsItem == fps)
        {
          newRRRequired = (currentRR != palHZ || !deviceReset);
          if (newRRRequired)
          {
            newRRDescription = "PAL@" + palHZ + "hz";
            newRR = palHZ;
            newExtCmd = palEXT;
            return;
          }
          break;
        }
      }

      foreach (double fpsItem in ntscFPS)
      {
        if (fpsItem == fps)
        {
          newRRRequired = (currentRR != ntscHZ || !deviceReset);
          if (newRRRequired)
          {
            newRRDescription = "NTSC@" + ntscHZ + "hz";
            newRR = ntscHZ;
            newExtCmd = ntscEXT;
            return;
          }
          break;
        }
      }
      foreach (double fpsItem in tvFPS)
      {
        if (fpsItem == fps)
        {
          newRRRequired = (currentRR != tvHZ || !deviceReset);
          if (newRRRequired)
          {
            newRRDescription = "TV@" + tvHZ + "hz";
            newRR = tvHZ;
            newExtCmd = tvEXT;
            return;
          }
          break;
        }
      }
    }

    private static bool RunExternalJob(string newExtCmd, string strFile, MediaType type, bool deviceReset)
    {      
      Log.Info("RefreshRateChanger.RunExternalJob: running external job in order to change refreshrate {0}", newExtCmd);


      // extract the command from the parameters.
      string args = "";
      string cmd = "";

      newExtCmd = newExtCmd.Trim().ToLower();

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
      finally
      {
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
    }

    public static void SetRefreshRateBasedOnFPS(double fps, string strFile, MediaType type)
    {
      int currentScreenNr = GUIGraphicsContext.currentScreenNumber;
      double currentRR = 0;
      if ((currentScreenNr == -1) || (Manager.Adapters.Count <= currentScreenNr))
      {
        Log.Info(
          "RefreshRateChanger.SetRefreshRateBasedOnFPS: could not aquire current screen number, or current screen number bigger than number of adapters available.");
      }
      else
      {
        currentRR = Manager.Adapters[currentScreenNr].CurrentDisplayMode.RefreshRate;
      }

      _refreshrateChangeCurrentRR = currentRR;

      bool enabled = false;
      bool deviceReset = false;
      bool force_refresh_rate = false;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);

        if (!enabled)
        {
          Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: 'auto refreshrate changer' disabled");
          return;
        }
        force_refresh_rate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false);        
        deviceReset = xmlreader.GetValueAsBool("general", "devicereset", false);
      }

      double newRR = 0;
      string newExtCmd = "";
      string newRRDescription = "";
      FindExtCmdfromSettings(fps, currentRR, deviceReset, out newRR, out newExtCmd, out newRRDescription);

      if ((currentRR != newRR && newRR > 0) || force_refresh_rate)
        // //run external command in order to change refresh rate.
      {
        Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: current refreshrate is {0}hz - changing it to {1}hz",
                 currentRR, newRR);

        if (newExtCmd.Length == 0)
        {
          Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: using internal win32 method for changing refreshrate. current is {0}hz, desired is {1}",currentRR, newRR);
          Win32.CycleRefreshRate((uint)currentScreenNr, (uint)newRR);
          NotifyRefreshRateChanged(newRRDescription, (strFile.Length > 0));
        }
        else if (RunExternalJob(newExtCmd, strFile, type, deviceReset) && newRR != currentRR)
        {
          NotifyRefreshRateChanged(newRRDescription, (strFile.Length > 0));
        }
      }
      else
      {
        Log.Info(
          "RefreshRateChanger.SetRefreshRateBasedOnFPS: no refreshrate change required. current is {0}hz, desired is {1}",
          currentRR, newRR);
      }
    }


    // defaults the refreshrate
    public static void AdaptRefreshRate()
    {
      if (_refreshrateChangePending)
      {
        return;
      }

      string defaultKeyHZ = "";
      double defaultHZ = 0;
      bool enabled = false;
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";
      double defaultFPS = 0;
      bool deviceReset = false;
      bool force_refresh_rate = false;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);
        if (!enabled)
        {
          Log.Info("RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' disabled");
          return;
        }

        force_refresh_rate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false);
        ;

        bool useDefaultHz = xmlreader.GetValueAsBool("general", "use_default_hz", false);
        ;
        if (!useDefaultHz)
        {
          Log.Info(
            "RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' not going back to default refreshrate");
          return;
        }

        defaultKeyHZ = xmlreader.GetValueAsString("general", "default_hz", "");

        if (defaultKeyHZ.Length > 0)
        {
          double.TryParse(xmlreader.GetValueAsString("general", defaultKeyHZ, ""), NumberStyles.AllowDecimalPoint,
                          provider, out defaultHZ);
        }

        if (defaultKeyHZ.IndexOf("cinema") > -1)
        {
          double[] cinemafps = RetriveRefreshRateChangerSettings("cinema_fps");
          if (cinemafps.Length > 0)
          {
            defaultFPS = cinemafps[0];
          }
        }
        else if (defaultKeyHZ.IndexOf("pal") > -1)
        {
          double[] palfps = RetriveRefreshRateChangerSettings("pal_fps");
          if (palfps.Length > 0)
          {
            defaultFPS = palfps[0];
          }
        }
        else if (defaultKeyHZ.IndexOf("ntsc") > -1)
        {
          double[] ntscfps = RetriveRefreshRateChangerSettings("ntsc_fps");
          if (ntscfps.Length > 0)
          {
            defaultFPS = ntscfps[0];
          }
        }
        else
        {
          double[] tvfps = RetriveRefreshRateChangerSettings("tv_fps");
          if (tvfps.Length > 0)
          {
            defaultFPS = tvfps[0];
          }
        }

        deviceReset = xmlreader.GetValueAsBool("general", "devicereset", false);
      }

      SetRefreshRateBasedOnFPS(defaultFPS, "", MediaType.Unknown);
      /*
      double currentRR = 0;
      int currentScreenNr = GUIGraphicsContext.currentScreenNumber;      
      
      if ((currentScreenNr == -1) || (Microsoft.DirectX.Direct3D.Manager.Adapters.Count <= currentScreenNr))
      {
        Log.Info("g_Player could not aquire current screen number, or current screen number bigger than number of adapters available.");
      }
      else
      {
        currentRR = Microsoft.DirectX.Direct3D.Manager.Adapters[currentScreenNr].CurrentDisplayMode.RefreshRate;
      }      

      if ((defaultHZ > 0 && defaultHZ != currentRR) || force_refresh_rate)
      {
        Log.Info("g_Player changing back refreshrate to {0}hz", defaultHZ);
        double newRR = 0;
        string newExtCmd = "";
        string newRRDescription = "";
        FindExtCmdfromSettings(defaultFPS, currentRR, deviceReset, out newRR, out newExtCmd, out newRRDescription);

        if ((currentRR != newRR && newRR > 0) || force_refresh_rate) //run external command in order to change refresh rate.
        {
          if (RunExternalJob(newExtCmd, "", MediaType.Unknown, deviceReset) && newRR != currentRR)
          {          
            NotifyRefreshRateChanged(newRRDescription, false);
          }
        }
      } 
      */
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
      bool IsAVStream = Util.Utils.IsAVStream(strFile); //rtsp users for live TV and recordings.


      if (!isTV && !isDVD && !isVideo)
      {
        return;
      }

      bool enabled = false;
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";
      bool deviceReset = false;
      bool force_refresh_rate = false;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);

        if (!enabled)
        {
          Log.Info("RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' disabled");
          return;
        }

        deviceReset = xmlreader.GetValueAsBool("general", "devicereset", false);
        force_refresh_rate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false);
        ;
      }
      double[] tvFPS = RetriveRefreshRateChangerSettings("tv_fps");
      double fps = -1;

      if ((isVideo || isDVD) && (!IsAVStream && !isTV))
      {
        if (g_Player.MediaInfo != null)
        {
          fps = g_Player.MediaInfo.Framerate;
        }
        else
        {
          Log.Error("RefreshRateChanger.AdaptRefreshRate: g_Player.MediaInfo was null.");
          return;
        }
        /*
        MediaInfo mI = null;
        try
        {
          mI = new MediaInfo();
          mI.Open(strFile);
          double.TryParse(mI.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, provider, out fps);
        }
        catch (Exception ex)
        {
          Log.Error(
            "RefreshRateChanger.AdaptRefreshRate: unable to call external DLL - medialib info (make sure 'MediaInfo.dll' is located in MP root dir.) {0}",
            ex.Message);
        }
        finally
        {
          if (mI != null)
          {
            mI.Close();
          }
        }
        */
      }
      else if (isTV || IsAVStream)
      {
        if (tvFPS.Length > 0)
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

      /*
      int currentScreenNr = GUIGraphicsContext.currentScreenNumber;
      double currentRR = 0;
      if ((currentScreenNr == -1) || (Microsoft.DirectX.Direct3D.Manager.Adapters.Count <= currentScreenNr))
      {
        Log.Info("g_Player could not aquire current screen number, or current screen number bigger than number of adapters available.");        
      }
      else
      {
        currentRR = Microsoft.DirectX.Direct3D.Manager.Adapters[currentScreenNr].CurrentDisplayMode.RefreshRate;
      }
      
      double newRR = 0;      
      string newExtCmd = "";

      string newRRDescription = "";
      FindExtCmdfromSettings(fps, currentRR, deviceReset, out newRR, out newExtCmd, out newRRDescription);

      if ((currentRR != newRR && newRR > 0) || force_refresh_rate)// //run external command in order to change refresh rate.
      {        
        Log.Info("g_Player current refreshrate is {0}hz - changing it to {1}hz", currentRR, newRR);
        if (RunExternalJob(newExtCmd, strFile, type, deviceReset) && newRR != currentRR)
        {          
          NotifyRefreshRateChanged(newRRDescription, true);
        }
      }
      {
        Log.Info("g_Player no refreshrate change required. current is {0}hz, desired is {1}", currentRR, newRR);
      }
      */
    }

    #endregion

    #region public properties

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

    #endregion
  }
}