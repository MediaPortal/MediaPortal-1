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
using System.Collections.Generic;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for DVB-T BDA cards
  /// </summary>
  public class DVBTScanning : DvbBaseScanning
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DVBTScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DVBTScanning(TvCardDvbBase card) : base(card) {}

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="channel">The high level tuning detail.</param>
    /// <param name="info">The subchannel detail.</param>
    /// <returns>The new channel.</returns>
    protected override IChannel CreateNewChannel(IChannel channel, ChannelInfo info)
    {
      DVBTChannel tuningChannel = (DVBTChannel)channel;
      DVBTChannel dvbtChannel = new DVBTChannel();
      dvbtChannel.Name = info.service_name;
      dvbtChannel.LogicalChannelNumber = info.LCN;
      dvbtChannel.Provider = info.service_provider_name;
      dvbtChannel.BandWidth = tuningChannel.BandWidth;
      dvbtChannel.Frequency = tuningChannel.Frequency;
      dvbtChannel.IsTv = IsTvService(info.serviceType);
      dvbtChannel.IsRadio = IsRadioService(info.serviceType);
      dvbtChannel.NetworkId = info.networkID;
      dvbtChannel.ServiceId = info.serviceID;
      dvbtChannel.TransportId = info.transportStreamID;
      dvbtChannel.PmtPid = info.network_pmt_PID;
      dvbtChannel.FreeToAir = !info.scrambled;
      Log.Log.Write("Found: {0}", dvbtChannel);
      return dvbtChannel;
    }
  }
}