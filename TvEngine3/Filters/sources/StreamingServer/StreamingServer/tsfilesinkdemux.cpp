/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 2.1 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2005 Live Networks, Inc.  All rights reserved.
// A server demultiplexer for a MPEG 1 or 2 Program Stream
// Implementation

#include "TsFileSinkDemux.hh"
#include "TsFileSourceDemuxedServerMediaSubsession.hh"
extern void Log(const char *fmt, ...) ;
TsFileSinkDemux* TsFileSinkDemux::createNew(UsageEnvironment& env, char const* fileName,Boolean reuseFirstSource) 
{
  return new TsFileSinkDemux(env, fileName, reuseFirstSource);
}

static float MPEG1or2ProgramStreamFileDuration(UsageEnvironment& env,char const* fileName,__int64& fileSize); // forward

TsFileSinkDemux::TsFileSinkDemux(UsageEnvironment& env, char const* fileName,Boolean reuseFirstSource)
  : Medium(env),
    fReuseFirstSource(reuseFirstSource),
    fSession0Demux(NULL), fLastCreatedDemux(NULL), fLastClientSessionId(~0) 
{
  _lastTicks=0;
	m_env=&env;
  fFileName = strDup(fileName);
  fFileDuration = MPEG1or2ProgramStreamFileDuration(env, fileName, fFileSize);
  _updatingDuration=false;
}

TsFileSinkDemux::~TsFileSinkDemux() 
{
  Medium::close(fSession0Demux);
  delete[] (char*)fFileName;
}

__int64 TsFileSinkDemux::fileSize() const 
{ 
  return 2LL*1024LL*1000LL*100LL;
  /*
  __int64 fileSize=m_pFileSource->fileSize();
  char sz[128];
  sprintf(sz,"TsFileSinkDemux::fileSize:%d MB", (int) ( (fileSize/1024LL)/1000LL) );
  Log(sz);
	return fileSize; */
}
float TsFileSinkDemux::fileDuration()
{ 
  return 3600;
  /*
  if (_updatingDuration) 
	{
		Log("TsFileSinkDemux::busyfileDuration:%f", fFileDuration);
		return fFileDuration;
	}
  _updatingDuration=true;

	__int64 fileSize;
  char sz[128];
  DWORD ticks=GetTickCount();
  if (ticks-_lastTicks >=5000)
  {
	  float duration= MPEG1or2ProgramStreamFileDuration(*m_env, fFileName, fileSize);
    
    if (duration>0)
    {
      fFileDuration=duration;
      _lastTicks=GetTickCount();
		Log("TsFileSinkDemux::newfileDuration:%f", fFileDuration);
    }
  }
	else
	{
		Log("TsFileSinkDemux::cachefileDuration:%f", fFileDuration);
	}
  _updatingDuration=false;
  return fFileDuration;*/
}

ServerMediaSubsession* TsFileSinkDemux::newAudioServerMediaSubsession() 
{
  return TsFileSourceDemuxedServerMediaSubsession::createNew(*this, 0xC0, fReuseFirstSource);
}

ServerMediaSubsession* TsFileSinkDemux::newVideoServerMediaSubsession(Boolean iFramesOnly,double vshPeriod) 
{
  return TsFileSourceDemuxedServerMediaSubsession::createNew(*this, 0xE0, fReuseFirstSource,iFramesOnly, vshPeriod);
}

ServerMediaSubsession* TsFileSinkDemux::newAC3AudioServerMediaSubsession() 
{
  return TsFileSourceDemuxedServerMediaSubsession::createNew(*this, 0xBD, fReuseFirstSource);
  // because, in a VOB file, the AC3 audio has stream id 0xBD
}

MPEG1or2DemuxedElementaryStream* TsFileSinkDemux::newElementaryStream(unsigned clientSessionId,u_int8_t streamIdTag) 
{
  MPEG1or2Demux* demuxToUse;
  if (clientSessionId == 0) 
  {
    // 'Session 0' is treated especially, because its audio & video streams
    // are created and destroyed one-at-a-time, rather than both streams being
    // created, and then (later) both streams being destroyed (as is the case
    // for other ('real') session ids).  Because of this, a separate demux is
    // used for session 0, and its deletion is managed by us, rather than
    // happening automatically.
    if (fSession0Demux == NULL) 
    {
      // Open our input file as a 'byte-stream file source':
      TsStreamFileSource* fileSource= TsStreamFileSource::createNew(envir(), fFileName);
      if (fileSource == NULL) return NULL;
      //m_pFileSource=fileSource;
      fSession0Demux = MPEG1or2Demux::createNew(envir(), fileSource, False/*note!*/);
    }
    demuxToUse = fSession0Demux;
  } 
  else 
  {
    // First, check whether this is a new client session.  If so, create a new
    // demux for it:
    if (clientSessionId != fLastClientSessionId) 
    {
      // Open our input file as a 'byte-stream file source':
      TsStreamFileSource* fileSource= TsStreamFileSource::createNew(envir(), fFileName);
      if (fileSource == NULL) return NULL;
//      m_pFileSource=fileSource;
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


static Boolean getMPEG1or2TimeCode(FramedSource* dataSource,MPEG1or2Demux& parentDemux,Boolean returnFirstSeenCode,float& timeCode); // forward

static float MPEG1or2ProgramStreamFileDuration(UsageEnvironment& env,char const* fileName,__int64& fileSize) 
{
  FramedSource* dataSource = NULL;
  float duration = 0.0; // until we learn otherwise
  fileSize = 0; // ditto

  do 
  {
    // Open the input file as a 'byte-stream file source':
    TsStreamFileSource* fileSource = TsStreamFileSource::createNew(env, fileName);
    if (fileSource == NULL) break;
    dataSource = fileSource;

    fileSize = (__int64)(fileSource->fileSize());
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
    __int64 const startByteFromEnd = 100000;
    __int64 newFilePosition= fileSize < startByteFromEnd ? 0 : fileSize - startByteFromEnd;
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

class TsDummySink: public MediaSink 
{
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
static float TscomputeSCRTimeCode(MPEG1or2Demux::SCR const& scr); // forward

static Boolean getMPEG1or2TimeCode(FramedSource* dataSource,
				   MPEG1or2Demux& parentDemux,
				   Boolean returnFirstSeenCode,
				   float& timeCode) 
{
  // Start reading through "dataSource", until we see a SCR time code:
  parentDemux.lastSeenSCR().isValid = False;
  UsageEnvironment& env = dataSource->envir(); // alias
  TsDummySink sink(parentDemux, returnFirstSeenCode);
  sink.startPlaying(*dataSource,
		    (MediaSink::afterPlayingFunc*)afterPlayingTsDummySink, &sink);
  env.taskScheduler().doEventLoop(&sink.watchVariable);
  
  timeCode = TscomputeSCRTimeCode(parentDemux.lastSeenSCR());
  return parentDemux.lastSeenSCR().isValid;
}


////////// TsDummySink implementation //////////

TsDummySink::TsDummySink(MPEG1or2Demux& demux, Boolean returnFirstSeenCode)
  : MediaSink(demux.envir()),
    watchVariable(0), fOurDemux(demux), fReturnFirstSeenCode(returnFirstSeenCode) 
{
}

TsDummySink::~TsDummySink() {
}

Boolean TsDummySink::continuePlaying() 
{
  fSource->getNextFrame(fBuf, sizeof fBuf,
			afterGettingFrame, this,
			onSourceClosure, this);
  return True;
}

void TsDummySink::afterGettingFrame(void* clientData, unsigned /*frameSize*/,
				  unsigned /*numTruncatedBytes*/,
				  struct timeval /*presentationTime*/,
				  unsigned /*durationInMicroseconds*/) 
{
  TsDummySink* sink = (TsDummySink*)clientData;
  sink->afterGettingFrame1();
}

void TsDummySink::afterGettingFrame1() 
{
  if (fReturnFirstSeenCode && fOurDemux.lastSeenSCR().isValid) 
	{
    // We were asked to return the first SCR that we saw, and we've seen one,
    // so we're done.  (Handle this as if the input source had closed.)
    onSourceClosure(this);
    return;
  }

  continuePlaying();
}

static void afterPlayingTsDummySink(TsDummySink* sink) 
{
  // Return from the "doEventLoop()" call: 
  sink->watchVariable = ~0;
}

static float TscomputeSCRTimeCode(MPEG1or2Demux::SCR const& scr) 
{
  double result = scr.remainingBits/90000.0 + scr.extension/300.0;
  if (scr.highBit) 
	{
    // Add (2^32)/90000 == (2^28)/5625
    double const highBitValue = (256*1024*1024)/5625.0;
    result += highBitValue;
  }

  return (float)result;
}
