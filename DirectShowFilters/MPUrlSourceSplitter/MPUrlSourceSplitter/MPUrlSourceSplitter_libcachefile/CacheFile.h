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

#ifndef __CACHE_FILE_DEFINED
#define __CACHE_FILE_DEFINED

#include "CacheFileItemCollection.h"
#include "FreeSpaceCollection.h"

#define CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT                   2000
#define CACHE_FILE_BUFFER_SIZE_DEFAULT                                20971520   // 20 * 1024 * 1024
#define CACHE_FILE_RELOAD_SIZE                                        10485760             

class CCacheFile
{
public:
  CCacheFile(HRESULT *result);
  ~CCacheFile(void);

  /* get methods */

  // gets cache file name
  // @return : cache file name or NULL if error or not set
  const wchar_t *GetCacheFile(void);

  // gets load to memory time span
  // @return : load to memory time span
  unsigned int GetLoadToMemoryTimeSpan(void);

  /* set methods */

  // sets cache file name
  // @param cacheFile : the cache file name
  // @return : true if successful, false otherwise
  bool SetCacheFile(const wchar_t *cacheFile);

  /* other methods */

  // clears instance to its default state
  void Clear(void);

  // loads item with specified index to memory from cache file (if needed)
  // if item is loading from cache file, next items in collection are also loaded from cache file (until buffer size is reached)
  // @param collection : the collection of items to load from cache file
  // @param index : the index of item to load from cache file
  // @param loadFromCacheFileAllowed : true if load from cache file is allowed, false otherwise
  // @return : true if item successfully loaded (also in case if item is in memory), false otherwise
  bool LoadItems(CCacheFileItemCollection *collection, unsigned int index, bool loadFromCacheFileAllowed);

  // loads item with specified index to memory from cache file (if needed)
  // if item is loading from cache file, next items in collection are also loaded from cache file (until buffer size is reached)
  // @param collection : the collection of items to load from cache file
  // @param index : the index of item to load from cache file
  // @param loadFromCacheFileAllowed : true if load from cache file is allowed, false otherwise
  // @param maxItems : the maximum items to load from cache file
  // @return : true if item successfully loaded (also in case if item is in memory), false otherwise
  bool LoadItems(CCacheFileItemCollection *collection, unsigned int index, bool loadFromCacheFileAllowed, unsigned int maxItems);

  // loads item with specified index to memory from cache file (if needed)
  // if item is loading from cache file, next items in collection are also loaded from cache file (until buffer size is reached)
  // @param collection : the collection of items to load from cache file
  // @param index : the index of item to load from cache file
  // @param loadFromCacheFileAllowed : true if load from cache file is allowed, false otherwise
  // @param maxItems : the maximum items to load from cache file
  // @param maxSize : the maximum size of data to load from cache file
  // @return : true if item successfully loaded (also in case if item is in memory), false otherwise
  bool LoadItems(CCacheFileItemCollection *collection, unsigned int index, bool loadFromCacheFileAllowed, unsigned int maxItems, unsigned int maxSize);

  // stores unstored items to cache file
  // it also release from memory items which are loaded to memory more than specified time span
  // @param collection : the collection of items to store to cache file or release from memory
  // @param lastCheckTime : the last check time in ticks
  // @return : true if successful (all unstored items (which are longer in memory than specified time stamp) were stored, all items (which are longer in memory than specified time stamp) were released), false otherwise
  bool StoreItems(CCacheFileItemCollection *collection, unsigned int lastCheckTime);

  // stores unstored items to cache file
  // it also release from memory items which are loaded to memory more than specified timestamp
  // @param collection : the collection of items to store to cache file or release from memory
  // @param lastCheckTime : the last check time in ticks
  // @param forceCleanUp : true if all items should be released from memory, false otherwise
  // @param forceStoreToFile : true if all items should be stored to cache file, false otherwise
  // @return : true if successful (all unstored items (which are longer in memory than specified time stamp) were stored, all items (which are longer in memory than specified time stamp) were released), false otherwise
  bool StoreItems(CCacheFileItemCollection *collection, unsigned int lastCheckTime, bool forceCleanUp, bool forceStoreToFile);

  // removes stored items from cache file, used space is marked as free and can be possibly reused
  // @param collection : the collection of items to remove from cache file
  // @param index : the index of item to start removing from cache file
  // @param count : the count of items to remove from cache file
  // @return : true if successful, false otherwise
  bool RemoveItems(CCacheFileItemCollection *collection, unsigned int index, unsigned int count);

protected:
  // holds cache file
  wchar_t *cacheFile;
  // holds load to memory time span
  unsigned int loadToMemoryTimeSpan;
  // holds cache file size
  int64_t cacheFileSize;
  // holds free spaces
  CFreeSpaceCollection *freeSpaces;
};

#endif