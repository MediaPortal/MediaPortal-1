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

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "pcrDecoder.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

CPcrDecoder::CPcrDecoder(MultiFileReader& reader)
:m_reader(reader)
{
}
CPcrDecoder::~CPcrDecoder(void)
{
}
double CPcrDecoder::GetPcr(bool fromEnd)
{
	byte buffer[8192];

	ULONG bytesRead;
	__int64 pos=m_reader.GetFilePointer();
	m_reader.Read(buffer,sizeof(buffer),&bytesRead);
	if (bytesRead==0) return 0;
	if (fromEnd)
	{
		for (int i=bytesRead-1;i>4;i--)
		{
			if (buffer[i]==0xba && buffer[i-1]==1 && buffer[i-2]==0 && buffer[i-3]==0)
			{
				i-=3;
				return GetPcr(&buffer[i]);
			}
		}
	}
	else
	{
		for (int i=0; i < (int)(bytesRead-4);++i)
		{
			if (buffer[i]==0 && buffer[i+1]==0 && buffer[i+2]==1 && buffer[i+3]==0xba)
			{
				return GetPcr(&buffer[i]);
			}
		}
	}
	return 0LL;
}


double CPcrDecoder::GetPcr(byte* buffer)
{
	__int64 pcrHi;
	__int64 pcrLo;
	pcrLo =  (buffer[9] >> 1);
	pcrLo +=( (buffer[8] & 0x3) << 7 );

	pcrHi =  (buffer[8]>>3);							//5 bits
	pcrHi += (buffer[7]<<5);							//8 bits
	pcrHi += ((buffer[6]&0x3)<<13);			  //2 bits
	pcrHi += ((buffer[6]>>3)<<15);				//5 bits
	pcrHi += ((buffer[5])<<20);           //8 bits
	pcrHi += ((buffer[4]&0x3)<<28);			  //8 bits
	pcrHi += (((buffer[4]>>3)&0x7)<<30);	//3 bits

	double interval=1.0/90000.0;
	double dTime=(pcrHi*interval)*1000.0;
	dTime += ((pcrLo*interval)/100.0);
	return dTime;
}

bool CPcrDecoder::GetPtsDts(byte* pesHeader, double& pts, double& dts)
{
	pts=0.0;
	dts=0.0;
	bool ptsAvailable=false;
	bool dtsAvailable=false;
	if ( (pesHeader[7]&0x80)!=0) ptsAvailable=true;
	if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	if (ptsAvailable)
	{
		__int64 pcr;
		pcr= (pesHeader[13]>>1);								// 7bits	7
		pcr+=(pesHeader[12]<<7);								// 8bits	15
		pcr+=((pesHeader[11]>>1)<<15);					// 7bits	22
		pcr+=((pesHeader[10])<<22);							// 8bits	30
		pcr+=(((pesHeader[9]>>1)&0x7)<<30);			// 3bits
	
		double interval=1.0/90000.0;
		pts=(pcr*interval)*1000.0;
	}
	if (dtsAvailable)
	{
		__int64 pcr;
		pcr= (pesHeader[18]>>1);								// 7bits	7
		pcr+=(pesHeader[17]<<7);								// 8bits	15
		pcr+=((pesHeader[16]>>1)<<15);					// 7bits	22
		pcr+=((pesHeader[15])<<22);							// 8bits	30
		pcr+=(((pesHeader[14]>>1)&0x7)<<30);			// 3bits
	
		double interval=1.0/90000.0;
		dts=(pcr*interval)*1000.0;
	}
	return (ptsAvailable||dtsAvailable);
}