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
#include "SoundTouchEx.h"

#include "alloctracing.h"

// conversion functions
#define NOISE_COEFF   0.7f
//#define SAMPLE_FLOAT  float


CSoundTouchEx::CSoundTouchEx()
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
}

CSoundTouchEx::~CSoundTouchEx() 
{
}

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
