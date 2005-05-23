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

namespace MediaPortal.TV.Recording
{
    public class DVBDemuxer : ISampleGrabberCB
    {
        
        #region Global Arrays
		int[,,] AudioBitrates = new int[,,]{{
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
	
	int[,] AudioFrequencies = new int[,]{
		{ 22050,24000,16000,0 },	
		{ 44100,48000,32000,0 },	
		{ 11025,12000,8000,0 }		
	};
	
	double[] AudioTimes = new double[]{ 0.0,103680000.0,103680000.0,34560000.0 };
	 
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

        #endregion

        #region Contructor/Destructor
        public DVBDemuxer()
		{
            using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(System.Windows.Forms.Application.StartupPath+@"\MediaPortal.xml"))
            {
                m_pluginsEnabled = xmlreader.GetValueAsBool("dvb_ts_cards", "enablePlugins", false);
            }
        }
        ~DVBDemuxer()
        {
        }
        #endregion

        #region global Vars
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
		// pmt
		int m_currentPMTVersion=0;
		// card
		int m_currentDVBCard=0;
		// for pmt pid
		int m_grabbingLenPMT=0;
		bool m_grabbingPMT=false;
		byte[] m_tableBufferPMT=new byte[4096];
		int m_bufferPositionPMT=0;


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
		public delegate bool OnAudioChanged(AudioHeader audioFormat);
        public event OnAudioChanged OnAudioFormatChanged; 
		public delegate void OnPMTChanged(byte[] pmtTable);
		public event OnPMTChanged OnPMTIsChanged;
	    #endregion

        #region public functions

		public void SetChannelData(int audio, int video, int teletext, int subtitle, string channelName,int pmtPid)
		{
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

        #region Properties

        public DVBTeletext Teletext
        {
            get { return m_teleText; }
        }
        public int CardType
        {
            set 
			{
				m_currentDVBCard=value;
				m_epgClass=new DVBEPG(value);
			}
        }

        #endregion

		#region functions
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
				if(GUIGraphicsContext.DX9Device!=null)// only grab from epg-grabber
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

		#endregion
    }//class dvbdemuxer
 
}//namespace
