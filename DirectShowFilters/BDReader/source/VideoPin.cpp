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
#include "bdreader.h"
#include "AudioPin.h"
#include "Videopin.h"
#include "mediaformats.h"
#include <wmcodecdsp.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);
extern void SetThreadName(DWORD dwThreadID, char* threadName);

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

CVideoPin::CVideoPin(LPUNKNOWN pUnk, CBDReaderFilter* pFilter, HRESULT* phr, CCritSec* pSection, CDeMultiplexer& pDemux) :
  CSourceStream(NAME("pinVideo"), phr, pFilter, L"Video"),
  m_pFilter(pFilter),
  m_section(pSection),
  m_demux(pDemux),
  m_decoderType(general),
  CSourceSeeking(NAME("pinVideo"), pUnk, phr, pSection),
  m_pPinConnection(NULL),
  m_pReceiver(NULL),
  m_pCachedBuffer(NULL),
  m_rtStreamOffset(0),
  m_bFlushing(false),
  m_bSeekDone(true),
  m_bDiscontinuity(false),
  m_bFirstSample(true),
  m_bInitDuration(true),
  m_bClipEndingNotified(false),
  m_bStopWait(false),
  m_rtPrevSample(_I64_MIN),
  m_rtStreamTimeOffset(0),
  m_rtTitleDuration(0),
  m_prevPl(-1),
  m_prevCl(-1),
  m_bDoFakeSeek(false)
{
  m_rtStart = 0;
  m_bConnected = false;
  m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute  |
    AM_SEEKING_CanSeekForwards  |
    AM_SEEKING_CanSeekBackwards |
    AM_SEEKING_CanGetStopPos  |
    AM_SEEKING_CanGetDuration |
    //AM_SEEKING_CanGetCurrentPos |
    AM_SEEKING_Source;

  m_eFlushStart = new CAMEvent(true);
}

CVideoPin::~CVideoPin()
{
  m_eFlushStart->Set();
  delete m_eFlushStart;
  delete m_pCachedBuffer;
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
  demux.GetVideoStreamPMT(*pmt);

  DetectVideoDecoder();

  if (pmt->subtype == FOURCCMap('1CVW'))
  {
    if (m_decoderType == Arcsoft)
    {
      LogDebug("vid: GetMediaType - force Arcsoft VC-1 GUID");
      pmt->subtype = MEDIASUBTYPE_WVC1_ARCSOFT;
    }
    else if (m_decoderType == Cyberlink)
    {
      LogDebug("vid: GetMediaType - force Cyberlink VC-1 GUID");
      pmt->subtype = MEDIASUBTYPE_WVC1_CYBERLINK;
    }
  }

  return S_OK;
}

bool CVideoPin::CompareMediaTypes(AM_MEDIA_TYPE* lhs_pmt, AM_MEDIA_TYPE* rhs_pmt)
{
  if (lhs_pmt->subtype == rhs_pmt->subtype) return true;
  if (lhs_pmt->subtype == FOURCCMap('1CVW'))
  {
    if (m_decoderType == Arcsoft)
    {
      lhs_pmt->subtype = MEDIASUBTYPE_WVC1_ARCSOFT;
    }
    else if (m_decoderType == Cyberlink)
    {
      lhs_pmt->subtype = MEDIASUBTYPE_WVC1_CYBERLINK;
    }
    return (lhs_pmt->subtype == rhs_pmt->subtype);
  }
  return false;
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

DWORD CVideoPin::ThreadProc()
{
  SetThreadName(-1, "BDReader_VIDEO");
  return __super::ThreadProc();
}

void CVideoPin::StopWait()
{
  m_bStopWait = true;

  if (m_eFlushStart)
    m_eFlushStart->Set();
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
        if (pSample->GetActualDataLength() > 0 || m_decoderType != Cyberlink)
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

void CVideoPin::CheckPlaybackState()
{
  if (m_demux.m_bVideoPlSeen)
  {
    if (m_demux.m_bVideoRequiresRebuild)
      DeliverEndOfStream();

    m_demux.m_eAudioPlSeen->Wait();

    m_pFilter->SetTitleDuration(m_rtTitleDuration);
    m_pFilter->ResetPlaybackOffset(m_rtStreamOffset);

    if (m_demux.m_bVideoRequiresRebuild || m_demux.m_bAudioRequiresRebuild)
    {
      m_demux.m_bVideoRequiresRebuild = false;
      m_demux.m_bAudioRequiresRebuild = false;

      LogDebug("vid: REBUILD");
      m_pFilter->IssueCommand(REBUILD, m_rtStreamOffset);
      m_demux.m_bRebuildOngoing = true;
    }
    else if (!m_bStopWait && (m_demux.m_bStreamPaused || m_bDoFakeSeek))    
    {
      LogDebug("vid: Request zeroing the stream time");
      m_eFlushStart->Reset();
      m_pFilter->IssueCommand(SEEK, m_rtStreamOffset);
      m_eFlushStart->Wait();
    }

    m_bStopWait = m_demux.m_bStreamPaused = false;

    m_demux.m_eAudioPlSeen->Reset();
    m_demux.m_bVideoPlSeen = false;
  }
  else
  {
    // Audio stream requires a rebuild (in middle of the clip - user initiated)
    if (!m_demux.m_eAudioPlSeen->Check() && !m_demux.m_bVideoPlSeen && m_demux.m_bAudioRequiresRebuild)
    {
      m_demux.m_bAudioRequiresRebuild = false;
      m_demux.m_eAudioPlSeen->Reset();

      LogDebug("vid: REBUILD for audio - keep stream position");
      m_pFilter->IssueCommand(REBUILD, -1);
    }
    else
      Sleep(5);        
  }
}

HRESULT CVideoPin::FillBuffer(IMediaSample* pSample)
{
  try
  {
    Packet* buffer = NULL;

    do
    {
      if (m_pFilter->IsStopping() || m_demux.IsMediaChanging() || m_bFlushing || !m_bSeekDone || m_demux.m_bRebuildOngoing)
      {
        CreateEmptySample(pSample);
        Sleep(1);
        return S_OK;
      }

      if (m_demux.EndOfFile())
      {
        LogDebug("vid: set EOF");
        CreateEmptySample(pSample);
        return S_FALSE;
      }

      if (m_demux.m_bVideoPlSeen || m_demux.m_bAudioRequiresRebuild && !m_demux.m_bVideoPlSeen && !m_demux.m_eAudioPlSeen->Check())
      {
        CreateEmptySample(pSample);
        CheckPlaybackState();
        return S_OK;
      }

      if (m_pCachedBuffer)
      {
        LogDebug("vid: cached fetch %6.3f corr %6.3f clip: %d playlist: %d", m_pCachedBuffer->rtStart / 10000000.0, (m_pCachedBuffer->rtStart - m_rtStart) / 10000000.0, m_pCachedBuffer->nClipNumber, m_pCachedBuffer->nPlaylist);
        buffer = m_pCachedBuffer;
        m_pCachedBuffer = NULL;
        m_rtStreamTimeOffset =  buffer->rtStart - (buffer->rtPlaylistTime - m_rtStart);
      }
      else
        buffer = m_demux.GetVideo();

      if (!buffer)
      {
        if (m_bFirstSample)
          Sleep(10);
        else 
        {
          CreateEmptySample(pSample);
          if (!m_bClipEndingNotified)
          {
            DeliverEndOfStream();
            pSample->SetMediaType(&m_mt);
            m_bClipEndingNotified = true;
          }
          else
            Sleep(10);
		  
          return S_OK;
        }
      }
      else
      {
        bool useEmptySample = false;
        bool checkPlaybackState = false;

        {
          CAutoLock lock(m_section);

          if (m_prevPl == -1)
          {
            m_prevPl = buffer->nPlaylist;
            m_prevCl = buffer->nClipNumber;
          }

          if (m_bZeroStreamOffset)
          {
            m_rtStreamTimeOffset =  buffer->rtStart - (buffer->rtPlaylistTime - m_rtStart);
            m_bZeroStreamOffset = false;
          }

          if (buffer->bSeekRequired)
          {
            LogDebug("vid: Playlist changed to %d - bSeekRequired: %d offset: %6.3f rtStart: %6.3f m_rtPrevSample: %6.3f rtPlaylistTime: %6.3f", 
              buffer->nPlaylist, buffer->bSeekRequired, buffer->rtOffset / 10000000.0, buffer->rtStart / 10000000.0, m_rtPrevSample / 10000000.0, buffer->rtPlaylistTime / 10000000.0);
 
            m_demux.m_bVideoPlSeen = true;
            buffer->bSeekRequired = false;
            useEmptySample = true;
            m_bClipEndingNotified = false;

            m_bDoFakeSeek = true;
            checkPlaybackState = true;

            m_prevPl = buffer->nPlaylist;
            m_prevCl = buffer->nClipNumber;
          }
          else if (m_prevPl != buffer->nPlaylist || m_prevCl != buffer->nClipNumber)
          {
          //  LogDebug("vid: Playlist changed to %d - bSeekRequired: %d offset: %I64d rtStart: %I64d", buffer->nPlaylist, buffer->bSeekRequired, buffer->rtOffset, buffer->rtStart);

            m_pFilter->SetTitleDuration(buffer->rtTitleDuration);
            m_pFilter->ResetPlaybackOffset(buffer->rtPlaylistTime);
            m_prevPl = buffer->nPlaylist;
          //  m_prevCl = buffer->nClipNumber;
          //  m_demux.m_bVideoPlSeen = true;
          //  useEmptySample = true;

           // checkPlaybackState = true;
          }

          if (buffer->pmt && !CompareMediaTypes(buffer->pmt, &m_mt))
          {
            LogMediaType(buffer->pmt);
            
            HRESULT hrAccept = S_FALSE;

            if (m_pPinConnection)
              hrAccept = m_pPinConnection->DynamicQueryAccept(buffer->pmt);
            else if (m_pReceiver)
            {
              // Dynamic format changes cause lot of issues - currently not enabled
              //LogDebug("DynamicQueryAccept - not avail");
              //hrAccept = m_pReceiver->QueryAccept(buffer->pmt);
            }

            if (hrAccept != S_OK)
            {
              CMediaType mt(*buffer->pmt);
              SetMediaType(&mt);

              LogDebug("vid: graph rebuilding required");

              m_demux.m_bVideoRequiresRebuild = true;
              useEmptySample = true;
            }
            else
            {
              LogDebug("vid: format change accepted");
              CMediaType mt(*buffer->pmt);
              SetMediaType(&mt);
              pSample->SetMediaType(&mt);

              // Flush the stream if format change is done on the fly
              if (!buffer->bSeekRequired)
              {
                DeliverBeginFlush();
                DeliverEndFlush();
                DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
              }
            }
          }
        } // lock ends

        m_rtTitleDuration = buffer->rtTitleDuration;

        if (useEmptySample || checkPlaybackState)
        {
          m_pCachedBuffer = buffer;
          LogDebug("vid: cached push  %6.3f corr %6.3f clip: %d playlist: %d", m_pCachedBuffer->rtStart / 10000000.0, (m_pCachedBuffer->rtStart - m_rtStart) / 10000000.0, m_pCachedBuffer->nClipNumber, m_pCachedBuffer->nPlaylist);
         
          CreateEmptySample(pSample);
          CheckPlaybackState();

          return S_OK;
        }

        bool hasTimestamp = buffer->rtStart != Packet::INVALID_TIME;
        REFERENCE_TIME rtCorrectedStartTime = 0;
        REFERENCE_TIME rtCorrectedStopTime = 0;

        if (hasTimestamp)
        {
          if (m_bDiscontinuity)
          {
            LogDebug("vid: set discontinuity");
            pSample->SetDiscontinuity(true);
            pSample->SetMediaType(buffer->pmt);
            m_bDiscontinuity = false;
          }

          rtCorrectedStartTime = buffer->rtStart - m_rtStreamTimeOffset;
          rtCorrectedStopTime = buffer->rtStop - m_rtStreamTimeOffset;

          if (abs(m_dRateSeeking - 1.0) > 0.5)
            pSample->SetTime(&rtCorrectedStartTime, &rtCorrectedStopTime);
          else
            pSample->SetTime(&rtCorrectedStartTime, &rtCorrectedStopTime);

          if (m_bInitDuration)
          {
            m_pFilter->SetTitleDuration(m_rtTitleDuration);
            m_pFilter->ResetPlaybackOffset(buffer->rtPlaylistTime);
            m_bInitDuration = false;
          }

          // TODO Check if we could use a bit bigger delta time when updating the playback position
          m_pFilter->OnPlaybackPositionChange();

          m_rtPrevSample = rtCorrectedStopTime;
        }
        else // Buffer has no timestamp
          pSample->SetTime(NULL, NULL);

        pSample->SetSyncPoint(buffer->bSyncPoint);
        // Copy buffer into the sample
        BYTE* pSampleBuffer;
        pSample->SetActualDataLength(buffer->GetDataSize());
        pSample->GetPointer(&pSampleBuffer);
        memcpy(pSampleBuffer, buffer->GetData(), buffer->GetDataSize());

        m_bFirstSample = false;

#ifdef LOG_VIDEO_PIN_SAMPLES
        LogDebug("vid: %6.3f corr %6.3f playlist time %6.3f clip: %d playlist: %d size: %d", buffer->rtStart / 10000000.0, rtCorrectedStartTime / 10000000.0, 
          buffer->rtPlaylistTime / 10000000.0, buffer->nClipNumber, buffer->nPlaylist, buffer->GetCount());
#endif
        delete buffer;
      }
    } while (!buffer);
    return NOERROR;
  }

  catch(...)
  {
    LogDebug("vid: FillBuffer exception");
  }
  return NOERROR;
}

HRESULT CVideoPin::OnThreadStartPlay()
{
  {
    CAutoLock lock(CSourceSeeking::m_pLock);
    m_bDiscontinuity = true;
    m_bFirstSample = true;
    m_bClipEndingNotified = false;
  }

  return S_OK;
}

HRESULT CVideoPin::DeliverBeginFlush()
{
  m_eFlushStart->Set();
  m_bFlushing = true;
  m_bSeekDone = false;
  HRESULT hr = __super::DeliverBeginFlush();
  LogDebug("vid: DeliverBeginFlush - hr: %08lX", hr);
  return hr;
}

HRESULT CVideoPin::DeliverEndFlush()
{
  HRESULT hr = __super::DeliverEndFlush();
  LogDebug("vid: DeliverEndFlush - hr: %08lX", hr);
  m_bFlushing = false;
  return hr;
}

HRESULT CVideoPin::DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  if (m_bFlushing || !ThreadExists()) 
  {
    m_bSeekDone = true;
    return S_FALSE;
  }

  LogDebug("vid: DeliverNewSegment start: %6.3f stop: %6.3f rate: %6.3f", tStart / 10000000.0, tStop / 10000000.0, dRate);
  m_rtStart = tStart;
  m_rtPrevSample = 0;

  m_bInitDuration = true;
  
  HRESULT hr = __super::DeliverNewSegment(tStart, tStop, dRate);
  if (FAILED(hr))
    LogDebug("vid: DeliverNewSegment - error: %08lX", hr);

  m_bSeekDone = true;

  return hr;
}

STDMETHODIMP CVideoPin::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags)
{
  return m_pFilter->SetPositionsInternal(this, pCurrent, CurrentFlags, pStop, StopFlags);
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

HRESULT CVideoPin::ChangeStart()
{
  return S_OK;
}

HRESULT CVideoPin::ChangeStop()
{
  return S_OK;
}

HRESULT CVideoPin::ChangeRate()
{
  return S_OK;
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

void CVideoPin::LogMediaType(AM_MEDIA_TYPE* pmt)
{
  if (!pmt)
  {
    LogDebug("Missing Video PMT");
  }
  else
  {
    LogDebug("Video format %d {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X}", pmt->cbFormat,
      pmt->subtype.Data1, pmt->subtype.Data2, pmt->subtype.Data3,
      pmt->subtype.Data4[0], pmt->subtype.Data4[1], pmt->subtype.Data4[2],
      pmt->subtype.Data4[3], pmt->subtype.Data4[4], pmt->subtype.Data4[5], 
      pmt->subtype.Data4[6], pmt->subtype.Data4[7]);
  }
}

