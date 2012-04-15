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
using TvLibrary.Channels;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling DiSEqC and DVB-S2 tuning for Geniatech tuners.
  /// </summary>
  public class Geniatech : ConexantBDA, IDiSEqCController, IDisposable
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

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
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

    /// <summary>
    /// Initialises a new instance of the <see cref="Geniatech"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Geniatech(IBaseFilter tunerFilter)
      : base(tunerFilter)
    {
      if (!IsConexant)
      {
        return;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr == 0 && (support & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Geniatech: supported tuner detected");
        _isGeniatech = true;

        SetPowerState(true);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Geniatech-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Geniatech-compatible tuner, otherwise <c>false</c></value>
    public bool IsGeniatech
    {
      get
      {
        return _isGeniatech;
      }
    }

    /// <summary>
    /// Turn the LNB or aerial power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn power supply on, otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if the power supply state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      Log.Log.Debug("Geniatech: set power state, on = {0}", powerOn);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Geniatech: property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
        Log.Log.Debug("Geniatech: result = success");
        return true;
      }

      Log.Log.Debug("Geniatech: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Geniatech: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      if (ch.ModulationType == ModulationType.ModQpsk)
      {
        ch.ModulationType = ModulationType.ModNbcQpsk;
      }
      else if (ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.ModNbc8Psk;
      }
      Log.Log.WriteFile("  modulation = {0}", ch.ModulationType);

      NbcParams nbcParams = new NbcParams();
      if (ch.Pilot == Pilot.On)
      {
        nbcParams.Pilot = GtPilot.On;
      }
      else
      {
        nbcParams.Pilot = GtPilot.Off;
      }
      Log.Log.WriteFile("  pilot      = {0}", nbcParams.Pilot);

      if (ch.Rolloff == RollOff.Twenty)
      {
        nbcParams.RollOff = GtRollOff.Twenty;
      }
      else if (ch.Rolloff == RollOff.TwentyFive)
      {
        nbcParams.RollOff = GtRollOff.TwentyFive;
      }
      else if (ch.Rolloff == RollOff.ThirtyFive)
      {
        nbcParams.RollOff = GtRollOff.ThirtyFive;
      }
      else
      {
        nbcParams.RollOff = GtRollOff.Undefined;
      }
      Log.Log.WriteFile("  roll-off   = {0}", nbcParams.RollOff);

      Marshal.StructureToPtr(nbcParams, _paramBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
        _instanceBuffer, InstanceSize,
        _paramBuffer, NbcParamsSize
      );
      if (hr != 0)
      {
        Log.Log.Debug("Geniatech: failed to set pilot and roll-off, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return ch as DVBBaseChannel;
    }

    #region IDisposable member

    /// <summary>
    /// Turn off power.
    /// </summary>
    public override void Dispose()
    {
      SetPowerState(false);
      base.Dispose();
    }

    #endregion
  }
}