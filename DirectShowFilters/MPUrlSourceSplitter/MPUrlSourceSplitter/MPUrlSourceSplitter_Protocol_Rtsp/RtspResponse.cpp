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

#include "RtspResponse.h"
#include "RtspResponseHeaderFactory.h"
#include "conversions.h"
#include "RtspContentLengthResponseHeader.h"
#include "RtspSessionResponseHeader.h"

CRtspResponse::CRtspResponse(HRESULT *result)
{
  this->version = NULL;
  this->sequenceNumber = RTSP_SEQUENCE_NUMBER_UNSPECIFIED;
  this->responseHeaders = NULL;
  this->statusCode = RTSP_STATUS_CODE_UNSPECIFIED;
  this->statusReason = NULL;
  this->content = NULL;
  this->contentLength = 0;
  this->sessionId = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->responseHeaders = new CRtspResponseHeaderCollection(result);
    CHECK_POINTER_HRESULT(*result, this->responseHeaders, *result, E_OUTOFMEMORY);
  }
}

CRtspResponse::~CRtspResponse(void)
{
  FREE_MEM(this->version);
  FREE_MEM(this->statusReason);
  FREE_MEM_CLASS(this->responseHeaders);
  FREE_MEM(this->content);
  FREE_MEM(this->sessionId);
}

/* get methods */

const wchar_t *CRtspResponse::GetVersion(void)
{
  return this->version;
}

CRtspResponseHeaderCollection *CRtspResponse::GetResponseHeaders(void)
{
  return this->responseHeaders;
}

unsigned int CRtspResponse::GetSequenceNumber(void)
{
  return this->sequenceNumber;
}

unsigned int CRtspResponse::GetStatusCode(void)
{
  return this->statusCode;
}

const wchar_t *CRtspResponse::GetStatusReason(void)
{
  return this->statusReason;
}

const unsigned char *CRtspResponse::GetContent(void)
{
  return this->content;
}

unsigned int CRtspResponse::GetContentLength(void)
{
  return this->contentLength;
}

const wchar_t *CRtspResponse::GetSessionId(void)
{
  return this->sessionId;
}

/* set methods */

/* other methods */

bool CRtspResponse::IsSuccess(void)
{
  return (this->statusCode < RTSP_STATUS_CODE_MULTIPLE_CHOICES);
}

HRESULT CRtspResponse::Parse(const unsigned char *buffer, unsigned int length)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->responseHeaders);
  CHECK_CONDITION_HRESULT(result, length > RTSP_VERSION_START_LENGTH, result, HRESULT_FROM_WIN32(ERROR_MORE_DATA));

  if (SUCCEEDED(result))
  {
    result = S_OK;
    FREE_MEM(this->version);
    FREE_MEM(this->statusReason);
    FREE_MEM(this->content);
    FREE_MEM(this->sessionId);
    this->contentLength = 0;
    this->statusCode = RTSP_STATUS_CODE_UNSPECIFIED;
    this->sequenceNumber = RTSP_SEQUENCE_NUMBER_UNSPECIFIED;
    this->responseHeaders->Clear();

    // compare first RTSP_VERSION_START_LENGTH characters in buffer with RTSP_VERSION_START
    // if equal, then we have RTSP response

    if (strncmp((const char *)buffer, RTSP_VERSION_START, RTSP_VERSION_START_LENGTH) == 0)
    {
      const char *data = (const char *)buffer;
      // each RTSP response must be closed with CRLF CRLF - empty line with line ending

      CRtspResponseHeaderFactory *factory = new CRtspResponseHeaderFactory();
      CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

      bool foundEndOfResponse = false;
      while (SUCCEEDED(result) && (!foundEndOfResponse))
      {
        LineEnding ending = GetEndOfLineA(data, length, result);

        if (ending.position == (-1))
        {
          break;
        }
        else if (ending.position == result)
        {
          // empty line
          result = ending.position + ending.size;
          foundEndOfResponse = true;
        }
        else
        {
          // process line
          unsigned int lineLength = ending.position + 1 - result;
          ALLOC_MEM_DEFINE_SET(lineA, char, lineLength, 0);
          CHECK_POINTER_HRESULT(result, lineA, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            memcpy(lineA, buffer + result, lineLength - 1);

            wchar_t *lineW = ConvertToUnicodeA(lineA);
            CHECK_POINTER_HRESULT(result, lineW, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              CRtspResponseHeader *header = factory->CreateResponseHeader(lineW, lineLength - 1);
              CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                if ((header->GetNameLength() == 0) && (result == 0))
                {
                  // header with name length zero and position is zero - it have to be first line (RTSP status line)
                  // the RTSP status line is in form: RTSP-Version SP Status-Code SP Reason-Phrase
                  unsigned int headerValueLength = header->GetValueLength();

                  int spaceIndex = IndexOf(header->GetValue() + RTSP_VERSION_START_LENGTH, headerValueLength - RTSP_VERSION_START_LENGTH, L" ", 1);
                  CHECK_CONDITION_HRESULT(result, spaceIndex != (-1), result, HRESULT_FROM_WIN32(ERROR_INVALID_DATA));

                  if (SUCCEEDED(result))
                  {
                    this->version = Substring(header->GetValue() + RTSP_VERSION_START_LENGTH, 0, spaceIndex);
                  }

                  int beforeStatusCode = RTSP_VERSION_START_LENGTH + spaceIndex + 1;
                  spaceIndex = IndexOf(header->GetValue() + beforeStatusCode, headerValueLength - beforeStatusCode, L" ", 1);
                  CHECK_CONDITION_HRESULT(result, spaceIndex != (-1), result, HRESULT_FROM_WIN32(ERROR_INVALID_DATA));

                  if (SUCCEEDED(result))
                  {
                    this->statusCode = GetValueUint(header->GetValue() + beforeStatusCode, RTSP_STATUS_CODE_UNSPECIFIED);
                    this->statusReason = Substring(header->GetValue(), beforeStatusCode + spaceIndex + 1, headerValueLength - beforeStatusCode - spaceIndex - 1);
                  }
                }

                if (header->IsResponseHeaderType(RTSP_SEQUENCE_RESPONSE_HEADER_TYPE))
                {
                  CRtspSequenceResponseHeader *sequenceResponseHeader = (CRtspSequenceResponseHeader *)header;
                  this->sequenceNumber = sequenceResponseHeader->GetSequenceNumber();
                }

                if (header->IsResponseHeaderType(RTSP_CONTENT_LENGTH_RESPONSE_HEADER_TYPE))
                {
                  CRtspContentLengthResponseHeader *contentLengthResponseHeader = (CRtspContentLengthResponseHeader *)header;
                  this->contentLength = contentLengthResponseHeader->GetContentLength();
                }

                if (header->IsResponseHeaderType(RTSP_SESSION_RESPONSE_HEADER_TYPE))
                {
                  CRtspSessionResponseHeader *sessionResponseHeader = (CRtspSessionResponseHeader *)header;
                  this->sessionId = Duplicate(sessionResponseHeader->GetSessionId());
                }

                result = (this->responseHeaders->Add(header)) ? result : E_OUTOFMEMORY;
              }

              if (FAILED(result))
              {
                FREE_MEM_CLASS(header);
              }
            }

            FREE_MEM(lineW);
          }

          FREE_MEM(lineA);

          // move to next line
          result = ending.position + ending.size;
        }
      }

      FREE_MEM_CLASS(factory);
      CHECK_CONDITION_HRESULT(result, foundEndOfResponse, result, HRESULT_FROM_WIN32(ERROR_MORE_DATA));

      if (SUCCEEDED(result) && foundEndOfResponse && (this->contentLength != 0))
      {
        // check the length of buffer if we have enough data
        CHECK_CONDITION_HRESULT(result, (result + this->contentLength) <= length, result, HRESULT_FROM_WIN32(ERROR_MORE_DATA));

        if (SUCCEEDED(result))
        {
          // we have enough data
          this->content = ALLOC_MEM_SET(this->content, unsigned char, this->contentLength, 0);
          CHECK_CONDITION_HRESULT(result, this->content != NULL, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            memcpy(this->content, buffer + result, this->contentLength);
            result += this->contentLength;
          }
        }
      }

      CHECK_CONDITION_HRESULT(result, this->version != NULL, result, HRESULT_FROM_WIN32(ERROR_INVALID_DATA));
      CHECK_CONDITION_HRESULT(result, this->sequenceNumber != RTSP_SEQUENCE_NUMBER_UNSPECIFIED, result, HRESULT_FROM_WIN32(ERROR_INVALID_DATA));
      CHECK_CONDITION_HRESULT(result, this->statusCode != RTSP_STATUS_CODE_UNSPECIFIED, result, HRESULT_FROM_WIN32(ERROR_INVALID_DATA));
      CHECK_CONDITION_HRESULT(result, this->statusReason != NULL, result, HRESULT_FROM_WIN32(ERROR_INVALID_DATA));
    }
  }

  if (FAILED(result))
  {
    FREE_MEM(this->version);
    FREE_MEM(this->statusReason);
    FREE_MEM(this->content);
    this->contentLength = 0;
    this->statusCode = RTSP_STATUS_CODE_UNSPECIFIED;
    this->sequenceNumber = RTSP_SEQUENCE_NUMBER_UNSPECIFIED;
    CHECK_CONDITION_EXECUTE(this->responseHeaders != NULL, this->responseHeaders->Clear());
  }

  return result;
}

bool CRtspResponse::IsEmpty(void)
{
  return ((this->version == NULL) && (this->statusReason == NULL) && (this->statusCode == RTSP_STATUS_CODE_UNSPECIFIED) && (this->sequenceNumber == RTSP_SEQUENCE_NUMBER_UNSPECIFIED));
}

CRtspResponse *CRtspResponse::Clone(void)
{
  CRtspResponse *result = this->CreateResponse();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CRtspResponse::CloneInternal(CRtspResponse *clone)
{
  bool result = true;

  SET_STRING_AND_RESULT_WITH_NULL(clone->version, this->version, result);
  SET_STRING_AND_RESULT_WITH_NULL(clone->statusReason, this->statusReason, result);
  SET_STRING_AND_RESULT_WITH_NULL(clone->sessionId, this->sessionId, result);
  clone->sequenceNumber = this->sequenceNumber;
  clone->statusCode = this->statusCode;
  clone->contentLength = this->contentLength;

  if (this->contentLength != 0)
  {
    clone->content = ALLOC_MEM_SET(clone->content, unsigned char, this->contentLength, 0);
    result = (clone->content != NULL);

    if (result)
    {
      memcpy(clone->content, this->content, this->contentLength);
    }
  }

  result &= clone->responseHeaders->Append(this->responseHeaders);

  return result;
}

CRtspResponse *CRtspResponse::CreateResponse(void)
{
  HRESULT result = S_OK;
  CRtspResponse *response = new CRtspResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}