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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using BroadcastStandard = Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer.BroadcastStandard;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVScanning"/> for MPEG 2
  /// transport streams.
  /// </summary>
  internal class ScannerMpeg2TsBase : IScannerInternal, IChannelScanCallBack
  {
    #region variables

    private ITsChannelScan _analyser;
    protected ITVCard _tuner;
    private ManualResetEvent _event;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="ScannerMpeg2TsBase"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="analyser">The stream analyser instance to use for scanning.</param>
    public ScannerMpeg2TsBase(ITVCard tuner, ITsChannelScan analyser)
    {
      _tuner = tuner;
      _analyser = analyser;
    }

    #endregion

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

    #region IScannerInternal member

    /// <summary>
    /// Set the scanner's _tuner.
    /// </summary>
    public virtual ITVCard Tuner
    {
      set
      {
        _tuner = value;
      }
    }

    #endregion

    #region channel scanning

    /// <summary>
    /// Scans the specified transponder.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    public virtual List<IChannel> Scan(IChannel channel, ScanParameters settings)
    {
      try
      {
        _tuner.IsScanning = true;
        // An exception is thrown here if signal is not locked.
        _tuner.Tune(0, channel);

        this.LogDebug("Scan: tuner locked:{0} signal:{1} quality:{2}", _tuner.IsTunerLocked, _tuner.SignalLevel,
                          _tuner.SignalQuality);

        if (_analyser == null)
        {
          this.LogError("Scan: analyser interface not available, not possible to scan");
          return new List<IChannel>();
        }

        try
        {
          _event = new ManualResetEvent(false);
          _analyser.SetCallBack(this);

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
          _analyser.ScanStream(standard);
          _event.WaitOne(_tuner.Parameters.TimeOutSDT * 1000, true);

          int found = 0;
          int serviceCount;
          _analyser.GetServiceCount(out serviceCount);
          this.LogDebug("Found {0} service(s)...", serviceCount);
          List<IChannel> channelsFound = new List<IChannel>();

          for (int i = 0; i < serviceCount; i++)
          {
            int originalNetworkId;
            int transportStreamId;
            int serviceId;
            IntPtr serviceNamePtr;
            IntPtr providerNamePtr;
            IntPtr logicalChannelNumberPtr;
            int serviceType;
            int videoStreamCount;
            int audioStreamCount;
            bool isHighDefinition;
            bool isEncrypted;
            bool isRunning;
            int pmtPid;
            int previousOriginalNetworkId;
            int previousTransportStreamId;
            int previousServiceId;
            int networkIdCount;
            IntPtr networkIdBuffer;
            int bouquetIdCount;
            IntPtr bouquetIdBuffer;
            int languageCount;
            IntPtr languageBuffer;
            int availableInCellCount;
            IntPtr availableInCellBuffer;
            int unavailableInCellCount;
            IntPtr unavailableInCellBuffer;
            int targetRegionCount;
            IntPtr targetRegionBuffer;
            int availableInCountryCount;
            IntPtr availableInCountryBuffer;
            int unavailableInCountryCount;
            IntPtr unavailableInCountryBuffer;
            _analyser.GetServiceDetail(i,
                          out originalNetworkId, out transportStreamId, out serviceId,
                          out serviceNamePtr, out providerNamePtr, out logicalChannelNumberPtr,
                          out serviceType, out videoStreamCount, out audioStreamCount, out isHighDefinition, out isEncrypted, out isRunning, out pmtPid,
                          out previousOriginalNetworkId, out previousTransportStreamId, out previousServiceId,
                          out networkIdCount, out networkIdBuffer,
                          out bouquetIdCount, out bouquetIdBuffer,
                          out languageCount, out languageBuffer,
                          out availableInCellCount, out availableInCellBuffer, out unavailableInCellCount, out unavailableInCellBuffer,
                          out targetRegionCount, out targetRegionBuffer,
                          out availableInCountryCount, out availableInCountryBuffer, out unavailableInCountryCount, out unavailableInCountryBuffer);

            string serviceName = DvbTextConverter.Convert(serviceNamePtr);
            string providerName = DvbTextConverter.Convert(providerNamePtr);
            string logicalChannelNumber = Marshal.PtrToStringAnsi(logicalChannelNumberPtr);
            this.LogDebug("{0}) {1,-32} provider = {2,-16}, LCN = {3,-7}, ONID = {4,-5}, TSID = {5,-5}, SID = {6,-5}, PMT PID = {7,-5}, previous ONID = {8,-5}, previous TSID = {9,-5}, previous SID = {10,-5}",
                            i + 1, serviceName, providerName, logicalChannelNumber, originalNetworkId, transportStreamId, serviceId, pmtPid, previousOriginalNetworkId, previousTransportStreamId, previousServiceId);
            this.LogDebug("    type = {0}, video stream count = {1}, audio stream count = {2}, is high definition = {3}, is encrypted = {4}, is running = {5}",
                            serviceType, videoStreamCount, audioStreamCount, isHighDefinition, isEncrypted, isRunning);

            List<string> details = new List<string>();
            IntPtr name;
            List<int> networkIds = (List<int>)BufferToList(networkIdBuffer, typeof(int), networkIdCount);
            foreach (int nid in networkIds)
            {
              _analyser.GetNetworkName(nid, out name);
              details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", nid));
            }
            this.LogDebug("    network ID count = {0}, network IDs = {1}", networkIdCount, string.Join(", ", details));

            details.Clear();
            List<int> bouquetIds = (List<int>)BufferToList(bouquetIdBuffer, typeof(int), bouquetIdCount);
            foreach (int bid in bouquetIds)
            {
              _analyser.GetBouquetName(bid, out name);
              details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", bid));
            }
            this.LogDebug("    bouquet ID count = {0}, bouquet IDs = {1}", bouquetIdCount, string.Join(", ", details));

            List<string> languages = (List<string>)LangCodeBufferToList(languageBuffer, languageCount);
            this.LogDebug("    language count = {0}, languages = {1}", languageCount, string.Join(", ", languages));

            List<int> availableInCells = (List<int>)BufferToList(availableInCellBuffer, typeof(int), availableInCellCount);
            this.LogDebug("    available in cells count = {0}, cells = {1}", availableInCellCount, string.Join(", ", availableInCells));
            List<int> unavailableInCells = (List<int>)BufferToList(unavailableInCellBuffer, typeof(int), unavailableInCellCount);
            this.LogDebug("    unavailable in cells count = {0}, cells = {1}", unavailableInCellCount, string.Join(", ", unavailableInCells));

            details.Clear();
            List<long> targetRegionIds = (List<long>)BufferToList(targetRegionBuffer, typeof(long), targetRegionCount);
            foreach (int regionId in targetRegionIds)
            {
              _analyser.GetTargetRegionName(regionId, out name);
              details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", regionId));
            }
            this.LogDebug("    target region count = {0}, regions = {1}", targetRegionCount, string.Join(", ", details));

            List<string> availableInCountries = (List<string>)LangCodeBufferToList(availableInCountryBuffer, availableInCountryCount);
            this.LogDebug("    available in country count = {0}, countries = {1}", availableInCountryCount, string.Join(", ", availableInCountries));
            List<string> unavailableInCountries = (List<string>)LangCodeBufferToList(unavailableInCountryBuffer, unavailableInCountryCount);
            this.LogDebug("    unavailable in country count = {0}, countries = {1}", unavailableInCountryCount, string.Join(", ", unavailableInCountries));

            // The SDT/VCT service type is unfortunately not sufficient for service type identification. Many DVB-IP
            // and some ATSC and North American cable broadcasters in particular do not set the service type.
            serviceType = SetMissingServiceType(serviceType, videoStreamCount, audioStreamCount);

            if (!IsKnownServiceType(serviceType))
            {
              this.LogDebug("Service is not a TV or radio service.");
              continue;
            }
            found++;

            IChannel newChannel = (IChannel)channel.Clone();
            newChannel.Name = serviceName;
            newChannel.FreeToAir = !isEncrypted;

            // Set non-tuning parameters (ie. parameters determined by scanning).
            DVBBaseChannel digitalChannel = channel as DVBBaseChannel;
            if (digitalChannel != null)
            {
              digitalChannel.Provider = providerName;
              digitalChannel.NetworkId = originalNetworkId;
              digitalChannel.TransportId = transportStreamId;
              digitalChannel.ServiceId = serviceId;
              digitalChannel.PmtPid = pmtPid;

              // TODO this case should be moved to a separate ATSC scanner after adding a TsWriter ATSC/SCTE scan interface
              if (standard == BroadcastStandard.Atsc || standard == BroadcastStandard.Scte)
              {
                ATSCChannel newAtscChannel = newChannel as ATSCChannel;
                if (string.IsNullOrEmpty(logicalChannelNumber))
                {
                  newAtscChannel.MajorChannel = newAtscChannel.PhysicalChannel;
                  newAtscChannel.MinorChannel = newAtscChannel.ServiceId;
                }
                else
                {
                  // ATSC x.y LCNs
                  // TODO LCN should be a string in the DB so that we don't have to do this, and then we could remove major and minor channel
                  Match m = Regex.Match(logicalChannelNumber, @"^(\d+)(\.(\d+))?$");
                  if (m.Success)
                  {
                    newAtscChannel.MajorChannel = int.Parse(m.Groups[1].Captures[0].Value);
                    if (m.Groups[2].Captures.Count > 0)
                    {
                      newAtscChannel.MinorChannel = int.Parse(m.Groups[3].Captures[0].Value);
                    }
                    else
                    {
                      newAtscChannel.MinorChannel = 0;
                    }
                  }
                }
                if (newAtscChannel.MajorChannel > 0)
                {
                  if (newAtscChannel.MinorChannel > 0)
                  {
                    digitalChannel.LogicalChannelNumber = (newAtscChannel.MajorChannel * 1000) + newAtscChannel.MinorChannel;
                  }
                  else
                  {
                    digitalChannel.LogicalChannelNumber = newAtscChannel.MajorChannel;
                  }
                }
                else
                {
                  digitalChannel.LogicalChannelNumber = 10000;
                }
              }
              else
              {
                int lcn = 10000;
                if (!string.IsNullOrEmpty(logicalChannelNumber) && int.TryParse(logicalChannelNumber, out lcn))
                {
                  digitalChannel.LogicalChannelNumber = lcn;
                }
                else
                {
                  digitalChannel.LogicalChannelNumber = 10000;
                }
              }
            }

            if (IsTvService(serviceType))
            {
              newChannel.MediaType = MediaTypeEnum.TV;
            }
            else if (IsRadioService(serviceType))
            {
              newChannel.MediaType = MediaTypeEnum.Radio;
            }
            
            if (serviceName.Length == 0)
            {
              SetMissingServiceName(newChannel);
            }
            this.LogDebug("Found: {0}", newChannel);
            channelsFound.Add(newChannel);
          }

          this.LogDebug("Scan found {0} channels from {1} services", found, serviceCount);
          return channelsFound;
        }
        finally
        {
          if (_analyser != null)
          {
            _analyser.SetCallBack(null);
            _analyser.StopStreamScan();
          }
          _event.Close();
        }
      }
      finally
      {
        _tuner.IsScanning = false;
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
        _tuner.IsScanning = true;
        // An exception is thrown here if signal is not locked.
        _tuner.Tune(0, channel);

        if (_analyser == null)
        {
          this.LogError("Scan: analyser interface not available, not possible to scan");
          return new List<IChannel>();
        }

        try
        {
          _event = new ManualResetEvent(false);
          _analyser.SetCallBack(this);
          _analyser.ScanNetwork();

          this.LogDebug("ScanNIT: tuner locked:{0} signal:{1} quality:{2}", _tuner.IsTunerLocked, _tuner.SignalLevel,
                            _tuner.SignalQuality);

          // Start scanning, then wait for TsWriter to tell us that scanning is complete.
          _event.WaitOne(_tuner.Parameters.TimeOutSDT * 1000, true); //TODO: timeout SDT should be "max scan time"

          //TODO: add min scan time

          // Stop scanning. We have to do this explicitly for a network scan in order to merge sets
          // of multiplex tuning details found in different SI tables.
          bool isServiceInfoAvailable = false;
          _analyser.StopNetworkScan(out isServiceInfoAvailable);

          int multiplexCount;
          _analyser.GetMultiplexCount(out multiplexCount);
          this.LogDebug("Found {0} multiplex(es), service information available = {1}...", multiplexCount, isServiceInfoAvailable);

          // Channels found will contain a distinct list of multiplex tuning details.
          List<IChannel> channelsFound = new List<IChannel>();
          // Multiplexes found will contain a dictionary of ONID + TSID => multiplex tuning details.
          Dictionary<uint, IChannel> multiplexesFound = new Dictionary<uint, IChannel>();

          for (int i = 0; i < multiplexCount; ++i)
          {
            int originalNetworkId;
            int transportStreamId;
            int type;   // This is as-per the TVE channel types.
            int frequency;
            int polarisation;
            int modulation;
            int symbolRate;
            int bandwidth;
            int innerFecRate;
            int rollOff;
            int longitude;
            int cellId;
            int cellIdExtension;
            int plpId;
            _analyser.GetMultiplexDetail(i,
                          out originalNetworkId, out transportStreamId, out type,
                          out frequency, out polarisation, out modulation, out symbolRate, out bandwidth, out innerFecRate, out rollOff,
                          out longitude, out cellId, out cellIdExtension, out plpId);

            DVBBaseChannel ch;
            if (type == 2)
            {
              DVBCChannel dvbcChannel = new DVBCChannel();
              dvbcChannel.ModulationType = (ModulationType)modulation;
              dvbcChannel.SymbolRate = symbolRate;
              ch = dvbcChannel;
            }
            else if (type == 3)
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

              // We're missing an all important detail for the channel - the LNB type.
              DVBSChannel currentChannel = channel as DVBSChannel;
              if (currentChannel != null)
              {
                dvbsChannel.LnbType = currentChannel.LnbType.Clone();
              }
              else
              {

                // todo gibman : ILnbType cast will fail, for now, but it will compile
                // why do we need an interface for this ?
                // why not just have the lnbtype changed to the entity type ?                
                dvbsChannel.LnbType = LnbTypeManagement.GetLnbType(1);  // default: universal LNB
              }

              ch = dvbsChannel;
            }
            else if (type == 4)
            {
              DVBTChannel dvbtChannel = new DVBTChannel();
              dvbtChannel.Bandwidth = bandwidth;
              ch = dvbtChannel;
            }
            else
            {
              throw new TvException("ScannerMpeg2TsBase: unsupported channel type " + type + " returned from TsWriter network scan");
            }
            ch.Frequency = frequency;

            bool isUniqueTuning = true;
            foreach (IChannel mux in channelsFound)
            {
              if (mux.Equals(ch))
              {
                isUniqueTuning = false;
                break;
              }
            }
            if (isUniqueTuning)
            {
              channelsFound.Add(ch);
            }

            if (isServiceInfoAvailable)
            {
              uint key = (uint)((uint)originalNetworkId << 16) + (uint)transportStreamId;
              if (multiplexesFound.ContainsKey(key))
              {
                this.LogDebug("Tuning details for ONID {0} and TSID {1} are ambiguous, disregarding service information", originalNetworkId, transportStreamId);
                isServiceInfoAvailable = false;
              }
              else
              {
                multiplexesFound.Add(key, ch);
              }
            }
          }

          // TODO implement support for fast scan channel handling.
          return channelsFound;

          // If service information is not available or the corresponding tuning details are ambiguous then we return
          // a set of multiplex tuning details.
          if (!isServiceInfoAvailable)
          {
            return channelsFound;
          }

          // We're going to attempt to return a set of services.
          int found = 0;
          int serviceCount;
          _analyser.GetServiceCount(out serviceCount);
          this.LogDebug("Found {0} service(s)...", serviceCount);
          List<IChannel> servicesFound = new List<IChannel>();
          for (int i = 0; i < serviceCount; i++)
          {
            int originalNetworkId;
            int transportStreamId;
            int serviceId;
            IntPtr serviceNamePtr;
            IntPtr providerNamePtr;
            IntPtr logicalChannelNumberPtr;
            int serviceType;
            int videoStreamCount;
            int audioStreamCount;
            bool isHighDefinition;
            bool isEncrypted;
            bool isRunning;
            int pmtPid;
            int previousOriginalNetworkId;
            int previousTransportStreamId;
            int previousServiceId;
            int networkIdCount;
            IntPtr networkIdBuffer;
            int bouquetIdCount;
            IntPtr bouquetIdBuffer;
            int languageCount;
            IntPtr languageBuffer;
            int availableInCellCount;
            IntPtr availableInCellBuffer;
            int unavailableInCellCount;
            IntPtr unavailableInCellBuffer;
            int targetRegionCount;
            IntPtr targetRegionBuffer;
            int availableInCountryCount;
            IntPtr availableInCountryBuffer;
            int unavailableInCountryCount;
            IntPtr unavailableInCountryBuffer;
            _analyser.GetServiceDetail(i,
                          out originalNetworkId, out transportStreamId, out serviceId,
                          out serviceNamePtr, out providerNamePtr, out logicalChannelNumberPtr,
                          out serviceType, out videoStreamCount, out audioStreamCount, out isHighDefinition, out isEncrypted, out isRunning, out pmtPid,
                          out previousOriginalNetworkId, out previousTransportStreamId, out previousServiceId,
                          out networkIdCount, out networkIdBuffer,
                          out bouquetIdCount, out bouquetIdBuffer,
                          out languageCount, out languageBuffer,
                          out availableInCellCount, out availableInCellBuffer, out unavailableInCellCount, out unavailableInCellBuffer,
                          out targetRegionCount, out targetRegionBuffer,
                          out availableInCountryCount, out availableInCountryBuffer, out unavailableInCountryCount, out unavailableInCountryBuffer);

            string serviceName = DvbTextConverter.Convert(serviceNamePtr);
            string providerName = DvbTextConverter.Convert(providerNamePtr);
            string logicalChannelNumber = Marshal.PtrToStringAnsi(logicalChannelNumberPtr);
            this.LogDebug("{0}) {1,-32} provider = {2,-16}, LCN = {3,-7}, ONID = {4,-5}, TSID = {5,-5}, SID = {6,-5}, PMT PID = {7,-5}, previous ONID = {8,-5}, previous TSID = {9,-5}, previous SID = {10,-5}",
                            i + 1, serviceName, providerName, logicalChannelNumber, originalNetworkId, transportStreamId, serviceId, pmtPid, previousOriginalNetworkId, previousTransportStreamId, previousServiceId);
            this.LogDebug("    type = {0}, video stream count = {1}, audio stream count = {2}, is high definition = {3}, is encrypted = {4}, is running = {5}",
                            serviceType, videoStreamCount, audioStreamCount, isHighDefinition, isEncrypted, isRunning);

            List<string> details = new List<string>();
            IntPtr name;
            List<int> networkIds = (List<int>)BufferToList(networkIdBuffer, typeof(int), networkIdCount);
            foreach (int nid in networkIds)
            {
              _analyser.GetNetworkName(nid, out name);
              details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", nid));
            }
            this.LogDebug("    network ID count = {0}, network IDs = {1}", networkIdCount, string.Join(", ", details));

            details.Clear();
            List<int> bouquetIds = (List<int>)BufferToList(bouquetIdBuffer, typeof(int), bouquetIdCount);
            foreach (int bid in bouquetIds)
            {
              _analyser.GetBouquetName(bid, out name);
              details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", bid));
            }
            this.LogDebug("    bouquet ID count = {0}, bouquet IDs = {1}", bouquetIdCount, string.Join(", ", details));

            List<string> languages = (List<string>)LangCodeBufferToList(languageBuffer, languageCount);
            this.LogDebug("    language count = {0}, languages = {1}", languageCount, string.Join(", ", languages));

            List<int> availableInCells = (List<int>)BufferToList(availableInCellBuffer, typeof(int), availableInCellCount);
            this.LogDebug("    available in cells count = {0}, cells = {1}", availableInCellCount, string.Join(", ", availableInCells));
            List<int> unavailableInCells = (List<int>)BufferToList(unavailableInCellBuffer, typeof(int), unavailableInCellCount);
            this.LogDebug("    unavailable in cells count = {0}, cells = {1}", unavailableInCellCount, string.Join(", ", unavailableInCells));

            details.Clear();
            List<long> targetRegionIds = (List<long>)BufferToList(targetRegionBuffer, typeof(long), targetRegionCount);
            foreach (int regionId in targetRegionIds)
            {
              _analyser.GetTargetRegionName(regionId, out name);
              details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", regionId));
            }
            this.LogDebug("    target region count = {0}, regions = {1}", targetRegionCount, string.Join(", ", details));

            List<string> availableInCountries = (List<string>)LangCodeBufferToList(availableInCountryBuffer, availableInCountryCount);
            this.LogDebug("    available in country count = {0}, countries = {1}", availableInCountryCount, string.Join(", ", availableInCountries));
            List<string> unavailableInCountries = (List<string>)LangCodeBufferToList(unavailableInCountryBuffer, unavailableInCountryCount);
            this.LogDebug("    unavailable in country count = {0}, countries = {1}", unavailableInCountryCount, string.Join(", ", unavailableInCountries));

            // The SDT/VCT service type is unfortunately not sufficient for service type identification. Many DVB-IP
            // and some ATSC and North American cable broadcasters in particular do not set the service type.
            serviceType = SetMissingServiceType(serviceType, videoStreamCount, audioStreamCount);

            if (!IsKnownServiceType(serviceType))
            {
              this.LogDebug("Service is not a TV or radio service.");
              continue;
            }

            // Find the corresponding multiplex for this service.
            uint key = (uint)((uint)originalNetworkId << 16) + (uint)transportStreamId;
            if (!multiplexesFound.ContainsKey(key))
            {
              this.LogWarn("Discarding service, no multiplex details available.");
              continue;
            }
            found++;

            // If this service comes from another multiplex then we won't know what the PMT PID
            // is. The current value should be set to zero. We set the value to negative one here
            // so that the TV library will determine and set the PMT PID the first time the channel
            // is tuned.
            if (pmtPid == 0)
            {
              pmtPid = -1;
            }

            DVBBaseChannel newChannel = (DVBBaseChannel)multiplexesFound[key].Clone();

            // Set non-tuning parameters (ie. parameters determined by scanning).
            newChannel.Name = serviceName;
            newChannel.Provider = providerName;
            newChannel.NetworkId = originalNetworkId;
            newChannel.TransportId = transportStreamId;
            newChannel.ServiceId = serviceId;
            newChannel.PmtPid = pmtPid;
            if (IsTvService(serviceType))
            {
              newChannel.MediaType = MediaTypeEnum.TV;
            }
            else if (IsRadioService(serviceType))
            {
              newChannel.MediaType = MediaTypeEnum.Radio;
            }
            try
            {
              newChannel.LogicalChannelNumber = int.Parse(logicalChannelNumber); //TODO this won't work for ATSC x.y LCNs. LCN must be a string.
            }
            catch (Exception)
            {
              newChannel.LogicalChannelNumber = 10000;
            }
            newChannel.FreeToAir = !isEncrypted;

            if (serviceName.Length == 0)
            {
              SetMissingServiceName(newChannel);
            }
            this.LogDebug("Found: {0}", newChannel);
            servicesFound.Add(newChannel);
          }

          this.LogDebug("Scan found {0} channels from {1} services", found, serviceCount);
          return servicesFound;
        }
        finally
        {
          if (_analyser != null)
          {
            _analyser.SetCallBack(null);
          }
          _event.Close();
        }
      }
      finally
      {
        _tuner.IsScanning = false;
      }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Set the service type for services which do not supply a service type.
    /// </summary>
    /// <param name="serviceType">The service type to check/update.</param>
    /// <param name="videoStreamCount">The number of video streams associated with the service.</param>
    /// <param name="audioStreamCount">The number of audio streams associated with the service.</param>
    /// <returns>the updated service type</returns>
    protected virtual int SetMissingServiceType(int serviceType, int videoStreamCount, int audioStreamCount)
    {
      if (serviceType <= 0)
      {
        if (videoStreamCount != 0)
        {
          return (int)DvbServiceType.DigitalTelevision;
        }
        else if (audioStreamCount != 0)
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
    /// Known service types are the types that the TV Engine is able to manage. At present only
    /// television and radio service types are supported.
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
        serviceType == (int)DvbServiceType.AdvancedCodecFrameCompatiblePlanoStereoscopicHdDigitalTelevision ||
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

    /// <summary>
    /// Read the elements from a buffer into a list.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="elementType">The type of the elements contained in the buffer.</param>
    /// <param name="elementCount">The number of elements in the buffer.</param>
    /// <returns>a list containing the elements from the buffer</returns>
    private IList BufferToList(IntPtr buffer, Type elementType, int elementCount)
    {
      Type customListType = typeof(List<>).MakeGenericType(elementType);
      IList toReturn = (IList)Activator.CreateInstance(customListType);
      int size = Marshal.SizeOf(elementType);
      int offset = 0;
      for (int i = 0; i < elementCount; i++)
      {
        toReturn.Add(Marshal.PtrToStructure(IntPtr.Add(buffer, offset), elementType));
        offset += size;
      }
      return toReturn;
    }
    private IList LangCodeBufferToList(IntPtr buffer, int elementCount)
    {
      IList toReturn = new List<string>();
      int offset = 0;
      for (int i = 0; i < elementCount; i++)
      {
        toReturn.Add(Marshal.PtrToStringAnsi(IntPtr.Add(buffer, offset), 3));
        offset += 4;
      }
      return toReturn;
    }

    #endregion
  }
}