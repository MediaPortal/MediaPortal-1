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
using System.Runtime.InteropServices;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer
{
  ///<summary>
  /// TsWriter channel scanner callback interface.
  ///</summary>
  [ComVisible(true), ComImport,
    Guid("ce141670-1840-4188-8a40-618ba3a5a1c3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IChannelScanCallBack
  {
    /// <summary>
    /// Called by an ITsChannelScan instance when all available service information is ready to
    /// be retrieved.
    /// </summary>
    /// <returns>an HRESULT indicating whether the notification was successfully handled</returns>
    [PreserveSig]
    int OnServiceInfoComplete();

    /// <summary>
    /// Called by an ITsChannelScan instance when all available network information is ready to
    /// be retrieved.
    /// </summary>
    /// <returns>an HRESULT indicating whether the notification was successfully handled</returns>
    [PreserveSig]
    int OnNetworkInfoComplete();
  }

  /// <summary>
  /// Broadcast standards supported by the TsWriter scanner.
  /// </summary>
  public enum BroadcastStandard
  {
    /// <summary>
    /// DVB - EN 300 468.
    /// </summary>
    Dvb = 0,
    /// <summary>
    /// ATSC - A/53 part 3, A/65.
    /// </summary>
    Atsc = 1,
    /// <summary>
    /// North American cable - SCTE 54, treated the same as ATSC.
    /// </summary>
    Scte = 2,
    /// <summary>
    /// ISDB - treated the same as DVB.
    /// </summary>
    Isdb = 3
  }

  /// <summary>
  /// A structure used for service lists returned by ITsChannelScan.GetNetworkTransportStreamDetail()
  /// and ITsChannelScan.GetBouquetTransportStreamDetail().
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Service
  {
    /// <summary>
    /// The service identifier.
    /// </summary>
    public int ServiceId;
    /// <summary>
    /// The logical channel number associated with the service.
    /// </summary>
    public int LogicalChannelNumber;
  }

  /// <summary>
  /// A structure used for the frequency list returned by ITsChannelScan.GetNetworkTransportStreamDetail().
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct CellFrequency
  {
    /// <summary>
    /// The frequency (in kHz) used to transmit a transport stream in a cell.
    /// </summary>
    public int Frequency;
    /// <summary>
    /// The cell identifier. Only applicable for DVB-T transmitters.
    /// </summary>
    public int CellId;
    /// <summary>
    /// The cell identifier extension. Only applicable for DVB-T transposers/repeaters.
    /// </summary>
    public int CellIdExtension;
  }

  /// <summary>
  /// TsWriter channel scanner interface.
  /// </summary>
  [ComVisible(true), ComImport,
    Guid("1663dc42-d169-41da-bce2-eeec482cb9fb"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsChannelScan
  {
    /// <summary>
    /// Set the delegate for the scanner to notify when scanning is complete.
    /// </summary>
    /// <param name="callBack">The delegate callback interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallBack(IChannelScanCallBack callBack);

    /// <summary>
    /// Start scanning for service and network information in the stream that is currently being received.
    /// </summary>
    /// <param name="broadcastStandard">The broadcast standard that the stream conforms with.</param>
    /// <param name="recordOtherTransmitterServiceInfo"><c>True</c> to record information about services
    ///   that are not present in the current stream, otherwise <c>false</c>.</param>
    /// <returns>an HRESULT indicating whether scanning is successfully started</returns>
    [PreserveSig]
    int StartScanning(BroadcastStandard broadcastStandard, [MarshalAs(UnmanagedType.I1)] bool recordOtherTransmitterServiceInfo);

    /// <summary>
    /// Stop scanning.
    /// </summary>
    /// <param name="isOtherTransmitterServiceInfoAvailable"><c>True</c> if information about services
    ///   that are not present in the current stream is available, otherwise <c>false</c>.</param>
    /// <returns>an HRESULT indicating whether scanning is successfully stopped</returns>
    [PreserveSig]
    int StopScanning([MarshalAs(UnmanagedType.I1)] out bool isOtherTransmitterServiceInfoAvailable);

    /// <summary>
    /// Get the number of services found during the most recently completed scan.
    /// </summary>
    /// <param name="serviceCount">The number of services found by the scanner.</param>
    /// <returns>an HRESULT indicating whether the service count was successfully retrieved</returns>
    [PreserveSig]
    int GetServiceCount(out int serviceCount);

    /// <summary>
    /// Retrieve the details for a specific service from the scanner.
    /// </summary>
    /// <remarks>
    /// Many of the parameters for this method are not applicable for North American television transmission standards.
    /// Specifically: isHighDefinition, previous service identifiers and all of the lists except the language list will
    /// not be populated for ATSC, SCTE cable and clear QAM services.
    /// In the same way, the logicalChannelNumber parameter is not applicable for DVB services. DVB logical channel
    /// numbers can be retrieved from the network and bouquet service lists.
    /// </remarks>
    /// <param name="index">The service index. The value of this parameter should be in the range 0..[GetServiceCount() - 1] (inclusive).</param>
    /// <param name="originalNetworkId">The identifier of the network that the service originates from.</param>
    /// <param name="transportStreamId">The identifier of the transport stream that contains the service.</param>
    /// <param name="serviceId">The service's identifier.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="providerName">The name of the service's provider.</param>
    /// <param name="logicalChannelNumber">The logical channel number associated with the service.</param>
    /// <param name="serviceType">The standard-specific type code for the service (eg. for DVB, 1 is digital television).</param>
    /// <param name="videoStreamCount">The number of video streams associated with the service.</param>
    /// <param name="audioStreamCount">The number of audio streams associated with the service.</param>
    /// <param name="isHighDefinition"><c>True</c> if the service has high resolution video, otherwise <c>false</c>.</param>
    /// <param name="isEncrypted"><c>True</c> if the service is encrypted, otherwise <c>false</c>.</param>
    /// <param name="isRunning"><c>True</c> if the service is currently active, otherwise <c>false</c>.</param>
    /// <param name="pmtPid">The service's PMT PID.</param>
    /// <param name="previousOriginalNetworkId">The service's previous original network ID. Only populated when it can
    ///     be determined that the service identifier changed recently.</param>
    /// <param name="previousTransportStreamId">The service's previous transport stream ID. Only populated when it can
    ///     be determined that the service identifier changed recently.</param>
    /// <param name="previousServiceId">The service's previous service ID. Only populated when it can be determined that
    ///     the service identifier changed recently.</param>
    /// <param name="bouquetIdCount">The number of bouquets that the service is included in.</param>
    /// <param name="bouquetIds">A buffer containing the Int32 identifiers of each bouquet that the service is included in.</param>
    /// <param name="languageCount">The distinct number of audio and subtitle languages that are transmitted as part of the service.</param>
    /// <param name="languages">A buffer containing the ISO 639 3 byte language codes associated with the audio and subtitle stream languages.</param>
    /// <param name="availableInCellCount">The number of cells in which the service may be received.</param>
    /// <param name="availableInCells">A buffer containing the Int32 identifiers of each cell in which the service may be received.</param>
    /// <param name="unavailableInCellCount">The number of cells in which the service may not be received.</param>
    /// <param name="unavailableInCells">A buffer containing the Int32 identifiers of each cell in which the service may not be received.</param>
    /// <param name="targetRegionCount">The number of regions in which the service is intended to be received.</param>
    /// <param name="targetRegions">A buffer containing the Int64 identifiers of each region in which the service is intended to be received.</param>
    /// <param name="availableInCountryCount">The number of countries in which the service is intended to be available.</param>
    /// <param name="availableInCountries">A buffer containing the ISO 3166 3 byte code for each country in which the service is intended to be available.</param>
    /// <param name="unavailableInCountryCount">The number of countries in which the service is not intended to be available.</param>
    /// <param name="unavailableInCountries">A buffer containing the ISO 3166 3 byte code for each country in which the service is not intended to be available.</param>
    /// <returns>an HRESULT indicating whether the service details were successfully retrieved</returns>
    [PreserveSig]
    int GetServiceDetail(int index,
                          out int originalNetworkId,
                          out int transportStreamId,
                          out int serviceId,
                          out IntPtr serviceName,
                          out IntPtr providerName,
                          out IntPtr logicalChannelNumber,
                          out int serviceType,
                          out int videoStreamCount,
                          out int audioStreamCount,
                          [MarshalAs(UnmanagedType.I1)] out bool isHighDefinition,
                          [MarshalAs(UnmanagedType.I1)] out bool isEncrypted,
                          [MarshalAs(UnmanagedType.I1)] out bool isRunning,
                          out int pmtPid,
                          out int previousOriginalNetworkId,
                          out int previousTransportStreamId,
                          out int previousServiceId,
                          out int bouquetIdCount,
                          out IntPtr bouquetIds,
                          out int languageCount,
                          out IntPtr languages,
                          out int availableInCellCount,
                          out IntPtr availableInCells,
                          out int unavailableInCellCount,
                          out IntPtr unavailableInCells,
                          out int targetRegionCount,
                          out IntPtr targetRegions,
                          out int availableInCountryCount,
                          out IntPtr availableInCountries,
                          out int unavailableInCountryCount,
                          out IntPtr unavailableInCountries);

    /// <summary>
    /// Get the number of networks found during the most recently completed scan.
    /// </summary>
    /// <param name="networkCount">The number of networks found by the scanner.</param>
    /// <returns>an HRESULT indicating whether the network count was successfully retrieved</returns>
    [PreserveSig]
    int GetNetworkCount(out int networkCount);

    /// <summary>
    /// Retrieve the details for a specific network from the scanner.
    /// </summary>
    /// <param name="index">The network index. The value of this parameter should be in the range 0..[GetNetworkCount() - 1] (inclusive).</param>
    /// <param name="networkId">The network's identifier.</param>
    /// <param name="transportStreamCount">The number of transport streams that belong to the network.</param>
    /// <param name="targetRegionCount">The number of regions in which the network is intended to be received.</param>
    /// <param name="targetRegions">A buffer containing the Int64 identifiers of each region in which the network is intended to be received.</param>
    /// <returns>an HRESULT indicating whether the network detail was successfully retrieved</returns>
    [PreserveSig]
    int GetNetworkDetail(int index,
                          out int networkId,
                          out int transportStreamCount,
                          out int targetRegionCount,
                          out IntPtr targetRegions);

    /// <summary>
    /// Retrieve the name of a specific network.
    /// </summary>
    /// <param name="networkId">The network identifier.</param>
    /// <param name="name">The name of the network.</param>
    /// <returns>an HRESULT indicating whether the network name was successfully retrieved</returns>
    [PreserveSig]
    int GetNetworkName(int networkId, out IntPtr name);

    /// <summary>
    /// Retrieve the details for a specific network transport stream from the scanner.
    /// </summary>
    /// <param name="networkId">The identifier of the network to retrieve details for. The value of this parameter
    ///   should match one of the networkId values retrieved using GetNetworkDetail().</param>
    /// <param name="index">The transport stream index. The value of this parameter should be in the range
    ///   0..[GetNetworkDetail().transportStreamCount - 1] (inclusive).</param>
    /// <param name="originalNetworkId">The identifier of the network that the transport stream originates from.</param>
    /// <param name="transportStreamId">The transport stream's identifier.</param>
    /// <param name="serviceCount">The number of services multiplexed into the transport stream. If zero, all services
    ///   with matching original network and transport stream identifiers should be considered as belonging to the network.</param>
    /// <param name="services">A buffer containing the Service details of each service which belongs to the network.</param>
    /// <param name="targetRegionCount">The number of regions in which the transport stream is intended to be received.</param>
    /// <param name="targetRegions">A buffer containing the Int64 identifiers of each region in which the transport stream is intended to be received.</param>
    /// <param name="type">The type of transmitter (eg. cable, satellite, terrestrial) used to transmit the transport
    ///   stream, as per the TV database tuning detail type.</param>
    /// <param name="frequencyCount">The number of frequencies on which the transport stream is transmitted.</param>
    /// <param name="frequencies">A buffer containing the Frequency details for each frequency on which the transport stream is transmitted.</param>
    /// <param name="polarisation">The transmitter polarisation. Only applicable for DVB-S/2 transmitters.</param>
    /// <param name="modulation">The transmitter modulation scheme. Only applicable for DVB-S/2 and DVB-C transmitters.</param>
    /// <param name="symbolRate">The transmitter symbol rate, in ks/s. Only applicable for DVB-S/2 and DVB-C transmitters.</param>
    /// <param name="bandwidth">The transmitter bandwith, in MHz. Only applicable for DVB-T transmitters.</param>
    /// <param name="innerFecRate">The transmitter inner FEC rate. Only applicable for DVB-S/2 transmitters.</param>
    /// <param name="rollOff">The transmitter roll-off parameter. Only applicable for DVB-S2 transmitters.</param>
    /// <param name="longitude">The longitude of the geostationary satellite that the transmitter is fixed to, in
    ///     tenths of a degree. Positive longitude values specify East position; negative longitude values specify West
    ///     position. Only applicable for DVB-S/S2 transmitters.</param>
    /// <param name="plpId">The physical layer pipe identifier associated with the transport stream. Only applicable for
    ///   DVB-S2 and DVB-T2 transmitters.</param>
    /// <returns>an HRESULT indicating whether the transport stream details were successfully retrieved</returns>
    [PreserveSig]
    int GetNetworkTransportStreamDetail(int networkId,
                                        int index,
                                        out int originalNetworkId,
                                        out int transportStreamId,
                                        out int serviceCount,
                                        out IntPtr services,
                                        out int targetRegionCount,
                                        out IntPtr targetRegions,
                                        out int type,
                                        out int frequencyCount,
                                        out IntPtr frequencies,
                                        out int polarisation,
                                        out int modulation,
                                        out int symbolRate,
                                        out int bandwidth,
                                        out int innerFecRate,
                                        out int rollOff,
                                        out int longitude,
                                        out int plpId);

    /// <summary>
    /// Get the number of bouquets found during the most recently completed scan.
    /// </summary>
    /// <param name="bouquetCount">The number of bouquets found by the scanner.</param>
    /// <returns>an HRESULT indicating whether the bouquet count was successfully retrieved</returns>
    [PreserveSig]
    int GetBouquetCount(out int bouquetCount);

    /// <summary>
    /// Retrieve the details for a specific bouquet from the scanner.
    /// </summary>
    /// <param name="index">The bouquet index. The value of this parameter should be in the range 0..[GetBouquetCount() - 1] (inclusive).</param>
    /// <param name="networkId">The bouquet's identifier.</param>
    /// <param name="transportStreamCount">The number of transport streams that belong to the bouquet.</param>
    /// <param name="targetRegionCount">The number of regions in which the bouquet is intended to be received.</param>
    /// <param name="targetRegions">A buffer containing the Int64 identifiers of each region in which the bouquet is intended to be received.</param>
    /// <param name="availableInCountryCount">The number of countries in which the bouquet is intended to be available.</param>
    /// <param name="availableInCountries">A buffer containing the ISO 3166 3 byte code for each country in which the bouquet is intended to be available.</param>
    /// <param name="unavailableInCountryCount">The number of countries in which the bouquet is not intended to be available.</param>
    /// <param name="unavailableInCountries">A buffer containing the ISO 3166 3 byte code for each country in which the bouquet is not intended to be available.</param>
    /// <returns>an HRESULT indicating whether the bouquet detail was successfully retrieved</returns>
    [PreserveSig]
    int GetBouquetDetail(int index,
                          out int bouquetId,
                          out int transportStreamCount,
                          out int targetRegionCount,
                          out IntPtr targetRegions,
                          out int availableInCountryCount,
                          out IntPtr availableInCountries,
                          out int unavailableInCountryCount,
                          out IntPtr unavailableInCountries);

    /// <summary>
    /// Retrieve the name of a specific bouquet.
    /// </summary>
    /// <param name="bouquetId">The bouquet identifier.</param>
    /// <param name="name">The name of the bouquet.</param>
    /// <returns>an HRESULT indicating whether the bouquet name was successfully retrieved</returns>
    [PreserveSig]
    int GetBouquetName(int networkId, out IntPtr name);

    /// <summary>
    /// Retrieve the details for a specific bouquet transport stream from the scanner.
    /// </summary>
    /// <param name="bouquetId">The identifier of the bouquet to retrieve details for. The value of this parameter
    ///   should match one of the bouquetId values retrieved using GetBouquetDetail().</param>
    /// <param name="index">The transport stream index. The value of this parameter should be in the range
    ///   0..[GetBouquetDetail().transportStreamCount - 1] (inclusive).</param>
    /// <param name="originalNetworkId">The identifier of the network that the transport stream originates from.</param>
    /// <param name="transportStreamId">The transport stream's identifier.</param>
    /// <param name="serviceCount">The number of services multiplexed into the transport stream. If zero, all services
    ///   with matching original network and transport stream identifiers should be considered as belonging to the bouquet.</param>
    /// <param name="services">A buffer containing the Service details of each service which belongs to the bouquet.</param>
    /// <param name="targetRegionCount">The number of regions in which the transport stream is intended to be received.</param>
    /// <param name="targetRegions">A buffer containing the Int64 identifiers of each region in which the transport stream is intended to be received.</param>
    /// <returns>an HRESULT indicating whether the transport stream details were successfully retrieved</returns>
    [PreserveSig]
    int GetBouquetTransportStreamDetail(int bouquetId,
                                        int index,
                                        out int originalNetworkId,
                                        out int transportStreamId,
                                        out int serviceCount,
                                        out IntPtr services,
                                        out int targetRegionCount,
                                        out IntPtr targetRegions);

    /// <summary>
    /// Retrieve the name of a specific target region.
    /// </summary>
    /// <param name="targetRegionId">The region identifier.</param>
    /// <param name="name">The name of the region.</param>
    /// <returns>an HRESULT indicating whether the target region name was successfully retrieved</returns>
    [PreserveSig]
    int GetTargetRegionName(Int64 targetRegionId, out IntPtr name);
  }
}