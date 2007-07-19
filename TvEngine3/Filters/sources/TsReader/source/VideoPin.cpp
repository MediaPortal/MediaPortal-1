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
#include "AudioPin.h"
#include "Videopin.h"

#define MAX_TIME  86400000L
extern void LogDebug(const char *fmt, ...) ;

CVideoPin::CVideoPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinVideo"), phr, pFilter, L"Video"),
	m_pTsReaderFilter(pFilter),
	m_section(section),
  CSourceSeeking(NAME("pinVideo"),pUnk,phr,section)
{
	m_rtStart=0;
  m_bConnected=false;
  m_bMeasureCompensation=false;
	m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute	|
	AM_SEEKING_CanSeekForwards	|
	AM_SEEKING_CanSeekBackwards	|
	AM_SEEKING_CanGetStopPos	|
	AM_SEEKING_CanGetDuration |
  //AM_SEEKING_CanGetCurrentPos |
	AM_SEEKING_Source;
  m_bSeeking=false;
}

CVideoPin::~CVideoPin()
{
	LogDebug("pin:dtor()");
}

bool CVideoPin::IsConnected()
{
  return m_bConnected;
}
STDMETHODIMP CVideoPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
  if (riid == IID_IStreamBufferConfigure)
  {
	    LogDebug("vid:IID_IStreamBufferConfigure()");
  }
  if (riid == IID_IStreamBufferInitialize)
  {
  
	    LogDebug("vid:IID_IStreamBufferInitialize()");
  }
  if (riid == IID_IStreamBufferMediaSeeking||riid == IID_IStreamBufferMediaSeeking2)
  {
	    LogDebug("vid:IID_IStreamBufferMediaSeeking()");
  }
  if (riid == IID_IStreamBufferSource)
  {
	    LogDebug("vid:IID_IStreamBufferSource()");
  }
  if (riid == IID_IStreamBufferDataCounters)
  {
	    LogDebug("vid:IID_IStreamBufferDataCounters()");
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

HRESULT CVideoPin::GetMediaType(CMediaType *pmt)
{
  LogDebug("vid:GetMediaType()");
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  demux.GetVideoStreamType(*pmt);
	return S_OK;
}

HRESULT CVideoPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
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

HRESULT CVideoPin::CheckConnect(IPin *pReceivePin)
{
  HRESULT hr;

  bool mpeg2Video=false;
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  if (demux.GetVideoServiceType()==SERVICE_TYPE_VIDEO_MPEG1 ||
    demux.GetVideoServiceType()==SERVICE_TYPE_VIDEO_MPEG2)
  {
    mpeg2Video=true;
  }
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  hr=pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (pinInfo.pFilter==NULL) return E_FAIL;
  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (mpeg2Video)
  {
    //dont accept FFDShow for mpeg1/2 video playback
    if (wcscmp(filterInfo.achName,L"ffdshow Video Decoder")==0)
    {
      return E_FAIL;
    }
    if (wcscmp(filterInfo.achName,L"ffdshow raw video Decoder")==0)
    {
      return E_FAIL;
    }
  }

  LogDebug("vid:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}
HRESULT CVideoPin::CompleteConnect(IPin *pReceivePin)
{
	LogDebug("vid:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		LogDebug("vid:CompleteConnect() done");
    m_bConnected=true;
	}
	else
	{
		LogDebug("vid:CompleteConnect() failed:%x",hr);
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
  LogDebug("vid:CompleteConnect() ok");
	return hr;
}


HRESULT CVideoPin::BreakConnect()
{
  LogDebug("vid:BreakConnect() ok");
  m_bConnected=false;
  return CSourceStream::BreakConnect();
}

HRESULT CVideoPin::FillBuffer(IMediaSample *pSample)
{
//	::OutputDebugStringA("CVideoPin::FillBuffer()\n");
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
  if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
	{
    LogDebug("vid:isseeking:%d %d",m_pTsReaderFilter->IsSeeking() ,m_bSeeking);
		Sleep(20);
    pSample->SetTime(NULL,NULL); 
	  pSample->SetActualDataLength(0);
    pSample->SetSyncPoint(FALSE);
		return NOERROR;
	}
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();

  m_bInFillBuffer=true;
  CBuffer* buffer=NULL;
  while (buffer==NULL)
  {
      {
        CAutoLock lock(&m_bufferLock);
        buffer=demux.GetVideo();
      }
      if (buffer!=NULL) break;
      if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
      {
        LogDebug("vid:isseeking2");
	      Sleep(20);
        pSample->SetTime(NULL,NULL); 
        pSample->SetActualDataLength(0);
        pSample->SetDiscontinuity(TRUE);
        pSample->SetSyncPoint(FALSE);
        m_bInFillBuffer=false;
	      return NOERROR;
      }
      if (demux.EndOfFile()) 
      {
        m_bInFillBuffer=false;
        return S_FALSE;
      }
      Sleep(10);
  }

  if (m_bDiscontinuity)
  {
    LogDebug("vid:set discontinuity");
    pSample->SetDiscontinuity(TRUE);
    m_bDiscontinuity=FALSE;
  }
  if (buffer!=NULL)
  {
    BYTE* pSampleBuffer;
    CRefTime cRefTime;
    if (buffer->MediaTime(cRefTime))
    {
      cRefTime-=m_rtStart;
      if (m_bMeasureCompensation)
      {
        m_bMeasureCompensation=false;
        m_pTsReaderFilter->Compensation=cRefTime;
        float fTime=(float)cRefTime.Millisecs();
        fTime/=1000.0f;
        //LogDebug("vid:compensation:%03.3f",fTime);
      }
      cRefTime -=m_pTsReaderFilter->Compensation;
      REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
      pSample->SetTime(&refTime,&refTime); 
      pSample->SetSyncPoint(TRUE);
      float fTime=(float)cRefTime.Millisecs();
      fTime/=1000.0f;
      
     //LogDebug("vid:gotbuffer:%d %03.3f",buffer->Length(),fTime);
    }
    else
    {
     // LogDebug("vid:gotbuffer:%d ",buffer->Length());
      pSample->SetTime(NULL,NULL);  
      pSample->SetSyncPoint(FALSE);
    }
	  pSample->SetActualDataLength(buffer->Length());
    pSample->GetPointer(&pSampleBuffer);
    memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
    delete buffer;
    m_bInFillBuffer=false;
    return NOERROR;
  }

  m_bInFillBuffer=false;
  return NOERROR;
}



HRESULT CVideoPin::OnThreadStartPlay()
{    
  m_bDiscontinuity=TRUE;
  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;
  LogDebug("vid:OnThreadStartPlay(%f)", fStart);
  m_bMeasureCompensation=true;
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
	return CSourceStream::OnThreadStartPlay( );
}


	// CSourceSeeking
HRESULT CVideoPin::ChangeStart()
{
    UpdateFromSeek();
  return S_OK;
}
HRESULT CVideoPin::ChangeStop()
{
    UpdateFromSeek();
  return S_OK;
}
HRESULT CVideoPin::ChangeRate()
{
  return S_OK;
}

STDMETHODIMP CVideoPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{ /*
	REFERENCE_TIME rtStop = *pStop;
	REFERENCE_TIME rtCurrent = *pCurrent;
	if (CurrentFlags & AM_SEEKING_RelativePositioning)
	{
		rtCurrent += m_rtStart;
		CurrentFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
		CurrentFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
	}
	if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
	{
		m_rtStart = rtCurrent;
	}
  
	if (StopFlags & AM_SEEKING_RelativePositioning)
	{
		rtStop += m_rtStop;
		StopFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
		StopFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
	}
	if (!(CurrentFlags & AM_SEEKING_NoFlush) && (CurrentFlags & AM_SEEKING_PositioningBitsMask))
  {
    m_pTsReaderFilter->SeekStart();
    CRefTime rtSeek=rtCurrent;
    float seekTime=rtSeek.Millisecs();
    seekTime/=1000.0f;
    LogDebug("seek to %f", seekTime);
  				
    m_rtStart = rtCurrent;
    
	  if (m_pTsReaderFilter->IsActive())
	  {
		  DeliverBeginFlush();
	  }

	  CSourceStream::Stop();

    m_pTsReaderFilter->Seek(CRefTime(rtCurrent));
    
		if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
		{
			m_rtStart = rtCurrent;
		}
	  if (m_pTsReaderFilter->IsActive())
	  {
		  DeliverEndFlush();
	  }
    m_pTsReaderFilter->SeekDone();

    m_bDiscontinuity=TRUE;
    CSourceStream::Run();

	  if (CurrentFlags & AM_SEEKING_ReturnTime)
    {
      *pCurrent=rtCurrent;
    }
			
    return CSourceSeeking::SetPositions(&rtCurrent, CurrentFlags, pStop, StopFlags);

  }*/
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

void CVideoPin::UpdateFromSeek()
{
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();

	if (m_rtStart>m_rtDuration)
		m_rtStart=m_rtDuration;
  if (GetTickCount()-m_seekTimer<5000)
  {
      LogDebug("vid:skip seek");
      return;
  }
  m_seekTimer=GetTickCount();
  m_lastSeek=m_rtStart;

  CRefTime rtSeek=m_rtStart;
  float seekTime=(float)rtSeek.Millisecs();
  seekTime/=1000.0f;
  LogDebug("vid seek to %f", seekTime);
  m_bSeeking=true;
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);
//   LogDebug("vid seek filter->Iseeking() done");
	demux.SetHoldVideo(true);
  while (m_bInFillBuffer) Sleep(1);
  CAutoLock lock(&m_bufferLock);
//  LogDebug("vid seek buffer locked");
  if (ThreadExists()) 
  {
      // next time around the loop, the worker thread will
      // pick up the position change.
      // We need to flush all the existing data - we must do that here
      // as our thread will probably be blocked in GetBuffer otherwise
      
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
//        LogDebug("vid seek filter->seekstart");
				m_pTsReaderFilter->SeekStart();
      }
//			LogDebug("vid seek begindeliverflush");
      HRESULT hr=DeliverBeginFlush();
//      LogDebug("vid:beginflush:%x",hr);
      // make sure we have stopped pushing
//			LogDebug("vid seek stop");
      Stop();
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
//				LogDebug("vid seek filter->seek");
        m_pTsReaderFilter->Seek(CRefTime(m_rtStart),true);
      }
      // complete the flush
//			LogDebug("vid seek deliverendflush");
      hr=DeliverEndFlush();
//      LogDebug("vid:endflush:%x",hr);
      
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
//				LogDebug("vid seek filter->seekdone");
        m_pTsReaderFilter->SeekDone(rtSeek);
      }
      // restart
      
//			LogDebug("vid seek restart");
      m_rtStart=rtSeek;
      Run();
//			LogDebug("vid seek running");
  }
  else
  {
    m_pTsReaderFilter->Seek(CRefTime(m_rtStart),false);
  }
	//demux.Flush();
	demux.SetHoldVideo(false);
  m_bSeeking=false;
  LogDebug("vid seek done---");
}

STDMETHODIMP CVideoPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
//  LogDebug("vid:GetAvailable");
  return CSourceSeeking::GetAvailable( pEarliest, pLatest );
}

STDMETHODIMP CVideoPin::GetDuration(LONGLONG *pDuration)
{
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

STDMETHODIMP CVideoPin::GetCurrentPosition(LONGLONG *pCurrent)
{
 // LogDebug("vid:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}
