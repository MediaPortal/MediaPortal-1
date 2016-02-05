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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl
{
  /// <summary>
  /// A class for handling legacy remote control support for Twinhan tuners.
  /// </summary>
  internal class RemoteControlLegacy : ITwinhanRemoteControl
  {
    #region constants

    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100; // unit = ms

    #endregion

    #region variables

    private static HashSet<string> _openProducts = new HashSet<string>();

    private bool _isInterfaceOpen = false;
    private string _productInstanceId = null;

    private IKsPropertySet _propertySet = null;
    private IoControl _ioControl = null;

    private Thread _listenerThread = null;
    private ManualResetEvent _listenerThreadStopEvent = null;

    #endregion

    public RemoteControlLegacy(string productInstanceId, IKsPropertySet propertySet)
    {
      _productInstanceId = productInstanceId;
      _propertySet = propertySet;
    }

    /// <summary>
    /// Open the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool Open()
    {
      this.LogDebug("Twinhan legacy RC: open interface, product instance ID = {0}", _productInstanceId ?? "[null]");

      if (_isInterfaceOpen)
      {
        this.LogWarn("Twinhan legacy RC: interface is already open");
        return true;
      }
      if (_productInstanceId == null)
      {
        this.LogDebug("Twinhan legacy RC: product instance identifier is null");
        return false;
      }
      if (_openProducts.Contains(_productInstanceId))
      {
        this.LogDebug("Twinhan legacy RC: multi-tuner product remote control opened for other tuner");
        return true;
      }

      _ioControl = new IoControl(_propertySet);
      int hr = _ioControl.Set(IoControlCode.StartRemoteControl, IntPtr.Zero, 0);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Twinhan legacy RC: failed to start remote control, hr = 0x{0:x}", hr);
        return false;
      }

      _isInterfaceOpen = true;
      _openProducts.Add(_productInstanceId);
      StartListenerThread();
      return true;
    }

    /// <summary>
    /// Close the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool Close()
    {
      this.LogDebug("Twinhan legacy RC: close interface");

      StopListenerThread();

      if (_isInterfaceOpen)
      {
        int hr = _ioControl.Set(IoControlCode.StopRemoteControl, IntPtr.Zero, 0);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("Twinhan legacy RC: failed to stop remote control, hr = 0x{0:x}", hr);
        }

        if (_productInstanceId != null)
        {
          _openProducts.Remove(_productInstanceId);
        }
        _isInterfaceOpen = false;
      }
      _ioControl = null;
      return true;
    }

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_listenerThread != null && !_listenerThread.IsAlive)
      {
        StopListenerThread();
      }
      if (_listenerThread == null)
      {
        this.LogDebug("Twinhan legacy RC: starting new listener thread");
        _listenerThreadStopEvent = new ManualResetEvent(false);
        _listenerThread = new Thread(new ThreadStart(Listener));
        _listenerThread.Name = "Twinhan legacy remote control listener";
        _listenerThread.IsBackground = true;
        _listenerThread.Priority = ThreadPriority.Lowest;
        _listenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopListenerThread()
    {
      if (_listenerThread != null)
      {
        if (!_listenerThread.IsAlive)
        {
          this.LogWarn("Twinhan legacy RC: aborting old listener thread");
          _listenerThread.Abort();
        }
        else
        {
          _listenerThreadStopEvent.Set();
          if (!_listenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Twinhan legacy RC: failed to join listener thread, aborting thread");
            _listenerThread.Abort();
          }
        }
        _listenerThread = null;
        if (_listenerThreadStopEvent != null)
        {
          _listenerThreadStopEvent.Close();
          _listenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void Listener()
    {
      this.LogDebug("Twinhan legacy RC: listener thread start polling");

      IntPtr codeBuffer = Marshal.AllocCoTaskMem(1);
      int hr;
      int returnedByteCount;
      byte previousCode = 0;
      try
      {
        while (!_listenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          hr = _ioControl.Get(IoControlCode.GetRemoteControlValue, codeBuffer, 1, out returnedByteCount);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("Twinhan legacy RC: failed to read remote code, hr = 0x{0:x}", hr);
          }
          else
          {
            byte remoteCode = Marshal.ReadByte(codeBuffer, 0);
            if (remoteCode != previousCode)
            {
              this.LogDebug("Twinhan legacy RC: key press, code = {0}", remoteCode);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Twinhan legacy RC: listener thread exception");
        return;
      }
      finally
      {
        Marshal.FreeCoTaskMem(codeBuffer);
      }
      this.LogDebug("Twinhan legacy RC: listener thread stop polling");
    }

    #endregion
  }
}