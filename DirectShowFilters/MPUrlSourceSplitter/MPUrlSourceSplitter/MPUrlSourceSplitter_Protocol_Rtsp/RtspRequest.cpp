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

#include "RtspRequest.h"
#include "RtspSequenceRequestHeader.h"
#include "RtspSessionRequestHeader.h"

CRtspRequest::CRtspRequest(void)
{
  this->uri = NULL;
  this->request = NULL;
  this->version = NULL;
  this->requestHeaders = new CRtspRequestHeaderCollection();
  this->timeout = 0;

  if (this->requestHeaders != NULL)
  {
    CRtspSequenceRequestHeader *header = new CRtspSequenceRequestHeader();
    if (header != NULL)
    {
      if (!this->requestHeaders->Add(header))
      {
        FREE_MEM_CLASS(header);
      }
    }
  }
}

CRtspRequest::CRtspRequest(bool createDefaultHeaders)
{
  this->uri = NULL;
  this->request = NULL;
  this->version = NULL;
  this->requestHeaders = new CRtspRequestHeaderCollection();
  this->timeout = 0;

  if ((createDefaultHeaders) && (this->requestHeaders != NULL))
  {
    CRtspSequenceRequestHeader *header = new CRtspSequenceRequestHeader();
    if (header != NULL)
    {
      if (!this->requestHeaders->Add(header))
      {
        FREE_MEM_CLASS(header);
      }
    }
  }
}

CRtspRequest::~CRtspRequest(void)
{
  FREE_MEM(this->uri);
  FREE_MEM(this->request);
  FREE_MEM(this->version);
  FREE_MEM_CLASS(this->requestHeaders);
}

/* get methods */

const wchar_t *CRtspRequest::GetUri(void)
{
  return this->uri;
}

const wchar_t *CRtspRequest::GetVersion(void)
{
  return (this->version == NULL) ? RTSP_REQUEST_VERSION : this->version;
}

const wchar_t *CRtspRequest::GetRequest(void)
{
  FREE_MEM(this->request);
  this->CreateRequest();
  return this->request;
}

CRtspRequestHeaderCollection *CRtspRequest::GetRequestHeaders(void)
{
  return this->requestHeaders;
}

unsigned int CRtspRequest::GetSequenceNumber(void)
{
  unsigned int result = RTSP_SEQUENCE_NUMBER_UNSPECIFIED;

  if (this->requestHeaders != NULL)
  {
    CRtspSequenceRequestHeader *header = dynamic_cast<CRtspSequenceRequestHeader *>(this->requestHeaders->GetRtspHeader(RTSP_SEQUENCE_REQUEST_HEADER_NAME, false));
    if (header != NULL)
    {
      result = header->GetSequenceNumber();
    }
  }

  return result;
}

const wchar_t *CRtspRequest::GetSessionId(void)
{
  const wchar_t *result = NULL;

  if (this->requestHeaders != NULL)
  {
    CRtspSessionRequestHeader *header = dynamic_cast<CRtspSessionRequestHeader *>(this->requestHeaders->GetRtspHeader(RTSP_SESSION_REQUEST_HEADER_NAME, false));
    if (header != NULL)
    {
      result = header->GetSessionId();
    }
  }

  return result;
}

unsigned int CRtspRequest::GetTimeout(void)
{
  return this->timeout;
}

/* set methods */

bool CRtspRequest::SetUri(const wchar_t *uri)
{
  SET_STRING_RETURN_WITH_NULL(this->uri, uri);
}

bool CRtspRequest::SetVersion(const wchar_t *version)
{
  SET_STRING_RETURN_WITH_NULL(this->version, version);
}

void CRtspRequest::SetTimeout(unsigned int timeout)
{
  this->timeout = timeout;
}

void CRtspRequest::SetSequenceNumber(unsigned int sequenceNumber)
{
  if (this->requestHeaders != NULL)
  {
    CRtspSequenceRequestHeader *header = dynamic_cast<CRtspSequenceRequestHeader *>(this->requestHeaders->GetRtspHeader(RTSP_SEQUENCE_REQUEST_HEADER_NAME, false));
    if (header != NULL)
    {
      header->SetSequenceNumber(sequenceNumber);
    }
  }
}

bool CRtspRequest::SetSessionId(const wchar_t *sessionId)
{
  bool result = (this->requestHeaders != NULL);

  if (result)
  {
    unsigned int index = this->requestHeaders->GetRtspHeaderIndex(RTSP_SESSION_REQUEST_HEADER_NAME, false);

    if (index != UINT_MAX)
    {
      if (sessionId == NULL)
      {
        result &= this->requestHeaders->Remove(index);
      }
      else
      {
        CRtspSessionRequestHeader *header = dynamic_cast<CRtspSessionRequestHeader *>(this->requestHeaders->GetItem(index));
        result &= (header != NULL);

        if (result)
        {
          result &= header->SetSessionId(sessionId);
        }
      }
    }
    else if (sessionId != NULL)
    {
      // create new session ID header
      CRtspSessionRequestHeader *header = new CRtspSessionRequestHeader();
      result &= (header != NULL);
      
      CHECK_CONDITION_EXECUTE(result, result &= header->SetSessionId(sessionId));
      CHECK_CONDITION_EXECUTE(result, result &= this->requestHeaders->Add(header));
      CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(header));
    }
  }

  return result;
}

/* other methods */

bool CRtspRequest::CreateRequest(void)
{
  bool result = (this->GetRequestHeaders() != NULL);
  FREE_MEM(this->request);

  if (result)
  {
    wchar_t *requestLine = FormatString(RTSP_REQUEST_LINE_FORMAT, this->GetMethod(), this->GetUri(), this->GetVersion(), RTSP_CRLF);
    result &= (requestLine != NULL);

    wchar_t *requestHeader = Duplicate(L"\r\n");
    result &= (requestHeader != NULL);

    for (unsigned int i = 0; (result && (i < this->GetRequestHeaders()->Count())); i++)
    {
      CRtspRequestHeader *header = this->GetRequestHeaders()->GetItem(i);

      wchar_t *temp = FormatString(L"%s%s", header->GetRequestHeader(), requestHeader);
      result &= (temp != NULL);
      FREE_MEM(requestHeader);
      requestHeader = temp;
    }

    if (result)
    {
      this->request = FormatString(RTSP_REQUEST_FORMAT, requestLine, requestHeader);
      result &= (this->request != NULL);
    }

    FREE_MEM(requestLine);
    FREE_MEM(requestHeader);
  }

  return result;
}

CRtspRequest *CRtspRequest::Clone(void)
{
  CRtspRequest *result = this->GetNewRequest();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CRtspRequest::CloneInternal(CRtspRequest *clonedRequest)
{
  bool result = true;

  SET_STRING_AND_RESULT_WITH_NULL(clonedRequest->uri, this->uri, result);
  SET_STRING_AND_RESULT_WITH_NULL(clonedRequest->version, this->version, result);
  SET_STRING_AND_RESULT_WITH_NULL(clonedRequest->request, this->request, result);
  clonedRequest->timeout = this->timeout;

  result &= clonedRequest->requestHeaders->Append(this->requestHeaders);

  return result;
}

CRtspRequest *CRtspRequest::GetNewRequest(void)
{
  return NULL;
}