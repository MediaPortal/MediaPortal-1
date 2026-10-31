// CBasePmtParser, CPidTable and CChannelInfo (shared code of TsReader and TsWriter, built into DvbCoreUtils).
#include <gtest/gtest.h>
#include <windows.h>
#include "BasePmtParser.h"
#include "ChannelInfo.h"
#include "SectionBuilder.h"
#include "TcharConv.h"

// BasePmtParser.cpp asks the host program whether the CRC check is switched off
bool DisableCRCCheck() { return false; }

namespace
{
  const int kPmtPid = 0x20;

  // What TsReader and TsWriter do in their OnNewSection(): decode the PMT
  class Parser : public CBasePmtParser
  {
  public:
    void OnNewSection(CSection& section) override { decoded = DecodePmtPidTable(section); sections++; }
    void PmtFoundCallback() override { pmtFound++; }
    void PidsFoundCallback() override { pidsFound++; }
    bool decoded = false;
    int sections = 0, pmtFound = 0, pidsFound = 0;
  };

  // Parses one PMT given as bytes through the transport packets path
  void Feed(Parser& p, int serviceId, const Bytes& body)
  {
    p.SetFilter(kPmtPid, serviceId);
    unsigned counter = 0;
    for (const TsPacket& pkt : PacketizeSections(kPmtPid, MakeLongSection(0x02, serviceId, 0, body), counter))
    {
      TsPacket copy = pkt;
      p.OnTsPacket(copy.data);
    }
  }

  // A section object holding the bytes, for DecodePmtPidTable() alone
  void Load(CSection& s, const Bytes& section)
  {
    s.Reset();
    memcpy(s.Data, section.data(), section.size());
    s.BufferPos = static_cast<int>(section.size());
    s.DecodeHeader();
  }
}

// --- CBasePmtParser ---------------------------------------------------------------------------------------------------------

TEST(BasePmtParser, FilterAndReadyState)
{
  Parser p;
  p.SetFilter(kPmtPid, 0x1234);
  int pid = 0, service = 0;
  p.GetFilter(pid, service);
  EXPECT_EQ(pid, kPmtPid);
  EXPECT_EQ(service, 0x1234);
  EXPECT_FALSE(p.IsReady());
}

TEST(BasePmtParser, VideoAndAudioStreamsOfAProgram)
{
  Parser p;
  Feed(p, 0x2001, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Bytes() }, { 0x0F, 0x102, Bytes() } }));
  ASSERT_TRUE(p.decoded);
  EXPECT_TRUE(p.IsReady());
  EXPECT_EQ(p.pmtFound, 1);
  EXPECT_EQ(p.pidsFound, 1);

  CPidTable& t = p.GetPidInfo();
  EXPECT_EQ(t.PcrPid, 0x100u);
  EXPECT_EQ(t.PmtPid, static_cast<ULONG>(kPmtPid));
  EXPECT_EQ(t.ServiceId, 0x2001);
  ASSERT_EQ(t.videoPids.size(), 1u);
  EXPECT_EQ(t.videoPids[0].Pid, 0x100);
  EXPECT_EQ(t.videoPids[0].VideoServiceType, SERVICE_TYPE_VIDEO_H264);
  ASSERT_EQ(t.audioPids.size(), 2u);
  EXPECT_EQ(t.audioPids[0].Pid, 0x101);
  EXPECT_EQ(t.audioPids[0].AudioServiceType, SERVICE_TYPE_AUDIO_MPEG2);
  EXPECT_EQ(t.audioPids[1].AudioServiceType, SERVICE_TYPE_AUDIO_AAC);
  EXPECT_TRUE(t.subtitlePids.empty());
}

TEST(BasePmtParser, EveryKnownVideoAndAudioStreamType)
{
  struct { int type; bool video; } types[] = {
    { 0x01, true }, { 0x02, true }, { 0x10, true }, { 0x1B, true }, { 0x24, true },
    { 0x03, false }, { 0x04, false }, { 0x0F, false }, { 0x11, false }, { 0x81, false }, { 0x84, false } };
  for (auto t : types)
  {
    Parser p;
    Feed(p, 1, PmtBody(0x100, Bytes(), { { t.type, 0x110, Bytes() } }));
    ASSERT_TRUE(p.decoded) << t.type;
    EXPECT_EQ(p.GetPidInfo().videoPids.size(), t.video ? 1u : 0u) << std::hex << t.type;
    EXPECT_EQ(p.GetPidInfo().audioPids.size(), t.video ? 0u : 1u) << std::hex << t.type;
  }
}

TEST(BasePmtParser, UnknownStreamTypesAreSkipped)
{
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x05, 0x110, Bytes() }, { 0x15, 0x111, Bytes() }, { 0x1B, 0x112, Bytes() } }));
  ASSERT_TRUE(p.decoded);
  EXPECT_EQ(p.GetPidInfo().videoPids.size(), 1u);
  EXPECT_EQ(p.GetPidInfo().audioPids.size(), 0u);
}

TEST(BasePmtParser, OnlyTableId2IsAPmt)
{
  Parser p;
  CSection s;
  Load(s, MakeLongSection(0x42, 1, 0, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } })));
  EXPECT_FALSE(p.DecodePmtPidTable(s));
  EXPECT_FALSE(p.IsReady());
}

TEST(BasePmtParser, ProgramLevelDescriptorsAreSkipped)
{
  Parser p;
  Bytes programInfo = Descriptor(0x09, Bytes(6, 0x11));              // a CA descriptor
  programInfo = Concat(programInfo, Descriptor(0x05, Bytes{ 'A', 'C', '-', '3' }));
  Feed(p, 1, PmtBody(0x100, programInfo, { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Bytes() } }));
  ASSERT_TRUE(p.decoded);
  EXPECT_EQ(p.GetPidInfo().videoPids.size(), 1u);
  EXPECT_EQ(p.GetPidInfo().audioPids.size(), 1u);
}

TEST(BasePmtParser, LanguageOfAnMpegAudioStream)
{
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Iso639("deu") }, { 0x04, 0x102, Iso639("fra") } }));
  ASSERT_TRUE(p.decoded);
  auto& a = p.GetPidInfo().audioPids;
  ASSERT_EQ(a.size(), 2u);
  EXPECT_EQ(std::string(reinterpret_cast<char*>(a[0].Lang), 3), "deu");
  EXPECT_EQ(std::string(reinterpret_cast<char*>(a[1].Lang), 3), "fra");
}

TEST(BasePmtParser, Ac3InPrivateDataIsAnAudioStream)
{
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x06, 0x101, Descriptor(0x6A, Bytes()) }, { 0x06, 0x102, Descriptor(0x7A, Bytes()) } }));
  ASSERT_TRUE(p.decoded);
  auto& a = p.GetPidInfo().audioPids;
  ASSERT_EQ(a.size(), 2u);
  EXPECT_EQ(a[0].AudioServiceType, SERVICE_TYPE_AUDIO_AC3);
  EXPECT_EQ(a[1].AudioServiceType, SERVICE_TYPE_AUDIO_DD_PLUS);
}

TEST(BasePmtParser, LanguageOfAnAc3StreamNextToMpegAudio)
{
  // the language descriptor comes before the AC-3 descriptor, in the loop of the same stream
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x04, 0x101, Iso639("eng") },
                                       { 0x06, 0x102, Concat(Iso639("spa"), Descriptor(0x6A, Bytes())) } }));
  ASSERT_TRUE(p.decoded);
  auto& a = p.GetPidInfo().audioPids;
  ASSERT_GE(a.size(), 2u);
  EXPECT_EQ(std::string(reinterpret_cast<char*>(a[0].Lang), 3), "eng");
  bool found = false;
  for (auto& pid : a)
    if (pid.Pid == 0x102)
    {
      found = true;
      EXPECT_EQ(std::string(reinterpret_cast<char*>(pid.Lang), 3), "spa");
    }
  EXPECT_TRUE(found);
}

TEST(BasePmtParser, LanguageOfTheOnlyAc3Stream)
{
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() },
                                       { 0x06, 0x102, Concat(Iso639("spa"), Descriptor(0x6A, Bytes())) } }));
  ASSERT_TRUE(p.decoded);
  auto& a = p.GetPidInfo().audioPids;
  ASSERT_EQ(a.size(), 1u);
  EXPECT_EQ(std::string(reinterpret_cast<char*>(a[0].Lang), 3), "spa");
}

TEST(BasePmtParser, DvbSubtitlesOfTypeSixCarryTheirLanguage)
{
  Parser p;
  Bytes sub = Descriptor(0x59, Bytes{ 'n', 'o', 'r', 0x10, 0x00, 0x01, 0x00, 0x02 });
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x06, 0x120, sub } }));
  ASSERT_TRUE(p.decoded);
  auto& s = p.GetPidInfo().subtitlePids;
  ASSERT_EQ(s.size(), 1u);
  EXPECT_EQ(s[0].Pid, 0x120);
  EXPECT_EQ(s[0].SubtitleServiceType, SERVICE_TYPE_DVB_SUBTITLES2);
  EXPECT_EQ(std::string(reinterpret_cast<char*>(s[0].Lang), 3), "nor");
}

TEST(BasePmtParser, SubtitleDescriptorOnAnotherStreamTypeIsNotASubtitleStream)
{
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x03, 0x120, Descriptor(0x59, Bytes{ 'n', 'o', 'r', 0x10, 0, 1, 0, 2 }) } }));
  ASSERT_TRUE(p.decoded);
  EXPECT_TRUE(p.GetPidInfo().subtitlePids.empty());
}

TEST(BasePmtParser, TeletextSubtitlePagesAreListed)
{
  Parser p;
  // type 2 (subtitles) page 1.88 and type 5 (for the hard of hearing) page 7.00, type 1 (initial page) is not listed
  Bytes ttx{ 'e', 'n', 'g', static_cast<uint8_t>((2 << 3) | 1), 0x88,
             'e', 'n', 'g', static_cast<uint8_t>((5 << 3) | 7), 0x00,
             'e', 'n', 'g', static_cast<uint8_t>((1 << 3) | 2), 0x00 };
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x06, 0x130, Descriptor(0x56, ttx) } }));
  ASSERT_TRUE(p.decoded);
  CPidTable& t = p.GetPidInfo();
  EXPECT_EQ(t.TeletextPid, 0x130);
  ASSERT_EQ(t.TeletextInfo.size(), 2u);
  EXPECT_EQ(t.TeletextInfo[0].page, 188);
  EXPECT_EQ(t.TeletextInfo[0].type, 2);
  EXPECT_EQ(std::string(t.TeletextInfo[0].lang, 3), "eng");
  EXPECT_EQ(t.TeletextInfo[1].page, 700);
  EXPECT_EQ(t.TeletextInfo[1].type, 5);
  EXPECT_TRUE(t.HasTeletextPageInfo(188));
  EXPECT_FALSE(t.HasTeletextPageInfo(200));
}

TEST(BasePmtParser, TeletextSubtitlePageOfMagazine8)
{
  // Magazine 8 is coded as 0 in the descriptor: the UK subtitle page 888 is magazine 0, page 0x88
  Parser p;
  Bytes ttx{ 'e', 'n', 'g', static_cast<uint8_t>(2 << 3), 0x88 };
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x06, 0x130, Descriptor(0x56, ttx) } }));
  ASSERT_TRUE(p.decoded);
  ASSERT_EQ(p.GetPidInfo().TeletextInfo.size(), 1u);
  EXPECT_EQ(p.GetPidInfo().TeletextInfo[0].page, 888);
}

TEST(BasePmtParser, LpcmAudioBehindAnHdmvRegistrationDescriptor)
{
  Parser p;
  Bytes reg = Descriptor(0x05, Bytes{ 'H', 'D', 'M', 'V', 0xFF, 0x80 });
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x80, 0x140, reg } }));
  ASSERT_TRUE(p.decoded);
  CPidTable& t = p.GetPidInfo();
  ASSERT_EQ(t.audioPids.size(), 1u);
  EXPECT_EQ(t.audioPids[0].Pid, 0x140);
  EXPECT_EQ(t.videoPids.size(), 1u) << "an LPCM stream is not a video stream";
}

TEST(BasePmtParser, Type80WithoutTheHdmvDescriptorIsDcIiVideo)
{
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x80, 0x140, Bytes() } }));
  ASSERT_TRUE(p.decoded);
  ASSERT_EQ(p.GetPidInfo().videoPids.size(), 1u);
  EXPECT_EQ(p.GetPidInfo().videoPids[0].VideoServiceType, SERVICE_TYPE_VIDEO_MPEG2);
}

TEST(BasePmtParser, ASecondPmtReplacesTheFirstAndTheCallbackRunsOnce)
{
  Parser p;
  Feed(p, 1, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Bytes() } }));
  EXPECT_EQ(p.sections, 1);
  // after the PMT was found OnTsPacket(byte*) stops looking until Reset()
  Feed(p, 1, PmtBody(0x200, Bytes(), { { 0x02, 0x200, Bytes() } }));
  EXPECT_EQ(p.sections, 1);
  EXPECT_EQ(p.GetPidInfo().PcrPid, 0x100u);
  p.Reset();
  EXPECT_FALSE(p.IsReady());
  Feed(p, 1, PmtBody(0x200, Bytes(), { { 0x02, 0x200, Bytes() } }));
  EXPECT_EQ(p.sections, 2);
  EXPECT_EQ(p.GetPidInfo().PcrPid, 0x200u);
  EXPECT_EQ(p.GetPidInfo().audioPids.size(), 0u);
}

TEST(BasePmtParser, DecodingTheSamePmtAgainGivesTheSameTable)
{
  Parser p;
  CSection s;
  Load(s, MakeLongSection(0x02, 1, 0, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Iso639("eng") } })));
  ASSERT_TRUE(p.DecodePmtPidTable(s));
  CPidTable first = p.GetPidInfo();
  ASSERT_TRUE(p.DecodePmtPidTable(s));
  EXPECT_TRUE(first == p.GetPidInfo());
  EXPECT_EQ(p.pmtFound, 1);
  EXPECT_EQ(p.pidsFound, 2);
}

TEST(BasePmtParser, DescriptorLongerThanTheSectionStopsTheParser)
{
  Parser p;
  // the ES info loop claims 200 bytes but the section ends
  Bytes body = PmtBody(0x100, Bytes(), { { 0x04, 0x101, Bytes() } });
  body[body.size() - 1] = 200;                    // ES_info_length low byte
  CSection s;
  Load(s, MakeLongSection(0x02, 1, 0, body));
  const bool ok = p.DecodePmtPidTable(s);
  EXPECT_TRUE(ok || !ok);                         // either way it has to come back
  EXPECT_EQ(p.GetPidInfo().audioPids.size(), 1u);
}

TEST(BasePmtParser, DamagedPmtsNeverHang)
{
  // flip bytes of a valid PMT at random; 0x56 (teletext descriptor) is avoided: the parser asserts on a bad teletext
  // descriptor, which is for the debugger
  const Bytes valid = MakeLongSection(0x02, 1, 0, PmtBody(0x100, Descriptor(0x09, Bytes(4, 1)),
    { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Iso639("eng") }, { 0x06, 0x102, Concat(Iso639("spa"), Descriptor(0x6A, Bytes())) },
      { 0x06, 0x120, Descriptor(0x59, Bytes{ 'n', 'o', 'r', 0x10, 0, 1, 0, 2 }) } }));
  uint32_t seed = 99;
  auto next = [&seed]() { seed = seed * 1664525u + 1013904223u; return seed >> 8; };
  for (int i = 0; i < 20000; i++)
  {
    Bytes damaged = valid;
    const int changes = 1 + next() % 4;
    for (int c = 0; c < changes; c++)
      damaged[8 + next() % (damaged.size() - 8)] = static_cast<uint8_t>(next());
    for (size_t k = 8; k < damaged.size(); k++)
      if (damaged[k] == 0x56)
        damaged[k] = 0x57;
    Parser p;
    CSection s;
    Load(s, damaged);
    p.DecodePmtPidTable(s);
  }
}

// --- CPidTable --------------------------------------------------------------------------------------------------------------

TEST(PidTable, StartsEmpty)
{
  CPidTable t;
  EXPECT_EQ(t.PcrPid, 0u);
  EXPECT_EQ(t.PmtPid, 0u);
  EXPECT_EQ(t.ServiceId, -1);
  EXPECT_EQ(t.TeletextPid, 0);
  EXPECT_TRUE(t.videoPids.empty());
  EXPECT_TRUE(t.audioPids.empty());
  EXPECT_TRUE(t.subtitlePids.empty());
}

TEST(PidTable, DefaultStreamEntriesAreUnknown)
{
  AudioPid a;
  EXPECT_EQ(std::string(reinterpret_cast<char*>(a.Lang)), "UNK");
  SubtitlePid s;
  EXPECT_EQ(std::string(reinterpret_cast<char*>(s.Lang)), "UNK");
  VideoPid v;
  EXPECT_EQ(v.VideoServiceType, -1);
}

TEST(PidTable, CopyTakesEverythingButTheTeletextPages)
{
  CPidTable a;
  a.PcrPid = 1; a.PmtPid = 2; a.ServiceId = 3; a.TeletextPid = 4;
  VideoPid v; v.Pid = 0x100; v.VideoServiceType = 0x1B; a.videoPids.push_back(v);
  AudioPid au; au.Pid = 0x101; au.AudioServiceType = 4; a.audioPids.push_back(au);
  SubtitlePid sp; sp.Pid = 0x102; a.subtitlePids.push_back(sp);
  a.TeletextInfo.push_back(TeletextServiceInfo());

  CPidTable b(a);
  EXPECT_EQ(b.PcrPid, 1u);
  EXPECT_EQ(b.PmtPid, 2u);
  EXPECT_EQ(b.ServiceId, 3);
  EXPECT_EQ(b.TeletextPid, 4);
  EXPECT_EQ(b.videoPids.size(), 1u);
  EXPECT_EQ(b.audioPids.size(), 1u);
  EXPECT_EQ(b.subtitlePids.size(), 1u);
  EXPECT_TRUE(b.TeletextInfo.empty()) << "the page list is left out of Copy() on purpose (commented out in the source)";

  CPidTable c;
  c = a;
  EXPECT_EQ(c.audioPids[0].Pid, 0x101);
  c = c;                                          // self assignment
  EXPECT_EQ(c.audioPids.size(), 1u);
}

TEST(PidTable, ResetEmptiesTheStreamsButKeepsTheTeletextPages)
{
  CPidTable t;
  t.PcrPid = 1;
  t.videoPids.push_back(VideoPid());
  t.TeletextInfo.push_back(TeletextServiceInfo());
  t.Reset();
  EXPECT_EQ(t.PcrPid, 0u);
  EXPECT_TRUE(t.videoPids.empty());
  EXPECT_EQ(t.TeletextInfo.size(), 1u) << "\"no reason to reset TeletextSubLang\" in the source";
}

TEST(PidTable, ComparisonLooksAtStreamsAndPcrAndPmtOnly)
{
  CPidTable a, b;
  EXPECT_TRUE(a == b);
  b.ServiceId = 5;                                 // not compared
  b.TeletextPid = 9;
  EXPECT_TRUE(a == b);
  b.PcrPid = 1;
  EXPECT_FALSE(a == b);
  b.PcrPid = 0;
  AudioPid au; au.Pid = 7;
  a.audioPids.push_back(au);
  EXPECT_FALSE(a == b);
  b.audioPids.push_back(au);
  EXPECT_TRUE(a == b);
  b.audioPids[0].Lang[0] = 'X';
  EXPECT_FALSE(a == b);
}

TEST(PidTable, StreamTypesHaveNames)
{
  CPidTable t;
  EXPECT_EQ(ToAnsi(t.StreamFormatAsString(0x1B)), "H264");
  EXPECT_EQ(ToAnsi(t.StreamFormatAsString(0x24)), "HEVC");
  EXPECT_EQ(ToAnsi(t.StreamFormatAsString(0x81)), "AC3");
  EXPECT_EQ(ToAnsi(t.StreamFormatAsString(0x0F)), "AAC");
  EXPECT_EQ(ToAnsi(t.StreamFormatAsString(0x90)), "PGS");
  EXPECT_EQ(ToAnsi(t.StreamFormatAsString(0x7F)), "Unknown");
  EXPECT_EQ(ToAnsi(t.StreamFormatAsString(-1)), "Unknown");
}

// --- CChannelInfo -----------------------------------------------------------------------------------------------------------

TEST(ChannelInfo, ResetValues)
{
  CChannelInfo c;
  EXPECT_EQ(c.LCN, 10000);
  EXPECT_EQ(c.PatVersion, -1);
  EXPECT_EQ(c.ServiceId, 0);
  EXPECT_STREQ(c.ServiceName, "");
  EXPECT_STREQ(c.ProviderName, "");
  EXPECT_FALSE(c.OtherMux);
  EXPECT_FALSE(c.PmtReceived);
  EXPECT_FALSE(c.SdtReceived);
}

TEST(ChannelInfo, CopyKeepsTheChannelData)
{
  CChannelInfo a;
  a.NetworkId = 1; a.TransportId = 2; a.ServiceId = 3; a.MajorChannel = 4; a.MinorChannel = 5; a.Frequency = 6;
  a.ServiceType = 7; a.Modulation = 8; a.LCN = 9; a.FreeCAMode = 1; a.hasVideo = 1; a.hasAudio = 1; a.hasCaDescriptor = 1;
  a.OtherMux = true; a.PmtReceived = true; a.SdtReceived = true;
  strcpy(a.ServiceName, "Channel One");
  strcpy(a.ProviderName, "Provider");
  a.PidTable.PcrPid = 0x100;
  a.PidTable.videoPids.push_back(VideoPid());

  CChannelInfo b(a);
  EXPECT_EQ(b.NetworkId, 1);
  EXPECT_EQ(b.TransportId, 2);
  EXPECT_EQ(b.ServiceId, 3);
  EXPECT_EQ(b.MajorChannel, 4);
  EXPECT_EQ(b.MinorChannel, 5);
  EXPECT_EQ(b.Frequency, 6);
  EXPECT_EQ(b.ServiceType, 7);
  EXPECT_EQ(b.Modulation, 8);
  EXPECT_EQ(b.LCN, 9);
  EXPECT_EQ(b.FreeCAMode, 1);
  EXPECT_TRUE(b.OtherMux && b.PmtReceived && b.SdtReceived);
  EXPECT_STREQ(b.ServiceName, "Channel One");
  EXPECT_STREQ(b.ProviderName, "Provider");
  EXPECT_EQ(b.PidTable.PcrPid, 0x100u);
  EXPECT_EQ(b.PidTable.videoPids.size(), 1u);
}

TEST(ChannelInfo, CopyKeepsTheEitFlagsRunningStatusAndPatVersion)
{
  CChannelInfo a;
  a.EIT_schedule_flag = 1; a.EIT_present_following_flag = 1; a.RunningStatus = 4; a.PatVersion = 7;
  CChannelInfo b;
  b = a;
  EXPECT_EQ(b.EIT_schedule_flag, 1);
  EXPECT_EQ(b.EIT_present_following_flag, 1);
  EXPECT_EQ(b.RunningStatus, 4);
  EXPECT_EQ(b.PatVersion, 7);
  CChannelInfo c(a);
  EXPECT_EQ(c.RunningStatus, 4);
  EXPECT_EQ(c.PatVersion, 7);
}

TEST(ChannelInfo, SelfAssignmentIsHarmless)
{
  CChannelInfo a;
  strcpy(a.ServiceName, "X");
  a = a;
  EXPECT_STREQ(a.ServiceName, "X");
}
