
#ifndef _FILE_SINK_RECORDER_HH
#define _FILE_SINK_RECORDER_HH

#include "BaseFileWriterSink.h"
#include "FileWriter.h"

class CFileSinkRecorder: public CBaseFileWriterSink
{
public:
	static CFileSinkRecorder* createNew(UsageEnvironment& env, char const* fileName,unsigned bufferSize = 20000);
	// "bufferSize" should be at least as large as the largest expected
	//   input frame.

	virtual ~CFileSinkRecorder();
	virtual void OnTsPacket(byte* tsPacket);
protected:
	CFileSinkRecorder(UsageEnvironment& env, FileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix);
	// called only by createNew()

protected:
	FileWriter* fOutFid;
};


#endif
