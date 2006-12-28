#pragma once

#ifndef _MEDIA_SINK_HH
#include "MediaSink.hh"
#endif

#include "memorybuffer.h" 

class CMemorySink: public MediaSink
{
public:
  static CMemorySink* createNew(UsageEnvironment& env, CMemoryBuffer& buffer,unsigned bufferSize = 20000);
  void addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime);

protected:
  CMemorySink(UsageEnvironment& env, CMemoryBuffer& buffer, unsigned bufferSize = 20000);
  virtual ~CMemorySink(void);

protected:
  static void afterGettingFrame(void* clientData, unsigned frameSize,unsigned numTruncatedBytes,struct timeval presentationTime,unsigned durationInMicroseconds);
  virtual void afterGettingFrame1(unsigned frameSize,struct timeval presentationTime);

  unsigned char* fBuffer;
  unsigned fBufferSize;
  CMemoryBuffer& m_buffer;

private: // redefined virtual functions:
  virtual Boolean continuePlaying();
	CCritSec m_BufferLock;
	byte* m_pSubmitBuffer;
	int   m_iSubmitBufferPos;
	bool  m_bReEntrant;
	
};
