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

CParserHoster::CParserHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CHoster(result, logger, configuration, L"ParserHoster", L"mpurlsourcesplitter_parser_*.dll")
{
  this->protocolHoster = NULL;
  this->activeParser = NULL;

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
    CParameterCollection *urlConnection = new CParameterCollection(&result);
    CHECK_POINTER_HRESULT(result, urlConnection, result, E_INVALID_CONFIGURATION);

    if (SUCCEEDED(result))
    {
      urlConnection->Append((CParameterCollection *)parameters);
      bool newUrlSpecified = false;

      do
      {
        newUrlSpecified = false;

        // clear all protocol plugins and parse url connection
        // the first thing of ParseUrl() is call to ClearSession()
        result = this->protocolHoster->ParseUrl(urlConnection);

        if (SUCCEEDED(result))
        {
          result = this->protocolHoster->StartReceivingData(parameters);
        }

        if (SUCCEEDED(result))
        {
          for (unsigned int i = 0; i < this->hosterPluginMetadataCollection->Count(); i++)
          {
            CParserHosterPluginMetadata *metadata = dynamic_cast<CParserHosterPluginMetadata *>(this->hosterPluginMetadataCollection->GetItem(i));
            CParserPlugin *parser = (CParserPlugin *)metadata->GetPlugin();

            // clear parser session and notify about new url and parameters
            metadata->ClearSession();

            parser->ClearSession();
            result = parser->SetConnectionParameters(urlConnection);
          }
        }

        if (SUCCEEDED(result))
        {
          // we are receiving data, we can try parsers

          bool pendingParser = true;
          unsigned int endTicks = GetTickCount() + this->protocolHoster->GetOpenConnectionSleepTime() + this->protocolHoster->GetOpenConnectionTimeout();

          while (SUCCEEDED(result) && pendingParser && (GetTickCount() < endTicks))
          {
            // check if there is any pending parser
            
            pendingParser = false;
            for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->hosterPluginMetadataCollection->Count())); i++)
            {
              CParserHosterPluginMetadata *metadata = (CParserHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);

              if (metadata->IsParserStillPending())
              {
                CParserPlugin::ParserResult parserResult = metadata->GetParserResult();

                switch(parserResult)
                {
                case CParserPlugin::Pending:
                  pendingParser = true;
                  break;
                case CParserPlugin::NotKnown:
                  this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' doesn't recognize stream", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVING_DATA_NAME, metadata->GetPlugin()->GetName());
                  break;
                case CParserPlugin::Known:
                  this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' recognizes stream, score: %u", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVING_DATA_NAME, metadata->GetPlugin()->GetName(), metadata->GetParserScore());
                  break;
                case CParserPlugin::DrmProtected:
                  this->logger->Log(LOGGER_INFO, L"%s: %s: parser '%s' recognizes pattern, DRM protected", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVING_DATA_NAME, metadata->GetPlugin()->GetName());
                  break;
                default:
                  this->logger->Log(LOGGER_WARNING, L"%s: %s: parser '%s' returns unknown result", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVING_DATA_NAME, metadata->GetPlugin()->GetName());
                  break;
                }
              }
            }

            // sleep some time, get other threads chance to work
            if (SUCCEEDED(result) && pendingParser)
            {
              Sleep(1);
            }
          }

          if (SUCCEEDED(result) && pendingParser)
          {
            // timeout reached, some parser(s) is (are) still pending
            for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->hosterPluginMetadataCollection->Count())); i++)
            {
              CParserHosterPluginMetadata *metadata = (CParserHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);

              if (metadata->IsParserStillPending())
              {
                this->logger->Log(LOGGER_ERROR, L"%s: %s: parser '%s' still pending", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVING_DATA_NAME, metadata->GetPlugin()->GetName());
              }
            }

            result = E_PARSER_STILL_PENDING;
          }

          if (SUCCEEDED(result))
          {
            // we don't have timeout, also no pending parser
            // find parser with highest score and execute its action (specify new URL or set as active parser)

            unsigned int highestScore = 0;
            CParserPlugin *highestScoreParser = NULL;

            for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->hosterPluginMetadataCollection->Count())); i++)
            {
              CParserHosterPluginMetadata *metadata = (CParserHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);

              if ((metadata->GetParserResult() == CParserPlugin::Known) && (metadata->GetParserScore() > highestScore))
              {
                highestScore = metadata->GetParserScore();
                highestScoreParser = dynamic_cast<CParserPlugin *>(metadata->GetPlugin());
              }
            }

            switch(highestScoreParser->GetAction())
            {
            case CParserPlugin::ParseStream:
              this->activeParser = highestScoreParser;
              break;
            case CParserPlugin::GetNewConnection:
              newUrlSpecified = true;

              this->StopReceivingData();
              urlConnection->Clear();
              result = highestScoreParser->GetConnectionParameters(urlConnection);
              break;
            }
          }
        }
        
        CHECK_CONDITION_EXECUTE(FAILED(result), this->protocolHoster->StopReceivingData());
      }
      while ((newUrlSpecified) && SUCCEEDED(result));
    }
  }

  CHECK_POINTER_HRESULT(result, this->activeParser, result, E_NO_ACTIVE_PARSER);

  if (SUCCEEDED(result))
  {
    this->logger->Log(LOGGER_INFO, L"%s: %s: active parser: '%s'", MODULE_PARSER_HOSTER_NAME, METHOD_START_RECEIVING_DATA_NAME, this->activeParser->GetName());
  }

  return result;
}

HRESULT CParserHoster::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_STOP_RECEIVING_DATA_NAME);

  // stop receiving data
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->protocolHoster, this->protocolHoster->StopReceivingData());

  this->activeParser = NULL;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->hosterName, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CParserHoster::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return (this->activeParser != NULL) ? this->activeParser->QueryStreamProgress(streamProgress) : E_NOT_VALID_STATE;
}
  
HRESULT CParserHoster::ClearSession(void)
{
  // stop receiving data
  this->StopReceivingData();

  for (unsigned int i = 0; i < this->hosterPluginMetadataCollection->Count(); i++)
  {
    CParserHosterPluginMetadata *metadata = (CParserHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);
    CParserPlugin *parser = (CParserPlugin *)metadata->GetPlugin();

    this->logger->Log(LOGGER_INFO, L"%s: %s: reseting parser: %s", this->hosterName, METHOD_CLEAR_SESSION_NAME, parser->GetName());

    metadata->ClearSession();
    parser->ClearSession();
  }

  // reset all protocol implementations
  this->protocolHoster->ClearSession();

  return S_OK;
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