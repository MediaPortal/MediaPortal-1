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
#include <afx.h>
#include <afxwin.h>

#include <streams.h>
#include "bdreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "mediaformats.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define LPCM_HEADER_SIZE 4

extern void LogDebug(const char *fmt, ...);
extern void SetThreadName(DWORD dwThreadID, char* threadName);

CAudioPin::CAudioPin(LPUNKNOWN pUnk, CBDReaderFilter* pFilter, HRESULT* phr, CCritSec* pSection, CDeMultiplexer& pDemux) :
  CSourceStream(NAME("pinAudio"), phr, pFilter, L"Audio"),
  m_pFilter(pFilter),
  m_section(pSection),
  m_demux(pDemux),
  CSourceSeeking(NAME("pinAudio"), pUnk, phr, pSection),
  m_pPinConnection(NULL),
  m_pReceiver(NULL),
  m_pCachedBuffer(NULL),
  m_bFlushing(false),
  m_bSeekDone(true),
  m_bDiscontinuity(false),
  m_bUsePCM(false),
  m_bFirstSample(true),
  m_bZeroTimeStream(false),
  m_rtStreamTimeOffset(0)
{
  m_bConnected = false;
  m_rtStart = 0;
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

CAudioPin::~CAudioPin()
{
  if (m_eFlushStart)
  {
    m_eFlushStart->Set();
    delete m_eFlushStart;
  }

  if (m_demux.m_eAudioClipSeen)
    m_demux.m_eAudioClipSeen->Set();

  delete m_pCachedBuffer;
}

STDMETHODIMP CAudioPin::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  if (riid == IID_IMediaSeeking)
    return CSourceSeeking::NonDelegatingQueryInterface(riid, ppv);
  if (riid == IID_IMediaPosition)
    return CSourceSeeking::NonDelegatingQueryInterface(riid, ppv);

  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CAudioPin::CheckMediaType(const CMediaType* pmt)
{
  CAutoLock lock(m_pFilter->pStateLock());

  CMediaType mt;
  GetMediaTypeInternal(&mt);

  if (pmt->subtype == MEDIASUBTYPE_BD_LPCM_AUDIO)
    mt.subtype = MEDIASUBTYPE_BD_LPCM_AUDIO;

  if (mt == *pmt) 
    return NOERROR;

  return E_FAIL;  
}

HRESULT CAudioPin::GetMediaType(int iPosition, CMediaType* pMediaType)
{
  CAutoLock lock(m_pFilter->pStateLock());

  CMediaType mt;
  GetMediaTypeInternal(&mt);

  if (mt.subtype == MEDIASUBTYPE_PCM && iPosition == 0)
  {
    mt.subtype = MEDIASUBTYPE_BD_LPCM_AUDIO;
    *pMediaType = mt;
  }
  else if ((mt.subtype == MEDIASUBTYPE_PCM && iPosition == 1) || iPosition == 0)
    *pMediaType = mt;
  else
    return VFW_S_NO_MORE_ITEMS;

  return S_OK;
}

HRESULT CAudioPin::GetMediaTypeInternal(CMediaType *pmt)
{
  if (m_mt.formattype == GUID_NULL)
    *pmt = m_mtInitial;
  else
    *pmt = m_mt;

  return S_OK;
}

HRESULT CAudioPin::CheckConnect(IPin *pReceivePin)
{
  //LogDebug("aud:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CAudioPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
  HRESULT hr;
  CheckPointer(pAlloc, E_POINTER);
  CheckPointer(pRequest, E_POINTER);

  if (pRequest->cBuffers == 0)
    pRequest->cBuffers = 30;

  pRequest->cbBuffer = MAX_BUFFER_SIZE;

  ALLOCATOR_PROPERTIES Actual;
  hr = pAlloc->SetProperties(pRequest, &Actual);

  if (FAILED(hr))
    return hr;

  if (Actual.cbBuffer < pRequest->cbBuffer)
    return E_FAIL;

  return S_OK;
}

HRESULT CAudioPin::CompleteConnect(IPin *pReceivePin)
{
  HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
  if (SUCCEEDED(hr))
  {
    LogDebug("aud:CompleteConnect() done");
    m_bConnected = true;
  }
  else
  {
    LogDebug("aud:CompleteConnect() failed:%x", hr);
    return hr;
  }

  REFERENCE_TIME refTime;
  m_pFilter->GetDuration(&refTime);
  m_rtDuration = CRefTime(refTime);
  
  pReceivePin->QueryInterface(IID_IPinConnection, (void**)&m_pPinConnection);
  m_pReceiver = pReceivePin;

  return hr;
}

HRESULT CAudioPin::BreakConnect()
{
  m_bConnected = false;
  return CSourceStream::BreakConnect();
}

DWORD CAudioPin::ThreadProc()
{
  SetThreadName(-1, "BDReader_AUDIO");
  return __super::ThreadProc();
}

void CAudioPin::SetInitialMediaType(const CMediaType* pmt)
{
  m_mtInitial = *pmt;
}

void CAudioPin::CreateEmptySample(IMediaSample *pSample)
{
  if (pSample)
  {
    pSample->SetTime(NULL, NULL);
    pSample->SetActualDataLength(0);
    pSample->SetSyncPoint(false);
    pSample->SetDiscontinuity(true);
  }
  else
    LogDebug("aud: CreateEmptySample() invalid sample!");
}

HRESULT CAudioPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    Packet* buffer = NULL;

    do
    {
      if (m_demux.m_bAudioWaitForSeek)
      {
        m_demux.m_bAudioWaitForSeek = false;
        m_bSeekDone = false;
      }

      if (!m_bSeekDone || m_pFilter->IsStopping() || m_bFlushing || m_demux.IsMediaChanging() || m_demux.m_bRebuildOngoing || 
        m_demux.m_eAudioClipSeen->Check())
      {
        CreateEmptySample(pSample);
        Sleep(1);
        return S_OK;
      }

      if (m_pCachedBuffer)
      {
        LogDebug("aud: cached fetch %6.3f clip: %d playlist: %d", m_pCachedBuffer->rtStart / 10000000.0, m_pCachedBuffer->nClipNumber, m_pCachedBuffer->nPlaylist);
        buffer = m_pCachedBuffer;
        m_pCachedBuffer = NULL;
      }
      else
        buffer = m_demux.GetAudio();

      if (m_demux.EndOfFile())
      {
        LogDebug("aud: set EOF");
        CreateEmptySample(pSample);
        m_demux.m_eAudioClipSeen->Set();
        return S_FALSE;
      }

      if (!buffer)
      {
        if (m_bFirstSample)
          Sleep(10);
        else 
        {
          CreateEmptySample(pSample);
          Sleep(10);
          return S_OK;
        }
      }
      else
      {
        bool checkPlaybackState = false;
        REFERENCE_TIME rtStart = m_rtStart;

        //JoinAudioBuffers(buffer, &demux);
        
        {
          CAutoLock lock(m_section);

          if (m_demux.m_bAudioResetStreamPosition)
          {
            m_demux.m_bAudioResetStreamPosition = false;
            m_bZeroTimeStream = true;
          }

          if (buffer->bNewClip)
          {
            LogDebug("aud: Playlist changed to %d - bNewClip: %d offset: %6.3f rtStart: %6.3f rtPlaylistTime: %6.3f", 
              buffer->nPlaylist, buffer->bNewClip, buffer->rtOffset / 10000000.0, buffer->rtStart / 10000000.0, buffer->rtPlaylistTime / 10000000.0);

            checkPlaybackState = true;

            m_demux.m_eAudioClipSeen->Set();
          }

          // Do not convert LPCM to PCM if audio decoder supports LPCM (LAV audio decoder style)
          if (!m_bUsePCM && buffer->pmt && buffer->pmt->subtype == MEDIASUBTYPE_PCM)
            buffer->pmt->subtype = MEDIASUBTYPE_BD_LPCM_AUDIO;

          if (buffer->pmt && m_mt != *buffer->pmt && !buffer->bNewClip)
          {
            HRESULT hrAccept = S_FALSE;
            LogMediaType(buffer->pmt);

            if (m_pPinConnection && false) // TODO - DS audio renderer seems to be only one that supports this
              hrAccept = m_pPinConnection->DynamicQueryAccept(buffer->pmt);
            else if (m_pReceiver)
            {
              //LogDebug("aud: DynamicQueryAccept - not avail");
              GUID guid = buffer->pmt->subtype;
              if (buffer->pmt->subtype == MEDIASUBTYPE_PCM)
              {
                buffer->pmt->subtype = MEDIASUBTYPE_BD_LPCM_AUDIO;
                hrAccept = m_pReceiver->QueryAccept(buffer->pmt);
              }
              
              if (hrAccept != S_OK)
              {
                buffer->pmt->subtype = guid;
                hrAccept = m_pReceiver->QueryAccept(buffer->pmt);
                m_bUsePCM = true;
              }
              else
                m_bUsePCM = false;
            }

            if (hrAccept != S_OK)
            {
              CMediaType mt(*buffer->pmt);
              SetMediaType(&mt);

              LogDebug("aud: graph rebuilding required");

              m_demux.m_bAudioRequiresRebuild = true;
              checkPlaybackState = true;

              DeliverEndOfStream();
            }
            else
            {
              LogDebug("aud: format change accepted");
              CMediaType mt(*buffer->pmt);
              SetMediaType(&mt);
              pSample->SetMediaType(&mt);

              CreateEmptySample(pSample);
              m_pCachedBuffer = buffer;

              return S_OK;
            }
          }
        } // lock ends

        if (checkPlaybackState)
        {
          CreateEmptySample(pSample);
          m_pCachedBuffer = buffer;
          LogDebug("aud: cached push  %6.3f clip: %d playlist: %d", m_pCachedBuffer->rtStart / 10000000.0, m_pCachedBuffer->nClipNumber, m_pCachedBuffer->nPlaylist);
          
          if (checkPlaybackState)
          {
            if (buffer->pmt && m_mt != *buffer->pmt && !buffer->bNewClip)
            {
              CMediaType mt(*buffer->pmt);
              SetMediaType(&mt);
            }
          }
          m_pCachedBuffer->bNewClip = false;

          return S_OK;
        }
  
        bool hasTimestamp = buffer->rtStart != Packet::INVALID_TIME;

        REFERENCE_TIME rtCorrectedStartTime = 0;
        REFERENCE_TIME rtCorrectedStopTime = 0;

        if (hasTimestamp && m_dRateSeeking == 1.0)
        {
          if (m_bDiscontinuity)
          {
            LogDebug("aud: set discontinuity");
            pSample->SetDiscontinuity(true);
            pSample->SetMediaType(buffer->pmt);
            m_bDiscontinuity = false;
          }

          if (hasTimestamp)
          {
            if (m_bZeroTimeStream)
            {
              m_rtStreamTimeOffset = buffer->rtStart - buffer->rtClipStartTime;
              m_bZeroTimeStream=false;
            }
            // Now we have the final timestamp, set timestamp in sample
            //REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTimeStart;
            //refTime /= m_dRateSeeking; //the if rate===1.0 makes this redundant

            pSample->SetSyncPoint(true); // allow all packets to be seeking targets
            rtCorrectedStartTime = buffer->rtStart - m_rtStreamTimeOffset;//- m_rtStart;
            rtCorrectedStopTime = buffer->rtStop - m_rtStreamTimeOffset;// - m_rtStart;
            pSample->SetTime(&rtCorrectedStartTime, &rtCorrectedStopTime);
          }
          else
          {
            // Buffer has no timestamp
            pSample->SetTime(NULL, NULL);
            pSample->SetSyncPoint(false);
          }

          ProcessAudioSample(buffer, pSample);
#ifdef LOG_AUDIO_PIN_SAMPLES
          LogDebug("aud: %6.3f corr %6.3f Playlist time %6.3f clip: %d playlist: %d", buffer->rtStart / 10000000.0, rtCorrectedStartTime / 10000000.0,
            buffer->rtPlaylistTime / 10000000.0, buffer->nClipNumber, buffer->nPlaylist);
#endif
          m_bFirstSample = false;

          delete buffer;
        }
        else
        { // Buffer was not displayed because it was out of date, search for next.
          delete buffer;
          buffer = NULL;
        }
      }
    } while (!buffer);
    return NOERROR;
  }

  // Should we return something else than NOERROR when hitting an exception?
  catch (int e)
  {
    LogDebug("aud: FillBuffer exception %d", e);
  }
  catch (...)
  {
    LogDebug("aud: FillBuffer exception ...");
  }

  return NOERROR;
}

void CAudioPin::JoinAudioBuffers(Packet* pBuffer, CDeMultiplexer* pDemuxer)
{
  if (pBuffer->pmt)
  {
    // Currently only uncompressed PCM audio is supported
    if (pBuffer->pmt->subtype == MEDIASUBTYPE_PCM)
    {
      //LogDebug("aud: Joinig Audio Buffers");
      WAVEFORMATEXTENSIBLE* wfe = (WAVEFORMATEXTENSIBLE*)pBuffer->pmt->pbFormat;
      WAVEFORMATEX* wf = (WAVEFORMATEX*)wfe;

      // Assuming all packets in the stream are the same size
      int packetSize = pBuffer->GetDataSize();

      int maxDurationInBytes = wf->nAvgBytesPerSec / 10; // max 100 ms buffer

      while (true)
      {
        if ((MAX_BUFFER_SIZE - pBuffer->GetDataSize() >= packetSize ) && 
            (maxDurationInBytes >= pBuffer->GetDataSize() + packetSize))
        {
          Packet* buf = pDemuxer->GetAudio(pBuffer->nPlaylist,pBuffer->nClipNumber);
          if (buf)
          {
            byte* data = buf->GetData();
            // Skip LPCM header when copying the next buffer
            pBuffer->SetCount(pBuffer->GetDataSize() + buf->GetDataSize() - LPCM_HEADER_SIZE);
            memcpy(pBuffer->GetData()+pBuffer->GetDataSize() - (buf->GetDataSize() - LPCM_HEADER_SIZE), &data[LPCM_HEADER_SIZE], buf->GetDataSize() - LPCM_HEADER_SIZE);
            delete buf;
          }
          else
          {
            // No new buffer was available in the demuxer
            break;
          }
        }
        else
        {
          // buffer limit reached
          break;
        }
      }
    }
  }
}

void CAudioPin::ProcessAudioSample(Packet* pBuffer, IMediaSample *pSample)
{
  BYTE* pSampleBuffer;

  if (m_mt.subtype == MEDIASUBTYPE_PCM)
  {
    WAVEFORMATEXTENSIBLE* wfe = (WAVEFORMATEXTENSIBLE*)pBuffer->pmt->pbFormat;
    WAVEFORMATEX* wf = (WAVEFORMATEX*)wfe;

    int bufSize = pBuffer->GetDataSize();
    bufSize -= LPCM_HEADER_SIZE;

    BYTE* header = pBuffer->GetData();
    int bytesPerSample = (wfe->Samples.wValidBitsPerSample+4)>>3;
    int channel_layout = header[2] >> 4;
    int nChannels = wf->nChannels;
    int channelMap = channel_map_layouts[channel_layout];
    int discChannels = (nChannels + 1) &0xfe;
    
#ifdef SOUNDDEBUG
    LogDebug("Input Channels %d Output Channels %d nSamples Calc %d bytesPerSample %d",
      discChannels, nChannels, bufSize / (bytesPerSample * discChannels),bytesPerSample);
#endif

    int samples = bufSize / (bytesPerSample * discChannels);

    pSample->SetActualDataLength(samples * wf->nChannels * ((bytesPerSample+1)&0xfe));
    pSample->GetPointer(&pSampleBuffer);

    UINT32* dst32 = (UINT32*)pSampleBuffer;
    BYTE* src = pBuffer->GetData() + LPCM_HEADER_SIZE;

    ConvertLPCMFromBE(src, dst32, nChannels, samples, bytesPerSample , channelMap);
  }
  else // no specific handling - just copy the audio data
  {
    pSample->SetActualDataLength(pBuffer->GetDataSize());
    pSample->GetPointer(&pSampleBuffer);
    memcpy(pSampleBuffer, pBuffer->GetData(), pBuffer->GetDataSize());
  }
}

// switches the audio from big to little endian
// param src pointer to source data
// param dest pointer to destination for converted data
// param channels is the number of valid channels in the input stream
// param nSamples is the number of samples present
// param samplesize is the size in bytes of the sample (2 for 16 bit and 3 for 24 bit)
void CAudioPin::ConvertLPCMFromBE(BYTE * src,void * dest,int channels, int nSamples, int sampleSize, int channelMap)
{
  UINT16* dst16 = (UINT16*)dest;
  UINT32* dst32 = (UINT32*)dest;
  BYTE* csrc;
  int inputChannels = (channels + 1) & 0xfe; // there are always an even number of channels
  int outputChannels = channels;
  do 
  {
    int channel = outputChannels;
    do 
    {
      csrc = src + CHANNEL_MAP[channelMap][outputChannels-channel] * sampleSize;
      if (sampleSize == 2) // 16 bit
      {
        *dst16++ = *csrc<<8|*(csrc+1);
#ifdef SOUNDDEBUG
        LogDebug("Input 16 bit %4X:%02X%02X Output %4X:%04X", csrc,*(csrc+1),*csrc,dst16-2,*(dst16-1));
#endif
      }
      else
      {
        *dst32++ = (*csrc<<16|*(csrc+1)<<8|*(csrc+2)) << 8;
#ifdef SOUNDDEBUG
        LogDebug("Input 24 bit %4X:%02X%02X%02X Output %4X:%08X", csrc,*(csrc+2),*(csrc+1),*csrc,dst32-4,*(dst32-1));
#endif
      }
    } while (--channel);
    src += inputChannels * sampleSize;
#ifdef SOUNDDEBUG
    if (inputChannels!=outputChannels)
    {
      if (sampleSize == 2)
      {
        LogDebug("Dropped 16bit %4X:%02X%02X", src-2,*(src-1),*(src-2));
      }
      else
      {
        LogDebug("Dropped 24bit %4X:%02X%02X%02X", src-3,*(src-1),*(src-2),*(src-3));
      }
    }
#endif
  } while (--nSamples);
}

bool CAudioPin::IsConnected()
{
  return m_bConnected;
}

HRESULT CAudioPin::OnThreadStartPlay()
{
  {
    CAutoLock lock(CSourceSeeking::m_pLock);
    m_bDiscontinuity = true;
    m_bFirstSample = true;

    if (m_demux.m_eAudioClipSeen)
      m_demux.m_eAudioClipSeen->Reset();
  }

  return S_OK;
}

HRESULT CAudioPin::OnThreadDestroy()
{
  // Make sure video pin is not waiting for us
  if (m_demux.m_eAudioClipSeen)
    m_demux.m_eAudioClipSeen->Set(); 

  return S_OK;
}

HRESULT CAudioPin::DeliverBeginFlush()
{
  m_eFlushStart->Set();
  m_bFlushing = true;
  m_bSeekDone = false;
  HRESULT hr = __super::DeliverBeginFlush();
  LogDebug("aud: DeliverBeginFlush - hr: %08lX", hr);

  return hr;
}

HRESULT CAudioPin::DeliverEndFlush()
{
  HRESULT hr = __super::DeliverEndFlush();
  LogDebug("aud: DeliverEndFlush - hr: %08lX", hr);
  m_bZeroTimeStream = true;
  m_demux.m_eAudioClipSeen->Reset();
  m_bFlushing = false;

  return hr;
}

HRESULT CAudioPin::DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  if (m_bFlushing || !ThreadExists())
  {
    m_bSeekDone = true;
    return S_FALSE;
  }

  LogDebug("aud: DeliverNewSegment start: %6.3f stop: %6.3f rate: %6.3f", tStart / 10000000.0, tStop / 10000000.0, dRate);
  m_rtStart = tStart;

  HRESULT hr = __super::DeliverNewSegment(tStart, tStop, dRate);
  if (FAILED(hr))
    LogDebug("aud: DeliverNewSegment - error: %08lX", hr);

  m_bSeekDone = true;

  return hr;
}

STDMETHODIMP CAudioPin::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags)
{
  return m_pFilter->SetPositionsInternal(this, pCurrent, CurrentFlags, pStop, StopFlags);
}

STDMETHODIMP CAudioPin::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest )
{
  //LogDebug("aud: GetAvailable");
  return CSourceSeeking::GetAvailable(pEarliest, pLatest);
}

STDMETHODIMP CAudioPin::GetDuration(LONGLONG *pDuration)
{
  //LogDebug("aud:GetDuration");
  REFERENCE_TIME refTime;
  m_pFilter->GetDuration(&refTime);
  m_rtDuration = CRefTime(refTime);

  if (pDuration)
    return CSourceSeeking::GetDuration(pDuration);

  return S_OK;
}

HRESULT CAudioPin::ChangeStart()
{
  return S_OK;
}

HRESULT CAudioPin::ChangeStop()
{
  return S_OK;
}

HRESULT CAudioPin::ChangeRate()
{
  return S_OK;
}

STDMETHODIMP CAudioPin::GetCurrentPosition(LONGLONG* pCurrent)
{
  //LogDebug("aud: GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CAudioPin::Notify(IBaseFilter* pSender, Quality q)
{
  return E_NOTIMPL;
}

void CAudioPin::LogMediaType(AM_MEDIA_TYPE* pmt)
{
  if (!pmt)
    LogDebug("aud: missing audio PMT");
  else
  {
    LogDebug("aud: format %d {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X}", pmt->cbFormat,
      pmt->subtype.Data1, pmt->subtype.Data2, pmt->subtype.Data3,
      pmt->subtype.Data4[0], pmt->subtype.Data4[1], pmt->subtype.Data4[2],
      pmt->subtype.Data4[3], pmt->subtype.Data4[4], pmt->subtype.Data4[5], 
      pmt->subtype.Data4[6], pmt->subtype.Data4[7]);
  }
}