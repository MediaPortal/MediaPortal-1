using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for ATSCSections.
	/// </summary>
	public class ATSCSections
	{


		bool								m_syncWait=false;
		ArrayList						m_sectionsList;	
		static DVBDemuxer		m_streamDemuxer;
		System.Timers.Timer	m_eitTimeoutTimer=null;
		bool								m_breakAction=false;
		int									m_timeoutMS=1000; // the timeout in milliseconds

		public ATSCSections(DVBDemuxer demuxer)
		{
			m_streamDemuxer=demuxer;
			m_eitTimeoutTimer=new System.Timers.Timer(3000);
			m_eitTimeoutTimer.Elapsed+=new System.Timers.ElapsedEventHandler(m_eitTimeoutTimer_Elapsed);

		}
		
		//
		public int Timeout
		{
			get{return m_timeoutMS;}
			set{m_timeoutMS=value;}
		}

		public DVBSections.Transponder Scan(DShowNET.IBaseFilter filter)
		{
			Log.Write("ATSC-scan:");
			m_sectionsList=new ArrayList();	
			DVBSections.Transponder transponder = new DVBSections.Transponder();

			transponder.channels = new ArrayList();
			transponder.PMTTable = new ArrayList();

			//get Master Guide table (pid=0x1FFB, table id 0xc7)
			//GetStreamData(filter,0x1ffb, 0xc7,0,Timeout);
			//if (m_sectionsList.Count==0) return transponder;
			//foreach(byte[] arr in m_sectionsList)
			//	DecodeMasterGuideTable(arr);

			//get Terrestial Virtual Channel Table (pid=0x1FFB, table id 0xc8)
			
			Log.Write("ATSC-scan: get Terrestial Virtual Channel Table");
			GetStreamData(filter,0x1ffb, 0xc8,0,Timeout);
			foreach(byte[] arr in m_sectionsList)
				DecodeTerrestialVirtualChannelTable(transponder,arr);

			
			//get Cable Virtual Channel Table (pid=0x1FFB, table id 0xc9)
			//Log.Write("ATSC-scan: get Cable Virtual Channel Table");
			//GetStreamData(filter,0x1ffb, 0xc9,0,Timeout);
			//foreach(byte[] arr in m_sectionsList)
			//	DecodeCableVirtualChannelTable(transponder,arr);
			
			return transponder;
		}

		void DecodeServiceLocationDescriptor( byte[] buf,int start,ref DVBSections.ChannelInfo channelInfo)
		{

			//  8------ 8------- 3--13--- -------- 8-------       
			// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
			//    0        1        2         3        4       5       6         7        8       9     
			int pcr_pid = ((buf[start+2]&0x1f)<<8) + buf[start+3];
			int number_of_elements = buf[start+4];
			int off=start+5;
			Log.Write("DecodeServiceLocationDescriptor() pcr pid:{0} elements:{1}", pcr_pid,number_of_elements);
			channelInfo.pcr_pid=pcr_pid;
			for (int i=0; i < number_of_elements;++i)
			{
				//  8------ 3--13--- -------- 24------ -------- --------
				// 76543210|76543210|76543210|76543210|76543210|76543210|
				//    0        1        2         3        4       5     
				int streamtype						= buf[off];
				int elementary_pid				= ((buf[off+1]&0x1f)<<8) + buf[off+2];
				int ISO_639_language_code =	(buf[off+3]<<16) +(buf[off+4]<<8) + (buf[off+5]);
				Log.Write("  element:{0} streamtype:{1} pid:{2}", i,streamtype,elementary_pid);
				off+=6;
				DVBSections.PMTData pmtData = new DVBSections.PMTData();
				pmtData.elementary_PID=elementary_pid;
				//pmtData.data=ISO_639_language_code;
				switch (streamtype)
				{
					case 0x2: // video
						pmtData.isVideo=true;
					break;
					case 0x81: // audio
						pmtData.isAudio=true;
					break;
				}
				channelInfo.pid_list.Add(pmtData);
			}
		}

		string DecodeString(byte[] buf, int offset, int compression_type, int mode, int number_of_bytes)
		{
			return String.Empty;
		}
		string[] DecodeMultipleStrings(byte[] buf, int offset)
		{
			int number_of_strings = buf[offset];
			
			string[] labels = new string[number_of_strings];
		
			for (int i=0; i < number_of_strings;++i)
			{
				int ISO_639_language_code = (buf[offset+1]<<16)+(buf[offset+2]<<8)+(buf[offset+3]);
				int number_of_segments=buf[offset+4];
				int start=offset+5;
				labels[i]=String.Empty;
				for (int k=0; k < number_of_segments;++k)
				{
					int compression_type = buf[start];
					int mode             = buf[start+1];
					int number_bytes     = buf[start+2];
					//decode text....
					labels[i]+=DecodeString(buf, start+3, compression_type,mode,number_bytes);
					start += (number_bytes+3);
				}
			}
			return labels;
		}

		void DecodeExtendedChannelNameDescriptor( byte[] buf,ref DVBSections.ChannelInfo channelInfo)
		{
			// tid   
			//  8       8------- 8-------
			// 76543210|76543210|76543210
			//    0        1        2    

			int descriptor_tag = buf[0];
			int descriptor_len = buf[1];
			string[] labels = DecodeMultipleStrings(buf,2);
			if (labels.Length==0) return ;
			if (labels[0].Length==0) return;
			channelInfo.service_name=labels[0];
		}

		void DecodeTerrestialVirtualChannelTable(DVBSections.Transponder transponder,byte[] buf)
		{

			Log.Write("ATSC-scan: DecodeTerrestialVirtualChannelTable() len={0}",buf.Length);
			if (!System.IO.File.Exists("vct.dat"))
			{
				System.IO.FileStream stream = new System.IO.FileStream("vct.dat",System.IO.FileMode.Create,System.IO.FileAccess.Write,System.IO.FileShare.None);
				stream.Write(buf,0,buf.Length);
				stream.Close();
			}

			if (buf.Length<10) return;
			try
			{

				// tid   
				//  8       112-12-- -------- 16------ -------- 2-5----1 8------- 8------- 8------- 8------- 
				// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
				//    0        1        2         3        4       5       6         7        8       9     
				int table_id = buf[0];
				int section_syntax_indicator = (buf[1]>>7) & 1;
				int private_indicator = (buf[1]>>6) & 1;
				int section_length = ((buf[1]& 0xF)<<8) + buf[2];
				int transport_stream_id = (buf[3]<<8)+buf[4];
				int version_number = ((buf[5]>>1)&0x1F);
				int current_next_indicator = buf[5] & 1;
				int section_number = buf[6];
				int last_section_number = buf[7];
				int protocol_version = buf[8];
				int num_channels_in_section = buf[9];

				Log.Write("atsc: channels:{0}", num_channels_in_section);
				int start=10;
				for (int i=0; i < num_channels_in_section;i++)
				{
					string shortName="unknown";
					Log.Write("  channel:{0}", i);
					try
					{
						//shortname 7*16 bits (14 bytes) in UTF-16
						byte[] bufRev = new byte[14];
						for (int count=0; count < 7; count++)
						{
							bufRev[count*2] = buf[start+count*2+1];
							bufRev[count*2+1] = buf[start+count*2];
						}
						UnicodeEncoding encoding = new UnicodeEncoding();
						shortName=encoding.GetString(bufRev,0,7*2).Trim();
					}
					catch(Exception ex)
					{
						Log.Write("{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
					}

					Log.Write("atsc: chan:{0} name:{1}", i,shortName);
					start+= 7*2;
					// 4---10-- ------10 -------- 8------- 32------ -------- -------- -------- 16------ -------- 16------ -------- 2-111113 --6----- 16------ -------- 6-----10 --------
					// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
					//    112      113      114       115      116    117      118       119     120     121       123      124      125      126      127      128      129      130
					//     0        1        2         3        4      5        6         7       8       9        10       11       12       13       14       15       16       17 
					
					int major_channel    		 =((buf[start  ]&0xf)<<8) + buf[start+1];
					int minor_channel    		 =((buf[start+1]&0xf)<<8) + buf[start+2];
					int modulation_mode  		 = buf[start+3];
					int carrier_frequency		 = (buf[start+4]<<24) + (buf[start+5]<<16) + (buf[start+6]<<8) + (buf[start+7]);
					int channel_TSID         = ((buf[start+8]&0xf)<<8) + buf[start+9];
					int program_number			 = ((buf[start+10]&0xf)<<8) + buf[start+11];
					int ETM_location				 = ((buf[start+12]>>6)&0x3);
					int access_controlled		 = ((buf[start+12]>>4)&0x1);
					int hidden          		 = ((buf[start+12]>>3)&0x1);
					int path_select     		 = ((buf[start+12]>>2)&0x1);
					int out_of_band     		 = ((buf[start+12]>>1)&0x1);
					int hide_guide     		   = ((buf[start+12]   )&0x1);
					int service_type         = ((buf[start+13]   )&0x3f);
					int source_id						 = ((buf[start+14])<<8) + buf[start+15];
					int descriptors_length	 = ((buf[start+16]&0x3)<<8) + buf[start+17];

					
					Log.Write("atsc: chan:{0} minor:{1} major:{2} modulation:{3} freq:{4} tsid:{5} program:{6}", i,
											major_channel,minor_channel,modulation_mode, carrier_frequency,channel_TSID, program_number);
					Log.Write("atsc: chan:{0} etm:{1} access:{2} hidden:{3} path:{4} oub:{5} hide:{6} service:{7} source:{8}",
											i,ETM_location,access_controlled,hidden, path_select, out_of_band,hide_guide, service_type, source_id);
					Log.Write("atsc: chan:{0} description length:{1}", i,descriptors_length);
					DVBSections.ChannelInfo channelInfo = new DVBSections.ChannelInfo();
					channelInfo.minorChannel = minor_channel;
					channelInfo.majorChannel = major_channel;
					channelInfo.modulation   = modulation_mode;
					channelInfo.freq         = carrier_frequency;
					channelInfo.program_number= program_number;
					channelInfo.serviceType   = service_type;
					channelInfo.service_name  =shortName;
					channelInfo.transportStreamID = channel_TSID;
					channelInfo.pid_list = new ArrayList();

					Log.Write("decode descriptors...");
					start += 18;
					int len=0;
					while (len < descriptors_length)
					{
						Log.Write("  len:{0} / {1}", len,descriptors_length);
						int descriptor_tag = buf[start+len];
						int descriptor_len = buf[start+len+1];
						Log.Write("  descriptor:{0:X} len:{1} {2}", descriptor_tag, descriptor_len, start);
						switch (descriptor_tag)
						{
							case 0xa1:
								DecodeServiceLocationDescriptor( buf,start+len,ref channelInfo);
							break;
							case 0xa0:
								//DecodeExtendedChannelNameDescriptor( buf,start+len,ref channelInfo);
							break;
						}
						len += (descriptor_len+2);
					}
					start += descriptors_length;
					transponder.channels.Add(channelInfo);
				}
				//todo decode additional descriptors
			}
			catch(Exception ex)
			{
				Log.Write("{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
			}
		}

		void DecodeCableVirtualChannelTable(DVBSections.Transponder transponder,byte[] buf)
		{
			Log.Write("DecodeCableVirtualChannelTable()");
		}

		void DecodeMasterGuideTable(byte[] buf)
		{
			Log.Write("DecodeMasterGuideTable()");
#if DONTUSE
			// tid   
			//  8       112-12-- -------- 16------ -------- 2-5----1 8------- 8------- 8------- 16------ --------
      // 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
			//    0        1        2         3        4       5       6         7        8       9        10
			int table_id = buf[0];
			int section_syntax_indicator = (buf[1]>>7) & 1;
			int private_indicator = (buf[1]>>6) & 1;
			int section_length = ((buf[1]& 0xF)<<8) + buf[2];
			int transport_stream_id = (buf[3]<<8)+buf[4];
			int version_number = ((buf[5]>>1)&0x1F);
			int current_next_indicator = buf[5] & 1;
			int section_number = buf[6];
			int last_section_number = buf[7];
			int protocol_version = buf[8];
			int tables_defined = (buf[9]<<8) + buf[10];

			//decode tables...
			int start=11;
			// 16------ -------- 3--13--- -------- 3--5---- 32------ -------- -------- -------- 4---12-- --------
			// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
			//    0        1        2         3        4       5       6         7        8       9        10
			for (int i=0; i < tables_defined; ++i)
			{
				int table_type								 =  (buf[start]<<8) + (buf[start+1]);
				int table_type_PID						 = ((buf[start+2]&0xf)<<8) + (buf[start+3]);
				int table_type_version				 =   buf[start+4] & 0x1f;
				int number_of_bytes					   =  (buf[start+5]<<24) + (buf[start+6]<<16) + (buf[start+7]<<8)+ buf[start+8];
				int table_type_descriptors_len = ((buf[start+9]&0xf)<<8) + buf[start+10];
				int pos=0;
				int ofs=start+11;
				while (pos < table_type_descriptors_len)
				{
					int descriptor_tag = buf[ofs];
					int descriptor_len = buf[ofs+1];
					switch (descriptor_tag)
					{
						case 0x80: //stuffing
							break;
						case 0x81: //AC3 audio descriptor
							break;
						case 0x86: //caption service descriptor
							break;
						case 0x87: //content advisory descriptor
							break;
						case 0xa0: //extended channel name descriptor
							break;
						case 0xa1: //service location descriptor
							break;
						case 0xa2: //time-shifted service descriptor
							break;
						case 0xa3: //component name descriptor
							break;
						case 0xa8: //DCC departing request descriptor
							break;
						case 0xa9: //DCC arriving request descriptor
							break;
						case 0xaa: //redistribution control descriptor
							break;
					}
					pos += (2+descriptor_len);
					ofs += (2+descriptor_len);
				}
				start= start + 11 + table_type_descriptors_len;
			}
			//todo decoder other descriptors
#endif
		}

		public bool GetStreamData(DShowNET.IBaseFilter filter,int pid, int tid,int tableSection,int timeout)
		{
			m_sectionsList=new ArrayList();
			bool flag=false;

			m_streamDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);
			m_streamDemuxer.GetTable(pid,tid,timeout);
			m_syncWait=false;
			m_eitTimeoutTimer.Interval=timeout*2;
			m_eitTimeoutTimer.Start();
			while(m_syncWait==false)
			{
				System.Windows.Forms.Application.DoEvents();
			}
			m_eitTimeoutTimer.Stop();
			m_streamDemuxer.OnGotTable-=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);
			return flag;	

		}

		private void m_eitTimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Log.Write("timeout...");
			m_breakAction=true;
			m_syncWait=true;
		}		

		private void m_streamDemuxer_OnGotTable(int pid, int tableID, ArrayList tableList)
		{
			m_syncWait=true;
			if (pid==0x1ffb)
				Log.Write("Got table {0:X} {1:X} count:{2}", pid, tableID,tableList.Count);
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
	}
}
