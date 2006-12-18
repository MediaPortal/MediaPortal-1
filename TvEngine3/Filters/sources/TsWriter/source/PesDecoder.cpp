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
#include "packetsync.h"


extern void LogDebug(const char *fmt, ...) ;

CPesDecoder::CPesDecoder(CPesCallback* callback)
{
	m_pid=-1;
	m_pesBuffer = new byte[MAX_PES_PACKET];
	m_iWritePos=-1;
	m_pCallback=callback;
	m_iStreamId=-1;
  packet_number=0;
}

CPesDecoder::~CPesDecoder(void)
{
	delete[] m_pesBuffer; 
}

void CPesDecoder::Reset()
{
  m_packet.Reset();
	m_iWritePos=-1;
  packet_number=0;
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
bool CPesDecoder::OnTsPacket(byte* tsPacket, CPcr& pcr)
{
  if (tsPacket==NULL) return false;
	if (m_pid==-1) return false;
	m_tsHeader.Decode(tsPacket);
	if (m_tsHeader.Pid != m_pid) return false;
	if (m_tsHeader.SyncByte != TS_PACKET_SYNC) 
	{
		LogDebug("pesdecoder pid:%x sync error", m_pid);
		return false;
	}
  if (m_tsHeader.TransportError) 
	{
		m_iWritePos=0;
		//LogDebug("pesdecoder pid:%x transport error", m_pid);
		return false;
	}

	BOOL scrambled= (m_tsHeader.TScrambling!=0);
	if (scrambled) return false; 
	if ( m_tsHeader.AdaptionFieldOnly() ) 
	{
		return false;
	}
 
	int pos = m_tsHeader.PayLoadStart;

	bool result=false;
	if (m_tsHeader.PayloadUnitStart)
	{
		if (m_iWritePos>0)
		{
			if (m_pCallback!=NULL)
			{
				m_pCallback->OnNewPesPacket(this, m_pesBuffer, m_iWritePos);
				m_iWritePos=0;
			}
		}
		if (tsPacket[pos+0]==0 && tsPacket[pos+1]==0 && tsPacket[pos+2]==1)
		{
			if (m_iStreamId<0)
				m_iStreamId=tsPacket[pos+3];
			m_iWritePos=0;
      m_packet.startPcr=pcr;
		}
	}

	if (m_iWritePos < 0) return false;
	if (m_iStreamId <= 0) return false;

	memcpy(&m_pesBuffer[m_iWritePos], &tsPacket[pos], 188-pos);
	m_iWritePos += (188-pos);
  return result;
}
