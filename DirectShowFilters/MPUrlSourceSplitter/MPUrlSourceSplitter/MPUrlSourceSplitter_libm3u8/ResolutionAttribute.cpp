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

#include "ResolutionAttribute.h"

CResolutionAttribute::CResolutionAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->width = RESOLUTION_NOT_SPECIFIED;
  this->height = RESOLUTION_NOT_SPECIFIED;
}

CResolutionAttribute::~CResolutionAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

void CResolutionAttribute::Clear(void)
{
  __super::Clear();

  this->width = RESOLUTION_NOT_SPECIFIED;
  this->height = RESOLUTION_NOT_SPECIFIED;
}

bool CResolutionAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if ((version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
    {
      this->width = CAttribute::GetDecimalResolutionWidth(value);
      this->height = CAttribute::GetDecimalResolutionHeight(value);

      result &= (this->width != RESOLUTION_NOT_SPECIFIED);
      result &= (this->height != RESOLUTION_NOT_SPECIFIED);
    }
    else
    {
      result = false;
    }
  }

  return result;
}

/* protected methods */

CAttribute *CResolutionAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CResolutionAttribute *attribute = new CResolutionAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CResolutionAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CResolutionAttribute *resolution = dynamic_cast<CResolutionAttribute *>(attribute);
  result &= (resolution != NULL);

  if (result)
  {
    resolution->height = this->height;
    resolution->width = this->width;
  }

  return result;
}