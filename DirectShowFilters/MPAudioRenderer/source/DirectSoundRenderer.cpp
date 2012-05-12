// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

// parts of the code are based on MPC-HC audio renderer source code

#include "stdafx.h"
#include "DirectSoundRenderer.h"

extern void Log(const char *fmt, ...);
extern void LogWaveFormat(WAVEFORMATEX* pwfx, const char *text);

DirectSoundRenderer::DirectSoundRenderer(CMPAudioRenderer* pRenderer, HRESULT *phr) : 
  m_pRenderer(pRenderer),
  m_pDS(NULL),
  m_pDSBuffer(NULL),
  m_dwDSWriteOff(0),
  m_nDSBufSize(0),
  m_dRate(1.0),
  m_hnsActualDuration(0),
  m_dwLastBufferTime(0)
{
  *phr = DirectSoundCreate8(NULL, &m_pDS, NULL);
}

DirectSoundRenderer::~DirectSoundRenderer()
{
  SAFE_RELEASE(m_pDSBuffer);
  SAFE_RELEASE(m_pDS);
}

HRESULT DirectSoundRenderer::CheckFormat(WAVEFORMATEX* pwfx)
{
  return S_OK;
}

void DirectSoundRenderer::OnReceiveFirstSample(IMediaSample* /*pMediaSample*/)
{
  // TODO: check if this causes some random lip sync issues
  ClearBuffer();
}

HRESULT DirectSoundRenderer::SetMediaType(WAVEFORMATEX* pwfx)
{
  return S_OK;
}

HRESULT DirectSoundRenderer::CompleteConnect(IPin *pReceivePin)
{
  HRESULT hr = 0;

  if (SUCCEEDED(InitCoopLevel()))
    hr = CreateDSBuffer();
  return hr;
}

HRESULT DirectSoundRenderer::EndOfStream()
{
  Log("DirectSoundRenderer::EndOfStream");
  return S_OK;
}

HRESULT DirectSoundRenderer::BeginFlush()
{
  Log("DirectSoundRenderer::BeginFlush");
  
  // Make sure DShow audio buffers are empty when seeking occurs
  if (m_pDSBuffer) 
    m_pDSBuffer->Stop();

  return S_OK;
}

HRESULT DirectSoundRenderer::EndFlush()
{
  Log("DirectSoundRenderer::EndFlush");
  return S_OK;
}

HRESULT DirectSoundRenderer::Run(REFERENCE_TIME tStart)
{
  Log("DirectSoundRenderer::Run");

  HRESULT hr = 0;
  WAVEFORMATEX* pWaveFileFormat = m_pRenderer->WaveFormat();

  if (m_pDSBuffer && pWaveFileFormat) 
  {
    if (m_dRate < 1.0)
    {
      hr = m_pDSBuffer->SetFrequency((long)(pWaveFileFormat->nSamplesPerSec * m_dRate));
      if (FAILED (hr)) return hr;
    }
    else
    {
      hr = m_pDSBuffer->SetFrequency((long)pWaveFileFormat->nSamplesPerSec);
    }
  }
  ClearBuffer();

  return hr;
}


HRESULT DirectSoundRenderer::Stop(FILTER_STATE pState)
{
  if (m_pDSBuffer)
    m_pDSBuffer->Stop();    

  return S_OK;
}

HRESULT DirectSoundRenderer::Pause(FILTER_STATE pState)
{
  if (m_pDSBuffer)
    m_pDSBuffer->Stop();    

  return S_OK;
}

HRESULT DirectSoundRenderer::SetRate(double dRate)
{
  m_dRate = dRate;
  
  HRESULT hr = S_OK;

  WAVEFORMATEX* pWaveFileFormat = m_pRenderer->WaveFormat();
  if (m_pDSBuffer && pWaveFileFormat) 
  {
    if (m_dRate < 1.0)
    {
      hr = m_pDSBuffer->SetFrequency((long)(pWaveFileFormat->nSamplesPerSec * m_dRate));
      if (FAILED (hr)) return hr;
    }
    else
    {
      hr = m_pDSBuffer->SetFrequency((long)pWaveFileFormat->nSamplesPerSec);
    }
  }
  return hr;
}

HRESULT DirectSoundRenderer::CreateDSBuffer()
{
  WAVEFORMATEX* pWaveFileFormat = m_pRenderer->WaveFormat();

  if (!pWaveFileFormat) return E_POINTER;

  HRESULT hr = S_OK;
  LPDIRECTSOUNDBUFFER pDSBPrimary = NULL;
  DSBUFFERDESC dsbd;
  DSBUFFERDESC cDSBufferDesc;
  DSBCAPS bufferCaps;
  DWORD dwDSBufSize = pWaveFileFormat->nAvgBytesPerSec * 4;

  ZeroMemory(&bufferCaps, sizeof(bufferCaps));
  ZeroMemory(&dsbd, sizeof(DSBUFFERDESC));

  dsbd.dwSize = sizeof(DSBUFFERDESC);
  dsbd.dwFlags = DSBCAPS_PRIMARYBUFFER;
  dsbd.dwBufferBytes = 0;
  dsbd.lpwfxFormat = NULL;
  if (SUCCEEDED (hr = m_pDS->CreateSoundBuffer( &dsbd, &pDSBPrimary, NULL )))
  {
    hr = pDSBPrimary->SetFormat(pWaveFileFormat);
    ATLASSERT(SUCCEEDED(hr));
    SAFE_RELEASE (pDSBPrimary);
  }

  SAFE_RELEASE (m_pDSBuffer);
  cDSBufferDesc.dwSize = sizeof (DSBUFFERDESC);
  cDSBufferDesc.dwFlags     = DSBCAPS_GLOBALFOCUS			| 
                              DSBCAPS_GETCURRENTPOSITION2	| 
                              DSBCAPS_CTRLVOLUME 			|
                              DSBCAPS_CTRLPAN				|
                              DSBCAPS_CTRLFREQUENCY; 
  cDSBufferDesc.dwBufferBytes = dwDSBufSize; 
  cDSBufferDesc.dwReserved = 0; 
  cDSBufferDesc.lpwfxFormat = pWaveFileFormat; 
  cDSBufferDesc.guid3DAlgorithm	= GUID_NULL; 

  hr = m_pDS->CreateSoundBuffer(&cDSBufferDesc,  &m_pDSBuffer, NULL);

  m_nDSBufSize = 0;
  if (SUCCEEDED(hr))
  {
    bufferCaps.dwSize = sizeof(bufferCaps);
    hr = m_pDSBuffer->GetCaps(&bufferCaps);
  }
  if (SUCCEEDED (hr))
  {
    m_nDSBufSize = bufferCaps.dwBufferBytes;
    hr = ClearBuffer();
    m_pDSBuffer->SetFrequency((long)(pWaveFileFormat->nSamplesPerSec * m_dRate));
  }

  return hr;
}

HRESULT DirectSoundRenderer::ClearBuffer()
{
  HRESULT hr = S_FALSE;
  VOID* pDSLockedBuffer = NULL;
  DWORD dwDSLockedSize = 0;

  if (m_pDSBuffer)
  {
    m_dwDSWriteOff = 0;
    m_pDSBuffer->SetCurrentPosition(0);

    hr = m_pDSBuffer->Lock (0, 0, &pDSLockedBuffer, &dwDSLockedSize, NULL, NULL, DSBLOCK_ENTIREBUFFER);
    if (SUCCEEDED (hr))
    {
      memset (pDSLockedBuffer, 0, dwDSLockedSize);
      hr = m_pDSBuffer->Unlock (pDSLockedBuffer, dwDSLockedSize, NULL, NULL);
    }
  }

  return hr;
}

HRESULT DirectSoundRenderer::InitCoopLevel()
{
  HRESULT hr = S_OK;

  IVideoWindow* pVideoWindow	= NULL;
  HWND hWnd = NULL;
  CComBSTR bstrCaption;

  hr = m_pRenderer->Graph()->QueryInterface(__uuidof(IVideoWindow), (void**)&pVideoWindow);
  if (SUCCEEDED (hr))
  {
    pVideoWindow->get_Owner((OAHWND*)&hWnd);
    SAFE_RELEASE(pVideoWindow);
  }
  if (!hWnd) 
  {
    hWnd = GetTopWindow(NULL);
  }

  ATLASSERT(hWnd != NULL);
  hr = m_pDS->SetCooperativeLevel(hWnd, DSSCL_PRIORITY);
	return hr;
}

HRESULT	DirectSoundRenderer::DoRenderSample(IMediaSample *pMediaSample, LONGLONG pSampleCounter)
{
  HRESULT hr = S_OK;
  DWORD dwStatus = 0;
  const long lSize = pMediaSample->GetActualDataLength();
  DWORD dwPlayCursor = 0;
  DWORD dwWriteCursor = 0;

  hr = m_pDSBuffer->GetStatus(&dwStatus);

  if (SUCCEEDED(hr) && (dwStatus & DSBSTATUS_BUFFERLOST))
  {
    hr = m_pDSBuffer->Restore();
  }

  if ((SUCCEEDED(hr)) && ((dwStatus & DSBSTATUS_PLAYING) != DSBSTATUS_PLAYING))
  {
    hr = m_pDSBuffer->Play( 0, 0, DSBPLAY_LOOPING);
    ATLASSERT(SUCCEEDED(hr));
  }

  if (SUCCEEDED(hr)) 
  {
    hr = m_pDSBuffer->GetCurrentPosition(&dwPlayCursor, &dwWriteCursor);
  }

  if (SUCCEEDED(hr))
  {
    if (((dwPlayCursor < dwWriteCursor) &&
         (
          ((m_dwDSWriteOff >= dwPlayCursor) && (m_dwDSWriteOff <=  dwWriteCursor)) ||
          ((m_dwDSWriteOff < dwPlayCursor) && (m_dwDSWriteOff + lSize >= dwPlayCursor)))) ||
        ((dwWriteCursor < dwPlayCursor) && 
         ((m_dwDSWriteOff >= dwPlayCursor) || (m_dwDSWriteOff <  dwWriteCursor))))
    {
      m_dwDSWriteOff = dwWriteCursor;
    }

    if (m_dwDSWriteOff >= (DWORD)m_nDSBufSize)
    {
      m_dwDSWriteOff = 0;
    }
  }

  if (SUCCEEDED(hr)) hr = WriteSampleToDSBuffer(pMediaSample, NULL);

  return hr;
}

HRESULT DirectSoundRenderer::WriteSampleToDSBuffer(IMediaSample *pMediaSample, bool *looped)
{
  WAVEFORMATEX* pWaveFileFormat = m_pRenderer->WaveFormat();

  if (!m_pDSBuffer || !pWaveFileFormat) return E_POINTER;

  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtStop = 0;
  
  HRESULT hr = S_OK;
  bool loop = false;
  
  BYTE *mediaBufferResult = NULL;
  VOID* pDSLockedBuffers[2] = {NULL, NULL};
  DWORD dwDSLockedSize[2]	= {0, 0};
  BYTE* pMediaBuffer = NULL;

  long lSize = pMediaSample->GetActualDataLength();

  hr = pMediaSample->GetPointer(&pMediaBuffer);

  // resample audio stream if required
  if (m_pRenderer->Settings()->m_bUseTimeStretching)
  {
    CAutoLock cAutoLock(m_pRenderer->ResampleLock());
	
    int nBytePerSample = pWaveFileFormat->nBlockAlign;

    m_pRenderer->SoundTouch()->processSample(pMediaSample);
    lSize = m_pRenderer->SoundTouch()->receiveSamples((short**)&mediaBufferResult, 0) * nBytePerSample;
    pMediaBuffer = mediaBufferResult;
  }

  pMediaSample->GetTime(&rtStart, &rtStop);
  //Log("Sample times: start=%ld, end=%ld", rtStart, rtStop);

  if (rtStart < 0)
  {
    DWORD dwPercent	= (DWORD) ((-rtStart * 100) / (rtStop - rtStart));
    DWORD dwRemove= (lSize * dwPercent/100);

    dwRemove = (dwRemove / pWaveFileFormat->nBlockAlign) * pWaveFileFormat->nBlockAlign;
    pMediaBuffer += dwRemove;
    lSize -= dwRemove;
  }

  // Sleep for half the buffer duration since last buffer feed
  DWORD currentTime = GetTickCount();
  if (m_dwLastBufferTime != 0 && m_hnsActualDuration != 0 && m_dwLastBufferTime < currentTime && 
    (currentTime - m_dwLastBufferTime) < m_hnsActualDuration)
  {
    m_hnsActualDuration = m_hnsActualDuration - (currentTime - m_dwLastBufferTime);
    //Log("Sleeping %ld ms", m_hnsActualDuration);
    Sleep(m_hnsActualDuration);
  }

  while (SUCCEEDED (hr) && lSize > 0)
  {
    DWORD numBytesAvailable, numBytesToWrite;
    DWORD dwPlayPos, dwWritePos;

    m_pDSBuffer->GetCurrentPosition(&dwPlayPos, &dwWritePos);
    if (m_dwDSWriteOff < dwPlayPos)
      numBytesAvailable = dwPlayPos-m_dwDSWriteOff;
    else
      numBytesAvailable = dwPlayPos + m_nDSBufSize -m_dwDSWriteOff;
    
    if (lSize < numBytesAvailable)
      numBytesToWrite = lSize;
    else
      numBytesToWrite = numBytesAvailable;

    //Log("WriteSampleToDSBuffer: lSize=%d, numBytesAvailable=%d, m_dwDSWriteOff=%d, dwPlayPos=%d, dwWritePos=%d, m_nDSBufSize=%d", lSize, numBytesAvailable, m_dwDSWriteOff, dwPlayPos, dwWritePos, m_nDSBufSize);
    if (SUCCEEDED (hr))
      hr = m_pDSBuffer->Lock(m_dwDSWriteOff, numBytesToWrite, &pDSLockedBuffers[0], &dwDSLockedSize[0], &pDSLockedBuffers[1], &dwDSLockedSize[1], 0 );

    if (SUCCEEDED (hr))
    {
      if (pDSLockedBuffers [0] != NULL)
      {
        memcpy(pDSLockedBuffers[0], pMediaBuffer, dwDSLockedSize[0]);
        m_dwDSWriteOff += dwDSLockedSize[0];
      }

      if (pDSLockedBuffers [1] != NULL)
      {
        memcpy(pDSLockedBuffers[1], &pMediaBuffer[dwDSLockedSize[0]], dwDSLockedSize[1]);
        m_dwDSWriteOff = dwDSLockedSize[1];
        loop = true;
      }

      hr = m_pDSBuffer->Unlock(pDSLockedBuffers[0], dwDSLockedSize[0], pDSLockedBuffers[1], dwDSLockedSize[1]);
      //Log("Unlock returned %08x", hr);
      ATLASSERT (dwDSLockedSize [0] + dwDSLockedSize [1] == (DWORD)numBytesToWrite);
      lSize -= numBytesToWrite;
      pMediaBuffer += numBytesToWrite;

      if (lSize <= 0)
      {
        m_dwLastBufferTime = GetTickCount();
       
        // This is the duration of the filled buffer
        m_hnsActualDuration = (double)REFTIMES_PER_SEC * (m_nDSBufSize - numBytesAvailable + numBytesToWrite) 
          / pWaveFileFormat->nBlockAlign / pWaveFileFormat->nSamplesPerSec;
        
        // Sleep time is half this duration
        m_hnsActualDuration = (DWORD)(m_hnsActualDuration / REFTIMES_PER_MILLISEC / 2);
        break;
      }
      // Buffer not completely filled, sleep for half buffer capacity duration
      m_hnsActualDuration = (double)REFTIMES_PER_SEC * m_nDSBufSize / pWaveFileFormat->nBlockAlign 
        / pWaveFileFormat->nSamplesPerSec;
      
      // Sleep time is half this duration
      m_hnsActualDuration = (DWORD)(m_hnsActualDuration / REFTIMES_PER_MILLISEC / 2);
      //Log("Sleeping %ld ms", m_hnsActualDuration);
      Sleep(m_hnsActualDuration);
    }
  }
  if (SUCCEEDED(hr) && looped) *looped = loop;

  free(mediaBufferResult);

  return hr;
}
