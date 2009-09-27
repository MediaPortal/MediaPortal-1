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

#ifndef _MPRTSP_SERVER_H
#define _MPRTSP_SERVER_H

#include "RTSPServer.hh"
#include <vector>

using namespace std;

class MPRTSPServer: public RTSPServer {
public:
	static MPRTSPServer* createNew(UsageEnvironment& env, Port ourPort = 554,
		UserAuthenticationDatabase* authDatabase = NULL,
		unsigned reclamationTestSeconds = 45);

protected:
	MPRTSPServer(UsageEnvironment& env,
		int ourSocket, Port ourPort,
		UserAuthenticationDatabase* authDatabase,
		unsigned reclamationTestSeconds);
	// called only by createNew();
	virtual ~MPRTSPServer();
	// If you subclass "RTSPClientSession", then you should also redefine this virtual function in order
	// to create new objects of your subclass:
	virtual RTSPClientSession*
		createNewClientSession(unsigned sessionId, int clientSocket, struct sockaddr_in clientAddr);


public:
	// The state of each individual session handled by a RTSP server:
	class MPRTSPClientSession: public RTSPClientSession {
	public:
		MPRTSPClientSession(MPRTSPServer& ourServer, unsigned sessionId,
			int clientSocket, struct sockaddr_in clientAddr);
		virtual ~MPRTSPClientSession();
		ServerMediaSession* getOurServerMediaSession(){return fOurServerMediaSession;}
		Boolean IsSessionIsActive() {return fSessionIsActive;}
		struct sockaddr_in getClientAddr() {return fClientAddr;}
		LONG getStartDateTime() {return startDateTime;}
		bool isPaused() {return m_bPaused;}

	protected:
		LONG startDateTime;
		bool m_bPaused;
		MPRTSPServer& fOurMPServer;
		virtual void handleCmd_PLAY(ServerMediaSubsession* subsession,
			char const* cseq, char const* fullRequestStr);
		virtual void handleCmd_PAUSE(ServerMediaSubsession* subsession,
			char const* cseq);
		static void livenessTimeoutTaskMP(MPRTSPClientSession* clientSession);
		virtual void noteLiveness();

	};

public:
	void AddClient(MPRTSPServer::MPRTSPClientSession* client);
	void RemoveClient(MPRTSPClientSession* client);

	vector<MPRTSPServer::MPRTSPClientSession*> Clients();
	typedef vector<MPRTSPServer::MPRTSPClientSession*>::iterator itClients;

	vector<MPRTSPServer::MPRTSPClientSession*> m_clients;
private:
  unsigned fMPReclamationTestSeconds;

};

#endif
