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
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer
{
  ///<summary>
  /// TsWriter channel scanner call back interface.
  ///</summary>
  [Guid("ce141670-1840-4188-8a40-618ba3a5a1c3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IChannelScanCallBack
  {
    /// <summary>
    /// Called by an ITsChannelScan instance when scanning is complete and all available service
    /// information is ready to be retrieved.
    /// </summary>
    /// <returns>an HRESULT indicating whether the notification was successfully handled</returns>
    [PreserveSig]
    int OnScannerDone();
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
  internal struct Iso639Code
  {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    public string Code;

    public override string ToString()
    {
      return Code;
    }
  }

  /// <summary>
  /// Service information formats supported by the TsWriter scanner.
  /// </summary>
  internal enum ServiceInformationFormat
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
  /// TsWriter channel scanner interface.
  /// </summary>
  [Guid("1663dc42-d169-41da-bce2-eeec482cb9fb"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface ITsChannelScan
  {
    /// <summary>
    /// Set the delegate for the scanner to notify when scanning is complete.
    /// </summary>
    /// <param name="callBack">The delegate call back interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallBack(IChannelScanCallBack callBack);

    /// <summary>
    /// Start scanning for services in the stream that is currently being received.
    /// </summary>
    /// <param name="serviceInformationFormat">The format of the service information contained in the stream.</param>
    /// <returns>an HRESULT indicating whether scanning is successfully started</returns>
    [PreserveSig]
    int ScanStream(ServiceInformationFormat serviceInformationFormat);

    /// <summary>
    /// Stop stream scanning.
    /// </summary>
    /// <returns>an HRESULT indicating whether scanning is successfully stopped</returns>
    [PreserveSig]
    int StopStreamScan();

    /// <summary>
    /// Get the number of services found during the most recently completed scan.
    /// </summary>
    /// <remarks>
    /// In the case of a network scan where service information is available for other streams,
    /// this count will include the services from other streams.
    /// </remarks>
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
    /// </remarks>
    /// <param name="index">The service index. The value of this parameter should be in the range 0..[GetServiceCount() - 1] (inclusive).</param>
    /// <param name="originalNetworkId">The service's original network ID.</param>
    /// <param name="transportStreamId">The service's transport stream ID.</param>
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
    /// <param name="networkIdCount">The number of networks that the service is included in.</param>
    /// <param name="networkIds">A buffer containing the Int32 identifiers of each network that the service is included in.</param>
    /// <param name="bouquetIdCount">The number of bouquets that the service is included in.</param>
    /// <param name="bouquetIds">A buffer containing the Int32 identifiers of each bouquet that the service is included in.</param>
    /// <param name="languageCount">The distinct number of audio and sub-title languages that are transmitted as part of the service.</param>
    /// <param name="languages">A buffer containing the ISO 639 3 byte language codes associated with the audio and sub-title stream languages.</param>
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
                          ref int networkIdCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 17)] ref ushort[] networkIds,
                          ref int bouquetIdCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 19)] ref ushort[] bouquetIds,
                          ref int languageCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 21)] ref Iso639Code[] languages,
                          ref int availableInCellCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 23)] ref uint[] availableInCells,
                          ref int unavailableInCellCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 25)] ref uint[] unavailableInCells,
                          ref int targetRegionCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 27)] ref long[] targetRegions,
                          ref int availableInCountryCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 29)] ref Iso639Code[] availableInCountries,
                          ref int unavailableInCountryCount,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 31)] ref Iso639Code[] unavailableInCountries);

    /// <summary>
    /// Start scanning for network information in the stream that is currently being received.
    /// </summary>
    /// <remarks>
    /// Network scanning is currently only supported for DVB streams as other broadcast standards
    /// don't seem to carry in-band network information.
    /// </remarks>
    /// <returns>an HRESULT indicating whether scanning is successfully started</returns>
    [PreserveSig]
    int ScanNetwork();

    /// <summary>
    /// Stop network scanning.
    /// </summary>
    /// <param name="isOtherMuxServiceInfoAvailable">An indicator for whether service information
    ///   for the multiplexes identified by the scanner is available. If information is available,
    ///   it is not necessary to scan each individual multiplex, which is a huge timesaver.</param>
    /// <returns>an HRESULT indicating whether scanning is successfully stopped</returns>
    [PreserveSig]
    int StopNetworkScan([MarshalAs(UnmanagedType.I1)] out bool isOtherMuxServiceInfoAvailable);

    /// <summary>
    /// Get the number of multiplexes found during the most recently completed scan.
    /// </summary>
    /// <param name="multiplexCount">The number of multiplexes found by the scanner.</param>
    /// <returns>an HRESULT indicating whether the multiplex count was successfully retrieved</returns>
    [PreserveSig]
    int GetMultiplexCount(out int multiplexCount);

    /// <summary>
    /// Retrieve the details for a specific multiplex from the scanner.
    /// </summary>
    /// <param name="index">The multiplex index. The value of this parameter should be in the range 0..[GetMultiplexCount() - 1] (inclusive).</param>
    /// <param name="originalNetworkId">The multiplex's network ID.</param>
    /// <param name="transportStreamId">The multiplex's transport stream ID.</param>
    /// <param name="broadcastStandard">The multiplex broadcast standard.</param>
    /// <param name="frequency">The multiplex frequency, in kHz.</param>
    /// <param name="polarisation">The multiplex polarisation. Only applicable for DVB-S/2 multiplexes.</param>
    /// <param name="modulation">The multiplex modulation scheme. Only applicable for DVB-S/2 and DVB-C multiplexes.</param>
    /// <param name="symbolRate">The multiplex symbol rate, in ks/s. Only applicable for DVB-S/2 and DVB-C multiplexes.</param>
    /// <param name="bandwidth">The multiplex bandwith, in MHz. Only applicable for DVB-T multiplexes.</param>
    /// <param name="innerFecRate">The multiplex inner FEC rate. Only applicable for DVB-S/2 multiplexes.</param>
    /// <param name="rollOff">The multiplex roll-off parameter. Only applicable for DVB-S2 multiplexes.</param>
    /// <param name="longitude">The longitude of the geostationary satellite that the multiplex is transmitted from, in
    ///     tenths of a degree. Positive longitude values specify East position; negative longitude values specify West
    ///     position. Only applicable for DVB-S/S2 multiplexes.</param>
    /// <param name="cellId">The cell ID of the multiplex transmitter. Only applicable for DVB-T/T2 multiplexes.</param>
    /// <param name="cellIdExtension">The cell ID extension of the multiplex transmitter. Only applicable for DVB-T/T2 multiplexes.</param>
    /// <param name="plpId">The physical layer pipe ID of multiplex. Only applicable for DVB-S2 and DVB-T2 multiplexes.</param>
    /// <returns>an HRESULT indicating whether the multiplex details were successfully retrieved</returns>
    [PreserveSig]
    int GetMultiplexDetail(int index,
                            out int originalNetworkId,
                            out int transportStreamId,
                            out BroadcastStandard broadcastStandard,
                            out int frequency,
                            out int polarisation,
                            out int modulation,
                            out int symbolRate,
                            out int bandwidth,
                            out int innerFecRate,
                            out int rollOff,
                            out int longitude,
                            out int cellId,
                            out int cellIdExtension,
                            out int plpId);

    /// <summary>
    /// Retrieve the name of a specific target region.
    /// </summary>
    /// <param name="targetRegionId">The region identifier.</param>
    /// <param name="name">The name of the region.</param>
    /// <returns>an HRESULT indicating whether the target region name was successfully retrieved</returns>
    [PreserveSig]
    int GetTargetRegionName(long targetRegionId, out IntPtr name);

    /// <summary>
    /// Retrieve the name of a specific bouquet.
    /// </summary>
    /// <param name="bouquetId">The bouquet identifier.</param>
    /// <param name="name">The name of the bouquet.</param>
    /// <returns>an HRESULT indicating whether the bouquet name was successfully retrieved</returns>
    [PreserveSig]
    int GetBouquetName(int bouquetId, out IntPtr name);

    /// <summary>
    /// Retrieve the name of a specific network.
    /// </summary>
    /// <param name="networkId">The network identifier.</param>
    /// <param name="name">The name of the network.</param>
    /// <returns>an HRESULT indicating whether the network name was successfully retrieved</returns>
    [PreserveSig]
    int GetNetworkName(int networkId, out IntPtr name);
  }
}