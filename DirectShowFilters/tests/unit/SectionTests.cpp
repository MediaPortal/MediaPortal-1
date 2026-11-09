#include <gtest/gtest.h>
#include <windows.h>
#include "Section.h"

namespace
{
  // PAT-like header: table 0, syntax indicator set, section_length 13, id 1, version 5, section 2 of 3
  void FillHeader(CSection& s, int length)
  {
    s.Data[0] = 0x00;
    s.Data[1] = static_cast<byte>(0xB0 | ((length >> 8) & 0x0F));
    s.Data[2] = static_cast<byte>(length & 0xFF);
    s.Data[3] = 0x12;
    s.Data[4] = 0x34;
    s.Data[5] = static_cast<byte>(0xC1 | (5 << 1));
    s.Data[6] = 2;
    s.Data[7] = 3;
  }
}

TEST(Section, ResetMarksEverythingAsUnknown)
{
  CSection s;
  EXPECT_EQ(s.table_id, -1);
  EXPECT_EQ(s.section_length, -1);
  EXPECT_EQ(s.section_number, -1);
  EXPECT_EQ(s.version_number, -1);
  EXPECT_EQ(s.BufferPos, 0);
}

TEST(Section, HeaderIsNotDecodedBeforeEightBytesArrived)
{
  CSection s;
  FillHeader(s, 13);
  for (int pos = 0; pos < 8; pos++)
  {
    s.BufferPos = pos;
    EXPECT_FALSE(s.DecodeHeader()) << "BufferPos " << pos;
  }
}

TEST(Section, DecodeHeaderFields)
{
  CSection s;
  FillHeader(s, 13);
  s.BufferPos = 8;
  ASSERT_TRUE(s.DecodeHeader());
  EXPECT_EQ(s.table_id, 0);
  EXPECT_EQ(s.section_syntax_indicator, 1);
  EXPECT_EQ(s.section_length, 13);
  EXPECT_EQ(s.table_id_extension, 0x1234);
  EXPECT_EQ(s.version_number, 5);
  EXPECT_EQ(s.section_number, 2);
}

TEST(Section, SectionLengthUses12Bits)
{
  CSection s;
  FillHeader(s, 0xFFF);
  s.BufferPos = 8;
  ASSERT_TRUE(s.DecodeHeader());
  EXPECT_EQ(s.section_length, 0xFFF);
}

TEST(Section, CalcSectionLengthFromTheBuffer)
{
  CSection s;
  FillHeader(s, 0x123);
  s.BufferPos = 3;
  byte packet[188] = {};
  EXPECT_EQ(s.CalcSectionLength(packet, 5), 0x123);
}

TEST(Section, CalcSectionLengthWhenTheLengthIsSplitAcrossPackets)
{
  CSection s;
  byte packet[188] = {};

  // Only table_id is in the buffer: both length bytes come from the new packet
  s.BufferPos = 1;
  packet[10] = 0xB1;
  packet[11] = 0x23;
  EXPECT_EQ(s.CalcSectionLength(packet, 10), 0x123);

  // The first length byte is in the buffer, the second one is new
  s.BufferPos = 2;
  s.Data[1] = 0xB2;
  packet[20] = 0x34;
  EXPECT_EQ(s.CalcSectionLength(packet, 20), 0x234);
}

TEST(Section, CalcSectionLengthRefusesAStartOutsideThePacket)
{
  CSection s;
  s.BufferPos = 1;
  byte packet[188] = {};
  EXPECT_EQ(s.CalcSectionLength(packet, 188), 0);
  EXPECT_EQ(s.CalcSectionLength(packet, 1000), 0);
}

TEST(Section, CompleteWhenLengthPlusHeaderIsBuffered)
{
  CSection s;
  FillHeader(s, 20);
  s.BufferPos = 22;
  EXPECT_FALSE(s.SectionComplete());      // 3 header bytes + 20 = 23 needed
  s.BufferPos = 23;
  EXPECT_TRUE(s.SectionComplete());
  s.BufferPos = 100;
  EXPECT_TRUE(s.SectionComplete());
}

TEST(Section, NotCompleteBeforeTheHeaderIsThere)
{
  CSection s;
  FillHeader(s, 20);
  s.BufferPos = 5;
  EXPECT_FALSE(s.SectionComplete());
}

TEST(Section, CopyKeepsHeaderAndDataButResetsTheBufferPosition)
{
  CSection a;
  FillHeader(a, 13);
  a.BufferPos = 8;
  a.DecodeHeader();
  a.Data[100] = 0x5A;

  CSection b;
  b = a;
  EXPECT_EQ(b.table_id, a.table_id);
  EXPECT_EQ(b.section_length, 13);
  EXPECT_EQ(b.table_id_extension, 0x1234);
  EXPECT_EQ(b.version_number, 5);
  EXPECT_EQ(b.Data[100], 0x5A);
  EXPECT_EQ(b.BufferPos, 0);
}

TEST(Section, SelfAssignmentIsHarmless)
{
  CSection a;
  FillHeader(a, 13);
  a.BufferPos = 8;
  a.DecodeHeader();
  CSection& same = a;
  a = same;
  EXPECT_EQ(a.section_length, 13);
  EXPECT_EQ(a.BufferPos, 8);
}
