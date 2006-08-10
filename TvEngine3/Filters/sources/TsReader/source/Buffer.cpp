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


CBuffer::CBuffer()
{
	m_pcrTime=0;
	m_ptsTime=0;
	m_dtsTime=0;
	m_iLength=0;
	m_pBuffer = new byte[MAX_BUFFER_SIZE];
}

CBuffer::~CBuffer()
{
	if (m_pBuffer==NULL)
	{
		int x=1;
	}
	delete [] m_pBuffer;
	m_pBuffer=NULL;
	m_pcrTime=0;
	m_ptsTime=0;
	m_dtsTime=0;
	m_iLength=0;
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

void CBuffer::Set(double pcrTime, double ptsTime, double dtsTime,int length)
{
	m_pcrTime=pcrTime;
	m_ptsTime=ptsTime;
	m_dtsTime=dtsTime;
	m_iLength=length;
}

double CBuffer::Pcr()
{
	return m_pcrTime;
}
double CBuffer::Pts()
{
	return m_ptsTime;
}
double CBuffer::Dts()
{
	return m_dtsTime;
}

void CBuffer::Add(CBuffer* pBuffer)
{
	memcpy(&m_pBuffer[m_iLength], pBuffer->Data(), pBuffer->Length());
	m_iLength+=pBuffer->Length();
}