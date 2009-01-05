#ifndef _TSMPEG_1OR2_FILE_SERVER_DEMUX_HH
#define _TSMPEG_1OR2_FILE_SERVER_DEMUX_HH

#ifndef _SERVER_MEDIA_SESSION_HH
#include "ServerMediaSession.hh"
#endif
#ifndef _MPEG_1OR2_DEMUXED_ELEMENTARY_STREAM_HH
#include "MPEG1or2DemuxedElementaryStream.hh"
#endif

class TsMPEG1or2FileServerDemux: public Medium {
public:
  static TsMPEG1or2FileServerDemux*
  createNew(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource);

  ServerMediaSubsession* newAudioServerMediaSubsession(); // MPEG-1 or 2 audio
  ServerMediaSubsession* newVideoServerMediaSubsession(Boolean iFramesOnly = False,
						       double vshPeriod = 5.0
		       /* how often (in seconds) to inject a Video_Sequence_Header,
			  if one doesn't already appear in the stream */);
  ServerMediaSubsession* newAC3AudioServerMediaSubsession(); // AC-3 audio (from VOB)

  unsigned fileSize() const { return fFileSize; }
  float fileDuration() const { return fFileDuration; }
  void UpdateDuration();
private:
  TsMPEG1or2FileServerDemux(UsageEnvironment& env, char const* fileName,
			  Boolean reuseFirstSource);
      // called only by createNew();
  virtual ~TsMPEG1or2FileServerDemux();

private:
  friend class TsMPEG1or2DemuxedServerMediaSubsession;
  MPEG1or2DemuxedElementaryStream* newElementaryStream(unsigned clientSessionId,
						       u_int8_t streamIdTag);

private:
  char const* fFileName;
  unsigned fFileSize;
  float fFileDuration;
  Boolean fReuseFirstSource;
  MPEG1or2Demux* fSession0Demux;
  MPEG1or2Demux* fLastCreatedDemux;
  u_int8_t fLastClientSessionId;
};

#endif
