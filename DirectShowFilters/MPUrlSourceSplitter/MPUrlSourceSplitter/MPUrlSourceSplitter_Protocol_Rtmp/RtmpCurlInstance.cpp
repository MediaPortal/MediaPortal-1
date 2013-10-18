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

#include "RtmpCurlInstance.h"
#include "conversions.h"

#include <librtmp/log.h>

CRtmpCurlInstance::CRtmpCurlInstance(CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(logger, mutex, protocolName, instanceName)
{
  this->rtmpDownloadRequest = dynamic_cast<CRtmpDownloadRequest *>(this->downloadRequest);
  this->rtmpDownloadResponse = dynamic_cast<CRtmpDownloadResponse *>(this->downloadResponse);

  this->duration = RTMP_DURATION_UNSPECIFIED;
}

CRtmpCurlInstance::~CRtmpCurlInstance(void)
{
}

bool CRtmpCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  bool result = __super::Initialize(downloadRequest);

  this->duration = RTMP_DURATION_UNSPECIFIED;
  this->rtmpDownloadRequest = dynamic_cast<CRtmpDownloadRequest  *>(this->downloadRequest);
  this->rtmpDownloadResponse = dynamic_cast<CRtmpDownloadResponse *>(this->downloadResponse);
  result &= (this->rtmpDownloadRequest != NULL) && (this->rtmpDownloadResponse != NULL);

  if (result)
  {
    CURLcode errorCode = CURLE_OK;
    if (errorCode == CURLE_OK)
    {
      // librtmp needs url in specific format
      // timeout for RTMP protocol is set through libcurl options

      wchar_t *connectionString = Duplicate(this->rtmpDownloadRequest->GetUrl());

      if (this->rtmpDownloadRequest->GetRtmpApp() != RTMP_APP_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_APP, this->rtmpDownloadRequest->GetRtmpApp(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpArbitraryData() != RTMP_ARBITRARY_DATA_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, this->rtmpDownloadRequest->GetRtmpArbitraryData());
      }
      if (this->rtmpDownloadRequest->GetRtmpBuffer() != RTMP_BUFFER_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_BUFFER, this->rtmpDownloadRequest->GetRtmpBuffer());
      }
      if (this->rtmpDownloadRequest->GetRtmpFlashVersion() != RTMP_FLASH_VER_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_FLASHVER, this->rtmpDownloadRequest->GetRtmpFlashVersion(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpAuth() != RTMP_AUTH_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_AUTH, this->rtmpDownloadRequest->GetRtmpAuth(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpJtv() != RTMP_JTV_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_JTV, this->rtmpDownloadRequest->GetRtmpJtv(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpLive() != RTMP_LIVE_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_LIVE, this->rtmpDownloadRequest->GetRtmpLive() ? L"1" : L"0", true);
      }
      if (this->rtmpDownloadRequest->GetRtmpPageUrl() != RTMP_PAGE_URL_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_PAGE_URL, this->rtmpDownloadRequest->GetRtmpPageUrl(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpPlaylist() != RTMP_PLAYLIST_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_PLAYLIST, this->rtmpDownloadRequest->GetRtmpPlaylist() ? L"1" : L"0", true);
      }
      if (this->rtmpDownloadRequest->GetRtmpPlayPath() != RTMP_PLAY_PATH_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_PLAY_PATH, this->rtmpDownloadRequest->GetRtmpPlayPath(), true);
      }
      // always add start token
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_START, this->rtmpDownloadRequest->GetRtmpStart());
      }
      if (this->rtmpDownloadRequest->GetRtmpStop() != RTMP_STOP_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_STOP, this->rtmpDownloadRequest->GetRtmpStop());
      }
      if (this->rtmpDownloadRequest->GetRtmpSubscribe() != RTMP_SUBSCRIBE_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_SUBSCRIBE, this->rtmpDownloadRequest->GetRtmpSubscribe(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpSwfUrl() != RTMP_SWF_URL_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_SWF_URL, this->rtmpDownloadRequest->GetRtmpSwfUrl(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpSwfVerify() != RTMP_SWF_VERIFY_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_SWF_VERIFY, this->rtmpDownloadRequest->GetRtmpSwfVerify() ? L"1" : L"0", true);
      }
      if (this->rtmpDownloadRequest->GetRtmpTcUrl() != RTMP_TC_URL_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_TC_URL, this->rtmpDownloadRequest->GetRtmpTcUrl(), true);
      }
      if (this->rtmpDownloadRequest->GetRtmpToken() != RTMP_TOKEN_DEFAULT)
      {
        this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_TOKEN, this->rtmpDownloadRequest->GetRtmpToken() , true);
      }

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: librtmp connection string: %s", this->protocolName, METHOD_INITIALIZE_NAME, connectionString);
      
      char *curlUrl = ConvertToMultiByte(connectionString);
      errorCode = curl_easy_setopt(this->curl, CURLOPT_URL, curlUrl);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting url", errorCode);
        result = false;
      }
      FREE_MEM(curlUrl);
      FREE_MEM(connectionString);
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_RTMP_LOG_CALLBACK, &CRtmpCurlInstance::RtmpLogCallback);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting RTMP protocol log callback", errorCode);
        result = false;
      }
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_RTMP_LOG_USERDATA, this);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting RTMP protocol log callback user data", errorCode);
        result = false;
      }
    }
  }

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

void CRtmpCurlInstance::RtmpLogCallback(RTMP *r, int level, const char *format, va_list vl)
{
  CRtmpCurlInstance *caller = (CRtmpCurlInstance *)r->m_logUserData;

  int length = _vscprintf(format, vl) + 1;
  ALLOC_MEM_DEFINE_SET(buffer, char, length, 0);
  if (buffer != NULL)
  {
    vsprintf_s(buffer, length, format, vl);
  }

  // convert buffer to wchar_t
  wchar_t *convertedBuffer = ConvertToUnicodeA(buffer);

  unsigned int loggerLevel = UINT_MAX;

  switch (level)
  {
  case RTMP_LOGCRIT:
  case RTMP_LOGERROR:
    loggerLevel = LOGGER_ERROR;
    break;
  case RTMP_LOGWARNING:
    loggerLevel = LOGGER_WARNING;
    break;
  case RTMP_LOGINFO:
    loggerLevel = LOGGER_INFO;
    if ((caller->duration == RTMP_DURATION_UNSPECIFIED) && (convertedBuffer != NULL))
    {
      // duration is not set
      // parse message, if not contain duration
      int index = IndexOf(convertedBuffer, RTMP_RESPONSE_DURATION);
      if (index != (-1))
      {
        // duration tag found
        double val = GetValueDouble(convertedBuffer + index + RTMP_RESPONSE_DURATION_LENGTH, -1);
        if (val != (-1))
        {
          caller->duration = (uint64_t)(val * 1000);
        }
      }
    }
    break;
  case RTMP_LOGDEBUG:
    loggerLevel = (caller->state == CURL_STATE_RECEIVING_DATA) ? UINT_MAX : LOGGER_VERBOSE;
    break;
  case RTMP_LOGDEBUG2:
    loggerLevel = UINT_MAX;
    break;
  default:
    loggerLevel = LOGGER_NONE;
    break;
  }

  caller->logger->Log(loggerLevel, L"%s: %s: %s", caller->protocolName, L"RtmpLogCallback()", convertedBuffer);

  FREE_MEM(convertedBuffer);
  FREE_MEM(buffer);
}

wchar_t *CRtmpCurlInstance::EncodeString(const wchar_t *string)
{
  wchar_t *replacedString = ReplaceString(string, L"\\", L"\\5c");
  wchar_t *replacedString2 = ReplaceString(replacedString, L" ", L"\\20");
  FREE_MEM(replacedString);
  return replacedString2;
}

wchar_t *CRtmpCurlInstance::CreateRtmpParameter(const wchar_t *name, const wchar_t *value)
{
  if ((name == NULL) || (value == NULL))
  {
    return NULL;
  }
  else
  {
    return FormatString(L"%s=%s", name, value);
  }
}

wchar_t *CRtmpCurlInstance::CreateRtmpEncodedParameter(const wchar_t *name, const wchar_t *value)
{
  wchar_t *result = NULL;
  wchar_t *encodedValue = this->EncodeString(value);
  if (encodedValue != NULL)
  {
    result = this->CreateRtmpParameter(name, encodedValue);
  }
  FREE_MEM(encodedValue);

  return result;
}

bool CRtmpCurlInstance::AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *name, unsigned int value)
{
  wchar_t *formattedValue = FormatString(L"%u", value);
  bool result = this->AddToRtmpConnectionString(connectionString, name, formattedValue, false);
  FREE_MEM(formattedValue);
  return result;
}

bool CRtmpCurlInstance::AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *name, int64_t value)
{
  wchar_t *formattedValue = FormatString(L"%lld", value);
  bool result = this->AddToRtmpConnectionString(connectionString, name, formattedValue, false);
  FREE_MEM(formattedValue);
  return result;
}

bool CRtmpCurlInstance::AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *name, const wchar_t *value, bool encode)
{
  if ((connectionString == NULL) || (*connectionString == NULL) || (name == NULL) || (value == NULL))
  {
    return false;
  }

  wchar_t *temp = (encode) ? this->CreateRtmpEncodedParameter(name, value) : this->CreateRtmpParameter(name, value);
  bool result = this->AddToRtmpConnectionString(connectionString, temp);
  FREE_MEM(temp);

  return result;
}

bool CRtmpCurlInstance::AddToRtmpConnectionString(wchar_t **connectionString, const wchar_t *string)
{
  if ((connectionString == NULL) || (*connectionString == NULL) || (string == NULL))
  {
    return false;
  }

  wchar_t *temp = FormatString(L"%s %s", *connectionString, string);
  FREE_MEM(*connectionString);

  *connectionString = temp;

  return (*connectionString != NULL);
}

void CRtmpCurlInstance::CurlDebug(curl_infotype type, const wchar_t *data)
{
}

CRtmpDownloadResponse *CRtmpCurlInstance::GetRtmpDownloadResponse(void)
{
  return this->rtmpDownloadResponse;
}

CDownloadResponse *CRtmpCurlInstance::GetNewDownloadResponse(void)
{
  return new CRtmpDownloadResponse();
}

uint64_t CRtmpCurlInstance::GetDuration(void)
{
  return this->duration;
}