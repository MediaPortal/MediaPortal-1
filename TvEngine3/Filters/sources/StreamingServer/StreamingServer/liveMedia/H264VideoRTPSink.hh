/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 2.1 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2006 Live Networks, Inc.  All rights reserved.
// RTP sink for H.264 video (RFC 3984)
// C++ header

#ifndef _H264_VIDEO_RTP_SINK_HH
#define _H264_VIDEO_RTP_SINK_HH

#ifndef _VIDEO_RTP_SINK_HH
#include "VideoRTPSink.hh"
#endif

class H264VideoRTPSink: public VideoRTPSink {
public:
  static H264VideoRTPSink* createNew(UsageEnvironment& env,
				     Groupsock* RTPgs,
				     unsigned char rtpPayloadFormat,
				     unsigned profile_level_id,
				     char const* sprop_parameter_sets_str);

protected:
  H264VideoRTPSink(UsageEnvironment& env, Groupsock* RTPgs,
		   unsigned char rtpPayloadFormat,
		   unsigned profile_level_id,
		   char const* sprop_parameter_sets_str);
	// called only by createNew()

  virtual ~H264VideoRTPSink();

private: // redefined virtual functions:
  virtual Boolean sourceIsCompatibleWithUs(MediaSource& source);
  virtual Boolean continuePlaying();
  virtual void stopPlaying();
  virtual void doSpecialFrameHandling(unsigned fragmentationOffset,
                                      unsigned char* frameStart,
                                      unsigned numBytesInFrame,
                                      struct timeval frameTimestamp,
                                      unsigned numRemainingBytes);
  virtual char const* auxSDPLine();

private:
  class H264FUAFragmenter* fOurFragmenter;
  char* fFmtpSDPLine;
};

#endif
