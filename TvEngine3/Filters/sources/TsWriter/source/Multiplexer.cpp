/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#include <windows.h>
#include "multiplexer.h"
void LogDebug(const char *fmt, ...) ;

CMultiplexer::CMultiplexer()
{
	m_pCallback=NULL;
	
	m_videoPacketCounter=0;
}

CMultiplexer::~CMultiplexer()
{
}

void CMultiplexer::SetFileWriterCallBack(IFileWriter* callback)
{
	m_pCallback=callback;
}
void CMultiplexer::Reset()
{
	m_videoPacketCounter=0;
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		delete decoder;
	}
	m_pesDecoders.clear();
}

void CMultiplexer::SetPcrPid(int pcrPid)
{
	LogDebug("mux pcr pid:%x", pcrPid);
	m_pcrDecoder.SetPcrPid(pcrPid);
}

int CMultiplexer::GetPcrPid()
{
	return m_pcrDecoder.GetPcrPid();
}

void CMultiplexer::AddPesStream(int pid)
{
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->GetPid()==pid) return;
	}
	
	LogDebug("mux pes pid:%x", pid);
	CPesDecoder* decoder = new CPesDecoder();
	decoder->SetPid(pid);
	m_pesDecoders.push_back(decoder);
}


void CMultiplexer::OnTsPacket(byte* tsPacket)
{
	m_pcrDecoder.OnTsPacket(tsPacket);
  
	if (m_pcrDecoder.PcrHigh()==0 && m_pcrDecoder.PcrLow()==0) return;
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->OnTsPacket(tsPacket))
		{
			byte* pesPacket = decoder->GetPesPacket();
			int   pesLength = decoder->GetPesPacketLength();

			
			if (decoder->GetStreamId()==0xe0)
			{
				LogDebug("stream 0xe0 %d",m_videoPacketCounter);
				if ((m_videoPacketCounter%30)==0)
				{	
					LogDebug("write pack header %x %x",m_pcrDecoder.PcrHigh(),m_pcrDecoder.PcrLow());
					byte buffer[20];
					int packLen=WritePackHeader(m_pcrDecoder.PcrHigh(), m_pcrDecoder.PcrLow(),4000000,buffer);
					if (m_pCallback!=NULL)
					{
						m_pCallback->Write(buffer,packLen);
					}

					//int systemLen=WriteSystemHeader(buffer);
					//fwrite(buffer,1,systemLen,m_fp);
				}
				m_videoPacketCounter++;
				LogDebug("write video pid %x peslen:%x",decoder->GetPid(), pesLength);
				if (m_pCallback!=NULL)
				{
					m_pCallback->Write(pesPacket,pesLength);
				}
			}
			else 
			{
				LogDebug("stream 0xc0 %d",m_videoPacketCounter);
				if (m_videoPacketCounter>0)
				{
					LogDebug("write audio pid %x peslen:%x",decoder->GetPid(), pesLength);
					if (m_pCallback!=NULL)
					{
						m_pCallback->Write(pesPacket,pesLength);
					}
				}
			}
		}
	}
}

int CMultiplexer::WritePackHeader(__int64 pcrHi, int pcrLow, unsigned int muxRate,byte* pBuffer)
{
	//pack header
	pBuffer[0]=0;
	pBuffer[1]=0;
	pBuffer[2]=1;
	pBuffer[3]=0xba;
	
	// 4             5     6         7      8       9        10        11       12       13
	//76543210  76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210
	//01PPPMPP  PPPPPPPP PPPPPMPP PPPPPPPP PPPPPMEE EEEEEEEM RRRRRRRR RRRRRRRR RRRRRRMM VVVVVSSS
	//4..9 = pcr 33bits / 9 bits
	pBuffer[4]= ((pcrHi >> 28)  & 0x3) + 4 + (((pcrHi >> 30) & 7) << 3)+ 0x40;
	pBuffer[5]= (pcrHi >> 20)  & 0xff;
	pBuffer[6]= (pcrHi >> 13) & 0x3 + 4 + ( ( (pcrHi >> 15) & 0x1f) <<3);
	pBuffer[7]= (pcrHi >> 5) & 0xff;
	pBuffer[8]= ((pcrLow >> 7) & 3) + 4 + ((pcrHi & 0x1f)<<3);
	pBuffer[9]= ((pcrLow & 0x7f) << 1) +1 ;

	//10..12 = mux rate
	pBuffer[10]= (muxRate >> 14) & 0xff;
	pBuffer[11]= (muxRate >> 6) & 0xff;
	pBuffer[12]=((muxRate & 0x3f) << 2)+3;

	//13 pack stuffing length
	pBuffer[13]=0xf8;
	return 14;
}

int CMultiplexer::WriteSystemHeader(byte* pBuffer)
{
	//system-header-start-code
	pBuffer[0]=0;							//32			0-3
	pBuffer[1]=0;
	pBuffer[2]=1;
	pBuffer[3]=0xbb;
	
	//header_length						//16			4-5
	int headerLength=18-6;

	int marker_bit=1;
	int rate_bound=8000000; //value of the mux_rate field coded in any pack Program Stream
	int audio_bound=1;//integer from 0 to 32 greater than or equal to the maximum number of audio streams in the 
	int fixed_flag =0;//0=VBR, 1=CBR
	int CSPS_flag=0;  //bit flag.  If its value is set to '1' the ISO/IEC 13818 Program Stream meets the constraints defined in clause 2.5.7.9
	int system_audio_lock_flag=0;//The system_audio_lock_flag is a 1 bit flag indicating that there is a specified, constant rational relationship between the audio sampling rate and the system clock frequency in the system target decoderThe system_audio_lock_flag is a 1 bit flag indicating that there is a specified, constant rational relationship between the audio sampling rate and the system clock frequency in the system target decoder
	int system_video_lock_flag=0;//The system_video_lock_flag is a 1 bit flag indicating that there is a specified, constant rational relationship between the video picture rate and the system clock frequency in the system target decoder
	int video_bound=1; //The video_bound is an integer in the inclusive range from 0 to 16 greater than or equal to the maximum number of ISO/IEC 13818-2 streams 
	int reserved_byte =0xff;
	int buffer_bound_scale_audio=0;
	int buffer_bound_scale_video=1;
	int buffer_size_bound_audio=100;
	int buffer_size_bound_video=100;
	
	pBuffer[4]= ((headerLength>>8)&0xff);
	pBuffer[5]= (headerLength &0xff);
	pBuffer[6]= (marker_bit<<7) + ((rate_bound>>15)&0x7f);
	pBuffer[7]= (rate_bound>>7)&0xff;
	pBuffer[8]= marker_bit + ((rate_bound&0x7f)<<1);
	pBuffer[9]= ((audio_bound&0x3f)<<2) + (fixed_flag<<1) +CSPS_flag;
	pBuffer[10]=(system_audio_lock_flag<<7) + (system_video_lock_flag<<7) + (video_bound&0x3f);
	pBuffer[11]=reserved_byte;

	pBuffer[12]=0xc0;
	pBuffer[13]=(3<<6) + ((buffer_bound_scale_audio&1)<<5) + ((buffer_size_bound_audio>>8)&0x1f);
	pBuffer[14]=(buffer_size_bound_audio & 0x7f);

	pBuffer[15]=0xe0;
	pBuffer[16]=(3<<6) + ((buffer_bound_scale_video&1)<<5) + ((buffer_size_bound_video>>8)&0x1f);
	pBuffer[17]=(buffer_size_bound_video & 0x7f);



	// 6           7      8         9         10         11
	//76543210 76543210 76543210	76543210	76543210	76543210
	//MRRRRRRR RRRRRRRR RRRRRRRM	AAAAAAFC	AVMVVVVV	RRRRRRRR
	//marker bit							1					6		0x80
	//rate_bound							22
	//marker bit							1
	//audio_bound							6
	//fixed flag							1
	//CSPS_flag								1
	//system_audio_lock_flag	1
	//system_video_lock_flag	1
	//marker_bit							1
	//video_bound							5
	//reserved_byte						8
	//while (nextbits () == '1') 
	//{
	//	0					1					2
	//	76543210 76543210 76543210 
	//	SSSSSSSS 11BSSSSS SSSSSSSS 
	//	stream_id										8						0
	//	'11'												2						1
	//	P-STD_buffer_bound_scale		1
	//	P-STD_buffer_size_bound			13					2
	//}
	//P-STD_buffer_bound_scale -- 0=audio,1=video	
	//P-STD_buffer_size_bound 
	//P-STD_buffer_size_bound measures the buffer size bound in units of 128 bytes. If 
	//If P-STD_buffer_bound_scale has the value '0' then P-STD_buffer_size_bound measures the buffer size bound in units of 128 bytes. 
	//if P-STD_buffer_bound_scale has the value '1' then P-STD_buffer_size_bound measures the buffer size bound in units of 1024 bytes.  Thus:


	return 18;
}
