
#ifndef _TSMPEG_1OR2_DEMUXED_SERVER_MEDIA_SUBSESSION_HH
#define _TSMPEG_1OR2_DEMUXED_SERVER_MEDIA_SUBSESSION_HH

#ifndef _ON_DEMAND_SERVER_MEDIA_SUBSESSION_HH
#include "OnDemandServerMediaSubsession.hh"
#endif
#ifndef _TSMPEG_1OR2_FILE_SERVER_DEMUX_HH
#include "TsMPEG1or2FileServerDemux.h"
#endif

class TsMPEG1or2DemuxedServerMediaSubsession: public OnDemandServerMediaSubsession{
public:
  static TsMPEG1or2DemuxedServerMediaSubsession*
  createNew(TsMPEG1or2FileServerDemux& demux, u_int8_t streamIdTag,
	    Boolean reuseFirstSource,
	    Boolean iFramesOnly = False, double vshPeriod = 5.0);
  // The last two parameters are relevant for video streams only

private:
  TsMPEG1or2DemuxedServerMediaSubsession(TsMPEG1or2FileServerDemux& demux,
				       u_int8_t streamIdTag, Boolean reuseFirstSource,
				       Boolean iFramesOnly, double vshPeriod);
      // called only by createNew();
  virtual ~TsMPEG1or2DemuxedServerMediaSubsession();

private: // redefined virtual functions
  virtual void seekStreamSource(FramedSource* inputSource, float seekNPT);
  virtual FramedSource* createNewStreamSource(unsigned clientSessionId,
					      unsigned& estBitrate);
  virtual RTPSink* createNewRTPSink(Groupsock* rtpGroupsock,
                                    unsigned char rtpPayloadTypeIfDynamic,
				    FramedSource* inputSource);
  virtual float duration() const;

private:
  TsMPEG1or2FileServerDemux& fOurDemux;
  u_int8_t fStreamIdTag;
  Boolean fIFramesOnly; // for video streams
  double fVSHPeriod; // for video streams
};

#endif
