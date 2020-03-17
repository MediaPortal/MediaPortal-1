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

#ifndef __UDP_CURL_INSTANCE_DEFINED
#define __UDP_CURL_INSTANCE_DEFINED

#include "MPUrlSourceSplitter_Protocol_Udp_Parameters.h"
#include "CurlInstance.h"
#include "UdpDownloadRequest.h"
#include "UdpDownloadResponse.h"

#define PORT_UNSPECIFIED                                              UINT_MAX

#define UDP_CURL_INSTANCE_FLAG_NONE                                   CURL_INSTANCE_FLAG_NONE

#define UDP_CURL_INSTANCE_FLAG_TRANSPORT_UDP                          (1 << (CURL_INSTANCE_FLAG_LAST + 0))
#define UDP_CURL_INSTANCE_FLAG_TRANSPORT_RTP                          (1 << (CURL_INSTANCE_FLAG_LAST + 1))

#define UDP_CURL_INSTANCE_FLAG_LAST                                   (CURL_INSTANCE_FLAG_LAST + 2)

class CUdpCurlInstance : public CCurlInstance
{
public:
  // initializes a new instance of CUdpCurlInstance class
  // @param logger : logger for logging purposes
  // @param mutex : mutex for locking access to receive data buffer
  // @param protocolName : the protocol name instantiating
  // @param instanceName : the name of CURL instance
  CUdpCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName);

  // destructor
  virtual ~CUdpCurlInstance(void);

  /* get methods */

  // gets download response
  // @return : download respose
  virtual CUdpDownloadResponse *GetUdpDownloadResponse(void);

  /* set methods */

  /* other methods */

  // initializes CURL instance
  // @param downloadRequest : download request
  // @return : true if successful, false otherwise
  virtual HRESULT Initialize(CDownloadRequest *downloadRequest);

protected:
  wchar_t *localAddress;
  wchar_t *sourceAddress;

  unsigned int localPort;
  unsigned int sourcePort;

  // holds UDP download request
  // never created and never destroyed
  // initialized in constructor by deep cloning
  CUdpDownloadRequest *udpDownloadRequest;

  // holds UDP download response
  CUdpDownloadResponse *udpDownloadResponse;

  /* methods */

  // gets new instance of download response
  // @return : new download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // virtual CurlWorker() method is called from static CurlWorker() method
  virtual unsigned int CurlWorker(void);

  // creates dump box for dump file
  // @return : dump box or NULL if error
  virtual CDumpBox *CreateDumpBox(void);
};

#endif