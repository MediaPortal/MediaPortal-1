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

#pragma once

#ifndef __KEY_FORMAT_VERSIONS_ATTRIBUTE_DEFINED
#define __KEY_FORMAT_VERSIONS_ATTRIBUTE_DEFINED

#include "Attribute.h"

#define KEY_FORMAT_VERSIONS_ATTRIBUTE_FLAG_NONE                       ATTRIBUTE_FLAG_NONE

#define KEY_FORMAT_VERSIONS_ATTRIBUTE_FLAG_LAST                       (ATTRIBUTE_FLAG_LAST + 0)

#define KEY_FORMAT_VERSIONS_ATTRIBUTE_NAME                            L"KEYFORMATVERSIONS"

class CKeyFormatVersionsAttribute : public CAttribute
{
public:
  CKeyFormatVersionsAttribute(HRESULT *result);
  virtual ~CKeyFormatVersionsAttribute(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // clears current instance
  virtual void Clear(void);

  // parses name and value
  // @param name : the name of attribute
  // @param value : the value of attribute
  // @return : true if successful, false otherwise
  virtual bool Parse(const wchar_t *name, const wchar_t *value);

protected:

  // holds key format versions
  wchar_t *keyFormatVersions;

  /* methods */

  // creates attribute
  // @return : attribute or NULL if error
  virtual CAttribute *CreateAttribute(void);

  // deeply clones current instance to specified attribute
  // @param item : the attribute to clone current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CAttribute *attribute);
};

#endif