/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "stdafx.h"

#include "DemuxerUtils.h"

#pragma warning(push)
#pragma warning(disable:4244)
extern "C" {
#define __STDC_CONSTANT_MACROS
#include "libavformat/avformat.h"
#include "libavutil/intreadwrite.h"
#include "libavutil/pixdesc.h"
}
#pragma warning(pop)

#define countof(array) (sizeof(array) / sizeof(array[0]))

static struct
{
  const wchar_t *name;
  const wchar_t *iso6392;
  const wchar_t *iso6391;
  const wchar_t *iso6392_2;
  LCID lcid;
} isoLanguages [] =	// TODO : fill LCID !!!
{
  { L"Abkhazian", L"abk", L"ab" },
  { L"Achinese", L"ace", NULL },
  { L"Acoli", L"ach", NULL },
  { L"Adangme", L"ada", NULL },
  { L"Afar", L"aar", L"aa" },
  { L"Afrihili", L"afh", NULL },
  { L"Afrikaans", L"afr", L"af", NULL, MAKELCID( MAKELANGID(LANG_AFRIKAANS, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Afro-Asiatic (Other)", L"afa", NULL },
  { L"Akan", L"aka", L"ak" },
  { L"Akkadian", L"akk", NULL },
  { L"Albanian", L"sqi", L"sq", L"alb", MAKELCID( MAKELANGID(LANG_ALBANIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Aleut", L"ale", NULL },
  { L"Algonquian languages", L"alg", NULL },
  { L"Altaic (Other)", L"tut", NULL },
  { L"Amharic", L"amh", L"am" },
  { L"Apache languages", L"apa", NULL },
  { L"Arabic", L"ara", L"ar", NULL, MAKELCID( MAKELANGID(LANG_ARABIC, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Aragonese", L"arg", L"an" },
  { L"Aramaic", L"arc", NULL },
  { L"Arapaho", L"arp", NULL },
  { L"Araucanian", L"arn", NULL },
  { L"Arawak", L"arw", NULL },
  { L"Armenian", L"arm", L"hy", L"hye", MAKELCID( MAKELANGID(LANG_ARMENIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Artificial (Other)", L"art", NULL },
  { L"Assamese", L"asm", L"as", NULL, MAKELCID( MAKELANGID(LANG_ASSAMESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Asturian; Bable", L"ast", NULL },
  { L"Athapascan languages", L"ath", NULL },
  { L"Australian languages", L"aus", NULL },
  { L"Austronesian (Other)", L"map", NULL },
  { L"Avaric", L"ava", L"av" },
  { L"Avestan", L"ave", L"ae" },
  { L"Awadhi", L"awa", NULL },
  { L"Aymara", L"aym", L"ay" },
  { L"Azerbaijani", L"aze", L"az", NULL, MAKELCID( MAKELANGID(LANG_AZERI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Bable; Asturian", L"ast", NULL },
  { L"Balinese", L"ban", NULL },
  { L"Baltic (Other)", L"bat", NULL },
  { L"Baluchi", L"bal", NULL },
  { L"Bambara", L"bam", L"bm" },
  { L"Bamileke languages", L"bai", NULL },
  { L"Banda", L"bad", NULL },
  { L"Bantu (Other)", L"bnt", NULL },
  { L"Basa", L"bas", NULL },
  { L"Bashkir", L"bak", L"ba", NULL, MAKELCID( MAKELANGID(LANG_BASHKIR, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Basque", L"baq", L"eu", L"eus", MAKELCID( MAKELANGID(LANG_BASQUE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Batak (Indonesia)", L"btk", NULL },
  { L"Beja", L"bej", NULL },
  { L"Belarusian", L"bel", L"be", NULL, MAKELCID( MAKELANGID(LANG_BELARUSIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Bemba", L"bem", NULL },
  { L"Bengali", L"ben", L"bn", NULL, MAKELCID( MAKELANGID(LANG_BENGALI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Berber (Other)", L"ber", NULL },
  { L"Bhojpuri", L"bho", NULL },
  { L"Bihari", L"bih", L"bh" },
  { L"Bikol", L"bik", NULL },
  { L"Bini", L"bin", NULL },
  { L"Bislama", L"bis", L"bi" },
  { L"Bokmål, Norwegian; Norwegian Bokmål", L"nob", L"nb" },
  { L"Bosnian", L"bos", L"bs" },
  { L"Braj", L"bra", NULL },
  { L"Breton", L"bre", L"br", NULL, MAKELCID( MAKELANGID(LANG_BRETON, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Buginese", L"bug", NULL },
  { L"Bulgarian", L"bul", L"bg", NULL, MAKELCID( MAKELANGID(LANG_BULGARIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Buriat", L"bua", NULL },
  { L"Burmese", L"bur", L"my", L"mya" },
  { L"Caddo", L"cad", NULL },
  { L"Carib", L"car", NULL },
  { L"Spanish", L"spa", L"es", L"esp", MAKELCID( MAKELANGID(LANG_SPANISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Catalan", L"cat", L"ca", NULL, MAKELCID( MAKELANGID(LANG_CATALAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Caucasian (Other)", L"cau", NULL },
  { L"Cebuano", L"ceb", NULL },
  { L"Celtic (Other)", L"cel", NULL },
  { L"Central American Indian (Other)", L"cai", NULL },
  { L"Chagatai", L"chg", NULL },
  { L"Chamic languages", L"cmc", NULL },
  { L"Chamorro", L"cha", L"ch" },
  { L"Chechen", L"che", L"ce" },
  { L"Cherokee", L"chr", NULL },
  { L"Chewa; Chichewa; Nyanja", L"nya", L"ny" },
  { L"Cheyenne", L"chy", NULL },
  { L"Chibcha", L"chb", NULL },
  { L"Chinese", L"chi", L"zh", L"zho", MAKELCID( MAKELANGID(LANG_CHINESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Chinook jargon", L"chn", NULL },
  { L"Chipewyan", L"chp", NULL },
  { L"Choctaw", L"cho", NULL },
  { L"Chuang; Zhuang", L"zha", L"za" },
  { L"Church Slavic; Old Church Slavonic", L"chu", L"cu" },
  { L"Old Church Slavonic; Old Slavonic; ", L"chu", L"cu" },
  { L"Chuukese", L"chk", NULL },
  { L"Chuvash", L"chv", L"cv" },
  { L"Coptic", L"cop", NULL },
  { L"Cornish", L"cor", L"kw" },
  { L"Corsican", L"cos", L"co", NULL, MAKELCID( MAKELANGID(LANG_CORSICAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Cree", L"cre", L"cr" },
  { L"Creek", L"mus", NULL },
  { L"Creoles and pidgins (Other)", L"crp", NULL },
  { L"Creoles and pidgins,", L"cpe", NULL },
  //   {"English-based (Other)", NULL, NULL},
  { L"Creoles and pidgins,", L"cpf", NULL },
  //   {"French-based (Other)", NULL, NULL},
  { L"Creoles and pidgins,", L"cpp", NULL },
  //   {"Portuguese-based (Other)", NULL, NULL},
  { L"Croatian", L"scr", L"hr", L"hrv", MAKELCID( MAKELANGID(LANG_CROATIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Cushitic (Other)", L"cus", NULL },
  { L"Czech", L"cze", L"cs", L"ces", MAKELCID( MAKELANGID(LANG_CZECH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Dakota", L"dak", NULL },
  { L"Danish", L"dan", L"da", NULL, MAKELCID( MAKELANGID(LANG_DANISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Dargwa", L"dar", NULL },
  { L"Dayak", L"day", NULL },
  { L"Delaware", L"del", NULL },
  { L"Dinka", L"din", NULL },
  { L"Divehi", L"div", L"dv", NULL, MAKELCID( MAKELANGID(LANG_DIVEHI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Dogri", L"doi", NULL },
  { L"Dogrib", L"dgr", NULL },
  { L"Dravidian (Other)", L"dra", NULL },
  { L"Duala", L"dua", NULL },
  { L"Dutch", L"dut", L"nl", L"nld", MAKELCID( MAKELANGID(LANG_DUTCH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Dutch, Middle (ca. 1050-1350)", L"dum", NULL },
  { L"Dyula", L"dyu", NULL },
  { L"Dzongkha", L"dzo", L"dz" },
  { L"Efik", L"efi", NULL },
  { L"Egyptian (Ancient)", L"egy", NULL },
  { L"Ekajuk", L"eka", NULL },
  { L"Elamite", L"elx", NULL },
  { L"English", L"eng", L"en", NULL, MAKELCID( MAKELANGID(LANG_ENGLISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"English, Middle (1100-1500)", L"enm", NULL },
  { L"English, Old (ca.450-1100)", L"ang", NULL },
  { L"Esperanto", L"epo", L"eo" },
  { L"Estonian", L"est", L"et", NULL, MAKELCID( MAKELANGID(LANG_ESTONIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Ewe", L"ewe", L"ee" },
  { L"Ewondo", L"ewo", NULL },
  { L"Fang", L"fan", NULL },
  { L"Fanti", L"fat", NULL },
  { L"Faroese", L"fao", L"fo", NULL, MAKELCID( MAKELANGID(LANG_FAEROESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Fijian", L"fij", L"fj" },
  { L"Finnish", L"fin", L"fi", NULL, MAKELCID( MAKELANGID(LANG_FINNISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Finno-Ugrian (Other)", L"fiu", NULL },
  { L"Flemish; Dutch", L"dut", L"nl" },
  { L"Flemish; Dutch", L"nld", L"nl" },
  { L"Fon", L"fon", NULL },
  { L"French", L"fre", L"fr", L"fra", MAKELCID( MAKELANGID(LANG_FRENCH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"French, Middle (ca.1400-1600)", L"frm", NULL },
  { L"French, Old (842-ca.1400)", L"fro", NULL },
  { L"Frisian", L"fry", L"fy", NULL, MAKELCID( MAKELANGID(LANG_FRISIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Friulian", L"fur", NULL },
  { L"Fulah", L"ful", L"ff" },
  { L"Ga", L"gaa", NULL },
  { L"Gaelic; Scottish Gaelic", L"gla", L"gd", NULL, MAKELCID( MAKELANGID(LANG_GALICIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Gallegan", L"glg", L"gl" },
  { L"Ganda", L"lug", L"lg" },
  { L"Gayo", L"gay", NULL },
  { L"Gbaya", L"gba", NULL },
  { L"Geez", L"gez", NULL },
  { L"Georgian", L"geo", L"ka", L"kat", MAKELCID( MAKELANGID(LANG_GEORGIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"German", L"ger", L"de", L"deu", MAKELCID( MAKELANGID(LANG_GERMAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"German, Low; Saxon, Low; Low German; Low Saxon", L"nds", NULL },
  { L"German, Middle High (ca.1050-1500)", L"gmh", NULL },
  { L"German, Old High (ca.750-1050)", L"goh", NULL },
  { L"Germanic (Other)", L"gem", NULL },
  { L"Gikuyu; Kikuyu", L"kik", L"ki" },
  { L"Gilbertese", L"gil", NULL },
  { L"Gondi", L"gon", NULL },
  { L"Gorontalo", L"gor", NULL },
  { L"Gothic", L"got", NULL },
  { L"Grebo", L"grb", NULL },
  { L"Ancient Greek", L"grc", NULL },
  { L"Greek", L"gre", L"el", L"ell", MAKELCID( MAKELANGID(LANG_GREEK, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Greenlandic; Kalaallisut", L"kal", L"kl", NULL, MAKELCID( MAKELANGID(LANG_GREENLANDIC, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Guarani", L"grn", L"gn" },
  { L"Gujarati", L"guj", L"gu", NULL, MAKELCID( MAKELANGID(LANG_GUJARATI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Gwich´in", L"gwi", NULL },
  { L"Haida", L"hai", NULL },
  { L"Hausa", L"hau", L"ha", NULL, MAKELCID( MAKELANGID(LANG_HAUSA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Hawaiian", L"haw", NULL },
  { L"Hebrew", L"heb", L"he", NULL, MAKELCID( MAKELANGID(LANG_HEBREW, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Herero", L"her", L"hz" },
  { L"Hiligaynon", L"hil", NULL },
  { L"Himachali", L"him", NULL },
  { L"Hindi", L"hin", L"hi", NULL, MAKELCID( MAKELANGID(LANG_HINDI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Hiri Motu", L"hmo", L"ho" },
  { L"Hittite", L"hit", NULL },
  { L"Hmong", L"hmn", NULL },
  { L"Hungarian", L"hun", L"hu", NULL, MAKELCID( MAKELANGID(LANG_HUNGARIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Hupa", L"hup", NULL },
  { L"Iban", L"iba", NULL },
  { L"Icelandic", L"ice", L"is", L"isl", MAKELCID( MAKELANGID(LANG_ICELANDIC, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Ido", L"ido", L"io" },
  { L"Igbo", L"ibo", L"ig", NULL, MAKELCID( MAKELANGID(LANG_IGBO, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Ijo", L"ijo", NULL },
  { L"Iloko", L"ilo", NULL },
  { L"Inari Sami", L"smn", NULL },
  { L"Indic (Other)", L"inc", NULL },
  { L"Indo-European (Other)", L"ine", NULL },
  { L"Indonesian", L"ind", L"id", NULL, MAKELCID( MAKELANGID(LANG_INDONESIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Ingush", L"inh", NULL },
  { L"Interlingua (International", L"ina", L"ia" },
  //   {"Auxiliary Language Association)", NULL, NULL},
  { L"Interlingue", L"ile", L"ie" },
  { L"Inuktitut", L"iku", L"iu", NULL, MAKELCID( MAKELANGID(LANG_INUKTITUT, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Inupiaq", L"ipk", L"ik" },
  { L"Iranian (Other)", L"ira", NULL },
  { L"Irish", L"gle", L"ga", NULL, MAKELCID( MAKELANGID(LANG_IRISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Irish, Middle (900-1200)", L"mga", NULL, NULL, MAKELCID( MAKELANGID(LANG_IRISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Irish, Old (to 900)", L"sga", NULL, NULL, MAKELCID( MAKELANGID(LANG_IRISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Iroquoian languages", L"iro", NULL },
  { L"Italian", L"ita", L"it", NULL, MAKELCID( MAKELANGID(LANG_ITALIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Japanese", L"jpn", L"ja", NULL, MAKELCID( MAKELANGID(LANG_JAPANESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Javanese", L"jav", L"jv" },
  { L"Judeo-Arabic", L"jrb", NULL },
  { L"Judeo-Persian", L"jpr", NULL },
  { L"Kabardian", L"kbd", NULL },
  { L"Kabyle", L"kab", NULL },
  { L"Kachin", L"kac", NULL },
  { L"Kalaallisut; Greenlandic", L"kal", L"kl" },
  { L"Kamba", L"kam", NULL },
  { L"Kannada", L"kan", L"kn", NULL, MAKELCID( MAKELANGID(LANG_KANNADA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Kanuri", L"kau", L"kr" },
  { L"Kara-Kalpak", L"kaa", NULL },
  { L"Karen", L"kar", NULL },
  { L"Kashmiri", L"kas", L"ks", NULL, MAKELCID( MAKELANGID(LANG_KASHMIRI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Kawi", L"kaw", NULL },
  { L"Kazakh", L"kaz", L"kk", NULL, MAKELCID( MAKELANGID(LANG_KAZAK, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Khasi", L"kha", NULL },
  { L"Khmer", L"khm", L"km", NULL, MAKELCID( MAKELANGID(LANG_KHMER, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Khoisan (Other)", L"khi", NULL },
  { L"Khotanese", L"kho", NULL },
  { L"Kikuyu; Gikuyu", L"kik", L"ki" },
  { L"Kimbundu", L"kmb", NULL },
  { L"Kinyarwanda", L"kin", L"rw", NULL, MAKELCID( MAKELANGID(LANG_KINYARWANDA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Kirghiz", L"kir", L"ky" },
  { L"Komi", L"kom", L"kv" },
  { L"Kongo", L"kon", L"kg" },
  { L"Konkani", L"kok", NULL, NULL, MAKELCID( MAKELANGID(LANG_KONKANI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Korean", L"kor", L"ko", NULL, MAKELCID( MAKELANGID(LANG_KOREAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Kosraean", L"kos", NULL },
  { L"Kpelle", L"kpe", NULL },
  { L"Kru", L"kro", NULL },
  { L"Kuanyama; Kwanyama", L"kua", L"kj" },
  { L"Kumyk", L"kum", NULL },
  { L"Kurdish", L"kur", L"ku" },
  { L"Kurukh", L"kru", NULL },
  { L"Kutenai", L"kut", NULL },
  { L"Kwanyama, Kuanyama", L"kua", L"kj" },
  { L"Ladino", L"lad", NULL },
  { L"Lahnda", L"lah", NULL },
  { L"Lamba", L"lam", NULL },
  { L"Lao", L"lao", L"lo", NULL, MAKELCID( MAKELANGID(LANG_LAO, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Latin", L"lat", L"la" },
  { L"Latvian", L"lav", L"lv", NULL, MAKELCID( MAKELANGID(LANG_LATVIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Letzeburgesch; Luxembourgish", L"ltz", L"lb" },
  { L"Lezghian", L"lez", NULL },
  { L"Limburgan; Limburger; Limburgish", L"lim", L"li" },
  { L"Lingala", L"lin", L"ln" },
  { L"Lithuanian", L"lit", L"lt", NULL, MAKELCID( MAKELANGID(LANG_LITHUANIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Lozi", L"loz", NULL },
  { L"Luba-Katanga", L"lub", L"lu" },
  { L"Luba-Lulua", L"lua", NULL },
  { L"Luiseno", L"lui", NULL },
  { L"Lule Sami", L"smj", NULL },
  { L"Lunda", L"lun", NULL },
  { L"Luo (Kenya and Tanzania)", L"luo", NULL },
  { L"Lushai", L"lus", NULL },
  { L"Luxembourgish; Letzeburgesch", L"ltz", L"lb", NULL, MAKELCID( MAKELANGID(LANG_LUXEMBOURGISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Macedonian", L"mac", L"mk", L"mkd", MAKELCID( MAKELANGID(LANG_MACEDONIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Madurese", L"mad", NULL },
  { L"Magahi", L"mag", NULL },
  { L"Maithili", L"mai", NULL },
  { L"Makasar", L"mak", NULL },
  { L"Malagasy", L"mlg", L"mg" },
  { L"Malay", L"may", L"ms", L"msa", MAKELCID( MAKELANGID(LANG_MALAY, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Malayalam", L"mal", L"ml", NULL, MAKELCID( MAKELANGID(LANG_MALAYALAM, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Maltese", L"mlt", L"mt", NULL, MAKELCID( MAKELANGID(LANG_MALTESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Manchu", L"mnc", NULL },
  { L"Mandar", L"mdr", NULL },
  { L"Mandingo", L"man", NULL },
  { L"Manipuri", L"mni", NULL, NULL, MAKELCID( MAKELANGID(LANG_MANIPURI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Manobo languages", L"mno", NULL },
  { L"Manx", L"glv", L"gv" },
  { L"Maori", L"mao", L"mi", L"mri", MAKELCID( MAKELANGID(LANG_MAORI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Marathi", L"mar", L"mr", NULL, MAKELCID( MAKELANGID(LANG_MARATHI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Mari", L"chm", NULL },
  { L"Marshallese", L"mah", L"mh" },
  { L"Marwari", L"mwr", NULL },
  { L"Masai", L"mas", NULL },
  { L"Mayan languages", L"myn", NULL },
  { L"Mende", L"men", NULL },
  { L"Micmac", L"mic", NULL },
  { L"Minangkabau", L"min", NULL },
  { L"Miscellaneous languages", L"mis", NULL },
  { L"Mohawk", L"moh", NULL, NULL, MAKELCID( MAKELANGID(LANG_MOHAWK, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Moldavian", L"mol", L"mo" },
  { L"Mon-Khmer (Other)", L"mkh", NULL },
  { L"Mongo", L"lol", NULL },
  { L"Mongolian", L"mon", L"mn", NULL, MAKELCID( MAKELANGID(LANG_MONGOLIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Mossi", L"mos", NULL },
  { L"Multiple languages", L"mul", NULL },
  { L"Munda languages", L"mun", NULL },
  { L"Nahuatl", L"nah", NULL },
  { L"Nauru", L"nau", L"na" },
  { L"Navaho, Navajo", L"nav", L"nv" },
  { L"Ndebele, North", L"nde", L"nd" },
  { L"Ndebele, South", L"nbl", L"nr" },
  { L"Ndonga", L"ndo", L"ng" },
  { L"Neapolitan", L"nap", NULL },
  { L"Nepali", L"nep", L"ne", NULL, MAKELCID( MAKELANGID(LANG_NEPALI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Newari", L"new", NULL },
  { L"Nias", L"nia", NULL },
  { L"Niger-Kordofanian (Other)", L"nic", NULL },
  { L"Nilo-Saharan (Other)", L"ssa", NULL },
  { L"Niuean", L"niu", NULL },
  { L"Norse, Old", L"non", NULL },
  { L"North American Indian (Other)", L"nai", NULL },
  { L"Northern Sami", L"sme", L"se" },
  { L"North Ndebele", L"nde", L"nd" },
  { L"Norwegian", L"nor", L"no", NULL, MAKELCID( MAKELANGID(LANG_NORWEGIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Norwegian Bokmål; Bokmål, Norwegian", L"nob", L"nb", NULL, MAKELCID( MAKELANGID(LANG_NORWEGIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Norwegian Nynorsk; Nynorsk, Norwegian", L"nno", L"nn", NULL, MAKELCID( MAKELANGID(LANG_NORWEGIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Nubian languages", L"nub", NULL },
  { L"Nyamwezi", L"nym", NULL },
  { L"Nyanja; Chichewa; Chewa", L"nya", L"ny" },
  { L"Nyankole", L"nyn", NULL },
  { L"Nynorsk, Norwegian; Norwegian Nynorsk", L"nno", L"nn" },
  { L"Nyoro", L"nyo", NULL },
  { L"Nzima", L"nzi", NULL },
  { L"Occitan (post 1500},; Provençal", L"oci", L"oc", NULL, MAKELCID( MAKELANGID(LANG_OCCITAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Ojibwa", L"oji", L"oj" },
  { L"Old Bulgarian; Old Slavonic; Church Slavonic;", L"chu", L"cu" },
  { L"Oriya", L"ori", L"or" },
  { L"Oromo", L"orm", L"om" },
  { L"Osage", L"osa", NULL },
  { L"Ossetian; Ossetic", L"oss", L"os" },
  { L"Ossetic; Ossetian", L"oss", L"os" },
  { L"Otomian languages", L"oto", NULL },
  { L"Pahlavi", L"pal", NULL },
  { L"Palauan", L"pau", NULL },
  { L"Pali", L"pli", L"pi" },
  { L"Pampanga", L"pam", NULL },
  { L"Pangasinan", L"pag", NULL },
  { L"Panjabi", L"pan", L"pa" },
  { L"Papiamento", L"pap", NULL },
  { L"Papuan (Other)", L"paa", NULL },
  { L"Persian", L"per", L"fa", L"fas", MAKELCID( MAKELANGID(LANG_PERSIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Persian, Old (ca.600-400 B.C.)", L"peo", NULL },
  { L"Philippine (Other)", L"phi", NULL },
  { L"Phoenician", L"phn", NULL },
  { L"Pohnpeian", L"pon", NULL },
  { L"Polish", L"pol", L"pl", NULL, MAKELCID( MAKELANGID(LANG_POLISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Portuguese", L"por", L"pt", NULL, MAKELCID( MAKELANGID(LANG_PORTUGUESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Prakrit languages", L"pra", NULL },
  { L"Provençal; Occitan (post 1500)", L"oci", L"oc" },
  { L"Provençal, Old (to 1500)", L"pro", NULL },
  { L"Pushto", L"pus", L"ps" },
  { L"Quechua", L"que", L"qu", NULL, MAKELCID( MAKELANGID(LANG_QUECHUA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Raeto-Romance", L"roh", L"rm" },
  { L"Rajasthani", L"raj", NULL },
  { L"Rapanui", L"rap", NULL },
  { L"Rarotongan", L"rar", NULL },
  { L"Reserved for local use", L"qaa-qtz", NULL },
  { L"Romance (Other)", L"roa", NULL },
  { L"Romanian", L"rum", L"ro", L"ron", MAKELCID( MAKELANGID(LANG_ROMANIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Romany", L"rom", NULL },
  { L"Rundi", L"run", L"rn" },
  { L"Russian", L"rus", L"ru", NULL, MAKELCID( MAKELANGID(LANG_RUSSIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Salishan languages", L"sal", NULL },
  { L"Samaritan Aramaic", L"sam", NULL },
  { L"Sami languages (Other)", L"smi", NULL },
  { L"Samoan", L"smo", L"sm" },
  { L"Sandawe", L"sad", NULL },
  { L"Sango", L"sag", L"sg" },
  { L"Sanskrit", L"san", L"sa", NULL, MAKELCID( MAKELANGID(LANG_SANSKRIT, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Santali", L"sat", NULL },
  { L"Sardinian", L"srd", L"sc" },
  { L"Sasak", L"sas", NULL },
  { L"Scots", L"sco", NULL },
  { L"Scottish Gaelic; Gaelic", L"gla", L"gd" },
  { L"Selkup", L"sel", NULL },
  { L"Semitic (Other)", L"sem", NULL },
  { L"Serbian", L"srp", L"sr", L"scc", MAKELCID( MAKELANGID(LANG_SERBIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Serer", L"srr", NULL },
  { L"Shan", L"shn", NULL },
  { L"Shona", L"sna", L"sn" },
  { L"Sichuan Yi", L"iii", L"ii" },
  { L"Sidamo", L"sid", NULL },
  { L"Sign languages", L"sgn", NULL },
  { L"Siksika", L"bla", NULL },
  { L"Sindhi", L"snd", L"sd", NULL, MAKELCID( MAKELANGID(LANG_SINDHI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Sinhalese", L"sin", L"si", NULL, MAKELCID( MAKELANGID(LANG_SINHALESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Sino-Tibetan (Other)", L"sit", NULL },
  { L"Siouan languages", L"sio", NULL },
  { L"Skolt Sami", L"sms", NULL },
  { L"Slave (Athapascan)", L"den", NULL },
  { L"Slavic (Other)", L"sla", NULL },
  { L"Slovak", L"slo", L"sk", L"slk", MAKELCID( MAKELANGID(LANG_SLOVAK, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Slovenian", L"slv", L"sl", NULL, MAKELCID( MAKELANGID(LANG_SLOVENIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Sogdian", L"sog", NULL },
  { L"Somali", L"som", L"so" },
  { L"Songhai", L"son", NULL },
  { L"Soninke", L"snk", NULL },
  { L"Sorbian languages", L"wen", NULL },
  { L"Sotho, Northern", L"nso", NULL, NULL, MAKELCID( MAKELANGID(LANG_SOTHO, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Sotho, Southern", L"sot", L"st", NULL, MAKELCID( MAKELANGID(LANG_SOTHO, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"South American Indian (Other)", L"sai", NULL },
  { L"Southern Sami", L"sma", NULL },
  { L"South Ndebele", L"nbl", L"nr" },
  { L"Sukuma", L"suk", NULL },
  { L"Sumerian", L"sux", NULL },
  { L"Sundanese", L"sun", L"su" },
  { L"Susu", L"sus", NULL },
  { L"Swahili", L"swa", L"sw", NULL, MAKELCID( MAKELANGID(LANG_SWAHILI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Swati", L"ssw", L"ss" },
  { L"Swedish", L"swe", L"sv", NULL, MAKELCID( MAKELANGID(LANG_SWEDISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Syriac", L"syr", NULL, NULL, MAKELCID( MAKELANGID(LANG_SYRIAC, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tagalog", L"tgl", L"tl" },
  { L"Tahitian", L"tah", L"ty" },
  { L"Tai (Other)", L"tai", NULL },
  { L"Tajik", L"tgk", L"tg", NULL, MAKELCID( MAKELANGID(LANG_TAJIK, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tamashek", L"tmh", NULL },
  { L"Tamil", L"tam", L"ta", NULL, MAKELCID( MAKELANGID(LANG_TAMIL, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tatar", L"tat", L"tt", NULL, MAKELCID( MAKELANGID(LANG_TATAR, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Telugu", L"tel", L"te",  NULL, MAKELCID( MAKELANGID(LANG_TELUGU, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tereno", L"ter", NULL },
  { L"Tetum", L"tet", NULL },
  { L"Thai", L"tha", L"th", NULL, MAKELCID( MAKELANGID(LANG_THAI, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tibetan", L"tib", L"bo", L"bod", MAKELCID( MAKELANGID(LANG_TIBETAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tigre", L"tig", NULL },
  { L"Tigrinya", L"tir", L"ti", NULL, MAKELCID( MAKELANGID(LANG_TIGRIGNA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Timne", L"tem", NULL },
  { L"Tiv", L"tiv", NULL },
  { L"Tlingit", L"tli", NULL },
  { L"Tok Pisin", L"tpi", NULL },
  { L"Tokelau", L"tkl", NULL },
  { L"Tonga (Nyasa)", L"tog", NULL },
  { L"Tonga (Tonga Islands)", L"ton", L"to" },
  { L"Tsimshian", L"tsi", NULL },
  { L"Tsonga", L"tso", L"ts" },
  { L"Tswana", L"tsn", L"tn", NULL, MAKELCID( MAKELANGID(LANG_TSWANA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tumbuka", L"tum", NULL },
  { L"Tupi languages", L"tup", NULL },
  { L"Turkish", L"tur", L"tr", NULL, MAKELCID( MAKELANGID(LANG_TURKISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Turkish, Ottoman (1500-1928)", L"ota", NULL,	NULL, MAKELCID( MAKELANGID(LANG_TURKISH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Turkmen", L"tuk", L"tk", NULL, MAKELCID( MAKELANGID(LANG_TURKMEN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Tuvalu", L"tvl", NULL },
  { L"Tuvinian", L"tyv", NULL },
  { L"Twi", L"twi", L"tw" },
  { L"Ugaritic", L"uga", NULL },
  { L"Uighur", L"uig", L"ug", NULL, MAKELCID( MAKELANGID(LANG_UIGHUR, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Ukrainian", L"ukr", L"uk", NULL, MAKELCID( MAKELANGID(LANG_UKRAINIAN, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Umbundu", L"umb", NULL },
  { L"Undetermined", L"und", NULL },
  { L"Urdu", L"urd", L"ur", NULL, MAKELCID( MAKELANGID(LANG_URDU, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Uzbek", L"uzb", L"uz", NULL, MAKELCID( MAKELANGID(LANG_UZBEK, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Vai", L"vai", NULL },
  { L"Venda", L"ven", L"ve" },
  { L"Vietnamese", L"vie", L"vi", NULL, MAKELCID( MAKELANGID(LANG_VIETNAMESE, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Volapük", L"vol", L"vo" },
  { L"Votic", L"vot", NULL },
  { L"Wakashan languages", L"wak", NULL },
  { L"Walamo", L"wal", NULL },
  { L"Walloon", L"wln", L"wa" },
  { L"Waray", L"war", NULL },
  { L"Washo", L"was", NULL },
  { L"Welsh", L"wel", L"cy", L"cym", MAKELCID( MAKELANGID(LANG_WELSH, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Wolof", L"wol", L"wo", NULL, MAKELCID( MAKELANGID(LANG_WOLOF, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Xhosa", L"xho", L"xh", NULL, MAKELCID( MAKELANGID(LANG_XHOSA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Yakut", L"sah", NULL, NULL, MAKELCID( MAKELANGID(LANG_YAKUT, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Yao", L"yao", NULL },
  { L"Yapese", L"yap", NULL },
  { L"Yiddish", L"yid", L"yi" },
  { L"Yoruba", L"yor", L"yo", NULL, MAKELCID( MAKELANGID(LANG_YORUBA, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Yupik languages", L"ypk", NULL },
  { L"Zande", L"znd", NULL },
  { L"Zapotec", L"zap", NULL },
  { L"Zenaga", L"zen", NULL },
  { L"Zhuang; Chuang", L"zha", L"za" },
  { L"Zulu", L"zul", L"zu", NULL, MAKELCID( MAKELANGID(LANG_ZULU, SUBLANG_DEFAULT), SORT_DEFAULT) },
  { L"Zuni", L"zun", NULL },
  { L"Classical Newari", L"nwc", NULL },
  { L"Klingon", L"tlh", NULL },
  { L"Blin", L"byn", NULL },
  { L"Lojban", L"jbo", NULL },
  { L"Lower Sorbian", L"dsb", NULL },
  { L"Upper Sorbian", L"hsb", NULL },
  { L"Kashubian", L"csb", NULL },
  { L"Crimean Turkish", L"crh", NULL },
  { L"Erzya", L"myv", NULL },
  { L"Moksha", L"mdf", NULL },
  { L"Karachay-Balkar", L"krc", NULL },
  { L"Adyghe", L"ady", NULL },
  { L"Udmurt", L"udm", NULL },
  { L"Dargwa", L"dar", NULL },
  { L"Ingush", L"inh", NULL },
  { L"Nogai", L"nog", NULL },
  { L"Haitian", L"hat", L"ht" },
  { L"Kalmyk", L"xal", NULL },
  {NULL, NULL, NULL},
  {L"No subtitles", L"---", NULL, NULL, (LCID)LCID_NOSUBTITLES},
};

static struct
{
  unsigned int id;
  const wchar_t *name;
} niceCodecNames [] =	
{
  // Video
  { CODEC_ID_H264, L"h264" }, // XXX: Do not remove, required for custom profile/level formatting
  { CODEC_ID_VC1, L"vc-1" },  // XXX: Do not remove, required for custom profile/level formatting
  { CODEC_ID_MPEG2VIDEO, L"mpeg2" },
  // Audio
  { CODEC_ID_DTS, L"dts" },
  { CODEC_ID_AAC_LATM, L"aac (latm)" },
  // Subs
  { CODEC_ID_TEXT, L"txt" },
  { CODEC_ID_MOV_TEXT, L"tx3g" },
  { CODEC_ID_SRT, L"srt" },
  { CODEC_ID_HDMV_PGS_SUBTITLE, L"pgs" },
  { CODEC_ID_DVD_SUBTITLE, L"vobsub" },
  { CODEC_ID_DVB_SUBTITLE, L"dvbsub" },
  { CODEC_ID_SSA, L"ssa/ass" },
  { CODEC_ID_XSUB, L"xsub" }
};

static bool ShowSampleFmt(CodecID codecId)
{
  // PCM Codecs
  if ((codecId >= 0x10000) && (codecId < 0x12000))
  {
    return true;
  }

  // Lossless Codecs
  if ((codecId == CODEC_ID_MLP) ||
      (codecId == CODEC_ID_TRUEHD) ||
      (codecId == CODEC_ID_FLAC) ||
      (codecId == CODEC_ID_WMALOSSLESS) ||
      (codecId == CODEC_ID_WAVPACK) ||
      (codecId == CODEC_ID_MP4ALS) ||
      (codecId == CODEC_ID_ALAC))
  {
     return true;
  }

  return false;
}

int CDemuxerUtils::GetBitRate(AVCodecContext *codecContext)
{
  int bit_rate;
  int bits_per_sample;

  switch(codecContext->codec_type)
  {
  case AVMEDIA_TYPE_VIDEO:
  case AVMEDIA_TYPE_DATA:
  case AVMEDIA_TYPE_SUBTITLE:
  case AVMEDIA_TYPE_ATTACHMENT:
    bit_rate = codecContext->bit_rate;
    break;
  case AVMEDIA_TYPE_AUDIO:
    bits_per_sample = av_get_bits_per_sample(codecContext->codec_id);
    bit_rate = (codecContext->bit_rate != 0) ? codecContext->bit_rate : (codecContext->sample_rate * codecContext->channels * bits_per_sample);
    break;
  default:
    bit_rate = 0;
    break;
  }
  return bit_rate;
}

int CDemuxerUtils::GetBitsPerSample(AVCodecContext *codecContext, bool raw)
{
  int bits = av_get_bits_per_sample(codecContext->codec_id);

  if ((bits == 0) || raw)
  {
    bits = codecContext->bits_per_coded_sample;

    if((bits == 0) || raw)
    {
      if ((codecContext->sample_fmt == AV_SAMPLE_FMT_S32) && (codecContext->bits_per_raw_sample != 0))
      {
        bits = codecContext->bits_per_raw_sample;
      }
      else
      {
        bits = av_get_bits_per_sample_fmt(codecContext->sample_fmt);
      }
    }
  }

  return bits;
}

wchar_t *CDemuxerUtils::GetStreamLanguage(const AVStream *stream)
{
  wchar_t *result = NULL;

  const char *language = NULL;
  if (av_dict_get(stream->metadata, "language", NULL, 0) != NULL)
  {
    language = av_dict_get(stream->metadata, "language", NULL, 0)->value;
  }

  // don't bother with undetermined languages (fallback value in some containers)
  if ((language != NULL) && (strncmp(language, "und", 3) != 0))
  {
    result = ConvertToUnicodeA(language);
  }

  return result;
}

wchar_t *CDemuxerUtils::GetCodecName(AVCodecContext *codecContext)
{
  wchar_t *result = NULL;
  CodecID codecId = codecContext->codec_id;

  // grab the codec
  AVCodec *codec = avcodec_find_decoder(codecId);
  const char *profile = (codec != NULL) ? av_get_profile_name(codec, codecContext->profile) : NULL;

  // get nice codec name from codec ID
  const wchar_t *niceCodecName = NULL;
  for (unsigned int i = 0; i < countof(niceCodecNames); i++)
  {
    if (niceCodecNames[i].id == codecId)
    {
      niceCodecName = niceCodecNames[i].name;
      break;
    }
  }

  if ((codecId == CODEC_ID_DTS) && (codecContext->codec_tag == 0xA2))
  {
    profile = "DTS Express";
  }

  wchar_t *profileW = ConvertToUnicodeA(profile);
  wchar_t *lowerProfile = ToLowerW(profileW);

  if (codecId == CODEC_ID_H264)
  {
    result = FormatString(
      ((codecContext->level != 0) && (codecContext->level != FF_LEVEL_UNKNOWN) && (codecContext->level < 1000)) ?
      L"%s %s L%.1f" :
      L"%s %s",
      niceCodecName,
      (lowerProfile == NULL) ? L"NULL" : lowerProfile,
      codecContext->level / 10.0);
  }
  else if (codecId == CODEC_ID_VC1)
  {
    result = FormatString(
      (codecContext->level != FF_LEVEL_UNKNOWN) ?
      L"%s %s L%d" :
      L"%s %s",
      niceCodecName,
      (lowerProfile == NULL) ? L"NULL" : lowerProfile,
      codecContext->level);
  }
  else if (codecId == CODEC_ID_DTS)
  {
    result = Duplicate(lowerProfile);
  }
  else if (niceCodecName != NULL)
  {
    result = FormatString(
      (lowerProfile != NULL) ?
      L"%s %s" :
      L"%s",
      niceCodecName,
      lowerProfile);
  }
  else if ((codec != NULL) && (codec->name != NULL))
  {
    wchar_t *nameW = ConvertToUnicodeA(codec->name);

    if (nameW != NULL)
    {
      result = FormatString(
        (lowerProfile != NULL) ?
        L"%s %s" :
        L"%s",
        nameW,
        lowerProfile);
    }

    FREE_MEM(nameW);
  }
  else if (codecContext->codec_name[0] != '\0')
  {
    unsigned int length = sizeof(codecContext->codec_name) + 1;
    ALLOC_MEM_DEFINE_SET(buffer, char, length, 0);

    if (buffer != NULL)
    {
      memcpy(buffer, codecContext->codec_name, length - 1);
      result = ConvertToUnicodeA(buffer);
    }

    FREE_MEM(buffer);
  }
  else
  {
    /* output avi tags */

    unsigned int length = 32;
    ALLOC_MEM_DEFINE_SET(buffer, char, length, 0);

    if (buffer != NULL)
    {
      av_get_codec_tag_string(buffer, length, codecContext->codec_tag);

      wchar_t *bufferW = ConvertToUnicodeA(buffer);

      result = FormatString(L"%s / 0x%04X", (bufferW == NULL) ? L"NULL" : bufferW, codecContext->codec_tag);

      FREE_MEM(bufferW);
    }

    FREE_MEM(buffer);
  }

  FREE_MEM(profileW);
  FREE_MEM(lowerProfile);

  return result;
}

wchar_t *CDemuxerUtils::GetStreamDescription(AVStream *stream)
{
  AVCodecContext *codecContext = stream->codec;

  wchar_t *result = NULL;
  wchar_t *codecName = GetCodecName(codecContext);
  wchar_t *streamLanguage = GetStreamLanguage(stream);
  wchar_t *language = NULL;

  if (streamLanguage != NULL)
  {
    language = ProbeForLanguage(streamLanguage);

    CHECK_CONDITION_EXECUTE(IsNullOrEmpty(language), language = Duplicate(streamLanguage));
  }

  const char *titleA = NULL;
  if (av_dict_get(stream->metadata, "title", NULL, 0) != NULL)
  {
    titleA = av_dict_get(stream->metadata, "title", NULL, 0)->value;
  }
  else if (av_dict_get(stream->metadata, "handler_name", NULL, 0) != NULL)
  {
    titleA = av_dict_get(stream->metadata, "handler_name", NULL, 0)->value;

    if ((strcmp(titleA, "GPAC ISO Video Handler") == 0) || (strcmp(titleA, "VideoHandler") == 0) || (strcmp(titleA, "GPAC ISO Audio Handler") == 0) || (strcmp(titleA, "GPAC Streaming Text Handler") == 0))
    {
      titleA = NULL;
    }
  }

  // empty titles are rather useless
  if (IsNullOrEmptyA(titleA))
  {
    titleA = NULL;
  }

  wchar_t *title = ConvertToUnicodeA(titleA);

  int bitrate = GetBitRate(codecContext);

  switch(codecContext->codec_type)
  {
  case AVMEDIA_TYPE_VIDEO:
    {
      result = Duplicate(L"V: ");
      if ((title != NULL) && (streamLanguage != NULL))
      {
        result = AppendString(result, title);
        result = AppendString(result, L" [");
        result = AppendString(result, streamLanguage);
        result = AppendString(result, L"] (");
      }
      else if (title != NULL)
      {
        result = AppendString(result, title);
        result = AppendString(result, L" (");
      }
      else if (streamLanguage != NULL)
      {
        result = AppendString(result, (language == NULL) ? L"" : language);
        result = AppendString(result, L" [");
        result = AppendString(result, streamLanguage);
        result = AppendString(result, L"] (");
      }

      result = AppendString(result, (codecName == NULL) ? L"unknown codec" : codecName);

      if (codecContext->pix_fmt != PIX_FMT_NONE)
      {
        wchar_t *pixelFormatW = ConvertToUnicodeA(av_get_pix_fmt_name(codecContext->pix_fmt));
        if (pixelFormatW != NULL)
        {
          wchar_t *pixelFormat = FormatString(L", %s", pixelFormatW);
          CHECK_CONDITION_NOT_NULL_EXECUTE(pixelFormat, result = AppendString(result, pixelFormat));
          FREE_MEM(pixelFormat);
        }
        FREE_MEM(pixelFormatW);
      }

      if (codecContext->width != 0)
      {
        wchar_t *widthHeight = FormatString(L", %dx%d", codecContext->width, codecContext->height);
        CHECK_CONDITION_NOT_NULL_EXECUTE(widthHeight, result = AppendString(result, widthHeight));
        FREE_MEM(widthHeight);
      }

      if (bitrate > 0)
      {
        wchar_t *bitrateW = FormatString(L", %d kb/s", bitrate / 1000);
        CHECK_CONDITION_NOT_NULL_EXECUTE(bitrateW, result = AppendString(result, bitrateW));
        FREE_MEM(bitrateW);
      }

      if ((title != NULL) || (streamLanguage != NULL))
      {
        result = AppendString(result, L")");
      }

      wchar_t *formatFlags = CDemuxerUtils::GetFormatFlags(stream->disposition);
      CHECK_CONDITION_NOT_NULL_EXECUTE(formatFlags, result = AppendString(result, formatFlags));
      FREE_MEM(formatFlags);
    }
    break;
  case AVMEDIA_TYPE_AUDIO:
    {
      result = Duplicate(L"A: ");
      if ((title != NULL) && (streamLanguage != NULL))
      {
        result = AppendString(result, title);
        result = AppendString(result, L" [");
        result = AppendString(result, streamLanguage);
        result = AppendString(result, L"] (");
      }
      else if (title != NULL)
      {
        result = AppendString(result, title);
        result = AppendString(result, L" (");
      }
      else if (streamLanguage != NULL)
      {
        result = AppendString(result, (language == NULL) ? L"" : language);
        result = AppendString(result, L" [");
        result = AppendString(result, streamLanguage);
        result = AppendString(result, L"] (");
      }

      result = AppendString(result, (codecName == NULL) ? L"unknown codec" : codecName);

      if (codecContext->sample_rate != 0)
      {
        wchar_t *sampleRateW = FormatString(L", %d Hz", codecContext->sample_rate);
        CHECK_CONDITION_NOT_NULL_EXECUTE(sampleRateW, result = AppendString(result, sampleRateW));
        FREE_MEM(sampleRateW);
      }

      if (codecContext->channels != 0)
      {
        // get channel layout
        unsigned int length = 32;
        ALLOC_MEM_DEFINE_SET(buffer, char, length, 0);

        if (buffer != NULL)
        {
          av_get_channel_layout_string(buffer, length, codecContext->channels, codecContext->channel_layout);

          wchar_t *channels = ConvertToUnicodeA(buffer);
          CHECK_CONDITION_NOT_NULL_EXECUTE(channels, result = AppendString(result, L", "));
          CHECK_CONDITION_NOT_NULL_EXECUTE(channels, result = AppendString(result, channels));
          FREE_MEM(channels);
        }

        FREE_MEM(buffer);
      }

      if (ShowSampleFmt(codecContext->codec_id) && (CDemuxerUtils::GetBitsPerSample(codecContext, true) != 0))
      {
        if ((codecContext->sample_fmt == AV_SAMPLE_FMT_FLT) || (codecContext->sample_fmt == AV_SAMPLE_FMT_DBL))
        {
          result = AppendString(result, L", fp");
        }
        else
        {
          result = AppendString(result, L", s");
        }

        wchar_t *bitsPerSample = FormatString(L"%d", CDemuxerUtils::GetBitsPerSample(codecContext, true));
        CHECK_CONDITION_NOT_NULL_EXECUTE(bitsPerSample, result = AppendString(result, bitsPerSample));
        FREE_MEM(bitsPerSample);
      }

      if (bitrate > 0)
      {
        wchar_t *bitrateW = FormatString(L", %d kb/s", bitrate / 1000);
        CHECK_CONDITION_NOT_NULL_EXECUTE(bitrateW, result = AppendString(result, bitrateW));
        FREE_MEM(bitrateW);
      }

      if ((title != NULL) || (streamLanguage != NULL))
      {
        result = AppendString(result, L")");
      }

      wchar_t *formatFlags = CDemuxerUtils::GetFormatFlags(stream->disposition);
      CHECK_CONDITION_NOT_NULL_EXECUTE(formatFlags, result = AppendString(result, formatFlags));
      FREE_MEM(formatFlags);
    }
    break;
  case AVMEDIA_TYPE_SUBTITLE:
    {
      result = Duplicate(L"S: ");
      if ((title != NULL) && (streamLanguage != NULL))
      {
        result = AppendString(result, title);
        result = AppendString(result, L" [");
        result = AppendString(result, streamLanguage);
        result = AppendString(result, L"] (");
      }
      else if (title != NULL)
      {
        result = AppendString(result, title);
        result = AppendString(result, L" (");
      }
      else if (streamLanguage != NULL)
      {
        result = AppendString(result, (language == NULL) ? L"" : language);
        result = AppendString(result, L" [");
        result = AppendString(result, streamLanguage);
        result = AppendString(result, L"] (");
      }

      result = AppendString(result, (codecName == NULL) ? L"unknown codec" : codecName);

      if ((title != NULL) || (streamLanguage != NULL))
      {
        result = AppendString(result, L")");
      }

      wchar_t *formatFlags = CDemuxerUtils::GetFormatFlags(stream->disposition);
      CHECK_CONDITION_NOT_NULL_EXECUTE(formatFlags, result = AppendString(result, formatFlags));
      FREE_MEM(formatFlags);
    }
    break;
  default:
    result = FormatString(L"Unknown: Stream #%d", stream->index);
    break;
  }

  FREE_MEM(codecName);
  FREE_MEM(streamLanguage);
  FREE_MEM(language);
  FREE_MEM(title);

  return result;
}

wchar_t *CDemuxerUtils::GetFormatFlags(int flags)
{
  wchar_t *result = NULL;

  if (flags & (AV_DISPOSITION_FORCED | AV_DISPOSITION_DEFAULT | AV_DISPOSITION_HEARING_IMPAIRED | AV_DISPOSITION_VISUAL_IMPAIRED | AV_DISPOSITION_SUB_STREAM | AV_DISPOSITION_SECONDARY_AUDIO))
  {
    result = Duplicate(L" [");

    bool first = true;

    if (flags & AV_DISPOSITION_DEFAULT)
    {
      result = AppendString(result, first ? L"default" : L", default");
      first = false;
    }

    if (flags & AV_DISPOSITION_FORCED)
    {
      result = AppendString(result, first ? L"forced" : L", forced");
      first = false;
    }

    if (flags & AV_DISPOSITION_HEARING_IMPAIRED)
    {
      result = AppendString(result, first ? L"hearing impaired" : L", hearing impaired");
      first = false;
    }

    if (flags & AV_DISPOSITION_VISUAL_IMPAIRED)
    {
      result = AppendString(result, first ? L"visual impaired" : L", visual impaired");
      first = false;
    }

    if (flags & AV_DISPOSITION_SUB_STREAM)
    {
      result = AppendString(result, first ? L"sub" : L", sub");
      first = false;
    }

    if (flags & AV_DISPOSITION_SECONDARY_AUDIO)
    {
      result = AppendString(result, first ? L"secondary" : L", secondary");
      first = false;
    }

    result = AppendString(result, L"]");
  }

  return result;
}

/* language methods */

wchar_t *CDemuxerUtils::ISO6391ToLanguage(const wchar_t *code)
{
  wchar_t *result = NULL;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {
    if ((isoLanguages[i].iso6391 != NULL) && (_wcsnicmp(isoLanguages[i].iso6391, code, 2) == 0))
    {
      int nameLength = wcslen(isoLanguages[i].name);
      int index = IndexOf(isoLanguages[i].name, nameLength, L";", 1);
      result = Substring(isoLanguages[i].name, 0, (index >= 0) ? i : nameLength);
      break;
    }
  }

  return result;
}

wchar_t *CDemuxerUtils::ISO6392ToLanguage(const wchar_t *code)
{
  wchar_t *result = NULL;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {

    if (((isoLanguages[i].iso6392 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392, code, 3) == 0)) ||
        ((isoLanguages[i].iso6392_2 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392_2, code, 3) == 0)))
    {
      int nameLength = wcslen(isoLanguages[i].name);
      int index = IndexOf(isoLanguages[i].name, nameLength, L";", 1);
      result = Substring(isoLanguages[i].name, 0, (index >= 0) ? i : nameLength);
      break;
    }
  }

  return result;
}

wchar_t *CDemuxerUtils::ProbeForLanguage(const wchar_t *code)
{
  unsigned int length = (code == NULL) ? 0 : wcslen(code);
  if (length == 3)
  {
    return ISO6392ToLanguage(code);
  }
  else if (length >= 2)
  {
    return ISO6391ToLanguage(code);
  }

  return NULL;
}

const wchar_t *CDemuxerUtils::ISO6392Check(const wchar_t *code)
{
  const wchar_t *result = NULL;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {

    if (((isoLanguages[i].iso6392 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392, code, 3) == 0)) ||
        ((isoLanguages[i].iso6392_2 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392_2, code, 3) == 0)))
    {
      result = isoLanguages[i].iso6392;
      break;
    }
  }

  return result;
}

const wchar_t *CDemuxerUtils::LanguageToISO6392(const wchar_t *language)
{
  const wchar_t *result = NULL;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {
    if ((isoLanguages[i].name != NULL) && (_wcsicmp(isoLanguages[i].name, language) == 0))
    {
      result = isoLanguages[i].iso6392;
      break;
    }
  }

  return result;
}

const wchar_t *CDemuxerUtils::ProbeForISO6392(const wchar_t *language)
{
  unsigned int length = (language == NULL) ? 0 : wcslen(language);

  if (length == 2)
  {
    return ISO6391To6392(language);
  }
  else if (length == 3)
  {
    return ISO6392Check(language);
  }
  else if (length > 3)
  {
    return LanguageToISO6392(language);
  }

  return NULL;
}

LCID CDemuxerUtils::ISO6391ToLcid(const wchar_t *code)
{
  LCID result = 0;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {
    if ((isoLanguages[i].iso6391 != NULL) && (_wcsnicmp(isoLanguages[i].iso6391, code, 2) == 0))
    {
      result = isoLanguages[i].lcid;
      break;
    }
  }

  return result;
}

LCID CDemuxerUtils::ISO6392ToLcid(const wchar_t *code)
{
  LCID result = 0;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {
    if (((isoLanguages[i].iso6392 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392, code, 3) == 0)) ||
        ((isoLanguages[i].iso6392_2 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392_2, code, 3) == 0)))
    {
      result = isoLanguages[i].lcid;
      break;
    }
  }

  return result;
}

const wchar_t *CDemuxerUtils::ISO6391To6392(const wchar_t *code)
{
  const wchar_t *result = NULL;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {
    if ((isoLanguages[i].iso6391 != NULL) && (_wcsnicmp(isoLanguages[i].iso6391, code, 2) == 0))
    {
      result = isoLanguages[i].iso6392;
      break;
    }
  }

  return result;
}

const wchar_t *CDemuxerUtils::ISO6392To6391(const wchar_t *code)
{
  const wchar_t *result = NULL;

  for (unsigned int i = 0; i < countof(isoLanguages); i++)
  {
    if (((isoLanguages[i].iso6392 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392, code, 3) == 0)) ||
        ((isoLanguages[i].iso6392_2 != NULL) && (_wcsnicmp(isoLanguages[i].iso6392_2, code, 3) == 0)))
    {
      result = isoLanguages[i].iso6391;
      break;
    }
  }

  return result;
}

LCID CDemuxerUtils::ProbeForLCID(const wchar_t *code)
{
  unsigned int length = (code == NULL) ? 0 : wcslen(code);

  if (length == 3)
  {
    return ISO6392ToLcid(code);
  }
  else if (length >= 2)
  {
    return ISO6391ToLcid(code);
  }

  return 0;
}
