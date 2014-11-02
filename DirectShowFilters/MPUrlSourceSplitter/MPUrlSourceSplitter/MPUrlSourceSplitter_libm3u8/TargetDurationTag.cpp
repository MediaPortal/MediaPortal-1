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

#include "TargetDurationTag.h"

CTargetDurationTag::CTargetDurationTag(HRESULT *result)
  : CTag(result)
{
  this->targetDuration = TARGET_DURATION_NOT_SPECIFIED;
}

CTargetDurationTag::~CTargetDurationTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CTargetDurationTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03));
}

bool CTargetDurationTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CTargetDurationTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CTargetDurationTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

void CTargetDurationTag::Clear(void)
{
  __super::Clear();

  this->targetDuration = TARGET_DURATION_NOT_SPECIFIED;
}

bool CTargetDurationTag::ParseTag(unsigned int version)
{
  bool result = __super::ParseTag(version);

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_TARGET_DURATION) == 0);

    if (result)
    {
      this->targetDuration = CAttribute::GetDecimalInteger(this->tagContent);

      result &= (this->targetDuration != TARGET_DURATION_NOT_SPECIFIED);
    }
  }

  return result;
}

/* protected methods */

CItem *CTargetDurationTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CTargetDurationTag *item = new CTargetDurationTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CTargetDurationTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CTargetDurationTag *tag = dynamic_cast<CTargetDurationTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    tag->targetDuration = this->targetDuration;
  }

  return result;
}