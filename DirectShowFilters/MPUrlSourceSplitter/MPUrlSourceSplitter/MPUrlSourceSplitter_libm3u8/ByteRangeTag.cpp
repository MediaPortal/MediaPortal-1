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

#include "ByteRangeTag.h"

CByteRangeTag::CByteRangeTag(HRESULT *result)
  : CTag(result)
{
  this->length = BYTE_RANGE_LENGTH_NOT_SPECIFIED;
  this->offset = BYTE_RANGE_OFFSET_NOT_SPECIFIED;
}

CByteRangeTag::~CByteRangeTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CByteRangeTag::IsMediaPlaylistItem(unsigned int version)
{
  return false;
}

bool CByteRangeTag::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

bool CByteRangeTag::IsPlaylistItemTag(void)
{
  return true;
}

//bool CByteRangeTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
//{
//}

void CByteRangeTag::Clear(void)
{
  __super::Clear();

  this->length = BYTE_RANGE_LENGTH_NOT_SPECIFIED;
  this->offset = BYTE_RANGE_OFFSET_NOT_SPECIFIED;
}

bool CByteRangeTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // successful parsing of tag
    // compare it to our tag
    result &= (wcscmp(this->tag, TAG_BYTE_RANGE) == 0);

    if (result)
    {
      int index = IndexOf(this->tagContent, BYTE_RANGE_OFFSET_SEPARATOR);

      if (index != (-1))
      {
        // byte range length and offset specified

        wchar_t *lengthValue = Substring(this->tagContent, 0, index);
        wchar_t *offsetValue = Substring(this->tagContent, index + BYTE_RANGE_OFFSET_SEPARATOR_LENGTH);

        this->length = CAttribute::GetDecimalInteger(lengthValue);
        this->offset = CAttribute::GetDecimalInteger(offsetValue);

        FREE_MEM(lengthValue);
        FREE_MEM(offsetValue);

        result = (this->length != BYTE_RANGE_LENGTH_NOT_SPECIFIED) ? result : 0;
        result = (this->offset != BYTE_RANGE_OFFSET_NOT_SPECIFIED) ? result : 0;
      }
      else
      {
        // only byte range length specified
        this->length = CAttribute::GetDecimalInteger(this->tagContent);

        result &= (this->length != BYTE_RANGE_LENGTH_NOT_SPECIFIED);
      }
    }
  }

  return result;
}

/* protected methods */

CItem *CByteRangeTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CByteRangeTag *item = new CByteRangeTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CByteRangeTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CByteRangeTag *tag = dynamic_cast<CByteRangeTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    tag->length = this->length;
    tag->offset = this->offset;
  }

  return result;
}