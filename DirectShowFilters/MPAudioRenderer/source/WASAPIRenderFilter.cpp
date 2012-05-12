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

#include "stdafx.h"
#include "Globals.h"
#include "WASAPIRenderFilter.h"
#include "TimeSource.h"

#include "alloctracing.h"

CWASAPIRenderFilter::CWASAPIRenderFilter(AudioRendererSettings* pSettings, CSyncClock* pClock) :
  m_pSettings(pSettings),
  m_pClock(pClock),
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
  m_bIsAudioClientStarted(false),
  m_bDeviceInitialized(false),
  m_rtNextSampleTime(0),
  m_rtHwStart(0),
  m_nSampleOffset(0),
  m_nDataLeftInSample(0),
  m_bResyncHwClock(true),
  m_llPosError(0),
  m_ullPrevQpc(0),
  m_ullPrevPos(0)
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
    pSettings->GetAvailableAudioDevices(&devices, NULL, true);
    SAFE_RELEASE(devices); // currently only log available devices
  }
}

CWASAPIRenderFilter::~CWASAPIRenderFilter(void)
{
  Log("CWASAPIRenderFilter - destructor - instance 0x%x", this);
  
  SetEvent(m_hStopThreadEvent);
  WaitForSingleObject(m_hThread, INFINITE);

  CAutoLock lock(&m_csResources);
  FreeLibrary(m_hLibAVRT);

  if (m_hDataEvent)
  {
    CloseHandle(m_hDataEvent);
    m_hDataEvent = NULL;
  }

  Log("CWASAPIRenderFilter - destructor - instance 0x%x - end", this);
}

//Initialization
HRESULT CWASAPIRenderFilter::Init()
{
  if (!m_pSettings->m_bUseWASAPI)
    return S_FALSE;

  if (m_pSettings->m_bWASAPIUseEventMode)
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

  m_hDataEvents.push_back(m_hStopThreadEvent);
  m_hDataEvents.push_back(m_hDataEvent);

  m_dwDataWaitObjects.push_back(MPAR_S_THREAD_STOPPING);
  m_dwDataWaitObjects.push_back(MPAR_S_NEED_DATA);

  m_hSampleEvents.push_back(m_hStopThreadEvent);
  m_hSampleEvents.push_back(m_hOOBCommandAvailableEvent);
  m_hSampleEvents.push_back(m_hInputAvailableEvent);

  m_dwSampleWaitObjects.push_back(MPAR_S_THREAD_STOPPING);
  m_dwSampleWaitObjects.push_back(MPAR_S_OOB_COMMAND_AVAILABLE);
  m_dwSampleWaitObjects.push_back(S_OK);

  ResetClockData();

  return CQueuedAudioSink::Init();
}

HRESULT CWASAPIRenderFilter::Cleanup()
{
  HRESULT hr = CQueuedAudioSink::Cleanup();

  CAutoLock lock(&m_csResources);
  ReleaseResources();

  return hr;
}

void CWASAPIRenderFilter::ReleaseResources()
{
  m_bDeviceInitialized = false;
  StopAudioClient();
  SAFE_RELEASE(m_pAudioClock);
  SAFE_RELEASE(m_pRenderClient);
  SAFE_RELEASE(m_pAudioClient);
  SAFE_RELEASE(m_pMMDevice);
}

void CWASAPIRenderFilter::ReleaseDevice()
{
  Cleanup();
}

// Format negotiation
HRESULT CWASAPIRenderFilter::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

  if (FormatsEqual(pwfx, m_pInputFormat))
  {
    *pChOrder = m_chOrder;
    return S_OK;
  }

  bool bApplyChanges = nApplyChangesDepth != 0;

  bool bitDepthForced = (m_pSettings->m_nForceBitDepth != 0 && m_pSettings->m_nForceBitDepth != pwfx->Format.wBitsPerSample);
  bool sampleRateForced = (m_pSettings->m_nForceSamplingRate != 0 && m_pSettings->m_nForceSamplingRate != pwfx->Format.nSamplesPerSec);
  
  if ((bitDepthForced || sampleRateForced) &&
       pwfx->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL ||
       pwfx->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT && bitDepthForced)
    return VFW_E_TYPE_NOT_ACCEPTED;
  
  if (((bitDepthForced && m_pSettings->m_nForceBitDepth != pwfx->Format.wBitsPerSample) ||
       (sampleRateForced && m_pSettings->m_nForceSamplingRate != pwfx->Format.nSamplesPerSec)))
    return VFW_E_TYPE_NOT_ACCEPTED;

  CAutoLock lock(&m_csResources);

  HRESULT hr = CreateAudioClient();
  if (FAILED(hr))
  {
    Log("CWASAPIRenderFilter::NegotiateFormat Error, audio client not initialized: (0x%08x)", hr);
    return VFW_E_CANNOT_CONNECT;
  }

  WAVEFORMATEXTENSIBLE* pwfxAccepted = NULL;
  hr = IsFormatSupported(pwfx, &pwfxAccepted);
  if (FAILED(hr))
  {
    SAFE_DELETE_WAVEFORMATEX(pwfxAccepted);
    return hr;
  }

  if (bApplyChanges)
  {
    LogWaveFormat(pwfx, "REN - applying  ");

    // Stop and discard audio client
    StopAudioClient();
    SAFE_RELEASE(m_pRenderClient);
    SAFE_RELEASE(m_pAudioClock);
    SAFE_RELEASE(m_pAudioClient);

    // We must use incoming format so the WAVEFORMATEXTENSIBLE to WAVEFORMATEXT difference
    // that some audio drivers require is not causing an infonite loop of format changes
    SetInputFormat(pwfx);

    // Reinitialize audio client
    hr = CreateAudioClient(true);
  }
  else
    LogWaveFormat(pwfx, "Input format    ");

  m_chOrder = *pChOrder = DS_ORDER;
  SAFE_DELETE_WAVEFORMATEX(pwfxAccepted);

  return hr;
}

HRESULT CWASAPIRenderFilter::IsFormatSupported(const WAVEFORMATEXTENSIBLE* pwfx, WAVEFORMATEXTENSIBLE** pwfxAccepted)
{
  WAVEFORMATEXTENSIBLE* pwfxCM = NULL;
  WAVEFORMATEX* tmpPwfx = NULL;
  
  HRESULT hr = m_pAudioClient->IsFormatSupported(m_pSettings->m_WASAPIShareMode, (WAVEFORMATEX*)pwfx, (WAVEFORMATEX**)&pwfxCM);
  if (hr != S_OK)
  {
    CopyWaveFormatEx((WAVEFORMATEXTENSIBLE**)&tmpPwfx, pwfx);
    tmpPwfx->cbSize = 0;
    tmpPwfx->wFormatTag = WAVE_FORMAT_PCM;

    hr = m_pAudioClient->IsFormatSupported(m_pSettings->m_WASAPIShareMode, (WAVEFORMATEX*)tmpPwfx, (WAVEFORMATEX**)&pwfxCM);
    if (hr != S_OK)
    {
      Log("CWASAPIRenderFilter::NegotiateFormat WASAPI client refused the format: (0x%08x)", hr);
      LogWaveFormat(pwfxCM, "Closest match would be" );
      SAFE_DELETE_WAVEFORMATEX(tmpPwfx);
      CoTaskMemFree(pwfxCM);
      return VFW_E_TYPE_NOT_ACCEPTED;
    }

    ToWaveFormatExtensible(pwfxAccepted, tmpPwfx);
    (*pwfxAccepted)->Format.cbSize = 0;
    (*pwfxAccepted)->Format.wFormatTag = WAVE_FORMAT_PCM;

    SAFE_DELETE_WAVEFORMATEX(tmpPwfx);
  }
  else
    CopyWaveFormatEx(pwfxAccepted, pwfx);

  return hr;
}

HRESULT CWASAPIRenderFilter::CheckSample(IMediaSample* pSample, UINT32 framesToFlush)
{
  if (!pSample)
    return S_OK;

  AM_MEDIA_TYPE *pmt = NULL;
  bool bFormatChanged = false;
  
  HRESULT hr = S_OK;

  if (SUCCEEDED(pSample->GetMediaType(&pmt)) && pmt)
    bFormatChanged = !FormatsEqual((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, m_pInputFormat);

  if (bFormatChanged)
  {
    // Release outstanding buffer
    hr = m_pRenderClient->ReleaseBuffer(framesToFlush, 0);
    if (FAILED(hr))
      Log("CWASAPIRenderFilter::CheckFormat - ReleaseBuffer: 0x%08x", hr);

    // Apply format change
    ChannelOrder chOrder;
    hr = NegotiateFormat((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, 1, &chOrder);
    pSample->SetDiscontinuity(false);

    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("CWASAPIRenderFilter::CheckFormat failed to change format: 0x%08x", hr);
      return hr;
    }
    else
    {
      m_chOrder = chOrder;
      return S_FALSE;
    }
  }
  else if (pSample->IsDiscontinuity() == S_OK)
  {
    hr = m_pRenderClient->ReleaseBuffer(framesToFlush, 0);
    if (FAILED(hr))
      Log("CWASAPIRenderFilter::CheckFormat - discontinuity - ReleaseBuffer: 0x%08x", hr);
    
    pSample->SetDiscontinuity(false);
    m_nSampleNum = 0;
    return S_FALSE;
  }

  return S_OK;
}

HRESULT CWASAPIRenderFilter::CheckStreamTimeline(IMediaSample* pSample, REFERENCE_TIME* pDueTime, UINT32 sampleOffset)
{
  *pDueTime = 0;

  if (!pSample)
    return S_FALSE;

  REFERENCE_TIME rtHWTime = 0;
  REFERENCE_TIME rtRefClock = 0;
  REFERENCE_TIME rtStop = 0;
  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtDuration = 0;

  bool resync = false;

  HRESULT hr = pSample->GetTime(&rtStart, &rtStop);
  if (FAILED(hr))
  {
    // Render all samples flat that dont have presentation time
    m_nSampleNum++;
    return MPAR_S_RENDER_SAMPLE;
  }

  if (m_nSampleNum == 0)
    m_rtNextSampleTime = rtStart;

  long sampleLength = pSample->GetActualDataLength();

  UINT nFrames = sampleLength / m_pInputFormat->Format.nBlockAlign;
  rtDuration = nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;

  if (SUCCEEDED(m_pClock->GetHWTime(&rtRefClock, &rtHWTime)))
  {
    rtRefClock -= m_rtStart;
    rtHWTime -= m_rtHwStart;
  }
  else
  {
    m_nSampleNum++;
    return MPAR_S_RENDER_SAMPLE;
  }

  if (m_pSettings->m_bLogSampleTimes)
    Log("   sample start: %6.3f  stop: %6.3f dur: %6.3f diff: %6.3f rtHWTime: %6.3f rtRefClock: %6.3f early: %6.3f queue: %d %6.3f", 
      rtStart / 10000000.0, rtStop / 10000000.0, rtDuration / 10000000.0, (rtStart - m_rtNextSampleTime) / 10000000.0, 
      rtHWTime / 10000000.0, rtRefClock / 10000000.0, (rtStart - rtHWTime) / 10000000.0, m_inputQueue.size(), BufferredDataDuration() / 10000000.0);

  // Try to keep the A/V sync when data has been dropped
  if (abs(rtStart - m_rtNextSampleTime) > MAX_SAMPLE_TIME_ERROR)
  {
    resync = true;
    Log("   Discontinuity detected: diff: %7.3f ms MAX_SAMPLE_TIME_ERROR: %7.3f ms resync: %d", ((double)rtStart - (double)m_rtNextSampleTime) / 10000.0, (double)MAX_SAMPLE_TIME_ERROR / 10000.0, resync);
  }

  m_rtNextSampleTime = rtStart + rtDuration;

  REFERENCE_TIME offsetDelay = 0;
  if (sampleOffset > 0)
    offsetDelay = sampleOffset / m_pInputFormat->Format.nBlockAlign * UNITS / m_pInputFormat->Format.nSamplesPerSec;

  *pDueTime = rtStart + offsetDelay;

  if (*pDueTime < rtHWTime - Latency())
  {
    // TODO implement partial sample dropping
    Log("   dropping late sample - pDueTime: %6.3f rtHWTime: %6.3f", *pDueTime / 10000000.0, rtHWTime / 10000000.0);
    m_nSampleNum = 0;

    return MPAR_S_DROP_SAMPLE;
  }
  else if ((m_nSampleNum == 0 && *pDueTime > rtHWTime) || resync)
  {
    m_nSampleNum++;

    if (m_pSettings->m_bLogSampleTimes)
      Log("   MPAR_S_WAIT_RENDER_TIME - %6.3f", *pDueTime / 10000000.0);

    return MPAR_S_WAIT_RENDER_TIME;
  }

  m_nSampleNum++;

  return MPAR_S_RENDER_SAMPLE;
}

void CWASAPIRenderFilter::CalculateSilence(REFERENCE_TIME* pDueTime, LONGLONG* pBytesOfSilence)
{
  REFERENCE_TIME rtHWTime = 0;
  
  if (FAILED(m_pClock->GetHWTime(NULL, &rtHWTime)))
    return;

  rtHWTime -= m_rtHwStart;

  REFERENCE_TIME rtSilenceDuration = *pDueTime - rtHWTime;

  if (m_pSettings->m_bLogSampleTimes)
    Log("   calculateSilence: %6.3f pDueTime: %6.3f rtHWTime: %6.3f", 
      rtSilenceDuration / 10000000.0, *pDueTime / 10000000.0, rtHWTime / 10000000.0);

  if (rtSilenceDuration > 0)
  {
    UINT32 framesSilence = rtSilenceDuration / (UNITS / m_pInputFormat->Format.nSamplesPerSec);
    *pBytesOfSilence = framesSilence * m_pInputFormat->Format.nBlockAlign;
  }
  else
    *pBytesOfSilence = 0;
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

void CWASAPIRenderFilter::UpdateAudioClock()
{
  if (m_pAudioClock)
  {
    CAutoLock cAutoLock(&m_csClockLock);

    UINT64 timestamp = 0;
    UINT64 qpc = 0;
    HRESULT hr = m_pAudioClock->GetPosition(&timestamp, &qpc);
    if (hr != S_OK)
    {
      Log("UpdateAudioClock - error reading the position(1): (0x%08x)", hr);
    
      if (hr != S_FALSE)
        return;

      UINT32 loop = 0;

      do
      {
        hr = m_pAudioClock->GetPosition(&timestamp, &qpc);
        Log("UpdateAudioClock - error reading the position(2): (0x%08x)", hr);
        Sleep(1);
      } while (hr == S_FALSE && loop < 5);
      
      if (hr != S_OK)
      {
        Log("UpdateAudioClock - error reading the position(3): (0x%08x)", hr);
        return;
      }
    }

    UINT64 ullHwClock = cMulDiv64(timestamp, 10000000, m_nHWfreq);
    
    if (m_ullPrevPos > ullHwClock)
    {
      UINT64 correction = m_ullPrevPos - ullHwClock + qpc - m_ullPrevQpc;
      m_llPosError += correction;
      Log("UpdateAudioClock: prevPos: %6.3f > ullHwClock: %6.3f diff: %6.3f QPC diff: %6.3f m_llPosError: %6.3f correction: %6.3f", 
        m_ullPrevPos / 10000000.0, ullHwClock / 10000000.0, (m_ullPrevPos - ullHwClock) / 10000000.0, (qpc - m_ullPrevQpc) / 10000000.0, 
        m_llPosError / 10000000.0, correction / 10000000.0);
    }

    m_ullPrevPos = ullHwClock;
    m_ullPrevQpc = qpc;

    ullHwClock += m_llPosError;

    //Log("HW clock: %6.3f", ullHwClock / 10000000.0);

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
  m_llPosError = 0;
  m_ullPrevPos = 0;
  m_ullPrevQpc = 0;
  ZeroMemory((void*)&m_ullHwClock, sizeof(UINT64) * CLOCK_DATA_SIZE);
  ZeroMemory((void*)&m_ullHwQpc, sizeof(UINT64) * CLOCK_DATA_SIZE);
}

REFERENCE_TIME CWASAPIRenderFilter::Latency()
{
  return m_pSettings->m_hnsPeriod;
}

HRESULT CWASAPIRenderFilter::Run(REFERENCE_TIME rtStart)
{
  REFERENCE_TIME rtTime = 0;
  REFERENCE_TIME rtHwTime = 0;

  HRESULT hr = m_pClock->GetHWTime(&rtTime, &rtHwTime);

  if (SUCCEEDED(hr))
  {
    if (m_bResyncHwClock)
      m_rtHwStart = rtStart + (rtHwTime - rtTime);
    else
    {
      double currentBias = m_pClock->GetBias();
      REFERENCE_TIME biasBasedHwStart = rtStart / currentBias;

      double multiplier = (double)(rtTime - m_rtPauseTime) / (double)(rtHwTime - m_rtHwPauseTime);
      m_rtHwStart = rtStart / multiplier;

      Log("CWASAPIRenderFilter::Run - TEST: currentBias: %10.8f multiplier: %10.8f m_rtHwStart: %10.8f biasBasedHwStart: %10.8f diff: %10.8f",
        currentBias, multiplier, m_rtHwStart / 10000000.0, biasBasedHwStart / 10000000.0, (biasBasedHwStart - m_rtHwStart) / 10000000.0);
    }

    m_bResyncHwClock = false;
  }
  else
    Log("CWASAPIRenderFilter::Run - error (0x%08x)", hr);

  m_nSampleNum = 0;
  m_filterState = State_Running;

  return CQueuedAudioSink::Run(rtStart);
}

HRESULT CWASAPIRenderFilter::Pause()
{
  m_filterState = State_Paused;
  m_pClock->GetHWTime(&m_rtPauseTime, &m_rtHwPauseTime);

  return CQueuedAudioSink::Pause();
}

HRESULT CWASAPIRenderFilter::BeginStop()
{
  m_bResyncHwClock = true;
  m_filterState = State_Stopped;
  return CQueuedAudioSink::BeginStop();
}

// Processing
HRESULT CWASAPIRenderFilter::BeginFlush()
{
  m_bResyncHwClock = true;
  return CQueuedAudioSink::BeginFlush();
}

HRESULT CWASAPIRenderFilter::EndFlush()
{
  return CQueuedAudioSink::EndFlush();
}

HRESULT CWASAPIRenderFilter::PutSample(IMediaSample* pSample)
{
 HRESULT hr = CQueuedAudioSink::PutSample(pSample);

  if (m_filterState != State_Running)
    Log("Buffering...%6.3f", BufferredDataDuration() / 10000000.0);

  return hr;
}

DWORD CWASAPIRenderFilter::ThreadProc()
{
  Log("CWASAPIRenderFilter::Render thread - starting up - thread ID: %d", m_ThreadId);
  
  SetThreadName(0, "WASAPI-renderer");

  // Polling delay
  LARGE_INTEGER liDueTime; 
  liDueTime.QuadPart = -1LL;

  AudioSinkCommand command;
  
  LONGLONG writeSilence = 0;
  BYTE* sampleData = NULL;

  bool flush = false;
  bool sampleProcessed = false;

  REFERENCE_TIME dueTime = 0;
  REFERENCE_TIME maxSampleWaitTime = Latency() / 20000;

  HRESULT hr = S_FALSE;

  m_csResources.Lock();

  if (m_pSettings->m_bReleaseDeviceOnStop && !m_pAudioClient && m_pInputFormat)
  {
    hr = CreateAudioClient(true);
    if (FAILED(hr))
    {
      Log("CWASAPIRenderFilter::Render thread Error, audio client not available: (0x%08x)", hr);
      StopRenderThread();
      m_csResources.Unlock();
      return 0;
    }
  }

  if (m_pAudioClient)
  {
    hr = StartAudioClient();
    if (FAILED(hr))
    {
      Log("CWASAPIRenderFilter::Render thread Error, starting audio client failed: (0x%08x)", hr);
      StopRenderThread();
      m_csResources.Unlock();
      return 0;
    }
  }

  if (!m_bDeviceInitialized)
  {
    Log("CWASAPIRenderFilter::Render thread Error, device not initialized");
    StopRenderThread();
    m_csResources.Unlock();
    return 0;
  }

  EnableMMCSS();
  m_state = StateRunning;

  while (true)
  {
    if (flush)
    {
      Log("CWASAPIRenderFilter::Render thread flushing buffers");
      HandleFlush();
      flush = false;
    }
    
    m_csResources.Unlock();
    hr = WaitForEvents(INFINITE, &m_hDataEvents, &m_dwDataWaitObjects);
    m_csResources.Lock();

    if (hr == MPAR_S_THREAD_STOPPING || !m_pAudioClient)
    {
      StopRenderThread();
      return 0;
    }
    else if (hr == MPAR_S_NEED_DATA)
    {
      UpdateAudioClock();

      UINT32 bytesFilled = 0;
      UINT32 bufferSize = 0;
      UINT32 currentPadding = 0;
      UINT32 bufferSizeInBytes = 0;
      BYTE* data = NULL;
      DWORD flags = 0;

      static BYTE* prevData = NULL;
       
      hr = GetWASAPIBuffer(bufferSize, currentPadding, bufferSizeInBytes, &data);
      if (SUCCEEDED(hr))
      {
        do
        {
          fetchSample:

          bool OOBCommandOnly = m_nDataLeftInSample > 0;

          if (m_nDataLeftInSample == 0 || OOBCommandOnly)
          {
            m_csResources.Unlock();
            HRESULT result = GetNextSampleOrCommand(&command, &m_pCurrentSample.p, maxSampleWaitTime, &m_hSampleEvents,
                                                    &m_dwSampleWaitObjects, OOBCommandOnly);
            m_csResources.Lock();

            if (result == MPAR_S_THREAD_STOPPING || !m_pAudioClient)
            {
              if (m_pAudioClient)
              {
                hr = m_pRenderClient->ReleaseBuffer(bufferSize - currentPadding, flags);
                if (FAILED(hr) && hr != AUDCLNT_E_OUT_OF_ORDER)
                  Log("CWASAPIRenderFilter::Render thread: ReleaseBuffer failed (0x%08x)", hr);
              }

              StopRenderThread();
              return 0;
            }

            if (!m_pCurrentSample)
              m_nDataLeftInSample = 0;
              
            if (command == ASC_PutSample && m_pCurrentSample)
            {
              sampleProcessed = false;
              m_nSampleOffset = 0;
              m_nDataLeftInSample = m_pCurrentSample->GetActualDataLength();
            }
            else if (command == ASC_Flush)
            {
              m_pCurrentSample.Release();

              flush = true;
              sampleData = NULL;
              m_nSampleOffset = 0;
              m_nDataLeftInSample = 0;

              break;
            }
            else if (command == ASC_Pause)
            {
              m_pCurrentSample.Release();
              m_state = StatePaused;
            }
            else if (command == ASC_Resume)
            {
              sampleProcessed = false;
              writeSilence = 0;
              m_state = StateRunning;
              if (!m_pCurrentSample)
              {
                m_nDataLeftInSample = 0;
                goto fetchSample;
              }
            }
          }

          if (m_state != StateRunning)
            writeSilence = bufferSizeInBytes - bytesFilled;
          else if (m_nSampleOffset == 0 && !OOBCommandOnly)
          {
            // TODO error checking
            if (CheckSample(m_pCurrentSample, bufferSize - currentPadding) == S_FALSE)
            {
              GetWASAPIBuffer(bufferSize, currentPadding, bufferSizeInBytes, &data);
              bytesFilled = 0;
            }
          }

          if (writeSilence == 0 && (m_nSampleOffset == 0 || m_nSampleNum == 0) && !sampleProcessed)
          {
            HRESULT schedulingHR = CheckStreamTimeline(m_pCurrentSample, &dueTime, m_nSampleOffset);
            sampleProcessed = true;
              
            // m_pCurrentSample must exist if CheckStreamTimeline returns either of these
            if (schedulingHR == MPAR_S_DROP_SAMPLE)
            {
              m_pCurrentSample.Release();
              m_nDataLeftInSample = 0;
              goto fetchSample;
            }
            else if (schedulingHR == MPAR_S_WAIT_RENDER_TIME)
              CalculateSilence(&dueTime, &writeSilence);
          }

          if (writeSilence == 0 && m_pCurrentSample)
            RenderAudio(data, bufferSizeInBytes, m_nDataLeftInSample, m_nSampleOffset, m_pCurrentSample, bytesFilled);
          else
          {
            if (bufferSizeInBytes == writeSilence)
              flags = AUDCLNT_BUFFERFLAGS_SILENT;

            if (!m_pCurrentSample)
              writeSilence = bufferSizeInBytes;

            RenderSilence(data, bufferSizeInBytes, writeSilence, bytesFilled);
          }
        } while (bytesFilled < bufferSizeInBytes);

        hr = m_pRenderClient->ReleaseBuffer(bufferSize - currentPadding, flags);

        if (FAILED(hr) && hr != AUDCLNT_E_OUT_OF_ORDER)
          Log("CWASAPIRenderFilter::Render thread: ReleaseBuffer failed (0x%08x)", hr);
      }

      if (!m_pSettings->m_bWASAPIUseEventMode)
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
  
  m_csResources.Unlock();
  return 0;
}

void CWASAPIRenderFilter::StopRenderThread()
{
  Log("CWASAPIRenderFilter::Render thread - closing down - thread ID: %d", m_ThreadId);
  StopAudioClient();
  RevertMMCSS();
  CloseThread();
  m_pCurrentSample.Release();
  SetEvent(m_hCurrentSampleReleased);
  m_state = StateStopped;
  m_nSampleOffset = 0;
  m_nDataLeftInSample = 0;
  m_csResources.Unlock();
}

REFERENCE_TIME CWASAPIRenderFilter::BufferredDataDuration()
{
  CAutoLock queueLock(&m_inputQueueLock);

  REFERENCE_TIME rtDuration = 0;
  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtStop = 0;

  vector<TQueueEntry>::iterator it = m_inputQueue.begin();
  while (it != m_inputQueue.end())
  {
    it->Sample->GetTime(&rtStart, &rtStop);
    rtDuration += rtStop - rtStart;
    ++it;
  }

  if (m_pCurrentSample.p)
  {
    UINT nFrames = (m_pCurrentSample.p->GetActualDataLength() - m_nSampleOffset) / m_pInputFormat->Format.nBlockAlign;
    rtDuration += nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;
  }

  return rtDuration;
}

HRESULT CWASAPIRenderFilter::GetWASAPIBuffer(UINT32& bufferSize, UINT32& currentPadding, UINT32& bufferSizeInBytes, BYTE** pData)
{
  m_pAudioClient->GetBufferSize(&bufferSize);

  // In exclusive mode with even based buffer filling we threat the padding as zero 
  // -> it will make rest of the code a bit cleaner
  if (m_pSettings->m_WASAPIShareMode == AUDCLNT_SHAREMODE_SHARED || !m_pSettings->m_bWASAPIUseEventMode)
    m_pAudioClient->GetCurrentPadding(&currentPadding);

  bufferSizeInBytes = (bufferSize - currentPadding) * m_pInputFormat->Format.nBlockAlign;

  return m_pRenderClient->GetBuffer(bufferSize - currentPadding, pData);
}

void CWASAPIRenderFilter::RenderSilence(BYTE* pTarget, UINT32 bufferSizeInBytes, LONGLONG &writeSilence, UINT32 &bytesFilled)
{
  UINT32 silentBytes = min(writeSilence, bufferSizeInBytes - bytesFilled);
  memset(pTarget + bytesFilled, 0, silentBytes);
  bytesFilled += silentBytes;
  writeSilence -= silentBytes;

  if (m_pSettings->m_bLogDebug)
    Log("writing buffer with zeroes silentBytes: %d", silentBytes);
}

void CWASAPIRenderFilter::RenderAudio(BYTE* pTarget, UINT32 bufferSizeInBytes, UINT32 &dataLeftInSample, UINT32 &sampleOffset, IMediaSample* pSample, UINT32 &bytesFilled)
{
  BYTE* sampleData = NULL;
  pSample->GetPointer(&sampleData);

  dataLeftInSample = pSample->GetActualDataLength() - sampleOffset;

  UINT32 bytesToCopy = min(dataLeftInSample, bufferSizeInBytes - bytesFilled);
  memcpy(pTarget + bytesFilled, sampleData + sampleOffset, bytesToCopy); 
  bytesFilled += bytesToCopy;
  sampleOffset += bytesToCopy;
  
  if (m_pSettings->m_bLogDebug)
    Log("writing buffer with data: %d", bytesToCopy);
}

void CWASAPIRenderFilter::HandleFlush()
{
  UINT32 bufferSize = 0;
  UINT32 currentPadding = 0;
  UINT32 bufferSizeInBytes = 0;
  BYTE* data = NULL;

  HRESULT hr = GetWASAPIBuffer(bufferSize, currentPadding, bufferSizeInBytes, &data);
  if (SUCCEEDED(hr))
  {
    hr = m_pRenderClient->ReleaseBuffer(bufferSize - currentPadding, AUDCLNT_BUFFERFLAGS_SILENT);
    if (FAILED(hr))
      Log("CWASAPIRenderFilter::HandleFlush - GetWASAPIBuffer failed: (0x%08x)", hr);
  }
  else
    Log("CWASAPIRenderFilter::HandleFlush - ReleaseBuffer failed: (0x%08x)", hr);

  SetEvent(m_hCurrentSampleReleased);
}

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

HRESULT CWASAPIRenderFilter::GetBufferSize(const WAVEFORMATEX* pWaveFormatEx, REFERENCE_TIME* pHnsBufferPeriod)
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


HRESULT CWASAPIRenderFilter::CreateAudioClient(bool init)
{
  if (m_pAudioClient && !init)
    return S_OK;

  Log("WASAPIRenderFilter::CreateAudioClient");

  // AudioClient needs to be stopped beforehand
  //StopAudioClient(ppAudioClient);

  HRESULT hr = S_OK;

  if (!m_pMMDevice)
  {
    hr = m_pSettings->GetAudioDevice(&m_pMMDevice);

    if (FAILED(hr))
    {
      Log("WASAPIRenderFilter::CreateAudioClient failed - no MMDevice available (0x%08x)", hr);
      return E_FAIL;
    }
  }

  SAFE_RELEASE(m_pAudioClient);

  hr = m_pMMDevice->Activate(__uuidof(IAudioClient), CLSCTX_ALL, NULL, reinterpret_cast<void**>(&m_pAudioClient));
  if (FAILED(hr))
    Log("WASAPIRenderFilter::CreateAudioClient activation failed (0x%08x)", hr);
  else
  {
    Log("WASAPIRenderFilter::CreateAudioClient success");
    if (init)
    {
      unsigned int loopCount = 0;
      do 
      {
        loopCount++;
        hr = InitAudioClient();
        if (hr == AUDCLNT_E_DEVICE_IN_USE) // retry few times if we could get the exclusive WASAPI device handle
        {
          Log("WASAPIRenderFilter::CreateAudioClient - audio client in use, trying again - loopCount: %d", loopCount);
          Sleep(50);
        }
        else
          break;
      } while (hr == AUDCLNT_E_DEVICE_IN_USE && loopCount < 20);
    }
  }

  return hr;
}

HRESULT CWASAPIRenderFilter::StartAudioClient()
{
  HRESULT hr = S_OK;
  if (!m_bIsAudioClientStarted)
  {
    Log("WASAPIRenderFilter::StartAudioClient");

    if (m_pAudioClient)
    {
      hr = m_pAudioClient->Start();
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

  if (!m_pSettings->m_bWASAPIUseEventMode)
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
  if (!m_pSettings->m_bWASAPIUseEventMode)
  {
    CancelWaitableTimer(m_hDataEvent);
    if (CancelWaitableTimer(m_hDataEvent) == 0)
    {
      DWORD error = GetLastError();
      Log("WASAPIRenderFilter::CancelDataEvent - CancelWaitableTimer failed: %d", error);
    }
  }
}

HRESULT CWASAPIRenderFilter::StopAudioClient()
{
  HRESULT hr = S_OK;
  if (m_bIsAudioClientStarted)
  {
    Log("WASAPIRenderFilter::StopAudioClient");

    m_bIsAudioClientStarted = false;

    if (m_pAudioClient)
    {
      // Let the current audio buffer to be played completely.
      // Some amplifiers will "cache" the incomplete AC3 packets and that causes issues
      // when the next AC3 packets are received
      //  WaitForSingleObject(m_hDataEvent, Latency() / 10000);
      
      hr = m_pAudioClient->Stop();
      if (FAILED(hr))
        Log("   stop failed (0x%08x)", hr);

      hr = m_pAudioClient->Reset();
      if (FAILED(hr))
        Log("   reset failed (0x%08x)", hr);
    }
  }
  return hr;
}

HRESULT CWASAPIRenderFilter::InitAudioClient()
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

  WAVEFORMATEXTENSIBLE* pwfxAccepted = NULL;
  hr = IsFormatSupported(m_pInputFormat, &pwfxAccepted);
  if (FAILED(hr))
  {
    SAFE_DELETE_WAVEFORMATEX(pwfxAccepted);
    return hr;
  }

  GetBufferSize((WAVEFORMATEX*)pwfxAccepted, &m_pSettings->m_hnsPeriod);

  if (SUCCEEDED(hr))
    hr = m_pAudioClient->Initialize(m_pSettings->m_WASAPIShareMode, m_dwStreamFlags,
	                                m_pSettings->m_hnsPeriod, m_pSettings->m_hnsPeriod, (WAVEFORMATEX*)pwfxAccepted, NULL);

  if (FAILED(hr) && hr != AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED)
  {
    Log("WASAPIRenderFilter::InitAudioClient Initialize failed (0x%08x)", hr);
    SAFE_DELETE_WAVEFORMATEX(pwfxAccepted);
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
                  m_pInputFormat->Format.nSamplesPerSec  // (frames / s)
                  + 0.5 // rounding
    );

    if (SUCCEEDED(hr)) 
      hr = CreateAudioClient();
      
    Log("WASAPIRenderFilter::InitAudioClient Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames", m_pSettings->m_hnsPeriod, m_nFramesInBuffer);

    if (SUCCEEDED (hr)) 
      hr = m_pAudioClient->Initialize(m_pSettings->m_WASAPIShareMode, m_dwStreamFlags, 
	                                    m_pSettings->m_hnsPeriod, m_pSettings->m_hnsPeriod, (WAVEFORMATEX*)pwfxAccepted, NULL);
 
    if (FAILED(hr))
    {
      Log("WASAPIRenderFilter::InitAudioClient Failed to reinitialize the audio client");
      SAFE_DELETE_WAVEFORMATEX(pwfxAccepted);
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
    hr = m_pAudioClient->GetService(__uuidof(IAudioRenderClient), (void**)(&m_pRenderClient));

  if (FAILED(hr))
    Log("WASAPIRenderFilter::InitAudioClient service initialization failed (0x%08x)", hr);
  else
    Log("WASAPIRenderer::InitAudioClient service initialization success");

  if (m_pSettings->m_bWASAPIUseEventMode)
  {
    hr = m_pAudioClient->SetEventHandle(m_hDataEvent);
    if (FAILED(hr))
    {
      Log("WASAPIRenderFilter::InitAudioClient SetEventHandle failed (0x%08x)", hr);
      SAFE_DELETE_WAVEFORMATEX(pwfxAccepted);
      return hr;
    }
  }

  REFERENCE_TIME latency(0);
  m_pAudioClient->GetStreamLatency(&latency);
  
  Log("WASAPIRenderFilter::InitAudioClient device reported latency %I64u ms - buffer based latency %I64u ms", 
    latency / 10000, Latency() / 10000);

  // Dynamic format change requires restart for the audio client
  if (m_state != StateStopped)
    StartAudioClient();

  m_bDeviceInitialized = true;

  SAFE_DELETE_WAVEFORMATEX(pwfxAccepted);
  return hr;
}


