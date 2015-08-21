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

#ifndef __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_KNOWN_SECTION_CONTEXT_COLLECTION_DEFINED
#define __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_KNOWN_SECTION_CONTEXT_COLLECTION_DEFINED

#include "Collection.h"
#include "TransportStreamProgramMapParserKnownSectionContext.h"

class CTransportStreamProgramMapParserKnownSectionContextCollection : public CCollection<CTransportStreamProgramMapParserKnownSectionContext>
{
public:
  CTransportStreamProgramMapParserKnownSectionContextCollection(HRESULT *result);
  virtual ~CTransportStreamProgramMapParserKnownSectionContextCollection();

  /* get methods */

  /* set methods */

  /* other methods */

  // add item to collection
  // @param programNumber : program number to add
  // @param crc32 : the CRC32 to add
  // @return : true if successful, false otherwise
  virtual bool Add(uint16_t programNumber, unsigned int crc32);

  // tests if collection contains specified program number with CRC32
  // @param programNumber : program number to test
  // @param crc32 : the CRC32 to test
  // @return : true if collection contains program number with CRC32, false otherwise
  bool Contains(uint16_t programNumber, unsigned int crc32);

protected:

  /* methods */

  // returns indexes where item have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => item have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => item have to be placed on end
  // startIndex == endIndex => item with same value exists in collection (index of item is startIndex)
  // item have to be placed between startIndex and endIndex
  // @param position : the start position to compare
  // @param startIndex : reference to variable which holds start index where item have to be placed
  // @param endIndex : reference to variable which holds end index where item have to be placed
  // @return : true if successful, false otherwise
  bool GetItemInsertPosition(uint16_t value, unsigned int *startIndex, unsigned int *endIndex);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  virtual CTransportStreamProgramMapParserKnownSectionContext *Clone(CTransportStreamProgramMapParserKnownSectionContext *item);
};

#endif