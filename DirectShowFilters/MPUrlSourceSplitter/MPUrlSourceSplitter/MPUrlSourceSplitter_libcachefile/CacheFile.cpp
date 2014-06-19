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

bool CCacheFile::LoadItems(CCacheFileItemCollection *collection, unsigned int index, bool loadFromCacheFileAllowed, bool cleanUpPreviousItems)
{
  bool result = ((collection != NULL) && (index < collection->Count()));

  if (result)
  {
    CCacheFileItem *item = collection->GetItem(index);
    result &= (item->IsLoadedToMemory() || item->IsStoredToFile());

    if (result && (!collection->GetItem(index)->IsLoadedToMemory()))
    {
      result &= (loadFromCacheFileAllowed && (this->GetCacheFile() != NULL));

      if (result)
      {
        for (unsigned int i = 0; (SUCCEEDED(result) && (cleanUpPreviousItems) && (i < index)); i++)
        {
          CCacheFileItem *item = collection->GetItem(i);

          if (item->IsStoredToFile() && item->IsLoadedToMemory())
          {
            // release memory
            item->GetBuffer()->DeleteBuffer();
            item->SetLoadedToMemoryTime(CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET);
          }
        }

        // load items which are not in memory
        
        int64_t lastStoreFilePosition = item->GetCacheFilePosition() + (int64_t)item->GetLength();

        unsigned int totalItemsToReload = 1;
        unsigned int totalSizeToReload = item->GetLength();

        while (((index + totalItemsToReload) < collection->Count()) && (totalSizeToReload < CACHE_FILE_RELOAD_SIZE))
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
          result &= (buffer != NULL);

          if (result)
          {
            LARGE_INTEGER size;
            size.QuadPart = 0;

            // open or create file
            HANDLE hCacheFile = CreateFile(this->GetCacheFile(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
            result &= (hCacheFile != INVALID_HANDLE_VALUE);

            if (result)
            {
              LONG distanceToMoveLow = (LONG)(item->GetCacheFilePosition());
              LONG distanceToMoveHigh = (LONG)(item->GetCacheFilePosition() >> 32);
              LONG distanceToMoveHighResult = distanceToMoveHigh;
              DWORD setFileResult = SetFilePointer(hCacheFile, distanceToMoveLow, &distanceToMoveHighResult, FILE_BEGIN);
              if (setFileResult == INVALID_SET_FILE_POINTER)
              {
                result &= (GetLastError() == NO_ERROR);
              }

              if (result)
              {
                DWORD read = 0;

                result &= (ReadFile(hCacheFile, buffer, totalSizeToReload, &read, NULL) != 0);
                result &= (read == totalSizeToReload);
              }

              CloseHandle(hCacheFile);
              hCacheFile = INVALID_HANDLE_VALUE;
            }

            if (result)
            {
              unsigned int position = 0;
              unsigned int loadedToMemoryTime = GetTickCount();

              for (unsigned int i = 0; i < totalItemsToReload; i++)
              {
                CCacheFileItem *itemToLoad = collection->GetItem(index + i);

                if (itemToLoad->GetBuffer()->InitializeBuffer(itemToLoad->GetLength()))
                {
                  itemToLoad->GetBuffer()->AddToBuffer(buffer + position, itemToLoad->GetLength());
                  itemToLoad->SetLoadedToMemoryTime(loadedToMemoryTime);
                }

                position += itemToLoad->GetLength();
              }
            }
          }

          // clean-up buffer
          FREE_MEM(buffer);
        }
      }
    }
  }

  return result;
}

bool CCacheFile::StoreItems(CCacheFileItemCollection *collection, unsigned int lastCheckTime)
{
  return this->StoreItems(collection, lastCheckTime, false);
}

bool CCacheFile::StoreItems(CCacheFileItemCollection *collection, unsigned int lastCheckTime, bool force)
{
  bool result = ((collection != NULL) && (this->GetCacheFile() != NULL));

  if (result)
  {
    if (collection->Count() > 0)
    {
      // open or create file
      HANDLE hCacheFile = CreateFile(this->GetCacheFile(), GENERIC_WRITE, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
      result &= (hCacheFile != INVALID_HANDLE_VALUE);

      if (result)
      {
        unsigned int bufferPosition = 0;
        unsigned int bufferSize = CACHE_FILE_BUFFER_SIZE_DEFAULT;

        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
        result &= (buffer != NULL);

        if (result)
        {
          unsigned int i = 0;
          unsigned int timeSpan = this->GetLoadToMemoryTimeSpan();

          while (result && (i < collection->Count()))
          {
            unsigned int startPosition = i;

            // copy data to buffer
            while (result && (i < collection->Count()))
            {
              CCacheFileItem *item = collection->GetItem(i);

              // skip items, which should not be removed from memory
              if (!item->IsNoCleanUpFromMemory())
              {
                if (item->IsStoredToFile() && (item->IsLoadedToMemory()) && 
                  (force || ((lastCheckTime - item->GetLoadedToMemoryTime()) > timeSpan)))
                {
                  // release memory
                  item->GetBuffer()->DeleteBuffer();
                  item->SetLoadedToMemoryTime(CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET);
                }

                if ((!item->IsStoredToFile()) && (item->IsLoadedToMemory()) &&
                  (force || ((lastCheckTime - item->GetLoadedToMemoryTime()) > timeSpan)))
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
              }

              i++;
            }

            // if buffer position is not zero, flush buffer to file and continue
            if (bufferPosition > 0)
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

              if (result)
              {
                LONG distanceToMoveLow = (LONG)(size.QuadPart);
                LONG distanceToMoveHigh = (LONG)(size.QuadPart >> 32);
                LONG distanceToMoveHighResult = distanceToMoveHigh;
                DWORD setFileResult = SetFilePointer(hCacheFile, distanceToMoveLow, &distanceToMoveHighResult, FILE_BEGIN);
                if (setFileResult == INVALID_SET_FILE_POINTER)
                {
                  result &= (GetLastError() == NO_ERROR);
                }
              }

              if (result)
              {
                // write prepared buffer to file
                DWORD written = 0;
                result &= (WriteFile(hCacheFile, buffer, bufferPosition, &written, NULL) != 0);
                result &= (bufferPosition == written);

                CHECK_CONDITION_EXECUTE_RESULT(result && (this->freeSpaces != NULL) && (freeSpaceIndex != FREE_SPACE_NOT_FOUND), this->freeSpaces->RemoveFreeSpace(freeSpaceIndex, (int64_t)bufferPosition), result);

                if (result)
                {
                  // mark all items as stored
                  for (unsigned int j = startPosition; j < i; j++)
                  {
                    CCacheFileItem *item = collection->GetItem(j);

                    // skip items, which should not be removed from memory
                    if (!item->IsNoCleanUpFromMemory())
                    {
                      if ((!item->IsStoredToFile()) && (item->IsLoadedToMemory()) &&
                        (force || ((lastCheckTime - item->GetLoadedToMemoryTime()) > timeSpan)))
                      {
                        item->SetCacheFilePosition(size.QuadPart);
                        size.QuadPart += item->GetLength();
                      }
                    }
                  }

                  // update cache file size
                  CHECK_CONDITION_EXECUTE(size.QuadPart > this->cacheFileSize, this->cacheFileSize = size.QuadPart);
                }

                bufferPosition = 0;
              }
            }
          }
        }

        FREE_MEM(buffer);
      }

      CloseHandle(hCacheFile);
      hCacheFile = INVALID_HANDLE_VALUE;
    }
  }

  return result;
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
