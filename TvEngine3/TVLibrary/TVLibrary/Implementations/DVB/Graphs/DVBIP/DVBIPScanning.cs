/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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

using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  public class DVBIPScanning : DvbBaseScanning, ITVScanning
  {
    TvCardDVBIP _card;

    public DVBIPScanning(TvCardDVBIP card) : base(card)
    {
      _card = card;
    }

    #region Implementation of ITVScanning

    public ITVCard TvCard
    {
      get { return _card; }
    }

    #endregion

    protected override ITsChannelScan GetAnalyzer()
    {
      return _card.StreamAnalyzer;
    }

    protected override void  SetHwPids(System.Collections.Generic.List<ushort> pids)
    {
      _card.SendHwPids(pids);
    }

    protected override void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }

    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      DVBIPChannel tunningChannel = (DVBIPChannel) _card.CurrentChannel;
      DVBIPChannel dvbipChannel = new DVBIPChannel();
      dvbipChannel.Name = info.service_name;
      dvbipChannel.LogicalChannelNumber = info.LCN;
      dvbipChannel.Provider = info.service_provider_name;
      dvbipChannel.Url = tunningChannel.Url;
      dvbipChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg2HDStream || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.AdvancedCodecHDVideoStream || info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4OrH264Stream);
      dvbipChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
      dvbipChannel.NetworkId = info.networkID;
      dvbipChannel.ServiceId = info.serviceID;
      dvbipChannel.TransportId = info.transportStreamID;
      dvbipChannel.PmtPid = info.network_pmt_PID;
      dvbipChannel.PcrPid = info.pcr_pid;
      dvbipChannel.FreeToAir = !info.scrambled;
      dvbipChannel.VideoPid = info.videoPid;
      dvbipChannel.AudioPid = info.audioPid;
      Log.Log.Write("Found: {0}", dvbipChannel);
      return dvbipChannel;
    }
  }
}