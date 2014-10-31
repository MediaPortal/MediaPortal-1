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

#include "VersionTag.h"

CVersionTag::CVersionTag(HRESULT *result)
  : CTag(result)
{
  this->protocolVersion = PROTOCOL_VERSION_NOT_SPECIFIED;
}

CVersionTag::~CVersionTag(void)
{
}

/* get methods */

unsigned int CVersionTag::GetVersion(void)
{
  return this->protocolVersion;
}

/* set methods */

/* other methods */

bool CVersionTag::IsMediaPlaylistItem(unsigned int version)
{
  return true;
}

bool CVersionTag::IsMasterPlaylistItem(unsigned int version)
{
  return true;
}

bool CVersionTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CVersionTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

void CVersionTag::Clear(void)
{
  __super::Clear();

  this->protocolVersion = PROTOCOL_VERSION_NOT_SPECIFIED;
}

bool CVersionTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_VERSION) == 0);

    if (result)
    {
      this->protocolVersion = CAttribute::GetDecimalInteger(this->tagContent);

      result &= (this->protocolVersion != PROTOCOL_VERSION_NOT_SPECIFIED);
    }
  }

  return result;
}

/* protected methods */

CItem *CVersionTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CVersionTag *item = new CVersionTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CVersionTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CVersionTag *tag = dynamic_cast<CVersionTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    tag->protocolVersion = this->protocolVersion;
  }

  return result;
}