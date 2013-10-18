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

#include "StdAfx.h"

#include "RtmpDownloadRequest.h"
#include "RtmpCurlInstance.h"

CRtmpDownloadRequest::CRtmpDownloadRequest(void)
  : CDownloadRequest()
{
  this->rtmpApp = RTMP_APP_DEFAULT;
  this->rtmpArbitraryData = RTMP_ARBITRARY_DATA_DEFAULT;
  this->rtmpBuffer = RTMP_BUFFER_DEFAULT;
  this->rtmpFlashVersion = RTMP_FLASH_VER_DEFAULT;
  this->rtmpAuth = RTMP_AUTH_DEFAULT;
  this->rtmpJtv = RTMP_JTV_DEFAULT;
  this->rtmpLive = RTMP_LIVE_DEFAULT;
  this->rtmpPageUrl = RTMP_PAGE_URL_DEFAULT;
  this->rtmpPlaylist = RTMP_PLAYLIST_DEFAULT;
  this->rtmpPlayPath = RTMP_PLAY_PATH_DEFAULT;
  this->rtmpReceiveDataTimeout = RTMP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->rtmpStart = RTMP_START_DEFAULT;
  this->rtmpStop = RTMP_STOP_DEFAULT;
  this->rtmpSubscribe = RTMP_SUBSCRIBE_DEFAULT;
  this->rtmpSwfUrl = RTMP_SWF_URL_DEFAULT;
  this->rtmpSwfVerify = RTMP_SWF_VERIFY_DEFAULT;
  this->rtmpTcUrl = RTMP_TC_URL_DEFAULT;
  this->rtmpToken = RTMP_TOKEN_DEFAULT;
}

CRtmpDownloadRequest::~CRtmpDownloadRequest(void)
{
  FREE_MEM(this->rtmpApp);
  FREE_MEM(this->rtmpArbitraryData);
  FREE_MEM(this->rtmpFlashVersion);
  FREE_MEM(this->rtmpAuth);
  FREE_MEM(this->rtmpJtv);
  FREE_MEM(this->rtmpPageUrl);
  FREE_MEM(this->rtmpPlayPath);
  FREE_MEM(this->rtmpSubscribe);
  FREE_MEM(this->rtmpSwfUrl);
  FREE_MEM(this->rtmpTcUrl);
  FREE_MEM(this->rtmpToken);
}

/* get methods */

const wchar_t *CRtmpDownloadRequest::GetRtmpApp(void)
{
  return this->rtmpApp;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpTcUrl(void)
{
  return this->rtmpTcUrl;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpPageUrl(void)
{
  return this->rtmpPageUrl;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpSwfUrl(void)
{
  return this->rtmpSwfUrl;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpFlashVersion(void)
{
  return this->rtmpFlashVersion;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpAuth(void)
{
  return this->rtmpAuth;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpArbitraryData(void)
{
  return this->rtmpArbitraryData;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpPlayPath(void)
{
  return this->rtmpPlayPath;
}

bool CRtmpDownloadRequest::GetRtmpPlaylist(void)
{
  return this->rtmpPlaylist;
}

bool CRtmpDownloadRequest::GetRtmpLive(void)
{
  return this->rtmpLive;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpSubscribe(void)
{
  return this->rtmpSubscribe;
}

int64_t CRtmpDownloadRequest::GetRtmpStart(void)
{
  return this->rtmpStart;
}

int64_t CRtmpDownloadRequest::GetRtmpStop(void)
{
  return this->rtmpStop;
}

unsigned int CRtmpDownloadRequest::GetRtmpBuffer(void)
{
  return this->rtmpBuffer;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpToken(void)
{
  return this->rtmpToken;
}

const wchar_t *CRtmpDownloadRequest::GetRtmpJtv(void)
{
  return this->rtmpJtv;
}

bool CRtmpDownloadRequest::GetRtmpSwfVerify(void)
{
  return this->rtmpSwfVerify;
}

/* set methods */

void CRtmpDownloadRequest::SetRtmpApp(const wchar_t *rtmpApp)
{
  FREE_MEM(this->rtmpApp);
  this->rtmpApp = Duplicate(rtmpApp);
}

void CRtmpDownloadRequest::SetRtmpTcUrl(const wchar_t *rtmpTcUrl)
{
  FREE_MEM(this->rtmpTcUrl);
  this->rtmpTcUrl = Duplicate(rtmpTcUrl);
}

void CRtmpDownloadRequest::SetRtmpPageUrl(const wchar_t *rtmpPageUrl)
{
  FREE_MEM(this->rtmpPageUrl);
  this->rtmpPageUrl = Duplicate(rtmpPageUrl);
}

void CRtmpDownloadRequest::SetRtmpSwfUrl(const wchar_t *rtmpSwfUrl)
{
  FREE_MEM(this->rtmpSwfUrl);
  this->rtmpSwfUrl = Duplicate(rtmpSwfUrl);
}

void CRtmpDownloadRequest::SetRtmpFlashVersion(const wchar_t *rtmpFlashVersion)
{
  FREE_MEM(this->rtmpFlashVersion);
  this->rtmpFlashVersion = Duplicate(rtmpFlashVersion);
}

void CRtmpDownloadRequest::SetRtmpAuth(const wchar_t *rtmpAuth)
{
  FREE_MEM(this->rtmpAuth);
  this->rtmpAuth = Duplicate(rtmpAuth);
}

void CRtmpDownloadRequest::SetRtmpArbitraryData(const wchar_t *rtmpArbitraryData)
{
  FREE_MEM(this->rtmpArbitraryData);
  this->rtmpArbitraryData = Duplicate(rtmpArbitraryData);
}

void CRtmpDownloadRequest::SetRtmpPlayPath(const wchar_t *rtmpPlayPath)
{
  FREE_MEM(this->rtmpPlayPath);
  this->rtmpPlayPath = Duplicate(rtmpPlayPath);
}

void CRtmpDownloadRequest::SetRtmpPlaylist(bool rtmpPlaylist)
{
  this->rtmpPlaylist = rtmpPlaylist;
}

void CRtmpDownloadRequest::SetRtmpLive(bool rtmpLive)
{
  this->rtmpLive = rtmpLive;
}

void CRtmpDownloadRequest::SetRtmpSubscribe(const wchar_t *rtmpSubscribe)
{
  FREE_MEM(this->rtmpSubscribe);
  this->rtmpSubscribe = Duplicate(rtmpSubscribe);
}

void CRtmpDownloadRequest::SetRtmpStart(int64_t rtmpStart)
{
  this->rtmpStart = rtmpStart;
}

void CRtmpDownloadRequest::SetRtmpStop(int64_t rtmpStop)
{
  this->rtmpStop = this->rtmpStop;
}

void CRtmpDownloadRequest::SetRtmpBuffer(unsigned int rtmpBuffer)
{
  this->rtmpBuffer = rtmpBuffer;
}

void CRtmpDownloadRequest::SetRtmpToken(const wchar_t *rtmpToken)
{
  FREE_MEM(this->rtmpToken);
  this->rtmpToken = Duplicate(rtmpToken);
}

void CRtmpDownloadRequest::SetRtmpJtv(const wchar_t *rtmpJtv)
{
  FREE_MEM(this->rtmpJtv);
  this->rtmpJtv = Duplicate(rtmpJtv);
}

void CRtmpDownloadRequest::SetRtmpSwfVerify(bool rtmpSwfVerify)
{
  this->rtmpSwfVerify = rtmpSwfVerify;
}

/* other methods */

CRtmpDownloadRequest *CRtmpDownloadRequest::Clone(void)
{
  CRtmpDownloadRequest *result = new CRtmpDownloadRequest();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CRtmpDownloadRequest::CloneInternal(CRtmpDownloadRequest *clonedRequest)
{
  bool result = __super::CloneInternal(clonedRequest);
  if (result)
  {
    FREE_MEM(clonedRequest->rtmpApp);
    FREE_MEM(clonedRequest->rtmpArbitraryData);
    FREE_MEM(clonedRequest->rtmpFlashVersion);
    FREE_MEM(clonedRequest->rtmpAuth);
    FREE_MEM(clonedRequest->rtmpJtv);
    FREE_MEM(clonedRequest->rtmpPageUrl);
    FREE_MEM(clonedRequest->rtmpPlayPath);
    FREE_MEM(clonedRequest->rtmpSubscribe);
    FREE_MEM(clonedRequest->rtmpSwfUrl);
    FREE_MEM(clonedRequest->rtmpTcUrl);
    FREE_MEM(clonedRequest->rtmpToken);

    clonedRequest->rtmpApp = Duplicate(this->rtmpApp);
    clonedRequest->rtmpArbitraryData = Duplicate(this->rtmpArbitraryData);
    clonedRequest->rtmpBuffer = this->rtmpBuffer;
    clonedRequest->rtmpFlashVersion = Duplicate(this->rtmpFlashVersion);
    clonedRequest->rtmpAuth = Duplicate(this->rtmpAuth);
    clonedRequest->rtmpJtv = Duplicate(this->rtmpJtv);
    clonedRequest->rtmpLive = this->rtmpLive;
    clonedRequest->rtmpPageUrl = Duplicate(this->rtmpPageUrl);
    clonedRequest->rtmpPlaylist = this->rtmpPlaylist;
    clonedRequest->rtmpPlayPath = Duplicate(this->rtmpPlayPath);
    clonedRequest->rtmpReceiveDataTimeout = this->rtmpReceiveDataTimeout;
    clonedRequest->rtmpStart = this->rtmpStart;
    clonedRequest->rtmpStop = this->rtmpStop;
    clonedRequest->rtmpSubscribe = Duplicate(this->rtmpSubscribe);
    clonedRequest->rtmpSwfUrl = Duplicate(this->rtmpSwfUrl);
    clonedRequest->rtmpSwfVerify = this->rtmpSwfVerify;
    clonedRequest->rtmpTcUrl = Duplicate(this->rtmpTcUrl);
    clonedRequest->rtmpToken = Duplicate(this->rtmpToken);

    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpApp, this->rtmpApp);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpArbitraryData, this->rtmpArbitraryData);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpFlashVersion, this->rtmpFlashVersion);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpAuth, this->rtmpAuth);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpJtv, this->rtmpJtv);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpPageUrl, this->rtmpPageUrl);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpPlayPath, this->rtmpPlayPath);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpSubscribe, this->rtmpSubscribe);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpSwfUrl, this->rtmpSwfUrl);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpTcUrl, this->rtmpTcUrl);
    result &= TEST_STRING_WITH_NULL(clonedRequest->rtmpToken, this->rtmpToken);
  }
  return result;
}