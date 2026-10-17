// CGolombBuffer of TsReader (from MPC-HC): reading bits, bytes and Exp-Golomb codes from a buffer. The video and audio header
// parsers are built on it, so its behaviour at the end of the data matters.
#include <gtest/gtest.h>
#include <winsock2.h>
#include <windows.h>
#include <vector>
#include "GolombBuffer.h"
#include "../unit/BitWriter.h"
#include "../unit/GuardedBuffer.h"

namespace
{
  std::vector<BYTE> Make(std::initializer_list<int> v)
  {
    std::vector<BYTE> b;
    for (int x : v)
      b.push_back(static_cast<BYTE>(x));
    return b;
  }
}

TEST(GolombBuffer, ReadsBitsMostSignificantFirst)
{
  auto data = Make({ 0xA5, 0x3C });                       // 1010 0101 0011 1100
  CGolombBuffer g(data.data(), 2);
  EXPECT_EQ(g.BitRead(1), 1u);
  EXPECT_EQ(g.BitRead(3), 2u);                            // 010
  EXPECT_EQ(g.BitRead(4), 5u);                            // 0101
  EXPECT_EQ(g.BitRead(8), 0x3Cu);
  EXPECT_TRUE(g.IsEOF());
}

TEST(GolombBuffer, PeekDoesNotMove)
{
  auto data = Make({ 0xF0, 0x0F });
  CGolombBuffer g(data.data(), 2);
  EXPECT_EQ(g.BitRead(4, true), 0xFu);
  EXPECT_EQ(g.BitRead(4, true), 0xFu);
  EXPECT_EQ(g.BitRead(8), 0xF0u);
  EXPECT_EQ(g.BitRead(8), 0x0Fu);
}

TEST(GolombBuffer, FieldsOfAnyWidthUpTo32AcrossBytes)
{
  auto data = Make({ 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 });
  {
    CGolombBuffer g(data.data(), 8);
    EXPECT_EQ(g.BitRead(32), 0x12345678u);
    EXPECT_EQ(g.BitRead(32), 0x9ABCDEF0u);
  }
  {
    CGolombBuffer g(data.data(), 8);
    g.BitRead(3);
    EXPECT_EQ(g.BitRead(29), 0x12345678u & 0x1FFFFFFF);
    EXPECT_EQ(g.BitRead(1), 1u);                           // the top bit of 0x9A
    EXPECT_EQ(g.BitRead(31), 0x9ABCDEF0u & 0x7FFFFFFF);
  }
}

TEST(GolombBuffer, BigFieldsOf33To56Bits)
{
  auto data = Make({ 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD });
  CGolombBuffer g(data.data(), 7);
  EXPECT_EQ(g.BitRead(56), 0x0123456789ABCDull);
}

TEST(GolombBuffer, ReadsZeroAtTheEndOfTheData)
{
  auto data = Make({ 0xFF });
  CGolombBuffer g(data.data(), 1);
  EXPECT_EQ(g.BitRead(8), 0xFFu);
  EXPECT_EQ(g.BitRead(8), 0u);
  EXPECT_EQ(g.BitRead(1), 0u);
  EXPECT_EQ(g.BitRead(32), 0u);
}

TEST(GolombBuffer, NeverReadsBehindTheBuffer)
{
  for (int size = 0; size <= 9; size++)
  {
    GuardedBuffer buffer(size ? size : 1, 0xFF);
    CGolombBuffer g(buffer.data(), size);
    for (int i = 0; i < 100; i++)
      g.BitRead(1 + i % 32);
    g.UExpGolombRead();
    g.SExpGolombRead();
    SUCCEED();
  }
}

TEST(GolombBuffer, ByteHelpersAreBigEndian)
{
  auto data = Make({ 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE });
  CGolombBuffer g(data.data(), 7);
  EXPECT_EQ(g.ReadByte(), 0x12);
  EXPECT_EQ(g.ReadShort(), 0x3456);
  EXPECT_EQ(g.ReadDword(), 0x789ABCDEu);
}

TEST(GolombBuffer, ExpGolombCodes)
{
  // 0 -> 1, 1 -> 010, 2 -> 011, 3 -> 00100, 4 -> 00101, 7 -> 0001000, 255 -> 000000010000000
  const uint64_t values[] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 14, 15, 16, 100, 255, 256, 1000, 65535, 1000000, 0xFFFFFFFEull };
  BitWriter w;
  for (uint64_t v : values)
    w.UE(v);
  w.Trailing();
  auto bytes = w.Bytes();
  CGolombBuffer g(bytes.data(), static_cast<int>(bytes.size()));
  for (uint64_t v : values)
    EXPECT_EQ(g.UExpGolombRead(), v);
}

TEST(GolombBuffer, SignedExpGolombCodes)
{
  const int64_t values[] = { 0, 1, -1, 2, -2, 3, -3, 100, -100, 32767, -32768 };
  BitWriter w;
  for (int64_t v : values)
    w.SE(v);
  w.Trailing();
  auto bytes = w.Bytes();
  CGolombBuffer g(bytes.data(), static_cast<int>(bytes.size()));
  for (int64_t v : values)
    EXPECT_EQ(g.SExpGolombRead(), v);
}

TEST(GolombBuffer, ExpGolombAtTheEndOfTheDataEndsAndGivesTheCapValue)
{
  // all zero bits: the prefix is capped at 32 zeros, the value is (1<<32) - 1 + 0; the caller has to check for the end
  auto data = Make({ 0, 0, 0, 0, 0, 0 });
  CGolombBuffer g(data.data(), 6);
  EXPECT_EQ(g.UExpGolombRead(), 0xFFFFFFFFull);
  CGolombBuffer h(data.data(), 0);
  EXPECT_EQ(h.UExpGolombRead(), 0xFFFFFFFFull);
}

TEST(GolombBuffer, PositionAlignmentAndSkip)
{
  auto data = Make({ 1, 2, 3, 4, 5, 6 });
  CGolombBuffer g(data.data(), 6);
  EXPECT_EQ(g.GetPos(), 0);
  g.BitRead(8);
  EXPECT_EQ(g.GetPos(), 1);
  g.BitRead(3);
  g.BitByteAlign();
  EXPECT_EQ(g.BitRead(8), 3u);
  g.SkipBytes(2);
  EXPECT_EQ(g.BitRead(8), 6u);
  EXPECT_TRUE(g.IsEOF());
  EXPECT_EQ(g.GetRemaining(), 0);
}

TEST(GolombBuffer, SeekAndRemaining)
{
  auto data = Make({ 10, 20, 30, 40 });
  CGolombBuffer g(data.data(), 4);
  EXPECT_EQ(g.GetSize(), 4);
  EXPECT_EQ(g.GetRemaining(), 4);
  g.Seek(2);
  EXPECT_EQ(g.GetRemaining(), 2);
  EXPECT_EQ(g.ReadByte(), 30);
  g.Reset();
  EXPECT_EQ(g.ReadByte(), 10);
}

TEST(GolombBuffer, ReadBufferCopiesBytes)
{
  auto data = Make({ 1, 2, 3, 4, 5, 6 });
  CGolombBuffer g(data.data(), 6);
  g.SkipBytes(1);
  BYTE out[3] = {};
  g.ReadBuffer(out, 3);
  EXPECT_EQ(out[0], 2);
  EXPECT_EQ(out[2], 4);
  EXPECT_EQ(g.ReadByte(), 5);
}

TEST(GolombBuffer, ResetWithANewBuffer)
{
  auto a = Make({ 1, 2 }), b = Make({ 9, 8, 7 });
  CGolombBuffer g(a.data(), 2);
  g.ReadByte();
  g.Reset(b.data(), 3);
  EXPECT_EQ(g.GetSize(), 3);
  EXPECT_EQ(g.ReadByte(), 9);
}
