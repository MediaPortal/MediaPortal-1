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

#include "StdAfx.h"

#include "Mpeg2tsStreamFragmentCollection.h"

#define AFFECTED_INDEX_NONE                                                                 FLAGS_NONE

#define AFFECTED_INDEX_BASE                                                                 (1 << (FLAGS_LAST + 0))
#define AFFECTED_INDEX_READY_FOR_ALIGN_ADD                                                  (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_READY_FOR_ALIGN_INC                                                  (1 << (FLAGS_LAST + 2))
#define AFFECTED_INDEX_ALIGNED_ADD                                                          (1 << (FLAGS_LAST + 3))
#define AFFECTED_INDEX_ALIGNED_INC                                                          (1 << (FLAGS_LAST + 4))
#define AFFECTED_INDEX_DISCONTINUITY_PROCESSED_ADD                                          (1 << (FLAGS_LAST + 5))
#define AFFECTED_INDEX_DISCONTINUITY_PROCESSED_INC                                          (1 << (FLAGS_LAST + 6))
#define AFFECTED_INDEX_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED_ADD                   (1 << (FLAGS_LAST + 7))
#define AFFECTED_INDEX_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED_INC                   (1 << (FLAGS_LAST + 8))
#define AFFECTED_INDEX_TRANSPORT_STREAM_MAP_SECTION_DETECTION_FINISHED_ADD                  (1 << (FLAGS_LAST + 9))
#define AFFECTED_INDEX_TRANSPORT_STREAM_MAP_SECTION_DETECTION_FINISHED_INC                  (1 << (FLAGS_LAST + 10))
#define AFFECTED_INDEX_BOTH_SECTIONS_UPDATED_ADD                                            (1 << (FLAGS_LAST + 11))
#define AFFECTED_INDEX_BOTH_SECTIONS_UPDATED_INC                                            (1 << (FLAGS_LAST + 12))

CMpeg2tsStreamFragmentCollection::CMpeg2tsStreamFragmentCollection(HRESULT *result)
  : CStreamFragmentCollection(result)
{
  this->indexReadyForAlign = NULL;
  this->indexAligned = NULL;
  this->indexDiscontinuityProcessed = NULL;
  this->indexProgramAssociationSectionDetectionFinished = NULL;
  this->indexTransportStreamMapSectionDetectionFinished = NULL;
  this->indexBothSectionsUpdated = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexReadyForAlign = new CIndexCollection(result);
    this->indexAligned = new CIndexCollection(result);
    this->indexDiscontinuityProcessed = new CIndexCollection(result);
    this->indexProgramAssociationSectionDetectionFinished = new CIndexCollection(result);
    this->indexTransportStreamMapSectionDetectionFinished = new CIndexCollection(result);
    this->indexBothSectionsUpdated = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexReadyForAlign, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexAligned, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexDiscontinuityProcessed, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexProgramAssociationSectionDetectionFinished, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexTransportStreamMapSectionDetectionFinished, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexBothSectionsUpdated, *result, E_OUTOFMEMORY);
  }
}

CMpeg2tsStreamFragmentCollection::~CMpeg2tsStreamFragmentCollection(void)
{
  FREE_MEM_CLASS(this->indexReadyForAlign);
  FREE_MEM_CLASS(this->indexAligned);
  FREE_MEM_CLASS(this->indexDiscontinuityProcessed);
  FREE_MEM_CLASS(this->indexProgramAssociationSectionDetectionFinished);
  FREE_MEM_CLASS(this->indexTransportStreamMapSectionDetectionFinished);
  FREE_MEM_CLASS(this->indexBothSectionsUpdated);
}

/* get methods */

CMpeg2tsStreamFragment *CMpeg2tsStreamFragmentCollection::GetItem(unsigned int index)
{
  return (CMpeg2tsStreamFragment *)__super::GetItem(index);
}

HRESULT CMpeg2tsStreamFragmentCollection::GetReadyForAlignStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexReadyForAlign->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexReadyForAlign->Count())); i++)
  {
    unsigned int index = this->indexReadyForAlign->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetAlignedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexAligned->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexAligned->Count())); i++)
  {
    unsigned int index = this->indexAligned->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetDiscontinuityProcessedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexDiscontinuityProcessed->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexDiscontinuityProcessed->Count())); i++)
  {
    unsigned int index = this->indexDiscontinuityProcessed->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetProgramAssociationSectionDetectionFinishedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexProgramAssociationSectionDetectionFinished->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexProgramAssociationSectionDetectionFinished->Count())); i++)
  {
    unsigned int index = this->indexProgramAssociationSectionDetectionFinished->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetTransportStreamMapSectionDetectionFinishedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexTransportStreamMapSectionDetectionFinished->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexTransportStreamMapSectionDetectionFinished->Count())); i++)
  {
    unsigned int index = this->indexTransportStreamMapSectionDetectionFinished->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetBothSectionsUpdatedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexReadyForAlign->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexBothSectionsUpdated->Count())); i++)
  {
    unsigned int index = this->indexBothSectionsUpdated->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

/* set methods */

/* other methods */

bool CMpeg2tsStreamFragmentCollection::HasReadyForAlignStreamFragments(void)
{
  return (this->indexReadyForAlign->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasAlignedStreamFragments(void)
{
  return (this->indexAligned->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasDiscontinuityProcessedStreamFragments(void)
{
  return (this->indexDiscontinuityProcessed->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasProgramAssociationSectionDetectionFinishedStreamFragments(void)
{
  return (this->indexProgramAssociationSectionDetectionFinished->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasTransportStreamMapSectionDetectionFinishedStreamFragments(void)
{
  return (this->indexTransportStreamMapSectionDetectionFinished->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasBothSectionsUpdatedStreamFragments(void)
{
  return (this->indexBothSectionsUpdated->Count() != 0);
}

void CMpeg2tsStreamFragmentCollection::RecalculateAlignedStreamFragmentStartPosition(unsigned int startIndex)
{
  for (unsigned int i = startIndex; i < this->Count(); i++)
  {
    CMpeg2tsStreamFragment *fragment = this->GetItem(i);
    CMpeg2tsStreamFragment *previousFragment = (i > 0) ? this->GetItem(i - 1) : NULL;

    if (fragment->IsDownloaded())
    {
      if ((previousFragment != NULL) && (previousFragment->IsAtLeastAligned()))
      {
        fragment->SetFragmentStartPosition(previousFragment->GetFragmentStartPosition() + previousFragment->GetLength());
      }

      if (i == (this->GetStartSearchingIndex() + this->GetSearchCount()))
      {
        this->SetSearchCount(this->GetSearchCount() + 1);
      }
    }
    else
    {
      // we found not downloaded stream fragment, stop recalculating start positions
      break;
    }
  }
}

/* index methods */

bool CMpeg2tsStreamFragmentCollection::InsertIndexes(unsigned int itemIndex)
{
  uint32_t flags = AFFECTED_INDEX_NONE;
  bool result = __super::InsertIndexes(itemIndex);

  unsigned int indexReadyForAlignItemIndex = UINT_MAX;
  unsigned int indexAlignedItemIndex = UINT_MAX;
  unsigned int indexDiscontinuityProcessedItemIndex = UINT_MAX;
  unsigned int indexProgramAssociationSectionDetectionFinishedItemIndex = UINT_MAX;
  unsigned int indexTransportStreamMapSectionDetectionFinishedItemIndex = UINT_MAX;
  unsigned int indexBothSectionsUpdatedItemIndex = UINT_MAX;

  if (result)
  {
    flags |= AFFECTED_INDEX_BASE;

    // we must check if some index needs to be updated
    // in case of error we must revert all updated indexes
    CMpeg2tsStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexReadyForAlign->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexReadyForAlignItemIndex = min(endIndex, this->indexReadyForAlign->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexReadyForAlign->Increase(indexReadyForAlignItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_READY_FOR_ALIGN_INC);

        if (item->IsReadyForAlign())
        {
          result &= this->indexReadyForAlign->Insert(indexReadyForAlignItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_READY_FOR_ALIGN_ADD);
        }
      }
    }

    // second index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexAligned->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexAlignedItemIndex = min(endIndex, this->indexAligned->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexAligned->Increase(indexAlignedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_INC);

        if (item->IsAligned())
        {
          result &= this->indexAligned->Insert(indexAlignedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_ADD);
        }
      }
    }

    // third index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexDiscontinuityProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexDiscontinuityProcessedItemIndex = min(endIndex, this->indexDiscontinuityProcessed->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexDiscontinuityProcessed->Increase(indexDiscontinuityProcessedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_DISCONTINUITY_PROCESSED_INC);

        if (item->IsDiscontinuityProcessed())
        {
          result &= this->indexDiscontinuityProcessed->Insert(indexDiscontinuityProcessedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_DISCONTINUITY_PROCESSED_ADD);
        }
      }
    }

    // fourth index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexProgramAssociationSectionDetectionFinished->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexProgramAssociationSectionDetectionFinishedItemIndex = min(endIndex, this->indexProgramAssociationSectionDetectionFinished->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexProgramAssociationSectionDetectionFinished->Increase(indexProgramAssociationSectionDetectionFinishedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED_INC);

        if (item->IsProgramAssociationSectionDetectionFinished())
        {
          result &= this->indexProgramAssociationSectionDetectionFinished->Insert(indexProgramAssociationSectionDetectionFinishedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED_ADD);
        }
      }
    }

    // fifth index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexTransportStreamMapSectionDetectionFinished->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexTransportStreamMapSectionDetectionFinishedItemIndex = min(endIndex, this->indexTransportStreamMapSectionDetectionFinished->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexTransportStreamMapSectionDetectionFinished->Increase(indexTransportStreamMapSectionDetectionFinishedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_TRANSPORT_STREAM_MAP_SECTION_DETECTION_FINISHED_INC);

        if (item->IsTransportStreamMapSectionDetectionFinished())
        {
          result &= this->indexTransportStreamMapSectionDetectionFinished->Insert(indexTransportStreamMapSectionDetectionFinishedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_TRANSPORT_STREAM_MAP_SECTION_DETECTION_FINISHED_ADD);
        }
      }
    }

    // sixth index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexBothSectionsUpdated->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexBothSectionsUpdatedItemIndex = min(endIndex, this->indexBothSectionsUpdated->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexBothSectionsUpdated->Increase(indexBothSectionsUpdatedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_BOTH_SECTIONS_UPDATED_INC);

        if (item->IsProgramAssociationSectionUpdated() && item->IsTransportStreamMapSectionUpdated())
        {
          result &= this->indexBothSectionsUpdated->Insert(indexBothSectionsUpdatedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_BOTH_SECTIONS_UPDATED_ADD);
        }
      }
    }
  }

  if (!result)
  {
    // revert first index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_READY_FOR_ALIGN_ADD), this->indexReadyForAlign->Remove(indexReadyForAlignItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_READY_FOR_ALIGN_INC), this->indexReadyForAlign->Decrease(indexReadyForAlignItemIndex, 1));

    // revert second index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_ADD), this->indexAligned->Remove(indexAlignedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_INC), this->indexAligned->Decrease(indexAlignedItemIndex, 1));

    // revert third index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_DISCONTINUITY_PROCESSED_ADD), this->indexDiscontinuityProcessed->Remove(indexDiscontinuityProcessedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_DISCONTINUITY_PROCESSED_INC), this->indexDiscontinuityProcessed->Decrease(indexDiscontinuityProcessedItemIndex, 1));

    // revert fourth index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED_ADD), this->indexProgramAssociationSectionDetectionFinished->Remove(indexProgramAssociationSectionDetectionFinishedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED_INC), this->indexProgramAssociationSectionDetectionFinished->Decrease(indexProgramAssociationSectionDetectionFinishedItemIndex, 1));

    // revert fifth index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_TRANSPORT_STREAM_MAP_SECTION_DETECTION_FINISHED_ADD), this->indexTransportStreamMapSectionDetectionFinished->Remove(indexTransportStreamMapSectionDetectionFinishedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_TRANSPORT_STREAM_MAP_SECTION_DETECTION_FINISHED_INC), this->indexTransportStreamMapSectionDetectionFinished->Decrease(indexTransportStreamMapSectionDetectionFinishedItemIndex, 1));

    // revert sixth index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BOTH_SECTIONS_UPDATED_ADD), this->indexBothSectionsUpdated->Remove(indexBothSectionsUpdatedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BOTH_SECTIONS_UPDATED_INC), this->indexBothSectionsUpdated->Decrease(indexBothSectionsUpdatedItemIndex, 1));

    // revert base indexes
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BASE), __super::RemoveIndexes(itemIndex, 1));
  }


  return result;
}

void CMpeg2tsStreamFragmentCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
{
  __super::RemoveIndexes(startIndex, count);

  unsigned int start = 0;
  unsigned int end = 0;

  unsigned int indexStart = 0;
  unsigned int indexCount = 0;

  // first index
  this->indexReadyForAlign->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexReadyForAlign->Count());

  this->indexReadyForAlign->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexReadyForAlign->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexReadyForAlign->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexReadyForAlign->Decrease(indexStart, count);


  // second index
  this->indexAligned->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexAligned->Count());

  this->indexAligned->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexAligned->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexAligned->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexAligned->Decrease(indexStart, count);


  // third index
  this->indexDiscontinuityProcessed->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexDiscontinuityProcessed->Count());

  this->indexDiscontinuityProcessed->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexDiscontinuityProcessed->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexDiscontinuityProcessed->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexDiscontinuityProcessed->Decrease(indexStart, count);


  // fourth index
  this->indexProgramAssociationSectionDetectionFinished->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexProgramAssociationSectionDetectionFinished->Count());

  this->indexProgramAssociationSectionDetectionFinished->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexProgramAssociationSectionDetectionFinished->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexProgramAssociationSectionDetectionFinished->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexProgramAssociationSectionDetectionFinished->Decrease(indexStart, count);


  // fifth index
  this->indexTransportStreamMapSectionDetectionFinished->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexTransportStreamMapSectionDetectionFinished->Count());

  this->indexTransportStreamMapSectionDetectionFinished->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexTransportStreamMapSectionDetectionFinished->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexTransportStreamMapSectionDetectionFinished->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexTransportStreamMapSectionDetectionFinished->Decrease(indexStart, count);


  // sixth index
  this->indexBothSectionsUpdated->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexBothSectionsUpdated->Count());

  this->indexBothSectionsUpdated->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexBothSectionsUpdated->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexBothSectionsUpdated->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexBothSectionsUpdated->Decrease(indexStart, count);
}

bool CMpeg2tsStreamFragmentCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  bool result = __super::UpdateIndexes(itemIndex, count);

  if (result)
  {
    CMpeg2tsStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      bool test = item->IsReadyForAlign();
      unsigned int indexIndex = this->indexReadyForAlign->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexReadyForAlign->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexReadyForAlign->Insert(min(endIndex, this->indexReadyForAlign->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexReadyForAlign->Remove(indexIndex, count);
      }
    }

    // second index
    if (result)
    {
      bool test = item->IsAligned();
      unsigned int indexIndex = this->indexAligned->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexAligned->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexAligned->Insert(min(endIndex, this->indexAligned->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexAligned->Remove(indexIndex, count);
      }
    }

    // third index
    if (result)
    {
      bool test = item->IsDiscontinuityProcessed();
      unsigned int indexIndex = this->indexDiscontinuityProcessed->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexDiscontinuityProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexDiscontinuityProcessed->Insert(min(endIndex, this->indexDiscontinuityProcessed->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexDiscontinuityProcessed->Remove(indexIndex, count);
      }
    }

    // fourth index
    if (result)
    {
      bool test = item->IsProgramAssociationSectionDetectionFinished();
      unsigned int indexIndex = this->indexProgramAssociationSectionDetectionFinished->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexProgramAssociationSectionDetectionFinished->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexProgramAssociationSectionDetectionFinished->Insert(min(endIndex, this->indexProgramAssociationSectionDetectionFinished->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexProgramAssociationSectionDetectionFinished->Remove(indexIndex, count);
      }
    }

    // fifth index
    if (result)
    {
      bool test = item->IsTransportStreamMapSectionDetectionFinished();
      unsigned int indexIndex = this->indexTransportStreamMapSectionDetectionFinished->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexTransportStreamMapSectionDetectionFinished->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexTransportStreamMapSectionDetectionFinished->Insert(min(endIndex, this->indexTransportStreamMapSectionDetectionFinished->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexTransportStreamMapSectionDetectionFinished->Remove(indexIndex, count);
      }
    }

    // sixth index
    if (result)
    {
      bool test = item->IsProgramAssociationSectionUpdated() && item->IsTransportStreamMapSectionUpdated();
      unsigned int indexIndex = this->indexBothSectionsUpdated->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexBothSectionsUpdated->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexBothSectionsUpdated->Insert(min(endIndex, this->indexBothSectionsUpdated->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexBothSectionsUpdated->Remove(indexIndex, count);
      }
    }
  }

  return result;
}

bool CMpeg2tsStreamFragmentCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  bool result = __super::EnsureEnoughSpaceIndexes(addingCount);

  if (result)
  {
    result &= this->indexReadyForAlign->EnsureEnoughSpace(this->indexReadyForAlign->Count() + addingCount);
    result &= this->indexAligned->EnsureEnoughSpace(this->indexAligned->Count() + addingCount);
    result &= this->indexDiscontinuityProcessed->EnsureEnoughSpace(this->indexDiscontinuityProcessed->Count() + addingCount);
    result &= this->indexProgramAssociationSectionDetectionFinished->EnsureEnoughSpace(this->indexProgramAssociationSectionDetectionFinished->Count() + addingCount);
    result &= this->indexTransportStreamMapSectionDetectionFinished->EnsureEnoughSpace(this->indexTransportStreamMapSectionDetectionFinished->Count() + addingCount);
    result &= this->indexBothSectionsUpdated->EnsureEnoughSpace(this->indexBothSectionsUpdated->Count() + addingCount);
  }

  return result;
}

void CMpeg2tsStreamFragmentCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexReadyForAlign->Clear();
  this->indexAligned->Clear();
  this->indexDiscontinuityProcessed->Clear();
  this->indexProgramAssociationSectionDetectionFinished->Clear();
  this->indexTransportStreamMapSectionDetectionFinished->Clear();
  this->indexBothSectionsUpdated->Clear();
}


/* protected methods */
