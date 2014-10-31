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

#ifndef __TYPE_ATTRIBUTE_DEFINED
#define __TYPE_ATTRIBUTE_DEFINED

#include "Attribute.h"

#define TYPE_ATTRIBUTE_FLAG_NONE                                      ATTRIBUTE_FLAG_NONE

#define TYPE_ATTRIBUTE_FLAG_AUDIO                                     (1 << (ATTRIBUTE_FLAG_LAST + 0))
#define TYPE_ATTRIBUTE_FLAG_VIDEO                                     (1 << (ATTRIBUTE_FLAG_LAST + 1))
#define TYPE_ATTRIBUTE_FLAG_SUBTITLES                                 (1 << (ATTRIBUTE_FLAG_LAST + 2))
#define TYPE_ATTRIBUTE_FLAG_CLOSED_CAPTIONS                           (1 << (ATTRIBUTE_FLAG_LAST + 3))

#define TYPE_ATTRIBUTE_FLAG_LAST                                      (ATTRIBUTE_FLAG_LAST + 4)

#define TYPE_ATTRIBUTE_NAME                                           L"TYPE"

#define TYPE_ATTRIBUTE_AUDIO                                          L"AUDIO"
#define TYPE_ATTRIBUTE_VIDEO                                          L"VIDEO"
#define TYPE_ATTRIBUTE_SUBTITLES                                      L"SUBTITLES"
#define TYPE_ATTRIBUTE_CLOSED_CAPTIONS                                L"CLOSED-CAPTIONS"

class CTypeAttribute : public CAttribute
{
public:
  CTypeAttribute(HRESULT *result);
  virtual ~CTypeAttribute(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // parses name and value
  // @param name : the name of attribute
  // @param value : the value of attribute
  // @return : true if successful, false otherwise
  virtual bool Parse(const wchar_t *name, const wchar_t *value);

protected:

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