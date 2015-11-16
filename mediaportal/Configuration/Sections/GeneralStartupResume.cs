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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralStartupResume : SectionSettings
  {
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessageTimeout(
      IntPtr hwnd,
      IntPtr msg,
      IntPtr wParam,
      IntPtr lParam,
      IntPtr fuFlags,
      IntPtr uTimeout,
      out IntPtr lpdwResult);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
      [MarshalAs(UnmanagedType.U4)]
      public int cb;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string DeviceName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceString;
      [MarshalAs(UnmanagedType.U4)]
      public DisplayDeviceStateFlags StateFlags;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceID;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string DeviceKey;
    }

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }

    // ReSharper disable InconsistentNaming
    private const int WM_SETTINGCHANGE = 0x1A;
    private const int SMTO_ABORTIFHUNG = 0x2;
    private const int HWND_BROADCAST = 0xFFFF;
    // ReSharper restore InconsistentNaming

    public GeneralStartupResume()
      : this("Startup/Resume Settings") {}

    public GeneralStartupResume(string name)
      : base(name)
    {
      InitializeComponent();
    }

    private int _screennumber; // 0 is the primary screen
    private string _screenDeviceId = "";
    private bool _usePrimaryScreen;

    private readonly string[][] _sectionEntries = new[]
    {
                                            // 0 Start MediaPortal in fullscreen mode
                                            new[] {"general", "startfullscreen", "true"},
                                            // 1 Keep MediaPortal fullscreen mode (don't rely on windows resolution change)
                                            new[] {"general", "keepstartfullscreen", "false"},
                                            // 2 Use screenselector to choose on which screen MP should start
                                            new[] {"general", "usefullscreensplash", "true"},
                                            // 3 Keep MediaPortal always on top
                                            new[] {"general", "alwaysontop", "false"},
                                            // 4 Hide taskbar in fullscreen mode
                                            new[] {"general", "hidetaskbar", "false"},
                                            // 5 Autostart MediaPortal on Windows startup
                                            new[] {"general", "autostart", "false"},
                                            // 6 Minimize to tray on start up
                                            new[] {"general", "minimizeonstartup", "false"},
                                            // 7 Minimize to tray on GUI exit
                                            new[] {"general", "minimizeonexit", "false"},
                                            // 8 Minimize to tray on focus loss (fullscreen only)
                                            new[] {"general", "minimizeonfocusloss", "false"},
                                            // 9 Turn off monitor when blanking screen
                                            new[] {"general", "turnoffmonitor", "false"},
                                            // 10 Show last active module when starting / resuming from standby
                                            new[] {"general", "showlastactivemodule", "false"},
                                            // 11 Stop playback on removal of an audio renderer
                                            new[] {"general", "stoponaudioremoval", "true"},
                                            // 12 No AutoStart on RemoteDesktop
                                            new[] {"general", "noautostartonrdp", "false"},
                                            // 13 Always use Primary screen
                                            new[] {"general", "useprimaryscreen", "false"}
                                          };

    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private readonly IContainer components = null;

    // PLEASE NOTE: when adding items, adjust the box so it doesn't get scrollbars
    //              AND be careful cause depending on where you add a setting, the indexes might have changed!!!

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
                    string sSerFind = new string(new char[] {(char) 00, (char) 00, (char) 00, (char) 0xff});
                    string sModFind = new string(new char[] {(char) 00, (char) 00, (char) 00, (char) 0xfc});

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

    public override void LoadSettings()
    {
      int indexAdapter = 0;
      DataTable dtblDataSource = new DataTable();
      dtblDataSource.Columns.Add("DisplayMember");
      dtblDataSource.Columns.Add("ValueMember");
      dtblDataSource.Columns.Add("AdditionalInfo");
      dtblDataSource.Columns.Add("MonitorDisplayName");
      cbScreen.DataSource.SafeDispose();
      cbScreen.Items.Clear();
      cbScreen.DataSource = dtblDataSource;
      cbScreen.DisplayMember = "DisplayMember";
      cbScreen.ValueMember = "ValueMember";

      foreach (Screen screen in Screen.AllScreens)
      {
        const int dwf = 0;
        var info = new DISPLAY_DEVICE();
        string monitorname = null;
        string deviceId = null;
        info.cb = Marshal.SizeOf(info);
        if (EnumDisplayDevices(screen.DeviceName, 0, ref info, dwf))
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
                if (!string.IsNullOrEmpty(display.Model))
                {
                  dtblDataSource.Rows.Add(string.Format("{0} ({1}x{2}) on {3} - Screen Primary : {4}", display.Model,
                    adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height, adapter.Information.Description, screen.Primary ? "Yes" : "No"),
                    indexAdapter, info.DeviceID, adapter.Information.DeviceName.Trim());
                  indexAdapter++;
                  detectedId = true;
                  break;
                }
                dtblDataSource.Rows.Add(string.Format("{0} ({1}x{2}) on {3} - Screen Primary : {4}", monitorname,
                  adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height, adapter.Information.Description, screen.Primary ? "Yes" : "No"),
                  indexAdapter, info.DeviceID, adapter.Information.DeviceName.Trim());
                indexAdapter++;
                detectedId = true;
                break;
              }
            }
            if (!detectedId)
            {
              dtblDataSource.Rows.Add(string.Format("{0} ({1}x{2}) on {3} - Screen Primary : {4}", monitorname,
                adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height, adapter.Information.Description, screen.Primary ? "Yes" : "No"),
                indexAdapter, deviceId, adapter.Information.DeviceName.Trim());
              indexAdapter++;
              break;
            }
          }
        }
      }

      using (Settings xmlreader = new MPSettings())
      {
        // Load general settings
        for (int index = 0; index < _sectionEntries.Length; index++)
        {
          string[] currentSection = _sectionEntries[index];
          settingsCheckedListBox.SetItemChecked(index, xmlreader.GetValueAsBool(currentSection[0], currentSection[1], bool.Parse(currentSection[2])));
        }

        _screennumber = xmlreader.GetValueAsInt("screenselector", "screennumber", 0);
        _screenDeviceId = xmlreader.GetValueAsString("screenselector", "screendeviceid", "");
        _usePrimaryScreen = xmlreader.GetValueAsBool("general", "useprimaryscreen", false);

        while (cbScreen.Items.Count <= _screennumber)
        {
          dtblDataSource.Rows.Add("screen nr :" + cbScreen.Items.Count + " (currently unavailable)");
        }

        for (int index = 0; index < cbScreen.Items.Count; index++)
        {
          //Get additional info for selected item
          var dataRowView = cbScreen.Items[index] as DataRowView;
          if (dataRowView != null)
          {
            string screenDeviceId = dataRowView["AdditionalInfo"].ToString();
            if (screenDeviceId.Equals(_screenDeviceId))
            {
              cbScreen.SelectedIndex = index;
              _screennumber = index;
            }
          }
        }

        // Check if screen are present and if not force to use primary screen
        if (cbScreen.SelectedIndex == -1)
        {
          cbScreen.SelectedIndex = 0;
        }

        if (_usePrimaryScreen)
        {
          cbScreen.Enabled = false;
        }

        nudDelay.Value = xmlreader.GetValueAsInt("general", "delay", 0);
        mpCheckBoxMpStartup.Checked = xmlreader.GetValueAsBool("general", "delay startup", false);
        mpCheckBoxMpResume.Checked = xmlreader.GetValueAsBool("general", "delay resume", false);
      }

      // On single seat WaitForTvService is forced enabled !
      cbWaitForTvService.Checked = Network.IsSingleSeat();
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("screenselector", "screennumber", cbScreen.SelectedIndex);
        var dataRowView = cbScreen.Items[cbScreen.SelectedIndex] as DataRowView;
        if (dataRowView != null)
        {
          xmlwriter.SetValue("screenselector", "screendeviceid", dataRowView["AdditionalInfo"].ToString());
          xmlwriter.SetValue("screenselector", "screendisplayname", dataRowView["MonitorDisplayName"].ToString());
        }

        for (int index = 0; index < _sectionEntries.Length; index++)
        {
          string[] currentSection = _sectionEntries[index];
          xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
        }

        xmlwriter.SetValue("general", "delay", nudDelay.Value);
        xmlwriter.SetValueAsBool("general", "delay startup", mpCheckBoxMpStartup.Checked);
        xmlwriter.SetValueAsBool("general", "delay resume", mpCheckBoxMpResume.Checked);
        xmlwriter.SetValueAsBool("general", "wait for tvserver", cbWaitForTvService.Checked);
        xmlwriter.SetValueAsBool("general", "useprimaryscreen", _usePrimaryScreen);
      }

      try
      {
        if (settingsCheckedListBox.GetItemChecked(5)) // autostart on boot
        {
          string fileName = Config.GetFile(Config.Dir.Base, "MediaPortal.exe");
          using (
            RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)
            )
          {
            if (subkey != null) subkey.SetValue("MediaPortal", fileName);
          }
        }
        else
        {
          using (
            RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)
            )
          {
            if (subkey != null) subkey.DeleteValue("MediaPortal", false);
          }
        }

        if (settingsCheckedListBox.GetItemChecked(3)) // always on top
        {
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
          {
            if (subkey != null) subkey.SetValue("ForegroundLockTimeout", 0);
          }
        }

        IntPtr result;
        SendMessageTimeout((IntPtr)HWND_BROADCAST, (IntPtr)WM_SETTINGCHANGE, IntPtr.Zero, Marshal.StringToBSTR(string.Empty), (IntPtr)SMTO_ABORTIFHUNG, (IntPtr)3, out result);
      }
      catch (Exception ex)
      {
        Log.Error("General: Exception - {0}", ex);
      }
    }

    private void settingsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {

    }

    private void settingsCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      settingsCheckedListBox.SelectedIndexChanged -= settingsCheckedListBox_SelectedIndexChanged;
      settingsCheckedListBox.SelectionMode = SelectionMode.None;
      if (settingsCheckedListBox.GetItemChecked(1))
      {
        // if we use keepstartfullscreen, we need to force to use MP as fullscreen
        settingsCheckedListBox.SetItemChecked(0, true);
      }
      if (settingsCheckedListBox.GetItemChecked(13))
      {
        cbScreen.Enabled = false;
        _usePrimaryScreen = true;
        Log.Debug("General: item changed checked {0} {1}", cbScreen.Enabled.ToString(), _usePrimaryScreen);
      }
      else
      {
        cbScreen.Enabled = true;
        _usePrimaryScreen = false;
        Log.Debug("General: item changed no checked {0} {1}", cbScreen.Enabled.ToString(), _usePrimaryScreen);
      }
      settingsCheckedListBox.SelectedIndexChanged += settingsCheckedListBox_SelectedIndexChanged;
      settingsCheckedListBox.SelectionMode = SelectionMode.One;
    }

    private void lbScreen_Click(object sender, EventArgs e)
    {

    }

    private void cbScreen_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
  }
}