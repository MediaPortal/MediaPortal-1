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

#include "DVBSub.h"
#include "SubtitleInputPin.h"
#include "PcrInputPin.h"
#include "PMTInputPin.h"

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
	},
  {
		L"PCR",					    // Pin string name
		FALSE,						  // Is it rendered
		FALSE,						  // Is it an output
		FALSE,						  // Allowed none
		FALSE,						  // Likewise many
		&CLSID_NULL,			  // Connects to filter
		L"PCR",					    // Connects to pin
		1,							    // Number of types
		&sudPinTypesIn	    // Pin information
	},
	{
		L"PMT",					    // Pin string name
		FALSE,						  // Is it rendered
		FALSE,						  // Is it an output
		FALSE,						  // Allowed none
		FALSE,						  // Likewise many
		&CLSID_NULL,			  // Connects to filter
		L"PMT",					    // Connects to pin
		1,							    // Number of types
		&sudPinTypesIn	    // Pin information
	}
};


//
// Constructor
//
CDVBSub::CDVBSub( LPUNKNOWN pUnk, HRESULT *phr, CCritSec *pLock ) :
  CBaseFilter( NAME("MediaPortal DVBSub"), pUnk, &m_Lock, CLSID_DVBSub ),
  m_pSubtitleInputPin( NULL ),
	m_pSubDecoder( NULL ),
  m_pSubtitleObserver( NULL ),
  m_pTimestampResetObserver( NULL ),
  m_pIMediaSeeking( NULL ),
  m_basePCR( -1 ),
  m_firstPCR( -1 ),
  m_startTimestamp( -1 ),
  m_seekDifPCR( -1 )
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
	m_pSubtitleInputPin = new CSubtitleInputPin( this,
								GetOwner(),
								this,
								&m_Lock,
								&m_ReceiveLock,
								m_pSubDecoder,
								phr );

	if ( m_pSubtitleInputPin == NULL )
	{
    if( phr )
		{
      *phr = E_OUTOFMEMORY;
		}
    return;
  }

	// Create pcr input pin
	m_pPcrPin = new CPcrInputPin( this,
								GetOwner(),
								this,
								&m_Lock,
								&m_ReceiveLock,
								phr );

	if ( m_pPcrPin == NULL )
	{
    if( phr )
		{
      *phr = E_OUTOFMEMORY;
		}
    return;
  }

	// Create PMT input pin
	m_pPMTPin = new CPMTInputPin( this,
								GetOwner(),
								this,
								&m_Lock,
								&m_ReceiveLock,
								phr,
                this ); // MPidObserver

	if ( m_pPMTPin == NULL )
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
	m_pSubDecoder->SetObserver( NULL );
	delete m_pSubDecoder;
	delete m_pSubtitleInputPin;
	delete m_pPcrPin;
  delete m_pPMTPin;
}


//
// GetPin
//
CBasePin * CDVBSub::GetPin( int n )
{
	if( n == 0 )
		return m_pSubtitleInputPin;

  if( n == 1 )
		return m_pPcrPin;

  if( n == 2 )
		return m_pPMTPin;

  return NULL;
}


//
// GetPinCount
//
int CDVBSub::GetPinCount()
{
	return 3; // subtitle in, pmt, pcr
}


//
// CheckConnect
//
HRESULT CDVBSub::CheckConnect( PIN_DIRECTION dir, IPin *pPin )
{
  AM_MEDIA_TYPE mediaType;
  int videoPid = 0;

  pPin->ConnectionMediaType( &mediaType );

  // Search for demuxer's video pin
  if(  mediaType.majortype == MEDIATYPE_Video && dir == PINDIR_INPUT )
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
          m_VideoPid = pidMap.ulPID;
          m_pPMTPin->SetVideoPid( m_VideoPid );
          LogDebug( "  found video PID %d",  m_VideoPid );
			  }
		  }
		  pMuxMapPid->Release();
    }
  }
  return S_OK;
}


//
// Run
//
STDMETHODIMP CDVBSub::Run( REFERENCE_TIME tStart )
{
  // Get media seeking interface if missing
  if( !m_pIMediaSeeking )
  {
    IFilterGraph *pGraph = GetFilterGraph();
	  pGraph->QueryInterface( &m_pIMediaSeeking );
  }

  Reset();
  m_startTimestamp = tStart;
  CAutoLock cObjectLock( m_pLock );
	return CBaseFilter::Run( tStart );
}


//
// Pause
//
STDMETHODIMP CDVBSub::Pause()
{
  CAutoLock cObjectLock( m_pLock );
	return CBaseFilter::Pause();
}


//
// Stop
//
STDMETHODIMP CDVBSub::Stop()
{
  CAutoLock cObjectLock( m_pLock );
	Reset();
	return CBaseFilter::Stop();
}


//
// Reset
//
void CDVBSub::Reset()
{
	LogDebug( "Reset()" );
  CAutoLock cObjectLock( m_pLock );

	m_pSubDecoder->Reset();
	m_pSubtitleInputPin->Reset();
  
  m_seekDifPCR = -1;
  //m_fixPCR = -1; // update this only in the beginning
  //m_firstPCR = -1;

  if( m_pTSFileSource )
  {
    REFERENCE_TIME posBase( 0 );
    m_pTSFileSource->GetBasePCRPosition( &posBase );

    m_basePCR = ( posBase / 1000 ) * 9;
    LogDebugPTS( "TSFileSource base PCR:", m_basePCR );
  }

  // Notify reset observer
  if( m_pTimestampResetObserver )
    (*m_pTimestampResetObserver)();

  LONGLONG pos( 0 );
  IFilterGraph *pGraph = GetFilterGraph();
  IMediaSeeking* pIMediaSeeking;
  pGraph->QueryInterface( &pIMediaSeeking );
  if( pIMediaSeeking )
  {
    pIMediaSeeking->GetCurrentPosition( &pos );
	  if( pos > 0 )
	  {
		  pos = ( ( pos / 1000 ) * 9 ); // PTS = 90Khz, REFERENCE_TIME one tick 100ns
	    LogDebugPTS( "Reset - MediaSeeking Pos : ", pos ); 
    }
  } 
}


//
// Test
//
STDMETHODIMP CDVBSub::Test(int status){
	LogDebug("TEST : %i", status);
	return S_OK;
  }


//
// NotifySubtitle
//
void CDVBSub::NotifySubtitle()
{
	LogDebug("NOTIFY - subtitle");
  // calculate the time stamp
  CSubtitle* pSubtitle( NULL );
  pSubtitle = m_pSubDecoder->GetLatestSubtitle();
  if( pSubtitle )
  {
    // PTS to milliseconds ( 90khz )
    LONGLONG pts( 0 ); 
    LONGLONG subtitlePTS( pSubtitle->PTS() );
      
    // TODO: FIX SEEKING!
    pts = ( subtitlePTS - m_basePCR - m_seekDifPCR ) / 90; 
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
// NotifyFirstPTS
//
void CDVBSub::NotifyFirstPTS( ULONGLONG /*firstPTS*/ )
{
  // not used anymore
}


//
// SetPcrPid
//
void CDVBSub::SetPcrPid( LONG pid )
{
  m_pPcrPin->SetPcrPid( pid );
}


//
// SetSubtitlePid
//
void CDVBSub::SetSubtitlePid( LONG pid )
{
  m_pSubtitleInputPin->SetSubtitlePid( pid );
}


//
// SetPcr
//
void CDVBSub::SetPcr( ULONGLONG pcr )
{
  // This gets called from PcrInputPin
  m_curPCR = pcr;

  if( m_firstPCR < 0 )
    m_firstPCR = pcr;

  if( m_basePCR < 0 )
  {
    LogDebugPTS( "SetPcr PCR :", pcr );
    ConnectToTSFileSource();

    if( m_pTSFileSource )
    {
      REFERENCE_TIME posBase( 0 );
      m_pTSFileSource->GetBasePCRPosition( &posBase );

      m_basePCR = ( posBase / 1000 ) * 9;
      LogDebugPTS( "TSFileSource base PCR:", m_basePCR );
    }
	}
  
  if ( m_fixPCR < 0 )
  {
    // This is updated only on startup
    m_fixPCR = pcr - m_basePCR;
    LogDebugPTS( "fixPCR: ", m_fixPCR );
  }
  if( m_seekDifPCR < 0 )
  {
    // updated on every seek (reset)
	  LONGLONG pos( 0 );
	  if( m_pIMediaSeeking )
	  {
      m_pIMediaSeeking->GetCurrentPosition( &pos );
		  if( pos > 0 )
		  {
			  pos = ( ( pos / 1000 ) * 9 ); // PTS = 90Khz, REFERENCE_TIME one tick 100ns
		    LogDebugPTS( "Set PCR - MediaSeeking Pos : ", pos ); 
      }
	  }    
    m_seekDifPCR = pos;
    LogDebugPTS( "fixDifPCR: ", m_fixPCR );
  }
}


//
// ConnectToTSFileSource
//
HRESULT CDVBSub::ConnectToTSFileSource()
{
	// Already connected?
	if( m_pTSFileSource )
		return S_OK;

	IEnumFilters *pEnum( NULL );
  IBaseFilter *pFilter( NULL );
  ULONG cFetched( 0 );
	CComQIPtr<IBaseFilter> pBaseFilter;
	FILTER_INFO pFilterInfo;

	QueryInterface( IID_IBaseFilter,( void**)&pBaseFilter );
	pBaseFilter->QueryFilterInfo( &pFilterInfo );

	pFilterInfo.pGraph->Release();

	if( pFilterInfo.pGraph == NULL )
	{
		return S_FALSE;
	}

  HRESULT hr = pFilterInfo.pGraph->EnumFilters( &pEnum );
  if( FAILED(hr) )
	return hr;

  while( pEnum->Next( 1, &pFilter, &cFetched ) == S_OK )
  {
    FILTER_INFO FilterInfo;
    hr = pFilter->QueryFilterInfo( &FilterInfo );
    if ( FAILED(hr) )
    {
      continue;  // Next one?
    }

    char szName[MAX_FILTER_NAME];
    int cch = WideCharToMultiByte( CP_ACP, 0, FilterInfo.achName,
      MAX_FILTER_NAME, szName, MAX_FILTER_NAME, 0, 0 );

    if( strcmp( szName, "TsFileSource" ) == 0 )
    {
      pFilter->QueryInterface( IID_ITSFileSource, ( void**)&m_pTSFileSource );
    }

    if( FilterInfo.pGraph != NULL )
    {
      FilterInfo.pGraph->Release();
    }
    pFilter->Release();
  }

  if( pEnum )
  {
	  pEnum->Release();
  }
	return S_OK;
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
	if ( riid == IID_IDVBSubtitle )
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