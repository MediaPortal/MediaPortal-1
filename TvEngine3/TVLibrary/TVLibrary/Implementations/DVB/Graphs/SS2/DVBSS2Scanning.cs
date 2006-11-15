/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using DirectShowLib;
using DirectShowLib.BDA;

namespace TvLibrary.Implementations.DVB
{
    enum CardType
    {
      Analog,
      DvbS,
      DvbT,
      DvbC,
      Atsc
    }
  /// <summary>
  /// Class which implements scanning for tv/radio channels for DVB-S SkyStar 2 cards
  /// </summary>
  public class DVBSS2canning : DvbBaseScanning, ITVScanning, IDisposable
  {
    TvCardDvbSS2 _card;
    /// <summary>
    /// Initializes a new instance of the <see cref="DVBSS2canning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DVBSS2canning(TvCardDvbSS2 card)
      : base(card)
    {
      _card = card;
    }

    /// <summary>
    /// returns the tv card used
    /// </summary>
    /// <value></value>
    public ITVCard TvCard
    {
      get
      {
        return _card;
      }
    }

    /// <summary>
    /// Gets the analyzer.
    /// </summary>
    /// <returns></returns>
    protected override ITsChannelScan GetAnalyzer()
    {
      return _card.StreamAnalyzer;
    }
    /// <summary>
    /// Sets the hw pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    protected override void SetHwPids(ArrayList pids)
    {
      _card.SendHWPids(pids);
    }

    /// <summary>
    /// Resets the signal update.
    /// </summary>
    protected override void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }
    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <returns></returns>
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      switch (_card.cardType )
      {
        case (int)CardType.DvbS:
          DVBSChannel tuningChannels = (DVBSChannel)_card.Channel;
          DVBSChannel dvbsChannel = new DVBSChannel();
          dvbsChannel.Name = info.service_name;
          dvbsChannel.Provider = info.service_provider_name;
          dvbsChannel.SymbolRate = tuningChannels.SymbolRate;
          dvbsChannel.Polarisation = tuningChannels.Polarisation;
          dvbsChannel.SwitchingFrequency = tuningChannels.SwitchingFrequency;
          dvbsChannel.Frequency = tuningChannels.Frequency;
          dvbsChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4OrH264Stream);
          dvbsChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
          dvbsChannel.NetworkId = info.networkID;
          dvbsChannel.ServiceId = info.serviceID;
          dvbsChannel.TransportId = info.transportStreamID;
          dvbsChannel.PmtPid = info.network_pmt_PID;
          dvbsChannel.PcrPid = info.pcr_pid;
          dvbsChannel.DisEqc = tuningChannels.DisEqc;
          dvbsChannel.BandType = tuningChannels.BandType;
          dvbsChannel.FreeToAir = !info.scrambled;

          Log.Log.Write("Found:{0}", dvbsChannel);
          return dvbsChannel;
        case (int)CardType.DvbC:
          DVBCChannel tuningChannelc = (DVBCChannel)_card.Channel;
          DVBCChannel dvbcChannel = new DVBCChannel();
          dvbcChannel.Name = info.service_name;
          dvbcChannel.Provider = info.service_provider_name;
          dvbcChannel.SymbolRate = tuningChannelc.SymbolRate;
          dvbcChannel.ModulationType = tuningChannelc.ModulationType;
          dvbcChannel.Frequency = tuningChannelc.Frequency;
          dvbcChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4OrH264Stream);
          dvbcChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
          dvbcChannel.NetworkId = info.networkID;
          dvbcChannel.ServiceId = info.serviceID;
          dvbcChannel.TransportId = info.transportStreamID;
          dvbcChannel.PmtPid = info.network_pmt_PID;
          dvbcChannel.PcrPid = info.pcr_pid;
          dvbcChannel.FreeToAir = !info.scrambled;

          Log.Log.Write("Found:{0}", dvbcChannel);
          return dvbcChannel;
        case (int)CardType.DvbT:
          DVBTChannel tuningChannelt = (DVBTChannel)_card.Channel;
          DVBTChannel dvbtChannel = new DVBTChannel();
          dvbtChannel.Name = info.service_name;
          dvbtChannel.Provider = info.service_provider_name;
          dvbtChannel.Frequency = tuningChannelt.Frequency;
          dvbtChannel.BandWidth = tuningChannelt.BandWidth;
          dvbtChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4OrH264Stream);
          dvbtChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
          dvbtChannel.NetworkId = info.networkID;
          dvbtChannel.ServiceId = info.serviceID;
          dvbtChannel.TransportId = info.transportStreamID;
          dvbtChannel.PmtPid = info.network_pmt_PID;
          dvbtChannel.PcrPid = info.pcr_pid;
          dvbtChannel.FreeToAir = !info.scrambled;

          Log.Log.Write("Found:{0}", dvbtChannel);
          return dvbtChannel;
        case (int)CardType.Atsc:
          ATSCChannel tuningChannela = (ATSCChannel)_card.Channel;
          ATSCChannel dvbaChannel = new ATSCChannel();
          dvbaChannel.Name = info.service_name;
          dvbaChannel.Provider = info.service_provider_name;
          dvbaChannel.Frequency = tuningChannela.Frequency;
          dvbaChannel.PhysicalChannel = tuningChannela.PhysicalChannel;
          dvbaChannel.LogicalChannelNumber = tuningChannela.LogicalChannelNumber;
          dvbaChannel.MajorChannel = tuningChannela.MajorChannel;
          dvbaChannel.MinorChannel = tuningChannela.MinorChannel;
          dvbaChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4OrH264Stream);
          dvbaChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
          dvbaChannel.NetworkId = info.networkID;
          dvbaChannel.ServiceId = info.serviceID;
          dvbaChannel.TransportId = info.transportStreamID;
          dvbaChannel.PmtPid = info.network_pmt_PID;
          dvbaChannel.PcrPid = info.pcr_pid;
          dvbaChannel.FreeToAir = !info.scrambled;

          Log.Log.Write("Found:{0}", dvbaChannel);
          return dvbaChannel;
      }
      return null; // never append just to satify the dev tool :-)
    }
  }
}

