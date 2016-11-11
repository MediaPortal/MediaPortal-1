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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP.Description;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for SAT>IP satellite tuners.
  /// </summary>
  internal class TunerSatIpSatellite : TunerSatIpBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpSatellite"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerSatIpSatellite(DeviceDescriptor serverDescriptor, int sequenceNumber, ITunerInternal streamTuner)
      : base(serverDescriptor, sequenceNumber, "S", BroadcastStandard.DvbS | BroadcastStandard.DvbS2, streamTuner)
    {
    }

    #endregion

    #region ITunerInternal members

    #region state control

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
      return base.PerformLoading(streamFormat);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("SAT>IP satellite: perform tuning");
      ChannelDvbS dvbsChannel = channel as ChannelDvbS;
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (dvbsChannel == null && dvbs2Channel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      string frequency = (satelliteChannel.Frequency / 1000).ToString();

      string polarisation;
      switch (satelliteChannel.Polarisation)
      {
        case Polarisation.CircularLeft:
          polarisation = "l";
          break;
        case Polarisation.CircularRight:
          polarisation = "r";
          break;
        case Polarisation.LinearHorizontal:
          polarisation = "h";
          break;
        case Polarisation.LinearVertical:
          polarisation = "v";
          break;
        default:
          this.LogWarn("SAT>IP satellite: unsupported polarisation {0}, falling back to linear vertical", satelliteChannel.Polarisation);
          polarisation = "v";
          break;
      }

      string fecCodeRate;
      switch (satelliteChannel.FecCodeRate)
      {
        case FecCodeRate.Rate1_2:
          fecCodeRate = "12";
          break;
        case FecCodeRate.Rate2_3:
          fecCodeRate = "23";
          break;
        case FecCodeRate.Rate3_4:
          fecCodeRate = "34";
          break;
        case FecCodeRate.Rate5_6:
          fecCodeRate = "56";
          break;
        case FecCodeRate.Rate7_8:
          fecCodeRate = "78";
          break;
        case FecCodeRate.Rate8_9:
          fecCodeRate = "89";
          break;
        case FecCodeRate.Rate3_5:
          fecCodeRate = "35";
          break;
        case FecCodeRate.Rate4_5:
          fecCodeRate = "45";
          break;
        case FecCodeRate.Rate9_10:
          fecCodeRate = "910";
          break;
        default:
          this.LogWarn("SAT>IP satellite: unsupported FEC code rate {0}, falling back to 3/4", satelliteChannel.FecCodeRate);
          fecCodeRate = "34";
          break;
      }

      // DVB-S2 or DVB-S?
      string system = "dvbs";
      string modulation = "qpsk";
      string pilotTonesState = "off";
      string rollOffFactor = "0.35";
      if (dvbs2Channel != null)
      {
        system = "dvbs2";

        switch (dvbs2Channel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk4:
            modulation = "qpsk";
            break;
          case ModulationSchemePsk.Psk8:
            modulation = "8psk";
            break;
          default:
            this.LogWarn("SAT>IP satellite: unsupported modulation scheme {0}, falling back to 8 PSK", dvbs2Channel.ModulationScheme);
            modulation = "8psk";
            break;
        }

        switch (dvbs2Channel.RollOffFactor)
        {
          case RollOffFactor.ThirtyFive:
            rollOffFactor = "0.35";
            break;
          case RollOffFactor.TwentyFive:
            rollOffFactor = "0.25";
            break;
          case RollOffFactor.Twenty:
            rollOffFactor = "0.20";
            break;
          default:
            this.LogWarn("SAT>IP satellite: unsupported roll-off factor {0}, falling back to 0.35", dvbs2Channel.RollOffFactor);
            rollOffFactor = "0.35";
            break;
        }

        switch (dvbs2Channel.PilotTonesState)
        {
          case PilotTonesState.Off:
            pilotTonesState = "off";
            break;
          case PilotTonesState.On:
            pilotTonesState = "on";
            break;
          default:
            this.LogWarn("SAT>IP satellite: unsupported pilot tones state {0}, falling back to on", dvbs2Channel.PilotTonesState);
            pilotTonesState = "on";
            break;
        }
      }

      string parameters = string.Format("msys={0}&src={1}&freq={2}&pol={3}&mtype={4}&sr={5}&fec={6}&ro={7}&plts={8}", system, satelliteChannel.Longitude, frequency, polarisation, modulation, satelliteChannel.SymbolRate, fecCodeRate, rollOffFactor, pilotTonesState);
      PerformTuning(channel as IChannelDvb, parameters);
    }

    #endregion

    #endregion
  }
}