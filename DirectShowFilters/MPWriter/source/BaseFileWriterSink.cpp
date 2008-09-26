#include <streams.h>
#include "FileSinkRecorder.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"
#include "MPEG1or2Demux.hh"

////////// CBaseFileWriterSink //////////

extern void LogDebug(const char *fmt, ...) ;
CBaseFileWriterSink::CBaseFileWriterSink(UsageEnvironment& env, unsigned bufferSize,char const* perFrameFileNamePrefix, int writeBufferSize) 
: MediaSink(env), fBufferSize(bufferSize) 
{
	fBuffer = new unsigned char[bufferSize];
	m_startPcr=0;
	m_highestPcr=0;
	m_bDetermineNewStartPcr=false;
	m_bStartPcrFound=false;
	m_pWriteBuffer = new byte[writeBufferSize];
	m_iWriteBufferPos = 0;

}

CBaseFileWriterSink::~CBaseFileWriterSink() 
{
	delete [] fBuffer;
	delete [] m_pWriteBuffer;
}

Boolean CBaseFileWriterSink::continuePlaying() 
{
	if (fSource == NULL) return False;

	fSource->getNextFrame(fBuffer, fBufferSize,afterGettingFrame, this,onSourceClosure, this);

	return True;
}

void CBaseFileWriterSink::afterGettingFrame(void* clientData, unsigned frameSize,
										  unsigned /*numTruncatedBytes*/,
struct timeval presentationTime,
	unsigned /*durationInMicroseconds*/) {
		CBaseFileWriterSink* sink = (CBaseFileWriterSink*)clientData;
		sink->afterGettingFrame1(frameSize, presentationTime);
} 

void CBaseFileWriterSink::addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime) 
{
}

void CBaseFileWriterSink::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
	CAutoLock BufferLock(&m_Lock);
	OnRawData(fBuffer, frameSize);
	//addData(fBuffer, frameSize, presentationTime);
	// Then try getting the next frame:
	continuePlaying();
}

void CBaseFileWriterSink::Flush()
{
	m_iWriteBufferPos = 0;
}


void CBaseFileWriterSink::ClearStreams()
{
    m_bDetermineNewStartPcr=true;
}

