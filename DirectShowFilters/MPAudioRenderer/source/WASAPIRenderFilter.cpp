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
#include "Globals.h"
#include "WASAPIRenderFilter.h"
#include "TimeSource.h"

#include "alloctracing.h"

#include <FunctionDiscoveryKeys_devpkey.h>


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
  m_rtNextSampleTime(0),
  m_rtHwStart(0)
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
    Log("CWASAPIRenderFilter::NegotiateFormat - new format same as earlier");
    LogWaveFormat(m_pInputFormat, "CWASAPIRenderFilter::NegotiateFormat");
    *pChOrder = m_chOrder;

    return S_OK;
  }

  bool bApplyChanges = nApplyChangesDepth != 0;

  LogWaveFormat(pwfx, "CWASAPIRenderFilter::NegotiateFormat");

  bool bitDepthForced = (m_pSettings->m_nForceBitDepth != 0 && m_pSettings->m_nForceBitDepth != pwfx->Format.wBitsPerSample);
  bool sampleRateForced = (m_pSettings->m_nForceSamplingRate != 0 && m_pSettings->m_nForceSamplingRate != pwfx->Format.nSamplesPerSec);
  
  if ((bitDepthForced || sampleRateForced) &&
       pwfx->SubFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL ||
       pwfx->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT && bitDepthForced)
    return VFW_E_TYPE_NOT_ACCEPTED;
  
  if (((bitDepthForced && m_pSettings->m_nForceBitDepth != pwfx->Format.wBitsPerSample) ||
       (sampleRateForced && m_pSettings->m_nForceSamplingRate != pwfx->Format.nSamplesPerSec)))
    return VFW_E_TYPE_NOT_ACCEPTED;

  HRESULT hr = VFW_E_CANNOT_CONNECT;

  CAutoLock lock(&m_csResources);

  hr = CreateAudioClient();
  if (FAILED(hr))
  {
    Log("CWASAPIRenderFilter::NegotiateFormat Error, audio client not initialized: (0x%08x)", hr);
    return VFW_E_CANNOT_CONNECT;
  }

  WAVEFORMATEXTENSIBLE* pwfxCM = NULL;
  const WAVEFORMATEXTENSIBLE* pwfxAccepted = NULL;
  WAVEFORMATEX* tmpPwfx = NULL; 
  hr = m_pAudioClient->IsFormatSupported(m_pSettings->m_WASAPIShareMode, (WAVEFORMATEX*)pwfx, (WAVEFORMATEX**)&pwfxCM);
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

    WAVEFORMATEXTENSIBLE* wfe;
    ToWaveFormatExtensible(&wfe, tmpPwfx);

    pwfxAccepted = wfe;
  }
  else
    pwfxAccepted = pwfx;
  
  Log("CWASAPIRenderFilter::NegotiateFormat WASAPI client accepted the format");

  if (bApplyChanges)
  {
    // Stop and discard audio client
    StopAudioClient();
    SAFE_RELEASE(m_pRenderClient);
    SAFE_RELEASE(m_pAudioClock); // locking might be needed
    SAFE_RELEASE(m_pAudioClient);

    SetInputFormat(pwfxAccepted);
    // Reinitialize audio client
    hr = CreateAudioClient(true);
  }
  SAFE_DELETE_WAVEFORMATEX(tmpPwfx);

  m_chOrder = *pChOrder = DS_ORDER;

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

  REFERENCE_TIME rtTime = 0;
  REFERENCE_TIME rtStop = 0;
  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtDuration = 0;

  bool discontinuityDetected = false;

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

  // Get media time
  REFERENCE_TIME rtRefClock = 0;
  
  m_pClock->GetTime(&rtRefClock);
  rtRefClock -= m_rtStart;
  rtTime = m_pClock->GetHWTime() - m_rtHwStart;

  REFERENCE_TIME rtHWOffset = rtTime - rtRefClock;

  if (m_pSettings->m_bLogSampleTimes)
    Log("   sample start: %6.3f  stop: %6.3f dur: %6.3f diff: %6.3f rtTime: %6.3f rtRefClock: %6.3f early: %6.3f ", 
      rtStart / 10000000.0, rtStop / 10000000.0, rtDuration / 10000000.0, (rtStart - m_rtNextSampleTime) / 10000000.0, 
      rtTime / 10000000.0, rtRefClock / 10000000.0, (rtStart - rtTime) / 10000000.0);

  // Try to keep the A/V sync when data has been dropped
  if ((abs(rtStart - m_rtNextSampleTime) > MAX_SAMPLE_TIME_ERROR) && m_nSampleNum > 0)
  {
    discontinuityDetected = true;
    Log("   Dropped audio data detected: diff: %7.3f ms MAX_SAMPLE_TIME_ERROR: %7.3f ms", ((double)rtStart - (double)m_rtNextSampleTime) / 10000.0, (double)MAX_SAMPLE_TIME_ERROR / 10000.0);
  }

  m_rtNextSampleTime = rtStop;

  REFERENCE_TIME timeStamp = rtStart - rtTime;

  if ((timeStamp > 0 && m_nSampleNum == 0) || discontinuityDetected)
  {
    REFERENCE_TIME offsetDelay = 0;
    if (sampleOffset > 0)
      offsetDelay = sampleOffset / m_pInputFormat->Format.nBlockAlign * UNITS / m_pInputFormat->Format.nSamplesPerSec;

    *pDueTime = timeStamp + offsetDelay + rtHWOffset;
    m_nSampleNum++;

    if (m_pSettings->m_bLogSampleTimes)
      Log("   MPAR_S_WAIT_RENDER_TIME - %6.3f", *pDueTime / 10000000.0);

    return MPAR_S_WAIT_RENDER_TIME;
  }
  else if (timeStamp < 0)
  {
    // TODO implement partial sample dropping
    Log("   sample late - dropping sample: %6.3f rtTime: %6.3f", rtStart / 10000000.0, rtTime / 10000000.0);
    m_nSampleNum = 0;

    return MPAR_S_DROP_SAMPLE;
  }

  m_nSampleNum++;

  return MPAR_S_RENDER_SAMPLE;
}

void CWASAPIRenderFilter::CalculateSilence(REFERENCE_TIME* pDueTime, LONGLONG* pBytesOfSilence)
{
  REFERENCE_TIME rtTime = m_pClock->GetHWTime();
  rtTime -= m_rtHwStart;

  REFERENCE_TIME rtSilenceDuration = *pDueTime - rtTime;

  if (m_pSettings->m_bLogSampleTimes)
    Log("   calculateSilence: %6.3f", rtSilenceDuration / 10000000.0);

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

HRESULT CWASAPIRenderFilter::Run(REFERENCE_TIME rtStart)
{
  REFERENCE_TIME rtTime = 0;
  REFERENCE_TIME rtHwTime = 0;

  HRESULT hr = m_pClock->GetTime(&rtTime);
  rtHwTime = m_pClock->GetHWTime();

  if (SUCCEEDED(hr))
  {
    m_rtHwStart = rtStart + (rtHwTime - rtTime);
    Log("CWASAPIRenderFilter::Run - ref clock: %6.3f HW clock: %6.3f", rtStart / 10000000.0, m_rtHwStart / 10000000.0 );
  }
  else
    Log("CWASAPIRenderFilter::Run - error (0x%08x)", hr);

  return CQueuedAudioSink::Run(rtStart);
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
  
  UINT32 sampleOffset = 0;
  UINT32 dataLeftInSample = 0;
  LONGLONG writeSilence = 0;
  BYTE* sampleData = NULL;

  bool flush = false;
  bool sampleProcessed = false;

  REFERENCE_TIME dueTime = 0;

  HRESULT hr = S_FALSE;

  m_nSampleNum = 0;

  hr = GetNextSampleOrCommand(&command, &m_pCurrentSample.p, MAX_SAMPLE_WAIT_TIME, &m_hSampleEvents, &m_dwSampleWaitObjects);
  if (FAILED(hr))
    Log("CWASAPIRenderFilter::Render thread - failed to get 1st sample: (0x%08x)", hr);

  if (hr == MPAR_S_THREAD_STOPPING)
    return 0;

  EnableMMCSS();
  
  m_csResources.Lock();

  if (m_pSettings->m_bReleaseDeviceOnStop)
  {
    if (m_pInputFormat)
    {
      hr = CreateAudioClient(true);
      if (FAILED(hr))
      {
        Log("CWASAPIRenderFilter::Render thread Error, audio client not available: (0x%08x)", hr);
        m_csResources.Unlock();
        return 0;
      }
    }
  }

  if (m_pAudioClient)
  {
    hr = StartAudioClient();
    if (FAILED(hr))
    {
      Log("CWASAPIRenderFilter::Render thread Error, starting audio client failed: (0x%08x)", hr);
      m_csResources.Unlock();
      return 0;
    }
  }

  m_state = StateRunning;

  while (true)
  {
    if (flush)
    {
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

          bool OOBCommandOnly = dataLeftInSample > 0 || m_state != StateRunning;

          if (dataLeftInSample == 0 || OOBCommandOnly)
          {
            m_csResources.Unlock();
            HRESULT result = GetNextSampleOrCommand(&command, &m_pCurrentSample.p, MAX_SAMPLE_WAIT_TIME, &m_hSampleEvents,
                                                    &m_dwSampleWaitObjects, OOBCommandOnly);
            m_csResources.Lock();

            if (result == MPAR_S_THREAD_STOPPING || !m_pAudioClient)
            {
              StopRenderThread();
              return 0;
            }

            if (!m_pCurrentSample)
              dataLeftInSample = 0;
              
            if (command == ASC_PutSample && m_pCurrentSample)
            {
              sampleProcessed = false;
              sampleOffset = 0;
              dataLeftInSample = m_pCurrentSample->GetActualDataLength();
            }
            else if (command == ASC_Flush)
            {
              m_pCurrentSample.Release();

              flush = true;
              sampleData = NULL;
              sampleOffset = 0;
              dataLeftInSample = 0;

              //goto fetchSample;
              break;
            }
            else if (command == ASC_Pause)
            {
              m_nSampleNum = 0;
              m_state = StatePaused;
            }
            else if (command == ASC_Resume)
            {
              sampleProcessed = false;
              writeSilence = 0;
              m_state = StateRunning;
              if (!m_pCurrentSample)
                goto fetchSample;
            }
          }

          if (m_state != StateRunning)
            writeSilence = bufferSizeInBytes - bytesFilled;
          else if (sampleOffset == 0 && !OOBCommandOnly)
          {
            // TODO error checking
            if (CheckSample(m_pCurrentSample, bufferSize - currentPadding) == S_FALSE)
            {
              GetWASAPIBuffer(bufferSize, currentPadding, bufferSizeInBytes, &data);
              bytesFilled = 0;
            }
          }

          if (writeSilence == 0 && (sampleOffset == 0 || m_nSampleNum == 0) && !sampleProcessed)
          {
            HRESULT schedulingHR = CheckStreamTimeline(m_pCurrentSample, &dueTime, sampleOffset);
            sampleProcessed = true;
              
            // m_pCurrentSample must exist if CheckStreamTimeline returns either of these
            if (schedulingHR == MPAR_S_DROP_SAMPLE)
            {
              m_pCurrentSample.Release();
              goto fetchSample;
            }
            else if (schedulingHR == MPAR_S_WAIT_RENDER_TIME)
              CalculateSilence(&dueTime, &writeSilence);
          }

          if (writeSilence == 0 && m_pCurrentSample)
            RenderAudio(data, bufferSizeInBytes, dataLeftInSample, sampleOffset, m_pCurrentSample, bytesFilled);
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
  m_csResources.Unlock();  
  m_state = StateStopped;
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

HRESULT CWASAPIRenderFilter::GetAudioDevice(IMMDevice** ppMMDevice)
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
    
    for (UINT i = 0; i < count; i++)
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
          Log("  devices->GetId failed: (0x%08x)", hr);     
      }
      else
        Log("  devices->Item failed: (0x%08x)", hr);  

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
    for (UINT i = 0; i < count; i++)
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
    hr = GetAudioDevice(&m_pMMDevice);

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
      hr = InitAudioClient();
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
    HRESULT hr = CancelWaitableTimer(m_hDataEvent);
    if (FAILED(hr))
      Log("WASAPIRenderFilter::CancelDataEvent - CancelWaitableTimer failed: (0x%08x)", hr);
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

  WAVEFORMATEX *pwfxCM = NULL;
  hr = m_pAudioClient->IsFormatSupported(m_pSettings->m_WASAPIShareMode, (WAVEFORMATEX*)m_pInputFormat, &pwfxCM);    
  if (FAILED(hr))
    Log("WASAPIRenderFilter::InitAudioClient not supported (0x%08x)", hr);
  else
    Log("WASAPIRenderFilter::InitAudioClient format supported");

  GetBufferSize((WAVEFORMATEX*)m_pInputFormat, &m_pSettings->m_hnsPeriod);

  if (SUCCEEDED(hr))
    hr = m_pAudioClient->Initialize(m_pSettings->m_WASAPIShareMode, m_dwStreamFlags,
	                                m_pSettings->m_hnsPeriod, m_pSettings->m_hnsPeriod, (WAVEFORMATEX*)m_pInputFormat, NULL);

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
                  m_pInputFormat->Format.nSamplesPerSec  // (frames / s)
                  + 0.5 // rounding
    );

    if (SUCCEEDED(hr)) 
      hr = CreateAudioClient();
      
    Log("WASAPIRenderFilter::InitAudioClient Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames", m_pSettings->m_hnsPeriod, m_nFramesInBuffer);

    if (SUCCEEDED (hr)) 
      hr = m_pAudioClient->Initialize(m_pSettings->m_WASAPIShareMode, m_dwStreamFlags, 
	                                    m_pSettings->m_hnsPeriod, m_pSettings->m_hnsPeriod, (WAVEFORMATEX*)m_pInputFormat, NULL);
 
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

  return hr;
}


