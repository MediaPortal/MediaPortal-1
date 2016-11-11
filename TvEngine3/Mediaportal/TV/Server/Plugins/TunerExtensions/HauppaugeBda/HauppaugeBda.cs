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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBda
{
  /// <summary>
  /// A class for handling DiSEqC and DVB-S2 tuning for Hauppauge BDA tuners.
  /// </summary>
  public class HauppaugeBda : BaseTunerExtension, IDiseqcDevice, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For setting the DVB-S2 pilot tones state parameter value.
      PilotTonesState = 32,
      /// For setting the DVB-S2 roll-off factor parameter value.
      RollOffFactor = 33
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0x00, 0xa0, 0xc9, 0xf2, 0x1f, 0xc7);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private const int PARAM_BUFFER_SIZE = 4;

    #endregion

    #region variables

    private bool _isHauppaugeBda = false;
    private IDiseqcDevice _diseqcInterface = null;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// Set the roll-off factor tuning parameter.
    /// </summary>
    /// <param name="rollOffFactor">The roll-off factor value.</param>
    /// <returns><c>true</c> if the setting is successfully applied, otherwise <c>false</c></returns>
    private bool SetRollOffFactor(RollOff rollOffFactor)
    {
      this.LogDebug("Hauppauge BDA: set roll-off factor = {0}", rollOffFactor);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.RollOffFactor, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Hauppauge BDA: roll-off factor property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return true;  // This is not an error.
      }

      Marshal.WriteInt32(_paramBuffer, 0, (int)rollOffFactor);
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.RollOffFactor,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Hauppauge BDA: result = success");
        return true;
      }

      this.LogError("Hauppauge BDA: failed to set roll-off, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the pilot tones state tuning parameter.
    /// </summary>
    /// <param name="pilotTonesState">The pilot tones state value.</param>
    /// <returns><c>true</c> if the setting is successfully applied, otherwise <c>false</c></returns>
    private bool SetPilotTonesState(Pilot pilotTonesState)
    {
      this.LogDebug("Hauppauge BDA: set pilot tones state = {0}", pilotTonesState);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PilotTonesState, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Hauppauge BDA: pilot tones state property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return true;  // This is not an error.
      }

      Marshal.WriteInt32(_paramBuffer, 0, (int)pilotTonesState);
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PilotTonesState,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Hauppauge BDA: result = success");
        return true;
      }

      this.LogError("Hauppauge BDA: failed to set pilot tones state, hr = 0x{0:x}", hr);
      return false;
    }

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 70;
      }
    }

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Hauppauge BDA";
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Hauppauge BDA: initialising");

      if (_isHauppaugeBda)
      {
        this.LogWarn("Hauppauge BDA: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Hauppauge BDA: tuner type not supported");
        return false;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Hauppauge BDA: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Hauppauge BDA: pin is not a property set");
        Release.ComObject("Hauppauge BDA filter input pin", ref pin);
        return false;
      }

      _diseqcInterface = new Conexant.Conexant(BDA_EXTENSION_PROPERTY_SET);
      if (!_diseqcInterface.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context))
      {
        this.LogDebug("Hauppauge BDA: base Conexant interface not supported");
        Release.ComObject("Hauppauge BDA filter input pin", ref pin);
        IDisposable d = _diseqcInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _diseqcInterface = null;
        return false;
      }

      this.LogInfo("Hauppauge BDA: extension supported");
      _isHauppaugeBda = true;
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      _paramBuffer = Marshal.AllocCoTaskMem(PARAM_BUFFER_SIZE);
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
      this.LogDebug("Hauppauge BDA: on before tune call back");
      action = TunerAction.Default;

      if (!_isHauppaugeBda)
      {
        this.LogWarn("Hauppauge BDA: not initialised or interface not supported");
        return;
      }

      // This is important. Hauppauge recommends that the graph be running when
      // tune requests are submitted and DiSEqC commands are sent. DiSEqC
      // commands *will* fail if you don't do this.
      action = TunerAction.Start;

      // We only have work to do if the channel is a satellite channel.
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      // This code is questionable. Some tuners definitely need modulation
      // translation... but whether the translation is the same for all tuners,
      // driver versions and operating systems is unclear. Same applies for
      // pilot tones state and roll-off factor.
      ModulationType bdaModulation = ModulationType.ModNotSet;
      RollOff bdaRollOffFactor = RollOff.NotSet;        // Correct for DVB-S/non-DVB-S2???
      Pilot bdaPilotTonesState = Pilot.NotSet;          // Correct for DVB-S/non-DVB-S2???
      RollOffFactor tveRollOffFactor = RollOffFactor.Automatic;
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (dvbs2Channel != null)
      {
        if (dvbs2Channel.ModulationScheme == ModulationSchemePsk.Psk4)
        {
          bdaModulation = ModulationType.ModNbcQpsk;    // ...or ModBpsk for XP???
        }
        else if (dvbs2Channel.ModulationScheme == ModulationSchemePsk.Psk8)
        {
          bdaModulation = ModulationType.ModNbc8Psk;    // ...or Mod8Psk for XP???
        }

        tveRollOffFactor = dvbs2Channel.RollOffFactor;
        switch (dvbs2Channel.PilotTonesState)
        {
          case PilotTonesState.Off:
            bdaPilotTonesState = Pilot.Off;
            break;
          case PilotTonesState.On:
            bdaPilotTonesState = Pilot.On;
            break;
          default:
            this.LogWarn("Hauppauge: DVB-S2 tune request uses unsupported pilot tones state {0}", dvbs2Channel.PilotTonesState);
            break;
        }
      }
      else if (channel is ChannelDvbS)
      {
        // The driver may interpret ModBpsk as DVB-S2, so we must override.
        // Assume the driver can auto-detect BPSK vs. QPSK.
        bdaModulation = ModulationType.ModQpsk;
      }
      else
      {
        ChannelDvbDsng dvbDsngChannel = channel as ChannelDvbDsng;
        if (dvbDsngChannel != null)
        {
          tveRollOffFactor = dvbDsngChannel.RollOffFactor;
        }
      }

      if (tveRollOffFactor != RollOffFactor.Automatic)
      {
        switch (tveRollOffFactor)
        {
          case RollOffFactor.Twenty:
            bdaRollOffFactor = RollOff.Twenty;
            break;
          case RollOffFactor.TwentyFive:
            bdaRollOffFactor = RollOff.TwentyFive;
            break;
          case RollOffFactor.ThirtyFive:
            bdaRollOffFactor = RollOff.ThirtyFive;
            break;
          default:
            this.LogWarn("Hauppauge: DVB-DSNG/DVB-S2 tune request uses unsupported roll-off factor {0}", tveRollOffFactor);
            break;
        }
      }

      if (bdaModulation != ModulationType.ModNotSet)
      {
        this.LogDebug("  modulation = {0}", bdaModulation);
        satelliteChannel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
      }

      // Should these functions be called if not tuning DVB-S2???
      SetRollOffFactor(bdaRollOffFactor);
      SetPilotTonesState(bdaPilotTonesState);
    }

    #endregion

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

    ~HauppaugeBda()
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
        Release.ComObject("Hauppauge BDA property set", ref _propertySet);
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
      if (isDisposing)
      {
        IDisposable d = _diseqcInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _diseqcInterface = null;
      }
      _isHauppaugeBda = false;
    }

    #endregion
  }
}