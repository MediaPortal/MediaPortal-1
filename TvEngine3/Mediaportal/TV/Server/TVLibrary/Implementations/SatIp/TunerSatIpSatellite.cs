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

using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP.Description;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles SAT>IP
  /// satellite tuners.
  /// </summary>
  internal class TunerSatIpSatellite : TunerSatIpBase, IDiseqcDevice
  {
    #region variables

    /// <summary>
    /// The current SAT>IP src parameter value.
    /// </summary>
    private int _currentSource = 1;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpSatellite"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerSatIpSatellite(DeviceDescriptor serverDescriptor, int sequenceNumber, ITunerInternal streamTuner)
      : base(serverDescriptor, sequenceNumber, streamTuner, CardType.DvbS)
    {
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("SAT>IP satellite: construct URL");
      DVBSChannel satelliteChannel = channel as DVBSChannel;
      if (satelliteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      string frequency = ((int)(satelliteChannel.Frequency / 1000)).ToString();

      string polarisation = "v";
      if (satelliteChannel.Polarisation == Polarisation.LinearH)
      {
        polarisation = "h";
      }
      else if (satelliteChannel.Polarisation == Polarisation.LinearV)
      {
        polarisation = "v";
      }
      else if (satelliteChannel.Polarisation == Polarisation.CircularL)
      {
        polarisation = "l";
      }
      else if (satelliteChannel.Polarisation == Polarisation.CircularR)
      {
        polarisation = "r";
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported polarisation {0}, assuming linear vertical", satelliteChannel.Polarisation);
      }

      string fecRate = "34";
      if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate1_2)
      {
        fecRate = "12";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate2_3)
      {
        fecRate = "23";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate3_4)
      {
        fecRate = "34";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate5_6)
      {
        fecRate = "56";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate7_8)
      {
        fecRate = "78";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate8_9)
      {
        fecRate = "89";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate3_5)
      {
        fecRate = "35";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate4_5)
      {
        fecRate = "45";
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate9_10)
      {
        fecRate = "910";
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported inner FEC rate {0}, assuming 3/4", satelliteChannel.InnerFecRate);
      }

      string parameters = string.Format("src={0}&freq={1}&pol={2}&sr={3}&fec={4}&msys=dvbs", _currentSource, frequency, polarisation, satelliteChannel.SymbolRate, fecRate);

      // DVB-S2 or DVB-S?
      if (satelliteChannel.ModulationType == ModulationType.ModNotSet)
      {
        PerformTuning(parameters);
        return;
      }

      string modulation = "8psk";
      if (satelliteChannel.ModulationType == ModulationType.ModQpsk)
      {
        modulation = "qpsk";
      }
      else if (satelliteChannel.ModulationType == ModulationType.Mod8Psk)
      {
        modulation = "8psk";
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported modulation type {0}, assuming 8 PSK", satelliteChannel.ModulationType);
      }

      string pilots = "on";
      if (satelliteChannel.Pilot == Pilot.Off)
      {
        pilots = "off";
      }
      else if (satelliteChannel.Pilot == Pilot.On)
      {
        pilots = "on";
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported pilots setting {0}, assuming pilots on", satelliteChannel.Pilot);
      }

      string rollOff = "0.35";
      if (satelliteChannel.RollOff == RollOff.Twenty)
      {
        rollOff = "0.20";
      }
      else if (satelliteChannel.RollOff == RollOff.TwentyFive)
      {
        rollOff = "0.25";
      }
      else if (satelliteChannel.RollOff == RollOff.ThirtyFive)
      {
        rollOff = "0.35";
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported roll-off {0}, assuming 0.35", satelliteChannel.RollOff);
      }

      PerformTuning(parameters + string.Format("2&mtype={0}&plts={1}&ro={2}", modulation, pilots, rollOff));
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBSChannel;
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
      // Not supported.
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
    {
      this.LogDebug("SAT>IP satellite: send DiSEqC command");

      if (command == null || command.Length == 0)
      {
        this.LogWarn("SAT>IP satellite: DiSEqC command not supplied");
        return true;
      }

      // The SAT>IP interface currently doesn't support raw commands. We'll
      // attempt to send non-raw commands if the command is a DiSEqC 1.0 switch
      // command.
      if (command.Length != 4 ||
        (command[0] != (byte)DiseqcFrame.CommandFirstTransmissionNoReply &&
        command[0] != (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) ||
        command[1] != (byte)DiseqcAddress.AnySwitch ||
        command[2] != (byte)DiseqcCommand.WriteN0)
      {
        this.LogError("SAT>IP satellite: DiSEqC command not supported");
        Dump.DumpBinary(command);
        return false;
      }

      // Port A = 1, Port B = 2 etc.
      _currentSource = ((command[3] & 0xc) >> 2) + 1;
      this.LogDebug("SAT>IP satellite: result = success");
      return true;
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
