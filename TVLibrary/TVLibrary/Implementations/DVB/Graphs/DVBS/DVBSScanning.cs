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
  /// Class which implements scanning for tv/radio channels for DVB-S BDA cards
  /// </summary>
  public class DVBSScanning : DvbBaseScanning
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DVBSScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DVBSScanning(TvCardDvbBase card) : base(card) {}

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="channel">The high level tuning detail.</param>
    /// <param name="info">The subchannel detail.</param>
    /// <returns>The new channel.</returns>
    protected override IChannel CreateNewChannel(IChannel channel, ChannelInfo info)
    {
      DVBSChannel tuningChannel = (DVBSChannel)channel;
      DVBSChannel dvbsChannel = new DVBSChannel();
      dvbsChannel.Name = info.service_name;
      dvbsChannel.LogicalChannelNumber = info.LCN;
      dvbsChannel.Provider = info.service_provider_name;
      dvbsChannel.SymbolRate = tuningChannel.SymbolRate;
      dvbsChannel.Polarisation = tuningChannel.Polarisation;
      dvbsChannel.SwitchingFrequency = tuningChannel.SwitchingFrequency;
      dvbsChannel.Frequency = tuningChannel.Frequency;
      dvbsChannel.IsTv = IsTvService(info.serviceType);
      dvbsChannel.IsRadio = IsRadioService(info.serviceType);
      dvbsChannel.NetworkId = info.networkID;
      dvbsChannel.ServiceId = info.serviceID;
      dvbsChannel.TransportId = info.transportStreamID;
      dvbsChannel.PmtPid = info.network_pmt_PID;
      dvbsChannel.DisEqc = tuningChannel.DisEqc;
      dvbsChannel.BandType = tuningChannel.BandType;
      dvbsChannel.FreeToAir = !info.scrambled;
      dvbsChannel.SatelliteIndex = tuningChannel.SatelliteIndex;
      dvbsChannel.ModulationType = tuningChannel.ModulationType;
      dvbsChannel.InnerFecRate = tuningChannel.InnerFecRate;
      dvbsChannel.Pilot = tuningChannel.Pilot;
      dvbsChannel.Rolloff = tuningChannel.Rolloff;
      Log.Log.Write("Found: {0}", dvbsChannel);
      return dvbsChannel;
    }
  }
}