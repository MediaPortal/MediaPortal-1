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

#include "StreamVariantTag.h"
#include "ItemCollection.h"
#include "PlaylistItemCollection.h"
#include "PlaylistItem.h"

CStreamVariantTag::CStreamVariantTag(HRESULT *result)
  : CTag(result)
{
}

CStreamVariantTag::~CStreamVariantTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CStreamVariantTag::IsMediaPlaylistItem(unsigned int version)
{
  return false;
}

bool CStreamVariantTag::IsMasterPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05));
}

bool CStreamVariantTag::IsPlaylistItemTag(void)
{
  return true;
}

bool CStreamVariantTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  if ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05))
  {
    // it is applied to exactly next playlist item
    bool applied = false;

    if (this->ParseAttributes(version))
    {
      for (unsigned int i = 0; i < notProcessedItems->Count(); i++)
      {
        CPlaylistItem *playlistItem = dynamic_cast<CPlaylistItem *>(notProcessedItems->GetItem(i));

        if (playlistItem != NULL)
        {
          CTag *clone = (CTag *)this->Clone();
          if (clone != NULL)
          {
            applied |= playlistItem->GetTags()->Add(clone);
          }

          CHECK_CONDITION_EXECUTE(!applied, FREE_MEM_CLASS(clone));
          break;
        }
      }
    }

    return applied;
  }
  else
  {
    // unknown playlist version
    return false;
  }
}

bool CStreamVariantTag::ParseTag(unsigned int version)
{
  bool result = __super::ParseTag(version);
  result &= ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05));

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_STREAM_VARIANT) == 0);
  }

  return result;
}

/* protected methods */

CItem *CStreamVariantTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CStreamVariantTag *item = new CStreamVariantTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CStreamVariantTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CStreamVariantTag *tag = dynamic_cast<CStreamVariantTag *>(item);
  result &= (tag != NULL);

  return result;
}