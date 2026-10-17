// BDReader (Blu-ray source filter) with a real disc structure. The filter loads bluray.dll (libbluray, 32 bit only) from the
// folder of BDReader.ax, so these tests run in the x86 build; elsewhere they skip. The sample is a BD folder:
// <samples>\BD\BDMV\index.bdmv.
#include <gtest/gtest.h>
#include <streams.h>
#include "FilterHost.h"
#include "LocalSupport.h"

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

TEST_F(BdReader, LoadsTheSampleDisc)
{
  if (!HasLibbluray())
    GTEST_SKIP() << "bluray.dll (32 bit libbluray) is not next to BDReader.ax; run the x86 build";
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
