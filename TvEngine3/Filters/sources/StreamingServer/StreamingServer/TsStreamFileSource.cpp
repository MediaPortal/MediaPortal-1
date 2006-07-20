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

////////// TsStreamFileSource //////////

TsStreamFileSource* TsStreamFileSource::createNew(UsageEnvironment& env, char const* fileName,
				unsigned preferredFrameSize,
				unsigned playTimePerFrame) 
{
  MultiFileReader* reader = new MultiFileReader();
  reader->SetFileName((char*)fileName);
  reader->OpenFile();

  Boolean deleteFidOnClose =  true ;
  TsStreamFileSource* newSource= new TsStreamFileSource(env, reader, deleteFidOnClose,preferredFrameSize, playTimePerFrame);


  return newSource;
}

TsStreamFileSource* TsStreamFileSource::createNew(UsageEnvironment& env, MultiFileReader* fid,
				Boolean deleteFidOnClose,
				unsigned preferredFrameSize,
				unsigned playTimePerFrame) {
  if (fid == NULL) return NULL;

  TsStreamFileSource* newSource= new TsStreamFileSource(env, fid, deleteFidOnClose,preferredFrameSize, playTimePerFrame);


  return newSource;
}

void TsStreamFileSource::seekToByteAbsolute(u_int64_t byteNumber) 
{
  m_reader->setFilePointer((int64_t)byteNumber, FILE_BEGIN);
}

void TsStreamFileSource::seekToByteRelative(int64_t offset) 
{
  m_reader->setFilePointer((int64_t)offset, FILE_CURRENT 	);
}

TsStreamFileSource::TsStreamFileSource(UsageEnvironment& env, MultiFileReader* fid,
					   Boolean deleteFidOnClose,
					   unsigned preferredFrameSize,
					   unsigned playTimePerFrame)
  : FramedFileSource(env, NULL), fPreferredFrameSize(preferredFrameSize),
    fPlayTimePerFrame(playTimePerFrame), fLastPlayTime(0),
    fDeleteFidOnClose(deleteFidOnClose) 
{
  m_reader=fid;
}

TsStreamFileSource::~TsStreamFileSource() 
{
    printf("TsStreamFileSource::~TsStreamFileSource"); 
  if (fDeleteFidOnClose && m_reader != NULL) 
  {
    m_reader->CloseFile();
    delete m_reader;
    m_reader=NULL;
  }
}

void TsStreamFileSource::doGetNextFrame()
{
  /*if (feof(fFid) || ferror(fFid)) 
  {
    handleClosure(this);
    return;
  }*/

  // Try to read as many bytes as will fit in the buffer provided
  // (or "fPreferredFrameSize" if less)
  if (fPreferredFrameSize > 0 && fPreferredFrameSize < fMaxSize) 
  {
    fMaxSize = fPreferredFrameSize;
  }
  ULONG dwReadBytes;
  int hr = m_reader->Read(fTo, fMaxSize, &dwReadBytes);
  if (hr!=S_OK)
  {
    printf("TsStreamFileSource::doGetNextFrame() end of file"); 
    handleClosure(this);
    return;
  }
  //printf(" max:%d read:%d\n", (int)fMaxSize, (int)dwReadBytes);
  fFrameSize = dwReadBytes;

  // Set the 'presentation time':
  if (fPlayTimePerFrame > 0 && fPreferredFrameSize > 0) 
  {
    if (fPresentationTime.tv_sec == 0 && fPresentationTime.tv_usec == 0) 
    {
      // This is the first frame, so use the current time:
      gettimeofday(&fPresentationTime, NULL);
    } 
    else 
    {
      // Increment by the play time of the previous data:
      unsigned uSeconds	= fPresentationTime.tv_usec + fLastPlayTime;
      fPresentationTime.tv_sec += uSeconds/1000000;
      fPresentationTime.tv_usec = uSeconds%1000000;
    }

    // Remember the play time of this data:
    fLastPlayTime = (fPlayTimePerFrame*fFrameSize)/fPreferredFrameSize;
    fDurationInMicroseconds = fLastPlayTime;
  } 
  else 
  {
    // We don't know a specific play time duration for this data,
    // so just record the current time as being the 'presentation time':
    gettimeofday(&fPresentationTime, NULL);
  }

  // Switch to another task, and inform the reader that he has data:
  nextTask() = envir().taskScheduler().scheduleDelayedTask(0,(TaskFunc*)FramedSource::afterGetting, this);
}


u_int64_t TsStreamFileSource::fileSize() const 
{
	if (m_reader==NULL) return 0LL;
	return m_reader->GetFileSize();
}