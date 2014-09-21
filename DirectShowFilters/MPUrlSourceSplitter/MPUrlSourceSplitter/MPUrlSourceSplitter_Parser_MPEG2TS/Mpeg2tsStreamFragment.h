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

#ifndef __MPEG2TS_STREAM_FRAGMENT_DEFINED
#define __MPEG2TS_STREAM_FRAGMENT_DEFINED

#include "StreamFragment.h"

#define MPEG2TS_STREAM_FRAGMENT_FLAG_NONE                             STREAM_FRAGMENT_FLAG_NONE

#define MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN                  (1 << (STREAM_FRAGMENT_FLAG_LAST + 0))
#define MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED                          (1 << (STREAM_FRAGMENT_FLAG_LAST + 1))
#define MPEG2TS_STREAM_FRAGMENT_FLAG_PARTIALLY_PROCESSED              (1 << (STREAM_FRAGMENT_FLAG_LAST + 2))

#define MPEG2TS_STREAM_FRAGMENT_FLAG_LAST                             (STREAM_FRAGMENT_FLAG_LAST + 3)

class CMpeg2tsStreamFragment : public CStreamFragment
{
public:
  CMpeg2tsStreamFragment(HRESULT *result);
  virtual ~CMpeg2tsStreamFragment(void);

  /* get methods */

  // gets request start position within protocol stream
  // @return : request start position within protocol stream or STREAM_FRAGMENT_START_POSITION_NOT_SET if not set
  int64_t GetRequestStartPosition(void);

  /* set methods */

  // sets request start position within protocol stream
  // @param requestStartPosition : request start position within protocol stream to set
  void SetRequestStartPosition(int64_t requestStartPosition);

  // sets ready for align flag
  // @param readyForAlign : true if ready for align, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetReadyForAlign(bool readyForAlign, unsigned int streamFragmentIndex);

  // sets aligned flag
  // @param aligned : true if aligned, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetAligned(bool aligned, unsigned int streamFragmentIndex);

  // sets partially processed flag
  // @param partiallyProcessed : true if partially processed, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetPartiallyProcessed(bool partiallyProcessed, unsigned int streamFragmentIndex);

  /* other methods */

  // tests if fragment has set request start position (in protocol stream)
  // @return : true if fragment has set request start position, false otherwise
  bool IsSetRequestStartPosition(void);

  // tests if fragment is ready for aligning
  // @return : true if fragment is ready for aligning, false otherwise
  bool IsReadyForAlign(void);

  // tests if fragment is aligned
  // @return : true if fragment is aligned, false otherwise
  bool IsAligned(void);

  // tests if fragment is partially processed (some unprocessed MPEG2 TS packets are still in fragment)
  // @return : true if fragment is partially processed, false otherwise
  bool IsPartiallyProcessed(void);

protected:
  // holds request start position within protocol stream
  int64_t requestStartPosition;

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