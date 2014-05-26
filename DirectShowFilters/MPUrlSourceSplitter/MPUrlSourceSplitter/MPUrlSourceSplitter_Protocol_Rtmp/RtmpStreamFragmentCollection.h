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

#ifndef __RTMP_STREAM_FRAGMENT_COLLECTION_DEFINED
#define __RTMP_STREAM_FRAGMENT_COLLECTION_DEFINED

#include "RtmpStreamFragment.h"
#include "CacheFileItemCollection.h"

class CRtmpStreamFragmentCollection : public CCacheFileItemCollection
{
public:
  CRtmpStreamFragmentCollection(void);
  virtual ~CRtmpStreamFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CRtmpStreamFragment *GetItem(unsigned int index);

  // gets first not downloaded stream fragment
  // @param requested : start index for searching
  // @return : index of first not downloaded stream fragment or UINT_MAX if not exists
  unsigned int GetFirstNotDownloadedStreamFragment(unsigned int start);

  // gets fragment with specified timestamp, starts searching from specified position
  // @param timestamp : the timestamp to find
  // @param position : index where seraching starts
  // @return : index of found fragment or UINT_MAX if not found
  unsigned int GetFragmentWithTimestamp(uint64_t timestamp, unsigned int position);

  /* set methods */

  /* other methods */

protected:

};

#endif