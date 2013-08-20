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

CHttpDownloadRequest::CHttpDownloadRequest(void)
  : CDownloadRequest()
{
  this->cookie = NULL;
  this->endPosition = 0;
  this->ignoreContentLength = HTTP_IGNORE_CONTENT_LENGTH_DEFAULT;
  this->referer = NULL;
  this->startPosition = 0;
  this->userAgent = NULL;
  this->httpVersion = HTTP_VERSION_DEFAULT;
  this->headers = new CHttpHeaderCollection();
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
  return this->ignoreContentLength;
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
  this->ignoreContentLength = ignoreContentLength;
}

/* other methods */

CDownloadRequest *CHttpDownloadRequest::Clone(void)
{
  CHttpDownloadRequest *result = new CHttpDownloadRequest();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CHttpDownloadRequest::CloneInternal(CHttpDownloadRequest *clonedRequest)
{
  bool result = __super::CloneInternal(clonedRequest);

  if (result)
  {
    clonedRequest->cookie = Duplicate(this->cookie);
    clonedRequest->endPosition = this->endPosition;
    clonedRequest->httpVersion = this->httpVersion;
    clonedRequest->ignoreContentLength = this->ignoreContentLength;
    clonedRequest->referer = Duplicate(this->referer);
    clonedRequest->startPosition = this->startPosition;
    clonedRequest->userAgent = Duplicate(this->userAgent);

    clonedRequest->headers->Clear();

    result &= clonedRequest->headers->Append(this->headers);
    result &= TEST_STRING_WITH_NULL(clonedRequest->cookie, this->cookie);
    result &= TEST_STRING_WITH_NULL(clonedRequest->referer, this->referer);
    result &= TEST_STRING_WITH_NULL(clonedRequest->userAgent, this->userAgent);
  }

  return result;
}