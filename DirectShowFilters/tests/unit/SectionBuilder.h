// Builders for PSI/SI sections and their transport packets.
#pragma once

#include <windows.h>
#include <cstdint>
#include <cstring>
#include <vector>
#include "DvbUtil.h"
#include "TestPackets.h"

typedef std::vector<uint8_t> Bytes;

// A section with the "long" syntax: table_id, section_length, table_id_extension, version, section numbers, body and CRC-32.
inline Bytes MakeLongSection(int tableId, int extension, int version, const Bytes& body, int sectionNumber = 0,
                             int lastSectionNumber = 0, bool corruptCrc = false)
{
  Bytes s;
  const int length = 5 + static_cast<int>(body.size()) + 4;     // everything after the length field
  s.push_back(static_cast<uint8_t>(tableId));
  s.push_back(static_cast<uint8_t>(0xB0 | ((length >> 8) & 0x0F)));
  s.push_back(static_cast<uint8_t>(length & 0xFF));
  s.push_back(static_cast<uint8_t>(extension >> 8));
  s.push_back(static_cast<uint8_t>(extension));
  s.push_back(static_cast<uint8_t>(0xC1 | ((version & 0x1F) << 1)));
  s.push_back(static_cast<uint8_t>(sectionNumber));
  s.push_back(static_cast<uint8_t>(lastSectionNumber));
  s.insert(s.end(), body.begin(), body.end());
  const DWORD crc = crc32(reinterpret_cast<char*>(s.data()), static_cast<int>(s.size()));
  s.push_back(static_cast<uint8_t>(crc >> 24));
  s.push_back(static_cast<uint8_t>(crc >> 16));
  s.push_back(static_cast<uint8_t>(crc >> 8));
  s.push_back(static_cast<uint8_t>(crc ^ (corruptCrc ? 0x5A : 0)));
  return s;
}

// A section with the "short" syntax (TDT, TOT, ...): no CRC, section_syntax_indicator 0. The decoder wants 8 bytes at least.
inline Bytes MakeShortSection(int tableId, const Bytes& body)
{
  Bytes s;
  const int length = static_cast<int>(body.size());
  s.push_back(static_cast<uint8_t>(tableId));
  s.push_back(static_cast<uint8_t>(0x30 | ((length >> 8) & 0x0F)));
  s.push_back(static_cast<uint8_t>(length & 0xFF));
  s.insert(s.end(), body.begin(), body.end());
  return s;
}

// The packets that carry 'data' (one or more sections back to back, the first starting the payload): the first packet has the
// payload unit start flag and pointer_field 0, the rest is stuffed with 0xFF.
inline std::vector<TsPacket> PacketizeSections(unsigned pid, const Bytes& data, unsigned& counter)
{
  std::vector<TsPacket> packets;
  size_t pos = 0;
  bool first = true;
  while (pos < data.size())
  {
    TsPacket p = MakePacket(pid, 0x10 | (counter++ & 0x0F), first);
    size_t at = 4;
    if (first)
      p.data[at++] = 0;                                         // pointer_field
    const size_t n = (std::min<size_t>)(188 - at, data.size() - pos);
    memcpy(&p.data[at], &data[pos], n);
    pos += n;
    first = false;
    packets.push_back(p);
  }
  return packets;
}

inline std::vector<TsPacket> PacketizeSections(unsigned pid, const Bytes& data)
{
  unsigned counter = 0;
  return PacketizeSections(pid, data, counter);
}

inline Bytes Concat(const Bytes& a, const Bytes& b)
{
  Bytes r = a;
  r.insert(r.end(), b.begin(), b.end());
  return r;
}

struct Es
{
  int streamType;
  int pid;
  Bytes descriptors;
};

inline Bytes Descriptor(int tag, const Bytes& data)
{
  Bytes d;
  d.push_back(static_cast<uint8_t>(tag));
  d.push_back(static_cast<uint8_t>(data.size()));
  d.insert(d.end(), data.begin(), data.end());
  return d;
}

inline Bytes Iso639(const char* lang, int audioType = 0)
{
  Bytes b(lang, lang + 3);
  b.push_back(static_cast<uint8_t>(audioType));
  return Descriptor(0x0A, b);
}

// The body of a PMT (everything between the section header and the CRC)
inline Bytes PmtBody(int pcrPid, const Bytes& programInfo, const std::vector<Es>& streams)
{
  Bytes b;
  b.push_back(static_cast<uint8_t>(0xE0 | (pcrPid >> 8)));
  b.push_back(static_cast<uint8_t>(pcrPid));
  b.push_back(static_cast<uint8_t>(0xF0 | (programInfo.size() >> 8)));
  b.push_back(static_cast<uint8_t>(programInfo.size()));
  b.insert(b.end(), programInfo.begin(), programInfo.end());
  for (const Es& es : streams)
  {
    b.push_back(static_cast<uint8_t>(es.streamType));
    b.push_back(static_cast<uint8_t>(0xE0 | (es.pid >> 8)));
    b.push_back(static_cast<uint8_t>(es.pid));
    b.push_back(static_cast<uint8_t>(0xF0 | (es.descriptors.size() >> 8)));
    b.push_back(static_cast<uint8_t>(es.descriptors.size()));
    b.insert(b.end(), es.descriptors.begin(), es.descriptors.end());
  }
  return b;
}

// The body of an SDT (everything between the section header and the CRC): original_network_id and the services. Each service has
// one service descriptor (0x48) with provider and service name given as DVB text.
struct SdtService
{
  int serviceId;
  int runningStatus;
  int freeCa;
  int type;
  Bytes provider;
  Bytes name;
};

inline Bytes SdtBody(int originalNetworkId, const std::vector<SdtService>& services)
{
  Bytes b;
  b.push_back(static_cast<uint8_t>(originalNetworkId >> 8));
  b.push_back(static_cast<uint8_t>(originalNetworkId));
  b.push_back(0xFF);
  for (const SdtService& s : services)
  {
    Bytes d;
    d.push_back(static_cast<uint8_t>(s.type));
    d.push_back(static_cast<uint8_t>(s.provider.size()));
    d.insert(d.end(), s.provider.begin(), s.provider.end());
    d.push_back(static_cast<uint8_t>(s.name.size()));
    d.insert(d.end(), s.name.begin(), s.name.end());
    Bytes desc = Descriptor(0x48, d);
    b.push_back(static_cast<uint8_t>(s.serviceId >> 8));
    b.push_back(static_cast<uint8_t>(s.serviceId));
    b.push_back(0xFC | 0x03);                                      // EIT schedule and present/following flags
    b.push_back(static_cast<uint8_t>((s.runningStatus << 5) | (s.freeCa << 4) | (desc.size() >> 8)));
    b.push_back(static_cast<uint8_t>(desc.size()));
    b.insert(b.end(), desc.begin(), desc.end());
  }
  return b;
}

inline Bytes Text(const char* s) { return Bytes(s, s + strlen(s)); }
