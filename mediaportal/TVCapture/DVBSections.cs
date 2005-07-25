using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using System.Threading;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für DVB_SECTIONS.
	/// </summary>
	public class DVBSections : IDisposable
	{
		#region imports

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionPtr(int section,ref IntPtr dataPointer,ref int len,ref int header,ref int tableExtId,ref int version,ref int secNum,ref int lastSecNum);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool ReleaseSectionsBuffer();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid,int tid,ref int sectionCount,int tableSection,int timeout);
		// globals
		#endregion

		#region variables
		TPList[]							transp;
		static ArrayList							m_sectionsList;	
		int									m_diseqc=0;
		int									m_lnb0=0;
		int									m_lnb1=0;
		int									m_lnbsw=0;
		int									m_lnbkhz=0;
		int									m_lnbfreq=0;
		int									m_selKhz=0;
		int									m_timeoutMS=1000; // the timeout in milliseconds
		// two skystar2 specific globals
		bool								m_setPid=false;
		DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 m_dataControl=null;
		System.Timers.Timer					m_eitTimeoutTimer=null;
		bool								m_breakAction=false;
		object								m_dataCtrl=null;
		static DVBDemuxer					m_streamDemuxer;
		bool								m_syncWait=false;
		bool                m_getTablesUsingMS=false;
		static object						m_syncRelease=false;

		static string[] m_langCodes=new string[]{
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
		static string[] m_langLanguage=new string[]{
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

		#region Helper Methods
		//

		//
		public DVBDemuxer DemuxerObject
		{
			set
			{
				if(value!=null)
				{
					m_streamDemuxer=value;
					m_streamDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);
				}
			}
		}
		public DVBSections()
		{
			m_sectionsList=new ArrayList();	
			transp=new TPList[200];
			m_eitTimeoutTimer=new System.Timers.Timer(3000);
			m_eitTimeoutTimer.Elapsed+=new System.Timers.ElapsedEventHandler(m_eitTimeoutTimer_Elapsed);
		}
		public bool GetTablesUsingMicrosoft
		{
			get { return m_getTablesUsingMS;}
			set { m_getTablesUsingMS=value;}
		}
		//
		public int Timeout
		{
			get{return m_timeoutMS;}
			set{m_timeoutMS=value;}
		}

		#region tables
		// tables

		public struct CaPMT
		{
			public int CAPmt_Listmanagement;
			public int ProgramNumber;
			public int reserved0;
			public int VersionNumber;
			public int CurrentNextIndicator;
			public int reserved1;
			public int ProgramInfoLength;
			public int CAPmt_CommandID_PRG;
			public ArrayList CADescriptors_PRG;
			public int StreamType;
			public int reserved2;
			public int ElementaryStreamPID;
			public int reserved3;
			public int ElementaryStreamInfoLength;
			public int CAPmt_CommandID_ES;
			public ArrayList CADescriptors_ES;


		}
		public struct EITDescr
		{
			public int		version;
			public int		event_id;
			public string	genere_text;
			public string	event_item;
			public string	event_item_text;
			public string	event_name;
			public string	event_text;
			public int		starttime_y;
			public int		starttime_m;
			public int		starttime_d;
			public int		starttime_hh;
			public int		starttime_mm;
			public int		starttime_ss;
			public int		duration_hh;
			public int		duration_mm;
			public int		duration_ss;
			public int		program_number;
			public int		ts_id;
			public int		org_network_id;
			public bool		handled;
			public int		section;
			public int		lastSection;
			public int		table;
			public int		lastTable;
			public string	eeLanguageCode;
			public string	seLanguageCode;
			public bool		extendedEventUseable;
			public bool		extendedEventComplete;
			public bool		shortEventUseable;
			public bool		shortEventComplete;
			public DateTime mhwStartTime;
			public bool		isMHWEvent;
		}
		public struct EIT_Program_Info
		{
			public ArrayList eitList;
			public int program_id;
			public bool scrambled;
			public int running_status;
		}
		// defines
		public struct TPList
		{
			public int TPfreq; // frequency
			public int TPpol; // polarisation 0=hori, 1=vert
			public int TPsymb; // symbol rate
		}
		public struct AudioLanguage
		{
			public int		AudioPid;
			public string	AudioLanguageCode;
		}
		//
		//
		public struct Transponder
		{
			public ArrayList PMTTable;
			public ArrayList channels;
		}

		//
		//
		public struct ServiceData
		{
			public string serviceProviderName;
			public string serviceName;
			public int serviceType;
		}
		//
		// nit structs
		public struct NITSatDescriptor
		{
			public int Frequency;
			public float OrbitalPosition;
			public int WestEastFlag;
			public int Polarisation;
			public int Modulation;
			public int Symbolrate;
			public int FECInner;
			public string NetworkName;
		}
		//
		public struct NITCableDescriptor
		{
			public int Frequency;
			public int FECOuter;
			public int Modulation;
			public int Symbolrate;
			public int FECInner;
			public string NetworkName;
		}
		//
		public struct NITTerrestrialDescriptor
		{
			public int CentreFrequency;
			public int Bandwidth;
			public int Constellation;
			public int HierarchyInformation;
			public int CoderateHPStream;
			public int CoderateLPStream;
			public int GuardInterval;
			public int TransmissionMode; 
			public int OtherFrequencyFlag;
			public string NetworkName;
		}

		public struct DVBNetworkInfo
		{
			public ArrayList	NITDescriptorList;
			public string		NetworkName;
		}

		//
		//
		public struct ChannelInfo
		{
			public int			program_number;
			public int			reserved;
			public int			network_pmt_PID;
			public int			transportStreamID;
			public string		service_provider_name;
			public string		service_name;
			public int			serviceType;
			public bool			eitSchedule;
			public bool			eitPreFollow;
			public bool			scrambled;
			public int			freq;// 12188
			public int			symb;// 27500
			public int			fec;// 6
			public int			diseqc;// 1
			public int			lnb01;// 10600
			public int			lnbkhz;// 1 = 22
			public int			pol; // 0 - h
			public int			pcr_pid;
			public ArrayList	pid_list;
			public int			serviceID;
			public int			networkID;
			public string		pidCache;
			public int      minorChannel;
			public int      majorChannel;
			public int      modulation;
			public CaPMT	caPMT;
		}
		//
		//
		public struct PMTData
		{
			public int		stream_type;
			public int		reserved_1;
			public int		elementary_PID;
			public int		reserved_2;
			public int		ES_info_length;
			public string	data;
			public bool		isAC3Audio;
			public bool		isAudio;
			public bool		isVideo;
			public bool		isTeletext;
			public bool		isDVBSubtitle;
			public string	teletextLANG;
		}
		#endregion
		//
		//
		public ChannelInfo GetChannelInfo(IntPtr data)
		{
			byte[] da=new byte[600];
			Marshal.Copy(data,da,0,570);
			ChannelInfo ch=new ChannelInfo();
			ch.transportStreamID=Marshal.ReadInt32(data,0);
			ch.program_number=Marshal.ReadInt32(data,4);
			ch.network_pmt_PID=Marshal.ReadInt32(data,8);
			ch.pcr_pid=Marshal.ReadInt32(data,12);
			ch.serviceID=ch.program_number;
			ch.pid_list=new ArrayList();
			PMTData pmt=new PMTData();
			// video
			pmt.elementary_PID=Marshal.ReadInt16(data,16);
			pmt.isVideo=true;
			pmt.stream_type=1;
			pmt.data="";
			ch.pid_list.Add(pmt);
			pmt=new PMTData();			
			// audio 1
			pmt.elementary_PID=Marshal.ReadInt16(data,18);
			pmt.isAudio=true;
			pmt.stream_type=3;
			pmt.data=""+(char)Marshal.ReadByte(data,20)+(char)Marshal.ReadByte(data,21)+(char)Marshal.ReadByte(data,22);
			ch.pid_list.Add(pmt);
			pmt=new PMTData();			
			// audio 2
			pmt.elementary_PID=Marshal.ReadInt16(data,24);
			pmt.isAudio=true;
			pmt.stream_type=3;
			pmt.data=""+(char)Marshal.ReadByte(data,26)+(char)Marshal.ReadByte(data,27)+(char)Marshal.ReadByte(data,28);
			ch.pid_list.Add(pmt);
			pmt=new PMTData();			
			// audio 3
			pmt.elementary_PID=Marshal.ReadInt16(data,30);
			pmt.isAudio=true;
			pmt.stream_type=3;
			pmt.data=""+(char)Marshal.ReadByte(data,32)+(char)Marshal.ReadByte(data,33)+(char)Marshal.ReadByte(data,34);
			ch.pid_list.Add(pmt);
			pmt=new PMTData();			
			// ac3
			pmt.elementary_PID=Marshal.ReadInt16(data,36);
			pmt.isAC3Audio=true;
			pmt.stream_type=0;
			pmt.data="";
			ch.pid_list.Add(pmt);
			pmt=new PMTData();			
			// teletext
			pmt.elementary_PID=Marshal.ReadInt16(data,38);
			pmt.isTeletext=true;
			pmt.stream_type=0;
			pmt.data="";
			ch.pid_list.Add(pmt);
			pmt=new PMTData();			
			// sub
			pmt.elementary_PID=Marshal.ReadInt16(data,40);
			pmt.isDVBSubtitle=true;
			pmt.stream_type=0;
			pmt.data="";
			ch.pid_list.Add(pmt);
			pmt=new PMTData();			
			// 48 -
			/*
		ULONG TransportStreamID;// 0-3
		ULONG ProgrammNumber;// 4-7
		ULONG ProgrammPMTPID;// 8-11
		ULONG PCRPid;// 12-15
		WORD VideoPid;//16-17
		WORD AudioPid1;//18-19
		char Lang1[3];//20-22
		WORD AudioPid2;//23-24
		char Lang2[3];//25-27
        WORD AudioPid3;//28-29
		char Lang3[3];//30-32
		WORD AC3;//33-34
		WORD Teletext;//35-36
		WORD Subtitles;//37-38
		BYTE ServiceName[255];// 39-293
		BYTE ProviderName[255];// 294-548
		WORD EITPreFollow;// 549-550
		WORD EITSchedule;// 551-552
		WORD Scrambled;// 553-554
		WORD ServiceType;// 555-556
		BYTE PMTReady;// 557-557
		BYTE SDTReady;// 558-558			
		*/
			byte[] d=new byte[255];
			Marshal.Copy((IntPtr)(((int)data)+42),d,0,255);
			ch.service_name=System.Text.Encoding.ASCII.GetString(d);
			Marshal.Copy((IntPtr)(((int)data)+297),d,0,255);
			ch.service_provider_name=System.Text.Encoding.ASCII.GetString(d);
			ch.eitPreFollow=(Marshal.ReadInt16(data,552))==1?true:false;
			ch.eitSchedule=(Marshal.ReadInt16(data,554))==1?true:false;
			ch.scrambled=(Marshal.ReadInt16(data,556))==1?true:false;
			ch.serviceType=Marshal.ReadInt16(data,558);
			ch.networkID=Marshal.ReadInt32(data,560);

			RemoveInvalidChars(ref ch.service_name);
			RemoveInvalidChars(ref ch.service_provider_name);
			return ch;
		}
		void RemoveInvalidChars(ref string strTxt)
		{
			if (strTxt==null) 
			{
				strTxt=String.Empty;
				return;
			}
			if (strTxt.Length==0) 
			{
				strTxt=String.Empty;
				return;
			}
			string strReturn = String.Empty;
			for (int i = 0; i < (int)strTxt.Length; ++i)
			{
				char k = strTxt[i];
				if (k == '\'') 
				{
					strReturn += "'";
				}
				if((byte)k==0)// remove 0-bytes from the string
					k=(char)32;

				strReturn += k;
			}
			strReturn=strReturn.Trim();
			strTxt = strReturn;
		}
		public bool SetPidsForTechnisat
		{
			get{return m_setPid;}
			set{m_setPid=value;}
		}
		public object DataControl
		{
			get{return m_dataCtrl;}
			set{m_dataCtrl=value;}
		}
		//
		//
		public void SetLNBParams(int diseqc,int lnb0,int lnb1,int lnbsw,int lnbkhz,int selKhz,int lnbfreq)
		{
			m_diseqc=diseqc;
			m_lnb0=lnb0;
			m_lnb1=lnb1;
			m_lnbsw=lnbsw;
			m_lnbkhz=lnbkhz;
			m_lnbfreq=lnbfreq;
			m_selKhz=selKhz;
		
		}
		
		int getUTC(int val)
		{
			if ((val&0xF0)>=0xA0)
				return 0;
			if ((val&0xF)>=0xA)
				return 0;
			return ((val&0xF0)>>4)*10+(val&0xF);
		}

		public ArrayList GetLanguages()
		{
			ArrayList langs=new ArrayList();
			foreach(string str in m_langLanguage)
				langs.Add(str);
			return langs;
		}
		//
		//
		public ArrayList GetLanguageCodes()
		{
			ArrayList langs=new ArrayList();
			foreach(string str in m_langCodes)
				langs.Add(str);
			return langs;
		}
		//
		//
		static public string GetLanguageFromCode(string code)
		{
			int n=0;
			if(code==null)
				return Strings.Unknown;
			if(code=="")
				return Strings.Unknown;
			if(code.Length>3)
				return code;
			foreach(string langCode in m_langCodes)
			{
				if(langCode.Equals(code))
					return m_langLanguage.GetValue(n).ToString();
				n++;
			}
			return code;
		}
		//
		//
		// iso 639 language codes

		private void m_eitTimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			m_breakAction=true;
			m_syncWait=true;
			if(m_streamDemuxer!=null)
				m_streamDemuxer.ResetGrabber();//stop timer and set pid to -1
		}

		// get stream data with mps demuxer-class
		public bool GetStreamData(DShowNET.IBaseFilter filter,int pid, int tid,int tableSection,int timeout)
		{
			//when in config...
			if (GUIGraphicsContext.DX9Device==null || m_getTablesUsingMS)
			{
				return MsGetStreamData(filter,pid,tid,tableSection,timeout);
			}

			m_sectionsList=new ArrayList();
			bool flag=false;
			if(m_streamDemuxer.GetTable(pid,tid,timeout)==true)
			{
				m_syncWait=false;
				m_eitTimeoutTimer.Interval=timeout*2;
				m_eitTimeoutTimer.Start();
				while(m_syncWait==false)
				{
					System.Windows.Forms.Application.DoEvents();
					System.Threading.Thread.Sleep(100);
				}
				m_eitTimeoutTimer.Stop();
			}
			return flag;	

		}
		private bool MsGetStreamData(DShowNET.IBaseFilter filter,int pid, int tid,int tableSection,int timeout)
		{
			bool flag;
			int dataLen=0;
			int header=0;
			int tableExt=0;
			int sectNum=0;
			int sectLast=0;
			int version=0;
			byte[] arr = new byte[1];
			IntPtr sectionBuffer=IntPtr.Zero;


			//Log.Write("Get pid:{0:X} tid:{1:X} section:{2:X}",pid,tid,tableSection);
			m_sectionsList=new ArrayList();
			flag = GetSectionData(filter,pid, tid,ref sectLast,tableSection,timeout);
			if(flag==false)
			{
				
				Log.WriteFile(Log.LogType.Capture,"DVBSections:MsGetStreamData() failed for pid:{0:X} tid:{1:X} section:{2} timeout:{3}", pid,tid,tableSection,timeout);
				return false;
			}
			if (sectLast<=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBSections:Sections:MsGetStreamData() timeout for pid:{0:X} tid:{1:X} section:{2} timeout:{3}", pid,tid,tableSection,timeout);
			}
			//Log.Write("sections:{0}",sectLast);
			int totalSections=sectLast;
			for(int n=0;n<totalSections;n++)
			{
				flag=GetSectionPtr(n,ref sectionBuffer,ref dataLen,ref header, ref tableExt, ref version,ref sectNum, ref sectLast);
				//Log.Write(" get sect:{0} returned len:{1} ext:{2} num:{3} last:{4} version:{5}",flag,dataLen,tableExt,sectNum, sectLast,version);
				if(flag)
				{
					if (tableExt != - 1)
					{
						arr = new byte[dataLen+8 + 1];
						try
						{
							Marshal.Copy(sectionBuffer, arr, 8, dataLen);
						}
						catch
						{
							Log.WriteFile(Log.LogType.Capture,true,"dvbsections: error on copy data. address={0}, length ={1}",sectionBuffer,dataLen);
							m_sectionsList.Clear();
							break;
						}
						arr[0] = (byte)tid;
						arr[1] = (byte)((header >> 8) & 255);
						arr[2] =(byte) (header & 255);
						arr[3] = (byte)((tableExt >> 8) & 255);
						arr[4] = (byte)(tableExt & 255);
						arr[5] =(byte) version;
						arr[6] = (byte)sectNum;
						arr[7] = (byte)sectLast;
						m_sectionsList.Add(arr);
						if(tableSection!=0)
							break;
					}
					else
					{
						arr = new byte[dataLen+3 + 1];
						try
						{
							Marshal.Copy(sectionBuffer, arr, 3, dataLen);
						}
						catch
						{
							Log.WriteFile(Log.LogType.Capture,true,"dvbsections: error on copy data. address={0}, length ={1}",sectionBuffer,dataLen);
							m_sectionsList.Clear();
							break;
						}
						arr[0] = System.Convert.ToByte(tid);
						arr[1] = System.Convert.ToByte((header >> 8) & 255);
						arr[2] = System.Convert.ToByte(header & 255);
						m_sectionsList.Add(arr);
						if(tableSection!=0)
							break;
					}// else

				}// if(flag)
			}//for
			ReleaseSectionsBuffer();
			return true;
		}		
		string DVB_GetLanguageFromISOCode(string code)
		{
			return "";
		}
		public string GetNetworkProvider(int onid)
		{
			return DVB_GetNetworkProvider(onid);
		}
		string DVB_GetNetworkProvider(int orgNetworkID)
		{
			switch(orgNetworkID)
			{
				case 0x0000: return "Reserved";
				case 0x0001: return "Astra 19,2°E" ;
				case 0x0002: return "Astra 28,2°E" ;
				case 0x0019: return "Astra" ;
				case 0x001A: return "Quiero Televisión" ;
				case 0x001B: return "RAI" ;
				case 0x001F: return "Europe Online Networks" ;
				case 0x0020: return "ASTRA" ;
				case 0x0026: return "Hispasat Network" ;
				case 0x0027: return "Hispasat 30°W" ;
				case 0x0028: return "Hispasat 30°W" ;
				case 0x0029: return "Hispasat 30°W" ;
				case 0x002E: return "Xantic" ;
				case 0x002F: return "TVNZ Digital" ;
				case 0x0030: return "Canal+ Satellite Network" ;
				case 0x0031: return "Hispasat - VIA DIGITAL" ;
				case 0x0034: return "Hispasat Network" ;
				case 0x0035: return "Nethold Main Mux System" ;
				case 0x0036: return "TV Cabo" ;
				case 0x0037: return "STENTOR" ;
				case 0x0038: return "OTE" ;
				case 0x0040: return "Croatian Post and Telecommunications" ;
				case 0x0041: return "Mindport network" ;
				case 0x0047: return "Telenor" ;
				case 0x0048: return "STAR DIGITAL" ;
				case 0x0049: return "Sentech" ;
				case 0x0050: return "HRT Croatian Radio and Television" ;
				case 0x0051: return "Havas" ;
				case 0x0052: return "StarGuide Digital Networks" ;
				case 0x0054: return "Teracom Satellite" ;
				case 0x0055: return "Sirius (Teracom)" ;
				case 0x0058: return "UBC Thailand" ;
				case 0x005E: return "Sirius" ;
				case 0x005F: return "Sirius" ;
				case 0x0060: return "MSG MediaServices GmbH" ;
				case 0x0069: return "Optus Communications" ;
				case 0x0070: return "NTV+" ;
				case 0x0073: return "PanAmSat 4 68.5°E" ;
				case 0x007D: return "Skylogic" ;
				case 0x007E: return "Eutelsat" ;
				case 0x007F: return "Eutelsat" ;
				case 0x0085: return "BetaTechnik" ;
				case 0x0090: return "TDF" ;
				case 0x00A0: return "News Datacom" ;
				case 0x00A5: return "News Datacom" ;
				case 0x00A6: return "ART" ;
				case 0x00A7: return "Globecast" ;
				case 0x00A8: return "Foxtel" ;
				case 0x00A9: return "Sky New Zealand" ;
				case 0x00B3: return "TPS" ;
				case 0x00B4: return "Telesat 107.3°W | Telesat Canada" ;
				case 0x00B5: return "Telesat 111.1°W" ;
				case 0x00B6: return "Telstra Saturn" ;
				case 0x00BA: return "Satellite Express 6 (80°E)" ;
				case 0x00CD: return "Canal +" ;
				case 0x00EB: return "Eurovision Network" ;
				case 0x0100: return "ExpressVu" ;
				case 0x010D: return "Skylogic Italia" ;
				case 0x010E: return "Eutelsat 10°E" ;
				case 0x010F: return "Eutelsat 10°E" ;
				case 0x0110: return "Mediaset" ;
				case 0x011F: return "visAvision Network" ;
				case 0x013D: return "Skylogic Italia" ;
				case 0x013E: return "Eutelsat 13°E" ;
				case 0x013F: return "Eutelsat 13°E" ;
				case 0x016D: return "Skylogic Italia" ;
				case 0x016E: return "Eutelsat 16°E" ;
				case 0x016F: return "Eutelsat 16°E" ;
				case 0x01F4: return "MediaKabel B.V" ;
				case 0x022D: return "Skylogic Italia" ;
				case 0x022F: return "Eutelsat 21.5°E" ;
				case 0x026D: return "Skylogic Italia" ;
				case 0x026F: return "Eutelsat 25.5°E" ;
				case 0x029D: return "Skylogic Italia" ;
				case 0x029E: return "Eutelsat 29°E" ;
				case 0x029F: return "Eutelsat 28.5°E" ;
				case 0x02BE: return "Arabsat" ;
				case 0x033D: return "Skylogic Italia" ;
				case 0x033f: return "Eutelsat 33°E " ;
				case 0x036D: return "Skylogic Italia" ;
				case 0x036E: return "Eutelsat 36°E" ;
				case 0x036F: return "Eutelsat 36°E" ;
				case 0x03E8: return "Telia, Sweden" ;
				case 0x047D: return "Skylogic Italia" ;
				case 0x047f: return "Eutelsat 12.5°W" ;
				case 0x048D: return "Skylogic Italia" ;
				case 0x048E: return "Eutelsat 48°E" ;
				case 0x048F: return "Eutelsat 48°E" ;
				case 0x052D: return "Skylogic Italia" ;
				case 0x052f: return "Eutelsat 8°W" ;
				case 0x055D: return "Skylogic Italia" ;
				case 0x055f: return "Eutelsat" ;
				case 0x0600: return "UPC Satellite" ;
				case 0x0601: return "UPC Cable" ;
				case 0x0602: return "Tevel" ;
				case 0x071D: return "Skylogic Italia" ;
				case 0x071f: return "Eutelsat 70.5°E" ;
				case 0x0801: return "Nilesat 101" ;
				case 0x0880: return "MEASAT 1, 91.5°E" ;
				case 0x0882: return "MEASAT 2, 91.5°E" ;
				case 0x0883: return "MEASAT 2, 148.0°E" ;
				case 0x088F: return "MEASAT 3" ;
				case 0x1000: return "Optus B3 156°E" ;
				case 0x1001: return "DISH Network" ;
				case 0x1002: return "Dish Network 61.5 W" ;
				case 0x1003: return "Dish Network 83 W" ;
				case 0x1004: return "Dish Network 119" ;
				case 0x1005: return "Dish Network 121" ;
				case 0x1006: return "Dish Network 148" ;
				case 0x1007: return "Dish Network 175" ;
				case 0x1008: return "Dish Network W" ;
				case 0x1009: return "Dish Network X" ;
				case 0x100A: return "Dish Network Y" ;
				case 0x100B: return "Dish Network Z" ;
				case 0x1010: return "ABC TV" ;
				case 0x1011: return "SBS" ;
				case 0x1012: return "Nine Network Australia" ;
				case 0x1013: return "Seven Network Australia" ;
				case 0x1014: return "Network TEN Australia" ;
				case 0x1015: return "WIN Television Australia" ;
				case 0x1016: return "Prime Television Australia" ;
				case 0x1017: return "Southern Cross Broadcasting Australia" ;
				case 0x1018: return "Telecasters Australia" ;
				case 0x1019: return "NBN Australia" ;
				case 0x101A: return "Imparja Television Australia" ;
				case 0x101f: return "Reserved" ;
				case 0x1100: return "GE Americom" ;
				case 0x2000: return "Thiacom 1,2 78.5°E" ;
				case 0x2024: return "Australian Digital Terrestrial Television" ;
				case 0x2038: return "Belgian Digital Terrestrial Television" ;
				case 0x20CB: return "Czech Republic Digital Terrestrial Television" ;
				case 0x20D0: return "Danish Digital Terrestrial Television" ;
				case 0x20E9: return "Estonian Digital Terrestrial Television" ;
				case 0x20F6: return "Finnish Digital Terrestrial Television" ;
				case 0x2114: return "German Digital Terrestrial Television DVB-T broadcasts" ;
				case 0x2174: return "Irish Digital Terrestrial Television" ;
				case 0x2178: return "Israeli Digital Terrestrial Television" ;
				case 0x2210: return "Netherlands Digital Terrestrial Television" ;
				case 0x22BE: return "Singapore Digital Terrestrial Television" ;
				case 0x22D4: return "Spanish Digital Terrestrial Television" ;
				case 0x22F1: return "Swedish Digital Terrestrial Television" ;
				case 0x22F4: return "Swiss Digital Terrestrial Television" ;
				case 0x233A: return "UK Digital Terrestrial Television" ;
				case 0x3000: return "PanAmSat 4 68.5°E" ;
				case 0x5000: return "Irdeto Mux System" ;
				case 0x616D: return "BellSouth Entertainment" ;
				case 0x6600: return "UPC Satellite" ;
				case 0x6601: return "UPC Cable" ;
				case 0xF000: return "Small Cable networks" ;
				case 0xF001: return "Deutsche Telekom" ;
				case 0xF010: return "Telefónica Cable" ;
				case 0xF020: return "Cable and Wireless Communication " ;
				case 0xF100: return "Casema" ;
				case 0xF750: return "Telewest Communications Cable Network" ;
				case 0xF751: return "OMNE Communications" ;
				case 0xFBFC: return "MATAV" ;
				case 0xFBFD: return "Telia Kabel-TV" ;
				case 0xFBFE: return "TPS" ;
				case 0xFBFF: return "Sky Italia" ;
				case 0xFC10: return "Rhône Vision Cable" ;
				case 0xFC41: return "France Telecom Cable" ;
				case 0xFD00: return "National Cable Network";
				case 0xFE00: return "TeleDenmark Cable TV";
				case 0xFEff: return "Network Interface Modules" ;
				case 0xFFfe: return "ESTI Private" ;
				default: return "Unknown Network Provider";
			}
		}


		#endregion

		#region Table Decoding
		#region tables
		private void decodeBATTable(byte[] buf, TPList transponderInfo, ref Transponder tp)
		{
			//  8------ 112-12-- -------- 16------ -------- 2-5----1 8------- 8------- 4---12-- --------
			// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
			//    0        1        2         3        4       5       6         7        8       9     

			Log.Write("DecodeBAT {0}",buf.Length);
			int table_id = buf[0];
			int section_syntax_indicator = (buf[1]>>7) & 1;
			int section_length = ((buf[1]& 0x7)<<8) + buf[2];
			int bouquet_id = (buf[3]<<8)+buf[4];
			int version_number = ((buf[5]>>1)&0x1F);
			int current_next_indicator = buf[5] & 1;
			int section_number = buf[6];
			int last_section_number = buf[7];
			int descriptor_length = ((buf[8]&0xf)<<8)+buf[9];
			int start=10;
			int len=0;
			
			Log.Write("DecodeBAT: desc length:{0} bouquet id:{1} tid:{2} sectionlen:{3}", 
						descriptor_length,bouquet_id,table_id,section_length);
			while (len < descriptor_length)
			{
				int descriptor_tag = buf[start+len];
				int descriptor_len = buf[start+len+1];
				Log.Write("  descriptor:{0:X} len:{1}", descriptor_tag, descriptor_len);
				len += (descriptor_len+2);
			}
			start += descriptor_length;


			Log.Write("DecodeBAT: decode transport length at:{0}", start);
			int transport_length = ((buf[start]&0xf)<<8)+buf[start+1];
			start+=2;
			Log.Write("DecodeBAT: transport_length:{0}", transport_length);
			len=0;
			while (len < transport_length)
			{
				int tsid = ((buf[start+len]& 0xF)<<8) + buf[start+len+1];
				int onid = ((buf[start+len+2]& 0xF)<<8) + buf[start+len+3];
				descriptor_length = ((buf[start+len+4]& 0xF)<<8) + buf[start+len+5];
				Log.Write("  tsid:{0:X} onid:{1} descriptor_length:{2}", tsid,onid,descriptor_length);

				int descstart=start+len+6;
				int desclen=0;
				while (desclen < descriptor_length)
				{
					int descriptor_tag = buf[descstart+desclen];
					int descriptor_len = buf[descstart+desclen+1];
					Log.Write("  descriptor:{0:X} len:{1}", descriptor_tag, descriptor_len);
					desclen += (descriptor_len+2);
				}
				len += (descriptor_length+6);
			}
		}

		/// <summary>
		/// The PAT table contains the PMT pid of each program
		/// </summary>
		/// <param name="buf">PAT table</param>
		/// <returns>tp.PMTTable table will contain all the PMT pids</returns>
		private int decodePATTable(byte[] buf, TPList transponderInfo, ref Transponder tp)
		{
			int loop;
			// check
			if(buf.Length<10)
			{
				Log.Write("decodePATTable() length < 10 length={0}", buf.Length);
				return 0;
			}
			int table_id = buf[0];
			int section_syntax_indicator = (buf[1]>>7) & 1;
			int section_length = ((buf[1]& 0xF)<<8) + buf[2];
			int transport_stream_id = (buf[3]<<8)+buf[4];
			int version_number = ((buf[5]>>1)&0x1F);
			int current_next_indicator = buf[5] & 1;
			int section_number = buf[6];
			int last_section_number = buf[7];

			byte[] b = new byte[5];
			loop =(section_length - 9) / 4;
			//Log.WriteFile(Log.LogType.Capture,"dvbsections:decodePatTable() loop={0}", loop);
			if(loop<1)
			{
				Log.Write("decodePATTable() loop < 1 loop={0}", buf.Length);
				return 0;
			}
			for (int count=0;count<loop;count++)
			{
				ChannelInfo ch=new ChannelInfo();
				System.Array.Copy(buf, 8 +(count * 4), b, 0, 4);
				ch.transportStreamID=transport_stream_id;
				ch.program_number = (b[0]<<8)+b[1];
				ch.reserved = ((b[2]>>5) & 7);
				ch.network_pmt_PID = ((b[2] & 0x1F)<<8)+b[3];
				//Log.WriteFile(Log.LogType.Capture,"dvbsections:decodePatTable() chan:{0} {1} {2}", ch.transportStreamID,ch.networkID,ch.network_pmt_PID);
				if(ch.program_number!=0)
				{
					tp.PMTTable.Add(ch);
				}
				else
				{
					Log.Write("dvbsections:decodePATTable() program number=0");
				}
			}
			return loop;
		}

		public bool GetChannelInfoFromPMT(byte[] buffer, ref ChannelInfo channelInfo)
		{
			TPList unused1 = new TPList();
			Transponder unused2 = new Transponder();

			channelInfo.program_number = (buffer[3] << 8) + buffer[4];

			if(decodePMTTable(buffer, unused1, unused2, ref channelInfo)  == 0)
				return false;

			return true;
		}

		public CaPMT GetCAPMT(byte[] pmt)
		{
			ChannelInfo inf=new ChannelInfo();
			TPList unused1 = new TPList();
			Transponder unused2 = new Transponder();
			decodePMTTable(pmt,unused1,unused2,ref inf);
			return inf.caPMT;
		}
		/// <summary>
		/// The PMT table contains the audio/video/teletext pids for each program
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="transponderInfo"></param>
		/// <param name="tp"></param>
		/// <param name="pat"></param>
		/// <returns></returns>
		private int decodePMTTable(byte[] buf, TPList transponderInfo, Transponder tp,ref ChannelInfo pat)
		{
			if(buf.Length<13)
			{
				Log.Write("decodePMTTable() len < 13 len={0}", buf.Length);
				return 0;
			}
			int table_id = buf[0];
			int section_syntax_indicator = (buf[1]>>7) & 1;
			int section_length = ((buf[1]& 0xF)<<8) + buf[2];
			int program_number = (buf[3]<<8)+buf[4];
			int version_number = ((buf[5]>>1)&0x1F);
			int current_next_indicator = buf[5] & 1;
			int section_number = buf[6];
			int last_section_number = buf[7];
			int pcr_pid = ((buf[8]& 0x1F)<<8)+buf[9];
			int program_info_length = ((buf[10] & 0xF)<<8)+buf[11];
			//
			// store info about the channel
			//
			pat.caPMT=new CaPMT();
			pat.caPMT.CADescriptors_ES=new ArrayList();
			pat.caPMT.CADescriptors_PRG=new ArrayList();

			pat.caPMT.ProgramNumber=program_number;
			pat.caPMT.CurrentNextIndicator=current_next_indicator;
			pat.caPMT.VersionNumber=version_number;
			pat.caPMT.CAPmt_Listmanagement=0x9f8032;

			if(pat.program_number!=program_number)
			{
				
				Log.Write("decodePMTTable() pat program#!=program numer {0}!={1}", pat.program_number,program_number);
				//return 0;
			}
			pat.pid_list = new ArrayList();

			pat.pcr_pid = pcr_pid;
			string pidText="";
			//
			int len1 = section_length - 4;
			int len2 = program_info_length;
			//len1=
			int pointer = 12;
			int x;
			while (len2 > 0)
			{
				int indicator=buf[pointer];
				x = 0;
				x = buf[pointer + 1] + 2;
				byte[] data=new byte[x];
				System.Array.Copy(buf,pointer,data,0,x);
				if(indicator==0x9)
				{
					pat.caPMT.CADescriptors_PRG.Add(data);
					pat.caPMT.ProgramInfoLength+=data.Length;
					string tmpString=DVB_CADescriptor(data);
					if(pidText.IndexOf(tmpString,0)==-1)
						pidText+=tmpString+";";
				}
				if(pat.caPMT.ProgramInfoLength>0)
				{
					pat.caPMT.CAPmt_CommandID_PRG=1;
					pat.caPMT.ProgramInfoLength+=1;
				}
				len2 -= x;
				pointer += x;
				len1 -= x;
			}
			//byte[] b = new byte[6];
			PMTData pmt;
			while (len1 > 4)
			{
				pmt=new PMTData();
				//System.Array.Copy(buf, pointer, b, 0, 5);
				try
				{
					pmt.stream_type = buf[pointer];
					pmt.reserved_1 = (buf[pointer+1]>>5)& 7;
					pmt.elementary_PID = ((buf[pointer+1]&0x1F)<<8)+buf[pointer+2];
					pmt.reserved_2 = (buf[pointer+3]>>4) & 0xF;
					pmt.ES_info_length = ((buf[pointer+3] & 0xF)<<8)+buf[pointer+4];
				}
				catch
				{
				}
				switch(pmt.stream_type)
				{
					case 0x1:
						pmt.isVideo=true;
						break;
					case 0x2:
						pmt.isVideo=true;
						break;
					case 0x3:
						pmt.isAudio=true;
						break;
					case 0x4:
						pmt.isAudio=true;
						break;
				}
				pointer += 5;
				len1 -= 5;
				len2 = pmt.ES_info_length;
				if (len1 > 0)
				{
					while (len2 > 0)
					{
						x = 0;
						if (pointer + 1 < buf.Length)
						{
							int indicator=buf[pointer];
							x = buf[pointer + 1] + 2;
							if(x+pointer<buf.Length) // parse descriptor data
							{
								byte[] data=new byte[x];
								System.Array.Copy(buf,pointer,data,0,x);
								switch(indicator)
								{
									case 0x02: // video
									case 0x03: // audio
										//Log.Write("dvbsections: indicator {1} {0} found",(indicator==0x02?"for video":"for audio"),indicator);
										break;
									case 0x09:
										pat.caPMT.StreamType=pmt.stream_type;
										pat.caPMT.ElementaryStreamPID=pmt.elementary_PID;
										pat.caPMT.CAPmt_CommandID_ES=1;
										pat.caPMT.CADescriptors_ES.Add(data);
										pat.caPMT.ElementaryStreamInfoLength=pmt.ES_info_length;
										//pat.caData.Add(data);
										string tmpString=DVB_CADescriptor(data);
										if(pidText.IndexOf(tmpString,0)==-1)
											pidText+=tmpString+";";
										break;
									case 0x0A:
										pmt.data=DVB_GetMPEGISO639Lang(data);
										break;
									case 0x6A:
										pmt.isAC3Audio=true;
										break;
									case 0x56:
										pmt.isTeletext=true;
										pmt.teletextLANG=DVB_GetTeletextDescriptor(data);
										break;
										//case 0xc2:
									case 0x59:
										if(pmt.stream_type==0x05 || pmt.stream_type==0x06)
										{
											pmt.isDVBSubtitle=true;
											pmt.data=DVB_SubtitleDescriptior(data);
										}
										break;
									default:
										pmt.data="";
										break;
								}

							}
						}
						else
						{
							break;
						}
						len2 -= x;
						len1 -= x;
						pointer += x;
					}
				}
				pat.pid_list.Add(pmt);

			}
			pat.pidCache=pidText;
			return 1;
		}


		/// <summary>
		/// the sdt table contains the service name & provider name
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="transponderInfo"></param>
		/// <param name="tp"></param>
		/// <param name="pat"></param>
		/// <returns></returns>
		private int decodeSDTTable(byte[] buf, TPList transponderInfo, ref Transponder tp,ref ChannelInfo pat)
		{

			//tableid syntax res res sectlen  tsid    res vers cni sectn lsectn onid    resv
			//8       1      1   2   4 + 8    8 + 8   2   5    1   8     8      8 + 8   8
			//[0]     [1..............] [2]  [3] [4] [5.........] [6]   [7]    [8] [9] [10]
			if(buf.Length<12)
			{
				Log.Write("decodeSDTTable() len < 12 len={0}", buf.Length);
				return -1;
			}
			
			int table_id = buf[0];
			int section_syntax_indicator = (buf[1]>>7) & 1;
			int section_length = ((buf[1]& 0xF)<<8) + buf[2];
			int transport_stream_id = (buf[3]<<8)+buf[4];
			int version_number = ((buf[5]>>1)&0x1F);
			int current_next_indicator = buf[5] & 1;
			int section_number = buf[6];
			int last_section_number = buf[7];
			int original_network_id = (buf[8]<<8)+buf[9];
			int len1 = section_length - 11 - 4;
			int descriptors_loop_length;
			int len2;
			int service_id;
			int EIT_schedule_flag;
			int free_CA_mode;
			int running_status;
			int EIT_present_following_flag;
			int pointer = 11;
			int x = 0;
		 
			//Log.Write("decodeSDTTable len={0}/{1} section no:{2} last section no:{3}", buf.Length,section_length,section_number,last_section_number);

			while (len1 > 0)
			{
				service_id = (buf[pointer]<<8)+buf[pointer+1];
				EIT_schedule_flag = (buf[pointer+2]>>1) & 1;
				EIT_present_following_flag = buf[pointer+2] & 1;
				running_status = (buf[pointer+3]>>5) & 7;
				free_CA_mode = (buf[pointer+3]>>4) &1;
				descriptors_loop_length = ((buf[pointer+3] & 0xF)<<8)+buf[pointer+4];
				//
				pointer += 5;
				len1 -= 5;
				len2 = descriptors_loop_length;
				
				//
				while (len2 > 0)
				{
					int indicator=buf[pointer];
					x = 0;
					x = buf[pointer + 1] + 2;
					byte[] service = new byte[buf.Length-pointer + 1];
					System.Array.Copy(buf, pointer, service, 0, buf.Length - pointer);
					//Log.Write("indicator = {0:X}",indicator);
					if (indicator == 0x48)
					{
						ServiceData serviceData;
							
						serviceData = DVB_GetService(service);
						if (serviceData.serviceName.Length < 1)
						{
							serviceData.serviceName = "Unknown Channel";
						}
						if (serviceData.serviceProviderName.Length < 1)
						{
							serviceData.serviceProviderName = "Unknown Provider";
						}
						if(service_id==pat.program_number)
						{
							pat.serviceType=serviceData.serviceType;
							pat.service_name=serviceData.serviceName ;
							pat.service_provider_name=serviceData.serviceProviderName;
							pat.transportStreamID=transport_stream_id;
							pat.networkID=original_network_id;
							pat.serviceID = service_id;
							pat.eitPreFollow=(EIT_present_following_flag==0)?false:true;
							pat.eitSchedule=(EIT_schedule_flag==0)?false:true;
							pat.scrambled=(free_CA_mode==0)?false:true;
							// freq tuning data
							pat.diseqc=m_diseqc;
							pat.freq=transponderInfo.TPfreq;
							pat.pol=transponderInfo.TPpol;
							pat.symb=transponderInfo.TPsymb;
							pat.lnbkhz=m_selKhz;
							pat.fec=6; // always auto for b2c2
							pat.lnb01=m_lnbfreq;
						}
							//
						//tp.serviceData.Add(serviceData);

					}
					else
					{
						int st=indicator;
						if(st!=0x53 && st!=0x64)
							st=1;
					}
					len2 -= x;
					pointer += x;
					len1 -= x;
				}
				
			}
			if(last_section_number>section_number)
				return last_section_number;
			return 0;
		}

		private object decodeNITTable(byte[] buf,ref DVBNetworkInfo nit)
		{
			int table_id;
			int section_syntax_indicator;
			int section_length;
			int network_id;
			int version_number;
			int current_next_indicator;
			int section_number;
			int last_section_number;
			int network_descriptor_length;
			int transport_stream_loop_length;
			int transport_stream_id;
			int original_network_id;

			int transport_descriptor_length=0;
			//
			int pointer=0;
			int l1=0;
			int l2=0;

			try
			{
				table_id = buf[0];
				section_syntax_indicator = buf[1] &0x80;
				section_length = ((buf[1] &0xF)<<8)+buf[2];
				network_id = (buf[3]<<8)+buf[4];
				version_number = (buf[5]>>1) &0x1F;
				current_next_indicator = buf[5]&1;
				section_number = buf[6];
				last_section_number = buf[7];
				network_descriptor_length = ((buf[8]&0xF)<<8)+buf[9];
			
			
				l1 = network_descriptor_length;
				pointer += 10;
				int x = 0;
			

				while (l1 > 0)
				{
					int indicator=buf[pointer];
					x = buf[pointer + 1] + 2;
					byte[] service = new byte[x];
					System.Array.Copy(buf, pointer, service, 0, x);
					if(indicator==0x40)
					{
						nit.NetworkName=System.Text.Encoding.ASCII.GetString(service,2,x-2);
					}
					l1 -= x;
					pointer += x;
				}
				transport_stream_loop_length = ((buf[pointer] &0xF)<<8)+buf[pointer+1];
				l1 = transport_stream_loop_length;
				pointer += 2;
				while (l1 > 0)
				{
					transport_stream_id = (buf[pointer]<<8)+buf[pointer+1];
					original_network_id = (buf[pointer+2]<<8)+buf[pointer+3];
					transport_descriptor_length = ((buf[pointer+4] & 0xF)<<8)+buf[pointer+5];
					pointer += 6;
					l1 -= 6;
					l2 = transport_descriptor_length;
					while (l2 > 0)
					{
						int indicator=buf[pointer];
						x = buf[pointer + 1]+2 ;
						byte[] service = new byte[x + 1];
						System.Array.Copy(buf, pointer, service, 0, x);
						if(indicator==0x43) // sat
						{
							NITSatDescriptor tp=new NITSatDescriptor();
							DVB_GetSatDelivSys(service,ref tp);
							nit.NITDescriptorList.Add(tp);
						}
						if(indicator==0x44) // cable
						{
							NITCableDescriptor tp=new NITCableDescriptor();
							DVB_GetCableDelivSys(service,ref tp);
							nit.NITDescriptorList.Add(tp);
						}
						if(indicator==0x5A) // terrestrial
						{
							NITTerrestrialDescriptor tp=new NITTerrestrialDescriptor();
							DVB_GetTerrestrialDelivSys(service,ref tp);
							nit.NITDescriptorList.Add(tp);
						}
						//
						pointer += x;
						l2 -= x;
						l1 -= x;
					}
				
				}
				x = 0;
			}
			catch
			{
				//int a=0;
			}
			return 0;
		}

		private object decodeNITTable(byte[] buf,ref Transponder tp)
		{
			int table_id;
			int section_syntax_indicator;
			int section_length;
			int network_id;
			int version_number;
			int current_next_indicator;
			int section_number;
			int last_section_number;
			int network_descriptor_length;
			int transport_stream_loop_length;
			int transport_stream_id;
			int original_network_id;

			int transport_descriptor_length=0;
			//
			int pointer=0;
			int l1=0;
			int l2=0;

			try
			{
				table_id = buf[0];
				section_syntax_indicator = buf[1] &0x80;
				section_length = ((buf[1] &0xF)<<8)+buf[2];
				network_id = (buf[3]<<8)+buf[4];
				version_number = (buf[5]>>1) &0x1F;
				current_next_indicator = buf[5]&1;
				section_number = buf[6];
				last_section_number = buf[7];
				network_descriptor_length = ((buf[8]&0xF)<<8)+buf[9];
			
			
				l1 = network_descriptor_length;
				pointer += 10;
				int x = 0;
			

				while (l1 > 0)
				{
					int indicator=buf[pointer];
					x = buf[pointer + 1] + 2;
					byte[] service = new byte[x];
					System.Array.Copy(buf, pointer, service, 0, x);
					//					if(indicator==0x40)
					//					{
					//						nit.NetworkName=System.Text.Encoding.ASCII.GetString(service,2,x-2);
					//					}
					//					// This should be neccessary be some networks may have their information here
					//					if(indicator==0x43) // sat
					//					{
					//						NITSatDescriptor tp=new NITSatDescriptor();
					//						DVB_GetSatDelivSys(service,ref tp);
					//						nit.NITDescriptorList.Add(tp);
					//					}
					//					if(indicator==0x44) // cable
					//					{
					//						NITCableDescriptor tp=new NITCableDescriptor();
					//						DVB_GetCableDelivSys(service,ref tp);
					//						nit.NITDescriptorList.Add(tp);
					//					}
					//					if(indicator==0x5A) // terrestrial
					//					{
					//						NITTerrestrialDescriptor tp=new NITTerrestrialDescriptor();
					//						DVB_GetTerrestrialDelivSys(service,ref tp);
					//						nit.NITDescriptorList.Add(tp);
					//					}
					l1 -= x;
					pointer += x;
				}
				transport_stream_loop_length = ((buf[pointer] &0xF)<<8)+buf[pointer+1];
				l1 = transport_stream_loop_length;
				pointer += 2;
				while (l1 > 0)
				{
					transport_stream_id = (buf[pointer]<<8)+buf[pointer+1];
					original_network_id = (buf[pointer+2]<<8)+buf[pointer+3];
					transport_descriptor_length = ((buf[pointer+4] & 0xF)<<8)+buf[pointer+5];
					pointer += 6;
					l1 -= 6;
					l2 = transport_descriptor_length;
					while (l2 > 0)
					{
						int indicator=buf[pointer];
						x = buf[pointer + 1]+2 ;
						byte[] service = new byte[x + 1];
						System.Array.Copy(buf, pointer, service, 0, x);
						//						if(indicator==0x43) // sat
						//						{
						//							NITSatDescriptor tp=new NITSatDescriptor();
						//							DVB_GetSatDelivSys(service,ref tp);
						//							nit.NITDescriptorList.Add(tp);
						//						}
						//						if(indicator==0x44) // cable
						//						{
						//							NITCableDescriptor tp=new NITCableDescriptor();
						//							DVB_GetCableDelivSys(service,ref tp);
						//							nit.NITDescriptorList.Add(tp);
						//						}
						//						if(indicator==0x5A) // terrestrial
						//						{
						//							NITTerrestrialDescriptor tp=new NITTerrestrialDescriptor();
						//							DVB_GetTerrestrialDelivSys(service,ref tp);
						//							nit.NITDescriptorList.Add(tp);
						//						}
						if(indicator==0x83) // lcn
						{
							Log.Write("Found LCN Descriptor in NIT");
							this.DVB_GetLogicalChannelNumber(service, ref tp.channels);
						}
						//
						pointer += x;
						l2 -= x;
						l1 -= x;
					}
				
				}
				x = 0;
			}
			catch
			{
				//int a=0;
			}
			return 0;
		}

		void LoadPMTTables (DShowNET.IBaseFilter filter,TPList tpInfo, ref Transponder tp)
		{
			int t;
			int n;
			ArrayList	tab42=new ArrayList();
			ArrayList	tab46=new ArrayList();

			//get SDT (Service Description Table) (pid 0x11)
			//The SDT is used to list the names and other parameters of the services within TSs. 
			//For each TS a separate SDT sub-Table exists.
			GetStreamData(filter,0x11, 0x42,0,m_timeoutMS);
			tab42=(ArrayList)m_sectionsList.Clone();
			
			GetStreamData(filter,0x11, 0x46,0,500); // low value, nothing in most of time
			tab46=(ArrayList)m_sectionsList.Clone();

			//for each PMT ...
			ChannelInfo pat;
			ArrayList pmtList = tp.PMTTable;
			int pmtScans;
			pmtScans = (pmtList.Count / 20) + 1;
			for (t = 1; t <= pmtScans; t++)
			{
				for (n = 0; n <= 19; n++)
				{
					if (((t - 1) * 20) + n > pmtList.Count - 1)
					{
						break;
					}
					pat = (ChannelInfo) pmtList[((t - 1) * 20) + n];
					
					int res=0;
					if(m_setPid)
					{
						DVBSkyStar2Helper.DeleteAllPIDs((DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3)m_dataCtrl,0);
						DVBSkyStar2Helper.SetPidToPin((DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3)m_dataCtrl,0,pat.network_pmt_PID);
					}

					//get the PMT table
					
					GetStreamData(filter,pat.network_pmt_PID, 2,0,m_timeoutMS); 

					//PMT table contains the audio/video/teletext pids, so decode them
					
					foreach(byte[] wdata in m_sectionsList)
					{
						res=decodePMTTable(wdata, tpInfo, tp,ref pat);
					}
					
					//SDT contains the service/provider name, so get it
					if(res>0)
					{
						
						foreach(byte[] wdata in tab42)
							decodeSDTTable(wdata, tpInfo,ref tp,ref pat);
						foreach(byte[] wdata in tab46)
							decodeSDTTable(wdata, tpInfo,ref tp,ref pat);
					}

					if (pat.pid_list==null)
						Log.Write("DVBSections: no PMT found for channel:#{0}", ((t - 1) * 20) + n);
					else if (pat.pid_list.Count==0)
						Log.Write("DVBSections: no PMT found for channel:#{0}", ((t - 1) * 20) + n);
					
					if (pat.serviceID==0)
						Log.Write("DVBSections: no SDT found for channel:#{0}", ((t - 1) * 20) + n);

					tp.channels.Add(pat);
				}
			}
			
		}//private void LoadPMTTables (DShowNET.IBaseFilter filter,TPList tpInfo, ref Transponder tp)
		

		private int decodeEITTable(byte[] buf, ref EIT_Program_Info eitInfo,int lastSection,bool flag)
		{
			int table_id;
			int section_syntax_indicator;
			int section_length;
			int service_id;
			int version_number;
			int current_next_indicator;
			int section_number=0;
			int last_section_number;
			int transport_stream_id;
			int original_network_id;
			int segment_last_section_number;
			int last_table_id;
			int event_id;
			long start_time_MJD;
			long start_time_UTC;
			long duration;
			int running_status;
			int free_CA_mode;
			int descriptors_loop_length;
			int indicator;
			int len1;
			int len2;
			int x;

			try
			{
				if (buf.Length < 14)
				{
					return 0;
				}
				table_id = buf[0];
				section_syntax_indicator = (buf[1]>>7) & 1;
				section_length = ((buf[1]& 0xF)<<8) + buf[2];
				service_id = (buf[3]<<8)+buf[4];
				version_number = ((buf[5]>>1)&0x1F);
				current_next_indicator = buf[5] & 1;
				section_number = buf[6];
				last_section_number = buf[7];
				transport_stream_id = (buf[8]<<8)+buf[9];
				original_network_id = (buf[10]<<8)+buf[11];
				segment_last_section_number = buf[12];
				last_table_id = buf[13];
				//
				if (service_id == 0xFFFF) // scrambled
				{
					return 0;
				}

				len1 = section_length - 11;
				int pointer = 14;
				//
				//
				while (len1 > 4)
				{
					System.Windows.Forms.Application.DoEvents();
					EITDescr eit=new EITDescr();
					event_id = (buf[pointer]<<8)+buf[pointer+1];
					start_time_MJD = (buf[pointer+2]<<8)+buf[pointer+3];
					start_time_UTC = (buf[pointer+4]<<16)+(buf[pointer+5]<<8)+buf[pointer+6];
					duration = (buf[pointer+7]<<16)+(buf[pointer+8]<<8)+buf[pointer+9];
					running_status = (buf[pointer+10]>>5)&7;
					free_CA_mode = (buf[pointer+10]>>4)& 1;
					descriptors_loop_length = ((buf[pointer+10]&0xF)<<8)+buf[pointer+11];

					pointer += 12;
					len1 -= 12 + descriptors_loop_length;
					len2 = descriptors_loop_length;
				
					while (len2 > 0)
					{
						indicator = buf[pointer];
						x = buf[pointer + 1] + 2;
						byte[] descrEIT = new byte[x + 1];
						try
						{
							try
							{
								System.Array.Copy(buf, pointer, descrEIT, 0, x);
								switch (indicator)
								{
									case 0x4E:
										//Log.Write("dvbsection: extended event found...");
										DVB_ExtendedEvent(descrEIT,ref eit);
										break;
									case 0x4D:
										//Log.Write("dvbsection: short event found...");
										DVB_ShortEvent(descrEIT,ref eit);
										break;
									case 0x54:
										DVB_ContentDescription(descrEIT,ref eit);
										break;
							
								}
							}
							catch
							{
								// ignore on error
							}
						}
						catch(Exception)
						{
							//Log.WriteFile(Log.LogType.Capture,true,"dvbsection: exception on EIT: {0} {1} {2}",ex.Message,ex.StackTrace,ex.Source);
						}
						System.Windows.Forms.Application.DoEvents();

						eit.section=section_number;
						eit.lastSection=last_section_number;
						eit.table=table_id;
						eit.lastTable=last_table_id;
						eit.program_number=service_id;
						eit.org_network_id=original_network_id;
						eit.ts_id=transport_stream_id;
						eit.starttime_y=0;
						eit.starttime_d=0;
						eit.starttime_m=0;
						eit.event_id = event_id;
						eit.version = version_number;
						eit.duration_hh = getUTC((int) ((duration >> 16) )& 255);
						eit.duration_mm = getUTC((int) ((duration >> 8) )& 255);
						eit.duration_ss = getUTC((int) (duration )& 255);
						eit.starttime_hh = getUTC((int) ((start_time_UTC >> 16) )& 255);
						eit.starttime_mm =getUTC((int) ((start_time_UTC >> 8) )& 255);
						eit.starttime_ss =getUTC((int) (start_time_UTC )& 255);


						// convert the julian date
						int year = (int) ((start_time_MJD - 15078.2) / 365.25);
						int month = (int) ((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
						int day = (int) (start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
						int k = (month == 14 || month == 15) ? 1 : 0;
						year += 1900+ k; // start from year 1900, so add that here
						month = month - 1 - k * 12;
						eit.starttime_y=year;
						eit.starttime_m=month;
						eit.starttime_d=day;

						DateTime dtUTC = new DateTime(eit.starttime_y,eit.starttime_m,eit.starttime_d,
																					eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,0);
						DateTime dtLocal=dtUTC.ToLocalTime();
						eit.starttime_hh=dtLocal.Hour;
						eit.starttime_mm=dtLocal.Minute;
						eit.starttime_ss=dtLocal.Second;
						eit.starttime_y=dtLocal.Year;
						eit.starttime_m=dtLocal.Month;
						eit.starttime_d=dtLocal.Day;

						eitInfo.program_id = service_id;
						eitInfo.running_status = running_status;
						if (free_CA_mode == 0)
						{
							eitInfo.scrambled = false;
						}
						else
						{
							eitInfo.scrambled = true;
						}
						eit.handled=true;
				
						pointer += x;
						len2 -= x;
					}
					if(eit.program_number>0)
						eitInfo.eitList.Add(eit);
				}
				//eitInfo.evt_info_act_ts=eit;
				System.Windows.Forms.Application.DoEvents();
				if(section_number==0 && lastSection==last_section_number && flag==false)
					return -1;// start grab
				if(section_number==0 && lastSection==last_section_number && flag==true)
					return -2;// end grab
			}
			catch
			{
				// empty. data error found
			}
			return section_number; // normal grab
		}


		#endregion

		#region Descriptors
		//
		private void DVB_GetLogicalChannelNumber(byte[] b, ref ArrayList servicesArray)
		{
			try
			{
				// 32 bits per record
				int n = b[1] / 4;
				if (n < 1)
					return;

				// desc id, desc len, (service id, service number)
				byte[] descriptors = new byte[b[1]];
				Array.Copy(b, 2, descriptors, 0, b[1]);

				int ServiceID, LCN;
				byte[] buf = new byte[4];
				for (int i = 0; i < n; i++) 
				{
					ServiceID = 0;
					LCN = 0;
					//Log.Write("loop count: {0}", i);
					Array.Copy(descriptors, (i * 4), buf, 0, 4);
					ServiceID = (buf[0]<<8)|(buf[1]&0xff);
					LCN = (buf[2]&0x03<<8)|(buf[3]&0xff);
					//Log.Write("Service {0} has channel number {1}", myService.SID, myService.LCN);
					for (int j = 0; j < servicesArray.Count; j++)
					{
						ChannelInfo info = (ChannelInfo)servicesArray[j];
						if (info.serviceID == ServiceID)
						{
							info.program_number = LCN;
							servicesArray[j] = info;
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Write("{0} {1}", e.Message, e.StackTrace);
			}
		}
		//
		private string DVB_GetTeletextDescriptor (byte[] b)
		{
			int      descriptor_tag;
			int      descriptor_length;		
			string   ISO_639_language_code="";
			int      teletext_type;
			int      teletext_magazine_number;
			int      teletext_page_number;
			int      len;
			if (b.Length<2) return String.Empty;
			descriptor_tag		 = b[0];
			descriptor_length  = b[1];

			len = descriptor_length;
			byte[] bytes=new byte[len+1];
			if(len<b.Length+2)
				if (descriptor_tag==0x56)
				{
					int pointer  = 2;

					while ( len > 0 && (pointer+3<=b.Length)) 
					{
						System.Array.Copy(b,pointer,bytes,0,3);
						ISO_639_language_code+=System.Text.Encoding.ASCII.GetString(bytes,0,3);
						teletext_type		= (bytes[3]>>3) & 0x1F;
						teletext_magazine_number	= bytes[3] & 7;
						teletext_page_number	= bytes[4];
						pointer += 5;
						len -= 5;
					}
				}
			if(ISO_639_language_code.Length>=3)
				return ISO_639_language_code.Substring(0,3);
			return "";
		}
		// ca
		//
		string DVB_CADescriptor (byte[] b)
		{

			int      descriptor_tag;
			int      descriptor_length;		
			int      CA_system_ID;
			int      CA_PID;
			string   CA_Text="";

			byte[] data=new byte[10]{0,0,0,0,0,0,0,0,0,0};

			if(b==null)
				return "";
			if(b.Length==0)
				return "";
			if(b[0]!=0x09)
				return "";

			int dataLen=b.Length;

			if(b.Length>10)
				dataLen=10;

			System.Array.Copy(b,0,data,0,dataLen);

			descriptor_tag= data[0];
			descriptor_length= data[1];

			CA_system_ID= (data[2]<<8)+data[3];
			CA_PID= (((data[0x4]<<16)+(data[5]<<8)+data[6]) ^ 0xE00000)>>8;
			CA_Text=CA_PID.ToString()+"/"+CA_system_ID.ToString();

			return CA_Text;

		}
		//
		private string DVB_GetMPEGISO639Lang (byte[] b)

		{

			int		descriptor_tag;
			int     descriptor_length;		
			string  ISO_639_language_code="";
			int     audio_type;
			int     len;

			descriptor_tag= b[0];
			descriptor_length= b[1];
			if(descriptor_length<b.Length)
				if(descriptor_tag==0xa)
				{
					len = descriptor_length;
					byte[] bytes=new byte[len+1];

					int pointer= 2;

					while ( len > 0) 
					{
						System.Array.Copy(b,pointer,bytes,0,len);
						ISO_639_language_code+=System.Text.Encoding.ASCII.GetString(bytes,0,3);
						if(bytes.Length>=4)
							audio_type = bytes[3];
						pointer += 4;
						len -= 4;
					}
				}
			
			return ISO_639_language_code;
		}
		private bool DVB_GetAC3Audio(byte[] b)

		{

			int      descriptor_tag;
			int      descriptor_length;
			int      component_type_flag;
			int      bsid_flag;
			int      mainid_flag;
			int      asvc_flag;
			int      reserved_1;
			int      component_type=0;
			//			int      bsid_type=0;
			//			int      mainid_type=0;
			//			int      asvc_type=0;
			int      len;



			descriptor_tag		= b[0];
			descriptor_length	= b[1];

			component_type_flag=(b[2]>>7) & 1;
			bsid_flag			= (b[2]>>6) & 1;
			mainid_flag		= (b[2]>>5)&1;
			asvc_flag			= (b[2]>>4) & 1;
			reserved_1			= b[2] & 0xF;

			int pointer=3 ;
			len  = descriptor_length - 2;

			if (component_type_flag!=0) 
			{
				component_type	= b[pointer];
				pointer++;
				len--;
			}

			if (bsid_flag!=0) 
			{
				pointer++;
				//				bsid_flag	= b[pointer];
				//				len--;
			}

			if (mainid_flag!=0) 
			{
				pointer++;
				//				mainid_flag	= b[pointer];
				//				len--;
			}

			if (asvc_flag!=0) 
			{
				pointer++;
				//				asvc_flag	= b[pointer];
				//				len--;
			}
			if((component_type & 0x4)!=0)// multichannel
				return true;

			return false;

		}
		//
		// cat

		//
		//
		private void DVB_ShortEvent(byte[] buf, ref EITDescr eit)
		{
			int descriptor_tag;
			int descriptor_length;
			int event_name_length;
			int text_length;
			byte[] b = new byte[4097];
			
			descriptor_tag = buf[0];
			descriptor_length = buf[1];
			eit.seLanguageCode=System.Text.Encoding.ASCII.GetString(buf,2,3);
			if(eit.seLanguageCode.Length>0)
			{
				//Log.WriteFile(Log.LogType.Capture,"epg-grab: language={0}", eit.seLanguageCode);
			}
			eit.event_name = "";
			eit.event_text = "";
			if (descriptor_tag == 0x4D)
			{
				event_name_length = buf[5];
				int pointer = 6;
				try
				{
					System.Array.Copy(buf, pointer, b, 0, event_name_length);
					eit.event_name = getString468A(b, event_name_length);
					pointer += event_name_length;
					text_length = buf[pointer];
					pointer += 1;
					System.Array.Copy(buf, pointer, b, 0, buf.Length - pointer);
					eit.event_text = getString468A(b, text_length);
				}
				catch(Exception)
				{
					//Log.WriteFile(Log.LogType.Capture,true,"dvbsections: short-event exception={0} stack={1} source={2}",ex.Message,ex.StackTrace,ex.Source);
					eit.event_text="";
					eit.event_name="";
				}
				if(eit.event_name.Length>0 && eit.event_text.Length>0)
					eit.shortEventComplete=true;

			}
		}
		private void DVB_ContentDescription(byte[] buf, ref EITDescr eit)
		{
			int      descriptor_tag;
			int      descriptor_length;		
			int      content_nibble_level_1;
			int      content_nibble_level_2;
			int      user_nibble_1;
			int      user_nibble_2;
			int nibble=0;
			string genereText="";
			int           len;
			byte[]	b=new byte[2];


			descriptor_tag		 = buf[0];
			descriptor_length       	 = buf[1];


			len = descriptor_length;
			int pointer=  2;
			if(descriptor_tag==0x54)
				while ( len > 0) 
				{

					System.Array.Copy(buf,pointer,b,0,2);
					content_nibble_level_1	 = (b[0]>>4) & 0xF;
					content_nibble_level_2	 = b[0] & 0xF;
					user_nibble_1		 = (b[1]>>4) & 0xF;
					user_nibble_2		 = b[1] & 0xF;

					pointer   += 2;
					len -= 2;
					genereText="";
					nibble=(content_nibble_level_1 << 8) | content_nibble_level_2;
					switch(nibble)
					{
						case 0x0100: genereText="movie/drama (general)" ;break;
						case 0x0101: genereText="detective/thriller" ;break;
						case 0x0102: genereText="adventure/western/war" ;break;
						case 0x0103: genereText="science fiction/fantasy/horror" ;break;
						case 0x0104: genereText="comedy" ;break;
						case 0x0105: genereText="soap/melodram/folkloric" ;break;
						case 0x0106: genereText="romance" ;break;
						case 0x0107: genereText="serious/classical/religious/historical movie/drama" ;break;
						case 0x0108: genereText="adult movie/drama" ;break;

						case 0x010E: genereText="reserved" ;break;
						case 0x010F: genereText="user defined" ;break;

							// News Current Affairs
						case 0x0200: genereText="news/current affairs (general)" ;break;
						case 0x0201: genereText="news/weather report" ;break;
						case 0x0202: genereText="news magazine" ;break;
						case 0x0203: genereText="documentary" ;break;
						case 0x0204: genereText="discussion/interview/debate" ;break;
						case 0x020E: genereText="reserved" ;break;
						case 0x020F: genereText="user defined" ;break;

							// Show Games show
						case 0x0300: genereText="show/game show (general)" ;break;
						case 0x0301: genereText="game show/quiz/contest" ;break;
						case 0x0302: genereText="variety show" ;break;
						case 0x0303: genereText="talk show" ;break;
						case 0x030E: genereText="reserved" ;break;
						case 0x030F: genereText="user defined" ;break;

							// Sports
						case 0x0400: genereText="sports (general)" ;break;
						case 0x0401: genereText="special events" ;break;
						case 0x0402: genereText="sports magazine" ;break;
						case 0x0403: genereText="football/soccer" ;break;
						case 0x0404: genereText="tennis/squash" ;break;
						case 0x0405: genereText="team sports" ;break;
						case 0x0406: genereText="athletics" ;break;
						case 0x0407: genereText="motor sport" ;break;
						case 0x0408: genereText="water sport" ;break;
						case 0x0409: genereText="winter sport" ;break;
						case 0x040A: genereText="equestrian" ;break;
						case 0x040B: genereText="martial sports" ;break;
						case 0x040E: genereText="reserved" ;break;
						case 0x040F: genereText="user defined" ;break;

							// Children/Youth
						case 0x0500: genereText="childrens's/youth program (general)" ;break;
						case 0x0501: genereText="pre-school children's program" ;break;
						case 0x0502: genereText="entertainment (6-14 year old)" ;break;
						case 0x0503: genereText="entertainment (10-16 year old)" ;break;
						case 0x0504: genereText="information/education/school program" ;break;
						case 0x0505: genereText="cartoon/puppets" ;break;
						case 0x050E: genereText="reserved" ;break;
						case 0x050F: genereText="user defined" ;break;

						case 0x0600: genereText="music/ballet/dance (general)" ;break;
						case 0x0601: genereText="rock/pop" ;break;
						case 0x0602: genereText="serious music/classic music" ;break;
						case 0x0603: genereText="folk/traditional music" ;break;
						case 0x0604: genereText="jazz" ;break;
						case 0x0605: genereText="musical/opera" ;break;
						case 0x0606: genereText="ballet" ;break;
						case 0x060E: genereText="reserved" ;break;
						case 0x060F: genereText="user defined" ;break;

						case 0x0700: genereText="arts/culture (without music, general)" ;break;
						case 0x0701: genereText="performing arts" ;break;
						case 0x0702: genereText="fine arts" ;break;
						case 0x0703: genereText="religion" ;break;
						case 0x0704: genereText="popular culture/traditional arts" ;break;
						case 0x0705: genereText="literature" ;break;
						case 0x0706: genereText="film/cinema" ;break;
						case 0x0707: genereText="experimental film/video" ;break;
						case 0x0708: genereText="broadcasting/press" ;break;
						case 0x0709: genereText="new media" ;break;
						case 0x070A: genereText="arts/culture magazine" ;break;
						case 0x070B: genereText="fashion" ;break;
						case 0x070E: genereText="reserved" ;break;
						case 0x070F: genereText="user defined" ;break;

						case 0x0800: genereText="social/political issues/economics (general)" ;break;
						case 0x0801: genereText="magazines/reports/documentary" ;break;
						case 0x0802: genereText="economics/social advisory" ;break;
						case 0x0803: genereText="remarkable people" ;break;
						case 0x080E: genereText="reserved" ;break;
						case 0x080F: genereText="user defined" ;break;

						case 0x0900: genereText="education/science/factual topics (general)" ;break;
						case 0x0901: genereText="nature/animals/environment" ;break;
						case 0x0902: genereText="technology/natural science" ;break;
						case 0x0903: genereText="medicine/physiology/psychology" ;break;
						case 0x0904: genereText="foreign countries/expeditions" ;break;
						case 0x0905: genereText="social/spiritual science" ;break;
						case 0x0906: genereText="further education" ;break;
						case 0x0907: genereText="languages" ;break;
						case 0x090E: genereText="reserved" ;break;
						case 0x090F: genereText="user defined" ;break;
						case 0x0A00: genereText="leisure hobbies (general)" ;break;
						case 0x0A01: genereText="tourism/travel" ;break;
						case 0x0A02: genereText="handicraft" ;break;
						case 0x0A03: genereText="motoring" ;break;
						case 0x0A04: genereText="fitness & health" ;break;
						case 0x0A05: genereText="cooking" ;break;
						case 0x0A06: genereText="advertisement/shopping" ;break;
						case 0x0A07: genereText="gardening" ;break;
						case 0x0A0E: genereText="reserved" ;break;
						case 0x0A0F: genereText="user defined" ;break;

						case 0x0B00: genereText="original language" ;break;
						case 0x0B01: genereText="black & white" ;break;
						case 0x0B02: genereText="unpublished" ;break;
						case 0x0B03: genereText="live broadcast" ;break;
						case 0x0B0E: genereText="reserved" ;break;
						case 0x0B0F: genereText="user defined" ;break;

						case 0x0E0F: genereText="reserved" ;break;
						case 0x0F0F: genereText="user defined" ;break;					

					}
					if(eit.genere_text==null)
						eit.genere_text="";
					if(eit.genere_text=="")
						eit.genere_text=genereText;
				}
		}
		//
		string DVB_SubtitleDescriptior(byte[] buf)
		{
			int		descriptor_tag;
			int     descriptor_length;		
			string  ISO_639_language_code="";
			int		subtitling_type	;
			int		composition_page_id;
			int		ancillary_page_id;
			int     len;

			descriptor_tag= buf[0];
			descriptor_length= buf[1];
			if(descriptor_length<buf.Length)
				if(descriptor_tag==0x59)
				{
					len = descriptor_length;
					byte[] bytes=new byte[len+1];

					int pointer= 2;

					while ( len > 0) 
					{
						System.Array.Copy(buf,pointer,bytes,0,len);
						ISO_639_language_code+=System.Text.Encoding.ASCII.GetString(bytes,0,3);
						if(bytes.Length>=4)
							subtitling_type = bytes[3];
						if(bytes.Length>=6)
							composition_page_id = (bytes[4]<<8)+bytes[5];
						if(bytes.Length>=8)
							ancillary_page_id = (bytes[6]<<8)+bytes[7];
						
						pointer += 8;
						len -= 8;
					}
				}
			
			return ISO_639_language_code;		
		}
		//
		private object DVB_ExtendedEvent(byte[] buf, ref EITDescr eit)
		{
			int descriptor_tag;
			int descriptor_length;
			int descriptor_number;
			int last_descriptor_number;
			//int event_name_length;
			int text_length;
			int length_of_items;
			//string event_Name;
			byte[] b = new byte[4097];
			byte[] data = new byte[8];
			string text = "";
			int pointer = 0;
			int lenB;
			int len1;
			int item_description_length;
			int item_length;
			string item = "";
			try
			{
				System.Array.Copy(buf, 0, data, 0, 7);
			
				descriptor_tag = data[0];
				descriptor_length = data[1];
				descriptor_number = (data[1]>>4) & 0xF;
				last_descriptor_number = data[1] & 0xF;
				eit.eeLanguageCode=System.Text.Encoding.ASCII.GetString(data,3,3);
				length_of_items = data[6];

				if(eit.eeLanguageCode.Length>0)
				{
					//Log.WriteFile(Log.LogType.Capture,"epg-grab: language={0}", eit.eeLanguageCode);
				}
				pointer += 7;
				lenB = descriptor_length - 5;
				len1 = length_of_items;
			
				while (len1 > 0)
				{
					System.Array.Copy(buf, pointer, b, 0, lenB - pointer);
					item_description_length = b[0];
					pointer += 1 + item_description_length;
					System.Array.Copy(buf, pointer, b, 0, lenB - pointer);
					string testText=getString468A(b, item_description_length);
					if (testText==null)
						testText="-not avail.-";
					//Log.WriteFile(Log.LogType.Capture,"dvbsections: item-description={0}",testText);
					item_length = b[0];
					System.Array.Copy(buf, pointer + 1, b, 0, item_length);
					item = getString468A(b, item_length);
					pointer += 1 + item_length;
					len1 -= (2 + item_description_length + item_length);
					lenB -= (2 + item_description_length + item_length);
				}
				System.Array.Copy(buf, pointer, b, 0, 1);
				text_length = b[0];
				pointer += 1;
				lenB -= 1;
				System.Array.Copy(buf, pointer, b, 0, text_length);
				text = getString468A(b, text_length);
				eit.event_item += item;
				eit.event_item_text += text;
			}
			catch(Exception)
			{
				//Log.WriteFile(Log.LogType.Capture,true,"dvbsections: extended-event exception={0} stack={1} source={2}",ex.Message,ex.StackTrace,ex.Source);
			}
			if(eit.event_item==null)
				eit.event_item="";
			if(eit.event_item_text==null)
				eit.event_item_text="";

			if(eit.event_item.Length>0 && eit.event_item_text.Length>0)
				eit.extendedEventComplete=true;

			return 0;
		}

		//
		private void DVB_GetSatDelivSys(byte[] b, ref NITSatDescriptor tp)
		{
			if(b[0]==0x43 && b.Length>=13)
			{
				int descriptor_tag = b[0];
				int descriptor_length = b[1];
				tp.Frequency= (b[2]<<24)+(b[3]<<16)+(b[4]<<8)+b[5];
				tp.OrbitalPosition = (b[6]<<8)+b[7];
				tp.WestEastFlag = (b[8] & 0x80)>>7;
				tp.Polarisation = (b[8]& 0x60)>>5;
				if(tp.Polarisation>1)
					tp.Polarisation-=2;
				// polarisation
				// 0 - horizontal/left (linear/circluar)
				// 1 - vertical/right (linear/circluar)
				tp.Modulation = (b[8] & 0x1F);
				tp.Symbolrate = (b[9]<<24)+(b[10]<<16)+(b[11]<<8)+(b[12]>>4);
				tp.FECInner = (b[12] & 0xF);
				// change hex to int for freq & symbolrate
				string valString="";
				valString=Convert.ToString(tp.Frequency,16);
				if(valString.Length>5)
					valString=valString.Substring(0,5);
				tp.Frequency=Convert.ToInt32(valString);
				valString=Convert.ToString(tp.Symbolrate,16);
				if(valString.Length>5)
					valString=valString.Substring(0,5);
				tp.Symbolrate=Convert.ToInt32(valString);
			}
			
		}
		private void DVB_GetCableDelivSys(byte[] b, ref NITCableDescriptor tp)
		{
			if(b[0]==0x44 && b.Length>=13)
			{
				int descriptor_tag = b[0];
				int descriptor_length = b[1];
				tp.Frequency= (b[2]<<24)+(b[3]<<16)+(b[4]<<8)+b[5];
				//
				tp.FECOuter = (b[7] & 0xF);
				// fec-outer
				// 0- not defined
				// 1- no outer FEC coding
				// 2- RS(204/188)
				// other reserved
				tp.Modulation = b[8];
				// modulation
				// 0x00 not defined
				// 0x01 16-QAM
				// 0x02 32-QAM
				// 0x03 64-QAM
				// 0x04 128-QAM
				// 0x05 256-QAM
				tp.Symbolrate = (b[9]<<24)+(b[10]<<16)+(b[11]<<8)+(b[12]>>4);
				//
				tp.FECInner = (b[12] & 0xF);
				// fec inner
				// 0- not defined
				// 1- 1/2 conv. code rate
				// 2- 2/3 conv. code rate
				// 3- 3/4 conv. code rate
				// 4- 5/6 conv. code rate
				// 5- 7/8 conv. code rate
				// 6- 8/9 conv. code rate
				// 15- No conv. coding
			}
			
		}
		// terrestrial
		private void DVB_GetTerrestrialDelivSys(byte[] b, ref NITTerrestrialDescriptor tp)
		{
			if(b[0]==0x5A)
			{
				int descriptor_tag = b[0];
				int descriptor_length = b[1];
				tp.CentreFrequency= (b[2]<<24)+(b[3]<<16)+(b[4]<<8)+b[5];
				tp.Bandwidth = (b[6]>>5);
				// bandwith
				// 0- 8 MHz
				// 1- 7 MHz
				// 2- 6 MHz
				tp.Constellation=(b[7]>>6);
				// constellation
				// 0- QPSK
				// 1- 16-QAM
				// 2- 64-QAM
				tp.HierarchyInformation=(b[7]>>3)& 7;
				// 0- non-hierarchical
				// 1- a == 1
				// 2- a == 2
				// 3- a == 4
				tp.CoderateHPStream=(b[7] & 7);
				tp.CoderateLPStream=(b[8]>>5);
				// coderate (fec)
				// 0- 1/2
				// 1- 2/3
				// 2- 3/4
				// 3- 5/6
				// 4- 7/8
				// Coderate: The code_rate is a 3-bit field specifying the inner FEC scheme used according to table 43. Non-hierarchical
				// channel coding and modulation requires signalling of one code rate. In this case, 3 bits specifying code_rate according
				// to table 44 are followed by another 3 bits of value '000". Two different code rates may be applied to two different levels
				// of modulation with the aim of achieving hierarchy. Transmission then starts with the code rate for the HP level of the
				// modulation and ends with the one for the LP level.
				tp.GuardInterval=(b[8]>>3) & 3;
				// 0 - 1/32
				// 1 - 1/16
				// 2 - 1/8
				// 3 - 1/4
				//
				tp.TransmissionMode=(b[8]>>1) & 3;
				// 0 - 2k Mode
				// 1 - 8k Mode
				tp.OtherFrequencyFlag=(b[8] & 3);
				// 0 - no other frequency in use
			}
			
		}		//

		private ServiceData DVB_GetService(byte[] b)
		{
			int descriptor_tag;
			int descriptor_length;
			int service_provider_name_length;
			int service_name_length;
			int pointer = 0;
			ServiceData serviceData=new ServiceData();
			descriptor_tag = b[0];
			descriptor_length = b[1];
			serviceData.serviceType = b[2];
			service_provider_name_length = b[3];
			pointer = 4;
			byte[] spn = new byte[b.Length-pointer + 1];
			System.Array.Copy(b, pointer, spn, 0, b.Length - pointer);
			serviceData.serviceProviderName = getString468A(spn, service_provider_name_length);
			pointer += service_provider_name_length;
			service_name_length = b[pointer];
			pointer += 1;
			byte[] sn = new byte[b.Length-pointer + 1];
			System.Array.Copy(b, pointer, sn, 0, b.Length - pointer);
			serviceData.serviceName = getString468A(sn, service_name_length);
			return serviceData;
		}
		static public  string getString468A(byte[] b, int l1)
		{
			//			int in_emphasis = 0;
			int i = 0;
			char c;
			char em_ON = (char)0x86;
			char em_OFF = (char)0x87;
			string text = "";
			//			char c1;
			do
			{
				c = (char)b[i];
				
				if (Convert.ToInt16(c) >= 0x80 & Convert.ToInt16(c) <= 0x9F)
				{
					goto cont;
				}
				if (i == 0 & Convert.ToInt16(c) < 0x20)
				{
					goto cont;
				}
				
				if (c == em_ON)
				{
					//					
					goto cont;
				}
				if (c == em_OFF)
				{
					//					
					goto cont;
				}
				
				if (Convert.ToInt16(c) == 0x84)
				{
					text = text + '\r';
					goto cont;
				}
				
				if (Convert.ToInt16(c) < 0x20)
				{
					goto cont;
				}
				
				text = text + c;
			@cont:
				l1 -= 1;
				i += 1;
			} while (!(l1 <= 0));
			return text;
		}

		//
		//
		//
		#endregion

		#region process nit/pat sections
		//
		//
		public bool ProcessNITSections(DShowNET.IBaseFilter filter,ref DVBNetworkInfo nit)
		{
			GetStreamData(filter,16, 0x40,0,m_timeoutMS);
			foreach(byte[] arr in m_sectionsList)
				decodeNITTable(arr, ref nit);
			return true;
		}

		public bool ProcessNITSections(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,DShowNET.IBaseFilter filter,ref DVBNetworkInfo nit)
		{
			m_setPid=true;
			m_dataControl=dataCtrl;
			GetStreamData(filter,16, 0x40,0,m_timeoutMS);
			foreach(byte[] arr in m_sectionsList)
				decodeNITTable(arr, ref nit);
			
			return true;
		}
		//
		//
		public bool ProcessPATSections(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,DShowNET.IBaseFilter filter,TPList tp,ref Transponder transponder)
		{
			m_setPid=true;
			m_dataControl=dataCtrl;
			int count=0;
			GetStreamData(filter,0, 0,0,m_timeoutMS);
			foreach(byte[] arr in m_sectionsList)
				count=decodePATTable(arr, tp, ref transponder);
			if(count>0)
				LoadPMTTables(filter,tp,ref transponder);
			return true;
		}
		public bool ProcessPATSections(DShowNET.IBaseFilter filter,TPList tp,ref Transponder transponder)
		{
			
			m_setPid=false;
			GetStreamData(filter,0, 0,0,m_timeoutMS);
			foreach(byte[] arr in m_sectionsList)
				decodePATTable(arr, tp, ref transponder);
			LoadPMTTables(filter,tp,ref transponder);
			return true;
		}
		//
		//
		#endregion

		#endregion

		#region EPG
		public ArrayList GetEITPresentFollowing(DShowNET.IBaseFilter filter)
		{
			EIT_Program_Info eit=new EIT_Program_Info();
			eit.eitList=new ArrayList();
			GetStreamData(filter,18,0x4E,0,50);

			foreach(byte[] arr in m_sectionsList)
				decodeEITTable(arr,ref eit,0,false);
			return eit.eitList;
		}

		//
		// returns events found in tables >=0x50 && <=0x6f 
		// lastTab holds the last table
		//
		public ArrayList GetEITSchedule(int tab,DShowNET.IBaseFilter filter,ref int lastTab)
		{
			if(tab<0x50 || tab>0x6f)
				return null;
			EIT_Program_Info eit=new EIT_Program_Info();
			eit.eitList=new ArrayList();
			EITDescr descr=new EITDescr();

			bool startFlag=false;
			bool endFlag=false;
			int ret=-1;
			m_breakAction=false;
			m_eitTimeoutTimer.Interval=10000;// one minute
			m_eitTimeoutTimer.Start();
			while(1!=0)
			{
				System.Windows.Forms.Application.DoEvents();
				GetStreamData(filter,18,tab,1,m_timeoutMS);
				
				foreach(byte[] arr in m_sectionsList)
					ret=decodeEITTable(arr,ref eit,ret,startFlag);
				
				if(eit.eitList.Count>0)
				{
					descr=(EITDescr)eit.eitList[0];
					lastTab=descr.lastTable;
					//Log.WriteFile(Log.LogType.Capture,"epg-grab: last Table={0}",lastTab);
					//Log.WriteFile(Log.LogType.Capture,"epg-grab: last section={0}",descr.lastSection);
				}
				
				if(ret==-1)
				{
					
					startFlag=true;
					//Log.WriteFile(Log.LogType.Capture,"epg-grab: start grabbing table");
					m_eitTimeoutTimer.Start();
				}

				if(ret==-2)
				{
					endFlag=true;
					//Log.WriteFile(Log.LogType.Capture,"epg-grab: end grabbing table");
					m_eitTimeoutTimer.Start();

				}
				
				if(ret>=0 && startFlag==true)
					//Log.WriteFile(Log.LogType.Capture,"epg-grab: grab section {0} complete",ret);

				if(startFlag==false)
				{
					eit.eitList.Clear();
				}

				if(startFlag==true && endFlag==true)
					break;
				
				
				if(m_breakAction==true)
				{
					Log.WriteFile(Log.LogType.Capture,"epg-grab: FAILED timeout on getting epg");
					break;
				}


			}

			return eit.eitList;
		}
		public ArrayList GetEITSchedule(ArrayList epgData)
		{
			if(epgData==null)
				return null;
			if(epgData.Count<1)
				return null;

			EIT_Program_Info eit=new EIT_Program_Info();
			eit.eitList=new ArrayList();
			EITDescr descr=new EITDescr();
			bool startFlag=false;
			int ret=-1;

			foreach(byte[] arr in epgData)
			{
				System.Windows.Forms.Application.DoEvents();
				System.Windows.Forms.Application.DoEvents();
				ret=decodeEITTable(arr,ref eit,ret,startFlag);
				System.Windows.Forms.Application.DoEvents();
				System.Windows.Forms.Application.DoEvents();
			}
			return eit.eitList;
		}
		#endregion

		#region Scanning
		//
		//

		/// <summary>
		/// Get all information about channels & services 
		/// </summary>
		/// <param name="filter">IBase filter implementing IMpeg2Data</param>
		/// <returns>Transponder object containg all channels/services found</returns>
		public Transponder Scan(DShowNET.IBaseFilter filter)
		{
			m_sectionsList=new ArrayList();	
			Transponder transponder = new Transponder();
			if(m_setPid==true)
			{
				DVBSkyStar2Helper.DeleteAllPIDs((DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3)m_dataCtrl,0);
				DVBSkyStar2Helper.SetPidToPin((DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3)m_dataCtrl,0,0);
				DVBSkyStar2Helper.SetPidToPin((DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3)m_dataCtrl,0,16);
				DVBSkyStar2Helper.SetPidToPin((DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3)m_dataCtrl,0,17);
				Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: pids set");
			}
			try
			{
				Log.Write("AutoTune()");

				transponder.channels = new ArrayList();
				transponder.PMTTable = new ArrayList();

				//get Program Assocation Table (PAT) (pid=0x0)
				GetStreamData(filter,0, 0,0,Timeout);
				
				//The PAT contains a list of services
				//for each services it specifies the transport stream id, program number and network PMT pid
				foreach(byte[] patTable in m_sectionsList)
					decodePATTable(patTable, transp[0], ref transponder);

				if (transponder.PMTTable.Count>0)
				{
					Log.Write("DVBSections: decoding PAT delivered {0} services",transponder.PMTTable.Count);
				}
				else return transponder;

				// now that we got the network PMT (Program Map Table) pids for all services
				// load & decode the PMT tables themselves
				LoadPMTTables(filter,transp[0],ref transponder);

				// Fill in Logical Channel Numbers
				//Log.Write("Number of channels before processing NIT: {0}", transponder.channels.Count);
				//GetStreamData(filter, 16, 0x40, 0, Timeout);
				//foreach(byte[] arr in m_sectionsList)
				//	decodeNITTable(arr, ref transponder);
				
				Log.Write("AutoTune() done");
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"dvbsections:Scan() exception:{0}", ex.ToString());
			}
			return transponder;
		}//public Transponder Scan(DShowNET.IBaseFilter filter)
		

		public DVBChannel GetDVBChannel(DShowNET.IBaseFilter filter, int serviceId)
		{
			Log.WriteFile(Log.LogType.Capture,"DVBSections.GetDVBChannel for service:{0}", serviceId);
			DVBChannel chan = new DVBChannel();

			ArrayList pmtTable=new ArrayList();
			try
			{
				m_sectionsList=new ArrayList();	
				Transponder transponder = new Transponder();
				transponder.channels = new ArrayList();
				transponder.PMTTable = new ArrayList();
				GetStreamData(filter,0, 0,0,5000);
				if (m_sectionsList.Count==0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBSections.GetDVBChannel no sections found");
					return null;
				}
				// jump to parser
				foreach(byte[] arr in m_sectionsList)
					decodePATTable(arr, transp[0], ref transponder);

				LoadPMTTables(filter,transp[0],ref transponder);
				int t;
				int n;
				ArrayList	tab42=new ArrayList();
				ArrayList	tab46=new ArrayList();

				// check tables
				//AddTSPid(17);
				//
				
				Debug.WriteLine("GET tab42");
				GetStreamData(filter,17, 0x42,0,200);
				tab42=(ArrayList)m_sectionsList.Clone();
				
				Debug.WriteLine("GET tab46");
				GetStreamData(filter,17, 0x46,0,200);
				tab46=(ArrayList)m_sectionsList.Clone();

				
				//bool flag=false;
				ChannelInfo pat;
				ArrayList pmtList = transponder.PMTTable;
				int pmtScans;
				pmtScans = (pmtList.Count / 20) + 1;
				
				for (t = 1; t <= pmtScans; t++)
				{
					//flag = DeleteAllPIDsI();
					for (n = 0; n <= 19; n++)
					{
						if (((t - 1) * 20) + n > pmtList.Count - 1)
						{
							break;
						}
						pat = (ChannelInfo) pmtList[((t - 1) * 20) + n];
						
						// parse pmt
						int res=0;
						GetStreamData(filter,pat.network_pmt_PID, 2,0,5000); // get here the pmt
						foreach(byte[] wdata in m_sectionsList)
						{
							res=decodePMTTable(wdata, transp[0], transponder,ref pat);
						}

						if(res>0)
						{

							foreach(byte[] wdata in tab42)
								decodeSDTTable(wdata, transp[0],ref transponder,ref pat);

							foreach(byte[] wdata in tab46)
								decodeSDTTable(wdata, transp[0],ref transponder,ref pat);
						}
						transponder.channels.Add(pat);
					}
				}

				foreach (ChannelInfo chanInfo in transponder.channels)
				{
					if (chanInfo.serviceID==serviceId)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBSections.GetDVBChannel found channel details");
						for (int pids =0; pids < chanInfo.pid_list.Count;pids++)
						{
							DVBSections.PMTData data=(DVBSections.PMTData) chanInfo.pid_list[pids];
							if (data.isVideo)
								chan.VideoPid=data.elementary_PID;
							if (data.isAC3Audio)
								chan.AC3Pid=data.elementary_PID;
							if (data.isTeletext)
								chan.TeletextPid=data.elementary_PID;
							if (data.isAudio)
								chan.AudioPid=data.elementary_PID;
						}
						
						chan.TransportStreamID=chanInfo.transportStreamID;
						chan.Symbolrate=chanInfo.symb;
						chan.ServiceType=chanInfo.serviceType;
						chan.ServiceProvider=chanInfo.service_provider_name;
						chan.ServiceName=chanInfo.service_name;
						chan.ProgramNumber=chanInfo.program_number;
						chan.Polarity=chanInfo.pol;
						chan.PMTPid=chanInfo.network_pmt_PID;
						chan.PCRPid=chanInfo.pcr_pid;
						chan.NetworkID=chanInfo.networkID;
						chan.LNBKHz=chanInfo.lnb01;
						chan.LNBFrequency=chanInfo.lnbkhz;
						chan.IsScrambled=chanInfo.scrambled;
						chan.ID=1;
						chan.HasEITSchedule=chanInfo.eitSchedule;
						chan.HasEITPresentFollow=chanInfo.eitPreFollow;
						chan.Frequency=chanInfo.freq;
						chan.FEC=chanInfo.fec;
						chan.ECMPid=0;
						chan.DiSEqC=0;
						Log.WriteFile(Log.LogType.Capture,"name:{0} audio:{1:X} video:{2:X} txt:{3:X} EIT:{4} EITPF:{5}",
							chan.ServiceName,chan.AudioPid,chan.VideoPid,chan.TeletextPid,chan.HasEITSchedule,chan.HasEITPresentFollow);
					}
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"dvbsections:GetDVBChannel() exception:{0}", ex.ToString());
			}
			return chan;


		}//public Transponder GetRAWPMT(DShowNET.IBaseFilter filter)
	
		#endregion

		// get current/next info
		//

		private void m_streamDemuxer_OnGotTable(int pid, int tableID, ArrayList tableList)
		{
			//Log.Write("dvbsections:OnGotTable() pid:{0:X} tid:{1:X} count:{2}", pid,tableID,tableList.Count);
			m_syncWait=true;
			if(tableList.Count>0)
			{
			
				try
				{
					m_sectionsList=(ArrayList)tableList.Clone();
				}
				catch
				{
				}
			}

		}
		#region IDisposable Members

		public void Dispose()
		{
			if (m_streamDemuxer!=null)
			{
				m_streamDemuxer.OnGotTable-=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);
				m_streamDemuxer=null;
			}
		}

		#endregion
	}// class
}// namespace
