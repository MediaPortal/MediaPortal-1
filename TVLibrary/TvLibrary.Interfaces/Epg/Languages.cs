/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Epg
{
  /// <summary>
  /// Class which holds all dvb epg languages
  /// </summary>
  public class Languages
  {
    #region languages
    static string[] m_langCodes = new string[]{
													"abk","ace","ach","ada","aar",
													"afh","afr","afa","aka","akk",
													"alb","sqi","ale","alg","tut",
													"amh","apa","ara","arc","arp",
													"arn","arw","arm","hye","art",
													"asm","ath","map","ava","ave",
													"awa","aym","aze","nah","ban",
													"bat","bal","bam","bai","bad",
													"bnt","bas","bak","baq","eus",
													"bej","bem","ben","ber","bho",
													"bih","bik","bin","bis","bra",
													"bre","bug","bul","bua","bur",
													"mya","bel","cad","car","cat",
													"cau","ceb","cel","cai","chg",
													"cha","che","chr","chy","chb",
													"chi","zho","chn","cho","chu",
													"chv","cop","cor","cos","cre",
													"mus","crp","cpe","cpf","cpp",
													"cus","ces","cze","dak","dan",
													"del","din","div","doi","dra",
													"dua","dut","nla","dum","dyu",
													"dzo","efi","egy","eka","elx",
													"eng","enm","ang","esk","epo",
													"est","ewe","ewo","fan","fat",
													"fao","fij","fin","fiu","fon",
													"fra","fre","frm","fro","fry",
													"ful","gaa","gae","gdh","glg",
													"lug","gay","gez","geo","kat",
													"deu","ger","gmh","goh","gem",
													"gil","gon","got","grb","grc",
													"ell","gre","kal","grn","guj",
													"hai","hau","haw","heb","her",
													"hil","him","hin","hmo","hun",
													"hup","iba","ice","isl","ibo",
													"ijo","ilo","inc","ine","ind",
													"ina","ine","iku","ipk","ira",
													"gai","iri","sga","mga","iro",
													"ita","jpn","jav","jaw","jrb",
													"jpr","kab","kac","kam","kan",
													"kau","kaa","kar","kas","kaw",
													"kaz","kha","khm","khi","kho",
													"kik","kin","kir","kom","kon",
													"kok","kor","kpe","kro","kua",
													"kum","kur","kru","kus","kut",
													"lad","lah","lam","oci","lao",
													"lat","lav","ltz","lez","lin",
													"lit","loz","lub","lui","lun",
													"luo","mac","mak","mad","mag",
													"mai","mak","mlg","may","msa",
													"mal","mlt","man","mni","mno",
													"max","mao","mri","mar","chm",
													"mah","mwr","mas","myn","men",
													"mic","min","mis","moh","mol",
													"mkh","lol","mon","mos","mul",
													"mun","nau","nav","nde","nbl",
													"ndo","nep","new","nic","ssa",
													"niu","non","nai","nor","nno",
													"nub","nym","nya","nyn","nyo",
													"nzi","oji","ori","orm","osa",
													"oss","oto","pal","pau","pli",
													"pam","pag","pan","pap","paa",
													"fas","per","peo","phn","pol",
													"pon","por","pra","pro","pus",
													"que","roh","raj","rar","roa",
													"ron","rum","rom","run","rus",
													"sal","sam","smi","smo","sad",
													"sag","san","srd","sco","sel",
													"sem","scr","srr","shn","sna",
													"sid","bla","snd","sin","sit",
													"sio","sla","ssw","slk","slo",
													"slv","sog","som","son","wen",
													"nso","sot","sai","esl","spa",
													"suk","sux","sun","sus","swa",
													"ssw","sve","swe","syr","tgl",
													"tah","tgk","tmh","tam","tat",
													"tel","ter","tha","bod","tib",
													"tig","tir","tem","tiv","tli",
													"tog","ton","tru","tsi","tso",
													"tsn","tum","tur","ota","tuk",
													"tyv","twi","uga","uig","ukr",
													"umb","und","urd","uzb","vai",
													"ven","vie","vol","vot","wak",
													"wal","war","was","cym","wel",
													"wol","xho","sah","yao","yap",
													"yid","yor","zap","zen","zha","zul"};
    static string[] m_langLanguage = new string[]{
													   "Abkhazian","Achinese","Acoli","Adangme","Afar",
													   "Afrihili","Afrikaans","Afro-Asiatic","Akan",
													   "Akkadian","Albanian","Albanian","Aleut","Algonquian",
													   "Altaic","Amharic","Apache","Arabic","Aramaic",
													   "Arapaho","Araucanian","Arawak","Armenian","Armenian",
													   "Artificial","Assamese","Athapascan","Austronesian",
													   "Avaric","Avestan","Awadhi","Aymara","Azerbaijani",
													   "Aztec","Balinese","Baltic","Baluchi","Bambara",
													   "Bamileke","Banda","Bantu","Basa","Bashkir","Basque",
													   "Basque","Beja","Bemba","Bengali","Berber","Bhojpuri",
													   "Bihari","Bikol","Bini","Bislama","Braj","Breton",
													   "Buginese","Bulgarian","Buriat","Burmese","Burmese",
													   "Byelorussian","Caddo","Carib","Catalan","Caucasian",
													   "Cebuano","Celtic","Central-American(Indian)","Chagatai",
													   "Chamorro","Chechen","Cherokee","Cheyenne","Chibcha",
													   "Chinese","Chinese","Chinook","Choctaw","Church","Chuvash",
													   "Coptic","Cornish","Corsican","Cree","Creek",
													   "Creoles(Pidgins)","Creoles(Pidgins)","Creoles(Pidgins)",
													   "Creoles(Pidgins)","Cushitic","Czech","Czech","Dakota",
													   "Danish","Delaware","Dinka","Divehi","Dogri","Dravidian",
													   "Duala","Dutch","Dutch","Dutch-Middle","Dyula","Dzongkha",
													   "Efik","Egyptian","Ekajuk","Elamite","English",
													   "English-Middle","English-Old","Eskimo","Esperanto",
													   "Estonian","Ewe","Ewondo","Fang","Fanti","Faroese",
													   "Fijian","Finnish","Finno-Ugrian","Fon","French",
													   "French","French-Middle","French-Old","Frisian",
													   "Fulah","Ga","Gaelic","Gaelic","Gallegan","Ganda",
													   "Gayo","Geez","Georgian","Georgian","German","German",
													   "German-Middle","German-Old","Germanic","Gilbertese",
													   "Gondi","Gothic","Grebo","Greek-Ancient","Greek",
													   "Greek","Greenlandic","Guarani","Gujarati","Haida",
													   "Hausa","Hawaiian","Hebrew","Herero","Hiligaynon",
													   "Himachali","Hindi","Hiri","Hungarian","Hupa","Iban",
													   "Icelandic","Icelandic","Igbo","Ijo","Iloko","Indic",
													   "Indo-European","Indonesian","Interlingua","Interlingue",
													   "Inuktitut","Inupiak","Iranian","Irish","Irish",
													   "Irish-Old","Irish-Middle","Iroquoian","Italian",
													   "Japanese","Javanese","Javanese","Judeo-Arabic",
													   "Judeo-Persian","Kabyle","Kachin","Kamba","Kannada",
													   "Kanuri","Kara-Kalpak","Karen","Kashmiri","Kawi",
													   "Kazakh","Khasi","Khmer","Khoisan","Khotanese","Kikuyu",
													   "Kinyarwanda","Kirghiz","Komi","Kongo","Konkani",
													   "Korean","Kpelle","Kru","Kuanyama","Kumyk","Kurdish",
													   "Kurukh","Kusaie","Kutenai","Ladino","Lahnda","Lamba",
													   "Langue","Lao","Latin","Latvian","Letzeburgesch",
													   "Lezghian","Lingala","Lithuanian","Lozi","Luba-Katanga",
													   "Luiseno","Lunda","Luo","Macedonian","Macedonian",
													   "Madurese","Magahi","Maithili","Makasar","Malagasy",
													   "Malay","Malay","Malayalam","Maltese","Mandingo",
													   "Manipuri","Manobo","Manx","Maori","Maori","Marathi",
													   "Mari","Marshall","Marwari","Masai","Mayan","Mende",
													   "Micmac","Minangkabau","Miscellaneous","Mohawk",
													   "Moldavian","Mon-Kmer","Mongo","Mongolian","Mossi",
													   "Multiple","Munda","Nauru","Navajo","Ndebele-North",
													   "Ndebele-South","Ndongo","Nepali","Newari",
													   "Niger-Kordofanian","Nilo-Saharan","Niuean",
													   "Norse-Old","North-American(Indian)","Norwegian",
													   "Norwegian","Nubian","Nyamwezi","Nyanja","Nyankole",
													   "Nyoro","Nzima","Ojibwa","Oriya","Oromo","Osage",
													   "Ossetic","Otomian","Pahlavi","Palauan","Pali",
													   "Pampanga","Pangasinan","Panjabi","Papiamento",
													   "Papuan-Australian","Persian","Persian","Persian-Old",
													   "Phoenician","Polish","Ponape","Portuguese","Prakrit",
													   "Provencal-Old","Pushto","Quechua","Rhaeto-Romance",
													   "Rajasthani","Rarotongan","Romance","Romanian","Romanian",
													   "Romany","Rundi","Russian","Salishan","Samaritan(Aramaic)",
													   "Sami","Samoan","Sandawe","Sango","Sanskrit","Sardinian",
													   "Scots","Selkup","Semitic","Serbo-Croatian","Serer","Shan",
													   "Shona","Sidamo","Siksika","Sindhi","Singhalese",
													   "Sino-Tibetan","Siouan","Slavic","Siswant","Slovak",
													   "Slovak","Slovenian","Sogdian","Somali","Songhai","Sorbian",
													   "Sotho-Northern","Sotho-Southern","South-American(Indian)",
													   "Spanish","Spanish","Sukuma","Sumerian","Sudanese","Susu",
													   "Swahili","Swazi","Swedish","Swedish","Syriac","Tagalog",
													   "Tahitian","Tajik","Tamashek","Tamil","Tatar","Telugu",
													   "Tereno","Thai","Tibetan","Tibetan","Tigre","Tigrinya",
													   "Timne","Tivi","Tlingit","Tonga","Tonga(Tonga-Islands)",
													   "Truk","Tsimshian","Tsonga","Tswana","Tumbuka","Turkish",
													   "Turkish-Ottoman","Turkmen","Tuvinian","Twi","Ugaritic",
													   "Uighur","Ukrainian","Umbundu","Undetermined","Urdu",
													   "Uzbek","Vai","Venda","Vietnamese","Volapük","Votic",
													   "Wakashan","Walamo","Waray","Washo","Welsh","Welsh",
													   "Wolof","Xhosa","Yakut","Yao","Yap","Yiddish","Yoruba",
													   "Zapotec","Zenaga","Zhuang","Zulu"};

    #endregion

    /// <summary>
    /// Gets the languages.
    /// </summary>
    /// <returns>list of all languages</returns>
    public List<String> GetLanguages()
    {
      List<String> langs = new List<String>();
      foreach (string str in m_langLanguage)
        langs.Add(str);
      return langs;
    }

    /// <summary>
    /// Gets the language codes.
    /// </summary>
    /// <returns>list of all language codes</returns>
    public List<String> GetLanguageCodes()
    {
      List<String> langs = new List<String>();
      foreach (string str in m_langCodes)
        langs.Add(str);
      return langs;
    }

    /// <summary>
    /// Gets the language from a language code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>language</returns>
    public static string GetLanguageFromCode(string code)
    {
      int n = 0;
      if (code == null)
        return "";
      if (code == "")
        return "";
      if (code.Length > 3)
        return code;
      foreach (string langCode in m_langCodes)
      {
        if (langCode.Equals(code))
          return m_langLanguage.GetValue(n).ToString();
        n++;
      }
      return code;
    }
  }
}
