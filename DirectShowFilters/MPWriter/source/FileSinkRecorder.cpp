#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "FileSinkRecorder.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"
#include "MPEG1or2Demux.hh"

////////// CFileSinkRecorder //////////

extern void LogDebug(const char *fmt, ...) ;
extern void LogDebug(const wchar_t *fmt, ...);

CFileSinkRecorder::CFileSinkRecorder(UsageEnvironment& env, FileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix) 
  : MediaSink(env), fOutFid(fid), fBufferSize(bufferSize) 
{
  fBuffer = new unsigned char[bufferSize];
	m_startPcr=0;
	m_highestPcr=0;
  m_bDetermineNewStartPcr=false;
  m_bStartPcrFound=false;
  LogDebug("CFileSinkRecorder::ctor");
   	m_pWriteBuffer = new byte[RECORD_BUFFER_SIZE];
	m_iWriteBufferPos = 0;

}

CFileSinkRecorder::~CFileSinkRecorder() 
{
  LogDebug("CFileSinkRecorder::dtor");
  if (fOutFid != NULL) 
  {
    fOutFid->CloseFile();
    delete fOutFid;
    fOutFid=NULL;
  }
    delete[] fBuffer;
  delete[] m_pWriteBuffer;

}

CFileSinkRecorder* CFileSinkRecorder::createNew(UsageEnvironment& env, wchar_t const* fileName,unsigned bufferSize) 
{
  do 
  {
    LogDebug(L"CFileSinkRecorder::create file:%s",fileName);
    FileWriter* fid = new FileWriter();
    fid->SetFileName(fileName);
    if (FAILED(fid->OpenFile()))
    {
      LogDebug(L"CFileSinkRecorder::create file:%s failed", fileName);
      delete fid;
      return NULL;
    }
    return new CFileSinkRecorder(env, fid, bufferSize, NULL);
  } while (0);

  return NULL;
}

Boolean CFileSinkRecorder::continuePlaying() 
{
  if (fSource == NULL) return False;

  fSource->getNextFrame(fBuffer, fBufferSize,afterGettingFrame, this,onSourceClosure, this);

  return True;
}

void CFileSinkRecorder::afterGettingFrame(void* clientData, unsigned frameSize,
				 unsigned /*numTruncatedBytes*/,
				 struct timeval presentationTime,
				 unsigned /*durationInMicroseconds*/) {
  CFileSinkRecorder* sink = (CFileSinkRecorder*)clientData;
  sink->afterGettingFrame1(frameSize, presentationTime);
} 

void CFileSinkRecorder::addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime) 
{
}

void CFileSinkRecorder::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
	CAutoLock BufferLock(&m_Lock);
  OnRawData(fBuffer, frameSize);
  //addData(fBuffer, frameSize, presentationTime);
  // Then try getting the next frame:
  continuePlaying();
}


void CFileSinkRecorder::OnTsPacket(byte* tsPacket)
{
	if(m_iWriteBufferPos +TS_PACKET_LEN >= RECORD_BUFFER_SIZE){
		fOutFid->Write(m_pWriteBuffer, m_iWriteBufferPos);
		m_iWriteBufferPos = 0;
	}
	memcpy(&m_pWriteBuffer[m_iWriteBufferPos],tsPacket,TS_PACKET_LEN);
	m_iWriteBufferPos += TS_PACKET_LEN;
}

  
void CFileSinkRecorder::ClearStreams()
{
    m_bDetermineNewStartPcr=true;
}
