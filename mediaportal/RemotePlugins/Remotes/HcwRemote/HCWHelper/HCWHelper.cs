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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.Win32;
using UdpHelper;

namespace MediaPortal.InputDevices.HcwHelper
{
  public partial class HcwHelper : Form
  {
    private const int HCWPVR2 = 0x001E; // 45-Button Remote
    private const int HCWPVR = 0x001F; // 34-Button Remote
    private const int HCWCLASSIC = 0x0000; // 21-Button Remote
    private const int WM_TIMER = 0x0113;

    private bool cancelWait = false;
    private bool logVerbose = false;
    private bool hcwEnabled = false;
    private Connection connection;
    private int port = 2110;
    private bool registered = false;
    private bool restartIRApp = false; // Restart Haupp. IR-app. after MP quit

    /// <summary>
    /// Initialization
    /// </summary>
    public HcwHelper()
    {
      InitializeComponent();

      using (Settings xmlreader = new MPSettings())
      {
        logVerbose = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        port = xmlreader.GetValueAsInt("remote", "HCWHelperPort", 2110);
        hcwEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
      }

      connection = new Connection(logVerbose);
      if (hcwEnabled && (GetDllPath() != string.Empty) && connection.Start(port) &&
          irremote.IRSetDllDirectory(GetDllPath()))
      {
        Thread checkThread = new Thread(new ThreadStart(CheckThread));
        checkThread.IsBackground = true;
        checkThread.Priority = ThreadPriority.Highest;
        checkThread.Name = "HcwHelperChecker";
        checkThread.Start();
        connection.ReceiveEvent += new Connection.ReceiveEventHandler(OnReceive);
        StartIR();
      }
      else
      {
        connection.Send(port + 1, "APP", "STOP", DateTime.Now);
        Application.Exit();
      }
    }


    /// <summary>
    /// Let's get out of here
    /// </summary>
    private void Exit()
    {
      if (logVerbose)
      {
        Log.Info("HCWHelper: OnClosing");
      }
      connection.ReceiveEvent -= new Connection.ReceiveEventHandler(OnReceive);
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
      while (!cancelWait &&
             ((Process.GetProcessesByName("MediaPortal").Length > 0) ||
              (Process.GetProcessesByName("MediaPortal.vshost").Length > 0) ||
              Process.GetProcessesByName("Configuration").Length > 0))
      {
        Thread.Sleep(1000);
      }

      if (logVerbose)
      {
        Log.Info("HCWHelper: MediaPortal is not running");
      }
      Exit();
    }


    /// <summary>
    /// Register app with HCW driver
    /// </summary>
    private void StartIR()
    {
      if (registered)
      {
        StopIR();
      }
      if (Process.GetProcessesByName("Ir").Length != 0)
      {
        restartIRApp = true;
        int i = 0;
        while ((Process.GetProcessesByName("Ir").Length != 0) && (i < 15))
        {
          i++;
          if (logVerbose)
          {
            Log.Info("HCWHelper: terminating external control: attempt #{0}", i);
          }
          if (Process.GetProcessesByName("Ir").Length != 0)
          {
            Process.Start(GetHCWPath() + "Ir.exe", "/QUIT");
            Thread.Sleep(500);
          }
        }
        if (Process.GetProcessesByName("Ir").Length != 0)
        {
          foreach (Process proc in Process.GetProcessesByName("Ir"))
          {
            proc.Kill();
          }
        }
      }
      Thread.Sleep(200);
      if (Process.GetProcessesByName("Ir").Length != 0)
      {
        Log.Info("HCWHelper: external control could not be terminated!");
        Exit();
      }
      else if (irremote.IROpen(this.Handle, 0, false, 0))
      {
        registered = true;
      }
      else
      {
        Log.Info("HCWHelper: Can't open IR device - IR in use?");
        Exit();
      }
    }


    /// <summary>
    /// Unregister app from HCW driver
    /// </summary>
    private void StopIR()
    {
      if (irremote.IRClose(this.Handle, 0))
      {
        registered = false;
        if (logVerbose)
        {
          Log.Info("HCWHelper: closing driver successful");
        }
      }
      else
      {
        Log.Info("HCWHelper: Can't close IR device");
      }

      if (restartIRApp)
      {
        try
        {
          if (Process.GetProcessesByName("Ir").Length == 0)
          {
            Process.Start(GetHCWPath() + "Ir.exe", "/QUIET");
          }
        }
        catch (Exception ex)
        {
          Log.Info("HCWHelper: Exception while restarting IR.exe: {0}", ex.Message);
        }
      }
    }


    /// <summary>
    /// Receive Commands from MP
    /// </summary>
    private void OnReceive(string strReceive)
    {
      if (logVerbose)
      {
        Log.Info("HCWHelper: received {0}", strReceive);
      }
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
                  {
                    Process.Start(GetHCWPath() + "Ir.exe", "/QUIET");
                  }
                }
                catch (Exception ex)
                {
                  Log.Info("HCWHelper: Exception: {0}", ex.Message);
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
          int repeatCount = 0;
          int remoteCode = 0;
          int keyCode = 0;
          if (!irremote.IRGetSystemKeyCode(out repeatCount, out remoteCode, out keyCode))
          {
            break;
          }
          DateTime attackTime = DateTime.Now;

          int remoteCommand = 0;
          switch (remoteCode)
          {
            case HCWCLASSIC:
              remoteCommand = keyCode;
              break;
            case HCWPVR:
              remoteCommand = keyCode + 1000;
              break;
            case HCWPVR2:
              remoteCommand = keyCode + 2000;
              break;
          }
          connection.Send(port + 1, "CMD", remoteCommand.ToString(), attackTime);
          if (logVerbose)
          {
            Log.Info("HCWHelper: command sent: {0}", remoteCommand);
          }
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
        using (
          RegistryKey rkey =
            Registry.LocalMachine.OpenSubKey(
              "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Hauppauge WinTV Infrared Remote"))
        {
          dllPath = rkey.GetValue("UninstallString").ToString();
          if (dllPath.IndexOf("UNir32") > 0)
          {
            dllPath = dllPath.Substring(0, dllPath.IndexOf("UNir32"));
          }
          else if (dllPath.IndexOf("UNIR32") > 0)
          {
            dllPath = dllPath.Substring(0, dllPath.IndexOf("UNIR32"));
          }
        }
      }
      catch (NullReferenceException) {}
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
      {
        dllPath = Environment.ExpandEnvironmentVariables("%ProgramFiles%\\WinTV\\");
      }

      if (!File.Exists(dllPath + "irremote.DLL"))
      {
        dllPath = string.Empty;
      }

      if (dllPath == string.Empty)
      {
        Log.Info("HCWHelper: Could not find registry entries for driver components! (Not installed?)");
      }
      return dllPath;
    }
  }
}