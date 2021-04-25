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
#include "ErrorCodes.h"
#include <stdio.h>

CProgramDateTimeTag::CProgramDateTimeTag(HRESULT *result)
  : CTag(result)
{
}

CProgramDateTimeTag::~CProgramDateTimeTag(void)
{
}

/* get methods */

tm CProgramDateTimeTag::GetTime(void)
{
  return time;
}

/* set methods */

/* other methods */

bool CProgramDateTimeTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
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
  if ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
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

HRESULT CProgramDateTimeTag::ParseTag(unsigned int version)
{
  HRESULT result = __super::ParseTag(version);
  CHECK_CONDITION_HRESULT(result, (version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07), result, E_M3U8_NOT_SUPPORTED_TAG);

  if (SUCCEEDED(result))
  {
    // successful parsing of tag
    // compare it to our tag
    CHECK_CONDITION_HRESULT(result, wcscmp(this->tag, TAG_PROGRAM_DATE_TIME) == 0, result, E_M3U8_TAG_IS_NOT_OF_SPECIFIED_TYPE);
    //this->tagContent holds the datetime
    int y, M, d, h, m;
    float s;
    time = { 0 };
    int res = swscanf(this->tagContent, L"%d-%d-%dT%d:%d:%fZ", &y, &M, &d, &h, &m, &s);
    if (res == 6)
    {
      time.tm_year = y - 1900; // Year since 1900
      time.tm_mon = M - 1;     // 0-11
      time.tm_mday = d;        // 1-31
      time.tm_hour = h;        // 0-23
      time.tm_min = m;         // 0-59
      time.tm_sec = (int)s;    // 0-61 (0-60 in C++11)
    }
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

  if (result)
  {
    tag->time = this->time;
  }

  return result;
}