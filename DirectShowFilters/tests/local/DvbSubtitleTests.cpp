// DVBSub3 with a real DVB subtitle stream: the TS packets of the subtitle PID of a broadcast recording are delivered to the
// filter through a real DirectShow graph (the test source stands in for TsReader), and the bitmaps come back through the
// callback that MediaPortal's managed code uses.
#include <gtest/gtest.h>
#include <random>
#include <streams.h>
#include "IDVBSub.h"
#include "PacketSync.h"
#include "FilterHost.h"
#include "LocalSupport.h"
#include "TestSourceFilter.h"
#include "TsSubtitleExtractor.h"

namespace
{
  const wchar_t* kDvbSample = L"HD Audio\\12-10_19-18-52_BBC HD_Wild China.ts";

  struct Captured
  {
    LONG width, height, widthBytes;
    WORD bitsPerPixel;
    LONG screenWidth, screenHeight;
    uint64_t timestamp, timeOut;
    int firstScanLine, horizontalPosition;
    std::vector<byte> bits;
  };

  std::vector<Captured>* g_captured = nullptr;
  int g_resets = 0;

  int CALLBACK OnSubtitle(SUBTITLE* s)
  {
    if (!g_captured)
      return 0;
    Captured c = {};
    c.width = s->bmWidth;
    c.height = s->bmHeight;
    c.widthBytes = s->bmWidthBytes;
    c.bitsPerPixel = s->bmBitsPixel;
    c.screenWidth = s->screenWidth;
    c.screenHeight = s->screenHeight;
    c.timestamp = s->timestamp;
    c.timeOut = s->timeOut;
    c.firstScanLine = s->firstScanLine;
    c.horizontalPosition = s->horizontalPosition;
    if (s->bmBits && s->bmWidthBytes > 0 && s->bmHeight > 0 && s->bmWidthBytes * s->bmHeight < (64 << 20))
    {
      const byte* b = static_cast<const byte*>(s->bmBits);
      c.bits.assign(b, b + static_cast<size_t>(s->bmWidthBytes) * s->bmHeight);   // the filter frees the bitmap after the callback
    }
    g_captured->push_back(std::move(c));
    return 0;
  }

  int CALLBACK OnReset()
  {
    g_resets++;
    return 0;
  }

  // The subtitle packets of the sample, read once for all tests
  const SubtitleStream* Stream(std::string& problem)
  {
    static SubtitleStream stream;
    static std::string error;
    static bool done = false;
    if (!done)
    {
      done = true;
      std::wstring path = SamplePath(kDvbSample);
      if (path.empty())
        error = "sample not found";
      else
        ExtractDvbSubtitles(path, stream, error);
    }
    problem = error;
    return error.empty() ? &stream : nullptr;
  }

  class DvbSub3Graph : public testing::Test
  {
  protected:
    ComScope com;
    FilterModule module;
    Ptr<IGraphBuilder> graph;
    CTestSourceFilter* source = nullptr;
    Ptr<IBaseFilter> dvb;
    IDVBSubtitle* control = nullptr;
    IDVBSubtitleSource* observer = nullptr;
    std::vector<Captured> captured;
    const SubtitleStream* stream = nullptr;

    void SetUp() override
    {
      REQUIRE_SAMPLE(sample, kDvbSample);
      (void)sample;
      std::string problem;
      stream = Stream(problem);
      ASSERT_NE(stream, nullptr) << "no DVB subtitles in the sample: " << problem;

      ASSERT_TRUE(SUCCEEDED(com.Result()));
      ASSERT_TRUE(module.Load(L"DVBSub3.ax")) << ToUtf8(module.Error());
      ASSERT_EQ(CreateGraph(&graph), S_OK);

      HRESULT hr = S_OK;
      source = new CTestSourceFilter(&hr);
      source->AddRef();
      ASSERT_EQ(hr, S_OK);
      ASSERT_EQ(graph->AddFilter(source, L"TestSource"), S_OK);
      ASSERT_EQ(module.CreateFilter(CLSID_DVBSub3, &dvb), S_OK);
      ASSERT_EQ(graph->AddFilter(dvb.Get(), L"DVBSub3"), S_OK);

      Ptr<IPin> in;
      ASSERT_EQ(FirstPin(dvb.Get(), PINDIR_INPUT, &in), S_OK);
      ASSERT_EQ(graph->ConnectDirect(source->Pin(), in.Get(), nullptr), S_OK);

      ASSERT_EQ(dvb->QueryInterface(IID_IDVBSubtitle3, reinterpret_cast<void**>(&control)), S_OK);
      ASSERT_EQ(dvb->QueryInterface(IID_IDVBSubtitleSource, reinterpret_cast<void**>(&observer)), S_OK);

      g_captured = &captured;
      g_resets = 0;
      ASSERT_EQ(observer->SetBitmapCallback(OnSubtitle), S_OK);
      ASSERT_EQ(observer->SetResetCallback(OnReset), S_OK);
    }

    void TearDown() override
    {
      g_captured = nullptr;
      if (graph)
      {
        Ptr<IMediaControl> mc;
        if (SUCCEEDED(graph.As(mc)))
          mc->Stop();
      }
      if (observer) { observer->SetBitmapCallback(nullptr); observer->SetResetCallback(nullptr); observer->Release(); }
      if (control) control->Release();
      if (graph && dvb) graph->RemoveFilter(dvb.Get());
      if (graph && source) graph->RemoveFilter(source);
      dvb.Reset();
      if (source) { source->Release(); source = nullptr; }
      graph.Reset();
    }

    static HRESULT FirstPin(IBaseFilter* f, PIN_DIRECTION dir, IPin** pin)
    {
      Ptr<IEnumPins> e;
      HRESULT hr = f->EnumPins(&e);
      if (FAILED(hr))
        return hr;
      while (e->Next(1, pin, nullptr) == S_OK)
      {
        PIN_DIRECTION d;
        if (SUCCEEDED((*pin)->QueryDirection(&d)) && d == dir)
          return S_OK;
        (*pin)->Release();
        *pin = nullptr;
      }
      return E_FAIL;
    }

    // 90 kHz ticks: a second before the first subtitle, so every subtitle has a positive time stamp
    void Configure(int pid)
    {
      ASSERT_EQ(control->SetSubtitlePid(pid), S_OK);
      ASSERT_EQ(control->SetFirstPcr(static_cast<LONGLONG>(stream->firstPts) - 90000), S_OK);
    }

    void Run()
    {
      Ptr<IMediaControl> mc;
      ASSERT_EQ(graph.As(mc), S_OK);
      HRESULT hr = mc->Run();
      ASSERT_TRUE(hr == S_OK || hr == S_FALSE) << std::hex << hr;
      OAFilterState state;
      ASSERT_TRUE(SUCCEEDED(mc->GetState(5000, &state)));
      ASSERT_EQ(state, State_Running);
    }

    HRESULT Feed(size_t sampleSize)
    {
      return source->Pin()->DeliverBytes(stream->packets.data(), stream->packets.size(), sampleSize);
    }
  };

  bool HasVisiblePixels(const Captured& c)
  {
    for (byte b : c.bits)
      if (b != 0)
        return true;
    return false;
  }
}

TEST(DvbSampleStream, HasASubtitleStreamWithPackets)
{
  REQUIRE_SAMPLE(sample, kDvbSample);
  (void)sample;
  std::string problem;
  const SubtitleStream* s = Stream(problem);
  ASSERT_NE(s, nullptr) << problem;
  EXPECT_EQ(s->pid, 0x69);
  EXPECT_EQ(s->language, "eng");
  EXPECT_TRUE(s->hasPts);
  EXPECT_GT(s->pesPackets, 20u);
  EXPECT_EQ(s->packets.size() % 188, 0u);
}

namespace
{
  class CountingSync : public CPacketSync
  {
  public:
    using CPacketSync::OnTsPacket;
    void OnTsPacket(byte* packet) override
    {
      count++;
      if (packet[0] != TS_PACKET_SYNC)
        broken++;
    }
    size_t count = 0, broken = 0;
  };
}

TEST(DvbSampleStream, PacketSyncDeliversEveryPacketWhateverTheSampleSize)
{
  REQUIRE_SAMPLE(sample, kDvbSample);
  (void)sample;
  std::string problem;
  const SubtitleStream* s = Stream(problem);
  ASSERT_NE(s, nullptr) << problem;
  const size_t total = s->packets.size() / 188;

  for (size_t chunk : { size_t(188), size_t(1000), size_t(13), size_t(7), size_t(1) })
  {
    CountingSync sync;
    std::vector<byte> copy = s->packets;                    // OnRawData may write into the buffer
    for (size_t pos = 0; pos < copy.size(); pos += chunk)
      sync.OnRawData(&copy[pos], static_cast<int>((std::min)(chunk, copy.size() - pos)));
    EXPECT_EQ(sync.broken, 0u) << "chunk " << chunk;
    EXPECT_GE(sync.count + 1, total) << "chunk " << chunk << ": " << sync.count << " of " << total << " packets";
    EXPECT_LE(sync.count, total) << "chunk " << chunk;
  }
}

TEST_F(DvbSub3Graph, FilterConnectsToTheTransportStreamSource)
{
  Ptr<IPin> in;
  ASSERT_EQ(FirstPin(dvb.Get(), PINDIR_INPUT, &in), S_OK);
  Ptr<IPin> connected;
  ASSERT_EQ(in->ConnectedTo(&connected), S_OK);
  AM_MEDIA_TYPE mt = {};
  ASSERT_EQ(in->ConnectionMediaType(&mt), S_OK);
  EXPECT_TRUE(IsEqualGUID(mt.majortype, MEDIATYPE_Stream));
  EXPECT_TRUE(IsEqualGUID(mt.subtype, MEDIASUBTYPE_MPEG2_TRANSPORT));
  FreeMediaType(mt);
}

TEST_F(DvbSub3Graph, DecodesSubtitleBitmapsFromARealBroadcast)
{
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);

  ASSERT_GE(captured.size(), 5u) << "decoded subtitles: " << captured.size();
  bool anyVisible = false;
  for (size_t i = 0; i < captured.size(); i++)
  {
    const Captured& c = captured[i];
    SCOPED_TRACE(testing::Message() << "subtitle " << i);
    EXPECT_GT(c.width, 0);
    EXPECT_GT(c.height, 0);
    EXPECT_GE(c.widthBytes * 8, c.width * c.bitsPerPixel);
    EXPECT_EQ(c.bits.size(), static_cast<size_t>(c.widthBytes) * c.height);
    EXPECT_TRUE(c.bitsPerPixel == 8 || c.bitsPerPixel == 16 || c.bitsPerPixel == 24 || c.bitsPerPixel == 32) << c.bitsPerPixel;
    // the bitmap lies inside the video frame it belongs to
    EXPECT_GT(c.screenWidth, 0);
    EXPECT_GT(c.screenHeight, 0);
    EXPECT_LE(c.width, c.screenWidth);
    EXPECT_LE(c.height, c.screenHeight);
    EXPECT_GE(c.firstScanLine, 0);
    EXPECT_LT(c.firstScanLine, c.screenHeight);
    EXPECT_GE(c.horizontalPosition, 0);
    EXPECT_GT(c.timestamp, 0u);
    EXPECT_GT(c.timeOut, 0u);
    anyVisible = anyVisible || HasVisiblePixels(c);
  }
  EXPECT_TRUE(anyVisible) << "every decoded bitmap is empty";
}

TEST_F(DvbSub3Graph, TimeStampsDoNotGoBackwards)
{
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  ASSERT_GE(captured.size(), 2u);
  for (size_t i = 1; i < captured.size(); i++)
    EXPECT_GE(captured[i].timestamp, captured[i - 1].timestamp) << "subtitle " << i;
}

TEST_F(DvbSub3Graph, TheSizeOfTheDeliveredSamplesDoesNotChangeTheResult)
{
  // The stream is cut into samples of different sizes, also in the middle of a TS packet and below one packet
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  const std::vector<Captured> reference = captured;
  ASSERT_GE(reference.size(), 5u);

  for (size_t sampleSize : { size_t(188), size_t(188 * 64), size_t(1000), size_t(250), size_t(13), size_t(1) })
  {
    SCOPED_TRACE(testing::Message() << "sample size " << sampleSize);
    // a fresh filter instance for every size
    TearDown();
    captured.clear();
    SetUp();
    Configure(stream->pid);
    Run();
    ASSERT_EQ(Feed(sampleSize), S_OK);
    ASSERT_EQ(captured.size(), reference.size());
    for (size_t i = 0; i < reference.size(); i++)
    {
      EXPECT_EQ(captured[i].timestamp, reference[i].timestamp) << i;
      EXPECT_EQ(captured[i].bits, reference[i].bits) << i;
    }
  }
}

// DVBSub3 used to have its own copy of CPacketSync that read a sample shorter than one TS packet past its end (0 of 13
// subtitles decoded with 7 byte samples). It now uses the shared CPacketSync, which keeps the incomplete packet.
TEST_F(DvbSub3Graph, SamplesShorterThanOnePacketAreDecodedLikeAnyOther)
{
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  const size_t reference = captured.size();
  ASSERT_GE(reference, 5u);

  TearDown();
  captured.clear();
  SetUp();
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(7), S_OK);
  EXPECT_EQ(captured.size(), reference);
}

TEST_F(DvbSub3Graph, NothingIsDecodedWithoutASubtitlePid)
{
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  EXPECT_TRUE(captured.empty());
}

TEST_F(DvbSub3Graph, NothingIsDecodedForAnotherPid)
{
  Configure(0x1234);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  EXPECT_TRUE(captured.empty());
}

TEST_F(DvbSub3Graph, NothingIsDecodedWhileTheFilterIsStopped)
{
  Configure(stream->pid);
  // not running: the base input pin refuses the samples
  HRESULT hr = Feed(188 * 10);
  EXPECT_NE(hr, S_OK);
  EXPECT_TRUE(captured.empty());
}

TEST_F(DvbSub3Graph, SeekingTellsTheClientToDropItsSubtitles)
{
  Configure(stream->pid);
  Run();
  const int before = g_resets;
  CRefTime seek(static_cast<LONGLONG>(10000000));
  ASSERT_EQ(control->SeekDone(seek), S_OK);
  EXPECT_GT(g_resets, before);
}

TEST_F(DvbSub3Graph, FlushingAndFeedingAgainStillDecodes)
{
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  const size_t first = captured.size();
  ASSERT_GE(first, 5u);

  ASSERT_EQ(source->Pin()->FlushDownstream(), S_OK);
  captured.clear();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  EXPECT_EQ(captured.size(), first);
}

TEST_F(DvbSub3Graph, ChannelChangeResetsTheDecoderAndItKeepsWorking)
{
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  const size_t first = captured.size();

  ASSERT_EQ(control->NotifyChannelChange(), S_OK);
  captured.clear();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  EXPECT_GT(captured.size(), 0u);
  EXPECT_LE(captured.size(), first);
}

TEST_F(DvbSub3Graph, DamagedPacketsDoNotCrashTheDecoder)
{
  Configure(stream->pid);
  Run();
  std::vector<byte> damaged = stream->packets;
  std::mt19937 rng(2024);
  for (size_t i = 0; i < damaged.size() / 50; i++)           // 2 % of the bytes
    damaged[rng() % damaged.size()] = static_cast<byte>(rng());
  HRESULT hr = source->Pin()->DeliverBytes(damaged.data(), damaged.size(), 188 * 10);
  EXPECT_TRUE(SUCCEEDED(hr)) << std::hex << hr;
  // whatever came out has to be a consistent bitmap
  for (const Captured& c : captured)
    EXPECT_EQ(c.bits.size(), static_cast<size_t>(c.widthBytes) * c.height);
}

TEST_F(DvbSub3Graph, RandomDataOnTheSubtitlePidDoesNotCrashTheDecoder)
{
  Configure(stream->pid);
  Run();
  std::mt19937 rng(99);
  std::vector<byte> data;
  for (int i = 0; i < 2000; i++)
  {
    byte packet[188];
    for (byte& b : packet)
      b = static_cast<byte>(rng());
    packet[0] = 0x47;
    packet[1] = static_cast<byte>((i % 3 == 0 ? 0x40 : 0) | ((stream->pid >> 8) & 0x1F));
    packet[2] = static_cast<byte>(stream->pid & 0xFF);
    packet[3] = static_cast<byte>(0x10 | (i & 0x0F));
    data.insert(data.end(), packet, packet + 188);
  }
  HRESULT hr = source->Pin()->DeliverBytes(data.data(), data.size(), 188 * 10);
  EXPECT_TRUE(SUCCEEDED(hr)) << std::hex << hr;
}

TEST_F(DvbSub3Graph, RestartingTheGraphKeepsDecoding)
{
  Configure(stream->pid);
  Run();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  const size_t first = captured.size();
  ASSERT_GE(first, 5u);

  Ptr<IMediaControl> mc;
  ASSERT_EQ(graph.As(mc), S_OK);
  ASSERT_TRUE(SUCCEEDED(mc->Stop()));
  Run();
  captured.clear();
  ASSERT_EQ(Feed(188 * 10), S_OK);
  EXPECT_GT(captured.size(), 0u);
}
