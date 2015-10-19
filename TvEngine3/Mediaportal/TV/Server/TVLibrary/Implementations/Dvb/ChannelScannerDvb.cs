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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb
{
  /// <summary>
  /// An implementation of <see cref="IChannelScanner"/> for DVB-compliant
  /// transport streams.
  /// </summary>
  internal class ChannelScannerDvb : IChannelScannerInternal, ICallBackGrabber
  {
    #region variables

    private bool _isScanning = false;
    private int _scanTimeLimit = 20000;   // unit = milli-seconds
    private IChannelScannerHelper _scanHelper = null;
    private IGrabberSiDvb _analyser = null;
    protected ITuner _tuner = null;
    private ManualResetEvent _event = null;
    private volatile bool _cancelScan = false;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="ChannelScannerDirectShowBase"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="helper">The helper to use for channel logic.</param>
    /// <param name="analyser">The stream analyser instance to use for scanning.</param>
    public ChannelScannerDvb(ITuner tuner, IChannelScannerHelper helper, IGrabberSiDvb analyser)
    {
      _tuner = tuner;
      _scanHelper = helper;
      _analyser = analyser;
    }

    #endregion

    #region IChannelScannerInternal members

    /// <summary>
    /// Set the scanner's tuner.
    /// </summary>
    public ITuner Tuner
    {
      set
      {
        _tuner = value;
      }
    }

    /// <summary>
    /// Set the scanner's helper.
    /// </summary>
    public IChannelScannerHelper Helper
    {
      set
      {
        _scanHelper = value;
      }
    }

    #endregion

    #region channel scanning

    /// <summary>
    /// Reload the scanner's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("scan: reload configuration");
      _scanTimeLimit = SettingsManagement.GetValue("timeLimitScan", 20000);
    }

    /// <summary>
    /// Get the scanner's current status.
    /// </summary>
    /// <value><c>true</c> if the scanner is scanning, otherwise <c>false</c></value>
    public bool IsScanning
    {
      get
      {
        return _isScanning;
      }
    }

    /// <summary>
    /// Abort scanning for channels.
    /// </summary>
    public void AbortScanning()
    {
      this.LogInfo("scan: abort");
      _cancelScan = true;
      try
      {
        if (_tuner != null)
        {
          _tuner.CancelTune(0);
        }
        if (_event != null)
        {
          _event.Set();
        }
      }
      catch
      {
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for channel information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the channel information found</returns>
    public virtual List<IChannel> Scan(IChannel channel)
    {
      try
      {
        _isScanning = true;
        // An exception is thrown here if signal is not locked.
        _tuner.Tune(0, channel);
        if (_analyser == null)
        {
          this.LogError("scan: analyser interface not available, not possible to scan");
          return new List<IChannel>();
        }

        try
        {
          _event = new ManualResetEvent(false);
          _analyser.SetCallBack(this);

          // Start scanning, then wait for TsWriter to tell us that scanning is complete.
          _analyser.ScanStream(format);
          _event.WaitOne(_scanTimeLimit, true);

          int found = 0;
          int serviceCount;
          _analyser.GetServiceCount(out serviceCount);
          this.LogDebug("Found {0} service(s)...", serviceCount);
          List<IChannel> channelsFound = new List<IChannel>();

          for (int i = 0; i < serviceCount; i++)
          {
            if (_cancelScan)
            {
              return new List<IChannel>();
            }
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
            int networkIdCount = 10;
            ushort[] networkIds = new ushort[10];
            int bouquetIdCount = 20;
            ushort[] bouquetIds = new ushort[20];
            int languageCount = 10;
            Iso639Code[] languages = new Iso639Code[10];
            int availableInCellCount = 30;
            uint[] availableInCells = new uint[30];
            int unavailableInCellCount = 30;
            uint[] unavailableInCells = new uint[30];
            int targetRegionCount = 30;
            long[] targetRegionIds = new long[30];
            int availableInCountryCount = 10;
            Iso639Code[] availableInCountries = new Iso639Code[10];
            int unavailableInCountryCount = 10;
            Iso639Code[] unavailableInCountries = new Iso639Code[10];
            _analyser.GetServiceDetail(i,
                          out originalNetworkId, out transportStreamId, out serviceId,
                          out serviceNamePtr, out providerNamePtr, out logicalChannelNumberPtr,
                          out serviceType, out videoStreamCount, out audioStreamCount, out isHighDefinition, out isEncrypted, out isRunning, out pmtPid,
                          out previousOriginalNetworkId, out previousTransportStreamId, out previousServiceId,
                          ref networkIdCount, ref networkIds,
                          ref bouquetIdCount, ref bouquetIds,
                          ref languageCount, ref languages,
                          ref availableInCellCount, ref availableInCells, ref unavailableInCellCount, ref unavailableInCells,
                          ref targetRegionCount, ref targetRegionIds,
                          ref availableInCountryCount, ref availableInCountries, ref unavailableInCountryCount, ref unavailableInCountries);

            string serviceName = DvbTextConverter.Convert(serviceNamePtr).Trim();
            string providerName = DvbTextConverter.Convert(providerNamePtr);
            string logicalChannelNumber = Marshal.PtrToStringAnsi(logicalChannelNumberPtr);
            this.LogDebug("{0}) {1,-32} provider = {2,-16}, LCN = {3,-7}, ONID = {4,-5}, TSID = {5,-5}, SID = {6,-5}, PMT PID = {7,-5}, previous ONID = {8,-5}, previous TSID = {9,-5}, previous SID = {10,-5}",
                            i + 1, serviceName, providerName, logicalChannelNumber, originalNetworkId, transportStreamId, serviceId, pmtPid, previousOriginalNetworkId, previousTransportStreamId, previousServiceId);
            this.LogDebug("    type = {0}, video stream count = {1}, audio stream count = {2}, is high definition = {3}, is encrypted = {4}, is running = {5}",
                            serviceType, videoStreamCount, audioStreamCount, isHighDefinition, isEncrypted, isRunning);

            List<string> details = new List<string>();
            IntPtr name;
            if (networkIds != null)
            {
              foreach (int nid in networkIds)
              {
                _analyser.GetNetworkName(nid, out name);
                details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", nid));
              }
            }
            this.LogDebug("    network ID count = {0}, network IDs = {1}", networkIdCount, string.Join(", ", details));

            details.Clear();
            if (bouquetIds != null)
            {
              foreach (int bid in bouquetIds)
              {
                _analyser.GetBouquetName(bid, out name);
                details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", bid));
              }
            }
            this.LogDebug("    bouquet ID count = {0}, bouquet IDs = {1}", bouquetIdCount, string.Join(", ", details));

            this.LogDebug("    language count = {0}, languages = {1}", languageCount, string.Join(", ", languages ?? new Iso639Code[0]));
            this.LogDebug("    available in cells count = {0}, cells = {1}", availableInCellCount, string.Join(", ", availableInCells ?? new uint[0]));
            this.LogDebug("    unavailable in cells count = {0}, cells = {1}", unavailableInCellCount, string.Join(", ", unavailableInCells ?? new uint[0]));

            details.Clear();
            if (targetRegionIds != null)
            {
              foreach (int regionId in targetRegionIds)
              {
                _analyser.GetTargetRegionName(regionId, out name);
                details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", regionId));
              }
            }
            this.LogDebug("    target region count = {0}, regions = {1}", targetRegionCount, string.Join(", ", details));

            this.LogDebug("    available in country count = {0}, countries = {1}", availableInCountryCount, string.Join(", ", availableInCountries ?? new Iso639Code[0]));
            this.LogDebug("    unavailable in country count = {0}, countries = {1}", unavailableInCountryCount, string.Join(", ", unavailableInCountries ?? new Iso639Code[0]));

            // The SDT/VCT service type is unfortunately not sufficient for
            // service type identification. Many DVB-IP and some ATSC and North
            // American cable broadcasters in particular do not set the service
            // type.
            MediaType? mediaType = _scanHelper.GetMediaType(serviceType, videoStreamCount, audioStreamCount);
            if (!mediaType.HasValue)
            {
              this.LogDebug("Service type is not supported.");
              continue;
            }
            found++;

            IChannel newChannel = (IChannel)channel.Clone();
            newChannel.Name = serviceName;
            newChannel.Provider = providerName;
            newChannel.MediaType = mediaType.Value;
            newChannel.IsEncrypted = isEncrypted;
            newChannel.LogicalChannelNumber = logicalChannelNumber;

            // Set non-tuning parameters (ie. parameters determined by scanning).
            ChannelMpeg2Base mpeg2Channel = newChannel as ChannelMpeg2Base;
            if (mpeg2Channel != null)
            {
              mpeg2Channel.TransportStreamId = transportStreamId;
              mpeg2Channel.ProgramNumber = serviceId;
              mpeg2Channel.PmtPid = pmtPid;
            }
            ChannelDvbBase dvbChannel = newChannel as ChannelDvbBase;
            if (dvbChannel != null)
            {
              dvbChannel.OriginalNetworkId = originalNetworkId;
            }
            // TODO remove this hacky code!!!
            ChannelAtsc atscChannel = newChannel as ChannelAtsc;
            if (atscChannel != null)
            {
              atscChannel.SourceId = originalNetworkId;
            }
            ChannelScte scteChannel = newChannel as ChannelScte;
            if (scteChannel != null)
            {
              scteChannel.SourceId = originalNetworkId;
            }

            _scanHelper.UpdateChannel(ref newChannel);
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
          _event = null;
        }
      }
      finally
      {
        _isScanning = false;
      }
    }

    /// <summary>
    /// Tune to a specified channel and scan for network information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the network information found</returns>
    public List<TuningDetail> ScanNIT(IChannel channel)
    {
      try
      {
        _isScanning = true;
        // An exception is thrown here if signal is not locked.
        _tuner.Tune(0, channel);

        if (_analyser == null)
        {
          this.LogError("scan NIT: analyser interface not available, not possible to scan");
          return new List<TuningDetail>();
        }

        try
        {
          _event = new ManualResetEvent(false);
          _analyser.SetCallBack(this);
          _analyser.ScanNetwork();

          // Start scanning, then wait for TsWriter to tell us that scanning is complete.
          _event.WaitOne(_scanTimeLimit, true); //TODO: timeout SDT should be "max scan time"

          //TODO: add min scan time

          // Stop scanning. We have to do this explicitly for a network scan in order to merge sets
          // of multiplex tuning details found in different SI tables.
          bool isServiceInfoAvailable = false;
          _analyser.StopNetworkScan(out isServiceInfoAvailable);

          int multiplexCount;
          _analyser.GetMultiplexCount(out multiplexCount);
          this.LogDebug("Found {0} multiplex(es), service information available = {1}...", multiplexCount, isServiceInfoAvailable);

          // This list will contain a distinct list of transmitter tuning details.
          List<TuningDetail> tuningDetailsFound = new List<TuningDetail>();
          // Multiplexes found will contain a dictionary of ONID + TSID => multiplex tuning details.
          Dictionary<uint, TuningDetail> multiplexesFound = new Dictionary<uint, TuningDetail>();

          for (int i = 0; i < multiplexCount; ++i)
          {
            if (_cancelScan)
            {
              return new List<TuningDetail>();
            }
            int originalNetworkId;
            int transportStreamId;
            BroadcastStandard broadcastStandard;
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
                          out originalNetworkId, out transportStreamId,
                          out broadcastStandard, out frequency,
                          out polarisation, out modulation, out symbolRate,
                          out bandwidth, out innerFecRate, out rollOff,
                          out longitude, out cellId, out cellIdExtension,
                          out plpId);

            TuningDetail tuningDetail = new TuningDetail();
            tuningDetail.BroadcastStandard = broadcastStandard;
            tuningDetail.Frequency = frequency;
            tuningDetail.SymbolRate = symbolRate;
            tuningDetail.Bandwidth = bandwidth;
            tuningDetail.StreamId = plpId;
            if (broadcastStandard == BroadcastStandard.DvbC)
            {
              ModulationSchemeQam modulationScheme;
              switch (modulation)
              {
                case 1:
                  modulationScheme = ModulationSchemeQam.Qam16;
                  break;
                case 2:
                  modulationScheme = ModulationSchemeQam.Qam32;
                  break;
                case 3:
                  modulationScheme = ModulationSchemeQam.Qam64;
                  break;
                case 4:
                  modulationScheme = ModulationSchemeQam.Qam128;
                  break;
                case 5:
                  modulationScheme = ModulationSchemeQam.Qam256;
                  break;
                default:
                  this.LogWarn("scan NIT: unsupported DVB-C modulation scheme {0}, falling back to automatic", modulation);
                  modulationScheme = ModulationSchemeQam.Automatic;
                  break;
              }
              tuningDetail.ModulationScheme = modulationScheme.ToString();
            }
            else if (broadcastStandard == BroadcastStandard.DvbS2)
            {
              switch (rollOff)
              {
                case 0:
                  tuningDetail.RollOffFactor = RollOffFactor.ThirtyFive;
                  break;
                case 1:
                  tuningDetail.RollOffFactor = RollOffFactor.TwentyFive;
                  break;
                case 2:
                  tuningDetail.RollOffFactor = RollOffFactor.Twenty;
                  break;
                default:
                  this.LogWarn("scan NIT: unsupported DVB-S2 roll-off factor {0}, falling back to automatic", rollOff);
                  tuningDetail.RollOffFactor = RollOffFactor.Automatic;
                  break;
              }
            }
            else if (
              broadcastStandard != BroadcastStandard.DvbC2 &&
              broadcastStandard != BroadcastStandard.DvbS &&
              broadcastStandard != BroadcastStandard.DvbT &&
              broadcastStandard != BroadcastStandard.DvbT2
            )
            {
              throw new TvException("ScannerDirectShowBase: unsupported broadcast standard {0} returned from TsWriter network scan", broadcastStandard);
            }

            if ((broadcastStandard & BroadcastStandard.MaskSatellite) != 0)
            {
              ModulationSchemePsk modulationScheme;
              switch (modulation)
              {
                case 0:
                  modulationScheme = ModulationSchemePsk.Automatic;
                  this.LogWarn("scan NIT: automatic satellite modulation specified, not supported by all hardware");
                  break;
                case 1:
                  modulationScheme = ModulationSchemePsk.Psk4;
                  break;
                case 2:
                  modulationScheme = ModulationSchemePsk.Psk8;
                  break;
                default:
                  // 16 QAM and any other unsupported value
                  this.LogWarn("scan NIT: unsupported DVB-S/S2 modulation scheme {0}, falling back to automatic", modulation);
                  modulationScheme = ModulationSchemePsk.Automatic;
                  break;
              }
              tuningDetail.ModulationScheme = modulationScheme.ToString();

              switch (innerFecRate)
              {
                case 1:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate1_2;
                  break;
                case 2:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate2_3;
                  break;
                case 3:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate3_4;
                  break;
                case 4:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate5_6;
                  break;
                case 5:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate7_8;
                  break;
                case 6:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate8_9;
                  break;
                case 7:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate3_5;
                  break;
                case 8:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate4_5;
                  break;
                case 9:
                  tuningDetail.FecCodeRate = FecCodeRate.Rate9_10;
                  break;
                default:
                  this.LogWarn("scan NIT: unsupported DVB-S/S2 FEC code rate {0}, falling back to automatic", modulation);
                  tuningDetail.FecCodeRate = FecCodeRate.Automatic;
                  break;
              }

              switch (polarisation)
              {
                case 0:
                  tuningDetail.Polarisation = Polarisation.LinearHorizontal;
                  break;
                case 1:
                  tuningDetail.Polarisation = Polarisation.LinearVertical;
                  break;
                case 2:
                  tuningDetail.Polarisation = Polarisation.CircularLeft;
                  break;
                case 3:
                  tuningDetail.Polarisation = Polarisation.CircularRight;
                  break;
                default:
                  this.LogWarn("scan NIT: unsupported DVB-S/S2 polarisation {0}, falling back to automatic", polarisation);
                  tuningDetail.Polarisation = Polarisation.Automatic;
                  break;
              }
            }

            bool isUniqueTuning = true;
            foreach (TuningDetail td in tuningDetailsFound)
            {
              if (td.Equals(tuningDetail))
              {
                isUniqueTuning = false;
                break;
              }
            }
            if (isUniqueTuning)
            {
              tuningDetailsFound.Add(tuningDetail);
            }

            if (isServiceInfoAvailable)
            {
              uint key = (uint)((uint)originalNetworkId << 16) + (uint)transportStreamId;
              if (multiplexesFound.ContainsKey(key))
              {
                this.LogDebug("scan NIT: tuning details for ONID {0} and TSID {1} are ambiguous, disregarding service information", originalNetworkId, transportStreamId);
                isServiceInfoAvailable = false;
              }
              else
              {
                multiplexesFound.Add(key, tuningDetail);
              }
            }
          }

          // TODO implement support for fast scan channel handling.
          return tuningDetailsFound;

          // If service information is not available or the corresponding tuning details are ambiguous then we return
          // a set of multiplex tuning details.
          if (!isServiceInfoAvailable)
          {
            return tuningDetailsFound;
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
            int networkIdCount = 10;
            ushort[] networkIds = new ushort[10];
            int bouquetIdCount = 20;
            ushort[] bouquetIds = new ushort[20];
            int languageCount = 10;
            Iso639Code[] languages = new Iso639Code[10];
            int availableInCellCount = 30;
            uint[] availableInCells = new uint[30];
            int unavailableInCellCount = 30;
            uint[] unavailableInCells = new uint[30];
            int targetRegionCount = 30;
            long[] targetRegionIds = new long[30];
            int availableInCountryCount = 10;
            Iso639Code[] availableInCountries = new Iso639Code[10];
            int unavailableInCountryCount = 10;
            Iso639Code[] unavailableInCountries = new Iso639Code[10];
            _analyser.GetServiceDetail(i,
                          out originalNetworkId, out transportStreamId, out serviceId,
                          out serviceNamePtr, out providerNamePtr, out logicalChannelNumberPtr,
                          out serviceType, out videoStreamCount, out audioStreamCount, out isHighDefinition, out isEncrypted, out isRunning, out pmtPid,
                          out previousOriginalNetworkId, out previousTransportStreamId, out previousServiceId,
                          ref networkIdCount, ref networkIds,
                          ref bouquetIdCount, ref bouquetIds,
                          ref languageCount, ref languages,
                          ref availableInCellCount, ref availableInCells, ref unavailableInCellCount, ref unavailableInCells,
                          ref targetRegionCount, ref targetRegionIds,
                          ref availableInCountryCount, ref availableInCountries, ref unavailableInCountryCount, ref unavailableInCountries);

            string serviceName = DvbTextConverter.Convert(serviceNamePtr);
            string providerName = DvbTextConverter.Convert(providerNamePtr);
            string logicalChannelNumber = Marshal.PtrToStringAnsi(logicalChannelNumberPtr);
            this.LogDebug("{0}) {1,-32} provider = {2,-16}, LCN = {3,-7}, ONID = {4,-5}, TSID = {5,-5}, SID = {6,-5}, PMT PID = {7,-5}, previous ONID = {8,-5}, previous TSID = {9,-5}, previous SID = {10,-5}",
                            i + 1, serviceName, providerName, logicalChannelNumber, originalNetworkId, transportStreamId, serviceId, pmtPid, previousOriginalNetworkId, previousTransportStreamId, previousServiceId);
            this.LogDebug("    type = {0}, video stream count = {1}, audio stream count = {2}, is high definition = {3}, is encrypted = {4}, is running = {5}",
                            serviceType, videoStreamCount, audioStreamCount, isHighDefinition, isEncrypted, isRunning);

            List<string> details = new List<string>();
            IntPtr name;
            if (networkIds != null)
            {
              foreach (int nid in networkIds)
              {
                _analyser.GetNetworkName(nid, out name);
                details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", nid));
              }
            }
            this.LogDebug("    network ID count = {0}, network IDs = {1}", networkIdCount, string.Join(", ", details));

            details.Clear();
            if (bouquetIds != null)
            {
              foreach (int bid in bouquetIds)
              {
                _analyser.GetBouquetName(bid, out name);
                details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", bid));
              }
            }
            this.LogDebug("    bouquet ID count = {0}, bouquet IDs = {1}", bouquetIdCount, string.Join(", ", details));

            this.LogDebug("    language count = {0}, languages = {1}", languageCount, string.Join(", ", languages ?? new Iso639Code[0]));
            this.LogDebug("    available in cells count = {0}, cells = {1}", availableInCellCount, string.Join(", ", availableInCells ?? new uint[0]));
            this.LogDebug("    unavailable in cells count = {0}, cells = {1}", unavailableInCellCount, string.Join(", ", unavailableInCells ?? new uint[0]));

            details.Clear();
            if (targetRegionIds != null)
            {
              foreach (int regionId in targetRegionIds)
              {
                _analyser.GetTargetRegionName(regionId, out name);
                details.Add(DvbTextConverter.Convert(name) + string.Format(" ({0})", regionId));
              }
            }
            this.LogDebug("    target region count = {0}, regions = {1}", targetRegionCount, string.Join(", ", details));

            this.LogDebug("    available in country count = {0}, countries = {1}", availableInCountryCount, string.Join(", ", availableInCountries ?? new Iso639Code[0]));
            this.LogDebug("    unavailable in country count = {0}, countries = {1}", unavailableInCountryCount, string.Join(", ", unavailableInCountries ?? new Iso639Code[0]));

            // The SDT/VCT service type is unfortunately not sufficient for
            // service type identification. Many DVB-IP and some ATSC and North
            // American cable broadcasters in particular do not set the service
            // type.
            MediaType? mediaType = _scanHelper.GetMediaType(serviceType, videoStreamCount, audioStreamCount);
            if (!mediaType.HasValue)
            {
              this.LogDebug("Service type is not supported.");
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

            ChannelDvbBase newChannel = (ChannelDvbBase)multiplexesFound[key].GetTuningChannel();
            newChannel.Name = serviceName;
            newChannel.Provider = providerName;
            newChannel.MediaType = mediaType.Value;
            newChannel.IsEncrypted = isEncrypted;
            newChannel.LogicalChannelNumber = logicalChannelNumber;
            newChannel.OriginalNetworkId = originalNetworkId;
            newChannel.TransportStreamId = transportStreamId;
            newChannel.ProgramNumber = serviceId;
            newChannel.PmtPid = pmtPid;   // Important: this should be zero when not known, otherwise tuning will fail.

            IChannel c = newChannel as IChannel;
            _scanHelper.UpdateChannel(ref c);
            this.LogDebug("Found: {0}", c);
            servicesFound.Add(c);
          }

          this.LogDebug("Scan found {0} channels from {1} services", found, serviceCount);
          //return servicesFound;
        }
        finally
        {
          if (_analyser != null)
          {
            _analyser.SetCallBack(null);
          }
          _event.Close();
          _event = null;
        }
      }
      finally
      {
        _isScanning = false;
      }
    }

    #endregion
  }
}