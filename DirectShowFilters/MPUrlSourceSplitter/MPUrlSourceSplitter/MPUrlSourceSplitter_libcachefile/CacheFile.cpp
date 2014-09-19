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

#include "StdAfx.h"

#include "CacheFile.h"

CCacheFile::CCacheFile(HRESULT *result)
{
  this->cacheFile = NULL;
  this->freeSpaces = NULL;

  this->Clear();
}

CCacheFile::~CCacheFile(void)
{
  this->Clear();
}

/* get methods */

const wchar_t *CCacheFile::GetCacheFile(void)
{
  return this->cacheFile;
}

unsigned int CCacheFile::GetLoadToMemoryTimeSpan(void)
{
  return this->loadToMemoryTimeSpan;
}

/* set methods */

bool CCacheFile::SetCacheFile(const wchar_t *cacheFile)
{
  SET_STRING_RETURN_WITH_NULL(this->cacheFile, cacheFile);
}

/* other methods */

void CCacheFile::Clear()
{
  if (this->GetCacheFile() != NULL)
  {
    DeleteFile(this->GetCacheFile());
  }

  FREE_MEM(this->cacheFile);
  FREE_MEM_CLASS(this->freeSpaces);

  this->loadToMemoryTimeSpan = CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT;
  this->cacheFileSize = 0;
}

bool CCacheFile::LoadItems(CCacheFileItemCollection *collection, unsigned int index, bool loadFromCacheFileAllowed)
{
  return this->LoadItems(collection, index, loadFromCacheFileAllowed, UINT_MAX);
}

bool CCacheFile::LoadItems(CCacheFileItemCollection *collection, unsigned int index, bool loadFromCacheFileAllowed, unsigned int maxItems)
{
  return this->LoadItems(collection, index, loadFromCacheFileAllowed, maxItems, CACHE_FILE_RELOAD_SIZE);
}

bool CCacheFile::LoadItems(CCacheFileItemCollection *collection, unsigned int index, bool loadFromCacheFileAllowed, unsigned int maxItems, unsigned int maxSize)
{
  HRESULT result = ((collection != NULL) && (index < collection->Count())) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    CCacheFileItem *item = collection->GetItem(index);
    CHECK_CONDITION_HRESULT(result, item->IsLoadedToMemory() || item->IsStoredToFile(), result, E_NOT_VALID_STATE);

    if (SUCCEEDED(result) && (!collection->GetItem(index)->IsLoadedToMemory()))
    {
      CHECK_CONDITION_HRESULT(result, loadFromCacheFileAllowed && (this->GetCacheFile() != NULL), result, E_NOT_VALID_STATE);

      if (SUCCEEDED(result))
      {
        // load items which are not in memory
        int64_t lastStoreFilePosition = item->GetCacheFilePosition() + (int64_t)item->GetLength();

        unsigned int totalItemsToReload = 1;
        unsigned int totalSizeToReload = item->GetLength();

        while (((index + totalItemsToReload) < collection->Count()) && (((maxItems == UINT_MAX) && (totalSizeToReload < maxSize)) || (maxItems != UINT_MAX)) && (totalItemsToReload < maxItems))
        {
          CCacheFileItem *itemToReload = collection->GetItem(index + totalItemsToReload);

          if (itemToReload->IsLoadedToMemory())
          {
            break;
          }
          else if (itemToReload->GetCacheFilePosition() == lastStoreFilePosition)
          {
            // item is in store file
            totalItemsToReload++;
            totalSizeToReload += itemToReload->GetLength();
            lastStoreFilePosition += (int64_t)(itemToReload->GetLength());
          }
          else
          {
            // this item doesn't start at specified position in file
            break;
          }
        }

        if (totalSizeToReload > 0)
        {
          // temporary buffer for data (from cache file)
          ALLOC_MEM_DEFINE_SET(buffer, unsigned char, totalSizeToReload, 0);
          CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            LARGE_INTEGER size;
            size.QuadPart = 0;

            // open or create file
            HANDLE hCacheFile = CreateFile(this->GetCacheFile(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
            CHECK_CONDITION_HRESULT(result, hCacheFile != INVALID_HANDLE_VALUE, result, E_NOT_VALID_STATE);

            if (SUCCEEDED(result))
            {
              LONG distanceToMoveLow = (LONG)(item->GetCacheFilePosition());
              LONG distanceToMoveHigh = (LONG)(item->GetCacheFilePosition() >> 32);
              LONG distanceToMoveHighResult = distanceToMoveHigh;
              DWORD setFileResult = SetFilePointer(hCacheFile, distanceToMoveLow, &distanceToMoveHighResult, FILE_BEGIN);
              if (setFileResult == INVALID_SET_FILE_POINTER)
              {
                CHECK_CONDITION_HRESULT(result, GetLastError() == NO_ERROR, result, E_NOT_VALID_STATE);
              }

              if (SUCCEEDED(result))
              {
                DWORD read = 0;

                CHECK_CONDITION_HRESULT(result, ReadFile(hCacheFile, buffer, totalSizeToReload, &read, NULL) != 0, result, E_FAIL);
                CHECK_CONDITION_HRESULT(result, read == totalSizeToReload, result, E_FAIL);
              }

              CloseHandle(hCacheFile);
              hCacheFile = INVALID_HANDLE_VALUE;
            }


            // mark all items as loaded to memory
            // for updating indexes use bulk update
            if (SUCCEEDED(result) && (totalItemsToReload != 0))
            {
              unsigned int position = 0;
              unsigned int loadedToMemoryTime = GetTickCount();

              for (unsigned int i = 0; i < totalItemsToReload; i++)
              {
                CCacheFileItem *itemToLoad = collection->GetItem(index + i);

                if (itemToLoad->GetBuffer()->InitializeBuffer(itemToLoad->GetLength()))
                {
                  itemToLoad->GetBuffer()->AddToBuffer(buffer + position, itemToLoad->GetLength());
                  itemToLoad->SetLoadedToMemoryTime(loadedToMemoryTime, UINT_MAX);
                }

                position += itemToLoad->GetLength();
              }

              CHECK_CONDITION_HRESULT(result, collection->UpdateIndexes(index, totalItemsToReload), result, E_FAIL);
            }
          }

          // clean-up buffer
          FREE_MEM(buffer);
        }
      }
    }
  }

  return SUCCEEDED(result);
}

bool CCacheFile::StoreItems(CCacheFileItemCollection *collection, unsigned int lastCheckTime)
{
  return this->StoreItems(collection, lastCheckTime, false, false);
}

bool CCacheFile::StoreItems(CCacheFileItemCollection *collection, unsigned int lastCheckTime, bool forceCleanUp, bool forceStoreToFile)
{
  HRESULT result = ((collection != NULL) && (this->GetCacheFile() != NULL)) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    if (collection->Count() > 0)
    {
      // open or create file
      HANDLE hCacheFile = CreateFile(this->GetCacheFile(), GENERIC_WRITE, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
      CHECK_CONDITION_HRESULT(result, hCacheFile != INVALID_HANDLE_VALUE, result, E_NOT_VALID_STATE); 

      if (SUCCEEDED(result))
      {
        unsigned int bufferPosition = 0;
        unsigned int bufferSize = CACHE_FILE_BUFFER_SIZE_DEFAULT;

        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
        CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          CIndexedCacheFileItemCollection *cleanUpFromMemoryStoredToFileLoadedToMemoryItems = new CIndexedCacheFileItemCollection(&result);
          CIndexedCacheFileItemCollection *cleanUpFromMemoryNotStoredToFileLoadedToMemoryItems = new CIndexedCacheFileItemCollection(&result);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = collection->GetCleanUpFromMemoryStoredToFileLoadedToMemoryItems(cleanUpFromMemoryStoredToFileLoadedToMemoryItems));
          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = collection->GetCleanUpFromMemoryNotStoredToFileLoadedToMemoryItems(cleanUpFromMemoryNotStoredToFileLoadedToMemoryItems));

          unsigned int i = 0;
          unsigned int timeSpan = this->GetLoadToMemoryTimeSpan();

          // clean up all items, which are stored to cache file and loaded to memory
          // for updating indexes use bulk update
          unsigned int startIndex = UINT_MAX;
          unsigned int count = 0;
          unsigned int lastIndex = UINT_MAX;

          for (unsigned int i = 0; (SUCCEEDED(result) && (i < cleanUpFromMemoryStoredToFileLoadedToMemoryItems->Count())); i++)
          {
            CIndexedCacheFileItem *indexedItem = cleanUpFromMemoryStoredToFileLoadedToMemoryItems->GetItem(i);
            CCacheFileItem *item = indexedItem->GetItem();

            if ((count != 0) && (lastIndex != UINT_MAX) && ((lastIndex + 1) != indexedItem->GetItemIndex()))
            {
              // force updating index, current indexed item is not exactly after last item
              CHECK_CONDITION_HRESULT(result, collection->UpdateIndexes(startIndex, count), result, E_FAIL);

              startIndex = UINT_MAX;
              lastIndex = UINT_MAX;
              count = 0;
            }

            if (forceCleanUp || ((lastCheckTime - item->GetLoadedToMemoryTime()) > timeSpan))
            {
              // release memory
              item->GetBuffer()->DeleteBuffer();
              item->SetLoadedToMemoryTime(CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET, UINT_MAX);

              CHECK_CONDITION_EXECUTE(startIndex == UINT_MAX, startIndex = indexedItem->GetItemIndex());
              count++;

              lastIndex = indexedItem->GetItemIndex();
            }
          }

          if (count != 0)
          {
            // we must update indexes
            CHECK_CONDITION_HRESULT(result, collection->UpdateIndexes(startIndex, count), result, E_FAIL);
          }

          i = 0;
          while (SUCCEEDED(result) && (i < cleanUpFromMemoryNotStoredToFileLoadedToMemoryItems->Count()))
          {
            unsigned int startPosition = i;

            // copy data to buffer
            while (SUCCEEDED(result) && (i < cleanUpFromMemoryNotStoredToFileLoadedToMemoryItems->Count()))
            {
              CIndexedCacheFileItem *indexedItem = cleanUpFromMemoryNotStoredToFileLoadedToMemoryItems->GetItem(i);
              CCacheFileItem *item = indexedItem->GetItem();

              if (forceStoreToFile || ((lastCheckTime - item->GetLoadedToMemoryTime()) > timeSpan))
              {
                // if item is not stored to file, store it to file
                if (item->GetLength() <= (bufferSize - bufferPosition))
                {
                  item->GetBuffer()->CopyFromBuffer(buffer + bufferPosition, item->GetLength());
                  bufferPosition += item->GetLength();
                }
                else
                {
                  // this item cannot fit in buffer
                  if (bufferPosition == 0)
                  {
                    // no data in buffer, buffer is too small
                    bufferSize = item->GetLength();
                    buffer = REALLOC_MEM(buffer, unsigned char, bufferSize);
                    result &= (buffer != NULL);
                  }
                  break;
                }
              }

              i++;
            }

            // if buffer position is not zero, flush buffer to file and continue
            if (SUCCEEDED(result) && (bufferPosition > 0))
            {
              LARGE_INTEGER size;
              size.QuadPart = this->cacheFileSize;

              // find suitable free space
              unsigned int freeSpaceIndex = (this->freeSpaces != NULL) ? this->freeSpaces->FindSuitableFreeSpace((int64_t)bufferPosition) : FREE_SPACE_NOT_FOUND;

              if (freeSpaceIndex != FREE_SPACE_NOT_FOUND)
              {
                size.QuadPart = this->freeSpaces->GetItem(freeSpaceIndex)->GetStart();
              }

              result &= (size.QuadPart != (-1));

              if (SUCCEEDED(result))
              {
                LONG distanceToMoveLow = (LONG)(size.QuadPart);
                LONG distanceToMoveHigh = (LONG)(size.QuadPart >> 32);
                LONG distanceToMoveHighResult = distanceToMoveHigh;
                DWORD setFileResult = SetFilePointer(hCacheFile, distanceToMoveLow, &distanceToMoveHighResult, FILE_BEGIN);
                if (setFileResult == INVALID_SET_FILE_POINTER)
                {
                  CHECK_CONDITION_HRESULT(result, GetLastError() == NO_ERROR, result, E_FAIL);
                }
              }

              if (SUCCEEDED(result))
              {
                // write prepared buffer to file
                DWORD written = 0;
                CHECK_CONDITION_HRESULT(result, WriteFile(hCacheFile, buffer, bufferPosition, &written, NULL) != 0, result, E_FAIL);
                CHECK_CONDITION_HRESULT(result, bufferPosition == written, result, E_FAIL);

                if (SUCCEEDED(result) && (this->freeSpaces != NULL) && (freeSpaceIndex != FREE_SPACE_NOT_FOUND))
                {
                  CHECK_CONDITION_HRESULT(result, this->freeSpaces->RemoveFreeSpace(freeSpaceIndex, (int64_t)bufferPosition), result, E_FAIL); 
                }

                if (SUCCEEDED(result))
                {
                  // mark all items as stored
                  // for updating indexes use bulk update
                  startIndex = UINT_MAX;
                  count = 0;
                  lastIndex = UINT_MAX;

                  for (unsigned int j = startPosition; (SUCCEEDED(result) && (j < i)); j++)
                  {
                    CIndexedCacheFileItem *indexedItem = cleanUpFromMemoryNotStoredToFileLoadedToMemoryItems->GetItem(j);
                    CCacheFileItem *item = indexedItem->GetItem();

                    if ((count != 0) && (lastIndex != UINT_MAX) && ((lastIndex + 1) != indexedItem->GetItemIndex()))
                    {
                      // force updating index, current indexed item is not exactly after last item
                      CHECK_CONDITION_HRESULT(result, collection->UpdateIndexes(startIndex, count), result, E_FAIL);

                      startIndex = UINT_MAX;
                      lastIndex = UINT_MAX;
                      count = 0;
                    }

                    if (forceStoreToFile || ((lastCheckTime - item->GetLoadedToMemoryTime()) > timeSpan))
                    {
                      item->SetCacheFilePosition(size.QuadPart, UINT_MAX);
                      size.QuadPart += item->GetLength();

                      CHECK_CONDITION_EXECUTE(startIndex == UINT_MAX, startIndex = indexedItem->GetItemIndex());
                      count++;

                      lastIndex = indexedItem->GetItemIndex();
                    }
                  }

                  if (count != 0)
                  {
                    // we must update indexes
                    CHECK_CONDITION_HRESULT(result, collection->UpdateIndexes(startIndex, count), result, E_FAIL);
                  }

                  // update cache file size
                  CHECK_CONDITION_EXECUTE(size.QuadPart > this->cacheFileSize, this->cacheFileSize = size.QuadPart);
                }

                bufferPosition = 0;
              }
            }
          }

          FREE_MEM_CLASS(cleanUpFromMemoryStoredToFileLoadedToMemoryItems);
          FREE_MEM_CLASS(cleanUpFromMemoryNotStoredToFileLoadedToMemoryItems);
        }

        FREE_MEM(buffer);

        CloseHandle(hCacheFile);
        hCacheFile = INVALID_HANDLE_VALUE;
      }
    }
  }

  return SUCCEEDED(result);
}

bool CCacheFile::RemoveItems(CCacheFileItemCollection *collection, unsigned int index, unsigned int count)
{
  if (this->freeSpaces == NULL)
  {
    HRESULT result = S_OK;
    this->freeSpaces = new CFreeSpaceCollection(&result);

    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->freeSpaces));
  }

  bool result = ((this->freeSpaces != NULL) && (collection != NULL) && ((index + count) <= collection->Count()));

  if (result)
  {
    unsigned int i = 0;
    
    while (result && (i < count))
    {
      int64_t start = -1;
      int64_t length = 0;

      while (result && (i < count))
      {
        CCacheFileItem *item = collection->GetItem(index + i);

        if (item->IsStoredToFile())
        {
          // we can free only space in cache file

          if (start == (-1))
          {
            start = item->GetCacheFilePosition();
            length = item->GetLength();
          }
          else
          {
            // check if item is continuous in occupied space
            // in another case free current continuous space and continue with current (unprocessed) item

            if ((start + length) == item->GetCacheFilePosition())
            {
              // continuous space to be released
              length += item->GetLength();
            }
            else
            {
              break;
            }
          }
        }

        i++;
      }

      if (length != 0)
      {
        result &= this->freeSpaces->AddFreeSpace(start, length);
      }
    }

    if (result)
    {
      // check if free spaces length is not equal to cache file size
      // in that case we can delete cache file

      int64_t size = 0;
      for (unsigned int i = 0; i < this->freeSpaces->Count(); i++)
      {
        size += this->freeSpaces->GetItem(i)->GetLength();
      }

      if (size == this->cacheFileSize)
      {
        if (DeleteFile(this->cacheFile))
        {
          this->cacheFileSize = 0;
          this->freeSpaces->Clear();
        }
      }
    }
  }

  return true;
}

/* protected methods */
