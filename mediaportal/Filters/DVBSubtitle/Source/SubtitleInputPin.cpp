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
#include <initguid.h>

#include "SubtitleInputPin.h"
#include "dvbsubs\dvbsubdecoder.h"

extern void LogDebug( const char *fmt, ... );


//
// Constructor
//
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


//
// Destructor
//
CSubtitleInputPin::~CSubtitleInputPin()
{
  delete m_pesDecoder;
}


//
// CheckMediaType
//
HRESULT CSubtitleInputPin::CheckMediaType( const CMediaType *pmt )
{
  if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
    LogDebug("Subtitle: CSubtitleInputPin::CheckMediaType() - found MEDIASUBTYPE_MPEG2_TRANSPORT");
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


//
// CompleteConnect
//
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


//
// ReceiveCanBlock
//
STDMETHODIMP CSubtitleInputPin::ReceiveCanBlock()
{
  return S_OK;
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
		LogDebug( "SubtitlePin: reset" );
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


//
// OnTsPacket
//
void CSubtitleInputPin::OnTsPacket( byte* tsPacket )
{
  m_pesDecoder->OnTsPacket( tsPacket );
}


//
// OnNewPesPacket
//
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


//
// Reset
//
void CSubtitleInputPin::Reset()
{
	m_bReset = true;
	m_pesDecoder->Reset();
}


//
// SetSubtitlePid
//
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
}


//
// BeginFlush
//
STDMETHODIMP CSubtitleInputPin::BeginFlush( void )
{
	return CRenderedInputPin::BeginFlush();
}


//
// EndFlush
//
STDMETHODIMP CSubtitleInputPin::EndFlush( void )
{
  m_pDVBSub->NotifySeeking();
	return CRenderedInputPin::EndFlush();
}

//
// NewSegment
//
STDMETHODIMP CSubtitleInputPin::NewSegment( REFERENCE_TIME tStart,
											REFERENCE_TIME tStop,
											double dRate )
{
  return CRenderedInputPin::NewSegment( tStart, tStop, dRate );
}
