/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: tourettes
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

//#include "MPDVBSub.h"
//#include "proppage.h"
#include "SubtitleInputPin.h"

// Subtitle decoding 
#include "dvbsubs\dvbsubs.h"

extern void Log( const char *fmt, ... );

CSubtitleInputPin::CSubtitleInputPin( CSubTransform *pDump,
										LPUNKNOWN pUnk,
										CBaseFilter *pFilter,
										CCritSec *pLock,
										CCritSec *pReceiveLock,
										CDVBSubDecoder* pSubDecoder,
										HRESULT *phr ) :

    CRenderedInputPin(NAME( "CSubtitleInputPin" ),
					pFilter,						  // Filter
					pLock,							  // Locking
					phr,							    // Return code
					L"Subtitle" ),				// Pin name
					m_pReceiveLock( pReceiveLock ),
          m_pDemuxerPin( NULL ),
					m_pDump( pDump ),
					m_pSubDecoder( pSubDecoder ),
					m_SubtitlePid( -1 )
{
  m_pesDecoder = new CPesDecoder( this );
  
  Reset();
	Log( "Subtitle: Input pin created" );
}

CSubtitleInputPin::~CSubtitleInputPin()
{
  delete m_pesDecoder;
}
//
// CheckMediaType
//
HRESULT CSubtitleInputPin::CheckMediaType( const CMediaType *pmt )
{
  Log("Subtitle: CSubtitleInputPin::CheckMediaType()");
  if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}

//
// BreakConnect
//
HRESULT CSubtitleInputPin::BreakConnect()
{
  return CRenderedInputPin::BreakConnect();
}

HRESULT CSubtitleInputPin::CompleteConnect( IPin *pPin )
{
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  m_pDemuxerPin = pPin;

  if( m_SubtitlePid == -1 )
    return hr;  // PID is mapped later when we have it 

  hr = MapPidToDemuxer( m_SubtitlePid, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );

  m_pesDecoder->SetPid( m_SubtitlePid );

  return hr;
}

//
// ReceiveCanBlock
//
// We don't hold up source threads on Receive
//
STDMETHODIMP CSubtitleInputPin::ReceiveCanBlock()
{
    return S_FALSE;
}


//
// Receive
//
STDMETHODIMP CSubtitleInputPin::Receive( IMediaSample *pSample )
{
	CAutoLock lock(m_pReceiveLock);

	if( m_SubtitlePid == -1 )
    return S_OK;  // Nothing to be done yet

	if ( m_bReset )
	{
		Log( "Subtitle: reset" );
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
		Log( "Subtitle: Receive() err" );
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
  MapPidToDemuxer( m_SubtitlePid, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );
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
