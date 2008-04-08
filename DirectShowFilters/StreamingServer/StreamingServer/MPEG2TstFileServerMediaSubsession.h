#pragma once
  
#ifndef _MPEG2TS_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH
#define _MPEG2TS_TRANSPORT_FILE_SERVER_MEDIA_SUBSESSION_HH

#ifndef _FILE_SERVER_MEDIA_SUBSESSION_HH
#include "FileServerMediaSubsession.hh"
#endif

#include "SimpleRTPSink.hh"
#include "ByteStreamFileSource.hh"
#include "MPEG2TransportStreamFramer.hh"
#include "MPEG1or2Demux.hh"
#include "MPEG2TransportStreamFromPESSource.hh"
#include "TsFileDuration.h"
#include "TsStreamFileSource.hh"


class MPEG2TstFileServerMediaSubsession: public FileServerMediaSubsession
{
public:
  static MPEG2TstFileServerMediaSubsession* createNew(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource);
  virtual void OnDelete();
protected:
  MPEG2TstFileServerMediaSubsession(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource);
  // called only by createNew();
  virtual ~MPEG2TstFileServerMediaSubsession();
  virtual float duration() const;

private: // redefined virtual functions
  virtual FramedSource* createNewStreamSource(unsigned clientSessionId,unsigned& estBitrate);
  virtual RTPSink* createNewRTPSink(Groupsock* rtpGroupsock,unsigned char rtpPayloadTypeIfDynamic,FramedSource* inputSource);
	virtual void seekStreamSource(FramedSource* inputSource, float seekNPT);
  char m_fileName[MAX_PATH];
};

#endif
