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

#include "stdafx.h"

#include "ProtocolHoster.h"
#include "ErrorCodes.h"
#include "ProtocolHosterPluginMetadata.h"
#include "ProtocolPluginConfiguration.h"
#include "LockMutex.h"
#include "Parameters.h"

#include <process.h>

CProtocolHoster::CProtocolHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CHoster(result, logger, configuration, L"ProtocolHoster", L"mpurlsourcesplitter_protocol_*.dll")
{
  this->activeProtocol = NULL;
  this->receiveDataWorkerShouldExit = false;
  this->receiveDataWorkerThread = NULL;
  this->streamPackages = NULL;
  this->mutex = NULL;
  this->pauseSeekStopMode = PAUSE_SEEK_STOP_MODE_NONE;
  this->finishTime = 0;
  this->startReceivingData = false;
  this->protocolError = S_OK;
  this->startReceiveDataWorkerShouldExit = false;
  this->startReceiveDataWorkerThread = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, MODULE_PROTOCOL_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->streamPackages = new CStreamPackageCollection(result);
    this->mutex = CreateMutex(NULL, FALSE, NULL);

    CHECK_POINTER_HRESULT(*result, this->streamPackages, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->mutex, *result, E_OUTOFMEMORY);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_PROTOCOL_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CProtocolHoster::~CProtocolHoster(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_PROTOCOL_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));

  // stop receiving data
  this->StopReceivingData();

  FREE_MEM_CLASS(this->streamPackages);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
    this->mutex = NULL;
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_PROTOCOL_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));
}

// IProtocol interface implementation

ProtocolConnectionState CProtocolHoster::GetConnectionState(void)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->GetConnectionState() : None;
}

HRESULT CProtocolHoster::ParseUrl(const CParameterCollection *parameters)
{
  HRESULT retval = (this->hosterPluginMetadataCollection->Count() == 0) ? E_NO_PROTOCOL_LOADED : S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(retval, parameters);

  if (SUCCEEDED(retval))
  {
    this->activeProtocol = NULL;
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(retval, this->configuration->Append((CParameterCollection *)parameters), retval, E_OUTOFMEMORY);

    for (unsigned int i = 0; (SUCCEEDED(retval) && (i < this->hosterPluginMetadataCollection->Count())); i++)
    {
      CProtocolHosterPluginMetadata *metadata = (CProtocolHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);
      CProtocolPlugin *protocol = (CProtocolPlugin *)metadata->GetPlugin();

      metadata->SetSupported(protocol->ParseUrl(parameters) == S_OK);
      if (metadata->IsSupported() && (this->activeProtocol == NULL))
      {
        // active protocol wasn't set yet
        this->activeProtocol = protocol;
      }
    }

    CHECK_CONDITION_HRESULT(retval, this->activeProtocol != NULL, retval, E_NO_ACTIVE_PROTOCOL);
  }

  return retval;
}

HRESULT CProtocolHoster::ReceiveData(CStreamPackage *streamPackage)
{
  if (this->activeProtocol != NULL)
  {
    return this->activeProtocol->ReceiveData(streamPackage);
  }

  return E_NO_ACTIVE_PROTOCOL;
}

HRESULT CProtocolHoster::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);
  CHECK_POINTER_HRESULT(result, this->activeProtocol, result, E_NO_ACTIVE_PROTOCOL);

  if (SUCCEEDED(result))
  {
    result = this->activeProtocol->GetConnectionParameters(parameters);
  }

  return result;
}

// ISimpleProtocol interface implementation

unsigned int CProtocolHoster::GetOpenConnectionTimeout(void)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->GetOpenConnectionTimeout() : 0;
}

unsigned int CProtocolHoster::GetOpenConnectionSleepTime(void)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->GetOpenConnectionSleepTime() : 0;
}

unsigned int CProtocolHoster::GetTotalReopenConnectionTimeout(void)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->GetTotalReopenConnectionTimeout() : 0;
}

HRESULT CProtocolHoster::StartReceivingData(CParameterCollection *parameters)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_START_RECEIVING_DATA_NAME);

  HRESULT result = S_OK;

  do
  {
    result = this->StartReceivingDataAsync(parameters);

    if (result == S_FALSE)
    {
      Sleep(1);
    }
  }
  while (result == S_FALSE);

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->hosterName, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CProtocolHoster::StopReceivingData(void)
{
  // stop receive data worker
  this->DestroyReceiveDataWorker();
  // stop start receive data worker
  this->DestroyStartReceiveDataWorker();

  this->pauseSeekStopMode = PAUSE_SEEK_STOP_MODE_NONE;
  this->finishTime = 0;
  this->startReceivingData = false;

  // close active protocol connection
  if (this->activeProtocol != NULL)
  {
    if (this->activeProtocol->GetConnectionState() == Opened)
    {
      this->activeProtocol->StopReceivingData();
    }
  }

  return S_OK;
}

HRESULT CProtocolHoster::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->QueryStreamProgress(streamProgress) : E_NOT_VALID_STATE;
}
  
void CProtocolHoster::ClearSession(void)
{
  // stop receiving data
  this->StopReceivingData();

  CHoster::ClearSession();

  this->activeProtocol = NULL;
  this->protocolError = S_OK;
}

int64_t CProtocolHoster::GetDuration(void)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->GetDuration() : DURATION_UNSPECIFIED;
}

void CProtocolHoster::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  if (this->activeProtocol != NULL)
  {
    this->activeProtocol->ReportStreamTime(streamTime, streamPosition);
  }
}

HRESULT CProtocolHoster::GetStreamInformation(CStreamInformationCollection *streams)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->GetStreamInformation(streams) : E_NO_ACTIVE_PROTOCOL;
}

// ISeeking interface implementation

unsigned int CProtocolHoster::GetSeekingCapabilities(void)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->GetSeekingCapabilities() : SEEKING_METHOD_NONE;
}

int64_t CProtocolHoster::SeekToTime(unsigned int streamId, int64_t time)
{
  return (this->activeProtocol != NULL) ? this->activeProtocol->SeekToTime(streamId, time) : E_NOT_VALID_STATE;
}

void CProtocolHoster::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->pauseSeekStopMode = pauseSeekStopMode;

  if (this->activeProtocol != NULL)
  {
    this->activeProtocol->SetPauseSeekStopMode(pauseSeekStopMode);
  }
}

// IDemuxerOwner interface implementation

HRESULT CProtocolHoster::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    {
      // lock mutex to add stream package to stream package collection

      CLockMutex lock(this->mutex, INFINITE);

      CStreamPackage *clone = streamPackage->Clone();
      CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->streamPackages->Add(clone), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
    }
  }

  if (SUCCEEDED(result))
  {
    // monitor processing of stream package

    bool processed = false;
    while (!processed)
    {
      {
        // lock mutex to get exclussive access to stream package
        // don't wait too long
        CLockMutex lock(this->mutex, 20);

        if (lock.IsLocked())
        {
          for (unsigned int i = 0; ((!processed) && (i < this->streamPackages->Count())); i++)
          {
            CStreamPackage *package = this->streamPackages->GetItem(i);

            CHECK_CONDITION_EXECUTE(this->activeProtocol == NULL, package->SetCompleted(E_NO_ACTIVE_PROTOCOL));

            if ((package->GetState() == CStreamPackage::Completed) &&
              (package->GetRequest()->GetId() == streamPackage->GetRequest()->GetId()) &&
              (package->GetRequest()->GetStreamId() == streamPackage->GetRequest()->GetStreamId()))
            {
              // clone response to stream package
              if (package->GetResponse() != NULL)
              {
                CStreamPackageResponse *response = package->GetResponse()->Clone();
                CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

                CHECK_CONDITION_EXECUTE(SUCCEEDED(result), streamPackage->SetResponse(response));
                CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
              }

              streamPackage->SetCompleted(SUCCEEDED(result) ? package->GetError() : result);

              // remove stream package from collection
              this->streamPackages->Remove(i);
              processed = true;
            }
          }
        }
      }

      if (!processed)
      {
        // sleep some time
        Sleep(1);
      }
    }
  }

  return result;
}

/* other methods */

CProtocolPlugin *CProtocolHoster::GetActiveProtocol(void)
{
  return this->activeProtocol;
}

HRESULT CProtocolHoster::LoadPlugins(void)
{
  HRESULT result = __super::LoadPlugins();
  CHECK_CONDITION_HRESULT(result, this->hosterPluginMetadataCollection->Count() != 0, result, E_NO_PROTOCOL_LOADED);

  CHECK_CONDITION_EXECUTE(result == E_NO_PROTOCOL_LOADED, this->logger->Log(LOGGER_ERROR, L"%s: %s: no protocol loaded", this->hosterName, METHOD_LOAD_PLUGINS_NAME));
  return result;
}

bool CProtocolHoster::IsLiveStreamSpecified(void)
{
  return this->activeProtocol->IsLiveStreamSpecified();
}

bool CProtocolHoster::IsLiveStreamDetected(void)
{
  return this->activeProtocol->IsLiveStreamDetected();
}

bool CProtocolHoster::IsLiveStream(void)
{
  return this->activeProtocol->IsLiveStream();
}

bool CProtocolHoster::IsSetStreamLength(void)
{
  return this->activeProtocol->IsSetStreamLength();
}

bool CProtocolHoster::IsStreamLengthEstimated(void)
{
  return this->activeProtocol->IsStreamLengthEstimated();
}

bool CProtocolHoster::IsWholeStreamDownloaded(void)
{
  return this->activeProtocol->IsWholeStreamDownloaded();
}

bool CProtocolHoster::IsEndOfStreamReached(void)
{
  return this->activeProtocol->IsEndOfStreamReached();
}

bool CProtocolHoster::IsConnectionLostCannotReopen(void)
{
  return this->activeProtocol->IsConnectionLostCannotReopen();
}

HRESULT CProtocolHoster::StartReceivingDataAsync(CParameterCollection *parameters)
{
  HRESULT result = S_FALSE;

  if (this->startReceiveDataWorkerThread == NULL)
  {
    result = this->CreateStartReceiveDataWorker();
    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = S_FALSE);
  }

  if (SUCCEEDED(result) && (this->startReceiveDataWorkerThread != NULL))
  {
    result = (WaitForSingleObject(this->startReceiveDataWorkerThread, 0) == WAIT_TIMEOUT) ? S_FALSE : this->protocolError;
  }
  
  if (result != S_FALSE)
  {
    // thread finished or error
    this->DestroyStartReceiveDataWorker();
  }

  return result;
}

/* protected methods */

CHosterPluginMetadata *CProtocolHoster::CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
{
  CProtocolHosterPluginMetadata *protocolMetadata = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    protocolMetadata = new CProtocolHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName);
    CHECK_POINTER_HRESULT(*result, protocolMetadata, *result, E_OUTOFMEMORY);
  
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(protocolMetadata));
  }

  return protocolMetadata;
}

CPluginConfiguration *CProtocolHoster::CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration)
{
  CProtocolPluginConfiguration *protocolConfiguration = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    protocolConfiguration = new CProtocolPluginConfiguration(result, configuration);
    CHECK_POINTER_HRESULT(*result, protocolConfiguration, *result, E_OUTOFMEMORY);
  
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(protocolConfiguration));
  }

  return protocolConfiguration;
}

HRESULT CProtocolHoster::CreateStartReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_CREATE_START_RECEIVE_DATA_WORKER_NAME);

  if (this->startReceiveDataWorkerThread == NULL)
  {
    this->startReceiveDataWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CProtocolHoster::StartReceiveDataWorker, this, 0, NULL);
  }

  if (this->startReceiveDataWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", this->hosterName, METHOD_CREATE_START_RECEIVE_DATA_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->hosterName, METHOD_CREATE_START_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

HRESULT CProtocolHoster::DestroyStartReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_DESTROY_START_RECEIVE_DATA_WORKER_NAME);

  this->startReceiveDataWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->startReceiveDataWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->startReceiveDataWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, this->hosterName, METHOD_DESTROY_START_RECEIVE_DATA_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->startReceiveDataWorkerThread, 0);
    }
    CloseHandle(this->startReceiveDataWorkerThread);
  }

  this->startReceiveDataWorkerThread = NULL;
  this->startReceiveDataWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->hosterName, METHOD_DESTROY_START_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

unsigned int WINAPI CProtocolHoster::StartReceiveDataWorker(LPVOID lpParam)
{
  CProtocolHoster *caller = (CProtocolHoster *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, caller->hosterName, METHOD_START_RECEIVE_DATA_WORKER_NAME);

  CHECK_POINTER_HRESULT(caller->protocolError, caller->activeProtocol, caller->protocolError, E_NO_ACTIVE_PROTOCOL);

  unsigned int timeout = 0;

  if (SUCCEEDED(caller->protocolError))
  {
    // get open connection data timeout for active protocol
    caller->finishTime = GetTickCount() + caller->activeProtocol->GetOpenConnectionTimeout();

    // now we have active protocol with loaded url, but still not working
    // create thread for receiving data

    caller->startReceivingData = true;
    caller->protocolError = caller->CreateReceiveDataWorker();
  }

  // wait for receiving data, timeout or exit
  while (SUCCEEDED(caller->protocolError) && (caller->activeProtocol->GetConnectionState() != Opened) && (!caller->activeProtocol->IsWholeStreamDownloaded()) && (!caller->activeProtocol->IsConnectionLostCannotReopen()) && (GetTickCount() <= caller->finishTime) && (!caller->startReceiveDataWorkerShouldExit))
  {
    Sleep(1);
  }

  CHECK_CONDITION_EXECUTE(FAILED(caller->protocolError), caller->StopReceivingData());
  CHECK_CONDITION_HRESULT(caller->protocolError, (!caller->activeProtocol->IsConnectionLostCannotReopen()) && (GetTickCount() <= caller->finishTime) && (!caller->startReceiveDataWorkerShouldExit), caller->protocolError, E_CONNECTION_LOST_CANNOT_REOPEN);
  caller->startReceivingData = false;

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, caller->hosterName, METHOD_START_RECEIVE_DATA_WORKER_NAME);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CProtocolHoster::CreateReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME);

  if (this->receiveDataWorkerThread == NULL)
  {
    this->receiveDataWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CProtocolHoster::ReceiveDataWorker, this, 0, NULL);
  }

  if (this->receiveDataWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", this->hosterName, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->hosterName, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

HRESULT CProtocolHoster::DestroyReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME);

  this->receiveDataWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->receiveDataWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->receiveDataWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, this->hosterName, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->receiveDataWorkerThread, 0);
    }
    CloseHandle(this->receiveDataWorkerThread);
  }

  this->receiveDataWorkerThread = NULL;
  this->receiveDataWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->hosterName, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

unsigned int WINAPI CProtocolHoster::ReceiveDataWorker(LPVOID lpParam)
{
  CProtocolHoster *caller = (CProtocolHoster *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME);

  bool openedConnection = false;

  HRESULT result = S_OK;

  unsigned int openConnectionTimeout = caller->activeProtocol->GetOpenConnectionTimeout();
  unsigned int openConnectionSleepTime = caller->activeProtocol->GetOpenConnectionSleepTime();
  unsigned int totalReopenConnectionTimeout = caller->activeProtocol->GetTotalReopenConnectionTimeout();

  // start ticks and end ticks are only for one try to open connection
  // total end ticks is absolutely last time for openning connection, otherwise PROTOCOL_PLUGIN_FLAG_CONNECTION_LOST_CANNOT_REOPEN will be set
  unsigned int currentTime = GetTickCount();
  unsigned int startTicks = currentTime + openConnectionSleepTime;
  unsigned int endTicks = currentTime + openConnectionSleepTime + openConnectionTimeout;
  unsigned int totalEndTicks = caller->startReceivingData ? endTicks : (caller->activeProtocol->IsLiveStreamSpecified() ? UINT_MAX : (startTicks + totalReopenConnectionTimeout));

  CStreamPackage *tempStreamPackage = new CStreamPackage(&result);
  CHECK_POINTER_HRESULT(result, tempStreamPackage, result, E_OUTOFMEMORY);

  while (!caller->receiveDataWorkerShouldExit)
  {
    Sleep(1);

    if (FAILED(result) || (caller->pauseSeekStopMode == PAUSE_SEEK_STOP_MODE_DISABLE_READING))
    {
      // we have error, for each stream package (if any) return error

      // don't wait too long
      CLockMutex lock(caller->mutex, 20);

      if (lock.IsLocked())
      {
        for (unsigned int i = 0; i < caller->streamPackages->Count(); i++)
        {
          HRESULT res = S_OK;
          CStreamPackage *package = caller->streamPackages->GetItem(i);

          package->SetCompleted((caller->pauseSeekStopMode == PAUSE_SEEK_STOP_MODE_DISABLE_READING) ? E_PAUSE_SEEK_STOP_MODE_DISABLE_READING : result);
        }
      }
    }

    if (SUCCEEDED(result))
    {
      unsigned int connectionState = caller->activeProtocol->GetConnectionState();
      currentTime = GetTickCount();

      if ((connectionState == InitializeFailed) || (connectionState == OpeningFailed))
      {
        // initialization or opening failed, stop receiving data and in next run try to open connection
        caller->activeProtocol->StopReceivingData();
      }
      else if ((!caller->activeProtocol->IsConnectionLostCannotReopen()) && (!caller->activeProtocol->IsWholeStreamDownloaded()) && (connectionState == None))
      {
        // problem with connection, try to (re)open

        if (openedConnection)
        {
          // we had opened connection, we lost it
          // some protocols may need some sleep before loading (e.g. multicast UDP protocol needs some time between unsubscribing and subscribing in multicast groups)
          // set new timeout for reconnecting

          startTicks = currentTime + openConnectionSleepTime;
          endTicks = currentTime + openConnectionSleepTime + openConnectionTimeout;
          totalEndTicks = caller->startReceivingData ? endTicks : (caller->activeProtocol->IsLiveStreamSpecified() ? UINT_MAX : (startTicks + totalReopenConnectionTimeout));

          caller->logger->Log(LOGGER_WARNING, L"%s: %s: connection closed, trying to open, current time: %u, re-open time: %u (%u ms), re-open timeout: %u (ms), maximum re-open time: %u (%u ms)", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME, currentTime, startTicks, startTicks - currentTime, openConnectionTimeout, totalEndTicks, totalEndTicks - currentTime);
        }

        if ((currentTime >= startTicks) && (currentTime < endTicks))
        {
          CParameterCollection *parameters = new CParameterCollection(&result);
          wchar_t *finishTimeString = FormatString(L"%u", caller->startReceivingData ? caller->finishTime : endTicks);

          if ((parameters != NULL) && (finishTimeString != NULL))
          {
            parameters->Add(PARAMETER_NAME_FINISH_TIME, finishTimeString);
          }

          result = caller->activeProtocol->StartReceivingData(parameters);

          FREE_MEM(finishTimeString);
          FREE_MEM_CLASS(parameters);

          CHECK_CONDITION_EXECUTE(openedConnection, caller->logger->Log(LOGGER_WARNING, L"%s: %s: connection closed, trying to open, re-open timeout: %u (ms)", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME, openConnectionTimeout));
        }
        else if (currentTime >= totalEndTicks)
        {
          caller->logger->Log(LOGGER_ERROR, L"%s: %s: maximum time of re-opening connection reached, maximum re-open time: %u (ms)", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME, openConnectionTimeout);

          // instead of error we stop receiving data in protocol
          caller->activeProtocol->StopReceivingData();

          // we also set end of stream, whole stream downloaded and also special flag that connection was lost
          caller->activeProtocol->SetFlags(caller->activeProtocol->GetFlags() | PROTOCOL_PLUGIN_FLAG_CONNECTION_LOST_CANNOT_REOPEN | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED | PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED);
        }
        else if (currentTime >= endTicks)
        {
          caller->logger->Log(LOGGER_ERROR, L"%s: %s: time of re-opening connection reached, re-open timeout: %u (ms)", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME, openConnectionTimeout);

          // instead of error we stop receiving data in protocol
          caller->activeProtocol->StopReceivingData();

          // some protocols may need some sleep before loading (e.g. multicast UDP protocol needs some time between unsubscribing and subscribing in multicast groups)
          // set new timeout for reconnecting

          startTicks = currentTime + openConnectionSleepTime;
          endTicks = currentTime + openConnectionSleepTime + openConnectionTimeout;

          caller->logger->Log(LOGGER_WARNING, L"%s: %s: connection closed, trying to open, current time: %u, re-open time: %u (%u ms), re-open timeout: %u (ms), maximum re-open time: %u (%u ms)", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME, currentTime, startTicks, startTicks - currentTime, openConnectionTimeout, totalEndTicks, totalEndTicks - currentTime);
        }

        // we don't have opened connection
        openedConnection = false;
      }
      else if (caller->activeProtocol->IsConnectionLostCannotReopen() ||
                (connectionState == Initializing) ||
                (connectionState == Opening) ||
                (connectionState == Opened) ||
                (connectionState == Closing))
      {
        if ((!caller->activeProtocol->IsWholeStreamDownloaded()) && (openedConnection) && (connectionState == Closing))
        {
          // we had opened connection, we lost it - protocol is closing connection
          // some protocols may need some sleep before loading (e.g. multicast UDP protocol needs some time between unsubscribing and subscribing in multicast groups)
          // set new timeout for reconnecting

          startTicks = currentTime + openConnectionSleepTime;
          endTicks = currentTime + openConnectionSleepTime + openConnectionTimeout;
          totalEndTicks = caller->startReceivingData ? endTicks : (caller->activeProtocol->IsLiveStreamSpecified() ? UINT_MAX : (startTicks + totalReopenConnectionTimeout));

          caller->logger->Log(LOGGER_WARNING, L"%s: %s: connection opened, trying to close and open, current time: %u, re-open time: %u (%u ms), re-open timeout: %u (ms), maximum re-open time: %u (%u ms)", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME, currentTime, startTicks, startTicks - currentTime, openConnectionTimeout, totalEndTicks, totalEndTicks - currentTime);

          openedConnection = false;
        }

        if ((!caller->activeProtocol->IsWholeStreamDownloaded()) && (currentTime >= totalEndTicks) && (connectionState == Closing))
        {
          caller->logger->Log(LOGGER_ERROR, L"%s: %s: maximum time of closing and opening connection reached", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME);

          // instead of error we stop receiving data in protocol
          caller->activeProtocol->StopReceivingData();

          // we also set end of stream, whole stream downloaded and also special flag that connection was lost
          caller->activeProtocol->SetFlags(caller->activeProtocol->GetFlags() | PROTOCOL_PLUGIN_FLAG_CONNECTION_LOST_CANNOT_REOPEN | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED | PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED);
        }

        openedConnection |= (connectionState == Opened);

        // we have opened connection, we can process requests (if any)
        // if no request available, we call ReceiveData() with temporary dummy request (we don't request any data)

        // don't wait too long
        CLockMutex lock(caller->mutex, 20);

        if (lock.IsLocked())
        {
          CStreamPackage *package = NULL;

          if ((caller->pauseSeekStopMode != PAUSE_SEEK_STOP_MODE_DISABLE_READING) && (caller->streamPackages->Count() != 0))
          {
            // we have opened connection, we can process requests (if any)

            for (unsigned int i = 0; (SUCCEEDED(result) && (i < caller->streamPackages->Count())); i++)
            {
              result = caller->activeProtocol->ReceiveData(caller->streamPackages->GetItem(i));
            }
          }
          else
          {
            tempStreamPackage->Clear();

            // we don't have any request, we can process only dummy request
            result = caller->activeProtocol->ReceiveData(tempStreamPackage);
          }

          if (FAILED(result))
          {
            caller->protocolError = result;
            caller->logger->Log(LOGGER_ERROR, L"%s: %s: protocol returned error: 0x%08X", caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME, result);
          }
        }
      }
    }
  }

  FREE_MEM_CLASS(tempStreamPackage);

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, caller->hosterName, METHOD_RECEIVE_DATA_WORKER_NAME);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}