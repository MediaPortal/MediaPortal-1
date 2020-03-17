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
#include "ErrorCodes.h"

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
  return ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07));
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

HRESULT CMediaSequenceTag::ParseTag(unsigned int version)
{
  HRESULT result = __super::ParseTag(version);
  CHECK_CONDITION_HRESULT(result, (version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07), result, E_M3U8_NOT_SUPPORTED_TAG);

  if (SUCCEEDED(result))
  {
    // successful parsing of tag
    // compare it to our tag
    CHECK_CONDITION_HRESULT(result, wcscmp(this->tag, TAG_MEDIA_SEQUENCE) == 0, result, E_M3U8_TAG_IS_NOT_OF_SPECIFIED_TYPE);
    CHECK_POINTER_HRESULT(result, this->tagContent, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);

    if (SUCCEEDED(result))
    {
      this->sequenceNumber = CAttribute::GetDecimalInteger(this->tagContent);
      CHECK_CONDITION_HRESULT(result, this->sequenceNumber != SEQUENCE_NUMBER_NOT_DEFINED, result, E_M3U8_INCOMPLETE_PLAYLIST_TAG);
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