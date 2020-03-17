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

#include "StreamFragment.h"

#define UDP_STREAM_FRAGMENT_FLAG_NONE                                 STREAM_FRAGMENT_FLAG_NONE

#define UDP_STREAM_FRAGMENT_FLAG_LAST                                 (STREAM_FRAGMENT_FLAG_LAST + 0)

class CUdpStreamFragment : public CStreamFragment
{
public:
  CUdpStreamFragment(HRESULT *result);
  virtual ~CUdpStreamFragment(void);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

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