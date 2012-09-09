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
#include "BaseAudioSink.h"

#include "alloctracing.h"

CBaseAudioSink::CBaseAudioSink(bool bHandleSampleRelease) : 
  m_bHandleSampleRelease(bHandleSampleRelease),
  m_pNextSink(NULL),
  m_pInputFormat(NULL),
  m_pOutputFormat(NULL),
  m_bOutFormatChanged(false),
  m_bDiscontinuity(false),
  m_pMemAllocator((IUnknown *)NULL),
  m_pNextOutSample(NULL),
  m_nSampleNum(0),
  m_bFlushing(false),
  m_chOrder(DS_ORDER)
{
}

CBaseAudioSink::~CBaseAudioSink()
{
  SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
  SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
}

// Initialization
HRESULT CBaseAudioSink::ConnectTo(IAudioSink* pSink)
{
  m_pNextSink = pSink;
  return S_OK;
}

HRESULT CBaseAudioSink::Disconnect()
{
  m_pNextSink = NULL;
  return S_OK;
}

HRESULT CBaseAudioSink::DisconnectAll()
{
  if (m_pNextSink)
    return m_pNextSink->DisconnectAll();

  return S_OK;
}

HRESULT CBaseAudioSink::Init()
{
  if (m_pNextSink)
    return m_pNextSink->Init();

  return S_OK;
}

HRESULT CBaseAudioSink::Cleanup()
{
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();
  
  m_pMemAllocator.Release();

  if (m_pNextSink)
    return m_pNextSink->Cleanup();

  return S_OK;
}

// Control
HRESULT CBaseAudioSink::Start(REFERENCE_TIME rtStart)
{
  if (m_pNextSink)
    return m_pNextSink->Start(rtStart);

  return S_OK;
}

HRESULT CBaseAudioSink::Run(REFERENCE_TIME rtStart)
{
  m_rtStart = rtStart;
  
  if (m_pMemAllocator)
    m_pMemAllocator->Commit();

  if (m_pNextSink)
    return m_pNextSink->Run(rtStart);

  return S_OK;
}

HRESULT CBaseAudioSink::Pause()
{
  if (m_pNextSink)
    return m_pNextSink->Pause();

  return S_OK;
}

HRESULT CBaseAudioSink::BeginStop()
{
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();
  
  if (m_pNextSink)
    return m_pNextSink->BeginStop();

  return S_OK;
}

HRESULT CBaseAudioSink::EndStop()
{
  CAutoLock lock (&m_csOutputSample);
  
  if (m_pNextOutSample)
    m_pNextOutSample.Release();

  if (m_pNextSink)
    return m_pNextSink->EndStop();

  return S_OK;
}

// Format negotiation
HRESULT CBaseAudioSink::NegotiateFormat(const WAVEFORMATEXTENSIBLE* pwfx, int nApplyChangesDepth, ChannelOrder* pChOrder)
{
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  if (m_pNextSink)
  {
    HRESULT hr = m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth, pChOrder);
    if (SUCCEEDED(hr))
      m_chOrder = *pChOrder;
	
    return hr;
  }

  return VFW_E_TYPE_NOT_ACCEPTED;
}

// Processing
HRESULT CBaseAudioSink::PutSample(IMediaSample* pSample)
{
  if (m_pNextSink)
    return m_pNextSink->PutSample(pSample);
  
  return S_OK;
}

HRESULT CBaseAudioSink::EndOfStream()
{
  if (m_pNextSink)
    return m_pNextSink->EndOfStream();

  return S_OK;
}

HRESULT CBaseAudioSink::BeginFlush()
{
  m_bFlushing = true;
  
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();

  if (m_pNextSink)
    return m_pNextSink->BeginFlush();

  return S_OK;
}

HRESULT CBaseAudioSink::EndFlush()
{
  CAutoLock lock (&m_csOutputSample);

  m_nSampleNum = 0;

  if (m_bHandleSampleRelease)
    m_pNextOutSample.Release();
  
  if (m_pMemAllocator)
    m_pMemAllocator->Commit();

  HRESULT hr = S_OK;

  if (m_pNextSink)
    hr = m_pNextSink->EndFlush();

  m_bFlushing = false;

  return hr;
}

// Helpers

bool CBaseAudioSink::FormatsEqual(const WAVEFORMATEXTENSIBLE* pwfx1, const WAVEFORMATEXTENSIBLE* pwfx2)
{
  if ((!pwfx1 && pwfx2) || (pwfx1 && !pwfx2))
    return false;

  if (pwfx1->Format.wFormatTag != pwfx2->Format.wFormatTag ||
      pwfx1->Format.nChannels != pwfx2->Format.nChannels ||
      pwfx1->Format.wBitsPerSample != pwfx2->Format.wBitsPerSample ||
      pwfx1->Format.nSamplesPerSec != pwfx2->Format.nSamplesPerSec ||
      pwfx1->Format.nBlockAlign != pwfx2->Format.nBlockAlign ||
      pwfx1->Format.nAvgBytesPerSec != pwfx2->Format.nAvgBytesPerSec ||
      pwfx1->dwChannelMask != pwfx2->dwChannelMask ||
      pwfx1->SubFormat != pwfx2->SubFormat ||
      pwfx1->Samples.wSamplesPerBlock != pwfx2->Samples.wSamplesPerBlock ||
      pwfx1->Samples.wValidBitsPerSample != pwfx2->Samples.wValidBitsPerSample)
    return false;

  return true;
}

HRESULT CBaseAudioSink::InitAllocator()
{
  ALLOCATOR_PROPERTIES propIn;
  ALLOCATOR_PROPERTIES propOut;
  HRESULT hr = OnInitAllocatorProperties(&propIn);
  
  if (FAILED(hr))
  {
    Log("CBaseAudioSink: Failed to get sample allocator properties (0x%08x)", hr);
    return hr;
  }
  
  CMemAllocator *pAllocator = new CMemAllocator("output sample allocator", NULL, &hr);

  if (FAILED(hr))
  {
    Log("CBaseAudioSink: Failed to create sample allocator (0x%08x)", hr);
    delete pAllocator;
    return hr;
  }

  hr = pAllocator->QueryInterface(IID_IMemAllocator, (void **)&m_pMemAllocator);

  if (FAILED(hr))
  {
    Log("CBaseAudioSink: Failed to get allocator interface (0x%08x)", hr);
    delete pAllocator;
    return hr;
  }

  m_pMemAllocator->SetProperties(&propIn, &propOut);
  hr = m_pMemAllocator->Commit();
  if (FAILED(hr))
  {
    Log("CBaseAudioSink: Failed to commit allocator properties (0x%08x)", hr);
    m_pMemAllocator.Release();
  }

  return hr;
}

HRESULT CBaseAudioSink::OnInitAllocatorProperties(ALLOCATOR_PROPERTIES *properties)
{
  properties->cBuffers = DEFAULT_OUT_BUFFER_COUNT;
  properties->cbBuffer = DEFAULT_OUT_BUFFER_SIZE;
  properties->cbPrefix = 0;
  properties->cbAlign = 8;

  return S_OK;
}

HRESULT CBaseAudioSink::SetInputFormat(WAVEFORMATEXTENSIBLE* pwfx, bool bAssumeOwnerShip)
{
  SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
  if (bAssumeOwnerShip)
    m_pInputFormat = pwfx;
  else
    return CopyWaveFormatEx(&m_pInputFormat, pwfx);

  return S_OK;
}

HRESULT CBaseAudioSink::SetInputFormat(const WAVEFORMATEXTENSIBLE* pwfx)
{
  SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
  return CopyWaveFormatEx(&m_pInputFormat, pwfx);
}

HRESULT CBaseAudioSink::SetOutputFormat(const WAVEFORMATEXTENSIBLE* pwfx)
{
  SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
  HRESULT hr = CopyWaveFormatEx(&m_pOutputFormat, pwfx);
  m_bOutFormatChanged = SUCCEEDED(hr);

  return hr;
}

HRESULT CBaseAudioSink::SetOutputFormat(WAVEFORMATEXTENSIBLE* pwfx, bool bAssumeOwnerShip)
{
  HRESULT hr = S_OK;

  SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
  if (bAssumeOwnerShip)
    m_pOutputFormat = pwfx;
  else
    hr = CopyWaveFormatEx(&m_pOutputFormat, pwfx);

  m_bOutFormatChanged = SUCCEEDED(hr);

  return hr;
}

HRESULT CBaseAudioSink::OutputNextSample()
{
  HRESULT hr = S_OK;
  if (m_pNextSink && m_pNextOutSample)
  {
    REFERENCE_TIME rtStart = 0;
    REFERENCE_TIME rtStop = 0;
    
    if (SUCCEEDED(m_pNextOutSample->GetTime(&rtStart, &rtStop)))
    {
      UINT nFrames = m_pNextOutSample->GetActualDataLength() / m_pOutputFormat->Format.nBlockAlign;
      REFERENCE_TIME rtDuration = nFrames * UNITS / m_pOutputFormat->Format.nSamplesPerSec;
    
      rtStop = rtStart + rtDuration;

      m_pNextOutSample->SetTime(&rtStart, &rtStop);
    }

    hr = m_pNextSink->PutSample(m_pNextOutSample);
    if (FAILED(hr))
      Log("CBaseAudioSink: Failed to output next sample: 0x%08x", hr);
  }
  m_pNextOutSample = NULL;
  
  return hr;
}

HRESULT CBaseAudioSink::RequestNextOutBuffer(REFERENCE_TIME rtStart)
{
  if (!m_pMemAllocator)
    return E_POINTER;

  HRESULT hr = m_pMemAllocator->GetBuffer(&m_pNextOutSample, NULL, NULL, 0);
  if (FAILED(hr))
    return hr;

  ASSERT(m_pNextOutSample);

  m_pNextOutSample->SetActualDataLength(0);
  m_pNextOutSample->SetTime(&rtStart, NULL);

  AM_MEDIA_TYPE pmt;
  if (SUCCEEDED(CreateAudioMediaType((WAVEFORMATEX*)m_pOutputFormat, &pmt, true)))
  {
    if (FAILED(m_pNextOutSample->SetMediaType(&pmt)))
      Log("CBaseAudioSink - failed to set mediatype: 0x%08x", hr);
    FreeMediaType(pmt);
  }
  m_bOutFormatChanged = false;

  if (m_bDiscontinuity)
  {
    if (FAILED(m_pNextOutSample->SetDiscontinuity(true)))
      Log("CBaseAudioSink - failed to set discontinuity: 0x%08x", hr);
    m_bDiscontinuity = false;
  }

  return S_OK;
}

