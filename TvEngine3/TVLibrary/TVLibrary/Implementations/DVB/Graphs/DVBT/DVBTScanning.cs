/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
  /// Class which implements scanning for tv/radio channels for DVB-T BDA cards
  /// </summary>
  public class DVBTScanning : DvbBaseScanning, ITVScanning, IDisposable
  {
    readonly TvCardDVBT _card;
    /// <summary>
    /// Initializes a new instance of the <see cref="DVBTScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DVBTScanning(TvCardDVBT card)
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
      DVBTChannel tuningChannel = (DVBTChannel)_card.CurrentChannel;
      DVBTChannel dvbtChannel = new DVBTChannel();
      dvbtChannel.Name = info.service_name;
      dvbtChannel.LogicalChannelNumber = info.LCN;
      dvbtChannel.Provider = info.service_provider_name;
      dvbtChannel.BandWidth = tuningChannel.BandWidth;
      dvbtChannel.Frequency = tuningChannel.Frequency;
      dvbtChannel.IsTv = (info.serviceType == (int)ServiceType.Video || info.serviceType == (int)ServiceType.Mpeg2HDStream || info.serviceType == (int)ServiceType.H264Stream || info.serviceType == (int)ServiceType.AdvancedCodecHDVideoStream || info.serviceType == (int)ServiceType.Mpeg4OrH264Stream);
      dvbtChannel.IsRadio = (info.serviceType == (int)ServiceType.Audio);
      dvbtChannel.NetworkId = info.networkID;
      dvbtChannel.ServiceId = info.serviceID;
      dvbtChannel.TransportId = info.transportStreamID;
      dvbtChannel.PmtPid = info.network_pmt_PID;
      dvbtChannel.PcrPid = info.pcr_pid;
      dvbtChannel.FreeToAir = !info.scrambled;
      dvbtChannel.VideoPid = info.videoPid;
      dvbtChannel.AudioPid = info.audioPid;
      Log.Log.Write("Found: {0}", dvbtChannel);
      return dvbtChannel;
    }
  }
}
