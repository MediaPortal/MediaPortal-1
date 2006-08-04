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
#define MAX_PES_PACKET_LENGTH 0x7ec
#define PES_HEADER_LENGTH 6
#define PACK_HEADER_LENGTH 14

CMultiplexer::CStreamBuffer::CStreamBuffer()
{
  m_ipesBufferPos=0;
  m_pesPacket = new byte[0x50000];
}
CMultiplexer::CStreamBuffer::~CStreamBuffer()
{
  delete[] m_pesPacket;
}

CMultiplexer::CMultiplexer()
{
  m_pesBuffer = new byte[0x10000];
	m_pCallback=NULL;
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
	LogDebug("mux: reset");
	m_videoPacketCounter=0;
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		delete decoder;
	}
  imapStreamBuffer im;
	for (im=m_mapStreamBuffer.begin(); im != m_mapStreamBuffer.end();++im)
	{
		CStreamBuffer* buffer=im->second;
		delete buffer;
	}
  m_mapStreamBuffer.clear();
	m_pesDecoders.clear();
	m_pcrDecoder.Reset();
}

void CMultiplexer::SetPcrPid(int pcrPid)
{
	LogDebug("mux: set pcr pid:%x", pcrPid);
	m_pcrDecoder.SetPcrPid(pcrPid);
}

int CMultiplexer::GetPcrPid()
{
	return m_pcrDecoder.GetPcrPid();
}

void CMultiplexer::RemovePesStream(int pid)
{
	LogDebug("mux: remove pes pid:%x", pid);
	ivecPesDecoders it;
	it=m_pesDecoders.begin(); 
	while (it != m_pesDecoders.end())
	{
		CPesDecoder* decoder=*it;
		if (decoder->GetPid()==pid) 
		{
			delete[] decoder;
			m_pesDecoders.erase(it);
			return;
		}
		++it;
	}
}

void CMultiplexer::AddPesStream(int pid)
{
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->GetPid()==pid) return;
	}
	
	LogDebug("mux: add pes pid:%x", pid);
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
			if (pesLength>0)
			{
				if (decoder->GetStreamId()==0xe0)
				{
          bool writeToDisk=true;
          if (m_videoPacketCounter==0)
          {
            // make sure we start the file with a mpeg-2 sequence header...
            writeToDisk=false;
            for (int x=6; x < pesLength-3;++x)
            {
              if (pesPacket[x]==0 && pesPacket[x+1]==0 && pesPacket[x+2]==1 && pesPacket[x+3]==0xb3)
              {
                writeToDisk=true;
                break;
              }
            }
          }

          if (writeToDisk)
          {
					  m_videoPacketCounter++;
					  SplitPesPacket(pesPacket,pesLength);
          }
					
				}
				else 
				{
					if (m_videoPacketCounter>0)
					{
						SplitPesPacket(pesPacket,pesLength);
					}
				}
			}
		}
	}
}

int CMultiplexer::WritePackHeader()
{
  __int64 pcrHi=m_pcrDecoder.PcrHigh();
  int pcrLow=m_pcrDecoder.PcrLow();
  int muxRate=4000000/50;
  byte pBuffer[0x20];
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
	pBuffer[6]= ((pcrHi >> 13) & 0x3) + 4 + ( ( (pcrHi >> 15) & 0x1f) <<3);
	pBuffer[7]= (pcrHi >> 5) & 0xff;
	pBuffer[8]= ((pcrLow >> 7) & 3) + 4 + ((pcrHi & 0x1f)<<3);
	pBuffer[9]= ((pcrLow & 0x7f) << 1) +1 ;

	//10..12 = mux rate
	pBuffer[10]= (muxRate >> 14) & 0xff;
	pBuffer[11]= (muxRate >> 6) & 0xff;
	pBuffer[12]=((muxRate & 0x3f) << 2)+3;

	//13 pack stuffing length
	pBuffer[13]=0xf8;
  if (m_pCallback!=NULL)
  {
	  m_pCallback->Write((byte*)pBuffer,	PACK_HEADER_LENGTH);
  }
  return PACK_HEADER_LENGTH;
}



void CMultiplexer::SplitPesPacket(byte* pesPacket, int sectionLength)
{
  if (m_pCallback==NULL) return;
  int streamId=pesPacket[3];
  CStreamBuffer* buffer;
  imapStreamBuffer imap = m_mapStreamBuffer.find(streamId);
  if (imap==m_mapStreamBuffer.end())
  {
     buffer = new CStreamBuffer();
     m_mapStreamBuffer[streamId]=buffer;
  }
  else
  {
    buffer=imap->second;
  }
  int headerLen=pesPacket[8];
  sectionLength -= (headerLen+9);
  int start=buffer->m_ipesBufferPos;

  byte* data=&buffer->m_pesPacket[buffer->m_ipesBufferPos];
  memcpy( &buffer->m_pesPacket[buffer->m_ipesBufferPos], &pesPacket[headerLen+9],sectionLength);
  buffer->m_ipesBufferPos+=sectionLength;
  if (buffer->m_ipesBufferPos < 0x800) return;

  int offset=0;
  while (offset+0x800 < buffer->m_ipesBufferPos)
  {
    WritePackHeader();
    if (offset==0)
    {
      byte* data=m_pesBuffer;
      int len = (0x800-PACK_HEADER_LENGTH) - 6;
      len -=3;
      len-=headerLen;

      memcpy(m_pesBuffer, pesPacket, headerLen+9);
      memcpy(&m_pesBuffer[headerLen+9],&buffer->m_pesPacket[offset],len);
      m_pesBuffer[4]=0x7;
      m_pesBuffer[5]=0xec;
	    m_pCallback->Write(m_pesBuffer, MAX_PES_PACKET_LENGTH + PES_HEADER_LENGTH);
      offset += len;
      
    }
    else
    {
      // 0   1  2  3  4 5  6  7  8  9
      // 00 00 01 e0 07 ec 81 00 00 DD DD DD DD
      int len = (0x800-PACK_HEADER_LENGTH) - 6;
      len -=3;
      m_pesBuffer[0]=0;
      m_pesBuffer[1]=0;
      m_pesBuffer[2]=1;
      m_pesBuffer[3]=pesPacket[3];
      m_pesBuffer[4]=0x7;
      m_pesBuffer[5]=0xec;
      m_pesBuffer[6]=0x81;
      m_pesBuffer[7]=0;
      m_pesBuffer[8]=0;
      memcpy(&m_pesBuffer[9], &buffer->m_pesPacket[offset],len);
	    m_pCallback->Write(m_pesBuffer, MAX_PES_PACKET_LENGTH + PES_HEADER_LENGTH);
      offset += len;
    }
  }
  if (offset < buffer->m_ipesBufferPos)
  {
      memcpy(buffer->m_pesPacket, &buffer->m_pesPacket[offset],buffer->m_ipesBufferPos-offset);
      buffer->m_ipesBufferPos=buffer->m_ipesBufferPos-offset;
  }
}