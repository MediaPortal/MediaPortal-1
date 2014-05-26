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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.CP.Description;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.SatIp
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles SAT>IP DVB-C
  /// tuners.
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
      : base(serverDescriptor, sequenceNumber, streamTuner, CardType.DvbC)
    {
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("SAT>IP cable: construct URL");
      DVBCChannel cableChannel = channel as DVBCChannel;
      if (cableChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      string modulation = "256qam";
      if (cableChannel.ModulationType == ModulationType.Mod64Qam)
      {
        modulation = "64qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod80Qam)
      {
        modulation = "80qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod96Qam)
      {
        modulation = "96qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod112Qam)
      {
        modulation = "112qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod128Qam)
      {
        modulation = "128qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod160Qam)
      {
        modulation = "160qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod192Qam)
      {
        modulation = "192qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod224Qam)
      {
        modulation = "224qam";
      }
      else if (cableChannel.ModulationType == ModulationType.Mod256Qam)
      {
        modulation = "256qam";
      }
      else
      {
        this.LogWarn("SAT>IP cable: unsupported modulation type {0}, assuming 256 QAM", cableChannel.ModulationType);
      }

      PerformTuning(string.Format("msys=dvbc&freq={0}&mtype={1}&sr={2}", cableChannel.Frequency / 1000, modulation, cableChannel.SymbolRate));
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBCChannel;
    }

    #endregion
  }
}