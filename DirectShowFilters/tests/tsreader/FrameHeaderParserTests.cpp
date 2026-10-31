// CFrameHeaderParser and CMpegPesParser of TsReader: the headers of audio and video frames (MPEG audio, AAC, AC-3, E-AC-3, DTS,
// MPEG-2 video, H.264) as they come out of the transport stream. The data comes from the air or from files of any quality, so
// every parser also gets random and damaged data in front of a guard page.
#include <gtest/gtest.h>
#include <winsock2.h>
#include <streams.h>
#include <dvdmedia.h>
#include <mmreg.h>
#include <uuids.h>
#include <wmcodecdsp.h>
#include <memory>
#include <vector>
#include "FrameHeaderParser.h"
#include "MpegPesParser.h"
#include "mediaformats.h"
#include "../unit/BitWriter.h"
#include "../unit/GuardedBuffer.h"
#include "../unit/TestPackets.h"
#include "../unit/CrtAsserts.h"
#include "../unit/AbortGuard.h"

namespace
{
  typedef std::vector<BYTE> Data;

  // A parser that works on 'data' (value initialised: CFrameHeaderParser has no constructor that sets everything)
  struct Parser
  {
    explicit Parser(Data d) : data(std::move(d)), p(new CFrameHeaderParser())
    {
      p->Reset(data.data(), static_cast<int>(data.size()));
    }
    Data data;
    std::unique_ptr<CFrameHeaderParser> p;
  };

  Data MpaHeader(int version, int layer, int noCrc, int bitrate, int freq, int padding, int channels, int emphasis = 0)
  {
    BitWriter w;
    w.Put(0x7FF, 11).Put(version, 2).Put(layer, 2).Put(noCrc, 1).Put(bitrate, 4).Put(freq, 2).Put(padding, 1).Put(0, 1)
     .Put(channels, 2).Put(0, 2).Put(0, 1).Put(0, 1).Put(emphasis, 2);
    return w.Bytes();
  }

  Data AdtsHeader(int profile, int freq, int channels, int frameLength, int protectionAbsent = 1, int layer = 0)
  {
    BitWriter w;
    w.Put(0xFFF, 12).Put(0, 1).Put(layer, 2).Put(protectionAbsent, 1).Put(profile, 2).Put(freq, 4).Put(0, 1).Put(channels, 3)
     .Put(0, 1).Put(0, 1).Put(0, 1).Put(0, 1).Put(frameLength, 13).Put(0x7FF, 11).Put(0, 2);
    if (!protectionAbsent)
      w.Put(0x1234, 16);
    return w.Bytes();
  }

  Data Ac3Header(int fscod, int frmsizecod, int bsid, int acmod, int lfe)
  {
    BitWriter w;
    w.Put(0x0B77, 16).Put(0xABCD, 16).Put(fscod, 2).Put(frmsizecod, 6).Put(bsid, 5).Put(0, 3).Put(acmod, 3);
    if ((acmod & 1) && acmod != 1) w.Put(1, 2);
    if (acmod & 4) w.Put(1, 2);
    if (acmod == 2) w.Put(0, 2);
    w.Put(lfe, 1);
    w.Align();
    Data d = w.Bytes();
    d.resize(16, 0);
    return d;
  }

  Data Eac3Header(int frmsizMinus1, int fscod, int fscod2, int acmod, int lfe, int bsid)
  {
    BitWriter w;
    w.Put(0x0B77, 16).Put(0, 2).Put(0, 3).Put(frmsizMinus1, 11).Put(fscod, 2).Put(fscod2, 2).Put(acmod, 3).Put(lfe, 1).Put(bsid, 5).Put(0, 3);
    Data d = w.Bytes();
    d.resize(16, 0);
    return d;
  }

  Data DtsHeader(int amode, int sfreq, int rate, int lfe)
  {
    BitWriter w;
    w.Put(0x7FFE8001, 32).Put(1, 1).Put(31, 5).Put(0, 1).Put(15, 7).Put(2012, 14).Put(amode, 6).Put(sfreq, 4).Put(rate, 5)
     .Put(0, 1).Put(0, 1).Put(0, 1).Put(0, 1).Put(0, 1).Put(0, 3).Put(0, 1).Put(0, 1).Put(lfe, 2).Put(0, 1);
    Data d = w.Bytes();
    d.resize(16, 0);
    return d;
  }

  Data Concat(Data a, const Data& b)
  {
    a.insert(a.end(), b.begin(), b.end());
    return a;
  }

  std::string Lang(const WAVEFORMATEX* w) { return std::to_string(w->wFormatTag); }
}

// ---------------------------------------------------------------------------------------------------------------------------
// MPEG audio
// ---------------------------------------------------------------------------------------------------------------------------

TEST(FrameHeaderParser, MpegLayer2At48kHz)
{
  Parser s(MpaHeader(3, 2, 1, 10, 1, 0, 0));                 // MPEG-1, layer II, 192 kbit/s, 48 kHz, stereo
  mpahdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), false));
  EXPECT_EQ(h.layer, 2);
  EXPECT_EQ(h.nSamplesPerSec, 48000);
  EXPECT_EQ(h.nBytesPerSec, 24000);
  EXPECT_EQ(h.FrameSize, 576);
  EXPECT_EQ(h.rtDuration, 10000000LL * 1152 / 48000);
}

TEST(FrameHeaderParser, MpegLayer3At44kHzWithPadding)
{
  Parser s(MpaHeader(3, 1, 1, 9, 0, 1, 1));                  // MPEG-1, layer III, 128 kbit/s, 44.1 kHz, padding
  mpahdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), false));
  EXPECT_EQ(h.layer, 3);
  EXPECT_EQ(h.nSamplesPerSec, 44100);
  EXPECT_EQ(h.FrameSize, 144 * 128000 / 44100 + 1);
  EXPECT_EQ(h.nBytesPerSec, 16000);
}

TEST(FrameHeaderParser, MpegLayer1)
{
  Parser s(MpaHeader(3, 3, 1, 4, 0, 0, 0));                  // layer I, 128 kbit/s, 44.1 kHz
  mpahdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), false));
  EXPECT_EQ(h.layer, 1);
  EXPECT_EQ(h.FrameSize, (12 * 128000 / 44100) * 4);
  EXPECT_EQ(h.rtDuration, 10000000LL * 384 / 44100);
}

TEST(FrameHeaderParser, MpegLowSamplingRateLayer3)
{
  Parser s(MpaHeader(2, 1, 1, 8, 0, 0, 0));                  // MPEG-2, layer III, 64 kbit/s, 22.05 kHz
  mpahdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), false));
  EXPECT_EQ(h.nSamplesPerSec, 22050);
  EXPECT_EQ(h.FrameSize, 72 * 64000 / 22050);
  EXPECT_EQ(h.rtDuration, 10000000LL * 576 / 22050);
}

TEST(FrameHeaderParser, MpegHeaderIsFoundBehindOtherBytes)
{
  Data d(37, 0x11);
  d = Concat(d, MpaHeader(3, 2, 1, 10, 1, 0, 0));
  Parser s(d);
  mpahdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(d.size()), false));
  EXPECT_EQ(h.nSamplesPerSec, 48000);
}

TEST(FrameHeaderParser, MpegWithoutAHeaderOrTooShortIsRefused)
{
  mpahdr h;
  Parser none(Data(100, 0x11));
  EXPECT_FALSE(none.p->Read(h, 100, false));
  Parser shortData(Data{ 0xFF, 0xFD, 0xA4 });
  EXPECT_FALSE(shortData.p->Read(h, 3, false));
  Parser empty(Data(1, 0));
  EXPECT_FALSE(empty.p->Read(h, 0, false));
}

TEST(FrameHeaderParser, MpegReservedValuesAreRefused)
{
  mpahdr h;
  struct { int version, layer, bitrate, freq, emphasis; } bad[] = {
    { 1, 2, 10, 1, 0 },       // reserved version
    { 3, 0, 10, 1, 0 },       // reserved layer
    { 3, 2, 15, 1, 0 },       // bad bitrate
    { 3, 2, 0, 1, 0 },        // free format: no bitrate known
    { 3, 2, 10, 3, 0 },       // reserved sampling rate
    { 3, 2, 10, 1, 2 } };     // reserved emphasis
  for (auto b : bad)
  {
    Parser s(MpaHeader(b.version, b.layer, 1, b.bitrate, b.freq, 0, 0, b.emphasis));
    EXPECT_FALSE(s.p->Read(h, static_cast<int>(s.data.size()), false)) << b.version << " " << b.layer << " " << b.bitrate;
  }
}

TEST(FrameHeaderParser, MpegVersion25NeedsThePermission)
{
  Data d = MpaHeader(0, 1, 1, 8, 0, 0, 0);                    // MPEG-2.5 layer III
  d = Concat(d, Data(16, 0));
  mpahdr h;
  Parser strict(d);
  EXPECT_FALSE(strict.p->Read(h, static_cast<int>(d.size()), false));
  Parser lax(d);
  EXPECT_TRUE(lax.p->Read(h, static_cast<int>(d.size()), true));
  EXPECT_EQ(h.nSamplesPerSec, 11025);
}

TEST(FrameHeaderParser, MpegMediaType)
{
  Parser s(MpaHeader(3, 2, 1, 10, 1, 0, 3));                  // single channel
  mpahdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), false, &mt));
  EXPECT_EQ(mt.majortype, MEDIATYPE_Audio);
  ASSERT_EQ(mt.formattype, FORMAT_WaveFormatEx);
  const WAVEFORMATEX* w = reinterpret_cast<const WAVEFORMATEX*>(mt.Format());
  EXPECT_EQ(w->wFormatTag, WAVE_FORMAT_MPEG);
  EXPECT_EQ(w->nChannels, 1);
  EXPECT_EQ(w->nSamplesPerSec, 48000u);
  EXPECT_EQ(w->nAvgBytesPerSec, 24000u);
  EXPECT_EQ(mt.cbFormat, sizeof(MPEG1WAVEFORMAT));
}

// ---------------------------------------------------------------------------------------------------------------------------
// AAC (ADTS)
// ---------------------------------------------------------------------------------------------------------------------------

TEST(FrameHeaderParser, AdtsHeader)
{
  Parser s(AdtsHeader(1, 4, 2, 400));                         // LC, 44.1 kHz, stereo, 400 byte frame
  aachdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), nullptr));
  EXPECT_EQ(h.profile, 1);
  EXPECT_EQ(h.nSamplesPerSec, 44100);
  EXPECT_EQ(h.channels, 2);
  EXPECT_EQ(h.aac_frame_length, 400);
  EXPECT_EQ(h.FrameSize, 400 - 7);
  EXPECT_EQ(h.nBytesPerSec, 400 * 44100 / 1024);
  EXPECT_EQ(h.rtDuration, 10000000LL * 1024 / 44100);
}

TEST(FrameHeaderParser, AdtsHeaderWithCrc)
{
  Parser s(AdtsHeader(1, 3, 6, 600, 0));
  aachdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), nullptr));
  EXPECT_EQ(h.nSamplesPerSec, 48000);
  EXPECT_EQ(h.crc, 0x1234);
  EXPECT_EQ(h.FrameSize, 600 - 9);
}

TEST(FrameHeaderParser, AdtsMediaTypeCarriesTheAudioSpecificConfig)
{
  Parser s(AdtsHeader(1, 4, 2, 400));
  aachdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), &mt));
  EXPECT_EQ(mt.subtype, MEDIASUBTYPE_AAC);
  ASSERT_GE(mt.cbFormat, sizeof(WAVEFORMATEX) + 2);
  const WAVEFORMATEX* w = reinterpret_cast<const WAVEFORMATEX*>(mt.Format());
  EXPECT_EQ(w->wFormatTag, WAVE_FORMAT_AAC);
  EXPECT_EQ(w->nChannels, 2);
  EXPECT_EQ(w->nSamplesPerSec, 44100u);
  ASSERT_EQ(w->cbSize, 2);
  const BYTE* config = mt.Format() + sizeof(WAVEFORMATEX);
  EXPECT_EQ(config[0], 0x12);                                  // AAC-LC, 44.1 kHz ...
  EXPECT_EQ(config[1], 0x10);                                  // ... stereo
}

TEST(FrameHeaderParser, AdtsWithMoreThanSixChannelsIsTakenAsStereo)
{
  Parser s(AdtsHeader(1, 4, 7, 400));
  aachdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), &mt));
  EXPECT_EQ(h.channels, 2);
}

TEST(FrameHeaderParser, AdtsBadHeadersAreRefused)
{
  aachdr h;
  Parser badFreq(AdtsHeader(1, 12, 2, 400));
  EXPECT_FALSE(badFreq.p->Read(h, static_cast<int>(badFreq.data.size()), nullptr));
  Parser badLayer(AdtsHeader(1, 4, 2, 400, 1, 1));
  EXPECT_FALSE(badLayer.p->Read(h, static_cast<int>(badLayer.data.size()), nullptr));
  Parser tiny(AdtsHeader(1, 4, 2, 7));
  EXPECT_FALSE(tiny.p->Read(h, static_cast<int>(tiny.data.size()), nullptr)) << "a frame no longer than its header";
  Parser tinyCrc(AdtsHeader(1, 4, 2, 9, 0));
  EXPECT_FALSE(tinyCrc.p->Read(h, static_cast<int>(tinyCrc.data.size()), nullptr));
}

// ---------------------------------------------------------------------------------------------------------------------------
// AC-3, E-AC-3, DTS
// ---------------------------------------------------------------------------------------------------------------------------

TEST(FrameHeaderParser, Ac3FiveOne)
{
  Parser s(Ac3Header(0, 28, 8, 7, 1));                        // 48 kHz, 384 kbit/s, 3/2 + LFE
  ac3hdr h;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size())));
  EXPECT_EQ(h.nSamplesPerSec, 48000);
  EXPECT_EQ(h.nChannels, 6);
  EXPECT_EQ(h.nBytesPerSec, 384 * 1000 / 8);
  EXPECT_EQ(h.bsid, 8);
}

TEST(FrameHeaderParser, Ac3ChannelModes)
{
  const int expected[8] = { 2, 1, 2, 3, 3, 4, 4, 5 };
  for (int acmod = 0; acmod < 8; acmod++)
  {
    Parser s(Ac3Header(0, 20, 8, acmod, 0));
    ac3hdr h;
    ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()))) << acmod;
    EXPECT_EQ(h.nChannels, expected[acmod]) << acmod;
  }
}

TEST(FrameHeaderParser, Ac3SamplingRatesAndHalfRateStreams)
{
  ac3hdr h;
  Parser a(Ac3Header(1, 20, 8, 2, 0));
  ASSERT_TRUE(a.p->Read(h, 16));
  EXPECT_EQ(h.nSamplesPerSec, 44100);
  Parser b(Ac3Header(2, 20, 8, 2, 0));
  ASSERT_TRUE(b.p->Read(h, 16));
  EXPECT_EQ(h.nSamplesPerSec, 32000);
  Parser c(Ac3Header(0, 20, 9, 2, 0));                        // bsid 9: half the rate
  ASSERT_TRUE(c.p->Read(h, 16));
  EXPECT_EQ(h.nSamplesPerSec, 24000);
  Parser d(Ac3Header(0, 20, 11, 2, 0));
  ASSERT_TRUE(d.p->Read(h, 16));
  EXPECT_EQ(h.nSamplesPerSec, 6000);
}

TEST(FrameHeaderParser, Ac3BadHeadersAreRefused)
{
  ac3hdr h;
  Parser fscod(Ac3Header(3, 20, 8, 2, 0));
  EXPECT_FALSE(fscod.p->Read(h, 16));
  Parser size(Ac3Header(0, 48, 8, 2, 0));
  EXPECT_FALSE(size.p->Read(h, 16));
  Parser bsid(Ac3Header(0, 20, 17, 2, 0));
  EXPECT_FALSE(bsid.p->Read(h, 16));
  Parser none(Data(50, 0x55));
  EXPECT_FALSE(none.p->Read(h, 50));
}

TEST(FrameHeaderParser, Ac3MediaType)
{
  Parser s(Ac3Header(0, 28, 8, 7, 1));
  ac3hdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, 16, &mt));
  EXPECT_EQ(mt.subtype, MEDIASUBTYPE_DOLBY_AC3);
  const WAVEFORMATEX* w = reinterpret_cast<const WAVEFORMATEX*>(mt.Format());
  EXPECT_EQ(w->nChannels, 6);
  EXPECT_EQ(w->nBlockAlign, 1536 * 48000 / 48000);
}

TEST(FrameHeaderParser, EnhancedAc3)
{
  Parser s(Eac3Header(767, 0, 0, 7, 1, 16));                   // 768 words = 1536 bytes, 48 kHz, 5.1
  eac3hdr h;
  ASSERT_TRUE(s.p->Read(h, 16));
  EXPECT_EQ(h.frmsiz, 768);
  EXPECT_EQ(h.nSamplesPerSec, 48000);
  EXPECT_EQ(h.nChannels, 6);
  EXPECT_EQ(h.nBytesPerSec, 1000 * (768 * 48000 / (16 * 48000)));
  CMediaType mt;
  Parser t(Eac3Header(767, 0, 0, 7, 1, 16));
  ASSERT_TRUE(t.p->Read(h, 16, &mt));
  EXPECT_EQ(mt.subtype, MEDIASUBTYPE_DOLBY_DDPLUS);
}

TEST(FrameHeaderParser, EnhancedAc3AtTheHalfRates)
{
  Parser s(Eac3Header(383, 3, 0, 2, 0, 16));                   // fscod 3: the rate comes from fscod2 (halved)
  eac3hdr h;
  ASSERT_TRUE(s.p->Read(h, 16));
  EXPECT_EQ(h.nSamplesPerSec, 24000);
}

TEST(FrameHeaderParser, EnhancedAc3WithBsidAbove16IsRefused)
{
  Parser s(Eac3Header(383, 0, 0, 2, 0, 17));
  eac3hdr h;
  EXPECT_FALSE(s.p->Read(h, 16));
}

TEST(FrameHeaderParser, EnhancedAc3WithAReservedSamplingRateIsRefused)
{
  Parser s(Eac3Header(383, 3, 3, 2, 0, 16));
  eac3hdr h;
  CMediaType mt;
  EXPECT_FALSE(s.p->Read(h, 16, &mt));
}

TEST(FrameHeaderParser, DtsCoreHeader)
{
  Parser s(DtsHeader(9, 13, 17, 1));                           // 3/2, 48 kHz, 1024 kbit/s, LFE
  dtshdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, 16, &mt));
  EXPECT_EQ(h.framebytes, 2013);
  EXPECT_EQ(h.amode, 9);
  const WAVEFORMATEX* w = reinterpret_cast<const WAVEFORMATEX*>(mt.Format());
  EXPECT_EQ(w->nChannels, 6);
  EXPECT_EQ(w->nSamplesPerSec, 48000u);
  EXPECT_EQ(w->nAvgBytesPerSec, (1024000u + 4) / 8);
}

TEST(FrameHeaderParser, DtsWithUnknownModesStillGivesAMediaType)
{
  Parser s(DtsHeader(40, 0, 31, 0));
  dtshdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, 16, &mt));
  const WAVEFORMATEX* w = reinterpret_cast<const WAVEFORMATEX*>(mt.Format());
  EXPECT_EQ(w->nChannels, 0);
  EXPECT_EQ(w->nSamplesPerSec, 0u);
}

// ---------------------------------------------------------------------------------------------------------------------------
// MPEG-2 video, PES header, start codes
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  Data Mpeg2SequenceHeader(int width, int height, int ar, int fps, bool extension)
  {
    BitWriter w;
    w.Put(0x000001B3, 32).Put(width & 0xFFF, 12).Put(height & 0xFFF, 12).Put(ar, 4).Put(fps, 4).Put(0x3FFFF, 18).Put(1, 1)
     .Put(112, 10).Put(0, 1).Put(0, 1).Put(0, 1);
    if (extension)
      w.Put(0x000001B5, 32).Put(1, 4).Put(0, 1).Put(4, 3).Put(8, 4).Put(1, 1).Put(1, 2).Put(width >> 12, 2).Put(height >> 12, 2)
       .Put(0, 12).Put(1, 1).Put(0, 8).Put(0, 1).Put(0, 2).Put(0, 5);
    w.Align();
    Data d = w.Bytes();
    d.resize(d.size() + 8, 0);
    return d;
  }
}

TEST(FrameHeaderParser, Mpeg2SequenceHeaderWithExtension)
{
  Parser s(Mpeg2SequenceHeader(720, 576, 2, 3, true));        // 4:3, 25 frames per second
  seqhdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), &mt));
  EXPECT_EQ(h.width, 720);
  EXPECT_EQ(h.height, 576);
  EXPECT_EQ(h.ifps, 400000u);
  EXPECT_EQ(h.arx, 4);
  EXPECT_EQ(h.ary, 3);
  EXPECT_EQ(h.progressive, 1);
  EXPECT_EQ(mt.subtype, MEDIASUBTYPE_MPEG2_VIDEO);
  const MPEG2VIDEOINFO* vi = reinterpret_cast<const MPEG2VIDEOINFO*>(mt.Format());
  EXPECT_EQ(vi->hdr.bmiHeader.biWidth, 720);
  EXPECT_EQ(vi->hdr.bmiHeader.biHeight, 576);
  EXPECT_EQ(vi->hdr.AvgTimePerFrame, 400000);
  EXPECT_GT(vi->cbSequenceHeader, 8u);
}

TEST(FrameHeaderParser, Mpeg1SequenceHeaderWithoutExtension)
{
  Parser s(Mpeg2SequenceHeader(352, 288, 1, 3, false));
  seqhdr h;
  CMediaType mt;
  ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), &mt));
  EXPECT_EQ(h.width, 352);
  EXPECT_EQ(h.height, 288);
  EXPECT_EQ(mt.subtype, MEDIASUBTYPE_MPEG1Payload);
}

TEST(FrameHeaderParser, Mpeg2FrameRates)
{
  struct { int code; unsigned ifps; } rates[] = { { 1, 417083 }, { 2, 416666 }, { 3, 400000 }, { 4, 333666 }, { 5, 333333 }, { 6, 200000 }, { 7, 166833 }, { 8, 166666 } };
  for (auto r : rates)
  {
    Parser s(Mpeg2SequenceHeader(720, 576, 2, r.code, true));
    seqhdr h;
    ASSERT_TRUE(s.p->Read(h, static_cast<int>(s.data.size()), nullptr)) << r.code;
    EXPECT_NEAR(static_cast<double>(h.ifps), static_cast<double>(r.ifps), 1.0) << r.code;
  }
}

TEST(FrameHeaderParser, Mpeg2SequenceHeaderWithBadValuesIsRefused)
{
  seqhdr h;
  Parser noRate(Mpeg2SequenceHeader(720, 576, 2, 0, true));                 // frame rate code 0
  EXPECT_FALSE(noRate.p->Read(h, static_cast<int>(noRate.data.size()), nullptr));
  Parser tiny(Mpeg2SequenceHeader(64, 48, 2, 3, true));                     // below 100 x 100
  EXPECT_FALSE(tiny.p->Read(h, static_cast<int>(tiny.data.size()), nullptr));
  Parser none(Data(200, 0x44));
  EXPECT_FALSE(none.p->Read(h, 200, nullptr));
}

TEST(FrameHeaderParser, FindsMpegStartCodes)
{
  Parser s(Data{ 0x12, 0x34, 0x00, 0x00, 0x01, 0xE0, 0x55, 0x00, 0x00, 0x01, 0xB3 });
  BYTE code = 0;
  ASSERT_TRUE(s.p->NextMpegStartCode(code));
  EXPECT_EQ(code, 0xE0);
  ASSERT_TRUE(s.p->NextMpegStartCode(code));
  EXPECT_EQ(code, 0xB3);
  EXPECT_FALSE(s.p->NextMpegStartCode(code));
}

TEST(FrameHeaderParser, StartCodeSearchHonoursTheLimit)
{
  Data d(100, 0x55);
  d.insert(d.end(), { 0x00, 0x00, 0x01, 0xB3 });
  Parser s(d);
  BYTE code = 0;
  EXPECT_FALSE(s.p->NextMpegStartCode(code, 50));
}

TEST(FrameHeaderParser, PesHeaderWithPtsAndDts)
{
  Data d{ 0x00, 0x00, 0x84, 0xC0, 0x0A };                       // length 0, MPEG-2 flags, PTS and DTS, 10 header bytes
  d[0] = 0x00; d[1] = 0x00;
  BYTE pts[5], dts[5];
  EncodeTimestamp(pts, 3, 90000);                               // one second
  EncodeTimestamp(dts, 1, 86400);
  d.insert(d.end(), pts, pts + 5);
  d.insert(d.end(), dts, dts + 5);
  d.insert(d.end(), 20, 0xAA);
  Parser s(d);
  peshdr h;
  ASSERT_TRUE(s.p->Read(h, 0xE0));
  EXPECT_EQ(h.type, 2);
  EXPECT_EQ(h.fpts, 1);
  EXPECT_EQ(h.fdts, 1);
  EXPECT_EQ(h.pts, 10000000);
  EXPECT_EQ(h.dts, 9600000);
  EXPECT_EQ(h.hdrlen, 10);
  EXPECT_EQ(h.alignment, 1);
}

TEST(FrameHeaderParser, PesHeaderOfAStreamIdThatHasNone)
{
  Parser s(Data{ 0x00, 0x10, 0x55, 0x55 });
  peshdr h;
  EXPECT_FALSE(s.p->Read(h, 0xBC));                              // program stream map
  Parser t(Data{ 0x00, 0x10, 0x55, 0x55 });
  EXPECT_TRUE(t.p->Read(h, 0xBE));                               // padding: only the length
  EXPECT_EQ(h.len, 0x10);
}

// ---------------------------------------------------------------------------------------------------------------------------
// H.264
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  struct SpsOptions
  {
    int profile = 100;
    int level = 40;
    int chromaFormat = 1;
    int widthInMbs = 120;
    int heightInMapUnits = 68;
    int frameMbsOnly = 1;
    bool cropping = true;
    int aspectIdc = 1;           // -1: no aspect ratio information, 255: extended
    int sarWidth = 0, sarHeight = 0;
    bool timing = true;
    uint32_t unitsInTick = 1, timeScale = 50;
  };

  Data Sps(const SpsOptions& o)
  {
    BitWriter w;
    w.Put(o.profile, 8).Put(0, 8).Put(o.level, 8).UE(0);
    if (o.profile >= 100)
    {
      w.UE(o.chromaFormat);
      if (o.chromaFormat == 3) w.Put(0, 1);
      w.UE(0).UE(0).Put(0, 1).Put(0, 1);
    }
    w.UE(0);                                         // log2_max_frame_num_minus4
    w.UE(0).UE(0);                                   // pic_order_cnt_type 0 and its lsb
    w.UE(4).Put(0, 1);                               // num_ref_frames, gaps
    w.UE(o.widthInMbs - 1).UE(o.heightInMapUnits - 1).Put(o.frameMbsOnly, 1);
    if (!o.frameMbsOnly) w.Put(0, 1);
    w.Put(1, 1);                                     // direct_8x8_inference
    w.Put(o.cropping ? 1 : 0, 1);
    if (o.cropping) w.UE(0).UE(0).UE(0).UE(4);
    w.Put(1, 1);                                     // vui_parameters_present_flag
    if (o.aspectIdc >= 0)
    {
      w.Put(1, 1).Put(o.aspectIdc, 8);
      if (o.aspectIdc == 255) w.Put(o.sarWidth, 16).Put(o.sarHeight, 16);
    }
    else
      w.Put(0, 1);
    w.Put(0, 1);                                     // overscan
    w.Put(0, 1);                                     // video signal type
    w.Put(0, 1);                                     // chroma location
    if (o.timing)
      w.Put(1, 1).Put(o.unitsInTick, 32).Put(o.timeScale, 32).Put(1, 1);
    else
      w.Put(0, 1);
    w.Put(0, 1);                                     // nal_hrd
    w.Put(0, 1);                                     // vcl_hrd
    w.Put(0, 1);                                     // pic_struct_present
    w.Put(0, 1);                                     // bitstream_restriction
    w.Trailing();
    return w.Bytes();
  }

  // [4 byte length][NAL header][payload with emulation prevention]
  Data Nal(int header, const Data& rbsp)
  {
    Data payload = BitWriter::EmulationPrevention(rbsp);
    const uint32_t length = static_cast<uint32_t>(payload.size() + 1);
    Data d{ static_cast<BYTE>(length >> 24), static_cast<BYTE>(length >> 16), static_cast<BYTE>(length >> 8), static_cast<BYTE>(length), static_cast<BYTE>(header) };
    d.insert(d.end(), payload.begin(), payload.end());
    return d;
  }

  Data Pps() { return Nal(0x68, Data{ 0xCE, 0x3C, 0x80 }); }
}

TEST(FrameHeaderParser, H264SpsAndPps)
{
  Parser sps(Nal(0x67, Sps(SpsOptions())));
  avchdr h;
  CMediaType mt;
  EXPECT_FALSE(sps.p->Read(h, static_cast<int>(sps.data.size()), &mt, true)) << "not usable without the PPS";
  EXPECT_EQ(h.profile, 100);
  EXPECT_EQ(h.level, 40);
  EXPECT_EQ(h.width, 1920u);
  EXPECT_EQ(h.height, 1080u) << "1088 is reported as 1080";
  EXPECT_TRUE(h.progressive);
  EXPECT_EQ(h.AvgTimePerFrame, 400000) << "time_scale 50 with a tick of 1: 25 frames, counted as 50 fields";
  EXPECT_EQ(h.arx, 16);
  EXPECT_EQ(h.ary, 9);

  Parser pps(Pps());
  EXPECT_TRUE(pps.p->Read(h, static_cast<int>(pps.data.size()), &mt, false));
  EXPECT_EQ(mt.subtype, FOURCCMap(MAKEFOURCC('A', 'V', 'C', '1')));
  ASSERT_EQ(mt.formattype, FORMAT_MPEG2_VIDEO);
  const MPEG2VIDEOINFO* vi = reinterpret_cast<const MPEG2VIDEOINFO*>(mt.Format());
  EXPECT_EQ(vi->dwProfile, 100u);
  EXPECT_EQ(vi->dwLevel, 40u);
  EXPECT_EQ(vi->hdr.bmiHeader.biWidth, 1920);
  EXPECT_EQ(vi->hdr.AvgTimePerFrame, 400000);
  EXPECT_EQ(vi->hdr.bmiHeader.biBitCount, 12);
  EXPECT_GT(vi->cbSequenceHeader, 0u);
}

TEST(FrameHeaderParser, H264InterlacedAndSmallerSizes)
{
  SpsOptions o;
  o.profile = 77;
  o.widthInMbs = 45;                // 720
  o.heightInMapUnits = 18;          // 18 map units of two macroblock rows: 576
  o.frameMbsOnly = 0;
  o.cropping = false;
  Parser sps(Nal(0x67, Sps(o)));
  avchdr h;
  sps.p->Read(h, static_cast<int>(sps.data.size()), nullptr, true);
  EXPECT_EQ(h.width, 720u);
  EXPECT_EQ(h.height, 576u);
  EXPECT_FALSE(h.progressive);
}

TEST(FrameHeaderParser, H264AspectRatios)
{
  SpsOptions o;
  o.aspectIdc = 255;
  o.sarWidth = 0;
  o.sarHeight = 5;
  Parser zero(Nal(0x67, Sps(o)));
  avchdr h;
  zero.p->Read(h, static_cast<int>(zero.data.size()), nullptr, true);
  EXPECT_EQ(h.arx, 0) << "a sample aspect ratio with a zero is dropped";
  EXPECT_EQ(h.ary, 0);

  o.aspectIdc = 99;                                      // reserved value
  Parser reserved(Nal(0x67, Sps(o)));
  reserved.p->Read(h, static_cast<int>(reserved.data.size()), nullptr, true);
  EXPECT_EQ(h.arx, 0);
  EXPECT_EQ(h.ary, 0);

  o.aspectIdc = 255;                                     // an extended sample aspect ratio of 32:27 on 1920 x 1080
  o.sarWidth = 32;
  o.sarHeight = 27;
  Parser extended(Nal(0x67, Sps(o)));
  extended.p->Read(h, static_cast<int>(extended.data.size()), nullptr, true);
  EXPECT_EQ(h.arx, 512);                                // 32 * 1920 : 27 * 1080 reduced by 120
  EXPECT_EQ(h.ary, 243);
}

TEST(FrameHeaderParser, H264WithoutTimingInformationAssumes27Hz)
{
  SpsOptions o;
  o.timing = false;
  Parser sps(Nal(0x67, Sps(o)));
  avchdr h;
  sps.p->Read(h, static_cast<int>(sps.data.size()), nullptr, true);
  EXPECT_EQ(h.AvgTimePerFrame, 370000);
}

TEST(FrameHeaderParser, H264HighBitDepthAndChromaFormats)
{
  SpsOptions o;
  o.chromaFormat = 3;
  Parser sps(Nal(0x67, Sps(o)));
  avchdr h;
  sps.p->Read(h, static_cast<int>(sps.data.size()), nullptr, true);
  EXPECT_EQ(h.chromaFormat, 3u);
  EXPECT_EQ(h.lumaDepth, 8);
}

TEST(FrameHeaderParser, H264SpsWithEmulationPreventionBytes)
{
  // time_scale 1 and a tick of 1 put 00 00 00 01 into the byte stream twice: the encoder has to insert 03 bytes, and the parser
  // has to take them out again before it reads the fields behind them
  SpsOptions o;
  o.unitsInTick = 1;
  o.timeScale = 1;
  const Data nal = Nal(0x67, Sps(o));
  bool escaped = false;
  for (size_t i = 0; i + 2 < nal.size(); i++)
    if (nal[i] == 0 && nal[i + 1] == 0 && nal[i + 2] == 3)
      escaped = true;
  ASSERT_TRUE(escaped);
  Parser sps(nal);
  avchdr h;
  sps.p->Read(h, static_cast<int>(sps.data.size()), nullptr, true);
  EXPECT_EQ(h.width, 1920u);
  EXPECT_EQ(h.AvgTimePerFrame, 20000000);
}

TEST(FrameHeaderParser, EmulationPreventionHelper)
{
  EXPECT_EQ(BitWriter::EmulationPrevention(Data{ 0, 0, 1, 2, 0, 0, 3, 9 }), (Data{ 0, 0, 3, 1, 2, 0, 0, 3, 3, 9 }));
  EXPECT_EQ(BitWriter::EmulationPrevention(Data{ 0, 0, 4 }), (Data{ 0, 0, 4 }));
}

TEST(FrameHeaderParser, H264IgnoresOtherNalUnitsAndTooShortData)
{
  avchdr h;
  Parser slice(Nal(0x65, Data{ 1, 2, 3, 4, 5, 6 }));
  EXPECT_FALSE(slice.p->Read(h, static_cast<int>(slice.data.size()), nullptr, true));
  Parser unreferenced(Nal(0x07, Sps(SpsOptions())));          // nal_ref_idc 0: not accepted
  EXPECT_FALSE(unreferenced.p->Read(h, static_cast<int>(unreferenced.data.size()), nullptr, true));
  Parser tiny(Data{ 0, 0, 0, 1, 0x67 });
  EXPECT_FALSE(tiny.p->Read(h, 5, nullptr, true));
  Parser huge(Data(70000, 0));
  EXPECT_FALSE(huge.p->Read(h, 70000, nullptr, true));
}

// ---------------------------------------------------------------------------------------------------------------------------
// CMpegPesParser: what the demultiplexer calls with a whole frame
// ---------------------------------------------------------------------------------------------------------------------------

TEST(MpegPesParser, AudioFramesGiveTheBasicAudioInfo)
{
  CMpegPesParser parser;
  Data aac = AdtsHeader(1, 3, 6, 700);
  aac.resize(100, 0);
  ASSERT_TRUE(parser.OnAudioPacket(aac.data(), static_cast<int>(aac.size()), SERVICE_TYPE_AUDIO_AAC, 2, true));
  EXPECT_EQ(parser.basicAudioInfo.sampleRate, 48000);
  EXPECT_EQ(parser.basicAudioInfo.channels, 6);
  EXPECT_EQ(parser.basicAudioInfo.aacObjectType, 2);
  EXPECT_EQ(parser.basicAudioInfo.streamIndex, 2u);
  EXPECT_TRUE(parser.basicAudioInfo.isValid);
  EXPECT_TRUE(parser.basicAudioInfo.pmtValid);

  Data ac3 = Ac3Header(0, 28, 8, 7, 1);
  ASSERT_TRUE(parser.OnAudioPacket(ac3.data(), static_cast<int>(ac3.size()), SERVICE_TYPE_AUDIO_AC3, 0, true));
  EXPECT_EQ(parser.basicAudioInfo.channels, 6);
  EXPECT_EQ(parser.basicAudioInfo.bitrate, 384000);

  Data mpa = MpaHeader(3, 2, 1, 10, 1, 0, 0);
  mpa.resize(64, 0);
  ASSERT_TRUE(parser.OnAudioPacket(mpa.data(), static_cast<int>(mpa.size()), SERVICE_TYPE_AUDIO_MPEG2, 0, true));
  EXPECT_EQ(parser.basicAudioInfo.sampleRate, 48000);
  EXPECT_EQ(parser.basicAudioInfo.channels, 2);
  EXPECT_EQ(parser.basicAudioInfo.bitrate, 192000);

  Data eac3 = Eac3Header(767, 0, 0, 7, 1, 16);
  ASSERT_TRUE(parser.OnAudioPacket(eac3.data(), static_cast<int>(eac3.size()), SERVICE_TYPE_AUDIO_DD_PLUS, 0, true));
  EXPECT_EQ(parser.basicAudioInfo.channels, 6);
}

TEST(MpegPesParser, AudioThatCannotBeParsedGetsDefaults)
{
  CMpegPesParser parser;
  Data junk(100, 0x55);
  EXPECT_FALSE(parser.OnAudioPacket(junk.data(), 100, SERVICE_TYPE_AUDIO_AC3, 0, true));
  EXPECT_EQ(parser.basicAudioInfo.sampleRate, 48000);
  EXPECT_EQ(parser.basicAudioInfo.channels, 6) << "AC-3 is assumed to be 5.1";
  EXPECT_TRUE(parser.basicAudioInfo.isValid);
  EXPECT_FALSE(parser.basicAudioInfo.pmtValid);

  EXPECT_FALSE(parser.OnAudioPacket(junk.data(), 100, SERVICE_TYPE_AUDIO_MPEG2, 0, true));
  EXPECT_EQ(parser.basicAudioInfo.channels, 2);

  EXPECT_FALSE(parser.OnAudioPacket(junk.data(), 100, SERVICE_TYPE_AUDIO_LATM_AAC, 0, true));
  EXPECT_EQ(parser.basicAudioInfo.aacObjectType, 2);

  EXPECT_FALSE(parser.OnAudioPacket(nullptr, 0, SERVICE_TYPE_AUDIO_AAC, 0, true));
}

TEST(MpegPesParser, ResetsClearTheInformation)
{
  CMpegPesParser parser;
  Data aac = AdtsHeader(1, 3, 6, 700);
  aac.resize(100, 0);
  parser.OnAudioPacket(aac.data(), 100, SERVICE_TYPE_AUDIO_AAC, 1, true);
  parser.AudioValidReset();
  EXPECT_FALSE(parser.basicAudioInfo.isValid);
  EXPECT_EQ(parser.basicAudioInfo.sampleRate, 48000);
  parser.AudioReset();
  EXPECT_EQ(parser.basicAudioInfo.sampleRate, 0);
  EXPECT_EQ(parser.basicAudioInfo.channels, 0);
  EXPECT_EQ(parser.basicAudioInfo.streamType, SERVICE_TYPE_AUDIO_UNKNOWN);
}

TEST(MpegPesParser, Mpeg2VideoGivesTheBasicVideoInfo)
{
  CMpegPesParser parser;
  Data video = Mpeg2SequenceHeader(1920, 1080, 3, 4, true);
  ASSERT_TRUE(parser.OnTsPacket(video.data(), static_cast<int>(video.size()), VIDEO_STREAM_TYPE_MPEG2, true));
  EXPECT_EQ(parser.basicVideoInfo.width, 1920);
  EXPECT_EQ(parser.basicVideoInfo.height, 1080);
  EXPECT_NEAR(parser.basicVideoInfo.fps, 29.97, 0.01);
  EXPECT_EQ(parser.basicVideoInfo.arx, 16);
  EXPECT_EQ(parser.basicVideoInfo.ary, 9);
  EXPECT_TRUE(parser.basicVideoInfo.isValid);
  EXPECT_EQ(parser.basicVideoInfo.isInterlaced, 0);
  parser.VideoValidReset();
  EXPECT_FALSE(parser.basicVideoInfo.isValid);
  parser.VideoReset();
  EXPECT_EQ(parser.basicVideoInfo.width, 0);
}

TEST(MpegPesParser, H264VideoNeedsSpsAndPps)
{
  CMpegPesParser parser;
  Data sps = Nal(0x67, Sps(SpsOptions()));
  EXPECT_FALSE(parser.OnTsPacket(sps.data(), static_cast<int>(sps.size()), VIDEO_STREAM_TYPE_H264, true));
  Data pps = Pps();
  ASSERT_TRUE(parser.OnTsPacket(pps.data(), static_cast<int>(pps.size()), VIDEO_STREAM_TYPE_H264, false));
  EXPECT_EQ(parser.basicVideoInfo.width, 1920);
  EXPECT_EQ(parser.basicVideoInfo.height, 1080);
  EXPECT_NEAR(parser.basicVideoInfo.fps, 25.0, 0.01);
  EXPECT_GT(parser.basicVideoInfo.spslen, 0);
  EXPECT_GT(parser.basicVideoInfo.ppslen, 0);
  EXPECT_EQ(parser.basicVideoInfo.streamType, VIDEO_STREAM_TYPE_H264);
}

// ---------------------------------------------------------------------------------------------------------------------------
// damaged data
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  uint32_t g_seed = 20240601;
  uint32_t Next() { g_seed = g_seed * 1664525u + 1013904223u; return g_seed >> 8; }

  // Runs every parser on 'data' placed right in front of a guard page
  void ParseEverything(const Data& data)
  {
    GuardedBuffer buffer(data);
    const int len = static_cast<int>(data.size());
    {
      mpahdr h; CFrameHeaderParser p; p.Reset(buffer.data(), len); p.Read(h, len, false);
    }
    {
      mpahdr h; CMediaType mt; CFrameHeaderParser p; p.Reset(buffer.data(), len); p.Read(h, len, true, &mt);
    }
    {
      aachdr h; CMediaType mt; CFrameHeaderParser p; p.Reset(buffer.data(), len); p.Read(h, len, &mt);
    }
    {
      ac3hdr h; CMediaType mt; CFrameHeaderParser p; p.Reset(buffer.data(), len); p.Read(h, len, &mt);
    }
    {
      dtshdr h; CMediaType mt; CFrameHeaderParser p; p.Reset(buffer.data(), len); p.Read(h, len, &mt);
    }
    {
      seqhdr h; CMediaType mt; CFrameHeaderParser p; p.Reset(buffer.data(), len); p.Read(h, len, &mt);
    }
    {
      peshdr h; CFrameHeaderParser p; p.Reset(buffer.data(), len); p.Read(h, 0xE0); p.Read(h, 0xC0);
    }
    {
      // The length in front of the NAL unit is made by the demultiplexer, it is not read from the stream: keep it true to the data.
      // A false one is refused (H264NalLengthBeyondTheDataIsRefusedWithoutBigAllocations).
      Data nal = data;
      if (nal.size() >= 6)
      {
        const uint32_t length = static_cast<uint32_t>(nal.size() - 4);
        nal[0] = static_cast<BYTE>(length >> 24); nal[1] = static_cast<BYTE>(length >> 16);
        nal[2] = static_cast<BYTE>(length >> 8);  nal[3] = static_cast<BYTE>(length);
        nal[4] = (nal[4] & 1) ? 0x67 : 0x68;
      }
      GuardedBuffer avcBuffer(nal);
      avchdr h; CMediaType mt; CFrameHeaderParser p; p.Reset(avcBuffer.data(), len);
      p.Read(h, len, &mt, true);
      p.Reset(avcBuffer.data(), len);
      p.Read(h, len, &mt, false);
    }
  }

}

// KNOWN DEFECT (disabled): the length of the NAL unit is taken from the first four bytes of the data and trusted: malloc() of that
// size for the SPS copy and again for the unescaped copy, whatever the data holds (here 64 MB, a gigabyte just the same). The
// length has to be checked against the size of the data first. The test watches the allocations (Debug CRT only).
#ifdef _DEBUG
namespace
{
  size_t g_largestAllocation = 0;

  int AllocationHook(int type, void*, size_t size, int, long, const unsigned char*, int)
  {
    if (type == _HOOK_ALLOC && size > g_largestAllocation)
      g_largestAllocation = size;
    return TRUE;
  }
}
#endif

TEST(FrameHeaderParser, H264NalLengthBeyondTheDataIsRefusedWithoutBigAllocations)
{
#ifndef _DEBUG
  GTEST_SKIP() << "allocations are only watched in Debug builds";
#else
  Data d{ 0x04, 0x00, 0x00, 0x00, 0x67, 0x64, 0x00, 0x28, 0xAC, 0xD9, 0x40, 0x78, 0x02, 0x27, 0xE5, 0xC0, 0x44, 0, 0, 0 };   // length 64 MB
  Parser s(d);
  avchdr h;
  g_largestAllocation = 0;
  _CRT_ALLOC_HOOK previous = _CrtSetAllocHook(AllocationHook);
  const bool ok = s.p->Read(h, static_cast<int>(d.size()), nullptr, true);
  _CrtSetAllocHook(previous);
  EXPECT_FALSE(ok);
  EXPECT_LT(g_largestAllocation, static_cast<size_t>(1 << 20)) << "allocated " << g_largestAllocation << " bytes for 20 bytes of data";
#endif
}

TEST(FrameHeaderParser, RandomDataNeverReadsBehindTheBuffer)
{
  ResetCrtAsserts();
  for (int i = 0; i < 4000; i++)
  {
    Data d(1 + Next() % 120);
    for (BYTE& b : d)
      b = static_cast<BYTE>(Next());
    ParseEverything(d);
  }
}

TEST(FrameHeaderParser, DamagedValidHeadersNeverReadBehindTheBuffer)
{
  ResetCrtAsserts();
  std::vector<Data> valid = {
    Concat(MpaHeader(3, 2, 1, 10, 1, 0, 0), Data(30, 0)), Concat(AdtsHeader(1, 4, 2, 400), Data(30, 0)), Ac3Header(0, 28, 8, 7, 1),
    DtsHeader(9, 13, 17, 1), Mpeg2SequenceHeader(720, 576, 2, 3, true), Nal(0x67, Sps(SpsOptions())), Pps() };
  for (const Data& v : valid)
    for (int i = 0; i < 1500; i++)
    {
      Data d = v;
      const int changes = 1 + Next() % 4;
      for (int c = 0; c < changes; c++)
        d[Next() % d.size()] = static_cast<BYTE>(Next());
      if (Next() % 4 == 0)
        d.resize(1 + Next() % d.size());                  // cut short
      ParseEverything(d);
    }
}

TEST(FrameHeaderParser, TruncatedValidHeadersNeverReadBehindTheBuffer)
{
  ResetCrtAsserts();
  std::vector<Data> valid = {
    Concat(MpaHeader(3, 2, 1, 10, 1, 0, 0), Data(8, 0)), Concat(AdtsHeader(1, 4, 2, 400), Data(8, 0)), Ac3Header(0, 28, 8, 7, 1),
    DtsHeader(9, 13, 17, 1), Mpeg2SequenceHeader(720, 576, 2, 3, true), Nal(0x67, Sps(SpsOptions())), Pps() };
  for (const Data& v : valid)
    for (size_t cut = 1; cut <= v.size(); cut++)
      ParseEverything(Data(v.begin(), v.begin() + cut));
}

namespace
{
  // One random SPS NAL unit through the HEVC parser; true when the parser called abort() (an assert() in Debug builds)
  bool HevcAborts(const Data& d)
  {
    GuardedBuffer buffer(d);
    HEVC::HevcNalDecode* decoder = new HEVC::HevcNalDecode();
    HEVC::hevchdr* h = new HEVC::hevchdr();
    const uint8_t* data = buffer.data();
    const size_t size = buffer.size();
    return abort_guard::Aborts([&]() { decoder->processNALUnit(data, size, *h); });
  }

  Data RandomHevcSps()
  {
    Data d(3 + Next() % 100);
    for (BYTE& b : d)
      b = static_cast<BYTE>(Next());
    d[0] = static_cast<BYTE>((33 << 1) | (Next() & 1));         // SPS NAL header
    d[1] = 1;
    return d;
  }
}

TEST(FrameHeaderParser, HevcSpsParserNeverReadsBehindTheBuffer)
{
  // an access violation here fails the test; an abort() from an assert() is counted by the next test
  for (int i = 0; i < 6000; i++)
    HevcAborts(RandomHevcSps());
}

TEST(FrameHeaderParser, HevcSpsWithImplausibleValuesDoesNotAbort)
{
  int aborts = 0;
  for (int i = 0; i < 6000; i++)
    if (HevcAborts(RandomHevcSps()))
      aborts++;
  EXPECT_EQ(aborts, 0);
}
