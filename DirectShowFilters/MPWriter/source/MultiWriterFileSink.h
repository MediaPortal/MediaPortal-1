
#ifndef _MULTIFILE_SINK_HH
#define _MULTIFILE_SINK_HH

#include "BaseFileWriterSink.h"
#include "MultiFileWriter.h"

class CMultiWriterFileSink: public CBaseFileWriterSink
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
	virtual void OnTsPacket(byte* tsPacket);
protected:
	CMultiWriterFileSink(UsageEnvironment& env, MultiFileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix);
	// called only by createNew()

protected:
	MultiFileWriter* fOutFid;
};


#endif
