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

#include "SessionDataTag.h"
#include "DataIdAttribute.h"
#include "ValueAttribute.h"
#include "UriAttribute.h"

CSessionDataTag::CSessionDataTag(HRESULT *result)
  : CTag(result)
{
}

CSessionDataTag::~CSessionDataTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CSessionDataTag::IsMediaPlaylistItem(unsigned int version)
{
  return false;
}

bool CSessionDataTag::IsMasterPlaylistItem(unsigned int version)
{
  return (version == PLAYLIST_VERSION_07);
}

bool CSessionDataTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CSessionDataTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

bool CSessionDataTag::ParseTag(unsigned int version)
{
  bool result = __super::ParseTag(version);
  result &= (version == PLAYLIST_VERSION_07);

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_SESSION_DATA) == 0);

    if (result)
    {
      result &= this->ParseAttributes(version);

      if (result)
      {
        if (version == PLAYLIST_VERSION_07)
        {
          // DATA-ID attribute is mandatory

          CDataIdAttribute *dataId = dynamic_cast<CDataIdAttribute *>(this->GetAttributes()->GetAttribute(DATA_ID_ATTRIBUTE_NAME, true));
          result &= (dataId != NULL);

          // there must be VALUE attribute or URI attribute, but not both

          CValueAttribute *valueAttr = dynamic_cast<CValueAttribute *>(this->GetAttributes()->GetAttribute(VALUE_ATTRIBUTE_NAME, true));
          CUriAttribute *uriAttr = dynamic_cast<CUriAttribute *>(this->GetAttributes()->GetAttribute(URI_ATTRIBUTE_NAME, true));

          result &= (!((valueAttr != NULL) && (uriAttr != NULL)));
          result &= ((valueAttr != NULL) || (uriAttr != NULL));
        }
      }
    }
  }

  return result;
}

/* protected methods */

CItem *CSessionDataTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CSessionDataTag *item = new CSessionDataTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CSessionDataTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CSessionDataTag *tag = dynamic_cast<CSessionDataTag *>(item);
  result &= (tag != NULL);

  return result;
}