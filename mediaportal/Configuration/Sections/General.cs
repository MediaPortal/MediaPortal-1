#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using System.Runtime.InteropServices;
using Microsoft.DirectX.Direct3D;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class General : MediaPortal.Configuration.SectionSettings
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
    public extern static bool EnumDisplayDevices(string lpDevice,
      int iDevNum, [In, Out] DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

    const int WM_SETTINGCHANGE = 0x1A;
    const int SMTO_ABORTIFHUNG = 0x2;
    const int HWND_BROADCAST = 0xFFFF;

    public General()
      : this("General") { }

    public General(string name)
      : base(name)
    {
      InitializeComponent();
    }

    string loglevel = "3";  // Debug is default
    int screennumber = 0;   // 0 is the primary screen

    string[][] sectionEntries = new string[][] { 
      new string[] { "general", "startfullscreen", "false" },
      new string[] { "general", "minimizeonstartup", "false" },
      new string[] { "general", "minimizeonexit", "false" },
      new string[] { "general", "autohidemouse", "true" },
	  new string[] { "general", "mousesupport", "false" }, 
      new string[] { "general", "hideextensions", "true" },
      new string[] { "general", "animations", "true" },
	  new string[] { "general", "autostart", "false" },
	  new string[] { "general", "baloontips", "false" },
	  new string[] { "general", "dblclickasrightclick", "false" },
	  new string[] { "general", "hidetaskbar", "false" },
	  new string[] { "general", "alwaysontop", "false" },
	 	  new string[] { "general", "enableguisounds", "true" },
	  new string[] { "general", "screensaver", "false" },
      new string[] { "general", "turnoffmonitor", "false" },
	  new string[] { "general", "startbasichome", "false" },
      new string[] { "general", "turnmonitoronafterresume", "false" },
      new string[] { "general", "enables3trick","true" },
      new string[] { "general", "autosize", "false" },
      // new string[] { "general", "userenderthread", "true" }
      //new string[] { "general", "allowfocus", "false" }
      new string[] { "general","usevrm9forwebstreams","false" },
      new string[] { "general","showlastactivemodule","false" },
      new string[] { "screenselector","usescreenselector","false" }
      };

    // PLEASE NOTE: when adding items, adjust the box so it doesn't get scrollbars
    //              AND be aware that "allowfocus" has to be last item in the list
    //              AND be careful cause depending on where you add a setting, the indexes might change
    //              (e.g. SaveSettings depends on the index!!!)


    /// <summary>
    /// 
    /// </summary>
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
          monitorname = info.DeviceString;
        if (monitorname == null)
          monitorname = "";
        
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        //
        // Load general settings
        //
        for (int index = 0; index < sectionEntries.Length; index++)
        {
          string[] currentSection = sectionEntries[index];
          settingsCheckedListBox.SetItemChecked(index, xmlreader.GetValueAsBool(currentSection[0], currentSection[1], bool.Parse(currentSection[2])));
        }

        loglevel = xmlreader.GetValueAsString("general", "loglevel", "3");
        cbDebug.SelectedIndex = Convert.ToInt16(loglevel);
        screennumber = xmlreader.GetValueAsInt("screenselector", "screennumber", 0);

        while (cbScreen.Items.Count <= screennumber)
        {
          cbScreen.Items.Add("screen nr :" + cbScreen.Items.Count + " (currently unavailable)");
        }
        cbScreen.SelectedIndex = screennumber;

        string prio = xmlreader.GetValueAsString("MP", "ThreadPriority", "Normal");
        // Set the selected index, otherwise the SelectedItem in SaveSettings will be null, if the box isn't checked
        mpThreadPriority.SelectedIndex = mpThreadPriority.Items.IndexOf(prio);

        //numericUpDown1.Value=xmlreader.GetValueAsInt("vmr9OSDSkin","alphaValue",10);

        //// Allow Focus
        //using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
        //  settingsCheckedListBox.SetItemChecked(settingsCheckedListBox.Items.Count - 1, ((int)subkey.GetValue("ForegroundLockTimeout", 2000000) == 0));
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        // Save Debug Level
        xmlwriter.SetValue("general", "loglevel", cbDebug.SelectedIndex);
        xmlwriter.SetValue("MP", "ThreadPriority", mpThreadPriority.SelectedItem.ToString());
        xmlwriter.SetValue("screenselector", "screennumber", cbScreen.SelectedIndex);
        //
        // Load general settings
        //
        for (int index = 0; index < sectionEntries.Length; index++)  // Leave out last setting (focus)!
        {
          string[] currentSection = sectionEntries[index];
          xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
        }
        //xmlwriter.SetValue("vmr9OSDSkin","alphaValue",numericUpDown1.Value);
      }

      try
      {
        if (settingsCheckedListBox.GetItemChecked(7))
        {
          string fileName = Config.GetFile(Config.Dir.Base, "MediaPortal.exe");
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            subkey.SetValue("MediaPortal", fileName);
        }
        else
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            subkey.DeleteValue("MediaPortal", false);

        Int32 iValue = 1;
        if (settingsCheckedListBox.GetItemChecked(8))
          iValue = 0;

        using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", true))
          subkey.SetValue("EnableBalloonTips", iValue);

        if (settingsCheckedListBox.GetItemChecked(settingsCheckedListBox.Items.Count - 2))
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            subkey.SetValue("ForegroundLockTimeout", 0);

        //// Allow Focus
        //using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
        //{
        //  bool focusChecked = ((int)subkey.GetValue("ForegroundLockTimeout", 200000) == 0);

        //  if (focusChecked != settingsCheckedListBox.GetItemChecked(18))
        //    if (settingsCheckedListBox.GetItemChecked(settingsCheckedListBox.Items.Count - 1))
        //      subkey.SetValue("ForegroundLockTimeout", 0);
        //    else
        //      subkey.SetValue("ForegroundLockTimeout", 200000);
        //}

        IntPtr result = IntPtr.Zero;
        SendMessageTimeout((IntPtr)HWND_BROADCAST, (IntPtr)WM_SETTINGCHANGE, IntPtr.Zero, Marshal.StringToBSTR(string.Empty), (IntPtr)SMTO_ABORTIFHUNG, (IntPtr)3, out result);
      }
      catch (Exception ex)
      {
        Log.Info("Exception: {0}", ex.Message);
        Log.Info("Exception: {0}", ex);
        Log.Info("Exception: {0}", ex.StackTrace);
      }
    }

    private void settingsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {      
      if (sectionEntries[e.Index][1].Equals("usescreenselector"))
        cbScreen.Enabled = e.NewValue == CheckState.Checked;
    }
  }
}
