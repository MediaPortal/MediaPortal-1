// Tests of mpc-hc_subs/src/dsutil/RectUtils.h: the helpers on top of RECT, POINT and SIZE of the Windows SDK.
#include <gtest/gtest.h>

#include "RectUtils.h"

TEST(RectUtils, MakeAndMeasure)
{
  const RECT r = MakeRect(10, 20, 110, 70);
  EXPECT_EQ(RectWidth(r), 100);
  EXPECT_EQ(RectHeight(r), 50);
  EXPECT_EQ(RectSize(r), MakeSize(100, 50));
  EXPECT_EQ(RectTopLeft(r), MakePoint(10, 20));
  EXPECT_EQ(RectBottomRight(r), MakePoint(110, 70));
  EXPECT_EQ(RectCenter(r), MakePoint(60, 45));
  EXPECT_TRUE(RectEqual(MakeRect(MakePoint(10, 20), MakeSize(100, 50)), r));
  EXPECT_TRUE(RectEqual(MakeRect(MakePoint(10, 20), MakePoint(110, 70)), r));
  EXPECT_FALSE(RectEqual(r, MakeRect(0, 0, 1, 1)));
}

TEST(RectUtils, EmptyAndNull)
{
  EXPECT_TRUE(RectIsNull(MakeRect(0, 0, 0, 0)));
  EXPECT_FALSE(RectIsNull(MakeRect(0, 0, 0, 1)));
  EXPECT_TRUE(RectIsEmpty(MakeRect(5, 5, 5, 9)));
  EXPECT_FALSE(RectIsEmpty(MakeRect(5, 5, 6, 9)));
}

TEST(RectUtils, NormalizeSwapsOnlyTheWrongSides)
{
  RECT r = MakeRect(10, 2, 4, 8);
  NormalizeRect(r);
  EXPECT_TRUE(RectEqual(r, MakeRect(4, 2, 10, 8)));
  RECT ok = MakeRect(1, 2, 3, 4);
  NormalizeRect(ok);
  EXPECT_TRUE(RectEqual(ok, MakeRect(1, 2, 3, 4)));
}

TEST(RectUtils, IntersectUnionOffset)
{
  const RECT a = MakeRect(0, 0, 10, 10), b = MakeRect(5, 5, 20, 20);
  EXPECT_TRUE(RectEqual(RectIntersect(a, b), MakeRect(5, 5, 10, 10)));
  EXPECT_TRUE(RectEqual(RectUnion(a, b), MakeRect(0, 0, 20, 20)));
  EXPECT_TRUE(RectIsEmpty(RectIntersect(a, MakeRect(50, 50, 60, 60))));
  EXPECT_TRUE(RectEqual(RectOffset(a, 3, -2), MakeRect(3, -2, 13, 8)));
  RECT m = a;
  RectMove(m, MakePoint(1, 2));
  RectMove(m, MakeSize(1, 1));
  EXPECT_TRUE(RectEqual(m, MakeRect(2, 3, 12, 13)));
}

TEST(RectUtils, InflateMovesEverySideByItsOwnValue)
{
  // what CRect did for rect + rect: left and top go out by the values of 'by', right and bottom too
  EXPECT_TRUE(RectEqual(RectInflate(MakeRect(10, 10, 20, 20), MakeRect(1, 2, 3, 4)), MakeRect(9, 8, 23, 24)));
  RECT r = MakeRect(10, 10, 20, 20);
  DeflateRect(r, 2, 3);
  EXPECT_TRUE(RectEqual(r, MakeRect(12, 13, 18, 17)));
}

TEST(RectUtils, PointAndSizeOperators)
{
  EXPECT_EQ(MakePoint(1, 2) + MakePoint(3, 4), MakePoint(4, 6));
  EXPECT_EQ(MakePoint(5, 5) - MakePoint(2, 1), MakePoint(3, 4));
  EXPECT_EQ(-MakePoint(1, -2), MakePoint(-1, 2));
  EXPECT_EQ(MakeSize(1, 2) + MakeSize(3, 4), MakeSize(4, 6));
  EXPECT_EQ(MakeSize(5, 5) - MakeSize(2, 1), MakeSize(3, 4));
  EXPECT_EQ(-MakeSize(1, -2), MakeSize(-1, 2));
  EXPECT_NE(MakeSize(1, 2), MakeSize(1, 3));
}
