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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Hooks;
using MediaPortal.Profile;

namespace MediaPortal.InputDevices
{
  public class HidListener
  {
    private bool controlEnabled = false;
    private bool controlEnabledGlobally = false;
    private bool logVerbose = false; // Verbose logging
    private InputHandler _inputHandler;
    private KeyboardHook _keyboardHook;

    public HidListener() {}

    public void Init(IntPtr hwnd)
    {
      Init();
    }

    private void Init()
    {
      using (Settings xmlreader = new MPSettings())
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HID", false);
        controlEnabledGlobally = xmlreader.GetValueAsBool("remote", "HIDGlobal", false);
        logVerbose = xmlreader.GetValueAsBool("remote", "HIDVerboseLog", false);
      }

      if (controlEnabled)
      {
        _inputHandler = new InputHandler("General HID");
        if (!_inputHandler.IsLoaded)
        {
          controlEnabled = false;
          Log.Info("HID: Error loading default mapping file - please reinstall MediaPortal");
        }
      }

      if (controlEnabledGlobally)
      {
        _keyboardHook = new KeyboardHook();
        _keyboardHook.KeyDown += new KeyEventHandler(OnKeyDown);
        _keyboardHook.IsEnabled = true;
      }
    }

    public void DeInit()
    {
      if (_keyboardHook != null && _keyboardHook.IsEnabled)
      {
        _keyboardHook.IsEnabled = false;
      }
    }

    [DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (GUIGraphicsContext.form != null && GUIGraphicsContext.form.Focused == false)
      {
        AppCommands appCommand = KeyCodeToAppCommand(e.KeyCode);

        if (appCommand != AppCommands.None)
        {
          int device = 0;
          int keys = (((int)appCommand & ~0xF000) | (device & 0xF000));
          int lParam = (((keys) << 16) | (((int)e.KeyCode)));

          // since the normal process involves geting polled via WndProc we have to get a tiny bit dirty 
          // and send a message back to the main form in order to get the key press handled without 
          // duplicating action mapping code from the main app
          SendMessage(GUIGraphicsContext.form.Handle, 0x0319, (uint)GUIGraphicsContext.form.Handle, (uint)lParam);
        }
      }
    }

    private AppCommands KeyCodeToAppCommand(Keys keyCode)
    {
      switch (keyCode)
      {
        case Keys.MediaNextTrack:
          return AppCommands.MediaNextTrack;
        case Keys.MediaPlayPause:
          return AppCommands.MediaPlayPause;
        case Keys.MediaPreviousTrack:
          return AppCommands.MediaPreviousTrack;
        case Keys.MediaStop:
          return AppCommands.MediaStop;
        case Keys.VolumeDown:
          return AppCommands.VolumeDown;
        case Keys.VolumeMute:
          return AppCommands.VolumeMute;
        case Keys.VolumeUp:
          return AppCommands.VolumeUp;
      }

      return AppCommands.None;
    }

    public bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
    {
      action = null;
      key = (char)0;
      keyCode = Keys.A;

      if (controlEnabled)
      {
        // we are only interested in WM_APPCOMMAND
        if (msg.Msg != 0x0319)
        {
          return false;
        }

        AppCommands appCommand = (AppCommands)((msg.LParam.ToInt32() >> 16) & ~0xF000);

        // find out which request the MCE remote handled last
        if ((appCommand == InputDevices.LastHidRequest) && (appCommand != AppCommands.VolumeDown) &&
            (appCommand != AppCommands.VolumeUp))
        {
          if (Enum.IsDefined(typeof (AppCommands), InputDevices.LastHidRequest))
          {
            // possible that it is the same request mapped to an app command?
            if (Environment.TickCount - InputDevices.LastHidRequestTick < 500)
            {
              return true;
            }
          }
        }

        InputDevices.LastHidRequest = appCommand;

        if (logVerbose)
        {
          Log.Info("HID: Command: {0} - {1}", ((msg.LParam.ToInt32() >> 16) & ~0xF000),
                   InputDevices.LastHidRequest.ToString());
        }

        if (!_inputHandler.MapAction((msg.LParam.ToInt32() >> 16) & ~0xF000))
        {
          return false;
        }

        msg.Result = new IntPtr(1);

        return true;
      }
      return false;
    }
  }
}