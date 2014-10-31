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

#include "PlaylistItem.h"

CPlaylistItem::CPlaylistItem(HRESULT *result)
  : CItem(result)
{
  this->tags = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->tags = new CTagCollection(result);

    CHECK_POINTER_HRESULT(*result, this->tags, *result, E_OUTOFMEMORY);
  }
}

CPlaylistItem::~CPlaylistItem(void)
{
  FREE_MEM_CLASS(this->tags);
}

/* get methods */

CTagCollection *CPlaylistItem::GetTags(void)
{
  return this->tags;
}

/* set methods */

/* other methods */

bool CPlaylistItem::IsMediaPlaylistItem(unsigned int version)
{
  return true;
}

bool CPlaylistItem::IsMasterPlaylistItem(unsigned int version)
{
  return true;
}

void CPlaylistItem::Clear(void)
{
  __super::Clear();

  this->tags->Clear();
}

unsigned int CPlaylistItem::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int result = __super::Parse(buffer, length);

  if (result != 0)
  {
    this->flags |= ITEM_FLAG_PLAYLIST_ITEM;
  }

  return result;
}

bool CPlaylistItem::ParsePlaylistItem(CItem *item)
{
  bool result = __super::ParseItem(item);

  if (result)
  {
    result &= (!IsNullOrEmptyOrWhitespace(this->itemContent));

    this->flags |= result ? ITEM_FLAG_PLAYLIST_ITEM : ITEM_FLAG_NONE;
  }

  return result;
}

/* protected methods */

CItem *CPlaylistItem::CreateItem(void)
{
  HRESULT result = S_OK;
  CPlaylistItem *item = new CPlaylistItem(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CPlaylistItem::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CPlaylistItem *tag = dynamic_cast<CPlaylistItem *>(item);
  result &= (tag != NULL);

  if (result)
  {
    result &= tag->tags->Append(this->tags);
  }

  return result;
}