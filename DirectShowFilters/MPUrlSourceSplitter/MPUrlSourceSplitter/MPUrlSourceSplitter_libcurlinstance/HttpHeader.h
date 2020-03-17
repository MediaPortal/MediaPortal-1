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

#ifndef __HTTP_HEADER_DEFINED
#define __HTTP_HEADER_DEFINED

#include "Flags.h"

#define HTTP_HEADER_FLAG_NONE                                         FLAGS_NONE

#define HTTP_HEADER_FLAG_LAST                                         (FLAGS_LAST + 0)

class CHttpHeader : public CFlags
{
public:
  CHttpHeader(HRESULT *result);
  virtual ~CHttpHeader(void);

  /* get methods */

  // gets HTTP header name
  // @return : HTTP header name
  virtual const wchar_t *GetName(void);

  // gets HTTP header name length
  // @return : HTTP header name length or UINT_MAX if error
  virtual unsigned int GetNameLength(void);

  // gets HTTP header value
  // @return : HTTP header value
  virtual const wchar_t *GetValue(void);

  // gets HTTP header value length
  // @return : HTTP header value length or UINT_MAX if error
  virtual unsigned int GetValueLength(void);

  /* set methods */

  // sets HTTP header name
  // @param name : HTTP header name to set
  // @return : true if successful, false otherwise
  virtual bool SetName(const wchar_t *name);

  // sets HTTP header value
  // @param value : HTTP header value to set
  // @return : true if successful, false otherwise
  virtual bool SetValue(const wchar_t *value);

  /* other methods */

  // clears HTTP header name and its value
  virtual void Clear(void);

  // tests if instance is valid (if HTTP header name and its value are not NULL)
  // @return : true if instance is valid, false otherwise
  virtual bool IsValid(void);

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  virtual CHttpHeader *Clone(void);

  // parses header and stores name and value to internal variables
  // @param header : header to parse
  // @return : true if successful, false otherwise
  virtual bool Parse(const wchar_t *header);

  // parses header and stores name and value to internal variables
  // @param header : header to parse
  // @param length : the length of header
  // @return : true if successful, false otherwise
  virtual bool Parse(const wchar_t *header, unsigned int length);

protected:
  // holds HTTP header name
  wchar_t *name;
  // holds HTTP header value
  wchar_t *value;

  // deeply clones current instance to cloned header
  // @param  clone : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clone);

  // returns new header object to be used in cloning
  // @return : header object or NULL if error
  virtual CHttpHeader *CreateHeader(void);
};

#endif