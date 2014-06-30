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

#ifndef __RTSP_CURL_INSTANCE_DEFINED
#define __RTSP_CURL_INSTANCE_DEFINED

#include "MPUrlSourceSplitter_Protocol_Rtsp_Parameters.h"
#include "CurlInstance.h"
#include "RtspDownloadRequest.h"
#include "RtspDownloadResponse.h"
#include "BaseRtpPacketCollection.h"
#include "StreamReceiveData.h"

// connection parameters

// connection preference can be adjusted by configuration parameters:
// PARAMETER_NAME_RTSP_MULTICAST_PREFERENCE
// PARAMETER_NAME_RTSP_UDP_PREFERENCE
// PARAMETER_NAME_RTSP_SAME_CONNECTION_TCP_PREFERENCE

#define RTSP_DESCRIBE_CONTENT_TYPE                                    L"application/sdp"

#define RTSP_CURL_INSTANCE_FLAG_NONE                                  CURL_INSTANCE_FLAG_NONE

#define RTSP_CURL_INSTANCE_FLAG_REQUEST_COMMAND_FINISHED              (1 << (CURL_INSTANCE_FLAG_LAST + 0))
#define RTSP_CURL_INSTANCE_FLAG_IGNORE_RTP_PAYLOAD_TYPE               (1 << (CURL_INSTANCE_FLAG_LAST + 1))
#define RTSP_CURL_INSTANCE_FLAG_METHOD_GET_PARAMETER_SUPPORTED        (1 << (CURL_INSTANCE_FLAG_LAST + 2))

#define RTSP_CURL_INSTANCE_FLAG_LAST                                  (CURL_INSTANCE_FLAG_LAST + 3)

#define RTSP_CURL_INSTANCE_COMMAND_NONE                               0x00000000
#define RTSP_CURL_INSTANCE_COMMAND_OPTIONS                            0x00000001
#define RTSP_CURL_INSTANCE_COMMAND_DESCRIBE                           0x00000002
#define RTSP_CURL_INSTANCE_COMMAND_SETUP_RESPONSE_VALID               0x00000003
#define RTSP_CURL_INSTANCE_COMMAND_SETUP_RESPONSE_NOT_VALID           0x00000004
#define RTSP_CURL_INSTANCE_COMMAND_PLAY_RESPONSE_VALID                0x00000005
#define RTSP_CURL_INSTANCE_COMMAND_PLAY_RESPONSE_NOT_VALID            0x00000006
#define RTSP_CURL_INSTANCE_COMMAND_TEARDOWN                           0x00000007

class CRtspCurlInstance : public CCurlInstance
{
public:
  // initializes a new instance of CRtspCurlInstance class
  // @param logger : logger for logging purposes
  // @param mutex : mutex for locking access to receive data buffer
  // @param protocolName : the protocol name instantiating
  // @param instanceName : the name of CURL instance
  CRtspCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName);

  // destructor
  virtual ~CRtspCurlInstance(void);

  /* get methods */

  // gets download response
  // @return : download respose
  virtual CRtspDownloadResponse *GetRtspDownloadResponse(void);

  // gets same connection TCP transport preference
  // @return : same connection TCP transport preference
  virtual unsigned int GetSameConnectionTcpPreference(void);

  // gets multicast preference
  // @return : multicast preference
  virtual unsigned int GetMulticastPreference(void);

  // gets UDP preference
  // @return : UDP preference
  virtual unsigned int GetUdpPreference(void);

  // gets RTSP minimum client port
  // @return : RTSP minimum client port
  virtual unsigned int GetRtspClientPortMin(void);

  // gets RTSP maximum client port
  // @return : RTSP maximum client port
  virtual unsigned int GetRtspClientPortMax(void);

  /* set methods */

  // sets same connection TCP transport preference
  // @param preference : same connection TCP transport preference to set
  virtual void SetSameConnectionTcpPreference(unsigned int preference);

  // sets multicast preference
  // @param preference : multicast preference to set
  virtual void SetMulticastPreference(unsigned int preference);

  // sets UDP preference
  // @param preference : UDP preference to set
  virtual void SetUdpPreference(unsigned int preference);

  // sets RTSP minimum client port
  // @param clientPortMin : RTSP minimum client port to set
  virtual void SetRtspClientPortMin(unsigned int clientPortMin);

  // sets RTSP maximum client port
  // @param clientPortMax : RTSP maximum client port to set
  virtual void SetRtspClientPortMax(unsigned int clientPortMax);

  // sets ignore RTP payload type flag
  // specifies if RTP payload type have to be ignored while processing RTP packets for RTSP tracks
  // @param ignoreRtpPayloadType : ignore RTP payload type flag to set
  virtual void SetIgnoreRtpPayloadType(bool ignoreRtpPayloadType);

  /* other methods */

  // tests if ignore RTP payload type flag is set
  // @return : true if ignore RTP payload type flag is set, false otherwise
  virtual bool IsIgnoreRtpPayloadTypeFlag(void);

  // initializes CURL instance
  // @param downloadRequest : download request
  // @return : true if successful, false otherwise
  virtual HRESULT Initialize(CDownloadRequest *downloadRequest);

  // stops receiving data
  // @return : true if successful, false otherwise
  virtual HRESULT StopReceivingData(void);

protected:
  // holds RTSP download request
  // never created and never destroyed
  // initialized in constructor by deep cloning
  CRtspDownloadRequest *rtspDownloadRequest;

  // holds RTSP download response
  CRtspDownloadResponse *rtspDownloadResponse;

  // holds min and max port for transport connection parameter
  unsigned int clientPortMin;
  unsigned int clientPortMax;

  // holds request command
  // command is cleared when request is done
  unsigned int requestCommand;

  // holds last successful command
  unsigned int lastCommand;

  // holds preferences for each type of transmission
  unsigned int sameConnectionTcpPreference;
  unsigned int multicastPreference;
  unsigned int udpPreference;

  // holds last sequence number (increased with every request)
  unsigned int lastSequenceNumber;
  // holds session ID (if specified by any response)
  wchar_t *sessionId;

  /* methods */

  // gets new instance of download response
  // @return : new download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // process received base RTP packets
  // @param track : RTSP track to process packets
  // @param clientPort : the client port of received packets
  // @param packets : packets to process
  // @return : S_OK if processed, error code otherwise
  virtual HRESULT ProcessReceivedBaseRtpPackets(CRtspTrack *track, unsigned int clientPort, CBaseRtpPacketCollection *packets);

  // Implementations should look for a base URL in the following order:
  // 1.     The RTSP Content-Base field
  // 2.     The RTSP Content-Location field
  // 3.     The RTSP request URL 

  // gets base URL
  // @return : base URL or NULL if error
  virtual const wchar_t *GetBaseUrl(void);

  // virtual CurlWorker() method is called from static CurlWorker() method
  virtual unsigned int CurlWorker(void);

  HRESULT SendAndReceive(CRtspRequest *request, const wchar_t *rtspMethodName, const wchar_t *functionName);

  // creates dump box for dump file
  // @return : dump box or NULL if error
  virtual CDumpBox *CreateDumpBox(void);
};

#endif