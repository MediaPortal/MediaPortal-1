#include "TestSourceFilter.h"

#include <algorithm>

CTestSourcePin::CTestSourcePin(CBaseFilter* filter, CCritSec* lock, HRESULT* phr)
  : CBaseOutputPin(NAME("TestSourcePin"), filter, lock, phr, L"Out")
{
  m_type.InitMediaType();
  m_type.majortype = MEDIATYPE_Stream;
  m_type.subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;
}

HRESULT CTestSourcePin::CheckMediaType(const CMediaType* mt)
{
  return (mt->majortype == m_type.majortype && mt->subtype == m_type.subtype) ? S_OK : S_FALSE;
}

HRESULT CTestSourcePin::GetMediaType(int position, CMediaType* mt)
{
  if (position != 0)
    return VFW_S_NO_MORE_ITEMS;
  *mt = m_type;
  return S_OK;
}

HRESULT CTestSourcePin::DecideBufferSize(IMemAllocator* allocator, ALLOCATOR_PROPERTIES* request)
{
  if (request->cBuffers < 2)
    request->cBuffers = 2;
  if (request->cbBuffer < 188 * 256)
    request->cbBuffer = 188 * 256;
  ALLOCATOR_PROPERTIES actual = {};
  HRESULT hr = allocator->SetProperties(request, &actual);
  if (FAILED(hr))
    return hr;
  return (actual.cbBuffer < request->cbBuffer) ? E_FAIL : S_OK;
}

HRESULT CTestSourcePin::DeliverBytes(const byte* data, size_t size, size_t sampleSize)
{
  size_t pos = 0;
  while (pos < size)
  {
    IMediaSample* sample = nullptr;
    HRESULT hr = GetDeliveryBuffer(&sample, nullptr, nullptr, 0);
    if (FAILED(hr))
      return hr;

    size_t n = (std::min)((std::min)(size - pos, sampleSize), static_cast<size_t>(sample->GetSize()));
    BYTE* buffer = nullptr;
    sample->GetPointer(&buffer);
    memcpy(buffer, data + pos, n);
    sample->SetActualDataLength(static_cast<long>(n));
    hr = Deliver(sample);
    sample->Release();
    if (hr != S_OK)
      return hr;
    pos += n;
  }
  return S_OK;
}

HRESULT CTestSourcePin::DeliverTimed(const byte* data, size_t size, REFERENCE_TIME start, REFERENCE_TIME stop, bool discontinuity)
{
  IMediaSample* sample = nullptr;
  HRESULT hr = GetDeliveryBuffer(&sample, nullptr, nullptr, 0);
  if (FAILED(hr))
    return hr;
  if (static_cast<size_t>(sample->GetSize()) < size)
  {
    sample->Release();
    return E_FAIL;
  }
  BYTE* buffer = nullptr;
  sample->GetPointer(&buffer);
  memcpy(buffer, data, size);
  sample->SetActualDataLength(static_cast<long>(size));
  sample->SetTime(&start, &stop);
  sample->SetSyncPoint(TRUE);
  sample->SetDiscontinuity(discontinuity ? TRUE : FALSE);
  hr = Deliver(sample);
  sample->Release();
  return hr;
}

HRESULT CTestSourcePin::FlushDownstream()
{
  HRESULT hr = DeliverBeginFlush();
  if (FAILED(hr))
    return hr;
  return DeliverEndFlush();
}

CTestSourceFilter::CTestSourceFilter(HRESULT* phr)
  : CBaseFilter(NAME("TestSource"), nullptr, &m_lock, GUID_NULL), m_pin(nullptr)
{
  m_pin = new CTestSourcePin(this, &m_lock, phr);
  if (!m_pin && phr)
    *phr = E_OUTOFMEMORY;
}

CTestSourceFilter::~CTestSourceFilter()
{
  delete m_pin;
}
