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
using System.IO;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl
{
  internal class RemoteControlNxp : ITurbosightRemoteControl, INxpRemoteControlKeyPressCallBack
  {
    #region COM interfaces

    /// <summary>
    /// MediaPortal's wrapper class for the TBS NXP infra red remote control
    /// receiver.
    /// </summary>
    [Guid("fa698ab4-c8fe-4823-a638-4d1f2f2bf8bf")]
    private class MpTbsNxpIrRcReceiver
    {
    }

    /// <summary>
    /// The main interface on the TBS NXP IR RC receiver wrapper class.
    /// </summary>
    [Guid("1cde3f2e-c771-4e42-9b9b-c32bd2467fcb"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMpTbsNxpIrRcReceiver
    {
      [PreserveSig]
      int Initialise(IKsControl control, NxpRemoteControlProtocol protocol, INxpRemoteControlKeyPressCallBack callBack, IntPtr callBackContext);

      [PreserveSig]
      int Dispose();
    }

    #endregion

    #region variables

    private static HashSet<string> _openProducts = new HashSet<string>();

    private bool _isInterfaceOpen = false;
    private string _productInstanceId = null;
    private DsDevice _device = null;
    private IKsControl _control = null;
    private IMpTbsNxpIrRcReceiver _receiver = null;

    #endregion

    public RemoteControlNxp(string productInstanceId)
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
      this.LogDebug("Turbosight NXP RC: open interface, product instance ID = {0}", _productInstanceId ?? "[null]");
      if (_isInterfaceOpen)
      {
        this.LogWarn("Turbosight NXP RC: interface is already open");
        return true;
      }
      if (_productInstanceId == null)
      {
        this.LogDebug("Turbosight NXP RC: product instance identifier is null");
        return false;
      }
      if (_openProducts.Contains(_productInstanceId))
      {
        this.LogDebug("Turbosight NXP RC: multi-tuner product remote control opened for other tuner");
        return true;
      }

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.DeviceControlCategory);
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

          this.LogDebug("Turbosight NXP RC: found IR device, name = {0}, device path = {1}", name, devicePath);
          object obj = null;
          try
          {
            Guid filterClsid = typeof(IBaseFilter).GUID;
            device.Mon.BindToObject(null, null, ref filterClsid, out obj);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Turbosight NXP RC: failed to create IR filter instance, name = {0}, device path = {1}", name, devicePath);
            continue;
          }

          try
          {
            _control = obj as IKsControl;
            if (_control == null)
            {
              this.LogError("Turbosight NXP RC: IR filter is not a control, name = {0}, device path = {1}", name, devicePath);
              continue;
            }

            try
            {
              string fileName = Path.Combine(PathManager.BuildAssemblyRelativePath("Resources"), "TbsNxpIrRcReceiver.dll");
              _receiver = ComHelper.LoadComObjectFromFile(fileName, typeof(MpTbsNxpIrRcReceiver).GUID, typeof(IMpTbsNxpIrRcReceiver).GUID, true) as IMpTbsNxpIrRcReceiver;
            }
            catch (Exception ex)
            {
              this.LogError(ex, "Turbosight NXP RC: failed to load interface");
              continue;
            }
            int hr = _receiver.Initialise(_control, NxpRemoteControlProtocol.Nec32, this, IntPtr.Zero);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              this.LogError("Turbosight NXP RC: failed to start remote control, hr = 0x{0:x}, name = {1}, device path = {2}", hr, name, devicePath);
              continue;
            }

            _isInterfaceOpen = true;
            _device = device;
            _openProducts.Add(_productInstanceId);
            return true;
          }
          finally
          {
            if (!_isInterfaceOpen)
            {
              Release.ComObject("Turbosight NXP remote control IR filter candidate", ref obj);
              _control = null;
              if (_receiver != null)
              {
                _receiver.Dispose();
              }
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

      this.LogDebug("Turbosight NXP RC: failed to find remote control for product");
      return false;
    }

    /// <summary>
    /// Close the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool Close()
    {
      this.LogDebug("Turbosight NXP RC: close interface");

      if (_receiver != null)
      {
        _receiver.Dispose();
        Release.ComObject("Turbosight NXP remote control interface", ref _receiver);
      }
      if (_control != null)
      {
        Release.ComObject("Turbosight NXP remote control", ref _control);
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

    #region IRemoteControlKeyPressCallBack member

    /// <summary>
    /// Invoked by the wrapper when a remote control key press event is fired
    /// by the underlying driver.
    /// </summary>
    /// <param name="keyCode">The remote control key's unique code.</param>
    /// <param name="protocol">The protocol/format used to detect and process the code.</param>
    /// <param name="context">The optional context passed to the wrapper when the interface was initialised.</param>
    /// <returns>an HRESULT indicating whether the event was handled successfully</returns>
    public int OnKeyPress(uint keyCode, NxpRemoteControlProtocol protocol, IntPtr context)
    {
      if (protocol == NxpRemoteControlProtocol.Rc5)
      {
        uint fieldBit = (keyCode & 0x1000) >> 12;
        uint toggleBit = (keyCode & 0x800) >> 11;
        uint systemAddress = (keyCode & 0x7c0) >> 6;
        uint command = keyCode & 0x3f;
        this.LogDebug("Turbosight NXP RC: RC-5 remote control key press, field bit = {0}, toggle bit = {1} system address = {2}, command = {3}", fieldBit, toggleBit, systemAddress, command);
      }
      else if (protocol == NxpRemoteControlProtocol.Rc6)
      {
        uint header = keyCode >> 16;
        uint control = ((keyCode >> 8) & 0xff);
        uint information = keyCode & 0xff;
        this.LogDebug("Turbosight NXP RC: RC-6 remote control key press, header = {0}, control = {1}, information = {2}", header, control, information);
      }
      else if (protocol == NxpRemoteControlProtocol.Nec32 || protocol == NxpRemoteControlProtocol.Nec40)
      {
        uint address = keyCode >> 8;
        uint command = keyCode & 0xff;
        if (command < (int)RemoteCodeBig.MINIMUM_VALUE)
        {
          this.LogDebug("Turbosight NXP RC: NEC small remote control key press, address = {0}, command = {1}", address, (RemoteCodeSmall)command);
        }
        else
        {
          this.LogDebug("Turbosight NXP RC: NEC big remote control key press, address = {0}, command = {1}", address, (RemoteCodeBig)command);
        }
      }
      else
      {
        this.LogDebug("Turbosight NXP RC: unknkown remote control key press, protocol = {0}, key code = 0x{1:x8}", protocol, keyCode);
      }
      return (int)NativeMethods.HResult.S_OK;
    }

    #endregion
  }
}