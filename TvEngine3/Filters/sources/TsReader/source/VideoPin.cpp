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
BYTE g_Mpeg2ProgramVideo[]= {
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.left
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.top
      0xd0, 0x02, 0x00, 0x00,							//  .hdr.rcSource.right
      0x40, 0x02, 0x00, 0x00,							//  .hdr.rcSource.bottom
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.left
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.top
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.right
      0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.bottom
      0xc0, 0xe1, 0xe4, 0x00,							//  .hdr.dwBitRate
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwBitErrorRate
      0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwInterlaceFlags
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwCopyProtectFlags
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioX
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioY
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved1
      0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved2
      0x28, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSize
      0xd0, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biWidth
      0x40, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biHeight
      0x00, 0x00,										//  .hdr.bmiHeader.biPlanes
      0x00, 0x00,										//  .hdr.bmiHeader.biBitCount
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biCompression
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSizeImage
      0xd0, 0x07, 0x00, 0x00,							//  .hdr.bmiHeader.biXPelsPerMeter
      0x42, 0xd8, 0x00, 0x00,							//  .hdr.bmiHeader.biYPelsPerMeter
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrUsed
      0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrImportant
      0x00, 0x00, 0x00, 0x00,							//  .dwStartTimeCode
      0x4c, 0x00, 0x00, 0x00,							//  .cbSequenceHeader
      0x00, 0x00, 0x00, 0x00,							//  .dwProfile
      0x00, 0x00, 0x00, 0x00,							//  .dwLevel
      0x00, 0x00, 0x00, 0x00,							//  .Flags
			                        //  .dwSequenceHeader [1]
      0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33, 
      0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
      0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
      0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
      0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
      0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
      0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
      0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b, 
      0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e, 
      0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
};

extern void LogDebug(const char *fmt, ...) ;

CVideoPin::CVideoPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinVideo"), phr, pFilter, L"Video"),
	m_pTsReaderFilter(pFilter),
	m_section(section),
  CSourceSeeking(NAME("pinVideo"),pUnk,phr,section)
{
	m_rtStart=0;
  m_bConnected=false;
	m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute	|
	AM_SEEKING_CanSeekForwards	|
	AM_SEEKING_CanSeekBackwards	|
	AM_SEEKING_CanGetStopPos	|
	AM_SEEKING_CanGetDuration |
  AM_SEEKING_CanGetCurrentPos |
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

	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Video);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_VIDEO);
	pmt->SetFormatType(&FORMAT_MPEG2Video);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();
	pmt->SetFormat(g_Mpeg2ProgramVideo,sizeof(g_Mpeg2ProgramVideo));

	return S_OK;
}

HRESULT CVideoPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;


	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
			pRequest->cBuffers = 2;
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

HRESULT CVideoPin::CheckConnect(IPin *pReceivePin)
{
  HRESULT hr;
#ifndef DEBUG
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  hr=pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (pinInfo.pFilter==NULL) return E_FAIL;
  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (wcscmp(filterInfo.achName,L"MPV Decoder Filter")==0)
  {
    return E_FAIL;
  }
  if (wcscmp(filterInfo.achName,L"DScaler Mpeg2 Video Decoder")==0)
  {
    return E_FAIL;
  }
#endif
  return CBaseOutputPin::CheckConnect(pReceivePin);
}
HRESULT CVideoPin::CompleteConnect(IPin *pReceivePin)
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
    m_rtDuration=CRefTime(MAX_TIME);
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
	return hr;
}


HRESULT CVideoPin::BreakConnect()
{
  m_bConnected=false;
  return CSourceStream::BreakConnect();
}

HRESULT CVideoPin::FillBuffer(IMediaSample *pSample)
{
//	::OutputDebugStringA("CVideoPin::FillBuffer()\n");
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    m_rtDuration=CRefTime(MAX_TIME);
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
    pSample->SetTime(NULL,NULL); 
	  pSample->SetActualDataLength(0);
		return NOERROR;
	}
  CAutoLock lock(&m_bufferLock);
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  CBuffer* buffer=demux.GetVideo();
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
      REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
      pSample->SetTime(&refTime,NULL); 
      pSample->SetSyncPoint(TRUE);
      float fTime=(float)cRefTime.Millisecs();
      fTime/=1000.0f;
      //LogDebug("vid:%f", fTime);
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
    //LogDebug("vid:no buffer");
    pSample->SetDiscontinuity(TRUE);
	  pSample->SetActualDataLength(0);
    pSample->SetTime(NULL,NULL);  
  }

  return NOERROR;
}



HRESULT CVideoPin::OnThreadStartPlay()
{    
  m_bDiscontinuity=TRUE;
  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;
  LogDebug("vid:OnThreadStartPlay(%f)", fStart);
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

  CRefTime rtSeek=m_rtStart;
  float seekTime=(float)rtSeek.Millisecs();
  seekTime/=1000.0f;
  LogDebug("vid seek to %f", seekTime);
  m_bSeeking=true;
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);
  CAutoLock lock(&m_bufferLock);
  if (ThreadExists()) 
  {
      // next time around the loop, the worker thread will
      // pick up the position change.
      // We need to flush all the existing data - we must do that here
      // as our thread will probably be blocked in GetBuffer otherwise
      
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
        m_pTsReaderFilter->SeekStart();
      }
      HRESULT hr=DeliverBeginFlush();
     // LogDebug("vid:beginflush:%x",hr);
      // make sure we have stopped pushing
      Stop();
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
        m_pTsReaderFilter->Seek(CRefTime(m_rtStart));
      }
      // complete the flush
      hr=DeliverEndFlush();
     // LogDebug("vid:endflush:%x",hr);
      
      if (!m_pTsReaderFilter->GetAudioPin()->IsConnected())
      {
        m_pTsReaderFilter->SeekDone(rtSeek);
      }
      // restart
      
      m_rtStart=rtSeek;
      Run();
  }
  m_bSeeking=false;
}

STDMETHODIMP CVideoPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
  LogDebug("vid:GetAvailable");
  return CSourceSeeking::GetAvailable( pEarliest, pLatest );
}

STDMETHODIMP CVideoPin::GetDuration(LONGLONG *pDuration)
{
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    m_rtDuration=CRefTime(MAX_TIME);
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
  return CSourceSeeking::GetCurrentPosition(pCurrent);
}
