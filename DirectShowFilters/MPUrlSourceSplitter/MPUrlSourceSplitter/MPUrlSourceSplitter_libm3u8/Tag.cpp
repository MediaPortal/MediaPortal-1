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

#include "Tag.h"
#include "AttributeFactory.h"

CTag::CTag(HRESULT *result)
  : CGeneralTag(result)
{
  this->attributes = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->attributes = new CAttributeCollection(result);

    CHECK_POINTER_HRESULT(*result, this->attributes, *result, E_OUTOFMEMORY);
  }
}

CTag::~CTag(void)
{
  FREE_MEM_CLASS(this->attributes);
}

/* get methods */

CAttributeCollection *CTag::GetAttributes(void)
{
  return this->attributes;
}

/* set methods */

/* other methods */

bool CTag::IsMediaPlaylistItem(unsigned int version)
{
  return true;
}

bool CTag::IsMasterPlaylistItem(unsigned int version)
{
  return true;
}

bool CTag::IsPlaylistItemTag(void)
{
  return false;
}

bool CTag::ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems)
{
  return false;
}

void CTag::Clear(void)
{
  __super::Clear();

  this->attributes->Clear();
}

/* protected methods */

bool CTag::ParseTag(void)
{
  bool result = __super::ParseTag();

  if (result)
  {
    // tag only when contains TAG_PREFIX
    result &= (wcsncmp(this->tag, TAG_PREFIX, TAG_PREFIX_SIZE) == 0);
  }

  if (result)
  {
    this->flags |= ITEM_FLAG_TAG;
  }

  return result;
}

bool CTag::ParseAttributes(unsigned int version)
{
  HRESULT result = S_OK;
  CAttributeFactory *factory = new CAttributeFactory(&result);
  CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    // split tag content into attributes and create attributes
    unsigned int processed = 0;
    unsigned int tagContentLength = wcslen(this->tagContent);

    // we need to mask quoted strings, because they can have attribute separator
    wchar_t *tagContentMasked = Duplicate(this->tagContent);
    CHECK_POINTER_HRESULT(result, tagContentMasked, result, E_OUTOFMEMORY);

    bool masking = false;
    for (unsigned int i = 0; i < tagContentLength; i++)
    {
      if (tagContentMasked[i] == L'"')
      {
        masking = !masking;
      }
      else if (masking)
      {
        tagContentMasked[i] = L' ';
      }
    }
    
    while (SUCCEEDED(result) && (processed < tagContentLength))
    {
      int index = IndexOf(tagContentMasked + processed, tagContentLength - processed, ATTRIBUTE_SEPARATOR, ATTRIBUTE_SEPARATOR_LENGTH);
      index = (index == (-1)) ? (tagContentLength - processed) : index;

      CAttribute *attribute = factory->CreateAttribute(version, this->tagContent + processed, index);
      CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->attributes->Add(attribute), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));

      processed += index + ATTRIBUTE_SEPARATOR_LENGTH;
    }

    FREE_MEM(tagContentMasked);
  }

  FREE_MEM_CLASS(factory);

  return SUCCEEDED(result);
}

CItem *CTag::CreateItem(void)
{
  HRESULT result = S_OK;
  CTag *item = new CTag(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CTag::CloneInternal(CItem *item)
{
  bool result = __super::CloneInternal(item);
  CTag *tag = dynamic_cast<CTag *>(item);
  result &= (tag != NULL);

  if (result)
  {
    result &= tag->attributes->Append(this->attributes);
  }

  return result;
}