using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für DVB_SECTIONS.
	/// </summary>
	public class DVBSections
	{
		

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionPtr(int section,ref IntPtr dataPointer,ref int len,ref int header,ref int tableExtId,ref int version,ref int secNum,ref int lastSecNum);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool ReleaseSectionsBuffer();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid,int tid,ref int sectionCount,int tableSection,int timeout);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionDataI(int pid,int tid,ref int sectionCount,int tableSection,int timeout);
		// globals
		TPList[]							transp;
		ArrayList							m_sectionsList;	
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
		//

		//
		public DVBSections()
		{
			m_sectionsList=new ArrayList();	
			transp=new TPList[200];
		}
		//
		public int Timeout
		{
			get{return m_timeoutMS;}
			set{m_timeoutMS=value;}
		}
		// tables
		public struct EITDescr
		{
			public int version;
			public int event_id;
			public string genere_text;
			public string event_item;
			public string event_item_text;
			public string event_name;
			public string event_text;
			public int starttime_y;
			public int starttime_m;
			public int starttime_d;
			public int starttime_hh;
			public int starttime_mm;
			public int starttime_ss;
			public int duration_hh;
			public int duration_mm;
			public int duration_ss;
			public int program_number;
			public int ts_id;
			public int org_network_id;
			public bool handled;
			public int section;
			public int lastSection;
			public int table;
			public int lastTable;
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
		
		private int decodePATTable(byte[] buf, TPList transponderInfo, ref Transponder tp)
		{
			int loop;
			byte[] bytes = new byte[9];
			int n;
			// check
			if(buf.Length<10)
				return 0;

			System.Array.Copy(buf, 0, bytes, 0, 9);
			
			int table_id = bytes[0];
			int section_syntax_indicator = BitsFromBArray(bytes, 0, 8, 1);
			int reserved_1 = BitsFromBArray(bytes, 0, 10, 2);
			int section_length = BitsFromBArray(bytes, 0, 12, 12);
			int transport_stream_id = BitsFromBArray(bytes, 0, 24, 16);
			int reserved_2 = BitsFromBArray(bytes, 0, 40, 2);
			int version_number = BitsFromBArray(bytes, 0, 42, 5);
			int current_next_indicator = BitsFromBArray(bytes, 0, 47, 1);
			int section_number = BitsFromBArray(bytes, 0, 48, 8);
			int last_section_number = BitsFromBArray(bytes, 0, 56, 8);
			
			byte[] b = new byte[5];
			loop =(section_length - 9) / 4;
			//Log.Write("dvbsections:decodePatTable() loop={0}", loop);
			if(loop<1)
				return 0;

			ChannelInfo ch=new ChannelInfo();
			for (int count=0;count<loop;count++)
			{
				System.Array.Copy(buf, 8 +(count * 4), b, 0, 4);
				ch.transportStreamID=transport_stream_id;
				ch.program_number = BitsFromBArray(b, 0, 0, 16);
				ch.reserved = BitsFromBArray(b, 0, 16, 3);
				ch.network_pmt_PID = BitsFromBArray(b, 0, 19, 13);
				//Log.Write("dvbsections:decodePatTable() chan:{0} {1} {2}", ch.transportStreamID,ch.networkID,ch.network_pmt_PID);
				if(ch.program_number!=0)
					tp.PMTTable.Add(ch);
			}
			return loop;
		}
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
			try
			{
				transponder.channels = new ArrayList();
				transponder.PMTTable = new ArrayList();
				GetStreamData(filter,0, 0,0,5000);
				// jump to parser
				foreach(byte[] arr in m_sectionsList)
					decodePATTable(arr, transp[0], ref transponder);

				LoadPMTTables(filter,transp[0],ref transponder);
			}
			catch(Exception ex)
			{
				Log.Write("dvbsections:Scan() exception:{0}", ex.ToString());
			}
			return transponder;
		}//public Transponder Scan(DShowNET.IBaseFilter filter)
		
		/// <summary>
		/// Get all information about channels & services 
		/// </summary>
		/// <param name="filter">[in] IBase filter implementing IMpeg2Data</param>
		/// <param name="serviceId">[in] The service id for which the raw PMT should be returned</param>
		/// <param name="info">[Out] The channel info for the service id</param>
		/// <returns>byte array containing the raw PMT or null if no PMT is found</returns>
		public byte[] GetRAWPMT(DShowNET.IBaseFilter filter, int serviceId, out ChannelInfo info)
		{
			byte[] PMTTable=null;
			info=new ChannelInfo();

			ArrayList sectionTable=new ArrayList();
			try
			{
				m_sectionsList=new ArrayList();	
				Transponder transponder = new Transponder();
				transponder.channels = new ArrayList();
				transponder.PMTTable = new ArrayList();
				Log.Write("dvbSections:GetRAWPMT for channel:{0}",serviceId);
				GetStreamData(filter,0, 0,0,200);
				if (m_sectionsList.Count==0)
				{
					Log.Write("dvbSections:GetRAWPMT() timeout");
					return null;
				}
				//Log.Write("dvbSections:Decode PAT :{0}",m_sectionsList.Count);
				// jump to parser
				foreach(byte[] arr in m_sectionsList)
					decodePATTable(arr, transp[0], ref transponder);

				LoadPMTTables(filter,transp[0],ref transponder);
				bool found=false;
				foreach (ChannelInfo chanInfo in transponder.channels)
				{
					//Log.Write("dvbSections:Got channel:{0} {1}",chanInfo.service_name, chanInfo.serviceID);
					if (chanInfo.serviceID==serviceId)
					{
						Log.Write("dvbSections:GetRAWPMT() found channel:{0} service id:{1} network PMT pid:{2:X} program:{3}",chanInfo.service_name,chanInfo.serviceID,chanInfo.network_pmt_PID,chanInfo.program_number);
						found=true;
						info=chanInfo;
						break;
					}
				}
				if (!found) 
				{
					Log.Write("dvbSections:GetRAWPMT() channel not found");
					return null;
				}
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
				
				Log.Write("dvbSections: PMT table list:{0} pmtScans:{1}", pmtList.Count,pmtScans);
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
						Log.Write("dvbSections.Get PMT pid:{0:X}",pat.network_pmt_PID);
						GetStreamData(filter,pat.network_pmt_PID, 2,0,200); // get here the pmt
						foreach(byte[] wdata in m_sectionsList)
						{
							if (pat.program_number==serviceId)
							{
								Log.Write("dvbsections:service id:{0} program:{1} PMT pid:{2:X} length:{3}",pat.serviceID,pat.program_number,pat.network_pmt_PID,wdata.Length);
								for (int l=0; l < wdata.Length;++l)
									sectionTable.Add(wdata[l]);
							}
							Debug.WriteLine("decode PMT:"+n.ToString());
							res=decodePMTTable(wdata, transp[0], transponder,ref pat);
						}

						if(res>0)
						{

							Debug.WriteLine("decode SDT table42");
							foreach(byte[] wdata in tab42)
								decodeSDTTable(wdata, transp[0],ref transponder,ref pat);

							Debug.WriteLine("decode SDT table46");
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
						Debug.WriteLine("  got channelinfo");
						info=chanInfo;
						break;
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write("dvbsections:GetRAWPMT() exception:{0}", ex.ToString());
			}

			if (sectionTable.Count>3)
			{
				byte byLen=(byte)sectionTable[2];
				byLen+=3;
				PMTTable = new byte[byLen];
				for (int i=0; i < byLen;++i)
				{
					PMTTable[i]=(byte)sectionTable[i];
				}
			}
			Log.Write("dvbsections:GetRAWPMT done");
			return PMTTable;
		}//public Transponder GetRAWPMT(DShowNET.IBaseFilter filter)

		public DVBChannel GetDVBChannel(DShowNET.IBaseFilter filter, int serviceId)
		{
			Log.Write("DVBSections.GetDVBChannel for service:{0}", serviceId);
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
					Log.Write("DVBSections.GetDVBChannel no sections found");
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
				GetStreamData(filter,17, 0x42,0,5000);
				tab42=(ArrayList)m_sectionsList.Clone();
				
				Debug.WriteLine("GET tab46");
				GetStreamData(filter,17, 0x46,0,5000);
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
						Log.Write("DVBSections.GetDVBChannel found channel details");
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
						Log.Write("name:{0} audio:{1:X} video:{2:X} txt:{3:X} EIT:{4} EITPF:{5}",
										chan.ServiceName,chan.AudioPid,chan.VideoPid,chan.TeletextPid,chan.HasEITSchedule,chan.HasEITPresentFollow);
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write("dvbsections:GetDVBChannel() exception:{0}", ex.ToString());
			}
			return chan;


		}//public Transponder GetRAWPMT(DShowNET.IBaseFilter filter)
	
		void LoadPMTTables (DShowNET.IBaseFilter filter,TPList tpInfo, ref Transponder tp)
		{
			int t;
			int n;
			ArrayList	tab42=new ArrayList();
			ArrayList	tab46=new ArrayList();

			// check tables
			//AddTSPid(17);
			//
			GetStreamData(filter,17, 0x42,0,m_timeoutMS);
			tab42=(ArrayList)m_sectionsList.Clone();
			GetStreamData(filter,17, 0x46,0,250); // low value, nothing in most of time
			tab46=(ArrayList)m_sectionsList.Clone();

			//bool flag;
			ChannelInfo pat;
			ArrayList pmtList = tp.PMTTable;
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
					//flag = AddTSPid(pat.network_pmt_PID);
					//if (flag == false)
					//{
					//	break;
					//}
					
					// parse pmt
					int res=0;
					if(m_setPid)
					{
						DVBSkyStar2Helper.DeleteAllPIDs(m_dataControl,0);
						DVBSkyStar2Helper.SetPidToPin(m_dataControl,0,pat.network_pmt_PID);
					}

					GetStreamData(filter,pat.network_pmt_PID, 2,0,m_timeoutMS); // get here the pmt
					foreach(byte[] wdata in m_sectionsList)
						res=decodePMTTable(wdata, tpInfo, tp,ref pat);

					if(res>0)
					{

						foreach(byte[] wdata in tab42)
							decodeSDTTable(wdata, tpInfo,ref tp,ref pat);

						foreach(byte[] wdata in tab46)
							decodeSDTTable(wdata, tpInfo,ref tp,ref pat);
					}
					tp.channels.Add(pat);
				}
			}
		}//private void LoadPMTTables (DShowNET.IBaseFilter filter,TPList tpInfo, ref Transponder tp)
		

		//
		//
		//
		private int decodePMTTable(byte[] buf, TPList transponderInfo, Transponder tp,ref ChannelInfo pat)
		{
			byte[] bytes = new byte[14];
			// check
			if(buf.Length<13)
				return 0;

			System.Array.Copy(buf, 0, bytes, 0, 12);
			int table_id = bytes[0];
			int section_syntax_indicator = BitsFromBArray(bytes, 0, 8, 1);
			int b_null = BitsFromBArray(bytes, 0, 9, 1);
			int reserved_1 = BitsFromBArray(bytes, 0, 10, 2);
			int section_length = BitsFromBArray(bytes, 0, 12, 12);
			int program_number = BitsFromBArray(bytes, 0, 24, 16);
			int reserved_2 = BitsFromBArray(bytes, 0, 40, 2);
			int version_number = BitsFromBArray(bytes, 0, 42, 5);
			int current_next_indicator = BitsFromBArray(bytes, 0, 47, 1);
			int section_number = BitsFromBArray(bytes, 0, 48, 8);
			int last_section_number = BitsFromBArray(bytes, 0, 56, 8);
			int reserved_3 = BitsFromBArray(bytes, 0, 64, 3);
			int pcr_pid = BitsFromBArray(bytes, 0, 67, 13);
			int reserved_4 = BitsFromBArray(bytes, 0, 80, 4);
			int program_info_length = BitsFromBArray(bytes, 0, 84, 12);
			//
			// store info about the channel
			//
			if(pat.program_number!=program_number)
				return 0;
			
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
					string tmpString=DVB_CADescriptor(data);
					if(pidText.IndexOf(tmpString,0)==-1)
						pidText+=tmpString+";";
				}
				len2 -= x;
				pointer += x;
				len1 -= x;
			}
			byte[] b = new byte[6];
			PMTData pmt;
			while (len1 > 4)
			{
				pmt=new PMTData();
				System.Array.Copy(buf, pointer, b, 0, 5);
				pmt.stream_type = BitsFromBArray(b, 0, 0, 8);
				pmt.reserved_1 = BitsFromBArray(b, 0, 8, 3);
				pmt.elementary_PID = BitsFromBArray(b, 0, 11, 13);
				pmt.reserved_2 = BitsFromBArray(b, 0, 24, 4);
				pmt.ES_info_length = BitsFromBArray(b, 0, 28, 12);

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
									case 0x09:
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
										pmt.isDVBSubtitle=true;
										pmt.data=DVB_SubtitleDescriptior(data);
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

			descriptor_tag		 = b[0];
			descriptor_length       	 = b[1];

			len = descriptor_length;
			byte[] bytes=new byte[len+1];
			if(len<b.Length)
			if (descriptor_tag==0x56)
			{
				int pointer  = 2;

				while ( len > 0 && (pointer+3<=b.Length)) 
				{
					System.Array.Copy(b,pointer,bytes,0,3);
					ISO_639_language_code+=System.Text.Encoding.ASCII.GetString(bytes,0,3);
					teletext_type		= BitsFromBArray (bytes,0,24,5);
					teletext_magazine_number	= BitsFromBArray (bytes,0,29,3);
					teletext_page_number	= BitsFromBArray (bytes,0,32,8);
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
		private string DVB_CADescriptor (byte[] b)
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
			// so we need some more info
			// we return
			//return "";
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

			component_type_flag= BitsFromBArray(b, 0, 16, 1);
			bsid_flag			= BitsFromBArray (b, 0, 17, 1);
			mainid_flag		= BitsFromBArray (b, 0, 18, 1);
			asvc_flag			= BitsFromBArray (b, 0, 19, 1);
			reserved_1			= BitsFromBArray (b, 0, 20, 4);

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
		void decodeCATTable (byte[] buf, TPList transponderInfo, ref Transponder tp,ref ChannelInfo pat)
		{

			int      table_id;
			int      section_syntax_indicator;		
			int      reserved_1;
			int      section_length;
			int      reserved_2;
			int      version_number;
			int      current_next_indicator;
			int      section_number;
			int      last_section_number;

			// private section


			int  len1;

 
			table_id 			 = buf[0];
			section_syntax_indicator= BitsFromBArray (buf, 0, 8, 1);
			reserved_1 			 = BitsFromBArray (buf, 0, 10, 2);
			section_length		 = BitsFromBArray (buf, 0, 12, 12);
			reserved_2 			 = BitsFromBArray (buf, 0, 24, 18);
			version_number 		 = BitsFromBArray (buf, 0, 42, 5);
			current_next_indicator	 = BitsFromBArray (buf, 0, 47, 1);
			section_number 		 = BitsFromBArray (buf, 0, 48, 8);
			last_section_number = BitsFromBArray (buf, 0, 56, 8);


			byte[] b=new byte[buf.Length];
			len1 = section_length - 5;
			int pointer = 8;
			int x;
			string caText="";

			while (len1 > 4) 
			{
				x =  buf[pointer+1]+2;
				try
				{
					System.Array.Copy(buf,pointer,b,0,x);
					caText=DVB_CADescriptor(b);
					if(caText!=null)
						if(pat.pidCache.IndexOf(caText)==-1)
						{
							pat.pidCache+=caText+";";
						}
				}
				catch
				{}
				len1 -= x;
				pointer += x;
   
			}

		}
		//
		//
		private int decodeSDTTable(byte[] buf, TPList transponderInfo, ref Transponder tp,ref ChannelInfo pat)
		{
			byte[] bytes = new byte[14];
			// check
			if(buf.Length<12)
				return -1;
			System.Array.Copy(buf, 0, bytes, 0, 11);
			int table_id = bytes[0];
			int section_syntax_indicator = BitsFromBArray(bytes, 0, 8, 1);
			int a_reserved_1 = BitsFromBArray(bytes, 0, 9, 1);
			int a_reserved_2 = BitsFromBArray(bytes, 0, 10, 2);
			int section_length = BitsFromBArray(bytes, 0, 12, 12);
			int transport_stream_id = BitsFromBArray(bytes, 0, 24, 16);
			int a_reserved_3 = BitsFromBArray(bytes, 0, 40, 2);
			int version_number = BitsFromBArray(bytes, 0, 42, 5);
			int current_next_indicator = BitsFromBArray(bytes, 0, 47, 1);
			int section_number = BitsFromBArray(bytes, 0, 48, 8);
			int last_section_number = BitsFromBArray(bytes, 0, 56, 8);
			int original_network_id = BitsFromBArray(bytes, 0, 64, 16);
			int a_reserved_4 = BitsFromBArray(bytes, 0, 80, 8);

			int len1 = section_length - 11 - 4;
			int descriptors_loop_length;
			int len2;
			int service_id;
			int reserved_1;
			int EIT_schedule_flag;
			int free_CA_mode;
			int running_status;
			int EIT_present_following_flag;
			int pointer = 11;
			int x = 0;
			byte[] b = new byte[6];


			while (len1 > 0)
			{
				System.Array.Copy(buf, pointer, b, 0, 5);
				service_id = BitsFromBArray(b, 0, 0, 16);
				reserved_1 = BitsFromBArray(b, 0, 16, 6);
				EIT_schedule_flag = BitsFromBArray(b, 0, 22, 1);
				EIT_present_following_flag = BitsFromBArray(b, 0, 23, 1);
				running_status = BitsFromBArray(b, 0, 24, 3);
				free_CA_mode = BitsFromBArray(b, 0, 27, 1);
				descriptors_loop_length = BitsFromBArray(b, 0, 28, 12);
					//
					pointer += 5;
					len1 -= 5;
					len2 = descriptors_loop_length;
				
					//
					while (len2 > 0)
					{
						x = 0;
						x = buf[pointer + 1] + 2;
						byte[] service = new byte[buf.Length-pointer + 1];
						System.Array.Copy(buf, pointer, service, 0, buf.Length - pointer);
						if (service[0] == 0x48)
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
							int st=service[0];
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
		//
		//
		//
		private int decodeBATTable(byte[] buf, TPList transponderInfo, ref Transponder tp)
		{
			byte[] bytes = new byte[14];
			// check
			if(buf.Length<11)
				return 0;

			System.Array.Copy(buf, 0, bytes, 0, 10);
			int table_id = bytes[0];
			int section_syntax_indicator = BitsFromBArray(bytes, 0, 8, 1);
			int reserved_1 = BitsFromBArray(bytes, 0, 9, 1);
			int reserved_2 = BitsFromBArray(bytes, 0, 10, 2);
			int section_length = BitsFromBArray(bytes, 0, 12, 12);
			int bouquet_id = BitsFromBArray(bytes, 0, 24, 16);
			int reserved_3 = BitsFromBArray(bytes, 0, 40, 2);
			int version_number = BitsFromBArray(bytes, 0, 42, 5);
			int current_next_indicator = BitsFromBArray(bytes, 0, 47, 1);
			int section_number = BitsFromBArray(bytes, 0, 48, 8);
			int last_section_number = BitsFromBArray(bytes, 0, 56, 8);
			int reserved_4 = BitsFromBArray(bytes, 0, 64, 4);
			int bouquet_descriptors_length = BitsFromBArray(bytes, 0, 68, 12); //
			int len1 = section_length - 10;
			int pointer = 10;
			int x = 0;
			int dvbSIIndicator;
			int len2 = bouquet_descriptors_length;
			string bouquetName;
			byte[] bData = new byte[129];
			//
			while (len2 > 0)
			{
				dvbSIIndicator = buf[pointer];
				x = buf[pointer + 1] + 2;
				System.Array.Copy(buf, pointer + 2, bData, 0, buf[pointer + 1]);
				bouquetName = getString468A(bData, buf[pointer + 1]);
				len2 -= x;
				pointer += x;
				len1 -= x;
			}
			//
			byte[] b = new byte[7];
			System.Array.Copy(buf, pointer, b, 0, 2);
			int reserved_5 = BitsFromBArray(b, 0, 0, 4);
			int transport_stream_loop_length = BitsFromBArray(b, 0, 4, 12);
			int transport_stream_id;
			int original_network_id;
			int a_reserved_1;
			int transport_descriptors_length;
			pointer += 2;
			while (len1 > 4)
			{
				System.Array.Copy(buf, pointer, b, 0, 6);
				transport_stream_id = BitsFromBArray(b, 0, 0, 16);
				original_network_id = BitsFromBArray(b, 0, 16, 16);
				a_reserved_1 = BitsFromBArray(b, 0, 32, 4);
				transport_descriptors_length = BitsFromBArray(b, 0, 36, 12);
				pointer += 6;
				len1 -= 6;
				len2 = transport_descriptors_length;
				while (len2 > 0)
				{
					dvbSIIndicator = buf[pointer];
					x = buf[pointer + 1] + 2;
					len2 -= x;
					pointer += x;
					len1 -= x;
				}
			}

			return 0;
		}
		private int decodeEITTable(byte[] buf, ref EIT_Program_Info eitInfo)
		{
			int table_id;
			int section_syntax_indicator;
			int reserved_1;
			int reserved_2;
			int section_length;
			int service_id;
			int reserved_3;
			int version_number;
			int current_next_indicator;
			int section_number;
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
			byte[] b = new byte[15];
			byte[] bytes = new byte[32001];
			//
			if (buf.Length < 14)
			{
				return 0;
			}
			System.Array.Copy(buf, 0, b, 0, 14);
			table_id = buf[0];
			section_syntax_indicator = BitsFromBArray(b, 0, 8, 1);
			reserved_1 = BitsFromBArray(b, 0, 9, 1);
			reserved_2 = BitsFromBArray(b, 0, 10, 2);
			section_length = BitsFromBArray(b, 0, 12, 12);
			service_id = BitsFromBArray(b, 0, 24, 16);
			reserved_3 = BitsFromBArray(b, 0, 40, 2);
			version_number = BitsFromBArray(b, 0, 42, 5);
			current_next_indicator = BitsFromBArray(b, 0, 47, 1);
			section_number = BitsFromBArray(b, 0, 48, 8);
			last_section_number = BitsFromBArray(b, 0, 56, 8);
			transport_stream_id = BitsFromBArray(b, 0, 64, 16);
			original_network_id = BitsFromBArray(b, 0, 80, 16);
			segment_last_section_number = BitsFromBArray(b, 0, 96, 8);
			last_table_id = BitsFromBArray(b, 0, 104, 8);
			//
			if (service_id == 0xFFFF) // scrambled
			{
				return 0;
			}

			//eitInfo.running_status

			len1 = section_length - 11;
			int pointer = 14;
			//
			//eitInfo.evt_info_running;
			//Dim eitInfo As EIT_Program_Info
			//
			while (len1 > 4)
			{
				EITDescr eit=new EITDescr();
                System.Array.Copy(buf, pointer, b, 0, 12);
				event_id = BitsFromBArray(b, 0, 0, 16);
				start_time_MJD = BitsFromBArray(b, 0, 16, 16);
				start_time_UTC = BitsFromBArray(b, 0, 32, 24);
				duration = BitsFromBArray(b, 0, 56, 24);
				running_status = BitsFromBArray(b, 0, 80, 3);
				free_CA_mode = BitsFromBArray(b, 0, 83, 1);
				descriptors_loop_length = BitsFromBArray(b, 0, 84, 12);
				// is there an event already in the list?
				// process info
				pointer += 12;
				len1 -= 12 + descriptors_loop_length;
				len2 = descriptors_loop_length;
				
				while (len2 > 0)
				{
					indicator = buf[pointer];
					x = buf[pointer + 1] + 2; //(b, DVB_SI);
					byte[] descrEIT = new byte[x + 1];
					System.Array.Copy(buf, pointer, descrEIT, 0, x);
					switch (indicator)
					{
						case 0x4E:
							DVB_ExtendedEvent(descrEIT,ref eit);
							break;
						case 0x4D:
							DVB_ShortEvent(descrEIT,ref eit);
							break;
						case 0x54:
							DVB_ContentDescription(descrEIT,ref eit);
							break;
							
					}

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
				eitInfo.eitList.Add(eit);
			}
			//eitInfo.evt_info_act_ts=eit;
			return 0;
		}
		private void DVB_ShortEvent(byte[] buf, ref EITDescr eit)
		{
			int descriptor_tag;
			int descriptor_length;
			//string ISO639_2_language_code="";
			int event_name_length;
			int text_length;
			byte[] b = new byte[4097];
			
			descriptor_tag = buf[0];
			descriptor_length = buf[1];
			eit.event_name = "n.v.";
			eit.event_text = "n.v.";
			if (descriptor_tag == 0x4D)
			{
				event_name_length = buf[5];
				int pointer = 6;
				System.Array.Copy(buf, pointer, b, 0, event_name_length);
				eit.event_name = getString468A(b, event_name_length);
				
				pointer += event_name_length;
				text_length = buf[pointer];
				pointer += 1;
				System.Array.Copy(buf, pointer, b, 0, buf.Length - pointer);
				eit.event_text = getString468A(b, text_length);
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
					content_nibble_level_1	 = BitsFromBArray (b,0, 0,4);
					content_nibble_level_2	 = BitsFromBArray (b,0, 4,4);
					user_nibble_1		 = BitsFromBArray (b,0, 8,4);
					user_nibble_2		 = BitsFromBArray (b,0,12,4);

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
			//string ISO639_2_language_code;
			int descriptor_number;
			int last_descriptor_number;
			//int event_name_length;
			int text_length;
			int length_of_items;
			//string event_Name;
			byte[] b = new byte[4097];
			byte[] data = new byte[8];
			string text = "not avail.";
			int pointer = 0;
			int lenB;
			int len1;
			int item_description_length;
			int item_length;
			string item = "n.a. ";
			
			System.Array.Copy(buf, 0, data, 0, 7);
			
			descriptor_tag = data[0];
			descriptor_length = data[1];
			descriptor_number = BitsFromBArray(data, 0, 16, 4);
			last_descriptor_number = BitsFromBArray(data, 0, 20, 4);
			length_of_items = BitsFromBArray(data, 0, 48, 8);
			
			pointer += 7;
			lenB = descriptor_length - 5;
			len1 = length_of_items;
			
			while (len1 > 0)
			{
				System.Array.Copy(buf, pointer, b, 0, lenB - pointer);
				item_description_length = BitsFromBArray(b, 0, 0, 8);
				pointer += 1 + item_description_length;
				System.Array.Copy(buf, pointer, b, 0, lenB - pointer);
				item_length = BitsFromBArray(b, 0, 0, 8);
				System.Array.Copy(buf, pointer + 1, b, 0, item_length);
				item = getString468A(b, item_length);
				pointer += 1 + item_length;
				len1 -= (2 + item_description_length + item_length);
				lenB -= (2 + item_description_length + item_length);
			}
			System.Array.Copy(buf, pointer, b, 0, 1);
			text_length = BitsFromBArray(b, 0, 0, 8);
			pointer += 1;
			lenB -= 1;
			System.Array.Copy(buf, pointer, b, 0, text_length);
			text = getString468A(b, text_length);
			eit.event_item += item;
			eit.event_item_text += text;
			return 0;
		}
		//
		//
		//
		private object decodeNITTable(byte[] buf,ref TPList[] transponders)
		{
			int table_id;
			int section_syntax_indicator;
			int reserved_1;
			int reserved_2;
			int section_length;
			int network_id;
			int reserved_3;
			int version_number;
			int current_next_indicator;
			int section_number;
			int last_section_number;
			int reserved_4;
			int network_descriptor_length;
			//N ... descriptor
			int reserved_5;
			int transport_stream_loop_length;

			// nit tsl
			int transport_stream_id;
			int original_network_id;

			int transport_descriptor_length=0;
			//
			int pointer=0;
			int l1=0;
			int l2=0;
			byte[] b = new byte[buf.Length + 1];
			//
			//
			int transpCount=0;
			//
			System.Array.Copy(buf, 0, b, 0, 10);
			table_id = b[0];
			section_syntax_indicator = BitsFromBArray(b, 0, 8, 1);
			reserved_1 = BitsFromBArray(b, 0, 9, 1);
			reserved_2 = BitsFromBArray(b, 0, 10, 2);
			section_length = BitsFromBArray(b, 0, 12, 12);
			network_id = BitsFromBArray(b, 0, 24, 16);
			reserved_3 = BitsFromBArray(b, 0, 40, 2);
			version_number = BitsFromBArray(b, 0, 42, 5);
			current_next_indicator = BitsFromBArray(b, 0, 47, 1);
			section_number = BitsFromBArray(b, 0, 48, 8);
			last_section_number = BitsFromBArray(b, 0, 56, 8);
			reserved_4 = BitsFromBArray(b, 0, 64, 4);
			network_descriptor_length = BitsFromBArray(b, 0, 68, 12);
			
			
			l1 = network_descriptor_length;
			pointer += 10;
			int x = 0;
			byte[] b0 = new byte[buf.Length-pointer + 1];
			//m_transponderData.transp_List = new ArrayList();
			ArrayList data=new ArrayList();

			while (l1 > 0)
			{
				x = buf[pointer + 1] + 2;
				byte[] service = new byte[buf.Length-pointer + 1];
				System.Array.Copy(buf, pointer, service, 0, buf.Length - pointer);
				l1 -= x;
				pointer += x;
			}
			System.Array.Copy(buf, pointer, b, 0, buf.Length - pointer);
			reserved_5 = BitsFromBArray(b, 0, 0, 4);
			transport_stream_loop_length = BitsFromBArray(b, 0, 4, 12);
			l1 = transport_stream_loop_length;
			pointer += 2;
			System.Array.Copy(buf, pointer, b0, 0, buf.Length - pointer);
			while (l1 > 0)
			{
				transport_stream_id = BitsFromBArray(b0, 0, 0, 16);
				original_network_id = BitsFromBArray(b0, 0, 16, 16);
				reserved_1 = BitsFromBArray(b0, 0, 32, 4);
				transport_descriptor_length = BitsFromBArray(b0, 0, 36, 12);
				pointer += 6;
				l1 -= 6;
				l2 = transport_descriptor_length;
				transpCount += 1;
				while (l2 > 0)
				{
					TPList tp=new TPList();
					x = buf[pointer + 1] ;
					byte[] service = new byte[x + 1];
					System.Array.Copy(buf, pointer, service, 0, x);
					DVB_GetSatDelivSys(service, transport_stream_id, original_network_id,ref tp);
					data.Add(tp);
					pointer += x;
					l2 -= x;
					l1 -= x;
				}
			}
			x = 0;
			return 0;
		}
		private object DVB_GetSatDelivSys(byte[] b, int tr, int onid,ref TPList tp)
		{
			if(b[0]==0x43)
			{
				int descriptor_tag = b[0];
				int descriptor_length = b[1];
				int frequency = (b[2]<<24)+(b[3]<<16)+(b[4]<<8)+b[5];
				int orbital_position = BitsFromBArray(b, 0, 48, 16);
				int west_east_flag = BitsFromBArray(b, 0, 64, 1);
				int polarisation = BitsFromBArray(b, 0, 65, 2);
				int modulation = BitsFromBArray(b, 0, 67, 5);
				int symbol_rate = (b[9]<<24)+(b[10]<<16)+(b[11]<<8)+(b[12]>>4);
				int FEC_inner = BitsFromBArray(b, 0, 100, 4);
				int org_Network_ID = onid;
				int transport_Stream_ID = tr;
				tp.TPfreq=frequency;
				tp.TPpol=polarisation;
				tp.TPsymb=symbol_rate;
			}
			
			return null;
		}
		//
		//
		//
		private int SetBit(int InByte, byte Bit)
		{
			int returnValue;
			
			returnValue = InByte |(int)Math.Pow(2, Bit); //Set het n'de Bit
			
			return returnValue;
		}


		private byte ToggleBit(long InByte, byte Bit)
		{
			byte returnValue;
			returnValue = System.Convert.ToByte(Math.Pow(InByte,(Math.Pow(2, Bit))));
			return returnValue;
		}
		private int BitsFromBArray (byte[] buf, int byte_offset, int startbit, int bitlen)
		{
			byte b=0;
			byte b1=0;
			byte b2=0;
			byte b3=0;
			ulong  val=0;
			ulong bitMask=0;
			ulong longVal=0;
			int  highVal=0;
			int oldStartBit=startbit;
			int pos=byte_offset + (startbit >> 3);
			int bufLen=buf.Length-1;

			try
			{
				b=buf[pos];

				if(pos+1>=bufLen)
					b1=0;
				else
					b1=buf[pos+1];
			
				if(pos+2>=bufLen)
					b2=0;
				else
					b2=buf[pos+2];

				if(pos+3>=bufLen)
					b3=0;
				else
					b3=buf[pos+3];


				startbit %= 8;
				switch ((bitlen-1) >> 3) 
				{
					case -1:	
						return 0;
				
					case 0:		
						longVal = (ulong)((b << 8) +  b1);
						highVal = 16;
						break;

					case 1:		
						longVal = (ulong)((b <<16) + (b1<< 8) +  b2);
						highVal = 24;
						break;

					case 2:		
						longVal = (ulong)((b<<24) + (b1<<16) + (b2<< 8) +  b3);
						highVal = 32;
						break;

					default:	
						return (int) -1;
				}
			

				startbit = highVal - startbit - bitlen;
				longVal = longVal >> startbit;
				bitMask= ((ulong)1UL << bitlen) - 1; 
				val= longVal & bitMask;
			}
			catch
			{val=0;}
			return (int)val;
		}
		//
		public ArrayList GetEITSchedule(int tab,DShowNET.IBaseFilter filter)
		{
			EIT_Program_Info eit=new EIT_Program_Info();
			eit.eitList=new ArrayList();
			GetStreamData(filter,18,tab,1,200);
			foreach(byte[] arr in m_sectionsList)
				decodeEITTable(arr,ref eit);

			return eit.eitList;
		}

		public int GrabEIT(DVBChannel ch,DShowNET.IBaseFilter filter)
		{
			// there must be an ts (card tuned) to get eit
			
			int eventsCount=0;

			ArrayList eitList=new ArrayList();
			if(ch.HasEITPresentFollow==true)
			{
				eitList=GetEITSchedule(0x4e,filter);
				eventsCount+=eitList.Count;
				foreach(EITDescr eit in eitList)
					SetEITToDatabase(eit,ch.ServiceName);
				eitList=GetEITSchedule(0x4f,filter);
				eventsCount+=eitList.Count;
				foreach(EITDescr eit in eitList)
					SetEITToDatabase(eit,ch.ServiceName);
			}
			if(ch.HasEITSchedule==true)
			{
				int lastTable=0x50;
				eitList=GetEITSchedule(0x50,filter);

				eventsCount+=eitList.Count;
				foreach(EITDescr eit in eitList)
				{
					if(eit.lastTable>lastTable)
						lastTable=eit.lastTable;
					SetEITToDatabase(eit,ch.ServiceName);
				}
				for(int table=0x51;table<lastTable;table++)
				{
					eitList=GetEITSchedule(table,filter);
					eventsCount+=eitList.Count;
					foreach(EITDescr eit in eitList)
						SetEITToDatabase(eit,ch.ServiceName);

				}

			}
			
			GC.Collect();
			return 	eventsCount;

		}
		//
		public long GetStartDateFromEIT(EITDescr data)
		{
			
			string chName=TVDatabase.GetSatChannelName(data.program_number,data.org_network_id,data.ts_id);
			try
			{
				System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
				date=date.ToLocalTime();
				return GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
		
			}
			catch
			{}
			return 0;
		}
		//
		public long GetEndDateFromEIT(EITDescr data)
		{
			
			string chName=TVDatabase.GetSatChannelName(data.program_number,data.org_network_id,data.ts_id);
			try
			{
				System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
				System.DateTime dur=new DateTime();
				date=date.ToLocalTime();
				dur=date;
				dur=dur.AddHours((double)data.duration_hh);
				dur=dur.AddMinutes((double)data.duration_mm);
				dur=dur.AddSeconds((double)data.duration_ss);
				return GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
			}
			catch
			{}
			return 0;
		}

		//
		public int GrabEIT(DShowNET.IBaseFilter filter)
		{
			// there must be an ts (card tuned) to get eit
			
			int eventsCount=0;

			ArrayList eitList=new ArrayList();
			eitList=GetEITSchedule(0x4e,filter);
			foreach(EITDescr eit in eitList)
			{
				string progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id,eit.ts_id);
				eventsCount+=SetEITToDatabase(eit,progName,0);
			}
			eitList=GetEITSchedule(0x4f,filter);
			foreach(EITDescr eit in eitList)
			{
				string progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id,eit.ts_id);
				eventsCount+=SetEITToDatabase(eit,progName,0);
			}
			int lastTable=0x50;
			eitList=GetEITSchedule(0x50,filter);

			foreach(EITDescr eit in eitList)
			{
				if(eit.lastTable>lastTable)
					lastTable=eit.lastTable;
				string progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id,eit.ts_id);
				eventsCount+=SetEITToDatabase(eit,progName,0);
			}
			for(int table=0x51;table<lastTable;table++)
			{
				eitList=GetEITSchedule(table,filter);
				foreach(EITDescr eit in eitList)
				{
					string progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id,eit.ts_id);
					eventsCount+=SetEITToDatabase(eit,progName,0);
				}

			}

			lastTable=0x60;
			eitList=GetEITSchedule(0x60,filter);

			foreach(EITDescr eit in eitList)
			{
				if(eit.lastTable>lastTable)
					lastTable=eit.lastTable;
				string progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id,eit.ts_id);
				eventsCount+=SetEITToDatabase(eit,progName,0);
			}
			for(int table=0x61;table<lastTable;table++)
			{
				eitList=GetEITSchedule(table,filter);
				foreach(EITDescr eit in eitList)
				{
					string progName=TVDatabase.GetSatChannelName(eit.program_number,eit.org_network_id,eit.ts_id);
					eventsCount+=SetEITToDatabase(eit,progName,0);
				}


			}			
			GC.Collect();
			return 	eventsCount;

		}
		//
		public int SetEITToDatabase(EITDescr data,string channelName,int dummy)
		{
			try
			{
				int retVal=0;
				TVProgram tv=new TVProgram();
				System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
				date=date.ToLocalTime();
				if(date<System.DateTime.Now)
					return 0;
				System.DateTime dur=new DateTime();
				dur=date;
				dur=dur.AddHours((double)data.duration_hh);
				dur=dur.AddMinutes((double)data.duration_mm);
				dur=dur.AddSeconds((double)data.duration_ss);
				if(dur<date)
					return 0;
				tv.Channel=channelName;
				tv.Genre=data.genere_text;
				tv.Description=data.event_item_text;
				tv.Title=data.event_name;
				tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
				tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
				ArrayList programsInDatabase = new ArrayList();
				TVDatabase.GetProgramsPerChannel(tv.Channel,tv.Start,tv.End,ref programsInDatabase);
				if(programsInDatabase.Count==0 && channelName!="")
				{
					int programID=TVDatabase.AddProgram(tv);
					TVDatabase.RemoveOverlappingPrograms();
					if(programID!=-1)
					{
						Log.Write("added to database for channel {0} event start:{1} end:{2} text:{3}",tv.Channel,tv.Start,tv.End,data.event_name);
						retVal= 1;
					}
				}
				return retVal;
			}
			catch{return 0;}
		}
		//
		public void SetEITToDatabase(EITDescr data, string channelName)
		{
			try
			{
				TVProgram tv=new TVProgram();
				System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
				date=date.ToLocalTime();
				if(date<System.DateTime.Now)
					return;
				System.DateTime dur=new DateTime();
				dur=date;
				dur=dur.AddHours((double)data.duration_hh);
				dur=dur.AddMinutes((double)data.duration_mm);
				dur=dur.AddSeconds((double)data.duration_ss);
				if(dur<date)
					return;
				tv.Channel=channelName;
				tv.Genre=data.genere_text;
				tv.Description=data.event_item_text;
				tv.Title=data.event_name;
				tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
				tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
				Log.Write("program:{0} {1} {2} {3} {4}-{5}",
						tv.Channel,tv.Genre,tv.Description,tv.Title,tv.StartTime,tv.EndTime);
				ArrayList programsInDatabase = new ArrayList();
				TVDatabase.GetProgramsPerChannel(tv.Channel,tv.Start,tv.End,ref programsInDatabase);
				if(programsInDatabase.Count==0)
				{
					int programID=TVDatabase.AddProgram(tv);
				}
			}
			catch{}

		}
		private long GetLongFromDate(int year,int mon,int day,int hour,int min,int sec)
		{
			string longVal="";
			string yr=year.ToString();
			string mo=mon.ToString();
			string da=day.ToString();
			string h=hour.ToString();
			string m=min.ToString();
			string s=sec.ToString();
			if(mo.Length==1)
				mo="0"+mo;
			if(da.Length==1)
				da="0"+da;
			if(h.Length==1)
				h="0"+h;
			if(m.Length==1)
				m="0"+m;
			if(s.Length==1)
				s="0"+s;
			longVal=yr+mo+da+h+m+s;
			return (long)Convert.ToUInt64(longVal);
		}
		int getUTC(int val)
		{
			if ((val&0xF0)>=0xA0)
				return 0;
			if ((val&0xF)>=0xA)
				return 0;
			return ((val&0xF0)>>4)*10+(val&0xF);
		}

		
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
		private string getString468A(byte[] b, int l1)
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
		private bool GetStreamData(DShowNET.IBaseFilter filter,int pid, int tid,int tableSection,int timeout)
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

			m_sectionsList=new ArrayList();
			flag = GetSectionData(filter,pid, tid,ref sectLast,tableSection,timeout);
			if(flag==false)
				return false;

			for(int n=0;n<sectLast;n++)
			{
				flag=GetSectionPtr(n,ref sectionBuffer,ref dataLen,ref header, ref tableExt, ref version,ref sectNum, ref sectLast);
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
							Log.Write("dvbsections: error on copy data. address={0}, length ={1}",sectionBuffer,dataLen);
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
							Log.Write("dvbsections: error on copy data. address={0}, length ={1}",sectionBuffer,dataLen);
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
	}
}
