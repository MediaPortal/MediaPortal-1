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

#ifndef __SEGMENT_FRAGMENT_COLLECTION_DEFINED
#define __SEGMENT_FRAGMENT_COLLECTION_DEFINED

#include "CacheFileItemCollection.h"
#include "AfhsSegmentFragment.h"

#define SEGMENT_FRAGMENT_INDEX_NOT_SET                                UINT_MAX

class CAfhsSegmentFragmentCollection : public CCacheFileItemCollection
{
public:
  CAfhsSegmentFragmentCollection(HRESULT *result);
  ~CAfhsSegmentFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CAfhsSegmentFragment *GetItem(unsigned int index);

  // gets first not downloaded segment fragment
  // @param requested : start index for searching
  // @return : index of first not downloaded segment fragment or UINT_MAX if not exists
  unsigned int GetFirstNotDownloadedSegmentFragment(unsigned int start);

  // gets index of stream segment where position is between start position and end position
  // @param position : the position between start position and end position
  // @return : index of segment fragment or UINT_MAX if not exists
  unsigned int GetSegmentFragmentIndexBetweenPositions(int64_t position);

  // returns indexes where segment fragment have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => segment fragment have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => segment fragment have to be placed on end
  // startIndex == endIndex => segment fragment with same start position exists in collection (index of segment fragment is startIndex)
  // segment fragment have to be placed between startIndex and endIndex
  // @param position : the start position to compare
  // @param startIndex : reference to variable which holds start index where segment fragment have to be placed
  // @param endIndex : reference to variable which holds end index where segment fragment have to be placed
  // @return : true if successful, false otherwise
  bool GetSegmentFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex);

  // gets start index of segment fragment to start searching for specific position
  // @return : start index of segment fragment to start searching for specific position or SEGMENT_FRAGMENT_INDEX_NOT_SET if not set
  unsigned int GetStartSearchingIndex(void);

  // gets count of segment fragments to search for specific position
  // @return : count of segment fragments to search for specific position
  unsigned int GetSearchCount(void);

  // gets default url for segment and fragment
  // @param segmentFragment : the segment and fragment to get default url
  // @return : default url for segment and fragment or NULL if error
  wchar_t *GetSegmentFragmentUrl(CAfhsSegmentFragment *segmentFragment);

  // gets default base url for all segments and fragment
  // @return : default base url for all segments and fragments or NULL if error
  const wchar_t *GetBaseUrl(void);

  // gets extra parameters for all segments and fragment
  // @return : extra parameters for all segments and fragments or NULL if error
  const wchar_t *GetSegmentFragmentUrlExtraParameters(void);

  /* set methods */

  // sets start index of segment fragment to start searching for specific position
  // @param startSearchingIndex : start index of segment fragment to start searching for specific position or SEGMENT_FRAGMENT_INDEX_NOT_SET if not set
  void SetStartSearchingIndex(unsigned int startSearchingIndex);

  // sets count of segment fragments to search for specific position
  // @param searchCount : count of segment fragments to search for specific position
  void SetSearchCount(unsigned int searchCount);

  // sets default base url for all segments and fragments
  // @param baseUrl : default base url to set
  // @return : true if successful, false otherwise
  bool SetBaseUrl(const wchar_t *baseUrl);

  // sets extra parameters for all segments and fragments
  // @param segmentFragmentUrlExtraParameters : segment and fragment URL extra parameters to set
  // @return : true if successful, false otherwise
  bool SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters);

  /* other methods */

  // insert item to collection
  // @param position : zero-based position to insert new item
  // @param item : item to insert
  // @result : true if successful, false otherwise
  virtual bool Insert(unsigned int position, CCacheFileItem *item);

  /* old implementation */

  //// get the segment and fragment from collection with specified url
  //// @param name : the URL of segment and fragment to find
  //// @param invariant : specifies if segment and fragment URL shoud be find with invariant casing
  //// @return : the reference to segment and fragment or NULL if not find
  //CSegmentFragment *GetSegmentFragment(const wchar_t *url, bool invariant);

  //// gets first not downloaded segment and fragment
  //// @param requested : start index for searching
  //// @return : index of first not downloaded segment and fragment or UINT_MAX if not exists
  //unsigned int GetFirstNotDownloadedSegmentFragment(unsigned int start);

  //// gets first not processed segment and fragment
  //// @param requested : start index for searching
  //// @return : index of first not processed segment and fragment or UINT_MAX if not exists
  //unsigned int GetFirstNotProcessedSegmentFragment(unsigned int start);

  //// gets default url for segment and fragment
  //// @param segmentFragment : the segment and fragment to get default url
  //// @return : default url for segment and fragment or NULL if error
  //wchar_t *GetSegmentFragmentUrl(CSegmentFragment *segmentFragment);

  //// gets default base url for all segments and fragment
  //// @return : default base url for all segments and fragments or NULL if error
  //const wchar_t *GetBaseUrl(void);

  //// sets default base url for all segments and fragments
  //// @param baseUrl : default base url to set
  //// @return : true if successful, false otherwise
  //bool SetBaseUrl(const wchar_t *baseUrl);

  //// gets extra parameters for all segments and fragment
  //// @return : extra parameters for all segments and fragments or NULL if error
  //const wchar_t *GetSegmentFragmentUrlExtraParameters(void);

  //// sets extra parameters for all segments and fragments
  //// @param segmentFragmentUrlExtraParameters : segment and fragment URL extra parameters to set
  //// @return : true if successful, false otherwise
  //bool SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters);

protected:
  // holds start index of segment fragment to start searching for specific position
  unsigned int startSearchingIndex;
  // holds segment fragments count to search for specific position
  unsigned int searchCount;
  // holds default base url for all segments and fragments
  wchar_t *defaultBaseUrl;
  // holds extra parameters which are added to all segments and fragments
  wchar_t *segmentFragmentUrlExtraParameters;

  /* old implementation */

  //// holds default base url for all segments and fragments
  //wchar_t *defaultBaseUrl;

  //// holds extra parameters which are added to all segments and fragments
  //wchar_t *segmentFragmentUrlExtraParameters;

  //// compare two item keys
  //// @param firstKey : the first item key to compare
  //// @param secondKey : the second item key to compare
  //// @param context : the reference to user defined context
  //// @return : 0 if keys are equal, lower than zero if firstKey is lower than secondKey, greater than zero if firstKey is greater than secondKey
  //int CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context);

  //// gets key for item
  //// @param item : the item to get key
  //// @return : the key of item
  //const wchar_t *GetKey(CSegmentFragment *item);

  //// clones specified item
  //// @param item : the item to clone
  //// @return : deep clone of item or NULL if not implemented
  //CSegmentFragment *Clone(CSegmentFragment *item);
};

#endif