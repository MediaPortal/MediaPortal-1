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
#include <windows.h>
#include <stdio.h>
#include "pesdecoder.h"
#include "TsHeader.h"
#include "packetsync.h"


extern void Log(const char *fmt, ...) ;

CPesDecoder::CPesDecoder(CPesCallback* callback)
{
	//LogDebug("pes decoder ctor");
	m_pid=-1;
	m_pesBuffer = new byte[MAX_PES_PACKET];
	m_iWritePos=-1;
	m_iMaxLength=MAX_PES_PACKET;
	m_pCallback=callback;
	m_iStreamId=-1;
  m_iPesHeaderLen=0;
}
void CPesDecoder::SetMaxLength(int len)
{
//	LogDebug("pes decoder pid:%x set maxlen:%x",m_pid,len);
	m_iMaxLength=len;
}

CPesDecoder::~CPesDecoder(void)
{
	delete[] m_pesBuffer; 
}

void CPesDecoder::Reset()
{
	m_iWritePos=-1;
  m_iPesHeaderLen=0;
}

int CPesDecoder::GetPid()
{
	return m_pid;
}

void CPesDecoder::SetPid(int pid)
{
	m_pid=pid;
}


bool CPesDecoder::IsAudio()
{
	return (m_iStreamId>=0xc0 && m_iStreamId<=0xcf);
}
bool CPesDecoder::IsVideo()
{
	return (m_iStreamId>=0xe0 && m_iStreamId<=0xef);
}
void CPesDecoder::SetStreamId(int streamId)
{
	m_iStreamId=streamId;
}
int	CPesDecoder::GetStreamId()
{
	return m_iStreamId;
}
bool CPesDecoder::OnTsPacket(byte* tsPacket)
{
  if (tsPacket==NULL) return false;
	if (m_pid==-1) return false;
	
	CTsHeader  header(tsPacket);
	if (header.Pid != m_pid) return false;
	if (header.SyncByte != TS_PACKET_SYNC) 
	{
		Log("pesdecoder pid:%x sync error", m_pid);
		return false;
	}
  if (header.TransportError) 
	{
    m_bStart=false;
		m_iWritePos=0;
		//Log("pesdecoder pid:%x transport error", m_pid);
		return false;
	}

	BOOL scrambled= (header.TScrambling!=0);
	if (scrambled) return false; 
	if ( header.AdaptionFieldOnly() ) 
	{
		return false;
	}
 
	int pos = header.PayLoadStart;

	bool result=false;
	if (header.PayloadUnitStart)
	{

		if (m_iWritePos>0)
		{
			if (m_pCallback!=NULL)
			{
				//Log(" pes %x start:%x", m_iStreamId,m_iWritePos);
				int written=m_pCallback->OnNewPesPacket(m_iStreamId,m_pesHeader, m_iPesHeaderLen,  m_pesBuffer, m_iWritePos, m_bStart);
        if (written>=0)
        {
				  //Log(" pes %x written:%x", m_iStreamId,written);
          m_bStart=false;
				  m_iWritePos=0;
        }
			}
		}

		if (tsPacket[pos+0]==0 && tsPacket[pos+1]==0 && tsPacket[pos+2]==1)
		{
			if (m_iStreamId<0)
				m_iStreamId=tsPacket[pos+3];
			if (m_iWritePos<0) m_iWritePos=0;

      m_iPesHeaderLen=tsPacket[pos+8]+9;
      memcpy(m_pesHeader,&tsPacket[pos],m_iPesHeaderLen);
      pos += (m_iPesHeaderLen);
			m_bStart=true;
		}
	}

	if (m_iWritePos < 0) return false;
	if (m_iStreamId <= 0) return false;

	memcpy(&m_pesBuffer[m_iWritePos], &tsPacket[pos], 188-pos);
	m_iWritePos += (188-pos);
	//Log(" pes %x copy:%x len:%x maxlen:%x start:%d", m_iStreamId,m_iWritePos,(188-pos),m_iMaxLength,m_bStart);
	if (m_iWritePos  >= m_iMaxLength)
	{
    int written=0;
		if (m_pCallback!=NULL)
		{
			written=m_pCallback->OnNewPesPacket(m_iStreamId,m_pesHeader, m_iPesHeaderLen,  m_pesBuffer, m_iMaxLength, m_bStart);
			//Log(" pes %x next:%x written:%x", m_iStreamId,m_iWritePos,written);
		}
    m_bStart=false;

		memcpy(m_pesBuffer, &m_pesBuffer[written] , m_iWritePos-written);
		m_iWritePos -= written;
		//Log(" pes %x now:%x", m_iStreamId,m_iWritePos);
	}
  return result;
}
