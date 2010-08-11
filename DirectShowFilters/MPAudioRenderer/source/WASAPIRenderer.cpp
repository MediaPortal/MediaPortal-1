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

#include "stdafx.h"
#include "WASAPIRenderer.h"

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

extern void Log(const char *fmt, ...);
extern void LogWaveFormat(WAVEFORMATEX* pwfx, const char *text);

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
  m_hDataEvent(NULL),
  m_hStopRenderThreadEvent(NULL),
  m_hWaitRenderThreadToExitEvent(NULL),
  m_pAudioClock(NULL),
  m_nHWfreq(0)
{
  IMMDeviceCollection* devices = NULL;
  GetAvailableAudioDevices(&devices, true);
  SAFE_RELEASE(devices); // currently only log available devices

  m_hDataEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  m_hStopRenderThreadEvent = CreateEvent(0, FALSE, FALSE, 0);
  m_hWaitRenderThreadToExitEvent = CreateEvent(0, FALSE, FALSE, 0);

  m_hRenderThread = CreateThread(0, 0, WASAPIRenderer::RenderThreadEntryPoint, (LPVOID)this, 0, &m_threadId);

  HMODULE hLib = NULL;

  // Load Vista specifics DLLs
  hLib = LoadLibrary ("AVRT.dll");
  if (hLib)
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

  if (m_hTask && pfAvRevertMmThreadCharacteristics)
  {
    pfAvRevertMmThreadCharacteristics(m_hTask);
  }

  if (m_hWaitRenderThreadToExitEvent)
    CloseHandle(m_hWaitRenderThreadToExitEvent);
  if (m_hStopRenderThreadEvent)
    CloseHandle(m_hStopRenderThreadEvent);
  if (m_hDataEvent)
    CloseHandle(m_hDataEvent);

  Log("WASAPIRenderer - destructor - instance 0x%x - end", this);
}

HRESULT WASAPIRenderer::StopRendererThread()
{
  Log("WASAPIRenderer::StopRendererThread");
  // Get rid of the render thread
  if (m_hRenderThread)
  {
    SetEvent(m_hStopRenderThreadEvent);
    WaitForSingleObject(m_hWaitRenderThreadToExitEvent, INFINITE);

    CloseHandle(m_hRenderThread);
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
    Log("WASAPIRenderer::CheckMediaType WASAPI client refused the format: (0x%08x)", hr);
    LogWaveFormat(pwfxCM, "Closest match would be" );
    CoTaskMemFree(pwfxCM);
    return VFW_E_TYPE_NOT_ACCEPTED;
  }
  Log("WASAPIRenderer::CheckMediaType WASAPI client accepted the format");

  return S_OK;
}

HRESULT WASAPIRenderer::SetMediaType(const CMediaType *pmt)
{
  // New media type set but render client already initialized => reset it
  if (m_pRenderClient)
  {
    WAVEFORMATEX *pNewWf = (WAVEFORMATEX *)pmt->Format();
    Log("WASAPIRenderer::SetMediaType Render client already initialized. Reinitialization...");
    CheckAudioClient(pNewWf);
  }
  return S_OK;
}

HRESULT WASAPIRenderer::CompleteConnect(IPin *pReceivePin)
{
  return InitCoopLevel();
}

HRESULT WASAPIRenderer::EndOfStream()
{
  return S_OK;
}

HRESULT WASAPIRenderer::BeginFlush()
{
  HRESULT hr = S_OK;

  m_bDiscardCurrentSample = true;

  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    m_pAudioClient->Stop();
    hr = m_pAudioClient->Reset();
    m_bIsAudioClientStarted = false;

    if (hr != S_OK)
    {
      Log("WASAPIRenderer::BeginFlush - m_pAudioClient reset failed with (0x%08x)", hr);
    }
  }

  return hr;
}

HRESULT WASAPIRenderer::EndFlush()
{
  return S_OK;
}
HRESULT WASAPIRenderer::Run(REFERENCE_TIME tStart)
{
  Log("WASAPIRenderer::Run");

  HRESULT hr = 0;
  WAVEFORMATEX* pWaveFileFormat = m_pRenderer->WaveFormat();

  hr = CheckAudioClient(pWaveFileFormat);
  if (FAILED(hr)) 
  {
    Log("WASAPIRenderer::Run - error on check audio client (0x%08x)", hr);
    return hr;
  }

  // this is required for the .NET GC workaround
  if (m_bReinitAfterStop)
  {
    m_bReinitAfterStop = false;
    hr = InitAudioClient(pWaveFileFormat, m_pAudioClient, &m_pRenderClient);
    if (FAILED(hr)) 
    {
      Log("WASAPIRenderer::Run - error on reinit after stop (0x%08x) - trying to continue", hr);
      //return hr;
    }
  }

  if (!m_bIsAudioClientStarted)
  {
    Log("WASAPIRenderer::Run - Starting audio client");
    m_pAudioClient->Start();
    m_bIsAudioClientStarted = true;
  }

  return hr;
}


HRESULT WASAPIRenderer::Stop(FILTER_STATE pState)
{
  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    m_pAudioClient->Stop();
    m_pAudioClient->Reset();
  }

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

  m_bIsAudioClientStarted = false;  
  
  return S_OK;
}

HRESULT WASAPIRenderer::Pause(FILTER_STATE pState)
{
  // TODO: check if this could be fixed without requiring to drop all sample
  if (pState == State_Running)
  {
    m_bDiscardCurrentSample = true;
  }

  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    m_pAudioClient->Stop();
  }

  m_bIsAudioClientStarted = false;

  return S_OK;
}

HRESULT WASAPIRenderer::AudioClock(ULONGLONG& pTimestamp, ULONGLONG& pQpc)
{
  if (m_pAudioClock)
  {
    m_pAudioClock->GetPosition(&pTimestamp, &pQpc);
    pTimestamp = pTimestamp * 10000000 / m_nHWfreq;
    return S_OK;
  }
  return S_FALSE;
}

REFERENCE_TIME WASAPIRenderer::Latency()
{
  return m_pRenderer->Settings()->m_hnsPeriod;
}

HRESULT WASAPIRenderer::SetRate(double dRate)
{
  HRESULT hr = S_FALSE;
  m_dRate = dRate;

  m_bDiscardCurrentSample = true;      

  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    HRESULT hr = S_OK;

    m_pAudioClient->Stop();
    hr = m_pAudioClient->Reset();
    m_bIsAudioClientStarted = false;

    if (hr != S_OK)
    {
      Log("WASAPIRenderer::SetRate - m_pAudioClient reset failed with (0x%08x)", hr);
    }
  }
  
  return hr;
}


HRESULT WASAPIRenderer::InitCoopLevel()
{
  if (!m_hTask)
  {
    // Ask MMCSS to temporarily boost the thread priority
    // to reduce glitches while the low-latency stream plays.
    DWORD taskIndex = 0;

    if (pfAvSetMmThreadCharacteristicsW)
    {
      m_hTask = pfAvSetMmThreadCharacteristicsW(L"Pro Audio", &taskIndex);
      Log("WASAPIRenderer::InitCoopLevel Putting thread in higher priority for Wasapi mode (lowest latency)");
      
      if (!m_hTask)
      {
        return GetLastError();      
      }
    }
  }

	return S_OK;
}

HRESULT	WASAPIRenderer::DoRenderSample(IMediaSample *pMediaSample, LONGLONG /*pSampleCounter*/)
{
  HRESULT	hr = S_OK;
  
  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtStop = 0;
  
  DWORD flags = 0;
  BYTE* pMediaBuffer = NULL;
  BYTE* mediaBufferResult = NULL; 

  BYTE* pInputBufferPointer = NULL;
  BYTE* pInputBufferEnd = NULL;
  BYTE* pData;

  m_nBufferSize = pMediaSample->GetActualDataLength();
  long lSize = m_nBufferSize;
  long lResampledSize = 0;

  pMediaSample->GetTime(&rtStart, &rtStop);
  
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
  CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());

  WAVEFORMATEX* pWaveFileFormat = m_pRenderer->WaveFormat();

  Log("WASAPIRenderer::CheckAudioClient");
  LogWaveFormat(pWaveFormatEx, "WASAPIRenderer::CheckAudioClient");

  WAVEFORMATEX AC3WaveFormatEx;

  // Negotiate the SPDIF connection type only with the audio device
  if (m_pRenderer->Settings()->m_bEnableAC3Encoding)
  {
    Log("  AC3 encoding mode enabled");
    CreateWaveFormatForAC3(&AC3WaveFormatEx);
  } 

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
    // Format has changed, audio client has to be reinitialized
    Log("WASAPIRenderer::CheckAudioClient Format changed, reinitialize the audio client");
    if (pWaveFileFormat)
    {
      BYTE *p = (BYTE *)pWaveFileFormat;
      SAFE_DELETE_ARRAY(p);
    }
  
    pWaveFileFormat = pNewWaveFormatEx;

    if (m_pRenderer->Settings()->m_bEnableAC3Encoding)
    {
      hr = m_pAudioClient->IsFormatSupported(m_pRenderer->Settings()->m_WASAPIShareMode, &AC3WaveFormatEx, NULL);
    }
    else
    {
      hr = m_pAudioClient->IsFormatSupported(m_pRenderer->Settings()->m_WASAPIShareMode, pWaveFormatEx, NULL);    
    }
  
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

  SAFE_RELEASE(m_pRenderClient);

  if (SUCCEEDED (hr)) 
  {
    if (m_pRenderer->Settings()->m_bEnableAC3Encoding)
    {
      hr = InitAudioClient(&AC3WaveFormatEx, m_pAudioClient, &m_pRenderClient);
    }
    else
    {
      hr = InitAudioClient(pWaveFormatEx, m_pAudioClient, &m_pRenderClient);
    }
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

  if (GetAvailableAudioDevices(&devices, false) == S_OK)
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

    if (SUCCEEDED(pProps->GetValue(PKEY_Device_FriendlyName, &varName)) &&
        SUCCEEDED(pProps->GetValue(PKEY_AudioEndpoint_Supports_EventDriven_Mode, &eventDriven)) &&
        SUCCEEDED((*ppMMDevice)->GetId(&pwszID)))
    {
      Log("Default audio endpoint: \"%S\" (%S) - supports pull mode: %d",varName.pwszVal, pwszID, eventDriven.intVal);
    }

    CoTaskMemFree(pwszID);
    pwszID = NULL;
    PropVariantClear(&varName);
    PropVariantClear(&eventDriven);
    SAFE_RELEASE(pProps)
  }

  SAFE_RELEASE(devices);

  return hr;
}

HRESULT WASAPIRenderer::GetAvailableAudioDevices(IMMDeviceCollection **ppMMDevices, bool pLog)
{
  HRESULT hr;

  CComPtr<IMMDeviceEnumerator> enumerator;
  Log("WASAPIRenderer::GetAvailableAudioDevices\n");
  hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

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

      if (pProps->GetValue(PKEY_Device_FriendlyName, &varName) != S_OK)
        break;

      Log("Audio endpoint %d:", i);
      Log("  %S", varName.pwszVal);
      Log("  %S",  pwszID);

      if (pProps->GetValue(PKEY_AudioEndpoint_Supports_EventDriven_Mode, &eventDriven) == S_OK)
      {
        Log("  supports pull mode: %d", eventDriven.intVal);
      }
      else
      {
        Log("   pull mode query failed!");
      }
  
      Log("");

      CoTaskMemFree(pwszID);
      pwszID = NULL;
      PropVariantClear(&varName);
      PropVariantClear(&eventDriven);
      SAFE_RELEASE(pProps)
      SAFE_RELEASE(pEndpoint)
    }
  }

  return hr;
}

bool WASAPIRenderer::CheckFormatChanged(WAVEFORMATEX *pWaveFormatEx, WAVEFORMATEX **ppNewWaveFormatEx)
{
  WAVEFORMATEX* pWaveFileFormat = m_pRenderer->WaveFormat();
  if (!pWaveFileFormat) return E_POINTER;
  
  bool formatChanged = false;

  if (!pWaveFileFormat)
  {
    formatChanged = true;
  }
  else if (pWaveFormatEx->wFormatTag != pWaveFileFormat->wFormatTag ||
           pWaveFormatEx->nChannels != pWaveFileFormat->nChannels ||
           pWaveFormatEx->wBitsPerSample != pWaveFileFormat->wBitsPerSample) // TODO : improve the checks
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

HRESULT WASAPIRenderer::GetBufferSize(WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod)
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

  Log("WASAPIRenderer::GetBufferSize set a %lld period for a %ld buffer size",*pHnsBufferPeriod, m_nBufferSize);

  return S_OK;
}

HRESULT WASAPIRenderer::InitAudioClient(WAVEFORMATEX *pWaveFormatEx, IAudioClient *pAudioClient, IAudioRenderClient **ppRenderClient)
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
      Log("WASAPIRenderer::InitAudioClient using device period from drivers %d ms", m_pRenderer->Settings()->m_hnsPeriod / 10000);
    }
    else
    {
      Log("WASAPIRenderer::InitAudioClient failed to get device period from drivers (0x%08x) - using 50 ms", hr); 
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

  if (SUCCEEDED (hr))
  {
    hr = m_pAudioClient->Initialize(m_pRenderer->Settings()->m_WASAPIShareMode, AUDCLNT_STREAMFLAGS_EVENTCALLBACK,
	                                m_pRenderer->Settings()->m_hnsPeriod, m_pRenderer->Settings()->m_hnsPeriod, pWaveFormatEx, NULL);
    
    // when rebuilding the graph between SD / HD zapping the .NET GC workaround
    // might call the init again. In that case just eat the error 
    // this needs to be fixed properly if .NET GC workaround is going to be the final solution...
    if (hr == AUDCLNT_E_ALREADY_INITIALIZED)
      return S_OK;
  }

  if (FAILED (hr) && hr != AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED)
  {
    Log("WASAPIRenderer::InitAudioClient Initialize failed (0x%08x)", hr);
    return hr;
  }

  if (hr == S_OK)
  {
    SAFE_RELEASE(m_pAudioClock);
    hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
    if(SUCCEEDED(hr))
    {
      m_pAudioClock->GetFrequency(&m_nHWfreq);
    }
    else
    {
      Log("WASAPIRenderer::IAudioClock not found!");
    }
  }

  if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) 
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
      
    Log("WASAPIRenderer::InitAudioClient Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames.\n", m_pRenderer->Settings()->m_hnsPeriod, m_nFramesInBuffer);

    if (SUCCEEDED (hr)) 
    {
      hr = m_pAudioClient->Initialize(m_pRenderer->Settings()->m_WASAPIShareMode, AUDCLNT_STREAMFLAGS_EVENTCALLBACK, 
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
      if(FAILED(hr))
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
    Log("WASAPIRenderer::InitAudioClient service initialization failed (0x%08x)", hr);
  }
  else
  {
    Log("WASAPIRenderer::InitAudioClient service initialization success");
  }

  hr = m_pAudioClient->SetEventHandle(m_hDataEvent);

  if (FAILED(hr))
  {
    Log("WASAPIRenderer::InitAudioClient SetEventHandle failed (0x%08x)", hr);
    return hr;
  }

  return hr;
}

HRESULT WASAPIRenderer::CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient)
{
  CAutoLock cInterfaceLock(m_pRenderer->InterfaceLock());
  CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());

  HRESULT hr = S_OK;

  Log("WASAPIRenderer::CreateAudioClient");

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

void WASAPIRenderer::CreateWaveFormatForAC3(WAVEFORMATEX* pwfx)
{
  if (pwfx)
  {
    pwfx->wFormatTag = WAVE_FORMAT_DOLBY_AC3_SPDIF;
    pwfx->wBitsPerSample = 16;
    pwfx->nBlockAlign = 4;
    pwfx->nChannels = 2;
    pwfx->nSamplesPerSec = 48000;
    pwfx->nAvgBytesPerSec = 80000;//192000;
    pwfx->cbSize = 0;
  }
}

DWORD WASAPIRenderer::RenderThread()
{
  Log("WASAPIRenderer::Render thread - starting up - thread ID: %d", m_threadId);
  
  HRESULT hr = S_OK;

  // These are wait handles for the thread stopping and new sample arrival
  HANDLE handles[2];
  handles[0] = m_hStopRenderThreadEvent;
  handles[1] = m_hDataEvent;

  IMediaSample* sample = NULL;
  UINT32 sampleOffset = 0;
  bool writeSilence = false;

  while(true)
  {
    // 1) Waiting for the next WASAPI buffer to be available to be filled
    // 2) Exit requested for the thread
    DWORD result = WaitForMultipleObjects(2, handles, false, INFINITE);
    if (result == WAIT_OBJECT_0)
    {
      Log("WASAPIRenderer::Render thread - closing down - thread ID: %d", m_threadId);
      m_pRenderer->SoundTouch()->GetNextSample(NULL, true);
      SetEvent(m_hWaitRenderThreadToExitEvent);
      return 0;
    }
    else if (result == WAIT_OBJECT_0 + 1)
    {
      CAutoLock cRenderThreadLock(m_pRenderer->RenderThreadLock());

      HRESULT hr = S_FALSE;

      if (m_bDiscardCurrentSample)
      {
        sample = NULL;
        m_bDiscardCurrentSample = false;
      }

      // Fetch the sample for the first time
      if(!sample)
      {
        sampleOffset = 0;
        hr = m_pRenderer->SoundTouch()->GetNextSample(&sample, false);
        if (FAILED(hr))
        {
          // No samples in queue 
          writeSilence = true;
        }
      }

      UINT32 bufferSize = 0;
      BYTE* data;

      // Interface lock should be us safe different threads closing down the audio client
      if (m_pAudioClient && m_pRenderClient)
      {
        DWORD bufferFlags = 0;
        UINT32 currentPadding = 0;
        
        m_pAudioClient->GetBufferSize(&bufferSize);

        // In exclusive mode we threat the padding as zero -> it will make 
        // rest of the code a bit cleaner
        if (m_pRenderer->Settings()->m_WASAPIShareMode == AUDCLNT_SHAREMODE_SHARED)
          m_pAudioClient->GetCurrentPadding(&currentPadding);

        UINT32 bufferSizeInBytes = (bufferSize - currentPadding) * m_pRenderer->WaveFormat()->nBlockAlign;

        hr = m_pRenderClient->GetBuffer(bufferSize - currentPadding, &data);
        if (SUCCEEDED(hr) && m_pRenderClient)
        {
          if (writeSilence || !sample)
            bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
          else if (sample) // we have at least some data to be written
          {
            UINT32 bytesCopied = 0;
            BYTE* sampleData = NULL;
            UINT32 sampleLength = sample->GetActualDataLength();
            UINT32 dataLeftInSample = sampleLength - sampleOffset;

            sample->GetPointer(&sampleData);

            do
            {
              // no data in current sample anymore
              if(sampleLength - sampleOffset == 0)
              {
                sample = NULL;
                hr = m_pRenderer->SoundTouch()->GetNextSample(&sample, false);
                if (FAILED(hr) || !sample)
                {
                  // no next sample available
                  Log("WASAPIRenderer::Render thread: Buffer underrun, no new samples available!");
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
      }
    }
    else
    {
      DWORD error = GetLastError();
      Log("WASAPIRenderer::Render thread: WaitForMultipleObjects failed: %d", error);
    }
  }
  
  Log("WASAPIRenderer::Render thread - closing down - thread ID: %d", m_threadId);
  return 0;
}

DWORD WINAPI WASAPIRenderer::RenderThreadEntryPoint(LPVOID lpParameter)
{
  return ((WASAPIRenderer *)lpParameter)->RenderThread();
}
