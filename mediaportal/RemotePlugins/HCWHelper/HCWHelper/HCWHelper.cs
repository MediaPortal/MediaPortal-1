#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using MediaPortal.GUI.Library;

namespace MediaPortal.InputDevices.HcwHelper
{
  public partial class HcwHelper : Form
  {
    const int HCWPVR2 = 0x001E;  // 43-Button Remote
    const int HCWPVR = 0x001F;  // 34-Button Remote
    const int WM_TIMER = 0x0113;

    private bool cancelWait = false;
    private bool logVerbose = false;
    private bool hcwEnabled = false;
    private UdpHelper.Connection connection;
    private int port = 2110;
    private bool registered = false;
    bool restartIRApp = false;                // Restart Haupp. IR-app. after MP quit


    /// <summary>
    /// Initialization
    /// </summary>
    public HcwHelper()
    {
      InitializeComponent();

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        logVerbose = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        port = xmlreader.GetValueAsInt("remote", "HCWHelperPort", 2110);
        hcwEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
      }

      connection = new UdpHelper.Connection(logVerbose);
      if (hcwEnabled && (GetDllPath() != string.Empty) && connection.Start(port) && irremote.IRSetDllDirectory(GetDllPath()))
      {
        Thread checkThread = new Thread(new ThreadStart(CheckThread));
        checkThread.IsBackground = true;
        checkThread.Priority = ThreadPriority.Highest;
        checkThread.Start();
        connection.ReceiveEvent += new UdpHelper.Connection.ReceiveEventHandler(OnReceive);
        StartIR();
      }
      else
      {
        connection.Send(port + 1, "APP", "STOP", DateTime.Now);
        connection = null;
        Application.Exit();
      }
    }


    /// <summary>
    /// Let's get out of here
    /// </summary>
    private void Exit()
    {
      if (logVerbose) Log.Write("HCWHelper: OnClosing");
      connection.ReceiveEvent -= new UdpHelper.Connection.ReceiveEventHandler(OnReceive);
      connection.Send(port + 1, "APP", "STOP", DateTime.Now);
      connection = null;
      StopIR();
      Application.Exit();
    }


    /// <summary>
    /// Checks if there's a running MP instance
    /// </summary>
    private void CheckThread()
    {
      while (!cancelWait && ((Process.GetProcessesByName("MediaPortal").Length > 0) || (Process.GetProcessesByName("MediaPortal.vshost").Length > 0)))
        Thread.Sleep(1000);

      if (logVerbose) Log.Write("HCWHelper: MediaPortal is not running");
      Exit();
    }


    /// <summary>
    /// Register app with HCW driver
    /// </summary>
    private void StartIR()
    {
      if (registered)
        StopIR();
      if (Process.GetProcessesByName("Ir").Length != 0)
      {
        restartIRApp = true;
        int i = 0;
        while ((Process.GetProcessesByName("Ir").Length != 0) && (i < 15))
        {
          i++;
          if (logVerbose) Log.Write("HCWHelper: terminating external control: attempt #{0}", i);
          if (Process.GetProcessesByName("Ir").Length != 0)
          {
            Process.Start(GetHCWPath() + "Ir.exe", "/QUIT");
            Thread.Sleep(500);
          }
        }
        if (Process.GetProcessesByName("Ir").Length != 0)
          foreach (Process proc in Process.GetProcessesByName("Ir"))
            proc.Kill();
      }
      Thread.Sleep(200);
      if (Process.GetProcessesByName("Ir").Length != 0)
      {
        Log.Write("HCWHelper: external control could not be terminated!");
        Exit();
      }
      else
        try
        {
          irremote.IROpen(this.Handle, 0, false, 0);
          registered = true;
        }
        catch (irremote.IRFailedException ex)
        {
          Log.Write("HCWHelper: (open) {0}", ex.Message);
          Exit();
        }
    }


    /// <summary>
    /// Unregister app from HCW driver
    /// </summary>
    private void StopIR()
    {
      try
      {
        irremote.IRClose(this.Handle, 0);
        registered = false;
        if (logVerbose) Log.Write("HCWHelper: closing driver successful");
      }
      catch (irremote.IRFailedException ex)
      {
        Log.Write("HCWHelper: (close) {0}", ex.Message);
      }
      if (restartIRApp)
        try
        {
          if (Process.GetProcessesByName("Ir").Length == 0)
            Process.Start(GetHCWPath() + "Ir.exe", "/QUIET");
        }
        catch (Exception ex)
        {
          Log.Write("HCWHelper: Exception while restarting IR.exe: {0}", ex.Message);
        }
    }


    /// <summary>
    /// Receive Commands from MP
    /// </summary>
    private void OnReceive(string strReceive)
    {
      if (logVerbose) Log.Write("HCWHelper: received {0}", strReceive);
      foreach (string msg in strReceive.Split('~'))
      {
        switch (msg.Split('|')[0])
        {
          case "APP":
            switch (msg.Split('|')[1])
            {
              case "IR_STOP":
                StopIR();
                break;

              case "IR_START":
                StartIR();
                break;

              case "SHUTDOWN":
                Exit();
                break;
            }
            break;

          case "HCWAPP":
            switch (msg.Split('|')[1])
            {
              case "START":
                try
                {
                  StopIR();
                  if (Process.GetProcessesByName("Ir").Length == 0)
                    Process.Start(GetHCWPath() + "Ir.exe", "/QUIET");
                }
                catch (Exception ex)
                {
                  Log.Write("HCWHelper: Exception: {0}", ex.Message);
                }
                break;
            }
            break;
        }
      }
    }


    /// <summary>
    /// Receive Messages
    /// </summary>
    /// <param name="msg">Message</param>
    protected override void WndProc(ref Message msg)
    {
      switch (msg.Msg)
      {
        case WM_TIMER:
          IntPtr repeatCount = new IntPtr();
          IntPtr remoteCode = new IntPtr();
          IntPtr keyCode = new IntPtr();
          try
          {
            irremote.IRGetSystemKeyCode(ref repeatCount, ref remoteCode, ref keyCode);
          }
          catch (irremote.IRNoMessage)
          {
            break;
          }
          DateTime attackTime = DateTime.Now;

          int remoteCommand = 0;
          switch ((int)remoteCode)
          {
            case HCWPVR:
              remoteCommand = ((int)keyCode) + 1000;
              break;
            case HCWPVR2:
              remoteCommand = ((int)keyCode) + 2000;
              break;
          }
          connection.Send(port + 1, "CMD", remoteCommand.ToString(), attackTime);
          if (logVerbose) Log.Write("HCWHelper: command sent: {0}", remoteCommand);
          break;
      }
      base.WndProc(ref msg);
    }


    /// <summary>
    /// Get the Hauppauge IR components installation path from the windows registry.
    /// </summary>
    /// <returns>Installation path of the Hauppauge IR components</returns>
    private string GetHCWPath()
    {
      string dllPath = string.Empty;
      try
      {
        RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Hauppauge WinTV Infrared Remote");
        dllPath = rkey.GetValue("UninstallString").ToString();
        if (dllPath.IndexOf("UNir32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNir32"));
        else if (dllPath.IndexOf("UNIR32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNIR32"));
      }
      catch (System.NullReferenceException)
      {
      }
      return dllPath;
    }


    /// <summary>
    /// Returns the path of the DLL component
    /// </summary>
    /// <returns>DLL path</returns>
    private string GetDllPath()
    {
      string dllPath = GetHCWPath();
      if (dllPath == string.Empty)
        dllPath = System.Environment.ExpandEnvironmentVariables("%ProgramFiles%\\WinTV\\");

      if (!File.Exists(dllPath + "irremote.DLL"))
        dllPath = string.Empty;

      if (dllPath == string.Empty)
        Log.Write("HCWHelper: Could not find registry entries for driver components! (Not installed?)");
      return dllPath;
    }

  }
}