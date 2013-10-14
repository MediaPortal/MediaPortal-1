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

#include "LinearBuffer.h"
#include "HttpDownloadRequest.h"
#include "HttpDownloadResponse.h"

#include <stdint.h>

class CSegmentFragment
{
public:
  // initializes a new instance of CSegmentFragment class
  // @param segment : segment ID
  // @param fragment : fragment ID
  // @param fragmentTimestamp : the timestamp of segment and fragment in ms
  CSegmentFragment(unsigned int segment, unsigned int fragment, uint64_t fragmentTimestamp);

  // destructor
  ~CSegmentFragment(void);

  /* get methods */

  // gets segment ID
  // @return : segment ID
  unsigned int GetSegment(void);

  // gets fragment ID
  // @return : fragment ID
  unsigned int GetFragment(void);

  // gets fragment timestamp in ms
  // @return : fragment timestamp in ms
  uint64_t GetFragmentTimestamp(void);

  // gets position of start of segment and fragment within store file
  // @return : file position or -1 if error
  int64_t GetStoreFilePosition(void);

  // gets the length of segment and fragment data
  // @return : the length of segment and fragment data
  unsigned int GetLength(void);

  // gets HTTP download request for segment and fragment
  // @return : HTTP download request
  CHttpDownloadRequest *GetHttpDownloadRequest(void);

  // gets HTTP download response for segment and fragment
  // @return : HTTP download response
  CHttpDownloadResponse *GetHttpDownloadResponse(void);

  /* set methods */

  // sets if segment and fragment is downloaded
  // @param downloaded : true if segment and fragment is downloaded
  void SetDownloaded(bool downloaded);

  // sets if segment and fragment is processed
  // @param processed : true if segment and fragment is processed
  void SetProcessed(bool processed);

  // sets position within store file
  // if segment and fragment is stored than linear buffer is deleted
  // if store file path is cleared (NULL) than linear buffer is created
  // @param position : the position of start of segment and fragment within store file or (-1) if segment and fragment is in memory
  void SetStoredToFile(int64_t position);

  // sets HTTP download request for segment and fragment
  // @param downloadRequest : HTTP download request to set
  // @return : true if successful, false otherwise
  bool SetHttpDownloadRequest(CHttpDownloadRequest *downloadRequest);

  // sets HTTP download response for segment and fragment
  // @param downloadResponse : HTTP download response to set
  // @return : true if successful, false otherwise
  bool SetHttpDownloadResponse(CHttpDownloadResponse *downloadResponse);

  /* other methods */

  // tests if media packet is stored to file
  // @return : true if media packet is stored to file, false otherwise
  bool IsStoredToFile(void);

  // tests if segment and fragment is downloaded
  // @return : true if downloaded, false otherwise
  bool IsDownloaded(void);

  // tests if segment and fragment is processed
  // @return : true if downloaded, false otherwise
  bool IsProcessed(void);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CSegmentFragment *Clone(void);

  // creates HTTP download request, previous request (if exists) is destroyed
  // @return : true if HTTP download request created, false otherwise
  bool CreateHttpDownloadRequest(void);

  // creates HTTP download response, previous response (if exists) is destroyed
  // @return : true if HTTP download response created, false otherwise
  bool CreateHttpDownloadResponse(void);

  // frees HTTP download request
  void FreeHttpDownloadRequest(void);

  // frees HTTP download response
  void FreeHttpDownloadResponse(void);

private:
  // stores segment ID
  unsigned int segment;
  // stores fragment ID
  unsigned int fragment;
  // stores fragment timestamp
  uint64_t fragmentTimestamp;
  // stores if segment and fragment is downloaded
  bool downloaded;
  // stores if segment and fragment is processed
  bool processed;
  // posittion in store file
  int64_t storeFilePosition;
  // the length of segment and fragment data
  unsigned int length;
  // holds HTTP download request for current segment and fragment
  CHttpDownloadRequest *httpDownloadRequest;
  // holds HTTP download response for current segment and fragment
  CHttpDownloadResponse *httpDownloadResponse;
};

#endif