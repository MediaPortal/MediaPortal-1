// BDReader (Blu-ray source filter) with a real disc structure. The filter loads bluray.dll (libbluray) from the
// folder of BDReader.ax; the build copies it, and the BD-J jars, from the NuGet package MediaPortal.libbluray (x86 and x64). The sample is a BD folder:
// <samples>\BD\BDMV\index.bdmv.
#include <gtest/gtest.h>
#include <streams.h>
#include <chrono>
#include <thread>
#include "FilterHost.h"
#include "LocalSupport.h"
#include "TestSinkFilter.h"

namespace
{
  const GUID kBDReader = { 0x79a37017, 0x3178, 0x4859, { 0x80, 0x79, 0xec, 0xb9, 0xd5, 0x46, 0xfe, 0xb2 } };
  const GUID kIBDReader = { 0x79a37017, 0x3178, 0x4859, { 0x80, 0x79, 0xec, 0xb9, 0xd5, 0x46, 0xfe, 0xc2 } };

  // The first slots of IBDReader (BDReader.h): the header needs the libbluray headers, the tests only call these.
  struct IBDReaderHead : IUnknown
  {
    virtual HRESULT STDMETHODCALLTYPE SetGraphCallback(void*) = 0;
    virtual HRESULT STDMETHODCALLTYPE Action(int) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetAngle(UINT8) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetChapter(UINT32) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetAngle(UINT8*) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetChapter(UINT32*) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetTitleCount(UINT32*) = 0;
  };

  class BdReader : public testing::Test
  {
  protected:
    ComScope com;
    FilterModule module;
    Ptr<IBaseFilter> filter;
    Ptr<IFileSourceFilter> file;

    void SetUp() override
    {
      ASSERT_TRUE(SUCCEEDED(com.Result()));
      if (!module.Load(L"BDReader.ax"))
        GTEST_SKIP() << "BDReader.ax not built: " << ToUtf8(module.Error());
      ASSERT_EQ(module.CreateFilter(kBDReader, &filter), S_OK);
      ASSERT_EQ(filter.As(file), S_OK);
    }

    static bool HasLibbluray() { return GetFileAttributesW((FilterModule::PathOf(L"bluray.dll")).c_str()) != INVALID_FILE_ATTRIBUTES; }
  };
}

TEST_F(BdReader, IsASourceWithoutInputPins)
{
  EXPECT_EQ(CountPins(filter.Get(), PINDIR_INPUT), 0);
}

TEST_F(BdReader, RefusesAPathThatIsNotABluray)
{
  // Load() looks at the path before it touches libbluray: S_FALSE means "not for me", so the graph builder tries others
  for (const wchar_t* path : { L"C:\\movie.mkv", L"C:\\BDMV\\index.bdmv.bak", L"x", L"" })
    EXPECT_EQ(file->Load(path, nullptr), S_FALSE) << ToUtf8(path);
  EXPECT_EQ(file->Load(nullptr, nullptr), E_POINTER);
}

TEST_F(BdReader, TheBluraySuffixIsMatchedWithoutRegardToCase)
{
  TempFolder folder;   // an empty folder: past the path check, libbluray cannot open it
  const HRESULT hr = file->Load((folder.Path() + L"\\bdmv\\INDEX.BDMV").c_str(), nullptr);
  EXPECT_EQ(hr, static_cast<HRESULT>(VFW_E_NOT_FOUND));
}

TEST_F(BdReader, ADiscThatIsNotThereIsNotFound)
{
  TempFolder folder;
  EXPECT_EQ(file->Load((folder.Path() + L"\\BDMV\\index.bdmv").c_str(), nullptr), static_cast<HRESULT>(VFW_E_NOT_FOUND));
}

TEST_F(BdReader, ANameThatDoesNotFitTheFilterIsRefused)
{
  // the filter keeps 1023 characters of the name: a longer one used to be copied past the end of the buffer
  const std::wstring name = L"C:\\" + std::wstring(1500, L'a') + L"\\BDMV\\index.bdmv";
  EXPECT_EQ(file->Load(name.c_str(), nullptr), S_FALSE);
}

TEST_F(BdReader, AFolderPathLongerThanMaxPathIsNotCopiedIntoAShortBuffer)
{
  // 900 characters before \BDMV: more than MAX_PATH, it fits the buffers of the filter and the disc is simply not there
  const std::wstring name = L"C:\\" + std::wstring(900, L'a') + L"\\BDMV\\index.bdmv";
  EXPECT_EQ(file->Load(name.c_str(), nullptr), static_cast<HRESULT>(VFW_E_NOT_FOUND));
}

TEST_F(BdReader, LoadsTheSampleDisc)
{
  if (!HasLibbluray())
    GTEST_SKIP() << "bluray.dll is not next to BDReader.ax (NuGet package MediaPortal.libbluray is missing)";
  REQUIRE_SAMPLE(index, L"BD\\BDMV\\index.bdmv");

  const HRESULT loaded = file->Load(index.c_str(), nullptr);
  ASSERT_TRUE(SUCCEEDED(loaded)) << std::hex << loaded;

  LPOLESTR name = nullptr;
  ASSERT_EQ(file->GetCurFile(&name, nullptr), S_OK);
  ASSERT_NE(name, nullptr);
  EXPECT_EQ(std::wstring(name), index);
  CoTaskMemFree(name);

  EXPECT_GT(CountPins(filter.Get(), PINDIR_OUTPUT), 0);

  Ptr<IBDReaderHead> bd;
  ASSERT_EQ(filter->QueryInterface(kIBDReader, reinterpret_cast<void**>(&bd)), S_OK);
  UINT32 titles = 0;
  EXPECT_EQ(bd->GetTitleCount(&titles), S_OK);
  EXPECT_GT(titles, 0u);
  UINT8 angle = 255;
  EXPECT_EQ(bd->GetAngle(&angle), S_OK);
  EXPECT_LT(angle, 9);
}

// Plays the sample disc: BDReader in a graph, a sink on every output pin. libbluray reads the playlist, BDReader demultiplexes it and
// the video pin has to deliver samples with time stamps.
TEST_F(BdReader, PlaysTheSampleDisc)
{
  if (!HasLibbluray())
    GTEST_SKIP() << "bluray.dll is not next to BDReader.ax (NuGet package MediaPortal.libbluray is missing)";
  REQUIRE_SAMPLE(index, L"BD\\BDMV\\index.bdmv");

  Ptr<IGraphBuilder> graph;
  ASSERT_EQ(CreateGraph(&graph), S_OK);
  ASSERT_EQ(graph->AddFilter(filter.Get(), L"BDReader"), S_OK);
  ASSERT_TRUE(SUCCEEDED(file->Load(index.c_str(), nullptr)));

  std::vector<CTestSinkFilter*> sinks;
  Ptr<IEnumPins> pins;
  ASSERT_EQ(filter->EnumPins(&pins), S_OK);
  Ptr<IPin> pin;
  while (pins->Next(1, &pin, nullptr) == S_OK)
  {
    PIN_DIRECTION dir;
    pin->QueryDirection(&dir);
    if (dir == PINDIR_OUTPUT)
    {
      HRESULT hr = S_OK;
      auto* sink = new CTestSinkFilter(&hr);
      sink->AddRef();
      ASSERT_EQ(hr, S_OK);
      ASSERT_TRUE(SUCCEEDED(graph->AddFilter(sink, L"Sink")));   // VFW_S_DUPLICATE_NAME for the second one is a success
      if (graph->ConnectDirect(pin.Get(), sink->Pin(), nullptr) == S_OK)
        sinks.push_back(sink);
      else
      {
        graph->RemoveFilter(sink);
        sink->Release();
      }
    }
    pin.Reset();
  }
  ASSERT_FALSE(sinks.empty()) << "no output pin of BDReader accepted a connection";

  Ptr<IMediaControl> mc;
  ASSERT_EQ(graph.As(mc), S_OK);
  ASSERT_TRUE(SUCCEEDED(mc->Run()));

  size_t bytes = 0, samples = 0;
  const auto until = std::chrono::steady_clock::now() + std::chrono::seconds(8);
  while (std::chrono::steady_clock::now() < until && bytes == 0)
  {
    std::this_thread::sleep_for(std::chrono::milliseconds(200));
    for (auto* s : sinks)
    {
      bytes += s->Pin()->Bytes();
      samples += s->Pin()->Samples().size();
    }
  }
  mc->Stop();

  EXPECT_GT(samples, 0u);
  EXPECT_GT(bytes, 0u);
  size_t timed = 0;
  for (auto* s : sinks)
    for (const auto& r : s->Pin()->Samples())
      timed += r.hasTime ? 1 : 0;
  EXPECT_GT(timed, 0u) << "no sample carries a time stamp";

  for (auto* s : sinks)
  {
    graph->RemoveFilter(s);
    s->Release();
  }
  graph->RemoveFilter(filter.Get());
}
