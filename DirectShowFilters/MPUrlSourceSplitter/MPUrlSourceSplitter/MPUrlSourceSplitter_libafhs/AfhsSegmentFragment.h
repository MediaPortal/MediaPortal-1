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

#ifndef __SEGMENT_FRAGMENT_DEFINED
#define __SEGMENT_FRAGMENT_DEFINED

#include "HttpDownloadRequest.h"
#include "HttpDownloadResponse.h"
#include "CacheFileItem.h"

#define AFHS_SEGMENT_FRAGMENT_FLAG_NONE                               CACHE_FILE_ITEM_FLAG_NONE

#define AFHS_SEGMENT_FRAGMENT_FLAG_ENCRYPTED                          (1 << (CACHE_FILE_ITEM_FLAG_LAST + 0))
#define AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED                          (1 << (CACHE_FILE_ITEM_FLAG_LAST + 1))
#define AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET     (1 << (CACHE_FILE_ITEM_FLAG_LAST + 2))

#define AFHS_SEGMENT_FRAGMENT_FLAG_LAST                               (CACHE_FILE_ITEM_FLAG_LAST + 3)

#define AFHS_SEGMENT_FRAGMENT_START_POSITION_NOT_SET                  -1

class CAfhsSegmentFragment : public CCacheFileItem
{
public:
  // initializes a new instance of CAfhsSegmentFragment class
  CAfhsSegmentFragment(HRESULT *result, unsigned int segment, unsigned int fragment, int64_t fragmentTimestamp);
  CAfhsSegmentFragment(HRESULT *result, unsigned int segment, unsigned int fragment);

  // destructor
  ~CAfhsSegmentFragment(void);

  /* get methods */

  // gets fragment timestamp
  // @return : fragment timestamp
  int64_t GetFragmentTimestamp(void);

  // gets fragment start position within stream
  // @return : fragment start position within stream or AFHS_SEGMENT_FRAGMENT_START_POSITION_NOT_SET if not set
  int64_t GetFragmentStartPosition(void);

  // gets segment ID
  // @return : segment ID
  unsigned int GetSegment(void);

  // gets fragment ID
  // @return : fragment ID
  unsigned int GetFragment(void);

  /* set methods */

  // sets if segment and fragment is decrypted
  // @param decrypted : true if segment and fragment is decrypted, false otherwise
  // @param segmentFragmentItemIndex : the index of segment fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetDecrypted(bool decrypted, unsigned int segmentFragmentItemIndex);

  // sets if segment and fragment is encrypted
  // @param encrypted : true if segment and fragment is encrypted, false otherwise
  // @param segmentFragmentItemIndex : the index of segment fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetEncrypted(bool encrypted, unsigned int segmentFragmentItemIndex);

  // sets fragment start position
  // @param fragmentStartPosition : fragment start position to set
  void SetFragmentStartPosition(int64_t fragmentStartPosition);

  // sets if fragment contains header or meta packet
  // @param containsHeaderOrMetaPacket : true if fragment contains header or meta packet, false otherwise
  void SetContainsHeaderOrMetaPacket(bool containsHeaderOrMetaPacket);

  /* other methods */

  // tests if fragment is decrypted
  // @return : true if decrypted, false otherwise
  bool IsDecrypted(void);

  // tests if fragment is encrypted
  // @return : true if encrypted, false otherwise
  bool IsEncrypted(void);

  // tests if fragment has set start position
  // @return : true if fragment has set start position, false otherwise
  bool IsSetFragmentStartPosition(void);

  // tests if fragment contains header or meta packet
  // @return : true if fragment contains header or meta packet, false otherwise
  bool ContainsHeaderOrMetaPacket(void);

private:
  // stores segment ID
  unsigned int segment;
  // stores fragment ID
  unsigned int fragment;
  // holds fragment timestamp
  int64_t fragmentTimestamp;
  // holds fragment start position within stream
  int64_t fragmentStartPosition;

  /* methods */

  // gets new instance of cache file item
  // @return : new cache file item instance or NULL if error
  virtual CFastSearchItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the cache file item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CFastSearchItem *item);
};

#endif