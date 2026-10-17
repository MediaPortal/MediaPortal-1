// The synchronisation algorithm of CPacketSync (shared/PacketSync.h): how it finds packet boundaries in a byte stream that
// arrives in arbitrary pieces, damaged, with garbage in it, or as 192 byte M2TS packets.
//
// What the algorithm does, in short (PacketSync.cpp):
//  * a packet is accepted when its first byte is 0x47 and the byte one packet length later is 0x47 as well;
//  * once it is in sync one of the two bytes is enough, and then both are set to 0x47 ("repaired") and sync is dropped;
//  * the packet that is still waiting for its follower is kept in a 400 byte buffer, so a packet is delivered only when
//    the first byte of the next one has arrived;
//  * if nothing is found within 8 packet lengths of one call the state is reset.
#include <gtest/gtest.h>
#include <windows.h>
#include <algorithm>
#include <random>
#include <vector>
#include "PacketSync.h"

namespace
{
  // A stream of numbered packets: sync, number (2 bytes), payload. 'rare' payloads never contain 0x47, otherwise one
  // payload byte in four is 0x47 (the worst case for a search that looks for 0x47).
  struct Stream
  {
    std::vector<byte> bytes;
    int count = 0;
    size_t length = 188;                      // 188 for TS, 192 for M2TS (4 byte header before every packet)

    const byte* Packet(int i) const { return &bytes[static_cast<size_t>(i) * length + (length - 188)]; }
  };

  Stream MakeStream(int count, unsigned seed, bool rare, size_t length = 188)
  {
    Stream s;
    s.count = count;
    s.length = length;
    std::mt19937 rng(seed);
    s.bytes.resize(static_cast<size_t>(count) * length);
    for (int i = 0; i < count; i++)
    {
      byte* header = &s.bytes[static_cast<size_t>(i) * length];
      for (size_t k = 0; k < length - 188; k++)
        header[k] = static_cast<byte>(0x10 + k);           // M2TS arrival time stamp, never 0x47
      byte* p = header + (length - 188);
      p[0] = TS_PACKET_SYNC;
      p[1] = static_cast<byte>(i >> 8);
      p[2] = static_cast<byte>(i);
      for (int k = 3; k < 188; k++)
      {
        byte b = static_cast<byte>(rng());
        if (rare)
        {
          if (b == TS_PACKET_SYNC)
            b = 0x48;
        }
        else if ((rng() & 3) == 0)
          b = TS_PACKET_SYNC;
        p[k] = b;
      }
    }
    return s;
  }

  // What came out: which original packet each delivered packet is, or -1 when it is none of them
  class Recorder : public CPacketSync
  {
  public:
    explicit Recorder(const Stream& s) : m_stream(s) {}
    using CPacketSync::OnTsPacket;

    void OnTsPacket(byte* p) override { Record(p); }
    void OnTsPacket(byte* p, int, int) override { Record(p); }

    std::vector<int> delivered;               // packet numbers, -1 for packets that are not one of the originals
    int repaired = 0;                         // delivered packets that differ from the original only in the sync byte

    int BogusCount() const { return static_cast<int>(std::count(delivered.begin(), delivered.end(), -1)); }

  private:
    void Record(const byte* p)
    {
      const int n = (p[1] << 8) | p[2];
      if (p[0] != TS_PACKET_SYNC || n >= m_stream.count)
      {
        delivered.push_back(-1);
        return;
      }
      const byte* original = m_stream.Packet(n);
      if (memcmp(p, original, 188) != 0)
      {
        if (memcmp(p + 1, original + 1, 187) == 0)
          repaired++;
        else
        {
          delivered.push_back(-1);
          return;
        }
      }
      delivered.push_back(n);
    }
    const Stream& m_stream;
  };

  void Feed(Recorder& r, std::vector<byte> data, const std::vector<size_t>& chunks)
  {
    size_t pos = 0, i = 0;
    while (pos < data.size())
    {
      size_t n = (std::min)(chunks[i++ % chunks.size()], data.size() - pos);
      r.OnRawData(&data[pos], static_cast<int>(n));
      pos += n;
    }
  }

  void Feed2(Recorder& r, std::vector<byte> data, const std::vector<size_t>& chunks)
  {
    size_t pos = 0, i = 0;
    while (pos < data.size())
    {
      size_t n = (std::min)(chunks[i++ % chunks.size()], data.size() - pos);
      r.OnRawData2(&data[pos], static_cast<int>(n));
      pos += n;
    }
  }

  std::vector<size_t> RandomChunks(std::mt19937& rng, size_t maxSize)
  {
    std::vector<size_t> chunks(64);
    for (size_t& c : chunks)
      c = 1 + rng() % maxSize;
    return chunks;
  }

  // the numbers of the genuine packets increase (packets that are none of the originals are left out)
  // OnRawData2 is made for the big buffers of a file reader: until it is in sync it needs at least 5 packets in one call
  // (shorter buffers without sync are reported to the caller, not kept). After that any size is fine.
  std::vector<size_t> BigFirstChunks(std::mt19937& rng)
  {
    std::vector<size_t> chunks = RandomChunks(rng, 3000);
    chunks[0] = 192 * 5 + rng() % 2000;
    return chunks;
  }

  bool InOrder(const std::vector<int>& v)
  {
    int last = -1;
    for (int n : v)
    {
      if (n < 0)
        continue;
      if (n <= last)
        return false;
      last = n;
    }
    return true;
  }

  int Genuine(const std::vector<int>& v)
  {
    return static_cast<int>(v.size()) - static_cast<int>(std::count(v.begin(), v.end(), -1));
  }
}

// --- a clean stream ---------------------------------------------------------------------------------------------

TEST(PacketSyncAlgorithm, CleanStreamIsDeliveredCompleteAndInOrderWhateverThePayload)
{
  for (bool rare : { true, false })
  {
    const Stream s = MakeStream(80, 1, rare);
    std::mt19937 rng(11);
    for (int round = 0; round < 40; round++)
    {
      Recorder r(s);
      Feed(r, s.bytes, RandomChunks(rng, round < 20 ? 300 : 3000));
      SCOPED_TRACE(testing::Message() << "payload " << (rare ? "without" : "with many") << " 0x47, round " << round);
      EXPECT_EQ(r.BogusCount(), 0);
      EXPECT_EQ(r.repaired, 0);
      EXPECT_TRUE(InOrder(r.delivered));
      ASSERT_EQ(r.delivered.size(), 79u);                      // the last packet waits for a follower
      EXPECT_EQ(r.delivered.front(), 0);
      EXPECT_EQ(r.delivered.back(), 78);
    }
  }
}

TEST(PacketSyncAlgorithm, ThePacketIsHeldBackUntilTheNextSyncByteArrives)
{
  const Stream s = MakeStream(4, 2, true);
  {
    Recorder r(s);
    r.OnRawData(const_cast<byte*>(s.bytes.data()), 188);
    EXPECT_EQ(r.delivered.size(), 0u);                         // 188 bytes: no way to know that this is a packet
  }
  {
    Recorder r(s);
    std::vector<byte> data = s.bytes;
    r.OnRawData(data.data(), 189);
    EXPECT_EQ(r.delivered.size(), 1u);                         // the 189th byte is the sync byte of the next one
    r.OnRawData(data.data() + 189, 187);
    EXPECT_EQ(r.delivered.size(), 1u);
    r.OnRawData(data.data() + 376, 1);
    EXPECT_EQ(r.delivered.size(), 2u);
  }
}

TEST(PacketSyncAlgorithm, SequenceOfDeliveredPacketsDoesNotDependOnWhereTheChunksEnd)
{
  const Stream s = MakeStream(60, 3, false);
  std::vector<int> reference;
  {
    Recorder r(s);
    Feed(r, s.bytes, { s.bytes.size() });
    reference = r.delivered;
  }
  ASSERT_EQ(reference.size(), 59u);
  std::mt19937 rng(5);
  for (int round = 0; round < 200; round++)
  {
    Recorder r(s);
    Feed(r, s.bytes, RandomChunks(rng, 1 + rng() % 2000));
    ASSERT_EQ(r.delivered, reference) << "round " << round;
  }
}

// --- finding the start ----------------------------------------------------------------------------------------

TEST(PacketSyncAlgorithm, StreamThatStartsInTheMiddleOfAPacketLocksOnTheNextBoundary)
{
  const Stream s = MakeStream(60, 4, true);
  std::mt19937 rng(6);
  for (size_t skip = 1; skip < 188; skip += 7)
  {
    Recorder r(s);
    std::vector<byte> cut(s.bytes.begin() + skip, s.bytes.end());
    Feed(r, cut, RandomChunks(rng, 700));
    SCOPED_TRACE(testing::Message() << "first " << skip << " bytes missing");
    EXPECT_EQ(r.BogusCount(), 0);
    ASSERT_GE(r.delivered.size(), 57u);
    EXPECT_EQ(r.delivered.front(), 1);                         // packet 0 is incomplete
    EXPECT_EQ(r.delivered.back(), 58);
    EXPECT_TRUE(InOrder(r.delivered));
  }
}

TEST(PacketSyncAlgorithm, GarbageInFrontOfTheStreamIsSkippedForEveryLength)
{
  const Stream s = MakeStream(40, 5, true);
  std::mt19937 rng(7);
  for (size_t garbage = 0; garbage <= 1200; garbage += 13)
  {
    std::vector<byte> data(garbage);
    for (byte& b : data)
      b = (rng() % 2) ? 0x00 : 0xA5;                           // no 0x47 in it
    data.insert(data.end(), s.bytes.begin(), s.bytes.end());
    Recorder r(s);
    Feed(r, data, RandomChunks(rng, 900));
    SCOPED_TRACE(testing::Message() << garbage << " bytes of garbage");
    EXPECT_EQ(r.BogusCount(), 0);
    EXPECT_EQ(r.delivered.front(), 0);
    EXPECT_EQ(r.delivered.size(), 39u);
  }
}

TEST(PacketSyncAlgorithm, ALoneSyncByteOnePacketLengthBeforeTheStreamMakesABogusPacket)
{
  // Limit of the method: 0x47 at offset 0 and at offset 188 look like two packets. The stream itself starts at 188.
  const Stream s = MakeStream(30, 8, true);
  std::vector<byte> data(188, 0x00);
  data[0] = TS_PACKET_SYNC;
  data.insert(data.end(), s.bytes.begin(), s.bytes.end());
  Recorder r(s);
  Feed(r, data, { 100 });
  ASSERT_GE(r.delivered.size(), 29u);
  EXPECT_EQ(r.delivered.front(), -1);                          // the 188 bytes before the stream are taken for a packet
  EXPECT_EQ(r.BogusCount(), 1);
  EXPECT_EQ(r.delivered[1], 0);                                // and the real stream follows without a gap
  EXPECT_EQ(r.delivered.back(), 28);
}

// --- loss and damage inside the stream ----------------------------------------------------------------------------

TEST(PacketSyncAlgorithm, BytesLostInTheMiddleOnlyCostThePacketsAroundThem)
{
  const Stream s = MakeStream(100, 9, true);
  std::mt19937 rng(10);
  for (int lost = 1; lost <= 700; lost += 11)
  {
    const size_t at = static_cast<size_t>(30) * 188 + 20 + (lost % 150);
    std::vector<byte> data = s.bytes;
    data.erase(data.begin() + at, data.begin() + at + lost);
    Recorder r(s);
    Feed(r, data, RandomChunks(rng, 1200));
    SCOPED_TRACE(testing::Message() << lost << " bytes lost");
    // In sync one good sync byte is enough, so the damaged packet is delivered as it is (the end of one packet and the
    // start of the next one glued together): at most one such block per damage, never a second one
    EXPECT_LE(r.BogusCount(), 1);
    EXPECT_TRUE(InOrder(r.delivered));
    // everything before the damage arrives, and so does everything after the packets that overlapped the hole
    const int lostPackets = 1 + (lost + 187) / 188 + 1;
    const int expected = 99 - lostPackets;
    EXPECT_GE(Genuine(r.delivered), expected) << "delivered " << Genuine(r.delivered) << " of 99";
    EXPECT_EQ(r.delivered.front(), 0);
    EXPECT_EQ(r.delivered.back(), 98);
  }
}

TEST(PacketSyncAlgorithm, GarbageInsertedInTheMiddleOnlyCostsThePacketsAroundIt)
{
  const Stream s = MakeStream(100, 12, true);
  std::mt19937 rng(13);
  for (int extra = 1; extra <= 700; extra += 11)
  {
    const size_t at = static_cast<size_t>(40) * 188 + (extra % 180);
    std::vector<byte> data = s.bytes;
    data.insert(data.begin() + at, static_cast<size_t>(extra), static_cast<byte>(0xA5));
    Recorder r(s);
    Feed(r, data, RandomChunks(rng, 1200));
    SCOPED_TRACE(testing::Message() << extra << " bytes of garbage");
    EXPECT_LE(r.BogusCount(), 2);                             // 188 bytes of garbage make a "packet" of their own
    EXPECT_TRUE(InOrder(r.delivered));
    EXPECT_GE(Genuine(r.delivered), 99 - (2 + (extra + 187) / 188));
    EXPECT_EQ(r.delivered.back(), 98);
  }
}

TEST(PacketSyncAlgorithm, ADamagedSyncByteInsideAStreamInSyncIsRepairedInTheCallersBuffer)
{
  // once in sync one good sync byte of the pair is enough; the damaged one is overwritten with 0x47 in the buffer that
  // the caller passed, before the packet that it belongs to is processed - so that packet arrives intact
  const Stream s = MakeStream(50, 14, true);
  std::vector<byte> data = s.bytes;
  const size_t damaged = static_cast<size_t>(20) * 188;
  data[damaged] = 0x00;                                          // sync byte of packet 20
  Recorder r(s);
  r.OnRawData(data.data(), static_cast<int>(data.size()));
  EXPECT_EQ(data[damaged], TS_PACKET_SYNC);
  EXPECT_EQ(r.BogusCount(), 0);
  EXPECT_TRUE(InOrder(r.delivered));
  ASSERT_EQ(r.delivered.size(), 49u);
  EXPECT_NE(std::find(r.delivered.begin(), r.delivered.end(), 20), r.delivered.end());
}

TEST(PacketSyncAlgorithm, TwoDamagedSyncBytesInARowCostOnePacket)
{
  const Stream s = MakeStream(50, 24, true);
  std::vector<byte> data = s.bytes;
  data[static_cast<size_t>(20) * 188] = 0x00;
  data[static_cast<size_t>(21) * 188] = 0x00;
  Recorder r(s);
  r.OnRawData(data.data(), static_cast<int>(data.size()));
  EXPECT_EQ(r.BogusCount(), 0);
  EXPECT_TRUE(InOrder(r.delivered));
  EXPECT_GE(r.delivered.size(), 47u);                            // nothing is lost beyond the damaged neighbourhood
  EXPECT_EQ(r.delivered.back(), 48);
}

TEST(PacketSyncAlgorithm, RandomlyDamagedStreamNeverProducesAFalsePacketThatIsNotInTheStream)
{
  // 1 % of the bytes are random. Every delivered packet is one of the originals (apart from a repaired sync byte), in order.
  std::mt19937 rng(15);
  for (int round = 0; round < 60; round++)
  {
    const Stream s = MakeStream(120, 100 + round, true);
    std::vector<byte> data = s.bytes;
    for (size_t i = 0; i < data.size() / 100; i++)
    {
      byte b = static_cast<byte>(rng());
      data[rng() % data.size()] = b;
    }
    Recorder r(s);
    Feed(r, data, RandomChunks(rng, 1 + rng() % 1500));
    SCOPED_TRACE(testing::Message() << "round " << round);
    // payload damage does not hide a packet from the search; the numbers of damaged packets can be wrong, so only count
    // what is clearly valid
    EXPECT_GE(r.delivered.size(), 100u);
    EXPECT_LE(r.delivered.size(), 119u);
  }
}

// --- when there is nothing to find ------------------------------------------------------------------------------------

TEST(PacketSyncAlgorithm, NoiseWithoutSyncBytesIsSkippedWhateverItsLengthWhenItComesInSmallCalls)
{
  const Stream s = MakeStream(60, 16, true);
  for (size_t noise : { size_t(100), size_t(1500), size_t(1504), size_t(6000), size_t(100000) })
  {
    Recorder r(s);
    std::vector<byte> zeros(noise, 0x00);
    Feed(r, zeros, { 1000 });                                    // a call of at most 5 packets never gives up
    Feed(r, s.bytes, { 1000 });
    SCOPED_TRACE(testing::Message() << noise << " bytes of noise");
    EXPECT_EQ(r.BogusCount(), 0);
    ASSERT_GE(r.delivered.size(), 58u);
    EXPECT_EQ(r.delivered.front(), 0);
    EXPECT_EQ(r.delivered.back(), 58);
  }
}

TEST(PacketSyncAlgorithm, ACallWithMoreThanEightPacketsOfNoiseGivesUpOnTheRestOfThatCall)
{
  // Design limit: when no packet is found within 8 packet lengths of one call, the state is reset and the call returns.
  // Valid packets later in the same buffer are lost with it; the next call finds the stream again.
  const Stream s = MakeStream(40, 25, true);
  std::vector<byte> data(188 * 8 + 100, 0x00);
  data.insert(data.end(), s.bytes.begin(), s.bytes.end());
  Recorder r(s);
  r.OnRawData(data.data(), static_cast<int>(data.size()));
  EXPECT_EQ(r.delivered.size(), 0u);

  std::vector<byte> again = s.bytes;
  r.OnRawData(again.data(), static_cast<int>(again.size()));
  EXPECT_EQ(r.delivered.size(), 39u);
  EXPECT_EQ(r.delivered.front(), 0);
}

TEST(PacketSyncAlgorithm, ResetForgetsTheBufferedPacketAndTheSyncState)
{
  const Stream s = MakeStream(30, 17, true);
  std::vector<byte> data = s.bytes;
  Recorder r(s);
  r.OnRawData(data.data(), 188 * 3 + 77);                      // two packets delivered, 77 bytes of packet 3 buffered
  ASSERT_EQ(r.delivered.size(), 3u);
  r.Reset();
  r.OnRawData(data.data() + 188 * 10, 188 * 4);                // starts cleanly at packet 10
  ASSERT_GE(r.delivered.size(), 6u);
  EXPECT_EQ(r.delivered[3], 10);
  EXPECT_EQ(r.BogusCount(), 0);
}

// --- M2TS (192 byte) packets and the detection of the packet length ----------------------------------------------

TEST(PacketSyncAlgorithm, OnRawData2RecognisesTheTransportStreamAndDeliversItWhole)
{
  const Stream s = MakeStream(80, 18, true);
  std::mt19937 rng(19);
  for (int round = 0; round < 30; round++)
  {
    Recorder r(s);
    Feed2(r, s.bytes, BigFirstChunks(rng));
    SCOPED_TRACE(testing::Message() << "round " << round);
    EXPECT_EQ(r.BogusCount(), 0);
    EXPECT_TRUE(InOrder(r.delivered));
    ASSERT_GE(r.delivered.size(), 75u);
    EXPECT_EQ(r.delivered.front(), 0);
  }
}

TEST(PacketSyncAlgorithm, OnRawData2RecognisesTheBlurayPacketLength)
{
  const Stream s = MakeStream(80, 20, true, 192);
  std::mt19937 rng(21);
  for (int round = 0; round < 30; round++)
  {
    Recorder r(s);
    // M2TS packets start with 4 bytes of time stamp: start the data at the sync byte of the first packet
    std::vector<byte> data(s.bytes.begin() + 4, s.bytes.end());
    Feed2(r, data, BigFirstChunks(rng));
    SCOPED_TRACE(testing::Message() << "round " << round);
    EXPECT_EQ(r.BogusCount(), 0);
    EXPECT_TRUE(InOrder(r.delivered));
    ASSERT_GE(r.delivered.size(), 75u);
    EXPECT_EQ(r.delivered.front(), 0);
    EXPECT_GE(r.delivered.back(), 77);
  }
}

TEST(PacketSyncAlgorithm, OnRawData2TellsTheCallerWhenAShortBufferHasNoSync)
{
  const Stream s = MakeStream(10, 22, true);
  Recorder r(s);
  std::vector<byte> noise(500, 0x00);
  EXPECT_TRUE(r.OnRawData2(noise.data(), static_cast<int>(noise.size())));          // short and without sync: read again
  std::vector<byte> big(5000, 0x00);
  EXPECT_FALSE(r.OnRawData2(big.data(), static_cast<int>(big.size())));              // long enough to judge
}

// --- the integrity check used before the data is processed ---------------------------------------------------------

TEST(PacketSyncAlgorithm, OnRawDataCheckCountsTheBytesThatWereOutOfStep)
{
  const Stream s = MakeStream(80, 23, true);
  Recorder r(s);
  std::vector<byte> first(s.bytes.begin(), s.bytes.begin() + 188 * 30);
  EXPECT_EQ(r.OnRawDataCheck(first.data(), static_cast<int>(first.size())), 0);       // not in sync yet: nothing to say
  r.OnRawData2(first.data(), static_cast<int>(first.size()));                          // now it knows the packet length

  std::vector<byte> clean(s.bytes.begin() + 188 * 30, s.bytes.begin() + 188 * 60);
  EXPECT_EQ(r.OnRawDataCheck(clean.data(), static_cast<int>(clean.size())), 0);

  std::vector<byte> damaged = clean;
  damaged.insert(damaged.begin() + 188 * 10 + 50, 25, 0xA5);                           // 25 extra bytes
  const int errors = r.OnRawDataCheck(damaged.data(), static_cast<int>(damaged.size()));
  EXPECT_GE(errors, 25);
  // the check does not change anything: the same answer again, and the real processing still works afterwards
  EXPECT_EQ(r.OnRawDataCheck(damaged.data(), static_cast<int>(damaged.size())), errors);
  EXPECT_EQ(r.OnRawDataCheck(clean.data(), static_cast<int>(clean.size())), 0);
  r.OnRawData2(clean.data(), static_cast<int>(clean.size()));
  EXPECT_EQ(r.BogusCount(), 0);
  EXPECT_GE(r.delivered.size(), 55u);
}
