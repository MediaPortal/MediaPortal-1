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

// conversion functions
#define NOISE_COEFF   0.7f
//#define SAMPLE_FLOAT  float


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
  m_nInLeftOffset = 0;
  m_nInRightOffset = 2;
  m_pfnDeInterleave = &CSoundTouchEx::StereoDeInterleave;

  m_nOutFrameSize = 4;
  m_nOutBytesPerSample = 2;
  m_nOutLeftOffset = 0;
  m_nOutRightOffset = 2;
  m_pfnInterleave = &CSoundTouchEx::StereoInterleave;

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

bool CSoundTouchEx::SetInputFormat(int frameSize, int bytesPerSample)
{
  m_nInFrameSize = frameSize;
  m_nInBytesPerSample = bytesPerSample;

  return true;
}

void CSoundTouchEx::SetInputChannels(int leftOffset, int rightOffset)
{
  m_nInLeftOffset = leftOffset;
  m_nInRightOffset = rightOffset;
}

bool CSoundTouchEx::SetOutputFormat(int frameSize, int bytesPerSample)
{
  m_nOutFrameSize = frameSize;
  m_nOutBytesPerSample = bytesPerSample;

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
  // select appropriate input de-interleaving method
  m_pfnDeInterleave = (channels == 1? &CSoundTouchEx::MonoDeInterleave : &CSoundTouchEx::StereoDeInterleave);

  // select appropriate output interleaving method
  m_pfnInterleave = (channels == 1? &CSoundTouchEx::MonoInterleave : &CSoundTouchEx::StereoInterleave);
  return true;
}

// Internal functions to separate/merge streams out of/into sample buffers (from IMediaSample)

#ifdef INTEGER_SAMPLES

void CSoundTouchEx::StereoDeInterleave(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;
  while(count--)
  {
    *outBuffer++ = *(short *)(pInBuffer + m_nInLeftOffset);
    *outBuffer++ = *(short *)(pInBuffer + m_nInRightOffset);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoInterleave(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;

  while(count--)
  {
    *(short *)(pOutBuffer + m_nOutLeftOffset) = *inBuffer++;
    *(short *)(pOutBuffer + m_nOutRightOffset) = *inBuffer++;
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleave(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;

  while(count--)
  {
    *outBuffer++ = *(short *)pInBuffer;
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoInterleave(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;

  while(count--)
  {
    *(short *)pOutBuffer = *inBuffer++;
    pOutBuffer += m_nOutFrameSize;
  }
}

#else

void CSoundTouchEx::StereoDeInterleave(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = (BYTE *)inBuffer;

  while(count--)
  {
    *outBuffer++ = *(float *)(pInBuffer + m_nInLeftOffset);
    *outBuffer++ = *(float *)(pInBuffer + m_nInRightOffset);
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::StereoInterleave(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = (BYTE *)outBuffer;

  while(count--)
  {
    *(float *)(pOutBuffer + m_nOutLeftOffset) = *inBuffer++;
    *(float *)(pOutBuffer + m_nOutRightOffset) = *inBuffer++;
    pOutBuffer += m_nOutFrameSize;
  }
}

void CSoundTouchEx::MonoDeInterleave(const void *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count)
{
  BYTE *pInBuffer = ((BYTE *)inBuffer) + m_nInLeftOffset;

  while(count--)
  {
    *outBuffer++ = *(float *)pInBuffer;
    pInBuffer += m_nInFrameSize;
  }
}

void CSoundTouchEx::MonoInterleave(const soundtouch::SAMPLETYPE *inBuffer, void *outBuffer, uint count)
{
  BYTE *pOutBuffer = ((BYTE *)outBuffer) + m_nOutLeftOffset;

  while(count--)
  {
    *(float *)pOutBuffer = *inBuffer++;
    pOutBuffer += m_nOutFrameSize;
  }
}

#endif
