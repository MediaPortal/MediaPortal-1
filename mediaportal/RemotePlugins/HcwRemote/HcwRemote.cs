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
using System.Diagnostics;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Threading;
using System.Collections;
using System.IO;


namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Hauppauge HCW remote control class / by mPod
  /// all remotes are supported, if the buttons are defined in the XML file
  /// </summary>
  public class HcwRemote
  {
    bool controlEnabled = false;       // HCW remote enabled
    bool allowExternal = false;        // External processes are controlled by the Hauppauge app
    bool keepControl = false;          // Keep control, if MP loses focus
    bool logVerbose = false;           // Verbose logging
    DateTime lastTime = DateTime.Now;  // Timestamp of last recieved keycode from remote
    int sameCommandCount = 0;          // Counts how many times a button has been pressed (used to get progressive repetition delay, first time, long delay, then short delay)
    int lastExecutedCommandCount = 0;
    int lastCommand = 0;               // Last executed command
    InputHandler hcwHandler;
    UdpHelper.Connection connection = null;
    bool exit = false;
    int port = 2110;

    TimeSpan buttonRelease;
    int repeatFilter = 0;
    int repeatSpeed = 0;
    bool filterDoubleKlicks = false;

    const int WM_ACTIVATE = 0x0006;
    const int WM_POWERBROADCAST = 0x0218;
    const int WA_INACTIVE = 0;
    const int WA_ACTIVE = 1;
    const int WA_CLICKACTIVE = 2;

    const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    const int PBT_APMRESUMECRITICAL = 0x0006;


    /// <summary>
    /// HCW control enabled
    /// </summary>
    /// <returns>Returns true/false.</returns>
    public bool Enabled { get { return controlEnabled; } }


    /// <summary>
    /// Constructor
    /// </summary>
    public HcwRemote()
    {
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
      exit = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
        allowExternal = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        keepControl = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        logVerbose = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        buttonRelease = TimeSpan.FromMilliseconds(xmlreader.GetValueAsInt("remote", "HCWButtonRelease", 200));
        repeatFilter = xmlreader.GetValueAsInt("remote", "HCWRepeatFilter", 2);
        repeatSpeed = xmlreader.GetValueAsInt("remote", "HCWRepeatSpeed", 0);
        filterDoubleKlicks = xmlreader.GetValueAsBool("remote", "HCWFilterDoubleKlicks", false);
        port = xmlreader.GetValueAsInt("remote", "HCWHelperPort", 2110);
      }
      if (controlEnabled)
      {
        string exePath = irremote.GetHCWPath();
        string dllPath = irremote.GetDllPath();

        bool hcwDriverUpToDate = true;

        if (File.Exists(exePath + "Ir.exe"))
        {
          FileVersionInfo exeVersionInfo = FileVersionInfo.GetVersionInfo(exePath + "Ir.exe");
          if (exeVersionInfo.FileVersion.CompareTo(irremote.CurrentVersion) < 0)
            hcwDriverUpToDate = false;
        }

        if (File.Exists(dllPath + "irremote.DLL"))
        {
          FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath + "irremote.DLL");
          if (dllVersionInfo.FileVersion.CompareTo(irremote.CurrentVersion) < 0)
            hcwDriverUpToDate = false;
        }

        if (!hcwDriverUpToDate)
        {
          Log.Write("HCW: ==============================================================================================");
          Log.Write("HCW: Your remote control driver components are not up to date! To avoid problems, you should");
          Log.Write("HCW: get the latest Hauppauge drivers here: http://www.hauppauge.co.uk/board/showthread.php?p=25253");
          Log.Write("HCW: ==============================================================================================");
        }

        try
        {
          hcwHandler = new InputHandler("Hauppauge HCW");
        }
        catch (System.IO.FileNotFoundException)
        {
          controlEnabled = false;
          Log.Write("HCW: can't find default mapping file - reinstall MediaPortal");
        }
        catch (System.Xml.XmlException)
        {
          controlEnabled = false;
          Log.Write("HCW: error in default mapping file - reinstall MediaPortal");
        }
        catch (System.ApplicationException)
        {
          controlEnabled = false;
          Log.Write("HCW: version mismatch in default mapping file - reinstall MediaPortal");
        }
      }

      if (controlEnabled)
      {
        connection = new UdpHelper.Connection(logVerbose);

        connection.Start(port + 1);
        connection.ReceiveEvent += new UdpHelper.Connection.ReceiveEventHandler(OnReceive);

        Process process = Process.GetCurrentProcess();
        Log.Write("Process: {0}", process.ProcessName);

        Process procHelper = new Process();
        procHelper.StartInfo.FileName = string.Format("{0}\\HcwHelper.exe", System.Windows.Forms.Application.StartupPath);
        procHelper.Start();
        if (allowExternal)
        {
          Log.Write("HCW: AllowExternal");
          Utils.OnStartExternal += new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal += new Utils.UtilEventHandler(OnStopExternal);
        }
        Thread checkThread = new Thread(new ThreadStart(CheckThread));
        checkThread.IsBackground = true;
        checkThread.Priority = ThreadPriority.Highest;
        checkThread.Start();
      }
    }


    /// <summary>
    /// Makes sure that HCWHelper is always running when we need it
    /// </summary>
    private void CheckThread()
    {
      do
      {
        Thread.Sleep(1000);

        while (!exit && (Process.GetProcessesByName("HcwHelper").Length > 0))
          Thread.Sleep(1000);

        if (!exit)
        {
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            controlEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
          if (controlEnabled)
            Process.Start(System.Windows.Forms.Application.StartupPath + @"\HcwHelper.exe");
          else
            exit = true;
        }
      }
      while (!exit);
    }


    /// <summary>
    /// DeInit all
    /// </summary>
    public void DeInit()
    {
      if (controlEnabled)
      {
        exit = true;
        if (allowExternal)
        {
          Utils.OnStartExternal -= new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal -= new Utils.UtilEventHandler(OnStopExternal);
        }
        connection.ReceiveEvent -= new UdpHelper.Connection.ReceiveEventHandler(OnReceive);
        connection.Send(port, "APP", "SHUTDOWN", DateTime.Now);
        connection.Stop();
        connection = null;
      }
    }


    /// <summary>
    /// Start HCW control
    /// </summary>
    public void StartHcw()
    {
      connection.Send(port, "APP", "IR_START", DateTime.Now);
    }


    /// <summary>
    /// Stop HCW control
    /// </summary>
    public void StopHcw()
    {
      connection.Send(port, "APP", "IR_STOP", DateTime.Now);
    }


    void OnReceive(string strReceive)
    {
      if (logVerbose) Log.Write("HCW: received: {0}", strReceive);

      string msg = strReceive.Split('~')[0];
      if (logVerbose) Log.Write("HCW: Accepted: {0}", msg);
      switch (msg.Split('|')[0])
      {
        case "CMD":
          {
            // Time of button press - Use this for repeat delay calculations
            DateTime sentTime = DateTime.FromBinary(Convert.ToInt64(msg.Split('|')[2]));
            int newCommand = Convert.ToInt16(msg.Split('|')[1]);

            if (logVerbose) Log.Write("HCW: elapsed time: {0}", ((TimeSpan)(sentTime - lastTime)).Milliseconds);
            if (logVerbose) Log.Write("HCW: sameCommandCount: {0}", sameCommandCount.ToString());

            if (lastCommand == newCommand)
            {
              // button release time elapsed since last identical command
              // if so, reset counter & start new session
              if ((sentTime - lastTime) > buttonRelease)
              {
                sameCommandCount = 0;   // new session with this button
                if (logVerbose) Log.Write("HCW: same command, timeout true");
              }
              else
              {
                if (logVerbose) Log.Write("HCW: same command, timeout false");
                sameCommandCount++;   // button release time not elapsed
              }
            }
            else
              sameCommandCount = 0;   // we got a new button

            bool executeKey = false;

            // new button / session
            if (sameCommandCount == 0)
              executeKey = true;

            //// we got the identical button often enough to accept it
            if (sameCommandCount == repeatFilter)
              executeKey = true;

            // we got the identical button accepted and still pressed, repeat with repeatSpeed
            if ((sameCommandCount > repeatFilter) && (sameCommandCount > lastExecutedCommandCount + repeatSpeed))
              executeKey = true;

            // double click filter
            if (executeKey && filterDoubleKlicks)
            {
              int keyCode = newCommand;

              // strip remote type
              if (keyCode > 2000)
                keyCode = keyCode - 2000;
              else if (keyCode > 1000)
                keyCode = keyCode - 1000;

              if ((sameCommandCount > 0) &&
                (keyCode == 46 || //46 = fullscreen/green button
                keyCode == 37 ||  //37 = OK button
                keyCode == 56 ||  //56 = yellow button
                keyCode == 11 ||  //11 = red button
                keyCode == 41 ||  //41 = blue button
                keyCode == 13 ||  //13 = menu button
                keyCode == 15 ||  //15 = mute button
                keyCode == 48))   //48 = pause button
              {
                executeKey = false;
                if (logVerbose) Log.Write("HCW: doubleclick supressed: {0}", newCommand.ToString());
              }
            }

            if (executeKey)
            {
              lastExecutedCommandCount = sameCommandCount;
              lastCommand = newCommand;
              try
              {
                hcwHandler.MapAction(newCommand);    //Send command to application...
              }
              catch (ApplicationException ex)
              {
                Log.Write("HCW: Exception: {0}", ex.Message);
                Log.Write("HCW: Exception: {1}", ex.InnerException);
              }
              if (logVerbose) Log.Write("HCW: repeat filter accepted: {0}", newCommand.ToString());
            }
            lastTime = sentTime;
          }
          break;
        case "APP":
          if (msg.Split('|')[1] == "STOP")
          {
            if (logVerbose) Log.Write("HCW: received STOP from HcwHelper");
            controlEnabled = false;
            exit = true;
            StopHcw();
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
      StopHcw();
    }


    /// <summary>
    /// External process (e.g. myPrograms) stopped
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="waitForExit"></param>
    public void OnStopExternal(Process proc, bool waitForExit)
    {
      StartHcw();
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
              StartHcw();
            break;

          case WM_ACTIVATE:
            if (allowExternal && !keepControl)
              switch ((int)msg.WParam)
              {
                case WA_INACTIVE:
                  if (logVerbose) Log.Write("HCW: lost focus");
                  StopHcw();
                  break;
                case WA_ACTIVE:
                case WA_CLICKACTIVE:
                  if (logVerbose) Log.Write("HCW: got focus");
                  StartHcw();
                  break;
              }
            break;
        }
    }

  }
}