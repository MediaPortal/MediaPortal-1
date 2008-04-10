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
#include <stdlib.h>
#include "SectionDecoder.h"
#include "TsHeader.h"
#include "packetsync.h"


void Log(const char *fmt, ...) ;
CSectionDecoder::CSectionDecoder(void)
{
  m_pid=-1;
  m_tableId=-1;
  m_iContinuityCounter=0;
  m_section.Reset();
	m_pCallback=NULL;
}

CSectionDecoder::~CSectionDecoder(void)
{ 
}

void CSectionDecoder::SetCallBack(ISectionCallback* callback)
{
	m_pCallback=callback;
}

void CSectionDecoder::SetPid(int pid)
{
  m_pid=pid;
}

int CSectionDecoder::GetPid()
{
  return m_pid;
}

void CSectionDecoder::SetTableId(int tableId)
{
  m_tableId=tableId;
}

int CSectionDecoder::GetTableId()
{
  return m_tableId;
}
void CSectionDecoder::Reset()
{
  m_section.Reset();
}
void CSectionDecoder::OnTsPacket(byte* tsPacket)
{
  if (m_tableId < 0 || m_pid < 0) return;

  CTsHeader header(tsPacket);
  if (header.Pid != m_pid) return;
    
  //Log(" section decoder pid:%x ontspacket payloadunitstart:%d",m_pid,header.PayloadUnitStart);
	if (header.PayloadUnitStart)
	{
		int start=header.PayLoadStart+1;
		int table_id = tsPacket[start];
		
		int section_syntax_indicator = (tsPacket[start+1]>>7) & 1;
		int current_next_indicator = tsPacket[start+5] & 1;
   // Log("  tableid:%x si:%x cni:%x",table_id,section_syntax_indicator,current_next_indicator);
		if (current_next_indicator==0)  return;
		if (table_id != m_tableId) return ;
		if (section_syntax_indicator!=1) return;

		int section_length = ((tsPacket[start+1]& 0xF)<<8) + tsPacket[start+2];
		int transport_stream_id = (tsPacket[start+3]<<8)+tsPacket[start+4];
		int version_number = ((tsPacket[start+5]>>1)&0x1F);
		int section_number = tsPacket[start+6];
		int last_section_number = tsPacket[start+7];
    unsigned int network_id= (tsPacket[start+8]<<16)+(tsPacket[start+9]);

    //section is identified by:pid , tableId, sectionNumber, TransportId and networkId

    //Log("%x:%x %x %x %x %d/%d len:%d ",m_pid,m_tableId,network_id,transport_stream_id,current_next_indicator,section_number,last_section_number,section_length);

		m_section.Length=section_length+start+3;
    m_section.NetworkId=network_id;
    m_section.TransportId=transport_stream_id;
    m_section.Version=version_number;
    m_section.SectionNumber=section_number;
    m_section.SectionLength=section_length;
		m_section.LastSectionNumber=last_section_number;
		int len = TS_PACKET_LEN-8;
    m_section.SectionPos=0;
    m_section.BufferPos=0;
		m_iContinuityCounter=header.ContinuityCounter;

		if (m_section.BufferPos+188>=MAX_SECTION_LENGTH)
		{
//      Log("section decoder:section length to large pid:%x table:%x", m_pid,m_tableId);
			return;
		}
    memcpy(&m_section.Data[m_section.BufferPos], tsPacket, 188);
		m_section.BufferPos+=188;
    m_section.SectionPos+= 188-(start+3);
		
    if (m_section.SectionPos >= m_section.SectionLength)
		{
			OnNewSection(m_section);
			if (m_pCallback!=NULL)
			{
				m_pCallback->OnNewSection(m_pid, m_tableId, m_section);
			}
		}
	}
	else
	{
		if (m_section.BufferPos==0) return;//wait for payloadunit start...
		if (m_section.SectionPos>=m_section.SectionLength) 
    {
//      Log("section decoder:section length to large2 pid:%x table:%x", m_pid,m_tableId);
      m_section.BufferPos=0;
      return;
    }

		int start=header.PayLoadStart;
    int len=188-start;
		
		if (m_section.BufferPos+len>=MAX_SECTION_LENGTH)
		{
//      Log("section decoder:section length to large3 pid:%x table:%x", m_pid,m_tableId);
			return;
		}
		if (len <=0)
		{
//      Log("section decoder:section len < 0 pid:%x table:%x", m_pid,m_tableId);
			return;
		}
		memcpy(&m_section.Data[m_section.BufferPos], &tsPacket[start], len);
		m_section.BufferPos += (len);
    m_section.SectionPos+= (len);
		if (m_section.SectionPos >= m_section.SectionLength)
		{
			OnNewSection(m_section);
			if (m_pCallback!=NULL)
			{
				m_pCallback->OnNewSection(m_pid, m_tableId, m_section);
			}
      m_section.BufferPos=0;
		}
	}
}


void CSectionDecoder::OnNewSection(CSection& section)
{
}


