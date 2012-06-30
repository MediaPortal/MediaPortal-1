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
using System.Text;
using System.Collections.Generic;

namespace TvLibrary.Interfaces
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
    /// Conditional access information programme map table.
    /// </summary>
    ConditionalAccessPmt,
    /// <summary>
    /// Conditional access information programme map table response.
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
    Mpeg1Part2Video = 1,
    /// <summary>
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream (MPEG 2 video)
    /// </summary>
    Mpeg2Part2Video,
    /// <summary>
    /// MPEG ISO/IEC 11172-3 audio (MPEG 1 audio)
    /// </summary>
    Mpeg1Part3Audio,
    /// <summary>
    /// MPEG ISO/IEC 13818-3 audio (MPEG 2 audio)
    /// </summary>
    Mpeg2Part3Audio,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 private_sections
    /// </summary>
    Mpeg2Part1PrivateSection,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing private data
    /// </summary>
    Mpeg2Part1PrivateData,
    /// <summary>
    /// MPEG ISO/IEC 13522 MHEG
    /// </summary>
    Mheg5,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Annex A DSM-CC
    /// </summary>
    AnnexADsmCc,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.1
    /// </summary>
    MediaMultiplex,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type A - multi-protocol encapsulation
    /// </summary>
    Mpeg2Part6DsmCcMultiProtocolEncapsulation,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type B - DSM-CC U-N messages
    /// </summary>
    Mpeg2Part6DsmCcUnMessages,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type C - DSM-CC stream descriptors
    /// </summary>
    Mpeg2Part6DsmCcStreamDescriptor,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 type D - DSM-CC sections (any type, including private data) or DSM-CC addressable sections
    /// </summary>
    Mpeg2Part6DsmCcSection,
    /// <summary>
    /// MPEG ITU-T Rec. H.222.0 | ISO/IEC 13818-1 auxiliary
    /// </summary>
    Mpeg2Part1Auxiliary,
    /// <summary>
    /// MPEG ISO/IEC 13818-7 audio with ADTS transport syntax (AAC audio)
    /// </summary>
    Mpeg2Part7Audio,
    /// <summary>
    /// MPEG ISO/IEC 14496-2 visual (MPEG 4)
    /// </summary>
    Mpeg4Part2Video,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio with the LATM transport syntax (LATM-AAC audio)
    /// </summary>
    Mpeg4Part3Audio,
    /// <summary>
    /// MPEG ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in PES packets
    /// </summary>
    Mpeg4Part1PesPacketisedStream,
    /// <summary>
    /// MPEG ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in ISO/IEC 14496_sections
    /// </summary>
    Mpeg4Part1PacketisedStream,
    /// <summary>
    /// MPEG ISO/IEC 13818-6 synchronized download protocol
    /// </summary>
    Mpeg2Part6SynchronisedDownloadProtocol,
    /// <summary>
    /// MPEG metadata carried in PES packets
    /// </summary>
    PesMetadata,
    /// <summary>
    /// MPEG metadata carried in metadata_sections
    /// </summary>
    SectionMetadata,
    /// <summary>
    /// MPEG metadata carried in ISO/IEC 13818-6 data carousel
    /// </summary>
    Mpeg2Part6DataCarousel,
    /// <summary>
    /// MPEG metadata carried in ISO/IEC 13818-6 object carousel
    /// </summary>
    Mpeg2Part6ObjectCarousel,
    /// <summary>
    /// MPEG metadata carried in ISO/IEC 13818-6 synchronized download protocol
    /// </summary>
    Mpeg2Part6SynchronisedDownloadProtocolMetadata,
    /// <summary>
    /// MPEG IPMP stream defined in ISO/IEC 13818-11
    /// </summary>
    Mpeg4Part11Ipmp,
    /// <summary>
    /// MPEG AVC video stream as defined in ITU-T Rec. H.264 | ISO/IEC 14496-10 video
    /// </summary>
    Mpeg4Part10Video = 0x1b,

    // 0x1c - 0x7e  ITU-T Rec. H.222.0 | ISO/IEC 13818-1 reserved

    #endregion

    #region ATSC A-53 part 3

    /// <summary>
    /// ATSC AC3 audio
    /// </summary>
    Ac3Audio = 0x81,
    /// <summary>
    /// ATSC E-AC3 audio
    /// </summary>
    EnhancedAc3Audio = 0x87

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

    // 45 - 63 ITU-T Rec. H.222.0 | ISO/IEC 13818-1 reserved

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
    /// DVB cable deliver system descriptor
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
    TimeShiftedService,
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
    /// DVB stream identifier descriptor
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

    #region ATSC A-53 part 3

    /// <summary>
    /// ATSC AC3 audio descriptor
    /// </summary>
    Ac3Audio = 0x81,
    /// <summary>
    /// ATSC private information descriptor
    /// </summary>
    AtscPrivateInformation = 0xad,
    /// <summary>
    /// ATSC enhanced signalling descriptor
    /// </summary>
    EnhancedSignalling = 0xb2

    #endregion
  }

  /// <summary>
  /// DVB service types.
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
    /// Near Video On Demand reference service
    /// </summary>
    NvodReference = 0x04,
    /// <summary>
    /// Near Video On Demand time-shifted service
    /// </summary>
    NvodTimeShifted = 0x05,
    /// <summary>
    /// mosaic service
    /// </summary>
    Mosaic = 0x06,
    /// <summary>
    /// FM radio service
    /// </summary>
    FmRadio = 0x07,
    /// <summary>
    /// DVB System Renewability Messages service
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
    /// Return Channel via Satellite map
    /// </summary>
    RcsMap = 0x0e,

    /// <summary>
    /// Return Channel via Satellite Forward Link Signalling
    /// </summary>
    RcsFls = 0x0f,
    /// <summary>
    /// DVB Multimedia Home Platform service
    /// </summary>
    DvbMhp = 0x10,
    /// <summary>
    /// MPEG 2 HD digital television service
    /// </summary>
    Mpeg2HdDigitalTelevision = 0x11,

    // (0x12 to 0x15 reserved)

    /// <summary>
    /// advanced codec SD digital television service
    /// </summary>
    AdvancedCodecSdDigitalTelevision = 0x16,
    /// <summary>
    /// advanced codec SD Near Video On Demand time-shifted service
    /// </summary>
    AdvancedCodecSdNvodTimeShifted = 0x17,
    /// <summary>
    /// advanced codec SD Near Video On Demand reference service
    /// </summary>
    AdvancedCodecSdNvodReference = 0x18,
    /// <summary>
    /// advanced codec HD digital television
    /// </summary>
    AdvancedCodecHdDigitalTelevision = 0x19,
    /// <summary>
    /// advanced codec HD Near Video On Demand time-shifted service
    /// </summary>
    AdvancedCodecHdNvodTimeShifted = 0x1a,
    /// <summary>
    /// advanced codec HD Near Video On Demand reference service
    /// </summary>
    AdvancedCodecHdNvodReference = 0x1b,

    // (0x1c to 0x7f reserved)
    // (0x80 to 0xfe user defined)

    /// <summary>
    /// Sky Germany portal service (also known as linked or option services)
    /// </summary>
    SkyGermanyOptionChannel = 0xd3

    // (0xff reserved)
  }

  /// <summary>
  /// ATSC service types.
  /// </summary>
  public enum AtscServiceType
  {
    /// <summary>
    /// analog television (see A/65 [9])
    /// </summary>
    AnalogTelevision = 0x01,
    /// <summary>
    /// ATSC digital television (see A/53 part 3 [2])
    /// </summary>
    DigitalTelevision = 0x02,
    /// <summary>
    /// ATSC audio (see A/53 part 3 [2])
    /// </summary>
    Audio = 0x03
  }

  #endregion

  /// <summary>
  /// A class that models the transport stream conditional access table section defined in ISO/IEC 13818-1.
  /// </summary>
  public class Cat
  {
    #region variables

    private byte _tableId;
    private byte _sectionSyntaxIndicator;
    private UInt16 _sectionLength;
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
    public UInt16 SectionLength
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
    public List<IDescriptor> Descriptors
    {
      get
      {
        return _descriptors;
      }
    }

    /// <summary>
    /// The conditional access descriptors for the service described by the conditional access table.
    /// </summary>
    public List<IDescriptor> CaDescriptors
    {
      get
      {
        return _caDescriptors;
      }
    }

    /// <summary>
    /// Cyclic redundancy check bytes for confirming the integrity of the conditional access section data.
    /// </summary>
    public byte[] Crc
    {
      get
      {
        return _crc;
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
      Log.Log.Debug("CAT: decode");
      if (data == null || data.Length < 12)
      {
        Log.Log.Debug("CAT: CAT not supplied or too short");
        return null;
      }

      if (data[0] != 0x01)
      {
        Log.Log.Debug("CAT: invalid table ID");
        return null;
      }
      if ((data[1] & 0x80) != 0x80)
      {
        Log.Log.Debug("CAT: section syntax indicator is not 1");
        return null;
      }
      if ((data[1] & 0x40) != 0)
      {
        Log.Log.Debug("CAT: corruption detected (zero)");
        return null;
      }

      Cat cat = new Cat();
      cat._tableId = data[0];
      cat._sectionSyntaxIndicator = (byte)(data[1] & 0x80);
      cat._sectionLength = (UInt16)(((data[1] & 0x0f) << 8) + data[2]);
      if (3 + cat._sectionLength > data.Length)
      {
        Log.Log.Debug("CAT: section length is invalid");
        return null;
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
          Log.Log.Debug("CAT: descriptor {0} is invalid", cat._descriptors.Count + cat._caDescriptors.Count + 1);
          return null;
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
        Log.Log.Debug("CAT: corruption detected (descriptors)");
        return null;
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

    /// <summary>
    /// Retrieve a copy of the original conditional access section data that was decoded to create this Cat instance.
    /// </summary>
    /// <returns>a copy of the raw conditional access section data</returns>
    public byte[] GetRawCatCopy()
    {
      // Make a copy of our raw CAT for the caller.
      byte[] outputCat = new byte[_rawCat.Length];
      Buffer.BlockCopy(_rawCat, 0, outputCat, 0, _rawCat.Length);
      return outputCat;
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      Log.Log.Debug("CAT: dump...");
      DVB_MMI.DumpBinary(_rawCat, 0, _rawCat.Length);
      Log.Log.Debug("  table ID                 = {0}", _tableId);
      Log.Log.Debug("  section syntax indicator = {0}", _sectionSyntaxIndicator);
      Log.Log.Debug("  section length           = {0}", _sectionLength);
      Log.Log.Debug("  version                  = {0}", _version);
      Log.Log.Debug("  current next indicator   = {0}", _currentNextIndicator);
      Log.Log.Debug("  section number           = {0}", _sectionNumber);
      Log.Log.Debug("  last section number      = {0}", _lastSectionNumber);
      Log.Log.Debug("  CRC                      = 0x{0:x}{1:x}{2:x}{3:x}", _crc[0], _crc[1], _crc[2], _crc[3]);
      Log.Log.Debug("  {0} descriptor(s)...", _descriptors.Count + _caDescriptors.Count);
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
    #region variables

    private byte _tableId;
    private byte _sectionSyntaxIndicator;
    private UInt16 _sectionLength;
    private UInt16 _programNumber;
    private byte _version;
    private byte _currentNextIndicator;
    private byte _sectionNumber;
    private byte _lastSectionNumber;
    private UInt16 _pcrPid;
    private UInt16 _programInfoLength;
    private List<IDescriptor> _programDescriptors;
    private List<IDescriptor> _programCaDescriptors;
    private List<PmtElementaryStream> _elementaryStreams;
    private byte[] _crc;

    private byte[] _rawPmt;

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
    public UInt16 SectionLength
    {
      get
      {
        return _sectionLength;
      }
    }

    /// <summary>
    /// The program number (service ID) of the service that the program map describes.
    /// </summary>
    public UInt16 ProgramNumber
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
    public UInt16 PcrPid
    {
      get
      {
        return _pcrPid;
      }
    }

    /// <summary>
    /// The total number of bytes in the program map program descriptors.
    /// </summary>
    public UInt16 ProgramInfoLength
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
    public List<IDescriptor> ProgramDescriptors
    {
      get
      {
        return _programDescriptors;
      }
    }

    /// <summary>
    /// The conditional access descriptors for the service described by the program map.
    /// </summary>
    public List<IDescriptor> ProgramCaDescriptors
    {
      get
      {
        return _programCaDescriptors;
      }
    }

    /// <summary>
    /// The elementary streams described in the program map.
    /// </summary>
    public List<PmtElementaryStream> ElementaryStreams
    {
      get
      {
        return _elementaryStreams;
      }
    }

    /// <summary>
    /// Cyclic redundancy check bytes for confirming the integrity of the program map section data.
    /// </summary>
    public byte[] Crc
    {
      get
      {
        return _crc;
      }
    }

    #endregion

    /// <summary>
    /// Decode and check the validity of raw program map section data.
    /// </summary>
    /// <param name="data">The raw program map section data.</param>
    /// <param name="camType">The type of CAM that the program map will be passed to.</param>
    /// <returns>a fully populated Pmt instance if the section is valid, otherwise <c>null</c></returns>
    public static Pmt Decode(byte[] data, CamType camType)
    {
      Log.Log.Debug("PMT: decode, CAM type = {0}", camType);
      if (data == null || data.Length < 16)
      {
        Log.Log.Debug("PMT: PMT not supplied or too short");
        return null;
      }

      if (data[0] != 0x02)
      {
        Log.Log.Debug("PMT: invalid table ID");
        return null;
      }
      if ((data[1] & 0x80) != 0x80)
      {
        Log.Log.Debug("PMT: section syntax indicator is not 1");
        return null;
      }
      if ((data[1] & 0x40) != 0)
      {
        Log.Log.Debug("PMT: corruption detected (zero)");
        return null;
      }
      if (data[6] != 0)
      {
        Log.Log.Debug("PMT: section number is not zero");
        return null;
      }
      if (data[7] != 0)
      {
        Log.Log.Debug("PMT: last section number is not zero");
        return null;
      }

      Pmt pmt = new Pmt();
      pmt._tableId = data[0];
      pmt._sectionSyntaxIndicator = (byte)(data[1] & 0x80);
      pmt._sectionLength = (UInt16)(((data[1] & 0x0f) << 8) + data[2]);
      if (3 + pmt._sectionLength != data.Length)
      {
        Log.Log.Debug("PMT: section length is invalid");
        return null;
      }

      pmt._programNumber = (UInt16)((data[3] << 8) + data[4]);
      pmt._version = (byte)((data[5] & 0x3e) >> 1);
      pmt._currentNextIndicator = (byte)(data[5] & 0x01);
      pmt._sectionNumber = data[6];
      pmt._lastSectionNumber = data[7];
      pmt._pcrPid = (UInt16)(((data[8] & 0x1f) << 8) + data[9]);
      pmt._programInfoLength = (UInt16)(((data[10] & 0x0f) << 8) + data[11]);
      if (12 + pmt._programInfoLength + 4 > data.Length)
      {
        Log.Log.Debug("PMT: program info length is invalid");
        return null;
      }

      // Program descriptors.
      int offset = 12;
      int endProgramDescriptors = offset + pmt._programInfoLength;
      pmt._programDescriptors = new List<IDescriptor>();
      pmt._programCaDescriptors = new List<IDescriptor>();
      while (offset + 1 < endProgramDescriptors)
      {
        IDescriptor d = Descriptor.Decode(data, offset);
        if (d == null)
        {
          Log.Log.Debug("PMT: program descriptor {0} is invalid", pmt._programDescriptors.Count + pmt._programCaDescriptors.Count + 1);
          return null;
        }
        offset += d.Length + 2;
        if (d.Tag == DescriptorTag.ConditionalAccess)
        {
          pmt._programCaDescriptors.Add(d);
        }
        else
        {
          pmt._programDescriptors.Add(d);
        }
      }
      if (offset != endProgramDescriptors)
      {
        Log.Log.Debug("PMT: corruption detected (program descriptors)");
        return null;
      }

      // Elementary streams.
      pmt._elementaryStreams = new List<PmtElementaryStream>();
      int endEsData = data.Length - 4;
      while (offset + 4 < endEsData)
      {
        PmtElementaryStream es = new PmtElementaryStream();
        es.PrimaryDescriptorTag = DescriptorTag.Reserved;
        es.StreamType = (StreamType)data[offset++];
        es.Pid = (UInt16)(((data[offset] & 0x1f) << 8) + data[offset + 1]);
        offset += 2;
        es.EsInfoLength = (UInt16)(((data[offset] & 0x0f) << 8) + data[offset + 1]);
        offset += 2;

        // Elementary stream descriptors.
        int endEsDescriptors = offset + es.EsInfoLength;
        if (endEsDescriptors > endEsData)
        {
          Log.Log.Debug("PMT: elementary stream info length for PID {0} (0x{0:x}) is invalid", es.Pid);
          return null;
        }
        es.Descriptors = new List<IDescriptor>();
        es.CaDescriptors = new List<IDescriptor>();
        while (offset + 1 < endEsDescriptors)
        {
          IDescriptor d = Descriptor.Decode(data, offset);
          if (d == null)
          {
            Log.Log.Debug("PMT: elementary stream descriptor {0} for PID {1} (0x{1:x}) is invalid", es.Descriptors.Count + es.CaDescriptors.Count + 1, es.Pid);
            return null;
          }
          offset += d.Length + 2;
          if (d.Tag == DescriptorTag.ConditionalAccess)
          {
            es.CaDescriptors.Add(d);
          }
          else
          {
            es.Descriptors.Add(d);
            if (d.Tag == DescriptorTag.VideoStream ||
              d.Tag == DescriptorTag.Mpeg4Video ||
              d.Tag == DescriptorTag.AvcVideo ||
              d.Tag == DescriptorTag.AudioStream ||
              d.Tag == DescriptorTag.Mpeg4Audio ||
              d.Tag == DescriptorTag.Mpeg2AacAudio ||
              d.Tag == DescriptorTag.Aac ||
              d.Tag == DescriptorTag.Ac3 ||        // DVB
              d.Tag == DescriptorTag.Ac3Audio ||   // ATSC
              d.Tag == DescriptorTag.EnhancedAc3 ||
              d.Tag == DescriptorTag.Dts ||
              d.Tag == DescriptorTag.Subtitling ||
              d.Tag == DescriptorTag.Teletext ||
              d.Tag == DescriptorTag.VbiTeletext)
            {
              es.PrimaryDescriptorTag = d.Tag;
            }
          }
        }
        if (offset != endEsDescriptors)
        {
          Log.Log.Debug("PMT: corruption detected (elementary stream descriptors)");
          return null;
        }

        pmt.ElementaryStreams.Add(es);
      }
      if (offset != endEsData)
      {
        Log.Log.Debug("PMT: corruption detected (elementary stream data)");
        return null;
      }

      pmt._crc = new byte[4];
      Buffer.BlockCopy(data, offset, pmt._crc, 0, 4);

      // Make a copy of the PMT so that changes made by the caller on the original array have no effect on
      // our reference/copy.
      pmt._rawPmt = new byte[data.Length];
      Buffer.BlockCopy(data, 0, pmt._rawPmt, 0, data.Length);

      // One last thing: for Astoncrypt 2 CAMs, we patch the stream type on AC3 streams...
      if (camType == CamType.Astoncrypt2)
      {
        // Move to the first ES stream type.
        offset = 12 + pmt._programInfoLength;
        foreach (PmtElementaryStream es in pmt._elementaryStreams)
        {
          if (es.StreamType == StreamType.Mpeg2Part1PrivateData && es.PrimaryDescriptorTag == DescriptorTag.Ac3)
          {
            es.StreamType = StreamType.Ac3Audio;
            pmt._rawPmt[offset] = (byte)StreamType.Ac3Audio;
          }
          offset += 5 + es.EsInfoLength;
        }
      }

      //pmt.Dump();

      return pmt;
    }

    /// <summary>
    /// Retrieve a copy of the original program map section data that was decoded to create this Pmt instance.
    /// </summary>
    /// <returns>a copy of the raw program map section data</returns>
    public byte[] GetRawPmtCopy()
    {
      // Make a copy of our raw PMT for the caller.
      byte[] outputPmt = new byte[_rawPmt.Length];
      Buffer.BlockCopy(_rawPmt, 0, outputPmt, 0, _rawPmt.Length);
      return outputPmt;
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
      Log.Log.Debug("PMT: get CA PMT, list action = {0}, command = {1}", listAction, command);
      byte[] tempCaPmt = new byte[4096];
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
        byte[] descriptorData = d.GetRawDataCopy();
        Buffer.BlockCopy(descriptorData, 0, tempCaPmt, offset, descriptorData.Length);
        offset += descriptorData.Length;
        programInfoLength += descriptorData.Length;
      }

      // Set the program_info_length now that we know what the length is.
      tempCaPmt[4] = (byte)((programInfoLength >> 8) & 0x0f);
      tempCaPmt[5] = (byte)(programInfoLength & 0xff);

      // Elementary streams.
      foreach (PmtElementaryStream es in _elementaryStreams)
      {
        // We add each video, audio, subtitle and teletext stream with their corresponding conditional access
        // descriptors to the CA PMT. The first trick is to find out if the stream is one that we want to
        // include. That requires checking the stream type and primary descriptor type.
        if (es.StreamType == StreamType.Mpeg1Part2Video ||
          es.StreamType == StreamType.Mpeg2Part2Video ||
          es.StreamType == StreamType.Mpeg4Part2Video ||
          es.StreamType == StreamType.Mpeg4Part10Video ||
          es.StreamType == StreamType.Mpeg1Part3Audio ||
          es.StreamType == StreamType.Mpeg2Part3Audio ||
          es.StreamType == StreamType.Mpeg2Part7Audio ||
          es.StreamType == StreamType.Mpeg4Part3Audio ||
          es.StreamType == StreamType.Ac3Audio ||
          es.StreamType == StreamType.EnhancedAc3Audio ||
          es.PrimaryDescriptorTag == DescriptorTag.VideoStream ||
          es.PrimaryDescriptorTag == DescriptorTag.Mpeg4Video ||
          es.PrimaryDescriptorTag == DescriptorTag.AvcVideo ||
          es.PrimaryDescriptorTag == DescriptorTag.AudioStream ||
          es.PrimaryDescriptorTag == DescriptorTag.Mpeg4Audio ||
          es.PrimaryDescriptorTag == DescriptorTag.Mpeg2AacAudio ||
          es.PrimaryDescriptorTag == DescriptorTag.Aac ||
          es.PrimaryDescriptorTag == DescriptorTag.Ac3 ||        // DVB
          es.PrimaryDescriptorTag == DescriptorTag.Ac3Audio ||   // ATSC
          es.PrimaryDescriptorTag == DescriptorTag.EnhancedAc3 ||
          es.PrimaryDescriptorTag == DescriptorTag.Dts ||
          es.PrimaryDescriptorTag == DescriptorTag.Subtitling ||
          es.PrimaryDescriptorTag == DescriptorTag.Teletext ||
          es.PrimaryDescriptorTag == DescriptorTag.VbiTeletext)
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
            byte[] descriptorData = d.GetRawDataCopy();
            Buffer.BlockCopy(descriptorData, 0, tempCaPmt, offset, descriptorData.Length);
            offset += descriptorData.Length;
            esInfoLength += descriptorData.Length;
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

      //DVB_MMI.DumpBinary(caPmt, 0, caPmt.Length);

      return caPmt;
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      Log.Log.Debug("PMT: dump...");
      DVB_MMI.DumpBinary(_rawPmt, 0, _rawPmt.Length);
      Log.Log.Debug("  table ID                 = {0}", _tableId);
      Log.Log.Debug("  section syntax indicator = {0}", _sectionSyntaxIndicator);
      Log.Log.Debug("  section length           = {0}", _sectionLength);
      Log.Log.Debug("  program number           = {0} (0x{0:x})", _programNumber);
      Log.Log.Debug("  version                  = {0}", _version);
      Log.Log.Debug("  current next indicator   = {0}", _currentNextIndicator);
      Log.Log.Debug("  section number           = {0}", _sectionNumber);
      Log.Log.Debug("  last section number      = {0}", _lastSectionNumber);
      Log.Log.Debug("  PCR PID                  = {0} (0x{0:x})", _pcrPid);
      Log.Log.Debug("  program info length      = {0}", _programInfoLength);
      Log.Log.Debug("  CRC                      = 0x{0:x}{1:x}{2:x}{3:x}", _crc[0], _crc[1], _crc[2], _crc[3]);
      Log.Log.Debug("  {0} descriptor(s)...", _programDescriptors.Count + _programCaDescriptors.Count);
      foreach (IDescriptor d in _programDescriptors)
      {
        d.Dump();
      }
      foreach (IDescriptor cad in _programCaDescriptors)
      {
        cad.Dump();
      }
      Log.Log.Debug("  {0} elementary stream(s)...", _elementaryStreams.Count);
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
    private UInt16 _pid;
    private UInt16 _esInfoLength;
    private List<IDescriptor> _descriptors;
    private List<IDescriptor> _caDescriptors;

    private DescriptorTag _primaryDescriptorTag;

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
      set
      {
        _streamType = value;
      }
    }

    /// <summary>
    /// The elementary stream's PID.
    /// </summary>
    public UInt16 Pid
    {
      get
      {
        return _pid;
      }
      set
      {
        _pid = value;
      }
    }

    /// <summary>
    /// The total number of bytes in the elementary stream's descriptors.
    /// </summary>
    public UInt16 EsInfoLength
    {
      get
      {
        return _esInfoLength;
      }
      set
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
      set
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
      set
      {
        _caDescriptors = value;
      }
    }

    /// <summary>
    /// The descriptor tag that best describes the elementary stream type. This property can be used to
    /// quickly and precisely determine the stream type when the stream type property is not specific enough.
    /// </summary>
    public DescriptorTag PrimaryDescriptorTag
    {
      get
      {
        return _primaryDescriptorTag;
      }
      set
      {
        _primaryDescriptorTag = value;
      }
    }

    #endregion

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      Log.Log.Debug("Elementary Stream: dump...");
      Log.Log.Debug("  stream type = {0}", _streamType);
      Log.Log.Debug("  PID         = {0} (0x{0:x})", _pid);
      Log.Log.Debug("  length      = {0}", _esInfoLength);
      Log.Log.Debug("  main tag    = {0}", _primaryDescriptorTag);
      Log.Log.Debug("  {0} descriptor(s)...", _descriptors.Count + _caDescriptors.Count);
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
    byte[] Data { get; }

    /// <summary>
    /// Retrieve a copy of the original data that was decoded to create a descriptor instance.
    /// </summary>
    /// <remarks>
    /// The copy includes tag and length bytes.
    /// </remarks>
    /// <returns>a copy of the raw descriptor data</returns>
    byte[] GetRawDataCopy();

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
      _data = data;
    }

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
    public byte[] Data
    {
      get
      {
        return _data;
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
      if (offset + 2 + length >= data.Length)
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
    /// Retrieve a copy of the original data that was decoded to create this Descriptor instance.
    /// </summary>
    /// <returns>a copy of the raw descriptor data</returns>
    public byte[] GetRawDataCopy()
    {
      // Make a copy of our raw data for the caller.
      byte[] outputDescriptor = new byte[_rawData.Length];
      Buffer.BlockCopy(_rawData, 0, outputDescriptor, 0, _rawData.Length);
      return outputDescriptor;
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public virtual void Dump()
    {
      Log.Log.Debug("Descriptor: dump...");
      Log.Log.Debug("  tag    = {0}", _tag);
      Log.Log.Debug("  length = {0}", _length);
      DVB_MMI.DumpBinary(_data, 0, _data.Length);
    }
  }

  /// <summary>
  /// A class that models the conditional access descriptor structure defined in ISO/IEC 13818-1.
  /// </summary>
  public class ConditionalAccessDescriptor : Descriptor, IDescriptor
  {
    #region variables

    private UInt16 _caSystemId;
    private UInt16 _caPid;
    private byte[] _privateData;
    private Dictionary<UInt16, UInt32> _pids;

    #endregion

    /// <summary>
    /// Constructor. Protected to ensure instances can only be created by derived classes or by calling
    /// Decode().
    /// </summary>
    /// <param name="descriptor">The base descriptor to use to instantiate this descriptor.</param>
    protected ConditionalAccessDescriptor(IDescriptor descriptor)
      : base(descriptor.Tag, descriptor.Length, descriptor.Data)
    {
    }

    #region properties

    /// <summary>
    /// The type of CA system applicable for the CA PID.
    /// </summary>
    public UInt16 CaSystemId
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
    public UInt16 CaPid
    {
      get
      {
        return _caPid;
      }
    }

    /// <summary>
    /// The private conditional access system data.
    /// </summary>
    public byte[] PrivateData
    {
      get
      {
        return _privateData;
      }
    }

    /// <summary>
    /// A dictionary of ECM/EMM PIDs and their associated provider ID, interpretted from the private
    /// descriptor data.
    /// </summary>
    public Dictionary<UInt16, UInt32> Pids
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
        descriptor.Data == null || descriptor.Data.Length < 4)
      {
        return null;
      }
      ConditionalAccessDescriptor d = new ConditionalAccessDescriptor(descriptor);

      byte offset = 0;
      d._caSystemId = (UInt16)((descriptor.Data[offset] << 8) + descriptor.Data[offset + 1]);
      offset += 2;
      d._caPid = (UInt16)(((descriptor.Data[offset] & 0x1f) << 8) + descriptor.Data[offset + 1]);
      offset += 2;

      d._privateData = new byte[d._length - 4];
      Buffer.BlockCopy(descriptor.Data, offset, d._privateData, 0, d._length - 4);

      // Canal Plus...
      UInt32 providerId;
      d._pids = new Dictionary<UInt16, UInt32>();
      if (d._caSystemId == 0x100)
      {
        if (d._privateData.Length >= 3 && d._privateData[2] == 0xff)
        {
          //  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
          // 09 11 01 00 E6 43 00 6A FF 00 00 00 00 00 00 02 14 21 8C
          // 09 11 01 00 F6 BD 00 6D FF FF E0 00 00 00 00 00 00 2B 6D
          // 09 11 01 00 E6 1C 41 01 FF FF FF FF FF FF FF FF FF 21 8C
          d._pids.Add(d._caPid, (UInt32)(((d._privateData[0] & 0x1f) << 8) + d._privateData[1]));
          // TODO: loop required here? Example of longer descriptor required.
        }
        else if (d._privateData.Length >= 5 && d._privateData[2] != 0xff)
        {
          d._pids.Add(d._caPid, 0);
          int extraPidPairs = d._privateData[0];
          offset = 1;
          while (offset + 3 < d._privateData.Length)
          {
            //  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
            // 09 11 01 00 E0 C1 03 E0 92 41 01 E0 93 40 01 E0 C4 00 64
            // 09 0D 01 00 E0 B6 02 E0 B7 00 6A E0 B9 00 6C
            // 09       CA descriptor tag
            // 0d       total descriptor length (minus tag and length byte)
            // 01 00    CA system ID
            // e0 b6    PID
            // 02       "additional PID pair count"
            //   e0 b7  PID
            //   00 6a  provider ID
            //   e0 b9  PID
            //   00 6c  provider ID
            UInt16 pid = (UInt16)(((d._privateData[offset] & 0x1f) << 8) + d._privateData[offset + 1]);
            offset += 2;
            providerId = (UInt32)((d._privateData[offset] << 8) + d._privateData[offset + 1]);
            offset += 2;
            UInt32 oldProviderId = 0;
            d._pids.TryGetValue(pid, out oldProviderId);
            if (oldProviderId != 0)
            {
              Log.Log.Debug("CA Descriptor: overwriting provider ID {0} (0x{0:x}) for PID {1} (0x{1:x}) with value {2} (0x{2:x})",
                      oldProviderId, pid, providerId);
              d._pids[pid] = providerId;
            }
            else
            {
              d._pids.Add(pid, providerId);
            }
          }
        }
        return d;
      }

      offset = 0;
      bool foundProviderId = false;
      while (offset + 1 < d._privateData.Length)
      {
        byte tagInd = d._privateData[offset++];
        byte tagLen = d._privateData[offset++];
        if (offset + tagLen <= d._privateData.Length)
        {
          if (tagInd == 0x14 && tagLen >= 3)
          {
            providerId = (UInt32)((d._privateData[offset] << 16) + (d._privateData[offset + 1] << 8) +
                              d._privateData[offset + 2]);
            // Some providers send wrong information in the provider id (Boxer),
            // so reset the lower 4 bits for Via Access.
            if (d._caSystemId == 0x500)
            {
              providerId = providerId & 0xfffff0;
            }
            foundProviderId = true;
            d._pids.Add(d._caPid, providerId);
            break;
          }
        }
        offset += tagLen;
      }
      if (!foundProviderId)
      {
        d._pids.Add(d._caPid, 0);
      }

      return d;
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public override void Dump()
    {
      Log.Log.Debug("CA Descriptor: dump...");
      Log.Log.Debug("  tag          = {0}", _tag);
      Log.Log.Debug("  length       = {0}", _length);
      Log.Log.Debug("  CA system ID = {0} (0x{0:x})", _caSystemId);
      Log.Log.Debug("  CA PID       = {0} (0x{0:x})", _caPid);
      foreach (UInt16 pid in _pids.Keys)
      {
        Log.Log.Debug("  PID = {0} (0x{0:x}), provider = {1} (0x{1:x})", pid, _pids[pid]);
      }
      DVB_MMI.DumpBinary(_data, 0, _data.Length);
    }
  }

  /// <summary>
  /// This class parses DVB EN 50221 compliant MMI APDU objects, performing CAM menu callbacks appropriately.
  /// It is compatible with any conditional access interface that provides access to "raw" DVB compliant APDU
  /// objects.
  /// </summary>
  public class DvbMmiHandler
  {
    #region MMI/APDU interpretation

    /// <summary>
    /// Handle raw MMI data which contains one or more APDU objects, performing CAM menu callbacks appropriately.
    /// </summary>
    /// <param name="mmi">The MMI data.</param>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    public static void HandleMmiData(byte[] mmi, ref ICiMenuCallbacks ciMenuHandler)
    {
      Log.Log.Debug("DvbMmiHandler: handle MMI data");
      if (mmi == null || mmi.Length < 4)
      {
        Log.Log.Debug("DvbMmiHandler: data not supplied or too short");
      }

      //DVB_MMI.DumpBinary(mmi, 0, mmi.Length);

      // The first 3 bytes contains an MMI tag to tell us which APDUs we should expect to encounter.
      MmiTag tag = DvbMmiHandler.ReadMmiTag(mmi, 0);
      int countLengthBytes;
      int apduLength = ReadLength(mmi, 3, out countLengthBytes);
      Log.Log.Debug("DvbMmiHandler: data length = {0}, first APDU tag = {1}, length = {2}", mmi.Length, tag, apduLength);
      int offset = 3 + countLengthBytes;
      if (apduLength < 0 || offset + apduLength != mmi.Length)
      {
        Log.Log.Debug("DvbMmiHandler: APDU length is invalid");
        DVB_MMI.DumpBinary(mmi, 0, mmi.Length);
        return;
      }

      if (tag == MmiTag.CloseMmi)
      {
        // The CAM is requesting that we close the menu.
        HandleClose(mmi, offset, apduLength, ref ciMenuHandler);
      }
      else if (tag == MmiTag.Enquiry)
      {
        // The CAM wants input from the user.
        HandleEnquiry(mmi, offset, apduLength, ref ciMenuHandler);
      }
      else if (tag == MmiTag.ListLast || tag == MmiTag.MenuLast ||
          tag == MmiTag.MenuMore || tag == MmiTag.ListMore)
      {
        // The CAM is providing a menu or list to present to the user.
        HandleMenu(mmi, offset, apduLength, ref ciMenuHandler);
      }
      else
      {
        Log.Log.Debug("DvbMmiHandler: unexpected MMI tag {0}", tag);
        DVB_MMI.DumpBinary(mmi, 0, mmi.Length);
      }
    }

    private static void HandleClose(byte[] apdu, int offset, int apduLength, ref ICiMenuCallbacks ciMenuHandler)
    {
      Log.Log.Debug("DvbMmiHandler: handle close");

      if (offset >= apdu.Length)
      {
        Log.Log.Debug("DvbMmiHandler: invalid APDU");
        return;
      }

      MmiCloseType command = (MmiCloseType)apdu[offset++];
      int delay = 0;
      if (command == MmiCloseType.Delayed)
      {
        if (offset >= apdu.Length)
        {
          Log.Log.Debug("DvbMmiHandler: invalid APDU (delayed close)");
          return;
        }
        delay = apdu[offset];
      }

      Log.Log.Debug("  command = {0}", command);
      Log.Log.Debug("  delay   = {0} s", delay);

      if (ciMenuHandler != null)
      {
        try
        {
          ciMenuHandler.OnCiCloseDisplay(delay);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("DvbMmiHandler: close menu callback exception\r\n{0}", ex.ToString());
        }
      }
      else
      {
        Log.Log.Debug("DvbMmiHandler: menu callbacks are not set");
      }
    }

    private static void HandleEnquiry(byte[] apdu, int offset, int apduLength, ref ICiMenuCallbacks ciMenuHandler)
    {
      Log.Log.Debug("DvbMmiHandler: handle enquiry");

      if (offset + 4 >= apdu.Length)
      {
        Log.Log.Debug("DvbMmiHandler: invalid APDU");
        return;
      }

      bool passwordMode = (apdu[offset++] & 0x01) != 0;
      byte expectedAnswerLength = apdu[offset++];
      // Note: there are 2 other bytes before text starts.
      String prompt = System.Text.Encoding.ASCII.GetString(apdu, offset + 2, apdu.Length - offset - 2);

      Log.Log.Debug("  text   = {0}", prompt);
      Log.Log.Debug("  length = {0}", expectedAnswerLength);
      Log.Log.Debug("  blind  = {0}", passwordMode);

      if (ciMenuHandler != null)
      {
        try
        {
          ciMenuHandler.OnCiRequest(passwordMode, expectedAnswerLength, prompt);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("DvbMmiHandler: request callback exception\r\n{0}", ex.ToString());
        }
      }
      else
      {
        Log.Log.Debug("DvbMmiHandler: menu callbacks are not set");
      }
    }

    private static void HandleMenu(byte[] apdu, int offset, int apduLength, ref ICiMenuCallbacks ciMenuHandler)
    {
      Log.Log.Debug("DvbMmiHandler: handle menu");

      byte entryCount = apdu[offset++];
      List<String> entries = new List<String>();

      // Read the menu entries into the entries list.
      while (entryCount > 0 && offset < apdu.Length)
      {
        if (apdu[offset] != 0x9f)
        {
          Log.Log.Debug("DvbMmiHandler: unexpected APDU format, expected MMI tag at offset {0}", offset);
          DVB_MMI.DumpBinary(apdu, 0, apdu.Length);
          return;
        }
        int bytesRead;
        String entry = ReadText(apdu, offset, out bytesRead);
        if (entry == null)
        {
          Log.Log.Debug("DvbMmiHandler: unexpected APDU format, null entry at offset {0}", offset);
          DVB_MMI.DumpBinary(apdu, 0, apdu.Length);
          return;
        }
        entries.Add(entry);
        offset += bytesRead;
      }
      if (entries.Count < 3)
      {
        Log.Log.Debug("DvbMmiHandler: unexpected MMI format, less than 3 entries");
        DVB_MMI.DumpBinary(apdu, 0, apdu.Length);
        return;
      }

      if (ciMenuHandler == null)
      {
        Log.Log.Debug("DvbMmiHandler: menu callbacks are not set");
      }

      entryCount = (byte)(entries.Count - 3);
      Log.Log.Debug("  title     = {0}", entries[0]);
      Log.Log.Debug("  sub-title = {0}", entries[1]);
      Log.Log.Debug("  footer    = {0}", entries[2]);
      Log.Log.Debug("  # entries = {0}", entryCount);
      if (ciMenuHandler != null)
      {
        try
        {
          ciMenuHandler.OnCiMenu(entries[0], entries[1], entries[2], entryCount);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("DvbMmiHandler: menu callback exception\r\n{0}", ex.ToString());
        }
      }

      for (byte i = 0; i < entryCount; i++)
      {
        if (ciMenuHandler != null)
        {
          Log.Log.Debug("  entry {0,-2}  = {1}", i + 1, entries[i + 3]);
          try
          {
            ciMenuHandler.OnCiMenuChoice(i, entries[i + 3]);
          }
          catch (Exception ex)
          {
            Log.Log.Debug("DvbMmiHandler: menu entry callback exception\r\n{0}", ex.ToString());
          }
        }
      }
    }

    #endregion

    #region helpers

    /// <summary>
    /// Interpret an MMI tag.
    /// </summary>
    /// <param name="sourceData">An MMI data array containing the tag.</param>
    /// <param name="offset">The offset of the tag in sourceData.</param>
    /// <returns>the tag</returns>
    public static MmiTag ReadMmiTag(byte[] sourceData, int offset)
    {
      if (offset + 2 >= sourceData.Length)
      {
        return MmiTag.Unknown;
      }
      return (MmiTag)
        ((sourceData[offset] << 16) | (sourceData[offset + 1] << 8) | (sourceData[offset + 2]));
    }

    /// <summary>
    /// Write an MMI tag.
    /// </summary>
    /// <param name="tag">The tag to write.</param>
    /// <param name="outputData">The MMI data array to write the tag into.</param>
    /// <param name="offset">The offset of the tag in outputData.</param>
    public static void WriteMmiTag(MmiTag tag, ref byte[] outputData, int offset)
    {
      if (outputData == null || offset + 2 >= outputData.Length)
      {
        Log.Log.Debug("DvbMmiHandler: failed to write tag");
        return;
      }
      outputData[offset++] = (byte)(((int)tag >> 16) & 0xff);
      outputData[offset++] = (byte)(((int)tag >> 8) & 0xff);
      outputData[offset++] = (byte)((int)tag & 0xff);
    }

    /// <summary>
    /// Interpret a DVB MMI APDU length field, which is encoded using ASN.1 length encoding rules.
    /// </summary>
    /// <remarks>
    /// The value is encoded in a variable number of bytes as follows:
    /// - if the most significant bit in the first byte is set, it means that the first byte contains the number
    /// of byte(s) in which the length is encoded - the following byte(s) contain the length value
    /// - if the most significant bit in the first byte is *not* set, the first byte is the length value
    /// </remarks>
    /// <param name="sourceData">An MMI data array containing the length field.</param>
    /// <param name="offset">The offset of the length field in sourceData.</param>
    /// <param name="bytesRead">The number of bytes in the length field.</param>
    /// <returns>the value encoded in the length field, otherwise <c>-1</c> if the length is not valid</returns>
    public static int ReadLength(byte[] sourceData, int offset, out int bytesRead)
    {
      bytesRead = -1;

      if (sourceData == null || offset >= sourceData.Length)
      {
        Log.Log.Debug("DvbMmiHandler: offset is out of range");
        return -1;
      }

      byte byte1 = sourceData[offset++];

      // When the MSB of the first byte is not set, the first byte contains the length value.
      if ((byte1 & 0x80) == 0)
      {
        bytesRead = 1;
        return byte1;
      }

      // Multi-byte length field.
      bytesRead = byte1 & 0x7f;
      if (bytesRead > 4)
      {
        Log.Log.Debug("DvbMmiHandler: length encoded in {0} bytes, can't be interpretted", bytesRead);
        return -1;
      }
      if (offset + bytesRead >= sourceData.Length)
      {
        Log.Log.Debug("DvbMmiHandler: number of length bytes is invalid", bytesRead);
        return -1;
      }

      Int32 value = sourceData[offset++];
      for (byte i = 1; i < bytesRead; i++)
      {
        value = (value << 8) + sourceData[offset++];
      }
      bytesRead++;    // (for the first byte read into byte1)
      return value;
    }

    /// <summary>
    /// Write a DVB MMI APDU length field using ASN.1 length encoding rules.
    /// </summary>
    /// <remarks>
    /// The value is encoded in a variable number of bytes as follows:
    /// - if the most significant bit in the first byte is set, it means that the first byte contains the number
    /// of byte(s) in which the length is encoded - the following byte(s) contain the length value
    /// - if the most significant bit in the first byte is *not* set, the first byte is the length value
    /// </remarks>
    /// <param name="length">The length to write.</param>
    /// <param name="outputData">The MMI data array to write the length field into.</param>
    /// <param name="offset">The offset of the field in outputData.</param>
    /// <param name="bytesWritten">The number of bytes used to encode the length field.</param>
    public static void WriteLength(int length, ref byte[] outputData, int offset, out int bytesWritten)
    {
      bytesWritten = 1;

      // Figure out how many bytes we're going to need to encode the length.
      if (length > 127)
      {
        int tempLength = length;
        while (tempLength > 255)
        {
          tempLength = tempLength >> 8;
          bytesWritten++;
        }
      }

      if (outputData == null || offset + bytesWritten >= outputData.Length)
      {
        Log.Log.Debug("DvbMmiHandler: failed to write length");
        return;
      }

      // One byte length.
      if (length < 128)
      {
        outputData[offset] = (byte)length;
        return;
      }

      // Multi-byte length.
      outputData[offset] = (byte)(bytesWritten-- | 0x80);
      while (bytesWritten > 0)
      {
        outputData[offset + bytesWritten--] = (byte)(length & 0xff);
        length = length >> 8;
      }
    }

    /// <summary>
    /// Intepret an MMI text APDU.
    /// </summary>
    /// <param name="sourceData">An MMI data array containing the text APDU.</param>
    /// <param name="offset">The offset of the APDU in sourceData.</param>
    /// <param name="bytesRead">The number of bytes in the APDU field.</param>
    /// <returns>the string encoded in the text APDU, otherwise <c>null</c></returns>
    public static String ReadText(byte[] sourceData, int offset, out int bytesRead)
    {
      bytesRead = -1;

      MmiTag tag = ReadMmiTag(sourceData, offset);
      if (tag != MmiTag.TextMore && tag != MmiTag.TextLast)
      {
        Log.Log.Debug("DvbMmiHandler: invalid text tag {0}", tag);
        return null;
      }

      int lengthByteCount;
      int length = ReadLength(sourceData, offset + 3, out lengthByteCount);
      if (length == -1)
      {
        return null;
      }

      bytesRead = 3 + lengthByteCount;
      if (length > 0)
      {
        bytesRead += length;
        return System.Text.Encoding.ASCII.GetString(sourceData, offset + 3 + lengthByteCount, length);
      }
      else
      {
        return String.Empty;
      }
    }

    #endregion

    #region MMI/APDU encoding

    /// <summary>
    /// Create a "close_mmi" APDU, used to close an MMI dialog.
    /// </summary>
    /// <param name="delay">The delay before the host should close the MMI dialog.</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateMmiClose(byte delay)
    {
      byte[] apdu;
      if (delay > 0)
      {
        apdu = new byte[6];
        apdu[3] = 2;    // length
        apdu[4] = (byte)MmiCloseType.Delayed;
        apdu[5] = delay;
      }
      else
      {
        apdu = new byte[5];
        apdu[3] = 1;    // length
        apdu[4] = (byte)MmiCloseType.Immediate;
      }
      WriteMmiTag(MmiTag.CloseMmi, ref apdu, 0);
      return apdu;
    }

    /// <summary>
    /// Create a "menu_answ" APDU, used to select an entry in an MMI menu.
    /// </summary>
    /// <param name="choice">The elected index (0 means back)</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateMmiMenuAnswer(byte choice)
    {
      byte[] apdu = new byte[5];
      WriteMmiTag(MmiTag.MenuAnswer, ref apdu, 0);
      apdu[3] = 1;    // length
      apdu[4] = choice;
      return apdu;
    }

    /// <summary>
    /// Create an enquiry "answ" APDU, used to respond to an enquiry from the host.
    /// </summary>
    /// <param name="responseType">The response type.</param>
    /// <param name="answer">The answer.</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateMmiEnquiryAnswer(MmiResponseType responseType, String answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      char[] answerChars = answer.ToCharArray();

      // Encode the length into a temporary array so we know how many bytes are required for the APDU.
      byte[] length = new byte[5];  // max possible bytes for length field
      int lengthByteCount;
      WriteLength(answerChars.Length + 1, ref length, 0, out lengthByteCount);  // + 1 for response type

      // Encode the APDU.
      byte[] apdu = new byte[answerChars.Length + lengthByteCount + 4]; // + 4 = 3 for MMI tag, 1 for response type
      WriteMmiTag(MmiTag.Answer, ref apdu, 0);
      Buffer.BlockCopy(length, 0, apdu, 3, lengthByteCount);
      apdu[3 + lengthByteCount] = (byte)responseType;
      Buffer.BlockCopy(answerChars, 0, apdu, 4 + lengthByteCount, answerChars.Length);
      return apdu;
    }

    /// <summary>
    /// Create a "ca_pmt" APDU, used to query or manage a host's capabilities and actions in relation to a
    /// particular service.
    /// </summary>
    /// <param name="caPmt">A CA PMT structure encoded according to EN 50221, describing the service and
    ///   associated elementary streams.</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateCaPmtRequest(byte[] caPmt)
    {
      if (caPmt == null || caPmt.Length == 0)
      {
        // This is bad!
        throw new ArgumentException("DvbMmiHandler: CA PMT passed to CreateCaPmtRequest() is not set!");
      }

      // Encode the length into a temporary array so we know how many bytes are required for the APDU.
      byte[] length = new byte[5];  // max possible bytes for length field
      int lengthByteCount;
      WriteLength(caPmt.Length, ref length, 0, out lengthByteCount);

      // Encode the APDU.
      byte[] apdu = new byte[caPmt.Length + lengthByteCount + 3]; // + 3 for MMI tag
      WriteMmiTag(MmiTag.ConditionalAccessPmt, ref apdu, 0);
      Buffer.BlockCopy(length, 0, apdu, 3, lengthByteCount);
      Buffer.BlockCopy(caPmt, 0, apdu, 3 + lengthByteCount, caPmt.Length);
      return apdu;
    }

    #endregion
  }

  /// <summary>
  /// Common class for handling DVB MMI objects
  /// </summary>
  public static class DVB_MMI
  {
    /// <summary>
    /// returns a safe "printable" character or _
    /// </summary>
    /// <param name="b">byte code</param>
    /// <returns>char</returns>
    private static char ToSafeAscii(byte b)
    {
      if (b >= 32 && b <= 126)
      {
        return (char)b;
      }
      return '_';
    }

    /// <summary>
    /// Output binary buffer to log for debugging
    /// </summary>
    /// <param name="sourceData">source byte[]</param>
    /// <param name="offset">starting offset</param>
    /// <param name="length">total length</param>
    public static void DumpBinary(byte[] sourceData, int offset, int length)
    {
      StringBuilder row = new StringBuilder();
      StringBuilder rowText = new StringBuilder();

      for (int position = offset; position < offset + length; position++)
      {
        if (position == offset || position % 0x10 == 0)
        {
          if (row.Length > 0)
          {
            Log.Log.WriteFile(String.Format("{0}|{1}", row.ToString().PadRight(55, ' '),
                                            rowText.ToString().PadRight(16, ' ')));
          }
          rowText.Length = 0;
          row.Length = 0;
          row.AppendFormat("{0:X4}|", position);
        }
        row.AppendFormat("{0:X2} ", sourceData[position]); // the hex code
        rowText.Append(ToSafeAscii(sourceData[position])); // the ascii char
      }
      if (row.Length > 0)
      {
        Log.Log.WriteFile(String.Format("{0}|{1}", row.ToString().PadRight(55, ' '),
                                        rowText.ToString().PadRight(16, ' ')));
      }
    }

    /// <summary>
    /// Output binary buffer to log for debugging (Wrapper for IntPtr)
    /// </summary>
    /// <param name="sourceData">source IntPtr</param>
    /// <param name="offset">starting offset</param>
    /// <param name="length">total length</param>
    public static void DumpBinary(IntPtr sourceData, int offset, int length)
    {
      byte[] tmpBuffer = new byte[length];
      Marshal.Copy(sourceData, tmpBuffer, offset, length);
      DumpBinary(tmpBuffer, offset, length);
    }
  }
}
