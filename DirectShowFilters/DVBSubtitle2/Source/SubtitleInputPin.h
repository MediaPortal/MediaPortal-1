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

#pragma once
#pragma warning(disable: 4511 4512 4995)

#include "DVBSub.h"
#include "dvbsubs\dvbsubdecoder.h"
#include "PesDecoder\PacketSync.h"
#include "PesDecoder\PesDecoder.h"

class CSubtitleInputPin : public CBaseInputPin,  
                          public CPacketSync, public CPesCallback
{
public:

  CSubtitleInputPin( CDVBSub *m_pDVBSub,
                LPUNKNOWN pUnk,
                CBaseFilter *pFilter,
                CCritSec *pLock,
                CCritSec *pReceiveLock,
                CDVBSubDecoder* pSubDecoder,
                HRESULT *phr );

  ~CSubtitleInputPin();

  // Do something with this media sample
  STDMETHODIMP Receive( IMediaSample *pSample );
  STDMETHODIMP ReceiveCanBlock();

  STDMETHODIMP BeginFlush( void );
  STDMETHODIMP EndFlush( void );

  HRESULT CheckMediaType( const CMediaType * );
  HRESULT CompleteConnect( IPin *pPin );
  HRESULT BreakConnect();

  void Reset();
  void SetSubtitlePid( LONG pPID );

  // From CPacketSync
  void OnTsPacket( byte* tsPacket );

  // From CPesCallback
  int OnNewPesPacket( int streamid,byte* header, int headerlen,byte* data, int len, bool isStart );

#ifdef _DEBUG
  STDMETHODIMP_(ULONG) AddRef();
  STDMETHODIMP_(ULONG) Release();
#endif

private:

  CDVBSubDecoder*   m_pSubDecoder;
  CPesDecoder*      m_pesDecoder;
  
  LONG  m_SubtitlePid;

  CDVBSub* const    m_pDVBSub;      // Main renderer object
  CCritSec * const	m_pReceiveLock; // Sample critical section
  CCritSec* const   m_Lock;	
  bool              m_bReset;
};
