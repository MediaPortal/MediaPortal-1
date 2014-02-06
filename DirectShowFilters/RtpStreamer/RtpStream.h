#pragma once
#pragma comment(lib,"LiveMedia555D.lib")
#pragma unmanaged

#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MPTaskScheduler.h"
#include "GroupsockHelper.hh"
#include <shlobj.h>

static char logbuffer[2000]; 
static wchar_t logbufferw[2000];
void LogDebugRtp(const wchar_t *);

void LogDebugRtp(const char , ...);

class __declspec(dllexport) MPrtpStream {
	UsageEnvironment*		env;
	Boolean static const isSSM =	False;

	char const* inputFileName;
	#define TRANSPORT_PACKET_SIZE 188
	#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7

  public:
    RTPSink*				videoSink;
	FramedSource*			videoSource;
	char					stop;
	  
	MPrtpStream ();
	void MPrtpStreamCreate (/*char*, */char*, int, char*);
	void RtpStop();
	void play();
};