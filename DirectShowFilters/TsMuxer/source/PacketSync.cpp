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
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include "PacketSync.h"

extern void LogDebug(const char *fmt, ...) ;


CPacketSync::CPacketSync(void)
{
	m_tempBufferPos=0; // bytes of a packet that wait for the rest
}

CPacketSync::~CPacketSync(void)
{
}

// [188]   [188]		 [188]		[100]				[88]
// 0..187  188..375	 376..563 564..663		0..87
// datalen=664
void CPacketSync::OnRawData(byte* pData, int nDataLen)
{
	if (pData==NULL || nDataLen<=0) return;

	int syncOffset=0;
	if (m_tempBufferPos > 0)
	{
		// finish the packet that was waiting, with as many bytes as there are
		int n = TS_PACKET_LEN - m_tempBufferPos;
		if (n > nDataLen) n = nDataLen;
		memcpy(&m_tempBuffer[m_tempBufferPos], pData, n);
		m_tempBufferPos += n;
		syncOffset = n;
		if (m_tempBufferPos < TS_PACKET_LEN) return; // still not complete
		m_tempBufferPos = 0;
		OnTsPacket(m_tempBuffer);
	}

	while (syncOffset + TS_PACKET_LEN <= nDataLen)
	{
		if (pData[syncOffset] != TS_PACKET_SYNC)
		{
			syncOffset++;
			continue;
		}
		OnTsPacket( &pData[syncOffset] );
		syncOffset += TS_PACKET_LEN;
	}

	// less than a packet is left: keep it from the first sync byte on
	while (syncOffset < nDataLen && pData[syncOffset] != TS_PACKET_SYNC)
		syncOffset++;
	if (syncOffset < nDataLen)
	{
		m_tempBufferPos = nDataLen - syncOffset;
		memcpy( m_tempBuffer, &pData[syncOffset], m_tempBufferPos );
	}
}

void CPacketSync::OnTsPacket(byte* tsPacket)
{
}
