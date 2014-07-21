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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Kworld
{
  /// <summary>
  /// A class for handling LNB power control for the KWorld VS-DVB-S 100/IS.
  /// </summary>
  public class Kworld : BaseCustomDevice, IPowerDevice
  {
    #region enums

    private enum BdaExtensionProperty
    {
      I2cAccess = 4,
      RegisterAccess = 6
    }

    private enum I2cAccessOperation : int
    {
      IsInitOkay = 0x11000001,
      InitHwMode = 0x11000002
    }

    private enum RegisterAccessOperation : int
    {
      SetAddress = 0x30000001,
      Read = 0x30000003,
      Write = 0x30000005
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Data
    {
      public KsProperty Property;
      public int Operation;
      public int Address;
      public int Value;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xc44a1a10, 0x0a37, 0x11d2, 0x83, 0x28, 0x00, 0x60, 0x97, 0xba, 0x83, 0xab);

    private const int COMMAND_I2C_RESET = 100000;
    private const int REGISTER_ADDRESS_MO_GP0_IO = 0x350010;

    private const int I2C_DATA_SIZE = 120;
    private const int REGISTER_DATA_SIZE = 40;

    private static readonly int BUFFER_SIZE = Math.Max(I2C_DATA_SIZE, REGISTER_DATA_SIZE);

    #endregion

    #region variables

    private bool _isKworld = false;
    private DsDevice _device = null;
    private IKsObject _ksObject = null;
    private IntPtr _ksObjectHandle = IntPtr.Zero;
    private IntPtr _buffer = IntPtr.Zero;

    #endregion

    private int Ioctl(BdaExtensionProperty property, KsPropertyFlag propertyFlag, int operation, int address, int value, out Data data)
    {
      uint bufferSize;
      if (property == BdaExtensionProperty.I2cAccess)
      {
        bufferSize = I2C_DATA_SIZE;
      }
      else
      {
        bufferSize = REGISTER_DATA_SIZE;
      }
      for (int i = 0; i < bufferSize; i++)
      {
        Marshal.WriteByte(_buffer, i, 0);
      }
      data = new Data();
      data.Property = new KsProperty();
      data.Property.Set = BDA_EXTENSION_PROPERTY_SET;
      data.Property.Id = (int)property;
      data.Property.Flags = propertyFlag;
      data.Operation = (int)operation;
      data.Address = address;
      data.Value = value;
      Marshal.StructureToPtr(data, _buffer, false);
      uint returnedByteCount;
      if (!NativeMethods.DeviceIoControl(_ksObjectHandle, NativeMethods.IOCTL_KS_PROPERTY, _buffer, bufferSize, _buffer, bufferSize, out returnedByteCount, IntPtr.Zero))
      {
        return Marshal.GetLastWin32Error();
      }
      if (propertyFlag == KsPropertyFlag.Get)
      {
        data = (Data)Marshal.PtrToStructure(_buffer, typeof(Data));
      }
      return (int)HResult.Severity.Success;
    }

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "KWorld";
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
      this.LogDebug("KWorld: initialising");

      if (_isKworld)
      {
        this.LogWarn("KWorld: extension already initialised");
        return true;
      }

      if (tunerType != CardType.DvbS)
      {
        this.LogDebug("KWorld: tuner type not supported");
        return false;
      }

      // Find the corresponding BDA source. Note we can't directly check the
      // tuner's external ID for the PCI ID because the driver is a stream
      // class driver.
      string productInstanceId = null;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      try
      {
        foreach (DsDevice d in devices)
        {
          string devicePath = d.DevicePath;
          if (devicePath != null && tunerExternalId.Contains(devicePath))
          {
            this.LogDebug("KWorld: found BDA source");
            this.LogDebug("KWorld:   name                = {0}", d.Name);
            this.LogDebug("KWorld:   device path         = {0}", devicePath);
            this.LogDebug("KWorld:   product instance ID = {0}", d.ProductInstanceIdentifier);
            productInstanceId = d.ProductInstanceIdentifier;
            break;
          }
        }
        if (productInstanceId == null)
        {
          this.LogDebug("KWorld: not a BDA source");
          return false;
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          d.Dispose();
        }
      }

      // Find the corresponding video input (capture) device.
      devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
      try
      {
        foreach (DsDevice d in devices)
        {
          if (productInstanceId.Equals(d.ProductInstanceIdentifier))
          {
            this.LogDebug("KWorld: found video input device");
            this.LogDebug("KWorld:   name                = {0}", d.Name);
            this.LogDebug("KWorld:   device path         = {0}", d.DevicePath);

            // We have to be careful to restrict to hardware that we know is
            // compatible. The code used in this extension is somewhat generic
            // in that it probably applies to a range of hardware using
            // Conexant CX2388x chips. However the purpose of the register we
            // manipulate varies from design to design. Enabling the extension
            // for incompatible hardware could result in unpredictable results
            // including hardware damage.
            //
            // The regex only checks for the KWorld VS-DVB-S 100/IS.
            // - The 88\d{2} part is because the driver is a stream class
            //   driver and the ID for the video capture component is not
            //   known.
            // - The [236] part is because it seems there are 3 hardware
            //   revisions.
            Match m = Regex.Match(d.DevicePath, @"ven_14f1&dev_88\d{2}&subsys_08b[236]17de", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
              this.LogDebug("KWorld: hardware not supported");
              return false;
            }
            _device = d;
            break;
          }
        }
        if (_device == null)
        {
          this.LogWarn("KWorld: failed to find corresponding video input device, tuner external ID = {0}, product instance ID = {1}", tunerExternalId, productInstanceId);
          return false;
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          if (d != _device)
          {
            d.Dispose();
          }
        }
      }

      // Get the KS object handle.
      object obj = null;
      Guid filterIid = typeof(IBaseFilter).GUID;
      try
      {
        _device.Mon.BindToObject(null, null, ref filterIid, out obj);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "KWorld: failed to instanciate video input filter");
        return false;
      }
      _ksObject = obj as IKsObject;
      if (_ksObject == null)
      {
        this.LogDebug("KWorld: filter is not a KS object");
        Release.ComObject("KWorld video input filter", ref obj);
        return false;
      }

      _ksObjectHandle = _ksObject.KsGetObjectHandle();
      if (_ksObjectHandle == IntPtr.Zero)
      {
        this.LogDebug("KWorld: KS object handle is not valid");
        return false;
      }

      this.LogInfo("KWorld: extension supported");
      _isKworld = true;
      _buffer = Marshal.AllocCoTaskMem(BUFFER_SIZE);
      return true;
    }

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("KWorld: set power state, state = {0}", state);

      if (!_isKworld)
      {
        this.LogWarn("KWorld: not initialised or interface not supported");
        return false;
      }

      // Set the address of the register to read from.
      Data data;
      int hr = Ioctl(BdaExtensionProperty.RegisterAccess, KsPropertyFlag.Set, (int)RegisterAccessOperation.SetAddress, REGISTER_ADDRESS_MO_GP0_IO, 0, out data);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("KWorld: failed to set register address to read current config, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      // Read the current register value.
      hr = Ioctl(BdaExtensionProperty.RegisterAccess, KsPropertyFlag.Set, (int)RegisterAccessOperation.Read, 0, 0, out data);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("KWorld: failed to set-read current config to register, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      hr = Ioctl(BdaExtensionProperty.RegisterAccess, KsPropertyFlag.Get, (int)RegisterAccessOperation.Read, 0, 0, out data);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("KWorld: failed to get-read current config from register, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      // Set/unset the control bit (which is bit 1). Yes, unsetting turns the power on!
      int value = data.Value;
      if (state == PowerState.On)
      {
        value &= unchecked((int)0xfffffefd);
        value |= 0x00000200;
      }
      else
      {
        value |= 2;
      }

      // Write the change back into the register.
      hr = Ioctl(BdaExtensionProperty.RegisterAccess, KsPropertyFlag.Set, (int)RegisterAccessOperation.Write, REGISTER_ADDRESS_MO_GP0_IO, value, out data);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("KWorld: failed to write new config to register, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      // Reset the hardware and check it accepted the configuration.
      hr = Ioctl(BdaExtensionProperty.I2cAccess, KsPropertyFlag.Set, (int)I2cAccessOperation.InitHwMode, 0, COMMAND_I2C_RESET, out data);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("KWorld: failed to reset I2C after config update, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      hr = Ioctl(BdaExtensionProperty.I2cAccess, KsPropertyFlag.Set, (int)I2cAccessOperation.IsInitOkay, 0, COMMAND_I2C_RESET, out data);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("KWorld: failed to reinitialise hardware after config update and I2C reset, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("KWorld: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      _ksObjectHandle = IntPtr.Zero;
      Release.ComObject("KWorld KS object", ref _ksObject);
      if (_device != null)
      {
        _device.Dispose();
      }
      if (_buffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_buffer);
      }
      _isKworld = false;
    }

    #endregion
  }
}