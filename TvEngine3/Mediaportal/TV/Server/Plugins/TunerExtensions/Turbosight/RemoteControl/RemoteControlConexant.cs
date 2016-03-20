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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl
{
  internal class RemoteControlConexant : ITurbosightRemoteControl
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

    private static readonly TimeSpan REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = new TimeSpan(0, 0, 0, 0, 100);

    #endregion

    #region variables

    private static HashSet<string> _openProducts = new HashSet<string>();

    private bool _isInterfaceOpen = false;
    private string _productInstanceId = null;
    private DsDevice _device = null;
    private IKsPropertySet _propertySet = null;

    private Thread _listenerThread = null;
    private ManualResetEvent _listenerThreadStopEvent = null;

    #endregion

    public RemoteControlConexant(string productInstanceId)
    {
      _productInstanceId = productInstanceId;
    }

    #region ITurbosightRemoteControl members

    /// <summary>
    /// Open the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool Open()
    {
      this.LogDebug("Turbosight Conexant RC: open interface, product instance ID = {0}", _productInstanceId ?? "[null]");
      if (_isInterfaceOpen)
      {
        this.LogWarn("Turbosight Conexant RC: interface is already open");
        return true;
      }
      if (_productInstanceId == null)
      {
        this.LogDebug("Turbosight Conexant RC: product instance identifier is null");
        return false;
      }
      if (_openProducts.Contains(_productInstanceId))
      {
        this.LogDebug("Turbosight Conexant RC: multi-tuner product remote control opened for other tuner");
        return true;
      }

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
      try
      {
        foreach (DsDevice device in devices)
        {
          string name = device.Name;
          string devicePath = device.DevicePath;
          if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath) || !string.Equals(device.ProductInstanceIdentifier, _productInstanceId))
          {
            continue;
          }

          this.LogDebug("Turbosight Conexant RC: found IR device, name = {0}, device path = {1}", name, devicePath);
          object obj = null;
          try
          {
            Guid filterClsid = typeof(IBaseFilter).GUID;
            device.Mon.BindToObject(null, null, ref filterClsid, out obj);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Turbosight Conexant RC: failed to create IR filter instance, name = {0}, device path = {1}", name, devicePath);
            continue;
          }

          try
          {
            _propertySet = obj as IKsPropertySet;
            if (_propertySet == null)
            {
              this.LogError("Turbosight Conexant RC: IR filter is not a property set, name = {0}, device path = {1}", name, devicePath);
              continue;
            }

            KSPropertySupport support;
            int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, out support);
            if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
            {
              this.LogError("Turbosight Conexant RC: IR filter does not support property set, hr = 0x{0:x}, support = {1} name = {2}, device path = {3}", hr, support, name, devicePath);
              continue;
            }

            IntPtr commandBuffer = Marshal.AllocCoTaskMem(1);
            try
            {
              Marshal.WriteByte(commandBuffer, 0, (byte)TbsIrCommand.Start);
              hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, commandBuffer, 1, commandBuffer, 1);
              if (hr != (int)NativeMethods.HResult.S_OK)
              {
                this.LogError("Turbosight Conexant RC: failed to start remote control, hr = 0x{0:x}, name = {1}, device path = {2}", hr, name, devicePath);
                continue;
              }
            }
            finally
            {
              Marshal.FreeCoTaskMem(commandBuffer);
            }

            _isInterfaceOpen = true;
            _openProducts.Add(_productInstanceId);
            _device = device;
            StartListenerThread();
            return true;
          }
          finally
          {
            if (!_isInterfaceOpen)
            {
              Release.ComObject("Turbosight Conexant remote control IR filter candidate", ref obj);
              _propertySet = null;
            }
          }
        }
      }
      finally
      {
        foreach (DsDevice device in devices)
        {
          if (device != _device)
          {
            device.Dispose();
          }
        }
      }

      this.LogDebug("Turbosight Conexant RC: failed to find remote control for product");
      return false;
    }

    /// <summary>
    /// Close the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool Close()
    {
      this.LogDebug("Turbosight Conexant RC: close interface");

      StopListenerThread();

      if (_propertySet != null)
      {
        IntPtr commandBuffer = Marshal.AllocCoTaskMem(1);
        try
        {
          Marshal.WriteByte(commandBuffer, 0, (byte)TbsIrCommand.Stop);
          int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Command, commandBuffer, 1, commandBuffer, 1);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("Turbosight Conexant RC: failed to stop remote control, hr = 0x{0:x}, name = {1}, device path = {2}", hr, _device.Name, _device.DevicePath);
          }
          Release.ComObject("Turbosight Conexant remote control property set", ref _propertySet);
        }
        finally
        {
          Marshal.FreeCoTaskMem(commandBuffer);
        }
      }
      if (_device != null)
      {
        _device.Dispose();
        _device = null;
      }

      if (_isInterfaceOpen)
      {
        if (_productInstanceId != null)
        {
          _openProducts.Remove(_productInstanceId);
        }
        _isInterfaceOpen = false;
      }

      return true;
    }

    #endregion

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
        this.LogDebug("Turbosight Conexant RC: starting new listener thread");
        _listenerThreadStopEvent = new ManualResetEvent(false);
        _listenerThread = new Thread(new ThreadStart(Listener));
        _listenerThread.Name = "Turbosight Conexant remote control listener";
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
          this.LogWarn("Turbosight Conexant RC: aborting old listener thread");
          _listenerThread.Abort();
        }
        else
        {
          _listenerThreadStopEvent.Set();
          if (!_listenerThread.Join((int)REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME.TotalMilliseconds * 2))
          {
            this.LogWarn("Turbosight Conexant RC: failed to join listener thread, aborting thread");
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
      this.LogDebug("Turbosight Conexant RC: listener thread start polling");

      IntPtr instanceBuffer = Marshal.AllocCoTaskMem(KS_PROPERTY_SIZE);
      IntPtr codeBuffer = Marshal.AllocCoTaskMem(IR_DATA_SIZE);
      for (int i = 0; i < KS_PROPERTY_SIZE; i++)
      {
        Marshal.WriteByte(instanceBuffer, i, 0);
      }
      int hr;
      int returnedByteCount;
      try
      {
        while (!_listenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Code,
            instanceBuffer, KS_PROPERTY_SIZE,
            codeBuffer, IR_DATA_SIZE,
            out returnedByteCount
          );
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != IR_DATA_SIZE)
          {
            this.LogError("Turbosight Conexant RC: failed to read remote code, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          }
          else
          {
            IrData data = (IrData)Marshal.PtrToStructure(codeBuffer, typeof(IrData));
            if (data.Address != 0xffff && data.Command != 0xffff && data.Command != 0xff && data.Command != 0)
            {
              this.LogDebug("Turbosight Conexant RC: key press, address = 0x{0:x8}, command = 0x{1:x8}", data.Address, data.Command);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Turbosight Conexant RC: listener thread exception");
        return;
      }
      finally
      {
        Marshal.FreeCoTaskMem(instanceBuffer);
        Marshal.FreeCoTaskMem(codeBuffer);
      }
      this.LogDebug("Turbosight Conexant RC: listener thread stop polling");
    }

    #endregion
  }
}