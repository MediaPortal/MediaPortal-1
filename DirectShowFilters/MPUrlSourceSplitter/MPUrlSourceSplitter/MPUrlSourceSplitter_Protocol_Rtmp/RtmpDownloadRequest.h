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

#ifndef __RTMP_DOWNLOAD_REQUEST_DEFINED
#define __RTMP_DOWNLOAD_REQUEST_DEFINED

#include "DownloadRequest.h"

class CRtmpDownloadRequest : public CDownloadRequest
{
public:
  CRtmpDownloadRequest(void);
  virtual ~CRtmpDownloadRequest(void);

  /* get methods */

  // RTMP protocol specific variables getters
  virtual const wchar_t *GetRtmpApp(void);
  virtual const wchar_t *GetRtmpTcUrl(void);
  virtual const wchar_t *GetRtmpPageUrl(void);
  virtual const wchar_t *GetRtmpSwfUrl(void);
  virtual const wchar_t *GetRtmpFlashVersion(void);
  virtual const wchar_t *GetRtmpAuth(void);
  virtual const wchar_t *GetRtmpArbitraryData(void);
  virtual const wchar_t *GetRtmpPlayPath(void);
  virtual bool GetRtmpPlaylist(void);
  virtual bool GetRtmpLive(void);
  virtual const wchar_t *GetRtmpSubscribe(void);
  virtual int64_t GetRtmpStart(void);
  virtual int64_t GetRtmpStop(void);
  virtual unsigned int GetRtmpBuffer(void);
  virtual const wchar_t *GetRtmpToken(void);
  virtual const wchar_t *GetRtmpJtv(void);
  virtual bool GetRtmpSwfVerify(void);

  /* set methods */

  // RTMP protocol specific variables setters
  virtual void SetRtmpApp(const wchar_t *rtmpApp);
  virtual void SetRtmpTcUrl(const wchar_t *rtmpTcUrl);
  virtual void SetRtmpPageUrl(const wchar_t *rtmpPageUrl);
  virtual void SetRtmpSwfUrl(const wchar_t *rtmpSwfUrl);
  virtual void SetRtmpFlashVersion(const wchar_t *rtmpFlashVersion);
  virtual void SetRtmpAuth(const wchar_t *rtmpAuth);
  virtual void SetRtmpArbitraryData(const wchar_t *rtmpArbitraryData);
  virtual void SetRtmpPlayPath(const wchar_t *rtmpPlayPath);
  virtual void SetRtmpPlaylist(bool rtmpPlaylist);
  virtual void SetRtmpLive(bool rtmpLive);
  virtual void SetRtmpSubscribe(const wchar_t *rtmpSubscribe);
  virtual void SetRtmpStart(int64_t rtmpStart);
  virtual void SetRtmpStop(int64_t rtmpStop);
  virtual void SetRtmpBuffer(unsigned int rtmpBuffer);
  virtual void SetRtmpToken(const wchar_t *rtmpToken);
  virtual void SetRtmpJtv(const wchar_t *rtmpJtv);
  virtual void SetRtmpSwfVerify(bool rtmpSwfVerify);

  /* other methods */

  // deeply clones current instance
  // @result : deep clone of current instance or NULL if error
  virtual CRtmpDownloadRequest *Clone(void);

protected:

  // RTMP protocol specific variables

  // name of application to connect to on the RTMP server
  // if not NULL than overrides the app in the RTMP URL
  wchar_t *rtmpApp;

  // URL of the target stream
  // if not NULL than overrides the tcUrl in the RTMP URL
  wchar_t *rtmpTcUrl;

  // URL of the web page in which the media was embedded
  // if not NULL than value is sent
  wchar_t *rtmpPageUrl;

  // URL of the SWF player for the media
  // if not NULL than value is sent
  wchar_t *rtmpSwfUrl;

  // version of the Flash plugin used to run the SWF player
  // if not NULL than overrides the default flash version "LNX 10,0,32,18"
  wchar_t *rtmpFlashVersion;

  // authentication string to be appended to the connect string
  wchar_t *rtmpAuth;

  // if not NULL, than append arbitrary AMF data to the Connect message
  wchar_t *rtmpArbitraryData;

  // timeout the session after num of milliseconds without receiving any data from the server
  // if not set (UINT_MAX) then default value of 120 seconds is used
  unsigned int rtmpReceiveDataTimeout;

  // if not NULL than overrides the playpath parsed from the RTMP URL
  wchar_t *rtmpPlayPath;

  // if the value is true than issue a set_playlist command before sending the play command
  // the playlist will just contain the current playpath
  // if the value is false than the set_playlist command will not be sent
  // the default is RTMP_PLAYLIST_DEFAULT
  bool rtmpPlaylist;

  // specify that the media is a live stream
  // no resuming or seeking in live streams is possible
  bool rtmpLive;

  // name of live stream to subscribe to
  // defaults to playpath, if not NULL than value is sent
  wchar_t *rtmpSubscribe;

  // start at num seconds into the stream
  // not valid for live streams
  // the default value is not set (INT64_MAX)
  int64_t rtmpStart;

  // stop at num seconds into the stream
  // the default value is not set (INT64_MAX)
  int64_t rtmpStop;  

  // set buffer time to num milliseconds
  // the default is RTMP_BUFFER_DEFAULT
  unsigned int rtmpBuffer;

  // key for SecureToken response, used if the server requires SecureToken authentication
  wchar_t *rtmpToken;

  // JSON token used by legacy Justin.tv servers, invokes NetStream.Authenticate.UsherToken
  wchar_t *rtmpJtv;

  // if the value is true, the SWF player is retrieved from the specified swfUrl for performing SWF Verification
  // the SWF hash and size (used in the verification step) are computed automatically
  // also the SWF information is cached in a .swfinfo file in the user's home directory,
  // so that it doesn't need to be retrieved and recalculated every time
  // the .swfinfo file records the SWF URL, the time it was fetched, the modification timestamp of the SWF file,
  // its size, and its hash
  // by default, the cached info will be used for 30 days before re-checking
  // the default value is RTMP_SWF_VERIFY_DEFAULT
  bool rtmpSwfVerify;

  // deeply clones current instance to cloned request
  // @param  clonedRequest : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CRtmpDownloadRequest *clonedRequest);
};

#endif