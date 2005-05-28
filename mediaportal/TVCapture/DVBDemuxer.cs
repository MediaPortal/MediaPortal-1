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
			// calc crc table
		}
        ~DVBDemuxer()
        {
        }
        #endregion

        #region global Vars
		int SECTIONS_BUFFER_WIDTH=65535;// 32kb for tables
        const int m_mhwPid1 = 0xD2;
        const int m_mhwPid2 = 0xD3;
        int m_teletextPid = 0;
        int m_subtitlePid = 0;
        int m_videoPid = 0;
        int m_audioPid = 0;
		int m_pmtPid = 0;
        string m_channelName = "";
        bool m_pluginsEnabled = false;
		// for pid 0xd3
		byte[] m_tableBufferD3=new byte[65535];
		int m_bufferPositionD3=0;
		// for pid 0xd2
		byte[] m_tableBufferD2=new byte[65535];
		int m_bufferPositionD2=0;
		// for dvb-sections
		int m_sectionPid=-1;
		int m_sectionTableID=-1;
		byte[] m_tableBufferSec=new byte[65535];
		int m_bufferPositionSec=0;
		static ArrayList m_tableSections=new ArrayList();
		System.Timers.Timer m_secTimer=new System.Timers.Timer();
		int m_eitScheduleLastTable=0x50;
		// pmt
		int m_currentPMTVersion=-1;
		// card
		static int m_currentDVBCard=0;
		static NetworkType m_currentNetworkType;
		// for pmt pid
		int m_grabbingLenPMT=0;
		bool m_grabbingPMT=false;
		byte[] m_tableBufferPMT=new byte[4096];
		int m_bufferPositionPMT=0;
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

			if(data.Length<14)
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
			m_secTimer.Interval=timeout;
			return GetTable(pid,tableID);
		}
		public bool GetTable(int pid,int tableID)
		{
			if(m_sectionPid!=-1)
				return false;
			m_tableBufferSec=new byte[SECTIONS_BUFFER_WIDTH+1];
			m_tableSections=new ArrayList();
			m_sectionPid=pid;
			m_sectionTableID=tableID;
			m_secTimer.AutoReset=false;
			m_secTimer.Start();
			return true;
		}
		public void GetEPGSchedule(int tableID,int programID)
		{
			if(tableID<0x50 || tableID>0x6f)
				return;
			if(m_sectionPid!=-1)
				return;
			m_eitScheduleLastTable=0x50;
			GetTable(0x12,tableID,10000);// timeout 10 sec
		}
		void ProcessEPGData()
		{
			Thread epgThread=new Thread(new ThreadStart(ProcessEPG));
			epgThread.IsBackground=false;
			epgThread.Priority=ThreadPriority.Lowest;
			epgThread.Name="EPG-Parser";
			epgThread.Start();
			if(epgThread.Join(1000)==true)
				Log.Write("thread end:{0}",epgThread.Name);
		}
		public void SetChannelData(int audio, int video, int teletext, int subtitle, string channelName,int pmtPid)
		{
			epgRegrabTime= DateTime.MinValue;
			m_currentPMTVersion=-1;
			// audio
			if (audio > 0x1FFD)
				m_audioPid = 0;
			else
				m_audioPid = audio;
			// video
			if (video > 0x1FFD)
				m_videoPid = 0;
			else
				m_videoPid = audio;
			// teletext
			if (teletext > 0x1FFD)
				m_teletextPid = 0;
			else
				m_teletextPid = teletext;
			// subtitle
			if (subtitle > 0x1FFD)
				m_subtitlePid = 0;
			else
				m_subtitlePid = subtitle;
			// pmt pid
			if (pmtPid > 0x1FFD)
				m_pmtPid = 0;
			else
				m_pmtPid = pmtPid;
			// name
			m_channelName = "";
			if (channelName != null)
				if (channelName != "")
				{
					m_channelName = channelName;
				}

			// clear buffers

			m_epgClass.ClearBuffer();
			m_teleText.ClearBuffer();
			Log.Write("DVBDemuxer:{0} audio:{1:X} video:{2:X} teletext:{3:X} pmt:{4:X} subtitle:{5:X}",
				channelName,audio, video, teletext, pmtPid,subtitle);
		
		}


        #endregion

		#region functions
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
				OnGotTable(m_sectionPid,m_sectionTableID,(ArrayList)m_tableSections.Clone());
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
                //int next = a + 6 + ( (0xFF & check[a+4])<<8 | (0xFF & check[a+5]) );

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
			return false;
		}
		bool SectionIsInBuffer(int sectionNumber)
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
			return false;
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
        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            int add = (int)pBuffer;
            int end = add + BufferLen;

			for (int ptr = add; ptr < end; ptr += 188)//main loop
			{
				if (m_pluginsEnabled == true)
					PidCallback((IntPtr)ptr);
				
				m_packetHeader=m_tsHelper.GetHeader((IntPtr)ptr);
				if(m_packetHeader.TransportError==true)
					continue;// error, ignore packet
				// teletext
				if (m_teleText != null)
					m_teleText.SaveData((IntPtr)ptr);
				// plugins
				// get the header object
				// audio & video
				#region Audio & Video
				if (m_packetHeader.Pid == m_audioPid && m_audioPid > 0)
				{
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
								if(OnAudioFormatChanged!=null)
								{
									bool success=OnAudioFormatChanged(ah);
									if(success) m_usedAudioFormat = ah;
								}
                                
							}
						}
					}
				}
				if (m_packetHeader.Pid == m_videoPid && m_videoPid > 0)
				{
					if (m_packetHeader.PayloadUnitStart == true)// start
					{
						byte[] packet = new byte[188];
						Marshal.Copy((IntPtr)ptr, packet, 0, 188);
					}
				}
				#endregion

				#region mhw grabbing
				if(GUIGraphicsContext.DX9Device==null)// only grab from epg-grabber
				{
					m_packetHeader.Payload=new byte[184];
					Marshal.Copy((IntPtr)(ptr+4),m_packetHeader.Payload,0,184);
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
									Array.Copy(m_packetHeader.Payload,offset,m_tableBufferD2,m_bufferPositionD2,184-offset);
									m_bufferPositionD2+=(184-offset);
								}
								else
									GetTablesD2();
							}
							if(m_packetHeader.Pid==0xd3)
							{
								if(m_bufferPositionD3+(184-offset)<65534)
								{
									Array.Copy(m_packetHeader.Payload,offset,m_tableBufferD3,m_bufferPositionD3,184-offset);
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
				if(m_packetHeader.Pid==m_pmtPid && OnPMTIsChanged!=null)
				{
					bool pmtComplete=false;
					m_packetHeader.Payload=new byte[184];
					Marshal.Copy((IntPtr)(ptr+4),m_packetHeader.Payload,0,184);
					int sectionLen = ((m_packetHeader.Payload[2]& 0xF)<<8) + m_packetHeader.Payload[3];
					sectionLen+=3;
					int ptr1=-1;
					int offset=0;
					
					if(m_packetHeader.PayloadUnitStart==true)
						m_grabbingLenPMT=sectionLen;

					if(m_grabbingLenPMT>183)
					{
						
						pmtComplete=false;
						if(m_packetHeader.AdaptionFieldControl==3)
							offset++;
						if(m_packetHeader.AdaptionFieldControl==1 && m_packetHeader.PayloadUnitStart==true)
							ptr1=m_packetHeader.AdaptionField+1;
	
						if(ptr1==1 && m_grabbingPMT==false)
						{
							// copy from packet
							Array.Copy(m_packetHeader.Payload,1,m_tableBufferPMT,0,183);
							m_bufferPositionPMT=183;
							m_grabbingPMT=true;
						}
						if(ptr1>1)
						{
							if(m_grabbingPMT==true && m_bufferPositionPMT!=0)
							{
								Array.Copy(m_packetHeader.Payload,0,m_tableBufferPMT,m_bufferPositionPMT,183-ptr1);
								m_grabbingPMT=false;
								m_bufferPositionPMT=0;
								pmtComplete=true;
							}
						}
						if(ptr1<1 && m_grabbingPMT==true)
						{
							Array.Copy(m_packetHeader.Payload,0,m_tableBufferPMT,m_bufferPositionPMT,184);
							m_bufferPositionPMT+=184;
							if(m_bufferPositionPMT>=m_grabbingLenPMT)
								pmtComplete=true;

						}
					}
					else if (sectionLen < 183)// in one packet
					{
						pmtComplete=true;
						Array.Copy(m_packetHeader.Payload,1,m_tableBufferPMT,0,sectionLen);
					}
					if(pmtComplete && m_grabbingLenPMT>0)
					{
						lock(m_tableBufferPMT.SyncRoot)
						{
							int version=((m_tableBufferPMT[5]>>1)&0x1F);
							
							if(m_currentPMTVersion!=version)
							{
								Log.Write("DVB Demuxer PMT: new version={0}, old version={1}",version,m_currentPMTVersion);
								if(OnPMTIsChanged!=null)
								{
									byte[] pmtData=new byte[m_grabbingLenPMT];
									Array.Copy(m_tableBufferPMT,0,pmtData,0,m_grabbingLenPMT);
									OnPMTIsChanged(pmtData);
									m_currentPMTVersion=version;
								}
							}
							m_tableBufferPMT=new byte[4096];
							m_bufferPositionPMT=0;
							m_grabbingPMT=false;
						}
					}

				}
				#endregion

				#region sections

				if(m_sectionPid!=-1 && m_packetHeader.Pid==m_sectionPid)
				{
					m_packetHeader.Payload=new byte[184];
					Marshal.Copy((IntPtr)(ptr+4),m_packetHeader.Payload,0,184);
					try
					{

						int offset=0;
						//
						// calc offset & pointers
						if(m_packetHeader.PayloadUnitStart==true)
							offset=1;
						if(m_packetHeader.PayloadUnitStart==true && m_bufferPositionSec==0)
							offset=m_packetHeader.AdaptionField+1;
						
						//
						// start copy data for every section on its table-id-byte
						if(m_bufferPositionSec==0 && m_sectionTableID!=m_packetHeader.Payload[offset])
							offset=184;
						//
						// copy data
						DVBSectionHeader header=GetHeader();
						if(m_bufferPositionSec+(184-offset)<=SECTIONS_BUFFER_WIDTH)
						{
							Array.Copy(m_packetHeader.Payload,offset,m_tableBufferSec,m_bufferPositionSec,184-offset);
							m_bufferPositionSec+=(184-offset);
						}
						else 
						{
							GetAllSections();
							m_bufferPositionSec=0;
						}
					}
					catch(Exception ex)
					{
						Log.Write("mhw-epg: exception {0} source:{1}",ex.Message,ex.StackTrace);
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
					if(ptr<65533)
					{
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
				m_tableBufferD2=new byte[65535];
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
					if(ptr<65533)
					{
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
				m_tableBufferD3=new byte[65535];
				m_bufferPositionD3=0;
			}

		}

		void ParseSection()
		{

			lock(m_tableBufferSec.SyncRoot)
			{
				int ptr=0;
				byte[] data=new byte[0];
				DVBSectionHeader header=new DVBSectionHeader();

				header=GetHeader();
				header.SectionLength+=3;
				bool grabEitSchedule=false;
				bool sectionOK=false;
				// table ok?
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

				if(m_sectionPid==0x12 && (m_sectionTableID>=0x50 && m_sectionTableID<=0x6F))
				{
					grabEitSchedule=true;
				}						
				if(grabEitSchedule==true) // check section-number and service-id
				{
					if(SectionIsInBuffer(header.SectionNumber,header.TableIDExtension)==false)
					{
						//Log.Write("added section-number: {0} segment last: {1} last_section: {2} section len:{3}",header.SectionNumber,header.HeaderExtB12,header.LastSectionNumber,header.SectionLength);
						m_tableSections.Add(data);
						m_secTimer.Stop();
						m_secTimer.Interval=6000;
						m_secTimer.Start();
						// timeout only after grabbing last section
					}
				}
				else 
				{
					if(SectionIsInBuffer(header.SectionNumber)==false)
					{
						//Log.Write("added section-number: {0} segment last: {1} last_section: {2} section len:{3}",header.SectionNumber,header.HeaderExtB12,header.LastSectionNumber,header.SectionLength);
						m_tableSections.Add(data);
						m_secTimer.Stop();
						m_secTimer.Interval=6000;
						m_secTimer.Start();
					}
					// timeout only after grabbing last section
				}
				// check if table is complete
				if((grabEitSchedule==false && m_tableSections.Count-1==header.LastSectionNumber)/* ||
					(grabEitSchedule==true && m_tableSections.Count==256)*/)
				{
					// ready, clean up
					m_secTimer.Stop();
					ClearGrabber();
				}
				else
				{
					m_bufferPositionSec=0;// grab next
				}
			
				if (grabEitSchedule==false)
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
		}// parsesection
		#endregion

		private void m_secTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			m_secTimer.Stop();
			int tableID=m_sectionTableID;
			ClearGrabber();// clear up
		}
		//
		//
		public static void ProcessEPG()
		{
			if(m_currentDVBCard==(int)DVBEPG.EPGCard.Invalid || m_currentDVBCard==(int)DVBEPG.EPGCard.Unknown)
				return ;
			if(m_tableSections==null)
				return;
			if(m_tableSections.Count==0)
				return;
			ArrayList dataList=(ArrayList)m_tableSections.Clone();
			m_tableSections.Clear();
			
			DVBEPG		tmpEPGClass=new DVBEPG(m_currentDVBCard,m_currentNetworkType);
			Log.Write("started thread {0}",System.Threading.Thread.CurrentThread.Name);
			int count=tmpEPGClass.GetEPG(dataList,0, out epgRegrabTime);
			Log.Write("epg ready. added {0} events to database! Next grab at{1}",count, epgRegrabTime.ToString() );
		}
	}//class dvbdemuxer
 
}//namespace
