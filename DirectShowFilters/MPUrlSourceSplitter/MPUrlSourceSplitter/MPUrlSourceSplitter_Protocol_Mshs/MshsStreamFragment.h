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

#ifndef __MSHS_STREAM_FRAGMENT_DEFINED
#define __MSHS_STREAM_FRAGMENT_DEFINED

#include "CacheFileItem.h"

#define MSHS_STREAM_FRAGMENT_FLAG_NONE                                CACHE_FILE_ITEM_FLAG_NONE

#define MSHS_STREAM_FRAGMENT_FLAG_VIDEO                               (1 << (CACHE_FILE_ITEM_FLAG_LAST + 0))
#define MSHS_STREAM_FRAGMENT_FLAG_AUDIO                               (1 << (CACHE_FILE_ITEM_FLAG_LAST + 1))
#define MSHS_STREAM_FRAGMENT_FLAG_READY_FOR_PROCESSING                (1 << (CACHE_FILE_ITEM_FLAG_LAST + 2))
#define MSHS_STREAM_FRAGMENT_FLAG_CONTAINS_RECONSTRUCTED_HEADER       (1 << (CACHE_FILE_ITEM_FLAG_LAST + 3))

#define MSHS_STREAM_FRAGMENT_FLAG_LAST                                (CACHE_FILE_ITEM_FLAG_LAST + 4)

#define MSHS_STREAM_FRAGMENT_START_POSITION_NOT_SET                   -1

class CMshsStreamFragment : public CCacheFileItem
{
public:
  // creats new instance of CMshsStreamFragment class
  CMshsStreamFragment(HRESULT *result, int64_t fragmentTimestamp);
  CMshsStreamFragment(HRESULT *result, int64_t fragmentTimestamp, const wchar_t *url, unsigned int flags);
  // desctructor
  virtual ~CMshsStreamFragment(void);

  /* get methods */

  // gets fragment timestamp
  // @return : fragment timestamp
  int64_t GetFragmentTimestamp(void);

  // gets fragment start position within stream
  // @return : fragment start position within stream or MSHS_STREAM_FRAGMENT_START_POSITION_NOT_SET if not set
  int64_t GetFragmentStartPosition(void);

  // gets stream fragment URL
  // @return : stream fragment URL or NULL if error
  const wchar_t *GetUrl(void);

  /* set methods */

  // sets fragment start position
  // @param fragmentStartPosition : fragment start position to set
  void SetFragmentStartPosition(int64_t fragmentStartPosition);

  // sets if stream fragment is ready for processing
  // @param readyForProcessing : true if stream fragment is ready for processing
  // @param streamFragmentItemIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetReadyForProcessing(bool readyForProcessing, unsigned int streamFragmentItemIndex);

  /* other methods */

  // tests if fragment has set start position
  // @return : true if fragment has set start position, false otherwise
  bool IsSetFragmentStartPosition(void);

  // tests if fragment is ready for processing
  // @return : true if ready for processing, false otherwise
  bool IsReadyForProcessing(void);

  // tests if fragment is video fragment
  // @return : true if fragment is video, false otherwise
  bool IsVideo(void);

  // tests if fragment is audio fragment
  // @return : true if fragment is audio, false otherwise
  bool IsAudio(void);

  // tests if fragment contains reconstructed header
  // @return : true if contains reconstructed header, false otherwise
  bool ContainsReconstructedHeader(void);

protected:
  // stores url for stream fragment
  wchar_t *url;
  // holds fragment timestamp
  int64_t fragmentTimestamp;
  // holds fragment start position within stream
  int64_t fragmentStartPosition;

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