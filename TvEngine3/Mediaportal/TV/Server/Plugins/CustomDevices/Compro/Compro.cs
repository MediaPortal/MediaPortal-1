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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Compro
{
  public class Compro : BaseCustomDevice, IPowerDevice, IDiseqcDevice
  {
    #region enums

    private enum BdaExtensionDiseqcProperty
    {
      DiseqcBasic = 0,
      DiseqcRaw,
      TonePower
    }

    private enum BdaExtensionProperty
    {
      MacAddress = 0
    }

    private enum Compro22k : byte
    {
      Off = 0,
      On
    }

    private enum ComproLnbPower : byte
    {
      Off = 0x02,
      On = 0x03
    }

    private enum ComproSwitchType
    {
      None = 0,
      Single,
      Tone,               // 22 kHz 2 port
      Mini,               // tone burst 2 port
      Diseqc1_0,          // DiSEqC 1.0 4 port (committed)
      Diseqc1_2           // DiSEqC 1.2 16 port (uncommitted)
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionDiseqcPropertySet = new Guid(0x0c12bf87, 0x5bc0, 0x4dda, 0x9d, 0x07, 0x21, 0xe5, 0xc2, 0xf3, 0xb9, 0xae);
    private static readonly Guid BdaExtensionPropertySet = new Guid(0xa1aa3f96, 0x2ea, 0x4ccb, 0xa7, 0x14, 0x0, 0xbc, 0xd3, 0x98, 0xad, 0xb4);

    private const int KsPropertySize = 24;
    private const int MaxDiseqcMessageLength = 8;
    private const int MacAddressLength = 6;
    private const int GeneralBufferSize = KsPropertySize + 4;

    #endregion

    #region variables

    private bool _isCompro = false;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _commandBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

    #endregion

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      this.LogDebug("Compro: read device information");

      // MAC address.
      this.LogDebug("Compro: reading MAC address");
      for (int i = 0; i < MacAddressLength; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.MacAddress,
        _generalBuffer, MacAddressLength,
        _generalBuffer, MacAddressLength,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != MacAddressLength)
      {
        this.LogDebug("Compro: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        String address = String.Empty;
        for (int i = 0; i < returnedByteCount; i++)
        {
          address += String.Format("{0:x2}-", Marshal.ReadByte(_generalBuffer, i));
        }
        this.LogDebug("  MAC address = {0}", address.Substring(0, (returnedByteCount * 3) - 1));
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return "Compro";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      this.LogDebug("Compro: initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("Compro: tuner filter is null");
        return false;
      }
      if (_isCompro)
      {
        this.LogDebug("Compro: device is already initialised");
        return true;
      }

      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Compro: tuner filter is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.DiseqcRaw, out support);
      if (hr != 0 || (support & KSPropertySupport.Get) == 0)
      {
        this.LogDebug("Compro: device does not support the Compro property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("Compro: supported device detected");
      _isCompro = true;
      _generalBuffer = Marshal.AllocCoTaskMem(GeneralBufferSize);
      _commandBuffer = Marshal.AllocCoTaskMem(MaxDiseqcMessageLength);
      ReadDeviceInfo();
      return true;
    }

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      this.LogDebug("Compro: set power state, on = {0}", powerOn);

      if (!_isCompro || _propertySet == null)
      {
        this.LogDebug("Compro: device not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.TonePower, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Compro: property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      if (powerOn)
      {
        Marshal.WriteByte(_commandBuffer, 0, (byte)ComproLnbPower.On);
      }
      else
      {
        Marshal.WriteByte(_commandBuffer, 0, (byte)ComproLnbPower.Off);
      }

      hr = _propertySet.Set(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.TonePower,
        IntPtr.Zero, 0,
        _commandBuffer, 1
      );
      if (hr == 0)
      {
        this.LogDebug("Compro: result = success");
        return true;
      }

      this.LogDebug("Compro: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22k">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Compro: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isCompro || _propertySet == null)
      {
        this.LogDebug("Compro: device not initialised or interface not supported");
        return false;
      }

      bool success = true;
      KSPropertySupport support;
      int hr;

      if (toneBurstState != ToneBurst.None)
      {
        hr = _propertySet.QuerySupported(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.DiseqcBasic,
                                    out support);
        if (hr != 0 || (support & KSPropertySupport.Set) == 0)
        {
          this.LogDebug("Compro: DiSEqC basic property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }
        else
        {
          Marshal.WriteInt32(_generalBuffer, 0, (int)ComproSwitchType.Mini);
          if (toneBurstState == ToneBurst.ToneBurst)
          {
            Marshal.WriteInt32(_commandBuffer, 0, 0);
          }
          else if (toneBurstState == ToneBurst.DataBurst)
          {
            Marshal.WriteInt32(_commandBuffer, 0, 1);
          }
          hr = _propertySet.Set(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.DiseqcBasic,
            _generalBuffer, 4,
            _commandBuffer, 4
          );
          if (hr != 0)
          {
            this.LogDebug("Compro: failed to set tone state, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            success = false;
          }
        }
      }

      hr = _propertySet.QuerySupported(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.TonePower,
                                  out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Compro: tone/power property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        if (tone22kState == Tone22k.On)
        {
          Marshal.WriteByte(_commandBuffer, 0, (byte)Compro22k.On);
        }
        else
        {
          Marshal.WriteByte(_commandBuffer, 0, (byte)Compro22k.Off);
        }
        hr = _propertySet.Set(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.TonePower,
          IntPtr.Zero, 0,
          _commandBuffer, 1
        );
        if (hr != 0)
        {
          this.LogDebug("Compro: failed to set 22 kHz state, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      if (success)
      {
        this.LogDebug("Compro: result = success");
      }
      return success;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      this.LogDebug("Compro: send DiSEqC command");

      if (!_isCompro || _propertySet == null)
      {
        this.LogDebug("Compro: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("Compro: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        this.LogDebug("Compro: command too long, length = {0}", command.Length);
        return false;
      }

      for (int i = 0; i < GeneralBufferSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      Marshal.WriteInt32(_generalBuffer, 0, command.Length);

      Marshal.Copy(command, 0, _commandBuffer, command.Length);

      int hr = _propertySet.Set(BdaExtensionDiseqcPropertySet, (int)BdaExtensionDiseqcProperty.DiseqcRaw,
        _generalBuffer, GeneralBufferSize,
        _commandBuffer, MaxDiseqcMessageLength
      );
      if (hr == 0)
      {
        this.LogDebug("Compro: result = success");
        return true;
      }

      this.LogDebug("Compro: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      // Not implemented.
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (_commandBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_commandBuffer);
        _commandBuffer = IntPtr.Zero;
      }
      _isCompro = false;
    }

    #endregion
  }
}
