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

namespace TvLibrary.Interfaces.Analyzer
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
    /// Called by an ITsChannelScan instance when scanning is complete and all available service
    /// information is ready to be retrieved.
    /// </summary>
    /// <returns>an HRESULT indicating whether the notification was successfully handled</returns>
    [PreserveSig]
    int OnScannerDone();
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
    /// Start scanning for services in the stream that is currently being received.
    /// </summary>
    /// <param name="broadcastStandard">The broadcast standard that the stream conforms with.</param>
    /// <returns>an HRESULT indicating whether scanning is successfully started</returns>
    [PreserveSig]
    int ScanStream(BroadcastStandard broadcastStandard);

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
    int GetServiceCount(out int channelCount);

    /// <summary>
    /// Retrieve the details for a specific service from the scanner.
    /// </summary>
    /// <param name="index">The service index. The value of this parameter should be in the range 0..[GetServiceCount() - 1] (inclusive).</param>
    /// <param name="networkId">The service's network ID.</param>
    /// <param name="transportStreamId">The service's transport stream ID.</param>
    /// <param name="serviceId">The service's ID.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="providerName">The name of the service's provider.</param>
    /// <param name="networkNames">The names of the networks that the service is included in. Names are ",," separated.</param>
    /// <param name="bouquetNames">The names of the bouquets that the service is included in. Names are ",," separated.</param>
    /// <param name="logicalChannelNumber">The logical channel number associated with the service.</param>
    /// <param name="serviceType">The type of the service (eg. TV, radio).</param>
    /// <param name="hasVideo">The number of video streams associated with the service.</param>
    /// <param name="hasAudio">The number of audio streams associated with the service.</param>
    /// <param name="isEncrypted"><c>One</c> if the service is encrypted, otherwise <c>zero</c>.</param>
    /// <param name="pmtPid">The service's PMT PID.</param>
    /// <returns>an HRESULT indicating whether the service details were successfully retrieved</returns>
    [PreserveSig]
    int GetServiceDetail(int index,
                          out int networkId,
                          out int transportStreamId,
                          out int serviceId,
                          out IntPtr serviceName,
                          out IntPtr providerName,
                          out IntPtr networkNames,
                          out IntPtr bouquetNames,
                          out IntPtr logicalChannelNumber,
                          out int serviceType,
                          out int hasVideo,
                          out int hasAudio,
                          out int isEncrypted,
                          out int pmtPid);

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
    int StopNetworkScan(out bool isOtherMuxServiceInfoAvailable);

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
    /// <param name="networkId">The multiplex's network ID.</param>
    /// <param name="transportStreamId">The multiplex's transport stream ID.</param>
    /// <param name="type">The multiplex type (eg. cable, satellite, terrestrial), as per the TV database tuning detail type.</param>
    /// <param name="frequency">The multiplex frequency, in kHz.</param>
    /// <param name="polarisation">The multiplex polarisation. Only applicable for DVB-S/2 multiplexes.</param>
    /// <param name="modulation">The multiplex modulation scheme. Only applicable for DVB-S/2 and DVB-C multiplexes.</param>
    /// <param name="symbolRate">The multiplex symbol rate, in ks/s. Only applicable for DVB-S/2 and DVB-C multiplexes.</param>
    /// <param name="bandwidth">The multiplex bandwith, in MHz. Only applicable for DVB-T multiplexes.</param>
    /// <param name="innerFecRate">The multiplex inner FEC rate. Only applicable for DVB-S/2 multiplexes.</param>
    /// <param name="rollOff">The multiplex roll-off parameter. Only applicable for DVB-S2 multiplexes.</param>
    /// <returns>an HRESULT indicating whether the multiplex details were successfully retrieved</returns>
    [PreserveSig]
    int GetMultiplexDetail(int index,
                            out int networkId,
                            out int transportStreamId,
                            out int type,
                            out int frequency,
                            out int polarisation,
                            out int modulation,
                            out int symbolRate,
                            out int bandwidth,
                            out int innerFecRate,
                            out int rollOff);
  }
}