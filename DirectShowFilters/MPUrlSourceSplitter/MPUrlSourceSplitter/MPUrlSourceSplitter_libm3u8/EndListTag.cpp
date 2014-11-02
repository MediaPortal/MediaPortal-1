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

#include "EndListTag.h"

CEndListTag::CEndListTag(HRESULT *result)
  : CTag(result)
{
}

CEndListTag::~CEndListTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CEndListTag::IsMediaPlaylistItem(unsigned int version)
{
  return true;
}

bool CEndListTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CEndListTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CEndListTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

bool CEndListTag::ParseTag(unsigned int version)
{
  bool result = __super::ParseTag(version);

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_END_LIST) == 0);
  }

  return result;
}

/* protected methods */

CItem *CEndListTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CEndListTag *item = new CEndListTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CEndListTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CEndListTag *tag = dynamic_cast<CEndListTag *>(item);
  result &= (tag != NULL);

  return result;
}