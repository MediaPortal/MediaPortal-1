
#ifndef _MULTIFILE_SINK_HH
#define _MULTI_SINK_HH

#ifndef _MEDIA_SINK_HH
#include "MediaSink.hh"
#endif

#include "MultiFileWriter.h"
#include "packetsync.h" 

const int WRITE_BUFFER_SIZE = 188*10;

class CMultiWriterFileSink: public MediaSink,public CPacketSync 
{
public:
  static CMultiWriterFileSink* createNew(UsageEnvironment& env, char const* fileName,int minFiles, int maxFiles, ULONG maxFileSize,unsigned bufferSize = 20000,Boolean oneFilePerFrame = False);
  // "bufferSize" should be at least as large as the largest expected
  //   input frame.
  // "oneFilePerFrame" - if True - specifies that each input frame will
  //   be written to a separate file (using the presentation time as a
  //   file name suffix).  The default behavior ("oneFilePerFrame" == False)
  //   is to output all incoming data into a single file.

  virtual ~CMultiWriterFileSink();
  void addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime);
  // (Available in case a client wants to add extra data to the output file)

	virtual void OnTsPacket(byte* tsPacket);
  void ClearStreams();
protected:
  CMultiWriterFileSink(UsageEnvironment& env, MultiFileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix);
      // called only by createNew()

protected:
  static void afterGettingFrame(void* clientData, unsigned frameSize,unsigned numTruncatedBytes,struct timeval presentationTime,
				unsigned durationInMicroseconds);
  virtual void afterGettingFrame1(unsigned frameSize,struct timeval presentationTime);

  MultiFileWriter* fOutFid;
  unsigned char* fBuffer;
  unsigned fBufferSize;
  char* fPerFrameFileNamePrefix; // used if "oneFilePerFrame" is True
  char* fPerFrameFileNameBuffer; // used if "oneFilePerFrame" is True

  
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
