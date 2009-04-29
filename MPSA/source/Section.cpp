
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: Agree
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
#include "Section.h"
#include "mpsa.h"
#include "crc.h"
#include "autostring.h"
//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

char *logbuffer=NULL; 
#ifdef DEBUG
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
		GetLocalTime(&systemTime);
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
void Dump(const char *fmt, ...) 
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
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			logbuffer);
		fclose(fp);
	}
};

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
	for (int i=0; i < MAX_PAT_TABLES;++i)
	{
		m_patTSID[i]=-1;
		m_patSectionLen[i]=-1;
		m_patTableVersion[i]=-1;
	}
	m_patsFound=0;
}

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
	try
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
		ch->PMTReady=1;
		if(last_section_number>section_number)
			return section_number+1;
		else
			return -1;
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodePMT()");
	}	
	return -1;
}
int Sections::decodeSDT(BYTE *buf,ChannelInfo ch[],int channels, int len)
{
	try
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
		int original_network_id = ((buf[8])<<8)+buf[9];
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

		
	//	Log("decodeSDTTable len=%d section no:%d last section no:%d channels:%d", section_length,section_number,last_section_number,channels);

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
				return -1;
				//Log("sdt: channel not found for program:%d", service_id);
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
						if (ch[channel].SDTReady==0)
						{
							ServiceData serviceData;							
							DVB_GetService(buf+pointer,&serviceData);
							ch[channel].ServiceType=serviceData.ServiceType;
							strcpy((char*)ch[channel].ProviderName,serviceData.Provider);
							strcpy((char*)ch[channel].ServiceName,serviceData.Name);
							ch[channel].Scrambled=free_CA_mode;
							ch[channel].EITPreFollow=EIT_present_following_flag;
							ch[channel].EITSchedule=EIT_schedule_flag;
							ch[channel].SDTReady=1;
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
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodeST()");
	}	
	return -1;
}

void Sections::DVB_GetService(BYTE *b,ServiceData *serviceData)
{
	try
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
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DVB_GetService()");
	}	
}
bool Sections::IsNewPat(BYTE *pData, int len)
{
	try
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

		if(table_id!=0 || section_number!=0||last_section_number!=0) return false;//invalid table id
		for (int i=0; i < m_patsFound; ++i)
		{
			if (version_number==m_patTableVersion[i] && 
				transport_stream_id==m_patTSID[i] && 
				section_length==m_patSectionLen[i]) return false; // same version number as before
		}
		int loop =(section_length - 9) / 4;
		if ( ( (section_length-9) %4 ) !=0) 
		{
			//Log("newpat: invalid section length:%d", section_length);
			return false; // invalid length
		}
		if (loop < 1) 
		{
			Log("newpat: invalid loop<1 :%d", loop);
			return false; // invalid number of channels
		}

		int pmtcount=0;
		for(int i=0;i<loop;i++)
		{
			int offset=(8 +(i * 4));
			int PMTPID=((pData[offset+2] & 0x1F)<<8)+pData[offset+3];
			if (PMTPID < 0x10 || PMTPID >=0x1fff) 
			{
				Log("newpat: invalid pid:%x", PMTPID);
				return false;
			}
		}
		Log("Found new PAT: version:0x%x tsid:0x%x len:0x%x channels:%d ssi:%x cni:%x",
			version_number,transport_stream_id,section_length,loop,section_syntax_indicator,current_next_indicator);
		return true;
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::IsNewPAT()");
	}	
	return false;
}
void Sections::decodePAT(BYTE *pData,ChannelInfo chInfo[],int *channelCount, int len)
{
	if (m_patsFound>=MAX_PAT_TABLES) return;
	if ((*channelCount)>=500) return;
	try
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
		m_patTableVersion[m_patsFound]=version_number;
		m_patTSID[m_patsFound]=transport_stream_id;
		m_patSectionLen[m_patsFound]=section_length;
		int channelOffset=*channelCount;
		if (channelOffset>500) return;

		Log("Decode pat:0x%x len:0x%x tsid:0x%x version:0x%x (%d/%d) channels:%d",
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
			ch.PMTReady=0;
			ch.SDTReady=0;
			Log("  ch:%d prog:%d tsid:0x%x pmtpid:0x%x", i,ch.ProgrammNumber,ch.TransportStreamID,ch.ProgrammPMTPID);
			if(ch.ProgrammPMTPID>0x12)
			{
				chInfo[channelOffset+pmtcount]=ch;

				pmtcount++;
				if (channelOffset+pmtcount>500)
					break;
			}
			if(i>254)
				break;
		}
		*channelCount=(*channelCount)+pmtcount;
		m_patsFound++;
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodePAT()");
	}	
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
void Sections::ResetPAT()
{
	Log("sections::Reset");
	for (int i=0; i < MAX_PAT_TABLES;++i)
	{
		m_patTSID[i]=-1;
		m_patSectionLen[i]=-1;
		m_patTableVersion[i]=-1;
	}
	m_patsFound=0;
	
}

void  Sections::decodeNITTable(byte* buf,ChannelInfo *channels, int channelCount)
{
	try
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

		//  0        1         2        3        4         5       6      7         8         9       10
		//76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
		//++++++++ -+--++++ ++++++++ -------- -------- ++-----+ -------- ++++++++ ----++++ ++++++++
		table_id = buf[0];
		//0x40=actual (current) network
		//0x41=other networks
		if (table_id!=0x40 && table_id!=0x41) return;
		
		
		section_syntax_indicator = buf[1] &0x80;
		section_length = ((buf[1] &0xF)<<8)+buf[2];
		network_id = (buf[3]<<8)+buf[4];
		version_number = (buf[5]>>1) &0x1F;
		current_next_indicator = buf[5]&1;
		section_number = buf[6];
		last_section_number = buf[7];
		network_descriptor_length = ((buf[8]&0xF)<<8)+buf[9];

		//Log("NIT section len:%d network_descriptor_length :%d", section_length,network_descriptor_length);
		l1 = network_descriptor_length;
		pointer = 10;
		int x = 0;

		while (l1 > 0)
		{
			int indicator=buf[pointer];
			x = buf[pointer + 1] + 2;
			//Log("decode nit desc1:%x len:%d", indicator,x);
			/*
			if(indicator==0x40)
			{
				CAutoString networkName (x+10);
				strncpy(networkName.GetBuffer() ,(char*)&buf[pointer+2],x-2);
				networkName.GetBuffer()[x-2]=0;
				m_nit.NetworkName=networkName.GetBuffer();
			}*/
			l1 -= x;
			pointer += x;
		}
		pointer=10+network_descriptor_length;

		if (pointer > section_length)
		{
			return;
		}
	//	Log("NIT: decode() network:'%s'", m_nit.NetworkName);
		
		transport_stream_loop_length = ((buf[pointer] &0xF)<<8)+buf[pointer+1];
		l1 = transport_stream_loop_length;
		pointer += 2;
		
		while (l1 > 0)
		{
			//Log("loop1: %d/%d l1:%d",pointer,section_length,l1);
			if (pointer+2 > section_length)
			{
				//Log("check1");
				return;
			}
			transport_stream_id = (buf[pointer]<<8)+buf[pointer+1];
			original_network_id = (buf[pointer+2]<<8)+buf[pointer+3];
			transport_descriptor_length = ((buf[pointer+4] & 0xF)<<8)+buf[pointer+5];
			pointer += 6;
			l1 -= 6;
			l2 = transport_descriptor_length;
			
			//Log("    transport_descriptor_length :%d", transport_descriptor_length);
			while (l2 > 0)
			{
				//Log("    loop2: %d/%d l1:%d",pointer,transport_descriptor_length,l2);
				if (pointer+2 > section_length)
				{
				//	Log("check2");
					return;
				}
				int indicator=buf[pointer];
				x = buf[pointer + 1]+2 ;
				//Log("     decode desc:%x len:%d", indicator,x);
				if(indicator==0x43) // sat
				{
					try
					{
						DVB_GetSatDelivSys(&buf[pointer],x);
					}
					catch(...)
					{
						Log("exception in DVB_GetSatDelivSys");
					}
				}
				if(indicator==0x44) // cable
				{
					try
					{
						DVB_GetCableDelivSys(&buf[pointer],x);
					}
					catch(...)
					{
						Log("exception in DVB_GetCableDelivSys");
					}
				}
				if(indicator==0x5A) // terrestrial
				{
					try
					{
						DVB_GetTerrestrialDelivSys(&buf[pointer],x);
					}
					catch(...)
					{
						Log("exception in DVB_GetTerrestrialDelivSys");
					}
				}
				
				if(indicator==0x83) // lcn
				{
					try
					{
						DVB_GetLogicalChannelNumber(original_network_id,transport_stream_id,&buf[pointer], channels,channelCount);
					}
					catch(...)
					{
						Log("exception in DVB_GetLogicalChannelNumber");
					}
				}
				//
				pointer += x;
				l2 -= x;
				l1 -= x;
			}
		}
		
		//Log("NIT: terrestial:%d satellite:%d cable:%d LCN:%d",
		//	m_nit.terrestialNIT.size(),m_nit.satteliteNIT.size(),m_nit.cableNIT.size(),m_nit.lcnNIT.size());
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodeNIT()");
	}	
}

void Sections::DVB_GetLogicalChannelNumber(int original_network_id,int transport_stream_id,byte* buf,ChannelInfo *channels, int channelCount)
{
	try
	{
		// 32 bits per record
		int n = buf[1] / 4;
		if (n < 1)
			return;

		// desc id, desc len, (service id, service number)
		int pointer=2;
		int ServiceID, LCN;
		for (int i = 0; i < n; i++) 
		{
			//service id:16
			//visible_service_flag:1
			//reserved:5
			//logical channel number:10
			ServiceID = 0;
			LCN = 0;
			ServiceID = (buf[pointer+0]<<8)|(buf[pointer+1]&0xff);
			LCN		  = (buf[pointer+2]&0x03<<8)|(buf[pointer+3]&0xff);
			pointer+=4;
			bool alreadyAdded=false;
			for (int j=0; j < m_nit.lcnNIT.size();++j)
			{
				NITLCN& lcn = m_nit.lcnNIT[j];
				if (lcn.LCN==LCN && lcn.network_id==original_network_id && lcn.transport_id==transport_stream_id && lcn.service_id==ServiceID)
				{
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				if (original_network_id>0 && transport_stream_id>0 &&ServiceID>0 && LCN>0)
				{
					NITLCN lcn;
					lcn.LCN=LCN;
					lcn.network_id=original_network_id;
					lcn.transport_id=transport_stream_id;
					lcn.service_id=ServiceID;
					m_nit.lcnNIT.push_back(lcn);
					Log("LCN:%03.3d network id:0x%x transport id:0x%x service id:0x%x (%d)", LCN,original_network_id,transport_stream_id,ServiceID,m_nit.lcnNIT.size());
				}
			}
		}
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DVB_GetLogicalChannelNumber()");
	}	
}

void Sections::DVB_GetSatDelivSys(byte* b,int maxLen)
{
	try
	{
		if(b[0]==0x43 && maxLen>=13)
		{
			int descriptor_tag = b[0];
			int descriptor_length = b[1];
			
			if (descriptor_length>13) 
			{
				Log("DVB_GetSatDelivSys() desclen:%d", descriptor_length);
				return;
			}
			NITSatDescriptor satteliteNIT;
			satteliteNIT.Frequency = (10000000* ((b[2]>>4)&0xf));
			satteliteNIT.Frequency+= (1000000*  ((b[2]&0xf)));
			satteliteNIT.Frequency+= (100000*   ((b[3]>>4)&0xf));
			satteliteNIT.Frequency+= (10000*    ((b[3]&0xf)));
			satteliteNIT.Frequency+= (1000*     ((b[4]>>4)&0xf));
			satteliteNIT.Frequency+= (100*      ((b[4]&0xf)));
			satteliteNIT.Frequency+= ( 10*      ((b[5]>>4)&0xf));
			satteliteNIT.Frequency+= (b[5]&0xf);

			satteliteNIT.OrbitalPosition+= (1000*     ((b[6]>>4)&0xf));
			satteliteNIT.OrbitalPosition+= (100*      ((b[6]&0xf)));
			satteliteNIT.OrbitalPosition+= ( 10*      ((b[7]>>4)&0xf));
			satteliteNIT.OrbitalPosition+= (b[7]&0xf);

			satteliteNIT.WestEastFlag = (b[8] & 0x80)>>7;
			satteliteNIT.Polarisation = (b[8]& 0x60)>>5;
			if(satteliteNIT.Polarisation>1)
				satteliteNIT.Polarisation-=2;
			// polarisation
			// 0 - horizontal/left (linear/circluar)
			// 1 - vertical/right (linear/circluar)
			satteliteNIT.Modulation = (b[8] & 0x1F);
			satteliteNIT.Symbolrate = (1000000* ((b[9]>>4)&0xf));
			satteliteNIT.Symbolrate+= (100000*  ((b[9]&0xf)));
			satteliteNIT.Symbolrate+= (10000*   ((b[10]>>4)&0xf));
			satteliteNIT.Symbolrate+= (1000*    ((b[10]&0xf)));
			satteliteNIT.Symbolrate+= (100*     ((b[11]>>4)&0xf));
			satteliteNIT.Symbolrate+= (10*      ((b[11]&0xf)));
			satteliteNIT.Symbolrate+= (         ((b[12]>>4)&0xf));
			satteliteNIT.FECInner = (b[12] & 0xF);
			

			
			bool alreadyAdded=false;
			for (int i=0; i < m_nit.satteliteNIT.size();++i)
			{
				NITSatDescriptor& nit=m_nit.satteliteNIT[i];
				if (nit.Frequency==satteliteNIT.Frequency)
				{
					Dump("Sat nit: frequency:%d Symbolrate:%d Polarisation:%d", 
						satteliteNIT.Frequency,satteliteNIT.Symbolrate,satteliteNIT.Polarisation);
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				m_nit.satteliteNIT.push_back(satteliteNIT);
			}
		}	

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DVB_GetSatDelivSys()");
	}	
}
void Sections::DVB_GetTerrestrialDelivSys(byte*b , int maxLen)
{
	try
	{

		if(b[0]==0x5A)
		{
			int descriptor_tag = b[0];
			int descriptor_length = b[1];
			if (descriptor_length>11) 
			{
				Log("DVB_GetTerrestrialDelivSys() desclen:%d", descriptor_length);
				return;
			}
			NITTerrestrialDescriptor terrestialNIT;
			terrestialNIT.CentreFrequency= (b[2]<<24)+(b[3]<<16)+(b[4]<<8)+b[5];
			if (terrestialNIT.CentreFrequency < 40000000 ||
				terrestialNIT.CentreFrequency >900000000) return; // invalid frequency

			terrestialNIT.Bandwidth = (b[6]>>5);
			// bandwith
			// 0- 8 MHz
			// 1- 7 MHz
			// 2- 6 MHz
			if (terrestialNIT.Bandwidth==0) terrestialNIT.Bandwidth=8;
			else if (terrestialNIT.Bandwidth==1) terrestialNIT.Bandwidth=7;
			else if (terrestialNIT.Bandwidth==2) terrestialNIT.Bandwidth=6;
			else terrestialNIT.Bandwidth=8;

			terrestialNIT.Constellation=(b[7]>>6);
			// constellation
			// 0- QPSK
			// 1- 16-QAM
			// 2- 64-QAM
			if (terrestialNIT.Constellation==0) terrestialNIT.Constellation=BDA_MOD_QPSK ;
			else if (terrestialNIT.Constellation==1) terrestialNIT.Constellation=BDA_MOD_16QAM ;
			else if (terrestialNIT.Constellation==2) terrestialNIT.Constellation=BDA_MOD_64QAM ;
			else  terrestialNIT.Constellation=BDA_MOD_NOT_SET;

			terrestialNIT.HierarchyInformation=(b[7]>>3)& 7;
			// 0- non-hierarchical
			// 1- a == 1
			// 2- a == 2
			// 3- a == 4
			switch (terrestialNIT.HierarchyInformation)
			{
				case 0:	terrestialNIT.HierarchyInformation=BDA_HALPHA_NOT_DEFINED ;break;
				case 1:	terrestialNIT.HierarchyInformation=BDA_HALPHA_1 ;break;
				case 2:	terrestialNIT.HierarchyInformation=BDA_HALPHA_2 ;break;
				case 3:	terrestialNIT.HierarchyInformation=BDA_HALPHA_4 ;break;
				default:terrestialNIT.HierarchyInformation=BDA_GUARD_NOT_SET; break;
			}
			terrestialNIT.CoderateHPStream=(b[7] & 7);
			terrestialNIT.CoderateLPStream=(b[8]>>5);
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
			switch (terrestialNIT.CoderateHPStream)
			{
				case 0:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_1_2;break;
				case 1:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_2_3;break;
				case 2:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_3_4;break;
				case 3:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_5_6;break;
				case 4:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_7_8;break;
				default:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_NOT_SET; break;
			}
			switch (terrestialNIT.CoderateLPStream)
			{
				case 0:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_1_2;break;
				case 1:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_2_3;break;
				case 2:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_3_4;break;
				case 3:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_5_6;break;
				case 4:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_7_8;break;
				default:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_NOT_SET; break;
			}
			terrestialNIT.GuardInterval=(b[8]>>3) & 3;
			// 0 - 1/32
			// 1 - 1/16
			// 2 - 1/8
			// 3 - 1/4
			//
			switch (terrestialNIT.GuardInterval)
			{
				case 0: terrestialNIT.GuardInterval=BDA_GUARD_1_32;break;
				case 1: terrestialNIT.GuardInterval=BDA_GUARD_1_16;break;
				case 2: terrestialNIT.GuardInterval=BDA_GUARD_1_8;break;
				case 3: terrestialNIT.GuardInterval=BDA_GUARD_1_4;break;
				default:terrestialNIT.GuardInterval=BDA_GUARD_NOT_SET;break;
			}
			terrestialNIT.TransmissionMode=(b[8]>>1) & 3;
			// 0 - 2k Mode
			// 1 - 8k Mode
			if (terrestialNIT.TransmissionMode==0) terrestialNIT.TransmissionMode=BDA_XMIT_MODE_2K;
			else if (terrestialNIT.TransmissionMode==1) terrestialNIT.TransmissionMode=BDA_XMIT_MODE_8K;
			else terrestialNIT.TransmissionMode=BDA_XMIT_MODE_NOT_SET;
			
			terrestialNIT.OtherFrequencyFlag=(b[8] & 3);
			// 0 - no other frequency in use
			bool alreadyAdded=false;
			for (int i=0; i < m_nit.terrestialNIT.size();++i)
			{
				NITTerrestrialDescriptor& nit=m_nit.terrestialNIT[i];
				if (nit.CentreFrequency==terrestialNIT.CentreFrequency)
				{
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				Dump("NIT: terrestial frequency=%d bandwidth=%d other freqs:%d", terrestialNIT.CentreFrequency,terrestialNIT.Bandwidth,terrestialNIT.OtherFrequencyFlag);
				m_nit.terrestialNIT.push_back(terrestialNIT);
			}
		}

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DVB_GetTerrestrialDelivSys()");
	}	
}		


void Sections::DVB_GetCableDelivSys(byte* b, int maxLen)
{
	try
	{
		if(b[0]==0x44 && maxLen>=13)
		{
			int descriptor_tag = b[0];
			int descriptor_length = b[1];
			if (descriptor_length>13) 
			{
				Log("DVB_GetCableDelivSys() desclen:%d", descriptor_length);
				return;
			}
			NITCableDescriptor cableNIT;
			cableNIT.Frequency = (10000000* ((b[2]>>4)&0xf));
			cableNIT.Frequency+= (1000000*  ((b[2]&0xf)));
			cableNIT.Frequency+= (100000*   ((b[3]>>4)&0xf));
			cableNIT.Frequency+= (10000*    ((b[3]&0xf)));
			cableNIT.Frequency+= (1000*     ((b[4]>>4)&0xf));
			cableNIT.Frequency+= (100*      ((b[4]&0xf)));
			cableNIT.Frequency+= ( 10*      ((b[5]>>4)&0xf));
			cableNIT.Frequency+= (b[5]&0xf);
			//
			cableNIT.FECOuter = (b[7] & 0xF);
			// fec-outer
			// 0- not defined
			// 1- no outer FEC coding
			// 2- RS(204/188)
			// other reserved
			switch (cableNIT.FECOuter)
			{
				case 0:cableNIT.FECOuter=BDA_FEC_METHOD_NOT_SET;break;
				case 1:cableNIT.FECOuter=BDA_FEC_METHOD_NOT_DEFINED;break;
				case 2:cableNIT.FECOuter=BDA_FEC_RS_204_188;break;
				default:cableNIT.FECOuter=BDA_FEC_METHOD_NOT_SET;break;
			}
			cableNIT.Modulation = b[8];
			// modulation
			// 0x00 not defined
			// 0x01 16-QAM
			// 0x02 32-QAM
			// 0x03 64-QAM
			// 0x04 128-QAM
			// 0x05 256-QAM
			switch(cableNIT.Modulation)
			{
				case 0: cableNIT.Modulation=BDA_MOD_NOT_DEFINED; break;
				case 1: cableNIT.Modulation=BDA_MOD_16QAM; break;
				case 2: cableNIT.Modulation=BDA_MOD_32QAM; break;
				case 3: cableNIT.Modulation=BDA_MOD_64QAM; break;
				case 4: cableNIT.Modulation=BDA_MOD_128QAM; break;
				case 5: cableNIT.Modulation=BDA_MOD_256QAM; break;
				default: cableNIT.Modulation=BDA_MOD_NOT_SET; break;
			}
			
			cableNIT.Symbolrate = (1000000* ((b[9]>>4)&0xf));
			cableNIT.Symbolrate+= (100000*  ((b[9]&0xf)));
			cableNIT.Symbolrate+= (10000*   ((b[10]>>4)&0xf));
			cableNIT.Symbolrate+= (1000*    ((b[10]&0xf)));
			cableNIT.Symbolrate+= (100*     ((b[11]>>4)&0xf));
			cableNIT.Symbolrate+= (10*      ((b[11]&0xf)));
			cableNIT.Symbolrate+= (         ((b[12]>>4)&0xf));
			
			

			cableNIT.FECInner = (b[12] & 0xF);
			// fec inner
			// 0- not defined
			// 1- 1/2 conv. code rate
			// 2- 2/3 conv. code rate
			// 3- 3/4 conv. code rate
			// 4- 5/6 conv. code rate
			// 5- 7/8 conv. code rate
			// 6- 8/9 conv. code rate
			// 15- No conv. coding
			switch (cableNIT.FECInner)
			{
				case 0:cableNIT.FECInner=BDA_BCC_RATE_NOT_DEFINED;break;
				case 1:cableNIT.FECInner=BDA_BCC_RATE_1_2;break;
				case 2:cableNIT.FECInner=BDA_BCC_RATE_2_3;break;
				case 3:cableNIT.FECInner=BDA_BCC_RATE_3_4;break;
				case 4:cableNIT.FECInner=BDA_BCC_RATE_5_6;break;
				case 5:cableNIT.FECInner=BDA_BCC_RATE_7_8;break;
				case 6:cableNIT.FECInner=BDA_BCC_RATE_NOT_DEFINED;break;
				case 15:cableNIT.FECInner=BDA_BCC_RATE_NOT_DEFINED;break;
				default:cableNIT.FECInner=BDA_BCC_RATE_NOT_SET;break;
			}
			bool alreadyAdded=false;
			for (int i=0; i < m_nit.cableNIT.size();++i)
			{
				NITCableDescriptor& nit=m_nit.cableNIT[i];
				if (nit.Frequency==cableNIT.Frequency)
				{
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				m_nit.cableNIT.push_back(cableNIT);
				//Log("NIT: network:%s", cableNIT.NetworkName);
				Dump("cable NIT: frequency:%d modulation:%d symbolrate:%d", cableNIT.Frequency, cableNIT.Modulation, cableNIT.Symbolrate);
			}
		}

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DVB_GetCableDelivSys()");
	}	
}

HRESULT Sections::GetLCN(WORD channelIndex,WORD* networkId, WORD* transportId, WORD* serviceID, WORD* LCN)
{
	try
	{
		*LCN=0;
		*networkId=0;
		*transportId=0;
		*serviceID=0;
		if (channelIndex >=m_nit.lcnNIT.size())
		{
			return S_OK;
		}
		
		int counter=0;
		vector<NITLCN>::iterator it;
		it=m_nit.lcnNIT.begin();
		while (it != m_nit.lcnNIT.end())
		{
			if (counter==channelIndex)
			{
				NITLCN& lcn = *it;
				*networkId=lcn.network_id;
				*transportId=lcn.transport_id;
				*serviceID=lcn.service_id;
				*LCN=lcn.LCN;
				
				return S_OK;
			}
			++it;
			counter++;
		}
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::GetLCN()");
	}	
	return S_OK;
}
