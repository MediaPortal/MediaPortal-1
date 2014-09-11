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

#include "ContinuousStreamRangeCollection.h"

CContinuousStreamRangeCollection::CContinuousStreamRangeCollection(HRESULT *result)
  : CCollection(result)
{
}

CContinuousStreamRangeCollection::~CContinuousStreamRangeCollection(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CContinuousStreamRangeCollection::AddContinuousStreamRange(int64_t filterStartPosition, int64_t protocolStartPosition, int64_t streamLength, uint32_t flags)
{
  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  HRESULT result = this->GetItemInsertPosition(filterStartPosition, &startIndex, &endIndex) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    endIndex = min(endIndex, this->Count());

    bool added = false;
    if (endIndex != this->Count())
    {
      // we need to check if current continuous stream range can be attached to existing continuous stream range
      CContinuousStreamRange *streamRange = this->GetItem(endIndex);

      if (((filterStartPosition + streamLength) == streamRange->GetFilterStartPosition()) &&
          ((protocolStartPosition + streamLength) == streamRange->GetProtocolStartPosition()))
      {
        // continuous stream range
        // update existing stream range

        streamRange->SetStreamLength(streamRange->GetStreamLength() + streamRange->GetFilterStartPosition() - filterStartPosition);
        streamRange->SetFilterStartPosition(filterStartPosition);
        streamRange->SetProtocolStartPosition(protocolStartPosition);

        added = true;
      }
    }

    if (startIndex != UINT_MAX)
    {
      // we need to check if current continuous stream range can be attached to existing continuous stream range
      CContinuousStreamRange *streamRange = this->GetItem(startIndex);

      if (((streamRange->GetFilterStartPosition() + streamRange->GetStreamLength()) == filterStartPosition) &&
          ((streamRange->GetProtocolStartPosition() + streamRange->GetStreamLength()) == protocolStartPosition))
      {
        // continuous stream range
        // update existing stream range

        streamRange->SetStreamLength(streamRange->GetStreamLength() + streamLength);

        added = true;
      }
    }

    if (!added)
    {
      // add new continuous stream range
      CContinuousStreamRange *streamRange = new CContinuousStreamRange(&result);
      CHECK_POINTER_HRESULT(result, streamRange, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        streamRange->SetFilterStartPosition(filterStartPosition);
        streamRange->SetProtocolStartPosition(protocolStartPosition);
        streamRange->SetStreamLength(streamLength);
      }

      CHECK_CONDITION_HRESULT(result, this->Insert(endIndex, streamRange), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamRange));
    }
    //else if ((startIndex != UINT_MAX) && (endIndex != this->Count()))
    //{
    //  // check if two adjacent continuous stream ranges are not now one continuous stream range
    //  CContinuousStreamRange *startStreamRange = this->GetItem(startIndex);
    //  CContinuousStreamRange *endStreamRange = this->GetItem(endIndex);

    //  if ((startStreamRange->GetFilterStartPosition() + startStreamRange->GetStreamLength()) == endStreamRange->GetFilterStartPosition())
    //  {
    //    // continuous stream range
    //    // update existing stream range, remove last

    //    startStreamRange->SetStreamLength(startStreamRange->GetStreamLength() + endStreamRange->GetStreamLength());
    //    this->Remove(endIndex);
    //  }
    //}
  }

  return SUCCEEDED(result);
}

//bool CContinuousStreamRangeCollection::AddFreeSpace(int64_t start, int64_t length)
//{
//  bool result = ((start >= 0) && (length >= 0));
//
//  if (result)
//  {
//    bool merged = false;
//
//    for (unsigned int i = 0; i < this->Count(); i++)
//    {
//      CFreeSpace *freeSpace = this->GetItem(i);
//
//      if ((freeSpace->GetStart() + freeSpace->GetLength()) == start)
//      {
//        // free space to be added on end of freeSpace
//        freeSpace->SetLength(freeSpace->GetLength() + length);
//        merged = true;
//        break;
//      }
//      else if (freeSpace->GetStart() == (start + length))
//      {
//        // free space to be added on begin of freeSpace
//        freeSpace->SetStart(start);
//        freeSpace->SetLength(freeSpace->GetLength() + length);
//        merged = true;
//        break;
//      }
//    }
//
//    if (result && (!merged))
//    {
//      // not merged free space, just add to free space
//      HRESULT res = S_OK;
//      CFreeSpace *freeSpace = new CFreeSpace(&res);
//      result &= ((freeSpace != NULL) && SUCCEEDED(res));
//
//      if (result)
//      {
//        freeSpace->SetStart(start);
//        freeSpace->SetLength(length);
//
//        result &= this->Add(freeSpace);
//      }
//
//      CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(freeSpace));
//    }
//
//    while (result && merged)
//    {
//      merged = false;
//      unsigned int count = this->Count();
//
//      for (unsigned int i = 0; (result && (i < count) && (!merged)); i++)
//      {
//        CFreeSpace *first = this->GetItem(i);
//
//        for (unsigned int j = (i + 1); (result && (j < count) && (!merged)); j++)
//        {
//          CFreeSpace *second = this->GetItem(j);
//
//          if ((second->GetStart() + second->GetLength()) == first->GetStart())
//          {
//            first->SetStart(second->GetStart());
//            first->SetLength(first->GetLength() + second->GetLength());
//            this->Remove(j);
//            merged = true;
//            break;
//          }
//          else if (second->GetStart() == (first->GetStart() + first->GetLength()))
//          {
//            first->SetLength(first->GetLength() + second->GetLength());
//            this->Remove(j);
//            merged = true;
//            break;
//          }
//        }
//      }
//    }
//  }
//
//  return result;
//}

//bool CContinuousStreamRangeCollection::RemoveFreeSpace(unsigned int index, int64_t length)
//{
//  bool result = ((index < this->Count()) && (length >= 0));
//
//  if (result)
//  {
//    CFreeSpace *freeSpace = this->GetItem(index);
//    result &= (freeSpace->GetLength() >= length);
//
//    if (result)
//    {
//      if (freeSpace->GetLength() == length)
//      {
//        // the length is exactly same, remove free space item
//        result &= this->Remove(index);
//      }
//      else
//      {
//        // change free space item start and length, but do not remove (still some free space)
//        int64_t end = freeSpace->GetStart() + freeSpace->GetLength();
//        freeSpace->SetLength(freeSpace->GetLength() - length);
//        freeSpace->SetStart(end - freeSpace->GetLength());
//      }
//    }
//  }
//
//  return result;
//}

bool CContinuousStreamRangeCollection::GetItemInsertPosition(int64_t start, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = (this->Count() > 0);

    if (result)
    {
      unsigned int first = 0;
      unsigned int last = this->Count() - 1;
      result = false;

      while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
      {
        // compute middle index
        unsigned int middle = (first + last) / 2;

        // get stream range at middle index
        CContinuousStreamRange *streamRange = this->GetItem(middle);

        // tests stream range start position value
        if (start > streamRange->GetFilterStartPosition())
        {
          // start is bigger than stream range start position
          // search in top half
          first = middle + 1;
        }
        else if (start < streamRange->GetFilterStartPosition()) 
        {
          // start is lower than stream range start position
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found stream range with same starting position
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found stream range
        // it means that stream range belongs between first and last
        *startIndex = last;
        *endIndex = (first >= this->Count()) ? UINT_MAX : first;
        result = true;
      }
    }
    else
    {
      *startIndex = UINT_MAX;
      *endIndex = 0;
      result = true;
    }
  }

  return result;
}

//unsigned int CContinuousStreamRangeCollection::FindSuitableFreeSpace(int64_t length)
//{
//  unsigned int index = FREE_SPACE_NOT_FOUND;
//  int64_t minLength = INT64_MAX;
//
//  // we assume that free space collection contains small count of free spaces
//  for (unsigned int i = 0; i < this->Count(); i++)
//  {
//    CFreeSpace *freeSpace = this->GetItem(i);
//
//    if ((freeSpace->GetLength() >= length) && (minLength > freeSpace->GetLength()))
//    {
//      // last stored length is greater then current free space length
//      // we trying to find as closest free space length as possible
//
//      index = i;
//      minLength = freeSpace->GetLength();
//    }
//  }
//
//  return index;
//}

/* protected methods */

bool CContinuousStreamRangeCollection::Add(CContinuousStreamRange *item)
{
  if (item == NULL)
  {
    return false;
  }

  if (!this->EnsureEnoughSpace(this->Count() + 1))
  {
    return false;
  }

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->GetItemInsertPosition(item->GetFilterStartPosition(), &startIndex, &endIndex))
  {
    if (startIndex == endIndex)
    {
      // same continuous stream range exists in collection
      return false;
    }

    return __super::Insert(min(endIndex, this->Count()), item);
  }

  return true;
}

CContinuousStreamRange *CContinuousStreamRangeCollection::Clone(CContinuousStreamRange *item)
{
  return NULL;
}