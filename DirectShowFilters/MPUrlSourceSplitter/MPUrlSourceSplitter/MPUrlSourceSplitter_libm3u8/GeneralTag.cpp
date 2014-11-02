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

#include "GeneralTag.h"

CGeneralTag::CGeneralTag(HRESULT *result)
  : CItem(result)
{
  this->tag = NULL;
  this->tagContent = NULL;
}

CGeneralTag::~CGeneralTag(void)
{
  FREE_MEM(this->tag);
  FREE_MEM(this->tagContent);
}

/* get methods */

const wchar_t *CGeneralTag::GetTag(void)
{
  return this->tag;
}

const wchar_t *CGeneralTag::GetTagContent(void)
{
  return this->tagContent;
}

/* set methods */

/* other methods */

bool CGeneralTag::IsMediaPlaylistItem(unsigned int version)
{
  return true;
}

bool CGeneralTag::IsMasterPlaylistItem(unsigned int version)
{
  return true;
}

void CGeneralTag::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->tag);
  FREE_MEM(this->tagContent);
}

unsigned int CGeneralTag::Parse(const wchar_t *buffer, unsigned int length, unsigned int version)
{
  unsigned int result = __super::Parse(buffer, length, version);

  if (result != 0)
  {
    result = (result >= TAG_DIRECTIVE_SIZE) ? result : 0;

    if (result != 0)
    {
      if (wcsncmp(this->itemContent, TAG_DIRECTIVE_PREFIX, TAG_DIRECTIVE_SIZE) == 0)
      {
        unsigned int contentSize = wcslen(this->itemContent) - TAG_DIRECTIVE_SIZE;

        int index = IndexOf(this->itemContent + TAG_DIRECTIVE_SIZE, contentSize, TAG_SEPARATOR, TAG_SEPARATOR_SIZE);

        if (index != (-1))
        {
          // there is specified separator

          if (contentSize >= ((unsigned int)index + TAG_SEPARATOR_SIZE))
          {
            this->tag = Substring(this->itemContent + TAG_DIRECTIVE_SIZE, 0, index);
            this->tagContent = (contentSize == ((unsigned int)index + TAG_SEPARATOR_SIZE)) ? Duplicate(L"") : Substring(this->itemContent + TAG_DIRECTIVE_SIZE, (unsigned int)index + TAG_SEPARATOR_SIZE, contentSize - (unsigned int)index - TAG_SEPARATOR_SIZE);
          }

          result = (this->tag != NULL) ? result : 0;
          result = (this->tagContent != NULL) ? result : 0;
        }
        else
        {
          this->tag = Substring(this->itemContent, 0, contentSize);

          result = (this->tag != NULL) ? result : 0;
        }

        if (result != 0)
        {
          result = this->ParseTag(version) ? result : 0;
        }
      }
    }
  }

  return result;
}

bool CGeneralTag::ParseItem(CItem *item)
{
  bool result = __super::ParseItem(item);

  if (result)
  {
    result &= (wcsncmp(this->itemContent, TAG_DIRECTIVE_PREFIX, TAG_DIRECTIVE_SIZE) == 0);

    if (result)
    {
      unsigned int contentSize = wcslen(this->itemContent) - TAG_DIRECTIVE_SIZE;

      int index = IndexOf(this->itemContent + TAG_DIRECTIVE_SIZE, contentSize, TAG_SEPARATOR, TAG_SEPARATOR_SIZE);

      if (index != (-1))
      {
        // there is specified separator

        if (contentSize >= ((unsigned int)index + TAG_SEPARATOR_SIZE))
        {
          this->tag = Substring(this->itemContent + TAG_DIRECTIVE_SIZE, 0, index);
          this->tagContent = (contentSize == ((unsigned int)index + TAG_SEPARATOR_SIZE)) ? Duplicate(L"") : Substring(this->itemContent + TAG_DIRECTIVE_SIZE, (unsigned int)index + TAG_SEPARATOR_SIZE, contentSize - (unsigned int)index - TAG_SEPARATOR_SIZE);
        }

        result &= (this->tag != NULL);
        result &= (this->tagContent != NULL);
      }
      else
      {
        this->tag = Substring(this->itemContent + TAG_DIRECTIVE_SIZE, 0, contentSize);

        result &= (this->tag != NULL);
      }
    }
  }

  return result;
}

bool CGeneralTag::ParseGeneralTag(CGeneralTag *tag, unsigned int version)
{
  this->Clear();
  bool result = (tag != NULL);

  if (result)
  {
    this->flags = tag->flags;
    
    SET_STRING_AND_RESULT_WITH_NULL(this->itemContent, tag->itemContent, result);
    SET_STRING_AND_RESULT_WITH_NULL(this->tag, tag->tag, result);
    SET_STRING_AND_RESULT_WITH_NULL(this->tagContent, tag->tagContent, result);

    CHECK_CONDITION_EXECUTE(result, result &= this->ParseTag(version));
  }

  return result;
}

/* protected methods */

bool CGeneralTag::ParseTag(unsigned int version)
{
  return (this->tag != NULL);
}

CItem *CGeneralTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CGeneralTag *item = new CGeneralTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CGeneralTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CGeneralTag *tag = dynamic_cast<CGeneralTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(tag->tag, this->tag, result);
    SET_STRING_AND_RESULT_WITH_NULL(tag->tagContent, this->tagContent, result);
  }

  return result;
}
