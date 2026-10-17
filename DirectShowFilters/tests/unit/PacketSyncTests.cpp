#include <gtest/gtest.h>
#include <windows.h>
#include <vector>
#include "PacketSync.h"

namespace
{
  // Collects the packets that CPacketSync delivers
  class Collector : public CPacketSync
  {
  public:
    using CPacketSync::OnTsPacket;
    void OnTsPacket(byte* tsPacket) override
    {
      ASSERT_EQ(tsPacket[0], TS_PACKET_SYNC);
      counters.push_back(tsPacket[1]);          // the tests number the packets in byte 1
      lengths.push_back(TS_PACKET_LEN);
      payloadIntact = payloadIntact && (tsPacket[2] == static_cast<byte>(tsPacket[1] ^ 0x5A)) && (tsPacket[187] == tsPacket[1]);
    }
    std::vector<int> counters;
    std::vector<int> lengths;
    bool payloadIntact = true;
  };

  // 'count' packets: sync, number, number ^ 0x5A, filler..., number in the last byte
  std::vector<byte> MakeStream(int count, int firstNumber = 0)
  {
    std::vector<byte> data(static_cast<size_t>(count) * TS_PACKET_LEN, 0x11);
    for (int i = 0; i < count; i++)
    {
      byte* p = &data[static_cast<size_t>(i) * TS_PACKET_LEN];
      byte number = static_cast<byte>(firstNumber + i);
      p[0] = TS_PACKET_SYNC;
      p[1] = number;
      p[2] = static_cast<byte>(number ^ 0x5A);
      p[187] = number;
    }
    return data;
  }

  void Feed(Collector& c, std::vector<byte> data, size_t chunk)
  {
    for (size_t pos = 0; pos < data.size(); pos += chunk)
    {
      size_t n = (std::min)(chunk, data.size() - pos);
      c.OnRawData(&data[pos], static_cast<int>(n));
    }
  }

  void ExpectPrefixInOrder(const Collector& c, int minimum, int total)
  {
    ASSERT_GE(static_cast<int>(c.counters.size()), minimum);
    ASSERT_LE(static_cast<int>(c.counters.size()), total);
    for (size_t i = 0; i < c.counters.size(); i++)
      EXPECT_EQ(c.counters[i], static_cast<int>(i & 0xFF)) << "packet " << i;
    EXPECT_TRUE(c.payloadIntact);
  }
}

TEST(PacketSync, WholeBufferInOneCall)
{
  Collector c;
  c.OnRawData(MakeStream(20).data(), 20 * TS_PACKET_LEN);
  // The last packet stays in the buffer until the next byte shows that it ends there
  ExpectPrefixInOrder(c, 19, 20);
}

TEST(PacketSync, EverySplitSizeDeliversTheSamePackets)
{
  const int total = 30;
  for (size_t chunk : { size_t(1), size_t(7), size_t(100), size_t(187), size_t(188), size_t(189), size_t(376), size_t(1000), size_t(5640) })
  {
    Collector c;
    Feed(c, MakeStream(total), chunk);
    SCOPED_TRACE(testing::Message() << "chunk " << chunk);
    ExpectPrefixInOrder(c, total - 1, total);
  }
}

TEST(PacketSync, SkipsGarbageInFrontOfTheFirstPacket)
{
  std::vector<byte> garbage(77, 0x00);
  std::vector<byte> stream = MakeStream(12);
  garbage.insert(garbage.end(), stream.begin(), stream.end());
  Collector c;
  Feed(c, garbage, 500);
  ExpectPrefixInOrder(c, 11, 12);
}

TEST(PacketSync, RecoversAfterLostBytesInTheMiddle)
{
  std::vector<byte> a = MakeStream(10, 0);
  std::vector<byte> b = MakeStream(10, 100);
  a.insert(a.end(), 20, 0x00);               // 20 bytes of garbage between the two parts
  a.insert(a.end(), b.begin(), b.end());
  Collector c;
  Feed(c, a, 400);
  ASSERT_GE(c.counters.size(), 16u);
  // both parts arrive in order; the very last packet stays buffered until more data follows
  EXPECT_EQ(c.counters.front(), 0);
  EXPECT_EQ(c.counters.back(), 108);
  EXPECT_TRUE(c.payloadIntact);
}

TEST(PacketSync, NoSyncBytesAtAllDeliversNothingAndDoesNotCrash)
{
  std::vector<byte> noise(188 * 40, 0x00);
  Collector c;
  Feed(c, noise, 188);
  Feed(c, noise, 13);
  EXPECT_TRUE(c.counters.empty());
}

TEST(PacketSync, ShortBuffersAreKeptUntilMoreDataArrives)
{
  Collector c;
  std::vector<byte> stream = MakeStream(3);
  c.OnRawData(stream.data(), 100);
  EXPECT_TRUE(c.counters.empty());
  c.OnRawData(stream.data() + 100, static_cast<int>(stream.size()) - 100);
  ExpectPrefixInOrder(c, 2, 3);
}

TEST(PacketSync, ResetForgetsBufferedData)
{
  Collector c;
  std::vector<byte> stream = MakeStream(5);
  c.OnRawData(stream.data(), 188 + 50);      // delivers packet 0, keeps 50 bytes of packet 1
  c.Reset();
  std::vector<byte> next = MakeStream(6, 40);
  c.OnRawData(next.data(), static_cast<int>(next.size()));
  ASSERT_GE(c.counters.size(), 6u);
  EXPECT_EQ(c.counters[0], 0);
  // the 50 buffered bytes are gone: the next packet is the first one of the new data
  EXPECT_EQ(c.counters[1], 40);
}

TEST(PacketSync, BadPacketsInsideAGoodStreamAreResynchronised)
{
  // the second sync byte is damaged: the stream is still followed (in sync mode one good sync byte is enough)
  std::vector<byte> stream = MakeStream(20);
  stream[188 * 7] = 0x00;
  Collector c;
  Feed(c, stream, 1000);
  EXPECT_GE(c.counters.size(), 15u);
}
