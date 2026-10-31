// LiveMedia555: MPMPEG2TransportStreamFromPESSource, the program stream to transport stream conversion behind the MPWriter
// filter, with a real program stream. Needs <samples>\Video\a52_dvb_remuxed.m2p.
#include <gtest/gtest.h>
#include <winsock2.h>
#include <chrono>
#include <vector>
#include "BasicUsageEnvironment.hh"
#include "ByteStreamFileSource.hh"
#include "MPEG1or2Demux.hh"
#include "MPMPEG2TransportStreamFromPESSource.h"
#include "MPTaskScheduler.h"
#include "LocalSupport.h"

void LogDebug(const wchar_t*, ...) {}

namespace
{
  struct Frames
  {
    bool pending = false;
    bool closed = false;
    unsigned size = 0;
  };

  void OnFrame(void* context, unsigned size, unsigned, struct timeval, unsigned)
  {
    Frames* f = static_cast<Frames*>(context);
    f->size = size;
    f->pending = false;
  }

  void OnClosed(void* context) { static_cast<Frames*>(context)->closed = true; }
}

TEST(LiveMediaPesToTs, ProducesAlignedTransportPackets)
{
  REQUIRE_SAMPLE(path, L"Video\\a52_dvb_remuxed.m2p");
  const std::string file = ToUtf8(path);

  MPTaskScheduler* scheduler = MPTaskScheduler::createNew();
  UsageEnvironment* env = BasicUsageEnvironment::createNew(*scheduler);

  ByteStreamFileSource* input = ByteStreamFileSource::createNew(*env, file.c_str());
  ASSERT_NE(input, nullptr);
  MPEG1or2Demux* demux = MPEG1or2Demux::createNew(*env, input);
  MPEG1or2DemuxedElementaryStream* pes = demux->newRawPESStream();
  MPMPEG2TransportStreamFromPESSource* ts = MPMPEG2TransportStreamFromPESSource::createNew(*env, pes, false);
  ASSERT_NE(ts, nullptr);

  Frames frames;
  std::vector<unsigned char> data;
  std::vector<unsigned char> buffer(188 * 20);
  const auto end = std::chrono::steady_clock::now() + std::chrono::seconds(20);
  while (data.size() < 188u * 2000 && !frames.closed && std::chrono::steady_clock::now() < end)
  {
    frames.pending = true;
    ts->getNextFrame(buffer.data(), static_cast<unsigned>(buffer.size()), OnFrame, &frames, OnClosed, &frames);
    while (frames.pending && !frames.closed && std::chrono::steady_clock::now() < end)
      static_cast<TaskScheduler*>(scheduler)->doEventLoop();   // one step, waits up to 1 s for an event
    if (!frames.pending)
      data.insert(data.end(), buffer.begin(), buffer.begin() + frames.size);
  }

  ASSERT_GE(data.size(), 188u * 100) << "the converter delivered hardly anything";
  EXPECT_EQ(data.size() % 188, 0u) << "transport packets are 188 bytes";
  int pid0 = 0, bad = 0;
  std::vector<int> pids;
  for (size_t i = 0; i + 188 <= data.size(); i += 188)
  {
    if (data[i] != 0x47) { bad++; continue; }
    const int pid = ((data[i + 1] & 0x1F) << 8) | data[i + 2];
    if (pid == 0) pid0++;
    bool known = false;
    for (int p : pids) known = known || p == pid;
    if (!known) pids.push_back(pid);
  }
  EXPECT_EQ(bad, 0) << "packets without the sync byte 0x47";
  EXPECT_GT(pid0, 0) << "no PAT";
  EXPECT_GE(pids.size(), 3u) << "PAT, PMT and at least one stream expected";

  Medium::close(ts);
  env->reclaim();
  delete scheduler;
}
