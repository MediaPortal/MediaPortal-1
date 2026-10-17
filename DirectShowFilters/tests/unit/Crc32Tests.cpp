#include <gtest/gtest.h>
#include <windows.h>
#include "DvbUtil.h"

// crc32() is the CRC-32/MPEG-2 of PSI sections: init 0xFFFFFFFF, no reflection, no final xor.

TEST(Crc32, StandardCheckValue)
{
  char data[] = "123456789";
  EXPECT_EQ(crc32(data, 9), 0x0376E6E7u);
}

TEST(Crc32, EmptyInputGivesTheInitialValue)
{
  char data[] = "";
  EXPECT_EQ(crc32(data, 0), 0xFFFFFFFFu);
}

TEST(Crc32, ASectionWithItsCrcAppendedChecksToZero)
{
  // PAT with one program (1 -> PID 0x100); the CRC covers the bytes before it.
  unsigned char section[16] = { 0x00, 0xB0, 0x0D, 0x00, 0x01, 0xC1, 0x00, 0x00, 0x00, 0x01, 0xE1, 0x00 };
  DWORD crc = crc32(reinterpret_cast<char*>(section), 12);
  section[12] = static_cast<unsigned char>(crc >> 24);
  section[13] = static_cast<unsigned char>(crc >> 16);
  section[14] = static_cast<unsigned char>(crc >> 8);
  section[15] = static_cast<unsigned char>(crc);
  EXPECT_EQ(crc32(reinterpret_cast<char*>(section), 16), 0u);
}

TEST(Crc32, AChangedBitIsDetected)
{
  char a[] = { 1, 2, 3, 4, 5, 6, 7, 8 };
  char b[] = { 1, 2, 3, 4, 5, 6, 7, 9 };
  EXPECT_NE(crc32(a, 8), crc32(b, 8));
}
