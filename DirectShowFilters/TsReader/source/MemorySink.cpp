#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "MemorySink.h"
#include "GroupsockHelper.hh"

////////// CMemorySink //////////
#define SUBMIT_BUF_SIZE (1316*30)
extern void LogDebug(const char *fmt, ...) ;

CMemorySink::CMemorySink(UsageEnvironment& env,CMemoryBuffer& buffer, unsigned bufferSize) 
  : MediaSink(env),  
  fBufferSize(bufferSize),
  m_buffer(buffer)
{
  LogDebug("CMemorySink::ctor");
  fBuffer = new unsigned char[bufferSize];
  m_pSubmitBuffer = new byte[SUBMIT_BUF_SIZE];
  m_iSubmitBufferPos=0;
  m_bReEntrant=false;
}

CMemorySink::~CMemorySink() 
{
  LogDebug("CMemorySink::dtor");
  delete[] fBuffer;
  delete[] m_pSubmitBuffer;
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
static int testsize=0;
void CMemorySink::addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime) 
{
  if (testsize ==0)
  {
    LogDebug("CMemorySink:addData");
    testsize=1;
  }
  if (dataSize==0) return;
  if (data==NULL) return;
  if (m_bReEntrant)
  {
    LogDebug("REENTRANT IN MEMORYSINK.CPP");
    return;
  }
  CAutoLock BufferLock(&m_BufferLock);
  m_bReEntrant=true;

  m_buffer.PutBuffer(data, dataSize);

  m_bReEntrant=false;
}


void CMemorySink::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
  addData(fBuffer, frameSize, presentationTime);
}
