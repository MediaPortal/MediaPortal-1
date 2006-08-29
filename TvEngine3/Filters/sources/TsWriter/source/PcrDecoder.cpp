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
void CPcrDecoder::Reset()
{
	m_pcrHigh=0;
	m_pcrLow=0;
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
__int64 CPcrDecoder::Pcr()
{
	return m_pcrHigh;
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
bool CPcrDecoder::GetPtsDts(byte* pesHeader, __int64& pts, __int64& dts)
{
	pts=0;
	dts=0;
	bool ptsAvailable=false;
	bool dtsAvailable=false;
	if ( (pesHeader[7]&0x80)!=0) ptsAvailable=true;
	if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	if (ptsAvailable)
	{	
		pts+= ((pesHeader[13]>>1)&0x7f);					// 7bits	7
		pts+=(pesHeader[12]<<7);								// 8bits	15
		pts+=((pesHeader[11]>>1)<<15);					// 7bits	22
		pts+=((pesHeader[10])<<22);							// 8bits	30
    __int64 k=((pesHeader[9]>>1)&0x7);
    k <<=30;
		pts+=k;			// 3bits
	}
	if (dtsAvailable)
	{
		dts= (pesHeader[18]>>1);								// 7bits	7
		dts+=(pesHeader[17]<<7);								// 8bits	15
		dts+=((pesHeader[16]>>1)<<15);					// 7bits	22
		dts+=((pesHeader[15])<<22);							// 8bits	30
    __int64 k=((pesHeader[14]>>1)&0x7);
    k <<=30;
		dts+=k;			// 3bits
	
	}
	return (ptsAvailable||dtsAvailable);
}

void CPcrDecoder::ChangePtsDts(byte* header, __int64 startPcr)
{
	__int64 pts=0,dts=0;
	if (!GetPtsDts(header, pts, dts)) 
	{
		return ;
	}
	if (pts>0)
	{
		if (pts < startPcr) 
			pts=0;
		else
			pts-=startPcr;
		byte marker=0x21;
		if (dts!=0) marker=0x31;
		header[13]=(((pts&0x7f)<<1)+1); pts>>=7;
		header[12]= (pts&0xff);				  pts>>=8;
		header[11]=(((pts&0x7f)<<1)+1); pts>>=7;
		header[10]=(pts&0xff);					pts>>=8;
		header[9]= (((pts&7)<<1)+marker); 
	}
	if (dts >0)
	{
		if (dts < startPcr) 
			dts=0;
		else
			dts-=startPcr;

		header[18]=(((dts&0x7f)<<1)+1); dts>>=7;
		header[17]= (dts&0xff);				  dts>>=8;
		header[16]=(((dts&0x7f)<<1)+1); dts>>=7;
		header[15]=(dts&0xff);					dts>>=8;
		header[14]= (((dts&7)<<1)+0x11); 
	}
}