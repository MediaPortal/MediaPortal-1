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

#ifndef __SECTION_MULTIPLEXER_COLLECTION_DEFINED
#define __SECTION_MULTIPLEXER_COLLECTION_DEFINED

#include "Collection.h"
#include "SectionMultiplexer.h"
#include "TsPacketConstants.h"

#define INDEX_NOT_SET                                                 0xFF
#define MPEG2TS_MULTIPEXER_NOT_EXISTS                                 UINT_MAX

class CSectionMultiplexerCollection : public CCollection<CSectionMultiplexer>
{
public:
  CSectionMultiplexerCollection(HRESULT *result);
  virtual ~CSectionMultiplexerCollection();

  /* get methods */

  // gets multiplexer ID in collection by PID
  // @param pid : the PID of multiplexer to get
  // @return : the ID of multiplexer or MPEG2TS_MULTIPEXER_NOT_EXISTS if such multiplexer doesn't exist
  unsigned int GetMultiplexerIdByPID(unsigned int pid);

  // gets multiplexer by PID
  // @param pid : the PID of multipexer to get
  // @return : the reference to multiplexer with specified PID or NULL if such multiplexer doesn't exist
  CSectionMultiplexer *GetMultiplexerByPID(unsigned int pid);

  /* set methods */

  /* other methods */

  // adds multiplexer to collection
  // @param multiplexer : the reference to multiplexer to add
  // @return : true if successful, false otherwise
  virtual bool Add(CSectionMultiplexer *multiplexer);

  // inserts multiplexer to collection
  // @param position : zero-based position to insert new multiplexer
  // @param multiplexer : context to insert
  // @return : always false
  virtual bool Insert(unsigned int position, CSectionMultiplexer *multiplexer);

  // clear collection of items
  virtual void Clear(void);

  // removes count of multiplexers from collection from specified index
  // @param index : the index of multiplexer to start removing
  // @param count : the count of multiplexers to remove
  // @return : true if removed, false otherwise
  virtual bool Remove(unsigned int index, unsigned int count);

protected:
  // holds multiplexer PID mapping to collection index
  uint8_t *pidMap;

  /* methods */

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  virtual CSectionMultiplexer *Clone(CSectionMultiplexer *item);
};

#endif