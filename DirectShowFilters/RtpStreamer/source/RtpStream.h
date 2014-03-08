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
	UsageEnvironment*		env;
	ByteStreamMemoryBufferSource* fileSource;
	Boolean static const isSSM =	False;

	char const* inputFileName;

  public:
    RTPSink*				videoSink;
	FramedSource*			videoSource;
	char					stop;
	  
	MPrtpStream ();
	void MPrtpStreamCreate (/*char*, */char*, int, char*);
	void RtpStop();
	void play();
	void write(unsigned char *dataPtr, int numBytes);
};