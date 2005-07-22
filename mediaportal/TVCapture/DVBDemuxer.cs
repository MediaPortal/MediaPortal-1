//#define MAKEDUMP
using System;
using System.Text;
using DShowNET;
using MediaPortal;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace MediaPortal.TV.Recording
{
    public class DVBDemuxer : ISampleGrabberCB
    {
        
        #region Global Arrays
		readonly static int[,,] AudioBitrates = new int[,,]{{
		{-1,8000,16000,24000,32000,40000,48000,56000,64000,
		80000,96000,112000,128000,144000,160000,0 },		
		{-1,8000,16000,24000,32000,40000,48000,56000,64000,
		80000,96000,112000,128000,144000,160000,0 },		
		{-1,32000,48000,56000,64000,80000,96000,112000,128000,
		144000,160000,176000,192000,224000,256000,0 }		
	},{
		{-1,32000,40000,48000,56000,64000,80000,96000,
		112000,128000,160000,192000,224000,256000,320000, 0 },	
		{-1,32000,48000,56000,64000,80000,96000,112000,
		128000,160000,192000,224000,256000,320000,384000, 0 },	
		{-1,32000,64000,96000,128000,160000,192000,224000,
		256000,288000,320000,352000,384000,416000,448000,0 }	
	},{
		{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,    
		28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,    
		28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 8000, 12000, 16000, 20000, 24000, 32000, 40000,    
		48000, 560000, 64000, 80000, 96000, 112000, 128000, 0 }
	}};
	
	readonly static int[,] AudioFrequencies = new int[,]{
		{ 22050,24000,16000,0 },	
		{ 44100,48000,32000,0 },	
		{ 11025,12000,8000,0 }		
	};
	
	readonly static double[] AudioTimes = new double[]{ 0.0,103680000.0,103680000.0,34560000.0 };
	UInt32[] CRC32Data = new UInt32[]{0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b,
		 0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
		 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 0x4c11db70, 0x48d0c6c7,
		 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
		 0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3,
		 0x709f7b7a, 0x745e66cd, 0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
		 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef,
		 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
		 0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb,
		 0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
		 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 0x34867077, 0x30476dc0,
		 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
		 0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4,
		 0x0808d07d, 0x0cc9cdca, 0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
		 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08,
		 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
		 0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc,
		 0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
		 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 0xe0b41de7, 0xe4750050,
		 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
		 0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34,
		 0xdc3abded, 0xd8fba05a, 0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
		 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1,
		 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
		 0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5,
		 0x3f9b762c, 0x3b5a6b9b, 0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
		 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 0xf12f560e, 0xf5ee4bb9,
		 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
		 0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd,
		 0xcda1f604, 0xc960ebb3, 0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
		 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71,
		 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
		 0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2,
		 0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
		 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 0x119b4be9, 0x155a565e,
		 0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
		 0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a,
		 0x2d15ebe3, 0x29d4f654, 0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
		 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676,
		 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
		 0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662,
		 0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
		 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4};
	#endregion

        #region Structs
        public struct AudioHeader
        {
            //AudioHeader
            public int ID;
            public int Emphasis;
            public int Layer;
            public int ProtectionBit;
            public int Bitrate;
            public int SamplingFreq;
            public int PaddingBit;
            public int PrivateBit;
            public int Mode;
            public int ModeExtension;
            public int Bound;
            public int Channel;
            public int Copyright;
            public int Original;
            public int TimeLength;
            public int Size;
            public int SizeBase;
        } ;

		//
		// section header
		public struct DVBSectionHeader
		{
			public int TableID;
			public int SectionSyntaxIndicator;
			public int SectionLength;
			public int TableIDExtension;
			public int VersionNumber;
			public int CurrentNextIndicator;
			public int SectionNumber;
			public int LastSectionNumber;
			public int HeaderExtB8B9;
			public int HeaderExtB10B11;
			public int HeaderExtB12;
			public int HeaderExtB13;
		}
        #endregion

        #region Contructor/Destructor
		public DVBDemuxer()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(System.Windows.Forms.Application.StartupPath+@"\MediaPortal.xml"))
			{
				m_pluginsEnabled = xmlreader.GetValueAsBool("dvb_ts_cards", "enablePlugins", false);
			}
			m_secTimer.Elapsed+=new System.Timers.ElapsedEventHandler(m_secTimer_Elapsed);
			m_secTimer.Interval=5000;
			m_secTimer.AutoReset=false;
		}
        ~DVBDemuxer()
        {
        }
        #endregion

        #region global Vars
		int SECTIONS_BUFFER_WIDTH=1024+187;// 32kb for tables
        int m_teletextPid = 0;
        int m_subtitlePid = 0;
        int m_videoPid = 0;
        int m_audioPid = 0;
		int m_pmtPid = 0;
        string m_channelName = "";
        bool m_pluginsEnabled = false;
		// mhw
		bool m_mhwGrabbing=false;
		static ArrayList m_mhwChannels=new ArrayList();
		static ArrayList m_mhwTitles=new ArrayList();
		static ArrayList m_mhwSummaries=new ArrayList();
		static ArrayList m_mhwThemes=new ArrayList();
		// for pid 0xd3
		byte[] m_tableBufferD3=new byte[0];
		int m_bufferPositionD3=0;
		// for pid 0xd2
		byte[] m_tableBufferD2=new byte[0];
		int m_bufferPositionD2=0;
		// for dvb-sections
		int m_sectionPid=-1;
		int m_sectionTableID=-1;
		byte[] m_tableBufferSec=new byte[0];
		int m_bufferPositionSec=0;
		static ArrayList m_tableSections=new ArrayList();
		System.Timers.Timer m_secTimer=new System.Timers.Timer();
		int m_eitScheduleLastTable=0x50;
		int m_lastContinuityCounter=0;
		// pmt
		int m_currentPMTVersion=-1;
		int m_programNumber=-1;
		// card
		static int m_currentDVBCard=0;
		static NetworkType m_currentNetworkType;
		// for pmt pid
		byte[] m_tableBufferPMT=new byte[4096];
		int m_bufferPositionPMT=0;
		bool m_packetsReceived=false;
		DVBSectionHeader m_sectionHeader=new DVBSectionHeader();

		static DateTime epgRegrabTime=DateTime.MinValue;
        #endregion

        #region global Objects

        TSHelperTools.TSHeader m_packetHeader = new TSHelperTools.TSHeader();
        TSHelperTools m_tsHelper = new TSHelperTools();
        DVBTeletext m_teleText = new DVBTeletext();
        DVBEPG m_epgClass = new DVBEPG();
        AudioHeader m_usedAudioFormat = new AudioHeader();
        #endregion

        #region Imports

        [DllImport("SoftCSA.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern void PidCallback([In] IntPtr data);
        #endregion

        #region Delegates/Events
		// audio format
		public delegate bool OnAudioChanged(AudioHeader audioFormat);
        public event OnAudioChanged OnAudioFormatChanged; 
		// pmt handling
		public delegate void OnPMTChanged(byte[] pmtTable);
		public event OnPMTChanged OnPMTIsChanged;
	    // grab table
		public delegate void OnTableReceived(int pid,int tableID,ArrayList tableList);
		public event OnTableReceived OnGotTable;
		#endregion

        #region public functions
		public void GetEPGSchedule(int tableID,int programID)
		{
			
			if(tableID<0x50 || tableID>0x6f)
				return;
			if(m_sectionPid!=-1)
				return;
			SECTIONS_BUFFER_WIDTH=65535;
			m_eitScheduleLastTable=0x50;
			m_secTimer.Interval=10000;
			GetTable(0x12,tableID);// timeout 10 sec
			Log.Write("start getting epg for table 0x{0:X}",tableID);
		}
		public void GetMHWEPG()
		{
			if(m_sectionPid!=-1)
			{
				Log.Write("MHWEPG: section pid already set to {0:X}",m_sectionPid);
				return; // only grab when no other grabbing was started
			}
			m_tableBufferD2=new byte[66000];
			m_tableBufferD3=new byte[66000];
			m_mhwGrabbing=true;
			m_secTimer.Interval=30000;// grab for 30 sec.
			m_secTimer.Start();
		}
		public void ResetGrabber()
		{
			m_packetsReceived=false;
			m_sectionPid=-1;
			m_sectionTableID=-1;
			m_secTimer.Stop();
		}
		public void SetCardType(int cardType, NetworkType networkType)
		{
			m_currentDVBCard=cardType;
			m_currentNetworkType=networkType;
			m_epgClass=new DVBEPG(cardType,networkType);
		}

		public DVBSectionHeader GetSectionHeader(byte[] data)
		{
			return GetSectionHeader(data,0);
		}
		public DVBSectionHeader GetSectionHeader(byte[] data,int offset)
		{
			if(data==null)
				return new DVBSectionHeader();

			if(data.Length<14 || data.Length<offset+14)
				return new DVBSectionHeader();

			DVBSectionHeader header=new DVBSectionHeader();
			header.TableID = data[offset];
			header.SectionSyntaxIndicator = (data[offset+1]>>7) & 1;
			header.SectionLength = ((data[offset+1]& 0xF)<<8) + data[offset+2];
			header.TableIDExtension = (data[offset+3]<<8)+data[offset+4];
			header.VersionNumber = ((data[offset+5]>>1)&0x1F);
			header.CurrentNextIndicator = data[offset+5] & 1;
			header.SectionNumber = data[offset+6];
			header.LastSectionNumber = data[offset+7];
			header.HeaderExtB8B9=(data[offset+8]<<8)+data[offset+9];
			header.HeaderExtB10B11 = (data[offset+10]<<8)+data[offset+11];
			header.HeaderExtB12 = data[offset+12];
			header.HeaderExtB13 = data[offset+13];
			return header;
		}

		public bool GetTable(int pid,int tableID,int timeout)
		{
			if(pid<0 || pid>0x1FFF)
			{
				
				Log.Write("dvb-demuxer: GetTable invalid pid:{0:X}", pid);
				return false;
			}
			SECTIONS_BUFFER_WIDTH=1280;
			m_secTimer.Interval=timeout;
			return GetTable(pid,tableID);
		}
		public void SetChannelData(int audio, int video, int teletext, int subtitle, string channelName,int pmtPid, int programnumber)
		{
			m_secTimer.Stop();
			m_packetsReceived=false;
			epgRegrabTime= DateTime.MinValue;
			m_currentPMTVersion=-1;
			m_bufferPositionPMT=0;
			m_programNumber=-1;
			if (programnumber>0)
				m_programNumber=programnumber;
			// audio
			if (audio > 0x1FFF)
				m_audioPid = -1;
			else
				m_audioPid = audio;
			// video
			if (video > 0x1FFF)
				m_videoPid = -1;
			else
				m_videoPid = video;
			// teletext
			if (teletext > 0x1FFF)
				m_teletextPid = -1;
			else
				m_teletextPid = teletext;
			// subtitle
			if (subtitle > 0x1FFF)
				m_subtitlePid = -1;
			else
				m_subtitlePid = subtitle;
			// pmt pid
			if (pmtPid > 0x1FFF)
				m_pmtPid = -1;
			else
				m_pmtPid = pmtPid;
			// name
			m_channelName = "";
			if (channelName != null)
				if (channelName != "")
				{
					m_channelName = channelName;
				}

			// clear buffers and reset pids

			m_tableBufferD2=new byte[0];
			m_tableBufferD3=new byte[0];
			m_tableBufferSec=new byte[0];
			m_tableSections.Clear();
			m_mhwGrabbing=false;
			m_sectionPid=-1;
			m_sectionTableID=-1;

			//
			m_epgClass.ClearBuffer();
			m_teleText.ClearBuffer();
		//	Log.Write("DVBDemuxer:{0} audio:{1:X} video:{2:X} teletext:{3:X} pmt:{4:X} subtitle:{5:X}",
		//		channelName,audio, video, teletext, pmtPid,subtitle);
		
		}

        #endregion

		#region functions
		bool IsEPGScheduleGrabbing()
		{
			if(m_sectionTableID>=0x50 && m_sectionTableID<=0x6f && m_sectionPid==0x12)
			{
				return true;
			}
			return false;
		}

		bool GetTable(int pid,int tableID)
		{
			if(m_sectionPid!=-1)
			{
				Log.Write("dvb-demuxer: sectionpid already set to:{0:X}", m_sectionPid);
				return false;
			}
			m_tableBufferSec=new byte[SECTIONS_BUFFER_WIDTH+1];
			m_tableSections=new ArrayList();
			m_packetsReceived=false;
			m_sectionPid=pid;
			m_sectionTableID=tableID;
			return true;
		}
		void ProcessEPGData()
		{
			Thread epgThread=new Thread(new ThreadStart(ProcessEPG));
			epgThread.IsBackground=false;
			epgThread.Priority=ThreadPriority.Lowest;
			epgThread.Name="EPG-Parser";
			epgThread.Start();
			if(epgThread.Join(1000)==true)
				Log.WriteFile(Log.LogType.EPG,"thread end:{0}",epgThread.Name);
		}
		void GetAllSections()
		{
			if(m_tableBufferSec==null)
				return;
			if(m_tableBufferSec.Length<SECTIONS_BUFFER_WIDTH)
				return;
			lock(m_tableBufferSec.SyncRoot)
			{
				byte[] data=(byte[])m_tableBufferSec.Clone();
				for(int i=0;i<SECTIONS_BUFFER_WIDTH;)
				{
					if(i+13>=SECTIONS_BUFFER_WIDTH)
						break;
					if(data[i]==0xff)
					{
						i++;
						continue;
					}
					DVBSectionHeader header=GetSectionHeader(data,i);
					if(header.SectionLength<1 || header.SectionLength+4+i>SECTIONS_BUFFER_WIDTH)
						break;
					try
					{
						Array.Copy(data,i,m_tableBufferSec,0,header.SectionLength+3);
					}
					catch
					{
					}
					if(header.TableID==m_sectionTableID)
						ParseSection();
					i+=header.SectionLength+4;
				}
			}
		}
		void ClearGrabber()
		{
			// timeout for grabbing sections
			// clean up
			m_secTimer.Stop();

			int currentTable=m_sectionTableID;
			int currentPid=m_sectionPid;

			if(currentTable>=0x50 && currentTable<=0x6f && currentPid==0x12)
			{
				m_sectionPid=-1;
				ProcessEPGData();
			}
			else if(OnGotTable!=null)
			{			
				m_sectionPid=-1;
				OnGotTable(currentPid,currentTable,(ArrayList)m_tableSections.Clone());
				m_tableSections.Clear();
			}

			
			ClearSectionsGrabber();

			if(currentTable>=0x50 && currentTable<=0x6f && currentPid==0x12)
			{
				if(m_eitScheduleLastTable>currentTable)
				{
					int table=currentTable+1;
					Log.Write("additional grab table id {0}",table);
					GetEPGSchedule(table,0);
				}
			}
			GC.Collect();
			GC.Collect();
		}

		void ClearSectionsGrabber()
		{
			m_tableBufferSec=new byte[0];
			m_sectionPid=-1;
			m_sectionTableID=-1;
			m_bufferPositionSec=0;
			m_tableSections=new ArrayList();
		}
		UInt32 GetCRC32(byte[] data)
		{
			UInt32 crc = 0xffffffff;
			for (UInt32 i=0;i<data.Length-4;i++) 
			{
				crc = (crc << 8) ^ CRC32Data[((crc >> 24) ^ data[i]) & 0xff];
			}
			return crc;

		}
		UInt32 GetCRC32(byte[] data,UInt32 skip,UInt32 len)
		{
			UInt32 crc = 0xffffffff;
			for (UInt32 i=skip;i<len;++i) 
			{
				crc = (crc << 8) ^ CRC32Data[((crc >> 24) ^ data[i]) & 0xff];
			}
			return crc;

		}

		UInt32 GetSectionCRCValue(byte[] data,int ptr)
		{
			if(data.Length<ptr+3)
				return (UInt32)0;

			
			return (UInt32)((data[ptr]<<24)+(data[ptr+1]<<16)+(data[ptr+2]<<8)+data[ptr+3]);
		}
		DVBSectionHeader GetHeader()
		{
			return GetSectionHeader(m_tableBufferSec);
		}
		bool ParseAudioHeader(byte[] data, ref AudioHeader header)
        {

            header = new AudioHeader();
            int limit = 32;

            if ((data[0] & 0xFF) != 0xFF || (data[1] & 0xF0) != 0xF0)
                return false;

            header.ID = ((data[1] >> 3) &0x01) ;
            header.Emphasis = data[3] & 0x03;

            if (header.ID == 1 && header.Emphasis == 2)
                header.ID = 2;
            header.Layer = ((data[1] >>1) &0x03);

            if (header.Layer < 1)
                return false;

            header.ProtectionBit = (data[1] & 0x01) ^ 1;
            header.Bitrate = AudioBitrates[header.ID, header.Layer - 1, ((data[2] >>4)& 0x0F)];
            if (header.Bitrate < 1)
                return false;
            header.SamplingFreq = AudioFrequencies[header.ID, ((data[2] >>2)& 0x03)];
            if (header.SamplingFreq == 0)
                return false;

            header.PaddingBit = ((data[2] >>1)& 0x01) ;
            header.PrivateBit = data[2] & 0x01;

            header.Mode = ((data[3] >>6)& 0x03) & 0x03;
            header.ModeExtension = ((data[3] >>4)& 0x03) ;
            if (header.Mode == 0)
                header.ModeExtension = 0;

            header.Bound = (header.Mode == 1) ? ((header.ModeExtension + 1) << 2) : limit;
            header.Channel = (header.Mode == 3) ? 1 : 2;
            header.Copyright = ((data[3]>>3) & 0x01);
            header.Original = ((data[3] >>2)& 0x01) ;
            header.TimeLength = (int)(AudioTimes[header.Layer] / header.SamplingFreq);

            if (header.ID == 1 && header.Layer == 2)
            {	

				if (header.Bitrate / header.Channel < 32000)
                    return false;
                if (header.Bitrate / header.Channel > 192000)
                    return false;

                if (header.Bitrate < 56000)
                {
                    if (header.SamplingFreq == 32000)
                        limit = 12;
                    else
                        limit = 8;
                }
                else if (header.Bitrate < 96000)
                    limit = 27;
                else
                {
                    if (header.SamplingFreq == 48000)
                        limit = 27;
                    else
                        limit = 30;
                }
                if (header.Bound > limit)
                    header.Bound = limit;
            }
            else if (header.Layer == 2)  // MPEG-2
            {
                limit = 30;
            }

            if (header.Layer < 3)
            {
                if (header.Bound > limit)
                    header.Bound = limit;
                header.Size = (header.SizeBase = 144 * header.Bitrate / header.SamplingFreq) + header.PaddingBit;
                return true;
            }
            else
            {
                limit = 32;
                header.Size = (header.SizeBase = (12 * header.Bitrate / header.SamplingFreq) * 4) + (4 * header.PaddingBit);
                return true;
            }

        }
		void SaveData(int pid,int tableID,byte[] data)
		{
			lock(data.SyncRoot)
			{
				if(pid==0xd3)
				{
					if(tableID==0x91)
						m_epgClass.ParseChannels(data);
					else if(tableID==0x90)
						m_epgClass.ParseSummaries(data);
					else if(tableID==0x92)
						m_epgClass.ParseThemes(data);

				}
				if(pid==0xd2)
				{
					if(tableID==0x90)
						m_epgClass.ParseTitles(data);
				}

			}
		}

        byte[] GetAudioHeader(byte[] data)
        {
            int pos=0;
            bool found = false;
            for (; pos < data.Length; pos++)
            {
                if (data[pos] == 0 && data[pos + 1] == 0 && data[pos + 2] == 1)
                {
                    found = true;
                    break;
                }
            }
            if (found == false)
                return new byte[184];


            if ((data[pos+3] & 0xE0) == 0xC0)
            {
                if ((data[pos + 3] & 0xE0) == 0xC0)
                {
                    System.Array.Copy(data, pos, data, 0, data.Length - pos);
                    return GetPES(data);
                }

            }
            return new byte[184];
        }
        public byte[] GetPES(byte[] data)
        {
            byte[] pesData=new byte[184];
            int ptr = 0; 
            int offset = 0; 
            bool isMPEG1=false;

            int i = 0;
            for (; i < data.Length; )
            {
                ptr = (0xFF & data[i + 4]) << 8 | (0xFF & data[i + 5]);
                isMPEG1 = (0x80 & data[i + 6]) == 0 ? true : false;
                offset = i + 6 + (!isMPEG1 ? 3 + (0xFF & data[i + 8]) : 0);

                Array.Copy(data,offset,pesData,0, data.Length-offset);
                i += 6 + ptr;
            }

            return pesData;
        }
		bool SectionIsInBuffer(int sectionNumber,int serviceID)
		{
			lock(m_tableSections.SyncRoot)
			{
				if(m_tableSections==null)
				{
					m_tableSections=new ArrayList();
					return false;
				}
				foreach(byte[] data in m_tableSections)
				{
					DVBSectionHeader header=GetSectionHeader(data);
					if(header.SectionNumber==sectionNumber && header.TableIDExtension==serviceID)
						return true;
				}
			}
			return false;
		}
		bool SectionIsInBuffer(int sectionNumber)
		{
			lock(m_tableSections.SyncRoot)
			{
				
				if(m_tableSections==null)
				{
					m_tableSections=new ArrayList();
					return false;
				}
				foreach(byte[] data in m_tableSections)
				{
					DVBSectionHeader header=GetSectionHeader(data);
					if(header.SectionNumber==sectionNumber)
						return true;
				}
			}
			return false;
		}
		static void ProcessEPG()
		{
			if(m_currentDVBCard==(int)DVBEPG.EPGCard.Invalid || m_currentDVBCard==(int)DVBEPG.EPGCard.Unknown)
				return ;
			ArrayList dataList=new ArrayList();
			if(m_tableSections!=null)
			{
				dataList=(ArrayList)m_tableSections.Clone();
				m_tableSections.Clear();
			}
			DVBEPG		tmpEPGClass=new DVBEPG(m_currentDVBCard,m_currentNetworkType);
			Log.WriteFile(Log.LogType.EPG,"mhw started thread {0}",System.Threading.Thread.CurrentThread.Name);
			int count=0;
			if(dataList!=null)
			{
				count+=tmpEPGClass.GetEPG(dataList,0, out epgRegrabTime);
			}
			if(m_mhwChannels!=null && m_mhwTitles!=null && m_mhwThemes!=null && m_mhwSummaries!=null)
			{
				if(m_mhwChannels.Count>0)
				{
					tmpEPGClass.SetMHWBuffer(m_mhwChannels,m_mhwTitles,m_mhwSummaries,m_mhwThemes);
					count+=tmpEPGClass.GetEPG( out epgRegrabTime);
					m_mhwChannels.Clear();
					m_mhwTitles.Clear();
					m_mhwThemes.Clear();
					m_mhwSummaries.Clear();
				}
			}
			Log.WriteFile(Log.LogType.EPG,"mhwepg ready. added {0} events to database! Next grab at {1}",count, epgRegrabTime.ToString() );
		}

		private void m_secTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(m_mhwGrabbing==false)
			{
				//Log.Write("dvb-demuxer: timeout for pid:{0} tableid:{1}",m_sectionPid, m_sectionTableID);//change
				m_secTimer.Stop();
				ClearGrabber();// clear up
			}
			else
			{
				// delete buffers and save epg-data
				Log.Write("MHW-EPG grabbing stop");
				m_secTimer.Stop();
				m_mhwGrabbing=false;
				m_epgClass.GetMHWBuffer(ref m_mhwChannels,ref m_mhwTitles,ref m_mhwSummaries,ref m_mhwThemes);
				ProcessEPGData();
				m_tableBufferD2=new byte[0];
				m_tableBufferD3=new byte[0];

			}
		}

		#endregion

        #region Properties

        public DVBTeletext Teletext
        {
            get { return m_teleText; }
        }
        #endregion

        #region ISampleGrabberCB Members
        #region Unused SampleCB()

        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            //throw new Exception("The method or operation is not implemented.");
            return 0;
        }
        #endregion
			
#if MAKEDUMP
		System.IO.BinaryWriter writer=null;
		System.IO.FileStream stream=null;
		ulong fileLen=0;
#endif
		public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
		{
			int add = (int)pBuffer;
			int end = add + BufferLen;

			// when here, we can set graph as running
#if MAKEDUMP
			if (fileLen==0&&writer==null && stream==null)
			{
				fileLen=0;
				stream = new System.IO.FileStream("dump4GB.ts",System.IO.FileMode.Create,System.IO.FileAccess.Write,System.IO.FileShare.None);
				writer = new System.IO.BinaryWriter(stream);
			}
			
			if (writer!=null)
			{
				for (int ptr = add; ptr < end; ptr += 188)//main loop
				{
					m_packetHeader=m_tsHelper.GetHeader((IntPtr)ptr);
					if (m_packetHeader.Pid==2222 ||
						m_packetHeader.Pid==2220 ||
						m_packetHeader.Pid==0)
					{
						byte[] byData=new byte[188];
						Marshal.Copy((IntPtr)ptr,byData,0,188);
						writer.Write(byData,0,188);
						fileLen +=(ulong)188;
					}

					//limit to 100MB
					if (fileLen > 1024L*1024L*100L)
					{
						writer.Close();
						stream.Close();
						writer=null;
						stream=null;
					}
				}
			}
#endif

			for (int ptr = add; ptr < end; ptr += 188)//main loop
			{
				if (m_pluginsEnabled == true)
					PidCallback((IntPtr)ptr);
				
				m_packetHeader=m_tsHelper.GetHeader((IntPtr)ptr);
				if(m_packetHeader.SyncByte!=0x47) 
					continue;
				if(m_packetHeader.TransportError==true)
				{
//					if(m_sectionPid!=-1 && m_packetHeader.Pid==m_sectionPid)
//						Log.Write("pid:0x{0:X} transport error", m_sectionPid);
					continue;// error, ignore packet
				}
/*
				if (m_packetHeader.Pid==0)
				{
					Log.Write("pid:0x{0:X} pos:{1} cont:{2} adapt:{3} payloadunitstart:{4} len:{5} {0:X}",
						m_packetHeader.Pid,
						m_bufferPositionSec,
						m_packetHeader.ContinuityCounter,
						m_packetHeader.AdaptionFieldControl,
						m_packetHeader.PayloadUnitStart,
						m_packetHeader.SectionLen,
						m_sectionPid);
				}*/
				// teletext

				if (m_packetHeader.Pid==m_teletextPid && m_teleText != null && m_teletextPid>0)
				{
//					Log.Write("grab teletext 0x{0:X}",m_teletextPid);
					m_teleText.SaveData((IntPtr)ptr);
				}

				if(m_packetsReceived==false)
				{
					m_secTimer.Start();
					m_packetsReceived=true;
				}
				// plugins
				// get the header object
				// audio & video
				#region subtitles
				if(m_subtitlePid>0 && m_subtitlePid==m_packetHeader.Pid)
				{
				}

				#endregion

				#region Audio & Video
				if (m_packetHeader.Pid == m_audioPid && m_audioPid > 0)
				{
					if(OnAudioFormatChanged!=null)
					{
						//					Log.Write("got audio pid:0x{0:X}", m_audioPid);
						if (m_packetHeader.PayloadUnitStart == true)// start
						{
							AudioHeader ah = new AudioHeader();
							byte[] packet = new byte[184];
							Marshal.Copy((IntPtr)(ptr+4), packet, 0, 184);
							if (ParseAudioHeader((byte[])GetAudioHeader(packet).Clone(),ref ah) == true)
							{
								if (ah.Bitrate!=m_usedAudioFormat.Bitrate || 
									ah.Channel!=m_usedAudioFormat.Channel ||
									ah.SamplingFreq!=m_usedAudioFormat.SamplingFreq ||
									ah.Layer!=m_usedAudioFormat.Layer ||
									ah.Mode!=m_usedAudioFormat.Mode)
								{
										bool success=OnAudioFormatChanged(ah);
										if(success) m_usedAudioFormat = ah;
								}
							}
						}
					}
				}
				/*
				if (m_packetHeader.Pid == m_videoPid && m_videoPid > 0)
				{
//					Log.Write("got video pid:0x{0:X}", m_videoPid);
					if (m_packetHeader.PayloadUnitStart == true)// start
					{
						byte[] packet = new byte[188];
						Marshal.Copy((IntPtr)ptr, packet, 0, 188);
					}
				}
				*/
				#endregion

				#region mhw grabbing
				if(m_mhwGrabbing==true)// only grab from epg-grabber
				{
//					Log.Write("grab mhw");
					try
					{

						int offset=0;
						//
						// calc offset & pointers
						if(m_packetHeader.PayloadUnitStart==true && m_packetHeader.AdaptionFieldControl==1)
							offset=1;
						//
						// copy data and set grabbing flag
						// mhw (pids 0xd3 & 0xd2)

						if(m_packetHeader.Pid==0xd2)
						{
							if(m_bufferPositionD2+(184-offset)<65534)
							{
								Marshal.Copy((IntPtr)(ptr+4+offset),m_tableBufferD2,m_bufferPositionD2,184-offset);
								m_bufferPositionD2+=(184-offset);
							}
							else
								GetTablesD2();
						}
						if(m_packetHeader.Pid==0xd3)
						{
							if(m_bufferPositionD3+(184-offset)<65534)
							{
								Marshal.Copy((IntPtr)(ptr+4+offset),m_tableBufferD3,m_bufferPositionD3,184-offset);
								m_bufferPositionD3+=(184-offset);
							}
							else
								GetTablesD3();
								
						}

					}
					catch(Exception ex)
					{
						Log.Write("mhw-epg: exception {0} source:{1}",ex.Message,ex.StackTrace);
					}

				
				}

				#endregion

				#region pmt handling
				if(m_pmtPid >0 && m_packetHeader.Pid==m_pmtPid && OnPMTIsChanged!=null)
				{
//					if (m_currentPMTVersion==-1)
//						Log.Write("grab pmt:0x{0:X} adpt:{1} pos:{2}",m_pmtPid,m_packetHeader.AdaptionFieldControl,m_bufferPositionPMT);
					try
					{
						int offset=0;
						//
						// calc offset & pointers
						if(m_packetHeader.AdaptionFieldControl==2 || m_packetHeader.AdaptionFieldControl==3)
							continue;
						if(m_packetHeader.PayloadUnitStart==true && m_bufferPositionSec==0)
							offset=m_packetHeader.AdaptionField+1;
						else if(m_packetHeader.PayloadUnitStart==true)
							offset=1;
		
						if(m_bufferPositionPMT+(184-offset)<=4093)
						{
							Marshal.Copy((IntPtr)(ptr+4+offset),m_tableBufferPMT,m_bufferPositionPMT,184-offset);
							m_bufferPositionPMT+=(184-offset);
						}
						DVBSectionHeader header=GetSectionHeader(m_tableBufferPMT,0);

						if(m_bufferPositionPMT>=header.SectionLength+3 && header.TableID==0x02 && header.SectionLength>0)
						{
							if(header.VersionNumber!=m_currentPMTVersion)
							{
							//	Log.Write("Got new PMT version:{0} progr:{1:X}=={2:X}",
							//		header.VersionNumber,m_programNumber,header.TableIDExtension);
								int len=header.SectionLength+3;
								byte[] data=new byte[len];
								Array.Copy(m_tableBufferPMT,0,data,0,len);
								UInt32 crc1=GetCRC32(data);
								UInt32 crc2=GetSectionCRCValue(data,len-4);
								if(crc1==crc2)
								{
									if (m_programNumber <=0 || m_programNumber == header.TableIDExtension)
									{
										OnPMTIsChanged((byte[])data.Clone());
										m_currentPMTVersion=header.VersionNumber;
									}
								}
								else 
								{
									//Log.Write("PMT CRC error");
								}
							}
							m_bufferPositionPMT=0;
						}
					}
					catch(Exception ex)
					{
						Log.Write("mhw-epg: exception {0} source:{1}",ex.Message,ex.StackTrace);
					}

				
				}
			
				#endregion

				#region sections

				if(m_sectionPid!=-1 && m_packetHeader.Pid==m_sectionPid)
				{
					try
					{
/*						Log.Write("pid:0x{0:X} pos:{1} cont:{2} adapt:{3} payloadunitstart:{4} len:{5}",
												m_packetHeader.Pid,
												m_bufferPositionSec,
												m_packetHeader.ContinuityCounter,
												m_packetHeader.AdaptionFieldControl,
												m_packetHeader.PayloadUnitStart,
												m_packetHeader.SectionLen);*/
						int offset=0;
						//
						// 
						// check continuity counter
						if(m_bufferPositionSec>0)
						{
							int counter=m_lastContinuityCounter;
							if(counter==15)
								counter=-1;
							if(counter+1!=m_packetHeader.ContinuityCounter)
							{
								//Log.Write("dvb-demuxer: continuity counter dont match for pid {0}!={1}",counter+1,m_packetHeader.ContinuityCounter);
								m_bufferPositionSec=0;
							}
						}
						// calc offset
						if(m_packetHeader.AdaptionFieldControl==2)
						{
							//Log.Write("ignore adapt=2");
							continue;
						}
						if(m_packetHeader.AdaptionFieldControl==3)
						{
							//Log.Write("ignore adapt=3");
							continue;
						}
						if(m_packetHeader.PayloadUnitStart==true && m_bufferPositionSec==0)
							offset=m_packetHeader.AdaptionField+1;
						else if(m_packetHeader.PayloadUnitStart==true)
							offset=1;
						//
						// start copy data for every section on its table-id-byte
						if(m_bufferPositionSec==0 && m_sectionTableID!=Marshal.ReadByte((IntPtr)(ptr+4+offset)))
						{
							//Log.Write("ignore sectiontableid wrong");
							continue;
						}
						//
						// copy data
						if(m_bufferPositionSec+(184-offset)<=SECTIONS_BUFFER_WIDTH)
						{
							Marshal.Copy((IntPtr)(ptr+4+offset),m_tableBufferSec,m_bufferPositionSec,184-offset);
							m_bufferPositionSec+=(184-offset);
							
							DVBSectionHeader header=GetHeader();
							m_lastContinuityCounter=m_packetHeader.ContinuityCounter;

							if(IsEPGScheduleGrabbing()==false && m_bufferPositionSec>=header.SectionLength && header.SectionLength>0)
							{
//								Log.Write("Parse...");
								ParseSection();
							}
								
						}
						else 
						{
							DVBSectionHeader header=GetHeader();
							if(IsEPGScheduleGrabbing()==false && m_bufferPositionSec>header.SectionLength && header.SectionLength>0)
							{
//								Log.Write("Parse2...");
								ParseSection();
							}
							else if(IsEPGScheduleGrabbing()==true)
								GetAllSections();
							m_bufferPositionSec=0;
						}
					}
					catch(Exception ex)
					{
						Log.WriteFile(Log.LogType.EPG,"mhw-epg: exception {0} source:{1}",ex.Message,ex.StackTrace);
					}

				
				}
				#endregion


			}
            return 0;
        }

        #endregion

		#region Table Handling / saving data
		void GetTablesD2()
		{
			int tableID=0;
			int sectionLen=0;

			lock(m_tableBufferD2.SyncRoot)
			{
				int ptr=0;
				// find first start of table
				do
				{
					if(ptr>=0 && ptr<65533)
					{
						if(ptr>65534 || ptr<0)
							break;
						tableID=m_tableBufferD2[ptr];
						sectionLen=((m_tableBufferD2[ptr+1]-0x70)<<8)+m_tableBufferD2[ptr+2];
						// table ok?
						if((tableID==0x90 || tableID==0x91) && (m_tableBufferD2[ptr+1]>=0x70 && m_tableBufferD2[ptr+1]<=0x7F))
						{
							// found table
							// ignore last data and ready
							if(ptr+sectionLen+3>=65535) break;
							if(sectionLen<1) continue;
							if(sectionLen+3>65533) continue;
							byte[] data=new byte[sectionLen+3];
							try
							{
								Array.Copy(m_tableBufferD2,ptr,data,0,sectionLen+3);
								SaveData(0xd2,tableID,data);
							}
							catch
							{
							}
							ptr+=sectionLen+3;
						}// else ptr+=1
						else
							ptr++;
						
					}
					else break;
				}while(1!=0);
				// clear up
				m_bufferPositionD2=0;
			}
		}
		void GetTablesD3()
		{
			int tableID=0;
			int sectionLen=0;

			lock(m_tableBufferD3.SyncRoot)
			{
				int ptr=0;
				// find first start of table
				do
				{
					if(ptr>=0 && ptr<65533)
					{
						if(ptr>65534 || ptr<0)
							break;
						tableID=m_tableBufferD3[ptr];
						sectionLen=((m_tableBufferD3[ptr+1]-0x70)<<8)+m_tableBufferD3[ptr+2];
						// table ok?
						if((tableID==0x90 || tableID==0x91 || tableID==0x92) && (m_tableBufferD3[ptr+1]>=0x70 && m_tableBufferD3[ptr+1]<=0x7F))
						{
							// found table
							// ignore last data and ready
							
							if(ptr+sectionLen+3>=65535) break;
							if(sectionLen<1) continue;
							if(sectionLen+3>65533) continue;
							byte[] data=new byte[sectionLen+3];
							try
							{
								Array.Copy(m_tableBufferD3,ptr,data,0,sectionLen+3);
								SaveData(0xd3,tableID,data);
							}
							catch
							{
							}
							ptr+=sectionLen+3;
						}// else ptr+=1
						else
							ptr++;
						
					}
					else break;
				}while(1!=0);
				// clear up
				m_bufferPositionD3=0;
			}

		}

		void ParseSection()
		{

			lock(m_tableSections.SyncRoot)
			{
				int ptr=0;
				byte[] data=new byte[0];
				DVBSectionHeader header=new DVBSectionHeader();

				header=GetHeader();
				header.SectionLength+=3;
				bool sectionOK=false;
				// table ok?
//				Log.Write("table id:{0} =={1}",header.TableID,m_sectionTableID);
				if(header.TableID==m_sectionTableID)
				{
					// found table
					// ignore last data and ready
					if(ptr+header.SectionLength>=65535) return;
					if(header.SectionLength<1) return;
					if(header.SectionLength>65535) return;
					data=new byte[header.SectionLength];
					try
					{
						Array.Copy(m_tableBufferSec,ptr,data,0,header.SectionLength);
						UInt32 crc1=GetCRC32(data);
						UInt32 crc2=GetSectionCRCValue(data,header.SectionLength-4);
						if(crc1==crc2)
						{
							if(header.TableID>m_eitScheduleLastTable)
								m_eitScheduleLastTable=header.HeaderExtB13;
							sectionOK=true;
						}
						else
						{
							//Log.WriteFile(Log.LogType.Error,"CRC error:{0:X} != {1:X}", crc1,crc2);
							sectionOK=false;
							// reject the section
						}
					}
					catch
					{
						sectionOK=false;
					}
				}

				// clean up
				if(sectionOK==false)
				{
					m_bufferPositionSec=0;
					return;
				}

				if(IsEPGScheduleGrabbing()) // check section-number and service-id
				{
					if(SectionIsInBuffer(header.SectionNumber,header.TableIDExtension)==false)
					{
//						Log.Write("added section-number: {0} segment last: {1} last_section: {2} section len:{3}",header.SectionNumber,header.HeaderExtB12,header.LastSectionNumber,header.SectionLength);
						m_secTimer.Stop();
						m_secTimer.Interval=6000;
						m_secTimer.Start();
						m_tableSections.Add(data);
						// timeout only after grabbing last section
					}
				}
				else 
				{
					if(SectionIsInBuffer(header.SectionNumber)==false)
					{
//						Log.Write("added section-number: {0} segment last: {1} last_section: {2} section len:{3}",header.SectionNumber,header.HeaderExtB12,header.LastSectionNumber,header.SectionLength);
						m_secTimer.Stop();
						m_secTimer.Start();
						m_tableSections.Add(data);

					}
					// timeout only after grabbing last section
				}
				// check if table is complete
				if(IsEPGScheduleGrabbing()==false && m_tableSections.Count-1==header.LastSectionNumber)
				{
					// ready, clean up
					m_secTimer.Stop();
					ClearGrabber();
				}
				else
				{
					m_bufferPositionSec=0;// grab next
				}
				
				if (IsEPGScheduleGrabbing()==false)
				{
					if (epgRegrabTime!=DateTime.MinValue)
					{
						if (DateTime.Now.AddMinutes(10) > epgRegrabTime)
						{
							GetEPGSchedule(0x50,0);
						}
					}
				}
			}
		}
		// parsesection
		#endregion

		//
		//
	}//class dvbdemuxer
 
}//namespace
