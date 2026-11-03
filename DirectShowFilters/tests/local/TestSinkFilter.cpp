#include "TestSinkFilter.h"

CTestSinkPin::CTestSinkPin(CTestSinkFilter* filter, CCritSec* lock, HRESULT* phr)
  : CBaseInputPin(NAME("TestSinkPin"), filter, lock, phr, L"In")
{
}

HRESULT CTestSinkPin::CheckMediaType(const CMediaType* mt)
{
  return (m_major == GUID_NULL || mt->majortype == m_major) ? S_OK : S_FALSE;
}

STDMETHODIMP CTestSinkPin::Receive(IMediaSample* sample)
{
  HRESULT hr = CBaseInputPin::Receive(sample);
  if (hr != S_OK)
    return hr;

  ReceivedSample r;
  BYTE* data = nullptr;
  if (SUCCEEDED(sample->GetPointer(&data)) && data)
    r.data.assign(data, data + sample->GetActualDataLength());
  r.hasTime = SUCCEEDED(sample->GetTime(&r.start, &r.stop));
  r.discontinuity = sample->IsDiscontinuity() == S_OK;
  AM_MEDIA_TYPE* changed = nullptr;
  if (sample->GetMediaType(&changed) == S_OK && changed)
  {
    r.typeChanged = true;
    DeleteMediaType(changed);
  }
  CAutoLock lock(&m_dataLock);
  m_samples.push_back(std::move(r));
  return S_OK;
}

STDMETHODIMP CTestSinkPin::EndOfStream()
{
  m_eos = true;
  return CBaseInputPin::EndOfStream();
}

STDMETHODIMP CTestSinkPin::BeginFlush()
{
  m_flushes++;
  return CBaseInputPin::BeginFlush();
}

STDMETHODIMP CTestSinkPin::EndFlush()
{
  return CBaseInputPin::EndFlush();
}

STDMETHODIMP CTestSinkPin::NewSegment(REFERENCE_TIME start, REFERENCE_TIME stop, double rate)
{
  m_segments++;
  return CBaseInputPin::NewSegment(start, stop, rate);
}

std::vector<ReceivedSample> CTestSinkPin::Samples()
{
  CAutoLock lock(&m_dataLock);
  return m_samples;
}

size_t CTestSinkPin::Bytes()
{
  CAutoLock lock(&m_dataLock);
  size_t n = 0;
  for (const auto& s : m_samples)
    n += s.data.size();
  return n;
}

void CTestSinkPin::Clear()
{
  CAutoLock lock(&m_dataLock);
  m_samples.clear();
  m_eos = false;
}

CTestSinkFilter::CTestSinkFilter(HRESULT* phr)
  : CBaseFilter(NAME("TestSink"), nullptr, &m_lock, GUID_NULL), m_pin(nullptr)
{
  m_pin = new CTestSinkPin(this, &m_lock, phr);
  if (!m_pin && phr)
    *phr = E_OUTOFMEMORY;
}

CTestSinkFilter::~CTestSinkFilter()
{
  delete m_pin;
}
