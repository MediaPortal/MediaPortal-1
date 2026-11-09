// A copy of some bytes placed so that the last byte is the last byte before a page without access rights:
// any read or write past the end of the buffer is an access violation (and so a failing test).
#pragma once

#include <windows.h>
#include <cstdint>
#include <cstring>
#include <vector>

class GuardedBuffer
{
public:
  explicit GuardedBuffer(size_t size, uint8_t fill = 0) : m_size(size)
  {
    SYSTEM_INFO si;
    GetSystemInfo(&si);
    const size_t page = si.dwPageSize;
    const size_t pages = (size + page - 1) / page;
    m_base = static_cast<uint8_t*>(VirtualAlloc(NULL, (pages + 1) * page, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE));
    DWORD old = 0;
    VirtualProtect(m_base + pages * page, page, PAGE_NOACCESS, &old);
    m_data = m_base + pages * page - size;
    memset(m_data, fill, size);
  }

  explicit GuardedBuffer(const std::vector<uint8_t>& bytes) : GuardedBuffer(bytes.size())
  {
    if (!bytes.empty())
      memcpy(m_data, bytes.data(), bytes.size());
  }

  ~GuardedBuffer() { VirtualFree(m_base, 0, MEM_RELEASE); }
  GuardedBuffer(const GuardedBuffer&) = delete;
  GuardedBuffer& operator=(const GuardedBuffer&) = delete;

  uint8_t* data() { return m_data; }
  const uint8_t* data() const { return m_data; }
  size_t size() const { return m_size; }

private:
  uint8_t* m_base;
  uint8_t* m_data;
  size_t m_size;
};
