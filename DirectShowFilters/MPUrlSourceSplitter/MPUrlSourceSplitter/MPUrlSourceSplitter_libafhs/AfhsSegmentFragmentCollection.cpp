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

#include "AfhsSegmentFragmentCollection.h"

CAfhsSegmentFragmentCollection::CAfhsSegmentFragmentCollection(HRESULT *result)
  : CCacheFileItemCollection(result)
{
  this->startSearchingIndex = 0;
  this->searchCount = 0;

  this->defaultBaseUrl = NULL;
  this->segmentFragmentUrlExtraParameters = NULL;
}

CAfhsSegmentFragmentCollection::~CAfhsSegmentFragmentCollection(void)
{
  FREE_MEM(this->defaultBaseUrl);
  FREE_MEM(this->segmentFragmentUrlExtraParameters);
}

/* get methods */

CAfhsSegmentFragment *CAfhsSegmentFragmentCollection::GetItem(unsigned int index)
{
  return (CAfhsSegmentFragment *)__super::GetItem(index);
}

unsigned int CAfhsSegmentFragmentCollection::GetFirstNotDownloadedSegmentFragment(unsigned int start)
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

unsigned int CAfhsSegmentFragmentCollection::GetSegmentFragmentIndexBetweenPositions(int64_t position)
{
  unsigned int index = UINT_MAX;

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  CAfhsSegmentFragment *zeroPositionFragment = this->GetItem(this->startSearchingIndex);

  if (zeroPositionFragment != NULL)
  {
    position += zeroPositionFragment->GetFragmentStartPosition();

    if (this->GetSegmentFragmentInsertPosition(position, &startIndex, &endIndex))
    {
      if (startIndex != UINT_MAX)
      {
        // if requested position is somewhere after start of segment fragments
        CAfhsSegmentFragment *segmentFragment = this->GetItem(startIndex);
        int64_t positionStart = segmentFragment->GetFragmentStartPosition();
        int64_t positionEnd = positionStart + segmentFragment->GetLength();

        if ((position >= positionStart) && (position < positionEnd))
        {
          // we found segment fragment
          index = startIndex;
        }
      }
    }
  }

  return index;
}

bool CAfhsSegmentFragmentCollection::GetSegmentFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = ((this->startSearchingIndex != SEGMENT_FRAGMENT_INDEX_NOT_SET) && (this->searchCount > 0));

    if (result)
    {
      unsigned int first = this->startSearchingIndex;
      unsigned int last = this->startSearchingIndex + this->searchCount - 1;
      result = false;

      while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
      {
        // compute middle index
        unsigned int middle = (first + last) / 2;

        // get segment fragment at middle index
        CAfhsSegmentFragment *segmentFragment = this->GetItem(middle);

        // compare segment fragment start to position
        if (position > segmentFragment->GetFragmentStartPosition())
        {
          // position is bigger than segment fragment start time
          // search in top half
          first = middle + 1;
        }
        else if (position < segmentFragment->GetFragmentStartPosition()) 
        {
          // position is lower than segment fragment start time
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found segment fragment with same starting time as position
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found segment fragment
        // it means that segment fragment with 'position' belongs between first and last
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

unsigned int CAfhsSegmentFragmentCollection::GetStartSearchingIndex(void)
{
  return this->startSearchingIndex;
}

unsigned int CAfhsSegmentFragmentCollection::GetSearchCount(void)
{
  return this->searchCount;
}

wchar_t *CAfhsSegmentFragmentCollection::GetSegmentFragmentUrl(CAfhsSegmentFragment *segmentFragment)
{
  return FormatString(L"%sSeg%d-Frag%d%s", this->defaultBaseUrl, segmentFragment->GetSegment(), segmentFragment->GetFragment(), (this->segmentFragmentUrlExtraParameters == NULL) ? L"" : this->segmentFragmentUrlExtraParameters);
}

const wchar_t *CAfhsSegmentFragmentCollection::GetBaseUrl(void)
{
  return this->defaultBaseUrl;
}

const wchar_t *CAfhsSegmentFragmentCollection::GetSegmentFragmentUrlExtraParameters(void)
{
  return this->segmentFragmentUrlExtraParameters;
}

/* set methods */

void CAfhsSegmentFragmentCollection::SetStartSearchingIndex(unsigned int startSearchingIndex)
{
  this->startSearchingIndex = startSearchingIndex;
}

void CAfhsSegmentFragmentCollection::SetSearchCount(unsigned int searchCount)
{
  this->searchCount = searchCount;
}

bool CAfhsSegmentFragmentCollection::SetBaseUrl(const wchar_t *baseUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->defaultBaseUrl, baseUrl);
}

bool CAfhsSegmentFragmentCollection::SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters)
{
  SET_STRING_RETURN_WITH_NULL(this->segmentFragmentUrlExtraParameters, segmentFragmentUrlExtraParameters);
}

/* other methods */

bool CAfhsSegmentFragmentCollection::Insert(unsigned int position, CCacheFileItem *item)
{
  bool result = __super::Insert(position, item);

  if (result)
  {
    if (position <= this->startSearchingIndex)
    {
      this->startSearchingIndex++;
    }
  }

  return result;
}

/* old implementation */

//int CSegmentFragmentCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
//{
//  bool invariant = (*(bool *)context);
//
//  if (invariant)
//  {
//    return _wcsicmp(firstKey, secondKey);
//  }
//  else
//  {
//    return wcscmp(firstKey, secondKey);
//  }
//}
//
//const wchar_t *CSegmentFragmentCollection::GetKey(CSegmentFragment *item)
//{
//  return L"";
//}
//
//CSegmentFragment *CSegmentFragmentCollection::Clone(CSegmentFragment *item)
//{
//  return item->Clone();
//}
//
//CSegmentFragment *CSegmentFragmentCollection::GetSegmentFragment(const wchar_t *url, bool invariant)
//{
//  return this->GetItem(url, (void *)&invariant);
//}
//
//unsigned int CSegmentFragmentCollection::GetFirstNotDownloadedSegmentFragment(unsigned int start)
//{
//  unsigned int result = UINT_MAX;
//
//  for (unsigned int i = start; i < this->Count(); i++)
//  {
//    if (!this->GetItem(i)->IsDownloaded())
//    {
//      result = i;
//      break;
//    }
//  }
//
//  return result;
//}
//
//unsigned int CSegmentFragmentCollection::GetFirstNotProcessedSegmentFragment(unsigned int start)
//{
//  unsigned int result = UINT_MAX;
//
//  for (unsigned int i = start; i < this->Count(); i++)
//  {
//    if (this->GetItem(i)->IsDownloaded() && (!this->GetItem(i)->IsProcessed()))
//    {
//      result = i;
//      break;
//    }
//  }
//
//  return result;
//}
//
//wchar_t *CSegmentFragmentCollection::GetSegmentFragmentUrl(CSegmentFragment *segmentFragment)
//{
//  return FormatString(L"%sSeg%d-Frag%d%s", this->defaultBaseUrl, segmentFragment->GetSegment(), segmentFragment->GetFragment(), (this->segmentFragmentUrlExtraParameters == NULL) ? L"" : this->segmentFragmentUrlExtraParameters);
//}
//
//const wchar_t *CSegmentFragmentCollection::GetBaseUrl(void)
//{
//  return this->defaultBaseUrl;
//}
//
//bool CSegmentFragmentCollection::SetBaseUrl(const wchar_t *baseUrl)
//{
//  SET_STRING_RETURN_WITH_NULL(this->defaultBaseUrl, baseUrl);
//}
//
//const wchar_t *CSegmentFragmentCollection::GetSegmentFragmentUrlExtraParameters(void)
//{
//  return this->segmentFragmentUrlExtraParameters;
//}
//
//bool CSegmentFragmentCollection::SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters)
//{
//  SET_STRING_RETURN_WITH_NULL(this->segmentFragmentUrlExtraParameters, segmentFragmentUrlExtraParameters);
//}