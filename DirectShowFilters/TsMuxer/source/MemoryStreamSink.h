#pragma once

#ifndef _MEDIA_SINK_HH
#include "MediaSink.hh"
#endif

#include "memorybuffer.h" 
#include "TsOutputPin.h"
#include "PacketSync.h"

const int SUBMIT_BUFFER_SIZE = 256000;

class CMemoryStreamSink: public MediaSink, public CPacketSync
{
public:
	static CMemoryStreamSink* createNew(UsageEnvironment& env, CTsMuxerTsOutputPin* outputPin,unsigned bufferSize = 20000);
	//void addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime);
	virtual void OnTsPacket(byte* tsPacket);

protected:
	CMemoryStreamSink(UsageEnvironment& env, CTsMuxerTsOutputPin* muxer,unsigned bufferSize);
	virtual ~CMemoryStreamSink(void);

protected:
	static void afterGettingFrame(void* clientData, unsigned frameSize,unsigned numTruncatedBytes,struct timeval presentationTime,unsigned durationInMicroseconds);
	virtual void afterGettingFrame1(unsigned frameSize,struct timeval presentationTime);

	unsigned char* fBuffer;
	unsigned fBufferSize;

private: // redefined virtual functions:
	virtual Boolean continuePlaying();
	CCritSec m_BufferLock;
	bool  m_bReEntrant;
	CTsMuxerTsOutputPin* m_pTsOutputPin;
  	byte*	m_pWriteBuffer;
	int     m_iWriteBufferPos;
	IMediaSample* m_pSample;

};
