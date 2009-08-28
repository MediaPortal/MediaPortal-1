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
  m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute  |
    AM_SEEKING_CanSeekForwards  |
    AM_SEEKING_CanSeekBackwards |
    AM_SEEKING_CanGetStopPos  |
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
    pRequest->cBuffers = 30;
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

  bool mpeg2Video=false;
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  if (demux.GetVideoServiceType()==SERVICE_TYPE_VIDEO_MPEG1 ||
    demux.GetVideoServiceType()==SERVICE_TYPE_VIDEO_MPEG2 || demux.GetVideoServiceType()==SERVICE_TYPE_DCII_VIDEO_MPEG2)
  {
    mpeg2Video=true;
  }
  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  hr=pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  if (pinInfo.pFilter==NULL) return E_FAIL;
  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  filterInfo.pGraph->Release();
  pinInfo.pFilter->Release();

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

  //LogDebug("vid:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}
HRESULT CVideoPin::CompleteConnect(IPin *pReceivePin)
{
  m_bInFillBuffer=false;
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
  //LogDebug("vid:BreakConnect() ok");
  m_bConnected=false;
  return CSourceStream::BreakConnect();
}

void CVideoPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity=onOff;
}

int ShowBuffer=100 ;

HRESULT CVideoPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
    CBuffer* buffer=NULL;
    DWORD m_LastTickCount = GetTickCount() ;

    do
    {
      //get file-duration and set m_rtDuration
      GetDuration(NULL);

      m_bInFillBuffer=true;

      //if the filter is currently seeking to a new position
      //or this pin is currently seeking to a new position then
      //we dont try to read any packets, but simply return...
      if (m_pTsReaderFilter->IsSeeking() || m_bSeeking || m_pTsReaderFilter->IsSeekingToEof())
      {
//        if (ShowBuffer) LogDebug("vid:isseeking:%d %d",m_pTsReaderFilter->IsSeeking() ,m_bSeeking);
        m_bInFillBuffer=false;
        Sleep(20);
        pSample->SetTime(NULL,NULL);
        pSample->SetActualDataLength(0);
        pSample->SetSyncPoint(FALSE);
        pSample->SetDiscontinuity(FALSE);  // TRUE seems to hold sometimes the working thread....( ambass )
        return NOERROR;
      }

      if (m_pTsReaderFilter->m_bStreamCompensated)
      {
        //get next buffer from demultiplexer
        {
          CAutoLock lock(&m_bufferLock);
          buffer=demux.GetVideo();
        }
      }

      //did we reach the end of the file
      if (demux.EndOfFile() || ((GetTickCount()-m_LastTickCount > 3000) && !m_pTsReaderFilter->IsTimeShifting()))
      {
        LogDebug("vid:set eof");
        pSample->SetTime(NULL,NULL);
        pSample->SetActualDataLength(0);
        pSample->SetSyncPoint(FALSE);
        pSample->SetDiscontinuity(TRUE);
        m_bInFillBuffer=false;
        return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }

      if (buffer==NULL)
      {
        m_bInFillBuffer=false;
        Sleep(10);
      }
      else
      {
        m_LastTickCount = GetTickCount() ;
        #define PRESENT_DELAY 500000
        CRefTime RefTime,cRefTime ;
        bool HasTimestamp ;
        //check if it has a timestamp
        if ((HasTimestamp=buffer->MediaTime(RefTime)))
        {
          CRefTime AddOffset=m_pTsReaderFilter->AddVideoComp ;
          #define DRIFT_RATE 0.5f     // Try duration of drift recovery 10 times the compensation. ( ie: 10sec for 1 sec compensation )
          cRefTime = RefTime ;
          cRefTime -= m_rtStart ;
          //adjust the timestamp with the compensation
          cRefTime -= m_pTsReaderFilter->Compensation ;
          cRefTime -= AddOffset ;
          cRefTime -= m_pTsReaderFilter->m_ClockOnStart.m_time ;

          CRefTime Dur ;
          Dur = (m_pTsReaderFilter->AddVideoComp.m_time * DRIFT_RATE) ;

          if (cRefTime.m_time < (m_pTsReaderFilter->AddVideoComp.m_time * DRIFT_RATE))
          {
            // Ambass : try to stretch video after zapping
            AddOffset = cRefTime.m_time / DRIFT_RATE ;
//            LogDebug("%03.3f, %03.3f, %03.3f", (float)AddOffset.Millisecs()/1000.0f,(float)cRefTime.Millisecs()/1000.0f, (float)m_pTsReaderFilter->AddVideoComp.Millisecs()/1000.0f);
//            m_pTsReaderFilter->AddVideoComp.m_time =0 ;
          }

          cRefTime += AddOffset ;
          cRefTime += m_pTsReaderFilter->m_ClockOnStart.m_time ;

          if (cRefTime.m_time >= 0) // + PRESENT_DELAY)
            Sleep(5) ;
          m_bPresentSample = true ;
//          else
            // Sample is too late.
//            m_ShowSample = false ;
       }

        if (m_bPresentSample)
        {
          //do we need to set the discontinuity flag?
          if (m_bDiscontinuity || buffer->GetDiscontinuity())
          {
            //ifso, set it
            LogDebug("vid:set discontinuity");
            pSample->SetDiscontinuity(TRUE);
            m_bDiscontinuity=FALSE;
          }

          if (HasTimestamp)
          {
            //now we have the final timestamp, set timestamp in sample
            REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
            pSample->SetSyncPoint(TRUE);
            pSample->SetTime(&refTime,&refTime);
            if (1)
            {
              double clock=0 ;
              if (m_pTsReaderFilter->State() == State_Running)
                clock = (double)(GetTickCount() - m_pTsReaderFilter->m_lastRun) / 1000.0 ;

              float fTime=(float)cRefTime.Millisecs();
              fTime/=1000.0f;
              double fTime3 = fTime - clock ;

              if (ShowBuffer || fTime3 < 0.030)
								{
				        int cntA,cntV ;
        				CRefTime firstAudio, lastAudio ;
        				CRefTime firstVideo, lastVideo ;
        				cntA = demux.GetAudioBufferPts(firstAudio, lastAudio) ; 
        				cntV = demux.GetVideoBufferPts(firstVideo, lastVideo) +1 ;

                LogDebug("Vid/Ref : %03.3f, Late %c-frame(%02d), Compensated = %03.3f ( %0.3f A/V buffers=%02d/%02d), Clk : %f, State %d", (float)RefTime.Millisecs()/1000.0f,buffer->GetFrameType(),buffer->GetFrameCount(), (float)cRefTime.Millisecs()/1000.0f, fTime3, cntA,cntV,clock, m_pTsReaderFilter->State());
								}
              if (ShowBuffer) ShowBuffer-- ;
            }
          }
          else
          {
            //buffer has no timestamp
            pSample->SetTime(NULL,NULL);
            pSample->SetSyncPoint(FALSE);
          }

          //copy buffer in sample
          BYTE* pSampleBuffer;
          pSample->SetActualDataLength(buffer->Length());
          pSample->GetPointer(&pSampleBuffer);
          memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
          //delete the buffer and return
          delete buffer;
        }
        else
        { // Buffer was not displayed because it was out of date, search for next.
          delete buffer;
          buffer=NULL ;
        }
      }
      m_bInFillBuffer=false;
    } while (buffer==NULL);
    return NOERROR;
  }

  catch(...)
  {
    LogDebug("vid:fillbuffer exception");
  }
  m_bInFillBuffer=false;
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
  m_bInFillBuffer=false;
  m_bPresentSample=false;

  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;

  //tell demuxer to start deliver video packets again
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  demux.SetHoldVideo(false);

  m_pTsReaderFilter->SetWaitForSeekToEof(false) ;

	while(demux.IsVideoChanging() && !m_pTsReaderFilter->m_bStopping) Sleep(5) ;

  LogDebug("vid:OnThreadStartPlay(%f) %02.2f %d", fStart,m_dRateSeeking,m_pTsReaderFilter->IsSeekingToEof());

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
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  CTsDuration tsduration=m_pTsReaderFilter->GetDuration();

  //if (m_rtStart>m_rtDuration)
  //  m_rtStart=m_rtDuration;

  //there is a bug in directshow causing UpdateFromSeek() to be called multiple times
  //directly after eachother
  //for a single seek operation. To 'fix' this we only perform the seeking operation
  //if we didnt do a seek in the last 5 seconds...
  if (GetTickCount()-m_seekTimer<5000)
  {
    if (m_lastSeek==m_rtStart)
    {
      LogDebug("vid:skip seek");
      return;
    }
  }
  if (demux.IsVideoChanging())
  {
    LogDebug("vid:skip seek-2");
    demux.FlushVideo() ;
    return;
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
  LogDebug("vid seek to %f", seekTime);

  seekTime*=1000.0f;
  rtSeek = CRefTime((LONG)seekTime);

  //if another output pin is seeking, then wait until its finished
  m_bSeeking=true;
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);

  //tell demuxer to stop deliver video data and wait until
  //FillBuffer() finished
  demux.SetHoldVideo(true);
  while (m_bInFillBuffer) Sleep(10);
  CAutoLock lock(&m_bufferLock);

  //if a pin-output thread exists...
  if (ThreadExists() || !m_pTsReaderFilter->IsStreaming())
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

    if (!m_pTsReaderFilter->GetAudioPin()-> IsConnected())
    {
      //tell filter we're done with seeking
      m_pTsReaderFilter->SeekDone(rtSeek);
    }

    //set our start time
    //m_rtStart=rtSeek;

    //clear flags indiciating that the pin is seeking
    LogDebug("vid seek done 1  --- %d",m_pTsReaderFilter->IsSeekingToEof());
    m_bSeeking=false;

    demux.FlushVideo() ;

    // and restart the thread
    Run();
  }
  else
  {
    //no thread running? then simply seek to the position
    m_pTsReaderFilter->Seek(rtSeek,false);

    demux.FlushVideo() ;

    //clear flags indiciating that the pin is seeking
    LogDebug("vid seek done 2  --- %d",m_pTsReaderFilter->IsSeekingToEof());
    m_bSeeking=false;
  }
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
