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
#include "pcrDecoder.h"

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
	m_pcrHigh=0;
	m_pcrLow=0;
	m_pcrPid=pid;
}

int CPcrDecoder::GetPcrPid()
{
	return m_pcrPid;
}

UINT64 CPcrDecoder::PcrHigh()
{
	return m_pcrHigh;
}

UINT64 CPcrDecoder::PcrLow()
{
	return m_pcrLow;
}
UINT64 CPcrDecoder::Pcr()
{
	return m_pcrHigh;
}

void CPcrDecoder::OnTsPacket(byte* tsPacket)
{
	if (m_pcrPid==-1) return;
	m_tsHeader.Decode(tsPacket);
	if (m_tsHeader.Pid != m_pcrPid) return;
  if (m_tsHeader.PayLoadOnly()) return;
  if (tsPacket[4]<7) return; //adaptation field length
  if ((tsPacket[5] & 0x10) ==0 ) return;

	UINT64 pcrBaseHigh=0LL;
	UINT64 k=tsPacket[6]; k<<=25LL;pcrBaseHigh+=k;
	k=tsPacket[7]; k<<=17LL;pcrBaseHigh+=k;
	k=tsPacket[8]; k<<=9LL;pcrBaseHigh+=k;
	k=tsPacket[9]; k<<=1LL;pcrBaseHigh+=k;
	k=((tsPacket[10]>>7)&0x1); pcrBaseHigh +=k;
  m_pcrHigh=pcrBaseHigh;
  m_pcrLow=0;


}
bool CPcrDecoder::GetPtsDts(byte* pesHeader, UINT64& pts, UINT64& dts)
{
	pts=0LL;
	dts=0LL;
	bool ptsAvailable=false;
	bool dtsAvailable=false;
	if ( (pesHeader[7]&0x80)!=0) ptsAvailable=true;
	if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	if (ptsAvailable)
	{	
		pts+= ((pesHeader[13]>>1)&0x7f);				// 7bits	7
		pts+=(pesHeader[12]<<7);								// 8bits	15
		pts+=((pesHeader[11]>>1)<<15);					// 7bits	22
		pts+=((pesHeader[10])<<22);							// 8bits	30
    UINT64 k=((pesHeader[9]>>1)&0x7);
    k <<=30LL;
		pts+=k;			// 3bits
		pts &= 0x1FFFFFFFFLL;
	}
	if (dtsAvailable)
	{
		dts= (pesHeader[18]>>1);								// 7bits	7
		dts+=(pesHeader[17]<<7);								// 8bits	15
		dts+=((pesHeader[16]>>1)<<15);					// 7bits	22
		dts+=((pesHeader[15])<<22);							// 8bits	30
    UINT64 k=((pesHeader[14]>>1)&0x7);
    k <<=30LL;
		dts+=k;			// 3bits
		dts &= 0x1FFFFFFFFLL;
	
	}
	
	return (ptsAvailable||dtsAvailable);
}

void CPcrDecoder::ChangePtsDts(byte* header, UINT64 startPcr)
{
  if (header[0] !=0 || header[1] !=0  || header[2] !=1) return; 
  byte* pesHeader=header;
	UINT64 pts=0LL;
	UINT64 dts=0LL;
	if (!GetPtsDts(pesHeader, pts, dts)) 
	{
		return ;
	}
	if (pts>0LL)
	{
		UINT64 ptsorg=pts;
		if (pts > startPcr) 
			pts = (UINT64)( ((UINT64)pts) - ((UINT64)startPcr) );
		else pts=0LL;
		//LogDebug("pts: org:%x new:%x start:%x pid:%x", (DWORD)ptsorg,(DWORD)pts,(DWORD)startPcr,m_tsHeader.Pid);
		
		byte marker=0x21;
		if (dts!=0) marker=0x31;
		pesHeader[13]=(byte)((((pts&0x7f)<<1)+1)); pts>>=7;
		pesHeader[12]=(byte)( (pts&0xff));				  pts>>=8;
		pesHeader[11]=(byte)((((pts&0x7f)<<1)+1)); pts>>=7;
		pesHeader[10]=(byte)((pts&0xff));					pts>>=8;
		pesHeader[9]=(byte)( (((pts&7)<<1)+marker)); 
	}
	if (dts >0LL)
	{
		if (dts > startPcr) 
			dts = (UINT64)( ((UINT64)dts) - ((UINT64)startPcr) );
		else dts=0LL;
		pesHeader[18]=(byte)((((dts&0x7f)<<1)+1)); dts>>=7;
		pesHeader[17]=(byte)( (dts&0xff));				  dts>>=8;
		pesHeader[16]=(byte)((((dts&0x7f)<<1)+1)); dts>>=7;
		pesHeader[15]=(byte)((dts&0xff));					dts>>=8;
		pesHeader[14]=(byte)( (((dts&7)<<1)+0x11)); 
	}
}
