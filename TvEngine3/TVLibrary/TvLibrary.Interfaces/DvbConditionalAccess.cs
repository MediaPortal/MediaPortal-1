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
  /// DVB MMI application information application type.
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
  /// DVB MMI enquiry answer response type.
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
  /// DVB MMI message tag.
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
  /// Stream type.
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
  /// Descriptor type.
  /// </summary>
  public enum DescriptorType
  {
    #region MPEG ISO/IEC 13818-1

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

    #region DVB EN 300 468

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

  #endregion

  public class CaPmt
  {
    public static bool GetFromPmt(byte[] pmt, CaPmtListManagementAction listAction, CaPmtCommand command, out byte[] caPmt)
    {
      caPmt = null;
      if (pmt == null || pmt.Length < 12)
      {
        Log.Log.Debug("CA PMT: PMT not supplied or too short");
        return false;
      }

      byte[] tempCaPmt = new byte[4096];
      tempCaPmt[0] = (byte)listAction;
      tempCaPmt[1] = pmt[3];
      tempCaPmt[2] = pmt[4];
      tempCaPmt[3] = pmt[5];

      // program descriptors
      int caPmtOffset = 6;
      int caPmtProgramDescriptorsLength = 0;
      int pmtOffset = 12;
      int pmtProgramInfoEnd = ((pmt[10] & 0x0f) << 8) + pmt[11] + pmtOffset;
      if (pmtProgramInfoEnd > pmt.Length - 4)
      {
        Log.Log.Debug("CA PMT: program info length is invalid");
        return false;
      }
      while (pmtOffset + 1 <= pmtProgramInfoEnd)
      {
        // As per EN 50221, we only add conditional access descriptors to the CA PMT.
        byte descriptorTag = pmt[pmtOffset];
        byte descriptorLength = pmt[pmtOffset + 1];
        if (pmtOffset + 2 + descriptorLength > pmtProgramInfoEnd)
        {
          Log.Log.Debug("CA PMT: program descriptor {0} ({1:x}) length is invalid", descriptorTag, descriptorTag);
          return false;
        }
        if (descriptorTag == (byte)DescriptorType.ConditionalAccess)
        {
          if (caPmtProgramDescriptorsLength == 0)
          {
            tempCaPmt[caPmtOffset++] = (byte)command;
          }
          Buffer.BlockCopy(pmt, pmtOffset, tempCaPmt, caPmtOffset, descriptorLength + 2);
          caPmtProgramDescriptorsLength += descriptorLength + 2;
          caPmtOffset += descriptorLength + 2;
        }
        pmtOffset += descriptorLength + 2;
      }
      if (pmtOffset != pmtProgramInfoEnd)
      {
        Log.Log.Debug("CA PMT: PMT corruption detected");
        return false;
      }

      // Set the program_info_length now that we know what the length is.
      tempCaPmt[4] = (byte)((caPmtProgramDescriptorsLength >> 8) & 0x0f);
      tempCaPmt[5] = (byte)(caPmtProgramDescriptorsLength & 0xff);

      // elementary streams
      int pmtEnd = ((pmt[1] & 0x0f) << 8) + pmt[2] + 3 - 4;   // = section length + first 3 bytes - CRC length
      if (pmtEnd > pmt.Length - 4)
      {
        Log.Log.Debug("CA PMT: section length is invalid");
        return false;
      }
      while (pmtOffset + 4 <= pmtEnd)
      {
        // We add each video, audio, subtitle and teletext stream with their corresponding conditional access
        // descriptors to the CA PMT. The first trick is to find out if the stream is one that we want to
        // include. That requires checking the stream type and descriptor type(s).
        byte streamType = pmt[pmtOffset];
        int streamPid = ((pmt[pmtOffset + 1] & 0x01f) << 8) + pmt[pmtOffset + 2];
        int esInfoLength = ((pmt[pmtOffset + 3] & 0x0f) << 8) + pmt[pmtOffset + 4];
        if (pmtOffset + 5 + esInfoLength > pmtEnd)
        {
          Log.Log.Debug("CA PMT: elementary stream info length for PID {0} ({1:x}) is invalid", streamPid, streamPid);
          return false;
        }
        bool isStreamToInclude = false;
        if (streamType == (byte)StreamType.Mpeg1Part2Video ||
          streamType == (byte)StreamType.Mpeg2Part2Video ||
          streamType == (byte)StreamType.Mpeg4Part2Video ||
          streamType == (byte)StreamType.Mpeg4Part10Video ||
          streamType == (byte)StreamType.Mpeg1Part3Audio ||
          streamType == (byte)StreamType.Mpeg2Part3Audio ||
          streamType == (byte)StreamType.Mpeg2Part7Audio ||
          streamType == (byte)StreamType.Mpeg4Part3Audio ||
          streamType == (byte)StreamType.Ac3Audio ||
          streamType == (byte)StreamType.EnhancedAc3Audio)
        {
          isStreamToInclude = true;
        }
        else
        {
          // It isn't obvious from the stream type that this would be a stream that we want to include.
          // Quickly step over the stream descriptors and use them to help us make the decision about whether
          // to include this stream or not.
          int length = esInfoLength;
          int offset = pmtOffset + 5;   // This should point at the tag of the first descriptor.
          while (length > 2)
          {
            byte descriptorTag = pmt[offset];
            byte descriptorLength = pmt[offset + 1];
            if (descriptorTag == (byte)DescriptorType.VideoStream ||
              descriptorTag == (byte)DescriptorType.Mpeg4Video ||
              descriptorTag == (byte)DescriptorType.AvcVideo ||
              descriptorTag == (byte)DescriptorType.AudioStream ||
              descriptorTag == (byte)DescriptorType.Mpeg4Audio ||
              descriptorTag == (byte)DescriptorType.Mpeg2AacAudio ||
              descriptorTag == (byte)DescriptorType.Aac ||
              descriptorTag == (byte)DescriptorType.Ac3 ||        // DVB
              descriptorTag == (byte)DescriptorType.Ac3Audio ||   // ATSC
              descriptorTag == (byte)DescriptorType.EnhancedAc3 ||
              descriptorTag == (byte)DescriptorType.Dts ||
              descriptorTag == (byte)DescriptorType.Subtitling ||
              descriptorTag == (byte)DescriptorType.Teletext)
            {
              isStreamToInclude = true;
              break;
            }
            length -= (descriptorLength + 2);
            offset += descriptorLength + 2;
          }
        }

        if (isStreamToInclude)
        {
          // We include video, audio, subtitle and teletext stream details and CA descriptors in the CA PMT.
          Buffer.BlockCopy(pmt, pmtOffset, tempCaPmt, caPmtOffset, 3);
          pmtOffset += 5;
          caPmtOffset += 5;   // Skip the ES_info_length field until we know what the length is.

          // elementary stream descriptors
          int caPmtEsDescriptorsLength = 0;
          while (esInfoLength >= 2)
          {
            // As per EN 50221, we only add conditional access descriptors to the CA PMT.
            byte descriptorTag = pmt[pmtOffset];
            byte descriptorLength = pmt[pmtOffset + 1];
            if (pmtOffset + descriptorLength + 2 > pmtEnd)
            {
              Log.Log.Debug("CA PMT: elementary stream descriptor {0} ({1:x}) length for PID {2} ({3:x}) is invalid", descriptorTag, descriptorTag, streamPid, streamPid);
              return false;
            }
            if (descriptorTag == (byte)DescriptorType.ConditionalAccess)
            {
              if (caPmtEsDescriptorsLength == 0)
              {
                tempCaPmt[caPmtOffset++] = (byte)command;
              }
              Buffer.BlockCopy(pmt, pmtOffset, tempCaPmt, caPmtOffset, descriptorLength + 2);
              caPmtEsDescriptorsLength += descriptorLength + 2;
              caPmtOffset += descriptorLength + 2;
            }
            esInfoLength -= (descriptorLength + 2);
            pmtOffset += descriptorLength + 2;
          }
          tempCaPmt[caPmtOffset - caPmtEsDescriptorsLength - 2] = (byte)((caPmtEsDescriptorsLength >> 8) & 0x0f);
          tempCaPmt[caPmtOffset - caPmtEsDescriptorsLength - 1] = (byte)(caPmtEsDescriptorsLength & 0xff);
        }
        else
        {
          // Skip over streams that we're not going to include.
          pmtOffset += esInfoLength + 5;
        }
      }
      if (pmtOffset != pmtEnd)
      {
        Log.Log.Debug("CA PMT: PMT corruption detected");
        return false;
      }

      caPmt = new byte[caPmtOffset];
      Buffer.BlockCopy(tempCaPmt, 0, caPmt, 0, caPmtOffset);
      return true;
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
