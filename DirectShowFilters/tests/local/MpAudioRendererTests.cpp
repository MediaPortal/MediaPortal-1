// MPAudioRenderer in a graph, behind a source that delivers generated PCM audio. The renderer opens the default Windows
// audio endpoint (WASAPI), so these tests need an audio device; they play silence only.
#include <gtest/gtest.h>
#include <chrono>
#include <streams.h>
#include <crtdbg.h>
#include "../../MPAudioRenderer/source/IMPAudioSettings.h"
#include "AudioSupport.h"
#include "FilterHost.h"
#include "LocalSupport.h"
#include "TestSourceFilter.h"

namespace
{
  const GUID kMpAudioRenderer = { 0xEC9ED6FC, 0x7B03, 0x4CB6, { 0x8C, 0x01, 0x4E, 0xAB, 0xE1, 0x09, 0xF2, 0x6B } };

  class MpAudioRenderer : public testing::Test
  {
  protected:
    ComScope com;
    FilterModule module;
    Ptr<IGraphBuilder> graph;
    CTestSourceFilter* source = nullptr;
    Ptr<IBaseFilter> renderer;
    Ptr<IPin> input;

    void SetUp() override
    {
      ASSERT_TRUE(SUCCEEDED(com.Result()));
      if (!HasDefaultAudioRenderDevice())
        GTEST_SKIP() << "no default audio render device";
      ASSERT_TRUE(module.Load(L"mpaudiorenderer.ax")) << ToUtf8(module.Error());
      Build();
    }

    // A new graph with a new renderer (what a player does for every playback)
    void Build()
    {
      ASSERT_EQ(CreateGraph(&graph), S_OK);

      HRESULT hr = S_OK;
      source = new CTestSourceFilter(&hr);
      source->AddRef();
      ASSERT_EQ(hr, S_OK);
      ASSERT_EQ(graph->AddFilter(source, L"TestSource"), S_OK);
      ASSERT_EQ(module.CreateFilter(kMpAudioRenderer, &renderer), S_OK);
      ASSERT_EQ(graph->AddFilter(renderer.Get(), L"MediaPortal Audio Renderer"), S_OK);

      Ptr<IEnumPins> pins;
      ASSERT_EQ(renderer->EnumPins(&pins), S_OK);
      ASSERT_EQ(pins->Next(1, &input, nullptr), S_OK);
    }

    void Destroy()
    {
      if (graph)
      {
        Ptr<IMediaControl> mc;
        if (SUCCEEDED(graph.As(mc)))
          mc->Stop();
      }
      input.Reset();
      if (graph && renderer) graph->RemoveFilter(renderer.Get());
      if (graph && source) graph->RemoveFilter(source);
      renderer.Reset();
      if (source) { source->Release(); source = nullptr; }
      graph.Reset();
    }

    void TearDown() override { Destroy(); }

    HRESULT Connect(const PcmFormat& format)
    {
      source->Pin()->SetMediaType(format.MediaType());
      return graph->ConnectDirect(source->Pin(), input.Get(), nullptr);
    }

    void Run()
    {
      Ptr<IMediaControl> mc;
      ASSERT_EQ(graph.As(mc), S_OK);
      HRESULT hr = mc->Run();
      ASSERT_TRUE(hr == S_OK || hr == S_FALSE) << std::hex << hr;
    }

    // 'seconds' of silence in 10 ms samples with stream times from 'start'; blocks as long as the renderer holds the samples
    HRESULT PlaySilence(const PcmFormat& format, double seconds, REFERENCE_TIME start = 0)
    {
      const size_t frames = format.rate / 100;
      const std::vector<uint8_t> chunk = format.Sine(1000, 0.0, frames);
      REFERENCE_TIME t = start;
      for (int i = 0; i < static_cast<int>(seconds * 100); i++)
      {
        const REFERENCE_TIME next = start + static_cast<REFERENCE_TIME>(i + 1) * 100000;
        HRESULT hr = source->Pin()->DeliverTimed(chunk.data(), chunk.size(), t, next, i == 0);
        if (hr != S_OK)
          return hr;
        t = next;
      }
      return S_OK;
    }
  };

  double SecondsSince(std::chrono::steady_clock::time_point t)
  {
    return std::chrono::duration<double>(std::chrono::steady_clock::now() - t).count();
  }
}

TEST_F(MpAudioRenderer, ConnectsToStereoPcm)
{
  const PcmFormat f;
  ASSERT_EQ(Connect(f), S_OK);
  Ptr<IPin> connected;
  EXPECT_EQ(input->ConnectedTo(&connected), S_OK);
}

TEST_F(MpAudioRenderer, AcceptsTheCommonPcmFormats)
{
  struct Case { int rate, channels, bits; bool isFloat; };
  const Case cases[] = {
    { 48000, 2, 16, false }, { 44100, 2, 16, false }, { 96000, 2, 16, false }, { 48000, 2, 24, false }, { 48000, 2, 32, false },
    { 48000, 2, 32, true }, { 48000, 1, 16, false }, { 48000, 6, 16, false }, { 48000, 6, 24, false }, { 48000, 8, 24, false },
    { 192000, 2, 24, false }, { 22050, 2, 16, false },
  };
  for (const Case& c : cases)
  {
    PcmFormat f;
    f.rate = c.rate; f.channels = c.channels; f.bits = c.bits; f.isFloat = c.isFloat;
    SCOPED_TRACE(testing::Message() << c.rate << " Hz, " << c.channels << " ch, " << c.bits << (c.isFloat ? " bit float" : " bit"));
    Destroy();                                  // a new renderer for every format, see the disabled test below
    Build();
    HRESULT hr = Connect(f);
    EXPECT_EQ(hr, S_OK) << std::hex << hr;
  }
}

// A renderer that is connected, disconnected and connected again with another format used to corrupt the heap: the render
// thread kept writing into the buffer of IAudioRenderClient::GetBuffer after the format change had released the audio client
// (found with AddressSanitizer, heap-use-after-free WRITE; the Debug build died with 0xC0000374). The thread now notices that
// the audio client was replaced and takes a new buffer. Run it under AddressSanitizer to see the difference.
TEST_F(MpAudioRenderer, ReconnectingWithAnotherFormatDoesNotCorruptTheHeap)
{
  for (int rate : { 48000, 44100, 96000, 48000, 22050, 192000 })
  {
    PcmFormat f;
    f.rate = rate;
    ASSERT_EQ(Connect(f), S_OK) << rate;
    EXPECT_EQ(graph->Disconnect(input.Get()), S_OK);
    graph->Disconnect(source->Pin());
  }
  Sleep(500);                                  // time for the render thread to touch the freed buffer
}

// KNOWN DEFECT (disabled): CopyWaveFormatEx copied sizeof(WAVEFORMATEXTENSIBLE) + cbSize = 62 bytes from a format of 40 bytes.
// It is fixed (dllmain.cpp); this test passes a format block of exactly 40 bytes, which only AddressSanitizer or a guard page
// can tell from a padded one, so it is enabled only for that kind of run.
TEST_F(MpAudioRenderer, DISABLED_AFormatBlockOfExactlyTheRightSizeIsNeverReadBeyondItsEnd)
{
  PcmFormat f;
  CMediaType mt = f.MediaType();
  std::vector<BYTE> exact(mt.Format(), mt.Format() + sizeof(WAVEFORMATEXTENSIBLE));
  mt.SetFormat(exact.data(), static_cast<ULONG>(exact.size()));   // no padding behind the structure
  source->Pin()->SetMediaType(mt);
  EXPECT_EQ(graph->ConnectDirect(source->Pin(), input.Get(), nullptr), S_OK);
}

TEST_F(MpAudioRenderer, RefusesAStreamThatIsNotAudio)
{
  CMediaType video;
  video.SetType(&MEDIATYPE_Video);
  video.SetSubtype(&MEDIASUBTYPE_RGB32);
  source->Pin()->SetMediaType(video);
  EXPECT_NE(graph->ConnectDirect(source->Pin(), input.Get(), nullptr), S_OK);
}

TEST_F(MpAudioRenderer, PlaysSilenceInRealTimeAndCompletes)
{
  const PcmFormat f;
  ASSERT_EQ(Connect(f), S_OK);
  Run();

  const auto begin = std::chrono::steady_clock::now();
  ASSERT_EQ(PlaySilence(f, 1.0, 5000000), S_OK);            // 1 s of audio, starting 0.5 s after the start of the stream
  ASSERT_EQ(source->Pin()->DeliverEndOfStream(), S_OK);

  Ptr<IMediaEvent> events;
  ASSERT_EQ(graph.As(events), S_OK);
  long code = 0;
  EXPECT_EQ(events->WaitForCompletion(5000, &code), S_OK) << "EC_COMPLETE did not arrive";
  EXPECT_EQ(code, EC_COMPLETE);
  const double elapsed = SecondsSince(begin);
  EXPECT_GT(elapsed, 0.9) << "the renderer did not wait for the sample times";
  EXPECT_LT(elapsed, 4.0);
}

TEST_F(MpAudioRenderer, PlaysFloatAndSurroundWithoutErrors)
{
  struct Case { int channels, bits; bool isFloat; };
  for (const Case& c : { Case{ 2, 32, true }, Case{ 6, 16, false }, Case{ 2, 24, false } })
  {
    PcmFormat f;
    f.channels = c.channels; f.bits = c.bits; f.isFloat = c.isFloat;
    SCOPED_TRACE(testing::Message() << c.channels << " ch " << c.bits << (c.isFloat ? " float" : " bit"));
    ASSERT_EQ(Connect(f), S_OK);
    Run();
    ASSERT_EQ(PlaySilence(f, 0.3, 2000000), S_OK);
    Ptr<IMediaControl> mc;
    ASSERT_EQ(graph.As(mc), S_OK);
    ASSERT_TRUE(SUCCEEDED(mc->Stop()));
    graph->Disconnect(input.Get());
    graph->Disconnect(source->Pin());
  }
}

TEST_F(MpAudioRenderer, StopsPromptlyWhileSamplesAreWaiting)
{
  const PcmFormat f;
  ASSERT_EQ(Connect(f), S_OK);
  Run();
  ASSERT_EQ(PlaySilence(f, 0.1, 0), S_OK);

  Ptr<IMediaControl> mc;
  ASSERT_EQ(graph.As(mc), S_OK);
  const auto begin = std::chrono::steady_clock::now();
  EXPECT_TRUE(SUCCEEDED(mc->Stop()));
  EXPECT_LT(SecondsSince(begin), 3.0);
}

TEST_F(MpAudioRenderer, KeepsPlayingAfterAFlush)
{
  const PcmFormat f;
  ASSERT_EQ(Connect(f), S_OK);
  Run();
  ASSERT_EQ(PlaySilence(f, 0.2, 0), S_OK);
  ASSERT_EQ(source->Pin()->FlushDownstream(), S_OK);
  EXPECT_EQ(PlaySilence(f, 0.2, 0), S_OK);
}

TEST_F(MpAudioRenderer, VolumeAndBalanceAreKept)
{
  const PcmFormat f;
  ASSERT_EQ(Connect(f), S_OK);
  Ptr<IBasicAudio> audio;
  ASSERT_EQ(renderer.As(audio), S_OK);

  long volume = 12345;
  ASSERT_EQ(audio->get_Volume(&volume), S_OK);
  EXPECT_LE(volume, 0);
  EXPECT_GE(volume, -10000);

  ASSERT_EQ(audio->put_Volume(-2500), S_OK);
  ASSERT_EQ(audio->get_Volume(&volume), S_OK);
  EXPECT_EQ(volume, -2500);
  ASSERT_EQ(audio->put_Volume(volume = 0), S_OK);

  long balance = 0;
  EXPECT_EQ(audio->get_Balance(&balance), S_OK);
  EXPECT_EQ(balance, 0);
}

TEST_F(MpAudioRenderer, SeekingIsPassedOnToTheUpstreamFilter)
{
  // IMediaSeeking of the renderer is a pass-through (CPosPassThru) to the filter in front of it. The test source has no
  // IMediaSeeking, so the renderer has nothing to ask and says so, without crashing.
  const PcmFormat f;
  ASSERT_EQ(Connect(f), S_OK);
  Ptr<IMediaSeeking> seeking;
  ASSERT_EQ(renderer.As(seeking), S_OK);
  DWORD capabilities = 0;
  EXPECT_EQ(seeking->GetCapabilities(&capabilities), E_NOTIMPL);
  GUID format = {};
  EXPECT_EQ(seeking->GetTimeFormat(&format), E_NOTIMPL);
  LONGLONG position = -1;
  EXPECT_EQ(seeking->GetCurrentPosition(&position), E_NOTIMPL);
}

namespace
{
  struct ConfigPtr
  {
    IMPAudioRendererConfig* p = nullptr;
    ~ConfigPtr() { if (p) p->Release(); }
  };
}

TEST_F(MpAudioRenderer, SettingsCanBeRead)
{
  ConfigPtr config;
  ASSERT_EQ(renderer->QueryInterface(__uuidof(IMPAudioRendererConfig), reinterpret_cast<void**>(&config.p)), S_OK);

  int mode = -1;
  ASSERT_EQ(config.p->GetInt(WASAPI_MODE, &mode), S_OK);
  EXPECT_TRUE(mode == SHARED || mode == EXCLUSIVE) << mode;

  int rate = -1;
  EXPECT_EQ(config.p->GetInt(SAMPLE_RATE, &rate), S_OK);
  EXPECT_TRUE(rate == 0 || (rate >= 8000 && rate <= 384000)) << "0 means that the rate of the stream is kept: " << rate;

  int depth = -1;
  EXPECT_EQ(config.p->GetInt(BIT_DEPTH, &depth), S_OK);
  EXPECT_TRUE(depth == 0 || depth == 8 || depth == 16 || depth == 24 || depth == 32) << depth;

  bool flag = false;
  EXPECT_EQ(config.p->GetBool(ENABLE_SYNC_ADJUSTMENT, &flag), S_OK);
  EXPECT_EQ(config.p->GetBool(ENABLE_TIME_STRETCHING, &flag), S_OK);

  // a setting of the wrong kind or an unknown one is refused
  EXPECT_NE(config.p->GetInt(static_cast<MPARSetting>(999), &mode), S_OK);
  EXPECT_NE(config.p->GetBool(WASAPI_MODE, &flag), S_OK);
}

TEST_F(MpAudioRenderer, ResamplingQualityIsAcceptedAndKept)
{
  // SetInt(LIB_RESAMPLE_QUALITY) used to run on into the USE_FILTERS check and refused every valid value
  ConfigPtr config;
  ASSERT_EQ(renderer->QueryInterface(__uuidof(IMPAudioRendererConfig), reinterpret_cast<void**>(&config.p)), S_OK);
  for (int quality : { 0, 1, 2, 3, 4 })
  {
    EXPECT_EQ(config.p->SetInt(LIB_RESAMPLE_QUALITY, quality), S_OK) << quality;
    int value = -1;
    EXPECT_EQ(config.p->GetInt(LIB_RESAMPLE_QUALITY, &value), S_OK);
    EXPECT_EQ(value, quality);
  }
  EXPECT_EQ(config.p->SetInt(LIB_RESAMPLE_QUALITY, 5), E_INVALIDARG);
}

TEST_F(MpAudioRenderer, SettingsOutOfRangeAreRefusedAndTheOldValueStays)
{
  ConfigPtr config;
  ASSERT_EQ(renderer->QueryInterface(__uuidof(IMPAudioRendererConfig), reinterpret_cast<void**>(&config.p)), S_OK);
  int before = -1, after = -1;
  ASSERT_EQ(config.p->GetInt(SAMPLE_RATE, &before), S_OK);
  EXPECT_EQ(config.p->SetInt(SAMPLE_RATE, 12345), E_INVALIDARG);
  ASSERT_EQ(config.p->GetInt(SAMPLE_RATE, &after), S_OK);
  EXPECT_EQ(after, before);
  EXPECT_EQ(config.p->SetInt(WASAPI_MODE, 7), E_INVALIDARG);
}

// SETTING_AUDIO_DEVICE is the id of the end point (what the IMMDevice::GetId gives); empty means the default device.
TEST_F(MpAudioRenderer, AudioDeviceIsTheDefaultOneUntilSet)
{
  ConfigPtr config;
  ASSERT_EQ(renderer->QueryInterface(__uuidof(IMPAudioRendererConfig), reinterpret_cast<void**>(&config.p)), S_OK);
  ASSERT_EQ(config.p->SetString(SETTING_AUDIO_DEVICE, const_cast<LPWSTR>(L"")), S_OK);
  LPWSTR device = nullptr;
  ASSERT_EQ(config.p->GetString(SETTING_AUDIO_DEVICE, &device), S_OK);
  ASSERT_NE(device, nullptr);
  EXPECT_STREQ(device, L"");                                     // empty: the default audio device
  CoTaskMemFree(device);
}

TEST_F(MpAudioRenderer, AudioDeviceIdComesBackAsItWasSet)
{
  ConfigPtr config;
  ASSERT_EQ(renderer->QueryInterface(__uuidof(IMPAudioRendererConfig), reinterpret_cast<void**>(&config.p)), S_OK);

  // the id of the default end point, the form SetString() takes
  IMMDeviceEnumerator* enumerator = nullptr;
  ASSERT_EQ(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_ALL, __uuidof(IMMDeviceEnumerator), reinterpret_cast<void**>(&enumerator)), S_OK);
  IMMDevice* endpoint = nullptr;
  ASSERT_EQ(enumerator->GetDefaultAudioEndpoint(eRender, eConsole, &endpoint), S_OK);
  LPWSTR id = nullptr;
  ASSERT_EQ(endpoint->GetId(&id), S_OK);
  endpoint->Release();
  enumerator->Release();

  EXPECT_EQ(config.p->SetString(SETTING_AUDIO_DEVICE, id), S_OK);
  LPWSTR device = nullptr;
  ASSERT_EQ(config.p->GetString(SETTING_AUDIO_DEVICE, &device), S_OK);
  ASSERT_NE(device, nullptr);
  EXPECT_STREQ(device, id);
  CoTaskMemFree(device);

  // an id that is not an end point leaves the setting alone
  EXPECT_EQ(config.p->SetString(SETTING_AUDIO_DEVICE, const_cast<LPWSTR>(L"{no-such-end-point}")), S_OK);
  device = nullptr;
  ASSERT_EQ(config.p->GetString(SETTING_AUDIO_DEVICE, &device), S_OK);
  EXPECT_STREQ(device, id);
  CoTaskMemFree(device);
  CoTaskMemFree(id);
}

TEST_F(MpAudioRenderer, GetStringRefusesWhatItDoesNotKnow)
{
  ConfigPtr config;
  ASSERT_EQ(renderer->QueryInterface(__uuidof(IMPAudioRendererConfig), reinterpret_cast<void**>(&config.p)), S_OK);
  LPWSTR text = reinterpret_cast<LPWSTR>(0x1);
  EXPECT_EQ(config.p->GetString(AC3_ENCODING, &text), E_NOTIMPL);
  EXPECT_EQ(text, nullptr) << "no pointer is handed out on failure";
  EXPECT_EQ(config.p->GetString(SETTING_AUDIO_DEVICE, nullptr), E_POINTER);
}

TEST_F(MpAudioRenderer, NoAssertWhenAConnectionIsRefusedAndTheFilterIsDestroyed)
{
  const int before = AssertsCaught();
  CMediaType video;
  video.SetType(&MEDIATYPE_Video);
  video.SetSubtype(&MEDIASUBTYPE_RGB32);
  source->Pin()->SetMediaType(video);
  EXPECT_NE(graph->ConnectDirect(source->Pin(), input.Get(), nullptr), S_OK);
  Destroy();                                                     // the processing thread of the time stretch filter stops without ever having a stream
  Sleep(300);                                                    // the watcher closes an ASSERT box within 30 ms
  EXPECT_EQ(AssertsCaught(), before) << "CTimeStretchFilter::clear() asserted";
}
