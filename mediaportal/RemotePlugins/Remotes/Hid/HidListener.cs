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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Win32;
using Action = MediaPortal.GUI.Library.Action;
using Mapping = MediaPortal.InputDevices.InputHandler.Mapping;

namespace MediaPortal.InputDevices
{
  public class HidListener : IInputDevice
  {
    private bool _controlEnabled;

    /// <summary>
    ///   HID Handler is responsible for:
    ///   * Loading and parsing Generic HID XML configuration.
    ///   * Registering for raw input as per configuration.
    ///   * Handling HID raw input as per configuration.
    /// </summary>
    private HidHandler _hidHandler;

    /// <summary>
    /// Tells whether verbose logs are enabled.
    /// </summary>
    static public bool Verbose{get; private set;}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hwnd"></param>
    public void Init(IntPtr hwnd)
    {
      //Load from XML configuration
      Init();

      //Once our configuration is loaded register raw input devices as needed
      if (_hidHandler != null && _hidHandler.IsLoaded)
      {
        _hidHandler.Register(hwnd);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Init()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _controlEnabled = xmlreader.GetValueAsBool("remote", "HidEnabled", false);
        Verbose = xmlreader.GetValueAsBool("remote", "HidVerbose", false);
      }

      if (_controlEnabled)
      {
        _hidHandler = new HidHandler("Generic-HID");
      }
    }

    /// <summary>
    /// Required for IInputDevice Interface
    /// </summary>
    public void DeInit()
    {
    }

    /// <summary>
    /// Get the first mapping for this message
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public Mapping GetMapping(Message msg)
    {
      if (msg.Msg == Win32.Const.WM_INPUT)
      {
        //Just ask our handler to process
        var actions = _hidHandler.ProcessInput(msg, false);

        if (actions != null && actions.Count > 0)
        {
          var action = actions[0];
          return new Mapping(action.Layer, action.Condition, action.ConProperty, action.Command, action.CmdProperty, action.CmdKeyChar, action.CmdKeyCode, action.Sound, action.Focus);
        }
      }
      return null;
    }

    /// <summary>
    ///   Handle WM_INPUT messages.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="action"></param>
    /// <param name="key"></param>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool WndProcInput(ref Message msg, out Action action, out char key, out Keys keyCode)
    {
      action = null;
      key = (char) 0;
      keyCode = Keys.A;

      //Just ask our handler to process
      _hidHandler.ProcessInput(msg);

      return false;
    }

    /// <summary>
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="action"></param>
    /// <param name="key"></param>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
    {
      action = null;
      key = (char) 0;
      keyCode = Keys.A;

      if (!_controlEnabled)
      {
        return false;
      }
      //We are only interested in WM_INPUT
      switch (msg.Msg)
      {
        case Const.WM_INPUT:
          return WndProcInput(ref msg, out action, out key, out keyCode);

        default:
          return false;
      }
    }

    /// <summary>
    /// Utility function for logging.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="arg"></param>
    static public void LogInfo(string format, params object[] arg)
    {
      if (Verbose)
      {
        Log.Info(format, arg);
      }
    }
  }
}