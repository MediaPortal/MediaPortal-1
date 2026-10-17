// TsReader (MediaPortal File Reader) in a graph, reading transport stream files made by the tests.
#include <gtest/gtest.h>
#include <random>
#include "FilterHost.h"
#include "TsStreamBuilder.h"

namespace
{
  const CLSID kTsReader = { 0xb9559486, 0xe1bb, 0x45d3, { 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x3f } };

  std::wstring TempFile(const wchar_t* name)
  {
    wchar_t dir[MAX_PATH] = {};
    GetTempPathW(MAX_PATH, dir);
    return std::wstring(dir) + L"mp_tests_" + std::to_wstring(GetCurrentProcessId()) + L"_" + name;
  }

  class TsReaderGraph : public testing::Test
  {
  protected:
    ComScope com;
    FilterModule module;
    Ptr<IGraphBuilder> graph;
    Ptr<IBaseFilter> reader;
    std::vector<std::wstring> files;

    void SetUp() override
    {
      ASSERT_TRUE(SUCCEEDED(com.Result()));
      ASSERT_TRUE(module.Load(L"TsReader.ax")) << std::string(module.Error().begin(), module.Error().end());
      ASSERT_EQ(CreateGraph(&graph), S_OK);
      ASSERT_EQ(module.CreateFilter(kTsReader, &reader), S_OK);
      ASSERT_TRUE(SUCCEEDED(graph->AddFilter(reader.Get(), L"TsReader")));
    }

    void TearDown() override
    {
      if (graph && reader)
      {
        Ptr<IMediaControl> control;
        if (SUCCEEDED(graph.As(control)))
          control->Stop();
        graph->RemoveFilter(reader.Get());
      }
      reader.Reset();
      graph.Reset();
      for (const auto& f : files)
        DeleteFileW(f.c_str());
    }

    std::wstring MakeFile(const wchar_t* name, const std::vector<byte>& data)
    {
      std::wstring path = TempFile(name);
      files.push_back(path);
      EXPECT_TRUE(TsStreamBuilder::WriteFile(path, data));
      return path;
    }

    HRESULT LoadFile(const std::wstring& path)
    {
      Ptr<IFileSourceFilter> source;
      HRESULT hr = reader.As(source);
      if (FAILED(hr))
        return hr;
      return source->Load(path.c_str(), NULL);
    }
  };
}

TEST_F(TsReaderGraph, ImplementsIFileSourceFilter)
{
  Ptr<IFileSourceFilter> source;
  EXPECT_EQ(reader.As(source), S_OK);
}

TEST_F(TsReaderGraph, MissingFileIsRefused)
{
  HRESULT hr = LoadFile(TempFile(L"does_not_exist.ts"));
  EXPECT_TRUE(FAILED(hr)) << std::hex << hr;
}

TEST_F(TsReaderGraph, LoadedFileNameCanBeReadBack)
{
  std::wstring path = MakeFile(L"readback.ts", TsStreamBuilder::Build(40));
  ASSERT_EQ(LoadFile(path), S_OK);

  Ptr<IFileSourceFilter> source;
  ASSERT_EQ(reader.As(source), S_OK);
  LPOLESTR name = NULL;
  ASSERT_EQ(source->GetCurFile(&name, NULL), S_OK);
  ASSERT_NE(name, nullptr);
  EXPECT_EQ(std::wstring(name), path);
  CoTaskMemFree(name);
}

TEST_F(TsReaderGraph, ValidStreamExposesOutputPins)
{
  std::wstring path = MakeFile(L"valid.ts", TsStreamBuilder::Build(60));
  ASSERT_EQ(LoadFile(path), S_OK);

  EXPECT_EQ(CountPins(reader.Get(), PINDIR_INPUT), 0);
  EXPECT_GE(CountPins(reader.Get(), PINDIR_OUTPUT), 1);
}

TEST_F(TsReaderGraph, RandomBytesDoNotCrashTheReader)
{
  // not a transport stream at all
  std::mt19937 rng(12345);
  std::vector<byte> noise(188 * 200);
  for (auto& b : noise)
    b = static_cast<byte>(rng());
  std::wstring path = MakeFile(L"noise.ts", noise);
  HRESULT hr = LoadFile(path);
  (void)hr;                                  // accepted or refused; it only has to survive
  SUCCEED();
}

TEST_F(TsReaderGraph, DamagedStreamDoesNotCrashTheReader)
{
  // valid stream with bytes overwritten here and there
  std::vector<byte> ts = TsStreamBuilder::Build(60);
  std::mt19937 rng(777);
  for (int i = 0; i < 400; i++)
    ts[rng() % ts.size()] = static_cast<byte>(rng());
  std::wstring path = MakeFile(L"damaged.ts", ts);
  HRESULT hr = LoadFile(path);
  (void)hr;
  SUCCEED();
}

TEST_F(TsReaderGraph, TruncatedStreamDoesNotCrashTheReader)
{
  std::vector<byte> ts = TsStreamBuilder::Build(60);
  for (size_t size : { size_t(0), size_t(1), size_t(187), size_t(188), size_t(189), size_t(188 * 3 + 50) })
  {
    std::vector<byte> part(ts.begin(), ts.begin() + (std::min)(size, ts.size()));
    std::wstring path = MakeFile(L"truncated.ts", part);
    Ptr<IFileSourceFilter> source;
    ASSERT_EQ(reader.As(source), S_OK);
    HRESULT hr = source->Load(path.c_str(), NULL);
    (void)hr;
  }
  SUCCEED();
}

TEST_F(TsReaderGraph, OnlyPsiAndNoMediaDoesNotCrashTheReader)
{
  std::vector<byte> ts = TsStreamBuilder::Build(0);
  std::wstring path = MakeFile(L"empty.ts", ts);
  HRESULT hr = LoadFile(path);
  (void)hr;
  SUCCEED();
}

TEST_F(TsReaderGraph, LoadingTwiceReplacesTheFile)
{
  std::wstring a = MakeFile(L"a.ts", TsStreamBuilder::Build(40));
  std::wstring b = MakeFile(L"b.ts", TsStreamBuilder::Build(40));
  ASSERT_EQ(LoadFile(a), S_OK);
  ASSERT_EQ(LoadFile(b), S_OK);

  Ptr<IFileSourceFilter> source;
  ASSERT_EQ(reader.As(source), S_OK);
  LPOLESTR name = NULL;
  ASSERT_EQ(source->GetCurFile(&name, NULL), S_OK);
  EXPECT_EQ(std::wstring(name), b);
  CoTaskMemFree(name);
}
