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
  public class DVBSS2canning : DvbBaseScanning, ITVScanning, IDisposable
  {
    TvCardDvbSS2 _card;
    public DVBSS2canning(TvCardDvbSS2 card)
      : base(card)
    {
      _card = card;
    }

    public ITVCard TvCard
    {
      get
      {
        return _card;
      }
    }

    protected override ITsChannelScan GetAnalyzer()
    {
      return _card.StreamAnalyzer;
    }
    protected override void SetHwPids(ArrayList pids)
    {
      _card.SendHWPids(pids);
    }

    protected override void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      DVBSChannel tuningChannel = (DVBSChannel)_card.Channel;
      DVBSChannel dvbsChannel = new DVBSChannel();
      dvbsChannel.Name = info.service_name;
      dvbsChannel.Provider = info.service_provider_name;
      dvbsChannel.SymbolRate = tuningChannel.SymbolRate;
      dvbsChannel.Polarisation = tuningChannel.Polarisation;
      dvbsChannel.SwitchingFrequency = tuningChannel.SwitchingFrequency;
      dvbsChannel.Frequency = tuningChannel.Frequency;
      dvbsChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream);
      dvbsChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
      dvbsChannel.NetworkId = info.networkID;
      dvbsChannel.ServiceId = info.serviceID;
      dvbsChannel.TransportId = info.transportStreamID;
      dvbsChannel.PmtPid = info.network_pmt_PID;
      dvbsChannel.PcrPid = info.pcr_pid;
      dvbsChannel.DisEqc = tuningChannel.DisEqc;
      dvbsChannel.FreeToAir = !info.scrambled;

      Log.Log.Write("Found:{0}", dvbsChannel);
      return dvbsChannel;
    }
  }
}

