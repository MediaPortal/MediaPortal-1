// Writes bit fields, Exp-Golomb codes and emulation prevention bytes: to build headers of elementary streams for the tests.
#pragma once

#include <cstdint>
#include <vector>

class BitWriter
{
public:
  // the lowest 'bits' bits of value, most significant first
  BitWriter& Put(uint64_t value, int bits)
  {
    for (int i = bits - 1; i >= 0; i--)
      PutBit(static_cast<int>((value >> i) & 1));
    return *this;
  }

  BitWriter& UE(uint64_t value)
  {
    const uint64_t code = value + 1;
    int length = 0;
    while ((code >> (length + 1)) != 0)
      length++;
    Put(0, length);
    Put(code, length + 1);
    return *this;
  }

  BitWriter& SE(int64_t value)
  {
    return UE(value > 0 ? static_cast<uint64_t>(value) * 2 - 1 : static_cast<uint64_t>(-value) * 2);
  }

  // the rbsp_trailing_bits: a one and zeros up to the byte boundary
  BitWriter& Trailing()
  {
    PutBit(1);
    while (m_bit != 0)
      PutBit(0);
    return *this;
  }

  // pads to a byte boundary with zeros
  BitWriter& Align()
  {
    while (m_bit != 0)
      PutBit(0);
    return *this;
  }

  const std::vector<uint8_t>& Bytes() const { return m_bytes; }

  // 00 00 03 inserted wherever 00 00 is followed by 00, 01, 02 or 03
  static std::vector<uint8_t> EmulationPrevention(const std::vector<uint8_t>& rbsp)
  {
    std::vector<uint8_t> out;
    int zeros = 0;
    for (uint8_t b : rbsp)
    {
      if (zeros >= 2 && b <= 3)
      {
        out.push_back(3);
        zeros = 0;
      }
      out.push_back(b);
      zeros = (b == 0) ? zeros + 1 : 0;
    }
    return out;
  }

private:
  void PutBit(int bit)
  {
    if (m_bit == 0)
      m_bytes.push_back(0);
    if (bit)
      m_bytes.back() = static_cast<uint8_t>(m_bytes.back() | (0x80 >> m_bit));
    m_bit = (m_bit + 1) & 7;
  }

  std::vector<uint8_t> m_bytes;
  int m_bit = 0;
};
