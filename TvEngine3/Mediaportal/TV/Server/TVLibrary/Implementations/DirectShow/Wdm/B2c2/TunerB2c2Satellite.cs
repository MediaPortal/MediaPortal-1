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
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using B2c2DiseqcPort = Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum.DiseqcPort;
using B2c2Polarisation = Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum.Polarisation;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for TechniSat satellite tuners with B2C2 chipsets and WDM drivers.
  /// </summary>
  internal class TunerB2c2Satellite : TunerB2c2Base, IDiseqcDevice
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
      : base(info, CardType.DvbS)
    {
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    public override void PerformLoading()
    {
      base.PerformLoading();
      _isRawDiseqcSupported = _capabilities.AcquisitionCapabilities.HasFlag(AcquisitionCapability.RawDiseqc);
    }

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBSChannel;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("B2C2 satellite: set tuning parameters");
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      lock (_tunerAccessLock)
      {
        HResult.ThrowException(_interfaceData.SelectDevice(_deviceInfo.DeviceId), "Failed to select device.");
        HResult.ThrowException(_interfaceTuner.SetFrequency((int)dvbsChannel.Frequency / 1000), "Failed to set frequency.");
        HResult.ThrowException(_interfaceTuner.SetSymbolRate(dvbsChannel.SymbolRate), "Failed to set symbol rate.");

        FecRate fec = FecRate.Auto;
        switch (dvbsChannel.InnerFecRate)
        {
          case BinaryConvolutionCodeRate.Rate1_2:
            fec = FecRate.Rate1_2;
            break;
          case BinaryConvolutionCodeRate.Rate2_3:
            fec = FecRate.Rate2_3;
            break;
          case BinaryConvolutionCodeRate.Rate3_4:
            fec = FecRate.Rate3_4;
            break;
          case BinaryConvolutionCodeRate.Rate5_6:
            fec = FecRate.Rate5_6;
            break;
          case BinaryConvolutionCodeRate.Rate7_8:
            fec = FecRate.Rate7_8;
            break;
        }
        HResult.ThrowException(_interfaceTuner.SetFec(fec), "Failed to set FEC rate.");

        B2c2Polarisation b2c2Polarisation = B2c2Polarisation.Horizontal;
        if (dvbsChannel.Polarisation == DirectShowLib.BDA.Polarisation.LinearV || dvbsChannel.Polarisation == DirectShowLib.BDA.Polarisation.CircularR)
        {
          b2c2Polarisation = B2c2Polarisation.Vertical;
        }
        HResult.ThrowException(_interfaceTuner.SetPolarity(b2c2Polarisation), "Failed to set polarisation.");

        int hr = (int)HResult.Severity.Success;
        if (dvbsChannel.Frequency > dvbsChannel.LnbType.SwitchFrequency)
        {
          hr = _interfaceTuner.SetLnbFrequency(dvbsChannel.LnbType.HighBandFrequency / 1000);
        }
        else
        {
          hr = _interfaceTuner.SetLnbFrequency(dvbsChannel.LnbType.LowBandFrequency / 1000);
        }
        HResult.ThrowException(hr, "Failed to set LNB LOF frequency.");

        HResult.ThrowException(_interfaceTuner.SetTunerStatus(), "Failed to apply tuning parameters.");
      }
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("B2C2 satellite: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);
      if (_interfaceTuner == null)
      {
        this.LogError("B2C2 satellite: not initialised or interface not supported");
        return false;
      }

      bool success = true;
      int hr = (int)HResult.Severity.Success;

      B2c2DiseqcPort burst = B2c2DiseqcPort.None;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        burst = B2c2DiseqcPort.SimpleA;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        burst = B2c2DiseqcPort.SimpleB;
      }
      if (burst != B2c2DiseqcPort.None)
      {
        hr = _interfaceTuner.SetDiseqc(burst);
        if (hr == (int)HResult.Severity.Success)
        {
          this.LogDebug("B2C2 satellite: burst result = success");
        }
        else
        {
          this.LogDebug("B2C2 satellite: failed to send tone burst command, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      Tone tone = Tone.Off;
      if (tone22kState == Tone22k.On)
      {
        tone = Tone.Tone22k;
      }
      hr = _interfaceTuner.SetLnbKHz(tone);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("B2C2 satellite: 22 kHz result = success");
      }
      else
      {
        this.LogDebug("B2C2 satellite: failed to set 22 kHz state, hr = 0x{0:x}", hr);
        success = false;
      }

      return success;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
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

      int hr = (int)HResult.Severity.Success;
      if (_isRawDiseqcSupported)
      {
        try
        {
          hr = _interfaceTuner.SendDiSEqCCommand(command.Length, command);
          if (hr == (int)HResult.Severity.Success)
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
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("B2C2 satellite: result = success");
        return true;
      }

      this.LogDebug("B2C2 satellite: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadDiseqcResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion
  }
}