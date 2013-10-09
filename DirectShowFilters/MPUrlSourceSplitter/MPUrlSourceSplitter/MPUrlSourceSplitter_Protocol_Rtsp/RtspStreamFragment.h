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

#ifndef __RTSP_STREAM_FRAGMENT_DEFINED
#define __RTSP_STREAM_FRAGMENT_DEFINED

#include "LinearBuffer.h"
#include "RtpPacket.h"

#include <stdint.h>

#define RTSP_STREAM_FRAGMENT_FLAG_NONE                                0x00000000
#define RTSP_STREAM_FRAGMENT_FLAG_DOWNLOADED                          0x00000001
#define RTSP_STREAM_FRAGMENT_FLAG_STORED_TO_FILE                      0x00000002

class CRtspStreamFragment
{
public:
  // initializes a new instance of CSegmentFragment class
  CRtspStreamFragment(uint64_t fragmentStartTimestamp);

  // destructor
  ~CRtspStreamFragment(void);

  /* get methods */

  // gets fragment start timestamp in ms
  // @return : fragment start timestamp in ms
  uint64_t GetFragmentStartTimestamp(void);

  // gets fragment end timestamp in ms
  // @return : fragment end timestamp in ms or UINT64_MAX if not specified
  uint64_t GetFragmentEndTimestamp(void);

  // gets position of start of segment and fragment within store file
  // @return : file position or -1 if error
  int64_t GetStoreFilePosition(void);

  // gets the length of segment and fragment data
  // @return : the length of segment and fragment data
  unsigned int GetLength(void);

  // gets received data
  // @return : received data
  CLinearBuffer *GetReceivedData(void);

  /* set methods */

  // sets if segment and fragment is downloaded
  // @param downloaded : true if segment and fragment is downloaded
  void SetDownloaded(bool downloaded);

  // sets position within store file
  // if segment and fragment is stored than linear buffer is deleted
  // if store file path is cleared (NULL) than linear buffer is created
  // @param position : the position of start of segment and fragment within store file or (-1) if segment and fragment is in memory
  void SetStoredToFile(int64_t position);

  // sets fragment start timestamp in ms
  // @param fragmentStartTimestamp : the fragment start timestamp in ms to set
  void SetFragmentStartTimestamp(uint64_t fragmentStartTimestamp);

  // sets fragment end timestamp in ms
  // @param fragmentEndTimestamp : the fragment end timestamp in ms to set
  void SetFragmentEndTimestamp(uint64_t fragmentEndTimestamp);

  /* other methods */

  // tests if media packet is stored to file
  // @return : true if media packet is stored to file, false otherwise
  bool IsStoredToFile(void);

  // tests if fragment is downloaded
  // @return : true if downloaded, false otherwise
  bool IsDownloaded(void);

  // tests if specific combination of flags is set
  // @return : true if specific combination of flags is set, false otherwise
  bool IsFlags(unsigned int flags);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CRtspStreamFragment *Clone(void);

private:
  // holds various flags
  unsigned int flags;
  // stores fragment start timestamp in ms
  uint64_t fragmentStartTimestamp;
  // stores fragment end timestamp in ms
  uint64_t fragmentEndTimestamp;
  // posittion in store file
  int64_t storeFilePosition;
  // the length of segment and fragment data
  unsigned int length;
  // holds buffer with received data
  CLinearBuffer *receivedData;
};

#endif