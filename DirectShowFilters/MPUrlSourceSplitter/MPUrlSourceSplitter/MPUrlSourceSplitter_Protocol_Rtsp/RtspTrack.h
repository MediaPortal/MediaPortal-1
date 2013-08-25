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
#include "IpAddress.h"

#define PORT_UNSPECIFIED                                              UINT_MAX
// receiver report minimum time is 5000 ms
#define RECEIVER_REPORT_MIN_TIME                                      5000

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

  // gets last receiver report time
  // @return : last receiver report time
  virtual DWORD GetLastReceiverReportTime(void);

  // gets receiver report interval
  // @return : receiver report interval
  virtual DWORD GetReceiverReportInterval(void);

  // gets track synchronization source identifier
  // SSRC for track is generated when created class
  // @return : synchronization source identifier
  virtual unsigned int GetSynchronizationSourceIdentifier(void);

  // gets sender synchronization source identifier
  // SSRC for track is generated when created class
  // @return : sender synchronization source identifier
  virtual unsigned int GetSenderSynchronizationSourceIdentifier(void);

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

  // sets last receiver report time
  // @param lastReceiverReportTime : last receiver report time to set
  virtual void SetLastReceiverReportTime(DWORD lastReceiverReportTime);

  // sets receiver report interval
  // @param receiverReportInterval : receiver report interval to set
  virtual void SetReceiverReportInterval(DWORD receiverReportInterval);

  // sets synchronization source identifier
  // @param synchronizationSourceIdentifier : synchronization source identifier to set
  virtual void SetSynchronizationSourceIdentifier(unsigned int synchronizationSourceIdentifier);

  // sets sender synchronization source identifier
  // @param senderSynchronizationSourceIdentifier : sender synchronization source identifier to set
  virtual void SetSenderSynchronizationSourceIdentifier(unsigned int senderSynchronizationSourceIdentifier);

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

  // tests if sender synchronization source identifier is set or not
  // @return : true if SSRC is set, false otherwise
  virtual bool IsSetSenderSynchronizationSourceIdentifier(void);

  // deeply clones current instance
  // curl handle is not cloned
  // @result : deep clone of current instance or NULL if error
  virtual CRtspTrack *Clone(void);

protected:

  // holds remote server data and control ports
  unsigned int serverDataPort;
  unsigned int serverControlPort;

  // holds our data and server ports
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

  // holds last receiver report time
  DWORD lastReceiverReportTime;
  // holds receiver report interval
  DWORD receiverReportInterval;

  // holds track SSRC
  unsigned int synchronizationSourceIdentifier;
  // holds sender SSRC
  unsigned int senderSynchronizationSourceIdentifier;
  bool senderSynchronizationSourceIdentifierSet;
};

#endif