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

#ifndef __METHOD_ATTRIBUTE_DEFINED
#define __METHOD_ATTRIBUTE_DEFINED

#include "Attribute.h"

#define METHOD_ATTRIBUTE_FLAG_NONE                                    ATTRIBUTE_FLAG_NONE

#define METHOD_ATTRIBUTE_FLAG_METHOD_NONE                             (1 << (ATTRIBUTE_FLAG_NONE + 0))
#define METHOD_ATTRIBUTE_FLAG_METHOD_AES_128                          (1 << (ATTRIBUTE_FLAG_NONE + 1))
#define METHOD_ATTRIBUTE_FLAG_METHOD_SAMPLE_AES                       (1 << (ATTRIBUTE_FLAG_NONE + 2))

#define METHOD_ATTRIBUTE_FLAG_LAST                                    (ATTRIBUTE_FLAG_LAST + 3)

#define METHOD_ATTRIBUTE_NAME                                         L"METHOD"

#define METHOD_ATTRIBUTE_VALUE_NONE                                   L"NONE"
#define METHOD_ATTRIBUTE_VALUE_AES_128                                L"AES-128"
#define METHOD_ATTRIBUTE_VALUE_SAMPLE_AES                             L"SAMPLE-AES"

class CMethodAttribute : public CAttribute
{
public:
  CMethodAttribute(HRESULT *result);
  virtual ~CMethodAttribute(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // tests if encryption is none
  // @return : true of encryption is none, false otherwise
  bool IsNone(void);

  // tests if encryption is AES-128
  // @return : true of encryption is AES-128, false otherwise
  bool IsAes128(void);

  // tests if encryption is SAMPLE-AES
  // @return : true of encryption is SAMPLE-AES, false otherwise
  bool IsSampleAes(void);

  // parses name and value
  // @param version : the playlist version
  // @param name : the name of attribute
  // @param value : the value of attribute
  // @return : true if successful, false otherwise
  virtual bool Parse(unsigned int version, const wchar_t *name, const wchar_t *value);

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