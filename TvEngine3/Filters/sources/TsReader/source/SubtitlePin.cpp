/* 
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
#include "SubtitlePin.h"
#include "AudioPin.h"
#include "Videopin.h"
#define MAX_TIME  86400000L
extern void LogDebug(const char *fmt, ...) ;

CSubtitlePin::CSubtitlePin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinSubtitle"), phr, pFilter, L"Subtitle"),
	m_pTsReaderFilter(pFilter),
  CSourceSeeking(NAME("pinSubtitle"),pUnk,phr,section),
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
}

CSubtitlePin::~CSubtitlePin()
{
	LogDebug("pin:dtor()");
}
STDMETHODIMP CSubtitlePin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
  if (riid == IID_IStreamBufferConfigure)
  {
		LogDebug("sub:IID_IStreamBufferConfigure()");
  }
  if (riid == IID_IStreamBufferInitialize)
  {
		LogDebug("sub:IID_IStreamBufferInitialize()");
  }
  if (riid == IID_IStreamBufferMediaSeeking||riid == IID_IStreamBufferMediaSeeking2)
  {
		LogDebug("sub:IID_IStreamBufferMediaSeeking()");
  }
  if (riid == IID_IStreamBufferSource)
  {
		LogDebug("sub:IID_IStreamBufferSource()");
  }
  if (riid == IID_IStreamBufferDataCounters)
  {
		LogDebug("sub:IID_IStreamBufferDataCounters()");
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

HRESULT CSubtitlePin::GetMediaType(CMediaType *pmt)
{
	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();

	return S_OK;
}
HRESULT CSubtitlePin::CheckConnect(IPin *pReceivePin)
{
/*  HRESULT hr;
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  hr=pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (pinInfo.pFilter==NULL) return E_FAIL;
  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (wcscmp(filterInfo.achName,L"MediaPortal DVBSub2")!=0)
  {
    return E_FAIL;
  }*/
  return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CSubtitlePin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;
	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
			pRequest->cBuffers = 1;
	}
	pRequest->cbBuffer = 0x10000;

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

HRESULT CSubtitlePin::CompleteConnect(IPin *pReceivePin)
{
	LogDebug("pin:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	
  if (SUCCEEDED(hr))
	{
		LogDebug("pin:CompleteConnect() done");
    m_bConnected=true;
	}
	else
	{
		LogDebug("pin:CompleteConnect() failed:%x",hr);
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
	return hr;
}


HRESULT CSubtitlePin::BreakConnect()
{
  m_bConnected=false;
  return CBaseOutputPin::BreakConnect();
//  return CSourceStream::BreakConnect();
}

HRESULT CSubtitlePin::FillBuffer(IMediaSample *pSample)
{
  if (m_pTsReaderFilter->IsTimeShifting())
  {
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

  if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
	{
		Sleep(1);
		EmptySample(pSample);
		return NOERROR;
	}
  
  CAutoLock lock(&m_bufferLock);
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  CBuffer* buffer=demux.GetSubtitle();

	if (m_bDiscontinuity)
  {
    LogDebug("sub:set discontinuity");
    pSample->SetDiscontinuity(TRUE);
    m_bDiscontinuity=FALSE;
  }

	while(buffer == NULL)
	{
		LogDebug("sub: SLEEPING");
		Sleep( 100 );

		if( !m_bRunning )
			{
				LogDebug("sub: FillBuffer - stopping");
				Sleep(1);
				EmptySample(pSample);
				return NOERROR;
			}

		if (demux.EndOfFile())
			{
      LogDebug("sub: FillBuffer - end of file");
			Sleep(1);
			EmptySample(pSample);
			return NOERROR;
			}

		if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
		{
			LogDebug("sub: FillBuffer - seeking detected!");
			Sleep(1);
			EmptySample(pSample); 
			return NOERROR;
		}
		// check if new subtitle data has arrived
		buffer=demux.GetSubtitle();
	}

	LogDebug("sub: SLEEPING DONE");

  if (buffer!=NULL)
  {
    BYTE* pSampleBuffer;
    CRefTime cRefTime;
    if (buffer->MediaTime(cRefTime))
    {
			CPcr pcr = buffer->Pcr();
      LogDebug("sub: buffer->Pcr = %lld" ,pcr.PcrReferenceBase );
      
      cRefTime-=m_rtStart;
      REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
      pSample->SetTime(&refTime,NULL); 
      pSample->SetSyncPoint(TRUE);
			pSample->SetDiscontinuity(TRUE);

      float fTime=(float)cRefTime.Millisecs();
      fTime/=1000.0f;
      LogCurrentPosition();
			LogDebug("sub pSample->SetTime %f ", fTime);
    }
    else
    {
      pSample->SetTime(NULL,NULL);  
    }
	  pSample->SetActualDataLength(buffer->Length());
    pSample->GetPointer(&pSampleBuffer);
    memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
    delete buffer;
    return NOERROR;
  }
  else
  {
    LogDebug("sub:no buffer --- SHOULD NOT HAPPEN!");
		EmptySample(pSample);
    if (demux.EndOfFile()) 
      return S_FALSE;
  }
  return NOERROR;
}


bool CSubtitlePin::IsConnected()
{
  return m_bConnected;
}
// CMediaSeeking
HRESULT CSubtitlePin::ChangeStart()
{
  UpdateFromSeek();
	return S_OK;
}
HRESULT CSubtitlePin::ChangeStop()
{
  UpdateFromSeek();
	return S_OK;
}
HRESULT CSubtitlePin::ChangeRate()
{
	return S_OK;
}


HRESULT CSubtitlePin::OnThreadStartPlay()
{    
  m_bDiscontinuity=TRUE;
  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;
  LogDebug("sub:OnThreadStartPlay(%f)", fStart);
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay( );
}

void CSubtitlePin::SetStart(CRefTime rtStartTime)
{
}
STDMETHODIMP CSubtitlePin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

void CSubtitlePin::UpdateFromSeek()
{
	return;
  
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();

	if (m_rtStart>m_rtDuration)
		m_rtStart=m_rtDuration;
  if (GetTickCount()-m_seekTimer<2000)
  {
    if (m_lastSeek.Millisecs()==m_rtStart.Millisecs()) 
    {
      return;
    }
  }
  m_seekTimer=GetTickCount();
  m_lastSeek=m_rtStart;

  CRefTime rtSeek=m_rtStart;
  float seekTime=(float)rtSeek.Millisecs();
  seekTime/=1000.0f;
  LogDebug("sub seek to %f", seekTime);
  m_bSeeking=true;
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);
   LogDebug("sub seek filter->Iseeking() done");
//  while (m_bInFillBuffer) Sleep(1);
  CAutoLock lock(&m_bufferLock);
  LogDebug("sub seek buffer locked");
  if (ThreadExists()) 
  {
      // next time around the loop, the worker thread will
      // pick up the position change.
      // We need to flush all the existing data - we must do that here
      // as our thread will probably be blocked in GetBuffer otherwise
      
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
        LogDebug("sub seek filter->seekstart");
				m_pTsReaderFilter->SeekStart();
      }
			LogDebug("sub seek begindeliverflush");
      HRESULT hr=DeliverBeginFlush();
      LogDebug("sub:beginflush:%x",hr);
      // make sure we have stopped pushing
			LogDebug("sub seek stop");
      Stop();
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
				LogDebug("sub seek filter->seek");
        m_pTsReaderFilter->Seek(CRefTime(m_rtStart),true);
      }
      // complete the flush
			LogDebug("sub seek deliverendflush");
      hr=DeliverEndFlush();
      LogDebug("sub:endflush:%x",hr);
      
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
				LogDebug("sub seek filter->seekdone");
        m_pTsReaderFilter->SeekDone(rtSeek);
      }
      // restart
      
			LogDebug("sub seek restart");
      m_rtStart=rtSeek;
      Run();
			LogDebug("sub seek running");
  }
  else
  {
    m_pTsReaderFilter->Seek(CRefTime(m_rtStart),false);
  }
	//demux.Flush();
	//demux.SetHoldVideo(false);
  m_bSeeking=false;
  LogDebug("sub seek done---");
}


STDMETHODIMP CSubtitlePin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
//  LogDebug("sub:GetAvailable");
  return CSourceSeeking::GetAvailable( pEarliest, pLatest );
}

STDMETHODIMP CSubtitlePin::GetDuration(LONGLONG *pDuration)
{
 // LogDebug("sub:GetDuration");
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
  return CSourceSeeking::GetDuration(pDuration);
}

STDMETHODIMP CSubtitlePin::GetCurrentPosition(LONGLONG *pCurrent)
{
  LogDebug("sub:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

void CSubtitlePin::SetRunningStatus(bool onOff)
{
	m_bRunning = onOff;
}

void CSubtitlePin::EmptySample(IMediaSample *pSample)    
{
  pSample->SetDiscontinuity(TRUE);
  pSample->SetActualDataLength(0);
  pSample->SetTime(NULL,NULL);  
}

void CSubtitlePin::LogCurrentPosition()  
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
