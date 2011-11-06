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
  if(!m_streamTypeDetected)
  {
    if( !AnalyzeStream(pData, nDataLen) )
    {
      // unable to detect between 188 / 192 byte packets
      // need more data...
      return;
    }
  }
  
  int syncOffset=0;
  if (m_tempBufferPos > 0 )
  {
    if (pData[m_packetLen - m_tempBufferPos]==TS_PACKET_SYNC)
    {
      syncOffset = m_packetLen - m_tempBufferPos;// - m_syncbyteOffset;
      if (syncOffset) memcpy(&m_tempBuffer[m_tempBufferPos], pData, syncOffset);
      OnTsPacket(m_tempBuffer);
    }
    m_tempBufferPos = 0;
  }

  while (syncOffset + m_packetLen < nDataLen)
  {
    if ((pData[syncOffset] == TS_PACKET_SYNC) &&
        (pData[syncOffset + m_packetLen]==TS_PACKET_SYNC))
    {
      OnTsPacket( &pData[syncOffset/*+m_syncbyteOffset*/] );
      syncOffset += m_packetLen;
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

bool CPacketSync::AnalyzeStream(byte* pData, int nDataLen)
{
  int count_188=0;
  int count_192=0;
  int corrupted=0;
  int syncOffset=0;

  while (syncOffset < nDataLen)
  {
    if (syncOffset + TS_PACKET_LEN > nDataLen) 
    {
      break;
    }
    if (pData[syncOffset] == TS_PACKET_SYNC) 
    {
      if(pData[syncOffset+MT2S_PACKET_LEN] == TS_PACKET_SYNC)
      {
        count_192++;
        syncOffset += MT2S_PACKET_LEN -1;
      }
      else if(pData[syncOffset+TS_PACKET_LEN] == TS_PACKET_SYNC)
      {
        count_188++;
        syncOffset += TS_PACKET_LEN -1;
      }
      else
      {
        corrupted++;
      }
    }
    syncOffset++;
  }

  // allow 10% amount of corrupted packets
  if( corrupted < count_188/10 || corrupted < count_192/10)
  {
    if( count_188 > count_192)
    {
      m_packetLen=TS_PACKET_LEN;
      m_syncbyteOffset=0;
    }
    else
    {
      m_packetLen=MT2S_PACKET_LEN;
      m_syncbyteOffset=4; // MT2S has four byte timestamp before TS sync byte
    }
    m_streamTypeDetected = true;
    return true;
  }
  return false;
}

