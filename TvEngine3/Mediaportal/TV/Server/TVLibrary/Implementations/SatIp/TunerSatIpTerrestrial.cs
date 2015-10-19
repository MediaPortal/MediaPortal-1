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

using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.CP.Description;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for SAT>IP DVB-T and DVB-T2
  /// tuners.
  /// </summary>
  internal class TunerSatIpTerrestrial : TunerSatIpBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpTerrestrial"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerSatIpTerrestrial(DeviceDescriptor serverDescriptor, int sequenceNumber, BroadcastStandard supportedBroadcastStandards, ITunerInternal streamTuner)
      : base(serverDescriptor, sequenceNumber, "T", supportedBroadcastStandards, streamTuner)
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
      this.LogDebug("SAT>IP terrestrial: construct URL");

      IChannelOfdm ofdmChannel = channel as IChannelOfdm;
      if (ofdmChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      // The specification includes guard interval, transmission mode,
      // modulation type and inner FEC rate. However, we don't have these
      // details, and the Digital Devices Octopus Net - currently the only
      // avaliable SAT>IP DVB-T/T2 tuner - doesn't require/use them.
      string parameters = string.Format("freq={0}&bw={1}&msys=dvbt", (ofdmChannel.Frequency / 1000).ToString(), (ofdmChannel.Bandwidth/ 1000).ToString());
      ChannelDvbT2 dvbt2Channel = channel as ChannelDvbT2;
      if (dvbt2Channel != null)
      {
        int plpId = dvbt2Channel.PlpId;
        if (plpId < 0)
        {
          plpId = 0;
        }
        parameters = string.Format("{0}2&plp={1}", parameters, plpId);
      }
      PerformTuning(channel as ChannelDvbBase, parameters);
    }

    #endregion
  }
}