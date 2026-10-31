// TsMuxer.ax: the parts of the filter that work on bytes. Hamming 8/4 decoding of teletext, the packet synchroniser, the memory
// buffer between the pins and the program to transport stream converter, the teletext grabber that makes TS packets from VBI
// lines and the analog channel scan that reads the station name from it.
#include <gtest/gtest.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <streams.h>
#include <atomic>
#include <chrono>
#include <thread>
#include <vector>
#include "Hamming.h"
#include "PacketSync.h"
#include "MemoryBuffer.h"
#include "TeletextGrabber.h"
#include "ChannelScan.h"
#include "../unit/GuardedBuffer.h"

extern unsigned char unhamtab[256];
extern unsigned char invtab[256];

// ---------------------------------------------------------------------------------------------------------------------------
// Hamming 8/4
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  // ETS 300 706: bits 0..7 are P1 D1 P2 D2 P3 D3 P4 D4, odd parity
  unsigned char HammingEncode(int nibble)
  {
    const int d1 = nibble & 1, d2 = (nibble >> 1) & 1, d3 = (nibble >> 2) & 1, d4 = (nibble >> 3) & 1;
    const int p1 = 1 ^ d1 ^ d3 ^ d4;
    const int p2 = 1 ^ d1 ^ d2 ^ d4;
    const int p3 = 1 ^ d1 ^ d2 ^ d3;
    int code = p1 | (d1 << 1) | (p2 << 2) | (d2 << 3) | (p3 << 4) | (d3 << 5) | (d4 << 7);
    int ones = 0;
    for (int i = 0; i < 8; i++)
      ones += (code >> i) & 1;
    const int p4 = (ones & 1) ? 0 : 1;                            // odd parity over all eight bits
    return static_cast<unsigned char>(code | (p4 << 6));
  }

  int Distance(int a, int b)
  {
    int x = a ^ b, n = 0;
    while (x) { n += x & 1; x >>= 1; }
    return n;
  }
}

TEST(Hamming, TableAgreesWithTheCodeForEveryByte)
{
  // distance 0: a code word, entry is the nibble with bit 7 set; distance 1: corrected, the nibble; more: 0xFF
  for (int b = 0; b < 256; b++)
  {
    int best = 99, nibble = -1;
    for (int n = 0; n < 16; n++)
    {
      const int d = Distance(b, HammingEncode(n));
      if (d < best) { best = d; nibble = n; }
    }
    const int expected = best == 0 ? (0x80 | nibble) : best == 1 ? nibble : 0xFF;
    EXPECT_EQ(unhamtab[b], expected) << "byte " << b;
  }
}

TEST(Hamming, UnhamJoinsTwoCodeWordsIntoAByte)
{
  for (int value = 0; value < 256; value++)
    EXPECT_EQ(unham(HammingEncode(value & 15), HammingEncode(value >> 4)), value);
}

TEST(Hamming, SingleBitErrorsAreCorrected)
{
  for (int value : { 0x00, 0x5A, 0xF0, 0xFF, 0x37 })
    for (int bit = 0; bit < 8; bit++)
    {
      const unsigned char low = static_cast<unsigned char>(HammingEncode(value & 15) ^ (1 << bit));
      const unsigned char high = static_cast<unsigned char>(HammingEncode(value >> 4) ^ (1 << ((bit + 3) & 7)));
      EXPECT_EQ(unham(low, high), value) << value << " bit " << bit;
    }
}

TEST(Hamming, ADamagedLowNibbleReadsAsF)
{
  // two bit errors: the table says 0xFF; unham() masks the low nibble to F and does not tell the caller
  const unsigned char twoBitsOff = static_cast<unsigned char>(HammingEncode(3) ^ 0x03);
  ASSERT_EQ(unhamtab[twoBitsOff], 0xFF);
  EXPECT_EQ(unham(twoBitsOff, HammingEncode(0)) & 0x0F, 0x0F);
}

// ---------------------------------------------------------------------------------------------------------------------------
// CPacketSync of TsMuxer
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class CountingSync : public CPacketSync
  {
  public:
    void OnTsPacket(byte* p) override
    {
      packets.push_back(std::vector<byte>(p, p + 188));
    }
    std::vector<std::vector<byte>> packets;
  };

  std::vector<byte> Stream(int packets, int firstTag = 0)
  {
    std::vector<byte> s(188 * packets);
    for (int i = 0; i < packets; i++)
    {
      s[i * 188] = 0x47;
      s[i * 188 + 1] = static_cast<byte>(firstTag + i);                 // tag to tell the packets apart
      for (int k = 2; k < 188; k++)
        s[i * 188 + k] = static_cast<byte>(k);
    }
    return s;
  }
}

TEST(MuxerPacketSync, WholePacketsPassStraightThrough)
{
  CountingSync sync;
  auto s = Stream(5);
  sync.OnRawData(s.data(), static_cast<int>(s.size()));
  ASSERT_EQ(sync.packets.size(), 5u);
  for (int i = 0; i < 5; i++)
    EXPECT_EQ(sync.packets[i][1], i);
}

TEST(MuxerPacketSync, PacketsSplitAtAnyPlaceAreJoined)
{
  for (int cut = 1; cut < 188 * 2; cut += 7)
  {
    CountingSync sync;
    auto s = Stream(6);
    sync.OnRawData(s.data(), cut);
    sync.OnRawData(s.data() + cut, static_cast<int>(s.size()) - cut);
    ASSERT_EQ(sync.packets.size(), 6u) << "cut " << cut;
    for (int i = 0; i < 6; i++)
      ASSERT_EQ(sync.packets[i][1], i) << "cut " << cut;
  }
}

TEST(MuxerPacketSync, GarbageInFrontIsSkipped)
{
  CountingSync sync;
  std::vector<byte> data(100, 0x11);
  auto s = Stream(3);
  data.insert(data.end(), s.begin(), s.end());
  sync.OnRawData(data.data(), static_cast<int>(data.size()));
  ASSERT_EQ(sync.packets.size(), 3u);
  EXPECT_EQ(sync.packets[0][1], 0);
}

TEST(MuxerPacketSync, SingleBytesAreEnough)
{
  CountingSync sync;
  auto s = Stream(3);
  for (byte b : s)
    sync.OnRawData(&b, 1);
  EXPECT_EQ(sync.packets.size(), 3u);
}

TEST(MuxerPacketSync, ABufferEndingExactlyOnAPacketLeavesNothingBehind)
{
  CountingSync sync;
  auto s = Stream(2);
  sync.OnRawData(s.data(), 188 * 2);
  ASSERT_EQ(sync.packets.size(), 2u);
  auto t = Stream(2, 10);
  sync.OnRawData(t.data(), 188 * 2);
  ASSERT_EQ(sync.packets.size(), 4u);
  EXPECT_EQ(sync.packets[2][1], 10);
}

TEST(MuxerPacketSync, ShortPiecesAreNotReadPastTheirEnd)
{
  CountingSync sync;
  auto s = Stream(1);
  sync.OnRawData(s.data(), 100);
  GuardedBuffer rest(10);
  memcpy(rest.data(), s.data() + 100, 10);
  sync.OnRawData(rest.data(), 10);
  EXPECT_TRUE(sync.packets.empty()) << "78 bytes are still missing";
}

// ---------------------------------------------------------------------------------------------------------------------------
// CMemoryBuffer
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class Recorder : public IMemoryCallback
  {
  public:
    void OnRawDataReceived(BYTE* data, long length) override { lengths.push_back(length); first.push_back(length > 0 ? data[0] : 0); }
    std::vector<long> lengths;
    std::vector<int> first;
  };

  std::vector<BYTE> Pattern(size_t n, BYTE seed = 0)
  {
    std::vector<BYTE> v(n);
    for (size_t i = 0; i < n; i++)
      v[i] = static_cast<BYTE>(seed + i);
    return v;
  }
}

TEST(MuxerMemoryBuffer, StartsEmpty)
{
  CMemoryBuffer b;
  EXPECT_EQ(b.Size(), 0u);
}

TEST(MuxerMemoryBuffer, ReadsWhatWasPutInOrder)
{
  CMemoryBuffer b;
  auto a = Pattern(100, 1), c = Pattern(50, 200);
  EXPECT_EQ(b.PutBuffer(a.data(), 100, 0), S_OK);
  EXPECT_EQ(b.PutBuffer(c.data(), 50, 0), S_OK);
  EXPECT_EQ(b.Size(), 150u);
  std::vector<BYTE> out(150);
  EXPECT_EQ(b.ReadFromBuffer(out.data(), 150, 0), 150u);
  EXPECT_EQ(0, memcmp(out.data(), a.data(), 100));
  EXPECT_EQ(0, memcmp(out.data() + 100, c.data(), 50));
  EXPECT_EQ(b.Size(), 0u);
}

TEST(MuxerMemoryBuffer, ReadsInPiecesThatDoNotMatchThePuts)
{
  CMemoryBuffer b;
  auto a = Pattern(30, 0), c = Pattern(30, 30), d = Pattern(30, 60);
  b.PutBuffer(a.data(), 30, 0);
  b.PutBuffer(c.data(), 30, 0);
  b.PutBuffer(d.data(), 30, 0);
  std::vector<BYTE> out;
  for (int piece : { 7, 31, 1, 25, 26 })
  {
    std::vector<BYTE> p(piece);
    ASSERT_EQ(b.ReadFromBuffer(p.data(), piece, 0), static_cast<DWORD>(piece));
    out.insert(out.end(), p.begin(), p.end());
  }
  EXPECT_EQ(out, Pattern(90, 0));
  EXPECT_EQ(b.Size(), 0u);
}

TEST(MuxerMemoryBuffer, TheOffsetOfPutBufferSkipsTheStart)
{
  CMemoryBuffer b;
  auto a = Pattern(20, 0);
  EXPECT_EQ(b.PutBuffer(a.data(), 20, 5), S_OK);
  EXPECT_EQ(b.Size(), 15u);
  std::vector<BYTE> out(15);
  b.ReadFromBuffer(out.data(), 15, 0);
  EXPECT_EQ(out, Pattern(15, 5));
}

TEST(MuxerMemoryBuffer, PutRefusesWhatCannotBeStored)
{
  CMemoryBuffer b;
  auto a = Pattern(10);
  EXPECT_EQ(b.PutBuffer(a.data(), 0, 0), E_FAIL);
  EXPECT_EQ(b.PutBuffer(a.data(), -5, 0), E_FAIL);
  EXPECT_EQ(b.PutBuffer(a.data(), 10, -1), E_FAIL);
  EXPECT_EQ(b.PutBuffer(nullptr, 10, 0), E_FAIL);
  EXPECT_EQ(b.Size(), 0u);
}

TEST(MuxerMemoryBuffer, OffsetBeyondTheLengthIsRefused)
{
  CMemoryBuffer b;
  auto a = Pattern(10);
  EXPECT_EQ(b.PutBuffer(a.data(), 10, 20), E_FAIL);
  EXPECT_EQ(b.Size(), 0u);
}

TEST(MuxerMemoryBuffer, CallbackGetsTheLengthBehindTheOffset)
{
  CMemoryBuffer b;
  Recorder r;
  b.SetCallback(&r);
  auto a = Pattern(20);
  b.PutBuffer(a.data(), 20, 5);
  ASSERT_EQ(r.lengths.size(), 1u);
  EXPECT_EQ(r.lengths[0], 15);
  EXPECT_EQ(r.first[0], 5);
}

TEST(MuxerMemoryBuffer, CallbackSeesEveryPut)
{
  CMemoryBuffer b;
  Recorder r;
  b.SetCallback(&r);
  auto a = Pattern(20, 9), c = Pattern(10, 77);
  b.PutBuffer(a.data(), 20, 0);
  b.PutBuffer(c.data(), 10, 0);
  EXPECT_EQ(r.lengths, (std::vector<long>{ 20, 10 }));
  EXPECT_EQ(r.first, (std::vector<int>{ 9, 77 }));
}

TEST(MuxerMemoryBuffer, TheOldestDataIsDroppedAboveHalfAMegabyte)
{
  CMemoryBuffer b;
  auto chunk = Pattern(100000, 1);
  for (int i = 0; i < 8; i++)
    b.PutBuffer(chunk.data(), 100000, 0);
  EXPECT_LE(b.Size(), 500000u);
  EXPECT_GE(b.Size(), 400000u);
}

TEST(MuxerMemoryBuffer, ClearEmptiesTheBuffer)
{
  CMemoryBuffer b;
  auto a = Pattern(100);
  b.PutBuffer(a.data(), 100, 0);
  b.Clear();
  EXPECT_EQ(b.Size(), 0u);
}

TEST(MuxerMemoryBuffer, NegativeReadLengthGivesNothing)
{
  CMemoryBuffer b;
  BYTE out[4];
  EXPECT_EQ(b.ReadFromBuffer(out, -1, 0), 0u);
}

TEST(MuxerMemoryBuffer, ReadWaitsForDataThatComesLater)
{
  CMemoryBuffer b;
  std::atomic<DWORD> got(999);
  std::vector<BYTE> out(40);
  std::thread reader([&]() { got = b.ReadFromBuffer(out.data(), 40, 0); });
  std::this_thread::sleep_for(std::chrono::milliseconds(150));
  EXPECT_EQ(got.load(), 999u) << "the reader should still wait";
  auto a = Pattern(40, 3);
  b.PutBuffer(a.data(), 40, 0);
  reader.join();
  EXPECT_EQ(got.load(), 40u);
  EXPECT_EQ(out, a);
}

TEST(MuxerMemoryBuffer, StopReleasesAWaitingReader)
{
  CMemoryBuffer b;
  std::atomic<DWORD> got(999);
  BYTE out[40];
  std::thread reader([&]() { got = b.ReadFromBuffer(out, 40, 0); });
  std::this_thread::sleep_for(std::chrono::milliseconds(150));
  b.Stop();
  reader.join();
  EXPECT_EQ(got.load(), 0u);
}

TEST(MuxerMemoryBuffer, StopThrowsAwayWhatIsInTheBuffer)
{
  CMemoryBuffer b;
  auto a = Pattern(100);
  b.PutBuffer(a.data(), 100, 0);
  b.Stop();
  EXPECT_EQ(b.Size(), 0u);
  BYTE out[10];
  EXPECT_EQ(b.ReadFromBuffer(out, 10, 0), 0u);
}

// ---------------------------------------------------------------------------------------------------------------------------
// CTeletextGrabber
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class TsCollector : public IAnalogTeletextCallBack
  {
  public:
    STDMETHODIMP QueryInterface(REFIID, void**) override { return E_NOINTERFACE; }
    STDMETHODIMP_(ULONG) AddRef() override { return 2; }
    STDMETHODIMP_(ULONG) Release() override { return 1; }
    STDMETHODIMP OnTeletextReceived(BYTE* data, int packetCounts) override
    {
      calls++;
      counts.push_back(packetCounts);
      bytes.assign(data, data + packetCounts * 188);
      return S_OK;
    }
    int calls = 0;
    std::vector<int> counts;
    std::vector<BYTE> bytes;
  };

  // 43 bytes of a VBI line as the driver delivers it: bit reversed
  std::vector<BYTE> VbiLine(BYTE tag)
  {
    std::vector<BYTE> line(43);
    for (int i = 0; i < 43; i++)
      line[i] = invtab[static_cast<BYTE>(tag + i)];
    line[0] = invtab[static_cast<BYTE>(tag)];
    return line;
  }
}

TEST(TeletextGrabber, InverseTableReversesTheBitOrder)
{
  for (int v = 0; v < 256; v++)
  {
    int r = 0;
    for (int bit = 0; bit < 8; bit++)
      if (v & (1 << bit))
        r |= 0x80 >> bit;
    EXPECT_EQ(invtab[v], r) << v;
  }
}

TEST(TeletextGrabber, DoesNothingUntilItIsStartedAndHasACallback)
{
  CTeletextGrabber g;
  TsCollector c;
  auto line = VbiLine(1);
  std::vector<BYTE> many;
  for (int i = 0; i < 120; i++)
    many.insert(many.end(), line.begin(), line.end());
  g.SetCallBack(&c);
  g.OnSampleReceived(many.data(), static_cast<int>(many.size()));       // not started
  EXPECT_EQ(c.calls, 0);

  CTeletextGrabber g2;
  g2.Start();
  g2.OnSampleReceived(many.data(), static_cast<int>(many.size()));     // no callback
  SUCCEED();
}

TEST(TeletextGrabber, TwentyFiveTsPacketsMakeOneCallback)
{
  CTeletextGrabber g;
  TsCollector c;
  g.SetCallBack(&c);
  g.Start();
  // 100 lines = 25 packets of 4 lines; the last line is only used when more data follows (the loop wants more than a line)
  std::vector<BYTE> data;
  for (int i = 0; i < 101; i++)
  {
    auto line = VbiLine(static_cast<BYTE>(i + 1));
    data.insert(data.end(), line.begin(), line.end());
  }
  g.OnSampleReceived(data.data(), static_cast<int>(data.size()));
  ASSERT_EQ(c.calls, 1);
  EXPECT_EQ(c.counts[0], 25);
  ASSERT_EQ(c.bytes.size(), 25u * 188);

  for (int p = 0; p < 25; p++)
  {
    const BYTE* ts = &c.bytes[p * 188];
    EXPECT_EQ(ts[0], 0x47) << p;
    EXPECT_EQ(ts[1], 0x70) << p;
    EXPECT_EQ(ts[2], 0x00) << p;
    EXPECT_EQ(ts[3], 0x00) << p;
    for (int line = 0; line < 4; line++)
    {
      const BYTE* du = ts + 4 + line * 46;
      EXPECT_EQ(du[0], 0x02);                        // data_unit_id: EBU teletext
      EXPECT_EQ(du[1], 0x2C);                        // data_unit_length
      EXPECT_EQ(du[2], 0x00);                        // field / line
      EXPECT_EQ(du[3], 0xE4);                        // framing code
    }
  }
  // the first line of the first packet: 42 bytes of the VBI line with the bit order turned back (the driver delivers it reversed)
  const BYTE* first = &c.bytes[4 + 4];
  EXPECT_EQ(first[0], 1);
  EXPECT_EQ(first[1], 2);
}

TEST(TeletextGrabber, LinesThatStartWithFiveZeroBytesAreSkipped)
{
  CTeletextGrabber g;
  TsCollector c;
  g.SetCallBack(&c);
  g.Start();
  std::vector<BYTE> zeros(43 * 200, 0);
  g.OnSampleReceived(zeros.data(), static_cast<int>(zeros.size()));
  EXPECT_EQ(c.calls, 0);
}

TEST(TeletextGrabber, StopAndStartAgainBeginsANewPacket)
{
  CTeletextGrabber g;
  TsCollector c;
  g.SetCallBack(&c);
  g.Start();
  auto line = VbiLine(5);
  std::vector<BYTE> some;
  for (int i = 0; i < 50; i++)
    some.insert(some.end(), line.begin(), line.end());
  g.OnSampleReceived(some.data(), static_cast<int>(some.size()));
  g.Stop();
  g.OnSampleReceived(some.data(), static_cast<int>(some.size()));
  g.Start();
  std::vector<BYTE> more;
  for (int i = 0; i < 101; i++)
    more.insert(more.end(), line.begin(), line.end());
  g.OnSampleReceived(more.data(), static_cast<int>(more.size()));
  EXPECT_EQ(c.calls, 1);
}

TEST(TeletextGrabber, LargestSampleThatFitsTheBuffer)
{
  // 20000 bytes of buffer: with up to 43 bytes waiting from before, a sample of 19957 still fits
  CTeletextGrabber g;
  TsCollector c;
  g.SetCallBack(&c);
  g.Start();
  GuardedBuffer sample(19957, 0x33);
  g.OnSampleReceived(sample.data(), 19957);
  SUCCEED();
}

TEST(TeletextGrabber, SamplesLargerThanTheBufferAreRefused)
{
  CTeletextGrabber g;
  TsCollector c;
  g.SetCallBack(&c);
  g.Start();
  std::vector<BYTE> big(25000, 0x33);
  g.OnSampleReceived(big.data(), static_cast<int>(big.size()));
}

// ---------------------------------------------------------------------------------------------------------------------------
// CChannelScan
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  class ScanDone : public IAnalogChannelScanCallback
  {
  public:
    STDMETHODIMP QueryInterface(REFIID, void**) override { return E_NOINTERFACE; }
    STDMETHODIMP_(ULONG) AddRef() override { return 2; }
    STDMETHODIMP_(ULONG) Release() override { return 1; }
    STDMETHODIMP OnScannerDone() override { done++; return S_OK; }
    int done = 0;
  };

  // A teletext line: two Hamming coded bytes with magazine and packet number, then the data; for packet 30 of magazine 8 the
  // station name is in bytes 22 to 41.
  std::vector<BYTE> TeletextLine(int magazine, int packet, const char* name)
  {
    std::vector<BYTE> line(43, 0x20);
    const int value = (packet << 3) | (magazine & 7);
    line[0] = HammingEncode(value & 15);
    line[1] = HammingEncode(value >> 4);
    for (int i = 0; i < 20 && name[i]; i++)
      line[22 + i] = static_cast<BYTE>(name[i]);
    return line;
  }

  std::vector<BYTE> Lines(std::initializer_list<std::vector<BYTE>> lines)
  {
    std::vector<BYTE> all;
    for (const auto& l : lines)
      all.insert(all.end(), l.begin(), l.end());
    return all;
  }
}

TEST(ChannelScan, FindsTheStationNameInPacket30OfMagazine8)
{
  HRESULT hr = S_OK;
  CChannelScan scan(nullptr, &hr);
  ScanDone done;
  scan.SetCallBack(&done);
  scan.Start();
  auto data = Lines({ TeletextLine(1, 0, ""), TeletextLine(0, 30, "ZDF HD              "), TeletextLine(1, 5, "") });
  scan.OnTeletextData(data.data(), static_cast<int>(data.size()));
  BOOL ready = FALSE;
  scan.IsReady(&ready);
  EXPECT_TRUE(ready);
  EXPECT_EQ(done.done, 1);
  char* name = nullptr;
  scan.GetChannel(&name);
  ASSERT_NE(name, nullptr);
  EXPECT_EQ(std::string(name, 6), "ZDF HD");
}

TEST(ChannelScan, OtherMagazinesAndPacketsAreNotTheStationName)
{
  HRESULT hr = S_OK;
  CChannelScan scan(nullptr, &hr);
  ScanDone done;
  scan.SetCallBack(&done);
  scan.Start();
  auto data = Lines({ TeletextLine(1, 30, "WRONG MAGAZINE      "), TeletextLine(0, 29, "WRONG PACKET        "),
                      TeletextLine(0, 0, "NO NAME             "), TeletextLine(2, 1, "") });
  scan.OnTeletextData(data.data(), static_cast<int>(data.size()));
  BOOL ready = TRUE;
  scan.IsReady(&ready);
  EXPECT_FALSE(ready);
  EXPECT_EQ(done.done, 0);
}

TEST(ChannelScan, NothingHappensWhileItIsNotScanning)
{
  HRESULT hr = S_OK;
  CChannelScan scan(nullptr, &hr);
  ScanDone done;
  scan.SetCallBack(&done);
  scan.Stop();                                       // sets m_bChannelFound, which the constructor leaves undefined
  auto data = Lines({ TeletextLine(0, 30, "IGNORED             "), TeletextLine(1, 1, "") });
  scan.OnTeletextData(data.data(), static_cast<int>(data.size()));
  BOOL ready = TRUE;
  scan.IsReady(&ready);
  EXPECT_FALSE(ready);
}

TEST(ChannelScan, StartForgetsTheLastStation)
{
  HRESULT hr = S_OK;
  CChannelScan scan(nullptr, &hr);
  ScanDone done;
  scan.SetCallBack(&done);
  scan.Start();
  auto data = Lines({ TeletextLine(0, 30, "FIRST               "), TeletextLine(1, 1, "") });
  scan.OnTeletextData(data.data(), static_cast<int>(data.size()));
  scan.Stop();
  scan.Start();
  BOOL ready = TRUE;
  scan.IsReady(&ready);
  EXPECT_FALSE(ready);
  char* name = nullptr;
  scan.GetChannel(&name);
  EXPECT_EQ(name[0], ' ');
}

TEST(ChannelScan, TheNameKeepsOnlySevenBits)
{
  HRESULT hr = S_OK;
  CChannelScan scan(nullptr, &hr);
  ScanDone done;
  scan.SetCallBack(&done);
  scan.Start();
  auto line = TeletextLine(0, 30, "ABC");
  line[22] = 'A' | 0x80;                             // parity bit set
  auto data = Lines({ line, TeletextLine(1, 1, "") });
  scan.OnTeletextData(data.data(), static_cast<int>(data.size()));
  char* name = nullptr;
  scan.GetChannel(&name);
  EXPECT_EQ(name[0], 'A');
}

TEST(ChannelScan, DataThatArrivesInSmallPiecesIsJoined)
{
  HRESULT hr = S_OK;
  CChannelScan scan(nullptr, &hr);
  ScanDone done;
  scan.SetCallBack(&done);
  scan.Start();
  auto data = Lines({ TeletextLine(0, 30, "PIECES              "), TeletextLine(1, 1, "") });
  for (size_t i = 0; i < data.size(); i += 5)
    scan.OnTeletextData(data.data() + i, static_cast<int>((std::min<size_t>)(5, data.size() - i)));
  BOOL ready = FALSE;
  scan.IsReady(&ready);
  EXPECT_TRUE(ready);
}

TEST(ChannelScan, IsNotReadyBeforeTheFirstStart)
{
  HRESULT hr = S_OK;
  CChannelScan* scan = new CChannelScan(nullptr, &hr);
  BOOL ready = TRUE;
  scan->IsReady(&ready);
  EXPECT_FALSE(ready);
  delete scan;
}

TEST(ChannelScan, StationNameWithoutACallbackIsFine)
{
  HRESULT hr = S_OK;
  CChannelScan* scan = new CChannelScan(nullptr, &hr);
  scan->Start();
  auto data = Lines({ TeletextLine(0, 30, "NO CALLBACK         "), TeletextLine(1, 1, "") });
  scan->OnTeletextData(data.data(), static_cast<int>(data.size()));
  delete scan;
}
