// Loads each built filter DLL the way DirectShow does (DllGetClassObject), without registering it.
#include <gtest/gtest.h>
#include "FilterHost.h"

namespace
{
  struct FilterInfo
  {
    const wchar_t* file;
    GUID clsid;
    const char* name;
  };

  const FilterInfo kFilters[] = {
    { L"TsReader.ax",        { 0xb9559486, 0xe1bb, 0x45d3, { 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x3f } }, "TsReader" },
    { L"TsWriter.ax",        { 0xfc50bed6, 0xfe38, 0x42d3, { 0xb8, 0x31, 0x77, 0x16, 0x90, 0x09, 0x1a, 0x6e } }, "TsWriter" },
    { L"mpaudiorenderer.ax", { 0xEC9ED6FC, 0x7B03, 0x4CB6, { 0x8C, 0x01, 0x4E, 0xAB, 0xE1, 0x09, 0xF2, 0x6B } }, "MPAudioRenderer" },
    { L"DVBSub3.ax",         { 0x3b4c4f66, 0x739f, 0x452c, { 0xaf, 0xc4, 0x1c, 0x03, 0x9b, 0xed, 0x32, 0x99 } }, "DVBSub3" },
    { L"MPFileWriter.ax",    { 0xdb35f5ed, 0x26b2, 0x4a2a, { 0x92, 0xd3, 0x85, 0x2e, 0x14, 0x5b, 0xf3, 0x2d } }, "MPFileWriter" },
    { L"MPIPTVSource.ax",    { 0xd3dd4c59, 0xd3a7, 0x4b82, { 0x97, 0x27, 0x7b, 0x92, 0x03, 0xeb, 0x67, 0xc0 } }, "MPIPTVSource" },
    { L"TsMuxer.ax",         { 0x511d13f0, 0x8a56, 0x42fa, { 0xb1, 0x51, 0xb7, 0x2a, 0x32, 0x5c, 0xf7, 0x1a } }, "TsMuxer" },
    { L"cccp.ax",            { 0x6f0b7d9c, 0x7548, 0x49a9, { 0xac, 0x4c, 0x1d, 0xa1, 0x92, 0x7e, 0x6c, 0x15 } }, "CcParser" },
  };

  class FilterLoad : public testing::TestWithParam<FilterInfo>
  {
  protected:
    ComScope com;
    void SetUp() override { ASSERT_TRUE(SUCCEEDED(com.Result())); }
  };
}

INSTANTIATE_TEST_SUITE_P(Filters, FilterLoad, testing::ValuesIn(kFilters),
  [](const testing::TestParamInfo<FilterInfo>& i) { return std::string(i.param.name); });

TEST_P(FilterLoad, DllLoadsAndExportsTheDirectShowEntryPoints)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(GetParam().file)) << std::string(m.Error().begin(), m.Error().end());
  EXPECT_TRUE(m.HasExport("DllGetClassObject"));
  EXPECT_TRUE(m.HasExport("DllCanUnloadNow"));
  EXPECT_TRUE(m.HasExport("DllRegisterServer"));
  EXPECT_TRUE(m.HasExport("DllUnregisterServer"));
}

TEST_P(FilterLoad, CreatesTheFilterAndReportsItsClassId)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(GetParam().file)) << std::string(m.Error().begin(), m.Error().end());

  Ptr<IBaseFilter> filter;
  ASSERT_EQ(m.CreateFilter(GetParam().clsid, &filter), S_OK);
  ASSERT_TRUE(filter);

  CLSID id = {};
  ASSERT_EQ(filter->GetClassID(&id), S_OK);
  EXPECT_TRUE(IsEqualCLSID(id, GetParam().clsid));
}

TEST_P(FilterLoad, UnknownClassIsRefused)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(GetParam().file)) << std::string(m.Error().begin(), m.Error().end());

  const CLSID unknown = { 0x12345678, 0x1234, 0x1234, { 1, 2, 3, 4, 5, 6, 7, 8 } };
  void* object = nullptr;
  EXPECT_EQ(m.GetClassObject(unknown, IID_IClassFactory, &object), CLASS_E_CLASSNOTAVAILABLE);
  EXPECT_EQ(object, nullptr);
}

TEST_P(FilterLoad, FilterJoinsAndLeavesAGraph)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(GetParam().file)) << std::string(m.Error().begin(), m.Error().end());

  Ptr<IGraphBuilder> graph;
  ASSERT_EQ(CreateGraph(&graph), S_OK);
  Ptr<IBaseFilter> filter;
  ASSERT_EQ(m.CreateFilter(GetParam().clsid, &filter), S_OK);

  ASSERT_TRUE(SUCCEEDED(graph->AddFilter(filter.Get(), L"filter under test")));

  FILTER_INFO info = {};
  ASSERT_EQ(filter->QueryFilterInfo(&info), S_OK);
  EXPECT_EQ(info.pGraph, static_cast<IFilterGraph*>(graph.Get()));
  EXPECT_STREQ(info.achName, L"filter under test");
  if (info.pGraph)
    info.pGraph->Release();

  EXPECT_EQ(graph->RemoveFilter(filter.Get()), S_OK);
}

TEST_P(FilterLoad, PinsCanBeEnumerated)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(GetParam().file)) << std::string(m.Error().begin(), m.Error().end());

  Ptr<IBaseFilter> filter;
  ASSERT_EQ(m.CreateFilter(GetParam().clsid, &filter), S_OK);
  EXPECT_GE(CountPins(filter.Get(), PINDIR_INPUT), 0);
  EXPECT_GE(CountPins(filter.Get(), PINDIR_OUTPUT), 0);
  // every filter in the set has at least one pin
  EXPECT_GT(CountPins(filter.Get(), PINDIR_INPUT) + CountPins(filter.Get(), PINDIR_OUTPUT), 0);
}

TEST_P(FilterLoad, CanBeCreatedAndDestroyedRepeatedly)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(GetParam().file)) << std::string(m.Error().begin(), m.Error().end());
  for (int i = 0; i < 20; i++)
  {
    Ptr<IBaseFilter> filter;
    ASSERT_EQ(m.CreateFilter(GetParam().clsid, &filter), S_OK) << "round " << i;
  }
  // all instances are gone: the DLL must agree that it can be unloaded
  EXPECT_EQ(m.CanUnloadNow(), S_OK);
}

TEST_P(FilterLoad, InterfacesOfBaseFilter)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(GetParam().file)) << std::string(m.Error().begin(), m.Error().end());

  Ptr<IBaseFilter> filter;
  ASSERT_EQ(m.CreateFilter(GetParam().clsid, &filter), S_OK);

  Ptr<IMediaFilter> mediaFilter;
  EXPECT_EQ(filter.As(mediaFilter), S_OK);
  Ptr<IPersist> persist;
  EXPECT_EQ(filter.As(persist), S_OK);
  Ptr<IUnknown> unknown;
  EXPECT_EQ(filter.As(unknown), S_OK);

  // an interface that no filter implements
  void* none = nullptr;
  const IID bogus = { 0xdeadbeef, 0x1, 0x2, { 3, 4, 5, 6, 7, 8, 9, 10 } };
  EXPECT_EQ(filter->QueryInterface(bogus, &none), E_NOINTERFACE);
  EXPECT_EQ(none, nullptr);
}
