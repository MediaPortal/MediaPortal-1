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
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Microsoft.DirectX.Direct3D;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUISettingsGeneralResume : GUIInternalWindow
  {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class DISPLAY_DEVICE
    {
      public int cb = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string DeviceName = new String(' ', 32);
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceString = new String(' ', 128);
      public int StateFlags = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceID = new String(' ', 128);
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceKey = new String(' ', 128);
    }

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayDevices(string lpDevice,
                                                 int iDevNum, [In, Out] DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

    [SkinControl(17)]protected GUICheckButton cmTurnoffmonitor = null;
    [SkinControl(18)]protected GUICheckButton cmTurnmonitoronafterresume = null;
    [SkinControl(19)]protected GUICheckButton cmEnables3trick = null;
    [SkinControl(20)]protected GUICheckButton cmUseS3Hack = null;
    [SkinControl(21)]protected GUICheckButton cmRestartonresume = null;
    [SkinControl(22)]protected GUICheckButton cmShowlastactivemodule = null;
    [SkinControl(23)]protected GUICheckButton cmUsescreenselector = null;
    [SkinControl(24)]protected GUIButtonControl btnShowScreens = null;
    [SkinControl(30)]protected GUIButtonControl btnDelayStartup = null;
    [SkinControl(31)]protected GUICheckButton cmDelayStartup = null;
    [SkinControl(32)]protected GUICheckButton cmDelayResume = null;


    private int _iStartUpDelay = 0;
    private int _screennumber = 0; // 0 is the primary screen
    private ArrayList _screenCollection = new ArrayList();

    public GUISettingsGeneralResume()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GENERALRESUME; //1017
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_General_Resume.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // Resume settings
        cmTurnoffmonitor.Selected = xmlreader.GetValueAsBool("general", "turnoffmonitor", false);
        cmTurnmonitoronafterresume.Selected = xmlreader.GetValueAsBool("general", "turnmonitoronafterresume", false);
        cmEnables3trick.Selected = xmlreader.GetValueAsBool("general", "enables3trick", true);
        cmUseS3Hack.Selected = xmlreader.GetValueAsBool("general", "useS3Hack", false);
        cmRestartonresume.Selected = xmlreader.GetValueAsBool("general", "restartonresume", false);
        cmShowlastactivemodule.Selected = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
        cmUsescreenselector.Selected = xmlreader.GetValueAsBool("screenselector", "usescreenselector", false);

        if (cmUsescreenselector.Selected)
        {
          btnShowScreens.IsEnabled = true;
        }
        else
        {
          btnShowScreens.IsEnabled = false;
        }

        _screennumber = xmlreader.GetValueAsInt("screenselector", "screennumber", 0);

        // Delay startup
        _iStartUpDelay = xmlreader.GetValueAsInt("general", "delay", 0);
        string property = _iStartUpDelay + " sec";
        GUIPropertyManager.SetProperty("#delayStartup", property);

        if (_iStartUpDelay == 0)
        {
          cmDelayStartup.IsEnabled = false;
          cmDelayResume.IsEnabled = false;
        }
        else
        {
          cmDelayStartup.IsEnabled = true;
          cmDelayResume.IsEnabled = true;
        }
        cmDelayStartup.Selected = xmlreader.GetValueAsBool("general", "delay startup", false);
        cmDelayResume.Selected = xmlreader.GetValueAsBool("general", "delay resume", false);

        GetScreens();
        GUIPropertyManager.SetProperty("#defScreen", _screenCollection[_screennumber].ToString());
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "turnoffmonitor", cmTurnoffmonitor.Selected);
        xmlwriter.SetValueAsBool("general", "turnmonitoronafterresume", cmTurnmonitoronafterresume.Selected);
        xmlwriter.SetValueAsBool("general", "enables3trick", cmEnables3trick.Selected);
        xmlwriter.SetValueAsBool("general", "useS3Hack", cmUseS3Hack.Selected);
        xmlwriter.SetValueAsBool("general", "restartonresume", cmRestartonresume.Selected);
        xmlwriter.SetValueAsBool("general", "showlastactivemodule", cmShowlastactivemodule.Selected);
        xmlwriter.SetValueAsBool("screenselector", "usescreenselector", cmUsescreenselector.Selected);
        xmlwriter.SetValueAsBool("general", "delay startup", cmDelayStartup.Selected);
        xmlwriter.SetValueAsBool("general", "delay resume", cmDelayResume.Selected);
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      LoadSettings();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101017)); //General - Resume
      base.OnPageLoad();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      // Resume
      if (control == cmTurnoffmonitor)
      {
        SettingsChanged(true);
      }
      if (control == cmTurnmonitoronafterresume)
      {
        SettingsChanged(true);
      }
      if (control == cmEnables3trick)
      {
        SettingsChanged(true);
      }
      if (control == cmUseS3Hack)
      {
        SettingsChanged(true);
      }
      if (control == cmRestartonresume)
      {
        SettingsChanged(true);
      }
      if (control == cmShowlastactivemodule)
      {
        SettingsChanged(true);
      }
      if (control == cmUsescreenselector)
      {
        SettingsChanged(true);

        if (cmUsescreenselector.Selected)
        {
          btnShowScreens.IsEnabled = true;
        }
        else
        {
          btnShowScreens.IsEnabled = false;
        }
      }
      // Delay at startup
      if (control == btnDelayStartup)
      {
        OnStartUpDelay();
      }
      if (control == cmDelayStartup)
      {
        SettingsChanged(true);
      }
      if (control == cmDelayResume)
      {
        SettingsChanged(true);
      }
      if (control == btnShowScreens)
      {
        OnShowScreens();
      }

      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    private void OnStartUpDelay()
    {
      string seconds = _iStartUpDelay.ToString();
      GetNumberFromKeyboard(ref seconds);
      _iStartUpDelay = Convert.ToInt32(seconds);

      string property = _iStartUpDelay + " " + GUILocalizeStrings.Get(2999); // sec
      GUIPropertyManager.SetProperty("#delayStartup", property);

      if (_iStartUpDelay == 0)
      {
        cmDelayStartup.IsEnabled = false;
        cmDelayResume.IsEnabled = false;
      }
      else
      {
        cmDelayStartup.IsEnabled = true;
        cmDelayResume.IsEnabled = true;
      }

      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("general", "delay", _iStartUpDelay);
        SettingsChanged(true);
      }
    }

    private void OnShowScreens()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Options

      foreach (string screen in _screenCollection)
      {
        dlg.Add(screen);
      }

      if (_screennumber < _screenCollection.Count)
      {
        dlg.SelectedLabel = _screennumber;
      }

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("screenselector", "screennumber", dlg.SelectedLabel);
        SettingsChanged(true);
      }
    }

    private void GetNumberFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;

      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      
      if (keyboard.IsConfirmed)
      {
        int number;
        if (Int32.TryParse(keyboard.Text, out number))
        {
          _iStartUpDelay = number;
          strLine = keyboard.Text;
        }
      }
    }

    public void GetScreens()
    {
      _screenCollection.Clear();
      foreach (Screen screen in Screen.AllScreens)
      {
        int dwf = 0;
        DISPLAY_DEVICE info = new DISPLAY_DEVICE();
        string monitorname = null;
        info.cb = Marshal.SizeOf(info);
        if (EnumDisplayDevices(screen.DeviceName, 0, info, dwf))
        {
          monitorname = info.DeviceString;
        }
        if (monitorname == null)
        {
          monitorname = "";
        }

        foreach (AdapterInformation adapter in Manager.Adapters)
        {
          if (screen.DeviceName.StartsWith(adapter.Information.DeviceName.Trim()))
          {
            _screenCollection.Add(string.Format("{0} ({1}x{2}) on {3}",
                                             monitorname, screen.Bounds.Width, screen.Bounds.Height,
                                             adapter.Information.Description));
          }
        }
      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }

  }
}