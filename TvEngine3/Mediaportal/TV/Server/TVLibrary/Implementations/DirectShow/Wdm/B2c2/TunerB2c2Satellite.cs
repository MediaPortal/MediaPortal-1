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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using B2c2DiseqcPort = Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum.DiseqcPort;
using B2c2Polarisation = Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum.Polarisation;
using MpPolarisation = Mediaportal.TV.Server.Common.Types.Enum.Polarisation;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for TechniSat satellite tuners
  /// with B2C2 chipsets and WDM drivers.
  /// </summary>
  internal class TunerB2c2Satellite : TunerB2c2Base, IDiseqcDevice, IPowerDevice
  {
    #region constants

    private static readonly int MAX_DISEQC_MESSAGE_LENGTH = 10;

    #endregion

    #region variables

    /// <summary>
    /// <c>True</c> if the tuner is capable of sending raw DiSEqC commands, otherwise <c>false</c>.
    /// </summary>
    private bool _isRawDiseqcSupported = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Satellite"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    public TunerB2c2Satellite(DeviceInfo info)
      : base(info, BroadcastStandard.DvbS)
    {
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Dvb | StreamFormat.Freesat;
      }
      IList<ITunerExtension> extensions = base.PerformLoading(streamFormat);
      _isRawDiseqcSupported = _capabilities.AcquisitionCapabilities.HasFlag(AcquisitionCapability.RawDiseqc);
      return extensions;
    }

    #region tuning

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    public override BroadcastStandard PossibleBroadcastStandards
    {
      get
      {
        return BroadcastStandard.DvbDsng | BroadcastStandard.DvbS | BroadcastStandard.DvbS2;
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("B2C2 satellite: set tuning parameters");
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      lock (_tunerAccessLock)
      {
        TvExceptionDirectShowError.Throw(_interfaceData.SelectDevice(_deviceInfo.DeviceId), "Failed to select device.");
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetFrequency(satelliteChannel.Frequency / 1000), "Failed to set frequency.");
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetSymbolRate(satelliteChannel.SymbolRate), "Failed to set symbol rate.");

        FecRate fecCodeRate;
        switch (satelliteChannel.FecCodeRate)
        {
          case FecCodeRate.Rate1_2:
            fecCodeRate = FecRate.Rate1_2;
            break;
          case FecCodeRate.Rate2_3:
            fecCodeRate = FecRate.Rate2_3;
            break;
          case FecCodeRate.Rate3_4:
            fecCodeRate = FecRate.Rate3_4;
            break;
          case FecCodeRate.Rate5_6:
            fecCodeRate = FecRate.Rate5_6;
            break;
          case FecCodeRate.Rate7_8:
            fecCodeRate = FecRate.Rate7_8;
            break;
          default:
            this.LogWarn("B2C2 satellite: falling back to automatic FEC code rate");
            fecCodeRate = FecRate.Auto;
            break;
        }
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetFec(fecCodeRate), "Failed to set FEC rate.");

        int lnbLof;
        Tone22kState bandSelectionTone;
        MpPolarisation bandSelectionPolarisation;
        satelliteChannel.LnbType.GetTuningParameters(satelliteChannel.Frequency, satelliteChannel.Polarisation, Tone22kState.Automatic, out lnbLof, out bandSelectionTone, out bandSelectionPolarisation);

        B2c2Polarisation polarisation;
        switch (bandSelectionPolarisation)
        {
          case MpPolarisation.LinearHorizontal:
          case MpPolarisation.CircularLeft:
            polarisation = B2c2Polarisation.Horizontal;
            break;
          case MpPolarisation.LinearVertical:
          case MpPolarisation.CircularRight:
            polarisation = B2c2Polarisation.Vertical;
            break;
          case MpPolarisation.Automatic:
            this.LogWarn("B2C2 satellite: falling back to linear vertical polarisation");
            polarisation = B2c2Polarisation.Vertical;
            break;
          default:
            polarisation = (B2c2Polarisation)bandSelectionPolarisation;
            break;
        }
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetPolarity(polarisation), "Failed to set polarisation.");

        TvExceptionDirectShowError.Throw(_interfaceTuner.SetLnbFrequency(lnbLof / 1000), "Failed to set LNB LOF frequency.");

        base.PerformTuning(channel);
      }
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
      this.LogDebug("B2C2 satellite: set power state, state = {0}", state);
      if (_interfaceTuner == null)
      {
        this.LogWarn("B2C2 satellite: not initialised or interface not supported");
        return false;
      }

      if (state == PowerState.On)
      {
        // Power will be turned on automatically during tuning.
        this.LogDebug("B2C2 satellite: result = success");
        return true;
      }

      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("B2C2 satellite: failed to select device to set power state, hr = 0x{0:x}", hr);
        return false;
      }

      hr = _interfaceTuner.SetPolarity(B2c2Polarisation.PowerOff);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("B2C2 satellite: failed to set power state, hr = 0x{0:x}", hr);
        return false;
      }

      hr = _interfaceTuner.SetTunerStatusEx(1);
      if (hr != (int)NativeMethods.HResult.S_OK && hr != (int)Error.NotLockedOnSignal)
      {
        this.LogError("B2C2 satellite: failed to apply power state, hr = 0x{0:x}", hr);
        return false;
      }

      this.LogDebug("B2C2 satellite: result = success");
      return true;
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
      this.LogDebug("B2C2 satellite: send DiSEqC command");

      if (_interfaceTuner == null)
      {
        this.LogError("B2C2 satellite: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("B2C2 satellite: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogDebug("B2C2 satellite: DiSEqC command too long, length = {0}", command.Length);
        return true;
      }

      int hr = (int)NativeMethods.HResult.S_OK;
      if (_isRawDiseqcSupported)
      {
        try
        {
          hr = _interfaceTuner.SendDiSEqCCommand(command.Length, command);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("B2C2 satellite: result = success");
            return true;
          }
        }
        catch (COMException ex)
        {
          if ((Error)ex.ErrorCode == Error.Diseqc12NotSupported)
          {
            // DiSEqC 1.2 commands not supported. This is a little unexpected given that the
            // driver previously reported that it supports them.
            this.LogWarn("B2C2 satellite: raw DiSEqC commands not supported");
          }
          else
          {
            this.LogError(ex, "B2C2 satellite: failed to send raw DiSEqC command, hr = 0x{0:x}", ex.ErrorCode);
          }
        }
      }

      // If we get to here then the driver/hardware doesn't support raw commands. We'll attempt to send
      // non-raw commands if the command is a DiSEqC 1.0 switch command.
      if (command.Length != 4 ||
        (command[0] != (byte)DiseqcFrame.CommandFirstTransmissionNoReply &&
        command[0] != (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) ||
        command[1] != (byte)DiseqcAddress.AnySwitch ||
        command[2] != (byte)DiseqcCommand.WriteN0)
      {
        this.LogError("B2C2 satellite: DiSEqC command not supported");
        Dump.DumpBinary(command);
        return false;
      }

      // Port A = 3, Port B = 4 etc.
      B2c2DiseqcPort port = (B2c2DiseqcPort)((command[3] & 0xc) >> 2) + 3;
      hr = _interfaceTuner.SetDiseqc(port);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("B2C2 satellite: result = success");
        return true;
      }

      this.LogDebug("B2C2 satellite: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("B2C2 satellite: send tone burst command, command = {0}", command);

      if (_interfaceTuner == null)
      {
        this.LogWarn("B2C2 satellite: not initialised or interface not supported");
        return false;
      }

      B2c2DiseqcPort burstCommand = B2c2DiseqcPort.None;
      if (command == ToneBurst.ToneBurst)
      {
        burstCommand = B2c2DiseqcPort.SimpleA;
      }
      else if (command == ToneBurst.DataBurst)
      {
        burstCommand = B2c2DiseqcPort.SimpleB;
      }
      int hr = _interfaceTuner.SetDiseqc(burstCommand);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("B2C2 satellite: result = success");
        return true;
      }

      this.LogError("B2C2 satellite: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("B2C2 satellite: set tone state, state = {0}", state);

      if (_interfaceTuner == null)
      {
        this.LogWarn("B2C2 satellite: not initialised or interface not supported");
        return false;
      }

      Tone tone = Tone.Off;
      if (state == Tone22kState.On)
      {
        tone = Tone.Tone22k;
      }
      else if (state == Tone22kState.Off)
      {
        tone = Tone.Off;
      }
      int hr = _interfaceTuner.SetLnbKHz(tone);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("B2C2 satellite: result = success");
        return true;
      }

      this.LogError("B2C2 satellite: failed to set tone state, hr = 0x{0:x}", hr);
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
      // Not supported.
      response = null;
      return false;
    }

    #endregion
  }
}