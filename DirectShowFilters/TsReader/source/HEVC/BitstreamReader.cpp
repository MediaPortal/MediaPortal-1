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


#include "BitstreamReader.h"

#include <climits>
#include <stdexcept>
#include <cassert>


BitstreamReader::BitstreamReader(const uint8_t *ptr, std::size_t size):
  m_ptr(ptr)
  ,m_size(size)
  ,m_posBase(0)
  ,m_posInBase(CHAR_BIT - 1)
{
}

std::size_t BitstreamReader::available()
{
  return (m_size - m_posBase -1) * CHAR_BIT + m_posInBase + 1;
}

std::size_t BitstreamReader::availableInNalU()
{
  std::size_t pos = m_posBase;
  if(m_posInBase)
    pos++;
  for(; pos<(m_size - 3); pos++)
  {
    bool naluFinded = m_ptr[pos] == 0 && m_ptr[pos+1] == 0 && m_ptr[pos+2] == 1;

    if(!naluFinded)
    {
      if(m_size - pos >= 4 && m_ptr[pos] == 0 && m_ptr[pos+1] == 0 && m_ptr[pos+2] == 0 && m_ptr[pos+3] == 1)
        naluFinded = true;
    }

    if(naluFinded)
    {
      return (pos - m_posBase - 1) * CHAR_BIT + m_posInBase + 1;
    }

  }

  return m_size;
}


bool BitstreamReader::getBit()
{
  if(m_posBase >= m_size)
    throw std::runtime_error("BitstreamReader: not enough data");

  bool res = (m_ptr[m_posBase] & (1 << m_posInBase)) != 0;

  m_posInBase--;

  if(m_posInBase > CHAR_BIT)
  {
    m_posInBase = CHAR_BIT-1;
    m_posBase++;

    if(m_posBase >= 2)
    {
      if(m_ptr[m_posBase - 2] == 0 && m_ptr[m_posBase - 1] == 0 && m_ptr[m_posBase] == 3)
        m_posBase++;
    }
  }

  return res;
}


uint32_t BitstreamReader::getBits(std::size_t num)
{
  assert(num <= 32);

  uint32_t result = 0;
  for(std::size_t i=0; i<num; i++)
    if(getBit())
      result |= 1 << (num - i - 1);

  return result;
}


void BitstreamReader::skipBits(std::size_t num)
{
  if(m_posBase >= 2)
  {
    if(m_ptr[m_posBase - 2] == 0 && m_ptr[m_posBase - 1] == 0 && m_ptr[m_posBase] == 3)
      m_posBase++;
  }

  uint32_t scipBytes = num / 8;


  while(scipBytes)
  {
    scipBytes--;
    m_posBase++;
    if(m_posBase >= 2)
    {
      if(m_ptr[m_posBase - 2] == 0 && m_ptr[m_posBase - 1] == 0 && m_ptr[m_posBase] == 3)
        m_posBase++;
    }
  }

  if(m_posInBase > num % 8)
    m_posInBase -= num % 8;
  else
  {
    m_posBase++;
    m_posInBase = m_posInBase - num % 8 + 8;
  }

}

uint32_t BitstreamReader::showBits(std::size_t num)
{
  assert(num <= 32);

  std::size_t posBasePrev = m_posBase;
  std::size_t posInBasePrev = m_posInBase;

  uint32_t result = 0;
  for(std::size_t i=0; i<num; i++)
    if(getBit())
      result |= 1 << (num - i - 1);

  m_posBase = posBasePrev;
  m_posInBase = posInBasePrev;

  return result;
}



uint32_t BitstreamReader::getGolombU()
{
  long numZeroBits = -1;
  for(long bit = 0; !bit; numZeroBits++)
    bit = getBit();

  if(numZeroBits >= 32)
    return 0;
//    throw std::range_error("Golomb: size of value more then 32 bits");

  return (1 << numZeroBits) - 1 + getBits(numZeroBits);
}



int32_t BitstreamReader::getGolombS()
{
  int32_t buf = getGolombU();

  if (buf & 1)
    buf = (buf + 1) >> 1;
  else
    buf = -(buf >> 1);

  return buf;
}

