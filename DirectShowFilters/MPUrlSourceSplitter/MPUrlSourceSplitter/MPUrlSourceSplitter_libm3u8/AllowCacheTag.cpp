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

#include "AllowCacheTag.h"
#include "ErrorCodes.h"

CAllowCacheTag::CAllowCacheTag(HRESULT *result)
  : CTag(result)
{
}

CAllowCacheTag::~CAllowCacheTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CAllowCacheTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06));
}

bool CAllowCacheTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CAllowCacheTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CAllowCacheTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

HRESULT CAllowCacheTag::ParseTag(unsigned int version)
{
  HRESULT result = __super::ParseTag(version);
  CHECK_CONDITION_HRESULT(result, (version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06), result, E_M3U8_NOT_SUPPORTED_TAG);

  if (SUCCEEDED(result))
  {
    // successful parsing of tag
    // compare it to our tag
    CHECK_CONDITION_HRESULT(result, wcscmp(this->tag, TAG_ALLOW_CACHE) == 0, result, E_M3U8_TAG_IS_NOT_OF_SPECIFIED_TYPE);

    if (SUCCEEDED(result))
    {
      wchar_t *specification = CAttribute::GetEnumeratedString(this->tagContent);
      CHECK_POINTER_HRESULT(result, specification, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);

      if (SUCCEEDED(result))
      {
        this->flags |= (wcscmp(specification, ALLOW_CACHE_YES) == 0) ? ALLOW_CACHE_TAG_FLAG_YES : ALLOW_CACHE_TAG_FLAG_NONE;
        this->flags |= (wcscmp(specification, ALLOW_CACHE_NO) == 0) ? ALLOW_CACHE_TAG_FLAG_NO : ALLOW_CACHE_TAG_FLAG_NONE;
      }

      FREE_MEM(specification);
    }
  }

  return result;
}

/* protected methods */

CItem *CAllowCacheTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CAllowCacheTag *item = new CAllowCacheTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CAllowCacheTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CAllowCacheTag *tag = dynamic_cast<CAllowCacheTag *>(item);
  result &= (tag != NULL);

  return result;
}