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

#ifndef __RTSP_DOWNLOAD_RESPONSE_DEFINED
#define __RTSP_DOWNLOAD_RESPONE_DEFINED

#include "DownloadResponse.h"
#include "SessionDescription.h"
#include "HttpHeaderCollection.h"
#include "RtspTrackCollection.h"

#include "RtspRequest.h"
#include "RtspResponse.h"

#define RTSP_DOWNLOAD_RESPONSE_FLAG_NONE                              DOWNLOAD_RESPONSE_FLAG_NONE

#define RTSP_DOWNLOAD_RESPONSE_FLAG_LAST                              (DOWNLOAD_RESPONSE_FLAG_LAST + 0)

class CRtspDownloadResponse : public CDownloadResponse
{
public:
  CRtspDownloadResponse(HRESULT *result);
  virtual ~CRtspDownloadResponse(void);

  /* get methods */

  // gets session description
  // @return : session description or NULL if error
  virtual CSessionDescription *GetSessionDescription(void);

  // gets raw session description response
  // @return : raw session description or NULL if error
  virtual const wchar_t *GetRawSessionDescription(void);

  // gets content base URL
  // @return : content base URL or NULL if not specified
  virtual const wchar_t *GetContentBaseUrl(void);

  // gets content location URL
  // @return : content location URL or NULL if not specified
  virtual const wchar_t *GetContentLocationUrl(void);

  // gets RTSP tracks
  // @return : RTSP track collection or NULL if error
  virtual CRtspTrackCollection *GetRtspTracks(void);

  // gets RTSP request to send to remote server
  // @return : RTSP request to send to remote server or NULL if none
  virtual CRtspRequest *GetRtspRequest(void);

  // gets RTSP response received from remote server
  // @return : RTSP response received from remote server or NULL if none
  virtual CRtspResponse *GetRtspResponse(void);

  // gets RTSP session timeout
  // @return : session timeout in ms
  virtual unsigned int GetSessionTimeout(void);

  /* set methods */

  // sets raw session description
  // @return : true if successful, false otherwise
  virtual bool SetRawSessionDescription(const wchar_t *rawSessionDescription);

  // sets content base URL
  // @param contentBaseUrl : the content base URL to set
  // @return : true if successful, false otherwise
  virtual bool SetContentBaseUrl(const wchar_t *contentBaseUrl);

  // sets content location URL
  // @param contentLocationUrl : the content location URL to set
  // @return : true if successful, false otherwise
  virtual bool SetContentLocationUrl(const wchar_t *contentLocationUrl);

  // sets RTSP request to send to remote server
  // @param request : RTSP request to send to remote server
  virtual void SetRtspRequest(CRtspRequest *request);

  // sets RTSP response received from remote server (by default is empty reponse)
  // @param response : RTSP response received from remote server
  virtual void SetRtspResponse(CRtspResponse *response);

  // sets RTSP session timeout
  // @param sessionTimeout : the session timeout in ms to set
  virtual void SetSessionTimeout(unsigned int sessionTimeout);

  /* other methods */

  // clears RTSP request
  virtual void ClearRtspRequest(void);

  // clears RTSP response
  virtual void ClearRtspResponse(void);

  // clears RTSP request and response
  virtual void ClearRtspRequestAndResponse(void);

  // parses raw session description
  // @return : true if successful, false otherwise
  virtual bool ParseRawSessionDescription(void);

protected:
  // holds raw session description (unparsed)
  wchar_t *sessionDescriptionRaw;

  // holds session description received after DESCRIBE request
  CSessionDescription *sessionDescription;

  // holds content base URL
  wchar_t *contentBaseUrl;

  // holds content location URL
  wchar_t *contentLocationUrl;

  // holds tracks
  CRtspTrackCollection *tracks;

  // holds RTSP request and response pair
  CRtspRequest *rtspRequest;
  CRtspResponse *rtspResponse;

  // holds session timeout
  unsigned int sessionTimeout;

  /* methods */

  // creates download response
  // @return : download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // deeply clones current instance to cloned request
  // @param  clone : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadResponse *clone);
};

#endif