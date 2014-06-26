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

#ifndef __PAYLOAD_TYPE_COLLECTION_DEFINED
#define __PAYLOAD_TYPE_COLLECTION_DEFINED

#include "PayloadType.h"
#include "KeyedCollection.h"

class CPayloadTypeCollection : public CKeyedCollection<CPayloadType, const wchar_t *>
{
public:
  CPayloadTypeCollection(HRESULT *result);
  virtual ~CPayloadTypeCollection(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // adds payload type to payload type collection
  // @param id : the payload type ID or PAYLOAD_TYPE_ID_DYNAMIC if not specified
  // @param encodingName : the payload type encoding name or NULL if not specified
  // @param mediaType : the payload type media type
  // @param clockRate : the payload type clock rate or PAYLOAD_TYPE_CLOCK_RATE_VARIABLE if not specified
  // @param channels : the payload type channel count or PAYLOAD_TYPE_CHANNELS_VARIABLE if not specified
  // @return : true if successful, false otherwise
  bool AddPayloadType(unsigned int id, const wchar_t *encodingName, CPayloadType::MediaType mediaType, unsigned int clockRate, unsigned int channels);

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
  const wchar_t *GetKey(CPayloadType *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CPayloadType *Clone(CPayloadType *item);
};

#endif