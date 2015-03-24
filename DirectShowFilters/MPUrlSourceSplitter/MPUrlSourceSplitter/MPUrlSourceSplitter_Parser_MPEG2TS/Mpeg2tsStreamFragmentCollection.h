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

#ifndef __MPEG2TS_STREAM_FRAGMENT_COLLECTION_DEFINED
#define __MPEG2TS_STREAM_FRAGMENT_COLLECTION_DEFINED

#include "StreamFragmentCollection.h"
#include "Mpeg2tsStreamFragment.h"
#include "IndexedMpeg2tsStreamFragmentCollection.h"

class CMpeg2tsStreamFragmentCollection : public CStreamFragmentCollection
{
public:
  CMpeg2tsStreamFragmentCollection(HRESULT *result);
  virtual ~CMpeg2tsStreamFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CMpeg2tsStreamFragment *GetItem(unsigned int index);

  // gets collection of indexed stream fragments which are ready for align
  // @param collection : the collection to fill in indexed stream fragments
  // @return : S_OK if successful, error code otherwise
  HRESULT GetReadyForAlignStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection);

  // gets collection of indexed stream fragments which are aligned
  // @param collection : the collection to fill in indexed stream fragments
  // @return : S_OK if successful, error code otherwise
  HRESULT GetAlignedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection);

  // gets collection of indexed stream fragments which are discontinuity processed
  // @param collection : the collection to fill in indexed stream fragments
  // @return : S_OK if successful, error code otherwise
  HRESULT GetDiscontinuityProcessedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection);

  // gets collection of indexed stream fragments which are parsed for program association sections
  // @param collection : the collection to fill in indexed stream fragments
  // @return : S_OK if successful, error code otherwise
  HRESULT GetProgramAssociationSectionDetectionFinishedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection);

  // gets collection of indexed stream fragments which are parsed for transport stream map sections
  // @param collection : the collection to fill in indexed stream fragments
  // @return : S_OK if successful, error code otherwise
  HRESULT GetTransportStreamMapSectionDetectionFinishedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection);

  // gets collection of indexed stream fragments which have program association sections and transport stream map sections updated
  // @param collection : the collection to fill in indexed stream fragments
  // @return : S_OK if successful, error code otherwise
  HRESULT GetBothSectionsUpdatedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection);

  /* set methods */

  /* other methods */

  // tests if collection has some stream fragments ready for align
  // @return : true if collection has such fragments, false otherwise
  bool HasReadyForAlignStreamFragments(void);

  // tests if collection has some stream fragments, which are aligned
  // @return : true if collection has such fragments, false otherwise
  bool HasAlignedStreamFragments(void);

  // tests if collection has some stream fragments, which are discontinuity processed
  // @return : true if collection has such fragments, false otherwise
  bool HasDiscontinuityProcessedStreamFragments(void);

  // tests if collection has some stream fragments, which are partially processed
  // @return : true if collection has such fragments, false otherwise
  bool HasProgramAssociationSectionDetectionFinishedStreamFragments(void);

  // tests if collection has some stream fragments, which are partially processed
  // @return : true if collection has such fragments, false otherwise
  bool HasTransportStreamMapSectionDetectionFinishedStreamFragments(void);

  // tests if collection has some stream fragments, which have program association sections and transport stream map sections updated
  // @return : true if collection has such fragments, false otherwise
  bool HasBothSectionsUpdatedStreamFragments(void);

  // recalculate aligned stream fragments start positions based on previous aligned stream fragments
  // @param startIndex : the index of first aligned stream fragment to recalculate start position
  void RecalculateAlignedStreamFragmentStartPosition(unsigned int startIndex);

  /* index methods */

  // insert item with specified item index to indexes
  // @param itemIndex : the item index in collection to insert into indexes
  // @return : true if successful, false otherwise
  virtual bool InsertIndexes(unsigned int itemIndex);

  // removes items from indexes
  // @param startIndex : the start index of items to remove from indexes
  // @param count : the count of items to remove from indexes
  virtual void RemoveIndexes(unsigned int startIndex, unsigned int count);

  // updates indexes by using specified item
  // @param itemIndex : index of item to update indexes
  // @param count : the count of items to updates indexes
  // @retur : true if successful, false otherwise
  virtual bool UpdateIndexes(unsigned int itemIndex, unsigned int count);

  // ensures that in internal buffer of indexes is enough space
  // each index must check against its count of items and add addingCount
  // if in internal buffer of indexes is not enough space, method tries to allocate enough space in index
  // @param addingCount : the count of added index items
  // @return : true if in internal buffer of indexes is enough space, false otherwise
  virtual bool EnsureEnoughSpaceIndexes(unsigned int addingCount);

  // clears all indexes to default state
  virtual void ClearIndexes(void);

protected:

  // we need to maintain several indexes
  // first index : item->IsReadyForAlign()
  // second index : item->IsAligned()
  // third index : item->IsDiscontinuityProcessed()
  // fourth index : item->IsProgramAssociationSectionDetectionFinished()
  // fifth index : item->IsTransportStreamMapSectionDetectionFinished()
  // sixth index : item->IsProgramAssociationSectionUpdated() && item->IsTransportStreamMapSectionUpdated()

  CIndexCollection *indexReadyForAlign;
  CIndexCollection *indexAligned;
  CIndexCollection *indexDiscontinuityProcessed;
  CIndexCollection *indexProgramAssociationSectionDetectionFinished;
  CIndexCollection *indexTransportStreamMapSectionDetectionFinished;
  CIndexCollection *indexBothSectionsUpdated;

  /* methods */
};

#endif