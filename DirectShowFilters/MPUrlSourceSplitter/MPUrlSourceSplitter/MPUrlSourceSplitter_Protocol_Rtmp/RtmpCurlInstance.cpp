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
#include "RtmpDumpBox.h"
#include "LockMutex.h"
#include "conversions.h"
#include "ErrorCodes.h"

CRtmpCurlInstance::CRtmpCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(result, logger, mutex, protocolName, instanceName)
{
  this->rtmpDownloadRequest = dynamic_cast<CRtmpDownloadRequest *>(this->downloadRequest);
  this->rtmpDownloadResponse = dynamic_cast<CRtmpDownloadResponse *>(this->downloadResponse);
  this->rtmp = NULL;
  this->librtmpUrl = NULL;

  this->SetWriteCallback(CRtmpCurlInstance::CurlReceiveDataCallback, this);
}

CRtmpCurlInstance::~CRtmpCurlInstance(void)
{
  this->StopReceivingData();

  FREE_MEM(this->librtmpUrl);
}

/* get methods */

CRtmpDownloadRequest *CRtmpCurlInstance::GetRtmpDownloadRequest(void)
{
  return this->rtmpDownloadRequest;
}

CRtmpDownloadResponse *CRtmpCurlInstance::GetRtmpDownloadResponse(void)
{
  return this->rtmpDownloadResponse;
}

/* set methods */

/* other methods */

HRESULT CRtmpCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->rtmpDownloadRequest = dynamic_cast<CRtmpDownloadRequest  *>(this->downloadRequest);
  this->rtmpDownloadResponse = dynamic_cast<CRtmpDownloadResponse *>(this->downloadResponse);

  CHECK_POINTER_HRESULT(result, this->rtmpDownloadRequest, result, E_NOT_VALID_STATE);
  CHECK_POINTER_HRESULT(result, this->rtmpDownloadResponse, result, E_NOT_VALID_STATE);

  if (SUCCEEDED(result))
  {
    // librtmp needs url in specific format, we must format it

    wchar_t *connectionString = Duplicate(this->rtmpDownloadRequest->GetUrl());

    if (this->rtmpDownloadRequest->GetRtmpApp() != RTMP_APP_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_APP, this->rtmpDownloadRequest->GetRtmpApp(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpArbitraryData() != RTMP_ARBITRARY_DATA_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, this->rtmpDownloadRequest->GetRtmpArbitraryData()), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpBuffer() != RTMP_BUFFER_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_BUFFER, this->rtmpDownloadRequest->GetRtmpBuffer()), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpFlashVersion() != RTMP_FLASH_VER_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_FLASHVER, this->rtmpDownloadRequest->GetRtmpFlashVersion(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpAuth() != RTMP_AUTH_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_AUTH, this->rtmpDownloadRequest->GetRtmpAuth(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpJtv() != RTMP_JTV_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_JTV, this->rtmpDownloadRequest->GetRtmpJtv(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpLive() != RTMP_LIVE_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_LIVE, this->rtmpDownloadRequest->GetRtmpLive() ? L"1" : L"0", true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpPageUrl() != RTMP_PAGE_URL_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_PAGE_URL, this->rtmpDownloadRequest->GetRtmpPageUrl(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpPlaylist() != RTMP_PLAYLIST_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_PLAYLIST, this->rtmpDownloadRequest->GetRtmpPlaylist() ? L"1" : L"0", true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpPlayPath() != RTMP_PLAY_PATH_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_PLAY_PATH, this->rtmpDownloadRequest->GetRtmpPlayPath(), true), result, E_OUTOFMEMORY);
    }
    // always add start token
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_START, this->rtmpDownloadRequest->GetRtmpStart()), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpStop() != RTMP_STOP_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_STOP, this->rtmpDownloadRequest->GetRtmpStop()), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpSubscribe() != RTMP_SUBSCRIBE_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_SUBSCRIBE, this->rtmpDownloadRequest->GetRtmpSubscribe(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpSwfUrl() != RTMP_SWF_URL_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_SWF_URL, this->rtmpDownloadRequest->GetRtmpSwfUrl(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpSwfVerify() != RTMP_SWF_VERIFY_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_SWF_VERIFY, this->rtmpDownloadRequest->GetRtmpSwfVerify() ? L"1" : L"0", true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpTcUrl() != RTMP_TC_URL_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_TC_URL, this->rtmpDownloadRequest->GetRtmpTcUrl(), true), result, E_OUTOFMEMORY);
    }
    if (this->rtmpDownloadRequest->GetRtmpToken() != RTMP_TOKEN_DEFAULT)
    {
      CHECK_CONDITION_HRESULT(result, this->AddToRtmpConnectionString(&connectionString, RTMP_TOKEN_TOKEN, this->rtmpDownloadRequest->GetRtmpToken() , true), result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: librtmp connection string: %s", this->protocolName, METHOD_INITIALIZE_NAME, connectionString);

      this->librtmpUrl = ConvertToMultiByte(connectionString);
      CHECK_POINTER_HRESULT(result, this->librtmpUrl, result, E_CONVERT_STRING_ERROR);

      if (SUCCEEDED(result))
      {
        this->rtmp = RTMP_Alloc();
        CHECK_POINTER_HRESULT(result, this->rtmp, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          RTMP_Init(this->rtmp);

          // set up logger and dump raw data callback
          this->rtmp->m_logUserData = this;
          this->rtmp->m_logCallback = &CRtmpCurlInstance::RtmpLogCallback;
          this->rtmp->m_dumpRawDataCallback = &CRtmpCurlInstance::RtmpDumpRawDataCallback;

          RTMP_SetBufferMS(this->rtmp, DEF_BUFTIME);

          if (!RTMP_SetupURL(this->rtmp, this->librtmpUrl))
          {
            RTMP_Free(this->rtmp);
            this->rtmp = NULL;

            result = E_RTMP_BAD_URL_FORMAT;
          }
        }
      }
    }
  }

  if (SUCCEEDED(result))
  {
    unsigned int endTicks = (this->downloadRequest->GetFinishTime() == FINISH_TIME_NOT_SPECIFIED) ? (GetTickCount() + this->downloadRequest->GetReceiveDataTimeout()) : this->downloadRequest->GetFinishTime();

    // we have own RTMP implementation, using CURL only to connect, send and receive data

    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_CONNECT_ONLY, 1L)), result);
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_perform(this->curl)), result);

    if (SUCCEEDED(result))
    {
      curl_socket_t socket = CURL_SOCKET_BAD;
      curl_easy_getinfo(this->curl, CURLINFO_LASTSOCKET, &socket);
    
      CHECK_CONDITION_HRESULT(result, socket != CURL_SOCKET_BAD, result, E_NOT_VALID_STATE);

      if (SUCCEEDED(result))
      {
        // next code (until closing bracket) is taken and rewritten from curl_rtmp.c
        this->rtmp->m_sb.sb_socket = socket;

        // for plain streams, use the buffer toggle trick to keep data flowing

        if ((!(this->rtmp->Link.lFlags & RTMP_LF_LIVE)) && (!(this->rtmp->Link.protocol & RTMP_FEATURE_HTTP)))
        {
          this->rtmp->Link.lFlags |= RTMP_LF_BUFX;
        }

        // librtmp is written for blocking sockets :(
        unsigned long flags = 0UL;
        ioctlsocket(this->rtmp->m_sb.sb_socket, FIONBIO, &flags);

        int timeout = max(endTicks - GetTickCount(), 1);
        this->logger->Log(LOGGER_INFO, L"%s: %s: socket timeout: %d (ms)", this->protocolName, METHOD_INITIALIZE_NAME, timeout);
        if (setsockopt(this->rtmp->m_sb.sb_socket, SOL_SOCKET, SO_RCVTIMEO, (char *)&timeout, sizeof(timeout)) == SOCKET_ERROR)
        {
          this->logger->Log(LOGGER_WARNING, L"%s: %s: error while setting timeout on socket: %d", this->protocolName, METHOD_INITIALIZE_NAME, WSAGetLastError());
        }

        int on = 1;
        if (setsockopt(this->rtmp->m_sb.sb_socket, IPPROTO_TCP, TCP_NODELAY, (char *) &on, sizeof(on)) == SOCKET_ERROR)
        {
          this->logger->Log(LOGGER_WARNING, L"%s: %s: error while setting TCP no delay on socket: %d", this->protocolName, METHOD_INITIALIZE_NAME, WSAGetLastError());
        }

        CHECK_CONDITION_HRESULT(result, RTMP_Connect1(this->rtmp, NULL), result, E_RTMP_CONNECT_FAILED);

        if (SUCCEEDED(result))
        {
          // clients must send a periodic BytesReceived report to the server
          this->rtmp->m_bSendCounter = true;
        }

        CHECK_CONDITION_HRESULT(result, RTMP_ConnectStream(this->rtmp, 0), result, E_RTMP_CONNECT_STREAM_FAILED);
        
        if (FAILED(result))
        {
          RTMP_Close(this->rtmp);
          RTMP_Free(this->rtmp);

          this->rtmp = NULL;
        }

        if (SUCCEEDED(result))
        {
          this->state = CURL_STATE_RECEIVING_DATA;
          this->lastReceiveTime = GetTickCount();
        }
      }
    }
  }

  this->state = SUCCEEDED(result) ? CURL_STATE_RECEIVING_DATA : CURL_STATE_CREATED;
  return result;
}

HRESULT CRtmpCurlInstance::StopReceivingData(void)
{
  // CurlInstance::StopReceivingData returns always S_OK
  __super::StopReceivingData();

  if (this->rtmp != NULL)
  {
    RTMP_Close(this->rtmp);
    RTMP_Free(this->rtmp);

    this->rtmp = NULL;
  }

  return S_OK;
}

/* protected methods */

CDownloadResponse *CRtmpCurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CRtmpDownloadResponse *response = new CRtmpDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

unsigned int CRtmpCurlInstance::CurlWorker(void)
{
  this->logger->Log(LOGGER_INFO, L"%s: %s: Start, url: '%s'", this->protocolName, METHOD_CURL_WORKER_NAME, this->downloadRequest->GetUrl());
  this->startReceivingTicks = GetTickCount();
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;

  HRESULT result = S_OK;
  int readBytes = 0;

  ALLOC_MEM_DEFINE_SET(buffer, char, RTMP_READ_BUFFER_SIZE, 0);
  CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

  while (!this->curlWorkerShouldExit)
  {
    readBytes = 0;

    if ((this->state == CURL_STATE_RECEIVING_DATA) && (this->rtmp != NULL))
    {
      readBytes = RTMP_Read(this->rtmp, buffer, RTMP_READ_BUFFER_SIZE);

      if (readBytes > 0)
      {
        // we must add read data to RTMP response

        // only one thread can work with RTMP data in one time
        LOCK_MUTEX(this->mutex, INFINITE)

        CHECK_CONDITION_HRESULT(result, this->rtmpDownloadResponse->GetReceivedData()->AddToBufferWithResize((unsigned char *)buffer, readBytes) == readBytes, result, E_OUTOFMEMORY);

        this->totalReceivedBytes += readBytes;
        this->lastReceiveTime = GetTickCount();

        UNLOCK_MUTEX(this->mutex)
      }
      else if ((readBytes < 0) || (this->rtmp->m_read.status == RTMP_READ_COMPLETE) || (this->rtmp->m_read.status == RTMP_READ_EOF))
      {
        // error occured or end of stream reached
        if ((this->rtmp->m_read.status == RTMP_READ_COMPLETE) || (this->rtmp->m_read.status == RTMP_READ_EOF))
        {
          readBytes = 0;
        }

        LOCK_MUTEX(this->mutex, INFINITE)

        this->rtmpDownloadResponse->SetResultError((readBytes < 0) ? E_RTMP_GENERAL_READ_ERROR : S_OK);
        this->state = CURL_STATE_RECEIVED_ALL_DATA;

        UNLOCK_MUTEX(this->mutex)
      }
    }

    if (FAILED(result) && (this->state != CURL_STATE_RECEIVED_ALL_DATA))
    {
      // we have some error, we can't do more
      // report error code and wait for destroying CURL instance

      LOCK_MUTEX(this->mutex, INFINITE)

      this->rtmpDownloadResponse->SetResultError(result);
      this->state = CURL_STATE_RECEIVED_ALL_DATA;

      UNLOCK_MUTEX(this->mutex)
    }

    Sleep(1);
  }

  FREE_MEM(buffer);

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while sending, receiving or processing data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, result));
  this->rtmpDownloadResponse->SetResultError(result);

  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: received bytes: %lld", this->protocolName, METHOD_CURL_WORKER_NAME, this->totalReceivedBytes);

  this->state = CURL_STATE_RECEIVED_ALL_DATA;
  this->stopReceivingTicks = GetTickCount();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->protocolName, METHOD_CURL_WORKER_NAME);
  return S_OK;
}

CDumpBox *CRtmpCurlInstance::CreateDumpBox(void)
{
  HRESULT result = S_OK;
  CRtmpDumpBox *box = new CRtmpDumpBox(&result);
  CHECK_POINTER_HRESULT(result, box, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(box));
  return box;
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
    {
      loggerLevel = LOGGER_INFO;

      if ((caller->rtmpDownloadResponse->GetDuration() == RTMP_DURATION_UNSPECIFIED) && (convertedBuffer != NULL))
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
            caller->rtmpDownloadResponse->SetDuration((uint64_t)(val * 1000));
          }
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

void CRtmpCurlInstance::RtmpDumpRawDataCallback(struct RTMP *r, char *buffer, int length)
{
  if ((r != NULL) && (buffer != NULL) && (length > 0))
  {
    CRtmpCurlInstance *caller = (CRtmpCurlInstance *)r->m_logUserData;

    CDumpBox *dumpBox = NULL;
    CHECK_CONDITION_NOT_NULL_EXECUTE(caller->dumpFile->GetDumpFile(), dumpBox = caller->CreateDumpBox());

    if (dumpBox != NULL)
    {
      dumpBox->SetTimeWithLocalTime();
      dumpBox->SetPayload((uint8_t *)buffer, (uint32_t)length);
    }

    CHECK_CONDITION_EXECUTE((dumpBox != NULL) && (!caller->dumpFile->AddDumpBox(dumpBox)), FREE_MEM_CLASS(dumpBox));
  }
}