#pragma once

#include "liveMedia.hh"
class CRTSPClient
{
public:
  CRTSPClient(void);
  virtual ~CRTSPClient(void);
  bool OpenStream(string url);

protected:
	UsageEnvironment* m_env;
	Medium* m_ourClient ;
	MediaSession* m_session ;
};
