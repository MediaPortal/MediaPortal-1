#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Picture.Database;
using MediaPortal.Player;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Settings
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// Change and tweak settings like Video codecs, skins, etc from inside MP
  /// </summary>
  [PluginIcons("GUISettings.Settings.gif", "GUISettings.SettingsDisabled.gif")]
  public sealed class GUISettings : GUIInternalWindow, ISetupForm, IShowPlugin
  {
    [SkinControl(11)] private readonly GUIButtonControl _btnMiniDisplay = null;
    [SkinControl(10)] private readonly GUIButtonControl _btnTV = null;
    [SkinControl(20)] private readonly GUICheckButton _btnLocked = null;

    [DllImport("shlwapi.dll")]
    private static extern bool PathIsNetworkPath(string path);

    private static bool _settingsChanged;
    private static string _pin = string.Empty;
    
    public GUISettings()
    {
      GetID = (int)Window.WINDOW_SETTINGS; //4
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings.xml");
    }

    #region Serialization

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        _pin = Util.Utils.DecryptPin(xmlreader.GetValueAsString("mpsettings", "pin", string.Empty));

        if (_pin != string.Empty)
        {
          _btnLocked.Selected = true;
        }
      }
      
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("mpsettings", "pin", Util.Utils.EncryptPin(_pin));
      }
    }

    #endregion

    protected override void OnPageLoad()
    {
      if (!Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        _settingsChanged = false;
      }
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100004)); // Settings home
      LoadSettings();
      base.OnPageLoad();

      if (!Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()) && _pin != string.Empty)
      {
        if (!RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
        }
      }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      if (_settingsChanged && !Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        OnRestartMP(GetID);
      }

      if (!Util.Utils.IsGUISettingsWindow(newWindowId))
      {
      }
      base.OnPageDestroy(newWindowId);
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == _btnLocked)
      {
        // User want's to lock settings with PIN
        if (_btnLocked.Selected)
        {
          if (!SetPin())
          {
            // No PIN entered, reset control
            _btnLocked.Selected = false;
          }
        }
        else
        {
          // User want's to remove or change PIN (need current PIN validation first)
          if (RequestPin())
          {
            _pin = string.Empty;
          }
          else
          {
            // Wrong PIN, reset control
            _btnLocked.Selected = true;
          }
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            _btnMiniDisplay.Visible = MiniDisplayHelper.IsSetupAvailable();
            _btnTV.Visible = Util.Utils.UsingTvServer;
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT: {}
          break;
      }
      return base.OnMessage(message);
    }
    
    //public static bool IsGUISettingsWindow(int windowId)
    //{
    //  if (windowId == (int)Window.WINDOW_SETTINGS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_DVD ||
    //      windowId == (int)Window.WINDOW_SETTINGS_BLURAY ||
    //      windowId == (int)Window.WINDOW_SETTINGS_EXTENSIONS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_FOLDERS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GENERALMAIN||
    //      windowId == (int)Window.WINDOW_SETTINGS_GENERALMP ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GENERALSTARTUP ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GENERALRESUME ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GENERALREFRESHRATE ||
    //      windowId == (int)Window.WINDOW_SETTINGS_MOVIES ||
    //      windowId == (int)Window.WINDOW_SETTINGS_MUSIC ||
    //      windowId == (int)Window.WINDOW_SETTINGS_MUSICDATABASE ||
    //      windowId == (int)Window.WINDOW_SETTINGS_MUSICNOWPLAYING ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUIMAIN ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUISKIN ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUIGENERAL||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUIONSCREEN_DISPLAY ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUICONTROL ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUISKIPSTEPS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUITHUMBNAILS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_PICTURES ||
    //      windowId == (int)Window.WINDOW_SETTINGS_PICTURES_SLIDESHOW ||
    //      windowId == (int)Window.WINDOW_SETTINGS_PICTURESDATABASE ||
    //      windowId == (int)Window.WINDOW_SETTINGS_PLAYLIST ||
    //      windowId == (int)Window.WINDOW_SETTINGS_RECORDINGS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUISCREENSETUP ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GUISCREENSAVER ||
    //      windowId == (int)Window.WINDOW_SETTINGS_SORT_CHANNELS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_TV ||
    //      windowId == (int)Window.WINDOW_SETTINGS_TV_EPG ||
    //      windowId == (int)Window.WINDOW_SETTINGS_VIDEODATABASE ||
    //      windowId == (int)Window.WINDOW_SETTINGS_VIDEOOTHERSETTINGS ||
    //      windowId == (int)Window.WINDOW_SETTINGS_GENERALVOLUME ||
    //    // Minidisplay (no enum values in GUIWindow)
    //      windowId == 9000 ||
    //      windowId == 9001 ||
    //      windowId == 9002 ||
    //      windowId == 9003 ||
    //      windowId == 9004 ||
    //      windowId == 9005 ||
    //      windowId == 9006)
    //  {
    //    return true;
    //  }
    //  return false;
    //}

    public static bool SettingsChanged
    {
      get { return _settingsChanged; }
      set { _settingsChanged = value; }
    }

    private bool SetPin()
    {
      var msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
      GUIWindowManager.SendMessage(msgGetPassword);
        
      try
      {
        int iPincode = Int32.Parse(msgGetPassword.Label);
        _pin = iPincode.ToString(CultureInfo.InvariantCulture);
        return true;
      }
      // ReSharper disable EmptyGeneralCatchClause
      catch (Exception) {}
      // ReSharper restore EmptyGeneralCatchClause
      return false;
    }

    public static bool RequestPin()
    {
      bool retry = true;
      bool sucess = false;
      
      while (retry)
      {
        var msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
        GUIWindowManager.SendMessage(msgGetPassword);
        int iPincode = -1;
        try
        {
          iPincode = Int32.Parse(msgGetPassword.Label);
        }
        // ReSharper disable EmptyGeneralCatchClause
        catch (Exception) { }
        // ReSharper restore EmptyGeneralCatchClause

        if (iPincode == Convert.ToInt32(_pin))
        {
          sucess = true;
        }
        
        if (sucess)
        {
          return true;
        }

        var msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0,
                                                     0);
        GUIWindowManager.SendMessage(msgWrongPassword);

        if (!(bool)msgWrongPassword.Object)
        {
          retry = false;
        }
        }
      return false;
    }

    public static bool IsPinLocked()
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        _pin = Util.Utils.DecryptPin(xmlreader.GetValueAsString("mpsettings", "pin", string.Empty));
      }

      if (_pin == string.Empty)
      {
        return false;
      }
      return true;
    }

    #region RestartMP

    public static void OnRestartMP(int windowId)
    {
      var dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      dlgYesNo.SetHeading(927);

      dlgYesNo.SetLine(1, "Settings changed!");
      dlgYesNo.SetLine(2, "Do you want to restart MediaPortal?");
      dlgYesNo.DoModal(windowId);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }

      using (Profile.Settings xmlreader = new MPSettings())
        {
        if (xmlreader.GetValueAsBool("general", "hidetaskbar", false))
        {
          Win32API.EnableStartBar(true);
          Win32API.ShowStartBar(true);
        }
      }

        Log.Info("Settings: OnRestart - prepare for restart!");
        File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
        Log.Info("Settings: OnRestart - saving settings...");
        Profile.Settings.SaveCache();
        DisposeDBs();
        VolumeHandler.Dispose();
        Log.Info("Main: OnSuspend - Done");
      var restartScript = new Process
      {
        EnableRaisingEvents = false,
        StartInfo =
        {
          WorkingDirectory = Config.GetFolder(Config.Dir.Base),
          FileName = Config.GetFile(Config.Dir.Base, @"restart.vbs")
        }
      };
        Log.Debug("Settings: OnRestart - executing script {0}", restartScript.StartInfo.FileName);
        restartScript.Start();
        try
        {
          // Maybe the scripting host is not available therefore do not wait infinitely.
          if (!restartScript.HasExited)
          {
            restartScript.WaitForExit();
          }
        }
        catch (Exception ex)
        {
          Log.Error("Settings: OnRestart - WaitForExit: {0}", ex.Message);
        }
      }

    private static void DisposeDBs()
    {
      string dbPath = FolderSettings.DatabaseName;
      bool isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
      if (isRemotePath)
      {
        Log.Info("Settings: disposing FolderDatabase3 sqllite database.");
        FolderSettings.Dispose();
      }

      dbPath = PictureDatabase.DatabaseName;
      isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
      if (isRemotePath)
      {
        Log.Info("Settings: disposing PictureDatabase sqllite database.");
        PictureDatabase.Dispose();
      }

      dbPath = MediaPortal.Video.Database.VideoDatabase.DatabaseName;
      isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
      if (isRemotePath)
      {
        Log.Info("Settings: disposing VideoDatabaseV5.db3 sqllite database.");
        MediaPortal.Video.Database.VideoDatabase.Dispose();
      }

      dbPath = MediaPortal.Music.Database.MusicDatabase.Instance.DatabaseName;
      isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
      if (isRemotePath)
      {
        Log.Info("Settings: disposing MusicDatabase db3 sqllite database.");
        MediaPortal.Music.Database.MusicDatabase.Dispose();
      }
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public string PluginName()
    {
      return "Settings";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(5);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Configure MediaPortal using graphical user interface";
    }

    public void ShowPlugin() {}

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }
    
    #endregion
  }
}