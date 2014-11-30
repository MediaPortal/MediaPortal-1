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
#include "ParserHosterPluginMetadata.h"
#include "ParserPluginConfiguration.h"

#include <Shlwapi.h>
#include <Shlobj.h>
#include <process.h>

CParserHoster::CParserHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CHoster(result, logger, configuration, L"ParserHoster", L"mpurlsourcesplitter_parser_*.dll")
{
  this->protocolHoster = NULL;
  this->activeParser = NULL;
  this->startReceiveDataWorkerShouldExit = false;
  this->startReceiveDataWorkerThread = NULL;
  this->parserError = S_OK;
  this->startReceiveDataParameters = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME, this);
    
    this->protocolHoster = new CProtocolHoster(result, logger, configuration);

    CHECK_POINTER_HRESULT(*result, this->protocolHoster, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(*result), this->protocolHoster->LoadPlugins(), *result);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CParserHoster::~CParserHoster(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));

  this->StopReceivingData();

  if (this->protocolHoster != NULL)
  {
    FREE_MEM_CLASS(this->protocolHoster);
  }
  
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_PARSER_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));
}

// ISeeking interface implementation

unsigned int CParserHoster::GetSeekingCapabilities(void)
{
  return (this->activeParser != NULL) ? this->activeParser->GetSeekingCapabilities() : SEEKING_METHOD_NONE;
}

int64_t CParserHoster::SeekToTime(unsigned int streamId, int64_t time)
{
  return (this->activeParser != NULL) ? this->activeParser->SeekToTime(streamId, time) : E_NOT_VALID_STATE;
}

void CParserHoster::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->activeParser, this->activeParser->SetPauseSeekStopMode(pauseSeekStopMode));
}

// ISimpleProtocol implementation

unsigned int CParserHoster::GetOpenConnectionTimeout(void)
{
  return (this->activeParser != NULL) ? this->activeParser->GetOpenConnectionTimeout() : UINT_MAX;
}

unsigned int CParserHoster::GetOpenConnectionSleepTime(void)
{
  return (this->activeParser != NULL) ? this->activeParser->GetOpenConnectionSleepTime() : 0;
}

unsigned int CParserHoster::GetTotalReopenConnectionTimeout(void)
{
  return (this->activeParser != NULL) ? this->activeParser->GetTotalReopenConnectionTimeout() : UINT_MAX;
}

HRESULT CParserHoster::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->protocolHoster);

  if (SUCCEEDED(result))
  {
    do
    {
      result = this->StartReceivingDataAsync(parameters);

      if (result == S_FALSE)
      {
        Sleep(1);
      }
    }
    while (result == S_FALSE);
  }

  return result;
}

HRESULT CParserHoster::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_STOP_RECEIVING_DATA_NAME);

  // stop start receive data worker
  this->DestroyStartReceiveDataWorker();

  // stop receiving data
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->activeParser, this->activeParser->StopReceivingData());
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->protocolHoster, this->protocolHoster->StopReceivingData());

  this->activeParser = NULL;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->hosterName, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CParserHoster::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return (this->activeParser != NULL) ? this->activeParser->QueryStreamProgress(streamProgress) : E_NOT_VALID_STATE;
}
  
void CParserHoster::ClearSession(void)
{
  // stop receiving data
  this->StopReceivingData();

  CHoster::ClearSession();

  // reset all protocol implementations
  this->protocolHoster->ClearSession();

  this->parserError = S_OK;
  this->startReceiveDataParameters = NULL;
}

int64_t CParserHoster::GetDuration(void)
{
  return (this->activeParser != NULL) ? this->activeParser->GetDuration() : DURATION_UNSPECIFIED;
}

void CParserHoster::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  if (this->activeParser != NULL)
  {
    this->activeParser->ReportStreamTime(streamTime, streamPosition);
  }
}

HRESULT CParserHoster::GetStreamInformation(CStreamInformationCollection *streams)
{
  return (this->activeParser != NULL) ? this->activeParser->GetStreamInformation(streams) : E_NO_ACTIVE_PARSER;
}

// IDemuxerOwner interface

HRESULT CParserHoster::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  return (this->activeParser != NULL) ? this->activeParser->ProcessStreamPackage(streamPackage) : E_NOT_VALID_STATE;
}

/* other methods */

HRESULT CParserHoster::LoadPlugins(void)
{
  HRESULT result = __super::LoadPlugins();
  CHECK_CONDITION_HRESULT(result, this->hosterPluginMetadataCollection->Count() != 0, result, E_NO_PARSER_LOADED);

  CHECK_CONDITION_EXECUTE(result == E_NO_PARSER_LOADED, this->logger->Log(LOGGER_ERROR, L"%s: %s: no parser loaded", this->hosterName, METHOD_LOAD_PLUGINS_NAME));
  return result;
}

HRESULT CParserHoster::StartReceivingDataAsync(CParameterCollection *parameters)
{
  HRESULT result = S_FALSE;

  if (this->startReceiveDataWorkerThread == NULL)
  {
    this->startReceiveDataParameters = parameters;

    result = this->CreateStartReceiveDataWorker();
    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = S_FALSE);
  }

  if (SUCCEEDED(result) && (this->startReceiveDataWorkerThread != NULL))
  {
    result = (WaitForSingleObject(this->startReceiveDataWorkerThread, 0) == WAIT_TIMEOUT) ? S_FALSE : this->parserError;
  }
  
  if (result != S_FALSE)
  {
    // thread finished or error
    this->DestroyStartReceiveDataWorker();

    this->startReceiveDataParameters = NULL;
  }

  return result;
}

/* protected methods */

CHosterPluginMetadata *CParserHoster::CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
{
  CParserHosterPluginMetadata *parserMetadata = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    parserMetadata = new CParserHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName);
    CHECK_POINTER_HRESULT(*result, parserMetadata, *result, E_OUTOFMEMORY);
  
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(parserMetadata));
  }

  return parserMetadata;
}

CPluginConfiguration *CParserHoster::CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration)
{
  CParserPluginConfiguration *parserConfiguration = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    parserConfiguration = new CParserPluginConfiguration(result, configuration, this->protocolHoster);
    CHECK_POINTER_HRESULT(*result, parserConfiguration, *result, E_OUTOFMEMORY);
  
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(parserConfiguration));
  }

  return parserConfiguration;
}

HRESULT CParserHoster::CreateStartReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_CREATE_START_RECEIVE_DATA_WORKER_NAME);

  if (this->startReceiveDataWorkerThread == NULL)
  {
    this->startReceiveDataWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CParserHoster::StartReceiveDataWorker, this, 0, NULL);
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

HRESULT CParserHoster::DestroyStartReceiveDataWorker(void)
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

unsigned int WINAPI CParserHoster::StartReceiveDataWorker(LPVOID lpParam)
{
  CParserHoster *caller = (CParserHoster *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, caller->hosterName, METHOD_START_RECEIVE_DATA_WORKER_NAME);

  if (SUCCEEDED(caller->parserError))
  {
    CParameterCollection *urlConnection = new CParameterCollection(&caller->parserError);
    CHECK_POINTER_HRESULT(caller->parserError, urlConnection, caller->parserError, E_INVALID_CONFIGURATION);

    if (SUCCEEDED(caller->parserError))
    {
      urlConnection->Append((CParameterCollection *)caller->startReceiveDataParameters);
      bool newUrlSpecified = false;

      do
      {
        newUrlSpecified = false;

        // clear all protocol plugins and parse url connection
        // the first thing of ParseUrl() is call to ClearSession()
        caller->parserError = caller->protocolHoster->ParseUrl(urlConnection);

        if (SUCCEEDED(caller->parserError))
        {
          do
          {
            caller->parserError = caller->protocolHoster->StartReceivingDataAsync(caller->startReceiveDataParameters);

            if (caller->parserError == S_FALSE)
            {
              Sleep(1);
            }
          }
          while ((!caller->startReceiveDataWorkerShouldExit) && (caller->parserError == S_FALSE));

          CHECK_CONDITION_HRESULT(caller->parserError, !caller->startReceiveDataWorkerShouldExit, caller->parserError, E_CONNECTION_LOST_CANNOT_REOPEN);
        }

        if (SUCCEEDED(caller->parserError))
        {
          for (unsigned int i = 0; i < caller->hosterPluginMetadataCollection->Count(); i++)
          {
            CParserHosterPluginMetadata *metadata = dynamic_cast<CParserHosterPluginMetadata *>(caller->hosterPluginMetadataCollection->GetItem(i));
            CParserPlugin *parser = (CParserPlugin *)metadata->GetPlugin();

            // clear parser session and notify about new url and parameters
            metadata->ClearSession();

            parser->ClearSession();
            caller->parserError = parser->SetConnectionParameters(urlConnection);
          }
        }

        if (SUCCEEDED(caller->parserError))
        {
          // we are receiving data, we can try parsers

          bool pendingParser = true;
          unsigned int endTicks = GetTickCount() + caller->protocolHoster->GetOpenConnectionSleepTime() + caller->protocolHoster->GetOpenConnectionTimeout();

          while (SUCCEEDED(caller->parserError) && pendingParser && (GetTickCount() < endTicks))
          {
            // check if there is any pending parser
            
            pendingParser = false;
            for (unsigned int i = 0; (SUCCEEDED(caller->parserError) && (i < caller->hosterPluginMetadataCollection->Count())); i++)
            {
              CParserHosterPluginMetadata *metadata = (CParserHosterPluginMetadata *)caller->hosterPluginMetadataCollection->GetItem(i);

              if (metadata->IsParserStillPending())
              {
                HRESULT parserResult = metadata->GetParserResult();

                switch(parserResult)
                {
                case PARSER_RESULT_PENDING:
                  pendingParser = true;
                  break;
                case PARSER_RESULT_NOT_KNOWN:
                  caller->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' doesn't recognize stream", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVE_DATA_WORKER_NAME, metadata->GetPlugin()->GetName());
                  break;
                case PARSER_RESULT_KNOWN:
                  caller->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' recognizes stream, score: %u", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVE_DATA_WORKER_NAME, metadata->GetPlugin()->GetName(), metadata->GetParserScore());
                  break;
                case PARSER_RESULT_DRM_PROTECTED:
                  caller->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' recognizes pattern, DRM protected", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVE_DATA_WORKER_NAME, metadata->GetPlugin()->GetName());
                  break;
                default:
                  caller->logger->Log(LOGGER_WARNING, L"%s: %s: parser '%s' returns error: 0x%08X", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVE_DATA_WORKER_NAME, metadata->GetPlugin()->GetName(), parserResult);
                  caller->parserError = parserResult;
                  break;
                }
              }
            }

            // sleep some time, get other threads chance to work
            if (SUCCEEDED(caller->parserError) && pendingParser)
            {
              Sleep(1);
            }
          }

          if (SUCCEEDED(caller->parserError) && pendingParser)
          {
            // timeout reached, some parser(s) is (are) still pending
            for (unsigned int i = 0; (SUCCEEDED(caller->parserError) && (i < caller->hosterPluginMetadataCollection->Count())); i++)
            {
              CParserHosterPluginMetadata *metadata = (CParserHosterPluginMetadata *)caller->hosterPluginMetadataCollection->GetItem(i);

              if (metadata->IsParserStillPending())
              {
                caller->logger->Log(LOGGER_ERROR, L"%s: %s: parser '%s' still pending", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVE_DATA_WORKER_NAME, metadata->GetPlugin()->GetName());
              }
            }

            caller->parserError = E_PARSER_STILL_PENDING;
          }

          if (SUCCEEDED(caller->parserError))
          {
            // we don't have timeout, also no pending parser
            // find parser with highest score and execute its action (specify new URL or set as active parser)

            unsigned int highestScore = 0;
            CParserPlugin *highestScoreParser = NULL;

            for (unsigned int i = 0; (SUCCEEDED(caller->parserError) && (i < caller->hosterPluginMetadataCollection->Count())); i++)
            {
              CParserHosterPluginMetadata *metadata = (CParserHosterPluginMetadata *)caller->hosterPluginMetadataCollection->GetItem(i);

              if ((metadata->GetParserResult() == PARSER_RESULT_KNOWN) && (metadata->GetParserScore() > highestScore))
              {
                highestScore = metadata->GetParserScore();
                highestScoreParser = dynamic_cast<CParserPlugin *>(metadata->GetPlugin());
              }
            }

            switch(highestScoreParser->GetAction())
            {
            case CParserPlugin::ParseStream:
              caller->activeParser = highestScoreParser;
              break;
            case CParserPlugin::GetNewConnection:
              newUrlSpecified = true;

              // stop receiving data
              CHECK_CONDITION_NOT_NULL_EXECUTE(caller->activeParser, caller->activeParser->StopReceivingData());
              CHECK_CONDITION_NOT_NULL_EXECUTE(caller->protocolHoster, caller->protocolHoster->StopReceivingData());

              caller->activeParser = NULL;

              urlConnection->Clear();
              caller->parserError = highestScoreParser->GetConnectionParameters(urlConnection);
              break;
            }
          }
        }
        
        CHECK_CONDITION_EXECUTE(FAILED(caller->parserError), caller->protocolHoster->StopReceivingData());
      }
      while ((!caller->startReceiveDataWorkerShouldExit) && (newUrlSpecified) && SUCCEEDED(caller->parserError));
    }

    FREE_MEM_CLASS(urlConnection);
  }

  CHECK_POINTER_HRESULT(caller->parserError, caller->activeParser, caller->parserError, E_NO_ACTIVE_PARSER);

  if (SUCCEEDED(caller->parserError))
  {
    caller->logger->Log(LOGGER_INFO, L"%s: %s: active parser: '%s'", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVE_DATA_WORKER_NAME, caller->activeParser->GetName());

    caller->parserError = caller->activeParser->StartReceivingData(caller->startReceiveDataParameters);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, caller->hosterName, METHOD_START_RECEIVE_DATA_WORKER_NAME);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}