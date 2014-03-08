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
// Implementation

#include "Stdafx.h"
#include "ByteStreamMemoryBufferSource.hh"
#include "GroupsockHelper.hh"
#include <string>
#include <errno.h>

void LogDebugRtp2(const wchar_t *fmt, ...)
{
	va_list ap;
	va_start(ap, fmt);

	va_start(ap, fmt);
	vswprintf_s(logbufferw2, fmt, ap);
	va_end(ap);

	wchar_t folder[MAX_PATH];
	wchar_t fileName[MAX_PATH];
	::SHGetSpecialFolderPathW(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);
	swprintf_s(fileName, L"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log", folder);

	FILE* fp = _wfopen(fileName, L"a+, ccs=UTF-8");
	if (fp != NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fwprintf(fp, L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour, systemTime.wMinute, systemTime.wSecond, systemTime.wMilliseconds,
			logbufferw2);
		fclose(fp);
	}
};

void LogDebugRtp2(const char *fmt, ...)
{
	va_list ap;
	va_start(ap, fmt);

	va_start(ap, fmt);
	vsprintf(logbuffer2, fmt, ap);
	va_end(ap);

	MultiByteToWideChar(CP_ACP, 0, logbuffer2, -1, logbufferw2, sizeof(logbuffer2) / sizeof(wchar_t));
	LogDebugRtp2(L"%s", logbufferw2);
};

////////// ByteStreamMemoryBufferSource //////////

ByteStreamMemoryBufferSource*
ByteStreamMemoryBufferSource::createNew(UsageEnvironment& env/*,
					u_int8_t* buffer*/, u_int64_t bufferSize,
					Boolean deleteBufferOnClose,
					unsigned preferredFrameSize,
					unsigned playTimePerFrame) {
  //if (buffer == NULL) return NULL;

  return new ByteStreamMemoryBufferSource(env/*, buffer*/, bufferSize, deleteBufferOnClose, preferredFrameSize, playTimePerFrame);
}

ByteStreamMemoryBufferSource::ByteStreamMemoryBufferSource(UsageEnvironment& env/*,
							   u_int8_t* buffer*/, u_int64_t bufferSize,
							   Boolean deleteBufferOnClose,
							   unsigned preferredFrameSize,
							   unsigned playTimePerFrame)
  : FramedSource(env)/*, fBuffer(buffer)*/, fBufferSize(bufferSize), fCurIndex(0), fDeleteBufferOnClose(deleteBufferOnClose),
    fPreferredFrameSize(preferredFrameSize), fPlayTimePerFrame(playTimePerFrame), fLastPlayTime(0),
	fLimitNumBytesToStream(False), fNumBytesToStream(0) {
	_data = new unsigned char[bufferSize];
	memset(_data, 0, bufferSize);
	_readPtr = 0;
	_writePtr = 0;
	_writeBytesAvail = bufferSize;
	InitializeCriticalSection(&csBufferInAccess);
}

ByteStreamMemoryBufferSource::~ByteStreamMemoryBufferSource() {
  //if (fDeleteBufferOnClose) delete[] fBuffer; // TODO add delete _data
  delete[] _data;
  DeleteCriticalSection(&csBufferInAccess);
}

void ByteStreamMemoryBufferSource::seekToByteAbsolute(u_int64_t byteNumber, u_int64_t numBytesToStream) {
  /*fCurIndex = byteNumber;
  if (fCurIndex > fBufferSize) fCurIndex = fBufferSize;

  fNumBytesToStream = numBytesToStream;
  fLimitNumBytesToStream = fNumBytesToStream > 0;*/
}

void ByteStreamMemoryBufferSource::seekToByteRelative(int64_t offset) {
  /*int64_t newIndex = fCurIndex + offset;
  if (newIndex < 0) {
    fCurIndex = 0;
  } else {
    fCurIndex = (u_int64_t)offset;
    if (fCurIndex > fBufferSize) fCurIndex = fBufferSize;
  }*/
}

void ByteStreamMemoryBufferSource::doGetNextFrame() {
	//mtx.lock();
	EnterCriticalSection(&csBufferInAccess);
	
  // If there's nothing to read or no data available, then we can't read anything.
  if (_writeBytesAvail == fBufferSize || (fLimitNumBytesToStream && fNumBytesToStream == 0))
  {
	  LogDebugRtp2("Close");
	  handleClosure(this);
	  return;
  }

  // Try to read as many bytes as will fit in the buffer provided (or "fPreferredFrameSize" if less)
  fFrameSize = fMaxSize;
  if (fLimitNumBytesToStream && fNumBytesToStream < (u_int64_t)fFrameSize) {	// comment: will be in the current code never be true
    fFrameSize = (unsigned)fNumBytesToStream;
  }
  if (fPreferredFrameSize > 0 && fPreferredFrameSize < fFrameSize) {	// comment: might get true, but typically it won't be
    fFrameSize = fPreferredFrameSize;
  }

  // start mod
  int readBytesAvail = fBufferSize - _writeBytesAvail;

  // Cap our read at the number of bytes available to be read.
  if (fFrameSize > readBytesAvail)
  {
	  fFrameSize = readBytesAvail;
	  LogDebugRtp2("fFrameSize > readBytesAvail");
  }

  // Simultaneously keep track of how many bytes we've read and our position in the outgoing buffer
  if (fFrameSize > fBufferSize - _readPtr)
  {
	  int len = fBufferSize - _readPtr;
	  memcpy(fTo, _data + _readPtr, len);
	  memcpy(fTo + len, _data, fFrameSize - len);
  }
  else
  {
	  memcpy(fTo, _data + _readPtr, fFrameSize);
  }

  _readPtr = (_readPtr + fFrameSize) % fBufferSize;
  _writeBytesAvail += fFrameSize;

  //mtx.unlock();
  LeaveCriticalSection(&csBufferInAccess);


  // Set the 'presentation time':
  if (fPlayTimePerFrame > 0 && fPreferredFrameSize > 0) {
	  if (fPresentationTime.tv_sec == 0 && fPresentationTime.tv_usec == 0) {
		  // This is the first frame, so use the current time:
		  gettimeofday(&fPresentationTime, NULL);
	  }
	  else {
		  // Increment by the play time of the previous data:
		  unsigned uSeconds = fPresentationTime.tv_usec + fLastPlayTime;
		  fPresentationTime.tv_sec += uSeconds / 1000000;
		  fPresentationTime.tv_usec = uSeconds % 1000000;
	  }

	  // Remember the play time of this data:
	  fLastPlayTime = (fPlayTimePerFrame*fFrameSize) / fPreferredFrameSize;
	  fDurationInMicroseconds = fLastPlayTime;
  }
  else {
	  // We don't know a specific play time duration for this data,
	  // so just record the current time as being the 'presentation time':
	  gettimeofday(&fPresentationTime, NULL);
  }

  // Inform the downstream object that it has data:
  FramedSource::afterGetting(this);
}

// Write to the ring buffer. Do not overwrite data that has not yet
// been read.
int ByteStreamMemoryBufferSource::Write(unsigned char *dataPtr, int numBytes)
{
	//mtx.lock();
	EnterCriticalSection(&csBufferInAccess);
	// If there's nothing to write or no room available, we can't write anything.
	if (dataPtr == 0 || numBytes <= 0 || _writeBytesAvail == 0)
	{
		LogDebugRtp2("nothing to write or no room available");
		//mtx.unlock();
		LeaveCriticalSection(&csBufferInAccess);
		return 0;
	}

	// Cap our write at the number of bytes available to be written.
	if (numBytes > _writeBytesAvail)
	{
		numBytes = _writeBytesAvail;
		LogDebugRtp2("numBytes > _writeBytesAvail");
	}

	// Simultaneously keep track of how many bytes we've written and our position in the incoming buffer
	if (numBytes > fBufferSize - _writePtr)
	{
		int len = fBufferSize - _writePtr;
		memcpy(_data + _writePtr, dataPtr, len);
		memcpy(_data, dataPtr + len, numBytes - len);
	}
	else
	{
		memcpy(_data + _writePtr, dataPtr, numBytes);
	}

	_writePtr = (_writePtr + numBytes) % fBufferSize;
	_writeBytesAvail -= numBytes;

	//mtx.unlock();
	LeaveCriticalSection(&csBufferInAccess);

	return numBytes;
}