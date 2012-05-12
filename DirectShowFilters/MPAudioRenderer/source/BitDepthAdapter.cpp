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
#include "Globals.h"
#include "BitDepthAdapter.h"

#include "alloctracing.h"

// integer types
typedef signed char      int8_t;
typedef signed short     int16_t;
typedef signed int       int32_t;
typedef unsigned char    uint8_t;
typedef unsigned short   uint16_t;
typedef unsigned int     uint32_t;


#pragma pack(push, 1)
struct int24_t 
{
  WORD wLSW;
  BYTE bMSB;
  __inline int24_t(long value)
  {
    wLSW = ((DWORD)value) >> 8;
    bMSB = ((DWORD)value) >> 24;
  };
  __inline operator long()
  {
    return (((long)bMSB) << 24) | (((long)wLSW) << 8);
  };
};
#pragma pack(pop)


struct BitDepthDescriptor 
{
  BYTE wContainerBits;
  BYTE wValidBits;
  bool bIsFloat;

  bool operator==(const BitDepthDescriptor &arg)
  {
    return wContainerBits == arg.wContainerBits
        && wValidBits == arg.wValidBits
        && bIsFloat == arg.bIsFloat;
  };
};

struct BitDepthConversionDescriptor 
{
  BitDepthDescriptor Source;
  BitDepthDescriptor *Targets;
};

#define NULL_BITDEPTH_DESC  {0, 0, false}

static BitDepthDescriptor gConversionsFromInt16[]     = {{16, 16, false}, {32, 24, false}, {32, 32, false}, {24, 24, false}, {32, 32, true}, NULL_BITDEPTH_DESC};
static BitDepthDescriptor gConversionsFromInt24[]     = {{24, 24, false}, {32, 24, false}, {32, 32, false}, {32, 32, true}, {16, 16, false}, NULL_BITDEPTH_DESC};
static BitDepthDescriptor gConversionsFromInt32_24[]  = {{32, 32, false}, {24, 24, false}, {32, 32, true}, {16, 16, false}, NULL_BITDEPTH_DESC};
static BitDepthDescriptor gConversionsFromInt32[]     = {{32, 32, false}, {32, 32, true}, {32, 24, false}, {24, 24, false}, {16, 16, false}, NULL_BITDEPTH_DESC};
static BitDepthDescriptor gConversionsFromFloat[]     = {{32, 32, false}, {32, 24, false}, {24, 24, false}, {16, 16, false}, NULL_BITDEPTH_DESC};

static BitDepthConversionDescriptor gValidConversions[] = 
{
  {{16, 0, false}, gConversionsFromInt16},
  {{24, 0, false}, gConversionsFromInt24},
  {{32, 24, false}, gConversionsFromInt32_24},
  {{32, 0, false}, gConversionsFromInt32},
  {{32, 32, true}, gConversionsFromFloat},
  {NULL_BITDEPTH_DESC, NULL}
};

BitDepthDescriptor* FindConversion(const BitDepthDescriptor& source);


CBitDepthAdapter::CBitDepthAdapter() : 
  CBaseAudioSink(true),  
  m_bPassThrough(false),
  m_rtInSampleTime(0),
  m_rtNextIncomingSampleTime(0),
  m_pfnConvert(NULL)
{
  ResetDithering();
}

CBitDepthAdapter::~CBitDepthAdapter()
{
}

HRESULT CBitDepthAdapter::Init()
{
  HRESULT hr = InitAllocator();
  if (FAILED(hr))
    return hr;
  return CBaseAudioSink::Init();
}

HRESULT CBitDepthAdapter::Cleanup()
{
  return CBaseAudioSink::Cleanup();
}

HRESULT CBitDepthAdapter::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

  if (FormatsEqual(pwfx, m_pInputFormat))
  {
    *pChOrder = m_chOrder;
    return S_OK;
  }

  if (!m_pNextSink)
    return VFW_E_TYPE_NOT_ACCEPTED;

  bool bApplyChanges = (nApplyChangesDepth != 0);
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  // Try passthrough
  HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
  if (SUCCEEDED(hr))
  {
    if (bApplyChanges)
    {
      m_bPassThrough = true;
      SetInputFormat(pwfx);
      SetOutputFormat(pwfx);
    }

    m_chOrder = *pChOrder;
    return hr;
  }

  // Verify input format is PCM or IEEE_FLOAT
  bool isPCM = pwfx->SubFormat == KSDATAFORMAT_SUBTYPE_PCM;
  bool isFloat = pwfx->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;

  if (!isPCM && !isFloat)
    return VFW_E_TYPE_NOT_ACCEPTED;

  // Finally try alternate bit depths
  WAVEFORMATEXTENSIBLE* pOutWfx = NULL;
  CopyWaveFormatEx(&pOutWfx, pwfx);

  BitDepthDescriptor inBitDepth = {pwfx->Format.wBitsPerSample, pwfx->Format.wBitsPerSample, isFloat};
  inBitDepth.wValidBits = pwfx->Samples.wValidBitsPerSample;

  BitDepthDescriptor* pOutBitDepth = FindConversion(inBitDepth);

  if (pOutBitDepth == NULL)
  {
    SAFE_DELETE_WAVEFORMATEX(pOutWfx);
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  hr = VFW_E_TYPE_NOT_ACCEPTED;
  for(; FAILED(hr) && pOutBitDepth->wContainerBits != 0; pOutBitDepth++)
  {
    if (*pOutBitDepth == inBitDepth)
      continue; // skip if same as source

    pOutWfx->Format.wBitsPerSample = pOutBitDepth->wContainerBits;
    pOutWfx->SubFormat = pOutBitDepth->bIsFloat? KSDATAFORMAT_SUBTYPE_IEEE_FLOAT : KSDATAFORMAT_SUBTYPE_PCM;
    pOutWfx->Samples.wValidBitsPerSample = pOutBitDepth->wValidBits;
    pOutWfx->Format.nBlockAlign = pOutWfx->Format.wBitsPerSample/8 * pOutWfx->Format.nChannels;
    pOutWfx->Format.nAvgBytesPerSec = pOutWfx->Format.nBlockAlign * pOutWfx->Format.nSamplesPerSec;
  
    hr = m_pNextSink->NegotiateFormat(pOutWfx, nApplyChangesDepth, pChOrder);
  }

  if (FAILED(hr))
  {
    SAFE_DELETE_WAVEFORMATEX(pOutWfx);
    return hr;
  }
  if (bApplyChanges)
  {
    LogWaveFormat(pwfx, "BDA  - applying ");

    m_bPassThrough = false;
    SetInputFormat(pwfx);
    SetOutputFormat(pOutWfx, true);
    hr = SetupConversion();
    // TODO: do something meaningfull if SetupConversion fails
    if (FAILED(hr))
      m_pfnConvert = NULL;
  }
  else
  {
    LogWaveFormat(pwfx, "BDA  -          ");
    SAFE_DELETE_WAVEFORMATEX(pOutWfx);
  }

  m_chOrder = *pChOrder;

  return S_OK;
}

// Processing
HRESULT CBitDepthAdapter::PutSample(IMediaSample *pSample)
{
  if (!pSample)
    return S_OK;

  AM_MEDIA_TYPE *pmt = NULL;
  bool bFormatChanged = false;
  
  HRESULT hr = S_OK;

  if (SUCCEEDED(pSample->GetMediaType(&pmt)) && pmt != NULL)
    bFormatChanged = !FormatsEqual((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, m_pInputFormat);

  if (pSample->IsDiscontinuity() == S_OK)
    m_bDiscontinuity = true;

  CAutoLock lock (&m_csOutputSample);
  if (m_bFlushing)
    return S_OK;

  if (bFormatChanged)
  {
    // Process any remaining input
    if (!m_bPassThrough)
      hr = ProcessData(NULL, 0, NULL);

    // Apply format change locally, 
    // next filter will evaluate the format change when it receives the sample
    Log("CBitDepthAdapter::PutSample: Processing format change");
    ChannelOrder chOrder;
    hr = NegotiateFormat((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, 1, &chOrder);
    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("BitDepthAdapter: PutSample failed to change format: 0x%08x", hr);
      return hr;
    }
    m_chOrder = chOrder;
  }

  if (pmt)
    DeleteMediaType(pmt);

  if (m_bPassThrough)
  {
    if (m_pNextSink)
      return m_pNextSink->PutSample(pSample);
    return S_OK; // perhaps we should return S_FALSE to indicate sample was dropped
  }

  long nOffset = 0;
  long cbSampleData = pSample->GetActualDataLength();
  BYTE *pData = NULL;
  REFERENCE_TIME rtStop = 0;
  REFERENCE_TIME rtStart = 0;
  pSample->GetTime(&rtStart, &rtStop);

  // Detect discontinuity in stream timeline
  if ((abs(m_rtNextIncomingSampleTime - rtStart) > MAX_SAMPLE_TIME_ERROR) && m_nSampleNum != 0)
  {
    Log("CBitDepthAdapter - stream discontinuity: %6.3f", (rtStart - m_rtNextIncomingSampleTime) / 10000000.0);

    m_rtInSampleTime = rtStart;

    if (m_nSampleNum > 0)
    {
      Log("CBitDepthAdapter - using buffered sample data");
      ProcessData(NULL, 0, NULL);
    }
    else
      Log("CBitDepthAdapter - discarding buffered sample data");
  }

  if (m_nSampleNum == 0)
    m_rtInSampleTime = rtStart;

  UINT nFrames = cbSampleData / m_pInputFormat->Format.nBlockAlign;
  REFERENCE_TIME duration = nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

  m_rtNextIncomingSampleTime = rtStart + duration;
  m_nSampleNum++;

  hr = pSample->GetPointer(&pData);
  ASSERT(pData);
  if (FAILED(hr))
  {
    Log("CBitDepthAdapter::PutSample - failed to get sample's data pointer: 0x%08x", hr);
    return hr;
  }

  while (nOffset < cbSampleData && SUCCEEDED(hr))
  {
    long cbProcessed = 0;
    hr = ProcessData(pData + nOffset, cbSampleData - nOffset, &cbProcessed);
    nOffset += cbProcessed;
  }
  return hr;
}

HRESULT CBitDepthAdapter::EndOfStream()
{
  if (!m_bPassThrough)
    ProcessData(NULL, 0, NULL);
  return CBaseAudioSink::EndOfStream();  
}

void CBitDepthAdapter::ResetDithering()
{
  memset(m_lInSampleError, 0, sizeof(m_lInSampleError));
}

HRESULT CBitDepthAdapter::SetupConversion()
{
  Log("CBitDepthAdapter::SetupConversion");
  LogWaveFormat(m_pInputFormat, "Input format    ");
  LogWaveFormat(m_pOutputFormat, "Output format   ");
  m_bInFloatSamples = IS_WAVEFORMAT_FLOAT(m_pInputFormat);
  m_nInFrameSize = m_pInputFormat->Format.nBlockAlign;
  m_nInBytesPerSample = m_pInputFormat->Format.wBitsPerSample / 8;
  m_nInBitsPerSample = m_pInputFormat->Samples.wValidBitsPerSample;

  m_bOutFloatSamples = IS_WAVEFORMAT_FLOAT(m_pOutputFormat);
  m_nOutFrameSize = m_pOutputFormat->Format.nBlockAlign;
  m_nOutBytesPerSample = m_pOutputFormat->Format.wBitsPerSample / 8;
  m_nOutBitsPerSample = m_pOutputFormat->Samples.wValidBitsPerSample;

  int srcFuncIdx = m_nInBytesPerSample - 2;
  int dstFuncIdx = m_nOutBytesPerSample - 2;

  if (m_bInFloatSamples)
  {
    if (m_nInBytesPerSample != 4)
      return E_FAIL;
    srcFuncIdx = 3;
  }

  if (m_bOutFloatSamples)
  {
    if (m_nOutBytesPerSample != 4)
      return E_FAIL;
    dstFuncIdx = 3;
  }

  if (srcFuncIdx < 0 || srcFuncIdx > 3 || dstFuncIdx < 0 || dstFuncIdx > 3)
    return E_FAIL;

  m_pfnConvert = gConversions[srcFuncIdx][dstFuncIdx];
  return S_OK;
}

HRESULT CBitDepthAdapter::ProcessData(const BYTE* pData, long cbData, long* pcbDataProcessed)
{
  if (!m_pfnConvert)
    return E_POINTER;

  HRESULT hr = S_OK;

  CAutoLock lock (&m_csOutputSample);

  if (!pData) // need to flush any existing data
  {
    if (m_pNextOutSample)
    {
      long nOffset = m_pNextOutSample->GetActualDataLength();
      hr = OutputNextSample();

      UINT nFrames = nOffset / m_pOutputFormat->Format.nBlockAlign;
      m_rtInSampleTime += nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

      if (FAILED(hr))
      {
        Log("CBitDepthAdapter::ProcessData OutputNextSample failed with: 0x%08x", hr);
        return hr;
      }
    }

    if (pcbDataProcessed)
      *pcbDataProcessed = 0;
    
    return hr;
  }

  long bytesOutput = 0;

  while (cbData)
  {
    if (m_pNextOutSample)
    {
      // If there is not enough space in output sample, flush it
      long nOffset = m_pNextOutSample->GetActualDataLength();
      long nSize = m_pNextOutSample->GetSize();

      if (nOffset + m_nOutFrameSize > nSize)
      {
        hr = OutputNextSample();

        UINT nFrames = nOffset / m_pOutputFormat->Format.nBlockAlign;
        m_rtInSampleTime += nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

        if (FAILED(hr))
        {
          Log("CBitDepthAdapter::ProcessData OutputNextSample failed with: 0x%08x", hr);
          return hr;
        }
      }
    }

    // try to get an output buffer if none available
    if (!m_pNextOutSample && FAILED(hr = RequestNextOutBuffer(m_rtInSampleTime)))
    {
     if (pcbDataProcessed)
        *pcbDataProcessed = bytesOutput + cbData; // we can't realy process the data, lie about it!
      return hr;
    }

    long nOffset = m_pNextOutSample->GetActualDataLength();
    long nSize = m_pNextOutSample->GetSize();
    BYTE *pOutData = NULL;

    if (FAILED(hr = m_pNextOutSample->GetPointer(&pOutData)))
    {
      Log("BiDepthAdapter: Failed to get output buffer pointer: 0x%08x", hr);
      return hr;
    }
    ASSERT(pOutData);
    pOutData += nOffset;

    int framesToConvert = min(cbData / m_nInFrameSize, (nSize - nOffset) / m_nOutFrameSize);

    hr = (this->*m_pfnConvert)(pOutData, pData, framesToConvert);
    if (FAILED(hr))
    {
      Log("CBitDepthAdapter::ProcessData m_pfnConvert failed with: 0x%08x", hr);
      return hr;
    }

    pData += framesToConvert * m_nInFrameSize;
    bytesOutput += framesToConvert * m_nInFrameSize;
    cbData -= framesToConvert * m_nInFrameSize; 
    nOffset += framesToConvert * m_nOutFrameSize;
    m_pNextOutSample->SetActualDataLength(nOffset);

    if (nOffset + m_nOutFrameSize > nSize)
    {
      hr = OutputNextSample();
      
      UINT nFrames = nOffset / m_pOutputFormat->Format.nBlockAlign;
      m_rtInSampleTime += nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

      if (FAILED(hr))
      {
        Log("CBitDepthAdapter::ProcessData OutputNextSample failed with: 0x%08x", hr);
        return hr;
      }
    }

    // all samples should contain an integral number of frames
    ASSERT(cbData == 0 || cbData >= m_nInFrameSize);
  }
  
  if (pcbDataProcessed)
    *pcbDataProcessed = bytesOutput;

  return hr;
}

// conversion functions
#define NOISE_COEFF   0.7f
#define SAMPLE_FLOAT  double

// TODO: use a better noise function
__inline SAMPLE_FLOAT WhiteNoise()
{
  return ((SAMPLE_FLOAT)rand())/(RAND_MAX/2) - 1;
}

template<class Td, class Ts> __inline Td ConvertSample(const Ts src, long bitMask, register long& error)
{
  return (Td)src;
}

template<> __inline float ConvertSample<float, int16_t>(int16_t src, long bitMask, register long& error)      { return ((float)src)/SHRT_MAX; }
template<> __inline int32_t ConvertSample<int32_t, int16_t>(int16_t src, long bitMask, register long& error)  { return ((int32_t)src)<<16; }
template<> __inline int24_t ConvertSample<int24_t, int16_t>(int16_t src, long bitMask, register long& error)  { return ((int32_t)src)<<16; }
template<> __inline int16_t ConvertSample<int16_t, int16_t>(int16_t src, long bitMask, register long& error)  { return src; }

template<> __inline float ConvertSample<float, int24_t>(int24_t src, long bitMask, register long& error)      { return ((float)(int32_t)src)/LONG_MAX; }
template<> __inline int32_t ConvertSample<int32_t, int24_t>(int24_t src, long bitMask, register long& error)  { return (int32_t)src; }
template<> __inline int24_t ConvertSample<int24_t, int24_t>(int24_t src, long bitMask, register long& error)  { return src; }
//template<> __inline int16_t ConvertSample<int16_t, int24_t>(int24_t src, long bitMask, register long& error)  { return ConvertSample<int16_t, int32_t>(src, bitMask, error); }

template<> __inline float ConvertSample<float, int32_t>(int32_t src, long bitMask, register long& error)      { return ((float)src)/LONG_MAX; }
template<> __inline int32_t ConvertSample<int32_t, int32_t>(int32_t src, long bitMask, register long& error)  { return src; }
template<> __inline int24_t ConvertSample<int24_t, int32_t>(int32_t src, long bitMask, register long& error)
{ 
#ifdef DITHER_SAMPLES
  src += WhiteNoise() * NOISE_COEFF * bitMask + error;
  error = sample & bitMask;
#endif
  return (src & ~bitMask);
}

template<> __inline int16_t ConvertSample<int16_t, int32_t>(int32_t src, long bitMask, register long& error)
{ 
#ifdef DITHER_SAMPLES
  src += WhiteNoise() * NOISE_COEFF * bitMask + error;
  error = sample & bitMask;
#endif
  return (src & ~bitMask) >> 16;
}

template<> __inline float ConvertSample<float, float>(float src, long bitMask, register long& error)      { return src; }
template<> __inline int32_t ConvertSample<int32_t, float>(float src, long bitMask, register long& error)
{
  if (src > 1.0)
    return LONG_MAX;
  if (src < -1.0)
    return -LONG_MAX;

  src *= LONG_MAX;
  return (((int32_t)src) & ~bitMask);
}

template<> __inline int24_t ConvertSample<int24_t, float>(float src, long bitMask, register long& error)  { return ConvertSample<int32_t, float>(src, bitMask, error); }
template<> __inline int16_t ConvertSample<int16_t, float>(float src, long bitMask, register long& error)
{
  if (src > 1.0)
    return SHRT_MAX;
  if (src < -1.0)
    return -SHRT_MAX;

  src *= LONG_MAX;
#ifdef DITHER_SAMPLES
  src += WhiteNoise() * NOISE_COEFF * bitMask + error;
  error = ((long)src) & bitMask;
#endif
  return (((long)src) & ~bitMask) >> 16;
}

// delayed specialization
template<> __inline int16_t ConvertSample<int16_t, int24_t>(int24_t src, long bitMask, register long& error)  { return ConvertSample<int16_t, int32_t>(src, bitMask, error); }

template<class Td, class Ts> HRESULT CBitDepthAdapter::ConvertBuffer(BYTE *dst, const BYTE *src, long count)
{
  long bitMask = (0xffffffffu >> m_nOutBitsPerSample);
  int nChannels = m_pInputFormat->Format.nChannels;
  int i;
  while(count--)
  {
    const Ts* pSrc = (const Ts *)src;
    Td* pDst = (Td *)dst;
    for(i = 0; i < nChannels; i++)
      *(pDst++) = ConvertSample<Td, Ts>((*pSrc++), bitMask, m_lInSampleError[i]);
    src += m_nInFrameSize;
    dst += m_nOutFrameSize;
  }
  return S_OK;
}

template<class Td, class Ts, int nChannels> HRESULT CBitDepthAdapter::ConvertBuffer(BYTE *dst, const BYTE *src, long count)
{
  long bitMask = (0xffffffffu >> m_nOutBitsPerSample);
  while(count--)
  {
    const Ts* pSrc = (const Ts *)src;
    Td* pDst = (Td *)dst;
    for (i = 0; i < nChannels; i++)
      pDst[i] = ConvertSample<Td, Ts>(pSrc[i], bitMask, m_lInSampleError[i]);
    src += m_nInFrameSize;
    dst += m_nOutFrameSize;
  }
  return S_OK;
}

CBitDepthAdapter::BitDepthConversionFunc CBitDepthAdapter::gConversions[4][4] = {
  {
    &CBitDepthAdapter::ConvertBuffer<int16_t, int16_t>,
    &CBitDepthAdapter::ConvertBuffer<int24_t, int16_t>,
    &CBitDepthAdapter::ConvertBuffer<int32_t, int16_t>,
    &CBitDepthAdapter::ConvertBuffer<float, int16_t>,
  },
  {
    &CBitDepthAdapter::ConvertBuffer<int16_t, int24_t>,
    &CBitDepthAdapter::ConvertBuffer<int24_t, int24_t>,
    &CBitDepthAdapter::ConvertBuffer<int32_t, int24_t>,
    &CBitDepthAdapter::ConvertBuffer<float, int24_t>,
  },
  {
    &CBitDepthAdapter::ConvertBuffer<int16_t, int32_t>,
    &CBitDepthAdapter::ConvertBuffer<int24_t, int32_t>,
    &CBitDepthAdapter::ConvertBuffer<int32_t, int32_t>,
    &CBitDepthAdapter::ConvertBuffer<float, int32_t>,
  },
  {
    &CBitDepthAdapter::ConvertBuffer<int16_t, float>,
    &CBitDepthAdapter::ConvertBuffer<int24_t, float>,
    &CBitDepthAdapter::ConvertBuffer<int32_t, float>,
    &CBitDepthAdapter::ConvertBuffer<float, float>,
  }
};

BitDepthDescriptor* FindConversion(const BitDepthDescriptor& source)
{
  BitDepthConversionDescriptor* pConv = gValidConversions;

  while(pConv->Source.wContainerBits != 0)
  {
    if (source.bIsFloat == pConv->Source.bIsFloat &&
        source.wContainerBits == pConv->Source.wContainerBits &&
        (source.wValidBits == pConv->Source.wValidBits || pConv->Source.wValidBits == 0))
      return pConv->Targets;
    pConv++;
  }
  return NULL;
}

