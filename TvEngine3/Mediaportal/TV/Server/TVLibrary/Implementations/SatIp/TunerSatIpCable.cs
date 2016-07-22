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
  /// An implementation of <see cref="ITuner"/> for SAT>IP DVB-C tuners.
  /// </summary>
  internal class TunerSatIpCable : TunerSatIpBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpCable"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerSatIpCable(DeviceDescriptor serverDescriptor, int sequenceNumber, ITunerInternal streamTuner)
      : base(serverDescriptor, sequenceNumber, "C", BroadcastStandard.DvbC, streamTuner)
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
      this.LogDebug("SAT>IP cable: perform tuning");
      ChannelDvbC dvbcChannel = channel as ChannelDvbC;
      if (dvbcChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      string modulation;
      switch (dvbcChannel.ModulationScheme)
      {
        case ModulationSchemeQam.Qam16:
          modulation = "16qam";
          break;
        case ModulationSchemeQam.Qam32:
          modulation = "32qam";
          break;
        case ModulationSchemeQam.Qam64:
          modulation = "64qam";
          break;
        case ModulationSchemeQam.Qam128:
          modulation = "128qam";
          break;
        case ModulationSchemeQam.Qam256:
          modulation = "256qam";
          break;
        default:
          this.LogWarn("SAT>IP cable: unsupported modulation scheme {0}, falling back to 256 QAM", dvbcChannel.ModulationScheme);
          modulation = "256qam";
          break;
      }

      PerformTuning(dvbcChannel, string.Format("msys=dvbc&freq={0}&mtype={1}&sr={2}", dvbcChannel.Frequency / 1000, modulation, dvbcChannel.SymbolRate));
    }

    #endregion
  }
}