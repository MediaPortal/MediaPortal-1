#pragma once


#ifndef _ON_DEMAND_SERVER_MEDIA_SUBSESSION_HH
#include "OnDemandServerMediaSubsession.hh"
#endif

class TsFileServerMediaSubsession: public OnDemandServerMediaSubsession {
protected: // we're a virtual base class
  TsFileServerMediaSubsession(UsageEnvironment& env, char const* fileName,
			    Boolean reuseFirstSource);
  virtual ~TsFileServerMediaSubsession();

protected:
  char const* fFileName;
  u_int64_t fFileSize; // if known
};
