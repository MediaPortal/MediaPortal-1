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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
    // ReSharper disable InconsistentNaming
    public class DISPLAY_DEVICE
    // ReSharper restore InconsistentNaming
    {
      public int cb = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName = new String(' ', 32);
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString = new String(' ', 128);
      public int StateFlags = 0;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID = new String(' ', 128);
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey = new String(' ', 128);
    }

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, [In, Out] DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

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
                                            new[] {"general", "noautostartonrdp", "true"}
                                          };

    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private readonly IContainer components = null;

    // PLEASE NOTE: when adding items, adjust the box so it doesn't get scrollbars    
    //              AND be careful cause depending on where you add a setting, the indexes might have changed!!!

    public override void LoadSettings()
    {
      cbScreen.Items.Clear();
      foreach (Screen screen in Screen.AllScreens)
      {
        const int dwf = 0;
        var info = new DISPLAY_DEVICE();
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
          if (screen.DeviceName.Equals(adapter.Information.DeviceName.Trim()))
          {
            cbScreen.Items.Add(string.Format("{0} ({1}x{2}) on {3}", monitorname, adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height, adapter.Information.Description));
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

        while (cbScreen.Items.Count <= _screennumber)
        {
          cbScreen.Items.Add("screen nr :" + cbScreen.Items.Count + " (currently unavailable)");
        }
        cbScreen.SelectedIndex = _screennumber;

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

        for (int index = 0; index < _sectionEntries.Length; index++)
        {
          string[] currentSection = _sectionEntries[index];
          xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
        }

        xmlwriter.SetValue("general", "delay", nudDelay.Value);
        xmlwriter.SetValueAsBool("general", "delay startup", mpCheckBoxMpStartup.Checked);
        xmlwriter.SetValueAsBool("general", "delay resume", mpCheckBoxMpResume.Checked);
        xmlwriter.SetValueAsBool("general", "wait for tvserver", cbWaitForTvService.Checked);
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

    }

    private void lbScreen_Click(object sender, EventArgs e)
    {

    }

    private void cbScreen_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
  }
}