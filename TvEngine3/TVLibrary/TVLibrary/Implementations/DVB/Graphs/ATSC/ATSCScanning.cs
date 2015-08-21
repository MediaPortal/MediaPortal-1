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
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for ATSC BDA cards
  /// </summary>
  public class ATSCScanning : DvbBaseScanning
  {
    /// <summary>
    /// ATSC service types - see A/53 part 1
    /// </summary>
    protected enum AtscServiceType
    {
      /// <summary>
      /// Analog Television (See A/65 [9])
      /// </summary>
      AnalogTelevision = 0x01,
      /// <summary>
      /// ATSC Digital Television (See A/53-3 [2])
      /// </summary>
      DigitalTelevision = 0x02,
      /// <summary>
      /// ATSC Audio (See A/53-3 [2])
      /// </summary>
      Audio = 0x03
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ATSCScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public ATSCScanning(TvCardDvbBase card)
      : base(card)
    {
    }

    /// <summary>
    /// Scans the specified transponder.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    public override List<IChannel> Scan(IChannel channel, ScanParameters settings)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        _transportStreamStandard = TransportStreamStandard.Default;
      }
      else if (atscChannel.ModulationType == ModulationType.Mod8Vsb || atscChannel.ModulationType == ModulationType.Mod16Vsb)
      {
        _transportStreamStandard = TransportStreamStandard.Atsc;
      }
      else
      {
        _transportStreamStandard = TransportStreamStandard.Scte;
      }
      return base.Scan(channel, settings);
    }

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="channel">The high level tuning detail.</param>
    /// <param name="info">The subchannel detail.</param>
    /// <returns>The new channel.</returns>
    protected override IChannel CreateNewChannel(IChannel channel, ChannelInfo info)
    {
      ATSCChannel tuningChannel = (ATSCChannel)channel;
      ATSCChannel atscChannel = new ATSCChannel();
      // When scanning with a CableCARD tuner and/or NIT or VCT frequency info
      // has been received and it looks plausible...
      if (tuningChannel.PhysicalChannel == 0 ||
        (info.freq > 1750 && tuningChannel.Frequency > 0 && info.freq != tuningChannel.Frequency))
      {
        atscChannel.PhysicalChannel = ATSCChannel.GetPhysicalChannelFromFrequency(info.freq);
        // Convert from centre frequency to the analog video carrier
        // frequency. This is a BDA convention.
        atscChannel.Frequency = info.freq - 1750;
      }
      else
      {
        atscChannel.PhysicalChannel = tuningChannel.PhysicalChannel;
        atscChannel.Frequency = tuningChannel.Frequency;
      }
      if (info.minorChannel == 0)
      {
        atscChannel.LogicalChannelNumber = info.majorChannel;
      }
      else
      {
        atscChannel.LogicalChannelNumber = (info.majorChannel * 1000) + info.minorChannel;
      }
      atscChannel.Name = info.service_name;
      atscChannel.Provider = info.service_provider_name;
      atscChannel.ModulationType = tuningChannel.ModulationType;
      atscChannel.MajorChannel = info.majorChannel;
      atscChannel.MinorChannel = info.minorChannel;
      atscChannel.IsTv = IsTvService(info.serviceType);
      atscChannel.IsRadio = IsRadioService(info.serviceType);
      atscChannel.NetworkId = info.networkID;
      atscChannel.ServiceId = info.serviceID;
      atscChannel.TransportId = info.transportStreamID;
      atscChannel.PmtPid = info.network_pmt_PID;
      atscChannel.FreeToAir = !info.scrambled;
      Log.Log.Write("atsc:Found: {0}", atscChannel);
      return atscChannel;
    }

    protected override void SetNameForUnknownChannel(IChannel channel, ChannelInfo info)
    {
      if (info.minorChannel > 0)
      {
        // Standard ATSC two part channel number available.
        info.service_name = String.Format("Unknown {0}-{1}", info.majorChannel, info.minorChannel);
      }
      else
      {
        // QAM channel number. Informal standard used by TVs and other clear QAM equipment when PSIP is not available.
        info.service_name = String.Format("Unknown {0}-{1}", ((ATSCChannel)channel).PhysicalChannel, info.serviceID);
      }
      Log.Log.Info("ATSC: service name is not available, so set to {0}", info.service_name);
    }

    protected override bool IsRadioService(int serviceType)
    {
      return serviceType == (int)AtscServiceType.Audio;
    }

    protected override bool IsTvService(int serviceType)
    {
      return serviceType == (int)AtscServiceType.AnalogTelevision ||
             serviceType == (int)AtscServiceType.DigitalTelevision;
    }
  }
}