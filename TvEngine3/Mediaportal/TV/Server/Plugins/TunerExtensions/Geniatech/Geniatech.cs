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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Geniatech
{
  /// <summary>
  /// A class for handling DiSEqC and DVB-S2 tuning for Geniatech tuners.
  /// </summary>
  public class Geniatech : BaseTunerExtension, IDiseqcDevice, IDisposable, IPowerDevice
  {
    #region enums

    private enum BdaExtensionProperty
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

    private enum GtRollOffFactor
    {
      Undefined = 0xff,
      Twenty = 0,           // 0.2
      TwentyFive,           // 0.25
      ThirtyFive            // 0.35
    }

    private enum GtPilotTonesState
    {
      Off = 0,
      On,
      Unknown               // (Not used...)
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NbcParams
    {
      public GtRollOffFactor RollOffFactor;
      public GtPilotTonesState PilotTonesState;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private static readonly int NBC_PARAMS_SIZE = Marshal.SizeOf(typeof(NbcParams));    // 8
    private static readonly int PARAM_BUFFER_SIZE = NBC_PARAMS_SIZE;

    #endregion

    #region variables

    private bool _isGeniatech = false;
    private IDiseqcDevice _diseqcInterface = null;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

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
      this.LogDebug("Geniatech: initialising");

      if (_isGeniatech)
      {
        this.LogWarn("Geniatech: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Geniatech: tuner type not supported");
        return false;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Geniatech: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Geniatech: pin is not a property set");
        Release.ComObject("Geniatech filter input pin", ref pin);
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Geniatech: NBC parameter property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        Release.ComObject("Geniatech filter input pin", ref pin);
        _propertySet = null;
        return false;
      }

      this.LogInfo("Geniatech: extension supported");
      _isGeniatech = true;
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      _paramBuffer = Marshal.AllocCoTaskMem(PARAM_BUFFER_SIZE);

      _diseqcInterface = new Conexant.Conexant();
      if (!_diseqcInterface.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context))
      {
        this.LogWarn("Geniatech: failed to initialise base Conexant interface");
        IDisposable d = _diseqcInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _diseqcInterface = null;
      }
      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Geniatech: on before tune call back");
      action = TunerAction.Default;

      if (!_isGeniatech)
      {
        this.LogWarn("Geniatech: not initialised or interface not supported");
        return;
      }

      // We only have work to do if the channel is a satellite channel.
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      // For non-DVB-S2, maybe these NBC parameter values should be 0.35/off...
      // or maybe we shouldn't even use the property.
      NbcParams nbcParams = new NbcParams();
      nbcParams.RollOffFactor = GtRollOffFactor.Undefined;
      nbcParams.PilotTonesState = GtPilotTonesState.Unknown;
      RollOffFactor rollOffFactor = RollOffFactor.Automatic;
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (dvbs2Channel != null)
      {
        // I'm not sure if these modulation values can be used on XP.
        ModulationType bdaModulation = ModulationType.ModNotSet;
        if (dvbs2Channel.ModulationScheme == ModulationSchemePsk.Psk4)
        {
          bdaModulation = ModulationType.ModNbcQpsk;
        }
        else if (dvbs2Channel.ModulationScheme == ModulationSchemePsk.Psk8)
        {
          bdaModulation = ModulationType.ModNbc8Psk;
        }
        if (bdaModulation != ModulationType.ModNotSet)
        {
          this.LogDebug("  modulation      = {0}", bdaModulation);
          dvbs2Channel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
        }

        rollOffFactor = dvbs2Channel.RollOffFactor;
        switch (dvbs2Channel.PilotTonesState)
        {
          case PilotTonesState.On:
            nbcParams.PilotTonesState = GtPilotTonesState.On;
            break;
          case PilotTonesState.Off:
            nbcParams.PilotTonesState = GtPilotTonesState.Off;
            break;
          default:
            this.LogWarn("Geniatech: DVB-S2 tune request uses unsupported pilot tones state {0}", dvbs2Channel.PilotTonesState);
            break;
        }
        this.LogDebug("  pilot tones     = {0}", nbcParams.PilotTonesState);
      }
      else
      {
        ChannelDvbDsng dvbDsngChannel = channel as ChannelDvbDsng;
        if (dvbDsngChannel != null)
        {
          rollOffFactor = dvbDsngChannel.RollOffFactor;
        }
      }

      if (rollOffFactor != RollOffFactor.Automatic)
      {
        switch (rollOffFactor)
        {
          case RollOffFactor.Twenty:
            nbcParams.RollOffFactor = GtRollOffFactor.Twenty;
            break;
          case RollOffFactor.TwentyFive:
            nbcParams.RollOffFactor = GtRollOffFactor.TwentyFive;
            break;
          case RollOffFactor.ThirtyFive:
            nbcParams.RollOffFactor = GtRollOffFactor.ThirtyFive;
            break;
          default:
            this.LogWarn("Geniatech: DVB-DSNG/DVB-S2 tune request uses unsupported roll-off factor {0}", rollOffFactor);
            break;
        }
        this.LogDebug("  roll-off factor = {0}", nbcParams.RollOffFactor);
      }

      Marshal.StructureToPtr(nbcParams, _paramBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.NbcParams,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, NBC_PARAMS_SIZE
      );
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Geniatech: failed to set pilot tones state and roll-off factor, hr = 0x{0:x}", hr);
      }
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("Geniatech: set power state, state = {0}", state);

      if (!_isGeniatech)
      {
        this.LogWarn("Geniatech: not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Geniatech: LNB power property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      if (state == PowerState.On)
      {
        Marshal.WriteInt32(_paramBuffer, 0, 1);
      }
      else
      {
        Marshal.WriteInt32(_paramBuffer, 0, 0);
      }
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbPower,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Geniatech: result = success");
        return true;
      }

      this.LogError("Geniatech: failed to set power state, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.SendCommand(command);
      }
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.SendCommand(command);
      }
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      if (_diseqcInterface != null)
      {
        // Set by tune request LNB frequency parameters.
        return _diseqcInterface.SetToneState(state);
      }
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.ReadResponse(out response);
      }
      response = null;
      return false;
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

    ~Geniatech()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        Release.ComObject("Geniatech property set", ref _propertySet);
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (_paramBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_paramBuffer);
        _paramBuffer = IntPtr.Zero;
      }
      if (isDisposing && _diseqcInterface != null)
      {
        IDisposable d = _diseqcInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _diseqcInterface = null;
      }
      _isGeniatech = false;
    }

    #endregion
  }
}