#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "TsMPEG2TransportFileServerMediaSubsession.h"
#include "SimpleRTPSink.hh"
#include "TsStreamFileSource.h"
#include "TsMPEG2TransportStreamFramer.h"

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

  if (strstr(m_fileName,".tsbuffer")!=NULL)
  {
    m_pFileDuration = new MultiFileReader();
  }
  else
  {
    m_pFileDuration = new FileReader();
  }

  //open file
  WCHAR wFileName[1024];
  MultiByteToWideChar(CP_ACP,0,m_fileName,-1,wFileName,1024);
  m_pFileDuration->SetFileName(wFileName);
  m_pFileDuration->OpenFile();

  //get file duration
  m_pDuration = new CTsDuration();
  m_pDuration->SetFileReader(m_pFileDuration);
  m_pDuration->UpdateDuration();
  m_pFileDuration->CloseFile();

}

TsMPEG2TransportFileServerMediaSubsession::~TsMPEG2TransportFileServerMediaSubsession() 
{
  delete m_pDuration;
  m_pDuration = NULL;
  delete m_pFileDuration;
  m_pFileDuration = NULL;
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
	double fileDuration=duration();

	if (seekNPT>(fileDuration-0.1)) seekNPT=(fileDuration-0.1);
	if (seekNPT <0) seekNPT=0;

	double pos=seekNPT / fileDuration;
	__int64 fileSize=source->fileSize();
	pos*=fileSize;
	pos/=188;
	pos*=188;
	__int64 newPos=(__int64) pos;

//	source->seekToByteAbsolute(newPos);
  source->seekToTimeAbsolute(CRefTime((LONG)(seekNPT*1000.0)), *m_pDuration) ;
	LogDebug("ts seekStreamSource %f / %f ->%d", seekNPT,fileDuration, (DWORD)newPos);
}

float TsMPEG2TransportFileServerMediaSubsession::duration() const
{
  m_pFileDuration->OpenFile();
  m_pDuration->UpdateDuration();
  m_pFileDuration->CloseFile();
	return m_pDuration->Duration().Millisecs() / 1000.0f;
}

__int64 TsMPEG2TransportFileServerMediaSubsession::filelength() const
{  
	__int64	fileSizeTmp = 0;

  m_pFileDuration->OpenFile();
  fileSizeTmp = m_pFileDuration->GetFileSize();
  m_pFileDuration->CloseFile();

	return fileSizeTmp;
}

