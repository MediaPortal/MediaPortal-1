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

#include "Item.h"

CItem::CItem(HRESULT *result)
  : CFlags()
{
  this->itemContent = NULL;
}

CItem::~CItem(void)
{
  FREE_MEM(this->itemContent);
}

/* get methods */

const wchar_t *CItem::GetItemContent(void)
{
  return this->itemContent;
}

/* set methods */

/* other methods */

bool CItem::IsPlaylistItem(void)
{
  return this->IsSetFlags(ITEM_FLAG_PLAYLIST_ITEM);
}

bool CItem::IsTag(void)
{
  return this->IsSetFlags(ITEM_FLAG_TAG);
}

bool CItem::IsComment(void)
{
  return this->IsSetFlags(ITEM_FLAG_COMMENT);
}

bool CItem::IsMediaPlaylistItem(unsigned int version)
{
  return false;
}

bool CItem::IsMasterPlaylistItem(unsigned int version)
{
  return false;
}

void CItem::Clear(void)
{
  this->flags = ITEM_FLAG_NONE;

  FREE_MEM(this->itemContent);
}

unsigned int CItem::Parse(const wchar_t *buffer, unsigned int length)
{
  this->Clear();
  unsigned int result = 0;

  if ((buffer != NULL) && (length > 0))
  {
    // get next end of line
    LineEnding endOfLine = GetEndOfLine(buffer, length);
    unsigned int lineSize = (endOfLine.position == (-1)) ? length : endOfLine.position;

    this->itemContent = Substring(buffer, 0, lineSize);
    result = (this->itemContent != NULL) ? (lineSize + endOfLine.size) : 0;
  }

  return result;
}

bool CItem::ParseItem(CItem *item)
{
  this->Clear();
  bool result = (item != NULL);

  if (result)
  {
    this->flags = item->flags;

    SET_STRING_AND_RESULT_WITH_NULL(this->itemContent, item->itemContent, result);
  }

  return result;
}

CItem *CItem::Clone(void)
{
  HRESULT result = S_OK;
  CItem *clone = this->CreateItem();
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, this->CloneInternal(clone), result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}

/* protected methods */

CItem *CItem::CreateItem(void)
{
  HRESULT result = S_OK;
  CItem *item = new CItem(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CItem::CloneInternal(CItem *item)
{
  bool result = (item != NULL);

  if (result)
  {
    item->flags = this->flags;
    SET_STRING_AND_RESULT_WITH_NULL(item->itemContent, this->itemContent, result);
  }

  return result;
}
