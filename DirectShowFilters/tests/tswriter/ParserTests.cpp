// TsWriter: the small decoders that look at single packets and tables. PCR and PTS/DTS, the SDT (service names), the PMT
// (streams of a service) and the PES reassembly.
#include <gtest/gtest.h>
#include <windows.h>
#include <memory>
#include <string>
#include "PcrDecoder.h"
#include "SdtParser.h"
#include "PmtParser.h"
#include "PesDecoder.h"
#include "../unit/GuardedBuffer.h"
#include "../unit/SectionBuilder.h"

// the parsers ask whether the CRC check is switched off (a registry setting of the filter)
bool DisableCRCCheck() { return false; }

namespace
{
  // A packet with an adaption field that has a PCR
  TsPacket PcrPacket(unsigned pid, uint64_t base, unsigned extension = 0)
  {
    TsPacket p = MakePacket(pid, 0x20);
    p.data[4] = 183;
    p.data[5] = 0x10;
    EncodePcr(&p.data[6], base, extension);
    return p;
  }

  // The start of a PES packet: 00 00 01 id, length, flags, header length, PTS (and DTS)
  std::vector<byte> PesHeader(int streamId, bool pts, bool dts, uint64_t ptsValue, uint64_t dtsValue)
  {
    std::vector<byte> h(19, 0);
    h[2] = 1;
    h[3] = static_cast<byte>(streamId);
    h[6] = 0x80;
    h[7] = static_cast<byte>((pts ? 0x80 : 0) | (dts ? 0x40 : 0));
    h[8] = static_cast<byte>((pts ? 5 : 0) + (dts ? 5 : 0));
    if (pts)
      EncodeTimestamp(&h[9], dts ? 3 : 2, ptsValue);
    if (dts)
      EncodeTimestamp(&h[14], 1, dtsValue);
    return h;
  }
}

// ---------------------------------------------------------------------------------------------------------------------------
// CPcrDecoder
// ---------------------------------------------------------------------------------------------------------------------------

TEST(PcrDecoder, IgnoresEverythingWithoutAPid)
{
  CPcrDecoder d;
  EXPECT_EQ(d.GetPcrPid(), -1);
  TsPacket p = PcrPacket(0x100, 12345);
  d.OnTsPacket(p.data);
  EXPECT_EQ(d.Pcr(), 0u);
}

TEST(PcrDecoder, ReadsThePcrBase)
{
  CPcrDecoder d;
  d.SetPcrPid(0x100);
  EXPECT_EQ(d.GetPcrPid(), 0x100);
  for (uint64_t base : { 0ull, 1ull, 90000ull, 0x12345678ull, 0x1FFFFFFFFull })
  {
    TsPacket p = PcrPacket(0x100, base, 77);
    d.OnTsPacket(p.data);
    EXPECT_EQ(d.Pcr(), base);
    EXPECT_EQ(d.PcrHigh(), base);
    EXPECT_EQ(d.PcrLow(), 0u);
  }
}

TEST(PcrDecoder, OtherPidsPayloadOnlyPacketsAndShortFieldsAreIgnored)
{
  CPcrDecoder d;
  d.SetPcrPid(0x100);
  TsPacket p = PcrPacket(0x101, 500);
  d.OnTsPacket(p.data);
  EXPECT_EQ(d.Pcr(), 0u);

  p = MakePacket(0x100, 0x10);                         // payload only
  d.OnTsPacket(p.data);
  EXPECT_EQ(d.Pcr(), 0u);

  p = PcrPacket(0x100, 500);
  p.data[4] = 6;                                       // adaption field too short for a PCR
  d.OnTsPacket(p.data);
  EXPECT_EQ(d.Pcr(), 0u);

  p = PcrPacket(0x100, 500);
  p.data[5] = 0x00;                                    // PCR flag not set
  d.OnTsPacket(p.data);
  EXPECT_EQ(d.Pcr(), 0u);
}

TEST(PcrDecoder, ResetAndNewPidForgetThePcr)
{
  CPcrDecoder d;
  d.SetPcrPid(0x100);
  TsPacket p = PcrPacket(0x100, 4242);
  d.OnTsPacket(p.data);
  d.Reset();
  EXPECT_EQ(d.Pcr(), 0u);
  EXPECT_EQ(d.GetPcrPid(), 0x100);
  d.OnTsPacket(p.data);
  d.SetPcrPid(0x200);
  EXPECT_EQ(d.Pcr(), 0u);
}

TEST(PcrDecoder, PtsAndDtsOfAPesHeader)
{
  CPcrDecoder d;
  UINT64 pts = 99, dts = 99;
  auto h = PesHeader(0xE0, true, true, 0x123456789ull & 0x1FFFFFFFFull, 0x0ABCDEF01ull);
  EXPECT_TRUE(d.GetPtsDts(h.data(), pts, dts));
  EXPECT_EQ(pts, 0x123456789ull & 0x1FFFFFFFFull);
  EXPECT_EQ(dts, 0x0ABCDEF01ull);

  h = PesHeader(0xC0, true, false, 900000, 0);
  EXPECT_TRUE(d.GetPtsDts(h.data(), pts, dts));
  EXPECT_EQ(pts, 900000u);
  EXPECT_EQ(dts, 0u);

  h = PesHeader(0xC0, false, false, 0, 0);
  EXPECT_FALSE(d.GetPtsDts(h.data(), pts, dts));
  EXPECT_EQ(pts, 0u);
}

TEST(PcrDecoder, PtsOf33Bits)
{
  CPcrDecoder d;
  UINT64 pts = 0, dts = 0;
  for (uint64_t value : { 0ull, 1ull, 0xFFFFFFFFull, 0x100000000ull, 0x1FFFFFFFFull })
  {
    auto h = PesHeader(0xE0, true, false, value, 0);
    ASSERT_TRUE(d.GetPtsDts(h.data(), pts, dts));
    EXPECT_EQ(pts, value);
  }
}

TEST(PcrDecoder, ChangePtsDtsSubtractsTheStartTime)
{
  CPcrDecoder d;
  UINT64 pts = 0, dts = 0;
  auto h = PesHeader(0xE0, true, true, 1000000, 996400);
  d.ChangePtsDts(h.data(), 900000);
  ASSERT_TRUE(d.GetPtsDts(h.data(), pts, dts));
  EXPECT_EQ(pts, 100000u);
  EXPECT_EQ(dts, 96400u);
  // the marker bits are still right: PTS 0011 with a DTS following, DTS 0001
  EXPECT_EQ(h[9] & 0xF1, 0x31);
  EXPECT_EQ(h[14] & 0xF1, 0x11);
}

TEST(PcrDecoder, ChangePtsDtsOfAPtsOnlyHeader)
{
  CPcrDecoder d;
  UINT64 pts = 0, dts = 0;
  auto h = PesHeader(0xC0, true, false, 5000, 0);
  d.ChangePtsDts(h.data(), 1000);
  ASSERT_TRUE(d.GetPtsDts(h.data(), pts, dts));
  EXPECT_EQ(pts, 4000u);
  EXPECT_EQ(h[9] & 0xF1, 0x21);
}

TEST(PcrDecoder, ChangePtsDtsNeverGoesBelowZero)
{
  CPcrDecoder d;
  UINT64 pts = 0, dts = 0;
  auto h = PesHeader(0xE0, true, true, 1000, 900);
  d.ChangePtsDts(h.data(), 5000);
  ASSERT_TRUE(d.GetPtsDts(h.data(), pts, dts));
  EXPECT_EQ(pts, 0u);
  EXPECT_EQ(dts, 0u);
}

TEST(PcrDecoder, ChangePtsDtsLeavesDataWithoutAStartCodeAlone)
{
  CPcrDecoder d;
  auto h = PesHeader(0xE0, true, true, 1000000, 996400);
  h[2] = 0;                                            // not 00 00 01
  const auto copy = h;
  d.ChangePtsDts(h.data(), 900000);
  EXPECT_EQ(h, copy);
}

// ---------------------------------------------------------------------------------------------------------------------------
// CSdtParser
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class SdtCollector : public ISdtCallBack
  {
  public:
    void OnSdtReceived(const CChannelInfo& info) override { services.push_back(info); }
    std::vector<CChannelInfo> services;
  };

  void FeedSdt(CSdtParser& parser, int tableId, int transportId, const Bytes& body)
  {
    unsigned counter = 0;
    for (const TsPacket& p : PacketizeSections(0x11, MakeLongSection(tableId, transportId, 0, body), counter))
      parser.OnTsPacket(p.data);
  }
}

TEST(SdtParser, ReadsServiceNamesAndFlags)
{
  CSdtParser parser;
  SdtCollector c;
  parser.SetCallback(&c);
  FeedSdt(parser, 0x42, 0x0404, SdtBody(0x20FB,
    { { 0x2001, 4, 0, 0x19, Text("ZDF Vision"), Text("ZDF HD") }, { 0x2002, 1, 1, 0x01, Text("ZDFvision"), Text("3sat") } }));
  ASSERT_EQ(c.services.size(), 2u);
  EXPECT_EQ(c.services[0].ServiceId, 0x2001);
  EXPECT_EQ(c.services[0].NetworkId, 0x20FB);
  EXPECT_EQ(c.services[0].TransportId, 0x0404);
  EXPECT_EQ(c.services[0].ServiceType, 0x19);
  EXPECT_EQ(c.services[0].FreeCAMode, 0);
  EXPECT_FALSE(c.services[0].OtherMux);
  EXPECT_STREQ(c.services[0].ServiceName, "\x15" "ZDF HD");
  EXPECT_STREQ(c.services[0].ProviderName, "\x15" "ZDF Vision");
  EXPECT_EQ(c.services[1].ServiceId, 0x2002);
  EXPECT_EQ(c.services[1].FreeCAMode, 1);
  EXPECT_STREQ(c.services[1].ServiceName, "\x15" "3sat");
}

TEST(SdtParser, TheOtherMuxTableIsMarked)
{
  CSdtParser parser;
  SdtCollector c;
  parser.SetCallback(&c);
  FeedSdt(parser, 0x46, 0x0405, SdtBody(1, { { 7, 4, 0, 1, Text("P"), Text("N") } }));
  ASSERT_EQ(c.services.size(), 1u);
  EXPECT_TRUE(c.services[0].OtherMux);
  EXPECT_EQ(c.services[0].TransportId, 0x0405);
}

TEST(SdtParser, OtherTablesOnThePidAreIgnored)
{
  CSdtParser parser;
  SdtCollector c;
  parser.SetCallback(&c);
  FeedSdt(parser, 0x4A, 1, SdtBody(1, { { 7, 4, 0, 1, Text("P"), Text("N") } }));         // BAT
  FeedSdt(parser, 0x40, 1, SdtBody(1, { { 7, 4, 0, 1, Text("P"), Text("N") } }));         // NIT actual
  EXPECT_TRUE(c.services.empty());
}

TEST(SdtParser, NamesInOtherCodingsAreConverted)
{
  CSdtParser parser;
  SdtCollector c;
  parser.SetCallback(&c);
  Bytes accent{ 'C', 'a', 'f', 0xC2, 'e' };                              // ISO 6937: Cafe with acute accent
  Bytes ucs2{ 0x11, 0x04, 0x16, 0x04, 0x30 };                           // UCS-2: Cyrillic
  FeedSdt(parser, 0x42, 1, SdtBody(1, { { 1, 4, 0, 1, Text("P"), accent }, { 2, 4, 0, 1, Text("P"), ucs2 } }));
  ASSERT_EQ(c.services.size(), 2u);
  EXPECT_STREQ(c.services[0].ServiceName, "\x15" "Caf\xC3\xA9");
  EXPECT_STREQ(c.services[1].ServiceName, "\x15" "\xD0\x96\xD0\xB0");
}

TEST(SdtParser, ASectionWithAWrongCrcIsDropped)
{
  CSdtParser parser;
  SdtCollector c;
  parser.SetCallback(&c);
  unsigned counter = 0;
  for (const TsPacket& p : PacketizeSections(0x11, MakeLongSection(0x42, 1, 0, SdtBody(1, { { 7, 4, 0, 1, Text("P"), Text("N") } }), 0, 0, true), counter))
    parser.OnTsPacket(p.data);
  EXPECT_TRUE(c.services.empty());
}

TEST(SdtParser, NoCallbackIsFine)
{
  CSdtParser parser;
  FeedSdt(parser, 0x42, 1, SdtBody(1, { { 7, 4, 0, 1, Text("P"), Text("N") } }));
}

TEST(SdtParser, NamesAreCutToTheSizeOfTheFields)
{
  CSdtParser parser;
  SdtCollector c;
  parser.SetCallback(&c);
  FeedSdt(parser, 0x42, 1, SdtBody(1, { { 7, 4, 0, 1, Bytes(120, 'p'), Bytes(120, 'n') } }));
  ASSERT_EQ(c.services.size(), 1u);
  EXPECT_LT(strlen(c.services[0].ServiceName), 255u);
  EXPECT_LT(strlen(c.services[0].ProviderName), 255u);
}

TEST(SdtParser, DamagedTablesNeverCrash)
{
  const Bytes valid = SdtBody(0x20FB, { { 0x2001, 4, 0, 0x19, Text("Provider"), Text("Station") }, { 0x2002, 1, 1, 0x01, Text("P2"), Text("Two") } });
  uint32_t seed = 31;
  auto next = [&seed]() { seed = seed * 1664525u + 1013904223u; return seed >> 8; };
  for (int i = 0; i < 20000; i++)
  {
    Bytes body = valid;
    const int changes = 1 + next() % 5;
    for (int c = 0; c < changes; c++)
      body[next() % body.size()] = static_cast<uint8_t>(next());
    CSdtParser parser;
    SdtCollector c;
    parser.SetCallback(&c);
    FeedSdt(parser, 0x42, 1, body);
    for (const CChannelInfo& s : c.services)
    {
      ASSERT_LT(strlen(s.ServiceName), 255u);
      ASSERT_LT(strlen(s.ProviderName), 255u);
    }
  }
}

// ---------------------------------------------------------------------------------------------------------------------------
// CPmtParser
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class PmtCollector : public IPmtCallBack2
  {
  public:
    void OnPmtReceived2(int pid, int serviceId, int pcrPid, bool ca, vector<PidInfo2> info) override
    {
      calls++;
      this->pid = pid; this->serviceId = serviceId; this->pcrPid = pcrPid; hasCa = ca; infos = info;
    }
    int calls = 0, pid = 0, serviceId = 0, pcrPid = 0;
    bool hasCa = false;
    vector<PidInfo2> infos;
  };

  CSection AsSection(const Bytes& bytes)
  {
    CSection s;
    memcpy(s.Data, bytes.data(), bytes.size());
    s.BufferPos = static_cast<int>(bytes.size());
    s.DecodeHeader();
    return s;
  }
}

TEST(PmtParser, DecodesTheStreamsOfAService)
{
  CPmtParser p;
  p.SetFilter(0x20, 5);
  int pcr = 0;
  bool ca = false;
  vector<PidInfo2> infos;
  CSection s = AsSection(MakeLongSection(2, 5, 0, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Iso639("eng") }, { 0x0F, 0x102, Bytes() } })));
  EXPECT_TRUE(p.DecodePmt(s, pcr, ca, infos));
  EXPECT_EQ(pcr, 0x100);
  EXPECT_FALSE(ca);
  ASSERT_EQ(infos.size(), 3u);
  EXPECT_EQ(infos[0].elementaryPid, 0x100);
  EXPECT_EQ(infos[0].streamType, 0x1B);
  EXPECT_EQ(infos[0].logicalStreamType, 0x1B);
  EXPECT_EQ(infos[1].elementaryPid, 0x101);
  EXPECT_EQ(infos[1].rawDescriptorSize, 6);
  EXPECT_EQ(infos[1].rawDescriptorData[0], 0x0A);
  EXPECT_EQ(infos[2].streamType, 0x0F);
}

TEST(PmtParser, ADifferentServiceIsRefused)
{
  CPmtParser p;
  p.SetFilter(0x20, 5);
  int pcr = 0;
  bool ca = false;
  vector<PidInfo2> infos;
  CSection s = AsSection(MakeLongSection(2, 6, 0, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } })));
  EXPECT_FALSE(p.DecodePmt(s, pcr, ca, infos));
}

TEST(PmtParser, AnyServiceIsAcceptedWithoutAFilter)
{
  CPmtParser p;
  int pcr = 0;
  bool ca = false;
  vector<PidInfo2> infos;
  CSection s = AsSection(MakeLongSection(2, 77, 0, PmtBody(0x100, Bytes(), { { 0x1B, 0x100, Bytes() } })));
  p.SetFilter(0x20, -1);
  EXPECT_TRUE(p.DecodePmt(s, pcr, ca, infos));
}

TEST(PmtParser, OnlyTableId2IsAPmt)
{
  CPmtParser p;
  int pcr = 0;
  bool ca = false;
  vector<PidInfo2> infos;
  CSection s = AsSection(MakeLongSection(0x42, 1, 0, PmtBody(0x100, Bytes(), {})));
  EXPECT_FALSE(p.DecodePmt(s, pcr, ca, infos));
}

TEST(PmtParser, ACaDescriptorOfTheProgramMarksTheServiceScrambled)
{
  CPmtParser p;
  p.SetFilter(0x20, 1);
  int pcr = 0;
  bool ca = false;
  vector<PidInfo2> infos;
  CSection s = AsSection(MakeLongSection(2, 1, 0, PmtBody(0x100, Descriptor(0x09, Bytes{ 0x06, 0x14, 0xE1, 0x00, 0, 0 }), { { 0x1B, 0x100, Bytes() } })));
  EXPECT_TRUE(p.DecodePmt(s, pcr, ca, infos));
  EXPECT_TRUE(ca);
}

TEST(PmtParser, DescriptorsDecideTheLogicalStreamType)
{
  CPmtParser p;
  p.SetFilter(0x20, 1);
  int pcr = 0;
  bool ca = false;
  vector<PidInfo2> infos;
  Bytes subs = Descriptor(0x59, Bytes{ 'e', 'n', 'g', 0x10, 0, 1, 0, 2 });
  CSection s = AsSection(MakeLongSection(2, 1, 0, PmtBody(0x100, Bytes(), {
    { 0x06, 0x101, Descriptor(0x6A, Bytes()) },            // AC-3
    { 0x06, 0x102, Descriptor(0x7A, Bytes()) },            // enhanced AC-3
    { 0x06, 0x103, Descriptor(0x56, Bytes{ 'e', 'n', 'g', 0x10, 0x88 }) },   // teletext
    { 0x06, 0x104, subs },                                 // DVB subtitles
    { 0x06, 0x105, Bytes() } })));                         // private data without a descriptor
  EXPECT_TRUE(p.DecodePmt(s, pcr, ca, infos));
  ASSERT_EQ(infos.size(), 5u);
  EXPECT_EQ(infos[0].logicalStreamType, SERVICE_TYPE_AUDIO_AC3);
  EXPECT_EQ(infos[1].logicalStreamType, SERVICE_TYPE_AUDIO_E_AC3);
  EXPECT_EQ(infos[2].logicalStreamType, DESCRIPTOR_DVB_TELETEXT);
  EXPECT_EQ(infos[3].logicalStreamType, SERVICE_TYPE_DVB_SUBTITLES2);
  EXPECT_EQ(infos[4].logicalStreamType, -1) << "type 6 without a descriptor is not a stream the filter knows";
}

TEST(PmtParser, AnEsInfoLengthOfMoreThan1023IsRefused)
{
  CPmtParser p;
  p.SetFilter(0x20, 1);
  int pcr = 0;
  bool ca = false;
  vector<PidInfo2> infos;
  Bytes big = Descriptor(0x80, Bytes(250, 1));
  Bytes many;
  for (int i = 0; i < 5; i++)
    many = Concat(many, big);                              // 1260 bytes
  CSection s = AsSection(MakeLongSection(2, 1, 0, PmtBody(0x100, Bytes(), { { 0x06, 0x101, many } })));
  EXPECT_FALSE(p.DecodePmt(s, pcr, ca, infos));
}

TEST(PmtParser, TheCallbackGetsTheTableOnceThroughPackets)
{
  CPmtParser p;
  PmtCollector c;
  p.SetFilter(0x20, 9);
  p.SetPmtCallBack2(&c);
  unsigned counter = 0;
  const auto packets = PacketizeSections(0x20, MakeLongSection(2, 9, 0, PmtBody(0x300, Descriptor(0x09, Bytes(4, 0)), { { 0x02, 0x300, Bytes() }, { 0x03, 0x301, Bytes() } })), counter);
  for (const TsPacket& pkt : packets)
  {
    TsPacket copy = pkt;
    p.OnTsPacket(copy.data);
  }
  EXPECT_EQ(c.calls, 1);
  EXPECT_EQ(c.pid, 0x20);
  EXPECT_EQ(c.serviceId, 9);
  EXPECT_EQ(c.pcrPid, 0x300);
  EXPECT_TRUE(c.hasCa);
  EXPECT_EQ(c.infos.size(), 2u);

  // the same table again: already found, nothing more
  for (const TsPacket& pkt : PacketizeSections(0x20, MakeLongSection(2, 9, 0, PmtBody(0x300, Bytes(), {})), counter))
  {
    TsPacket copy = pkt;
    p.OnTsPacket(copy.data);
  }
  EXPECT_EQ(c.calls, 1);
}

TEST(PmtParser, DamagedTablesNeverCrash)
{
  const Bytes valid = MakeLongSection(2, 1, 0, PmtBody(0x100, Descriptor(0x09, Bytes(4, 0)),
    { { 0x1B, 0x100, Bytes() }, { 0x04, 0x101, Iso639("eng") }, { 0x06, 0x102, Concat(Iso639("spa"), Descriptor(0x6A, Bytes())) },
      { 0x06, 0x120, Descriptor(0x59, Bytes{ 'n', 'o', 'r', 0x10, 0, 1, 0, 2 }) } }));
  uint32_t seed = 5;
  auto next = [&seed]() { seed = seed * 1664525u + 1013904223u; return seed >> 8; };
  for (int i = 0; i < 20000; i++)
  {
    Bytes damaged = valid;
    const int changes = 1 + next() % 4;
    for (int c = 0; c < changes; c++)
      damaged[8 + next() % (damaged.size() - 8)] = static_cast<uint8_t>(next());
    CPmtParser p;
    p.SetFilter(0x20, -1);
    int pcr = 0;
    bool ca = false;
    vector<PidInfo2> infos;
    CSection s = AsSection(damaged);
    p.DecodePmt(s, pcr, ca, infos);
  }
}

// ---------------------------------------------------------------------------------------------------------------------------
// CPesDecoder / CPesPacket
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class PesCounter : public CPesCallback
  {
  public:
    int OnNewPesPacket(CPesDecoder*) override { calls++; return 0; }
    int calls = 0;
  };

  // A packet of the PES stream: the first one carries the PES header
  TsPacket PesPacket(unsigned pid, bool start, byte fill, unsigned counter, const std::vector<byte>* header = nullptr)
  {
    TsPacket p = MakePacket(pid, 0x10 | (counter & 0xF), start);
    memset(&p.data[4], fill, 184);
    if (start && header)
      memcpy(&p.data[4], header->data(), header->size());
    return p;
  }
}

TEST(PesDecoder, StreamIdTellsTheKindOfStream)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetStreamId(0xE0);
  EXPECT_TRUE(d->IsVideo());
  EXPECT_FALSE(d->IsAudio());
  d->SetStreamId(0xC0);
  EXPECT_TRUE(d->IsAudio());
  d->SetStreamId(0xBD);
  EXPECT_TRUE(d->IsAc3());
  d->SetStreamId(0xBF);
  EXPECT_TRUE(d->IsAc3());
  d->SetStreamId(0xF0);
  EXPECT_FALSE(d->IsVideo() || d->IsAudio() || d->IsAc3());
  EXPECT_EQ(d->GetStreamId(), 0xF0);
}

TEST(PesDecoder, WaitsForThePayloadUnitStart)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetPid(0x100);
  d->Reset();                                          // the constructor does not set m_bStartFound, see below
  CPcr pcr;
  TsPacket p = PesPacket(0x100, false, 0xAA, 0);
  d->OnTsPacket(p.data, pcr);
  EXPECT_EQ(d->m_packet.InUse(), 0);
  auto h = PesHeader(0xE0, true, true, 90000, 86400);
  p = PesPacket(0x100, true, 0xBB, 1, &h);
  d->OnTsPacket(p.data, pcr);
  EXPECT_EQ(d->m_packet.InUse(), 1);
  EXPECT_TRUE(d->m_packet.IsStart());
}

TEST(PesDecoder, ANewDecoderWaitsForTheStart)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetPid(0x100);
  CPcr pcr;
  TsPacket p = PesPacket(0x100, false, 0xAA, 0);
  d->OnTsPacket(p.data, pcr);
  EXPECT_EQ(d->m_packet.InUse(), 0);
}

TEST(PesDecoder, IgnoresOtherPidsScrambledAndAdaptionOnlyPackets)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetPid(0x100);
  d->Reset();
  CPcr pcr;
  auto h = PesHeader(0xE0, false, false, 0, 0);
  TsPacket p = PesPacket(0x101, true, 0xBB, 0, &h);
  EXPECT_FALSE(d->OnTsPacket(p.data, pcr));
  EXPECT_EQ(d->m_packet.InUse(), 0);

  p = PesPacket(0x100, true, 0xBB, 0, &h);
  p.data[3] |= 0x80;                                   // scrambled
  d->OnTsPacket(p.data, pcr);
  EXPECT_EQ(d->m_packet.InUse(), 0);

  p = MakePacket(0x100, 0x20, true);                   // adaption field only
  p.data[4] = 183;
  d->OnTsPacket(p.data, pcr);
  EXPECT_EQ(d->m_packet.InUse(), 0);

  p = PesPacket(0x100, true, 0xBB, 0, &h);
  p.data[1] |= 0x80;                                   // transport error
  d->OnTsPacket(p.data, pcr);
  EXPECT_EQ(d->m_packet.InUse(), 0);
  d->OnTsPacket(nullptr, pcr);
}

TEST(PesDecoder, CallsBackWhenMoreThanTenPacketsAreWaiting)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetPid(0x100);
  CPcr pcr;
  auto h = PesHeader(0xE0, true, false, 90000, 0);
  for (int i = 0; i < 12; i++)
  {
    TsPacket p = PesPacket(0x100, i == 0, static_cast<byte>(i), i, &h);
    d->OnTsPacket(p.data, pcr);
  }
  EXPECT_EQ(c.calls, 2) << "after the 11th and the 12th packet";
  EXPECT_EQ(d->m_packet.InUse(), 12);
}

TEST(PesDecoder, ReadGivesBackThePayloadInOrder)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetPid(0x100);
  CPcr pcr;
  auto h = PesHeader(0xE0, false, false, 0, 0);
  for (int i = 0; i < 3; i++)
  {
    TsPacket p = PesPacket(0x100, i == 0, static_cast<byte>(0x10 + i), i, &h);
    d->OnTsPacket(p.data, pcr);
  }
  EXPECT_TRUE(d->m_packet.IsAvailable(300));
  std::vector<byte> out(184);
  EXPECT_EQ(d->m_packet.Read(out.data(), 184), 184);
  EXPECT_EQ(out[0], 0);                                    // the PES header starts the first packet
  EXPECT_EQ(out[2], 1);
  EXPECT_EQ(out[183], 0x10);
  EXPECT_EQ(d->m_packet.Read(out.data(), 184), 184);
  EXPECT_EQ(out[0], 0x11);
  EXPECT_EQ(d->m_packet.Read(out.data(), 184), 184);
  EXPECT_EQ(out[100], 0x12);
}

TEST(PesDecoder, PtsDtsAndSequenceHeaderOfTheStart)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetPid(0x100);
  CPcr pcr;
  auto h = PesHeader(0xE0, true, true, 90000, 86400);   // 19 bytes: PES header data length 10, so the payload starts at 19
  h.insert(h.end(), { 0x00, 0x00, 0x01, 0xB3 });         // a video sequence header there
  TsPacket p = PesPacket(0x100, true, 0xFF, 0, &h);
  d->OnTsPacket(p.data, pcr);
  bool pts = false, dts = false;
  d->m_packet.NextPacketHasPtsDts(pts, dts);
  EXPECT_TRUE(pts);
  EXPECT_TRUE(dts);
  EXPECT_TRUE(d->m_packet.HasSequenceHeader());
  EXPECT_EQ(d->m_packet.Pts().PcrReferenceBase, 90000u);
  EXPECT_EQ(d->m_packet.Dts().PcrReferenceBase, 86400u);
}

TEST(PesDecoder, ResetEmptiesThePacketAndWaitsForANewStart)
{
  PesCounter c;
  std::unique_ptr<CPesDecoder> d(new CPesDecoder(&c));
  d->SetPid(0x100);
  CPcr pcr;
  auto h = PesHeader(0xE0, false, false, 0, 0);
  TsPacket p = PesPacket(0x100, true, 0x11, 0, &h);
  d->OnTsPacket(p.data, pcr);
  d->Reset();
  EXPECT_EQ(d->m_packet.InUse(), 0);
  p = PesPacket(0x100, false, 0x12, 1);
  d->OnTsPacket(p.data, pcr);
  EXPECT_EQ(d->m_packet.InUse(), 0) << "a packet without a start is ignored again";
}

TEST(PesDecoder, SequenceHeaderSearchStaysInsideThePayload)
{
  CPesPacket* packet = new CPesPacket();
  GuardedBuffer payload(40, 0);                           // 40 byte payload at the end of a page
  payload.data()[2] = 1;
  payload.data()[3] = 0xE0;
  payload.data()[7] = 0x00;
  payload.data()[8] = 30;                                 // 9 + 30 = 39: the last byte
  CPcr pcr;
  packet->Write(payload.data(), 40, true, pcr);
  delete packet;
}
