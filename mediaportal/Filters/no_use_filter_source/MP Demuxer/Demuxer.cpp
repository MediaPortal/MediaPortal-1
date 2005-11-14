// TS Mux Splitter Copyright by Agree / Team MediaPortal 2005
//
//
//

#include "stdafx.h"
#include "Demuxer.h"
#include <string>

#include "TSMuxSplitter.h"

class SplitterOutputPin;

	// audio data
	const ULONG AudioFrequencies[3][4]={{22050,24000,16000,0},{44100,48000,32000,0},{11025,12000,8000,0}};

	const ULONG AudioBitrates[3][3][16] = {
		{{-1,8000,16000,24000,32000,40000,48000,56000,64000,80000,96000,112000,128000,144000,160000,0 },		
		{-1,8000,16000,24000,32000,40000,48000,56000,64000,80000,96000,112000,128000,144000,160000,0 },		
		{-1,32000,48000,56000,64000,80000,96000,112000,128000,144000,160000,176000,192000,224000,256000,0 }	}
		,
		{{-1,32000,40000,48000,56000,64000,80000,96000,112000,128000,160000,192000,224000,256000,320000, 0 },	
		{-1,32000,48000,56000,64000,80000,96000,112000,128000,160000,192000,224000,256000,320000,384000, 0 },	
		{-1,32000,64000,96000,128000,160000,192000,224000,256000,288000,320000,352000,384000,416000,448000,0 }}
		,
		{{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 8000, 12000, 16000, 20000, 24000, 32000, 40000,48000, 560000, 64000, 80000, 96000, 112000, 128000, 0}
	}};

	const double AudioTimes[4] = { 0.0,103680000.0,103680000.0,34560000.0 };
	// ac3
	const int AC3FrequencyIndex[4] = 
	{ 
		48000,44100,32000,0 
	};
	
	const int AC3BitrateIndex[32] =  
	{ 
		32000,40000,48000,56000,64000,80000,96000,
		112000,128000,160000,192000,224000,256000,
		320000,384000,448000,512000,576000,640000,
		0,0,0,0,0,0,0,0,0,0,0,0,0
	};
	
	const int AC3SizeTable[3][19] = 
	{
		{ 128,160,192,224,256,320,384,448,512,640,768,896,1024,1280,1536,1792,2080,2304,2560 },
		{ 138,174,208,242,278,348,416,486,556,696,834,974,1114,1392,1670,1950,2228,2506,2786 },
		{ 192,240,288,336,384,480,576,672,768,960,1152,1344,1536,1920,2304,2688,3120,3456,3840 }
	};

	const int AC3Channels[8] = 
	{
		2,1,2,3,3,4,4,5
	};
	// video
	static const int PICTURE_START_CODE    = 0x00000100;
	static const int USER_DATA_START_CODE  = 0x000001B2;
	static const int SEQUENCE_HEADER_CODE  = 0x000001B3;
	static const int EXTENSION_START_CODE  = 0x000001B5;
	static const int SEQUENCE_END_CODE     = 0x000001B7;
	static const int GROUP_START_CODE      = 0x000001B8;

	static const double MPEG2FrameRate[] = {0.0,23.976,24.0,25.0,29.97,30.0,50.0,59.94,60};
	//static const string AspectRatio[] ={"forbidden","1:1","4:3","16:9","2.21:1","reserved","reserved","reserved","reserved","reserved"};
	static const int I_FRAME = 0x01;
	static const int P_FRAME = 0x02;
	static const int B_FRAME = 0x03;
	static const int D_FRAME = 0x01;

//
HRESULT Demux::GetTSHeader(BYTE *data,TSHeader *header)
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
HRESULT Demux::GetPESHeader(BYTE *data,PESHeader *header)
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
HRESULT Demux::ParseAudioHeader(BYTE *data,AudioHeader *head)
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
//static 
LONGLONG Demux::GetPTS(const BYTE* data)
{
 	ULONGLONG ptsVal;
	bool ptsFlag=false;
	ptsVal= 0xFFFFFFFFL & ( (6&data[0])<<29 | (255&data[1])<<22 | (254&data[2])<<14 | (255&data[3])<<7 | (((254&data[4])>>1)& 0x7F));
	if ((ptsVal & 0xFF000000L)==0xFF000000L) 
		ptsFlag=true;
	if (ptsFlag && ptsVal<0xF0000000L) 
		ptsVal |=0x100000000L;
    return ptsVal;
}


Demux::Demux()
{
	m_videoPointer=0;
	m_audioPointer=0;
	m_videoPID=0;
	m_audioPID=0;
	ZeroMemory(&m_VideoHeader,sizeof(VideoHeader));
	ZeroMemory(&m_AudioHeader,sizeof(AudioHeader));
}

HRESULT Demux::Process(const BYTE* pData, long cBytes,void *calcFunc)
{
	if(pData==NULL)
		return S_FALSE;

	TSHeader tsHeader;
	PESHeader pesHeader;
	const DWORD pesHeaderOffset=6;
	const DWORD pesPTSOffset=9;
	DWORD pesOffset=0;
	bool found=false;
	DWORD len=cBytes+m_remainingBytes;
	ULONGLONG counter=0;
	DWORD len1=(len/188)*188;
	CopyMemory(m_ParseBuffer+m_remainingBytes,pData,cBytes);
	DWORD offset=0;

	for(;offset<len1;offset+=188)
	{
		counter=offset;
		if(m_ParseBuffer[offset]!=0x47)
		{
			int a=0;
		}
		GetTSHeader(m_ParseBuffer+offset,&tsHeader);
		if(tsHeader.SyncByte==0x47)
		{
				if(tsHeader.Pid==0x1FFF)
					continue;
				if(tsHeader.TransportError)
					continue;// no packet
				if(tsHeader.AdaptionControl==2 || tsHeader.AdaptionControl==0)
					continue;
				pesOffset=4+(tsHeader.AdaptionControl==3?1+m_ParseBuffer[offset+4]:0);

				if(tsHeader.PayloadUnitStart)
				{
					
					LONGLONG tmpPTS=-1;
					GetPESHeader(m_ParseBuffer+offset+pesOffset+pesHeaderOffset,&pesHeader);
					if(pesHeader.Reserved==0x02 && pesHeader.PTSFlags)
					{
						tmpPTS=GetPTS(m_ParseBuffer+offset+pesOffset+pesPTSOffset);
						found=true;
					}

					DWORD streamID=m_ParseBuffer[offset+pesOffset+3];
					DWORD headerLen=pesHeader.PESHeaderDataLength+9;
					DWORD streamSync=(m_ParseBuffer[offset+pesOffset]<<16)+(m_ParseBuffer[offset+pesOffset+1]<<8)+m_ParseBuffer[offset+pesOffset+2];

					if(tsHeader.Pid==m_audioPID)
					{
						pesOffset+=headerLen;
						if(m_audioPointer>0)
						{
							
							HRESULT hr;
							hr=((TSMuxSplitter*)calcFunc)->ProcessPacket(m_AudioStreamBuffer,m_audioPointer,m_currentAudioPTS,true);
							if(hr==2|| hr==S_FALSE)
								return S_OK;
							m_currentAudioPTS=tmpPTS;
							m_audioPointer=0;
						}
					}
					if(tsHeader.Pid==m_videoPID)
					{
						pesOffset+=headerLen;
						if(m_videoPointer>0)
						{
							HRESULT hr;
							hr=((TSMuxSplitter*)calcFunc)->ProcessPacket(m_VideoStreamBuffer,m_videoPointer,m_currentVideoPTS,false);
							if(hr==2 || hr==S_FALSE)
								return S_OK;
							m_videoPointer=0;
							m_currentVideoPTS=tmpPTS;
						}
					}
				}
					// copy data
				if(m_audioPID==tsHeader.Pid)
				{
					CopyMemory(m_AudioStreamBuffer+m_audioPointer,m_ParseBuffer+offset+pesOffset,188-pesOffset);
					m_audioPointer+=188-pesOffset;
					if(m_audioPointer>=65000)
					{
						((TSMuxSplitter*)calcFunc)->ProcessPacket(m_AudioStreamBuffer,m_audioPointer,m_currentAudioPTS,true);
						m_currentAudioPTS=0;
						m_audioPointer=0;
					}
				}
				if(m_videoPID==tsHeader.Pid)
				{
					CopyMemory(m_VideoStreamBuffer+m_videoPointer,m_ParseBuffer+offset+pesOffset,188-pesOffset);
					m_videoPointer+=188-pesOffset;
					if(m_videoPointer>=60000)
					{
						((TSMuxSplitter*)calcFunc)->ProcessPacket(m_VideoStreamBuffer,m_videoPointer,m_currentVideoPTS,false);
						m_currentVideoPTS=0;
						m_videoPointer=0;
					}
				}
				
		}
			
	}
	// copy rest data
	counter+=188;
	CopyMemory(m_ParseBuffer,m_ParseBuffer+counter,len-counter);
	m_remainingBytes=len-counter;
    return S_OK;
}
void Demux::PTSToPTSTime(LONGLONG pts,PTSTime* ptsTime)
{
	ULONG  _90khz = (ULONG)(pts/90);
	ptsTime->h=(_90khz/(1000*60*60));
	ptsTime->m=(_90khz/(1000*60))-(ptsTime->h*60);
	ptsTime->s=(_90khz/1000)-(ptsTime->h*3600)-(ptsTime->m*60);
	ptsTime->u=_90khz-(ptsTime->h*1000*60*60)-(ptsTime->m*1000*60)-(ptsTime->s*1000);

}
bool Demux::CheckTSFile(IAsyncReader* pReader)
{
    // copy up buffer to free up processed space
    LONGLONG pos=0;
    long cThis = ParseBufferSize;
	TSHeader tsHeader;
	PESHeader pesHeader;
	const DWORD pesHeaderOffset=6;
	const DWORD pesPTSOffset=9;
	DWORD pesOffset=0;
    // validate read length against file size
    m_firstPTS=0;
	m_lastPTS=0;
	LONGLONG tmpPTS=0;
	bool searchEnd=false;
	m_videoPID=0;
	m_audioPID=0;
	m_isAC3Audio=false;
	DWORD tmpAC3Pid=0;
	m_StartOffset=0;
	ZeroMemory(&m_VideoHeader,sizeof(VideoHeader));
	ZeroMemory(&m_AudioHeader,sizeof(AudioHeader));
	do
	{
		LONGLONG total, actual;
		pReader->Length(&total, &actual);
		if ((total - pos) < cThis)
		{
			cThis = long(total - pos);
		}
		if (cThis <= 0)
		{
			break;
		}
		cThis=(cThis/188)*188;

		HRESULT hr = pReader->SyncRead(pos, cThis, &m_ParseBuffer[0]);
		if (hr != S_OK)
		{
			break;
		}
		DWORD offset=0;

		if(pos==0 && m_ParseBuffer[0]!=0x47)
		{
			do
			{
				offset+=1;cThis-=1;
			}while(m_ParseBuffer[offset]!=0x47 && m_ParseBuffer[offset+188]!=0x47);
			m_StartOffset=offset;
		}
		if(m_VideoHeader.Done==false)
			ParseVideoHeader(m_ParseBuffer,cThis,&m_VideoHeader);
		for(;offset<cThis;offset+=188)
		{
			GetTSHeader(m_ParseBuffer+offset,&tsHeader);
			if(tsHeader.SyncByte==0x47)
			{
				if(tsHeader.TransportError)
					continue;// no packet
				if(tsHeader.AdaptionControl==2 || tsHeader.AdaptionControl==0)
					continue;
				pesOffset=4+(tsHeader.AdaptionControl==3?1+m_ParseBuffer[offset+4]:0);

				if(tsHeader.PayloadUnitStart)
				{
					DWORD streamSync=(m_ParseBuffer[offset+pesOffset]<<16)+(m_ParseBuffer[offset+pesOffset+1]<<8)+m_ParseBuffer[offset+pesOffset+2];
					DWORD streamID=m_ParseBuffer[offset+pesOffset+3];
					DWORD packLen=(m_ParseBuffer[offset+pesOffset+4]<<8)+m_ParseBuffer[offset+pesOffset+5];
					GetPESHeader(m_ParseBuffer+offset+pesOffset+pesHeaderOffset,&pesHeader);

					if(streamSync==0x01 && streamID==0xBD && m_isAC3Audio==false)
					{
					
						DWORD subStream=0;
						DWORD offset1=pesHeader.PESHeaderDataLength+9;
						bool teleText=false;
						if(offset1<6+packLen)
						{
							subStream=m_ParseBuffer[offset+pesOffset+offset1];
							teleText=(((subStream>>4) & 0x0F)>0?true:false) && (pesHeader.PESHeaderDataLength==0x24);
                        }
						if(teleText==false)
						{
							m_firstPTS=0;
							m_audioPID=tsHeader.Pid;
							m_isAC3Audio=true;
							ParseAC3Header(m_ParseBuffer+offset+pesOffset+offset1,&m_AudioHeader);

						}
					}

					if(streamSync==0x01 && (streamID>=0xE0 && streamID<=0xEF) && m_videoPID==0)
					{
						m_videoPID=tsHeader.Pid;
					}
					if(streamSync==0x01 && (streamID>=0xC0 && streamID<=0xCF) && m_audioPID==0)
					{
						m_audioPID=tsHeader.Pid;
						ParseAudioHeader(m_ParseBuffer+offset+pesOffset+pesHeader.PESHeaderDataLength+9,&m_AudioHeader);
					}

					if(pesHeader.Reserved==0x02 && pesHeader.PTSFlags)
					{
						if(m_audioPID>0 && tsHeader.Pid==m_audioPID)
						{
							if(m_firstPTS==0)
							{
								PTSTime time;
								m_firstPTS=GetPTS(m_ParseBuffer+offset+pesOffset+pesPTSOffset);
								PTSToPTSTime(m_firstPTS,&time);
							}
							else
								tmpPTS=GetPTS(m_ParseBuffer+offset+pesOffset+pesPTSOffset);
					
						}
					}
				}
			}
		}

		pos+=cThis;
	}while(1);

	m_lastPTS=tmpPTS;

	if(m_lastPTS==0) m_lastPTS=m_firstPTS*2;
    // update current position
    return cThis;
}

void Demux::Empty()
{
	m_videoPointer=0;
	m_audioPointer=0;
	m_remainingBytes=0;
}

//
HRESULT Demux::ParseVideoHeader(BYTE *videoPacket,ULONG length,VideoHeader *vh)
{
	VideoHeader header;
	ZeroMemory(&header,sizeof(VideoHeader));
	short frameRate=0;

	for(DWORD t=0;t<length;t++)
	{
		DWORD val=(videoPacket[t]<<24)|(videoPacket[t+1]<<16)|(videoPacket[t+2]<<8)|videoPacket[t+3];
		if(val==SEQUENCE_HEADER_CODE)
		{
				// frame rate
				frameRate=(videoPacket[t+7] & 0x0F);
				header.vOrgFrameRate=MPEG2FrameRate[frameRate];
				if(frameRate>0)
					header.vFrameRate=(1/MPEG2FrameRate[frameRate])*10000000;
				
				// picture size
				header.vWidth = (videoPacket[t+4] << 4)+((videoPacket[t+5] & 0xF0) >> 4);
				header.vHeight = ((videoPacket[t+5] & 0x0F) << 8)+videoPacket[t+6];

				// aspect ratio
				header.vAspectRatioValue=(videoPacket[t+7]>>4) & 0x0F;
				switch(header.vAspectRatioValue)
				{
				case 0:
					strcpy(header.vAspectRatio,TEXT("Forbidden"));
					break;
				case 1:
					strcpy(header.vAspectRatio,TEXT("1:1"));
					break;
				case 2:
					strcpy(header.vAspectRatio,TEXT("4:3"));
					break;
				case 3:
					strcpy(header.vAspectRatio,TEXT("16:9"));
					break;
				case 4:
					strcpy(header.vAspectRatio,TEXT("2.21:1"));
					break;
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
					strcpy(header.vAspectRatio,TEXT("reserved"));
					break;

				}

				
				// bitrate
				header.vBitrate=(videoPacket[t+8] << 10)+(videoPacket[t+9] << 2)+(videoPacket[t+10] >> 6);
				header.vBitrate=((header.vBitrate==0x3FFFF)?0:header.vBitrate*400);
				header.Done=true;
				break;
		}
	}
	if(header.Done)
		CopyMemory(vh,&header,sizeof(VideoHeader));
	return S_OK;
}
HRESULT Demux::ParseAC3Header(BYTE *data,AudioHeader *header)
{
	AudioHeader ah;
	ZeroMemory(&ah,sizeof(AudioHeader));
	DWORD offset=0;
	if ( (0xFF & data[offset]) != 0xB || (0xFF & data[offset+1]) != 0x77 ) 
		return -1;
	
	DWORD ID = 0;
	DWORD Emphasis = 0;
	DWORD Private_bit = 0;

	DWORD Protection_bit = 0 ^ 1;
	DWORD Sampling_frequency;
	DWORD Bitrate;

	if ( (Sampling_frequency = AC3FrequencyIndex[3 & data[offset+4]>>6]) < 1) 
		return -4;

	if ( (Bitrate = AC3BitrateIndex[0x1F & data[offset+4]>>1]) < 1) 
		return -3;
	
	DWORD Layer = 7 & data[offset+5];       //bsmod
	DWORD Padding_bit = 1 & data[offset+4];
	DWORD Mode = 7 & data[offset+6]>>5;
	DWORD Mode_extension = 0;
	
	int mode = (0xFF & data[offset+6])<<8 | (0xFF & data[offset+7]);
	int skip=0;

	if ( (Mode & 1) > 0 && Mode != 1)  // cmix
	{
		Emphasis = 1 + (3 & data[offset+6]>>3);
		skip++;
	}

	if ( (Mode & 4) > 0) //surmix
	{
		Private_bit = 1 + (3 & data[offset+6]>>(skip > 0 ? 1 : 3));
		skip++;
	}

	if ( Mode == 2 )
	{
		Mode_extension |= 6 & mode>>(10 - (2 * skip));  //DS
		skip++;
	}
	
	switch (skip)
	{  //lfe
		case 0:
		        Mode_extension |= 1 & mode>>12;
			break;
		case 1:
		        Mode_extension |= 1 & mode>>10;
			break;
		case 2:
		        Mode_extension |= 1 & mode>>8;
			break;
		case 3:
		        Mode_extension |= 1 & mode>>6;
	}
	
	DWORD Channel =AC3Channels[Mode] + (1 & Mode_extension);
	DWORD Copyright = 0;
	DWORD Original = 0;
	DWORD Time_length = 138240000.0 / Sampling_frequency;
	DWORD Size_base;
	DWORD Size = (Size_base = AC3SizeTable[3 & data[offset+4]>>6][0x1F & data[offset+4]>>1]) + Padding_bit * 2;
	// 
	ah.Bitrate=Bitrate;
	ah.Channel=Channel;
	ah.Copyright=Copyright;
	ah.Emphasis=Emphasis;
	ah.Size=Size;
	ah.SizeBase=Size_base;
	ah.Original=Original;
	ah.TimeLength=Time_length;
	ah.ModeExtension=Mode_extension;
	ah.Mode=Mode;
	ah.SamplingFreq=Sampling_frequency;
	ah.Layer=Layer;
	ah.ProtectionBit=Protection_bit;
	CopyMemory(header,&ah,sizeof(AudioHeader));

	return 1;
}