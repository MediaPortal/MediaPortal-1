/*
	MediaPortal TS-SourceFilter by Agree

	
*/


#include <streams.h>
#include "Section.h"
#include "mpsa.h"
#include "crc.h"
//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

Sections::Sections()
{
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
int Sections::decodePMT(BYTE *buf,ChannelInfo *ch)
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
	int len1 = section_length - 4;
	int len2 = program_info_length;
	int pointer = 12;
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
int Sections::decodeSDT(BYTE *buf,ChannelInfo ch[],int channels)
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
void Sections::decodePAT(BYTE *pData,ChannelInfo chInfo[],int *channelCount)
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
		if(ch.ProgrammPMTPID>0x12)
		{
			chInfo[pmtcount]=ch;
			pmtcount++;
		}
		if(i>254)
			break;
	}
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
		if(c=='Ü')
		{
			int a=0;
		}
		if (c >= 0x80 & c <= 0x9F)
		{
			goto cont;
		}
		if (i==0 & c < 0x20)
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
				
		if (c == 0x84)
		{
			text[num] = '\r';
			num++;
			goto cont;
		}
				
		if (c < 0x20)
		{
			goto cont;
		}
				
		text[num] = c;
		num++;
cont:
		l1 -= 1;
		i += 1;
	}while (!(l1 <= 0));

}

