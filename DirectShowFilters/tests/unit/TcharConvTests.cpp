#include <gtest/gtest.h>
#include <cstring>
#include "TcharConv.h"

TEST(TcharConv, AsciiRoundTrip)
{
  EXPECT_EQ(ToAnsi(L"Hello 123"), "Hello 123");
  EXPECT_EQ(ToWide("Hello 123"), L"Hello 123");
}

TEST(TcharConv, SameTypeIsACopy)
{
  EXPECT_EQ(ToAnsi("abc"), "abc");
  EXPECT_EQ(ToWide(L"abc"), L"abc");
}

TEST(TcharConv, NullAndEmptyGiveAnEmptyString)
{
  EXPECT_TRUE(ToAnsi(static_cast<const char*>(nullptr)).empty());
  EXPECT_TRUE(ToAnsi(static_cast<const wchar_t*>(nullptr)).empty());
  EXPECT_TRUE(ToWide(static_cast<const char*>(nullptr)).empty());
  EXPECT_TRUE(ToWide(static_cast<const wchar_t*>(nullptr)).empty());
  EXPECT_TRUE(ToAnsi(L"").empty());
  EXPECT_TRUE(ToWide("").empty());
}

TEST(TcharConv, ToTcharFollowsTheBuildCharacterSet)
{
  tstring t = ToTchar(L"Pin 42");
  EXPECT_EQ(t, tstring(TEXT("Pin 42")));
  EXPECT_EQ(sizeof(t[0]), sizeof(TCHAR));
}

TEST(TcharConv, LongStringsAreNotTruncated)
{
  std::wstring wide(100000, L'x');
  std::string narrow = ToAnsi(wide.c_str());
  ASSERT_EQ(narrow.size(), wide.size());
  EXPECT_EQ(narrow.find_first_not_of('x'), std::string::npos);
  EXPECT_EQ(ToWide(narrow.c_str()), wide);
}

TEST(TcharConv, ResultIsValidForTheWholeCall)
{
  // The filters pass ToAnsi(x).c_str() straight into a call: the pointer has to stay valid for that call.
  auto length = [](const char* s) { return strlen(s); };
  EXPECT_EQ(length(ToAnsi(L"twelve chars").c_str()), 12u);
}

TEST(TcharConv, NonAsciiDoesNotCrash)
{
  // The result depends on the ANSI code page of the machine, so only check that something comes back.
  const wchar_t text[] = L"éЖ中";
  std::string narrow = ToAnsi(text);
  EXPECT_FALSE(narrow.empty());
  EXPECT_FALSE(ToWide(narrow.c_str()).empty());
}
