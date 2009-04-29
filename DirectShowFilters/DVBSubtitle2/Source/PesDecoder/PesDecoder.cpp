/* 
*	Copyright (C) 2006-2009 Team MediaPortal
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
#include <stdio.h>
#include "pesdecoder.h"
#include "TsHeader.h"
#include "packetsync.h"
#include <cassert>

const int PADDING_STREAM_ID = 190;

extern void LogDebug( const char *fmt, ... );

CPesDecoder::CPesDecoder( CPesCallback* callback )
{
  //LogDebug("pes decoder ctor");
  m_pid = -1;
  m_pesBuffer = new byte[MAX_PES_PACKET];
  m_iWritePos = 0;
  m_pCallback = callback;
  m_iStreamId = -1;
  m_iPesHeaderLen = 0;
  m_iPesLength = 0;
}

CPesDecoder::~CPesDecoder( void )
{
  delete[] m_pesBuffer; 
}

void CPesDecoder::Reset()
{
  m_iWritePos = 0;
  m_iPesHeaderLen = 0;
}

int CPesDecoder::GetPid()
{
  return m_pid;
}

void CPesDecoder::SetPid( int pid )
{
  m_pid = pid;
}

void CPesDecoder::SetStreamId( int streamId )
{
  m_iStreamId=streamId;
}
int	CPesDecoder::GetStreamId()
{
  return m_iStreamId;
}

bool CPesDecoder::OnTsPacket( byte* tsPacket )
{
  //LogDebug("PesDecoder::OnTsPacket %i", tsPacketCount++);
  if (tsPacket==NULL)
  {
    LogDebug( "tsPacket null!" );	
    return false;
  }

  assert( m_pid >= 0 );

  CTsHeader header( tsPacket );
  if ( header.Pid != m_pid )
  {
    LogDebug( "Header Pid is %i, expected %i", header.Pid, m_pid );	
    return false;
  }
  if (header.SyncByte != TS_PACKET_SYNC ) 
  {
    LogDebug( "pesdecoder pid:%x sync error", m_pid );
    return false;
  }
  if (header.TransportError) 
  {
    m_bStart = false;
    m_iWritePos = 0;
    m_iPesLength = 0;
    LogDebug( "pesdecoder pid:%x transport error", m_pid );
    return false;
  }

  BOOL scrambled = ( header.TScrambling != 0 );
  if ( scrambled )
  {
    LogDebug("pesdecoder scrambled!");
    return false; 
  }
  if ( header.AdaptionFieldOnly() ) 
  {
    LogDebug("pesdecoder AdaptionFieldOnly!");
    return false;
  }

  //LogDebug("Ts packet count %i", ++tsPacketCount);

  int pos = header.PayLoadStart; // where in the TS packet does the payload data start?
  bool result = false;

  // if this header starts a new PES packet
  if (header.PayloadUnitStart) 
  {
    if ( tsPacket[pos+0] == 0 && tsPacket[pos+1] == 0 && tsPacket[pos+2] == 1 )
    {
      if ( m_iStreamId < 0 )
      {
        LogDebug("PES decoder - no stream ID set!");
      }
      else 
      {
        int streamId = tsPacket[pos+3];
        // Stream ID should be the same or padding stream id
        if( streamId != m_iStreamId )
        {
          if( streamId != PADDING_STREAM_ID )
          {
            LogDebug("PES decoder - wrong stream ID received! - %d", streamId );
          }
          return false;
        }
      }

    if( m_iWritePos != 0 )
    {
      LogDebug("PES decoder - start of the new PES packet received before the previuos packet was complete! Dropped data?");
      Reset();
      // do not return here so that the next PES packet is decoded from the beginning
    }

    m_iPesHeaderLen = tsPacket[pos+8] + 9;
    memcpy( m_pesHeader, &tsPacket[pos], m_iPesHeaderLen );
    pos += m_iPesHeaderLen;
    m_bStart = true;

    // calculate expected actual payload length
    m_iPesLength = ( m_pesHeader[4] << 8 ) + m_pesHeader[5] - ( m_iPesHeaderLen - 6); 
    //LogDebug("  PES decoder - PES lenght %d", m_iPesLenght );
    }
  }

  if (m_iWritePos < 0)
  {
    LogDebug("m_iWritePos < 0");
    return false;
  }
  if (m_iStreamId <= 0)
  {
    LogDebug("m_iStreamId <= 0");
    return false;
  }

  //LogDebug("%i : adaption field length, data start %i", header.AdaptionFieldLength, header.PayLoadStart);
  assert(m_iWritePos + 188-pos <= MAX_PES_PACKET );

  if( ( 188 - pos < 0 ) )
  {
    LogDebug( "sanity check failed! error in the stream? m_iWritePos %d pos %d", m_iWritePos, pos );
    Reset();
    return false;
  }

  memcpy( &m_pesBuffer[m_iWritePos], &tsPacket[pos], 188 - pos );
  //LogDebug("Copied %i bytes of pes data into buffer ", 188-pos);

  m_iWritePos += ( 188 - pos );

  //LogDebug("write pos %i", m_iWritePos);
  //LogDebug(" pes %x copy:%x len:%x maxlen:%x start:%d", m_iStreamId,m_iWritePos,(188-pos),m_iMaxLength,m_bStart);
  //LogDebug( "m_iPesLenght %d  m_iWritePos %d  diff is %d", m_iPesLenght, m_iWritePos, m_iPesLenght - m_iWritePos );

  if ( m_iPesLength == m_iWritePos  ) // we have the expected data
  {
    if ( m_iWritePos>0 && m_pCallback != NULL )
      {
      //LogDebug(" pes %x start:%x", m_iStreamId,m_iWritePos);
      int written = m_pCallback->OnNewPesPacket( m_iStreamId, m_pesHeader, m_iPesHeaderLen, m_pesBuffer, m_iWritePos, m_bStart);
      assert( written == 0 ); // we dont use the return value

      if ( written >= 0 )
      {
        //LogDebug(" pes %x written:%x", m_iStreamId,written);
        m_bStart = false;
        m_iWritePos = 0;
      }
    }
  }
  return result;
}
