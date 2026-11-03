// dvblib.dll: the helper functions that TsReader's host (MediaPortal, via P/Invoke) and the B2C2 card code use to set up the
// MPEG-2 demultiplexer of Windows and a B2C2 data control. The demultiplexer (mpg2splt.ax) is part of Windows; the B2C2 control
// is replaced by a recording stand-in. The functions are called through the exports of the built DLL.
#include <gtest/gtest.h>
#include <streams.h>
#include <initguid.h>
#include <bdaiface.h>
#include <mpeg2data.h>
#include <vector>
#include <algorithm>
#include "FilterHost.h"
#include "../local/TestSourceFilter.h"
#include "../local/TestSinkFilter.h"
#include "b2c2_defs.h"
#include "ib2c2mpeg2datactrl.h"

namespace
{
  // the MPEG-2 Demultiplexer of Windows
  DEFINE_GUID(kClsidDemux, 0xAFB6C280, 0x2C41, 0x11D3, 0x8A, 0x60, 0x00, 0x00, 0xF8, 0x1E, 0x0E, 0x4A);

  typedef HRESULT(*GetPidMapFn)(IPin*, unsigned long*, unsigned long*);
  typedef HRESULT(*GetPidMappingFn)(IPin*, int*, int*, int*);
  typedef HRESULT(*SetupDemuxerFn)(IPin*, int, IPin*, int, IPin*, int);
  typedef HRESULT(*SetupDemuxerPinFn)(IPin*, int, int, bool);
  typedef HRESULT(*SetupDemuxerPidsFn)(IPin*, int*, int, int, bool);
  typedef HRESULT(*DumpFn)(IBaseFilter*);
  typedef HRESULT(*DeliverFn)(IPin*, IMediaSample*);
  typedef BOOL(*DeleteAllPIDsFn)(IB2C2MPEG2DataCtrl3*, long);
  typedef long(*SetPidToPinFn)(IB2C2MPEG2DataCtrl3*, long, long);
  typedef HRESULT(*SetWmvProfileFn)(IBaseFilter*, ULONG, ULONG, ULONG, ULONG);

  // A B2C2 data control that keeps the PIDs of its pins in a list
  class FakeDataCtrl : public IB2C2MPEG2DataCtrl3
  {
  public:
    std::vector<long> pids;
    std::vector<long> deletedFromPin;
    long lastPin = -1;
    bool failState = false;
    bool failAdd = false;
    int stateCalls = 0;

    STDMETHODIMP QueryInterface(REFIID, void**) override { return E_NOINTERFACE; }
    STDMETHODIMP_(ULONG) AddRef() override { return 2; }
    STDMETHODIMP_(ULONG) Release() override { return 1; }

    STDMETHODIMP GetMaxPIDCount(long*) override { return E_NOTIMPL; }
    STDMETHODIMP AddPIDs(long, long*) override { return E_NOTIMPL; }
    STDMETHODIMP DeletePIDs(long, long*) override { return E_NOTIMPL; }
    STDMETHODIMP GetMaxIpPIDCount(long*) override { return E_NOTIMPL; }
    STDMETHODIMP AddIpPIDs(long, long*) override { return E_NOTIMPL; }
    STDMETHODIMP DeleteIpPIDs(long, long*) override { return E_NOTIMPL; }
    STDMETHODIMP GetIpPIDs(long*, long*) override { return E_NOTIMPL; }
    STDMETHODIMP PurgeGlobalPIDs() override { return E_NOTIMPL; }
    STDMETHODIMP GetMaxGlobalPIDCount(long*) override { return E_NOTIMPL; }
    STDMETHODIMP GetGlobalPIDs(long*, long*) override { return E_NOTIMPL; }
    STDMETHODIMP ResetDataReceptionStats() override { return E_NOTIMPL; }
    STDMETHODIMP GetDataReceptionStats(long*, long*) override { return E_NOTIMPL; }

    STDMETHODIMP AddPIDsToPin(long* count, long* list, long pin) override
    {
      if (failAdd)
        return E_FAIL;
      lastPin = pin;
      for (long i = 0; i < *count; i++)
        pids.push_back(list[i]);
      return S_OK;
    }
    STDMETHODIMP DeletePIDsFromPin(long count, long* list, long pin) override
    {
      lastPin = pin;
      for (long i = 0; i < count; i++)
      {
        deletedFromPin.push_back(list[i]);
        pids.erase(std::remove(pids.begin(), pids.end(), list[i]), pids.end());
      }
      return S_OK;
    }

    STDMETHODIMP AddTsPIDs(long, long*) override { return E_NOTIMPL; }
    STDMETHODIMP DeleteTsPIDs(long, long*) override { return E_NOTIMPL; }
    STDMETHODIMP GetTsState(long*, long*, long* count, long* array) override
    {
      stateCalls++;
      if (failState)
        return E_FAIL;
      long n = static_cast<long>(pids.size());
      if (count && *count < n)
        n = *count;
      for (long i = 0; i < n; i++)
        array[i] = pids[i];
      if (count)
        *count = n;
      return S_OK;
    }
    STDMETHODIMP GetIpState(long*, long*, long*, long*) override { return E_NOTIMPL; }
    STDMETHODIMP GetReceivedDataIp(__int64*, __int64*) override { return E_NOTIMPL; }
    STDMETHODIMP AddMulticastMacAddress(tMacAddressList*) override { return E_NOTIMPL; }
    STDMETHODIMP GetMulticastMacAddressList(tMacAddressList*) override { return E_NOTIMPL; }
    STDMETHODIMP DeleteMulticastMacAddress(tMacAddressList*) override { return E_NOTIMPL; }
    STDMETHODIMP SetUnicastMacAddress(unsigned char*) override { return E_NOTIMPL; }
    STDMETHODIMP GetUnicastMacAddress(unsigned char*) override { return E_NOTIMPL; }
    STDMETHODIMP RestoreUnicastMacAddress() override { return E_NOTIMPL; }
  };

  class DvbLib : public testing::Test
  {
  protected:
    ComScope com;
    FilterModule module;
    Ptr<IBaseFilter> demux;
    Ptr<IMpeg2Demultiplexer> demuxer;

    void SetUp() override
    {
      ASSERT_TRUE(SUCCEEDED(com.Result()));
      ASSERT_TRUE(module.Load(L"dvblib.dll")) << "dvblib.dll";
    }

    template <class F> F Fn(const char* name)
    {
      F f = reinterpret_cast<F>(module.Export(name));
      EXPECT_NE(f, nullptr) << name;
      return f;
    }

    void MakeDemux()
    {
      ASSERT_EQ(CoCreateInstance(kClsidDemux, nullptr, CLSCTX_INPROC_SERVER, IID_IBaseFilter, reinterpret_cast<void**>(&demux)), S_OK)
        << "the MPEG-2 Demultiplexer of Windows is not available";
      ASSERT_EQ(demux.As(demuxer), S_OK);
    }

    // An output pin of the demultiplexer for a transport stream elementary stream
    HRESULT MakePin(const wchar_t* name, bool video, IPin** pin)
    {
      AM_MEDIA_TYPE mt = {};
      mt.majortype = video ? MEDIATYPE_Video : MEDIATYPE_Audio;
      mt.subtype = video ? MEDIASUBTYPE_MPEG2_VIDEO : MEDIASUBTYPE_MPEG2_AUDIO;
      mt.formattype = FORMAT_None;
      return demuxer->CreateOutputPin(&mt, const_cast<LPWSTR>(name), pin);
    }

    static void Map(IPin* pin, std::vector<ULONG> pids, MEDIA_SAMPLE_CONTENT content = MEDIA_ELEMENTARY_STREAM)
    {
      Ptr<IMPEG2PIDMap> map;
      ASSERT_EQ(pin->QueryInterface(__uuidof(IMPEG2PIDMap), reinterpret_cast<void**>(&map)), S_OK);
      ASSERT_EQ(map->MapPID(static_cast<ULONG>(pids.size()), pids.data(), content), S_OK);
    }

    static std::vector<ULONG> Mapped(IPin* pin)
    {
      std::vector<ULONG> result;
      Ptr<IMPEG2PIDMap> map;
      if (FAILED(pin->QueryInterface(__uuidof(IMPEG2PIDMap), reinterpret_cast<void**>(&map))))
        return result;
      Ptr<IEnumPIDMap> e;
      if (FAILED(map->EnumPIDMap(&e)))
        return result;
      PID_MAP pm[40];
      ULONG n = 0;
      e->Next(40, pm, &n);
      for (ULONG i = 0; i < n; i++)
        result.push_back(pm[i].ulPID);
      std::sort(result.begin(), result.end());
      return result;
    }
  };
}

TEST_F(DvbLib, ExportsTheDocumentedFunctions)
{
  for (const char* name : { "DeleteAllPIDs", "GetSectionData", "GetSectionCount", "SetPidToPin", "GetSectionPtr", "ReleaseSectionsBuffer",
                            "SetupDemuxer", "GetPidMap", "GetSNR", "SetWmvProfile", "DeliverMediaSample", "SetupDemuxerPin",
                            "DumpMpeg2DemuxerMappings", "SetupDemuxerPids", "GetPidMapping" })
    EXPECT_TRUE(module.HasExport(name)) << name;
}

// --- GetPidMapping / GetPidMap ----------------------------------------------------------------------------------------------

TEST_F(DvbLib, GetPidMappingReturnsWhatTheDemultiplexerHas)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  Map(pin.Get(), { 0x100, 0x101, 0x1FF });

  int pids[10] = {}, content[10] = {}, count = 10;
  EXPECT_EQ(Fn<GetPidMappingFn>("GetPidMapping")(pin.Get(), pids, content, &count), S_OK);
  ASSERT_EQ(count, 3);
  std::vector<int> got(pids, pids + count);
  std::sort(got.begin(), got.end());
  EXPECT_EQ(got, (std::vector<int>{ 0x100, 0x101, 0x1FF }));
  for (int i = 0; i < count; i++)
    EXPECT_EQ(content[i], static_cast<int>(MEDIA_ELEMENTARY_STREAM));
}

TEST_F(DvbLib, GetPidMappingNeverReturnsMoreThanTheCallerAsksFor)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  Map(pin.Get(), { 0x100, 0x101, 0x102, 0x103 });
  int pids[2] = { -1, -1 }, content[2] = {}, count = 2;
  EXPECT_EQ(Fn<GetPidMappingFn>("GetPidMapping")(pin.Get(), pids, content, &count), S_OK);
  EXPECT_EQ(count, 2);
}

TEST_F(DvbLib, GetPidMappingOfAPinWithoutMappingsIsEmpty)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  int pids[4] = {}, content[4] = {}, count = 4;
  EXPECT_EQ(Fn<GetPidMappingFn>("GetPidMapping")(pin.Get(), pids, content, &count), S_OK);
  EXPECT_EQ(count, 0);
}

TEST_F(DvbLib, GetPidMappingWithoutAPinOrOnAPinThatCannotMapIsEmpty)
{
  int pids[4] = {}, content[4] = {}, count = 4;
  EXPECT_EQ(Fn<GetPidMappingFn>("GetPidMapping")(nullptr, pids, content, &count), S_OK);
  EXPECT_EQ(count, 0);

  HRESULT hr = S_OK;
  CTestSinkFilter* sink = new CTestSinkFilter(&hr);
  sink->AddRef();
  count = 4;
  EXPECT_EQ(Fn<GetPidMappingFn>("GetPidMapping")(sink->Pin(), pids, content, &count), S_OK);
  EXPECT_EQ(count, 0);
  sink->Release();
}

TEST_F(DvbLib, GetPidMapReadsTheFirstMapping)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Audio", false, &pin), S_OK);
  Map(pin.Get(), { 0x201 });
  unsigned long pid = 0, type = 99;
  EXPECT_EQ(Fn<GetPidMapFn>("GetPidMap")(pin.Get(), &pid, &type), 0);
  EXPECT_EQ(pid, 0x201ul);
  EXPECT_EQ(type, static_cast<unsigned long>(MEDIA_ELEMENTARY_STREAM));
}

TEST_F(DvbLib, GetPidMapReportsAPinThatCannotMapWithAnErrorCode)
{
  HRESULT hr = S_OK;
  CTestSinkFilter* sink = new CTestSinkFilter(&hr);
  sink->AddRef();
  unsigned long pid = 0, type = 0;
  EXPECT_EQ(Fn<GetPidMapFn>("GetPidMap")(sink->Pin(), &pid, &type), 1);
  sink->Release();
}

// --- SetupDemuxerPin / SetupDemuxerPids / SetupDemuxer ---------------------------------------------------------------------

TEST_F(DvbLib, SetupDemuxerPinMapsThePid)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  EXPECT_EQ(Fn<SetupDemuxerPinFn>("SetupDemuxerPin")(pin.Get(), 0x300, MEDIA_ELEMENTARY_STREAM, false), S_OK);
  EXPECT_EQ(Mapped(pin.Get()), (std::vector<ULONG>{ 0x300 }));
}

TEST_F(DvbLib, SetupDemuxerPinKeepsOtherPidsUnlessAskedToDropThem)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  Map(pin.Get(), { 0x100, 0x101 });
  Fn<SetupDemuxerPinFn>("SetupDemuxerPin")(pin.Get(), 0x300, MEDIA_ELEMENTARY_STREAM, false);
  EXPECT_EQ(Mapped(pin.Get()), (std::vector<ULONG>{ 0x100, 0x101, 0x300 }));
  Fn<SetupDemuxerPinFn>("SetupDemuxerPin")(pin.Get(), 0x300, MEDIA_ELEMENTARY_STREAM, true);
  EXPECT_EQ(Mapped(pin.Get()), (std::vector<ULONG>{ 0x300 }));
}

TEST_F(DvbLib, SetupDemuxerPinRefusesAPinThatCannotMap)
{
  HRESULT hr = S_OK;
  CTestSinkFilter* sink = new CTestSinkFilter(&hr);
  sink->AddRef();
  EXPECT_EQ(Fn<SetupDemuxerPinFn>("SetupDemuxerPin")(sink->Pin(), 0x300, MEDIA_ELEMENTARY_STREAM, false), 1);
  sink->Release();
}

// no pin: nothing to map, like SetupDemuxer() and SetupDemuxerPids()
TEST_F(DvbLib, SetupDemuxerPinWithoutAPinDoesNothing)
{
  EXPECT_EQ(Fn<SetupDemuxerPinFn>("SetupDemuxerPin")(nullptr, 0x300, MEDIA_ELEMENTARY_STREAM, false), S_OK);
}

TEST_F(DvbLib, SetupDemuxerPidsMapsSeveralPids)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  int pids[3] = { 0x100, 0x110, 0x120 };
  EXPECT_EQ(Fn<SetupDemuxerPidsFn>("SetupDemuxerPids")(pin.Get(), pids, 3, MEDIA_ELEMENTARY_STREAM, false), S_OK);
  EXPECT_EQ(Mapped(pin.Get()), (std::vector<ULONG>{ 0x100, 0x110, 0x120 }));
}

TEST_F(DvbLib, SetupDemuxerPidsDropsThePidsThatAreNotInTheList)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  Map(pin.Get(), { 0x50, 0x51, 0x52 });
  int pids[2] = { 0x51, 0x400 };
  EXPECT_EQ(Fn<SetupDemuxerPidsFn>("SetupDemuxerPids")(pin.Get(), pids, 2, MEDIA_ELEMENTARY_STREAM, true), S_OK);
  EXPECT_EQ(Mapped(pin.Get()), (std::vector<ULONG>{ 0x51, 0x400 }));
}

TEST_F(DvbLib, SetupDemuxerPidsIgnoresPidsOutOfRange)
{
  MakeDemux();
  Ptr<IPin> pin;
  ASSERT_EQ(MakePin(L"Video", true, &pin), S_OK);
  int pids[3] = { -1, 0x1FFF, 0x2000 };
  EXPECT_EQ(Fn<SetupDemuxerPidsFn>("SetupDemuxerPids")(pin.Get(), pids, 3, MEDIA_ELEMENTARY_STREAM, false), S_OK);
  EXPECT_TRUE(Mapped(pin.Get()).empty());
}

TEST_F(DvbLib, SetupDemuxerReplacesTheMappingOfEveryPin)
{
  MakeDemux();
  Ptr<IPin> video, audio, ac3;
  ASSERT_EQ(MakePin(L"Video", true, &video), S_OK);
  ASSERT_EQ(MakePin(L"Audio", false, &audio), S_OK);
  ASSERT_EQ(MakePin(L"Ac3", false, &ac3), S_OK);
  Map(video.Get(), { 0x10, 0x11, 0x12 });
  Map(audio.Get(), { 0x20 });
  Map(ac3.Get(), { 0x30, 0x31 });

  EXPECT_EQ(Fn<SetupDemuxerFn>("SetupDemuxer")(video.Get(), 0x100, audio.Get(), 0x101, ac3.Get(), 0x102), S_OK);
  EXPECT_EQ(Mapped(video.Get()), (std::vector<ULONG>{ 0x100 }));
  EXPECT_EQ(Mapped(audio.Get()), (std::vector<ULONG>{ 0x101 }));
  EXPECT_EQ(Mapped(ac3.Get()), (std::vector<ULONG>{ 0x102 }));
}

TEST_F(DvbLib, SetupDemuxerWithoutPinsDoesNothing)
{
  EXPECT_EQ(Fn<SetupDemuxerFn>("SetupDemuxer")(nullptr, 0, nullptr, 0, nullptr, 0), S_OK);
}

TEST_F(DvbLib, SetupDemuxerWithAPidOfZeroOnlyUnmaps)
{
  MakeDemux();
  Ptr<IPin> video;
  ASSERT_EQ(MakePin(L"Video", true, &video), S_OK);
  Map(video.Get(), { 0x10 });
  EXPECT_EQ(Fn<SetupDemuxerFn>("SetupDemuxer")(video.Get(), 0, nullptr, 0, nullptr, 0), S_OK);
  EXPECT_TRUE(Mapped(video.Get()).empty());
}

TEST_F(DvbLib, DumpingTheMappingsOfADemultiplexerSucceeds)
{
  MakeDemux();
  Ptr<IPin> video, audio;
  ASSERT_EQ(MakePin(L"Video", true, &video), S_OK);
  ASSERT_EQ(MakePin(L"Audio", false, &audio), S_OK);
  Map(video.Get(), { 0x10 });
  EXPECT_EQ(Fn<DumpFn>("DumpMpeg2DemuxerMappings")(demux.Get()), S_OK);
}

// --- DeliverMediaSample -----------------------------------------------------------------------------------------------------

TEST_F(DvbLib, DeliverMediaSampleRefusesAPinThatTakesNoSamples)
{
  MakeDemux();
  Ptr<IPin> output;
  ASSERT_EQ(MakePin(L"Video", true, &output), S_OK);
  EXPECT_NE(Fn<DeliverFn>("DeliverMediaSample")(output.Get(), nullptr), S_OK) << "an output pin has no IMemInputPin";
}

// --- B2C2 data control ------------------------------------------------------------------------------------------------------

TEST_F(DvbLib, DeleteAllPIDsEmptiesThePin)
{
  FakeDataCtrl ctrl;
  ctrl.pids = { 0x100, 0x101, 0x102, 0x10 };
  EXPECT_TRUE(Fn<DeleteAllPIDsFn>("DeleteAllPIDs")(&ctrl, 2));
  EXPECT_TRUE(ctrl.pids.empty());
  EXPECT_EQ(ctrl.lastPin, 2);
  EXPECT_EQ(ctrl.deletedFromPin.size(), 4u);
}

TEST_F(DvbLib, DeleteAllPIDsNeedsSeveralRoundsForMoreThanTheMaximumPerPin)
{
  FakeDataCtrl ctrl;
  for (long i = 0; i < 100; i++)
    ctrl.pids.push_back(0x100 + i);
  EXPECT_TRUE(Fn<DeleteAllPIDsFn>("DeleteAllPIDs")(&ctrl, 0));
  EXPECT_TRUE(ctrl.pids.empty()) << "the control reports at most 39 PIDs per call";
  EXPECT_GE(ctrl.stateCalls, 3);
}

TEST_F(DvbLib, DeleteAllPIDsOfAnEmptyPin)
{
  FakeDataCtrl ctrl;
  EXPECT_TRUE(Fn<DeleteAllPIDsFn>("DeleteAllPIDs")(&ctrl, 0));
  EXPECT_TRUE(ctrl.deletedFromPin.empty() || ctrl.pids.empty());
}

TEST_F(DvbLib, DeleteAllPIDsFailsWhenTheStateCannotBeRead)
{
  FakeDataCtrl ctrl;
  ctrl.pids = { 1, 2 };
  ctrl.failState = true;
  EXPECT_FALSE(Fn<DeleteAllPIDsFn>("DeleteAllPIDs")(&ctrl, 0));
  EXPECT_EQ(ctrl.pids.size(), 2u);
}

TEST_F(DvbLib, DeleteAllPIDsWithoutAControlIsFine)
{
  EXPECT_TRUE(Fn<DeleteAllPIDsFn>("DeleteAllPIDs")(nullptr, 0));
}

TEST_F(DvbLib, SetPidToPinAddsOnePid)
{
  FakeDataCtrl ctrl;
  EXPECT_EQ(Fn<SetPidToPinFn>("SetPidToPin")(&ctrl, 3, 0x123), 1);
  EXPECT_EQ(ctrl.pids, (std::vector<long>{ 0x123 }));
  EXPECT_EQ(ctrl.lastPin, 3);
}

TEST_F(DvbLib, SetPidToPinReportsFailureAsZero)
{
  FakeDataCtrl ctrl;
  ctrl.failAdd = true;
  EXPECT_EQ(Fn<SetPidToPinFn>("SetPidToPin")(&ctrl, 0, 0x123), 0);
  EXPECT_EQ(Fn<SetPidToPinFn>("SetPidToPin")(nullptr, 0, 0x123), 0);
}

// --- SetWmvProfile ----------------------------------------------------------------------------------------------------------

TEST_F(DvbLib, SetWmvProfileIsAStub)
{
  // the whole body is commented out in the source: whatever filter it gets, it only answers S_OK (the host calls it from
  // TSReader2WMV and ignores the answer)
  HRESULT hr = S_OK;
  CTestSinkFilter* sink = new CTestSinkFilter(&hr);
  sink->AddRef();
  EXPECT_EQ(Fn<SetWmvProfileFn>("SetWmvProfile")(sink, 1000, 25, 640, 480), S_OK);
  sink->Release();
}
