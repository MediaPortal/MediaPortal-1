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
  /// DVB MMI message tags.
  /// </summary>
  public enum MmiTag
  {
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
    CloseMmi = 0x9F8800,
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
    private List<Descriptor> _descriptors;
    private List<Descriptor> _caDescriptors;
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
    public List<Descriptor> Descriptors
    {
      get
      {
        return _descriptors;
      }
    }

    /// <summary>
    /// The conditional access descriptors for the service described by the conditional access table.
    /// </summary>
    public List<Descriptor> CaDescriptors
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
      cat._descriptors = new List<Descriptor>();
      cat._caDescriptors = new List<Descriptor>();
      while (offset + 1 < endDescriptors)
      {
        Descriptor d = Descriptor.Decode(data, offset);
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
    /// Retrieve the original conditional access section data that was decoded to create this Cat instance.
    /// </summary>
    /// <returns>the raw conditional access section data</returns>
    public byte[] GetRawCat()
    {
      // Make a copy of our raw CAT for the caller. We copy so that subsequent changes made by the caller
      // have no effect on our reference/copy.
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
      foreach (Descriptor d in _descriptors)
      {
        d.Dump();
      }
      foreach (Descriptor cad in _caDescriptors)
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
    private List<Descriptor> _programDescriptors;
    private List<Descriptor> _programCaDescriptors;
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
    public List<Descriptor> ProgramDescriptors
    {
      get
      {
        return _programDescriptors;
      }
    }

    /// <summary>
    /// The conditional access descriptors for the service described by the program map.
    /// </summary>
    public List<Descriptor> ProgramCaDescriptors
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
      pmt._programDescriptors = new List<Descriptor>();
      pmt._programCaDescriptors = new List<Descriptor>();
      while (offset + 1 < endProgramDescriptors)
      {
        Descriptor d = Descriptor.Decode(data, offset);
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
          Log.Log.Debug("PMT: elementary stream info length for PID {0} (0x{1:x}) is invalid", es.Pid, es.Pid);
          return null;
        }
        es.Descriptors = new List<Descriptor>();
        es.CaDescriptors = new List<Descriptor>();
        while (offset + 1 < endEsDescriptors)
        {
          Descriptor d = Descriptor.Decode(data, offset);
          if (d == null)
          {
            Log.Log.Debug("PMT: elementary stream descriptor {0} for PID {1} (0x{2:x}) is invalid", es.Descriptors.Count + es.CaDescriptors.Count + 1, es.Pid, es.Pid);
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
    /// Retrieve the original program map section data that was decoded to create this Pmt instance.
    /// </summary>
    /// <returns>the raw program map section data</returns>
    public byte[] GetRawPmt()
    {
      // Make a copy of our raw PMT for the caller. We copy so that subsequent changes made by the caller
      // have no effect on our reference/copy.
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
      foreach (Descriptor d in _programCaDescriptors)
      {
        if (programInfoLength == 0)
        {
          tempCaPmt[offset++] = (byte)command;
          programInfoLength++;
        }
        byte[] descriptorData = d.GetRawData();
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
          foreach (Descriptor d in es.CaDescriptors)
          {
            if (esInfoLength == 0)
            {
              tempCaPmt[offset++] = (byte)command;
              esInfoLength++;
            }
            byte[] descriptorData = d.GetRawData();
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

      DVB_MMI.DumpBinary(caPmt, 0, caPmt.Length);

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
      Log.Log.Debug("  program number           = {0} (0x{1:x})", _programNumber, _programNumber);
      Log.Log.Debug("  version                  = {0}", _version);
      Log.Log.Debug("  current next indicator   = {0}", _currentNextIndicator);
      Log.Log.Debug("  section number           = {0}", _sectionNumber);
      Log.Log.Debug("  last section number      = {0}", _lastSectionNumber);
      Log.Log.Debug("  PCR PID                  = {0} (0x{1:x})", _pcrPid, _pcrPid);
      Log.Log.Debug("  program info length      = {0}", _programInfoLength);
      Log.Log.Debug("  CRC                      = 0x{0:x}{1:x}{2:x}{3:x}", _crc[0], _crc[1], _crc[2], _crc[3]);
      Log.Log.Debug("  {0} descriptor(s)...", _programDescriptors.Count + _programCaDescriptors.Count);
      foreach (Descriptor d in _programDescriptors)
      {
        d.Dump();
      }
      foreach (Descriptor cad in _programCaDescriptors)
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
    private StreamType _streamType;
    private UInt16 _pid;
    private UInt16 _esInfoLength;
    private List<Descriptor> _descriptors;
    private List<Descriptor> _caDescriptors;

    private DescriptorTag _primaryDescriptorTag;

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
    public List<Descriptor> Descriptors
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
    public List<Descriptor> CaDescriptors
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
      Log.Log.Debug("  PID         = {0} (0x{1:x})", _pid, _pid);
      Log.Log.Debug("  length      = {0}", _esInfoLength);
      Log.Log.Debug("  main tag    = {0}", _primaryDescriptorTag);
      Log.Log.Debug("  {0} descriptor(s)...", _descriptors.Count + _caDescriptors.Count);
      foreach (Descriptor d in _descriptors)
      {
        d.Dump();
      }
      foreach (Descriptor cad in _caDescriptors)
      {
        cad.Dump();
      }
    }
  }

  /// <summary>
  /// A class that models the descriptor structure defined in ISO/IEC 13818-1.
  /// </summary>
  public class Descriptor
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
    /// Decode().
    /// </summary>
    protected Descriptor()
    {
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
    /// <returns>a fully populated Descriptor instance</returns>
    public static Descriptor Decode(byte[] data, int offset)
    {
      if (offset + 1 >= data.Length)
      {
        return null;
      }
      Descriptor d = new Descriptor();
      d._tag = (DescriptorTag)data[offset];
      d._length = data[offset + 1];
      if (offset + 2 + d._length >= data.Length)
      {
        return null;
      }
      d._data = new byte[d._length];
      Buffer.BlockCopy(data, offset + 2, d._data, 0, d._length);

      // Make a copy of the entire descriptor so that changes made by the caller on the original array have
      // no effect on our reference/copy.
      d._rawData = new byte[2 + d._length];
      Buffer.BlockCopy(data, offset, d._rawData, 0, 2 + d._length);

      return d;
    }

    /// <summary>
    /// Retrieve the original descriptor data that was decoded to create this Descriptor instance.
    /// </summary>
    /// <returns>the raw program map section data</returns>
    public byte[] GetRawData()
    {
      // Make a copy of our raw data for the caller. We copy so that subsequent changes made by the caller
      // have no effect on our reference/copy.
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
  public class ConditionalAccessDescriptor : Descriptor
  {
    #region variables

    private UInt16 _caSystemId;
    private UInt16 _caPid;
    private byte[] _privateData;
    private Dictionary<UInt16, UInt32> _pids;

    #endregion

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
    /// Decode raw descriptor data.
    /// </summary>
    /// <param name="data">The raw descriptor data.</param>
    /// <param name="offset">The offset in the data array at which the descriptor starts.</param>
    /// <returns>a fully populated ConditionalAccessDescriptor instance</returns>
    public static new ConditionalAccessDescriptor Decode(byte[] data, int offset)
    {
      ConditionalAccessDescriptor d = (ConditionalAccessDescriptor)Descriptor.Decode(data, offset);
      if (d == null || d._length < 4)
      {
        return null;
      }
      offset = 2;
      d._caSystemId = (UInt16)((data[offset] << 8) + data[offset + 1]);
      offset += 2;
      d._caPid = (UInt16)(((data[offset] & 0x1f) << 8) + data[offset + 1]);
      offset += 2;

      d._privateData = new byte[d._length - 4];
      Buffer.BlockCopy(data, offset, d._privateData, 0, d._length - 4);

      // Canal Plus...
      UInt32 providerId;
      d._pids = new Dictionary<UInt16, UInt32>();
      if (d._caSystemId == 0x100)
      {
        if (d._privateData.Length >= 3 && d._privateData[2] == 0xff)
        {
          //  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
          // 09 11 01 00 E6 43 00 6A FF 00 00 00 00 00 00 02 14 21 8C
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
              Log.Log.Debug("CA Descriptor: overwriting provider ID {0} (0x{1:x}) for PID {2} (0x{3:x}) with value {4} (0x{5:x})",
                      oldProviderId, oldProviderId, pid, pid, providerId, providerId);
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
      Log.Log.Debug("  CA system ID = {0} (0x{1:x})", _caSystemId, _caSystemId);
      Log.Log.Debug("  CA PID       = {0} (0x{1:x})", _caPid, _caPid);
      foreach (UInt16 pid in _pids.Keys)
      {
        Log.Log.Debug("  PID = {0} (0x{1:x}), provider = {2} (0x{3:x})", pid, pid, _pids[pid], _pids[pid]);
      }
      DVB_MMI.DumpBinary(_data, 0, _data.Length);
    }
  }

  /// <summary>
  /// This class parses DVB MMI APDU objects, performing callbacks appropriately. It is compatible with any
  /// conditional access interface that provides access to DVB compliant APDU objects.
  /// </summary>
  public class DvbMmiHandler
  {
    #region variables

    private ICiMenuCallbacks _ciMenuCallbacks;
    private String _deviceName;

    #endregion

    /// <summary>
    /// Constructor for an APDU based MMI parser.
    /// </summary>
    /// <param name="deviceName">The name of the device that will own the handler instance.</param>
    public DvbMmiHandler(String deviceName)
    {
      _deviceName = deviceName;
    }

    /// <summary>
    /// Set the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ref ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Handles APDU (MMI) objects and perform callbacks
    /// </summary>
    /// <param name="MMI">MMI byte[]</param>
    public void HandleMMI(byte[] MMI)
    {
      // parse starting 3 bytes == tag
      MmiTag uMMITag = DVB_MMI.ToMMITag(MMI, 0);

      // dumping binary APDU
      DVB_MMI.DumpBinary(MMI, 0, MMI.Length);


      // calculate length and offset
      int countLengthBytes;
      int mmiLength = DVB_MMI.GetLength(MMI, 3 /* bytes for mmi_tag */, out countLengthBytes);
      int mmiOffset = 3 + countLengthBytes; // 3 bytes mmi tag + 1 byte length field ?

      Log.Log.Debug("{0}: MMITag:{1}, MMIObjectLength: {2} ({2:X2}), mmiOffset: {3}", _deviceName, uMMITag, mmiLength,
                    mmiOffset);

      int offset = 0; // starting with 0; reading whole struct from start
      if (uMMITag == MmiTag.CloseMmi)
      {
        // Close menu
        byte nDelay = 0;
        byte CloseCmd = MMI[mmiOffset + 0];
        if (CloseCmd != 0)
        {
          nDelay = MMI[mmiOffset + 1];
        }
        if (_ciMenuCallbacks != null)
        {
          Log.Log.Debug("{0}: OnCiClose()", _deviceName);
          _ciMenuCallbacks.OnCiCloseDisplay(nDelay);
        }
        else
        {
          Log.Log.Debug("{0}: OnCiCloseDisplay: cannot do callback!", _deviceName);
        }
      }
      if (uMMITag == MmiTag.Enquiry)
      {
        // request input
        bool bPasswordMode = false;
        byte answer_text_length = MMI[mmiOffset + 1];
        string strText = "";

        if ((MMI[mmiOffset + 0] & 0x01) != 0)
        {
          bPasswordMode = true;
        }

        // mmioffset +4 because there a 2 other bytes before text starts
        // length is these 2 bytes shorter 
        strText = DVB_MMI.BytesToString(MMI, mmiOffset + 4, mmiLength - mmiOffset - 2);
        if (_ciMenuCallbacks != null)
        {
          Log.Log.Debug("{0}: OnCiRequest: bPasswordMode:{1}, answer_text_length:{2}, strText:{3}", _deviceName,
                        bPasswordMode, answer_text_length, strText);
          _ciMenuCallbacks.OnCiRequest(bPasswordMode, answer_text_length, strText);
        }
        else
        {
          Log.Log.Debug("{0}: OnCiRequest: cannot do callback!", _deviceName);
        }
      }
      if (uMMITag == MmiTag.ListLast || uMMITag == MmiTag.MenuLast ||
          uMMITag == MmiTag.MenuMore || uMMITag == MmiTag.ListMore)
      {
        // step forward; begin with offset+1; stop when 0x9F reached
        // should be modified to offset + mmioffset+1 ?
        offset++;
        while (MMI[offset] != (byte)0x9F)
        {
          //Log.Log.Debug("Skip to offset {0} value {1:X2}", offset, MMI[offset]);
          offset++;
        }
        uMMITag = DVB_MMI.ToMMITag(MMI, offset); // get next MMI tag
        Log.Log.Debug("{0}: _mmiHandler Parse: Got MENU_LAST, skipped to next block on index: {1}; new Tag {2}", _deviceName,
                      offset, uMMITag);

        int nChoices = 0;
        List<string> Choices = new List<string>();
        // Always three line with menu info (DVB Standard)
        // Title Text
        offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);
        // Subtitle Text
        offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);
        // Bottom Text
        offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);

        // first step through the choices, to get info and count them
        int max = 20;
        while (max-- > 0)
        {
          // if the offset gets to mmi object length then end here
          if (offset >= mmiLength - 1)
            break;

          offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);
          nChoices++;
        }
        // when title and choices are ready now, send to client
        for (int c = 0; c < Choices.Count; c++)
        {
          Log.Log.Debug("{0}: {1} : {2}", _deviceName, c, Choices[c]);
        }
        if (_ciMenuCallbacks != null)
        {
          _ciMenuCallbacks.OnCiMenu(Choices[0], Choices[1], Choices[2], nChoices);
          for (int c = 3; c < Choices.Count; c++)
          {
            _ciMenuCallbacks.OnCiMenuChoice(c - 3, Choices[c]);
          }
        }
        else
        {
          Log.Log.Debug("{0}: OnCiMenu: cannot do callback!", _deviceName);
        }
      }
    }
  }

  /// <summary>
  /// Common class for handling DVB MMI objects
  /// </summary>
  public static class DVB_MMI
  {
    /// <summary>
    /// interpretes parts of an byte[] as status int
    /// </summary>
    /// <param name="sourceData"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static MmiTag ToMMITag(byte[] sourceData, int offset)
    {
      return
        (MmiTag)
        (((Int32)sourceData[offset] << 16) | ((Int32)sourceData[offset + 1] << 8) | ((Int32)sourceData[offset + 2]));
    }

    /// <summary>
    /// interpretes length() info which can be of different size
    /// </summary>
    /// <param name="sourceData">source byte array</param>
    /// <param name="offset">index to start</param>
    /// <param name="bytesRead">returns the number of bytes interpreted</param>
    /// <returns>length of following object</returns>
    public static int GetLength(byte[] sourceData, int offset, out int bytesRead)
    {
      byte bLen = sourceData[offset];
      // if highest bit set, it means there are > 127 bytes
      if ((bLen & 0x80) == 0)
      {
        bytesRead = 1;
        return bLen;
      }
      else
      {
        bLen &= 0x7f;
        // clear 8th bit; remaining 7 bit tell the number of following bytes to interpret (most probably 2)
        bytesRead = 1 + bLen;
        Int32 shiftBy;
        Int32 iLen = 0;
        for (Int32 p = 0; p < bLen; p++)
        {
          shiftBy = (Int32)(bLen - p - 1) * 8; // number of bits to shift up, i.e. 2 bytes -> 1st byte <<8, 2nd byte <<0
          iLen = iLen | (sourceData[offset + 1 + p] << shiftBy);
          // shift byte to right position, concat by "or" operation
        }
        return iLen;
      }
    }

    /// <summary>
    /// Converts bytes to String
    /// </summary>
    /// <param name="sourceData">source byte[]</param>
    /// <param name="offset">starting offset</param>
    /// <param name="length">length</param>
    /// <returns>String</returns>
    public static String BytesToString(byte[] sourceData, int offset, int length)
    {
      StringBuilder StringEntry = new StringBuilder();
      for (int l = offset; l < offset + length; l++)
      {
        StringEntry.Append((char)sourceData[l]);
      }
      return StringEntry.ToString();
    }

    /// <summary>
    /// intepretes string for ci menu entries
    /// </summary>
    /// <param name="sourceData">source byte array</param>
    /// <param name="offset">index to start</param>
    /// <param name="menuEntries">reference to target string list</param>
    /// <returns>offset for further readings</returns>
    public static int GetCIText(byte[] sourceData, int offset, ref List<String> menuEntries)
    {
      byte Length; // We assume that text Length is smaller 127
      MmiTag Tag;

      Tag = ToMMITag(sourceData, offset);
      if ((Tag != MmiTag.TextMore) && (Tag != MmiTag.TextLast))
      {
        return -1;
      }

      Length = sourceData[offset + 3];

      // Check if our assumption is TRUE
      if (Length > 127)
        return -1; // Length is > 127

      if (Length > 0)
      {
        // Create string from byte array 
        String menuEntry = BytesToString(sourceData, offset + 4, Length);
        //Log.Log.Debug("FireDTV: MMI Parse GetCIText: {0}", menuEntry.ToString());
        menuEntries.Add(menuEntry.ToString());
      }
      else
      {
        // empty String ? add to keep correct index positions
        menuEntries.Add("");
      }
      return (Length + 4);
    }

    /// <summary>
    /// Creates a "CloseMMI" data set
    /// </summary>
    /// <returns>a CloseMmi APDU</returns>
    public static byte[] CreateMMIClose()
    {
      // MMI tag
      byte[] uData = new byte[5];
      uData[0] = 0x9F;
      uData[1] = 0x88;
      uData[2] = 0x00;
      uData[3] = 0x01; // length field
      uData[4] = 0x00; // close_mmi_cmd_id (immediately)
      return uData;
    }

    /// <summary>
    /// Creates a "SelectMenuChoice" data set
    /// </summary>
    /// <param name="choice">selected index (0 means back)</param>
    /// <returns>a MenuAnswer APDU</returns>
    public static byte[] CreateMMISelect(byte choice)
    {
      // MMI tag
      byte[] uData = new byte[5];
      uData[0] = 0x9F;
      uData[1] = 0x88;
      uData[2] = 0x0B; // send choice
      uData[3] = 0x01; // length field
      uData[4] = choice; // choice
      return uData;
    }

    /// <summary>
    /// Creates an CI Menu Answer package
    /// </summary>
    /// <param name="responseType">The DVB MMI response type.</param>
    /// <param name="answer">answer string</param>
    /// <returns>an Answer APDU</returns>
    public static byte[] CreateMMIAnswer(MmiResponseType responseType, String answer)
    {
      char[] answerChars = answer.ToCharArray();

      // Calculate the number of bytes that are needed to encode the
      // length value.
      int lengthToEncode = answerChars.Length + 1;
      int byteCount = 1;
      if (lengthToEncode > 127)
      {
        int tempLength = lengthToEncode;
        while (tempLength > 255)
        {
          tempLength = tempLength / 256;
          byteCount++;
        }
      }

      // MMI tag
      byte[] uData = new byte[4 + byteCount + answerChars.Length];
      uData[0] = 0x9F;
      uData[1] = 0x88;
      uData[2] = 0x08; // send enquiry answer

      uData[3 + byteCount] = (byte)responseType;

      // answer string
      int offset = 4 + byteCount;
      for (int p = 0; p < answerChars.Length; p++)
      {
        uData[offset + p] = (byte)answerChars[p];
      }

      // length field
      if (byteCount == 1)
      {
        uData[3] = (byte)lengthToEncode;
      }
      else
      {
        uData[3] = (byte)(byteCount | 0x80);
        while (lengthToEncode > 1)
        {
          uData[2 + byteCount] = (byte)(lengthToEncode % 256);
          lengthToEncode = lengthToEncode / 256;
          byteCount--;
        }
      }

      return uData;
    }

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
