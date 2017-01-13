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

using System.ComponentModel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum
{
  /// <summary>
  /// TV Engine stream types used for unified standard handling.
  /// </summary>
  /// <remarks>
  /// Standard, de-facto standard or unique stream types are used where
  /// possible to simplify the conversion between stream type and logical
  /// stream type.
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
    /// MPEG ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2
    /// constrained parameter video stream (MPEG 2 video)
    /// </summary>
    [Description("MPEG 2 Video")]
    VideoMpeg2 = 0x02,
    /// <summary>
    /// MPEG ISO/IEC 14496-2 visual (MPEG 4 video)
    /// </summary>
    [Description("MPEG 4 Part 2 Video")]
    VideoMpeg4Part2 = 0x10,
    /// <summary>
    /// MPEG AVC video stream conforming to one or more profiles defined in
    /// Annex A of Rec. ITU-T H.264 | ISO/IEC 14496-10 or AVC video
    /// sub-bitstream of SVC as defined in 2.1.78 or MVC base view sub
    /// bitstream, as defined in 2.1.85, or AVC video sub-bitstream of MVC, as
    /// defined in 2.1.88 or MVCD base view sub bitstream, as defined in
    /// 2.1.97, or AVC video sub-bitstream of MVCD, as defined in 2.1.100
    /// (H.264 video)
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) Video")]
    VideoMpeg4Part10 = 0x1b,
    /// <summary>
    /// MPEG auxiliary video stream as defined in ISO/IEC 23002-3
    /// </summary>
    [Description("MPEG C Video")]
    VideoMpegC = 0x1e,
    /// <summary>
    /// MPEG SVC video sub-bitstream of an AVC video stream conforming to one
    /// or more profiles defined in Annex G of ITU-T Rec. H.264 | ISO/IEC
    /// 14496-10
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) Annex G (SVC) Video")]
    VideoMpeg4Part10AnnexG = 0x1f,
    /// <summary>
    /// MPEG MVC video sub-bitstream of an AVC video stream conforming to one
    /// or more profiles defined in Annex H of ITU-T Rec. H.264 | ISO/IEC
    /// 14496-10
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) Annex H (MVC) Video")]
    VideoMpeg4Part10AnnexH = 0x20,
    /// <summary>
    /// MPEG video stream conforming to one or more profiles as defined in
    /// ITU-T Rec T.800 | ISO/IEC 15444-1
    /// </summary>
    [Description("JPEG Video")]
    VideoJpeg = 0x21,
    /// <summary>
    /// MPEG additional view ITU-T Rec. H.262 | ISO/IEC 13818-2 video stream
    /// for service-compatible stereoscopic 3D services
    /// </summary>
    [Description("MPEG 2 3D Video")]
    VideoMpeg2StereoscopicAdditionalView = 0x22,
    /// <summary>
    /// MPEG additional view ITU-T Rec. H.264 | ISO/IEC 14496-10 video stream
    /// conforming to one or more profiles defined in Annex A for
    /// service-compatible stereoscopic 3D services
    /// </summary>
    [Description("MPEG 4 Part 10 (H.264/AVC) 3D Video")]
    VideoMpeg4Part10StereoscopicAdditionalView = 0x23,
    /// <summary>
    /// MPEG ITU-T Rec. H.265 | ISO/IEC 23008-2 visual
    /// </summary>
    [Description("MPEG H Part 2 (H.265/HEVC)")]
    VideoMpegHPart2 = 0x24,

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
    /// MPEG ISO/IEC 14496-3 audio with the LATM transport syntax as defined in
    /// ISO/IEC 14496-3 (LATM-AAC audio)
    /// </summary>
    [Description("MPEG 4 Part 3 (LATM-AAC) Audio")]
    AudioMpeg4Part3Latm = 0x11,
    /// <summary>
    /// MPEG ISO/IEC 14496-3 audio, without using any additional transport
    /// syntax, such as DST, ALS and SLS
    /// </summary>
    [Description("MPEG 4 Part 3 (AAC) Audio")]
    AudioMpeg4Part3 = 0x1c,

    #endregion

    #region non-standard

    /// <summary>
    /// VC-1 video
    /// </summary>
    /// <remarks>
    /// Matches the SCTE stream type. DVB use a registration descriptor to
    /// identify VC-1. Use in ATSC is a work in progress (A/73).
    /// </remarks>
    [Description("VC-1 Video")]
    VideoVc1 = 0xea,
    /// <summary>
    /// AC-3/Dolby Digital audio
    /// </summary>
    /// <remarks>
    /// Matches the ATSC and SCTE stream type. DVB has the AC-3 descriptor (tag
    /// 0x6a).
    /// </remarks>
    [Description("AC-3 (Dolby Digital) Audio")]
    AudioAc3 = 0x81,
    /// <summary>
    /// Enhanced AC-3/Dolby Digital Plus audio
    /// </summary>
    /// <remarks>
    /// Matches the ATSC and SCTE stream type. DVB has the enhanced AC-3
    /// descriptor (tag 0x7a).
    /// </remarks>
    [Description("Enhanced AC-3 (Dolby Digital Plus) Audio")]
    AudioEnhancedAc3 = 0x87,
    /// <summary>
    /// AC-4 audio
    /// </summary>
    /// <remarks>
    /// DVB has the AC-4 [extended] descriptor (tag extension 0x15).
    /// </remarks>
    [Description("AC-4 Audio")]
    AudioAc4 = 1002,
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
    /// Matches the ATSC 2.0 and SCTE stream type. DVB has the DTS-HD
    /// [extended] descriptor (tag extension 0x0e). Note SCTE previously used
    /// the DTS-HD descriptor (tag 0x7b).
    /// </remarks>
    [Description("DTS-HD Audio")]
    AudioDtsHd = 0x88,
    /// <summary>
    /// Teletext
    /// </summary>
    /// <remarks>
    /// DVB has the teletext descriptor (tag 0x56). ATSC and SCTE don't support
    /// teletext.
    /// </remarks>
    Teletext = 1001,
    /// <summary>
    /// Subtitles
    /// </summary>
    /// <remarks>
    /// Matches the SCTE stream type (format probably not supported). DVB has
    /// the subtitling descriptor (tag 0x59).
    /// </remarks>
    Subtitles = 0x82

    #endregion
  }
}