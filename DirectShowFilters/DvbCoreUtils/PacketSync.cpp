/* 
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include "..\shared\PacketSync.h"


CPacketSync::CPacketSync(void)
{
  m_tempBufferPos=-1;
}

CPacketSync::~CPacketSync(void)
{
}

void CPacketSync::Reset(void)
{
  m_tempBufferPos=-1;
}

// Ambass : Now, need to have 2 consecutive TS_PACKET_SYNC to try avoiding bad synchronisation.  
//          In case of data flow change ( Seek, tv Zap .... ) Reset() should be called first to flush buffer.
void CPacketSync::OnRawData(byte* pData, int nDataLen)
{
  int syncOffset=0;
  if (m_tempBufferPos > 0 )
  {
    if (pData[TS_PACKET_LEN - m_tempBufferPos]==TS_PACKET_SYNC)
    {
      syncOffset = TS_PACKET_LEN - m_tempBufferPos;
      if (syncOffset) memcpy(&m_tempBuffer[m_tempBufferPos], pData, syncOffset);
      OnTsPacket(m_tempBuffer);
    }
    m_tempBufferPos = 0;
  }

  while (syncOffset + TS_PACKET_LEN < nDataLen)
  {
    if ((pData[syncOffset] == TS_PACKET_SYNC) &&
        (pData[syncOffset + TS_PACKET_LEN]==TS_PACKET_SYNC))
    {
      OnTsPacket( &pData[syncOffset] );
      syncOffset += TS_PACKET_LEN;
    }
    else
      syncOffset++;
  }

  // Here we have less than 188+1 bytes
  while (syncOffset < nDataLen)
  {
    if (pData[syncOffset] == TS_PACKET_SYNC)
    {
      m_tempBufferPos= nDataLen - syncOffset;
      memcpy( m_tempBuffer, &pData[syncOffset], m_tempBufferPos );
      return ;
    }
    else
      syncOffset++;
  }

  m_tempBufferPos=0 ;
}

void CPacketSync::OnTsPacket(byte* tsPacket)
{
}
