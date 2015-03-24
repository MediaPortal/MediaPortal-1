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

#include "MediaTag.h"
#include "TypeAttribute.h"
#include "GroupIdAttribute.h"
#include "NameAttribute.h"

CMediaTag::CMediaTag(HRESULT *result)
  : CTag(result)
{
}

CMediaTag::~CMediaTag(void)
{
}
/* get methods */

/* set methods */

/* other methods */

bool CMediaTag::IsMediaPlaylistItem(unsigned int version)
{
  return false;
}

bool CMediaTag::IsMasterPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
}

bool CMediaTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CMediaTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

bool CMediaTag::ParseTag(unsigned int version)
{
  bool result = __super::ParseTag(version);
  result &= ((version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_MEDIA) == 0);

    if (result)
    {
      result &= this->ParseAttributes(version);

      if (result)
      {
        if ((version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
        {
          // TYPE attribute is mandatory
          CTypeAttribute *type = dynamic_cast<CTypeAttribute *>(this->GetAttributes()->GetAttribute(TYPE_ATTRIBUTE_NAME, true));
          result &= (type != NULL);

          // GROUP-ID attribute is mandatory
          CGroupIdAttribute *groupId = dynamic_cast<CGroupIdAttribute *>(this->GetAttributes()->GetAttribute(GROUP_ID_ATTRIBUTE_NAME, true));
          result &= (groupId != NULL);

          // NAME attribute is mandatory
          CNameAttribute *name = dynamic_cast<CNameAttribute *>(this->GetAttributes()->GetAttribute(NAME_ATTRIBUTE_NAME, true));
          result &= (name != NULL);
        }
      }
    }
  }

  return result;
}

/* protected methods */

CItem *CMediaTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CMediaTag *item = new CMediaTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CMediaTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CMediaTag *tag = dynamic_cast<CMediaTag *>(item);
  result &= (tag != NULL);

  return result;
}