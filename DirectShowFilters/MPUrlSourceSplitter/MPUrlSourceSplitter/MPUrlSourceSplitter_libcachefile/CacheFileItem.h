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

#ifndef __CACHE_FILE_ITEM_DEFINED
#define __CACHE_FILE_ITEM_DEFINED

#include "LinearBuffer.h"
#include "FastSearchItem.h"

#include <stdint.h>

#define CACHE_FILE_ITEM_FLAG_NONE                                     FAST_SEARCH_ITEM_FLAG_NONE

#define CACHE_FILE_ITEM_FLAG_NO_CLEAN_UP_FROM_MEMORY                  (1 << (FAST_SEARCH_ITEM_FLAG_LAST + 0))
#define CACHE_FILE_ITEM_FLAG_DOWNLOADED                               (1 << (FAST_SEARCH_ITEM_FLAG_LAST + 1))
#define CACHE_FILE_ITEM_FLAG_DISCONTINUITY                            (1 << (FAST_SEARCH_ITEM_FLAG_LAST + 2))

#define CACHE_FILE_ITEM_FLAG_LAST                                     (FAST_SEARCH_ITEM_FLAG_LAST + 3)

#define CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET                      0
#define CACHE_FILE_ITEM_POSITION_NOT_SET                              -1

class CCacheFileItem : public CFastSearchItem
{
public:
  CCacheFileItem(HRESULT *result);
  virtual ~CCacheFileItem(void);

  /* get methods */

  // gets buffer
  // @return : buffer or NULL if error
  virtual CLinearBuffer *GetBuffer(void);

  // gets length of data
  // @return : length of data
  virtual unsigned int GetLength(void);

  // gets position within cache file where item is stored
  // @return : position in cache file or CACHE_FILE_ITEM_POSITION_NOT_SET if not stored to cache file
  virtual int64_t GetCacheFilePosition(void);

  // gets time (in ms) when item was loaded into memory
  // @return : time (in ms) when item was loaded into memory or CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET if never
  virtual unsigned int GetLoadedToMemoryTime(void);

  /* set methods */

  // sets position within cache file
  // if item is stored than buffer is deleted
  // if cache file path is cleared (NULL) than buffer is created
  // @param position : the position of start of item within cache file or (-1) if item is in memory
  // @param cacheFileItemIndex : the index of cache file item (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetCacheFilePosition(int64_t position, unsigned int cacheFileItemIndex);

  // sets loaded from memory time (in ms)
  // @param time : the time (in ms, GetTickCount()) to set or CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET to unset
  // @param cacheFileItemIndex : the index of cache file item (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetLoadedToMemoryTime(unsigned int time, unsigned int cacheFileItemIndex);

  // sets no clean up from memory
  // @param noCleanUpFromMemory : true if no clean up from memory to set, false otherwise
  // @param cacheFileItemIndex : the index of cache file item (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetNoCleanUpFromMemory(bool noCleanUpFromMemory, unsigned int cacheFileItemIndex);

  // sets if cache file item is downloaded
  // @param downloaded : true if cache file item is downloaded
  // @param cacheFileItemIndex : the index of cache file item (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetDownloaded(bool downloaded, unsigned int cacheFileItemIndex);

  // set discontinuity
  // @param discontinuity : true if discontinuity after data, false otherwise
  // @param cacheFileItemIndex : the index of cache file item (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  virtual void SetDiscontinuity(bool discontinuity, unsigned int cacheFileItemIndex);

  /* other methods */

  // tests if item is stored to file
  // @return : true if item is stored to file, false otherwise
  virtual bool IsStoredToFile(void);

  // tests if item is loaded to memory
  // @return : true if item is loaded to memory, false otherwise
  virtual bool IsLoadedToMemory(void);

  // tests if no clean up from memory is set
  // @return : true if no clean up from memory is set, false otherwise
  virtual bool IsNoCleanUpFromMemory(void);

  // tests if discontinuity is set
  // @return : true if discontinuity is set, false otherwise
  virtual bool IsDiscontinuity(void);

  // tests if fragment is downloaded
  // @return : true if downloaded, false otherwise
  virtual bool IsDownloaded(void);

protected:
  // holds buffer data
  CLinearBuffer *buffer;
  // position in cache file
  int64_t cacheFilePosition;
  // load to memory time
  unsigned int loadedToMemoryTime;
  // holds length
  unsigned int length;

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