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
}