/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using irremote;

namespace HCWHelper
{
  public partial class HCWHelper : Form
  {
    const int HCWPVR2 = 0x001E;  // 43-Button Remote
    const int HCWPVR = 0x001F;  // 34-Button Remote
    const int WM_TIMER = 0x0113;

    private bool cancelWait = false;
    private bool logVerbose = false;

    private static NetHelper.Connection connection = new NetHelper.Connection();
    private irremote.irremote irremote = new irremote.irremote();


    /// <summary>
    /// Initialization
    /// </summary>
    public HCWHelper()
    {
      InitializeComponent();
      notifyIcon.Visible = true;

      if (connection.Connect(2110))
      {
        irremote.IRSetDllDirectory(GetDllPath());

        Thread waitThread = new Thread(new ThreadStart(WaitForConnect));
        waitThread.Start();

        Thread checkThread = new Thread(new ThreadStart(CheckThread));
        checkThread.IsBackground = true;
        checkThread.Start();

        connection.ReceiveEvent += new NetHelper.Connection.ReceiveEventHandler(OnReceive);
      }
      else
      {
        notifyIcon.Icon = notifyIconRed.Icon;
        this.Close();
      }
    }

    private void CheckThread()
    {
      while (Process.GetProcessesByName("MediaPortal").Length > 0)
      {
        Thread.Sleep(1000);
      }
      notifyIcon.Icon = notifyIconRed.Icon;
      Application.Exit();
    }

    /// <summary>
    /// MP-Log
    /// </summary>
    /// <param name="entry"></param>
    private static void Log(string entry)
    {
      connection.Send("LOG", entry);
    }


    /// <summary>
    /// Wait for MP to connect
    /// </summary>
    /// <param name="winHandle"></param>
    private void WaitForConnect()
    {
      if (!connection.IsOnline)
      {
        notifyIcon.Icon = notifyIconRed.Icon;
        Application.Exit();
      }
      else
      {
        notifyIcon.Icon = notifyIconYellow.Icon;
        do
          Thread.Sleep(200);
        while (!cancelWait && !connection.IsConnected);
        if (!cancelWait)
          StartIR();
      }
    }


    /// <summary>
    /// Register app with HCW driver
    /// </summary>
    private void StartIR()
    {
      if (Process.GetProcessesByName("Ir").Length != 0)
      {
        int i = 0;
        while ((Process.GetProcessesByName("Ir").Length != 0) && (i < 15))
        {
          i++;
          if (logVerbose)
            Log(string.Format("Terminating external control: attempt #{0}", i));
          if (Process.GetProcessesByName("Ir").Length != 0)
          {
            Process.Start(GetHCWPath() + "Ir.exe", "/QUIT");
            Thread.Sleep(200);
          }
        }
        if (Process.GetProcessesByName("Ir").Length != 0)
          Log("External control could not be terminated!");
      }

      if (!irremote.IROpen(this.Handle, 0, false, 0))
      {
        notifyIcon.Icon = notifyIconRed.Icon;
        Log("Connect to IR failed");
        Application.Exit();
      }
      else
        notifyIcon.Icon = notifyIconGreen.Icon;
    }


    /// <summary>
    /// Unregister app with HCW driver
    /// </summary>
    private void StopIR()
    {
      try
      {
        irremote.IRClose(this.Handle, 0);
      }
      catch
      {
      }
      notifyIcon.Icon = notifyIconYellow.Icon;
    }


    /// <summary>
    /// Receive Commands from MP
    /// </summary>
    private void OnReceive(object sender, NetHelper.Connection.EventArguments e)
    {
      foreach (string msg in e.Message.Split('~'))
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
                this.Close();
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
                  if (logVerbose) Log("Exception: " + ex.Message);
                }
                break;
            }
            break;

          case "LOG":
            logVerbose = System.Convert.ToBoolean(msg.Split('|')[1]);
            break;
        }
        Thread.Sleep(100);
      }
    }


    /// <summary>
    /// Clean up the mess
    /// </summary>
    private void Sender_FormClosing(object sender, FormClosingEventArgs e)
    {
      cancelWait = true;
      StopIR();
      connection.ReceiveEvent -= new NetHelper.Connection.ReceiveEventHandler(OnReceive);
      connection = null;
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
            if (irremote.IRGetSystemKeyCode(ref repeatCount, ref remoteCode, ref keyCode))
            {
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
              if (connection.IsConnected)
                connection.Send("CMD", remoteCommand.ToString());
            }
          }
          catch (Exception)
          {
          }
          break;
      }
      base.WndProc(ref msg);
    }


    /// <summary>
    /// Exit (Context Menu)
    /// </summary>
    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }


    /// <summary>
    /// Status (Context Menu)
    /// </summary>
    private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
      if (connection != null)
      {
        statusOfflineToolStripMenuItem.Text = "Status: ";
        peerStatusToolStripMenuItem.Text = "Peer   : ";
        if (connection.IsOnline)
          statusOfflineToolStripMenuItem.Text += "Online (";
        else
          statusOfflineToolStripMenuItem.Text += "Offline (";
        if (connection.IsServer)
          statusOfflineToolStripMenuItem.Text += "Server)";
        else
          statusOfflineToolStripMenuItem.Text += "Client)";
        if (connection.IsConnected)
          peerStatusToolStripMenuItem.Text += "Connected";
        else
          peerStatusToolStripMenuItem.Text += "Disconnected";
      }
    }


    /// <summary>
    /// Get the Hauppauge IR components installation path from the windows registry.
    /// </summary>
    /// <returns>Installation path of the Hauppauge IR components</returns>
    private string GetHCWPath()
    {
      string dllPath = null;
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
        Log("Could not find registry entries for driver components! (Not installed?)");
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
      if (!File.Exists(dllPath + "irremote.DLL"))
        dllPath = null;
      return dllPath;
    }

  }
}