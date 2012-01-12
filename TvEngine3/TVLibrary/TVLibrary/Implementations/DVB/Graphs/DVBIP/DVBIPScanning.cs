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

using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class for DVBIP scanning
  /// </summary>
  public class DVBIPScanning : DvbBaseScanning
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="card"></param>
    public DVBIPScanning(TvCardDVBIP card) : base(card) {}

    /// <summary>
    /// CreateNewChannel
    /// </summary>
    /// <param name="channel">The high level tuning detail.</param>
    /// <param name="info">The subchannel detail.</param>
    /// <returns>The new channel.</returns>
    protected override IChannel CreateNewChannel(IChannel channel, ChannelInfo info)
    {
      DVBIPChannel tuningChannel = (DVBIPChannel)channel;
      DVBIPChannel dvbipChannel = new DVBIPChannel();
      dvbipChannel.Name = info.service_name;
      dvbipChannel.LogicalChannelNumber = info.LCN;
      dvbipChannel.Provider = info.service_provider_name;
      dvbipChannel.Url = tuningChannel.Url;
      dvbipChannel.IsTv = IsTvService(info.serviceType);
      dvbipChannel.IsRadio = IsRadioService(info.serviceType);
      dvbipChannel.NetworkId = info.networkID;
      dvbipChannel.ServiceId = info.serviceID;
      dvbipChannel.TransportId = info.transportStreamID;
      dvbipChannel.PmtPid = info.network_pmt_PID;
      dvbipChannel.FreeToAir = !info.scrambled;
      Log.Log.Write("Found: {0}", dvbipChannel);
      return dvbipChannel;
    }

    protected override bool IsValidChannel(ChannelInfo info, short hasAudio, short hasVideo)
    {
      //In DVB-IP there are several ways to describe a channel. It is possible that no service type is given. 
      //Until we can fully implement DVB-IP support we check if video or audio is available to determine the channel type.
      if (info.serviceType <= 0)
      {
        if (hasVideo != 0)
        {
          info.serviceType = (int)DvbServiceType.DigitalTelevision;
        }
        else if (hasAudio != 0)
        {
          info.serviceType = (int)DvbServiceType.DigitalRadio;
        }
      } 
      return base.IsValidChannel(info, hasAudio, hasVideo);
    }
  }
}