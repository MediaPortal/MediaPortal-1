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
#include "SampleRateConverterFilter.h"

#include "alloctracing.h"

extern HRESULT CopyWaveFormatEx(WAVEFORMATEX **dst, const WAVEFORMATEX *src);
extern void Log(const char *fmt, ...);

CSampleRateConverter::CSampleRateConverter(AudioRendererSettings *pSettings)
: m_bPassThrough(false),
  m_rtInSampleTime(0),
  m_pSettings(pSettings)
{
}

CSampleRateConverter::~CSampleRateConverter(void)
{
}

HRESULT CSampleRateConverter::Init()
{
  HRESULT hr = InitAllocator();
  if (FAILED(hr))
    return hr;

  return CBaseAudioSink::Init();
}

HRESULT CSampleRateConverter::Cleanup()
{
  m_pMemAllocator.Release();
  return CBaseAudioSink::Cleanup();
}

HRESULT CSampleRateConverter::NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

  if (FormatsEqual(pwfx, m_pInputFormat))
    return S_OK;

  if (!m_pNextSink)
    return VFW_E_TYPE_NOT_ACCEPTED;

  bool bApplyChanges = (nApplyChangesDepth != 0);
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  // try passthrough
  HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth);
  if (SUCCEEDED(hr))
  {
    if (bApplyChanges)
    {
      //m_bPassThrough = true;
      SetInputFormat(pwfx);
      SetOutputFormat(pwfx);
    }
    SetupConversion(); // testing only
    return hr;
  }

  // verify input format is PCM or IEEE_FLOAT
  bool isPCM = false;
  bool isFloat = false;
  bool isWFExtensible = pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE && 
                        pwfx->cbSize >= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX);

  if (isWFExtensible)
  {
    isPCM = (((WAVEFORMATEXTENSIBLE *)pwfx)->SubFormat == KSDATAFORMAT_SUBTYPE_PCM);
    isFloat = (((WAVEFORMATEXTENSIBLE *)pwfx)->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT);
  }
  else 
  {
    isPCM = (pwfx->wFormatTag == WAVE_FORMAT_PCM);
    isFloat = (pwfx->wFormatTag == WAVE_FORMAT_IEEE_FLOAT);
  }

  if (!isPCM && !isFloat)
    return VFW_E_TYPE_NOT_ACCEPTED;

  WAVEFORMATEX *pOutWfx;
  CopyWaveFormatEx(&pOutWfx, pwfx);

  hr = VFW_E_TYPE_NOT_ACCEPTED;
  
  /*
  for(; FAILED(hr) && pOutBitDepth->wContainerBits != 0; pOutBitDepth++)
  {
    if (*pOutBitDepth == inBitDepth)
      continue; // skip if same as source

    pOutWfx->wBitsPerSample = pOutBitDepth->wContainerBits;
    if (isWFExtensible)
    {
      ((WAVEFORMATEXTENSIBLE *)pwfx)->SubFormat = pOutBitDepth->bIsFloat? KSDATAFORMAT_SUBTYPE_IEEE_FLOAT : KSDATAFORMAT_SUBTYPE_PCM;
      ((WAVEFORMATEXTENSIBLE *)pwfx)->Samples.wValidBitsPerSample = pOutBitDepth->wValidBits;
    }
    else
    {
      if (pOutBitDepth->wContainerBits != pOutBitDepth->wValidBits)
        continue; // WAVEFORMATEX cannot describe this format - skip it
      
      pOutWfx->wFormatTag = pOutBitDepth->bIsFloat? WAVE_FORMAT_IEEE_FLOAT : WAVE_FORMAT_PCM;
    }
    pOutWfx->nBlockAlign = pOutWfx->wBitsPerSample/8 * pOutWfx->nChannels;
    pOutWfx->nAvgBytesPerSec = pOutWfx->nBlockAlign * pOutWfx->nSamplesPerSec;
  
    hr = m_pNextSink->NegotiateFormat(pOutWfx, nApplyChangesDepth);
  }

  if (FAILED(hr))
  {
    SAFE_DELETE_WAVEFORMATEX(pOutWfx);
    return hr;
  }
  if (bApplyChanges)
  {
    m_bPassThrough = false;
    SetInputFormat(pwfx);
    SetOutputFormat(pOutWfx, true);
    hr = SetupConversion();
    // TODO: do something meaningfull if SetupConversion fails
    if (FAILED(hr))
      m_pfnConvert = NULL;
  }*/

  return S_OK;
}

// Processing
HRESULT CSampleRateConverter::PutSample(IMediaSample *pSample)
{
  if (!pSample)
    return S_OK;

  AM_MEDIA_TYPE *pmt = NULL;
  bool bFormatChanged = false;
  
  HRESULT hr = S_OK;

  if (SUCCEEDED(pSample->GetMediaType(&pmt)) && pmt != NULL)
    bFormatChanged = !FormatsEqual((WAVEFORMATEX*)pmt->pbFormat, m_pInputFormat);

  if (bFormatChanged)
  {
    // process any remaining input
    if (!m_bPassThrough)
      hr = ProcessData(NULL, 0, NULL);
    // Apply format change locally, 
    // next filter will evaluate the format change when it receives the sample
    hr = NegotiateFormat((WAVEFORMATEX*)pmt->pbFormat, 1);
    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("BitDepthAdapter: PutSample failed to change format: 0x%08x", hr);
      return hr;
    }
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
  REFERENCE_TIME rtStop;
  pSample->GetTime(&m_rtInSampleTime, &rtStop);

  hr = pSample->GetPointer(&pData);
  ASSERT(pData != NULL);

  while (nOffset < cbSampleData && SUCCEEDED(hr))
  {
    long cbProcessed = 0;
    hr = ProcessData(pData+nOffset, cbSampleData - nOffset, &cbProcessed);
    nOffset += cbProcessed;
  }
  return hr;
}

HRESULT CSampleRateConverter::EndOfStream()
{
  if (!m_bPassThrough)
    ProcessData(NULL, 0, NULL);
  return CBaseAudioSink::EndOfStream();  
}

HRESULT CSampleRateConverter::OnInitAllocatorProperties(ALLOCATOR_PROPERTIES *properties)
{
  return CBaseAudioSink::OnInitAllocatorProperties(properties); // for now use defaults
}

HRESULT CSampleRateConverter::SetupConversion()
{
  // Only floats
  m_nOutBitsPerSample = 16;//32;
  m_nInBitsPerSample = 16;//32;

  m_nInFrameSize = m_pInputFormat->nBlockAlign;
  m_nInBytesPerSample = m_pInputFormat->wBitsPerSample / 8;

  m_nOutFrameSize = m_pOutputFormat->nBlockAlign;
  m_nOutBytesPerSample = m_pOutputFormat->wBitsPerSample / 8;

  return S_OK;
}

HRESULT CSampleRateConverter::ProcessData(const BYTE *pData, long cbData, long *pcbDataProcessed)
{
  HRESULT hr = S_OK;

  if (!pData) // need to flush any existing data
  {
    if (m_pNextOutSample)
      hr = OutputNextSample();

    if (pcbDataProcessed)
      *pcbDataProcessed = 0;
    return hr;
  }

  long bytesOutput = 0;

  while(cbData)
  {
    if (m_pNextOutSample)
    {

      // if there is not enough space in output sample, flush it
      long nOffset = m_pNextOutSample->GetActualDataLength();
      long nSize = m_pNextOutSample->GetSize();

      if (nOffset + m_nOutFrameSize > nSize)
        hr = OutputNextSample();
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
      Log("CSampleRateConverter: Failed to get output buffer pointer: 0x%08x", hr);
      return hr;
    }
    ASSERT(pOutData);

    int framesToConvert = min(cbData / m_nInFrameSize, (nSize - nOffset) / m_nOutFrameSize);

    //hr = (this->*m_pfnConvert)(pOutData, pData, framesToConvert);
    memcpy(pOutData, pData, framesToConvert * m_nInFrameSize);

    pData += framesToConvert * m_nInFrameSize;
    bytesOutput += framesToConvert * m_nInFrameSize;
    cbData -= framesToConvert * m_nInFrameSize; 
    nOffset += framesToConvert * m_nOutFrameSize;
    m_pNextOutSample->SetActualDataLength(nOffset);
    if (nOffset + m_nOutFrameSize > nSize)
      OutputNextSample();

    m_rtInSampleTime += framesToConvert * m_nInFrameSize * UNITS / m_pInputFormat->nAvgBytesPerSec;

    // all samples should contain an integral number of frames
    ASSERT(cbData == 0 || cbData >= m_nInFrameSize);
  }
  
  if (pcbDataProcessed)
    *pcbDataProcessed = bytesOutput;
  return hr;
}


