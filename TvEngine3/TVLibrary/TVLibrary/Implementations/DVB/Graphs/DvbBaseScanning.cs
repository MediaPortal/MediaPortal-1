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
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A base class which implements TV and radio service scanning for digital tuners with BDA drivers.
  /// </summary>
  public class DvbBaseScanning : IChannelScanCallback, ITVScanning
  {
    #region variables

    private ITsChannelScan _analyzer;
    private readonly TvCardDvbBase _card;
    private ManualResetEvent _event;

    /// <summary>
    /// Enable wait for VCT indicator. VCT is the virtual channel table, only relevant for ATSC/QAM streams.
    /// </summary>
    protected bool _enableWaitForVct;

    #endregion

    #region ctor

    /// <summary>
    /// Initialise a new instance of the <see cref="DvbBaseScanning"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    public DvbBaseScanning(TvCardDvbBase tuner)
    {
      _card = tuner;
      _enableWaitForVct = false;
    }

    #endregion

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset() {}

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
        // An exception is thrown here if signal is not locked.
        _card.Scan(0, channel);

        Log.Log.WriteFile("Scan: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel,
                          _card.SignalQuality);

        _analyzer = _card.StreamAnalyzer;
        if (_analyzer == null)
        {
          Log.Log.WriteFile("Scan: no analyzer interface available");
          return new List<IChannel>();
        }

        try
        {
          _event = new ManualResetEvent(false);
          _analyzer.SetCallBack(this);
          _analyzer.Start(_enableWaitForVct);
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
            IntPtr providerNamePtr;
            IntPtr serviceNamePtr;
            short pmtPid;
            short hasVideo;
            short hasAudio;
            short hasCaDescriptor;
            short lcn;
            _analyzer.GetChannel((short)i,
                                  out networkId, out transportId, out serviceId, out majorChannel, out minorChannel,
                                  out frequency, out lcn, out freeCAMode, out serviceType, out modulation,
                                  out providerNamePtr, out serviceNamePtr,
                                  out pmtPid, out hasVideo, out hasAudio, out hasCaDescriptor);

            string serviceName = DvbTextConverter.Convert(serviceNamePtr, "");
            string providerName = DvbTextConverter.Convert(providerNamePtr, "");
            Log.Log.Write("{0}) 0x{1:X} 0x{2:X} 0x{3:X} 0x{4:X} {5} type:{6:X}", i + 1, networkId, transportId, serviceId,
                          pmtPid, serviceName, serviceType);

            found++;

            // The SDT service type is unfortunately not sufficient for service type identification. Many DVB-IP
            // and some ATSC and annex-C broadcasters in particular do not comply with specifications. Well, either
            // that, or we do not fully/properly implement the specifications (which is true for DVB-IP at present)!
            // In any case we want to err on the side of caution and pick up any channels that TsWriter says have
            // video and/or audio streams until we can find a better way to properly identify TV and radio services.
            serviceType = SetMissingServiceType(serviceType, hasVideo, hasAudio);

            if (!IsKnownServiceType(serviceType))
            {
              Log.Log.Write(
                "Found Unknown: {0} {1} type:{2} onid:{3:X} tsid:{4:X} sid:{5:X} pmt:{6:X} hasVideo:{7} hasAudio:{8}",
                providerName, serviceName, serviceType, networkId, transportId, serviceId, pmtPid, hasVideo, hasAudio
              );
              continue;
            }

            DVBBaseChannel newChannel = (DVBBaseChannel)channel.Clone();

            // Set non-tuning parameters (ie. parameters determined by scanning).
            newChannel.Name = serviceName;
            newChannel.Provider = providerName;
            newChannel.NetworkId = networkId;
            newChannel.TransportId = transportId;
            newChannel.ServiceId = serviceId;
            newChannel.PmtPid = pmtPid;
            newChannel.IsTv = IsTvService(serviceType);
            newChannel.IsRadio = IsRadioService(serviceType);
            newChannel.LogicalChannelNumber = lcn;
            newChannel.FreeToAir = (freeCAMode == 0 && hasCaDescriptor == 0);

            // There are a couple of ATSC-specific parameters that are not tuning parameters and not
            // available as properties on DVBBaseChannel...
            ATSCChannel atscChannel = newChannel as ATSCChannel;
            if (atscChannel != null)
            {
              atscChannel.MajorChannel = majorChannel;
              atscChannel.MinorChannel = minorChannel;
              newChannel = atscChannel;
            }

            if (serviceName.Length == 0)
            {
              SetMissingServiceName(newChannel);
            }
            Log.Log.Write("Found: {0}", newChannel);
            channelsFound.Add(newChannel);
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
      finally
      {
        _card.IsScanning = false;
      }
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
        // An exception is thrown here if signal is not locked.
        _card.Scan(0, channel);

        _analyzer = _card.StreamAnalyzer;
        if (_analyzer == null)
        {
          Log.Log.WriteFile("Scan: no analyzer interface available");
          return new List<IChannel>();
        }
        _analyzer.SetCallBack(null);
        _analyzer.ScanNIT();

        Log.Log.WriteFile("ScanNIT: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel,
                          _card.SignalQuality);

        // Wait for TsWriter to find all the NIT frequencies.
        // TODO: this should *definitely* not be hard-coded!
        _event = new ManualResetEvent(false);
        _event.WaitOne(16000, true);
        _event.Close();

        List<IChannel> channelsFound = new List<IChannel>();
        int count;
        _analyzer.GetNITCount(out count);
        for (int i = 0; i < count; ++i)
        {
          int frequency, polarisation, modulation, symbolRate, bandwidth, innerFecRate, rollOff, chType;
          IntPtr ptrName;
          _analyzer.GetNITChannel((short)i, out chType, out frequency, out polarisation, out modulation, out symbolRate, out bandwidth,
                                  out innerFecRate, out rollOff, out ptrName);
          string name = DvbTextConverter.Convert(ptrName, "");
          DVBBaseChannel ch;
          if (chType == 0)
          {
            DVBSChannel dvbsChannel = new DVBSChannel();
            dvbsChannel.RollOff = (RollOff)rollOff;
            dvbsChannel.ModulationType = ModulationType.ModNotSet;
            switch (modulation)
            {
              case 1:
                // Modulation not set indicates DVB-S; QPSK is DVB-S2 QPSK.
                if (dvbsChannel.RollOff != RollOff.NotSet)
                {
                  dvbsChannel.ModulationType = ModulationType.ModQpsk;
                }
                break;
              case 2:
                dvbsChannel.ModulationType = ModulationType.Mod8Psk;
                break;
              case 3:
                dvbsChannel.ModulationType = ModulationType.Mod16Qam;
                break;
            }
            dvbsChannel.SymbolRate = symbolRate;
            dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate)innerFecRate;
            dvbsChannel.Polarisation = (Polarisation)polarisation;
            ch = dvbsChannel;
          }
          else if (chType == 1)
          {
            DVBCChannel dvbcChannel = new DVBCChannel();
            dvbcChannel.ModulationType = (ModulationType)modulation;
            dvbcChannel.SymbolRate = symbolRate;
            ch = dvbcChannel;
          }
          else if (chType == 2)
          {
            DVBTChannel dvbtChannel = new DVBTChannel();
            dvbtChannel.Bandwidth = bandwidth;
            ch = dvbtChannel;
          }
          else
          {
            throw new TvException("DvbBaseScanning: unsupported channel type " + chType + " returned by TsWriter NIT scan");
          }
          ch.Name = name;
          ch.Frequency = frequency;
          channelsFound.Add(ch);
        }
        _analyzer.StopNIT();
        return channelsFound;
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

    /// <summary>
    /// Set the service type for services which do not supply a service type.
    /// </summary>
    /// <param name="serviceType">The service type to check/update.</param>
    /// <param name="hasVideo">Non-zero if the corresponding service has at least one video stream.</param>
    /// <param name="hasAudio">Non-zero if the corresponding service has at least one audio stream.</param>
    /// <returns>the updated service type</returns>
    protected virtual short SetMissingServiceType(short serviceType, short hasVideo, short hasAudio)
    {
      if (serviceType <= 0)
      {
        if (hasVideo != 0)
        {
          return (short)DvbServiceType.DigitalTelevision;
        }
        else if (hasAudio != 0)
        {
          return (short)DvbServiceType.DigitalRadio;
        }
      }
      return serviceType;
    }

    /// <summary>
    /// Determine whether a service type is a known service type.
    /// </summary>
    /// <remarks>
    /// Known service types are the types that TV Server is able to manage. At present only television and
    /// radio service types are supported.
    /// </remarks>
    /// <param name="serviceType">The service type to check.</param>
    /// <returns><c>true</c> if the service type is a known service type, otherwise <c>false</c></returns>
    protected virtual bool IsKnownServiceType(int serviceType)
    {
      return IsRadioService(serviceType) || IsTvService(serviceType);
    }

    /// <summary>
    /// Determine whether a service type is a radio service type.
    /// </summary>
    /// <param name="serviceType">The service type to check.</param>
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
    /// Determine whether a service type is a television service type.
    /// </summary>
    /// <param name="serviceType">The service type to check.</param>
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

    /// <summary>
    /// Set the name for services which do not supply a name.
    /// </summary>
    /// <param name="channel">The service details.</param>
    protected virtual void SetMissingServiceName(IChannel channel)
    {
      DVBBaseChannel dvbChannel = channel as DVBBaseChannel;
      if (dvbChannel == null)
      {
        return;
      }
      // Default: use "Unknown <frequency>-<service ID>". At least that way people can often tell which transponder
      // the service came from.
      dvbChannel.Name = "Unknown " + (dvbChannel.Frequency / 1000) + "-" + dvbChannel.ServiceId;
    }

    #endregion
  }
}