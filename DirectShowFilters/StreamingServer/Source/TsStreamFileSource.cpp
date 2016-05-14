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

#include "StdAfx.h"
#include "TsStreamFileSource.h"
#include "FileReader.h"
#include "MultiFileReader.h"
#include "TsFileSeek.h"

#ifndef _GROUPSOCK_HELPER_HH
#include <GroupsockHelper.hh>   // gettimeofday()
#endif

extern void LogDebug(const char *fmt, ...) ;
extern void LogDebug(const wchar_t *fmt, ...) ;


TsStreamFileSource*
TsStreamFileSource::createNew(UsageEnvironment& env, wchar_t const* fileName,
							  unsigned preferredFrameSize,
							  unsigned playTimePerFrame, int channelType) 
{
	LogDebug(L"ts:open %s", fileName);  
	FileReader* reader;
	if (wcsstr(fileName, L".tsbuffer")!=NULL)
	{
      //MultiFileReader::MultiFileReader(BOOL useFileNext, BOOL useDummyWrites, CCritSec* pFilterLock, BOOL useRandomAccess, BOOL extraLogging):
    reader = new MultiFileReader(FALSE, FALSE, NULL, TRUE, FALSE);
    reader->SetTimeshift(true);
	}
	else
	{
		reader = new FileReader();
    reader->SetTimeshift(false);
	}
	reader->SetFileName(fileName);
	reader->OpenFile();


	Boolean deleteFidOnClose = true;
	TsStreamFileSource* newSource = new TsStreamFileSource(env, (FILE*)reader, deleteFidOnClose, preferredFrameSize, playTimePerFrame, channelType);
  __int64 fileSize = reader->GetFileSize();
	if (fileSize < 0) fileSize = 0;  
	newSource->fFileSize = fileSize;
	LogDebug("ts:size %I64d", fileSize);  
	return newSource;
}

TsStreamFileSource*
TsStreamFileSource::createNew(UsageEnvironment& env, FILE* fid,
							  Boolean deleteFidOnClose,
							  unsigned preferredFrameSize,
							  unsigned playTimePerFrame,
							  int channelType) {
								  if (fid == NULL) return NULL;

								  TsStreamFileSource* newSource = new TsStreamFileSource(env, fid, deleteFidOnClose, preferredFrameSize, playTimePerFrame, channelType);
								  FileReader* reader = (FileReader*)fid;
                  __int64 fileSize = reader->GetFileSize();
                	if (fileSize < 0) fileSize = 0;  
								  newSource->fFileSize = fileSize;
								  LogDebug("ts:createNew size %I64d", fileSize);  

								  return newSource;
}

void TsStreamFileSource::seekToByteAbsolute(u_int64_t byteNumber) 
{
	LogDebug("ts:seek %d",(DWORD)byteNumber);  
	FileReader* reader = (FileReader*)fFid;
	byteNumber/=188LL;
	byteNumber*=188LL;
	reader->SetFilePointer( (int64_t)byteNumber, FILE_BEGIN);
	m_buffer.Clear();
}

void TsStreamFileSource::seekToTimeAbsolute(CRefTime& seekTime, CTsDuration& duration) 
{
  	FileReader* reader = (FileReader*)fFid;
    double startTime = seekTime.Millisecs();
    startTime /= 1000.0f;
    LogDebug("StreamingServer::  Seek-> %f/%f", startTime, duration.Duration().Millisecs()/1000.0f);
    CTsFileSeek seek(duration);
    seek.SetFileReader(reader);
    
    for(int i(0) ; i < 4 ; i++)
    {
      bool eof = seek.Seek(seekTime);
      if (eof)
      {
        REFERENCE_TIME rollBackTime = reader->GetTimeshift() ? 5000000 : 30000000;  // 0.5s/3s 
        //reached end-of-file, try to seek to an earlier position
        if ((seekTime.m_time - rollBackTime) > 0)
        {
          seekTime.m_time -= rollBackTime;
        }
        else
        {
          break; //very short file....
        }
      }
      else
      {
        break; //we've succeeded
      }
    }
   
  	m_buffer.Clear();
}







void TsStreamFileSource::seekToByteRelative(int64_t offset) 
{
	FileReader* reader = (FileReader*)fFid;
	LogDebug("ts:seek rel %I64d/%I64d", offset, reader->GetFileSize());  
	offset/=188LL;
	offset*=188LL;
	reader->SetFilePointer((int64_t)offset, FILE_CURRENT);
	m_buffer.Clear();
}

TsStreamFileSource::TsStreamFileSource(UsageEnvironment& env, FILE* fid,
									   Boolean deleteFidOnClose,
									   unsigned preferredFrameSize,
									   unsigned playTimePerFrame,
									   int channelType)
									   : FramedFileSource(env, fid), fPreferredFrameSize(preferredFrameSize),
									   fPlayTimePerFrame(playTimePerFrame), fLastPlayTime(0), fFileSize(0),
									   fDeleteFidOnClose(deleteFidOnClose) 
{
	LogDebug("ts:ctor:%x",this);  
	FileReader* reader = (FileReader*)fFid;
	m_buffer.Clear();
	m_buffer.SetChannelType(channelType);
	m_buffer.SetFileReader(reader);

}

TsStreamFileSource::~TsStreamFileSource() 
{
	LogDebug("ts:dtor:%x",this);  
	if (fDeleteFidOnClose && fFid != NULL) 
	{
		FileReader* reader = (FileReader*)fFid;
		reader->CloseFile();
		delete reader;
		fFid=NULL;
		m_buffer.Clear();
	}
}

void TsStreamFileSource::doGetNextFrame() {
	//if (feof(fFid) || ferror(fFid)) {
	//  handleClosure(this);
	//  return;
	//}

	FileReader* reader = (FileReader*)fFid;

	// Try to read as many bytes as will fit in the buffer provided
	// (or "fPreferredFrameSize" if less)
	if (fPreferredFrameSize > 0 && fPreferredFrameSize < fMaxSize) {
		fMaxSize = fPreferredFrameSize;
	}
	
	long lReadBytes = 0;
	
	if (m_buffer.DequeFromBuffer(fTo,fMaxSize, &lReadBytes)!=S_OK)
	{
	  if (reader->GetTimeshift())
	  {
	    //Timeshifting is theoretically endless, so send NULL TS packets as there is not enough real data to send
    	if (m_buffer.GetNullTsBuffer(fTo,fMaxSize, &lReadBytes)!=S_OK)
    	{
  			LogDebug("ts:GetNullTsBuffer() timeout, closing stream"); //See TSBuffer.cpp for timeout value
  			handleClosure(this);
  			return;
    	}
	  }
	  else  //It's a recording, so not endless - assume end-of-file
	  {
			LogDebug("ts:eof reached, closing stream");  
			handleClosure(this);
			return;
	  }
	}
	
	fFrameSize = lReadBytes;

  __int64 fileSize = reader->GetFileSize();
  
	if (fileSize < 0) fileSize = 0;  
	fFileSize = fileSize;
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
