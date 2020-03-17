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

#include "HttpDownloadRequest.h"
#include "HttpCurlInstance.h"

CHttpDownloadRequest::CHttpDownloadRequest(HRESULT *result)
  : CDownloadRequest(result)
{
  this->cookie = NULL;
  this->endPosition = 0;
  this->referer = NULL;
  this->startPosition = 0;
  this->userAgent = NULL;
  this->httpVersion = HTTP_VERSION_DEFAULT;
  this->headers = NULL;
  this->serverUserName = NULL;
  this->serverPassword = NULL;
  this->proxyServer = NULL;
  this->proxyServerPort = 0;
  this->proxyServerUserName = NULL;
  this->proxyServerPassword = NULL;
  this->proxyServerType = HTTP_PROXY_TYPE_NONE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->headers = new CHttpHeaderCollection(result);
    CHECK_POINTER_HRESULT(*result, this->headers, *result, E_OUTOFMEMORY);
  }
}

CHttpDownloadRequest::~CHttpDownloadRequest(void)
{
  FREE_MEM(this->cookie);
  FREE_MEM(this->referer);
  FREE_MEM(this->userAgent);
  FREE_MEM_CLASS(this->headers);
  FREE_MEM(this->serverUserName);
  FREE_MEM(this->serverPassword);
  FREE_MEM(this->proxyServer);
  FREE_MEM(this->proxyServerUserName);
  FREE_MEM(this->proxyServerPassword);
}

/* get methods */

uint64_t CHttpDownloadRequest::GetStartPosition(void)
{
  return this->startPosition;
}

uint64_t CHttpDownloadRequest::GetEndPosition(void)
{
  return this->endPosition;
}

const wchar_t *CHttpDownloadRequest::GetCookie(void)
{
  return this->cookie;
}

const wchar_t *CHttpDownloadRequest::GetReferer(void)
{
  return this->referer;
}

const wchar_t *CHttpDownloadRequest::GetUserAgent(void)
{
  return this->userAgent;
}

unsigned int CHttpDownloadRequest::GetHttpVersion(void)
{
  return this->httpVersion;
}

bool CHttpDownloadRequest::GetIgnoreContentLength(void)
{
  return this->IsSetFlags(HTTP_DOWNLOAD_REQUEST_FLAG_IGNORE_CONTENT_LENGTH);
}

CHttpHeaderCollection *CHttpDownloadRequest::GetHeaders(void)
{
  return this->headers;
}

const wchar_t *CHttpDownloadRequest::GetServerUserName(void)
{
  return this->serverUserName;
}

const wchar_t *CHttpDownloadRequest::GetServerPassword(void)
{
  return this->serverPassword;
}

const wchar_t *CHttpDownloadRequest::GetProxyServer(void)
{
  return this->proxyServer;
}

unsigned short CHttpDownloadRequest::GetProxyServerPort(void)
{
  return this->proxyServerPort;
}

const wchar_t *CHttpDownloadRequest::GetProxyServerUserName(void)
{
  return this->proxyServerUserName;
}

const wchar_t *CHttpDownloadRequest::GetProxyServerPassword(void)
{
  return this->proxyServerPassword;
}

unsigned int CHttpDownloadRequest::GetProxyServerType(void)
{
  return this->proxyServerType;
}

/* set methods */

void CHttpDownloadRequest::SetStartPosition(uint64_t startPosition)
{
  this->startPosition = startPosition;
}

void CHttpDownloadRequest::SetEndPosition(uint64_t endPosition)
{
  this->endPosition = endPosition;
}

bool CHttpDownloadRequest::SetCookie(const wchar_t *cookie)
{
  SET_STRING_RETURN_WITH_NULL(this->cookie, cookie);
}

bool CHttpDownloadRequest::SetReferer(const wchar_t *referer)
{
  SET_STRING_RETURN_WITH_NULL(this->referer, referer);
}

bool CHttpDownloadRequest::SetUserAgent(const wchar_t *userAgent)
{
  SET_STRING_RETURN_WITH_NULL(this->userAgent, userAgent);
}

void CHttpDownloadRequest::SetHttpVersion(unsigned int httpVersion)
{
  this->httpVersion = httpVersion;
}

void CHttpDownloadRequest::SetIgnoreContentLength(bool ignoreContentLength)
{
  this->flags &= ~HTTP_DOWNLOAD_REQUEST_FLAG_IGNORE_CONTENT_LENGTH;
  this->flags |= (ignoreContentLength) ? HTTP_DOWNLOAD_REQUEST_FLAG_IGNORE_CONTENT_LENGTH : HTTP_DOWNLOAD_REQUEST_FLAG_NONE;
}

bool CHttpDownloadRequest::SetAuthentication(bool authenticate, const wchar_t *serverUserName, const wchar_t *serverPassword)
{
  bool result = true;
  this->flags &= ~HTTP_DOWNLOAD_REQUEST_FLAG_AUTHENTICATE;

  if (authenticate)
  {
    this->flags |= HTTP_DOWNLOAD_REQUEST_FLAG_AUTHENTICATE;

    SET_STRING_AND_RESULT_WITH_NULL(this->serverUserName, serverUserName, result);
    SET_STRING_AND_RESULT_WITH_NULL(this->serverPassword, serverPassword, result);
  }

  return result;
}

bool CHttpDownloadRequest::SetProxyAuthentication(bool authenticate, const wchar_t *proxyServer, unsigned short proxyServerPort, unsigned int proxyServerType, const wchar_t *proxyServerUserName, const wchar_t *proxyServerPassword)
{
  bool result = true;
  this->flags &= ~HTTP_DOWNLOAD_REQUEST_FLAG_PROXY_AUTHENTICATE;

  if (authenticate)
  {
    this->flags |= HTTP_DOWNLOAD_REQUEST_FLAG_PROXY_AUTHENTICATE;
    result &= (proxyServerType > HTTP_PROXY_TYPE_NONE) && (proxyServerType <= HTTP_PROXY_TYPE_SOCKS5_HOSTNAME);

    SET_STRING_AND_RESULT_WITH_NULL(this->proxyServer, proxyServer, result);
    SET_STRING_AND_RESULT_WITH_NULL(this->proxyServerUserName, proxyServerUserName, result);
    SET_STRING_AND_RESULT_WITH_NULL(this->proxyServerPassword, proxyServerPassword, result);
    this->proxyServerPort = proxyServerPort;
    this->proxyServerType = proxyServerType;
  }

  return result;
}

/* other methods */

/* protected methods */

CDownloadRequest *CHttpDownloadRequest::CreateDownloadRequest(void)
{
  HRESULT result = S_OK;
  CHttpDownloadRequest *request = new CHttpDownloadRequest(&result);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}

bool CHttpDownloadRequest::CloneInternal(CDownloadRequest *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CHttpDownloadRequest *request = dynamic_cast<CHttpDownloadRequest *>(clone);

    request->endPosition = this->endPosition;
    request->httpVersion = this->httpVersion;
    request->startPosition = this->startPosition;
    request->proxyServerPort = this->proxyServerPort;
    request->proxyServerType = this->proxyServerType;

    SET_STRING_AND_RESULT_WITH_NULL(request->cookie, this->cookie, result);
    SET_STRING_AND_RESULT_WITH_NULL(request->referer, this->referer, result);
    SET_STRING_AND_RESULT_WITH_NULL(request->userAgent, this->userAgent, result);
    SET_STRING_AND_RESULT_WITH_NULL(request->serverUserName, this->serverUserName, result);
    SET_STRING_AND_RESULT_WITH_NULL(request->serverPassword, this->serverPassword, result);
    SET_STRING_AND_RESULT_WITH_NULL(request->proxyServer, this->proxyServer, result);
    SET_STRING_AND_RESULT_WITH_NULL(request->proxyServerUserName, this->proxyServerUserName, result);
    SET_STRING_AND_RESULT_WITH_NULL(request->proxyServerPassword, this->proxyServerPassword, result);

    request->headers->Clear();

    result &= request->headers->Append(this->headers);
  }

  return result;
}