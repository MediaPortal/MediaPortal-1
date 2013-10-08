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

#include "CurlInstance.h"
#include "Logger.h"
#include "LockMutex.h"

#include <process.h>

CCurlInstance::CCurlInstance(CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
{
  this->logger = logger;
  this->protocolName = FormatString(L"%s: instance '%s'", protocolName, instanceName);
  this->curl = NULL;
  this->multi_curl = NULL;
  this->hCurlWorkerThread = NULL;
  this->receiveDataTimeout = UINT_MAX;
  this->writeCallback = NULL;
  this->writeData = NULL;
  this->state = CURL_STATE_CREATED;
  this->mutex = mutex;
  this->startReceivingTicks = 0;
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;
  this->curlWorkerShouldExit = false;
  this->lastReceiveTime = 0;
  this->networkInterfaceName = NULL;
  this->networkInterfaces = new CNetworkInterfaceCollection();
  this->finishTime = FINISH_TIME_NOT_SPECIFIED;

  this->SetNetworkInterfaceName(this->networkInterfaceName);    // this sets network interfaces
  this->SetWriteCallback(CCurlInstance::CurlReceiveDataCallback, this);

  this->downloadRequest = NULL;
  this->downloadResponse = NULL;
}

CCurlInstance::~CCurlInstance(void)
{
  this->StopReceivingData();

  if (this->curl != NULL)
  {
    curl_easy_cleanup(this->curl);
    this->curl = NULL;
  }

  if (this->multi_curl != NULL)
  {
    curl_multi_cleanup(this->multi_curl);
    this->multi_curl = NULL;
  }

  FREE_MEM(this->networkInterfaceName);
  FREE_MEM(this->protocolName);
  FREE_MEM_CLASS(this->downloadRequest);
  FREE_MEM_CLASS(this->downloadResponse);
  FREE_MEM_CLASS(this->networkInterfaces);
}

/* get methods */

unsigned int CCurlInstance::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

unsigned int CCurlInstance::GetCurlState(void)
{
  return this->state;
}

wchar_t *CCurlInstance::GetCurlVersion(void)
{
  char *curlVersion = curl_version();

  return ConvertToUnicodeA(curlVersion);
}

CDownloadRequest *CCurlInstance::GetDownloadRequest(void)
{
  return this->downloadRequest;
}

CDownloadResponse *CCurlInstance::GetDownloadResponse(void)
{
  return this->downloadResponse;
}

CDownloadResponse *CCurlInstance::GetNewDownloadResponse(void)
{
  return new CDownloadResponse();
}

const wchar_t *CCurlInstance::GetNetworkInterfaceName(void)
{
  return this->networkInterfaceName;
}

unsigned int CCurlInstance::GetFinishTime(void)
{
  return this->finishTime;
}

/* set methods */

void CCurlInstance::SetReceivedDataTimeout(unsigned int timeout)
{
  this->receiveDataTimeout = timeout;
}

bool CCurlInstance::SetNetworkInterfaceName(const wchar_t *networkInterfaceName)
{
  bool result = (this->networkInterfaces != NULL);

  if (result)
  {
    this->networkInterfaces->Clear();
    SET_STRING_RESULT_WITH_NULL(this->networkInterfaceName, networkInterfaceName, result);
  }

  if (result)
  {
    result = SUCCEEDED(CNetworkInterface::GetAllNetworkInterfaces(this->networkInterfaces, AF_UNSPEC));
  }

  if (result && (this->networkInterfaceName != NULL))
  {
    // in network interface collection have to stay only interfaces with specified network interface name
    unsigned int i = 0;
    while (i < this->networkInterfaces->Count())
    {
      CNetworkInterface *nic = this->networkInterfaces->GetItem(i);

      if (CompareWithNull(nic->GetFriendlyName(), this->networkInterfaceName) == 0)
      {
        i++;
      }
      else
      {
        this->networkInterfaces->Remove(i);
      }
    }

    result &= (this->networkInterfaces->Count() != 0);
  }

  return result;
}

void CCurlInstance::SetFinishTime(unsigned int finishTime)
{
  this->finishTime = finishTime;
}

/* other methods */

bool CCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  bool result = (this->logger != NULL) && (this->protocolName != NULL) && (this->networkInterfaces != NULL);
  result &= SUCCEEDED(this->DestroyCurlWorker());

  if (result)
  {
    FREE_MEM_CLASS(this->downloadRequest);
    this->downloadRequest = downloadRequest->Clone();
    result &= (this->downloadRequest != NULL) && (this->downloadRequest != NULL)  && (this->downloadRequest->GetUrl() != NULL) && (this->networkInterfaces->Count() != 0);
  }

  if (result)
  {
    if (this->multi_curl == NULL)
    {
      this->multi_curl = curl_multi_init();
    }
    result = (this->multi_curl != NULL);
  }

  if (result)
  {
    if (this->curl == NULL)
    {
      this->curl = curl_easy_init();
    }
    result = (this->curl != NULL);
  }

  if (result)
  {
    CURLcode errorCode = CURLE_OK;
    unsigned int currentTime = GetTickCount();

    CHECK_CONDITION_EXECUTE(this->finishTime < currentTime, errorCode = CURLE_OPERATION_TIMEOUTED);
    if (errorCode != CURLE_OK)
    {
      this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"finish time before current time", errorCode);
      result = false;
    }

    if (errorCode == CURLE_OK)
    {
      unsigned int dataTimeout = (this->finishTime == FINISH_TIME_NOT_SPECIFIED) ? this->receiveDataTimeout : (this->finishTime - GetTickCount());

      if (dataTimeout != UINT_MAX)
      {
        errorCode = curl_easy_setopt(this->curl, CURLOPT_CONNECTTIMEOUT, (long)(dataTimeout / 1000));
        if (errorCode != CURLE_OK)
        {
          this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting connection timeout", errorCode);
          result = false;
        }
      }
    }

    if ((errorCode == CURLE_OK) && (this->networkInterfaceName != NULL))
    {
      wchar_t *curlNic = FormatString(L"if!%s", this->networkInterfaceName);
      char *curlInterface = ConvertToMultiByte(curlNic);
      errorCode = curl_easy_setopt(this->curl, CURLOPT_INTERFACE, curlInterface);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting network interface", errorCode);
        result = false;
      }

      FREE_MEM(curlInterface);
      FREE_MEM(curlNic);
    }

    if (errorCode == CURLE_OK)
    {
      char *curlUrl = ConvertToMultiByte(this->downloadRequest->GetUrl());
      errorCode = curl_easy_setopt(this->curl, CURLOPT_URL, curlUrl);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting url", errorCode);
        result = false;
      }
      FREE_MEM(curlUrl);
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_WRITEFUNCTION, this->writeCallback);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting write callback", errorCode);
        result = false;
      }
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_WRITEDATA, this->writeData);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting write callback data", errorCode);
        result = false;
      }
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_DEBUGFUNCTION, CCurlInstance::CurlDebugCallback);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting debug callback", errorCode);
        result = false;
      }
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_DEBUGDATA, this);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting debug callback data", errorCode);
        result = false;
      }
    }

    if (errorCode == CURLE_OK)
    {
      errorCode = curl_easy_setopt(this->curl, CURLOPT_VERBOSE, 1L);
      if (errorCode != CURLE_OK)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while setting verbose level", errorCode);
        result = false;
      }
    }
  }

  if (result)
  {
    FREE_MEM_CLASS(this->downloadResponse);
    this->downloadResponse = this->GetNewDownloadResponse();
    result &= (this->downloadResponse != NULL) && (this->downloadResponse->GetReceivedData() != NULL);
    if (result)
    {
      result = this->downloadResponse->GetReceivedData()->InitializeBuffer(MINIMUM_BUFFER_SIZE);
    }
  }

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

void CCurlInstance::ReportCurlErrorMessage(unsigned int logLevel, const wchar_t *protocolName, const wchar_t *functionName, const wchar_t *message, CURLcode errorCode)
{
  wchar_t *error = ConvertToUnicodeA(curl_easy_strerror(errorCode));
  this->logger->Log(logLevel, METHOD_CURL_ERROR_MESSAGE, protocolName, functionName, (message == NULL) ? L"libcurl error" : message, error);
  FREE_MEM(error);
}

void CCurlInstance::ReportCurlErrorMessage(unsigned int logLevel, const wchar_t *protocolName, const wchar_t *functionName, const wchar_t *message, CURLMcode errorCode)
{
  wchar_t *error = ConvertToUnicodeA(curl_multi_strerror(errorCode));
  this->logger->Log(logLevel, METHOD_CURL_ERROR_MESSAGE, protocolName, functionName, (message == NULL) ? L"libcurl (multi) error" : message, error);
  FREE_MEM(error);
}

HRESULT CCurlInstance::CreateCurlWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->protocolName, METHOD_CREATE_CURL_WORKER_NAME);

  // clear curl error code
  this->downloadResponse->SetResultCode(CURLE_OK);

  if (this->hCurlWorkerThread == NULL)
  {
    this->hCurlWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CCurlInstance::CurlWorker, this, 0, NULL);
  }

  if (this->hCurlWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", this->protocolName, METHOD_CREATE_CURL_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->protocolName, METHOD_CREATE_CURL_WORKER_NAME, result);
  return result;
}

HRESULT CCurlInstance::DestroyCurlWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->protocolName, METHOD_DESTROY_CURL_WORKER_NAME);

  // wait for the receive data worker thread to exit      
  if (this->hCurlWorkerThread != NULL)
  {
    this->curlWorkerShouldExit = true;
    if (WaitForSingleObject(this->hCurlWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_DESTROY_CURL_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->hCurlWorkerThread, 0);
    }
    CloseHandle(this->hCurlWorkerThread);

    long responseCode;
    if (curl_easy_getinfo(this->curl, CURLINFO_RESPONSE_CODE, &responseCode) == CURLE_OK)
    {
      this->downloadResponse->SetResponseCode(responseCode);
    }

    if (this->stopReceivingTicks == 0)
    {
      this->stopReceivingTicks = GetTickCount();
    }

    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: start: %u, end: %u, received bytes: %lld", this->protocolName, METHOD_DESTROY_CURL_WORKER_NAME, this->startReceivingTicks, this->stopReceivingTicks, this->totalReceivedBytes);
  }
  this->curlWorkerShouldExit = false;
  this->hCurlWorkerThread = NULL;
  
  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->protocolName, METHOD_DESTROY_CURL_WORKER_NAME, result);
  return result;
}

unsigned int WINAPI CCurlInstance::CurlWorker(LPVOID lpParam)
{
  CCurlInstance *caller = (CCurlInstance *)lpParam;

  unsigned int result = caller->CurlWorker();

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return result;
}

unsigned int CCurlInstance::CurlWorker(void)
{
  this->logger->Log(LOGGER_INFO, L"%s: %s: Start, url: '%s'", this->protocolName, METHOD_CURL_WORKER_NAME, this->downloadRequest->GetUrl());
  this->startReceivingTicks = GetTickCount();
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;

  CURLMcode multiErrorCode = curl_multi_add_handle(this->multi_curl, this->curl);
  if (multiErrorCode == CURLM_OK)
  {
    while ((!this->curlWorkerShouldExit) && (multiErrorCode == CURLM_OK))
    {
      int runningHandles = 0;
      multiErrorCode = curl_multi_perform(this->multi_curl, &runningHandles);

      if (runningHandles == 0)
      {
        // process messages
        int messageCount = 0;
        CURLMsg *message = NULL;
        do
        {
          message = curl_multi_info_read(this->multi_curl, &messageCount);

          if (message != NULL)
          {
            if (message->msg = CURLMSG_DONE)
            {
              this->downloadResponse->SetResultCode(message->data.result);
            }
          }

        } while (messageCount != 0);

        // no running transfers, we can quit
        break;
      }
      else if ((GetTickCount() - this->lastReceiveTime) > (this->GetReceiveDataTimeout()))
      {
        // timeout occured
        this->downloadResponse->SetResultCode(CURLE_OPERATION_TIMEDOUT);
        break;
      }

      // sleep some time, get chance for other threads to work
      Sleep(10);
    }

    if (multiErrorCode != CURLM_OK)
    {
      this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_CURL_WORKER_NAME, L"error while receiving data", multiErrorCode);
    }

    multiErrorCode = curl_multi_remove_handle(this->multi_curl, this->curl);
    if (multiErrorCode != CURLM_OK)
    {
      this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_CURL_WORKER_NAME, L"error while removing curl handle", multiErrorCode);
    }

    if (this->stopReceivingTicks == 0)
    {
      this->stopReceivingTicks = GetTickCount();
    }
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: start: %u, end: %u, received bytes: %lld", this->protocolName, METHOD_CURL_WORKER_NAME, this->startReceivingTicks, this->stopReceivingTicks, this->totalReceivedBytes);

    long responseCode;
    if (curl_easy_getinfo(this->curl, CURLINFO_RESPONSE_CODE, &responseCode) == CURLE_OK)
    {
      this->downloadResponse->SetResponseCode(responseCode);
    }

    this->state = CURL_STATE_RECEIVED_ALL_DATA;

    if ((this->downloadResponse->GetResultCode() != CURLE_OK) && (this->downloadResponse->GetResultCode() != CURLE_WRITE_ERROR))
    {
      this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_CURL_WORKER_NAME, L"error while receiving data", this->downloadResponse->GetResultCode());
    }
  }
  else
  {
    this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_CURL_WORKER_NAME, L"error while adding curl handle", multiErrorCode);
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->protocolName, METHOD_CURL_WORKER_NAME);
  return S_OK;
}

void CCurlInstance::SetWriteCallback(curl_write_callback writeCallback, void *writeData)
{
  this->writeCallback = writeCallback;
  this->writeData = writeData;
}

bool CCurlInstance::StartReceivingData(void)
{
  // set last receive time of any data to avoid timeout in CurlWorker()
  this->lastReceiveTime = GetTickCount();

  return (this->CreateCurlWorker() == S_OK);
}

bool CCurlInstance::StopReceivingData(void)
{
  return (this->DestroyCurlWorker() == S_OK);
}

size_t CCurlInstance::CurlReceiveDataCallback(char *buffer, size_t size, size_t nmemb, void *userdata)
{
  CCurlInstance *caller = (CCurlInstance *)userdata;

  caller->state = CURL_STATE_RECEIVING_DATA;
  caller->lastReceiveTime = GetTickCount();

  return (caller->curlWorkerShouldExit) ? 0 : caller->CurlReceiveData((unsigned char *)buffer, (size_t)(size * nmemb));
}

size_t CCurlInstance::CurlReceiveData(const unsigned char *buffer, size_t length)
{
  long responseCode;
  if (curl_easy_getinfo(this->curl, CURLINFO_RESPONSE_CODE, &responseCode) == CURLE_OK)
  {
    this->downloadResponse->SetResponseCode(responseCode);
  }

  if (length != 0)
  {
    // lock access to receive data buffer
    // if mutex is NULL then access to received data buffer is not locked
    CLockMutex lock(this->mutex, INFINITE);

    this->totalReceivedBytes += length;

    unsigned int bufferSize = this->downloadResponse->GetReceivedData()->GetBufferSize();
    unsigned int freeSpace = this->downloadResponse->GetReceivedData()->GetBufferFreeSpace();
    unsigned int newBufferSize = max(bufferSize * 2, bufferSize + length);

    if (freeSpace < length)
    {
      if (!this->downloadResponse->GetReceivedData()->ResizeBuffer(newBufferSize))
      {
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_CURL_RECEIVE_DATA_NAME, L"resizing of buffer unsuccessful");
        // it indicates error
        length = 0;
      }
    }

    if (length != 0)
    {
      this->downloadResponse->GetReceivedData()->AddToBuffer(buffer, length);
    }
  }

  return length;
}

int CCurlInstance::CurlDebugCallback(CURL *handle, curl_infotype type, char *data, size_t size, void *userptr)
{
  CCurlInstance *caller = (CCurlInstance *)userptr;

  // warning: data ARE NOT terminated with null character !!
  if (size > 0)
  {
    size_t length = size + 1;
    ALLOC_MEM_DEFINE_SET(tempData, char, length, 0);
    if (tempData != NULL)
    {
      memcpy(tempData, data, size);

      // now convert data to used character set
      wchar_t *curlData = ConvertToUnicodeA(tempData);

      if (curlData != NULL)
      {
        // we have converted and null terminated data
        caller->CurlDebug(type, curlData);
      }

      FREE_MEM(curlData);
    }
    FREE_MEM(tempData);
  }

  return 0;
}

void CCurlInstance::CurlDebug(curl_infotype type, const wchar_t *data)
{
}

CURLcode CCurlInstance::SetString(CURLoption option, const wchar_t *string)
{
  return CCurlInstance::SetString(this->curl, option, string);
}

CURLcode CCurlInstance::SetString(CURL *curl, CURLoption option, const wchar_t *string)
{
  CURLcode result = CURLE_OK;

  char *multiByteString = ConvertToMultiByteW(string);
  result = ((string == NULL) || ((multiByteString != NULL) && (string != NULL))) ? result : CURLE_OUT_OF_MEMORY;

  if (result == CURLE_OK)
  {
    result = curl_easy_setopt(curl, option, multiByteString);
  }

  FREE_MEM(multiByteString);

  return result;
}

HRESULT Select(SOCKET socket, bool read, bool write, unsigned int timeout)
{
  HRESULT result = S_OK;
  CHECK_CONDITION_HRESULT(result, socket != INVALID_SOCKET, result, E_INVALIDARG);
  CHECK_CONDITION_HRESULT(result, read || write, result, E_INVALIDARG);
  CHECK_CONDITION_HRESULT(result, timeout > 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    fd_set readFD;
    fd_set writeFD;
    fd_set exceptFD;

    FD_ZERO(&readFD);
    FD_ZERO(&writeFD);
    FD_ZERO(&exceptFD);

    if (read)
    {
      // want to read from socket
      FD_SET(socket, &readFD);
    }
    if (write)
    {
      // want to write to socket
      FD_SET(socket, &writeFD);
    }
    // we want to receive errors
    FD_SET(socket, &exceptFD);

    timeval sendTimeout;
    sendTimeout.tv_sec = timeout;
    sendTimeout.tv_usec = 0;

    int selectResult = select(0, &readFD, &writeFD, &exceptFD, &sendTimeout);
    if (selectResult == 0)
    {
      // timeout occured
      result = HRESULT_FROM_WIN32(WSAETIMEDOUT);
    }
    else if (selectResult == SOCKET_ERROR)
    {
      // socket error occured
      result = HRESULT_FROM_WIN32(WSAGetLastError());
    }

    if (SUCCEEDED(result))
    {
      if (FD_ISSET(socket, &exceptFD))
      {
        // error occured on socket, select function was successful
        int err;
        int errlen = sizeof(err);

        if (getsockopt(socket, SOL_SOCKET, SO_ERROR, (char *)&err, &errlen) == 0)
        {
          // successfully get error
          result = HRESULT_FROM_WIN32(err);
        }
        else
        {
          // error occured while getting error
          result = HRESULT_FROM_WIN32(WSAGetLastError());
        }
      }

      if (result == 0)
      {
        if (read && (FD_ISSET(socket, &readFD) == 0))
        {
          result = E_NOT_VALID_STATE;
        }

        if (write && (FD_ISSET(socket, &writeFD) == 0))
        {
          result = E_NOT_VALID_STATE;
        }
      }
    }
  }

  return result;
}

CURLcode CCurlInstance::SendData(const unsigned char *data, unsigned int length, unsigned int timeout)
{
  unsigned int receivedData = 0;
  CURLcode errorCode = curl_easy_send(this->curl, data, length, &receivedData);

  if (errorCode == CURLE_AGAIN)
  {
    errorCode = CURLE_OK;
    curl_socket_t socket = CURL_SOCKET_BAD;

    CHECK_CONDITION_EXECUTE_RESULT(errorCode == CURLE_OK, curl_easy_getinfo(curl, CURLINFO_LASTSOCKET, &socket), errorCode);
    CHECK_CONDITION_EXECUTE((errorCode == CURLE_OK) && (socket == CURL_SOCKET_BAD), errorCode = CURLE_NO_CONNECTION_AVAILABLE);
    CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, L"SendData()", L"error while sending data", errorCode));
    
    if (errorCode == CURLE_OK)
    {
      HRESULT result = Select(socket, false, true, timeout);

      if (FAILED(result))
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: error while sending data: 0x%08X", protocolName, L"SendData()", result);
        errorCode = CURLE_SEND_ERROR;
      }
    }

    CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, L"SendData()", L"error while sending data", errorCode));
  }

  return errorCode;
}

int CCurlInstance::ReadData(unsigned char *data, unsigned int length)
{
  int result = 0;
  CHECK_CONDITION_EXECUTE(data == NULL, result = -CURLE_OUT_OF_MEMORY);
  CHECK_CONDITION_EXECUTE(length == 0, result = -CURLE_OUT_OF_MEMORY);

  if (result == 0)
  {
    unsigned int receivedData = 0;
    CURLcode errorCode = curl_easy_recv(this->curl, data, length, &receivedData);
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_AGAIN, errorCode = CURLE_OK);
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, this->totalReceivedBytes += receivedData);

    result = (errorCode == CURLE_OK) ? receivedData : (-errorCode);
  }

  return result;
}