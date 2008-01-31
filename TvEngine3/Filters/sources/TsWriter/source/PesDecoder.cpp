/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
	m_pCallback=callback;
	m_iStreamId=-1;
}

CPesDecoder::~CPesDecoder(void)
{
}

void CPesDecoder::Reset()
{
  m_packet.Reset();
  m_bStartFound=false;
}

int CPesDecoder::GetPid()
{
	return m_pid;
}

void CPesDecoder::SetPid(int pid)
{
	m_pid=pid;
}

bool  CPesDecoder::IsAc3()
{
	return (m_iStreamId>=0xbd && m_iStreamId<=0xbf);
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
	if (pos >=188)
	{
		LogDebug("PesDecoder: pid:%x payload starts at:%d", m_pid,pos);
		return false;
	}
	int len=188-pos;
	if (len <=0)
	{
		LogDebug("PesDecoder: pid:%x no payload:%d", m_pid,pos);
		return false;
	}
	bool result=false;
	if (m_tsHeader.PayloadUnitStart)
	{
    m_bStartFound=true;
	}

	if (m_bStartFound==false) return false;
  m_packet.Write( &tsPacket[pos], 188-pos, m_tsHeader.PayloadUnitStart,pcr);
  if (m_packet.InUse() > 10 && m_pCallback!=NULL)
  {
    m_pCallback->OnNewPesPacket(this);
  }
  return result;
}
