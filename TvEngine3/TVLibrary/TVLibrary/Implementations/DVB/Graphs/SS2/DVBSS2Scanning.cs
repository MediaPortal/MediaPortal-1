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

