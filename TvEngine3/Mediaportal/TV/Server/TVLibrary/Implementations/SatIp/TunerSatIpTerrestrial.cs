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

using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.CP.Description;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles SAT>IP DVB-T
  /// and DVB-T2 tuners.
  /// </summary>
  internal class TunerSatIpTerrestrial : TunerSatIpBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpTerrestrial"/> class.
    /// </summary>
    /// <param name="serverDescriptor">The server's UPnP device description.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerSatIpTerrestrial(DeviceDescriptor serverDescriptor, int sequenceNumber, ITunerInternal streamTuner)
      : base(serverDescriptor, sequenceNumber, streamTuner, CardType.DvbT)
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
      DVBTChannel terrestrialChannel = channel as DVBTChannel;
      if (terrestrialChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      string frequency = ((int)terrestrialChannel.Frequency / 1000).ToString();
      string bandwidth = (terrestrialChannel.Bandwidth / 1000).ToString();

      // TODO add DVB-T2 support when we can distinguish DVB-T and DVB-T2
      // The specification includes guard interval, transmission mode,
      // modulation type and inner FEC rate. However, we don't have these
      // details, and the Digital Devices Octopus Net - currently the only
      // avaliable SAT>IP DVB-T/T2 tuner - doesn't require/use them.
      PerformTuning(terrestrialChannel, string.Format("msys=dvbt&freq={0}&bw={1}", frequency, bandwidth));
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBTChannel;
    }

    #endregion
  }
}