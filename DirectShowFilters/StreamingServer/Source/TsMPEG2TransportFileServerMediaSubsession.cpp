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

  m_pDuration = new CTsDuration();
}

TsMPEG2TransportFileServerMediaSubsession::~TsMPEG2TransportFileServerMediaSubsession() 
{
  delete m_pDuration;
  m_pDuration = NULL;
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
  FileReader *pFileDuration = OpenFileDuration();
  if (pFileDuration)
  {
    m_pDuration->UpdateDuration();
    CloseFileDuration(pFileDuration);
	  return m_pDuration->Duration().Millisecs() / 1000.0f;
  }
  return 10.0f; //fake it
}

__int64 TsMPEG2TransportFileServerMediaSubsession::filelength() const
{  
	__int64	fileSizeTmp = 0;
  FileReader *pFileDuration = OpenFileDuration();
  if (pFileDuration)
  {
    fileSizeTmp = pFileDuration->GetFileSize();
	  CloseFileDuration(pFileDuration);
  }
  return fileSizeTmp;
}

FileReader* TsMPEG2TransportFileServerMediaSubsession::OpenFileDuration() const
{
  FileReader *pFileDuration;

  if (strstr(m_fileName,".tsbuffer")!=NULL)
  {
    pFileDuration = new MultiFileReader();
  }
  else
  {
    pFileDuration = new FileReader();
  }

  // initialize duration estimator
  WCHAR wFileName[1024];
  MultiByteToWideChar(CP_ACP,0,m_fileName,-1,wFileName,1024);
  pFileDuration->SetFileName(wFileName);
  if(FAILED(pFileDuration->OpenFile()))
  {
    CloseFileDuration(pFileDuration);
    pFileDuration = NULL;
  }

  m_pDuration->SetFileReader(pFileDuration);
  return pFileDuration;
}

void TsMPEG2TransportFileServerMediaSubsession::CloseFileDuration(FileReader *pFileDuration) const
{
  if(pFileDuration)
  {
    pFileDuration->CloseFile();
    m_pDuration->SetFileReader(NULL);
    delete pFileDuration;
  }
}
