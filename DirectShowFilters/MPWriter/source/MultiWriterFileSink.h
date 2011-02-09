
#ifndef _MULTIFILE_SINK_HH
#define _MULTI_SINK_HH

#ifndef _MEDIA_SINK_HH
#include "MediaSink.hh"
#endif

#include "MultiFileWriter.h"
#include "packetsync.h" 

#define TS_PACKET_SIZE	188
#define THROTTLE_MAXIMUM_RADIO_PACKETS			10		//	Throttle to 10 for radio
#define THROTTLE_MAXIMUM_TV_PACKETS				172		//	Throttle to 172 fo tv for reduced disk IOs


//	Incremental buffer sizes
#define NUMBER_THROTTLE_BUFFER_SIZES	20


enum ChannelType
{
	TV = 0,
	Radio = 1,
};


class CMultiWriterFileSink: public MediaSink,public CPacketSync 
{
public:
  static CMultiWriterFileSink* createNew(UsageEnvironment& env, char const* fileName,int minFiles, int maxFiles, ULONG maxFileSize,unsigned bufferSize = 20000,Boolean oneFilePerFrame = False, int channelType = 0);
  // "bufferSize" should be at least as large as the largest expected
  //   input frame.
  // "oneFilePerFrame" - if True - specifies that each input frame will
  //   be written to a separate file (using the presentation time as a
  //   file name suffix).  The default behavior ("oneFilePerFrame" == False)
  //   is to output all incoming data into a single file.

  virtual ~CMultiWriterFileSink();
  void addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime);
  // (Available in case a client wants to add extra data to the output file)

  void SetChannelType(int channelType);
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
	int		m_iWriteBufferSize;
  int				m_iWriteBufferThrottle;
  BOOL				m_bThrottleAtMax;
  ChannelType		m_eChannelType;
  int			m_iThrottleBufferSizes[NUMBER_THROTTLE_BUFFER_SIZES];

private: // redefined virtual functions:
  CCritSec m_Lock;
  virtual Boolean continuePlaying();
};


#endif
