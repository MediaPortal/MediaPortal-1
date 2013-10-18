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

#ifndef __RTMP_CURL_INSTANCE_DEFINED
#define __RTMP_CURL_INSTANCE_DEFINED

#include "MPUrlSourceSplitter_Protocol_Rtmp_Parameters.h"
#include "CurlInstance.h"
#include "RtmpDownloadRequest.h"
#include "RtmpDownloadResponse.h"

#include <librtmp/rtmp.h>

// define tokens for librtmp

/* CONNECTION PARAMETERS */

#define RTMP_TOKEN_APP                                      L"app"
#define RTMP_TOKEN_TC_URL                                   L"tcUrl"
#define RTMP_TOKEN_PAGE_URL                                 L"pageUrl"
#define RTMP_TOKEN_SWF_URL                                  L"swfUrl"
#define RTMP_TOKEN_FLASHVER                                 L"flashVer"
#define RTMP_TOKEN_AUTH                                     L"auth"

// arbitrary data are passed as they are = they MUST be escaped
// so no token for abitrary data is needed

/* SESSION PARAMETERS */

#define RTMP_TOKEN_PLAY_PATH                                L"playpath"
#define RTMP_TOKEN_PLAYLIST                                 L"playlist"
#define RTMP_TOKEN_LIVE                                     L"live"
#define RTMP_TOKEN_SUBSCRIBE                                L"subscribe"
#define RTMP_TOKEN_START                                    L"start"
#define RTMP_TOKEN_STOP                                     L"stop"
#define RTMP_TOKEN_BUFFER                                   L"buffer"

 /* SECURITY PARAMETERS */

#define RTMP_TOKEN_TOKEN                                    L"token"
#define RTMP_TOKEN_JTV                                      L"jtv"
#define RTMP_TOKEN_SWF_VERIFY                               L"swfVfy"

// Special characters in values may need to be escaped to prevent misinterpretation by the option parser.
// The escape encoding uses a backslash followed by two hexadecimal digits representing the ASCII value of the character.
// E.g., spaces must be escaped as \20 and backslashes must be escaped as \5c.

#define RTMP_RESPONSE_DURATION                              L"duration "
#define RTMP_RESPONSE_DURATION_LENGTH                       9
#define RTMP_DURATION_UNSPECIFIED                           UINT64_MAX

class CRtmpCurlInstance :
  public CCurlInstance
{
public:
  // initializes a new instance of CRtmpCurlInstance class
  // @param logger : logger for logging purposes
  // @param mutex : mutex for locking access to receive data buffer
  // @param protocolName : the protocol name instantiating
  // @param instanceName : the name of CURL instance
  CRtmpCurlInstance(CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName);

  // destructor
  virtual ~CRtmpCurlInstance(void);

  // initializes CURL instance
  // @param downloadRequest : download request
  // @return : true if successful, false otherwise
  virtual bool Initialize(CDownloadRequest *downloadRequest);

  // gets download response
  // @return : download respose
  virtual CRtmpDownloadResponse *GetRtmpDownloadResponse(void);

  // gets duration in ms of RTMP stream
  // @return : duration in ms of RTMP stream or RTMP_DURATION_UNSPECIFIED if duration of stream unspecified
  uint64_t GetDuration(void);

protected:

  // holds duration in ms of RTMP steam
  // UINT64_MAX if not specified
  uint64_t duration;

  // holds RTMP download request
  // never created and never destroyed
  // initialized in constructor by deep cloning
  CRtmpDownloadRequest *rtmpDownloadRequest;

  // holds RTMP download response
  CRtmpDownloadResponse *rtmpDownloadResponse;

  // called when CURL debug message arives
  // @param type : CURL message type
  // @param data : received CURL message data
  virtual void CurlDebug(curl_infotype type, const wchar_t *data);

  // gets new instance of download response
  // @return : new download response or NULL if error
  virtual CDownloadResponse *GetNewDownloadResponse(void);

  // logging callback from librtmp
  static void RtmpLogCallback(struct RTMP *r, int level, const char *format, va_list vl);

  // encodes string to be used by librtmp
  // @return : encoded string (null terminated) or NULL if error
  wchar_t *EncodeString(const wchar_t *string);

  // creates librtmp parameter
  // @return : parameter or NULL if error
  wchar_t *CreateRtmpParameter(const wchar_t *name, const wchar_t *value);

  // creates librtmp parameter, first encoded value
  // @return : parameter or NULL if error
  wchar_t *CreateRtmpEncodedParameter(const wchar_t *name, const wchar_t *value);

  // adds to librtmp connection string new parameter with specified name and value
  // @param connectionString : librtmp connection string
  // @param name : the name of parameter
  // @param value : the value of parameter
  // @return : true if successful, false otherwise
  bool AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *name, unsigned int value);

  // adds to librtmp connection string new parameter with specified name and value
  // @param connectionString : librtmp connection string
  // @param name : the name of parameter
  // @param value : the value of parameter
  // @return : true if successful, false otherwise
  bool AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *name, int64_t value);

  // adds to librtmp connection string new parameter with specified name and value
  // @param connectionString : librtmp connection string
  // @param name : the name of parameter
  // @param value : the value of parameter
  // @param encode : specifies if value have to be first encoded
  // @return : true if successful, false otherwise
  bool AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *name, const wchar_t *value, bool encode);

  // adds to librtmp connection string new specified string
  // @param connectionString : librtmp connection string
  // @param string : the string to add to librtmp connection string
  // @return : true if successful, false otherwise
  bool AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *string);
};

#endif