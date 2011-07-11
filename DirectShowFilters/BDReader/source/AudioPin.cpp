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
#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include <sbe.h>
#include "bdreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "pmtparser.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

// you will get NO sound if you enable debugging as there is a lot of output 
//#define SOUNDDEBUG 

#define MAX_TIME  86400000L
#define LPCM_HEADER_SIZE 4

extern void LogDebug(const char *fmt, ...);

CAudioPin::CAudioPin(LPUNKNOWN pUnk, CBDReaderFilter* pFilter, HRESULT* phr, CCritSec* section) :
  CSourceStream(NAME("pinAudio"), phr, pFilter, L"Audio"),
  m_pFilter(pFilter),
  CSourceSeeking(NAME("pinAudio"), pUnk, phr, section),
  m_section(section),
  m_pPinConnection(NULL),
  m_pReceiver(NULL),
  prevPl(-1),
  m_rtPrevSample(REFERENCE_TIME(0)),
  m_pCachedBuffer(NULL)
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
  m_bPresentSample = false;
}

CAudioPin::~CAudioPin()
{
  LogDebug("pin:dtor()");
}

STDMETHODIMP CAudioPin::NonDelegatingQueryInterface(REFIID riid, void** ppv)
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

HRESULT CAudioPin::GetMediaType(CMediaType *pmt)
{
  if (m_mt.formattype == GUID_NULL)
  {
    *pmt = m_mtInitial;
  }
  else
  {
    *pmt = m_mt;
  }

  return S_OK;
}

void CAudioPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity = onOff;
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
  {
    pRequest->cBuffers = 30;
  }

  pRequest->cbBuffer = MAX_BUFFER_SIZE;

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
  {
    LogDebug("aud:CreateEmptySample() invalid sample!");
  }
}

HRESULT CAudioPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
    CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();
    Packet* buffer = NULL;

    do
    {
      if (m_pFilter->IsSeeking() || m_pFilter->IsStopping() || demux.IsMediaChanging() || demux.m_audioPlSeen)
      {
        Sleep(20);
        CreateEmptySample(pSample);
        
        return NOERROR;
      }

      if (m_pCachedBuffer)
      {
        buffer = m_pCachedBuffer;
        m_pCachedBuffer = NULL;
      }
      else
      {
        buffer = demux.GetAudio();
      }

      // Did we reach the end of the file
      if (demux.EndOfFile())
      {
        LogDebug("aud:set eof");
        CreateEmptySample(pSample);
        
        return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }

      if (!buffer)
      {
        Sleep(10);
      }
      else
      {
//        JoinAudioBuffers(buffer, &demux);
        
        m_pFilter->m_bStreamCompensated = true; // fake

        if (buffer && buffer->nPlaylist != prevPl)
        {
          LogDebug("Playlist changed From %d To %d",prevPl,buffer->nPlaylist);
          prevPl = buffer->nPlaylist;
          buffer->bDiscontinuity=true;
        
          if (m_pFilter->State() == State_Running)
          {
            demux.m_audioPlSeen = true;

            if (demux.m_videoPlSeen)
            {
              LogDebug("AUD: Request zeroing the stream time");
              m_pFilter->m_bForceSeekAfterRateChange = true;
              m_pFilter->IssueCommand(SEEK, 0);
            }

            m_pCachedBuffer = buffer;

            CreateEmptySample(pSample);
            return NOERROR;
          }
        }
/*
        if (buffer->pmt==NULL)
        {
          LogDebug("Missing Audio PMT");
        }
        else
        {
          LogDebug("Audio buffer %I64d format %d {%08x-%04x-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X}",buffer->rtStart, buffer->pmt->cbFormat,
            buffer->pmt->formattype.Data1, buffer->pmt->formattype.Data2, buffer->pmt->formattype.Data3,
            buffer->pmt->formattype.Data4[0], buffer->pmt->formattype.Data4[1], buffer->pmt->formattype.Data4[2],
            buffer->pmt->formattype.Data4[3], buffer->pmt->formattype.Data4[4], buffer->pmt->formattype.Data4[5], 
            buffer->pmt->formattype.Data4[6], buffer->pmt->formattype.Data4[7]);
        }
*/
        if (buffer->pmt->cbFormat==0)
        {
          buffer->pmt = CreateMediaType(&m_mt);
        }

        if (buffer->pmt && m_mt != *buffer->pmt)
        {
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

            //m_pFilter->OnMediaTypeChanged(3);
            m_pFilter->IssueCommand(REBUILD, 0);

            LogDebug("REBUILD: AUDIO");

            m_pCachedBuffer = buffer;

            CreateEmptySample(pSample);
        
            return NOERROR;
          }
          else
          {
            LogDebug("AUDIO CHANGE ACCEPTED");
            CMediaType* mt = new CMediaType(*buffer->pmt);
            SetMediaType(mt);
          }
        }

        if (buffer)
          m_rtPrevSample = buffer->rtStart;

        REFERENCE_TIME cRefTimeStart = -1, cRefTimeStop = -1, cRefTimeOrig = -1;
        bool hasTimestamp = buffer->rtStart != Packet::INVALID_TIME;

        if (hasTimestamp)
        {
          m_bPresentSample = true;
        }
        else
        {
          LogDebug("No Timestamp");
        }

        if (m_bPresentSample && m_dRateSeeking == 1.0)
        {
          if (m_bDiscontinuity || buffer->bDiscontinuity)
          {
            LogDebug("aud:set discontinuity");
            pSample->SetDiscontinuity(TRUE);
            m_bDiscontinuity = FALSE;
          }

          if (hasTimestamp)
          {
            // Now we have the final timestamp, set timestamp in sample
            //REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTimeStart;
            //refTime /= m_dRateSeeking; //the if rate===1.0 makes this redundant

            pSample->SetSyncPoint(true); // allow all packets to be seeking targets
            pSample->SetTime(&buffer->rtStart, &buffer->rtStop);
          }
          else
          {
            // Buffer has no timestamp
            pSample->SetTime(NULL, NULL);
            pSample->SetSyncPoint(false);
          }

          // TODO - ffdshow audio decoder doesn't like invalid media types...
          //pSample->SetMediaType(buffer->pmt);

          // currently true if timestamp is present
          //pSample->SetSyncPoint(buffer->bSyncPoint); 

          ProcessAudioSample(buffer, pSample);
          
//          LogDebug("aud: %6.3f clip: %d playlist: %d", buffer->rtStart / 10000000.0, buffer->nClipNumber, buffer->nPlaylist);          
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
  catch(int e)
  {
    LogDebug("aud:fillbuffer exception %d", e);
  }
  catch(...)
  {
    LogDebug("aud:fillbuffer exception ...");
  }
  return NOERROR;
}

void CAudioPin::JoinAudioBuffers(Packet* pBuffer, CDeMultiplexer* pDemuxer)
{
  int audioStream = 0;
  CMediaType pmt;
  pDemuxer->GetAudioStream(audioStream);
  pDemuxer->GetAudioStreamType(audioStream, pmt);

  // Currently only uncompressed PCM audio is supported
  if (pmt.subtype == MEDIASUBTYPE_PCM)
  {
    LogDebug("Joininig Audio Buffers");
    WAVEFORMATEXTENSIBLE* wfe = (WAVEFORMATEXTENSIBLE*)pmt.Format();
    WAVEFORMATEX* wf = (WAVEFORMATEX*)wfe;

    // Assuming all packets in the stream are the same size
    int packetSize = pBuffer->GetDataSize();

    int maxDurationInBytes = wf->nAvgBytesPerSec / 10; // max 100 ms buffer

    while (true)
    {
      if ((MAX_BUFFER_SIZE - pBuffer->GetDataSize() >= packetSize ) && 
          (maxDurationInBytes >= pBuffer->GetDataSize() + packetSize))
      {
        //this is now broken... TODO
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

void CAudioPin::ProcessAudioSample(Packet* pBuffer, IMediaSample *pSample)
{
  BYTE* pSampleBuffer;

  int audioStream = 0;
  CMediaType pmt;
  
  // TODO demuxer should provide a pmt on stream type changes only
  CDeMultiplexer& demux = m_pFilter->GetDemultiplexer();
  demux.GetAudioStream(audioStream);
  demux.GetAudioStreamType(audioStream, pmt);
    
  if (pmt.subtype == MEDIASUBTYPE_PCM)
  {
    WAVEFORMATEXTENSIBLE* wfe = (WAVEFORMATEXTENSIBLE*)pmt.Format();
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

HRESULT CAudioPin::ChangeStart()
{
  UpdateFromSeek();
  return S_OK;
}

HRESULT CAudioPin::ChangeStop()
{
  UpdateFromSeek();
  return S_OK;
}

HRESULT CAudioPin::ChangeRate()
{
  /*if( m_dRateSeeking <= 0 )
  {
    m_dRateSeeking = 1.0;  // Reset to a reasonable value.
    return E_FAIL;
  }*/
  LogDebug("aud: ChangeRate, m_dRateSeeking %f, Force seek done %d",(float)m_dRateSeeking, m_pFilter->m_bSeekAfterRcDone);
  if (!m_pFilter->m_bSeekAfterRcDone) //Don't force seek if another pin has already triggered it
  {
    m_pFilter->m_bForceSeekAfterRateChange = true;
  }
  UpdateFromSeek();
  return S_OK;
}

//******************************************************
/// Called when thread is about to start delivering data to the codec
///
HRESULT CAudioPin::OnThreadStartPlay()
{
  //set flag to compensate any differences in the stream time & file time
  m_pFilter->GetDemultiplexer().m_bAudioVideoReady = false;
  m_pFilter->GetDemultiplexer().m_audioPlSeen = false;

  delete m_pCachedBuffer;
  m_pCachedBuffer = NULL;

  //set discontinuity flag indicating to codec that the new data
  //is not belonging to any previous data
  m_bDiscontinuity = true;
  m_bPresentSample = false;

  LogDebug("aud:OnThreadStartPlay(%f) %02.2f", (float)m_rtStart.Millisecs() / 1000.0f, m_dRateSeeking);

  //start playing
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
  return CSourceStream::OnThreadStartPlay();
}

void CAudioPin::SetStart(CRefTime rtStartTime)
{
  m_rtStart = rtStartTime;
}

STDMETHODIMP CAudioPin::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG* pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop, StopFlags);
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
///
void CAudioPin::UpdateFromSeek()
{
  m_pFilter->SeekPreStart(m_rtStart);
  LogDebug("aud: seek done %f/%f",(float)m_rtStart.Millisecs()/1000.0f,(float)m_rtDuration.Millisecs()/1000.0f);
}

STDMETHODIMP CAudioPin::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest )
{
  //LogDebug("aud:GetAvailable");
  return CSourceSeeking::GetAvailable(pEarliest, pLatest);
}

STDMETHODIMP CAudioPin::GetDuration(LONGLONG *pDuration)
{
  //LogDebug("aud:GetDuration");
  REFERENCE_TIME refTime;
  m_pFilter->GetDuration(&refTime);
  m_rtDuration = CRefTime(refTime);

  if (pDuration != NULL)
  {
    return CSourceSeeking::GetDuration(pDuration);
  }
  return S_OK;
}

STDMETHODIMP CAudioPin::GetCurrentPosition(LONGLONG* pCurrent)
{
  //LogDebug("aud:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CAudioPin::Notify(IBaseFilter* pSender, Quality q)
{
  return E_NOTIMPL;
}