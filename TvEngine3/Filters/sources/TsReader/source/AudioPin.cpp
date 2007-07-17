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
#include "audiopin.h"

#define MAX_TIME  86400000L
byte MPEG1AudioFormat[] = 
{
  0x50, 0x00,				//wFormatTag
  0x02, 0x00,				//nChannels
  0x80, 0xBB,	0x00, 0x00, //nSamplesPerSec
  0x00, 0x7D,	0x00, 0x00, //nAvgBytesPerSec
  0x00, 0x03,				//nBlockAlign
  0x00, 0x00,				//wBitsPerSample
  0x16, 0x00,				//cbSize
  0x02, 0x00,				//wValidBitsPerSample
  0x00, 0xE8,				//wSamplesPerBlock
  0x03, 0x00,				//wReserved
  0x01, 0x00,	0x01,0x00,  //dwChannelMask
  0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00

};
extern void LogDebug(const char *fmt, ...) ;

CAudioPin::CAudioPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinAudio"), phr, pFilter, L"Audio"),
	m_pTsReaderFilter(pFilter),
  CSourceSeeking(NAME("pinAudio"),pUnk,phr,section),
	m_section(section)
{
	m_refStartTime=m_rtStart;
	m_bDropPackets=false;
  m_bDropSeek=false;
  m_bConnected=false;
  m_bMeasureCompensation=false;
  m_bInFillBuffer=false;
	m_rtStart=0;
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

CAudioPin::~CAudioPin()
{
	LogDebug("pin:dtor()");
}
STDMETHODIMP CAudioPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
  if (riid == IID_IStreamBufferConfigure)
  {
  
	    LogDebug("aud:IID_IStreamBufferConfigure()");
  }
  if (riid == IID_IStreamBufferInitialize)
  {
  
	    LogDebug("aud:IID_IStreamBufferInitialize()");
  }
  if (riid == IID_IStreamBufferMediaSeeking||riid == IID_IStreamBufferMediaSeeking2)
  {
  
	    LogDebug("aud:IID_IStreamBufferMediaSeeking()");
  }
  if (riid == IID_IStreamBufferSource)
  {
	    LogDebug("aud:IID_IStreamBufferSource()");
  }
  if (riid == IID_IStreamBufferDataCounters)
  {
	    LogDebug("aud:IID_IStreamBufferDataCounters()");
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

HRESULT CAudioPin::GetMediaType(CMediaType *pmt)
{
  LogDebug("aud:GetMediaType()");
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  demux.GetAudioStreamType(demux.GetAudioStream(), *pmt);
	return S_OK;
}

void CAudioPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity=onOff;
}
HRESULT CAudioPin::CheckConnect(IPin *pReceivePin)
{
  //HRESULT hr;
  /*
#ifndef DEBUG
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  hr=pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (pinInfo.pFilter==NULL) return E_FAIL;
  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (wcscmp(filterInfo.achName,L"ffdshow Audio Decoder")==0)
  {
    return E_FAIL;
  }
#endif
  */
  LogDebug("aud:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CAudioPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
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

HRESULT CAudioPin::CompleteConnect(IPin *pReceivePin)
{
	LogDebug("aud:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		LogDebug("aud:CompleteConnect() done");
    m_bConnected=true;
	}
	else
	{
		LogDebug("aud:CompleteConnect() failed:%x",hr);
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
	LogDebug("aud:CompleteConnect() ok");
	return hr;
}

HRESULT CAudioPin::BreakConnect()
{
  m_bConnected=false;
	LogDebug("aud:BreakConnect()");
  return CSourceStream::BreakConnect();
}

HRESULT CAudioPin::FillBuffer(IMediaSample *pSample)
{
 // LogDebug(".");

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
    LogDebug("aud:isseeking");
		Sleep(20);
    pSample->SetTime(NULL,NULL); 
	  pSample->SetActualDataLength(0);
    pSample->SetDiscontinuity(TRUE);
    pSample->SetSyncPoint(FALSE);
		return NOERROR;
	}
  

  m_bInFillBuffer=true;

	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  CBuffer* buffer=NULL;
  while (buffer==NULL)
  {
    {
      CAutoLock lock(&m_bufferLock);
      buffer=demux.GetAudio();
    }
    if (buffer!=NULL) break;
    if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
    {
      LogDebug("aud:isseeking2");
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
    LogDebug("aud:set eof");
      m_bInFillBuffer=false;
      return S_FALSE;
    }
    Sleep(10);
  }

  if (m_bDiscontinuity)
  {
    LogDebug("aud:set discontinuity");
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
        LogDebug("aud:compensation:%03.3f",fTime);
      }
      cRefTime -=m_pTsReaderFilter->Compensation;
      REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
      pSample->SetTime(&refTime,&refTime);  
      pSample->SetSyncPoint(TRUE);
      float fTime=(float)cRefTime.Millisecs();
      fTime/=1000.0f;
      
      LogDebug("aud:gotbuffer:%d %03.3f",buffer->Length(),fTime);
    } 
    else
    {
      //LogDebug("aud:gotbuffer:%d ",buffer->Length());
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


bool CAudioPin::IsConnected()
{
  return m_bConnected;
}
// CMediaSeeking
HRESULT CAudioPin::ChangeStart()
{

  UpdateFromSeek();
	return S_OK;
}
HRESULT CAudioPin::ChangeStop()
{
    UpdateFromSeek();
	return S_OK;
}
HRESULT CAudioPin::ChangeRate()
{
	return S_OK;
}


HRESULT CAudioPin::OnThreadStartPlay()
{    
   m_bDiscontinuity=TRUE;
  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;
  LogDebug("aud:OnThreadStartPlay(%f)", fStart);
  m_bMeasureCompensation=true;
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
	return CSourceStream::OnThreadStartPlay( );
}

void CAudioPin::SetStart(CRefTime rtStartTime)
{
}
STDMETHODIMP CAudioPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

void CAudioPin::UpdateFromSeek()
{
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
  float duration=(float)m_rtDuration.Millisecs();
  duration /=1000.0f;
  LogDebug("aud seek to %f/%f", seekTime, duration);
  m_bSeeking=true;
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);
  LogDebug("aud seek filter->Iseeking() done");
  demux.SetHoldAudio(true);
  while (m_bInFillBuffer) Sleep(1);
  CAutoLock lock(&m_bufferLock);
  LogDebug("aud seek buffer locked");
  if (ThreadExists()) 
  {
      // next time around the loop, the worker thread will
      // pick up the position change.
      // We need to flush all the existing data - we must do that here
      // as our thread will probably be blocked in GetBuffer otherwise
      
			LogDebug("aud seek filter->seekstart");
      m_pTsReaderFilter->SeekStart();
			LogDebug("aud seek begindeliverflush");
      DeliverBeginFlush();
			LogDebug("aud seek stop");
      // make sure we have stopped pushing
      Stop();
			LogDebug("aud seek filter->seek");
      m_pTsReaderFilter->Seek(CRefTime(m_rtStart),true);

      // complete the flush
			LogDebug("aud seek deliverendflush");
      DeliverEndFlush();
			LogDebug("aud seek filter->seekdone");
      m_pTsReaderFilter->SeekDone(rtSeek);

      // restart
			LogDebug("aud seek restart");
      m_rtStart=rtSeek;
      Run();
			LogDebug("aud seek running");
  }
  else
  {
    m_pTsReaderFilter->Seek(CRefTime(m_rtStart),false);
  }
	//demux.Flush();
	demux.SetHoldAudio(false);
  m_bSeeking=false;
  LogDebug("aud seek done---");
}


STDMETHODIMP CAudioPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
//  LogDebug("aud:GetAvailable");
  return CSourceSeeking::GetAvailable( pEarliest, pLatest );
}

STDMETHODIMP CAudioPin::GetDuration(LONGLONG *pDuration)
{
 // LogDebug("aud:GetDuration");
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

STDMETHODIMP CAudioPin::GetCurrentPosition(LONGLONG *pCurrent)
{
 // LogDebug("aud:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}