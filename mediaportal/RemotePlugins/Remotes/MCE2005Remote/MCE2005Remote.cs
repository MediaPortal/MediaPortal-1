#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Hardware;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mapping = MediaPortal.InputDevices.InputHandler.Mapping;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// MCE 2005 remote plugin
  /// </summary>
  public class MCE2005Remote : IInputDevice
  {
    private bool controlEnabled = false; // MCE Remote enabled in config and initialised
    private bool controlActive = false; // MCE Remote currently active
    private bool logVerbose = false; // Verbose logging
    private InputHandler _inputHandler; // Input Mapper


    /// <summary>
    /// Constructor
    /// </summary>
    public MCE2005Remote() {}


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
        controlActive = controlEnabled;
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
        controlActive = false;
        Log.Info("MCE: {0} - support disabled until MP restart", ex.InnerException.Message);
        return;
      }

      // Kill ehtray.exe since that program catches the MCE remote keys and would start MCE 2005
      Process[] myProcesses;
      myProcesses = Process.GetProcesses();
      foreach (Process myProcess in myProcesses)
      {
        if (myProcess.ProcessName.ToLowerInvariant().Equals("ehtray"))
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

      InitInputHandler();
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
        controlActive = false;
      }
    }
    
    /// <summary>
    /// Initialise the input handler
    /// </summary>
    private void InitInputHandler()
    {
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
      if (controlEnabled && (msg.Msg == Win32.Const.WM_APPCOMMAND))
      {
        int command = Win32.Macro.GET_APPCOMMAND_LPARAM(msg.LParam);
        InputDevices.LastHidRequest = (AppCommands)command;

        RemoteButton button = RemoteButton.None;

        if ((AppCommands)command == AppCommands.VolumeUp)
        {
          button = RemoteButton.VolumeUp;
        }

        if ((AppCommands)command == AppCommands.VolumeDown)
        {
          button = RemoteButton.VolumeDown;
        }

        if ((AppCommands)command == AppCommands.MediaChannelUp)
        {
            button = RemoteButton.ChannelUp;
        }

        if ((AppCommands)command == AppCommands.MediaChannelDown)
        {
            button = RemoteButton.ChannelDown;
        }

        if (button != RemoteButton.None)
        {
          // Get & execute Mapping
          if (_inputHandler.MapAction((int)button))
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
          return; // Don't handle this command, benefit from OS' repeat handling instead
        case RemoteButton.ChannelDown:
          return; // Don't handle this command, benefit from OS' repeat handling instead
        case RemoteButton.Mute:
          InputDevices.LastHidRequest = AppCommands.VolumeMute;
          break;
        case RemoteButton.VolumeUp:
          return; // Don't handle this command, benefit from OS' repeat handling instead
        case RemoteButton.VolumeDown:
          return; // Don't handle this command, benefit from OS' repeat handling instead
      }

      // Get & execute Mapping
      if (_inputHandler.MapAction((int)button))
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

    /// <summary>
    /// Required for the IInputDevice interface
    /// </summary>    
    /// <param name="msg"></param>
    /// <param name="action"></param>
    /// <param name="key"></param>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool WndProc(ref System.Windows.Forms.Message msg, out GUI.Library.Action action, out char key, out Keys keyCode)
    {
        action = null;
        key = (char)0;
        keyCode = Keys.A;
        return WndProc(msg);
    }

    /// <summary>
    /// Load the mapping for this message
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public Mapping GetMapping(Message msg)
    {
        if (controlEnabled && (msg.Msg == Win32.Const.WM_APPCOMMAND))
        {
            AppCommands command = (AppCommands)Win32.Macro.GET_APPCOMMAND_LPARAM(msg.LParam);
            return MapFromAppCommand(command);
        }
        return null;
    }

    /// <summary>
    /// Map an app command to the MCE RemoteButton
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private Mapping MapFromAppCommand(AppCommands command)
    {
        Mapping result = null;
        RemoteButton button = RemoteButton.None;
        switch (command)
        {
            case AppCommands.BrowserBackward:
                button = RemoteButton.Back;
                break;
            case AppCommands.VolumeMute:
                button = RemoteButton.Mute;
                break;
            case AppCommands.VolumeDown:
                button = RemoteButton.VolumeDown;
                break;
            case AppCommands.VolumeUp:
                button = RemoteButton.VolumeUp;
                break;
            case AppCommands.MediaNextTrack:
                button = RemoteButton.Skip;
                break;
            case AppCommands.MediaPreviousTrack:
                button = RemoteButton.Replay;
                break;
            case AppCommands.MediaStop:
                button = RemoteButton.Stop;
                break;
            case AppCommands.MediaPlayPause:
                button = RemoteButton.Pause;
                break;
            case AppCommands.Print:
                button = RemoteButton.Print;
                break;
            case AppCommands.MediaPlay:
                button = RemoteButton.Play;
                break;
            case AppCommands.MediaPause:
                button = RemoteButton.Pause;
                break;
            case AppCommands.MediaRecord:
                button = RemoteButton.Record;
                break;
            case AppCommands.MediaFastForward:
                button = RemoteButton.Forward;
                break;
            case AppCommands.MediaRewind:
                button = RemoteButton.Rewind;
                break;
            case AppCommands.MediaChannelUp:
                button = RemoteButton.ChannelUp;
                break;
            case AppCommands.MediaChannelDown:
                button = RemoteButton.ChannelDown;
                break;
        }

        if (button != RemoteButton.None)
        {
            if (_inputHandler == null) InitInputHandler();
            if (_inputHandler == null || !_inputHandler.IsLoaded) return null;

            result = _inputHandler.GetMapping(((int)button).ToString());

            // Get the mapping
            if (result != null)
            {
                if (logVerbose)
                {
                    Log.Info("MCE: Command \"{0}\" mapped from AppCommands {1}", button, command);
                }
            }
            else if (logVerbose)
            {
                Log.Info("MCE: Command \"{0}\" not mapped from AppCommands {1}", button, command);
            }
        }
        return result;
    }
  }
}