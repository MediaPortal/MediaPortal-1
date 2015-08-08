#pragma once

#ifndef _FILE_SERVER_MEDIA_SUBSESSION_HH
#include "FileServerMediaSubsession.hh"
#endif
#ifndef _FRAMED_SOURCE_HH
#include "FramedSource.hh"
#endif
#ifndef _RTP_SINK_HH
#include "RTPSink.h"
#endif
#include "TsDuration.h"
#include "FileReader.h"

class TsMPEG2TransportFileServerMediaSubsession: public FileServerMediaSubsession{
public:
	static TsMPEG2TransportFileServerMediaSubsession*
		createNew(UsageEnvironment& env, wchar_t const* fileName, Boolean reuseFirstSource, Boolean timeshifting, int channelType);

protected:
	TsMPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,
		wchar_t const* fileName,
		Boolean reuseFirstSource, Boolean timeshifting, int channelType);
	// called only by createNew();
	virtual ~TsMPEG2TransportFileServerMediaSubsession();
	virtual float duration() const;
	virtual __int64 filelength() const;
  FileReader* OpenFileDuration() const;
  void CloseFileDuration(FileReader *pFileDuration) const;

  CTsDuration *m_pDuration;

private: // redefined virtual functions
	virtual void seekStreamSource(FramedSource* inputSource, double seekNPT);
	virtual FramedSource* createNewStreamSource(unsigned clientSessionId,
		unsigned& estBitrate);
	virtual RTPSink* createNewRTPSink(Groupsock* rtpGroupsock,
		unsigned char rtpPayloadTypeIfDynamic,
		FramedSource* inputSource);
	wchar_t m_fileName[MAX_PATH];
	Boolean m_bTimeshifting;
	int m_iChannelType;
};