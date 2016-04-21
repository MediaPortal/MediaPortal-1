#include "StdAfx.h"
#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include <shlobj.h>
#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MPTaskScheduler.h"
#include "TsMPEG2TransportFileServerMediaSubsession.h" 
#include "MPRTSPServer.h"
#include <sstream>
#include <iomanip>  // setfill(), setw()

static char logbuffer[2000]; 
static wchar_t logbufferw[2000];
void LogDebug(const wchar_t *fmt, ...)
{
	va_list ap;
	va_start(ap,fmt);

	va_start(ap,fmt);
	vswprintf_s(logbufferw, fmt, ap);
	va_end(ap); 

	wchar_t folder[MAX_PATH];
	wchar_t fileName[MAX_PATH];
	::SHGetSpecialFolderPathW(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);
	swprintf_s(fileName, L"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log", folder);

	FILE* fp = _wfopen(fileName,L"a+, ccs=UTF-8");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fwprintf(fp,L"[%04.4d-%02.2d-%02.2d %02.2d:%02.2d:%02.2d,%03.3d] [%x] - %s\n",
      systemTime.wYear, systemTime.wMonth, systemTime.wDay,
      systemTime.wHour, systemTime.wMinute, systemTime.wSecond, systemTime.wMilliseconds,
			GetCurrentThreadId(),
			logbufferw);
		fclose(fp);
	}
};

void LogDebug(const char *fmt, ...)
{
	va_list ap;
	va_start(ap,fmt);

	va_start(ap,fmt);
	vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	MultiByteToWideChar(CP_ACP, 0, logbuffer, -1, logbufferw, sizeof(logbuffer)/sizeof(wchar_t));
	LogDebug(L"%s", logbufferw);
};

const char* STREAM_NAME = "testStream";
const char* STREAM_DESCRIPTION = "Session streamed by \"MediaPortal Tv Server v1.2 Beta 1\"";
const char* FILE_NAME = "C:\\temp\\testApp\\live.ts.tsbuffer";
const int	DEFAULT_RTSP_PORT = 554;

UsageEnvironment* m_env;
MPRTSPServer*			m_rtspServer;

void StreamSetup(char* ipAdress);
int  StreamSetupEx(char* ipAdress, int port);
void StreamShutdown();
void StreamRun();
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms, char * streamName, wchar_t * inputFileName); // fwd
void StreamAddTimeShiftFile(char* streamName, wchar_t* fileName,bool isProgramStream,int channelType);
void StreamAddMpegFile(char* streamName, wchar_t* fileName, int channelType);
void StreamRemove(char* streamName);

extern netAddressBits SendingInterfaceAddr ;
extern netAddressBits ReceivingInterfaceAddr ;

#if _DEBUG

int main(int argc, char* argv[])
{
	StreamSetup("192.168.1.130");
	StreamAddTimeShiftFile("test1", L"C:\\1\\live5-0.ts.tsbuffer",false,0);
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
	*clients = m_rtspServer->GetSessionCount();
}

//**************************************************************************************
void StreamGetClientDetail(unsigned short index, char** ipAddress, char** streamId, int* isActive, long* connectionTickCount)
{
  static char szIpAddress[50];
  static char szStreamId[260];
  *ipAddress = NULL;
  *streamId = NULL;
  *isActive = 0;
  *connectionTickCount = 0;

  if (m_rtspServer == NULL)
  {
    return;
  }
  MPRTSPServer::MPRTSPClientSession* client = m_rtspServer->GetSessionByIndex(index);
  if (client == NULL)
  {
    return;
  }

  struct sockaddr_in clientAddress = client->ClientAddress();
  sprintf(szIpAddress, "%d.%d.%d.%d",
          clientAddress.sin_addr.S_un.S_un_b.s_b1,
          clientAddress.sin_addr.S_un.S_un_b.s_b2,
          clientAddress.sin_addr.S_un.S_un_b.s_b3,
          clientAddress.sin_addr.S_un.S_un_b.s_b4);
  *ipAddress = &szIpAddress[0];

  const char* sid = client->StreamId();
  if (sid != NULL)
  {
    strcpy(szStreamId, sid);
  }
  *streamId = &szStreamId[0];
  *isActive = client->IsPaused() ? 0 : 1;
  *connectionTickCount = client->StartDateTime();
}

//**************************************************************************************
void StreamSetup(char* ipAdress)
{
	if (StreamSetupEx(ipAdress, DEFAULT_RTSP_PORT) == 1)
	{
		LogDebug("Exiting process after error");
		exit(1);
	}
}
//**************************************************************************************
int StreamSetupEx(char* ipAdress, int port)
{
	wchar_t folder[MAX_PATH];
	wchar_t fileName[MAX_PATH];
	::SHGetSpecialFolderPathW(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
	swprintf(fileName, MAX_PATH, L"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log", folder);
	::DeleteFileW(fileName);

	LogDebug("-------------- v1.1.0 ---------------");
  StreamShutdown();
	if (port == DEFAULT_RTSP_PORT) {
		LogDebug("Stream server:Setup stream server for ip: %s", ipAdress);
	}
	else
	{
		LogDebug("Stream server:Setup stream server for ip: %s:%d", ipAdress, port);
	}

	ReceivingInterfaceAddr=inet_addr(ipAdress );
	SendingInterfaceAddr=inet_addr(ipAdress );

	TaskScheduler* scheduler = MPTaskScheduler::createNew();
	m_env = BasicUsageEnvironment::createNew(*scheduler);
	m_rtspServer = MPRTSPServer::createNew(*m_env, port);
	if (m_rtspServer == NULL) 
	{
		LogDebug("Stream server:Failed to create RTSP server: %s",m_env->getResultMsg());
		return 1;
	}
	return 0; // ok
}
//**************************************************************************************
void StreamShutdown()
{
  if (m_rtspServer != NULL)
  {
    LogDebug("Stream server:Shutting down RTSP server");
    MPRTSPServer *server = m_rtspServer;
    m_rtspServer = NULL;
    Medium::close(server);
  }

  if (m_env != NULL)
  {
    LogDebug("Stream server:Cleaning up environment");
    UsageEnvironment *env = m_env;
    m_env = NULL;
    TaskScheduler *scheduler = &env->taskScheduler();
    env->reclaim();
    delete scheduler;
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
	LogDebug("Stream server:Stream server: remove stream %s", streamName);
	m_rtspServer->removeServerMediaSession(streamName);
}

//**************************************************************************************
void StreamAddTimeShiftFile(char* streamName, wchar_t* fileName,bool isProgramStream,int channelType)
{
	try
	{
		LogDebug(L"Stream server: add timeshift  mpeg-2 transport stream %S filename:%s", streamName,fileName);
		ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
		sms->addSubsession(TsMPEG2TransportFileServerMediaSubsession::createNew(*m_env, fileName, false,true,channelType));
		m_rtspServer->addServerMediaSession(sms);

		announceStream(m_rtspServer, sms, streamName, fileName);
	}
	catch(...)
	{
		LogDebug(L"Stream server: unable to add stream %S filename:%s", streamName,fileName);
	}
}

//**************************************************************************************
void StreamAddMpegFile(char* streamName, wchar_t* fileName, int channelType)
{
	try
	{
		LogDebug(L"Stream server: add mpeg-2 ts stream %S filename:%s", streamName,fileName);
		ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
		sms->addSubsession(TsMPEG2TransportFileServerMediaSubsession::createNew(*m_env, fileName, false,false,channelType));
		m_rtspServer->addServerMediaSession(sms);
		announceStream(m_rtspServer, sms, streamName, fileName);
	}
	catch(...)
	{
		LogDebug(L"Stream server: unable to add stream %S filename:%s", streamName,fileName);
	}
}

//**************************************************************************************
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms, char * streamName, wchar_t * inputFileName) 
{
	char* url = rtspServer->rtspURL(sms);
	LogDebug("Stream server: url for stream is %s", url);
}
