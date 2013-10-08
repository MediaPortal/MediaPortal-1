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

#include "F4MBootstrapInfo.h"

#include "base64.h"
#include "formatUrl.h"
#include "HttpCurlInstance.h"

CF4MBootstrapInfo::CF4MBootstrapInfo(void)
{
  this->id = NULL;
  this->profile = NULL;
  this->url = NULL;
  this->value = NULL;

  this->decodedLength = UINT_MAX;
  this->decodedValue = NULL;
  this->decodeResult = E_NOT_VALID_STATE;
  this->baseUrl = NULL;
}

CF4MBootstrapInfo::~CF4MBootstrapInfo(void)
{
  FREE_MEM(this->id);
  FREE_MEM(this->profile);
  FREE_MEM(this->url);
  FREE_MEM(this->value);

  FREE_MEM(this->decodedValue);
  FREE_MEM(this->baseUrl);
}

/* get methods */

const wchar_t *CF4MBootstrapInfo::GetId(void)
{
  return this->id;
}

const wchar_t *CF4MBootstrapInfo::GetProfile(void)
{
  return this->profile;
}

 const wchar_t *CF4MBootstrapInfo::GetUrl(void)
 {
   return this->url;
 }

const wchar_t *CF4MBootstrapInfo::GetValue(void)
{
  return this->value;
}

HRESULT CF4MBootstrapInfo::GetDecodeResult(void)
{
  FREE_MEM(this->decodedValue);
  HRESULT result = this->decodeResult;

  if ((this->value != NULL) && (result == E_NOT_VALID_STATE))
  {
    // no conversion occured until now
    result = S_OK;

    char *val = ConvertToMultiByteW(this->value);
    CHECK_POINTER_HRESULT(result, val, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      result = base64_decode(val, &this->decodedValue, &this->decodedLength);
    }

    FREE_MEM(val);
    if (FAILED(result))
    {
      this->decodedLength = UINT_MAX;
    }
  }

  return result;
}

const unsigned char *CF4MBootstrapInfo::GetDecodedValue(void)
{
  return this->decodedValue;
}

unsigned int CF4MBootstrapInfo::GetDecodedValueLength(void)
{
  return this->decodedLength;
}

const wchar_t *CF4MBootstrapInfo::GetBaseUrl(void)
{
  return this->baseUrl;
}

/* set methods */

bool CF4MBootstrapInfo::SetId(const wchar_t *id)
{
  SET_STRING_RETURN_WITH_NULL(this->id, id);
}

bool CF4MBootstrapInfo::SetProfile(const wchar_t *profile)
{
  SET_STRING_RETURN_WITH_NULL(this->profile, profile);
}

bool CF4MBootstrapInfo::SetUrl(const wchar_t *url)
{
  SET_STRING_RETURN_WITH_NULL(this->url, url);
}

bool CF4MBootstrapInfo::SetValue(const wchar_t *value)
{
  SET_STRING_RETURN_WITH_NULL(this->value, value);
}

bool CF4MBootstrapInfo::SetBaseUrl(const wchar_t *baseUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->baseUrl, baseUrl);
}

/* other methods */

bool CF4MBootstrapInfo::IsValid(void)
{
  return ((this->id != NULL) && (this->profile != NULL) && (((this->url != NULL) && (this->value == NULL)) || ((this->url == NULL) && (this->value != NULL))));
}

bool CF4MBootstrapInfo::HasUrl(void)
{
  return (this->url != NULL);
}

bool CF4MBootstrapInfo::HasValue(void)
{
  return (this->value != NULL);
}

HRESULT CF4MBootstrapInfo::DownloadBootstrapInfo(CLogger *logger, const wchar_t *protocolName, unsigned int finishTime, const wchar_t *referer, const wchar_t *userAgent, const wchar_t *cookie, CParameterCollection *cookies, const wchar_t *networkInterfaceName)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, logger);
  CHECK_POINTER_DEFAULT_HRESULT(result, protocolName);

  if (SUCCEEDED(result))
  {
    wchar_t *bootstrapInfoUrl = FormatAbsoluteUrl(this->baseUrl, this->url);
    CHECK_POINTER_HRESULT(result, bootstrapInfoUrl, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      CHttpCurlInstance *curlInstance = new CHttpCurlInstance(logger, NULL, protocolName, L"CF4MBootstrapInfo");
      CHECK_POINTER_HRESULT(result, curlInstance, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        curlInstance->SetFinishTime(finishTime);
        curlInstance->SetNetworkInterfaceName(networkInterfaceName);

        CHttpDownloadRequest *request = new CHttpDownloadRequest();
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result) && (cookies != NULL))
        {
          result = (curlInstance->SetCurrentCookies(cookies)) ? result : E_FAIL;
        }

        if (SUCCEEDED(result))
        {
          request->SetUrl(bootstrapInfoUrl);
          request->SetReferer(referer);
          request->SetUserAgent(userAgent);
          request->SetCookie(cookie);

          result = (curlInstance->Initialize(request)) ? S_OK : E_FAIL;
        }
        FREE_MEM_CLASS(request);
      }

      if (SUCCEEDED(result))
      {
        // all parameters set
        // start receiving data

        result = (curlInstance->StartReceivingData()) ? S_OK : E_FAIL;
      }

      if (SUCCEEDED(result))
      {
        // wait for HTTP status code

        long responseCode = curlInstance->GetHttpDownloadResponse()->GetResponseCode();
        while (responseCode == 0)
        {
          responseCode = curlInstance->GetHttpDownloadResponse()->GetResponseCode();
          if ((responseCode != 0) && ((responseCode < 200) || (responseCode >= 400)))
          {
            // response code 200 - 299 = OK
            // response code 300 - 399 = redirect (OK)
            result = E_FAIL;
          }

          if ((responseCode == 0) && (curlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
          {
            // we received data too fast
            result = E_FAIL;
            break;
          }

          // wait some time
          Sleep(1);
        }

        if (SUCCEEDED(result))
        {
          // wait until all data are received
          while (curlInstance->GetCurlState() != CURL_STATE_RECEIVED_ALL_DATA)
          {
            // sleep some time
            Sleep(10);
          }

          // copy cookies from CURL instance to passed cookies
          CParameterCollection *currentCookies = curlInstance->GetCurrentCookies();
          if (SUCCEEDED(result) && (currentCookies != NULL))
          {
            cookies->Clear();
            for (unsigned int i = 0; (SUCCEEDED(result) && (i < currentCookies->Count())); i++)
            {
              CParameter *clone = currentCookies->GetItem(i)->Clone();
              CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                result = (cookies->Add(clone)) ? result : E_OUTOFMEMORY;
              }

              if (FAILED(result))
              {
                FREE_MEM_CLASS(clone);
              }
            }
          }
          FREE_MEM_CLASS(currentCookies);

          result = (curlInstance->GetHttpDownloadResponse()->GetResultCode() != CURLE_OK) ? HRESULT_FROM_WIN32(ERROR_INVALID_DATA) : result;
        }

        if (SUCCEEDED(result))
        {
          // copy received data for parsing
          FREE_MEM(this->decodedValue);
          this->decodedLength = curlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
          this->decodedValue = ALLOC_MEM_SET(this->decodedValue, unsigned char, this->decodedLength, 0);
          CHECK_POINTER_HRESULT(result, this->decodedValue, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            curlInstance->GetHttpDownloadResponse()->GetReceivedData()->CopyFromBuffer(this->decodedValue, this->decodedLength);

            char *base64EncodedValue = NULL;
            result = base64_encode(this->decodedValue, this->decodedLength, &base64EncodedValue);
            if (SUCCEEDED(result))
            {
              FREE_MEM(this->value);
              this->value = ConvertToUnicodeA(base64EncodedValue);
              CHECK_POINTER_HRESULT(result, this->value, result, E_OUTOFMEMORY);
            }
            FREE_MEM(base64EncodedValue);
          }

          this->decodeResult = E_NOT_VALID_STATE;
          this->decodedLength = 0;
          FREE_MEM(this->decodedValue);
        }
      }

      FREE_MEM_CLASS(curlInstance);
    }

    FREE_MEM(bootstrapInfoUrl);
  }

  return result;
}