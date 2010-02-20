/*
 *	Copyright (C) 2006-2008 Team MediaPortal
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

#include "SubtitleInputPin.h"
#include "PCRInputPin.h"
#include "PMTInputPin.h"
#include "PidObserver.h"
#include "PatParser\PatParser.h"
#include "PatParser\PmtParser.h"
#include "PatParser\PidTable.h"

extern void LogDebug( const char *fmt, ... );

const int   TSPacketSize = 188;
const ULONG PMT_PID = 0x0;

//
// Constructor
//
CPMTInputPin::CPMTInputPin( CDVBSub *pSubFilter,
								LPUNKNOWN pUnk,
								CBaseFilter *pFilter,
								CCritSec *pLock,
								CCritSec *pReceiveLock,
                HRESULT *phr,
                CSubtitleInputPin *pSubtitlePin,
                CPcrInputPin *pPCRPin ) :

    CBaseInputPin( NAME( "CPMTInputPin" ),
					      pFilter,						// Filter
					      pLock,							// Locking
					      phr,							  // Return code
					      L"PMT" ),						// Pin name
					      m_pReceiveLock( pReceiveLock ),
					      m_pFilter( pSubFilter ),
  m_pDemuxerPin( NULL ),
  m_pSubtitlePin( pSubtitlePin ),
  m_pPCRPin( pPCRPin ),
  m_subtitlePid( -1 ),
  m_streamVideoPid( -1 ),
  m_pcrPid( -1 ),
  m_sampleCount( 0 )
{
	m_pPatParser = new CPatParser();
  m_pPatParser->Reset();
  LogDebug( "PMT: Pin created" );
}


//
// Destructor
//
CPMTInputPin::~CPMTInputPin()
{
  delete m_pPatParser;
}


//
// CheckMediaType
//
HRESULT CPMTInputPin::CheckMediaType( const CMediaType *pmt )
{
	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}


//
// CompleteConnect
//
HRESULT CPMTInputPin::CompleteConnect( IPin *pPin )
{
  LogDebug( "PMT input pin - CompleteConnect ");
  m_pDemuxerPin = pPin;
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  
  if( hr == S_OK )
  {
    hr = m_pFilter->SetPid( this, PMT_PID, MEDIA_TRANSPORT_PACKET );
  }
  LogDebug( "PMT input pin - CompleteConnect - done - hr = %i", hr);
  return hr;
}


//
// GetDemuxerPin
//
IPin* CPMTInputPin::GetDemuxerPin()
{
  return m_pDemuxerPin;
}


//
// SetVideoPid
//
void CPMTInputPin::SetVideoPid( int videoPid )
{
  m_streamVideoPid = videoPid;
}

//
// ReceiveCanBlock
//
STDMETHODIMP CPMTInputPin::ReceiveCanBlock()
{
  return S_OK;
}

//
// Receive
//
STDMETHODIMP CPMTInputPin::Receive( IMediaSample *pSample )
{
  CAutoLock lock( m_pReceiveLock );
//  LogDebug( "CPMTInputPin::Receive" );

  HRESULT hr = CBaseInputPin::Receive( pSample );
  if( hr != S_OK ) 
  {
    LogDebug( "CPMTInputPin::Receive - BaseInputPin ignored the sample!" ); 
    return hr;
  }

  CheckPointer( pSample, E_POINTER );

  PBYTE pbData = NULL;
	long lDataLen = 0;

  hr = pSample->GetPointer( &pbData );
  if( FAILED(hr) )
    return hr;

	lDataLen = pSample->GetActualDataLength();

	if( lDataLen > 5 )
		hr = Process( pbData, lDataLen );

  if( hr == S_OK && ( m_subtitlePid == -1 || m_pcrPid == -1 ) )
  {
    // Try to find a correct PMT
    int count = m_pPatParser->m_pmtParsers.size();
    if( count > 0 )
    {
      CPidTable pidTable;
      ULONG videoPid = 0;
      for( int i = 0; i < count ; i++ )
      {
        pidTable = m_pPatParser->m_pmtParsers[i]->GetPidInfo();
        videoPid = pidTable.VideoPid;

        if( m_streamVideoPid == videoPid )
        {
          if( m_pcrPid == -1 && pidTable.PcrPid > 0 && m_pPCRPin != NULL )
          {
            m_pcrPid = pidTable.PcrPid;
            m_pPCRPin->SetPcrPid( m_pcrPid  );
          }
          if( m_subtitlePid == -1 && pidTable.SubtitlePid > 0 && m_pSubtitlePin != NULL )
          {
            m_subtitlePid = pidTable.SubtitlePid;
            m_pSubtitlePin->SetSubtitlePid( m_subtitlePid );
          }
          break; // correct PMT is found
        }
      }
    }
  }
  //LogDebug( "CPMTInputPin::Receive - done" );
  return hr;
}


//
// Process
//
HRESULT CPMTInputPin::Process( BYTE *pbData, long len )
{
  OnRawData( pbData, len );
  return S_OK;
}


//
// OnTsPacket
//
void CPMTInputPin::OnTsPacket( byte* tsPacket )
{
  m_pPatParser->OnTsPacket( tsPacket );

  unsigned int count = m_pPatParser->m_pmtParsers.size();
  if( count > 0 )
  {
    // Map all PMT pids
    if( count > mappedPids.size() )
    {
      for( unsigned int i = 0 ; i < count ; i++ )
      {
        bool alreadyMapped = false;
        int pid = m_pPatParser->m_pmtParsers[i]->GetPid();

        for( unsigned int j = 0 ; j < mappedPids.size() ; j++ )
        {
          if( mappedPids[j] == pid )
          {
            alreadyMapped = true;
            break;
          }
        }

        if( !alreadyMapped )
        {
          m_pFilter->SetPid( this, pid, MEDIA_TRANSPORT_PACKET );
          mappedPids.resize( mappedPids.size() + 1 );
          mappedPids[mappedPids.size() - 1] = pid;
        }
      }
    }
  }
}


//
// Reset
//
void CPMTInputPin::Reset()
{
  LogDebug( "CPMTInputPin - reset" );
  m_sampleCount = 0;
  m_bReset = true;
  m_subtitlePid = -1;
  m_pcrPid = -1;
  m_pPatParser->Reset();
  LogDebug( "CPMTInputPin - PatParser reset done" );
  mappedPids.clear();
  LogDebug( "CPMTInputPin - reset done" );
}


//
// BeginFlush
//
STDMETHODIMP CPMTInputPin::BeginFlush( void )
{
  CAutoLock lock_it( m_pReceiveLock );
  LogDebug( "CPMTInputPin::BeginFlush" );
  HRESULT hr = CBaseInputPin::BeginFlush();
  LogDebug( "CPMTInputPin::BeginFlush - done" );
	return hr;
}


//
// EndFlush
//
STDMETHODIMP CPMTInputPin::EndFlush( void )
{
  CAutoLock lock_it( m_pReceiveLock );  
  LogDebug( "CPMTInputPin::EndFlush" );
  Reset();
  HRESULT hr = CBaseInputPin::EndFlush();
  LogDebug( "CPMTInputPin::EndFlush - done" );
	return hr; 
}
