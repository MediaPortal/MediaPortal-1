#include <gtest/gtest.h>
#include <windows.h>
#include "AdaptionField.h"
#include "TestPackets.h"

namespace
{
  // Packet with an adaption field: byte 4 = length, byte 5 = flags, then the optional fields.
  TsPacket MakeAdaptionPacket(byte length, byte flags)
  {
    TsPacket p = MakePacket(0x100, 0x30);
    p.data[4] = length;
    p.data[5] = flags;
    return p;
  }
}

TEST(AdaptionField, PayloadOnlyPacketHasNoFlagsAndNoPcr)
{
  TsPacket p = MakePacket(0x100, 0x10);
  CTsHeader h(p.data);
  CAdaptionField af(h, p.data);
  EXPECT_FALSE(af.DiscontinuityIndicator);
  EXPECT_FALSE(af.RandomAccessInidicator);
  EXPECT_FALSE(af.PcrFlag);
  EXPECT_FALSE(af.OPcrFlag);
  EXPECT_FALSE(af.Pcr.IsValid);
  EXPECT_FALSE(af.OPcr.IsValid);
}

TEST(AdaptionField, TooShortFieldIsIgnored)
{
  // A field shorter than 7 bytes cannot hold flags plus a PCR; the decoder skips it entirely
  TsPacket p = MakeAdaptionPacket(6, 0xFF);
  CTsHeader h(p.data);
  CAdaptionField af(h, p.data);
  EXPECT_FALSE(af.DiscontinuityIndicator);
  EXPECT_FALSE(af.PcrFlag);
  EXPECT_FALSE(af.Pcr.IsValid);
}

TEST(AdaptionField, EachFlagBit)
{
  struct { byte bit; bool CAdaptionField::* member; } flags[] = {
    { 0x80, &CAdaptionField::DiscontinuityIndicator },
    { 0x40, &CAdaptionField::RandomAccessInidicator },
    { 0x20, &CAdaptionField::ElementaryStreamPriorityIndicator },
    { 0x10, &CAdaptionField::PcrFlag },
    { 0x08, &CAdaptionField::OPcrFlag },
    { 0x04, &CAdaptionField::SplicingPointFlag },
    { 0x02, &CAdaptionField::TransportPrivateDataFlag },
    { 0x01, &CAdaptionField::AdaptionFieldExtensionFlag },
  };
  for (const auto& f : flags)
  {
    // the optional fields that a flag announces are zero bytes here; only the flag itself is checked
    TsPacket p = MakeAdaptionPacket(30, f.bit);
    memset(&p.data[6], 0, 30);
    CTsHeader h(p.data);
    CAdaptionField af(h, p.data);
    EXPECT_TRUE(af.*(f.member)) << "bit 0x" << std::hex << int(f.bit);
  }
}

TEST(AdaptionField, PcrIsDecoded)
{
  TsPacket p = MakeAdaptionPacket(7, 0x10);
  EncodePcr(&p.data[6], 0x123456789ULL, 0x1AB);
  CTsHeader h(p.data);
  CAdaptionField af(h, p.data);
  EXPECT_TRUE(af.PcrFlag);
  ASSERT_TRUE(af.Pcr.IsValid);
  EXPECT_EQ(af.Pcr.PcrReferenceBase, 0x123456789ULL);
  EXPECT_EQ(af.Pcr.PcrReferenceExtension, 0x1ABu);
  EXPECT_FALSE(af.OPcr.IsValid);
}

TEST(AdaptionField, OriginalPcrFollowsThePcr)
{
  TsPacket p = MakeAdaptionPacket(13, 0x18);
  EncodePcr(&p.data[6], 1000, 1);
  EncodePcr(&p.data[12], 2000, 2);
  CTsHeader h(p.data);
  CAdaptionField af(h, p.data);
  ASSERT_TRUE(af.Pcr.IsValid);
  ASSERT_TRUE(af.OPcr.IsValid);
  EXPECT_EQ(af.Pcr.PcrReferenceBase, 1000u);
  EXPECT_EQ(af.OPcr.PcrReferenceBase, 2000u);
  EXPECT_EQ(af.OPcr.PcrReferenceExtension, 2u);
}

TEST(AdaptionField, OriginalPcrWithoutPcrStartsAtByte6)
{
  TsPacket p = MakeAdaptionPacket(7, 0x08);
  EncodePcr(&p.data[6], 4242, 7);
  CTsHeader h(p.data);
  CAdaptionField af(h, p.data);
  EXPECT_FALSE(af.Pcr.IsValid);
  ASSERT_TRUE(af.OPcr.IsValid);
  EXPECT_EQ(af.OPcr.PcrReferenceBase, 4242u);
}

TEST(AdaptionField, SpliceCountdownFollowsThePcr)
{
  TsPacket p = MakeAdaptionPacket(13, 0x14);
  EncodePcr(&p.data[6], 5, 0);
  p.data[12] = 0x2A;
  CTsHeader h(p.data);
  CAdaptionField af(h, p.data);
  EXPECT_TRUE(af.SplicingPointFlag);
  EXPECT_EQ(af.SpliceCountDown, 0x2A);
}
