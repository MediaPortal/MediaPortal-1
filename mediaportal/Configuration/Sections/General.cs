#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class General : SectionSettings
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

    private const int WM_SETTINGCHANGE = 0x1A;
    private const int SMTO_ABORTIFHUNG = 0x2;
    private const int HWND_BROADCAST = 0xFFFF;

    public General()
      : this("General") { }

    public General(string name)
      : base(name)
    {
      InitializeComponent();
    }

    private string loglevel = "3"; // 1= error, 2 = info, 3 = debug
    private int screennumber = 0; // 0 is the primary screen

    private string[][] sectionEntries = new string[][]
                                          {                                            
                                            new string[] {"general", "startfullscreen", "true"},
                                            // 0 Start MediaPortal in fullscreen mode
                                            new string[] {"general", "usefullscreensplash", "true"},
                                            // 1 Use screenselector to choose on which screen MP should start
                                            new string[] {"general", "alwaysontop", "false"},
                                            // 2 Keep MediaPortal always on top
                                            new string[] {"general", "hidetaskbar", "false"},
                                            // 3 Hide taskbar in fullscreen mode      
                                            new string[] {"general", "autostart", "false"},
                                            // 4 Autostart MediaPortal on Windows startup
                                            new string[] {"general", "minimizeonstartup", "false"},
                                            // 5 Minimize to tray on start up
                                            new string[] {"general", "minimizeonexit", "false"},
                                            // 6 Minimize to tray on GUI exit
                                            new string[] {"general", "mousesupport", "false"},
                                            // 7 Show special mouse controls (scrollbars, etc)      
                                            new string[] {"general", "hideextensions", "true"},
                                            // 8 Hide file extensions like .mp3, .avi, .mpg,...                                             
                                            new string[] {"general", "turnoffmonitor", "false"},
                                            // 9 Turn off monitor when blanking screen	    
                                            new string[] {"general", "turnmonitoronafterresume", "true"},
                                            // 10 Turn monitor/tv on when resuming from standby
                                            new string[] {"general", "enables3trick", "true"},
                                            // 11 Allow S3 standby although wake up devices are present
                                            new string[] {"debug", "useS3Hack", "false"},
                                            // 12 Apply workaround to fix MP freezing on resume on some systems
                                            new string[] {"general", "restartonresume", "false"},
                                            // 13 Restart MediaPortal on resume (avoids stuttering playback with nvidia)
                                            new string[] {"general", "showlastactivemodule", "false"},
                                            // 14 Show last active module when starting / resuming from standby
                                            new string[] {"comskip", "automaticskip", "false"},
                                            // 15 Automatically skip commercials for videos with ComSkip data available
                                            new string[] {"screenselector", "usescreenselector", "false"},
                                            // 16 Allow remember last focused item on supported window/skin
                                            new string[] {"general", "allowRememberLastFocusedItem", "true"},
                                          };

    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private IContainer components = null;

    // PLEASE NOTE: when adding items, adjust the box so it doesn't get scrollbars    
    //              AND be careful cause depending on where you add a setting, the indexes might have changed!!!

    public override void LoadSettings()
    {
      cbScreen.Items.Clear();
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
            cbScreen.Items.Add(string.Format("{0} ({1}x{2}) on {3}",
                                             monitorname, screen.Bounds.Width, screen.Bounds.Height,
                                             adapter.Information.Description));
          }
        }
      }

      using (Settings xmlreader = new MPSettings())
      {
        // Load general settings
        for (int index = 0; index < sectionEntries.Length; index++)
        {
          string[] currentSection = sectionEntries[index];
          settingsCheckedListBox.SetItemChecked(index,
                                                xmlreader.GetValueAsBool(currentSection[0], currentSection[1],
                                                                         bool.Parse(currentSection[2])));
        }

        loglevel = xmlreader.GetValueAsString("general", "loglevel", "3"); // set loglevel to debug
        cbDebug.SelectedIndex = Convert.ToInt16(loglevel);
        screennumber = xmlreader.GetValueAsInt("screenselector", "screennumber", 0);

        while (cbScreen.Items.Count <= screennumber)
        {
          cbScreen.Items.Add("screen nr :" + cbScreen.Items.Count + " (currently unavailable)");
        }
        cbScreen.SelectedIndex = screennumber;

        string prio = xmlreader.GetValueAsString("general", "ThreadPriority", "Normal");
        // Set the selected index, otherwise the SelectedItem in SaveSettings will be null, if the box isn't checked
        mpThreadPriority.SelectedIndex = mpThreadPriority.Items.IndexOf(prio);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        // Save Debug Level
        xmlwriter.SetValue("general", "loglevel", cbDebug.SelectedIndex);
        xmlwriter.SetValue("general", "ThreadPriority", mpThreadPriority.SelectedItem.ToString());
        xmlwriter.SetValue("screenselector", "screennumber", cbScreen.SelectedIndex);

        for (int index = 0; index < sectionEntries.Length; index++)
        {
          string[] currentSection = sectionEntries[index];
          xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
        }
      }

      try
      {
        if (settingsCheckedListBox.GetItemChecked(4)) // autostart on boot
        {
          string fileName = Config.GetFile(Config.Dir.Base, "MediaPortal.exe");
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
          {
            subkey.SetValue("MediaPortal", fileName);
          }
        }
        else
        {
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
          {
            subkey.DeleteValue("MediaPortal", false);
          }
        }

        //Int32 iValue = 1;
        //if (settingsCheckedListBox.GetItemChecked(13)) // disable ballon tips
        //{
        //  iValue = 0;
        //}
        //using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", true))
        //{
        //  subkey.SetValue("EnableBalloonTips", iValue);
        //}

        if (settingsCheckedListBox.GetItemChecked(2)) // always on top
        {
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
          {
            subkey.SetValue("ForegroundLockTimeout", 0);
          }
        }

        IntPtr result = IntPtr.Zero;
        SendMessageTimeout((IntPtr)HWND_BROADCAST, (IntPtr)WM_SETTINGCHANGE, IntPtr.Zero,
                           Marshal.StringToBSTR(string.Empty), (IntPtr)SMTO_ABORTIFHUNG, (IntPtr)3, out result);
      }
      catch (Exception ex)
      {
        Log.Error("General: Exception - {0}", ex);
      }
    }

    private void settingsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (sectionEntries[e.Index][1].Equals("usescreenselector"))
      {
        cbScreen.Enabled = e.NewValue == CheckState.Checked;
      }
    }

    public override void OnSectionActivated()
    {
      mpThreadPriority.Visible = SettingsForm.AdvancedMode;
      labelPriority.Visible = SettingsForm.AdvancedMode;
      base.OnSectionActivated();
    }
  }
}