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
#include <map>

#include "MultiSoundTouch.h"

#define OUT_BUFFER_SIZE   16384
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

// move these in a separate common header 
#define SAFE_DELETE(p)       { if(p) { delete (p);     (p)=NULL; } }
#define SAFE_DELETE_ARRAY(p) { if(p) { delete[] (p);   (p)=NULL; } }
#define SAFE_RELEASE(p)      { if(p) { (p)->Release(); (p)=NULL; } }

extern void Log(const char *fmt, ...);

typedef struct tagSpeakerPair {
  DWORD dwLeft, dwRight;
  __inline DWORD PairMask()  { return dwLeft | dwRight; };
} SpeakerPair;

// List of speakers that should be handled as a pair
static SpeakerPair PairedSpeakers[] = {
  {SPEAKER_FRONT_LEFT, SPEAKER_FRONT_RIGHT},
  {SPEAKER_BACK_LEFT, SPEAKER_BACK_RIGHT},
  {SPEAKER_FRONT_LEFT_OF_CENTER, SPEAKER_FRONT_RIGHT_OF_CENTER},
  {SPEAKER_FRONT_CENTER, SPEAKER_BACK_CENTER}, // not sure about this one
  {SPEAKER_SIDE_LEFT, SPEAKER_SIDE_RIGHT},
  {SPEAKER_TOP_FRONT_LEFT, SPEAKER_TOP_FRONT_RIGHT},
  {SPEAKER_TOP_BACK_LEFT, SPEAKER_TOP_BACK_RIGHT},
  {SPEAKER_TOP_FRONT_CENTER, SPEAKER_TOP_BACK_CENTER},  // not sure about this one
  {NULL, NULL} // end marker
};

static DWORD gdwDefaultChannelMask[] = {
  0, // no channels - invalid
  KSAUDIO_SPEAKER_MONO,
  KSAUDIO_SPEAKER_STEREO,
  KSAUDIO_SPEAKER_STEREO | KSAUDIO_SPEAKER_GROUND_FRONT_CENTER,
  KSAUDIO_SPEAKER_QUAD,
  0, // 5 channels?
  KSAUDIO_SPEAKER_5POINT1_SURROUND,
  0, // 7 channels?
  KSAUDIO_SPEAKER_7POINT1_SURROUND
};

// TODO add support for multiple channel pairs
DWORD WINAPI CMultiSoundTouch::ResampleThreadEntryPoint(LPVOID lpParameter)
{
  return ((CMultiSoundTouch *)lpParameter)->ResampleThread();
}

CMultiSoundTouch::CMultiSoundTouch() 
: m_Streams(NULL)
, m_bFlushSamples(false)
, m_pMemAllocator(NULL)
, m_hSampleArrivedEvent(NULL)
, m_hStopThreadEvent(NULL)
, m_hWaitThreadToExitEvent(NULL)
, m_hThread(NULL)
, m_threadId(0)
, m_pWaveFormat(NULL)
, m_pPreviousSample(NULL)
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
}

void CMultiSoundTouch::StopResamplingThread()
{
  SetEvent(m_hStopThreadEvent);
  WaitForSingleObject(m_hWaitThreadToExitEvent, INFINITE);
}

DEFINE_STREAM_FUNC(setRate,float, newRate)
DEFINE_STREAM_FUNC(setTempo, float, newTempo)
DEFINE_STREAM_FUNC(setRateChange, float, newRate)
DEFINE_STREAM_FUNC(setTempoChange, float, newTempo)
DEFINE_STREAM_FUNC(setPitchOctaves, float, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, int, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, float, newPitch)
DEFINE_STREAM_FUNC(setSampleRate, uint, srate)
DEFINE_STREAM_FUNC(clear,,)


// flush requires a specific handling since we need to be able to use the CAutoLock
void CMultiSoundTouch::flush() 
{ 
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();
  
  CAutoLock allocatorLock(&m_allocatorLock);
  if (m_Streams) 
  { 
    for(int i=0; i<m_Streams->size(); i++) 
      m_Streams->at(i)->flush(); 
  } 
}


DWORD CMultiSoundTouch::ResampleThread()
{
  Log("Resampler thread - starting up - thread ID: %d", m_threadId);
  
  HRESULT hr = S_OK;

  // These are wait handles for the thread stopping and new sample arrival
  HANDLE handles[2];
  handles[0] = m_hStopThreadEvent;
  handles[1] = m_hSampleArrivedEvent;

  while(true)
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
          // Process the sample 
          putSamplesInternal((const short*)pMediaBuffer, size / m_pWaveFormat->Format.nBlockAlign);
          
          unsigned int sampleLength = numSamples();

          if (sampleLength > 0)
          {
            IMediaSample* outSample = NULL;
            m_pMemAllocator->GetBuffer(&outSample, NULL, NULL, 0);

            if (outSample)
            {
              BYTE *pMediaBufferOut = NULL;
              outSample->GetPointer(&pMediaBufferOut);
              
              if (pMediaBufferOut)
              {
                int maxBufferSamples = OUT_BUFFER_SIZE/m_pWaveFormat->Format.nBlockAlign;
                if (sampleLength > maxBufferSamples)
                  sampleLength = maxBufferSamples;
                outSample->SetActualDataLength(sampleLength * m_pWaveFormat->Format.nBlockAlign);
                receiveSamplesInternal((short*)pMediaBufferOut, sampleLength);

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

  if (hr != S_OK)
  {
    Log("Failed to create sample allocator!");
    delete pAllocator;
    return false;
  }

  hr = pAllocator->QueryInterface(IID_IMemAllocator, (void **)&m_pMemAllocator);

  if (hr != S_OK)
  {
    Log("Failed to get allocator interface!");
    delete pAllocator;
    return false;
  }

  m_pMemAllocator->SetProperties(&propIn, &propOut);
  hr = m_pMemAllocator->Commit();
  if (hr != S_OK)
  {
    Log("Failed to commit allocator properties!");
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
    if (minSamples == 0 || minSamples > samples)
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
  ASSERT(pwf->cbSize <= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX));
  memcpy(pwfe, pwf, sizeof(WAVEFORMATEX) + pwf->cbSize);
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

  if (pwf->cbSize >= 22)
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

    DWORD dwChannelMask = pwfe->dwChannelMask;

    std::map<DWORD, int> speakerOffset;
    int currOffset = 0;
    // Each bit position in dwChannelMask corresponds to a speaker position
    // try every bit position from 0 to 31
    for(DWORD dwSpeaker = 1; dwSpeaker != 0; dwSpeaker <<= 1) 
    {
      if (dwChannelMask & dwSpeaker)
      {
        speakerOffset[dwSpeaker] = currOffset;
        currOffset += pwfe->Format.wBitsPerSample / 8;
      }
    }

    ASSERT(speakerOffset.size() == pwfe->Format.nChannels);

    // TODO: First find a set of channels that can be used 
    // for syncing mono channels like LFE and Center
    
    // Now start adding channels
    bool isFloat = (pwfe->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT);
    // First try all speaker pairs
    for(SpeakerPair *pPair = PairedSpeakers; pPair->dwLeft; pPair++)
    {
      if ((pPair->PairMask() & dwChannelMask) == pPair->PairMask())
      {
        CSoundTouchEx *pStream = new CSoundTouchEx();
        pStream->setChannels(2);
        pStream->SetInputChannels(speakerOffset[pPair->dwLeft], speakerOffset[pPair->dwRight]);
        pStream->SetFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8, pwfe->Samples.wValidBitsPerSample, isFloat);
        pStream->SetOutputChannels(speakerOffset[pPair->dwLeft], speakerOffset[pPair->dwRight]);
        newStreams->push_back(pStream);
        dwChannelMask &= ~pPair->PairMask(); // mark channels as processed
      }
    }
    // Then add all remaining channels as mono streams
    // try every bit position from 0 to 31
    for(DWORD dwSpeaker = 1; dwSpeaker != 0; dwSpeaker <<= 1) 
    {
      if (dwChannelMask & dwSpeaker)
      {
        CSoundTouchEx *pStream = new CSoundTouchEx();
        // TODO: make this a mixing stream, so that the channel can be synchronized 
        // to the mix of the main channels (normally Front Left/Right if available)
        pStream->setChannels(1); 
        pStream->SetInputChannels(speakerOffset[dwSpeaker]);
        pStream->SetFormat(pwfe->Format.nBlockAlign, pwfe->Format.wBitsPerSample / 8, pwfe->Samples.wValidBitsPerSample, isFloat);
        pStream->SetOutputChannels(speakerOffset[dwSpeaker]);
        newStreams->push_back(pStream);
        // The following is only necessary if we skip some channels
        // currently we don't
        //dwChannelMask &= ~dwSpeaker; // mark channel as processed        
      }
    }
  }

  // delete old ones
  std::vector<CSoundTouchEx *> *oldStreams = m_Streams;
  WAVEFORMATEXTENSIBLE *pOldFormat = m_pWaveFormat;

  m_Streams = newStreams;
  m_pWaveFormat = pWaveFormat;

  if (pOldFormat)
    delete[] (BYTE *)pOldFormat;

  if (oldStreams)
  {
    for(int i=0; i<oldStreams->size(); i++)
    {
      SAFE_DELETE(oldStreams->at(i));
    }
    SAFE_DELETE(oldStreams);
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
  CAutoLock cRendererLock(&m_sampleOutQueueLock);

  pSample->AddRef();
  m_sampleOutQueue.push_back(pSample);

  return S_OK;
}

uint CMultiSoundTouch::receiveSamples(short **outBuffer, uint maxSamples)
{
  static bool ignoreEmptySamples = true;
  IMediaSample* sample = NULL;
 
  {
    // Fetch one sample
    CAutoLock outputLock(&m_sampleOutQueueLock);
    
    // TODO use some event with small delay? Possible to get data before next sample 
    // is received in audio renderer and this method gets called again...
    if(m_sampleOutQueue.empty())
    {
      if (!ignoreEmptySamples)
      {
        Log("receiveSamples: Output sample queue was empty!");
      }
      return 0;
    }

    // just for logging (don't log the initial samples that are empty)
    ignoreEmptySamples = false; 

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

