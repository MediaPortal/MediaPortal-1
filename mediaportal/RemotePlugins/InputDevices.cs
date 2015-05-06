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
using Action = MediaPortal.GUI.Library.Action;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MediaPortal.InputDevices
{
  public static class InputDevices
  {
    #region Methods

    static InputDevices() {}

    public static void Init()
    {
      if (_initialized)
      {
        Log.Info("Remotes: Init was called before Stop - stopping devices now");
        Stop();
      }
      _initialized = true;

      foreach (var device in Devices)
      {
        device.Init(GUIGraphicsContext.ActiveForm);
      }
    }

    public static void Stop()
    {
      if (!_initialized)
      {
        Log.Info("Remotes: Stop was called without Init - exiting");
        return;
      }
      foreach (var device in Devices)
      {
        device.DeInit();
      }

      _initialized = false;
    }

    public static bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
    {
      action = null;
      key = (char)0;
      keyCode = Keys.A;

      foreach (var device in Devices)
      {
        if (device.WndProc(ref msg, out action, out key, out keyCode))
        {
            return true;
        }
      }

      return false;
    }


    /// <summary>
    /// Try and map a WndProc message to an action regardless of whether the remote is stopped or not (will still ignore if it's disabled)
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static Action MapToAction(Message msg)
    {
      Action result = null;
      Log.Info(string.Format("WndProc message to be processed {0}, appCommand {1}, LParam {2}, WParam {3}", msg.Msg, Win32.Macro.GET_APPCOMMAND_LPARAM(msg.LParam), msg.LParam, msg.WParam));
      foreach (var device in Devices)
      {
        var mapping = device.GetMapping(msg);
        if (mapping != null)
        {
          switch (mapping.Command)
          {
            case "ACTION": // execute Action x
              Key key = new Key(mapping.CmdKeyChar, mapping.CmdKeyCode);
              Log.Info("MappingToAction: key {0} / {1} / Action: {2} / {3}", mapping.CmdKeyChar, mapping.CmdKeyCode,
                mapping.CmdProperty,
                ((Action.ActionType) Convert.ToInt32(mapping.CmdProperty)).ToString());
              result = new Action(key, (Action.ActionType) Convert.ToInt32(mapping.CmdProperty), 0, 0);
              break;
            case "KEY": // Try and map the key to the Keys enum and process that way
              var tmpKey = Keys.A;
              if (Enum.TryParse<Keys>(mapping.CmdProperty, out tmpKey))
                result = MapToAction((int) tmpKey);
              break;
          }
          if (result != null) break;
        }
      }

      if (result == null) Log.Info("No mapping found");

      return result;
    }

    /// <summary>
    /// Map a key press to an action
    /// </summary>
    /// <param name="keyPressed"></param>
    /// <returns></returns>
    public static Action MapToAction(int keyPressed)
    {
      var action = new Action();

      if (ActionTranslator.GetAction(-1, new Key(0, keyPressed), ref action))
      {
        return action;
      }
      //See if it's mapped to KeyPressed instead
      if (keyPressed >= (int) Keys.A && keyPressed <= (int) Keys.Z)
      {
        keyPressed += 32; //convert to char code
      }
      if (ActionTranslator.GetAction(-1, new Key(keyPressed, 0), ref action))
      {
        return action;
      }
      return null;
    }

    #endregion Methods

    #region Properties

    internal static AppCommands LastHidRequest
    {
      get { return _lastHidRequest; }
      set
      {
        _lastHidRequest = value;
        _lastHidRequestTick = Environment.TickCount;
      }
    }

    internal static int LastHidRequestTick
    {
      get { return _lastHidRequestTick; }
    }

    /// <summary>
    /// All remote devices currently supported
    /// </summary>
    public static ReadOnlyCollection<IInputDevice> Devices 
    {
        get { return _devices; }
    }

    #endregion Properties

    #region Fields

    private static ReadOnlyCollection<IInputDevice> _devices = new List<IInputDevice> { 
                                                                    new HidListener(),
                                                                    new AppCommandListener(),
                                                                    new MCE2005Remote(),
                                                                    new HcwRemote(),
                                                                    new X10Remote(),
                                                                    new IrTrans(),
                                                                    new FireDTVRemote(),
                                                                    new CentareaRemote()}.AsReadOnly();

    private static AppCommands _lastHidRequest;

    private static int _lastHidRequestTick;
    private static bool _initialized = false;

    #endregion Fields
  }
}