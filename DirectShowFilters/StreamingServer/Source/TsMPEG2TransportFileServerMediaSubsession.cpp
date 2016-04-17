#include "StdAfx.h"
#include "TsMPEG2TransportFileServerMediaSubsession.h"
#ifndef _SIMPLE_RTP_SINK_HH
#include "SimpleRTPSink.hh"
#endif
#include "TsStreamFileSource.h"
#include "TsMPEG2TransportStreamFramer.h"
#include "MultiFileReader.h"

extern void LogDebug(const char *fmt, ...) ;
extern void LogDebug(const wchar_t *fmt, ...) ;

TsMPEG2TransportFileServerMediaSubsession* TsMPEG2TransportFileServerMediaSubsession::createNew(UsageEnvironment& env,wchar_t const* fileName,Boolean reuseFirstSource, Boolean timeshifting, int channelType) 
{
	return new TsMPEG2TransportFileServerMediaSubsession(env, fileName, reuseFirstSource, timeshifting, channelType);
}

TsMPEG2TransportFileServerMediaSubsession::TsMPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,wchar_t const* fileName, Boolean reuseFirstSource, Boolean timeshifting, int channelType)
: FileServerMediaSubsession(env, 0, reuseFirstSource) 
{
	wcscpy(m_fileName, fileName);
	m_bTimeshifting = timeshifting;
	m_iChannelType = channelType;
	m_iDurationCount = 0;

  m_pDuration = new CTsDuration();
  m_pFileDuration = NULL;
  OpenFileDuration();
	LogDebug(L"TsMp2TFSMediaSubsession::ctor, m_fileName %s", m_fileName);  
}

TsMPEG2TransportFileServerMediaSubsession::~TsMPEG2TransportFileServerMediaSubsession() 
{
  CloseFileDuration();
  delete m_pDuration;
  m_pDuration = NULL;
	LogDebug(L"TsMp2TFSMediaSubsession::dtor, m_fileName %s", m_fileName);  
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* TsMPEG2TransportFileServerMediaSubsession::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) 
{
	estBitrate = 15000; // kbps, estimate

	// Create the video source:
	unsigned const inputDataChunkSize= TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
	TsStreamFileSource* fileSource= TsStreamFileSource::createNew(envir(), m_fileName, inputDataChunkSize, 0, m_iChannelType);
	if (fileSource == NULL) return NULL;
	fFileSize = fileSource->fileSize();
 
  // Create a framer for the Transport Stream:
	return TsMPEG2TransportStreamFramer::createNew(envir(), fileSource);
}

RTPSink* TsMPEG2TransportFileServerMediaSubsession::createNewRTPSink(Groupsock* rtpGroupsock,unsigned char /*rtpPayloadTypeIfDynamic*/,FramedSource* /*inputSource*/) 
{
	return SimpleRTPSink::createNew(envir(), rtpGroupsock,
		33, 90000, "video", "mp2t",
		1, True, False /*no 'M' bit*/);
}
void TsMPEG2TransportFileServerMediaSubsession::seekStreamSource(FramedSource* inputSource, double& seekNPT, double streamDuration, u_int64_t& numBytes)
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
	LogDebug("TsMp2TFSMediaSubsession::seekStreamSource %f / %f ->%d", seekNPT,fileDuration, (DWORD)newPos);
}

float TsMPEG2TransportFileServerMediaSubsession::duration() const
{
  if (m_pFileDuration)
  {
    // void CTsDuration::UpdateDuration(bool logging, bool background)
    m_pDuration->UpdateDuration(false, false);      
    m_pDuration->CloseBufferFiles(); //avoid leaving timeshift data files open

    if (m_iDurationCount < 1)
    {
	    LogDebug("TsMp2TFSMediaSubsession::duration(): %f s", m_pDuration->Duration().Millisecs() / 1000.0f);
      m_iDurationCount++;
    }

	  return m_pDuration->Duration().Millisecs() / 1000.0f;
  }
  return 10.0f; //fake it
}

__int64 TsMPEG2TransportFileServerMediaSubsession::filelength() const
{  
	__int64	fileSizeTmp = 0;
  if (m_pFileDuration)
  {
    fileSizeTmp = m_pFileDuration->GetFileSize();
	  //LogDebug("TsMp2TFSMediaSubsession::filelength() %I64", fileSizeTmp);
  }
  return fileSizeTmp;
}

void TsMPEG2TransportFileServerMediaSubsession::OpenFileDuration()
{
  if(m_pFileDuration==NULL)
  {
    if (wcsstr(m_fileName, L".tsbuffer")!=NULL)
    {
        //MultiFileReader::MultiFileReader(BOOL useFileNext, BOOL useDummyWrites, CCritSec* pFilterLock, BOOL useRandomAccess, BOOL extraLogging):
      m_pFileDuration = new MultiFileReader(FALSE, FALSE, NULL, TRUE, FALSE);
    }
    else
    {
      m_pFileDuration = new FileReader();
    }

    if(m_pFileDuration==NULL)
    {
	    LogDebug(L"TsMp2TFSMediaSubsession::OpenFileDuration() failed, m_fileName %s", m_fileName);  
      return;
    }
  
    // initialize duration estimator
    m_pFileDuration->SetFileName(m_fileName);
    if(FAILED(m_pFileDuration->OpenFile()))
    {
      CloseFileDuration();
      return;
    }
  
    m_pDuration->SetFileReader(m_pFileDuration);
	  LogDebug(L"TsMp2TFSMediaSubsession::OpenFileDuration() OK: %s", m_fileName);  
  }
}

void TsMPEG2TransportFileServerMediaSubsession::CloseFileDuration()
{
  if(m_pFileDuration)
  {
    m_pDuration->StopUpdate(true);
    m_pFileDuration->CloseFile();
    m_pDuration->SetFileReader(NULL);
    delete m_pFileDuration;
    m_pFileDuration = NULL;
  }
}
