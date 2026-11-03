// CPmtParser and CPatParser of TsReader: which streams a service has, and the list of services of a transport stream. (TsWriter has
// its own copies of this code; the shared base is tested with the unit tests.)
#include <gtest/gtest.h>
#include <winsock2.h>
#include <windows.h>
#include <string>
#include <vector>
#include "PatParser.h"
#include "PmtParser.h"
#include "../unit/SectionBuilder.h"
#include "../unit/CrtAsserts.h"

// the shared parser code asks whether the CRC check is switched off
bool DisableCRCCheck() { return false; }

namespace
{
  const int kPmtPid = 0x20;

  class PmtCollector : public IPmtCallBack
  {
  public:
    void OnPmtReceived(int pid) override { received.push_back(pid); }
    void OnPidsReceived(const CPidTable& info) override { pids++; last = info; }
    std::vector<int> received;
    int pids = 0;
    CPidTable last;
  };

  void FeedPmt(CPmtParser& p, int serviceId, const Bytes& body, int pid = kPmtPid)
  {
    unsigned counter = 0;
    for (const TsPacket& pkt : PacketizeSections(pid, MakeLongSection(0x02, serviceId, 0, body), counter))
      p.OnTsPacket(pkt.data);
  }

  std::string Lang(const BYTE* l, int n = 3) { return std::string(reinterpret_cast<const char*>(l), n); }
}

// ---------------------------------------------------------------------------------------------------------------------------
// CPmtParser
// ---------------------------------------------------------------------------------------------------------------------------

TEST(ReaderPmtParser, StreamsOfAService)
{
  CPmtParser p;
  PmtCollector c;
  p.SetPid(kPmtPid);
  p.SetPmtCallBack(&c);
  EXPECT_FALSE(p.IsReady());
  FeedPmt(p, 0x44, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Iso639("eng") }, { 0x0F, 0x102, Bytes() } }));
  EXPECT_TRUE(p.IsReady());
  EXPECT_EQ(c.received, (std::vector<int>{ kPmtPid }));
  EXPECT_EQ(c.pids, 1);
  CPidTable& t = p.GetPidInfo();
  EXPECT_EQ(t.PcrPid, 0x100u);
  EXPECT_EQ(t.PmtPid, static_cast<ULONG>(kPmtPid));
  EXPECT_EQ(t.ServiceId, 0x44);
  ASSERT_EQ(t.videoPids.size(), 1u);
  EXPECT_EQ(t.videoPids[0].VideoServiceType, 0x1B);
  ASSERT_EQ(t.audioPids.size(), 2u);
  EXPECT_EQ(Lang(t.audioPids[0].Lang), "eng");
  p.ClearReady();
  EXPECT_FALSE(p.IsReady());
}

TEST(ReaderPmtParser, EveryStreamTypeThatIsKnown)
{
  struct { int type; bool video; } types[] = {
    { 0x01, true }, { 0x02, true }, { 0x10, true }, { 0x1B, true }, { 0x24, true },
    { 0x03, false }, { 0x04, false }, { 0x0F, false }, { 0x11, false }, { 0x81, false }, { 0x82, false }, { 0x84, false },
    { 0x85, false }, { 0x86, false }, { 0x87, false } };
  for (auto t : types)
  {
    CPmtParser p;
    p.SetPid(kPmtPid);
    FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { t.type, 0x110, Bytes() } }));
    ASSERT_TRUE(p.IsReady()) << t.type;
    EXPECT_EQ(p.GetPidInfo().videoPids.size(), t.video ? 1u : 0u) << std::hex << t.type;
    EXPECT_EQ(p.GetPidInfo().audioPids.size(), t.video ? 0u : 1u) << std::hex << t.type;
  }
}

TEST(ReaderPmtParser, PrivateDataWithAnAc3OrDtsDescriptorIsAudio)
{
  CPmtParser p;
  p.SetPid(kPmtPid);
  FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { 0x06, 0x101, Descriptor(0x6A, Bytes()) }, { 0x06, 0x102, Descriptor(0x7A, Bytes()) },
                                          { 0x06, 0x103, Descriptor(0x7B, Bytes()) } }));
  auto& a = p.GetPidInfo().audioPids;
  ASSERT_EQ(a.size(), 3u);
  EXPECT_EQ(a[0].AudioServiceType, SERVICE_TYPE_AUDIO_AC3);
  EXPECT_EQ(a[1].AudioServiceType, SERVICE_TYPE_AUDIO_DD_PLUS);
  EXPECT_EQ(a[2].AudioServiceType, SERVICE_TYPE_AUDIO_DTS);
}

TEST(ReaderPmtParser, PrivateDataWithAHevcRegistrationDescriptorIsVideo)
{
  CPmtParser p;
  p.SetPid(kPmtPid);
  FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { 0x06, 0x100, Descriptor(0x05, Bytes{ 'H', 'E', 'V', 'C' }) }, { 0x04, 0x101, Bytes() } }));
  ASSERT_EQ(p.GetPidInfo().videoPids.size(), 1u);
  EXPECT_EQ(p.GetPidInfo().videoPids[0].VideoServiceType, SERVICE_TYPE_VIDEO_HEVC);
}

TEST(ReaderPmtParser, AvcAndHevcVideoDescriptorsAddASecondVideoStream)
{
  CPmtParser p;
  p.SetPid(kPmtPid);
  FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { 0x02, 0x100, Bytes() }, { 0x05, 0x101, Descriptor(0x28, Bytes{ 0x64, 0, 0x28 }) } }));
  auto& v = p.GetPidInfo().videoPids;
  ASSERT_EQ(v.size(), 2u);
  EXPECT_EQ(v[1].Pid, 0x101);
  EXPECT_EQ(v[1].VideoServiceType, SERVICE_TYPE_VIDEO_H264);
}

TEST(ReaderPmtParser, LanguageOfAnAc3StreamNextToMpegAudio)
{
  CPmtParser p;
  p.SetPid(kPmtPid);
  FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { 0x04, 0x101, Iso639("eng") },
                                          { 0x06, 0x102, Concat(Iso639("spa"), Descriptor(0x6A, Bytes())) } }));
  auto& a = p.GetPidInfo().audioPids;
  ASSERT_EQ(a.size(), 2u);
  EXPECT_EQ(Lang(a[1].Lang), "spa");
}

TEST(ReaderPmtParser, LanguageOfTheOnlyAc3Stream)
{
  CPmtParser p;
  p.SetPid(kPmtPid);
  FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x06, 0x102, Concat(Iso639("spa"), Descriptor(0x6A, Bytes())) } }));
  auto& a = p.GetPidInfo().audioPids;
  ASSERT_EQ(a.size(), 1u);
  EXPECT_EQ(Lang(a[0].Lang), "spa");
}

TEST(ReaderPmtParser, SubtitlesAndTeletext)
{
  CPmtParser p;
  p.SetPid(kPmtPid);
  Bytes ttx{ 'e', 'n', 'g', static_cast<uint8_t>((2 << 3) | 1), 0x88 };
  FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() },
    { 0x06, 0x120, Descriptor(0x59, Bytes{ 'n', 'o', 'r', 0x10, 0, 1, 0, 2 }) }, { 0x06, 0x130, Descriptor(0x56, ttx) } }));
  CPidTable& t = p.GetPidInfo();
  ASSERT_EQ(t.subtitlePids.size(), 1u);
  EXPECT_EQ(t.subtitlePids[0].Pid, 0x120);
  EXPECT_EQ(Lang(t.subtitlePids[0].Lang), "nor");
  EXPECT_EQ(t.TeletextPid, 0x130);
  ASSERT_EQ(t.TeletextInfo.size(), 1u);
  EXPECT_EQ(t.TeletextInfo[0].page, 188);
}

TEST(ReaderPmtParser, OtherTablesAreIgnored)
{
  CPmtParser p;
  p.SetPid(kPmtPid);
  unsigned counter = 0;
  for (const TsPacket& pkt : PacketizeSections(kPmtPid, MakeLongSection(0x42, 1, 0, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } })), counter))
    p.OnTsPacket(pkt.data);
  EXPECT_FALSE(p.IsReady());
}

TEST(ReaderPmtParser, TheLastPmtWinsAndTheCallbackReportsEach)
{
  CPmtParser p;
  PmtCollector c;
  p.SetPid(kPmtPid);
  p.SetPmtCallBack(&c);
  FeedPmt(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Bytes() } }));
  FeedPmt(p, 1, PmtBody(0x200, Bytes(), { { 0x02, 0x200, Bytes() } }));
  EXPECT_EQ(c.received.size(), 1u) << "the PMT is announced once until ClearReady()";
  EXPECT_EQ(c.pids, 2);
  EXPECT_EQ(p.GetPidInfo().PcrPid, 0x200u);
  EXPECT_EQ(p.GetPidInfo().audioPids.size(), 0u);
}

TEST(ReaderPmtParser, DamagedTablesNeverHang)
{
  const Bytes valid = MakeLongSection(2, 1, 0, PmtBody(0x100, Descriptor(0x09, Bytes(4, 0)),
    { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Iso639("eng") }, { 0x06, 0x102, Concat(Iso639("spa"), Descriptor(0x6A, Bytes())) },
      { 0x06, 0x120, Descriptor(0x59, Bytes{ 'n', 'o', 'r', 0x10, 0, 1, 0, 2 }) } }));
  uint32_t seed = 71;
  auto next = [&seed]() { seed = seed * 1664525u + 1013904223u; return seed >> 8; };
  for (int i = 0; i < 10000; i++)
  {
    Bytes damaged = valid;
    const int changes = 1 + next() % 4;
    for (int c = 0; c < changes; c++)
      damaged[8 + next() % (damaged.size() - 8)] = static_cast<uint8_t>(next());
    for (size_t k = 8; k < damaged.size(); k++)
      if (damaged[k] == 0x56)                                // the teletext descriptor asserts on bad data
        damaged[k] = 0x57;
    CPmtParser p;
    p.EnableCrcCheck(false);
    p.SetPid(kPmtPid);
    unsigned counter = 0;
    for (const TsPacket& pkt : PacketizeSections(kPmtPid, damaged, counter))
      p.OnTsPacket(pkt.data);
  }
}

// ---------------------------------------------------------------------------------------------------------------------------
// CPatParser
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class ChannelCollector : public IPatParserCallback
  {
  public:
    void OnNewChannel(CChannelInfo& info) override { channels.push_back(info); }
    std::vector<CChannelInfo> channels;
  };

  Bytes PatBody(const std::vector<std::pair<int, int>>& programs)
  {
    Bytes b;
    for (auto& p : programs)
    {
      b.push_back(static_cast<uint8_t>(p.first >> 8));
      b.push_back(static_cast<uint8_t>(p.first));
      b.push_back(static_cast<uint8_t>(0xE0 | (p.second >> 8)));
      b.push_back(static_cast<uint8_t>(p.second));
    }
    return b;
  }

  void FeedPat(CPatParser& p, int version, const Bytes& body)
  {
    unsigned counter = 0;
    for (const TsPacket& pkt : PacketizeSections(0, MakeLongSection(0, 1, version, body), counter))
    {
      TsPacket copy = pkt;
      p.OnTsPacket(copy.data);
    }
  }

  void FeedPmtTo(CPatParser& p, int pid, int service, const Bytes& body)
  {
    unsigned counter = 0;
    for (const TsPacket& pkt : PacketizeSections(pid, MakeLongSection(2, service, 0, body), counter))
    {
      TsPacket copy = pkt;
      p.OnTsPacket(copy.data);
    }
  }
}

TEST(ReaderPatParser, ServicesAppearWhenTheirPmtArrives)
{
  // without a callback the services wait in the parser to be fetched with GetChannel()
  CPatParser p;
  EXPECT_EQ(p.Count(), 0);
  FeedPat(p, 3, PatBody({ { 0x11, 0x20 }, { 0x12, 0x21 } }));
  EXPECT_EQ(p.Count(), 0) << "no PMT yet";
  FeedPmtTo(p, 0x20, 0x11, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Bytes() } }));
  EXPECT_EQ(p.Count(), 2);
  CChannelInfo info;
  ASSERT_TRUE(p.GetChannel(0, info));
  EXPECT_EQ(info.PidTable.PcrPid, 0x100u);
  EXPECT_EQ(info.PidTable.ServiceId, 0x11);
  EXPECT_FALSE(p.GetChannel(1, info)) << "the PMT of the second service has not come";

  FeedPmtTo(p, 0x21, 0x12, PmtBody(0x200, Bytes(), { { 0x02, 0x200, Bytes() } }));
  ASSERT_TRUE(p.GetChannel(1, info));
  EXPECT_EQ(info.PidTable.ServiceId, 0x12);
}

TEST(ReaderPatParser, WithACallbackEveryServiceIsReportedOnce)
{
  CPatParser p;
  ChannelCollector c;
  p.SetCallBack(&c);
  FeedPat(p, 3, PatBody({ { 0x11, 0x20 }, { 0x12, 0x21 } }));
  FeedPmtTo(p, 0x20, 0x11, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } }));
  ASSERT_EQ(c.channels.size(), 1u);
  EXPECT_EQ(c.channels[0].PidTable.ServiceId, 0x11);
  EXPECT_EQ(c.channels[0].PatVersion, 3);
  EXPECT_EQ(p.Count(), 0) << "reported services are no longer waiting";
  FeedPmtTo(p, 0x21, 0x12, PmtBody(0x200, Bytes(), { { 0x02, 0x200, Bytes() } }));
  ASSERT_EQ(c.channels.size(), 2u);
  EXPECT_EQ(c.channels[1].PidTable.ServiceId, 0x12);
}

TEST(ReaderPatParser, ANewPatVersionStartsAgain)
{
  CPatParser p;
  ChannelCollector c;
  p.SetCallBack(&c);
  FeedPat(p, 1, PatBody({ { 0x11, 0x20 } }));
  FeedPmtTo(p, 0x20, 0x11, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } }));
  ASSERT_EQ(c.channels.size(), 1u);
  FeedPat(p, 2, PatBody({ { 0x33, 0x30 } }));
  EXPECT_EQ(p.Count(), 0) << "the old services are gone";
  FeedPmtTo(p, 0x30, 0x33, PmtBody(0x300, Bytes(), { { 0x1B, 0x300, Bytes() } }));
  ASSERT_EQ(c.channels.size(), 2u);
  EXPECT_EQ(c.channels[1].PidTable.ServiceId, 0x33);
  EXPECT_EQ(c.channels[1].PatVersion, 2);
}

TEST(ReaderPatParser, ResetForgetsEverything)
{
  CPatParser p;
  FeedPat(p, 1, PatBody({ { 0x11, 0x20 } }));
  FeedPmtTo(p, 0x20, 0x11, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } }));
  EXPECT_EQ(p.Count(), 1);
  p.Reset();
  EXPECT_EQ(p.Count(), 0);
}

TEST(ReaderPatParser, PacketsAtTheStartCanBeSkipped)
{
  CPatParser p;
  p.SkipPacketsAtStart(1);
  FeedPat(p, 1, PatBody({ { 0x11, 0x20 } }));                  // the PAT is a single packet: skipped
  FeedPmtTo(p, 0x20, 0x11, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } }));
  EXPECT_EQ(p.Count(), 0);
}

TEST(ReaderPatParser, AProgramWithAnInvalidPidEndsTheTable)
{
  CPatParser p;
  FeedPat(p, 1, PatBody({ { 0x11, 0x20 }, { 0x12, 0x05 }, { 0x13, 0x22 } }));
  FeedPmtTo(p, 0x20, 0x11, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } }));
  FeedPmtTo(p, 0x22, 0x13, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } }));
  EXPECT_EQ(p.Count(), 1) << "the program behind the invalid PID is not looked at any more";
}

TEST(ReaderPatParser, ChannelNumberEqualToTheCountIsRefused)
{
  CPatParser empty;
  CChannelInfo info;
  EXPECT_FALSE(empty.GetChannel(0, info));

  CPatParser p;
  FeedPat(p, 1, PatBody({ { 0x11, 0x20 } }));
  FeedPmtTo(p, 0x20, 0x11, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } }));
  EXPECT_FALSE(p.GetChannel(p.Count(), info));
}

TEST(ReaderPatParser, NegativeIndexIsRefused)
{
  CPatParser p;
  CChannelInfo info;
  EXPECT_FALSE(p.GetChannel(-1, info));
}
