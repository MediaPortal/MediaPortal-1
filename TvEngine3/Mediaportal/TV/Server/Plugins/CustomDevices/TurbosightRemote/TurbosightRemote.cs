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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight
{
  /// <summary>
  /// A class for handling remote controls for Turbosight PCI and PCIe tuners.
  /// </summary>
  public class TurbosightRemote : BaseCustomDevice, IRemoteControlListener
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Code = 0,
      Command
    }

    private enum TbsIrCommand : byte
    {
      Start = 1,
      Stop,
      Flush
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct IrData
    {
      public uint Address;
      public uint Command;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xb51c4994, 0x0054, 0x4749, 0x82, 0x43, 0x02, 0x9a, 0x66, 0x86, 0x36, 0x36);

    private static readonly int KS_PROPERTY_SIZE = Marshal.SizeOf(typeof(KsProperty));    // 24
    private static readonly int IR_DATA_SIZE = Marshal.SizeOf(typeof(IrData));            // 8

    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    // This plugin is not tuner-specific. We use this variable to restrict to
    // one instance.
    private static bool _isLoaded = false;
    private static object _instanceLock = new object();

    private bool _isTurbosightRemote = false;
    private IFilterGraph2 _graph = null;
    private List<DsDevice> _devices = new List<DsDevice>();
    private List<IBaseFilter> _filters = new List<IBaseFilter>();
    private List<IKsPropertySet> _propertySets = new List<IKsPropertySet>();
    private IntPtr _commandBuffer = IntPtr.Zero;
    private IntPtr _codeBuffer = IntPtr.Zero;

    private bool _isRemoteControlInterfaceOpen = false;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

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
        this.LogDebug("Turbosight remote: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Turbosight remote remote control listener";
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
          this.LogWarn("Turbosight remote: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Turbosight remote: failed to join remote control listener thread, aborting thread");
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
      this.LogDebug("Turbosight remote: remote control listener thread start polling");

      for (int i = 0; i < KS_PROPERTY_SIZE; i++)
      {
        Marshal.WriteByte(_commandBuffer, i, 0);
      }
      int hr;
      int returnedByteCount;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          foreach (IKsPropertySet ps in _propertySets)
          {
            hr = ps.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Code,
              _commandBuffer, KS_PROPERTY_SIZE,
              _codeBuffer, IR_DATA_SIZE,
              out returnedByteCount
            );
            if (hr != (int)HResult.Severity.Success || returnedByteCount != IR_DATA_SIZE)
            {
              this.LogError("Turbosight remote: failed to read remote code, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
            }
            else
            {
              IrData data = (IrData)Marshal.PtrToStructure(_codeBuffer, typeof(IrData));
              if (data.Command != 0)
              {
                this.LogDebug("Turbosight remote: remote control key press, address = 0x{0:x4}, command = 0x{1:x4}", data.Address, data.Command);
              }
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Turbosight remote: remote control listener thread exception");
        return;
      }
      this.LogDebug("Turbosight remote: remote control listener thread stop polling");
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Turbosight remote";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("Turbosight remote: initialising");

      if (_isTurbosightRemote)
      {
        this.LogWarn("Turbosight remote: extension already initialised");
        return true;
      }

      lock (_instanceLock)
      {
        if (_isLoaded)
        {
          this.LogDebug("Turbosight remote: already loaded");
          return false;
        }

        _graph = (IFilterGraph2)new FilterGraph();
        try
        {
          DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
          foreach (DsDevice device in devices)
          {
            bool isTbsIrFilter = false;
            IBaseFilter filter = null;
            try
            {
              if (string.IsNullOrEmpty(device.Name) || string.IsNullOrEmpty(device.DevicePath))
              {
                continue;
              }

              this.LogDebug("Turbosight remote: check {0} {1}", device.Name, device.DevicePath);
              int hr = _graph.AddSourceFilterForMoniker(device.Mon, null, device.Name, out filter);
              if (hr != (int)HResult.Severity.Success)
              {
                this.LogError("Turbosight remote: failed to add filter to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                continue;
              }

              IKsPropertySet propertySet = filter as IKsPropertySet;
              if (propertySet == null)
              {
                this.LogDebug("Turbosight remote: filter is not a property set");
                continue;
              }

              KSPropertySupport support;
              hr = propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Code, out support);
              if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Get))
              {
                this.LogDebug("Turbosight remote: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                continue;
              }

              this.LogDebug("Turbosight remote: property set supported");
              isTbsIrFilter = true;
              _isLoaded = true;
              _devices.Add(device);
              _filters.Add(filter);
              _propertySets.Add(propertySet);
            }
            finally
            {
              if (!isTbsIrFilter)
              {
                if (filter != null)
                {
                  _graph.RemoveFilter(filter);
                }
                Release.ComObject("Turbosight remote IR filter candidate", ref filter);
                device.Dispose();
              }
            }
          }
        }
        finally
        {
          Release.ComObject("Turbosight remote graph", ref _graph);
        }
      }

      if (_filters.Count == 0)
      {
        this.LogDebug("Turbosight remote: no supported filters detected");
        return false;
      }

      this.LogInfo("Turbosight remote: extension supported, {0} receiver(s)", _filters.Count);
      _isTurbosightRemote = true;
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenRemoteControlInterface()
    {
      this.LogDebug("Turbosight remote: open remote control interface");

      if (!_isTurbosightRemote)
      {
        this.LogWarn("Turbosight remote: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Turbosight remote: interface is already open");
        return true;
      }

      this.LogDebug("Turbosight remote: starting graph");
      int hr = ((IMediaControl)_graph).Run();
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("Turbosight remote: failed to start graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("Turbosight remote: starting receivers");
      _commandBuffer = Marshal.AllocCoTaskMem(KS_PROPERTY_SIZE);
      _codeBuffer = Marshal.AllocCoTaskMem(IR_DATA_SIZE);
      Marshal.WriteByte(_commandBuffer, 0, (byte)TbsIrCommand.Start);
      int i = 0;
      foreach (DsDevice device in _devices)
      {
        hr = _propertySets[i].Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, _commandBuffer, 1, _commandBuffer, 1);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("Turbosight remote: failed to start receiver {0} {1} {2}, hr = 0x{3:x} ({4})", i + 1, device.Name, device.DevicePath, hr, HResult.GetDXErrorString(hr));
        }
        i++;
      }

      StartRemoteControlListenerThread();

      _isRemoteControlInterfaceOpen = true;
      this.LogDebug("Turbosight remote: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseRemoteControlInterface()
    {
      this.LogDebug("Turbosight remote: close remote control interface");

      if (_isRemoteControlInterfaceOpen)
      {
        StopRemoteControlListenerThread();

        this.LogDebug("Turbosight remote: stopping graph");
        int hr = ((IMediaControl)_graph).Stop();
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Turbosight remote: failed to stop graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        Marshal.WriteByte(_commandBuffer, 0, (byte)TbsIrCommand.Stop);
        int i = 0;
        foreach (DsDevice device in _devices)
        {
          hr = _propertySets[i].Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, _commandBuffer, 1, _commandBuffer, 1);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("Turbosight remote: failed to stop receiver {0} {1} {2}, hr = 0x{3:x} ({4})", i + 1, device.Name, device.DevicePath, hr, HResult.GetDXErrorString(hr));
          }
          i++;
        }

        if (_commandBuffer != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(_commandBuffer);
          _commandBuffer = IntPtr.Zero;
        }
        if (_codeBuffer != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(_codeBuffer);
          _codeBuffer = IntPtr.Zero;
        }

        _isRemoteControlInterfaceOpen = false;
      }
      this.LogDebug("Turbosight remote: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_isTurbosightRemote)
      {
        CloseRemoteControlInterface();
      }
      if (_graph != null)
      {
        foreach (IBaseFilter filter in _filters)
        {
          _graph.RemoveFilter(filter);
        }
        Release.ComObject("Turbosight remote graph", ref _graph);
      }
      for (int i = 0; i < _filters.Count; i++)
      {
        IBaseFilter filter = _filters[i];
        Release.ComObject(string.Format("Turbosight remote filter {0}", i), ref filter);
      }
      _filters.Clear();
      foreach (DsDevice device in _devices)
      {
        device.Dispose();
      }
      lock (_instanceLock)
      {
        _isLoaded = false;
      }
      _isTurbosightRemote = false;
    }

    #endregion
  }
}