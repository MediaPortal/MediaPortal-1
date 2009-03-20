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

#pragma warning( disable: 4995 4996 )

#include <bdaiface.h>
#include <shlobj.h>
#include "DVBSub.h"
#include "SubtitleInputPin.h"
extern void LogDebug( const char *fmt, ... );
extern void LogDebugPTS( const char *fmt, uint64_t pts );
extern void GetLogFile(char *pLog);

//
// Constructor
//
CDVBSub::CDVBSub( LPUNKNOWN pUnk, HRESULT *phr, CCritSec *pLock ) :
  CBaseFilter( NAME("MediaPortal DVBSub2"), pUnk, &m_Lock, CLSID_DVBSub2 ),
  m_pSubtitlePin( NULL ),
  m_pSubDecoder( NULL ),
  m_pSubtitleObserver( NULL ),
  m_pUpdateTimeoutObserver( NULL ),
  m_pResetObserver( NULL ),
  m_pIMediaSeeking( NULL ),
  m_bSeekingDone( true ),
  m_startTimestamp( -1 ),
  m_CurrentSeekPosition( 0 ),
  m_prevSubtitleTimestamp( 0 ),
  m_bBasePcrSet( false )
{
  TCHAR filename[1024];
  GetLogFile(filename);
  ::DeleteFile(filename);

  LogDebug("-------------- MediaPortal DVBSub2.ax version 1.3.2 ----------------");
  
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
  {
    return m_pSubtitlePin;
  }
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

  LogDebug( "CDVBSub::Stop - beginning" );

  // Make sure no further processing is done
  if( m_pSubDecoder ) m_pSubDecoder->Reset();
  if( m_pSubtitlePin ) m_pSubtitlePin->Reset();
  
  //LogDebug( "Release m_pIMediaSeeking" );
  //if( m_pIMediaSeeking )
  {
    //m_pIMediaSeeking->Release();
    //m_pIMediaSeeking = NULL;
  }
  //LogDebug( "Release m_pIMediaSeeking - done" );

  LogDebug( "CDVBSub::Stop - done" );

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
  if( m_pResetObserver )
  {
    (*m_pResetObserver)();
  }

  //LogDebugMediaPosition( "CDVBSub::Reset - media seeking position" );  
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
// Test
//
STDMETHODIMP CDVBSub::StatusTest(int status)
{
  LogDebug("STATUSTEST : %i", status);
  return S_OK;
}


//
// SetSubtitlePid
//
STDMETHODIMP CDVBSub::SetSubtitlePid( LONG pPid )
{
  LogDebug( "CDVBSub::SetSubtitlePid() %d", pPid );
  
  if( m_subtitlePid != pPid )
  {
    LogDebug( "Subtitle PID has changed!" );
    Reset();
  }
  
  m_subtitlePid = pPid;

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
  if( !m_bBasePcrSet )
  {
    m_basePCR = pPcr;
    m_bBasePcrSet = true;
    LogDebugPTS( "m_basePCR set", pPcr ); 
  }
  else
  {
    LogDebugPTS( " m_basePCR already set", pPcr );
    LogDebugPTS( "   new would have been", pPcr );
  }
    
  return S_OK;
}

//
// SeekDone
//
STDMETHODIMP CDVBSub::SeekDone( CRefTime& rtSeek )
{
  // Notify reset observer (clears all cached subtitles on client side)
  if( m_pResetObserver )
  {
    (*m_pResetObserver)();
  }

  // milliseconds to PCR (90Khz)
  m_CurrentSeekPosition = rtSeek.Millisecs() * 90;
  LogDebugPTS( "SeekDone", m_CurrentSeekPosition );
  return S_OK;
}


//
// SetTimeCompensation
//
STDMETHODIMP CDVBSub::SetTimeCompensation( CRefTime& rtCompensation )
{
  m_currentTimeCompensation = rtCompensation;
  float fTime = (float)m_currentTimeCompensation.Millisecs();
  fTime /= 1000.0f;
  LogDebug( "SetTimeCompensation: %03.3f", fTime );
  return S_OK;
}


//
// NotifyChannelChange
//
STDMETHODIMP CDVBSub::NotifyChannelChange()
{
  Reset();
  return S_OK;
}


//
// NotifySubtitle
//
void CDVBSub::NotifySubtitle()
{
  LogDebugMediaPosition( "Subtitle arrived - media position" );  

  // Calculate the time stamp
  CSubtitle* pSubtitle( NULL );
  pSubtitle = m_pSubDecoder->GetLatestSubtitle();
  if( pSubtitle )
  {
    // PTS to milliseconds ( 90khz )
    LONGLONG pts( 0 ); 
   
    pts = ( pSubtitle->PTS() - m_basePCR ) / 90 - m_currentTimeCompensation.Millisecs();

    float fTime = (float)m_currentTimeCompensation.Millisecs();
    fTime /= 1000.0f;
    LogDebugPTS( "subtitlePTS               ", pSubtitle->PTS() ); 
    LogDebugPTS( "m_basePCR                 ", m_basePCR ); 
    LogDebugPTS( "timestamp                 ", pts * 90 ); 
    LogDebugPTS( "m_CurrentSeekPosition     ", m_CurrentSeekPosition ); 
    LogDebug( "m_currentTimeCompensation  %03.3f", fTime ); 

    pSubtitle->SetTimestamp( pts );
    m_prevSubtitleTimestamp = pSubtitle->PTS();

    if( pts <= 0 )
    {
      LogDebug( "Discarding subtitle, invalid timestamp!" );
      DiscardOldestSubtitle();
      return;
    }
  }
  if( m_pSubtitleObserver )
  {
    // Notify the MediaPortal side
    SUBTITLE sub;
    GetSubtitle( 0, &sub );
    LogDebug( "Calling subtitle callback" );
    int retval = (*m_pSubtitleObserver)( &sub );
    LogDebug( "Subtitle Callback returned" );
    DiscardOldestSubtitle();
  }
  else
  {
    LogDebug( "No callback set" );
  }
}


//
// UpdateSubtitleTimeout
//
void CDVBSub::UpdateSubtitleTimeout( uint64_t pTimeout )
{
  if( m_pUpdateTimeoutObserver )
  {
    // Calculate the timeout
    __int64 timeOut( 0 ); 
    timeOut = pTimeout + m_currentTimeCompensation.Millisecs() - m_prevSubtitleTimestamp;
    timeOut = timeOut/90;

    LogDebug("Calling update timeout observer - timeout = %lld ms", timeOut );

    (*m_pUpdateTimeoutObserver)( &timeOut );
  }
  else
  {
    LogDebug( "No m_pUpdateTimeoutObserver set" );
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
    subtitle->bmWidthBytes = bitmap->bmWidthBytes;
    subtitle->screenHeight = pCSubtitle->ScreenHeight();
    subtitle->screenWidth = pCSubtitle->ScreenWidth();
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
// SetBitmapCallback
//
STDMETHODIMP CDVBSub::SetBitmapCallback( int (CALLBACK *pSubtitleObserver)(SUBTITLE* sub))
{
  LogDebug( "SetBitmapCallback called" );
  m_pSubtitleObserver = pSubtitleObserver;
  return S_OK;
}


//
// SetResetCallback
//
STDMETHODIMP CDVBSub::SetResetCallback( int (CALLBACK *pResetObserver)() )
{
  LogDebug( "SetTimestampResetedCallback called" );
  m_pResetObserver = pResetObserver;
  return S_OK;
}


//
// SetUpdateTimeoutCallback
//
STDMETHODIMP CDVBSub::SetUpdateTimeoutCallback( int (CALLBACK *pUpdateTimeoutObserver)(__int64* pTimeout) )
{
  LogDebug( "SetUpdateTimeoutCallback called" );
  m_pUpdateTimeoutObserver = pUpdateTimeoutObserver;
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
  if( m_State != State_Running )
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
  else if ( riid == IID_IDVBSubtitleSource )
  {
    //LogDebug( "QueryInterface in DVBSub.CPP accepting" );
    return GetInterface( (IDVBSubtitleSource *) this, ppv );
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

#ifdef _DEBUG
// DEBUG ONLY
//
// NonDelegatingAddRef
//
STDMETHODIMP_(ULONG) CDVBSub::NonDelegatingAddRef()
{
  int tmp = m_cRef;
  HRESULT hr = CBaseFilter::NonDelegatingAddRef();
  //LogDebug("CDVBSub::NonDelegatingAddRef - m_cRef %d", m_cRef );
  return hr;
}

// DEBUG ONLY
//
// NonDelegatingRelease
//
STDMETHODIMP_(ULONG) CDVBSub::NonDelegatingRelease()
{
  int tmp = m_cRef;
  HRESULT hr = CBaseFilter::NonDelegatingRelease();
  //LogDebug("CDVBSub::NonDelegatingRelease - m_cRef %d", m_cRef );
  return hr;
}
#endif
