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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using Action = MediaPortal.GUI.Library.Action;

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Settings
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public sealed class GUISettingsGeneralResume : GUIInternalWindow
  {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    // ReSharper disable InconsistentNaming
    public class DISPLAY_DEVICE
    // ReSharper restore InconsistentNaming
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
    public static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, [In, Out] DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

    [SkinControl(20)] private readonly GUICheckButton _cmTurnoffmonitor = null;
    [SkinControl(21)] private readonly GUICheckButton _cmShowlastactivemodule = null;
    [SkinControl(22)] private readonly GUICheckButton _cmStopOnAudioRemoval = null;
    [SkinControl(24)] private readonly GUIButtonControl _btnStartScreen = null;
    [SkinControl(30)] private readonly GUIButtonControl _btnDelayStartup = null;
    [SkinControl(31)] private readonly GUICheckButton _cmDelayStartup = null;
    [SkinControl(32)] private readonly GUICheckButton _cmDelayResume = null;

    private int _iStartUpDelay;
    private int _screennumber; // 0 is the default screen for MP
    private string _screendeviceid = "";
    private readonly List<Tuple<string, string, string>> _screenCollection = new List<Tuple<string, string, string>>();

    public GUISettingsGeneralResume()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GENERALRESUME; //1017
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_General_Resume.xml"));
    }

    /// <summary>
    /// The Constructor to create a new instances of the DisplayDetails class...
    /// </summary>
    public class DisplayDetails
    {
      public string PnPID { get; set; }

      public string SerialNumber { get; set; }

      public string Model { get; set; }

      public string MonitorID { get; set; }

      public string DriverID { get; set; }

      /// <summary>
      /// The Constructor to create a new instances of the DisplayDetails class...
      /// </summary>
      public DisplayDetails(string sPnPID, string sSerialNumber, string sModel, string sMonitorID, string sDriverID)
      {
        PnPID = sPnPID;
        SerialNumber = sSerialNumber;
        Model = sModel;
        MonitorID = sMonitorID;
        DriverID = sDriverID;
      }

      /// <summary>
      /// This Function returns all Monitor Details
      /// </summary>
      /// <returns></returns>
      static public IEnumerable<DisplayDetails> GetMonitorDetails()
      {
        //Open the Display Reg-Key
        RegistryKey Display = Registry.LocalMachine;
        Boolean bFailed = false;
        try
        {
          Display = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\DISPLAY");
        }
        catch
        {
          bFailed = true;
        }

        if (!bFailed & (Display != null))
        {

          //Get all MonitorIDss
          foreach (string sMonitorID in Display.GetSubKeyNames())
          {
            RegistryKey MonitorID = Display.OpenSubKey(sMonitorID);

            if (MonitorID != null)
            {
              //Get all Plug&Play ID's
              foreach (string sPNPID in MonitorID.GetSubKeyNames())
              {
                RegistryKey PnPID = MonitorID.OpenSubKey(sPNPID);
                if (PnPID != null)
                {
                  string[] sSubkeys = PnPID.GetSubKeyNames();

                  //Check if Monitor is active
                  if (sSubkeys.Contains("Device Parameters"))
                  {
                    string DriverID = PnPID.GetValue("Driver", null) as string;
                    RegistryKey DevParam = PnPID.OpenSubKey("Device Parameters");
                    string sSerial = "";
                    string sModel = "";

                    //Define Search Keys
                    string sSerFind = new string(new char[] { (char)00, (char)00, (char)00, (char)0xff });
                    string sModFind = new string(new char[] { (char)00, (char)00, (char)00, (char)0xfc });

                    //Get the EDID code
                    byte[] bObj = DevParam.GetValue("EDID", null) as byte[];
                    if (bObj != null)
                    {
                      //Get the 4 Vesa descriptor blocks
                      string[] sDescriptor = new string[4];
                      sDescriptor[0] = Encoding.Default.GetString(bObj, 0x36, 18);
                      sDescriptor[1] = Encoding.Default.GetString(bObj, 0x48, 18);
                      sDescriptor[2] = Encoding.Default.GetString(bObj, 0x5A, 18);
                      sDescriptor[3] = Encoding.Default.GetString(bObj, 0x6C, 18);

                      //Search the Keys
                      foreach (string sDesc in sDescriptor)
                      {
                        if (sDesc.Contains(sSerFind))
                        {
                          sSerial = sDesc.Substring(4).Replace("\0", "").Trim();
                        }
                        if (sDesc.Contains(sModFind))
                        {
                          sModel = sDesc.Substring(4).Replace("\0", "").Trim();
                        }
                      }
                    }
                    if (!string.IsNullOrEmpty(sPNPID + sSerFind + sModel + sMonitorID + DriverID))
                    {
                      yield return new DisplayDetails(sPNPID, sSerial, sModel, sMonitorID, DriverID);
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // Resume settings
        _cmTurnoffmonitor.Selected = xmlreader.GetValueAsBool("general", "turnoffmonitor", false);
        _cmShowlastactivemodule.Selected = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
        _cmStopOnAudioRemoval.Selected = xmlreader.GetValueAsBool("general", "stoponaudioremoval", false);
        _screennumber = xmlreader.GetValueAsInt("screenselector", "screennumber", 0);
        _screendeviceid = xmlreader.GetValueAsString("screenselector", "screendeviceid", "");

        // Delay startup
        _iStartUpDelay = xmlreader.GetValueAsInt("general", "delay", 0);
        string property = _iStartUpDelay + " sec";
        GUIPropertyManager.SetProperty("#delayStartup", property);

        if (_iStartUpDelay == 0)
        {
          _cmDelayStartup.IsEnabled = false;
          _cmDelayResume.IsEnabled = false;
        }
        else
        {
          _cmDelayStartup.IsEnabled = true;
          _cmDelayResume.IsEnabled = true;
        }
        _cmDelayStartup.Selected = xmlreader.GetValueAsBool("general", "delay startup", false);
        _cmDelayResume.Selected = xmlreader.GetValueAsBool("general", "delay resume", false);

        GetScreens();
        GUIPropertyManager.SetProperty("#defScreen", _screenCollection[_screennumber].Item1);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "turnoffmonitor", _cmTurnoffmonitor.Selected);
        xmlwriter.SetValueAsBool("general", "showlastactivemodule", _cmShowlastactivemodule.Selected);
        xmlwriter.SetValueAsBool("general", "stoponaudioremoval", _cmStopOnAudioRemoval.Selected);
        xmlwriter.SetValueAsBool("general", "delay startup", _cmDelayStartup.Selected);
        xmlwriter.SetValueAsBool("general", "delay resume", _cmDelayResume.Selected);
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      LoadSettings();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101017)); //General - Resume
      base.OnPageLoad();

      if (!Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (GUISettings.IsPinLocked() && !GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      if (GUISettings.SettingsChanged && !Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
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
      if (control == _cmTurnoffmonitor)
      {
        SettingsChanged(true);
      }
      if (control == _cmShowlastactivemodule)
      {
        SettingsChanged(true);
      }
      if (control == _cmStopOnAudioRemoval)
      {
        SettingsChanged(true);
      }
      // Delay at startup
      if (control == _btnDelayStartup)
      {
        OnStartUpDelay();
      }
      if (control == _cmDelayStartup)
      {
        SettingsChanged(true);
      }
      if (control == _cmDelayResume)
      {
        SettingsChanged(true);
      }
      if (control == _btnStartScreen)
      {
        OnShowScreens();
      }

      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    private void OnStartUpDelay()
    {
      string seconds = _iStartUpDelay.ToString(CultureInfo.InvariantCulture);
      GetNumberFromKeyboard(ref seconds);
      _iStartUpDelay = Convert.ToInt32(seconds);

      string property = _iStartUpDelay + " " + GUILocalizeStrings.Get(2999); // sec
      GUIPropertyManager.SetProperty("#delayStartup", property);

      if (_iStartUpDelay == 0)
      {
        _cmDelayStartup.IsEnabled = false;
        _cmDelayResume.IsEnabled = false;
      }
      else
      {
        _cmDelayStartup.IsEnabled = true;
        _cmDelayResume.IsEnabled = true;
      }

      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("general", "delay", _iStartUpDelay);
        SettingsChanged(true);
      }
    }

    private void OnShowScreens()
    {
      // Refresh screen
      GetScreens();

      var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Options

      foreach (Tuple<string, string, string> screen in _screenCollection)
      {
        dlg.Add(screen.Item1);
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

      // Check if screen are present and if not force to use primary screen
      if (dlg.SelectedLabel == -1)
      {
        dlg.SelectedLabel = 0;
      }

      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("screenselector", "screennumber", dlg.SelectedLabel);
        xmlwriter.SetValue("screenselector", "screendeviceid", _screenCollection[dlg.SelectedLabel].Item2);
        xmlwriter.SetValue("screenselector", "screendisplayname", _screenCollection[dlg.SelectedLabel].Item3);
        SettingsChanged(true);
      }

      GUIPropertyManager.SetProperty("#defScreen", _screenCollection[dlg.SelectedLabel].Item1);
    }

    private void GetNumberFromKeyboard(ref string strLine)
    {
      var keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
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
        const int dwf = 0;
        var info = new DISPLAY_DEVICE();
        string monitorname = null;
        string deviceId = null;
        info.cb = Marshal.SizeOf(info);
        if (EnumDisplayDevices(screen.DeviceName, 0, info, dwf))
        {
          monitorname = info.DeviceString;
          deviceId = info.DeviceID;
        }
        if (monitorname == null)
        {
          monitorname = "";
        }
        if (deviceId == null)
        {
          deviceId = "";
        }

        foreach (AdapterInformation adapter in Manager.Adapters)
        {
          bool detectedId = false;
          if (screen.DeviceName.Equals(adapter.Information.DeviceName.Trim()))
          {
            foreach (var display in DisplayDetails.GetMonitorDetails())
            {
              // double check to add display with name from extracted EDID
              if (("MONITOR" + "\\" + display.MonitorID + "\\" + display.DriverID).Equals(info.DeviceID))
              {
                _screenCollection.Add(
                  new Tuple<string, string, string>((
                    string.Format("{0} ({1}x{2}) on {3}", display.Model, adapter.CurrentDisplayMode.Width,
                      adapter.CurrentDisplayMode.Height, adapter.Information.Description)), deviceId, adapter.Information.DeviceName.Trim()));
                detectedId = true;
                break;
              }
            }
            if (!detectedId)
            {
              _screenCollection.Add(
                new Tuple<string, string, string>((
                  string.Format("{0} ({1}x{2}) on {3}", monitorname, adapter.CurrentDisplayMode.Width,
                    adapter.CurrentDisplayMode.Height, adapter.Information.Description)), deviceId, adapter.Information.DeviceName.Trim()));
              break;
            }
          }
        }
      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      GUISettings.SettingsChanged = settingsChanged;
    }

  }
}