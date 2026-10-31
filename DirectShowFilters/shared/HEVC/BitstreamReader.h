// Copyright (C) 2016 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

// ========================================================================
// The code in this file is derived from the 'HEVCESBrowser' project,
// a tool for analyzing HEVC(h265) bitstreams authored by 'virinext'.
// See https://github.com/virinext/hevcesbrowser
// and http://www.codeproject.com/Tips/896030/The-Structure-of-HEVC-Video
// Licensed under the GNU General Public License and 
// the Code Project Open License, http://www.codeproject.com/info/cpol10.aspx
// ========================================================================

#ifndef BITSTREAM_READER_H_
#define BITSTREAM_READER_H_

#include <cstddef>
#include <cstdint>

class BitstreamReader
{
public:
  BitstreamReader(const uint8_t *ptr, std::size_t size);
  bool getBit();
  uint32_t getBits(std::size_t num);
  void skipBits(std::size_t num);
  uint32_t showBits(std::size_t num);
  uint32_t getGolombU();
  int32_t getGolombS();

  std::size_t available();
  std::size_t availableInNalU();

private:
  const uint8_t              *m_ptr;
  std::size_t                 m_size;
  std::size_t                 m_posBase;
  std::size_t                 m_posInBase;  
};


#endif