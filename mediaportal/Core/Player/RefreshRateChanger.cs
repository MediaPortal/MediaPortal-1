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
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;

#endregion

namespace MediaPortal.Player
{
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

    private static string _refreshrateChangeStrFile = "";
    private static MediaType _refreshrateChangeMediaType;
    private static bool _refreshrateChangePending = false;
    private static bool _refreshrateChangeFullscreenVideo = false;
    private static DateTime _refreshrateChangeExecutionTime = DateTime.MinValue;

    #endregion

    #region public enums

    public enum MediaType { Video, TV, Radio, Music, Recording, Unknown };

    #endregion   

    #region private static methods

    private static void NotifyRefreshRateChangedCompleted(string msg, bool waitForFullScreen)
    {
      try
      {
        GUIGraphicsContext.form.Invoke(new NotifyRefreshRateInteractGUI(RefreshRateShowNotification), new object[] { msg, waitForFullScreen });
      }
      catch (Exception) { }
    }


    private static void RefreshRateShowNotification(string msg, bool waitForFullscreen)
    {
      if (GUIGraphicsContext.IsFullScreenVideo == waitForFullscreen)
      {
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

        pDlgNotify = null;
      }
    }

    private static void NotifyRefreshRateChangedThread(object oMsg, object oWaitForFullScreen)
    {
      if (!(oMsg is string)) return;
      if (!(oWaitForFullScreen is bool)) return;

      string msg = (string)oMsg;
      bool waitForFullScreen = (bool)oWaitForFullScreen;

      DateTime now = DateTime.Now;
      TimeSpan ts = DateTime.Now - now;


      while (GUIGraphicsContext.IsFullScreenVideo != waitForFullScreen && ts.TotalSeconds < 10) //lets wait 5sec for fullscreen video to occur.
      {
        Thread.Sleep(50);
        ts = DateTime.Now - now;
      }

      if (GUIGraphicsContext.IsFullScreenVideo == waitForFullScreen)
      {
        if (OnNotifyRefreshRateChangedCompleted != null)
          OnNotifyRefreshRateChangedCompleted(msg, waitForFullScreen);
      }
      Thread.CurrentThread.Abort();
    }

    private static void NotifyRefreshRateChanged(string msg, bool waitForFullScreen)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bool notify = xmlreader.GetValueAsBool("general", "notify_on_refreshrate", false);

        if (notify)
        {
          if (OnNotifyRefreshRateChangedCompleted == null)
          {
            OnNotifyRefreshRateChangedCompleted += new NotifyRefreshRateChangedSuccessful(NotifyRefreshRateChangedCompleted);
          }

          ThreadStart starter = delegate { NotifyRefreshRateChangedThread((object)msg, (object)waitForFullScreen); };
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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

    private static void FindExtCmdfromSettings(double fps, double currentRR, bool deviceReset, out double newRR, out string newExtCmd, out string newRRDescription)
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

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        NumberFormatInfo provider = new NumberFormatInfo();
        provider.NumberDecimalSeparator = ".";

        double.TryParse(xmlreader.GetValueAsString("general", "cinema_hz", ""), NumberStyles.AllowDecimalPoint, provider, out cinemaHZ);
        double.TryParse(xmlreader.GetValueAsString("general", "pal_hz", ""), NumberStyles.AllowDecimalPoint, provider, out palHZ);
        double.TryParse(xmlreader.GetValueAsString("general", "ntsc_hz", ""), NumberStyles.AllowDecimalPoint, provider, out ntscHZ);
        double.TryParse(xmlreader.GetValueAsString("general", "tv_hz", ""), NumberStyles.AllowDecimalPoint, provider, out tvHZ);

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

      System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(cmd);
      psi.RedirectStandardOutput = true;
      psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
      psi.UseShellExecute = false;
      psi.Arguments = args;
      psi.CreateNoWindow = true;

      System.Diagnostics.Process changeRR = null;

      try
      {
        if (deviceReset)
        {
          _refreshrateChangeStrFile = strFile;
          _refreshrateChangeMediaType = type;
          _refreshrateChangePending = true;
          _refreshrateChangeExecutionTime = DateTime.Now;
        }
        changeRR = System.Diagnostics.Process.Start(psi);
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
      if ((currentScreenNr == -1) || (Microsoft.DirectX.Direct3D.Manager.Adapters.Count <= currentScreenNr))
      {
        Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: could not aquire current screen number, or current screen number bigger than number of adapters available.");
      }
      else
      {
        currentRR = Microsoft.DirectX.Direct3D.Manager.Adapters[currentScreenNr].CurrentDisplayMode.RefreshRate;
      }

      bool enabled = false;
      bool deviceReset = false;
      bool force_refresh_rate = false;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);

        if (!enabled)
        {
          Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: 'auto refreshrate changer' disabled");
          return;
        }
        force_refresh_rate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false); ;
        deviceReset = xmlreader.GetValueAsBool("general", "devicereset", false);
      }

      double newRR = 0;
      string newExtCmd = "";
      string newRRDescription = "";
      FindExtCmdfromSettings(fps, currentRR, deviceReset, out newRR, out newExtCmd, out newRRDescription);

      if ((currentRR != newRR && newRR > 0) || force_refresh_rate)// //run external command in order to change refresh rate.
      {
        Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: current refreshrate is {0}hz - changing it to {1}hz", currentRR, newRR);
        if (RunExternalJob(newExtCmd, strFile, type, deviceReset) && newRR != currentRR)
        {
          NotifyRefreshRateChanged(newRRDescription, (strFile.Length > 0));
        }
      }
      {
        Log.Info("RefreshRateChanger.SetRefreshRateBasedOnFPS: no refreshrate change required. current is {0}hz, desired is {1}", currentRR, newRR);
      }
    }



    // defaults the refreshrate
    public static void AdaptRefreshRate()
    {
      if (_refreshrateChangePending) return;

      string defaultKeyHZ = "";
      double defaultHZ = 0;
      bool enabled = false;
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";
      double defaultFPS = 0;
      bool deviceReset = false;
      bool force_refresh_rate = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);
        if (!enabled)
        {
          Log.Info("RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' disabled");
          return;
        }

        force_refresh_rate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false); ;

        bool useDefaultHz = xmlreader.GetValueAsBool("general", "use_default_hz", false); ;
        if (!useDefaultHz)
        {
          Log.Info("RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' not going back to default refreshrate");
          return;
        }

        defaultKeyHZ = xmlreader.GetValueAsString("general", "default_hz", "");

        if (defaultKeyHZ.Length > 0)
        {
          double.TryParse(xmlreader.GetValueAsString("general", defaultKeyHZ, ""), NumberStyles.AllowDecimalPoint, provider, out defaultHZ);
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
      if (_refreshrateChangePending) return;

      bool isTV = MediaPortal.Util.Utils.IsLiveTv(strFile);
      bool isDVD = MediaPortal.Util.Utils.IsDVD(strFile);
      bool isVideo = MediaPortal.Util.Utils.IsVideo(strFile);
      bool IsAVStream = MediaPortal.Util.Utils.IsAVStream(strFile); //rtsp users for live TV and recordings.


      if (!isTV && !isDVD && !isVideo)
      {
        return;
      }

      bool enabled = false;
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";
      bool deviceReset = false;
      bool force_refresh_rate = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        enabled = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);

        if (!enabled)
        {
          Log.Info("RefreshRateChanger.AdaptRefreshRate: 'auto refreshrate changer' disabled");
          return;
        }

        deviceReset = xmlreader.GetValueAsBool("general", "devicereset", false);
        force_refresh_rate = xmlreader.GetValueAsBool("general", "force_refresh_rate", false); ;
      }
      double[] tvFPS = RetriveRefreshRateChangerSettings("tv_fps");
      double fps = -1;

      if ((isVideo || isDVD) && (!IsAVStream && !isTV))
      {
        MediaInfo mI = null;
        try
        {
          mI = new MediaInfo();
          mI.Open(strFile);
          double.TryParse(mI.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, provider, out fps);
        }
        catch (Exception ex)
        {
          Log.Error("RefreshRateChanger.AdaptRefreshRate: unable to call external DLL - medialib info (make sure 'MediaInfo.dll' is located in MP root dir.) {0}", ex.Message);
        }
        finally
        {
          if (mI != null)
          {
            mI.Close();
          }
        }
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
      set { _refreshrateChangePending = value;}
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

    #endregion

  }
}
