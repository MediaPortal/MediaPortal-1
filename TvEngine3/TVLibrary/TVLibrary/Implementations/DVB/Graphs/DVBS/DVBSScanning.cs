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
  public class DVBSScanning : DvbBaseScanning, ITVScanning, IDisposable
  {
    TvCardDVBS _card;
    /// <summary>
    /// Initializes a new instance of the <see cref="T:DVBSScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DVBSScanning(TvCardDVBS card)
      :base(card)
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
    protected override void SetHwPids(System.Collections.ArrayList pids)
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
      DVBSChannel tuningChannel = (DVBSChannel)_card.Channel;
      DVBSChannel dvbsChannel = new DVBSChannel();
      dvbsChannel.Name = info.service_name;
      dvbsChannel.LogicalChannelNumber = info.LCN;
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
      Log.Log.Write("Found: {0}", dvbsChannel);
      return dvbsChannel;
    }

  }
}
