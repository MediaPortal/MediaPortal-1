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

#include "MediaSequenceTag.h"

CMediaSequenceTag::CMediaSequenceTag(HRESULT *result)
  : CTag(result)
{
  this->sequenceNumber = SEQUENCE_NUMBER_NOT_DEFINED;
}

CMediaSequenceTag::~CMediaSequenceTag(void)
{
}

/* get methods */

unsigned int CMediaSequenceTag::GetSequenceNumber(void)
{
  return this->sequenceNumber;
}

/* set methods */

/* other methods */

bool CMediaSequenceTag::IsMediaPlaylistItem(unsigned int version)
{
  return (version == PLAYLIST_VERSION_01);
}

bool CMediaSequenceTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CMediaSequenceTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CMediaSequenceTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

void CMediaSequenceTag::Clear(void)
{
  __super::Clear();

  this->sequenceNumber = SEQUENCE_NUMBER_NOT_DEFINED;
}

bool CMediaSequenceTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_MEDIA_SEQUENCE) == 0);

    if (result)
    {
      this->sequenceNumber = CAttribute::GetDecimalInteger(this->tagContent);

      result &= (this->sequenceNumber != SEQUENCE_NUMBER_NOT_DEFINED);
    }
  }

  return result;
}

/* protected methods */

CItem *CMediaSequenceTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CMediaSequenceTag *item = new CMediaSequenceTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CMediaSequenceTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CMediaSequenceTag *tag = dynamic_cast<CMediaSequenceTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    tag->sequenceNumber = this->sequenceNumber;
  }

  return result;
}