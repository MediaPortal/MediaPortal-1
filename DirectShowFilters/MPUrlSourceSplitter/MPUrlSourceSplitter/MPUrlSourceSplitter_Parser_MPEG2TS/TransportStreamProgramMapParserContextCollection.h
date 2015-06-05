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

#ifndef __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_COLLECTION_DEFINED
#define __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_COLLECTION_DEFINED

#include "Collection.h"
#include "TransportStreamProgramMapParserContext.h"
#include "TsPacketConstants.h"

#define INDEX_NOT_SET                                                 0xFF
#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_NOT_EXISTS        UINT_MAX

class CTransportStreamProgramMapParserContextCollection : public CCollection<CTransportStreamProgramMapParserContext>
{
public:
  CTransportStreamProgramMapParserContextCollection(HRESULT *result);
  virtual ~CTransportStreamProgramMapParserContextCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CTransportStreamProgramMapParserContext *GetItem(unsigned int index);

  // gets transport stream program map parser context ID in collection by PID
  // @param pid : the PID of transport stream program map parser context to get
  // @return : the ID of transport stream program map parser context or TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_NOT_EXISTS if such parser context doesn't exist
  unsigned int GetParserContextIdByPID(unsigned int pid);

  // gets transport stream program map parser context by PID
  // @param pid : the PID of transport stream program map parser context to get
  // @return : the reference to transport stream program map parser context with specified PID or NULL if such parser context doesn't exist
  CTransportStreamProgramMapParserContext *GetParserContextByPID(unsigned int pid);

  /* set methods */

  /* other methods */

  // adds context to collection
  // @param context : the reference to context to add
  // @return : true if successful, false otherwise
  virtual bool Add(CTransportStreamProgramMapParserContext *context);

  // inserts context to collection
  // @param position : zero-based position to insert new context
  // @param context : context to insert
  // @return : true if successful, false otherwise
  virtual bool Insert(unsigned int position, CTransportStreamProgramMapParserContext *context);

  // clear collection of items
  virtual void Clear(void);

  // removes count of contexts from collection from specified index
  // @param index : the index of context to start removing
  // @param count : the count of contexts to remove
  // @return : true if removed, false otherwise
  virtual bool Remove(unsigned int index, unsigned int count);

protected:
  // holds transport stream program map PID mapping to collection index
  uint8_t *pidMap;

  /* methods */

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  virtual CTransportStreamProgramMapParserContext *Clone(CTransportStreamProgramMapParserContext *item);
};

#endif
