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
#include <math.h>
#include "..\shared\pcr.h" 
#define MAX_CLOCK  95443.71768

extern void LogDebug(const char *fmt, ...) ;

/*****************
*The TS packets contain clock synchronization information in fields called the Program Reference Clock (PCR), 
*Decoding Time Stamp, Presentation time Stamp (PTS). During the video transmission it is important 
*that the transmitter and receiver maintain synchronization and frame rate. Subtle variation in the 
*clock frequency used at the encoder/transrriitter and the decoder/receiver can lead to overflowing 
*or empty buffers-especially because of the high data rates involved with video material. To avoid 
*this, the PCR field is used to synchronize a 27 MHz master clock at both ends.
*
*The clock consists of a 42-bit counter that increments at 27 MHz (with the upper 33 bits incrementing 
*at a 90 kHz rate). The PCR field is used to transmit periodic samples of this counter. 
*The receiver can compare its counter with the received values and a simple phase locked 
*loop (PLL) circuit can be used to adjust the local rate to accurately match the transmitter. 
*This clock can be used with the frame time stamps that specify when a frame needs to be displayed, 
*to buffer and smooth jitter that may have occurred when the MPEG stream was transported through 
*variable delay (e.g. ATM switch buffering). These timestamps can also be used for effects such as 
*slow motion and pausing the video (e.g. VCR-like control).
*
* The PCR is split in 2 parts:
* the base is the upper 33 bits, incrementing at 90Khz, each tick=1/90Khz
* the extension is the lower 9 bits, incrementing at 27Mhz
******************/

CPcr::CPcr()
{
  Reset();
}
CPcr::CPcr(const CPcr& pcr)
{
  PcrReferenceBase=pcr.PcrReferenceBase;
  PcrReferenceExtension=pcr.PcrReferenceExtension;
	IsValid=pcr.IsValid;
}
CPcr::~CPcr(void)
{
}

void CPcr::Reset()
{
	IsValid=false;
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
	IsValid=true;
}

bool CPcr::DecodeFromPesHeader(byte* pesHeader,int payloadStart,CPcr& pts, CPcr& dts)
{
  pts.Reset();
	dts.Reset();
	bool ptsAvailable=false;
	bool dtsAvailable=false;

	if (payloadStart+8>187) return false;

	if (((pesHeader[6]&0xC0)==0x80) && ((pesHeader[7]&0x80)!=0)) 
	{
		ptsAvailable=true;
		if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	}
	if (ptsAvailable)
	{	
		if (payloadStart+13>187) return false;
		//if (PTS_DTS_flags =='10' ) {
    //  '0010'        4 bslbf       0010                9
    //  PTS [32..30]  3 bslbf           111             9
    //  marker_bit    1 bslbf              1            9
    //  PTS [29..15] 15 bslbf       001100011001000    10/11
    //  marker_bit    1 bslbf                         1 11
    //  PTS [14..0]  15 bslbf       110000111101011    12/13
    //  marker_bit    1 bslbf                         1 13        | 111001100011001000110000111101011 = 1CC6461EB
    //  }
		// 9       10        11        12      13
		//76543210 76543210 76543210 76543210 76543210
		//0011pppM pppppppp pppppppM pppppppp pppppppM 
	  UINT64 ptsTicks=0LL;
		UINT64 k;
		k=((pesHeader[9]>>1)&0x7); k <<=30; ptsTicks+=k;      //9: 00101111
		k=  pesHeader[10];				 k <<=22; ptsTicks+=k;      //10:00110001
		k= (pesHeader[11]>>1);		 k <<=15; ptsTicks+=k;      //11:10010001
		k=  pesHeader[12];				 k <<=7;  ptsTicks+=k;      //12:11000011
		k= (pesHeader[13]>>1);		          ptsTicks+=k;      //13:11010111
    pts.PcrReferenceBase = ptsTicks;
		pts.IsValid=true;
	}

	if (dtsAvailable)
	{
		if (payloadStart+18>187) return false;
		// 14       15        16        17      18
		//76543210 76543210 76543210 76543210 76543210
		//0001dddM dddddddd dddddddM dddddddd dddddddM 
	  UINT64 dtsTicks=0LL;
		UINT64 k;
		k=((pesHeader[14]>>1)&0x7); k <<=30; dtsTicks+=k;
		k=  pesHeader[15];				  k <<=22; dtsTicks+=k;
		k= (pesHeader[16]>>1);		  k <<=15; dtsTicks+=k;
		k=  pesHeader[17];				  k <<=7;  dtsTicks+=k;
		k= (pesHeader[18]>>1);		           dtsTicks+=k;
    dts.PcrReferenceBase = dtsTicks;
		dts.IsValid=true;
	}
	
	return (ptsAvailable||dtsAvailable);
}

//***********************************
//* convert from clock(in seconds) to pcr 
//***********************************
void CPcr::FromClock(double clock)
{
  double khz90Ticks = clock / ((1.0/90000.0));
  PcrReferenceBase = (UINT64)(fabs(khz90Ticks));

  clock -= (PcrReferenceBase*((1.0/90000.0)));
  double mhz27Ticks= clock / ((1.0/27000000.0));

  PcrReferenceExtension=(UINT64)(fabs(mhz27Ticks));
}

double CPcr::ToClock() const
{
  double clock = ((double)(PcrReferenceBase)) / 90000.0;
  clock += ((double)PcrReferenceExtension) / 27000000.0;
  
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
CPcr  CPcr::operator+(const CPcr &rhs) 
{    
  return (CPcr(*this) += rhs);
}
CPcr  CPcr::operator-(const CPcr &rhs) 
{
  return (CPcr(*this) -= rhs);      
}
CPcr&  CPcr::operator=(const CPcr &rhs) 
{
  if (this == &rhs)     
      return *this;      
  PcrReferenceBase=rhs.PcrReferenceBase;
  PcrReferenceExtension=rhs.PcrReferenceExtension;
	IsValid=rhs.IsValid;
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
  sprintf(m_buffer,"%d days %02.2d:%02.2d:%02.2d %d", day,hour,minutes,seconds,millsecs);
  return m_buffer;
}
