// Win7RefreshRateHelper.dll: W7GetRefreshRate(monitor) as dshowhelper calls it (GetProcAddress, cdecl, double result).
// The values are compared with what the display driver reports through EnumDisplaySettings, so they need a display.
#include <gtest/gtest.h>
#include <cmath>
#include <thread>
#include <vector>
#include "FilterHost.h"

namespace
{
  typedef double(__cdecl* GetRefreshRateFn)(int monitorIdx);

  class RefreshRateHelper : public testing::Test
  {
  protected:
    FilterModule module;
    GetRefreshRateFn getRefreshRate = nullptr;

    void SetUp() override
    {
      ASSERT_TRUE(module.Load(L"Win7RefreshRateHelper.dll")) << std::string(module.Error().begin(), module.Error().end());
      HMODULE h = GetModuleHandleW(L"Win7RefreshRateHelper.dll");
      ASSERT_NE(h, nullptr);
      getRefreshRate = reinterpret_cast<GetRefreshRateFn>(GetProcAddress(h, "W7GetRefreshRate"));
      ASSERT_NE(getRefreshRate, nullptr);
    }

    // What the driver says about display device 'index' (0 when there is none)
    static double DriverRefreshRate(int index)
    {
      DISPLAY_DEVICEW device = {};
      device.cb = sizeof(device);
      if (!EnumDisplayDevicesW(NULL, index, &device, 0))
        return 0;
      DEVMODEW mode = {};
      mode.dmSize = sizeof(mode);
      if (!EnumDisplaySettingsW(device.DeviceName, ENUM_CURRENT_SETTINGS, &mode))
        return 0;
      return mode.dmDisplayFrequency;
    }
  };
}

TEST_F(RefreshRateHelper, ExportsTheFunctionByItsPlainName)
{
  EXPECT_TRUE(module.HasExport("W7GetRefreshRate"));
}

TEST_F(RefreshRateHelper, PrimaryMonitorRateIsWhatTheDriverReports)
{
  const double driver = DriverRefreshRate(0);
  if (driver < 1)
    GTEST_SKIP() << "this session has no display device 0";
  const double rate = getRefreshRate(0);
  if (rate < 0)
    GTEST_SKIP() << "W7GetRefreshRate found no active display path for monitor 0 (remote session?)";
  // 59.94 Hz is reported as 59 or 60 by EnumDisplaySettings
  GTEST_LOG_(INFO) << "W7GetRefreshRate(0) = " << rate << " Hz, driver: " << driver << " Hz";
  EXPECT_NEAR(rate, driver, 1.01) << "driver reports " << driver << " Hz";
  EXPECT_GE(rate, 23.0);
  EXPECT_LE(rate, 600.0);
}

TEST_F(RefreshRateHelper, EveryAttachedMonitorHasAPlausibleRate)
{
  int checked = 0;
  for (int i = 0; i < 8; i++)
  {
    const double driver = DriverRefreshRate(i);
    if (driver < 1)
      continue;
    const double rate = getRefreshRate(i);
    if (rate < 0)
      continue;                                  // a display that is not part of the desktop
    checked++;
    EXPECT_NEAR(rate, driver, 1.01) << "monitor " << i;
  }
  if (checked == 0)
    GTEST_SKIP() << "no monitor with an active display path";
}

TEST_F(RefreshRateHelper, ANonExistentMonitorGivesMinusOne)
{
  EXPECT_EQ(getRefreshRate(99), -1.0);
  EXPECT_EQ(getRefreshRate(1000000), -1.0);
}

TEST_F(RefreshRateHelper, ANegativeMonitorIndexGivesMinusOne)
{
  EXPECT_EQ(getRefreshRate(-1), -1.0);
}

TEST_F(RefreshRateHelper, RepeatedCallsGiveTheSameAnswer)
{
  const double first = getRefreshRate(0);
  for (int i = 0; i < 50; i++)
    ASSERT_EQ(getRefreshRate(0), first) << "call " << i;
}

TEST_F(RefreshRateHelper, CallsFromSeveralThreadsAreIndependent)
{
  const double expected = getRefreshRate(0);
  std::vector<std::thread> threads;
  std::vector<int> wrong(8, 0);
  for (int t = 0; t < 8; t++)
  {
    threads.emplace_back([&, t] {
      for (int i = 0; i < 100; i++)
        if (getRefreshRate(0) != expected)
          wrong[t]++;
    });
  }
  for (auto& th : threads)
    th.join();
  for (int t = 0; t < 8; t++)
    EXPECT_EQ(wrong[t], 0) << "thread " << t;
}

TEST_F(RefreshRateHelper, UnknownMonitorIsNeverAnsweredWithTheRateOfAnother)
{
  const double first = getRefreshRate(0);
  ASSERT_GT(first, 0.0);
  EXPECT_EQ(getRefreshRate(99), -1.0);
  EXPECT_EQ(getRefreshRate(-1), -1.0);
}
