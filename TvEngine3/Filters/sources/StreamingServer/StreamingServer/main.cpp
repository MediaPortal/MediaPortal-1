#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "GroupsockHelper.hh"
#include "TsStreamFileSource.hh"
#include "TsFileSinkDemux.hh"

const char* STREAM_NAME = "testStream";
const char* STREAM_DESCRIPTION = "Session streamed by \"Streamserver v1.0\"";
const char* FILE_NAME = "C:\\temp\\testApp\\live.ts.tsbuffer";
extern void Log(const char *fmt, ...) ;
UsageEnvironment* env;
RTSPServer* rtspServer;
void StreamSetup();
void StreamRun();
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName); // fwd
void StreamAddTs(char* streamName, char* fileName);
void StreamAddMpg(char* streamName, char* fileName);
void StreamRemove(char* streamName);
/*
int main(int argc, char** argv) 
{
  printf("Starting...");

  Setup();
  AddStreamTs(rtspServer,STREAM_NAME, FILE_NAME);

  Run();

  return 0; // only to prevent compiler warning
}*/

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

void StreamAddTs(char* streamName, char* fileName)
{
	Log("Stream server: add stream %s filename:%s", streamName,fileName);
  //add a stream...
  UsageEnvironment& env = rtspServer->envir(); 
  ServerMediaSession* sms= ServerMediaSession::createNew(env, streamName,streamName,STREAM_DESCRIPTION,false);
  TsFileSinkDemux* demux= TsFileSinkDemux::createNew(env, fileName, false);
  sms->addSubsession(demux->newVideoServerMediaSubsession(false));
  sms->addSubsession(demux->newAudioServerMediaSubsession());
  rtspServer->addServerMediaSession(sms);
  
  announceStream(rtspServer, sms, streamName, fileName);
}
void StreamAddMpg(char* streamName, char* fileName)
{
	Log("Stream server: add stream %s filename:%s", streamName,fileName);
  //add a stream...
  UsageEnvironment& env = rtspServer->envir(); 
  ServerMediaSession* sms= ServerMediaSession::createNew(env, streamName,streamName,STREAM_DESCRIPTION,false);
  MPEG1or2FileServerDemux* demux= MPEG1or2FileServerDemux::createNew(env, fileName, false);
  sms->addSubsession(demux->newVideoServerMediaSubsession(false));
  sms->addSubsession(demux->newAudioServerMediaSubsession());
  rtspServer->addServerMediaSession(sms);
  
  announceStream(rtspServer, sms, streamName, fileName);
}

void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName) 
{
  char* url = rtspServer->rtspURL(sms);
  UsageEnvironment& env = rtspServer->envir(); 
	Log("Stream server: url for stream is %s", url);
}
