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

///*******************************************
///Class which holds a single PES-packet
///
CBuffer::CBuffer()
{
  bufferCount++;
  m_bDiscontinuity=false;
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


void CBuffer::SetDiscontinuity()
{
  m_bDiscontinuity=true;
}
bool CBuffer::GetDiscontinuity()
{
  return m_bDiscontinuity;
}
///***************************************************************
///returns a CRefTime which contains the current timestamp
// current timestamp starts at 0 at start of file
// and continously increases (depending on the pcr/pts value of the pes packet)
bool CBuffer::MediaTime(CRefTime &reftime)
{
  //does this pes packet have a pts timestamp
  if (!m_pts.IsValid) return false; //no

  // if PCR rollover occured
  // firstpcr--------------->maxpcr->0-------------------->endpcr
  // <--- partTimeStamp----->
  if (m_firstPcr > m_pts )  
  {
    //pcr rolled over
    //formule : timestamp = pts + (maxpcr-startpc)
    CPcr pts = m_pts;
    double partTimeStamp= ( m_maxPcr.ToClock() - m_firstPcr.ToClock() );
    double ptsTimeStamp = m_pts.ToClock();
    ptsTimeStamp += partTimeStamp;
    ptsTimeStamp *= 1000.0f;
    CRefTime mediaTime((LONG)ptsTimeStamp);
    reftime=mediaTime;
    return true;
  }
  
  // pcr did not rollover
  // startpcr----------------------------->endpcr
  //formule : timestamp = pts -startpc
  CPcr pts=m_pts;
  double startOfFileTimeStamp = m_firstPcr.ToClock();
  double ptsTimeStamp = m_pts.ToClock();
  ptsTimeStamp -= startOfFileTimeStamp;
  ptsTimeStamp *= 1000.0f;
  CRefTime mediaTime( (LONG)ptsTimeStamp );
  reftime=mediaTime;
  return true;
}


///***************************************************************
///Sets the length in bytes of the PES packet
void CBuffer::SetLength(int len)
{
  m_iLength=len;
}

///***************************************************************
///returns the length in bytes of the PES packet
int CBuffer::Length()
{
	return m_iLength;
}

///***************************************************************
///returns the PES packet
byte* CBuffer::Data()
{
	if (m_pBuffer==NULL)
	{
		return NULL;
	}
	return m_pBuffer;
}

///***************************************************************
///Sets PTS packet for the PES packet
void CBuffer::SetPts(CPcr& pts)
{
  m_pts=pts;
}


///***************************************************************
///Sets pcr,startpcr and endpcr when this packet was received
// m_firstPcr   : earliest pcr value found since start of playback
// m_maxPcr     : in case of a PCR rollover, endpcr contains the last pcr timestamp before the
//                rollover occured
void CBuffer::SetPcr(CPcr& firstPcr,CPcr& maxPcr)
{
  m_firstPcr=firstPcr;
  m_maxPcr=maxPcr;
}


///***************************************************************
// Adds data contained in pBuffer to this pes packet
void CBuffer::Add(CBuffer* pBuffer)
{
	if(pBuffer && ( MAX_BUFFER_SIZE >= m_iLength + pBuffer->Length()))
  {
    memcpy(&m_pBuffer[m_iLength], pBuffer->Data(), pBuffer->Length());
	  m_iLength+=pBuffer->Length();
  }
  else
  {
    LogDebug("CBuffer::Add CBuffer - sanity check failed! MAX_BUFFER_SIZE %d lenght %d", MAX_BUFFER_SIZE, pBuffer->Length()+m_iLength );
    if(pBuffer == NULL)
    {
      LogDebug("  pBuffer was NULL!");
    }
  }
}

///***************************************************************
// Adds data contained to this pes packet
void CBuffer::Add(byte* data, int len)
{
	if((MAX_BUFFER_SIZE >= m_iLength + len ) && data) 
  {
    memcpy(&m_pBuffer[m_iLength], data, len);
	  m_iLength+=len;
  }
  else
  {
    LogDebug("CBuffer::Add - sanity check failed! MAX_BUFFER_SIZE %d lenght %d", MAX_BUFFER_SIZE, m_iLength + len );
    if(data == NULL)
    {
      LogDebug("  data was NULL!");
    }
  }
}