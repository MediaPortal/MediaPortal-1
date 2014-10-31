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

#include "DurationTitleTag.h"
#include "ItemCollection.h"
#include "PlaylistItemCollection.h"
#include "PlaylistItem.h"

CDurationTitleTag::CDurationTitleTag(HRESULT *result)
  : CTag(result)
{
  this->duration = DECIMAL_INTEGER_NOT_SPECIFIED;
  this->title = NULL;
}

CDurationTitleTag::~CDurationTitleTag(void)
{
  FREE_MEM(this->title);
}

/* get methods */

unsigned int CDurationTitleTag::GetDuration(void)
{
  return this->duration;
}

const wchar_t *CDurationTitleTag::GetTitle(void)
{
  return this->title;
}

/* set methods */

/* other methods */

bool CDurationTitleTag::IsMediaPlaylistItem(unsigned int version)
{
  return (version == PLAYLIST_VERSION_01);
}

bool CDurationTitleTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CDurationTitleTag::IsPlaylistItemTag(void)
{
  return true;
}

bool CDurationTitleTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  if (version == PLAYLIST_VERSION_01)
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

void CDurationTitleTag::Clear(void)
{
  __super::Clear();

  this->duration = DECIMAL_INTEGER_NOT_SPECIFIED;
  FREE_MEM(this->title);
}

bool CDurationTitleTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_DURATION_TITLE) == 0);

    if (result)
    {
      unsigned int tagContentSize = wcslen(this->tagContent);

      // duration is required, must be present
      int index = IndexOf(this->tagContent, tagContentSize, DURATION_TITLE_SEPARATOR, DURATION_TITLE_SEPARATOR_LENGTH);
      result &= (index != (-1));

      if (result)
      {
        wchar_t *durationValue = Substring(this->tagContent, 0, (unsigned int)index);
        result &= (durationValue != NULL);

        if (result)
        {
          this->duration = CAttribute::GetDecimalInteger(durationValue);
          result &= (this->duration != DECIMAL_INTEGER_NOT_SPECIFIED);
        }

        FREE_MEM(durationValue);
      }

      if (result)
      {
        // title is optional
        this->title = (tagContentSize == ((unsigned int)index + DURATION_TITLE_SEPARATOR_LENGTH)) ? Duplicate(L"") : Substring(this->tagContent, (unsigned int)index + DURATION_TITLE_SEPARATOR_LENGTH, tagContentSize - (unsigned int)index - DURATION_TITLE_SEPARATOR_LENGTH);
        result &= (this->title != NULL);
      }
    }
  }

  return result;
}

/* protected methods */

CItem *CDurationTitleTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CDurationTitleTag *item = new CDurationTitleTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CDurationTitleTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CDurationTitleTag *tag = dynamic_cast<CDurationTitleTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    tag->duration = this->duration;
    SET_STRING_AND_RESULT_WITH_NULL(tag->title, this->title, result);
  }

  return result;
}