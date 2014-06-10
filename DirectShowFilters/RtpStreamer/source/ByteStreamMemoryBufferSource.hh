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
// Copyright (c) 1996-2014 Live Networks, Inc.  All rights reserved.
// A class for streaming data from a (static) memory buffer, as if it were a file.
// C++ header


#ifndef _BYTE_STREAM_MEMORY_BUFFER_SOURCE_HH
#define _BYTE_STREAM_MEMORY_BUFFER_SOURCE_HH

#ifndef _FRAMED_SOURCE_HH
#include "FramedSource.hh"
#endif

#include <vector>
#include <cstdint>

#include <shlobj.h>
static char logbuffer2[2000];
static wchar_t logbufferw2[2000];
void LogDebugRtp2(const wchar_t *);

void LogDebugRtp2(const char, ...);

class ByteStreamMemoryBufferSource: public FramedSource {
public:
  static ByteStreamMemoryBufferSource* createNew(UsageEnvironment& env,
						 char* stop,
						 Boolean deleteBufferOnClose = True,
						 unsigned preferredFrameSize = 0,
						 unsigned playTimePerFrame = 0);
      // "preferredFrameSize" == 0 means 'no preference'
      // "playTimePerFrame" is in microseconds

  u_int64_t bufferSize() const { return bufferVector.size(); }

  int ByteStreamMemoryBufferSource::Write(unsigned char *dataPtr, size_t numBytes);
  size_t getReadSizeAvailable() const;
  size_t getWriteSizeAvailable() const;
protected:
  ByteStreamMemoryBufferSource(UsageEnvironment& env,
				   char* stop,
			       Boolean deleteBufferOnClose,
			       unsigned preferredFrameSize,
			       unsigned playTimePerFrame);
	// called only by createNew()

  virtual ~ByteStreamMemoryBufferSource();

private:
  // redefined virtual functions:
  virtual void doGetNextFrame();

  void resizeVectorTo(size_t minSizeRequirement);
  void writeByte(unsigned char byte);
  void writeTsNullPackageToBuffer();

  Boolean fDeleteBufferOnClose;
  unsigned fPreferredFrameSize;
  unsigned fPlayTimePerFrame;
  unsigned fLastPlayTime;
  Boolean fLimitNumBytesToStream;
  u_int64_t fNumBytesToStream; // used iff "fLimitNumBytesToStream" is True

  //std::mutex mtx;
  CRITICAL_SECTION csBufferInAccess;
  Boolean _stop;
  char* stopAll;

  std::vector<unsigned char> bufferVector;
  size_t vectorReadPointer;
  size_t vectorWritePointer;
};

#endif
