/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#pragma warning(disable : 4995)
#include <windows.h>
#include "multiplexer.h"
void LogDebug(const char *fmt, ...) ;

#define MAX_PES_BUFFER_LENGTH 0x10000
#define MAX_PES_PACKET_LENGTH 0x7ec
#define PES_HEADER_LENGTH     6
#define PACK_HEADER_LENGTH    14


CMultiplexer::CMultiplexer()
{
  m_pesBuffer = new byte[MAX_PES_BUFFER_LENGTH];
	m_pCallback=NULL;
	m_pcrPid=-1;
  Reset();
}

CMultiplexer::~CMultiplexer()
{
  delete [] m_pesBuffer;
  Reset();
}

void CMultiplexer::SetFileWriterCallBack(IFileWriter* callback)
{
	m_pCallback=callback;
}

void CMultiplexer::Reset()
{
//	LogDebug("mux: reset");
	m_videoPacketCounter= 0;
  m_audioPacketCounter=0;
	
	ClearStreams();
}

void CMultiplexer::ClearStreams()
{
  //LogDebug("mux: clear streams startpcr:%x highestpcr:%x",(DWORD)m_startPcr,(DWORD)m_highestPcr);
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		delete decoder;
	}
	m_pesDecoders.clear();
	m_pcrPid=-1;
	m_adaptionField.Pcr.Reset();
}

void CMultiplexer::SetPcrPid(int pcrPid)
{
//	LogDebug("mux: set pcr pid:%x", pcrPid);
	m_pcrPid=pcrPid;
}

int CMultiplexer::GetPcrPid()
{
	return m_pcrPid;
}

void CMultiplexer::RemovePesStream(int pid)
{
//	LogDebug("mux: remove pes pid:%x", pid);
	ivecPesDecoders it;
	it=m_pesDecoders.begin(); 
	while (it != m_pesDecoders.end())
	{
		CPesDecoder* decoder=*it;
		if (decoder->GetPid()== pid) 
		{
			delete[] decoder;
			m_pesDecoders.erase(it);
			return;
		}
		++it;
	}
}

void CMultiplexer::AddPesStream(int pid, bool isAudio, bool isVideo)
{
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->GetPid()== pid) return;
	}
	
//	LogDebug("mux: add pes pid:%x", pid);
	int audioStreamId=0xc0;
	int videoStreamId=0xe0;

	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->IsAudio()) audioStreamId++;
		if (decoder->IsVideo()) videoStreamId++;
	}

	CPesDecoder* decoder = new CPesDecoder(this);
	decoder->SetPid(pid);
	if (isAudio)
	{
		//LogDebug("mux pid:%x audio stream id:%x", pid,audioStreamId);
		decoder->SetStreamId(audioStreamId);
	}
	else if (isVideo)
	{
		//LogDebug("mux pid:%x video stream id:%x", pid,videoStreamId);
		decoder->SetStreamId(videoStreamId);
	}
	else
	{
		//LogDebug("mux pid:%x video stream id:-1", pid);
		decoder->SetStreamId(-1);
	}
	decoder->SetMaxLength(0x7e9);
	m_pesDecoders.push_back(decoder);
	//LogDebug("mux streams:%d", m_pesDecoders.size());
}


void CMultiplexer::OnTsPacket(byte* tsPacket)
{
	m_header.Decode(tsPacket);
	if (m_header.Pid==m_pcrPid)
	{
		m_adaptionField.Decode(m_header,tsPacket);
		if (m_adaptionField.PcrFlag)
		{
			m_pcr=m_adaptionField.Pcr;
		}
	}
	if (m_pcr.PcrReferenceBase==0) return;

	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		decoder->OnTsPacket(tsPacket);
	}
}


int CMultiplexer::OnNewPesPacket(int streamId,byte* header, int headerlen,byte* pesPacket, int pesLength, bool isStart)
{
	if (pesLength<=0) return 0;
  if (m_pesDecoders.size()==1)
  {
		return SplitPesPacket(streamId,header,headerlen,pesPacket,pesLength,isStart);
  }

	//is it a video stream
	if (streamId>= 0xe0 && streamId <= 0xef)
	{
		if (m_videoPacketCounter==0)
    {
      if (m_audioPacketCounter==0) return pesLength;
      if (isStart==false) return pesLength;
   
      bool containsStart=false;
			if (pesPacket[0] == 0 && pesPacket[1] == 0 && pesPacket[2] == 1 && pesPacket[3] == 0xb3)
			{
				containsStart=true;
			}
      if (false==containsStart) return pesLength;
    }
	  m_videoPacketCounter++;
	  return SplitPesPacket(streamId,header,headerlen,pesPacket,pesLength,isStart);
	}
	else 
	{
		//audio stream (or private stream)
    if (m_audioPacketCounter==0 && isStart==false)
    {
      return pesLength;
    }
    m_audioPacketCounter++;
		return SplitPesPacket(streamId,header,headerlen,pesPacket,pesLength,isStart);
	}
  return pesLength;
}

int CMultiplexer::WritePackHeader()
{

	UINT64 pcrHi=m_pcr.PcrReferenceBase;
	UINT64 pcrLow=m_pcr.PcrReferenceExtension;
  int muxRate=(6*1024*1024)/50; //6MB/s
  byte pBuffer[0x20];
	//pack header
	pBuffer[0] = 0;
	pBuffer[1] = 0;
	pBuffer[2] = 1;
	pBuffer[3] = 0xba;
	// 4             5     6         7      8       9        10        11       12       13
	//76543210  76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210
	//01PPPMPP  PPPPPPPP PPPPPMPP PPPPPPPP PPPPPMEE EEEEEEEM RRRRRRRR RRRRRRRR RRRRRRMM VVVVVSSS	 
	//4..9 = pcr 33bits / 9 bits
	pBuffer[9]=(byte)(1  + ((pcrLow&0x7f)<<1)); pcrLow>>=7;
	pBuffer[8]=(byte)(4  +  (pcrLow&0x3) + ((pcrHi&0x1f)<<3)); pcrHi>>=5;
	pBuffer[7]=(byte)(0  +  (pcrHi&0xff)); pcrHi>>=8;
	pBuffer[6]=(byte)(4  +  (pcrHi&0x3));pcrHi>>=2; pBuffer[6]+=(byte)((pcrHi&0x1f)<<3);pcrHi>>=5;
	pBuffer[5]=(byte)(0  +  (pcrHi&0xff));pcrHi>>=8;
	pBuffer[4]=(byte)(0x44 + (pcrHi&0x3));pcrHi>>=2; pBuffer[4]+=(byte)((pcrHi&7)<<3);


	//10..12 = mux rate
	pBuffer[10] = (muxRate >> 14) & 0xff;
	pBuffer[11] = (muxRate >> 6) & 0xff;
	pBuffer[12] =((muxRate & 0x3f) << 2)+3;

	//13 pack stuffing length
	pBuffer[13] = 0xf8;
  if (m_pCallback!=NULL)
  {
	  m_pCallback->Write((byte*)pBuffer,	PACK_HEADER_LENGTH);
  }
  return PACK_HEADER_LENGTH;
}


int CMultiplexer::SplitPesPacket(int streamId,byte* header, int headerlen, byte* pesPacket, int sectionLength, bool isStart)
{ 
	if (streamId<0) return sectionLength;
  if (m_pCallback == NULL) return sectionLength;

  if (sectionLength != 0x7e9)
  {
    if (isStart)
    {

      int len=headerlen+sectionLength-6;
			memset(m_pesBuffer,0xff,0x800);
      WritePackHeader();
	    header[3]=streamId;
      header[4]=((len)>>8)&0xff;
      header[5]=((len))&0xff;
      m_pCallback->Write(header, headerlen);              //e
		  m_pCallback->Write(pesPacket, sectionLength);  

		  //write padding stream;
      int rest=0x7f2-(headerlen+sectionLength);
		  m_pesBuffer[0] = 0;
		  m_pesBuffer[1] = 0;
		  m_pesBuffer[2] = 1;
		  m_pesBuffer[3] = 0xbe;
		  m_pesBuffer[4] = ((rest-6)>>8)&0xff;
		  m_pesBuffer[5] = ((rest-6)&0xff);
		  m_pCallback->Write(m_pesBuffer, rest);
	    return sectionLength;
    }
    
    WritePackHeader();
		memset(m_pesBuffer,0xff,0x800);

		//write original header
    headerlen=9;
    int len=headerlen+sectionLength-6;
		m_pesBuffer[0] = 0;
		m_pesBuffer[1] = 0;
		m_pesBuffer[2] = 1;
		m_pesBuffer[3] = streamId;
		m_pesBuffer[4] = ((len)>>8)&0xff;
		m_pesBuffer[5] = ((len))&0xff;
		m_pesBuffer[6] = 0x81;
		m_pesBuffer[7] = 0;
		m_pesBuffer[8] = 0;
		m_pCallback->Write(m_pesBuffer, 9);
		m_pCallback->Write(pesPacket, sectionLength);


	  //write padding stream;
    int rest=0x7f2-(headerlen+sectionLength);
	  m_pesBuffer[0] = 0;
	  m_pesBuffer[1] = 0;
	  m_pesBuffer[2] = 1;
	  m_pesBuffer[3] = 0xbe;
	  m_pesBuffer[4] = ((rest-6)>>8)&0xff;
	  m_pesBuffer[5] = ((rest-6)&0xff);
	  m_pCallback->Write(m_pesBuffer, rest);
	  return sectionLength;
  }

	if (isStart)
	{
    WritePackHeader();
    int len=0x7e9-(headerlen-9);
		header[3]=streamId;
    header[4]=0x7;
    header[5]=0xec;
    m_pCallback->Write(header, headerlen);
    m_pCallback->Write(pesPacket, len);
		return len;
	}

  WritePackHeader();
  m_pesBuffer[0] = 0;
  m_pesBuffer[1] = 0;
  m_pesBuffer[2] = 1;
	m_pesBuffer[3] = streamId;
  m_pesBuffer[4] = 0x7;
  m_pesBuffer[5] = 0xec;
  m_pesBuffer[6] = 0x81;
  m_pesBuffer[7] = 0;
  m_pesBuffer[8] = 0;
  m_pCallback->Write(m_pesBuffer, 9);
  m_pCallback->Write(pesPacket, sectionLength);

	return sectionLength;
}