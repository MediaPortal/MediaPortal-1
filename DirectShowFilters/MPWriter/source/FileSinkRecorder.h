
#ifndef _MULTIFILE_SINK_HH
#define _MULTI_SINK_HH

#ifndef _MEDIA_SINK_HH
#include "MediaSink.hh"
#endif

#include "FileWriter.h"
#include "packetsync.h" 

const int RECORD_BUFFER_SIZE = 256000;

class CFileSinkRecorder: public MediaSink,public CPacketSync 
{
public:
  static CFileSinkRecorder* createNew(UsageEnvironment& env, char const* fileName,unsigned bufferSize = 20000);
  // "bufferSize" should be at least as large as the largest expected
  //   input frame.

  virtual ~CFileSinkRecorder();
  void addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime);
  // (Available in case a client wants to add extra data to the output file)

	virtual void OnTsPacket(byte* tsPacket);
  void ClearStreams();
protected:
  CFileSinkRecorder(UsageEnvironment& env, FileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix);
      // called only by createNew()

protected:
  static void afterGettingFrame(void* clientData, unsigned frameSize,unsigned numTruncatedBytes,struct timeval presentationTime,
				unsigned durationInMicroseconds);
  virtual void afterGettingFrame1(unsigned frameSize,struct timeval presentationTime);

  FileWriter* fOutFid;
  unsigned char* fBuffer;
  unsigned fBufferSize;
  
	__int64 m_startPcr;
	__int64 m_highestPcr;
  bool    m_bDetermineNewStartPcr;
  bool    m_bStartPcrFound;
  	byte*	m_pWriteBuffer;
	int     m_iWriteBufferPos;

private: // redefined virtual functions:
  CCritSec m_Lock;
  virtual Boolean continuePlaying();
};


#endif
