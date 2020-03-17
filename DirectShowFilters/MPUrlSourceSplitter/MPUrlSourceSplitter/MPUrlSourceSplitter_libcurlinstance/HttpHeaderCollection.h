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

#ifndef __HTTP_HEADER_COLLECTION_DEFINED
#define __HTTP_HEADER_COLLECTION_DEFINED

#include "KeyedCollection.h"
#include "HttpHeader.h"

class CHttpHeaderCollection : public CKeyedCollection<CHttpHeader, const wchar_t *>
{
public:
  CHttpHeaderCollection(HRESULT *result);
  ~CHttpHeaderCollection(void);

  /* get methods */

  // get the HTTP header from collection with specified name
  // @param name : the name of HTTP header to find
  // @param invariant : specifies if HTTP header name shoud be find with invariant casing
  // @return : HTTP header or NULL if not find
  CHttpHeader *GetHeader(const wchar_t *name, bool invariant);

  /* set methods */

  /* other methods */

  // adds HTTP header to collection
  // @param name : the name of HTTP header to add
  // @param value : the value of HTTP header to add
  // @return : true if successful, false otherwise
  virtual bool Add(const wchar_t *name, const wchar_t *value);

  // adds HTTP header to collection
  // @param header : the HTTP header to parse and to add
  // @return : true if successful, false otherwise
  virtual bool Add(const wchar_t *header);

protected:
  // compare two item keys
  // @param firstKey : the first item key to compare
  // @param secondKey : the second item key to compare
  // @param context : the reference to user defined context
  // @return : 0 if keys are equal, lower than zero if firstKey is lower than secondKey, greater than zero if firstKey is greater than secondKey
  int CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context);

  // gets key for item
  // @param item : the item to get key
  // @return : the key of item
  const wchar_t *GetKey(CHttpHeader *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CHttpHeader *Clone(CHttpHeader *item);
};

#endif