
#ifndef _BASE_FILE_WRITER_SINK_HH
#define _BASE_FILE_WRITER_SINK_HH

#include "MediaSink.hh"
#include "FileWriter.h"
#include "packetsync.h" 

const int WRITE_BUFFER_SIZE = 188*10;
const int RECORD_BUFFER_SIZE = 255868;
const int TS_SIZE = 188;

class CBaseFileWriterSink: public MediaSink,public CPacketSync 
{
public:
	virtual ~CBaseFileWriterSink();
	void addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime);
	// (Available in case a client wants to add extra data to the output file)

	virtual void Flush();
	void ClearStreams();
protected:
	CBaseFileWriterSink(UsageEnvironment& env, unsigned bufferSize,char const* perFrameFileNamePrefix, int writeBufferSize);
	// called only by createNew()

protected:
	static void afterGettingFrame(void* clientData, unsigned frameSize,unsigned numTruncatedBytes,struct timeval presentationTime,
		unsigned durationInMicroseconds);
	virtual void afterGettingFrame1(unsigned frameSize,struct timeval presentationTime);

	unsigned char* fBuffer;
	unsigned fBufferSize;

	__int64 m_startPcr;
	__int64 m_highestPcr;
	bool    m_bDetermineNewStartPcr;
	bool    m_bStartPcrFound;
	byte*	m_pWriteBuffer;
	int     m_iWriteBufferPos;
	CCritSec m_Lock;
	virtual Boolean continuePlaying();
};

#endif
