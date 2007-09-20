/** 
 *	Copyright (C) 2005 Team MediaPortal
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

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <streams.h>
#include <sbe.h>
#include "tsreader.h"
#include "AudioPin.h"
#include "Videopin.h"
#include "TeletextPin.h"
#define MAX_TIME  86400000L
extern void LogDebug(const char *fmt, ...) ;

CTeletextPin::CTeletextPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinTeletext"), phr, pFilter, L"Teletext"),
	m_pTsReaderFilter(pFilter),
  CSourceSeeking(NAME("pinTeletext"),pUnk,phr,section),
	m_section(section)
{
	m_rtStart=0;
  m_bConnected=false;
	m_dwSeekingCaps =
	AM_SEEKING_CanSeekAbsolute	|
	AM_SEEKING_CanSeekForwards	|
	AM_SEEKING_CanSeekBackwards	|
	AM_SEEKING_CanGetStopPos	|
	AM_SEEKING_CanGetDuration	|
  //AM_SEEKING_CanGetCurrentPos |
	AM_SEEKING_Source;
  m_bSeeking=false;

  LogDebug("Creating teletext output pin");
}

CTeletextPin::~CTeletextPin()
{
	LogDebug("ttxt pin:dtor()");
}

bool CTeletextPin::IsConnected()
{
	//LogDebug("CTelextPin connected? %i",m_bConnected); 
  return m_bConnected;
}
STDMETHODIMP CTeletextPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
  if (riid == IID_IStreamBufferConfigure)
  {
		LogDebug("ttxt:IID_IStreamBufferConfigure()");
  }
  if (riid == IID_IStreamBufferInitialize)
  {
		LogDebug("ttxt:IID_IStreamBufferInitialize()");
  }
  if (riid == IID_IStreamBufferMediaSeeking||riid == IID_IStreamBufferMediaSeeking2)
  {
		LogDebug("ttxt:IID_IStreamBufferMediaSeeking()");
  }
  if (riid == IID_IStreamBufferSource)
  {
		LogDebug("ttxt:IID_IStreamBufferSource()");
  }
  if (riid == IID_IStreamBufferDataCounters)
  {
		LogDebug("ttxt:IID_IStreamBufferDataCounters()");
  }
  if (riid == IID_IMediaSeeking)
  {
		return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  if (riid == IID_IMediaPosition)
  {
		return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CTeletextPin::GetMediaType(CMediaType *pmt)
{
	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();

	return S_OK;
}

HRESULT CTeletextPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;
	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
			pRequest->cBuffers = 30;
	}
	pRequest->cbBuffer = 8192;

	ALLOCATOR_PROPERTIES Actual;
	hr = pAlloc->SetProperties(pRequest, &Actual);
	if (FAILED(hr))
	{
		return hr;
	}

	if (Actual.cbBuffer < pRequest->cbBuffer)
	{
		return E_FAIL;
	}

	return S_OK;
}

HRESULT CTeletextPin::CheckConnect(IPin *pReceivePin)
{
  HRESULT hr;
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;

  hr=pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  else if (pinInfo.pFilter==NULL) return E_FAIL;
  else pinInfo.pFilter->Release(); // we dotn need the filter just the info

  // we only want to connect to the DVB subtitle input pin
  // on the subtitle filter (and not the teletext one for example!)
  if( wcscmp(pinInfo.achName, L"TeletextIn") != 0){
    //LogDebug("Ttxt pin: Cant connect to pin name %s", pinInfo.achName);	
	return E_FAIL;
  }

  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  filterInfo.pGraph->Release();
  pinInfo.pFilter->Release(); // we dotn need the filter just the info

  if (!SUCCEEDED(hr)) return E_FAIL;
  if (wcscmp(filterInfo.achName,L"MediaPortal DVBSub2")!=0)
  {
	//LogDebug("Ttxt pin: Cant connect to filter name %s", filterInfo.achName);
    return E_FAIL;
  }
  return CBaseOutputPin::CheckConnect(pReceivePin);
}
HRESULT CTeletextPin::CompleteConnect(IPin *pReceivePin)
{
  m_bInFillBuffer=false;
	LogDebug("teletext pin:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	
  if (SUCCEEDED(hr))
	{
		LogDebug("teletext pin:CompleteConnect() done");
    m_bConnected=true;
	}
	else
	{
		LogDebug("teletext pin:CompleteConnect() failed:%x",hr);
	}

  if (m_pTsReaderFilter->IsTimeShifting())
  {
    //m_rtDuration=CRefTime(MAX_TIME);
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  LogDebug("teletext pin:CompleteConnect() ok");
	return hr;
}


HRESULT CTeletextPin::BreakConnect()
{
  LogDebug("ttxt:BreakConnect() ok");
  m_bConnected=false;
  //return CBaseOutputPin::BreakConnect();
  return CSourceStream::BreakConnect();
}

HRESULT CTeletextPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    //get file-duration and set m_rtDuration
    GetDuration(NULL);

    //if the filter is currently seeking to a new position
    //or this pin is currently seeking to a new position then
    //we dont try to read any packets, but simply return...
    if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
	  {
      LogDebug("vid:isseeking:%d %d",m_pTsReaderFilter->IsSeeking() ,m_bSeeking);
		  Sleep(20);
      pSample->SetTime(NULL,NULL); 
	    pSample->SetActualDataLength(0);
      pSample->SetSyncPoint(FALSE);
		  return NOERROR;
	  }

    //get next buffer from demultiplexer
    m_bInFillBuffer=true;
	  CDeMultiplexer& demux = m_pTsReaderFilter->GetDemultiplexer();
    CBuffer* buffer = NULL;

	  if (m_bDiscontinuity)
    {
      LogDebug("ttxt:set discontinuity");
      pSample->SetDiscontinuity(TRUE);
      m_bDiscontinuity=FALSE;
    }

    //LogDebug("sub: FillBuffer - start blocking the output pin...");

	  while (buffer == NULL)
	  {
		  Sleep(1);

		  if (!m_bRunning)
			  {
				  LogDebug("ttxt: FillBuffer - stopping");
				  Sleep(1);
				  EmptySample(pSample);
				  m_bInFillBuffer=false;
          return NOERROR;
			  }

		  if (demux.EndOfFile())
			  {
        LogDebug("ttxt: FillBuffer - end of file");
			  Sleep(1);
			  EmptySample(pSample);
			  m_bInFillBuffer=false;
        return NOERROR;
			  }

		  if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
		  {
			  LogDebug("ttxt: FillBuffer - seeking detected!");
			  Sleep(1);
			  EmptySample(pSample); 
        m_bInFillBuffer=false;
			  return NOERROR;
		  }
      // check if new teletext data has arrived
      {
        CAutoLock lock(&m_bufferLock);
        buffer=demux.GetTeletext();
      }
	  }

	  //LogDebug("sub: SLEEPING DONE");

    if (buffer!=NULL)
    {
      BYTE* pSampleBuffer;
      CRefTime cRefTime;
      if (buffer->MediaTime(cRefTime))
      {
			  //CPcr pcr = buffer->Pcr();
        //LogDebug("sub: buffer->Pcr = %lld" ,pcr.PcrReferenceBase );
        
        cRefTime-=m_rtStart;
        REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
        pSample->SetTime(&refTime,NULL); 
        pSample->SetSyncPoint(TRUE);
			  pSample->SetDiscontinuity(TRUE);

        float fTime=(float)cRefTime.Millisecs();
        fTime/=1000.0f;
        //LogCurrentPosition();
			  //LogDebug("sub pSample->SetTime %f ", fTime);
      }
      else
      {
        pSample->SetTime(NULL,NULL);  
      }
	    pSample->SetActualDataLength(buffer->Length());
      pSample->GetPointer(&pSampleBuffer);
      memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
      delete buffer;
      m_bInFillBuffer=false;
      return NOERROR;
    }
    else
    {
      LogDebug("ttxt:no buffer --- THIS SHOULD NOT HAPPEN!");
		  EmptySample(pSample);
      if (demux.EndOfFile())
      {
        m_bInFillBuffer=false;
        return S_FALSE;
      }
    }
  }
  catch(...)
  {
    LogDebug("ttxt:fillbuffer exception");
  }

  m_bInFillBuffer=false;
  return NOERROR;
}

//******************************************************
/// Called when thread is about to start delivering data to the filter
/// 
HRESULT CTeletextPin::OnThreadStartPlay()
{    
  m_bInFillBuffer=false;
  m_bDiscontinuity=TRUE;
  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;
  LogDebug("ttxt:OnThreadStartPlay(%f)", fStart);
  //tell demuxer to delete any subtitle packets it still might have
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  demux.FlushTeletext();
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay( );
}

// CMediaSeeking
HRESULT CTeletextPin::ChangeStart()
{
  UpdateFromSeek();
	return S_OK;
}
HRESULT CTeletextPin::ChangeStop()
{
  UpdateFromSeek();
	return S_OK;
}
HRESULT CTeletextPin::ChangeRate()
{
  if( m_dRateSeeking <= 0 ) 
  {
      m_dRateSeeking = 1.0;  // Reset to a reasonable value.
      return E_FAIL;
  }
  UpdateFromSeek();
  return S_OK;
}

STDMETHODIMP CTeletextPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

//******************************************************
/// Returns true if a thread is currently executing in UpdateFromSeek()
/// 
bool CTeletextPin::IsSeeking()
{
  return m_binUpdateFromSeek;
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
/// 
void CTeletextPin::UpdateFromSeek()
{
  m_binUpdateFromSeek=true;
  
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  CTsDuration tsduration=m_pTsReaderFilter->GetDuration();

	//if (m_rtStart>m_rtDuration)
	//	m_rtStart=m_rtDuration;
  
  //there is a bug in directshow causing UpdateFromSeek() to be called multiple times 
  //directly after eachother
  //for a single seek operation. To 'fix' this we only perform the seeking operation
  //if we didnt do a seek in the last 5 seconds...
  if (GetTickCount()-m_seekTimer<5000)
  {
    if (m_lastSeek==m_rtStart)
    {
      LogDebug("ttxt:skip seek");
      m_binUpdateFromSeek=false;
      return;
    }
  }
  
  //Note that the seek timestamp (m_rtStart) is done in the range
  //from earliest - latest from GetAvailable()
  //We however would like the seek timestamp to be in the range 0-fileduration
  m_seekTimer=GetTickCount();
  m_lastSeek=m_rtStart;

  CRefTime rtSeek=m_rtStart;
  float seekTime=(float)rtSeek.Millisecs();
  seekTime/=1000.0f;
  
  
  //get the earliest timestamp available in the file
  float earliesTimeStamp= tsduration.StartPcr().ToClock() - tsduration.FirstStartPcr().ToClock();
  if (earliesTimeStamp<0) earliesTimeStamp=0;

  //correct the seek time
  seekTime-=earliesTimeStamp;
  if (seekTime<0) seekTime=0;
  LogDebug("sub seek to %f", seekTime);
  
  seekTime*=1000.0f;
  rtSeek = CRefTime((LONG)seekTime);

  //if another output pin is seeking, then wait until its finished
  m_bSeeking=true;
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);

  //tell demuxer to stop deliver subtitle data and wait until 
  //FillBuffer() finished
	demux.SetHoldSubtitle(true);
  while (m_bInFillBuffer) Sleep(1);
  CAutoLock lock(&m_bufferLock);

  //if a pin-output thread exists...
  if (ThreadExists()) 
  {
    //normally the audio pin does the acutal seeking
    //check if its connected. If not, we'll do the seeking
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
        //tell the filter we are starting a seek operation
				m_pTsReaderFilter->SeekStart();
      }

	  	//deliver a begin-flush to the codec filter so it stops asking for data
      HRESULT hr=DeliverBeginFlush();

      //stop the thread
      Stop();
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
        //do the seek..
        m_pTsReaderFilter->Seek(rtSeek,true);
      }

      //deliver a end-flush to the codec filter so it will start asking for data again
      hr=DeliverEndFlush();
      
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
        //tell filter we're done with seeking
        m_pTsReaderFilter->SeekDone(rtSeek);
      }

      //set our start time
      //m_rtStart=rtSeek;

      // and restart the thread
      Run();
  }
  else
  {
    //no thread running? then simply seek to the position
    m_pTsReaderFilter->Seek(rtSeek,false);
  }
  
  //tell demuxer to start deliver subtitle packets again
	demux.SetHoldSubtitle(false);

  //clear flags indiciating that the pin is seeking
  m_bSeeking=false;
  LogDebug("sub seek done---");
  m_binUpdateFromSeek=false;
}

//******************************************************
/// GetAvailable() returns 
/// pEarliest -> the earliest (pcr) timestamp in the file
/// pLatest   -> the latest (pcr) timestamp in the file
/// 
STDMETHODIMP CTeletextPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
//  LogDebug("vid:GetAvailable");
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    CTsDuration duration=m_pTsReaderFilter->GetDuration();
    if (pEarliest)
    {
      //return the startpcr, which is the earliest pcr timestamp available in the timeshifting file 
      double d2=duration.StartPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pEarliest= mediaTime;
    }
    if (pLatest)
    {
      //return the endpcr, which is the latest pcr timestamp available in the timeshifting file 
      double d2=duration.EndPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pLatest= mediaTime;
    }
    return S_OK;
  }

  //not timeshifting, then leave it to the default sourceseeking class
  //which returns earliest=0, latest=m_rtDuration
  return CSourceSeeking::GetAvailable( pEarliest, pLatest );
}

//******************************************************
/// Returns the file duration in REFERENCE_TIME
/// For nomal .ts files it returns the current pcr - first pcr in the file
/// for timeshifting files it returns the current pcr - the first pcr ever read
/// So the duration keeps growing, even if timeshifting files are wrapped and being resued!
//
STDMETHODIMP CTeletextPin::GetDuration(LONGLONG *pDuration)
{
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    CTsDuration duration=m_pTsReaderFilter->GetDuration();
    CRefTime totalDuration=duration.TotalDuration();
    m_rtDuration=totalDuration;
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  return CSourceSeeking::GetDuration(pDuration);
}

//******************************************************
/// GetCurrentPosition() simply returns that this is not implemented by this pin
/// reason is that only the audio/video renderer now exactly the 
/// current playing position and they do implement GetCurrentPosition()
/// 
STDMETHODIMP CTeletextPin::GetCurrentPosition(LONGLONG *pCurrent)
{
//  LogDebug("sub:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

void CTeletextPin::SetRunningStatus(bool onOff)
{
	m_bRunning = onOff;
}

void CTeletextPin::EmptySample(IMediaSample *pSample)    
{
  pSample->SetDiscontinuity(TRUE);
  pSample->SetActualDataLength(0);
  pSample->SetTime(NULL,NULL);  
}

void CTeletextPin::LogCurrentPosition()  
{
  IFilterGraph* pGraph = m_pTsReaderFilter->GetFilterGraph();
	IMediaSeeking* pMediaSeeking( NULL );

	if( pGraph ) 
	{
		pGraph->QueryInterface( &pMediaSeeking );
		pGraph->Release();
	}

	LONGLONG pos( 0 );
	pMediaSeeking->GetCurrentPosition( &pos );
	//pMediaSeeking->Release();
	float fPos = (float)pos;
	fPos = ( ( fPos / 10000000 ) );			
	LogDebug("sub current position %f", fPos );
}
