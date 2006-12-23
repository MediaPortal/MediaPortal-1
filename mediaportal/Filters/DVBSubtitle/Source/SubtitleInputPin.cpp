/* 
 *	Copyright (C) 2006-2007 Team MediaPortal
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
#include <commdlg.h>
#include <xprtdefs.h>
#include <ksuuids.h>
#include <streams.h>
#include <bdaiface.h>
#include <commctrl.h>
#include <initguid.h>

#include "SubtitleInputPin.h"
#include "dvbsubs\dvbsubdecoder.h"

extern void LogDebug( const char *fmt, ... );

CSubtitleInputPin::CSubtitleInputPin( CDVBSub *pDVBSub,
										LPUNKNOWN pUnk,
										CBaseFilter *pFilter,
										CCritSec *pLock,
										CCritSec *pReceiveLock,
										CDVBSubDecoder* pSubDecoder,
										HRESULT *phr ) :

    CRenderedInputPin(NAME( "CSubtitleInputPin" ),
					pFilter,						    // Filter
					pLock,							    // Locking
					phr,							      // Return code
					L"In" ),				      	// Pin name
					m_pReceiveLock( pReceiveLock ),
					m_pDVBSub( pDVBSub ),
					m_pSubDecoder( pSubDecoder ),
					m_SubtitlePid( -1 ),
          m_pPin( NULL )
{
  m_pesDecoder = new CPesDecoder( this );
  
  Reset();
	LogDebug( "Subtitle: Input pin created" );
}

CSubtitleInputPin::~CSubtitleInputPin()
{
  delete m_pesDecoder;
}

HRESULT CSubtitleInputPin::CheckMediaType( const CMediaType *pmt )
{
  LogDebug("Subtitle: CSubtitleInputPin::CheckMediaType()");
  if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}


HRESULT CSubtitleInputPin::BreakConnect()
{
  return CRenderedInputPin::BreakConnect();
}

HRESULT CSubtitleInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  m_pPin = pPin;
  
  if( m_SubtitlePid == -1 )
    return hr;  // PID is mapped later when we have it 

  hr = MapPidToDemuxer( m_SubtitlePid, m_pPin, MEDIA_TRANSPORT_PACKET );

  m_pesDecoder->SetPid( m_SubtitlePid );

  return hr;
}


STDMETHODIMP CSubtitleInputPin::ReceiveCanBlock()
{
    return S_FALSE;
}


STDMETHODIMP CSubtitleInputPin::Receive( IMediaSample *pSample )
{
	CAutoLock lock(m_pReceiveLock);

	if( m_SubtitlePid == -1 )
    return S_OK;  // Nothing to be done yet

	if ( m_bReset )
	{
		LogDebug( "Subtitle: reset" );
		m_bReset = false;
	}
	CheckPointer( pSample, E_POINTER );

	PBYTE pbData = NULL;
	
	REFERENCE_TIME tStart, tStop;
	pSample->GetTime( &tStart, &tStop);
	long lDataLen = 0;
	HRESULT hr = pSample->GetPointer( &pbData );

  if( FAILED( hr ) ) 
	{
		LogDebug( "Subtitle: Receive() err" );
		return hr;
	}
	lDataLen = pSample->GetActualDataLength();

  OnRawData( pbData, lDataLen );
  
  return S_OK;
}

void CSubtitleInputPin::OnTsPacket( byte* tsPacket )
{
  m_pesDecoder->OnTsPacket( tsPacket );
}

int CSubtitleInputPin::OnNewPesPacket( int streamid, byte* header, int headerlen, 
                                       byte* data, int len, bool isStart )
{
  byte* pesData = NULL;
  pesData = (unsigned char*)malloc( headerlen + len ); 

  memcpy( pesData, header, headerlen );
  memcpy( pesData + headerlen, data, len );
    
  m_pSubDecoder->ProcessPES( pesData, headerlen + len, m_SubtitlePid );

  delete pesData;
  pesData = NULL;

  return 0;
}

void CSubtitleInputPin::Reset()
{
	m_bReset = true;
}

void CSubtitleInputPin::SetSubtitlePid( LONG pPid )
{
	m_SubtitlePid = pPid;
  MapPidToDemuxer( m_SubtitlePid, m_pPin, MEDIA_TRANSPORT_PACKET );
  m_pesDecoder->SetPid( m_SubtitlePid );
}

//
// EndOfStream
//
STDMETHODIMP CSubtitleInputPin::EndOfStream( void )
{
    CAutoLock lock( m_pReceiveLock );
    return CRenderedInputPin::EndOfStream();

} // EndOfStream

STDMETHODIMP CSubtitleInputPin::BeginFlush( void )
{
//	Reset();
	return CRenderedInputPin::BeginFlush();
}
STDMETHODIMP CSubtitleInputPin::EndFlush( void )
{
//	Reset();
	return CRenderedInputPin::EndFlush();
}

//
// NewSegment
//
STDMETHODIMP CSubtitleInputPin::NewSegment( REFERENCE_TIME tStart,
											REFERENCE_TIME tStop,
											double dRate )
{
    return S_OK;
} // NewSegment
