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
#include "pcrDecoder.h"
#include "tsheader.h"

extern void LogDebug(const char *fmt, ...) ;

CPcrDecoder::CPcrDecoder(void)
{
	m_pcrPid=-1;
	m_pcrHigh=0;
	m_pcrLow=0;

}

CPcrDecoder::~CPcrDecoder(void)
{
}

void CPcrDecoder::SetPcrPid(int pid)
{
	m_pcrPid=pid;
}

int CPcrDecoder::GetPcrPid()
{
	return m_pcrPid;
}

__int64 CPcrDecoder::PcrHigh()
{
	return m_pcrHigh;
}

int CPcrDecoder::PcrLow()
{
	return m_pcrLow;
}

void CPcrDecoder::OnTsPacket(byte* tsPacket)
{
	if (m_pcrPid==-1) return;
	CTsHeader header(tsPacket);
	if (header.Pid != m_pcrPid) return;

	int afc, len, flags;
	byte *p;
	unsigned int v;

	afc = (tsPacket[3] >> 4) & 3;
	if (afc <= 1)
	{
		return ;
	}
	p = tsPacket + 4;
	len = p[0];
	p++;
	if (len == 0)
	{
			return ;
	}
	flags = *p++;
	len--;
	if (!(flags & 0x10))
	{
			return ;
	}
	if (len < 6)
	{
			return ;
	}
	v = (p[0] << 24) | (p[1] << 16) | (p[2] << 8) | p[3];
	m_pcrHigh = ((__int64)v << 1) | (p[4] >> 7);			//33 bits
	m_pcrLow = ((p[4] & 1) << 8) | p[5];							//9	 bits

//	LogDebug("pcr:%02.2x%02.2x%02.2x%02.2x%02.2x%02.2x",p[0],p[1],p[2],p[3],p[4],p[5]);
}
