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

#ifndef __RTSP_STREAM_TRACK_DEFINED
#define __RTSP_STREAM_TRACK_DEFINED

#include "RtspStreamFragmentCollection.h"

#include <stdint.h>

#define RTSP_STREAM_TRACK_FLAG_NONE                                   0x00000000
#define RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP         0x00000001
#define RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH                      0x00000002
#define RTSP_STREAM_TRACK_FLAG_END_OF_STREAM                          0x00000004

class CRtspStreamTrack
{
public:
  // initializes a new instance of CRtspStreamTrack class
  CRtspStreamTrack(void);
  ~CRtspStreamTrack(void);

  /* get methods */

  // gets RTSP steam fragments
  // @return : RTSP stream fragments
  CRtspStreamFragmentCollection *GetStreamFragments(void);

  // gets fragment timestamp based on current RTP packet timestamp and clock frequency defined by SDP
  // @param currentRtpPacketTimestamp : current RTP packet timestamp to get fragment timestamp
  // @param clockFrequency : clock frequency defined by SDP (value is per second)
  // @param initialTime : the time in ms to add to first timestamp (if no first timestamp is set)
  // @return : fragment timestamp in ms
  uint64_t GetFragmentTimestamp(unsigned int currentRtpPacketTimestamp, unsigned int clockFrequency, uint64_t initialTime);

  // gets fragment RTP timestamp based on current RTP packet timestamp
  // @param currentRtpPacketTimestamp : current RTP packet timestamp to get fragment timestamp
  // @return : fragment RTP timestamp
  uint64_t GetFragmentRtpTimestamp(unsigned int currentRtpPacketTimestamp);

  // gets currently downloading fragment
  // @return : currently downloading fragment or UINT_MAX if none
  unsigned int GetStreamFragmentDownloading(void);

  // gets currently processed fragment
  // @return : currently processed fragment
  unsigned int GetStreamFragmentProcessing(void);

  // gets fragment to be downloaded
  // always reset after started download of fragment
  // @return : fragment to be downloaded or UINT_MAX for next fragment
  unsigned int GetStreamFragmentToDownload(void);

  // gets stream length in bytes
  // @return : the stream length in bytes
  int64_t GetStreamLength(void);

  // gets byte position in buffer
  // it is always reset on seek
  // @return : byte position in buffer
  int64_t GetBytePosition(void);

  // gets full path to store file
  // @return : store file path or NULL if not specified
  const wchar_t *GetStoreFilePath(void);

  // gets last RTP packet stream fragment index
  // @return : last RTP packet stream fragment index
  unsigned int GetLastRtpPacketStreamFragmentIndex(void);
  
  // gets last RTP packet fragment received data position
  // @return : last RTP packet fragment received data position
  unsigned int GetLastRtpPacketFragmentReceivedDataPosition(void);

  /* set methods */

  // sets currently downloading fragment
  // @param streamFragmentDownloading : currently downloading fragment or UINT_MAX if none
  void SetStreamFragmentDownloading(unsigned int streamFragmentDownloading);

  // sets currently processed fragment
  // @param streamFragmentProcessing : currently processed fragment
  void SetStreamFragmentProcessing(unsigned int streamFragmentProcessing);

  // sets fragment to be downloaded
  // always reset after started download of fragment
  // @param streamFragmentToDownload : fragment to be downloaded or UINT_MAX for next fragment
  void SetStreamFragmentToDownload(unsigned int streamFragmentToDownload);

  // sets stream length in bytes
  // @param streamLength : the stream length in bytes to set
  void SetStreamLength(int64_t streamLength);

  // sets byte position in buffer
  // @param bytePosition : byte position in buffer to set
  void SetBytePosition(int64_t bytePosition);

  // sets set stream length flag
  // @param setStreamLengthFlag : set stream length flag to set
  void SetStreamLengthFlag(bool setStreamLengthFlag);

  // sets end of stream flag
  // @param endOfStreamFlag : end of stream flag to set
  void SetEndOfStreamFlag(bool endOfStreamFlag);

  // set full path to store file
  // @param storeFilePath : full path to store file to set
  // @return : true if successful, false otherwise
  bool SetStoreFilePath(const wchar_t *storeFilePath);

  // sets first RTP packet timestamp flag
  // @param setStreamLengthFlag : first RTP packet timestamp flag to set
  void SetFirstRtpPacketTimestampFlag(bool firstRtpPacketTimestampFlag);

  // sets last RTP packet stream fragment index
  // @param lastRtpPacketStreamFragmentIndex : last RTP packet stream fragment index to set
  void SetLastRtpPacketStreamFragmentIndex(unsigned int lastRtpPacketStreamFragmentIndex);
  
  // sets last RTP packet fragment received data position
  // @param lastRtpPacketFragmentReceivedDataPosition : last RTP packet fragment received data position to set
  void SetLastRtpPacketFragmentReceivedDataPosition(unsigned int lastRtpPacketFragmentReceivedDataPosition);

  /* other methods */

  // tests if first RTP packet timestamp is set
  // @return : true if first RTP packet timestamp is set, false otherwise
  bool IsSetFirstRtpPacketTimestamp(void);

  // tests if stream length is set
  // @return : true if stream length is set, false otherwise
  bool IsSetStreamLength(void);

  // tests if end of stream is set
  // @return : true if end of stream is set, false otherwise
  bool IsSetEndOfStream(void);

  // tests if specific combination of flags is set
  // @return : true if specific combination of flags is set, false otherwise
  bool IsFlags(unsigned int flags);

protected:
  // holds various flags
  unsigned int flags;
  // holds RTSP stream fragments
  CRtspStreamFragmentCollection *streamFragments;
  // holds RTP timestamp of first packet
  unsigned int firstRtpPacketTimestamp;
  // holds last RTP packet timestamp
  unsigned int lastRtpPacketTimestamp;
  // holds last fragment RTP timestamp
  uint64_t lastFragmentRtpTimestamp;

  // holds which fragment is currently downloading (UINT_MAX means none)
  unsigned int streamFragmentDownloading;
  // holds which fragment is currently processed
  unsigned int streamFragmentProcessing;
  // holds which fragment have to be downloaded
  // (UINT_MAX means next fragment, always reset after started download of fragment)
  unsigned int streamFragmentToDownload;

  // the lenght of stream track
  int64_t streamLength;
  // specifies position in buffer
  // it is always reset on seek
  int64_t bytePosition;

  // holds full path to store file
  wchar_t *storeFilePath;

  // holds last RTP packet stream fragment index
  unsigned int lastRtpPacketStreamFragmentIndex;
  // holds last RTP packet fragment received data position
  unsigned int lastRtpPacketFragmentReceivedDataPosition;
};

#endif