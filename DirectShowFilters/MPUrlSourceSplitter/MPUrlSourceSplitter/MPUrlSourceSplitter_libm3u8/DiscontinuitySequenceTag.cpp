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

#include "DiscontinuitySequenceTag.h"

CDiscontinuitySequenceTag::CDiscontinuitySequenceTag(HRESULT *result)
  : CTag(result)
{
  this->discontinuitySequenceNumber = DISCONTINUITY_SEQUENCE_NUMBER_NOT_SPECIFIED;
}

CDiscontinuitySequenceTag::~CDiscontinuitySequenceTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */




bool CDiscontinuitySequenceTag::IsMediaPlaylistItem(unsigned int version)
{
  return ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
}

bool CDiscontinuitySequenceTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CDiscontinuitySequenceTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CDiscontinuitySequenceTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

void CDiscontinuitySequenceTag::Clear(void)
{
  __super::Clear();

  this->discontinuitySequenceNumber = DISCONTINUITY_SEQUENCE_NUMBER_NOT_SPECIFIED;
}

bool CDiscontinuitySequenceTag::ParseTag(unsigned int version)
{
  bool result = __super::ParseTag(version);
  result &= ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_DISCONTINUITY_SEQUENCE) == 0);

    if (result)
    {
      this->discontinuitySequenceNumber = CAttribute::GetDecimalInteger(this->tagContent);

      result &= (this->discontinuitySequenceNumber != DISCONTINUITY_SEQUENCE_NUMBER_NOT_SPECIFIED);
    }
  }

  return result;
}

/* protected methods */

CItem *CDiscontinuitySequenceTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CDiscontinuitySequenceTag *item = new CDiscontinuitySequenceTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CDiscontinuitySequenceTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CDiscontinuitySequenceTag *tag = dynamic_cast<CDiscontinuitySequenceTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    tag->discontinuitySequenceNumber = this->discontinuitySequenceNumber;
  }

  return result;
}