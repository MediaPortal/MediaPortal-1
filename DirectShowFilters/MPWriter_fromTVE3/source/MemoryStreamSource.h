
#ifndef _BYTE_MEMORYSTREAM_SOURCE_HH
#define _BYTE_MEMORYSTREAM_SOURCE_HH

#ifndef _FRAMED_FILE_SOURCE_HH
#include "FramedFileSource.hh"
#endif
#include "memorybuffer.h"

class CMemoryStreamSource: public FramedFileSource 
{
public:
  static CMemoryStreamSource* createNew(UsageEnvironment& env,
					 char const* fileName,
           CMemoryBuffer& buffer,
					 unsigned preferredFrameSize = 0,
					 unsigned playTimePerFrame = 0);
  // "preferredFrameSize" == 0 means 'no preference'
  // "playTimePerFrame" is in microseconds

  static CMemoryStreamSource* createNew(UsageEnvironment& env,
					 FILE* fid,
           CMemoryBuffer& buffer,
					 Boolean deleteFidOnClose = False,
					 unsigned preferredFrameSize = 0,
					 unsigned playTimePerFrame = 0);
      // an alternative version of "createNew()" that's used if you already have
      // an open file.

  virtual ~CMemoryStreamSource();
  u_int64_t fileSize() const { return fFileSize; }
      // 0 means zero-length, unbounded, or unknown

  void seekToByteAbsolute(u_int64_t byteNumber);
  void seekToByteRelative(int64_t offset);

  int DoRead(byte* fTo, int offset, int length);
protected:
  CMemoryStreamSource(UsageEnvironment& env,
		       FILE* fid, 
           CMemoryBuffer& buffer,
           Boolean deleteFidOnClose,
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
  u_int64_t fFileSize;
  Boolean fDeleteFidOnClose;
  CMemoryBuffer& m_buffer;
};

#endif
