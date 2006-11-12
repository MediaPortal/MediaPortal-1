#include <streams.h>
#include <winsock.h>
#include <shlobj.h>
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
const char* STREAM_DESCRIPTION = "Session streamed by \"MediaPortal Tv Server v1.0\"";
const char* FILE_NAME = "C:\\temp\\testApp\\live.ts.tsbuffer";
extern void Log(const char *fmt, ...) ;

UsageEnvironment* m_env;
RTSPServer*			m_rtspServer;

void StreamSetup(char* ipAdress);
void StreamRun();
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName); // fwd
void StreamAddTimeShiftFile(char* streamName, char* fileName,bool isProgramStream);
void StreamAddMpegFile(char* streamName, char* fileName);
void StreamRemove(char* streamName);

extern netAddressBits SendingInterfaceAddr ;
extern netAddressBits ReceivingInterfaceAddr ;
#if _DEBUG

int _tmain(int argc, _TCHAR* argv[])
{
  StreamSetup("192.168.1.58");
  StreamAddTimeShiftFile("test", "C:\\1.ts.tsbuffer",false);
  while (true)
  {
    StreamRun();
  }
	return 0;
}

#endif

//**************************************************************************************
void StreamSetup(char* ipAdress)
{
  TCHAR folder[MAX_PATH];
  TCHAR fileName[MAX_PATH];
  ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  sprintf(fileName,"%s\\MediaPortal TV Server\\log\\streaming server.Log",folder);
	::DeleteFile(fileName);


	Log("Stream server:Setup stream server for ip:%s",ipAdress);
	
	ReceivingInterfaceAddr=inet_addr(ipAdress );
	SendingInterfaceAddr=inet_addr(ipAdress );

  TaskScheduler* scheduler = BasicTaskScheduler::createNew();
  m_env = BasicUsageEnvironment::createNew(*scheduler);

  m_rtspServer = RTSPServer::createNew(*m_env);
  if (m_rtspServer == NULL) 
  {
    Log("Stream server:Failed to create RTSP server: %s",m_env->getResultMsg());
    exit(1);
  }
}

//**************************************************************************************
void StreamRun()
{
	m_env->taskScheduler().doEventLoop(); 
}

//**************************************************************************************
void StreamRemove( char* streamName)
{
	Log("Stream server:Stream server: remove stream %s", streamName);
	m_rtspServer->removeServerMediaSession(streamName);
}

//**************************************************************************************
void StreamAddTimeShiftFile(char* streamName, char* fileName,bool isProgramStream)
{
	try
	{
		//add a stream...
    if (isProgramStream)
    {
		  Log("Stream server: add timeshift  mpeg-2 program stream %s filename:%s", streamName,fileName);
		  ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);

      TsMPEG1or2FileServerDemux* demux= TsMPEG1or2FileServerDemux::createNew(*m_env, fileName, false);
      sms->addSubsession(demux->newVideoServerMediaSubsession(false));
      sms->addSubsession(demux->newAudioServerMediaSubsession());
      m_rtspServer->addServerMediaSession(sms);

  	  
		  announceStream(m_rtspServer, sms, streamName, fileName);
    }
    else
    { 
      Log("Stream server: add timeshift  mpeg-2 transport stream %s filename:%s", streamName,fileName);
      ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
      sms->addSubsession(TsMPEG2TransportFileServerMediaSubsession::createNew(*m_env, fileName, false));
      m_rtspServer->addServerMediaSession(sms);

		  announceStream(m_rtspServer, sms, streamName, fileName);
    }
	}
	catch(...)
	{
		Log("Stream server: unable to add stream %s filename:%s", streamName,fileName);
	}
}

//**************************************************************************************
void StreamAddMpegFile(char* streamName, char* fileName)
{
	try
	{
		Log("Stream server: add mpeg-2 stream %s filename:%s", streamName,fileName);
		//add a stream...
    ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
    MPEG1or2FileServerDemux* demux= MPEG1or2FileServerDemux::createNew(*m_env, fileName, false);
    sms->addSubsession(demux->newVideoServerMediaSubsession(false));
    sms->addSubsession(demux->newAudioServerMediaSubsession());
		m_rtspServer->addServerMediaSession(sms);
	  
		announceStream(m_rtspServer, sms, streamName, fileName);
	}
	catch(...)
	{
		Log("Stream server: unable to add stream %s filename:%s", streamName,fileName);
	}
}

//**************************************************************************************
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName) 
{
  char* url = rtspServer->rtspURL(sms);
	Log("Stream server: url for stream is %s", url);
}
