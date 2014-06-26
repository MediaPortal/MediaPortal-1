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

#include "RtspStreamFragmentCollection.h"

CRtspStreamFragmentCollection::CRtspStreamFragmentCollection(HRESULT *result)
  : CCacheFileItemCollection(result)
{
  this->startSearchingIndex = STREAM_FRAGMENT_INDEX_NOT_SET;
  this->searchCount = 0;
}

CRtspStreamFragmentCollection::~CRtspStreamFragmentCollection(void)
{
}

/* get methods */

CRtspStreamFragment *CRtspStreamFragmentCollection::GetItem(unsigned int index)
{
  return (CRtspStreamFragment *)__super::GetItem(index);
}

unsigned int CRtspStreamFragmentCollection::GetFirstNotDownloadedStreamFragment(unsigned int start)
{
  unsigned int result = UINT_MAX;

  for (unsigned int i = start; i < this->itemCount; i++)
  {
    if (!this->GetItem(i)->IsDownloaded())
    {
      result = i;
      break;
    }
  }

  return result;
}

unsigned int CRtspStreamFragmentCollection::GetStreamFragmentIndexBetweenPositions(int64_t position)
{
  unsigned int index = UINT_MAX;

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  CRtspStreamFragment *zeroPositionFragment = this->GetItem(this->startSearchingIndex);

  if (zeroPositionFragment != NULL)
  {
    position += zeroPositionFragment->GetFragmentStartPosition();

    if (this->GetStreamFragmentInsertPosition(position, &startIndex, &endIndex))
    {
      if (startIndex != UINT_MAX)
      {
        // if requested position is somewhere after start of stream fragments
        CRtspStreamFragment *streamFragment = this->GetItem(startIndex);
        int64_t positionStart = streamFragment->GetFragmentStartPosition();
        int64_t positionEnd = positionStart + streamFragment->GetLength();

        if ((position >= positionStart) && (position < positionEnd))
        {
          // we found stream fragment
          index = startIndex;
        }
      }
    }
  }

  return index;
}

bool CRtspStreamFragmentCollection::GetStreamFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = ((this->startSearchingIndex != STREAM_FRAGMENT_INDEX_NOT_SET) && (this->searchCount > 0));

    if (result)
    {
      unsigned int first = this->startSearchingIndex;
      unsigned int last = this->startSearchingIndex + this->searchCount - 1;
      result = false;

      while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
      {
        // compute middle index
        unsigned int middle = (first + last) / 2;

        // get stream fragment at middle index
        CRtspStreamFragment *streamFragment = this->GetItem(middle);

        // compare stream fragment start to position
        if (position > streamFragment->GetFragmentStartPosition())
        {
          // position is bigger than stream fragment start time
          // search in top half
          first = middle + 1;
        }
        else if (position < streamFragment->GetFragmentStartPosition()) 
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
        *endIndex = (first >= (this->startSearchingIndex + this->searchCount)) ? UINT_MAX : first;
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

unsigned int CRtspStreamFragmentCollection::GetStartSearchingIndex(void)
{
  return this->startSearchingIndex;
}

unsigned int CRtspStreamFragmentCollection::GetSearchCount(void)
{
  return this->searchCount;
}

/* set methods */

void CRtspStreamFragmentCollection::SetStartSearchingIndex(unsigned int startSearchingIndex)
{
  this->startSearchingIndex = startSearchingIndex;
}

void CRtspStreamFragmentCollection::SetSearchCount(unsigned int searchCount)
{
  this->searchCount = searchCount;
}

/* other methods */

bool CRtspStreamFragmentCollection::Insert(unsigned int position, CCacheFileItem *item)
{
  bool result = __super::Insert(position, item);

  if (result)
  {
    if (position < this->startSearchingIndex)
    {
      this->startSearchingIndex++;
    }
  }

  return result;
}