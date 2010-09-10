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
#include "BaseAudioSink.h"

extern void Log(const char *fmt, ...);
extern HRESULT CopyWaveFormatEx(WAVEFORMATEX **dst, const WAVEFORMATEX *src);

CBaseAudioSink::CBaseAudioSink(void) 
: m_pNextSink(NULL)
, m_pInputFormat(NULL)
, m_pOutputFormat(NULL)
, m_bOutFormatChanged(false)
, m_pMemAllocator((IUnknown *)NULL)
, m_pNextOutSample(NULL)
{
}

CBaseAudioSink::~CBaseAudioSink(void)
{
  SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
  SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
}

// Initialization
HRESULT CBaseAudioSink::ConnectTo(IAudioSink *pSink)
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
  if(m_pNextSink)
    return m_pNextSink->DisconnectAll();
  return S_OK;
}


HRESULT CBaseAudioSink::Init()
{
  if(m_pNextSink)
    return m_pNextSink->Init();
  return S_OK;
}

HRESULT CBaseAudioSink::Cleanup()
{
  m_pMemAllocator.Release();

  if(m_pNextSink)
    return m_pNextSink->Cleanup();
  return S_OK;
}

// Control
HRESULT CBaseAudioSink::Start()
{
  if(m_pNextSink)
    return m_pNextSink->Start();
  return S_OK;
}

HRESULT CBaseAudioSink::BeginStop()
{
  if(m_pNextSink)
    return m_pNextSink->BeginStop();
  return S_OK;
}

HRESULT CBaseAudioSink::EndStop()
{
  if(m_pNextSink)
    return m_pNextSink->EndStop();
  return S_OK;
}

// Format negotiation
HRESULT CBaseAudioSink::NegotiateFormat(const WAVEFORMATEX *pwfx, int nApplyChangesDepth)
{
  if (nApplyChangesDepth != INFINITE && nApplyChangesDepth > 0)
    nApplyChangesDepth--;

  if(m_pNextSink)
    return m_pNextSink->NegotiateFormat(pwfx, nApplyChangesDepth);
  return VFW_E_TYPE_NOT_ACCEPTED;
}

// Processing
HRESULT CBaseAudioSink::PutSample(IMediaSample *pSample)
{
  if(m_pNextSink)
    return m_pNextSink->PutSample(pSample);
  return S_OK;
}

HRESULT CBaseAudioSink::EndOfStream()
{
  if(m_pNextSink)
    return m_pNextSink->EndOfStream();
  return S_OK;
}

HRESULT CBaseAudioSink::BeginFlush()
{
  if (m_pMemAllocator)
    m_pMemAllocator->Decommit();
  
  m_pNextOutSample.Release();

  if(m_pNextSink)
    return m_pNextSink->BeginFlush();
  return S_OK;
}

HRESULT CBaseAudioSink::EndFlush()
{
  if (m_pMemAllocator)
    m_pMemAllocator->Commit();

  if(m_pNextSink)
    return m_pNextSink->EndFlush();
  return S_OK;
}

// Helpers

bool CBaseAudioSink::FormatsEqual(const WAVEFORMATEX *pwfx1, const WAVEFORMATEX *pwfx2)
{
  if (pwfx1 == NULL && pwfx2 != NULL)
    return true;

  if (pwfx1 != NULL && pwfx2 == NULL)
    return true;

  if (pwfx1->wFormatTag != pwfx2->wFormatTag ||
      pwfx1->nChannels != pwfx2->nChannels ||
      pwfx1->wBitsPerSample != pwfx2->wBitsPerSample) // TODO : improve the checks
    return true;

  return false;
}


HRESULT CBaseAudioSink::InitAllocator()
{
  ALLOCATOR_PROPERTIES propIn;
  ALLOCATOR_PROPERTIES propOut;
  HRESULT hr = OnInitAllocatorProperties(&propIn);
  
  if (FAILED(hr))
  {
    Log("Failed to get sample allocator properties (0x%08x)", hr);
    return hr;
  }
  
  CMemAllocator *pAllocator = new CMemAllocator("output sample allocator", NULL, &hr);

  if (FAILED(hr))
  {
    Log("Failed to create sample allocator (0x%08x)", hr);
    delete pAllocator;
    return hr;
  }

  hr = pAllocator->QueryInterface(IID_IMemAllocator, (void **)&m_pMemAllocator);

  if (FAILED(hr))
  {
    Log("Failed to get allocator interface (0x%08x)", hr);
    delete pAllocator;
    return hr;
  }

  m_pMemAllocator->SetProperties(&propIn, &propOut);
  hr = m_pMemAllocator->Commit();
  if (FAILED(hr))
  {
    Log("Failed to commit allocator properties (0x%08x)", hr);
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

HRESULT CBaseAudioSink::SetInputFormat(WAVEFORMATEX *pwfx, bool bAssumeOwnerShip)
{
  SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
  if (bAssumeOwnerShip)
    m_pInputFormat = pwfx;
  else
    return CopyWaveFormatEx(&m_pInputFormat, pwfx);

  return S_OK;
}

HRESULT CBaseAudioSink::SetInputFormat(const WAVEFORMATEX *pwfx)
{
  SAFE_DELETE_WAVEFORMATEX(m_pInputFormat);
  return CopyWaveFormatEx(&m_pInputFormat, pwfx);
}

HRESULT CBaseAudioSink::SetOutputFormat(const WAVEFORMATEX *pwfx)
{
  SAFE_DELETE_WAVEFORMATEX(m_pOutputFormat);
  HRESULT hr = CopyWaveFormatEx(&m_pOutputFormat, pwfx);
  m_bOutFormatChanged = SUCCEEDED(hr);

  return hr;
}

HRESULT CBaseAudioSink::SetOutputFormat(WAVEFORMATEX *pwfx, bool bAssumeOwnerShip)
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
    hr = m_pNextSink->PutSample(m_pNextOutSample);
    if (FAILED(hr))
      Log("CBaseAudioSink: Failed to output next sample: 0x%08x", hr);
  }
  m_pNextOutSample = NULL;
  return hr;
}

HRESULT CBaseAudioSink::RequestNextOutBuffer(REFERENCE_TIME rtStart)
{
  if (m_pMemAllocator == NULL)
    return E_POINTER;

  HRESULT hr = m_pMemAllocator->GetBuffer(&m_pNextOutSample, NULL, NULL, 0);
  if(FAILED(hr))
    return hr;

  ASSERT(m_pNextOutSample != NULL);

  m_pNextOutSample->SetActualDataLength(0);
  m_pNextOutSample->SetTime(&rtStart, NULL);

  if (m_bOutFormatChanged)
  {
    AM_MEDIA_TYPE pmt;
    if (SUCCEEDED(CreateAudioMediaType(m_pOutputFormat, &pmt, true)))
    {
      m_pNextOutSample->SetMediaType(&pmt);
      FreeMediaType(pmt);
    }
    m_bOutFormatChanged = false;
  }
  return S_OK;
}

