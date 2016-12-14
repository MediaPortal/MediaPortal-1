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
#include <string>
#include <iomanip>  // setfill(), setw()

using namespace std;


//-----------------------------------------------------------------------------
// LOGGING
//-----------------------------------------------------------------------------
static CCritSec g_logLock;
static CCritSec g_logFilePathLock;
static wstring g_logFilePath;
static wstring g_logFileName;
static WORD g_currentDay = -1;
static wchar_t g_logBuffer[2000];

void LogDebug(const wchar_t* fmt, ...)
{
  CAutoLock lock(&g_logLock);
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  if (g_currentDay != systemTime.wDay)
  {
    CAutoLock lock(&g_logFilePathLock);
    wstringstream logFileName;
    logFileName << g_logFilePath << L"\\StreamingServer-" << systemTime.wYear <<
                    L"-" << setfill(L'0') << setw(2) << systemTime.wMonth <<
                    L"-" << setw(2) << systemTime.wDay << L".log";
    g_logFileName = logFileName.str();
    g_currentDay = systemTime.wDay;
  }

  FILE* file = _wfopen(g_logFileName.c_str(), L"a+, ccs=UTF-8");
  if (file != NULL)
  {
    va_list ap;
    va_start(ap, fmt);
    vswprintf(g_logBuffer, sizeof(g_logBuffer) / sizeof(g_logBuffer[0]), fmt, ap);
    va_end(ap);
    fwprintf(file, L"%04.4hd-%02.2hd-%02.2hd %02.2hd:%02.2hd:%02.2hd.%03.3hd %s\n",
              systemTime.wYear, systemTime.wMonth, systemTime.wDay,
              systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
              systemTime.wMilliseconds, g_logBuffer);
    fclose(file);
  }

  //::OutputDebugStringW(g_logBuffer);
  //::OutputDebugStringW(L"\n");
};

const char* STREAM_DESCRIPTION = "Session streamed by \"MediaPortal TV Server\"";

UsageEnvironment* m_env = NULL;
MPRTSPServer* m_rtspServer = NULL;

extern netAddressBits SendingInterfaceAddr;
extern netAddressBits ReceivingInterfaceAddr;


long ServerSetup(char* ipAddress, unsigned short port)
{
  if (m_rtspServer != NULL)
  {
    // Must shutdown before setting up again.
    LogDebug(L"stream server: server is already running");
    return 0;
  }

  wchar_t temp[MAX_PATH];
  ::SHGetSpecialFolderPathW(NULL, temp, CSIDL_COMMON_APPDATA, FALSE);
  g_logFilePath = temp;
  g_logFilePath += L"\\Team MediaPortal\\MediaPortal TV Server\\log";

  LogDebug(L"--------------- v1.0.6 ---------------");
  LogDebug(L"stream server: setup server, IP address = %S, port = %hu", ipAddress, port);

  ReceivingInterfaceAddr = inet_addr(ipAddress);
  SendingInterfaceAddr = inet_addr(ipAddress);

  TaskScheduler* scheduler = MPTaskScheduler::createNew();
  if (scheduler == NULL) 
  {
    LogDebug(L"stream server: failed to create task scheduler");
    return 1;
  }

  m_env = BasicUsageEnvironment::createNew(*scheduler);
  if (m_env == NULL) 
  {
    LogDebug(L"stream server: failed to create usage environment");
    delete scheduler;
    return 2;
  }

  m_rtspServer = MPRTSPServer::createNew(*m_env, port);
  if (m_rtspServer == NULL) 
  {
    LogDebug(L"stream server: failed to create RTSP server, error = %S", m_env->getResultMsg());
    m_env->reclaim();
    m_env = NULL;
    delete scheduler;
    return 3;
  }

  return 0;
}

void ServerShutdown()
{
  if (m_rtspServer != NULL)
  {
    LogDebug(L"stream server: shutting down RTSP server");
    MPRTSPServer* server = m_rtspServer;
    m_rtspServer = NULL;
    Medium::close(server);
  }

  if (m_env != NULL)
  {
    LogDebug(L"stream server: cleaning up environment");
    UsageEnvironment* env = m_env;
    m_env = NULL;
    TaskScheduler* scheduler = &env->taskScheduler();
    env->reclaim();
    delete scheduler;
  }
}

void StreamAdd(char* id, wchar_t* fileName, unsigned char channelType, bool isStaticFile)
{
  if (m_rtspServer == NULL || m_env == NULL)
  {
    LogDebug(L"stream server: failed to add stream, server is not running");
    return;
  }
  try
  {
    LogDebug(L"stream server: add stream, ID = %S, channel type = %hhu, is static file = %d, file name = %s", id, channelType, isStaticFile, fileName);
    ServerMediaSession* sms = ServerMediaSession::createNew(*m_env, id, id, STREAM_DESCRIPTION, false);
    sms->addSubsession(TsMPEG2TransportFileServerMediaSubsession::createNew(*m_env, fileName, false, !isStaticFile, channelType));
    m_rtspServer->addServerMediaSession(sms);
    LogDebug(L"  URL = %S", m_rtspServer->rtspURL(sms));
  }
  catch (...)
  {
    LogDebug(L"stream server: failed to add stream, ID = %S, channel type = %hhu, is static file = %d, file name = %s", id, channelType, isStaticFile, fileName);
  }
}

void StreamRun()
{
  if (m_env == NULL)
  {
    return;
  }
  for (unsigned char i = 0; i < 10; ++i)
  {
    m_env->taskScheduler().doEventLoop();
  }
}

void StreamRemove(char* id)
{
  if (m_rtspServer == NULL)
  {
    LogDebug(L"stream server: failed to remove stream, server is not running");
    return;
  }

  LogDebug(L"stream server: remove stream, ID = %S", id);
  m_rtspServer->deleteServerMediaSession(id);
}

unsigned short ClientGetCount()
{
  if (m_rtspServer != NULL)
  {
    return m_rtspServer->GetSessionCount();
  }
  return 0;
}

void ClientGetDetail(unsigned short index, unsigned long* sessionId, char** ipAddress, char** streamId, unsigned long long* connectionTickCount, bool* isActive)
{
  static char szIpAddress[50];
  static char szStreamId[260];
  *sessionId = 0;
  *ipAddress = NULL;
  *streamId = NULL;
  *connectionTickCount = 0;
  *isActive = false;

  if (m_rtspServer == NULL)
  {
    return;
  }
  MPRTSPServer::MPRTSPClientSession* client = m_rtspServer->GetSessionByIndex(index);
  if (client == NULL)
  {
    return;
  }

  *sessionId = client->SessionId();

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

  *connectionTickCount = client->StartDateTime();
  *isActive = !client->IsPaused();
}

void ClientRemove(unsigned long sessionId)
{
  if (m_rtspServer != NULL)
  {
    LogDebug(L"stream server: failed to remove client, server is not running");
    return;
  }
  if (m_rtspServer->RemoveSessionById(sessionId))
  {
    LogDebug(L"stream server: remove client, ID = %lu", sessionId);
  }
}

#if _DEBUG

int main(int argc, char* argv[])
{
  int result = ServerSetup("192.168.1.130", 554);
  if (result != 0)
  {
    return result;
  }

  StreamAdd("test1", L"C:\\1\\live5-0.ts.tsbuffer", 0, false);
  //StreamAdd("test2", L"C:\\media\\movies\\NED 1.mpg", true);
  //StreamAdd("test3", L"C:\\media\\movies\\PREMIERE 420070201-1146.ts", true);
  while (true)
  {
    StreamRun();
  }
  StreamRemove("test1");
  return 0;
}

#endif