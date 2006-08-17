#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Windows.Forms;
using System.Data;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.Reflection;
using MediaPortal.RemoteControls;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using MediaPortal.RemoteControls.FireDTV;
using MediaPortal.InputDevices;
using MediaPortal.Utils.Services;

namespace MediaPortal.RemoteControls
{
  /// <summary>
  /// Summary description for FireDTVRemote.
  /// </summary>
  public class FireDTVRemote
  {
    #region Private Variables
    private static bool _enabled = false;
    private bool _logVerbose = false;
    private static string _name;

    private FireDTVControl _fireDTV = null;
    private InputHandler _inputHandler;
    private IConfig _config;
    private ILog _log;
    #endregion

    #region Private Methods
    /// <summary>
    ///  
    /// </summary>
    private void StartFireDTVComms()
    {
      if (_enabled)
      {
        // Open driver
        _enabled = _fireDTV.OpenDrivers();

        if (!_enabled)
        {
          _log.Error("FireDTVRemote: Failed to open driver");
          return;
        }

        // Search for the named sourcefilter
        FireDTVSourceFilterInfo sourceFilter = _fireDTV.SourceFilters.ItemByName(_name);
        if (sourceFilter != null)
        {
          sourceFilter.StartFireDTVRemoteControlSupport();
        }
        else
          _log.Error("FireDTVRemote: SourceFilter {0} Not Found", _name);
      }
    }

    #endregion

    public FireDTVRemote()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    /// <summary>
    /// Initialise the FireDTV remote
    /// </summary>
    /// <param name="hwnd">The window handler where the remote messages are send</param>
    public void Init(IntPtr hwnd)
    {
      try
      {
        ServiceProvider services = GlobalServiceProvider.Instance;
        _log = services.Get<ILog>();
        _config = services.Get<IConfig>();
        // first read the configuration, to determine the initialisation is needed
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(_config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
        {
          _enabled = ((xmlreader.GetValueAsBool("remote", "FireDTV", false)));
          _name = xmlreader.GetValueAsString("remote", "FireDTVDeviceName", string.Empty);
          if (!_enabled) return;
        }

        // load the default input mapping
        _inputHandler = new InputHandler("FireDTV");
        if (!_inputHandler.IsLoaded)
        {
          _enabled = false;
          _log.Error("FireDTVRemote: Error loading default mapping file - please reinstall MediaPortal");
          return;
        }

        _fireDTV = new FireDTVControl(hwnd);
        _log.Info("FireDTVRemote: Starting on handler {0}", hwnd);

        // start communication with the FireDTV library
        StartFireDTVComms();
      }
      catch (FileNotFoundException eFileNotFound)
      {
        _log.Error(eFileNotFound);
      }
      catch (FireDTVException eFireDTV)
      {
        _log.Error(eFireDTV);
      }
    }

    public void DeInit()
    {
      if (_fireDTV != null) _fireDTV.CloseDrivers();
    }

    public bool WndProc(ref System.Windows.Forms.Message msg, out MediaPortal.GUI.Library.Action action, out char key, out System.Windows.Forms.Keys keyCode)
    {
      keyCode = System.Windows.Forms.Keys.A;
      key = (char)0;
      action = null;
      switch ((FireDTVConstants.FireDTVWindowMessages)msg.Msg)
      {
        case FireDTVConstants.FireDTVWindowMessages.DeviceAttached:
          _log.Info("FireDTVRemote: DeviceAttached");
          StartFireDTVComms();
          break;

        case FireDTVConstants.FireDTVWindowMessages.DeviceDetached:
          _log.Info("FireDTVRemote: DeviceDetached");
          _fireDTV.SourceFilters.RemoveByHandle((uint)msg.WParam);
          break;

        case FireDTVConstants.FireDTVWindowMessages.DeviceChanged:
          _log.Info("FireDTVRemote: DeviceChanged");
          StartFireDTVComms();
          break;

        case FireDTVConstants.FireDTVWindowMessages.RemoteControlEvent:
          if (_enabled)
          {
            int remoteKeyCode = msg.LParam.ToInt32();
            if (_logVerbose) _log.Info("FireDTVRemote: RemoteControlEvent {0}", remoteKeyCode);

            if (!_inputHandler.MapAction(remoteKeyCode))
              return false;

            msg.Result = new IntPtr(1);
            return true;
          }
          break;
      }

      return false;
    }

    public Guid RCGuid
    {
      get
      {
        return new Guid("{73DF3DFD-855A-418c-B98B-121C513BD2E4}");
      }
    }

  }
}
