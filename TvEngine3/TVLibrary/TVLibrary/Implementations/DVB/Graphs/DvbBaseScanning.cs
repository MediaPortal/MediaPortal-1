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
using System.Runtime.InteropServices;
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
  public class DvbBaseScanning : IChannelScanCallBack, ITVScanning
  {
    #region variables

    private ITsChannelScan _analyzer;
    private readonly TvCardDvbBase _card;
    private ManualResetEvent _event;

    #endregion

    #region ctor

    /// <summary>
    /// Initialise a new instance of the <see cref="DvbBaseScanning"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    public DvbBaseScanning(TvCardDvbBase tuner)
    {
      _card = tuner;
    }

    #endregion

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset() {}

    #region IChannelScanCallBack member

    /// <summary>
    /// Called by TsWriter when all available service and/or network information has been received.
    /// </summary>
    /// <returns>an HRESULT indicating whether the notification was successfully handled</returns>
    public int OnScannerDone()
    {
      _event.Set();
      return 0; // success
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

          // Determine the broadcast standard that the stream conforms to.
          BroadcastStandard standard = BroadcastStandard.Dvb; // default
          ATSCChannel atscChannel = channel as ATSCChannel;
          if (atscChannel != null)
          {
            if (atscChannel.ModulationType == ModulationType.Mod8Vsb || atscChannel.ModulationType == ModulationType.Mod16Vsb)
            {
              standard = BroadcastStandard.Atsc;
            }
            else
            {
              standard = BroadcastStandard.Scte;
            }
          }

          // Start scanning, then wait for TsWriter to tell us that scanning is complete.
          _analyzer.ScanStream(standard);
          _event.WaitOne(settings.TimeOutSDT * 1000, true);

          int found = 0;
          int serviceCount;
          _analyzer.GetServiceCount(out serviceCount);
          Log.Log.Write("Found {0} service(s)...", serviceCount);
          List<IChannel> channelsFound = new List<IChannel>();

          for (int i = 0; i < serviceCount; i++)
          {
            int networkId;
            int transportStreamId;
            int serviceId;
            IntPtr serviceNamePtr;
            IntPtr providerNamePtr;
            IntPtr networkNamesPtr;
            IntPtr logicalChannelNumberPtr;
            int serviceType;
            int hasVideo;
            int hasAudio;
            bool isEncrypted;
            int pmtPid;
            _analyzer.GetServiceDetail(i,
                          out networkId, out transportStreamId, out serviceId,
                          out serviceNamePtr, out providerNamePtr, out networkNamesPtr, out logicalChannelNumberPtr,
                          out serviceType, out hasVideo, out hasAudio, out isEncrypted, out pmtPid);

            string serviceName = DvbTextConverter.Convert(serviceNamePtr, "");
            string providerName = DvbTextConverter.Convert(providerNamePtr, "");
            DVB_MMI.DumpBinary(networkNamesPtr, 0, Marshal.SizeOf(networkNamesPtr));
            string logicalChannelNumber = Marshal.PtrToStringAnsi(logicalChannelNumberPtr);
            Log.Log.Write("{0}) {1,-32} provider = {2,-16}, LCN = {3,-7}, NID = 0x{4:4x}, TSID = 0x{5:4x}, SID = 0x{6:4x}, type = {7}, has video = {8}, has audio = {9}, is encrypted = {10}, PMT PID = 0x{11:4x}",
                            i + 1, serviceName, providerName, logicalChannelNumber, networkId, transportStreamId, serviceId,
                            serviceType, hasVideo, hasAudio, isEncrypted, pmtPid);

            // The SDT/VCT service type is unfortunately not sufficient for service type identification. Many DVB-IP
            // and some ATSC and North American cable broadcasters in particular do not set the service type.
            serviceType = SetMissingServiceType(serviceType, hasVideo, hasAudio);

            if (!IsKnownServiceType(serviceType))
            {
              Log.Log.Write("Service is not a TV or radio service.");
              continue;
            }
            found++;

            DVBBaseChannel newChannel = (DVBBaseChannel)channel.Clone();

            // Set non-tuning parameters (ie. parameters determined by scanning).
            newChannel.Name = serviceName;
            newChannel.Provider = providerName;
            newChannel.NetworkId = networkId;
            newChannel.TransportId = transportStreamId;
            newChannel.ServiceId = serviceId;
            newChannel.PmtPid = pmtPid;
            newChannel.IsTv = IsTvService(serviceType);
            newChannel.IsRadio = IsRadioService(serviceType);
            newChannel.LogicalChannelNumber = Int32.Parse(logicalChannelNumber); //TODO this won't work for ATSC x.y LCNs. LCN must be a string.
            newChannel.FreeToAir = !isEncrypted;

            if (serviceName.Length == 0)
            {
              SetMissingServiceName(newChannel);
            }
            Log.Log.Write("Found: {0}", newChannel);
            channelsFound.Add(newChannel);
          }

          Log.Log.Write("Scan found {0} channels from {1} services", found, serviceCount);
          return channelsFound;
        }
        finally
        {
          if (_analyzer != null)
          {
            _analyzer.SetCallBack(null);
            _analyzer.StopStreamScan();
          }
          _event.Close();
        }
      }
      finally
      {
        _card.IsScanning = false;
      }
    }

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

        try
        {
          _event = new ManualResetEvent(false);
          _analyzer.SetCallBack(this);
          _analyzer.ScanNetwork();

          Log.Log.WriteFile("ScanNIT: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel,
                            _card.SignalQuality);

          // Start scanning, then wait for TsWriter to tell us that scanning is complete.
          _event = new ManualResetEvent(false);
          _event.WaitOne(settings.TimeOutSDT * 1000, true); //TODO: timeout SDT should be "max scan time"

          //TODO: add min scan time

          // Stop scanning. We have to do this explicitly for a network scan in order to merge sets
          // of multiplex tuning details found in different SI tables.
          bool isServiceInfoAvailable = false;
          _analyzer.StopNetworkScan(out isServiceInfoAvailable);

          int multiplexCount;
          _analyzer.GetMultiplexCount(out multiplexCount);
          Log.Log.Write("Found {0} multiplex(es), service information available = {1}...", multiplexCount, isServiceInfoAvailable);
          List<IChannel> channelsFound = new List<IChannel>();
          Dictionary<uint, IChannel> multiplexesFound = new Dictionary<uint, IChannel>();

          for (int i = 0; i < multiplexCount; ++i)
          {
            int networkId;
            int transportStreamId;
            int type;
            int frequency;
            int polarisation;
            int modulation;
            int symbolRate;
            int bandwidth;
            int innerFecRate;
            int rollOff;
            _analyzer.GetMultiplexDetail(i,
                          out networkId, out transportStreamId, out type,
                          out frequency, out polarisation, out modulation, out symbolRate, out bandwidth, out innerFecRate, out rollOff);

            DVBBaseChannel ch;
            if (type == 0)  //TODO switch to DB types, and create an enum of channel types
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
            else if (type == 1)
            {
              DVBCChannel dvbcChannel = new DVBCChannel();
              dvbcChannel.ModulationType = (ModulationType)modulation;
              dvbcChannel.SymbolRate = symbolRate;
              ch = dvbcChannel;
            }
            else if (type == 2)
            {
              DVBTChannel dvbtChannel = new DVBTChannel();
              dvbtChannel.Bandwidth = bandwidth;
              ch = dvbtChannel;
            }
            else
            {
              throw new TvException("DvbBaseScanning: unsupported channel type " + type + " returned from TsWriter network scan");
            }
            ch.Frequency = frequency;

            channelsFound.Add(ch);
            uint key = (uint)((uint)networkId << 16) + (uint)transportStreamId;
            if (multiplexesFound.ContainsKey(key))
            {
              Log.Log.WriteFile("Tuning details for NID 0x{0:x} and TSID 0x{1:x} are ambiguous, disregarding service information", networkId, transportStreamId);
              isServiceInfoAvailable = false;
            }
            else
            {
              multiplexesFound.Add(key, ch);
            }
          }

          // If service information is not available or the corresponding tuning details are ambiguous then we return
          // a set of multiplex tuning details.
          if (!isServiceInfoAvailable)
          {
            return channelsFound;
          }

          // We're going to attempt to return a set of services.
          int found = 0;
          int serviceCount;
          _analyzer.GetServiceCount(out serviceCount);
          Log.Log.Write("Found {0} service(s)...", serviceCount);
          List<IChannel> servicesFound = new List<IChannel>();
          for (int i = 0; i < serviceCount; i++)
          {
            int networkId;
            int transportStreamId;
            int serviceId;
            IntPtr serviceNamePtr;
            IntPtr providerNamePtr;
            IntPtr networkNamesPtr;
            IntPtr logicalChannelNumberPtr;
            int serviceType;
            int hasVideo;
            int hasAudio;
            bool isEncrypted;
            int pmtPid;
            _analyzer.GetServiceDetail(i,
                          out networkId, out transportStreamId, out serviceId,
                          out serviceNamePtr, out providerNamePtr, out networkNamesPtr, out logicalChannelNumberPtr,
                          out serviceType, out hasVideo, out hasAudio, out isEncrypted, out pmtPid);

            string serviceName = DvbTextConverter.Convert(serviceNamePtr, "");
            string providerName = DvbTextConverter.Convert(providerNamePtr, "");
            DVB_MMI.DumpBinary(networkNamesPtr, 0, Marshal.SizeOf(networkNamesPtr));
            string logicalChannelNumber = Marshal.PtrToStringAnsi(logicalChannelNumberPtr);
            Log.Log.Write("{0}) {1,-32} provider = {2,-16}, LCN = {3,-7}, NID = 0x{4:4x}, TSID = 0x{5:4x}, SID = 0x{6:4x}, type = {7}, has video = {8}, has audio = {9}, is encrypted = {10}, PMT PID = 0x{11:4x}",
                            i + 1, serviceName, providerName, logicalChannelNumber, networkId, transportStreamId, serviceId,
                            serviceType, hasVideo, hasAudio, isEncrypted, pmtPid);

            // The SDT/VCT service type is unfortunately not sufficient for service type identification. Many DVB-IP
            // and some ATSC and North American cable broadcasters in particular do not set the service type.
            serviceType = SetMissingServiceType(serviceType, hasVideo, hasAudio);

            if (!IsKnownServiceType(serviceType))
            {
              Log.Log.Write("Service is not a TV or radio service.");
              continue;
            }

            // Find the corresponding multiplex for this service.
            uint key = (uint)((uint)networkId << 16) + (uint)transportStreamId;
            if (!multiplexesFound.ContainsKey(key))
            {
              Log.Log.Write("Discarding service, no multiplex details available.");
              continue;
            }
            found++;

            DVBBaseChannel newChannel = (DVBBaseChannel)multiplexesFound[key].Clone();

            // Set non-tuning parameters (ie. parameters determined by scanning).
            newChannel.Name = serviceName;
            newChannel.Provider = providerName;
            newChannel.NetworkId = networkId;
            newChannel.TransportId = transportStreamId;
            newChannel.ServiceId = serviceId;
            newChannel.PmtPid = pmtPid;
            newChannel.IsTv = IsTvService(serviceType);
            newChannel.IsRadio = IsRadioService(serviceType);
            newChannel.LogicalChannelNumber = Int32.Parse(logicalChannelNumber); //TODO this won't work for ATSC x.y LCNs. LCN must be a string.
            newChannel.FreeToAir = !isEncrypted;

            if (serviceName.Length == 0)
            {
              SetMissingServiceName(newChannel);
            }
            Log.Log.Write("Found: {0}", newChannel);
            servicesFound.Add(newChannel);
          }

          Log.Log.Write("Scan found {0} channels from {1} services", found, serviceCount);
          return servicesFound;
        }
        finally
        {
          if (_analyzer != null)
          {
            _analyzer.SetCallBack(null);
          }
          _event.Close();
        }
      }
      finally
      {
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
    protected virtual int SetMissingServiceType(int serviceType, int hasVideo, int hasAudio)
    {
      if (serviceType <= 0)
      {
        if (hasVideo != 0)
        {
          return (int)DvbServiceType.DigitalTelevision;
        }
        else if (hasAudio != 0)
        {
          return (int)DvbServiceType.DigitalRadio;
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