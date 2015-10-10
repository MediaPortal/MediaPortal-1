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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Realtek
{
  /// <summary>
  /// A class for handling PID filter and remote control support for various Realtek RTL283x based
  /// tuners.
  /// </summary>
  public class Realtek : BaseTunerExtension, IDisposable, IMpeg2PidFilter, IRemoteControlListener
  {
    #region enums

    private enum BdaExtensionProperty
    {
      IrCode = 0,
      UsbMode,
      EnablePid,
      DisablePid,
      DeviceSuspendStatus,
      PidFilterStatus
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0x1bfb70f7, 0xadfb, 0x4414, 0x9f, 0xd4, 0x60, 0xe9, 0xe5, 0x40, 0xa5, 0x59);

    private static readonly int KS_PROPERTY_SIZE = Marshal.SizeOf(typeof(KsProperty));    // 24

    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    private bool _isRealtek = false;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _generalBuffer = IntPtr.Zero;

    private bool _isRemoteControlInterfaceOpen = false;
    private IntPtr _remoteControlBuffer = IntPtr.Zero;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

    private HashSet<ushort> _pidFilterPids = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidFilterPidsToAdd = new HashSet<ushort>();
    private bool _isPidFilterDisabled = false;

    #endregion

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartRemoteControlListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isRemoteControlInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_remoteControlListenerThread != null && !_remoteControlListenerThread.IsAlive)
      {
        StopRemoteControlListenerThread();
      }
      if (_remoteControlListenerThread == null)
      {
        this.LogDebug("Realtek: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Realtek remote control listener";
        _remoteControlListenerThread.IsBackground = true;
        _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
        _remoteControlListenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopRemoteControlListenerThread()
    {
      if (_remoteControlListenerThread != null)
      {
        if (!_remoteControlListenerThread.IsAlive)
        {
          this.LogWarn("Realtek: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Realtek: failed to join remote control listener thread, aborting thread");
            _remoteControlListenerThread.Abort();
          }
        }
        _remoteControlListenerThread = null;
        if (_remoteControlListenerThreadStopEvent != null)
        {
          _remoteControlListenerThreadStopEvent.Close();
          _remoteControlListenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void RemoteControlListener()
    {
      this.LogDebug("Realtek: remote control listener thread start polling");
      int hr;
      int returnedByteCount;
      int previousCode = 0;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          Marshal.WriteInt32(_remoteControlBuffer, 0, 0);
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.IrCode,
            _instanceBuffer, KS_PROPERTY_SIZE,
            _remoteControlBuffer, 4,
            out returnedByteCount
          );
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != 4)
          {
            this.LogError("Realtek: failed to read remote code, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          }
          else
          {
            int code = Marshal.ReadInt32(_remoteControlBuffer, 0);
            if (code != previousCode)
            {
              this.LogDebug("Realtek: remote control key press, code = 0x{0:x8}", code);
              previousCode = code;
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Realtek: remote control listener thread exception");
        return;
      }
      this.LogDebug("Realtek: remote control listener thread stop polling");
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Realtek: initialising");

      if (_isRealtek)
      {
        this.LogWarn("Realtek: extension already initialised");
        return true;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Realtek: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.IrCode, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Realtek: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      this.LogInfo("Realtek: extension supported");
      _isRealtek = true;
      _instanceBuffer = Marshal.AllocCoTaskMem(KS_PROPERTY_SIZE);
      _generalBuffer = Marshal.AllocCoTaskMem(4);

      // We need to sync the state of the PID filter and our PID list. Because we can't query for
      // the current PID list, the only way to do this is to disable the filter (and assume that
      // clears the PID list).
      _isPidFilterDisabled = false;
      (this as IMpeg2PidFilter).Disable();
      return true;
    }

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for a given transmitter.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ShouldEnable(IChannel tuningDetail)
    {
      // Assume it is necessary/desirable to enable the filter. The hardware
      // does have limits... though the exact details are not known.
      return true;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      if (_isPidFilterDisabled)
      {
        _pidFilterPids.Clear();
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        return true;
      }

      this.LogDebug("Realtek: disable PID filter");
      Marshal.WriteInt32(_generalBuffer, 0, 0);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PidFilterStatus,
          _instanceBuffer, KS_PROPERTY_SIZE,
          _generalBuffer, 4
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Realtek: result = success");
        _isPidFilterDisabled = true;
        _pidFilterPids.Clear();
        _pidFilterPidsToRemove.Clear();
        _pidFilterPidsToAdd.Clear();
        return true;
      }

      this.LogError("Realtek: failed to disable PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return -1;  // maximum not known
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.UnionWith(pids);
      _pidFilterPidsToRemove.ExceptWith(pids);
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPidsToAdd.ExceptWith(pids);
      _pidFilterPidsToRemove.UnionWith(pids);
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ApplyConfiguration()
    {
      if (_pidFilterPidsToAdd.Count == 0 && _pidFilterPidsToRemove.Count == 0)
      {
        return true;
      }

      this.LogDebug("Realtek: apply PID filter configuration");
      int hr = (int)NativeMethods.HResult.S_OK;
      if (_isPidFilterDisabled)
      {
        this.LogDebug("  enable filter...");
        Marshal.WriteInt32(_generalBuffer, 0, 1);
        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PidFilterStatus,
            _instanceBuffer, KS_PROPERTY_SIZE,
            _generalBuffer, 4
        );
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Realtek: failed to enable PID filter, hr = 0x{0:x}", hr);
          return false;
        }

        this.LogDebug("Realtek: result = success");
        _isPidFilterDisabled = true;
      }

      if (_pidFilterPidsToRemove.Count > 0)
      {
        this.LogDebug("  disable {0} current PID(s)...", _pidFilterPidsToRemove.Count);
        foreach (ushort pid in _pidFilterPidsToRemove)
        {
          Marshal.WriteInt32(_generalBuffer, 0, pid);
          hr |= _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DisablePid,
              _instanceBuffer, KS_PROPERTY_SIZE,
              _generalBuffer, 4
          );
        }
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Realtek: failed to disable current PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        _pidFilterPids.ExceptWith(_pidFilterPidsToRemove);
        _pidFilterPidsToRemove.Clear();
      }

      if (_pidFilterPidsToAdd.Count > 0)
      {
        this.LogDebug("  enable {0} new PID(s)...", _pidFilterPidsToAdd.Count);
        foreach (ushort pid in _pidFilterPidsToAdd)
        {
          Marshal.WriteInt32(_generalBuffer, 0, pid);
          hr |= _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.EnablePid,
              _instanceBuffer, KS_PROPERTY_SIZE,
              _generalBuffer, 4
          );
        }
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Realtek: failed to enable new PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        _pidFilterPids.UnionWith(_pidFilterPidsToAdd);
        _pidFilterPidsToAdd.Clear();
      }

      this.LogDebug("Realtek: result = success");
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("Realtek: open remote control interface");

      if (!_isRealtek)
      {
        this.LogWarn("Realtek: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Realtek: remote control interface is already open");
        return true;
      }

      // Initialise() already checked that the property is supported.
      _remoteControlBuffer = Marshal.AllocCoTaskMem(4);
      _isRemoteControlInterfaceOpen = true;
      StartRemoteControlListenerThread();

      this.LogDebug("Realtek: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Close()
    {
      return CloseRemoteControlListenerInterface(true);
    }

    private bool CloseRemoteControlListenerInterface(bool isDisposing)
    {
      this.LogDebug("Realtek: close remote control interface");

      if (isDisposing)
      {
        StopRemoteControlListenerThread();
      }
      if (_remoteControlBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_remoteControlBuffer);
        _remoteControlBuffer = IntPtr.Zero;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Realtek: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Realtek()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isRealtek)
      {
        CloseRemoteControlListenerInterface(isDisposing);
      }
      if (isDisposing)
      {
        _propertySet = null;
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isRealtek = false;
    }

    #endregion
  }
}