/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for DVB-S SkyStar 2 cards
  /// </summary>
  public class DVBSS2canning : DvbBaseScanning, ITVScanning, IDisposable
  {
    readonly TvCardDvbSS2 _card;
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
    protected override void SetHwPids(List<ushort> pids)
    {
      _card.SendHwPids(pids);
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
      switch (_card.CardType)
      {
        case CardType.DvbS:
          DVBSChannel tuningChannels = (DVBSChannel)_card.CurrentChannel;
          DVBSChannel dvbsChannel = new DVBSChannel();
          dvbsChannel.Name = info.service_name;
          dvbsChannel.Provider = info.service_provider_name;
          dvbsChannel.SymbolRate = tuningChannels.SymbolRate;
          dvbsChannel.Polarisation = tuningChannels.Polarisation;
          dvbsChannel.SwitchingFrequency = tuningChannels.SwitchingFrequency;
          dvbsChannel.Frequency = tuningChannels.Frequency;
          dvbsChannel.IsTv = (info.serviceType == (int)ServiceType.Video || info.serviceType == (int)ServiceType.Mpeg2HDStream || info.serviceType == (int)ServiceType.H264Stream || info.serviceType == (int)ServiceType.AdvancedCodecHDVideoStream || info.serviceType == (int)ServiceType.Mpeg4OrH264Stream);
          dvbsChannel.IsRadio = (info.serviceType == (int)ServiceType.Audio);
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
        case CardType.DvbC:
          DVBCChannel tuningChannelc = (DVBCChannel)_card.CurrentChannel;
          DVBCChannel dvbcChannel = new DVBCChannel();
          dvbcChannel.Name = info.service_name;
          dvbcChannel.Provider = info.service_provider_name;
          dvbcChannel.SymbolRate = tuningChannelc.SymbolRate;
          dvbcChannel.ModulationType = tuningChannelc.ModulationType;
          dvbcChannel.Frequency = tuningChannelc.Frequency;
          dvbcChannel.IsTv = (info.serviceType == (int)ServiceType.Video || info.serviceType == (int)ServiceType.Mpeg2HDStream || info.serviceType == (int)ServiceType.H264Stream || info.serviceType == (int)ServiceType.AdvancedCodecHDVideoStream || info.serviceType == (int)ServiceType.Mpeg4OrH264Stream);
          dvbcChannel.IsRadio = (info.serviceType == (int)ServiceType.Audio);
          dvbcChannel.NetworkId = info.networkID;
          dvbcChannel.ServiceId = info.serviceID;
          dvbcChannel.TransportId = info.transportStreamID;
          dvbcChannel.PmtPid = info.network_pmt_PID;
          dvbcChannel.PcrPid = info.pcr_pid;
          dvbcChannel.FreeToAir = !info.scrambled;
          Log.Log.Write("Found:{0}", dvbcChannel);
          return dvbcChannel;
        case CardType.DvbT:
          DVBTChannel tuningChannelt = (DVBTChannel)_card.CurrentChannel;
          DVBTChannel dvbtChannel = new DVBTChannel();
          dvbtChannel.Name = info.service_name;
          dvbtChannel.Provider = info.service_provider_name;
          dvbtChannel.Frequency = tuningChannelt.Frequency;
          dvbtChannel.BandWidth = tuningChannelt.BandWidth;
          dvbtChannel.IsTv = (info.serviceType == (int)ServiceType.Video || info.serviceType == (int)ServiceType.Mpeg2HDStream || info.serviceType == (int)ServiceType.H264Stream || info.serviceType == (int)ServiceType.AdvancedCodecHDVideoStream || info.serviceType == (int)ServiceType.Mpeg4OrH264Stream);
          dvbtChannel.IsRadio = (info.serviceType == (int)ServiceType.Audio);
          dvbtChannel.NetworkId = info.networkID;
          dvbtChannel.ServiceId = info.serviceID;
          dvbtChannel.TransportId = info.transportStreamID;
          dvbtChannel.PmtPid = info.network_pmt_PID;
          dvbtChannel.PcrPid = info.pcr_pid;
          dvbtChannel.FreeToAir = !info.scrambled;
          Log.Log.Write("Found:{0}", dvbtChannel);
          return dvbtChannel;
        case CardType.Atsc:
          ATSCChannel tuningChannela = (ATSCChannel)_card.CurrentChannel;
          ATSCChannel atscChannel = new ATSCChannel();
          atscChannel.Name = info.service_name;
          atscChannel.Provider = info.service_provider_name;
          atscChannel.Frequency = tuningChannela.Frequency;
          atscChannel.PhysicalChannel = tuningChannela.PhysicalChannel;
          atscChannel.ModulationType = tuningChannela.ModulationType;
          atscChannel.LogicalChannelNumber = tuningChannela.LogicalChannelNumber;
          atscChannel.MajorChannel = tuningChannela.MajorChannel;
          atscChannel.MinorChannel = tuningChannela.MinorChannel;
          atscChannel.IsTv = (info.serviceType == (int)ServiceType.Video || info.serviceType == (int)ServiceType.Mpeg2HDStream || info.serviceType == (int)ServiceType.H264Stream || info.serviceType == (int)ServiceType.AdvancedCodecHDVideoStream || info.serviceType == (int)ServiceType.Mpeg4OrH264Stream);
          atscChannel.IsRadio = (info.serviceType == (int)ServiceType.Audio);
          atscChannel.NetworkId = info.networkID;
          atscChannel.ServiceId = info.serviceID;
          atscChannel.TransportId = info.transportStreamID;
          atscChannel.PmtPid = info.network_pmt_PID;
          atscChannel.PcrPid = info.pcr_pid;
          atscChannel.FreeToAir = !info.scrambled;
          foreach (PidInfo pid in info.pids)
          {
            if (pid.isAC3Audio)
            {
              if (pid.pid > 0)
              {
                atscChannel.AudioPid = pid.pid;
              }
            }
            if (pid.isVideo)
              atscChannel.VideoPid = pid.pid;
          }
          Log.Log.Write("Found:{0}", atscChannel);
          return atscChannel;
      }
      return null; // never append just to satify the dev tool :-)
    }
  }
}

