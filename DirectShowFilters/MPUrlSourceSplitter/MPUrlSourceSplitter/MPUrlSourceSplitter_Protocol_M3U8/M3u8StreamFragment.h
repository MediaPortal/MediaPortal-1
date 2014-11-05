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

#ifndef __M3U8_SEGMENT_FRAGMENT_DEFINED
#define __M3U8_SEGMENT_FRAGMENT_DEFINED

#include "StreamFragment.h"

#define M3U8_STREAM_FRAGMENT_FLAG_NONE                                STREAM_FRAGMENT_FLAG_NONE

#define M3U8_STREAM_FRAGMENT_FLAG_ENCRYPTED                           (1 << (STREAM_FRAGMENT_FLAG_LAST + 0))
#define M3U8_STREAM_FRAGMENT_FLAG_END_OF_STREAM                       (1 << (STREAM_FRAGMENT_FLAG_LAST + 1))

#define M3U8_STREAM_FRAGMENT_FLAG_LAST                                (STREAM_FRAGMENT_LAST + 2)

class CM3u8StreamFragment : public CStreamFragment
{
public:
  // initializes a new instance of CM3u8StreamFragment class
  CM3u8StreamFragment(HRESULT *result, const wchar_t *uri, unsigned int fragment, int64_t fragmentTimestamp, unsigned int duration);

  // destructor
  ~CM3u8StreamFragment(void);

  /* get methods */

  // gets fragment ID
  // @return : fragment ID
  unsigned int GetFragment(void);

  // gets fragment timestamp
  // @return : fragment timestamp
  int64_t GetFragmentTimestamp(void);

  // gets stream fragment URI
  // @return : segment ID
  const wchar_t *GetUri(void);

  // gets stream fragment duration in ms
  // @return : stream fragment duration in ms
  unsigned int GetDuration(void);

  // gets offset of range request
  // @return : offset or UINT_MAX if not specified
  unsigned int GetByteRangeOffset(void);

  // gets length of range request
  // @return : length or UINT_MAX if not specified
  unsigned int GetByteRangeLength(void);

  /* set methods */

  // sets if fragment is encrypted
  // @param encrypted : true if after fragment is discontinuity, false otherwise
  void SetEncrypted(bool ecnrypted);

  // sets if fragment is end of stream
  // @param endOfStream : true if after fragment is end of stream, false otherwise
  void SetEndOfStream(bool endOfStream);

  // sets offset of range request
  // @param offset : offset or UINT_MAX if not specified
  void SetByteRangeOffset(unsigned int offset);

  // sets length of range request
  // @param length : length or UINT_MAX if not specified
  void SetByteRangeLength(unsigned int length);

  /* other methods */

  // tests if fragment is encrypted
  // @return : true if encrypted, false otherwise
  bool IsEncrypted(void);

  // tests if fragment is end of stream
  // @return : true if end of stream, false otherwise
  bool IsEndOfStream(void);

private:
  // stores fragment URI
  wchar_t *uri;
  // stores fragment ID
  unsigned int fragment;
  // holds fragment timestamp
  int64_t fragmentTimestamp;
  // holds duration in ms
  unsigned int duration;
  // holds offset of range request
  unsigned int byteRangeOffset;
  // holds length of range request
  unsigned int byteRangeLength;

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