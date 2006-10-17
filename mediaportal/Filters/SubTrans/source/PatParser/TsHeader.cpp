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
#include "TsHeader.h" 
#include "packetsync.h"

extern void Log(const char *fmt, ...) ;

CTsHeader::CTsHeader(byte* tsPacket)
{
	m_packet=tsPacket;
	Decode(tsPacket);
}

CTsHeader::~CTsHeader(void)
{
}

bool CTsHeader::PayLoadOnly()
{
	return (AdaptionControl==1);
}

bool CTsHeader::AdaptionFieldOnly()
{
	return (AdaptionControl==2);
}

bool CTsHeader::AdaptionFieldAndPayLoad()
{
	return (AdaptionControl==3);
}


void CTsHeader::Decode(byte *data)
{
	//															bits  byteNo		mask
	//SyncByte											:	8			0				
	//TransportError								:	1			1				0x80  10000000
	//PayloadUnitStart							: 1			1				0x40  01000000
	//TransportPriority							: 1			1				0x20  00100000
	//Pid														: 13	 1&2	          00011111 11111111
	//Transport Scrambling Control	: 2			3				0xc0  11000000
	//Adaption Field Control				: 2			3				0x30	00110000
	//ContinuityCounter							: 4			3				0xf   00001111
	
	//Two adaption field control bits which may take four values:
  // 1. 01 – no adaptation field, payload only				0x10		1
  // 2. 10 – adaptation field only, no payload				0x20		2
  // 3. 11 – adaptation field followed by payload			0x30		3
  // 4. 00 - RESERVED for future use 									0x00

	SyncByte=data[0];
	TransportError=(data[1] & 0x80)>0?true:false;
	PayloadUnitStart=(data[1] & 0x40)>0?true:false;
	TransportPriority=(data[1] & 0x20)>0?true:false;
	Pid=((data[1] & 0x1F) <<8)+data[2];
	TScrambling=data[3] & 0xC0;
	AdaptionControl=(data[3]>>4) & 0x3;
	ContinuityCounter=data[3] & 0x0F;
	AdaptionFieldLength=0;
	PayLoadStart=4;
	if (AdaptionControl >=2 ) 
	{
		AdaptionFieldLength=data[4];
		PayLoadStart=5+AdaptionFieldLength;
	}
}

void CTsHeader::LogHeader()
{
	Log("tsheader:%02.2x%02.2x%02.2x%02.2x%02.2x%02.2x%02.2x%02.2x%02.2x%02.2x",
					m_packet[0],m_packet[1],m_packet[2],m_packet[3],m_packet[4],m_packet[5],m_packet[6],m_packet[7],m_packet[8],m_packet[9]);
	Log("  SyncByte           :%x", SyncByte);
	Log("  TransportError     :%x", TransportError);
	Log("  PayloadUnitStart   :%d", PayloadUnitStart);
	Log("  TransportPriority  :%x", TransportPriority);
	Log("  Pid                :%x", Pid);
	Log("  TScrambling        :%x", TScrambling);
	Log("  AdaptionControl    :%x", AdaptionControl);
	Log("  ContinuityCounter  :%x", ContinuityCounter);
	Log("  AdaptionFieldLength:%d", AdaptionFieldLength);
	Log("  PayLoadStart       :%d", PayLoadStart);
	Log("  PayLoadOnly            :%d", PayLoadOnly());
	Log("  AdaptionFieldOnly      :%d", AdaptionFieldOnly());
	Log("  AdaptionFieldAndPayLoad:%d", AdaptionFieldAndPayLoad());
}