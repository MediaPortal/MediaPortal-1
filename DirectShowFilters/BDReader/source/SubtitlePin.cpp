/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
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

#include "StdAfx.h"
#include <streams.h>
#include "SubtitlePin.h"
#include "bdreader.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);
extern void SetThreadName(DWORD dwThreadID, char* threadName);

CSubtitlePin::CSubtitlePin(LPUNKNOWN pUnk, CBDReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
  CSourceStream(NAME("pinSubtitle"), phr, pFilter, L"Subtitle"),
  m_pFilter(pFilter),
  CSourceSeeking(NAME("pinSubtitle"),pUnk,phr,section),
  m_section(section)
{
  m_rtStart = 0;
  m_bConnected=false;
  m_dwSeekingCaps =
  AM_SEEKING_CanSeekAbsolute  |
  AM_SEEKING_CanSeekForwards  |
  AM_SEEKING_CanSeekBackwards |
  AM_SEEKING_CanGetStopPos  |
  AM_SEEKING_CanGetDuration |
  //AM_SEEKING_CanGetCurrentPos |
  AM_SEEKING_Source;
  m_bSeeking = false;
  m_bInFillBuffer = false;
  m_bPresentSample = false;
  m_bRunning = false;
}

CSubtitlePin::~CSubtitlePin()
{
}

bool CSubtitlePin::IsConnected()
{
  return m_bConnected;
}
STDMETHODIMP CSubtitlePin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
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

HRESULT CSubtitlePin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
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

HRESULT CSubtitlePin::CheckConnect(IPin *pReceivePin)
{
  HRESULT hr;
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;

  hr = pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  else if (pinInfo.pFilter == NULL) return E_FAIL;
  else pinInfo.pFilter->Release(); // we dont need the filter just the info

  // we only want to connect to the DVB subtitle input pin
  // on the subtitle filter (and not the teletext one for example!)
  if (wcscmp(pinInfo.achName, L"In") != 0)
  {
    //LogDebug("sub pin: Cant connect to pin name %s", pinInfo.achName);
    return E_FAIL;
  }

  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  filterInfo.pGraph->Release();

  if (!SUCCEEDED(hr)) return E_FAIL;
  if (wcscmp(filterInfo.achName, L"MediaPortal DVBSub3") !=0)
  {
    //LogDebug("sub pin: Cant connect to filter name %s", filterInfo.achName);
    return E_FAIL;
  }
  return CBaseOutputPin::CheckConnect(pReceivePin);
}
HRESULT CSubtitlePin::CompleteConnect(IPin *pReceivePin)
{
  m_bInFillBuffer = false;
  HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);

  if (SUCCEEDED(hr))
  {
    m_bConnected = true;
  }
  else
  {
    LogDebug("pin:CompleteConnect() failed:%x", hr);
  }

  REFERENCE_TIME refTime;
  m_pFilter->GetDuration(&refTime);
  m_rtDuration = CRefTime(refTime);
  
  return hr;
}

HRESULT CSubtitlePin::BreakConnect()
{
  //LogDebug("sub:BreakConnect() ok");
  m_bConnected = false;
  return CSourceStream::BreakConnect();
}

DWORD CSubtitlePin::ThreadProc()
{
  SetThreadName(-1, "BDReader_SUBTITLE");
  return __super::ThreadProc();
}

HRESULT CSubtitlePin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();
    Packet* buffer=NULL;
    do
    {
      m_bInFillBuffer = true;

      if (m_pFilter->IsStopping() || !m_bRunning)
      {
        //LogDebug("sub:isseeking:%d %d",m_pFilter->IsSeeking() ,m_bSeeking);
        Sleep(20);
        pSample->SetTime(NULL, NULL);
        pSample->SetActualDataLength(0);
        pSample->SetSyncPoint(false);
        pSample->SetDiscontinuity(false);
        m_bInFillBuffer = false;
        return NOERROR;
      }

      {
        CAutoLock lock(&m_bufferLock);
        buffer = demux.GetSubtitle();
      }

      if (demux.EndOfFile())
      {
        LogDebug("sub: Set EOF");
        pSample->SetTime(NULL, NULL);
        pSample->SetActualDataLength(0);
        pSample->SetSyncPoint(false);
        pSample->SetDiscontinuity(true);
        m_bInFillBuffer = false;
        return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }

      if (buffer == NULL)
      {
        m_bInFillBuffer = false;
        Sleep(20);
      }
      else
      {
        if (m_bDiscontinuity || buffer->bDiscontinuity)
        {
          LogDebug("sub: Set discontinuity");
          pSample->SetDiscontinuity(TRUE);
          m_bDiscontinuity = FALSE;
        }

        pSample->SetTime(NULL, NULL);
        pSample->SetSyncPoint(FALSE);
        BYTE* pSampleBuffer;
        pSample->SetActualDataLength(buffer->GetDataSize());
        pSample->GetPointer(&pSampleBuffer);
        memcpy(pSampleBuffer, buffer->GetData(), buffer->GetDataSize());

        delete buffer;
      }
      m_bInFillBuffer = false;
    } while (buffer == NULL);
    return NOERROR;
  }
  catch(...)
  {
    LogDebug("sub: Fillbuffer exception");
  }

  m_bInFillBuffer = false;
  return NOERROR;
}

//******************************************************
/// Called when thread is about to start delivering data to the filter
///
HRESULT CSubtitlePin::OnThreadStartPlay()
{
  //set discontinuity flag indicating to codec that the new data
  //is not belonging to any previous data
  m_bDiscontinuity = TRUE;
  m_bInFillBuffer = false;
  m_bPresentSample = false;

  float fStart = (float)m_rtStart.Millisecs();
  fStart/=1000.0f;

  //tell demuxer to start deliver subtitle packets again
  CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();
  demux.SetHoldSubtitle(false);

  // New seek can occur before video changing complete.
  // We must keep separate waiting loops to avoid hangs.
//  while(m_pFilter->IsSeeking() && !m_pFilter->m_bStopping) Sleep(5);
  
//  if (m_pFilter->GetVideoPin()->IsConnected())
//    while(demux.IsMediaChanging() && !m_pFilter->m_bStopping) Sleep(5);

  LogDebug("sub: OnThreadStartPlay(%f)", fStart);

  //start playing
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay();
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
  if (m_dRateSeeking <= 0)
  {
      m_dRateSeeking = 1.0;  // Reset to a reasonable value.
      return E_FAIL;
  }
  UpdateFromSeek();
  return S_OK;
}

STDMETHODIMP CSubtitlePin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
///
void CSubtitlePin::UpdateFromSeek()
{
  CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();

  //there is a bug in directshow causing UpdateFromSeek() to be called multiple times
  //directly after eachother
  //for a single seek operation. To 'fix' this we only perform the seeking operation
  //if we didnt do a seek in the last 5 seconds...
  if (GetTickCount() - m_seekTimer<5000)
  {
    if (m_lastSeek == m_rtStart)
    {
      LogDebug("sub:skip seek");
      return;
    }
  }

  m_seekTimer = GetTickCount();
  m_lastSeek = m_rtStart;

  CRefTime rtSeek = m_rtStart;
  float seekTime = (float)rtSeek.Millisecs();
  seekTime /= 1000.0f;

  if (seekTime < 0) seekTime = 0;
  LogDebug("sub seek to %f", seekTime);

  seekTime *= 1000.0f;
  rtSeek = CRefTime((LONG)seekTime);

  //if another output pin is seeking, then wait until its finished
  m_bSeeking = true;
  //while (m_pFilter->IsSeeking()) Sleep(1);

  //tell demuxer to stop deliver subtitle data and wait until
  //FillBuffer() finished
  demux.SetHoldSubtitle(true);
  while (m_bInFillBuffer) Sleep(1);
  CAutoLock lock(&m_bufferLock);

  //if a pin-output thread exists...
  if (ThreadExists())
  {
    HRESULT hr = DeliverBeginFlush();

    Stop();

    hr = DeliverEndFlush();

    Run();
  }
  else
  {
    //no thread running? then simply seek to the position
    //m_pFilter->Seek(rtSeek, false);
  }

  //tell demuxer to start deliver subtitle packets again
  demux.SetHoldSubtitle(false);

  //clear flags indiciating that the pin is seeking
  m_bSeeking = false;
  LogDebug("sub seek done---");
}

STDMETHODIMP CSubtitlePin::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  //LogDebug("sub:GetAvailable");
  return CSourceSeeking::GetAvailable(pEarliest, pLatest);
}

STDMETHODIMP CSubtitlePin::GetDuration(LONGLONG *pDuration)
{
  REFERENCE_TIME refTime;
  m_pFilter->GetDuration(&refTime);
  m_rtDuration = CRefTime(refTime);

  return CSourceSeeking::GetDuration(pDuration);
}

STDMETHODIMP CSubtitlePin::GetCurrentPosition(LONGLONG *pCurrent)
{
  //LogDebug("sub:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

void CSubtitlePin::SetRunningStatus(bool onOff)
{
	m_bRunning = onOff;
}

