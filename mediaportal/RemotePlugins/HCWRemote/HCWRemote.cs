/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Util;


namespace MediaPortal
{
  /// <summary>
  /// Hauppauge HCW remote control class / by mPod
  /// all remotes are supported, if the buttons are defined in the XML file
  /// </summary>
  public class HCWRemote
  {
    bool controlEnabled;            // HCW remote enabled
    bool allowExternal;             // External processes are controlled by the Hauppauge app
    bool keepControl;               // Keep control, if MP loses focus
    bool logVerbose;                // Verbose logging
    int repeatDelay;                // Repeat delay
    bool restartIRApp     = false;  // Restart Haupp. IR-app. after MP quit
    //IntPtr handlerIR;               // Window handler
    DateTime lastTime;              // Timestamp of last execution
    int lastCommand;                // Last executed command
    HCWHandler hcwHandler;
    NetHelper.Connection connection = new NetHelper.Connection();

    const int IR_NOKEY               = 0x1FFF;  // No key received
    const int HCWPVR2                = 0x001E;  // 43-Button Remote
    const int HCWPVR                 = 0x001F;  // 34-Button Remote

    const int WM_TIMER               = 0x0113;
    const int WM_ACTIVATEAPP         = 0x001C;
    const int WM_ACTIVATE            = 0x0006;
    const int WM_POWERBROADCAST      = 0x0218;
    const int WA_INACTIVE            = 0;
    const int WA_ACTIVE              = 1;
    const int WA_CLICKACTIVE         = 2;
      
    const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    const int PBT_APMRESUMECRITICAL  = 0x0006;

    /// <summary>
    /// HCW control enabled
    /// </summary>
    /// <returns>Returns true/false.</returns>
    public bool Enabled { get { return controlEnabled; } }

    /// <summary>
    /// Constructor: Initializes remote control settings
    /// </summary>
    public HCWRemote()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
        allowExternal  = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        keepControl    = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        logVerbose     = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        repeatDelay    = xmlreader.GetValueAsInt ("remote", "HCWDelay", 0);
      }
      bool result = false;
      if (controlEnabled)
        hcwHandler = new HCWHandler("Hauppauge HCW", out result);
      controlEnabled = (controlEnabled && result);

      if (controlEnabled)
      {
        Process.Start(System.Windows.Forms.Application.StartupPath + @"\HCWHelper.exe");

        if (allowExternal)
        {
          Utils.OnStartExternal  += new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal  += new Utils.UtilEventHandler(OnStopExternal);
        }
        if (logVerbose) Log.Write("HCW: Repeat-delay: {0}", repeatDelay);

        //try
        //{
        //  if (!irremote.irremote.IRSetDllDirectory(GetDllPath()))
        //    Log.Write("HCW: Set DLL path failed!");
        //}
        //catch (Exception e)
        //{
        //  if (logVerbose) Log.Write("HCW Exception: SetDllDirectory: " + e.Message);
        //}
      }
      try
      {
        if (Process.GetProcessesByName("Ir").Length != 0)
        {
          restartIRApp = true;
        }
      }
      catch
      {
      }
    }
    
    public void Init(IntPtr hwnd)
    {
      Init();
    }

    /// <summary>
    /// Stop IR.exe and initiate HCW start
    /// </summary>
    public void Init()
    {
      if (!controlEnabled)
        return;

      try
      {
        //if (Process.GetProcessesByName("Ir").Length != 0)
        //{
        //  int i = 0;
        //  while ((Process.GetProcessesByName("Ir").Length != 0) && (i < 15))
        //  {
        //    i++;
        //    if (logVerbose) Log.Write("HCW: Terminating external control: attempt #{0}", i);
        //    if (Process.GetProcessesByName("Ir").Length != 0)
        //    {
        //      Process.Start(GetHCWPath() + "Ir.exe", "/QUIT");
        //      Thread.Sleep(200);
        //    }
        //  }
        //  if (Process.GetProcessesByName("Ir").Length != 0)
        //  {
        //    Log.Write("HCW: External control could not be terminated!");
        //  }
        //}
        

        connection.Connect(2110);
        connection.ReceiveEvent += new NetHelper.Connection.ReceiveEventHandler(OnReceive);
        connection.Send("LOG", logVerbose.ToString());

        //StartHCW();
      }
      catch (Exception e)
      {
        Log.Write("HCW: Failed to start driver components! (Not installed?)");
        if (logVerbose) Log.Write("HCW Exception: StartHCW: " + e.Message);
      }
    }


    /// <summary>
    /// Remove all events
    /// </summary>
    public void DeInit()
    {
      if (controlEnabled)
      {
        if (allowExternal)
        {
          Utils.OnStartExternal -= new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal -= new Utils.UtilEventHandler(OnStopExternal);
        }
      }
      connection.ReceiveEvent -= new NetHelper.Connection.ReceiveEventHandler(OnReceive);
      connection.Send("APP", "SHUTDOWN");
      connection = null;
    }


    /// <summary>
    /// Start HCW control
    /// </summary>
    public void StartHCW()
    {
      connection.Send("APP", "IR_START");
    }


    /// <summary>
    /// Stop HCW control
    /// </summary>
    public void StopHCW()
    {
      connection.Send("APP", "IR_STOP");

      if ((Process.GetProcessesByName("Ir").Length == 0) && (restartIRApp))
      {
        connection.Send("HCWAPP", "START");
      }
    }


    void OnReceive(object sender, NetHelper.Connection.EventArguments e)
    {
      TimeSpan elapsed = DateTime.Now - e.Timestamp;

      switch (e.Message.Split('|')[0])
      {
        case "CMD":
          {
            int remoteCommand = Convert.ToInt16(e.Message.Split('|')[1]);

            if (((lastTime.AddMilliseconds(repeatDelay)) <= e.Timestamp) ||
                      (lastCommand != remoteCommand) ||
                      (repeatDelay == 0))
            {
              lastTime = e.Timestamp;
              lastCommand = remoteCommand;
              hcwHandler.MapAction(remoteCommand);
            }
          }
          break;
        case "LOG":
          {
            Log.Write("HCWHelper: {0}", e.message.Split('|')[1]);
          }
          break;
      }
    }

    /// <summary>
    /// External process (e.g. myPrograms) started
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="waitForExit"></param>
    public void OnStartExternal(Process proc, bool waitForExit)
    {
      StopHCW();
    }


    /// <summary>
    /// External process (e.g. myPrograms) stopped
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="waitForExit"></param>
    public void OnStopExternal(Process proc, bool waitForExit)
    {
      StartHCW();
    }


    /// <summary>
    /// Evaluates received messages and sends them to the mapper
    /// </summary>
    /// <param name="msg">Message</param>
    public void WndProc(Message msg)
    {
      if (controlEnabled)
        switch (msg.Msg)
        {
          case WM_POWERBROADCAST:
            if (msg.WParam.ToInt32() == PBT_APMRESUMEAUTOMATIC)
              StartHCW();
            break;

          case WM_ACTIVATE:
            if (allowExternal && !keepControl)
            {
              switch ((int)msg.WParam)
              {
                case WA_INACTIVE:
                  StopHCW();
                  break;
                case WA_ACTIVE:
                case WA_CLICKACTIVE:
                  StartHCW();
                  break;
              }
            }
            break;
       }
    }
  }
}