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
#pragma warning(disable : 4995)
#include <windows.h>
#include <stdio.h>
#include <math.h>
#include "pespacket.h" 


CBuffer::CBuffer()
{
  m_pData = new byte[200];
  m_iReadPtr=0;
  m_iSize=0;
  m_bIsStart=false;
	m_bSequenceHeader=false;
}

CBuffer::~CBuffer()
{
  delete[] m_pData;
}

void CBuffer::Reset()
{
  m_iReadPtr=0;
  m_iSize=0;
  m_bIsStart=false;
	m_bSequenceHeader=false;
  m_pcr.Reset();
  m_dts.Reset();
  m_pts.Reset();
}

int CBuffer::Write(byte* data, int len, bool isStart,CPcr& pcr)
{
		for (int i=0; i < len-4;++i)
		{
			if (data[i]==0 && data[i+1]==0 && data[i+2]==1 && data[i+3]==0xba)
			{
				int x=1;
			}
		}
  m_bSequenceHeader=false;
  if (isStart)
  {
    CPcr::DecodeFromPesHeader(data,m_pts,m_dts);
		int pos=data[8]+9;
		if (data[pos]==0 && data[pos+1]==0 && data[pos+2]==1 && data[pos+3]==0xb3)
			m_bSequenceHeader=true;

  }
  else
  {
    m_pts.Reset();
    m_dts.Reset();
  }
    
  memcpy(m_pData,data,len);
  m_bIsStart=isStart;
  m_pcr=pcr;
  m_iSize=len;
  m_iReadPtr=0;
  return m_iSize;
}
int CBuffer::Read(byte* data, int len)
{
  int available=m_iSize-m_iReadPtr;
  if (available==0)
  {
    int x=1;
  }
  if (len>available) len=available;
  memcpy(data,&m_pData[m_iReadPtr],len);
  m_iReadPtr+=len;
  return len;
}
int CBuffer::Size()
{
  return (m_iSize-m_iReadPtr);
}

void CBuffer::HasPtsDts(bool& pts, bool &dts)
{
  pts=dts=false;
  if (m_pts.PcrReferenceBase!=0)
  {
    pts=true;
    if (m_dts.PcrReferenceBase!=0)
    {
      dts=true;
    }
  }
}

bool CBuffer::HasSequenceHeader()
{
	return m_bSequenceHeader;
}

bool CBuffer::IsStart()
{
  return m_bIsStart;
}

CPcr& CBuffer::Pcr()
{
  return m_pcr;
}

CPcr& CBuffer::Pts()
{
  return m_pts;
}
CPcr& CBuffer::Dts()
{
  return m_dts;
}
CPesPacket::CPesPacket()
{
 Reset();
}

CPesPacket::~CPesPacket()
{
}

void CPesPacket::Reset()
{
  packet_number=0;
  m_totalSize=0;
  m_iCurrentWriteBuffer=0;
  m_iCurrentReadBuffer=0;
  m_inUse=m_maxInUse=0;
  for (int i=0; i < MAX_BUFFERS;++i)
  {
    m_buffers[i].Reset();
  }
}

bool CPesPacket::IsStart()
{
  return m_buffers[m_iCurrentReadBuffer].IsStart();
}
CPcr& CPesPacket::Pcr()
{
  return m_buffers[m_iCurrentReadBuffer].Pcr();
}
CPcr& CPesPacket::Pts()
{
  return m_buffers[m_iCurrentReadBuffer].Pts();
}
CPcr& CPesPacket::Dts()
{
  return m_buffers[m_iCurrentReadBuffer].Dts();
}
bool CPesPacket::HasSequenceHeader()
{
	return m_buffers[m_iCurrentReadBuffer].HasSequenceHeader();
}

void CPesPacket::Skip()
{
  m_iCurrentReadBuffer++;
  if (m_iCurrentReadBuffer>=MAX_BUFFERS) m_iCurrentReadBuffer=0;
}

void CPesPacket::Write(byte* data, int len, bool isStart,CPcr& pcr)
{
  m_inUse++;
  if (m_inUse>m_maxInUse)
  {
    m_maxInUse=m_inUse;
    printf("inuse:%d\n",m_maxInUse);
  }
  m_totalSize+=m_buffers[m_iCurrentWriteBuffer].Write(data, len, isStart,pcr);
  m_iCurrentWriteBuffer++;
  if (m_iCurrentWriteBuffer>=MAX_BUFFERS) m_iCurrentWriteBuffer=0;
  if (m_iCurrentWriteBuffer==m_iCurrentReadBuffer)
  {
    int x=1;
  }
}

int CPesPacket::InUse()
{
  return m_inUse;
}
void CPesPacket::NextPacketHasPtsDts(bool& pts, bool &dts)
{
   m_buffers[m_iCurrentReadBuffer].HasPtsDts(pts, dts);
}

int CPesPacket::Read(byte* data, int len)
{
  int off=0;
  int bytesRead=0;
  while (len>0)
  {
    int read=m_buffers[m_iCurrentReadBuffer].Read( &data[off],len);
    off+=read;
    len-=read;
    m_totalSize-=read;
    bytesRead+=read;
    if (m_buffers[m_iCurrentReadBuffer].Size()==0 || read==0) 
    {
      m_inUse--;
      if (m_inUse>m_maxInUse)
      {
        m_maxInUse=m_inUse;
        printf("inuse:%d\n",m_maxInUse);
      }
      m_iCurrentReadBuffer++;
      if (m_iCurrentReadBuffer>=MAX_BUFFERS) m_iCurrentReadBuffer=0;
      if (IsStart()) return bytesRead;
    }
  }
  return bytesRead;
}

bool CPesPacket::IsAvailable(int size)
{
  return (m_totalSize>size);
}