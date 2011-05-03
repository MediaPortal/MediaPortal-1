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
using MediaPortal.CoreServices;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for DVB-C BDA cards
  /// </summary>
  public class DVBCScanning : DvbBaseScanning
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DVBCScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DVBCScanning(TvCardDvbBase card) : base(card) {}

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <returns></returns>
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      DVBCChannel tuningChannel = (DVBCChannel)_card.CurrentChannel;
      DVBCChannel dvbcChannel = new DVBCChannel();
      dvbcChannel.Name = info.service_name;
      dvbcChannel.LogicalChannelNumber = info.LCN;
      dvbcChannel.Provider = info.service_provider_name;
      dvbcChannel.SymbolRate = tuningChannel.SymbolRate;
      dvbcChannel.ModulationType = tuningChannel.ModulationType;
      dvbcChannel.Frequency = tuningChannel.Frequency;
      dvbcChannel.IsTv = IsTvService(info.serviceType);
      dvbcChannel.IsRadio = IsRadioService(info.serviceType);
      dvbcChannel.NetworkId = info.networkID;
      dvbcChannel.ServiceId = info.serviceID;
      dvbcChannel.TransportId = info.transportStreamID;
      dvbcChannel.PmtPid = info.network_pmt_PID;
      dvbcChannel.FreeToAir = !info.scrambled;
      GlobalServiceProvider.Instance.Get<ILogger>().Info("Found: {0}", dvbcChannel);
      return dvbcChannel;
    }
  }
}