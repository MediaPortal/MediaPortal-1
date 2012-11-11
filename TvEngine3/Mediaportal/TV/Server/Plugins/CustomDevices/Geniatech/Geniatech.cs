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
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Geniatech
{
  /// <summary>
  /// A class for handling DiSEqC and DVB-S2 tuning for Geniatech devices.
  /// </summary>
  public class Geniatech : Conexant.Conexant, IPowerDevice
  {
    #region enums

    private new enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For initialising DiSEqC interfaces.
      DiseqcInit,
      /// Unsupported generic Conexant property.
      ScanFrequency,
      /// For direct/custom tuning.
      ChannelChange,
      /// For retrieving demodulator firmware state and version.
      DemodInfo,
      /// Unsupported generic Conexant property.
      EffectiveFrequency,
      /// For retrieving signal quality, strength, BER and other attributes.
      SignalStatus,
      /// For retrieving demodulator lock indicators.
      LockStatus,
      /// For controlling error correction and BER window.
      ErrorControl,
      /// For retrieving the locked values of frequency, symbol rate etc. after fine tuning.
      ChannelInfo,
      /// For setting DVB-S2 parameters that could not initially be set through BDA interfaces.
      NbcParams,
      /// For controlling the LNB power supply state.
      LnbPower
    }

    private enum GtPilot
    {
      Off = 0,
      On,
      Unknown               // (Not used...)
    }

    private enum GtRollOff
    {
      Undefined = 0xff,
      Twenty = 0,           // 0.2
      TwentyFive,           // 0.25
      ThirtyFive            // 0.35
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NbcParams
    {
      public GtRollOff RollOff;
      public GtPilot Pilot;
    }

    #endregion

    #region constants

    private const int NbcParamsSize = 8;

    #endregion

    #region variables

    private bool _isGeniatech = false;

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // TeVii, Hauppauge, Geniatech, Turbosight, DVBSky, Prof and possibly others all use or implement the
        // same Conexant property set for DiSEqC support, often adding custom extensions. In order to ensure
        // that the full device functionality is available for all hardware we use the following priority
        // hierarchy:
        // TeVii [75] > Hauppauge, DVBSky, Turbosight [70] > Prof (USB) [65] > Prof (PCI, PCIe) [60] > Geniatech [50] > Conexant [40]
        return 50;
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
      this.LogDebug("Geniatech: initialising device");

      if (_isGeniatech)
      {
        this.LogDebug("Geniatech: device is already initialised");
        return true;
      }

      bool result = base.Initialise(tunerFilter, tunerType, tunerDevicePath);
      if (!result)
      {
        this.LogDebug("Geniatech: base Conexant interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Geniatech: device does not support the NBC parameter property, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("Geniatech: supported device detected");
      _isGeniatech = true;
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out DeviceAction action)
    {
      this.LogDebug("Geniatech: on before tune callback");
      action = DeviceAction.Default;

      if (!_isGeniatech || _propertySet == null)
      {
        this.LogDebug("Geniatech: device not initialised or interface not supported");
        return;
      }

      // We only have work to do if the channel is a DVB-S/2 channel.
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }

      if (ch.ModulationType == ModulationType.ModQpsk)
      {
        ch.ModulationType = ModulationType.ModNbcQpsk;
      }
      else if (ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.ModNbc8Psk;
      }
      this.LogDebug("  modulation = {0}", ch.ModulationType);

      NbcParams nbcParams = new NbcParams();
      if (ch.Pilot == Pilot.On)
      {
        nbcParams.Pilot = GtPilot.On;
      }
      else
      {
        nbcParams.Pilot = GtPilot.Off;
      }
      this.LogDebug("  pilot      = {0}", nbcParams.Pilot);

      if (ch.RollOff == RollOff.Twenty)
      {
        nbcParams.RollOff = GtRollOff.Twenty;
      }
      else if (ch.RollOff == RollOff.TwentyFive)
      {
        nbcParams.RollOff = GtRollOff.TwentyFive;
      }
      else if (ch.RollOff == RollOff.ThirtyFive)
      {
        nbcParams.RollOff = GtRollOff.ThirtyFive;
      }
      else
      {
        nbcParams.RollOff = GtRollOff.Undefined;
      }
      this.LogDebug("  roll-off   = {0}", nbcParams.RollOff);

      Marshal.StructureToPtr(nbcParams, _paramBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
        _instanceBuffer, InstanceSize,
        _paramBuffer, NbcParamsSize
      );
      if (hr != 0)
      {
        this.LogDebug("Geniatech: failed to set pilot and roll-off, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      this.LogDebug("Geniatech: set power state, on = {0}", powerOn);

      if (!_isGeniatech || _propertySet == null)
      {
        this.LogDebug("Geniatech: device not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Geniatech: LNB power property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      if (powerOn)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 1);
      }
      else
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
      }
      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower,
        _instanceBuffer, InstanceSize,
        _paramBuffer, sizeof(Int32)
      );
      if (hr == 0)
      {
        this.LogDebug("Geniatech: result = success");
        return true;
      }

      this.LogDebug("Geniatech: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      _isGeniatech = false;
    }

    #endregion
  }
}