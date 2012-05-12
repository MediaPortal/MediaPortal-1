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
    if (m_Streams) \
    { \
      for(int i=0; i<m_Streams->size(); i++) \
        m_Streams->at(i)->funcname(paramname); \
    } \
  }

CTimeStretchFilter::CTimeStretchFilter(AudioRendererSettings* pSettings, CSyncClock* pClock) :
  m_pSettings(pSettings),
  m_Streams(NULL),
  m_fCurrentTempo(1.0),
  m_fNewAdjustment(1.0),
  m_fCurrentAdjustment(1.0),
  m_fNewTempo(1.0),
  m_pMediaType(NULL),
  m_rtInSampleTime(0),
  m_rtNextIncomingSampleTime(0),
  m_pClock(pClock)
{
}

CTimeStretchFilter::~CTimeStretchFilter(void)
{
  Log("CTimeStretchFilter - destructor - instance 0x%x", this);
  
  SetEvent(m_hStopThreadEvent);
  WaitForSingleObject(m_hThread, INFINITE);

  CAutoLock lock(&m_csResources);
  SetFormat(NULL);
  DeleteMediaType(m_pMediaType);

  Log("CTimeStretchFilter - destructor - instance 0x%x - end", this);
}

//Initialization
HRESULT CTimeStretchFilter::Init()
{
  HRESULT hr = InitAllocator();
  if (FAILED(hr))
    return hr;

  m_hSampleEvents.push_back(m_hInputAvailableEvent);
  m_hSampleEvents.push_back(m_hOOBCommandAvailableEvent);
  m_hSampleEvents.push_back(m_hStopThreadEvent);

  m_dwSampleWaitObjects.push_back(S_OK);
  m_dwSampleWaitObjects.push_back(MPAR_S_OOB_COMMAND_AVAILABLE);
  m_dwSampleWaitObjects.push_back(MPAR_S_THREAD_STOPPING);

  setTempoChange(0);
  setPitchSemiTones(0);
  setSetting(SETTING_USE_QUICKSEEK, m_pSettings->m_bQuality_USE_QUICKSEEK);
  setSetting(SETTING_USE_AA_FILTER, m_pSettings->m_bQuality_USE_AA_FILTER);
  setSetting(SETTING_AA_FILTER_LENGTH, m_pSettings->m_lQuality_AA_FILTER_LENGTH);
  setSetting(SETTING_SEQUENCE_MS, m_pSettings->m_lQuality_SEQUENCE_MS); 
  setSetting(SETTING_SEEKWINDOW_MS, m_pSettings->m_lQuality_SEEKWINDOW_MS);
  setSetting(SETTING_OVERLAP_MS, m_pSettings->m_lQuality_SEQUENCE_MS);

  return CQueuedAudioSink::Init();
}

HRESULT CTimeStretchFilter::PutSample(IMediaSample* pSample)
{
  if (m_pSettings->m_bUseTimeStretching)
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

#ifdef INTEGER_SAMPLES
  // only accept 16bit int
  if (pwfx->Format.wBitsPerSample != 16 || pwfx->SubFormat != KSDATAFORMAT_SUBTYPE_PCM)
    return VFW_E_TYPE_NOT_ACCEPTED;
#else 
  // only accept 32bit float
  if (pwfx->Format.wBitsPerSample != 32 || pwfx->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
    return VFW_E_TYPE_NOT_ACCEPTED;
#endif

  if (FormatsEqual(pwfx, m_pInputFormat))
  {
    *pChOrder = m_chOrder;
    return S_OK;
  }

  bool bApplyChanges = (nApplyChangesDepth != 0);
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
  if (FAILED(hr))
    return hr;

  hr = VFW_E_CANNOT_CONNECT;
  
  if (!pwfx)
    return SetFormat(NULL);

  if (bApplyChanges)
  {
    LogWaveFormat(pwfx, "TS   - applying ");

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
    SetFormat(pwfx);
  }
  else
    LogWaveFormat(pwfx, "TS   -          ");

  m_chOrder = *pChOrder;

  return S_OK;
}

HRESULT CTimeStretchFilter::CheckSample(IMediaSample* pSample)
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
    uint unprocessedSamplesBefore = numUnprocessedSamples();
     uint zeros = flushEx();

    uint unprocessedSamplesAfter = numUnprocessedSamples();
    UINT32 outFramesAfter = numSamples();
    
    UINT32 totalSamples = zeros + unprocessedSamplesBefore;
    UINT32 totalProcessedSamples = totalSamples - unprocessedSamplesAfter;
    //double bias = (double)totalProcessedSamples / (double)outFramesAfter;

    REFERENCE_TIME estimatedSampleDuration = totalProcessedSamples * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

    double bias = m_pClock->GetBias();
    double adjustment = m_pClock->Adjustment();
    double AVMult = m_pClock->SuggestedAudioMultiplier(estimatedSampleDuration, bias, adjustment);
    setTempoInternal(AVMult, 1.0);

    CreateOutput(totalProcessedSamples, outFramesAfter, bias, adjustment, AVMult, true);
    
    // Empty SoundTouch's buffers
    clear();

    // Apply format change
    ChannelOrder chOrder;
    hr = NegotiateFormat((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, 1, &chOrder);
    pSample->SetDiscontinuity(false);

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
  std::vector<CSoundTouchEx*>* newStreams = NULL;

  if (pwfe)
  {
    // First verify format is supported
    if (pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_PCM &&
        pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
      return VFW_E_TYPE_NOT_ACCEPTED;

    DWORD dwChannelMask = pwfe->dwChannelMask;

    newStreams =  new std::vector<CSoundTouchEx*>;
    if (!newStreams)
      return E_OUTOFMEMORY;

    map<DWORD, int> inSpeakerOffset;
    map<DWORD, int> outSpeakerOffset;
    int currOffset = 0;
    // Each bit position in dwChannelMask corresponds to a speaker position
    // try every bit position from 0 to 31
    for (DWORD dwSpeaker = 1; dwSpeaker != 0; dwSpeaker <<= 1) 
    {
      if (dwChannelMask & dwSpeaker)
      {
        inSpeakerOffset[dwSpeaker] = currOffset;
        currOffset += pwfe->Format.wBitsPerSample / 8;
      }
    }

    ASSERT(inSpeakerOffset.size() == pwfe->Format.nChannels);

    // PCM output, 1-to-1 mapping of input to output
    outSpeakerOffset.insert(inSpeakerOffset.begin(), inSpeakerOffset.end());

    // TODO: First create the base downmixing coefficients
    // for syncing mono channels like LFE and Center
    
    // Now start adding channels
    // First try all speaker pairs
    for (SpeakerPair *pPair = PairedSpeakers; pPair->dwLeft; pPair++)
    {
      if ((pPair->PairMask() & dwChannelMask) == pPair->PairMask())
      {
        CSoundTouchEx* pStream = new CSoundTouchEx();
        pStream->setChannels(2);
        pStream->SetInputChannels(inSpeakerOffset[pPair->dwLeft], inSpeakerOffset[pPair->dwRight]);
        pStream->SetInputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8);
        pStream->SetOutputChannels(outSpeakerOffset[pPair->dwLeft], outSpeakerOffset[pPair->dwRight]);
        pStream->SetOutputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8);
        pStream->SetupFormats();
        newStreams->push_back(pStream);
        dwChannelMask &= ~pPair->PairMask(); // mark channels as processed
      }
    }
    // Then add all remaining channels as mono streams
    // try every bit position from 0 to 31
    for (DWORD dwSpeaker = 1; dwSpeaker != 0; dwSpeaker <<= 1) 
    {
      if (dwChannelMask & dwSpeaker)
      {
        CSoundTouchEx *pStream = new CSoundTouchEx();
        // TODO: make this a mixing stream, so that the channel can be synchronized 
        // to the mix of the main channels (normally Front Left/Right if available)
        pStream->setChannels(1); 
        pStream->SetInputChannels(inSpeakerOffset[dwSpeaker]);
        pStream->SetInputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8);
        pStream->SetOutputChannels(outSpeakerOffset[dwSpeaker]);
        pStream->SetOutputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8);
        pStream->SetupFormats();
        newStreams->push_back(pStream);
        // The following is only necessary if we skip some channels
        // currently we don't
        //dwChannelMask &= ~dwSpeaker; // mark channel as processed        
      }
    }
  }

  // delete old ones
  std::vector<CSoundTouchEx*>* oldStreams = m_Streams;
  m_Streams = newStreams;

  if (oldStreams)
  {
    for (int i = 0; i < oldStreams->size(); i++)
    {
      SAFE_DELETE(oldStreams->at(i));
    }
    SAFE_DELETE(oldStreams);
  }

  if (m_Streams)
  {
    setTempoInternal(m_fNewTempo, m_fNewAdjustment);
    setSampleRate(pwfe->Format.nSamplesPerSec);
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

void CTimeStretchFilter::CheckStreamContinuity(IMediaSample* pSample)
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
    REFERENCE_TIME rtGap = m_rtNextIncomingSampleTime - rtStart;

    if (abs(rtGap) > MAX_SAMPLE_TIME_ERROR)
    {
      m_rtNextIncomingSampleTime = rtStart;

      if (m_nSampleNum == 0) 
      {
        m_rtInSampleTime = rtStart;

        Log("CTimeStretchFilter - resyncing in start of stream - m_rtStart: %6.3f rtStart: %6.3f m_rtInSampleTime: %6.3f", 
          m_rtStart / 10000000.0, rtStart / 10000000.0, m_rtInSampleTime / 10000000.0);
      }
      else
      {
        Log("CTimeStretchFilter - stream discontinuity detected!");
        ASSERT(false); // shouldnt ever happen - stream is stream is continuous 
      }
    }

    UINT nFrames = pSample->GetActualDataLength() / m_pInputFormat->Format.nBlockAlign;
    m_rtNextIncomingSampleTime += nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;
  }
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

        if (m_pNextOutSample)
          m_pNextOutSample.Release();

        flush();

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

        if (CheckSample(sample) == S_FALSE)
        {
          DeleteMediaType(m_pMediaType);
          sample->GetMediaType(&m_pMediaType);
        }

        CheckStreamContinuity(sample);
        m_nSampleNum++;

        hr = sample->GetPointer(&pMediaBuffer);

        if ((hr == S_OK) && m_pMemAllocator)
        {
          uint unprocessedSamplesBefore = numUnprocessedSamples();
          uint unprocessedSamplesAfter = 0;

          UINT32 nFrames = size / m_pOutputFormat->Format.nBlockAlign;
          REFERENCE_TIME estimatedSampleDuration = nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

          double bias = m_pClock->GetBias();
          double adjustment = m_pClock->Adjustment();
          double AVMult = m_pClock->SuggestedAudioMultiplier(estimatedSampleDuration, bias, adjustment);
          setTempoInternal(AVMult, 1.0); // this should be the same as previous line, but in future we want to get rid of the 2nd parameter

          // Process the sample 
          putSamplesInternal((const short*)pMediaBuffer, size / m_pOutputFormat->Format.nBlockAlign);
          unprocessedSamplesAfter = numUnprocessedSamples();

          UINT32 nInFrames = (size / m_pOutputFormat->Format.nBlockAlign) - unprocessedSamplesAfter + unprocessedSamplesBefore;
          UINT32 nOutFrames = numSamples();
          
          CreateOutput(nInFrames, nOutFrames, bias, adjustment, AVMult, false);
        }
      }
    }
  }
}

void CTimeStretchFilter::CreateOutput(UINT32 nInFrames, UINT32 nOutFrames, double dBias, double dAdjustment, double dAVMult, bool bFlushPartialSample)
{
  HRESULT hr = S_OK;
  UINT32 maxBufferFrames = DEFAULT_OUT_BUFFER_SIZE / m_pOutputFormat->Format.nBlockAlign;
  UINT32 nOutFramesTotal = 0;

  while (nOutFrames > 0)
  {
    // try to get an output buffer if none available
    if (!m_pNextOutSample && FAILED(hr = RequestNextOutBuffer(m_rtInSampleTime)))
    {
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

  if (nOutFramesTotal > 0)
  {
    double rtSampleDuration = (double)nInFrames * (double)UNITS / (double)m_pOutputFormat->Format.nSamplesPerSec;
    double rtProcessedSampleDuration = (double)(nOutFramesTotal) * (double)UNITS / (double)m_pOutputFormat->Format.nSamplesPerSec;

    m_pClock->AudioResampled(rtProcessedSampleDuration, rtSampleDuration, dBias, dAdjustment, dAVMult);

    //Log(m_pClock->DebugData());
  }
}

void CTimeStretchFilter::OutputSample(bool bForce)
{
  if (m_pNextOutSample)
  {
    UINT32 sampleLen = m_pNextOutSample->GetActualDataLength();
    if (bForce || (sampleLen + m_pOutputFormat->Format.nBlockAlign > DEFAULT_OUT_BUFFER_SIZE))
    {
      HRESULT hr = OutputNextSample();
      m_rtInSampleTime += sampleLen / m_pOutputFormat->Format.nBlockAlign * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

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

// clear requires a specific handling since we need to be able to use the CAutoLock
void CTimeStretchFilter::clear() 
{ 
  CAutoLock allocatorLock(&m_allocatorLock);
  if (m_Streams) 
  { 
    for(int i = 0; i < m_Streams->size(); i++) 
      m_Streams->at(i)->clear(); 
  } 
}

// flush requires a specific handling since we need to be able to use the CAutoLock
void CTimeStretchFilter::flush() 
{ 
  CAutoLock allocatorLock(&m_allocatorLock);
  if (m_Streams) 
  { 
    for(int i = 0; i < m_Streams->size(); i++) 
      m_Streams->at(i)->flush(); 
  }
}

uint CTimeStretchFilter::flushEx()
{ 
  CAutoLock allocatorLock(&m_allocatorLock);
  uint minZeros = 0;
  if (m_Streams) 
  { 
    for(int i = 0; i < m_Streams->size(); i++)
    {
      uint zeros = m_Streams->at(i)->flushEx();
      if (i == 0 || minZeros > zeros)
        minZeros = zeros;
    }
  }

  return minZeros;
}

void CTimeStretchFilter::setTempo(float newTempo, float newAdjustment)
{
  m_fNewTempo = newTempo;
  m_fNewAdjustment = newAdjustment;
}

void CTimeStretchFilter::setTempoInternal(float newTempo, float newAdjustment)
{
  if (m_Streams) 
  { 
    for (int i = 0; i < m_Streams->size(); i++) 
      m_Streams->at(i)->setTempo(newTempo * newAdjustment); 

    m_fCurrentTempo = newTempo;
    m_fCurrentAdjustment = newAdjustment;
  } 
}

BOOL CTimeStretchFilter::setSetting(int settingId, int value)
{
  // TODO should LFE channel have separate settings since it is by nature quite
  // different when it comes to frequency response
  if (m_Streams)
  {
    for(int i = 0; i < m_Streams->size(); i++)
      m_Streams->at(i)->setSetting(settingId, value);
    return true;
  } 
  return false;
}

uint CTimeStretchFilter::numUnprocessedSamples() const
{
  uint maxSamples = 0;
  for (int i = 0; i < m_Streams->size(); i++)
  {
    uint samples = m_Streams->at(i)->numUnprocessedSamples();
    if (maxSamples == 0 || maxSamples < samples)
      maxSamples = samples;
  }
  return maxSamples;
}

/// Returns number of samples currently available.
uint CTimeStretchFilter::numSamples() const
{
  uint minSamples = 0;
  for (int i = 0; i < m_Streams->size(); i++)
  {
    uint samples = m_Streams->at(i)->numSamples();
    if (i == 0 || minSamples > samples)
      minSamples = samples;
  }

  return minSamples;
}

/// Returns nonzero if there aren't any samples available for outputting.
int CTimeStretchFilter::isEmpty() const
{
  for (int i = 0; i < m_Streams->size(); i++)
  {
    if (m_Streams->at(i)->isEmpty())
      return true;
  }

  return false;
}

bool CTimeStretchFilter::putSamplesInternal(const short *inBuffer, long inSamples)
{
  if (!m_Streams)
    return false;

  for (int i = 0; i < m_Streams->size(); i++)
  {
    CSoundTouchEx* stream = m_Streams->at(i);
    stream->putBuffer((BYTE *)inBuffer, inSamples);
  }

  return true;
}

uint CTimeStretchFilter::receiveSamplesInternal(short *outBuffer, uint maxSamples)
{
  if (!m_Streams)
    return 0;

  uint outSamples = numSamples();
  if (outSamples > maxSamples)
    outSamples = maxSamples;

  for (int i = 0; i < m_Streams->size(); i++)
  {
    m_Streams->at(i)->getBuffer((BYTE *)outBuffer, outSamples);
  }

  return outSamples;
}