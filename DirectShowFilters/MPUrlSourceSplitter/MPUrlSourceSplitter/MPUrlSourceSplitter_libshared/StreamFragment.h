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

#ifndef __STREAM_FRAGMENT_DEFINED
#define __STREAM_FRAGMENT_DEFINED

#include "CacheFileItem.h"

#define STREAM_FRAGMENT_FLAG_NONE                                     CACHE_FILE_ITEM_FLAG_NONE

#define STREAM_FRAGMENT_FLAG_DOWNLOADED                               (1 << (CACHE_FILE_ITEM_FLAG_LAST + 0))
#define STREAM_FRAGMENT_FLAG_PROCESSED                                (1 << (CACHE_FILE_ITEM_FLAG_LAST + 1))
#define STREAM_FRAGMENT_FLAG_DISCONTINUITY                            (1 << (CACHE_FILE_ITEM_FLAG_LAST + 2))

#define STREAM_FRAGMENT_FLAG_LAST                                     (CACHE_FILE_ITEM_FLAG_LAST + 3)

#define STREAM_FRAGMENT_START_POSITION_NOT_SET                        -1

class CStreamFragment : public CCacheFileItem
{
public:
  CStreamFragment(HRESULT *result);
  virtual ~CStreamFragment(void);

  /* get methods */

  // gets fragment start position within stream
  // @return : fragment start position within stream or STREAM_FRAGMENT_START_POSITION_NOT_SET if not set
  virtual int64_t GetFragmentStartPosition(void);

  /* set methods */

  // sets fragment start position
  // @param fragmentStartPosition : fragment start position to set
  virtual void SetFragmentStartPosition(int64_t fragmentStartPosition);

  // sets if stream fragment is downloaded
  // @param downloaded : true if stream fragment is downloaded
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetDownloaded(bool downloaded, unsigned int streamFragmentIndex);

  // sets if stream fragment is processed
  // @param processed : true if stream fragment is processed
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetProcessed(bool processed, unsigned int streamFragmentIndex);

  // set discontinuity
  // @param discontinuity : true if discontinuity after data, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetDiscontinuity(bool discontinuity, unsigned int streamFragmentIndex);

  /* other methods */

  // tests if fragment has set start position
  // @return : true if fragment has set start position, false otherwise
  virtual bool IsSetFragmentStartPosition(void);

  // tests if fragment is downloaded
  // @return : true if downloaded, false otherwise
  virtual bool IsDownloaded(void);

  // tests if fragment is processed
  // @return : true if processed, false otherwise
  virtual bool IsProcessed(void);

  // tests if discontinuity is set
  // @return : true if discontinuity is set, false otherwise
  virtual bool IsDiscontinuity(void);

protected:
  // holds fragment start position within stream
  int64_t fragmentStartPosition;

  /* methods */

  // gets new instance of stream fragment
  // @return : new stream fragment instance or NULL if error
  virtual CFastSearchItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the stream fragment instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CFastSearchItem *item);
};

#endif