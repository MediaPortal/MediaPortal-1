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
#pragma warning(disable: 4786)
#include <streams.h>
#include <bdatypes.h>
#include <time.h>
#include ".\atscparser.h"

extern void Log(const char *fmt, ...) ;
extern void Dump(const char *fmt, ...) ;
ATSCParser::ATSCParser(IPin* pPin)
{
	m_pPin=pPin;
	m_demuxer=NULL;
	masterGuideTableDecoded=false;
}

ATSCParser::~ATSCParser(void)
{
}
void ATSCParser::Reset()
{	
	try
	{
		Log("ATSC:reset");
		CAutoLock lock(&m_Lock);
		m_epgTimeout=time(NULL);
		m_mapEvents.clear();
		m_mapEtm.clear();
		masterGuideTableDecoded=false;
		if (m_demuxer!=NULL) 
		{
			m_demuxer->SetSectionMapping(m_pPin);
		}
	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::Reset()");
	}
}

void ATSCParser::SetDemuxer(SplitterSetup* demuxer)
{
	m_demuxer=demuxer;
}

void ATSCParser::ATSCDecodeMasterGuideTable(byte* buf, int len,int* channelsFound)
{	
	try
	{
		int table_id = buf[0];
		if (table_id!=0xc7) return;
		CAutoLock lock(&m_Lock);

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

		if (masterGuideTableDecoded) 
		{
			if (section_length==mgSectionLength &&
				section_number==mgSectionNumber &&
				last_section_number==mgLastSectionNumber)
			{
				return ;
			}
		}
				
		Log("ATSCDecodeMasterGuideTable()");
		if (m_demuxer!=NULL) m_demuxer->SetSectionMapping(m_pPin);
		mgLastSectionNumber=last_section_number;
		mgSectionNumber=section_number;
		mgSectionLength=section_length;
		masterGuideTableDecoded=true;
		*channelsFound=0;

		//decode tables...
		int start=11;
		// 16------ -------- 3--13--- -------- 3--5---- 32------ -------- -------- -------- 4---12-- --------
		// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
		//    0        1        2         3        4       5       6         7        8       9        10
		for (int i=0; i < tables_defined; ++i)
		{
			//table type		description
			//0x0000 			Terrestrial VCT with current_next_indicator=1
			//0x0001 			Terrestrial VCT with current_next_indicator=0
			//0x0002 			Cable VCT with current_next_indicator=1
			//0x0003 			Cable VCT with current_next_indicator=0
			//0x0004 			Channel ETT
			//0x0005 			DCCSCT
			//0x0006-0x00FF 	[Reserved for future ATSC use]
			//0x0100-0x017F 	EIT-0 to EIT-127
			//0x0180-0x01FF 	[Reserved for future ATSC use]
			//0x0200-0x027F 	Event ETT-0 to event ETT-127
			//0x0280-0x0300 	[Reserved for future ATSC use]
			//0x0301-0x03FF 	RRT with rating_region 1-255
			//0x0400-0x0FFF 	[User private]
			//0x1000-0x13FF 	[Reserved for future ATSC use]
			//0x1400-0x14FF 	DCCT with dcc_id 0x00 – 0xFF
			//0x1500-0xFFFF 	[Reserved for future ATSC use]
			int table_type				=  (buf[start]<<8) + (buf[start+1]);
			int table_type_PID			= ((buf[start+2]&0x1f)<<8) + (buf[start+3]);
			int table_type_version		=   buf[start+4] & 0x1f;
			int number_of_bytes			=  (buf[start+5]<<24) + (buf[start+6]<<16) + (buf[start+7]<<8)+ buf[start+8];
			int table_type_descriptors_len = ((buf[start+9]&0xf)<<8) + buf[start+10];
			int pos=0;
			int ofs=start+11;

			if (m_demuxer!=NULL) 
			{
				//if (table_type >=0x100 && table_type  <= 0x17f)			//EIT
				//	m_demuxer->MapAdditionalPID(m_pPin,table_type_PID);
				//else if (table_type >=0x200 && table_type  <= 0x27F)	//ETT
				//	m_demuxer->MapAdditionalPID(m_pPin,table_type_PID);
				//else if (table_type >=0x0301 && table_type  <= 0x03FF)	//RTT
				//	m_demuxer->MapAdditionalPID(m_pPin,table_type_PID);
				//else if (table_type == 0x0004)							//channel ETT
				//	m_demuxer->MapAdditionalPID(m_pPin,table_type_PID);
			}
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

	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::ATSCDecodeMasterGuideTable()");
	}
}

void ATSCParser::ATSCDecodeEIT(byte* buf, int len)
{
	try
	{
		// -------- +-++---- -------- ++++++++ ++++++++ --+++++- -------- ++++++++ -------- ++++++++
		// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
		//    0        1        2         3        4       5       6         7        8       9        10
		int table_id = buf[0];
		if (table_id!=0xcb) return;
		if (len < 10) return;
		CAutoLock lock(&m_Lock);

		int section_syntax_indicator = (buf[1]>>7) & 1;
		int private_indicator = (buf[1]>>6) & 1;
		int section_length = ((buf[1]& 0xF)<<8) + buf[2];
		int source_id=(buf[3]<<8)+buf[4];
		int version=(buf[5]>>1)&0x1f;
		int current_next_indicator=buf[5]&0x1;
		int section_number=buf[6];
		int last_section_number=buf[7];
		int protocol_version=buf[8];
		int num_events=buf[9];
		int off=10;
		for (int i=0; i < num_events;++i)
		{
			if (off+10 >=len) return;
			// ++------ --------  +++++++ ++++++++ ++++++++ ++++++++ --++---- -------- -------- ++++++++ ----    
			// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
			//    0        1        2         3        4       5       6         7        8       9        10
			int event_id=((buf[off] &0x3f)<<8) + buf[off+1];
			ULONG start_time = (buf[off+2]<<24) +(buf[off+3]<<16)+(buf[off+4]<<8)+(buf[off+5]);
			int ETM_location = (buf[off+6]>>4)&0x3;
			int length_in_secs=((buf[off+6]&0xf)<<16) + (buf[off+7]<<8) + buf[off+8];
			int title_len=buf[off+9];
			char title[4096];
			off+=10;
			char *pTitle=DecodeMultipleStrings(buf,off,len);
			strcpy(title, pTitle);
			off+=title_len;
			int descriptor_len = ((buf[off]&0xf)<<8) + buf[off+1];
			off+=2;
			int lenDesc=0;
			while (lenDesc < descriptor_len)
			{
				int descriptor_tag = buf[off+lenDesc];
				int descriptor_len = buf[off+lenDesc+1];
				lenDesc += (2+descriptor_len);
			}
			off +=descriptor_len;
				
			ULONG key=(source_id<<16)+event_id;
			imapEvents it=m_mapEvents.find(key);
			if (it==m_mapEvents.end())
			{
				ATSCEvent newEvent;
				newEvent.ETM_location=ETM_location;
				newEvent.source_id = source_id;
				newEvent.event_id = event_id;
				newEvent.start_time=start_time;
				newEvent.length_in_secs=length_in_secs;
				newEvent.title=title;

				m_mapEvents[key] = newEvent;
				Log("ATSC:EIT: chan:%d event:%x start:%x duration:%x etm:%x title:'%s'",
					source_id,event_id,start_time,length_in_secs,ETM_location,title);
				m_epgTimeout=time(NULL);
			}
		}

	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::ATSCDecodeEIT()");
	}
}
void ATSCParser::ATSCDecodeETT(byte* buf, int len)
{
	try
	{
		// -------- +-++---- -------- ++++++++ ++++++++ --+++++- ++++++++ -------- ++++++++  
		// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
		//    0        1        2         3        4       5       6         7        8       9        10
		int table_id = buf[0];
		if (table_id!=0xcc) return;
		if (len < 12) return;
		CAutoLock lock(&m_Lock);

		int section_syntax_indicator = (buf[1]>>7) & 1;
		int private_indicator = (buf[1]>>6) & 1;
		int section_length = ((buf[1]& 0xF)<<8) + buf[2];
		int EIT_table_id_extension=(buf[3]<<8)+(buf[4]);
		int version=(buf[5]>>1)&0x1f;
		int current_next_indicator=buf[5]&0x1;
		int section_number=buf[6];
		int last_section_number=buf[7];
		int protocol_version=buf[8];
		ULONG ETM_id=(buf[9]<<24)+(buf[10]<<16)+(buf[11]<<8)+(buf[12]);
		char *pDescription=DecodeMultipleStrings(buf,13,len);

		imapEtm it=m_mapEtm.find(ETM_id);
		if (it==m_mapEtm.end())
		{
			//TODO:store description
			ETMEvent newEvent;
			newEvent.description=pDescription;
			newEvent.ETM_id=ETM_id;
			int source_id=(ETM_id>>16);
			int event_id=((ETM_id &0xffff) >>2);
			int type=ETM_id&3;
			newEvent.source_id=source_id;
			newEvent.event_id=event_id;
			newEvent.type=type;
			m_mapEtm[ETM_id]=newEvent;
			Log("ATSC:ETT: chan:%d event:%x type:%x description:'%s'",source_id,event_id,type,pDescription);
			m_epgTimeout=time(NULL);
		}
	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::ATSCDecodeETT()");
	}
}
void ATSCParser::ATSCDecodeRTT(byte* buf, int len)
{
	Log("ATSCDecodeRTT() %x",buf[0]);
}
void ATSCParser::ATSCDecodeChannelEIT(byte* buf, int len)
{
	Log("ATSCDecodeChannelEIT() %x",buf[0]);
}
void ATSCParser::ATSCDecodeEPG(byte* buf, int len)
{
  return;
	try
	{
		CAutoLock lock(&m_Lock);

		int table_id = buf[0];
		if (table_id ==0xcb)
		{
			//Decode EIT-0 - EIT-127
			ATSCDecodeEIT(buf,len);
		}
		else if (table_id ==0xcc)
		{
			//Decode ETT-0 - ETT-127
			ATSCDecodeETT(buf,len);
		}
		else if (table_id ==0xca)
		{
			//Decode RTT with region 1-255
			ATSCDecodeRTT(buf,len);
		}

	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::ATSCDecodeEPG()");
	}
}
void ATSCParser::ATSCDecodeChannelTable(BYTE *buf,ChannelInfo *ch, int* channelsFound, int maxLen)
{
	try
	{
		CAutoLock lock(&m_Lock);

		int table_id = buf[0];
		if (table_id!=0xc8 && table_id != 0xc9) return;
		//dump table!
		*channelsFound=0;
		Log("ATSCDecodeChannelTable()");
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
		if (num_channels_in_section <= 0) return;
	/*
		FILE* fp = fopen("table.dat","wb+");
		if (fp!=NULL)
		{
			fwrite(buf,1,section_length,fp);
			fclose(fp);
		}*/
		Log("  table id:0x%x section length:%d channels:%d (%d)", table_id,section_length,num_channels_in_section, (*channelsFound));
		int start=10;
		for (int i=0; i < num_channels_in_section;i++)
		{
			Log("  decode channel:%d", i);
			char shortName[127];
			strcpy(shortName,"unknown");
			try
			{
				//shortname 7*16 bits (14 bytes) in UTF-16
				for (int count=0; count < 7; count++)
				{
					shortName[count] = buf[1+start+count*2];
					shortName[count+1]=0; 
				}
			}
			catch(...)
			{
			}
			
			Log("  channel:%d shortname:%s", i,shortName);

			start+= 7*2;
			// 4---10-- ------10 -------- 8------- 32------ -------- -------- -------- 16------ -------- 16------ -------- 2-111113 --6----- 16------ -------- 6-----10 --------
			// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
			//    112      113      114       115      116    117      118       119     120     121       123      124      125      126      127      128      129      130
			//     0        1        2         3        4      5        6         7       8       9        10       11       12       13       14       15       16       17 
			//  ++++++++ ++++++++ --+-++-	
			// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
			int major_channel    		 =((buf[start  ]&0xf)<<8) + (buf[start+1]>>2);
			int minor_channel    		 =((buf[start+1]&0x3)<<8) + buf[start+2];
			int modulation_mode  		 = buf[start+3];
			int carrier_frequency		 = (buf[start+4]<<24) + (buf[start+5]<<16) + (buf[start+6]<<8) + (buf[start+7]);
			int channel_TSID			 = ((buf[start+8])<<8) + buf[start+9];
			int program_number			 = ((buf[start+10])<<8) + buf[start+11];
			int ETM_location			 = ((buf[start+12]>>6)&0x3);
			int access_controlled		 = ((buf[start+12]>>4)&0x1);
			int hidden          		 = ((buf[start+12]>>3)&0x1);
			int path_select     		 = ((buf[start+12]>>2)&0x1);
			int out_of_band     		 = ((buf[start+12]>>1)&0x1);
			int hide_guide     		     = ((buf[start+12]   )&0x1);
			int service_type             = ((buf[start+13]   )&0x3f);
			int source_id				 = ((buf[start+14])<<8) + buf[start+15];
			int descriptors_length		 = ((buf[start+16]&0x3)<<8) + buf[start+17];

			if (major_channel==0 && minor_channel==0 && channel_TSID==0 && service_type==0 )
			{
				*channelsFound=0;
				return;
			}
			if (modulation_mode < 0 || modulation_mode > 5)
			{
				*channelsFound=0;
				return;
			}
			Log("  channel:%d major:%d minor:%d modulation:%d frequency:%d tsid:%d program:%d servicetype:%d descriptor len:%d", 
							i,major_channel,minor_channel,modulation_mode,carrier_frequency, channel_TSID, program_number,service_type, descriptors_length);
			ChannelInfo* channelInfo = &ch[*channelsFound];
			memset(channelInfo->ProviderName,0,255);
			memset(channelInfo->ServiceName,0,255);
			strcpy((char*)channelInfo->ProviderName,"unknown");
			strcpy((char*)channelInfo->ServiceName,shortName);
			channelInfo->MinorChannel = minor_channel;
			channelInfo->MajorChannel = major_channel;
			channelInfo->Frequency    = carrier_frequency;
			channelInfo->ProgrammNumber= program_number;
			channelInfo->TransportStreamID = channel_TSID;		
			channelInfo->Pids.Teletext=-1;
			channelInfo->Pids.AudioPid1=-1;
			channelInfo->Pids.AudioPid2=-1;
			channelInfo->Pids.AudioPid3=-1;
			channelInfo->Pids.AC3=-1;
			channelInfo->Pids.Subtitles=-1;
			channelInfo->Pids.VideoPid=-1;
			channelInfo->Pids.Lang1_1=0;
			channelInfo->Pids.Lang1_2=0;
			channelInfo->Pids.Lang1_3=0;
			channelInfo->Pids.Lang2_1=0;
			channelInfo->Pids.Lang2_2=0;
			channelInfo->Pids.Lang2_3=0;
			channelInfo->Pids.Lang3_1=0;
			channelInfo->Pids.Lang3_2=0;
			channelInfo->Pids.Lang3_3=0;
			channelInfo->EITPreFollow=0;
			channelInfo->EITSchedule=0;
			channelInfo->ProgrammPMTPID=-1;
			channelInfo->NetworkID    =-1;
			channelInfo->PMTReady	  = 1;
			channelInfo->SDTReady	  = 1;
			if (service_type==1||service_type==2) channelInfo->ServiceType=1;//ATSC video
			if (service_type==3) channelInfo->ServiceType=2;//ATSC audio
			switch (modulation_mode)
			{
				case 0: //reserved
					channelInfo->Modulation   = BDA_MOD_NOT_SET;
				break;
				case 1: //analog
					channelInfo->Modulation   = BDA_MOD_ANALOG_FREQUENCY;
				break;
				case 2: //QAM64
					channelInfo->Modulation   = BDA_MOD_64QAM;
				break;
				case 3: //QAM256
					channelInfo->Modulation   = BDA_MOD_256QAM;
				break;
				case 4: //8 VSB
					channelInfo->Modulation   = BDA_MOD_8VSB;
				break;
				case 5: //16 VSB
					channelInfo->Modulation   = BDA_MOD_16VSB;
				break;
				default: //
					channelInfo->Modulation   = BDA_MOD_NOT_SET;
				break;

			}

			start += 18;
			int len=0;
			while (len < descriptors_length && descriptors_length>0)
			{
				int descriptor_tag = buf[start+len];
				int descriptor_len = buf[start+len+1];
				if (descriptor_len==0 || descriptor_len+start > section_length)
				{
					*channelsFound=0;
					return;
				}			
				Log("    decode descriptor start:%d len:%d tag:%x", start, descriptor_len, descriptor_tag);
				switch (descriptor_tag)
				{
					case 0xa1:
						DecodeServiceLocationDescriptor( buf,start+len, channelInfo);
					break;
					case 0xa0:
						DecodeExtendedChannelNameDescriptor( buf,start+len,channelInfo, maxLen);
					break;
				}
				len += (descriptor_len+2);
			}
			start += descriptors_length;
			*channelsFound=*channelsFound+1;
		}
		Log("ATSCDecodeChannelTable() done, found %d channels", (*channelsFound));
	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::ATSCDecodeChannelTable()");
	}
}

void ATSCParser::DecodeServiceLocationDescriptor( byte* buf,int start,ChannelInfo* channelInfo)
{
	try
	{
		CAutoLock lock(&m_Lock);

		Log("DecodeServiceLocationDescriptor()");
		//  8------ 8------- 3--13--- -------- 8-------       
		// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
		//    0        1        2         3        4       5       6         7        8       9     
		int pcr_pid = ((buf[start+2]&0x1f)<<8) + buf[start+3];
		int number_of_elements = buf[start+4];
		int off=start+5;
		channelInfo->PCRPid=pcr_pid;

		if (number_of_elements==0) return;
		Log(" pcr pid:%x elements:%d", pcr_pid, number_of_elements);
		for (int i=0; i < number_of_elements;++i)
		{

			//  8------ 3--13--- -------- 24------ -------- --------
			// 76543210|76543210|76543210|76543210|76543210|76543210|
			//    0        1        2         3        4       5     
			int streamtype			  = buf[off];
			int elementary_pid		  = ((buf[off+1]&0x1f)<<8) + buf[off+2];
			int ISO_639_language_code =	(buf[off+3]<<16) +(buf[off+4]<<8) + (buf[off+5]);

			Log(" element:%d type:%d pid:%x", i,streamtype, elementary_pid);
			off+=6;
			//pmtData.data=ISO_639_language_code;
			switch (streamtype)
			{
				case 0x2: // video
					channelInfo->Pids.VideoPid=elementary_pid;
					break;
				case 0x81: // audio
					channelInfo->Pids.AC3=elementary_pid;
					break;
				default:
					break;
			}
		}
		Log("DecodeServiceLocationDescriptor() done");
	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::DecodeServiceLocationDescriptor()");
	}
}
void ATSCParser::DecodeExtendedChannelNameDescriptor( byte* buf,int start,ChannelInfo* channelInfo, int maxLen)
{
	try
	{
		CAutoLock lock(&m_Lock);

		Log("DecodeExtendedChannelNameDescriptor() ");
		// tid   
		//  8       8------- 8-------
		// 76543210|76543210|76543210
		//    0        1        2    
		int descriptor_tag = buf[start+0];
		int descriptor_len = buf[start+1];
		Log(" tag:%x len:%d", descriptor_tag, descriptor_len);
		char* label = DecodeMultipleStrings(buf,start+2,maxLen);
		if (label==NULL) return ;
		strcpy((char*)channelInfo->ServiceName,label);

		Log(" label:%s", label);
		delete [] label;
		Log("DecodeExtendedChannelNameDescriptor() done");
	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::DecodeExtendedChannelNameDescriptor()");
	}
}
char* ATSCParser::DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes)
{
	try
	{
		//Log("DecodeString() compression type:%d numberofbytes:%d",compression_type, mode);
		if (compression_type==0 && mode==0)
		{
			char* label = new char[number_of_bytes+1];
			memcpy(label,&buf[offset],number_of_bytes);
			label[number_of_bytes]=0;
			return (char*)label;
		}
		//string data="";
		//for (int i=0; i < number_of_bytes;++i)
		//	data += String.Format(" {0:X}", buf[offset+i]);
		Log("DecodeString() unknown type or mode");

	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::DecodeString()");
	}
	return NULL;
}

char* ATSCParser::DecodeMultipleStrings(byte* buf, int offset, int maxLen)
{
	try
	{
		int number_of_strings = buf[offset];
		//Log("DecodeMultipleStrings() number_of_strings:%d",number_of_strings);
		

		for (int i=0; i < number_of_strings;++i)
		{
			//Log("  string:%d", i);
			if (offset+4>=maxLen)
			{
				return "";
			}
			int ISO_639_language_code = (buf[offset+1]<<16)+(buf[offset+2]<<8)+(buf[offset+3]);
			int number_of_segments=buf[offset+4];
			int start=offset+5;
			//Log("  segments:%d", number_of_segments);
			for (int k=0; k < number_of_segments;++k)
			{
				if (start+2>=maxLen)
				{
					return "";
				}
				//Log("  decode segment:%d", k);
				int compression_type = buf[start];
				int mode             = buf[start+1];
				int number_bytes     = buf[start+2];
				//decode text....
				char *label=DecodeString(buf, start+3, compression_type,mode,number_bytes);
				start += (number_bytes+3);
				if (label!=NULL) return label;
			}
		}
	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::DecodeMultipleStrings()");
	}
	return NULL;
}


bool ATSCParser::IsReady()
{
	try
	{
		int secondsPassed= time(NULL)-m_epgTimeout;
		if (secondsPassed >=30)
		{
			return true;
		}

	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::IsReady()");
	}
	return false;
}
int ATSCParser::GetEPGCount()
{
	try
	{
		return m_mapEvents.size();
	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::GetEPGCount()");
	}
	return 0;
}
void ATSCParser::GetEPGTitle(WORD no, WORD* source_id, ULONG* starttime, WORD* length_in_secs, char** title, char** description)
{
	try
	{
		CAutoLock lock(&m_Lock);

		if (no >= m_mapEvents.size()) return;
		imapEvents it=m_mapEvents.begin();
		int count=0;
		while (count < no) { ++it; ++count;}
		*source_id=it->second.source_id;
		*starttime=it->second.start_time;
		*length_in_secs=(it->second.length_in_secs/60);
		*title=(char*)it->second.title.c_str();
		int event_id=it->second.event_id;
		*description="";
		if (it->second.ETM_location!=0)
		{
			ETMEvent etmEvent;
			imapEtm itEtm=m_mapEtm.begin();
			while (itEtm != m_mapEtm.end())
			{
				if (itEtm->second.source_id==(*source_id) &&
					itEtm->second.event_id ==event_id)
				{
					*description=(char*)itEtm->second.description.c_str();
					return;
				}
				++itEtm;
			}
		}

	}
	catch(...)
	{
		Dump("unhandled exception in ATSCParser::GetEPGTitle()");
	}
}
