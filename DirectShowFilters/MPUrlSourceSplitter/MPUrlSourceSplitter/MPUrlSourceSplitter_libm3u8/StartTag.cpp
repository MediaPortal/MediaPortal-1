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

#include "StartTag.h"
#include "TimeOffsetAttribute.h"

CStartTag::CStartTag(HRESULT *result)
  : CTag(result)
{
}

CStartTag::~CStartTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CStartTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
}

bool CStartTag::IsMasterPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
}

bool CStartTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CStartTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

bool CStartTag::ParseTag(unsigned int version)
{
  bool result = __super::ParseTag(version);
  result &= ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_START) == 0);

    if (result)
    {
      result &= this->ParseAttributes(version);

      if (result)
      {
        if ((version == PLAYLIST_VERSION_06)  || (version == PLAYLIST_VERSION_07))
        {
          // TIME-OFFSET attribute is mandatory
          CTimeOffsetAttribute *timeOffset = dynamic_cast<CTimeOffsetAttribute *>(this->GetAttributes()->GetAttribute(TIME_OFFSET_ATTRIBUTE_NAME, true));
          result &= (timeOffset != NULL);
        }
      }
    }
  }

  return result;
}

/* protected methods */

CItem *CStartTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CStartTag *item = new CStartTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CStartTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CStartTag *tag = dynamic_cast<CStartTag *>(item);
  result &= (tag != NULL);

  return result;
}