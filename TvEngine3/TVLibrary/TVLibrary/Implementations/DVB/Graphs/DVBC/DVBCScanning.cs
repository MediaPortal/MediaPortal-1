using System;
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
  public class DVBCScanning : DvbBaseScanning, ITVScanning, IDisposable
  {
    TvCardDVBC _card;
    public DVBCScanning(TvCardDVBC card)
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
    protected override void SetHwPids(System.Collections.ArrayList pids)
    {
      _card.SendHwPids(pids);
    }

    protected override void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      DVBCChannel tuningChannel = (DVBCChannel)_card.Channel;
      DVBCChannel dvbcChannel = new DVBCChannel();
      dvbcChannel.Name = info.service_name;
      dvbcChannel.Provider = info.service_provider_name;
      dvbcChannel.SymbolRate = tuningChannel.SymbolRate;
      dvbcChannel.ModulationType = tuningChannel.ModulationType;
      dvbcChannel.Frequency = tuningChannel.Frequency;
      dvbcChannel.IsTv= (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream);
      dvbcChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
      dvbcChannel.NetworkId = info.networkID;
      dvbcChannel.ServiceId = info.serviceID;
      dvbcChannel.TransportId = info.transportStreamID;
      dvbcChannel.PmtPid = info.network_pmt_PID;
      dvbcChannel.PcrPid = info.pcr_pid;
      dvbcChannel.FreeToAir = !info.scrambled;
      Log.Log.Write("Found: {0}",dvbcChannel);
      return dvbcChannel;
    }
  }
}
