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

#ifndef __RTSP_RESPONSE_DEFINED
#define __RTSP_RESPONSE_DEFINED

#include "RtspResponseHeaderCollection.h"
#include "RtspSequenceResponseHeader.h"

#define RTSP_STATUS_CODE_CONTINUE                                     100

#define RTSP_STATUS_CODE_OK                                           200
#define RTSP_STATUS_CODE_CREATED                                      201
#define RTSP_STATUS_CODE_LOW_ON_STORAGE_SPACE                         250

#define RTSP_STATUS_CODE_MULTIPLE_CHOICES                             300
#define RTSP_STATUS_CODE_MOVED_PERMANENTLY                            301
#define RTSP_STATUS_CODE_MOVED_TEMPORARILY                            302
#define RTSP_STATUS_CODE_SEE_OTHER                                    303
#define RTSP_STATUS_CODE_NOT_MODIFIED                                 304
#define RTSP_STATUS_CODE_USE_PROXY                                    305

#define RTSP_STATUS_CODE_BAD_REQUEST                                  400
#define RTSP_STATUS_CODE_UNAUTHORIZED                                 401
#define RTSP_STATUS_CODE_PAYMENT_REQUIRED                             402
#define RTSP_STATUS_CODE_FORBIDDEN                                    403
#define RTSP_STATUS_CODE_NOT_FOUND                                    404
#define RTSP_STATUS_CODE_METHOD_NOT_ALLOWED                           405
#define RTSP_STATUS_CODE_NOT_ACCEPTABLE                               406
#define RTSP_STATUS_CODE_PROXY_AUTHENTICATION_REQUIRED                407
#define RTSP_STATUS_CODE_REQUEST_TIME_OUT                             408
#define RTSP_STATUS_CODE_GONE                                         410
#define RTSP_STATUS_CODE_LENGTH_REQUIRED                              411
#define RTSP_STATUS_CODE_PRECONDITION_FAILED                          412
#define RTSP_STATUS_CODE_REQUEST_ENTITY_TOO_LARGE                     413
#define RTSP_STATUS_CODE_REQUEST_URI_TOO_LARGE                        414
#define RTSP_STATUS_CODE_UNSUPPORTED_MEDIA_TYPE                       415
#define RTSP_STATUS_CODE_PARAMETER_NOT_UNDERSTOOD                     451
#define RTSP_STATUS_CODE_CONFERENCE_NOT_FOUND                         452
#define RTSP_STATUS_CODE_NOT_ENOUGH_BANDWIDTH                         453
#define RTSP_STATUS_CODE_SESSION_NOT_FOUND                            454
#define RTSP_STATUS_CODE_METHOD_NOT_VALID_IN_THIS_STATE               455
#define RTSP_STATUS_CODE_HEADER_FIELD_NOT_VALID_FOR_RESOURCE          456
#define RTSP_STATUS_CODE_INVALID_RANGE                                457
#define RTSP_STATUS_CODE_PARAMETER_IS_READ_ONLY                       458
#define RTSP_STATUS_CODE_AGGREGATE_OPERATION_NOT_ALLOWED              459
#define RTSP_STATUS_CODE_ONLY_AGGREGATE_OPERATION_ALLOWED             460
#define RTSP_STATUS_CODE_UNSUPPORTED_TRANSPORT                        461
#define RTSP_STATUS_CODE_DESTINATION_UNREACHABLE                      462

#define RTSP_STATUS_CODE_INTERNAL_SERVER_ERROR                        500
#define RTSP_STATUS_CODE_NOT_IMPLEMENTED                              501
#define RTSP_STATUS_CODE_BAD_GATEWAY                                  502
#define RTSP_STATUS_CODE_SERVICE_UNAVAILABLE                          503
#define RTSP_STATUS_CODE_GATEWAY_TIME_OUT                             504
#define RTSP_STATUS_CODE_RTSP_VERSION_NOT_SUPPORTED                   505
#define RTSP_STATUS_CODE_OPTION_NOT_SUPPORTED                         551

#define RTSP_STATUS_CODE_UNSPECIFIED                                  UINT_MAX

#define RTSP_VERSION_START                                            "RTSP/"
#define RTSP_VERSION_START_LENGTH                                     5

class CRtspResponse
{
public:
  CRtspResponse(HRESULT *result);
  virtual ~CRtspResponse(void);

  /* get methods */

  // gets RTSP response version
  // @return : RTSP response version
  virtual const wchar_t *GetVersion(void);

  // gets RTSP response headers
  // @return : RTSP response headers
  virtual CRtspResponseHeaderCollection *GetResponseHeaders(void);

  // gets RTSP sequence number
  // @return : RTSP sequence number or RTSP_SEQUENCE_NUMBER_UNSPECIFIED if unspecified
  virtual unsigned int GetSequenceNumber(void);

  // gets RTSP status code
  // @return : RTSP status code or RTSP_STATUS_CODE_UNSPECIFIED if unspecified
  virtual unsigned int GetStatusCode(void);

  // gets RTSP status reason
  // @return : RTSP status reason or NULL if error
  virtual const wchar_t *GetStatusReason(void);

  // gets RTSP response content
  // @return : RTSP content response or NULL if none
  virtual const unsigned char *GetContent(void);

  // gets RTSP response content length
  // @return : RTSP response content length
  virtual unsigned int GetContentLength(void);

  // gets RTSP session ID
  // @return : session ID or NULL if not specified
  virtual const wchar_t *GetSessionId(void);

  /* set methods */

  /* other methods */

  // tests if response is successful (status codes lower than RTSP_STATUS_CODE_MULTIPLE_CHOICES)
  // @return : true if successful, false otherwise
  virtual bool IsSuccess(void);

  // tests if current instance is empty
  // @return : true if current instance is empty, false otherwise
  virtual bool IsEmpty(void);

  // parses buffer for RTSP response
  // @param buffer : buffer with raw data from socket to parse for RTSP response
  // @param length : the length of available data
  // @return :
  // 1. size of RTSP response if successful (zero for no RTSP response in buffer)
  // 2. HRESULT_FROM_WIN32(ERROR_MORE_DATA) if not enough data
  // 3. HRESULT_FROM_WIN32(ERROR_INVALID_DATA) if invalid data in buffer
  // 4. error code otherwise
  virtual HRESULT Parse(const unsigned char *buffer, unsigned int length);

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  virtual CRtspResponse *Clone(void);

protected:

  wchar_t *version;

  unsigned int sequenceNumber;

  unsigned int statusCode;

  wchar_t *statusReason;

  unsigned char *content;
  unsigned int contentLength;

  wchar_t *sessionId;

  CRtspResponseHeaderCollection *responseHeaders;

  // deeply clones current instance to cloned RTSP response
  // @param  clone : cloned RTSP response to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CRtspResponse *clone);

  // returns new RTSP response object to be used in cloning
  // @return : RTSP response object or NULL if error
  virtual CRtspResponse *CreateResponse(void);
};

#endif