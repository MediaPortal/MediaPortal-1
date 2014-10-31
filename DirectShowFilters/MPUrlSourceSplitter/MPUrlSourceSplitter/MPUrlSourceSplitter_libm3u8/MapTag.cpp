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

#include "MapTag.h"

CMapTag::CMapTag(HRESULT *result)
  : CTag(result)
{
}

CMapTag::~CMapTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CMapTag::IsMediaPlaylistItem(unsigned int version)
{
  return false;
}

bool CMapTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CMapTag::IsPlaylistItemTag(void)
{
  return true;
}

//bool CMapTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
//{
//  return false;
//}

bool CMapTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_MAP) == 0);

    if (result)
    {
      result &= this->ParseAttributes();
    }
  }

  return result;
}

/* protected methods */

CItem *CMapTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CMapTag *item = new CMapTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CMapTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CMapTag *tag = dynamic_cast<CMapTag *>(item);
  result &= (tag != NULL);

  return result;
}