#include <streams.h>
#include "MemorySink.h"
#include "GroupsockHelper.hh"

////////// CMemorySink //////////
#define SUBMIT_BUF_SIZE (1316*30)

CMemorySink::CMemorySink(UsageEnvironment& env,CMemoryBuffer& buffer, unsigned bufferSize) 
  : MediaSink(env),  
  fBufferSize(bufferSize),
  m_buffer(buffer)
{
  fBuffer = new unsigned char[bufferSize];
	m_pSubmitBuffer = new byte[SUBMIT_BUF_SIZE];
	m_iSubmitBufferPos=0;
}

CMemorySink::~CMemorySink() 
{
  delete[] fBuffer;
}

CMemorySink* CMemorySink::createNew(UsageEnvironment& env, CMemoryBuffer& buffer,unsigned bufferSize) 
{
  return new CMemorySink(env, buffer,bufferSize);
}

Boolean CMemorySink::continuePlaying() 
{
  if (fSource == NULL) return False;

  fSource->getNextFrame(fBuffer, fBufferSize,afterGettingFrame, this,onSourceClosure, this);
  return True;
}

void CMemorySink::afterGettingFrame(void* clientData, unsigned frameSize,unsigned /*numTruncatedBytes*/,struct timeval presentationTime,unsigned /*durationInMicroseconds*/) 
{
  CMemorySink* sink = (CMemorySink*)clientData;
  sink->afterGettingFrame1(frameSize, presentationTime);
	sink->continuePlaying();
} 

void CMemorySink::addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime) 
{
	CAutoLock BufferLock(&m_BufferLock);
	if (m_iSubmitBufferPos+dataSize > SUBMIT_BUF_SIZE)
	{
		m_buffer.PutBuffer(m_pSubmitBuffer, m_iSubmitBufferPos,0);
		m_iSubmitBufferPos=0;
	}
	memcpy(&m_pSubmitBuffer[m_iSubmitBufferPos],data,dataSize);
	m_iSubmitBufferPos+=dataSize;
}


void CMemorySink::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
  addData(fBuffer, frameSize, presentationTime);
}
