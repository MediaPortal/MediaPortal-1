// mpcSubs.dll (the MPC-HC subtitle engine of MediaPortal) driven through its exported C API, the way MpcEngine.cs does it:
// subtitle files next to a movie are found and listed, and the subtitles are drawn on a Direct3D 9 render target.
//
// The tests are skipped when mpcSubs.dll is not found (MP_MPCSUBS_DLL, or next to the tests).
#include <gtest/gtest.h>
#include <d3d9.h>
#include <dshow.h>
#include <string>
#include "FilterHost.h"
#include "LocalSupport.h"

namespace
{
  // --- the exported API (mpcSubs.h, cdecl as MpcEngine.cs declares it) --------------------------------

  struct SubtitleStyle
  {
    wchar_t* fontName;
    int fontColor;
    int fontIsBold;
    int fontSize;
    int fontCharset;
    int shadow;
    int borderWidth;
    int isBorderOutline;
  };

  struct MpcSubsApi
  {
    HMODULE module = nullptr;
    BOOL(__cdecl* SetDevice)(IDirect3DDevice9*) = nullptr;
    void(__cdecl* SetDefaultStyle)(const SubtitleStyle*, BOOL) = nullptr;
    void(__cdecl* SetAdvancedOptions)(int, SIZE, BOOL, BOOL) = nullptr;
    void(__cdecl* SetShowForcedOnly)(BOOL) = nullptr;
    BOOL(__cdecl* LoadSubtitles)(IDirect3DDevice9*, SIZE, const wchar_t*, IGraphBuilder*, const wchar_t*, LCID) = nullptr;
    void(__cdecl* SetTime)(REFERENCE_TIME) = nullptr;
    void(__cdecl* Render)(int, int, int, int, int) = nullptr;
    int(__cdecl* GetCount)() = nullptr;
    BSTR(__cdecl* GetLanguage)(int) = nullptr;
    BSTR(__cdecl* GetTrackName)(int) = nullptr;
    int(__cdecl* GetCurrent)() = nullptr;
    void(__cdecl* SetCurrent)(int) = nullptr;
    BOOL(__cdecl* GetEnable)() = nullptr;
    void(__cdecl* SetEnable)(BOOL) = nullptr;
    int(__cdecl* GetDelay)() = nullptr;
    void(__cdecl* SetDelay)(int) = nullptr;
    void(__cdecl* FreeSubtitles)() = nullptr;

    ~MpcSubsApi()
    {
      if (module)
      {
        if (FreeSubtitles)
          FreeSubtitles();
        FreeLibrary(module);
      }
    }

    // false (with a reason) when the DLL is not there or does not export what is expected
    bool Load(std::string& why)
    {
      wchar_t env[MAX_PATH] = {};
      std::wstring path = GetEnvironmentVariableW(L"MP_MPCSUBS_DLL", env, MAX_PATH) ? std::wstring(env) : FilterModule::PathOf(L"mpcSubs.dll");
      if (GetFileAttributesW(path.c_str()) == INVALID_FILE_ATTRIBUTES)
      {
        why = "mpcSubs.dll not found (" + ToUtf8(path) + ")";
        return false;
      }
      module = LoadLibraryExW(path.c_str(), NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
      if (!module)
      {
        why = "cannot load " + ToUtf8(path) + ", error " + std::to_string(GetLastError());
        return false;
      }
#define MP_BIND(name) name = reinterpret_cast<decltype(name)>(GetProcAddress(module, #name)); if (!name) { why = "mpcSubs.dll does not export " #name; return false; }
      MP_BIND(SetDevice) MP_BIND(SetDefaultStyle) MP_BIND(SetAdvancedOptions) MP_BIND(SetShowForcedOnly) MP_BIND(LoadSubtitles)
      MP_BIND(SetTime) MP_BIND(Render) MP_BIND(GetCount) MP_BIND(GetLanguage) MP_BIND(GetTrackName) MP_BIND(GetCurrent)
      MP_BIND(SetCurrent) MP_BIND(GetEnable) MP_BIND(SetEnable) MP_BIND(GetDelay) MP_BIND(SetDelay) MP_BIND(FreeSubtitles)
#undef MP_BIND
      return true;
    }
  };

  // --- a Direct3D 9 device with a render target we can read back ---------------------------------------

  class D3dHost
  {
  public:
    static const int Width = 640;
    static const int Height = 360;

    ~D3dHost()
    {
      if (m_target) m_target->Release();
      if (m_system) m_system->Release();
      if (m_device) m_device->Release();
      if (m_d3d) m_d3d->Release();
      if (m_window) DestroyWindow(m_window);
    }

    bool Create(std::string& why)
    {
      m_d3d = Direct3DCreate9(D3D_SDK_VERSION);
      if (!m_d3d)
      {
        why = "Direct3D 9 is not available";
        return false;
      }
      m_window = CreateWindowExW(0, L"STATIC", L"mp_local_tests", WS_POPUP, 0, 0, Width, Height, NULL, NULL, GetModuleHandleW(NULL), NULL);
      D3DPRESENT_PARAMETERS pp = {};
      pp.Windowed = TRUE;
      pp.SwapEffect = D3DSWAPEFFECT_DISCARD;
      pp.BackBufferFormat = D3DFMT_UNKNOWN;
      pp.hDeviceWindow = m_window;
      pp.BackBufferWidth = Width;
      pp.BackBufferHeight = Height;
      const DWORD flags = D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE;
      HRESULT hr = m_d3d->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, m_window, flags | D3DCREATE_HARDWARE_VERTEXPROCESSING, &pp, &m_device);
      if (FAILED(hr))
        hr = m_d3d->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, m_window, flags | D3DCREATE_SOFTWARE_VERTEXPROCESSING, &pp, &m_device);
      if (FAILED(hr))
      {
        why = "cannot create a Direct3D 9 HAL device, hr=" + std::to_string(static_cast<long>(hr));
        return false;
      }
      if (FAILED(m_device->CreateRenderTarget(Width, Height, D3DFMT_X8R8G8B8, D3DMULTISAMPLE_NONE, 0, FALSE, &m_target, NULL)) ||
          FAILED(m_device->CreateOffscreenPlainSurface(Width, Height, D3DFMT_X8R8G8B8, D3DPOOL_SYSTEMMEM, &m_system, NULL)))
      {
        why = "cannot create the render target";
        return false;
      }
      return true;
    }

    IDirect3DDevice9* Device() const { return m_device; }

    // Clears the target to black, lets 'draw' paint, and returns how many pixels are not black
    template <class F>
    int Draw(F draw)
    {
      m_device->SetRenderTarget(0, m_target);
      m_device->Clear(0, NULL, D3DCLEAR_TARGET, D3DCOLOR_XRGB(0, 0, 0), 1.0f, 0);
      if (FAILED(m_device->BeginScene()))
        return -1;
      draw();
      m_device->EndScene();
      if (FAILED(m_device->GetRenderTargetData(m_target, m_system)))
        return -1;
      D3DLOCKED_RECT lr;
      if (FAILED(m_system->LockRect(&lr, NULL, D3DLOCK_READONLY)))
        return -1;
      int lit = 0;
      for (int y = 0; y < Height; y++)
      {
        const DWORD* row = reinterpret_cast<const DWORD*>(static_cast<const BYTE*>(lr.pBits) + static_cast<size_t>(y) * lr.Pitch);
        for (int x = 0; x < Width; x++)
          if ((row[x] & 0x00FFFFFF) != 0)
            lit++;
      }
      m_system->UnlockRect();
      return lit;
    }

  private:
    HWND m_window = nullptr;
    IDirect3D9* m_d3d = nullptr;
    IDirect3DDevice9* m_device = nullptr;
    IDirect3DSurface9* m_target = nullptr;
    IDirect3DSurface9* m_system = nullptr;
  };

  const GUID kEnhancedVideoRenderer = { 0xfa10746c, 0x9b63, 0x4b6c, { 0xbc, 0x49, 0xfc, 0x30, 0x0e, 0xa5, 0xf2, 0x56 } };

  const char kSrt[] =
    "1\r\n00:00:01,000 --> 00:00:03,000\r\nHello MediaPortal\r\n\r\n"
    "2\r\n00:00:05,000 --> 00:00:07,000\r\nSecond subtitle line\r\n\r\n";

  const char kAss[] =
    "[Script Info]\r\nScriptType: v4.00+\r\nPlayResX: 640\r\nPlayResY: 360\r\n\r\n"
    "[V4+ Styles]\r\n"
    "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\r\n"
    "Style: Default,Arial,40,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,0,2,10,10,10,1\r\n\r\n"
    "[Events]\r\n"
    "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\r\n"
    "Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Hello ASS\r\n";

  const REFERENCE_TIME kSecond = 10000000;

  class MpcSubs : public testing::Test
  {
  protected:
    ComScope com;
    MpcSubsApi api;
    D3dHost d3d;
    Ptr<IGraphBuilder> graph;
    TempFolder temp;

    void SetUp() override
    {
      ASSERT_TRUE(SUCCEEDED(com.Result()));
      std::string why;
      if (!api.Load(why))
        GTEST_SKIP() << why;
      if (!d3d.Create(why))
        GTEST_SKIP() << why;
      ASSERT_EQ(CreateGraph(&graph), S_OK);
      if (!AddVideoRenderer())
        GTEST_SKIP() << "the Enhanced Video Renderer (evr.dll) is not available";

      // the options the client uses: no sub picture thread, so the subtitles are ready when Render is called
      api.SetAdvancedOptions(0, SIZE{ 800, 600 }, FALSE, TRUE);
      wchar_t font[] = L"Arial";
      SubtitleStyle style = { font, 0x00FFFFFF, 0, 28, 1, 0, 2, 1 };
      api.SetDefaultStyle(&style, FALSE);
      api.SetShowForcedOnly(FALSE);
    }

    // mpcSubs looks for the renderer by name to hook it; an unconnected one is enough for external subtitles
    bool AddVideoRenderer()
    {
      Ptr<IBaseFilter> evr;
      if (FAILED(CoCreateInstance(kEnhancedVideoRenderer, NULL, CLSCTX_INPROC_SERVER, IID_IBaseFilter, reinterpret_cast<void**>(&evr))))
        return false;
      return graph->AddFilter(evr.Get(), L"Enhanced Video Renderer") == S_OK;
    }

    // movie.avi (an empty file is enough, only the name is used) with the given subtitle files next to it
    std::wstring MakeMovie(const char* srt, const char* ass)
    {
      EXPECT_TRUE(temp.Write(L"movie.avi", ""));
      if (srt) EXPECT_TRUE(temp.Write(L"movie.srt", srt));
      if (ass) EXPECT_TRUE(temp.Write(L"movie.ass", ass));
      return temp.File(L"movie.avi");
    }

    BOOL Load(const std::wstring& movie)
    {
      return api.LoadSubtitles(d3d.Device(), SIZE{ D3dHost::Width, D3dHost::Height }, movie.c_str(), graph.Get(), temp.Path().c_str(), 0x0409);
    }

    // Subtitles at 'time' (100 ns units) drawn on the black render target; returns the number of lit pixels
    int LitPixelsAt(REFERENCE_TIME time)
    {
      api.SetTime(time);
      return d3d.Draw([&] { api.Render(0, 0, D3dHost::Width, D3dHost::Height, 0); });
    }

    static std::string Text(BSTR b)
    {
      if (!b)
        return "<null>";
      std::string s = ToUtf8(std::wstring(b, SysStringLen(b)));
      SysFreeString(b);
      return s;
    }
  };
}

// --- the parts of the test bench that do not need mpcSubs.dll (so they can be checked on any machine) --------

TEST(LocalEnvironment, Direct3DHostDrawsAndReadsBackPixels)
{
  D3dHost host;
  std::string why;
  if (!host.Create(why))
    GTEST_SKIP() << why;
  EXPECT_EQ(host.Draw([] {}), 0);
  EXPECT_EQ(host.Draw([&] { host.Device()->Clear(0, NULL, D3DCLEAR_TARGET, D3DCOLOR_XRGB(255, 255, 255), 1.0f, 0); }),
            D3dHost::Width * D3dHost::Height);
  const RECT part = { 10, 10, 20, 30 };
  EXPECT_EQ(host.Draw([&] { host.Device()->Clear(1, reinterpret_cast<const D3DRECT*>(&part), D3DCLEAR_TARGET, D3DCOLOR_XRGB(255, 0, 0), 1.0f, 0); }),
            10 * 20);
}

TEST(LocalEnvironment, VideoRendererIsFoundUnderTheNameMpcSubsLooksFor)
{
  ComScope com;
  Ptr<IGraphBuilder> graph;
  ASSERT_EQ(CreateGraph(&graph), S_OK);
  Ptr<IBaseFilter> evr;
  if (FAILED(CoCreateInstance(kEnhancedVideoRenderer, NULL, CLSCTX_INPROC_SERVER, IID_IBaseFilter, reinterpret_cast<void**>(&evr))))
    GTEST_SKIP() << "the Enhanced Video Renderer (evr.dll) is not available";
  ASSERT_EQ(graph->AddFilter(evr.Get(), L"Enhanced Video Renderer"), S_OK);
  Ptr<IBaseFilter> found;
  EXPECT_EQ(graph->FindFilterByName(L"Enhanced Video Renderer", &found), S_OK);
  EXPECT_EQ(found.Get(), evr.Get());
}

TEST(LocalEnvironment, TempFolderHoldsFilesAndIsRemoved)
{
  std::wstring path;
  {
    TempFolder temp;
    path = temp.Path();
    ASSERT_TRUE(temp.Write(L"a.srt", "text"));
    EXPECT_NE(GetFileAttributesW(temp.File(L"a.srt").c_str()), INVALID_FILE_ATTRIBUTES);
  }
  EXPECT_EQ(GetFileAttributesW(path.c_str()), INVALID_FILE_ATTRIBUTES);
}

TEST_F(MpcSubs, SubRipFileNextToTheMovieIsListed)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, nullptr)));
  ASSERT_EQ(api.GetCount(), 1);
  EXPECT_NE(Text(api.GetLanguage(0)), "<null>");
  EXPECT_NE(Text(api.GetTrackName(0)), "<null>");
  EXPECT_EQ(Text(api.GetLanguage(1)), "<null>");           // out of range
}

TEST_F(MpcSubs, SubStationAlphaFileNextToTheMovieIsListed)
{
  ASSERT_TRUE(Load(MakeMovie(nullptr, kAss)));
  EXPECT_EQ(api.GetCount(), 1);
}

TEST_F(MpcSubs, EveryFileIsAStreamOfItsOwn)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, kAss)));
  EXPECT_EQ(api.GetCount(), 2);
}

TEST_F(MpcSubs, NoSubtitleFilesMeansNoStreams)
{
  ASSERT_TRUE(Load(MakeMovie(nullptr, nullptr)));
  EXPECT_EQ(api.GetCount(), 0);
  EXPECT_EQ(api.GetCurrent(), -1);
}

TEST_F(MpcSubs, WithoutAVideoRendererNothingIsLoaded)
{
  // the engine hooks the renderer; without one it gives up
  graph.Reset();
  ASSERT_EQ(CreateGraph(&graph), S_OK);
  ASSERT_TRUE(Load(MakeMovie(kSrt, nullptr)));
  EXPECT_EQ(api.GetCount(), 0);
}

TEST_F(MpcSubs, SubRipTextIsDrawnOnlyWhileItIsDisplayed)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, nullptr)));
  ASSERT_EQ(api.GetCount(), 1);
  api.SetCurrent(0);
  api.SetEnable(TRUE);

  EXPECT_GT(LitPixelsAt(2 * kSecond), 50) << "first subtitle (1 s to 3 s)";
  EXPECT_EQ(LitPixelsAt(4 * kSecond), 0) << "gap between the subtitles";
  EXPECT_GT(LitPixelsAt(6 * kSecond), 50) << "second subtitle (5 s to 7 s)";
  EXPECT_EQ(LitPixelsAt(10 * kSecond), 0) << "after the last subtitle";
  EXPECT_EQ(LitPixelsAt(kSecond / 2), 0) << "before the first subtitle";
}

TEST_F(MpcSubs, SubStationAlphaTextIsDrawn)
{
  ASSERT_TRUE(Load(MakeMovie(nullptr, kAss)));
  ASSERT_EQ(api.GetCount(), 1);
  api.SetCurrent(0);
  api.SetEnable(TRUE);
  EXPECT_GT(LitPixelsAt(2 * kSecond), 50);
  EXPECT_EQ(LitPixelsAt(5 * kSecond), 0);
}

TEST_F(MpcSubs, DisabledSubtitlesDrawNothing)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, nullptr)));
  api.SetCurrent(0);
  api.SetEnable(FALSE);
  EXPECT_FALSE(api.GetEnable());
  EXPECT_EQ(LitPixelsAt(2 * kSecond), 0);
  api.SetEnable(TRUE);
  EXPECT_TRUE(api.GetEnable());
  EXPECT_GT(LitPixelsAt(2 * kSecond), 50);
}

TEST_F(MpcSubs, TheCurrentStreamCanBeSwitched)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, kAss)));
  ASSERT_EQ(api.GetCount(), 2);
  api.SetEnable(TRUE);
  api.SetCurrent(0);
  EXPECT_EQ(api.GetCurrent(), 0);
  api.SetCurrent(1);
  EXPECT_EQ(api.GetCurrent(), 1);
  EXPECT_GT(LitPixelsAt(2 * kSecond), 50);
}

TEST_F(MpcSubs, DelayIsStoredInMilliseconds)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, nullptr)));
  EXPECT_EQ(api.GetDelay(), 0);
  api.SetDelay(1500);
  EXPECT_EQ(api.GetDelay(), 1500);
  api.SetDelay(-250);
  EXPECT_EQ(api.GetDelay(), -250);
}

TEST_F(MpcSubs, LoadingAnotherMovieReplacesTheStreams)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, kAss)));
  ASSERT_EQ(api.GetCount(), 2);
  TempFolder other;
  ASSERT_TRUE(other.Write(L"other.avi", ""));
  ASSERT_TRUE(other.Write(L"other.srt", kSrt));
  ASSERT_TRUE(api.LoadSubtitles(d3d.Device(), SIZE{ D3dHost::Width, D3dHost::Height }, other.File(L"other.avi").c_str(), graph.Get(),
                                other.Path().c_str(), 0x0409));
  EXPECT_EQ(api.GetCount(), 1);
}

TEST_F(MpcSubs, FreeingTheSubtitlesEmptiesTheList)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, nullptr)));
  ASSERT_EQ(api.GetCount(), 1);
  api.FreeSubtitles();
  EXPECT_EQ(api.GetCount(), 0);
  EXPECT_EQ(api.GetCurrent(), -1);
  api.FreeSubtitles();                                       // a second call is harmless
  api.Render(0, 0, D3dHost::Width, D3dHost::Height, 0);      // so is drawing without subtitles
}

TEST_F(MpcSubs, SubtitlesSurviveADeviceChange)
{
  ASSERT_TRUE(Load(MakeMovie(kSrt, nullptr)));
  api.SetCurrent(0);
  api.SetEnable(TRUE);
  EXPECT_TRUE(api.SetDevice(d3d.Device()));
  EXPECT_GT(LitPixelsAt(2 * kSecond), 50);
}

TEST_F(MpcSubs, BrokenSubtitleFilesDoNotCrashTheEngine)
{
  EXPECT_TRUE(temp.Write(L"movie.avi", ""));
  EXPECT_TRUE(temp.Write(L"movie.srt", std::string("\xFF\xFE\x00\x01 not a subtitle \x00\x00 1\r\n99:99:99,999 --> -1\r\n", 40)));
  EXPECT_TRUE(temp.Write(L"movie.ass", std::string(2000, '\x7F')));
  EXPECT_TRUE(temp.Write(L"movie.ssa", ""));
  ASSERT_TRUE(Load(temp.File(L"movie.avi")));
  api.SetEnable(TRUE);
  for (int i = 0; i < api.GetCount(); i++)
  {
    api.SetCurrent(i);
    LitPixelsAt(2 * kSecond);
  }
  SUCCEED();
}

// --- subtitles inside a media file (needs a DirectShow splitter that exposes them through IAMStreamSelect) ----

TEST_F(MpcSubs, EmbeddedSubRipStreamsOfAMatroskaFile)
{
  REQUIRE_SAMPLE(movie, L"HD Audio\\zx.eva.renewal.01.divx511.mkv");       // two SubRip tracks: eng, ger
  HRESULT hr = graph->RenderFile(movie.c_str(), nullptr);
  if (FAILED(hr))
    GTEST_SKIP() << "no DirectShow filters for Matroska on this machine (RenderFile hr=" << std::hex << hr << ")";
  ASSERT_TRUE(api.LoadSubtitles(d3d.Device(), SIZE{ D3dHost::Width, D3dHost::Height }, movie.c_str(), graph.Get(), L"", 0x0409));
  if (api.GetCount() == 0)
    GTEST_SKIP() << "the splitter does not expose the subtitle tracks";
  EXPECT_GE(api.GetCount(), 2);
  for (int i = 0; i < api.GetCount(); i++)
    EXPECT_NE(Text(api.GetLanguage(i)), "<null>") << "track " << i;
}

TEST_F(MpcSubs, EmbeddedSubStationAlphaStreamOfAMatroskaFile)
{
  REQUIRE_SAMPLE(movie, L"HD\\[Kametsu] Princess Principal Picture Drama - 01 (BD 1080p Hi10 FLAC) [BBF1B4CE].mkv");
  HRESULT hr = graph->RenderFile(movie.c_str(), nullptr);
  if (FAILED(hr))
    GTEST_SKIP() << "no DirectShow filters for this file on this machine (RenderFile hr=" << std::hex << hr << ")";
  ASSERT_TRUE(api.LoadSubtitles(d3d.Device(), SIZE{ D3dHost::Width, D3dHost::Height }, movie.c_str(), graph.Get(), L"", 0x0409));
  if (api.GetCount() == 0)
    GTEST_SKIP() << "the splitter does not expose the subtitle track";
  EXPECT_GE(api.GetCount(), 1);
}
