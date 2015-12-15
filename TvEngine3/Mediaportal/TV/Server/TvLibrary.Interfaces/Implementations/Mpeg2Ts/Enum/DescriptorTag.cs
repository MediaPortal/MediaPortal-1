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
    AudioStream = 3,
    /// <summary>
    /// MPEG hierarchy descriptor
    /// </summary>
    Hierarchy = 4,
    /// <summary>
    /// MPEG registration descriptor
    /// </summary>
    Registration = 5,
    /// <summary>
    /// MPEG data stream alignment descriptor
    /// </summary>
    DataStreamAlignment = 6,
    /// <summary>
    /// MPEG target background grid descriptor
    /// </summary>
    TargetBackgroundGrid = 7,
    /// <summary>
    /// MPEG video window descriptor
    /// </summary>
    VideoWindow = 8,
    /// <summary>
    /// MPEG conditional access descriptor
    /// </summary>
    ConditionalAccess = 9,
    /// <summary>
    /// MPEG ISO 639 language descriptor
    /// </summary>
    Iso639Language = 10,
    /// <summary>
    /// MPEG system clock descriptor
    /// </summary>
    SystemClock = 11,
    /// <summary>
    /// MPEG multiplex buffer utilisation descriptor
    /// </summary>
    MultiplexBufferUtilisation = 12,
    /// <summary>
    /// MPEG copyright descriptor
    /// </summary>
    Copyright = 13,
    /// <summary>
    /// MPEG maximum bitrate descriptor
    /// </summary>
    MaximumBitRateDescriptor = 14,
    /// <summary>
    /// MPEG private data descriptor
    /// </summary>
    PrivateData = 15,
    /// <summary>
    /// MPEG smoothing buffer descriptor
    /// </summary>
    SmoothingBuffer = 16,
    /// <summary>
    /// MPEG STD descriptor
    /// </summary>
    Std = 17,
    /// <summary>
    /// MPEG IBP descriptor
    /// </summary>
    Ibp = 18,

    // 19 - 26 defined in ISO/IEC 13818-6

    /// <summary>
    /// MPEG 4 video descriptor
    /// </summary>
    Mpeg4Video = 27,
    /// <summary>
    /// MPEG 4 audio descriptor
    /// </summary>
    Mpeg4Audio = 28,
    /// <summary>
    /// MPEG IOD descriptor
    /// </summary>
    Iod = 29,
    /// <summary>
    /// MPEG SL descriptor
    /// </summary>
    Sl = 30,
    /// <summary>
    /// MPEG FMC descriptor
    /// </summary>
    Fmc = 31,
    /// <summary>
    /// MPEG external elementary stream ID descriptor
    /// </summary>
    ExternalEsId = 32,
    /// <summary>
    /// MPEG mux code descriptor
    /// </summary>
    MuxCode = 33,
    /// <summary>
    /// MPEG FMX buffer size descriptor
    /// </summary>
    FmxBufferSize = 34,
    /// <summary>
    /// MPEG multiplex buffer descriptor
    /// </summary>
    MultiplexBuffer = 35,
    /// <summary>
    /// MPEG content labeling descriptor
    /// </summary>
    ContentLabeling = 36,
    /// <summary>
    /// MPEG metadata pointer descriptor
    /// </summary>
    MetadataPointer = 37,
    /// <summary>
    /// MPEG metadata descriptor
    /// </summary>
    Metadata = 38,
    /// <summary>
    /// MPEG metadata STD descriptor
    /// </summary>
    MetadataStd = 39,
    /// <summary>
    /// MPEG AVC video descriptor
    /// </summary>
    AvcVideo = 40,
    /// <summary>
    /// MPEG IPMP descriptor
    /// </summary>
    Ipmp = 41,
    /// <summary>
    /// MPEG AVC timing and HRD descriptor
    /// </summary>
    AvcTimingAndHrd = 42,
    /// <summary>
    /// MPEG 2 AAC audio descriptor
    /// </summary>
    Mpeg2AacAudio = 43,
    /// <summary>
    /// MPEG FlexMux timing descriptor
    /// </summary>
    FlexMuxTiming = 44,
    /// <summary>
    /// MPEG 4 text descriptor
    /// </summary>
    Mpeg4Text = 45,
    /// <summary>
    /// MPEG 4 audio extension descriptor
    /// </summary>
    Mpeg4AudioExtension = 46,
    /// <summary>
    /// MPEG auxiliary video stream descriptor
    /// </summary>
    AuxiliaryVideoStream = 47,
    /// <summary>
    /// MPEG SVC extension descriptor
    /// </summary>
    SvcExtension = 48,
    /// <summary>
    /// MPEG MVC extension descriptor
    /// </summary>
    MvcExtension = 49,
    /// <summary>
    /// MPEG J2K video descriptor
    /// </summary>
    J2kVideo = 50,
    /// <summary>
    /// MPEG MVC operation point descriptor
    /// </summary>
    MvcOperationPoint = 51,
    /// <summary>
    /// MPEG 2 stereoscopic video format descriptor
    /// </summary>
    Mpeg2StereoscopicVideoFormat = 52,
    /// <summary>
    /// MPEG stereoscopic program info descriptor
    /// </summary>
    StereoscopicProgramInfo = 53,
    /// <summary>
    /// MPEG stereoscopic video info descriptor
    /// </summary>
    StereoscopicVideoInfo = 54,
    /// <summary>
    /// MPEG transport profile descriptor
    /// </summary>
    TransportProfile = 55,
    /// <summary>
    /// HEVC video descriptor
    /// </summary>
    HevcVideo = 56,

    // 57 - 62 ITU-T Rec. H.222.0 | ISO/IEC 13818-1 reserved

    /// <summary>
    /// MPEG extension descriptor
    /// </summary>
    MpegExtension = 63,

    #endregion

    #region DVB ETSI EN 300 468

    /// <summary>
    /// DVB network name descriptor
    /// </summary>
    NetworkName = 0x40,
    /// <summary>
    /// DVB service list descriptor
    /// </summary>
    ServiceList = 0x41,
    /// <summary>
    /// DVB stuffing descriptor
    /// </summary>
    Stuffing = 0x42,
    /// <summary>
    /// DVB satellite delivery system descriptor
    /// </summary>
    SatelliteDeliverySystem = 0x43,
    /// <summary>
    /// DVB cable delivery system descriptor
    /// </summary>
    CableDeliverySystem = 0x44,
    /// <summary>
    /// DVB vertical blanking interval data descriptor
    /// </summary>
    VbiData = 0x45,
    /// <summary>
    /// DVB vertical blanking interval teletext descriptor
    /// </summary>
    VbiTeletext = 0x46,
    /// <summary>
    /// DVB bouquet name descriptor
    /// </summary>
    BouquetName = 0x47,
    /// <summary>
    /// DVB service descriptor
    /// </summary>
    Service = 0x48,
    /// <summary>
    /// DVB country availability descriptor
    /// </summary>
    CountryAvailability = 0x49,
    /// <summary>
    /// DVB linkage descriptor
    /// </summary>
    Linkage = 0x4a,
    /// <summary>
    /// DVB near video on demand reference descriptor
    /// </summary>
    NvodReference = 0x4b,
    /// <summary>
    /// DVB time shifted service descriptor
    /// </summary>
    DvbTimeShiftedService = 0x4c,
    /// <summary>
    /// DVB short event descriptor
    /// </summary>
    ShortEvent = 0x4d,
    /// <summary>
    /// DVB extended event descriptor
    /// </summary>
    ExtendedEvent = 0x4e,
    /// <summary>
    /// DVB time shifted event descriptor
    /// </summary>
    TimeShiftedEvent = 0x4f,
    /// <summary>
    /// DVB component descriptor
    /// </summary>
    Component = 0x50,
    /// <summary>
    /// DVB mosaic descriptor
    /// </summary>
    Mosaic = 0x51,
    /// <summary>
    /// DVB stream identifier descriptor. Harmonised with SCTE (ANSI/SCTE 35).
    /// </summary>
    StreamIdentifier = 0x52,
    /// <summary>
    /// DVB conditional access descriptor
    /// </summary>
    ConditionalAccessIdentifier = 0x53,
    /// <summary>
    /// DVB content descriptor
    /// </summary>
    Content = 0x54,
    /// <summary>
    /// DVB parental rating descriptor
    /// </summary>
    ParentalRating = 0x55,
    /// <summary>
    /// DVB teletext descriptor
    /// </summary>
    Teletext = 0x56,
    /// <summary>
    /// DVB telephone descriptor
    /// </summary>
    Telephone = 0x57,
    /// <summary>
    /// DVB local time offset descriptor
    /// </summary>
    LocalTimeOffset = 0x58,
    /// <summary>
    /// DVB subtitling descriptor
    /// </summary>
    Subtitling = 0x59,
    /// <summary>
    /// DVB terrestrial delivery system descriptor
    /// </summary>
    TerrestrialDeliverySystem = 0x5a,
    /// <summary>
    /// DVB multilingual network name descriptor
    /// </summary>
    MultilingualNetworkName = 0x5b,
    /// <summary>
    /// DVB multilingual bouquet name descriptor
    /// </summary>
    MultilingualBouquetName = 0x5c,
    /// <summary>
    /// DVB multilingual service name descriptor
    /// </summary>
    MultilingualServiceName = 0x5d,
    /// <summary>
    /// DVB multilingual component descriptor
    /// </summary>
    MultilingualComponent = 0x5e,
    /// <summary>
    /// DVB private data specifier descriptor
    /// </summary>
    PrivateDataSpecifier = 0x5f,
    /// <summary>
    /// DVB service move descriptor
    /// </summary>
    ServiceMove = 0x60,
    /// <summary>
    /// DVB short smoothing buffer descriptor
    /// </summary>
    ShortSmoothingBuffer = 0x61,
    /// <summary>
    /// DVB frequency list descriptor
    /// </summary>
    FrequencyList = 0x62,
    /// <summary>
    /// DVB partial transport stream descriptor
    /// </summary>
    PartialTransportStream = 0x63,
    /// <summary>
    /// DVB data broadcast descriptor
    /// </summary>
    DataBroadcast = 0x64,
    /// <summary>
    /// DVB scrambling descriptor
    /// </summary>
    Scrambling = 0x65,
    /// <summary>
    /// DVB data broadcast ID descriptor
    /// </summary>
    DataBroadcastId = 0x66,
    /// <summary>
    /// DVB transport stream descriptor
    /// </summary>
    TransportStream = 0x67,
    /// <summary>
    /// DVB digital satellite news gathering descriptor
    /// </summary>
    Dsng = 0x68,
    /// <summary>
    /// DVB program delivery control descriptor
    /// </summary>
    Pdc = 0x69,
    /// <summary>
    /// DVB AC3 descriptor
    /// </summary>
    Ac3 = 0x6a,
    /// <summary>
    /// DVB anciliary data descriptor
    /// </summary>
    AnciliaryData = 0x6b,
    /// <summary>
    /// DVB cell list descriptor
    /// </summary>
    CellList = 0x6c,
    /// <summary>
    /// DVB cell frequency link descriptor
    /// </summary>
    CellFrequencyLink = 0x6d,
    /// <summary>
    /// DVB announcement support descriptor
    /// </summary>
    AnnouncementSupport = 0x6e,
    /// <summary>
    /// DVB application signalling descriptor
    /// </summary>
    ApplicationSignalling = 0x6f,
    /// <summary>
    /// DVB adaptation field data descriptor
    /// </summary>
    AdaptationFieldData = 0x70,
    /// <summary>
    /// DVB service identifier descriptor
    /// </summary>
    ServiceIdentfier = 0x71,
    /// <summary>
    /// DVB service availability descriptor
    /// </summary>
    ServiceAvailability = 0x72,
    /// <summary>
    /// DVB default authority descriptor
    /// </summary>
    DefaultAuthority = 0x73,
    /// <summary>
    /// DVB related content descriptor
    /// </summary>
    RelatedContent = 0x74,
    /// <summary>
    /// DVB TVA ID descriptor
    /// </summary>
    TvaId = 0x75,
    /// <summary>
    /// DVB content identifier descriptor
    /// </summary>
    ContentIdentifier = 0x76,
    /// <summary>
    /// DVB time slice FEC identifier descriptor
    /// </summary>
    TimeSliceFecIdentifier = 0x77,
    /// <summary>
    /// DVB entitlement control message repetition rate descriptor
    /// </summary>
    EcmRepetitionRate = 0x78,
    /// <summary>
    /// DVB S2 satellite delivery system descriptor
    /// </summary>
    S2SatelliteDeliverySystem = 0x79,
    /// <summary>
    /// DVB enhanced AC3 descriptor
    /// </summary>
    EnhancedAc3 = 0x7a,
    /// <summary>
    /// DVB DTS descriptor
    /// </summary>
    Dts = 0x7b,
    /// <summary>
    /// DVB AAC descriptor
    /// </summary>
    Aac = 0x7c,
    /// <summary>
    /// DVB XAIT location descriptor
    /// </summary>
    XaitLocation = 0x7d,
    /// <summary>
    /// DVB free-to-air content management descriptor
    /// </summary>
    FtaContentManagement = 0x7e,
    /// <summary>
    /// DVB extension descriptor
    /// </summary>
    DvbExtension = 0x7f,

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
    ParameterisedService = 0x8d,
    /// <summary>
    /// ATSC interactive services filtering criteria descriptor
    /// </summary>
    InteractiveServicesFilteringCriteria = 0x8e,
    /// <summary>
    /// ATSC interactive services near real time services summary descriptor
    /// </summary>
    InteractiveServicesNrtServicesSummary = 0x8f,
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