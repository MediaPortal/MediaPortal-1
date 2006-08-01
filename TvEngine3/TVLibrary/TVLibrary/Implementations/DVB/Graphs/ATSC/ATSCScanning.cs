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
  public class ATSCScanning : DvbBaseScanning, ITVScanning, IDisposable
  {
    TvCardATSC _card;
    public ATSCScanning(TvCardATSC card)
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
    protected override IPin PinAnalyzerSI
    {
      get
      {
        return _card.PinAnalyzerSI;
      }
    }

    protected override void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      ATSCChannel tuningChannel = (ATSCChannel)_card.Channel;
      ATSCChannel atscChannel = new ATSCChannel();
      atscChannel.Name = info.service_name;
      atscChannel.Provider = info.service_provider_name;
      atscChannel.SymbolRate = tuningChannel.SymbolRate;
      atscChannel.ModulationType = tuningChannel.ModulationType;
      atscChannel.Frequency = tuningChannel.Frequency;
      atscChannel.PhysicalChannel = tuningChannel.PhysicalChannel;
      atscChannel.MajorChannel = info.majorChannel;
      atscChannel.MinorChannel = info.minorChannel;
      atscChannel.IsTv = (info.serviceType == (int)DvbBaseScanning.ServiceType.Mpeg4Stream || info.serviceType == (int)DvbBaseScanning.ServiceType.Video || info.serviceType == (int)DvbBaseScanning.ServiceType.H264Stream);
      atscChannel.IsRadio = (info.serviceType == (int)DvbBaseScanning.ServiceType.Audio);
      atscChannel.TransportId = info.transportStreamID;
      atscChannel.PmtPid = info.network_pmt_PID;
      atscChannel.PcrPid = info.pcr_pid;
      atscChannel.FreeToAir = !info.scrambled;
      Log.Log.Write("Found:{1}", atscChannel);
      return atscChannel;
    }
  }
}
