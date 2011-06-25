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
#include "bdreader.h"
#include "AudioPin.h"
#include "Videopin.h"
#include "mediaformats.h"
#include <wmcodecdsp.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define MAX_TIME  86400000L

extern void LogDebug(const char *fmt, ...) ;

void LogMediaSample(IMediaSample * pSample, int iFrameNumber)
{
  char filename[1024]="c:\\BDReaderAnalysis\\BDReader\\Log.log";
  char frameFilename[1024];
  sprintf(frameFilename,"c:\\BDReaderAnalysis\\BDReader\\Frames\\%d.log",iFrameNumber);
  FILE* fp = fopen(filename,"a+");
  long iSampleSize;

  if (fp != NULL)
  {
    REFERENCE_TIME rtStart, rtStop;
    pSample->GetMediaTime(&rtStart, &rtStop);
    AM_MEDIA_TYPE * pMediaType;
    pSample->GetMediaType(&pMediaType);
    REFERENCE_TIME rtTimeStart,rtTimeStop;
    pSample->GetTime(&rtTimeStart,&rtTimeStop);
    bool bDiscontinuity = pSample->IsDiscontinuity() == S_OK;
    bool bPreRoll = pSample->IsPreroll() == S_OK;
    bool bSyncPoint = pSample->IsSyncPoint() == S_OK;
    iSampleSize = pSample->GetActualDataLength();
    // We'll leave Mediatype for now
    fprintf(fp,"%d %I64d %I64d %I64d %I64d %d %d %d %d\n",
      iFrameNumber, rtStart, rtStop, rtTimeStart, rtTimeStop, bDiscontinuity, bPreRoll, bSyncPoint, iSampleSize);
    fclose(fp);
  }
  fp = fopen(frameFilename,"w+");
  if (fp != NULL)
  {
    BYTE * pData;
    pSample->GetPointer(&pData);
    for (int i= 0;i<(iSampleSize+79)/80;i++)
    {
      for (int j=0;j<80;j++)
      {
        if (i*80+j<iSampleSize)
        {
          fprintf(fp,"%02X",pData[i*16+j]);
        }
      }
      fprintf(fp,"\n");
    }
    fclose(fp);  
  }
};

CVideoPin::CVideoPin(LPUNKNOWN pUnk, CBDReaderFilter* pFilter, HRESULT* phr, CCritSec* section) :
  CSourceStream(NAME("pinVideo"), phr, pFilter, L"Video"),
  m_pFilter(pFilter),
  m_section(section),
  m_decoderType(general),
  CSourceSeeking(NAME("pinVideo"),pUnk, phr, section),
  m_pPinConnection(NULL),
  m_pReceiver(NULL),
  prevPl(-1),
  m_rtPrevSample(REFERENCE_TIME(0)),
  m_pCachedBuffer(NULL)
{
  m_rtStart = 0;
  m_bConnected = false;
  m_bPresentSample = false;
  m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute  |
    AM_SEEKING_CanSeekForwards  |
    AM_SEEKING_CanSeekBackwards |
    AM_SEEKING_CanGetStopPos  |
    AM_SEEKING_CanGetDuration |
    //AM_SEEKING_CanGetCurrentPos |
    AM_SEEKING_Source;
}

CVideoPin::~CVideoPin()
{
}

void CVideoPin::DetectVideoDecoder()
{
  HRESULT hr = S_FALSE;
  ULONG fetched = 0;
  bool decoderFound = false;

  FILTER_INFO filterInfo;
  m_decoderType = general;
  
  IEnumFilters * piEnumFilters = NULL;
  if (m_pFilter->GetFilterGraph() && 
      SUCCEEDED(m_pFilter->GetFilterGraph()->EnumFilters(&piEnumFilters)))
  {
    IBaseFilter * pFilter;
    while (piEnumFilters->Next(1, &pFilter, &fetched) == NOERROR && !decoderFound)
    {
      if (pFilter->QueryFilterInfo(&filterInfo) == S_OK)
      {
        if (wcscmp(filterInfo.achName, L"ArcSoft Video Decoder") == 0)
        {
          m_decoderType = Arcsoft;
          decoderFound = true;
        }
        else if (wcsncmp(filterInfo.achName, L"CyberLink Video Decoder", 23) == 0 ||
                 wcsncmp(filterInfo.achName, L"CyberLink H.264/AVC Decoder", 27) == 0 ||
                 wcsncmp(filterInfo.achName, L"CyberLink VC-1 Decoder", 22) == 0)
        {
          m_decoderType = Cyberlink;
          decoderFound = true;
        }

        filterInfo.pGraph->Release();
      }
      pFilter->Release();
      pFilter = NULL;
    }
    piEnumFilters->Release();
  }
}

bool CVideoPin::IsConnected()
{
  return m_bConnected;
}

STDMETHODIMP CVideoPin::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  if (riid == IID_IMediaSeeking)
  {
    return CSourceSeeking::NonDelegatingQueryInterface(riid, ppv);
  }
  if (riid == IID_IMediaPosition)
  {
    return CSourceSeeking::NonDelegatingQueryInterface(riid, ppv);
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CVideoPin::GetMediaType(CMediaType* pmt)
{
  CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();
  demux.GetVideoStreamType(*pmt);

  DetectVideoDecoder();

  if (pmt->subtype == FOURCCMap('1CVW'))
  {
    if (m_decoderType == Arcsoft)
    {
      LogDebug("CVideoPin::GetMediaType - force Arcsoft VC-1 GUID");
      pmt->subtype = MEDIASUBTYPE_WVC1_ARCSOFT;
    }
    else if (m_decoderType == Cyberlink)
    {
      LogDebug("CVideoPin::GetMediaType - force Cyberlink VC-1 GUID");
      pmt->subtype = MEDIASUBTYPE_WVC1_CYBERLINK;
    }
  }

  return S_OK;
}

void CVideoPin::SetInitialMediaType(const CMediaType* pmt)
{
  m_mtInitial = *pmt;
}

HRESULT CVideoPin::DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pRequest)
{
  CheckPointer(pAlloc, E_POINTER);
  CheckPointer(pRequest, E_POINTER);

  if (pRequest->cBuffers == 0)
  {
    pRequest->cBuffers = 1;
  }

  // Would be better if this would be allocated on sample basis
  pRequest->cbBuffer = 0x1000000;

  ALLOCATOR_PROPERTIES Actual;
  HRESULT hr = pAlloc->SetProperties(pRequest, &Actual);
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

HRESULT CVideoPin::CheckConnect(IPin* pReceivePin)
{
  return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CVideoPin::CompleteConnect(IPin* pReceivePin)
{
  HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
  if (SUCCEEDED(hr))
  {
    LogDebug("vid:CompleteConnect() done");
    m_bConnected = true;
  }
  else
  {
    LogDebug("vid:CompleteConnect() failed:%x", hr);
    return hr;
  }

  REFERENCE_TIME refTime;
  m_pFilter->GetDuration(&refTime);
  m_rtDuration = CRefTime(refTime);

  pReceivePin->QueryInterface(IID_IPinConnection, (void**)&m_pPinConnection);
  m_pReceiver = pReceivePin;

  return hr;
}

HRESULT CVideoPin::BreakConnect()
{
  m_bConnected = false;
  return CSourceStream::BreakConnect();
}

void CVideoPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity = onOff;
}

HRESULT CVideoPin::DoBufferProcessingLoop(void)
{
  Command com;
  OnThreadStartPlay();

  do 
  {
    while (!CheckRequest(&com)) 
    {
      IMediaSample* pSample;
      HRESULT hr = GetDeliveryBuffer(&pSample, NULL, NULL, 0);
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
        if (pSample->GetActualDataLength() > 0)
        {
          //static int iFrameNumber = 0;
          //LogMediaSample(pSample, iFrameNumber++);
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

void CVideoPin::CreateEmptySample(IMediaSample *pSample)
{
  if (pSample)
  {
    pSample->SetTime(NULL, NULL);
    pSample->SetActualDataLength(0);
    pSample->SetSyncPoint(false);
    pSample->SetDiscontinuity(true);
  }
  else
  {
    LogDebug("aud:CreateEmptySample() invalid sample!");
  }
}

HRESULT CVideoPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();
    Packet* buffer = NULL;

    do
    {
      if (m_pFilter->IsSeeking() || m_pFilter->IsStopping() || demux.IsMediaChanging() || demux.m_videoPlSeen)
      {
        Sleep(5);
        CreateEmptySample(pSample);
        
        return NOERROR;
      }
      
      bool noAudioStreams = demux.GetAudioStreamCount() == -1;

      if (m_pCachedBuffer)
      {
        buffer = m_pCachedBuffer;
        m_pCachedBuffer = NULL;
      }
      else
      {
        buffer = demux.GetVideo();
      }

      if (buffer && (buffer->nClipNumber < 0 || buffer->nPlaylist < 0))
        int debugMe = 0;

      // sample belongs to a new playlist
      if (buffer && buffer->nPlaylist != prevPl)
      {
        prevPl = buffer->nPlaylist;
        
        if (m_pFilter->State() == State_Running)
        {
          demux.m_videoPlSeen = true;

          if (demux.m_audioPlSeen)
          {
            CRefTime zeroTime;

            LogDebug("VID: Request zeroing the stream time");
            m_pFilter->m_bForceSeekAfterRateChange = true;
            m_pFilter->RefreshStreamPosition(zeroTime);
          }

          m_pCachedBuffer = buffer;

          CreateEmptySample(pSample);
          return NOERROR;
        }
      }

      if (buffer)
        m_rtPrevSample = buffer->rtStart;

      // Did we reach the end of the file?
      if (demux.EndOfFile())
      {
        LogDebug("vid:set eof");
        CreateEmptySample(pSample);
        
        return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }

      if (!buffer)
      {
        Sleep(10);
      }
      else
      {
/*        if (buffer->pmt==NULL)
        {
          LogDebug("Missing Video PMT");
        }
        else
        {
          LogDebug("Video buffer %I64d format %d {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X}",buffer->rtStart, buffer->pmt->cbFormat,
            buffer->pmt->formattype.Data1, buffer->pmt->formattype.Data2, buffer->pmt->formattype.Data3,
            buffer->pmt->formattype.Data4[0], buffer->pmt->formattype.Data4[1], buffer->pmt->formattype.Data4[2],
            buffer->pmt->formattype.Data4[3], buffer->pmt->formattype.Data4[4], buffer->pmt->formattype.Data4[5], 
            buffer->pmt->formattype.Data4[6], buffer->pmt->formattype.Data4[7]);
        }
*/
        if (buffer->pmt && m_mt.cbFormat != buffer->pmt->cbFormat)
        {
          LogDebug("NEW VIDEO FORMAT %d - old %d", buffer->pmt->cbFormat, m_mt.cbFormat);
            
          HRESULT hrAccept = S_FALSE;

          if (m_pPinConnection)
          {
            hrAccept = m_pPinConnection->DynamicQueryAccept(buffer->pmt);
          }
          else if (m_pReceiver)
          {
            LogDebug("DynamicQueryAccept - not avail"); 
            hrAccept = m_pReceiver->QueryAccept(buffer->pmt);
          }

          if (hrAccept != S_OK)
          {
            demux.SetMediaChanging(true);

            CMediaType* mt = new CMediaType(*buffer->pmt);
            SetMediaType(mt);

            m_pFilter->OnMediaTypeChanged(3);

            LogDebug("REBUILD: VIDEO");

            CreateEmptySample(pSample);
        
            m_pCachedBuffer = buffer;

            return NOERROR;
          }
          else
          {
            LogDebug("VIDEO CHANGE ACCEPTED");
            CMediaType* mt = new CMediaType(*buffer->pmt);
            SetMediaType(mt);
          }
        }

        REFERENCE_TIME cRefTimeStart = -1, cRefTimeStop = -1, cRefTimeOrig = -1;
        bool hasTimestamp = buffer->rtStart != Packet::INVALID_TIME;

        if (hasTimestamp)
        {
          m_bPresentSample = true;
       }

        if (m_bPresentSample)
        {
          if (m_bDiscontinuity || buffer->bDiscontinuity)
          {
            LogDebug("vid:set discontinuity");
            pSample->SetDiscontinuity(true);
            m_bDiscontinuity = false;
          }

          //LogDebug("vid: video buffer type = %d", buffer->GetVideoServiceType());

          if (hasTimestamp)
          {
            //now we have the final timestamp, set timestamp in sample
            if (abs(m_dRateSeeking - 1.0) > 0.5)
            {
              pSample->SetTime(&buffer->rtStart, &buffer->rtStop);
            }
            else
            {
              pSample->SetTime(&buffer->rtStart, &buffer->rtStop);
            }
          }
          else
          {
            // Buffer has no timestamp
            pSample->SetTime(NULL, NULL);
          }

          pSample->SetSyncPoint(buffer->bSyncPoint);
          // Copy buffer into the sample
          BYTE* pSampleBuffer;
          pSample->SetActualDataLength(buffer->GetDataSize());
          pSample->GetPointer(&pSampleBuffer);
          memcpy(pSampleBuffer, buffer->GetData(), buffer->GetDataSize());
          
//          LogDebug("vid: %6.3f clip: %d playlist: %d", buffer->rtStart / 10000000.0, buffer->nClipNumber, buffer->nPlaylist);

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
  m_bDiscontinuity = true;
  m_bPresentSample = false;

  m_pFilter->GetDemultiplexer().m_videoPlSeen = false;

  LogDebug("vid:OnThreadStartPlay(%f) %02.2f %d", (float)m_rtStart.Millisecs() / 1000.0f, m_dRateSeeking, m_pFilter->IsSeeking());

  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay();
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
  LogDebug("vid: ChangeRate, m_dRateSeeking %f, Force seek done %d",(float)m_dRateSeeking, m_pFilter->m_bSeekAfterRcDone);
  if (!m_pFilter->m_bSeekAfterRcDone) //Don't force seek if another pin has already triggered it
  {
    m_pFilter->m_bForceSeekAfterRateChange = true;
  }
  UpdateFromSeek();
  return S_OK;
}

void CVideoPin::SetStart(CRefTime rtStartTime)
{
  m_rtStart = rtStartTime;
  LogDebug("vid: SetStart, m_rtStart %f", (float)m_rtStart.Millisecs() / 1000.0f);
}

STDMETHODIMP CVideoPin::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop, StopFlags);
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
///
void CVideoPin::UpdateFromSeek()
{
  m_pFilter->SeekPreStart(m_rtStart);
  LogDebug("vid: UpdateFromSeek, m_rtStart %f, m_dRateSeeking %f", (float)m_rtStart.Millisecs() / 1000.0f, (float)m_dRateSeeking);
}

STDMETHODIMP CVideoPin::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  //LogDebug("vid:GetAvailable");
  return CSourceSeeking::GetAvailable(pEarliest, pLatest);
}

STDMETHODIMP CVideoPin::GetDuration(LONGLONG *pDuration)
{
  REFERENCE_TIME refTime;
  m_pFilter->GetDuration(&refTime);
  m_rtDuration = CRefTime(refTime);

  return CSourceSeeking::GetDuration(pDuration);
}

STDMETHODIMP CVideoPin::GetCurrentPosition(LONGLONG *pCurrent)
{
  //LogDebug("vid:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CVideoPin::Notify(IBaseFilter* pSender, Quality q)
{
  return E_NOTIMPL;
}

