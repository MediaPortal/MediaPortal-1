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
#include "SoundTouchEx.h"

#include "alloctracing.h"

extern void Log(const char *fmt, ...);

// conversion functions
#define NOISE_COEFF   0.7f
#define SAMPLE_FLOAT  double

__inline long ReadInt24(const BYTE *pValue)
{
  return (long)(((DWORD)*(WORD *)pValue) << 8) | (((DWORD)pValue[2]) << 24);
}

__inline void WriteInt24(BYTE *pDest, long value)
{
  value >>= 8;
  *(WORD *)pDest = (WORD)value;
  pDest[2] = (BYTE)(value >> 16);
}

__inline SAMPLE_FLOAT WhiteNoise()
{
  return ((SAMPLE_FLOAT)rand())/(RAND_MAX/2) - 1;
}

__inline short SampleFloatToInt16(SAMPLE_FLOAT sample, long bitMask, register long& error)
{
  if (sample > 1.0)
    return SHRT_MAX;
  if (sample < -1.0)
    return -SHRT_MAX;

  sample *= LONG_MAX;
#ifdef DITHER_SAMPLES
  sample += WhiteNoise() * NOISE_COEFF * bitMask + error;
  error = ((long)sample) & bitMask;
#endif
  return (((long)sample) & ~bitMask) >> 16;
}

__inline short SampleInt32ToInt16(long sample, long bitMask, long& error)
{
#ifdef DITHER_SAMPLES
  sample += WhiteNoise() * NOISE_COEFF * bitMask + error;
  error = sample & bitMask;
#endif
  return (sample & ~bitMask) >> 16;
}

__inline SAMPLE_FLOAT SampleInt16ToFloat(short sample)
{
  return ((SAMPLE_FLOAT)sample) / SHRT_MAX;
}

__inline short SampleInt16ToInt16(short sample, long bitMask, long& error)
{
  long lSample = ((long)sample)<<16;
#ifdef DITHER_SAMPLES
  sample += WhiteNoise() * NOISE_COEFF * bitMask + error;
  error = sample & bitMask;
#endif
  return (lSample & ~bitMask)>>16;
}

__inline long SampleInt16ToInt32(short sample, long bitMask)
{
  return ((long)sample) << 16;
}

__inline SAMPLE_FLOAT SampleInt32ToFloat(long sample)
{
  return ((SAMPLE_FLOAT)sample) / LONG_MAX;
}

__inline long SampleFloatToInt32(SAMPLE_FLOAT sample, long bitMask, register long& error)
{
  if (sample > 1.0)
    return LONG_MAX;
  if (sample < -1.0)
    return -LONG_MAX;

  sample *= LONG_MAX;
  return (((long)sample) & ~bitMask);
}


CSoundTouchEx::CSoundTouchEx()
: m_hResampleThread(NULL)
, m_dwResampleThreadId(0)
, m_hInSampleArrivedEvent(NULL)
, m_hStopResampleThreadEvent(NULL)
//, m_hWaitResampleThreadToExitEvent(NULL)

{
  // default to stereo
  m_nInFrameSize = 4;
  m_nInBytesPerSample = 2;
  m_nInBitsPerSample = 16;
  m_bInFloatSamples = false;
  m_nInLeftOffset = 0;
  m_nInRightOffset = 2;
  m_pfnDeInterleave = &CSoundTouchEx::StereoDeInterleaveInt16;

  m_nOutFrameSize = 4;
  m_nOutBytesPerSample = 2;
  m_nOutBitsPerSample = 16;
  m_bOutFloatSamples = false;
  m_nOutLeftOffset = 0;
  m_nOutRightOffset = 2;
  m_pfnInterleave = &CSoundTouchEx::StereoInterleaveInt16;

  ResetDithering();

  m_hInSampleArrivedEvent = CreateEvent(0, FALSE, FALSE, 0);
  m_hStopResampleThreadEvent = CreateEvent(0, FALSE, FALSE, 0);
  //m_hWaitResampleThreadToExitEvent = CreateEvent(0, FALSE, FALSE, 0);

}

CSoundTouchEx::~CSoundTouchEx() 
{
}

// Resampling thread support

bool CSoundTouchEx::StartResampling()
{
//  m_hResampleThread = CreateThread(0, 0, CSoundTouchEx::ResampleThreadEntryPoint, (LPVOID)this, 0, &m_dwResampleThreadId);
  return m_hResampleThread != NULL;
}

bool CSoundTouchEx::StopResampling()
{
  if (m_hResampleThread == NULL)
    return false;

  SetEvent(m_hStopResampleThreadEvent);
  WaitForSingleObject(m_hResampleThread, INFINITE);
  CloseHandle(m_hResampleThread);
  m_hResampleThread = NULL;

  return true;
}

/*
DWORD WINAPI CSoundTouchEx::ResampleThreadEntryPoint(LPVOID lpParameter)
{
  return ((CSoundTouchEx *)lpParameter)->ResampleThread();
}

DWORD CSoundTouchEx::ResampleThread()
{
  Log("Resampler thread - starting up - thread ID: %d", m_dwResampleThreadId);
  
  HRESULT hr = S_OK;

  // These are wait handles for the thread stopping and new sample arrival
  HANDLE handles[2];
  handles[0] = m_hStopResampleThreadEvent;
  handles[1] = m_hInSampleArrivedEvent;

  CComPtr<IMediaSample> outSample = NULL;
  UINT nOutSampleOffset = 0;
  DWORD dwOutSampleSeqNo = 0;

  while(true)
  {
    // Check event for stop thread
    if (WaitForSingleObject(m_hStopResampleThreadEvent, 0) == WAIT_OBJECT_0)
    {
      Log("Resampler thread - closing down - thread ID: %d", m_dwResampleThreadId);
      //SetEvent(m_hWaitThreadToExitEvent);
      return 0;
    }

    CComPtr<IMediaSample> sample = NULL;
    {
      bool waitForData = false;
      {
        CAutoLock sampleQueueLock(&m_InSampleQueueLock);
        if (m_InSampleQueue.empty())
        {
          // Make sure that we wont fall thru with the previous sample's 
          // arrival event in the WaitForMultipleObjects stage 
          ResetEvent(m_hInSampleArrivedEvent);

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
          Log("Resampler thread - closing down - thread ID: %d", m_dwResampleThreadId);
          //SetEvent(m_hWaitThreadToExitEvent);
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
        CAutoLock sampleQueueLock(&m_InSampleQueueLock);
        if (!m_InSampleQueue.empty())
        {
          sample = m_InSampleQueue.front();
          m_InSampleQueue.erase(m_InSampleQueue.begin());
        }
      }
    }

    if (sample)
    {
      BYTE *pMediaBuffer = NULL;
      long size = sample->GetActualDataLength();
      hr = sample->GetPointer(&pMediaBuffer);
      
      if (hr == S_OK)
      {
        // Process the sample 
        putBuffer(pMediaBuffer, size / m_nInFrameSize);
        

        m_pMemAllocator->GetBuffer(&outSample, NULL, NULL, 0);
        if (outSample)
        {
          BYTE *pMediaBufferOut = NULL;
          outSample->GetPointer(&pMediaBufferOut);
          
          if (pMediaBufferOut)
          {
            unsigned int sampleLength = numSamples();
            int maxBufferSamples = OUT_BUFFER_SIZE/m_nOutFrameSize;
            if (sampleLength > maxBufferSamples)
              sampleLength = maxBufferSamples;
            outSample->SetActualDataLength(sampleLength * m_nOutFrameSize);
            receiveSamplesInternal((short*)pMediaBufferOut, sampleLength);

            { // lock that the playback thread wont access the queue at the same time
              CAutoLock cOutputQueueLock(&m_sampleOutQueueLock);
              m_sampleOutQueue.push_back(outSample);
            }
          }
        }
      }
      
      // We aren't using the sample anymore (AddRef() is done when sample arrives)
      //sample->Release();
    }
  }
  
  Log("Resampler thread - closing down - thread ID: %d", m_dwResampleThreadId);
  return 0;
}
*/

// Queue Handling

void CSoundTouchEx::putBuffer(const BYTE *pInBuffer, int numSamples)
{
  int inSamplesRemaining = numSamples;

  // input samples
  while(inSamplesRemaining > 0)
  {
    int batchLen = (inSamplesRemaining < BATCH_LEN? inSamplesRemaining : BATCH_LEN);
    (this->*m_pfnDeInterleave)(pInBuffer, m_tempBuffer, batchLen);
    putSamples(m_tempBuffer, batchLen);
    inSamplesRemaining -= batchLen;
    pInBuffer += batchLen * m_nInFrameSize;
  }
}

void CSoundTouchEx::putMediaSample(IMediaSample *pMediaSample)
{
  BYTE *pMediaBuffer = NULL;
  long size = pMediaSample->GetActualDataLength();
  HRESULT hr = pMediaSample->GetPointer(&pMediaBuffer);

  if (hr == S_OK)
    putBuffer(pMediaBuffer, size / m_nInFrameSize);
}

void CSoundTouchEx::queueMediaSample(IMediaSample *pMediaSample)
{
  // TODO: code to handle queued media samples
  CAutoLock cQueueLock(&m_InSampleQueueLock);
  
  //pMediaSample->AddRef();
  m_InSampleQueue.push_back(pMediaSample);

  // Signal to the thread that there is new sample available
  SetEvent(m_hInSampleArrivedEvent);
  //return true;
}

int CSoundTouchEx::getBuffer(BYTE *pOutBuffer, int maxSamples)
{
  long outSampleSpace = maxSamples;
  while(outSampleSpace > 0)
  {
    uint batchLen = (outSampleSpace < BATCH_LEN? outSampleSpace : BATCH_LEN);
    batchLen = receiveSamples(m_tempBuffer, batchLen);
    
    if(batchLen == 0)
      break;

    (this->*m_pfnInterleave)(m_tempBuffer, pOutBuffer, batchLen);

    outSampleSpace -= batchLen;
    pOutBuffer += batchLen * m_nOutFrameSize;
  }
  return maxSamples-outSampleSpace;
}

//bool CSoundTouchEx::SetFormat(int frameSize, int bytesPerSample, int bitsPerSample, bool isFloat)
//{
//  m_nInFrameSize = m_nOutFrameSize = frameSize;
//  m_nInBytesPerSample = m_nOutBytesPerSample = bytesPerSample;
//  m_nInBitsPerSample = m_nOutBitsPerSample = bitsPerSample;
//  m_bInFloatSamples = m_bOutFloatSamples = isFloat;
//
//  return SetupFormats();
//}

bool CSoundTouchEx::SetInputFormat(int frameSize, int bytesPerSample, int bitsPerSample, bool isFloat)
{
  m_nInFrameSize = frameSize;
  m_nInBytesPerSample = bytesPerSample;
  m_nInBitsPerSample = bitsPerSample;
  m_bInFloatSamples = isFloat;

  return true;
}

void CSoundTouchEx::SetInputChannels(int leftOffset, int rightOffset)
{
  m_nInLeftOffset = leftOffset;
  m_nInRightOffset = rightOffset;
}

bool CSoundTouchEx::SetOutputFormat(int frameSize, int bytesPerSample, int bitsPerSample, bool isFloat)
{
  m_nOutFrameSize = frameSize;
  m_nOutBytesPerSample = bytesPerSample;
  m_nOutBitsPerSample = bitsPerSample;
  m_bOutFloatSamples = isFloat;

  return true;
}

void CSoundTouchEx::SetOutputChannels(int leftOffset, int rightOffset)
{
  m_nOutLeftOffset = leftOffset;
  m_nOutRightOffset = rightOffset;
}

// TODO: provide better error information
bool CSoundTouchEx::SetupFormats()
{
  ResetDithering();

  // select appropriate input de-interleaving method
  if (m_bInFloatSamples)
  {
    if (m_nInBytesPerSample != 4)
      return false;
    m_pfnDeInterleave = (channels == 1? &CSoundTouchEx::MonoDeInterleaveFloat : &CSoundTouchEx::StereoDeInterleaveFloat);
  }
  else switch(m_nInBytesPerSample)
  {
  case 2:
    m_pfnDeInterleave = (channels == 1? &CSoundTouchEx::MonoDeInterleaveInt16 : &CSoundTouchEx::StereoDeInterleaveInt16);
    break;
  case 3:
    m_pfnDeInterleave = (channels == 1? &CSoundTouchEx::MonoDeInterleaveInt24 : &CSoundTouchEx::StereoDeInterleaveInt24);
    break;
  case 4:
    m_pfnDeInterleave = (channels == 1? &CSoundTouchEx::MonoDeInterleaveInt32 : &CSoundTouchEx::StereoDeInterleaveInt32);
    break;
  default:
    return false;
  }

  // select appropriate output interleaving method
  if (m_bOutFloatSamples)
  {
    if (m_nOutBytesPerSample != 4)
      return false;
    m_pfnInterleave = (channels == 1? &CSoundTouchEx::MonoInterleaveFloat : &CSoundTouchEx::StereoInterleaveFloat);
  }
  else switch(m_nOutBytesPerSample)
  {
  case 2:
    m_pfnInterleave = (channels == 1? &CSoundTouchEx::MonoInterleaveInt16 : &CSoundTouchEx::StereoInterleaveInt16);
    break;
  case 3:
    m_pfnInterleave = (channels == 1? &CSoundTouchEx::MonoInterleaveInt24 : &CSoundTouchEx::StereoInterleaveInt24);
    break;
  case 4:
    m_pfnInterleave = (channels == 1? &CSoundTouchEx::MonoInterleaveInt32 : &CSoundTouchEx::StereoInterleaveInt32);
    break;
  default:
    return false;
  }

  return true;
}

#ifdef DITHER_SAMPLES
void CSoundTouchEx::ResetDithering()
{
  m_lInSampleErrorLeft = 
    m_lInSampleErrorRight =
    m_lOutSampleErrorLeft =
    m_lOutSampleErrorRight = 0;
}
#endif

// Internal functions to separate/merge streams out of/into sample buffers (from IMediaSample)

#ifdef INTEGER_SAMPLES

void CSoundTouchEx::StereoDeInterleaveFloat(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;

  while(count--)
  {
    *outBuffer++ = SampleFloatToInt16(*(float *)(pInBuffer + m_nInLeftOffset), 0x0000FFFFL, m_lInSampleErrorLeft);
    *outBuffer++ = SampleFloatToInt16(*(float *)(pInBuffer + m_nInRightOffset), 0x0000FFFFL, m_lInSampleErrorRight);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoDeInterleaveInt16(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;
  while(count--)
  {
    *outBuffer++ = *(short *)(pInBuffer + m_nInLeftOffset);
    *outBuffer++ = *(short *)(pInBuffer + m_nInRightOffset);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoDeInterleaveInt24(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;
  
  while(count--)
  {
    *outBuffer++ = SampleInt32ToInt16(ReadInt24(pInBuffer + m_nInLeftOffset), 0x0000FFFFL, m_lInSampleErrorLeft);
    *outBuffer++ = SampleInt32ToInt16(ReadInt24(pInBuffer + m_nInRightOffset), 0x0000FFFFL, m_lInSampleErrorRight);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoDeInterleaveInt32(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;
  
  while(count--)
  {
    *outBuffer++ = SampleInt32ToInt16(*(long *)(pInBuffer + m_nInLeftOffset), 0x0000FFFFL, m_lInSampleErrorLeft);
    *outBuffer++ = SampleInt32ToInt16(*(long *)(pInBuffer + m_nInRightOffset), 0x0000FFFFL, m_lInSampleErrorRight);
    pInBuffer += m_nInFrameSize;
  }
}


void CSoundTouchEx::StereoInterleaveFloat(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;
  
  while(count--)
  {
    *(double *)(pOutBuffer + m_nOutLeftOffset) = SampleInt16ToFloat(*inBuffer++);
    *(double *)(pOutBuffer + m_nOutRightOffset) = SampleInt16ToFloat(*inBuffer++);
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::StereoInterleaveInt16(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(short *)(pOutBuffer + m_nOutLeftOffset) = SampleInt16ToInt16(*inBuffer++, bitMask, m_lOutSampleErrorLeft);
    *(short *)(pOutBuffer + m_nOutRightOffset) = SampleInt16ToInt16(*inBuffer++, bitMask, m_lOutSampleErrorRight);
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::StereoInterleaveInt24(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    WriteInt24(pOutBuffer + m_nOutLeftOffset, SampleInt16ToInt32(*inBuffer++, bitMask));
    WriteInt24(pOutBuffer + m_nOutRightOffset, SampleInt16ToInt32(*inBuffer++, bitMask));
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::StereoInterleaveInt32(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(long *)(pOutBuffer + m_nOutLeftOffset) = SampleInt16ToInt32(*inBuffer++, bitMask);
    *(long *)(pOutBuffer + m_nOutRightOffset) = SampleInt16ToInt32(*inBuffer++, bitMask);
    pOutBuffer += m_nOutFrameSize;
  }
}


void CSoundTouchEx::MonoDeInterleaveFloat(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;
  
  while(count--)
  {
    *outBuffer++ = SampleFloatToInt16(*(double *)pInBuffer, 0x0000FFFFL, m_lInSampleErrorLeft);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleaveInt16(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;

  while(count--)
  {
    *outBuffer++ = *(short *)pInBuffer;
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleaveInt24(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;
  
  while(count--)
  {
    *outBuffer++ = SampleInt32ToInt16(ReadInt24(pInBuffer), 0x0000FFFFL, m_lInSampleErrorLeft);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleaveInt32(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;
  
  while(count--)
  {
    *outBuffer++ = SampleInt32ToInt16(*(long *)pInBuffer, 0x0000FFFFL, m_lInSampleErrorLeft);
    pInBuffer += m_nInFrameSize;
  }
}


void CSoundTouchEx::MonoInterleaveFloat(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;

  while(count--)
  {
    *(double *)pOutBuffer = SampleInt16ToFloat(*inBuffer++);
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoInterleaveInt16(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(short *)pOutBuffer = SampleInt16ToInt16(*inBuffer++, bitMask, m_lOutSampleErrorLeft);
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoInterleaveInt24(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    WriteInt24(pOutBuffer, SampleInt16ToInt32(*inBuffer++, bitMask));
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoInterleaveInt32(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(long *)pOutBuffer = SampleInt16ToInt32(*inBuffer++, bitMask);
    pOutBuffer += m_nOutFrameSize;
  }
}

#else

void CSoundTouchEx::StereoDeInterleaveFloat(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;

  while(count--)
  {
    *outBuffer++ = *(double *)(pInBuffer + m_nInLeftOffset);
    *outBuffer++ = *(double *)(pInBuffer + m_nInRightOffset);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoDeInterleaveInt16(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;

  while(count--)
  {
    *outBuffer++ = SampleInt16ToFloat(*(short *)(pInBuffer + m_nInLeftOffset));
    *outBuffer++ = SampleInt16ToFloat(*(short *)(pInBuffer + m_nInRightOffset));
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoDeInterleaveInt24(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;

  while(count--)
  {
    *outBuffer++ = SampleInt32ToFloat(ReadInt24(pInBuffer + m_nInLeftOffset));
    *outBuffer++ = SampleInt32ToFloat(ReadInt24(pInBuffer + m_nInRightOffset));
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoDeInterleaveInt32(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;

  while(count--)
  {
    *outBuffer++ = SampleInt32ToFloat(*(long *)(pInBuffer + m_nInLeftOffset));
    *outBuffer++ = SampleInt32ToFloat(*(long *)(pInBuffer + m_nInRightOffset));
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoInterleaveFloat(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;

  while(count--)
  {
    *(double *)(pOutBuffer + m_nOutLeftOffset) = *inBuffer++;
    *(double *)(pOutBuffer + m_nOutRightOffset) = *inBuffer++;
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::StereoInterleaveInt16(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(short *)(pOutBuffer + m_nOutLeftOffset) = SampleFloatToInt16(*inBuffer++, bitMask, m_lOutSampleErrorLeft);
    *(short *)(pOutBuffer + m_nOutRightOffset) = SampleFloatToInt16(*inBuffer++, bitMask, m_lOutSampleErrorRight);
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::StereoInterleaveInt24(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    WriteInt24(pOutBuffer + m_nOutLeftOffset, SampleFloatToInt32(*inBuffer++, bitMask, m_lOutSampleErrorLeft));
    WriteInt24(pOutBuffer + m_nOutRightOffset, SampleFloatToInt32(*inBuffer++, bitMask, m_lOutSampleErrorRight));
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::StereoInterleaveInt32(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(long *)(pOutBuffer + m_nOutLeftOffset) = SampleFloatToInt32(*inBuffer++, bitMask, m_lOutSampleErrorLeft);
    *(long *)(pOutBuffer + m_nOutRightOffset) = SampleFloatToInt32(*inBuffer++, bitMask, m_lOutSampleErrorRight);
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleaveFloat(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;

  while(count--)
  {
    *outBuffer++ = *(double *)pInBuffer;
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleaveInt16(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;

  while(count--)
  {
    *outBuffer++ = SampleInt16ToFloat(*(short *)pInBuffer);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleaveInt24(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;

  while(count--)
  {
    *outBuffer++ = SampleInt32ToFloat(ReadInt24(pInBuffer));
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleaveInt32(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;

  while(count--)
  {
    *outBuffer++ = SampleInt32ToFloat(*(long *)pInBuffer);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoInterleaveFloat(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;

  while(count--)
  {
    *(double *)pOutBuffer = *inBuffer++;
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoInterleaveInt16(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(short *)pOutBuffer = SampleFloatToInt16(*inBuffer++, bitMask, m_lOutSampleErrorLeft);
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoInterleaveInt24(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    WriteInt24(pOutBuffer, SampleFloatToInt32(*inBuffer++, bitMask, m_lOutSampleErrorLeft));
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoInterleaveInt32(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;
  long bitMask = (long)(0xFFFFFFFFUL >> m_nOutBitsPerSample);

  while(count--)
  {
    *(long *)pOutBuffer = SampleFloatToInt32(*inBuffer++, bitMask, m_lOutSampleErrorLeft);
    pOutBuffer += m_nOutFrameSize;
  }
}

#endif
