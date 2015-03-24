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

#ifndef __RTSP_TRANSPORT_RESPONSE_HEADER_DEFINED
#define __RTSP_TRANSPORT_RESPONSE_HEADER_DEFINED

#include "RtspResponseHeader.h"

#define RTSP_TRANSPORT_RESPONSE_HEADER_TYPE                               L"Transport"

#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_NONE                          RTSP_RESPONSE_HEADER_FLAG_NONE

#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_UNICAST                       (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 0))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_MULTICAST                     (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 1))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_INTERLEAVED                   (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 2))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_TRANSPORT_PROTOCOL_RTP        (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 3))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_PROFILE_AVP                   (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 4))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LOWER_TRANSPORT_TCP           (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 5))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LOWER_TRANSPORT_UDP           (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 6))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_APPEND                        (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 7))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_TIME_TO_LIVE                  (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 8))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LAYERS                        (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 9))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_PORT                          (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 10))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_CLIENT_PORT                   (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 11))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_SERVER_PORT                   (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 12))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_SSRC                          (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 13))
#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_MODE                          (1 << (RTSP_RESPONSE_HEADER_FLAG_LAST + 14))

#define RTSP_TRANSPORT_RESPONSE_HEADER_FLAG_LAST                          (RTSP_RESPONSE_HEADER_FLAG_LAST + 15)

#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_UNICAST                  L"unicast"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MULTICAST                L"multicast"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_DESTINATION              L"destination"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_INTERLEAVED              L"interleaved"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_APPEND                   L"append"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_TIME_TO_LIVE             L"ttl"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_LAYERS                   L"layers"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_PORT                     L"port"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_CLIENT_PORT              L"client_port"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SERVER_PORT              L"server_port"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SSRC                     L"ssrc"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MODE                     L"mode"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SOURCE                   L"source"

#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_UNICAST_LENGTH           7
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MULTICAST_LENGTH         9
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_DESTINATION_LENGTH       11
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_INTERLEAVED_LENGTH       11
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_APPEND_LENGTH            6
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_TIME_TO_LIVE_LENGTH      3
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_LAYERS_LENGTH            6
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_PORT_LENGTH              4
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_CLIENT_PORT_LENGTH       11
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SERVER_PORT_LENGTH       11
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SSRC_LENGTH              4
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_MODE_LENGTH              4
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_SOURCE_LENGTH            6

#define RTSP_TRANSPORT_RESPONSE_HEADER_SEPARATOR                          L";"
#define RTSP_TRANSPORT_RESPONSE_HEADER_SEPARATOR_LENGTH                   1

#define RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR                 L"/"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_SEPARATOR_LENGTH          1

#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR          L"="
#define RTSP_TRANSPORT_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH   1

#define RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR                    L"-"
#define RTSP_TRANSPORT_RESPONSE_HEADER_RANGE_SEPARATOR_LENGTH             1

#define RTSP_TRANSPORT_RESPONSE_HEADER_PROTOCOL_RTP                       L"RTP"
#define RTSP_TRANSPORT_RESPONSE_HEADER_PROFILE_AVP                        L"AVP"
#define RTSP_TRANSPORT_RESPONSE_HEADER_LOWER_TRANSPORT_TCP                L"TCP"
#define RTSP_TRANSPORT_RESPONSE_HEADER_LOWER_TRANSPORT_UDP                L"UDP"

class CRtspTransportResponseHeader : public CRtspResponseHeader
{
public:
  CRtspTransportResponseHeader(HRESULT *result);
  virtual ~CRtspTransportResponseHeader(void);

  /* get methods */

  // gets transport protocol
  // @return : transport protoco or NULL if not specified
  virtual const wchar_t *GetTransportProtocol(void);

  // gets profile
  // @return : profile or NULL if not specified
  virtual const wchar_t *GetProfile(void);

  // gets lower transport
  // @return : lower transport or NULL if not specified
  virtual const wchar_t *GetLowerTransport(void);

  // gets destination
  // @return : destination or NULL if not specified
  virtual const wchar_t *GetDestination(void);

  // gets min interleaved channel
  // @return : min interleaved channel
  virtual unsigned int GetMinInterleavedChannel(void);

  // gets max interleaved channel
  // @return : max interleaved channel
  virtual unsigned int GetMaxInterleavedChannel(void);

  // gets multicast time-to-live
  // @return : multicast time-to-live
  virtual unsigned int GetTimeToLive(void);

  // gets number of multicast layers to be used for this media stream
  // @return : number of multicast layers to be used for this media stream
  virtual unsigned int GetLayers(void);

  // gets multicast session min port
  // @return : multicast session min port
  virtual unsigned int GetMinPort(void);

  // gets multicast session max port
  // @return : multicast session max port
  virtual unsigned int GetMaxPort(void);

  // gets min client port
  // @return : min client port
  virtual unsigned int GetMinClientPort(void);

  // gets max client port
  // @return : max client port
  virtual unsigned int GetMaxClientPort(void);

  // gets min server port
  // @return : min server port
  virtual unsigned int GetMinServerPort(void);

  // gets max server port
  // @return : max server port
  virtual unsigned int GetMaxServerPort(void);

  // gets mode
  // @return : mode or NULL if not specified
  virtual const wchar_t *GetMode(void);

  // gets RTP synchronization source identifier
  // @return : RTP synchronization source identifier
  virtual unsigned int GetSynchronizationSourceIdentifier(void);

  // gets source
  // @return : source or NULL if not specified
  virtual const wchar_t *GetSource(void);

  /* set methods */

  /* other methods */

  // tests if unicast is set
  // @return : true if unicast is set, false otherwise
  virtual bool IsUnicast(void);

  // tests if multicast is set
  // @return : true if multiast is set, false otherwise
  virtual bool IsMulticast(void);

  // tests if interleaved is set
  // @return : true if interleaved is set, false otherwise
  virtual bool IsInterleaved(void);

  // tests if transport protocol is RTP
  // @return : true if transport protocol is RTP, false otherwise
  virtual bool IsTransportProtocolRTP(void);

  // tests if profile is AVP
  // @return : true if profile is AVP, false otherwise
  virtual bool IsProfileAVP(void);

  // tests if lower transport is TCP
  // @return : true if lower transport is TCP, false otherwise
  virtual bool IsLowerTransportTCP(void);

  // tests if lower transport is UDP
  // @return : true if lower transport is UDP, false otherwise
  virtual bool IsLowerTransportUDP(void);

  // tests if append is set
  // @return : true if append is set, false otherwise
  virtual bool IsAppend(void);

  // tests if time-to-live is set
  // @return : true if time-to-live is set, false otherwise
  virtual bool IsTimeToLive(void);

  // tests if layers is set
  // @return : true if layers is set, false otherwise
  virtual bool IsLayers(void);

  // tests if port is set
  // @return : true if port is set, false otherwise
  virtual bool IsPort(void);

  // tests if client port is set
  // @return : true if client port is set, false otherwise
  virtual bool IsClientPort(void);

  // tests if server port is set
  // @return : true if server port is set, false otherwise
  virtual bool IsServerPort(void);

  // tests if synchronization source identifier is set
  // @return : true if synchronization source identifier is set, false otherwise
  virtual bool IsSynchronizationSourceIdentifier(void);

  // parses header and stores name and value to internal variables
  // @param header : header to parse
  // @param length : the length of header
  // @return : true if successful, false otherwise
  virtual bool Parse(const wchar_t *header, unsigned int length);

protected:

  // holds transport protocol (it should be RTP)
  wchar_t *transportProtocol;

  // holds profile (it should be AVP)
  wchar_t *profile;

  // holds lower transport (it should be TCP or UDP)
  wchar_t *lowerTransport;

  // holds destination
  wchar_t *destination;
  // holds source
  wchar_t *source;

  // holds min and max interleaved channel number (if specified)
  unsigned int minInterleaved;
  unsigned int maxInterleaved;

  // holds multicast time-to-live
  unsigned int timeToLive;

  // holds the number of multicast layers to be used for this media stream
  unsigned int layers;

  // holds pair for a multicast session, it is specified as a range, e.g., port=3456-3457
  unsigned int minPort;
  unsigned int maxPort;

  // holds pair on which the client has chosen to receive media data and control information
  // it is specified as a range, e.g., client_port=3456-3457.
  unsigned int minClientPort;
  unsigned int maxClientPort;

  // holds pair on which the server has chosen to receive media data and control information
  // it is specified as a range, e.g., server_port=3456-3457. 
  unsigned int minServerPort;
  unsigned int maxServerPort;

  // holds mode
  wchar_t *mode;

  // holds ssrc (synchronization source identifier)
  unsigned int synchronizationSourceIdentifier;

  // deeply clones current instance to cloned header
  // @param  clone : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clone);

  // returns new header object to be used in cloning
  // @return : header object or NULL if error
  virtual CHttpHeader *CreateHeader(void);
};

#endif