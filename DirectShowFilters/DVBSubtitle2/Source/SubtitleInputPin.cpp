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
#include <xprtdefs.h>
#include <bdaiface.h>

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

    CBaseInputPin(NAME( "CSubtitleInputPin" ),
          pFilter,                       // Filter
          pLock,                         // Locking
          phr,                           // Return code
          L"In" ),                       // Pin name
          m_pReceiveLock( pReceiveLock ),
          m_pDVBSub( pDVBSub ),
          m_pSubDecoder( pSubDecoder ),
          m_SubtitlePid( -1 ),
          m_Lock( pLock )
{
  m_pesDecoder = new CPesDecoder( this );
  m_pesDecoder->SetPid( -1 );
  m_pesDecoder->SetStreamId( 0xBD ); // PES private stream

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
  if( ( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT ) &&
      ( pmt->majortype == MEDIATYPE_Stream ) )
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
  return CBaseInputPin::BreakConnect();
}


//
// CompleteConnect
//
HRESULT CSubtitleInputPin::CompleteConnect( IPin *pPin )
{
  HRESULT hr = CBasePin::CompleteConnect( pPin );
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
  CAutoLock lock( m_pReceiveLock );
  //LogDebug( "CSubtitleInputPin::Receive" ); 
  HRESULT hr = CBaseInputPin::Receive( pSample );
  if( hr != S_OK ) 
  {
    LogDebug( "CSubtitleInputPin::Receive - BaseInputPin ignored the sample!" ); 
    return hr;
  }

  if( m_SubtitlePid == -1 )
  {
    return S_OK;  // Nothing to be done yet
  }

  CheckPointer( pSample, E_POINTER );
  PBYTE pbData = NULL;

  long lDataLen = 0;
  hr = pSample->GetPointer( &pbData );

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
//  LogDebug(" new TS packet received");
  m_pesDecoder->OnTsPacket( tsPacket );
}


//
// OnNewPesPacket
//
int CSubtitleInputPin::OnNewPesPacket( int streamid, byte* header, int headerlen,
                                       byte* data, int len, bool isStart )
{
  //LogDebug( "CSubtitleInputPin::OnNewPesPacket" ); 
  
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
  m_pesDecoder->SetPid( m_SubtitlePid );
}


//
// BeginFlush
//
STDMETHODIMP CSubtitleInputPin::BeginFlush( void )
{
  LogDebug( "CSubtitleInputPin::BeginFlush" );
  HRESULT hr = CBaseInputPin::BeginFlush();
  CAutoLock lock_it( m_Lock );
  LogDebug( "CSubtitleInputPin::BeginFlush - done" );
  return hr;
}


//
// EndFlush
//
STDMETHODIMP CSubtitleInputPin::EndFlush( void )
{
  CAutoLock lock_it( m_Lock );
  LogDebug( "CSubtitleInputPin::BeginFlush" );
  m_pDVBSub->NotifySeeking();
  HRESULT hr = CBaseInputPin::EndFlush();
  LogDebug( "CSubtitleInputPin::BeginFlush - done" );
  return hr; 
}


#ifdef _DEBUG
// DEBUG ONLY
//
// AddRef
//
STDMETHODIMP_(ULONG) CSubtitleInputPin::AddRef()
{
  int tmp = m_cRef;
  HRESULT hr = CBaseInputPin::NonDelegatingAddRef();
  //LogDebug("CSubtitleInputPin::NonDelegatingAddRef - m_cRef %d", m_cRef );
  return hr;
}


// DEBUG ONLY
//
// Release
//
STDMETHODIMP_(ULONG) CSubtitleInputPin::Release()
{
  int tmp = m_cRef;
  HRESULT hr = CBaseInputPin::NonDelegatingRelease();
  //LogDebug("CSubtitleInputPin::NonDelegatingRelease - m_cRef %d", m_cRef );
  return hr;
}
#endif 
