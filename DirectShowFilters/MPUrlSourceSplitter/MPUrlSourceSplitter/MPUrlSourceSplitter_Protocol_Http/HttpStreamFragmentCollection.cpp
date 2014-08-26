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

#include "HttpStreamFragmentCollection.h"

CHttpStreamFragmentCollection::CHttpStreamFragmentCollection(HRESULT *result)
  : CCacheFileItemCollection(result)
{
}

CHttpStreamFragmentCollection::~CHttpStreamFragmentCollection(void)
{
}

/* get methods */

CHttpStreamFragment *CHttpStreamFragmentCollection::GetItem(unsigned int index)
{
  return (CHttpStreamFragment *)__super::GetItem(index);
}

unsigned int CHttpStreamFragmentCollection::GetFirstNotDownloadedItemIndexAfterStartPosition(int64_t position)
{
  unsigned int result = UINT_MAX;

  // get position to insert item index
  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->GetItemInsertPosition(position, &startIndex, &endIndex))
  {
    if (this->indexNotDownloaded->GetItemInsertPosition(endIndex, &startIndex, &endIndex))
    {
      result = this->indexNotDownloaded->GetItem(endIndex);
    }
  }

  return result;
}

/* set methods */

/* other methods */

bool CHttpStreamFragmentCollection::Add(CHttpStreamFragment *item)
{
  bool result = (item != NULL);

  CHECK_CONDITION_EXECUTE(result, result &= this->EnsureEnoughSpace(this->Count() + 1));

  if (result)
  {
    unsigned int startIndex = 0;
    unsigned int endIndex = 0;

    if (this->GetItemInsertPosition(item->GetStart(), &startIndex, &endIndex))
    {
      // check if media packet exists in collection
      result &= (startIndex != endIndex);

      if (result)
      {
        result &= __super::Insert(min(endIndex, this->Count()), item);
      }
    }
  }

  return result;
}

unsigned int CHttpStreamFragmentCollection::GetStreamFragmentIndexBetweenPositions(int64_t position)
{
  unsigned int index = UINT_MAX;

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->GetItemInsertPosition(position, &startIndex, &endIndex))
  {
    if (startIndex != UINT_MAX)
    {
      // if requested position is somewhere after start of stream fragments
      CHttpStreamFragment *fragment = this->GetItem(startIndex);
      int64_t positionStart = fragment->GetStart();
      int64_t positionEnd = positionStart + fragment->GetLength();

      if ((position >= positionStart) && (position < positionEnd))
      {
        // we found stream fragment
        index = startIndex;
      }
    }
  }

  return index;
}

bool CHttpStreamFragmentCollection::GetItemInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex)
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

        // get stream fragment at middle index
        CHttpStreamFragment *fragment = this->GetItem(middle);

        // compare stream fragment start to position
        if (position > fragment->GetStart())
        {
          // position is bigger than stream fragment start time
          // search in top half
          first = middle + 1;
        }
        else if (position < fragment->GetStart()) 
        {
          // position is lower than stream fragment start time
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found stream fragment with same starting time as position
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found stream fragment
        // it means that stream fragment with 'position' belongs between first and last
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

/* protected methods */
