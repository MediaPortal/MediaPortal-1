#include <gtest/gtest.h>
#include <algorithm>
#include <cstdint>
#include <vector>
#include <windows.h>
#include <streams.h>
#include "BitWriter.h"
#include "GuardedBuffer.h"
#include "GolombBuffer.h"
#include "FrameHeaderParser.h"

// BDReader parses the elementary streams of a disc: the lengths and sizes in the stream cannot be trusted.

namespace
{
  std::vector<uint8_t> LengthPrefixed(const std::vector<uint8_t>& nal)
  {
    const uint32_t n = static_cast<uint32_t>(nal.size());
    std::vector<uint8_t> out = { static_cast<uint8_t>(n >> 24), static_cast<uint8_t>(n >> 16), static_cast<uint8_t>(n >> 8), static_cast<uint8_t>(n) };
    out.insert(out.end(), nal.begin(), nal.end());
    return out;
  }

  // a baseline profile SPS with a VUI that carries the timing
  std::vector<uint8_t> SpsNal(uint64_t widthMbsMinus1, uint64_t heightMapUnitsMinus1, uint32_t numUnitsInTick, uint32_t timeScale)
  {
    BitWriter w;
    w.Put(66, 8).Put(0, 8).Put(40, 8);          // profile_idc, constraint flags, level_idc
    w.UE(0);                                    // seq_parameter_set_id
    w.UE(0);                                    // log2_max_frame_num_minus4
    w.UE(2);                                    // pic_order_cnt_type
    w.UE(1);                                    // num_ref_frames
    w.Put(0, 1);                                // gaps_in_frame_num_value_allowed_flag
    w.UE(widthMbsMinus1).UE(heightMapUnitsMinus1);
    w.Put(1, 1);                                // frame_mbs_only_flag
    w.Put(1, 1);                                // direct_8x8_inference_flag
    w.Put(0, 1);                                // frame_cropping_flag
    w.Put(1, 1);                                // vui_parameters_present_flag
    w.Put(0, 1).Put(0, 1).Put(0, 1).Put(0, 1); // aspect ratio, overscan, video signal type, chroma location
    w.Put(1, 1);                                // timing_info_present_flag
    w.Put(numUnitsInTick, 32).Put(timeScale, 32).Put(1, 1);
    w.Trailing();
    std::vector<uint8_t> nal = { 0x67 };
    const std::vector<uint8_t> ebsp = BitWriter::EmulationPrevention(w.Bytes());
    nal.insert(nal.end(), ebsp.begin(), ebsp.end());
    return nal;
  }

  const std::vector<uint8_t> PpsNal = { 0x68, 0xCE, 0x3C, 0x80 };

  // the length of the SPS is at SpsLengthOffset
  const size_t SpsLengthOffset = 6;

  std::vector<uint8_t> Frame(const std::vector<uint8_t>& sps)
  {
    std::vector<uint8_t> frame = LengthPrefixed({ 0x09, 0xF0 });      // access unit delimiter
    const std::vector<uint8_t> lengthAndSps = LengthPrefixed(sps);
    frame.insert(frame.end(), lengthAndSps.begin(), lengthAndSps.end());
    const std::vector<uint8_t> pps = LengthPrefixed(PpsNal);
    frame.insert(frame.end(), pps.begin(), pps.end());
    return frame;
  }

  bool ReadAvc(const std::vector<uint8_t>& bytes, avchdr& h)
  {
    GuardedBuffer buffer(bytes);
    CFrameHeaderParser parser;
    parser.Reset(buffer.data(), static_cast<int>(buffer.size()));
    return parser.Read(h, static_cast<int>(buffer.size()), nullptr);
  }
}

TEST(BdAvcHeader, ReadsASimpleSps)
{
  avchdr h;
  ASSERT_TRUE(ReadAvc(Frame(SpsNal(119, 67, 1001, 60000)), h));
  EXPECT_EQ(h.width, 1920u);
  EXPECT_EQ(h.height, 1080u);                  // 1088 is shown as 1080
  EXPECT_EQ(h.AvgTimePerFrame, 333666);        // 30000/1001 frames per second
  EXPECT_TRUE(h.progressive);
}

TEST(BdAvcHeader, ZeroTicksInTheVuiDoNotDivideByZero)
{
  avchdr h;
  EXPECT_FALSE(ReadAvc(Frame(SpsNal(119, 67, 0, 50)), h));
}

TEST(BdAvcHeader, NalLengthBeyondTheBufferIsRejected)
{
  std::vector<uint8_t> frame = Frame(SpsNal(119, 67, 1001, 60000));
  std::fill(frame.begin() + SpsLengthOffset, frame.begin() + SpsLengthOffset + 4, 0xFF);
  avchdr h;
  EXPECT_FALSE(ReadAvc(frame, h));
}

TEST(BdAvcHeader, ZeroNalLengthIsRejected)
{
  std::vector<uint8_t> frame = Frame(SpsNal(119, 67, 1001, 60000));
  std::fill(frame.begin() + SpsLengthOffset, frame.begin() + SpsLengthOffset + 4, 0);
  avchdr h;
  EXPECT_FALSE(ReadAvc(frame, h));
}

TEST(BdAvcHeader, AbsurdFrameSizeIsRejected)
{
  avchdr h;
  EXPECT_FALSE(ReadAvc(Frame(SpsNal(5000, 67, 1001, 60000)), h));
  EXPECT_FALSE(ReadAvc(Frame(SpsNal(119, 5000, 1001, 60000)), h));
}

TEST(BdHevcHeader, NalLengthBeyondTheBufferIsRejected)
{
  // the length says 2 GB, the buffer holds a few bytes: processNALUnit and malloc must not get that length
  const std::vector<uint8_t> bytes = { 0x7F, 0xFF, 0xFF, 0xFF, 0x42, 0x01, 0x01, 0x01, 0x60, 0x00, 0x00, 0x03 };
  GuardedBuffer buffer(bytes);
  CFrameHeaderParser parser;
  parser.Reset(buffer.data(), static_cast<int>(buffer.size()));
  hevchdr h;
  EXPECT_FALSE(parser.Read(h, static_cast<int>(buffer.size()), nullptr));
}

TEST(BdGolombBuffer, ReadBufferPastTheEndCopiesNothing)
{
#ifdef _DEBUG
  GTEST_SKIP() << "ReadBuffer asserts when it is asked for more than is left";
#else
  GuardedBuffer source(std::vector<uint8_t>{ 1, 2, 3, 4 });
  CGolombBuffer gb(source.data(), static_cast<int>(source.size()));
  gb.Seek(10);
  std::vector<uint8_t> dest(8, 0xEE);
  gb.ReadBuffer(dest.data(), 4);
  EXPECT_EQ(dest, std::vector<uint8_t>(8, 0xEE));
#endif
}

TEST(BdGolombBuffer, ByteReadWithAnImpossibleLengthCopiesNothing)
{
#ifdef _DEBUG
  GTEST_SKIP() << "ReadBuffer asserts when it is asked for more than is left";
#else
  GuardedBuffer source(std::vector<uint8_t>{ 1, 2, 3, 4 });
  CGolombBuffer gb(source.data(), static_cast<int>(source.size()));
  std::vector<uint8_t> dest(8, 0xEE);
  gb.ByteRead(dest.data(), -1);
  EXPECT_EQ(dest, std::vector<uint8_t>(8, 0xEE));
  gb.ByteRead(dest.data(), 0x100000000LL);   // more than an int can hold: only what is left is copied
  EXPECT_EQ(dest[0], 1);
  EXPECT_EQ(dest[3], 4);
  EXPECT_EQ(dest[4], 0xEE);
#endif
}

TEST(BdGolombBuffer, SeekBeforeTheStartReadsZeros)
{
  const std::vector<uint8_t> memory = { 0x55, 0xAA, 0xAA };   // 0x55 is not part of the buffer
  CGolombBuffer gb(const_cast<BYTE*>(memory.data()) + 1, 2);
  gb.Seek(-1);
  EXPECT_EQ(gb.BitRead(8), 0u);
}
