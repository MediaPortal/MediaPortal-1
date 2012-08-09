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
  m_section(section),
  m_bConnected(false),
  m_bRunning(false),
  m_bFlushing(false),
  m_bSeekDone(true)
{
  m_rtStart = 0;

  m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute  |
    AM_SEEKING_CanSeekForwards  |
    AM_SEEKING_CanSeekBackwards |
    AM_SEEKING_CanGetStopPos  |
    AM_SEEKING_CanGetDuration |
    //AM_SEEKING_CanGetCurrentPos |
    AM_SEEKING_Source;
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
    return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  if (riid == IID_IMediaPosition)
    return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );

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
    pRequest->cBuffers = 30;

  pRequest->cbBuffer = 8192;

  ALLOCATOR_PROPERTIES Actual;
  hr = pAlloc->SetProperties(pRequest, &Actual);
  if (FAILED(hr))
    return hr;

  if (Actual.cbBuffer < pRequest->cbBuffer)
    return E_FAIL;

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
  HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);

  if (SUCCEEDED(hr))
    m_bConnected = true;
  else
    LogDebug("pin:CompleteConnect() failed:%x", hr);

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

void CSubtitlePin::CreateEmptySample(IMediaSample *pSample)
{
  if (pSample)
  {
    pSample->SetTime(NULL, NULL);
    pSample->SetActualDataLength(0);
    pSample->SetSyncPoint(false);
  }
  else
    LogDebug("aud:CreateEmptySample() invalid sample!");
}

HRESULT CSubtitlePin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();
    Packet* buffer=NULL;
    do
    {
      if (m_pFilter->IsStopping() || !m_bRunning || !m_bSeekDone)
      {
        //LogDebug("sub:isseeking:%d %d",m_pFilter->IsSeeking() ,m_bSeeking);
        CreateEmptySample(pSample);
        Sleep(20);

        return S_OK;
      }

      buffer = demux.GetSubtitle();

      if (demux.EndOfFile())
      {
        CreateEmptySample(pSample);
        return S_FALSE; 
      }

      if (!buffer)
        Sleep(20);
      else
      {
        if (m_bDiscontinuity || buffer->bDiscontinuity)
        {
          LogDebug("sub: Set discontinuity");
          pSample->SetDiscontinuity(true);
          m_bDiscontinuity = false;
        }

        pSample->SetTime(NULL, NULL);
        pSample->SetSyncPoint(FALSE);
        BYTE* pSampleBuffer;
        pSample->SetActualDataLength(buffer->GetDataSize());
        pSample->GetPointer(&pSampleBuffer);
        memcpy(pSampleBuffer, buffer->GetData(), buffer->GetDataSize());

        delete buffer;
      }
    } while (!buffer);
    return S_OK;
  }
  catch(...)
  {
    LogDebug("sub: Fillbuffer exception");
  }

  return NOERROR;
}

HRESULT CSubtitlePin::ChangeStart()
{
  return S_OK;
}

HRESULT CSubtitlePin::ChangeStop()
{
  return S_OK;
}

HRESULT CSubtitlePin::ChangeRate()
{
  return S_OK;
}

HRESULT CSubtitlePin::OnThreadStartPlay()
{
  m_bDiscontinuity = true;

  LogDebug("sub: OnThreadStartPlay: %6.3f", m_rtStart / 10000000.0);

  return CSourceStream::OnThreadStartPlay();
}

HRESULT CSubtitlePin::DeliverBeginFlush()
{
  m_bFlushing = true;
  m_bSeekDone = false;
  HRESULT hr = __super::DeliverBeginFlush();
  LogDebug("sub: DeliverBeginFlush - hr: %08lX", hr);

  if (hr != S_OK)
  {
    m_bFlushing = true;
    m_bSeekDone = true;
  }

  return hr;
}

HRESULT CSubtitlePin::DeliverEndFlush()
{
  HRESULT hr = __super::DeliverEndFlush();
  LogDebug("sub: DeliverEndFlush - hr: %08lX", hr);
  m_bFlushing = false;

  return hr;
}

HRESULT CSubtitlePin::DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  if (m_bFlushing || !ThreadExists())
  {
    m_bSeekDone = true;
    return S_FALSE;
  }

  LogDebug("sub: DeliverNewSegment start: %6.3f stop: %6.3f rate: %6.3f", tStart / 10000000.0, tStop / 10000000.0, dRate);
  m_rtStart = tStart;

  HRESULT hr = __super::DeliverNewSegment(tStart, tStop, dRate);
  if (FAILED(hr))
    LogDebug("sub: DeliverNewSegment - error: %08lX", hr);

  m_bSeekDone = true;

  return hr;
}

STDMETHODIMP CSubtitlePin::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags)
{
  return m_pFilter->SetPositionsInternal(this, pCurrent, CurrentFlags, pStop, StopFlags);
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

