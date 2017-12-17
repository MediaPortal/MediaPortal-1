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
#include "audiopin.h"
#include "videopin.h"
#include "pmtparser.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define MAX_TIME  86400000L
byte MPEG1AudioFormat[] =
{
  0x50, 0x00,       //wFormatTag
  0x02, 0x00,       //nChannels
  0x80, 0xBB, 0x00, 0x00, //nSamplesPerSec
  0x00, 0x7D, 0x00, 0x00, //nAvgBytesPerSec
  0x00, 0x03,       //nBlockAlign
  0x00, 0x00,       //wBitsPerSample
  0x16, 0x00,       //cbSize
  0x02, 0x00,       //wValidBitsPerSample
  0x00, 0xE8,       //wSamplesPerBlock
  0x03, 0x00,       //wReserved
  0x01, 0x00, 0x01,0x00,  //dwChannelMask
  0x01, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00

};
extern void LogDebug(const char *fmt, ...) ;
extern DWORD m_tGTStartTime;

CAudioPin::CAudioPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
  CSourceStream(NAME("pinAudio"), phr, pFilter, L"Audio"),
  m_pTsReaderFilter(pFilter),
  CSourceSeeking(NAME("pinAudio"),pUnk,phr,section),
  m_section(section)
{
  m_bConnected=false;
  m_iPosition=0;
  m_rtStart=0;
  m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute  |
    AM_SEEKING_CanSeekForwards  |
    AM_SEEKING_CanSeekBackwards |
    AM_SEEKING_CanGetStopPos  |
    AM_SEEKING_CanGetDuration |
    AM_SEEKING_Source;
  m_bInFillBuffer=false;
  m_bPinNoAddPMT = false;
  m_bAddPMT = false;
  m_bDisableSlowPlayDiscontinuity=false;
  m_nMaxAFT = -1;
  m_bThreadRunning = false;
}

CAudioPin::~CAudioPin()
{
  LogDebug("audPin:dtor()");
}
STDMETHODIMP CAudioPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
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

HRESULT CAudioPin::CheckMediaType(const CMediaType* pmt)
{
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();

  if (!demux.PatParsed())
  {
    return E_FAIL;
  }
  if (!demux.AudPidGood())
  {
    return E_FAIL;
  }

  CMediaType pmti;
  int audioIndex = 0;
  demux.GetAudioStream(audioIndex);
  
  for (int iPosition(0); iPosition <2; iPosition++)
  {
    demux.GetAudioStreamType(audioIndex, pmti, iPosition);
    CMediaType* ppmti = &pmti;
  
    if(*pmt == *ppmti)
    {
      m_iPosition = iPosition;
      //LogDebug("audPin:CheckMediaType() ok, iPosition = %d", iPosition);  
      return S_OK;
    }
  }

  //LogDebug("audPin:CheckMediaType() fail");  
  return E_FAIL;
}

HRESULT CAudioPin::GetMediaType(int iPosition, CMediaType *pmt)
{
  CheckPointer(pmt, E_POINTER);

  //LogDebug("audPin:GetMediaType() index = %d", iPosition);
  
  // This should never happen          
  if (iPosition < 0) 
  {              
    return E_INVALIDARG;
  }                                        
  if (iPosition > 1)   
  {           
    return VFW_S_NO_MORE_ITEMS;
  }   

  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();

//  int audioIdx = 0;
//  demux.GetAudioStream(audioIdx);
//  LogDebug("audPin:GetMediaType(), audioIdx: %d, iPosition: %d, patParsed: %d", audioIdx, iPosition, demux.PatParsed());

  for (int i=0; i < 400; i++) //Wait up to 2 sec for pmt to be valid
  {
    if (demux.PatParsed())
    {
      if (!demux.AudPidGood())
      {
        //No audio stream
        //LogDebug("audPin:GetMediaType() - no pid");
        return VFW_S_NO_MORE_ITEMS;
      }
      else
      {
        int audioIndex = 0;
        demux.GetAudioStream(audioIndex);
        if(demux.GetAudioStreamType(audioIndex, *pmt, iPosition))
        {
          //LogDebug("audPin:GetMediaType() - good pid");
          return S_OK;
        }
      }
    }
    Sleep(5);
  }

  //Return a null media type
  LogDebug("audPin:GetMediaType() - Timeout");
  pmt->InitMediaType();
  return S_OK;
}

void CAudioPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity=onOff;
}

void CAudioPin::SetAddPMT()
{
  if (!m_bAddPMT) 
  {
    LogDebug("audPin:SetAddPMT()");
  }
  m_bAddPMT = true;
  m_sampleCount = 0;
}

HRESULT CAudioPin::CheckConnect(IPin *pReceivePin)
{
  //LogDebug("audPin:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CAudioPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
  HRESULT hr;
  CheckPointer(pAlloc, E_POINTER);
  CheckPointer(pRequest, E_POINTER);

  pRequest->cBuffers = max(AUD_PIN_BUFFERS, pRequest->cBuffers);
  pRequest->cbBuffer = max(MAX_BUFFER_SIZE, (ULONG)pRequest->cbBuffer);

  ALLOCATOR_PROPERTIES Actual;
  hr = pAlloc->SetProperties(pRequest, &Actual);
  if (FAILED(hr))
  {
    return hr;
  }

  if (Actual.cbBuffer < pRequest->cbBuffer)
  {
    LogDebug("audPin:DecideBufferSize - failed to get buffer");
    return E_FAIL;
  }

  return S_OK;
}

HRESULT CAudioPin::CompleteConnect(IPin *pReceivePin)
{
  m_bInFillBuffer = false;
  m_bPinNoAddPMT = false;
  m_bAddPMT = true;
  m_bDisableSlowPlayDiscontinuity=false;
  //LogDebug("audPin:CompleteConnect()");
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
    LogDebug("audPin:CompleteConnect() ok, filter: %s, iPosition: %d", szName, m_iPosition);
    
    m_bConnected=true;

    CLSID &ref=m_pTsReaderFilter->GetCLSIDFromPin(pReceivePin);
    m_pTsReaderFilter->m_audioDecoderCLSID = ref;
    if (m_pTsReaderFilter->m_audioDecoderCLSID == CLSID_LAVAUDIO)
    {
      m_bDisableSlowPlayDiscontinuity = (m_pTsReaderFilter->m_regLAV_AutoAVSync > 0);
      LogDebug("audPin:CompleteConnect() DisableSlowPlayDiscontinuity = %d", m_bDisableSlowPlayDiscontinuity);
    }
  }
  else
  {
    LogDebug("audPin:CompleteConnect() failed:%x, iPosition: %d", hr, m_iPosition);
    return E_FAIL;
  }

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
  //LogDebug("audPin:CompleteConnect() end");
  return hr;
}

HRESULT CAudioPin::BreakConnect()
{
  m_bConnected=false;
  m_iPosition=0;
  return CSourceStream::BreakConnect();
}

void CAudioPin::CreateEmptySample(IMediaSample *pSample)
{
  if (pSample)
  {
    pSample->SetTime(NULL, NULL);
    pSample->SetActualDataLength(0);
    pSample->SetSyncPoint(false);
    pSample->SetDiscontinuity(false);
  }
  else
    LogDebug("audPin: CreateEmptySample() invalid sample!");
}

HRESULT CAudioPin::DoBufferProcessingLoop(void)
{
  if (!m_bConnected) 
  {
    return S_OK;
    m_bThreadRunning = false;
  }
    
  Command com;
  OnThreadStartPlay();
  m_bThreadRunning = true;
  SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_NORMAL);

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
        if ((pSample->GetActualDataLength() > 0) && !m_pTsReaderFilter->IsStopping() && m_bConnected)
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
          m_bThreadRunning = false;
          return S_OK;
        }
      } 
      else if (hr == S_FALSE) 
      {
        // derived class wants us to stop pushing data
        pSample->Release();
        DeliverEndOfStream();
        m_bThreadRunning = false;
        return S_OK;
      } 
      else 
      {
        // derived class encountered an error
        pSample->Release();
        DbgLog((LOG_ERROR, 1, TEXT("Error %08lX from FillBuffer!!!"), hr));
        DeliverEndOfStream();
        m_pFilter->NotifyEvent(EC_ERRORABORT, hr, 0);
        m_bThreadRunning = false;
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

  m_bThreadRunning = false;  
  return S_FALSE;
}


HRESULT CAudioPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
    CBuffer* buffer=NULL;
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
        int ACnt, VCnt;
        demux.GetBufferCounts(&ACnt, &VCnt);
        if (ACnt <= 0 && VCnt <= 0) //have we used all the data ?
        {
          LogDebug("audPin:set eof");
          m_FillBuffSleepTime = 5;
          CreateEmptySample(pSample);
          m_bInFillBuffer = false;
          return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
        }
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
          //Force discon on next good sample
          m_sampleCount = 0;
          m_bDiscontinuity=true;
        }
        if (!m_pTsReaderFilter->m_bStreamCompensated && (m_nNextAFT != 0))
        {
          ClearAverageFtime();
        }
        return NOERROR;
      }
      else
      {
        m_FillBuffSleepTime = 1;
        m_bInFillBuffer = true;
      }     
                  
      // Get next audio buffer from demultiplexer
      buffer=demux.GetAudio(earlyStall, m_rtStart);

      if (buffer==NULL)
      {
        m_FillBuffSleepTime = 5;
      }
      else
      {
        m_bPresentSample = true ;
        
        if (buffer->GetForcePMT())
        {
          m_bAddPMT = true;
        }
        if (buffer->GetDiscontinuity())
        {
          m_bDiscontinuity = true;
        }
        
        CRefTime RefTime,cRefTime ;
        bool HasTimestamp ;
        double fTime = 0.0;
        double clock = 0.0;
        double stallPoint = AUDIO_STALL_POINT;
        //check if it has a timestamp
        if ((HasTimestamp=buffer->MediaTime(RefTime)))
        {
          cRefTime = RefTime ;
					cRefTime -= m_rtStart ;
          //adjust the timestamp with the compensation
          cRefTime-= m_pTsReaderFilter->GetCompensation() ;
          
          //Check if total compensation offset is more than +/-10ms
          if (abs(m_pTsReaderFilter->GetTotalDeltaComp()) > 100000)
          {
            if (!m_bDisableSlowPlayDiscontinuity)
            {
              //Force downstream filters to resync by setting discontinuity flag
              pSample->SetDiscontinuity(TRUE);
            }
            m_pTsReaderFilter->ClearTotalDeltaComp();
          }

          REFERENCE_TIME RefClock = 0;
          m_pTsReaderFilter->GetMediaPosition(&RefClock) ;
          clock = (double)(RefClock-m_rtStart.m_time)/10000000.0 ;
          fTime = ((double)cRefTime.m_time/10000000.0) - clock ;
          
          //Calculate a mean 'fTime' value using 'raw' fTime data
          CalcAverageFtime(fTime);
          if (timeNow < (m_pTsReaderFilter->m_lastPauseRun + (30*1000)))
          {
            //do this for 30s after start of play, a flush or pause
            m_fAFTMeanRef = m_fAFTMean;
          }
          
          //Add compensation time for external downstream audio delay
          //to stop samples becoming 'late' (note: this does NOT change the actual sample timestamps)
          fTime -= m_fAFTMeanRef;  //remove the 'mean' offset
          fTime += ((AUDIO_STALL_POINT/2.0) + 0.2); //re-centre the timing                 

          //Discard late samples at start of play,
          //and samples outside a sensible timing window during play 
          //(helps with signal corruption recovery)
          cRefTime -= m_pTsReaderFilter->m_ClockOnStart.m_time;

          if (fTime < -2.0)
          {                          
            if ((m_dRateSeeking == 1.0) && (m_pTsReaderFilter->State() == State_Running) && (clock > 8.0) && !demux.m_bFlushDelegated)
            { 
              //Very late - request internal flush and re-sync to stream
              demux.DelegatedFlush(false, false);
              LogDebug("audPin : Audio to render very late, flushing") ;
            }
          }
          
          if ((cRefTime.m_time >= PRESENT_DELAY) && 
              (fTime > ((cRefTime.m_time >= FS_TIM_LIM) ? -0.3 : -0.5)) && (fTime < 2.5))
          {
            if ((fTime > stallPoint) && (m_sampleCount > 2))
            {
              //Too early - stall to avoid over-filling of audio decode/renderer buffers,
              //but don't enable at start of play to make sure graph starts properly
              m_FillBuffSleepTime = 10;
              buffer = NULL;
              earlyStall = true;
              continue;
            }           
          }
          else //Don't drop samples normally - it upsets the rate matching in the audio renderer
          {
            // Sample is too late.
            m_bPresentSample = false ;
          }
          cRefTime += m_pTsReaderFilter->m_ClockOnStart.m_time;         
        }

        if (m_bPresentSample && (m_dRateSeeking == 1.0) && (buffer->Length() > 0))
        {
          //do we need to set the discontinuity flag?
          if (m_bDiscontinuity)
          {
            //ifso, set it
            pSample->SetDiscontinuity(TRUE);
            
            LogDebug("audPin: Set discontinuity L:%d B:%d fTime:%03.3f SampCnt:%d", m_bDiscontinuity, buffer->GetDiscontinuity(), (float)fTime, m_sampleCount);
            m_bDiscontinuity=FALSE;
          }

          if (m_bAddPMT && !m_pTsReaderFilter->m_bDisableAddPMT && !m_bPinNoAddPMT)
          {
            //Add MediaType info to sample
            CMediaType mt; 
            int audioIndex = 0;
            demux.GetAudioStream(audioIndex);
            demux.GetAudioStreamType(audioIndex, mt, m_iPosition);
            pSample->SetMediaType(&mt); 
            SetMediaType(&mt);               
            WAVEFORMATEX* wfe = (WAVEFORMATEX*)mt.Format();         
            LogDebug("audPin: Add pmt, fTime:%03.3f SampCnt:%d, Ch:%d, Sr:%d", (float)fTime, m_sampleCount, wfe->nChannels, wfe->nSamplesPerSec);
            m_bAddPMT = false; //Only add once
          }   

          if (HasTimestamp)
          {
            //now we have the final timestamp, set timestamp in sample
            REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
            refTime = (REFERENCE_TIME)((double)refTime/m_dRateSeeking);
            refTime += m_pTsReaderFilter->m_regAudioDelay; //add offset (to produce delay relative to video)

            pSample->SetSyncPoint(TRUE);
            pSample->SetTime(&refTime,&refTime);
            
            if (m_pTsReaderFilter->m_ShowBufferAudio || fTime < 0.02 || (m_sampleCount < 3))
            {
              int cntA, cntV;
              CRefTime firstAudio, lastAudio;
              CRefTime firstVideo, lastVideo, zeroVideo;
              cntA = demux.GetAudioBufferPts(firstAudio, lastAudio); 
              cntV = demux.GetVideoBufferPts(firstVideo, lastVideo, zeroVideo);
              
              LogDebug("Aud/Ref : %03.3f, Compensated = %03.3f ( %0.3f A/V buffers=%02d/%02d), Clk : %f, SampCnt %d, Sleep %d ms, stallPt %03.3f", (float)RefTime.Millisecs()/1000.0f, (float)cRefTime.Millisecs()/1000.0f, fTime,cntA,cntV, clock, m_sampleCount, m_FillBuffSleepTime, (float)stallPoint);
            }
            if (m_pTsReaderFilter->m_ShowBufferAudio) m_pTsReaderFilter->m_ShowBufferAudio--;
            // CalcAverageFtime(fTime);
              
            if (((float)cRefTime.Millisecs()/1000.0f) > AUDIO_READY_POINT)
            {
              m_pTsReaderFilter->m_audioReady = true;
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
          demux.EraseAudioBuff();
        }
        else
        { // Buffer was not displayed because it was out of date, search for next.
          delete buffer;
          demux.EraseAudioBuff();
          buffer=NULL ;
          m_FillBuffSleepTime = (m_dRateSeeking == 1.0) ? 1 : 2;
          m_bDiscontinuity = TRUE; //Next good sample will be discontinuous
        }
      }      
      earlyStall = false;
    } while (buffer==NULL);

    m_bInFillBuffer = false;
    return NOERROR;
  }

  // Should we return something else than NOERROR when hitting an exception?
  catch(int e)
  {
    LogDebug("audPin:fillbuffer exception %d", e);
  }
  catch(...)
  {
    LogDebug("audPin:fillbuffer exception ...");
  }
  m_FillBuffSleepTime = 5;
  CreateEmptySample(pSample);
  m_bDiscontinuity = TRUE; //Next good sample will be discontinuous  
  m_bInFillBuffer = false; 
  return NOERROR;
}

void CAudioPin::ClearAverageFtime()
{
  m_nMaxAFT = -1;
  m_nNextAFT = 0;
	m_fAFTMean = 0.0;
	m_llAFTSumAvg = 0.0;
	m_fAFTMeanRef = 0.0; 
  ZeroMemory((void*)&m_pllAFT, sizeof(double) * NB_AFTSIZE);
}

// Calculate rolling average audio ftime
void CAudioPin::CalcAverageFtime(double ftime)
{      
  if (m_nMaxAFT < 0)   
  { 
    CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();    
    if (demux.m_dfAudSampleDuration > 0.0)
    {
      //Calculate the number of samples needed to provide an average over 3 seconds 
      //but limit to maximum array size and a minimum of 16 samples
      m_nMaxAFT = max(16, min(NB_AFTSIZE, (int)(3.0/demux.m_dfAudSampleDuration)));
    }
    else
    {
      m_nMaxAFT = NB_AFTSIZE;
    }
    LogDebug("audPin:CalcAverageFtime m_nMaxAFT: %d, SampDur: %03.6f", m_nMaxAFT, (float)demux.m_dfAudSampleDuration);
  }

    // Calculate the mean timestamp difference
  if (m_nNextAFT >= m_nMaxAFT)
  {
    m_fAFTMean = m_llAFTSumAvg / (double)m_nMaxAFT;
  }
  else if (m_nNextAFT > 1)
  {
    m_fAFTMean = m_llAFTSumAvg / (double)m_nNextAFT;
  }
  else
  {
    m_fAFTMean = ftime;
  }

  // Update the rolling timestamp to presentation diff average
  // (these values are initialised in OnThreadStartPlay())
  int tempNextASD = (m_nNextAFT % m_nMaxAFT);
  m_llAFTSumAvg -= m_pllAFT[tempNextASD];
  m_pllAFT[tempNextASD] = ftime;
  m_llAFTSumAvg += ftime;
  m_nNextAFT++;
  
  //LogDebug("audPin:GetAverageSampleTime, nextASD %d, TsMeanDiff %0.3f, ftime %0.3f", m_nNextAFT, (float)m_fAFTMean/10000.0f, (float)ftime/10000.0f);  
}

double CAudioPin::GetAudToPresMeanDelta()
{
  return m_fAFTMean;
}

double CAudioPin::GetAudioPresToRefDiff()
{
  if ((m_dRateSeeking == 1.0) && (m_pTsReaderFilter->State() == State_Running))
  {
    return  m_fAFTMean - m_fAFTMeanRef;
  }
  else
  {
    return  0.0;
  }
}

bool CAudioPin::IsThreadRunning(CRefTime *pRtStartTime)
{
  *pRtStartTime = m_rtStart;
  return m_bThreadRunning;
}

bool CAudioPin::IsInFillBuffer()
{
  return (m_bInFillBuffer && m_bConnected);
}

bool CAudioPin::HasDeliveredSample()
{
  return ((m_sampleCount > 0) || !m_bConnected);
}

bool CAudioPin::IsConnected()
{
  return m_bConnected;
}

int CAudioPin::GetPMTiPosition()
{
  return m_iPosition;
}


HRESULT CAudioPin::ChangeStart()
{
  m_pTsReaderFilter->SetSeeking(true);
  return UpdateFromSeek();
}

HRESULT CAudioPin::ChangeStop()
{
  m_pTsReaderFilter->SetSeeking(true);
  return UpdateFromSeek();
}

HRESULT CAudioPin::ChangeRate()
{
  if( m_dRateSeeking <= 0 )
  {
    m_dRateSeeking = 1.0;  // Reset to a reasonable value.
    return E_FAIL;
  }
  
  LogDebug("audPin: ChangeRate, m_dRateSeeking %f, Force seek done %d, IsSeeking %d",(float)m_dRateSeeking, m_pTsReaderFilter->m_bSeekAfterRcDone, m_pTsReaderFilter->IsSeeking());
  if (!m_pTsReaderFilter->m_bSeekAfterRcDone && !m_pTsReaderFilter->IsSeeking()) //Don't force seek if another pin has already triggered it
  {
    m_pTsReaderFilter->m_bForceSeekAfterRateChange = true;
    m_pTsReaderFilter->SetSeeking(true);
    return UpdateFromSeek();
  }
  return S_OK;
}

//******************************************************
/// Called when thread is about to start delivering data to the codec
///
HRESULT CAudioPin::OnThreadStartPlay()
{
  DWORD thrdID = GetCurrentThreadId();
  LogDebug("audPin:OnThreadStartPlay(%f), rate:%02.2f, threadID:0x%x, GET_TIME_NOW:0x%x", (float)m_rtStart.Millisecs()/1000.0f, m_dRateSeeking, thrdID, GET_TIME_NOW());
  
  //set flag to compensate any differences in the stream time & file time
  m_pTsReaderFilter->m_bStreamCompensated = false;

  m_pTsReaderFilter->m_bForcePosnUpdate = true;
  m_pTsReaderFilter->WakeThread();

  m_pTsReaderFilter->m_ShowBufferAudio = INIT_SHOWBUFFERAUDIO;

  //set discontinuity flag indicating to codec that the new data
  //is not belonging to any previous data
  m_bDiscontinuity = TRUE;
  m_bPresentSample = false;
  m_sampleCount = 0;
  m_bInFillBuffer=false;
  m_pTsReaderFilter->m_audioReady = false;

  m_FillBuffSleepTime = 1;
  m_LastFillBuffTime = GET_TIME_NOW();

  ClearAverageFtime();
  
  //get file-duration and set m_rtDuration
  GetDuration(NULL);

  //Downstream flush
  DeliverBeginFlush();
  DeliverEndFlush();
  
  //start playing
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay( );
}

HRESULT CAudioPin::DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  LogDebug("audPin:DeliverNewSegment(start %f, stop %f), rate:%02.2f", (float)tStart/10000000.0f, (float)tStop/10000000.0f, dRate);

  return CBaseOutputPin::DeliverNewSegment(tStart, tStop, dRate);
}

void CAudioPin::SetStart(CRefTime rtStartTime)
{
  m_rtStart = rtStartTime ;
}

STDMETHODIMP CAudioPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
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
HRESULT CAudioPin::UpdateFromSeek()
{
  LogDebug("audPin: UpdateFromSeek, m_rtStart %f, m_dRateSeeking %f",(float)m_rtStart.Millisecs()/1000.0f,(float)m_dRateSeeking);
  return m_pTsReaderFilter->SeekPreStart(m_rtStart);

//  LogDebug("audPin: seek done %f/%f",(float)m_rtStart.Millisecs()/1000.0f,(float)m_rtDuration.Millisecs()/1000.0f);
//  return ;
}

//******************************************************
/// GetAvailable() returns
/// pEarliest -> the earliest (pcr) timestamp in the file
/// pLatest   -> the latest (pcr) timestamp in the file
///
STDMETHODIMP CAudioPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
  //LogDebug("audPin:GetAvailable");
  //if we are timeshifting, the earliest/latest timestamp can change
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    if (pEarliest)
    {
      //return the startpcr, which is the earliest pcr timestamp available in the timeshifting file
      double d2=m_pTsReaderFilter->m_duration.StartPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pEarliest= mediaTime;
    }
    if (pLatest)
    {
      //return the endpcr, which is the latest pcr timestamp available in the timeshifting file
      double d2=m_pTsReaderFilter->m_duration.EndPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pLatest= mediaTime;
    }
    //LogDebug("audPin:GetAvailable duration = %.3f ms", (float)m_pTsReaderFilter->m_duration.Duration().Millisecs());
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
STDMETHODIMP CAudioPin::GetDuration(LONGLONG *pDuration)
{
  //LogDebug("audPin:GetDuration");
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    CRefTime totalDuration=m_pTsReaderFilter->m_duration.TotalDuration();
    m_rtDuration=totalDuration;
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  if (pDuration!=NULL)
  {
    return CSourceSeeking::GetDuration(pDuration);
  }
  //LogDebug("audPin:GetDuration duration = %.3f ms, TotDuration = %.3f ms", (float)m_pTsReaderFilter->m_duration.Duration().Millisecs(), (float)m_pTsReaderFilter->m_duration.TotalDuration().Millisecs());
  return S_OK;
}

//******************************************************
/// GetCurrentPosition() simply returns that this is not implemented by this pin
/// reason is that only the audio/video renderer now exactly the
/// current playing position and they do implement GetCurrentPosition()
///
STDMETHODIMP CAudioPin::GetCurrentPosition(LONGLONG *pCurrent)
{
  //LogDebug("audPin:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CAudioPin::Notify(IBaseFilter * pSender, Quality q)
{
  return E_NOTIMPL;
}