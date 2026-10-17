#include "TsSubtitleExtractor.h"

#include <algorithm>

namespace
{
  const size_t kPacket = 188;

  // Offset of the payload in a TS packet, or 0 when there is none
  size_t PayloadOffset(const byte* p)
  {
    const unsigned adaption = (p[3] >> 4) & 3;
    if ((adaption & 1) == 0)
      return 0;
    size_t offset = 4;
    if (adaption & 2)
      offset += 1 + p[4];
    return offset < kPacket ? offset : 0;
  }

  // Collects a PSI section that may span several packets
  struct SectionBuffer
  {
    std::vector<byte> data;
    bool active = false;

    // true when a complete section is in 'data'
    bool Add(const byte* p)
    {
      size_t offset = PayloadOffset(p);
      if (!offset)
        return false;
      const bool start = (p[1] & 0x40) != 0;
      if (start)
      {
        size_t pointer = p[offset];
        offset += 1 + pointer;
        if (offset >= kPacket)
          return false;
        data.clear();
        active = true;
      }
      if (!active)
        return false;
      data.insert(data.end(), p + offset, p + kPacket);
      if (data.size() < 3)
        return false;
      const size_t total = 3 + (((data[1] & 0x0F) << 8) | data[2]);
      if (data.size() < total)
        return false;
      data.resize(total);
      active = false;
      return true;
    }
  };

  bool ParsePat(const std::vector<byte>& s, int& pmtPid)
  {
    if (s.size() < 12 || s[0] != 0x00)
      return false;
    for (size_t i = 8; i + 4 <= s.size() - 4; i += 4)
    {
      int program = (s[i] << 8) | s[i + 1];
      int pid = ((s[i + 2] & 0x1F) << 8) | s[i + 3];
      if (program != 0)
      {
        pmtPid = pid;
        return true;
      }
    }
    return false;
  }

  bool ParsePmtForSubtitles(const std::vector<byte>& s, int& pid, std::string& language)
  {
    if (s.size() < 16 || s[0] != 0x02)
      return false;
    const size_t programInfo = ((s[10] & 0x0F) << 8) | s[11];
    size_t i = 12 + programInfo;
    const size_t end = s.size() - 4;                // without the CRC
    while (i + 5 <= end)
    {
      const int streamType = s[i];
      const int esPid = ((s[i + 1] & 0x1F) << 8) | s[i + 2];
      const size_t esInfo = ((s[i + 3] & 0x0F) << 8) | s[i + 4];
      size_t d = i + 5;
      const size_t dEnd = (std::min)(d + esInfo, end);
      while (d + 2 <= dEnd)
      {
        const int tag = s[d];
        const size_t len = s[d + 1];
        if (streamType == 0x06 && tag == 0x59 && len >= 8 && d + 2 + 3 <= dEnd)    // subtitling descriptor
        {
          pid = esPid;
          language.assign(reinterpret_cast<const char*>(&s[d + 2]), 3);
          return true;
        }
        d += 2 + len;
      }
      i += 5 + esInfo;
    }
    return false;
  }
}

bool ExtractDvbSubtitles(const std::wstring& path, SubtitleStream& out, std::string& error, uint64_t maxBytes)
{
  out = SubtitleStream();
  HANDLE file = CreateFileW(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);
  if (file == INVALID_HANDLE_VALUE)
  {
    error = "cannot open the file";
    return false;
  }

  SectionBuffer pat, pmt;
  int pmtPid = -1;
  std::vector<byte> buffer(kPacket * 20000);           // 3.7 MB per read
  std::vector<byte> carry;

  for (;;)
  {
    DWORD got = 0;
    if (!ReadFile(file, buffer.data(), static_cast<DWORD>(buffer.size()), &got, NULL) || got == 0)
      break;
    out.scannedBytes += got;

    std::vector<byte> chunk;
    chunk.swap(carry);
    chunk.insert(chunk.end(), buffer.begin(), buffer.begin() + got);

    size_t pos = 0;
    // find the first sync byte that has another one 188 bytes later
    while (pos + 2 * kPacket <= chunk.size() && !(chunk[pos] == 0x47 && chunk[pos + kPacket] == 0x47))
      pos++;
    for (; pos + kPacket <= chunk.size(); pos += kPacket)
    {
      const byte* p = &chunk[pos];
      if (p[0] != 0x47)
        continue;
      const int pid = ((p[1] & 0x1F) << 8) | p[2];

      if (pid == 0 && pmtPid < 0)
      {
        if (pat.Add(p))
          ParsePat(pat.data, pmtPid);
      }
      else if (pid == pmtPid && out.pid < 0)
      {
        if (pmt.Add(p))
          ParsePmtForSubtitles(pmt.data, out.pid, out.language);
      }
      else if (pid == out.pid)
      {
        out.packets.insert(out.packets.end(), p, p + kPacket);
        if (p[1] & 0x40)
        {
          out.pesPackets++;
          size_t offset = PayloadOffset(p);
          if (!out.hasPts && offset && offset + 14 <= kPacket && p[offset] == 0 && p[offset + 1] == 0 && p[offset + 2] == 1 &&
              (p[offset + 7] & 0x80))
          {
            const byte* t = p + offset + 9;
            out.firstPts = (static_cast<uint64_t>((t[0] >> 1) & 7) << 30) | (static_cast<uint64_t>(t[1]) << 22) |
                           (static_cast<uint64_t>(t[2] >> 1) << 15) | (static_cast<uint64_t>(t[3]) << 7) | (t[4] >> 1);
            out.hasPts = true;
          }
        }
      }
    }
    carry.assign(chunk.begin() + (pos < chunk.size() ? pos : chunk.size()), chunk.end());
    if (out.scannedBytes >= maxBytes)
      break;
  }
  CloseHandle(file);

  if (pmtPid < 0)
  {
    error = "no PAT found";
    return false;
  }
  if (out.pid < 0)
  {
    error = "the PMT has no stream with a subtitling descriptor";
    return false;
  }
  if (out.packets.empty())
  {
    error = "no packets of the subtitle PID";
    return false;
  }
  return true;
}
