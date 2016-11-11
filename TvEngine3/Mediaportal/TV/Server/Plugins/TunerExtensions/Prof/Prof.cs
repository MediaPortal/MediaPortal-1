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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Prof
{
  /// <summary>
  /// A class for handling DiSEqC for Prof tuners, including clones from Satrade and Prof. The
  /// interface was originally a customised Conexant interface created by Turbosight, however
  /// Turbosight have implemented a new unified interface for their products.
  /// </summary>
  public class Prof : BaseTunerExtension, IDiseqcDevice, IDisposable, IPowerDevice
  {
    #region enums

    private enum BdaExtensionProperty
    {
      DiseqcMessage = 0,    // For DiSEqC messaging.
      DiseqcInit,           // For intialising DiSEqC.
      ScanFrequency,        // (Not supported...)
      ChannelChange,        // For changing channel.
      DemodInfo,            // For returning demodulator firmware state and version.
      EffectiveFrequency,   // (Not supported...)
      SignalStatus,         // For retrieving signal quality, strength, BER and other attributes.
      LockStatus,           // For retrieving demodulator lock indicators.
      ErrorControl,         // For controlling error correction and BER window.
      ChannelInfo,          // For retrieving the locked values of frequency, symbol rate etc. after corrections and adjustments.
      NbcParams             // For setting DVB-S2 parameters that could not initially be set through BDA interfaces.
    }

    private enum BdaExtensionCommand : uint
    {
      LnbPower = 0,
      Motor,
      Tone,
      Diseqc
    }

    private enum ProfTone22kState : byte
    {
      Off = 0,
      On
    }

    private enum ProfToneBurst : byte
    {
      ToneBurst = 0,        // simple A
      DataBurst,            // simple B
      Off
    }

    private enum ProfToneModulation : uint
    {
      Undefined = 0,        // (Results in an error - *do not use*!)
      Modulated,
      Unmodulated
    }

    private enum ProfDiseqcReceiveMode : uint
    {
      Interrogation = 0,    // Expecting multiple devices attached.
      QuickReply,           // Expecting one response (receiving is suspended after first response).
      NoReply,              // Expecting no response(s).
    }

    private enum ProfRollOffFactor : uint
    {
      Undefined = 0xff,
      Twenty = 0,           // 0.2
      TwentyFive,           // 0.25
      ThirtyFive            // 0.35
    }

    private enum ProfPilotTonesState : uint
    {
      Off = 0,
      On,
      Unknown               // (Not used...)
    }

    private enum ProfDvbsStandard : uint
    {
      Auto = 0,
      Dvbs,
      Dvbs2
    }

    private enum ProfLnbPower : uint
    {
      Off = 0,
      On
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BdaExtensionParams
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_TX_MESSAGE_LENGTH)]
      public byte[] DiseqcTransmitMessage;
      public byte DiseqcTransmitMessageLength;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_RX_MESSAGE_LENGTH)]
      public byte[] DiseqcReceiveMessage;
      public byte DiseqcReceiveMessageLength;
      private ushort Padding;

      public ProfToneModulation ToneModulation;
      public ProfDiseqcReceiveMode ReceiveMode;

      public BdaExtensionCommand Command;
      public ProfTone22kState Tone22kState;
      public ProfToneBurst ToneBurst;
      public byte MicroControllerParityErrors;        // Parity errors: 0 indicates no errors, binary 1 indicates an error.
      public byte MicroControllerReplyErrors;         // 1 in bit i indicates error in byte i.

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLastMessage;                      // This may have some influence over whether a tone burst command will be sent and 22 kHz tone state set.
      public ProfLnbPower LnbPower;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NbcTuningParams
    {
      public ProfRollOffFactor RollOffFactor;
      public ProfPilotTonesState PilotTonesState;
      public ProfDvbsStandard DvbsStandard;
      public BinaryConvolutionCodeRate FecCodeRate;
      public ModulationType ModulationType;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);

    private static readonly int BDA_EXTENSION_PARAMS_SIZE = Marshal.SizeOf(typeof(BdaExtensionParams));   // 188
    private static readonly int NBC_TUNING_PARAMS_SIZE = Marshal.SizeOf(typeof(NbcTuningParams));         // 20

    private const byte MAX_DISEQC_TX_MESSAGE_LENGTH = 151;  // 3 bytes per message * 50 messages
    private const byte MAX_DISEQC_RX_MESSAGE_LENGTH = 9;    // reply fifo size, do not increase (hardware limitation)

    private static readonly int GENERAL_BUFFER_SIZE = Math.Max(BDA_EXTENSION_PARAMS_SIZE, NBC_TUNING_PARAMS_SIZE);

    #endregion

    #region variables

    private bool _isProf = false;
    private IKsPropertySet _propertySet = null;
    private IntPtr _generalBuffer = IntPtr.Zero;

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 60;
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
      this.LogDebug("Prof: initialising");

      if (_isProf)
      {
        this.LogWarn("Prof: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Prof: tuner type not supported");
        return false;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Prof: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Prof: pin is not a property set");
        Release.ComObject("Prof filter input pin", ref pin);
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcMessage, out support);
      // The original Conexant interface uses the set method; this interface uses the get method.
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Prof: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        Release.ComObject("Prof property set", ref _propertySet);
        return false;
      }

      this.LogInfo("Prof: extension supported");
      _isProf = true;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
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
      this.LogDebug("Prof: on before tune call back");
      action = TunerAction.Default;

      if (!_isProf)
      {
        this.LogWarn("Prof: not initialised or interface not supported");
        return;
      }

      // We only have work to do if the channel is a satellite channel.
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      NbcTuningParams command = new NbcTuningParams();
      ModulationType bdaModulation = ModulationType.ModNotSet;
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (dvbs2Channel != null)
      {
        command.DvbsStandard = ProfDvbsStandard.Dvbs2;
        switch (satelliteChannel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk4:
            if (Environment.OSVersion.Version.Major >= 6)
            {
              bdaModulation = ModulationType.ModNbcQpsk;
            }
            else
            {
              bdaModulation = ModulationType.ModOqpsk;
            }
            break;
          case ModulationSchemePsk.Psk8:
            if (Environment.OSVersion.Version.Major >= 6)
            {
              bdaModulation = ModulationType.ModNbc8Psk;
            }
            else
            {
              bdaModulation = ModulationType.ModBpsk;
            }
            break;
          // I'm not sure what values to use for 16 and 32 APSK on XP.
          // However, AFAIK the hardware only supports QPSK and 8 PSK anyway.
          case ModulationSchemePsk.Psk16:
            bdaModulation = ModulationType.Mod16Apsk;
            break;
          case ModulationSchemePsk.Psk32:
            bdaModulation = ModulationType.ModNbc8Psk;
            break;
          default:
            this.LogWarn("Prof: DVB-S2 tune request uses unsupported modulation scheme {0}", satelliteChannel.ModulationScheme);
            break;
        }
      }
      else if (channel is ChannelDvbS)
      {
        command.DvbsStandard = ProfDvbsStandard.Dvbs;
        switch (satelliteChannel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk2:
            // The driver maps ModBpsk to DVB-S2, so we have to override the
            // default mapping. Assume Mod16Qam is mapped to DVB-S.
            bdaModulation = ModulationType.Mod16Qam;
            break;
          case ModulationSchemePsk.Psk4:
            bdaModulation = ModulationType.ModQpsk;
            break;
          default:
            this.LogWarn("Prof: DVB-S tune request uses unsupported modulation scheme {0}", satelliteChannel.ModulationScheme);
            break;
        }
      }
      else
      {
        // Tuning with "auto" is slower, so avoid it if possible.
        this.LogWarn("Prof: tune request for unsupported satellite standard");
        command.DvbsStandard = ProfDvbsStandard.Auto;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        if (command.ModulationType != ModulationType.ModNotSet)
        {
          this.LogDebug("  modulation = {0}", command.ModulationType);
          satelliteChannel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
        }
        this.LogDebug("Prof: NBC tuning parameter property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return;
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
          this.LogWarn("Prof: tune request uses unsupported FEC code rate {0}", satelliteChannel.FecCodeRate);
          command.FecCodeRate = BinaryConvolutionCodeRate.RateNotSet;
          break;
      }

      // Maybe these NBC parameter values should be 0.35/off for non-DVB-S2.
      command.RollOffFactor = ProfRollOffFactor.Undefined;
      command.PilotTonesState = ProfPilotTonesState.Unknown;
      RollOffFactor rollOffFactor = RollOffFactor.Automatic;
      if (dvbs2Channel == null)
      {
        ChannelDvbDsng dvbDsngChannel = channel as ChannelDvbDsng;
        if (dvbDsngChannel != null)
        {
          rollOffFactor = dvbDsngChannel.RollOffFactor;
        }
      }
      else
      {
        if (command.FecCodeRate == BinaryConvolutionCodeRate.RateNotSet)
        {
          // Old demods can't auto-detect DVB-S2 FEC code rate.
          command.DvbsStandard = ProfDvbsStandard.Auto;
        }

        rollOffFactor = dvbs2Channel.RollOffFactor;
        switch (dvbs2Channel.PilotTonesState)
        {
          case PilotTonesState.Off:
            command.PilotTonesState = ProfPilotTonesState.Off;
            break;
          case PilotTonesState.On:
            command.PilotTonesState = ProfPilotTonesState.On;
            break;
          default:
            this.LogWarn("Prof: DVB-S2 tune request uses unsupported pilot tones state {0}", dvbs2Channel.PilotTonesState);
            break;
        }
      }

      if (rollOffFactor != RollOffFactor.Automatic)
      {
        switch (rollOffFactor)
        {
          case RollOffFactor.Twenty:
            command.RollOffFactor = ProfRollOffFactor.Twenty;
            break;
          case RollOffFactor.TwentyFive:
            command.RollOffFactor = ProfRollOffFactor.TwentyFive;
            break;
          case RollOffFactor.ThirtyFive:
            command.RollOffFactor = ProfRollOffFactor.ThirtyFive;
            break;
          default:
            this.LogWarn("Prof: DVB-DSNG/DVB-S2 tune request uses unsupported roll-off factor {0}", rollOffFactor);
            break;
        }
      }

      this.LogDebug("  standard        = {0}", command.DvbsStandard);
      this.LogDebug("  modulation      = {0}", command.ModulationType);
      this.LogDebug("  FEC code rate   = {0}", command.FecCodeRate);
      this.LogDebug("  roll-off factor = {0}", command.RollOffFactor);
      this.LogDebug("  pilot tones     = {0}", command.PilotTonesState);

      Marshal.StructureToPtr(command, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, NBC_TUNING_PARAMS_SIZE);

      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.NbcParams,
        _generalBuffer, NBC_TUNING_PARAMS_SIZE,
        _generalBuffer, NBC_TUNING_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof: result = success");
      }
      else
      {
        this.LogError("Prof: failed to set NBC tuning parameters, hr = 0x{0:x}", hr);
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
    public virtual bool SetPowerState(PowerState state)
    {
      this.LogDebug("Prof: set power state, state = {0}", state);

      if (!_isProf)
      {
        this.LogWarn("Prof: not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      command.Command = BdaExtensionCommand.LnbPower;
      command.LnbPower = ProfLnbPower.Off;
      if (state == PowerState.On)
      {
        command.LnbPower = ProfLnbPower.On;
      }

      Marshal.StructureToPtr(command, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int returnedByteCount = 0;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcMessage,
         _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
         _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
         out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof: result = success");
        return true;
      }

      this.LogError("Prof: failed to set power state, hr = 0x{0:x}", hr);
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
      this.LogDebug("Prof: send DiSEqC command");

      if (!_isProf)
      {
        this.LogWarn("Prof: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Prof: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_TX_MESSAGE_LENGTH)
      {
        this.LogError("Prof: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.Command = BdaExtensionCommand.Diseqc;
      propertyParams.DiseqcTransmitMessage = new byte[MAX_DISEQC_TX_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, propertyParams.DiseqcTransmitMessage, 0, command.Length);
      propertyParams.DiseqcTransmitMessageLength = (byte)command.Length;
      propertyParams.ReceiveMode = ProfDiseqcReceiveMode.NoReply;
      propertyParams.IsLastMessage = false;

      Marshal.StructureToPtr(propertyParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int returnedByteCount = 0;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcMessage,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof: result = success");
        return true;
      }

      this.LogError("Prof: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Prof: send tone burst command, command = {0}", command);

      if (!_isProf)
      {
        this.LogWarn("Prof: not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.Command = BdaExtensionCommand.Tone;
      propertyParams.IsLastMessage = true;
      propertyParams.ToneModulation = ProfToneModulation.Unmodulated;   // Can't use undefined, so use simple A instead.
      propertyParams.Tone22kState = ProfTone22kState.Off;
      if (command == ToneBurst.ToneBurst)
      {
        propertyParams.ToneBurst = ProfToneBurst.ToneBurst;
      }
      else if (command == ToneBurst.DataBurst)
      {
        propertyParams.ToneBurst = ProfToneBurst.DataBurst;
        propertyParams.ToneModulation = ProfToneModulation.Modulated;
      }

      Marshal.StructureToPtr(propertyParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int returnedByteCount = 0;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcMessage,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof: result = success");
        return true;
      }

      this.LogError("Prof: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("Prof: set tone state, state = {0}", state);

      if (!_isProf)
      {
        this.LogWarn("Prof: not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.Command = BdaExtensionCommand.Tone;
      propertyParams.IsLastMessage = true;
      propertyParams.ToneBurst = ProfToneBurst.Off;
      propertyParams.ToneModulation = ProfToneModulation.Unmodulated;   // Can't use undefined, so use simple A instead.
      propertyParams.Tone22kState = ProfTone22kState.Off;
      if (state == Tone22kState.On)
      {
        propertyParams.Tone22kState = ProfTone22kState.On;
      }

      Marshal.StructureToPtr(propertyParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int returnedByteCount = 0;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcMessage,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof: result = success");
        return true;
      }

      this.LogError("Prof: failed to set tone state, hr = 0x{0:x}", hr);
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
      this.LogDebug("Prof: read DiSEqC response");
      response = null;

      if (!_isProf)
      {
        this.LogWarn("Prof: not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.Command = BdaExtensionCommand.Diseqc;
      propertyParams.DiseqcTransmitMessageLength = 0;
      propertyParams.ReceiveMode = ProfDiseqcReceiveMode.QuickReply;
      propertyParams.IsLastMessage = false;
      Marshal.StructureToPtr(propertyParams, _generalBuffer, false);
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcMessage,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != BDA_EXTENSION_PARAMS_SIZE)
      {
        this.LogError("Prof: failed to read DiSEqC response, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return false;
      }

      Dump.DumpBinary(_generalBuffer, returnedByteCount);

      propertyParams = (BdaExtensionParams)Marshal.PtrToStructure(_generalBuffer, typeof(BdaExtensionParams));
      if (propertyParams.DiseqcReceiveMessageLength > MAX_DISEQC_RX_MESSAGE_LENGTH)
      {
        this.LogError("Prof: unexpected number of DiSEqC response message bytes ({0}) returned", propertyParams.DiseqcReceiveMessageLength);
        return false;
      }
      response = new byte[propertyParams.DiseqcReceiveMessageLength];
      Buffer.BlockCopy(propertyParams.DiseqcReceiveMessage, 0, response, 0, (int)propertyParams.DiseqcReceiveMessageLength);
      this.LogDebug("Prof: result = success");
      return true;
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

    ~Prof()
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
      if (isDisposing)
      {
        Release.ComObject("Prof property set", ref _propertySet);
      }
      _isProf = false;
    }

    #endregion
  }
}