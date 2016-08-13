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
#pragma once
#ifndef _RTSP_SERVER_HH
#include "RTSPServer.hh"
#endif
#include <map>

using namespace std;


class MPRTSPServer : public RTSPServer
{
  public:
    static MPRTSPServer* createNew(UsageEnvironment& env,
                                    Port ourPort = 554,
                                    UserAuthenticationDatabase* authDatabase = NULL,
                                    unsigned reclamationTimeSeconds = 65);

    // The state of a TCP connection used by a RTSP client:
    class MPRTSPClientConnection : public RTSPClientConnection
    {
      friend class MPRTSPServer;
      public:
        struct sockaddr_in ClientAddress()
        {
          return fClientAddr;
        }

      protected:
        MPRTSPClientConnection(MPRTSPServer& ourServer, int clientSocket, struct sockaddr_in clientAddr);
        virtual ~MPRTSPClientConnection();
    };

    // The state of an individual client session (using one or more sequential TCP connections) handled by a RTSP server:
    class MPRTSPClientSession : public RTSPClientSession
    {
      friend class MPRTSPServer;
      public:
        u_int32_t SessionId() const
        {
          return fOurSessionId;
        }

        const char* StreamId() const
        {
          if (fOurServerMediaSession == NULL)
          {
            return NULL;
          }
          return fOurServerMediaSession->streamName();
        }

        struct sockaddr_in ClientAddress() const
        {
          return m_clientAddress;
        }

        time_t StartDateTime() const
        {
          return m_startDateTime;
        }

        bool IsPaused() const
        {
          return m_isPaused;
        }
        
      protected:
        MPRTSPClientSession(MPRTSPServer& ourServer, u_int32_t sessionId);
        virtual ~MPRTSPClientSession();

        virtual void handleCmd_PLAY(RTSPClientConnection* ourClientConnection,
                                    ServerMediaSubsession* subsession,
                                    const char* fullRequestStr);
        virtual void handleCmd_PAUSE(RTSPClientConnection* ourClientConnection,
                                      ServerMediaSubsession* subsession);

      private:
        struct sockaddr_in m_clientAddress;
        time_t m_startDateTime;
        bool m_isPaused;
    };

    unsigned short GetSessionCount();
    MPRTSPClientSession* GetSessionByIndex(unsigned short index);
    bool RemoveSessionById(u_int32_t sessionId);

  protected:
    // called only by createNew();
    MPRTSPServer(UsageEnvironment& env,
                  int ourSocket,
                  Port ourPort,
                  UserAuthenticationDatabase* authDatabase,
                  unsigned reclamationTimeSeconds);
    virtual ~MPRTSPServer();

    // If you subclass "RTSPClientConnection", then you must also redefine this virtual function in order
    // to create new objects of your subclass:
    virtual ClientConnection* createNewClientConnection(int clientSocket, struct sockaddr_in clientAddr);

    // If you subclass "RTSPClientSession", then you should also redefine this virtual function in order
    // to create new objects of your subclass:
    virtual ClientSession* createNewClientSession(u_int32_t sessionId);

  private:
    map<u_int32_t, MPRTSPClientSession*> m_clientSessions;
};