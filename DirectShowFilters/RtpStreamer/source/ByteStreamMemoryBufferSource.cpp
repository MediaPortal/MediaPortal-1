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
#include "config.h"
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
ByteStreamMemoryBufferSource::createNew(UsageEnvironment& env,
					char* stop,
					Boolean deleteBufferOnClose,
					unsigned preferredFrameSize,
					unsigned playTimePerFrame) {
  //if (buffer == NULL) return NULL;

  return new ByteStreamMemoryBufferSource(env, stop, deleteBufferOnClose, preferredFrameSize, playTimePerFrame);
}

ByteStreamMemoryBufferSource::ByteStreamMemoryBufferSource(UsageEnvironment& env,
							   char* stop,
							   Boolean deleteBufferOnClose,
							   unsigned preferredFrameSize,
							   unsigned playTimePerFrame)
  : FramedSource(env), fDeleteBufferOnClose(deleteBufferOnClose),
    fPreferredFrameSize(preferredFrameSize), fPlayTimePerFrame(playTimePerFrame), fLastPlayTime(0),
	fLimitNumBytesToStream(False), fNumBytesToStream(0), bufferVector(MEMORY_BUFFER_SIZE, '0'), vectorReadPointer(MEMORY_BUFFER_SIZE - 1), vectorWritePointer(0) {
	
	_stop = false;
	stopAll = stop;
	InitializeCriticalSection(&csBufferInAccess);

	LogDebugRtp2("Starting ByteStreamMemoryBufferSource.");
}

ByteStreamMemoryBufferSource::~ByteStreamMemoryBufferSource() {
	//if (fDeleteBufferOnClose) delete[] fBuffer; // TODO add delete _data
	bufferVector.clear();
	DeleteCriticalSection(&csBufferInAccess);
}

void ByteStreamMemoryBufferSource::doGetNextFrame() {
	//mtx.lock();
	EnterCriticalSection(&csBufferInAccess);
	
	// we are closing down, so stop everything
	if (*stopAll != 0)
	{
		LogDebugRtp2("Close");
		_stop = true;
		LeaveCriticalSection(&csBufferInAccess);
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

  size_t readBytesAvailable = getReadSizeAvailable();
  while (readBytesAvailable < fFrameSize) {
	  writeTsNullPackageToBuffer();
	  readBytesAvailable = getReadSizeAvailable();
  }

  // Cap our read at the number of bytes available to be read.
  if (fFrameSize > readBytesAvailable)
  {
	  fFrameSize = readBytesAvailable;
	  LogDebugRtp2("fFrameSize > readBytesAvail");
  }

  // Simultaneously keep track of how many bytes we've read and our position in the outgoing buffer
  if (vectorReadPointer < vectorWritePointer) {
	  memcpy(fTo, bufferVector.data() + vectorReadPointer + 1, fFrameSize);
	  vectorReadPointer += fFrameSize;
  }
  else {
	  ptrdiff_t ptrdiff = vectorReadPointer;
	  if (vectorReadPointer >= (bufferVector.size() - 1)) {
		  ptrdiff = -1;
	  }
	  size_t copyFirstSize = bufferVector.size() - vectorReadPointer - 1;
	  if (copyFirstSize >= fFrameSize) {
		  memcpy(fTo, bufferVector.data() + ptrdiff + 1, fFrameSize);
		  vectorReadPointer += fFrameSize;
	  }
	  else if (copyFirstSize == 0) {
		  memcpy(fTo, bufferVector.data(), fFrameSize);
		  vectorReadPointer = fFrameSize - 1;
	  }
	  else {
		  memcpy(fTo, bufferVector.data() + vectorReadPointer + 1, copyFirstSize);
		  memcpy(fTo + copyFirstSize, bufferVector.data(), fFrameSize - copyFirstSize);
		  vectorReadPointer = fFrameSize - copyFirstSize - 1;
	  }
  }

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
int ByteStreamMemoryBufferSource::Write(unsigned char *dataPtr, size_t numBytes)
{
	//mtx.lock();
	if (_stop) {
		LogDebugRtp2("We are in stop state, don't write data!");
		return 0;
	} else
		EnterCriticalSection(&csBufferInAccess);
	
	// If there's nothing to write or no room available, we resize the vector.
	if (getWriteSizeAvailable() < numBytes) {
		resizeVectorTo(bufferVector.size() + (numBytes - getWriteSizeAvailable()));
	}
	
	// Test if now enough memory is available
	if (getWriteSizeAvailable() < numBytes) {
		LogDebugRtp2("Could not get enough memory to write request.");
		//mtx.unlock();
		LeaveCriticalSection(&csBufferInAccess);
		return 0;
	}

	if (vectorWritePointer < vectorReadPointer) {
		memcpy(bufferVector.data() + vectorWritePointer, dataPtr, numBytes);
		vectorWritePointer += numBytes;
	}
	else {
		size_t copyFirstSize = bufferVector.size() - vectorWritePointer;
		if (copyFirstSize >= numBytes) {
			memcpy(bufferVector.data() + vectorWritePointer, dataPtr, numBytes);
			vectorWritePointer += numBytes;
			while (vectorWritePointer >= bufferVector.size()) {
				vectorWritePointer -= bufferVector.size();
			}
		}
		else {
			memcpy(bufferVector.data() + vectorWritePointer, dataPtr, copyFirstSize);
			memcpy(bufferVector.data(), dataPtr + copyFirstSize, numBytes - copyFirstSize);
			vectorWritePointer = numBytes - copyFirstSize;
		}
	}
	
	//mtx.unlock();
	if (!_stop)
		LeaveCriticalSection(&csBufferInAccess);

	return numBytes;
}

size_t ByteStreamMemoryBufferSource::getReadSizeAvailable() const {
	if (vectorReadPointer < vectorWritePointer) {
		return vectorWritePointer - vectorReadPointer - 1;
	}
	else {
		return vectorWritePointer + (bufferVector.size() - vectorReadPointer - 1);
	}
}

size_t ByteStreamMemoryBufferSource::getWriteSizeAvailable() const {
	if (vectorWritePointer < vectorReadPointer) {
		return vectorReadPointer - vectorWritePointer - 1;
	}
	else {
		return vectorReadPointer + (bufferVector.size() - vectorWritePointer - 1);
	}
}

void ByteStreamMemoryBufferSource::resizeVectorTo(size_t minSizeRequirement) {
	// Is a resize needed
	if (bufferVector.size() <= minSizeRequirement) {
		LogDebugRtp2("Resizing Vector to %d Bytes", minSizeRequirement);
		size_t newSize = bufferVector.size();
		while (newSize < minSizeRequirement) {
			newSize += MEMORY_BUFFER_INCREMENT;
		}
		std::vector<unsigned char> newBuffer(newSize, '0');

		// Backup Write Pointer
		size_t backupWritePointerSize = getReadSizeAvailable();

		// Copy all old available Read Data
		if (vectorReadPointer < vectorWritePointer) {
			size_t newIndex = 0;
			for (size_t i = vectorReadPointer + 1; i < vectorWritePointer; ++i) {
				newBuffer.at(newIndex) = bufferVector.at(i);
				++newIndex;
			}
		}
		else {
			// Copy all data to end and from start to write Pointer
			size_t newIndex = 0;
			for (size_t i = vectorReadPointer + 1; i < bufferVector.size(); ++i) {
				newBuffer.at(newIndex) = bufferVector.at(i);
				++newIndex;
			}
			// and from start to write Pointer
			for (size_t i = 0; i < vectorWritePointer; ++i) {
				newBuffer.at(newIndex) = bufferVector.at(i);
				++newIndex;
			}
		}
		vectorReadPointer = newBuffer.size() - 1;
		vectorWritePointer = backupWritePointerSize;

		// Asign new Buffer
		bufferVector = newBuffer;
	}
}

void ByteStreamMemoryBufferSource::writeByte(unsigned char byte) {
	bufferVector.at(vectorWritePointer) = byte;
	++vectorWritePointer;
	if (vectorWritePointer >= bufferVector.size()) {
		vectorWritePointer -= bufferVector.size();
	}
}

void ByteStreamMemoryBufferSource::writeTsNullPackageToBuffer() {
	if (getWriteSizeAvailable() < 188) {
		resizeVectorTo(bufferVector.size() + 188);
	}
	// First write 0x47, sync byte
	writeByte(0x47);
	// Bit flags ("transport error indicator"=0 (1 bit); "payload unit start indicator"=0 (1 bit); "transport priority"=0 (1 bit)) and PID (13 bit), 16 bit
	writeByte(0x1F);
	writeByte(0xFF);
	// flags ("transport scamble control"=00 (2 bit); "adaption field control"=01 (2 bit)) and Continuity counter (4bit)
	writeByte(0x10);
	// 184 byte payload just zeros
	for (size_t i = 184; i > 0; --i) {
		writeByte(0x00);
	}
}

void ByteStreamMemoryBufferSource::reset() {
	LogDebugRtp2("ByteStreamMemoryBufferSource: reset()");
	//Sleep(2000);
	EnterCriticalSection(&csBufferInAccess);
	_stop = false;
	LeaveCriticalSection(&csBufferInAccess);
	LogDebugRtp2("ByteStreamMemoryBufferSource: reset() finish");
}