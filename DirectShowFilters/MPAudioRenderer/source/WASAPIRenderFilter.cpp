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
#include "WASAPIRenderFilter.h"
#include "TimeSource.h"

#include "alloctracing.h"

#include <FunctionDiscoveryKeys_devpkey.h>

extern HRESULT CopyWaveFormatEx(WAVEFORMATEX** dst, const WAVEFORMATEX* src);

extern void Log(const char* fmt, ...);
extern void LogWaveFormat(const WAVEFORMATEX* pwfx, const char *text);
extern void SetThreadName(DWORD dwThreadID, char* threadName);

CWASAPIRenderFilter::CWASAPIRenderFilter(AudioRendererSettings* pSettings) :
  m_pSettings(pSettings),
  m_hLibAVRT(NULL),
  m_pMMDevice(NULL),
  m_pAudioClient(NULL),
  m_pRenderClient(NULL),
  m_nFramesInBuffer(0),
  m_hTask(NULL),
  m_nBufferSize(0),
  m_hDataEvent(NULL),
  m_pAudioClock(NULL),
  m_nHWfreq(0),
  m_dwStreamFlags(AUDCLNT_STREAMFLAGS_EVENTCALLBACK),
  m_state(StateStopped),
  m_bIsAudioClientStarted(false)
{
  OSVERSIONINFO osvi;
  ZeroMemory(&osvi, sizeof(OSVERSIONINFO));
  osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);

  GetVersionEx(&osvi);
  bool bWASAPIAvailable = osvi.dwMajorVersion > 5;

  if (!bWASAPIAvailable)
    Log("Disabling WASAPI - OS version earlier than Vista detected");

  // Load Vista specifics DLLs
  m_hLibAVRT = LoadLibrary ("AVRT.dll");
  if (m_hLibAVRT && bWASAPIAvailable)
  {
    pfAvSetMmThreadCharacteristicsW   = (PTR_AvSetMmThreadCharacteristicsW)	GetProcAddress (m_hLibAVRT, "AvSetMmThreadCharacteristicsW");
    pfAvRevertMmThreadCharacteristics	= (PTR_AvRevertMmThreadCharacteristics)	GetProcAddress (m_hLibAVRT, "AvRevertMmThreadCharacteristics");
  }
  else
    pSettings->m_bUseWASAPI = false;	// WASAPI not available below Vista

  if (pSettings->m_bUseWASAPI)
  {
    IMMDeviceCollection* devices = NULL;
    GetAvailableAudioDevices(&devices, true);
    SAFE_RELEASE(devices); // currently only log available devices
  }
}

CWASAPIRenderFilter::~CWASAPIRenderFilter(void)
{
  Log("CWASAPIRenderFilter - destructor - instance 0x%x", this);
  
  FreeLibrary(m_hLibAVRT);

  Log("CWASAPIRenderFilter - destructor - instance 0x%x - end", this);
}

//Initialization
HRESULT CWASAPIRenderFilter::Init()
{
  if (!m_pSettings->m_bUseWASAPI)
    return S_FALSE;

  if (m_pSettings->m_WASAPIUseEventMode)
  {
    // Using HW DMA buffer based event notification
    m_hDataEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
    m_dwStreamFlags = AUDCLNT_STREAMFLAGS_EVENTCALLBACK;
  }
  else
  {
    // Using rendering thread polling
    m_hDataEvent = CreateWaitableTimer(NULL, TRUE, NULL);
    m_dwStreamFlags = 0;
  }

  m_hDataEvents.push_back(m_hDataEvent);
  m_hDataEvents.push_back(m_hStopThreadEvent);

  m_dwDataWaitObjects.push_back(MPAR_S_NEED_DATA);
  m_dwDataWaitObjects.push_back(MPAR_S_THREAD_STOPPING);

  m_hSampleEvents.push_back(m_hInputAvailableEvent);
  m_hSampleEvents.push_back(m_hStopThreadEvent);

  m_dwSampleWaitObjects.push_back(S_OK);
  m_dwSampleWaitObjects.push_back(MPAR_S_THREAD_STOPPING);

  m_hOOBCommandEvents.push_back(m_hStopThreadEvent);
  m_hOOBCommandEvents.push_back(m_hOOBCommandAvailableEvent);

  m_dwOOBCommandWaitObjects.push_back(MPAR_S_THREAD_STOPPING);
  m_dwOOBCommandWaitObjects.push_back(MPAR_S_OOB_COMMAND_AVAILABLE);

  ResetClockData();

  return CQueuedAudioSink::Init();
}

HRESULT CWASAPIRenderFilter::Cleanup()
{
  HRESULT hr = CQueuedAudioSink::Cleanup();

  SAFE_RELEASE(m_pAudioClock);
  SAFE_RELEASE(m_pRenderClient);
  SAFE_RELEASE(m_pAudioClient);
  SAFE_RELEASE(m_pMMDevice);

  if (m_hDataEvent)
    CloseHandle(m_hDataEvent);

  return hr;
}

// Format negotiation
HRESULT CWASAPIRenderFilter::NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // check always from the renderer device?
  if (FormatsEqual(pwfx, m_pInputFormat))
    return S_OK;

  bool bApplyChanges = nApplyChangesDepth != 0;

  //if (!bApplyChanges)
  //  return S_OK;

  Log("CWASAPIRenderFilter::NegotiateFormat");
  LogWaveFormat(pwfx, "CWASAPIRenderFilter::NegotiateFormat");

  HRESULT hr = VFW_E_CANNOT_CONNECT;

//  if (pwfx->wBitsPerSample != 16)
//    return VFW_E_CANNOT_CONNECT;

  if (!m_pAudioClient) 
  {
    if (!m_pMMDevice) 
      hr = GetAudioDevice(&m_pMMDevice);

    if (SUCCEEDED(hr))
    {
      hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
      if (FAILED(hr))
      {
        Log("CWASAPIRenderFilter::NegotiateFormat Error, audio client not loaded");
        return VFW_E_CANNOT_CONNECT;
      }
    }
  }

  WAVEFORMATEX *pwfxCM = NULL;
  const WAVEFORMATEX *pwfxAccepted = NULL;
  WAVEFORMATEX* tmpPwfx = NULL; 
  hr = m_pAudioClient->IsFormatSupported(m_pSettings->m_WASAPIShareMode, pwfx, &pwfxCM);
  if (hr != S_OK)
  {
    CopyWaveFormatEx(&tmpPwfx, pwfx);
    tmpPwfx->cbSize = 0;

    hr = m_pAudioClient->IsFormatSupported(m_pSettings->m_WASAPIShareMode, tmpPwfx, &pwfxCM);
    if (hr != S_OK)
    {
      Log("CWASAPIRenderFilter::NegotiateFormat WASAPI client refused the format: (0x%08x)", hr);
      LogWaveFormat(pwfxCM, "Closest match would be" );
      SAFE_DELETE_WAVEFORMATEX(tmpPwfx);
      CoTaskMemFree(pwfxCM);
      return VFW_E_TYPE_NOT_ACCEPTED;
    }

    pwfxAccepted = tmpPwfx;
  }
  else
    pwfxAccepted = pwfx;
  Log("CWASAPIRenderFilter::NegotiateFormat WASAPI client accepted the format");

  if (bApplyChanges)
  {
    // Stop and discard audio client
    StopAudioClient(&m_pAudioClient);
    SAFE_RELEASE(m_pRenderClient);
    SAFE_RELEASE(m_pAudioClock); // locking might be needed
    SAFE_RELEASE(m_pAudioClient);

    SetInputFormat(pwfxAccepted);
    // Reinitialize audio client
    hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
    if (SUCCEEDED (hr)) 
    {
      hr = InitAudioClient(m_pInputFormat, &m_pRenderClient);
    }
  }
  SAFE_DELETE_WAVEFORMATEX(tmpPwfx);

  return hr;
}

HRESULT CWASAPIRenderFilter::EndOfStream()
{
  // Queue an EOS marker so that it gets processed in 
  // the same thread as the audio data.
  PutSample(NULL);
  // wait until input queue is empty
  //if(m_hInputQueueEmptyEvent)
  //  WaitForSingleObject(m_hInputQueueEmptyEvent, END_OF_STREAM_FLUSH_TIMEOUT); // TODO make this depend on the amount of data in the queue
  return S_OK;
}

HRESULT CWASAPIRenderFilter::AudioClock(ULONGLONG& pTimestamp, ULONGLONG& pQpc)
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

void CWASAPIRenderFilter::UpdateAudioClock()
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

void CWASAPIRenderFilter::ResetClockData()
{
  CAutoLock cAutoLock(&m_csClockLock);
  Log("WASAPIRenderer::ResetClockData");
  m_dClockPosIn = 0;
  m_dClockPosOut = 0;
  m_dClockDataCollectionCount = 0;
  ZeroMemory((void*)&m_ullHwClock, sizeof(UINT64) * CLOCK_DATA_SIZE);
  ZeroMemory((void*)&m_ullHwQpc, sizeof(UINT64) * CLOCK_DATA_SIZE);
}

REFERENCE_TIME CWASAPIRenderFilter::Latency()
{
  return m_pSettings->m_hnsPeriod;
}

// Processing
DWORD CWASAPIRenderFilter::ThreadProc()
{
  Log("CWASAPIRenderFilter::Render thread - starting up - thread ID: %d", m_ThreadId);
  
  SetThreadName(-1, "WASAPI-renderer");

  // Polling delay
  LARGE_INTEGER liDueTime; 
  liDueTime.QuadPart = -1LL;

  AudioSinkCommand command;

  CComPtr<IMediaSample> sample;
  UINT32 sampleOffset = 0;
  UINT32 writeSilence = 0;

  EnableMMCSS();

  HRESULT hr = GetNextSampleOrCommand(&command, &sample.p, INFINITE, &m_hSampleEvents, &m_dwSampleWaitObjects);

  StartAudioClient(&m_pAudioClient);
  m_state = StateRunning;

  while(true)
  {
    hr = WaitForEvents(INFINITE, &m_hDataEvents, &m_dwDataWaitObjects);

    if (hr == MPAR_S_THREAD_STOPPING) // exit event
    {
      Log("CWASAPIRenderFilter::Render thread - closing down - thread ID: %d", m_ThreadId);
      StopAudioClient(&m_pAudioClient);
      RevertMMCSS();
      return 0;
    }
    else if (hr == MPAR_S_NEED_DATA) // data event
    {
      UpdateAudioClock();

      DWORD bufferFlags = 0;

      if (!sample && writeSilence == 0 && m_state == StateRunning)
        hr = GetNextSampleOrCommand(&command, &sample.p, INFINITE, &m_hSampleEvents, &m_dwSampleWaitObjects);
      else if (m_state == StatePaused && writeSilence == 0)
        hr = GetNextSampleOrCommand(&command, &sample.p, INFINITE, &m_hOOBCommandEvents, &m_dwOOBCommandWaitObjects);

      if (hr == MPAR_S_THREAD_STOPPING)
      {
        Log("CWASAPIRenderFilter::Render thread - closing down - thread ID: %d", m_ThreadId);
        StopAudioClient(&m_pAudioClient);
        RevertMMCSS();
        return 0;
      }
      else if (command == ASC_Resume)
      {
        m_state = StateRunning;
        bufferFlags = 0;
        writeSilence = false;
      }

      UINT32 bufferSize = 0;
      UINT32 currentPadding = 0;
      BYTE* data = NULL;
        
      m_pAudioClient->GetBufferSize(&bufferSize);
    
      // In exclusive mode with even based buffer filling we threat the padding as zero 
      // -> it will make rest of the code a bit cleaner
      if (m_pSettings->m_WASAPIShareMode == AUDCLNT_SHAREMODE_SHARED || !m_pSettings->m_WASAPIUseEventMode)
        m_pAudioClient->GetCurrentPadding(&currentPadding);

      UINT32 bufferSizeInBytes = (bufferSize - currentPadding) * m_pInputFormat->nBlockAlign;

      hr = m_pRenderClient->GetBuffer(bufferSize - currentPadding, &data);
      if (SUCCEEDED(hr))
      {
        if (writeSilence > 0 || !sample)
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
              HRESULT result = GetNextSampleOrCommand(&command, &sample.p, INFINITE, &m_hSampleEvents, &m_dwSampleWaitObjects);
              if (hr == MPAR_S_THREAD_STOPPING) // exit event
              {
                Log("CWASAPIRenderFilter::Render thread - closing down - thread ID: %d", m_ThreadId);
                StopAudioClient(&m_pAudioClient);
                RevertMMCSS();
                return 0;
              }
              else if (FAILED(result))
              {
                Log("CWASAPIRenderFilter::Render thread: Buffer underrun, fetching sample failed (0x%08x)", result);
                if (bytesCopied == 0)
                  bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
                break;
              }
              else if (command == ASC_Pause)
              {
                bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
                m_state = StatePaused;
                writeSilence = 2;
                sample.Release();
                sample = NULL;
                break;
              }

              // TODO: is this even possible? GetNextSampleOrCommand should fail with some code
              /*else if (!sample)
              {
                Log("WASAPIRenderer::Render thread: Buffer underrun, no new samples available!");  
                if (bytesCopied == 0)
                  bufferFlags = AUDCLNT_BUFFERFLAGS_SILENT;
                break;
              }*/

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
          Log("CWASAPIRenderFilter::Render thread: ReleaseBuffer failed (0x%08x)", hr);

        if (bufferFlags == AUDCLNT_BUFFERFLAGS_SILENT && writeSilence > 0)
          writeSilence--;
      }

      if (!m_pSettings->m_WASAPIUseEventMode)
      {
        if (m_pAudioClient)
          hr = m_pAudioClient->GetCurrentPadding(&currentPadding);
        else
          hr = S_FALSE;

        if (SUCCEEDED(hr) && bufferSize > 0)
        {
          liDueTime.QuadPart = (double)currentPadding / (double)bufferSize * (double)m_pSettings->m_hnsPeriod * -0.9;
          // Log(" currentPadding: %d QuadPart: %lld", currentPadding, liDueTime.QuadPart);
        }
        else
        {
          liDueTime.QuadPart = (double)m_pSettings->m_hnsPeriod * -0.9;
          if (hr != AUDCLNT_E_NOT_INITIALIZED)
            Log("CWASAPIRenderFilter::Render thread: GetCurrentPadding failed (0x%08x)", hr);  
        }
        SetWaitableTimer(m_hDataEvent, &liDueTime, 0, NULL, NULL, 0);
      }
    }
  }

  return 0;
}

// Internal implementation
HRESULT CWASAPIRenderFilter::EnableMMCSS()
{
  if (!m_hTask)
  {
    // Ask MMCSS to temporarily boost the thread priority
    // to reduce glitches while the low-latency stream plays.
    DWORD taskIndex = 0;

    if (pfAvSetMmThreadCharacteristicsW)
    {
      m_hTask = pfAvSetMmThreadCharacteristicsW(L"Pro Audio", &taskIndex);
      Log("CWASAPIRenderFilter::EnableMMCSS Putting thread in higher priority for WASAPI mode");
      
      if (!m_hTask)
        return HRESULT_FROM_WIN32(GetLastError());
    }
  }
  return S_OK;
}

HRESULT CWASAPIRenderFilter::RevertMMCSS()
{
  if (m_hTask && pfAvRevertMmThreadCharacteristics)
  {
    if (pfAvRevertMmThreadCharacteristics(m_hTask))
      return S_OK;
    else
      return HRESULT_FROM_WIN32(GetLastError());
  }
  return S_FALSE; // failed since no thread had been boosted
}

HRESULT CWASAPIRenderFilter::GetAudioDevice(IMMDevice **ppMMDevice)
{
  Log("CWASAPIRenderFilter::GetAudioDevice");

  CComPtr<IMMDeviceEnumerator> enumerator;
  IMMDeviceCollection* devices;
  HRESULT hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

  if (hr != S_OK)
  {
    Log("  failed to create MMDeviceEnumerator!");
    return hr;
  }

  Log("Target end point: %S", m_pSettings->m_wWASAPIPreferredDeviceId);

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
          if (wcscmp(pwszID, m_pSettings->m_wWASAPIPreferredDeviceId) == 0)
          {
            enumerator->GetDevice(m_pSettings->m_wWASAPIPreferredDeviceId, ppMMDevice); 
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

HRESULT CWASAPIRenderFilter::GetAvailableAudioDevices(IMMDeviceCollection** ppMMDevices, bool pLog)
{
  HRESULT hr;

  CComPtr<IMMDeviceEnumerator> enumerator;
  Log("CWASAPIRenderFilter::GetAvailableAudioDevices");
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
        Log("  supports pull mode: %d", eventDriven.intVal);
      else
        Log("  pull mode query failed!");

      if (pProps->GetValue(PKEY_AudioEndpoint_PhysicalSpeakers, &speakerMask) == S_OK)
        Log("  speaker mask: %d", speakerMask.uintVal);
      else
        Log("  PhysicalSpeakers query failed!");

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

HRESULT CWASAPIRenderFilter::GetBufferSize(const WAVEFORMATEX *pWaveFormatEx, REFERENCE_TIME *pHnsBufferPeriod)
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

  Log("CWASAPIRenderFilter::GetBufferSize set a %lld period for a %ld buffer size",*pHnsBufferPeriod, m_nBufferSize);

  return S_OK;
}


HRESULT CWASAPIRenderFilter::CreateAudioClient(IMMDevice *pMMDevice, IAudioClient **ppAudioClient)
{
  HRESULT hr = S_OK;

  Log("WASAPIRenderFilter::CreateAudioClient");

  // AudioClient needs to be stopped beforehand
  //StopAudioClient(ppAudioClient);

  if (!pMMDevice)
  {
    Log("WASAPIRenderFilter::CreateAudioClient failed, device not loaded");
    return E_FAIL;
  }

  hr = pMMDevice->Activate(__uuidof(IAudioClient), CLSCTX_ALL, NULL, reinterpret_cast<void**>(ppAudioClient));
  if (FAILED(hr))
    Log("WASAPIRenderFilter::CreateAudioClient activation failed (0x%08x)", hr);
  else
    Log("WASAPIRenderFilter::CreateAudioClient success");

  return hr;
}

HRESULT CWASAPIRenderFilter::StartAudioClient(IAudioClient** ppAudioClient)
{
  HRESULT hr = S_OK;
  if (!m_bIsAudioClientStarted)
  {
    Log("WASAPIRenderFilter::StartAudioClient");

    if ((*ppAudioClient))
    {
      hr = (*ppAudioClient)->Start();
      if (FAILED(hr))
      {
        m_bIsAudioClientStarted = false;
        Log("  start failed (0x%08x)", hr);
      }
      else
        m_bIsAudioClientStarted = true;
    }
    else
      return E_POINTER;
  }
  else
  {
    Log("WASAPIRenderFilter::StartAudioClient - ignored, already started"); 
    return hr;
  }

  if (!m_pSettings->m_WASAPIUseEventMode)
  {
    LARGE_INTEGER liDueTime;
    liDueTime.QuadPart = 0LL;
  
    CancelDataEvent();

    // We need to manually start the rendering thread when in polling mode
    SetWaitableTimer(m_hDataEvent, &liDueTime, 0, NULL, NULL, 0);
  }

  return hr;
}

void CWASAPIRenderFilter::CancelDataEvent()
{
  if (!m_pSettings->m_WASAPIUseEventMode)
  {
    HRESULT hr = CancelWaitableTimer(m_hDataEvent);
    if (FAILED(hr))
      Log("WASAPIRenderFilter::CancelDataEvent - CancelWaitableTimer failed: (0x%08x)", hr);
  }
}

HRESULT CWASAPIRenderFilter::StopAudioClient(IAudioClient** ppAudioClient)
{
  HRESULT hr = S_OK;
  if (m_bIsAudioClientStarted)
  {
    Log("WASAPIRenderFilter::StopAudioClient");

    m_bIsAudioClientStarted = false;

    if (*ppAudioClient)
    {
      // Let the current audio buffer to be played completely.
      // Some amplifiers will "cache" the incomplete AC3 packets and that causes issues
      // when the next AC3 packets are received
      //  WaitForSingleObject(m_hDataEvent, Latency() / 10000);
      
      hr = (*ppAudioClient)->Stop();
      if (FAILED(hr))
        Log("   stop failed (0x%08x)", hr);

      hr = (*ppAudioClient)->Reset();
      if (FAILED(hr))
        Log("   reset failed (0x%08x)", hr);
    }
  }
  return hr;
}

HRESULT CWASAPIRenderFilter::InitAudioClient(const WAVEFORMATEX *pWaveFormatEx, IAudioRenderClient **ppRenderClient)
{
  Log("WASAPIRenderFilter::InitAudioClient");
  HRESULT hr = S_OK;
  
  if (m_pSettings->m_hnsPeriod == 0 || m_pSettings->m_hnsPeriod == 1)
  {
    REFERENCE_TIME defaultPeriod(0);
    REFERENCE_TIME minimumPeriod(0);

    hr = m_pAudioClient->GetDevicePeriod(&defaultPeriod, &minimumPeriod);
    if (SUCCEEDED(hr))
    {
      if (m_pSettings->m_hnsPeriod == 0)
        m_pSettings->m_hnsPeriod = defaultPeriod;
      else
        m_pSettings->m_hnsPeriod = minimumPeriod;
      Log("WASAPIRenderFilter::InitAudioClient using device period from driver %I64u ms", m_pSettings->m_hnsPeriod / 10000);
    }
    else
    {
      Log("WASAPIRenderFilter::InitAudioClient failed to get device period from driver (0x%08x) - using 50 ms", hr); 
      m_pSettings->m_hnsPeriod = 500000; //50 ms is the best according to James @Slysoft
    }
  }

  if (!m_pAudioClient)
  {
    hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
    if (FAILED(hr))
    {
      Log("WASAPIRenderFilter::InitAudioClient failed to create audio client (0x%08x)", hr);
      return hr;
    }
    else
      Log("WASAPIRenderFilter::InitAudioClient created missing audio client");
  }

  WAVEFORMATEX *pwfxCM = NULL;
  hr = m_pAudioClient->IsFormatSupported(m_pSettings->m_WASAPIShareMode, pWaveFormatEx, &pwfxCM);    
  if (FAILED(hr))
    Log("WASAPIRenderFilter::InitAudioClient not supported (0x%08x)", hr);
  else
    Log("WASAPIRenderFilter::InitAudioClient format supported");

  GetBufferSize(pWaveFormatEx, &m_pSettings->m_hnsPeriod);

  if (SUCCEEDED(hr))
  {
    hr = m_pAudioClient->Initialize(m_pSettings->m_WASAPIShareMode, m_dwStreamFlags,
	                                m_pSettings->m_hnsPeriod, m_pSettings->m_hnsPeriod, pWaveFormatEx, NULL);
    
    // when rebuilding the graph between SD / HD zapping the .NET GC workaround
    // might call the init again. In that case just eat the error 
    // this needs to be fixed properly if .NET GC workaround is going to be the final solution...
    if (hr == AUDCLNT_E_ALREADY_INITIALIZED)
      return S_OK;
  }

  if (FAILED(hr) && hr != AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED)
  {
    Log("WASAPIRenderFilter::InitAudioClient Initialize failed (0x%08x)", hr);
    return hr;
  }

  if (hr == S_OK)
  {
    SAFE_RELEASE(m_pAudioClock);
    hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
    if (SUCCEEDED(hr))
      m_pAudioClock->GetFrequency(&m_nHWfreq);
    else
      Log("WASAPIRenderFilter::IAudioClock not found!");
  }

  if (hr == AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED) 
  {
    // if the buffer size was not aligned, need to do the alignment dance
    Log("WASAPIRenderFilter::InitAudioClient Buffer size not aligned. Realigning");

    // get the buffer size, which will be aligned
    hr = m_pAudioClient->GetBufferSize(&m_nFramesInBuffer);

    // throw away this IAudioClient
    SAFE_RELEASE(m_pAudioClient);

    // calculate the new aligned periodicity
    m_pSettings->m_hnsPeriod = // hns =
                  (REFERENCE_TIME)(
                  10000.0 * // (hns / ms) *
                  1000 * // (ms / s) *
                  m_nFramesInBuffer / // frames /
                  pWaveFormatEx->nSamplesPerSec  // (frames / s)
                  + 0.5 // rounding
    );

    if (SUCCEEDED(hr)) 
      hr = CreateAudioClient(m_pMMDevice, &m_pAudioClient);
      
    Log("WASAPIRenderFilter::InitAudioClient Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames", m_pSettings->m_hnsPeriod, m_nFramesInBuffer);

    if (SUCCEEDED (hr)) 
      hr = m_pAudioClient->Initialize(m_pSettings->m_WASAPIShareMode, m_dwStreamFlags, 
	                                    m_pSettings->m_hnsPeriod, m_pSettings->m_hnsPeriod, pWaveFormatEx, NULL);
 
    if (FAILED(hr))
    {
      Log("WASAPIRenderFilter::InitAudioClient Failed to reinitialize the audio client");
      return hr;
    }
    else
    {
      SAFE_RELEASE(m_pAudioClock);
      hr = m_pAudioClient->GetService(__uuidof(IAudioClock), (void**)&m_pAudioClock);
      if (FAILED(hr))
        Log("WASAPIRenderFilter::IAudioClock not found!");
      else
        m_pAudioClock->GetFrequency(&m_nHWfreq);
    }
  } // if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) 

  // get the buffer size, which is aligned
  if (SUCCEEDED(hr)) 
    hr = m_pAudioClient->GetBufferSize(&m_nFramesInBuffer);

  // calculate the new period
  if (SUCCEEDED (hr)) 
    hr = m_pAudioClient->GetService(__uuidof(IAudioRenderClient), (void**)(ppRenderClient));

  if (FAILED(hr))
    Log("WASAPIRenderFilter::InitAudioClient service initialization failed (0x%08x)", hr);
  else
    Log("WASAPIRenderer::InitAudioClient service initialization success");

  if (m_pSettings->m_WASAPIUseEventMode)
  {
    hr = m_pAudioClient->SetEventHandle(m_hDataEvent);
    if (FAILED(hr))
    {
      Log("WASAPIRenderFilter::InitAudioClient SetEventHandle failed (0x%08x)", hr);
      return hr;
    }
  }

  REFERENCE_TIME latency(0);
  m_pAudioClient->GetStreamLatency(&latency);
  
  Log("WASAPIRenderFilter::InitAudioClient device reported latency %I64u ms - buffer based latency %I64u ms", 
    latency / 10000, Latency() / 10000);

  // Dynamic format change requires restart for the audio client
  if (m_state != StateStopped)
    StartAudioClient(&m_pAudioClient);

  return hr;
}

