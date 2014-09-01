/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __CHUNK_OFFSET_DEFINED
#define __CHUNK_OFFSET_DEFINED

#include <stdint.h>

class CChunkOffset
{
public:
  // initializes a new instance of CChunkOffset class
  CChunkOffset(HRESULT *result);

  // destructor
  ~CChunkOffset(void);

  /* get methods */

  // gets chunk offset
  // @return : chunk offset
  virtual uint64_t GetChunkOffset(void);

  /* set methods */

  // sets chunk offset
  // @param chunkOffset : chunk offset to set
  virtual void SetChunkOffset(uint64_t chunkOffset);

  /* other methods */

protected:

  // stores chunk offset
  // integer that gives the offset of the start of a chunk into its containing media file
  uint64_t chunkOffset;
};

#endif