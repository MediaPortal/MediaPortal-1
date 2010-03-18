/* 
*	Copyright (C) 2006-2009 Team MediaPortal
*	http://www.team-mediaportal.com
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
#include "RTSPCommon.hh"
#include <GroupsockHelper.hh>

#if defined(__WIN32__) || defined(_WIN32) || defined(_QNX4)
#else
#include <signal.h>
#define USE_SIGNALS 1
#endif
#include <time.h> // for "strftime()" and "gmtime()"

#define RTPINFO_INCLUDE_RTPTIME 1

////////// RTSPServer //////////

extern void LogDebug(const char *fmt, ...) ;
MPRTSPServer*
MPRTSPServer::createNew(UsageEnvironment& env, Port ourPort,
						UserAuthenticationDatabase* authDatabase,
						unsigned reclamationTestSeconds) {
							int ourSocket = -1;

							do {
								int ourSocket = setUpOurSocket(env, ourPort);
								if (ourSocket == -1) break;

								return new MPRTSPServer(env, ourSocket, ourPort, authDatabase,
									reclamationTestSeconds);
							} while (0);

							if (ourSocket != -1) ::closeSocket(ourSocket);
							return NULL;
}

MPRTSPServer::MPRTSPServer(UsageEnvironment& env,
						   int ourSocket, Port ourPort,
						   UserAuthenticationDatabase* authDatabase,
						   unsigned reclamationTestSeconds)
						   : RTSPServer(env,ourSocket,ourPort,authDatabase,reclamationTestSeconds){
							   fMPReclamationTestSeconds=reclamationTestSeconds;
}

MPRTSPServer::~MPRTSPServer() {
}

RTSPServer::RTSPClientSession*
MPRTSPServer::createNewClientSession(unsigned sessionId, int clientSocket, struct sockaddr_in clientAddr) {
	return new MPRTSPClientSession(*this, sessionId, clientSocket, clientAddr);
}


////////// MPRTSPServer::MPRTSPClientSession //////////

MPRTSPServer::MPRTSPClientSession
::MPRTSPClientSession(MPRTSPServer& ourServer, unsigned sessionId,
					  int clientSocket, struct sockaddr_in clientAddr)
					  : RTSPClientSession(ourServer,sessionId,clientSocket,clientAddr),
					  fOurMPServer(ourServer) {
						  startDateTime=time(NULL);
						  m_bPaused=false;
						  fOurMPServer.AddClient(this);
}

MPRTSPServer::MPRTSPClientSession::~MPRTSPClientSession() {
	fOurMPServer.RemoveClient(this);
}

void MPRTSPServer::MPRTSPClientSession
::handleCmd_PLAY(ServerMediaSubsession* subsession, char const* cseq,
				 char const* fullRequestStr) {
					 RTSPClientSession::handleCmd_PLAY(subsession,cseq,fullRequestStr);
					 m_bPaused=false;
}

void MPRTSPServer::MPRTSPClientSession
::handleCmd_PAUSE(ServerMediaSubsession* subsession, char const* cseq) {
	RTSPClientSession::handleCmd_PAUSE(subsession,cseq);
	m_bPaused=true;
}

vector<MPRTSPServer::MPRTSPClientSession*> MPRTSPServer::Clients()
{
	return m_clients;
}
void MPRTSPServer::AddClient(MPRTSPClientSession* client)
{
	m_clients.push_back(client);
}
void MPRTSPServer::RemoveClient(MPRTSPClientSession* client)
{
	itClients it;
	it=m_clients.begin();
	while (it!=m_clients.end())
	{
		if (*it==client)
		{
			m_clients.erase(it);
			return;
		}
		++it;
	}
}

void MPRTSPServer::MPRTSPClientSession
::livenessTimeoutTaskMP(MPRTSPClientSession* clientSession) {
	if (clientSession->m_bPaused) 
	{
		LogDebug("livenessTimeoutTask - Paused returning");
		return;
	}
	LogDebug("livenessTimeoutTask");
	RTSPServer::RTSPClientSession::livenessTimeoutTask(clientSession);
}

void MPRTSPServer::MPRTSPClientSession::noteLiveness() {
	if (fOurMPServer.fMPReclamationTestSeconds > 0) {
		//LogDebug("noteLiveness::RescheduleDelayedTask");
		envir().taskScheduler()
			.rescheduleDelayedTask(fLivenessCheckTask,
			fOurMPServer.fMPReclamationTestSeconds*1000000,
			(TaskFunc*)livenessTimeoutTaskMP, this);
	}
}
