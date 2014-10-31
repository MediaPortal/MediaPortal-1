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

#include "CommentTag.h"

CCommentTag::CCommentTag(HRESULT *result)
  : CGeneralTag(result)
{
}

CCommentTag::~CCommentTag(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CCommentTag::IsMediaPlaylistItem(unsigned int version)
{
  return true;
}

bool CCommentTag::IsMasterPlaylistItem(unsigned int version)
{
  return true;
}

/* protected methods */

bool CCommentTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    this->flags |= ITEM_FLAG_COMMENT;
  }

  return result;
}

CItem *CCommentTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CCommentTag *item = new CCommentTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CCommentTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CCommentTag *tag = dynamic_cast<CCommentTag *>(item);
  result &= (tag != NULL);

  return result;
}