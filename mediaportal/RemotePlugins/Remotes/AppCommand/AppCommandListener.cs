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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Hooks;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.InputDevices
{
    public class AppCommandListener
    {
        private bool controlEnabled = false;
        private bool controlEnabledGlobally = false;
        private bool logVerbose = false; // Verbose logging
        private InputHandler _inputHandler;
        private KeyboardHook _keyboardHook;

        public AppCommandListener() { }

        public void Init(IntPtr hwnd)
        {
            Init();

        }

        private void Init()
        {
            using (Settings xmlreader = new MPSettings())
            {
                controlEnabled = xmlreader.GetValueAsBool("remote", "AppCommand", false);
                controlEnabledGlobally = xmlreader.GetValueAsBool("remote", "AppCommandBackground", false);
                logVerbose = xmlreader.GetValueAsBool("remote", "AppCommandVerbose", false);
            }

            if (controlEnabled)
            {
                _inputHandler = new InputHandler("AppCommand");
                if (!_inputHandler.IsLoaded)
                {
                    controlEnabled = false;
                    Log.Info("AppCommand: Error loading default mapping file - please reinstall MediaPortal");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    // since the normal process involves getting polled via WndProc we have to get a tiny bit dirty 
                    // and send a message back to the main form in order to get the key press handled without 
                    // duplicating action mapping code from the main app
                    Win32API.SendMessage(GUIGraphicsContext.form.Handle, Win32.Const.WM_APPCOMMAND, (uint)GUIGraphicsContext.form.Handle, (uint)lParam);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="action"></param>
        /// <param name="key"></param>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool WndProcAppCommand(ref Message msg, out Action action, out char key, out Keys keyCode)
        {
            action = null;
            key = (char)0;
            keyCode = Keys.A;

            AppCommands appCommand = (AppCommands)Win32.Macro.GET_APPCOMMAND_LPARAM(msg.LParam);

            // find out which request the MCE remote handled last
            if ((appCommand == InputDevices.LastHidRequest) && (appCommand != AppCommands.VolumeDown) &&
                (appCommand != AppCommands.VolumeUp))
            {
                if (Enum.IsDefined(typeof(AppCommands), InputDevices.LastHidRequest))
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
                Log.Info("AppCommand: {0} - {1}", appCommand, InputDevices.LastHidRequest.ToString());
            }

            if (!_inputHandler.MapAction((int)appCommand))
            {
                return false;
            }

            msg.Result = new IntPtr(1);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="action"></param>
        /// <param name="key"></param>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
        {
            action = null;
            key = (char)0;
            keyCode = Keys.A;

            if (!controlEnabled)
            {
                return false;
            }
            // we are only interested in WM_APPCOMMAND
            switch (msg.Msg)
            {
                case Win32.Const.WM_APPCOMMAND:
                    return WndProcAppCommand(ref msg, out action, out key, out keyCode);

                default:
                    return false;
            }
        }


    }
}