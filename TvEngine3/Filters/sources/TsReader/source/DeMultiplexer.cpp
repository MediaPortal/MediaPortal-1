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
#include <streams.h>
#include "demultiplexer.h"
#include "buffer.h"

#define BUFFER_LENGTH 0x1000

CDeMultiplexer::CDeMultiplexer(MultiFileReader& reader, CCritSec* section)
:m_reader(reader)
,m_pcrDecoder(reader)
,m_section(section)
{
	m_pBuffer = new byte[BUFFER_LENGTH];
	m_iBufferPosWrite=0;
	m_iBufferPosRead=0;
	m_iBytesInBuffer=0;
	m_ptsTime=0;
	m_dtsTime=0;
	m_pcrTime=0;
}
CDeMultiplexer::~CDeMultiplexer()
{
	delete[] m_pBuffer;
}

void CDeMultiplexer::Reset()
{
	::OutputDebugStringA("CDeMultiplexer::Reset()\n");
	CAutoLock lock(m_section);
	
	m_iBufferPosWrite=0;
	m_iBufferPosRead=0;
	m_iBytesInBuffer=0;
	m_ptsTime=0;
	m_dtsTime=0;
	m_pcrTime=0;

	ivecBuffer i=m_vecAudioBuffers.begin();
	while (i != m_vecAudioBuffers.end())
	{
		CBuffer* buffer= *i;
		delete buffer;
		i=m_vecAudioBuffers.erase(i);
	}
	i=m_vecVideoBuffers.begin();
	while (i != m_vecVideoBuffers.end())
	{
		CBuffer* buffer= *i;
		delete buffer;
		i=m_vecVideoBuffers.erase(i);
	}
	m_vecAudioBuffers.clear();
	m_vecVideoBuffers.clear();
}

int CDeMultiplexer::VideoPacketCount()
{
	return m_vecVideoBuffers.size();
}
int CDeMultiplexer::AudioPacketCount()
{
	return m_vecAudioBuffers.size();
}
CBuffer* CDeMultiplexer::GetAudio()
{
	CAutoLock lock(m_section);
	while (m_vecAudioBuffers.size() < 1) 
	{
		Parse();
	}
	if (m_vecAudioBuffers.size() > 0) 
	{
		ivecBuffer i = m_vecAudioBuffers.begin();
		CBuffer* buffer= *i;
		m_vecAudioBuffers.erase(i);
		return buffer;
	}
	return NULL;
}

CBuffer* CDeMultiplexer::GetVideo()
{
	CAutoLock lock(m_section);
	while (m_vecVideoBuffers.size() < 1) 
	{
		Parse();
	}
	if (m_vecVideoBuffers.size() > 0) 
	{
		ivecBuffer i = m_vecVideoBuffers.begin();
		CBuffer* buffer= *i;
		m_vecVideoBuffers.erase(i);
		return buffer;
	}
	return NULL;
}


void CDeMultiplexer::Require()
{
	int len;
	if (m_iBufferPosRead <= m_iBufferPosWrite )
	{
		//------R-----------W--------------L
		len=BUFFER_LENGTH - m_iBufferPosWrite;
	}
	else
	{
		//----W-------------R-------------L
		len=(m_iBufferPosRead - m_iBufferPosWrite)-1;
	}



	if (len>0)
	{
		
		ULONG bytesRead;
		m_reader.Read(&m_pBuffer[m_iBufferPosWrite],len,&bytesRead);

		m_iBytesInBuffer+=bytesRead;
		m_iBufferPosWrite+=bytesRead;
		if (m_iBufferPosWrite>=BUFFER_LENGTH) 
		{
			m_iBufferPosWrite=0;
		}
	}
	
}

byte CDeMultiplexer::Next(int len)
{
	if (len >m_iBytesInBuffer)
	{
		int x=1;
	}
	int pos=m_iBufferPosRead+len;
	if (pos >= BUFFER_LENGTH)
		pos-=BUFFER_LENGTH;
	return m_pBuffer[pos];
}

void CDeMultiplexer::Advance(int len)
{
	if (len >m_iBytesInBuffer)
	{
		int x=1;
	}
	int pos=m_iBufferPosRead+len;
	if (pos >= BUFFER_LENGTH)
		pos-=BUFFER_LENGTH;
	m_iBufferPosRead=pos;
	m_iBytesInBuffer-=len;
}

int CDeMultiplexer::BufferLength()
{
	return m_iBytesInBuffer;
}

void CDeMultiplexer::Copy(int len, byte* destination)
{
	if (BufferLength() < len)
	{
		Require();
	}
	if (m_iBufferPosRead+len <= BUFFER_LENGTH)
	{
		memcpy(destination,&m_pBuffer[m_iBufferPosRead],len);
	}
	else
	{
		int len1=BUFFER_LENGTH-m_iBufferPosRead;
		memcpy(destination,&m_pBuffer[m_iBufferPosRead],len1);
		memcpy(&destination[len1],&m_pBuffer[0],len-len1);
	}
	
}

void CDeMultiplexer::Parse()
{
	byte header[1200];
	if (BufferLength() < 0x100)
	{
		Require();
	}
	while (BufferLength() >= 50)
	{
		if (Next(0)==0 && Next(1)==0 && Next(2)==1)
		{
			switch (Next(3))
			{
				//pack header
				case 0xba:
					//decode the pcr
					byte buffer[14];
					Copy(14, buffer);
					m_pcrTime=m_pcrDecoder.GetPcr(buffer);
					Advance(14);
				break;

				//audio
				case 0xc0:
				{
					m_streamId=0xc0;
					int len=(Next(4)<<8) + Next(5);
					if (BufferLength()<len+10)
					{
						Require();
					}
					int headerLen=Next(8);
					Copy(headerLen+9,header);
					Advance(9);
					int byteKar=Next(0);
					Advance(headerLen);
					byteKar=Next(0);
					len -=headerLen;
					len -=3;
					CBuffer* pBuffer= new CBuffer();
					Copy(len, pBuffer->Data());
					double pts=0,dts=0;
					if (headerLen>0)
					{
						m_pcrDecoder.GetPtsDts(header,pts,dts);
					}
					pBuffer->Set(m_pcrTime,pts,dts,len);
					/*if (m_vecAudioBuffers.size()>=2)
					{
						ivecBuffer it=m_vecAudioBuffers.begin();
						CBuffer* pBuf=*it;
						delete pBuf;
						m_vecAudioBuffers.erase(it);
					}*/
					m_vecAudioBuffers.push_back(pBuffer);
					Advance(len);
					return;
				}
				break;
				
				//video
				case 0xe0:
				{
					m_streamId=0xe0;
					int len=(Next(4)<<8) + Next(5);
					if (BufferLength()<len+10)
					{
						Require();
					}
					int headerLen=Next(8);
					Copy(headerLen+9,header);
					Advance(9);
					int byteKar=Next(0);
					Advance(headerLen);
					 byteKar=Next(0);
					len -=headerLen;
					len -=3;
					CBuffer* pBuffer= new CBuffer();
					Copy(len, pBuffer->Data());
					double pts=0,dts=0;
					if (headerLen>0)
					{
						m_pcrDecoder.GetPtsDts(header,pts,dts);
					}
					pBuffer->Set(m_pcrTime,pts,dts,len);
					m_vecVideoBuffers.push_back(pBuffer);
					Advance(len);
					return;
				}
				break;

				default:
					Advance(1);
				break;
			}
		}
		else 
		{	
			Advance(1);
		}
	}
}