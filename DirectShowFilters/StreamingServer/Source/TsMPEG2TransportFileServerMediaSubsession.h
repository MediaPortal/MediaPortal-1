#ifndef _TSMPEG2_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH
#define _TSMPEG2_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH

#ifndef _FILE_SERVER_MEDIA_SUBSESSION_HH
#include "FileServerMediaSubsession.hh"
#endif
#include "TsDuration.h"

class TsMPEG2TransportFileServerMediaSubsession: public FileServerMediaSubsession{
public:
	static TsMPEG2TransportFileServerMediaSubsession*
		createNew(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource, Boolean timeshifting);

protected:
	TsMPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,
		char const* fileName,
		Boolean reuseFirstSource, Boolean timeshifting);
	// called only by createNew();
	virtual ~TsMPEG2TransportFileServerMediaSubsession();
	virtual float duration() const;
	virtual __int64 filelength() const;

  CTsDuration *m_pDuration;
  FileReader *m_pFileDuration;

private: // redefined virtual functions
	virtual void seekStreamSource(FramedSource* inputSource, double seekNPT);
	virtual FramedSource* createNewStreamSource(unsigned clientSessionId,
		unsigned& estBitrate);
	virtual RTPSink* createNewRTPSink(Groupsock* rtpGroupsock,
		unsigned char rtpPayloadTypeIfDynamic,
		FramedSource* inputSource);
	char m_fileName[MAX_PATH];
	Boolean m_bTimeshifting;
};

#endif
