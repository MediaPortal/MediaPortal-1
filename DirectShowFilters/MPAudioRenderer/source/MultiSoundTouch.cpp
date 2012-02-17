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

// parts of the code for AC3 encoding is derived from ffdshow / ffmpeg 

#include "stdafx.h"
#include <map>

#include "alloctracing.h"

#include "MultiSoundTouch.h"

#define AC3_FRAME_LENGTH  1536
#define OUT_BUFFER_SIZE   65536
#define OUT_BUFFER_COUNT  20

#define DEFINE_STREAM_FUNC(funcname, paramtype, paramname) \
  void CMultiSoundTouch::funcname(paramtype paramname) \
  { \
    if (m_Streams) \
    { \
      for(int i=0; i<m_Streams->size(); i++) \
        m_Streams->at(i)->funcname(paramname); \
    } \
  }

template<class T> inline T odd2even(T x)
{
  return x&1 ? x + 1 : x;
}

// TODO add support for multiple channel pairs
DWORD WINAPI CMultiSoundTouch::ResampleThreadEntryPoint(LPVOID lpParameter)
{
  return ((CMultiSoundTouch *)lpParameter)->ResampleThread();
}

CMultiSoundTouch::CMultiSoundTouch(bool pEnableAC3Encoding, int AC3bitrate, CSyncClock* pClock) 
: m_pClock(pClock)
, m_Streams(NULL)
, m_bFlushSamples(false)
, m_pMemAllocator(NULL)
, m_hSampleArrivedEvent(NULL)
, m_hStopThreadEvent(NULL)
, m_hWaitThreadToExitEvent(NULL)
, m_hThread(NULL)
, m_threadId(0)
, m_pWaveFormat(NULL)
, m_pPreviousSample(NULL)
, m_pEncoder(NULL)
, m_bEnableAC3Encoding(pEnableAC3Encoding)
, m_dAC3bitrate(AC3bitrate)
, m_fCurrentTempo(1.0)
, m_fNewAdjustment(1.0)
, m_fCurrentAdjustment(1.0)
, m_fNewTempo(1.0)
, m_nFrameCorr(0)
, m_nPrevFrameCorr(0)
{
  // Use separate thread per channnel pair?
  m_hSampleArrivedEvent = CreateEvent(0, FALSE, FALSE, 0);
  m_hStopThreadEvent = CreateEvent(0, FALSE, FALSE, 0);
  m_hWaitThreadToExitEvent = CreateEvent(0, FALSE, FALSE, 0);

  if (InitializeAllocator())
  {
    m_hThread = CreateThread(0, 0, CMultiSoundTouch::ResampleThreadEntryPoint, (LPVOID)this, 0, &m_threadId);
  }
  ZeroMemory(m_temp, 2*SAMPLE_LEN);
}

CMultiSoundTouch::~CMultiSoundTouch()
{
  // releases allocator's samples
  BeginFlush();

  {
    CAutoLock allocatorLock(&m_allocatorLock);
    SAFE_RELEASE(m_pMemAllocator);
  }

  if (m_hSampleArrivedEvent)
    CloseHandle(m_hSampleArrivedEvent);
  if (m_hWaitThreadToExitEvent)
    CloseHandle(m_hWaitThreadToExitEvent);
  if (m_hStopThreadEvent)
    CloseHandle(m_hStopThreadEvent);
  if (m_hThread)
    CloseHandle(m_hThread);

  SetFormat((PWAVEFORMATEXTENSIBLE)NULL);

  if(m_bEnableAC3Encoding && FAILED(CloseAC3Encoder()))
  {
    Log("Error when closing down the AC3 encoder!");
  }
}

void CMultiSoundTouch::StopResamplingThread()
{
  SetEvent(m_hStopThreadEvent);
  WaitForSingleObject(m_hWaitThreadToExitEvent, INFINITE);
}

DEFINE_STREAM_FUNC(setRate, double, newRate)
DEFINE_STREAM_FUNC(setRateChange, double, newRate)
DEFINE_STREAM_FUNC(setTempoChange, double, newTempo)
DEFINE_STREAM_FUNC(setPitchOctaves, double, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, int, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, double, newPitch)
DEFINE_STREAM_FUNC(setSampleRate, uint, srate)

// clear requires a specific handling since we need to be able to use the CAutoLock
void CMultiSoundTouch::clear() 
{ 
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();
  
  CAutoLock allocatorLock(&m_allocatorLock);
  if (m_Streams) 
  { 
    for(int i=0; i<m_Streams->size(); i++) 
      m_Streams->at(i)->clear(); 
  } 
}

// flush requires a specific handling since we need to be able to use the CAutoLock
void CMultiSoundTouch::flush() 
{ 
  CAutoLock allocatorLock(&m_allocatorLock);
  if (m_Streams) 
  { 
    for(int i=0; i<m_Streams->size(); i++) 
      m_Streams->at(i)->flush(); 
  } 
}

void CMultiSoundTouch::setTempo(double newTempo, double newAdjustment)
{
  m_fNewTempo = newTempo;
  m_fNewAdjustment = newAdjustment;
}

void CMultiSoundTouch::setTempoInternal(double newTempo, double newAdjustment)
{
  if (m_Streams) 
  { 
    for(int i=0; i<m_Streams->size(); i++) 
      m_Streams->at(i)->setTempo(newTempo * newAdjustment); 

    m_fCurrentTempo = newTempo;
    m_fCurrentAdjustment = newAdjustment;
  } 
}

DWORD CMultiSoundTouch::ResampleThread()
{
  Log("Resampler thread - starting up - thread ID: %d", m_threadId);
  
  HRESULT hr = S_OK;
  
  return 0;

  // These are wait handles for the thread stopping and new sample arrival
  HANDLE handles[2];
  handles[0] = m_hStopThreadEvent;
  handles[1] = m_hSampleArrivedEvent;

  while (true)
  {
    // Check event for stop thread
    if (WaitForSingleObject(m_hStopThreadEvent, 0) == WAIT_OBJECT_0)
    {
      Log("Resampler thread - closing down - thread ID: %d", m_threadId);
      SetEvent(m_hWaitThreadToExitEvent);
      return 0;
    }

    IMediaSample* sample = NULL;
    {
      bool waitForData = false;
      {
        CAutoLock sampleQueueLock(&m_sampleQueueLock);
        if (m_sampleQueue.empty())
        {
          // Make sure that we wont fall thru with the previous sample's 
          // arrival event in the WaitForMultipleObjects stage 
          ResetEvent(m_hSampleArrivedEvent);

          // Actual waiting beeds to be done outside the scope of sampleQueueLock 
          // since we would be creating a deadlock otherwise 
          waitForData = true;
        }
      }

      if (waitForData)
      {
        // 1) No data was available, waiting until at least one sample is present
        // 2) Exit requested for the thread
        DWORD result = WaitForMultipleObjects(2, handles, false, INFINITE);
        if (result == WAIT_OBJECT_0)
        {
          Log("Resampler thread - closing down - thread ID: %d", m_threadId);
          SetEvent(m_hWaitThreadToExitEvent);
          return 0;
        }
        else if (result == WAIT_OBJECT_0 + 1)
        {
          // new sample ready
        }
        else
        {
          DWORD error = GetLastError();
          Log("Resampler thread - WaitForMultipleObjects failed: %d", error);
        }
      }
      
      { // Fetch one sample
        CAutoLock sampleQueueLock(&m_sampleQueueLock);
        if (!m_sampleQueue.empty())
        {
          sample = m_sampleQueue.front();
          m_sampleQueue.erase(m_sampleQueue.begin());
        }
      }
    }

    {
      CAutoLock allocatorLock(&m_allocatorLock);

      if (sample && m_pMemAllocator)
      {
        BYTE *pMediaBuffer = NULL;
        long size = sample->GetActualDataLength();
        hr = sample->GetPointer(&pMediaBuffer);
        
        if ((hr == S_OK) && m_pMemAllocator)
        {
          uint unprocessedSamplesBefore = numUnprocessedSamples();
          uint unprocessedSamplesAfter = 0;

          UINT32 nFrames = size / m_pWaveFormat->Format.nBlockAlign;
          REFERENCE_TIME estimatedSampleDuration = nFrames * UNITS / m_pWaveFormat->Format.nSamplesPerSec;

          double bias = m_pClock->GetBias();
          double adjustment = m_pClock->Adjustment();
          double AVMult = m_pClock->SuggestedAudioMultiplier(estimatedSampleDuration, bias, adjustment);
          setTempoInternal(AVMult, 1.0); // this should be the same as previous line, but in future we want to get rid of the 2nd parameter

          // Process the sample 
          putSamplesInternal((const short*)pMediaBuffer, size / m_pWaveFormat->Format.nBlockAlign);
          unprocessedSamplesAfter = numUnprocessedSamples();

          UINT32 nOutFrames = numSamples();

          if ((!m_pEncoder && nOutFrames > 0) || (m_pEncoder && nOutFrames >= AC3_FRAME_LENGTH))
          {
            UINT32 nOrigOutFrames = nOutFrames;
            m_nPrevFrameCorr = m_nFrameCorr;

            if (m_pEncoder)
            {
              // with AC3 encoder enabled, the resampled buffer is not emptied completely
              m_nFrameCorr = nOutFrames % AC3_FRAME_LENGTH;
            }
            else
            {
              m_nFrameCorr = 0;
            }
            nOutFrames = nOutFrames - m_nFrameCorr;  
            
            UINT32 nInFrames = (size / m_pWaveFormat->Format.nBlockAlign) - unprocessedSamplesAfter + unprocessedSamplesBefore;
            double rtSampleDuration = (double)nInFrames * (double)UNITS / (double)m_pWaveFormat->Format.nSamplesPerSec;
            double rtProcessedSampleDuration = (double)(nOrigOutFrames - m_nPrevFrameCorr) * (double)UNITS / (double)m_pWaveFormat->Format.nSamplesPerSec;

            m_pClock->AudioResampled(rtProcessedSampleDuration, rtSampleDuration, bias, adjustment, AVMult);
            
            IMediaSample* outSample = NULL;
            m_pMemAllocator->GetBuffer(&outSample, NULL, NULL, 0);

            if (outSample)
            {
              BYTE *pMediaBufferOut = NULL;
              outSample->GetPointer(&pMediaBufferOut);
              
              if (pMediaBufferOut)
              {
                int maxBufferSamples = OUT_BUFFER_SIZE / m_pWaveFormat->Format.nBlockAlign;
                if (nOutFrames > maxBufferSamples)
                  nOutFrames = maxBufferSamples;

                if (m_pEncoder)
                {
                  BYTE outbuf[OUT_BUFFER_SIZE];
                  BYTE resampledData[OUT_BUFFER_SIZE];
                  long resultLenght = 0;

                  while (numSamples() >= AC3_FRAME_LENGTH)
                  {
                    (void)receiveSamplesInternal((short*)resampledData, AC3_FRAME_LENGTH);
                    int AC3lenght = ac3_encoder_frame(m_pEncoder, (short*)resampledData, outbuf, sizeof(outbuf));
                    resultLenght += CreateAC3Bitstream(outbuf, AC3lenght, pMediaBufferOut + resultLenght);
                  }

                  outSample->SetActualDataLength(resultLenght);

                  //Log("sampleLength: %d resultLenght: %d remaining samples: %d", sampleLength,resultLenght,numSamples());
                }
                else
                {
                  outSample->SetActualDataLength(nOutFrames * m_pWaveFormat->Format.nBlockAlign);
                  receiveSamplesInternal((short*)pMediaBufferOut, nOutFrames);
                  //Log("sampleLength: %d remaining samples: %d", sampleLength, numSamples());
                }
                { // lock that the playback thread wont access the queue at the same time
                  CAutoLock cOutputQueueLock(&m_sampleOutQueueLock);
                  m_sampleOutQueue.push_back(outSample);
                }
              }
            }
          }
          else
          {
            //Log("Zero Sized sample");
          }
        }
      
        // We aren't using the sample anymore (AddRef() is done when sample arrives)
        sample->Release();
      }
    }
  }
  
  Log("Resampler thread - closing down - thread ID: %d", m_threadId);
  return 0;
}

bool CMultiSoundTouch::InitializeAllocator()
{
  ALLOCATOR_PROPERTIES propIn;
  ALLOCATOR_PROPERTIES propOut;
  propIn.cBuffers = OUT_BUFFER_COUNT;
  propIn.cbBuffer = OUT_BUFFER_SIZE*OUT_BUFFER_COUNT;
  propIn.cbPrefix = 0;
  propIn.cbAlign = 8;

  HRESULT hr = S_OK;

  CMemAllocator *pAllocator = new CMemAllocator("output sample allocator", NULL, &hr);

  if (FAILED(hr))
  {
    Log("Failed to create sample allocator (0x%08x)", hr);
    delete pAllocator;
    return false;
  }

  hr = pAllocator->QueryInterface(IID_IMemAllocator, (void **)&m_pMemAllocator);

  if (FAILED(hr))
  {
    Log("Failed to get allocator interface (0x%08x)", hr);
    delete pAllocator;
    return false;
  }

  m_pMemAllocator->SetProperties(&propIn, &propOut);
  hr = m_pMemAllocator->Commit();
  if (FAILED(hr))
  {
    Log("Failed to commit allocator properties (0x%08x)", hr);
    SAFE_RELEASE(m_pMemAllocator);
    return false;
  }
  return true;
}


void CMultiSoundTouch::BeginFlush()
{
  // Resampler thread waiting in the IMemAllocator::GetBuffer method return with an error. 
  // Further calls to GetBuffer fail, until the IMemAllocator::Commit method is called.
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();

  { // Make sure that the resample thread is not accessing allocator
    CAutoLock allocatorLock(&m_allocatorLock);

    { // Release samples that are in input queue
      CAutoLock cQueueLock(&m_sampleQueueLock);
      for(int i = 0; i < m_sampleQueue.size(); i++)
      {
        m_sampleQueue[i]->Release();
      }
      m_sampleQueue.clear();
    }
    
    { // Release samples that are in output queue
      CAutoLock cOutputQueueLock(&m_sampleOutQueueLock);
      for(int i = 0; i < m_sampleOutQueue.size(); i++)
      {
        m_sampleOutQueue[i]->Release();
      }
      m_sampleOutQueue.clear();
    }

    if (m_pPreviousSample)
    {
      m_pPreviousSample->Release();
      m_pPreviousSample = NULL;
    }
  } 
}

void CMultiSoundTouch::EndFlush()
{
  if (m_pMemAllocator)
    m_pMemAllocator->Commit();
}

BOOL CMultiSoundTouch::setSetting(int settingId, int value)
{
  // TODO should LFE channel have separate settings since it is by nature quite
  // different when it comes to frequency response
  if (m_Streams)
  {
    for(int i=0; i<m_Streams->size(); i++)
      m_Streams->at(i)->setSetting(settingId, value);
    return true;
  } 
  return false;
}

uint CMultiSoundTouch::numUnprocessedSamples() const
{
  uint maxSamples = 0;
  for (int i=0 ; i<m_Streams->size(); i++)
  {
    uint samples = m_Streams->at(i)->numUnprocessedSamples();
    if (maxSamples == 0 || maxSamples < samples)
      maxSamples = samples;
  }
  return maxSamples;
}

/// Returns number of samples currently available.
uint CMultiSoundTouch::numSamples() const
{
  uint minSamples = 0;
  for (int i=0 ; i<m_Streams->size(); i++)
  {
    uint samples = m_Streams->at(i)->numSamples();
    if (i == 0 || minSamples > samples)
      minSamples = samples;
  }
  return minSamples;
}

/// Returns nonzero if there aren't any samples available for outputting.
int CMultiSoundTouch::isEmpty() const
{
  for (int i=0 ; i<m_Streams->size(); i++)
  {
    if (m_Streams->at(i)->isEmpty())
      return true;
  }
  return false;
}

HRESULT CMultiSoundTouch::ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE *pwfe, WAVEFORMATEX *pwf)
{
  //ASSERT(pwf->cbSize <= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX));
  memcpy(pwfe, pwf, sizeof(WAVEFORMATEX)/* + pwf->cbSize*/);
  pwfe->Format.cbSize = sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX);
  switch(pwfe->Format.wFormatTag)
  {
  case WAVE_FORMAT_PCM:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
    break;
  case WAVE_FORMAT_IEEE_FLOAT:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
    break;
  default:
    return VFW_E_TYPE_NOT_ACCEPTED;
  }
  if (pwfe->Format.nChannels >= 1 && pwfe->Format.nChannels <= 8)
  {
    pwfe->dwChannelMask = gdwDefaultChannelMask[pwfe->Format.nChannels];
    if (pwfe->dwChannelMask == 0)
      return VFW_E_TYPE_NOT_ACCEPTED;
  }
  else
    return VFW_E_TYPE_NOT_ACCEPTED;

  pwfe->Samples.wValidBitsPerSample = pwfe->Format.wBitsPerSample;
  pwfe->Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
  return S_OK;
}

HRESULT CMultiSoundTouch::CheckFormat(WAVEFORMATEX *pwf)
{
  if (pwf == NULL)
    return CheckFormat((WAVEFORMATEXTENSIBLE *) NULL);

  if (pwf->cbSize >= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX))
  {
    if (pwf->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
      return CheckFormat((WAVEFORMATEXTENSIBLE *)pwf);
    else
      return VFW_E_TYPE_NOT_ACCEPTED;
  }

  WAVEFORMATEXTENSIBLE wfe;
  // Setup WFE
  HRESULT hr = ToWaveFormatExtensible(&wfe, pwf);
  if (FAILED(hr))
    return hr;
  return CheckFormat(&wfe);
}

HRESULT CMultiSoundTouch::CheckFormat(WAVEFORMATEXTENSIBLE *pwfe)
{
  if (pwfe != NULL &&
      pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_PCM &&
      pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
    return VFW_E_TYPE_NOT_ACCEPTED;
  return S_OK; // Should we return OK if format is NULL?
}

HRESULT CMultiSoundTouch::SetFormat(WAVEFORMATEX *pwf)
{
  if (pwf == NULL)
    return SetFormat((WAVEFORMATEXTENSIBLE *) NULL);

  if (pwf->cbSize >= 22)
  {
    if (pwf->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
      return SetFormat((WAVEFORMATEXTENSIBLE *)pwf);
    else
      return VFW_E_TYPE_NOT_ACCEPTED;
  }

  WAVEFORMATEXTENSIBLE wfe;
  // Setup WFE
  HRESULT hr = ToWaveFormatExtensible(&wfe, pwf);
  if (FAILED(hr))
    return hr;
  return SetFormat(&wfe);
}

HRESULT CMultiSoundTouch::SetFormat(WAVEFORMATEXTENSIBLE *pwfe)
{
  std::vector<CSoundTouchEx *> *newStreams = NULL;
  WAVEFORMATEXTENSIBLE *pWaveFormat = NULL;

  if (pwfe != NULL)
  {
    // First verify format is supported
    if (pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_PCM &&
        pwfe->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
      return VFW_E_TYPE_NOT_ACCEPTED;

    DWORD dwChannelMask = pwfe->dwChannelMask;

    // If AC3 encoding is enabled we should not accepted a 
    // format that cannot be output to AC3
    if (m_bEnableAC3Encoding && pwfe->Format.nChannels > 2 && 
       (dwChannelMask & SPEAKER_AC3_VALID_POSITIONS) != dwChannelMask)
      return VFW_E_TYPE_NOT_ACCEPTED;

    newStreams =  new std::vector<CSoundTouchEx *>;
    if (!newStreams)
      return E_OUTOFMEMORY;

    int size = sizeof(WAVEFORMATEX) + pwfe->Format.cbSize;
    pWaveFormat = (WAVEFORMATEXTENSIBLE *)new BYTE[size];
    if (!pWaveFormat)
    {
      delete newStreams;
      return E_OUTOFMEMORY;
    }
    memcpy(pWaveFormat, pwfe, size);

    std::map<DWORD, int> inSpeakerOffset;
    std::map<DWORD, int> outSpeakerOffset;
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

    if (m_bEnableAC3Encoding && pwfe->Format.nChannels > 2)
    {
      // If we ever support other formats with different speaker offsets
      // just create anoter speaker order table and set these three accordingly
      int nMaxOutSpeakers = cAC3SpeakerOrder;
      DWORD *pSpeakerOrder = gdwAC3SpeakerOrder;
      WORD wOutBytesPerSample = 2;

      currOffset = 0;
      for (int i = 0; i < nMaxOutSpeakers; i++)
      {
        DWORD dwSpeaker = pSpeakerOrder[i];
        if (dwChannelMask & dwSpeaker)
        {
          outSpeakerOffset[dwSpeaker] = currOffset;
          currOffset += wOutBytesPerSample;
        }
      }
    }
    else
    {
      // PCM output, 1-to-1 mapping of input to output
      outSpeakerOffset.insert(inSpeakerOffset.begin(), inSpeakerOffset.end());
    }

    // TODO: First create the base downmixing coefficients
    // for syncing mono channels like LFE and Center
    
    // Now start adding channels
    bool isFloat = (pwfe->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT);
    // First try all speaker pairs
    for (SpeakerPair *pPair = PairedSpeakers; pPair->dwLeft; pPair++)
    {
      if ((pPair->PairMask() & dwChannelMask) == pPair->PairMask())
      {
        CSoundTouchEx *pStream = new CSoundTouchEx();
        pStream->setChannels(2);
        pStream->SetInputChannels(inSpeakerOffset[pPair->dwLeft], inSpeakerOffset[pPair->dwRight]);
        pStream->SetInputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8, pwfe->Samples.wValidBitsPerSample, isFloat);
        pStream->SetOutputChannels(outSpeakerOffset[pPair->dwLeft], outSpeakerOffset[pPair->dwRight]);
        if (m_bEnableAC3Encoding)
          pStream->SetOutputFormat(pwfe->Format.nChannels*2, 2, 16, false);
        else
          pStream->SetOutputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8, pwfe->Samples.wValidBitsPerSample, isFloat);
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
        pStream->SetInputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8, pwfe->Samples.wValidBitsPerSample, isFloat);
        pStream->SetOutputChannels(outSpeakerOffset[dwSpeaker]);
        if (m_bEnableAC3Encoding)
          pStream->SetOutputFormat(pwfe->Format.nChannels*2, 2, 16, false);
        else
          pStream->SetOutputFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8, pwfe->Samples.wValidBitsPerSample, isFloat);
        pStream->SetupFormats();
        newStreams->push_back(pStream);
        // The following is only necessary if we skip some channels
        // currently we don't
        //dwChannelMask &= ~dwSpeaker; // mark channel as processed        
      }
    }
  }

  // Need to lock the resampling thread from accessing the streams
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();

  CAutoLock allocatorLock(&m_allocatorLock);

  // delete old ones
  std::vector<CSoundTouchEx *> *oldStreams = m_Streams;
  WAVEFORMATEXTENSIBLE *pOldFormat = m_pWaveFormat;

  m_Streams = newStreams;
  m_pWaveFormat = pWaveFormat;

  if (pOldFormat)
    delete[] (BYTE *)pOldFormat;

  if (oldStreams)
  {
    for (int i = 0; i < oldStreams->size(); i++)
    {
      SAFE_DELETE(oldStreams->at(i));
    }
    SAFE_DELETE(oldStreams);
  }

  setTempoInternal(m_fNewTempo, m_fNewAdjustment);

  if (m_pEncoder && pwfe->Format.nChannels <= 2)
  {
    (void)CloseAC3Encoder();
  }

  if (m_bEnableAC3Encoding && m_pWaveFormat && pwfe->Format.nChannels > 2)
  {
    (void)OpenAC3Encoder(m_dAC3bitrate, m_pWaveFormat->Format.nChannels, m_pWaveFormat->Format.nSamplesPerSec);
  }

  return S_OK;
}

bool CMultiSoundTouch::putSamples(const short *inBuffer, long inSamples)
{
  return putSamplesInternal(inBuffer, inSamples);
}

bool CMultiSoundTouch::processSample(IMediaSample *pMediaSample)
{
  CAutoLock cRendererLock(&m_sampleQueueLock);

  pMediaSample->AddRef();
  m_sampleQueue.push_back(pMediaSample);

  // Signal to the thread that there is new sample available
  SetEvent(m_hSampleArrivedEvent);
  return true;
}


bool CMultiSoundTouch::putSamplesInternal(const short *inBuffer, long inSamples)
{
  if(m_Streams == NULL)
    return false;

  for(int i=0; i<m_Streams->size(); i++)
  {
    CSoundTouchEx *stream = m_Streams->at(i);
    //stream->putSamples(inBuffer, inSamples);
    stream->putBuffer((BYTE *)inBuffer, inSamples);
  }
  return true;
}

HRESULT CMultiSoundTouch::GetNextSample(IMediaSample** pSample, bool pReleaseOnly)
{
  CAutoLock outputLock(&m_sampleOutQueueLock);

  if (pReleaseOnly)
  {
    if (m_pPreviousSample)
    {
      m_pPreviousSample->Release();
      m_pPreviousSample = NULL;
    }
    return S_OK;
  }

  if(m_sampleOutQueue.empty())
  {
    return S_FALSE;
  }

  // Fetch one sample
  (*pSample) = m_sampleOutQueue.front();
  m_sampleOutQueue.erase(m_sampleOutQueue.begin());

  if (m_pPreviousSample)
    m_pPreviousSample->Release();
  m_pPreviousSample = (*pSample);

  return S_OK;
}


HRESULT CMultiSoundTouch::QueueSample(IMediaSample* pSample)
{
  if (pSample && m_pMemAllocator)
  {
    CAutoLock allocatorLock(&m_allocatorLock);
    
    IMediaSample* outSample = NULL;
    m_pMemAllocator->GetBuffer(&outSample, NULL, NULL, 0);

    if (outSample)
    {
      BYTE *pMediaBufferOut = NULL;
      BYTE *pMediaBufferIn = NULL;
      outSample->GetPointer(&pMediaBufferOut);
      pSample->GetPointer(&pMediaBufferIn);

      if (pMediaBufferOut && pMediaBufferIn)
      {
        long sampleLength = pSample->GetActualDataLength();
        outSample->SetActualDataLength(sampleLength);
        memcpy(pMediaBufferOut, pMediaBufferIn, sampleLength);

        { // lock that the playback thread wont access the queue at the same time
          CAutoLock cOutputQueueLock(&m_sampleOutQueueLock);
          m_sampleOutQueue.push_back(outSample);
        }
      }
    }
  }

  return S_OK;
}

uint CMultiSoundTouch::receiveSamples(short **outBuffer, uint maxSamples)
{
  IMediaSample* sample = NULL;
 
  {
    // Fetch one sample
    CAutoLock outputLock(&m_sampleOutQueueLock);
    
    // TODO use some event with small delay? Possible to get data before next sample 
    // is received in audio renderer and this method gets called again...
    if(m_sampleOutQueue.empty())
    {
      return 0;
    }

    sample = m_sampleOutQueue.front();
    m_sampleOutQueue.erase(m_sampleOutQueue.begin());
  }
  
  long sampleLength = sample->GetActualDataLength();
  
  if (sampleLength == 0)
  {
    sample->Release();
    return 0;
  }

  BYTE *pSampleBuffer = NULL;
  HRESULT hr = sample->GetPointer(&pSampleBuffer);

  if (hr != S_OK)
  {
    Log("receiveSamples: Failed to get sample buffer");
    sample->Release();
    return 0;
  }

  (*outBuffer) = (short*)malloc(sampleLength);
  memcpy(*outBuffer, pSampleBuffer, sampleLength);
  sample->Release();

  return sampleLength / m_pWaveFormat->Format.nBlockAlign;
}


uint CMultiSoundTouch::receiveSamplesInternal(short *outBuffer, uint maxSamples)
{
  if(m_Streams == NULL)
    return 0;

  uint outSamples = numSamples();
  CAutoLock cRendererLock(&m_sampleOutQueueLock);
  if (outSamples > maxSamples)
    outSamples = maxSamples;

  for(int i=0; i<m_Streams->size(); i++)
  {
    //m_Streams->at(i)->receiveSamples(outBuffer, outSamples);
    m_Streams->at(i)->getBuffer((BYTE *)outBuffer, outSamples);
  }
  return outSamples;
}

// Internal functions to separate/merge streams out of/into sample buffers (from IMediaSample)
// All buffers are arrays of Int16.
// The sample buffers are assumed to be SampleCount*Channels words long (1 word = 2 bytes)
// The stereo stream buffers are assumed to be 2*SampleCount words long (1 word = sizeof(soundtouch::SAMPLETYPE))
// The mono stream buffers are assumed to be SampleCount words long (1 word = sizeof(soundtouch::SAMPLETYPE))
bool CMultiSoundTouch::ProcessSamples(const short *inBuffer, long inSamples, short *outBuffer, long *outSamples, long maxOutSamples)
{
  if (!putSamples(inBuffer, inSamples))
    return false;

  *outSamples = receiveSamples(&outBuffer, maxOutSamples);
  return true;
}


HRESULT CMultiSoundTouch::OpenAC3Encoder(unsigned int bitrate, unsigned int channels, unsigned int sampleRate)
{
  Log("OpenEncoder - Creating AC3 encoder - bitrate: %d sampleRate: %d channels: %d", bitrate, sampleRate, channels);

  delete m_pEncoder;

  m_pEncoder = ac3_encoder_open();
  if (!m_pEncoder) return S_FALSE;

  m_pEncoder->bit_rate = bitrate;
  m_pEncoder->sample_rate = sampleRate;
  m_pEncoder->channels = channels;

  if (ac3_encoder_init(m_pEncoder) < 0) 
  {
    ac3_encoder_close(m_pEncoder);
    m_pEncoder = NULL;
  }
	
  return S_OK;
}

HRESULT CMultiSoundTouch::CloseAC3Encoder()
{
  if (!m_pEncoder) return S_FALSE;

  Log("CloseEncoder - Closing AC3 encoder");

  ac3_encoder_close(m_pEncoder);
  m_pEncoder = NULL;

  return S_OK;
}

long CMultiSoundTouch::CreateAC3Bitstream(void *buf, size_t size, BYTE *pDataOut)
{
  size_t length = 0;
  size_t repetition_burst = 0x800; // 2048 = AC3 

  unsigned int size2 = AC3_FRAME_LENGTH * 4;
  length = 0;

  // Add 4 more words (8 bytes) for AC3/DTS (for backward compatibility, should be *4 for other codecs)
  // AC3/DTS streams start with 8 blank bytes (why, don't know but let's going on with)
  while (length < odd2even(size) + sizeof(WORD) * 8)
    length += repetition_burst;

  while (length < size2)
  length += repetition_burst;

  if (length == 0) length = repetition_burst;

  // IEC 61936 structure writing (HDMI bitstream, SPDIF)
  DWORD type = 0x0001;
  short subDataType = 0; 
  short errorFlag = 0;
  short datatypeInfo = 0;
  short bitstreamNumber = 0;
  
  type=1; // CODEC_ID_SPDIF_AC3

  DWORD Pc=type | (subDataType << 5) | (errorFlag << 7) | (datatypeInfo << 8) | (bitstreamNumber << 13);

  WORD *pDataOutW=(WORD*)pDataOut; // Header is filled with words instead of bytes

  // Preamble : 16 bytes for AC3/DTS, 8 bytes for other formats
  int index = 0;
  pDataOutW[0] = pDataOutW[1] = pDataOutW[2] = pDataOutW[3] = 0; // Stuffing at the beginning, not sure if this is useful
  index = 4; // First additional four words filled with 0 only for backward compatibility for AC3/DTS

  // Fill after the input buffer with zeros if any extra bytes
  if (length > 8 + index * 2 + size)
  {
    // Fill the output buffer with zeros 
    memset(pDataOut + 8 + index * 2 + size, 0, length - 8 - index * 2 - size); 
  }

  // Fill the 8 bytes (4 words) of IEC header
  pDataOutW[index++] = 0xf872;
  pDataOutW[index++] = 0x4e1f;
  pDataOutW[index++] = (WORD)Pc;
  pDataOutW[index++] = WORD(size * 8); // size in bits for AC3/DTS

  // Data : swap bytes from first byte of data on size length (input buffer lentgh)
  _swab((char*)buf,(char*)&pDataOutW[index],(int)(size & ~1));
  if (size & 1) // _swab doesn't like odd number.
  {
    pDataOut[index * 2 + size] = ((BYTE*)buf)[size - 1];
    pDataOut[index * 2 - 1 + size] = 0;
  }

  return length;
}