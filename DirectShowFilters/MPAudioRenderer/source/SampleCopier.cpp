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
#include "SampleCopier.h"
#include <Audioclient.h>

#include "alloctracing.h"

CSampleCopier::CSampleCopier(AudioRendererSettings* pSettings) : 
  CBaseAudioSink(true, pSettings),  
  m_bPassThrough(false),
  m_rtInSampleTime(0),
  m_rtNextIncomingSampleTime(0)
{
  m_bNextFormatPassthru = false;
}

CSampleCopier::~CSampleCopier()
{
}

HRESULT CSampleCopier::Init()
{
  return CBaseAudioSink::Init();
}

HRESULT CSampleCopier::Cleanup()
{
  return CBaseAudioSink::Cleanup();
}

HRESULT CSampleCopier::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
{
  if (!pwfx)
    return VFW_E_TYPE_NOT_ACCEPTED;

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

    m_bNextFormatPassthru = true;
    m_chOrder = *pChOrder;
    return hr;
  }

  hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
  if (SUCCEEDED(hr))
  {
    if (CanBitstream(pwfx))
    {
      hr = m_pNextSink->NegotiateBuffer(pwfx, &m_nOutBufferSize, &m_nOutBufferCount, true);
      m_chOrder = *pChOrder;
    }
  }

  return hr;
}

// Processing
HRESULT CSampleCopier::PutSample(IMediaSample *pSample)
{
  if (!pSample)
    return S_OK;

  WAVEFORMATEXTENSIBLE* pwfe = NULL;
  AM_MEDIA_TYPE *pmt = NULL;
  bool bFormatChanged = false;
  
  HRESULT hr = S_OK;

  CAutoLock lock (&m_csOutputSample);
  if (m_bFlushing)
    return S_OK;

  if (SUCCEEDED(pSample->GetMediaType(&pmt)) && pmt != NULL)
  {
    pwfe = (WAVEFORMATEXTENSIBLE*)pmt->pbFormat;
    bFormatChanged = !FormatsEqual(pwfe, m_pInputFormat);
  }

  if (bFormatChanged)
  {
    m_bBitstreaming = CanBitstream(pwfe);

    // Apply format change locally, 
    // next filter will evaluate the format change when it receives the sample
    Log("CSampleCopier::PutSample: Processing format change");
    ChannelOrder chOrder;
    hr = NegotiateFormat((WAVEFORMATEXTENSIBLE*)pmt->pbFormat, 1, &chOrder);
    if (FAILED(hr))
    {
      DeleteMediaType(pmt);
      Log("SampleCopier: PutSample failed to change format: 0x%08x", hr);
      return hr;
    }

    SetInputFormat(pwfe);
    m_chOrder = chOrder;
  }

  if (m_bBitstreaming)
  {
    ASSERT(m_pMemAllocator);

    if (!m_pMemAllocator)
    {
      Log("CSampleCopier::PutSample m_pMemAllocator is NULL");
      return E_POINTER;
    }

    REFERENCE_TIME rtStop = 0;
    REFERENCE_TIME rtStart = 0;
    if (SUCCEEDED(hr = pSample->GetTime(&rtStart, &rtStop)))
    {
      if (FAILED(hr = RequestNextOutBuffer(rtStart)))
      {
        if (pmt)
          DeleteMediaType(pmt);

        return hr;
      }
    }
    else
      return hr;

    ASSERT(m_pNextOutSample);

    if (pmt)
    {
      if (FAILED(m_pNextOutSample->SetMediaType(pmt)))
        Log("CBaseAudioSink - failed to set mediatype: 0x%08x", hr);

      DeleteMediaType(pmt);
    }

    if (pSample->IsDiscontinuity() == S_OK)
      m_bDiscontinuity = true;

    if (m_bDiscontinuity)
    {
      if (FAILED(m_pNextOutSample->SetDiscontinuity(true)))
        Log("CBaseAudioSink - failed to set discontinuity: 0x%08x", hr);
    }

    long cbSampleData = pSample->GetActualDataLength();
    long cbDestSize = m_pNextOutSample->GetSize();

    ASSERT(cbDestSize >= cbSampleData);

    m_pNextOutSample->SetActualDataLength(cbSampleData);

    BYTE *pSourceData = NULL;
    BYTE *pDestData = NULL;
    if (FAILED(hr = pSample->GetPointer(&pSourceData)))
    {
      ASSERT(pSourceData);
      Log("CSampleCopier::PutSample - failed to get input sample's data pointer: 0x%08x", hr);
      return hr;
    }

    if (FAILED(hr = m_pNextOutSample->GetPointer(&pDestData)))
    {
      ASSERT(pSourceData);
      Log("CSampleCopier::PutSample - failed to get output sample's data pointer: 0x%08x", hr);
      return hr;
    }

    memcpy(pDestData, pSourceData, cbSampleData);

    return OutputNextSample();
  }
  else
    return m_pNextSink->PutSample(pSample);
}

HRESULT CSampleCopier::EndOfStream()
{
  return CBaseAudioSink::EndOfStream();
}
