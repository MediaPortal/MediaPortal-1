/* 
 * $Id: MpcAudioRenderer.cpp 1785 2010-04-09 14:12:59Z xhmikosr $
 *
 * (C) 2006-2010 see AUTHORS
 *
 * This file is part of mplayerc.
 *
 * Mplayerc is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * Mplayerc is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */


#include "stdafx.h"

#include <initguid.h>
#include "moreuuids.h"
#include <ks.h>
#include <ksmedia.h>

#include "MpAudioRenderer.h"
#include "FilterApp.h"

CFilterApp theApp;

#define SAFE_DELETE(p)       { if(p) { delete (p);     (p)=NULL; } }
#define SAFE_DELETE_ARRAY(p) { if(p) { delete[] (p);   (p)=NULL; } }
#define SAFE_RELEASE(p)      { if(p) { (p)->Release(); (p)=NULL; } }

// === Compatibility with Windows SDK v6.0A (define in KSMedia.h in Windows 7 SDK or later)
#ifndef STATIC_KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL

#define STATIC_KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL\
    DEFINE_WAVEFORMATEX_GUID(WAVE_FORMAT_DOLBY_AC3_SPDIF)
DEFINE_GUIDSTRUCT("00000092-0000-0010-8000-00aa00389b71", KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL);
#define KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL DEFINE_GUIDNAMED(KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL)

#define STATIC_KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL_PLUS \
    0x0000000aL, 0x0cea, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71
DEFINE_GUIDSTRUCT("0000000a-0cea-0010-8000-00aa00389b71", KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL_PLUS);
#define KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL_PLUS DEFINE_GUIDNAMED(KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL_PLUS)

#define STATIC_KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_MLP \
    0x0000000cL, 0x0cea, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71
DEFINE_GUIDSTRUCT("0000000c-0cea-0010-8000-00aa00389b71", KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_MLP);
#define KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_MLP DEFINE_GUIDNAMED(KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_MLP)

#define STATIC_KSDATAFORMAT_SUBTYPE_IEC61937_DTS\
    DEFINE_WAVEFORMATEX_GUID(WAVE_FORMAT_DTS)
DEFINE_GUIDSTRUCT("00000008-0000-0010-8000-00aa00389b71", KSDATAFORMAT_SUBTYPE_IEC61937_DTS);
#define KSDATAFORMAT_SUBTYPE_IEC61937_DTS DEFINE_GUIDNAMED(KSDATAFORMAT_SUBTYPE_IEC61937_DTS)

#define STATIC_KSDATAFORMAT_SUBTYPE_IEC61937_DTS_HD \
    0x0000000bL, 0x0cea, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71
DEFINE_GUIDSTRUCT("0000000b-0cea-0010-8000-00aa00389b71", KSDATAFORMAT_SUBTYPE_IEC61937_DTS_HD);
#define KSDATAFORMAT_SUBTYPE_IEC61937_DTS_HD DEFINE_GUIDNAMED(KSDATAFORMAT_SUBTYPE_IEC61937_DTS_HD)

#endif

CUnknown* WINAPI CMpcAudioRenderer::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
  ASSERT(phr);
  CMpcAudioRenderer *pNewObject = new CMpcAudioRenderer(punk, phr);

  if (!pNewObject)
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
  }
  return pNewObject;
}

CMpcAudioRenderer::CMpcAudioRenderer(LPUNKNOWN punk, HRESULT *phr)
: CBaseRenderer(__uuidof(this), NAME("MediaPortal - Audio Renderer"), punk, phr)
, m_Clock(static_cast<IBaseFilter*>(this), phr, this)
, m_pDSBuffer(NULL)
, m_pSoundTouch(NULL)
, m_pDS(NULL)
, m_dwDSWriteOff(0)
, m_nDSBufSize(0)
, m_dRate(1.0)
, m_pReferenceClock(NULL)
, m_pWaveFileFormat(NULL)
, m_pMMDevice(NULL)
, m_pAudioClient(NULL)
, m_pRenderClient(NULL)
, m_bUseWASAPI(true)
, m_nFramesInBuffer(0)
, m_hnsPeriod(0)
, m_hTask(NULL)
, m_nBufferSize(0)
, m_bIsAudioClientStarted(false)
, m_dwLastBufferTime(0)
, m_hnsActualDuration(0)
, m_dBias(1.0)
, m_dAdjustment(1.0)
, m_bUseTimeStretching(true)
, m_bFirstAudioSample(true)
, m_pAudioClock(NULL)
, m_nHWfreq(0)
{
  HMODULE hLib = NULL;

  // Load Vista specifics DLLs
  hLib = LoadLibrary ("AVRT.dll");
  if (hLib)
	{
    pfAvSetMmThreadCharacteristicsW		= (PTR_AvSetMmThreadCharacteristicsW)	GetProcAddress (hLib, "AvSetMmThreadCharacteristicsW");
    pfAvRevertMmThreadCharacteristics	= (PTR_AvRevertMmThreadCharacteristics)	GetProcAddress (hLib, "AvRevertMmThreadCharacteristics");
	}
	else
  {
    m_bUseWASAPI = false;	// Wasapi not available below Vista
  }

  // For debugging...
  //m_bUseWASAPI = false;

  TRACE(_T("CMpcAudioRenderer constructor"));
  if (!m_bUseWASAPI)
  {
    *phr = DirectSoundCreate8(NULL, &m_pDS, NULL);
  }
  
  if (m_bUseTimeStretching)
  {
	  // TODO: use channel based sound objects (since lib has two channel limit)
    m_pSoundTouch = new soundtouch::SoundTouch();
  }
}


CMpcAudioRenderer::~CMpcAudioRenderer()
{
  Stop();

  // DSound
  SAFE_DELETE(m_pSoundTouch);
  SAFE_RELEASE(m_pDSBuffer);
  SAFE_RELEASE(m_pDS);

  // WASAPI
  SAFE_RELEASE(m_pAudioClock);
  SAFE_RELEASE(m_pRenderClient);
  SAFE_RELEASE(m_pAudioClient);
  SAFE_RELEASE(m_pMMDevice);

  if (m_pReferenceClock)
  {
    SetSyncSource(NULL);
    SAFE_RELEASE(m_pReferenceClock);
  }

  if (m_pWaveFileFormat)
  {
    BYTE *p = (BYTE *)m_pWaveFileFormat;
    SAFE_DELETE_ARRAY(p);
  }

  SAFE_RELEASE(m_pAudioClock);

  if (m_hTask && pfAvRevertMmThreadCharacteristics)
  {
    pfAvRevertMmThreadCharacteristics(m_hTask);
  }
}


HRESULT CMpcAudioRenderer::CheckInputType(const CMediaType *pmt)
{
  return CheckMediaType(pmt);
}

HRESULT	CMpcAudioRenderer::CheckMediaType(const CMediaType *pmt)
{
  HRESULT hr = S_OK;
  
  if (!pmt) 
    return E_INVALIDARG;
  
  TRACE(_T("CMpcAudioRenderer::CheckMediaType"));
  WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->Format();

  if (!pwfx) 
    return VFW_E_TYPE_NOT_ACCEPTED;

  if ((pmt->majortype	!= MEDIATYPE_Audio) ||
      (pmt->formattype != FORMAT_WaveFormatEx))
  {
    TRACE(_T("CMpcAudioRenderer::CheckMediaType Not supported"));
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
  {
    // TODO: should we do something specific here? At least W7 audio codec is providing this info
    // WAVEFORMATPCMEX *test = (WAVEFORMATPCMEX *) pmt->Format();
    // return VFW_E_TYPE_NOT_ACCEPTED;
  }

  if (m_bUseWASAPI)
  {
    hr = CheckAudioClient((WAVEFORMATEX *)NULL);
    if (FAILED(hr))
    {
      TRACE(_T("CMpcAudioRenderer::CheckMediaType Error on check audio client"));
      return hr;
    }
    if (!m_pAudioClient) 
    {
      TRACE(_T("CMpcAudioRenderer::CheckMediaType Error, audio client not loaded"));
      return VFW_E_CANNOT_CONNECT;
    }

    if (m_pAudioClient->IsFormatSupported(AUDCLNT_SHAREMODE_EXCLUSIVE, pwfx, NULL) != S_OK)
    {
      TRACE(_T("CMpcAudioRenderer::CheckMediaType WASAPI client refused the format"));
      return VFW_E_TYPE_NOT_ACCEPTED;
    }
    TRACE(_T("CMpcAudioRenderer::CheckMediaType WASAPI client accepted the format"));
  }
  else if	(pwfx->wFormatTag	!= WAVE_FORMAT_PCM)
  {
    return VFW_E_TYPE_NOT_ACCEPTED;
  }
  return S_OK;
}

void CMpcAudioRenderer::AudioClock(UINT64& pTimestamp, UINT64& pQpc)
{
  if (m_pAudioClock)
  {
    m_pAudioClock->GetPosition(&pTimestamp, &pQpc);
    pTimestamp = pTimestamp * 10000000 / m_nHWfreq;
  }

  //TRACE(_T("AudioClock query pos: %I64d qpc: %I64d"), pTimestamp, pQpc);
}

void CMpcAudioRenderer::OnReceiveFirstSample(IMediaSample *pMediaSample)
{
  if (!m_bUseWASAPI)
  {
    ClearBuffer();
  }
}

BOOL CMpcAudioRenderer::ScheduleSample(IMediaSample *pMediaSample)
{
  REFERENCE_TIME		StartSample;
  REFERENCE_TIME		EndSample;

  // Is someone pulling our leg
  if (!pMediaSample) return FALSE;

  // Get the next sample due up for rendering.  If there aren't any ready
  // then GetNextSampleTimes returns an error.  If there is one to be done
  // then it succeeds and yields the sample times. If it is due now then
  // it returns S_OK other if it's to be done when due it returns S_FALSE
  HRESULT hr = GetSampleTimes(pMediaSample, &StartSample, &EndSample);
  if (FAILED(hr)) return FALSE;

  // If we don't have a reference clock then we cannot set up the advise
  // time so we simply set the event indicating an image to render. This
  // will cause us to run flat out without any timing or synchronisation

  // Audio should be renderer always when it arrives and the reference clock 
  // should be based on the audio HW
  //if (hr == S_OK) 
  if (!m_bFirstAudioSample)
  {
    EXECUTE_ASSERT(SetEvent((HANDLE) m_RenderEvent));
    return TRUE;
  }

  if (m_dRate <= 1.1)
  {
    ASSERT(m_dwAdvise == 0);
    ASSERT(m_pClock);
    WaitForSingleObject((HANDLE)m_RenderEvent,0);

    hr = m_pClock->AdviseTime( (REFERENCE_TIME) m_tStart, StartSample, (HEVENT)(HANDLE) m_RenderEvent, &m_dwAdvise);
    
    if (SUCCEEDED(hr)) return TRUE;
  }
  else
  {
    hr = DoRenderSample (pMediaSample);
  }

  // We could not schedule the next sample for rendering despite the fact
  // we have a valid sample here. This is a fair indication that either
  // the system clock is wrong or the time stamp for the sample is duff
  ASSERT(m_dwAdvise == 0);

  return FALSE;
}

HRESULT	CMpcAudioRenderer::DoRenderSample(IMediaSample *pMediaSample)
{
  CAutoLock cRendererLock(&m_InterfaceLock);
  
  if (m_bFirstAudioSample)
  {
    m_bFirstAudioSample = false;
  }
  
  if (m_bUseWASAPI)
  {
    return DoRenderSampleWasapi(pMediaSample);
  }
  else
  {
    return DoRenderSampleDirectSound(pMediaSample);
  }
}


STDMETHODIMP CMpcAudioRenderer::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
  if(riid == IID_IReferenceClock)
  {
    return GetInterface(static_cast<IReferenceClock*>(&m_Clock), ppv);
  }

  if (riid == IID_IAVSyncClock) 
  {
    return GetInterface(static_cast<IAVSyncClock*>(this), ppv);
  }

  if (riid == IID_IMediaSeeking) 
  {
    return GetInterface(static_cast<IMediaSeeking*>(this), ppv);
  }

	return CBaseRenderer::NonDelegatingQueryInterface (riid, ppv);
}

HRESULT CMpcAudioRenderer::SetMediaType(const CMediaType *pmt)
{
	if (!pmt) return E_POINTER;
  
  HRESULT hr = S_OK;
  int	size = 0;
  TRACE(_T("CMpcAudioRenderer::SetMediaType"));

  if (m_bUseWASAPI)
  {
    // New media type set but render client already initialized => reset it
    if (m_pRenderClient)
    {
      WAVEFORMATEX *pNewWf = (WAVEFORMATEX *)pmt->Format();
      TRACE(_T("CMpcAudioRenderer::SetMediaType Render client already initialized. Reinitialization..."));
      CheckAudioClient(pNewWf);
    }
  }

  if (m_pWaveFileFormat)
  {
    BYTE *p = (BYTE *)m_pWaveFileFormat;
    SAFE_DELETE_ARRAY(p);
  }
  
  m_pWaveFileFormat = NULL;

  WAVEFORMATEX *pwf = (WAVEFORMATEX *) pmt->Format();
  
  if (pwf)
  {
    size = sizeof(WAVEFORMATEX) + pwf->cbSize;

    m_pWaveFileFormat = (WAVEFORMATEX *)new BYTE[size];

    if (!m_pWaveFileFormat)
      return E_OUTOFMEMORY;

    memcpy(m_pWaveFileFormat, pwf, size);

    if (m_pSoundTouch && (pwf->nChannels <= 2))
    {
      m_pSoundTouch->setSampleRate(pwf->nSamplesPerSec);
      m_pSoundTouch->setChannels(pwf->nChannels);
      m_pSoundTouch->setTempoChange(0);
      m_pSoundTouch->setPitchSemiTones(0);

      // settings from Reclock - watch CPU usage when enabling these!
      /*bool usequickseek = false;
      bool useaafilter = false; //seems clearer without it
      int aafiltertaps = 56; //Def=32 doesnt matter coz its not used
      int seqms = 120; //reclock original is 82
      int seekwinms = 28; //reclock original is 28
      int overlapms = seekwinms; //reduces cutting sound if this is large
      int seqmslfe = 180; //larger value seems to preserve low frequencies better
      int seekwinmslfe = 42; //as percentage of seqms
      int overlapmslfe = seekwinmslfe; //reduces cutting sound if this is large

      m_pSoundTouch->setSetting(SETTING_USE_QUICKSEEK, usequickseek);
      m_pSoundTouch->setSetting(SETTING_USE_AA_FILTER, useaafilter);
      m_pSoundTouch->setSetting(SETTING_AA_FILTER_LENGTH, aafiltertaps);
      m_pSoundTouch->setSetting(SETTING_SEQUENCE_MS, seqms); 
      m_pSoundTouch->setSetting(SETTING_SEEKWINDOW_MS, seekwinms);
      m_pSoundTouch->setSetting(SETTING_OVERLAP_MS, overlapms);
      */
    }
  }

  return CBaseRenderer::SetMediaType (pmt);
}

HRESULT CMpcAudioRenderer::CompleteConnect(IPin *pReceivePin)
{
  TRACE(_T("CMpcAudioRenderer::CompleteConnect"));
  
  HRESULT hr = S_OK;

  if (!m_bUseWASAPI && !m_pDS) return E_FAIL;

  if (SUCCEEDED(hr)) hr = CBaseRenderer::CompleteConnect(pReceivePin);
  if (SUCCEEDED(hr)) hr = InitCoopLevel();

  if (!m_bUseWASAPI)
  {
    if (SUCCEEDED(hr)) hr = CreateDSBuffer();
  }
  if (SUCCEEDED(hr)) TRACE(_T("CMpcAudioRenderer::CompleteConnect Success"));

  return hr;
}

STDMETHODIMP CMpcAudioRenderer::Run(REFERENCE_TIME tStart)
{
  TRACE(_T("CMpcAudioRenderer::Run"));

  CAutoLock cRendererLock(&m_InterfaceLock);
  
  HRESULT	hr;
  m_dwTimeStart = timeGetTime();

  if (m_State == State_Running) return NOERROR;

  if (m_bUseWASAPI)
  {
    hr = CheckAudioClient(m_pWaveFileFormat);
    if (FAILED(hr)) 
    {
      TRACE(_T("CMpcAudioRenderer::Run Error on check audio client"));
      return hr;
    }
    
    /*hr = m_pAudioClient->Start();
    if (FAILED (hr))
    {
      TRACE(_T("CMpcAudioRenderer::Run Start error"));
      return hr;
    }*/

    if(SUCCEEDED(m_pPosition->GetRate(&m_dRate)))
    {
      if (m_dRate < 1.0) // TODO should be !=
      {
        if (FAILED (hr)) return hr;
      }
      else if (m_pSoundTouch)
      {
        m_pSoundTouch->setRateChange((float)(m_dRate-1.0)*100);
      }
    }
  }
  else
  {
    if (m_pDSBuffer && m_pPosition && m_pWaveFileFormat && 
        SUCCEEDED(m_pPosition->GetRate(&m_dRate))) 
    {
      if (m_dRate < 1.0)
      {
        hr = m_pDSBuffer->SetFrequency((long)(m_pWaveFileFormat->nSamplesPerSec * m_dRate));
        if (FAILED (hr)) return hr;
      }
      else
      {
        hr = m_pDSBuffer->SetFrequency((long)m_pWaveFileFormat->nSamplesPerSec);
        if (m_pSoundTouch)
        {
          m_pSoundTouch->setRateChange((float)(m_dRate-1.0)*100);
        }
      }
    }
    ClearBuffer();
  }
  
  return CBaseRenderer::Run(tStart);
}

STDMETHODIMP CMpcAudioRenderer::Stop() 
{
  TRACE(_T("CMpcAudioRenderer::Stop"));

  CAutoLock cRendererLock(&m_InterfaceLock);
  
  if (m_pDSBuffer)
  {
    m_pDSBuffer->Stop();
  }
  
  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    m_pAudioClient->Stop();
    m_pAudioClient->Reset();
    m_bIsAudioClientStarted = false;
  }

  FILTER_STATE state; 
  GetState(10000, &state);

  if (state == State_Paused)
  {
    TRACE(_T("CMpcAudioRenderer::Stop - releasing WASAPI resources"));
    SAFE_RELEASE(m_pAudioClock);
    SAFE_RELEASE(m_pRenderClient);
    SAFE_RELEASE(m_pAudioClient);
    SAFE_RELEASE(m_pMMDevice);
  }

  return CBaseRenderer::Stop(); 
};


STDMETHODIMP CMpcAudioRenderer::Pause()
{
  TRACE(_T("CMpcAudioRenderer::Pause"));
  
  CAutoLock cRendererLock(&m_InterfaceLock);

  if (m_pDSBuffer)
  {
    m_pDSBuffer->Stop();
  }
  
  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    BYTE *pData = NULL;
    UINT32 bufferSize(0);
    HRESULT hr = S_OK;

    hr = m_pAudioClient->GetBufferSize(&bufferSize);
    if (SUCCEEDED(hr) && m_pRenderClient)
    {
      hr = m_pRenderClient->GetBuffer(bufferSize, &pData);
      if (SUCCEEDED(hr))
      {
        // Clear the WASAPI buffers so that no looping audio is
        // played when graph is in the paused state
        m_pRenderClient->ReleaseBuffer(bufferSize, AUDCLNT_BUFFERFLAGS_SILENT);
      }
    }
    else
    {
      TRACE(_T("CMpcAudioRenderer::Pause - m_pRenderClient not available!"));
    }

    m_pAudioClient->Stop();
  }
  
  m_bIsAudioClientStarted = false;
  m_bFirstAudioSample = true;

  return CBaseRenderer::Pause(); 
};


HRESULT CMpcAudioRenderer::GetReferenceClockInterface(REFIID riid, void **ppv)
{
  HRESULT hr = S_OK;

  if (m_pReferenceClock)
  {
    return m_pReferenceClock->NonDelegatingQueryInterface(riid, ppv);
  }

  m_pReferenceClock = new CBaseReferenceClock (NAME("Mpc Audio Clock"), NULL, &hr);
	
  if (!m_pReferenceClock)
    return E_OUTOFMEMORY;

  m_pReferenceClock->AddRef();

  hr = SetSyncSource(m_pReferenceClock);
  if (FAILED(hr)) 
  {
    SetSyncSource(NULL);
    return hr;
  }

  return GetReferenceClockInterface(riid, ppv);
}

HRESULT CMpcAudioRenderer::EndOfStream(void)
{
  if (m_pDSBuffer)
  {
    m_pDSBuffer->Stop();
  }

  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    m_pAudioClient->Stop();
  }

  m_bIsAudioClientStarted = false;

  return CBaseRenderer::EndOfStream();
}

#pragma region // ==== DirectSound

HRESULT CMpcAudioRenderer::CreateDSBuffer()
{
  if (! m_pWaveFileFormat) return E_POINTER;

  HRESULT hr = S_OK;
  LPDIRECTSOUNDBUFFER pDSBPrimary = NULL;
  DSBUFFERDESC dsbd;
  DSBUFFERDESC cDSBufferDesc;
  DSBCAPS bufferCaps;
  DWORD dwDSBufSize = m_pWaveFileFormat->nAvgBytesPerSec * 4;

  ZeroMemory(&bufferCaps, sizeof(bufferCaps));
  ZeroMemory(&dsbd, sizeof(DSBUFFERDESC));

  dsbd.dwSize = sizeof(DSBUFFERDESC);
  dsbd.dwFlags = DSBCAPS_PRIMARYBUFFER;
  dsbd.dwBufferBytes = 0;
  dsbd.lpwfxFormat = NULL;
  if (SUCCEEDED (hr = m_pDS->CreateSoundBuffer( &dsbd, &pDSBPrimary, NULL )))
  {
    hr = pDSBPrimary->SetFormat(m_pWaveFileFormat);
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
  cDSBufferDesc.lpwfxFormat = m_pWaveFileFormat; 
  cDSBufferDesc.guid3DAlgorithm	= GUID_NULL; 

  hr = m_pDS->CreateSoundBuffer (&cDSBufferDesc,  &m_pDSBuffer, NULL);

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
    m_pDSBuffer->SetFrequency((long)(m_pWaveFileFormat->nSamplesPerSec * m_dRate));
  }

  return hr;
}

HRESULT CMpcAudioRenderer::ClearBuffer()
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

HRESULT CMpcAudioRenderer::InitCoopLevel()
{
  HRESULT hr = S_OK;
  IVideoWindow* pVideoWindow	= NULL;
  HWND hWnd = NULL;
  CComBSTR bstrCaption;

  hr = m_pGraph->QueryInterface(__uuidof(IVideoWindow), (void**)&pVideoWindow);
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
  if (!m_bUseWASAPI)
  {
    hr = m_pDS->SetCooperativeLevel(hWnd, DSSCL_PRIORITY);
  }
  else if (!m_hTask)
  {
    // Ask MMCSS to temporarily boost the thread priority
    // to reduce glitches while the low-latency stream plays.
    DWORD taskIndex = 0;

    if (pfAvSetMmThreadCharacteristicsW)
    {
      m_hTask = pfAvSetMmThreadCharacteristicsW(L"Pro Audio", &taskIndex);
      TRACE(_T("CMpcAudioRenderer::InitCoopLevel Putting thread in higher priority for Wasapi mode (lowest latency)"));
      hr = GetLastError();
      if (!m_hTask)
      {
        return hr;
      }
    }
  }

	return hr;
}

HRESULT	CMpcAudioRenderer::DoRenderSampleDirectSound(IMediaSample *pMediaSample)
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

HRESULT CMpcAudioRenderer::WriteSampleToDSBuffer(IMediaSample *pMediaSample, bool *looped)
{
  if (!m_pDSBuffer) return E_POINTER;

  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtStop = 0;
  
  HRESULT hr = S_OK;
  bool loop = false;
  
  VOID* pDSLockedBuffers[2] = {NULL, NULL};
  DWORD dwDSLockedSize[2]	= {0, 0};
  BYTE* pMediaBuffer = NULL;

  long lSize = pMediaSample->GetActualDataLength();

  hr = pMediaSample->GetPointer(&pMediaBuffer);

  // resample audio stream if required
  if (m_bUseTimeStretching)
  {
    CAutoLock cAutoLock(&m_csResampleLock);
	
    int nBytePerSample = m_pWaveFileFormat->nChannels * (m_pWaveFileFormat->wBitsPerSample/8);
    m_pSoundTouch->putSamples((const short*)pMediaBuffer, lSize / nBytePerSample);
    lSize = m_pSoundTouch->receiveSamples((short*)pMediaBuffer, lSize / nBytePerSample) * nBytePerSample;
  }

  pMediaSample->GetTime(&rtStart, &rtStop);

  if (rtStart < 0)
  {
    DWORD dwPercent	= (DWORD) ((-rtStart * 100) / (rtStop - rtStart));
    DWORD dwRemove= (lSize * dwPercent/100);

    dwRemove = (dwRemove / m_pWaveFileFormat->nBlockAlign) * m_pWaveFileFormat->nBlockAlign;
    pMediaBuffer += dwRemove;
    lSize -= dwRemove;
  }

  if (SUCCEEDED (hr))
  hr = m_pDSBuffer->Lock(m_dwDSWriteOff, lSize, &pDSLockedBuffers[0], &dwDSLockedSize[0], &pDSLockedBuffers[1], &dwDSLockedSize[1], 0 );

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
    ATLASSERT (dwDSLockedSize [0] + dwDSLockedSize [1] == (DWORD)lSize);
  }

  if (SUCCEEDED(hr) && looped) *looped = loop;

  return hr;
}

#pragma endregion

#pragma region // ==== WASAPI

HRESULT	CMpcAudioRenderer::DoRenderSampleWasapi(IMediaSample *pMediaSample)
{
  HRESULT	hr = S_OK;
  
  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtStop = 0;
  
  DWORD flags = 0;
  BYTE *pMediaBuffer = NULL;
  
  // TODO: needs to be converted to use static buffer and loops to 
  BYTE *mediaBufferResult = NULL; 

  BYTE *pInputBufferPointer = NULL;
  BYTE *pInputBufferEnd = NULL;
  BYTE *pData;

  m_nBufferSize = pMediaSample->GetActualDataLength();
  long lSize = m_nBufferSize;
  long lResampledSize = 0;

  pMediaSample->GetTime (&rtStart, &rtStop);
  
  AM_MEDIA_TYPE *pmt;
  if (SUCCEEDED(pMediaSample->GetMediaType(&pmt)) && pmt != NULL)
  {
    CMediaType mt(*pmt);
    if ((WAVEFORMATEXTENSIBLE*)mt.Format() != NULL)
    {
      hr = CheckAudioClient(&(((WAVEFORMATEXTENSIBLE*)mt.Format())->Format));
    }      
    else
    {
      hr = CheckAudioClient((WAVEFORMATEX*)mt.Format());
    }
    if (FAILED(hr))
    {
      TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi Error while checking audio client with input media type"));
      return hr;
    }
    DeleteMediaType(pmt);
    pmt = NULL;
  }

  // Initialization
  hr = pMediaSample->GetPointer(&pMediaBuffer);
  if (FAILED (hr)) return hr; 

  DWORD start = timeGetTime();
  DWORD step1 = 0;
  DWORD step2 = 0;

  // resample audio stream if required
  if (m_bUseTimeStretching)
  {
    CAutoLock cAutoLock(&m_csResampleLock);
	
    //TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi - resampling: m_nBufferSize: %d lSize: %d"), m_nBufferSize, lSize);
    int nBytePerSample = m_pWaveFileFormat->nChannels * (m_pWaveFileFormat->wBitsPerSample/8);
    
    step1 = timeGetTime();
    
    m_pSoundTouch->putSamples((const short*)pMediaBuffer, lSize / nBytePerSample);

    step2 = timeGetTime();

    //lResampledSize = m_pSoundTouch->receiveSamples((short*)pMediaBuffer, m_pSoundTouch->numSamples() / nBytePerSample) * nBytePerSample;
    
    mediaBufferResult = (BYTE*)malloc(m_pSoundTouch->numSamples());
    lResampledSize = m_pSoundTouch->receiveSamples((short*)mediaBufferResult, m_pSoundTouch->numSamples() / nBytePerSample) * nBytePerSample;

    if (lResampledSize != lSize)
    {
      TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi - lResampledSize: %d lSize: %d"), lResampledSize, lSize);
    }
  }

  DWORD end = timeGetTime();

  UINT64 whole = (end - start);
  UINT64 dur1 = (step1 - start);
  UINT64 dur2 = (step2 - step1);
  UINT64 dur3 = (end - step2);

  TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi - processing time %I64u %I64u %I64u %I64u "), whole, dur1, dur2, dur3);

  #ifndef DEBUG
  //CString OutMsg;
  //OutMsg.Format("CMpcAudioRenderer::DoRenderSampleWasapi - processing time %I64u %I64u %I64u %I64u ", whole, dur1, dur2, dur3);
  //OutputDebugString(OutMsg);
  #endif

  //pInputBufferPointer = &pMediaBuffer[0];
  //pInputBufferEnd = &pMediaBuffer[0] + lResampledSize;

  pInputBufferPointer = &mediaBufferResult[0];
  pInputBufferEnd = &mediaBufferResult[0] + lResampledSize;

  WORD frameSize = m_pWaveFileFormat->nBlockAlign;

  // Sleep for half the buffer duration since last buffer feed
  DWORD currentTime = GetTickCount();
  if (m_dwLastBufferTime != 0 && m_hnsActualDuration != 0 && m_dwLastBufferTime < currentTime && 
    (currentTime - m_dwLastBufferTime) < m_hnsActualDuration)
  {
    m_hnsActualDuration = m_hnsActualDuration - (currentTime - m_dwLastBufferTime);
    Sleep(m_hnsActualDuration);
  }

  // Each loop fills one of the two buffers.
  while (pInputBufferPointer < pInputBufferEnd)
  {  
    UINT32 numFramesPadding = 0;
    m_pAudioClient->GetCurrentPadding(&numFramesPadding);
    UINT32 numFramesAvailable = m_nFramesInBuffer - numFramesPadding;

    UINT32 nAvailableBytes = numFramesAvailable * frameSize;
    UINT32 nBytesToWrite = nAvailableBytes;
    // More room than enough in the output buffer
    if (nAvailableBytes > pInputBufferEnd - pInputBufferPointer)
    {
      nBytesToWrite = pInputBufferEnd - pInputBufferPointer;
      numFramesAvailable = (UINT32)((float)nBytesToWrite / frameSize);
    }

    // Grab the next empty buffer from the audio device.
    hr = m_pRenderClient->GetBuffer(numFramesAvailable, &pData);
    if (FAILED (hr))
    {
      TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi GetBuffer failed with size %ld : (error %lx)"),m_nFramesInBuffer,hr);
      delete mediaBufferResult;
      return hr;
    }

    // Load the buffer with data from the audio source.
    if (pData)
    {
      memcpy(&pData[0], pInputBufferPointer, nBytesToWrite);
      pInputBufferPointer += nBytesToWrite;
    }
    else
    {
      TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi Output buffer is NULL"));
    }

    hr = m_pRenderClient->ReleaseBuffer(numFramesAvailable, 0); // no flags
    if (FAILED (hr)) 
    {
      TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi ReleaseBuffer failed with size %ld (error %lx)"),m_nFramesInBuffer,hr);
      delete mediaBufferResult;
      return hr;
    }

    if (!m_bIsAudioClientStarted)
    {
      TRACE(_T("CMpcAudioRenderer::DoRenderSampleWasapi Starting audio client"));
      m_pAudioClient->Start();
      m_bIsAudioClientStarted = true;
    }

    if (pInputBufferPointer >= pInputBufferEnd)
    {
      m_dwLastBufferTime = GetTickCount();
      
      // This is the duration of the filled buffer
      m_hnsActualDuration = (double)REFTIMES_PER_SEC * numFramesAvailable / m_pWaveFileFormat->nSamplesPerSec;
      
      // Sleep time is half this duration
      m_hnsActualDuration = (DWORD)(m_hnsActualDuration / REFTIMES_PER_MILLISEC / 2);
      break;
    }

    // Buffer not completely filled, sleep for half buffer capacity duration
    m_hnsActualDuration = (double)REFTIMES_PER_SEC * m_nFramesInBuffer / m_pWaveFileFormat->nSamplesPerSec;
    
    // Sleep time is half this duration
    m_hnsActualDuration = (DWORD)(m_hnsActualDuration / REFTIMES_PER_MILLISEC / 2);
    Sleep(m_hnsActualDuration);
  }
  
  delete mediaBufferResult;
  return hr;
}

HRESULT CMpcAudioRenderer::CheckAudioClient(WAVEFORMATEX *pWaveFormatEx)
{
  TRACE(_T("CMpcAudioRenderer::CheckAudioClient"));

  HRESULT hr = S_OK;
  CAutoLock cAutoLock(&m_csCheck);
  
  if (!m_pMMDevice) 
    hr = GetDefaultAudioDevice(&m_pMMDevice);

  // If no WAVEFORMATEX structure provided and client already exists, return it
  if (m_pAudioClient && !pWaveFormatEx) 
    return hr;

  // Just create the audio client if no WAVEFORMATEX provided
  if (!m_pAudioClient && !pWaveFormatEx)
  {
    if (SUCCEEDED (hr)) hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
      return hr;
  }

  // Compare the exisiting WAVEFORMATEX with the one provided
  WAVEFORMATEX *pNewWaveFormatEx = NULL;
  if (CheckFormatChanged(pWaveFormatEx, &pNewWaveFormatEx))
  {
    // Format has changed, audio client has to be reinitialized
    TRACE(_T("CMpcAudioRenderer::CheckAudioClient Format changed, reinitialize the audio client"));
    if (m_pWaveFileFormat)
    {
      BYTE *p = (BYTE *)m_pWaveFileFormat;
      SAFE_DELETE_ARRAY(p);
    }
  
    m_pWaveFileFormat = pNewWaveFormatEx;
    hr = m_pAudioClient->IsFormatSupported(AUDCLNT_SHAREMODE_EXCLUSIVE, pWaveFormatEx, NULL);
  
    if (SUCCEEDED(hr))
    { 
      if (m_pAudioClient && m_bIsAudioClientStarted) 
        m_pAudioClient->Stop();
      
      m_bIsAudioClientStarted = false;
      SAFE_RELEASE(m_pRenderClient);
      SAFE_RELEASE(m_pAudioClock);
      SAFE_RELEASE(m_pAudioClient);
      
      if (SUCCEEDED (hr)) 
        hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
    }
    else
    {
      TRACE(_T("CMpcAudioRenderer::CheckAudioClient New format not supported, accept it anyway"));
      return S_OK;
    }
  }
  else if (!m_pRenderClient)
  {
    TRACE(_T("CMpcAudioRenderer::CheckAudioClient First initialization of the audio renderer"));
  }
  else
  {
    return hr;  
  }

  SAFE_RELEASE(m_pRenderClient);

  if (SUCCEEDED (hr)) 
  {
    hr = InitAudioClient(pWaveFormatEx, m_pAudioClient, &m_pRenderClient);
  }
  return hr;
}

/* 
 Retrieves the default audio device from the Core Audio API
 To be used for WASAPI mode
 
 TODO : choose a device in the renderer configuration dialogs
*/
HRESULT CMpcAudioRenderer::GetDefaultAudioDevice(IMMDevice **pm_pMMDevice)
{
  HRESULT hr;
  CComPtr<IMMDeviceEnumerator> enumerator;
  TRACE(_T("CMpcAudioRenderer::GetDefaultAudioDevice"));

  hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));
  hr = enumerator->GetDefaultAudioEndpoint(eRender, eConsole, pm_pMMDevice);

  return hr;
}

bool CMpcAudioRenderer::CheckFormatChanged(WAVEFORMATEX *pWaveFormatEx, WAVEFORMATEX **ppNewWaveFormatEx)
{
  bool formatChanged = false;

  if (!m_pWaveFileFormat)
  {
    formatChanged = true;
  }
  else if (pWaveFormatEx->wFormatTag != m_pWaveFileFormat->wFormatTag ||
           pWaveFormatEx->nChannels != m_pWaveFileFormat->nChannels ||
           pWaveFormatEx->wBitsPerSample != m_pWaveFileFormat->wBitsPerSample) // TODO : improve the checks
  {
    formatChanged = true;
  }

  if (!formatChanged)
    return false;

  int size = sizeof(WAVEFORMATEX) + pWaveFormatEx->cbSize; // Always true, even for WAVEFORMATEXTENSIBLE and WAVEFORMATEXTENSIBLE_IEC61937
  *ppNewWaveFormatEx = (WAVEFORMATEX *)new BYTE[size];
  
  if (!*ppNewWaveFormatEx)
    return false;
  
  memcpy(*ppNewWaveFormatEx, pWaveFormatEx, size);
  
  return true;
}

HRESULT CMpcAudioRenderer::GetBufferSize(WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod)
{ 
  if (!pWaveFormatEx) 
    return S_OK;

  if (pWaveFormatEx->cbSize <22) //WAVEFORMATEX
    return S_OK;

  WAVEFORMATEXTENSIBLE *wfext = (WAVEFORMATEXTENSIBLE*)pWaveFormatEx;

  if (m_nBufferSize == 0 )
  if (wfext->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_MLP)
    m_nBufferSize = 61440;
  else if (wfext->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DTS_HD)
    m_nBufferSize = 32768;
  else if (wfext->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL_PLUS)
    m_nBufferSize = 24576;
  else if (wfext->Format.wFormatTag == WAVE_FORMAT_DOLBY_AC3_SPDIF)
    m_nBufferSize = 6144;
  else return S_OK;

  *pHnsBufferPeriod = (REFERENCE_TIME)((REFERENCE_TIME)m_nBufferSize * 10000 * 8 / ((REFERENCE_TIME)pWaveFormatEx->nChannels * pWaveFormatEx->wBitsPerSample *
    1.0 * pWaveFormatEx->nSamplesPerSec) /*+ 0.5*/);
    *pHnsBufferPeriod *= 1000;

  TRACE(_T("CMpcAudioRenderer::GetBufferSize set a %lld period for a %ld buffer size"),*pHnsBufferPeriod, m_nBufferSize);

  return S_OK;
}

HRESULT CMpcAudioRenderer::InitAudioClient(WAVEFORMATEX *pWaveFormatEx, IAudioClient *pAudioClient, IAudioRenderClient **ppRenderClient)
{
  TRACE(_T("CMpcAudioRenderer::InitAudioClient"));
  HRESULT hr = S_OK;
  
  // Initialize the stream to play at the minimum latency.
  //if (SUCCEEDED (hr)) hr = m_pAudioClient->GetDevicePeriod(NULL, &m_hnsPeriod);
  m_hnsPeriod = 500000; //50 ms is the best according to James @Slysoft

  hr = m_pAudioClient->IsFormatSupported(AUDCLNT_SHAREMODE_EXCLUSIVE, pWaveFormatEx, NULL);
  if (FAILED(hr))
  {
    TRACE(_T("CMpcAudioRenderer::InitAudioClient not supported (0x%08x)"), hr);
  }
  else
  {
    TRACE(_T("CMpcAudioRenderer::InitAudioClient format supported"));
  }

  GetBufferSize(pWaveFormatEx, &m_hnsPeriod);

  if (SUCCEEDED (hr))
  {
    hr = m_pAudioClient->Initialize(AUDCLNT_SHAREMODE_EXCLUSIVE,0/*AUDCLNT_STREAMFLAGS_EVENTCALLBACK*/, m_hnsPeriod,m_hnsPeriod,pWaveFormatEx,NULL);
  }
    
  if (FAILED (hr) && hr != AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED)
  {
    TRACE(_T("CMpcAudioRenderer::InitAudioClient failed (0x%08x)"), hr);
    return hr;
  }
  
  if (hr == S_OK)
  {
    SAFE_RELEASE(m_pAudioClock);
    hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
    if(FAILED(hr))
    {
      TRACE(_T("CMpcAudioRenderer - IAudioClock not found"));
    }
    else
    {
      m_pAudioClock->GetFrequency(&m_nHWfreq);
    }
  }

  if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) 
  {
    // if the buffer size was not aligned, need to do the alignment dance
    TRACE(_T("CMpcAudioRenderer::InitAudioClient Buffer size not aligned. Realigning"));

    // get the buffer size, which will be aligned
    hr = m_pAudioClient->GetBufferSize(&m_nFramesInBuffer);

    // throw away this IAudioClient
    SAFE_RELEASE(m_pAudioClient);

    // calculate the new aligned periodicity
    m_hnsPeriod = // hns =
                  (REFERENCE_TIME)(
                  10000.0 * // (hns / ms) *
                  1000 * // (ms / s) *
                  m_nFramesInBuffer / // frames /
                  pWaveFormatEx->nSamplesPerSec  // (frames / s)
                  + 0.5 // rounding
    );

    if (SUCCEEDED(hr)) 
    {
      hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
    }
      
    TRACE(_T("CMpcAudioRenderer::InitAudioClient Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames.\n"), m_hnsPeriod, m_nFramesInBuffer);

    if (SUCCEEDED (hr)) 
    {
      hr = m_pAudioClient->Initialize(AUDCLNT_SHAREMODE_EXCLUSIVE,0/*AUDCLNT_STREAMFLAGS_EVENTCALLBACK*/, m_hnsPeriod, m_hnsPeriod, pWaveFormatEx, NULL);
    }
 
    if (FAILED(hr))
    {
      TRACE(_T("CMpcAudioRenderer::InitAudioClient Failed to reinitialize the audio client"));
      return hr;
    }
    else
    {
      SAFE_RELEASE(m_pAudioClock);
      hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
      if(FAILED(hr))
      {
        TRACE(_T("CMpcAudioRenderer - IAudioClock not found"));
      }
      else
      {
        m_pAudioClock->GetFrequency(&m_nHWfreq);
      }
    }
  } // if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) 

  // get the buffer size, which is aligned
  if (SUCCEEDED (hr)) 
  {
    hr = m_pAudioClient->GetBufferSize(&m_nFramesInBuffer);
  }

  // calculate the new period
  if (SUCCEEDED (hr)) 
  {
    hr = m_pAudioClient->GetService(__uuidof(IAudioRenderClient), (void**)(ppRenderClient));
  }

  if (FAILED(hr))
  {
    TRACE(_T("CMpcAudioRenderer::InitAudioClient service initialization failed (0x%08x)"), hr);
  }
  else
  {
    TRACE(_T("CMpcAudioRenderer::InitAudioClient service initialization success"));
  }

  return hr;
}

HRESULT CMpcAudioRenderer::CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient)
{
  HRESULT hr = S_OK;
  m_hnsPeriod = 0;

  TRACE(_T("CMpcAudioRenderer::CreateAudioClient"));

  if (*ppAudioClient)
  {
    if (m_bIsAudioClientStarted)
    {
      (*ppAudioClient)->Stop();
    }

    SAFE_RELEASE(*ppAudioClient);
    m_bIsAudioClientStarted = false;
  }

  if (!pMMDevice)
  {
    TRACE(_T("CMpcAudioRenderer::CreateAudioClient failed, device not loaded"));
    return E_FAIL;
  }

  hr = pMMDevice->Activate(__uuidof(IAudioClient), CLSCTX_ALL, NULL, reinterpret_cast<void**>(ppAudioClient));
  if (FAILED(hr))
  {
    TRACE(_T("CMpcAudioRenderer::CreateAudioClient activation failed (0x%08x)"), hr);
  }
  else
  {
    TRACE(_T("CMpcAudioRenderer::CreateAudioClient success"));
  }

  return hr;
}

HRESULT CMpcAudioRenderer::BeginFlush()
{
  // Make sure DShow audio buffers are empty when seeking occurs
  if (m_pDSBuffer) 
  {
    m_pDSBuffer->Stop();
  }
  
  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    HRESULT hr = S_OK;
    
    m_pAudioClient->Stop();
    hr = m_pAudioClient->Reset();
    m_bIsAudioClientStarted = false;

    if (hr != S_OK)
    {
      TRACE(_T("CMpcAudioRenderer::BeginFlush - m_pAudioClient reset failed with %d"), hr);
    }
  }
  
  if (m_pSoundTouch)
  {
    m_pSoundTouch->flush();
  }

  return CBaseRenderer::BeginFlush(); 
}

HRESULT CMpcAudioRenderer::EndFlush()
{
  m_bFirstAudioSample = true;
  return CBaseRenderer::EndFlush(); 
}


// IMediaSeeking interface implementation

STDMETHODIMP CMpcAudioRenderer::IsFormatSupported(const GUID* pFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::QueryPreferredFormat(GUID* pFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::SetTimeFormat(const GUID* pFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::IsUsingTimeFormat(const GUID* pFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetTimeFormat(GUID* pFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetDuration(LONGLONG* pDuration)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetStopPosition(LONGLONG* pStop)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetCurrentPosition(LONGLONG* pCurrent)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetCapabilities(DWORD* pCapabilities)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::CheckCapabilities(DWORD* pCapabilities)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG * pStop, DWORD StopFlags)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetPositions(LONGLONG* pCurrent, LONGLONG* pStop)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::SetRate(double dRate)
{
  m_dRate = dRate;
  return S_OK;
}

STDMETHODIMP CMpcAudioRenderer::GetRate(double* pdRate)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMpcAudioRenderer::GetPreroll(LONGLONG *pPreroll)
{
  return E_NOTIMPL;
}

// IAVSyncClock interface implementation

HRESULT CMpcAudioRenderer::AdjustClock(DOUBLE pAdjustment)
{
  CAutoLock cAutoLock(&m_csResampleLock);

  m_dAdjustment = pAdjustment;
  m_Clock.SetAdjustment(m_dAdjustment);
  if (m_pSoundTouch)
  {
    m_pSoundTouch->setTempo(m_dAdjustment * m_dBias);
  }
  return S_OK;
}

HRESULT CMpcAudioRenderer::SetBias(DOUBLE pBias)
{
  CAutoLock cAutoLock(&m_csResampleLock);

  m_dBias = pBias;
  m_Clock.SetBias(m_dBias);
  if (m_pSoundTouch)
  {
    m_pSoundTouch->setTempo(m_dAdjustment * m_dBias);
  }
  return S_OK;
}

HRESULT CMpcAudioRenderer::GetBias(DOUBLE* pBias)
{
  *pBias = m_Clock.Bias();
  return S_OK;
}

