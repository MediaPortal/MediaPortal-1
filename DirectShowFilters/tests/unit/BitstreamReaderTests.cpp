#include <gtest/gtest.h>
#include <cstdint>
#include <stdexcept>
#include <vector>
#include <windows.h>
#include "GuardedBuffer.h"
#include "HEVC/BitstreamReader.h"

// The reader is used on H.265 elementary streams that come straight from the network or a file.

TEST(BitstreamReader, BitsAreReadMostSignificantFirst)
{
  const std::vector<uint8_t> data = { 0xA5, 0x0F };      // 1010 0101  0000 1111
  BitstreamReader r(data.data(), data.size());
  const bool expected[] = { 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1 };
  for (bool bit : expected)
    EXPECT_EQ(r.getBit(), bit);
}

TEST(BitstreamReader, GetBitsAcrossByteBoundaries)
{
  const std::vector<uint8_t> data = { 0xA5, 0x3C, 0xF0, 0x01 };
  BitstreamReader r(data.data(), data.size());
  EXPECT_EQ(r.getBits(4), 0xAu);
  EXPECT_EQ(r.getBits(8), 0x53u);          // 0101 0011
  EXPECT_EQ(r.getBits(12), 0xCF0u);        // 1100 1111 0000
  EXPECT_EQ(r.getBits(0), 0u);
}

TEST(BitstreamReader, ShowBitsDoesNotConsume)
{
  const std::vector<uint8_t> data = { 0xDE, 0xAD, 0xBE, 0xEF };
  BitstreamReader r(data.data(), data.size());
  EXPECT_EQ(r.showBits(16), 0xDEADu);
  EXPECT_EQ(r.showBits(16), 0xDEADu);
  EXPECT_EQ(r.getBits(16), 0xDEADu);
  EXPECT_EQ(r.getBits(16), 0xBEEFu);
}

TEST(BitstreamReader, AvailableCountsRemainingBits)
{
  const std::vector<uint8_t> data = { 0x11, 0x22, 0x33, 0x44 };
  BitstreamReader r(data.data(), data.size());
  EXPECT_EQ(r.available(), 32u);
  r.getBits(5);
  EXPECT_EQ(r.available(), 27u);
  r.getBits(3);
  EXPECT_EQ(r.available(), 24u);
}

TEST(BitstreamReader, SkipBits)
{
  const std::vector<uint8_t> data = { 0xFF, 0x0F, 0xA0, 0x55 };
  BitstreamReader r(data.data(), data.size());
  r.skipBits(12);
  EXPECT_EQ(r.getBits(4), 0xFu);
  r.skipBits(8);
  EXPECT_EQ(r.getBits(8), 0x55u);
}

TEST(BitstreamReader, EmulationPreventionByteIsSkipped)
{
  // 00 00 03 xx: the 03 is not part of the payload
  const std::vector<uint8_t> data = { 0x00, 0x00, 0x03, 0x01, 0xFF };
  BitstreamReader r(data.data(), data.size());
  EXPECT_EQ(r.getBits(8), 0x00u);
  EXPECT_EQ(r.getBits(8), 0x00u);
  EXPECT_EQ(r.getBits(8), 0x01u);
  EXPECT_EQ(r.getBits(8), 0xFFu);
}

TEST(BitstreamReader, UnsignedExpGolomb)
{
  // 1 | 010 | 011 | 00100 | 00101 | 00110 | 00111 -> 0 1 2 3 4 5 6
  const std::vector<uint8_t> data = { 0xA6, 0x42, 0x98, 0xE0 };
  BitstreamReader r(data.data(), data.size());
  for (uint32_t expected = 0; expected <= 6; expected++)
    EXPECT_EQ(r.getGolombU(), expected);
}

TEST(BitstreamReader, SignedExpGolomb)
{
  // the same bit pattern read as se(v): 0 +1 -1 +2 -2 +3 -3
  const std::vector<uint8_t> data = { 0xA6, 0x42, 0x98, 0xE0 };
  BitstreamReader r(data.data(), data.size());
  const int32_t expected[] = { 0, 1, -1, 2, -2, 3, -3 };
  for (int32_t e : expected)
    EXPECT_EQ(r.getGolombS(), e);
}

TEST(BitstreamReader, ReadingPastTheEndThrows)
{
  const std::vector<uint8_t> data = { 0xAA };
  BitstreamReader r(data.data(), data.size());
  r.getBits(8);
  EXPECT_THROW(r.getBit(), std::runtime_error);
}

TEST(BitstreamReader, EmptyBufferThrowsOnFirstRead)
{
  BitstreamReader r(nullptr, 0);
  EXPECT_THROW(r.getBit(), std::runtime_error);
}

TEST(BitstreamReader, ReadingTheLastBitsNeverTouchesTheMemoryBehindTheBuffer)
{
  // The reader looks for "00 00 03" (emulation prevention) before the byte it is about to read. When the buffer
  // ends in 00 00 it must not look at the byte behind the buffer: the data ends exactly at a no-access page.
  for (size_t size : { size_t(3), size_t(4), size_t(16) })
  {
    GuardedBuffer buffer(std::vector<uint8_t>(size, 0x00));
    BitstreamReader r(buffer.data(), buffer.size());
    for (size_t i = 0; i < size * 8; i++)
      EXPECT_FALSE(r.getBit());
    EXPECT_THROW(r.getBit(), std::runtime_error);
  }
}

TEST(BitstreamReader, SkippingToTheEndNeverTouchesTheMemoryBehindTheBuffer)
{
  GuardedBuffer buffer(std::vector<uint8_t>(8, 0x00));
  BitstreamReader r(buffer.data(), buffer.size());
  r.skipBits(8 * 8);
  EXPECT_THROW(r.getBit(), std::runtime_error);
}

TEST(BitstreamReader, GolombWithTooManyZerosGivesZeroInsteadOfCrashing)
{
  std::vector<uint8_t> data(16, 0x00);
  data[15] = 0x01;
  BitstreamReader r(data.data(), data.size());
  EXPECT_EQ(r.getGolombU(), 0u);
}

TEST(BitstreamReader, AvailableInNaluStopsAtTheNextStartCode)
{
  const std::vector<uint8_t> data = { 0x11, 0x22, 0x33, 0x00, 0x00, 0x01, 0x44 };
  BitstreamReader r(data.data(), data.size());
  EXPECT_LE(r.availableInNalU(), 3u * 8u + 8u);
}
