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

#include "PMTInputPin.h"
#include "PidObserver.h"
#include "PatParser\PatParser.h"
#include "PatParser\PmtParser.h"
#include "PatParser\PidTable.h"

extern void Log( const char *fmt, ... );

const int   TSPacketSize = 188;
ULONG       PMT_PID = 0x0;

CPMTInputPin::CPMTInputPin( CSubTransform *m_pTransform,
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
  Log( "PMT: Pin created" );
}


CPMTInputPin::~CPMTInputPin()
{
  delete m_pPatParser;
}
//
// CheckMediaType
//
HRESULT CPMTInputPin::CheckMediaType( const CMediaType *pmt )
{
	Log("PMT pin: CheckMediaType()");
	if( pmt->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT )
	{
		return S_OK;
	}
	return S_FALSE;
}

HRESULT CPMTInputPin::CompleteConnect( IPin *pPin )
{
  m_pDemuxerPin = pPin;
	HRESULT hr = CBasePin::CompleteConnect( pPin );
  if( hr == S_OK )
  {
    return MapPidToDemuxer( PMT_PID, m_pDemuxerPin, MEDIA_TRANSPORT_PACKET );
  }
  else
  {
    return hr;
  }
}


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

  //CAutoLock lock(m_pReceiveLock);
  PBYTE pbData=NULL;
	long lDataLen=0;

  HRESULT hr = pSample->GetPointer( &pbData );
  if (FAILED(hr)) {
      return hr;
  }
	
	lDataLen = pSample->GetActualDataLength();
	// decode
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


HRESULT CPMTInputPin::Process( BYTE *pbData, long len )
{
  OnRawData( pbData, len );
  return S_OK;
}


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

void CPMTInputPin::Reset()
{
  m_subtitlePid = -1;
  m_pcrPid = -1;
  m_pPatParser->Reset();
  mappedPids.clear();
}


STDMETHODIMP CPMTInputPin::BeginFlush(void)
{
	Reset();
	return CBaseInputPin::BeginFlush();
}


STDMETHODIMP CPMTInputPin::EndFlush(void)
{
	Reset();
	return CBaseInputPin::EndFlush();
}
