/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __RTSP_TRACK_DEFINED
#define __RTSP_TRACK_DEFINED

#include "DownloadResponse.h"
#include "SimpleServer.h"
#include "RtspTransportResponseHeader.h"

#define PORT_UNSPECIFIED                                              UINT_MAX

class CRtspTrack
{
public:
  // initializes a new instance of CRtspTrack class
  CRtspTrack(void);
  virtual ~CRtspTrack(void);

  /* get methods */

  // gets server data port
  // @return : server data port or PORT_UNSPECIFIED if error
  virtual unsigned int GetServerDataPort(void);

  // gets server control port
  // @return : server control port or PORT_UNSPECIFIED if error
  virtual unsigned int GetServerControlPort(void);

  // gets client data port
  // @return : client data port or PORT_UNSPECIFIED if error
  virtual unsigned int GetClientDataPort(void);

  // gets client control port
  // @return : client control port or PORT_UNSPECIFIED if error
  virtual unsigned int GetClientControlPort(void);

  // gets download response associated with RTSP track
  // @return : download response or NULL if error
  virtual CDownloadResponse *GetDownloadResponse(void);

  // gets track URL
  // @return : track URL or NULL if error
  virtual const wchar_t *GetTrackUrl(void);

  // gets data server
  // @return : data server or NULL if not specified
  virtual CSimpleServer *GetDataServer(void);

  // gets control server
  // @return : control server or NULL if not specified
  virtual CSimpleServer *GetControlServer(void);

  // gets RTSP transport response header
  // @return : RTSP transport response header or NULL if not specified
  virtual CRtspTransportResponseHeader *GetTransportResponseHeader(void);

  /* set methods */

  // sets server data port
  // @param serverDataPort : server data port to set
  virtual void SetServerDataPort(unsigned int serverDataPort);

  // sets server control port
  // @param serverControlPort : server control port to set
  virtual void SetServerControlPort(unsigned int serverControlPort);

  // sets client data port
  // @param clientDataPort : client data port to set
  virtual void SetClientDataPort(unsigned int clientDataPort);

  // sets client control port
  // @param clientControlPort : client control port to set
  virtual void SetClientControlPort(unsigned int clientControlPort);

  // sets track URL
  // @param trackUrl : track URL to set
  // @return : true if successful, false otherwise
  virtual bool SetTrackUrl(const wchar_t *trackUrl);

  // sets data server to track
  // @param dataServer : data server to set
  // @return : true if successful, false otherwise
  virtual void SetDataServer(CSimpleServer *dataServer);

  // sets control server to track
  // @param controlServer : control server to set
  // @return : true if successful, false otherwise
  virtual void SetControlServer(CSimpleServer *controlServer);

  // sets RTSP transport response header
  // @param header : RTSP transport response header to set
  // @return : true if successful, false otherwise
  virtual bool SetTransportResponseHeader(CRtspTransportResponseHeader *header);

  /* other methods */

  // tests if specified port is server data port
  // @param port : the port to test
  // @return : true if tested port is server data port, false otherwise
  virtual bool IsServerDataPort(unsigned int port);

  // tests if specified port is client data port
  // @param port : the port to test
  // @return : true if tested port is client data port, false otherwise
  virtual bool IsClientDataPort(unsigned int port);

  // tests if specified port is server control port
  // @param port : the port to test
  // @return : true if tested port is server control port, false otherwise
  virtual bool IsServerControlPort(unsigned int port);

  // tests if specified port is client control port
  // @param port : the port to test
  // @return : true if tested port is client control port, false otherwise
  virtual bool IsClientControlPort(unsigned int port);

  // deeply clones current instance
  // curl handle is not cloned
  // @result : deep clone of current instance or NULL if error
  virtual CRtspTrack *Clone(void);

protected:

  unsigned int serverDataPort;
  unsigned int serverControlPort;

  unsigned int clientDataPort;
  unsigned int clientControlPort;

  // holds download response
  CDownloadResponse *downloadResponse;

  // holds track URL
  wchar_t *trackUrl;

  // holds data and control server (if necessary)
  CSimpleServer *dataServer;
  CSimpleServer *controlServer;

  // holds RTSP transport response header (mostly for interleaved channels)
  CRtspTransportResponseHeader *transportResponseHeader;
};

#endif