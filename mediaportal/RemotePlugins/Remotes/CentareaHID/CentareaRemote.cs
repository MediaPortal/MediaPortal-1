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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MediaPortal.InputDevices;
using MediaPortal.ServiceImplementations;
using MediaPortal.Configuration;
using MediaPortal.Hardware;


namespace MediaPortal.InputDevices
{
  public class CentareaRemote
  {
    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSKEYDOWN = 0x0104;
    const int WM_APPCOMMAND = 0x0319;
    const int WM_LBUTTONDOWN = 0x0201;
    const int WM_RBUTTONDOWN = 0x0204;
    const int WM_MOUSEMOVE = 0x0200;
    const int WM_SETCURSOR = 0x0020;

    enum MouseDirection { Up, Right, Down, Left, None };

    bool _remoteActive = false;   // Centarea Remote enabled and mapped
    bool _verboseLogging = false; // Log key presses
    bool _mapMouseButton = true;  // Interpret the joystick push as "ok" button
    bool _mapJoystick = true;    // Map any mouse movement to cursor keys
    int _lastMouseTick = 0;       // When did the last mouse action occur
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
        _mapMouseButton = xmlreader.GetValueAsBool("remote", "CentareaMouseOkMap", true);
        _mapJoystick = xmlreader.GetValueAsBool("remote", "CentareaJoystickMap", false);
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
        if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN || msg.Msg == WM_APPCOMMAND || msg.Msg == WM_LBUTTONDOWN || msg.Msg == WM_RBUTTONDOWN || msg.Msg == WM_MOUSEMOVE)
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

              AppCommands appCommand = (AppCommands)((msg.LParam.ToInt32() >> 16) & ~0xF000);
              // find out which request the MCE remote handled last
              if ((appCommand == InputDevices.LastHidRequest) && (appCommand != AppCommands.VolumeDown) && (appCommand != AppCommands.VolumeUp))
              {
                if (Enum.IsDefined(typeof(AppCommands), InputDevices.LastHidRequest))
                {
                  // possible that it is the same request mapped to an app command?
                  if (Environment.TickCount - InputDevices.LastHidRequestTick < 500)
                    return true;
                }
              }
              InputDevices.LastHidRequest = appCommand;

              // Due to the non-perfect placement of the OK button we allow the user to remap the joystick to okay.
              if (_mapMouseButton)
              {
                if (msg.Msg == WM_LBUTTONDOWN)
                {
                  if (_verboseLogging)
                    Log.Debug("Centarea: Command \"{0}\" mapped for left mouse button", keycode);
                  keycode = 13;
                }
                if (msg.Msg == WM_RBUTTONDOWN)
                {
                  if (_verboseLogging)
                    Log.Debug("Centarea: Command \"{0}\" mapped for right mouse button", keycode);
                  keycode = 10069;
                }
              }
              if (_mapJoystick)
              {
                if (msg.Msg == WM_MOUSEMOVE)
                {
                  if (Environment.TickCount - _lastMouseTick < 100)
                    return false;

                  Point p = new Point(msg.LParam.ToInt32());
                  MouseDirection mmove = OnMouseMoved(p);
                  MediaPortal.GUI.Library.GUIGraphicsContext.ResetCursor();
                  _lastMouseTick = Environment.TickCount;

                  switch (mmove)
                  {
                    case MouseDirection.Up:
                      keycode = 38;
                      break;
                    case MouseDirection.Right:
                      keycode = 39;
                      break;
                    case MouseDirection.Down:
                      keycode = 40;
                      break;
                    case MouseDirection.Left:
                      keycode = 37;
                      break;
                  }
                  if (_verboseLogging && mmove != MouseDirection.None)
                    Log.Debug("Centarea: Command \"{0}\" mapped for mouse movement", mmove.ToString());
                }
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
                  if (keycode > 0)
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

    #region Joystick handling

    private MouseDirection OnMouseMoved(Point p)
    {
      int x_Val, y_Val;
      MouseDirection direction = MouseDirection.None;
      //Translating mouse motion on the control to a 30 x 30 cartesian grid.
      y_Val = (p.Y * 31 / MediaPortal.GUI.Library.GUIGraphicsContext.Height) - 15;
      x_Val = (p.X * 31 / MediaPortal.GUI.Library.GUIGraphicsContext.Width) - 15;
      y_Val = -y_Val;

      int radius = (int)Math.Sqrt(y_Val * y_Val + x_Val * x_Val);
      if (radius > 2 && radius < 15)
        direction = GetDirection(x_Val, y_Val);
      return direction;
    }

    private MouseDirection GetDirection(float x, float y)
    {
      //Changing cartesian coordinates from control surface to  
      //more usable polar coordinates
      double theta;
      if (x >= 0 && y > 0)
        theta = (Math.Atan(y / x) * (180 / Math.PI));
      else if (x < 0)
        theta = ((Math.PI + Math.Atan(y / x)) * (180 / Math.PI));
      else theta = (((2 * Math.PI) + Math.Atan(y / x)) * (180 / Math.PI));

      if (theta <= 48.5 || theta > 318.5)
        return MouseDirection.Right;
      else if (theta > 48.5 && theta <= 138.5)
        return MouseDirection.Up;
      else if (theta > 138.5 && theta <= 228.5)
        return MouseDirection.Left;
      else if (theta > 228.5 && theta <= 318.5)
        return MouseDirection.Down;
      else
        return MouseDirection.None;
    }

    #endregion
  }
}
