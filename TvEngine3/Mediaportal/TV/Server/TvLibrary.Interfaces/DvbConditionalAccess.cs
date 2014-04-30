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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  #region enums

  /// <summary>
  /// DVB conditional access PMT list management actions.
  /// </summary>
  public enum CaPmtListManagementAction : byte
  {
    /// <summary>
    /// An item that is neither the first or last in a list of at least three services.
    /// </summary>
    More = 0,
    /// <summary>
    /// First item in a list of at least two services.
    /// </summary>
    First = 1,
    /// <summary>
    /// Last item in a list of at least two services.
    /// </summary>
    Last = 2,
    /// <summary>
    /// The single item in a list.
    /// </summary>
    Only = 3,
    /// <summary>
    /// Add the item to the list.
    /// </summary>
    Add = 4,
    /// <summary>
    /// Update an item in the list.
    /// </summary>
    Update = 5
  }

  /// <summary>
  /// DVB conditional access PMT commands.
  /// </summary>
  public enum CaPmtCommand : byte
  {
    /// <summary>
    /// Descrambling can begin immediately, and MMI dialogues may be opened.
    /// </summary>
    OkDescrambling = 1,
    /// <summary>
    /// Descrambling may not begin yet, but MMI dialogues may be opened.
    /// </summary>
    OkMmi = 2,
    /// <summary>
    /// A CA PMT reply is expected.
    /// </summary>
    Query = 3,
    /// <summary>
    /// Descrambling is not necessary and MMI dialogues are closed.
    /// </summary>
    NotSelected = 4
  }

  /// <summary>
  /// DVB MMI application information application types.
  /// </summary>
  public enum MmiApplicationType : byte
  {
    /// <summary>
    /// Conditional access application.
    /// </summary>
    ConditionalAccess = 1,
    /// <summary>
    /// Electronic programme guide application.
    /// </summary>
    ElectronicProgrammeGuide
  }

  /// <summary>
  /// DVB MMI enquiry answer response types.
  /// </summary>
  public enum MmiResponseType : byte
  {
    /// <summary>
    /// The response is a cancel request.
    /// </summary>
    Cancel = 0,
    /// <summary>
    /// The response contains an answer from the user.
    /// </summary>
    Answer
  }

  /// <summary>
  /// DVB MMI close MMI command types.
  /// </summary>
  public enum MmiCloseType : byte
  {
    /// <summary>
    /// The MMI dialog should be closed immediately.
    /// </summary>
    Immediate = 0,
    /// <summary>
    /// The MMI dialog should be closed after a [short] delay.
    /// </summary>
    Delayed
  }

  /// <summary>
  /// DVB MMI message tags.
  /// </summary>
  public enum MmiTag
  {
    /// <summary>
    /// Unknown tag.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Profile enquiry.
    /// </summary>
    ProfileEnquiry = 0x9f8010,
    /// <summary>
    /// Profile.
    /// </summary>
    Profile,
    /// <summary>
    /// Profile change.
    /// </summary>
    ProfileChange,

    /// <summary>
    /// Application information enquiry.
    /// </summary>
    ApplicationInfoEnquiry = 0x9f8020,
    /// <summary>
    /// Application information.
    /// </summary>
    ApplicationInfo,
    /// <summary>
    /// Enter menu.
    /// </summary>
    EnterMenu,

    /// <summary>
    /// Conditional access information enquiry.
    /// </summary>
    ConditionalAccessInfoEnquiry = 0x9f8030,
    /// <summary>
    /// Conditional access information.
    /// </summary>
    ConditionalAccessInfo,
    /// <summary>
    /// Conditional access information program map table.
    /// </summary>
    ConditionalAccessPmt,
    /// <summary>
    /// Conditional access information program map table response.
    /// </summary>
    ConditionalAccessPmtResponse,

    /// <summary>
    /// Tune.
    /// </summary>
    Tune = 0x9f8400,
    /// <summary>
    /// Replace.
    /// </summary>
    Replace,
    /// <summary>
    /// Clear replace.
    /// </summary>
    ClearReplace,
    /// <summary>
    /// Ask release.
    /// </summary>
    AskRelease,

    /// <summary>
    /// Date/time enquiry.
    /// </summary>
    DateTimeEnquiry = 0x9f8440,
    /// <summary>
    /// Date/time.
    /// </summary>
    DateTime,

    /// <summary>
    /// Close man-machine interface.
    /// </summary>
    CloseMmi = 0x9f8800,
    /// <summary>
    /// Display control.
    /// </summary>
    DisplayControl,
    /// <summary>
    /// Display reply.
    /// </summary>
    DisplayReply,
    /// <summary>
    /// Text - last.
    /// </summary>
    TextLast,
    /// <summary>
    /// Text - more.
    /// </summary>
    TextMore,
    /// <summary>
    /// Keypad control.
    /// </summary>
    KeypadControl,
    /// <summary>
    /// Key press.
    /// </summary>
    KeyPress,
    /// <summary>
    /// Enquiry.
    /// </summary>
    Enquiry,
    /// <summary>
    /// Answer.
    /// </summary>
    Answer,
    /// <summary>
    /// Menu - last.
    /// </summary>
    MenuLast,
    /// <summary>
    /// Menu - more.
    /// </summary>
    MenuMore,
    /// <summary>
    /// Menu answer.
    /// </summary>
    MenuAnswer,
    /// <summary>
    /// List - last.
    /// </summary>
    ListLast,
    /// <summary>
    /// List - more.
    /// </summary>
    ListMore,
    /// <summary>
    /// Subtitle segment - last.
    /// </summary>
    SubtitleSegmentLast,
    /// <summary>
    /// Subtitle segment - more.
    /// </summary>
    SubtitleSegmentMore,
    /// <summary>
    /// Display message.
    /// </summary>
    DisplayMessage,
    /// <summary>
    /// Scene end mark.
    /// </summary>
    SceneEndMark,
    /// <summary>
    /// Scene done.
    /// </summary>
    SceneDone,
    /// <summary>
    /// Scene control.
    /// </summary>
    SceneControl,
    /// <summary>
    /// Subtitle download - last.
    /// </summary>
    SubtitleDownloadLast,
    /// <summary>
    /// Subtitle download - more.
    /// </summary>
    SubtitleDownloadMore,
    /// <summary>
    /// Flush download.
    /// </summary>
    FlushDownload,
    /// <summary>
    /// Download reply.
    /// </summary>
    DownloadReply,

    /// <summary>
    /// Communication command.
    /// </summary>
    CommsCommand = 0x9f8c00,
    /// <summary>
    /// Connection descriptor.
    /// </summary>
    ConnectionDescriptor,
    /// <summary>
    /// Communication reply.
    /// </summary>
    CommsReply,
    /// <summary>
    /// Communication send - last.
    /// </summary>
    CommsSendLast,
    /// <summary>
    /// Communication send - more.
    /// </summary>
    CommsSendMore,
    /// <summary>
    /// Communication receive - last.
    /// </summary>
    CommsReceiveLast,
    /// <summary>
    /// Communication receive - more.
    /// </summary>
    CommsReceiveMore
  }

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
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream (MPEG 2 video)
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
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing private data
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
    /// MPEG ISO/IEC 13818-6 type D - DSM-CC sections (any type, including private data) or DSM-CC addressable sections
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
    /// MPEG ISO/IEC 14496-2 visual (MPEG 4)
    /// </summary>
    Mpeg4Part2Video = 0x10,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio with the LATM transport syntax (LATM-AAC audio)
    /// </summary>
    Mpeg4Part3AudioTransportLatm = 0x11,
    /// <summary>
    /// MPEG ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in PES packets
    /// </summary>
    Mpeg4Part1PesPacketisedStream = 0x12,
    /// <summary>
    /// MPEG ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in ISO/IEC 14496_sections
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
    /// MPEG IPMP stream defined in ISO/IEC 13818-11
    /// </summary>
    Mpeg2Part11Ipmp = 0x1a,
    /// <summary>
    /// MPEG AVC video stream as defined in ITU-T Rec. H.264 | ISO/IEC 14496-10 video
    /// </summary>
    Mpeg4Part10Video = 0x1b,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio, without using any additional transport syntax, such as DST, ALS and SLS
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
    /// MPEG SVC video sub-bitstream of an AVC video stream conforming to one or more profiles defined in Annex G of ITU-T Rec. H.264 | ISO/IEC 14496-10
    /// </summary>
    Mpeg4Part10AnnexGVideoSvc = 0x1f,
    /// <summary>
    /// MPEG MVC video sub-bitstream of an AVC video stream conforming to one or more profiles defined in Annex H of ITU-T Rec. H.264 | ISO/IEC 14496-10
    /// </summary>
    Mpeg4Part10AnnexHVideoMvc = 0x20,
    /// <summary>
    /// MPEG video stream conforming to one or more profiles as defined in ITU-T Rec T.800 | ISO/IEC 15444-1
    /// </summary>
    JpegVideo = 0x21,
    /// <summary>
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 video with stereoscopic content coded using frame packing arrangement information
    /// </summary>
    Mpeg2Part2VideoStereoscopicFramePacked = 0x22,
    /// <summary>
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 video with additional video stream for service-compatible stereoscopic services
    /// </summary>
    Mpeg2Part2VideoStereoscopicAdditionalStream = 0x23,
    /// <summary>
    /// MPEG ITU-T Rec. H.264 | ISO/IEC 14496-10 video with additional video stream for service-compatible stereoscopic services
    /// </summary>
    Mpeg4Part10VideoStereoscopicAdditionalStream = 0x24,

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
    /// <summary>
    /// ATSC DTS HD audio
    /// </summary>
    DtsHdAudio = 0x88,

    #endregion

    #region SCTE

    /// <summary>
    /// DigiCipher II video, identical to MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2
    /// </summary>
    /// <remarks>
    /// This stream type has been known to conflict with private elementary streams in DVB transport streams.
    /// See mantis #3950.
    /// </remarks>
    DigiCipher2Video = 0x80,
    /// <summary>
    /// SCTE subtitles (SCTE 27)
    /// </summary>
    Subtitles = 0x82

    #endregion
  }

  /// <summary>
  /// TV Server stream types used for unified standard handling.
  /// </summary>
  /// <remarks>
  /// Standard, de-facto standard or unique stream types are used where possible to simplify the conversion
  /// between stream type and logical stream type.
  /// </remarks>
  public enum LogicalStreamType
  {
    /// <summary>
    /// Unknown stream type.
    /// </summary>
    Unknown = 0,

    #region video stream types

    /// <summary>
    /// MPEG ISO/IEC 11172-2 video (MPEG 1 video)
    /// </summary>
    [Description("MPEG 1 Video")]
    VideoMpeg1 = 0x01,
    /// <summary>
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream (MPEG 2 video)
    /// </summary>
    [Description("MPEG 2 Video")]
    VideoMpeg2 = 0x02,
    /// <summary>
    /// MPEG ISO/IEC 14496-2 visual (MPEG 4)
    /// </summary>
    [Description("MPEG 4 Part 2 Video")]
    VideoMpeg4Part2 = 0x10,
    /// <summary>
    /// MPEG AVC video stream as defined in ITU-T Rec. H.264 | ISO/IEC 14496-10 video
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) Video")]
    VideoMpeg4Part10 = 0x1b,
    /// <summary>
    /// MPEG auxiliary video stream as defined in ISO/IEC 23002-3
    /// </summary>
    [Description("MPEG C Video")]
    VideoMpegC = 0x1e,
    /// <summary>
    /// MPEG SVC video sub-bitstream of an AVC video stream conforming to one or more profiles defined in Annex G of ITU-T Rec. H.264 | ISO/IEC 14496-10
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) Annex G Video")]
    VideoMpeg4Part10AnnexG = 0x1f,
    /// <summary>
    /// MPEG MVC video sub-bitstream of an AVC video stream conforming to one or more profiles defined in Annex H of ITU-T Rec. H.264 | ISO/IEC 14496-10
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) Annex H Video")]
    VideoMpeg4Part10AnnexH = 0x20,
    /// <summary>
    /// MPEG video stream conforming to one or more profiles as defined in ITU-T Rec T.800 | ISO/IEC 15444-1
    /// </summary>
    [Description("JPEG Video")]
    VideoJpeg = 0x21,
    /// <summary>
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 video with stereoscopic content coded using frame packing arrangement information
    /// </summary>
    [Description("MPEG 2 3D Frame-Packed Video")]
    VideoMpeg2StereoscopicFramePacked = 0x22,
    /// <summary>
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 video with additional video stream for service-compatible stereoscopic services
    /// </summary>
    [Description("MPEG 2 3D Video")]
    VideoMpeg2StereoscopicAdditionalStream = 0x23,
    /// <summary>
    /// MPEG ITU-T Rec. H.264 | ISO/IEC 14496-10 video with additional video stream for service-compatible stereoscopic services
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) 3D Video")]
    VideoMpeg4Part10StereoscopicAdditionalStream = 0x24,

    #endregion

    #region audio stream types

    /// <summary>
    /// MPEG ISO/IEC 11172-3 audio (MPEG 1 audio)
    /// </summary>
    [Description("MPEG 1 Audio")]
    AudioMpeg1 = 0x03,
    /// <summary>
    /// MPEG ISO/IEC 13818-3 audio (MPEG 2 audio)
    /// </summary>
    [Description("MPEG 2 Part 3 Audio")]
    AudioMpeg2 = 0x04,
    /// <summary>
    /// MPEG ISO/IEC 13818-7 audio with ADTS transport syntax (AAC audio)
    /// </summary>
    [Description("MPEG 2 Part 7 (ADTS-AAC) Audio")]
    AudioMpeg2Part7 = 0x0f,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio with the LATM transport syntax (LATM-AAC audio)
    /// </summary>
    [Description("MPEG 4 Part 3 (LATM-AAC) Audio")]
    AudioMpeg4Part3Latm = 0x11,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio, without using any additional transport syntax, such as DST, ALS and SLS
    /// </summary>
    [Description("MPEG 4 Part 3 (AAC) Audio")]
    AudioMpeg4Part3 = 0x1c,

    #endregion

    #region non-standard

    /// <summary>
    /// VC-1 video
    /// </summary>
    /// <remarks>
    /// Matches the SCTE stream type. DVB use a registration descriptor to identify VC-1. Use in ATSC is a work in progress (A/73).
    /// </remarks>
    [Description("VC-1 Video")]
    VideoVc1 = 0xea,
    /// <summary>
    /// AC-3/Dolby Digital audio
    /// </summary>
    /// <remarks>
    /// Matches the ATSC and SCTE stream type. DVB has the AC-3 descriptor (tag 0x6a).
    /// </remarks>
    [Description("AC-3 (Dolby Digital) Audio")]
    AudioAc3 = 0x81,
    /// <summary>
    /// Enhanced AC-3/Dolby Digital Plus audio
    /// </summary>
    /// <remarks>
    /// Matches the ATSC and SCTE stream type. DVB has the enhanced AC-3 descriptor (tag 0x7a).
    /// </remarks>
    [Description("Enhanced AC-3 (Dolby Digital Plus) Audio")]
    AudioEnhancedAc3 = 0x87,
    /// <summary>
    /// DTS audio
    /// </summary>
    /// <remarks>
    /// DVB has the DTS descriptor (tag 0x7b). ATSC and SCTE don't support DTS.
    /// </remarks>
    [Description("DTS Audio")]
    AudioDts = 1000,
    /// <summary>
    /// DTS HD audio
    /// </summary>
    /// <remarks>
    /// Matches the ATSC 2.0 stream type. DVB has the DTS-HD descriptor (extended descriptor, tag 0x0e). SCTE has the DTS-HD descriptor (tag 0x7b).
    /// </remarks>
    [Description("DTS-HD Audio")]
    AudioDtsHd = 0x88,
    /// <summary>
    /// Teletext
    /// </summary>
    /// <remarks>
    /// DVB has the teletext descriptor (tag 0x56). ATSC and SCTE don't support teletext.
    /// </remarks>
    Teletext = 1001,
    /// <summary>
    /// Subtitles
    /// </summary>
    /// <remarks>
    /// Matches the SCTE stream type (format probably not supported). DVB has the subtitling descriptor (tag 0x59).
    /// </remarks>
    Subtitles = 0x82

    #endregion
  }

  /// <summary>
  /// MPEG, DVB and ATSC descriptor tags.
  /// </summary>
  public enum DescriptorTag
  {
    #region MPEG ISO/IEC 13818-1

    /// <summary>
    /// Reserved. This value is used as a synonym for "not set".
    /// </summary>
    Reserved = 0,

    /// <summary>
    /// MPEG video stream descriptor
    /// </summary>
    VideoStream = 2,
    /// <summary>
    /// MPEG audio stream descriptor
    /// </summary>
    AudioStream,
    /// <summary>
    /// MPEG hierarchy descriptor
    /// </summary>
    Hierarchy,
    /// <summary>
    /// MPEG registration descriptor
    /// </summary>
    Registration,
    /// <summary>
    /// MPEG data stream alignment descriptor
    /// </summary>
    DataStreamAlignment,
    /// <summary>
    /// MPEG target background grid descriptor
    /// </summary>
    TargetBackgroundGrid,
    /// <summary>
    /// MPEG video window descriptor
    /// </summary>
    VideoWindow,
    /// <summary>
    /// MPEG conditional access descriptor
    /// </summary>
    ConditionalAccess,
    /// <summary>
    /// MPEG ISO 639 language descriptor
    /// </summary>
    Iso639Language,
    /// <summary>
    /// MPEG system clock descriptor
    /// </summary>
    SystemClock,
    /// <summary>
    /// MPEG multiplex buffer utilisation descriptor
    /// </summary>
    MultiplexBufferUtilisation,
    /// <summary>
    /// MPEG copyright descriptor
    /// </summary>
    Copyright,
    /// <summary>
    /// MPEG private data descriptor
    /// </summary>
    PrivateData,
    /// <summary>
    /// MPEG smoothing buffer descriptor
    /// </summary>
    SmoothingBuffer,
    /// <summary>
    /// MPEG STD descriptor
    /// </summary>
    Std,
    /// <summary>
    /// MPEG IBP descriptor
    /// </summary>
    Ibp,

    // 19 - 26 defined in ISO/IEC 13818-6

    /// <summary>
    /// MPEG 4 video descriptor
    /// </summary>
    Mpeg4Video = 27,
    /// <summary>
    /// MPEG 4 audio descriptor
    /// </summary>
    Mpeg4Audio,
    /// <summary>
    /// MPEG IOD descriptor
    /// </summary>
    Iod,
    /// <summary>
    /// MPEG SL descriptor
    /// </summary>
    Sl,
    /// <summary>
    /// MPEG FMC descriptor
    /// </summary>
    Fmc,
    /// <summary>
    /// MPEG external elementary stream ID descriptor
    /// </summary>
    ExternalEsId,
    /// <summary>
    /// MPEG mux code descriptor
    /// </summary>
    MuxCode,
    /// <summary>
    /// MPEG FMX buffer size descriptor
    /// </summary>
    FmxBufferSize,
    /// <summary>
    /// MPEG multiplex buffer descriptor
    /// </summary>
    MultiplexBuffer,
    /// <summary>
    /// MPEG content labeling descriptor
    /// </summary>
    ContentLabeling,
    /// <summary>
    /// MPEG metadata pointer descriptor
    /// </summary>
    MetadataPointer,
    /// <summary>
    /// MPEG metadata descriptor
    /// </summary>
    Metadata,
    /// <summary>
    /// MPEG metadata STD descriptor
    /// </summary>
    MetadataStd,
    /// <summary>
    /// MPEG AVC video descriptor
    /// </summary>
    AvcVideo,
    /// <summary>
    /// MPEG IPMP descriptor
    /// </summary>
    Ipmp,
    /// <summary>
    /// MPEG AVC timing and HRD descriptor
    /// </summary>
    AvcTimingAndHrd,
    /// <summary>
    /// MPEG 2 AAC audio descriptor
    /// </summary>
    Mpeg2AacAudio,
    /// <summary>
    /// MPEG FlexMux timing descriptor
    /// </summary>
    FlexMuxTiming,
    /// <summary>
    /// MPEG 4 text descriptor
    /// </summary>
    Mpeg4Text,
    /// <summary>
    /// MPEG 4 audio extension descriptor
    /// </summary>
    Mpeg4AudioExtension,
    /// <summary>
    /// MPEG auxiliary video stream descriptor
    /// </summary>
    AuxiliaryVideoStream,
    /// <summary>
    /// MPEG SVC extension descriptor
    /// </summary>
    SvcExtension,
    /// <summary>
    /// MPEG MVC extension descriptor
    /// </summary>
    MvcExtension,
    /// <summary>
    /// MPEG J2K video descriptor
    /// </summary>
    J2kVideo,
    /// <summary>
    /// MPEG MVC operation point descriptor
    /// </summary>
    MvcOperationPoint,
    /// <summary>
    /// MPEG 2 stereoscopic video format descriptor
    /// </summary>
    Mpeg2StereoscopicVideoFormat,
    /// <summary>
    /// MPEG stereoscopic program info descriptor
    /// </summary>
    StereoscopicProgramInfo,
    /// <summary>
    /// MPEG stereoscopic video info descriptor
    /// </summary>
    StereoscopicVideoInfo,

    // 55 - 63 ITU-T Rec. H.222.0 | ISO/IEC 13818-1 reserved

    #endregion

    #region DVB ETSI EN 300 468

    /// <summary>
    /// DVB network name descriptor
    /// </summary>
    NetworkName = 0x40,
    /// <summary>
    /// DVB service list descriptor
    /// </summary>
    ServiceList,
    /// <summary>
    /// DVB stuffing descriptor
    /// </summary>
    Stuffing,
    /// <summary>
    /// DVB satellite delivery system descriptor
    /// </summary>
    SatelliteDeliverySystem,
    /// <summary>
    /// DVB cable delivery system descriptor
    /// </summary>
    CableDeliverySystem,
    /// <summary>
    /// DVB vertical blanking interval data descriptor
    /// </summary>
    VbiData,
    /// <summary>
    /// DVB vertical blanking interval teletext descriptor
    /// </summary>
    VbiTeletext,
    /// <summary>
    /// DVB bouquet name descriptor
    /// </summary>
    BouquetName,
    /// <summary>
    /// DVB service descriptor
    /// </summary>
    Service,
    /// <summary>
    /// DVB country availability descriptor
    /// </summary>
    CountryAvailability,
    /// <summary>
    /// DVB linkage descriptor
    /// </summary>
    Linkage,
    /// <summary>
    /// DVB near video on demand reference descriptor
    /// </summary>
    NvodReference,
    /// <summary>
    /// DVB time shifted service descriptor
    /// </summary>
    DvbTimeShiftedService,
    /// <summary>
    /// DVB short event descriptor
    /// </summary>
    ShortEvent,
    /// <summary>
    /// DVB extended event descriptor
    /// </summary>
    ExtendedEvent,
    /// <summary>
    /// DVB time shifted event descriptor
    /// </summary>
    TimeShiftedEvent,
    /// <summary>
    /// DVB component descriptor
    /// </summary>
    Component,
    /// <summary>
    /// DVB mosaic descriptor
    /// </summary>
    Mosaic,
    /// <summary>
    /// DVB stream identifier descriptor. Harmonised with SCTE (ANSI/SCTE 35).
    /// </summary>
    StreamIdentifier,
    /// <summary>
    /// DVB conditional access descriptor
    /// </summary>
    ConditionalAccessIdentifier,
    /// <summary>
    /// DVB content descriptor
    /// </summary>
    Content,
    /// <summary>
    /// DVB parental rating descriptor
    /// </summary>
    ParentalRating,
    /// <summary>
    /// DVB teletext descriptor
    /// </summary>
    Teletext,
    /// <summary>
    /// DVB telephone descriptor
    /// </summary>
    Telephone,
    /// <summary>
    /// DVB local time offset descriptor
    /// </summary>
    LocalTimeOffset,
    /// <summary>
    /// DVB subtitling descriptor
    /// </summary>
    Subtitling,
    /// <summary>
    /// DVB terrestrial delivery system descriptor
    /// </summary>
    TerrestrialDeliverySystem,
    /// <summary>
    /// DVB multilingual network name descriptor
    /// </summary>
    MultilingualNetworkName,
    /// <summary>
    /// DVB multilingual bouquet name descriptor
    /// </summary>
    MultilingualBouquetName,
    /// <summary>
    /// DVB multilingual service name descriptor
    /// </summary>
    MultilingualServiceName,
    /// <summary>
    /// DVB multilingual component descriptor
    /// </summary>
    MultilingualComponent,
    /// <summary>
    /// DVB private data specifier descriptor
    /// </summary>
    PrivateDataSpecifier,
    /// <summary>
    /// DVB service move descriptor
    /// </summary>
    ServiceMove,
    /// <summary>
    /// DVB short smoothing buffer descriptor
    /// </summary>
    ShortSmoothingBuffer,
    /// <summary>
    /// DVB frequency list descriptor
    /// </summary>
    FrequencyList,
    /// <summary>
    /// DVB partial transport stream descriptor
    /// </summary>
    PartialTransportStream,
    /// <summary>
    /// DVB data broadcast descriptor
    /// </summary>
    DataBroadcast,
    /// <summary>
    /// DVB scrambling descriptor
    /// </summary>
    Scrambling,
    /// <summary>
    /// DVB data broadcast ID descriptor
    /// </summary>
    DataBroadcastId,
    /// <summary>
    /// DVB transport stream descriptor
    /// </summary>
    TransportStream,
    /// <summary>
    /// DVB digital satellite news gathering descriptor
    /// </summary>
    Dsng,
    /// <summary>
    /// DVB program delivery control descriptor
    /// </summary>
    Pdc,
    /// <summary>
    /// DVB AC3 descriptor
    /// </summary>
    Ac3,
    /// <summary>
    /// DVB anciliary data descriptor
    /// </summary>
    AnciliaryData,
    /// <summary>
    /// DVB cell list descriptor
    /// </summary>
    CellList,
    /// <summary>
    /// DVB cell frequency link descriptor
    /// </summary>
    CellFrequencyLink,
    /// <summary>
    /// DVB announcement support descriptor
    /// </summary>
    AnnouncementSupport,
    /// <summary>
    /// DVB application signalling descriptor
    /// </summary>
    ApplicationSignalling,
    /// <summary>
    /// DVB adaption field data descriptor
    /// </summary>
    AdaptionFieldData,
    /// <summary>
    /// DVB service identifier descriptor
    /// </summary>
    ServiceIdentfier,
    /// <summary>
    /// DVB service availability descriptor
    /// </summary>
    ServiceAvailability,
    /// <summary>
    /// DVB default authority descriptor
    /// </summary>
    DefaultAuthority,
    /// <summary>
    /// DVB related content descriptor
    /// </summary>
    RelatedContent,
    /// <summary>
    /// DVB TVA ID descriptor
    /// </summary>
    TvaId,
    /// <summary>
    /// DVB content identifier descriptor
    /// </summary>
    ContentIdentifier,
    /// <summary>
    /// DVB time slice FEC identifier descriptor
    /// </summary>
    TimeSliceFecIdentifier,
    /// <summary>
    /// DVB entitlement control message repetition rate descriptor
    /// </summary>
    EcmRepetitionRate,
    /// <summary>
    /// DVB S2 satellite delivery system descriptor
    /// </summary>
    S2SatelliteDeliverySystem,
    /// <summary>
    /// DVB enhanced AC3 descriptor
    /// </summary>
    EnhancedAc3,
    /// <summary>
    /// DVB DTS descriptor
    /// </summary>
    Dts,
    /// <summary>
    /// DVB AAC descriptor
    /// </summary>
    Aac,
    /// <summary>
    /// DVB XAIT location descriptor
    /// </summary>
    XaitLocation,
    /// <summary>
    /// DVB free-to-air content management descriptor
    /// </summary>
    FtaContentManagement,
    /// <summary>
    /// DVB extension descriptor
    /// </summary>
    Extension,

    #endregion

    #region ATSC/SCTE

    #region ATSC A/101

    /// <summary>
    /// ATSC Advanced Common Application Platform descriptor
    /// </summary>
    AcapXApplication = 0x60,
    /// <summary>
    /// ATSC Advanced Common Application Platform location descriptor
    /// </summary>
    AcapXApplicationLocation = 0x61,
    /// <summary>
    /// ATSC Advanced Common Application Platform boundary descriptor
    /// </summary>
    AcapXApplicationBoundary = 0x62,

    #endregion

    /// <summary>
    /// ATSC stuffing descriptor
    /// </summary>
    AtscStuffing = 0x80,
    /// <summary>
    /// ATSC AC3 audio descriptor
    /// </summary>
    Ac3Audio = 0x81,
    /// <summary>
    /// SCTE frame rate descriptor
    /// </summary>
    FrameRate = 0x82,
    /// <summary>
    /// SCTE extended video descriptor
    /// </summary>
    ExtendedVideo = 0x83,
    /// <summary>
    /// SCTE component name descriptor
    /// </summary>
    ScteComponentName = 0x84,
    /// <summary>
    /// ATSC program identifier descriptor
    /// </summary>
    ProgramIdentifier = 0x85,
    /// <summary>
    /// ATSC caption service descriptor
    /// </summary>
    CaptionService = 0x86,
    /// <summary>
    /// ATSC content advisory descriptor
    /// </summary>
    ContentAdvisory = 0x87,
    /// <summary>
    /// ATSC conditional access descriptor
    /// </summary>
    AtscConditionalAccess = 0x88,
    /// <summary>
    /// ATSC descriptor descriptor
    /// </summary>
    Descriptor = 0x89,
    /// <summary>
    /// SCTE cue identifier descriptor
    /// </summary>
    CueIdentifier = 0x8a,

    // 0x8b reserved

    /// <summary>
    /// ATSC timestamp descriptor
    /// </summary>
    TimeStamp = 0x8c,
    /// <summary>
    /// ATSC parameterised service descriptor
    /// </summary>
    ParameterisedService,
    /// <summary>
    /// ATSC interactive services filtering criteria descriptor
    /// </summary>
    InteractiveServicesFilteringCriteria,
    /// <summary>
    /// ATSC interactive services near real time services summary descriptor
    /// </summary>
    InteractiveServicesNrtServicesSummary,
    /// <summary>
    /// SCTE frequency spec descriptor
    /// </summary>
    FrequencySpec = 0x90,
    /// <summary>
    /// SCTE modulation params descriptor
    /// </summary>
    ModulationParams = 0x91,
    /// <summary>
    /// SCTE transport stream ID descriptor
    /// </summary>
    TransportStreamId = 0x92,

    #region SCTE 65, ATSC A/65

    /// <summary>
    /// SCTE revision detection descriptor
    /// </summary>
    RevisionDetection = 0x93,
    /// <summary>
    /// SCTE two part channel number descriptor
    /// </summary>
    TwoPartChannelNumber = 0x94,
    /// <summary>
    /// SCTE channel properties descriptor
    /// </summary>
    ChannelProperties = 0x95,
    /// <summary>
    /// SCTE daylight savings time descriptor
    /// </summary>
    DaylightSavingsTime = 0x96,
    /// <summary>
    /// SCTE adaption field data descriptor
    /// </summary>
    ScteAdaptionFieldData = 0x97,

    // 0x98 - 0x9f reserved

    /// <summary>
    /// ATSC extended channel name descriptor
    /// </summary>
    ExtendedChannelName = 0xa0,
    /// <summary>
    /// ATSC service location descriptor
    /// </summary>
    ServiceLocation = 0xa1,
    /// <summary>
    /// ATSC time shifted service descriptor
    /// </summary>
    AtscTimeShiftedService = 0xa2,
    /// <summary>
    /// ATSC component name descriptor
    /// </summary>
    AtscComponentName = 0xa3,

    #endregion

    #region ATSC A/90

    /// <summary>
    /// ATSC data service descriptor
    /// </summary>
    DataService = 0xa4,
    /// <summary>
    /// ATSC PID count descriptor
    /// </summary>
    PidCount = 0xa5,
    /// <summary>
    /// ATSC download descriptor
    /// </summary>
    Download = 0xa6,
    /// <summary>
    /// ATSC multiprotocol encapsulation descriptor
    /// </summary>
    MultiprotocolEncapsulation = 0xa7,

    #endregion

    #region ATSC A/65

    /// <summary>
    /// ATSC Directed Channel Change departing request descriptor
    /// </summary>
    DccDepartingRequest = 0xa8,
    /// <summary>
    /// ATSC Directed Channel Change arriving request descriptor
    /// </summary>
    DccArrivingRequest = 0xa9,
    /// <summary>
    /// ATSC redistribution control descriptor
    /// </summary>
    Rc = 0xaa,
    /// <summary>
    /// ATSC genre descriptor
    /// </summary>
    Genre = 0xab,

    #endregion

    /// <summary>
    /// SCTE MAC address list descriptor
    /// </summary>
    MacAddressList = 0xac,
    /// <summary>
    /// ATSC private information descriptor
    /// </summary>
    PrivateInformation = 0xad,

    #region ATSC A/94

    /// <summary>
    /// ATSC compatibility wrapper descriptor
    /// </summary>
    CompatibilityWrapper = 0xae,
    /// <summary>
    /// ATSC broadcaster policy descriptor
    /// </summary>
    BroadcasterPolicy = 0xaf,
    /// <summary>
    /// ATSC service name descriptor
    /// </summary>
    ServiceName = 0xb0,
    /// <summary>
    /// ATSC URI descriptor
    /// </summary>
    Uri = 0xb1,

    #endregion

    /// <summary>
    /// ATSC enhanced signalling descriptor
    /// </summary>
    EnhancedSignalling = 0xb2,
    /// <summary>
    /// ATSC string mapping descriptor
    /// </summary>
    StringMapping = 0xb3,
    /// <summary>
    /// ATSC module link descriptor
    /// </summary>
    ModuleLink = 0xb4,
    /// <summary>
    /// ATSC CRC32 descriptor
    /// </summary>
    Crc32 = 0xb5,
    /// <summary>
    /// ATSC content identifier descriptor
    /// </summary>
    AtscContentIdentifier = 0xb6,
    /// <summary>
    /// ATSC module info descriptor
    /// </summary>
    ModuleInfo = 0xb7,
    /// <summary>
    /// ATSC group link descriptor
    /// </summary>
    GroupLink = 0xb8,
    /// <summary>
    /// ATSC timestamp descriptor
    /// </summary>
    AtscTimeStamp = 0xb9,
    /// <summary>
    /// ATSC schedule descriptor
    /// </summary>
    Schedule = 0xba,
    /// <summary>
    /// ATSC component list descriptor
    /// </summary>
    ComponentList = 0xbb,

    #region ATSC M/H A/153 part 3

    /// <summary>
    /// ATSC M/H component descriptor
    /// </summary>
    AtscMhComponent = 0xbc,
    /// <summary>
    /// ATSC M/H rights issuer descriptor
    /// </summary>
    RightsIssuer = 0xbd,
    /// <summary>
    /// ATSC M/H current program descriptor
    /// </summary>
    CurrentProgram = 0xbe,
    /// <summary>
    /// ATSC M/H original service identification descriptor
    /// </summary>
    OriginalServiceIdentification = 0xbf,
    /// <summary>
    /// ATSC M/H protection descriptor
    /// </summary>
    Protection = 0xc0,
    /// <summary>
    /// ATSC M/H SG bootstrap descriptor
    /// </summary>
    MhSgBootstrap = 0xc1,

    #endregion

    #region ATSC A/103

    /// <summary>
    /// ATSC service ID descriptor
    /// </summary>
    ServiceId = 0xc2,
    /// <summary>
    /// ATSC protocol version descriptor
    /// </summary>
    ProtocolVersion = 0xc3,
    /// <summary>
    /// ATSC Non Real Time service descriptor
    /// </summary>
    NrtService = 0xc4,
    /// <summary>
    /// ATSC capabilities descriptor
    /// </summary>
    Capabilities = 0xc5,
    /// <summary>
    /// ATSC icon descriptor
    /// </summary>
    Icon = 0xc6,
    /// <summary>
    /// ATSC receiver targeting descriptor
    /// </summary>
    ReceiverTargeting = 0xc7,
    /// <summary>
    /// ATSC time slot descriptor
    /// </summary>
    TimeSlot = 0xc8,
    /// <summary>
    /// ATSC internet location descriptor
    /// </summary>
    InternetLocation = 0xc9,
    /// <summary>
    /// ATSC associated service descriptor
    /// </summary>
    AssociatedService = 0xca,
    /// <summary>
    /// ATSC eye identification descriptor
    /// </summary>
    EyeIdentification = 0xcb,

    #endregion

    /// <summary>
    /// ATSC enhanced AC-3 descriptor
    /// </summary>
    Eac3 = 0xcc,
    /// <summary>
    /// ATSC 2D 3D corresponding content descriptor
    /// </summary>
    CorrespondingContent = 0xcd,
    /// <summary>
    /// ATSC multimedia EPG linkage descriptor
    /// </summary>
    MultimediaEpgLinkage = 0xce,

    // 0xe0 - 0xe9 CableLabs

    /// <summary>
    /// SCTE MPEG AAC descriptor
    /// </summary>
    MpegAac = 0xea,
    /// <summary>
    /// ATSC IC3D event info descriptor
    /// </summary>
    Ic3dEventInfo = 0xeb,
    /// <summary>
    /// ATSC MDTV hybrid stereoscopic service descriptor
    /// </summary>
    MdtvHybridStereoscopicService = 0xec

    #endregion
  }

  /// <summary>
  /// DVB extension descriptor tags.
  /// </summary>
  public enum DvbExtensionDescriptorTag
  {
    /// <summary>
    /// image icon descriptor
    /// </summary>
    ImageIcon = 0x00,
    /// <summary>
    /// content protection copy management delivery signalling descriptor
    /// </summary>
    CpcmDeliverySignalling = 0x01,
    /// <summary>
    /// content protection descriptor
    /// </summary>
    Cp = 0x02,
    /// <summary>
    /// content protection identifier descriptor
    /// </summary>
    CpIdentifier = 0x03,
    /// <summary>
    /// T2 delivery system descriptor
    /// </summary>
    T2DeliverySystem = 0x04,
    /// <summary>
    /// SH delivery system descriptor
    /// </summary>
    ShDeliverySystem = 0x05,
    /// <summary>
    /// supplementary audio descriptor
    /// </summary>
    SupplementaryAudio = 0x06,
    /// <summary>
    /// network change notify descriptor
    /// </summary>
    NetworkChangeNotify = 0x07,
    /// <summary>
    /// message descriptor
    /// </summary>
    Message = 0x08,
    /// <summary>
    /// target region descriptor
    /// </summary>
    TargetRegion = 0x09,
    /// <summary>
    /// target region name descriptor
    /// </summary>
    TargetRegionName = 0x0a,
    /// <summary>
    /// service relocated descriptor
    /// </summary>
    ServiceRelocated = 0x0b,
    /// <summary>
    /// XAIT PID descriptor
    /// </summary>
    XaitPid = 0x0c,
    /// <summary>
    /// C2 delivery system descriptor
    /// </summary>
    C2DeliverySystem = 0x0d,
    /// <summary>
    /// DTS HD audio stream descriptor
    /// </summary>
    DtsHdAudioStream = 0x0e,
    /// <summary>
    /// DTS neural descriptor
    /// </summary>
    DtsNeural = 0x0f,
    /// <summary>
    /// video depth range descriptor
    /// </summary>
    VideoDepthRange = 0x10,
    /// <summary>
    /// T2 modulator interface descriptor
    /// </summary>
    T2mi = 0x11,

    // 0x12 reserved

    /// <summary>
    /// URI linkage descriptor
    /// </summary>
    UriLinkage = 0x13,
    /// <summary>
    /// broadcast identifier ancillary data descriptor
    /// </summary>
    BciAncillaryData = 0x14
  }

  /// <summary>
  /// A simple helper class to centralise logic that needs to distinguish between different stream
  /// categories or types.
  /// </summary>
  public static class StreamTypeHelper
  {
    /// <summary>
    /// Determine whether an elementary stream is a video stream.
    /// </summary>
    /// <param name="streamType">The logical stream type of the elementary stream.</param>
    /// <returns><c>true</c> if the elementary stream is a video stream, otherwise <c>false</c></returns>
    public static bool IsVideoStream(LogicalStreamType streamType)
    {
      if (streamType == LogicalStreamType.VideoMpeg1 ||
        streamType == LogicalStreamType.VideoMpeg2 ||
        streamType == LogicalStreamType.VideoMpeg4Part2 ||
        streamType == LogicalStreamType.VideoMpeg4Part10 ||
        streamType == LogicalStreamType.VideoMpegC ||
        streamType == LogicalStreamType.VideoMpeg4Part10AnnexG ||
        streamType == LogicalStreamType.VideoMpeg4Part10AnnexH ||
        streamType == LogicalStreamType.VideoJpeg ||
        streamType == LogicalStreamType.VideoMpeg2StereoscopicFramePacked ||
        streamType == LogicalStreamType.VideoMpeg2StereoscopicAdditionalStream ||
        streamType == LogicalStreamType.VideoMpeg4Part10StereoscopicAdditionalStream ||
        streamType == LogicalStreamType.VideoVc1)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determine whether an elementary stream is an audio stream.
    /// </summary>
    /// <param name="streamType">The logical stream type of the elementary stream.</param>
    /// <returns><c>true</c> if the elementary stream is an audio stream, otherwise <c>false</c></returns>
    public static bool IsAudioStream(LogicalStreamType streamType)
    {
      if (streamType == LogicalStreamType.AudioMpeg1 ||
        streamType == LogicalStreamType.AudioMpeg2 ||
        streamType == LogicalStreamType.AudioMpeg2Part7 ||
        streamType == LogicalStreamType.AudioMpeg4Part3Latm ||
        streamType == LogicalStreamType.AudioMpeg4Part3 ||
        streamType == LogicalStreamType.AudioAc3 ||
        streamType == LogicalStreamType.AudioEnhancedAc3 ||
        streamType == LogicalStreamType.AudioDts ||
        streamType == LogicalStreamType.AudioDtsHd)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determine whether an elementary stream is a valid DigiCipher II stream.
    /// </summary>
    /// <param name="streamType">The raw stream type of the elementary stream.</param>
    /// <returns><c>true</c> if the elementary stream is a valid DigiCipher II stream, otherwise <c>false</c></returns>
    public static bool IsValidDigiCipher2Stream(StreamType streamType)
    {
      if (streamType == StreamType.DigiCipher2Video ||
        streamType == StreamType.Ac3Audio ||
        streamType == StreamType.EnhancedAc3Audio ||
        streamType == StreamType.Subtitles)
      {
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// DVB service types. See EN 300 468 table 81.
  /// </summary>
  public enum DvbServiceType
  {
    // (0x00 reserved)

    /// <summary>
    /// digital television service
    /// </summary>
    DigitalTelevision = 0x01,
    /// <summary>
    /// digital radio sound service
    /// </summary>
    DigitalRadio = 0x02,
    /// <summary>
    /// teletext service
    /// </summary>
    Teletext = 0x03,
    /// <summary>
    /// near video on demand reference service
    /// </summary>
    NvodReference = 0x04,
    /// <summary>
    /// near video on demand time-shifted service
    /// </summary>
    NvodTimeShifted = 0x05,
    /// <summary>
    /// mosaic service
    /// </summary>
    Mosaic = 0x06,
    /// <summary>
    /// frequency modulated radio service
    /// </summary>
    FmRadio = 0x07,
    /// <summary>
    /// DVB system renewability messages service
    /// </summary>
    DvbSrm = 0x08,

    // (0x09 reserved)

    /// <summary>
    /// advanced codec digital radio sound service
    /// </summary>
    AdvancedCodecDigitalRadio = 0x0a,
    /// <summary>
    /// advanced codec mosaic service
    /// </summary>
    AdvancedCodecMosaic = 0x0b,
    /// <summary>
    /// data broadcast service
    /// </summary>
    DataBroadcast = 0x0c,

    // (0x0d reserved for common interface use)

    /// <summary>
    /// return channel via satellite map
    /// </summary>
    RcsMap = 0x0e,

    /// <summary>
    /// return channel via satellite forward link signalling
    /// </summary>
    RcsFls = 0x0f,
    /// <summary>
    /// DVB multimedia home platform service
    /// </summary>
    DvbMhp = 0x10,
    /// <summary>
    /// MPEG 2 high definition digital television service
    /// </summary>
    Mpeg2HdDigitalTelevision = 0x11,

    // (0x12 to 0x15 reserved)

    /// <summary>
    /// advanced codec standard definition digital television service
    /// </summary>
    AdvancedCodecSdDigitalTelevision = 0x16,
    /// <summary>
    /// advanced codec standard definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecSdNvodTimeShifted = 0x17,
    /// <summary>
    /// advanced codec standard definition near video on demand reference service
    /// </summary>
    AdvancedCodecSdNvodReference = 0x18,
    /// <summary>
    /// advanced codec high definition digital television
    /// </summary>
    AdvancedCodecHdDigitalTelevision = 0x19,
    /// <summary>
    /// advanced codec high definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecHdNvodTimeShifted = 0x1a,
    /// <summary>
    /// advanced codec high definition near video on demand reference service
    /// </summary>
    AdvancedCodecHdNvodReference = 0x1b,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition digital television service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdDigitalTelevision = 0x1c,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodTimeShifted = 0x1d,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition near video on demand reference service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodReference = 0x1e,

    // (0x1c to 0x7f reserved)
    // (0x80 to 0xfe user defined)

    /// <summary>
    /// Sky Germany portal service (also known as linked or option services)
    /// </summary>
    SkyGermanyOptionChannel = 0xd3

    // (0xff reserved)
  }

  /// <summary>
  /// ATSC service types. See A/53 part 1 table 4.1 and the ATSC code points registry "other" tab.
  /// </summary>
  public enum AtscServiceType
  {
    /// <summary>
    /// analog television (A/65)
    /// </summary>
    AnalogTelevision = 0x01,
    /// <summary>
    /// digital television service (A/53 part 3)
    /// </summary>
    DigitalTelevision = 0x02,
    /// <summary>
    /// audio service (A/53 part 3)
    /// </summary>
    Audio = 0x03,
    /// <summary>
    /// data only service (A/90)
    /// </summary>
    DataOnly = 0x04,
    /// <summary>
    /// software download service (A/97)
    /// </summary>
    SoftwareDownload = 0x05,
    /// <summary>
    /// unassociated/small screen service (A/53 part 3)
    /// </summary>
    SmallScreen = 0x06,
    /// <summary>
    /// parameterised service (A/71)
    /// </summary>
    Parameterised = 0x07,
    /// <summary>
    /// non real time service (A/103)
    /// </summary>
    Nrt = 0x08,
    /// <summary>
    /// extended parametised service (A/71)
    /// </summary>
    ExtendedParameterised = 0x09
  }

  #endregion

  /// <summary>
  /// A class that models the transport stream conditional access table section defined in ISO/IEC 13818-1.
  /// </summary>
  public class Cat
  {
    #region constants

    /// <summary>
    /// The maximum size of an ISO/IEC 13818-1 CAT section, in bytes.
    /// </summary>
    public const int MAX_SIZE = 4096;

    #endregion

    #region variables

    private byte _tableId;
    private byte _sectionSyntaxIndicator;
    private ushort _sectionLength;
    private byte _version;
    private byte _currentNextIndicator;
    private byte _sectionNumber;
    private byte _lastSectionNumber;
    private List<IDescriptor> _descriptors;
    private List<IDescriptor> _caDescriptors;
    private byte[] _crc;

    private byte[] _rawCat;

    #endregion

    // This class has a specific purpose - decoding CAT data. Although it may be tempting, we want to
    // prevent it being used for holding various other info. Therefore the only way you can get an instance
    // is by calling Decode() with a valid CAT section.
    private Cat()
    {
    }

    #region properties

    /// <summary>
    /// The conditional access table ID. Expected to be 0x01.
    /// </summary>
    public byte TableId
    {
      get
      {
        return _tableId;
      }
    }

    /// <summary>
    /// The conditional access section syntax indicator. Expected to be 1 (stored in this class as 0x80).
    /// </summary>
    public byte SectionSyntaxIndicator
    {
      get
      {
        return _sectionSyntaxIndicator;
      }
    }

    /// <summary>
    /// The length of the conditional access section, including the CRC but not the table ID, section syntax
    /// indicator or section length bytes.
    /// </summary>
    public ushort SectionLength
    {
      get
      {
        return _sectionLength;
      }
    }

    /// <summary>
    /// The version number of the conditional access table.
    /// </summary>
    public byte Version
    {
      get
      {
        return _version;
      }
    }

    /// <summary>
    /// When non-zero, indicates that the condtional access table information is current. Otherwise,
    /// indicates that the information will apply in the future.
    /// </summary>
    public byte CurrentNextIndicator
    {
      get
      {
        return _currentNextIndicator;
      }
    }

    /// <summary>
    /// The index corresponding with this section of the conditional access table.
    /// </summary>
    public byte SectionNumber
    {
      get
      {
        return _sectionNumber;
      }
    }

    /// <summary>
    /// The total number of sections (minus one) that comprise the complete conditional access table.
    /// </summary>
    public byte LastSectionNumber
    {
      get
      {
        return _lastSectionNumber;
      }
    }

    /// <summary>
    /// The descriptors for the service described by the conditional access table. Conditional access
    /// descriptors are not included.
    /// </summary>
    public ReadOnlyCollection<IDescriptor> Descriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_descriptors);
      }
    }

    /// <summary>
    /// The conditional access descriptors for the service described by the conditional access table.
    /// </summary>
    public ReadOnlyCollection<IDescriptor> CaDescriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_caDescriptors);
      }
    }

    /// <summary>
    /// Cyclic redundancy check bytes for confirming the integrity of the conditional access section data.
    /// </summary>
    public ReadOnlyCollection<byte> Crc
    {
      get
      {
        return new ReadOnlyCollection<byte>(_crc);
      }
    }

    #endregion

    /// <summary>
    /// Decode and check the validity of raw conditional access section data.
    /// </summary>
    /// <param name="data">The raw conditional access section data.</param>
    /// <returns>a fully populated cat instance if the section is valid, otherwise <c>null</c></returns>
    public static Cat Decode(byte[] data)
    {
      Log.Debug("CAT: decode");
      if (data == null || data.Length < 12)
      {
        Log.Error("CAT: CAT not supplied or too short");
        return null;
      }

      try
      {
        if (data[0] != 0x01)
        {
          Log.Error("CAT: invalid table ID {0}", data[0]);
          throw new Exception();
        }
        if ((data[1] & 0x80) != 0x80)
        {
          Log.Error("CAT: section syntax indicator is 0, should be 1");
          throw new Exception();
        }
        if ((data[1] & 0x40) != 0)
        {
          Log.Error("CAT: corruption detected at header zero bit");
          throw new Exception();
        }

        Cat cat = new Cat();
        cat._tableId = data[0];
        cat._sectionSyntaxIndicator = (byte)(data[1] & 0x80);
        cat._sectionLength = (ushort)(((data[1] & 0x0f) << 8) + data[2]);
        if (3 + cat._sectionLength > data.Length)
        {
          Log.Error("CAT: section length {0} is invalid, data length = {1}", cat._sectionLength, data.Length);
          throw new Exception();
        }

        cat._version = (byte)((data[5] & 0x3e) >> 1);
        cat._currentNextIndicator = (byte)(data[5] & 0x01);
        cat._sectionNumber = data[6];
        cat._lastSectionNumber = data[7];

        // Descriptors.
        int offset = 8;
        int endDescriptors = data.Length - 4;
        cat._descriptors = new List<IDescriptor>();
        cat._caDescriptors = new List<IDescriptor>();
        while (offset + 1 < endDescriptors)
        {
          IDescriptor d = Descriptor.Decode(data, offset);
          if (d == null)
          {
            Log.Error("CAT: descriptor {0} is invalid", cat._descriptors.Count + cat._caDescriptors.Count + 1);
            throw new Exception();
          }
          offset += d.Length + 2;
          if (d.Tag == DescriptorTag.ConditionalAccess)
          {
            cat._caDescriptors.Add(d);
          }
          else
          {
            cat._descriptors.Add(d);
          }
        }
        if (offset != endDescriptors)
        {
          Log.Error("CAT: corruption detected at end of descriptors, offset = {0}, descriptors end = {1}", offset, endDescriptors);
          throw new Exception();
        }

        cat._crc = new byte[4];
        Buffer.BlockCopy(data, offset, cat._crc, 0, 4);

        // Make a copy of the CAT so that changes made by the caller on the original array have no effect on
        // our reference/copy.
        cat._rawCat = new byte[data.Length];
        Buffer.BlockCopy(data, 0, cat._rawCat, 0, data.Length);

        //cat.Dump();

        return cat;
      }
      catch
      {
        Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(data, data.Length);
        return null;
      }
    }

    /// <summary>
    /// Retrieve a read-only copy of the original conditional access section data that was decoded to create this Cat instance.
    /// </summary>
    /// <returns>a copy of the raw conditional access section data</returns>
    public ReadOnlyCollection<byte> GetRawCat()
    {
      return new ReadOnlyCollection<byte>(_rawCat);
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      this.LogDebug("CAT: dump...");
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_rawCat, _rawCat.Length);
      this.LogDebug("  table ID                 = {0}", _tableId);
      this.LogDebug("  section syntax indicator = {0}", _sectionSyntaxIndicator);
      this.LogDebug("  section length           = {0}", _sectionLength);
      this.LogDebug("  version                  = {0}", _version);
      this.LogDebug("  current next indicator   = {0}", _currentNextIndicator);
      this.LogDebug("  section number           = {0}", _sectionNumber);
      this.LogDebug("  last section number      = {0}", _lastSectionNumber);
      this.LogDebug("  CRC                      = 0x{0:x2}{1:x2}{2:x2}{3:x2}", _crc[0], _crc[1], _crc[2], _crc[3]);
      this.LogDebug("  {0} descriptor(s)...", _descriptors.Count + _caDescriptors.Count);
      foreach (IDescriptor d in _descriptors)
      {
        d.Dump();
      }
      foreach (IDescriptor cad in _caDescriptors)
      {
        cad.Dump();
      }
    }
  }

  /// <summary>
  /// A class that models the transport stream program map table section defined in ISO/IEC 13818-1.
  /// </summary>
  public class Pmt
  {
    #region constants

    /// <summary>
    /// The maximum size of an ISO/IEC 13818-1 PMT section, in bytes.
    /// </summary>
    public const int MAX_SIZE = 1024;

    #endregion

    #region variables

    private byte _tableId;
    private byte _sectionSyntaxIndicator;
    private ushort _sectionLength;
    private ushort _programNumber;
    private byte _version;
    private byte _currentNextIndicator;
    private byte _sectionNumber;
    private byte _lastSectionNumber;
    private ushort _pcrPid;
    private ushort _programInfoLength;
    private List<IDescriptor> _programDescriptors;
    private List<IDescriptor> _programCaDescriptors;
    private List<PmtElementaryStream> _elementaryStreams;
    private byte[] _crc;

    private byte[] _rawPmt;
    private CamType _camType = CamType.Default;

    #endregion

    // This class has a specific purpose - decoding and translating between various PMT formats. Although it
    // may be tempting, we want to prevent it being used for holding various other info. Therefore the only
    // way you can get an instance is by calling Decode() with a valid PMT section.
    private Pmt()
    {
    }

    #region properties

    /// <summary>
    /// The program map table ID. Expected to be 0x02.
    /// </summary>
    public byte TableId
    {
      get
      {
        return _tableId;
      }
    }

    /// <summary>
    /// The program map section syntax indicator. Expected to be 1 (stored in this class as 0x80).
    /// </summary>
    public byte SectionSyntaxIndicator
    {
      get
      {
        return _sectionSyntaxIndicator;
      }
    }

    /// <summary>
    /// The length of the program map section, including the CRC but not the table ID, section syntax
    /// indicator or section length bytes.
    /// </summary>
    public ushort SectionLength
    {
      get
      {
        return _sectionLength;
      }
    }

    /// <summary>
    /// The program number (service ID) of the service that the program map describes.
    /// </summary>
    public ushort ProgramNumber
    {
      get
      {
        return _programNumber;
      }
    }

    /// <summary>
    /// The version number of the program map.
    /// </summary>
    public byte Version
    {
      get
      {
        return _version;
      }
    }

    /// <summary>
    /// When non-zero, indicates that the program map describes the service's current state. Otherwise,
    /// indicates that the program map describes the next service state.
    /// </summary>
    public byte CurrentNextIndicator
    {
      get
      {
        return _currentNextIndicator;
      }
    }

    /// <summary>
    /// The index corresponding with this section of the program map. Expected to be zero.
    /// </summary>
    public byte SectionNumber
    {
      get
      {
        return _sectionNumber;
      }
    }

    /// <summary>
    /// The total number of sections (minus one) that comprise the complete program map. Expected to be zero.
    /// </summary>
    public byte LastSectionNumber
    {
      get
      {
        return _lastSectionNumber;
      }
    }

    /// <summary>
    /// The PID containing the program clock reference data for the service described by the program map.
    /// </summary>
    public ushort PcrPid
    {
      get
      {
        return _pcrPid;
      }
    }

    /// <summary>
    /// The total number of bytes in the program map program descriptors.
    /// </summary>
    public ushort ProgramInfoLength
    {
      get
      {
        return _programInfoLength;
      }
    }

    /// <summary>
    /// The descriptors for the service described by the program map. Conditional access descriptors are not
    /// included.
    /// </summary>
    public ReadOnlyCollection<IDescriptor> ProgramDescriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_programDescriptors);
      }
    }

    /// <summary>
    /// The conditional access descriptors for the service described by the program map.
    /// </summary>
    public ReadOnlyCollection<IDescriptor> ProgramCaDescriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_programCaDescriptors);
      }
    }

    /// <summary>
    /// The elementary streams described in the program map.
    /// </summary>
    public ReadOnlyCollection<PmtElementaryStream> ElementaryStreams
    {
      get
      {
        return new ReadOnlyCollection<PmtElementaryStream>(_elementaryStreams);
      }
    }

    /// <summary>
    /// Cyclic redundancy check bytes for confirming the integrity of the program map section data.
    /// </summary>
    public ReadOnlyCollection<byte> Crc
    {
      get
      {
        return new ReadOnlyCollection<byte>(_crc);
      }
    }

    #endregion

    /// <summary>
    /// Decode and check the validity of raw program map section data.
    /// </summary>
    /// <param name="data">The raw program map section data.</param>
    /// <returns>a fully populated Pmt instance if the section is valid, otherwise <c>null</c></returns>
    public static Pmt Decode(byte[] data)
    {
      Log.Debug("PMT: decode");
      if (data == null || data.Length < 16)
      {
        Log.Error("PMT: data not supplied or too short");
        return null;
      }

      try
      {
        if (data[0] != 0x02)
        {
          Log.Error("PMT: invalid table ID {0}", data[0]);
          throw new Exception();
        }
        if ((data[1] & 0x80) != 0x80)
        {
          Log.Error("PMT: section syntax indicator is 0, should be 1");
          throw new Exception();
        }
        if ((data[1] & 0x40) != 0)
        {
          Log.Debug("PMT: corruption detected at header zero bit");
          throw new Exception();
        }
        if (data[6] != 0)
        {
          Log.Error("PMT: section number is {0}, should be 0", data[6]);
          throw new Exception();
        }
        if (data[7] != 0)
        {
          Log.Error("PMT: last section number is {0}, should be 0", data[7]);
          throw new Exception();
        }

        Pmt pmt = new Pmt();
        pmt._tableId = data[0];
        pmt._sectionSyntaxIndicator = (byte)(data[1] & 0x80);
        pmt._sectionLength = (ushort)(((data[1] & 0x0f) << 8) + data[2]);
        if (3 + pmt._sectionLength != data.Length)
        {
          Log.Error("PMT: section length {0} is invalid, data length = {1}", pmt._sectionLength, data.Length);
          throw new Exception();
        }

        pmt._programNumber = (ushort)((data[3] << 8) + data[4]);
        pmt._version = (byte)((data[5] & 0x3e) >> 1);
        pmt._currentNextIndicator = (byte)(data[5] & 0x01);
        pmt._sectionNumber = data[6];
        pmt._lastSectionNumber = data[7];
        pmt._pcrPid = (ushort)(((data[8] & 0x1f) << 8) + data[9]);
        pmt._programInfoLength = (ushort)(((data[10] & 0x0f) << 8) + data[11]);
        if (12 + pmt._programInfoLength + 4 > data.Length)
        {
          Log.Error("PMT: program info length {0} is invalid, data length = {1}", pmt._programInfoLength, data.Length);
          throw new Exception();
        }

        // Program descriptors.
        bool isScteTs = false;
        int offset = 12;
        int endProgramDescriptors = offset + pmt._programInfoLength;
        pmt._programDescriptors = new List<IDescriptor>();
        pmt._programCaDescriptors = new List<IDescriptor>();
        while (offset + 1 < endProgramDescriptors)
        {
          IDescriptor d = Descriptor.Decode(data, offset);
          if (d == null)
          {
            Log.Error("PMT: program descriptor {0} is invalid", pmt._programDescriptors.Count + pmt._programCaDescriptors.Count + 1);
            throw new Exception();
          }
          offset += d.Length + 2;
          if (d.Tag == DescriptorTag.ConditionalAccess)
          {
            pmt._programCaDescriptors.Add(d);
          }
          else
          {
            pmt._programDescriptors.Add(d);
            if (d.Tag == DescriptorTag.Registration && d.Length >= 4)
            {
              if (d.Data[0] == 'S' && d.Data[1] == 'C' && d.Data[2] == 'T' && d.Data[3] == 'E')
              {
                isScteTs = true;
              }
            }
          }
        }
        if (offset != endProgramDescriptors)
        {
          Log.Error("PMT: corruption detected at end of program descriptors, offset = {0}, program descriptors end = {1}", offset, endProgramDescriptors);
          throw new Exception();
        }

        // Elementary streams.
        bool isNotDc2Ts = false;
        bool foundDc2VideoStream = false;
        pmt._elementaryStreams = new List<PmtElementaryStream>();
        int endEsData = data.Length - 4;
        while (offset + 4 < endEsData)
        {
          PmtElementaryStream es = new PmtElementaryStream();
          es.StreamType = (StreamType)data[offset++];
          if (Enum.IsDefined(typeof(LogicalStreamType), (int)es.StreamType))
          {
            es.LogicalStreamType = (LogicalStreamType)(int)es.StreamType;
            // Could this still be a DC II transport stream?
            if (!isNotDc2Ts && !StreamTypeHelper.IsValidDigiCipher2Stream(es.StreamType))
            {
              isNotDc2Ts = true;
              if (foundDc2VideoStream)
              {
                // Fix false positive DC II video detections.
                for (int i = 0; i < pmt._elementaryStreams.Count; i++)
                {
                  if (pmt._elementaryStreams[i].StreamType == StreamType.DigiCipher2Video)
                  {
                    pmt._elementaryStreams[i].LogicalStreamType = LogicalStreamType.Unknown;
                  }
                }
                foundDc2VideoStream = false;
              }
            }
          }
          // We'll allow DC II video stream detection until we see clear
          // indication that this is not a DC II transport stream.
          else if (!isNotDc2Ts && es.StreamType == StreamType.DigiCipher2Video)
          {
            foundDc2VideoStream = true;
            es.LogicalStreamType = LogicalStreamType.VideoMpeg2;
          }
          else
          {
            es.LogicalStreamType = LogicalStreamType.Unknown;
          }
          es.Pid = (ushort)(((data[offset] & 0x1f) << 8) + data[offset + 1]);
          offset += 2;
          es.EsInfoLength = (ushort)(((data[offset] & 0x0f) << 8) + data[offset + 1]);
          offset += 2;

          // Elementary stream descriptors.
          int endEsDescriptors = offset + es.EsInfoLength;
          if (endEsDescriptors > endEsData)
          {
            Log.Error("PMT: elementary stream info length for PID {0} is invalid, ES data end = {1}, ES descriptors end = {2}", es.Pid, endEsData, endEsDescriptors);
            throw new Exception();
          }
          es.Descriptors = new List<IDescriptor>();
          es.CaDescriptors = new List<IDescriptor>();
          while (offset + 1 < endEsDescriptors)
          {
            IDescriptor d = Descriptor.Decode(data, offset);
            if (d == null)
            {
              Log.Error("PMT: elementary stream descriptor {0} for PID {1} is invalid", es.Descriptors.Count + es.CaDescriptors.Count + 1, es.Pid);
              throw new Exception();
            }

            if (d.Tag == DescriptorTag.ConditionalAccess)
            {
              es.CaDescriptors.Add(d);
            }
            else
            {
              es.Descriptors.Add(d);
              if (es.LogicalStreamType == LogicalStreamType.Unknown)
              {
                switch (d.Tag)
                {
                  case DescriptorTag.Mpeg4Video:
                    es.LogicalStreamType = LogicalStreamType.VideoMpeg4Part2;
                    break;
                  case DescriptorTag.AvcVideo:
                    es.LogicalStreamType = LogicalStreamType.VideoMpeg4Part10;
                    break;
                  case DescriptorTag.Registration:
                    if (d.Data[0] == 'V' && d.Data[1] == 'C' && d.Data[2] == '-' && d.Data[3] == '1')
                    {
                      es.LogicalStreamType = LogicalStreamType.VideoVc1;
                    }
                    break;
                  case DescriptorTag.Mpeg2AacAudio:
                    es.LogicalStreamType = LogicalStreamType.AudioMpeg2Part7;
                    break;
                  case DescriptorTag.Mpeg4Audio:    // MPEG
                  case DescriptorTag.Aac:           // DVB
                  case DescriptorTag.MpegAac:       // SCTE
                    es.LogicalStreamType = LogicalStreamType.AudioMpeg4Part3Latm;
                    break;
                  case DescriptorTag.Ac3:           // DVB
                  case DescriptorTag.Ac3Audio:      // ATSC
                    es.LogicalStreamType = LogicalStreamType.AudioAc3;
                    break;
                  case DescriptorTag.EnhancedAc3:   // DVB
                  case DescriptorTag.Eac3:          // ATSC
                    es.LogicalStreamType = LogicalStreamType.AudioEnhancedAc3;
                    break;
                  case DescriptorTag.Dts:
                    if (isScteTs)
                    {
                      es.LogicalStreamType = LogicalStreamType.AudioDtsHd;
                    }
                    else
                    {
                      es.LogicalStreamType = LogicalStreamType.AudioDts;
                    }
                    break;
                  case DescriptorTag.Subtitling:
                    es.LogicalStreamType = LogicalStreamType.Subtitles;
                    break;
                  case DescriptorTag.Teletext:
                  case DescriptorTag.VbiTeletext:
                    es.LogicalStreamType = LogicalStreamType.Teletext;
                    break;
                }
              }
            }
            offset += d.Length + 2;
          }
          if (offset != endEsDescriptors)
          {
            Log.Error("PMT: corruption detected at end of elementary strea descriptors for PID {0}, offset = {1}, ES descriptors end = {2}", es.Pid, offset, endEsDescriptors);
            throw new Exception();
          }

          pmt._elementaryStreams.Add(es);
        }
        if (offset != endEsData)
        {
          Log.Error("PMT: corruption detected at end of elementary stream data, offset = {0}, ES data end = {1}", offset, endEsData);
          throw new Exception();
        }

        pmt._crc = new byte[4];
        Buffer.BlockCopy(data, offset, pmt._crc, 0, 4);

        // Make a copy of the PMT so that changes made by the caller on the original array have no effect on
        // our reference/copy.
        pmt._rawPmt = new byte[data.Length];
        Buffer.BlockCopy(data, 0, pmt._rawPmt, 0, data.Length);

        //pmt.Dump();

        return pmt;
      }
      catch
      {
        Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(data, data.Length);
        return null;
      }
    }

    /// <summary>
    /// Patch the PMT to make it compatible with a specific CAM type.
    /// </summary>
    /// <param name="camType">The target CAM type.</param>
    public void PatchPmtForCam(CamType camType)
    {
      if (_camType == camType)
      {
        return;
      }

      this.LogDebug("PMT: patch for CAM type {0}", camType);
      if (_camType != CamType.Default)
      {
        throw new TvException("Not possible to patch PMT that is already patched.");
      }

      if (camType == CamType.Astoncrypt2)
      {
        // For Astoncrypt 2 CAMs, we patch the stream type on AC3 streams.
        // Move to the first ES stream type.
        int offset = 12 + _programInfoLength;
        foreach (PmtElementaryStream es in _elementaryStreams)
        {
          if (es.StreamType == StreamType.Mpeg2Part1PrivateData && es.LogicalStreamType == LogicalStreamType.AudioAc3)
          {
            es.StreamType = StreamType.Ac3Audio;
            _rawPmt[offset] = (byte)StreamType.Ac3Audio;
          }
          offset += 5 + es.EsInfoLength;
        }
      }
    }

    /// <summary>
    /// Retrieve a read-only copy of the original program map section data that was decoded to create this Pmt instance.
    /// </summary>
    /// <returns>a copy of the raw program map section data</returns>
    public ReadOnlyCollection<byte> GetRawPmt()
    {
      return new ReadOnlyCollection<byte>(_rawPmt);
    }

    /// <summary>
    /// Generate a conditional access program map command suitable for passing to an EN 50221 compliant
    /// conditional access module.
    /// </summary>
    /// <param name="listAction">The context of the command (in terms of other services that the conditional
    ///   access module will need to deal with.</param>
    /// <param name="command">The type of conditional access command.</param>
    /// <returns></returns>
    public byte[] GetCaPmt(CaPmtListManagementAction listAction, CaPmtCommand command)
    {
      this.LogDebug("PMT: get CA PMT, list action = {0}, command = {1}", listAction, command);
      byte[] tempCaPmt = new byte[MAX_SIZE];  // size of CA PMT <= size of PMT
      tempCaPmt[0] = (byte)listAction;
      tempCaPmt[1] = _rawPmt[3];
      tempCaPmt[2] = _rawPmt[4];
      tempCaPmt[3] = _rawPmt[5];

      // Program descriptors. As per EN 50221, we only add conditional access descriptors to the CA PMT.
      int offset = 6;
      int programInfoLength = 0;
      foreach (IDescriptor d in _programCaDescriptors)
      {
        if (programInfoLength == 0)
        {
          tempCaPmt[offset++] = (byte)command;
          programInfoLength++;
        }
        ReadOnlyCollection<byte> descriptorData = d.GetRawData();
        descriptorData.CopyTo(tempCaPmt, offset);
        offset += descriptorData.Count;
        programInfoLength += descriptorData.Count;
      }

      // Set the program_info_length now that we know what the length is.
      tempCaPmt[4] = (byte)((programInfoLength >> 8) & 0x0f);
      tempCaPmt[5] = (byte)(programInfoLength & 0xff);

      // Elementary streams.
      foreach (PmtElementaryStream es in _elementaryStreams)
      {
        // We add each video, audio, subtitle and teletext stream with their
        // corresponding conditional access descriptors to the CA PMT.
        if (StreamTypeHelper.IsVideoStream(es.LogicalStreamType) ||
          StreamTypeHelper.IsAudioStream(es.LogicalStreamType) ||
          es.LogicalStreamType == LogicalStreamType.Subtitles ||
          es.LogicalStreamType == LogicalStreamType.Teletext)
        {
          tempCaPmt[offset++] = (byte)es.StreamType;
          tempCaPmt[offset++] = (byte)((es.Pid >> 8) & 0x1f);
          tempCaPmt[offset++] = (byte)(es.Pid & 0xff);

          // Skip the ES_info_length field until we know what the length is.
          int esInfoLengthOffset = offset;
          offset += 2;

          // As per EN 50221, we only add conditional access descriptors to the CA PMT.
          int esInfoLength = 0;
          foreach (IDescriptor d in es.CaDescriptors)
          {
            if (esInfoLength == 0)
            {
              tempCaPmt[offset++] = (byte)command;
              esInfoLength++;
            }
            ReadOnlyCollection<byte> descriptorData = d.GetRawData();
            descriptorData.CopyTo(tempCaPmt, offset);
            offset += descriptorData.Count;
            esInfoLength += descriptorData.Count;
          }

          // Set the ES_info_length now that we know what the length is.
          tempCaPmt[esInfoLengthOffset++] = (byte)((esInfoLength >> 8) & 0x0f);
          tempCaPmt[esInfoLengthOffset] = (byte)(esInfoLength & 0xff);
        }
      }

      // There is no length output parameter, so we need to resize tempCaPmt to match the number of
      // meaningful CA PMT bytes.
      byte[] caPmt = new byte[offset];
      Buffer.BlockCopy(tempCaPmt, 0, caPmt, 0, offset);

      //Dump.DumpBinary(caPmt, caPmt.Length);

      return caPmt;
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      this.LogDebug("PMT: dump...");
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_rawPmt, _rawPmt.Length);
      this.LogDebug("  table ID                 = {0}", _tableId);
      this.LogDebug("  section syntax indicator = {0}", _sectionSyntaxIndicator);
      this.LogDebug("  section length           = {0}", _sectionLength);
      this.LogDebug("  program number           = {0}", _programNumber);
      this.LogDebug("  version                  = {0}", _version);
      this.LogDebug("  current next indicator   = {0}", _currentNextIndicator);
      this.LogDebug("  section number           = {0}", _sectionNumber);
      this.LogDebug("  last section number      = {0}", _lastSectionNumber);
      this.LogDebug("  PCR PID                  = {0}", _pcrPid);
      this.LogDebug("  program info length      = {0}", _programInfoLength);
      this.LogDebug("  CRC                      = 0x{0:x2}{1:x2}{2:x2}{3:x2}", _crc[0], _crc[1], _crc[2], _crc[3]);
      this.LogDebug("  {0} descriptor(s)...", _programDescriptors.Count + _programCaDescriptors.Count);
      foreach (IDescriptor d in _programDescriptors)
      {
        d.Dump();
      }
      foreach (IDescriptor cad in _programCaDescriptors)
      {
        cad.Dump();
      }
      this.LogDebug("  {0} elementary stream(s)...", _elementaryStreams.Count);
      foreach (PmtElementaryStream es in _elementaryStreams)
      {
        es.Dump();
      }
    }
  }

  /// <summary>
  /// A class capable of holding elementary stream information for one stream in a program map.
  /// </summary>
  public class PmtElementaryStream 
  {
    #region variables

    private StreamType _streamType;
    private ushort _pid;
    private ushort _esInfoLength;
    private List<IDescriptor> _descriptors;
    private List<IDescriptor> _caDescriptors;

    private LogicalStreamType _logicalStreamType;

    #endregion

    #region properties

    /// <summary>
    /// The elementary stream type.
    /// </summary>
    public StreamType StreamType
    {
      get
      {
        return _streamType;
      }
      internal set
      {
        _streamType = value;
      }
    }

    /// <summary>
    /// The elementary stream's PID.
    /// </summary>
    public ushort Pid
    {
      get
      {
        return _pid;
      }
      internal set
      {
        _pid = value;
      }
    }

    /// <summary>
    /// The total number of bytes in the elementary stream's descriptors.
    /// </summary>
    public ushort EsInfoLength
    {
      get
      {
        return _esInfoLength;
      }
      internal set
      {
        _esInfoLength = value;
      }
    }

    /// <summary>
    /// The elementary stream's descriptors. Conditional access descriptors are not included.
    /// </summary>
    public List<IDescriptor> Descriptors
    {
      get
      {
        return _descriptors;
      }
      internal set
      {
        _descriptors = value;
      }
    }

    /// <summary>
    /// The elementary stream's conditional access descriptors.
    /// </summary>
    public List<IDescriptor> CaDescriptors
    {
      get
      {
        return _caDescriptors;
      }
      internal set
      {
        _caDescriptors = value;
      }
    }

    /// <summary>
    /// The logical type or category of the elementary stream. This property can be used to quickly and
    /// precisely determine the stream type when the stream type would normally be indicated using one
    /// or more descriptors.
    /// </summary>
    public LogicalStreamType LogicalStreamType
    {
      get
      {
        return _logicalStreamType;
      }
      internal set
      {
        _logicalStreamType = value;
      }
    }

    #endregion

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      this.LogDebug("Elementary Stream: dump...");
      this.LogDebug("  stream type         = {0}", _streamType);
      this.LogDebug("  PID                 = {0}", _pid);
      this.LogDebug("  length              = {0}", _esInfoLength);
      this.LogDebug("  logical stream type = {0}", _logicalStreamType);
      this.LogDebug("  {0} descriptor(s)...", _descriptors.Count + _caDescriptors.Count);
      foreach (IDescriptor d in _descriptors)
      {
        d.Dump();
      }
      foreach (IDescriptor cad in _caDescriptors)
      {
        cad.Dump();
      }
    }
  }

  /// <summary>
  /// An interface that models the descriptor structure defined in ISO/IEC 13818-1.
  /// </summary>
  public interface IDescriptor
  {
    /// <summary>
    /// The descriptor's tag.
    /// </summary>
    DescriptorTag Tag { get; }

    /// <summary>
    /// The descriptor data length.
    /// </summary>
    byte Length { get; }

    /// <summary>
    /// The descriptor data.
    /// </summary>
    ReadOnlyCollection<byte> Data { get; }

    /// <summary>
    /// Retrieve a read-only copy of the original data that was decoded to create a descriptor instance.
    /// </summary>
    /// <remarks>
    /// The copy includes tag and length bytes.
    /// </remarks>
    /// <returns>a copy of the raw descriptor data</returns>
    ReadOnlyCollection<byte> GetRawData();

    /// <summary>
    /// Write the descriptor fields to the log file. Useful for debugging.
    /// </summary>
    void Dump();
  }

  /// <summary>
  /// A base class that implements the <see cref="IDescriptor"/> interface, modelling the basic descriptor
  /// structure defined in ISO/IEC 13818-1.
  /// </summary>
  public class Descriptor : IDescriptor
  {
    #region variables

    /// <summary>
    /// The descriptor's tag.
    /// </summary>
    protected DescriptorTag _tag;
    /// <summary>
    /// The descriptor data length.
    /// </summary>
    protected byte _length;
    /// <summary>
    /// The descriptor data.
    /// </summary>
    protected byte[] _data;

    /// <summary>
    /// The raw descriptor data (ie. including tag and length).
    /// </summary>
    protected byte[] _rawData;

    #endregion

    #region constructor

    /// <summary>
    /// Constructor. Protected to ensure instances can only be created by derived classes or by calling
    /// Decode(). This should ensure the safety of the parameters, which is why we don't check them.
    /// </summary>
    /// <param name="tag">The descriptor's tag.</param>
    /// <param name="length">The descriptor data length.</param>
    /// <param name="data">The descriptor data.</param>
    protected Descriptor(DescriptorTag tag, byte length, byte[] data)
    {
      _tag = tag;
      _length = length;
      // Make a copy of the data array so that changes in the caller's array don't affect our data.
      _data = new byte[data.Length];
      Buffer.BlockCopy(data, 0, _data, 0, data.Length);
    }

    /// <summary>
    /// Copy-constructor. Protected to ensure instances can only be created by derived classes or by calling
    /// Decode(). This should ensure the safety of the parameters, which is why we don't check them.
    /// </summary>
    /// <param name="descriptor">The descriptor to copy.</param>
    protected Descriptor(IDescriptor descriptor)
    {
      _tag = descriptor.Tag;
      _length = descriptor.Length;
      // Make a copy of the data array so that changes in the original descriptor data don't affect our data.
      _data = new byte[descriptor.Data.Count];
      descriptor.Data.CopyTo(_data, 0);
    }

    #endregion

    #region properties

    /// <summary>
    /// The descriptor's tag.
    /// </summary>
    public DescriptorTag Tag
    {
      get
      {
        return _tag;
      }
    }

    /// <summary>
    /// The descriptor data length.
    /// </summary>
    public byte Length
    {
      get
      {
        return _length;
      }
    }

    /// <summary>
    /// The descriptor data.
    /// </summary>
    public ReadOnlyCollection<byte> Data
    {
      get
      {
        return new ReadOnlyCollection<byte>(_data);
      }
    }

    #endregion

    /// <summary>
    /// Decode raw descriptor data.
    /// </summary>
    /// <param name="data">The raw descriptor data.</param>
    /// <param name="offset">The offset in the data array at which the descriptor starts.</param>
    /// <returns>an IDescriptor instance</returns>
    public static IDescriptor Decode(byte[] data, int offset)
    {
      // Parse the base descriptor fields. Return null if they're not valid for any reason.
      if (offset + 1 >= data.Length)
      {
        return null;
      }
      DescriptorTag tag = (DescriptorTag)data[offset];
      byte length = data[offset + 1];
      if (offset + 2 + length > data.Length)
      {
        return null;
      }

      // If we get to here, the descriptor data seems to be valid. Instantiate a descriptor.
      byte[] descData = new byte[length];
      Buffer.BlockCopy(data, offset + 2, descData, 0, length);
      Descriptor d = new Descriptor(tag, length, descData);

      // Make a copy of the entire descriptor so that changes made by the caller on the original array have
      // no effect on our reference/copy.
      d._rawData = new byte[2 + d._length];
      Buffer.BlockCopy(data, offset, d._rawData, 0, 2 + length);

      return d;
    }

    /// <summary>
    /// Retrieve a read-only copy of the original data that was decoded to create this Descriptor instance.
    /// </summary>
    /// <returns>a copy of the raw descriptor data</returns>
    public ReadOnlyCollection<byte> GetRawData()
    {
      return new ReadOnlyCollection<byte>(_rawData);
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public virtual void Dump()
    {
      this.LogDebug("Descriptor: dump...");
      this.LogDebug("  tag    = {0}", _tag);
      this.LogDebug("  length = {0}", _length);
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_data, _data.Length);
    }
  }

  /// <summary>
  /// A class that models the conditional access descriptor structure defined in ISO/IEC 13818-1.
  /// </summary>
  public class ConditionalAccessDescriptor : Descriptor
  {
    #region variables

    private ushort _caSystemId;
    private ushort _caPid;
    private byte[] _privateData;
    private Dictionary<ushort, HashSet<uint>> _pids;

    #endregion

    /// <summary>
    /// Constructor. Protected to ensure instances can only be created by derived classes or by calling
    /// Decode().
    /// </summary>
    /// <param name="descriptor">The base descriptor to use to instantiate this descriptor.</param>
    protected ConditionalAccessDescriptor(IDescriptor descriptor)
      : base(descriptor)
    {
    }

    #region properties

    /// <summary>
    /// The type of CA system applicable for the CA PID.
    /// </summary>
    public ushort CaSystemId
    {
      get
      {
        return _caSystemId;
      }
    }

    /// <summary>
    /// An encryption control or management message PID associated with the service to which this descriptor
    /// is attached.
    /// </summary>
    public ushort CaPid
    {
      get
      {
        return _caPid;
      }
    }

    /// <summary>
    /// The private conditional access system data.
    /// </summary>
    public ReadOnlyCollection<byte> PrivateData
    {
      get
      {
        return new ReadOnlyCollection<byte>(_privateData);
      }
    }

    /// <summary>
    /// A dictionary of ECM/EMM PIDs and their associated provider ID, interpretted from the private
    /// descriptor data.
    /// </summary>
    /// <remarks>
    /// This dictionary should be treated as read-only. Ideally we should enforce that, but there is currently no
    /// built in type for this.
    /// </remarks>
    public Dictionary<ushort, HashSet<uint>> Pids
    {
      get
      {
        return _pids;
      }
    }

    #endregion

    /// <summary>
    /// Attempt to decode an arbitrary descriptor as a conditional access descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor to decode.</param>
    /// <returns>a fully populated ConditionalAccessDescriptor instance if decoding is successful, otherwise <c>null</c></returns>
    public static ConditionalAccessDescriptor Decode(IDescriptor descriptor)
    {
      if (descriptor.Tag != DescriptorTag.ConditionalAccess || descriptor.Length < 4 ||
        descriptor.Data == null || descriptor.Data.Count < 4)
      {
        return null;
      }
      ConditionalAccessDescriptor d = new ConditionalAccessDescriptor(descriptor);

      // Standard fields.
      d._caSystemId = (ushort)((descriptor.Data[0] << 8) + descriptor.Data[1]);
      d._caPid = (ushort)(((descriptor.Data[2] & 0x1f) << 8) + descriptor.Data[3]);

      // Make our own copy of the private data.
      d._privateData = new byte[d._length - 4];
      Buffer.BlockCopy(d._data, 4, d._privateData, 0, d._length - 4);

      // Build a dictionary of PID info.
      uint providerId;
      d._pids = new Dictionary<ushort, HashSet<uint>>(); // PID -> provider ID(s)
      d._pids.Add(d._caPid, new HashSet<uint>());

      // Canal Plus
      if (d._caSystemId == 0x100)
      {
        HandleCanalPlusDescriptor(d);
        return d;
      }
      // Nagra
      else if ((d._caSystemId & 0xff00) == 0x1800)
      {
        HandleNagraDescriptor(d);
        return d;
      }

      // Default - most other CA systems (eg. Irdeto) don't include private data. Via Access (0x0500)
      // does. We use this as the default handling.
      int offset = 0;
      while (offset + 1 < d._privateData.Length)
      {
        byte tagInd = d._privateData[offset++];
        byte tagLen = d._privateData[offset++];
        if (offset + tagLen <= d._privateData.Length)
        {
          if (tagInd == 0x14 && tagLen >= 2)  // Tag 0x14 is the Via Access provider ID.
          {
            providerId = (uint)((d._privateData[offset] << 16) + (d._privateData[offset + 1] << 8) +
                              d._privateData[offset + 2]);
            // Some providers (eg. Boxer) send wrong information in the lower 4 bits of the provider ID,
            // so reset the lower 4 bits for Via Access.
            if (d._caSystemId == 0x500)
            {
              providerId = providerId & 0xfffffff0;
            }
            d._pids[d._caPid].Add(providerId);
          }
        }
        offset += tagLen;
      }

      return d;
    }

    #region proprietary descriptor format handling

    private static void HandleCanalPlusDescriptor(ConditionalAccessDescriptor d)
    {
      int offset = 0;
      ushort pid;
      uint providerId;

      // There are two formats...
      if (d._privateData.Length >= 3 && d._privateData[2] == 0xff)
      {
        // For this format, there is a loop of PID and provider ID info.
        #region examples
        //  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
        // 09 11 01 00 E6 43 00 6A FF 00 00 00 00 00 00 02 14 21 8C
        // 09 11 01 00 F6 BD 00 6D FF FF E0 00 00 00 00 00 00 2B 6D
        // 09 11 01 00 E6 1C 41 01 FF FF FF FF FF FF FF FF FF 21 8C
        // 09 11 01 00 F6 A1 33 17 FF 40 20 0C 00 00 00 00 04 2C E3 F6 9F 33 11 FF 00 00 0C 00 08 00 00 02 2C E3 F6 A0 A8 21 FF 51 70 C1 03 00 00 01 4E 2C E3

        // 09       CA descriptor tag
        // 11       total descriptor length (minus tag and length byte)
        // 01 00    CA system ID
        // f6 a1    PID
        // 33 17    provider ID
        // ff 40 20 0c 00 00 00 00 04 2c e3  (unknown)
        // f6 9f    PID
        // 33 11    provider ID
        // ff 00 00 0c 00 08 00 00 02 2c e3  (unknown)
        // f6 a0    PID
        // a8 21    provider ID
        // ff 51 70 c1 03 00 00 01 4e 2c e3  (unknown)
        #endregion

        // Handle the first provider ID "manually" - it is associated with the standardised PID.
        providerId = (uint)(((d._privateData[0] & 0x1f) << 8) + d._privateData[1]);
        d._pids[d._caPid].Add(providerId);

        offset = 13;
        while (offset + 3 < d._privateData.Length)
        {
          pid = (ushort)(((d._privateData[offset] & 0x1f) << 8) + d._privateData[offset + 1]);
          providerId = (uint)((d._privateData[offset + 2] << 8) + d._privateData[offset + 3]);

          HashSet<uint> pidProviders;
          if (d._pids.TryGetValue(pid, out pidProviders))
          {
            pidProviders.Add(providerId);
          }
          else
          {
            pidProviders = new HashSet<uint>() { providerId };
            d._pids.Add(pid, pidProviders);
          }
          offset += 15;
        }
      }
      else if (d._privateData.Length >= 5 && d._privateData[2] != 0xff)
      {
        // For this format, there are a variable number of PID and provider ID pairs.
        #region examples
        //  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
        // 09 11 01 00 E0 C1 03 E0 92 41 01 E0 93 40 01 E0 C4 00 64
        // 09 0D 01 00 E0 B6 02 E0 B7 00 6A E0 B9 00 6C

        // 09       CA descriptor tag
        // 0d       total descriptor length (minus tag and length byte)
        // 01 00    CA system ID
        // e0 b6    PID
        // 02       "additional PID pair count" (not always accurate)
        //   e0 b7  PID
        //   00 6a  provider ID
        //   e0 b9  PID
        //   00 6c  provider ID
        #endregion

        int extraPidPairs = d._privateData[offset++];
        while (offset + 3 < d._privateData.Length)
        {
          pid = (ushort)(((d._privateData[offset] & 0x1f) << 8) + d._privateData[offset + 1]);
          providerId = (uint)((d._privateData[offset + 2] << 8) + d._privateData[offset + 3]);
          offset += 4;

          HashSet<uint> pidProviders;
          if (d._pids.TryGetValue(pid, out pidProviders))
          {
            pidProviders.Add(providerId);
          }
          else
          {
            pidProviders = new HashSet<uint> { providerId };
            d._pids.Add(pid, pidProviders);
          }
        }
      }
    }

    private static void HandleNagraDescriptor(ConditionalAccessDescriptor d)
    {
      #region examples
      //  0  1  2  3  4  5  6  7  8  9 10 11 12
      // 09 07 18 11 E2 BD 02 33 11
      // 09 09 18 63 E2 C7 04 33 42 33 43
      // 09 0B 18 63 E2 C8 06 33 41 33 42 33 43

      // 09       CA descriptor tag
      // 11       total descriptor length (minus tag and length byte)
      // 18 63    CA system ID
      // e2 c8    PID
      // 06       number of bytes containing provider IDs
      //   33 41  provider ID
      //   33 42  provider ID
      //   33 43  provider ID
      #endregion

      int offset = 1;
      while (offset + 1 < d._privateData.Length)
      {
        uint providerId = (uint)((d._privateData[offset] << 8) + d._privateData[offset + 1]);
        d._pids[d._caPid].Add(providerId);
        offset += 2;
      }
    }

    #endregion

    /// <summary>
    /// For debug use.
    /// </summary>
    public override void Dump()
    {
      this.LogDebug("CA Descriptor: dump...");
      this.LogDebug("  tag          = {0}", _tag);
      this.LogDebug("  length       = {0}", _length);
      this.LogDebug("  CA system ID = {0}", _caSystemId);
      this.LogDebug("  CA PID       = {0}", _caPid);
      foreach (KeyValuePair<ushort, HashSet<uint>> pid in _pids)
      {
        this.LogDebug("  PID = {0}, provider IDs = {1}", pid.Key, string.Join(", ", pid.Value));
      }
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_data, _data.Length);
    }
  }
}
