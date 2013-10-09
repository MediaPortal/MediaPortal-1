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

#include "ParserHoster.h"
#include "ErrorCodes.h"
#include "LockMutex.h"
#include "Parameters.h"

#include <Shlwapi.h>
#include <Shlobj.h>
#include <process.h>

CParserHoster::CParserHoster(CLogger *logger, CParameterCollection *configuration, IParserOutputStream *parserOutputStream)
  : COutputStreamHoster(logger, configuration, L"ParserHoster", L"mpurlsourcesplitter_parser_*.dll", parserOutputStream)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);

  this->parserOutputStream = parserOutputStream;

  this->protocolHoster = new CProtocolHoster(this->logger, this->configuration);
  if (this->protocolHoster != NULL)
  {
    this->protocolHoster->LoadPlugins();
  }

  this->receiveDataWorkerShouldExit = false;
  this->parsingPlugin = NULL;
  this->parseMediaPackets = true;
  this->setTotalLengthCalled = false;
  this->endOfStreamReachedCalled = false;

  this->hReceiveDataWorkerThread = NULL;
  this->status = STATUS_NONE;

  this->supressData = false;
  this->startReceivingData = false;
  this->finishTime = 0;
  this->lockMutex = CreateMutex(NULL, FALSE, NULL);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
}

CParserHoster::~CParserHoster(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);

  this->StopReceivingData();

  if (this->protocolHoster != NULL)
  {
    this->protocolHoster->RemoveAllPlugins();
    FREE_MEM_CLASS(this->protocolHoster);
  }

  if (this->lockMutex != NULL)
  {
    CloseHandle(this->lockMutex);
    this->lockMutex = NULL;
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);
}

// hoster methods

PluginImplementation *CParserHoster::AllocatePluginsMemory(unsigned int maxPlugins)
{
  return ALLOC_MEM(ParserImplementation, maxPlugins);
}

PluginImplementation *CParserHoster::GetPluginImplementation(unsigned int position)
{
  if ((this->pluginImplementations != NULL) && (position < this->pluginImplementationsCount))
  {
    return (((ParserImplementation *)this->pluginImplementations) + position);
  }

  return NULL;
}

bool CParserHoster::AppendPluginImplementation(HINSTANCE hLibrary, DESTROYPLUGININSTANCE destroyPluginInstance, PIPlugin plugin)
{
  bool result = __super::AppendPluginImplementation(hLibrary, destroyPluginInstance, plugin);
  if (result)
  {
    ParserImplementation *implementation = (ParserImplementation *)this->GetPluginImplementation(this->pluginImplementationsCount - 1);
    implementation->result = ParseResult_Unspecified;
  }
  return result;
}

void CParserHoster::RemovePluginImplementation(void)
{
  __super::RemovePluginImplementation();
}

PluginConfiguration *CParserHoster::GetPluginConfiguration(void)
{
  ALLOC_MEM_DEFINE_SET(pluginConfiguration, ParserPluginConfiguration, 1, 0);
  if (pluginConfiguration != NULL)
  {
    pluginConfiguration->configuration = this->configuration;
  }

  return pluginConfiguration;
}

// IOutputStream interface implementation

HRESULT CParserHoster::SetTotalLength(int64_t total, bool estimate)
{
  if (this->outputStream != NULL)
  {
    if (status == STATUS_RECEIVING_DATA)
    {
      return this->outputStream->SetTotalLength(total, estimate);
    }
    else
    {
      this->total = total;
      this->estimate = estimate;
      this->setTotalLengthCalled = true;
    }

    return S_OK;
  }

  return E_NOT_VALID_STATE;
}

HRESULT CParserHoster::PushMediaPackets(CMediaPacketCollection *mediaPackets)
{
  HRESULT result = E_NOT_VALID_STATE;

  if ((this->parseMediaPackets) && (this->pluginImplementationsCount != 0))
  {
    bool pendingPlugin = false;
    bool pendingPluginsBeforeParsing = false;
    bool drmProtected = false;

    for (unsigned int i = 0; i < this->pluginImplementationsCount; i++)
    {
      ParserImplementation *implementation = (ParserImplementation *)this->GetPluginImplementation(i);
      if (implementation->result == ParseResult_Pending)
      {
        pendingPluginsBeforeParsing = true;
      }
      if (implementation->result == ParseResult_DrmProtected)
      {
        drmProtected = true;
      }
    }

    if (this->parsingPlugin != NULL)
    {
      // is there is plugin which returned ParseResult::Known result
      CParameterCollection *currentConnectionParameters = this->protocolHoster->GetConnectionParameters();
      this->parsingPlugin->ParseMediaPackets(mediaPackets, currentConnectionParameters);
      FREE_MEM_CLASS(currentConnectionParameters);
      result = S_OK;
    }
    else 
    {
      CParameterCollection *currentConnectionParameters = this->protocolHoster->GetConnectionParameters();

      // send received media packet to parsers
      for (unsigned int i = 0; (i < this->pluginImplementationsCount) && (this->parsingPlugin == NULL); i++)
      {
        ParserImplementation *implementation = (ParserImplementation *)this->GetPluginImplementation(i);
        IParserPlugin *plugin = (IParserPlugin *)implementation->pImplementation;

        if ((implementation->result == ParseResult_Unspecified) ||
          (implementation->result == ParseResult_Pending))
        {
          // parse data only in case when parser can process data
          // if parser returned ParseResult::NotKnown result than parser surely 
          // doesn't recognize any pattern in stream

          ParseResult pluginParseResult = plugin->ParseMediaPackets(mediaPackets, currentConnectionParameters);
          implementation->result = pluginParseResult;

          switch(pluginParseResult)
          {
          case ParseResult_Unspecified:
            this->logger->Log(LOGGER_WARNING, L"%s: %s: parser '%s' return unspecified result", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, implementation->name);
            break;
          case ParseResult_NotKnown:
            this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' doesn't recognize any pattern", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, implementation->name);
            break;
          case ParseResult_Pending:
            this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' waits for more data", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, implementation->name);
            pendingPlugin = true;
            break;
          case ParseResult_Known:
            this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' recognizes pattern", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, implementation->name);
            this->parsingPlugin = plugin;
            break;
          case ParseResult_DrmProtected:
            this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' recognizes pattern, DRM protected", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, implementation->name);
            drmProtected = true;
            break;
          default:
            this->logger->Log(LOGGER_WARNING, L"%s: %s: parser '%s' return unknown result", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, implementation->name);
            break;
          }
        }
      }

      FREE_MEM_CLASS(currentConnectionParameters);
    }

    if ((!pendingPlugin) && (this->parsingPlugin == NULL) && (drmProtected))
    {
      // there is no pending plugin, there is no parsing plugin
      // stream is DRM protected
      this->status = E_DRM_PROTECTED;
      result = S_OK;
    }
    else if ((!pendingPlugin) && (this->parsingPlugin == NULL))
    {
      // all parsers don't recognize any pattern in stream
      // do not parse media packets, just send them directly to filter
      this->parseMediaPackets = false;

      this->status = STATUS_RECEIVING_DATA;

      if (pendingPluginsBeforeParsing)
      {
        // we need to resend any store media packets
        CMediaPacketCollection *mediaPacketsToResend = NULL;
        unsigned int mediaPacketsToResendCount = 0;
        for (unsigned int i = 0; i < this->pluginImplementationsCount; i++)
        {
          ParserImplementation *implementation = (ParserImplementation *)this->GetPluginImplementation(i);
          IParserPlugin *plugin = (IParserPlugin *)implementation->pImplementation;

          CMediaPacketCollection *mediaPackets = plugin->GetStoredMediaPackets();
          if (mediaPackets != NULL)
          {
            if (mediaPackets->Count() > mediaPacketsToResendCount)
            {
              mediaPacketsToResendCount = mediaPackets->Count();
              mediaPacketsToResend = mediaPackets;
            }
          }
        }

        if ((mediaPacketsToResend != NULL) && (this->outputStream != NULL))
        {
          result = this->outputStream->PushMediaPackets(mediaPacketsToResend);
          if (FAILED(result))
          {
            this->logger->Log(LOGGER_WARNING, L"%s: %s: resending media packets failed: 0x%08X", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, result);
          }
        }
      }
      else if (this->outputStream != NULL)
      {
        result = this->outputStream->PushMediaPackets(mediaPackets);
      }
    }
    else if (this->parsingPlugin != NULL)
    {
      // there is plugin, which recognize pattern in stream

      Action action = this->parsingPlugin->GetAction();
      const wchar_t *name = this->parsingPlugin->GetName();

      switch (action)
      {
      case Action_Unspecified:
        this->logger->Log(LOGGER_WARNING, L"%s: %s: parser '%s' returns unspecified action", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, name);
        result = E_FAIL;
        break;
      case Action_GetNewConnection:
        this->status = STATUS_NEW_URL_SPECIFIED;
        this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' specifies new connection", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, name);
        result = S_OK;
        break;
      default:
        this->logger->Log(LOGGER_WARNING, L"%s: %s: parser '%s' returns unknown action", MODULE_PARSER_HOSTER_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, name);
        result = E_FAIL;
        break;
      }
    }
    else
    {
      // there is pending plugin
      this->status = STATUS_PARSER_PENDING;
      result = S_OK;
    }
  }
  else
  {
    this->status = STATUS_RECEIVING_DATA;

    if (this->outputStream != NULL)
    {
      result = this->outputStream->PushMediaPackets(mediaPackets);
    }
  }

  return result;
}

HRESULT CParserHoster::EndOfStreamReached(int64_t streamPosition)
{
  if (this->outputStream != NULL)
  {
    if (status == STATUS_RECEIVING_DATA)
    {
      return this->outputStream->EndOfStreamReached(streamPosition);
    }
    else
    {
      this->streamPosition = streamPosition;
      this->endOfStreamReachedCalled = true;
    }

    return S_OK;
  }

  return E_NOT_VALID_STATE;
}

// ISimpleProtocol implementation

unsigned int CParserHoster::GetReceiveDataTimeout(void)
{
  return (this->protocolHoster != NULL) ? this->protocolHoster->GetReceiveDataTimeout() : UINT_MAX;
}

HRESULT CParserHoster::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT retval = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(retval, parameters);
  CHECK_POINTER_DEFAULT_HRESULT(retval, this->protocolHoster);

  if (SUCCEEDED(retval))
  {
    CParameterCollection *urlConnection = new CParameterCollection();
    CHECK_POINTER_HRESULT(retval, urlConnection, S_OK, E_INVALID_CONFIGURATION);

    if (SUCCEEDED(retval))
    {
      urlConnection->Append((CParameterCollection *)parameters);
      bool newUrlSpecified = false;

      do
      {
        this->status = STATUS_NONE;
        newUrlSpecified = false;

        this->setTotalLengthCalled = false;
        this->endOfStreamReachedCalled = false;

        // clear all protocol plugins and parse url connection
        // the first thing of ParseUrl() is call to ClearSession()
        retval = this->protocolHoster->ParseUrl(urlConnection);

        if (SUCCEEDED(retval))
        {
          for (unsigned int i = 0; i < this->pluginImplementationsCount; i++)
          {
            ParserImplementation *implementation = (ParserImplementation *)this->GetPluginImplementation(i);
            IParserPlugin *plugin = (IParserPlugin *)implementation->pImplementation;

            // clear parser session and notify about new url and parameters
            plugin->ClearSession();
            plugin->SetConnectionParameters(urlConnection);
          }
        }

        DWORD timeout = 0;

        if (SUCCEEDED(retval))
        {
          // get receive data timeout for active protocol
          timeout = this->protocolHoster->GetReceiveDataTimeout();
          this->finishTime = GetTickCount() + timeout;

          // now we have active protocol with loaded url, but still not working
          // create thread for receiving data

          this->startReceivingData = true;
          retval = this->CreateReceiveDataWorker();
        }

        if (SUCCEEDED(retval))
        {          
          const wchar_t *protocolName = this->protocolHoster->GetName();
          if (protocolName != NULL)
          {
            this->logger->Log(LOGGER_INFO, L"%s: %s: active protocol '%s' timeout: %d (ms)", this->moduleName, METHOD_START_RECEIVING_DATA_NAME, protocolName, timeout);
          }
          else
          {
            this->logger->Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, this->moduleName, METHOD_START_RECEIVING_DATA_NAME, L"no active protocol");
            retval = E_NO_ACTIVE_PROTOCOL;
          }

          if (SUCCEEDED(retval))
          {
            // wait for receiving data, timeout or exit
            while ((this->status != STATUS_NEW_URL_SPECIFIED) && (this->status != STATUS_RECEIVING_DATA) && (this->status >= STATUS_NONE) && (GetTickCount() <= this->finishTime) && (!this->receiveDataWorkerShouldExit))
            {
              Sleep(1);
            }

            switch(this->status)
            {
            case STATUS_NONE:
              retval = E_NO_DATA_AVAILABLE;
              break;
            case STATUS_RECEIVING_DATA:
              retval = S_OK;
              break;
            case STATUS_PARSER_PENDING:
              retval = E_PARSER_STILL_PENDING;
              break;
            case STATUS_NEW_URL_SPECIFIED:
              this->DestroyReceiveDataWorker();
              newUrlSpecified = true;
              retval = S_OK;
              break;
            default:
              retval = this->status;
              break;
            }

            if (FAILED(retval))
            {
              this->StopReceivingData();
              //this->DestroyReceiveDataWorker();
            }

            if (this->status == STATUS_NEW_URL_SPECIFIED)
            {
              // known plugin will be cleared in StopReceivingData()
              IParserPlugin *plugin = this->parsingPlugin;
              this->StopReceivingData();
              urlConnection->Clear();
              retval = plugin->GetConnectionParameters(urlConnection);
            }
            else
            {
              // stop cycle
              break;
            }
          }
        }

        this->startReceivingData = false;
      } while ((newUrlSpecified) && SUCCEEDED(retval));
    }

    FREE_MEM_CLASS(urlConnection);
  }

  if (SUCCEEDED(retval))
  {
    // call SetTotalLength() or EndOfStreamReached() if there is need

    if (this->setTotalLengthCalled)
    {
      this->SetTotalLength(this->total, this->estimate);
    }

    if (this->endOfStreamReachedCalled)
    {
      this->EndOfStreamReached(this->streamPosition);
    }
  }

  return retval;
}

HRESULT CParserHoster::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->moduleName, METHOD_STOP_RECEIVING_DATA_NAME);

  // stop receive data worker
  this->DestroyReceiveDataWorker();

  // stop receiving data
  this->protocolHoster->StopReceivingData();
  this->parsingPlugin = NULL;
  this->parseMediaPackets = true;

  for (unsigned int i = 0; i < this->pluginImplementationsCount; i++)
  {
    ParserImplementation *implementation = (ParserImplementation *)this->GetPluginImplementation(i);
    implementation->result = ParseResult_Unspecified;
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->moduleName, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CParserHoster::QueryStreamProgress(LONGLONG *total, LONGLONG *current)
{
  HRESULT result = E_NOT_VALID_STATE;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    result = (this->protocolHoster != NULL) ? this->protocolHoster->QueryStreamProgress(total, current) : E_NOT_VALID_STATE;
  }

  return result;
}
  
HRESULT CParserHoster::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
{
  HRESULT result = E_NOT_VALID_STATE;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    result = (this->protocolHoster != NULL) ? this->protocolHoster->QueryStreamAvailableLength(availableLength) : E_NOT_VALID_STATE;
  }

  return result;
}

HRESULT CParserHoster::ClearSession(void)
{
  // stop receiving data
  this->protocolHoster->StopReceivingData();

  if (this->pluginImplementations != NULL)
  {
    for(unsigned int i = 0; i < this->pluginImplementationsCount; i++)
    {
      ParserImplementation *parserImplementation = (ParserImplementation *)this->GetPluginImplementation(i);
      parserImplementation->result = ParseResult_Unspecified;

      this->logger->Log(LOGGER_INFO, L"%s: %s: reseting parser: %s", this->moduleName, METHOD_CLEAR_SESSION_NAME, parserImplementation->name);

      if (parserImplementation->pImplementation != NULL)
      {
        IParserPlugin *parser = (IParserPlugin *)parserImplementation->pImplementation;
        parser->ClearSession();
      }
    }
  }

  // reset all protocol implementations
  this->protocolHoster->ClearSession();

  this->setTotalLengthCalled = false;
  this->endOfStreamReachedCalled = false;
  return S_OK;
}

int64_t CParserHoster::GetDuration(void)
{
  return (this->protocolHoster != NULL) ? this->protocolHoster->GetDuration() : DURATION_UNSPECIFIED;
}

void CParserHoster::ReportStreamTime(uint64_t streamTime)
{
  if (this->protocolHoster != NULL)
  {
    this->protocolHoster->ReportStreamTime(streamTime);
  }
}

// ISeeking interface implementation

unsigned int CParserHoster::GetSeekingCapabilities(void)
{
  return (this->protocolHoster != NULL) ? this->protocolHoster->GetSeekingCapabilities() : SEEKING_METHOD_NONE;
}

int64_t CParserHoster::SeekToTime(int64_t time)
{
  return (this->protocolHoster != NULL) ? this->protocolHoster->SeekToTime(time) : E_NOT_VALID_STATE;
}

int64_t CParserHoster::SeekToPosition(int64_t start, int64_t end)
{
  return (this->protocolHoster != NULL) ? this->protocolHoster->SeekToPosition(start, end) : E_NOT_VALID_STATE;
}

void CParserHoster::SetSupressData(bool supressData)
{
  // supress data can be set only when not receiving data in ReceiveDataWorker()
  CLockMutex lock(this->lockMutex, INFINITE);

  this->supressData = supressData;
  if (this->protocolHoster != NULL)
  {
    this->protocolHoster->SetSupressData(supressData);
  }
}

HRESULT CParserHoster::CreateReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->moduleName, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME);

  this->hReceiveDataWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CParserHoster::ReceiveDataWorker, this, 0, NULL);

  if (this->hReceiveDataWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", this->moduleName, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->moduleName, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

HRESULT CParserHoster::DestroyReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->moduleName, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME);

  this->receiveDataWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->hReceiveDataWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->hReceiveDataWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, this->moduleName, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->hReceiveDataWorkerThread, 0);
    }
    CloseHandle(this->hReceiveDataWorkerThread);
  }

  this->hReceiveDataWorkerThread = NULL;
  this->receiveDataWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->moduleName, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

unsigned int WINAPI CParserHoster::ReceiveDataWorker(LPVOID lpParam)
{
  CParserHoster *caller = (CParserHoster *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, caller->moduleName, METHOD_RECEIVE_DATA_WORKER_NAME);

  bool openedConnection = false;
  bool stopReceivingData = false;

  HRESULT result = S_OK;
  CReceiveData *receiveData = NULL;
  unsigned int maximumReopenTime = 0;
  DWORD endTicks = GetTickCount();

  while ((!caller->receiveDataWorkerShouldExit) && (!stopReceivingData))
  {
    if (receiveData == NULL)
    {
      receiveData = new CReceiveData();
    }

    Sleep(1);

    if (caller->protocolHoster != NULL)
    {
      if (maximumReopenTime == 0)
      {
        maximumReopenTime = caller->protocolHoster->GetReceiveDataTimeout();
        endTicks = GetTickCount() + maximumReopenTime;
      }

      if (maximumReopenTime != 0)
      {
        // if in active protocol is opened connection than receive data
        // if not than open connection
        if (caller->protocolHoster->IsConnected())
        {
          CLockMutex lock(caller->lockMutex, INFINITE);

          if ((!caller->supressData) && (receiveData != NULL))
          {
            receiveData->Clear();
            result = caller->protocolHoster->ReceiveData(receiveData);

            if (SUCCEEDED(result))
            {
              if (receiveData->GetTotalLength()->IsSet())
              {
                caller->SetTotalLength(receiveData->GetTotalLength()->GetTotalLength(), receiveData->GetTotalLength()->IsEstimate());
              }

              if (receiveData->GetMediaPacketCollection()->Count() != 0)
              {
                // we are receiving data
                openedConnection = true;

                caller->PushMediaPackets(receiveData->GetMediaPacketCollection());
              }

              if (receiveData->GetEndOfStreamReached()->IsSet())
              {
                caller->EndOfStreamReached(receiveData->GetEndOfStreamReached()->GetStreamPosition());
              }
            }

            if (FAILED(result))
            {
              caller->logger->Log(LOGGER_ERROR, L"%s: %s: protocol returned error: 0x%08X", caller->moduleName, METHOD_RECEIVE_DATA_WORKER_NAME, result);
              caller->status = result;
              stopReceivingData = true;
            }
          }
        }
        else if (!caller->supressData)
        {
          if (openedConnection)
          {
            // we had opened connection, we lost it
            // set timeout for reconnecting
            endTicks = GetTickCount() + maximumReopenTime;
          }

          if (GetTickCount() < endTicks)
          {
            CParameterCollection *parameters = new CParameterCollection();
            wchar_t *finishTimeString = FormatString(L"%u", caller->startReceivingData ? caller->finishTime : endTicks);

            if ((parameters != NULL) && (finishTimeString != NULL))
            {
              parameters->Add(PARAMETER_NAME_FINISH_TIME, finishTimeString);
            }

            result = caller->protocolHoster->StartReceivingData(parameters);

            FREE_MEM(finishTimeString);
            FREE_MEM_CLASS(parameters);

            CHECK_CONDITION_EXECUTE(openedConnection, caller->logger->Log(LOGGER_WARNING, L"%s: %s: connection closed, trying to open, maximum re-open time: %u (ms)", caller->moduleName, METHOD_RECEIVE_DATA_WORKER_NAME, maximumReopenTime));
          }
          else
          {
            caller->logger->Log(LOGGER_ERROR, L"%s: %s: maximum time of re-opening connection reached, maximum re-open time: %u (ms)", caller->moduleName, METHOD_RECEIVE_DATA_WORKER_NAME, maximumReopenTime);
            result = (caller->status == STATUS_RECEIVING_DATA) ? E_CONNECTION_LOST_CANNOT_REOPEN : result;
            caller->status = result;
            stopReceivingData = true;
          }

          // we don't have opened connection
          openedConnection = false;
        }
      }
    }
  }
  FREE_MEM_CLASS(receiveData);

  // signalize end of download with result, if needed
  if ((caller->status != STATUS_NEW_URL_SPECIFIED) && (caller->parserOutputStream->IsDownloading()))
  {
    caller->parserOutputStream->FinishDownload(result);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, caller->moduleName, METHOD_RECEIVE_DATA_WORKER_NAME);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CParserHoster::GetParserHosterStatus(void)
{
  return this->status;
}