// A sink filter that accepts a given media type and keeps what it receives: the stand-in for a renderer behind the filter
// under test (e.g. behind the audio switcher).
#pragma once

#include <streams.h>
#include <vector>

class CTestSinkFilter;

struct ReceivedSample
{
  std::vector<byte> data;
  REFERENCE_TIME start = 0, stop = 0;
  bool hasTime = false;
  bool discontinuity = false;
  bool typeChanged = false;                 // the sample carried a new media type
};

class CTestSinkPin : public CBaseInputPin
{
public:
  CTestSinkPin(CTestSinkFilter* filter, CCritSec* lock, HRESULT* phr);

  // The pin accepts every type with this major type (GUID_NULL: every type)
  void AcceptMajorType(const GUID& major) { m_major = major; }

  HRESULT CheckMediaType(const CMediaType* mt) override;
  STDMETHODIMP Receive(IMediaSample* sample) override;
  STDMETHODIMP EndOfStream() override;
  STDMETHODIMP BeginFlush() override;
  STDMETHODIMP EndFlush() override;
  STDMETHODIMP NewSegment(REFERENCE_TIME start, REFERENCE_TIME stop, double rate) override;

  std::vector<ReceivedSample> Samples();    // copy, taken under the lock
  size_t Bytes();
  bool EndOfStreamSeen() const { return m_eos; }
  int Flushes() const { return m_flushes; }
  int Segments() const { return m_segments; }
  CMediaType Connected() const { return m_mt; }
  void Clear();

private:
  CCritSec m_dataLock;
  std::vector<ReceivedSample> m_samples;
  GUID m_major = GUID_NULL;
  volatile bool m_eos = false;
  volatile int m_flushes = 0;
  volatile int m_segments = 0;
};

class CTestSinkFilter : public CBaseFilter
{
public:
  explicit CTestSinkFilter(HRESULT* phr);
  ~CTestSinkFilter() override;

  int GetPinCount() override { return 1; }
  CBasePin* GetPin(int n) override { return n == 0 ? m_pin : nullptr; }
  CTestSinkPin* Pin() const { return m_pin; }

private:
  CCritSec m_lock;
  CTestSinkPin* m_pin;
};
