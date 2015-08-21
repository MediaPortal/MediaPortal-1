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

#include "HeaderTag.h"
#include "ErrorCodes.h"

CHeaderTag::CHeaderTag(HRESULT *result)
  : CTag(result)
{
}

CHeaderTag::~CHeaderTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CHeaderTag::IsMediaPlaylistItem(unsigned int version)
{
  return true;
}

bool CHeaderTag::IsMasterPlaylistItem(unsigned int version)
{
  return true;
}

bool CHeaderTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CHeaderTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

HRESULT CHeaderTag::ParseTag(unsigned int version)
{
  HRESULT result = __super::ParseTag(version);

  if (SUCCEEDED(result))
  {
    // successful parsing of tag
    // compare it to our tag
    CHECK_CONDITION_HRESULT(result, wcscmp(this->tag, TAG_HEADER) == 0, result, E_M3U8_TAG_IS_NOT_OF_SPECIFIED_TYPE);
  }

  return result;
}

/* protected methods */

CItem *CHeaderTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CHeaderTag *item = new CHeaderTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CHeaderTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CHeaderTag *tag = dynamic_cast<CHeaderTag *>(item);
  result &= (tag != NULL);

  return result;
}