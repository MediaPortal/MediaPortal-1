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
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// An SCTE service information grabber interface.
  /// </summary>
  [Guid("e39e7902-650c-431c-81f1-5330154b75c4"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IGrabberSiScte : IGrabberSiAtsc
  {
    /// <summary>
    /// Set the grabber's call back delegate.
    /// </summary>
    /// <param name="callBack">The delegate.</param>
    [PreserveSig]
    new void SetCallBack(ICallBackGrabber callBack);

    /// <summary>
    /// Check if the grabber has received any long-form virtual channel table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more LVCT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool IsSeenLvct();

    /// <summary>
    /// Check if the grabber has received all available long-form virtual channel table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available LVCT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool IsReadyLvct();

    /// <summary>
    /// Get the number of SCTE channels received by the grabber from the long-form virtual channel table.
    /// </summary>
    /// <returns>the number of SCTE channels received by the grabber from the LVCT</returns>
    [PreserveSig]
    new ushort GetLvctChannelCount();

    /// <summary>
    /// Retrieve an SCTE long-form virtual channel's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the channel to retrieve. Should be in the range 0 to GetLvctChannelCount() - 1.</param>
    /// <param name="tableId">The identifier of the table from which the channel was received. Value is <c>0xc8</c> for SCTE terrestrial LVCT or <c>0xc9</c> for SCTE/SCTE cable LVCT.</param>
    /// <param name="sectionTransportStreamId">The identifier of the transport stream that the channel information was received from. Only applicable for channels defined in tables received via the in-band channel. Refer to ATSC specifications for further detail.</param>
    /// <param name="mapId">The identifier for this long-form virtual channel table. Only applicable for cable channels defined in tables received via the out-of-band channel. Refer to SCTE specifications for further detail.</param>
    /// <param name="shortName">A buffer containing the channel's short name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="shortNameBufferSize">As an input, the size of the <paramref name="shortName">short name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="longNameCount">The number of languages in which the channel's long name is available.</param>
    /// <param name="majorChannelNumber">The channel's major number.</param>
    /// <param name="minorChannelNumber">The channel's minor number, if any.</param>
    /// <param name="modulationMode">The modulation mode of the channel's transmitter, encoded according to SCTE specifications.</param>
    /// <param name="carrierFrequency">The frequency at which the channel's transmitter operates. The unit is kilo-Hertz (kHz).</param>
    /// <param name="transportStreamId">The identifier of the transport stream that the channel is associated with.</param>
    /// <param name="programNumber">The channel's SCTE/MPEG 2 identifier. Only unique when combined with the <paramref name="transportStreamId">transport stream ID</paramref>.</param>
    /// <param name="etmLocation">The channel's extended text message location, encoded according to SCTE specifications.</param>
    /// <param name="accessControlled">An indication of whether the channel's sub-streams are encrypted or not.</param>
    /// <param name="hidden">An indication of whether the channel is hidden. Refer to SCTE specifications for further detail.</param>
    /// <param name="pathSelect">The physical transmission path that the channel is associated with. Only applicable for cable channels. Refer to SCTE specifications for further detail.</param>
    /// <param name="outOfBand">An indication of whether the channel is transmitted in an out-of-band transmission path. Only applicable for cable channels. Refer to SCTE specifications for further detail.</param>
    /// <param name="hideGuide">An indication of whether the channel is intended to be visible in the electronic programme guide.</param>
    /// <param name="serviceType">The channel's type, encoded according to SCTE specifications.</param>
    /// <param name="sourceId">The identifier of the source that the channel is associated with. Refer to SCTE specifications for further detail.</param>
    /// <param name="streamCountVideo">The number of video streams associated with the channel.</param>
    /// <param name="streamCountAudio">The number of audio streams associated with the channel.</param>
    /// <param name="isThreeDimensional">An indication of whether the channel's video is three dimensional.</param>
    /// <param name="audioLanguages">The languages in which the channel's audio will be available. The caller must allocate this array.</param>
    /// <param name="audioLanguageCount">As an input, the size of the <paramref name="audioLanguages">audio languages array</paramref>; as an output, the consumed array size.</param>
    /// <param name="captionsLanguages">The languages in which the channel's captions will be available. The caller must allocate this array.</param>
    /// <param name="captionsLanguageCount">As an input, the size of the <paramref name="captionsLanguages">captions languages array</paramref>; as an output, the consumed array size.</param>
    /// <returns><c>true</c> if the channel's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool GetLvctChannel(ushort index,
                            out byte tableId,
                            out ushort sectionTransportStreamId,
                            out ushort mapId,
                            IntPtr shortName,
                            ref ushort shortNameBufferSize,
                            out byte longNameCount,
                            out ushort majorChannelNumber,
                            out ushort minorChannelNumber,
                            out ModulationMode modulationMode,
                            out uint carrierFrequency,
                            out ushort transportStreamId,
                            out ushort programNumber,
                            out EtmLocation etmLocation,
                            [MarshalAs(UnmanagedType.I1)] out bool accessControlled,
                            [MarshalAs(UnmanagedType.I1)] out bool hidden,
                            out PathSelect pathSelect,
                            [MarshalAs(UnmanagedType.I1)] out bool outOfBand,
                            [MarshalAs(UnmanagedType.I1)] out bool hideGuide,
                            out ServiceType serviceType,
                            out ushort sourceId,
                            out byte streamCountVideo,
                            out byte streamCountAudio,
                            [MarshalAs(UnmanagedType.I1)] out bool isThreeDimensional,
                            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 24)] Iso639Code[] audioLanguages,
                            ref byte audioLanguageCount,
                            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 26)] Iso639Code[] captionsLanguages,
                            ref byte captionsLanguageCount);

    /// <summary>
    /// Retrieve an SCTE long-form virtual channel's long name from the grabber.
    /// </summary>
    /// <param name="channelIndex">The channel's index. Should be in the range 0 to GetLvctChannelCount() - 1.</param>
    /// <param name="nameIndex">The index of the name to retrieve. Should be in the range 0 to longNameCount - 1 for the channel.</param>
    /// <param name="language">The name's language.</param>
    /// <param name="name">A buffer containing the channel's long name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">long name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the channel's long name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool GetLvctChannelLongNameByIndex(ushort channelIndex,
                                            byte nameIndex,
                                            out Iso639Code language,
                                            IntPtr name,
                                            ref ushort nameBufferSize);

    /// <summary>
    /// Retrieve an SCTE long-form virtual channel's long name from the grabber.
    /// </summary>
    /// <param name="channelIndex">The channel's index. Should be in the range 0 to GetLvctChannelCount() - 1.</param>
    /// <param name="language">The language of the name to retrieve.</param>
    /// <param name="name">A buffer containing the channel's long name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="nameBufferSize">As an input, the size of the <paramref name="name">long name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <returns><c>true</c> if the channel's long name is successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool GetLvctChannelLongNameByLanguage(ushort channelIndex,
                                              Iso639Code language,
                                              IntPtr name,
                                              ref ushort nameBufferSize);

    /// <summary>
    /// Check if the grabber has received any short-form virtual channel table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received one or more SVCT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool IsSeenSvct();

    /// <summary>
    /// Check if the grabber has received all available short-form virtual channel table sections.
    /// </summary>
    /// <returns><c>true</c> if the grabber has received all available SVCT sections, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool IsReadySvct();

    /// <summary>
    /// Get the number of SCTE virtual channels received by the grabber from the short-form virtual channel table.
    /// </summary>
    /// <returns>the number of SCTE virtual channels received by the grabber from the SVCT</returns>
    [PreserveSig]
    new ushort GetSvctVirtualChannelCount();

    /// <summary>
    /// Retrieve an SCTE short-form virtual channel's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the channel to retrieve. Should be in the range 0 to GetSvctVirtualChannelCount() - 1.</param>
    /// <param name="transmissionMedium">The medium which is used to transmit the channel.</param>
    /// <param name="vctId">The identifier of the virtual channel table which the channel is associated with.</param>
    /// <param name="mapNameLanguage">The language of the <paramref name="mapName">map/VCT's name</paramref>.</param>
    /// <param name="mapName">A buffer containing the map name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="mapNameBufferSize">As an input, the size of the <paramref name="mapName">map name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="splice">An indication of whether a splice is point is anticipated. Refer to SCTE specifications for further detail.</param>
    /// <param name="activationTime">The time at which the channel's details become valid. Also known as the splice point.</param>
    /// <param name="hdtvChannel">An indication of whether the channel's video is high definition.</param>
    /// <param name="preferredSource">An indication of whether a cable source for the channel is preferred over a satellite source.</param>
    /// <param name="applicationVirtualChannel">An indication of whether the channel is an application.</param>
    /// <param name="majorChannelNumber">The channel's major number.</param>
    /// <param name="minorChannelNumber">The channel's minor number, if any.</param>
    /// <param name="sourceId">The identifier of the source that the channel is associated with. Refer to SCTE specifications for further detail.</param>
    /// <param name="sourceNameLanguage">The language of the <paramref name="sourceName">source/channel's name</paramref>.</param>
    /// <param name="sourceName">A buffer containing the source name, encoded as DVB-compatible text. The caller must allocate and free this buffer.</param>
    /// <param name="sourceNameBufferSize">As an input, the size of the <paramref name="sourceName">source name buffer</paramref>; as an output, the consumed buffer size.</param>
    /// <param name="accessControlled">An indication of whether the channel's sub-streams are encrypted or not.</param>
    /// <param name="hideGuide">An indication of whether the channel is intended to be visible in the electronic programme guide.</param>
    /// <param name="serviceType">The channel's service type, encoded according to SCTE specifications.</param>
    /// <param name="outOfBand">An indication of whether the channel is transmitted in an out-of-band transmission path. Only applicable for cable channels. Refer to SCTE specifications for further detail.</param>
    /// <param name="bitstreamSelect">The bitstream (I or Q) that the channel's transport stream is associated with, if applicable.</param>
    /// <param name="pathSelect">The physical transmission path that the channel is associated with. Only applicable for cable channels. Refer to SCTE specifications for further detail.</param>
    /// <param name="channelType">The channel's type, encoded according to SCTE specifications. This field partially indicates which other fields will be populated.</param>
    /// <param name="nvodChannelBase">The index of the first hidden channel in a near-video-on-demand group. Only applicable for NVOD access channels.</param>
    /// <param name="transportType">The type of transport (eg. digital MPEG 2 TS, analog) used to transmit the channel, encoded according to SCTE specifications.</param>
    /// <param name="wideBandwidthVideo">An indication of whether the channel's video bandwidth is 10.75 MHz or 8.2 MHz. Not applicable for MPEG 2 transport.</param>
    /// <param name="waveformStandard">The waveform standard of the channel's transmission, encoded according to SCTE specifications. Not applicable for MPEG 2 transport.</param>
    /// <param name="videoStandard">The channel's video standard, encoded according to SCTE specifications. Not applicable for MPEG 2 transport.</param>
    /// <param name="wideBandwidthAudio">An indication of whether the channel's audio bandwidth is wide or standard. Not applicable for MPEG 2 transport.</param>
    /// <param name="compandedAudio">An indication of whether the channel's audio waveform is companded (compressed dynamic range). Not applicable for MPEG 2 transport.</param>
    /// <param name="matrixMode">The matrix mode/mapping for the channel's analog audio sub-carriers, encoded according to SCTE specifications. Not applicable for MPEG 2 transport.</param>
    /// <param name="subcarrier2Offset">The offset of the channel's second analog audio sub-carrier, if any. The unit is kilo-Hertz (kHz). Not applicable for MPEG 2 transport.</param>
    /// <param name="subcarrier1Offset">The offset of the channel's primary analog audio sub-carrier. The unit is kilo-Hertz (kHz). Not applicable for MPEG 2 transport.</param>
    /// <param name="suppressVideo">An indication of whether the channel's video should be supressed (ignored). Not applicable for MPEG 2 transport.</param>
    /// <param name="audioSelection">The channel's analog audio sub-carrier selection/configuration, encoded according to SCTE specifications. Not applicable for MPEG 2 transport.</param>
    /// <param name="programNumber">The channel's SCTE/MPEG 2 identifier. Only unique when combined with the <paramref name="transportStreamId">transport stream ID</paramref>. Only applicable for MPEG 2 transport.</param>
    /// <param name="transportStreamId">The identifier of the transport stream that the channel is associated with. Only applicable for MPEG 2 transport.</param>
    /// <param name="satelliteId">The identifier of satellite which the channel is transmitted from. Only applicable for satellite channels.</param>
    /// <param name="satelliteNameLanguage">The language of the satellite's <paramref name="satelliteReferenceName">reference</paramref> and <paramref name="satelliteFullName">full</paramref> names. Only applicable for satellite channels.</param>
    /// <param name="satelliteReferenceName">A buffer containing the satellite's reference name, encoded as DVB-compatible text. The caller must allocate and free this buffer. Only applicable for satellite channels.</param>
    /// <param name="satelliteReferenceNameBufferSize">As an input, the size of the <paramref name="satelliteReferenceName">satellite reference name buffer</paramref>; as an output, the consumed buffer size. Only applicable for satellite channels.</param>
    /// <param name="satelliteFullName">A buffer containing the satellite's full name, encoded as DVB-compatible text. The caller must allocate and free this buffer. Only applicable for satellite channels.</param>
    /// <param name="satelliteFullNameBufferSize">As an input, the size of the <paramref name="satelliteFullName">satellite full name buffer</paramref>; as an output, the consumed buffer size. Only applicable for satellite channels.</param>
    /// <param name="hemisphere">The hemisphere (East or West) in which the satellite that the channel is transmitted from is located. Only applicable for satellite channels.</param>
    /// <param name="orbitalPosition">The longitudinal position of the satellite that the channel is transmitted from. The unit is tenths of a degree. Must be combined with <paramref name="hemisphere"/> to get a fully qualified position. Only applicable for satellite channels.</param>
    /// <param name="youAreHere">An indication of whether the channel details were received from the satellite specified by <paramref name="satelliteId">the satellite identifier</paramref>. Only applicable for satellite channels.</param>
    /// <param name="frequencyBand">The frequency band in which the transponder that the channel is transmitted from operates, encoded according to SCTE specifications. Only applicable for satellite channels.</param>
    /// <param name="outOfService">An indication of whether the satellite specified by <paramref name="satelliteId">the satellite identifier</paramref> is permanently unavailable (ie. retired without replacement). Only applicable for satellite channels.</param>
    /// <param name="polarisationType">The polarisation type (ie. linear or circular) of the transponder that the channel is transmitted from. Only applicable for satellite channels.</param>
    /// <param name="transponderNumber">The identifier of the satellite transponder that the channel is transmitted from. Only applicable for satellite channels.</param>
    /// <param name="transponderNameLanguage">The language of the <paramref name="transponderName">transponder's name</paramref>. Only applicable for satellite channels.</param>
    /// <param name="transponderName">A buffer containing the transponder name, encoded as DVB-compatible text. The caller must allocate and free this buffer. Only applicable for satellite channels.</param>
    /// <param name="transponderNameBufferSize">As an input, the size of the <paramref name="transponderName">transponder name buffer</paramref>; as an output, the consumed buffer size. Only applicable for satellite channels.</param>
    /// <param name="rootTransponder">An indication of whether the transponder that the channel is transmitted from is the satellite's root/home transponder. Only applicable for satellite channels.</param>
    /// <param name="toneSelect">The 22 kHz tone state required to receive the transponder/transmitter that the channel is transmitted from. Only applicable for satellite channels.</param>
    /// <param name="polarisation">The polarisation (ie. horizontal/left or vertical/right) of the transponder that the channel is transmitted from. Only applicable for satellite channels.</param>
    /// <param name="frequency">The frequency at which the channel's transmitter operates. The unit is kilo-Hertz (kHz).</param>
    /// <param name="symbolRate">The symbol rate of the channel's transmitter. The unit is symbols-per-second (s/s).</param>
    /// <param name="transmissionSystem">The transmission system that the channel's transmitter is associated with.</param>
    /// <param name="innerCodingMode">The inner coding mode of the channel's transmitter, encoded according to SCTE specifications.</param>
    /// <param name="splitBitstreamMode">An indication of whether the channel's transmitter is operating in split mode.</param>
    /// <param name="modulationFormat">The modulation format of the channel's transmitter, encoded according to SCTE specifications.</param>
    /// <returns><c>true</c> if the channel's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool GetSvctVirtualChannel(ushort index,
                                    out TransmissionMedium transmissionMedium,
                                    out ushort vctId,
                                    out Iso639Code mapNameLanguage,
                                    IntPtr mapName,
                                    ref ushort mapNameBufferSize,
                                    [MarshalAs(UnmanagedType.I1)] out bool splice,
                                    out uint activationTime,
                                    [MarshalAs(UnmanagedType.I1)] out bool hdtvChannel,
                                    [MarshalAs(UnmanagedType.I1)] out bool preferredSource,
                                    [MarshalAs(UnmanagedType.I1)] out bool applicationVirtualChannel,
                                    out ushort majorChannelNumber,
                                    out ushort minorChannelNumber,
                                    out ushort sourceId,
                                    out Iso639Code sourceNameLanguage,
                                    IntPtr sourceName,
                                    ref ushort sourceNameBufferSize,
                                    [MarshalAs(UnmanagedType.I1)] out bool accessControlled,
                                    [MarshalAs(UnmanagedType.I1)] out bool hideGuide,
                                    out ServiceType serviceType,
                                    [MarshalAs(UnmanagedType.I1)] out bool outOfBand,
                                    out BitstreamSelect bitstreamSelect,
                                    out PathSelect pathSelect,
                                    out ChannelType channelType,
                                    out ushort nvodChannelBase,
                                    out TransportType transportType,
                                    [MarshalAs(UnmanagedType.I1)] out bool wideBandwidthVideo,
                                    out WaveformStandard waveformStandard,
                                    out VideoStandard videoStandard,
                                    [MarshalAs(UnmanagedType.I1)] out bool wideBandwidthAudio,
                                    [MarshalAs(UnmanagedType.I1)] out bool compandedAudio,
                                    out MatrixMode matrixMode,
                                    out ushort subcarrier2Offset,
                                    out ushort subcarrier1Offset,
                                    [MarshalAs(UnmanagedType.I1)] out bool suppressVideo,
                                    out AudioSelection audioSelection,
                                    out ushort programNumber,
                                    out ushort transportStreamId,
                                    out byte satelliteId,
                                    out Iso639Code satelliteNameLanguage,
                                    IntPtr satelliteReferenceName,
                                    ref ushort satelliteReferenceNameBufferSize,
                                    IntPtr satelliteFullName,
                                    ref ushort satelliteFullNameBufferSize,
                                    out Hemisphere hemisphere,
                                    out ushort orbitalPosition,
                                    [MarshalAs(UnmanagedType.I1)] out bool youAreHere,
                                    out FrequencyBand frequencyBand,
                                    [MarshalAs(UnmanagedType.I1)] out bool outOfService,
                                    out PolarisationType polarisationType,
                                    out byte transponderNumber,
                                    out Iso639Code transponderNameLanguage,
                                    IntPtr transponderName,
                                    ref ushort transponderNameBufferSize,
                                    [MarshalAs(UnmanagedType.I1)] out bool rootTransponder,
                                    out ToneSelect toneSelect,
                                    out Polarisation polarisation,
                                    out uint frequency,
                                    out uint symbolRate,
                                    out TransmissionSystem transmissionSystem,
                                    out InnerCodingMode innerCodingMode,
                                    [MarshalAs(UnmanagedType.I1)] out bool splitBitstreamMode,
                                    out ModulationFormat modulationFormat);

    /// <summary>
    /// Get the number of SCTE defined channels received by the grabber from the short-form virtual channel table.
    /// </summary>
    /// <returns>the number of SCTE defined channels received by the grabber from the SVCT</returns>
    [PreserveSig]
    new ushort GetSvctDefinedChannelCount();

    /// <summary>
    /// Retrieve an SCTE defined channel's details from the grabber.
    /// </summary>
    /// <param name="index">The index of the channel to retrieve. Should be in the range 0 to GetSvctDefinedChannelCount() - 1.</param>
    /// <param name="transmissionMedium">The medium which is used to transmit the channel.</param>
    /// <param name="vctId">The identifier of the virtual channel table which the channel is associated with.</param>
    /// <param name="virtualChannelNumber">The channel's virtual channel number.</param>
    /// <returns><c>true</c> if the channel's details are successfully retrieved, otherwise <c>false</c></returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    new bool GetSvctDefinedChannel(ushort index,
                                    out TransmissionMedium transmissionMedium,
                                    out ushort vctId,
                                    out ushort virtualChannelNumber);

    /// <summary>
    /// Pass a section received from the out-of-band channel to the grabber.
    /// </summary>
    /// <param name="sectionData">The section data.</param>
    /// <param name="sectionDataBufferSize">The size of the <paramref name="sectionData">section data buffer</paramref>.</param>
    [PreserveSig]
    void OnOutOfBandSectionReceived([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] sectionData,
                                    ushort sectionDataBufferSize);
  }
}