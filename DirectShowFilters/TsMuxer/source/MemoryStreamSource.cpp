#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "MemoryStreamSource.h"
#include "InputFile.hh"
#include "GroupsockHelper.hh"

////////// CMemoryStreamSource //////////

extern void LogDebug(const char *fmt, ...) ;

CMemoryStreamSource* CMemoryStreamSource::createNew(UsageEnvironment& env, CMemoryBuffer& buffer,unsigned preferredFrameSize,unsigned playTimePerFrame) 
{
	CMemoryStreamSource* newSource= new CMemoryStreamSource(env, buffer,preferredFrameSize, playTimePerFrame);
	return newSource;
}

void CMemoryStreamSource::seekToByteAbsolute(u_int64_t byteNumber) 
{
	//SeekFile64(fFid, (int64_t)byteNumber, SEEK_SET);
}

void CMemoryStreamSource::seekToByteRelative(int64_t offset) 
{
	//SeekFile64(fFid, offset, SEEK_CUR);
}

CMemoryStreamSource::CMemoryStreamSource(UsageEnvironment& env, CMemoryBuffer& buffer,unsigned preferredFrameSize,unsigned playTimePerFrame)
: FramedSource(env), fPreferredFrameSize(preferredFrameSize),
fPlayTimePerFrame(playTimePerFrame), fLastPlayTime(0), m_buffer(buffer)
{
	LogDebug("CMemoryStreamSource::ctor()");
}

CMemoryStreamSource::~CMemoryStreamSource() 
{
	LogDebug("CMemoryStreamSource::dtor()");
}

void CMemoryStreamSource::doGetNextFrame() 
{
	// Try to read as many bytes as will fit in the buffer provided
	// (or "fPreferredFrameSize" if less)
	if (fPreferredFrameSize > 0 && fPreferredFrameSize < fMaxSize) 
	{
		fMaxSize = fPreferredFrameSize;
	}
	fFrameSize = DoRead(fTo, 1, fMaxSize);

	if (fFrameSize==0) 
	{
		LogDebug("CMemoryStreamSource::doGetNextFrame->handleClosure()");
		handleClosure(this);
		return;
	}

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


int CMemoryStreamSource::DoRead(byte* fTo, int offset, int length)
{
	return m_buffer.ReadFromBuffer(fTo,length,0);
}