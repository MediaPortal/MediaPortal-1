namespace CybrDisplayPlugin
{
  using CybrDisplayPlugin.Setting;
  using DShowNET.AudioMixer;
  using MediaPortal.Configuration;
  using MediaPortal.Dialogs;
  using MediaPortal.GUI.Library;
  using MediaPortal.Player;
  using MediaPortal.Profile;
  using MediaPortal.TV.Recording;
  using Microsoft.Win32;
  using System;
  using System.Collections;
  using System.IO;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using System.Threading;
  using System.Windows.Forms;
  using Un4seen.Bass;

  [PluginIcons("CybrDisplayPlugin.lcd.gif", "CybrDisplayPlugin.lcd_deactivated.gif")]
  public class CybrDisplay : IPlugin, ISetupForm
  {
    private static int _IdleTimeout = 5;
    public static bool _PropertyBrowserAvailable = false;
    private GUIWindow BackLightWindow = new GUI_SettingsBacklight();
    private PropertyBrowser browser;
    private bool ControlScreenSaver;
    private IDisplay display;
    private GUIWindow DisplayControlWindow = new GUI_SettingsDisplayControl();
    private GUIWindow DisplayOptionsWindow = new GUI_SettingsDisplayOptions();
    private GUIWindow EqualizerWindow = new GUI_SettingsEqualizer();
    private DisplayHandler handler;
    private GUIWindow KeyPadWindow = new GUI_SettingsKeyPad();
    private DateTime lastAction = DateTime.MinValue;
    private static SystemStatus MPStatus;
    public static object PropertyBrowserMutex = new object();
    private GUIWindow RemoteControlWindow = new GUI_SettingsRemote();
    private bool ScreenSaverActive;
    private bool ScreenSaverActiveAtStart;
    private int ScreenSaverTimeOut;
    private GUIWindow SetupWindow = new GUI_SettingsMain();
    private CybrDisplayPlugin.Status status;
    private static object StatusMutex = new object();
    private bool stopRequested;
    private Thread t;
    private object ThreadAccessMutex = new object();
    private static bool UseTVServer = false;

    public CybrDisplay()
    {
    }

    public string Author()
    {
      return "CybrMage";
    }

    private void browser_Closing(object sender, FormClosingEventArgs e)
    {
      if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
      {
        Log.Info("CybrDisplay.browser_Closing(): PropertyBrowser is closing.", new object[0]);
      }
      this.browser = null;
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public string Description()
    {
      return "Shows current status information on an External VFD/LCD display";
    }

    public static void DisablePropertyBrowser()
    {
      lock (PropertyBrowserMutex)
      {
        _PropertyBrowserAvailable = false;
      }
    }

    private void DoStart()
    {
      if ((this.t == null) || !this.t.IsAlive)
      {
        try
        {
          this.display = CybrDisplayPlugin.Settings.Instance.LCDType;
          if (this.display == null)
          {
            Log.Info("CybrDisplay.DoStart(): Internal display type not found.  Plugin not started!!!", new object[0]);
            return;
          }
          Log.Info("CybrDisplay.DoStart(): Starting background thread", new object[0]);
          this.stopRequested = false;
          this.t = new Thread(new ThreadStart(this.Run));
          this.t.Priority = ThreadPriority.Lowest;
          this.t.Name = "CybrDisplay";
          this.t.TrySetApartmentState(ApartmentState.MTA);
          this.t.Start();
          GUIWindowManager.OnNewAction += new OnActionHandler(this.GUIWindowManager_OnNewAction);
          Thread.Sleep(100);
          if (!this.t.IsAlive)
          {
            Log.Info("CybrDisplay.DoStart(): ERROR - backgrund thread NOT STARTED", new object[0]);
          }
        }
        catch (Exception exception)
        {
          Log.Info("CybrDisplay.DoStart: Exception while starting plugin: " + exception.Message, new object[0]);
          if ((this.t != null) && this.t.IsAlive)
          {
            this.t.Abort();
          }
          this.t = null;
        }
        Log.Info("CybrDisplay.DoStart(): Completed", new object[0]);
      }
    }

    private void DoStop()
    {
      Log.Info("CybrDisplay.DoStop(): Called.", new object[0]);
      try
      {
        if ((this.t == null) || !this.t.IsAlive)
        {
          Log.Info("CybrDisplay.DoStop(): ERROR - background thread not running.", new object[0]);
        }
        else
        {
          Log.Info("CybrDisplay.DoStop(): waiting for background thread access.", new object[0]);
          lock (this.ThreadAccessMutex)
          {
            this.stopRequested = true;
          }
          Log.Info("CybrDisplay.DoStop(): Requesting background thread to stop.", new object[0]);
          DateTime time = DateTime.Now.AddSeconds(5.0);
          while (this.t.IsAlive && (DateTime.Now.Ticks < time.Ticks))
          {
            if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
            {
              Log.Info("CybrDisplay.DoStop: Background thread still alive, waiting 100ms...", new object[0]);
            }
            Thread.Sleep(100);
          }
          if (DateTime.Now.Ticks > time.Ticks)
          {
            this.t.Abort();
            Thread.Sleep(100);
            Log.Info("CybrDisplay.DoStop(): Forcing display thread shutdown. t.IsAlive = {0}", new object[] { this.t.IsAlive });
          }
          if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
          {
            Log.Info("CybrDisplay.DoStop(): Background thread has stopped.", new object[0]);
          }
          this.t = null;
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    private void DoWork()
    {
      bool flag = false;
      try
      {
        if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("CybrDisplay Processing status.", new object[0]);
        }
        GUIWindow.Window activeWindow = (GUIWindow.Window)GUIWindowManager.ActiveWindow;
        if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("Active window is {0}", new object[] { activeWindow.ToString() });
        }
        this.status = CybrDisplayPlugin.Status.Idle;
        if (g_Player.Player != null)
        {
          if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
          {
            Log.Debug("Active player detected", new object[0]);
          }
          GUIPropertyManager.SetProperty("#paused", g_Player.Paused ? "true" : string.Empty);
          if (g_Player.IsDVD)
          {
            this.status = CybrDisplayPlugin.Status.PlayingDVD;
            flag = true;
          }
          else if (g_Player.IsRadio)
          {
            this.status = CybrDisplayPlugin.Status.PlayingRadio;
          }
          else if (g_Player.IsMusic)
          {
            this.status = CybrDisplayPlugin.Status.PlayingMusic;
          }
          else if (g_Player.IsTimeShifting)
          {
            this.status = CybrDisplayPlugin.Status.Timeshifting;
            flag = true;
          }
          else if (g_Player.IsTVRecording)
          {
            this.status = CybrDisplayPlugin.Status.PlayingRecording;
            flag = true;
          }
          else if (g_Player.IsTV)
          {
            this.status = CybrDisplayPlugin.Status.PlayingTV;
            flag = true;
          }
          else if (g_Player.IsVideo)
          {
            this.status = CybrDisplayPlugin.Status.PlayingVideo;
            flag = true;
          }
          if (this.ControlScreenSaver && flag)
          {
            Win32Functions.DisableScreenSaver();
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#paused", string.Empty);
          if (this.IsTVWindow((int)activeWindow))
          {
            this.status = CybrDisplayPlugin.Status.PlayingTV;
          }
          if (this.ControlScreenSaver)
          {
            Win32Functions.EnableScreenSaver();
          }
        }
        if ((DateTime.Now - this.lastAction) < new TimeSpan(0, 0, _IdleTimeout))
        {
          this.status = CybrDisplayPlugin.Status.Action;
        }
        if (GUIWindowManager.IsRouted)
        {
          string dialogTitle = string.Empty;
          string dialogHighlightedItem = string.Empty;
          GUIWindow.Window activeWindowEx = (GUIWindow.Window)GUIWindowManager.ActiveWindowEx;
          if (this.GetDialogInfo(activeWindowEx, ref dialogTitle, ref dialogHighlightedItem))
          {
            this.status = CybrDisplayPlugin.Status.Dialog;
            GUIPropertyManager.GetProperty("#currentmodule");
            GUIPropertyManager.SetProperty("#DialogLabel", dialogTitle);
            GUIPropertyManager.SetProperty("#DialogItem", dialogHighlightedItem);
            if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
            {
              Log.Debug("DIALOG window is {0}: \"{1}\", \"{2}\"", new object[] { activeWindowEx.ToString(), dialogTitle, dialogHighlightedItem });
            }
          }
        }
        if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("Detected status is {0}", new object[] { this.status.ToString() });
        }
        lock (StatusMutex)
        {
          MPStatus.CurrentPluginStatus = this.status;
          MPStatus.MP_Is_Idle = false;
          if (this.status.Equals(CybrDisplayPlugin.Status.Idle))
          {
            MPStatus.MP_Is_Idle = true;
          }
          MPStatus.CurrentIconMask = SetPluginIcons();
          if (this.status.Equals(CybrDisplayPlugin.Status.PlayingDVD))
          {
            MPStatus.Media_IsDVD = true;
          }
          if (this.status.Equals(CybrDisplayPlugin.Status.PlayingRadio))
          {
            MPStatus.Media_IsRadio = true;
          }
          if (this.status.Equals(CybrDisplayPlugin.Status.PlayingMusic))
          {
            MPStatus.Media_IsMusic = true;
          }
          if (this.status.Equals(CybrDisplayPlugin.Status.PlayingRecording))
          {
            MPStatus.Media_IsTVRecording = true;
          }
          if (this.status.Equals(CybrDisplayPlugin.Status.PlayingTV))
          {
            MPStatus.Media_IsTV = true;
          }
          if (this.status.Equals(CybrDisplayPlugin.Status.Timeshifting))
          {
            MPStatus.Media_IsTVRecording = true;
          }
          if (this.status.Equals(CybrDisplayPlugin.Status.PlayingVideo))
          {
            MPStatus.Media_IsVideo = true;
          }
          ShowSystemStatus(ref MPStatus);
        }
        lock (PropertyBrowserMutex)
        {
          if (((this.browser != null) && !this.browser.IsDisposed) && _PropertyBrowserAvailable)
          {
            if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
            {
              Log.Info("CybrDisplayPlugin.DoWork(): Updating PropertyBrowser.", new object[0]);
            }
            this.browser.SetStatus(this.status);
            this.browser.SetActiveWindow(activeWindow);
          }
        }
        foreach (CybrDisplayPlugin.Setting.Message message in CybrDisplayPlugin.Settings.Instance.Messages)
        {
          if (((message.Status == CybrDisplayPlugin.Status.Any) || (message.Status == this.status)) && ((message.Windows.Count == 0) || message.Windows.Contains((int)activeWindow)))
          {
            if (!message.Process(this.handler))
            {
            }
            return;
          }
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public bool GetDialogInfo(GUIWindow.Window dialogWindow, ref string DialogTitle, ref string DialogHighlightedItem)
    {
      GUIListControl control = null;
      bool focus = false;
      switch (dialogWindow)
      {
        case GUIWindow.Window.WINDOW_DIALOG_YES_NO:
          {
            GUIDialogYesNo window = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)dialogWindow);
            DialogTitle = string.Empty;
            foreach (object obj16 in window.controlList)
            {
              if (obj16.GetType() == typeof(GUIFadeLabel))
              {
                GUIFadeLabel label3 = obj16 as GUIFadeLabel;
                if (DialogTitle == string.Empty)
                {
                  if (label3.Label != string.Empty)
                  {
                    DialogTitle = label3.Label;
                  }
                }
                else if (label3.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + label3.Label;
                }
              }
              if (obj16.GetType() == typeof(GUILabelControl))
              {
                GUILabelControl control14 = obj16 as GUILabelControl;
                if (DialogTitle == string.Empty)
                {
                  if (control14.Label != string.Empty)
                  {
                    DialogTitle = control14.Label;
                  }
                }
                else if (control14.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + control14.Label;
                }
              }
              if (obj16.GetType() == typeof(GUIButtonControl))
              {
                GUIButtonControl control15 = obj16 as GUIButtonControl;
                if (!control15.Focus)
                {
                  continue;
                }
                DialogHighlightedItem = control15.Description;
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_PROGRESS:
          {
            GUIDialogProgress progress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)dialogWindow);
            foreach (object obj6 in progress.controlList)
            {
              if (obj6.GetType() == typeof(GUILabelControl))
              {
                GUILabelControl control6 = obj6 as GUILabelControl;
                if (control6.GetID == 1)
                {
                  DialogTitle = control6.Label;
                }
              }
            }
            foreach (object obj7 in progress.controlList)
            {
              if (obj7.GetType() == typeof(GUIProgressControl))
              {
                GUIProgressControl control7 = obj7 as GUIProgressControl;
                DialogHighlightedItem = "Progress: " + control7.Percentage.ToString() + "%";
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_SELECT:
          {
            GUIDialogSelect select = (GUIDialogSelect)GUIWindowManager.GetWindow((int)dialogWindow);
            control = null;
            focus = false;
            foreach (object obj9 in select.controlList)
            {
              if (obj9.GetType() == typeof(GUIListControl))
              {
                control = obj9 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string strLabel = string.Empty;
              string str5 = string.Empty;
              string strThumb = string.Empty;
              control.GetSelectedItem(ref strLabel, ref str5, ref strThumb);
              DialogHighlightedItem = strLabel;
            }
            else
            {
              foreach (object obj10 in select.controlList)
              {
                if (obj10.GetType() == typeof(GUIButtonControl))
                {
                  GUIButtonControl control10 = obj10 as GUIButtonControl;
                  if (control10.Focus)
                  {
                    DialogHighlightedItem = control10.Description;
                  }
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_OK:
          {
            GUIDialogOK gok = (GUIDialogOK)GUIWindowManager.GetWindow((int)dialogWindow);
            foreach (object obj5 in gok.controlList)
            {
              if (obj5.GetType() == typeof(GUIButtonControl))
              {
                GUIButtonControl control4 = obj5 as GUIButtonControl;
                if (control4.Focus)
                {
                  DialogHighlightedItem = control4.Description;
                  if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
                  {
                    Log.Info("CybrDisplay.GetDialogInfo(): found WINDOW_DIALOG_OK buttoncontrol ID = {0} Label = \"{1}\" Desc = \"{2}\"", new object[] { control4.GetID, control4.Label, control4.Description });
                  }
                }
              }
              if (obj5.GetType() == typeof(GUIFadeLabel))
              {
                GUIFadeLabel label = obj5 as GUIFadeLabel;
                if (DialogTitle == string.Empty)
                {
                  if (label.Label != string.Empty)
                  {
                    DialogTitle = label.Label;
                  }
                }
                else if (label.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + label.Label;
                }
              }
              if (obj5.GetType() == typeof(GUILabelControl))
              {
                GUILabelControl control5 = obj5 as GUILabelControl;
                if (DialogTitle == string.Empty)
                {
                  if (control5.Label != string.Empty)
                  {
                    DialogTitle = control5.Label;
                  }
                  continue;
                }
                if (control5.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + control5.Label;
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_SELECT2:
          {
            GUIDialogSelect2 select2 = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)dialogWindow);
            control = null;
            focus = false;
            foreach (object obj11 in select2.controlList)
            {
              if (obj11.GetType() == typeof(GUIListControl))
              {
                control = obj11 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string str7 = string.Empty;
              string str8 = string.Empty;
              string str9 = string.Empty;
              control.GetSelectedItem(ref str7, ref str8, ref str9);
              DialogHighlightedItem = str7;
            }
            else
            {
              foreach (object obj12 in select2.controlList)
              {
                if (obj12.GetType() == typeof(GUIButtonControl))
                {
                  GUIButtonControl control11 = obj12 as GUIButtonControl;
                  if (control11.Focus)
                  {
                    DialogHighlightedItem = control11.Description;
                  }
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_MENU:
          {
            GUIDialogMenu menu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)dialogWindow);
            foreach (object obj13 in menu.controlList)
            {
              if (obj13.GetType() == typeof(GUILabelControl))
              {
                GUILabelControl control12 = obj13 as GUILabelControl;
                if (!control12.Label.Trim().ToLower().Equals("menu"))
                {
                  DialogTitle = control12.Label;
                }
              }
            }
            control = null;
            focus = false;
            foreach (object obj14 in menu.controlList)
            {
              if (obj14.GetType() == typeof(GUIListControl))
              {
                control = obj14 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string str10 = string.Empty;
              string str11 = string.Empty;
              string str12 = string.Empty;
              control.GetSelectedItem(ref str10, ref str11, ref str12);
              DialogHighlightedItem = str10;
            }
            else
            {
              foreach (object obj15 in menu.controlList)
              {
                if (obj15.GetType() == typeof(GUIButtonControl))
                {
                  GUIButtonControl control13 = obj15 as GUIButtonControl;
                  if (control13.Focus)
                  {
                    DialogHighlightedItem = control13.Description;
                  }
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_RATING:
          {
            GUIDialogSetRating rating = (GUIDialogSetRating)GUIWindowManager.GetWindow((int)dialogWindow);
            DialogTitle = string.Empty;
            foreach (object obj8 in rating.controlList)
            {
              if (obj8.GetType() == typeof(GUIFadeLabel))
              {
                GUIFadeLabel label2 = obj8 as GUIFadeLabel;
                if (DialogTitle == string.Empty)
                {
                  if (label2.Label != string.Empty)
                  {
                    DialogTitle = label2.Label;
                  }
                }
                else if (label2.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + label2.Label;
                }
              }
              if (obj8.GetType() == typeof(GUILabelControl))
              {
                GUILabelControl control8 = obj8 as GUILabelControl;
                if (DialogTitle == string.Empty)
                {
                  if (control8.Label != string.Empty)
                  {
                    DialogTitle = control8.Label;
                  }
                }
                else if (control8.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + control8.Label;
                }
              }
              if (obj8.GetType() == typeof(GUIButtonControl))
              {
                GUIButtonControl control9 = obj8 as GUIButtonControl;
                if (!control9.Focus)
                {
                  continue;
                }
                DialogHighlightedItem = control9.Description;
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT:
          {
            GUIDialogMenuBottomRight right = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)dialogWindow);
            DialogTitle = string.Empty;
            foreach (object obj2 in right.controlList)
            {
              if (obj2.GetType() == typeof(GUILabelControl))
              {
                GUILabelControl control2 = obj2 as GUILabelControl;
                if (!control2.Label.Trim().ToLower().Equals("menu") && (control2.Label != string.Empty))
                {
                  if (DialogTitle == string.Empty)
                  {
                    DialogTitle = control2.Label;
                  }
                  else
                  {
                    DialogTitle = DialogTitle + " - " + control2.Label;
                  }
                }
              }
            }
            control = null;
            focus = false;
            foreach (object obj3 in right.controlList)
            {
              if (obj3.GetType() == typeof(GUIListControl))
              {
                control = obj3 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string str = string.Empty;
              string str2 = string.Empty;
              string str3 = string.Empty;
              control.GetSelectedItem(ref str, ref str2, ref str3);
              DialogHighlightedItem = str;
            }
            else
            {
              foreach (object obj4 in right.controlList)
              {
                if (obj4.GetType() == typeof(GUIButtonControl))
                {
                  GUIButtonControl control3 = obj4 as GUIButtonControl;
                  if (control3.Focus)
                  {
                    DialogHighlightedItem = control3.Description;
                  }
                }
              }
            }
            return true;
          }
      }
      return false;
    }

    internal static bool GetEQ(ref EQControl EQSETTINGS)
    {
      bool extensiveLogging = CybrDisplayPlugin.Settings.Instance.ExtensiveLogging;
      bool flag2 = (EQSETTINGS.UseStereoEq | EQSETTINGS.UseVUmeter) | EQSETTINGS.UseVUmeter2;
      if (g_Player.Player != null)
      {
        if (!EQSETTINGS.UseEqDisplay)
        {
          return false;
        }
        if (EQSETTINGS._AudioUseASIO)
        {
          return false;
        }
        try
        {
          if (EQSETTINGS.DelayEQ & (g_Player.CurrentPosition < EQSETTINGS._DelayEQTime))
          {
            EQSETTINGS._EQDisplayTitle = false;
            EQSETTINGS._LastEQTitle = g_Player.CurrentPosition;
            return false;
          }
          if (EQSETTINGS.EQTitleDisplay)
          {
            if (g_Player.CurrentPosition < EQSETTINGS._EQTitleDisplayTime)
            {
              EQSETTINGS._EQDisplayTitle = false;
            }
            if ((g_Player.CurrentPosition - EQSETTINGS._LastEQTitle) > EQSETTINGS._EQTitleDisplayTime)
            {
              EQSETTINGS._LastEQTitle = g_Player.CurrentPosition;
              EQSETTINGS._EQDisplayTitle = !EQSETTINGS._EQDisplayTitle;
            }
            if (EQSETTINGS._EQDisplayTitle & ((g_Player.CurrentPosition - EQSETTINGS._LastEQTitle) < EQSETTINGS._EQTitleShowTime))
            {
              return false;
            }
          }
        }
        catch
        {
          EQSETTINGS._EQDisplayTitle = false;
          EQSETTINGS._LastEQTitle = g_Player.CurrentPosition;
          return false;
        }
        int handle = -1;
        try
        {
          handle = g_Player.Player.CurrentAudioStream;
        }
        catch (Exception exception)
        {
          Log.Debug("CybrDisplay.GetEQ(): Caugth exception obtaining audio stream: {0}", new object[] { exception });
          return false;
        }
        if ((handle != 0) & (handle != -1))
        {
          int num2;
          if (extensiveLogging)
          {
            Log.Info("CybrDisplay.GetEQ(): attempting to retrieve equalizer data from audio stream {0}", new object[] { handle });
          }
          try
          {
            int num3;
            if (flag2)
            {
              num3 = -2147483630;
            }
            else
            {
              num3 = -2147483646;
            }
            num2 = Un4seen.Bass.Bass.BASS_ChannelGetData(handle, ref EQSETTINGS.EqFftData[0], num3);
          }
          catch
          {
            if (extensiveLogging)
            {
              Log.Info("CybrDisplay.GetEQ(): CAUGHT EXCeption - audio stream {0} disappeared", new object[] { handle });
            }
            return false;
          }
          if (num2 > 0)
          {
            return true;
          }
          if (extensiveLogging)
          {
            Log.Info("CybrDisplay.GetEQ(): unable to retreive equalizer data", new object[0]);
          }
          return false;
        }
        if (extensiveLogging)
        {
          Log.Info("CybrDisplay.GetEQ(): Audio Stream not available", new object[0]);
        }
      }
      return false;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      string str;
      strButtonText = "CybrDisplay Setup";
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      if (Assembly.GetEntryAssembly().FullName.Contains("Configuration") | !File.Exists(Config.GetFile(Config.Dir.Config, "CybrDisplay.xml")))
      {
        return false;
      }
      if (((str = CybrDisplayPlugin.Settings.Instance.Type) == null) || (((str != "iMONLCDg") && (str != "MatrixMX")) && ((str != "MatrixGX") && (str != "VLSYS_Mplay"))))
      {
        return false;
      }
      if (CybrDisplayPlugin.Settings.Instance.DisableGUISetup)
      {
        return false;
      }
      return true;
    }

    internal static void GetSystemStatus(ref SystemStatus CurrentStatus)
    {
      lock (StatusMutex)
      {
        CurrentStatus.CurrentPluginStatus = MPStatus.CurrentPluginStatus;
        CurrentStatus.CurrentIconMask = MPStatus.CurrentIconMask;
        CurrentStatus.MP_Is_Idle = MPStatus.MP_Is_Idle;
        GetSystemVolume(ref CurrentStatus);
        CurrentStatus.MediaPlayer_Active = MPStatus.MediaPlayer_Active;
        CurrentStatus.MediaPlayer_Playing = MPStatus.MediaPlayer_Playing;
        CurrentStatus.MediaPlayer_Paused = MPStatus.MediaPlayer_Paused;
        CurrentStatus.Media_IsRecording = MPStatus.Media_IsRecording;
        CurrentStatus.Media_IsTV = MPStatus.Media_IsTV;
        CurrentStatus.Media_IsTVRecording = MPStatus.Media_IsTVRecording;
        CurrentStatus.Media_IsDVD = MPStatus.Media_IsDVD;
        CurrentStatus.Media_IsCD = MPStatus.Media_IsCD;
        CurrentStatus.Media_IsRadio = MPStatus.Media_IsRadio;
        CurrentStatus.Media_IsVideo = MPStatus.Media_IsVideo;
        CurrentStatus.Media_IsMusic = MPStatus.Media_IsMusic;
        CurrentStatus.Media_CurrentPosition = g_Player.CurrentPosition;
        CurrentStatus.Media_Duration = g_Player.Duration;
        CurrentStatus.Media_Speed = g_Player.Speed;
      }
    }

    internal static void GetSystemVolume(ref SystemStatus CurrentStatus)
    {
      CurrentStatus.SystemVolumeLevel = -1;
      try
      {
        if (!CurrentStatus.IsMuted)
        {
          try
          {
            CurrentStatus.SystemVolumeLevel = AudioMixerHelper.GetVolume();
          }
          catch
          {
          }
          if (CurrentStatus.SystemVolumeLevel < 0)
          {
            try
            {
              CurrentStatus.SystemVolumeLevel = VolumeHandler.Instance.Volume;
            }
            catch
            {
            }
          }
          if (CurrentStatus.SystemVolumeLevel >= 0)
          {
            return;
          }
          try
          {
            CurrentStatus.SystemVolumeLevel = g_Player.Volume;
            return;
          }
          catch
          {
            CurrentStatus.SystemVolumeLevel = 0;
            return;
          }
        }
        CurrentStatus.SystemVolumeLevel = 0;
      }
      catch
      {
        CurrentStatus.SystemVolumeLevel = 0;
        CurrentStatus.IsMuted = false;
      }
    }

    private void GetTVSource()
    {
      UseTVServer = false;
      if ((File.Exists(Config.GetFolder(Config.Dir.Base) + @"\TvControl.dll") && File.Exists(Config.GetFolder(Config.Dir.Base) + @"\TvLibrary.Interfaces.dll")) && File.Exists(Config.GetFolder(Config.Dir.Plugins) + @"\Windows\TvPlugin.dll"))
      {
        using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          if (settings.GetValueAsString("tvservice", "hostname", "") != string.Empty)
          {
            Log.Info("CybrDisplay.GetTVSource(): Found configured TVServer installation", new object[0]);
            UseTVServer = true;
          }
          else
          {
            Log.Info("CybrDisplay.GetTVSource(): Found TVServer installation", new object[0]);
          }
        }
      }
    }

    public int GetWindowId()
    {
      return 0x4da6; //????
    }

    private void GUIWindowManager_OnNewAction(Action action)
    {
      this.lastAction = DateTime.Now;
    }

    public bool HasSetup()
    {
      return true;
    }

    internal static void InitDisplayControl(ref DisplayControl DisplaySettings)
    {
      DisplaySettings._Shutdown1 = string.Empty;
      DisplaySettings._Shutdown2 = string.Empty;
      DisplaySettings.BlankDisplayWithVideo = false;
      DisplaySettings.EnableDisplayAction = false;
      DisplaySettings.BlankDisplayWhenIdle = false;
      DisplaySettings._BlankIdleTime = 0L;
      DisplaySettings._BlankIdleTimeout = 0L;
      DisplaySettings._DisplayControlAction = false;
      DisplaySettings._DisplayControlLastAction = 0L;
      DisplaySettings.DisplayActionTime = 0;
      DisplaySettings._DisplayControlTimeout = 0L;
    }

    internal static void InitDisplayOptions(ref DisplayOptions DisplayOptions)
    {
      DisplayOptions.DiskIcon = false;
      DisplayOptions.VolumeDisplay = false;
      DisplayOptions.ProgressDisplay = false;
      DisplayOptions.DiskMediaStatus = true;
      DisplayOptions.DiskMonitor = false;
      DisplayOptions.UseCustomFont = false;
      DisplayOptions.UseLargeIcons = false;
      DisplayOptions.UseCustomIcons = false;
      DisplayOptions.UseInvertedIcons = false;
    }

    internal static void InitEQ(ref EQControl EQSettings)
    {
      EQSettings.UseEqDisplay = false;
      EQSettings.UseNormalEq = true;
      EQSettings.UseStereoEq = false;
      EQSettings.UseVUmeter = false;
      EQSettings.UseVUmeter2 = false;
      EQSettings._useVUindicators = false;
      EQSettings._useEqMode = 0;
      EQSettings.RestrictEQ = false;
      EQSettings.SmoothEQ = false;
      EQSettings.DelayEQ = true;
      EQSettings.EQTitleDisplay = false;
      EQSettings._EqDataAvailable = false;
      EQSettings.EqFftData = new float[0x800];
      EQSettings.EqArray = new byte[0x11];
      EQSettings.LastEQ = new int[0x11];
      EQSettings._EQTitleDisplayTime = 10;
      EQSettings._EQTitleShowTime = 2;
      EQSettings._LastEQTitle = 0.0;
      EQSettings._EQDisplayTitle = false;
      EQSettings._Max_EQ_FPS = 0;
      EQSettings._EQ_Framecount = 0;
      EQSettings._EQ_Restrict_FPS = 10;
      EQSettings._EqUpdateDelay = 0;
      EQSettings._DelayEQTime = 0;
      using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        EQSettings._AudioIsMixing = settings.GetValueAsBool("audioplayer", "mixing", false);
        EQSettings._AudioUseASIO = settings.GetValueAsBool("audioplayer", "asio", false);
      }
    }

    internal static void InitSystemStatus(ref SystemStatus CurrentStatus)
    {
      lock (StatusMutex)
      {
        using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          CurrentStatus._AudioIsMixing = settings.GetValueAsBool("audioplayer", "mixing", false);
          CurrentStatus._AudioUseASIO = settings.GetValueAsBool("audioplayer", "asio", false);
          bool flag = settings.GetValueAsBool("volume", "digital", true);
          CurrentStatus._AudioUseMasterVolume = !flag;
          CurrentStatus._AudioUseWaveVolume = flag;
        }
        CurrentStatus.CurrentIconMask = 0L;
        CurrentStatus.MP_Is_Idle = false;
        CurrentStatus.SystemVolumeLevel = 0;
        CurrentStatus.IsMuted = false;
        CurrentStatus.MediaPlayer_Active = false;
        CurrentStatus.MediaPlayer_Playing = false;
        CurrentStatus.MediaPlayer_Paused = false;
        CurrentStatus.Media_IsRecording = false;
        CurrentStatus.Media_IsTV = false;
        CurrentStatus.Media_IsTVRecording = false;
        CurrentStatus.Media_IsDVD = false;
        CurrentStatus.Media_IsCD = false;
        CurrentStatus.Media_IsRadio = false;
        CurrentStatus.Media_IsVideo = false;
        CurrentStatus.Media_IsMusic = false;
        CurrentStatus.Media_IsTimeshifting = false;
        CurrentStatus.Media_CurrentPosition = 0.0;
        CurrentStatus.Media_Duration = 0.0;
        CurrentStatus.Media_Speed = 0;
      }
    }

    public static bool IsCaptureCardRecording()
    {
      if (UseTVServer)
      {
        return (bool)DynaInvoke.InvokeMethod(Config.GetFolder(Config.Dir.Base) + @"\TvControl.dll", "TvServer", "IsAnyCardRecording", null);
      }
      return Recorder.IsAnyCardRecording();
    }

    public static bool IsCaptureCardViewing()
    {
      if (UseTVServer)
      {
        bool flag = (bool)DynaInvoke.InvokeMethod(Config.GetFolder(Config.Dir.Base) + @"\TvControl.dll", "TvServer", "IsAnyCardRecording", null);
        return (((flag | Recorder.IsViewing()) | Recorder.IsTimeShifting()) | Recorder.IsRadio());
      }
      return ((Recorder.IsViewing() | Recorder.IsTimeShifting()) | Recorder.IsRadio());
    }

    private bool IsTVWindow(int windowId)
    {
      return ((windowId == 1) || ((windowId == 0x25a) || ((windowId == 600) || ((windowId == 0x25b) || ((windowId == 0x25e) || ((windowId == 0x25d) || ((windowId == 0x259) || ((windowId == 0x25c) || ((windowId == 0x1e14) || ((windowId == 0x1e15) || ((windowId == 0x25f) || ((windowId == 0x260) || ((windowId == 0x261) || ((windowId == 0x263) || ((windowId == 0x264) || ((windowId == 0x265) || ((windowId == 610) || ((windowId == 0x2ed) || (windowId == 0x2ec)))))))))))))))))));
    }

    private void LoadSetupWindows()
    {
      if (CybrDisplayPlugin.Settings.Instance.DisableGUISetup)
      {
        Log.Info("CybrDisplay.LoadSetupWindows(): Plugin setup explicitly disables GUI Setup.", new object[0]);
      }
      else
      {
        Log.Info("CybrDisplay.LoadSetupWindows(): called", new object[0]);
        this.SetupWindow.Init();
        this.EqualizerWindow.Init();
        this.DisplayOptionsWindow.Init();
        this.DisplayControlWindow.Init();
        this.RemoteControlWindow.Init();
        this.KeyPadWindow.Init();
        this.BackLightWindow.Init();
        GUIWindowManager.Add(ref this.SetupWindow);
        GUIWindowManager.Add(ref this.EqualizerWindow);
        GUIWindowManager.Add(ref this.DisplayOptionsWindow);
        GUIWindowManager.Add(ref this.DisplayControlWindow);
        GUIWindowManager.Add(ref this.RemoteControlWindow);
        GUIWindowManager.Add(ref this.KeyPadWindow);
        GUIWindowManager.Add(ref this.BackLightWindow);
        this.UnloadSetupWindows();
      }
    }

    public static bool Player_Playing()
    {
      return g_Player.Playing;
    }

    public static string PluginIconsToAudioFormat(ulong IconMask)
    {
      string str = string.Empty;
      if ((IconMask & ((ulong)0x8000000000L)) > 0L)
      {
        str = str + " ICON_WMA2";
      }
      if ((IconMask & ((ulong)0x4000000000L)) > 0L)
      {
        str = str + " ICON_WAV";
      }
      if ((IconMask & ((ulong)0x4000000L)) > 0L)
      {
        str = str + " ICON_WMA";
      }
      if ((IconMask & ((ulong)0x2000000L)) > 0L)
      {
        str = str + " ICON_MP3";
      }
      if ((IconMask & ((ulong)0x1000000L)) > 0L)
      {
        str = str + " ICON_OGG";
      }
      return str.Trim();
    }

    public static string PluginIconsToString(ulong IconMask)
    {
      string str = string.Empty;
      if ((IconMask & ((ulong)0x100000000000L)) > 0L)
      {
        str = str + " ICON_Play";
      }
      if ((IconMask & ((ulong)0x80000000000L)) > 0L)
      {
        str = str + " ICON_Pause";
      }
      if ((IconMask & ((ulong)0x40000000000L)) > 0L)
      {
        str = str + " ICON_Stop";
      }
      if ((IconMask & ((ulong)0x20000000000L)) > 0L)
      {
        str = str + " ICON_FFWD";
      }
      if ((IconMask & ((ulong)0x10000000000L)) > 0L)
      {
        str = str + " ICON_FRWD";
      }
      if ((IconMask & ((ulong)0x400000000L)) > 0L)
      {
        str = str + " ICON_Rec";
      }
      if ((IconMask & ((ulong)0x200000000L)) > 0L)
      {
        str = str + " ICON_Vol";
      }
      if ((IconMask & ((ulong)0x100000000L)) > 0L)
      {
        str = str + " ICON_Time";
      }
      if ((IconMask & ((ulong)0x80L)) > 0L)
      {
        str = str + " ICON_Music";
      }
      if ((IconMask & ((ulong)0x40L)) > 0L)
      {
        str = str + " ICON_Movie";
      }
      if ((IconMask & ((ulong)0x20L)) > 0L)
      {
        str = str + " ICON_Photo";
      }
      if ((IconMask & ((ulong)8L)) > 0L)
      {
        str = str + " ICON_TV";
      }
      if ((IconMask & ((ulong)0x10L)) > 0L)
      {
        str = str + " ICON_CD_DVD";
      }
      if ((IconMask & ((ulong)0x200000L)) > 0L)
      {
        str = str + " ICON_TV_2";
      }
      if ((IconMask & ((ulong)0x100000L)) > 0L)
      {
        str = str + " ICON_HDTV";
      }
      if ((IconMask & ((ulong)0x8000000000L)) > 0L)
      {
        str = str + " ICON_WMA2";
      }
      if ((IconMask & ((ulong)0x4000000000L)) > 0L)
      {
        str = str + " ICON_WAV";
      }
      if ((IconMask & ((ulong)0x4000000L)) > 0L)
      {
        str = str + " ICON_WMA";
      }
      if ((IconMask & ((ulong)0x2000000L)) > 0L)
      {
        str = str + " ICON_MP3";
      }
      if ((IconMask & ((ulong)0x1000000L)) > 0L)
      {
        str = str + " ICON_OGG";
      }
      if ((IconMask & 0x80000000L) > 0L)
      {
        str = str + " ICON_xVid";
      }
      if ((IconMask & ((ulong)0x40000000L)) > 0L)
      {
        str = str + " ICON_WMV";
      }
      if ((IconMask & ((ulong)0x20000000L)) > 0L)
      {
        str = str + " ICON_MPG2";
      }
      if ((IconMask & ((ulong)0x20000L)) > 0L)
      {
        str = str + " ICON_MPG";
      }
      if ((IconMask & ((ulong)0x10000L)) > 0L)
      {
        str = str + " ICON_DivX";
      }
      if ((IconMask & ((ulong)1L)) > 0L)
      {
        str = str + " SPKR_FL";
      }
      if ((IconMask & ((ulong)0x8000L)) > 0L)
      {
        str = str + " SPKR_FC";
      }
      if ((IconMask & ((ulong)0x4000L)) > 0L)
      {
        str = str + " SPKR_FR";
      }
      if ((IconMask & ((ulong)0x400L)) > 0L)
      {
        str = str + " SPKR_RL";
      }
      if ((IconMask & ((ulong)0x100L)) > 0L)
      {
        str = str + " SPKR_RR";
      }
      if ((IconMask & ((ulong)0x2000L)) > 0L)
      {
        str = str + " SPKR_SL";
      }
      if ((IconMask & ((ulong)0x800L)) > 0L)
      {
        str = str + " SPKR_SR";
      }
      if ((IconMask & ((ulong)0x1000L)) > 0L)
      {
        str = str + " SPKR_LFE";
      }
      if ((IconMask & ((ulong)0x200L)) > 0L)
      {
        str = str + " ICON_SPDIF";
      }
      if ((IconMask & ((ulong)0x10000000L)) > 0L)
      {
        str = str + " ICON_AC3";
      }
      if ((IconMask & ((ulong)0x8000000L)) > 0L)
      {
        str = str + " ICON_DTS";
      }
      if ((IconMask & ((ulong)0x2000000000L)) > 0L)
      {
        str = str + " ICON_REF";
      }
      if ((IconMask & ((ulong)0x1000000000L)) > 0L)
      {
        str = str + " ICON_SFL";
      }
      if ((IconMask & ((ulong)0x800000000L)) > 0L)
      {
        str = str + " ICON_Alarm";
      }
      if ((IconMask & ((ulong)0x800000L)) > 0L)
      {
        str = str + " ICON_SRC";
      }
      if ((IconMask & ((ulong)0x400000L)) > 0L)
      {
        str = str + " ICON_FIT";
      }
      if ((IconMask & ((ulong)0x80000L)) > 0L)
      {
        str = str + " ICON_SCR1";
      }
      if ((IconMask & ((ulong)0x40000L)) > 0L)
      {
        str = str + " ICON_SCR2";
      }
      if ((IconMask & ((ulong)4L)) > 0L)
      {
        str = str + " ICON_WebCast";
      }
      if ((IconMask & ((ulong)2L)) > 0L)
      {
        str = str + " ICON_News";
      }
      return str.Trim();
    }

    public static string PluginIconsToVideoFormat(ulong IconMask)
    {
      string str = string.Empty;
      if ((IconMask & 0x80000000L) > 0L)
      {
        str = str + " ICON_xVid";
      }
      if ((IconMask & ((ulong)0x40000000L)) > 0L)
      {
        str = str + " ICON_WMV";
      }
      if ((IconMask & ((ulong)0x20000000L)) > 0L)
      {
        str = str + " ICON_MPG2";
      }
      if ((IconMask & ((ulong)0x20000L)) > 0L)
      {
        str = str + " ICON_MPG";
      }
      if ((IconMask & ((ulong)0x10000L)) > 0L)
      {
        str = str + " ICON_DivX";
      }
      return str.Trim();
    }

    public string PluginName()
    {
      return "CybrDisplay";
    }

    internal static void ProcessEqData(ref EQControl EQSettings)
    {
      bool extensiveLogging = CybrDisplayPlugin.Settings.Instance.ExtensiveLogging;
      if (extensiveLogging)
      {
        Log.Info("CybrDisplay.ProcessEqData(): called... MaxValue = {0}, BANDS = {1}", new object[] { EQSettings.Render_MaxValue, EQSettings.Render_BANDS });
      }
      if ((EQSettings.UseStereoEq || EQSettings.UseVUmeter) || EQSettings.UseVUmeter2)
      {
        if (EQSettings.UseStereoEq)
        {
          int num = EQSettings.Render_MaxValue;
          int num2 = EQSettings.Render_BANDS;
          int num3 = 0;
          for (int i = 0; i < num2; i++)
          {
            float num7 = 0f;
            float num8 = 0f;
            int num10 = (int)Math.Pow(2.0, (i * 10.0) / ((double)(num2 - 1)));
            if (num10 > 0x3ff)
            {
              num10 = 0x3ff;
            }
            if (num10 <= num3)
            {
              num10 = num3 + 1;
            }
            int num9 = (10 + num10) - num3;
            while (num3 < num10)
            {
              num7 += EQSettings.EqFftData[2 + (num3 * 2)];
              num8 += EQSettings.EqFftData[(2 + (num3 * 2)) + 1];
              num3++;
            }
            int num4 = (int)((Math.Sqrt(((double)num7) / Math.Log10((double)num9)) * 1.7) * num);
            int num5 = (int)((Math.Sqrt(((double)num8) / Math.Log10((double)num9)) * 1.7) * num);
            if (extensiveLogging)
            {
              Log.Info("CybrDisplay.ProcessEqData(): Processing StereoEQ band {0}: L = {1}, R = {2}", new object[] { i, num4, num5 });
            }
            num4 = Math.Min(num, num4);
            EQSettings.EqArray[1 + i] = (byte)num4;
            num5 = Math.Min(num, num5);
            EQSettings.EqArray[9 + i] = (byte)num5;
            if (EQSettings.SmoothEQ)
            {
              if (EQSettings.EqArray[1 + i] < EQSettings.LastEQ[1 + i])
              {
                int num11 = EQSettings.LastEQ[1 + i];
                num11 = EQSettings.LastEQ[1 + i] - ((int)0.5);
                if (num11 < 0)
                {
                  num11 = 0;
                }
                EQSettings.EqArray[1 + i] = (byte)num11;
                EQSettings.LastEQ[1 + i] = num11;
              }
              else
              {
                EQSettings.LastEQ[1 + i] = EQSettings.EqArray[1 + i];
              }
              if (EQSettings.EqArray[9 + i] < EQSettings.LastEQ[9 + i])
              {
                int num12 = EQSettings.LastEQ[9 + i];
                num12 = EQSettings.LastEQ[9 + i] - ((int)0.5);
                if (num12 < 0)
                {
                  num12 = 0;
                }
                EQSettings.EqArray[9 + i] = (byte)num12;
                EQSettings.LastEQ[9 + i] = num12;
              }
              else
              {
                EQSettings.LastEQ[9 + i] = EQSettings.EqArray[9 + i];
              }
            }
            if (extensiveLogging)
            {
              Log.Info("CybrDisplay.ProcessEqData.(): Processed StereoEQ mode {0} byte {1}: L = {2}, R = {3}.", new object[] { EQSettings.EqArray[0], i, EQSettings.EqArray[1 + (i * 2)].ToString(), EQSettings.EqArray[2 + (i * 2)].ToString() });
            }
          }
        }
        else
        {
          int num13 = EQSettings.Render_MaxValue;
          int num14 = EQSettings.Render_BANDS;
          int num15 = 0;
          for (int j = 0; j < num14; j++)
          {
            float num19 = 0f;
            float num20 = 0f;
            int num22 = 0x3ff;
            int num21 = (10 + num22) - num15;
            while (num15 < num22)
            {
              if (EQSettings.EqFftData[2 + (num15 * 2)] > num19)
              {
                num19 = EQSettings.EqFftData[2 + (num15 * 2)];
              }
              if (EQSettings.EqFftData[(2 + (num15 * 2)) + 1] > num20)
              {
                num20 = EQSettings.EqFftData[(2 + (num15 * 2)) + 1];
              }
              num15++;
            }
            int num16 = (int)((Math.Sqrt(((double)num19) / Math.Log10((double)num21)) * 1.7) * num13);
            int num17 = (int)((Math.Sqrt(((double)num20) / Math.Log10((double)num21)) * 1.7) * num13);
            if (extensiveLogging)
            {
              Log.Info("CybrDisplay.ProcessEqData(): Processing VUmeter band {0}: L = {1}, R = {2}", new object[] { j, num16, num17 });
            }
            num16 = Math.Min(num13, num16);
            EQSettings.EqArray[1 + (j * 2)] = (byte)num16;
            num17 = Math.Min(num13, num17);
            EQSettings.EqArray[2 + (j * 2)] = (byte)num17;
            if (EQSettings.SmoothEQ)
            {
              if (EQSettings.EqArray[1] < EQSettings.LastEQ[1])
              {
                int num23 = EQSettings.LastEQ[1];
                num23 = EQSettings.LastEQ[1] - ((int)0.5);
                if (num23 < 0)
                {
                  num23 = 0;
                }
                EQSettings.EqArray[1] = (byte)num23;
                EQSettings.LastEQ[1] = num23;
              }
              else
              {
                EQSettings.LastEQ[1] = EQSettings.EqArray[1];
              }
              if (EQSettings.EqArray[2] < EQSettings.LastEQ[2])
              {
                int num24 = EQSettings.LastEQ[2];
                num24 = EQSettings.LastEQ[2] - ((int)0.5);
                if (num24 < 0)
                {
                  num24 = 0;
                }
                EQSettings.EqArray[2] = (byte)num24;
                EQSettings.LastEQ[2] = num24;
              }
              else
              {
                EQSettings.LastEQ[2] = EQSettings.EqArray[2];
              }
            }
            if (extensiveLogging)
            {
              Log.Info("CybrDisplay.ProcessEqData(): Processed VUmeter byte {0}: L = {1}, R = {2}.", new object[] { j, EQSettings.EqArray[1 + (j * 2)].ToString(), EQSettings.EqArray[2 + (j * 2)].ToString() });
            }
          }
        }
      }
      else
      {
        int num25 = EQSettings.Render_MaxValue;
        int num26 = EQSettings.Render_BANDS;
        int num27 = 0;
        for (int k = 0; k < num26; k++)
        {
          float num30 = 0f;
          int num32 = (int)Math.Pow(2.0, (k * 10.0) / ((double)(num26 - 1)));
          if (num32 > 0x3ff)
          {
            num32 = 0x3ff;
          }
          if (num32 <= num27)
          {
            num32 = num27 + 1;
          }
          int num31 = (10 + num32) - num27;
          while (num27 < num32)
          {
            num30 += EQSettings.EqFftData[1 + num27];
            num27++;
          }
          int num28 = (int)((Math.Sqrt(((double)num30) / Math.Log10((double)num31)) * 1.7) * num25);
          if (extensiveLogging)
          {
            Log.Info("CybrDisplay.ProcessEqData(): Processing EQ band {0} = {1}", new object[] { k, num28 });
          }
          num28 = Math.Min(num25, num28);
          EQSettings.EqArray[1 + k] = (byte)num28;
          if (EQSettings.SmoothEQ)
          {
            if (EQSettings.EqArray[1 + k] < EQSettings.LastEQ[1 + k])
            {
              int num33 = EQSettings.LastEQ[1 + k];
              num33 = EQSettings.LastEQ[1 + k] - ((int)0.5);
              if (num33 < 0)
              {
                num33 = 0;
              }
              EQSettings.EqArray[1 + k] = (byte)num33;
              EQSettings.LastEQ[1 + k] = num33;
            }
            else
            {
              EQSettings.LastEQ[1 + k] = EQSettings.EqArray[1 + k];
            }
          }
          if (extensiveLogging)
          {
            Log.Info("CybrDisplay.ProcessEqData(): Processed EQ mode {0} byte {1} = {2}.", new object[] { EQSettings.EqArray[0], k, EQSettings.EqArray[1 + k].ToString() });
          }
        }
      }
      if (extensiveLogging)
      {
        Log.Info("CybrDisplay.ProcessEqData(): called", new object[0]);
      }
    }

    public void Run()
    {
      SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
      this.ControlScreenSaver = CybrDisplayPlugin.Settings.Instance.ControlScreenSaver;
      Log.Info("CybrDisplay.Run(): Control ScreenSaver: {0}", new object[] { this.ControlScreenSaver.ToString() });
      bool extensiveLogging = CybrDisplayPlugin.Settings.Instance.ExtensiveLogging;
      bool flag2 = false;
      if (extensiveLogging)
      {
        Log.Info("CybrDisplay.Run(): Entering CybrDisplay run loop.", new object[0]);
      }
      try
      {
        if (extensiveLogging)
        {
          Log.Info("CybrDisplay.Run(): Creating CybrDisplay displayhandler.", new object[0]);
        }
        this.handler = new DisplayHandler(this.display);
        if (extensiveLogging)
        {
          Log.Info("CybrDisplay.Run(): Starting CybrDisplay displayhandler.", new object[0]);
        }
        this.handler.Start();
        while (!this.stopRequested)
        {
          if (!CybrDisplayPlugin.Settings.Instance.Type.Equals("MCEDisplay"))
          {
            try
            {
              lock (this.ThreadAccessMutex)
              {
                this.DoWork();
              }
            }
            catch (Exception exception)
            {
              Log.Debug("CybrDisplay.Run(): CAUGHT EXCEPTION in DoWork() - {0}", new object[] { exception });
              if (exception.Message.Contains("ThreadAbortException"))
              {
                this.stopRequested = true;
              }
            }
            try
            {
              lock (this.ThreadAccessMutex)
              {
                this.handler.DisplayLines();
              }
            }
            catch (Exception exception2)
            {
              Log.Debug("CybrDisplay.Run(): CAUGHT EXCEPTION in handler.DisplayLines() - {0}", new object[] { exception2 });
              if (exception2.Message.Contains("ThreadAbortException"))
              {
                this.stopRequested = true;
              }
            }
            if (extensiveLogging)
            {
              Log.Debug("CybrDisplay.Run(): CybrDisplay Sleeping...", new object[0]);
            }
            Thread.Sleep(CybrDisplayPlugin.Settings.Instance.ScrollDelay);
            if (extensiveLogging)
            {
              Log.Debug("CybrDisplay.Run(): CybrDisplay Sleeping... DONE", new object[0]);
            }
          }
          else
          {
            Thread.Sleep(100);
          }
        }
        if (extensiveLogging)
        {
          Log.Info("CybrDisplay.Run(): Stopping CybrDisplay displayhandler.", new object[0]);
        }
        flag2 = true;
        this.handler.Stop();
      }
      catch (ThreadAbortException)
      {
        Log.Error("CybrDisplay.Run(): CAUGHT ThreadAbortException", new object[0]);
        if (!flag2)
        {
          this.handler.Stop();
          flag2 = true;
        }
      }
      catch (Exception exception3)
      {
        Log.Error("CybrDisplay.Run(): CAUGHT EXCEPTION: {0}", new object[] { exception3 });
      }
      if (extensiveLogging)
      {
        Log.Info("CybrDisplay.Run(): Exiting CybrDisplay run loop.", new object[0]);
      }
      if (this.ControlScreenSaver & !this.ScreenSaverActive)
      {
        Win32Functions.ScreenSaver.SetScreenSaverActive(true);
      }
    }

    internal static void SetIdleTimeout(int TimeOutSeconds)
    {
      if (TimeOutSeconds == -1)
      {
        _IdleTimeout = 5;
      }
      else
      {
        _IdleTimeout = TimeOutSeconds;
      }
    }

    internal static ulong SetPluginIcons()
    {
      string[] strArray;
      string str3;
      ulong num = 0L;
      string property = string.Empty;
      MPStatus.MediaPlayer_Active = false;
      MPStatus.MediaPlayer_Playing = false;
      MPStatus.MediaPlayer_Paused = false;
      MPStatus.Media_IsCD = false;
      MPStatus.Media_IsRadio = false;
      MPStatus.Media_IsDVD = false;
      MPStatus.Media_IsMusic = false;
      MPStatus.Media_IsRecording = false;
      MPStatus.Media_IsTV = false;
      MPStatus.Media_IsTVRecording = false;
      MPStatus.Media_IsVideo = false;
      if (IsCaptureCardRecording())
      {
        num |= (ulong)0x400000000L;
        MPStatus.Media_IsRecording = true;
      }
      else if (IsCaptureCardViewing())
      {
        num |= (ulong)8L;
        MPStatus.Media_IsTV = true;
      }
      if (g_Player.Player == null)
      {
        return (num | ((ulong)0x40000000000L));
      }
      MPStatus.MediaPlayer_Active = true;
      if (!g_Player.Playing)
      {
        num |= (ulong)0x40000000000L;
        MPStatus.MediaPlayer_Active = false;
        MPStatus.MediaPlayer_Paused = false;
        MPStatus.MediaPlayer_Playing = false;
        return num;
      }
      if (g_Player.Playing & !g_Player.Paused)
      {
        MPStatus.MediaPlayer_Playing = true;
        if (g_Player.Speed > 1)
        {
          num |= (ulong)0x20000000000L;
        }
        else if (g_Player.Speed < 0)
        {
          num |= (ulong)0x10000000000L;
        }
        else
        {
          num |= (ulong)0x100000000000L;
        }
      }
      else
      {
        MPStatus.MediaPlayer_Paused = true;
        num |= (ulong)0x80000000000L;
      }
      if (g_Player.IsMusic)
      {
        MPStatus.Media_IsMusic = true;
        num |= (ulong)0x80L;
        property = GUIPropertyManager.GetProperty("#Play.Current.File");
        if (property.Length > 0)
        {
          string str2;
          strArray = property.Split(new char[] { '.' });
          if ((strArray.Length > 1) && ((str2 = strArray[1]) != null))
          {
            if (!(str2 == "mp3"))
            {
              if (str2 == "ogg")
              {
                num |= (ulong)0x1000000L;
              }
              else if (str2 == "wma")
              {
                num |= (ulong)0x4000000L;
              }
              else if (str2 == "wav")
              {
                num |= (ulong)0x4000000000L;
              }
            }
            else
            {
              num |= (ulong)0x2000000L;
            }
          }
        }
      }
      if ((g_Player.IsTV || g_Player.IsTVRecording) & (!g_Player.IsDVD && !g_Player.IsCDA))
      {
        if (g_Player.IsTV)
        {
          MPStatus.Media_IsTV = true;
        }
        else
        {
          MPStatus.Media_IsTVRecording = true;
        }
        num |= (ulong)8L;
      }
      if (g_Player.IsDVD || g_Player.IsCDA)
      {
        if (g_Player.IsDVD & g_Player.IsVideo)
        {
          MPStatus.Media_IsDVD = true;
          MPStatus.Media_IsVideo = true;
          num |= (ulong)0x40L;
        }
        else if (g_Player.IsCDA & !g_Player.IsVideo)
        {
          MPStatus.Media_IsCD = true;
          MPStatus.Media_IsMusic = true;
          num |= (ulong)0x80L;
        }
        num |= (ulong)0x10L;
      }
      if (!(g_Player.IsVideo & !g_Player.IsDVD))
      {
        return num;
      }
      MPStatus.Media_IsVideo = true;
      num |= (ulong)0x40L;
      property = GUIPropertyManager.GetProperty("#Play.Current.File");
      if (property.Length <= 0)
      {
        return num;
      }
      num |= (ulong)0x80L;
      strArray = property.Split(new char[] { '.' });
      if ((strArray.Length <= 1) || ((str3 = strArray[1].ToLower()) == null))
      {
        return num;
      }
      if ((!(str3 == "ifo") && !(str3 == "vob")) && !(str3 == "mpg"))
      {
        if (str3 != "wmv")
        {
          if (str3 == "divx")
          {
            return (num | ((ulong)0x10000L));
          }
          if (str3 != "xvid")
          {
            return num;
          }
          return (num | 0x80000000L);
        }
      }
      else
      {
        return (num | ((ulong)0x20000L));
      }
      return (num | ((ulong)0x40000000L));
    }

    public void ShowPlugin()
    {
      Log.Info("CybrDisplay.ShowPlugin(): Called", new object[0]);
      new SetupForm().ShowDialog();
      Log.Info("CybrDisplay.ShowPlugin(): Completed", new object[0]);
    }

    internal static void ShowSystemStatus(ref SystemStatus CurrentStatus)
    {
    }

    public void Start()
    {
      Log.Info("CybrDisplay.Start(): called", new object[0]);
      Log.Info("CybrDisplay.Start(): {0}", new object[] { Plugin_Version });
      Log.Info("CybrDisplay.Start(): plugin starting...", new object[0]);
      if (!File.Exists(Config.GetFile(Config.Dir.Config, "CybrDisplay.xml")))
      {
        Log.Info("CybrDisplay.Start(): plugin not configured... Unable to start", new object[0]);
      }
      else
      {
        Log.Info("CybrDisplay - constructor: forcing load of \"{0}\" as a window plugin", new object[] { Assembly.GetExecutingAssembly().Location });
        MPStatus = new SystemStatus();
        InitSystemStatus(ref MPStatus);
        this.GetTVSource();
        this.LoadSetupWindows();
        if (CybrDisplayPlugin.Settings.Instance.ShowPropertyBrowser)
        {
          lock (PropertyBrowserMutex)
          {
            Log.Info("CybrDisplay.Start(): opening PropertyBrowser.", new object[0]);
            this.browser = new PropertyBrowser();
            this.browser.FormClosing += new FormClosingEventHandler(this.browser_Closing);
            this.browser.Show();
            _PropertyBrowserAvailable = true;
          }
        }
        this.DoStart();
        Log.Info("CybrDisplay.Start(): completed", new object[0]);
      }
    }

    public void Stop()
    {
      Log.Info("CybrDisplay.Stop(): called", new object[0]);
      if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
      {
        Log.Debug("CybrDisplay: Plugin is being stopped.", new object[0]);
      }
      _PropertyBrowserAvailable = false;
      SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
      this.DoStop();
      if (this.browser != null)
      {
        Log.Info("CybrDisplay.Stop(): closing PropertyBrowser.", new object[0]);
        this.browser.Close();
        this.browser = null;
      }
      if (this.display != null)
      {
        this.display.Dispose();
        this.display = null;
      }
      Log.Info("CybrDisplay.Stop(): completed", new object[0]);
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      if (CybrDisplayPlugin.Settings.Instance.ExtensiveLogging)
      {
        Log.Debug("CybrDisplay: SystemPowerModeChanged event was raised.", new object[0]);
      }
      switch (e.Mode)
      {
        case PowerModes.Resume:
          Log.Info("CybrDisplay: Resume from Suspend or Hibernation detected, starting plugin", new object[0]);
          SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
          this.DoStart();
          break;

        case PowerModes.StatusChange:
          break;

        case PowerModes.Suspend:
          Log.Info("CybrDisplay: Suspend or Hibernation detected, shutting down plugin", new object[0]);
          this.DoStop();
          return;

        default:
          return;
      }
    }

    private void UnloadSetupWindows()
    {
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.BackLight);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.KeyPad);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.Remote);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.DisplayControl);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.DisplayOptions);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.Equalizer);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.MainMenu);
    }

    public static bool xPlayer_IsActive()
    {
      return (g_Player.Player != null);
    }

    public static bool xPlayer_IsCDA()
    {
      return g_Player.IsCDA;
    }

    public static bool xPlayer_IsDVD()
    {
      return g_Player.IsDVD;
    }

    public static bool xPlayer_IsMusic()
    {
      return g_Player.IsMusic;
    }

    public static bool xPlayer_IsRadio()
    {
      return g_Player.IsRadio;
    }

    public static bool xPlayer_IsTimeshifting()
    {
      return g_Player.IsTimeShifting;
    }

    public static bool xPlayer_IsTV()
    {
      return g_Player.IsTV;
    }

    public static bool xPlayer_IsTVRecording()
    {
      return g_Player.IsTV;
    }

    public static bool xPlayer_IsVideo()
    {
      return g_Player.IsVideo;
    }

    public static bool xPlayer_Paused()
    {
      return g_Player.Paused;
    }

    public static int xPlayer_Speed()
    {
      return g_Player.Speed;
    }

    internal static string Plugin_Version
    {
      get
      {
        return "CybrDisplay Plugin v05_07_2008";
      }
    }

    public enum DefaultControl
    {
      BackLight = 0x41,
      DisplayControls = 40,
      DisplayOptions = 30,
      Equalizer = 20,
      KeyPad = 60,
      Main = 3,
      RemoteControl = 50
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayControl
    {
      public bool BlankDisplayWithVideo;
      public bool EnableDisplayAction;
      public int DisplayActionTime;
      public bool BlankDisplayWhenIdle;
      public int BlankIdleDelay;
      public long _BlankIdleTime;
      public long _BlankIdleTimeout;
      public bool _DisplayControlAction;
      public long _DisplayControlLastAction;
      public long _DisplayControlTimeout;
      public string _Shutdown1;
      public string _Shutdown2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayOptions
    {
      public bool DiskIcon;
      public bool VolumeDisplay;
      public bool ProgressDisplay;
      public bool DiskMediaStatus;
      public bool DiskMonitor;
      public bool UseCustomFont;
      public bool UseLargeIcons;
      public bool UseCustomIcons;
      public bool UseInvertedIcons;
    }

    public class DynaInvoke
    {
      public static Hashtable AssemblyReferences = new Hashtable();
      public static Hashtable ClassReferences = new Hashtable();

      public static DynaClassInfo GetClassReference(string AssemblyName, string ClassName)
      {
        Assembly assembly;
        if (ClassReferences.ContainsKey(AssemblyName))
        {
          return (DynaClassInfo)ClassReferences[AssemblyName];
        }
        if (!AssemblyReferences.ContainsKey(AssemblyName))
        {
          AssemblyReferences.Add(AssemblyName, assembly = Assembly.LoadFrom(AssemblyName));
        }
        else
        {
          assembly = (Assembly)AssemblyReferences[AssemblyName];
        }
        foreach (System.Type type in assembly.GetTypes())
        {
          if (type.IsClass && type.FullName.EndsWith("." + ClassName))
          {
            DynaClassInfo info = new DynaClassInfo(type, Activator.CreateInstance(type));
            ClassReferences.Add(AssemblyName, info);
            return info;
          }
        }
        throw new Exception("could not instantiate class");
      }

      public static object InvokeMethod(DynaClassInfo ci, string MethodName, object[] args)
      {
        return ci.type.InvokeMember(MethodName, BindingFlags.InvokeMethod, null, ci.ClassObject, args);
      }

      public static object InvokeMethod(string AssemblyName, string ClassName, string MethodName, object[] args)
      {
        return InvokeMethod(GetClassReference(AssemblyName, ClassName), MethodName, args);
      }

      public static object InvokeMethodSlow(string AssemblyName, string ClassName, string MethodName, object[] args)
      {
        foreach (System.Type type in Assembly.LoadFrom(AssemblyName).GetTypes())
        {
          if (type.IsClass && type.FullName.EndsWith("." + ClassName))
          {
            object target = Activator.CreateInstance(type);
            return type.InvokeMember(MethodName, BindingFlags.InvokeMethod, null, target, args);
          }
        }
        throw new Exception("could not invoke method");
      }

      public class DynaClassInfo
      {
        public object ClassObject;
        public System.Type type;

        public DynaClassInfo()
        {
        }

        public DynaClassInfo(System.Type t, object c)
        {
          this.type = t;
          this.ClassObject = c;
        }
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EQControl
    {
      public int Render_BANDS;
      public int Render_MaxValue;
      public bool UseEqDisplay;
      public bool UseNormalEq;
      public bool UseStereoEq;
      public bool UseVUmeter;
      public bool UseVUmeter2;
      public bool RestrictEQ;
      public bool SmoothEQ;
      public bool DelayEQ;
      public bool _useVUindicators;
      public int _useEqMode;
      public bool EQTitleDisplay;
      public int _DelayEQTime;
      public int _EQTitleDisplayTime;
      public int _EQTitleShowTime;
      public bool _EqDataAvailable;
      public bool _EQDisplayTitle;
      public DateTime _LastEQupdate;
      public float[] EqFftData;
      public byte[] EqArray;
      public int[] LastEQ;
      public double _LastEQTitle;
      public int _Max_EQ_FPS;
      public int _EQ_Framecount;
      public int _EQ_Restrict_FPS;
      public int _EqUpdateDelay;
      public DateTime _EQ_FPS_time;
      public bool _AudioIsMixing;
      public bool _AudioUseASIO;
    }

    public enum PluginIcons : ulong
    {
      ICON_AC3 = 0x10000000L,
      ICON_Alarm = 0x800000000L,
      ICON_CD_DVD = 0x10L,
      ICON_DivX = 0x10000L,
      ICON_DTS = 0x8000000L,
      ICON_FFWD = 0x20000000000L,
      ICON_FIT = 0x400000L,
      ICON_FRWD = 0x10000000000L,
      ICON_HDTV = 0x100000L,
      ICON_Movie = 0x40L,
      ICON_MP3 = 0x2000000L,
      ICON_MPG = 0x20000L,
      ICON_MPG2 = 0x20000000L,
      ICON_Music = 0x80L,
      ICON_News = 2L,
      ICON_OGG = 0x1000000L,
      ICON_Pause = 0x80000000000L,
      ICON_Photo = 0x20L,
      ICON_Play = 0x100000000000L,
      ICON_Rec = 0x400000000L,
      ICON_REP = 0x2000000000L,
      ICON_SCR1 = 0x80000L,
      ICON_SCR2 = 0x40000L,
      ICON_SFL = 0x1000000000L,
      ICON_SPDIF = 0x200L,
      ICON_SRC = 0x800000L,
      ICON_Stop = 0x40000000000L,
      ICON_Time = 0x100000000L,
      ICON_TV = 8L,
      ICON_TV_2 = 0x200000L,
      ICON_Vol = 0x200000000L,
      ICON_WAV = 0x4000000000L,
      ICON_WebCast = 4L,
      ICON_WMA = 0x4000000L,
      ICON_WMA2 = 0x8000000000L,
      ICON_WMV = 0x40000000L,
      ICON_xVid = 0x80000000L,
      SPKR_FC = 0x8000L,
      SPKR_FL = 1L,
      SPKR_FR = 0x4000L,
      SPKR_LFE = 0x1000L,
      SPKR_RL = 0x400L,
      SPKR_RR = 0x100L,
      SPKR_SL = 0x2000L,
      SPKR_SR = 0x800L
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemStatus
    {
      public CybrDisplayPlugin.Status CurrentPluginStatus;
      public ulong CurrentIconMask;
      public bool MP_Is_Idle;
      public int SystemVolumeLevel;
      public bool IsMuted;
      public bool MediaPlayer_Active;
      public bool MediaPlayer_Playing;
      public bool MediaPlayer_Paused;
      public bool Media_IsRecording;
      public bool Media_IsTV;
      public bool Media_IsTVRecording;
      public bool Media_IsDVD;
      public bool Media_IsCD;
      public bool Media_IsRadio;
      public bool Media_IsVideo;
      public bool Media_IsMusic;
      public bool Media_IsTimeshifting;
      public double Media_CurrentPosition;
      public double Media_Duration;
      public int Media_Speed;
      public bool _AudioIsMixing;
      public bool _AudioUseASIO;
      public bool _AudioUseMasterVolume;
      public bool _AudioUseWaveVolume;
    }

    public enum WindowIDs
    {
      WindowID_BackLight = 0x4dac,
      WindowID_DisplayControl = 0x4da9,
      WindowID_DisplayOptions = 0x4da8,
      WindowID_Equalizer = 0x4da7,
      WindowID_Internal = 0x4da5,
      WindowID_KeyPad = 0x4dab,
      WindowID_Main = 0x4da6,
      WindowID_RemoteControl = 0x4daa
    }
  }
}

