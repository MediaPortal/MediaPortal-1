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
#include <stdio.h>
#include "pesdecoder.h"
#include "TsHeader.h"
#include "packetsync.h"


extern void LogDebug(const char *fmt, ...) ;

CPesDecoder::CPesDecoder(CPesCallback* callback)
{
	//LogDebug("pes decoder ctor");
	m_pid=-1;
	m_pesBuffer = new byte[MAX_PES_PACKET];
	m_iWritePos=-1;
	m_iMaxLength=MAX_PES_PACKET;
	m_pCallback=callback;
	m_iStreamId=-1;
//	m_fp=NULL;
}
void CPesDecoder::SetMaxLength(int len)
{
	LogDebug("pes decoder pid:%x set maxlen:%x",m_pid,len);
	m_iMaxLength=len;
}

CPesDecoder::~CPesDecoder(void)
{
	//LogDebug("pes decoder pid:%x reset",m_pid);
	delete[] m_pesBuffer;
	//if (m_fp!=NULL)
	//{
	//	fclose(m_fp);
	//	m_fp=NULL;
	//}
}

void CPesDecoder::Reset()
{
	//LogDebug("pes decoder pid:%x reset",m_pid);
	m_iWritePos=-1;
}

int CPesDecoder::GetPid()
{
	return m_pid;
}

void CPesDecoder::SetPid(int pid)
{
	//if (m_fp!=NULL)
	//{
	//	fclose(m_fp);
	//	m_fp=NULL;
	//}
	m_pid=pid;
	//char buf[128];
	//sprintf(buf,"pid%x.mpg", pid);
	//m_fp = fopen(buf,"wb+");
	//LogDebug("pes decoder pid:%x",pid);
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
	if (header.SyncByte != TS_PACKET_SYNC) 
	{
		LogDebug("pesdecoder pid:%x sync error", m_pid);
		return false;
	}
	if (header.TransportError ) 
	{
		LogDebug("pesdecoder pid:%x transport error", m_pid);
		return false;
	}
	if (header.Pid != m_pid) return false;
	BOOL scrambled= (header.TScrambling!=0);
	if (scrambled) return false;
	//header.LogHeader();
	if ( header.AdaptionFieldOnly() ) 
	{
		return false;
	}
 
	int pos = header.PayLoadStart;

	bool result=false;
	if (header.PayloadUnitStart)
	{
		if (tsPacket[pos+0]==0 && tsPacket[pos+1]==0 && tsPacket[pos+2]==1)
		{
			m_iStreamId=tsPacket[pos+3];
      if (m_iWritePos>0)
      {
		    bool start;
		    if (m_pesBuffer[0]==0 && m_pesBuffer[1]==0 && m_pesBuffer[2]==1)
		    {
			    start = true;
		    }
		    else
		    {
			    start = false;
		    }
		    if (m_pCallback!=NULL)
		    {
			    m_pCallback->OnNewPesPacket(m_iStreamId, m_pesBuffer, m_iWritePos, start);
		    }
      }
			m_iWritePos=0;
		}
	}

	if (m_iWritePos < 0) return false;
	if (m_iStreamId <= 0) return false;

	memcpy(&m_pesBuffer[m_iWritePos], &tsPacket[pos], 188-pos);
	m_iWritePos += (188-pos);
	if (m_iWritePos  >= m_iMaxLength)
	{
		bool start;
		int copyLen;
		if (m_pesBuffer[0]==0 && m_pesBuffer[1]==0 && m_pesBuffer[2]==1)
		{
			start = true;
			copyLen = m_iMaxLength;
		}
		else
		{
			start = false;
			copyLen = m_iMaxLength-9;
		}

		if (m_pCallback!=NULL)
		{
			m_pCallback->OnNewPesPacket(m_iStreamId, m_pesBuffer, copyLen, start);
		}

		memcpy(m_pesBuffer, &m_pesBuffer[copyLen] , m_iWritePos-copyLen);
		m_iWritePos -= copyLen;
	}
  return result;
}
