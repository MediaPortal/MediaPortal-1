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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using BdaPolarisation = DirectShowLib.BDA.Polarisation;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;
using TvePolarisation = Mediaportal.TV.Server.Common.Types.Enum.Polarisation;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Genpix
{
  /// <summary>
  /// A class for handling DiSEqC for Genpix tuners using the standard BDA driver.
  /// </summary>
  public class Genpix : BaseTunerExtension, ICustomTuner, IDiseqcDevice, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty : int
    {
      Tune = 0,               // For custom tuning implementation.
      Diseqc,                 // For DiSEqC messaging.
      SignalStatus,           // For retrieving signal quality, strength, lock status and the actual lock frequency.
    }

    private enum GenpixToneBurst : byte
    {
      ToneBurst = 0,
      DataBurst
    }

    private enum GenpixSwitchPort : uint
    {
      None = 0,

      // DiSEqC 1.0
      PortA,
      PortB,
      PortC,
      PortD,

      // Tone burst (simple DiSEqC)
      ToneBurst,
      DataBurst,

      //------------------------------
      // Legacy Dish Network switches
      //------------------------------
      // SW21 - a 2-in-1 out switch.
      Sw21PortA,
      Sw21PortB,

      // SW42 - a 2 x 2-in-1 out (ie. 2 satellites, 2 independent
      // receivers) switch with slightly different switching
      // commands to the SW21.
      Sw42PortA,
      Sw42PortB,

      // SW44???
      SW44PortB,

      // SW64 - a 6-in-4 out switch, usually used for connecting
      // 3 satellites (both polarities) to 4 independent receivers.
      Sw64PortA_Odd,
      Sw64PortA_Even,
      Sw64PortB_Odd,
      Sw64PortB_Even,
      Sw64PortC_Odd,
      Sw64PortC_Even,

      // Twin LNB - a dual head LNB with multiple independent outputs.
      TwinLnbSatA,
      TwinLnbSatB,

      // Quad LNB???
      QuadLnbSatB
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BdaExtensionParams
    {
      public int Frequency;                 // unit = MHz
      public int LnbLowBandLof;             // unit = MHz
      public int LnbHighBandLof;            // unit = MHz
      public int LnbSwitchFrequency;        // unit = MHz
      public int SymbolRate;                // unit = ks/s
      public BdaPolarisation Polarisation;
      public ModulationType Modulation;
      public BinaryConvolutionCodeRate FecCodeRate;
      public GenpixSwitchPort SwitchPort;

      public uint DiseqcRepeats;            // Set to zero to send once, one to send twice, two to send three times etc.

      public uint DiseqcMessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] DiseqcMessage;
      [MarshalAs(UnmanagedType.Bool)]
      public bool DiseqcForceHighVoltage;

      public uint SignalStrength;           // range = 0 - 100%
      public uint SignalQuality;            // range = 0 - 100%
      [MarshalAs(UnmanagedType.Bool)]
      public bool SignalIsLocked;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xdf981009, 0x0d8a, 0x430e, 0xa8, 0x03, 0x17, 0xc5, 0x14, 0xdc, 0x8e, 0xc0);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.

    private static readonly int BDA_EXTENSION_PARAMS_SIZE = Marshal.SizeOf(typeof(BdaExtensionParams));   // 68
    private const int MAX_DISEQC_MESSAGE_LENGTH = 8;

    private static readonly int GENERAL_BUFFER_SIZE = BDA_EXTENSION_PARAMS_SIZE;

    #endregion

    #region variables

    private bool _isGenpix = false;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

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
      this.LogDebug("Genpix: initialising");

      if (_isGenpix)
      {
        this.LogWarn("Genpix: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Genpix: tuner type not supported");
        return false;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Genpix: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Genpix: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      this.LogInfo("Genpix: extension supported");
      _isGenpix = true;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
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
      this.LogDebug("Genpix: on before tune call back");
      action = TunerAction.Default;

      if (!_isGenpix)
      {
        this.LogWarn("Genpix: not initialised or interface not supported");
        return;
      }

      // Genpix tuners support modulation types that many other tuners do not.
      // The driver interprets BDA ModulationType as follows:
      // QPSK     => DVB-S QPSK
      // 16 QAM   => turbo FEC QPSK
      // 8 PSK    => turbo FEC 8 PSK
      // DirecTV  => DSS QPSK [not supported, DSS does not use MPEG 2 TS]
      // 32 QAM   => DC II combo
      // 64 QAM   => DC II split (I)
      // 80 QAM   => DC II split (Q)
      // 96 QAM   => DC II offset QPSK
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      ModulationType bdaModulation = ModulationType.ModNotSet;
      if (satelliteChannel is ChannelDvbS)
      {
        if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
        {
          bdaModulation = ModulationType.ModQpsk;
        }
      }
      else if (satelliteChannel is ChannelSatelliteTurboFec)
      {
        if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
        {
          bdaModulation = ModulationType.Mod16Qam;
        }
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk8)
        {
          bdaModulation = ModulationType.Mod8Psk;
        }
      }
      else if (satelliteChannel is ChannelDigiCipher2)
      {
        if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
        {
          bdaModulation = ModulationType.Mod32Qam;
        }
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4SplitI)
        {
          bdaModulation = ModulationType.Mod64Qam;
        }
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4SplitQ)
        {
          bdaModulation = ModulationType.Mod80Qam;
        }
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4Offset)
        {
          bdaModulation = ModulationType.Mod96Qam;
        }
        // DC II 8 PSK is turbo FEC 8 PSK
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk8)
        {
          bdaModulation = ModulationType.Mod8Psk;
        }
      }

      if (bdaModulation == ModulationType.ModNotSet)
      {
        this.LogWarn("Genpix: tune request uses unsupported modulation scheme {0}, falling back to automatic", satelliteChannel.ModulationScheme);
      }
      else
      {
        this.LogDebug("  modulation = {0}", bdaModulation);
      }
      satelliteChannel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
    }

    #endregion

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the extension implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the extension supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      // This extension only supports satellite tuners. As such, tuning is only supported for satellite channels.
      return channel is IChannelSatellite;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("Genpix: tune to channel");

      if (!_isGenpix)
      {
        this.LogWarn("Genpix: not initialised or interface not supported");
        return false;
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        this.LogError("Genpix: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      command.Frequency = satelliteChannel.Frequency / 1000;
      command.LnbLowBandLof = SatelliteLnbHandler.LOW_BAND_LOF / 1000;
      command.LnbHighBandLof = SatelliteLnbHandler.HIGH_BAND_LOF / 1000;
      command.LnbSwitchFrequency = SatelliteLnbHandler.SWITCH_FREQUENCY / 1000;
      command.SymbolRate = satelliteChannel.SymbolRate;
      command.Modulation = (ModulationType)satelliteChannel.ModulationScheme;
      command.SwitchPort = GenpixSwitchPort.None;
      command.DiseqcRepeats = 0;

      switch (satelliteChannel.Polarisation)
      {
        case TvePolarisation.CircularLeft:
          command.Polarisation = BdaPolarisation.CircularL;
          break;
        case TvePolarisation.CircularRight:
          command.Polarisation = BdaPolarisation.CircularR;
          break;
        case TvePolarisation.LinearHorizontal:
          command.Polarisation = BdaPolarisation.LinearH;
          break;
        case TvePolarisation.LinearVertical:
          command.Polarisation = BdaPolarisation.LinearV;
          break;
        default:
          this.LogWarn("Genpix: tune request uses unsupported polarisation {0}, falling back to automatic", satelliteChannel.Polarisation);
          command.Polarisation = BdaPolarisation.NotSet;
          break;
      }

      switch (satelliteChannel.FecCodeRate)
      {
        case FecCodeRate.Rate1_2:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate1_2;
          break;
        case FecCodeRate.Rate1_3:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate1_3;
          break;
        case FecCodeRate.Rate1_4:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate1_4;
          break;
        case FecCodeRate.Rate2_3:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate2_3;
          break;
        case FecCodeRate.Rate2_5:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate2_5;
          break;
        case FecCodeRate.Rate3_4:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate3_4;
          break;
        case FecCodeRate.Rate3_5:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate3_5;
          break;
        case FecCodeRate.Rate4_5:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate4_5;
          break;
        case FecCodeRate.Rate5_11:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate5_11;
          break;
        case FecCodeRate.Rate5_6:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate5_6;
          break;
        case FecCodeRate.Rate6_7:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate6_7;
          break;
        case FecCodeRate.Rate7_8:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate7_8;
          break;
        case FecCodeRate.Rate8_9:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate8_9;
          break;
        case FecCodeRate.Rate9_10:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate9_10;
          break;
        default:
          this.LogWarn("Genpix: tune request uses unsupported FEC code rate {0}, falling back to automatic", satelliteChannel.FecCodeRate);
          command.FecCodeRate = BinaryConvolutionCodeRate.RateNotSet;
          break;
      }

      Marshal.StructureToPtr(command, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tune,
        _instanceBuffer, INSTANCE_SIZE,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Genpix: result = success");
        return true;
      }

      this.LogError("Genpix: failed to tune, hr = 0x{0:x}{1}{2}", hr, Environment.NewLine, channel);
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
      this.LogDebug("Genpix: send DiSEqC command");

      if (!_isGenpix)
      {
        this.LogWarn("Genpix: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Genpix: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Genpix: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams message = new BdaExtensionParams();
      message.DiseqcMessageLength = (uint)command.Length;
      message.DiseqcRepeats = 0;
      message.DiseqcForceHighVoltage = true;
      message.DiseqcMessage = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.DiseqcMessage, 0, command.Length);

      Marshal.StructureToPtr(message, _generalBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, INSTANCE_SIZE,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Genpix: result = success");
        return true;
      }

      this.LogError("Genpix: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Genpix: send tone burst command, command = {0}", command);

      if (!_isGenpix)
      {
        this.LogWarn("Genpix: not initialised or interface not supported");
        return false;
      }

      // The driver interprets sending a DiSEqC message with length zero as
      // a tone burst command.
      BdaExtensionParams message = new BdaExtensionParams();
      message.DiseqcMessageLength = 0;
      message.DiseqcRepeats = 0;
      message.DiseqcForceHighVoltage = false;
      message.DiseqcMessage = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      if (command == ToneBurst.ToneBurst)
      {
        message.DiseqcMessage[0] = (byte)GenpixToneBurst.ToneBurst;
      }
      else if (command == ToneBurst.DataBurst)
      {
        message.DiseqcMessage[0] = (byte)GenpixToneBurst.DataBurst;
      }

      Marshal.StructureToPtr(message, _generalBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, INSTANCE_SIZE,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Genpix: result = success");
        return true;
      }

      this.LogError("Genpix: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      // Set by tune request LNB frequency parameters.
      return true;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      // Not supported.
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

    ~Genpix()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (isDisposing)
      {
        _propertySet = null;
      }
      _isGenpix = false;
    }

    #endregion
  }
}