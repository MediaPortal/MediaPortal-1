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

#include "SubtitlesAttribute.h"

CSubtitlesAttribute::CSubtitlesAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->subtitlesGroupId = NULL;
}

CSubtitlesAttribute::~CSubtitlesAttribute(void)
{
  FREE_MEM(this->subtitlesGroupId);
}

/* get methods */

/* set methods */

/* other methods */

void CSubtitlesAttribute::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->subtitlesGroupId);
}

bool CSubtitlesAttribute::Parse(const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(name, value);

  if (result)
  {
    this->subtitlesGroupId = CAttribute::GetQuotedString(value);
    result &= (this->subtitlesGroupId != NULL);
  }

  return result;
}

/* protected methods */

CAttribute *CSubtitlesAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CSubtitlesAttribute *attribute = new CSubtitlesAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CSubtitlesAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CSubtitlesAttribute *subtitles = dynamic_cast<CSubtitlesAttribute *>(attribute);
  result &= (subtitles != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(subtitles->subtitlesGroupId, this->subtitlesGroupId, result);
  }

  return result;
}