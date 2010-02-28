/*
 *  Copyright (C) 2005 Team MediaPortal
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

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include <sbe.h>
#include "tsreader.h"
#include "AudioPin.h"
#include "Videopin.h"
#include "mediaformats.h"
#include <wmcodecdsp.h>

#define MAX_TIME  86400000L
#define DRIFT_RATE 0.5f

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
    AM_SEEKING_CanSeekAbsolute  |
    AM_SEEKING_CanSeekForwards  |
    AM_SEEKING_CanSeekBackwards |
    AM_SEEKING_CanGetStopPos  |
    AM_SEEKING_CanGetDuration |
    //AM_SEEKING_CanGetCurrentPos |
    AM_SEEKING_Source;
//  m_bSeeking=false;
}

CVideoPin::~CVideoPin()
{
}

bool CVideoPin::IsConnected()
{
  return m_bConnected;
}
STDMETHODIMP CVideoPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
  if (riid == IID_IStreamBufferConfigure)
  {
    //LogDebug("vid:IID_IStreamBufferConfigure()");
  }
  if (riid == IID_IStreamBufferInitialize)
  {
    //LogDebug("vid:IID_IStreamBufferInitialize()");
  }
  if (riid == IID_IStreamBufferMediaSeeking||riid == IID_IStreamBufferMediaSeeking2)
  {
    //LogDebug("vid:IID_IStreamBufferMediaSeeking()");
  }
  if (riid == IID_IStreamBufferSource)
  {
    //LogDebug("vid:IID_IStreamBufferSource()");
  }
  if (riid == IID_IStreamBufferDataCounters)
  {
    //LogDebug("vid:IID_IStreamBufferDataCounters()");
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
  //LogDebug("vid:GetMediaType()");
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
    pRequest->cBuffers = 1;
  }

  // Would be better if this would be allocated on sample basis
  pRequest->cbBuffer = 0x1000000;

  ALLOCATOR_PROPERTIES Actual;
  hr = pAlloc->SetProperties(pRequest, &Actual);
  if (FAILED(hr))
  {
    return hr;
  }

  if (Actual.cbBuffer < pRequest->cbBuffer)
  {
    LogDebug("vid:DecideBufferSize - failed to get buffer");
    return E_FAIL;
  }

  return S_OK;
}

HRESULT CVideoPin::CheckConnect(IPin *pReceivePin)
{
  //LogDebug("vid:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}
HRESULT CVideoPin::CompleteConnect(IPin *pReceivePin)
{
  //LogDebug("vid:CompleteConnect()");
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
  //LogDebug("vid:CompleteConnect() ok");
  return hr;
}

HRESULT CVideoPin::BreakConnect()
{
  //LogDebug("vid:BreakConnect() ok");
  m_bConnected=false;
  return CSourceStream::BreakConnect();
}

void CVideoPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity=onOff;
}

HRESULT CVideoPin::DoBufferProcessingLoop(void)
{
  Command com;
  OnThreadStartPlay();

  do 
  {
    while (!CheckRequest(&com)) 
    {
      IMediaSample *pSample;
      HRESULT hr = GetDeliveryBuffer(&pSample,NULL,NULL,0);
      if (FAILED(hr)) 
      {
        Sleep(1);
        continue;	// go round again. Perhaps the error will go away
        // or the allocator is decommited & we will be asked to
        // exit soon.
      }

      // Virtual function user will override.
      hr = FillBuffer(pSample);

      if (hr == S_OK) 
      {
        //LogDebug("Vid::DoBufferProcessingLoop() - sample len %d size %d", 
        //  pSample->GetActualDataLength(), pSample->GetSize());
        
        // This is the only change for base class implementation of DoBufferProcessingLoop()
        // Cyberlink H.264 decoder seems to crash when we provide empty samples for it 
        if( pSample->GetActualDataLength() > 0)
        {
          hr = Deliver(pSample);     
        }
		
        pSample->Release();

        // downstream filter returns S_FALSE if it wants us to
        // stop or an error if it's reporting an error.
        if(hr != S_OK)
        {
          DbgLog((LOG_TRACE, 2, TEXT("Deliver() returned %08x; stopping"), hr));
          return S_OK;
        }
      } 
      else if (hr == S_FALSE) 
      {
        // derived class wants us to stop pushing data
        pSample->Release();
        DeliverEndOfStream();
        return S_OK;
      } 
      else 
      {
        // derived class encountered an error
        pSample->Release();
        DbgLog((LOG_ERROR, 1, TEXT("Error %08lX from FillBuffer!!!"), hr));
        DeliverEndOfStream();
        m_pFilter->NotifyEvent(EC_ERRORABORT, hr, 0);
        return hr;
      }
     // all paths release the sample
    }
    // For all commands sent to us there must be a Reply call!
	  if (com == CMD_RUN || com == CMD_PAUSE) 
    {
      Reply(NOERROR);
	  } 
    else if (com != CMD_STOP) 
    {
      Reply((DWORD) E_UNEXPECTED);
      DbgLog((LOG_ERROR, 1, TEXT("Unexpected command!!!")));
	  }
  } while (com != CMD_STOP);

  return S_FALSE;
}

int ShowBuffer=100;

HRESULT CVideoPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux = m_pTsReaderFilter->GetDemultiplexer();
    CBuffer* buffer = NULL;

    do
    {
      //get file-duration and set m_rtDuration
      GetDuration(NULL);

      //if the filter is currently seeking to a new position
      //or this pin is currently seeking to a new position then
      //we dont try to read any packets, but simply return...
      if (m_pTsReaderFilter->IsSeeking() || m_pTsReaderFilter->IsStopping()) //|| m_bSeeking*/ || m_pTsReaderFilter->IsSeekingToEof())
      {
        //if (ShowBuffer) LogDebug("vid:isseeking:%d %d",m_pTsReaderFilter->IsSeeking() ,m_bSeeking);
        Sleep(5);
        pSample->SetActualDataLength(0);
        return NOERROR;
      }

      if (m_pTsReaderFilter->m_bStreamCompensated)
      {
        // Get next video buffer from demultiplexer
        buffer=demux.GetVideo();
      }

      //did we reach the end of the file
      if (demux.EndOfFile())
      {
        LogDebug("vid:set eof");
        pSample->SetTime(NULL,NULL);
        pSample->SetActualDataLength(0);
        pSample->SetSyncPoint(FALSE);
        pSample->SetDiscontinuity(TRUE);
        return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }

      if (buffer == NULL)
      {
        Sleep(10);
      }
      else
      {
        CRefTime RefTime, cRefTime;
        bool HasTimestamp;
        //check if it has a timestamp
        if ((HasTimestamp=buffer->MediaTime(RefTime)))
        {
          CRefTime AddOffset=m_pTsReaderFilter->AddVideoComp;
          // Try duration of drift recovery 10 times the compensation. ( ie: 10sec for 1 sec compensation )
          cRefTime = RefTime;
          cRefTime -= m_rtStart;
          //adjust the timestamp with the compensation
          cRefTime -= m_pTsReaderFilter->Compensation;
          cRefTime -= AddOffset;
          cRefTime -= m_pTsReaderFilter->m_ClockOnStart.m_time;

          CRefTime Dur;
          Dur = (m_pTsReaderFilter->AddVideoComp.m_time * DRIFT_RATE);

          if (cRefTime.m_time < (m_pTsReaderFilter->AddVideoComp.m_time * DRIFT_RATE))
          {
            // Ambass : try to stretch video after zapping
            AddOffset = cRefTime.m_time / DRIFT_RATE;
            // LogDebug("%03.3f, %03.3f, %03.3f", (float)AddOffset.Millisecs()/1000.0f,(float)cRefTime.Millisecs()/1000.0f, (float)m_pTsReaderFilter->AddVideoComp.Millisecs()/1000.0f);
            // m_pTsReaderFilter->AddVideoComp.m_time =0;
          }

          cRefTime += AddOffset;
          cRefTime += m_pTsReaderFilter->m_ClockOnStart.m_time;
                                                                      
          if (cRefTime.m_time >= 0)
            Sleep(1); // Ambass : avoid blocking audio FillBuffer method ( on audio/video starting ) by excessive video Fill buffer preemption
          m_bPresentSample = true;
          //else
          // Sample is too late.
          //m_ShowSample = false ;
       }

        if (m_bPresentSample)
        {
          //do we need to set the discontinuity flag?
          if (m_bDiscontinuity || buffer->GetDiscontinuity())
          {
            LogDebug("vid:set discontinuity");
            pSample->SetDiscontinuity(TRUE);
            m_bDiscontinuity=FALSE;
          }

          //LogDebug("vid: video buffer type = %d", buffer->GetVideoServiceType());

          if (HasTimestamp)
          {
            //now we have the final timestamp, set timestamp in sample
            REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
            pSample->SetSyncPoint(TRUE);
            pSample->SetTime(&refTime,&refTime);
            if (1)
            {
              REFERENCE_TIME RefClock = 0;
              m_pTsReaderFilter->GetMediaPosition(&RefClock) ;
              float clock = (double)(RefClock-m_rtStart.m_time)/10000000.0 ;
              float fTime=(float)cRefTime.Millisecs()/1000.0f - clock ;

              if (ShowBuffer || fTime < 0.030)
              {
                int cntA, cntV;
                CRefTime firstAudio, lastAudio;
                CRefTime firstVideo, lastVideo;
                cntA = demux.GetAudioBufferPts(firstAudio, lastAudio); 
                cntV = demux.GetVideoBufferPts(firstVideo, lastVideo) + 1;

                LogDebug("Vid/Ref : %03.3f, Late %c-frame(%02d), Compensated = %03.3f ( %0.3f A/V buffers=%02d/%02d), Clk : %f, State %d", (float)RefTime.Millisecs()/1000.0f,buffer->GetFrameType(),buffer->GetFrameCount(), (float)cRefTime.Millisecs()/1000.0f, fTime, cntA,cntV,clock, m_pTsReaderFilter->State());
								}
              if (ShowBuffer) ShowBuffer--;
            }
          }
          else
          {
            //buffer has no timestamp
            pSample->SetTime(NULL,NULL);
            pSample->SetSyncPoint(FALSE);
          }
          
          // copy buffer into the sample
          BYTE* pSampleBuffer;
          pSample->SetActualDataLength(buffer->Length());
          pSample->GetPointer(&pSampleBuffer);
          memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
          
          // delete the buffer
          delete buffer;
        }
        else
        { // Buffer was not displayed because it was out of date, search for next.
          delete buffer;
          buffer = NULL;
        }
      }
    } while (buffer == NULL);
    return NOERROR;
  }

  catch(...)
  {
    LogDebug("vid:fillbuffer exception");
  }
  return NOERROR;
}

//******************************************************
/// Called when thread is about to start delivering data to the codec
///
HRESULT CVideoPin::OnThreadStartPlay()
{
  //set discontinuity flag indicating to codec that the new data
  //is not belonging to any previous data
  m_bDiscontinuity=TRUE;
  m_bPresentSample=false;

  LogDebug("vid:OnThreadStartPlay(%f) %02.2f %d", (float)m_rtStart.Millisecs()/1000.0f,m_dRateSeeking,m_pTsReaderFilter->IsSeeking());

  //start playing
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
  if( m_dRateSeeking <= 0 )
  {
    m_dRateSeeking = 1.0;  // Reset to a reasonable value.
    return E_FAIL;
  }
  UpdateFromSeek();
  return S_OK;
}

void CVideoPin::SetStart(CRefTime rtStartTime)
{
  m_rtStart = rtStartTime ;
}

STDMETHODIMP CVideoPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
///
void CVideoPin::UpdateFromSeek()
{
  m_pTsReaderFilter->SeekPreStart(m_rtStart) ;
//  LogDebug("vid: seek done %f/%f",(float)m_rtStart.Millisecs()/1000.0f,(float)m_rtDuration.Millisecs()/1000.0f);
  return ;
}

//******************************************************
/// GetAvailable() returns
/// pEarliest -> the earliest (pcr) timestamp in the file
/// pLatest   -> the latest (pcr) timestamp in the file
///
STDMETHODIMP CVideoPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
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
STDMETHODIMP CVideoPin::GetDuration(LONGLONG *pDuration)
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
STDMETHODIMP CVideoPin::GetCurrentPosition(LONGLONG *pCurrent)
{
  //LogDebug("vid:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CVideoPin::Notify(IBaseFilter * pSender, Quality q)
{
  return E_NOTIMPL;
}
