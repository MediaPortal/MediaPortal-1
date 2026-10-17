// AudioStreamResampler of MPAudioSwitcher (Audio.cpp, from VirtualDub): the sample rate reduction in front of the
// S/PDIF-less audio path ("EnableDownSamplingTo441"). Audio.cpp is compiled into the tests as it is.
#include <gtest/gtest.h>
#include <windows.h>
#include <cmath>
#include <cstdint>
#include <vector>
#include "../../MPAudioswitcher/source/Audio.h"
#include "GuardedBuffer.h"

namespace
{
  const double kPi = 3.14159265358979323846;

  std::vector<int16_t> Sine16(double frequency, int rate, int samples, double amplitude = 10000.0)
  {
    std::vector<int16_t> v(samples);
    for (int i = 0; i < samples; i++)
      v[i] = static_cast<int16_t>(std::lround(amplitude * std::sin(2.0 * kPi * frequency * i / rate)));
    return v;
  }

  double Rms(const std::vector<int16_t>& v, size_t from, size_t to)
  {
    double sum = 0;
    for (size_t i = from; i < to; i++)
      sum += static_cast<double>(v[i]) * v[i];
    return std::sqrt(sum / (to - from));
  }

  // Amplitude of the component at 'frequency' (Goertzel) in the middle part of the signal
  double AmplitudeAt(const std::vector<int16_t>& v, int rate, double frequency)
  {
    const size_t from = v.size() / 4, to = v.size() * 3 / 4;
    const double w = 2.0 * kPi * frequency / rate;
    const double coeff = 2.0 * std::cos(w);
    double s1 = 0, s2 = 0;
    for (size_t i = from; i < to; i++)
    {
      const double s = v[i] + coeff * s1 - s2;
      s2 = s1;
      s1 = s;
    }
    const double power = s1 * s1 + s2 * s2 - coeff * s1 * s2;
    return 2.0 * std::sqrt(power) / (to - from);
  }

  // Frequency from the zero crossings (rising) of the middle part of the signal
  double Frequency(const std::vector<int16_t>& v, int rate)
  {
    const size_t from = v.size() / 4, to = v.size() * 3 / 4;
    int crossings = 0;
    for (size_t i = from + 1; i < to; i++)
      if (v[i - 1] < 0 && v[i] >= 0)
        crossings++;
    return crossings * static_cast<double>(rate) / (to - from);
  }

  // The way AudioSwitcher calls it: one call, room for as many output samples as the ratio gives for the input. The resampler
  // keeps the samples it has not used yet for the next call, so it can deliver a few samples less.
  std::vector<int16_t> Resample(const std::vector<int16_t>& in, int fromRate, int toRate, bool highQuality)
  {
    AudioStreamResampler r(2, fromRate, toRate, highQuality);
    std::vector<int16_t> input = in;
    std::vector<int16_t> out(static_cast<size_t>(static_cast<long long>(in.size()) * toRate / fromRate));   // 64 bit: 96000 * 48000 overflows 32 bit
    const long produced = r.Downsample(input.data(), static_cast<long>(in.size()), out.data(), static_cast<long>(out.size()));
    out.resize(produced);
    return out;
  }
}

TEST(Resampler, ProducesWhatTheRatioGivesForTheInput)
{
  // all input is used; the last few samples stay in the resampler (filter width) for the next call
  struct { int from, to; } cases[] = { { 48000, 44100 }, { 96000, 48000 }, { 96000, 44100 }, { 192000, 44100 }, { 48000, 32000 } };
  for (auto c : cases)
    for (bool hq : { false, true })
    {
      const std::vector<int16_t> in = Sine16(1000, c.from, c.from);
    {
      const long produced = static_cast<long>(Resample(in, c.from, c.to, hq).size());
      EXPECT_LE(produced, c.to) << c.from << " -> " << c.to << (hq ? " hq" : "");
      EXPECT_GE(produced, c.to - 40) << c.from << " -> " << c.to << (hq ? " hq" : "");
    }
    }
}

TEST(Resampler, ToneKeepsItsFrequencyAndLevel)
{
  const std::vector<int16_t> in = Sine16(1000, 48000, 48000);
  for (bool hq : { false, true })
  {
    const std::vector<int16_t> out = Resample(in, 48000, 44100, hq);
    SCOPED_TRACE(hq ? "high quality" : "point sampling");
    EXPECT_NEAR(Frequency(out, 44100), 1000.0, 15.0);
    const double inRms = Rms(in, 12000, 36000);
    const double outRms = Rms(out, out.size() / 4, out.size() * 3 / 4);
    EXPECT_NEAR(outRms / inRms, 1.0, 0.08);
  }
}

TEST(Resampler, HighQualityDoesNotClip)
{
  // full scale tone: the filter may overshoot but the result must stay inside 16 bit (saturated, not wrapped)
  const std::vector<int16_t> in = Sine16(3000, 96000, 96000, 32000.0);
  const std::vector<int16_t> out = Resample(in, 96000, 48000, true);
  int16_t peak = 0;
  for (size_t i = out.size() / 8; i < out.size() * 7 / 8; i++)
    peak = (std::max)(peak, static_cast<int16_t>(std::abs(out[i])));
  EXPECT_GT(peak, 28000);
  EXPECT_LE(peak, 32767);
}

TEST(Resampler, HighQualityAttenuatesWhatPointSamplingFoldsBack)
{
  // 30 kHz at 96 kHz is above half of 44.1 kHz: point sampling turns it into a 14.1 kHz alias, the filter damps it
  const std::vector<int16_t> in = Sine16(30000, 96000, 96000);
  const std::vector<int16_t> point = Resample(in, 96000, 44100, false);
  const std::vector<int16_t> hq = Resample(in, 96000, 44100, true);
  const double alias = Rms(point, point.size() / 4, point.size() * 3 / 4);
  const double filtered = Rms(hq, hq.size() / 4, hq.size() * 3 / 4);
  EXPECT_GT(alias, 5000.0) << "the alias is not there";
  EXPECT_LT(filtered, alias * 0.6) << "filtered " << filtered << " vs alias " << alias;
}

TEST(Resampler, HighQualityKeepsTheLowFrequenciesOfAMixture)
{
  // 200 Hz plus 30 kHz at 96 kHz: the low tone passes at its level, the 30 kHz tone (folded to 14.1 kHz) is damped
  std::vector<int16_t> in(96000);
  for (size_t i = 0; i < in.size(); i++)
    in[i] = static_cast<int16_t>(std::lround(8000 * std::sin(2 * kPi * 200 * i / 96000.0) + 8000 * std::sin(2 * kPi * 30000 * i / 96000.0)));
  const std::vector<int16_t> hq = Resample(in, 96000, 44100, true);
  const std::vector<int16_t> point = Resample(in, 96000, 44100, false);
  EXPECT_NEAR(AmplitudeAt(hq, 44100, 200.0), 8000.0, 600.0);
  EXPECT_NEAR(AmplitudeAt(point, 44100, 200.0), 8000.0, 600.0);
  EXPECT_LT(AmplitudeAt(hq, 44100, 14100.0), AmplitudeAt(point, 44100, 14100.0) * 0.6);
}

TEST(Resampler, SilenceAndConstantLevelStayAsTheyAre16Bit)
{
  for (int16_t level : { int16_t(0), int16_t(1234), int16_t(-20000) })
  {
    const std::vector<int16_t> in(96000, level);
    const std::vector<int16_t> out = Resample(in, 96000, 44100, true);
    ASSERT_GT(out.size(), 1000u);
    for (size_t i = out.size() / 8; i < out.size() * 7 / 8; i++)
      ASSERT_NEAR(out[i], level, (std::max)(2.0, std::abs(level) * 0.003)) << "level " << level << " at " << i;
  }
}

TEST(Resampler, EightBitAudioIsUnsignedAroundTheMiddle)
{
  AudioStreamResampler r(1, 96000, 44100, true);
  std::vector<uint8_t> in(96000, 0x80);
  std::vector<uint8_t> out(44100);
  const long produced = r.Downsample(in.data(), 96000, out.data(), static_cast<long>(out.size()));
  ASSERT_GE(produced, 44100 - 40);
  ASSERT_LE(produced, 44100);
  for (long i = produced / 8; i < produced * 7 / 8; i++)
    ASSERT_NEAR(out[i], 0x80, 1) << i;
}

TEST(Resampler, NothingIsWrittenBeyondTheRequestedOutput)
{
  const std::vector<int16_t> sine = Sine16(500, 96000, 5000);
  for (bool hq : { false, true })
  {
    AudioStreamResampler r(2, 96000, 44100, hq);
    GuardedBuffer out(1000 * 2);                                   // room for exactly 1000 samples, right in front of a guard page
    const long produced = r.Downsample(const_cast<int16_t*>(sine.data()), 5000, out.data(), 1000);
    EXPECT_EQ(produced, 1000) << (hq ? "hq" : "point");
  }
}

TEST(Resampler, EmptyInputAndEmptyOutputGiveNothing)
{
  AudioStreamResampler r(2, 96000, 44100, true);
  int16_t in[4] = {}, out[4] = {};
  EXPECT_EQ(r.Downsample(in, 0, out, 4), 0);
  EXPECT_EQ(r.Downsample(in, 4, out, 0), 0);
}

TEST(Resampler, EqualRatesCopyTheSamples)
{
  const std::vector<int16_t> in = Sine16(777, 48000, 4800);
  const std::vector<int16_t> out = Resample(in, 48000, 48000, true);
  ASSERT_EQ(out.size(), in.size());
  for (size_t i = 0; i < out.size(); i++)
    ASSERT_EQ(out[i], in[i]) << i;
}

// --- the down sampling path across buffers -------------------------------------------------------------------------------
// Downsample() used to leave 'samplesin' alone while it consumed the input (VirtualDub's original decreases it). It then
// read about 'filter_width' samples behind the input and started the next call with samples it had already used:
// AudioSwitcher::Transform calls it once per channel and buffer, so the filter read past a heap block of exactly len samples
// and the sound had a jump at every buffer boundary ("EnableDownSamplingTo441").

TEST(Resampler, DoesNotReadPastTheEndOfTheInput)
{
  for (bool hq : { false, true })
  {
    AudioStreamResampler r(2, 96000, 44100, hq);
    const std::vector<int16_t> sine = Sine16(500, 96000, 2048);
    GuardedBuffer in(sine.size() * 2);                             // the input ends right in front of a guard page
    memcpy(in.data(), sine.data(), in.size());
    std::vector<int16_t> out(2048 * 44100 / 96000);
    r.Downsample(in.data(), static_cast<long>(sine.size()), out.data(), static_cast<long>(out.size()));
  }
}

TEST(Resampler, ConsecutiveBuffersJoinWithoutAJump)
{
  // Point sampling takes the nearest input sample, so even inside a buffer the output steps by one or two input samples
  // (up to 2600 for this tone). What must not happen is a step at the join of two buffers that is bigger than those.
  for (bool hq : { false, true })
    for (int from : { 48000, 96000 })
    {
      AudioStreamResampler r(2, from, 44100, hq);
      const int len = 1024, lenout = len * 44100 / from;
      long position = 0;
      double maxInside = 0, maxJoin = 0;
      short last = 0;
      bool first = true;
      for (int b = 0; b < 100; b++)
      {
        std::vector<short> in(len, 0), out(lenout + 2);       // AudioSwitcher gives the output room for lenout + 2
        for (int i = 0; i < len; i++)
          in[i] = static_cast<short>(10000 * std::sin(2 * kPi * 1000 * (position + i) / from));
        position += len;
        const long n = r.Downsample(in.data(), len, out.data(), lenout + 2);
        ASSERT_GT(n, 0);
        for (long i = 0; i < n; i++)
        {
          if (!first)
          {
            const double step = static_cast<double>(std::abs(out[i] - last));
            (i == 0 ? maxJoin : maxInside) = (std::max)(i == 0 ? maxJoin : maxInside, step);
          }
          last = out[i];
          first = false;
        }
      }
      EXPECT_LE(maxJoin, maxInside * 1.05) << from << " Hz " << (hq ? "hq" : "point");
    }
}

TEST(Resampler, NoInputIsLostOverManyBuffers)
{
  // 1000 buffers of 1024 samples: the output count follows the ratio to within the filter delay, whatever the rates
  for (bool hq : { false, true })
    for (int from : { 48000, 96000, 192000 })
    {
      AudioStreamResampler r(2, from, 44100, hq);
      const int len = 1024, lenout = len * 44100 / from;
      std::vector<short> in(len, 100), out(lenout + 2);
      long long total = 0;
      for (int b = 0; b < 1000; b++)
        total += r.Downsample(in.data(), len, out.data(), lenout + 2);
      const double expected = 1000.0 * len * 44100 / from;
      EXPECT_NEAR(static_cast<double>(total), expected, 40.0) << from << " Hz " << (hq ? "hq" : "point");
    }
}
