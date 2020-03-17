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

#include "PlaylistTypeTag.h"
#include "ErrorCodes.h"

CPlaylistTypeTag::CPlaylistTypeTag(HRESULT *result)
  : CTag(result)
{
}

CPlaylistTypeTag::~CPlaylistTypeTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CPlaylistTypeTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
}

bool CPlaylistTypeTag::IsMasterPlaylistItem(unsigned int version)
{
  return  (version == PLAYLIST_VERSION_03);
}

bool CPlaylistTypeTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CPlaylistTypeTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

HRESULT CPlaylistTypeTag::ParseTag(unsigned int version)
{
  HRESULT result = __super::ParseTag(version);
  CHECK_CONDITION_HRESULT(result, (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07), result, E_M3U8_NOT_SUPPORTED_TAG);

  if (SUCCEEDED(result))
  {
    // successful parsing of tag
    // compare it to our tag
    CHECK_CONDITION_HRESULT(result, wcscmp(this->tag, TAG_PLAYLIST_TYPE) == 0, result, E_M3U8_TAG_IS_NOT_OF_SPECIFIED_TYPE);
    CHECK_POINTER_HRESULT(result, this->tagContent, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);

    if (SUCCEEDED(result))
    {
      wchar_t *playlistType = CAttribute::GetEnumeratedString(this->tagContent);
      CHECK_POINTER_HRESULT(result, playlistType, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);

      if (SUCCEEDED(result))
      {
        this->flags |= (wcscmp(playlistType, PLAYLIST_TYPE_EVENT) == 0) ? PLAYLIST_TYPE_TAG_FLAG_EVENT : 0;
        this->flags |= (wcscmp(playlistType, PLAYLIST_TYPE_VOD) == 0) ? PLAYLIST_TYPE_TAG_FLAG_VOD : 0;
      }

      FREE_MEM(playlistType);
    }
  }

  return result;
}

/* protected methods */

CItem *CPlaylistTypeTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CPlaylistTypeTag *item = new CPlaylistTypeTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CPlaylistTypeTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CPlaylistTypeTag *tag = dynamic_cast<CPlaylistTypeTag *>(item);
  result &= (tag != NULL);

  return result;
}