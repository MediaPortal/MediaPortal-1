#include <gtest/gtest.h>
#include "WStringUtil.h"

TEST(ReplaceAll, ReplacesEveryOccurrenceAndCountsThem)
{
  std::wstring s = L"a.ts.tsbuffer and b.ts.tsbuffer";
  EXPECT_EQ(ReplaceAll(s, L".ts.tsbuffer", L"X"), 2);
  EXPECT_EQ(s, L"aX and bX");
}

TEST(ReplaceAll, NothingToReplace)
{
  std::wstring s = L"file.ts";
  EXPECT_EQ(ReplaceAll(s, L".ts.tsbuffer", L"X"), 0);
  EXPECT_EQ(s, L"file.ts");
}

TEST(ReplaceAll, EmptySearchStringReplacesNothing)
{
  std::wstring s = L"abc";
  EXPECT_EQ(ReplaceAll(s, L"", L"X"), 0);
  EXPECT_EQ(s, L"abc");
}

TEST(ReplaceAll, ReplacementContainingTheSearchStringDoesNotLoop)
{
  std::wstring s = L"aaa";
  EXPECT_EQ(ReplaceAll(s, L"a", L"aa"), 3);
  EXPECT_EQ(s, L"aaaaaa");
}

TEST(ReplaceAll, ReplacementCanBeEmptyOrLonger)
{
  std::wstring s = L"x-y-z";
  EXPECT_EQ(ReplaceAll(s, L"-", L""), 2);
  EXPECT_EQ(s, L"xyz");
  EXPECT_EQ(ReplaceAll(s, L"y", L"<<<>>>"), 1);
  EXPECT_EQ(s, L"x<<<>>>z");
}

TEST(ReplaceAll, TheShapeUsedByTheFileReader)
{
  // The dummy file name replaces ".ts.tsbuffer" in the time shift file name.
  std::wstring s = L"C:\\tv\\live3.ts.tsbuffer";
  EXPECT_EQ(ReplaceAll(s, L".ts.tsbuffer", L"abcdefghijkl"), 1);
  EXPECT_EQ(s, L"C:\\tv\\live3abcdefghijkl");
}
