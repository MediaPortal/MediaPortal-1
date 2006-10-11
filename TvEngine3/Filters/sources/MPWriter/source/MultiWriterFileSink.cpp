#include <streams.h>
#include "MultiWriterFileSink.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"

////////// CMultiWriterFileSink //////////

CMultiWriterFileSink::CMultiWriterFileSink(UsageEnvironment& env, MultiFileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix) 
  : MediaSink(env), fOutFid(fid), fBufferSize(bufferSize) 
{
  fBuffer = new unsigned char[bufferSize];
 
}

CMultiWriterFileSink::~CMultiWriterFileSink() 
{
  if (fOutFid != NULL) 
  {
    fOutFid->CloseFile();
    delete fOutFid;
    fOutFid=NULL;
  }
}

CMultiWriterFileSink* CMultiWriterFileSink::createNew(UsageEnvironment& env, char const* fileName,unsigned bufferSize, Boolean oneFilePerFrame) 
{
  do 
  {
    MultiFileWriter* fid = new MultiFileWriter();
	  WCHAR wstrFileName[2048];
	  MultiByteToWideChar(CP_ACP,0,fileName,-1,wstrFileName,1+strlen(fileName));
    if (FAILED(fid->OpenFile(wstrFileName)))
    {
      delete fid;
      return NULL;
    }
    return new CMultiWriterFileSink(env, fid, bufferSize, NULL);
  } while (0);

  return NULL;
}

Boolean CMultiWriterFileSink::continuePlaying() 
{
  if (fSource == NULL) return False;

  fSource->getNextFrame(fBuffer, fBufferSize,afterGettingFrame, this,onSourceClosure, this);

  return True;
}

void CMultiWriterFileSink::afterGettingFrame(void* clientData, unsigned frameSize,
				 unsigned /*numTruncatedBytes*/,
				 struct timeval presentationTime,
				 unsigned /*durationInMicroseconds*/) {
  CMultiWriterFileSink* sink = (CMultiWriterFileSink*)clientData;
  sink->afterGettingFrame1(frameSize, presentationTime);
} 

void CMultiWriterFileSink::addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime) 
{
  if (fOutFid != NULL && data != NULL) 
  {
    fOutFid->Write(data, dataSize);
  }
}

void CMultiWriterFileSink::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
	CAutoLock BufferLock(&m_Lock);
  addData(fBuffer, frameSize, presentationTime);
  // Then try getting the next frame:
  continuePlaying();
}
