#pragma once
#pragma comment(lib,"LiveMedia555D.lib")
#pragma unmanaged

#include "StdAfx.h"
#include "RtpStream.h"
#include <string>

void LogDebugRtp(const wchar_t *fmt, ...)
{
	va_list ap;
	va_start(ap,fmt);

	va_start(ap,fmt);
	vswprintf_s(logbufferw, fmt, ap);
	va_end(ap); 

	wchar_t folder[MAX_PATH];
	wchar_t fileName[MAX_PATH];
	::SHGetSpecialFolderPathW(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);
	swprintf_s(fileName, L"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log", folder);

	FILE* fp = _wfopen(fileName,L"a+, ccs=UTF-8");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fwprintf(fp,L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,systemTime.wMilliseconds,
			logbufferw);
		fclose(fp);
	}
};

void LogDebugRtp(const char *fmt, ...)
{
	va_list ap;
	va_start(ap,fmt);

	va_start(ap,fmt);
	vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	MultiByteToWideChar(CP_ACP, 0, logbuffer, -1, logbufferw, sizeof(logbuffer)/sizeof(wchar_t));
	LogDebugRtp(L"%s", logbufferw);
};

void afterPlaying(void* clientData) {
  //MPrtpStream* streamState = (MPrtpStream*)clientData;
  LogDebugRtp("afterPlaying() called");
  MPrtpStream* streamState = (MPrtpStream*)clientData;
  LogDebugRtp("afterPlaying() stop playing");
  if (streamState->videoSink != NULL)
	streamState->videoSink->stopPlaying();
  LogDebugRtp("afterPlaying() close video source");
  if (streamState->videoSource != NULL)
	Medium::close(streamState->videoSource);

  if (streamState->stop == 's')
	  return;

  // Note that this also closes the input file that this source read from.
  LogDebugRtp("afterPlaying() reinitialise fileSource");
  unsigned const inputDataChunkSize
	  = TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
  streamState->fileSource = ByteStreamMemoryBufferSource::createNew(*streamState->env, &streamState->stop, true, inputDataChunkSize);
  LogDebugRtp("afterPlaying() wait for data");
  /*while (streamState->fileSource->getReadSizeAvailable() < TRANSPORT_PACKET_SIZE * TRANSPORT_PACKETS_PER_NETWORK_PACKET) {
	  LogDebugRtp("afterPlaying() - BytesAvailable %d", streamState->fileSource->getReadSizeAvailable());
	  Sleep(100);
  }*/
  LogDebugRtp("afterPlaying() play!");
  streamState->play();
}

MPrtpStream::MPrtpStream() {
	// Begin by setting up our usage environment:
	TaskScheduler* scheduler = BasicTaskScheduler::createNew();
	env = BasicUsageEnvironment::createNew(*scheduler);

	unsigned const inputDataChunkSize
		= TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;

	fileSource = ByteStreamMemoryBufferSource::createNew(*env, &stop, true, inputDataChunkSize);
}

void MPrtpStream::MPrtpStreamCreate(/*char* stopLoop, */const char* destinationAddressStr, int _rtpPort, char* fileName)
{
	LogDebugRtp("begin RtpSetup");

	
	const unsigned short rtpPortNum = _rtpPort;
	const unsigned short rtcpPortNum = rtpPortNum+1;
	const unsigned char ttl = 7; // low, in case routers don't admin scope
	inputFileName = fileName;
	stop = 0;

	LogDebugRtp("filename: %s",inputFileName);
	LogDebugRtp("client IP: %s", destinationAddressStr);
	char buffer [50];
	sprintf_s(buffer, "client RTP-Port: %d", _rtpPort);
	LogDebugRtp(buffer);
	
	// Begin by setting up our usage environment:
	//TaskScheduler* scheduler = BasicTaskScheduler::createNew();
	//env = BasicUsageEnvironment::createNew(*scheduler);

	struct in_addr destinationAddress;
	destinationAddress.s_addr = our_inet_addr(destinationAddressStr);
	const Port rtpPort(rtpPortNum);
	const Port rtcpPort(rtcpPortNum);

	Groupsock rtpGroupsock(*env, destinationAddress, rtpPort, ttl);
	Groupsock rtcpGroupsock(*env, destinationAddress, rtcpPort, ttl);

	// Create an appropriate 'RTP sink' from the RTP 'groupsock':
	videoSink =
		SimpleRTPSink::createNew(*env, &rtpGroupsock, 33, 90000, "video", "MP2T",
					1, True, False /*no 'M' bit*/);
	
	// Create (and start) a 'RTCP instance' for this RTP sink:
	const unsigned estimatedSessionBandwidth = 5000; // in kbps; for RTCP b/w share
	const unsigned maxCNAMElen = 100;
	unsigned char CNAME[maxCNAMElen+1];
	gethostname((char*)CNAME, maxCNAMElen);
	CNAME[maxCNAMElen] = '\0'; // just in case

	RTCPInstance::createNew(*env, &rtcpGroupsock,
			    estimatedSessionBandwidth, CNAME,
			    videoSink, NULL /* we're a server */, isSSM);
	// Note: This starts RTCP running automatically

	// Finally, start the streaming:
	LogDebugRtp("Beginning streaming...");
	play();

	env->taskScheduler().doEventLoop(&stop/*stopLoop*/); // does not return
	LogDebugRtp("Play() leaving function => stop");
	//return 0; // ok
}

// simple warpper
void MPrtpStream::write(unsigned char *dataPtr, int numBytes) {
	if (fileSource != 0) {
		fileSource->Write(dataPtr, numBytes);
	}
	else {
		LogDebugRtp("FileSource is Null");
	}
}

void MPrtpStream::play() {
  /*unsigned const inputDataChunkSize
    = TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;

  // Open the input file as a 'byte-stream file source':
  ByteStreamFileSource* fileSource
    = ByteStreamFileSource::createNew(*env, inputFileName, inputDataChunkSize);
  if (fileSource == NULL) {
    LogDebugRtp("Unable to open file \"%s\" as a byte-stream file source", inputFileName);
    exit(1);
  }*/

  // open memory stream
  //ByteStreamMemoryBufferSource* fileSource = ByteStreamMemoryBufferSource::createNew(*env, 18800);

  // Create a 'framer' for the input source (to give us proper inter-packet gaps):
  videoSource = MPEG2TransportStreamFramer::createNew(*env, fileSource);

  // Finally, start playing:
  LogDebugRtp("Beginning to read from file...");
  videoSink->startPlaying(*videoSource, afterPlaying, this/*videoSink*/);
}

void MPrtpStream::RtpStop() {
  LogDebugRtp("...stop");
  stop = 's';
  /*if (videoSink != NULL)
	videoSink->stopPlaying();
  LogDebugRtp("RtpStop() stopPlaying() finished");
  Medium::close(videoSource);
  LogDebugRtp("RtpStop() close VideoSource finished");
  */
  // Note that this also closes the input file that this source read from.
}

void* CreateClassInstance()
{
	return static_cast< void* > (new MPrtpStream);
}