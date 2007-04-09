/*
 *	Copyright (C) 2006-2007 Team MediaPortal
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
CPMTInputPin::CPMTInputPin( CDVBSub *m_pFilter,
								LPUNKNOWN pUnk,
								CBaseFilter *pFilter,
								CCritSec *pLock,
								CCritSec *pReceiveLock,
								HRESULT *phr,
                MPidObserver *pPidObserver ) :

    CBaseInputPin( NAME( "CPMTInputPin" ),
					      pFilter,						// Filter
					      pLock,							// Locking
					      phr,							  // Return code
					      L"PMT" ),						// Pin name
					      m_pReceiveLock( pReceiveLock ),
					      m_pTransform( m_pTransform ),
  m_pDemuxerPin( NULL ),
  m_pPidObserver( pPidObserver ),
  m_subtitlePid( -1 ),
  m_streamVideoPid( -1 ),
  m_pcrPid( -1 )
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
  m_pDemuxerPin = pPin;
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  if( hr == S_OK )
  {
    hr = MapPidToDemuxer( PMT_PID, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );

    if( hr == S_OK )
      hr = FindVideoPID();
  }
  return hr;
}


//
// SetVideoPid
//
void CPMTInputPin::SetVideoPid( int videoPid )
{
  m_streamVideoPid = videoPid;
}


//
// Receive
//
STDMETHODIMP CPMTInputPin::Receive( IMediaSample *pSample )
{
  CheckPointer( pSample, E_POINTER );

  if( m_bReset )
  {
    FindVideoPID();
    m_bReset = false;
  }

  CAutoLock lock( m_pReceiveLock );
  PBYTE pbData=NULL;
	long lDataLen=0;

  HRESULT hr = pSample->GetPointer( &pbData );
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

        if( m_streamVideoPid == videoPid && m_pPidObserver != NULL )
        {
          if( m_pcrPid == -1 && pidTable.PcrPid > 0)
          {
            m_pcrPid = pidTable.PcrPid;
            m_pPidObserver->SetPcrPid( m_pcrPid  );
          }
          if( m_subtitlePid == -1 && pidTable.SubtitlePid > 0)
          {
            m_subtitlePid = pidTable.SubtitlePid;
            m_pPidObserver->SetSubtitlePid( m_subtitlePid );
          }
          break; // correct PMT is found
        }
      }
    }
  }
  return hr;
}


//
// FindVideoPID
//
HRESULT CPMTInputPin::FindVideoPID()
{
  HRESULT hr;
  IFilterGraph *pGraph = m_pFilter->GetFilterGraph();
  IBaseFilter *pDemuxer = NULL;;

  hr = pGraph->FindFilterByName( L"MPEG-2 Demultiplexer", &pDemuxer );

  if( hr != S_OK )
  {
    LogDebug( "Unable to find demuxer!" );
    return hr;
  }
  IPin* pPin;
  PIN_DIRECTION  direction;
  IEnumPins *pIEnumPins = NULL;
  AM_MEDIA_TYPE *mediatype;
  if( SUCCEEDED( pDemuxer->EnumPins( &pIEnumPins ) ) )
  {
    ULONG count(0);
    while( pIEnumPins->Next( 1, &pPin, &count ) == S_OK )
    {
	    hr = pPin->QueryDirection( &direction );
	    if( direction == PINDIR_OUTPUT )
      {
		    IEnumMediaTypes* ppEnum;
		    if( SUCCEEDED( pPin->EnumMediaTypes( &ppEnum ) ) )
        {
			    ULONG fetched( 0 );
			    while( ppEnum->Next( 1, &mediatype, &fetched ) == S_OK )
			    {
				    if( mediatype->majortype == MEDIATYPE_Video )
            {
              IMPEG2PIDMap* pMuxMapPid;
              if( SUCCEEDED( pPin->QueryInterface( &pMuxMapPid ) ) )
              {
                IEnumPIDMap *pIEnumPIDMap;
                if( SUCCEEDED( pMuxMapPid->EnumPIDMap( &pIEnumPIDMap ) ) )
                {
	                ULONG count = 0;
	                PID_MAP pidMap;
	                while( pIEnumPIDMap->Next( 1, &pidMap, &count ) == S_OK )
                  {
                    SetVideoPid( pidMap.ulPID );
                    LogDebug( "  found video PID %d",  m_streamVideoPid );
	                }
                }
              pMuxMapPid->Release();
					    ppEnum->Release();
					    return S_OK;
              }
				    }
				    mediatype = NULL;
			    }
			    ppEnum->Release();
			    ppEnum = NULL;
		    }
	    }
	    pPin->Release();
	    pPin = NULL;
    }
    pIEnumPins->Release();
  }
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
          MapPidToDemuxer( pid, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );
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
	Reset();
	return CBaseInputPin::BeginFlush();
}


//
// EndFlush
//
STDMETHODIMP CPMTInputPin::EndFlush( void )
{
	Reset();
	return CBaseInputPin::EndFlush();
}
