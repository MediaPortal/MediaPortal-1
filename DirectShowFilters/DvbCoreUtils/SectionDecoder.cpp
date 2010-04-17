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
#include "..\shared\SectionDecoder.h"
#include "..\shared\Tsheader.h"

void LogDebug(const char *fmt, ...) ;

CSectionDecoder::CSectionDecoder(void)
{
  m_pid=-1;
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
  if (m_pid < 0) return;
  if (tsPacket==NULL) return;

  m_header.Decode(tsPacket);
  OnTsPacket(m_header,tsPacket);
}

int CSectionDecoder::StartNewSection(byte* tsPacket,int index,int sectionLen)
{
	int newstart=-1;
  int len=-1;
  if (sectionLen > -1)
  {
		if (index + sectionLen < 185)
    {
			len = sectionLen + 3;
      newstart = index + sectionLen + 3;
    }
    else
    {
			newstart = 188;
      len = 188 - index;
    }
  }
  else
  {
		newstart = 188;
    len = 188 - index;
  }
  m_section.Reset();
	memcpy(m_section.Data,&tsPacket[index],len);
  m_section.BufferPos = len;
  m_section.DecodeHeader();
  return newstart;
}

int CSectionDecoder::AppendSection(byte* tsPacket, int index, int sectionLen)
{
	int newstart=-1;
  int len=-1;
  if (index+sectionLen < 185)
  {
		len=sectionLen+3;
    newstart = index+sectionLen+3;
  }
  else
  {
		newstart = 188;
    len=188-index;
  }
	memcpy(&m_section.Data[m_section.BufferPos],&tsPacket[index],len);
  m_section.BufferPos += len;
  return newstart;
}

int CSectionDecoder::SnapshotSectionLength(byte* tsPacket,int start)
{
	if (start >= 184)
		return -1;
  return (int)(((tsPacket[start+1] & 0xF) << 8) + tsPacket[start+2]);
}

void CSectionDecoder::OnTsPacket(CTsHeader& header,byte* tsPacket)
{
	try
	{
    if (header.TransportError) 
    { 
      m_section.Reset(); // Will force us to wait for new PayloadUnitStart
      return; 
    } 
		if (m_pid >= 0x1fff) return;
		if (header.Pid != m_pid) return;
		if (!header.HasPayload) return;

		int start = header.PayLoadStart;
		int pointer_field=0;

    if (header.PayloadUnitStart)
    {
			pointer_field = start + tsPacket[start]+1;
      if (m_section.BufferPos == 0)
				start += tsPacket[start] + 1;
      else
        start++;
    }
	  int numloops=0;
		while (start < 188)
    {
			numloops++;
			if (m_section.BufferPos == 0)
      {
				if (!header.PayloadUnitStart) return;
        if (tsPacket[start] == 0xFF) return;
        int section_length = SnapshotSectionLength(tsPacket, start);
        start = StartNewSection(tsPacket, start, section_length);
      }
      else
      {
        if (m_section.section_length == -1)
          m_section.CalcSectionLength(tsPacket, start);
				if (m_section.section_length==0)
				{
					if (m_bLog)
						LogDebug("!!! CSectionDecoder::OnTsPacket got a section with section length: 0 on pid: 0x%X tableid: 0x%X bufferpos: %d start: %d - Discarding whole packet.",header.Pid,m_section.Data[0],m_section.BufferPos,start);
					m_section.Reset();
					return;
				}
        int len = m_section.section_length - m_section.BufferPos;
        if (pointer_field != 0 && ((start + len) > pointer_field))
        {
					// We have an incomplete section here
          len = pointer_field - start;
          start = AppendSection(tsPacket, start, len);
          m_section.section_length = m_section.BufferPos - 1;
          start = pointer_field;
        }
        else
          start = AppendSection(tsPacket, start, len);
      }
      if (m_section.SectionComplete() && m_section.section_length > 0)
      {
				DWORD crc=0;
        
        // Only long syntax (section_syntax_indicator == 1) has a CRC
        // Short syntax may have CRC e.g. TOT, but that is part of the specific section
        if (m_section.section_syntax_indicator == 1)
          crc=crc32((char*)m_section.Data,m_section.section_length+3);

				if (crc==0 || (m_bCrcCheck==false))
				{
					OnNewSection(m_section);
					if (m_pCallback!=NULL)
						m_pCallback->OnNewSection(header.Pid,m_section.table_id,m_section);
				}
        else
        {
          // If the section is complete and the CRC fails, then this section is crap!
          m_section.Reset();
          return;
        }

        m_section.Reset();
      }
      pointer_field=0;
			if (numloops>100)
			{
				LogDebug("!!! CSectionDecoder::OnTsPacket Entered infinite loop. pid: %X start: %d BufferPos: %d SectionLength: %d - Discarding section and moving to next packet",header.Pid,start,m_section.BufferPos,m_section.section_length);
				m_section.Reset();
				return;
			}
    }
	}
	catch(...)
	{
		LogDebug("exception in CSectionDecoder::OnTsPacket");
	}
}

void CSectionDecoder::OnNewSection(CSection& section)
{
}


