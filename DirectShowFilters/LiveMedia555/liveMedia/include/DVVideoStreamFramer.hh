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
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2009 Live Networks, Inc.  All rights reserved.
// A filter that parses a DV input stream into DV frames to deliver to the downstream object
// C++ header

#ifndef _DV_VIDEO_STREAM_FRAMER_HH
#define _DV_VIDEO_STREAM_FRAMER_HH

#ifndef _FRAMED_FILTER_HH
#include "FramedFilter.hh"
#endif

class DVVideoStreamFramer: public FramedFilter {
public:
  static DVVideoStreamFramer*
  createNew(UsageEnvironment& env, FramedSource* inputSource);

  char const* profileName() const;

protected:
  DVVideoStreamFramer(UsageEnvironment& env,
		      FramedSource* inputSource);
      // called only by createNew(), or by subclass constructors
  virtual ~DVVideoStreamFramer();

private:
  // redefined virtual functions:
  virtual Boolean isDVVideoStreamFramer() const;
  virtual void doGetNextFrame();

private:
  char const* fProfileName;
};

#endif
