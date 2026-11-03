// Finds the DVB subtitle stream of a transport stream file (PAT -> PMT -> stream with a subtitling descriptor 0x59)
// and collects its TS packets, so a test can feed them to the DVBSub3 filter.
#pragma once

#include <windows.h>
#include <cstdint>
#include <string>
#include <vector>

struct SubtitleStream
{
  int pid = -1;
  std::string language;                 // from the subtitling descriptor, e.g. "eng"
  std::vector<byte> packets;            // 188 byte TS packets of the subtitle PID, in stream order
  uint64_t firstPts = 0;                // PTS (90 kHz) of the first PES packet
  bool hasPts = false;
  size_t pesPackets = 0;                // PES packets that start in the collected packets
  uint64_t scannedBytes = 0;
};

// Scans at most 'maxBytes' of the file. Returns false with a reason in 'error' when there is no subtitle stream.
bool ExtractDvbSubtitles(const std::wstring& path, SubtitleStream& out, std::string& error, uint64_t maxBytes = 1ULL << 32);
