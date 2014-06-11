#pragma once
#pragma comment(lib,"LiveMedia555D.lib")
#pragma unmanaged

#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MPTaskScheduler.h"
#include "GroupsockHelper.hh"
#include <shlobj.h>
#include "RtpStreamInterface.h"
#include "ByteStreamMemoryBufferSource.hh"
#include "config.h"

static char logbuffer[2000]; 
static wchar_t logbufferw[2000];
void LogDebugRtp(const wchar_t *);

void LogDebugRtp(const char , ...);

void* CreateClassInstance();

class __declspec(dllexport) MPrtpStream : public IMPrtpStream {
	
	Boolean static const isSSM =	False;

	char const* inputFileName;

  public:
    RTPSink*				videoSink;
	FramedSource*			videoSource;
	ByteStreamMemoryBufferSource* fileSource;
	UsageEnvironment*		env;
	char					stop;
	  
	MPrtpStream ();
	void MPrtpStreamCreate (/*char*, */const char*, int, char*);
	void RtpStop();
	void RtpStart();
	void play();
	void write(unsigned char *dataPtr, int numBytes);
};