// CTextUtil of TsWriter: DVB text (ETSI EN 300 468 annex A) to the UTF-8 based form that the TV Server database takes. The text of
// service names and of the electronic programme guide comes from the air, so the limits of the buffers matter as much as the
// character tables.
#include <gtest/gtest.h>
#include <windows.h>
#include <string>
#include <vector>
#include "TextUtil.h"
#include "../unit/GuardedBuffer.h"

namespace
{
  // The text the way the TV Server reads it: the encoding byte 0x15 (UTF-8) in front of the characters
  std::string Convert(const std::vector<BYTE>& in, int textLen = 256, bool bbcHuffman = false, int* result = nullptr)
  {
    CTextUtil util;
    std::vector<char> out(textLen, 'X');
    std::vector<BYTE> input = in;
    const int n = util.DvbTextToString(input.data(), static_cast<int>(input.size()), out.data(), textLen, bbcHuffman);
    if (result)
      *result = n;
    return std::string(out.data());                          // up to the terminating NUL
  }

  std::vector<BYTE> Bytes(std::initializer_list<int> v)
  {
    std::vector<BYTE> b;
    for (int x : v)
      b.push_back(static_cast<BYTE>(x));
    return b;
  }

  std::vector<BYTE> Ascii(const char* s) { return std::vector<BYTE>(s, s + strlen(s)); }

  std::string Utf8(const std::string& rest) { return std::string("\x15") + rest; }

  // Sets the "pass through" switch of the registry settings for the duration of a test
  struct PassThru
  {
    explicit PassThru(bool on) : old(CRegistryUtil::m_bPassThruISO6937) { CRegistryUtil::m_bPassThruISO6937 = on; }
    ~PassThru() { CRegistryUtil::m_bPassThruISO6937 = old; }
    bool old;
  };
}

// --- the default table (ISO 6937) --------------------------------------------------------------------------------------------

TEST(TextUtil, PlainAsciiGetsTheUtf8Marker)
{
  int n = 0;
  EXPECT_EQ(Convert(Ascii("Das Erste HD"), 256, false, &n), Utf8("Das Erste HD"));
  EXPECT_EQ(n, 1 + 12 + 1) << "marker, text and the terminating NUL";
}

TEST(TextUtil, EuroSignAndOtherSingleByteCharacters)
{
  EXPECT_EQ(Convert(Bytes({ 'a', 0xA4, 'b' })), Utf8("a\xE2\x82\xAC" "b"));            // U+20AC
  EXPECT_EQ(Convert(Bytes({ 0xE9 })), Utf8("\xC3\x98"));                                // U+00D8 O with stroke
  EXPECT_EQ(Convert(Bytes({ 0xFB })), Utf8("\xC3\x9F"));                                // U+00DF sharp s
  EXPECT_EQ(Convert(Bytes({ 0xD4 })), Utf8("\xE2\x84\xA2"));                            // U+2122 trade mark
  EXPECT_EQ(Convert(Bytes({ 0xAC })), Utf8("\xE2\x86\x90"));                            // U+2190 left arrow
}

TEST(TextUtil, AccentedLettersAreTwoBytesInIso6937)
{
  EXPECT_EQ(Convert(Bytes({ 0xC2, 'e' })), Utf8("\xC3\xA9"));                           // acute: e -> U+00E9
  EXPECT_EQ(Convert(Bytes({ 0xC2, 'A' })), Utf8("\xC3\x81"));                           // U+00C1
  EXPECT_EQ(Convert(Bytes({ 0xC1, 'a' })), Utf8("\xC3\xA0"));                           // grave: U+00E0
  EXPECT_EQ(Convert(Bytes({ 0xC8, 'u' })), Utf8("\xC3\xBC"));                           // diaeresis: U+00FC
  EXPECT_EQ(Convert(Bytes({ 0xC8, 'O' })), Utf8("\xC3\x96"));                           // U+00D6
  EXPECT_EQ(Convert(Bytes({ 0xCA, 'a' })), Utf8("\xC3\xA5"));                           // ring: U+00E5
  EXPECT_EQ(Convert(Bytes({ 0xCB, 'c' })), Utf8("\xC3\xA7"));                           // cedilla: U+00E7
  EXPECT_EQ(Convert(Bytes({ 0xCF, 'c' })), Utf8("\xC4\x8D"));                           // caron: U+010D
  EXPECT_EQ(Convert(Bytes({ 0xCF, 'S' })), Utf8("\xC5\xA0"));                           // U+0160
}

TEST(TextUtil, AMissingSecondByteOfAnAccentEndsTheText)
{
  EXPECT_EQ(Convert(Bytes({ 'a', 'b', 0xC2 })), Utf8("ab"));
}

TEST(TextUtil, AnUnknownBaseLetterIsTakenAsItIs)
{
  EXPECT_EQ(Convert(Bytes({ 0xC2, 'x' })), Utf8("x"));
}

TEST(TextUtil, LineBreaksBecomeCarriageReturns)
{
  EXPECT_EQ(Convert(Bytes({ 'a', 0x8A, 'b', 0x0A, 'c', 0x0D })), Utf8("a\rb\rc\r"));
}

TEST(TextUtil, ControlCodesAreDropped)
{
  EXPECT_EQ(Convert(Bytes({ 0x20, 0x00, 'a', 0x01 })), Utf8(" a"));                       // 0x00 and 0x01 are skipped
  EXPECT_EQ(Convert(Bytes({ 'a', 0x86, 0x87, 'b', 0x9F })), Utf8("ab"));                  // emphasis on / off and the C1 range
}

// --- the other encodings -------------------------------------------------------------------------------------------------

TEST(TextUtil, Ucs2IsRecodedToUtf8)
{
  EXPECT_EQ(Convert(Bytes({ 0x11, 0x00, 'A', 0x00, 'b' })), Utf8("Ab"));
  EXPECT_EQ(Convert(Bytes({ 0x11, 0x00, 0xE9 })), Utf8("\xC3\xA9"));
  EXPECT_EQ(Convert(Bytes({ 0x11, 0x20, 0xAC })), Utf8("\xE2\x82\xAC"));
  EXPECT_EQ(Convert(Bytes({ 0x11, 0x04, 0x16 })), Utf8("\xD0\x96"));                       // Cyrillic zhe
  EXPECT_EQ(Convert(Bytes({ 0x11, 0x00, 'a', 0xE0, 0x8A, 0x00, 'b' })), Utf8("a\rb"));     // E08A: DVB line break
}

TEST(TextUtil, Ucs2WithAnOddByteCountIgnoresTheLastByte)
{
  EXPECT_EQ(Convert(Bytes({ 0x11, 0x00, 'A', 0x00 })), Utf8("A"));
}

TEST(TextUtil, Utf8InputIsKept)
{
  EXPECT_EQ(Convert(Bytes({ 0x15, 'a', 0xC3, 0xA9, 'z' })), Utf8("a\xC3\xA9z"));
  EXPECT_EQ(Convert(Bytes({ 0x15, 'a', 0x0A, 'b', 0x00, 'c' })), Utf8("a\rbc"));
}

TEST(TextUtil, OtherCodeTablesKeepTheirSelectorByte)
{
  // 0x01 to 0x0B are the ISO 8859 tables: the selector goes to the output, the rest is copied as it is
  EXPECT_EQ(Convert(Bytes({ 0x05, 'a', 0xE0, 'b' })), std::string("\x05" "a\xE0" "b"));
  // 0x10 + two more bytes: the 3 byte form for ISO 8859-x
  EXPECT_EQ(Convert(Bytes({ 0x10, 0x00, 0x09, 'a', 'b' })), std::string("\x10\x20\x09" "ab"));
  // 0x10 with a table number outside 1..15 is not accepted: nothing of the text is kept
  EXPECT_EQ(Convert(Bytes({ 0x10, 0x00, 0x20, 'a', 'b' })), std::string(""));
}

TEST(TextUtil, PassThroughKeepsTheDefaultTableUndecoded)
{
  PassThru on(true);
  EXPECT_EQ(Convert(Bytes({ 'a', 0xC2, 'e' })), std::string("a\xC2" "e"));
}

TEST(TextUtil, UnknownSelectorsEndTheText)
{
  // 0x12 to 0x14 and 0x16 to 0x1E are reserved: treated like the single byte tables, kept as a selector
  EXPECT_EQ(Convert(Bytes({ 0x12, 'a' })), std::string("\x12" "a"));
}

// --- the limits ----------------------------------------------------------------------------------------------------------------

TEST(TextUtil, RefusesNullAndEmptyInput)
{
  CTextUtil util;
  char out[16];
  BYTE in[4] = { 'a', 'b', 'c', 'd' };
  EXPECT_EQ(util.DvbTextToString(nullptr, 4, out, 16, false), 0);
  EXPECT_EQ(util.DvbTextToString(in, 4, nullptr, 16, false), 0);
  EXPECT_EQ(util.DvbTextToString(in, 0, out, 16, false), 0);
  EXPECT_EQ(util.DvbTextToString(in, -1, out, 16, false), 0);
  EXPECT_EQ(util.DvbTextToString(in, 4, out, 1, false), 0);
}

TEST(TextUtil, ShortOutputBufferIsNeverOverrun)
{
  // a buffer of exactly textLen bytes sits in front of a guard page; a long text of every kind has to stop in time
  const std::vector<std::vector<BYTE>> inputs = {
    std::vector<BYTE>(300, 'a'),
    [] { std::vector<BYTE> v(300, 0xA4); return v; }(),                                       // 3 byte characters
    [] { std::vector<BYTE> v{ 0x15 }; v.resize(300, 'u'); return v; }(),
    [] { std::vector<BYTE> v{ 0x11 }; for (int i = 0; i < 150; i++) { v.push_back(0x20); v.push_back(0xAC); } return v; }(),
    [] { std::vector<BYTE> v{ 0x05 }; v.resize(300, 'z'); return v; }(),
  };
  for (const auto& in : inputs)
    for (int size = 2; size <= 40; size++)
    {
      GuardedBuffer out(size, 'X');
      std::vector<BYTE> copy = in;
      CTextUtil util;
      const int n = util.DvbTextToString(copy.data(), static_cast<int>(copy.size()), reinterpret_cast<char*>(out.data()), size, false);
      ASSERT_LE(n, size) << "input " << static_cast<int>(in[0]) << " size " << size;
      ASSERT_GE(n, 1);
      EXPECT_EQ(out.data()[n - 1], 0) << "the result ends with a NUL";
    }
}

TEST(TextUtil, TheCheckForRoomKeepsOneMoreByteFreeThanNeeded)
{
  // textLen is reduced by one for the NUL and the room check "textIndex + width >= textLen" keeps another byte free: with
  // 8 bytes of buffer the result is the marker and 5 characters (6 bytes) and the NUL, one byte is left unused
  int n = 0;
  EXPECT_EQ(Convert(Ascii("abcdef"), 8, false, &n), Utf8("abcde"));
  EXPECT_EQ(n, 7);
  EXPECT_EQ(Convert(Ascii("abc"), 8, false, &n), Utf8("abc"));
  EXPECT_EQ(n, 5);
}

TEST(TextUtil, ThreeByteSelectorOnTooShortInputDoesNotReadPastIt)
{
  for (int len = 1; len <= 2; len++)
  {
    GuardedBuffer in(len, 0x10);
    char out[16];
    CTextUtil util;
    util.DvbTextToString(in.data(), len, out, 16, false);
  }
}

TEST(TextUtil, RandomBytesNeverOverrunInputOrOutput)
{
  uint32_t seed = 4711;
  auto next = [&seed]() { seed = seed * 1664525u + 1013904223u; return seed >> 8; };
  const int outSize = 48;
  for (int i = 0; i < 30000; i++)
  {
    const int len = 1 + next() % 40;
    GuardedBuffer in(len);
    for (int k = 0; k < len; k++)
      in.data()[k] = static_cast<uint8_t>(next());
    // the first byte is the encoding selector: try all of them but the three byte one (see the disabled test)
    if (in.data()[0] == 0x10 && len < 3)
      in.data()[0] = 0x11;
    GuardedBuffer out(outSize, 'X');
    CTextUtil util;
    const int n = util.DvbTextToString(in.data(), len, reinterpret_cast<char*>(out.data()), outSize, (i & 1) != 0);
    ASSERT_LE(n, outSize);
    if (n > 0)
      ASSERT_EQ(out.data()[n - 1], 0);
  }
}

// --- BBC / Freesat Huffman -----------------------------------------------------------------------------------------------------

TEST(TextUtil, HuffmanIsOnlyUsedWhenAllowed)
{
  // 0x1F is a table selector like the others when the Huffman decoding is not allowed: kept, then the printable rest
  EXPECT_EQ(Convert(Bytes({ 0x1F, 0x01, 'a' }), 64, false), std::string(1, static_cast<char>(0x1F)) + "a");
}

TEST(TextUtil, HuffmanTextWithAnUnknownTableGivesJustTheMarker)
{
  EXPECT_EQ(Convert(Bytes({ 0x1F, 0x07, 0x00, 0x00, 0x00 }), 64, true), Utf8(""));
}

TEST(TextUtil, HuffmanTextThatIsTooShortGivesNothing)
{
  int n = 0;
  Convert(Bytes({ 0x1F, 0x01 }), 64, true, &n);
  EXPECT_EQ(n, 1) << "only the NUL";
}

TEST(TextUtil, HuffmanDecodingOfRandomBitsStaysInsideTheBuffers)
{
  uint32_t seed = 99;
  auto next = [&seed]() { seed = seed * 1664525u + 1013904223u; return seed >> 8; };
  for (int table = 1; table <= 2; table++)
    for (int i = 0; i < 3000; i++)
    {
      const int len = 3 + next() % 60;
      GuardedBuffer in(len);
      for (int k = 0; k < len; k++)
        in.data()[k] = static_cast<uint8_t>(next());
      in.data()[0] = 0x1F;
      in.data()[1] = static_cast<uint8_t>(table);
      const int outSize = 2 + next() % 80;
      GuardedBuffer out(outSize, 'X');
      CTextUtil util;
      const int n = util.DvbTextToString(in.data(), len, reinterpret_cast<char*>(out.data()), outSize, true);
      ASSERT_LE(n, outSize);
      ASSERT_GE(n, 1);
      ASSERT_EQ(out.data()[n - 1], 0);
    }
}

// --- hexStr --------------------------------------------------------------------------------------------------------------------

TEST(TextUtil, HexStringOfBytes)
{
  CTextUtil util;
  const std::string hex = util.hexStr(std::string("\x01\xAB\xFF" "a", 4));
  EXPECT_EQ(hex.substr(0, 8), "01abff61");
  EXPECT_EQ(hex.size(), 9u) << "a NUL is appended to the string";
  EXPECT_EQ(hex.back(), '\0');
  EXPECT_EQ(util.hexStr(std::string()), std::string(1, '\0'));
}
