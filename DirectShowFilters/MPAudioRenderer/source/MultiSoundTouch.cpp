
#include "stdafx.h"

#include "MultiSoundTouch.h"

#define DEFINE_STREAM_FUNC(funcname, paramtype, paramname) \
  void CMultiSoundTouch::funcname(paramtype paramname) \
  { \
    if (m_Streams) \
    { \
      for(int i=0; i<m_nStreamCount; i++) \
        m_Streams[i].processor->funcname(paramname); \
    } \
  }

// move these in a separate common header 
#define SAFE_DELETE(p)       { if(p) { delete (p);     (p)=NULL; } }
#define SAFE_DELETE_ARRAY(p) { if(p) { delete[] (p);   (p)=NULL; } }

typedef void (CMultiSoundTouch::*DeInterleaveFunc)(const soundtouch::SAMPLETYPE *inBuffer, short *outBuffer, uint count);
typedef void (CMultiSoundTouch::*InterleaveFunc)(const soundtouch::SAMPLETYPE *inBuffer, short *outBuffer, uint count);

extern void Log(const char *fmt, ...);

// TODO add support for multiple channel pairs
static DWORD WINAPI ResampleThread(LPVOID lpParameter)
{
  ThreadData* data = static_cast<ThreadData*>(lpParameter);

  HRESULT hr = S_OK;

  ALLOCATOR_PROPERTIES propIn;
  ALLOCATOR_PROPERTIES propOut;
  propIn.cBuffers = 20;
  propIn.cbBuffer = 8192*20;
  propIn.cbPrefix = 0;
  propIn.cbAlign = 8;

  // These are wait handles for the thread stopping and new sample arrival
  HANDLE handles[2];
  handles[0] = data->stopThreadEvent;
  handles[1] = data->sampleArrivedEvent;

  // sample allocator per thread
  // TODO: use distictive name when adding multiple threads?

  CBaseAllocator* pMemAllocator = new CMemAllocator("output sample allocator", NULL, &hr);
  if (hr != S_OK)
  {
    Log("Failed to create sample allocator!");
    return 0;
  }

  pMemAllocator->SetProperties(&propIn, &propOut);
  hr = pMemAllocator->Commit();
  if (hr != S_OK)
  {
    Log("Failed to commit allocator properties!");
    return 0;
  }

  // Wait for the first sample to arrive before entering the resample loop	
  WaitForSingleObject(data->sampleArrivedEvent, INFINITE);

  while(true)
  {
    // Check event for stop thread
    if (WaitForSingleObject(data->stopThreadEvent, 0) == WAIT_OBJECT_0)
    {
      SetEvent(data->waitThreadToExitEvent);
      return 0;
    }

    //CAutoLock flushLock(data->flushReceiveLock);

    IMediaSample* sample = NULL;
    {
      bool waitForData = false;
      {
        CAutoLock cResampleLock(data->sampleQueueLock);
        if (data->sampleQueue->size() == 0)
        {
          // No data to be processed, reset the event in case we are looping too fast
          ResetEvent(data->sampleArrivedEvent);
          
          // Needs to be done outside the scope of cRendererLock 
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
          SetEvent(data->waitThreadToExitEvent);
          return 0;
        }
        // WAIT_OBJECT_0 + 1 <- new samples arrived
      }

      // Fetch one sample
      sample = data->sampleQueue->front();
      data->sampleQueue->erase(data->sampleQueue->begin());
    }

    BYTE *pMediaBuffer = NULL;
    long size = sample->GetActualDataLength();
    HRESULT hr = sample->GetPointer(&pMediaBuffer);
    
    if (hr == S_OK)
    {
      // Process the sample 
      // TODO: /4 needs to be fixed!
      data->resampler->putSamplesInternal((const short*)pMediaBuffer, size / 4);
      
      IMediaSample* outSample = NULL;
      pMemAllocator->GetBuffer(&outSample, NULL, NULL, 0);
      if (outSample)
      {
        // TODO: *4 needs to be fixed
        BYTE *pMediaBufferOut = NULL;
        outSample->GetPointer(&pMediaBufferOut);
        
        if (pMediaBufferOut)
        {
          unsigned int sampleLength = data->resampler->numSamples() * 4;
          data->resampler->receiveSamplesInternal((short*)pMediaBufferOut, propIn.cbBuffer/4);
          outSample->SetActualDataLength(sampleLength);

          { // lock that the playback thread wont access the queue at the same time
            CAutoLock cOutputQueueLock(data->sampleOutQueueLock);
            data->sampleOutQueue->push_back(outSample);
          }
        }
      }
    }
    
    // We aren't using the sample anymore (AddRef() is done when sample arrives)
    sample->Release();
  }
  return 0;
}

CMultiSoundTouch::CMultiSoundTouch(bool pUseThreads) 
: m_nChannels(0)
, m_Streams(NULL)
, m_nStreamCount(0)
, m_bUseThreads(pUseThreads)
, m_bFlushSamples(false)
{
  // Use separate thread per channnel pair?
  if (m_bUseThreads)
  {
    DWORD threadId = 0;
    CreateThread(0, 0, ResampleThread, (LPVOID)&m_ThreadData, 0, &threadId);

    m_hSampleArrivedEvent = CreateEvent(0, TRUE, FALSE, 0);
    m_hStopThreadEvent = CreateEvent(0, TRUE, FALSE, 0);
    m_hWaitThreadToExitEvent = CreateEvent(0, TRUE, FALSE, 0);

    m_ThreadData.buffer = &m_tempBuffer[0];
    m_ThreadData.resampler = this;
    m_ThreadData.sampleQueueLock = &m_sampleQueueLock;
    m_ThreadData.sampleOutQueueLock = &m_sampleOutQueueLock;
    m_ThreadData.sampleQueue = &m_sampleQueue;
    m_ThreadData.sampleOutQueue = &m_sampleOutQueue;
    m_ThreadData.sampleArrivedEvent = m_hSampleArrivedEvent;
    m_ThreadData.stopThreadEvent = m_hStopThreadEvent;
    m_ThreadData.waitThreadToExitEvent = m_hWaitThreadToExitEvent;
  }
}

CMultiSoundTouch::~CMultiSoundTouch()
{
  SetEvent(m_hStopThreadEvent);
  WaitForSingleObject(m_hWaitThreadToExitEvent, INFINITE);

  //CloseHandle(m_hSampleArrivedEvent);
  //CloseHandle(m_hWaitThreadToExitEvent);
  //CloseHandle(m_hStopThreadEvent);

  for(int i = 0; i < m_sampleQueue.size(); i++)
  {
    m_sampleQueue[i]->Release();
  }
  m_sampleQueue.clear();

  setChannels(0);
}


DEFINE_STREAM_FUNC(setRate,float, newRate)
DEFINE_STREAM_FUNC(setTempo, float, newTempo)
DEFINE_STREAM_FUNC(setRateChange, float, newRate)
DEFINE_STREAM_FUNC(setTempoChange, float, newTempo)
DEFINE_STREAM_FUNC(setPitchOctaves, float, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, int, newPitch)
DEFINE_STREAM_FUNC(setPitchSemiTones, float, newPitch)
DEFINE_STREAM_FUNC(setSampleRate, uint, srate)
DEFINE_STREAM_FUNC(flush,,)
DEFINE_STREAM_FUNC(clear,,)

// TODO: "unpack" the macros so ::flush can handle this as well
void CMultiSoundTouch::FlushQueues()
{
  m_bFlushSamples = true;
}

BOOL CMultiSoundTouch::setSetting(int settingId, int value)
{
  if (m_Streams)
  {
    for(int i=0; i<m_nStreamCount; i++)
      m_Streams[i].processor->setSetting(settingId, value);
    return true;
  } 
  return false;
}

uint CMultiSoundTouch::numUnprocessedSamples() const
{
  uint maxSamples = 0;
  for (int i=0 ; i<m_nStreamCount; i++)
  {
    uint samples = m_Streams[i].processor->numUnprocessedSamples();
    if (maxSamples == 0 || maxSamples < samples)
      maxSamples = samples;
  }
  return maxSamples;
}

/// Returns number of samples currently available.
uint CMultiSoundTouch::numSamples() const
{
  uint minSamples = 0;
  for (int i=0 ; i<m_nStreamCount; i++)
  {
    uint samples = m_Streams[i].processor->numSamples();
    if (minSamples == 0 || minSamples > samples)
      minSamples = samples;
  }
  return minSamples;
}

/// Returns nonzero if there aren't any samples available for outputting.
int CMultiSoundTouch::isEmpty() const
{
  for (int i=0 ; i<m_nStreamCount; i++)
  {
    if (m_Streams[i].processor->isEmpty())
      return true;
  }
  return false;
}


void CMultiSoundTouch::setChannels(int channels)
{
  int newStreamCount = channels/2 + (channels % 2);
  StreamProcessor *newStreams = NULL;
  m_nChannels = channels;
  if (channels > 0)
  {
    // create new streams
    newStreams = new StreamProcessor[newStreamCount];
    for(int i=0; i<newStreamCount; i++)
    {
      newStreams[i].channels = (channels>1)? 2:1;
      newStreams[i].processor = new soundtouch::SoundTouch();
      newStreams[i].processor->setChannels(newStreams[i].channels);
      channels -= newStreams[i].channels;
    }
  }
  // delete old ones
  StreamProcessor *oldStreams = m_Streams;
  int oldStreamCount = m_nStreamCount;
  {
    // this block might need to be protected by a lock
    m_Streams = newStreams;
    m_nStreamCount = newStreamCount;
  }
  for(int i=0; i<oldStreamCount; i++)
  {
    SAFE_DELETE(oldStreams[i].processor);
  }
  SAFE_DELETE_ARRAY(oldStreams);
}

bool CMultiSoundTouch::putSamples(const short *inBuffer, long inSamples)
{
  return putSamplesInternal(inBuffer, inSamples);
}

bool CMultiSoundTouch::processSample(IMediaSample *pMediaSample)
{
  CAutoLock cRendererLock(&m_sampleQueueLock);
  
  if (m_bFlushSamples)
  {
    m_bFlushSamples = false;
    for(int i = 0; i < m_sampleQueue.size(); i++)
    {
      m_sampleQueue[i]->Release();
    }
    m_sampleQueue.clear();
  }
    
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

  if(m_nChannels <= 2)
  {
    m_Streams[0].processor->putSamples(inBuffer, inSamples);
  }
  else
  {
    for(int i=0; i<m_nStreamCount; i++)
    {
      StreamProcessor *stream = &m_Streams[i];
      long inSamplesRemaining = inSamples;
      const short *streamInBuf = inBuffer;

      DeInterleaveFunc deInterleave = (stream->channels == 1? &CMultiSoundTouch::MonoDeInterleave : &CMultiSoundTouch::StereoDeInterleave);
      
      // input samples
      while(inSamplesRemaining)
      {
        uint batchLen = (inSamplesRemaining < SAMPLE_LEN? inSamplesRemaining : SAMPLE_LEN);
        (this->*deInterleave)(streamInBuf, m_temp, batchLen);
        stream->processor->putSamples(m_temp, batchLen);
        inSamplesRemaining -= batchLen;
        streamInBuf += batchLen * m_nChannels;
      }
      inBuffer += stream->channels;
    }
  }
  return true;
}

uint CMultiSoundTouch::receiveSamples(short **outBuffer, uint maxSamples)
{
  //return receiveSamplesInternal(outBuffer, maxSamples);
  static bool ignoreEmptySamples = true;

  //if(!outBuffer)
  //  return 0;

  IMediaSample* sample = NULL;
 


  {
    // Fetch one sample
    CAutoLock outputLock(&m_sampleOutQueueLock);
    
    // TODO use some event with small delay? Possible to get data before next sample 
    // is received in audio renderer and this method gets called again...
    if(m_sampleOutQueue.size() == 0)
    {
      if (!ignoreEmptySamples)
      {
        Log("receiveSamples: Output sample queue was empty! ");
      }
      return 0;
    }

    // just for logging (don't log the initial samples that are empty)
    ignoreEmptySamples = false; 

    sample = m_sampleOutQueue.front();
    m_sampleOutQueue.erase(m_sampleOutQueue.begin());
  }
  
  long sampleLength = sample->GetActualDataLength();
  
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

  // TODO fix /4
  return sampleLength / 4;
}


uint CMultiSoundTouch::receiveSamplesInternal(short *outBuffer, uint maxSamples)
{
  if(m_Streams == NULL)
    return 0;

  if(m_nChannels <= 2)
  {
    CAutoLock cRendererLock(&m_sampleOutQueueLock);
    return m_Streams[0].processor->receiveSamples(outBuffer, maxSamples);
  }
  else
  {
    uint outSamples = 0;
    for(int i=0; i<m_nStreamCount; i++)
    {
      StreamProcessor *stream = &m_Streams[i];

      InterleaveFunc interleave = (stream->channels == 1? &CMultiSoundTouch::MonoInterleave : &CMultiSoundTouch::StereoInterleave);
      
      // output samples
      short *streamOutBuf = outBuffer;
      long outSampleSpace = maxSamples;
      while(outSampleSpace > 0)
      {
        uint batchLen = (outSampleSpace < SAMPLE_LEN? outSampleSpace : SAMPLE_LEN);
        batchLen = stream->processor->receiveSamples(m_temp, batchLen);
        (this->*interleave)(m_temp, streamOutBuf, batchLen);
        if(batchLen == 0)
          break;
        outSampleSpace -= batchLen;
        streamOutBuf += batchLen * m_nChannels;
      }
      if(outSamples < maxSamples-outSampleSpace)
        outSamples = maxSamples-outSampleSpace;
      
      outBuffer += stream->channels;
    }
    return outSamples;
  }
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



void CMultiSoundTouch::StereoDeInterleave(const short *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  while(count--)
  {
    *outBuffer++ = *inBuffer;
    *outBuffer++ = *(inBuffer+1);
    inBuffer += m_nChannels;
  }
}

void CMultiSoundTouch::StereoInterleave(const soundtouch::SAMPLETYPE *inBuffer, short *outBuffer, uint count)
{
  while(count--)
  {
    *outBuffer = *inBuffer;
    *(outBuffer+1) = *inBuffer++;
    outBuffer += m_nChannels;
  }
}

void CMultiSoundTouch::MonoDeInterleave(const short *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  while(count--)
  {
    *outBuffer++ = *inBuffer;
    inBuffer += m_nChannels;
  }
}

void CMultiSoundTouch::MonoInterleave(const soundtouch::SAMPLETYPE *inBuffer, short *outBuffer, uint count)
{
  while(count--)
  {
    *outBuffer = *inBuffer;
    outBuffer += m_nChannels;
  }
}

