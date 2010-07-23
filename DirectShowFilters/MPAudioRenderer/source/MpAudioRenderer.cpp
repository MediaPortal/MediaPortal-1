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
#include <propkey.h>
#include <FunctionDiscoveryKeys_devpkey.h>

#include "MpAudioRenderer.h"
#include "FilterApp.h"

#include "alloctracing.h"

CFilterApp theApp;

#define SAFE_DELETE(p)       { if(p) { delete (p);     (p)=NULL; } }
#define SAFE_DELETE_ARRAY(p) { if(p) { delete[] (p);   (p)=NULL; } }
#define SAFE_RELEASE(p)      { if(p) { (p)->Release(); (p)=NULL; } }

#define MAX_SAMPLE_TIME_ERROR 10000 // 1.0 ms

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

CUnknown* WINAPI CMPAudioRenderer::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
  ASSERT(phr);
  CMPAudioRenderer *pNewObject = new CMPAudioRenderer(punk, phr);

  if (!pNewObject)
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
  }
  return pNewObject;
}

// for logging
extern void Log(const char *fmt, ...);
extern void LogWaveFormat(WAVEFORMATEX* pwfx);
extern void LogRotate();

DWORD CMPAudioRenderer::RenderThread()
{
  Log("Render thread - starting up - thread ID: %d", m_threadId);
  
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
      Log("Render thread - closing down - thread ID: %d", m_threadId);
      m_pSoundTouch->GetNextSample(NULL, true);
      SetEvent(m_hWaitRenderThreadToExitEvent);
      return 0;
    }
    else if (result == WAIT_OBJECT_0 + 1)
    {
      CAutoLock cRenderThreadLock(&m_RenderThreadLock);

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
        hr = m_pSoundTouch->GetNextSample(&sample, false);
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
        m_pAudioClient->GetBufferSize(&bufferSize);

        // TODO use non-hardcoded value
        UINT32 bufferSizeInBytes = bufferSize * 4;

        hr = m_pRenderClient->GetBuffer(bufferSize, &data);

        if (SUCCEEDED(hr) && m_pRenderClient)
        {
          if (writeSilence || !sample)
          {
            bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
          }
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
                hr = m_pSoundTouch->GetNextSample(&sample, false);
                if (FAILED(hr) || !sample)
                {
                  // no next sample available
                  Log("Render thread: Buffer underrun, no new samples available!");
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
        }

        hr = m_pRenderClient->ReleaseBuffer(bufferSize, bufferFlags);
        if (FAILED(hr))
        {
          Log("Render thread: ReleaseBuffer failed (0x%08x)", hr);
        }
      }
    }
    else
    {
      DWORD error = GetLastError();
      Log("Render thread: WaitForMultipleObjects failed: %d", error);
    }
  }
  
  Log("Render thread - closing down - thread ID: %d", m_threadId);
  return 0;
}

DWORD WINAPI CMPAudioRenderer::RenderThreadEntryPoint(LPVOID lpParameter)
{
  return ((CMPAudioRenderer *)lpParameter)->RenderThread();
}


CMPAudioRenderer::CMPAudioRenderer(LPUNKNOWN punk, HRESULT *phr)
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
, m_dSampleCounter(0)
, m_pAudioClock(NULL)
, m_nHWfreq(0)
, m_WASAPIShareMode(AUDCLNT_SHAREMODE_EXCLUSIVE)
, m_bReinitAfterStop(false)
, m_wWASAPIPreferredDeviceId(NULL)
, m_hDataEvent(NULL)
, m_hRenderThread(NULL)
, m_hWaitRenderThreadToExitEvent(NULL)
, m_hStopRenderThreadEvent(NULL)
, m_bDiscardCurrentSample(false)
, m_rtNextSampleTime(0)
, m_rtPrevSampleTime(0)
, m_bDropSamples(false)
{
  LogRotate();
  Log("MP Audio Renderer - v0.61 - instance 0x%x", this);

  LoadSettingsFromRegistry();

  if (m_bUseWASAPI)
  {
    IMMDeviceCollection* devices = NULL;
    GetAvailableAudioDevices(&devices, true);
    SAFE_RELEASE(devices); // currently only log available devices
    
    m_hDataEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
    m_hStopRenderThreadEvent = CreateEvent(0, FALSE, FALSE, 0);
    m_hWaitRenderThreadToExitEvent = CreateEvent(0, FALSE, FALSE, 0);

    m_hRenderThread = CreateThread(0, 0, CMPAudioRenderer::RenderThreadEntryPoint, (LPVOID)this, 0, &m_threadId);

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
      m_bUseWASAPI = false;	// WASAPI not available below Vista
    }
  }

  if (!m_bUseWASAPI)
  {
    *phr = DirectSoundCreate8(NULL, &m_pDS, NULL);
  }
  
  m_pSoundTouch = new CMultiSoundTouch();
  
  if (!m_pSoundTouch)
  {
    if(phr)
      *phr = E_OUTOFMEMORY;
  }
}


CMPAudioRenderer::~CMPAudioRenderer()
{
  Log("MP Audio Renderer - destructor - instance 0x%x", this);
  
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);
  CAutoLock cInterfaceLock(&m_InterfaceLock);

  Stop();

  if (m_pSoundTouch)
    m_pSoundTouch->StopResamplingThread();

  // Get rid of the render thread
  if (m_hRenderThread)
  {
    SetEvent(m_hStopRenderThreadEvent);
    WaitForSingleObject(m_hWaitRenderThreadToExitEvent, INFINITE);

    CloseHandle(m_hRenderThread);
  }

  delete m_pSoundTouch;

  // DSound
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

  if (m_hWaitRenderThreadToExitEvent)
    CloseHandle(m_hWaitRenderThreadToExitEvent);
  if (m_hStopRenderThreadEvent)
    CloseHandle(m_hStopRenderThreadEvent);
  if (m_hDataEvent)
    CloseHandle(m_hDataEvent);

  delete[] m_wWASAPIPreferredDeviceId;

  Log("MP Audio Renderer - destructor - instance 0x%x - end", this);
}

void CMPAudioRenderer::LoadSettingsFromRegistry()
{
  USES_CONVERSION;
  
  Log("Loading settings from registry");

  LPCTSTR folder = TEXT("Software\\Team MediaPortal\\Audio Renderer");

  HKEY hKey;
  char* lpData = new char[MAX_REG_LENGTH];

  // Registry setting names
  LPCTSTR forceDirectSound = TEXT("ForceDirectSound");
  LPCTSTR enableTimestretching = TEXT("EnableTimestretching");
  LPCTSTR WASAPIExclusive = TEXT("WASAPIExclusive");
  LPCTSTR devicePeriod = TEXT("DevicePeriod");
  LPCTSTR WASAPIPreferredDevice = TEXT("WASAPIPreferredDevice");
  
  // Default values for the settings in registry
  DWORD forceDirectSoundData = 0;
  DWORD enableTimestretchingData = 1;
  DWORD WASAPIExclusiveData = 1;
  DWORD devicePeriodData = 500000; // 50 ms
  LPCTSTR WASAPIPreferredDeviceData = new TCHAR[MAX_REG_LENGTH];

  ZeroMemory((void*)WASAPIPreferredDeviceData, MAX_REG_LENGTH);

  // Try to access the setting root "Software\Team MediaPortal\Audio Renderer"
  RegOpenKeyEx(HKEY_CURRENT_USER, folder, NULL, KEY_ALL_ACCESS, &hKey);

  if (hKey)
  {
    // Read settings from registry
    ReadRegistryKeyDword(hKey, forceDirectSound, forceDirectSoundData);
    ReadRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
    ReadRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
    ReadRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
    ReadRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);

    Log("   ForceDirectSound:        %d", forceDirectSoundData);
    Log("   EnableTimestrecthing:    %d", enableTimestretchingData);
    Log("   WASAPIExclusive:         %d", WASAPIExclusiveData);
    Log("   DevicePeriod:            %d (1 == minimal, 0 == default, other user defined)", devicePeriodData);
    Log("   WASAPIPreferredDevice:   %s", WASAPIPreferredDeviceData);

    if (forceDirectSoundData > 0)
      m_bUseWASAPI = false;
    else
      m_bUseWASAPI = true;

    if (enableTimestretchingData > 0)
      m_bUseTimeStretching = true;
    else
      m_bUseTimeStretching = false;

    if (WASAPIExclusiveData > 0)
      m_WASAPIShareMode = AUDCLNT_SHAREMODE_EXCLUSIVE;
    else
      m_WASAPIShareMode = AUDCLNT_SHAREMODE_SHARED;

    m_hnsPeriod = devicePeriodData;

    delete[] m_wWASAPIPreferredDeviceId;
    m_wWASAPIPreferredDeviceId = new WCHAR[MAX_REG_LENGTH];
    
    wcsncpy(m_wWASAPIPreferredDeviceId, T2W(WASAPIPreferredDeviceData), MAX_REG_LENGTH);

    delete[] WASAPIPreferredDeviceData;
  }

  else // no settings in registry, create default values
  {
    Log("Failed to open %s", folder);
    Log("Initializing registry with default settings");

    LONG result = RegCreateKeyEx(HKEY_CURRENT_USER, folder, 0, NULL, REG_OPTION_NON_VOLATILE,
                                  KEY_ALL_ACCESS, NULL, &hKey, NULL);

    if (result == ERROR_SUCCESS) 
    {
      Log("Success creating master key");
      WriteRegistryKeyDword(hKey, forceDirectSound, forceDirectSoundData);
      WriteRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
      WriteRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
      WriteRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
    } 
    else 
    {
      Log("Error creating master key %d", result);
    }
  }
  
  delete[] lpData;
  RegCloseKey (hKey);
}

void CMPAudioRenderer::ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  DWORD dwSize = sizeof(DWORD);
  DWORD dwType = REG_DWORD;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)&data, &dwSize);
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      Log("   create default value for %s", lpSubKey);
      WriteRegistryKeyDword(hKey, lpSubKey, data);
    }
    else
    {
      Log("   faíled to create default value for %s", lpSubKey);
    }
  }
}

void CMPAudioRenderer::WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{  
  DWORD dwSize = sizeof(DWORD);
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_DWORD, (LPBYTE)&data, dwSize);
  if (result == ERROR_SUCCESS) 
  {
    Log("Success writing to Registry: %s", lpSubKey);
  } 
  else 
  {
    Log("Error writing to Registry - subkey: %s error: %d", lpSubKey, result);
  }
}

void CMPAudioRenderer::ReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{
  DWORD dwSize = MAX_REG_LENGTH;
  DWORD dwType = REG_SZ;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)data, &dwSize);
  
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      Log("   create default value for %s", lpSubKey);
      WriteRegistryKeyString(hKey, lpSubKey, data);
    }
    else if (error == ERROR_MORE_DATA)
    {
      Log("   too much data, corrupted registry setting(?):  %s", lpSubKey);      
    }
    else
    {
      Log("   error: %d subkey: %s", error, lpSubKey);       
    }
  }
}

void CMPAudioRenderer::WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{  
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_SZ, (LPBYTE)data, strlen(data)+1);
  if (result == ERROR_SUCCESS) 
  {
    Log("Success writing to Registry: %s", lpSubKey);
  } 
  else 
  {
    Log("Error writing to Registry - subkey: %s error: %d", lpSubKey, result);
  }
}

HRESULT CMPAudioRenderer::CheckInputType(const CMediaType *pmt)
{
  return CheckMediaType(pmt);
}

HRESULT	CMPAudioRenderer::CheckMediaType(const CMediaType *pmt)
{
  HRESULT hr = S_OK;
  
  if (!pmt) 
    return E_INVALIDARG;
  
  //Log("CheckMediaType");
  WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->Format();

  if (!pwfx) 
    return VFW_E_TYPE_NOT_ACCEPTED;

  if ((pmt->majortype	!= MEDIATYPE_Audio) ||
      (pmt->formattype != FORMAT_WaveFormatEx))
  {
    //Log("CheckMediaType Not supported");
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  LogWaveFormat(pwfx);

  if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
  {
    // TODO: should we do something specific here? At least W7 audio codec is providing this info
    // WAVEFORMATPCMEX *test = (WAVEFORMATPCMEX *) pmt->Format();
    // return VFW_E_TYPE_NOT_ACCEPTED;
  }

  // TODO: allow other than 16 bit audio stream resampling!
  if (m_bUseTimeStretching && pwfx->wBitsPerSample != 16)
  {
    Log("CheckMediaType Error only 16 bit audio resampling is currently supported");
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  if (m_bUseWASAPI)
  {
    //hr = CheckAudioClient((WAVEFORMATEX *)NULL);
    hr = CheckAudioClient(pwfx);
    if (FAILED(hr))
    {
      Log("CheckMediaType Error on check audio client");
      return hr;
    }
    if (!m_pAudioClient) 
    {
      Log("CheckMediaType Error, audio client not loaded");
      return VFW_E_CANNOT_CONNECT;
    }
    
    // NOTE: using ClosestMatch parameter causes the call to succeed on some drivers 
    // even when the client wont support the format!
    if (m_pAudioClient->IsFormatSupported(m_WASAPIShareMode, pwfx, NULL) != S_OK)
    {
      Log("CheckMediaType WASAPI client refused the format, used mix format:");
      WAVEFORMATEX *pwfxCM = NULL;
      m_pAudioClient->GetMixFormat(&pwfxCM);
      LogWaveFormat(pwfxCM);
      CoTaskMemFree(pwfxCM);
      return VFW_E_TYPE_NOT_ACCEPTED;
    }
    Log("CheckMediaType WASAPI client accepted the format");
  }
  else if	(pwfx->wFormatTag	!= WAVE_FORMAT_PCM)
  {
    return VFW_E_TYPE_NOT_ACCEPTED;
  }
  return S_OK;
}

void CMPAudioRenderer::AudioClock(UINT64& pTimestamp, UINT64& pQpc)
{
  if (m_pAudioClock)
  {
    m_pAudioClock->GetPosition(&pTimestamp, &pQpc);
    pTimestamp = pTimestamp * 10000000 / m_nHWfreq;
  }

  //TRACE(_T("AudioClock query pos: %I64d qpc: %I64d"), pTimestamp, pQpc);
}

void CMPAudioRenderer::OnReceiveFirstSample(IMediaSample *pMediaSample)
{
  if (!m_bUseWASAPI)
  {
    ClearBuffer();
  }
}

BOOL CMPAudioRenderer::ScheduleSample(IMediaSample *pMediaSample)
{
  REFERENCE_TIME rtSampleTime = 0;
  REFERENCE_TIME rtSampleEndTime = 0;
  REFERENCE_TIME rtTime = 0;

  // Is someone pulling our leg
  if (!pMediaSample) return FALSE;

  if (m_dRate >= 2.0 || m_dRate <= -2.0)
  {
    // Do not render Micey Mouse(tm) audio
    return TRUE;
  }

  m_dSampleCounter++;

  // Get the next sample due up for rendering.  If there aren't any ready
  // then GetNextSampleTimes returns an error.  If there is one to be done
  // then it succeeds and yields the sample times. If it is due now then
  // it returns S_OK other if it's to be done when due it returns S_FALSE
  HRESULT hr = GetSampleTimes(pMediaSample, &rtSampleTime, &rtSampleEndTime);
  if (FAILED(hr)) return FALSE;

  // Try to keep the A/V sync when data has been dropped
  if (abs(rtSampleTime - m_rtNextSampleTime) > MAX_SAMPLE_TIME_ERROR)
  {
    m_bDropSamples = true;
    Log("Dropped audio data detected: diff: %lld ms MAX_SAMPLE_TIME_ERROR: %d ms", (rtSampleTime - m_rtNextSampleTime / 10000), MAX_SAMPLE_TIME_ERROR / 10000);
  }

  // Get media time
  m_pClock->GetTime(&rtTime);
  rtTime = rtTime - m_tStart;

  UINT nFrames = pMediaSample->GetActualDataLength() / m_pWaveFileFormat->nBlockAlign;
  REFERENCE_TIME rtSampleDuration = nFrames * UNITS / m_pWaveFileFormat->nSamplesPerSec;
  REFERENCE_TIME rtLate = rtTime - rtSampleTime;
  
  m_rtNextSampleTime = rtSampleTime + rtSampleDuration;

  //Log("  rtTime: %lld ms rtSampleTime: %lld ms diff %lld ms", rtTime / 10000, rtSampleTime / 10000, (rtTime - rtSampleTime) / 10000);

  // The whole timespan of the sampe is late
  if( rtLate > rtSampleDuration && m_bDropSamples)
  {
    Log("   dropping whole sample - late: %lld ms dur: %lld ms", rtLate/10000, rtSampleDuration/10000);
    
    pMediaSample->SetActualDataLength(0);

	// Ttriggers next sample to be scheduled
    EXECUTE_ASSERT(SetEvent((HANDLE)m_RenderEvent));
    return TRUE;
  }
  else if (m_bDropSamples && rtLate > 0)
  {
    long newLenght = m_pWaveFileFormat->nBlockAlign * ((rtSampleDuration - rtLate) * m_pWaveFileFormat->nSamplesPerSec / UNITS);
    long sampleLenght = pMediaSample->GetActualDataLength();
    
    // Just some sanity checks
    if( newLenght < 0 )
    {
      newLenght = 0;
    }
    else
    {
      newLenght = min(newLenght, sampleLenght);
    }

    Log("   dropping part of sample %d / %d", newLenght, sampleLenght);
    pMediaSample->SetActualDataLength(newLenght);

    BYTE* sampleData = NULL;
    pMediaSample->GetPointer(&sampleData);
    if (sampleData)
    {
      // Discard the oldest sample data to match the start timestamp
      memmove(sampleData, sampleData + newLenght, newLenght);
    }
  }
  
  if (m_dSampleCounter > 1 && !m_bDropSamples)
  {
    EXECUTE_ASSERT(SetEvent((HANDLE) m_RenderEvent));
    m_rtPrevSampleTime = rtSampleTime;
    return TRUE;
  }

  if (m_dRate <= 1.1)
  {
    // Discard all old data in queues
    if (m_bDropSamples)
    {
      CAutoLock cInterfaceLock(&m_InterfaceLock);
      CAutoLock cRenderThreadLock(&m_RenderThreadLock);

      m_pSoundTouch->BeginFlush();
      m_pSoundTouch->flush();
      m_pSoundTouch->EndFlush();
      m_bDropSamples = false; // stream is continuous from this point on
    }

    ASSERT(m_dwAdvise == 0);
    ASSERT(m_pClock);
    WaitForSingleObject((HANDLE)m_RenderEvent, 0);

    hr = m_pClock->AdviseTime((REFERENCE_TIME)m_tStart, rtSampleTime, (HEVENT)(HANDLE)m_RenderEvent, &m_dwAdvise);
    
    if (SUCCEEDED(hr)) return TRUE;
  }
  else
  {
    hr = DoRenderSample(pMediaSample);
  }

  m_rtPrevSampleTime = rtSampleTime;

  // We could not schedule the next sample for rendering despite the fact
  // we have a valid sample here. This is a fair indication that either
  // the system clock is wrong or the time stamp for the sample is duff
  ASSERT(m_dwAdvise == 0);

  return FALSE;
}

HRESULT	CMPAudioRenderer::DoRenderSample(IMediaSample *pMediaSample)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  HRESULT hr = S_FALSE;

  if (m_bUseWASAPI)
  {
    hr = DoRenderSampleWasapi(pMediaSample);
  }
  else
  {
    hr = DoRenderSampleDirectSound(pMediaSample);
  }

  return hr;
}


STDMETHODIMP CMPAudioRenderer::NonDelegatingQueryInterface(REFIID riid, void **ppv)
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

HRESULT CMPAudioRenderer::SetMediaType(const CMediaType *pmt)
{
	if (!pmt) return E_POINTER;
  
  HRESULT hr = S_OK;
  Log("SetMediaType");

  if (m_bUseWASAPI)
  {
    // New media type set but render client already initialized => reset it
    if (m_pRenderClient)
    {
      WAVEFORMATEX *pNewWf = (WAVEFORMATEX *)pmt->Format();
      Log("SetMediaType Render client already initialized. Reinitialization...");
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
    int	size = sizeof(WAVEFORMATEX) + pwf->cbSize;

    m_pWaveFileFormat = (WAVEFORMATEX *)new BYTE[size];

    if (!m_pWaveFileFormat)
      return E_OUTOFMEMORY;

    memcpy(m_pWaveFileFormat, pwf, size);

    if (m_pSoundTouch)
    {
      m_pSoundTouch->setChannels(pwf->nChannels);
      m_pSoundTouch->setSampleRate(pwf->nSamplesPerSec);
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

HRESULT CMPAudioRenderer::CompleteConnect(IPin *pReceivePin)
{
  Log("CompleteConnect");
  
  HRESULT hr = S_OK;

  if (!m_bUseWASAPI && !m_pDS) return E_FAIL;

  if (SUCCEEDED(hr)) hr = CBaseRenderer::CompleteConnect(pReceivePin);
  if (SUCCEEDED(hr)) hr = InitCoopLevel();

  if (!m_bUseWASAPI)
  {
    if (SUCCEEDED(hr)) hr = CreateDSBuffer();
  }
  if (SUCCEEDED(hr)) Log("CompleteConnect Success");

  return hr;
}

STDMETHODIMP CMPAudioRenderer::Run(REFERENCE_TIME tStart)
{
  Log("Run");

  CAutoLock cInterfaceLock(&m_InterfaceLock);
  
  HRESULT	hr;
  m_dwTimeStart = timeGetTime();

  if (m_State == State_Running) return NOERROR;

  if (m_bUseWASAPI)
  {
    hr = CheckAudioClient(m_pWaveFileFormat);
    if (FAILED(hr)) 
    {
      Log("Run: error on check audio client (0x%08x)", hr);
      return hr;
    }

    // this is required for the .NET GC workaround
    if (m_bReinitAfterStop)
    {
      m_bReinitAfterStop = false;
      hr = InitAudioClient(m_pWaveFileFormat, m_pAudioClient, &m_pRenderClient);
      if (FAILED(hr)) 
      {
        Log("Run: error on reinit after stop (0x%08x) - trying to continue", hr);
        //return hr;
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

STDMETHODIMP CMPAudioRenderer::Stop() 
{
  Log("Stop");

  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);
  
  if (m_pDSBuffer)
  {
    m_pDSBuffer->Stop();
  }
  
  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    m_pAudioClient->Stop();
    m_pAudioClient->Reset();
  }

  // This is an ugly workaround for the .NET GC not cleaning up the directshow resources
  // when playback is stopped. Needs to be done since otherwise the next session might
  // fail if the old one is still alive and it is using WASAPI exclusive mode
  if (GetRealState() == State_Paused)
  {
    Log("Stop - releasing WASAPI resources");
    SAFE_RELEASE(m_pAudioClock);
    SAFE_RELEASE(m_pRenderClient);
    SAFE_RELEASE(m_pAudioClient);
    SAFE_RELEASE(m_pMMDevice);

    m_bReinitAfterStop = true;
  }

  m_bIsAudioClientStarted = false;

  return CBaseRenderer::Stop(); 
};


STDMETHODIMP CMPAudioRenderer::Pause()
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);

  Log("Pause");

  // TODO: check if this could be fixed without requiring to drop all sample
  if (GetRealState() == State_Running)
  {
    m_pSoundTouch->GetNextSample(NULL, true);
    m_bDiscardCurrentSample = true;
    m_pSoundTouch->BeginFlush();
    m_pSoundTouch->EndFlush();
  }

  if (m_pDSBuffer)
  {
    m_pDSBuffer->Stop();
  }
  
  if (m_pAudioClient && m_bIsAudioClientStarted) 
  {
    m_pAudioClient->Stop();
  }
  
  m_bIsAudioClientStarted = false;
  m_dSampleCounter = 0;
  m_rtNextSampleTime = 0;

  return CBaseRenderer::Pause(); 
};


HRESULT CMPAudioRenderer::GetReferenceClockInterface(REFIID riid, void **ppv)
{
  HRESULT hr = S_OK;

  if (m_pReferenceClock)
  {
    return m_pReferenceClock->NonDelegatingQueryInterface(riid, ppv);
  }

  m_pReferenceClock = new CBaseReferenceClock (NAME("MP Audio Clock"), NULL, &hr);
	
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

HRESULT CMPAudioRenderer::EndOfStream(void)
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

HRESULT CMPAudioRenderer::CreateDSBuffer()
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

HRESULT CMPAudioRenderer::ClearBuffer()
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

HRESULT CMPAudioRenderer::InitCoopLevel()
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
      Log("InitCoopLevel Putting thread in higher priority for Wasapi mode (lowest latency)");
      hr = GetLastError();
      if (!m_hTask)
      {
        return hr;
      }
    }
  }

	return hr;
}

HRESULT	CMPAudioRenderer::DoRenderSampleDirectSound(IMediaSample *pMediaSample)
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

HRESULT CMPAudioRenderer::WriteSampleToDSBuffer(IMediaSample *pMediaSample, bool *looped)
{
  if (!m_pDSBuffer) return E_POINTER;

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
  if (m_bUseTimeStretching)
  {
    CAutoLock cAutoLock(&m_csResampleLock);
	
    int nBytePerSample = m_pWaveFileFormat->nBlockAlign;

    m_pSoundTouch->processSample(pMediaSample);
    lSize = m_pSoundTouch->receiveSamples((short**)&mediaBufferResult, 0) * nBytePerSample;
    pMediaBuffer = mediaBufferResult;
  }

  pMediaSample->GetTime(&rtStart, &rtStop);
  //Log("Sample times: start=%ld, end=%ld", rtStart, rtStop);

  if (rtStart < 0)
  {
    DWORD dwPercent	= (DWORD) ((-rtStart * 100) / (rtStop - rtStart));
    DWORD dwRemove= (lSize * dwPercent/100);

    dwRemove = (dwRemove / m_pWaveFileFormat->nBlockAlign) * m_pWaveFileFormat->nBlockAlign;
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
        m_hnsActualDuration = (double)REFTIMES_PER_SEC * (m_nDSBufSize - numBytesAvailable + numBytesToWrite) / m_pWaveFileFormat->nBlockAlign / m_pWaveFileFormat->nSamplesPerSec;
        
        // Sleep time is half this duration
        m_hnsActualDuration = (DWORD)(m_hnsActualDuration / REFTIMES_PER_MILLISEC / 2);
        break;
      }
      // Buffer not completely filled, sleep for half buffer capacity duration
      m_hnsActualDuration = (double)REFTIMES_PER_SEC * m_nDSBufSize / m_pWaveFileFormat->nBlockAlign / m_pWaveFileFormat->nSamplesPerSec;
      
      // Sleep time is half this duration
      m_hnsActualDuration = (DWORD)(m_hnsActualDuration / REFTIMES_PER_MILLISEC / 2);
      //Log("Sleeping %ld ms", m_hnsActualDuration);
      Sleep(m_hnsActualDuration);
    }
  }
  if (SUCCEEDED(hr) && looped) *looped = loop;

  if (mediaBufferResult)
    free(mediaBufferResult);

  return hr;
}

#pragma endregion

#pragma region // ==== WASAPI

HRESULT	CMPAudioRenderer::DoRenderSampleWasapi(IMediaSample *pMediaSample)
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
      Log("DoRenderSampleWasapi Error while checking audio client with input media type");
      return hr;
    }
    DeleteMediaType(pmt);
    pmt = NULL;
  }

  // resample audio stream if required
  if (m_bUseTimeStretching)
  {
    CAutoLock cAutoLock(&m_csResampleLock);
	
    m_pSoundTouch->processSample(pMediaSample);

    if (m_dSampleCounter == 1)
    {
      UINT32 nFramesInBuffer;
      hr = m_pAudioClient->GetBufferSize(&m_nFramesInBuffer);
      hr = m_pRenderClient->GetBuffer(m_nFramesInBuffer, &pData); // just a "ping" buffer
      hr = m_pRenderClient->ReleaseBuffer(m_nFramesInBuffer, AUDCLNT_BUFFERFLAGS_SILENT);
    }
  }
  else // if no time stretching is enabled the sample goes directly to the sample queue
  {
    m_pSoundTouch->QueueSample(pMediaSample);
  }

  if (!m_bIsAudioClientStarted)
  {
    Log("DoRenderSampleWasapi Starting audio client");
    m_pAudioClient->Start();
    m_bIsAudioClientStarted = true;
  }

  return hr;
}

HRESULT CMPAudioRenderer::CheckAudioClient(WAVEFORMATEX *pWaveFormatEx)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);

  Log("CheckAudioClient");
  LogWaveFormat(pWaveFormatEx);

  HRESULT hr = S_OK;
  CAutoLock cAutoLock(&m_csCheck);
  
  if (!m_pMMDevice) 
    hr = GetAudioDevice(&m_pMMDevice);

  // If no WAVEFORMATEX structure provided and client already exists, return it
  if (m_pAudioClient && !pWaveFormatEx) 
    return hr;

  // Just create the audio client if no WAVEFORMATEX provided
  if (!m_pAudioClient)// && !pWaveFormatEx)
  {
    if (SUCCEEDED (hr)) hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
      return hr;
  }

  // Compare the exisiting WAVEFORMATEX with the one provided
  WAVEFORMATEX *pNewWaveFormatEx = NULL;
  if (CheckFormatChanged(pWaveFormatEx, &pNewWaveFormatEx))
  {
    // Format has changed, audio client has to be reinitialized
    Log("CheckAudioClient Format changed, reinitialize the audio client");
    if (m_pWaveFileFormat)
    {
      BYTE *p = (BYTE *)m_pWaveFileFormat;
      SAFE_DELETE_ARRAY(p);
    }
  
    m_pWaveFileFormat = pNewWaveFormatEx;
    hr = m_pAudioClient->IsFormatSupported(m_WASAPIShareMode, pWaveFormatEx, NULL);
  
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
      Log("CheckAudioClient New format not supported, accept it anyway");
      return S_OK;
    }
  }
  else if (!m_pRenderClient)
  {
    Log("CheckAudioClient First initialization of the audio renderer");
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

HRESULT CMPAudioRenderer::GetAudioDevice(IMMDevice **ppMMDevice)
{
  Log("GetAudioDevice");

  CComPtr<IMMDeviceEnumerator> enumerator;
  IMMDeviceCollection* devices;
  HRESULT hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

  if (hr != S_OK)
  {
    Log("  failed to create MMDeviceEnumerator!");
    return hr;
  }

  Log("Target end point: %S", m_wWASAPIPreferredDeviceId);

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
          if (wcscmp(pwszID, m_wWASAPIPreferredDeviceId) == 0)
          {
            enumerator->GetDevice(m_wWASAPIPreferredDeviceId, ppMMDevice); 
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

  SAFE_RELEASE(devices);

  return hr;
}

HRESULT CMPAudioRenderer::GetAvailableAudioDevices(IMMDeviceCollection **ppMMDevices, bool pLog)
{
  HRESULT hr;

  CComPtr<IMMDeviceEnumerator> enumerator;
  Log("GetAvailableAudioDevices");
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

      if (pProps->GetValue(PKEY_Device_FriendlyName, &varName) != S_OK)
        break;

      Log("Audio endpoint %d: \"%S\" (%S)", i, varName.pwszVal, pwszID);

      CoTaskMemFree(pwszID);
      pwszID = NULL;
      PropVariantClear(&varName);
      SAFE_RELEASE(pProps)
      SAFE_RELEASE(pEndpoint)
    }
  }

  return hr;
}

bool CMPAudioRenderer::CheckFormatChanged(WAVEFORMATEX *pWaveFormatEx, WAVEFORMATEX **ppNewWaveFormatEx)
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

HRESULT CMPAudioRenderer::GetBufferSize(WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod)
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

  Log("GetBufferSize set a %lld period for a %ld buffer size",*pHnsBufferPeriod, m_nBufferSize);

  return S_OK;
}

HRESULT CMPAudioRenderer::InitAudioClient(WAVEFORMATEX *pWaveFormatEx, IAudioClient *pAudioClient, IAudioRenderClient **ppRenderClient)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);
  
  Log("InitAudioClient");
  HRESULT hr = S_OK;
  
  if (m_hnsPeriod == 0 || m_hnsPeriod == 1)
  {
    REFERENCE_TIME defaultPeriod(0);
    REFERENCE_TIME minimumPeriod(0);

    hr = m_pAudioClient->GetDevicePeriod(&defaultPeriod, &minimumPeriod);
    if (SUCCEEDED(hr))
    {
      if (m_hnsPeriod == 0)
        m_hnsPeriod = defaultPeriod;
      else
        m_hnsPeriod = minimumPeriod;
      Log("InitAudioClient using device period from drivers %d ms", m_hnsPeriod / 10000);
    }
    else
    {
      Log("InitAudioClient failed to get device period from drivers (0x%08x) - using 50 ms", hr); 
      m_hnsPeriod = 500000; //50 ms is the best according to James @Slysoft
    }
  }

  if (!m_pAudioClient)
  {
    hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
    if (FAILED(hr))
    {
      Log("InitAudioClient failed to create audio client (0x%08x)", hr);
      return hr;
    }
    else
    {
      Log("InitAudioClient created missing audio client");
    }
  }

  hr = m_pAudioClient->IsFormatSupported(m_WASAPIShareMode, pWaveFormatEx, NULL);
  if (FAILED(hr))
  {
    Log("InitAudioClient not supported (0x%08x)", hr);
  }
  else
  {
    Log("InitAudioClient format supported");
  }

  GetBufferSize(pWaveFormatEx, &m_hnsPeriod);

  if (SUCCEEDED (hr))
  {
    hr = m_pAudioClient->Initialize(m_WASAPIShareMode, AUDCLNT_STREAMFLAGS_EVENTCALLBACK,
	                                m_hnsPeriod, m_hnsPeriod, pWaveFormatEx, NULL);
    
    // when rebuilding the graph between SD / HD zapping the .NET GC workaround
    // might call the init again. In that case just eat the error 
    // this needs to be fixed properly if .NET GC workaround is going to be the final solution...
    if (hr == AUDCLNT_E_ALREADY_INITIALIZED)
      return S_OK;
  }

  if (FAILED (hr) && hr != AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED)
  {
    Log("InitAudioClient failed (0x%08x)", hr);
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
      Log("IAudioClock not found!");
    }
  }

  if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) 
  {
    // if the buffer size was not aligned, need to do the alignment dance
    Log("InitAudioClient Buffer size not aligned. Realigning");

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
      
    Log("InitAudioClient Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames.\n", m_hnsPeriod, m_nFramesInBuffer);

    if (SUCCEEDED (hr)) 
    {
      hr = m_pAudioClient->Initialize(m_WASAPIShareMode, AUDCLNT_STREAMFLAGS_EVENTCALLBACK, 
	                                  m_hnsPeriod, m_hnsPeriod, pWaveFormatEx, NULL);
    }
 
    if (FAILED(hr))
    {
      Log("InitAudioClient Failed to reinitialize the audio client");
      return hr;
    }
    else
    {
      SAFE_RELEASE(m_pAudioClock);
      hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
      if(FAILED(hr))
      {
        Log("IAudioClock not found!");
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
    Log("InitAudioClient service initialization failed (0x%08x)", hr);
  }
  else
  {
    Log("InitAudioClient service initialization success");
  }

  hr = m_pAudioClient->SetEventHandle(m_hDataEvent);

  if (FAILED(hr))
  {
    Log("InitAudioClient SetEventHandle failed (0x%08x)", hr);
    return hr;
  }

  return hr;
}

HRESULT CMPAudioRenderer::CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);

  HRESULT hr = S_OK;

  Log("CreateAudioClient");

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
    Log("CreateAudioClient failed, device not loaded");
    return E_FAIL;
  }

  hr = pMMDevice->Activate(__uuidof(IAudioClient), CLSCTX_ALL, NULL, reinterpret_cast<void**>(ppAudioClient));
  if (FAILED(hr))
  {
    Log("CreateAudioClient activation failed (0x%08x)", hr);
  }
  else
  {
    Log("CreateAudioClient success");
  }

  return hr;
}

HRESULT CMPAudioRenderer::BeginFlush()
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);

  HRESULT hrBase = CBaseRenderer::BeginFlush(); 

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
      Log("BeginFlush - m_pAudioClient reset failed with (0x%08x)", hr);
    }
  }
  
  if (m_pSoundTouch)
  {
    m_bDiscardCurrentSample = true;
    m_pSoundTouch->flush();
    m_pSoundTouch->BeginFlush();
  }

  return hrBase;
}

HRESULT CMPAudioRenderer::EndFlush()
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);
  
  m_dSampleCounter = 0;
  m_rtNextSampleTime = 0;

  if (m_pSoundTouch)
  {
    m_pSoundTouch->EndFlush();
  }

  return CBaseRenderer::EndFlush(); 
}


// IMediaSeeking interface implementation

STDMETHODIMP CMPAudioRenderer::IsFormatSupported(const GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  // only seeking in time (REFERENCE_TIME units) is supported
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

STDMETHODIMP CMPAudioRenderer::QueryPreferredFormat(GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  *pFormat = TIME_FORMAT_MEDIA_TIME;
  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::SetTimeFormat(const GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);

  // nothing to set; just check that it's TIME_FORMAT_TIME
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : E_INVALIDARG;
}

STDMETHODIMP CMPAudioRenderer::IsUsingTimeFormat(const GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

STDMETHODIMP CMPAudioRenderer::GetTimeFormat(GUID* pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  *pFormat = TIME_FORMAT_MEDIA_TIME;
  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::GetDuration(LONGLONG* pDuration)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetStopPosition(LONGLONG* pStop)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetCurrentPosition(LONGLONG* pCurrent)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetCapabilities(DWORD* pCapabilities)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::CheckCapabilities(DWORD* pCapabilities)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::SetPositions(LONGLONG* pCurrent, DWORD CurrentFlags, LONGLONG * pStop, DWORD StopFlags)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetPositions(LONGLONG* pCurrent, LONGLONG* pStop)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::SetRate(double dRate)
{
  CAutoLock cInterfaceLock(&m_InterfaceLock);
  CAutoLock cRenderThreadLock(&m_RenderThreadLock);

  if (m_dRate != dRate && dRate == 1.0)
  {
    if (m_pAudioClient && m_bIsAudioClientStarted) 
    {
      HRESULT hr = S_OK;
    
      m_pAudioClient->Stop();
      hr = m_pAudioClient->Reset();
      m_bIsAudioClientStarted = false;

      if (hr != S_OK)
      {
        Log("SetRate - m_pAudioClient reset failed with (0x%08x)", hr);
      }
    }
  
    m_bDiscardCurrentSample = true;
    m_pSoundTouch->flush();
    m_pSoundTouch->BeginFlush();
    m_pSoundTouch->EndFlush();
  }
  
  m_dRate = dRate;
  return S_OK;
}

STDMETHODIMP CMPAudioRenderer::GetRate(double* pdRate)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPAudioRenderer::GetPreroll(LONGLONG *pPreroll)
{
  return E_NOTIMPL;
}

// IAVSyncClock interface implementation

HRESULT CMPAudioRenderer::AdjustClock(DOUBLE pAdjustment)
{
  CAutoLock cAutoLock(&m_csResampleLock);
  
  if (m_bUseTimeStretching)
  {
    m_dAdjustment = pAdjustment;
    m_Clock.SetAdjustment(m_dAdjustment);
    if (m_pSoundTouch)
    {
      m_pSoundTouch->setTempo(m_dAdjustment * m_dBias);
    }
    return S_OK;
  }
  else
  {
    return S_FALSE;
  }
}

HRESULT CMPAudioRenderer::SetBias(DOUBLE pBias)
{
  CAutoLock cAutoLock(&m_csResampleLock);

  if (m_bUseTimeStretching)
  {
    Log("SetBias: %1.10f", pBias);

    m_dBias = pBias;
    m_Clock.SetBias(m_dBias);
    if (m_pSoundTouch)
    {
      m_pSoundTouch->setTempo(m_dAdjustment * m_dBias);
      Log("SetBias - updated SoundTouch tempo");
    }
    return S_OK;
  }
  else
  {
    Log("SetBias: %1.10f - failed, time stretching is disabled", pBias);
    return S_FALSE;  
  }
}

HRESULT CMPAudioRenderer::GetBias(DOUBLE* pBias)
{
  *pBias = m_Clock.Bias();
  return S_OK;
}

