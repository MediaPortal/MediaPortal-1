#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum
{
  /// <summary>
  /// MPEG, DVB and ATSC stream types.
  /// </summary>
  public enum StreamType
  {
    #region MPEG ISO/IEC 13818-1

    /// <summary>
    /// MPEG ISO/IEC 11172-2 video (MPEG 1 video)
    /// </summary>
    Mpeg1Part2Video = 0x01,
    /// <summary>
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2
    /// constrained parameter video stream (MPEG 2 video)
    /// </summary>
    Mpeg2Part2Video = 0x02,
    /// <summary>
    /// MPEG ISO/IEC 11172-3 audio (MPEG 1 audio)
    /// </summary>
    Mpeg1Part3Audio = 0x03,
    /// <summary>
    /// MPEG ISO/IEC 13818-3 audio (MPEG 2 audio)
    /// </summary>
    Mpeg2Part3Audio = 0x04,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 private_sections
    /// </summary>
    Mpeg2Part1PrivateSection = 0x05,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing
    /// private data
    /// </summary>
    Mpeg2Part1PrivateData = 0x06,
    /// <summary>
    /// MPEG ISO/IEC 13522 MHEG
    /// </summary>
    Mheg5 = 0x07,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Annex A DSM-CC
    /// </summary>
    AnnexADsmCc = 0x08,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.1
    /// </summary>
    MediaMultiplex = 0x09,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type A - multi-protocol encapsulation
    /// </summary>
    Mpeg2Part6DsmCcMultiProtocolEncapsulation = 0x0a,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type B - DSM-CC U-N messages
    /// </summary>
    Mpeg2Part6DsmCcUnMessages = 0x0b,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type C - DSM-CC stream descriptors
    /// </summary>
    Mpeg2Part6DsmCcStreamDescriptor = 0x0c,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type D - DSM-CC sections (any type, including
    /// private data) or DSM-CC addressable sections
    /// </summary>
    Mpeg2Part6DsmCcSection = 0x0d,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 auxiliary
    /// </summary>
    Mpeg2Part1Auxiliary = 0x0e,
    /// <summary>
    /// MPEG ISO/IEC 13818-7 audio with ADTS transport syntax (AAC audio)
    /// </summary>
    Mpeg2Part7Audio = 0x0f,
    /// <summary>
    /// MPEG ISO/IEC 14496-2 visual (MPEG 4 video)
    /// </summary>
    Mpeg4Part2Video = 0x10,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio with the LATM transport syntax as defined in
    /// ISO/IEC 14496-3 (LATM-AAC audio)
    /// </summary>
    Mpeg4Part3AudioTransportLatm = 0x11,
    /// <summary>
    /// MPEG ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in
    /// PES packets
    /// </summary>
    Mpeg4Part1PesPacketisedStream = 0x12,
    /// <summary>
    /// MPEG ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in
    /// ISO/IEC 14496_sections
    /// </summary>
    Mpeg4Part1PacketisedStream = 0x13,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 synchronized download protocol
    /// </summary>
    Mpeg2Part6SynchronisedDownloadProtocol = 0x14,
    /// <summary>
    /// MPEG metadata carried in PES packets
    /// </summary>
    PesMetadata = 0x15,
    /// <summary>
    /// MPEG metadata carried in metadata_sections
    /// </summary>
    SectionMetadata = 0x16,
    /// <summary>
    /// MPEG metadata carried in ISO/IEC 13818-6 data carousel
    /// </summary>
    Mpeg2Part6DataCarousel = 0x17,
    /// <summary>
    /// MPEG metadata carried in ISO/IEC 13818-6 object carousel
    /// </summary>
    Mpeg2Part6ObjectCarousel = 0x18,
    /// <summary>
    /// MPEG metadata carried in ISO/IEC 13818-6 synchronized download protocol
    /// </summary>
    Mpeg2Part6SynchronisedDownloadProtocolMetadata = 0x19,
    /// <summary>
    /// MPEG IPMP stream (defined in ISO/IEC 13818-11, MPEG-2 IPMP)
    /// </summary>
    Mpeg2Part11Ipmp = 0x1a,
    /// <summary>
    /// MPEG AVC video stream conforming to one or more profiles defined in
    /// Annex A of ITU-T Rec. H.264 | ISO/IEC 14496-10 or AVC video
    /// sub-bitstream of SVC as defined in 2.1.78 or MVC base view sub
    /// bitstream, as defined in 2.1.85, or AVC video sub-bitstream of MVC, as
    /// defined in 2.1.88 or MVCD base view sub bitstream, as defined in
    /// 2.1.97, or AVC video sub-bitstream of MVCD, as defined in 2.1.100
    /// (H.264 video)
    /// </summary>
    Mpeg4Part10Video = 0x1b,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio, without using any additional transport
    /// syntax, such as DST, ALS and SLS
    /// </summary>
    Mpeg4Part3AudioTransportNone = 0x1c,
    /// <summary>
    /// MPEG ISO/IEC 14496-17 text
    /// </summary>
    Mpeg4Part17Subtitles = 0x1d,
    /// <summary>
    /// MPEG auxiliary video stream as defined in ISO/IEC 23002-3
    /// </summary>
    MpegCAuxiliaryVideo = 0x1e,
    /// <summary>
    /// MPEG SVC video sub-bitstream of an AVC video stream conforming to one
    /// or more profiles defined in Annex G of ITU-T Rec. H.264 | ISO/IEC
    /// 14496-10
    /// </summary>
    Mpeg4Part10AnnexGVideoSvc = 0x1f,
    /// <summary>
    /// MPEG MVC video sub-bitstream of an AVC video stream conforming to one
    /// or more profiles defined in Annex H of ITU-T Rec. H.264 | ISO/IEC
    /// 14496-10
    /// </summary>
    Mpeg4Part10AnnexHVideoMvc = 0x20,
    /// <summary>
    /// MPEG video stream conforming to one or more profiles as defined in
    /// ITU-T Rec T.800 | ISO/IEC 15444-1
    /// </summary>
    JpegVideo = 0x21,
    /// <summary>
    /// MPEG additional view ITU-T Rec. H.262 | ISO/IEC 13818-2 video stream
    /// for service-compatible stereoscopic 3D services
    /// </summary>
    Mpeg2Part2VideoStereoscopicAdditionalView = 0x22,
    /// <summary>
    /// MPEG additional view ITU-T Rec. H.264 | ISO/IEC 14496-10 video stream
    /// conforming to one or more profiles defined in Annex A for
    /// service-compatible stereoscopic 3D services
    /// </summary>
    Mpeg4Part10VideoStereoscopicAdditionalView = 0x23,
    /// <summary>
    /// MPEG ITU-T Rec. H.265 | ISO/IEC 23008-2 visual
    /// </summary>
    MpegHPart2 = 0x24,

    // 0x25 - 0x7e  ITU-T Rec. H.222.0 | ISO/IEC 13818-1 reserved

    /// <summary>
    /// MPEG IPMP stream
    /// </summary>
    Ipmp = 0x7f,

    #endregion

    #region ATSC A/53 part 3

    /// <summary>
    /// ATSC AC3 audio
    /// </summary>
    Ac3Audio = 0x81,
    /// <summary>
    /// ATSC E-AC3 audio
    /// </summary>
    EnhancedAc3Audio = 0x87,

    #endregion

    #region ATSC 2.0 A/107

    /// <summary>
    /// ATSC DTS HD audio
    /// </summary>
    DtsHdAudio = 0x88,

    #endregion

    #region SCTE

    /// <summary>
    /// DigiCipher II video, identical to MPEG ITU-T Rec. H.262 | ISO/IEC
    /// 13818-2
    /// </summary>
    /// <remarks>
    /// This stream type has been known to conflict with private elementary
    /// streams in DVB transport streams. See mantis #3950.
    /// </remarks>
    DigiCipher2Video = 0x80,
    /// <summary>
    /// SCTE subtitles (SCTE 27)
    /// </summary>
    Subtitles = 0x82

    #endregion
  }
}