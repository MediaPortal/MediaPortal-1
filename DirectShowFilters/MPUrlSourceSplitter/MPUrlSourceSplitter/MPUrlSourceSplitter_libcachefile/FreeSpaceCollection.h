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

#ifndef __FREE_SPACE_COLLECTION_DEFINED
#define __FREE_SPACE_COLLECTION_DEFINED

#include "Collection.h"
#include "FreeSpace.h"

#define FREE_SPACE_NOT_FOUND                                          UINT_MAX

class CFreeSpaceCollection : public CCollection<CFreeSpace>
{
public:
  CFreeSpaceCollection(HRESULT *result);
  virtual ~CFreeSpaceCollection(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // adds free space from specified start with specified length
  // @param start : the start byte of space to be added
  // @param length : the length of space to be added
  // @return : true if successful, false otherwise
  bool AddFreeSpace(int64_t start, int64_t length);

  // removes free space from specified free space index with specified length
  // @param index : the index of free space to be released
  // @param length : the length of space to be released
  // @return : true if successful, false otherwise
  bool RemoveFreeSpace(unsigned int index, int64_t length);

  // returns indexes where item have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => item have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => item have to be placed on end
  // startIndex == endIndex => item with same key exists in collection (index of item is startIndex)
  // item have to be placed between startIndex and endIndex
  // @param start : the start byte to compare
  // @param startIndex : reference to variable which holds start index where item have to be placed
  // @param endIndex : reference to variable which holds end index where item have to be placed
  // @return : true if successful, false otherwise
  bool GetItemInsertPosition(int64_t start, unsigned int *startIndex, unsigned int *endIndex);

  // finds suitable free space in free space collection
  // @param length : the length of free space
  // @return : the index of free space instance with suitable suitable free space or FREE_SPACE_NOT_FOUND if not found
  unsigned int FindSuitableFreeSpace(int64_t length);

protected:

  /* methods */

  // adds free space to collection
  // do not call this method directly
  // @param item : the reference to free space to add
  // @return : true if successful, false otherwise
  virtual bool Add(CFreeSpace *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  virtual CFreeSpace *Clone(CFreeSpace *item);
};

#endif