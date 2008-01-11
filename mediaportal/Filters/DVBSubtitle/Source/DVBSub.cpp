/*
 *	Copyright (C) 2006-2008 Team MediaPortal
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
  m_pSubtitlePin( NULL ),
	m_pSubDecoder( NULL ),
  m_pSubtitleObserver( NULL ),
  m_pTimestampResetObserver( NULL ),
  m_pIMediaSeeking( NULL ),
  m_pTSFileSource( NULL ),
  m_pMediaFilter( NULL ),
  m_basePCR( -1 ),
  m_firstPCR( -1 ),
  m_startTimestamp( -1 ),
  m_seekDifPCR( -1 ),
  m_bSeekingDone( true )
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

	// Create pcr input pin
	m_pPCRPin = new CPcrInputPin( this,
								GetOwner(),
								this,
								&m_Lock,
								&m_ReceiveLock,
								phr );

	if ( m_pPCRPin == NULL )
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
                m_pSubtitlePin, 
                m_pPCRPin );

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
	LogDebug( "CDVBSub::~CDVBSub() - start" );
  if( m_pSubDecoder )
  {
    m_pSubDecoder->SetObserver( NULL );
  }
	delete m_pSubDecoder;
	delete m_pSubtitlePin;
	delete m_pPCRPin;
  delete m_pPMTPin;

  LogDebug( "CDVBSub::~CDVBSub() - end" );
}


//
// GetPin
//
CBasePin * CDVBSub::GetPin( int n )
{
	if( n == 0 )
		return m_pSubtitlePin;

  if( n == 1 )
		return m_pPCRPin;

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
  GetFilterGraph()->QueryInterface( IID_IMediaFilter, (void**)&m_pMediaFilter );
  m_pMediaFilter->GetSyncSource( &m_pReferenceClock );

  // Get media seeking interface if missing
  if( !m_pIMediaSeeking )
  {
    IFilterGraph *pGraph = GetFilterGraph();
	  pGraph->QueryInterface( &m_pIMediaSeeking );
  }

  LogDebugMediaPosition( "Run - media seeking position" );        

  if( ! m_pTSFileSource )
    ConnectToTSFileSource();

  if( m_pTSFileSource && m_basePCR < 0 )
  {
    REFERENCE_TIME posBase( 0 );
    m_pTSFileSource->GetBasePCRPosition( &posBase );

    m_basePCR = ( posBase / 1000 ) * 9;
    LogDebugPTS( "TSFileSource base PCR:", m_basePCR );
  }

  if( m_bSeekingDone )
  {
    m_bSeekingDone = false;
    Reset();
  }
  
  FindVideoPID();

  if( !IsThreadRunning() )
  {
    StartThread();
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

  if( IsThreadRunning() )
  {
    StopThread();
  }

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

  StopThread();

  // Make sure no further processing is done
  if( m_pSubDecoder ) m_pSubDecoder->Reset();
	if( m_pSubtitlePin ) m_pSubtitlePin->Reset();

  LogDebug( "Release ITSFileSource" );
  if( m_pTSFileSource ) m_pTSFileSource->Release();
  m_pTSFileSource = NULL;
  LogDebug( "Release ITSFileSource - done" );
  
  LogDebug( "Release m_pMediaFilter" );
  if( m_pMediaFilter )
  {
    m_pMediaFilter->Release();
    m_pMediaFilter = NULL;
  }
  LogDebug( "Release m_pMediaFilter - done" );

  LogDebug( "Release m_pIMediaSeeking" );
  if( m_pIMediaSeeking )
  {
    m_pIMediaSeeking->Release();
    m_pIMediaSeeking = NULL;
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

	m_pSubDecoder->Reset();
	m_pSubtitlePin->Reset();
  m_pPCRPin->Reset();
  m_pPMTPin->Reset();

  m_seekDifPCR = -1;
  //m_fixPCR = -1; // update this only in the beginning
  //m_firstPCR = -1;

  // Notify reset observer
  if( m_pTimestampResetObserver )
  {
    (*m_pTimestampResetObserver)();
  }

  FindVideoPID();

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
// NotifySubtitle
//
void CDVBSub::NotifySubtitle()
{
  LogDebug( "subtitle arrived" );

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
// SetPcr
//
void CDVBSub::SetPcr( ULONGLONG pcr )
{
  //CAutoLock cObjectLock( m_pLock );
  
  // This gets called from PcrInputPin
  m_curPCR = pcr;

  if( m_firstPCR < 0 )
    m_firstPCR = pcr;

  if( m_basePCR < 0 )
  {
    LogDebugPTS( "SetPcr PCR :", pcr );

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
	  if( m_pIMediaSeeking && m_State == State_Running )
	  {
      m_pIMediaSeeking->GetCurrentPosition( &pos );
		  if( pos > 0 )
		  {
			  pos = ( ( pos / 1000 ) * 9 ); // PTS = 90Khz, REFERENCE_TIME one tick 100ns
      }
	  }  

    m_seekDifPCR = pos;
    LogDebugPTS( "fixDifPCR: ", m_fixPCR );
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
// ConnectToTSFileSource
//
HRESULT CDVBSub::ConnectToTSFileSource()
{
	// Already connected?
	if( m_pTSFileSource )
		return S_OK;
  
  if( m_State == State_Stopped )
    return S_FALSE;

  LogDebug( "ConnectToTSFileSource - start");

  IEnumFilters *enumFilters;
  IBaseFilter *curFilter;
  ULONG fetched;
  FILTER_INFO pInfo;
  IFilterGraph *pGraph = GetFilterGraph();

  pGraph->EnumFilters( &enumFilters );
  enumFilters->Reset();
  
  while( enumFilters->Next( 1, &curFilter, &fetched ) == S_OK )
  {
    curFilter->QueryFilterInfo( &pInfo );
    if( wcscmp( pInfo.achName, L"TsFileSource" ) == 0 )
    {
      curFilter->QueryInterface( IID_ITSFileSource, ( void**)&m_pTSFileSource );
    }
    curFilter->Release();
    curFilter = NULL;
  }

  enumFilters->Release();
  enumFilters = NULL;

  if( m_pTSFileSource )
  {
    LogDebug( "ConnectToTSFileSource - done");  
    return S_OK;
  }
  else
  {
    LogDebug( "ConnectToTSFileSource - No TSFileSource filter in graph, using a remote client?");
    m_basePCR = 0;
    m_fixPCR = 0;
    return S_FALSE;
  }
}


//
// SetPid
//
HRESULT CDVBSub::SetPid( CBaseInputPin* pin, LONG pid, MEDIA_SAMPLE_CONTENT sampleContent )
{
  if( pin == m_pSubtitlePin )
  {
    LogDebug( "CDVBSub::SetPid - Subtitle pid - %x", pid );
    int SubtitlePidCount( m_SubtitlePinMapping.size() );
    for( int i( 0 ) ; i < SubtitlePidCount ; i++ )
    {
      if( m_SubtitlePinMapping[i].pid == pid )
        return S_OK; 
    }
    m_SubtitlePinMapping.resize( SubtitlePidCount + 1 );
    m_SubtitlePinMapping[SubtitlePidCount].pid = pid;
    m_SubtitlePinMapping[SubtitlePidCount].mappingState = PidAvailable;
    m_SubtitlePinMapping[SubtitlePidCount].sampleContent = sampleContent;
  }
  if( pin == m_pPCRPin )
  {
    LogDebug( "CDVBSub::SetPid - PCR pid - %x", pid );
    int PCRPidCount( m_PCRPinMapping.size() );
    for( int i( 0 ) ; i < PCRPidCount ; i++ )
    {
      if( m_PCRPinMapping[i].pid == pid )
        return S_OK; 
    }
    m_PCRPinMapping.resize( PCRPidCount + 1 );
    m_PCRPinMapping[PCRPidCount].pid = pid;
    m_PCRPinMapping[PCRPidCount].mappingState = PidAvailable;
    m_PCRPinMapping[PCRPidCount].sampleContent = sampleContent;
  }
  if( pin == m_pPMTPin )
  {
    LogDebug( "CDVBSub::SetPid - PMT pid - %x", pid );
    int PMTPidCount( m_PMTPinMapping.size() );
    for( int i( 0 ) ; i < PMTPidCount ; i++ )
    {
      if( m_PMTPinMapping[i].pid == pid )
        return S_OK; 
    }
    m_PMTPinMapping.resize( PMTPidCount + 1 );
    m_PMTPinMapping[PMTPidCount].pid = pid;
    m_PMTPinMapping[PMTPidCount].mappingState = PidAvailable;
    m_PMTPinMapping[PMTPidCount].sampleContent = sampleContent;
  }

  return S_OK;
}


//
// FindVideoPID
//
HRESULT CDVBSub::FindVideoPID()
{
  LogDebug( "CDVBSub::FindVideoPID()" );

  IFilterGraph *pGraph = GetFilterGraph();
  IBaseFilter *pDemuxer = NULL;
  HRESULT hr = pGraph->FindFilterByName( L"MPEG-2 Demultiplexer", &pDemuxer );
  
  if( hr != S_OK )
  {
    LogDebug( "  Unable to find demuxer!" );
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
                    m_pPMTPin->SetVideoPid( pidMap.ulPID );
                    LogDebug( "  found video PID %d", pidMap.ulPID );
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


// Worker thread functions
//
// ThreadProc
//
void CDVBSub::ThreadProc()
{
  while( !ThreadIsStopping( 100 ) )
  {
    int SubtitlePidCount( m_SubtitlePinMapping.size() );
    if( SubtitlePidCount > 0 )
    {
      for( int i( 0 ) ; i < SubtitlePidCount ; i++ )
      {
        if( m_SubtitlePinMapping[i].mappingState == PidAvailable )
        {
          if( S_OK == MapPidToDemuxer( m_pSubtitlePin, m_SubtitlePinMapping[i].pid, m_SubtitlePinMapping[i].sampleContent ) )
          {
            m_SubtitlePinMapping[i].mappingState = PidMapped;
          }
        }
      }
    }

    int PCRPidCount( m_PCRPinMapping.size() );
    if( PCRPidCount > 0 )
    {
      for( int i( 0 ) ; i < PCRPidCount ; i++ )
      {
        if( m_PCRPinMapping[i].mappingState == PidAvailable )
        {
          if( S_OK == MapPidToDemuxer( m_pPCRPin, m_PCRPinMapping[i].pid, m_PCRPinMapping[i].sampleContent ) )
          {
            m_PCRPinMapping[i].mappingState = PidMapped;
          }
        }
      }
    }

    int PMTPidCount( m_PMTPinMapping.size() );
    if( PMTPidCount > 0 )
    {
      for( int i( 0 ) ; i < PMTPidCount ; i++ )
      {
        if( m_PMTPinMapping[i].mappingState == PidAvailable )
        {
          if( S_OK == MapPidToDemuxer( m_pPMTPin, m_PMTPinMapping[i].pid, m_PMTPinMapping[i].sampleContent ) )
          {
            m_PMTPinMapping[i].mappingState = PidMapped;
          }
        }
      }
    }
  }
  Sleep( 10 );
}

//
// MapPidToDemuxer
//
HRESULT CDVBSub::MapPidToDemuxer( CBaseInputPin* pPin, LONG pid, MEDIA_SAMPLE_CONTENT sampleContent )
{
  IMPEG2PIDMap	*pMap( NULL );
	IEnumPIDMap		*pPidEnum( NULL );
	PID_MAP			  pm;
	ULONG			    count;
  HRESULT       hr( 0 );

  if( m_State != State_Running )
  {
    //return S_FALSE;
  }

  if( pPin == m_pSubtitlePin )
  {
    LogDebug( "CDVBSub::MapPidToDemuxer -Mapping subtitle pid - %x", pid );
    hr = m_pSubtitlePin->GetDemuxerPin()->QueryInterface( IID_IMPEG2PIDMap, (void**)&pMap );
  }
  if( pPin == m_pPCRPin )
  {
    LogDebug( "CDVBSub::MapPidToDemuxer -Mapping PCR pid - %x", pid );
    hr = m_pPCRPin->GetDemuxerPin()->QueryInterface( IID_IMPEG2PIDMap, (void**)&pMap );
  }
  if( pPin == m_pPMTPin )
  {
    LogDebug( "CDVBSub::MapPidToDemuxer -Mapping PMT pid - %x", pid );
    hr = m_pPMTPin->GetDemuxerPin()->QueryInterface( IID_IMPEG2PIDMap, (void**)&pMap );
  }

	if( SUCCEEDED(hr) && pMap != NULL )
	{
		hr = pMap->EnumPIDMap( &pPidEnum );
		if( SUCCEEDED(hr) && pPidEnum!=NULL )
		{
			while( pPidEnum->Next( 1, &pm, &count ) == S_OK )
			{
				if ( count != 1 )
				{
					break;
				}
			}
      if( m_State != State_Running )
      {
//        return S_FALSE;
      }
      hr = pMap->MapPID( 1, (ULONG*)&pid, sampleContent );
			pPidEnum->Release();
		}
		pMap->Release();
	}

  if( hr != S_OK )
  {
    LogDebug( "CDVBSub::MapPidToDemuxer failed! - %d", hr );
  }
  
  return hr;
}






















































