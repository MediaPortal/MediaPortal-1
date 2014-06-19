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

#include "CacheFileItem.h"

CCacheFileItem::CCacheFileItem(HRESULT *result)
  : CFlags()
{
  this->loadedToMemoryTime = CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET;
  this->cacheFilePosition = CACHE_FILE_ITEM_POSITION_NOT_SET;
  this->length = 0;
  this->buffer = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->buffer = new CLinearBuffer(result);
    CHECK_POINTER_HRESULT(*result, this->buffer, *result, E_OUTOFMEMORY);
  }
}

CCacheFileItem::~CCacheFileItem(void)
{
  FREE_MEM_CLASS(this->buffer);
}

/* get methods */

CLinearBuffer *CCacheFileItem::GetBuffer(void)
{
  return this->buffer;
}

unsigned int CCacheFileItem::GetLength(void)
{
  return (this->length == 0) ? this->GetBuffer()->GetBufferOccupiedSpace() : this->length;
}

int64_t CCacheFileItem::GetCacheFilePosition(void)
{
  return this->cacheFilePosition;
}

unsigned int CCacheFileItem::GetLoadedToMemoryTime(void)
{
  return this->loadedToMemoryTime;
}

/* set methods */

void CCacheFileItem::SetCacheFilePosition(int64_t position)
{
  this->cacheFilePosition = position;

  if (this->cacheFilePosition != CACHE_FILE_ITEM_POSITION_NOT_SET)
  {
    this->length = this->GetBuffer()->GetBufferOccupiedSpace();
    this->buffer->DeleteBuffer();
    this->loadedToMemoryTime = CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET;
  }
}

void CCacheFileItem::SetLoadedToMemoryTime(unsigned int time)
{
  this->loadedToMemoryTime = time;
}


void CCacheFileItem::SetNoCleanUpFromMemory(bool noCleanUpFromMemory)
{
  this->flags &= ~CACHE_FILE_ITEM_FLAG_NO_CLEAN_UP_FROM_MEMORY;
  this->flags |= (noCleanUpFromMemory) ? CACHE_FILE_ITEM_FLAG_NO_CLEAN_UP_FROM_MEMORY : CACHE_FILE_ITEM_FLAG_NONE;
}

/* other methods */

bool CCacheFileItem::IsStoredToFile(void)
{
  return (this->cacheFilePosition != CACHE_FILE_ITEM_POSITION_NOT_SET);
}

bool CCacheFileItem::IsLoadedToMemory(void)
{
  return (this->loadedToMemoryTime != CACHE_FILE_ITEM_LOAD_MEMORY_TIME_NOT_SET);
}

bool CCacheFileItem::IsNoCleanUpFromMemory(void)
{
  return this->IsSetFlags(CACHE_FILE_ITEM_FLAG_NO_CLEAN_UP_FROM_MEMORY);
}

CCacheFileItem *CCacheFileItem::Clone(void)
{
  CCacheFileItem *result = this->CreateItem();

  if (result != NULL)
  {
    if (!this->InternalClone(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
}

/* protected methods */


bool CCacheFileItem::InternalClone(CCacheFileItem *item)
{
  bool result = (item != NULL);

  if (result)
  {
    // free item buffer to avoid memory leak
    FREE_MEM_CLASS(item->buffer);

    item->flags = this->flags;
    item->buffer = this->buffer->Clone();
    item->loadedToMemoryTime = this->loadedToMemoryTime;
    item->cacheFilePosition = this->cacheFilePosition;
    item->length = this->length;

    result &= (item->buffer != NULL);
  }

  return result;
}