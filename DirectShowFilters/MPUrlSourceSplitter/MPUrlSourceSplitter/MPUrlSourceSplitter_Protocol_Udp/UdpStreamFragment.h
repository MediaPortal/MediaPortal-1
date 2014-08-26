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

#ifndef __UDP_STREAM_FRAGMENT_DEFINED
#define __UDP_STREAM_FRAGMENT_DEFINED

#include "CacheFileItem.h"

#define UDP_STREAM_FRAGMENT_FLAG_NONE                                 CACHE_FILE_ITEM_FLAG_NONE

#define UDP_STREAM_FRAGMENT_FLAG_LAST                                 (CACHE_FILE_ITEM_FLAG_LAST + 0)

class CUdpStreamFragment : public CCacheFileItem
{
public:
  CUdpStreamFragment(HRESULT *result);
  virtual ~CUdpStreamFragment(void);

  /* get methods */

  // gets the stream position where this stream fragment starts
  // @return : the stream position where this stream fragment starts
  int64_t GetStart(void);

  /* set methods */

  // sets the stream position where this stream fragment starts
  // @param position : the stream position where this stream fragment starts
  void SetStart(int64_t position);

  /* other methods */

protected:
  // start sample - byte position
  int64_t start;

  /* methods */

  // gets new instance of cache file item
  // @return : new cache file item instance or NULL if error
  virtual CFastSearchItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the cache file item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CFastSearchItem *item);
};

#endif