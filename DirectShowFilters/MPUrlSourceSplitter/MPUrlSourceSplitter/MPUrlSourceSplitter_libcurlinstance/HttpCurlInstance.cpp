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

#include "HttpCurlInstance.h"

CHttpCurlInstance::CHttpCurlInstance(CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(logger, mutex, protocolName, instanceName)
{
  this->httpHeaders = NULL;
  this->cookies = NULL;

  this->httpDownloadRequest = dynamic_cast<CHttpDownloadRequest *>(this->downloadRequest);
  this->httpDownloadResponse = dynamic_cast<CHttpDownloadResponse *>(this->downloadResponse);
}

CHttpCurlInstance::~CHttpCurlInstance(void)
{
  this->StopReceivingData();

  if (this->cookies != NULL)
  {
    curl_slist_free_all(this->cookies);
    this->cookies = NULL;
  }

  this->ClearHeaders();
}

bool CHttpCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  bool result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->httpDownloadRequest = dynamic_cast<CHttpDownloadRequest *>(this->downloadRequest);
  this->httpDownloadResponse = dynamic_cast<CHttpDownloadResponse *>(this->downloadResponse);
  result &= (this->httpDownloadRequest != NULL) && (this->httpDownloadResponse != NULL);

  if (result)
  {
    CURLcode errorCode = CURLE_OK;
    errorCode = curl_easy_setopt(this->curl, CURLOPT_FOLLOWLOCATION, 1L);
    if (errorCode != CURLE_OK)
    {
      this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting follow location", errorCode);
      result = false;
    }

    if (errorCode == CURLE_OK)
    {
      if (this->httpDownloadRequest->GetReferer() != NULL)
      {
        char *curlReferer = ConvertToMultiByte(this->httpDownloadRequest->GetReferer());
        errorCode = curl_easy_setopt(this->curl, CURLOPT_REFERER, curlReferer);
        if (errorCode != CURLE_OK)
        {
          this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting referer", errorCode);
          result = false;
        }
        FREE_MEM(curlReferer);
      }
    }

    if (errorCode == CURLE_OK)
    {
      if (this->httpDownloadRequest->GetCookie() != NULL)
      {
        char *curlCookie = ConvertToMultiByte(this->httpDownloadRequest->GetCookie());
        errorCode = curl_easy_setopt(this->curl, CURLOPT_COOKIE, curlCookie);
        if (errorCode != CURLE_OK)
        {
          this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting cookie", errorCode);
          result = false;
        }
        FREE_MEM(curlCookie);
      }
      else if (this->cookies != NULL)
      {
        // set cookies in CURL instance to supplied cookies
        for (curl_slist *item = this->cookies; (result & (item != NULL)); item = item->next)
        {
          errorCode = curl_easy_setopt(this->curl, CURLOPT_COOKIELIST, item->data);
          if (errorCode != CURLE_OK)
          {
            this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting cookies", errorCode);
            result = false;
          }
        }
      }
      else
      {
        // set default cookie, initializes cookie engine
        errorCode = curl_easy_setopt(this->curl, CURLOPT_COOKIEFILE, "");
        if (errorCode != CURLE_OK)
        {
          this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting default cookie", errorCode);
          result = false;
        }
      }
    }

    if (errorCode == CURLE_OK)
    {
      if (this->httpDownloadRequest->GetUserAgent() != NULL)
      {
        char *curlUserAgent = ConvertToMultiByte(this->httpDownloadRequest->GetUserAgent());
        errorCode = curl_easy_setopt(this->curl, CURLOPT_USERAGENT, curlUserAgent);
        if (errorCode != CURLE_OK)
        {
          this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting user agent", errorCode);
          result = false;
        }
        FREE_MEM(curlUserAgent);
      }
    }

    if (errorCode == CURLE_OK)
    {
      switch (this->httpDownloadRequest->GetHttpVersion())
      {
      case HTTP_VERSION_NONE:
        {
          long version = CURL_HTTP_VERSION_NONE;
          errorCode = curl_easy_setopt(this->curl, CURLOPT_HTTP_VERSION , version);
        }
        break;
      case HTTP_VERSION_FORCE_HTTP10:
        {
          long version = CURL_HTTP_VERSION_1_0;
          errorCode = curl_easy_setopt(this->curl, CURLOPT_HTTP_VERSION , version);
        }
        break;
      case HTTP_VERSION_FORCE_HTTP11:
        {
          long version = CURL_HTTP_VERSION_1_1;
          errorCode = curl_easy_setopt(this->curl, CURLOPT_HTTP_VERSION , version);
        }
        break;
      }

      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting HTTP version", errorCode);
        result = false;
      }
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_IGNORE_CONTENT_LENGTH, this->httpDownloadRequest->GetIgnoreContentLength() ? 1L : 0L);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting ignore content length", errorCode);
        result = false;
      }
    }

    if (errorCode == CURLE_OK)
    {
      wchar_t *range = FormatString((this->httpDownloadRequest->GetEndPosition() <= this->httpDownloadRequest->GetStartPosition()) ? L"%llu-" : L"%llu-%llu", this->httpDownloadRequest->GetStartPosition(), this->httpDownloadRequest->GetEndPosition());
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: requesting range: %s", this->protocolName, METHOD_INITIALIZE_NAME, range);
      char *curlRange = ConvertToMultiByte(range);
      errorCode = curl_easy_setopt(this->curl, CURLOPT_RANGE, curlRange);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting range", errorCode);
        result = false;
      }
      FREE_MEM(curlRange);
      FREE_MEM(range);
    }

    if (errorCode == CURLE_OK)
    {
      this->ClearHeaders();
      for (unsigned int i = 0; i < this->httpDownloadRequest->GetHeaders()->Count(); i++)
      {
        this->AppendToHeaders(this->httpDownloadRequest->GetHeaders()->GetItem(i));
      }

      errorCode = curl_easy_setopt(this->curl, CURLOPT_HTTPHEADER, this->httpHeaders);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting HTTP headers", errorCode);
        result = false;
      }
    }
  }

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

size_t CHttpCurlInstance::CurlReceiveData(const unsigned char *buffer, size_t length)
{
  size_t result = __super::CurlReceiveData(buffer, length);
  if (result == length)
  {
    long responseCode = this->httpDownloadResponse->GetResponseCode();
    if ((responseCode != 0) && ((responseCode < 200) || (responseCode >= 400)))
    {
      // response code 200 - 299 = OK
      // response code 300 - 399 = redirect (OK)
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: error response code: %u", this->protocolName, METHOD_CURL_RECEIVE_DATA_NAME, responseCode);
      // return error
      result = 0;
    }
  }

  return result;
}

void CHttpCurlInstance::CurlDebug(curl_infotype type, const wchar_t *data)
{
  if (type == CURLINFO_HEADER_OUT)
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: sent HTTP header: '%s'", this->protocolName, METHOD_CURL_DEBUG_CALLBACK, data);
  }

  if (type == CURLINFO_HEADER_IN)
  {
    wchar_t *trimmed = Trim(data);
    // we are just interested in headers comming in from peer
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: received HTTP header: '%s'", this->protocolName, METHOD_CURL_DEBUG_NAME, (trimmed != NULL) ? trimmed : data);
    CHECK_CONDITION_NOT_NULL_EXECUTE(trimmed, this->httpDownloadResponse->GetHeaders()->Add(trimmed));
    FREE_MEM(trimmed);

    // check for accept-ranges header
    wchar_t *lowerBuffer = Duplicate(data);
    if (lowerBuffer != NULL)
    {
      size_t length = wcslen(lowerBuffer);
      if (length > 0)
      {
        _wcslwr_s(lowerBuffer, length + 1);

        if (length > 13)
        {
          // the length of received data should be at least 5 characters 'Accept-Ranges'

          if (wcsncmp(lowerBuffer, L"accept-ranges", 13) == 0)
          {
            // Accept-Ranges header, try to parse

            wchar_t *startString = wcsstr(lowerBuffer, L":");
            if (startString != NULL)
            {
              wchar_t *endString1 = wcsstr(startString, L"\n");
              wchar_t *endString2 = wcsstr(startString, L"\r");

              wchar_t *endString = NULL;
              if ((endString1 != NULL) && (endString2 != NULL))
              {
                endString = (endString1 < endString2) ? endString1 : endString2;
              }
              else if (endString1 != NULL)
              {
                endString = endString1;
              }
              else if (endString2 != NULL)
              {
                endString = endString2;
              }

              if (endString != NULL)
              {
                wchar_t *first = startString + 1;

                first = (wchar_t *)SkipBlanks(first);
                if (first != NULL)
                {
                  if (wcsncmp(first, L"none", 4) == 0)
                  {
                    // ranges are not supported
                    this->httpDownloadResponse->SetRangesSupported(false);
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}

bool CHttpCurlInstance::AppendToHeaders(CHttpHeader *header)
{
  bool result = (header != NULL) && (header->IsValid());
  if (result)
  {
    wchar_t *headerString = FormatString(L"%s: %s", header->GetName(), header->GetValue());
    result &= (headerString != NULL);
    if (result)
    {
      char *curlHeader = ConvertToMultiByteW(headerString);
      result &= (curlHeader != NULL);
      if (result)
      {
        this->httpHeaders = curl_slist_append(this->httpHeaders, curlHeader);
        result &= (this->httpHeaders != NULL);
      }
      FREE_MEM(curlHeader);
    }
    FREE_MEM(headerString);
  }

  return result;
}

void CHttpCurlInstance::ClearHeaders(void)
{
  curl_slist_free_all(this->httpHeaders);
  this->httpHeaders = NULL;
}

CHttpDownloadRequest *CHttpCurlInstance::GetHttpDownloadRequest(void)
{
  return this->httpDownloadRequest;
}

CHttpDownloadResponse *CHttpCurlInstance::GetHttpDownloadResponse(void)
{
  return this->httpDownloadResponse;
}

CDownloadResponse *CHttpCurlInstance::GetNewDownloadResponse(void)
{
  return new CHttpDownloadResponse();
}

double CHttpCurlInstance::GetDownloadContentLength(void)
{
  double result = -1;

  if (this->curl != NULL)
  {
    CURLcode errorCode = curl_easy_getinfo(this->curl, CURLINFO_CONTENT_LENGTH_DOWNLOAD, &result);
    result = (errorCode == CURLE_OK) ? result : (-1);
  }

  return result;
}

CParameterCollection *CHttpCurlInstance::GetCurrentCookies(void)
{
  CParameterCollection *result = new CParameterCollection();

  if (this->curl != NULL)
  {
    curl_slist *cookieList = NULL;
    CURLcode errorCode = curl_easy_getinfo(this->curl, CURLINFO_COOKIELIST, &cookieList);

    if ((errorCode == CURLE_OK) && (cookieList != NULL))
    {
      for (curl_slist *cookie = cookieList; cookie != NULL; cookie = cookie->next)
      {
        wchar_t *convertedValue = ConvertToUnicodeA(cookie->data);

        if (convertedValue != NULL)
        {
          CParameter *parameter = new CParameter(L"", convertedValue);

          if (!result->Add(parameter))
          {
            FREE_MEM_CLASS(parameter);
          }
        }
        FREE_MEM(convertedValue);
      }

      curl_slist_free_all(cookieList);
      cookieList = NULL;
    }
  }

  return result;
}

bool CHttpCurlInstance::SetCurrentCookies(CParameterCollection *cookies)
{
  bool result = (cookies != NULL);

  if (result)
  {
    // release previous cookies (if exist)
    if (this->cookies != NULL)
    {
      curl_slist_free_all(this->cookies);
      this->cookies = NULL;
    }

    // convert cookies collection to CURL list
    for (unsigned int i = 0; (result && (i < cookies->Count())); i++)
    {
      CParameter *cookie = cookies->GetItem(i);

      char *convertedValue = ConvertToMultiByteW(cookie->GetValue());
      result &= (convertedValue != NULL);

      if (result)
      {
        this->cookies = curl_slist_append(this->cookies, convertedValue);
        result &= (this->cookies != NULL);
      }
      FREE_MEM(convertedValue);
    }
  }

  return result;
}