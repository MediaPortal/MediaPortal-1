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

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define MAX_TIME  86400000L
#define DRIFT_RATE 0.5f

extern void LogDebug(const char *fmt, ...) ;
extern DWORD m_tGTStartTime;

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
  m_bInFillBuffer = false;
  m_bPinNoAddPMT = false;
  m_bAddPMT = false;
  m_bPinNoNewSegFlush = false;
  m_bDownstreamFlush=false;
}

CVideoPin::~CVideoPin()
{
  LogDebug("vidPin:dtor()");
}

bool CVideoPin::IsInFillBuffer()
{
  return (m_bInFillBuffer && m_bConnected);
}

bool CVideoPin::HasDeliveredSample()
{
  return ((m_sampleCount > 0) || !m_bConnected);
}

bool CVideoPin::IsConnected()
{
  return m_bConnected;
}

STDMETHODIMP CVideoPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
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

HRESULT CVideoPin::CheckMediaType(const CMediaType* pmt)
{
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  
  if (!demux.PatParsed())
  {
    return E_FAIL;
  }
  if (!demux.VidPidGood())
  {
    return E_FAIL;
  }

  CMediaType pmti;  
  demux.GetVideoStreamType(pmti);
  CMediaType* ppmti = &pmti;

  if(*pmt == *ppmti)
  {
    //LogDebug("vidPin:CheckMediaType() ok");  
    return S_OK;
  }

  //LogDebug("vidPin:CheckMediaType() fail");  
  return E_FAIL;
}

HRESULT CVideoPin::GetMediaType(int iPosition, CMediaType *pmt)
{
  CheckPointer(pmt, E_POINTER);

  //LogDebug("vidPin:GetMediaType() index = %d", iPosition);
  
  // This should never happen          
  if (iPosition < 0) 
  {              
    return E_INVALIDARG;
  }                                        
  if (iPosition > 0)   
  {           
    return VFW_S_NO_MORE_ITEMS;
  }   

  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  
  for (int i=0; i < 200; i++) //Wait up to 1 sec for pmt to be valid
  {
    if (demux.PatParsed())
    {
      if (!demux.VidPidGood())
      {
        //No video stream
        //LogDebug("vidPin:GetMediaType() - no pid");
        return VFW_S_NO_MORE_ITEMS;
      }
      else if (demux.GetVideoStreamType(*pmt))
      {
        //LogDebug("vidPin:GetMediaType() - good pid");
        return S_OK;
      }
    }
    Sleep(5);
  }

  //Return a null media type
  pmt->InitMediaType();
  return S_OK;
}

HRESULT CVideoPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
  HRESULT hr;
  CheckPointer(pAlloc, E_POINTER);
  CheckPointer(pRequest, E_POINTER);

  pRequest->cBuffers = max(2, pRequest->cBuffers);
  pRequest->cbBuffer = max(8388608, (ULONG)pRequest->cbBuffer);

  ALLOCATOR_PROPERTIES Actual;
  hr = pAlloc->SetProperties(pRequest, &Actual);
  if (FAILED(hr))
  {
    return hr;
  }

  if (Actual.cbBuffer < pRequest->cbBuffer)
  {
    LogDebug("vidPin:DecideBufferSize - failed to get buffer");
    return E_FAIL;
  }

  return S_OK;
}

HRESULT CVideoPin::CheckConnect(IPin *pReceivePin)
{
  //LogDebug("vidPin:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}


HRESULT CVideoPin::CompleteConnect(IPin *pReceivePin)
{
  m_bInFillBuffer = false;
  m_bPinNoAddPMT = false;
  m_bPinNoNewSegFlush = false;
  m_bAddPMT = true;
  HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
  if (!SUCCEEDED(hr)) return E_FAIL;

  PIN_INFO pinInfo;
  FILTER_INFO filterInfo;
  hr=pReceivePin->QueryPinInfo(&pinInfo);
  if (!SUCCEEDED(hr)) return E_FAIL;
  else if (pinInfo.pFilter==NULL) return E_FAIL;
  else pinInfo.pFilter->Release(); // we dont need the filter just the info
    
  hr=pinInfo.pFilter->QueryFilterInfo(&filterInfo);
  filterInfo.pGraph->Release();

  if (SUCCEEDED(hr)) 
  {
    char szName[MAX_FILTER_NAME];
    int cch = WideCharToMultiByte(CP_ACP, 0, filterInfo.achName, MAX_FILTER_NAME, szName, MAX_FILTER_NAME, 0, 0);
    LogDebug("vidPin:CompleteConnect() ok, filter: %s", szName);
    
    m_bConnected=true;
    
    m_pTsReaderFilter->m_bFastSyncFFDShow = false;
    
    CLSID &ref=m_pTsReaderFilter->GetCLSIDFromPin(pReceivePin);
    m_pTsReaderFilter->m_videoDecoderCLSID = ref;
    if (m_pTsReaderFilter->m_videoDecoderCLSID == CLSID_FFDSHOWVIDEO)
    {
      m_pTsReaderFilter->m_bFastSyncFFDShow=true;
      //LogDebug("vidPin:CompleteConnect() FFDShow Video Decoder connected");
    }
    else if (m_pTsReaderFilter->m_videoDecoderCLSID == CLSID_FFDSHOWDXVA)
    {
      m_bPinNoAddPMT = true;
      //LogDebug("vidPin:CompleteConnect() FFDShow DXVA Video Decoder connected, disable AddPMT");
    }
    else if (m_pTsReaderFilter->m_videoDecoderCLSID == CLSID_MSDTVDVDVIDEO)
    {
      m_bPinNoNewSegFlush = true;
      //LogDebug("vidPin:CompleteConnect() MS DTV-DVD Video Decoder connected, disable NewSegFlush");
    }
  }
  else
  {
    LogDebug("vidPin:CompleteConnect() failed:%x",hr);
    return E_FAIL;
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
  //LogDebug("vidPin:CompleteConnect() end");
  return hr;
}

HRESULT CVideoPin::BreakConnect()
{  
  m_bConnected=false;
  return CSourceStream::BreakConnect();
}

void CVideoPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity=onOff;
}

void CVideoPin::SetAddPMT()
{
  LogDebug("vidPin:SetAddPMT()");
  m_bAddPMT = true;
  m_sampleCount = 0;
}

void CVideoPin::CreateEmptySample(IMediaSample *pSample)
{
  if (pSample)
  {
    pSample->SetTime(NULL, NULL);
    pSample->SetActualDataLength(0);
    pSample->SetSyncPoint(false);
    pSample->SetDiscontinuity(false);
  }
  else
    LogDebug("vidPin: CreateEmptySample() invalid sample!");
}


HRESULT CVideoPin::DoBufferProcessingLoop(void)
{
  Command com;
  OnThreadStartPlay();
  SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_ABOVE_NORMAL);

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
        // Some decoders seem to crash when we provide empty samples 
        if ((pSample->GetActualDataLength() > 0) && !m_pTsReaderFilter->IsStopping())
        {
          hr = Deliver(pSample);     
          m_sampleCount++ ;
        }
        else
        {
          m_bDiscontinuity = true;
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


HRESULT CVideoPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux = m_pTsReaderFilter->GetDemultiplexer();
    CBuffer* buffer = NULL;    
    bool earlyStall = false;

    //get file-duration and set m_rtDuration
    GetDuration(NULL);

    do
    {
      //Check if we need to wait for a while
      DWORD timeNow = GET_TIME_NOW();
      while (timeNow < (m_LastFillBuffTime + m_FillBuffSleepTime))
      {      
        Sleep(1);
        timeNow = GET_TIME_NOW();
      }
      m_LastFillBuffTime = timeNow;

      //did we reach the end of the file
      if (demux.EndOfFile())
      {
        LogDebug("vidPin:set eof");
        m_FillBuffSleepTime = 5;
        CreateEmptySample(pSample);
        m_bInFillBuffer = false;
        return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }

      //if the filter is currently seeking to a new position
      //or this pin is currently seeking to a new position then
      //we dont try to read any packets, but simply return...
      if (m_pTsReaderFilter->IsSeeking() || m_pTsReaderFilter->IsStopping() || demux.m_bFlushRunning || !m_pTsReaderFilter->m_bStreamCompensated)
      {
        m_FillBuffSleepTime = 5;
        CreateEmptySample(pSample);
        m_bInFillBuffer = false;
        if (demux.m_bFlushRunning || !m_pTsReaderFilter->m_bStreamCompensated)
        {
          //m_bDownstreamFlush=true;
          //Force discon on next good sample
          m_sampleCount = 0;
          m_bDiscontinuity=true;
        }
        return NOERROR;
      }
      else
      {
        m_FillBuffSleepTime = 1;
        m_bInFillBuffer = true;
      }     
            
      if(m_bDownstreamFlush)
      {
        if( !m_bPinNoNewSegFlush ) //MS DTV video decoder can hang if we flush at the wrong time...
        {
          //Downstream flush
          //LogDebug("vidPin : Downstream flush") ;
          DeliverBeginFlush();
          DeliverEndFlush();
        }
        m_bDownstreamFlush=false;
      }
      
      // Get next video buffer from demultiplexer
      buffer=demux.GetVideo(earlyStall);

      if (buffer == NULL)
      {
        m_FillBuffSleepTime = 5;
      }
      else
      {
        m_bPresentSample = true ;
        
        CRefTime RefTime, cRefTime;
        double fTime = 0.0;
        double clock = 0.0;
        double stallPoint = 2.5;
        //check if it has a timestamp
        bool HasTimestamp=buffer->MediaTime(RefTime);
        if (HasTimestamp)
        {
          bool ForcePresent = false;
          CRefTime compTemp = m_pTsReaderFilter->GetCompensation();
          if (m_pTsReaderFilter->m_bFastSyncFFDShow && (compTemp != m_llLastComp))
          {
            m_bDiscontinuity = true;
          }
          m_llLastComp = compTemp;
          cRefTime = RefTime;
          cRefTime -= m_rtStart;
          //adjust the timestamp with the compensation
          cRefTime -= compTemp;
          cRefTime -= m_pTsReaderFilter->m_ClockOnStart.m_time;
          
          // 'fast start' timestamp modification, during first (AddVideoComp + 1 sec) of play
          double fsAdjLimit = (double)m_pTsReaderFilter->AddVideoComp.m_time + FS_ADDON_LIM; //vid comp + 1 second
          if (m_pTsReaderFilter->m_EnableSlowMotionOnZapping && ((double)cRefTime.m_time < fsAdjLimit) )
          {
            //float startCref = (float)cRefTime.m_time/(1000*10000); //used in LogDebug below only
            //Assume desired timestamp span is zero to fsAdjLimit, actual span is AddVideoComp to fsAdjLimit
            double offsetRatio = fsAdjLimit/FS_ADDON_LIM; // == fsAdjLimit/(fsAdjLimit - (double)m_pTsReaderFilter->AddVideoComp.m_time);
            double currOffset = fsAdjLimit - (double)cRefTime.m_time;
            double newOffset = currOffset * offsetRatio;
            cRefTime = (fsAdjLimit > newOffset) ? (REFERENCE_TIME)(fsAdjLimit - newOffset) : 0;  //Don't allow negative cRefTime
            ForcePresent = true;
            //LogDebug("VFS cOfs %03.3f, nOfs %03.3f, cRefTimeS %03.3f, cRefTimeN %03.3f", (float)currOffset/(1000*10000), (float)newOffset/(1000*10000), startCref, (float)cRefTime.m_time/(1000*10000));         
            if (m_pTsReaderFilter->m_bFastSyncFFDShow)
            {
              m_delayedDiscont = 2; //Force I-frame timestamp updates for FFDShow
            }
          }          

          REFERENCE_TIME RefClock = 0;
          m_pTsReaderFilter->GetMediaPosition(&RefClock) ;
          clock = (double)(RefClock-m_rtStart.m_time)/10000000.0 ;
          fTime = ((double)(cRefTime.m_time + m_pTsReaderFilter->m_ClockOnStart.m_time)/10000000.0) - clock ;
                                                                      
          if (m_dRateSeeking == 1.0)
          {
            if ((fTime < 0.3) && (m_pTsReaderFilter->State() == State_Running) && (m_sampleCount > 10))
            {              
              if (!demux.m_bVideoSampleLate) 
              {
                LogDebug("vidPin : Video to render late= %03.3f", (float)fTime) ;
              }
              //Samples times are getting close to presentation time
              demux.m_bVideoSampleLate = true;   
              
              if (fTime < 0.02)
              {              
                //Samples are running very late - check if this is a persistant problem by counting over a period of time 
                //(m_AVDataLowCount is checked in CTsReaderFilter::ThreadProc())
                _InterlockedExchangeAdd(&demux.m_AVDataLowCount, 1);   
              }
            }

            //Discard late samples at start of play,
            //and samples outside a sensible timing window during play 
            //(helps with signal corruption recovery)
            if ((fTime > (ForcePresent ? -0.5 : -0.3)) && (fTime < (stallPoint + 1.0)))
            {
              //if ((fTime > stallPoint) && (m_pTsReaderFilter->State() == State_Running))
              if ((fTime > stallPoint) && (m_sampleCount > 10))
              {
                //Too early - stall for a while to avoid over-filling of video pipeline buffers,
                //but don't enable at start of play to make sure graph starts properly
                m_FillBuffSleepTime = 10;
                buffer = NULL;
                earlyStall = true;
                continue;
              }
            }
            else
            {              
              // Sample is too late.
              m_bPresentSample = false ;
            }
          }
          else if ((fTime < -1.0) || (fTime > 3.0)) //Fast-forward limits
          {
            // Sample is too late.
            m_bPresentSample = false ;
          }
          cRefTime += m_pTsReaderFilter->m_ClockOnStart.m_time;
        }

        if (m_bPresentSample && (buffer->Length() > 0))
        {
          
          //do we need to set the discontinuity flag?
          if (m_bDiscontinuity || buffer->GetDiscontinuity())
          {
            if ((m_sampleCount == 0) && m_bAddPMT && !m_pTsReaderFilter->m_bDisableAddPMT && !m_bPinNoAddPMT)
            {
              //Add MediaType info to first sample after OnThreadStartPlay()
              CMediaType mt; 
              if (demux.GetVideoStreamType(mt))
              {
                pSample->SetMediaType(&mt); 
                LogDebug("vidPin: Add pmt and set discontinuity L:%d B:%d fTime:%03.3f SampCnt:%d", m_bDiscontinuity, buffer->GetDiscontinuity(), (float)fTime, m_sampleCount);
              }
              else
              {
                LogDebug("vidPin: Add pmt failed - set discontinuity L:%d B:%d fTime:%03.3f SampCnt:%d", m_bDiscontinuity, buffer->GetDiscontinuity(), (float)fTime, m_sampleCount);
              }
              m_bAddPMT = false; //Only add once each time
            }   
            else
            {        
              LogDebug("vidPin: Set discontinuity L:%d B:%d fTime:%03.3f SampCnt:%d", m_bDiscontinuity, buffer->GetDiscontinuity(), (float)fTime, m_sampleCount);
            }

            pSample->SetDiscontinuity(TRUE);           
            m_bDiscontinuity=FALSE;
          }

          //LogDebug("vidPin: video buffer type = %d", buffer->GetVideoServiceType());

          if (HasTimestamp)
          {
            //now we have the final timestamp, set timestamp in sample
            REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
            pSample->SetSyncPoint(TRUE);
            
            bool stsDiscon = TimestampDisconChecker(refTime); //Update with current timestamp

            refTime = (REFERENCE_TIME)((double)refTime/m_dRateSeeking);
            pSample->SetTime(&refTime,&refTime);
            if (m_pTsReaderFilter->m_bFastSyncFFDShow && (m_dRateSeeking == 1.0))
            {
              if (stsDiscon || (pSample->IsDiscontinuity()==S_OK))
              {
                pSample->SetDiscontinuity(TRUE);
                m_delayedDiscont = 2;
              }

              if ((m_delayedDiscont > 0) && (buffer->GetFrameType() == 'I'))
              {
                if ((buffer->GetVideoServiceType() == SERVICE_TYPE_VIDEO_MPEG1 ||
                     buffer->GetVideoServiceType() == SERVICE_TYPE_VIDEO_MPEG2))
                {
                   //Use delayed discontinuity
                   pSample->SetDiscontinuity(TRUE);
                   m_delayedDiscont--;
                   LogDebug("vidPin:set I-frame discontinuity, count %d", m_delayedDiscont);
                }
                else
                {
                   m_delayedDiscont = 0;
                }      
              }                             
            }

            if (m_pTsReaderFilter->m_ShowBufferVideo || ((fTime < 0.02) && (m_dRateSeeking == 1.0)) || (m_sampleCount < 3))
            {
              int cntA, cntV;
              CRefTime firstAudio, lastAudio;
              CRefTime firstVideo, lastVideo;
              cntA = demux.GetAudioBufferPts(firstAudio, lastAudio); 
              cntV = demux.GetVideoBufferPts(firstVideo, lastVideo);

              LogDebug("Vid/Ref : %03.3f, %c-frame(%02d), Compensated = %03.3f ( %0.3f A/V buffers=%02d/%02d), Clk : %f, SampCnt %d, stallPt %03.3f", (float)RefTime.Millisecs()/1000.0f,buffer->GetFrameType(),buffer->GetFrameCount(), (float)cRefTime.Millisecs()/1000.0f, fTime, cntA,cntV,clock, m_sampleCount, (float)stallPoint);              
            }
            
            if (m_pTsReaderFilter->m_ShowBufferVideo) m_pTsReaderFilter->m_ShowBufferVideo--;
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
          demux.EraseVideoBuff();
          //m_sampleCount++ ;         
        }
        else
        { // Buffer was not displayed because it was out of date, search for next.
          delete buffer;
          demux.EraseVideoBuff();
          m_bDiscontinuity = TRUE; //Next good sample will be discontinuous
          buffer = NULL;
          m_FillBuffSleepTime = 1;
        }
      }      
      earlyStall = false;
    } while (buffer == NULL);

    m_bInFillBuffer = false;
    return NOERROR;
  }

  catch(...)
  {
    LogDebug("vidPin:fillbuffer exception");
  }
  m_FillBuffSleepTime = 5;
  CreateEmptySample(pSample);
  m_bDiscontinuity = TRUE; //Next good sample will be discontinuous
  m_bInFillBuffer = false;  
  return NOERROR;
}

// Check for timestamp discontinuities
bool CVideoPin::TimestampDisconChecker(REFERENCE_TIME timeStamp)
{
  bool mtdDiscontinuity = false;
  REFERENCE_TIME stsDiff = timeStamp - m_llLastMTDts;
  m_llLastMTDts = timeStamp;
        
    // Calculate the mean timestamp difference
  if (m_nNextMTD >= NB_MTDSIZE)
  {
    m_fMTDMean = m_llMTDSumAvg / (REFERENCE_TIME)NB_MTDSIZE;
  }
  else if (m_nNextMTD > 0)
  {
    m_fMTDMean = m_llMTDSumAvg / (REFERENCE_TIME)m_nNextMTD;
  }
  else
  {
    m_fMTDMean = stsDiff;
  }

    // Check for discontinuity
  if (stsDiff > (m_fMTDMean + (800 * 10000))) // diff - mean > 800ms
  {
    mtdDiscontinuity = true;
    //LogDebug("vidPin:Timestamp discontinuity, TsDiff %0.3f ms, TsMeanDiff %0.3f ms, samples %d", (float)stsDiff/10000.0f, (float)m_fMTDMean/10000.0f, m_nNextMTD);
  }

    // Update the rolling timestamp difference sum
    // (these values are initialised in OnThreadStartPlay())
  int tempNextMTD = (m_nNextMTD % NB_MTDSIZE);
  m_llMTDSumAvg -= m_pllMTD[tempNextMTD];
  m_pllMTD[tempNextMTD] = stsDiff;
  m_llMTDSumAvg += stsDiff;
  m_nNextMTD++;
  
  //LogDebug("vidPin:TimestampDisconChecker, nextMTD %d, TsMeanDiff %0.3f, stsDiff %0.3f", m_nNextMTD, (float)m_fMTDMean/10000.0f, (float)stsDiff/10000.0f);
  
  return mtdDiscontinuity;
}

//******************************************************
/// Called when thread is about to start delivering data to the codec
///
HRESULT CVideoPin::OnThreadStartPlay()
{  
  DWORD thrdID = GetCurrentThreadId();
  LogDebug("vidPin:OnThreadStartPlay(%f), rate:%02.2f, threadID:0x%x, GET_TIME_NOW:0x%x", (float)m_rtStart.Millisecs()/1000.0f, m_dRateSeeking, thrdID, GET_TIME_NOW());

  //set discontinuity flag indicating to codec that the new data
  //is not belonging to any previous data
  m_bDiscontinuity=TRUE;
  m_bPresentSample=false;
  m_delayedDiscont = 0;
  m_FillBuffSleepTime = 1;
  m_LastFillBuffTime = GET_TIME_NOW();
  m_sampleCount = 0;
  m_bInFillBuffer=false;
  m_bDownstreamFlush=false;

  m_pTsReaderFilter->m_ShowBufferVideo = INIT_SHOWBUFFERVIDEO;

  m_llLastComp = 0;
  m_llLastMTDts = 0;
  m_nNextMTD = 0;
	m_fMTDMean = 0;
	m_llMTDSumAvg = 0;
  ZeroMemory((void*)&m_pllMTD, sizeof(REFERENCE_TIME) * NB_MTDSIZE);

  //get file-duration and set m_rtDuration
  GetDuration(NULL);

  if( !m_bPinNoNewSegFlush ) //MS DTV video decoder can hang if we flush here...
  {
    //Downstream flush
    DeliverBeginFlush();
    DeliverEndFlush();
  }
  
  //start playing
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay( );
}

HRESULT CVideoPin::DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  LogDebug("vidPin:DeliverNewSegment(start %f, stop %f), rate:%02.2f", (float)tStart/10000000.0f, (float)tStop/10000000.0f, dRate);

  return CBaseOutputPin::DeliverNewSegment(tStart, tStop, dRate);
}

HRESULT CVideoPin::StartNewSegment()
{
  return DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
}


// CSourceSeeking
HRESULT CVideoPin::ChangeStart()
{
  m_pTsReaderFilter->SetSeeking(true);
  return UpdateFromSeek();
}
HRESULT CVideoPin::ChangeStop()
{
  m_pTsReaderFilter->SetSeeking(true);
  return UpdateFromSeek();
}
HRESULT CVideoPin::ChangeRate()
{
  if( m_dRateSeeking <= 0 )
  {
    m_dRateSeeking = 1.0;  // Reset to a reasonable value.
    return E_FAIL;
  }
  if( m_dRateSeeking > 2.0 && (m_pTsReaderFilter->m_videoDecoderCLSID == CLSID_FFDSHOWVIDEO 
                                || m_pTsReaderFilter->m_videoDecoderCLSID == CLSID_LAVVIDEO))
  {
    //FFDShow video decoder doesn't handle rate > 2.0 properly
    m_dRateSeeking = 1.0;  // Reset to a reasonable value.
    return E_FAIL;
  }

  LogDebug("vidPin: ChangeRate, m_dRateSeeking %f, Force seek done %d, IsSeeking %d",(float)m_dRateSeeking, m_pTsReaderFilter->m_bSeekAfterRcDone, m_pTsReaderFilter->IsSeeking());
  if (!m_pTsReaderFilter->m_bSeekAfterRcDone && !m_pTsReaderFilter->IsSeeking()) //Don't force seek if another pin has already triggered it
  {
    m_pTsReaderFilter->m_bForceSeekAfterRateChange = true;
    m_pTsReaderFilter->SetSeeking(true);
    return UpdateFromSeek();
  }

  return S_OK;
}

void CVideoPin::SetStart(CRefTime rtStartTime)
{
  m_rtStart = rtStartTime ;
  //LogDebug("vidPin: SetStart, m_rtStart %f",(float)m_rtStart.Millisecs()/1000.0f);
}

STDMETHODIMP CVideoPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  if (m_pTsReaderFilter->SetSeeking(true)) //We're not already seeking
  {
    return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
  }
  return S_OK;
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
///
HRESULT CVideoPin::UpdateFromSeek()
{
  LogDebug("vidPin: UpdateFromSeek, m_rtStart %f, m_dRateSeeking %f",(float)m_rtStart.Millisecs()/1000.0f,(float)m_dRateSeeking);
  return m_pTsReaderFilter->SeekPreStart(m_rtStart) ;
}

//******************************************************
/// GetAvailable() returns
/// pEarliest -> the earliest (pcr) timestamp in the file
/// pLatest   -> the latest (pcr) timestamp in the file
///
STDMETHODIMP CVideoPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
//  LogDebug("vidPin:GetAvailable");
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
  //LogDebug("vidPin:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CVideoPin::Notify(IBaseFilter * pSender, Quality q)
{
  return E_NOTIMPL;
}
