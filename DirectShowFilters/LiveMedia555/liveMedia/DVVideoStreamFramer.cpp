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
// Implementation

#include "DVVideoStreamFramer.hh"

////////// DVVideoStreamFramer implementation //////////

DVVideoStreamFramer::DVVideoStreamFramer(UsageEnvironment& env, FramedSource* inputSource)
  : FramedFilter(env, inputSource),
    fProfileName(NULL) {
}

DVVideoStreamFramer::~DVVideoStreamFramer() {
}

DVVideoStreamFramer*
DVVideoStreamFramer::createNew(UsageEnvironment& env, FramedSource* inputSource) {
  return new DVVideoStreamFramer(env, inputSource);
}

char const* DVVideoStreamFramer::profileName() const {
#if 0
  if (fProfileName == NULL) {
    unsigned char* fSavedFrame;
    unsigned fSavedFrameSize;
    char fSavedFrameFlag;
  }
#endif

    // We haven't yet read any data that will let us determine the profile, so read an initial block of data so we can do so:
  return "";//#####@@@@@
}

Boolean DVVideoStreamFramer::isDVVideoStreamFramer() const {
  return True;
}

void DVVideoStreamFramer::doGetNextFrame() {
  // COMPLETE THIS #####@@@@@
}
