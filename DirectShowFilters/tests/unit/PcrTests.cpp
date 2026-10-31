#include <gtest/gtest.h>
#include <windows.h>
#include "Pcr.h"
#include "TestPackets.h"

TEST(Pcr, DefaultIsInvalidAndZero)
{
  CPcr pcr;
  EXPECT_FALSE(pcr.IsValid);
  EXPECT_EQ(pcr.PcrReferenceBase, 0u);
  EXPECT_EQ(pcr.PcrReferenceExtension, 0u);
}

TEST(Pcr, DecodeReadsBaseAndExtension)
{
  byte field[6];
  EncodePcr(field, 0x123456789ULL, 0x155);
  CPcr pcr;
  pcr.Decode(field);
  EXPECT_TRUE(pcr.IsValid);
  EXPECT_EQ(pcr.PcrReferenceBase, 0x123456789ULL);
  EXPECT_EQ(pcr.PcrReferenceExtension, 0x155u);
}

TEST(Pcr, DecodeMaximumValues)
{
  byte field[6];
  EncodePcr(field, MAX_PCR, 0x1FF);
  CPcr pcr;
  pcr.Decode(field);
  EXPECT_EQ(pcr.PcrReferenceBase, MAX_PCR);
  EXPECT_EQ(pcr.PcrReferenceExtension, 0x1FFu);
}

TEST(Pcr, ToClockCombinesBaseAnd27MHzExtension)
{
  CPcr pcr;
  pcr.PcrReferenceBase = 90000;           // one second at 90 kHz
  pcr.PcrReferenceExtension = 135;
  EXPECT_NEAR(pcr.ToClock(), 1.0 + pcr.PcrReferenceExtension / 27000000.0, 1e-9);
}

TEST(Pcr, FromClockAndToClockRoundTrip)
{
  CPcr pcr;
  pcr.FromClock(12.5);
  EXPECT_NEAR(pcr.ToClock(), 12.5, 1e-6);
  pcr.FromClock(0.0);
  EXPECT_NEAR(pcr.ToClock(), 0.0, 1e-9);
}

TEST(Pcr, AddAndSubtract)
{
  CPcr a, b;
  a.FromClock(10.0);
  b.FromClock(2.5);
  CPcr sum = a + b;
  EXPECT_NEAR(sum.ToClock(), 12.5, 1e-6);
  CPcr diff = a - b;
  EXPECT_NEAR(diff.ToClock(), 7.5, 1e-6);
  a += b;
  EXPECT_NEAR(a.ToClock(), 12.5, 1e-6);
}

TEST(Pcr, TimeSplitsIntoHoursMinutesSeconds)
{
  CPcr pcr;
  pcr.FromClock(3600.0 + 120.0 + 5.0);
  int day, hour, minutes, seconds, millis;
  pcr.Time(day, hour, minutes, seconds, millis);
  EXPECT_EQ(day, 0);
  EXPECT_EQ(hour, 1);
  EXPECT_EQ(minutes, 2);
  EXPECT_EQ(seconds, 5);
}

TEST(Pcr, ResetClearsEverything)
{
  byte field[6];
  EncodePcr(field, 1234, 56);
  CPcr pcr;
  pcr.Decode(field);
  pcr.Reset();
  EXPECT_FALSE(pcr.IsValid);
  EXPECT_EQ(pcr.PcrReferenceBase, 0u);
  EXPECT_EQ(pcr.PcrReferenceExtension, 0u);
}

// --- PTS/DTS in a PES header -----------------------------------------------------------------

namespace
{
  // 'pesHeader' layout used by DecodeFromPesHeader: byte 6 = 10xxxxxx, byte 7 = PTS/DTS flags,
  // PTS at 9..13, DTS at 14..18.
  void MakePesHeader(byte* pes, bool pts, bool dts, uint64_t ptsValue, uint64_t dtsValue)
  {
    memset(pes, 0, 32);
    pes[2] = 1;
    pes[6] = 0x80;
    pes[7] = static_cast<byte>((pts ? 0x80 : 0) | (dts ? 0x40 : 0));
    pes[8] = static_cast<byte>(pts ? (dts ? 10 : 5) : 0);
    if (pts) EncodeTimestamp(&pes[9], dts ? 0x3 : 0x2, ptsValue);
    if (dts) EncodeTimestamp(&pes[14], 0x1, dtsValue);
  }
}

TEST(PesTimestamps, PtsOnly)
{
  byte pes[32];
  MakePesHeader(pes, true, false, 0x1CC6461EBULL, 0);
  CPcr pts, dts;
  EXPECT_TRUE(CPcr::DecodeFromPesHeader(pes, 4, pts, dts));
  EXPECT_TRUE(pts.IsValid);
  EXPECT_FALSE(dts.IsValid);
  EXPECT_EQ(pts.PcrReferenceBase, 0x1CC6461EBULL);
}

TEST(PesTimestamps, PtsAndDts)
{
  byte pes[32];
  MakePesHeader(pes, true, true, 0x100000000ULL + 5, 0xFFFFFFFFULL);
  CPcr pts, dts;
  EXPECT_TRUE(CPcr::DecodeFromPesHeader(pes, 4, pts, dts));
  EXPECT_TRUE(pts.IsValid);
  EXPECT_TRUE(dts.IsValid);
  EXPECT_EQ(pts.PcrReferenceBase, 0x100000000ULL + 5);
  EXPECT_EQ(dts.PcrReferenceBase, 0xFFFFFFFFULL);
}

TEST(PesTimestamps, NoTimestampFlags)
{
  byte pes[32];
  MakePesHeader(pes, false, false, 0, 0);
  CPcr pts, dts;
  EXPECT_FALSE(CPcr::DecodeFromPesHeader(pes, 4, pts, dts));
  EXPECT_FALSE(pts.IsValid);
  EXPECT_FALSE(dts.IsValid);
}

TEST(PesTimestamps, NotAnMpegPesHeader)
{
  byte pes[32];
  MakePesHeader(pes, true, true, 1, 1);
  pes[6] = 0x40;                           // the two marker bits must be 10
  CPcr pts, dts;
  EXPECT_FALSE(CPcr::DecodeFromPesHeader(pes, 4, pts, dts));
}

TEST(PesTimestamps, HeaderTooCloseToTheEndOfThePacketIsRejected)
{
  byte pes[32];
  MakePesHeader(pes, true, false, 77, 0);
  CPcr pts, dts;
  EXPECT_TRUE(CPcr::DecodeFromPesHeader(pes, 174, pts, dts));    // 174 + 13 = 187: still inside
  EXPECT_FALSE(CPcr::DecodeFromPesHeader(pes, 175, pts, dts));   // 175 + 13 = 188: the PTS would be cut
  EXPECT_FALSE(CPcr::DecodeFromPesHeader(pes, 180, pts, dts));   // not even the flags fit
  EXPECT_FALSE(CPcr::DecodeFromPesHeader(pes, 10000, pts, dts));

  MakePesHeader(pes, true, true, 77, 78);
  EXPECT_TRUE(CPcr::DecodeFromPesHeader(pes, 169, pts, dts));    // 169 + 18 = 187
  EXPECT_FALSE(CPcr::DecodeFromPesHeader(pes, 170, pts, dts));   // the DTS would be cut
}
