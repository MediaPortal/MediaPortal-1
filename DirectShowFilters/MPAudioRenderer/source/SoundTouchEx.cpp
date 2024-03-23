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

  m_nOutFrameSize = 4;
  m_nOutBytesPerSample = 2;

  m_tempBuffer = NULL;
}

CSoundTouchEx::~CSoundTouchEx()
{
   SAFE_DELETE_ARRAY(m_tempBuffer);
}

// Queue Handling
void CSoundTouchEx::putBuffer(const BYTE *pInBuffer, int numSamples)
{
  int inSamplesRemaining = numSamples;

  // input samples
  while(inSamplesRemaining > 0)
  {
    int batchLen = (inSamplesRemaining < BATCH_LEN? inSamplesRemaining : BATCH_LEN);
    deInterleave(pInBuffer, m_tempBuffer, batchLen);
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

    interleave(m_tempBuffer, pOutBuffer, batchLen);

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

bool CSoundTouchEx::SetOutputFormat(int frameSize, int bytesPerSample)
{
  m_nOutFrameSize = frameSize;
  m_nOutBytesPerSample = bytesPerSample;

  return true;
}

bool CSoundTouchEx::SetChannels(uint numChannels)
{
  SAFE_DELETE_ARRAY(m_tempBuffer);
  m_tempBuffer = new soundtouch::SAMPLETYPE[BATCH_LEN * numChannels];
  setChannels(numChannels);
  m_nSampleSize = sizeof(soundtouch::SAMPLETYPE) * numChannels;

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

void CSoundTouchEx::deInterleave(const void* inBuffer, soundtouch::SAMPLETYPE* outBuffer, uint count)
{
    BYTE* pInBuffer = ((BYTE*)inBuffer);
    int iPadding = m_nInFrameSize - m_nSampleSize;

    if (iPadding == 0)
        memcpy(outBuffer, pInBuffer, m_nSampleSize * count);
    else
    {
        while (count--)
        {
            //int i = channels;
            //while (i-- > 0)
            //{
            //    *outBuffer++ = *(soundtouch::SAMPLETYPE*)(pInBuffer);
            //    pInBuffer += sizeof(soundtouch::SAMPLETYPE);
            //}
            //pInBuffer += iPadding;
            memcpy(outBuffer, pInBuffer, m_nSampleSize);
            outBuffer += channels;
            pInBuffer += m_nInFrameSize;
        }
    }
}

void CSoundTouchEx::interleave(const soundtouch::SAMPLETYPE* inBuffer, void* outBuffer, uint count)
{
    BYTE* pOutBuffer = ((BYTE*)outBuffer);
    int iPadding = m_nOutFrameSize - m_nSampleSize;

    if (iPadding == 0)
        memcpy(outBuffer, inBuffer, m_nSampleSize * count);
    else
    {
        while (count--)
        {
            //int i = channels;
            //while (i-- > 0)
            //{
            //    *(soundtouch::SAMPLETYPE*)(pOutBuffer) = *inBuffer++;
            //    pOutBuffer += sizeof(soundtouch::SAMPLETYPE);
            //}
            //pOutBuffer += iPadding;
            memcpy(pOutBuffer, inBuffer, m_nSampleSize);
            inBuffer += channels;
            pOutBuffer += m_nInFrameSize;
        }
    }
}

#endif
