// The self contained parts of DirectShowHelper (dshowhelper.dll): the 128 bit multiply/divide and the clock of the EVR presenter
// (TimeSource.cpp), the XBMC derived millisecond clock and EndTime (threads/SystemClock), the ring buffer CMyQueue and CAutoInt.
// The presenters themselves need Direct3D and a video renderer: see the integration and local tests.
#include <gtest/gtest.h>
#include <windows.h>
#include <streams.h>
#include <map>
#include <thread>
#include "MyQueue.h"
#include "autoint.h"
#include "threads/SystemClock.h"

// dshowhelper.cpp provides the log function that MyQueue.h calls
void Log(const char* /*fmt*/, ...) {}

__int64 _stdcall cMulDiv64(__int64 operant, __int64 multiplier, __int64 divider);
LONGLONG GetCurrentTimestamp();

// --- cMulDiv64 ----------------------------------------------------------------------------------------------------------------

namespace
{
  struct MulDivCase { __int64 a, b, c, result; };

  const MulDivCase kVectors[] = {
#include "MulDivVectors.inc"
  };
}

TEST(MulDiv64, MatchesExactIntegerArithmetic)
{
  // operant * multiplier / divider with a 128 bit intermediate result, truncated towards zero
  for (const MulDivCase& v : kVectors)
    EXPECT_EQ(cMulDiv64(v.a, v.b, v.c), v.result) << v.a << " * " << v.b << " / " << v.c;
}

TEST(MulDiv64, ZeroAndOneAreNeutral)
{
  EXPECT_EQ(cMulDiv64(0, 123, 7), 0);
  EXPECT_EQ(cMulDiv64(123, 0, 7), 0);
  EXPECT_EQ(cMulDiv64(123456789012LL, 1, 1), 123456789012LL);
  EXPECT_EQ(cMulDiv64(123456789012LL, 5, 5), 123456789012LL);
}

TEST(MulDiv64, TheSignFollowsTheOperands)
{
  EXPECT_EQ(cMulDiv64(10, 10, 3), 33);
  EXPECT_EQ(cMulDiv64(-10, 10, 3), -33);
  EXPECT_EQ(cMulDiv64(10, -10, 3), -33);
  EXPECT_EQ(cMulDiv64(10, 10, -3), -33);
  EXPECT_EQ(cMulDiv64(-10, -10, 3), 33);
  EXPECT_EQ(cMulDiv64(-10, -10, -3), -33);
}

TEST(MulDiv64, ProductBeyond64BitDoesNotOverflow)
{
  const __int64 big = 0x7FFFFFFFFFFFFFFFLL;
  EXPECT_EQ(cMulDiv64(big, 10000000, big), 10000000);
  EXPECT_EQ(cMulDiv64(big, big, big), big);
  EXPECT_EQ(cMulDiv64(1LL << 62, 4, 8), 1LL << 61);
}

TEST(MulDiv64, QueryPerformanceCounterToHundredNanoseconds)
{
  // the way GetCurrentTimestamp() uses it: ticks of a 10 MHz and of a 3.579545 MHz counter to 100 ns units
  EXPECT_EQ(cMulDiv64(10000000, 10000000, 10000000), 10000000);
  EXPECT_EQ(cMulDiv64(3579545, 10000000, 3579545), 10000000);
  EXPECT_EQ(cMulDiv64(1, 10000000, 3579545), 2);                      // 2.79 truncated
  EXPECT_EQ(cMulDiv64(0x7FFFFFFFFFFFLL, 10000000, 3579545), 0x7FFFFFFFFFFFLL / 3579545 * 10000000 + (0x7FFFFFFFFFFFLL % 3579545) * 10000000 / 3579545);
}

TEST(MulDiv64, DifferentWaysThroughTheDivisionPaths)
{
  // 32 bit divisor path and 64 bit divisor path give the same result for the same quotient
  const __int64 a = 0x123456789ABCLL;
  EXPECT_EQ(cMulDiv64(a, 1000, 1000), a);
  EXPECT_EQ(cMulDiv64(a, 1LL << 40, 1LL << 40), a);
  EXPECT_EQ(cMulDiv64(a, (1LL << 40) + 1, (1LL << 40) + 1), a);
}

// --- GetCurrentTimestamp ------------------------------------------------------------------------------------------------------

TEST(GetCurrentTimestamp, RunsForwardInUnitsOf100Nanoseconds)
{
  const LONGLONG t0 = GetCurrentTimestamp();
  Sleep(100);
  const LONGLONG t1 = GetCurrentTimestamp();
  const double ms = (t1 - t0) / 10000.0;
  EXPECT_GE(ms, 95.0);
  EXPECT_LT(ms, 400.0);
}

TEST(GetCurrentTimestamp, NeverGoesBackwardsOnOneThread)
{
  LONGLONG last = GetCurrentTimestamp();
  for (int i = 0; i < 100000; i++)
  {
    const LONGLONG now = GetCurrentTimestamp();
    ASSERT_GE(now, last);
    last = now;
  }
}

TEST(GetCurrentTimestamp, SeveralThreadsInitialiseTheTimerTogether)
{
  // the first call sets the timer up under a lock; the threads of the presenter call it at start up
  std::vector<std::thread> threads;
  std::vector<LONGLONG> results(8, 0);
  for (int i = 0; i < 8; i++)
    threads.emplace_back([&results, i]() { results[i] = GetCurrentTimestamp(); });
  for (auto& t : threads)
    t.join();
  for (LONGLONG r : results)
    EXPECT_GT(r, 0);
}

// --- SystemClockMillis / EndTime ----------------------------------------------------------------------------------------------

TEST(SystemClock, MillisecondsRunForward)
{
  const unsigned int a = XbmcThreads::SystemClockMillis();
  Sleep(60);
  const unsigned int b = XbmcThreads::SystemClockMillis();
  EXPECT_GE(b - a, 50u);
  EXPECT_LT(b - a, 400u);
}

TEST(EndTime, DefaultIsAlreadyOver)
{
  XbmcThreads::EndTime t;
  EXPECT_TRUE(t.IsTimePast());
  EXPECT_EQ(t.MillisLeft(), 0u);
  EXPECT_FALSE(t.IsInfinite());
}

TEST(EndTime, ZeroMillisecondsIsOverAtOnce)
{
  XbmcThreads::EndTime t(0);
  EXPECT_TRUE(t.IsTimePast());
  EXPECT_EQ(t.MillisLeft(), 0u);
}

TEST(EndTime, CountsDownAndEnds)
{
  XbmcThreads::EndTime t(150);
  EXPECT_FALSE(t.IsTimePast());
  EXPECT_LE(t.MillisLeft(), 150u);
  EXPECT_GT(t.MillisLeft(), 0u);
  EXPECT_EQ(t.GetInitialTimeoutValue(), 150u);
  Sleep(200);
  EXPECT_TRUE(t.IsTimePast());
  EXPECT_EQ(t.MillisLeft(), 0u);
}

TEST(EndTime, SetRestartsTheTimeout)
{
  XbmcThreads::EndTime t(1);
  Sleep(30);
  EXPECT_TRUE(t.IsTimePast());
  t.Set(10000);
  EXPECT_FALSE(t.IsTimePast());
  t.SetExpired();
  EXPECT_TRUE(t.IsTimePast());
}

TEST(EndTime, InfiniteNeverEnds)
{
  XbmcThreads::EndTime t(100);
  t.SetInfinite();
  EXPECT_TRUE(t.IsInfinite());
  EXPECT_FALSE(t.IsTimePast());
  EXPECT_EQ(t.MillisLeft(), XbmcThreads::EndTime::InfiniteValue);
}

TEST(EndTime, TheInfiniteValueIsTheLargestUnsignedNumber)
{
  EXPECT_EQ(XbmcThreads::EndTime::InfiniteValue, 0xFFFFFFFFu);
  EXPECT_TRUE(XbmcThreads::EndTime(XbmcThreads::EndTime::InfiniteValue).IsInfinite());
  EXPECT_FALSE(XbmcThreads::EndTime(373728256u).IsInfinite()) << "a timeout of four days is a timeout";
}

// --- CMyQueue ---------------------------------------------------------------------------------------------------------------

TEST(MyQueue, GivesBackInTheOrderItWasPut)
{
  CMyQueue<int> q(4);
  EXPECT_TRUE(q.IsEmpty());
  EXPECT_TRUE(q.Put(11));
  EXPECT_TRUE(q.Put(22));
  EXPECT_TRUE(q.Put(33));
  EXPECT_EQ(q.Count(), 3);
  EXPECT_FALSE(q.IsEmpty());
  EXPECT_EQ(q.Get(), 11);
  EXPECT_EQ(q.Get(), 22);
  EXPECT_EQ(q.Get(), 33);
  EXPECT_TRUE(q.IsEmpty());
}

TEST(MyQueue, RefusesToTakeMoreThanItsSize)
{
  CMyQueue<int> q(3);
  EXPECT_TRUE(q.Put(1));
  EXPECT_TRUE(q.Put(2));
  EXPECT_TRUE(q.Put(3));
  EXPECT_TRUE(q.IsFull());
  EXPECT_FALSE(q.Put(4));
  EXPECT_EQ(q.Count(), 3);
  EXPECT_EQ(q.Get(), 1);                         // nothing was overwritten
  EXPECT_TRUE(q.Put(4));
  EXPECT_EQ(q.Get(), 2);
  EXPECT_EQ(q.Get(), 3);
  EXPECT_EQ(q.Get(), 4);
}

TEST(MyQueue, EmptyQueueGivesNull)
{
  CMyQueue<int> q(2);
  EXPECT_EQ(q.Get(), 0);
  EXPECT_EQ(q.Peek(), 0);
  EXPECT_EQ(q.PeekNext(), 0);
  EXPECT_EQ(q.Count(), 0);

  CMyQueue<int*> p(2);
  EXPECT_EQ(p.Get(), nullptr);
}

TEST(MyQueue, PeekDoesNotRemove)
{
  CMyQueue<int> q(4);
  q.Put(5);
  EXPECT_EQ(q.Peek(), 5);
  EXPECT_EQ(q.PeekNext(), 0) << "only one element";
  q.Put(6);
  EXPECT_EQ(q.Peek(), 5);
  EXPECT_EQ(q.PeekNext(), 6);
  EXPECT_EQ(q.Count(), 2);
}

TEST(MyQueue, WrapsAroundManyTimes)
{
  CMyQueue<int> q(5);
  int putNo = 0, getNo = 0;
  for (int round = 0; round < 1000; round++)
  {
    for (int i = 0; i < 1 + round % 5; i++)
      if (q.Put(++putNo) == false)
        --putNo;
    for (int i = 0; i < 1 + (round * 3) % 5 && !q.IsEmpty(); i++)
      ASSERT_EQ(q.Get(), ++getNo);
  }
  while (!q.IsEmpty())
    ASSERT_EQ(q.Get(), ++getNo);
  EXPECT_EQ(getNo, putNo);
}

TEST(MyQueue, ClearForgetsEverything)
{
  CMyQueue<int> q(3);
  q.Put(1);
  q.Put(2);
  q.Clear();
  EXPECT_TRUE(q.IsEmpty());
  EXPECT_EQ(q.Count(), 0);
  EXPECT_TRUE(q.Put(9));
  EXPECT_EQ(q.Get(), 9);
}

TEST(MyQueue, ResizeToASmallerSizeEmptiesTheQueue)
{
  CMyQueue<int> q(8);
  q.Put(1);
  q.Resize(2);
  EXPECT_TRUE(q.IsEmpty());
  EXPECT_TRUE(q.Put(1));
  EXPECT_TRUE(q.Put(2));
  EXPECT_FALSE(q.Put(3));
}

TEST(MyQueue, ResizeToALargerSizeAllocatesRoom)
{
  CMyQueue<int> q(2);
  q.Resize(100000);
  for (int i = 0; i < 100000; i++)
    ASSERT_TRUE(q.Put(i));
  for (int i = 0; i < 100000; i++)
    ASSERT_EQ(q.Get(), i);
}

TEST(MyQueue, HoldsPointers)
{
  int a = 1, b = 2;
  CMyQueue<int*> q(2);
  q.Put(&a);
  q.Put(&b);
  EXPECT_EQ(q.Get(), &a);
  EXPECT_EQ(q.Get(), &b);
  EXPECT_EQ(q.Get(), nullptr);
}

// --- CAutoInt ---------------------------------------------------------------------------------------------------------------

TEST(AutoInt, StartsAtZeroInAMap)
{
  // used to count how often a frame duration was seen: map[duration]++ on a key that is not there yet
  std::map<int, CAutoInt> counts;
  ++counts[400000];
  ++counts[400000];
  ++counts[416667];
  EXPECT_EQ(static_cast<int>(counts[400000]), 2);
  EXPECT_EQ(static_cast<int>(counts[416667]), 1);
  EXPECT_EQ(static_cast<int>(counts[1]), 0);
}

TEST(AutoInt, ConvertsToAndFromInt)
{
  CAutoInt a(41);
  ++a;
  int v = a;
  EXPECT_EQ(v, 42);
  EXPECT_EQ(a.m_Int, 42);
  EXPECT_EQ(static_cast<int>(CAutoInt()), 0);
}
