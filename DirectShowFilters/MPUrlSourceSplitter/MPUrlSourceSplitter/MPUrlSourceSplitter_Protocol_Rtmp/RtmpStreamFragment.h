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

#ifndef __RTMP_STREAM_FRAGMENT_DEFINED
#define __RTMP_STREAM_FRAGMENT_DEFINED

#include "LinearBuffer.h"

class CRtmpStreamFragment
{
public:
  // creates new instance of CRtmpStreamFragment class with specified key frame timestamp
  CRtmpStreamFragment(void);
  ~CRtmpStreamFragment(void);

  /* get methods */

  // gets fragment start timestamp in ms
  // @return : fragment start timestamp in ms
  uint64_t GetFragmentStartTimestamp(void);

  // gets fragment end timestamp in ms
  // @return : fragment end timestamp in ms
  uint64_t GetFragmentEndTimestamp(void);

  // gets position of start of fragment within store file
  // @return : file position or -1 if error
  int64_t GetStoreFilePosition(void);

  // gets the length of fragment data
  // @return : the length of fragment data
  unsigned int GetLength(void);

  // gets linear buffer
  // @return : linear buffer or NULL if error or fragment is stored to file
  CLinearBuffer *GetBuffer();

  // gets packet correction (positive or negative)
  // @return : packet correction
  int GetPacketCorrection(void);

  /* set methods */

  // sets fragment start timestamp
  // @param fragmentStartTimestamp : fragment start timestamp in ms to set
  // @param setStartTimestamp : specifies if start timestamp is set by received data
  void SetFragmentStartTimestamp(uint64_t fragmentStartTimestamp, bool setStartTimestamp);

  // sets fragment end timestamp
  // @param fragmentEndTimestamp : fragment end timestamp in ms to set
  void SetFragmentEndTimestamp(uint64_t fragmentEndTimestamp);

  // sets if fragment is downloaded
  // @param downloaded : true if fragment is downloaded
  void SetDownloaded(bool downloaded);

  // sets position within store file
  // if segment and fragment is stored than linear buffer is deleted
  // if store file path is cleared (NULL) than linear buffer is created
  // @param position : the position of start of segment and fragment within store file or (-1) if segment and fragment is in memory
  void SetStoredToFile(int64_t position);

  // sets if fragment is first fragment after seek (it can be incomplete)
  // @param seeked : true if fragment is first after seek, false otherwise
  void SetSeeked(bool seeked);

  // sets if fragment has incorrect timestamps (it happen after seeking)
  // @param hasIncorrectTimestamps : true if fragment has incorect timestamps, false otherwise
  void SetIncorrectTimestamps(bool hasIncorrectTimestamps);

  // sets packet correction (positive or negative)
  // @param packetCorrection : packet correction
  void SetPacketCorrection(int packetCorrection);

  /* other methods */

  // tests if media packet is stored to file
  // @return : true if media packet is stored to file, false otherwise
  bool IsStoredToFile(void);

  // tests if segment and fragment is downloaded
  // @return : true if downloaded, false otherwise
  bool IsDownloaded(void);

  // tests if start timestamp was set by received data
  bool IsStartTimestampSet(void);

  // tests if fragment is first fragment after seek (it can be incomplete)
  // @return : true if fragment is first after seek, false otherwise
  bool IsSeeked(void);

  // tests if fragment has incorrect timestamps (it happen after seeking)
  // @return : true if fragment has incorect timestamps, false otherwise
  bool HasIncorrectTimestamps(void);

private:
  // stores fragment start timestamp
  uint64_t fragmentStartTimestamp;
  // stores fragment end timestamp
  uint64_t fragmentEndTimestamp;
  // stores if fragment is downloaded
  bool downloaded;
  // posittion in store file
  int64_t storeFilePosition;
  // the length of fragment data
  unsigned int length;
  // internal linear buffer for fragment data
  CLinearBuffer *buffer;
  // specifies if start timestamp was set
  bool setStartTimestamp;
  // specifies if fragment is first fragment after seek (it can be incomplete)
  bool seeked;
  // specifies if fragment has incorrect timestamps (it happen after seeking)
  bool hasIncorrectTimestamps;
  // holds packet correction
  int packetCorrection;
};

#endif