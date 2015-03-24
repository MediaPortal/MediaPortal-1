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

#ifndef __HTTP_CURL_INSTANCE_DEFINED
#define __HTTP_CURL_INSTANCE_DEFINED

#include "CurlInstance.h"
#include "HttpDownloadRequest.h"
#include "HttpDownloadResponse.h"

#define HTTP_VERSION_NONE                                                     0
#define HTTP_VERSION_FORCE_HTTP10                                             1
#define HTTP_VERSION_FORCE_HTTP11                                             2

#define HTTP_VERSION_DEFAULT                                                  HTTP_VERSION_NONE
#define HTTP_IGNORE_CONTENT_LENGTH_DEFAULT                                    false

#define HTTP_CURL_INSTANCE_FLAG_NONE                                          CURL_INSTANCE_FLAG_NONE

#define HTTP_CURL_INSTANCE_FLAG_LAST                                          (CURL_INSTANCE_FLAG_LAST + 0)

class CHttpCurlInstance : public CCurlInstance
{
public:
  // initializes a new instance of CHttpCurlInstance class
  // @param logger : logger for logging purposes
  // @param mutex : mutex for locking access to receive data buffer
  // @param protocolName : the protocol name instantiating
  // @param instanceName : the name of CURL instance
  CHttpCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName);

  ~CHttpCurlInstance(void);

  /* get methods */

  // gets download request
  // @return : download request
  virtual CHttpDownloadRequest *GetHttpDownloadRequest(void);

  // gets download response
  // @return : download respose
  virtual CHttpDownloadResponse *GetHttpDownloadResponse(void);

  // gets download content length
  // @return : download content length or -1 if error or unknown
  virtual double GetDownloadContentLength(void);

  // gets current cookies in CURL instance
  // @return : collecion of current cookies
  virtual CParameterCollection *GetCurrentCookies(void);

  /* set methods */

  // adds cookies to current cookies in CURL instance (must be done before calling Initialize() method)
  // @param cookies : collection of cookies previously get by GetCurrentCookies() method
  // @return : true if successful, false otherwise
  virtual bool AddCookies(CParameterCollection *cookies);

  // sets current cookies in CURL instance (must be done before calling Initialize() method)
  // @param cookies : collection of cookies previously get by GetCurrentCookies() method
  // @return : true if successful, false otherwise
  virtual bool SetCurrentCookies(CParameterCollection *cookies);

  /* other methods */

  // initializes CURL instance
  // @param downloadRequest : download request
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT Initialize(CDownloadRequest *downloadRequest);

protected:
  // holds HTTP download request
  // never created and never destroyed
  // initialized in constructor by deep cloning
  CHttpDownloadRequest *httpDownloadRequest;

  // holds HTTP download response
  CHttpDownloadResponse *httpDownloadResponse;

  curl_slist *httpHeaders;

  // holds cookies to initialize instance
  curl_slist *cookies;

  /* methods */

  // called when CURL debug message arives
  // @param type : CURL message type
  // @param data : received CURL message data
  virtual void CurlDebug(curl_infotype type, const wchar_t *data);

  // process received data
  // @param dumpBox : the dump box for dump file (can be NULL if dumping is not required)
  // @param buffer : buffer with received data
  // @param length : the length of buffer
  // @return : the length of processed data (lower value than length means error)
  virtual size_t CurlReceiveData(CDumpBox *dumpBox, const unsigned char *buffer, size_t length);

  // appends header to HTTP headers
  // @param header : HTTP header to append
  // @return : true if successful, false otherwise
  bool AppendToHeaders(CHttpHeader *header);

  // clears headers
  void ClearHeaders(void);

  // gets new instance of download response
  // @return : new download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // destroys libcurl worker
  // @return : S_OK if successful
  virtual HRESULT DestroyCurlWorker(void);

  // creates dump box for dump file
  // @return : dump box or NULL if error
  virtual CDumpBox *CreateDumpBox(void);
};

#endif