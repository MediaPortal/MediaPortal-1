#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using DirectShowLib;
using DirectShowLib.BDA;


namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// base class for scanning DVB tv/radio channels
  /// </summary>
  public abstract class DvbBaseScanning : IHardwarePidFiltering, IChannelScanCallback, ITVScanning
  {
    #region enums

    /// <summary>
    /// DVB service types - see ETSI EN 300 468
    /// </summary>
    protected enum DvbServiceType
    {
      // (0x00 reserved)

      /// <summary>
      /// digital television service
      /// </summary>
      DigitalTelevision = 0x01,

      /// <summary>
      /// digital radio sound service
      /// </summary>
      DigitalRadio = 0x02,

      /// <summary>
      /// teletext service
      /// </summary>
      Teletext = 0x03,

      /// <summary>
      /// Near Video On Demand reference service
      /// </summary>
      NvodReference = 0x04,

      /// <summary>
      /// Near Video On Demand time-shifted service
      /// </summary>
      NvodTimeShifted = 0x05,

      /// <summary>
      /// mosaic service
      /// </summary>
      Mosaic = 0x06,

      /// <summary>
      /// FM radio service
      /// </summary>
      FmRadio = 0x07,

      /// <summary>
      /// DVB System Renewability Messages service
      /// </summary>
      DvbSrm = 0x08,

      // (0x09 reserved)

      /// <summary>
      /// advanced codec digital radio sound service
      /// </summary>
      AdvancedCodecDigitalRadio = 0x0A,

      /// <summary>
      /// advanced codec mosaic service
      /// </summary>
      AdvancedCodecMosaic = 0x0B,

      /// <summary>
      /// data broadcast service
      /// </summary>
      DataBroadcast = 0x0C,

      // (0x0d reserved for common interface use)

      /// <summary>
      /// Return Channel via Satellite map
      /// </summary>
      RcsMap = 0x0E,

      /// <summary>
      /// Return Channel via Satellite Forward Link Signalling
      /// </summary>
      RcsFls = 0x0F,

      /// <summary>
      /// DVB Multimedia Home Platform service
      /// </summary>
      DvbMhp = 0x10,

      /// <summary>
      /// MPEG 2 HD digital television service
      /// </summary>
      Mpeg2HdDigitalTelevision = 0x11,

      // (0x12 to 0x15 reserved)

      /// <summary>
      /// advanced codec SD digital television service
      /// </summary>
      AdvancedCodecSdDigitalTelevision = 0x16,

      /// <summary>
      /// advanced codec SD Near Video On Demand time-shifted service
      /// </summary>
      AdvancedCodecSdNvodTimeShifted = 0x17,

      /// <summary>
      /// advanced codec SD Near Video On Demand reference service
      /// </summary>
      AdvancedCodecSdNvodReference = 0x18,

      /// <summary>
      /// advanced codec HD digital television
      /// </summary>
      AdvancedCodecHdDigitalTelevision = 0x19,

      /// <summary>
      /// advanced codec HD Near Video On Demand time-shifted service
      /// </summary>
      AdvancedCodecHdNvodTimeShifted = 0x1A,

      /// <summary>
      /// advanced codec HD Near Video On Demand reference service
      /// </summary>
      AdvancedCodecHdNvodReference = 0x1B,

      /// <summary>
      /// sky germany linked channels (option channels)
      /// </summary>
      SkyGermanyOptionChannel = 0xd3

      // (0x1C to 0x7F reserved)
      // (0x80 to 0xFE user defined)
      // (0xFF reserved)
    }

    #endregion

    #region variables

    private ITsChannelScan _analyzer;
    protected readonly TvCardDvbBase _card;
    private ManualResetEvent _event;

    /// <summary>
    /// Enable wait for VCT indicator
    /// </summary>
    protected bool _enableWaitForVCT;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="DvbBaseScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DvbBaseScanning(TvCardDvbBase card)
    {
      _card = card;
    }

    #endregion

    /// <summary>
    /// returns the tv card used
    /// </summary>
    /// <value></value>
    public ITVCard TvCard
    {
      get { return _card; }
    }

    #region virtual members

    /// <summary>
    /// Gets the pin analyzer SI.
    /// </summary>
    /// <value>The pin analyzer SI.</value>
    protected virtual IPin PinAnalyzerSI
    {
      get { return null; }
    }

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="channel">The high level tuning detail.</param>
    /// <param name="info">The subchannel detail.</param>
    /// <returns>The new channel.</returns>
    protected abstract IChannel CreateNewChannel(IChannel channel, ChannelInfo info);

    /// <summary>
    /// Gets the analyzer.
    /// </summary>
    /// <returns></returns>
    protected virtual ITsChannelScan GetAnalyzer()
    {
      return _card.StreamAnalyzer;
    }

    /// <summary>
    /// Sets the hw pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    protected virtual void SetHwPids(List<ushort> pids)
    {
      _card.SendHwPids(pids);
    }

    /// <summary>
    /// Resets the signal update.
    /// </summary>
    protected virtual void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset() {}

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      if (_analyzer == null)
        return;
      //_analyzer.SetPidFilterCallback(null);
    }

    #endregion

    #region channel scanning

    /// <summary>
    /// Scans the specified transponder.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    public List<IChannel> Scan(IChannel channel, ScanParameters settings)
    {
      try
      {
        _card.IsScanning = true;
        _card.Scan(0, channel);
        _analyzer = GetAnalyzer();
        if (_analyzer == null)
        {
          Log.Log.WriteFile("Scan: no analyzer interface available");
          return new List<IChannel>();
        }
        ResetSignalUpdate();
        if (_card.IsTunerLocked == false)
        {
          Thread.Sleep(settings.TimeOutTune * 1000);
          ResetSignalUpdate();
        }
        Log.Log.WriteFile("Scan: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel,
                          _card.SignalQuality);
        if (_card.IsTunerLocked || _card.SignalLevel > 0 || _card.SignalQuality > 0)
        {
          try
          {
            _event = new ManualResetEvent(false);
            _analyzer.SetCallBack(this);
            _analyzer.Start(_enableWaitForVCT);
            _event.WaitOne(settings.TimeOutSDT * 1000, true);

            int found = 0;
            short channelCount;
            _analyzer.GetCount(out channelCount);
            List<IChannel> channelsFound = new List<IChannel>();

            for (int i = 0; i < channelCount; ++i)
            {
              int networkId;
              int transportId;
              int serviceId;
              short majorChannel;
              short minorChannel;
              short frequency;
              short freeCAMode;
              short serviceType;
              short modulation;
              IntPtr providerName;
              IntPtr serviceName;
              short pmtPid;
              short hasVideo;
              short hasAudio;
              short hasCaDescriptor;
              short lcn;
              _analyzer.GetChannel((short)i,
                                   out networkId, out transportId, out serviceId, out majorChannel, out minorChannel,
                                   out frequency, out lcn, out freeCAMode, out serviceType, out modulation,
                                   out providerName, out serviceName,
                                   out pmtPid, out hasVideo, out hasAudio, out hasCaDescriptor);

              string name = DvbTextConverter.Convert(serviceName, "");
              Log.Log.Write("{0}) 0x{1:X} 0x{2:X} 0x{3:X} 0x{4:X} {5} type:{6:X}", i, networkId, transportId, serviceId,
                            pmtPid, name, serviceType);

              found++;
              ChannelInfo info = new ChannelInfo();
              info.networkID = networkId;
              info.transportStreamID = transportId;
              info.serviceID = serviceId;
              info.majorChannel = majorChannel;
              info.minorChannel = minorChannel;
              info.freq = frequency;
              info.LCN = lcn;
              info.serviceType = serviceType;
              info.modulation = modulation;
              info.service_provider_name = DvbTextConverter.Convert(providerName, "");
              info.service_name = DvbTextConverter.Convert(serviceName, "");
              info.scrambled = (freeCAMode != 0 || hasCaDescriptor != 0);
              info.network_pmt_PID = pmtPid;

              if (IsValidChannel(info, hasAudio, hasVideo))
              {
                if (info.service_name.Length == 0)
                {
                  SetNameForUnknownChannel(channel, info);
                }
                IChannel result = CreateNewChannel(channel, info);
                if (result != null)
                {
                  channelsFound.Add(result);
                }
              }
              else
              {
                Log.Log.Write(
                  "Found Unknown: {0} {1} type:{2} onid:{3:X} tsid:{4:X} sid:{5:X} pmt:{6:X} hasVideo:{7} hasAudio:{8}",
                  info.service_provider_name, info.service_name, info.serviceType, info.networkID,
                  info.transportStreamID, info.serviceID, info.network_pmt_PID, hasVideo, hasAudio);
              }
            }

            Log.Log.Write("Scan Got {0} from {1} channels", found, channelCount);
            return channelsFound;
          }
          finally
          {
            if (_analyzer != null)
            {
              _analyzer.SetCallBack(null);
              _analyzer.Stop();
            }
            _event.Close();
          }
        }
        else
        {
          Log.Log.WriteFile("Scan: no signal detected");
          return new List<IChannel>();
        }
      }
      finally
      {
        _card.IsScanning = false;
      }
    }

    /// <summary>
    /// Filters the pids.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="pids">The pids.</param>
    /// <returns></returns>
    public int FilterPids(short count, IntPtr pids)
    {
      return 0;
    }

    #region IChannelScanCallback Members

    /// <summary>
    /// Called when [scanner done].
    /// </summary>
    /// <returns></returns>
    public int OnScannerDone()
    {
      _event.Set();
      return 0;
    }

    #endregion

    #endregion

    #region NIT scanning

    ///<summary>
    /// Scan NIT channel
    ///</summary>
    ///<param name="channel">Channel</param>
    ///<param name="settings">Scan Parameters</param>
    ///<returns>Found channels</returns>
    public List<IChannel> ScanNIT(IChannel channel, ScanParameters settings)
    {
      try
      {
        _card.IsScanning = true;
        _card.Scan(0, channel);
        _analyzer = GetAnalyzer();
        if (_analyzer == null)
        {
          Log.Log.WriteFile("Scan: no analyzer interface available");
          return new List<IChannel>();
        }
        _analyzer.SetCallBack(null);
        _analyzer.ScanNIT();
        Thread.Sleep(settings.TimeOutTune * 1000);
        ResetSignalUpdate();
        Log.Log.WriteFile("ScanNIT: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel,
                          _card.SignalQuality);
        if (_card.IsTunerLocked || _card.SignalLevel > 0 || _card.SignalQuality > 0)
        {
          int count;

          _event = new ManualResetEvent(false);
          _event.WaitOne(16000, true);
          _event.Close();
          List<IChannel> channelsFound = new List<IChannel>();
          _analyzer.GetNITCount(out count);
          for (int i = 0; i < count; ++i)
          {
            int freq, pol, mod, symbolrate, bandwidth, innerfec, rollOff, chType;
            IntPtr ptrName;
            _analyzer.GetNITChannel((short)i, out chType, out freq, out pol, out mod, out symbolrate, out bandwidth,
                                    out innerfec, out rollOff, out ptrName);
            string name = DvbTextConverter.Convert(ptrName, "");
            if (chType == 0)
            {
              DVBSChannel ch = new DVBSChannel();
              ch.Name = name;
              ch.Frequency = freq;
              Log.Log.Debug("{0},{1},{2},{3}", freq, mod, pol, symbolrate);
              switch (mod)
              {
                default:
                case 0:
                  ch.ModulationType = ModulationType.ModNotSet;
                  break;
                  //case 1: ch.ModulationType = ModulationType.ModQpsk; break;
                case 2:
                  ch.ModulationType = ModulationType.Mod8Psk;
                  break;
                case 3:
                  ch.ModulationType = ModulationType.Mod16Qam;
                  break;
              }
              ch.SymbolRate = symbolrate;
              ch.InnerFecRate = (BinaryConvolutionCodeRate)innerfec;
              ch.Polarisation = (Polarisation)pol;
              ch.Rolloff = (RollOff)rollOff;
              channelsFound.Add(ch);
            }
            else if (chType == 1)
            {
              DVBCChannel ch = new DVBCChannel();
              ch.Name = name;
              ch.Frequency = freq;
              ch.ModulationType = (ModulationType)mod;
              ch.SymbolRate = symbolrate;
              channelsFound.Add(ch);
            }
            else if (chType == 2)
            {
              DVBTChannel ch = new DVBTChannel();
              ch.Name = name;
              ch.Frequency = freq;
              ch.BandWidth = bandwidth;
              channelsFound.Add(ch);
            }
          }
          _analyzer.StopNIT();
          return channelsFound;
        }
        else
        {
          Log.Log.WriteFile("Scan: no signal detected");
          return new List<IChannel>();
        }
      }
      finally
      {
        if (_analyzer != null)
        {
          _analyzer.StopNIT();
        }
        _card.IsScanning = false;
      }
    }

    #endregion

    #region Helpers

    protected virtual bool IsValidChannel(ChannelInfo info, short hasAudio, short hasVideo)
    {
      // DVB/ATSC compliant services will be picked up here.
      if (IsKnownServiceType(info.serviceType))
      {
        return true;
      }

      // The SDT service type is unfortunately not sufficient for service type
      // identification. DVB-IP and some ATSC broadcasters in particular
      // do not comply with specifications. Well, either that, or we do not
      // fully/properly implement the specifications! In any case we need
      // to err on the side of caution and pick up any channels that TsWriter
      // says have video and/or audio streams until we can find a better
      // way to properly identify TV and radio services.
      if (hasVideo != 0)
      {
        info.serviceType = (int)DvbServiceType.DigitalTelevision;
      }
      else if (hasAudio != 0)
      {
        info.serviceType = (int)DvbServiceType.DigitalRadio;
      }
      return IsKnownServiceType(info.serviceType);
    }

    protected virtual bool IsKnownServiceType(int serviceType)
    {
      return IsRadioService(serviceType) || IsTvService(serviceType);
    }

    /// <summary>
    /// Determines whether a DVB service type is a radio service.
    /// </summary>
    /// <param name="serviceType">the service</param>
    /// <returns><c>true</c> if the service type is a radio service type, otherwise <c>false</c></returns>
    protected virtual bool IsRadioService(int serviceType)
    {
      if (
        serviceType == (int)DvbServiceType.DigitalRadio ||
        serviceType == (int)DvbServiceType.FmRadio ||
        serviceType == (int)DvbServiceType.AdvancedCodecDigitalRadio)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determines whether a DVB service type is a television service.
    /// </summary>
    /// <param name="serviceType">the service</param>
    /// <returns><c>true</c> if the service type is a television service type, otherwise <c>false</c></returns>
    protected virtual bool IsTvService(int serviceType)
    {
      if (
        serviceType == (int)DvbServiceType.DigitalTelevision ||
        serviceType == (int)DvbServiceType.Mpeg2HdDigitalTelevision ||
        serviceType == (int)DvbServiceType.AdvancedCodecSdDigitalTelevision ||
        serviceType == (int)DvbServiceType.AdvancedCodecHdDigitalTelevision ||
        serviceType == (int)DvbServiceType.SkyGermanyOptionChannel)
      {
        return true;
      }
      return false;
    }


    protected virtual void SetNameForUnknownChannel(IChannel channel, ChannelInfo info)
    {
      info.service_name = String.Format("Unknown {0:X}", info.serviceID);
    }

    #endregion
  }
}