#include <streams.h>
//#include <winsock2.h>
#include <shlobj.h>
#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "GroupsockHelper.hh"
#include "TsStreamFileSource.hh"
#include "MPEG2TransportFileServerMediaSubsession.hh"
#include "TsMPEG2TransportFileServerMediaSubsession.h" 
#include "MPEG2TstFileServerMediaSubsession.h" 
#include "TsMPEG1or2FileServerDemux.h" 
#include "MPEG1or2FileServerDemux.hh" 
#include "TsFileDuration.h"
//#include "RTSPOverHTTPServer.hh"

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
void StreamAdd3gpFile(char* streamName, char* fileName);
void StreamRemove(char* streamName);

extern netAddressBits SendingInterfaceAddr ;
extern netAddressBits ReceivingInterfaceAddr ;

#if _DEBUG

int main(int argc, char* argv[])
{
  StreamSetup("192.168.1.130");
  StreamAddTimeShiftFile("test1", "C:\\1\\live5-0.ts.tsbuffer",false);
  //StreamAddMpegFile("test2", "C:\\media\\movies\\NED 1.mpg");
  //StreamAddMpegFile("test3", "C:\\media\\movies\\PREMIERE 420070201-1146.ts");
  while (true)
  {
    StreamRun();
  }
  StreamRemove("test");
	return 0;
}

#endif


//**************************************************************************************
void StreamGetClientCount(int* clients)
{
  *clients=0;
  if (m_rtspServer==NULL) return ;
  *clients= m_rtspServer->Clients().size();
}

//**************************************************************************************
void StreamGetClientDetail(int clientNr, char** ipAdres, char** streamName, int* isActive, long* ticks)
{
  static char szipAdres[50];
  static char szstreamName[150];
  *ipAdres=NULL;
  *streamName=NULL;
  *isActive=0;
  *ticks=0;
  vector<RTSPServer::RTSPClientSession*> clients=m_rtspServer->Clients();
  if (clientNr>=clients.size()) return;
  RTSPServer::RTSPClientSession* client = clients[clientNr];

  sprintf(szipAdres,"%d.%d.%d.%d", client->fClientAddr.sin_addr.S_un.S_un_b.s_b1,
                                   client->fClientAddr.sin_addr.S_un.S_un_b.s_b2,
                                   client->fClientAddr.sin_addr.S_un.S_un_b.s_b3,
                                   client->fClientAddr.sin_addr.S_un.S_un_b.s_b4);
  *isActive=client->fSessionIsActive;
  strcpy(szstreamName,client->fOurServerMediaSession->fStreamName);
  *streamName=&szstreamName[0];
  *ipAdres=&szipAdres[0];
  *ticks=client->startDateTime;
}

//**************************************************************************************
void StreamSetup(char* ipAdress)
{
  TCHAR folder[MAX_PATH];
  TCHAR fileName[MAX_PATH];
  ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  sprintf(fileName,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log",folder);
  ::DeleteFile(fileName);

  Log("-------------- v1.0.0.1---------------");
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
  for (int i=0; i < 10;++i)
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
		if (strstr(fileName,".ts") !=NULL)
		{
			Log("Stream server: add mpeg-2 ts stream %s filename:%s", streamName,fileName);
			ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
			sms->addSubsession(MPEG2TransportFileServerMediaSubsession::createNew(*m_env, fileName, NULL,false));
			m_rtspServer->addServerMediaSession(sms);
			announceStream(m_rtspServer, sms, streamName, fileName);
		}
		else
		{
			Log("Stream server: add mpeg-2 ps stream %s filename:%s", streamName,fileName);
			ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
			sms->addSubsession(MPEG2TstFileServerMediaSubsession::createNew(*m_env, fileName, false));
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
void StreamAdd3gpFile(char* streamName, char* fileName)
{
	try
	{
		Log("Stream server: add mpeg-4 stream %s filename:%s", streamName,fileName);
    ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
//    sms->addSubsession(H263plusVideoFileServerMediaSubsession::createNew(*m_env, fileName, false));
//    m_rtspServer->addServerMediaSession(sms);

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
