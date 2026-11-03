// Builds a small, valid MPEG-2 transport stream (PAT, PMT, PCR, video and audio PES) for the tests.
#pragma once

#include <windows.h>
#include <cstdint>
#include <cstring>
#include <string>
#include <vector>
#include "DvbUtil.h"

class TsStreamBuilder
{
public:
  static const int VideoPid = 0x101;
  static const int AudioPid = 0x102;
  static const int PmtPid = 0x100;

  // 'pesPerStream' PES packets of video and of audio, with PAT/PMT/PCR repeated in between.
  static std::vector<byte> Build(int pesPerStream, bool withPsi = true)
  {
    TsStreamBuilder b;
    for (int i = 0; i < pesPerStream; i++)
    {
      if (withPsi && (i % 4) == 0)
      {
        b.AddPsi();
        b.AddPcr(static_cast<uint64_t>(i) * 3600);
      }
      b.AddPes(VideoPid, 0xE0, VideoPayload(i == 0), static_cast<uint64_t>(i) * 3600 + 90000);
      b.AddPes(AudioPid, 0xC0, AudioPayload(), static_cast<uint64_t>(i) * 3600 + 90000);
    }
    return b.m_data;
  }

  static bool WriteFile(const std::wstring& path, const std::vector<byte>& data)
  {
    HANDLE f = CreateFileW(path.c_str(), GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (f == INVALID_HANDLE_VALUE)
      return false;
    DWORD written = 0;
    BOOL ok = ::WriteFile(f, data.data(), static_cast<DWORD>(data.size()), &written, NULL);
    CloseHandle(f);
    return ok && written == data.size();
  }

private:
  std::vector<byte> m_data;
  int m_counter[0x2000] = {};

  static std::vector<byte> VideoPayload(bool sequenceHeader)
  {
    std::vector<byte> v;
    if (sequenceHeader)
    {
      // MPEG-2 sequence header: 720 x 576, 4:3, 25 fps
      const byte seq[] = { 0x00, 0x00, 0x01, 0xB3, 0x2D, 0x02, 0x40, 0x23, 0x00, 0x7D, 0xE3, 0x00,
                           0x00, 0x00, 0x01, 0xB5, 0x14, 0x82, 0x00, 0x01, 0x00, 0x00 };
      v.insert(v.end(), seq, seq + sizeof(seq));
    }
    const byte picture[] = { 0x00, 0x00, 0x01, 0x00, 0x00, 0x0F, 0xFF, 0xF8 };
    v.insert(v.end(), picture, picture + sizeof(picture));
    v.insert(v.end(), 300, 0x55);
    return v;
  }

  static std::vector<byte> AudioPayload()
  {
    // MPEG-1 layer 2, 192 kbit/s, 48 kHz: frame header FF FD A4 04
    std::vector<byte> v = { 0xFF, 0xFD, 0xA4, 0x04 };
    v.insert(v.end(), 500, 0x00);
    return v;
  }

  byte* NewPacket(int pid, byte adaption)
  {
    m_data.resize(m_data.size() + 188, 0xFF);
    byte* p = &m_data[m_data.size() - 188];
    p[0] = 0x47;
    p[1] = static_cast<byte>((pid >> 8) & 0x1F);
    p[2] = static_cast<byte>(pid & 0xFF);
    p[3] = static_cast<byte>((adaption << 4) | (m_counter[pid]++ & 0x0F));
    return p;
  }

  void AddSection(int pid, const std::vector<byte>& section)
  {
    byte* p = NewPacket(pid, 1);
    p[1] |= 0x40;                              // payload unit start
    p[4] = 0;                                  // pointer field
    memcpy(&p[5], section.data(), section.size());
  }

  static void AppendCrc(std::vector<byte>& s)
  {
    DWORD crc = crc32(reinterpret_cast<char*>(s.data()), static_cast<int>(s.size()));
    s.push_back(static_cast<byte>(crc >> 24));
    s.push_back(static_cast<byte>(crc >> 16));
    s.push_back(static_cast<byte>(crc >> 8));
    s.push_back(static_cast<byte>(crc));
  }

  void AddPsi()
  {
    // PAT: program 1 -> PMT PID
    std::vector<byte> pat = { 0x00, 0xB0, 0x0D, 0x00, 0x01, 0xC1, 0x00, 0x00,
                              0x00, 0x01, static_cast<byte>(0xE0 | (PmtPid >> 8)), static_cast<byte>(PmtPid & 0xFF) };
    AppendCrc(pat);
    AddSection(0, pat);

    // PMT: PCR on the video PID, MPEG-2 video and MPEG-1 audio
    std::vector<byte> pmt = { 0x02, 0xB0, 0x17, 0x00, 0x01, 0xC1, 0x00, 0x00,
                              static_cast<byte>(0xE0 | (VideoPid >> 8)), static_cast<byte>(VideoPid & 0xFF), 0xF0, 0x00,
                              0x02, static_cast<byte>(0xE0 | (VideoPid >> 8)), static_cast<byte>(VideoPid & 0xFF), 0xF0, 0x00,
                              0x03, static_cast<byte>(0xE0 | (AudioPid >> 8)), static_cast<byte>(AudioPid & 0xFF), 0xF0, 0x00 };
    AppendCrc(pmt);
    AddSection(PmtPid, pmt);
  }

  void AddPcr(uint64_t base)
  {
    byte* p = NewPacket(VideoPid, 2);          // adaption field only
    p[4] = 183;
    p[5] = 0x10;                               // PCR flag
    p[6] = static_cast<byte>(base >> 25);
    p[7] = static_cast<byte>(base >> 17);
    p[8] = static_cast<byte>(base >> 9);
    p[9] = static_cast<byte>(base >> 1);
    p[10] = static_cast<byte>(((base & 1) << 7) | 0x7E);
    p[11] = 0;
  }

  void AddPes(int pid, byte streamId, const std::vector<byte>& payload, uint64_t pts)
  {
    std::vector<byte> pes = { 0x00, 0x00, 0x01, streamId, 0x00, 0x00, 0x80, 0x80, 0x05,
                              static_cast<byte>(0x21 | (((pts >> 30) & 7) << 1)), static_cast<byte>(pts >> 22),
                              static_cast<byte>(0x01 | (((pts >> 15) & 0x7F) << 1)), static_cast<byte>(pts >> 7),
                              static_cast<byte>(0x01 | ((pts & 0x7F) << 1)) };
    pes.insert(pes.end(), payload.begin(), payload.end());
    if ((streamId & 0xE0) == 0xC0)                // audio PES packets carry their length (video may use 0)
    {
      pes[4] = static_cast<byte>((pes.size() - 6) >> 8);
      pes[5] = static_cast<byte>((pes.size() - 6) & 0xFF);
    }

    size_t pos = 0;
    bool first = true;
    while (pos < pes.size())
    {
      size_t left = pes.size() - pos;
      if (left >= 184)
      {
        byte* p = NewPacket(pid, 1);
        if (first) p[1] |= 0x40;
        memcpy(&p[4], &pes[pos], 184);
        pos += 184;
      }
      else
      {
        // the last packet is padded with an adaption field of stuffing bytes
        byte* p = NewPacket(pid, 3);
        if (first) p[1] |= 0x40;
        size_t stuffing = 184 - left;           // adaption field length + length byte
        p[4] = static_cast<byte>(stuffing - 1);
        if (stuffing > 1)
        {
          p[5] = 0;
          memset(&p[6], 0xFF, stuffing - 2);
        }
        memcpy(&p[4 + stuffing], &pes[pos], left);
        pos += left;
      }
      first = false;
    }
  }
};
