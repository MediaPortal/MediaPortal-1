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

CRtspRequest::CRtspRequest(HRESULT *result)
{
  this->uri = NULL;
  this->request = NULL;
  this->version = NULL;
  this->requestHeaders = NULL;
  this->timeout = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->requestHeaders = new CRtspRequestHeaderCollection(result);
    CHECK_POINTER_HRESULT(*result, this->requestHeaders, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(*result) && (this->requestHeaders != NULL))
    {
      CRtspSequenceRequestHeader *header = new CRtspSequenceRequestHeader(result);
      CHECK_POINTER_HRESULT(*result, header, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, this->requestHeaders->Add(header), *result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(header));
    }
  }
}

CRtspRequest::CRtspRequest(HRESULT *result, bool createDefaultHeaders)
{
  this->uri = NULL;
  this->request = NULL;
  this->version = NULL;
  this->requestHeaders = NULL;
  this->timeout = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->requestHeaders = new CRtspRequestHeaderCollection(result);
    CHECK_POINTER_HRESULT(*result, this->requestHeaders, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(*result) && createDefaultHeaders && (this->requestHeaders != NULL))
    {
      CRtspSequenceRequestHeader *header = new CRtspSequenceRequestHeader(result);
      CHECK_POINTER_HRESULT(*result, header, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, this->requestHeaders->Add(header), *result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(header));
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
  HRESULT result = (this->requestHeaders != NULL) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    unsigned int index = this->requestHeaders->GetRtspHeaderIndex(RTSP_SESSION_REQUEST_HEADER_NAME, false);

    if (index != UINT_MAX)
    {
      if (sessionId == NULL)
      {
        CHECK_CONDITION_HRESULT(result, this->requestHeaders->Remove(index), result, E_FAIL);
      }
      else
      {
        CRtspSessionRequestHeader *header = dynamic_cast<CRtspSessionRequestHeader *>(this->requestHeaders->GetItem(index));
        CHECK_POINTER_HRESULT(result, header, result, E_FAIL);

        CHECK_CONDITION_HRESULT(result, header->SetSessionId(sessionId), result, E_OUTOFMEMORY);
      }
    }
    else if (sessionId != NULL)
    {
      // create new session ID header
      CRtspSessionRequestHeader *header = new CRtspSessionRequestHeader(&result);
      CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, header->SetSessionId(sessionId), result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(result, this->requestHeaders->Add(header), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
    }
  }

  return SUCCEEDED(result);
}

/* other methods */

//bool CRtspRequest::CreateRequest(void)
//{
//  bool result = (this->GetRequestHeaders() != NULL);
//  FREE_MEM(this->request);
//
//  if (result)
//  {
//    wchar_t *requestLine = FormatString(RTSP_REQUEST_LINE_FORMAT, this->GetMethod(), this->GetUri(), this->GetVersion(), RTSP_CRLF);
//    result &= (requestLine != NULL);
//
//    wchar_t *requestHeader = Duplicate(L"\r\n");
//    result &= (requestHeader != NULL);
//
//    for (unsigned int i = 0; (result && (i < this->GetRequestHeaders()->Count())); i++)
//    {
//      CRtspRequestHeader *header = this->GetRequestHeaders()->GetItem(i);
//
//      wchar_t *temp = FormatString(L"%s%s", header->GetRequestHeader(), requestHeader);
//      result &= (temp != NULL);
//      FREE_MEM(requestHeader);
//      requestHeader = temp;
//    }
//
//    if (result)
//    {
//      this->request = FormatString(RTSP_REQUEST_FORMAT, requestLine, requestHeader);
//      result &= (this->request != NULL);
//    }
//
//    FREE_MEM(requestLine);
//    FREE_MEM(requestHeader);
//  }
//
//  return result;
//}

CRtspRequest *CRtspRequest::Clone(void)
{
  CRtspRequest *result = this->CreateRequest();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

/* protected methods */

bool CRtspRequest::CloneInternal(CRtspRequest *clone)
{
  bool result = true;

  SET_STRING_AND_RESULT_WITH_NULL(clone->uri, this->uri, result);
  SET_STRING_AND_RESULT_WITH_NULL(clone->version, this->version, result);
  SET_STRING_AND_RESULT_WITH_NULL(clone->request, this->request, result);
  clone->timeout = this->timeout;

  result &= clone->requestHeaders->Append(this->requestHeaders);

  return result;
}