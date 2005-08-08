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

#ifdef DEBUG
char *logbuffer=NULL; 
void Log(const char *fmt, ...) 
{
	if (logbuffer==NULL)
	{
		logbuffer=new char[100000];
	}
	va_list ap;
	va_start(ap,fmt);

	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("MPSA.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetSystemTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			logbuffer);
		fclose(fp);
	}
};
#else
void Log(const char *fmt, ...) 
{
}
#endif

ULONG Sections::GetSectionCRCValue(byte* data,int ptr)
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
	DeleteFile("MPSA.log");
	m_patTSID=-1;
	m_patSectionLen=-1;
	m_patTableVersion=-1;
	m_bParseEPG=false;
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
	if (offset>=188) return S_FALSE;
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
	if (len <12) return -1;

	
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
		if (pointer+1>=len) return -1;
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
		if (pointer+4>=len) return -1;
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
			if (pointer+1>=len) return -1;
			x = 0;
			int indicator=buf[pointer];
			x = buf[pointer + 1] + 2;
			if(indicator==0x6A)
				ch->Pids.AC3=elementary_PID;
			if(indicator==0x0A)
			{
				
				if (pointer+4>=len) return -1;
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
	Log("DecodePMT pid:0x%x pcrpid:0x%x videopid:0x%x audiopid:0x%x ac3pid:0x%x",
		ch->ProgrammPMTPID, ch->PCRPid,ch->Pids.VideoPid,ch->Pids.AudioPid1,ch->Pids.AC3);
	ch->PMTReady=true;
	if(last_section_number>section_number)
		return section_number+1;
	else
		return -1;
}
int Sections::decodeSDT(BYTE *buf,ChannelInfo ch[],int channels, int len)
{

	if (len < 10) return 0;
	if (channels<=0) return 0;
	
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
	if (table_id!=0x42) return 0;

	
	//Log.Write("decodeSDTTable len={0}/{1} section no:{2} last section no:{3}", buf.Length,section_length,section_number,last_section_number);

	while (len1 > 0)
	{
		if (pointer+4 >=len) return 0;
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
		if(channel==-1)
		{
			Log("sdt: channel not found for program:%x", service_id);
		}

		while (len2 > 0)
		{
			if (pointer+1 >=len) return 0;
			int indicator=buf[pointer];
			x = 0;
			x = buf[pointer + 1] + 2;
			//Log.Write("indicator = {0:X}",indicator);
			if (indicator == 0x48)
			{
				if (channel>=0)
				{
					if (ch[channel].SDTReady==false)
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
						Log("sdt: pmt:0x%x provider:'%s' channel:'%s' onid:0x%x tsid:0x%x", ch[channel].ProgrammPMTPID,ch[channel].ProviderName,ch[channel].ServiceName,ch[channel].NetworkID, transport_stream_id);
					}
				}
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
	memset(serviceData,0,sizeof(struct stserviceData));
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
bool Sections::IsNewPat(BYTE *pData, int len)
{
	int table_id = pData[0];
	if (table_id!=0) return false;
	if (len<9) return false;
	int section_syntax_indicator = (pData[1]>>7) & 1;
	int section_length = ((pData[1]& 0xF)<<8) + pData[2];
	int transport_stream_id = (pData[3]<<8)+pData[4];
	int version_number = ((pData[5]>>1)&0x1F);
	int current_next_indicator = pData[5] & 1;
	int section_number = pData[6];
	int last_section_number = pData[7];

	if (version_number==m_patTableVersion && 
		transport_stream_id==m_patTSID && 
		section_length==m_patSectionLen) return false;
	if(table_id!=0 || section_number!=0||last_section_number!=0) return false;
	Log("Found new PAT: version:%x==%x tsid:%x==%x len:%x==%x",
		version_number,m_patTableVersion, 
		transport_stream_id,m_patTSID,
		section_length,m_patSectionLen);
	return true;
}
void Sections::decodePAT(BYTE *pData,ChannelInfo chInfo[],int *channelCount, int len)
{
	int table_id = pData[0];
	if (table_id!=0) return;
	if (len<9) return;
	int section_syntax_indicator = (pData[1]>>7) & 1;
	int section_length = ((pData[1]& 0xF)<<8) + pData[2];
	int transport_stream_id = (pData[3]<<8)+pData[4];
	int version_number = ((pData[5]>>1)&0x1F);
	int current_next_indicator = pData[5] & 1;
	int section_number = pData[6];
	int last_section_number = pData[7];
	int loop =(section_length - 9) / 4;
	int offset=0;
	if(table_id!=0 || section_number!=0||last_section_number!=0)
	{
		return;
	}
	m_patTableVersion=version_number;
	m_patTSID=transport_stream_id;
	m_patSectionLen=section_length;
	*channelCount=loop;

	Log("Decode pat:%x len:%x tsid:0x%x version:%x (%d/%d) channels:%d",
		table_id,section_length,transport_stream_id,version_number,section_number,last_section_number,(*channelCount));
	ChannelInfo ch;
	
	memset(&ch,0,sizeof(struct chInfo));

	int pmtcount=0;
	for(int i=0;i<loop;i++)
	{
		offset=(8 +(i * 4));
		ch.ProgrammPMTPID=((pData[offset+2] & 0x1F)<<8)+pData[offset+3];
		ch.ProgrammNumber=(pData[offset]<<8)+pData[offset+1];
		ch.TransportStreamID=transport_stream_id;
		ch.PMTReady=false;
		Log("  ch:%d prog:%d tsid:0x%x pmtpid:0x%x", i,ch.ProgrammNumber,ch.TransportStreamID,ch.ProgrammPMTPID);
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
		if (i+9>len) 
			return;
		ptr = (0xFF & data[i + 4]) << 8 | (0xFF & data[i + 5]);
		isMPEG1 = (0x80 & data[i + 6]) == 0 ? true : false;
		offset = i + 6 + (!isMPEG1 ? 3 + (0xFF & data[i + 8]) : 0);
		if (offset+(len-offset) >=len) 
			return;
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
		memcpy(head,&header,sizeof(struct staudioHeader));
		return S_OK;
	}
	else
	{
		limit = 32;
		header.Size = (header.SizeBase = (12 * header.Bitrate / header.SamplingFreq) * 4) + (4 * header.PaddingBit);
		memcpy(head,&header,sizeof(struct staudioHeader));
		return S_OK;
	}

}

void Sections::DecodeEPG(byte* buf,int len)
{
	//Log("DecodeEPG():%d",len);
	if (!m_bParseEPG) return;
	if (buf==NULL) return;

	time_t currentTime=time(NULL);
	time_t timespan=currentTime-m_epgTimeout;
	if (timespan>60)
	{
		Log("EPG:timeout");
		m_bParseEPG=false;
		m_bEpgDone=true;
	}
	if (len<=14) return;
	int tableid = buf[0];
	
	if (tableid < 0x50 || tableid > 0x6f) return;
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


	unsigned long key=(network_id<<32)+(transport_id<<16)+service_id;
	//Log("DecodeEPG():key %x",key);
	imapEPG it=m_mapEPG.find(key);
	if (it==m_mapEPG.end())
	{
		//Log("DecodeEPG():new channel");
		EPGChannel newChannel ;
		newChannel.original_network_id=network_id;
		newChannel.service_id=service_id;
		newChannel.transport_id=transport_id;
		newChannel.allSectionsReceived=false;
		m_mapEPG[key]=newChannel;
		it=m_mapEPG.find(key);
		//Log("epg:add new channel table:%x onid:%x tsid:%x sid:%d",tableid,network_id,transport_id,service_id);
	}
	if (it==m_mapEPG.end()) return;
	EPGChannel& channel=it->second;
	if (channel.allSectionsReceived) return;

	//Log("DecodeEPG() check section");
	//did we already receive this section ?
	EPGChannel::imapSectionsReceived itSec=channel.mapSectionsReceived.find(section_number);
	if (itSec!=channel.mapSectionsReceived.end()) return; //yes
	channel.mapSectionsReceived[section_number]=true;



	//Log("epg: tid:%x len:%d %d (%d/%d) sid:%d tsid:%d onid:%d slsn:%d last table id:%x cn:%d version:%d", 
	//	buf[0],len,section_length,section_number,last_section_number, 
	//	service_id,transport_id,network_id,segment_last_section_number,last_table_id,
	//	current_next_indicator,version_number);

	m_epgTimeout=time(NULL);
	int start=14;
	while (start+11 < len)
	{
		//Log("epg:   %d/%d", start,len);
		unsigned int event_id=(buf[start]<<8)+buf[start+1];
		unsigned long dateMJD=(buf[start+2]<<8)+buf[start+3];
		unsigned long timeUTC=(buf[start+4]<<16)+(buf[start+5]<<8)+buf[6];
		unsigned long duration=(buf[start+7]<<16)+(buf[start+8]<<8)+buf[9];
		unsigned int running_status=buf[start+10]>>5;
		unsigned int free_CA_mode=(buf[start+10]>>4) & 0x1;
		int descriptors_len=((buf[start+10]&0xf)<<8) + buf[start+11];
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
		int off=0;
		//Log("epg:   event:%x date:%x time:%x duration:%x running:%d free:%d %d len:%d", event_id,dateMJD,timeUTC,duration,running_status,free_CA_mode,start,descriptors_len);
		while (off < descriptors_len)
		{
			if (start+off+1>len) return;
			int descriptor_tag = buf[start+off];
			int descriptor_len = buf[start+off+1];
			if (descriptor_len==0) return;
			if (start+off+descriptor_tag>len) return;
			//Log("epg:     descriptor:%x len:%d",descriptor_tag,descriptor_len);
			if (descriptor_tag ==0x4d)
			{
				DecodeShortEventDescriptor( &buf[start+off],epgEvent);
			}
			if (descriptor_tag ==0x54)
			{
				DecodeContentDescription( &buf[start+off],epgEvent);
			}
			if (descriptor_tag ==0x4e)
			{
				DecodeExtendedEvent(&buf[start+off],epgEvent);
			}
			off   +=(descriptor_len+2);
		}
		start +=descriptors_len;
	}
}

void Sections::DecodeExtendedEvent(byte* data, EPGEvent& event)
{
	int descriptor_tag;
	int descriptor_length;
	int descriptor_number;
	int last_descriptor_number;
	int text_length;
	int length_of_items;

	string text = "";
	int pointer = 0;
	int lenB;
	int len1;
	int item_description_length;
	int item_length;
	string item = "";

	descriptor_tag = data[0];
	descriptor_length = data[1];
	descriptor_number = (data[1]>>4) & 0xF;
	last_descriptor_number = data[1] & 0xF;
	event.language=(data[3]<<16)+(data[4]<<8)+data[5];
	length_of_items = data[6];
	pointer += 7;
	lenB = descriptor_length - 5;
	len1 = length_of_items;

	while (len1 > 0)
	{
		item_description_length = data[pointer];
		if (pointer+item_description_length>descriptor_length) return;

		char* buffer= new char[item_description_length+10];
		getString468A(&data[pointer+1], item_description_length,buffer);
		delete[] buffer;

		string testText=buffer;
		pointer += 1 + item_description_length;
		if (testText.size()==0)
			testText="-not avail.-";

		item_length = data[pointer];
		if (pointer+item_length>descriptor_length) return;
		buffer= new char[item_length+10];
		getString468A(&data[pointer+1], item_length,buffer);
		item = buffer;
		delete[] buffer;
		
		pointer += 1 + item_length;
		len1 -= (2 + item_description_length + item_length);
		lenB -= (2 + item_description_length + item_length);
	};
	text_length = data[pointer];
	pointer += 1;
	lenB -= 1;
	if (pointer+text_length>descriptor_length) return;
	char* buffer= new char[item_length+10];
	getString468A(&data[pointer], text_length,buffer);
	text = buffer;
	delete[] buffer;

	if (item.size()>0)
		event.event=item;
	if (text.size()>0)
		event.text=text;
}

void Sections::DecodeShortEventDescriptor(byte* buf, EPGEvent& event)
{
	char* buffer;
	int descriptor_tag = buf[0];
	int descriptor_len = buf[1];
	if(descriptor_tag!=0x4d) return;
	if (descriptor_len<6) return;

	unsigned long ISO_639_language_code=(buf[2]<<16)+(buf[3]<<8)+buf[4];
	event.language=ISO_639_language_code;
	int event_len = buf[5];
	
	if (event_len >0)
	{
		if (6+event_len > descriptor_len) return;
		buffer = new char[event_len+10];
		getString468A(&buf[6],event_len,buffer);
		event.event=buffer;
		delete [] buffer;
		//Log("  event:%s",event.event.c_str());
	}
	int off=6+event_len;
	int text_len = buf[off];
	if (text_len >0)
	{
		if (off+text_len > descriptor_len) return;
		buffer = new char[text_len+10];
		getString468A(&buf[off+1],text_len,buffer);
		event.text=buffer;
		delete [] buffer;
		//Log("  text:%s",event.text.c_str());
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
		if (pointer+1>descriptor_length) return;
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

void Sections::ResetEPG()
{
	m_mapEPG.clear();
	m_bParseEPG=false;
	m_bEpgDone=false;
	
	m_epgTimeout=time(NULL)+60;
}
void Sections::Reset()
{
	//Log("sections::Reset");
	m_patTSID=-1;
	m_patSectionLen=-1;
	m_patTableVersion=-1;
}

void Sections::GrabEPG()
{
	//Log("GrabEPG");
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
		//Log("EPG done");
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
	while (count < (int)channel) { it++; count++;}
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
	while (count < (int)channel && it!=m_mapEPG.end()) { it++; count++;}
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
void Sections::GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char**event,  char** text, char** genre    )
{
	*dateMJD=0;
	*timeUTC=0;
	*duration=0;
	
	if (channel>=m_mapEPG.size()) return;
	int count=0;
	imapEPG it =m_mapEPG.begin();
	while (count < (int)channel) { it++; count++;}
	EPGChannel& epgChannel=it->second;

	if (eventid >= epgChannel.mapEvents.size()) return;
	count=0;
	EPGChannel::imapEvents itEvent=epgChannel.mapEvents.begin();
	while (count < (int)eventid) { itEvent++; count++;}
	EPGEvent& epgEvent=itEvent->second;
	*dateMJD=epgEvent.dateMJD;
	*timeUTC=epgEvent.timeUTC;
	*duration=epgEvent.duration;
	*language=epgEvent.language;
	*event=(char*)epgEvent.event.c_str(); 
	*text=(char*)epgEvent.text.c_str() ;
	*genre=(char*)epgEvent.genre.c_str() ;
}
