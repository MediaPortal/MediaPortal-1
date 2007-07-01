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
#include "buffer.h"

static DWORD bufferCount=0;
extern void LogDebug(const char *fmt, ...) ;

CBuffer::CBuffer()
{
  bufferCount++;
  m_pcr.Reset();
	m_iLength=0;
	m_pBuffer = new byte[MAX_BUFFER_SIZE];
 // LogDebug("buffers:%d",bufferCount);
}

CBuffer::~CBuffer()
{
  
  bufferCount--;
	delete [] m_pBuffer;
	m_pBuffer=NULL;
	m_iLength=0;
}

bool CBuffer::MediaTime(CRefTime &reftime)
{
  if (!m_pts.IsValid) return false;

  if (m_startPcr > m_pts )
  {
    //pcr rolled over    
    CPcr pts=m_pts;
    double d1=( m_endPcr.ToClock() - m_startPcr.ToClock() );
    double d2=m_pts.ToClock();
    d2+=d1;
    d2*=1000.0f;
    CRefTime mediaTime((LONG)d2);
    reftime=mediaTime;
    return true;
    return false;
  }
  CPcr pts=m_pts;
  double d1=m_startPcr.ToClock();
  double d2=m_pts.ToClock();
  d2-=d1;
  d2*=1000.0f;
  CRefTime mediaTime((LONG)d2);
  reftime=mediaTime;
  return true;
}
void CBuffer::SetLength(int len)
{
  m_iLength=len;
}

int CBuffer::Length()
{
	return m_iLength;
}

byte* CBuffer::Data()
{
	if (m_pBuffer==NULL)
	{
		return NULL;
	}
	return m_pBuffer;

}

void CBuffer::SetPts(CPcr& pts)
{
  m_pts=pts;
}


void CBuffer::SetPcr(CPcr& pcr,CPcr& startPcr,CPcr& endPcr)
{
  m_pcr=pcr;
  m_startPcr=startPcr;
  m_endPcr=endPcr;
}

CPcr& CBuffer::Pcr()
{
	return m_pcr;
}

void CBuffer::Add(CBuffer* pBuffer)
{
	memcpy(&m_pBuffer[m_iLength], pBuffer->Data(), pBuffer->Length());
	m_iLength+=pBuffer->Length();
}

void CBuffer::Add(byte* data, int len)
{
	memcpy(&m_pBuffer[m_iLength], data, len);
	m_iLength+=len;
}