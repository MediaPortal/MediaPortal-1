#include <streams.h>
#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "GroupsockHelper.hh"
#include "TsStreamFileSource.hh"
#include "MPEG2TransportFileServerMediaSubsession.hh"
#include "TsMPEG2TransportFileServerMediaSubsession.h" 
#include "TsMPEG1or2FileServerDemux.h" 
#include "MPEG1or2FileServerDemux.hh" 
#include "TsFileDuration.h"

const char* STREAM_NAME = "testStream";
const char* STREAM_DESCRIPTION = "Session streamed by \"Streamserver v1.0\"";
const char* FILE_NAME = "C:\\temp\\testApp\\live.ts.tsbuffer";
extern void Log(const char *fmt, ...) ;
UsageEnvironment* env;
RTSPServer* rtspServer;
void StreamSetup();
void StreamRun();
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName); // fwd
void StreamAddTimeShiftFile(char* streamName, char* fileName,bool isProgramStream);
void StreamAddMpegFile(char* streamName, char* fileName);
void StreamRemove(char* streamName);

#if _DEBUG

int _tmain(int argc, _TCHAR* argv[])
{
  StreamSetup();
  StreamAddTimeShiftFile("test", "C:\\media\\movies\\NASA abc HDTV 720p.ts",false);
  while (true)
  {
    StreamRun();
  }
	return 0;
}

#endif

void StreamSetup()
{
	Log("Setup stream server");
  TaskScheduler* scheduler = BasicTaskScheduler::createNew();
  env = BasicUsageEnvironment::createNew(*scheduler);

  rtspServer = RTSPServer::createNew(*env);
  if (rtspServer == NULL) 
  {
    Log("Failed to create RTSP server: %s",env->getResultMsg());
    exit(1);
  }
}
void StreamRun()
{
	env->taskScheduler().doEventLoop(); 
}

void StreamRemove( char* streamName)
{
	Log("Stream server: remove stream %s", streamName);
	rtspServer->removeServerMediaSession(streamName);
}

void StreamAddTimeShiftFile(char* streamName, char* fileName,bool isProgramStream)
{
	try
	{
		//add a stream...
    if (isProgramStream)
    {
		  Log("Stream server: add timeshift  mpeg-2 program stream %s filename:%s", streamName,fileName);
		  ServerMediaSession* sms= ServerMediaSession::createNew(*env, streamName, streamName,STREAM_DESCRIPTION,false);

      TsMPEG1or2FileServerDemux* demux= TsMPEG1or2FileServerDemux::createNew(*env, fileName, False);
      sms->addSubsession(demux->newVideoServerMediaSubsession(False));
      sms->addSubsession(demux->newAudioServerMediaSubsession());
      rtspServer->addServerMediaSession(sms);

  	  
		  announceStream(rtspServer, sms, streamName, fileName);
    }
    else
    { 
      Log("Stream server: add timeshift  mpeg-2 transport stream %s filename:%s", streamName,fileName);
      ServerMediaSession* sms= ServerMediaSession::createNew(*env, streamName, streamName,STREAM_DESCRIPTION,false);
      sms->addSubsession(TsMPEG2TransportFileServerMediaSubsession::createNew(*env, fileName, false));
      rtspServer->addServerMediaSession(sms);

		  announceStream(rtspServer, sms, streamName, fileName);
    }
	}
	catch(...)
	{
		Log("Stream server: unable to add stream %s filename:%s", streamName,fileName);
	}
}
void StreamAddMpegFile(char* streamName, char* fileName)
{
	try
	{
		Log("Stream server: add mpeg-2 stream %s filename:%s", streamName,fileName);
		//add a stream...
    ServerMediaSession* sms= ServerMediaSession::createNew(*env, streamName, streamName,STREAM_DESCRIPTION,false);
    MPEG1or2FileServerDemux* demux= MPEG1or2FileServerDemux::createNew(*env, fileName, False);
    sms->addSubsession(demux->newVideoServerMediaSubsession(False));
    sms->addSubsession(demux->newAudioServerMediaSubsession());
		rtspServer->addServerMediaSession(sms);
	  
		announceStream(rtspServer, sms, streamName, fileName);
	}
	catch(...)
	{
		Log("Stream server: unable to add stream %s filename:%s", streamName,fileName);
	}
}

void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName) 
{
  char* url = rtspServer->rtspURL(sms);
  UsageEnvironment& env = rtspServer->envir(); 
	Log("Stream server: url for stream is %s", url);
}
