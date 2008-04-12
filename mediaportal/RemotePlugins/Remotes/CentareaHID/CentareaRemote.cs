#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Text;

using MediaPortal.InputDevices;
using MediaPortal.ServiceImplementations;
using MediaPortal.Configuration;
using MediaPortal.Hardware;
using System.Windows.Forms;

namespace MediaPortal.InputDevices
{
  public class CentareaRemote
  {
    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSKEYDOWN = 0x0104;
    const int WM_APPCOMMAND = 0x0319;
    const int WM_LBUTTONDOWN = 0x0201;
    const int WM_RBUTTONDOWN = 0x0204;
    const int WM_MOVE = 0x0003;

    bool _remoteActive = false;   // Centarea Remote enabled and mapped
    bool _verboseLogging = false; // Log key presses
    bool _mapMouseButton = true;  // Interpret the joystick push as "ok" button
    InputHandler _inputHandler;   // Input Mapper

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public CentareaRemote()
    {
    }

    #endregion

    #region Init && Deinit

    public void Init()
    {
      bool RemoteConfigured = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        RemoteConfigured = xmlreader.GetValueAsBool("remote", "Centarea", false);
        _verboseLogging = xmlreader.GetValueAsBool("remote", "CentareaVerbose", false);
      }
      if (!RemoteConfigured)
        return;

      Log.Debug("Centarea: Initializing Centarea HID remote");

      _inputHandler = new InputHandler("Centarea HID");
      if (!_inputHandler.IsLoaded)
      {
        Log.Error("Centarea: Error loading default mapping file - please reinstall MediaPortal");
        DeInit();
        return;
      }
      else
      {
        Log.Info("Centarea: Centarea HID mapping loaded successfully");
        _remoteActive = true;
      }
    }

    /// <summary>
    /// Remove all device handling
    /// </summary>
    public void DeInit()
    {
      if (_remoteActive)
      {
        Log.Info("Centarea: Stopping Centarea HID remote");
        _remoteActive = false;
        _inputHandler = null;
      }
    }

    #endregion

    #region Key handling

    /// <summary>
    /// Let everybody know that this HID message may not be handled by anyone else
    /// </summary>
    /// <param name="msg">System.Windows.Forms.Message</param>
    /// <returns>Command handled</returns>
    public bool WndProc(ref Message msg)
    {
      if (_remoteActive)
      {
        if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN || msg.Msg == WM_APPCOMMAND || msg.Msg == WM_LBUTTONDOWN)
        {
          switch ((Keys)msg.WParam)
          {
            case Keys.ControlKey:
              break;
            case Keys.ShiftKey:
              break;
            case Keys.Menu:
              break;
            default:
              int keycode = (int)msg.WParam;
              // Due to the non-perfect placement of the OK button we allow the user to remap the joystick to okay.
              // Unfortunately this will have SYSTEMWIDE effect.
              if (_mapMouseButton && msg.Msg == WM_LBUTTONDOWN)
              {
                if (_verboseLogging)
                  Log.Debug("Centarea: Command \"{0}\" mapped for joystick button", keycode);
                keycode = 13;
              }
              // The Centarea Remote sends key combos. Therefore we use this trick to get a 1:1 mapping
              if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) keycode += 1000;
              if ((Control.ModifierKeys & Keys.Control) == Keys.Control) keycode += 10000;
              if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt) keycode += 100000;

              try
              {
                // Get & execute Mapping
                if (_inputHandler.MapAction(keycode))
                {
                  if (_verboseLogging)
                    Log.Debug("Centarea: Command \"{0}\" mapped", keycode);
                }
                else
                {
                  Log.Debug("Centarea: Command \"{0}\" not mapped", keycode);
                  return false;
                }
              }
              catch (ApplicationException)
              {
                return false;
              }
              msg.Result = new IntPtr(1);
              break;
          }
          return true;
        }
      }
      return false;
    }

    #endregion
  }
}
