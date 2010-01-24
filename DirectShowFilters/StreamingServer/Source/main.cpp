#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include <shlobj.h>
#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MPTaskScheduler.h"
#include "GroupsockHelper.hh"
#include "TsStreamFileSource.h"
#include "TsMPEG2TransportFileServerMediaSubsession.h" 
#include "MPEG1or2FileServerDemux.hh" 
#include "MPRTSPServer.h"
//#include "RTSPOverHTTPServer.hh"

static char logbuffer[2000]; 

void LogDebug(const char *fmt, ...) 
{
	va_list ap;
	va_start(ap,fmt);

	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	TCHAR folder[MAX_PATH];
	TCHAR fileName[MAX_PATH];
	::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
	sprintf(fileName,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log",folder);

	FILE* fp = fopen(fileName,"a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,systemTime.wMilliseconds,
			logbuffer);
		fclose(fp);
	}
};

const char* STREAM_NAME = "testStream";
const char* STREAM_DESCRIPTION = "Session streamed by \"MediaPortal Tv Server v1.1 Beta 1\"";
const char* FILE_NAME = "C:\\temp\\testApp\\live.ts.tsbuffer";
const int	DEFAULT_RTSP_PORT = 554;

UsageEnvironment* m_env;
MPRTSPServer*			m_rtspServer;

void StreamSetup(char* ipAdress);
int  StreamSetupEx(char* ipAdress, int port);
void StreamRun();
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName); // fwd
void StreamAddTimeShiftFile(char* streamName, char* fileName,bool isProgramStream);
void StreamAddMpegFile(char* streamName, char* fileName);
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
void StreamGetClientDetail(unsigned int clientNr, char** ipAdres, char** streamName, int* isActive, long* ticks)
{
	static char szipAdres[50];
	static char szstreamName[150];
	*ipAdres=NULL;
	*streamName=NULL;
	*isActive=0;
	*ticks=0;
	vector<MPRTSPServer::MPRTSPClientSession*> clients=m_rtspServer->Clients();
	if (clientNr>=clients.size()) return;
	MPRTSPServer::MPRTSPClientSession* client = clients[clientNr];

	sprintf(szipAdres,"%d.%d.%d.%d", client->getClientAddr().sin_addr.S_un.S_un_b.s_b1,
		client->getClientAddr().sin_addr.S_un.S_un_b.s_b2,
		client->getClientAddr().sin_addr.S_un.S_un_b.s_b3,
		client->getClientAddr().sin_addr.S_un.S_un_b.s_b4);
	*isActive=client->IsSessionIsActive();
	strcpy(szstreamName,client->getOurServerMediaSession()->streamName());
	*streamName=&szstreamName[0];
	*ipAdres=&szipAdres[0];
	*ticks=client->getStartDateTime();
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
	TCHAR folder[MAX_PATH];
	TCHAR fileName[MAX_PATH];
	::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
	sprintf(fileName,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log",folder);
	::DeleteFile(fileName);

	LogDebug("-------------- v1.0.0.2---------------");
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
void StreamAddTimeShiftFile(char* streamName, char* fileName,bool isProgramStream)
{
	try
	{
		LogDebug("Stream server: add timeshift  mpeg-2 transport stream %s filename:%s", streamName,fileName);
		ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
		sms->addSubsession(TsMPEG2TransportFileServerMediaSubsession::createNew(*m_env, fileName, false,true));
		m_rtspServer->addServerMediaSession(sms);

		announceStream(m_rtspServer, sms, streamName, fileName);
	}
	catch(...)
	{
		LogDebug("Stream server: unable to add stream %s filename:%s", streamName,fileName);
	}
}

//**************************************************************************************
void StreamAddMpegFile(char* streamName, char* fileName)
{
	try
	{
		LogDebug("Stream server: add mpeg-2 ts stream %s filename:%s", streamName,fileName);
		ServerMediaSession* sms= ServerMediaSession::createNew(*m_env, streamName, streamName,STREAM_DESCRIPTION,false);
		sms->addSubsession(TsMPEG2TransportFileServerMediaSubsession::createNew(*m_env, fileName, false,false));
		m_rtspServer->addServerMediaSession(sms);
		announceStream(m_rtspServer, sms, streamName, fileName);
	}
	catch(...)
	{
		LogDebug("Stream server: unable to add stream %s filename:%s", streamName,fileName);
	}
}

//**************************************************************************************
void announceStream(RTSPServer* rtspServer, ServerMediaSession* sms,char * streamName, char * inputFileName) 
{
	char* url = rtspServer->rtspURL(sms);
	LogDebug("Stream server: url for stream is %s", url);
}
