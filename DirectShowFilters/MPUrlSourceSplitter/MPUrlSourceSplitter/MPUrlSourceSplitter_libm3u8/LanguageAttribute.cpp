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

#include "LanguageAttribute.h"

CLanguageAttribute::CLanguageAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->language = NULL;
}

CLanguageAttribute::~CLanguageAttribute(void)
{
  FREE_MEM(this->language);
}

/* get methods */

/* set methods */

/* other methods */

void CLanguageAttribute::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->language);
}

bool CLanguageAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if ((version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
    {
      this->language = CAttribute::GetQuotedString(value);
      result &= (this->language != NULL);
    }
    else
    {
      result = false;
    }
  }

  return result;
}

/* protected methods */

CAttribute *CLanguageAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CLanguageAttribute *attribute = new CLanguageAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CLanguageAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CLanguageAttribute *language = dynamic_cast<CLanguageAttribute *>(attribute);
  result &= (language != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(language->language, this->language, result);
  }

  return result;
}