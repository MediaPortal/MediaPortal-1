
#ifndef _BYTE_MEMORYSTREAM_SOURCE_HH
#define _BYTE_MEMORYSTREAM_SOURCE_HH

#ifndef _FRAMED_FILE_SOURCE_HH
#include "FramedFileSource.hh"
#endif
#include "memorybuffer.h"

class CMemoryStreamSource: public FramedSource 
{
public:
  static CMemoryStreamSource* createNew(UsageEnvironment& env,
           CMemoryBuffer& buffer,
					 unsigned preferredFrameSize = 0,
					 unsigned playTimePerFrame = 0);
  // "preferredFrameSize" == 0 means 'no preference'
  // "playTimePerFrame" is in microseconds

  virtual ~CMemoryStreamSource();
      // 0 means zero-length, unbounded, or unknown

  void seekToByteAbsolute(u_int64_t byteNumber);
  void seekToByteRelative(int64_t offset);

  int DoRead(byte* fTo, int offset, int length);
protected:
  CMemoryStreamSource(UsageEnvironment& env,
           CMemoryBuffer& buffer,
		       unsigned preferredFrameSize,
		       unsigned playTimePerFrame);
	// called only by createNew()


private:
  // redefined virtual functions:
  virtual void doGetNextFrame();

private:
  unsigned fPreferredFrameSize;
  unsigned fPlayTimePerFrame;
  unsigned fLastPlayTime;
  CMemoryBuffer& m_buffer;
};

#endif
