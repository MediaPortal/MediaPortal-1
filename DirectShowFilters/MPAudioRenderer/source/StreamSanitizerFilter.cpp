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
#include "StreamSanitizerFilter.h"

#include "alloctracing.h"

CStreamSanitizer::CStreamSanitizer(AudioRendererSettings* pSettings) :
  CBaseAudioSink(true), 
  m_rtInSampleTime(0),
  m_pSettings(pSettings),
  m_rtNextIncomingSampleTime(0),
  m_rtUncorrectedError(0)
{
}

CStreamSanitizer::~CStreamSanitizer(void)
{
}

HRESULT CStreamSanitizer::Init()
{
  HRESULT hr = InitAllocator();
  if (FAILED(hr))
    return hr;

  return CBaseAudioSink::Init();
}

HRESULT CStreamSanitizer::Cleanup()
{
  return CBaseAudioSink::Cleanup();
}

HRESULT CStreamSanitizer::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
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

  HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
  if (SUCCEEDED(hr) && bApplyChanges)
  {
    SetInputFormat(pwfx);
    SetOutputFormat(pwfx);
  }

  m_chOrder = *pChOrder;

  return hr;
}

// Processing
HRESULT CStreamSanitizer::PutSample(IMediaSample *pSample)
{
  if (!pSample)
    return S_OK;

  AM_MEDIA_TYPE* pmt = NULL;
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
    Log("CStreamSanitizer::PutSample: Processing format change");
    ChannelOrder chOrder;
    hr = NegotiateFormat((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, 1, &chOrder);
    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("SampleRateConverter: PutSample failed to change format: 0x%08x", hr);
      return hr;
    }
    m_chOrder = chOrder;
  }

  bool bNextSampleTimeCalculated = false;
  BYTE *pData = NULL;
  REFERENCE_TIME rtStop = 0;
  REFERENCE_TIME rtStart = 0;
  hr = pSample->GetTime(&rtStart, &rtStop);

  //Log("SAN:  rtStart: %6.3f m_rtNextIncomingSampleTime: %6.3f", rtStart / 10000000.0, m_rtNextIncomingSampleTime / 10000000.0);

  if (SUCCEEDED(hr))
  {
    if (m_nSampleNum == 0)
      m_rtNextIncomingSampleTime = rtStart;

    // Detect discontinuity in stream timeline
    REFERENCE_TIME rtGap = rtStart - m_rtNextIncomingSampleTime;

    if (abs(rtGap) > MAX_SAMPLE_TIME_ERROR)
    {
      if (m_nSampleNum > 0)
      {
        Log("CStreamSanitizer: gap detected: %6.3f (%I64d), previous error: %I64d", rtGap / 10000000.0, rtGap, m_rtUncorrectedError);

        rtGap += m_rtUncorrectedError;

        UINT32 nGapFrames = abs(rtGap) / (UNITS / m_pInputFormat->Format.nSamplesPerSec);
        UINT32 gapInBytes = nGapFrames * m_pInputFormat->Format.nBlockAlign;

        m_rtUncorrectedError = abs(rtGap) - nGapFrames * (UNITS / m_pInputFormat->Format.nSamplesPerSec);
        if (rtGap < 0) 
          m_rtUncorrectedError *= -1;

        if (rtGap > 0)
        {
          m_rtInSampleTime = m_rtNextIncomingSampleTime;

          while(SUCCEEDED(hr) && gapInBytes > 0)
          {
            hr = RequestNextOutBuffer(m_rtInSampleTime);
            if (FAILED(hr))
              break;

            UINT32 bytesSilence = min(gapInBytes, m_pNextOutSample->GetSize());
            bytesSilence -= bytesSilence % m_pInputFormat->Format.nBlockAlign;

            BYTE* pSampleData = NULL;
            hr = m_pNextOutSample->GetPointer(&pSampleData);

            if (FAILED(hr))
              break;

            ZeroMemory(pSampleData, bytesSilence);
            m_pNextOutSample->SetActualDataLength(bytesSilence);

            gapInBytes -= bytesSilence;
            m_pNextOutSample->SetMediaType(pmt);

            UINT nFrames = bytesSilence / m_pInputFormat->Format.nBlockAlign;
            m_rtNextIncomingSampleTime += nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;

            OutputSample();
          }
        }
        else
        {
          BYTE* pSampleData = NULL;
          pSample->GetPointer(&pSampleData);
          UINT32 nLength = pSample->GetActualDataLength();
          
          if (gapInBytes >= nLength)
            return S_OK; // Dropping the sample in this case is ok

          UINT32 nNewLength = nLength - gapInBytes;
          rtStart = rtStart - rtGap;

          UINT32 nFrames = nNewLength / m_pInputFormat->Format.nBlockAlign;
          REFERENCE_TIME rtDuration = nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;

          rtStop = rtStart + rtDuration;

          pSample->SetTime(&rtStart, &rtStop);
          pSample->SetActualDataLength(nNewLength);

          memmove(pSampleData, pSampleData + gapInBytes, nNewLength);

          m_rtNextIncomingSampleTime = rtStop;
          bNextSampleTimeCalculated = true;
        }
      }
      else
      {
        m_rtInSampleTime = rtStart;

        Log("CStreamSanitizer - resyncing in start of stream - m_rtStart: %I64d rtStart: %I64d m_rtInSampleTime: %I64d", 
          m_rtStart, rtStart, m_rtInSampleTime);
      }
    }
  }

  if (!bNextSampleTimeCalculated)
  {
    UINT nFrames = pSample->GetActualDataLength() / m_pInputFormat->Format.nBlockAlign;
    m_rtNextIncomingSampleTime += nFrames * UNITS / m_pInputFormat->Format.nSamplesPerSec;
  }

  m_nSampleNum++;

  if (pmt)
    DeleteMediaType(pmt);

  return m_pNextSink->PutSample(pSample);
}

void CStreamSanitizer::OutputSample()
{
  if (m_pNextOutSample)
  {
    UINT32 sampleLen = m_pNextOutSample->GetActualDataLength();
    HRESULT hr = OutputNextSample();
    m_rtInSampleTime += sampleLen / m_pOutputFormat->Format.nBlockAlign * UNITS / m_pOutputFormat->Format.nSamplesPerSec;

    if (FAILED(hr))
      Log("CTimeStretchFilter::timestretch thread OutputNextSample failed with: 0x%08x", hr);
  }
}