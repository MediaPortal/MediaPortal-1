#include <gtest/gtest.h>
#include <windows.h>
#include "TsHeader.h"
#include "TestPackets.h"

TEST(TsHeader, DecodesTheExampleFromTheSource)
{
  // 47 40 d2 10: payload unit start, PID 0xd2, payload only, continuity counter 0
  TsPacket p = MakePacket(0xD2, 0x10, true);
  CTsHeader h(p.data);
  EXPECT_EQ(h.SyncByte, 0x47);
  EXPECT_FALSE(h.TransportError);
  EXPECT_TRUE(h.PayloadUnitStart);
  EXPECT_FALSE(h.TransportPriority);
  EXPECT_EQ(h.Pid, 0xD2);
  EXPECT_TRUE(h.PayLoadOnly());
  EXPECT_FALSE(h.AdaptionFieldOnly());
  EXPECT_FALSE(h.AdaptionFieldAndPayLoad());
  EXPECT_TRUE(h.HasPayload);
  EXPECT_FALSE(h.HasAdaptionField);
  EXPECT_EQ(h.ContinuityCounter, 0);
  EXPECT_EQ(h.PayLoadStart, 4);
}

TEST(TsHeader, WrongSyncByteIsATransportError)
{
  TsPacket p = MakePacket(0x100, 0x10);
  p.data[0] = 0x46;
  CTsHeader h(p.data);
  EXPECT_EQ(h.SyncByte, 0x46);
  EXPECT_TRUE(h.TransportError);
}

TEST(TsHeader, PidUsesAll13Bits)
{
  TsPacket p = MakePacket(0x1FFF, 0x1F);
  CTsHeader h(p.data);
  EXPECT_EQ(h.Pid, 0x1FFF);
  EXPECT_EQ(h.ContinuityCounter, 0x0F);
}

TEST(TsHeader, ErrorAndPriorityFlags)
{
  TsPacket p = MakePacket(0x0011, 0x10);
  p.data[1] |= 0x80 | 0x20;
  CTsHeader h(p.data);
  EXPECT_TRUE(h.TransportError);
  EXPECT_TRUE(h.TransportPriority);
  EXPECT_EQ(h.Pid, 0x11);
}

TEST(TsHeader, ScramblingBitIsKept)
{
  TsPacket p = MakePacket(0x20, 0x10);
  p.data[3] |= 0x80;
  CTsHeader h(p.data);
  EXPECT_EQ(h.TScrambling, 0x80);
  EXPECT_TRUE(h.PayLoadOnly());
}

TEST(TsHeader, ContinuityCounterAllValues)
{
  for (int cc = 0; cc < 16; cc++)
  {
    TsPacket p = MakePacket(0x30, 0x10 | cc);
    CTsHeader h(p.data);
    EXPECT_EQ(h.ContinuityCounter, cc);
  }
}

TEST(TsHeader, AdaptionFieldOnly)
{
  TsPacket p = MakePacket(0x31, 0x20);
  p.data[4] = 183;
  CTsHeader h(p.data);
  EXPECT_TRUE(h.AdaptionFieldOnly());
  EXPECT_TRUE(h.HasAdaptionField);
  EXPECT_FALSE(h.HasPayload);
  EXPECT_EQ(h.AdaptionFieldLength, 183);
}

TEST(TsHeader, PayloadStartsAfterTheAdaptionField)
{
  TsPacket p = MakePacket(0x31, 0x30);
  p.data[4] = 7;
  CTsHeader h(p.data);
  EXPECT_TRUE(h.AdaptionFieldAndPayLoad());
  EXPECT_EQ(h.AdaptionFieldLength, 7);
  EXPECT_EQ(h.PayLoadStart, 12);
}

TEST(TsHeader, PayloadStartIsLimitedToThePacket)
{
  // The longest adaption field that still leaves one payload byte
  TsPacket p = MakePacket(0x31, 0x30);
  p.data[4] = 182;
  CTsHeader h(p.data);
  EXPECT_EQ(h.PayLoadStart, 187);

  // A field of 183 bytes or more leaves no payload; PayLoadStart then keeps its default of 4
  p.data[4] = 183;
  CTsHeader h2(p.data);
  EXPECT_EQ(h2.PayLoadStart, 4);
  p.data[4] = 255;
  CTsHeader h3(p.data);
  EXPECT_EQ(h3.PayLoadStart, 4);
}

TEST(TsHeader, PayloadUnitStartIsClearedWithoutPayload)
{
  TsPacket p = MakePacket(0x31, 0x20, true);
  p.data[4] = 183;
  CTsHeader h(p.data);
  EXPECT_FALSE(h.PayloadUnitStart);
}

TEST(TsHeader, ReservedAdaptionControlIsNoneOfTheThreeKinds)
{
  TsPacket p = MakePacket(0x31, 0x00);
  CTsHeader h(p.data);
  EXPECT_FALSE(h.PayLoadOnly());
  EXPECT_FALSE(h.AdaptionFieldOnly());
  EXPECT_FALSE(h.AdaptionFieldAndPayLoad());
}
