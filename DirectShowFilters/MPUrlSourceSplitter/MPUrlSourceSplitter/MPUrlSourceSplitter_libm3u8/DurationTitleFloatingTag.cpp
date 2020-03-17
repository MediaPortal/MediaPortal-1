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

#include "DurationTitleFloatingTag.h"
#include "ItemCollection.h"
#include "PlaylistItemCollection.h"
#include "PlaylistItem.h"
#include "ErrorCodes.h"

CDurationTitleFloatingTag::CDurationTitleFloatingTag(HRESULT *result)
  : CTag(result)
{
  this->duration = DECIMAL_INTEGER_NOT_SPECIFIED;
  this->title = NULL;
}

CDurationTitleFloatingTag::~CDurationTitleFloatingTag(void)
{
  FREE_MEM(this->title);
}

/* get methods */

unsigned int CDurationTitleFloatingTag::GetDuration(void)
{
  return this->duration;
}

const wchar_t *CDurationTitleFloatingTag::GetTitle(void)
{
  return this->title;
}

/* set methods */

/* other methods */

bool CDurationTitleFloatingTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
}

bool CDurationTitleFloatingTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CDurationTitleFloatingTag::IsPlaylistItemTag(void)
{
  return true;
}

bool CDurationTitleFloatingTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  if ((version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
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

void CDurationTitleFloatingTag::Clear(void)
{
  __super::Clear();

  this->duration = DECIMAL_INTEGER_NOT_SPECIFIED;
  FREE_MEM(this->title);
}

HRESULT CDurationTitleFloatingTag::ParseTag(unsigned int version)
{
  HRESULT result = __super::ParseTag(version);
  CHECK_CONDITION_HRESULT(result, (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07), result, E_M3U8_NOT_SUPPORTED_TAG);

  if (SUCCEEDED(result))
  {
    // successful parsing of tag
    // compare it to our tag
    CHECK_CONDITION_HRESULT(result, wcscmp(this->tag, TAG_DURATION_TITLE_FLOATING) == 0, result, E_M3U8_TAG_IS_NOT_OF_SPECIFIED_TYPE);
    CHECK_POINTER_HRESULT(result, this->tagContent, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);

    if (SUCCEEDED(result))
    {
      unsigned int tagContentSize = wcslen(this->tagContent);

      // duration is required, must be present
      // DURATION_TITLE_SEPARATOR is sometimes missing, assume that all tag content is duration
      int index = IndexOf(this->tagContent, tagContentSize, DURATION_TITLE_FLOATING_SEPARATOR, DURATION_TITLE_FLOATING_SEPARATOR_LENGTH);

      if (SUCCEEDED(result))
      {
        wchar_t *durationValue = Substring(this->tagContent, 0, (index >= 0) ? (unsigned int)index : tagContentSize);
        CHECK_POINTER_HRESULT(result, durationValue, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);

        if (SUCCEEDED(result))
        {
          double temp = CAttribute::GetDecimalFloatingPoint(durationValue);
          CHECK_CONDITION_HRESULT(result, temp != DECIMAL_FLOATING_NOT_SPECIFIED, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);

          if (SUCCEEDED(result))
          {
            this->duration = (unsigned int)(temp * 1000);
          }
        }

        FREE_MEM(durationValue);
      }

      if (SUCCEEDED(result))
      {
        // title is optional
        this->title = (index == (-1)) ? Duplicate(L"") : (tagContentSize == ((unsigned int)index + DURATION_TITLE_FLOATING_SEPARATOR_LENGTH)) ? Duplicate(L"") : Substring(this->tagContent, (unsigned int)index + DURATION_TITLE_FLOATING_SEPARATOR_LENGTH, tagContentSize - (unsigned int)index - DURATION_TITLE_FLOATING_SEPARATOR_LENGTH);
        CHECK_POINTER_HRESULT(result, this->title, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);
      }
    }
  }

  return result;
}

/* protected methods */

CItem *CDurationTitleFloatingTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CDurationTitleFloatingTag *item = new CDurationTitleFloatingTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CDurationTitleFloatingTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CDurationTitleFloatingTag *tag = dynamic_cast<CDurationTitleFloatingTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    tag->duration = this->duration;
    SET_STRING_AND_RESULT_WITH_NULL(tag->title, this->title, result);
  }

  return result;
}