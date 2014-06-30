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
#include "ErrorCodes.h"
#include "NetworkInterfaceCollection.h"

#include <process.h>

CCurlInstance::CCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CFlags()
{
  this->logger = NULL;
  this->protocolName = NULL;
  this->curl = NULL;
  this->multi_curl = NULL;
  this->hCurlWorkerThread = NULL;
  this->writeCallback = NULL;
  this->writeData = NULL;
  this->state = CURL_STATE_CREATED;
  this->mutex = NULL;
  this->startReceivingTicks = 0;
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;
  this->curlWorkerShouldExit = false;
  this->lastReceiveTime = 0;
  this->downloadRequest = NULL;
  this->downloadResponse = NULL;
  this->dumpFile = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, mutex);
    CHECK_POINTER_DEFAULT_HRESULT(*result, protocolName);
    CHECK_POINTER_DEFAULT_HRESULT(*result, instanceName);

    if (SUCCEEDED(*result))
    {
      this->logger = logger;
      this->mutex = mutex;
      this->protocolName = FormatString(L"%s: instance '%s'", protocolName, instanceName);
      this->dumpFile = new CDumpFile(result);

      CHECK_POINTER_HRESULT(*result, this->protocolName, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->dumpFile, *result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(*result))
    {
      this->SetWriteCallback(CCurlInstance::CurlReceiveDataCallback, this);
    }
  }
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

  FREE_MEM(this->protocolName);
  FREE_MEM_CLASS(this->downloadRequest);
  FREE_MEM_CLASS(this->downloadResponse);
  FREE_MEM_CLASS(this->dumpFile);
}

/* get methods */

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

CDownloadResponse *CCurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CDownloadResponse *response = new CDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

const wchar_t *CCurlInstance::GetDumpFile(void)
{
  return this->dumpFile->GetDumpFile();
}

/* set methods */

bool CCurlInstance::SetDumpFile(const wchar_t *dumpFile)
{
  return this->dumpFile->SetDumpFile(dumpFile);
}

/* other methods */

HRESULT CCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = ((this->logger != NULL) && (this->protocolName != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->DestroyCurlWorker(), result);

  if (SUCCEEDED(result))
  {
    FREE_MEM_CLASS(this->downloadRequest);
    this->downloadRequest = downloadRequest->Clone();

    CHECK_CONDITION_HRESULT(result, (this->downloadRequest != NULL) && (this->downloadRequest != NULL)  && (this->downloadRequest->GetUrl() != NULL), result, E_NOT_VALID_STATE);
  }

  if (SUCCEEDED(result))
  {
    if (this->multi_curl == NULL)
    {
      this->multi_curl = curl_multi_init();
    }

    CHECK_POINTER_HRESULT(result, this->multi_curl, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    if (this->curl == NULL)
    {
      this->curl = curl_easy_init();
    }

    CHECK_POINTER_HRESULT(result, this->curl, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    unsigned int currentTime = GetTickCount();

    CHECK_CONDITION_HRESULT(result, this->downloadRequest->GetFinishTime() >= currentTime, result, VFW_E_TIMEOUT);
    CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: finish time before current time, finish time: %u, current time: %u", this->protocolName, METHOD_INITIALIZE_NAME, this->downloadRequest->GetFinishTime(), currentTime));

    if (SUCCEEDED(result))
    {
      unsigned int dataTimeout = (this->downloadRequest->GetFinishTime() == FINISH_TIME_NOT_SPECIFIED) ? this->downloadRequest->GetReceiveDataTimeout() : (this->downloadRequest->GetFinishTime() - GetTickCount());

      CHECK_CONDITION_EXECUTE_RESULT(dataTimeout != UINT_MAX, HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_CONNECTTIMEOUT, (long)(dataTimeout / 1000))), result);
      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting connection timeout: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if ((SUCCEEDED(result)) && (this->downloadRequest->GetNetworkInterfaceName() != NULL))
    {
      CNetworkInterfaceCollection *networkInterfaces = new CNetworkInterfaceCollection(&result);
      CHECK_POINTER_HRESULT(result, networkInterfaces, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), CNetworkInterface::GetAllNetworkInterfaces(networkInterfaces, AF_UNSPEC), result);
      CHECK_CONDITION_HRESULT(result, networkInterfaces->Count() != 0, result, E_NOT_VALID_STATE);

      // in network interface collection have to stay only interfaces with specified network interface name
      unsigned int i = 0;
      while (SUCCEEDED(result) && (i < networkInterfaces->Count()))
      {
        CNetworkInterface *nic = networkInterfaces->GetItem(i);

        if (CompareWithNull(nic->GetFriendlyName(), this->downloadRequest->GetNetworkInterfaceName()) == 0)
        {
          i++;
        }
        else
        {
          networkInterfaces->Remove(i);
        }
      }

      CHECK_CONDITION_HRESULT(result, networkInterfaces->Count() != 0, result, E_NOT_FOUND_INTERFACE_NAME);

      FREE_MEM_CLASS(networkInterfaces);

      if (SUCCEEDED(result))
      {
        wchar_t *curlNic = FormatString(L"if!%s", this->downloadRequest->GetNetworkInterfaceName());
        CHECK_POINTER_HRESULT(result, curlNic, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          char *curlInterface = ConvertToMultiByte(curlNic);
          CHECK_POINTER_HRESULT(result, curlInterface, result, E_CONVERT_STRING_ERROR);

          CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_INTERFACE, curlInterface)), result);
          FREE_MEM(curlInterface);
        }

        FREE_MEM(curlNic);
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting network interface: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      char *curlUrl = ConvertToMultiByte(this->downloadRequest->GetUrl());
      CHECK_POINTER_HRESULT(result, curlUrl, result, E_CONVERT_STRING_ERROR);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_URL, curlUrl)), result);

      FREE_MEM(curlUrl);
      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting url: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_WRITEFUNCTION, this->writeCallback));

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting write callback: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_WRITEDATA, this->writeData));

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting write callback data: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_DEBUGFUNCTION, CCurlInstance::CurlDebugCallback));

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting debug callback: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_DEBUGDATA, this));

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting debug callback data: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }

    if (SUCCEEDED(result))
    {
      result = HRESULT_FROM_CURL_CODE(curl_easy_setopt(this->curl, CURLOPT_VERBOSE, 1L));

      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while setting verbose level: 0x%08X", this->protocolName, METHOD_INITIALIZE_NAME, result));
    }
  }

  if (SUCCEEDED(result))
  {
    FREE_MEM_CLASS(this->downloadResponse);
    this->downloadResponse = this->CreateDownloadResponse();
    CHECK_POINTER_HRESULT(result, this->downloadResponse, result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(result, this->downloadResponse->GetReceivedData(), result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, this->downloadResponse->GetReceivedData()->InitializeBuffer(MINIMUM_BUFFER_SIZE), result, E_OUTOFMEMORY);
  }

  this->state = SUCCEEDED(result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

HRESULT CCurlInstance::CreateCurlWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->protocolName, METHOD_CREATE_CURL_WORKER_NAME);

  // clear result error
  this->downloadResponse->SetResultError(S_OK);

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

  HRESULT result = HRESULT_FROM_CURLM_CODE(curl_multi_add_handle(this->multi_curl, this->curl));
  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while adding curl handle: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, result));

  if (SUCCEEDED(result))
  {
    while (SUCCEEDED(result) && (!this->curlWorkerShouldExit))
    {
      int runningHandles = 0;
      result = HRESULT_FROM_CURLM_CODE(curl_multi_perform(this->multi_curl, &runningHandles));

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
              this->downloadResponse->SetResultError(HRESULT_FROM_CURL_CODE(message->data.result));
            }
          }

        } while (messageCount != 0);

        // no running transfers, we can quit
        break;
      }
      else if ((GetTickCount() - this->lastReceiveTime) > (this->downloadRequest->GetReceiveDataTimeout()))
      {
        // timeout occured
        this->downloadResponse->SetResultError(HRESULT_FROM_CURL_CODE(CURLE_OPERATION_TIMEOUTED));
        break;
      }

      int ret = 0;
      if (curl_multi_wait(this->multi_curl, NULL, 0, 0, &ret) == CURLM_OK)
      {
        if (ret == 0)
        {
          // nothing is waiting to read data
          // sleep some time, get chance for other threads to work
          Sleep(1);
        }
      }
    }

    CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, result));

    result = HRESULT_FROM_CURLM_CODE(curl_multi_remove_handle(this->multi_curl, this->curl));
    CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while removing curl handle: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, result));

    if (this->stopReceivingTicks == 0)
    {
      this->stopReceivingTicks = GetTickCount();
    }
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: start: %u, end: %u, received bytes: %lld", this->protocolName, METHOD_CURL_WORKER_NAME, this->startReceivingTicks, this->stopReceivingTicks, this->totalReceivedBytes);

    this->state = CURL_STATE_RECEIVED_ALL_DATA;

    if (IS_CURL_ERROR(this->downloadResponse->GetResultError()) && (this->downloadResponse->GetResultError() != HRESULT_FROM_CURL_CODE(CURLE_WRITE_ERROR)))
    {
      this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, this->downloadResponse->GetResultError());
    }
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->protocolName, METHOD_CURL_WORKER_NAME);
  return S_OK;
}

void CCurlInstance::SetWriteCallback(curl_write_callback writeCallback, void *writeData)
{
  this->writeCallback = writeCallback;
  this->writeData = writeData;
}

HRESULT CCurlInstance::StartReceivingData(void)
{
  // set last receive time of any data to avoid timeout in CurlWorker()
  this->lastReceiveTime = GetTickCount();

  return this->CreateCurlWorker();
}

HRESULT CCurlInstance::StopReceivingData(void)
{
  return this->DestroyCurlWorker();
}

size_t CCurlInstance::CurlReceiveDataCallback(char *buffer, size_t size, size_t nmemb, void *userdata)
{
  CCurlInstance *caller = (CCurlInstance *)userdata;

  caller->state = CURL_STATE_RECEIVING_DATA;
  caller->lastReceiveTime = GetTickCount();

  CDumpBox *dumpBox = NULL;
  size_t result = 0;

  if (!caller->curlWorkerShouldExit)
  {
    CHECK_CONDITION_NOT_NULL_EXECUTE(caller->dumpFile->GetDumpFile(), dumpBox = caller->CreateDumpBox());

    result = caller->CurlReceiveData(dumpBox, (unsigned char *)buffer, (size_t)(size * nmemb));

    CHECK_CONDITION_EXECUTE((dumpBox != NULL) && (!caller->dumpFile->AddDumpBox(dumpBox)), FREE_MEM_CLASS(dumpBox));
  }

  return result;
}

size_t CCurlInstance::CurlReceiveData(CDumpBox *dumpBox, const unsigned char *buffer, size_t length)
{
  if (length != 0)
  {
    // lock access to receive data buffer
    // if mutex is NULL then access to received data buffer is not locked
    CLockMutex lock(this->mutex, INFINITE);

    if (dumpBox != NULL)
    {
      dumpBox->SetTimeWithLocalTime();
      dumpBox->SetPayload(buffer, length);
    }

    this->totalReceivedBytes += length;

    unsigned int bufferSize = this->downloadResponse->GetReceivedData()->GetBufferSize();
    unsigned int freeSpace = this->downloadResponse->GetReceivedData()->GetBufferFreeSpace();
    unsigned int newBufferSize = max(bufferSize * 2, bufferSize + length);

    if (freeSpace < length)
    {
      if (!this->downloadResponse->GetReceivedData()->ResizeBuffer(newBufferSize))
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: resizing of buffer unsuccessful, current size: %u, requested size: %u", this->protocolName, METHOD_CURL_RECEIVE_DATA_NAME, this->downloadResponse->GetReceivedData()->GetBufferSize(), newBufferSize);
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

HRESULT CCurlInstance::SetString(CURLoption option, const wchar_t *string)
{
  return CCurlInstance::SetString(this->curl, option, string);
}

HRESULT CCurlInstance::SetString(CURL *curl, CURLoption option, const wchar_t *string)
{
  char *multiByteString = ConvertToMultiByteW(string);
  HRESULT result = ((string == NULL) || ((multiByteString != NULL) && (string != NULL))) ? S_OK : E_CONVERT_STRING_ERROR;

  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_CURL_CODE(curl_easy_setopt(curl, option, multiByteString)), result);

  FREE_MEM(multiByteString);
  return result;
}

HRESULT CCurlInstance::Select(bool read, bool write, unsigned int timeout)
{
  HRESULT result = S_OK;

  CHECK_CONDITION_HRESULT(result, this->curl, result, E_NOT_VALID_STATE);
  CHECK_CONDITION_HRESULT(result, read || write, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    curl_socket_t socket = CURL_SOCKET_BAD;
    curl_easy_getinfo(this->curl, CURLINFO_LASTSOCKET, &socket);
    
    CHECK_CONDITION_HRESULT(result, socket != CURL_SOCKET_BAD, result, E_NOT_VALID_STATE);

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
      sendTimeout.tv_sec = (int)(timeout / 1000000);
      sendTimeout.tv_usec = (int)(timeout % 1000000);

      int selectResult = select(0, &readFD, &writeFD, &exceptFD, (timeout == UINT_MAX) ? NULL : &sendTimeout);
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
  }

  return result;
}

HRESULT CCurlInstance::SendData(const unsigned char *data, unsigned int length, unsigned int timeout)
{
  unsigned int receivedData = 0;
  HRESULT result = HRESULT_FROM_CURL_CODE(curl_easy_send(this->curl, data, length, &receivedData));

  CHECK_CONDITION_EXECUTE_RESULT(result == HRESULT_FROM_CURL_CODE(CURLE_AGAIN), this->Select(false, true, timeout), result);
  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while sending data: 0x%08X", this->protocolName, L"SendData()", result));
  return result;
}

HRESULT CCurlInstance::ReadData(unsigned char *data, unsigned int length)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, data);
  CHECK_CONDITION_HRESULT(result, length != 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    unsigned int receivedData = 0;
    result = HRESULT_FROM_CURL_CODE(curl_easy_recv(this->curl, data, length, &receivedData));

    CHECK_CONDITION_EXECUTE(result == HRESULT_FROM_CURL_CODE(CURLE_AGAIN), result = S_OK);
    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->totalReceivedBytes += receivedData);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (HRESULT)receivedData);
  }

  return result;
}

CDumpBox *CCurlInstance::CreateDumpBox(void)
{
  HRESULT result = S_OK;
  CDumpBox *box = new CDumpBox(&result);
  CHECK_POINTER_HRESULT(result, box, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(box));
  return box;
}