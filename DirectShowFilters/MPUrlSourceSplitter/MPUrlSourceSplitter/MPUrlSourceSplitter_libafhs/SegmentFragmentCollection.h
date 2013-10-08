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

#include "KeyedCollection.h"
#include "SegmentFragment.h"

class CSegmentFragmentCollection : public CKeyedCollection<CSegmentFragment, const wchar_t *>
{
public:
  CSegmentFragmentCollection(void);
  ~CSegmentFragmentCollection(void);

  // get the segment and fragment from collection with specified url
  // @param name : the URL of segment and fragment to find
  // @param invariant : specifies if segment and fragment URL shoud be find with invariant casing
  // @return : the reference to segment and fragment or NULL if not find
  CSegmentFragment *GetSegmentFragment(const wchar_t *url, bool invariant);

  // gets first not downloaded segment and fragment
  // @param requested : start index for searching
  // @return : index of first not downloaded segment and fragment or UINT_MAX if not exists
  unsigned int GetFirstNotDownloadedSegmentFragment(unsigned int start);

  // gets first not processed segment and fragment
  // @param requested : start index for searching
  // @return : index of first not processed segment and fragment or UINT_MAX if not exists
  unsigned int GetFirstNotProcessedSegmentFragment(unsigned int start);

  // gets default url for segment and fragment
  // @param segmentFragment : the segment and fragment to get default url
  // @return : default url for segment and fragment or NULL if error
  wchar_t *GetSegmentFragmentUrl(CSegmentFragment *segmentFragment);

  // gets default base url for all segments and fragment
  // @return : default base url for all segments and fragments or NULL if error
  const wchar_t *GetBaseUrl(void);

  // sets default base url for all segments and fragments
  // @param baseUrl : default base url to set
  // @return : true if successful, false otherwise
  bool SetBaseUrl(const wchar_t *baseUrl);

  // gets extra parameters for all segments and fragment
  // @return : extra parameters for all segments and fragments or NULL if error
  const wchar_t *GetSegmentFragmentUrlExtraParameters(void);

  // sets extra parameters for all segments and fragments
  // @param segmentFragmentUrlExtraParameters : segment and fragment URL extra parameters to set
  // @return : true if successful, false otherwise
  bool SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters);

protected:

  // holds default base url for all segments and fragments
  wchar_t *defaultBaseUrl;

  // holds extra parameters which are added to all segments and fragments
  wchar_t *segmentFragmentUrlExtraParameters;

  // compare two item keys
  // @param firstKey : the first item key to compare
  // @param secondKey : the second item key to compare
  // @param context : the reference to user defined context
  // @return : 0 if keys are equal, lower than zero if firstKey is lower than secondKey, greater than zero if firstKey is greater than secondKey
  int CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context);

  // gets key for item
  // @param item : the item to get key
  // @return : the key of item
  const wchar_t *GetKey(CSegmentFragment *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CSegmentFragment *Clone(CSegmentFragment *item);
};

#endif