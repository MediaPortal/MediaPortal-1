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

#ifndef __AFHS_SEGMENT_FRAGMENT_DEFINED
#define __AFHS_SEGMENT_FRAGMENT_DEFINED

#include "HttpDownloadRequest.h"
#include "HttpDownloadResponse.h"
#include "StreamFragment.h"

#define AFHS_SEGMENT_FRAGMENT_FLAG_NONE                               STREAM_FRAGMENT_FLAG_NONE

#define AFHS_SEGMENT_FRAGMENT_FLAG_ENCRYPTED                          (1 << (STREAM_FRAGMENT_FLAG_LAST + 0))
#define AFHS_SEGMENT_FRAGMENT_FLAG_DECRYPTED                          (1 << (STREAM_FRAGMENT_FLAG_LAST + 1))
#define AFHS_SEGMENT_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET     (1 << (STREAM_FRAGMENT_FLAG_LAST + 2))

#define AFHS_SEGMENT_FRAGMENT_FLAG_LAST                               (STREAM_FRAGMENT_LAST + 3)

class CAfhsSegmentFragment : public CStreamFragment
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