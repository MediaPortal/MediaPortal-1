#ifndef _TSMPEG2_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH
#define _TSMPEG2_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH

#ifndef _FILE_SERVER_MEDIA_SUBSESSION_HH
#include "FileServerMediaSubsession.hh"
#endif

class TsMPEG2TransportFileServerMediaSubsession: public FileServerMediaSubsession{
public:
  static TsMPEG2TransportFileServerMediaSubsession*
  createNew(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource);

protected:
  TsMPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,
					  char const* fileName,
					  Boolean reuseFirstSource);
      // called only by createNew();
  virtual ~TsMPEG2TransportFileServerMediaSubsession();

private: // redefined virtual functions
  virtual FramedSource* createNewStreamSource(unsigned clientSessionId,
					      unsigned& estBitrate);
  virtual RTPSink* createNewRTPSink(Groupsock* rtpGroupsock,
                                    unsigned char rtpPayloadTypeIfDynamic,
				    FramedSource* inputSource);
};

#endif
