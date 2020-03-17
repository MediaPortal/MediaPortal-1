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

#ifndef __INDEXED_M3U8_SEGMENT_FRAGMENT_DEFINED
#define __INDEXED_M3U8_SEGMENT_FRAGMENT_DEFINED

#include "IndexedStreamFragment.h"
#include "M3u8StreamFragment.h"

#define INDEXED_M3U8_SEGMENT_FRAGMENT_FLAG_NONE                       INDEXED_STREAM_FRAGMENT_FLAG_NONE

#define INDEXED_M3U8_SEGMENT_FRAGMENT_FLAG_LAST                       (INDEXED_STREAM_FRAGMENT_FLAG_LAST + 0)

class CIndexedM3u8StreamFragment : public CIndexedStreamFragment
{
public:
  CIndexedM3u8StreamFragment(HRESULT *result, CM3u8StreamFragment *item, unsigned int index);
  virtual ~CIndexedM3u8StreamFragment(void);

  /* get methods */

  // gets M3U8 segment fragment
  // @return : M3U8 segment fragment
  virtual CM3u8StreamFragment *GetItem(void);

  /* set methods */

  /* other methods */

protected:

  /* methods */
};

#endif