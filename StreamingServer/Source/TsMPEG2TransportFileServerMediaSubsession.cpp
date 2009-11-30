#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "TsMPEG2TransportFileServerMediaSubsession.h"
#include "SimpleRTPSink.hh"
#include "TsStreamFileSource.h"
#include "TsMPEG2TransportStreamFramer.h"
#include "TsFileDuration.h"

extern void LogDebug(const char *fmt, ...) ;

TsMPEG2TransportFileServerMediaSubsession* TsMPEG2TransportFileServerMediaSubsession::createNew(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource, Boolean timeshifting) 
{
	return new TsMPEG2TransportFileServerMediaSubsession(env, fileName, reuseFirstSource,timeshifting);
}

TsMPEG2TransportFileServerMediaSubsession::TsMPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,char const* fileName, Boolean reuseFirstSource, Boolean timeshifting)
: FileServerMediaSubsession(env, fileName, reuseFirstSource) 
{
	strcpy(m_fileName,fFileName);
	m_bTimeshifting = timeshifting;
}

TsMPEG2TransportFileServerMediaSubsession::~TsMPEG2TransportFileServerMediaSubsession() 
{
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* TsMPEG2TransportFileServerMediaSubsession::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) 
{
	estBitrate = 15000; // kbps, estimate

	// Create the video source:
	unsigned const inputDataChunkSize= TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
	TsStreamFileSource* fileSource= TsStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
	if (fileSource == NULL) return NULL;
	fFileSize = fileSource->fileSize();
	strcpy(m_fileName,fFileName);

	// Create a framer for the Transport Stream:
	return TsMPEG2TransportStreamFramer::createNew(envir(), fileSource);
}

RTPSink* TsMPEG2TransportFileServerMediaSubsession::createNewRTPSink(Groupsock* rtpGroupsock,unsigned char /*rtpPayloadTypeIfDynamic*/,FramedSource* /*inputSource*/) 
{
	return SimpleRTPSink::createNew(envir(), rtpGroupsock,
		33, 90000, "video", "mp2t",
		1, True, False /*no 'M' bit*/);
}
void TsMPEG2TransportFileServerMediaSubsession::seekStreamSource(FramedSource* inputSource, double seekNPT)
{  


	TsMPEG2TransportStreamFramer* framer=(TsMPEG2TransportStreamFramer*)inputSource;
	TsStreamFileSource* source=(TsStreamFileSource*)framer->inputSource();
	if (seekNPT==0.0)
	{
		source->seekToByteAbsolute(0LL);
		return;
	}
	float fileDuration=duration();
	if (seekNPT<0) seekNPT=0;
	if (seekNPT>(fileDuration-0.1)) seekNPT=(fileDuration-0.1);
	if (seekNPT <0) seekNPT=0;


	if(m_bTimeshifting){
		// This should fix the seeking being carried out to early.
		// if this happens (on multiseat) u get end up with a black screen and no sound, until u do a manaual seek.
		__int64	fileSizeInitial = filelength();
		__int64	fileSizeActual = filelength();
		DWORD dwTick=GetTickCount();	
		while (fileSizeInitial == fileSizeActual && (GetTickCount() - dwTick <=5000)) // lets exit the loop if filesize isnt increased for 5 secs.	
		{		
			LogDebug("waiting for TS file to grow ; %d, %d ", (DWORD)fileSizeInitial, (DWORD)fileSizeActual);
			fileSizeActual = filelength();	
			Sleep(100);	
		}
		LogDebug("TS file grown - now ready for the actual seek ; initial size %d, actual size %d, wait(ms) %d", (DWORD)fileSizeInitial, (DWORD)fileSizeActual, (GetTickCount() - dwTick));
	}

	double pos=seekNPT / fileDuration;
	__int64 fileSize=source->fileSize();
	pos*=fileSize;
	pos/=188;
	pos*=188;
	__int64 newPos=(__int64) pos;

	source->seekToByteAbsolute(newPos);
	LogDebug("ts seekStreamSource %f / %f ->%d", seekNPT,fileDuration, (DWORD)newPos);
}

float TsMPEG2TransportFileServerMediaSubsession::duration() const
{
	CTsFileDuration duration;
	duration.SetFileName((char*)m_fileName);
	duration.OpenFile();
	duration.UpdateDuration();
	duration.CloseFile();
	return duration.Duration();
}


__int64 TsMPEG2TransportFileServerMediaSubsession::filelength() const
{  
	__int64	fileSizeTmp = 0;

	CTsFileDuration duration;
	duration.SetFileName((char*)m_fileName);
	duration.OpenFile();

	fileSizeTmp = duration.GetFileSize();

	duration.CloseFile();
	return fileSizeTmp;
}
