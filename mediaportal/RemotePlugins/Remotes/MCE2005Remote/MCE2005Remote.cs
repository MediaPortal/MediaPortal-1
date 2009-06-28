#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Hardware;
using MediaPortal.Profile;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// MCE 2005 remote plugin
  /// </summary>
  public class MCE2005Remote
  {
    private bool controlEnabled = false; // MCE Remote enabled
    private bool logVerbose = false; // Verbose logging
    private InputHandler _inputHandler; // Input Mapper


    /// <summary>
    /// Constructor
    /// </summary>
    public MCE2005Remote()
    {
    }


    /// <summary>
    /// Initialize MCE Remote
    /// </summary>
    /// <param name="hwnd">Window Handle</param>
    public void Init(IntPtr hwnd)
    {
      Init();
    }


    /// <summary>
    /// Initialize MCE Remote
    /// </summary>
    private void Init()
    {
      using (Settings xmlreader = new MPSettings())
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "MCE", true);
        logVerbose = xmlreader.GetValueAsBool("remote", "MCEVerboseLog", false);
      }
      if (!controlEnabled)
      {
        return;
      }

      if (logVerbose)
      {
        Log.Info("MCE: Initializing MCE remote");
      }

      try
      {
        Remote.LogVerbose = logVerbose;
        // Register Device
        Remote.Click = null;
        Remote.Click += new RemoteEventHandler(OnRemoteClick);
        Remote.DeviceRemoval += new DeviceEventHandler(OnDeviceRemoval);
      }
      catch (Exception ex)
      {
        controlEnabled = false;
        Log.Info("MCE: {0} - support disabled until MP restart", ex.InnerException.Message);
        return;
      }

      // Kill ehtray.exe since that program catches the MCE remote keys and would start MCE 2005
      Process[] myProcesses;
      myProcesses = Process.GetProcesses();
      foreach (Process myProcess in myProcesses)
      {
        if (myProcess.ProcessName.ToLower().Equals("ehtray"))
        {
          try
          {
            Log.Info("MCE: Stopping Microsoft ehtray");
            myProcess.Kill();
          }
          catch (Exception)
          {
            Log.Info("MCE: Cannot stop Microsoft ehtray");
            DeInit();
            return;
          }
        }
      }

      _inputHandler = new InputHandler("Microsoft MCE");
      if (!_inputHandler.IsLoaded)
      {
        Log.Info("MCE: Error loading default mapping file - please reinstall MediaPortal");
        DeInit();
        return;
      }
      else
      {
        Log.Info("MCE: MCE remote enabled");
      }
    }


    /// <summary>
    /// Remove all device handling
    /// </summary>
    public void DeInit()
    {
      if (controlEnabled)
      {
        if (logVerbose)
        {
          Log.Info("MCE: Stopping MCE remote");
        }
        Remote.Click -= new RemoteEventHandler(OnRemoteClick);
        Remote.DeviceRemoval -= new DeviceEventHandler(OnDeviceRemoval);
        Remote.DeviceArrival -= new DeviceEventHandler(OnDeviceArrival);
        _inputHandler = null;
        controlEnabled = false;
      }
    }

    private void OnDeviceRemoval(object sender, EventArgs e)
    {
      Remote.DeviceRemoval -= new DeviceEventHandler(OnDeviceRemoval);
      Remote.DeviceArrival += new DeviceEventHandler(OnDeviceArrival);
      Log.Info("MCE: MCE receiver has been unplugged");
    }

    private void OnDeviceArrival(object sender, EventArgs e)
    {
      Remote.DeviceArrival -= new DeviceEventHandler(OnDeviceArrival);
      Remote.Click -= new RemoteEventHandler(OnRemoteClick);
      Log.Info("MCE: MCE receiver detected");
      Init();
    }

    /// <summary>
    /// Let everybody know that this HID message may not be handled by anyone else
    /// </summary>
    /// <param name="msg">System.Windows.Forms.Message</param>
    /// <returns>Command handled</returns>
    public bool WndProc(Message msg)
    {
      if (controlEnabled && (msg.Msg == 0x0319))
      {
        int command = (msg.LParam.ToInt32() >> 16) & ~0xF000;
        InputDevices.LastHidRequest = (AppCommands) command;

        RemoteButton button = RemoteButton.None;

        if ((AppCommands) command == AppCommands.VolumeUp)
        {
          button = RemoteButton.VolumeUp;
        }

        if ((AppCommands) command == AppCommands.VolumeDown)
        {
          button = RemoteButton.VolumeDown;
        }

        if (button != RemoteButton.None)
        {
          // Get & execute Mapping
          if (_inputHandler.MapAction((int) button))
          {
            if (logVerbose)
            {
              Log.Info("MCE: Command \"{0}\" mapped", button);
            }
          }
          else if (logVerbose)
          {
            Log.Info("MCE: Command \"{0}\" not mapped", button);
          }
        }

        return true;
      }
      return false;
    }


    /// <summary>
    /// Evaluate button press from remote
    /// </summary>
    /// <param name="button">Remote Button</param>
    private void OnRemoteClick(object sender, RemoteEventArgs e)
      //RemoteButton button)
    {
      RemoteButton button = e.Button;
      if (logVerbose)
      {
        Log.Info("MCE: Incoming button command: {0}", button);
      }

      // Set LastHidRequest, otherwise the HID handler (if enabled) would react on some remote buttons (double execution of command)
      switch (button)
      {
        case RemoteButton.Record:
          InputDevices.LastHidRequest = AppCommands.MediaRecord;
          break;
        case RemoteButton.Stop:
          InputDevices.LastHidRequest = AppCommands.MediaStop;
          break;
        case RemoteButton.Pause:
          InputDevices.LastHidRequest = AppCommands.MediaPause;
          break;
        case RemoteButton.Rewind:
          InputDevices.LastHidRequest = AppCommands.MediaRewind;
          break;
        case RemoteButton.Play:
          InputDevices.LastHidRequest = AppCommands.MediaPlay;
          break;
        case RemoteButton.Forward:
          InputDevices.LastHidRequest = AppCommands.MediaFastForward;
          break;
        case RemoteButton.Replay:
          InputDevices.LastHidRequest = AppCommands.MediaPreviousTrack;
          break;
        case RemoteButton.Skip:
          InputDevices.LastHidRequest = AppCommands.MediaNextTrack;
          break;
        case RemoteButton.Back:
          InputDevices.LastHidRequest = AppCommands.BrowserBackward;
          break;
        case RemoteButton.ChannelUp:
          InputDevices.LastHidRequest = AppCommands.MediaChannelUp;
          break;
        case RemoteButton.ChannelDown:
          InputDevices.LastHidRequest = AppCommands.MediaChannelDown;
          break;
        case RemoteButton.Mute:
          InputDevices.LastHidRequest = AppCommands.VolumeMute;
          break;
        case RemoteButton.VolumeUp:
          return; // Don't handle this command, benefit from OS' repeat handling instead
        case RemoteButton.VolumeDown:
          return; // Don't handle this command, benefit from OS' repeat handling instead
      }

      // Get & execute Mapping
      if (_inputHandler.MapAction((int) button))
      {
        if (logVerbose)
        {
          Log.Info("MCE: Command \"{0}\" mapped", button);
        }
      }
      else if (logVerbose)
      {
        Log.Info("MCE: Command \"{0}\" not mapped", button);
      }
    }
  }
}