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

#pragma warning( disable: 4995 4996 )

#include <streams.h>
#include <bdaiface.h>
#include "DVBSub.h"
#include "SubtitleInputPin.h"

extern void LogDebug( const char *fmt, ... );
extern void LogDebugPTS( const char *fmt, uint64_t pts );

// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypesSubtitle =
{
	&MEDIATYPE_MPEG2_SECTIONS, &MEDIASUBTYPE_DVB_SI
};

const AMOVIESETUP_MEDIATYPE sudPinTypesIn =
{
	&MEDIATYPE_NULL, &MEDIASUBTYPE_NULL
};

const AMOVIESETUP_PIN sudPins[4] =
{
	{
		L"In",				        // Pin string name
		FALSE,						    // Is it rendered
		FALSE,						    // Is it an output
		FALSE,						    // Allowed none
		FALSE,						    // Likewise many
		&CLSID_NULL,				  // Connects to filter
		L"In",				        // Connects to pin
		1,							      // Number of types
		&sudPinTypesSubtitle  // Pin information
	}
};


//
// Constructor
//
CDVBSub::CDVBSub( LPUNKNOWN pUnk, HRESULT *phr, CCritSec *pLock ) :
  CBaseFilter( NAME("MediaPortal DVBSub2"), pUnk, &m_Lock, CLSID_DVBSub2 ),
  m_pSubtitlePin( NULL ),
	m_pSubDecoder( NULL ),
  m_pSubtitleObserver( NULL ),
  m_pTimestampResetObserver( NULL ),
  m_pIMediaSeeking( NULL ),
  m_pMediaFilter( NULL ),
  m_bSeekingDone( true ),
  m_pReferenceClock( NULL ),
  m_startTimestamp( -1 ),
  m_CurrentSeekPosition( 0 )
{
	// Create subtitle decoder
	m_pSubDecoder = new CDVBSubDecoder();

	if( m_pSubDecoder == NULL )
	{
    if( phr )
	  {
      *phr = E_OUTOFMEMORY;
	  }
    return;
  }

  // Create subtitle input pin
	m_pSubtitlePin = new CSubtitleInputPin( this,
								GetOwner(),
								this,
								&m_Lock,
								&m_ReceiveLock,
								m_pSubDecoder,
								phr );

	if ( m_pSubtitlePin == NULL )
	{
    if( phr )
		{
      *phr = E_OUTOFMEMORY;
		}
    return;
  }

  if( m_pSubDecoder )
  {
    m_pSubDecoder->SetObserver( this );
  }
  else
  {
    LogDebug("No DVB subtitle decoder available!");
  }
}


//
// Destructor
//
CDVBSub::~CDVBSub()
{
	LogDebug( "CDVBSub::~CDVBSub() - start" );
  if( m_pSubDecoder )
  {
    m_pSubDecoder->SetObserver( NULL );
  }

	delete m_pSubDecoder;
	delete m_pSubtitlePin;
  
  LogDebug( "CDVBSub::~CDVBSub() - end" );
}


//
// GetPin
//
CBasePin * CDVBSub::GetPin( int n )
{
  if( n == 0 )
		return m_pSubtitlePin;

  return NULL;
}


//
// GetPinCount
//
int CDVBSub::GetPinCount()
{
	return 1; // subtitle in
}


//
// Run
//
STDMETHODIMP CDVBSub::Run( REFERENCE_TIME tStart )
{
  CAutoLock cObjectLock( m_pLock );
  LogDebug( "CDVBSub::Run" );
  m_startTimestamp = tStart;

  HRESULT hr = CBaseFilter::Run( tStart );

  if( hr != S_OK )
  {
    LogDebug( "CDVBSub::Run - BaseFilter returned %i", hr );
    return hr;
  }

  // Get the reference clock interface 
  if( !m_pReferenceClock )
  {
    IFilterGraph *pGraph = GetFilterGraph();
    if( pGraph )
    {
      pGraph->QueryInterface( IID_IMediaFilter, (void**)&m_pMediaFilter );
      m_pMediaFilter->GetSyncSource( &m_pReferenceClock );
      pGraph->Release();
    }
  }

  // Get media seeking interface if missing
  if( !m_pIMediaSeeking )
  {
    IFilterGraph *pGraph = GetFilterGraph();    
    if( pGraph )
    {
	    pGraph->QueryInterface( &m_pIMediaSeeking );
      pGraph->Release();
    }
  }
  LONGLONG pos( 0 );
  m_pIMediaSeeking->GetCurrentPosition( &pos );
  pos = ( ( pos / 1000 ) * 9 ); // PTS = 90Khz, REFERENCE_TIME one tick 100ns
  m_CurrentSeekPosition = pos;

  LogDebugMediaPosition( "Run - media seeking position" );        

  LogDebug( "CDVBSub::Run - done" );
	return hr; 
}


//
// Pause
//
STDMETHODIMP CDVBSub::Pause()
{
  CAutoLock cObjectLock( m_pLock );
  LogDebug( "CDVBSub::Pause" );
  HRESULT hr = CBaseFilter::Pause();
  LogDebug( "CDVBSub::Pause - done" );
  return hr;
}


//
// Stop
//
STDMETHODIMP CDVBSub::Stop()
{
  CAutoLock cObjectLock( m_pLock );
  HRESULT hr = CBaseFilter::Stop();

  LogDebug("CDVBSub::Stop - beginning" );

  // Make sure no further processing is done
  if( m_pSubDecoder ) m_pSubDecoder->Reset();
	if( m_pSubtitlePin ) m_pSubtitlePin->Reset();
  
  LogDebug( "Release m_pReferenceClock" );
  if( m_pReferenceClock )
  {
    //m_pReferenceClock->Release();
    //m_pReferenceClock = NULL;
  }
  LogDebug( "Release m_pReferenceClock - done" );

  LogDebug( "Release m_pMediaFilter" );
  if( m_pMediaFilter )
  {
    m_pMediaFilter->Release();
    //m_pMediaFilter = NULL;
  }
  LogDebug( "Release m_pMediaFilter - done" );

  LogDebug( "Release m_pIMediaSeeking" );
  if( m_pIMediaSeeking )
  {
    m_pIMediaSeeking->Release();
    //m_pIMediaSeeking = NULL;
  }
  LogDebug( "Release m_pIMediaSeeking - done" );
  LogDebug("CDVBSub::Stop - done" );

  return hr;
}


//
// Reset
//
void CDVBSub::Reset()
{
  CAutoLock cObjectLock( m_pLock );
  LogDebug( "CDVBSub::Reset()" );

	if( m_pSubDecoder ) m_pSubDecoder->Reset();
  if( m_pSubtitlePin ) m_pSubtitlePin->Reset();

  // Notify reset observer
  if( m_pTimestampResetObserver )
  {
    (*m_pTimestampResetObserver)();
  }

  LogDebugMediaPosition( "CDVBSub::Reset - media seeking position" );  
}


//
// Test
//
STDMETHODIMP CDVBSub::Test(int status)
{
	LogDebug("TEST : %i", status);
	return S_OK;
}

//
// SetSubtitlePid
//
STDMETHODIMP CDVBSub::SetSubtitlePid( LONG pPid )
{
  if( m_pSubtitlePin )
  {
    m_pSubtitlePin->SetSubtitlePid( pPid );
    return S_OK;
  }
  else
  {
    return S_FALSE;
  }
}

//
// SetFirstPcr
//
STDMETHODIMP CDVBSub::SetFirstPcr( LONGLONG pPcr )
{
  m_basePCR = pPcr;
  LogDebugMediaPosition( "SetFirstPcr - media position" );  
  LogDebugPTS( "SetFirstPcr", pPcr ); 
  return S_OK;
}

//
// NotifySubtitle
//
void CDVBSub::NotifySubtitle()
{
  LogDebugMediaPosition( "Subtitle arrived - media position" );  

  // calculate the time stamp
  CSubtitle* pSubtitle( NULL );
  pSubtitle = m_pSubDecoder->GetLatestSubtitle();
  if( pSubtitle )
  {
    // PTS to milliseconds ( 90khz )
    LONGLONG pts( 0 ); 
    LONGLONG subtitlePTS( pSubtitle->PTS() );
   
    pts = ( subtitlePTS - m_basePCR - m_CurrentSeekPosition ) / 90;

    LogDebugPTS( "subtitlePTS           ", subtitlePTS ); 
    LogDebugPTS( "m_basePCR             ", m_basePCR ); 
    LogDebugPTS( "m_CurrentSeekPosition ", m_CurrentSeekPosition ); 
    LogDebugPTS( "timestamp ms          ", pts * 90 ); 

    pSubtitle->SetTimestamp( pts );  

    if( pts <= 0 )
    {
      LogDebug( "Discarding subtitle, too old timestamp!" );
      this->DiscardOldestSubtitle();
      return;
    }
  }
  if( m_pSubtitleObserver )
  {
    // Notify the callback function
	  SUBTITLE sub;
	  this->GetSubtitle( 0, &sub );
	  LogDebug( "Calling subtitle callback" );
    int retval = (*m_pSubtitleObserver)( &sub );
	  LogDebug( "subtitle Callback returned" );
	  this->DiscardOldestSubtitle();
  }
  else
  {
	  LogDebug( "No callback set" );
  }
}

//
// NotifySeeking
//
void CDVBSub::NotifySeeking()
{
  m_bSeekingDone = true;
}


//
// Interface methods
//
STDMETHODIMP CDVBSub::GetSubtitle( int place, SUBTITLE* subtitle )
{
  CSubtitle* pCSubtitle = NULL;

  if( m_pSubDecoder )
  {
    pCSubtitle = m_pSubDecoder->GetSubtitle( place );
  }

  if( pCSubtitle )
  {
	  BITMAP* bitmap = pCSubtitle->GetBitmap();
	  LogDebug("Bitmap: bpp=%i, planes=%i, dim=%i x %i",bitmap->bmBitsPixel,bitmap->bmPlanes, bitmap->bmWidth, bitmap->bmHeight);
	  subtitle->bmBits = bitmap->bmBits;
	  subtitle->bmBitsPixel = bitmap->bmBitsPixel;
    subtitle->bmHeight = bitmap->bmHeight;
	  subtitle->bmPlanes = bitmap->bmPlanes;
	  subtitle->bmType = bitmap->bmType;
	  subtitle->bmWidth = bitmap->bmWidth;
	  //LogDebug("Stride: %i" , bitmap->bmWidthBytes);
	  subtitle->bmWidthBytes = bitmap->bmWidthBytes;
    subtitle->timestamp = pCSubtitle->Timestamp();
    subtitle->firstScanLine = pCSubtitle->FirstScanline();
    subtitle->timeOut = pCSubtitle->Timeout();
	  LogDebug("TIMEOUT  : %i", subtitle->timeOut);
	  LogDebug("TIMESTAMP: %i", subtitle->timestamp);
    return S_OK;
  }
  else
  {
    return S_FALSE;
  }
}


//
// SetCallback
//
STDMETHODIMP CDVBSub::SetCallback( int (CALLBACK *pSubtitleObserver)(SUBTITLE* sub) )
{
	LogDebug( "SetCallback called" );
  m_pSubtitleObserver = pSubtitleObserver;
  return S_OK;
}


//
// SetTimestampResetCallback
//
STDMETHODIMP CDVBSub::SetTimestampResetCallback( int (CALLBACK *pTimestampResetObserver)() )
{
	LogDebug( "SetTimestampResetedCallback called" );
  m_pTimestampResetObserver = pTimestampResetObserver;
  return S_OK;
}


//
// GetSubtitleCount
//
STDMETHODIMP CDVBSub::GetSubtitleCount( int* pcount )
{
	LogDebug( "GetSubtitleCount" );
  if( m_pSubDecoder )
  {
    *pcount = m_pSubDecoder->GetSubtitleCount();
	  return S_OK;
  }
  return S_FALSE;
}


//
// DiscardOldestSubtitle
//
STDMETHODIMP CDVBSub::DiscardOldestSubtitle()
{
	LogDebug( "DiscardOldestSubtitle" );
  if( m_pSubDecoder )
  {
    m_pSubDecoder->ReleaseOldestSubtitle();
	  return S_OK;
  }
  return S_FALSE;
}

//
// LogDebugMediaPosition
//
void CDVBSub::LogDebugMediaPosition( const char *text )
{
  if( m_State == State_Stopped || m_State == State_Paused )
  {
    LogDebug( text );
    LogDebug( "LogDebugMediaPosition - paused || stopped" );
    return;
  }
  
  LONGLONG pos( 0 );
  if( m_pIMediaSeeking )
  {
    m_pIMediaSeeking->GetCurrentPosition( &pos );
	  if( pos > 0 )
	  {
		  pos = ( ( pos / 1000 ) * 9 ); // PTS = 90Khz, REFERENCE_TIME one tick 100ns
	    LogDebugPTS( text, pos ); 
    }
  } 
}

//
// CreateInstance
//
CUnknown * WINAPI CDVBSub::CreateInstance( LPUNKNOWN punk, HRESULT *phr )
{
  ASSERT( phr );

  LogDebug( "CreateInstance" );
  CDVBSub *pFilter = new CDVBSub( punk, phr, NULL );
  if( pFilter == NULL )
	{
    if ( phr )
		{
      *phr = E_OUTOFMEMORY;
		}
  }
  return pFilter;
}


//
// NonDelegatingQueryInterface
//
STDMETHODIMP CDVBSub::NonDelegatingQueryInterface( REFIID riid, void** ppv )
{
	if ( riid == IID_IDVBSubtitle2 )
  {
		//LogDebug( "QueryInterface in DVBSub.CPP accepting" );
		return GetInterface( (IDVBSubtitle *) this, ppv );
	}
	else
	{
		//LogDebug( "Forwarding query interface call ... " );
		HRESULT hr = CBaseFilter::NonDelegatingQueryInterface( riid, ppv );

		if( SUCCEEDED(hr) )
    {
			//LogDebug("QI succeeded");
		}
		else if( hr == E_NOINTERFACE )
    {
			//LogDebug( "QI -> E_NOINTERFACE" );
		}
		else
    {
			//LogDebug( "QI failed" );
		}
		return hr;
	}
}