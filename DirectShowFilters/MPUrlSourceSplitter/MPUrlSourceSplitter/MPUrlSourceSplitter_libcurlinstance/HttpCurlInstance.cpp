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
#include "HttpDumpBox.h"
#include "ErrorCodes.h"

CHttpCurlInstance::CHttpCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(result, logger, mutex, protocolName, instanceName)
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

/* get methods */

CHttpDownloadRequest *CHttpCurlInstance::GetHttpDownloadRequest(void)
{
  return this->httpDownloadRequest;
}

CHttpDownloadResponse *CHttpCurlInstance::GetHttpDownloadResponse(void)
{
  return this->httpDownloadResponse;
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
  HRESULT result = S_OK;
  CParameterCollection *currentCookies = new CParameterCollection(&result);
  CHECK_POINTER_HRESULT(result, currentCookies, result, E_OUTOFMEMORY);

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
          CParameter *parameter = new CParameter(&result, L"", convertedValue);
          CHECK_POINTER_HRESULT(result, parameter, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, currentCookies->CCollection::Add(parameter), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(parameter));
        }

        FREE_MEM(convertedValue);
      }

      curl_slist_free_all(cookieList);
      cookieList = NULL;
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(currentCookies));
  return currentCookies;
}

/* set methods */

bool CHttpCurlInstance::AddCookies(CParameterCollection *cookies)
{
  bool result = (cookies != NULL);

  if (result)
  {
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

/* other methods */

HRESULT CHttpCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->httpDownloadRequest = dynamic_cast<CHttpDownloadRequest *>(this->downloadRequest);
  this->httpDownloadResponse = dynamic_cast<CHttpDownloadResponse *>(this->downloadResponse);
  CHECK_POINTER_HRESULT(result, this->httpDownloadRequest, result, E_NOT_VALID_STATE);
  CHECK_POINTER_HRESULT(result, this->httpDownloadResponse, result, E_NOT_VALID_STATE);

  if (SUCCEEDED(result))
  {
    result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_FOLLOWLOCATION, 1L));
    CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting follow location: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));

    if (SUCCEEDED(result))
    {
      result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, FALSE));
      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting ignoring verifying peer: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      if (this->httpDownloadRequest->GetReferer() != NULL)
      {
        char *curlReferer = ConvertToMultiByte(this->httpDownloadRequest->GetReferer());
        CHECK_POINTER_HRESULT(result, curlReferer, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_REFERER, curlReferer)), result);

        FREE_MEM(curlReferer);
        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting referer: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }
    }

    if (SUCCEEDED(result))
    {
      if (this->httpDownloadRequest->GetCookie() != NULL)
      {
        char *curlCookie = ConvertToMultiByte(this->httpDownloadRequest->GetCookie());
        CHECK_POINTER_HRESULT(result, curlCookie, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_COOKIE, curlCookie)), result);

        FREE_MEM(curlCookie);
        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting cookie: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }
      else if (this->cookies != NULL)
      {
        // set cookies in CURL instance to supplied cookies
        for (curl_slist *item = this->cookies; (SUCCEEDED(result) & (item != NULL)); item = item->next)
        {
          result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_COOKIELIST, item->data));
        }

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting cookies: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }
      else
      {
        // set default cookie, initializes cookie engine
        result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_COOKIEFILE, ""));

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting default cookie: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }
    }

    if (SUCCEEDED(result))
    {
      if (this->httpDownloadRequest->GetUserAgent() != NULL)
      {
        char *curlUserAgent = ConvertToMultiByte(this->httpDownloadRequest->GetUserAgent());
        CHECK_POINTER_HRESULT(result, curlUserAgent, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_USERAGENT, curlUserAgent)), result);

        FREE_MEM(curlUserAgent);
        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting user agent: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }
    }

    if (SUCCEEDED(result))
    {
      switch (this->httpDownloadRequest->GetHttpVersion())
      {
      case HTTP_VERSION_NONE:
        {
          long version = CURL_HTTP_VERSION_NONE;
          result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_HTTP_VERSION , version));
        }
        break;
      case HTTP_VERSION_FORCE_HTTP10:
        {
          long version = CURL_HTTP_VERSION_1_0;
          result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_HTTP_VERSION , version));
        }
        break;
      case HTTP_VERSION_FORCE_HTTP11:
        {
          long version = CURL_HTTP_VERSION_1_1;
          result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_HTTP_VERSION , version));
        }
        break;
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting HTTP version: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_IGNORE_CONTENT_LENGTH, this->httpDownloadRequest->GetIgnoreContentLength() ? 1L : 0L));

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting ignore content length: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result) && this->httpDownloadRequest->GetStartPosition() > 0)
    {
      wchar_t *range = FormatString((this->httpDownloadRequest->GetEndPosition() <= this->httpDownloadRequest->GetStartPosition()) ? L"%llu-" : L"%llu-%llu", this->httpDownloadRequest->GetStartPosition(), this->httpDownloadRequest->GetEndPosition());
      CHECK_POINTER_HRESULT(result, range, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: requesting range: %s", this->protocolName, METHOD_INITIALIZE_NAME, range);

        char *curlRange = ConvertToMultiByte(range);
        CHECK_POINTER_HRESULT(result, curlRange, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_RANGE, curlRange)), result);
        FREE_MEM(curlRange);
      }

      FREE_MEM(range);
      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting range: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result) && (this->httpDownloadRequest->IsSetFlags(HTTP_DOWNLOAD_REQUEST_FLAG_AUTHENTICATE)))
    {
      // remote server authentication, user name and password

      if (SUCCEEDED(result))
      {
        char *curlUserName = ConvertToMultiByte(this->httpDownloadRequest->GetServerUserName());
        CHECK_POINTER_HRESULT(result, curlUserName, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_USERNAME, curlUserName)), result);
        FREE_MEM(curlUserName);

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting user name: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }

      if (SUCCEEDED(result))
      {
        char *curlPassword = ConvertToMultiByte(this->httpDownloadRequest->GetServerPassword());
        CHECK_POINTER_HRESULT(result, curlPassword, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_PASSWORD, curlPassword)), result);
        FREE_MEM(curlPassword);

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting password: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }

      if (SUCCEEDED(result))
      {
        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_HTTPAUTH, (long)CURLAUTH_ANY)), result);
        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting authentication: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }
    }

    if (SUCCEEDED(result) && (this->httpDownloadRequest->IsSetFlags(HTTP_DOWNLOAD_REQUEST_FLAG_PROXY_AUTHENTICATE)))
    {
      // proxy server

      if (SUCCEEDED(result))
      {
        char *proxyServer = ConvertToMultiByte(this->httpDownloadRequest->GetProxyServer());
        CHECK_POINTER_HRESULT(result, proxyServer, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_PROXY, proxyServer)), result);
        FREE_MEM(proxyServer);

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting proxy server: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }

      if (SUCCEEDED(result))
      {
        long proxyServerPort = this->httpDownloadRequest->GetProxyServerPort();
        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_PROXYPORT, proxyServerPort)), result);

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting proxy server port: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }

      if (SUCCEEDED(result))
      {
        char *proxyServerUserName = ConvertToMultiByte(this->httpDownloadRequest->GetProxyServerUserName());
        CHECK_POINTER_HRESULT(result, proxyServerUserName, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_PROXYUSERNAME, proxyServerUserName)), result);
        FREE_MEM(proxyServerUserName);

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting proxy server user name: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }

      if (SUCCEEDED(result))
      {
        char *proxyServerPassword = ConvertToMultiByte(this->httpDownloadRequest->GetProxyServerPassword());
        CHECK_POINTER_HRESULT(result, proxyServerPassword, result, E_CONVERT_STRING_ERROR);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_PROXYPASSWORD, proxyServerPassword)), result);
        FREE_MEM(proxyServerPassword);

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting proxy server password: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }

      if (SUCCEEDED(result))
      {
        long proxyServerType = CURLPROXY_HTTP;

        switch (this->httpDownloadRequest->GetProxyServerType())
        {
        case HTTP_PROXY_TYPE_HTTP:
          {
            proxyServerType = CURLPROXY_HTTP;
          }
          break;
        case HTTP_PROXY_TYPE_HTTP_1_0:
          {
            proxyServerType = CURLPROXY_HTTP_1_0;
          }
          break;
        case HTTP_PROXY_TYPE_SOCKS4:
          {
            proxyServerType = CURLPROXY_SOCKS4;
          }
          break;
        case HTTP_PROXY_TYPE_SOCKS5:
          {
            proxyServerType = CURLPROXY_SOCKS5;
          }
          break;
        case HTTP_PROXY_TYPE_SOCKS4A:
          {
            proxyServerType = CURLPROXY_SOCKS4A;
          }
          break;
        case HTTP_PROXY_TYPE_SOCKS5_HOSTNAME:
          {
            proxyServerType = CURLPROXY_SOCKS5_HOSTNAME;
          }
          break;
        default:
          {
            proxyServerType = CURLPROXY_HTTP;
          }
          break;
        }

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_PROXYTYPE, proxyServerType)), result);

        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting proxy server type: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }

      if (SUCCEEDED(result))
      {
        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_HTTPAUTH, (long)CURLAUTH_ANY)), result);
        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting proxy server authentication: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
      }
    }

    if (SUCCEEDED(result))
    {
      this->ClearHeaders();
      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->httpDownloadRequest->GetHeaders()->Count())); i++)
      {
        CHECK_CONDITION_HRESULT(result, this->AppendToHeaders(this->httpDownloadRequest->GetHeaders()->GetItem(i)), result, E_OUTOFMEMORY);
      }

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_HTTPHEADER, this->httpHeaders)), result);

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting HTTP headers: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }
  }

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

void CHttpCurlInstance::ClearSession(void)
{
  __super::ClearSession();

  if (this->cookies != NULL)
  {
    curl_slist_free_all(this->cookies);
    this->cookies = NULL;
  }

  this->ClearHeaders();

  this->httpDownloadRequest = NULL;
  this->httpDownloadResponse = NULL;
}

/* protected methods */

void CHttpCurlInstance::CurlDebug(curl_infotype type, const unsigned char *data, size_t size)
{
  __super::CurlDebug(type, data, size);

  if (type == CURLINFO_HEADER_OUT)
  {
    __super::CurlDebug(CURLINFO_DATA_OUT, data, size);

    wchar_t *curlData = CCurlInstance::ConvertTextData(data, size);
    if (curlData != NULL)
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: sent HTTP header: '%s'", this->protocolName, METHOD_CURL_DEBUG_CALLBACK, curlData);
    }
    FREE_MEM(curlData);
  }
  else if (type == CURLINFO_HEADER_IN)
  {
    __super::CurlDebug(CURLINFO_DATA_IN, data, size);

    wchar_t *curlData = CCurlInstance::ConvertTextData(data, size);
    wchar_t *trimmed = Trim(curlData);

    if ((trimmed != NULL) || (curlData != NULL))
    {
      // we are just interested in headers comming in from peer
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: received HTTP header: '%s'", this->protocolName, METHOD_CURL_DEBUG_NAME, (trimmed != NULL) ? trimmed : curlData);
      CHECK_CONDITION_NOT_NULL_EXECUTE(trimmed, this->httpDownloadResponse->GetHeaders()->Add(trimmed));
    }
    FREE_MEM(trimmed);

    // check for accept-ranges header
    wchar_t *lowerBuffer = Duplicate(curlData);
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

    FREE_MEM(lowerBuffer);
    FREE_MEM(curlData);

    {
      long previousResponseCode = this->httpDownloadResponse->GetResponseCode();
      long responseCode = 0;

      if (curl_easy_getinfo(this->curl, CURLINFO_RESPONSE_CODE, &responseCode) == CURLE_OK)
      {
        this->httpDownloadResponse->SetResponseCode(responseCode);
      }

      responseCode = this->httpDownloadResponse->GetResponseCode();

      if (this->httpDownloadResponse->GetLastUsedUrl() == NULL)
      {
        char *effectiveUrl = NULL;
        if (curl_easy_getinfo(this->curl, CURLINFO_EFFECTIVE_URL, &effectiveUrl) == CURLE_OK)
        {
          // convert effective URL and set it to download response
          wchar_t *lastUsedUrl = ConvertToUnicodeA(effectiveUrl);
          this->httpDownloadResponse->SetLastUsedUrl(lastUsedUrl);
          FREE_MEM(lastUsedUrl);
        }
      }

      if ((previousResponseCode == 0) && (responseCode != 0) && IS_RESPONSE_CODE_ERROR(responseCode))
      {
        // response code 200 - 299 = OK
        // response code 300 - 399 = redirect (OK)
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: error response code: %u", this->protocolName, METHOD_CURL_DEBUG_NAME, responseCode);
      }
    }
  }
}

size_t CHttpCurlInstance::CurlReceiveData(const unsigned char *buffer, size_t length)
{
  size_t result = __super::CurlReceiveData(buffer, length);
  if (result == length)
  {
    long responseCode;
    if (curl_easy_getinfo(this->curl, CURLINFO_RESPONSE_CODE, &responseCode) == CURLE_OK)
    {
      this->httpDownloadResponse->SetResponseCode(responseCode);
    }

    responseCode = this->httpDownloadResponse->GetResponseCode();

    char *effectiveUrl = NULL;
    if (curl_easy_getinfo(this->curl, CURLINFO_EFFECTIVE_URL, &effectiveUrl) == CURLE_OK)
    {
      // convert effective URL and set it to download response
      wchar_t *lastUsedUrl = ConvertToUnicodeA(effectiveUrl);
      this->httpDownloadResponse->SetLastUsedUrl(lastUsedUrl);
      FREE_MEM(lastUsedUrl);
    }
                 
    if ((responseCode != 0) && IS_RESPONSE_CODE_ERROR(responseCode))
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

CDownloadResponse *CHttpCurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CHttpDownloadResponse *response = new CHttpDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

HRESULT CHttpCurlInstance::DestroyCurlWorker(void)
{
  HRESULT result = __super::DestroyCurlWorker();

  if (this->httpDownloadResponse != NULL)
  {
    long responseCode;
    if ((this->httpDownloadResponse->GetResponseCode() == 0) && (curl_easy_getinfo(this->curl, CURLINFO_RESPONSE_CODE, &responseCode) == CURLE_OK))
    {
      this->httpDownloadResponse->SetResponseCode(responseCode);
    }
  }

  return result;
}

CDumpBox *CHttpCurlInstance::CreateDumpBox(void)
{
  HRESULT result = S_OK;
  CHttpDumpBox *box = new CHttpDumpBox(&result);
  CHECK_POINTER_HRESULT(result, box, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(box));
  return box;
}