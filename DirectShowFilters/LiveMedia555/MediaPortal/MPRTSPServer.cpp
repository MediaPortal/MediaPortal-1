/* 
*  Copyright (C) 2006-2009 Team MediaPortal
*  http://www.team-mediaportal.com
*
*  This Program is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2, or (at your option)
*  any later version.
*
*  This Program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with GNU Make; see the file COPYING.  If not, write to
*  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
*  http://www.gnu.org/copyleft/gpl.html
*
*/
#include "MPRTSPServer.h"
#include <ctime>    // time_t


extern void LogDebug(const wchar_t* fmt, ...);

MPRTSPServer* MPRTSPServer::createNew(UsageEnvironment& env,
                                      Port ourPort,
                                      UserAuthenticationDatabase* authDatabase,
                                      unsigned reclamationTimeSeconds)
{
  int ourSocket = setUpOurSocket(env, ourPort);
  if (ourSocket < 0)
  {
    return NULL;
  }

  return new MPRTSPServer(env, ourSocket, ourPort, authDatabase, reclamationTimeSeconds);
}

unsigned short MPRTSPServer::GetSessionCount()
{
  return (unsigned short)fClientSessions->numEntries();
}

MPRTSPServer::MPRTSPClientSession* MPRTSPServer::GetSessionByIndex(unsigned short index)
{
  unsigned short currentIndex = 0;
  HashTable::Iterator* it = HashTable::Iterator::create(*fClientSessions);
  while (true)
  {
    const char* key;
    MPRTSPClientSession* clientSession = (MPRTSPClientSession*)it->next(key);
    if (currentIndex == index || clientSession == NULL)
    {
      return clientSession;
    }
    currentIndex++;
  }
}

bool MPRTSPServer::RemoveSessionById(u_int32_t sessionId)
{
  HashTable::Iterator* it = HashTable::Iterator::create(*fClientSessions);
  while (true)
  {
    const char* key;
    MPRTSPClientSession* clientSession = (MPRTSPClientSession*)it->next(key);
    if (clientSession == NULL)
    {
      return false;
    }
    if (clientSession->SessionId() == sessionId)
    {
      delete clientSession;
      return true;
    }
  }
}

MPRTSPServer::MPRTSPServer(UsageEnvironment& env,
                            int ourSocket,
                            Port ourPort,
                            UserAuthenticationDatabase* authDatabase,
                            unsigned reclamationTimeSeconds)
  : RTSPServer(env, ourSocket, ourPort, authDatabase, reclamationTimeSeconds)
{
  m_reclamationTimeSeconds = reclamationTimeSeconds;
}

MPRTSPServer::~MPRTSPServer()
{
}

GenericMediaServer::ClientConnection* MPRTSPServer::createNewClientConnection(int clientSocket, struct sockaddr_in clientAddr)
{
  return new MPRTSPClientConnection(*this, clientSocket, clientAddr);
}

GenericMediaServer::ClientSession* MPRTSPServer::createNewClientSession(u_int32_t sessionId)
{
  return new MPRTSPClientSession(*this, sessionId, m_reclamationTimeSeconds);
}


////////// MPRTSPServer::MPRTSPClientConnection //////////

MPRTSPServer::MPRTSPClientConnection::MPRTSPClientConnection(MPRTSPServer& ourServer, int clientSocket, struct sockaddr_in clientAddr)
  : RTSPClientConnection(ourServer, clientSocket, clientAddr)
{
}

MPRTSPServer::MPRTSPClientConnection::~MPRTSPClientConnection()
{
}


////////// MPRTSPServer::MPRTSPClientSession //////////

MPRTSPServer::MPRTSPClientSession::MPRTSPClientSession(MPRTSPServer& ourServer, u_int32_t sessionId, unsigned reclamationTimeSeconds)
  : RTSPClientSession(ourServer, sessionId)
{
  m_startDateTime = time(NULL);
  m_isPaused = false;
  m_reclamationTimeSeconds = reclamationTimeSeconds;
}

MPRTSPServer::MPRTSPClientSession::~MPRTSPClientSession()
{
}

void MPRTSPServer::MPRTSPClientSession::handleCmd_PLAY(RTSPClientConnection* ourClientConnection,
                                                        ServerMediaSubsession* subsession,
                                                        const char* fullRequestStr)
{
  MPRTSPServer::MPRTSPClientConnection* mpConnection = (MPRTSPServer::MPRTSPClientConnection*)ourClientConnection;
  if (mpConnection != NULL)
  {
    m_clientAddress = mpConnection->ClientAddress();
  }
  RTSPClientSession::handleCmd_PLAY(ourClientConnection, subsession, fullRequestStr);
  m_isPaused = false;
}

void MPRTSPServer::MPRTSPClientSession::handleCmd_PAUSE(RTSPClientConnection* ourClientConnection,
                                                        ServerMediaSubsession* subsession)
{
  RTSPClientSession::handleCmd_PAUSE(ourClientConnection, subsession);
  m_isPaused = true;
}

void MPRTSPServer::MPRTSPClientSession::livenessTimeoutTaskMP(MPRTSPClientSession* clientSession)
{
  if (clientSession->IsPaused()) 
  {
    LogDebug(L"livenessTimeoutTask - paused, returning");
    return;
  }
  LogDebug(L"livenessTimeoutTask");
  RTSPServer::RTSPClientSession::livenessTimeoutTask(clientSession);
}

void MPRTSPServer::MPRTSPClientSession::noteLiveness()
{
  if (m_reclamationTimeSeconds > 0)
  {
    //LogDebug(L"noteLiveness::RescheduleDelayedTask");
    envir().taskScheduler().rescheduleDelayedTask(fLivenessCheckTask,
                                                  m_reclamationTimeSeconds * 1000000,
                                                  (TaskFunc*)livenessTimeoutTaskMP,
                                                  this);
  }
}