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

#pragma once
#pragma warning( disable: 4995 )

#include "DvbSub.h"
#include "PatParser\PacketSync.h"
#include <streams.h>
#include <vector>

class CSubtitleInputPin;
class CPcrInputPin;
class CPatParser;

class CPMTInputPin : public CBaseInputPin, CPacketSync
{
public:

  CPMTInputPin( CDVBSub *pSubFilter,
				LPUNKNOWN pUnk,
				CBaseFilter *pFilter,
				CCritSec *pLock,
				CCritSec *pReceiveLock,
				HRESULT *phr,
        CSubtitleInputPin *pSubtitlePin,
        CPcrInputPin *pPCRPin );

  ~CPMTInputPin();

  STDMETHODIMP Receive( IMediaSample *pSample );
  STDMETHODIMP BeginFlush( void );
  STDMETHODIMP EndFlush( void );
  STDMETHODIMP ReceiveCanBlock();

  HRESULT CheckMediaType( const CMediaType * );
  HRESULT CompleteConnect( IPin *pPin );
  
  // From CPacketSync
  void OnTsPacket( byte* tsPacket );

  void SetVideoPid( int videoPid );
  IPin* GetDemuxerPin();
  void Reset();

private:
  
  HRESULT Process( BYTE *pbData, long len );

  CPatParser* m_pPatParser;
  IPin*       m_pDemuxerPin;

  CSubtitleInputPin*  m_pSubtitlePin;
  CPcrInputPin*       m_pPCRPin;

  LONG m_streamVideoPid;
  LONG m_subtitlePid;
  LONG m_pcrPid;

  std::vector<int> mappedPids;

  CDVBSub* const	  m_pFilter;		  	  // Main renderer object
  CCritSec * const  m_pReceiveLock;			// Sample critical section
  bool				      m_bReset;
  int               m_sampleCount;
};
