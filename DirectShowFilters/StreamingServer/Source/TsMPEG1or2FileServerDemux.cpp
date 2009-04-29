#include "TsMPEG1or2FileServerDemux.h"
#include "TsMPEG1or2DemuxedServerMediaSubsession.h"
#include "TsStreamFileSource.hh"

TsMPEG1or2FileServerDemux*
TsMPEG1or2FileServerDemux::createNew(UsageEnvironment& env, char const* fileName,
				   Boolean reuseFirstSource) {
  return new TsMPEG1or2FileServerDemux(env, fileName, reuseFirstSource);
}

static float TsMPEG1or2ProgramStreamFileDuration(UsageEnvironment& env,
					       char const* fileName,
					       unsigned& fileSize); // forward
TsMPEG1or2FileServerDemux
::TsMPEG1or2FileServerDemux(UsageEnvironment& env, char const* fileName,
			  Boolean reuseFirstSource)
  : Medium(env),
    fReuseFirstSource(reuseFirstSource),
    fSession0Demux(NULL), fLastCreatedDemux(NULL), fLastClientSessionId(~0) {
  fFileName = strDup(fileName);
  fFileDuration = 7200;//TsMPEG1or2ProgramStreamFileDuration(env, fileName, fFileSize);
  fFileSize=1024L*1024L*512L;
}

TsMPEG1or2FileServerDemux::~TsMPEG1or2FileServerDemux() {
  Medium::close(fSession0Demux);
  delete[] (char*)fFileName;
}


void TsMPEG1or2FileServerDemux::UpdateDuration()
{
  fFileDuration = TsMPEG1or2ProgramStreamFileDuration(envir(), fFileName, fFileSize);
}

ServerMediaSubsession*
TsMPEG1or2FileServerDemux::newAudioServerMediaSubsession() {
  return TsMPEG1or2DemuxedServerMediaSubsession::createNew(*this, 0xC0, fReuseFirstSource);
}

ServerMediaSubsession*
TsMPEG1or2FileServerDemux::newVideoServerMediaSubsession(Boolean iFramesOnly,
						       double vshPeriod) {
  return TsMPEG1or2DemuxedServerMediaSubsession::createNew(*this, 0xE0, fReuseFirstSource,
							 iFramesOnly, vshPeriod);
}

ServerMediaSubsession*
TsMPEG1or2FileServerDemux::newAC3AudioServerMediaSubsession() {
  return TsMPEG1or2DemuxedServerMediaSubsession::createNew(*this, 0xBD, fReuseFirstSource);
  // because, in a VOB file, the AC3 audio has stream id 0xBD
}

MPEG1or2DemuxedElementaryStream*
TsMPEG1or2FileServerDemux::newElementaryStream(unsigned clientSessionId,
					     u_int8_t streamIdTag) {
  MPEG1or2Demux* demuxToUse;
  if (clientSessionId == 0) {
    // 'Session 0' is treated especially, because its audio & video streams
    // are created and destroyed one-at-a-time, rather than both streams being
    // created, and then (later) both streams being destroyed (as is the case
    // for other ('real') session ids).  Because of this, a separate demux is
    // used for session 0, and its deletion is managed by us, rather than
    // happening automatically.
    if (fSession0Demux == NULL) {
      // Open our input file as a 'byte-stream file source':
      TsStreamFileSource* fileSource
	= TsStreamFileSource::createNew(envir(), fFileName);
      if (fileSource == NULL) return NULL;
      fSession0Demux = MPEG1or2Demux::createNew(envir(), fileSource, False/*note!*/);
    }
    demuxToUse = fSession0Demux;
  } else {
    // First, check whether this is a new client session.  If so, create a new
    // demux for it:
    if (clientSessionId != fLastClientSessionId) {
      // Open our input file as a 'byte-stream file source':
      TsStreamFileSource* fileSource
	= TsStreamFileSource::createNew(envir(), fFileName);
      if (fileSource == NULL) return NULL;

      fLastCreatedDemux = MPEG1or2Demux::createNew(envir(), fileSource, True);
      // Note: We tell the demux to delete itself when its last
      // elementary stream is deleted.
      fLastClientSessionId = clientSessionId;
      // Note: This code relies upon the fact that the creation of streams for
      // different client sessions do not overlap - so one "MPEG1or2Demux" is used
      // at a time.
    }
    demuxToUse = fLastCreatedDemux;
  }

  if (demuxToUse == NULL) return NULL; // shouldn't happen

  return demuxToUse->newElementaryStream(streamIdTag);
}


static Boolean getMPEG1or2TimeCode(FramedSource* dataSource,
				   MPEG1or2Demux& parentDemux,
				   Boolean returnFirstSeenCode,
				   float& timeCode); // forward

static float TsMPEG1or2ProgramStreamFileDuration(UsageEnvironment& env,
					       char const* fileName,
					       unsigned& fileSize) {
  FramedSource* dataSource = NULL;
  float duration = 0.0; // until we learn otherwise
  fileSize = 0; // ditto

  do {
    // Open the input file as a 'byte-stream file source':
    TsStreamFileSource* fileSource = TsStreamFileSource::createNew(env, fileName);
    if (fileSource == NULL) break;
    dataSource = fileSource;

    fileSize = (unsigned)(fileSource->fileSize());
    if (fileSize == 0) break;

    // Create a MPEG demultiplexor that reads from that source.
    MPEG1or2Demux* baseDemux = MPEG1or2Demux::createNew(env, dataSource, True);
    if (baseDemux == NULL) break;

    // Create, from this, a source that returns raw PES packets:
    dataSource = baseDemux->newRawPESStream();

    // Read the first time code from the file:
    float firstTimeCode;
    if (!getMPEG1or2TimeCode(dataSource, *baseDemux, True, firstTimeCode)) break;

    // Then, read the last time code from the file.
    // (Before doing this, flush the demux's input buffers, 
    //  and seek towards the end of the file, for efficiency.)
    baseDemux->flushInput();
    unsigned const startByteFromEnd = 100000;
    unsigned newFilePosition
      = fileSize < startByteFromEnd ? 0 : fileSize - startByteFromEnd;
    if (newFilePosition > 0) fileSource->seekToByteAbsolute(newFilePosition);

    float lastTimeCode;
    if (!getMPEG1or2TimeCode(dataSource, *baseDemux, False, lastTimeCode)) break;

    // Take the difference between these time codes as being the file duration:
    float timeCodeDiff = lastTimeCode - firstTimeCode;
    if (timeCodeDiff < 0) break;
    duration = timeCodeDiff;
  } while (0);

  Medium::close(dataSource);
  return duration;
}

class TsDummySink: public MediaSink {
public:
  TsDummySink(MPEG1or2Demux& demux, Boolean returnFirstSeenCode);
  virtual ~TsDummySink();

  char watchVariable;

private:
  // redefined virtual function:
  virtual Boolean continuePlaying();

private:
  static void afterGettingFrame(void* clientData, unsigned frameSize,
                                unsigned numTruncatedBytes,
                                struct timeval presentationTime,
                                unsigned durationInMicroseconds);
  void afterGettingFrame1();

private:
  MPEG1or2Demux& fOurDemux;
  Boolean fReturnFirstSeenCode;
  unsigned char fBuf[10000];
};

static void afterPlayingTsDummySink(TsDummySink* sink); // forward
static float computeSCRTimeCode(MPEG1or2Demux::SCR const& scr); // forward

static Boolean getMPEG1or2TimeCode(FramedSource* dataSource,
				   MPEG1or2Demux& parentDemux,
				   Boolean returnFirstSeenCode,
				   float& timeCode) {
  // Start reading through "dataSource", until we see a SCR time code:
  parentDemux.lastSeenSCR().isValid = False;
  UsageEnvironment& env = dataSource->envir(); // alias
  TsDummySink sink(parentDemux, returnFirstSeenCode);
  sink.startPlaying(*dataSource,
		    (MediaSink::afterPlayingFunc*)afterPlayingTsDummySink, &sink);
  env.taskScheduler().doEventLoop(&sink.watchVariable);
  
  timeCode = computeSCRTimeCode(parentDemux.lastSeenSCR());
  return parentDemux.lastSeenSCR().isValid;
}


////////// TsDummySink implementation //////////

TsDummySink::TsDummySink(MPEG1or2Demux& demux, Boolean returnFirstSeenCode)
  : MediaSink(demux.envir()),
    watchVariable(0), fOurDemux(demux), fReturnFirstSeenCode(returnFirstSeenCode) {
}

TsDummySink::~TsDummySink() {
}

Boolean TsDummySink::continuePlaying() {
  fSource->getNextFrame(fBuf, sizeof fBuf,
			afterGettingFrame, this,
			onSourceClosure, this);
  return True;
}

void TsDummySink::afterGettingFrame(void* clientData, unsigned /*frameSize*/,
				  unsigned /*numTruncatedBytes*/,
				  struct timeval /*presentationTime*/,
				  unsigned /*durationInMicroseconds*/) {
  TsDummySink* sink = (TsDummySink*)clientData;
  sink->afterGettingFrame1();
}

void TsDummySink::afterGettingFrame1() {
  if (fReturnFirstSeenCode && fOurDemux.lastSeenSCR().isValid) {
    // We were asked to return the first SCR that we saw, and we've seen one,
    // so we're done.  (Handle this as if the input source had closed.)
    onSourceClosure(this);
    return;
  }

  continuePlaying();
}

static void afterPlayingTsDummySink(TsDummySink* sink) {
  // Return from the "doEventLoop()" call: 
  sink->watchVariable = ~0;
}

static float computeSCRTimeCode(MPEG1or2Demux::SCR const& scr) {
  double result = scr.remainingBits/90000.0 + scr.extension/300.0;
  if (scr.highBit) {
    // Add (2^32)/90000 == (2^28)/5625
    double const highBitValue = (256*1024*1024)/5625.0;
    result += highBitValue;
  }

  return (float)result;
}
