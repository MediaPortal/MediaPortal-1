/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Agree
 *	http://mediaportal.sourceforge.net
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

#include <streams.h>
#include <bdatypes.h>
#include <time.h>
#include "Section.h"
#include "mpsa.h"
#include "crc.h"
//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////
void Log(const char *fmt, ...) 
{
#ifdef DEBUG
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("MPSA.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetSystemTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
	}
#endif
};

ULONG GetSectionCRCValue(byte* data,int ptr)
{
	return (ULONG)((data[ptr]<<24)+(data[ptr+1]<<16)+(data[ptr+2]<<8)+data[ptr+3]);
}
ULONG Sections::GetCRC32(BYTE *pData,WORD len)
{
	// returns in crc the dvb-crc32 checksum for sections etc.
	ULONG crc = 0xffffffff;
	for (ULONG i=0;i<len;i++) 
	{
		crc = (crc << 8) ^ CRC32Data[((crc >> 24) ^ pData[i]) & 0xff];
	}
	return crc;
}
Sections::Sections()
{
	m_bParseEPG=true;
	m_bEpgDone=false;
	m_epgTimeout=time(NULL)+60;}

Sections::~Sections()
{
	//delete m_pFileReader;
}

HRESULT Sections::ParseFromFile()
{
	return S_OK;
}
HRESULT Sections::CheckStream(void)
{
	return S_OK;
}
WORD Sections::CISize()
{
	return sizeof(struct chInfo);
}
HRESULT Sections::GetTSHeader(BYTE *data,TSHeader *header)
{
	header->SyncByte=data[0];
	header->TransportError=(data[1] & 0x80)>0?true:false;
	header->PayloadUnitStart=(data[1] & 0x40)>0?true:false;
	header->TransportPriority=(data[1] & 0x20)>0?true:false;
	header->Pid=((data[1] & 0x1F) <<8)+data[2];
	header->TScrambling=data[3] & 0xC0;
	header->AdaptionControl=(data[3]>>4) & 0x3;
	header->ContinuityCounter=data[3] & 0x0F;
	return S_OK;
}
HRESULT Sections::GetPESHeader(BYTE *data,PESHeader *header)
{
	header->Reserved=(data[0] & 0xC0)>>6;
	header->ScramblingControl=(data[0] &0x30)>>4;
	header->Priority=(data[0] & 0x08)>>3;
	header->dataAlignmentIndicator=(data[0] & 0x04)>>2;
	header->Copyright=(data[0] & 0x02)>>1;
	header->Original=data[0] & 0x01;
	header->PTSFlags=(data[1] & 0xC0)>>6;
	header->ESCRFlag=(data[1] & 0x20)>>5;
	header->ESRateFlag=(data[1] & 0x10)>>4;
	header->DSMTrickModeFlag=(data[1] & 0x08)>>3;
	header->AdditionalCopyInfoFlag=(data[1] & 0x04)>>2;
	header->PESCRCFlag=(data[1] & 0x02)>>1;
	header->PESExtensionFlag=data[1] & 0x01;
	header->PESHeaderDataLength=data[2];
	return S_OK;
}
void Sections::GetPTS(BYTE *data,ULONGLONG *pts)
{
	*pts= 0xFFFFFFFFL & ( (6&data[0])<<29 | (255&data[1])<<22 | (254&data[2])<<14 | (255&data[3])<<7 | (((254&data[4])>>1)& 0x7F));
}
void Sections::PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime)
{
	PTSTime time;
	ULONG  _90khz = pts/90;
	time.h=(_90khz/(1000*60*60));
	time.m=(_90khz/(1000*60))-(time.h*60);
	time.s=(_90khz/1000)-(time.h*3600)-(time.m*60);
	time.u=_90khz-(time.h*1000*60*60)-(time.m*1000*60)-(time.s*1000);
	*ptsTime=time;
}
HRESULT Sections::CurrentPTS(BYTE *pData,ULONGLONG *ptsValue,int *streamType)
{
	HRESULT hr=S_FALSE;
	*ptsValue=-1;
	TSHeader header;
	PESHeader pes;
	GetTSHeader(pData,&header);
	int offset=4;
	bool found=false;


	if(header.AdaptionControl==1 || header.AdaptionControl==3)
		offset+=pData[4];
	
	if(header.SyncByte==0x47 && pData[offset]==0 && pData[offset+1]==0 && pData[offset+2]==1)
	{
		*streamType=(int)((pData[offset+3]>>5) & 0x07);
		WORD pesLen=(pData[offset+4]<<8)+pData[offset+5];
		GetPESHeader(&pData[offset+6],&pes);
		BYTE pesHeaderLen=pData[offset+8];
		if(header.Pid) // valid header
		{
			if(pes.PTSFlags==0x02)
			{
			// audio pes found
				GetPTS(&pData[offset+9],ptsValue);
				hr=S_OK;
			}
		}	
	}
	return hr;
}
int Sections::decodePMT(BYTE *buf,ChannelInfo *ch, int len)
{
	// pmt should now in the pmtData array

	int table_id = buf[0];
	int section_syntax_indicator = (buf[1]>>7) & 1;
	int section_length = ((buf[1]& 0xF)<<8) + buf[2];
	int program_number = (buf[3]<<8)+buf[4];
	int version_number = ((buf[5]>>1)&0x1F);
	int current_next_indicator = buf[5] & 1;
	int section_number = buf[6];
	int last_section_number = buf[7];
	int pcr_pid=((buf[8]& 0x1F)<<8)+buf[9];
	int program_info_length = ((buf[10] & 0xF)<<8)+buf[11];
	int len2 = program_info_length;
	int pointer = 12;
	int len1 = section_length - pointer;
	int x;
	
	// loop 1
	while (len2 > 0)
	{
		int indicator=buf[pointer];
		x = 0;
		x = buf[pointer + 1] + 2;
		len2 -= x;
		pointer += x;
		len1 -= x;
	}
	// loop 2
	int stream_type=0;
	int elementary_PID=0;
	int ES_info_length=0;
	int audioToSet=0;

	while (len1 > 4)
	{
		stream_type = buf[pointer];
		elementary_PID = ((buf[pointer+1]&0x1F)<<8)+buf[pointer+2];
		ES_info_length = ((buf[pointer+3] & 0xF)<<8)+buf[pointer+4];
		if(stream_type==1 || stream_type==2)
		{
			if(ch->Pids.VideoPid==0)
				ch->Pids.VideoPid=elementary_PID;
		}
		if(stream_type==3 || stream_type==4)
		{
			audioToSet=0;
			if(ch->Pids.AudioPid1==0)
			{
				audioToSet=1;
				ch->Pids.AudioPid1=elementary_PID;
			}
			else
			{
				if(ch->Pids.AudioPid2==0)
				{
					audioToSet=2;
					ch->Pids.AudioPid2=elementary_PID;
				}
				else
				{
					if(ch->Pids.AudioPid3==0)
					{
						audioToSet=3;
						ch->Pids.AudioPid3=elementary_PID;
					}
				}
			}
		}
		ch->PCRPid=pcr_pid;

		if(stream_type==0x81)
		{
			if(ch->Pids.AC3==0)
				ch->Pids.AC3=elementary_PID;
		}
		pointer += 5;
		len1 -= 5;
		len2 = ES_info_length;
		while (len2 > 0)
		{
			x = 0;
			int indicator=buf[pointer];
			x = buf[pointer + 1] + 2;
			if(indicator==0x6A)
				ch->Pids.AC3=elementary_PID;
			if(indicator==0x0A)
			{
				BYTE d[3];
				d[0]=buf[pointer+2];
				d[1]=buf[pointer+3];
				d[2]=buf[pointer+4];
				if(audioToSet==1)
				{
					ch->Pids.Lang1_1=d[0];
					ch->Pids.Lang1_2=d[1];
					ch->Pids.Lang1_3=d[2];
				}
				if(audioToSet==2)
				{
					ch->Pids.Lang2_1=d[0];
					ch->Pids.Lang2_2=d[1];
					ch->Pids.Lang2_3=d[2];
				}
				if(audioToSet==3)
				{
					ch->Pids.Lang3_1=d[0];
					ch->Pids.Lang3_2=d[1];
					ch->Pids.Lang3_3=d[2];
				}

			}
			if(indicator==0x56 && ch->Pids.Teletext==0)
				ch->Pids.Teletext=elementary_PID;
			
			len2 -= x;
			len1 -= x;
			pointer += x;

		}
	}
	ch->PMTReady=true;
	if(last_section_number>section_number)
		return section_number+1;
	else
		return -1;

}
int Sections::decodeSDT(BYTE *buf,ChannelInfo ch[],int channels, int len)
{	

	int table_id = buf[0];
	int section_syntax_indicator = (buf[1]>>7) & 1;
	int section_length = ((buf[1]& 0xF)<<8) + buf[2];
	int transport_stream_id = (buf[3]<<8)+buf[4];
	int version_number = ((buf[5]>>1)&0x1F);
	int current_next_indicator = buf[5] & 1;
	int section_number = buf[6];
	int last_section_number = buf[7];
	int original_network_id = ((buf[8]& 0x1F)<<8)+buf[9];
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
	int channel;	 
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
		channel=-1;
		for(int n=0;n<channels;n++)
		{
			if(ch[n].ProgrammNumber==service_id)
			{
				channel=n;
				break;
			}
		}
		//
		if(channel!=-1)
		while (len2 > 0)
		{
			int indicator=buf[pointer];
			x = 0;
			x = buf[pointer + 1] + 2;
			//Log.Write("indicator = {0:X}",indicator);
			if (indicator == 0x48)
			{
				ServiceData serviceData;							
				DVB_GetService(buf+pointer,&serviceData);
				ch[channel].ServiceType=serviceData.ServiceType;
				strcpy((char*)ch[channel].ProviderName,serviceData.Provider);
				strcpy((char*)ch[channel].ServiceName,serviceData.Name);
				ch[channel].Scrambled=free_CA_mode;
				ch[channel].EITPreFollow=EIT_present_following_flag;
				ch[channel].EITSchedule=EIT_schedule_flag;
				ch[channel].SDTReady=true;
				ch[channel].NetworkID=original_network_id;
				//
				//
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
		return section_number+1;
	else
		return -1;

}
void Sections::DVB_GetService(BYTE *b,ServiceData *serviceData)
{
	int descriptor_tag;
	int descriptor_length;
	int service_provider_name_length;
	int service_name_length;
	int pointer = 0;
	memset(serviceData,0,sizeof(struct ServiceData));
	descriptor_tag = b[0];
	descriptor_length = b[1];
	serviceData->ServiceType = b[2];
	service_provider_name_length = b[3];
	pointer = 4;
	getString468A(b+pointer,service_provider_name_length,serviceData->Provider);
	pointer += service_provider_name_length;
	service_name_length = b[pointer];
	pointer += 1;
	getString468A(b+pointer, service_name_length,serviceData->Name);
}
void Sections::decodePAT(BYTE *pData,ChannelInfo chInfo[],int *channelCount, int len)
{


	int table_id = pData[0];
	int section_syntax_indicator = (pData[1]>>7) & 1;
	int section_length = ((pData[1]& 0xF)<<8) + pData[2];
	int transport_stream_id = (pData[3]<<8)+pData[4];
	int version_number = ((pData[5]>>1)&0x1F);
	int current_next_indicator = pData[5] & 1;
	int section_number = pData[6];
	int last_section_number = pData[7];
	int loop =(section_length - 9) / 4;
	int offset=0;
	*channelCount=loop;
	ChannelInfo ch;
	memset(&ch,0,sizeof(struct ChannelInfo));
	if(table_id!=0 || section_number>last_section_number)
	{
		*channelCount=0;
		return;
	}
	int pmtcount=0;
	for(int i=0;i<loop;i++)
	{
		offset=(8 +(i * 4));
		ch.ProgrammPMTPID=((pData[offset+2] & 0x1F)<<8)+pData[offset+3];
		ch.ProgrammNumber=(pData[offset]<<8)+pData[offset+1];
		ch.TransportStreamID=transport_stream_id;
		ch.PMTReady=false;
		Log("loop:%d prog:%d tsid:%x pid:%x", loop,ch.ProgrammNumber,ch.TransportStreamID,ch.ProgrammPMTPID);
		if(ch.ProgrammPMTPID>0x12)
		{
			chInfo[pmtcount]=ch;
			pmtcount++;
		}
		if(i>254)
			break;
	}
	*channelCount=pmtcount;
}

void Sections::getString468A(BYTE *b, int l1,char *text)
{
	int i = 0;
	int num=0;
	unsigned char c;
	char em_ON = (char)0x86;
	char em_OFF = (char)0x87;
	
	do
	{
		c = (char)b[i];
	/*	if(c=='Ü')
		{
			int a=0;
		}*/
		if ( (((BYTE)c) >= 0x80) && (((BYTE)c) <= 0x9F))
		{
			goto cont;
		}
		if (i==0 && ((BYTE)c) < 0x20)
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
				
		if ( ((BYTE)c) == 0x84)
		{
			text[num] = '\r';
			text[num+1]=0;
			num++;
			goto cont;
		}
				
		if (((BYTE)c) < 0x20)
		{
			goto cont;
		}
				
		text[num] = c;
		text[num+1]=0;
		num++;
cont:
		l1 -= 1;
		i += 1;
	}while (!(l1 <= 0));

}


void Sections::ATSCDecodeChannelTable(BYTE *buf,ChannelInfo *ch, int* channelsFound)
{
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
		if (descriptors_length<=0)
		{
			*channelsFound=0;
			return;
		}
		while (len < descriptors_length)
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
					DecodeExtendedChannelNameDescriptor( buf,start+len,channelInfo);
				break;
			}
			len += (descriptor_len+2);
		}
		start += descriptors_length;
		*channelsFound=*channelsFound+1;
	}
	Log("ATSCDecodeChannelTable() done, found %d channels", (*channelsFound));
}

void Sections::DecodeServiceLocationDescriptor( byte* buf,int start,ChannelInfo* channelInfo)
{

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
				channelInfo->Pids.AudioPid1=elementary_pid;
				break;
			default:
				break;
		}
	}
	Log("DecodeServiceLocationDescriptor() done");
}
void Sections::DecodeExtendedChannelNameDescriptor( byte* buf,int start,ChannelInfo* channelInfo)
{
	Log("DecodeExtendedChannelNameDescriptor() ");
	// tid   
	//  8       8------- 8-------
	// 76543210|76543210|76543210
	//    0        1        2    
	int descriptor_tag = buf[start+0];
	int descriptor_len = buf[start+1];
	Log(" tag:%x len:%d", descriptor_tag, descriptor_len);
	char* label = DecodeMultipleStrings(buf,start+2);
	if (label==NULL) return ;
	strcpy((char*)channelInfo->ServiceName,label);

	Log(" label:%s", label);
	delete [] label;
	Log("DecodeExtendedChannelNameDescriptor() done");
}
char* Sections::DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes)
{
	Log("DecodeString() compression type:%d numberofbytes:%d",compression_type, mode);
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
	return NULL;
}

char* Sections::DecodeMultipleStrings(byte* buf, int offset)
{
	int number_of_strings = buf[offset];
	Log("DecodeMultipleStrings() number_of_strings:%d",number_of_strings);
	

	for (int i=0; i < number_of_strings;++i)
	{
		Log("  string:%d", i);
		int ISO_639_language_code = (buf[offset+1]<<16)+(buf[offset+2]<<8)+(buf[offset+3]);
		int number_of_segments=buf[offset+4];
		int start=offset+5;
		Log("  segments:%d", number_of_segments);
		for (int k=0; k < number_of_segments;++k)
		{
			Log("  decode segment:%d", k);
			int compression_type = buf[start];
			int mode             = buf[start+1];
			int number_bytes     = buf[start+2];
			//decode text....
			char *label=DecodeString(buf, start+3, compression_type,mode,number_bytes);
			start += (number_bytes+3);
			if (label!=NULL) return label;
		}
	}
	return NULL;
}

//
//
// pes
void Sections::GetPES(BYTE *data,ULONG len,BYTE *pes)
{
	int ptr = 0; 
	int offset = 0; 
	bool isMPEG1=false;

	ULONG i = 0;
	for (;i<len;)
	{
		ptr = (0xFF & data[i + 4]) << 8 | (0xFF & data[i + 5]);
		isMPEG1 = (0x80 & data[i + 6]) == 0 ? true : false;
		offset = i + 6 + (!isMPEG1 ? 3 + (0xFF & data[i + 8]) : 0);
		memcpy(pes,data+offset,len-offset);
		i += 6 + ptr;
	}

}

HRESULT Sections::ParseAudioHeader(BYTE *data,AudioHeader *head)
{
    AudioHeader header;
	int limit = 32;

	if ((data[0] & 0xFF) != 0xFF || (data[1] & 0xF0) != 0xF0)
		return S_FALSE;

	header.ID = ((data[1] >> 3) &0x01) ;
	header.Emphasis = data[3] & 0x03;

	if (header.ID == 1 && header.Emphasis == 2)
		header.ID = 2;
	header.Layer = ((data[1] >>1) &0x03);

	if (header.Layer < 1)
		return S_FALSE;

	header.ProtectionBit = (data[1] & 0x01) ^ 1;
	header.Bitrate = AudioBitrates[header.ID][header.Layer-1][((data[2] >>4)& 0x0F)];
	if (header.Bitrate < 1)
		return S_FALSE;
	header.SamplingFreq = AudioFrequencies[header.ID][((data[2] >>2)& 0x03)];
	if (header.SamplingFreq == 0)
		return S_FALSE;

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
			return S_FALSE;
		if (header.Bitrate / header.Channel > 192000)
			return S_FALSE;

		if (header.Bitrate < 56000)
		{
			if (header.SamplingFreq == 32000)
				limit = 12;
			else
				limit = 8;
		}
		else 
			if (header.Bitrate < 96000)
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
	else 
	if (header.Layer == 2)  // MPEG-2
	{
		limit = 30;
	}

	if (header.Layer < 3)
	{
		if (header.Bound > limit)
			header.Bound = limit;
		header.Size = (header.SizeBase = 144 * header.Bitrate / header.SamplingFreq) + header.PaddingBit;
		memcpy(head,&header,sizeof(struct AudioHeader));
		return S_OK;
	}
	else
	{
		limit = 32;
		header.Size = (header.SizeBase = (12 * header.Bitrate / header.SamplingFreq) * 4) + (4 * header.PaddingBit);
		memcpy(head,&header,sizeof(struct AudioHeader));
		return S_OK;
	}

}

void Sections::DecodeEPG(byte* buf,int len)
{
	if (!m_bParseEPG) return;
	time_t currentTime=time(NULL);
	time_t timespan=currentTime-m_epgTimeout;
	if (timespan>10)
	{
		Log("EPG:timeout");
		m_bParseEPG=false;
		m_bEpgDone=true;
	}
	if (len<=14) return;
	int tableid = buf[0];
	if (tableid < 0x50 || tableid > 0x5f) return;
	int section_length = ((buf[1]& 0xF)<<8) + buf[2];

	int service_id = (buf[3]<<8)+buf[4];
	int version_number = (buf[5]>>1) & 0x1f;
	int current_next_indicator = buf[5]&1;
	int section_number=buf[6];
	int last_section_number=buf[7];
	int transport_id=(buf[8]<<8)+buf[9];
	int network_id=(buf[10]<<8)+buf[11];
	int segment_last_section_number=buf[12];
	int last_table_id=buf[13];

	if (last_table_id<tableid) return;
	if (last_table_id<0x50||last_table_id>0x6f) return;
	if (section_number>last_section_number) return;
	if (section_length>len) return;
	//if (last_section_number>88) return;

	//if (tableid!=0x5e) return;
	//if (service_id!=0x1a93) return;
	unsigned long key=(network_id<<32)+(transport_id<<16)+service_id;
	imapEPG it=m_mapEPG.find(key);
	if (it==m_mapEPG.end())
	{
		EPGChannel newChannel ;
		newChannel.original_network_id=network_id;
		newChannel.service_id=service_id;
		newChannel.transport_id=transport_id;
		newChannel.allSectionsReceived=false;
		m_mapEPG[key]=newChannel;
		it=m_mapEPG.find(key);
//		Log("add new channel table:%x onid:%x tsid:%x sid:%d",tableid,network_id,transport_id,service_id);
	}
	EPGChannel& channel=it->second;
	if (channel.allSectionsReceived) return;
	
	//did we already receive this section ?
	EPGChannel::imapSectionsReceived itSec=channel.mapSectionsReceived.find(section_number);
	if (itSec!=channel.mapSectionsReceived.end()) return; //yes
	channel.mapSectionsReceived[section_number]=true;

//	Log("EPG tid:%x len:%d %d (%d/%d) sid:%d tsid:%d onid:%d slsn:%d last table id:%x cn:%d version:%d", 
//		buf[0],len,section_length,section_number,last_section_number, 
//		service_id,transport_id,network_id,segment_last_section_number,last_table_id,
//		current_next_indicator,version_number);

	m_epgTimeout=time(NULL);
	int start=14;
	while (start+11 < len)
	{
		unsigned int event_id=(buf[start]<<8)+buf[start+1];
		unsigned long dateMJD=(buf[start+2]<<8)+buf[start+3];
		unsigned long timeUTC=(buf[start+4]<<16)+(buf[start+5]<<8)+buf[6];
		unsigned long duration=(buf[start+7]<<16)+(buf[start+8]<<8)+buf[9];
		unsigned int running_status=buf[start+10]>>5;
		unsigned int free_CA_mode=(buf[start+10]>>4) & 0x1;
		unsigned int descriptors_len=((buf[start+10]&0xf)<<8) + buf[start+11];
		EPGChannel::imapEvents itEvent=channel.mapEvents.find(event_id);
		if (itEvent==channel.mapEvents.end())
		{
			EPGEvent newEvent;
			newEvent.eventid=event_id;
			newEvent.dateMJD=dateMJD;
			newEvent.timeUTC=timeUTC;
			newEvent.duration=duration;
			newEvent.running_status=running_status;
			newEvent.free_CA_mode=free_CA_mode;
			channel.mapEvents[event_id]=newEvent;
			itEvent=channel.mapEvents.find(event_id);
		}
		EPGEvent& epgEvent=itEvent->second;
		
		start=start+12;
		unsigned int off=0;
//		Log(" event:%x date:%x time:%x duration:%x running:%d free:%d %d len:%d", event_id,dateMJD,timeUTC,duration,running_status,free_CA_mode,start,descriptors_len);
		while (off < descriptors_len)
		{
			int descriptor_tag = buf[start+off];
			int descriptor_len = buf[start+off+1];
			if (descriptor_len==0) return;
//			Log("  descriptor:%x len:%d",descriptor_tag,descriptor_len);
			if (descriptor_tag ==0x4d)
			{
				DecodeShortEventDescriptor( &buf[start+off],epgEvent);
			}
			if (descriptor_tag ==0x54)
			{
//				DecodeContentDescription( &buf[start+off],epgEvent);
			}
			off   +=(descriptor_len+2);
			start +=(descriptor_len+2);
		}
	}
}
void Sections::DecodeShortEventDescriptor(byte* buf, EPGEvent& event)
{
	char buffer[1028];
	int descriptor_tag = buf[0];
	int descriptor_len = buf[1];
	if(descriptor_tag!=0x4d) return;
	unsigned long ISO_639_language_code=(buf[2]<<16)+(buf[3]<<8)+buf[4];
	int event_len = buf[5];
	
	if (event_len >0)
	{
		getString468A(&buf[6],event_len,buffer);
		event.event=buffer;
		Log("  event:%s",buffer);
	}
	int off=6+event_len;
	int text_len = buf[off];
	if (text_len >0)
	{
		getString468A(&buf[off+1],text_len,buffer);
		event.text=buffer;
		Log("  text:%s",buffer);
	}
}
void Sections::DecodeContentDescription(byte* buf,EPGEvent& event)
{
	int      descriptor_tag;
	int      descriptor_length;		
	int      content_nibble_level_1;
	int      content_nibble_level_2;
	int      user_nibble_1;
	int      user_nibble_2;
	int nibble=0;
	char genreText[1024];
	int           len;

	strcpy(genreText,"");
	descriptor_tag		 = buf[0];
	descriptor_length    = buf[1];
	if(descriptor_tag!=0x54) return;

	len = descriptor_length;
	int pointer=  2;
	while ( len > 0) 
	{
		content_nibble_level_1	 = (buf[pointer+0]>>4) & 0xF;
		content_nibble_level_2	 = buf[pointer+0] & 0xF;
		user_nibble_1		 = (buf[pointer+1]>>4) & 0xF;
		user_nibble_2		 = buf[pointer+1] & 0xF;

		pointer   += 2;
		len -= 2;
		strcpy(genreText,"");
		nibble=(content_nibble_level_1 << 8) | content_nibble_level_2;
		switch(nibble)
		{
			case 0x0100: strcpy(genreText,"movie/drama (general)" );break;
			case 0x0101: strcpy(genreText,"detective/thriller" );break;
			case 0x0102: strcpy(genreText,"adventure/western/war" );break;
			case 0x0103: strcpy(genreText,"science fiction/fantasy/horror" );break;
			case 0x0104: strcpy(genreText,"comedy" );break;
			case 0x0105: strcpy(genreText,"soap/melodram/folkloric" );break;
			case 0x0106: strcpy(genreText,"romance" );break;
			case 0x0107: strcpy(genreText,"serious/classical/religious/historical movie/drama" );break;
			case 0x0108: strcpy(genreText,"adult movie/drama" );break;

			case 0x010E: strcpy(genreText,"reserved" );break;
			case 0x010F: strcpy(genreText,"user defined" );break;

				// News Current Affairs
			case 0x0200: strcpy(genreText,"news/current affairs (general)" );break;
			case 0x0201: strcpy(genreText,"news/weather report" );break;
			case 0x0202: strcpy(genreText,"news magazine" );break;
			case 0x0203: strcpy(genreText,"documentary" );break;
			case 0x0204: strcpy(genreText,"discussion/interview/debate" );break;
			case 0x020E: strcpy(genreText,"reserved" );break;
			case 0x020F: strcpy(genreText,"user defined" );break;

				// Show Games show
			case 0x0300: strcpy(genreText,"show/game show (general)" );break;
			case 0x0301: strcpy(genreText,"game show/quiz/contest" );break;
			case 0x0302: strcpy(genreText,"variety show" );break;
			case 0x0303: strcpy(genreText,"talk show" );break;
			case 0x030E: strcpy(genreText,"reserved" );break;
			case 0x030F: strcpy(genreText,"user defined" );break;

				// Sports
			case 0x0400: strcpy(genreText,"sports (general)" );break;
			case 0x0401: strcpy(genreText,"special events" );break;
			case 0x0402: strcpy(genreText,"sports magazine" );break;
			case 0x0403: strcpy(genreText,"football/soccer" );break;
			case 0x0404: strcpy(genreText,"tennis/squash" );break;
			case 0x0405: strcpy(genreText,"team sports" );break;
			case 0x0406: strcpy(genreText,"athletics" );break;
			case 0x0407: strcpy(genreText,"motor sport" );break;
			case 0x0408: strcpy(genreText,"water sport" );break;
			case 0x0409: strcpy(genreText,"winter sport" );break;
			case 0x040A: strcpy(genreText,"equestrian" );break;
			case 0x040B: strcpy(genreText,"martial sports" );break;
			case 0x040E: strcpy(genreText,"reserved" );break;
			case 0x040F: strcpy(genreText,"user defined" );break;

				// Children/Youth
			case 0x0500: strcpy(genreText,"childrens's/youth program (general)" );break;
			case 0x0501: strcpy(genreText,"pre-school children's program" );break;
			case 0x0502: strcpy(genreText,"entertainment (6-14 year old)" );break;
			case 0x0503: strcpy(genreText,"entertainment (10-16 year old)" );break;
			case 0x0504: strcpy(genreText,"information/education/school program" );break;
			case 0x0505: strcpy(genreText,"cartoon/puppets" );break;
			case 0x050E: strcpy(genreText,"reserved" );break;
			case 0x050F: strcpy(genreText,"user defined" );break;

			case 0x0600: strcpy(genreText,"music/ballet/dance (general)" );break;
			case 0x0601: strcpy(genreText,"rock/pop" );break;
			case 0x0602: strcpy(genreText,"serious music/classic music" );break;
			case 0x0603: strcpy(genreText,"folk/traditional music" );break;
			case 0x0604: strcpy(genreText,"jazz" );break;
			case 0x0605: strcpy(genreText,"musical/opera" );break;
			case 0x0606: strcpy(genreText,"ballet" );break;
			case 0x060E: strcpy(genreText,"reserved" );break;
			case 0x060F: strcpy(genreText,"user defined" );break;

			case 0x0700: strcpy(genreText,"arts/culture (without music, general)" );break;
			case 0x0701: strcpy(genreText,"performing arts" );break;
			case 0x0702: strcpy(genreText,"fine arts" );break;
			case 0x0703: strcpy(genreText,"religion" );break;
			case 0x0704: strcpy(genreText,"popular culture/traditional arts" );break;
			case 0x0705: strcpy(genreText,"literature" );break;
			case 0x0706: strcpy(genreText,"film/cinema" );break;
			case 0x0707: strcpy(genreText,"experimental film/video" );break;
			case 0x0708: strcpy(genreText,"broadcasting/press" );break;
			case 0x0709: strcpy(genreText,"new media" );break;
			case 0x070A: strcpy(genreText,"arts/culture magazine" );break;
			case 0x070B: strcpy(genreText,"fashion" );break;
			case 0x070E: strcpy(genreText,"reserved" );break;
			case 0x070F: strcpy(genreText,"user defined" );break;

			case 0x0800: strcpy(genreText,"social/political issues/economics (general)" );break;
			case 0x0801: strcpy(genreText,"magazines/reports/documentary" );break;
			case 0x0802: strcpy(genreText,"economics/social advisory" );break;
			case 0x0803: strcpy(genreText,"remarkable people" );break;
			case 0x080E: strcpy(genreText,"reserved" );break;
			case 0x080F: strcpy(genreText,"user defined" );break;

			case 0x0900: strcpy(genreText,"education/science/factual topics (general)" );break;
			case 0x0901: strcpy(genreText,"nature/animals/environment" );break;
			case 0x0902: strcpy(genreText,"technology/natural science" );break;
			case 0x0903: strcpy(genreText,"medicine/physiology/psychology" );break;
			case 0x0904: strcpy(genreText,"foreign countries/expeditions" );break;
			case 0x0905: strcpy(genreText,"social/spiritual science" );break;
			case 0x0906: strcpy(genreText,"further education" );break;
			case 0x0907: strcpy(genreText,"languages" );break;
			case 0x090E: strcpy(genreText,"reserved" );break;
			case 0x090F: strcpy(genreText,"user defined" );break;
			case 0x0A00: strcpy(genreText,"leisure hobbies (general)" );break;
			case 0x0A01: strcpy(genreText,"tourism/travel" );break;
			case 0x0A02: strcpy(genreText,"handicraft" );break;
			case 0x0A03: strcpy(genreText,"motoring" );break;
			case 0x0A04: strcpy(genreText,"fitness & health" );break;
			case 0x0A05: strcpy(genreText,"cooking" );break;
			case 0x0A06: strcpy(genreText,"advertisement/shopping" );break;
			case 0x0A07: strcpy(genreText,"gardening" );break;
			case 0x0A0E: strcpy(genreText,"reserved" );break;
			case 0x0A0F: strcpy(genreText,"user defined" );break;

			case 0x0B00: strcpy(genreText,"original language" );break;
			case 0x0B01: strcpy(genreText,"black & white" );break;
			case 0x0B02: strcpy(genreText,"unpublished" );break;
			case 0x0B03: strcpy(genreText,"live broadcast" );break;
			case 0x0B0E: strcpy(genreText,"reserved" );break;
			case 0x0B0F: strcpy(genreText,"user defined" );break;

			case 0x0E0F: strcpy(genreText,"reserved" );break;
			case 0x0F0F: strcpy(genreText,"user defined" );break;					
		}
		//Log("genre:%s", genreText);
		if (event.genre.size()==0)
			event.genre=genreText;
	}
}


void Sections::Reset()
{
	Log("Reset");
	m_mapEPG.clear();
	m_bParseEPG=false;
	m_bEpgDone=false;
	m_epgTimeout=time(NULL)+60;
}

void Sections::GrabEPG()
{
	Log("GrabEPG");
	m_mapEPG.clear();
	m_bParseEPG=true;
	m_bEpgDone=false;
	m_epgTimeout=time(NULL);
}
bool Sections::IsEPGGrabbing()
{
	return m_bParseEPG;
}
bool Sections::IsEPGReady()
{
	bool ready=m_bEpgDone;
	if (ready) 
	{
		Log("EPG done");
		m_bParseEPG=false;
		m_bEpgDone=false;
	}
	return ready;
}
ULONG Sections::GetEPGChannelCount( )
{
//	Log("GetEPGChannelCount:%d",m_mapEPG.size());
	return m_mapEPG.size();
}
ULONG  Sections::GetEPGEventCount( ULONG channel)
{
	if (channel>=m_mapEPG.size()) return 0;
	int count=0;
	imapEPG it =m_mapEPG.begin();
	while (count < channel) { it++; count++;}
	EPGChannel& epgChannel=it->second;

	
//	Log("GetEPGEventCount:%d %d",channel,epgChannel.mapEvents.size());
	return epgChannel.mapEvents.size();
}
void Sections::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
//	Log("GetEPGChannel#%d",channel);

	if (channel>=m_mapEPG.size()) return;
	ULONG count=0;
	imapEPG it =m_mapEPG.begin();
	while (count < channel && it!=m_mapEPG.end()) { it++; count++;}
//	Log("count:%d",count);
	if (it==m_mapEPG.end())
	{
//		Log("GetEPGChannel #%d not found",channel);
	}
	EPGChannel& epgChannel=it->second;

//	Log("  onid:%x tsid:%x sid:%x", epgChannel.original_network_id,epgChannel.transport_id,epgChannel.service_id);

	*networkId=epgChannel.original_network_id;
	*transportid=epgChannel.transport_id;
	*service_id=epgChannel.service_id;
//	Log("GetEPGChannel:%d done",channel);
}
void Sections::GetEPGEvent( ULONG channel,  ULONG eventid, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char**event,  char** text, char** genre    )
{
	*dateMJD=0;
	*timeUTC=0;
	*duration=0;
	
	if (channel>=m_mapEPG.size()) return;
	int count=0;
	imapEPG it =m_mapEPG.begin();
	while (count < channel) { it++; count++;}
	EPGChannel& epgChannel=it->second;

	if (eventid >= epgChannel.mapEvents.size()) return;
	count=0;
	EPGChannel::imapEvents itEvent=epgChannel.mapEvents.begin();
	while (count < eventid) { itEvent++; count++;}
	EPGEvent& epgEvent=itEvent->second;
	*dateMJD=epgEvent.dateMJD;
	*timeUTC=epgEvent.timeUTC;
	*duration=epgEvent.duration;
	*event=(char*)epgEvent.event.c_str(); 
	*text=(char*)epgEvent.text.c_str() ;
	*genre=(char*)epgEvent.genre.c_str() ;
}
