// Tests of mpc-hc_subs/src/dsutil/StrUtils.h: what the MPC-HC code did with CString (MFC/ATL), now functions on std::basic_string.
// The expected values are what CString gave.
#include <gtest/gtest.h>

#include "StrUtils.h"

TEST(StrUtils, FormatUsesTheWidthOfTheString)
{
  EXPECT_EQ(Format(L"%s-%d-%S", L"ab", 5, "cd"), L"ab-5-cd");
  EXPECT_EQ(Format("%s-%d-%S", "ab", 5, L"cd"), "ab-5-cd");
  EXPECT_TRUE(Format(L"").empty());
  const std::wstring fmt = L"%03d";
  EXPECT_EQ(Format(fmt, 7), L"007");
}

TEST(StrUtils, LeftRightMidAreClamped)
{
  const std::wstring s = L"abcdef";
  EXPECT_EQ(StringLeft(s, 2), L"ab");
  EXPECT_EQ(StringLeft(s, 99), L"abcdef");
  EXPECT_EQ(StringLeft(s, 0), L"");
  EXPECT_EQ(StringRight(s, 2), L"ef");
  EXPECT_EQ(StringRight(s, 99), L"abcdef");
  EXPECT_EQ(StringMid(s, 2), L"cdef");
  EXPECT_EQ(StringMid(s, 2, 2), L"cd");
  EXPECT_EQ(StringMid(s, 4, 99), L"ef");
  EXPECT_EQ(StringMid(s, 6), L"");
  EXPECT_EQ(StringMid(s, 7), L"");
}

TEST(StrUtils, FindFamilyReturnsMinusOneWhenNothingIsThere)
{
  const std::wstring s = L"a,b;c,d";
  EXPECT_EQ(StringFind(s, L','), 1);
  EXPECT_EQ(StringFind(s, L',', 2), 5);
  EXPECT_EQ(StringFind(s, L"b;"), 2);
  EXPECT_EQ(StringFind(s, L"zz"), -1);
  EXPECT_EQ(StringFind(s, L"a", 99), -1);
  EXPECT_EQ(StringFind(s, '.'), -1);  // a narrow character is taken for a wide one
  EXPECT_EQ(StringRFind(s, L','), 5);
  EXPECT_EQ(StringRFind(s, L'z'), -1);
  EXPECT_EQ(StringFindOneOf(s, L";,"), 1);
  EXPECT_EQ(StringFindOneOf(s, L"xyz"), -1);
}

TEST(StrUtils, ReplaceRemoveInsertDeleteReturnTheCount)
{
  std::wstring s = L"aXbXc";
  EXPECT_EQ(StringReplace(s, L'X', L'-'), 2);
  EXPECT_EQ(s, L"a-b-c");
  EXPECT_EQ(StringReplace(s, L"-", L"+++"), 2);
  EXPECT_EQ(s, L"a+++b+++c");
  EXPECT_EQ(StringReplace(s, L"zz", L"y"), 0);
  EXPECT_EQ(StringRemove(s, L'+'), 6);
  EXPECT_EQ(s, L"abc");
  StringInsert(s, 1, L"__");
  EXPECT_EQ(s, L"a__bc");
  StringInsert(s, 99, L'!');
  EXPECT_EQ(s, L"a__bc!");
  StringDelete(s, 1, 2);
  EXPECT_EQ(s, L"abc!");
  StringDelete(s, 99);
  EXPECT_EQ(s, L"abc!");
  StringTruncate(s, 2);
  EXPECT_EQ(s, L"ab");
  StringTruncate(s, 9);
  EXPECT_EQ(s, L"ab");
  std::string narrow = "a/b";
  EXPECT_EQ(StringReplace(narrow, '/', '\\'), 1);
  EXPECT_EQ(narrow, "a\\b");
}

TEST(StrUtils, TrimFamily)
{
  const std::wstring s = L"  \t x y \r\n";
  std::wstring a = s;
  EXPECT_EQ(StringTrim(a), L"x y");
  a = s;
  EXPECT_EQ(StringTrimLeft(a), L"x y \r\n");
  a = s;
  EXPECT_EQ(StringTrimRight(a), L"  \t x y");
  std::wstring t = L"--ab--";
  EXPECT_EQ(StringTrim(t, L'-'), L"ab");
  t = L"--ab--";
  EXPECT_EQ(StringTrim(t, L"-a"), L"b");
  std::wstring blank = L"   ";
  EXPECT_EQ(StringTrim(blank), L"");
  EXPECT_EQ(StringTrimmed(std::wstring(L" k ")), L"k");
}

TEST(StrUtils, TokenizeSkipsEmptyTokens)
{
  const std::wstring s = L"a,,b; c";
  int pos = 0;
  EXPECT_EQ(StringTokenize(s, L",; ", pos), L"a");
  EXPECT_EQ(StringTokenize(s, L",; ", pos), L"b");
  EXPECT_EQ(StringTokenize(s, L",; ", pos), L"c");
  EXPECT_EQ(pos, -1);
  EXPECT_EQ(StringTokenize(s, L",; ", pos), L"");
}

TEST(StrUtils, CompareFamily)
{
  const std::wstring s = L"Abc";
  EXPECT_EQ(StringCompare(s, L"Abc"), 0);
  EXPECT_NE(StringCompare(s, L"abc"), 0);
  EXPECT_EQ(StringICompare(s, L"aBC"), 0);
  EXPECT_LT(StringICompare(s, L"abd"), 0);
  EXPECT_LT(StringICompare(s, std::wstring(L"ABD")), 0);
  EXPECT_TRUE(StringIEquals(s, L"ABC"));
  EXPECT_FALSE(StringIEquals(s, L"ABCD"));
}

TEST(StrUtils, ConversionsBetweenNarrowAndWide)
{
  const std::string a = "caf\xe9";
  const std::wstring w = StringWiden(a.c_str());
  EXPECT_EQ(w.size(), 4u);
  EXPECT_EQ(w[3], L'\xe9');
  EXPECT_EQ(StringNarrow(w.c_str()), a);
  EXPECT_EQ(StringToW(a), w);
  EXPECT_EQ(StringToA(w), a);
  EXPECT_EQ(StringToT(a), tstring(w));
  EXPECT_EQ(StringToT("abc"), tstring(_T("abc")));
  EXPECT_EQ(StringToT(L"abc"), tstring(_T("abc")));
  EXPECT_TRUE(StringWiden(static_cast<const char*>(nullptr)).empty());
  EXPECT_TRUE(StringNarrow(static_cast<const wchar_t*>(nullptr)).empty());
}

TEST(StrUtils, BufferAccessAndRelease)
{
  std::wstring s;
  wchar_t* p = StringBuffer(s, 10);
  wcscpy_s(p, 10, L"abc");
  StringRelease(s);
  EXPECT_EQ(s, L"abc");
  StringBuffer(s, 2);
  EXPECT_EQ(s.size(), 3u);  // never shorter than what is there
  StringRelease(s, 1);
  EXPECT_EQ(s, L"a");
}

TEST(StrUtils, CaseAndSpans)
{
  std::wstring s = L"aBc";
  EXPECT_EQ(StringUppered(s), L"ABC");
  EXPECT_EQ(StringLowered(s), L"abc");
  EXPECT_EQ(s, L"aBc");
  EXPECT_EQ(StringUpper(s), L"ABC");
  EXPECT_EQ(StringLower(s), L"abc");
  const std::wstring d = L"123abc";
  EXPECT_EQ(StringSpanIncluding(d, L"0123456789"), L"123");
  EXPECT_EQ(StringSpanExcluding(d, L"abc"), L"123");
  BSTR b = StringAllocSys(d);
  EXPECT_EQ(SysStringLen(b), 6u);
  SysFreeString(b);
}

TEST(StrUtils, StringsAreKeysOfAMap)
{
  std::map<std::wstring, int> m;
  m[L"one"] = 1;
  m[L"two"] = 2;
  EXPECT_EQ(m.at(L"two"), 2);
  EXPECT_EQ(m.count(L"three"), 0u);
}
