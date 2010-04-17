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
#pragma warning(disable : 4995)
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include "SectionDecoder.h"
#include "Tsheader.h"
#include "packetsync.h"
extern DWORD crc32 (char *data, int len);

void LogDebug(const char *fmt, ...) ;

CSectionDecoder::CSectionDecoder(void)
{
  m_pid=-1;
  m_tableId=-1;
  m_iContinuityCounter=0;
  m_section.Reset();
	m_pCallback=NULL;
  m_bLog=false;
  m_bCrcCheck=true;
}

CSectionDecoder::~CSectionDecoder(void)
{ 
}

void CSectionDecoder::EnableLogging(bool onOff)
{
  m_bLog=onOff;
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

void CSectionDecoder::EnableCrcCheck(bool onOff)
{
  m_bCrcCheck=onOff;
}
void CSectionDecoder::OnTsPacket(byte* tsPacket)
{
  if (m_tableId < 0 || m_pid < 0) return;
  if (tsPacket==NULL) return;

  m_header.Decode(tsPacket);
  OnTsPacket(m_header,tsPacket);
}

void CSectionDecoder::OnTsPacket(CTsHeader& header,byte* tsPacket)
{
  if (m_tableId < 0 || m_pid < 0) return;
 
  if (header.Pid != m_pid) return;

 
	if (m_bLog)
    LogDebug("pid:%03.3x table id:%03.3x payloadunit start:%x start:%d tableid:%x len:%d",m_pid,m_tableId,(int)header.PayloadUnitStart,header.PayLoadStart, tsPacket[header.PayLoadStart], 188-header.PayLoadStart);
 	if (header.PayloadUnitStart)
	{
    
		int start=header.PayLoadStart;
    if (header.AdaptionFieldLength > 0 && tsPacket[start]==0 && m_tableId != 0 && m_section.BufferPos == 0)
    {
      //skip pointer_field ??
      start++;
    }
    if (m_section.BufferPos > 0 /*&& m_section.SectionLength > 0*/ && start > 5)
		{
      int len=start-5;
			if (m_section.BufferPos+len < MAX_SECTION_LENGTH)
		  {
				memcpy(&m_section.Data[m_section.BufferPos],&tsPacket[5],len);
				m_section.BufferPos +=len;
				m_section.SectionPos+=len;
        int startSec=5;
		    int section_syntax_indicator = (m_section.Data[startSec+1]>>7) & 1;
		    int current_next_indicator = m_section.Data[startSec+5] & 1;
		    int section_length = ((m_section.Data[startSec+1]& 0xF)<<8) + m_section.Data[startSec+2];
		    int transport_stream_id = (m_section.Data[startSec+3]<<8)+m_section.Data[startSec+4];
		    int version_number = ((m_section.Data[startSec+5]>>1)&0x1F);
		    int section_number = m_section.Data[startSec+6];
		    int last_section_number = m_section.Data[startSec+7];
        unsigned int network_id= (m_section.Data[startSec+8]<<16)+(m_section.Data[startSec+9]);

		    m_section.Length=section_length+startSec+3;
        m_section.NetworkId=network_id;
        m_section.TransportId=transport_stream_id;
        m_section.Version=version_number;
        m_section.SectionNumber=section_number;
        m_section.SectionLength=section_length;
		    m_section.LastSectionNumber=last_section_number;
				if (m_bLog)
          LogDebug("pid:%03.3x append buffer pos:%d sectionpos:%d section length:%d",m_pid,m_section.BufferPos,m_section.SectionPos,m_section.SectionLength);

        ProcessSection();
			}
      else
      {
				if (m_bLog) LogDebug("INVALID START: ERR2");
      }
		}
    else
    {
      if (tsPacket[start]!=m_tableId && tsPacket[1+start]==m_tableId) 
      {
        start++;
      }
      else
      {
			  if (m_bLog) LogDebug("INVALID START: ERR1");
      }
    }

		m_section.Reset();
		while (tsPacket[start]==m_tableId && start+10 < 188 )
    {
		  int table_id = tsPacket[start];
  		
		  int section_syntax_indicator = (tsPacket[start+1]>>7) & 1;
		  int current_next_indicator = tsPacket[start+5] & 1;
		  int section_length = ((tsPacket[start+1]& 0xF)<<8) + tsPacket[start+2];
		  int transport_stream_id = (tsPacket[start+3]<<8)+tsPacket[start+4];
		  int version_number = ((tsPacket[start+5]>>1)&0x1F);
		  int section_number = tsPacket[start+6];
		  int last_section_number = tsPacket[start+7];
      unsigned int network_id= (tsPacket[start+8]<<16)+(tsPacket[start+9]);

		  m_section.Length=section_length+start+3;
      m_section.NetworkId=network_id;
      m_section.TransportId=transport_stream_id;
      m_section.Version=version_number;
      m_section.SectionNumber=section_number;
      m_section.SectionLength=section_length;
		  m_section.LastSectionNumber=last_section_number;
		  //int len = TS_PACKET_LEN-8;
      m_section.SectionPos=0;
      m_section.BufferPos=0;
		  m_iContinuityCounter=header.ContinuityCounter;

      int len=section_length+3;
      if (len > 188-start) len=188-start;
      memcpy(&m_section.Data[0], tsPacket, 5);
      memcpy(&m_section.Data[5], &tsPacket[start], len);
      m_section.Data[4]=0;
		  m_section.BufferPos= (len+5);
      m_section.SectionPos= len-3;
  		
      if (m_section.SectionPos >= m_section.SectionLength)
		  {
				if (m_bLog)
          LogDebug("pid:%03.3x new bufferpos:%d sectionpos:%d section len:%d",m_pid,m_section.BufferPos,m_section.SectionPos,m_section.SectionLength);
        ProcessSection();
        start = start+section_length+3;
				if (start+10 > 188) break;
		  }
      else break;
    }
    
    int len=188-start;
    if (len>0)
    {
      if (tsPacket[start]==m_tableId)
      {
        memcpy(&m_section.Data[0], tsPacket, 5);
      m_section.Data[4]=0;
		    m_section.BufferPos= 5;
		    m_section.SectionPos= 0;
		    memcpy(&m_section.Data[m_section.BufferPos], &tsPacket[start], len);
		    m_section.BufferPos += (len);
        m_section.SectionPos+= (len);
		    if (m_bLog)
				    LogDebug("pid:%03.3x add %d %d %d",m_pid,m_section.BufferPos,m_section.SectionPos,m_section.SectionLength);
      }
    }
	}
	else
	{
		if (m_section.BufferPos==0) return;//wait for payloadunit start...
		int start=header.PayLoadStart;
    int len=188-start;
		if (m_section.BufferPos+len>=MAX_SECTION_LENGTH)
		{
      LogDebug("section decoder:section length to large3 pid:%x table:%x", m_pid,m_tableId);
			return;
		}
		if (len <=0)
		{
      LogDebug("section decoder:section len < 0 pid:%x table:%x", m_pid,m_tableId);
			return;
		}
		memcpy(&m_section.Data[m_section.BufferPos], &tsPacket[start], len);
		m_section.BufferPos += (len);
    m_section.SectionPos+= (len);
		if (m_bLog)
				LogDebug("pid:%03.3x add %d %d %d",m_pid,m_section.BufferPos,m_section.SectionPos,m_section.SectionLength);

		if (m_section.SectionPos >= m_section.SectionLength)
		{
      ProcessSection();
		}
	}
}


void CSectionDecoder::OnNewSection(CSection& section)
{
}

void CSectionDecoder::ProcessSection()
{
  if (m_section.SectionPos<=5 || m_section.SectionLength<=0) 
  {
		m_section.Reset();
    return;
  }
  m_headerSection.Decode(m_section.Data);
  int start=m_headerSection.PayLoadStart;
  if (m_section.Data[start]==m_tableId)
  {
		int section_length = ((m_section.Data[start+1]& 0xF)<<8) + m_section.Data[start+2];
    if (section_length>0)
    {
      DWORD crc=crc32((char*)&m_section.Data[m_headerSection.PayLoadStart],section_length+3);
      if (crc==0 || (m_bCrcCheck==false))
      {
		    if (m_bLog)
        {
          LogDebug("pid:%03.3x got bufferpos:%d sectionpos:%d section len:%d",m_pid,m_section.BufferPos,m_section.SectionPos,m_section.SectionLength);
          LogDebug("");
        }

	      OnNewSection(m_section);
	      if (m_pCallback!=NULL)
	      {
		      m_pCallback->OnNewSection(m_pid, m_tableId, m_section);
	      }
      }
      else if (m_bLog)
      {
        LogDebug("INVALID CRC ERROR");
        LogDebug("");
        int x=123;
      }
    }
      else if (m_bLog)
      {
        LogDebug("INVALID SECTION LEN");
        LogDebug("");
        int x=123;
      }
  }
  else
  {
		if (m_bLog)
        LogDebug("INVALID TABLEID");
    int x=123;
  }
	m_section.Reset();
}

