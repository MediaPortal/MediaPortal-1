// Copyright (C) 2005-2010 Team MediaPortal
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
#include "WASAPIRenderer.h"
#include "TimeSource.h"

#include "alloctracing.h"

#include <FunctionDiscoveryKeys_devpkey.h>

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

WASAPIRenderer::WASAPIRenderer(CMPAudioRenderer* pRenderer, HRESULT *phr) : 
  m_pMMDevice(NULL),
  m_pAudioClient(NULL),
  m_pRenderClient(NULL),
  m_nFramesInBuffer(0),
  m_hTask(NULL),
  m_nBufferSize(0),
  m_bIsAudioClientStarted(false),
  m_bReinitAfterStop(false),
  m_bDiscardCurrentSample(false),
  m_pRenderer(pRenderer),
  m_dRate(1.0),
  m_threadId(0),
  m_hRenderThread(NULL),
  m_bThreadPaused(false),
  m_hDataEvent(NULL),
  m_hPauseEvent(NULL),
  m_hWaitPauseEvent(NULL),
  m_hResumeEvent(NULL),
  m_hWaitResumeEvent(NULL),
  m_hStopRenderThreadEvent(NULL),
  m_pAudioClock(NULL),
  m_nHWfreq(0),
  m_pRenderFormat(NULL),
  m_StreamFlags(AUDCLNT_STREAMFLAGS_EVENTCALLBACK)
{
  ResetClockData();

  IMMDeviceCollection* devices = NULL;
  GetAvailableAudioDevices(&devices, true);
  SAFE_RELEASE(devices); // currently only log available devices

  if (pRenderer->Settings()->m_bWASAPIUseEventMode)
  {
    // Using HW DMA buffer based event notification
    m_hDataEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
    m_StreamFlags = AUDCLNT_STREAMFLAGS_EVENTCALLBACK;
  }
  else
  {
    // Using rendering thread polling
    m_hDataEvent = CreateWaitableTimer(NULL, TRUE, NULL);
    m_StreamFlags = 0;
  }

  m_hPauseEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  m_hWaitPauseEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  m_hResumeEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  m_hWaitResumeEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  m_hStopRenderThreadEvent = CreateEvent(0, FALSE, FALSE, 0);
  
  OSVERSIONINFO osvi;
  ZeroMemory(&osvi, sizeof(OSVERSIONINFO));
  osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);

  GetVersionEx(&osvi);
  bool bWASAPIAvailable = osvi.dwMajorVersion > 5;

  if (!bWASAPIAvailable)
    Log("Disabling WASAPI - OS version earlier than Vista detected");

  HMODULE hLib = NULL;

  // Load Vista specifics DLLs
  hLib = LoadLibrary ("AVRT.dll");
  if (hLib && bWASAPIAvailable)
  {
    pfAvSetMmThreadCharacteristicsW   = (PTR_AvSetMmThreadCharacteristicsW)	GetProcAddress (hLib, "AvSetMmThreadCharacteristicsW");
    pfAvRevertMmThreadCharacteristics	= (PTR_AvRevertMmThreadCharacteristics)	GetProcAddress (hLib, "AvRevertMmThreadCharacteristics");
  }
  else
  {
    pRenderer->Settings()->m_bUseWASAPI = false;	// WASAPI not available below Vista
  }
}

WASAPIRenderer::~WASAPIRenderer()
{
  Log("WASAPIRenderer - destructor - instance 0x%x", this);
  
  CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());
  CAutoLock cInterfaceLock(m_pRenderer->InterfaceLock());

  SAFE_RELEASE(m_pAudioClock);
  SAFE_RELEASE(m_pRenderClient);
  SAFE_RELEASE(m_pAudioClient);
  SAFE_RELEASE(m_pMMDevice);

  if (m_hStopRenderThreadEvent)
    CloseHandle(m_hStopRenderThreadEvent);
  if (m_hDataEvent)
    CloseHandle(m_hDataEvent);
  if (m_hPauseEvent)
    CloseHandle(m_hPauseEvent);
  if (m_hWaitPauseEvent)
    CloseHandle(m_hWaitPauseEvent);
  if (m_hResumeEvent)
    CloseHandle(m_hResumeEvent);
  if (m_hWaitResumeEvent)
    CloseHandle(m_hWaitResumeEvent);

  SAFE_DELETE_WAVEFORMATEX(m_pRenderFormat);

  Log("WASAPIRenderer - destructor - instance 0x%x - end", this);
}

HRESULT WASAPIRenderer::StopRendererThread()
{
  Log("WASAPIRenderer::StopRendererThread");
  // Get rid of the render thread
  if (m_hRenderThread)
  {
    SetEvent(m_hStopRenderThreadEvent);
    WaitForSingleObject(m_hRenderThread, INFINITE);

    CloseHandle(m_hRenderThread);
    m_hRenderThread = NULL;
  }

  return S_OK;
}

void WASAPIRenderer::CancelDataEvent()
{
  if (!m_pRenderer->Settings()->m_bWASAPIUseEventMode)
  {
    HRESULT hr = CancelWaitableTimer(m_hDataEvent);
    if (FAILED(hr))
      Log("WASAPIRenderer::CancelDataEvent - CancelWaitableTimer failed: (0x%08x)", hr);
  }
}

HRESULT WASAPIRenderer::PauseRendererThread()
{
  if (m_bThreadPaused)
    return S_OK;

  Log("WASAPIRenderer::PauseRendererThread");
  // Pause the render thread
  if (m_hRenderThread)
  {
    CancelDataEvent();

    SetEvent(m_hPauseEvent);
    DWORD result = WaitForSingleObject(m_hWaitPauseEvent, INFINITE);
    if (result != 0)
    {
      DWORD error = GetLastError();
      Log("   error: %d", error);
    }
  }
  else
  {
    Log("   No thread was created!");
    return S_FALSE;
  }

  return S_OK;
}

HRESULT WASAPIRenderer::StartRendererThread()
{
  if (!m_hRenderThread)
  {
    Log("WASAPIRenderer::StartRendererThread"); 
    m_hRenderThread = CreateThread(0, 0, WASAPIRenderer::RenderThreadEntryPoint, (LPVOID)this, 0, &m_threadId);

    if (m_hRenderThread)
      return S_OK;
    else
      return S_FALSE;
  }
  else
  {
    if (m_bThreadPaused)
    {
      Log("WASAPIRenderer::StartRendererThread - resuming");
      SetEvent(m_hResumeEvent);
      WaitForSingleObject(m_hWaitResumeEvent, INFINITE);
    }
  }

  return S_OK;
}

void WASAPIRenderer::OnReceiveFirstSample(IMediaSample* /*pMediaSample*/)
{
}

HRESULT WASAPIRenderer::CheckFormat(WAVEFORMATEX* pwfx)
{
  HRESULT hr = CheckAudioClient(pwfx);
  if (FAILED(hr))
  {
    Log("WASAPIRenderer::CheckMediaType Error on check audio client");
    return hr;
  }
  if (!m_pAudioClient) 
  {
    Log("WASAPIRenderer::CheckMediaType Error, audio client not loaded");
    return VFW_E_CANNOT_CONNECT;
  }
  
  WAVEFORMATEX *pwfxCM = NULL;
  hr = m_pAudioClient->IsFormatSupported(m_pRenderer->Settings()->m_WASAPIShareMode, pwfx, &pwfxCM);
  if (hr != S_OK)
  {
    //Log("WASAPIRenderer::CheckMediaType WASAPI client refused the format: (0x%08x)", hr);
    //Log("   test with WAVEFORMATEX");
    
    WAVEFORMATEX* tmpPwfx = NULL; 
    CopyWaveFormatEx(&tmpPwfx, pwfx);
    tmpPwfx->cbSize = 0;

    hr = m_pAudioClient->IsFormatSupported(m_pRenderer->Settings()->m_WASAPIShareMode, tmpPwfx, &pwfxCM);
    if (hr != S_OK)
    {
      Log("WASAPIRenderer::CheckMediaType WASAPI client refused the format: (0x%08x)", hr);
      LogWaveFormat(pwfxCM, "Closest match would be" );
      SAFE_DELETE_WAVEFORMATEX(tmpPwfx);
      CoTaskMemFree(pwfxCM);
      return VFW_E_TYPE_NOT_ACCEPTED;
    }

    // truncate the WAVEFORMATEXTENSIBLE part since driver is more happy with the WAVEFORMATEX
    pwfx->cbSize = 0;

    SAFE_DELETE_WAVEFORMATEX(tmpPwfx);
  }
  Log("WASAPIRenderer::CheckMediaType WASAPI client accepted the format");

  return S_OK;
}

HRESULT WASAPIRenderer::SetMediaType(WAVEFORMATEX* pwfx)
{
  // New media type set but render client already initialized => reset it
  if (m_pRenderClient)
  {
    Log("WASAPIRenderer::SetMediaType Render client already initialized. Reinitialization...");
    CheckAudioClient(pwfx);
    return S_OK;
  }
  SAFE_DELETE_WAVEFORMATEX(m_pRenderFormat);
  CopyWaveFormatEx(&m_pRenderFormat, pwfx);
  return S_OK;
}

HRESULT WASAPIRenderer::CompleteConnect(IPin *pReceivePin)
{
  return S_OK;
}

HRESULT WASAPIRenderer::EndOfStream()
{
  Log("WASAPIRenderer::EndOfStream");
  return S_OK;
}

HRESULT WASAPIRenderer::BeginFlush()
{
  Log("WASAPIRenderer::BeginFlush");

  m_bDiscardCurrentSample = true;
  ResetClockData();

  StopAudioClient(&m_pAudioClient);
  return S_OK;
}

HRESULT WASAPIRenderer::EndFlush()
{
  Log("WASAPIRenderer::EndFlush");
  return S_OK;
}
HRESULT WASAPIRenderer::Run(REFERENCE_TIME tStart)
{
  Log("WASAPIRenderer::Run");

  HRESULT hr = 0;

  ResetClockData();

  hr = CheckAudioClient(m_pRenderFormat);
  if (FAILED(hr)) 
  {
    Log("WASAPIRenderer::Run - error on check audio client (0x%08x)", hr);
    return hr;
  }

  // this is required for the .NET GC workaround
  if (m_bReinitAfterStop)
  {
    m_bReinitAfterStop = false;
    hr = InitAudioClient(m_pRenderFormat, m_pAudioClient, &m_pRenderClient);
    if (FAILED(hr)) 
    {
      Log("WASAPIRenderer::Run - error on reinit after stop (0x%08x) - trying to continue", hr);
      //return hr;
    }
  }

  return hr;
}


HRESULT WASAPIRenderer::Stop(FILTER_STATE pState)
{
  StopAudioClient(&m_pAudioClient);

  // This is an ugly workaround for the .NET GC not cleaning up the directshow resources
  // when playback is stopped. Needs to be done since otherwise the next session might
  // fail if the old one is still alive and it is using WASAPI exclusive mode
  if (pState == State_Paused)
  {
    Log("WASAPIRenderer::Stop - releasing WASAPI resources");
    SAFE_RELEASE(m_pAudioClock);
    SAFE_RELEASE(m_pRenderClient);
    SAFE_RELEASE(m_pAudioClient);
    SAFE_RELEASE(m_pMMDevice);

    m_bReinitAfterStop = true;
  }
  
  return S_OK;
}

HRESULT WASAPIRenderer::Pause(FILTER_STATE pState)
{
  // TODO: check if this could be fixed without requiring to drop all sample
  if (pState == State_Running)
  {
    m_bDiscardCurrentSample = true;
  }

  StopAudioClient(&m_pAudioClient);
  return S_OK;
}

HRESULT WASAPIRenderer::AudioClock(ULONGLONG& pTimestamp, ULONGLONG& pQpc)
{
  CAutoLock cAutoLock(&m_csClockLock);

  if (m_dClockPosIn == m_dClockPosOut || m_dClockDataCollectionCount < CLOCK_DATA_SIZE)
    return S_FALSE;

  //Log("m_dClockPosIn: %d m_dClockPosOut: %d diff: %I64u",m_dClockPosIn, m_dClockPosOut, m_ullHwClock[m_dClockPosIn] - m_ullHwClock[m_dClockPosOut] );

  UINT64 clock = m_ullHwClock[m_dClockPosIn] - m_ullHwClock[m_dClockPosOut];
  UINT64 qpc = m_ullHwQpc[m_dClockPosIn] - m_ullHwQpc[m_dClockPosOut];

  if (qpc == 0)
    return S_FALSE;

  UINT64 qpcNow = GetCurrentTimestamp() - m_ullHwQpc[m_dClockPosOut];

  pTimestamp = cMulDiv64(clock, qpcNow, qpc) + m_ullHwClock[m_dClockPosOut];
  pQpc = qpcNow + m_ullHwQpc[m_dClockPosOut];

  return S_OK;
}

static UINT64 prevPos = 0; // for debugging only, remove later

void WASAPIRenderer::UpdateAudioClock()
{
  if (m_pAudioClock)
  {
    CAutoLock cAutoLock(&m_csClockLock);

    UINT64 timestamp = 0;
    UINT64 qpc = 0;
    HRESULT hr = m_pAudioClock->GetPosition(&timestamp, &qpc);
    if (hr != S_OK)
      return; // no point in adding the data into collection when we cannot get real data

    UINT64 ullHwClock = cMulDiv64(timestamp, 10000000, m_nHWfreq);
    
    if (prevPos > ullHwClock)
      Log("UpdateAudioClock: prevPos: %I64u > ullHwClock: %I64u diff: %I64u ", prevPos, ullHwClock, prevPos - ullHwClock);

    prevPos = ullHwClock;

    if (m_dClockPosIn == m_dClockPosOut)
    {
        m_ullHwClock[m_dClockPosIn] = ullHwClock;
        m_ullHwQpc[m_dClockPosIn] = qpc;
    }
    m_dClockPosIn = (m_dClockPosIn + 1) % CLOCK_DATA_SIZE;
    //if (m_dClockPosIn == m_dClockPosOut)
    m_dClockPosOut = (m_dClockPosIn + 1) % CLOCK_DATA_SIZE;

    //Log("HW clock diff: %I64u QPC diff: %I64u", m_ullHwClock[m_dClockPosOut] - m_ullHwClock[m_dClockPosIn], m_ullHwQpc[m_dClockPosOut] - m_ullHwQpc[m_dClockPosIn]);

    m_ullHwClock[m_dClockPosIn] = ullHwClock;
    m_ullHwQpc[m_dClockPosIn] = qpc;
    m_dClockDataCollectionCount++;
  }
}

void WASAPIRenderer::ResetClockData()
{
  CAutoLock cAutoLock(&m_csClockLock);
  Log("WASAPIRenderer::ResetClockData");
  m_dClockPosIn = 0;
  m_dClockPosOut = 0;
  m_dClockDataCollectionCount = 0;
  ZeroMemory((void*)&m_ullHwClock, sizeof(UINT64) * CLOCK_DATA_SIZE);
  ZeroMemory((void*)&m_ullHwQpc, sizeof(UINT64) * CLOCK_DATA_SIZE);
}

REFERENCE_TIME WASAPIRenderer::Latency()
{
  return m_pRenderer->Settings()->m_hnsPeriod;
}

HRESULT WASAPIRenderer::SetRate(double dRate)
{
  m_dRate = dRate;
  m_bDiscardCurrentSample = true;      

  StopAudioClient(&m_pAudioClient);
  
  return S_OK;
}


HRESULT WASAPIRenderer::EnableMMCSS()
{
  if (!m_hTask)
  {
    // Ask MMCSS to temporarily boost the thread priority
    // to reduce glitches while the low-latency stream plays.
    DWORD taskIndex = 0;

    if (pfAvSetMmThreadCharacteristicsW)
    {
      m_hTask = pfAvSetMmThreadCharacteristicsW(L"Pro Audio", &taskIndex);
      Log("WASAPIRenderer::EnableMMCSS Putting thread in higher priority for WASAPI mode");
      
      if (!m_hTask)
      {
        return GetLastError();
      }
    }
  }
  return S_OK;
}

HRESULT WASAPIRenderer::RevertMMCSS()
{
  if (m_hTask && pfAvRevertMmThreadCharacteristics)
  {
    if (pfAvRevertMmThreadCharacteristics(m_hTask))
    {
      return S_OK;
    }
    else
    {
      return GetLastError();      
    }
  }
  return S_FALSE; // failed since no thread had been boosted
}

HRESULT	WASAPIRenderer::DoRenderSample(IMediaSample *pMediaSample, LONGLONG /*pSampleCounter*/)
{
  // To reduce logging we check the started status here 
  if (!m_bIsAudioClientStarted)
  {
    StartAudioClient(&m_pAudioClient);
  }

  HRESULT	hr = S_OK;

  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtStop = 0;
  
  DWORD flags = 0;

  m_nBufferSize = pMediaSample->GetActualDataLength();

  pMediaSample->GetTime(&rtStart, &rtStop);
  
  if (!m_pRenderer->Settings()->m_bEnableAC3Encoding)
  {
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
        Log("WASAPIRenderer::DoRenderSample - Error while checking audio client with input media type");
        return hr;
      }
      DeleteMediaType(pmt);
      pmt = NULL;
    }
  }

  // resample audio stream if required
  if (m_pRenderer->Settings()->m_bUseTimeStretching)
  {
    CAutoLock cAutoLock(m_pRenderer->ResampleLock());
    m_pRenderer->SoundTouch()->processSample(pMediaSample);
  }
  else // if no time stretching is enabled the sample goes directly to the sample queue
  {
    m_pRenderer->SoundTouch()->QueueSample(pMediaSample);
  }

  return hr;
}

HRESULT WASAPIRenderer::CheckAudioClient(WAVEFORMATEX *pWaveFormatEx)
{
  CAutoLock cInterfaceLock(m_pRenderer->InterfaceLock());
 
  Log("WASAPIRenderer::CheckAudioClient");
  LogWaveFormat(pWaveFormatEx, "WASAPIRenderer::CheckAudioClient");

  HRESULT hr = S_OK;
  CAutoLock cAutoLock(&m_csCheck);
  
  if (!m_pMMDevice) 
    hr = GetAudioDevice(&m_pMMDevice);

  // If no WAVEFORMATEX structure provided and client already exists, return it
  if (m_pAudioClient && !pWaveFormatEx) 
    return hr;

  // Just create the audio client if no WAVEFORMATEX provided
  if (!m_pAudioClient)
  {
    if (SUCCEEDED (hr)) hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
      return hr;
  }

  // Compare the exisiting WAVEFORMATEX with the one provided
  WAVEFORMATEX *pNewWaveFormatEx = NULL;
  if (CheckFormatChanged(pWaveFormatEx, &pNewWaveFormatEx))
  {
    Log("WASAPIRenderer::CheckAudioClient Format changed, reinitialize the audio client");
    
    { // Keep the render tread lock so it is safe to change the render format
      CAutoLock cRaenderThreadLock(m_pRenderer->RenderThreadLock());

      hr = m_pAudioClient->IsFormatSupported(m_pRenderer->Settings()->m_WASAPIShareMode, m_pRenderFormat, NULL);    
    
      if (FAILED(hr) && m_pRenderFormat)
      {
        //Log("   WASAPI client refused the format: (0x%08x) - try WAVEFORMATEX", hr);
        WAVEFORMATEX* tmpPwfx = NULL; 
        CopyWaveFormatEx(&tmpPwfx, m_pRenderFormat);
        tmpPwfx->cbSize = 0;
        hr = m_pAudioClient->IsFormatSupported(m_pRenderer->Settings()->m_WASAPIShareMode, tmpPwfx, NULL);

        // truncate the WAVEFORMATEXTENSIBLE part since driver is more happy with the WAVEFORMATEX
        if (SUCCEEDED(hr))
        {
          pWaveFormatEx->cbSize = 0;
        }

        SAFE_DELETE_WAVEFORMATEX(tmpPwfx);
      }
    } // End of the render thread lock

    if (SUCCEEDED(hr))
    { 
      // While pausing the render thread (done by stop call) we cannot hold the render thread lock
	  StopAudioClient(&m_pAudioClient);
      
      // Rendering thread shouldn't try to access the WASAPI device during recreation
      CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());
      SAFE_RELEASE(m_pRenderClient);
      SAFE_RELEASE(m_pAudioClock);
      SAFE_RELEASE(m_pAudioClient);
      
      SAFE_DELETE_WAVEFORMATEX(m_pRenderFormat);
      m_pRenderFormat = pNewWaveFormatEx;

      hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
    }
    else
    {
      Log("WASAPIRenderer::CheckAudioClient New format not supported, accept it anyway");
      return S_OK;
    }
  }
  else if (!m_pRenderClient)
  {
    Log("WASAPIRenderer::CheckAudioClient First initialization of the audio renderer");
  }
  else
  {
    return hr;  
  }

  CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());
  SAFE_RELEASE(m_pRenderClient);

  if (SUCCEEDED (hr)) 
  {
    hr = InitAudioClient(pWaveFormatEx, m_pAudioClient, &m_pRenderClient);
  }
  return hr;
}

HRESULT WASAPIRenderer::GetAudioDevice(IMMDevice **ppMMDevice)
{
  Log("WASAPIRenderer::GetAudioDevice");

  CComPtr<IMMDeviceEnumerator> enumerator;
  IMMDeviceCollection* devices;
  HRESULT hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

  if (hr != S_OK)
  {
    Log("  failed to create MMDeviceEnumerator!");
    return hr;
  }

  Log("Target end point: %S", m_pRenderer->Settings()->m_wWASAPIPreferredDeviceId);

  if (GetAvailableAudioDevices(&devices, false) == S_OK && devices)
  {
    UINT count(0);
    hr = devices->GetCount(&count);
    if (hr != S_OK)
    {
      Log("  devices->GetCount failed: (0x%08x)", hr);
      return hr;
    }
    
    for (int i = 0 ; i < count ; i++)
    {
      LPWSTR pwszID = NULL;
      IMMDevice *endpoint = NULL;
      hr = devices->Item(i, &endpoint);
      if (hr == S_OK)
      {
        hr = endpoint->GetId(&pwszID);
        if (hr == S_OK)
        {
          // Found the configured audio endpoint
          if (wcscmp(pwszID, m_pRenderer->Settings()->m_wWASAPIPreferredDeviceId) == 0)
          {
            enumerator->GetDevice(m_pRenderer->Settings()->m_wWASAPIPreferredDeviceId, ppMMDevice); 
            SAFE_RELEASE(devices);
            *(ppMMDevice) = endpoint;
            CoTaskMemFree(pwszID);
            pwszID = NULL;
            return S_OK;
          }
          else
          {
            SAFE_RELEASE(endpoint);
            CoTaskMemFree(pwszID);
            pwszID = NULL;
          }
        }
        else
        {
          Log("  devices->GetId failed: (0x%08x)", hr);     
        }
      }
      else
      {
        Log("  devices->Item failed: (0x%08x)", hr);  
      }

      CoTaskMemFree(pwszID);
      pwszID = NULL;
    }
  }

  Log("Unable to find selected audio device, using the default end point!");
  hr = enumerator->GetDefaultAudioEndpoint(eRender, eConsole, ppMMDevice);

  IPropertyStore* pProps = NULL;

  if (SUCCEEDED((*ppMMDevice)->OpenPropertyStore(STGM_READ, &pProps)))
  {
    LPWSTR pwszID = NULL;
    
    PROPVARIANT varName;
    PropVariantInit(&varName);

    PROPVARIANT eventDriven;
    PropVariantInit(&eventDriven);

    PROPVARIANT speakerMask;
    PropVariantInit(&speakerMask);

    if (SUCCEEDED(pProps->GetValue(PKEY_Device_FriendlyName, &varName)) &&
        SUCCEEDED(pProps->GetValue(PKEY_AudioEndpoint_Supports_EventDriven_Mode, &eventDriven)) &&
        SUCCEEDED((*ppMMDevice)->GetId(&pwszID)))
    {
      pProps->GetValue(PKEY_AudioEndpoint_PhysicalSpeakers, &speakerMask);
      Log("Default audio endpoint: \"%S\" (%S) - pull mode: %d sprk mask: %d" ,varName.pwszVal, pwszID, eventDriven.intVal, speakerMask.uintVal);
    }

    CoTaskMemFree(pwszID);
    pwszID = NULL;
    PropVariantClear(&varName);
    PropVariantClear(&eventDriven);
    PropVariantClear(&speakerMask);
    SAFE_RELEASE(pProps)
  }

  SAFE_RELEASE(devices);

  return hr;
}

HRESULT WASAPIRenderer::GetAvailableAudioDevices(IMMDeviceCollection **ppMMDevices, bool pLog)
{
  HRESULT hr;

  CComPtr<IMMDeviceEnumerator> enumerator;
  Log("WASAPIRenderer::GetAvailableAudioDevices");
  hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

  if (FAILED(hr))
  {
    Log("   failed to get MMDeviceEnumerator");
    return S_FALSE;
  }

  IMMDevice* pEndpoint = NULL;
  IPropertyStore* pProps = NULL;
  LPWSTR pwszID = NULL;

  enumerator->EnumAudioEndpoints(eRender, DEVICE_STATE_ACTIVE, ppMMDevices);
  UINT count(0);
  hr = (*ppMMDevices)->GetCount(&count);

  if (pLog)
  {
    for (ULONG i = 0; i < count; i++)
    {
      if ((*ppMMDevices)->Item(i, &pEndpoint) != S_OK)
        break;

      if (pEndpoint->GetId(&pwszID) != S_OK)
        break;

      if (pEndpoint->OpenPropertyStore(STGM_READ, &pProps) != S_OK)
        break;

      PROPVARIANT varName;
      PropVariantInit(&varName);

      PROPVARIANT eventDriven;
      PropVariantInit(&eventDriven);

      PROPVARIANT speakerMask;
      PropVariantInit(&speakerMask);

      if (pProps->GetValue(PKEY_Device_FriendlyName, &varName) != S_OK)
        break;

      Log(" ");
      Log("Audio endpoint %d:", i);
      Log("  %S", varName.pwszVal);
      Log("  %S",  pwszID);

      if (pProps->GetValue(PKEY_AudioEndpoint_Supports_EventDriven_Mode, &eventDriven) == S_OK)
      {
        Log("  supports pull mode: %d", eventDriven.intVal);
      }
      else
      {
        Log("  pull mode query failed!");
      }

      if (pProps->GetValue(PKEY_AudioEndpoint_PhysicalSpeakers, &speakerMask) == S_OK)
      {
        Log("  speaker mask: %d", speakerMask.uintVal);
      }
      else
      {
        Log("  PhysicalSpeakers query failed!");
      }

      CoTaskMemFree(pwszID);
      pwszID = NULL;
      PropVariantClear(&varName);
      PropVariantClear(&eventDriven);
      PropVariantClear(&speakerMask);
      SAFE_RELEASE(pProps)
      SAFE_RELEASE(pEndpoint)
    }
    Log(" ");
  }

  return hr;
}

bool WASAPIRenderer::CheckFormatChanged(const WAVEFORMATEX *pWaveFormatEx, WAVEFORMATEX **ppNewWaveFormatEx)
{
  //if (!pWaveFileFormat) return E_POINTER;
  
  bool formatChanged = false;

  if (!m_pRenderFormat)
  {
    formatChanged = true;
  }
  else if (pWaveFormatEx->wFormatTag != m_pRenderFormat->wFormatTag ||
           pWaveFormatEx->nChannels != m_pRenderFormat->nChannels ||
           pWaveFormatEx->wBitsPerSample != m_pRenderFormat->wBitsPerSample) // TODO : improve the checks
  {
    formatChanged = true;
  }

  if (!formatChanged)
    return false;

  if (!ppNewWaveFormatEx) // no copy requested
    return true;

  HRESULT hr = CopyWaveFormatEx(ppNewWaveFormatEx, pWaveFormatEx);
  
  if (FAILED(hr))
    return false;
  
  return true;
}

HRESULT WASAPIRenderer::GetBufferSize(const WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod)
{ 
  if (!pWaveFormatEx) 
    return S_OK;

  if (pWaveFormatEx->cbSize < sizeof(WAVEFORMATEXTENSIBLE)-sizeof(WAVEFORMATEX))
  {
    if (pWaveFormatEx->wFormatTag == WAVE_FORMAT_DOLBY_AC3_SPDIF)
      m_nBufferSize = 6144;
    else
      return S_OK; // PCM
  }
  else
  {
    WAVEFORMATEXTENSIBLE *wfext = (WAVEFORMATEXTENSIBLE*)pWaveFormatEx;
    
    if (wfext->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_MLP)
      m_nBufferSize = 61440;
    else if (wfext->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DTS_HD)
      m_nBufferSize = 32768;
    else if (wfext->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL_PLUS)
      m_nBufferSize = 24576;
    else return S_OK;
  }

  *pHnsBufferPeriod = (REFERENCE_TIME)((REFERENCE_TIME)m_nBufferSize * 10000 * 8 / ((REFERENCE_TIME)pWaveFormatEx->nChannels * pWaveFormatEx->wBitsPerSample *
    1.0 * pWaveFormatEx->nSamplesPerSec) /*+ 0.5*/);
    *pHnsBufferPeriod *= 1000;

  Log("WASAPIRenderer::GetBufferSize set a %lld period for a %ld buffer size",*pHnsBufferPeriod, m_nBufferSize);

  return S_OK;
}

HRESULT WASAPIRenderer::InitAudioClient(const WAVEFORMATEX *pWaveFormatEx, IAudioClient *pAudioClient, IAudioRenderClient **ppRenderClient)
{
  CAutoLock cInterfaceLock(m_pRenderer->InterfaceLock());
  CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());
  
  Log("WASAPIRenderer::InitAudioClient");
  HRESULT hr = S_OK;
  
  if (m_pRenderer->Settings()->m_hnsPeriod == 0 || m_pRenderer->Settings()->m_hnsPeriod == 1)
  {
    REFERENCE_TIME defaultPeriod(0);
    REFERENCE_TIME minimumPeriod(0);

    hr = m_pAudioClient->GetDevicePeriod(&defaultPeriod, &minimumPeriod);
    if (SUCCEEDED(hr))
    {
      if (m_pRenderer->Settings()->m_hnsPeriod == 0)
        m_pRenderer->Settings()->m_hnsPeriod = defaultPeriod;
      else
        m_pRenderer->Settings()->m_hnsPeriod = minimumPeriod;
      Log("WASAPIRenderer::InitAudioClient using device period from driver %I64u ms", m_pRenderer->Settings()->m_hnsPeriod / 10000);
    }
    else
    {
      Log("WASAPIRenderer::InitAudioClient failed to get device period from driver (0x%08x) - using 50 ms", hr); 
      m_pRenderer->Settings()->m_hnsPeriod = 500000; //50 ms is the best according to James @Slysoft
    }
  }

  if (!m_pAudioClient)
  {
    hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
    if (FAILED(hr))
    {
      Log("WASAPIRenderer::InitAudioClient failed to create audio client (0x%08x)", hr);
      return hr;
    }
    else
    {
      Log("WASAPIRenderer::InitAudioClient created missing audio client");
    }
  }

  WAVEFORMATEX *pwfxCM = NULL;
  hr = m_pAudioClient->IsFormatSupported(m_pRenderer->Settings()->m_WASAPIShareMode, pWaveFormatEx, &pwfxCM);    
  if (FAILED(hr))
  {
    Log("WASAPIRenderer::InitAudioClient not supported (0x%08x)", hr);
  }
  else
  {
    Log("WASAPIRenderer::InitAudioClient format supported");
  }

  GetBufferSize(pWaveFormatEx, &m_pRenderer->Settings()->m_hnsPeriod);

  if (SUCCEEDED(hr))
  {
    hr = m_pAudioClient->Initialize(m_pRenderer->Settings()->m_WASAPIShareMode, m_StreamFlags,
	                                m_pRenderer->Settings()->m_hnsPeriod, m_pRenderer->Settings()->m_hnsPeriod, pWaveFormatEx, NULL);
    
    // when rebuilding the graph between SD / HD zapping the .NET GC workaround
    // might call the init again. In that case just eat the error 
    // this needs to be fixed properly if .NET GC workaround is going to be the final solution...
    if (hr == AUDCLNT_E_ALREADY_INITIALIZED)
      return S_OK;
  }

  if (FAILED(hr) && hr != AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED)
  {
    Log("WASAPIRenderer::InitAudioClient Initialize failed (0x%08x)", hr);
    return hr;
  }

  if (hr == S_OK)
  {
    SAFE_RELEASE(m_pAudioClock);
    hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
    if (SUCCEEDED(hr))
    {
      m_pAudioClock->GetFrequency(&m_nHWfreq);
    }
    else
    {
      Log("WASAPIRenderer::IAudioClock not found!");
    }
  }

  if (hr == AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED) 
  {
    // if the buffer size was not aligned, need to do the alignment dance
    Log("WASAPIRenderer::InitAudioClient Buffer size not aligned. Realigning");

    // get the buffer size, which will be aligned
    hr = m_pAudioClient->GetBufferSize(&m_nFramesInBuffer);

    // throw away this IAudioClient
    SAFE_RELEASE(m_pAudioClient);

    // calculate the new aligned periodicity
    m_pRenderer->Settings()->m_hnsPeriod = // hns =
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
      
    Log("WASAPIRenderer::InitAudioClient Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames", m_pRenderer->Settings()->m_hnsPeriod, m_nFramesInBuffer);

    if (SUCCEEDED (hr)) 
    {
      hr = m_pAudioClient->Initialize(m_pRenderer->Settings()->m_WASAPIShareMode, m_StreamFlags, 
	                                    m_pRenderer->Settings()->m_hnsPeriod, m_pRenderer->Settings()->m_hnsPeriod, pWaveFormatEx, NULL);
    }
 
    if (FAILED(hr))
    {
      Log("WASAPIRenderer::InitAudioClient Failed to reinitialize the audio client");
      return hr;
    }
    else
    {
      SAFE_RELEASE(m_pAudioClock);
      hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
      if (FAILED(hr))
      {
        Log("WASAPIRenderer::IAudioClock not found!");
      }
      else
      {
        m_pAudioClock->GetFrequency(&m_nHWfreq);
      }
    }
  } // if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) 

  // get the buffer size, which is aligned
  if (SUCCEEDED(hr)) 
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
    Log("WASAPIRenderer::InitAudioClient service initialization failed (0x%08x)", hr);
  }
  else
  {
    Log("WASAPIRenderer::InitAudioClient service initialization success");
  }

  if (m_pRenderer->Settings()->m_bWASAPIUseEventMode)
  {
    hr = m_pAudioClient->SetEventHandle(m_hDataEvent);
    if (FAILED(hr))
    {
      Log("WASAPIRenderer::InitAudioClient SetEventHandle failed (0x%08x)", hr);
      return hr;
    }
  }

  REFERENCE_TIME latency(0);
  m_pAudioClient->GetStreamLatency(&latency);
  
  Log("WASAPIRenderer::InitAudioClient device reported latency %I64u ms - buffer based latency %I64u ms", 
    latency / 10000, Latency() / 10000);

  return hr;
}

HRESULT WASAPIRenderer::CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient)
{
  CAutoLock cInterfaceLock(m_pRenderer->InterfaceLock());
  CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());

  HRESULT hr = S_OK;

  Log("WASAPIRenderer::CreateAudioClient");

  StopAudioClient(ppAudioClient);

  if (!pMMDevice)
  {
    Log("WASAPIRenderer::CreateAudioClient failed, device not loaded");
    return E_FAIL;
  }

  hr = pMMDevice->Activate(__uuidof(IAudioClient), CLSCTX_ALL, NULL, reinterpret_cast<void**>(ppAudioClient));
  if (FAILED(hr))
  {
    Log("WASAPIRenderer::CreateAudioClient activation failed (0x%08x)", hr);
  }
  else
  {
    Log("WASAPIRenderer::CreateAudioClient success");
  }

  return hr;
}

void WASAPIRenderer::StartAudioClient(IAudioClient** ppAudioClient)
{
  if (!m_bIsAudioClientStarted)
  {
    Log("WASAPIRenderer::StartAudioClient");
    HRESULT hr = S_OK;

    if ((*ppAudioClient))
    {
      hr = (*ppAudioClient)->Start();
      if (FAILED(hr))
      {
        m_bIsAudioClientStarted = false;
        Log("  start failed (0x%08x)", hr);
      }
      else
      {
        m_bIsAudioClientStarted = true;
      }
      StartRendererThread();
    }
  }
  else
  {
    Log("WASAPIRenderer::StartAudioClient - ignored, already started"); 
  }

  if (!m_pRenderer->Settings()->m_bWASAPIUseEventMode)
  {
    LARGE_INTEGER liDueTime;
    liDueTime.QuadPart = 0LL;
  
    CancelDataEvent();

    // We need to manually start the rendering thread when in polling mode
    SetWaitableTimer(m_hDataEvent, &liDueTime, 0, NULL, NULL, 0);
  }
}

void WASAPIRenderer::StopAudioClient(IAudioClient** ppAudioClient)
{
  if (m_bIsAudioClientStarted)
  {
    Log("WASAPIRenderer::StopAudioClient");
    HRESULT hr = S_OK;

    m_bIsAudioClientStarted = false;

    CancelDataEvent();
    PauseRendererThread();

    if (*ppAudioClient)
    {
      // Let the current audio buffer to be played completely.
      // Some amplifiers will "cache" the incomplete AC3 packets and that causes issues
      // when the next AC3 packets are received
      Sleep(Latency() / 10000);
      
      hr = (*ppAudioClient)->Stop();
      if (FAILED(hr))
        Log("   stop failed (0x%08x)", hr);

      hr = (*ppAudioClient)->Reset();
      if (FAILED(hr))
        Log("   reset failed (0x%08x)", hr);
    }
  }
}

DWORD WASAPIRenderer::RenderThread()
{
  Log("WASAPIRenderer::Render thread - starting up - thread ID: %d", m_threadId);
  
  HRESULT hr = S_OK;

  // polling delay
  LARGE_INTEGER liDueTime; 
  liDueTime.QuadPart = -1LL;

  // These are wait handles for the thread stopping, new sample arrival and pausing redering
  HANDLE handles[3];
  handles[0] = m_hStopRenderThreadEvent;
  handles[1] = m_hPauseEvent;
  handles[2] = m_hDataEvent;

  // These are wait handles for resuming or exiting the thread
  HANDLE resumeHandles[2];
  resumeHandles[0] = m_hStopRenderThreadEvent;
  resumeHandles[1] = m_hResumeEvent;

  IMediaSample* sample = NULL;
  UINT32 sampleOffset = 0;
  bool writeSilence = false;

  EnableMMCSS();

  while (true)
  {
    // 1) Waiting for the next WASAPI buffer to be available to be filled
    // 2) Exit requested for the thread
    // 3) For a pause request
    DWORD result = WaitForMultipleObjects(3, handles, false, INFINITE);
    if (result == WAIT_OBJECT_0) // exit event
    {
      Log("WASAPIRenderer::Render thread - closing down - thread ID: %d", m_threadId);
      m_pRenderer->SoundTouch()->GetNextSample(NULL, true);
      RevertMMCSS();
      return 0;
    }
    else if (result == WAIT_OBJECT_0 + 1) // pause event
    {
      Log("WASAPIRenderer::Render thread - pausing");
      m_bThreadPaused = true;
      ResetEvent(m_hResumeEvent);
      SetEvent(m_hWaitPauseEvent);

      DWORD resultResume = WaitForMultipleObjects(2, resumeHandles, false, INFINITE);
      if (resultResume == WAIT_OBJECT_0) // exit event
      {
        Log("WASAPIRenderer::Render thread - closing down - thread ID: %d", m_threadId);
        m_pRenderer->SoundTouch()->GetNextSample(NULL, true);
        RevertMMCSS();
        return 0;
      }
      if (resultResume == WAIT_OBJECT_0 + 1) // resume event
      {
        Log("WASAPIRenderer::Render thread - resuming from pause");
        m_bThreadPaused = false;
        SetEvent(m_hWaitResumeEvent);
      }
    }
    else if (result == WAIT_OBJECT_0 + 2) // data event
    {
      UpdateAudioClock();

      UINT32 bufferSize = 0;
      UINT32 currentPadding = 0;
      
      CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());
      HRESULT hr = S_FALSE;

      if (m_bDiscardCurrentSample)
      {
        sample = NULL;
        m_bDiscardCurrentSample = false;
      }

      // Fetch the sample for the first time
      if (!sample)
      {
        sampleOffset = 0;
        hr = m_pRenderer->SoundTouch()->GetNextSample(&sample, false);
        if (FAILED(hr))
        {
          // No samples in queue 
          writeSilence = true;
        }
      }

      // Interface lock should keep us safe from different threads closing down the audio client
      if (m_pAudioClient && m_pRenderClient && m_bIsAudioClientStarted)
      {
        BYTE* data = NULL;
        
        m_pAudioClient->GetBufferSize(&bufferSize);

        // In exclusive mode with even based buffer filling we threat the padding as zero 
        // -> it will make rest of the code a bit cleaner
        if (m_pRenderer->Settings()->m_WASAPIShareMode == AUDCLNT_SHAREMODE_SHARED ||
            !m_pRenderer->Settings()->m_bWASAPIUseEventMode)
        {
          m_pAudioClient->GetCurrentPadding(&currentPadding);
        }

        UINT32 bufferSizeInBytes = (bufferSize - currentPadding) * m_pRenderFormat->nBlockAlign;

        hr = m_pRenderClient->GetBuffer(bufferSize - currentPadding, &data);
        if (SUCCEEDED(hr))
        {
          DWORD bufferFlags = 0;

          if (writeSilence || !sample)
            bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
          else if (sample) // we have at least some data to be written
          {
            UINT32 bytesCopied = 0;
            BYTE* sampleData = NULL;
            UINT32 dataLeftInSample = 0;
            UINT32 sampleLength = sample->GetActualDataLength();
            
            // Sanity check to make sure that the calculation won't underflow
            if (sampleOffset <= sampleLength)
               dataLeftInSample = sampleLength - sampleOffset;
            else
            {
              dataLeftInSample = 0;
              sampleOffset = 0;
            }

            sample->GetPointer(&sampleData);

            do
            {
              // no data in current sample anymore
              if (dataLeftInSample == 0)
              {
                sample = NULL;
                hr = m_pRenderer->SoundTouch()->GetNextSample(&sample, false);
                
                if (FAILED(hr))
                {
                  Log("WASAPIRenderer::Render thread: Buffer underrun, fetching sample failed (0x%08x)", hr);
                  if(bytesCopied == 0)
                    bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
                  break;
                }
                else if (!sample)
                {
                  Log("WASAPIRenderer::Render thread: Buffer underrun, no new samples available!");  
                  if(bytesCopied == 0)
                    bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
                  break;
                }

                sample->GetPointer(&sampleData);
                sampleLength = sample->GetActualDataLength();
                sampleOffset = 0;
              }

              dataLeftInSample = sampleLength - sampleOffset;

              UINT32 bytesToCopy = min(dataLeftInSample, bufferSizeInBytes - bytesCopied);
              memcpy(data + bytesCopied, sampleData + sampleOffset, bytesToCopy); 
              bytesCopied += bytesToCopy;
              sampleOffset += bytesToCopy;
            } while (bytesCopied < bufferSizeInBytes);
          }
          hr = m_pRenderClient->ReleaseBuffer(bufferSize - currentPadding, bufferFlags);
          if (FAILED(hr))
          {
            Log("WASAPIRenderer::Render thread: ReleaseBuffer failed (0x%08x)", hr);
          }
        }
        else
        {
          Log("WASAPIRenderer::Render thread: GetBuffer failed (0x%08x)", hr);
        }
      }
      
      if (!m_pRenderer->Settings()->m_bWASAPIUseEventMode)
  	  {
        if (m_pAudioClient)
        {
          hr = m_pAudioClient->GetCurrentPadding(&currentPadding);
        }
        else
        {
          hr = S_FALSE;
        }
        if (SUCCEEDED(hr) && bufferSize > 0)
        {
          liDueTime.QuadPart = (double)currentPadding / (double)bufferSize * (double)m_pRenderer->Settings()->m_hnsPeriod * -0.9;
          // Log(" currentPadding: %d QuadPart: %lld", currentPadding, liDueTime.QuadPart);
        }
        else
        {
          liDueTime.QuadPart = (double)m_pRenderer->Settings()->m_hnsPeriod * -0.9;
          if (hr != AUDCLNT_E_NOT_INITIALIZED)
            Log("WASAPIRenderer::Render thread: GetCurrentPadding failed (0x%08x)", hr);  
        }
		    SetWaitableTimer(m_hDataEvent, &liDueTime, 0, NULL, NULL, 0);
      }
    }
    else
    {
      DWORD error = GetLastError();
      Log("WASAPIRenderer::Render thread: WaitForMultipleObjects failed: %d", error);
    }
  }
  
  Log("WASAPIRenderer::Render thread - closing down - thread ID: %d", m_threadId);
  RevertMMCSS();
  return 0;
}

DWORD WINAPI WASAPIRenderer::RenderThreadEntryPoint(LPVOID lpParameter)
{
  return ((WASAPIRenderer *)lpParameter)->RenderThread();
}
