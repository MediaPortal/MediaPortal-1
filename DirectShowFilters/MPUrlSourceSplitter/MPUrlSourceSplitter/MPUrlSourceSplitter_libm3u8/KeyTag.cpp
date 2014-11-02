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

#include "KeyTag.h"
#include "ItemCollection.h"
#include "PlaylistItemCollection.h"
#include "PlaylistItem.h"

CKeyTag::CKeyTag(HRESULT *result)
  : CTag(result)
{
}

CKeyTag::~CKeyTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CKeyTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03));
}

bool CKeyTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CKeyTag::IsPlaylistItemTag(void)
{
  return true;
}

bool CKeyTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  if ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03))
  {
    // it is applied to all playlist items after this tag until next key tag or end of playlist
    bool applied = this->ParseAttributes(version);

    for (unsigned int i = 1; (applied & (i < notProcessedItems->Count())); i++)
    {
      CPlaylistItem *playlistItem = dynamic_cast<CPlaylistItem *>(notProcessedItems->GetItem(i));
      CKeyTag *keyTag = dynamic_cast<CKeyTag *>(notProcessedItems->GetItem(i));

      if (playlistItem != NULL)
      {
        CTag *clone = (CTag *)this->Clone();
        applied &= (clone != NULL);

        CHECK_CONDITION_EXECUTE(applied, applied &= playlistItem->GetTags()->Add(clone));

        CHECK_CONDITION_EXECUTE(!applied, FREE_MEM_CLASS(clone));
      }

      if (keyTag != NULL)
      {
        break;
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

bool CKeyTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_KEY) == 0);
  }

  return result;
}

/* protected methods */

CItem *CKeyTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CKeyTag *item = new CKeyTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CKeyTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CKeyTag *tag = dynamic_cast<CKeyTag *>(item);
  result &= (tag != NULL);

  return result;
}