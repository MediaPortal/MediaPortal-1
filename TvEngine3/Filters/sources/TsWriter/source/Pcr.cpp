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
#include "pcr.h" 
#define MAX_CLOCK  95443.71768
CPcr::CPcr()
{
  Reset();
}
CPcr::CPcr(const CPcr& pcr)
{
  PcrReferenceBase=pcr.PcrReferenceBase;
  PcrReferenceExtension=pcr.PcrReferenceExtension;
}
CPcr::~CPcr(void)
{
}

void CPcr::Reset()
{
  PcrReferenceBase=0LL;
  PcrReferenceExtension=0LL;
}

void CPcr::Decode(byte* data)
{
  // pcr reference base       33 bits  [0]      [1]        [2]     [3]     [4]     [5] 
  // reserved                  6 bits pppppppp-pppppppp-pppppppp-pppppppp-prrrrrre-eeeeeeee
  // pcr reference extension   9 bits
  PcrReferenceBase=0LL;
  UINT64 k=data[0];  k<<=25LL;PcrReferenceBase +=k;  // bit 25-32 
  k=data[1];         k<<=17LL;PcrReferenceBase +=k;  // bit 17-24
  k=data[2];         k<<=9LL; PcrReferenceBase +=k;  // bit 9-16
  k=data[3];         k<<=1LL; PcrReferenceBase +=k;  // bit 1-8
  k=((data[4]>>7)&0x1);       PcrReferenceBase +=k;  // bit 0

  PcrReferenceExtension=0;
  k=(data[4]& 0x1);  k<<=8LL;PcrReferenceExtension+=k; // bit 8
  k=data[5];                 PcrReferenceExtension+=k; // bit 0-7 
}

bool CPcr::DecodeFromPesHeader(byte* pesHeader, CPcr& pts, CPcr& dts)
{
  pts.Reset();
	dts.Reset();
	bool ptsAvailable=false;
	bool dtsAvailable=false;
	if ( (pesHeader[7]&0x80)!=0) ptsAvailable=true;
	if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	if (ptsAvailable)
	{	
	  UINT64 ptsTicks=0LL;
		ptsTicks += ((pesHeader[13]>>1)&0x7f);				// 7bits	7
		ptsTicks +=(pesHeader[12]<<7);								// 8bits	15
		ptsTicks +=((pesHeader[11]>>1)<<15);					// 7bits	22
		ptsTicks +=((pesHeader[10])<<22);							// 8bits	30
    UINT64 k=((pesHeader[9]>>1)&0x7);
    k <<=30LL;
		ptsTicks += k;			// 3bits
		ptsTicks &= 0x1FFFFFFFFLL;
    pts.PcrReferenceBase = ptsTicks;
	}

	if (dtsAvailable)
	{
	  UINT64 dtsTicks=0LL;
		dtsTicks = (pesHeader[18]>>1);								// 7bits	7
		dtsTicks +=(pesHeader[17]<<7);								// 8bits	15
		dtsTicks +=((pesHeader[16]>>1)<<15);					// 7bits	22
		dtsTicks +=((pesHeader[15])<<22);							// 8bits	30
    UINT64 k=((pesHeader[14]>>1)&0x7);
    k <<=30LL;
		dtsTicks+=k;			// 3bits
		dtsTicks &= 0x1FFFFFFFFLL;
    dts.PcrReferenceBase = dtsTicks;
	}
	
	return (ptsAvailable||dtsAvailable);
}

void CPcr::FromClock(double clock)
{
  while (clock > MAX_CLOCK)
  {
    clock -= MAX_CLOCK;
  }
  while (clock < MAX_CLOCK)
  {
    clock += MAX_CLOCK;
  }
  UINT64 ticks= floor(clock);
  PcrReferenceBase=ticks*90000LL;
  PcrReferenceExtension=(UINT64)( ( clock-floor(clock) )*27000000.0);
}

double CPcr::ToClock() const
{
  double clock = ((double)(PcrReferenceBase)) / 90000.0;
  clock += ((double)PcrReferenceExtension) / 27000000.0;
  
  while (clock > MAX_CLOCK)
  {
    clock -= MAX_CLOCK;
  }
  while (clock < 0)
  {
    clock += MAX_CLOCK;
  }
  return clock;
}
void CPcr::Time(int& day,int& hour, int &minutes, int& seconds, int & millsecs)
{
  double clock=ToClock();
  day = clock/86400;
  clock-=day*86400;
  hour = clock/3600;
  clock-=hour*3600;
  minutes=clock/60;
  clock-=minutes*60;
  seconds=floor(clock);
  millsecs=(clock-seconds)*1000;
}

CPcr & CPcr::operator+=(const CPcr &rhs) 
{
  double clockAdd=rhs.ToClock();
  double clock=ToClock();
  clock += clockAdd;
  FromClock(clock);
  return *this;
}
CPcr & CPcr::operator-=(const CPcr &rhs) 
{
  double clockAdd=rhs.ToClock();
  double clock=ToClock();
  clock -= clockAdd;
  FromClock(clock);
  return *this;
}
CPcr & CPcr::operator+(const CPcr &rhs) 
{    
  return (CPcr(*this) += rhs);
}
CPcr & CPcr::operator-(const CPcr &rhs) 
{
  return (CPcr(*this) -= rhs);      
}
CPcr & CPcr::operator=(const CPcr &rhs) 
{
  if (this == &rhs)     
      return *this;      
  PcrReferenceBase=rhs.PcrReferenceBase;
  PcrReferenceExtension=rhs.PcrReferenceExtension;

  return *this;
}
bool CPcr::operator==(const CPcr &other) const 
{
  return (PcrReferenceBase==other.PcrReferenceBase && PcrReferenceExtension==other.PcrReferenceExtension);
}
bool CPcr::operator!=(const CPcr &other) const 
{
  return !(*this == other);
}
bool CPcr::operator>(const CPcr &other) const 
{
  double clock1=ToClock();
  double clock2=other.ToClock();
  return (clock1>clock2);
}
char* CPcr::ToString()
{
  int day, hour,  minutes,  seconds,  millsecs;
  Time(day, hour,minutes, seconds,  millsecs);
  sprintf(m_buffer,"%d days %02.2d:%02.2d:%02.2d", day,hour,minutes,seconds);
  return m_buffer;
}