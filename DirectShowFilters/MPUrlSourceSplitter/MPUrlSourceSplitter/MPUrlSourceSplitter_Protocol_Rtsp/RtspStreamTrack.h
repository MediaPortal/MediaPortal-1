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
#include "CacheFile.h"
#include "Flags.h"

// DirectShow times are in 100ns units
#ifndef DSHOW_TIME_BASE
#define DSHOW_TIME_BASE                                               10000000
#endif

#define RTSP_STREAM_TRACK_FLAG_NONE                                   FLAGS_NONE

#define RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP         (1 << (FLAGS_LAST + 0))
#define RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH                      (1 << (FLAGS_LAST + 1))
#define RTSP_STREAM_TRACK_FLAG_END_OF_STREAM                          (1 << (FLAGS_LAST + 2))
#define RTSP_STREAM_TRACK_FLAG_SUPRESS_DATA                           (1 << (FLAGS_LAST + 3))
#define RTSP_STREAM_TRACK_FLAG_RECEIVED_ALL_DATA                      (1 << (FLAGS_LAST + 4))
#define RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TICKS             (1 << (FLAGS_LAST + 5))

#define RTSP_STREAM_TRACK_FLAG_LAST                                   (FLAGS_LAST + 6)

class CRtspStreamTrack : public CFlags
{
public:
  // initializes a new instance of CRtspStreamTrack class
  CRtspStreamTrack(HRESULT *result);
  ~CRtspStreamTrack(void);

  /* get methods */

  // gets RTSP steam fragments
  // @return : RTSP stream fragments
  CRtspStreamFragmentCollection *GetStreamFragments(void);

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

  // gets first RTP packet timestamp
  // @return : first RTP packet timestamp
  unsigned int GetFirstRtpPacketTimestamp(void);

  // gets first RTP packet ticks
  // @return : first RTP packet ticks
  unsigned int GetFirstRtpPacketTicks(void);

  // gets RTP packet timestamp based on current RTP packet timestamp
  // @param currentRtpPacketTimestamp : current RTP packet timestamp to get RTP packet timestamp
  // @param storeLastRtpPacketTimestamp : true if current RTP packet timestamp have to be stored as last RTP packet timestamp, false otherwise
  // @return : RTP packet timestamp
  int64_t GetRtpPacketTimestamp(unsigned int currentRtpPacketTimestamp, bool storeLastRtpPacketTimestamp);

  // gets RTP packet timestamp in DSHOW_TIME_BASE units
  // @param rtpPacketTimestamp : the RTP packet timestamp to get in DSHOW_TIME_BASE units
  // @return : RTP packet timestamp in DSHOW_TIME_BASE units
  int64_t GetRtpPacketTimestampInDshowTimeBaseUnits(int64_t rtpPacketTimestamp);

  // gets RTP timestamp correction
  // @return : RTP timestamp correction
  int64_t GetRtpTimestampCorrection(void);

  // gets track clock frequency
  // @return : track clock frequency
  unsigned int GetClockFrequency(void);

  // gets cache file instance
  // @return : cache file instance or NULL if error
  CCacheFile *GetCacheFile(void);

  // gets last receive data time (in ms)
  // @return : last receive data time (in ms)
  unsigned int GetLastReceiveDataTime(void);

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

  // sets set stream length flag
  // @param setStreamLengthFlag : set stream length flag to set
  void SetStreamLengthFlag(bool setStreamLengthFlag);

  // sets end of stream flag
  // @param endOfStreamFlag : end of stream flag to set
  void SetEndOfStreamFlag(bool endOfStreamFlag);

  // sets supress data flag
  // @param supressDataFlag : supress data flag to set
  void SetSupressDataFlag(bool supressDataFlag);

  // sets received all data flag
  // @param receivedAllDataFlag : received all data flag to set
  void SetReceivedAllDataFlag(bool receivedAllDataFlag);

  // sets first RTP packet timestamp (it also set last RTP packet timestamp)
  // @param rtpPacketTimestamp : RTP packet timestamp to set as first RTP packet timestamp
  // @param firstRtpPacketTimestampFlag : the first RTP packet timestamp flag (true if first RTP packet timestamp is to be set, false otherwise)
  // @param firstRtpPacketTicks : the first RTP packet ticks (it is only set when firstRtpPacketTimestampFlag is true and ticks were not set earlier)
  void SetFirstRtpPacketTimestamp(unsigned int rtpPacketTimestamp, bool firstRtpPacketTimestampFlag, unsigned int firstRtpPacketTicks);

  // sets clock frequency used to convert RTP timestamp to real time
  // it also sets numerator and denominator used to convert RTP timestamp to DSHOW_TIME_BASE units
  // @param clockFrequency : the clock frequency to set
  void SetClockFrequency(unsigned int clockFrequency);

  // sets RTP timestamp correction
  // @param rtpTimestampCorrection : the RTP timestamp correction to set
  void SetRtpTimestampCorrection(int64_t rtpTimestampCorrection);

  // sets last receive data time (in ms)
  // @param lastReceiveDataTime : last receive data time (in ms) to set
  void SetLastReceiveDataTime(unsigned int lastReceiveDataTime);

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

  // tests if supress data is set
  // @return : true if supress data is set, false otherwise
  bool IsSetSupressData(void);

  // tests if all data are received
  // @return : true if all data received flag is set, false otherwise
  bool IsReceivedAllData(void);

protected:
  // holds RTSP stream fragments
  CRtspStreamFragmentCollection *streamFragments;
  // holds RTP timestamp of first packet
  unsigned int firstRtpPacketTimestamp;
  // holds last RTP packet timestamp
  unsigned int lastRtpPacketTimestamp;
  // holds last cumulated RTP packet timestamp
  int64_t lastCumulatedRtpTimestamp;
  // holds RTP timestamp correction (calculated after seek)
  int64_t rtpTimestampCorrection;

  // holds ticks for only first RTP packet (if restarted download then firstRtpPacketTicks is not changed)
  unsigned int firstRtpPacketTicks;

  // holds which fragment is currently downloading (UINT_MAX means none)
  unsigned int streamFragmentDownloading;
  // holds which fragment is currently processed
  unsigned int streamFragmentProcessing;
  // holds which fragment have to be downloaded
  // (UINT_MAX means next fragment, always reset after started download of fragment)
  unsigned int streamFragmentToDownload;

  // the lenght of stream track
  int64_t streamLength;

  // numerator and denominator to convert RTP timestamp to DSHOW_TIME_BASE units
  int64_t dshowTimeBaseNumerator;
  int64_t dshowTimeBaseDenominator;

  // holds last receive data time
  unsigned int lastReceiveDataTime;

  // clock frequency 
  unsigned int clockFrequency;

  // holds cache file instance
  CCacheFile *cacheFile;

  /* methods */
};

#endif