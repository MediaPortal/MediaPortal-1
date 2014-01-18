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
using System.Text;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles SAT>IP
  /// satellite tuners.
  /// </summary>
  public class TunerSatIpSatellite : TunerSatIpBase, IDiseqcDevice
  {
    #region variables

    /// <summary>
    /// The current SAT>IP src parameter value.
    /// </summary>
    private int _currentSource = 1;

    /// <summary>
    /// The DiSEqC control interface for this tuner.
    /// </summary>
    private IDiseqcController _diseqcController = null;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpSatellite"/> class.
    /// </summary>
    /// <param name="name">The SAT>IP server's name.</param>
    /// <param name="uuid">A unique identifier for the SAT>IP server.</param>
    /// <param name="ipAddress">The SAT>IP server's current IP address.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerSatIpSatellite(string name, string uuid, string ipAddress, int sequenceNumber)
      : base(name + " S/S2 Tuner " + sequenceNumber, uuid + "S" + sequenceNumber, ipAddress, sequenceNumber)
    {
      _tunerType = CardType.DvbS;
      _uuid = uuid;
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      this.LogDebug("SAT>IP satellite: construct URL");
      DVBSChannel satelliteChannel = channel as DVBSChannel;
      if (satelliteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      StringBuilder url = new StringBuilder();
      // TODO remove pids=all when PID filter support can be added
      url.Append("rtsp://").Append(_ipAddress).Append(":554/pids=all&src=").Append(_currentSource);
      url.Append("&freq=").Append((int)(satelliteChannel.Frequency / 1000));
      url.Append("&sr=").Append(satelliteChannel.SymbolRate);

      if (satelliteChannel.Polarisation == Polarisation.LinearH)
      {
        url.Append("&pol=h");
      }
      else if (satelliteChannel.Polarisation == Polarisation.LinearV)
      {
        url.Append("&pol=v");
      }
      else if (satelliteChannel.Polarisation == Polarisation.CircularL)
      {
        url.Append("&pol=l");
      }
      else if (satelliteChannel.Polarisation == Polarisation.CircularR)
      {
        url.Append("&pol=r");
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported polarisation {0}, assuming linear vertical", satelliteChannel.Polarisation);
        url.Append("&pol=v");
      }

      if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate1_2)
      {
        url.Append("&fec=12");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate2_3)
      {
        url.Append("&fec=23");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate3_4)
      {
        url.Append("&fec=34");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate5_6)
      {
        url.Append("&fec=56");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate7_8)
      {
        url.Append("&fec=78");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate8_9)
      {
        url.Append("&fec=89");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate3_5)
      {
        url.Append("&fec=35");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate4_5)
      {
        url.Append("&fec=45");
      }
      else if (satelliteChannel.InnerFecRate == BinaryConvolutionCodeRate.Rate9_10)
      {
        url.Append("&fec=910");
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported inner FEC rate {0}, assuming 3/4", satelliteChannel.InnerFecRate);
        url.Append("&fec=34");
      }

      // DVB-S2 or DVB-S?
      if (satelliteChannel.ModulationType == ModulationType.ModNotSet)
      {
        url.Append("&system=dvbs");
        PerformTuning(url.ToString());
        return;
      }

      url.Append("&system=dvbs2");
      if (satelliteChannel.RollOff == RollOff.Twenty)
      {
        url.Append("&ro=0.20");
      }
      else if (satelliteChannel.RollOff == RollOff.TwentyFive)
      {
        url.Append("&ro=0.25");
      }
      else if (satelliteChannel.RollOff == RollOff.ThirtyFive)
      {
        url.Append("&ro=0.35");
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported roll-off {0}, assuming 0.35", satelliteChannel.RollOff);
        url.Append("&ro=0.35");
      }

      if (satelliteChannel.ModulationType == ModulationType.ModQpsk)
      {
        url.Append("&mtype=qpsk");
      }
      else if (satelliteChannel.ModulationType == ModulationType.Mod8Psk)
      {
        url.Append("&mtype=8psk");
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported modulation type {0}, assuming 8 PSK", satelliteChannel.ModulationType);
        url.Append("&mtype=8psk");
      }

      if (satelliteChannel.Pilot == Pilot.Off)
      {
        url.Append("&plts=off");
      }
      else if (satelliteChannel.Pilot == Pilot.On)
      {
        url.Append("&plts=on");
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported pilots setting {0}, assuming pilots on", satelliteChannel.Pilot);
        url.Append("&plts=on");
      }
      PerformTuning(url.ToString());
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

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      base.PerformLoading();
      _diseqcController = new DiseqcController(this);
      _diseqcController.ReloadConfiguration(_cardId);
    }

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();
      if (_diseqcController != null)
      {
        _diseqcController.ReloadConfiguration(_cardId);
      }
    }

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      DVBSChannel satelliteChannel = _currentTuningDetail as DVBSChannel;
      if (satelliteChannel == null)
      {
        _isSignalPresent = false;
        _isSignalLocked = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      string matchString = ((int)(satelliteChannel.Frequency / 1000)).ToString() + ",";
      if (satelliteChannel.Polarisation == Polarisation.LinearH)
      {
        matchString += "h";
      }
      else if (satelliteChannel.Polarisation == Polarisation.LinearV)
      {
        matchString += "v";
      }
      else if (satelliteChannel.Polarisation == Polarisation.CircularL)
      {
        matchString += "l";
      }
      else if (satelliteChannel.Polarisation == Polarisation.CircularR)
      {
        matchString += "r";
      }
      else
      {
        this.LogWarn("SAT>IP satellite: unsupported polarisation {0}, assuming linear vertical", satelliteChannel.Polarisation);
        matchString += "v";
      }
      if (satelliteChannel.ModulationType == ModulationType.ModNotSet)
      {
        matchString += ",dvbs";
      }
      else
      {
        matchString += ",dvbs2";
      }
      PerformSignalStatusUpdate(matchString);
    }

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBSChannel();
    }

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
    public bool SendCommand(byte[] command)
    {
      this.LogDebug("SAT>IP satellite: send DiSEqC command");

      if (command == null || command.Length == 0)
      {
        this.LogDebug("SAT>IP satellite: command not supplied");
        return true;
      }

      // If we get to here then the driver/hardware doesn't support raw commands. We'll attempt to send
      // non-raw commands if the command is a DiSEqC 1.0 switch command.
      if (command.Length != 4 ||
        (command[0] != (byte)DiseqcFrame.CommandFirstTransmissionNoReply &&
        command[0] != (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) ||
        command[1] != (byte)DiseqcAddress.AnySwitch ||
        command[2] != (byte)DiseqcCommand.WriteN0)
      {
        this.LogDebug("SAT>IP satellite: command not supported");
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
    public bool ReadResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion
  }
}
