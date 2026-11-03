#include <gtest/gtest.h>
#include <memory>
#include <vector>
#include <streams.h>
#include "Packet.h"

// BDReader's Packet is the unit that the demultiplexer fills, hands from one stage to the next and finally to the pins.

TEST(BdPacket, NewPacketHasNoDataAndInvalidTimes)
{
  Packet p;
  EXPECT_TRUE(p.empty());
  EXPECT_EQ(p.GetDataSize(), 0);
  EXPECT_EQ(p.rtStart, Packet::INVALID_TIME);
  EXPECT_EQ(p.rtStop, Packet::INVALID_TIME);
  EXPECT_EQ(p.nClipNumber, -100);
  EXPECT_EQ(p.nPlaylist, -100);
  EXPECT_FALSE(p.bDiscontinuity);
  EXPECT_FALSE(p.bSyncPoint);
  EXPECT_FALSE(p.bResuming);
  EXPECT_EQ(p.nNewSegment, 0);
  EXPECT_EQ(p.pmt, nullptr);
}

TEST(BdPacket, SetDataCopiesTheBytes)
{
  const unsigned char bytes[] = { 1, 2, 3, 4, 5 };
  Packet p;
  p.SetData(bytes, sizeof(bytes));
  ASSERT_EQ(p.GetDataSize(), 5);
  EXPECT_EQ(memcmp(p.data(), bytes, sizeof(bytes)), 0);

  p.SetData(bytes, 2);                     // replaces, does not append
  EXPECT_EQ(p.GetDataSize(), 2);
}

TEST(BdPacket, SetDataWithZeroLengthEmptiesThePacket)
{
  Packet p;
  p.resize(10);
  p.SetData(nullptr, 0);
  EXPECT_TRUE(p.empty());
}

TEST(BdPacket, AppendConcatenatesPackets)
{
  Packet a, b;
  a.insert(a.end(), { 1, 2, 3 });
  b.insert(b.end(), { 4, 5 });
  a.insert(a.end(), b.begin(), b.end());
  ASSERT_EQ(a.size(), 5u);
  EXPECT_EQ(a[3], 4);
  EXPECT_EQ(a[4], 5);
}

TEST(BdPacket, RemoveFromTheFrontLikeTheDemultiplexerDoes)
{
  // m_p->erase(m_p->begin(), m_p->begin() + n): drop the bytes that were already consumed
  Packet p;
  p.insert(p.end(), { 10, 11, 12, 13, 14, 15 });
  p.erase(p.begin(), p.begin() + 2);
  ASSERT_EQ(p.size(), 4u);
  EXPECT_EQ(p[0], 12);
}

TEST(BdPacket, CopyPropertiesCopiesTheStreamInfo)
{
  Packet src, dst;
  src.nClipNumber = 3;
  src.nPlaylist = 7;
  src.bDiscontinuity = true;
  src.bSyncPoint = true;
  src.nNewSegment = 2;
  src.bResuming = true;
  src.rtStart = 1000;
  src.rtStop = 2000;
  src.rtOffset = 50;
  src.rtPlaylistTime = 60;
  src.rtTitleDuration = 70;

  dst.CopyProperties(src);
  EXPECT_EQ(dst.nClipNumber, 3);
  EXPECT_EQ(dst.nPlaylist, 7);
  EXPECT_TRUE(dst.bDiscontinuity);
  EXPECT_TRUE(dst.bSyncPoint);
  EXPECT_EQ(dst.nNewSegment, 2);
  EXPECT_TRUE(dst.bResuming);
  EXPECT_EQ(dst.rtStart, 1000);
  EXPECT_EQ(dst.rtStop, 2000);
  EXPECT_EQ(dst.rtOffset, 50);
  EXPECT_EQ(dst.rtPlaylistTime, 60);
  EXPECT_EQ(dst.rtTitleDuration, 70);
}

TEST(BdPacket, CopyPropertiesCanKeepTheStartTimeWhenTheSourceHasNone)
{
  Packet src, dst;
  dst.rtStart = 111;
  dst.rtStop = 222;
  src.nClipNumber = 9;                      // rtStart of src is INVALID_TIME

  dst.CopyProperties(src, true);
  EXPECT_EQ(dst.nClipNumber, 9);
  EXPECT_EQ(dst.rtStart, 111);
  EXPECT_EQ(dst.rtStop, 222);

  dst.CopyProperties(src, false);
  EXPECT_EQ(dst.rtStart, Packet::INVALID_TIME);
}

TEST(BdPacket, ResetPropertiesKeepsTheClipInfoUnlessAsked)
{
  Packet p;
  p.nClipNumber = 4;
  p.nPlaylist = 5;
  p.rtOffset = 6;
  p.bDiscontinuity = true;
  p.rtStart = 99;

  p.ResetProperties(false);
  EXPECT_EQ(p.nClipNumber, 4);
  EXPECT_EQ(p.nPlaylist, 5);
  EXPECT_EQ(p.rtOffset, 6);
  EXPECT_FALSE(p.bDiscontinuity);
  EXPECT_EQ(p.rtStart, Packet::INVALID_TIME);

  p.ResetProperties(true);
  EXPECT_EQ(p.nClipNumber, -100);
  EXPECT_EQ(p.nPlaylist, -100);
  EXPECT_EQ(p.rtOffset, 0);
}

TEST(BdPacket, TransferPropertiesMovesThemToTheTarget)
{
  Packet src, dst;
  src.nClipNumber = 8;
  src.rtStart = 500;
  src.bSyncPoint = true;

  dst.TransferProperties(src, false, true);
  EXPECT_EQ(dst.nClipNumber, 8);
  EXPECT_EQ(dst.rtStart, 500);
  EXPECT_TRUE(dst.bSyncPoint);
  EXPECT_EQ(src.nClipNumber, -100);
  EXPECT_EQ(src.rtStart, Packet::INVALID_TIME);
  EXPECT_FALSE(src.bSyncPoint);
}

// --- PacketList / PopHead (owning list of packets) --------------------------------------------

TEST(BdPacketList, PopHeadReturnsPacketsInOrderAndTransfersOwnership)
{
  PacketList list;
  for (int i = 0; i < 3; i++)
  {
    std::unique_ptr<Packet> p(new Packet());
    p->nClipNumber = i;
    list.push_back(std::move(p));
  }
  for (int i = 0; i < 3; i++)
  {
    std::unique_ptr<Packet> p = PopHead(list);
    ASSERT_NE(p, nullptr);
    EXPECT_EQ(p->nClipNumber, i);
    EXPECT_EQ(list.size(), static_cast<size_t>(2 - i));
  }
  EXPECT_TRUE(list.empty());
}

TEST(BdPacketList, ClearDestroysThePackets)
{
  PacketList list;
  list.push_back(std::unique_ptr<Packet>(new Packet()));
  list.push_back(std::unique_ptr<Packet>(new Packet()));
  list.clear();
  EXPECT_TRUE(list.empty());
}

TEST(BdPacketList, MergeTheListIntoOnePacketLikeTheDemultiplexerDoes)
{
  PacketList list;
  for (int i = 0; i < 4; i++)
  {
    std::unique_ptr<Packet> p(new Packet());
    p->insert(p->end(), 3, static_cast<unsigned char>(i));
    list.push_back(std::move(p));
  }
  std::unique_ptr<Packet> first = PopHead(list);
  while (!list.empty())
  {
    std::unique_ptr<Packet> next = PopHead(list);
    first->insert(first->end(), next->begin(), next->end());
  }
  ASSERT_EQ(first->size(), 12u);
  EXPECT_EQ((*first)[0], 0);
  EXPECT_EQ((*first)[11], 3);
}
