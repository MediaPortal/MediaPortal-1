using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  class StringUtils
  {
    public static string StreamIdentifier2Str(int stream_ident)
    {
      switch (stream_ident)
      {
        #region MPEG-2 video (0x101-0x108)
        case 0x101:
          return "MPEG-2 video, 4:3 aspect ratio, 25 Hz";
        case 0x102:
          return "MPEG-2 video, 16:9 aspect ratio with pan vectors, 25 Hz";
        case 0x103:
          return "MPEG-2 video, 16:9 aspect ratio without pan vectors, 25 Hz";
        case 0x104:
          return "MPEG-2 video, >16:9 aspect ratio, 25 Hz";
        case 0x105:
          return "MPEG-2 video, 4:3 aspect ratio, 30 Hz";
        case 0x106:
          return "MPEG-2 video, 16:9 aspect ratio with pan vectors, 30 Hz";
        case 0x107:
          return "MPEG-2 video, 16:9 aspect ratio without pan vectors, 30 Hz";
        case 0x108:
          return "MPEG-2 video, >16:9 aspect ratio, 30 Hz";
        #endregion

        #region MPEG-2 HD video (0x109-0x110)
        case 0x109:
          return "MPEG-2 high definition video, 4:3 aspect ratio, 25 Hz";
        case 0x10A:
          return "MPEG-2 high definition video, 16:9 aspect ratio with pan vectors, 25 Hz";
        case 0x10B:
          return "MPEG-2 high definition video, 16:9 aspect ratio without pan vectors, 25 Hz";
        case 0x10C:
          return "MPEG-2 high definition video, >16:9 aspect ratio, 25 Hz";
        case 0x10D:
          return "MPEG-2 high definition video, 4:3 aspect ratio, 30 Hz";
        case 0x10E:
          return "MPEG-2 high definition video, 16:9 aspect ratio with pan vectors, 30 Hz";
        case 0x10F:
          return "MPEG-2 high definition video, 16:9 aspect ratio without pan vectors, 30 Hz";
        case 0x110:
          return "MPEG-2 high definition video, >16:9 aspect ratio, 30 Hz";
        #endregion

        #region MPEG-1 audio (0x201-0x241)
        case 0x201:
          return "MPEG-1 Layer 2 audio, single mono channel";
        case 0x202:
          return "MPEG-1 Layer 2 audio, dual mono channel";
        case 0x203:
          return "MPEG-1 Layer 2 audio, stereo (2 channels)";
        case 0x204:
          return "MPEG-1 Layer 2 audio, multilingual, multi-channel)";
        case 0x205:
          return "MPEG-1 Layer 2 audio, surround sound";
        case 0x240:
          return "MPEG-1 Layer 2 audio description for visually impaired";
        case 0x241:
          return "MPEG-1 Layer 2 audio for the hard of hearing";
        #endregion

        #region Teletext (0x301-0x302)
        case 0x301:
          return "EBU Teletext subtitles";
        case 0x302:
          return "associated EBU Teletext";
        #endregion

        case 0x303:
          return "VBI data";

        #region DVB subtitles (0x310-0x323)
        case 0x310:
          return "DVB subtitles (normal) with no monitor aspect ratio critical";
        case 0x311:
          return "DVB subtitles (normal) for display 4:3 aspect ratio monitor";
        case 0x312:
          return "DVB subtitles (normal) for display 16:9 aspect ratio monitor";
        case 0x313:
          return "DVB subtitles (normal) for display 2.21:1 aspect ratio monitor";
        case 0x320:
          return "DVB subtitles (for the hard hearing) with no monitor aspect ratio critical";
        case 0x321:
          return "DVB subtitles (for the hard hearing) for display 4:3 aspect ratio monitor";
        case 0x322:
          return "DVB subtitles (for the hard hearing) for display 16:9 aspect ratio monitor";
        case 0x323:
          return "DVB subtitles (for the hard hearing) for display 2.21:1 aspect ratio monitor";
        #endregion

        #region H.264/AVC video (0x501-0x510)
        case 0x501:
          return "H.264/AVC standard definition video, 4:3 aspect ratio, 25 Hz";
        case 0x503:
          return "H.264/AVC standard definition video, 16:9 aspect ratio, 25 Hz";
        case 0x504:
          return "H.264/AVC standard definition video, >16:9 aspect ratio, 25 Hz";
        case 0x505:
          return "H.264/AVC standard definition video, 4:3 aspect ratio, 30 Hz";
        case 0x507:
          return "H.264/AVC standard definition video, 16:9 aspect ratio, 30 Hz";
        case 0x508:
          return "H.264/AVC standard definition video, >16:9 aspect ratio, 30 Hz";
        case 0x50B:
          return "H.264/AVC high definition video, 16:9 aspect ratio, 25 Hz";
        case 0x50C:
          return "H.264/AVC high definition video, >16:9 aspect ratio, 25 Hz";
        case 0x50F:
          return "H.264/AVC high definition video, 16:9 aspect ratio, 30 Hz";
        case 0x510:
          return "H.264/AVC high definition video, >16:9 aspect ratio, 30 Hz";
        #endregion

        #region HE-AAC audio (0x601-0x642)
        case 0x601:
          return "HE-AAC audio, single mono channel";
        case 0x603:
          return "HE-AAC audio, stereo";
        case 0x605:
          return "HE-AAC audio, surround sound";
        case 0x640:
          return "HE-AAC audio description for the visually impaired";
        case 0x641:
          return "HE-AAC audio for the hard of hearing";
        case 0x642:
          return "HE-AAC audio receiver-mixed supplementary audio";
        #endregion

        #region HE-AAC v2 audio (0x643-0x646)
        case 0x643:
          return "HE-AAC v2 audio, stereo";
        case 0x644:
          return "HE-AAC v2 audio description for the visually impaired";
        case 0x645:
          return "HE-AAC v2 audio for the hard of hearing";
        case 0x646:
          return "HE-AAC v2 audio receiver-mixed supplementary audio";
        #endregion

        default:
          return "unknown (0x" + stream_ident.ToString("x")+")";
      }
    }
    public static string CA_System_ID2Str(int ca_id)
    {
      if (ca_id == 0)
        return "Reserved";
      if (ca_id < 0x100)
        return "Standardized Systems";
      if (ca_id < 0x200)
        return "Canal Plus (Seca/MediaGuard)";
      if (ca_id < 0x300)
        return "CCETT";
      if (ca_id < 0x400)
        return "MSG MediaServices GmbH";
      if (ca_id < 0x500)
        return "Eurodec";
      if (ca_id < 0x600)
        return "France Telecom (Viaccess)";
      if (ca_id < 0x700)
        return "Irdeto";
      if (ca_id < 0x8ff)
        return "Jerrold/GI/Motorola";
      if (ca_id < 0x900)
        return "Matra Communication";
      if (ca_id < 0xa00)
        return "News Datacom (Videoguard)";
      if (ca_id < 0xb00)
        return "Nokia";
      if (ca_id < 0xc00)
        return "Norwegian Telekom (Conax)";
      if (ca_id < 0xd00)
        return "NTL";
      if (ca_id < 0xe00)
        return "Philips (Cryptoworks)";
      if (ca_id < 0xf00)
        return "Scientific Atlanta (Power VU)";
      if (ca_id < 0x1000)
        return "Sony";
      if (ca_id < 0x1100)
        return "Tandberg Television";
      if (ca_id < 0x1200)
        return "Thompson";
      if (ca_id < 0x1300)
        return "TV/COM";
      if (ca_id < 0x1400)
        return "HPT - Croatian Post and Telecommunications";
      if (ca_id < 0x1500)
        return "HRT - Croatian Radio and Television";
      if (ca_id < 0x1600)
        return "IBM";
      if (ca_id < 0x1700)
        return "Nera";
      if (ca_id < 0x1800)
        return "Beta Technik (Betacrypt)";
      if (ca_id < 0x1900)
        return "Kudelski SA";
      if (ca_id < 0x1a00)
        return "Titan Information Systems";
      if (ca_id >= 0x2000 && ca_id <= 0x20ff)
        return "Telefónica Servicios Audiovisuales";
      if (ca_id >= 0x2100 && ca_id <= 0x21ff)
        return "STENTOR (France Telecom, CNES and DGA)";
      if (ca_id >= 0x2200 && ca_id <= 0x22ff)
        return "Scopus Network Technologies";
      if (ca_id >= 0x2300 && ca_id <= 0x23ff)
        return "BARCO AS";
      if (ca_id >= 0x2400 && ca_id <= 0x24ff)
        return "StarGuide Digital Networks";
      if (ca_id >= 0x2500 && ca_id <= 0x25ff)
        return "Mentor Data System, Inc.";
      if (ca_id >= 0x2600 && ca_id <= 0x26ff)
        return "European Broadcasting Union";
      if (ca_id >= 0x4700 && ca_id <= 0x47ff)
        return "General Instrument";
      if (ca_id >= 0x4800 && ca_id <= 0x48ff)
        return "Telemann";
      if (ca_id >= 0x4900 && ca_id <= 0x49ff)
        return "Digital TV Industry Alliance of China";
      if (ca_id >= 0x4a00 && ca_id <= 0x4aff)
        return "Tsinghua TongFang";
      if (ca_id >= 0x4a10 && ca_id <= 0x4a1f)
        return "Easycas";
      if (ca_id >= 0x4a20 && ca_id <= 0x4a2f)
        return "AlphaCrypt";
      if (ca_id >= 0x4a30 && ca_id <= 0x4a3f)
        return "DVN Holdings";
      if (ca_id >= 0x4a40 && ca_id <= 0x4a4f)
        return "Shanghai Advanced Digital Technology Co. Ltd. (ADT)";
      if (ca_id >= 0x4a50 && ca_id <= 0x4a5f)
        return "Shenzhen Kingsky Company (China) Ltd";
      if (ca_id >= 0x4a60 && ca_id <= 0x4a6f)
        return "@SKY";
      if (ca_id >= 0x4a70 && ca_id <= 0x4a7f)
        return "DreamCrypt";
      if (ca_id >= 0x4a80 && ca_id <= 0x4a8f)
        return "THALESCrypt";
      if (ca_id >= 0x4a90 && ca_id <= 0x4a9f)
        return "Runcom Technologies";
      if (ca_id >= 0x4aa0 && ca_id <= 0x4aaf)
        return "SIDSA";
      if (ca_id >= 0x4ab0 && ca_id <= 0x4abf)
        return "Beijing Compunicate Technology Inc.";
      if (ca_id >= 0x4ac0 && ca_id <= 0x4acf)
        return "Latens Systems Ltd";

      return "Unknown CA_System_ID (0x" + ca_id.ToString("x") + ")";
    }

    public static string StreamTypeToStr(int streamType)
    {
      switch (streamType)
      {
        case 0x00:
          return "ITU-T | ISO/IEC reserved";
        case 0x01:
          return "[Video MPEG-1] ISO/IEC 11172-2 Video";
        case 0x02:
          return "[Video MPEG-2] (ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream)";
        case 0x03:
          return "[Audio MPEG-1] (ISO/IEC 11172-3 Audio)";
        case 0x04:
          return "[Audio MPEG-2] (ISO/IEC 13818-3 Audio)";
        case 0x5:
          return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 private_sections";
        case 0x06:
          return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing private data";
        case 0x07:
          return "[MHW-MHEG] ISO/IEC 13522 MHEG";
        case 0x08:
          return "Annex A - DSM CC";
        case 0x09:
          return "[DATA] ITU-T Rec. H.222.1";
        case 0x0A:
          return "ISO/IEC 13818-6 type A";
        case 0x0B:
          return "ISO/IEC 13818-6 type B";
        case 0x0C:
          return "ISO/IEC 13818-6 type C";
        case 0x0D:
          return "ISO/IEC 13818-6 type D";
        case 0x0E:
          return "ISO/IEC 13818-1 auxiliary";
        case 0x0F:
          return "ISO/IEC 13818-7 Audio with ADTS transport syntax";
        case 0x10:
          return "ISO/IEC 14496-2 Visual";
        case 0x11:
          return "ISO/IEC 14496-3 Audio with the LATM transport syntax as defined in ISO/IEC 14496-3 / AMD 1";
        case 0x12:
          return "ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in PES packets";
        case 0x13:
          return "ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in ISO/IEC14496_sections.";
        case 0x14:
          return "ISO/IEC 13818-6 Synchronized Download Protocol"; 
        case 0x15:
          return "Metadata carried in PES packets";
        case 0x16:
          return "Metadata carried in metadata_sections"; 
        case 0x17:
          return "Metadata carried in ISO/IEC 13818-6 Data Carousel"; 
        case 0x18:
          return "Metadata carried in ISO/IEC 13818-6 Object Carousel"; 
        case 0x19:
          return "Metadata carried in ISO/IEC 13818-6 Synchronized Download Protocol";
        case 0x1A:
          return "IPMP stream (defined in ISO/IEC 13818-11, MPEG-2 IPMP)";
        case 0x1B:
          return "AVC video stream as defined in ITU-T Rec. H.264 | ISO/IEC 14496-10 Video";
        case 0x1C:
          return "ISO/IEC 14496-3 Audio, without using any additional transport syntax";
        case 0x1D:
          return "ISO/IEC 14496-17 Text";
        case 0x1E:
          return "Auxiliary video data stream as defined in ISO/IEC 23002-3";
        case 0x7F:
          return "IPMP stream";
      }
      if (streamType >= 0x1F && streamType <= 0x7E)
        return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 reserved";
      if (streamType > 0x80)
        return "User private";
      return "Unknown";
    }

    public static string getString468A(byte[] data, int offset, int len)
    {
      byte em_ON = 0x86;
      byte em_OFF = 0x87;
      if (len < 1) return "";
      if (offset + len > data.Length)
        return "";

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < len; i++)
      {
        if (data[offset + i] >= 0x80 && data[offset + i] <= 0x9f) continue;
        if (data[offset + i] < 0x20) continue;
        if (data[offset + i] == em_ON || data[offset + i] == em_OFF) continue;
        sb.Append((char)data[offset + i]);
      }
      return sb.ToString();
    }
  }
}
