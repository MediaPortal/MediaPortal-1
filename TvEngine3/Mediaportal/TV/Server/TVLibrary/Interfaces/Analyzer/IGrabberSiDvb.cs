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
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum;
using Polarisation = Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum.Polarisation;
using RollOffFactor = Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum.RollOffFactor;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// A DVB service information grabber interface.
  /// </summary>
  [Guid("dfbe4e16-fb7b-47d2-a1c0-6f6d9fb61e9a"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IGrabberSiDvb
  {
    /// <summary>
    /// Set the grabber's call back delegate.
    /// </summary>
    /// <param name="callBack">The delegate.</param>
    [PreserveSig]
    void SetCallBack(ICallBackGrabber callBack);

    /// <summary>
    /// Check if the grabber has received any bouquet association table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more BAT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsSeenBat();

    /// <summary>
    /// Check if the grabber has received any network information table sections for the actual transport stream.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more NIT-actual sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsSeenNitActual();

    /// <summary>
    /// Check if the grabber has received any network information table sections for other transport streams.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more NIT-other sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsSeenNitOther();

    /// <summary>
    /// Check if the grabber has received any service description table sections for the actual transport stream.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more SDT-actual sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsSeenSdtActual();

    /// <summary>
    /// Check if the grabber has received any service description table sections for other transport streams.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more SDT-other sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsSeenSdtOther();

    /// <summary>
    /// Check if the grabber has received all available bouquet association table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available PAT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadyBat();

    /// <summary>
    /// Check if the grabber has received all available network information table sections for the actual transport stream.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available NIT-actual sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadyNitActual();

    /// <summary>
    /// Check if the grabber has received all available network information table sections for other transport streams.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available NIT-other sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadyNitOther();

    /// <summary>
    /// Check if the grabber has received all available service description table sections for the actual transport stream.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available SDT-actual sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadySdtActual();

    /// <summary>
    /// Check if the grabber has received all available service description table sections for other transport streams.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available SDT-other sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool IsReadySdtOther();

    /// <summary>
    /// Get the number of DVB services received by the grabber.
    /// </summary>
    /// <param name="actualOriginalNetworkId">The identifier of the original network that the SDT-actual is associated with.</param>
    /// <param name="serviceCount">The number of DVB services received by the grabber.</param>
    [PreserveSig]
    void GetServiceCount(out ushort actualOriginalNetworkId, out ushort serviceCount);

    /// <summary>
    /// Retrieve a DVB service's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the service to retrieve. Should be in the range 0 to GetServiceCount() - 1.</param>
    /// <param name="tableId">The identifier of the table from which the service was received. Value is <c>0x42</c> for SDT-actual or <c>0x46</c> for SDT-other.</param>
    /// <param name="originalNetworkId">The identifier of the original network that the service is associated with.</param>
    /// <param name="transportStreamId">The identifier of the transport stream that the service is associated with.</param>
    /// <param name="serviceId">The service's DVB identifier. Only unique when combined with the <paramref name="originalNetworkId">original network ID</paramref> and <paramref name="transportStreamId">transport stream ID</paramref>.</param>
    /// <param name="referenceServiceId">The service identifier associated with the service's NVOD reference service, if any.</param>
    /// <param name="freesatChannelId">The service's Freesat identifier, if any. Should be unique among all Freesat services.</param>
    /// <param name="openTvChannelId">The service's OpenTV identifier, if any. Should be unique among all OpenTV services.</param>
    /// <param name="logicalChannelNumbers">The service's logical channel numbers, if any. The caller must allocate this array.</param>
    /// <param name="logicalChannelNumberCount">As an input, the size of the <paramref name="logicalChannelNumbers">logical channel numbers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="dishSubChannelNumber">The service's Dish Network sub-channel number, if any.</param>
    /// <param name="eitScheduleFlag">An indication of whether full event information for the service is available from this transport stream.</param>
    /// <param name="eitPresentFollowingFlag">An indication of whether the present and following event information for the service is available from this transport stream.</param>
    /// <param name="runningStatus">The service's running status, encoded according to DVB specifications.</param>
    /// <param name="freeCaMode">An indication of whether the service's streams are encrypted or not.</param>
    /// <param name="serviceType">The service's type, encoded according to DVB specifications.</param>
    /// <param name="serviceNameCount">The number of languages in which the service's name is available.</param>
    /// <param name="visibleInGuide">An indication of whether the service is intended to be visible in the electronic programme guide.</param>
    /// <param name="streamCountVideo">The number of video streams associated with the service.</param>
    /// <param name="streamCountAudio">The number of audio streams associated with the service.</param>
    /// <param name="isHighDefinition">An indication of whether the service's video is high definition.</param>
    /// <param name="isStandardDefinition">An indication of whether the service's video is standard definition.</param>
    /// <param name="isThreeDimensional">An indication of whether the service's video is three dimensional.</param>
    /// <param name="audioLanguages">The languages in which the service's audio will be available. The caller must allocate this array.</param>
    /// <param name="audioLanguageCount">As an input, the size of the <paramref name="audioLanguages">audio languages array</paramref>; as an output, the consumed array size.</param>
    /// <param name="subtitlesLanguages">The languages in which the service's subtitles will be available. The caller must allocate this array.</param>
    /// <param name="subtitlesLanguageCount">As an input, the size of the <paramref name="subtitlesLanguages">subtitles languages array</paramref>; as an output, the consumed array size.</param>
    /// <param name="networkIds">The identifiers of the networks which the service is associated with. The caller must allocate this array.</param>
    /// <param name="networkIdCount">As an input, the size of the <paramref name="networkIds">network identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="bouquetIds">The identifiers of the bouquets which the service is associated with. The caller must allocate this array.</param>
    /// <param name="bouquetIdCount">As an input, the size of the <paramref name="bouquetIds">bouquet identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="availableInCountries">The identifiers of the countries in which the service is intended to be available. The caller must allocate this array.</param>
    /// <param name="availableInCountryCount">As an input, the size of the <paramref name="availableInCountries">available-in-countries identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="unavailableInCountries">The identifiers of the countries in which the service is not intended to be available. The caller must allocate this array.</param>
    /// <param name="unavailableInCountryCount">As an input, the size of the <paramref name="unavailableInCountries">unavailable-in-countries identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="availableInCells">The identifiers of the terrestrial network cells in which the service is intended to be available. The caller must allocate this array.</param>
    /// <param name="availableInCellCount">As an input, the size of the <paramref name="availableInCells">available-in-cells identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="unavailableInCells">The identifiers of the terrestrial network cells in which the service is not intended to be available. The caller must allocate this array.</param>
    /// <param name="unavailableInCellCount">As an input, the size of the <paramref name="unavailableInCells">unavailable-in-cells identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="targetRegionIds">The identifiers of the regions at which the service is targeted. The caller must allocate this array.</param>
    /// <param name="targetRegionIdCount">As an input, the size of the <paramref name="targetRegionIds">target region identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="freesatRegionIds">The identifiers of the Freesat regions in which the service is intended to be available. The caller must allocate this array.</param>
    /// <param name="freesatRegionIdCount">As an input, the size of the <paramref name="freesatRegionIds">Freesat region identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="openTvRegionIds">The identifiers of the OpenTV regions in which the service is intended to be available. The caller must allocate this array.</param>
    /// <param name="openTvRegionIdCount">As an input, the size of the <paramref name="openTvRegionIds">OpenTV region identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="freesatChannelCategoryIds">The identifiers of the Freesat channel categories which the service is associated with. The caller must allocate this array.</param>
    /// <param name="freesatChannelCategoryIdCount">As an input, the size of the <paramref name="freesatChannelCategoryIds">Freesat channel category identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="virginMediaChannelCategoryId">The identifier of the Virgin Media channel category that the service is associated with, if any.</param>
    /// <param name="dishMarketId">The identifier of the Dish Network market (region) that the channel is intended for, if any.</param>
    /// <param name="norDigChannelListIds">The identifiers of the NorDig channel lists which the service is associated with. The caller must allocate this array.</param>
    /// <param name="norDigChannelListIdCount">As an input, the size of the <paramref name="norDigChannelListIds">NorDig channel list identifiers array</paramref>; as an output, the consumed array size.</param>
    /// <param name="previousOriginalNetworkId">The identifier of the original network that the service was previously associated with, if any.</param>
    /// <param name="previousTransportStreamId">The identifier of the transport stream that the service was previously associated with, if any.</param>
    /// <param name="previousServiceId">The service's previous DVB identifier.</param>
    /// <param name="epgOriginalNetworkId">The identifier of the original network that the service's electronic programme guide data is associated with.</param>
    /// <param name="epgTransportStreamId">The identifier of the transport stream that the service's electronic programme guide data is associated with.</param>
    /// <param name="epgServiceId">The identifier of the service that this service's electronic programme guide data is associated with.</param>
    /// <returns><c>true</c> if the service's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetService(ushort index,
                    out byte tableId,
                    out ushort originalNetworkId,
                    out ushort transportStreamId,
                    out ushort serviceId,
                    out ushort referenceServiceId,
                    out ushort freesatChannelId,
                    out ushort openTvChannelId,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 9)] LogicalChannelNumber[] logicalChannelNumbers,
                    ref ushort logicalChannelNumberCount,
                    out byte dishSubChannelNumber,
                    [MarshalAs(UnmanagedType.I1)] out bool eitScheduleFlag,
                    [MarshalAs(UnmanagedType.I1)] out bool eitPresentFollowingFlag,
                    out RunningStatus runningStatus,
                    [MarshalAs(UnmanagedType.I1)] out bool freeCaMode,
                    out ServiceType serviceType,
                    out byte serviceNameCount,
                    [MarshalAs(UnmanagedType.I1)] out bool visibleInGuide,
                    out ushort streamCountVideo,
                    out ushort streamCountAudio,
                    [MarshalAs(UnmanagedType.I1)] out bool isHighDefinition,
                    [MarshalAs(UnmanagedType.I1)] out bool isStandardDefinition,
                    [MarshalAs(UnmanagedType.I1)] out bool isThreeDimensional,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)] Iso639Code[] audioLanguages,
                    ref byte audioLanguageCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 26)] Iso639Code[] subtitlesLanguages,
                    ref byte subtitlesLanguageCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 28)] ushort[] networkIds,
                    ref byte networkIdCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 30)] ushort[] bouquetIds,
                    ref byte bouquetIdCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 32)] Iso639Code[] availableInCountries,
                    ref byte availableInCountryCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 34)] Iso639Code[] unavailableInCountries,
                    ref byte unavailableInCountryCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 36)] uint[] availableInCells,
                    ref byte availableInCellCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 38)] uint[] unavailableInCells,
                    ref byte unavailableInCellCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 40)] ulong[] targetRegionIds,
                    ref byte targetRegionIdCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 42)] uint[] freesatRegionIds,
                    ref byte freesatRegionIdCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 44)] uint[] openTvRegionIds,
                    ref byte openTvRegionIdCount,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 46)] ushort[] freesatChannelCategoryIds,
                    ref byte freesatChannelCategoryIdCount,
                    out byte virginMediaChannelCategoryId,
                    out ushort dishMarketId,
                    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 50)] byte[] norDigChannelListIds,
                    ref byte norDigChannelListIdCount,
                    out ushort previousOriginalNetworkId,
                    out ushort previousTransportStreamId,
                    out ushort previousServiceId,
                    out ushort epgOriginalNetworkId,
                    out ushort epgTransportStreamId,
                    out ushort epgServiceId);

    /// <summary>
    /// Retrieve a DVB service's name from the grabber.
    /// </summary>
    /// <param name="serviceIndex">The service's index. Should be in the range 0 to GetServiceCount() - 1.</param>
    /// <param name="nameIndex">The index of the name to retrieve. Should be in the range 0 to serviceNameCount - 1 for the service.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="providerName">A buffer containing the service provider's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="providerNameBufferSize">As an input, the size of the <paramref name="providerName">provider name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="serviceName">A buffer containing the service's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="serviceNameBufferSize">As an input, the size of the <paramref name="serviceName">service name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the service's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetServiceNameByIndex(ushort serviceIndex,
                                byte nameIndex,
                                out Iso639Code language,
                                IntPtr providerName,
                                ref ushort providerNameBufferSize,
                                IntPtr serviceName,
                                ref ushort serviceNameBufferSize);

    /// <summary>
    /// Retrieve a DVB service's name from the grabber.
    /// </summary>
    /// <param name="serviceIndex">The service's index. Should be in the range 0 to GetServiceCount() - 1.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="providerName">A buffer containing the service provider's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="providerNameBufferSize">As an input, the size of the <paramref name="providerName">provider name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="serviceName">A buffer containing the service's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="serviceNameBufferSize">As an input, the size of the <paramref name="serviceName">service name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the service's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetServiceNameByLanguage(ushort serviceIndex,
                                  Iso639Code language,
                                  IntPtr providerName,
                                  ref ushort providerNameBufferSize,
                                  IntPtr serviceName,
                                  ref ushort serviceNameBufferSize);

    /// <summary>
    /// Get the number of names received by the grabber for a given DVB network.
    /// </summary>
    /// <param name="networkId">The network's identifier.</param>
    /// <returns>the number of names received by the grabber for the given DVB network</returns>
    [PreserveSig]
    byte GetNetworkNameCount(ushort networkId);

    /// <summary>
    /// Retrieve a DVB network's name from the grabber.
    /// </summary>
    /// <param name="networkId">The network's identifier.</param>
    /// <param name="index">The index of the name to retrieve. Should be in the range 0 to GetNetworkNameCount() - 1 for the network.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="name">A buffer containing the network's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the network's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetNetworkNameByIndex(ushort networkId,
                                byte index,
                                out Iso639Code language,
                                IntPtr name,
                                ref ushort nameBufferSize);

    /// <summary>
    /// Retrieve a DVB network's name from the grabber.
    /// </summary>
    /// <param name="networkId">The network's identifier.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="name">A buffer containing the network's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the network's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetNetworkNameByLanguage(ushort networkId,
                                  Iso639Code language,
                                  IntPtr name,
                                  ref ushort nameBufferSize);

    /// <summary>
    /// Get the number of names received by the grabber for a given DVB bouquet.
    /// </summary>
    /// <param name="bouquetId">The bouquet's identifier.</param>
    /// <returns>the number of names received by the grabber for the given DVB bouquet</returns>
    [PreserveSig]
    byte GetBouquetNameCount(ushort bouquetId);

    /// <summary>
    /// Retrieve a DVB bouquet's name from the grabber.
    /// </summary>
    /// <param name="bouquetId">The bouquet's identifier.</param>
    /// <param name="index">The index of the name to retrieve. Should be in the range 0 to GetBouquetNameCount() - 1 for the bouquet.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="name">A buffer containing the bouquet's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the bouquet's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetBouquetNameByIndex(ushort bouquetId,
                                byte index,
                                out Iso639Code language,
                                IntPtr name,
                                ref ushort nameBufferSize);

    /// <summary>
    /// Retrieve a DVB bouquet's name from the grabber.
    /// </summary>
    /// <param name="bouquetId">The bouquet's identifier.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="name">A buffer containing the bouquet's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the bouquet's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetBouquetNameByLanguage(ushort bouquetId,
                                  Iso639Code language,
                                  IntPtr name,
                                  ref ushort nameBufferSize);

    /// <summary>
    /// Get the number of names received by the grabber for a given DVB target region.
    /// </summary>
    /// <param name="regionId">The target region's identifier.</param>
    /// <returns>the number of names received by the grabber for the given DVB target region</returns>
    [PreserveSig]
    byte GetTargetRegionNameCount(ulong regionId);

    /// <summary>
    /// Retrieve a DVB target region's name from the grabber.
    /// </summary>
    /// <param name="regionId">The target region's identifier.</param>
    /// <param name="index">The index of the name to retrieve. Should be in the range 0 to GetTargetRegionNameCount() - 1 for the target region.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="name">A buffer containing the target region's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the target region's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetTargetRegionNameByIndex(ulong regionId,
                                    byte index,
                                    out Iso639Code language,
                                    IntPtr name,
                                    ref ushort nameBufferSize);

    /// <summary>
    /// Retrieve a DVB target region's name from the grabber.
    /// </summary>
    /// <param name="regionId">The target region's identifier.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="name">A buffer containing the target region's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the target region's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetTargetRegionNameByLanguage(ulong regionId,
                                        Iso639Code language,
                                        IntPtr name,
                                        ref ushort nameBufferSize);

    /// <summary>
    /// Get the number of names received by the grabber for a given Freesat region.
    /// </summary>
    /// <param name="regionId">The Freesat region's identifier.</param>
    /// <returns>the number of names received by the grabber for the given Freesat region</returns>
    [PreserveSig]
    byte GetFreesatRegionNameCount(ushort regionId);

    /// <summary>
    /// Retrieve a Freesat region's name from the grabber.
    /// </summary>
    /// <param name="regionId">The Freesat region's identifier.</param>
    /// <param name="index">The index of the name to retrieve. Should be in the range 0 to GetFreesatRegionNameCount() - 1 for the Freesat region.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="name">A buffer containing the Freesat region's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the Freesat region's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetFreesatRegionNameByIndex(ushort regionId,
                                      byte index,
                                      out Iso639Code language,
                                      IntPtr name,
                                      ref ushort nameBufferSize);

    /// <summary>
    /// Retrieve a Freesat region's name from the grabber.
    /// </summary>
    /// <param name="regionId">The Freesat region's identifier.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="name">A buffer containing the Freesat region's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the Freesat region's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetFreesatRegionNameByLanguage(ushort regionId,
                                        Iso639Code language,
                                        IntPtr name,
                                        ref ushort nameBufferSize);

    /// <summary>
    /// Get the number of names received by the grabber for a given Freesat channel category.
    /// </summary>
    /// <param name="categoryId">The Freesat channel category's identifier.</param>
    /// <returns>the number of names received by the grabber for the given Freesat channel category</returns>
    [PreserveSig]
    byte GetFreesatChannelCategoryNameCount(ushort categoryId);

    /// <summary>
    /// Retrieve a Freesat channel category's name from the grabber.
    /// </summary>
    /// <param name="categoryId">The Freesat channel category's identifier.</param>
    /// <param name="index">The index of the name to retrieve. Should be in the range 0 to GetFreesatChannelCategoryNameCount() - 1 for the Freesat channel category.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="name">A buffer containing the Freesat channel category's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the Freesat channel category's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetFreesatChannelCategoryNameByIndex(ushort categoryId,
                                              byte index,
                                              out Iso639Code language,
                                              IntPtr name,
                                              ref ushort nameBufferSize);

    /// <summary>
    /// Retrieve a Freesat channel category's name from the grabber.
    /// </summary>
    /// <param name="categoryId">The Freesat channel category's identifier.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="name">A buffer containing the Freesat channel category's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the Freesat channel category's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetFreesatChannelCategoryNameByLanguage(ushort categoryId,
                                                  Iso639Code language,
                                                  IntPtr name,
                                                  ref ushort nameBufferSize);

    /// <summary>
    /// Get the number of names received by the grabber for a given NorDig channel list.
    /// </summary>
    /// <param name="channelListId">The NorDig channel list's identifier.</param>
    /// <returns>the number of names received by the grabber for the given NorDig channel list</returns>
    [PreserveSig]
    byte GetNorDigChannelListNameCount(byte channelListId);

    /// <summary>
    /// Retrieve a NorDig channel list's name from the grabber.
    /// </summary>
    /// <param name="channelListId">The NorDig channel list's identifier.</param>
    /// <param name="index">The index of the name to retrieve. Should be in the range 0 to GetNorDigChannelListNameCount() - 1 for the NorDig channel list.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="name">A buffer containing the NorDig channel list's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the NorDig channel list's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetNorDigChannelListNameByIndex(byte channelListId,
                                          byte index,
                                          out Iso639Code language,
                                          IntPtr name,
                                          ref ushort nameBufferSize);

    /// <summary>
    /// Retrieve a NorDig channel list's name from the grabber.
    /// </summary>
    /// <param name="channelListId">The NorDig channel list's identifier.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="name">A buffer containing the NorDig channel list's name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the NorDig channel list's name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetNorDigChannelListNameByLanguage(byte channelListId,
                                            Iso639Code language,
                                            IntPtr name,
                                            ref ushort nameBufferSize);

    /// <summary>
    /// Get the number of DVB transmitters received by the grabber.
    /// </summary>
    /// <returns>the number of DVB transmitters received by the grabber</returns>
    [PreserveSig]
    ushort GetTransmitterCount();

    /// <summary>
    /// Retrieve a DVB transmitter's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the transmitter to retrieve. Should be in the range 0 to GetTransmitterCount() - 1.</param>
    /// <param name="tableId">The identifier of the table from which the transmitter was received. Value is <c>0x40</c> for NIT-actual or <c>0x41</c> for NIT-other.</param>
    /// <param name="networkId">The identifier of the network that the transmitter is associated with.</param>
    /// <param name="originalNetworkId">The identifier of the original network that the transmitter is associated with.</param>
    /// <param name="transportStreamId">The identifier of the transport stream that the transmitter is associated with.</param>
    /// <param name="isHomeTransmitter">An indication of whether the transmitter is a "home" transmitter (ie. transmits complete network/bouquet SI).</param>
    /// <param name="broadcastStandard">The broadcast standard that the transmitter is associated with. This field indicates which other fields will be populated.</param>
    /// <param name="frequencies">The frequencies which the transmitter operates at. The unit is kilo-Hertz (kHz). The caller must allocate this array.</param>
    /// <param name="frequencyCount">As an input, the size of the <paramref name="frequencies"/> array; as an output, the consumed array size.</param>
    /// <param name="polarisation">The transmitter's polarisation. Only applicable for DVB-S and DVB-S2 transmitters.</param>
    /// <param name="modulation">The transmitter's modulation scheme. Only applicable for DVB-C, DVB-S and DVB-S2 transmitters.</param>
    /// <param name="symbolRate">The transmitter's symbol rate. The unit is kilo-symbols-per-second (ks/s). Only applicable for DVB-C, DVB-S and DVB-S2 transmitters.</param>
    /// <param name="bandwidth">The transmitter's bandwith. The unit is kilo-Hertz (kHz). Only applicable for DVB-C2, DVB-T and DVB-T2 transmitters.</param>
    /// <param name="innerFecRate">The transmitter's inner FEC rate. Only applicable for DVB-S and DVB-S2 transmitters.</param>
    /// <param name="rollOffFactor">The transmitter's roll-off factor. Only applicable for DVB-S2 transmitters.</param>
    /// <param name="longitude">The longitude of the geostationary satellite on which the transmitter is located. The unit is tenths of a degree. Positive values indicate Eastern hemisphere. Only applicable for DVB-S and DVB-S2 transmitters.</param>
    /// <param name="cellId">The identifier of the terrestrial cell in which the transmitter is located. Only applicable for DVB-T and DVB-T2 transmitters.</param>
    /// <param name="cellIdExtension">The identifier extension of the terrestrial cell in which the transmitter is located. Only applicable for DVB-T and DVB-T2 transmitters.</param>
    /// <param name="isMultipleInputStream">An indication of whether the transmitter broadcasts multiple transport streams. Only applicable for DVB-C2, DVB-S2 and DVBT2 transmitters.</param>
    /// <param name="plpId">The identifier of the physical layer pipe in which the <paramref name="transportStreamId">transport stream</paramref> is multiplexed. Only applicable for DVB-C2, DVB-S2 and DVB-T2 transmitters.</param>
    /// <returns><c>true</c> if the transmitter's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool GetTransmitter(ushort index,
                        out byte tableId,
                        out ushort networkId,
                        out ushort originalNetworkId,
                        out ushort transportStreamId,
                        [MarshalAs(UnmanagedType.I1)] out bool isHomeTransmitter,
                        out BroadcastStandard broadcastStandard,
                        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 8)] uint[] frequencies,
                        ref byte frequencyCount,
                        out Polarisation polarisation,
                        out byte modulation,
                        out uint symbolRate,
                        out ushort bandwidth,
                        out FecCodeRateDvbCS innerFecRate,
                        out RollOffFactor rollOffFactor,
                        out short longitude,
                        out ushort cellId,
                        out byte cellIdExtension,
                        [MarshalAs(UnmanagedType.I1)] out bool isMultipleInputStream,
                        out byte plpId);
  }
}