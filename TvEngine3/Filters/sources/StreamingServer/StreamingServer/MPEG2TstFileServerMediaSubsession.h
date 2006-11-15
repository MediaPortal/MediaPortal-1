#pragma once
  
#ifndef _MPEG2TS_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH
#define _MPEG2TS_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH

#ifndef _FILE_SERVER_MEDIA_SUBSESSION_HH
#include "FileServerMediaSubsession.hh"
#endif

class MPEG2TstFileServerMediaSubsession: public FileServerMediaSubsession{
public:
  static MPEG2TstFileServerMediaSubsession* createNew(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource);

protected:
  MPEG2TstFileServerMediaSubsession(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource);
  // called only by createNew();
  virtual ~MPEG2TstFileServerMediaSubsession();

private: // redefined virtual functions
  virtual FramedSource* createNewStreamSource(unsigned clientSessionId,unsigned& estBitrate);
  virtual RTPSink* createNewRTPSink(Groupsock* rtpGroupsock,unsigned char rtpPayloadTypeIfDynamic,FramedSource* inputSource);
};

#endif
