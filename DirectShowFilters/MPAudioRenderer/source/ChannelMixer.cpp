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
#include "ChannelMixer.h"

#include "alloctracing.h"

CChannelMixer::CChannelMixer(AudioRendererSettings* pSettings) :
  CBaseAudioSink(true),
  m_bPassThrough(false),
  m_rtInSampleTime(0),
  m_pSettings(pSettings),
  m_rtNextIncomingSampleTime(0)
{
  m_pRemap = new CAERemap();
}

CChannelMixer::~CChannelMixer(void)
{
  delete m_pRemap;
}

HRESULT CChannelMixer::Init()
{
  HRESULT hr = InitAllocator();
  if (FAILED(hr))
    return hr;

  return CBaseAudioSink::Init();
}

HRESULT CChannelMixer::Cleanup()
{
  return CBaseAudioSink::Cleanup();
}

HRESULT CChannelMixer::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
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

  if (pwfx->SubFormat != KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
    return VFW_E_TYPE_NOT_ACCEPTED;

  HRESULT hr = S_OK;
  bool expandToStereo = pwfx->Format.nChannels == 1 && m_pSettings->m_bExpandMonoToStereo;

  if (!m_pSettings->m_bForceChannelMixing && !expandToStereo)
  {
    // try the format directly
    hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
    if (SUCCEEDED(hr))
    {
      if (bApplyChanges)
      {
        SetInputFormat(pwfx);
        SetOutputFormat(pwfx);
        m_bPassThrough = false;
        hr = SetupConversion(*pChOrder);
      }

      m_chOrder = *pChOrder;
      return hr;
    }
  }

  WAVEFORMATEXTENSIBLE* pOutWfx;
  CopyWaveFormatEx(&pOutWfx, pwfx);

  if (!expandToStereo || m_pSettings->m_bForceChannelMixing)
  {
    pOutWfx->dwChannelMask = m_pSettings->m_lSpeakerConfig;
    pOutWfx->Format.nChannels = m_pSettings->m_lSpeakerCount;
  }
  else // Expand mono to stereo
  {
    pOutWfx->dwChannelMask = KSAUDIO_SPEAKER_STEREO;
    pOutWfx->Format.nChannels = 2;
  }

  pOutWfx->SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;  
  pOutWfx->Format.nBlockAlign = pOutWfx->Format.wBitsPerSample / 8 * pOutWfx->Format.nChannels;
  pOutWfx->Format.nAvgBytesPerSec = pOutWfx->Format.nBlockAlign * pOutWfx->Format.nSamplesPerSec;
  
  hr = m_pNextSink->NegotiateFormat(pOutWfx, nApplyChangesDepth, pChOrder);
  m_chOrder = *pChOrder;


  if (FAILED(hr))
  {
    SAFE_DELETE_WAVEFORMATEX(pOutWfx);
    return hr;
  }

  if (bApplyChanges)
  {
    LogWaveFormat(pwfx, "MIX  - applying ");

    m_bPassThrough = false;
    SetInputFormat(pwfx);
    SetOutputFormat(pOutWfx, true);
    hr = SetupConversion(*pChOrder);
  }
  else
  {
    LogWaveFormat(pwfx, "MIX  -          ");
    SAFE_DELETE_WAVEFORMATEX(pOutWfx);
  }

  return hr;
}

// Processing
HRESULT CChannelMixer::PutSample(IMediaSample *pSample)
{
  if (!pSample)
    return S_OK;

  AM_MEDIA_TYPE *pmt = NULL;
  bool bFormatChanged = false;
  
  HRESULT hr = S_OK;

  if (SUCCEEDED(pSample->GetMediaType(&pmt)) && pmt)
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
    Log("CChannelMixer::PutSample: Processing format change");
    ChannelOrder chOrder;
    hr = NegotiateFormat((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, 1, &chOrder);
    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("CChannelMixer: PutSample failed to change format: 0x%08x", hr);
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
    Log("CChannelMixer - stream discontinuity: %6.3f", (rtStart - m_rtNextIncomingSampleTime) / 10000000.0);

    m_rtInSampleTime = rtStart;

    if (m_nSampleNum > 0)
    {
      Log("CChannelMixer - using buffered sample data");
      FlushStream();
    }
    else
      Log("CChannelMixer - discarding buffered sample data");
  }

  if (m_nSampleNum == 0)
    m_rtInSampleTime = rtStart;

  UINT nFrames = cbSampleData / m_pInputFormat->Format.nBlockAlign;
  REFERENCE_TIME duration = nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;

  m_rtNextIncomingSampleTime = rtStart + duration;
  m_nSampleNum++;

  hr = pSample->GetPointer(&pData);
  ASSERT(pData);
  if (FAILED(hr))
  {
    Log("CChannelMixer::PutSample - failed to get sample's data pointer: 0x%08x", hr);
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

HRESULT CChannelMixer::EndOfStream()
{
  if (!m_bPassThrough)
    FlushStream();

  return CBaseAudioSink::EndOfStream();  
}

HRESULT CChannelMixer::SetupConversion(ChannelOrder chOrder)
{
  m_nInFrameSize = m_pInputFormat->Format.nBlockAlign;
  m_nOutFrameSize = m_pOutputFormat->Format.nBlockAlign;

  MapChannelsFromDStoAE(m_pInputFormat, &m_AEInput);
  MapChannelsFromDStoAE(m_pOutputFormat, &m_AEOutput, chOrder == AC3_ORDER);

  // TODO check the last parameter
  if (!m_pRemap->Initialize(m_AEInput, m_AEOutput, false, false, AE_CH_LAYOUT_7_1)) 
  {
    Log("CChannelMixer::SetupConversion - failed to initialize channel remapper");
    ASSERT(false);
  }

  m_bPassThrough = m_AEInput == m_AEOutput;

  Log("CChannelMixer::SetupConversion");
  LogWaveFormat(m_pInputFormat, "Input format    ");
  LogWaveFormat(m_pOutputFormat, "Output format   ");

  return S_OK;
}

HRESULT CChannelMixer::MapChannelsFromDStoAE(WAVEFORMATEXTENSIBLE* pWfex, CAEChannelInfo* pChannelInfo, bool useAC3Layout)
{
  CheckPointer(pWfex, E_POINTER);

  if (useAC3Layout)
  {
    switch (pWfex->Format.nChannels)
    {
      case 1:
        *pChannelInfo = AE_AC3_CH_LAYOUT_1_0;
        break;
      case 2:
        *pChannelInfo = AE_AC3_CH_LAYOUT_2_0;
        break;
      case 3:
        if (pWfex->dwChannelMask & SPEAKER_BACK_CENTER)
          *pChannelInfo = AE_AC3_CH_LAYOUT_2_S;
        else
          *pChannelInfo = AE_AC3_CH_LAYOUT_3_0;
        break;
      case 4:
        if (pWfex->dwChannelMask & SPEAKER_BACK_CENTER)
          *pChannelInfo = AE_AC3_CH_LAYOUT_3_S;
        else
          *pChannelInfo = AE_AC3_CH_LAYOUT_4_0;
        break;
      case 5:
        *pChannelInfo = AE_AC3_CH_LAYOUT_5_0;
        break;
      case 6:
        *pChannelInfo = AE_AC3_CH_LAYOUT_5_1;
        break;
      default:
        return S_FALSE;
      }
  }
  else // non-AC3
  {
    switch (pWfex->Format.nChannels)
    {
      case 1:
        *pChannelInfo = AE_CH_LAYOUT_1_0;
        break;
      case 2:
        *pChannelInfo = AE_CH_LAYOUT_2_0;
        break;
      case 3:
        if (pWfex->dwChannelMask & SPEAKER_LOW_FREQUENCY)
          *pChannelInfo = AE_CH_LAYOUT_2_1;
        else
          *pChannelInfo = AE_CH_LAYOUT_3_0;
        break;
      case 4:
        if (pWfex->dwChannelMask & SPEAKER_LOW_FREQUENCY)
          *pChannelInfo = AE_CH_LAYOUT_3_1;
        else
          *pChannelInfo = AE_CH_LAYOUT_4_0;
        break;
      case 5:
        if (pWfex->dwChannelMask & SPEAKER_LOW_FREQUENCY)
          *pChannelInfo = AE_CH_LAYOUT_4_1;
        else
          *pChannelInfo = AE_CH_LAYOUT_5_0;
        break;
      case 6:
        *pChannelInfo = AE_CH_LAYOUT_5_1;
        break;
      case 7:
        if (pWfex->dwChannelMask & SPEAKER_LOW_FREQUENCY)
        {
          if (pWfex->dwChannelMask == 0x13f)
            *pChannelInfo = AE_CH_LAYOUT_6_1_0x13f;
          else
            *pChannelInfo = AE_CH_LAYOUT_6_1_0x70f;
        }
        else
          *pChannelInfo = AE_CH_LAYOUT_7_0;
        break;
      case 8:
        *pChannelInfo = AE_CH_LAYOUT_7_1;
        break;
      default:
        return S_FALSE;
      }  
  }

  return S_OK;
}

HRESULT CChannelMixer::ProcessData(const BYTE* pData, long cbData, long* pcbDataProcessed)
{
  HRESULT hr = S_OK;

  long bytesOutput = 0;

  CAutoLock lock (&m_csOutputSample);

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
          Log("CChannelMixer::ProcessData OutputNextSample failed with: 0x%08x", hr);
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
    BYTE* pOutData = NULL;

    if (FAILED(hr = m_pNextOutSample->GetPointer(&pOutData)))
    {
      Log("CChannelMixer: Failed to get output buffer pointer: 0x%08x", hr);
      return hr;
    }
    ASSERT(pOutData);
    pOutData += nOffset;

    int framesToConvert = min(cbData / m_nInFrameSize, (nSize - nOffset) / m_nOutFrameSize);

    m_pRemap->Remap((float*)pData, (float*)pOutData, framesToConvert);

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
        Log("CChannelMixer::ProcessData OutputNextSample failed with: 0x%08x", hr);
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

HRESULT CChannelMixer::FlushStream()
{
  HRESULT hr = S_OK;

  CAutoLock lock (&m_csOutputSample);
  if (m_pNextOutSample)
  {
    hr = OutputNextSample();
    if (FAILED(hr))
      Log("CChannelMixer::FlushStream OutputNextSample failed with: 0x%08x", hr);
  }

  return hr;
}


