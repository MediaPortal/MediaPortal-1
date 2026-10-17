// Builders for transport stream test data.
#pragma once

#include <windows.h>
#include <cstdint>
#include <cstring>

struct TsPacket
{
  byte data[188];
};

// 188 byte TS packet with the given PID; 'control' is byte 3 (scrambling, adaption control, counter).
inline TsPacket MakePacket(unsigned pid, unsigned control, bool payloadUnitStart = false)
{
  TsPacket p;
  memset(p.data, 0xFF, sizeof(p.data));
  p.data[0] = 0x47;
  p.data[1] = static_cast<byte>((payloadUnitStart ? 0x40 : 0) | ((pid >> 8) & 0x1F));
  p.data[2] = static_cast<byte>(pid & 0xFF);
  p.data[3] = static_cast<byte>(control);
  return p;
}

// The 6 byte PCR field of an adaption field.
inline void EncodePcr(byte* out, uint64_t base, unsigned extension)
{
  out[0] = static_cast<byte>(base >> 25);
  out[1] = static_cast<byte>(base >> 17);
  out[2] = static_cast<byte>(base >> 9);
  out[3] = static_cast<byte>(base >> 1);
  out[4] = static_cast<byte>(((base & 1) << 7) | 0x7E | ((extension >> 8) & 1));
  out[5] = static_cast<byte>(extension & 0xFF);
}

// The 5 byte PTS/DTS field of a PES header. 'prefix' is 0x2 (PTS only), 0x3 (PTS when a DTS follows) or 0x1 (DTS).
inline void EncodeTimestamp(byte* out, unsigned prefix, uint64_t ts)
{
  out[0] = static_cast<byte>((prefix << 4) | (((ts >> 30) & 0x7) << 1) | 1);
  out[1] = static_cast<byte>(ts >> 22);
  out[2] = static_cast<byte>((((ts >> 15) & 0x7F) << 1) | 1);
  out[3] = static_cast<byte>(ts >> 7);
  out[4] = static_cast<byte>(((ts & 0x7F) << 1) | 1);
}
