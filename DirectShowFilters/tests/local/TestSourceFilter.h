// A source filter with one output pin that delivers the bytes a test gives it. By default the pin is
// MEDIATYPE_Stream / MEDIASUBTYPE_MPEG2_TRANSPORT (the stand-in for TsReader in front of the DVBSub3 filter); SetMediaType
// changes the type it offers (e.g. PCM audio in front of an audio renderer).
#pragma once

#include <streams.h>
#include <vector>

class CTestSourcePin : public CBaseOutputPin
{
public:
  CTestSourcePin(CBaseFilter* filter, CCritSec* lock, HRESULT* phr);

  // The only type that the pin offers and accepts
  void SetMediaType(const CMediaType& mt) { m_type = mt; }
  const CMediaType& MediaType() const { return m_type; }

  HRESULT CheckMediaType(const CMediaType* mt) override;
  HRESULT GetMediaType(int position, CMediaType* mt) override;
  HRESULT DecideBufferSize(IMemAllocator* allocator, ALLOCATOR_PROPERTIES* request) override;

  // Delivers 'size' bytes in samples of at most 'sampleSize' bytes. Stops at the first failure.
  HRESULT DeliverBytes(const byte* data, size_t size, size_t sampleSize);

  // Flush notifications as a source filter sends them on a seek
  HRESULT FlushDownstream();

  // Delivers a sample with a presentation time (100 ns units) from the stream time of the graph
  HRESULT DeliverTimed(const byte* data, size_t size, REFERENCE_TIME start, REFERENCE_TIME stop, bool discontinuity);

private:
  CMediaType m_type;
};

class CTestSourceFilter : public CBaseFilter
{
public:
  explicit CTestSourceFilter(HRESULT* phr);
  ~CTestSourceFilter() override;

  int GetPinCount() override { return 1; }
  CBasePin* GetPin(int n) override { return n == 0 ? m_pin : nullptr; }

  CTestSourcePin* Pin() const { return m_pin; }

private:
  CCritSec m_lock;
  CTestSourcePin* m_pin;
};
