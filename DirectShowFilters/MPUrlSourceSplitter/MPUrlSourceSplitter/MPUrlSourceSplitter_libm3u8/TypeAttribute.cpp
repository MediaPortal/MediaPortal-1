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

#include "TypeAttribute.h"

CTypeAttribute::CTypeAttribute(HRESULT *result)
  : CAttribute(result)
{
}

CTypeAttribute::~CTypeAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CTypeAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if ((version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
    {
      wchar_t *type = CAttribute::GetEnumeratedString(value);
      result &= (type != NULL);

      if (result)
      {
        this->flags |= (wcscmp(type, TYPE_ATTRIBUTE_AUDIO) == 0) ? TYPE_ATTRIBUTE_FLAG_AUDIO : TYPE_ATTRIBUTE_FLAG_NONE;
        this->flags |= (wcscmp(type, TYPE_ATTRIBUTE_VIDEO) == 0) ? TYPE_ATTRIBUTE_FLAG_VIDEO : TYPE_ATTRIBUTE_FLAG_NONE;

        if ((version == PLAYLIST_VERSION_05) || (version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
        {
          this->flags |= (wcscmp(type, TYPE_ATTRIBUTE_SUBTITLES) == 0) ? TYPE_ATTRIBUTE_FLAG_SUBTITLES : TYPE_ATTRIBUTE_FLAG_NONE;
        }

        if ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
        {
          this->flags |= (wcscmp(type, TYPE_ATTRIBUTE_CLOSED_CAPTIONS) == 0) ? TYPE_ATTRIBUTE_FLAG_CLOSED_CAPTIONS : TYPE_ATTRIBUTE_FLAG_NONE;
        }
      }

      FREE_MEM(type);
    }
    else
    {
      result = false;
    }
  }

  return result;
}

/* protected methods */

CAttribute *CTypeAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CTypeAttribute *attribute = new CTypeAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CTypeAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CTypeAttribute *type = dynamic_cast<CTypeAttribute *>(attribute);
  result &= (type != NULL);

  return result;
}