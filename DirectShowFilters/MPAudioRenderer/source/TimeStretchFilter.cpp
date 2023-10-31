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
#include "TimeStretchFilter.h"

#include <map>

#include "alloctracing.h"

#define DEFINE_STREAM_FUNC(funcname, paramtype, paramname) \
  void CTimeStretchFilter::funcname(paramtype paramname) \
  { \
    CAutoLock streamLock(&m_csStreamLock); \
    ASSERT(m_Stream || (m_bBitstreaming && !m_Stream)); \
    if (m_Stream) \
    { \
        m_Stream->funcname(paramname); \
    } \
  }

CTimeStretchFilter::CTimeStretchFilter(AudioRendererSettings* pSettings, CSyncClock* pClock, Logger* pLogger) :
  CQueuedAudioSink(pSettings, pLogger),
  m_Stream(NULL),
  m_fCurrentTempo(1.0),
  m_fNewAdjustment(1.0),
  m_fCurrentAdjustment(1.0),
  m_fNewTempo(1.0),
  m_pMediaType(NULL),
  m_rtInSampleTime(0),
  m_rtLastOuputStart(0),
  m_rtLastOuputEnd(-1),
  m_rtNextIncomingSampleTime(0),
  m_pClock(pClock),
  m_pLogger(pLogger)
{
}

CTimeStretchFilter::~CTimeStretchFilter(void)
{
  Log("CTimeStretchFilter - destructor - instance 0x%x", this);
  
  SetEvent(m_hStopThreadEvent);
  WaitForSingleObject(m_hThread, INFINITE);

  CAutoLock streamLock(&m_csStreamLock);
  CAutoLock lock(&m_csResources);
  SetFormat(NULL);
  DeleteMediaType(m_pMediaType);

  Log("CTimeStretchFilter - destructor - instance 0x%x - end", this);
}

//Initialization
HRESULT CTimeStretchFilter::Init()
{
  m_hSampleEvents.push_back(m_hInputAvailableEvent);
  m_hSampleEvents.push_back(m_hOOBCommandAvailableEvent);
  m_hSampleEvents.push_back(m_hStopThreadEvent);

  m_dwSampleWaitObjects.push_back(S_OK);
  m_dwSampleWaitObjects.push_back(MPAR_S_OOB_COMMAND_AVAILABLE);
  m_dwSampleWaitObjects.push_back(MPAR_S_THREAD_STOPPING);

  return CQueuedAudioSink::Init();
}

HRESULT CTimeStretchFilter::PutSample(IMediaSample* pSample)
{
  AM_MEDIA_TYPE* pmt = NULL;
  bool bFormatChanged = false;

  if (pSample && SUCCEEDED(pSample->GetMediaType(&pmt)) && pmt)
  {
    WAVEFORMATEXTENSIBLE* pwfx = (WAVEFORMATEXTENSIBLE*)pmt->pbFormat;
    bFormatChanged = !FormatsEqual(pwfx, m_pInputFormat);

    bool wasBitstreaming = m_bBitstreaming;

    if (bFormatChanged && CanBitstream(pwfx))
      m_bBitstreaming = true;
    else
      m_bBitstreaming = false;

    if (wasBitstreaming != m_bBitstreaming)
      Log("CTimeStretchFilter::PutSample - wasBitstreaming: %d bitstreaming: %d", wasBitstreaming, m_bBitstreaming);
  }

  if (m_pSettings->GetUseTimeStretching() && !m_bBitstreaming)
    CQueuedAudioSink::PutSample(pSample);
  else if (m_pNextSink)
    return m_pNextSink->PutSample(pSample);

  return S_OK;
}

// Format negotiation
HRESULT CTimeStretchFilter::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

  if (FormatsEqual(pwfx, m_pInputFormat))
  {
    *pChOrder = m_chOrder;
    return S_OK;
  }

  bool bApplyChanges = (nApplyChangesDepth != 0);
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  if (m_pSettings->GetAllowBitStreaming() && CanBitstream(pwfx))
  {
    HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
    if (SUCCEEDED(hr))
    {
      m_bNextFormatPassthru = true;
      m_chOrder = *pChOrder;
    }
    return hr;
  }

#ifdef INTEGER_SAMPLES
  // only accept 16bit int
  if (pwfx->Format.wBitsPerSample != 16 || pwfx->SubFormat != KSDATAFORMAT_SUBTYPE_PCM)
    return VFW_E_TYPE_NOT_ACCEPTED;
#else 
  // only accept 32bit float
  if (pwfx->Format.wBitsPerSample != 32 || pwfx->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
    return VFW_E_TYPE_NOT_ACCEPTED;
#endif

  HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
  if (FAILED(hr))
    return hr;

  hr = VFW_E_CANNOT_CONNECT;
  
  if (!pwfx)
    return SetFormat(NULL);

  if (bApplyChanges)
  {
    LogWaveFormat(pwfx, "TS   - applying ");

    m_pNextSink->NegotiateBuffer(pwfx, &m_nOutBufferSize, &m_nOutBufferCount, true);

    AM_MEDIA_TYPE tmp;
    HRESULT result = CreateAudioMediaType((WAVEFORMATEX*)pwfx, &tmp, true);
    if (SUCCEEDED(result))
    {
      if (m_pMediaType)
        DeleteMediaType(m_pMediaType);
      m_pMediaType = CreateMediaType(&tmp);
    }

    SetInputFormat(pwfx);
    SetOutputFormat(pwfx);
    hr = SetFormat(pwfx);
    if (FAILED(hr))
        return hr;
  }
  else
    LogWaveFormat(pwfx, "TS   -          ");

  m_bNextFormatPassthru = !m_pSettings->GetUseTimeStretching();
  m_chOrder = *pChOrder;

  return S_OK;
}

HRESULT CTimeStretchFilter::CheckSample(IMediaSample* pSample, REFERENCE_TIME* rtDrained)
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
    REFERENCE_TIME rtStart = 0;
    REFERENCE_TIME rtStop = 0;

    hr = pSample->GetTime(&rtStart, &rtStop);
    
    if (SUCCEEDED(hr))
    {
      Log("CTimeStretchFilter - CheckSample - resyncing in middle of stream on format change - rtStart: %6.3f m_rtInSampleTime: %6.3f", 
        rtStart / 10000000.0, m_rtInSampleTime / 10000000.0);

      *rtDrained = DrainBuffers(pSample, rtStart);
    }
    else
      Log("CTimeStretchFilter::CheckFormat failed to get timestamps from sample: 0x%08x", hr);

    // Apply format change
    ChannelOrder chOrder;
    WAVEFORMATEXTENSIBLE* pwfx = (WAVEFORMATEXTENSIBLE*)pmt->pbFormat;

    hr = NegotiateFormat(pwfx, 1, &chOrder);
    pSample->SetDiscontinuity(false);
    m_bBitstreaming = CanBitstream(pwfx);

    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("CTimeStretchFilter::CheckFormat failed to change format: 0x%08x", hr);
      return hr;
    }
    else
    {
      m_chOrder = chOrder;
      return S_FALSE; // format changed
    }
  }

  return S_OK;
}

HRESULT CTimeStretchFilter::SetFormat(const WAVEFORMATEXTENSIBLE *pwfe)
{
  CSoundTouchEx* newStream = NULL;

  if (pwfe)
  {
      // First verify format is supported
      if (pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_PCM &&
          pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
          return VFW_E_TYPE_NOT_ACCEPTED;

      if (pwfe->Format.nChannels < 1 || pwfe->Format.nChannels > SOUNDTOUCH_MAX_CHANNELS)
          return VFW_E_TYPE_NOT_ACCEPTED;

      newStream = new CSoundTouchEx();
      newStream->SetChannels(pwfe->Format.nChannels);
      newStream->SetInputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8);
      newStream->SetOutputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8);
  }

  CAutoLock streamLock(&m_csStreamLock);

  // delete old one
  CSoundTouchEx* oldStream = m_Stream;
  SAFE_DELETE(oldStream);

  m_Stream = newStream;

  if (newStream && pwfe)
  {
    setSetting(SETTING_USE_QUICKSEEK, m_pSettings->GetQuality_USE_QUICKSEEK());
    setSetting(SETTING_USE_AA_FILTER, m_pSettings->GetQuality_USE_AA_FILTER());
    setSetting(SETTING_AA_FILTER_LENGTH, m_pSettings->GetQuality_AA_FILTER_LENGTH());
    setSetting(SETTING_SEQUENCE_MS, m_pSettings->GetQuality_SEQUENCE_MS()); 
    setSetting(SETTING_SEEKWINDOW_MS, m_pSettings->GetQuality_SEEKWINDOW_MS());
    setSetting(SETTING_OVERLAP_MS, m_pSettings->GetQuality_OVERLAP_MS());

    setTempoInternal(m_fNewTempo, m_fNewAdjustment);
    setSampleRate(pwfe->Format.nSamplesPerSec);
    setTempoChange(0);
    setPitchSemiTones(0);
  }

  return S_OK;
}

HRESULT CTimeStretchFilter::EndOfStream()
{
  // Queue an EOS marker so that it gets processed in 
  // the same thread as the audio data.
  PutSample(NULL);
  // wait until input queue is empty
  //if(m_hInputQueueEmptyEvent)
  //  WaitForSingleObject(m_hInputQueueEmptyEvent, END_OF_STREAM_FLUSH_TIMEOUT); // TODO make this depend on the amount of data in the queue
  return S_OK;
}

void CTimeStretchFilter::CheckStreamContinuity(IMediaSample* pSample, REFERENCE_TIME rtDrained)
{
  REFERENCE_TIME rtStart = 0;
  REFERENCE_TIME rtStop = 0;
  HRESULT hr = pSample->GetTime(&rtStart, &rtStop);

  if (SUCCEEDED(hr))
  {
    if (m_nSampleNum == 0)
      m_rtNextIncomingSampleTime = m_rtInSampleTime = rtStart;

    //Log("Ts:   rtStart: %6.3f m_rtNextIncomingSampleTime: %6.3f", rtStart / 10000000.0, m_rtNextIncomingSampleTime / 10000000.0);

    // Detect discontinuity in stream timeline
    REFERENCE_TIME rtGap = rtStart - m_rtNextIncomingSampleTime;

    m_rtNextIncomingSampleTime = rtStart;
    bool hasGap = false;

    if (abs(rtGap) > MAX_SAMPLE_TIME_ERROR)
    {
      Log("TS - gap - rtStart: %6.3f m_rtNextIncomingSampleTime: %6.3f gap: %6.3f", rtStart / 10000000.0, m_rtNextIncomingSampleTime / 10000000.0, rtGap / 10000000.0);
      hasGap = true;

      if (m_nSampleNum == 0) 
      {
        Log("CTimeStretchFilter - resyncing in start of stream - m_rtStart: %6.3f rtStart: %6.3f m_rtInSampleTime: %6.3f", 
          m_rtStart / 10000000.0, rtStart / 10000000.0, m_rtInSampleTime / 10000000.0);

        m_rtInSampleTime = rtStart;
      }
      else 
      {
        if (rtDrained == 0)
          rtDrained = DrainBuffers(pSample, rtStart);

        REFERENCE_TIME rtEstimatedInSampleTime = rtStart / m_pClock->GetBias();

        Log("CTimeStretchFilter - resyncing in middle of stream - rtDrained: %6.3f rtStart: %6.3f m_rtInSampleTime: %6.3f rtEstimatedInSampleTime: %6.3f calculated: %6.3f", 
          rtDrained / 10000000.0, rtStart / 10000000.0, m_rtInSampleTime / 10000000.0, rtEstimatedInSampleTime / 10000000.0, (m_rtInSampleTime + (rtGap + rtDrained) / m_pClock->GetBias()) / 10000000.0);

        Log("CTimeStretchFilter - rtDrained: %6.3f rtGap: %6.3f m_rtInSampleTime: %6.3f m_rtLastOuputEnd: %6.3f Bias: %6.6f", 
          rtDrained / 10000000.0, rtGap / 10000000.0, m_rtInSampleTime / 10000000.0, m_rtLastOuputEnd / 10000000.0, m_pClock->GetBias());

        m_rtInSampleTime -= rtDrained;
        m_rtInSampleTime += (rtGap) / m_pClock->GetBias();
        m_rtLastOuputEnd += (rtGap) * m_pClock->GetBias();

        Log("CTimeStretchFilter - m_rtInSampleTime: %6.3f m_rtLastOuputEnd: %6.3f", 
          m_rtInSampleTime / 10000000.0, m_rtLastOuputEnd / 10000000.0);

        m_rtNextIncomingSampleTime = rtStop;
      }
    }

    if (!hasGap)
    {
      UINT nFrames = pSample->GetActualDataLength() / m_pInputFormat->Format.nBlockAlign;
      m_rtNextIncomingSampleTime += nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;
    }
  }
}

REFERENCE_TIME CTimeStretchFilter::DrainBuffers(IMediaSample* pSample, REFERENCE_TIME rtNewStart)
{
  if (!m_pOutputFormat)
    return 0;

  Log("TS - DrainBuffers - rtNewStart: %6.3f", rtNewStart / 10000000.0);

  uint unprocessedSamplesBefore = numUnprocessedSamples();
  uint zeros = flushEx() - 32; // Magic 32 to keep the SoundTouch's output in sync
  uint unprocessedSamplesAfter = numUnprocessedSamples();

  UINT32 outFramesAfter = numSamples();
  UINT32 totalSamples = zeros + unprocessedSamplesBefore;
  UINT32 totalProcessedSamples = totalSamples - unprocessedSamplesAfter;

  Log("TS - DrainBuffers - unprocessedSamplesBefore: %u zeros: %u unprocessedSamplesAfter: %u outFramesAfter: %u duration %6.3f",
    unprocessedSamplesBefore, zeros, unprocessedSamplesAfter, outFramesAfter, (double)unprocessedSamplesBefore * (double) UNITS / (double) m_pOutputFormat->Format.nSamplesPerSec);

  REFERENCE_TIME rtAHwTime = 0;
  REFERENCE_TIME rtRCTime = 0;
  REFERENCE_TIME estimatedExtraSampleDuration = (((int)zeros - (int)unprocessedSamplesAfter) * UNITS) / m_pOutputFormat->Format.nSamplesPerSec;

  double bias = m_pClock->GetBias();
  double adjustment = m_pClock->Adjustment();

  m_pClock->GetHWTime(&rtRCTime, &rtAHwTime);
  double AVMult = m_pClock->SuggestedAudioMultiplier(rtAHwTime, rtRCTime, bias, adjustment);
  setTempoInternal(AVMult, 1.0);

  CreateOutput(totalProcessedSamples, outFramesAfter, bias, adjustment, AVMult, true);
    
  // Empty SoundTouch's buffers
  clear();

  pSample->SetDiscontinuity(false);

  return estimatedExtraSampleDuration;
}

// Processing
DWORD CTimeStretchFilter::ThreadProc()
{
  Log("CTimeStretchFilter::timestretch thread - starting up - thread ID: %d", m_ThreadId);
  
  SetThreadName(0, "TimeStretchFilter");

  AudioSinkCommand command;
  CComPtr<IMediaSample> sample;

  while (true)
  {
    m_csResources.Unlock();
    HRESULT hr = GetNextSampleOrCommand(&command, &sample.p, INFINITE, &m_hSampleEvents, &m_dwSampleWaitObjects);
    m_csResources.Lock();

    if (hr == MPAR_S_THREAD_STOPPING)
    {
      Log("CTimeStretchFilter::timestretch thread - closing down - thread ID: %d", m_ThreadId);
      SetEvent(m_hCurrentSampleReleased);
      clear();
      CloseThread();
      m_csResources.Unlock();
      return 0;
    }
    else
    {
      if (command == ASC_Flush)
      {
      	Log("CTimeStretchFilter::timestretch thread - flushing");
        m_rtInSampleTime = m_rtNextIncomingSampleTime = 0;
        m_rtLastOuputStart = m_rtLastOuputEnd = -1;
        if (m_pNextOutSample)
          m_pNextOutSample.Release();

        clear();
        m_pClock->Flush();
        sample.Release();
        SetEvent(m_hCurrentSampleReleased);
      }
      else if (command == ASC_Pause || command == ASC_Resume)
        continue;
      else if (sample)
      {
        BYTE *pMediaBuffer = NULL;
        long size = sample->GetActualDataLength();

        if (sample->IsDiscontinuity() == S_OK)
        {
          sample->SetDiscontinuity(false);
          m_bDiscontinuity = true;
        }

        REFERENCE_TIME rtDrained = 0;

        if (CheckSample(sample, &rtDrained) == S_FALSE)
        {
          DeleteMediaType(m_pMediaType);
          sample->GetMediaType(&m_pMediaType);
        }

        CheckStreamContinuity(sample, rtDrained);

        m_nSampleNum++;

        hr = sample->GetPointer(&pMediaBuffer);

        if ((hr == S_OK) && m_pMemAllocator)
        {
          REFERENCE_TIME rtStart = 0;
          REFERENCE_TIME rtAdjustedStart = 0;
          REFERENCE_TIME rtEnd = 0;
          REFERENCE_TIME rtAdjustedEnd = 0;
          REFERENCE_TIME rtAHwTime = 0;
          REFERENCE_TIME rtRCTime = 0;

          m_pClock->GetHWTime(&rtRCTime, &rtAHwTime);

          sample->GetTime(&rtStart, &rtEnd);
          REFERENCE_TIME sampleDuration = rtEnd - rtStart;

          uint unprocessedSamplesBefore = numUnprocessedSamples();
          uint unprocessedSamplesAfter = 0;

          UINT32 nFrames = size / m_pOutputFormat->Format.nBlockAlign;

          double bias = m_pClock->GetBias();
          double adjustment = m_pClock->Adjustment();
          double AVMult = m_pClock->SuggestedAudioMultiplier(rtAHwTime, rtRCTime, bias, adjustment);
          setTempoInternal(AVMult, 1.0);

          if (m_rtLastOuputEnd == -1)
            m_rtLastOuputEnd = rtStart / AVMult - 1;

          m_rtLastOuputStart = m_rtLastOuputEnd + 1;

          // Process the sample 
          putSamplesInternal((const short*)pMediaBuffer, size / m_pOutputFormat->Format.nBlockAlign);

          unprocessedSamplesAfter = numUnprocessedSamples();

          UINT32 nInFrames = (size / m_pOutputFormat->Format.nBlockAlign) - unprocessedSamplesAfter + unprocessedSamplesBefore;
          UINT32 nOutFrames = numSamples();
          
          // TODO: Soundtouch can provide less samples than asked (but never more) so a cummulative error is possible.  This will not happen over the course of a long TV stint, but could be solved for correctness
          // m_rtLastOuputEnd += (nOutFrames + unprocessedSamplesAfter - unprocessedSamplesBefore) * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

          //rtStart = m_rtInSampleTime;
          rtEnd = rtStart + sampleDuration;
          rtAdjustedStart = m_rtLastOuputEnd +1;
          rtAdjustedEnd = rtAdjustedStart + sampleDuration / AVMult;

          m_rtLastOuputEnd += sampleDuration / AVMult;

          CreateOutput(nInFrames, nOutFrames, bias, adjustment, AVMult, false);

          m_pClock->AddSample(rtStart, rtAdjustedStart, rtEnd, rtAdjustedEnd);
        }
      }
    }
  }
}

void CTimeStretchFilter::CreateOutput(UINT32 nInFrames, UINT32 nOutFrames, double dBias, double dAdjustment, double dAVMult, bool bFlushPartialSample)
{
  HRESULT hr = S_OK;
  UINT32 maxBufferFrames = m_nOutBufferSize / m_pOutputFormat->Format.nBlockAlign;
  UINT32 nOutFramesTotal = 0;

  CAutoLock lock (&m_csOutputSample);

  while (nOutFrames > 0)
  {
    // try to get an output buffer if none available
    if (!m_pNextOutSample && FAILED(hr = RequestNextOutBuffer(m_rtInSampleTime)))
    {
      if (hr != VFW_E_NOT_COMMITTED)
        Log("CTimeStretchFilter::timestretch thread - Failed to get next output sample!");

      break;
    }

    BYTE* pOutData = NULL;
    m_pNextOutSample->GetPointer(&pOutData);

    if (pOutData)
    {
      UINT32 nOffset = m_pNextOutSample->GetActualDataLength();
      UINT32 nOffsetInFrames = nOffset / m_pOutputFormat->Format.nBlockAlign;

      if (nOutFrames > maxBufferFrames - nOffsetInFrames)
        nOutFrames = maxBufferFrames - nOffsetInFrames;

      m_pNextOutSample->SetActualDataLength(nOffset + nOutFrames * m_pOutputFormat->Format.nBlockAlign);
      pOutData += nOffset;
      receiveSamplesInternal((short*)pOutData, nOutFrames);
      nOutFramesTotal += nOutFrames;

      if (m_pMediaType)
        m_pNextOutSample->SetMediaType(m_pMediaType);

      OutputSample(bFlushPartialSample);

      nOutFrames = numSamples();
    }
  }
}

void CTimeStretchFilter::OutputSample(bool bForce)
{
  if (m_pNextOutSample)
  {
    UINT32 sampleLen = m_pNextOutSample->GetActualDataLength();
    if (bForce || (sampleLen + m_pOutputFormat->Format.nBlockAlign > m_nOutBufferSize))
    {
      HRESULT hr = OutputNextSample();
      m_rtInSampleTime += (sampleLen * UNITS) / (m_pOutputFormat->Format.nBlockAlign * m_pOutputFormat->Format.nSamplesPerSec);

      if (FAILED(hr))
        Log("CTimeStretchFilter::timestretch thread OutputNextSample failed with: 0x%08x", hr);
    }
  }
}

DEFINE_STREAM_FUNC(setRate, float, newRate)
DEFINE_STREAM_FUNC(setRateChange, float, newRate)
DEFINE_STREAM_FUNC(setTempoChange, float, newTempo)
DEFINE_STREAM_FUNC(setPitchOctaves, float, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, int, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, float, newPitch)
DEFINE_STREAM_FUNC(setSampleRate, uint, srate)

void CTimeStretchFilter::clear() 
{ 
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (m_Stream)
      m_Stream->clear(); 
}

void CTimeStretchFilter::flush() 
{ 
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (m_Stream)
      m_Stream->flush(); 

}

uint CTimeStretchFilter::flushEx()
{ 
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  
  if (m_Stream) 
      return m_Stream->flushEx();

  return 0;
}

bool CTimeStretchFilter::setTempo(float newTempo, float newAdjustment)
{
  if (m_bBitstreaming)
    return false;

  m_fNewTempo = newTempo;
  m_fNewAdjustment = newAdjustment;

  return true;
}

void CTimeStretchFilter::setTempoInternal(float newTempo, float newAdjustment)
{
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (m_Stream)
  { 
    m_Stream->setTempo(newTempo * newAdjustment);
    m_fCurrentTempo = newTempo;
    m_fCurrentAdjustment = newAdjustment;
  } 
}

BOOL CTimeStretchFilter::setSetting(int settingId, int value)
{
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  // TODO should LFE channel have separate settings since it is by nature quite
  // different when it comes to frequency response
  if (m_Stream)
  {
    m_Stream->setSetting(settingId, value);
    return true;
  }

  return false;
}

uint CTimeStretchFilter::numUnprocessedSamples() const
{
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (m_Stream)
      return m_Stream->numUnprocessedSamples();
    
  return 0;
}

/// Returns number of samples currently available.
uint CTimeStretchFilter::numSamples() const
{
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (m_Stream)
      return m_Stream->numSamples();
    
  return 0;
}

/// Returns nonzero if there aren't any samples available for outputting.
int CTimeStretchFilter::isEmpty() const
{
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (m_Stream)
  {
     if (m_Stream->isEmpty())
        return true;
  }

  return false; // TODO: not sure if this is the right thing to return if m_Streams is NULL
}

bool CTimeStretchFilter::putSamplesInternal(const short *inBuffer, long inSamples)
{
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (!m_Stream)
    return false;
  
  m_Stream->putBuffer((BYTE *)inBuffer, inSamples);

  return true;
}

uint CTimeStretchFilter::receiveSamplesInternal(short *outBuffer, uint maxSamples)
{
  CAutoLock streamLock(&m_csStreamLock);
  ASSERT(m_Stream || (m_bBitstreaming && !m_Stream));

  if (!m_Stream)
    return 0;

  uint outSamples = numSamples();
  if (outSamples > maxSamples)
    outSamples = maxSamples;

  m_Stream->getBuffer((BYTE *)outBuffer, outSamples);

  return outSamples;
}