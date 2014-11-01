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

#include "ProgramDateTimeTag.h"
#include "ItemCollection.h"
#include "PlaylistItemCollection.h"
#include "PlaylistItem.h"

CProgramDateTimeTag::CProgramDateTimeTag(HRESULT *result)
  : CTag(result)
{
}

CProgramDateTimeTag::~CProgramDateTimeTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CProgramDateTimeTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02));
}

bool CProgramDateTimeTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CProgramDateTimeTag::IsPlaylistItemTag(void)
{
  return true;
}

bool CProgramDateTimeTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  if ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02))
  {
    // it is applied to exactly next playlist item
    bool applied = false;

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

    return applied;
  }
  else
  {
    // unknown playlist version
    return false;
  }
}

bool CProgramDateTimeTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_PROGRAM_DATE_TIME) == 0);

    // we do not interpret date and time
  }

  return result;
}

/* protected methods */

CItem *CProgramDateTimeTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CProgramDateTimeTag *item = new CProgramDateTimeTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CProgramDateTimeTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CProgramDateTimeTag *tag = dynamic_cast<CProgramDateTimeTag *>(item);
  result &= (tag != NULL);

  return result;
}