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

#ifndef __STREAM_FRAGMENT_DEFINED
#define __STREAM_FRAGMENT_DEFINED

//#include "LinearBuffer.h"
//
//#include <stdint.h>
//
//#define FRAGMENT_TYPE_UNSPECIFIED                                             0
//#define FRAGMENT_TYPE_VIDEO                                                   1
//#define FRAGMENT_TYPE_AUDIO                                                   2
//
//class CStreamFragment
//{
//public:
//  // creats new instance of CStreamFragment class
//  CStreamFragment(const wchar_t *url, uint64_t fragmentDuration, uint64_t fragmentTime, unsigned int fragmentType);
//
//  // desctructor
//  ~CStreamFragment(void);
//
//  /* get methods */
//
//  // gets fragment duration
//  // @return : fragment duration
//  uint64_t GetFragmentDuration(void);
//
//  // gets fragment time
//  // @return : fragment time
//  uint64_t GetFragmentTime(void);
//
//  // gets stream fragment url
//  // @return : stream fragment url or NULL if error
//  const wchar_t *GetUrl(void);
//
//  // gets stream fragment type (audio, video, ...)
//  // @return : stream fragment type
//  unsigned int GetFragmentType(void);
//
//  // gets if stream fragment is downloaded
//  // @return : true if downloaded, false otherwise
//  bool GetDownloaded(void);
//
//  // gets position of start of segment and fragment within store file
//  // @return : file position or -1 if error
//  int64_t GetStoreFilePosition(void);
//
//  // gets linear buffer
//  // @return : linear buffer or NULL if error or segment and fragment is stored to file
//  CLinearBuffer *GetBuffer();
//
//  // gets the length of segment and fragment data
//  // @return : the length of segment and fragment data
//  unsigned int GetLength(void);
//
//  /* set methods */
//
//  // sets if stream fragment is downloaded
//  // @param downloaded : true if stream fragment is downloaded
//  void SetDownloaded(bool downloaded);
//
//  // sets position within store file
//  // if segment and fragment is stored than linear buffer is deleted
//  // if store file path is cleared (NULL) than linear buffer is created
//  // @param position : the position of start of segment and fragment within store file or (-1) if segment and fragment is in memory
//  void SetStoredToFile(int64_t position);
//
//  /* other methods */
//
//  // tests if media packet is stored to file
//  // @return : true if media packet is stored to file, false otherwise
//  bool IsStoredToFile(void);
//
//private:
//
//  // stores stream fragment duration
//  uint64_t fragmentDuration;
//  // stores stream fragment start time
//  uint64_t fragmentTime;
//  // stores url for stream fragment
//  wchar_t *url;
//  // stores if stream fragment is downloaded
//  bool downloaded;
//  // stores fragment type (audio, video, ...)
//  unsigned int fragmentType;
//  // posittion in store file
//  int64_t storeFilePosition;
//  // internal linear buffer for segment and fragment data
//  CLinearBuffer *buffer;
//  // the length of segment and fragment data
//  unsigned int length;
//};

#endif