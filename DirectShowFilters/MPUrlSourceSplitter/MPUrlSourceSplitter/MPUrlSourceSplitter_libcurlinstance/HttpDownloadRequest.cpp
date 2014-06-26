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

int CHttpDownloadRequest::GetHttpVersion(void)
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

void CHttpDownloadRequest::SetHttpVersion(int httpVersion)
{
  this->httpVersion = httpVersion;
}

void CHttpDownloadRequest::SetIgnoreContentLength(bool ignoreContentLength)
{
  this->flags &= ~HTTP_DOWNLOAD_REQUEST_FLAG_IGNORE_CONTENT_LENGTH;
  this->flags |= (ignoreContentLength) ? HTTP_DOWNLOAD_REQUEST_FLAG_IGNORE_CONTENT_LENGTH : HTTP_DOWNLOAD_REQUEST_FLAG_NONE;
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

    request->cookie = Duplicate(this->cookie);
    request->endPosition = this->endPosition;
    request->httpVersion = this->httpVersion;
    request->referer = Duplicate(this->referer);
    request->startPosition = this->startPosition;
    request->userAgent = Duplicate(this->userAgent);

    request->headers->Clear();

    result &= request->headers->Append(this->headers);
    result &= TEST_STRING_WITH_NULL(request->cookie, this->cookie);
    result &= TEST_STRING_WITH_NULL(request->referer, this->referer);
    result &= TEST_STRING_WITH_NULL(request->userAgent, this->userAgent);
  }

  return result;
}