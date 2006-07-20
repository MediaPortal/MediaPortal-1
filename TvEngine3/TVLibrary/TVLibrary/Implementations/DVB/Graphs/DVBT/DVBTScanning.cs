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
  public class DVBTScanning : DvbBaseScanning, ITVScanning, IDisposable
  {
    TvCardDVBT _card;
    public DVBTScanning(TvCardDVBT card)
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

    protected override IStreamAnalyzer GetAnalyzer()
    {
      return _card.StreamAnalyzer;
    }
    protected override IPin PinAnalyzerSI
    {
      get
      {
        return _card.PinAnalyzerSI;
      }
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
      DVBTChannel tuningChannel = (DVBTChannel)_card.Channel;
      DVBTChannel dvbtChannel = new DVBTChannel();
      dvbtChannel.Name = info.service_name;
      dvbtChannel.Provider = info.service_provider_name;
      dvbtChannel.BandWidth = tuningChannel.BandWidth;
      dvbtChannel.Frequency = tuningChannel.Frequency;
      dvbtChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream);
      dvbtChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
      dvbtChannel.NetworkId = info.networkID;
      dvbtChannel.ServiceId = info.serviceID;
      dvbtChannel.TransportId = info.transportStreamID;
      dvbtChannel.PmtPid = info.network_pmt_PID;
      dvbtChannel.PcrPid = info.pcr_pid;
      dvbtChannel.FreeToAir = !info.scrambled;

      Log.Log.Write("Found:{0}", dvbtChannel);
      return dvbtChannel;
    }

  }
}
