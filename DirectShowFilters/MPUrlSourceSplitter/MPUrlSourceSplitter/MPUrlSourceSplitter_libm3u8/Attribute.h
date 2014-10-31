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

#ifndef __ATTRIBUTE_DEFINED
#define __ATTRIBUTE_DEFINED

#include "Flags.h"
#include "float.h"

#define ATTRIBUTE_FLAG_NONE                                           FLAGS_NONE

#define ATTRIBUTE_FLAG_LAST                                           (FLAGS_LAST + 0)

#define ATTRIBUTE_NAME_VALUE_SEPARATOR                                L"="
#define ATTRIBUTE_NAME_VALUE_SEPARATOR_LENGTH                         1

#define ATTRIBUTE_SEPARATOR                                           L","
#define ATTRIBUTE_SEPARATOR_LENGTH                                    1

#define DECIMAL_INTEGER_NOT_SPECIFIED                                 UINT_MAX
#define DECIMAL_FLOATING_NOT_SPECIFIED                                DBL_MAX

#define RESOLUTION_NOT_SPECIFIED                                      UINT_MAX

class CAttribute : public CFlags
{
public:
  CAttribute(HRESULT *result);
  virtual ~CAttribute(void);

  /* get methods */

  // gets name of attribute
  // @return : name of attribute
  virtual const wchar_t *GetName(void);

  // gets value of attribute
  // @ return : attribute value or NULL if not specified
  virtual const wchar_t *GetValue(void);

  /* set methods */

  /* other methods */

  // clears current instance
  virtual void Clear(void);

  // parses data in buffer
  // @param version : the playlist version
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : true if successful, false otherwise
  virtual bool Parse(unsigned int version, const wchar_t *buffer, unsigned int length);

  // parses name and value
  // @param version : the playlist version
  // @param name : the name of attribute
  // @param value : the value of attribute
  // @return : true if successful, false otherwise
  virtual bool Parse(unsigned int version, const wchar_t *name, const wchar_t *value);

  // deep clone of current instance
  // @return : reference to clone of attribute or NULL if error
  virtual CAttribute *Clone(void);

  /* static methods */

  static unsigned int GetDecimalInteger(const wchar_t *value);

  static unsigned int GetHexadecimalInteger(const wchar_t *value);

  static double GetDecimalFloatingPoint(const wchar_t *value);

  static wchar_t *GetQuotedString(const wchar_t *value);

  static wchar_t *GetEnumeratedString(const wchar_t *value);

  static unsigned int GetDecimalResolutionWidth(const wchar_t *value);

  static unsigned int GetDecimalResolutionHeight(const wchar_t *value);

protected:

  // holds attribute name
  wchar_t *name;

  // holds attribute value
  wchar_t *value;

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