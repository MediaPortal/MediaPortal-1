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
  return (unsigned short)m_clientSessions.size();
}

MPRTSPServer::MPRTSPClientSession* MPRTSPServer::GetSessionByIndex(unsigned short index)
{
  if (index >= m_clientSessions.size())
  {
    return NULL;
  }
  unsigned short currentIndex = 0;
  map<u_int32_t, MPRTSPClientSession*>::iterator it = m_clientSessions.begin();
  while (it != m_clientSessions.end())
  {
    if (currentIndex == index)
    {
      return it->second;
    }
    it++;
    currentIndex++;
  }
  return NULL;
}

bool MPRTSPServer::RemoveSessionById(u_int32_t sessionId)
{
  map<u_int32_t, MPRTSPClientSession*>::iterator it = m_clientSessions.find(sessionId);
  if (it == m_clientSessions.end() || it->second == NULL)
  {
    return false;
  }
  delete it->second;
  return true;
}

MPRTSPServer::MPRTSPServer(UsageEnvironment& env,
                            int ourSocket,
                            Port ourPort,
                            UserAuthenticationDatabase* authDatabase,
                            unsigned reclamationTimeSeconds)
  : RTSPServer(env, ourSocket, ourPort, authDatabase, reclamationTimeSeconds)
{
}

MPRTSPServer::~MPRTSPServer()
{
  m_clientSessions.clear();
}

GenericMediaServer::ClientConnection* MPRTSPServer::createNewClientConnection(int clientSocket, struct sockaddr_in clientAddr)
{
  return new MPRTSPClientConnection(*this, clientSocket, clientAddr);
}

GenericMediaServer::ClientSession* MPRTSPServer::createNewClientSession(u_int32_t sessionId)
{
  MPRTSPClientSession* session = new MPRTSPClientSession(*this, sessionId);
  m_clientSessions[sessionId] = session;
  return session;
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

MPRTSPServer::MPRTSPClientSession::MPRTSPClientSession(MPRTSPServer& ourServer, u_int32_t sessionId)
  : RTSPClientSession(ourServer, sessionId)
{
  m_startDateTime = time(NULL);
  m_isPaused = false;
}

MPRTSPServer::MPRTSPClientSession::~MPRTSPClientSession()
{
  MPRTSPServer& server = static_cast<MPRTSPServer&>(fOurServer);
  server.m_clientSessions.erase(fOurSessionId);
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
}

void MPRTSPServer::MPRTSPClientSession::handleCmd_PAUSE(RTSPClientConnection* ourClientConnection,
                                                        ServerMediaSubsession* subsession)
{
  RTSPClientSession::handleCmd_PAUSE(ourClientConnection, subsession);
  m_isPaused = true;
}