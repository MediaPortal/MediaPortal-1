using System;
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
		private static extern bool ConvertJulianDate(long MJD, out int year, out int month, out int day);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool ClearAllUp();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool PauseSITable();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool RunSITable();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool IsTunerLocked();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool AddPidToTS(int pid);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool Tune(int fre, int symrate, int fec, int pol, int lnbkhz, int Diseq, int LNBfreq);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool DVBInit(string adapter);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern void ClearAllTsPids();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool StopSITable();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool DeleteAllPIDsI();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool SetupB2C2Graph();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionDataI(int pid, int tid, ref int secCount,int tableSection,int timeout);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool AddTSPid(int pid);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern void ResetDevice();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionPtr(int section,ref IntPtr dataPointer,ref int len,ref int header,ref int tableExtId,ref int version,ref int secNum,ref int lastSecNum);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool ReleaseSectionsBuffer();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid,int tid,ref int sectionCount,int tableSection,int timeout);
		// globals
		TPList[]							transp=new TPList[200];
		ArrayList							m_eitList;
		ArrayList							m_transponderList;
		int									m_diseqc=0;
		int									m_lnb0=0;
		int									m_lnb1=0;
		int									m_lnbsw=0;
		int									m_lnbkhz=0;
		System.Windows.Forms.ProgressBar	m_progress=null;
		System.Windows.Forms.TextBox		m_textBox=null;
		int									m_lnbfreq=0;
		int									m_selKhz=0;
		ArrayList							m_sectionsList=new ArrayList();	
		bool								m_breakScan=false;	
		

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
			public string	teletextLANG;
		}
		//
		//

		private void ListBox1_SelectedIndexChanged (System.Object sender, System.EventArgs e)
		{
		}
		
		public void OpenTPLFile (string fileName,ref Transponder[] list,int diseqc,int lnbkhz,int lnb0,int lnb1,int lnbsw,System.Windows.Forms.ProgressBar progBar,System.Windows.Forms.TextBox feedbackText)
		{
			string[] tpdata;
			string line;
			System.IO.TextReader tin;
			int count = 0;
			// set diseq & lnb
			m_diseqc=diseqc;
			m_lnb0=lnb0;
			m_lnb1=lnb1;
			m_lnbsw=lnbsw;
			m_lnbkhz=lnbkhz;
			// set dialog feedback
			m_progress=progBar;
			m_textBox=feedbackText;
            // load transponder list and start scan
			tin = System.IO.File.OpenText(fileName);
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
				if (line.Length > 0)
				{
					if(line.StartsWith(";"))
						continue;
					tpdata = line.Split(new char[]{','});
					if(tpdata.Length!=3)
						tpdata = line.Split(new char[]{';'});
					if (tpdata.Length == 3)
					{
						try
						{
						
							transp[count].TPfreq = Convert.ToInt16(tpdata[0]) * 1000;
							switch (tpdata[1].ToLower())
							{
								case "v":
									
									transp[count].TPpol = 1;
									break;
								case "h":
									
									transp[count].TPpol = 0;
									break;
								default:
									
									transp[count].TPpol = 0;
									break;
							}
							transp[count].TPsymb = Convert.ToInt16(tpdata[2]);
							count += 1;
						}
						catch
						{}
					}
				}
			} while (!(line == null));
			
			count -= 1;
			StartScan(count,ref list);
		}

		private int decodePATTable(byte[] buf, TPList transponderInfo, ref Transponder tp)
		{
			int a;
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
			
			n = section_length - 5 - 4;
			byte[] b = new byte[5];
			a = n / 4;
			ChannelInfo ch=new ChannelInfo();
			for (int count=0;count<a;count++)
			{
				System.Array.Copy(buf, 8 +(count * 4), b, 0, 4);
				ch.transportStreamID=transport_stream_id;
				ch.program_number = BitsFromBArray(b, 0, 0, 16);
				ch.reserved = BitsFromBArray(b, 0, 16, 3);
				ch.network_pmt_PID = BitsFromBArray(b, 0, 19, 13);
				if(ch.program_number!=0)
					tp.PMTTable.Add(ch);
			}
			return 0;
		}
		public void InterruptScan()
		{
			m_breakScan=true;
			m_textBox.Text="(Stopping scan...please wait a moment!)";

		}
		//
		//
		private void StartScan (int count, ref Transponder[] transplist)
		{
			int t;
			int lnbFreq;
			int servCount=0;
			int lnbkhz=0;

			m_transponderList = new ArrayList();
			transplist = new Transponder[count + 1];
			for (t = 0; t <= count; t++)
			{
				if(m_breakScan==true)
					break;
				DeleteAllPIDsI();
				AddTSPid(17);
				AddTSPid(0);
				// feedback
				if(m_progress!=null)
				{
					m_progress.Minimum = 0;
					m_progress.Maximum = count;
					m_progress.Value = t;
				}
				// set the tranponder data
				transplist[t].channels = new ArrayList();
				transplist[t].PMTTable = new ArrayList();
				// first get pmt
				if(m_lnb1!=-1 && m_lnbsw!=-1)
				{
					if (transp[t].TPfreq >= m_lnbsw*1000)
					{
						lnbFreq = m_lnb1;
						lnbkhz=m_lnbkhz;
					}
					else
					{
						lnbFreq = m_lnb0;
						lnbkhz=0;
					}
				}
				else
				{
					lnbFreq = m_lnb0;
					lnbkhz=0;
				}
				m_lnbfreq=lnbFreq;
				m_selKhz=lnbkhz;
				// feedback
				if(m_textBox!=null)
				{
					m_textBox.Text=Convert.ToString(transp[t].TPfreq/1000)+" MHz...("+Convert.ToString(servCount)+" services found yet)";
				}
				Tune(transp[t].TPfreq, transp[t].TPsymb, 6, transp[t].TPpol, m_selKhz, m_diseqc, lnbFreq);
				//System.Threading.Thread.Sleep(500);
				if (IsTunerLocked())
				{
					GetStreamData(0, 0,0,5000);
					// jump to parser
					foreach(byte[] arr in m_sectionsList)
						decodePATTable(arr, transp[t], ref transplist[t]);

					LoadPMTTables(transp[t],ref transplist[t]);
					if(m_textBox!=null)
					{
						servCount+=transplist[t].channels.Count;
					}
				}

			}
		}
		public Transponder Scan(DShowNET.IBaseFilter filter)
		{
			Transponder transplist = new Transponder();
			transplist.channels = new ArrayList();
			transplist.PMTTable = new ArrayList();
			DeleteAllPIDsI();
			AddTSPid(17);
			AddTSPid(0);
			GetStreamData(filter,0, 0,0,5000);
			// jump to parser
			foreach(byte[] arr in m_sectionsList)
				decodePATTable(arr, transp[0], ref transplist);

			LoadPMTTables(filter,transp[0],ref transplist);
			return transplist;
		}
		
		private void LoadPMTTables (DShowNET.IBaseFilter filter,TPList tpInfo, ref Transponder tp)
		{
			int t;
			int n;
			ArrayList	tab42=new ArrayList();
			ArrayList	tab46=new ArrayList();

			// check tables
			AddTSPid(17);
			//
			GetStreamData(filter,17, 0x42,0,5000);
			tab42=(ArrayList)m_sectionsList.Clone();
			GetStreamData(filter,17, 0x46,0,5000);
			tab46=(ArrayList)m_sectionsList.Clone();

			bool flag;
			ChannelInfo pat;
			ArrayList pmtList = tp.PMTTable;
			int pmtScans;
			pmtScans = (pmtList.Count / 20) + 1;
			for (t = 1; t <= pmtScans; t++)
			{
				flag = DeleteAllPIDsI();
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
					GetStreamData(filter,pat.network_pmt_PID, 2,0,5000); // get here the pmt
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
		}
		
		private void LoadPMTTables (TPList tpInfo, ref Transponder tp)
		{
			int t;
			int n;
			ArrayList	tab42=new ArrayList();
			ArrayList	tab46=new ArrayList();

			// check tables
			AddTSPid(17);
			//
			GetStreamData(17, 0x42,0,5000);
			tab42=(ArrayList)m_sectionsList.Clone();
			GetStreamData(17, 0x46,0,5000);
			tab46=(ArrayList)m_sectionsList.Clone();

			bool flag;
			ChannelInfo pat;
			ArrayList pmtList = tp.PMTTable;
			int pmtScans;
			pmtScans = (pmtList.Count / 20) + 1;
			for (t = 1; t <= pmtScans; t++)
			{
				flag = DeleteAllPIDsI();
				for (n = 0; n <= 19; n++)
				{
					if (((t - 1) * 20) + n > pmtList.Count - 1)
					{
						break;
					}
					pat = (ChannelInfo) pmtList[((t - 1) * 20) + n];
					flag = AddTSPid(pat.network_pmt_PID);
					if (flag == false)
					{
						break;
					}
					
					// parse pmt
					int res=0;
					GetStreamData(pat.network_pmt_PID, 2,0,5000); // get here the pmt
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


		}
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
			//
			int len1 = section_length - 4;
			int len2 = program_info_length;
			//len1=
			int pointer = 12;
			int x;
			while (len2 > 0)
			{
				x = 0;
				x = buf[pointer + 1] + 2;
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

			} // kill
			//tp.channels.Add(ch);
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
		private object decodeNITTable(byte[] buf)
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
					x = buf[pointer + 1] + 2;
					byte[] service = new byte[x + 1];
					System.Array.Copy(buf, pointer, service, 0, x);
					//m_transponderData.transp_List.Add(DVB_GetSatDelivSys(service, transport_stream_id, original_network_id));
					pointer += x;
					l2 -= x;
					l1 -= x;
				}
			}
			x = 0;
			return 0;
		}
		private object DVB_GetSatDelivSys(byte[] b, int tr, int onid)
		{
			int descriptor_tag = b[0];
			int descriptor_length = b[1];
			int frequency = BitsFromBArray(b, 0, 16, 32);
			int orbital_position = BitsFromBArray(b, 0, 48, 16);
			int west_east_flag = BitsFromBArray(b, 0, 64, 1);
			int polarisation = BitsFromBArray(b, 0, 65, 2);
			int modulation = BitsFromBArray(b, 0, 67, 5);
			int symbol_rate = BitsFromBArray(b, 0, 72, 28);
			int FEC_inner = BitsFromBArray(b, 0, 100, 4);
			int org_Network_ID = onid;
			int transport_Stream_ID = tr;
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
			GetStreamData(filter,18,tab,1,100);
			foreach(byte[] arr in m_sectionsList)
				decodeEITTable(arr,ref eit);

			return eit.eitList;


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

		public void SetEITToDatabase(EITDescr data)
		{
			TVProgram tv=new TVProgram();
			string chName=TVDatabase.GetSatChannelName(data.program_number,data.org_network_id,data.ts_id);
			System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
			date=date.ToLocalTime();
			System.DateTime dur=new DateTime();
			dur=date;
			dur=dur.AddHours((double)data.duration_hh);
			dur=dur.AddMinutes((double)data.duration_mm);
			dur=dur.AddSeconds((double)data.duration_ss);
			tv.Channel=chName;
			tv.Genre=data.genere_text;
			tv.Description=data.event_item_text;
			tv.Title=data.event_name;
			tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
			tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
			int ok=TVDatabase.AddProgram(tv);

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


		public void CleanUp ()
		{
			ClearAllUp();
			m_breakScan=false;
		}
		public bool Run()
		{
			bool flag;
			flag = SetupB2C2Graph();
			if (flag == true)
			{
				flag = RunSITable();
			}
			if (flag == false)
			{
				return false;
			}
			DeleteAllPIDsI();
			AddTSPid(18);
			m_eitList = new ArrayList();
			
			return true;
		}
		
		private bool GetStreamData(int pid, int tid,int tableSection,int timeout)
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
			DeleteAllPIDsI();
			AddTSPid(pid);
			flag = GetSectionDataI(pid, tid,ref sectLast,tableSection,timeout);

			for(int n=0;n<sectLast;n++)
			{
				flag=GetSectionPtr(n,ref sectionBuffer,ref dataLen,ref header, ref tableExt, ref version,ref sectNum, ref sectLast);
				if(flag)
				{
					if (tableExt != - 1)
					{
						arr = new byte[dataLen+8 + 1];
						Marshal.Copy(sectionBuffer, arr, 8, dataLen);
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
						Marshal.Copy(sectionBuffer, arr, 3, dataLen);
						arr[0] = System.Convert.ToByte(tid);
						arr[1] = System.Convert.ToByte((header >> 8) & 255);
						arr[2] = System.Convert.ToByte(header & 255);
						m_sectionsList.Add(arr);
					}// else

				}// if(flag)
			}//for
			ReleaseSectionsBuffer();
			return true;
		}
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

			for(int n=0;n<sectLast;n++)
			{
				flag=GetSectionPtr(n,ref sectionBuffer,ref dataLen,ref header, ref tableExt, ref version,ref sectNum, ref sectLast);
				if(flag)
				{
					if (tableExt != - 1)
					{
						arr = new byte[dataLen+8 + 1];
						Marshal.Copy(sectionBuffer, arr, 8, dataLen);
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
						Marshal.Copy(sectionBuffer, arr, 3, dataLen);
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
