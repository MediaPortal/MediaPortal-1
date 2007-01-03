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
// Copyright (c) 1996-2005 Live Networks, Inc.  All rights reserved.
// A file source that is a plain byte stream (rather than frames)
// Implementation

#if (defined(__WIN32__) || defined(_WIN32)) && !defined(_WIN32_WCE)
#include <io.h>
#include <fcntl.h>
#endif

#include "TsStreamFileSource.hh"
#include "InputFile.hh"
#include "GroupsockHelper.hh"
extern void Log(const char *fmt, ...) ;

  
TsStreamFileSource*
TsStreamFileSource::createNew(UsageEnvironment& env, char const* fileName,
				unsigned preferredFrameSize,
				unsigned playTimePerFrame) 
{
	Log("ts:open %s",fileName);  
  FileReader* reader;
  if (strstr(fileName,".tsbuffer")!=NULL)
  {
    reader = new MultiFileReader();
		__int64 fileSize= reader->GetFileSize();
  }
  else
  {
    reader = new FileReader();
  }
  reader->SetFileName((char*)fileName);
  reader->OpenFile();
  

  Boolean deleteFidOnClose = true;
  TsStreamFileSource* newSource = new TsStreamFileSource(env, (FILE*)reader, deleteFidOnClose, preferredFrameSize, playTimePerFrame);
  newSource->fFileSize = reader->GetFileSize();
	Log("ts:size %d",(DWORD)newSource->fFileSize);  
  return newSource;
}

TsStreamFileSource*
TsStreamFileSource::createNew(UsageEnvironment& env, FILE* fid,
				Boolean deleteFidOnClose,
				unsigned preferredFrameSize,
				unsigned playTimePerFrame) {
  if (fid == NULL) return NULL;

  TsStreamFileSource* newSource
    = new TsStreamFileSource(env, fid, deleteFidOnClose,
			       preferredFrameSize, playTimePerFrame);
  MultiFileReader* reader = (MultiFileReader*)fid;
  newSource->fFileSize = reader->GetFileSize();
	Log("ts:size %d",(DWORD)newSource->fFileSize);  

  return newSource;
}

void TsStreamFileSource::seekToByteAbsolute(u_int64_t byteNumber) {
	Log("ts:seek %d",(DWORD)byteNumber);  
  MultiFileReader* reader = (MultiFileReader*)fFid;
  reader->SetFilePointer( (int64_t)byteNumber, FILE_BEGIN);
}

void TsStreamFileSource::seekToByteRelative(int64_t offset) 
{
	Log("ts:seek rel %d",(DWORD)offset);  
  MultiFileReader* reader = (MultiFileReader*)fFid;
  reader->SetFilePointer((int64_t)offset, FILE_CURRENT);
}

TsStreamFileSource::TsStreamFileSource(UsageEnvironment& env, FILE* fid,
					   Boolean deleteFidOnClose,
					   unsigned preferredFrameSize,
					   unsigned playTimePerFrame)
  : FramedFileSource(env, fid), fPreferredFrameSize(preferredFrameSize),
    fPlayTimePerFrame(playTimePerFrame), fLastPlayTime(0), fFileSize(0),
    fDeleteFidOnClose(deleteFidOnClose) {
}

TsStreamFileSource::~TsStreamFileSource() {
  if (fDeleteFidOnClose && fFid != NULL) 
  {
    MultiFileReader* reader = (MultiFileReader*)fFid;
    reader->CloseFile();
    delete reader;
    fFid=NULL;
  }
}

void TsStreamFileSource::doGetNextFrame() {
  //if (feof(fFid) || ferror(fFid)) {
  //  handleClosure(this);
  //  return;
  //}

  // Try to read as many bytes as will fit in the buffer provided
  // (or "fPreferredFrameSize" if less)
  if (fPreferredFrameSize > 0 && fPreferredFrameSize < fMaxSize) {
    fMaxSize = fPreferredFrameSize;
  }
  
  MultiFileReader* reader = (MultiFileReader*)fFid;
  ULONG dwRead=0;
  if (reader->Read(fTo,fMaxSize,&dwRead)==S_FALSE)
  {
    handleClosure(this);
    return;
  }
  fFrameSize = dwRead;

  fFileSize = reader->GetFileSize();
  // Set the 'presentation time':
  if (fPlayTimePerFrame > 0 && fPreferredFrameSize > 0) {
    if (fPresentationTime.tv_sec == 0 && fPresentationTime.tv_usec == 0) {
      // This is the first frame, so use the current time:
      gettimeofday(&fPresentationTime, NULL);
    } else {
      // Increment by the play time of the previous data:
      unsigned uSeconds	= fPresentationTime.tv_usec + fLastPlayTime;
      fPresentationTime.tv_sec += uSeconds/1000000;
      fPresentationTime.tv_usec = uSeconds%1000000;
    }

    // Remember the play time of this data:
    fLastPlayTime = (fPlayTimePerFrame*fFrameSize)/fPreferredFrameSize;
    fDurationInMicroseconds = fLastPlayTime;
  } else {
    // We don't know a specific play time duration for this data,
    // so just record the current time as being the 'presentation time':
    gettimeofday(&fPresentationTime, NULL);
  }

  // Switch to another task, and inform the reader that he has data:
  nextTask() = envir().taskScheduler().scheduleDelayedTask(0,
				(TaskFunc*)FramedSource::afterGetting, this);
}
