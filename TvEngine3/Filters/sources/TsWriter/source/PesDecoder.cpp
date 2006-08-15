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

CPesDecoder::CPesDecoder()
{
	//LogDebug("pes decoder ctor");
	m_pid=-1;
	m_pesBuffer = new byte[MAX_PES_PACKET];
	m_pesPacket = new byte[MAX_PES_PACKET];
	m_pespacketLen=0;
	m_iPesBufferPos=-1;
//	m_fp=NULL;
}

CPesDecoder::~CPesDecoder(void)
{
	//LogDebug("pes decoder pid:%x reset",m_pid);
	delete[] m_pesBuffer;
	delete[] m_pesPacket;
	//if (m_fp!=NULL)
	//{
	//	fclose(m_fp);
	//	m_fp=NULL;
	//}
}

void CPesDecoder::Reset()
{
	//LogDebug("pes decoder pid:%x reset",m_pid);
	m_iPesBufferPos=-1;
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

byte* CPesDecoder::GetPesPacket()
{
	return m_pesPacket;
}
int	CPesDecoder::GetStreamId()
{
	return m_pesPacket[3];
}
int	CPesDecoder::GetPesPacketLength()
{
	return m_pespacketLen;
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
		if (m_iPesBufferPos>0)
		{
			if (m_pesBuffer[0]==0 && m_pesBuffer[1]==0 && m_pesBuffer[2]==1)
			{
//				LogDebug("pes pid:%x size:%x", m_pid,m_iPesBufferPos);
				OnNewPesPacket(m_pesBuffer, m_iPesBufferPos);
				result=true;
			}
		}
		if (tsPacket[pos]!=0 || tsPacket[pos+1] !=0 || tsPacket[pos+2] !=1)
		{
			m_iPesBufferPos=-1;	
			LogDebug("pes pid:%x, not a pes packet %0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x%0.2x",
				m_pid,tsPacket[0],tsPacket[1],tsPacket[2],tsPacket[3],tsPacket[4],tsPacket[5],tsPacket[6],tsPacket[7],tsPacket[8],tsPacket[9],tsPacket[10]
			,tsPacket[11],tsPacket[12],tsPacket[13],tsPacket[14],tsPacket[15],tsPacket[16],tsPacket[17],tsPacket[18],tsPacket[19],tsPacket[20]);
		}
		else
		{
			m_iPesBufferPos=0;
		}
		//memset(m_pesBuffer,0xff,MAX_PES_PACKET);
	}
	else
	{
		if (m_iPesBufferPos<0)
    {
      return  false;
    }
	}
	if (m_iPesBufferPos+188-pos >= MAX_PES_PACKET)
	{
		LogDebug("pesdecoder pid:%x invalid pes packet len:%d",m_pid, m_iPesBufferPos+188-pos);
		m_iPesBufferPos=0;
		return  false;
	}
	//LogDebug("copy %d>%d-%d", pos,m_iPesBufferPos,m_iPesBufferPos+188-pos);
  
	memcpy(&m_pesBuffer[m_iPesBufferPos], &tsPacket[pos], 188-pos);
	m_iPesBufferPos += (188-pos);
	return result;
}


void CPesDecoder::OnNewPesPacket(byte* pesPacket, int nLen)
{   
  if (pesPacket==NULL) return;
  if (nLen<=0) return;
  if (nLen > MAX_PES_PACKET)
  {
    LogDebug("pesdecoder::OnNewPesPacket pid:%x invalid pes packet len:%d",m_pid, nLen);
    return;
  }
  memcpy(m_pesPacket,pesPacket,nLen);
  m_pespacketLen=nLen;
}