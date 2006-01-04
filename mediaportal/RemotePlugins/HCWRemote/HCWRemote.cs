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


namespace MediaPortal
{
  /// <summary>
  /// Hauppauge HCW remote control class / by mPod
  /// all remotes are supported, if the buttons are defined in the XML file
  /// </summary>
  public class HCWRemote
  {
    bool controlEnabled;                      // HCW remote enabled
    bool allowExternal;                       // External processes are controlled by the Hauppauge app
    bool keepControl;                         // Keep control, if MP loses focus
    static bool logVerbose;                   // Verbose logging
    bool restartIRApp = false;                // Restart Haupp. IR-app. after MP quit
    static DateTime lastTime = DateTime.Now;  // Timestamp of last recieved keycode from remote
    static int sameCommandCount = 0;          // Counts how many times a button has been pressed (used to get progressive repetition delay, first time, long delay, then short delay)
    static int lastExecutedCommandCount = 0;
    static int lastCommand;                   // Last executed command
    static InputHandler hcwHandler;
    NetHelper.Connection connection;
    static bool exit = false;

    static TimeSpan buttonRelease;
    static int repeatFilter;
    static int repeatSpeed;
    static bool filterDoubleKlicks;

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
    /// Constructor: Initializes remote control settings
    /// </summary>
    public HCWRemote()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
        allowExternal = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        keepControl = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        logVerbose = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        buttonRelease = TimeSpan.FromMilliseconds(xmlreader.GetValueAsInt("remote", "HCWButtonRelease", 100));
        repeatFilter = xmlreader.GetValueAsInt("remote", "HCWRepeatFilter", 2);
        repeatSpeed = xmlreader.GetValueAsInt("remote", "HCWRepeatSpeed", 1);
        filterDoubleKlicks = xmlreader.GetValueAsBool("remote", "HCWFilterDoubleKlicks", false);
      }
      bool result = false;
      if (controlEnabled)
        hcwHandler = new InputHandler("Hauppauge HCW", out result);

      controlEnabled = (controlEnabled && result);
      if (controlEnabled)
      {
        connection = new NetHelper.Connection(logVerbose);
        Process.Start(System.Windows.Forms.Application.StartupPath + @"\HCWHelper.exe");
        if (allowExternal)
        {
          Utils.OnStartExternal += new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal += new Utils.UtilEventHandler(OnStopExternal);
        }
      }
      try
      {
        if (Process.GetProcessesByName("Ir").Length != 0)
          restartIRApp = true;
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
        connection.Connect(2110);
        connection.ReceiveEvent += new NetHelper.Connection.ReceiveEventHandler(OnReceive);
        Thread checkThread = new Thread(new ThreadStart(CheckThread));
        checkThread.IsBackground = true;
        checkThread.Priority = ThreadPriority.Highest;
        checkThread.Start();
      }
      catch (Exception ex)
      {
        Log.Write("HCW: Failed to start driver components! (Not installed?)");
        if (logVerbose) Log.Write("HCW Exception: StartHCW: " + ex.Message);
      }
    }


    private void CheckThread()
    {
      do
      {
        Thread.Sleep(1000);
        while (!exit && (Process.GetProcessesByName("HCWHelper").Length > 0))
        {
          Thread.Sleep(1000);
        }
        if (!exit)
        {
          Process.Start(System.Windows.Forms.Application.StartupPath + @"\HCWHelper.exe");
        }
      }
      while (!exit);
    }


    /// <summary>
    /// Remove all events
    /// </summary>
    public void DeInit()
    {
      exit = true;
      try
      {
        if (controlEnabled && allowExternal)
        {
          Utils.OnStartExternal -= new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal -= new Utils.UtilEventHandler(OnStopExternal);
        }
        connection.ReceiveEvent -= new NetHelper.Connection.ReceiveEventHandler(OnReceive);
        connection.Send("APP", "SHUTDOWN", DateTime.Now);
        connection = null;
      }
      catch (Exception ex)
      {
        Log.Write("HCW: Exception: {0}", ex.Message);
      }
    }


    /// <summary>
    /// Start HCW control
    /// </summary>
    public void StartHCW()
    {
      try
      {
        connection.Send("APP", "IR_START", DateTime.Now);
      }
      catch (Exception ex)
      {
        Log.Write("HCW: Exception: {0}", ex.Message);
      }
    }


    /// <summary>
    /// Stop HCW control
    /// </summary>
    public void StopHCW()
    {
      try
      {
        if (restartIRApp)
        {
          connection.Send("HCWAPP", "START", DateTime.Now);
        }
      }
      catch (Exception ex)
      {
        Log.Write("HCW: Exception: {0}", ex.Message);
      }
    }


    static void OnReceive(NetHelper.Connection.EventArguments e)
    {
      if (logVerbose) Log.Write("HCW: received: {0}", e.Message);

      string msg = e.Message.Split('~')[0];
      if (logVerbose) Log.Write("HCW: Accepted: {0}", msg);
      try
      {
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
                hcwHandler.MapAction(newCommand);    //Send command to application...
                if (logVerbose) Log.Write("HCW: repeat filter accepted: {0}", newCommand.ToString());
              }
              lastTime = sentTime;
            }
            break;
        }
      }
      catch (Exception ex)
      {
        Log.Write("HCW: Exception: {0}", ex.Message);
        Log.Write("HCW: Received: {0} - {1}", msg, e.Message);
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