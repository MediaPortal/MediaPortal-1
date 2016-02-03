#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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
using System.Linq;
using System.Text.RegularExpressions;

using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Player.MediaInfo;

namespace MediaPortal.Player
{
    public delegate void StoreStreamAction(string filterName, string name, int lcid, int id, DirectShowHelper.StreamType type, AMStreamSelectInfoFlags flag, IAMStreamSelect pStrm);

    public class DirectShowHelper
    {
        public enum StreamType
        {
            Video,
            Audio,
            Subtitle,
            Subtitle_hidden,
            Subtitle_shown,
            Edition,
            Subtitle_file,
            PostProcessing,
            Unknown,
        }

        private readonly StoreStreamAction storeAction;

        #region Match dictionaries

        private static readonly Dictionary<string, string> Channels = new Dictionary<string, string>
        {
            { "1 channel", "Mono" },
            { "2 channels", "Stereo" },
            { "6 channels", "5.1" },
            { "7 channels", "6.1" },
            { "8 channels", "7.1" },
        };

        private static readonly Dictionary<string, string> NameEncoders = new Dictionary<string, string>
        {
            { "Mainconcept MP4 Sound Media Handler", "" },
            { "Mainconcept MP4 Video Media Handler", "" },
            { "SoundHandler", "" },
            { "VideoHandler", "" },
            { "L-SMASH Video Handler", ""},
            { "L-SMASH Sound Handler", ""},
        };

        private static readonly Dictionary<string, string> Codecs = new Dictionary<string, string>
        {
            { "MP1", "MPEG Audio" },
            { "MP2", "MPEG Audio" },
            { "MP3", "MPEG Audio" },
            { "MP3ADU", "MPEG Audio" },
            { "MP3ON4", "MPEG Audio" },
            { "MP4ALS", "ALS" },
            { "AC3", "AC-3" },
            { "ADTS", "DTS" },
            { "DTS", "DTS" },
            { "DTSHD", "DTS" },
            { "DTS-HD MA", "DTS" },
            { "DTS-ES", "DTS" },
            { "DTS 96/24", "DTS" },
            { "DTS-HD HRA", "DTS" },
            { "DTS EXPRESS", "DTS" },
            { "EAC3", "E-AC-3" },
            { "FLAC", "FLAC" },
            { "OPUS", "OPUS" },
            { "TTA1", "TTA" },
            { "TTA", "TTA" },
            { "VORBIS", "Vorbis" },
            { "WAVPACK4", "WavPack" },
            { "WAVPACK", "WavPack" },
            { "TRUEHD", "TrueHD" },
            { "MLP", "MLP" },
            { "AAC", "AAC" },
            { "AAC_LATM", "AAC" },
            { "AAC LC", "AAC" },
            { "ALAC", "ALAC" },
            { "ATRAC1", "Atrac" },
            { "ATRAC3", "Atrac" },
            { "ATRAC3P", "Atrac" },
            { "BINKAUDIO_DCT", "" },
            { "BINKAUDIO_RDFT", "" },
            { "PCM_ALAW", "PCM" },
            { "PCM_MULAW", "PCM" },
            { "PCM_BLURAY", "PCM" },
            { "PCM_DVD", "PCM" },
            { "PCM_LXF", "PCM" },
            { "PCM_F32BE", "PCM" },
            { "PCM_F32LE", "PCM" },
            { "PCM_F64BE", "PCM" },
            { "PCM_F64LE", "PCM" },
            { "PCM_S16BE", "PCM" },
            { "PCM_S16BE_PLANAR", "PCM" },
            { "PCM_S16LE", "PCM" },
            { "PCM_S16LE_PLANAR", "PCM" },
            { "PCM_S24BE", "PCM" },
            { "PCM_S24DAUD", "PCM" },
            { "PCM_S24LE", "PCM" },
            { "PCM_S24LE_PLANAR", "PCM" },
            { "PCM_S32BE", "PCM" },
            { "PCM_S32LE", "PCM" },
            { "PCM_S32LE_PLANAR", "PCM" },
            { "PCM_S8", "PCM" },
            { "PCM_S8_PLANAR", "PCM" },
            { "PCM_U16BE", "PCM" },
            { "PCM_U16LE", "PCM" },
            { "PCM_U24BE", "PCM" },
            { "PCM_U24LE", "PCM" },
            { "PCM_U32BE", "PCM" },
            { "PCM_U32LE", "PCM" },
            { "PCM_U8", "PCM" },
            { "PCM_ZORK", "PCM" },
            { "PCM", "PCM" },
            { "APE", "Monkey's Audio" },
            { "RA_144", "RealAudio" },
            { "RA_288", "RealAudio" },
            { "RALF", "RealAudio" },
            { "WMALOSSLESS", "WMA" },
            { "WMAPRO", "WMA" },
            { "WMAV1", "WMA" },
            { "WMAV2", "WMA" },
            { "WMAVOICE", "WMA" },

            { "H264 HIGH L3", "AVC" },
            { "H264 HIGH L3.0", "AVC" },
            { "H264 HIGH L3.1", "AVC" },
            { "H264 HIGH L3.2", "AVC" },
            { "H264 HIGH L4", "AVC" },
            { "H264 HIGH L4.0", "AVC" },
            { "H264 HIGH L4.1", "AVC" },
            { "H264 HIGH L4.2", "AVC" },
            { "H264 HIGH L5", "AVC" },
            { "H264 HIGH L5.0", "AVC" },
            { "H264 HIGH L5.1", "AVC" },
            { "H264 MAIN L3", "AVC" },
            { "H264 MAIN L3.0", "AVC" },
            { "H264 MAIN L3.1", "AVC" },
            { "H264 MAIN L3.2", "AVC" },
            { "H264 MAIN L4", "AVC" },
            { "H264 MAIN L4.0", "AVC" },
            { "H264 MAIN L4.1", "AVC" },
            { "H264 MAIN L4.2", "AVC" },
            { "H264 MAIN L5", "AVC" },
            { "H264 MAIN L5.0", "AVC" },
            { "H264 MAIN L5.1", "AVC" },
            { "8BPS", "QuickTime 8bps" },

            { "DIVX", "" },
            { "DX50", "" },
            { "XVID", "" },
            { "BINKVIDEO", "" },
            { "AMV", "" },
            { "AVRN", "" },
            { "AVRP", "" },
            { "AYUV", "" },
            { "CAVS", "" },
            { "DVVIDEO", "" },
            { "FLASHSV", "" },
            { "FLASHSV2", "" },
            { "FLIC", "" },
            { "FLV1", "" },
            { "FRAPS", "" },
            { "H261", "H.261" },
            { "H263", "H.263" },
            { "H263I", "H.263" },
            { "H263P", "H.263" },
            { "HEVC", "HEVC" },
            { "INDEO2", "" },
            { "INDEO3", "" },
            { "INDEO4", "" },
            { "INDEO5", "" },
            { "MDEC", "" },
            { "MPEG1VIDEO", "" },
            { "MPEG2VIDEO", "" },
            { "MPEG4", "" },
            { "MPEGVIDEO_XVMC", "" },
            { "MSMPEG4V1", "" },
            { "MSMPEG4V2", "" },
            { "MSMPEG4V3", "" },
            { "MSS1", "" },
            { "MSS2", "" },
            { "MSVIDEO1", "" },
            { "PGM", "" },
            { "PGMYUV", "" },
            { "QTRLE", "" },
            { "RAWVIDEO", "" },
            { "RPZA", "" },
            { "RV10", "" },
            { "RV20", "" },
            { "RV30", "" },
            { "RV40", "" },
            { "THEORA", "" },
            { "V210", "" },
            { "V210X", "" },
            { "V308", "" },
            { "V408", "" },
            { "V410", "" },
            { "VC1", "" },
            { "VC1IMAGE", "" },
            { "VCR1", "" },
            { "VP3", "" },
            { "VP5", "" },
            { "VP6", "" },
            { "VP6A", "" },
            { "VP6F", "" },
            { "VP8", "" },
            { "VP9", "" },
            { "WEBP", "" },
            { "WMV1", "" },
            { "WMV2", "" },
            { "WMV3", "" },
            { "WMV3IMAGE", "" },
            { "Y41P", "" },
            { "YUV4", "" },
        };

        private static readonly Dictionary<string, string> Languages = new Dictionary<string, string>
        {
            { "ABK", "Abkhazian" },
            { "AB", "Abkhazian" },
            { "ACE", "Achinese" },
            { "ACH", "Acoli" },
            { "ADA", "Adangme" },
            { "AAR", "Afar" },
            { "AA", "Afar" },
            { "AFH", "Afrihili" },
            { "AFR", "Afrikaans" },
            { "AF", "Afrikaans" },
            { "AKA", "Akan" },
            { "AK", "Akan" },
            { "AKK", "Akkadian" },
            { "ALB", "Albanian" },
            { "SQI", "Albanian" },
            { "SQ", "Albanian" },
            { "ALE", "Aleut" },
            { "AMH", "Amharic" },
            { "AM", "Amharic" },
            { "ARA", "Arabic" },
            { "AR", "Arabic" },
            { "ARG", "Aragonese" },
            { "AN", "Aragonese" },
            { "ARC", "Aramaic" },
            { "ARP", "Arapaho" },
            { "ARN", "Araucanian" },
            { "ARW", "Arawak" },
            { "ARM", "Armenian" },
            { "HY", "Armenian" },
            { "HYE", "Armenian" },
            { "AS", "Assamese" },
            { "ASM", "Assamese" },
            { "AVA", "Avaric" },
            { "AV", "Avaric" },
            { "AVE", "Avestan" },
            { "AE", "Avestan" },
            { "AWA", "Awadhi" },
            { "AYM", "Aymara" },
            { "AY", "Aymara" },
            { "AZE", "Azerbaijani" },
            { "AZ", "Azerbaijani" },
            { "BAN", "Balinese" },
            { "BAL", "Baluchi" },
            { "BAM", "Bambara" },
            { "BM", "Bambara" },
            { "BAD", "Banda" },
            { "BAS", "Basa" },
            { "BAK", "Bashkir" },
            { "BA", "Bashkir" },
            { "BAQ", "Basque" },
            { "EU", "Basque" },
            { "EUS", "Basque" },
            { "BEJ", "Beja" },
            { "BEL", "Belarusian" },
            { "BE", "Belarusian" },
            { "BEM", "Bemba" },
            { "BEN", "Bengali" },
            { "BN", "Bengali" },
            { "BHO", "Bhojpuri" },
            { "BH", "Bihari" },
            { "BIH", "Bihari" },
            { "BIK", "Bikol" },
            { "BIN", "Bini" },
            { "BIS", "Bislama" },
            { "BI", "Bislama" },
            { "NOB", "Norwegian Bokmal" },
            { "NB", "Norwegian Bokmal" },
            { "BOS", "Bosnian" },
            { "BS", "Bosnian" },
            { "BRA", "Braj" },
            { "BRE", "Breton" },
            { "BR", "Breton" },
            { "BUG", "Buginese" },
            { "BUL", "Bulgarian" },
            { "BG", "Bulgarian" },
            { "BUA", "Buriat" },
            { "BUR", "Burmese" },
            { "MY", "Burmese" },
            { "MYA", "Burmese" },
            { "CAD", "Caddo" },
            { "CAR", "Carib" },
            { "SPA", "Spanish" },
            { "ES", "Spanish" },
            { "ESP", "Spanish" },
            { "CAT", "Catalan" },
            { "CA", "Catalan" },
            { "CEB", "Cebuano" },
            { "CEL", "Celtic" },
            { "CHG", "Chagatai" },
            { "CH", "Chamorro" },
            { "CHA", "Chamorro" },
            { "CE", "Chechen" },
            { "CHE", "Chechen" },
            { "CHR", "Cherokee" },
            { "CHY", "Cheyenne" },
            { "CHB", "Chibcha" },
            { "CHI", "Chinese" },
            { "ZH", "Chinese" },
            { "ZHO", "Chinese" },
            { "CHP", "Chipewyan" },
            { "CHO", "Choctaw" },
            { "CHK", "Chuukese" },
            { "CHV", "Chuvash" },
            { "CV", "Chuvash" },
            { "COP", "Coptic" },
            { "COR", "Cornish" },
            { "KW", "Cornish" },
            { "CO", "Corsican" },
            { "COS", "Corsican" },
            { "CRE", "Cree" },
            { "CR", "Cree" },
            { "MUS", "Creek" },
            { "CRP", "Creoles and pidgins" },
            { "CPE", "Creoles and pidgins" },
            { "CPF", "Creoles and pidgins" },
            { "CPP", "Creoles and pidgins" },
            { "SCR", "Croatian" },
            { "HR", "Croatian" },
            { "HRV", "Croatian" },
            { "CZE", "Czech" },
            { "CS", "Czech" },
            { "CES", "Czech" },
            { "DAK", "Dakota" },
            { "DAN", "Danish" },
            { "DA", "Danish" },
            { "DAR", "Dargwa" },
            { "DAY", "Dayak" },
            { "DEL", "Delaware" },
            { "DIN", "Dinka" },
            { "DIV", "Divehi" },
            { "DV", "Divehi" },
            { "DOI", "Dogri" },
            { "DGR", "Dogrib" },
            { "DRA", "Dravidian" },
            { "DUA", "Duala" },
            { "DUT", "Dutch" },
            { "NL", "Dutch" },
            { "NLD", "Dutch" },
            { "DUM", "Dutch" },
            { "DYU", "Dyula" },
            { "DZ", "Dzongkha" },
            { "DZO", "Dzongkha" },
            { "EFI", "Efik" },
            { "EGY", "Egyptian" },
            { "EKA", "Ekajuk" },
            { "ELX", "Elamite" },
            { "ENG", "English" },
            { "EN", "English" },
            { "ENM", "English" },
            { "ANG", "English" },
            { "EO", "Esperanto" },
            { "EPO", "Esperanto" },
            { "EST", "Estonian" },
            { "ET", "Estonian" },
            { "EE", "Ewe" },
            { "EWE", "Ewe" },
            { "EWO", "Ewondo" },
            { "FAN", "Fang" },
            { "FAT", "Fanti" },
            { "FAO", "Faroese" },
            { "FO", "Faroese" },
            { "FIJ", "Fijian" },
            { "FJ", "Fijian" },
            { "FIN", "Finnish" },
            { "FI", "Finnish" },
            { "FON", "Fon" },
            { "FRE", "French" },
            { "FR", "French" },
            { "FRA", "French" },
            { "FRM", "French" },
            { "FRO", "French" },
            { "FRY", "Frisian" },
            { "FY", "Frisian" },
            { "FUR", "Friulian" },
            { "FF", "Fulah" },
            { "FUL", "Fulah" },
            { "GAA", "Ga" },
            { "GLA", "Gaelic" },
            { "GD", "Gaelic" },
            { "GLG", "Gallegan" },
            { "GL", "Gallegan" },
            { "LUG", "Ganda" },
            { "LG", "Ganda" },
            { "GAY", "Gayo" },
            { "GBA", "Gbaya" },
            { "GEZ", "Geez" },
            { "GEO", "Georgian" },
            { "KA", "Georgian" },
            { "KAT", "Georgian" },
            { "GER", "German" },
            { "DE", "German" },
            { "DEU", "German" },
            { "NDS", "German" },
            { "GMH", "German" },
            { "GOH", "German" },
            { "GIL", "Gilbertese" },
            { "GON", "Gondi" },
            { "GOR", "Gorontalo" },
            { "GOT", "Gothic" },
            { "GRB", "Grebo" },
            { "GRC", "Ancient Greek" },
            { "GRE", "Greek" },
            { "EL", "Greek" },
            { "ELL", "Greek" },
            { "GRN", "Guarani" },
            { "GN", "Guarani" },
            { "GUJ", "Gujarati" },
            { "GU", "Gujarati" },
            { "HAI", "Haida" },
            { "HAU", "Hausa" },
            { "HA", "Hausa" },
            { "HAW", "Hawaiian" },
            { "HEB", "Hebrew" },
            { "HE", "Hebrew" },
            { "HER", "Herero" },
            { "HZ", "Herero" },
            { "HIL", "Hiligaynon" },
            { "HIM", "Himachali" },
            { "HIN", "Hindi" },
            { "HI", "Hindi" },
            { "HMO", "Hiri Motu" },
            { "HO", "Hiri Motu" },
            { "HIT", "Hittite" },
            { "HMN", "Hmong" },
            { "HUN", "Hungarian" },
            { "HU", "Hungarian" },
            { "HUP", "Hupa" },
            { "IBA", "Iban" },
            { "ICE", "Icelandic" },
            { "IS", "Icelandic" },
            { "ISL", "Icelandic" },
            { "IDO", "Ido" },
            { "IO", "Ido" },
            { "IBO", "Igbo" },
            { "IG", "Igbo" },
            { "IJO", "Ijo" },
            { "ILO", "Iloko" },
            { "SMN", "Inari Sami" },
            { "IND", "Indonesian" },
            { "ID", "Indonesian" },
            { "INH", "Ingush" },
            { "IKU", "Inuktitut" },
            { "IU", "Inuktitut" },
            { "IPK", "Inupiaq" },
            { "IK", "Inupiaq" },
            { "GLE", "Irish" },
            { "GA", "Irish" },
            { "MGA", "Irish" },
            { "SGA", "Irish" },
            { "ITA", "Italian" },
            { "IT", "Italian" },
            { "JPN", "Japanese" },
            { "JA", "Japanese" },
            { "JAV", "Javanese" },
            { "JV", "Javanese" },
            { "JRB", "Judeo-Arabic" },
            { "JPR", "Judeo-Persian" },
            { "KBD", "Kabardian" },
            { "KAB", "Kabyle" },
            { "KAC", "Kachin" },
            { "KAM", "Kamba" },
            { "KAN", "Kannada" },
            { "KN", "Kannada" },
            { "KAU", "Kanuri" },
            { "KR", "Kanuri" },
            { "KAA", "Kara-Kalpak" },
            { "KAR", "Karen" },
            { "KAS", "Kashmiri" },
            { "KS", "Kashmiri" },
            { "KAW", "Kawi" },
            { "KAZ", "Kazakh" },
            { "KK", "Kazakh" },
            { "KHA", "Khasi" },
            { "KHM", "Khmer" },
            { "KM", "Khmer" },
            { "KHO", "Khotanese" },
            { "KMB", "Kimbundu" },
            { "KIN", "Kinyarwanda" },
            { "RW", "Kinyarwanda" },
            { "KY", "Kirghiz" },
            { "KIR", "Kirghiz" },
            { "KV", "Komi" },
            { "KOM", "Komi" },
            { "KON", "Kongo" },
            { "KG", "Kongo" },
            { "KOK", "Konkani" },
            { "KOR", "Korean" },
            { "KO", "Korean" },
            { "KOS", "Kosraean" },
            { "KPE", "Kpelle" },
            { "KRO", "Kru" },
            { "KUM", "Kumyk" },
            { "KUR", "Kurdish" },
            { "KU", "Kurdish" },
            { "KRU", "Kurukh" },
            { "KUT", "Kutenai" },
            { "LAD", "Ladino" },
            { "LAH", "Lahnda" },
            { "LAM", "Lamba" },
            { "LAO", "Lao" },
            { "LO", "Lao" },
            { "LAT", "Latin" },
            { "LA", "Latin" },
            { "LAV", "Latvian" },
            { "LV", "Latvian" },
            { "LEZ", "Lezghian" },
            { "LN", "Lingala" },
            { "LIN", "Lingala" },
            { "LIT", "Lithuanian" },
            { "LT", "Lithuanian" },
            { "LOZ", "Lozi" },
            { "LUB", "Luba-Katanga" },
            { "LU", "Luba-Katanga" },
            { "LUA", "Luba-Lulua" },
            { "LUI", "Luiseno" },
            { "SMJ", "Lule Sami" },
            { "LUN", "Lunda" },
            { "LUS", "Lushai" },
            { "MAC", "Macedonian" },
            { "MK", "Macedonian" },
            { "MKD", "Macedonian" },
            { "MAD", "Madurese" },
            { "MAG", "Magahi" },
            { "MAI", "Maithili" },
            { "MAK", "Makasar" },
            { "MLG", "Malagasy" },
            { "MG", "Malagasy" },
            { "MAY", "Malay" },
            { "MS", "Malay" },
            { "MSA", "Malay" },
            { "MAL", "Malayalam" },
            { "ML", "Malayalam" },
            { "MLT", "Maltese" },
            { "MT", "Maltese" },
            { "MNC", "Manchu" },
            { "MDR", "Mandar" },
            { "MAN", "Mandingo" },
            { "MNI", "Manipuri" },
            { "GLV", "Manx" },
            { "GV", "Manx" },
            { "MAO", "Maori" },
            { "MI", "Maori" },
            { "MRI", "Maori" },
            { "MAR", "Marathi" },
            { "MR", "Marathi" },
            { "CHM", "Mari" },
            { "MAH", "Marshallese" },
            { "MH", "Marshallese" },
            { "MWR", "Marwari" },
            { "MAS", "Masai" },
            { "MEN", "Mende" },
            { "MIC", "Micmac" },
            { "MIN", "Minangkabau" },
            { "MOH", "Mohawk" },
            { "MOL", "Moldavian" },
            { "MO", "Moldavian" },
            { "LOL", "Mongo" },
            { "MON", "Mongolian" },
            { "MN", "Mongolian" },
            { "MOS", "Mossi" },
            { "NAH", "Nahuatl" },
            { "NAU", "Nauru" },
            { "NA", "Nauru" },
            { "NAV", "Navaho" },
            { "NV", "Navaho" },
            { "NDE", "Ndebele" },
            { "ND", "Ndebele" },
            { "NR", "Ndebele" },
            { "NBL", "Ndebele" },
            { "NDO", "Ndonga" },
            { "NG", "Ndonga" },
            { "NAP", "Neapolitan" },
            { "NEP", "Nepali" },
            { "NE", "Nepali" },
            { "NEW", "Newari" },
            { "NIA", "Nias" },
            { "NIU", "Niuean" },
            { "NON", "Norse" },
            { "SME", "Northern Sami" },
            { "SE", "Northern Sami" },
            { "NOR", "Norwegian" },
            { "NO", "Norwegian" },
            { "NNO", "Norwegian Nynorsk" },
            { "NN", "Norwegian Nynorsk" },
            { "NYM", "Nyamwezi" },
            { "NYA", "Nyanja" },
            { "NY", "Nyanja" },
            { "NYN", "Nyankole" },
            { "NYO", "Nyoro" },
            { "NZI", "Nzima" },
            { "OCI", "Occitan" },
            { "OC", "Occitan" },
            { "OJI", "Ojibwa" },
            { "OJ", "Ojibwa" },
            { "ORI", "Oriya" },
            { "OR", "Oriya" },
            { "ORM", "Oromo" },
            { "OM", "Oromo" },
            { "OSA", "Osage" },
            { "OS", "Ossetian" },
            { "OSS", "Ossetian" },
            { "PAL", "Pahlavi" },
            { "PAU", "Palauan" },
            { "PLI", "Pali" },
            { "PI", "Pali" },
            { "PAM", "Pampanga" },
            { "PAG", "Pangasinan" },
            { "PAN", "Panjabi" },
            { "PA", "Panjabi" },
            { "PAP", "Papiamento" },
            { "PER", "Persian" },
            { "FA", "Persian" },
            { "FAS", "Persian" },
            { "PEO", "Persian" },
            { "PHN", "Phoenician" },
            { "PON", "Pohnpeian" },
            { "POL", "Polish" },
            { "PL", "Polish" },
            { "POR", "Portuguese" },
            { "PT", "Portuguese" },
            { "POB", "Portuguese (Brazil)" },
            { "PB", "Portuguese (Brazil)" },
            { "PUS", "Pushto" },
            { "PS", "Pushto" },
            { "QUE", "Quechua" },
            { "QU", "Quechua" },
            { "ROH", "Raeto-Romance" },
            { "RM", "Raeto-Romance" },
            { "RAJ", "Rajasthani" },
            { "RAP", "Rapanui" },
            { "RAR", "Rarotongan" },
            { "RUM", "Romanian" },
            { "RO", "Romanian" },
            { "RON", "Romanian" },
            { "ROM", "Romany" },
            { "RUN", "Rundi" },
            { "RN", "Rundi" },
            { "RUS", "Russian" },
            { "RU", "Russian" },
            { "SAM", "Samaritan Aramaic" },
            { "SMO", "Samoan" },
            { "SM", "Samoan" },
            { "SAD", "Sandawe" },
            { "SAG", "Sango" },
            { "SG", "Sango" },
            { "SAN", "Sanskrit" },
            { "SA", "Sanskrit" },
            { "SAT", "Santali" },
            { "SRD", "Sardinian" },
            { "SC", "Sardinian" },
            { "SAS", "Sasak" },
            { "SCO", "Scots" },
            { "SEL", "Selkup" },
            { "SRP", "Serbian" },
            { "SR", "Serbian" },
            { "SCC", "Serbian" },
            { "SRR", "Serer" },
            { "SHN", "Shan" },
            { "SNA", "Shona" },
            { "SN", "Shona" },
            { "III", "Sichuan Yi" },
            { "II", "Sichuan Yi" },
            { "SID", "Sidamo" },
            { "BLA", "Siksika" },
            { "SND", "Sindhi" },
            { "SD", "Sindhi" },
            { "SIN", "Sinhalese" },
            { "SI", "Sinhalese" },
            { "SMS", "Skolt Sami" },
            { "SLO", "Slovak" },
            { "SK", "Slovak" },
            { "SLK", "Slovak" },
            { "SLV", "Slovenian" },
            { "SL", "Slovenian" },
            { "SOG", "Sogdian" },
            { "SOM", "Somali" },
            { "SO", "Somali" },
            { "SON", "Songhai" },
            { "SNK", "Soninke" },
            { "NSO", "Sotho" },
            { "SOT", "Sotho" },
            { "ST", "Sotho" },
            { "SMA", "Southern Sami" },
            { "SUK", "Sukuma" },
            { "SUX", "Sumerian" },
            { "SUN", "Sundanese" },
            { "SU", "Sundanese" },
            { "SUS", "Susu" },
            { "SWA", "Swahili" },
            { "SW", "Swahili" },
            { "SSW", "Swati" },
            { "SS", "Swati" },
            { "SWE", "Swedish" },
            { "SV", "Swedish" },
            { "SYR", "Syriac" },
            { "TGL", "Tagalog" },
            { "TL", "Tagalog" },
            { "TAH", "Tahitian" },
            { "TY", "Tahitian" },
            { "TGK", "Tajik" },
            { "TG", "Tajik" },
            { "TMH", "Tamashek" },
            { "TAM", "Tamil" },
            { "TA", "Tamil" },
            { "TAT", "Tatar" },
            { "TT", "Tatar" },
            { "TEL", "Telugu" },
            { "TE", "Telugu" },
            { "TER", "Tereno" },
            { "TET", "Tetum" },
            { "THA", "Thai" },
            { "TH", "Thai" },
            { "TIB", "Tibetan" },
            { "BO", "Tibetan" },
            { "BOD", "Tibetan" },
            { "TIG", "Tigre" },
            { "TIR", "Tigrinya" },
            { "TI", "Tigrinya" },
            { "TEM", "Timne" },
            { "TIV", "Tiv" },
            { "TLI", "Tlingit" },
            { "TPI", "Tok Pisin" },
            { "TKL", "Tokelau" },
            { "TOG", "Tonga" },
            { "TON", "Tonga" },
            { "TO", "Tonga" },
            { "TSI", "Tsimshian" },
            { "TS", "Tsonga" },
            { "TSO", "Tsonga" },
            { "TSN", "Tswana" },
            { "TN", "Tswana" },
            { "TUM", "Tumbuka" },
            { "TUR", "Turkish" },
            { "TR", "Turkish" },
            { "OTA", "Turkish" },
            { "TUK", "Turkmen" },
            { "TK", "Turkmen" },
            { "TVL", "Tuvalu" },
            { "TYV", "Tuvinian" },
            { "TWI", "Twi" },
            { "TW", "Twi" },
            { "UGA", "Ugaritic" },
            { "UIG", "Uighur" },
            { "UG", "Uighur" },
            { "UKR", "Ukrainian" },
            { "UK", "Ukrainian" },
            { "UMB", "Umbundu" },
            { "URD", "Urdu" },
            { "UR", "Urdu" },
            { "UZB", "Uzbek" },
            { "UZ", "Uzbek" },
            { "VAI", "Vai" },
            { "VEN", "Venda" },
            { "VE", "Venda" },
            { "VIE", "Vietnamese" },
            { "VI", "Vietnamese" },
            { "VOL", "Volapuk" },
            { "VO", "Volapuk" },
            { "VOT", "Votic" },
            { "WAL", "Walamo" },
            { "WLN", "Walloon" },
            { "WA", "Walloon" },
            { "WAR", "Waray" },
            { "WAS", "Washo" },
            { "WEL", "Welsh" },
            { "CY", "Welsh" },
            { "CYM", "Welsh" },
            { "WOL", "Wolof" },
            { "WO", "Wolof" },
            { "XHO", "Xhosa" },
            { "XH", "Xhosa" },
            { "SAH", "Yakut" },
            { "YAO", "Yao" },
            { "YAP", "Yapese" },
            { "YID", "Yiddish" },
            { "YI", "Yiddish" },
            { "YOR", "Yoruba" },
            { "YO", "Yoruba" },
            { "ZND", "Zande" },
            { "ZAP", "Zapotec" },
            { "ZEN", "Zenaga" },
            { "ZHA", "Zhuang" },
            { "ZA", "Zhuang" },
            { "ZUL", "Zulu" },
            { "ZU", "Zulu" },
            { "ZUN", "Zuni" },
            { "NWC", "Newari" },
            { "TLH", "Klingon" },
            { "BYN", "Blin" },
            { "JBO", "Lojban" },
            { "CSB", "Kashubian" },
            { "CRH", "Crimean Turkish" },
            { "MYV", "Erzya" },
            { "MDF", "Moksha" },
            { "KRC", "Karachay-Balkar" },
            { "ADY", "Adyghe" },
            { "UDM", "Udmurt" },
            { "NOG", "Nogai" },
            { "HAT", "Haitian" },
            { "HT", "Haitian" },
            { "XAL", "Kalmyk" }
        };

        private static readonly Dictionary<string, int> FiltersToSkip = new Dictionary<string, int>
        {
            { "ffdshow DXVA Video Decoder", 0 },
            { "ffdshow Video Decoder", 0 },
            { "ffdshow raw video filter", 0 }
        };

        #endregion

        public double[] Chapters { get; private set; }

        public string[] ChaptersName { get; private set; }

        public DirectShowHelper(StoreStreamAction storeAction)
        {
            Chapters = null;
            ChaptersName = null;
            this.storeAction = storeAction;
        }

        public bool AnalyseStreams(IGraphBuilder graphBuilder)
        {
            try
            {
                //RETRIEVING THE CURRENT SPLITTER
                var foundfilter = new IBaseFilter[2];
                IEnumFilters enumFilters;
                graphBuilder.EnumFilters(out enumFilters);
                if (enumFilters != null)
                {
                    try
                    {
                        enumFilters.Reset();
                        int fetched;
                        while (enumFilters.Next(1, foundfilter, out fetched) == 0)
                        {
                            if (foundfilter[0] != null && fetched == 1)
                            {
                                try
                                {
                                    if (Chapters == null)
                                    {
                                        var pEs = foundfilter[0] as IAMExtendedSeeking;
                                        if (pEs != null)
                                        {
                                            int markerCount;
                                            if (pEs.get_MarkerCount(out markerCount) == 0 && markerCount > 0)
                                            {
                                                Chapters = new double[markerCount];
                                                ChaptersName = new string[markerCount];
                                                for (var i = 1; i <= markerCount; i++)
                                                {
                                                    double markerTime;
                                                    pEs.GetMarkerTime(i, out markerTime);
                                                    Chapters[i - 1] = markerTime;
                                                    //fill up chapter names
                                                    string name;
                                                    pEs.GetMarkerName(i, out name);
                                                    ChaptersName[i - 1] = name;
                                                }
                                            }
                                        }
                                    }
                                    IAMStreamSelect pStrm = foundfilter[0] as IAMStreamSelect;
                                    if (pStrm != null)
                                    {
                                        FilterInfo foundfilterinfos;
                                        foundfilter[0].QueryFilterInfo(out foundfilterinfos);
                                        var filter = foundfilterinfos.achName;
                                        int cStreams;
                                        pStrm.Count(out cStreams);

                                        //GET STREAMS
                                        for (int istream = 0; istream < cStreams; istream++)
                                        {
                                            AMMediaType sType;
                                            AMStreamSelectInfoFlags sFlag;
                                            int sPdwGroup, sPlcId;
                                            string sName;
                                            object pppunk, ppobject;
                                            var type = StreamType.Unknown;
                                            //STREAM INFO
                                            pStrm.Info(istream, out sType, out sFlag, out sPlcId,
                                                       out sPdwGroup, out sName, out pppunk, out ppobject);
                                            //Avoid listing ffdshow video filter's plugins amongst subtitle and audio streams and editions.
                                            if (FiltersToSkip.ContainsKey(filter) &&
                                                ((sPdwGroup == 1) || (sPdwGroup == 2) || (sPdwGroup == 18) || (sPdwGroup == 4)))
                                            {
                                                type = StreamType.Unknown;
                                            }
                                            //VIDEO
                                            else if (sPdwGroup == 0)
                                            {
                                                type = StreamType.Video;
                                            }
                                            //AUDIO
                                            else if (sPdwGroup == 1)
                                            {
                                                type = StreamType.Audio;
                                            }
                                            //SUBTITLE
                                            else if (sPdwGroup == 2 && sName.LastIndexOf("off", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Hide ", StringComparison.Ordinal) == -1 &&
                                                     sName.LastIndexOf("No ", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Miscellaneous ", StringComparison.Ordinal) == -1)
                                            {
                                                type = StreamType.Subtitle;
                                            }
                                            //NO SUBTITILE TAG
                                            else if ((sPdwGroup == 2 && (sName.LastIndexOf("off", StringComparison.Ordinal) != -1 || sName.LastIndexOf("No ", StringComparison.Ordinal) != -1)) ||
                                                     (sPdwGroup == 6590033 && sName.LastIndexOf("Hide ", StringComparison.Ordinal) != -1))
                                            {
                                                type = StreamType.Subtitle_hidden;
                                            }
                                            //DirectVobSub SHOW SUBTITLE TAG
                                            else if (sPdwGroup == 6590033 && sName.LastIndexOf("Show ", StringComparison.Ordinal) != -1)
                                            {
                                                type = StreamType.Subtitle_shown;
                                            }
                                            //EDITION
                                            else if (sPdwGroup == 18)
                                            {
                                                type = StreamType.Edition;
                                            }
                                            else if (sPdwGroup == 4) //Subtitle file
                                            {
                                                type = StreamType.Subtitle_file;
                                            }
                                            else if (sPdwGroup == 10) //Postprocessing filter
                                            {
                                                type = StreamType.PostProcessing;
                                            }
                                            Log.Debug("DirectShowHelper: FoundStreams: Type={0}; Name={1}, Filter={2}, Id={3}, PDWGroup={4}, LCID={5}",
                                                      type.ToString(), sName, filter, istream.ToString(),
                                                      sPdwGroup.ToString(), sPlcId.ToString());

                                            if (storeAction != null)
                                            {
                                                storeAction(filter, sName, sPlcId, istream, type, sFlag, pStrm);
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    DirectShowUtil.ReleaseComObject(foundfilter[0]);
                                }
                            }
                        }
                    }
                    finally
                    {
                        DirectShowUtil.ReleaseComObject(enumFilters);
                    }
                }
            }
            catch { }
            return true;
        }

        private static readonly Regex LavSplitterAudio = new Regex(@"A\:\s*(((?<name>.+)\[(?<language>\w+)\]|\[(?<language>\w+)\]|(?<name>.+))\s*\()?\s*(?<codec>[a-z0-9\s\.'_\-]+),\s*(?<freq>\d+)\s*Hz,\s*(?<channels>[a-z0-9\s\.'_\-]+)(,\s*s(?<bit>\d+))?(,\s*(?<bitrate>\d+)\s*kb/s)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FfdshowNameAudio = new Regex(@"Audio\s*-\s*(?<codec>\w+),\s*(?<channels>[^,]+),\s*(?<freq>\d+)\s*Hz", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex TsReaderAudio = new Regex(@"^(?<language>\w{2,3})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static AudioStream MatchAudioStream(MediaInfoWrapper info, string filterName, string name, int lcid, int id)
        {
            Log.Debug(
                "DirectShowHelper: match audio stream Name={0}, Filter={1}, Id={2}, LCID={3}",
                name,
                filterName,
                id,
                lcid);

            var m = LavSplitterAudio.Match(name);
            if (m.Success)
            {
                return LavSplitterAudioMatch(info, m);
            }

            m = FfdshowNameAudio.Match(name);
            if (m.Success)
            {
                return FfdshowAudioMatch(info, m);
            }

            m = TsReaderAudio.Match(name);
            if (m.Success)
            {
                return TsReaderAudioMatch(info, id, m);
            }

            return null;
        }

        public static string GetLanguage(string source)
        {
            string result;
            return Languages.TryGetValue(source.ToUpper(), out result) ? result : string.Empty;
        }

        private static AudioStream TsReaderAudioMatch(MediaInfoWrapper info, int id, Match m)
        {
            var result = id < info.AudioStreams.Count ? info.AudioStreams[id] : null;
            if (result != null)
            {
                string language;
                if (Languages.TryGetValue(m.Groups["language"].ToString().ToUpper(), out language))
                {
                    if (result.Language != language)
                    {
                        return null;
                    }
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }

        private static string CheckNameEncoder(string name)
        {
            string result;
            return NameEncoders.TryGetValue(name, out result) ? result : name;
        }

        private static AudioStream FfdshowAudioMatch(MediaInfoWrapper info, Match m)
        {
            var codec = GetCodec(m.Groups["codec"].Value);
            int frequency;
            if (!int.TryParse(m.Groups["freq"].Value, out frequency))
            {
                frequency = 0;
            }

            var channelFrendly = m.Groups["channels"].Value;

            return
                info.AudioStreams.FirstOrDefault(
                    x =>
                    x.Format.Equals(codec, StringComparison.OrdinalIgnoreCase) && 
                    x.AudioChannelsFriendly.Equals(channelFrendly, StringComparison.OrdinalIgnoreCase) && 
                    (int)x.SamplingRate == frequency/* && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)*/);
        }

        private static string GetCodec(string sourceCodecName)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(sourceCodecName))
            {
                if (!Codecs.TryGetValue(sourceCodecName.ToUpper(), out result))
                {
                    result = sourceCodecName.ToUpper();
                }
            }

            return result;
        }

        private static string EncodeChannels(string source)
        {
            string result;
            return Channels.TryGetValue(source, out result) ? result : source;
        }

        private static AudioStream LavSplitterAudioMatch(MediaInfoWrapper info, Match m)
        {
            string language;
            string codec;
            string name;
            GetLavMainParameters(m, out language, out codec, out name);
            int frequency;
            if (!int.TryParse(m.Groups["freq"].Value, out frequency))
            {
                frequency = 0;
            }

            var channelFrendly = EncodeChannels(m.Groups["channels"].Value);
            int bit;
            if (!int.TryParse(m.Groups["bit"].Value, out bit))
            {
                bit = 0;
            }

            return 
                info.AudioStreams.FirstOrDefault(
                    x =>
                    x.Format.Equals(codec, StringComparison.OrdinalIgnoreCase) && 
                    x.AudioChannelsFriendly.Equals(channelFrendly, StringComparison.OrdinalIgnoreCase) &&
                    (int)x.SamplingRate == frequency && 
                    (x.BitDepth == bit || bit == 0) && 
                    (string.IsNullOrEmpty(language) || x.Language.Equals(language, StringComparison.OrdinalIgnoreCase)) &&
                    ((string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(language) && language.Equals(name, StringComparison.OrdinalIgnoreCase)) || x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
        }

        private static void GetLavMainParameters(Match m, out string language, out string codec, out string name)
        {
            name = CheckNameEncoder(m.Groups["name"].Value.TrimEnd());
            language = GetLanguage(m.Groups["language"].Value);
            codec = GetCodec(m.Groups["codec"].Value);
        }

        private static readonly Regex LavSplitterVideo = new Regex(@"V\:\s*(((?<name>.+)\[(?<language>\w+)\]|\[(?<language>\w+)\]|(?<name>.+))\s*\()?\s*(?<codec>[a-z0-9\s\.'_\-]+),\s*(?<output>[a-z0-9\s\.'_\-]+),\s*(?<width>\d+)x(?<height>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static VideoStream MatchVideoStream(MediaInfoWrapper info, string filterName, string name, int lcid, int id)
        {
            Log.Debug(
                "DirectShowHelper: match video stream Name={0}, Filter={1}, Id={2}, LCID={3}",
                name,
                filterName,
                id,
                lcid);

            var m = LavSplitterVideo.Match(name);
            if (m.Success)
            {
                return LavSplitterVideoMatch(info, m);
            }

            return null;
        }

        private static VideoStream LavSplitterVideoMatch(MediaInfoWrapper info, Match m)
        {
            string language;
            string codec;
            string name;
            GetLavMainParameters(m, out language, out codec, out name);
            int width;
            if (!int.TryParse(m.Groups["width"].Value, out width))
            {
                width = 0;
            }
            int height;
            if (!int.TryParse(m.Groups["height"].Value, out height))
            {
                height = 0;
            }

            return
                info.VideoStreams.FirstOrDefault(
                    x =>
                    (x.Width == width || width == 0) &&
                    (x.Height == height || height == 0) &&
                    (string.IsNullOrEmpty(codec) || x.Format.Equals(codec, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(language) || x.Language.Equals(language, StringComparison.OrdinalIgnoreCase)) &&
                    ((string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(language) && language.Equals(name, StringComparison.OrdinalIgnoreCase)) || x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}