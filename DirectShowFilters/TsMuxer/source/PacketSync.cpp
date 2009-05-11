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
#include "PacketSync.h"

extern void LogDebug(const char *fmt, ...) ;


CPacketSync::CPacketSync(void)
{
	m_tempBufferPos=-1;
}

CPacketSync::~CPacketSync(void)
{
}

// [188]   [188]		 [188]		[100]				[88]
// 0..187  188..375	 376..563 564..663		0..87
// datalen=664
void CPacketSync::OnRawData(byte* pData, int nDataLen)
{
	int syncOffset=0;
	if (m_tempBufferPos >=0 )
	{
		syncOffset = TS_PACKET_LEN - m_tempBufferPos;
		memcpy(&m_tempBuffer[m_tempBufferPos], pData, syncOffset);
		OnTsPacket(m_tempBuffer);
		m_tempBufferPos = 0;
	}

	while (syncOffset < nDataLen)
	{
		if (syncOffset + TS_PACKET_LEN > nDataLen) break;
		if (pData[syncOffset] != TS_PACKET_SYNC) 
		{
			syncOffset++;
			continue;
		}
		OnTsPacket( &pData[syncOffset] );
		syncOffset += TS_PACKET_LEN;
	}

	if (syncOffset < nDataLen)
	{
		m_tempBufferPos= nDataLen - syncOffset;
		memcpy( m_tempBuffer, &pData[syncOffset], m_tempBufferPos );
	}
}

void CPacketSync::OnTsPacket(byte* tsPacket)
{
}
